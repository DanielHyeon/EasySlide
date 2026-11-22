using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Util;
using OfficeLib;
using Xunit;

// The program has one UI thread and shares GDI+ objects on it (the ModernIcons bitmap cache, "UI thread only").
// Test classes that build forms on their own STA threads at the same time put one cached bitmap on two threads:
// "Object is currently in use elsewhere". So the classes of this assembly run one after another.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Axicultus.Tests;

public class MonitorPresentationTests
{
    private const string PrimaryId = @"\\.\DISPLAY1";
    private const string SecondaryId = @"\\.\DISPLAY2";
    private const string SavedId = @"\\.\DISPLAY3";
    private static readonly MonitorDevice[] Connected =
    [
        new(SecondaryId, new Rectangle(-2560, 0, 2560, 1440), false),
        new(PrimaryId, new Rectangle(0, 0, 3440, 1440), true)
    ];

    [Fact]
    public void OptionsMonitorListsShowTheConnectedWindowsDisplays()
    {
        RunOnSta(() =>
        {
            using var options = new FrmOptions();
            options.BuildMonitorsList();
            var outputStrip = Assert.IsType<ToolStrip>(Assert.Single(options.Controls.Find("toolStripMonitorList", true)));
            var lyricsStrip = Assert.IsType<ToolStrip>(Assert.Single(options.Controls.Find("toolStrip12", true)));
            var output = Assert.IsType<ToolStripComboBox>(outputStrip.Items["DualMonitorList"]);
            var lyrics = Assert.IsType<ToolStripComboBox>(lyricsStrip.Items["LyricsMonitorList"]);
            foreach (var screen in Screen.AllScreens.Where(s => s.Bounds.Width > 0 && s.Bounds.Height > 0))
            {
                string expected = $"{(screen.Primary ? "주 모니터" : "출력 모니터")} {screen.DeviceName} ({screen.Bounds.Width}x{screen.Bounds.Height})";
                Assert.True(output.FindStringExact(expected) >= 0);
                Assert.Equal(!screen.Primary, lyrics.FindStringExact(expected) >= 0);
            }
        });
    }

    [Fact]
    public void OutputPickerKeepsDisconnectedPreferenceAndRecognizesItAfterReconnect()
    {
        RunOnSta(() =>
        {
            using var item = new ToolStripComboBox();
            var picker = item.ComboBox;
            MonitorPicker.Populate(picker, Connected, MonitorPurpose.Output, SavedId);
            Assert.Equal(new[] { "자동 선택", $"주 모니터 {PrimaryId} (3440x1440)",
                $"출력 모니터 {SecondaryId} (2560x1440)", "사용 안 함", $"{SavedId} (연결 해제)" },
                picker.Items.Cast<object>().Select(choice => choice.ToString()));
            Assert.Equal(SavedId, MonitorPicker.SelectedId(picker));

            DirectoryInfo temp = Directory.CreateTempSubdirectory("axicultuss-monitor-choice-");
            try
            {
                string path = Path.Combine(temp.FullName, "settings.json");
                var store = new JsonProfileStore(path, () => new());
                store.WriteValue("options", "OutputmonitorName", MonitorPicker.SelectedId(picker), Microsoft.Win32.RegistryValueKind.String);
                var reopened = new JsonProfileStore(path, () => new());
                string saved = Assert.IsType<string>(reopened.ReadValue("options", "OutputmonitorName"));
                Assert.Equal(SavedId, saved);
                MonitorPicker.Populate(picker, [.. Connected, new(SavedId, new Rectangle(3440, 0, 1920, 1080), false)],
                    MonitorPurpose.Output, saved);
                Assert.Equal($"출력 모니터 {SavedId} (1920x1080)", picker.SelectedItem.ToString());
                Assert.Equal(1, picker.Items.Cast<object>().Count(choice => choice.ToString().Contains(SavedId)));
                picker.SelectedIndex = 0;
                Assert.Equal(MonitorLayout.Auto, MonitorPicker.SelectedId(picker));
                picker.SelectedIndex = picker.Items.Count - 1;
                Assert.Equal(MonitorLayout.None, MonitorPicker.SelectedId(picker));
            }
            finally { temp.Delete(recursive: true); }
        });
    }

    [Fact]
    public void LyricsPickerDistinguishesAnIneligiblePrimaryFromADisconnectedMonitor()
    {
        RunOnSta(() =>
        {
            using var item = new ToolStripComboBox();
            var picker = item.ComboBox;
            MonitorPicker.Populate(picker, Connected, MonitorPurpose.Lyrics, PrimaryId);
            Assert.Equal($"주 모니터 {PrimaryId} (3440x1440) (가사 모니터로 사용 불가)", picker.SelectedItem.ToString());
            Assert.Equal(PrimaryId, MonitorPicker.SelectedId(picker));
            MonitorPicker.Populate(picker, Connected, MonitorPurpose.Lyrics, SavedId);
            Assert.Equal($"{SavedId} (연결 해제)", picker.SelectedItem.ToString());
            Assert.DoesNotContain(picker.Items.Cast<object>(), choice => choice.ToString().Contains(PrimaryId));
        });
    }

    [Fact]
    public void MediaPickerUsesDeviceIdsAndKeepsTheDefaultOutputChoice()
    {
        RunOnSta(() =>
        {
            using var picker = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            MonitorPicker.Populate(picker, Connected, MonitorPurpose.Media, SecondaryId.ToLowerInvariant());
            Assert.Equal($"출력 모니터 {SecondaryId} (2560x1440)", picker.SelectedItem.ToString());
            Assert.Equal(SecondaryId, MonitorPicker.SelectedId(picker));
            MonitorPicker.Populate(picker, Connected, MonitorPurpose.Media, SavedId);
            Assert.Equal($"{SavedId} (연결 해제)", picker.SelectedItem.ToString());
            Assert.Equal(SavedId, MonitorPicker.SelectedId(picker));
            picker.SelectedIndex = 0;
            Assert.Equal("", MonitorPicker.SelectedId(picker));
        });
    }

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
        DirectoryInfo directory = Directory.CreateTempSubdirectory("axicultus-monitor-test-");
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
            Assert.Equal("AxicultusSlideImages/2", before[0]);
            before[0] = "EasiSlidesSlideImages/2";
            File.WriteAllLines(record, before);
            Assert.True(SlideImageManifest.TryMatch(prefix, presentation, out _));
            before[0] = "AxicultusSlideImages/1";
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
        DirectoryInfo directory = Directory.CreateTempSubdirectory("axicultus-monitor-test-");
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

    internal static void RunOnSta(Action action, int timeoutSeconds = 15)
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
        Assert.True(thread.Join(TimeSpan.FromSeconds(timeoutSeconds)));
        error?.Throw();
    }
}
