using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Platform;
using Axicultus.Wpf.Shell;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi;
using NetOffice.PowerPointApi.Enums;
using OfficeLib;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public partial class MainViewModelTests
{
    [Fact]
    public async Task LivePowerPointReloadKeepsLastGoodImageAndRetriesTheSavedRevision()
    {
        using var temp = TempSettingsFolder.Create();
        string path = Path.Combine(temp.Root, "edit.pptx");
        File.WriteAllText(path, "first");
        var renderer = new FixedSlideCountPowerPointRenderService(3);
        var pane = new PowerPointPreviewViewModel(renderer, _ => Frozen());
        await pane.LoadAsync(path, 2, 320, 180);
        var first = pane.PreviewImage;
        var firstVersion = pane.LoadedSourceVersion!;
        await pane.LoadThumbnailsAsync(path, 3, 320, 180);
        File.AppendAllText(path, " edit");
        var revision = SlideImageManifest.ReadSource(path);
        renderer.FailSlide = 2;
        await pane.LoadAsync(path, 2, 320, 180, expectedVersion: revision);
        Assert.Same(first, pane.PreviewImage);
        Assert.Equal(PowerPointPreviewState.Ready, pane.State);
        Assert.False(revision.Matches(pane.LoadedSourceVersion));
        Assert.False(pane.TryUseNativeThumbnail(3, revision));
        Assert.True(pane.TryUseNativeThumbnail(3, firstVersion));
        Assert.Same(pane.Thumbnails[2].Image, pane.PreviewImage);
        renderer.FailSlide = null;
        await pane.LoadAsync(path, 2, 320, 180, expectedVersion: revision);
        Assert.True(revision.Matches(pane.LoadedSourceVersion));
        var saved = pane.PreviewImage;
        File.AppendAllText(path, " newer");
        await pane.LoadAsync(path, 3, 320, 180, expectedVersion: revision);
        Assert.Same(saved, pane.PreviewImage);
        Assert.Equal(2, pane.SlideNumber);
    }

    private sealed class DesktopPowerPointTheoryAttribute : TheoryAttribute
    {
        public DesktopPowerPointTheoryAttribute()
        {
            if (Environment.GetEnvironmentVariable("AXICULTUS_TEST_POWERPOINT_DESKTOP") != "1")
                Skip = "Requires an interactive Windows desktop with PowerPoint.";
        }
    }

    [DesktopPowerPointTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task LivePowerPointReloadUpdatesNativeShowBothPanesAndThumbnails(bool editorOpenAtStart) =>
        StaHelper.RunOnStaAsync(async () =>
        {
            using var temp = TempSettingsFolder.Create();
            string path = Path.Combine(temp.Root, "live.pptx");
            string marker = Guid.NewGuid().ToString("N");
            using var app = new NetOffice.PowerPointApi.Application();
            using var documents = app.Presentations;
            Presentation? editor = documents.Add(MsoTriState.msoTrue);
            using var worker = new PowerPointWorker();
            using var render = new PowerPointRenderService(worker);
            var settings = temp.CreateSettings();
            var displays = new DisplayService(new SystemDisplayReader());
            var placement = new WindowPlacementService(settings, displays);
            using var show = new OfficePowerPointSlideShowControl(displays, placement);
            using var sut = CreateSut(settings: settings, display: displays, seedSampleQueue: false, powerPointSlideShow: show,
                powerPoint: new PowerPointPreviewViewModel(render), outputPowerPoint: new PowerPointPreviewViewModel(render));
            var preview = sut.PowerPoint;
            var output = sut.OutputPowerPoint;
            try
            {
                for (int i = 1; i <= 3; i++)
                {
                    using var slide = editor.Slides.Add(i, PpSlideLayout.ppLayoutTitleOnly);
                    slide.Shapes.Title.TextFrame.TextRange.Text = marker + i;
                    SetBackground(slide, 0xFFFFFF);
                }
                editor.SaveAs(path, PpSaveAsFileType.ppSaveAsOpenXMLPresentation);
                if (!editorOpenAtStart)
                {
                    editor.Close();
                    editor.Dispose();
                    editor = null;
                }

                var item = new LiveQueueItem("ppt", "Service", LiveItemKinds.PowerPoint) { ContentPath = path };
                var other = new LiveQueueItem("song", "Next item", LiveItemKinds.Song) { Lyrics = "standby" };
                sut.LoadQueue([item, other]);
                await UntilAsync(() => preview.Thumbnails.Count == 3, () => preview.StatusText);
                await sut.GoLiveCommand.ExecuteAsync(null);
                Assert.True(show.HasOutputShow, sut.StatusText);
                await sut.GoToSlideCommand.ExecuteAsync(2);
                using (var window = FindTestShow(app, marker))
                {
                    AssertPlacement(window, show, placement.CreateOutputPlacement(sut.SelectedOutputDisplay!, windowed: false));
                    window.View.GotoSlide(3);
                }
                await sut.SyncNativePowerPointPositionAsync();
                Assert.Equal(3, sut.Session.Current.PptSlideNumber);
                Assert.Equal(2, preview.SlideNumber);
                editor ??= documents.Open(path, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoTrue);

                SetBackground(editor.Slides[2], 0x00FF00);
                SetBackground(editor.Slides[3], 0x0000FF);
                editor.Slides[3].Shapes.Title.TextFrame.TextRange.Text = "saved edit";
                editor.Slides.Add(4, PpSlideLayout.ppLayoutBlank);
                editor.Save();
                editor.Slides[3].Shapes.Title.TextFrame.TextRange.Text = "unsaved draft";
                SetBackground(editor.Slides[3], 0xFF0000);
                await UntilAsync(() => preview.Thumbnails.Count == 4 && output.Thumbnails.Count == 4,
                    () => $"Preview {preview.StatusText}, output {output.StatusText}", sut.SyncNativePowerPointPositionAsync);
                using (var window = FindTestShow(app, marker))
                {
                    Assert.Equal(3, window.View.Slide.SlideIndex);
                    Assert.Equal("saved edit", window.View.Slide.Shapes.Title.TextFrame.TextRange.Text);
                    Assert.NotEqual(path, window.Presentation.FullName, StringComparer.OrdinalIgnoreCase);
                    AssertPlacement(window, show, placement.CreateOutputPlacement(sut.SelectedOutputDisplay!, windowed: false));
                }
                Assert.Equal("unsaved draft", editor.Slides[3].Shapes.Title.TextFrame.TextRange.Text);
                Assert.Equal(MsoTriState.msoFalse, editor.Saved);
                Assert.Equal((2, 4), (preview.SlideNumber, preview.SlideCount));
                Assert.Equal((3, 4), (output.SlideNumber, output.SlideCount));
                Assert.Equal(Colors.Lime, SampleBackground(sut.PreviewVisualSource));
                Assert.Equal(Colors.Red, SampleBackground(sut.Session.Current.CurrentItemPreviewSource));
                Assert.Equal(Colors.Red, SampleBackground(sut.OutputVisualSource));
                Assert.Same(output.PreviewImage, sut.Session.Current.CurrentItemPreviewSource);
                Assert.Equal(Colors.Red, SampleBackground(output.Thumbnails.Single(t => t.IsCurrent).Image));
                Assert.Equal(Colors.Lime, SampleBackground(preview.Thumbnails.Single(t => t.IsCurrent).Image));

                await show.SetBlackoutAsync(true);
                editor.Save();
                Assert.Null(await show.PollOutputAsync(path));
                using (var window = FindTestShow(app, marker))
                    Assert.Equal(PpSlideShowState.ppSlideShowBlackScreen, window.View.State);
                await show.SetBlackoutAsync(false);
                var sameCountEdit = SlideImageManifest.ReadSource(path);
                await UntilAsync(() => sameCountEdit.Matches(output.LoadedSourceVersion) &&
                    sameCountEdit.Matches(preview.LoadedSourceVersion) && output.Thumbnails.Count == 4 && preview.Thumbnails.Count == 4,
                    () => $"Output {output.StatusText}", sut.SyncNativePowerPointPositionAsync);
                using (var window = FindTestShow(app, marker))
                {
                    Assert.Equal(3, window.View.Slide.SlideIndex);
                    Assert.Equal("unsaved draft", window.View.Slide.Shapes.Title.TextFrame.TextRange.Text);
                }
                Assert.Equal(Colors.Blue, SampleBackground(sut.Session.Current.CurrentItemPreviewSource));
                Assert.Equal(2, preview.SlideNumber);

                sut.SelectedItem = other;
                using (var window = FindTestShow(app, marker))
                    window.View.GotoSlide(4);
                editor.Slides[4].Delete();
                editor.Slides[3].Delete();
                editor.Save();
                await UntilAsync(() => output.Thumbnails.Count == 2 && sut.Session.Current.PptSlideNumber == 2,
                    () => $"Output {output.StatusText}", sut.SyncNativePowerPointPositionAsync);
                using (var window = FindTestShow(app, marker))
                    Assert.Equal(2, window.View.Slide.SlideIndex);
                Assert.Equal(Colors.Lime, SampleBackground(sut.Session.Current.CurrentItemPreviewSource));
                Assert.Equal(other.Id, sut.PreviewPanelItem?.Id);
                Assert.Null(preview.PreviewImage);
            }
            finally
            {
                await show.StopAsync();
                if (editor != null)
                {
                    editor.Saved = MsoTriState.msoTrue;
                    editor.Close();
                    editor.Dispose();
                }
                if (documents.Count == 0) app.Quit();
            }
        });

    private static SlideShowWindow FindTestShow(NetOffice.PowerPointApi.Application app, string marker)
    {
        var windows = app.SlideShowWindows;
        for (int i = 1; i <= windows.Count; i++)
        {
            var window = windows[i];
            if (window.Presentation.Slides[1].Shapes.Title.TextFrame.TextRange.Text == marker + "1") return window;
            window.Dispose();
        }
        throw new InvalidOperationException("The test presentation has no native show.");
    }

    private static void SetBackground(Slide slide, int officeRgb)
    {
        slide.FollowMasterBackground = MsoTriState.msoFalse;
        slide.Background.Fill.Solid();
        slide.Background.Fill.ForeColor.RGB = officeRgb;
    }

    private static void AssertPlacement(SlideShowWindow window, OfficePowerPointSlideShowControl show, OutputWindowPlacement target)
    {
        var handle = OfficeLib.PowerPoint.FindSlideShowWindow(window, () => window.Presentation.Name, out string error);
        Assert.True(handle != 0 && show.OwnsOutputWindow(handle), error);
        Assert.True(Axicultus.Interop.DesktopWindows.GetWindowRect(handle, out var rect));
        Assert.Equal(((int)target.Left, (int)target.Top, (int)target.Width, (int)target.Height),
            (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top));
    }

    private static Color SampleBackground(ImageSource? source)
    {
        var image = new FormatConvertedBitmap(Assert.IsAssignableFrom<BitmapSource>(source), PixelFormats.Bgra32, null, 0);
        byte[] pixel = new byte[4];
        image.CopyPixels(new Int32Rect(5, 5, 1, 1), pixel, 4, 0);
        return Color.FromRgb(pixel[2], pixel[1], pixel[0]);
    }

    private static async Task UntilAsync(Func<bool> ready, Func<string> status, Func<Task>? tick = null)
    {
        var timer = Stopwatch.StartNew();
        while (!ready())
        {
            Assert.True(timer.Elapsed < TimeSpan.FromSeconds(50), status());
            if (tick != null) await tick();
            await Task.Delay(150);
        }
    }
}
