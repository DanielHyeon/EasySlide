using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// a046 bundle B: a host of the caption tab that is not the WPF app's view model (as Axicultuss will be): its screen text,
/// pages and picked row through <see cref="ICaptionHost"/> only, with none of the optional parts.
/// </summary>
internal sealed class FakeCaptionHost : ICaptionHost
{
    internal sealed class FakeLive : ICaptionLiveSource
    {
        public CaptionLiveSnapshot Current { get; private set; } = CaptionLiveSnapshot.Off;
        public event EventHandler<CaptionLiveSnapshot>? Changed;
        public void Set(CaptionLiveState state, string? pptPath = null, int slide = 0) => Changed?.Invoke(this, Current = new(state, pptPath, slide));
    }

    public readonly FakeLive LiveSource = new();
    public ICaptionLiveSource Live => LiveSource;
    public event EventHandler<CaptionHostChange>? Changed;
    public void Raise(CaptionHostChange change) => Changed?.Invoke(this, change);

    public string PlanName { get; set; } = "fake plan";
    public CaptionItem? OutputItem { get; set; }
    public string OutputText { get; set; } = "";
    public IReadOnlyList<CaptionPage> OutputPages { get; set; } = [];
    public CaptionSlidePanel? OutputPanel => null;
    public CaptionSlidePanel? PreviewPanel => null;

    // The output column's item: a song of these pages, shown at page `at` (as a host tells a turn: its text, then its pages).
    public string[] Song = [];
    public readonly List<int> PagesAsked = [];

    public void Show(CaptionItem item, string[] pages, int at)
    {
        (OutputItem, Song) = (item, pages);
        ShowPage(at);
    }

    // A deck in the output column: no text, no pages; its own slide is what the output shows (no OutputPanel here).
    public void ShowDeck(CaptionItem deck)
    {
        (OutputItem, OutputText, OutputPages, Song) = (deck, "", [], []);
        Raise(CaptionHostChange.Output);
    }

    private void ShowPage(int at)
    {
        OutputText = Song[at];
        Raise(CaptionHostChange.Output);
        OutputPages = [.. Song.Select((text, n) => new CaptionPage(n, Song.Length > 1 ? $"{n + 1}/{Song.Length}" : "", text, n == at))];
        Raise(CaptionHostChange.OutputPages);
    }

    public Func<int, Task>? Step; // the host's own step of its output (null: it cannot step)
    public Exception? Throws; // thrown by CanStepOutput and SendPicked (a host that fails there)
    public bool CanStepOutput(int direction) => Throws is { } error ? throw error : Step != null;
    public Task StepOutputAsync(int direction) => Step?.Invoke(direction) ?? Task.CompletedTask;

    public void GoToOutputPage(int index)
    {
        PagesAsked.Add(index);
        ShowPage(index);
    }

    public CaptionItem? Picked { get; set; }
    public bool CanSendPicked => true;
    public TaskCompletionSource? Sending; // a send under way (the test ends it)
    public Task PickedSending => Sending?.Task ?? Task.CompletedTask;
    public readonly List<CaptionItem> Sent = [];

    public async Task SendPickedAsync()
    {
        if (Throws is { } error)
        {
            await Task.Yield(); // (fails from the task, as a host's command does)
            throw error;
        }
        Sent.Add(Picked!);
        Show(Picked!, [Picked!.Title + " 1절"], 0);
    }

    public ICaptionSlideRenderer? Renderer => null;
    public Func<string, CancellationToken, Task<CaptionDeck>>? ReadLegacyDeck { get; set; }
    public ICaptionSongSource? SongDatabase => null;
    public CameraOwnership? Camera => null;
    public void StopLocalCamera() { }
    public Action? ToggleOutputCamera => null;
    public ICaptionQueueRecovery? Recovery { get; set; } // (read by the tab when it is made: set it in WithConsole's setUp)
    public readonly List<string> Errors = [];
    public void WriteError(string action, Exception error) => Errors.Add(action);

    internal static readonly CaptionScreen Screen = new(new(0, 0, 1920, 1080), new(0, 0, 1920, 1040), 1, true);

