using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Easislides.Util;
using OfficeLib;
using Xunit;

namespace Easislides.Tests;

public class MonitorPresentationTests
{
    [Theory]
    [InlineData(960, 540, 640, 360)]
    [InlineData(720, 540, 640, 480)]
    [InlineData(540, 960, 360, 640)]
    [InlineData(0, 0, 640, 480)]
    public void PowerPointExportFollowsTheSlideAspectWithoutUsingOffice(double width, double height, int expectedWidth, int expectedHeight)
    {
        Assert.Equal(new Size(expectedWidth, expectedHeight), PowerPoint.GetSlideExportSize(width, height));
    }

    [Fact]
    public void LegacyFourByThreeCachesAreRebuilt()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("easislides-monitor-test-");
        try
        {
            string presentation = Path.Combine(directory.FullName, "slides.pptx");
            string prefix = Path.Combine(directory.FullName, "preview-");
            File.WriteAllText(presentation, "Source identity only. Office is not used.");
            File.WriteAllText(SlideImageManifest.ImagePath(prefix, 1), "Image identity only.");
            SlideImageManifest.Commit(prefix, SlideImageManifest.ReadSource(presentation), 1);
            Assert.True(SlideImageManifest.TryMatch(prefix, presentation, out _));

            string record = Path.Combine(directory.FullName, "preview-build.txt");
            string[] before = File.ReadAllLines(record);
            before[0] = "EasiSlidesSlideImages/1";
            File.WriteAllLines(record, before);
            Assert.False(SlideImageManifest.TryMatch(prefix, presentation, out _));

            var info = new FileInfo(presentation);
            string legacyName = $"{ExternalPreviewCache.KeyOf(info.FullName)}_{info.LastWriteTimeUtc.Ticks:x}_{info.Length:x}.jpg";
            Assert.NotEqual(legacyName, Path.GetFileName(ExternalPreviewCache.GetPreviewPath(directory.FullName, presentation)));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData(320, 180, 135, 180)]
    [InlineData(320, 240, 180, 240)]
    public void PowerPointThumbnailUsesDecodedAspectOnLoadAndResize(int sourceWidth, int sourceHeight, int initialHeight, int resizedHeight)
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("easislides-monitor-test-");
        try
        {
            string file = Path.Combine(directory.FullName, "slide.jpg");
            using (var image = new Bitmap(sourceWidth, sourceHeight))
                image.Save(file, ImageFormat.Jpeg);

            RunOnSta(() =>
            {
                using var canvas = new ImageCanvas { PowerPoint = true };
                using var tooltip = new ToolTip();
                string[] images = [file];

                canvas.BuildNewImageThumbs(0, 240, 180, ref images, 1, "", 1, Color.White, tooltip, false);
                Assert.True(canvas.CalculateImage());
                Assert.Equal(new Size(240, initialHeight), canvas.Size);

                canvas.BuildNewImageThumbs(0, 320, 240, ref images, 1, "", 1, Color.White, tooltip, false);
                Assert.Equal(new Size(320, resizedHeight), canvas.Size);
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void LyricsWindowUsesResolvedBoundsWithoutOpeningTheApplication()
    {
        RunOnSta(() =>
        {
            Rectangle bounds = new(-1700, 120, 960, 540);
            var store = new RegistrySettingsTests.MemoryStore();
            var settings = new RegistrySettings(store, _ => { });
            using var window = new FrmLyricsScreen(new MonitorPlacement("STAGE", bounds, true), settings);
            window.PrepareLayout();
            Assert.Equal(FormStartPosition.Manual, window.StartPosition);
            Assert.Equal(bounds, window.Bounds);
            Assert.False(window.Visible);
            Assert.Empty(store.Values);
        });
    }

    internal static void RunOnSta(Action action)
    {
        ExceptionDispatchInfo error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ExceptionDispatchInfo.Capture(ex);
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)));
        error?.Throw();
    }
}
