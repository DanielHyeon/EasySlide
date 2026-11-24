using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a037 design 12: pictures of the console and its settings window for a review on a one-screen PC. Nothing is shown on
// screen: each window's content is laid out and drawn off screen, in a temporary profile with decks made here.
// Run alone: AXI_A037_SHOTS=<folder> dotnet test --filter "FullyQualifiedName~CaptionConsoleTests.DrawTheConsoleForReview"
public sealed partial class CaptionConsoleTests
{
    private sealed class ShotsFactAttribute : FactAttribute
    {
        public ShotsFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AXI_A037_SHOTS"))) Skip = "Set AXI_A037_SHOTS to a folder for the pictures.";
        }
    }

    private static readonly (string Name, double Width, double Height)[] ShotSizes = [("default", 1040, 720), ("min", 900, 620)];
    // 1024×768 at 125% (a work area of 819×566; the window there is that big and its body scrolls).
    private static readonly (string Name, double Width, double Height) SmallScreen = ("small", 819, 566);

    [ShotsFact]
    public Task DrawTheConsoleForReview()
    {
        var live = new Axicultus.Wpf.Shell.LiveSessionService(); // its own: nothing of the main window reacts to it
        var folder = Directory.CreateDirectory(Environment.GetEnvironmentVariable("AXI_A037_SHOTS")!).FullName;
        return WithConsole(async (console, session, directory) =>
        {
            async Task Draw(Window window, string name, params (string Name, double Width, double Height)[] sizes)
            {
                foreach (var (size, width, height) in sizes.Length > 0 ? sizes : ShotSizes)
                {
                    await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    File.WriteAllBytes(Path.Combine(folder, $"{name}-{size}.png"), DrawWindow(window, width, height));
                }
            }

            async Task Dark(string name)
            {
                var themes = new Axicultus.Wpf.Theme.ThemeService();
                themes.ApplyTheme(Axicultus.Wpf.Theme.ColorTheme.Dark);
                try { await Draw(console, name + "-dark", ShotSizes[0]); }
                finally { themes.ApplyTheme(Axicultus.Wpf.Theme.ColorTheme.Light); }
            }

            console.Draft.Text = "주님은 나의 목자시니 내게 부족함 없으리";
            await Draw(console, "1-hidden-start", [.. ShotSizes, SmallScreen]);
            await Dark("1-hidden-start");
            console.AirButton.Command.Execute(null);
            await Draw(console, "2-direct-on-air");

            var text = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("text").FullName, [],
                CaptionAllTextTests.TextSlide("오늘의 말씀", "시편 23편 1-3절"),
                CaptionAllTextTests.TextSlide("여호와는 나의 목자시니", "내게 부족함이 없으리로다"),
                CaptionAllTextTests.TextSlide("그가 나를 푸른 풀밭에 누이시며", "쉴 만한 물가로 인도하시는도다"));
            Assert.True(await console.PrepareDeckAsync(text, 1));
            console.ScreenMode.IsChecked = true;
            session.Follow(2, CaptionDeck.AllTextKey);
            await Draw(console, "3-follow-text");

            var score = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("score").FullName, [CaptionScorePictureTests.ScorePng()],
                CaptionAllTextTests.TextSlide("찬양 · 주 하나님 지으신 모든 세계"), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
            console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
            Assert.True(await console.PrepareDeckAsync(score, 2));
            await console.ScorePreparation;
            await console.LyricsReading;
            session.Follow(2, CaptionDeck.AllTextKey);
            await Draw(console, "4-follow-score-auto", [.. ShotSizes, SmallScreen]);
            await Dark("4-follow-score-auto");
            console.ScoreAndText.IsChecked = true;
            await Draw(console, "5-follow-score-text");
            console.AirButton.Command.Execute(null); // hidden again
            await Draw(console, "6-follow-score-hidden");
            console.AirButton.Command.Execute(null); // on air, then the output screen goes black: nothing goes out
            live.GoLive(new LiveQueueItem("x", "song", LiveItemKinds.Song), "m");
            live.HideOutput(blackout: true);
            await Draw(console, "8-screen-off");
            await Dark("8-screen-off");

            await Draw(console.Settings, "7-settings", ("default", console.Settings.Width, console.Settings.Height), ("min", console.Settings.MinWidth, console.Settings.MinHeight));
        }, show: false, appLook: true, live: live);
    }

    // The window's content as the window would show it at that size (its frame and title bar left out), never shown.
    private static byte[] DrawWindow(Window window, double width, double height)
    {
        var content = (FrameworkElement)window.Content;
        int w = (int)width - 16, h = (int)height - 39; // a standard frame: 8 each side, the title bar 31
        for (int pass = 0; pass < 2; pass++) // the second pass after what the first layout started (fits, bindings)
        {
            content.Measure(new Size(w, h));
            content.Arrange(new Rect(0, 0, w, h));
            content.UpdateLayout();
            content.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        }
        var page = new System.Windows.Shapes.Rectangle { Fill = window.Background, Width = w, Height = h };
        page.Measure(new Size(w, h));
        page.Arrange(new Rect(0, 0, w, h));
        var bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(page);
        bitmap.Render(content);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
