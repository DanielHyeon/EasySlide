using System;
using System.IO;
using System.Globalization;
using System.Windows.Media;
using Axicultus.Wpf.Rendering;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Rendering;

[Collection("WPF Application")]
public class TextFileProjectionTests
{
    [Fact]
    public void KoreanWordsMoveAsAUnitAtTheActualWpfLineBreak()
    {
        StaHelper.RunOnSta(() =>
        {
            var face = new Typeface("Malgun Gothic");
            FormattedText Layout(string text) => new(text, CultureInfo.GetCultureInfo("ko-KR"),
                FlowDirection.LeftToRight, face, 26, Brushes.Black, 1);
            string raw = "하나 둘셋넷";
            string display = LyricWordWrapping.ForDisplay(raw);
            var text = Layout(display);
            text.MaxTextWidth = Layout("하나 둘").Width + 1;
            double Y(char letter) => text.BuildHighlightGeometry(new Point(), display.IndexOf(letter), 1)!.Bounds.Y;
            Y('둘').Should().BeGreaterThan(Y('나'));
            Y('둘').Should().Be(Y('넷'));
            display.Replace("\u2060", "").Should().Be(raw);
            string multiline = "기도합니다.\n아멘";
            LyricWordWrapping.ForDisplay(multiline).Should().Contain(".\n아");
            LyricWordWrapping.ForDisplay("각나다").Should().StartWith("각\u2060");
        });
    }