    /// <summary>The caption tab of a fake host in a stand-in window (strip over it, the output column's size), no view model behind it.</summary>
    internal static Task WithConsole(Func<CaptionConsole, CaptionSession, FakeCaptionHost, DirectoryInfo, Task> exercise, bool? dark = null, bool show = true,
        Action<FakeCaptionHost>? setUp = null) =>
        StaHelper.RunOnStaAsync(async () =>
        {
            var directory = Directory.CreateTempSubdirectory();
            try
            {
                var host = new FakeCaptionHost();
                setUp?.Invoke(host);
                using var session = new CaptionSession(host.Live, null, Path.Combine(directory.FullName, "caption-style.json"));
                using var server = new CaptionBroadcastServer(session, 0);
                using var store = new CaptionRecoveryStore(Path.Combine(directory.FullName, "CaptionRecovery"));
                using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
                var console = new CaptionConsole(host, session, server, store, obs, () => [Screen]);
                if (dark is { } mode) CaptionTheme.Apply(console, mode);
                DockPanel.SetDock(console.Strip, Dock.Top);
                var window = new Window
                {
                    ShowActivated = false, ShowInTaskbar = false, Width = 720, Height = 960,
                    Content = new DockPanel { Children = { console.Strip, console } },
                };
                if (dark is { } look) CaptionTheme.Apply(window, look); // (the window's own background)
                console.Strip.CaptionTab.IsChecked = true;
                try
                {
                    if (show) window.Show();
                    await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    await exercise(console, session, host, directory);
                }
                finally
                {
                    console.EndSession();
                    window.Close();
                }
            }
            finally { directory.Delete(true); }
        });
}

public sealed partial class CaptionConsoleTests
{
    private static readonly CaptionItem Verses = new("song:fake-verses", "세 절 찬양", CaptionItemKind.Text);
    private static readonly string[] ThreeVerses = ["첫째 절", "둘째 절", "셋째 절"];

