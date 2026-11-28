using System;
using System.IO;
using System.Linq;
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

    // a045: the tab with its strip at the size of the main window's output column (the window sizes below include the
    // frame DrawWindow leaves out): the main window at its default 1180×760, and at its least 780×540 (the column scrolls).
    private static readonly (string Name, double Width, double Height)[] ShotSizes = [("default", 450, 680), ("min", 316, 460)];
    // (the narrowest column the tab keeps: wider than the main window's least output column, which then scrolls sideways)
    private static readonly (string Name, double Width, double Height) SmallScreen = ("small", 316, 400);
    // The main window maximized on a 1080p screen (where the operator keeps it during a service).
    private static readonly (string Name, double Width, double Height) Maximized = ("max", 736, 1000);

    [ShotsFact]
    public async Task DrawTheConsoleForReview()
    {
        var live = new Axicultus.Wpf.Shell.LiveSessionService(); // its own: nothing of the main window reacts to it
        var folder = Directory.CreateDirectory(Environment.GetEnvironmentVariable("AXI_A037_SHOTS")!).FullName;
        await DrawTheDefaultLook(folder); // a046
        await WithConsole(async (console, session, directory) =>
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
                try { await Draw(Window.GetWindow(console), name + "-dark", ShotSizes[0]); }
                finally { themes.ApplyTheme(Axicultus.Wpf.Theme.ColorTheme.Light); }
            }

            console.Strip.CaptionTab.IsChecked = true; // (as the main window shows it)
            console.Draft.Text = "주님은 나의 목자시니 내게 부족함 없으리";
            await Draw(Window.GetWindow(console), "1-hidden-start", [.. ShotSizes, SmallScreen]);
            await Dark("1-hidden-start");
            console.AirButton.Command.Execute(null);
            await Draw(Window.GetWindow(console), "2-direct-on-air", [.. ShotSizes, Maximized]);

            var text = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("text").FullName, [],
                CaptionAllTextTests.TextSlide("오늘의 말씀", "시편 23편 1-3절"),
                CaptionAllTextTests.TextSlide("여호와는 나의 목자시니", "내게 부족함이 없으리로다"),
                CaptionAllTextTests.TextSlide("그가 나를 푸른 풀밭에 누이시며", "쉴 만한 물가로 인도하시는도다"));
            Assert.True(await console.PrepareDeckAsync(text, 1));
            console.ScreenMode.IsChecked = true;
            session.Follow(2, CaptionDeck.AllTextKey);
            await Draw(Window.GetWindow(console), "3-follow-text");

            var score = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("score").FullName, [CaptionScorePictureTests.ScorePng()],
                CaptionAllTextTests.TextSlide("찬양 · 주 하나님 지으신 모든 세계"), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
            console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
            Assert.True(await console.PrepareDeckAsync(score, 2));
            await console.ScorePreparation;
            await console.LyricsReading;
            session.Follow(2, CaptionDeck.AllTextKey);
            await Draw(Window.GetWindow(console), "4-follow-score-auto", [.. ShotSizes, SmallScreen, Maximized]);
            await Dark("4-follow-score-auto");
            console.ScoreAndText.IsChecked = true;
            await Draw(Window.GetWindow(console), "5-follow-score-text");
            console.AirButton.Command.Execute(null); // hidden again
            await Draw(Window.GetWindow(console), "6-follow-score-hidden");
            console.AirButton.Command.Execute(null); // on air, then the output screen goes black: nothing goes out
            live.GoLive(new LiveQueueItem("x", "song", LiveItemKinds.Song), "m");
            live.HideOutput(blackout: true);
            await Draw(Window.GetWindow(console), "8-screen-off");
            await Dark("8-screen-off");

            await Draw(console.Settings, "7-settings", ("default", console.Settings.Width, console.Settings.Height), ("min", console.Settings.MinWidth, console.Settings.MinHeight));

            // a045: a song (no deck) in the output panel shows its pages in the strip; an empty panel keeps the strip's place.
            var main = Main(console);
            var verses = new LiveQueueItem("song:shot-verses", "세 절 찬양", LiveItemKinds.Song)
                { Lyrics = "[1]\n주님은 나의 목자시니\n[2]\n푸른 풀밭에 누이시며\n[3]\n내 영혼을 소생시키시고" };
            // (the console is not shown here, so it does not follow the panel by itself: the panel's text is given to it)
            live.Stop();
            if (!session.Visible) console.AirButton.Command.Execute(null);
            session.Mirror(true);
            main.Queue.Add(verses);
            main.SelectedItem = verses;
            await main.SendWorshipSelectionToOutputCommand.ExecuteAsync(null);
            session.ShowScreenText(main.OutputLyricsText);
            await Pump(console, 300);
            await Draw(Window.GetWindow(console), "9-follow-song-pages", ShotSizes[0], Maximized);
            main.OutputItem = null;
            session.ShowScreenText("");
            await Pump(console, 300);
            await Draw(Window.GetWindow(console), "10-follow-no-steps", ShotSizes[0]);
        }, show: false, appLook: true, live: live);
    }

    // a046 design 4b: the tab and its settings window of a host without the app's theme (Axicultuss), in the caption
    // library's own look, light and dark, a song's page on air. The application's resources (the app's theme and control
    // styles, there when another test of the run made an Application) are set aside meanwhile: run alone, nothing else
    // uses them.
    private static async Task DrawTheDefaultLook(string folder)
    {
        var app = Application.Current;
        var kept = app?.Resources;
        if (app != null) app.Resources = new ResourceDictionary();
        try
        {
            foreach (bool dark in new[] { false, true })
                await FakeCaptionHost.WithConsole(async (console, session, host, _) =>
                {
                    string mode = dark ? "dark" : "light";
                    // (not shown, so the tab does not follow by itself: the screen's text is given to it)
                    session.Mirror(true);
                    host.Show(new("song:shot", "주님은 나의 목자", CaptionItemKind.Text), ["주님은 나의 목자시니\n내게 부족함 없으리", "푸른 풀밭에 누이시며", "쉴 만한 물가로"], 0);
                    session.ShowScreenText(host.OutputText);
                    console.AirButton.Command.Execute(null);
                    await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    File.WriteAllBytes(Path.Combine(folder, $"11-default-look-{mode}.png"), DrawWindow(Window.GetWindow(console), ShotSizes[0].Width, ShotSizes[0].Height));
                    await console.Settings.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    File.WriteAllBytes(Path.Combine(folder, $"12-default-look-settings-{mode}.png"), DrawWindow(console.Settings, console.Settings.Width, console.Settings.Height));
                }, dark, show: false);
        }
        finally { if (app != null) app.Resources = kept!; }
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
