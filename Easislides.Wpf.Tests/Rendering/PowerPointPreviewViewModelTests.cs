using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

/// <summary>
/// PPT 미리보기 VM 검증 (G1 / gap-analysis.md §4 G-α).
/// 렌더 서비스는 스텁으로, 이미지 디코드는 주입 스텁으로 격리(실제 COM/PNG 디코드/STA 불필요).
/// </summary>
public class PowerPointPreviewViewModelTests
{
    // 빈 DrawingImage — WIC 픽셀 없이 만들 수 있는 frozen ImageSource(테스트 더미).
    private static readonly ImageSource DummyImage = CreateDummyImage();

    [Fact]
    public async Task LoadAsync_Success_SetsReadyAndImageAndSlideCount()
    {
        var vm = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);

        await vm.LoadAsync("deck.pptx", slideNumber: 2, pixelWidth: 800, pixelHeight: 600);

        vm.State.Should().Be(PowerPointPreviewState.Ready);
        vm.PreviewImage.Should().BeSameAs(DummyImage, "성공 시 디코드된 이미지가 노출돼야 함");
        vm.SlideNumber.Should().Be(2);
        vm.SlideCount.Should().Be(3);
        vm.StatusText.Should().Contain("2/3");
    }

    [Fact]
    public async Task LoadAsync_Failure_SetsFailedAndClearsImage()
    {
        var vm = new PowerPointPreviewViewModel(new StubRenderService(success: false), _ => DummyImage);

        await vm.LoadAsync("deck.pptx", slideNumber: 1, pixelWidth: 800, pixelHeight: 600);

        vm.State.Should().Be(PowerPointPreviewState.Failed);
        vm.PreviewImage.Should().BeNull("실패 시 미리보기 이미지는 비워져야 함");
    }

    [Fact]
    public async Task LoadAsync_DecodeThrows_SetsFailed_NotStuckRendering()
    {
        // 렌더 성공이지만 디코드가 던지면(손상 바이트 등) Rendering 고착·예외 누출 없이 Failed 로 마무리해야 함.
        var vm = new PowerPointPreviewViewModel(
            new StubRenderService(success: true),
            _ => throw new NotSupportedException("bad image"));

        await vm.LoadAsync("deck.pptx", slideNumber: 1, pixelWidth: 800, pixelHeight: 600);

        vm.State.Should().Be(PowerPointPreviewState.Failed, "디코드 실패도 Failed 상태로 마무리");
        vm.PreviewImage.Should().BeNull();
        vm.SlideNumber.Should().Be(0, "실패 시 슬라이드 번호도 리셋");
    }

    [Fact]
    public void Clear_Resets_To_Idle()
    {
        var vm = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);

        vm.Clear();

        vm.State.Should().Be(PowerPointPreviewState.Idle);
        vm.PreviewImage.Should().BeNull();
        vm.SlideCount.Should().Be(0);
    }

    [Fact]
    public async Task LoadThumbnailsAsync_PopulatesStripWithCurrentHighlighted()
    {
        var vm = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);
        await vm.LoadAsync("deck.pptx", slideNumber: 2, pixelWidth: 800, pixelHeight: 600); // 현재 슬라이드 2, 총 3장

        await vm.LoadThumbnailsAsync("deck.pptx", slideCount: 3, thumbnailWidth: 200, thumbnailHeight: 112);

        vm.Thumbnails.Should().HaveCount(3);
        vm.Thumbnails.Select(t => t.SlideNumber).Should().Equal(1, 2, 3);
        vm.Thumbnails.Single(t => t.SlideNumber == 2).IsCurrent.Should().BeTrue("현재 슬라이드(2) 강조");
        vm.Thumbnails.Where(t => t.SlideNumber != 2).Should().OnlyContain(t => !t.IsCurrent);
    }

    [Fact]
    public async Task LoadThumbnailsAsync_ClampsSmallRequestsToFrmMainHighResolution()
    {
        var render = new StubRenderService(success: true);
        var vm = new PowerPointPreviewViewModel(render, _ => DummyImage);
        await vm.LoadAsync("deck.pptx", slideNumber: 1, pixelWidth: 800, pixelHeight: 600);
        render.Requests.Clear();

        await vm.LoadThumbnailsAsync("deck.pptx", slideCount: 3, thumbnailWidth: 200, thumbnailHeight: 112);

        render.Requests.Should().HaveCount(3);
        render.Requests.Should().OnlyContain(r => r.PixelWidth == 640 && r.PixelHeight == 480,
            "FrmMain exports PPT preview thumbnails as 640x480 before drawing them into the thumbnail surface");
    }

    [Fact]
    public async Task SlideNumberChange_UpdatesThumbnailHighlight()
    {
        var vm = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);
        await vm.LoadAsync("deck.pptx", 1, 800, 600);
        await vm.LoadThumbnailsAsync("deck.pptx", 3, 200, 112);
        vm.Thumbnails.Single(t => t.SlideNumber == 1).IsCurrent.Should().BeTrue();

        await vm.LoadAsync("deck.pptx", 3, 800, 600); // 슬라이드 3 으로 이동

        vm.Thumbnails.Single(t => t.SlideNumber == 3).IsCurrent.Should().BeTrue("이동 후 강조도 이동");
        vm.Thumbnails.Single(t => t.SlideNumber == 1).IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task Clear_AlsoClearsThumbnails()
    {
        var vm = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);
        await vm.LoadAsync("deck.pptx", 1, 800, 600);
        await vm.LoadThumbnailsAsync("deck.pptx", 3, 200, 112);
        vm.Thumbnails.Should().NotBeEmpty();

        vm.Clear();

        vm.Thumbnails.Should().BeEmpty("덱 미리보기를 비우면 썸네일 스트립도 비운다");
    }

    [Fact]
    public async Task LoadThumbnailsAsync_StoresRenderedPixelSizeForQualityReloads()
    {
        var render = new StubRenderService(success: true);
        var vm = new PowerPointPreviewViewModel(render, _ => DummyImage);

        await vm.LoadThumbnailsAsync("deck.pptx", 3, 200, 112);

        render.Requests.Should().OnlyContain(request =>
            request.PixelWidth == 640 &&
            request.PixelHeight == 480);
        vm.Thumbnails.Should().OnlyContain(thumbnail =>
            thumbnail.PixelWidth == 640 &&
            thumbnail.PixelHeight == 480);
    }

    [Fact]
    public async Task CopyFrom_ClonesCurrentSlideAndThumbnailState()
    {
        var source = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);
        var target = new PowerPointPreviewViewModel(new StubRenderService(success: true), _ => DummyImage);
        await source.LoadAsync("deck.pptx", 2, 800, 600);
        await source.LoadThumbnailsAsync("deck.pptx", 3, 200, 112);

        target.CopyFrom(source);

        target.State.Should().Be(PowerPointPreviewState.Ready);
        target.PreviewImage.Should().BeSameAs(source.PreviewImage);
        target.LoadedContentPath.Should().Be("deck.pptx");
        target.SlideNumber.Should().Be(2);
        target.SlideCount.Should().Be(3);
        target.Thumbnails.Should().HaveCount(3);
        target.Thumbnails.Should().NotContain(source.Thumbnails[0], "Output thumbnails must not share the mutable collection items with Preview");
        target.Thumbnails.Single(t => t.SlideNumber == 2).IsCurrent.Should().BeTrue();
        target.Thumbnails.Should().OnlyContain(t => t.PixelWidth == 640 && t.PixelHeight == 480);

        source.Clear();

        target.State.Should().Be(PowerPointPreviewState.Ready, "clearing Preview after publish must not clear Output");
        target.Thumbnails.Should().HaveCount(3);
    }

    [Fact]
    public async Task RepeatedStripRequestsShareWorkAndOnlyRetryMissingOrLowResolutionSlides()
    {
        var render = new StubRenderService(success: true);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        render.BeforeRender = request => request.SlideNumber == 2 ? gate.Task : Task.CompletedTask;
        var vm = new PowerPointPreviewViewModel(render, _ => DummyImage);
        var first = vm.LoadThumbnailsAsync("deck.pptx", 3, 200, 112);
        var repeated = vm.LoadThumbnailsAsync("DECK.pptx", 3, 640, 480);
        Assert.Same(first, repeated);
        Assert.Single(vm.Thumbnails);
        gate.SetResult();
        await first;
        await vm.LoadThumbnailsAsync("deck.pptx", 3, 640, 480);
        Assert.Equal(new[] { 1, 2, 3 }, render.Requests.Select(r => r.SlideNumber));

        vm.Thumbnails.RemoveAt(0);
        vm.Thumbnails[1] = new(3, DummyImage, 640, 479);
        await vm.LoadThumbnailsAsync("deck.pptx", 3, 640, 480);
        Assert.Equal(new[] { 1, 2, 3, 1, 3 }, render.Requests.Select(r => r.SlideNumber));
        Assert.Equal(new[] { 1, 2, 3 }, vm.Thumbnails.Select(t => t.SlideNumber));
    }

    [Fact]
    public async Task ReplacingDeckOrChangingSizeSupersedesOldLoadAndClearRejectsLateResults()
    {
        var path = Path.GetTempFileName();
        try
        {
            var render = new StubRenderService(success: true);
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            render.BeforeRender = _ => gate.Task;
            var vm = new PowerPointPreviewViewModel(render, _ => DummyImage);
            var obsolete = vm.LoadThumbnailsAsync(path, 2, 640, 480);
            render.BeforeRender = null;
            File.AppendAllText(path, "updated");
            await vm.LoadThumbnailsAsync(path, 2, 640, 480);
            gate.SetResult();
            await obsolete;
            Assert.Equal(2, vm.Thumbnails.Count);
            await vm.LoadThumbnailsAsync(path, 2, 800, 600);
            Assert.All(vm.Thumbnails, t => Assert.Equal(800, t.PixelWidth));
            File.AppendAllText(path, "another version");
            await vm.LoadThumbnailsAsync(path, 2, 800, 600);
            Assert.Equal(7, render.Requests.Count);

            gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
            render.BeforeRender = _ => gate.Task;
            var pending = vm.LoadThumbnailsAsync(path, 1, 800, 600);
            vm.Clear();
            gate.SetResult();
            await pending;
            Assert.Empty(vm.Thumbnails);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task OlderPreviewCompletionCannotSwitchBackTheDeckAndRestartItsThumbnails()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var render = new StubRenderService(success: true) { BeforeRender = r =>
            r.FilePath == "old.pptx" ? gate.Task : Task.CompletedTask };
        var vm = new PowerPointPreviewViewModel(render, _ => DummyImage);
        var old = vm.LoadAsync("old.pptx", 1, 800, 600);
        await vm.LoadAsync("new.pptx", 2, 800, 600);
        gate.SetResult();
        await old;
        Assert.Equal("new.pptx", vm.LoadedContentPath);
        Assert.Equal(2, vm.SlideNumber);
    }

    [Fact]
    public async Task CopyFromDiscardsObsoleteOutputWorkAndResumesPartialStripIndependently()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var previewRender = new StubRenderService(success: true) { BeforeRender = r =>
            r.SlideNumber == 2 ? gate.Task : Task.CompletedTask };
        var outputRender = new StubRenderService(success: true) { BeforeRender = _ => gate.Task };
        var preview = new PowerPointPreviewViewModel(previewRender, _ => DummyImage);
        var output = new PowerPointPreviewViewModel(outputRender, _ => DummyImage);
        var pendingPreview = preview.LoadThumbnailsAsync("new.pptx", 3, 640, 480);
        var obsolete = output.LoadThumbnailsAsync("old.pptx", 3, 640, 480);
        output.CopyFrom(preview);
        var pendingOutput = output.LoadThumbnailsAsync("new.pptx", 3, 640, 480);
        output.CopyFrom(preview);
        Assert.Same(pendingOutput, output.LoadThumbnailsAsync("new.pptx", 3, 640, 480));
        Assert.NotSame(preview.Thumbnails[0], output.Thumbnails[0]);
        preview.Clear();
        gate.SetResult();
        await Task.WhenAll(pendingPreview, pendingOutput, obsolete);
        Assert.Empty(preview.Thumbnails);
        Assert.Equal(new[] { 1, 2, 3 }, output.Thumbnails.Select(t => t.SlideNumber));
        Assert.Equal(new[] { 2, 3 }, outputRender.Requests.Where(r => r.FilePath == "new.pptx").Select(r => r.SlideNumber));
    }

    private static ImageSource CreateDummyImage()
    {
        var image = new DrawingImage();
        image.Freeze();
        return image;
    }

    private sealed class StubRenderService : IPowerPointRenderService
    {
        private readonly bool _success;

        public StubRenderService(bool success) => _success = success;

        public List<PowerPointRenderRequest> Requests { get; } = new();

        public Func<PowerPointRenderRequest, Task>? BeforeRender { get; set; }

        public async Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (BeforeRender != null)
                await BeforeRender(request);
            if (!_success)
            {
                return new PowerPointRenderResult(
                    PowerPointRenderErrorKind.MissingOffice, Slide: null, ErrorMessage: "no office", FromCache: false, Elapsed: TimeSpan.Zero);
            }

            var slide = new PowerPointSlideSnapshot(
                request.FilePath,
                request.SlideNumber,
                SlideCount: 3,
                request.PixelWidth,
                request.PixelHeight,
                ImageBytes: new byte[] { 1, 2, 3 },
                ContentType: "image/png",
                GeneratedAtUtc: DateTimeOffset.UnixEpoch);

            return new PowerPointRenderResult(
                PowerPointRenderErrorKind.None, slide, ErrorMessage: null, FromCache: false, Elapsed: TimeSpan.Zero);
        }

        public void ClearCache()
        {
        }
    }
}