    // a046 spec "the caption tab opens without the app's view model": the host tells the screen's text changed; the tab prepares it, and the switch
    // puts it on air.
    [Fact]
    public Task AnotherHostsScreenTextIsFollowedAndTheSwitchPutsItOnAir() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        Assert.True(console.ScreenMode.IsChecked == true && session.Mirroring); // the tab opens following the screen
        host.Show(Verses, ThreeVerses, 0);
        await Pump(console, 200);
        Assert.Equal(("첫째 절", false, ""), (session.ReadyText, session.Visible, session.PublicText));
        Assert.Equal("세 절 찬양", console.ScoreRowLabel.Text); // the item's title on the card
        console.ToggleOnAir();
        await Pump(console, 100);
        Assert.Equal((true, "첫째 절"), (session.Visible, session.PublicText));
        host.GoToOutputPage(1); // the host's own turn of its output
        await Pump(console, 200);
        Assert.Equal("둘째 절", session.PublicText);
        host.Show(new("text:empty", "빈 글", CaptionItemKind.Text), [""], 0);
        await Pump(console, 200);
        Assert.Equal("", session.PublicText);
        Assert.Equal(CaptionText.Get("CaptionConsole", "NoScreenText"), console.Status.Text);
    });

    // The host's pages are the strip's steps ("1/1" for a page the host marks with nothing); a picked step asks the host for
    // that page (0-based) and the caption follows the page it then shows.
    [Fact]
    public Task AnotherHostsPagesAreTheStepsAndAPickedStepGoesToItsPage() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        console.ToggleOnAir();
        host.Show(Verses, ThreeVerses, 0);
        await Pump(console, 200);
        Assert.Equal(["1/3", "2/3", "3/3"], editor.Steps.Select(step => step.Label));
        Assert.Equal(ThreeVerses, editor.Steps.Select(step => step.Text));
        Assert.Equal((editor.Steps[0], editor.Steps[1]), (editor.CurrentStep, editor.NextStep));

        console.StepStrip.SelectedItem = editor.Steps[2];
        await Pump(console, 200);
        Assert.Equal([2], host.PagesAsked);
        Assert.Equal(("셋째 절", editor.Steps[2]), (session.PublicText, editor.CurrentStep));
        Assert.Null(editor.NextStep);

        host.Show(new("text:one", "한 쪽 글", CaptionItemKind.Text), ["한 쪽"], 0);
        await Pump(console, 200);
        Assert.Equal(["1/1"], editor.Steps.Select(step => step.Label));
    });

    // The picked row goes to the host's output column while nothing is on the screen (the tab shown, following it); not
    // while live or blacked out, nor a media row, nor the row the column already holds.
    [Fact]
    public Task AnotherHostsPickedRowIsSentOnlyWhileTheScreenIsNotLive() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        CaptionItem Row(string id, CaptionItemKind kind = CaptionItemKind.Text) => new(id, id + " 찬양", kind);
        async Task Pick(CaptionItem row)
        {
            host.Picked = row;
            host.Raise(CaptionHostChange.Picked);
            await Pump(console, 200);
        }
        var first = Row("song:a");
        await Pick(first);
        Assert.Equal([first], host.Sent);
        Assert.Equal(("song:a 찬양 1절", false), (session.ReadyText, session.Visible)); // seen in the tab; nothing goes out

        await Pick(Row("video:b", CaptionItemKind.Media));
        Assert.Single(host.Sent);

        host.LiveSource.Set(CaptionLiveState.Active);
        await Pick(Row("song:c"));
        host.LiveSource.Set(CaptionLiveState.Hidden);
        await Pick(Row("song:d"));
        Assert.Single(host.Sent);

        host.LiveSource.Set(CaptionLiveState.Off);
        await Pick(Row("song:a")); // the row the output column holds
        Assert.Single(host.Sent);
        console.DirectMode.IsChecked = true; // typing: a pick is the host's alone
        await Pick(Row("song:e"));
        Assert.Single(host.Sent);
    });

    // A host with none of the optional parts (no output or preview panel, renderer, old-format reader, song database,
    // camera, output camera, worship order recovery): the tab and its settings open, read and follow a deck, say that an
    // old-format deck cannot be read, and keep their recovery without a worship order.
    [Fact]
    public Task AHostWithNoneOfTheOptionalPartsHasAWorkingTab() => FakeCaptionHost.WithConsole(async (console, session, host, directory) =>
    {
        Assert.IsType<CameraOwnership>(console.CameraPreview.DataContext); // the tab's own, for the OBS camera preview
        Assert.Equal(Visibility.Collapsed, console.Settings.ToggleCameraButton.Visibility); // no output camera to turn
        await ClickAsync(console.SettingsButton);
        Assert.True(console.Settings.IsVisible);
        console.Settings.Hide();

        // A deck in the output column, no OutputPanel: the item's own slide is what the output shows.
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        CaptionItem Deck(int slide) => new("ppt:1", "덱", CaptionItemKind.PowerPoint, path, slide);
        host.ShowDeck(Deck(2));
        Assert.True(await Until(console, () => session.Following && session.CurrentSlide == 2));
        Assert.Equal("이장", session.ReadyText);
        Assert.Equal(3, console.StepStrip.Items.Count);
        host.ShowDeck(Deck(3)); // the host turns its output (not live): the caption follows
        Assert.True(await Until(console, () => session.CurrentSlide == 3));
        Assert.Equal("삼장", session.ReadyText);
        // Live with the deck on the screen: the screen's slide drives the caption, a turn of the column does not.
        host.LiveSource.Set(CaptionLiveState.Active, path, 1);
        Assert.True(await Until(console, () => session.CurrentSlide == 1));
        host.LiveSource.Set(CaptionLiveState.Active, path, 2);
        Assert.True(await Until(console, () => session.CurrentSlide == 2));
        host.ShowDeck(Deck(3));
        await Pump(console, 300);
        Assert.Equal(2, session.CurrentSlide);
        host.LiveSource.Set(CaptionLiveState.Off);

        Assert.False(await console.PrepareDeckAsync(Path.Combine(directory.FullName, "옛.ppt"), 1));
        Assert.Contains(CaptionText.Get("CaptionConsole", "LegacyDeckUnsupported"), console.Status.Text);

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(CaptionConsole).GetMethod("Changed", flags)!.Invoke(console, null);
        await (Task)typeof(CaptionConsole).GetMethod("SaveAsync", flags)!.Invoke(console, null)!;
        var store = (CaptionRecoveryStore)typeof(CaptionConsole).GetField("_store", flags)!.GetValue(console)!;
        var saved = store.Read(host.PlanName)!;
        Assert.Equal((0, -1, path), (saved.Items.Length, saved.SelectedIndex, saved.DeckPath));
        await console.RestoreAsync(saved with { Draft = "되돌린 초안" }); // (no worship order to put back)
        Assert.Equal("되돌린 초안", ((CaptionConsoleViewModel)console.DataContext).DraftText);
        Assert.Equal(["open caption PPT"], host.Errors); // (the old-format deck, told to the host's log)
    });

    // A picked deck, no PreviewPanel, read again from the settings ("reload the deck on the screen"): with nothing on the
    // screen it is the picked deck, at the preview column's slide, which a host without one does not have: the first.
    [Fact]
    public Task AnotherHostsPickedDeckIsReadAgainAtItsFirstSlide() => FakeCaptionHost.WithConsole(async (console, session, host, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        host.Picked = new("ppt:picked", "고른 덱", CaptionItemKind.PowerPoint, path, 3); // (not told: nothing is sent)
        typeof(CaptionConsole).GetMethod("OpenLiveDeck", Private)!.Invoke(console, [null, null]);
        Assert.True(await Until(console, () => session.Deck?.Path == path && session.Following));
        Assert.Equal((1, "일장"), (session.CurrentSlide, session.ReadyText));
        Assert.Empty(host.Sent);
    });

    // A pick while a send is under way waits for it; picks meanwhile are not sent, only the row the operator stops on.
    [Fact]
    public Task AnotherHostsPickDuringASendWaitsAndOnlyTheLastRowIsSent() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        host.Sending = new TaskCompletionSource();
        CaptionItem first = new("song:a", "첫 곡", CaptionItemKind.Text), last = new("song:b", "둘째 곡", CaptionItemKind.Text);
        host.Picked = first;
        host.Raise(CaptionHostChange.Picked);
        await Pump(console, 200);
        host.Picked = last;
        host.Raise(CaptionHostChange.Picked);
        await Pump(console, 200);
        Assert.Empty(host.Sent); // both wait for the send under way
        host.Sending.SetResult();
        await Pump(console, 300);
        Assert.Equal([last], host.Sent);
        Assert.Equal("둘째 곡 1절", session.ReadyText);
    });

    // No renderer: a slide whose score would come from its render goes out as its text (no row), and says the score is
    // being made. No song database: the lyrics read in a deck's pictures stay as read, and no failure is logged.
    [Fact]
    public Task AnotherHostWithoutRendererOrSongsKeepsTheTextAndTheLyricsAsRead() => FakeCaptionHost.WithConsole(async (console, session, host, directory) =>
    {
        var text = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("text").FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"));
        Assert.True(await console.PrepareDeckAsync(text, 1));
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(2, CaptionDeck.AllTextKey);
        await Pump(console, 300);
        Assert.Equal(("이장", (System.Windows.Media.Imaging.BitmapSource?)null), (session.ReadyText, session.ReadyImage));
        Assert.Equal(CaptionText.Get("CaptionConsole", "ScoreMaking"), console.Status.Text);

        static OcrLine At(int center, string line) => new([new OcrWord(line, 100, center - 10, 200, 20)]);
        console.ReadLyrics = (files, _) => Task.FromResult<OcrPage[]>([.. files.Select(file =>
            new OcrPage(file, null, [At(177, "나의 맘 받으소서"), At(357, "오셔서 주님의 저소 삽으소서")]))]); // (a misread a song database would put right)
        var score = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("score").FullName, [CaptionScorePictureTests.ScorePng()], CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(score, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal("나의 맘 받으소서\n오셔서 주님의 저소 삽으소서", session.Deck!.Slides[0].Boxes.Single(box => box.Key == CaptionSession.RecognizedKey).Text);
        Assert.Empty(host.Errors);
    });

    // A worship order the host cannot put back (its Restore throws): told, logged, and nothing is recovered (the draft
    // as it was); nothing is thrown out of the tab.
    private sealed class FailingRecovery : ICaptionQueueRecovery
    {
        public (JsonElement[] Items, int Selected) Capture() => ([], -1);
        public void Restore(IReadOnlyList<JsonElement> items, string plan, int selected) => throw new InvalidDataException("되돌릴 수 없는 순서");
        public event Action? Replacing { add { } remove { } }
    }

    [Fact]
    public Task AWorshipOrderTheHostCannotPutBackRecoversNothingAndSaysSo() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        console.DirectMode.IsChecked = true;
        console.Draft.SetCurrentValue(TextBox.TextProperty, "지금 초안");
        var row = JsonSerializer.SerializeToElement(new { Id = "song:1", Title = "곡", Kind = "Song" });
        await console.RestoreAsync(new CaptionRecovery(1, 1, host.PlanName, "저장한 초안", null, 1, "", [], [], [row], 0));
        Assert.Equal("지금 초안", editor.DraftText);
        Assert.Equal(CaptionText.Format("CaptionConsole", "RecoverFailed", "되돌릴 수 없는 순서"), console.Status.Text);
        Assert.Equal(["recover worship list"], host.Errors);
    }, setUp: host => host.Recovery = new FailingRecovery());

    // ---- the WPF app's adapter ----

    // The picked row is one object until the pick changes (the tab tells by reference whether it still waits for the same
    // row); the output and preview columns are read as they are.
    [Fact]
    public Task TheAppsPickedRowIsTheSameObjectUntilThePickChanges() => WithConsole(async (console, _, _) =>
    {
        var host = (MainViewModelCaptionHost)console.HostApp;
        var main = host.Main;
        var (first, second, _) = ThreeSongs(main);
        main.SelectedItem = first;
        var picked = host.Picked;
        Assert.Equal(new CaptionItem(first.Id, first.Title, CaptionItemKind.Text), picked);
        Assert.Same(picked, host.Picked);
        main.SelectedItem = second;
        Assert.NotSame(picked, host.Picked);
        Assert.Equal(second.Id, host.Picked!.Id);
        main.SelectedItem = null;
        Assert.Null(host.Picked);
        Assert.Equal(new CaptionItem("powerpoint:x", "덱", CaptionItemKind.PowerPoint, @"C:\덱.pptx", 3),
            MainViewModelCaptionHost.View(new LiveQueueItem("powerpoint:x", "덱") { ContentPath = @"C:\덱.pptx", SlideNumber = 3 })); // (a row of no kind: the app infers it)
        Assert.Equal(CaptionItemKind.Media, MainViewModelCaptionHost.View(new LiveQueueItem("m", "영상", LiveItemKinds.Media)).Kind);
        await Task.CompletedTask;
    }, asOpened: true);

    // Each row of the worship order is serialized for the recovery once: saved again, an unchanged row is the kept one;
    // a changed row (a new object) is serialized anew.
    [Fact]
    public Task TheAppSerializesEachRowForTheRecoveryOnce() => WithConsole(async (console, _, _) =>
    {
        var host = (MainViewModelCaptionHost)console.HostApp;
        var (first, second, third) = ThreeSongs(host.Main);
        ICaptionQueueRecovery recovery = host;
        var rows = recovery.Capture().Items;
        var kept = new[] { first, second, third }.Select(MainViewModelCaptionHost.RecoveryRow).ToArray();
        var again = recovery.Capture().Items;
        Assert.Equal(kept, new[] { first, second, third }.Select(MainViewModelCaptionHost.RecoveryRow)); // (the same boxes: not made again)
        Assert.Equal(rows.Select(row => row.GetRawText()), again.Select(row => row.GetRawText()));
        Assert.Equal(kept.Select(box => box.Value.GetRawText()), rows.Select(row => row.GetRawText()));
        var changed = second with { Lyrics = "[1]\n바뀐 가사" };
        Assert.NotSame(MainViewModelCaptionHost.RecoveryRow(second), MainViewModelCaptionHost.RecoveryRow(changed));
        Assert.Contains("바뀐 가사", MainViewModelCaptionHost.FromRecovery(MainViewModelCaptionHost.ToRecovery(changed)).Lyrics);
        await Task.CompletedTask;
    });

    // A recovery file as the app wrote it before a046 (the bytes 684e93d's CaptionRecoveryStore wrote) is read, its worship
    // order put back, and written again in the same shape.
    [Fact]
    public async Task ARecoveryFileOfTheShapeBeforeTheLibraryIsReadAndWrittenTheSame()
    {
        const string before = """{"Version":1,"Revision":5,"Plan":"\uC8FC\uC77C \uC608\uBC30","Draft":"\uCD08\uC548 \uAE00","DeckPath":"C:\\Axicultus\\Powerpoint\\\uAD11\uACE0.pptx","Slide":3,"Box":"shape:TextBox 3","Overrides":{"2":"\uACE0\uCE5C \uC790\uB9C9"},"History":[{"Time":"2026-11-22T11:05:00+09:00","Text":"\uC9C0\uB09C \uC790\uB9C9"}],"Items":[{"Id":"song:7","Title":"\uC8FC \uD558\uB098\uB2D8 \uC9C0\uC73C\uC2E0 \uBAA8\uB4E0 \uC138\uACC4","Kind":"Song","SlideNumber":2,"Lyrics":"[1]\n\uC8FC \uD558\uB098\uB2D8 \uC9C0\uC73C\uC2E0 \uBAA8\uB4E0 \uC138\uACC4","ContentPath":null,"FormatData":null,"UseIndividualFormatting":true,"Sequence":"1 C 2 C","SongLayout":null,"SourceFolderNo":3,"Capo":0,"Notations":null},{"Id":"ppt:1","Title":"\uAD11\uACE0","Kind":"PowerPoint","SlideNumber":4,"Lyrics":null,"ContentPath":"C:\\Axicultus\\Powerpoint\\\uAD11\uACE0.pptx","FormatData":null,"UseIndividualFormatting":true,"Sequence":null,"SongLayout":null,"SourceFolderNo":null,"Capo":0,"Notations":null}],"SelectedIndex":1}""";
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            const string plan = "주일 예배";
            // where the app has always kept it: the plan's SHA-256 in the profile's CaptionRecovery folder
            string file = Path.Combine(dir.FullName, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plan))) + ".json");
            await File.WriteAllTextAsync(file, before);
            using var store = new CaptionRecoveryStore(dir.FullName, MainViewModelCaptionHost.IsRecoverableItem);
            var saved = Assert.IsType<CaptionRecovery>(store.Read(plan));
            Assert.Equal(("초안 글", @"C:\Axicultus\Powerpoint\광고.pptx", 3, "shape:TextBox 3", "고친 자막", "지난 자막", 1),
                (saved.Draft, saved.DeckPath, saved.Slide, saved.Box, saved.Overrides[2], saved.History.Single().Text, saved.SelectedIndex));
            var rows = saved.Items.Select(MainViewModelCaptionHost.FromRecovery).ToArray();
            // (a row is equal only to itself: its fields are compared)
            static object Fields(LiveQueueItem row) => (row.Id, row.Title, row.Kind, row.SlideNumber, row.Lyrics, row.ContentPath, row.Sequence, row.SourceFolderNo, row.UseIndividualFormatting);
            Assert.Equal(("song:7", "주 하나님 지으신 모든 세계", "Song", 2, (string?)"[1]\n주 하나님 지으신 모든 세계", (string?)null, (string?)"1 C 2 C", (int?)3, true), Fields(rows[0]));
            Assert.Equal(("ppt:1", "광고", "PowerPoint", 4, (string?)null, (string?)@"C:\Axicultus\Powerpoint\광고.pptx", (string?)null, (int?)null, true), Fields(rows[1]));
            // the rows as the app keeps them now are the rows of the file
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(before)!["Items"], JsonSerializer.SerializeToNode(rows.Select(MainViewModelCaptionHost.ToRecovery).ToArray())));

            await store.SaveAsync(saved with { Revision = 6, Draft = "다음 초안" });
            store.Dispose();
            var after = JsonNode.Parse(await File.ReadAllTextAsync(file))!.AsObject();
            var expected = JsonNode.Parse(before)!.AsObject();
            expected["Revision"] = 6;
            expected["Draft"] = "다음 초안";
            Assert.True(JsonNode.DeepEquals(expected, after), after.ToJsonString());
            Assert.Equal(before, await File.ReadAllTextAsync(file + ".bak")); // the file before, kept as its backup

            // a damaged row (no title) in the latest file: the backup is read
            await File.WriteAllTextAsync(file, before.Replace("\"Title\":\"\\uAD11\\uACE0\",", ""));
            Assert.Equal("초안 글", store.Read(plan)!.Draft);
        }
        finally { dir.Delete(true); }
    }
}

