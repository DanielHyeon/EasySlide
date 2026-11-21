using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Rendering;

[Collection("WPF Application")]
public class TextFileProjectionTests
{
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