    [Fact]
    public async Task MigratingWorshipListKeepsPreviewAndAudiencePixels()
    {
        const string header = "43=Arial>47=60>29=-256>26=-16776961>31=2>45=10>63=1>64=6>";
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "Sunday.json");
            await File.WriteAllTextAsync(path, $$"""
                {"Format":"11=-16777216>","Items":[{"Id":"esw:T:sermon.txt","Title":"설교말씀.txt","Kind":"Notice",
                 "Lyrics":"설교 말씀","ContentPath":"sermon.txt","FormatData":"{{header}}","UseIndividualFormatting":true}]}
                """);
            var store = new WorshipListStore(directory.FullName);
            var original = await store.LoadSessionAsync("Sunday");
            await store.SaveSessionAsync("Sunday", original.Items, original.Format);
            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            var format = json.RootElement.GetProperty("Items")[0].GetProperty("FormatData");
            format.GetProperty("region1FontFamily").GetString().Should().Be("Arial");
            format.GetProperty("region1FontSize").GetInt32().Should().Be(60);
            format.GetProperty("region1TextColorArgb").GetString().Should().Be("#FFFFFF00");
            format.GetProperty("backgroundStartColorArgb").GetString().Should().Be("#FF0000FF");
            json.RootElement.GetProperty("Format").GetProperty("panelBackgroundColorArgb").GetString().Should().Be("#FF000000");
            var migrated = await store.LoadSessionAsync("Sunday");
            migrated.Format.Should().Be(original.Format);
            var before = original.Items.Single();
            var after = migrated.Items.Single();
            before.FormatData.Should().Be(header);
            after.FormatData.Should().Be(before.FormatData);
            StaHelper.RunOnSta(() =>
            {
                foreach (bool preview in new[] { true, false })
                {
                    var reference = RenderTextPage(before, preview);
                    var result = RenderTextPage(after, preview);
                    Assert.Equal(reference, result);
                    // The yellow text must actually be on the page, so two blank rasters cannot pass.
                    Assert.Contains(Enumerable.Range(0, result.Length / 4),
                        i => result[4 * i] < 50 && result[4 * i + 1] > 200 && result[4 * i + 2] > 200);
                }
            });
        }
        finally { directory.Delete(true); }
    }

    private static byte[] RenderTextPage(LiveQueueItem item, bool preview)
    {
        int width = preview ? 480 : 960, height = preview ? 270 : 540;
        var session = new LiveSessionService();
        session.GoLive(item, "projector");
        var snapshot = preview ? LiveSessionService.CreateSnapshot(LiveSessionSnapshot.Off,
            item, string.Empty, LiveState.Active, false, false) : session.Current;
        using var view = new OutputWindowViewModel();
        view.ApplyOutput(new(true, null, new(0, 0, width, height, false)));
        view.ApplySession(snapshot);
        var window = new OutputWindow();
        try
        {
            var canvas = (Grid)window.Content;
            window.Content = null;
            canvas.DataContext = view;
            var image = VisualRenderHarness.RenderToBitmap(canvas, width, height);
            var pixels = new byte[width * height * 4];
            image.CopyPixels(pixels, width * 4, 0);
            return pixels;
        }
        finally { window.Close(); }
    }

    [Fact]
    public void IndividualTextBackgroundRendersBothWinFormsColoursAndPattern()
    {
        StaHelper.RunOnSta(() =>
        {
            var item = new LiveQueueItem("text-file:order", "순서.txt", LiveItemKinds.Notice) {
                Lyrics = "기도", FormatData = "26=-65536>27=-16776961>28=2>" };
            var session = new LiveSessionService();
            session.GoLive(item, "projector");
            using var view = new OutputWindowViewModel();
            view.ApplySession(session.Current);
            var bitmap = (System.Windows.Media.Imaging.BitmapSource)((ImageBrush)view.SceneBackgroundBrush).ImageSource;
            var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
            Assert.Contains(Enumerable.Range(0, pixels.Length / 4), i => pixels[4*i] > pixels[4*i + 2]);
            Assert.Contains(Enumerable.Range(0, pixels.Length / 4), i => pixels[4*i] < pixels[4*i + 2]);
        });
    }

    [Theory]
    [InlineData(960, 540, 1, 67, 54, 324)]
    [InlineData(1920, 1080, 1.5, 89, 72, 432)]
    public void BilingualBiblePositionsBothRegionsRelativeToTheSlide(int width, int height, double scale, int font, int primaryTop, int secondaryTop)
    {
        StaHelper.RunOnSta(() =>
        {
            var item = new LiveQueueItem("bible:qa", "말씀", LiveItemKinds.Bible)
            {
                Lyrics = "태초에 하나님이\n[region 2]\nIn the beginning",
                SongLayout = new(50, "Malgun Gothic", 0, 0, 0, 0, 1, 100, 0),
                FormatData = "21=0>25=2>45=10>46=60>47=50>48=42>"
            };
            var session = new LiveSessionService();
            session.GoLive(item, "projector");
            using var view = new OutputWindowViewModel();
            var monitor = new OutputDisplay("projector", "Projector", 0, 0, width, height, scale);
            view.ApplyOutput(new(true, monitor, new(0, 0, width, height, false)));
            view.ApplySession(session.Current);
            view.PrimaryParagraph!.Text.Should().Be("태초에 하나님이");
            view.SecondaryParagraph!.Text.Should().Be("In the beginning");
            view.BodyFontSize.Should().Be(font);
            view.BodyContentMargin.Top.Should().Be(primaryTop);
            (view.BodyContentMargin.Top + view.PrimaryRegionHeight + view.BodyText2Margin.Top).Should().Be(secondaryTop);
        });
    }

    [Theory]
    [InlineData(960, 720, 1, 80, 57, 87)]
    [InlineData(1920, 1080, 1, 160, 115, 131)]
    [InlineData(1920, 1080, 1.5, 107, 76, 87)]
    public void SavedTextFileUsesSlideRelativeSizeAndInsets(int width, int height, double dpi, int font, int left, int top)
    {
        StaHelper.RunOnSta(() =>
        {
            // Format used by the Apostles' Creed in the legacy Sunday worship list.
            var session = new LiveSessionService();
            session.GoLive(new LiveQueueItem("T1", "사도신경.txt") {
                Lyrics = "첫 문단\n\n둘째 문단", LyricsPageIndex = 1,
                FormatData = "45=10>47=60>63=0>64=6>65=2>66=0>"
            }, "projector");
            using var view = new OutputWindowViewModel();
            var monitor = new OutputDisplay("projector", "Projector", 0, 0, width, height, dpi);
            view.ApplyOutput(new(true, monitor, new(0, 0, width, height, false)));
            view.ApplySession(session.Current);
            view.PrimaryParagraph!.Text.Should().Be("둘째 문단");
            view.BodyFontSize.Should().Be(font);
            view.BodyContentMargin.Left.Should().Be(left);
            view.BodyContentMargin.Top.Should().Be(top);
            view.BodyVerticalAlignment.Should().Be(VerticalAlignment.Top);
            view.BodyVerticalOffset.Should().Be(0);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RenderedTextHonorsAlignmentAndFitsCompleteParagraph(int alignment)
    {
        StaHelper.RunOnSta(() =>
        {
            const int width = 960, height = 540;
            var session = new LiveSessionService();
            var item = new LiveQueueItem("text-file:1", "예배.txt", LiveItemKinds.Notice) {
                Lyrics = "찬양", FormatData = $"29=-1>45=10>47=60>63={alignment}>64=6>"
            };
            using var view = new OutputWindowViewModel();
            view.ApplyOutput(new(true, null, new(0, 0, width, height, false)));
            Window window = new OutputWindow();
            Grid canvas = new() { DataContext = view };
            try
            {
                var area = (Grid)window.FindName("ContentArea");
                var body = area.Children.OfType<ContentControl>().Single();
                area.Children.Remove(body);
                canvas.Children.Add(body);
            }
            finally
            {
                window.Close();
            }

            (int First, int Last, int Bands) Render(string lyrics)
            {
                session.GoLive(item with { Lyrics = lyrics }, "projector");
                view.ApplySession(session.Current);
                var image = VisualRenderHarness.RenderToBitmap(canvas, width, height);
                var pixels = new byte[width * height * 4];
                image.CopyPixels(pixels, width * 4, 0);
                int first = height, last = -1, bands = 0;
                for (int y = 0; y < height; y++)
                {
                    bool ink = false;
                    for (int x = 0; x < width && !ink; x++)
                        ink = pixels[(y * width + x) * 4 + 3] > 128;
                    if (!ink) continue;
                    if (last < 0 || y > last + 1) bands++;
                    first = Math.Min(first, y);
                    last = y;
                }
                return (first, last, bands);
            }

            var single = Render("H");
            single.First.Should().BeInRange(alignment switch { 0 => 65, 1 => 210, _ => 420 },
                alignment switch { 0 => 125, 1 => 325, _ => 500 });
            (single.Last - single.First).Should().BeGreaterThan(40);
            var longPage = Render(string.Join("\n", Enumerable.Repeat("H", 14)));
            longPage.Bands.Should().Be(14);
            longPage.First.Should().BeGreaterThanOrEqualTo(65);
            longPage.Last.Should().BeLessThan(height - 16);
        });
    }
}