/// <summary>
/// The default look's tests set the application's resources aside while they run: nothing else may run meanwhile (the
/// other WPF tests read and change those resources).
/// </summary>
[CollectionDefinition("Caption default look", DisableParallelization = true)]
public sealed class CaptionDefaultLookCollection;

/// <summary>
/// a046 design 4b "default control look": the caption tab and its settings window with the library's default look only, no app
/// theme and no app control styles: the application's resources (the app's theme, when a test of the run made an
/// Application) are an empty dictionary while the tab is made and checked, and put back after.
/// </summary>
[Collection("Caption default look")]
public sealed class CaptionDefaultLookTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheTabAndItsSettingsOpenWithTheLibrarysDefaultLookOnly(bool dark)
    {
        var app = Application.Current;
        var kept = app?.Resources;
        if (app != null) app.Resources = new ResourceDictionary();
        try { await Check(dark); }
        finally { if (app != null) app.Resources = kept!; }
    }

    private Task Check(bool dark) => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        output.WriteLine(Application.Current == null ? "no Application in this process" : "the Application's resources set aside");
        Assert.True(Application.Current is null or { Resources: { Count: 0, MergedDictionaries.Count: 0 } }); // no app theme or control style
        var look = CaptionTheme.Defaults(dark);
        string[] keys = [.. look.Keys.OfType<string>().Where(key => key.StartsWith("Brush.", StringComparison.Ordinal)
            || key.StartsWith("Font.", StringComparison.Ordinal) || key.StartsWith("Type.", StringComparison.Ordinal))];
        Assert.Equal(24, keys.Length);
        await CaptionConsoleTests.ClickAsync(console.SettingsButton);
        Assert.True(console.Settings.IsVisible);
        foreach (FrameworkElement part in new FrameworkElement[] { console, console.Strip, console.Settings })
        {
            // the look on the element is the library's (none of the app's dictionaries is on it)
            var merged = Assert.Single(part.Resources.MergedDictionaries);
            foreach (var key in keys) Assert.Same(merged[key], part.FindResource(key));
        }
        var page = Assert.IsType<SolidColorBrush>(console.Background).Color;
        Assert.Equal(((SolidColorBrush)look["Brush.Surface.Page"]).Color, page);
        Assert.Equal(((SolidColorBrush)look["Brush.Surface.Page"]).Color, Assert.IsType<SolidColorBrush>(console.Settings.Background).Color);
        Assert.Equal(((FontFamily)look["Font.Primary"]).Source, console.FontFamily.Source);
        Assert.Equal(dark, page.R < 128);
        // the settings window's check boxes in the look's text colour (the system's black is not seen on the dark look)
        var text = ((SolidColorBrush)look["Brush.Text.Primary"]).Color;
        Assert.All(new[] { console.Settings.AutoObs, console.Settings.PreviewObsCamera, console.Settings.EnableSource },
            box => Assert.Equal(text, Assert.IsType<SolidColorBrush>(box.Foreground).Color));
        // and it works: the screen's text followed, put on air from the strip
        host.Show(new("song:look", "찬양", CaptionItemKind.Text), ["주님은 나의 목자"], 0);
        await CaptionConsoleTests.ClickAsync(console.AirButton);
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("주님은 나의 목자", session.PublicText);
        Assert.True(console.SettingsButton.ActualHeight > 0 && console.Draft.ActualWidth == 0 && console.StepStrip.ActualHeight > 0);
    }, dark);
}
