using System;
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

        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            if (!_success)
            {
                return Task.FromResult(new PowerPointRenderResult(
                    PowerPointRenderErrorKind.MissingOffice, Slide: null, ErrorMessage: "no office", FromCache: false, Elapsed: TimeSpan.Zero));
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

            return Task.FromResult(new PowerPointRenderResult(
                PowerPointRenderErrorKind.None, slide, ErrorMessage: null, FromCache: false, Elapsed: TimeSpan.Zero));
        }

        public void ClearCache()
        {
        }
    }
}
