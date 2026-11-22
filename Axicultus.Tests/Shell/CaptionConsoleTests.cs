using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public sealed class CaptionConsoleTests
{
    [Fact]
    public Task WindowPublishesOnlyOnTakeAndSuppressesImeComposition() => WithConsole(async (console, session, _) =>
    {
        Assert.True(console.Instant.IsChecked); // typing goes on air by default
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, false);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "첫 번째 문장");
        Assert.Equal("", session.PublicText);
        await ClickAsync(console.TakeButton);
        Assert.Equal("첫 번째 문장", session.PublicText);
        Assert.Equal("첫 번째 문장", Assert.Single(session.History).Text);

        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, true);
        var keyboard = InputManager.Current.PrimaryKeyboardDevice;
        var composition = new TextComposition(InputManager.Current, console.Draft, "한");
        console.Draft.RaiseEvent(new TextCompositionEventArgs(keyboard, composition)
            { RoutedEvent = TextCompositionManager.PreviewTextInputStartEvent });
        console.Draft.SetCurrentValue(TextBox.TextProperty, "ㅎ");
        Assert.Equal("첫 번째 문장", session.PublicText);
        Assert.False(console.TakeButton.IsEnabled);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "한");
        console.Draft.RaiseEvent(new TextCompositionEventArgs(keyboard, composition)
            { RoutedEvent = TextCompositionManager.TextInputEvent, Handled = true });
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("한", session.PublicText);
        Assert.Equal("첫 번째 문장", Assert.Single(session.History).Text);
    }, expectedSavedDraft: "한");

    [Fact]
    public Task HidingKeepsNewDraftOffAirAndHeldShortcutCannotAddHistory() => WithConsole(async (console, session, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        Assert.False(session.Visible); // a new console starts hidden
        console.Draft.SetCurrentValue(TextBox.TextProperty, "송출 중");
        Assert.Equal("", session.PublicText); // typing alone does not reveal it
        editor.TakeFromShortcut(isRepeat: false); // 송출 (Enter) does
        Assert.Equal("송출 중", session.PublicText);
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, true);
        await ClickAsync(console.HideButton);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "대기 중");
        editor.TakeFromShortcut(isRepeat: true);
        Assert.Equal("", session.PublicText);
        Assert.True(console.Instant.IsChecked);
        Assert.Equal("송출 중", Assert.Single(session.History).Text);
        await ClickAsync(console.ShowButton);
        Assert.Equal("대기 중", session.PublicText); // what was typed while hidden goes out on 표시
        console.Close();
        Assert.False(console.IsVisible);
        Assert.Equal("대기 중", session.PublicText);
        console.Show();
        Assert.Equal("대기 중", console.Draft.Text);
    }, expectedSavedDraft: "대기 중");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task FailedDeckReadKeepsFollowingTheCurrentDeckAndLogsTheError(bool corruptFile) =>
        WithConsole(async (console, session, directory) =>
        {
            var previous = new CaptionDeck("live.pptx", [new([new("body", "본문", "송출 중")])]);
            session.Prepare(previous);
            session.Follow(1, "body");
            var published = session.Published;
            var path = Path.Combine(directory.FullName, "새 자료.pptx");
            if (corruptFile) await File.WriteAllTextAsync(path, "invalid PPT");
            await console.PrepareDeckAsync(path, 1);
            Assert.Contains("PPT 읽기 실패", console.Status.Text);
            Assert.Same(previous, session.Deck);
            Assert.True(session.Following);
            Assert.Same(published, session.Published);
            var log = File.ReadAllText(Assert.Single(directory.GetFiles("*.log")).FullName);
            Assert.Contains("자막 PPT 열기", log);
            if (!corruptFile) Assert.Contains(nameof(FileNotFoundException), log);
        });

    [Fact]
    public void RestoringADraftCannotPublishOrRequestAnotherSave()
    {
        using var session = new CaptionSession(new LiveSessionService());
        session.Publish("현재 송출");
        var editor = new CaptionConsoleViewModel(session) { IsInstant = true, IsRestoring = true };
        int saves = 0;
        editor.SaveRequested += () => saves++;
        editor.DraftText = "복구한 초안";
        editor.TakeFromShortcut(isRepeat: false);
        Assert.False(editor.TakeCommand.CanExecute(null));
        Assert.Equal("현재 송출", session.PublicText);
        Assert.Empty(session.History);
        Assert.Equal(0, saves);
    }

    [Fact]
    public void EnterSendsTheLineEmptiesTheBoxAndKeepsTheCaption()
    {
        using var session = new CaptionSession(new LiveSessionService());
        var editor = new CaptionConsoleViewModel(session) { IsInstant = true };
        session.SetVisible(true); // 표시
        editor.DraftText = "주님은 나의 목자";
        Assert.Equal("주님은 나의 목자", session.PublicText);
        editor.TakeAndClear();
        Assert.Equal("", editor.DraftText);
        Assert.Equal("주님은 나의 목자", session.PublicText);
        editor.DraftText = "내게";
        Assert.Equal("내게", session.PublicText);
    }

    [Fact]
    public void StyleAndScoreImageReachThePublishedCaptionAndTheStyleIsKept() => StaHelper.RunOnSta(() =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            using (var session = new CaptionSession(new LiveSessionService(), null, path))
            {
                session.Publish("자막");
                session.SetStyle(new CaptionStyle("#FFE14D", false));
                Assert.Equal(("자막", "#FFE14D", false), (session.Published.Text, session.Published.Color, session.Published.Box));
                var score = System.Windows.Media.Imaging.BitmapSource.Create(4, 2, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, new byte[8], 4);
                session.SetImage(score);
                Assert.StartsWith("data:image/jpeg;base64,", session.Published.Image);
                Assert.Equal("", session.Published.Text); // score only
                Assert.True(session.HasText);
                Assert.False(session.HasPublicText);
                session.SetImage(score, withText: true); // score + caption
                Assert.Equal("자막", session.Published.Text);
                Assert.NotNull(session.Published.Image);
                Assert.NotNull(session.PublicImage);
                session.SetImage(null);
                Assert.Equal("자막", session.Published.Text);
            }
            using var reopened = new CaptionSession(new LiveSessionService(), null, path);
            Assert.Equal(new CaptionStyle("#FFE14D", false), reopened.Style);
            Assert.Equal(CaptionStyle.DefaultColor, CaptionStyle.Parse("red;background:url(x)", true).Color);
        }
        finally { directory.Delete(true); }
    });

    [Fact]
    public void ScoreHalfIsCutFromASlidePictureOwnedByAnotherThread() => StaHelper.RunOnSta(() =>
    {
        System.Windows.Media.Imaging.WriteableBitmap? slide = null;
        var ready = new System.Threading.ManualResetEventSlim();
        var owner = new System.Threading.Thread(() =>
        {
            var pixels = new byte[4 * 4];
            for (int i = 8; i < 16; i++) pixels[i] = 255; // bottom half white
            slide = new(4, 4, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
            slide.WritePixels(new System.Windows.Int32Rect(0, 0, 4, 4), pixels, 4, 0);
            ready.Set();
            Dispatcher.Run();
        });
        owner.SetApartmentState(System.Threading.ApartmentState.STA);
        owner.Start();
        ready.Wait();
        try
        {
            // No staff on it: the slide is cut in halves.
            var (bottom, rows, found) = CaptionConsole.CropRow(slide!, 1)!.Value;
            var top = CaptionConsole.CropRow(slide!, 0)!.Value.Image;
            Assert.Equal((2, false), (rows, found));
            Assert.True(bottom.IsFrozen);
            Assert.Equal((4, 2), (bottom.PixelWidth, bottom.PixelHeight));
            var row = new byte[8];
            bottom.CopyPixels(row, 4, 0);
            Assert.All(row, b => Assert.Equal(255, b));
            top.CopyPixels(row, 4, 0);
            Assert.All(row, b => Assert.Equal(0, b));
        }
        finally { Dispatcher.FromThread(owner)!.InvokeShutdown(); owner.Join(); }
    });

    // A slide like the hymn decks: title, then staves with a lyric line under each, at uneven heights.
    private static System.Windows.Media.Imaging.BitmapSource ScoreSlide(bool lightOnDark, params int[] staffTops)
    {
        const int width = 1200, height = 800;
        byte paper = lightOnDark ? (byte)20 : (byte)235, ink = lightOnDark ? (byte)240 : (byte)15;
        var pixels = new byte[width * height];
        Array.Fill(pixels, paper);
        void Fill(int x, int y, int w, int h) { for (int r = y; r < y + h; r++) Array.Fill(pixels, ink, r * width + x, w); }
        Fill(200, 20, 300, 30); // title
        foreach (var top in staffTops)
        {
            for (int line = 0; line < 5; line++) Fill(100, top + line * 12, 900, 2); // staff
            Fill(300, top - 20, 14, 20); // a note above the staff
            Fill(150, top + 80, 700, 34); // lyrics under it
        }
        Fill(100, 770, 450, 18); // the verse buttons at the foot of the slide
        Fill(0, 300, 40, 470);   // a background picture beside the lower staves
        return System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, pixels, width);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScoreRowsAreFoundWhereverTheStavesAre(bool lightOnDark) => StaHelper.RunOnSta(() =>
    {
        var rows = ScoreLayout.FindRows(ScoreSlide(lightOnDark, 90, 330, 560));
        Assert.Equal(3, rows.Length);
        Assert.Equal(0, rows[0].Area.Y); // the title goes with the first row
        for (int k = 0; k < 3; k++)
        {
            int staffTop = new[] { 90, 330, 560 }[k];
            var area = rows[k].Area;
            Assert.InRange(area.Y, 0, staffTop - 20);                   // the note above the staff is in
            Assert.True(area.Y + area.Height >= staffTop + 80 + 34);    // its lyrics are in
            Assert.InRange(area.X, 60, 100);                            // empty slide and the background picture are left out
            Assert.InRange(area.X + area.Width, 1000, 1199);
            Assert.Equal((staffTop, staffTop + 48), (rows[k].StaffTop, rows[k].StaffBottom));
        }
        Assert.InRange(rows[2].Area.Y + rows[2].Area.Height, 560 + 80 + 34, 769); // the verse buttons are not in

        // The same deck with the staves elsewhere.
        var moved = ScoreLayout.FindRows(ScoreSlide(lightOnDark, 200, 450));
        Assert.Equal(2, moved.Length);
        Assert.InRange(moved[1].Area.Y, 200 + 80 + 34, 450 - 20);
    });

    [Fact]
    public void AScoreRowIsCutFromTheSlide() => StaHelper.RunOnSta(() =>
    {
        var (row, rows, found) = CaptionConsole.CropRow(ScoreSlide(false, 90, 330), 1)!.Value;
        Assert.Equal((2, true), (rows, found));
        Assert.True(row.IsFrozen);
        Assert.InRange(row.PixelHeight, 330 + 80 + 34 - 310, 800);
    });

    private static OcrLine Line(int y, params string[] words) =>
        new([.. words.Select((word, i) => new OcrWord(word, 100 + i * 120, y, 100, 40))]);

    [Fact]
    public void ReadLyricsLoseTheirHoldDashesAndNotesReadAsLetters()
    {
        Assert.Equal("받으소서", LyricsReader.Clean(Line(0, "받으-소-서---")));
        Assert.Equal("주여 내 맘을 받아 주소서", LyricsReader.Clean(Line(0, "주여", "내", "맘을-", "받아", "주소-서")));
        Assert.Equal("", LyricsReader.Clean(Line(0, "OI", "J", "8")));      // note heads and rests
        Assert.Equal("Amazing grace", LyricsReader.Clean(Line(0, "Amazing", "grace")));
    }

    [Fact]
    public void EachScoreRowGetsTheLyricsUnderItsStaffOnly()
    {
        var page = new OcrPage("1.png", null,
        [
            Line(40, "나의", "빰으소서"),            // the title above the first staff
            Line(150, "OI"),                         // a sign inside the staff
            Line(300, "주가", "기뻐하는-"),          // row 1 lyrics
            Line(610, "주의", "성전", "되게", "하소서"), // row 2 lyrics
            Line(760, "1.나의", "맘", "받으소서"),    // the verse buttons under the last row
        ]);
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 0, 1000, 420), 120, 200),
            new(new System.Windows.Int32Rect(0, 420, 1000, 300), 470, 540),
        ];
        Assert.Equal(["주가 기뻐하는", "주의 성전 되게 하소서"], LyricsReader.RowLyrics(page, rows));
        Assert.Equal(["나의 빰으소서\n주가 기뻐하는\n주의 성전 되게 하소서\n1.나의 맘 받으소서"], LyricsReader.RowLyrics(page, [])); // not a score
    }

    [Fact]
    public void RecognizedLyricsBecomeABoxAndTheCaptionWhenChosen()
    {
        using var session = new CaptionSession(new LiveSessionService());
        session.Prepare(new CaptionDeck("deck.pptx", [new([new("label", "버튼", "1.나의 맘")], true), new([new("label", "버튼", "1.나의 맘")], true)]));
        session.Follow(1, CaptionSession.RecognizedKey);
        session.SetVisible(true);
        Assert.Equal("", session.PublicText);
        session.AddRecognized(1, "주가 기뻐하는");
        session.AddRecognized(1, "주가 기뻐하는\n주의 성전"); // read again: replaced, not added
        Assert.Equal("주가 기뻐하는\n주의 성전", session.PublicText);
        Assert.Single(session.Deck!.Slides[0].Boxes, box => box.Key == CaptionSession.RecognizedKey);
        Assert.Equal("1.나의 맘", session.TextFor(1, "label"));
    }

    [Fact]
    public void MirroringShowsTheOutputScreenTextEvenWhenNotLiveUntilTextIsTyped()
    {
        var live = new LiveSessionService(); // never goes live
        using var session = new CaptionSession(live);
        session.ShowScreenText("무시됨");
        Assert.Equal("", session.PublicText);
        session.Mirror(true);
        session.ShowScreenText("내 영혼아 찬양하라");
        Assert.Equal("", session.PublicText); // following the screen does not reveal a hidden caption
        session.SetVisible(true); // 표시
        Assert.Equal("내 영혼아 찬양하라", session.PublicText);
        Assert.True(session.HasAudienceText);
        session.Publish("직접 입력");
        session.ShowScreenText("다른 가사");
        Assert.False(session.Mirroring);
        Assert.Equal("직접 입력", session.PublicText);
    }

    [Fact]
    public void DestinationsChooseCameraOnlyObsOnlyOrBoth() => StaHelper.RunOnSta(() =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            using (var session = new CaptionSession(new LiveSessionService(), null, path))
            {
                var sent = new System.Collections.Generic.List<PublishedCaption>();
                session.PublishedChanged += _ => sent.Add(session.ForObs);
                session.Publish("자막");
                Assert.Equal(("자막", true), (session.ForObs.Text, session.HasAudienceText)); // both

                session.ObsEnabled = false; // camera only
                Assert.Equal(("", true), (session.ForObs.Text, session.HasAudienceText));
                Assert.Equal("", sent[^1].Text); // the OBS page is told to drop it

                session.ObsEnabled = true;
                session.AudienceEnabled = false; // OBS only
                Assert.Equal(("자막", false), (session.ForObs.Text, session.HasAudienceText));
                Assert.Equal("자막", sent[^1].Text);
                session.SyncShow = true; // kept with the destinations
                session.CaptionLines = 9; // clamped to 3
                Assert.Equal(3, session.CaptionLines);
                session.CaptionLines = 2;
                Assert.Equal(2, sent[^1].Lines); // the OBS page sizes its area from it
            }
            using var reopened = new CaptionSession(new LiveSessionService(), null, path);
            Assert.Equal((false, true, true, 2), (reopened.AudienceEnabled, reopened.ObsEnabled, reopened.SyncShow, reopened.CaptionLines));
        }
        finally { directory.Delete(true); }
    });

    private static async Task ClickAsync(Button button)
    {
        ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();
        await button.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static Task WithConsole(Func<CaptionConsole, CaptionSession, DirectoryInfo, Task> exercise,
        string? expectedSavedDraft = null) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var registrations = new ServiceCollection();
            App.ConfigureServices(registrations,
                new SettingsServiceOptions(Path.Combine(directory.FullName, "settings.json"), directory.FullName),
                Dispatcher.CurrentDispatcher);
            using var services = registrations.BuildServiceProvider();
            var main = services.GetRequiredService<MainViewModel>();
            var session = services.GetRequiredService<CaptionSession>();
            var store = services.GetRequiredService<CaptionRecoveryStore>();
            var console = new CaptionConsole(main, session, services.GetRequiredService<CaptionBroadcastServer>(), store,
                services.GetRequiredService<CameraOwnership>(), services.GetRequiredService<ObsOperator>(),
                new OperationalErrorLog(directory.FullName)) { ShowActivated = false, ShowInTaskbar = false };
            try
            {
                console.Show();
                await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                await exercise(console, session, directory);
            }
            finally { console.EndSession(); }
            if (expectedSavedDraft != null)
            {
                store.Dispose(); // Flush the last queued recovery write.
                Assert.Equal(expectedSavedDraft, store.Read(main.CurrentWorshipListName)!.Draft);
            }
        }
        finally { directory.Delete(true); }
    });
}
