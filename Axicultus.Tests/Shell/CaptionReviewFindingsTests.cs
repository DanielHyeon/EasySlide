using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>a050: the caption findings of the whole-branch review (design 2 to 5).</summary>
public sealed partial class CaptionConsoleTests
{
    // The start of a formatted caption text (what comes before its first value).
    private static string Lead(string key) => CaptionText.Format("CaptionConsole", key, "\u0001").Split('\u0001')[0];

    // a050 design 2 (the spec's scenario of a locked settings file): obs.json could not be read when the app started (not
    // JSON, or locked by another program): seeing the caption tab neither connects nor writes the file, and the tab says
    // the settings were not read. The operator's own Connect saves what the settings window holds.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task AnObsProfileThatCouldNotBeReadIsKeptUntilTheOperatorConnects(bool locked) => WithObsTab(
        locked ? Saved() : """{"Host":"192.0.2.10", not json""",
        ObsStart.BeforeTheTab, async (console, obs, path, profile) =>
        {
            Assert.StartsWith(Lead("SettingsReadFailed"), obs.Status.Message);
            Assert.False(await Until(console, () => obs.Connection != null, 1500));
            Assert.Equal(profile, File.ReadAllText(path)); // not written over with the window's defaults
            Assert.Contains(Lead("SettingsReadFailed"), console.ObsLabel.Text);
            // a050 design 13: the tab says what Connect will do
            Assert.Equal(CaptionText.Get("CaptionConsole", "ObsUnreadConnectSaves"), console.Settings.SettingsStatus.Text);

            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            console.Settings.Host.Text = "127.0.0.1";
            console.Settings.Port.Text = port.ToString();
            console.Settings.Password.Password = "typed-password";
            await ClickAsync(console.Settings.ConnectButton);
            Assert.True(await Until(console, () => obs.Connection != null));
            using var saved = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(("127.0.0.1", port), (saved.RootElement.GetProperty("Host").GetString(), saved.RootElement.GetProperty("Port").GetInt32()));
            Assert.Equal("typed-password", Decrypt(saved.RootElement.GetProperty("Secret").GetString()!));
            // a050 design 28: connected, the notice is gone from the settings window and the OBS line
            Assert.NotEqual(CaptionText.Get("CaptionConsole", "ObsUnreadConnectSaves"), console.Settings.SettingsStatus.Text);
            Assert.DoesNotContain(CaptionText.Get("CaptionConsole", "ObsUnreadConnectSaves"), console.ObsLabel.Text);
        }, lockWhileStarting: locked);

    private const string SavedProfile = """{"Host":"192.0.2.10","AutoConnect":false,"Secret":"@","Scenes":["설교","","",""]}""";
    private static string Saved() => SavedProfile.Replace("\"@\"", "\"" + CaptionSettingsFileTests.Secret("operator-obs-password") + "\"");

    // a050 design 18: the tab made before OBS's saved settings were read (the host starts the read just after making it,
    // or never): when it is first seen it waits for the read, shows the file's values and connects with them, as a tab
    // made afterwards does; the file keeps its host, password and scenes.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ATabMadeBeforeTheObsProfileWasReadWaitsForItAndUsesIt(bool hostStartsTheRead) => WithObsTab(Saved(),
        hostStartsTheRead ? ObsStart.AfterTheTab : ObsStart.Never, async (console, obs, path, profile) =>
        {
            Assert.True(await Until(console, () => obs.Connection != null));
            Assert.Equal(("192.0.2.10", "설교"), (console.Settings.Host.Text, console.Settings.Scene1.Text));
            Assert.Equal(Visibility.Visible, console.Settings.SavedPassword.Visibility);
            using var saved = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(("192.0.2.10", "설교"), (saved.RootElement.GetProperty("Host").GetString(), saved.RootElement.GetProperty("Scenes")[0].GetString()));
            Assert.Equal("operator-obs-password", Decrypt(saved.RootElement.GetProperty("Secret").GetString()!));
            Assert.NotEqual(CaptionText.Get("CaptionConsole", "ObsUnreadConnectSaves"), console.Settings.SettingsStatus.Text);
        });

    // a050 design 19: the OBS notice is on the OBS line and in the settings window; the tab's status line keeps its own
    // notice (a recovery to offer).
    [Fact]
    public Task TheObsNoticeLeavesTheTabsStatusLineToTheRecovery() => WithObsTab("""{"Host": not json""", ObsStart.BeforeTheTab,
        async (console, obs, path, profile) =>
        {
            Assert.False(await Until(console, () => obs.Connection != null, 1000));
            Assert.Equal(CaptionText.Get("CaptionConsole", "Recoverable"), console.Status.Text);
            Assert.Contains(CaptionText.Get("CaptionConsole", "ObsUnreadConnectSaves"), console.ObsLabel.Text);
            Assert.Equal(CaptionText.Get("CaptionConsole", "ObsUnreadConnectSaves"), console.Settings.SettingsStatus.Text);
        }, recoverable: true);

    // The tab made in the collapsed output column (as MainWindow's CaptionHost holds it: Loaded then, and again when first
    // shown); earlier: a recovery an earlier run left, there when the tab is made. show() shows the tab.
    private static Task WithCollapsedTab(string? earlier, Func<CaptionConsole, CaptionRecoveryStore, FakeCaptionHost, Func<Task>, Task> exercise,
        Func<JsonElement, bool>? isItem = null) =>
        StaHelper.RunOnStaAsync(async () =>
        {
            var directory = Directory.CreateTempSubdirectory();
            try
            {
                var host = new FakeCaptionHost();
                using var session = new CaptionSession(host.Live, null, Path.Combine(directory.FullName, "caption-style.json"));
                using var server = new CaptionBroadcastServer(session, 0);
                using var store = new CaptionRecoveryStore(Path.Combine(directory.FullName, "CaptionRecovery"), isItem);
                if (earlier != null)
                    await store.SaveAsync(new CaptionRecovery(1, 5, host.PlanName, earlier, null, 1, "", [], [], [JsonSerializer.SerializeToElement(new { Id = "song:1" })]));
                using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
                await obs.StartAsync();
                var console = new CaptionConsole(host, session, server, store, obs, () => [FakeCaptionHost.Screen]);
                var column = new ContentControl { Content = console, Visibility = Visibility.Collapsed };
                DockPanel.SetDock(console.Strip, Dock.Top);
                var window = new Window
                {
                    ShowActivated = false, ShowInTaskbar = false, Width = 720, Height = 960,
                    Content = new DockPanel { Children = { console.Strip, column } },
                };
                try
                {
                    window.Show();
                    await Pump(console, 300);
                    await exercise(console, store, host, async () =>
                    {
                        console.Strip.CaptionTab.IsChecked = true;
                        column.Visibility = Visibility.Visible;
                        await Pump(console, 500);
                    });
                }
                finally
                {
                    console.EndSession();
                    window.Close();
                }
            }
            finally { directory.Delete(true); }
        });

    // The tab's own save of the draft (the operator typing), through the debounce: done once the file holds it.
    private static async Task SaveOwnDraft(CaptionConsole console, CaptionRecoveryStore store, FakeCaptionHost host, string draft)
    {
        ((CaptionConsoleViewModel)console.DataContext).DraftText = draft;
        Assert.True(await Until(console, () => DraftSaved(store, host, draft)));
        await Pump(console, 100);
    }

    // (a read while the store replaces the file can find it in use: not saved yet)
    private static bool DraftSaved(CaptionRecoveryStore store, FakeCaptionHost host, string draft)
    {
        try { return store.Read(host.PlanName)?.Draft == draft; }
        catch (InvalidDataException) { return false; }
    }

    // a050 design 23 (fresh profile): the tab saves its own recovery while it is hidden; seen for the first time it does
    // not offer that (this run's own state) as a recovery.
    [Fact]
    public Task ARecoveryTheTabSavedItselfIsNotOfferedWhenItIsFirstShown() => WithCollapsedTab(null, async (console, store, host, show) =>
    {
        Assert.False(console.Settings.RecoverButton.IsEnabled);
        await SaveOwnDraft(console, store, host, "이번 실행 초안");
        await show();
        Assert.NotEqual(CaptionText.Get("CaptionConsole", "Recoverable"), console.Status.Text);
        Assert.False(console.Settings.RecoverButton.IsEnabled);
    });

    // a050 design 23: an earlier run's recovery, read when the tab was made, is still the one offered when the tab is first
    // shown after its own save: the hint, the button, and Recover gives the earlier run's draft.
    [Fact]
    public Task AnEarlierRunsRecoveryIsStillOfferedAfterTheTabsOwnSave() => WithCollapsedTab("앞선 실행 초안", async (console, store, host, show) =>
    {
        Assert.True(await Until(console, () => console.Settings.RecoverButton.IsEnabled)); // read when made
        await SaveOwnDraft(console, store, host, "이번 실행 초안");
        console.Status.Text = "";
        await show();
        Assert.Equal(CaptionText.Get("CaptionConsole", "Recoverable"), console.Status.Text);
        Assert.True(console.Settings.RecoverButton.IsEnabled);
        var kept = (CaptionRecovery)typeof(CaptionConsole).GetField("_recoverable", Private)!.GetValue(console)!; // what Recover restores
        Assert.Equal("앞선 실행 초안", kept.Draft);
        await console.RestoreAsync(kept);
        Assert.Equal("앞선 실행 초안", ((CaptionConsoleViewModel)console.DataContext).DraftText);
    });

    // a050 design 27: the first read of an earlier run's recovery is held back (the store's item check waits) while the
    // tab's own save comes due: the save waits for the read, so the earlier run's recovery is still offered on first show.
    [Fact]
    public async Task TheTabsOwnSaveWaitsForTheFirstRecoveryReadInProgress()
    {
        using var held = new ManualResetEventSlim();
        int checks = 0;
        await WithCollapsedTab("앞선 실행 초안", async (console, store, host, show) =>
        {
            ((CaptionConsoleViewModel)console.DataContext).DraftText = "이번 실행 초안";
            await Pump(console, 1500); // the debounce has come due while the first read is held
            held.Set();
            Assert.True(await Until(console, () => DraftSaved(store, host, "이번 실행 초안")));
            await Pump(console, 100);
            console.Status.Text = "";
            await show();
            Assert.Equal(CaptionText.Get("CaptionConsole", "Recoverable"), console.Status.Text);
            Assert.True(console.Settings.RecoverButton.IsEnabled);
            Assert.Equal("앞선 실행 초안", ((CaptionRecovery)typeof(CaptionConsole).GetField("_recoverable", Private)!.GetValue(console)!).Draft);
        }, isItem: item =>
        {
            if (Interlocked.Increment(ref checks) == 1) held.Wait(TimeSpan.FromSeconds(10)); // (only the first read is held)
            return true;
        });
    }

    // a050 design 28: a send of the picked row the host cancels ends quietly (as a cancelled step does).
    [Fact]
    public Task AnotherHostsCancelledSendOfThePickedRowEndsQuietly() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var unhandled = Unhandled(console);
        host.Throws = new OperationCanceledException();
        console.Status.Text = "";
        host.Picked = new("song:a", "찬양", CaptionItemKind.Text);
        host.Raise(CaptionHostChange.Picked);
        await Pump(console, 300);
        Assert.Empty(unhandled);
        Assert.Equal(("", 0), (console.Status.Text, host.Errors.Count));
    });

    // Pins what the harness saw: the tab is Loaded when made and again when first shown, and reads the recovery each time
    // while it has saved none of its own, so a recovery something else saved meanwhile is found on first show.
    [Fact]
    public Task ARecoverySavedBeforeTheTabsOwnSaveIsFoundWhenItIsFirstShown() => WithCollapsedTab(null, async (console, store, host, show) =>
    {
        int loaded = 1; // (the first, when made)
        console.Loaded += (_, _) => loaded++;
        Assert.False(console.Settings.RecoverButton.IsEnabled); // read when made: nothing yet
        await store.SaveAsync(new CaptionRecovery(1, 1, host.PlanName, "다른 저장", null, 1, "", [], [], []));
        await show();
        Assert.True(await Until(console, () => console.Settings.RecoverButton.IsEnabled));
        Assert.Equal((2, CaptionText.Get("CaptionConsole", "Recoverable")), (loaded, console.Status.Text));
    });

    private enum ObsStart { BeforeTheTab, AfterTheTab, Never }

    // obs.json as given; OBS's read started as the main window does before it makes the tab, just after, or never
    // (locked: another program holds the file while it is read; recoverable: a recovery of the plan is kept), then the
    // tab seen; then check.
    private static Task WithObsTab(string profile, ObsStart start, Func<CaptionConsole, ObsOperator, string, string, Task> check,
        bool lockWhileStarting = false, bool recoverable = false) =>
        StaHelper.RunOnStaAsync(async () =>
        {
            var directory = Directory.CreateTempSubdirectory();
            try
            {
                var path = Path.Combine(directory.FullName, "obs.json");
                File.WriteAllText(path, profile);
                var host = new FakeCaptionHost();
                using var session = new CaptionSession(host.Live, null, Path.Combine(directory.FullName, "caption-style.json"));
                using var server = new CaptionBroadcastServer(session, 0);
                using var store = new CaptionRecoveryStore(Path.Combine(directory.FullName, "CaptionRecovery"));
                if (recoverable) await store.SaveAsync(new CaptionRecovery(1, 1, host.PlanName, "지난 초안", null, 1, "", [], [], []));
                using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
                if (start == ObsStart.BeforeTheTab)
                {
                    using (lockWhileStarting ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None) : null)
                        await obs.StartAsync();
                }
                var console = new CaptionConsole(host, session, server, store, obs, () => [FakeCaptionHost.Screen]);
                if (start == ObsStart.AfterTheTab) _ = obs.StartAsync();
                DockPanel.SetDock(console.Strip, Dock.Top);
                var window = new Window
                {
                    ShowActivated = false, ShowInTaskbar = false, Width = 720, Height = 960,
                    Content = new DockPanel { Children = { console.Strip, console } },
                };
                console.Strip.CaptionTab.IsChecked = true;
                try
                {
                    window.Show();
                    await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    await check(console, obs, path, profile);
                }
                finally
                {
                    console.EndSession();
                    window.Close();
                }
            }
            finally { directory.Delete(true); }
        });

    // a050 design 3 (the spec's scenario of PowerPoint not answering): the host's step of the output fails; the tab says
    // so on its status line and logs it, nothing reaches the dispatcher. A cancelled step ends quietly.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task AStepTheHostFailsIsToldAndLoggedAndEndsNothing(bool cancelled) => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var unhandled = new List<Exception>();
        console.Dispatcher.UnhandledException += (_, e) => { unhandled.Add(e.Exception); e.Handled = true; };
        host.Step = async _ =>
        {
            await Task.Yield();
            throw cancelled ? new OperationCanceledException() : new TimeoutException("PowerPoint did not answer");
        };
        session.SyncShow = true;
        console.Status.Text = "";
        typeof(CaptionConsole).GetMethod("Step", Private)!.Invoke(console, [1]);
        await Pump(console, 300);
        Assert.Empty(unhandled);
        if (cancelled)
        {
            Assert.Equal(("", 0), (console.Status.Text, host.Errors.Count));
            return;
        }
        Assert.Equal(CaptionText.Format("CaptionConsole", "StepFailed", "PowerPoint did not answer"), console.Status.Text);
        Assert.Equal(["step caption output"], host.Errors);
    });

    // An exception the dispatcher sees while the exercise runs (none must reach it).
    private static List<Exception> Unhandled(CaptionConsole console)
    {
        var unhandled = new List<Exception>();
        console.Dispatcher.UnhandledException += (_, e) => { unhandled.Add(e.Exception); e.Handled = true; };
        return unhandled;
    }

    // a050 design 12: the host fails when asked whether it can step.
    [Fact]
    public Task AHostThatFailsToSayWhetherItCanStepIsToldAndLogged() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var unhandled = Unhandled(console);
        host.Throws = new InvalidOperationException("no output");
        session.SyncShow = true;
        typeof(CaptionConsole).GetMethod("Step", Private)!.Invoke(console, [1]);
        await Pump(console, 200);
        Assert.Empty(unhandled);
        Assert.Equal(CaptionText.Format("CaptionConsole", "StepFailed", "no output"), console.Status.Text);
        Assert.Equal(["step caption output"], host.Errors);
    });

    // a050 design 12: the host fails to put the picked row in its output panel.
    [Fact]
    public Task AHostThatFailsToSendThePickedRowIsToldAndLogged() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var unhandled = Unhandled(console);
        host.Throws = new InvalidOperationException("output panel busy");
        host.Picked = new("song:a", "찬양", CaptionItemKind.Text);
        host.Raise(CaptionHostChange.Picked);
        await Pump(console, 300);
        Assert.Empty(unhandled);
        Assert.Empty(host.Sent);
        Assert.Equal(CaptionText.Format("CaptionConsole", "SendPickedFailed", "output panel busy"), console.Status.Text);
        Assert.Equal(["send picked row to output"], host.Errors);
    });

    // a050 design 12: the host's reading of an old-format deck fails with anything (not only the kinds the tab knew); it is
    // a deck that could not be read: told and logged.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task AnOldFormatDeckTheHostFailsToReadIsToldAndLogged(bool cancelled) => FakeCaptionHost.WithConsole(async (console, session, host, directory) =>
    {
        var unhandled = Unhandled(console);
        Exception failure = cancelled ? new OperationCanceledException("PowerPoint gave up") : new System.Runtime.InteropServices.COMException("PowerPoint failed");
        host.ReadLegacyDeck = (_, _) => throw failure;
        host.Picked = new("ppt:old", "옛 덱", CaptionItemKind.PowerPoint, Path.Combine(directory.FullName, "옛.ppt"), 1); // (not told: nothing is sent)
        typeof(CaptionConsole).GetMethod("OpenLiveDeck", Private)!.Invoke(console, [null, null]);
        await Pump(console, 300);
        Assert.Empty(unhandled);
        Assert.Null(session.Deck);
        Assert.Equal(CaptionText.Format("CaptionConsole", "DeckReadFailed", failure.Message), console.Status.Text);
        Assert.Equal(["open caption PPT"], host.Errors);
    });

    // a050 design 20: a recovery whose old-format deck the host fails to read with anything (not only the kinds the tab
    // knew) goes on without the deck: the draft is recovered and the tab asks for the deck to be opened again.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ARecoveryWhoseDeckTheHostFailsToReadGoesOnWithoutIt(bool cancelled) => FakeCaptionHost.WithConsole(async (console, session, host, directory) =>
    {
        Exception failure = cancelled ? new OperationCanceledException("PowerPoint gave up") : new System.Runtime.InteropServices.COMException("PowerPoint failed");
        host.ReadLegacyDeck = (_, _) => throw failure;
        var deck = Path.Combine(directory.FullName, "옛.ppt");
        File.WriteAllText(deck, "");
        console.DirectMode.IsChecked = true;
        await console.RestoreAsync(new CaptionRecovery(1, 1, host.PlanName, "되돌린 초안", deck, 1, "", [], [], []));
        Assert.Equal("되돌린 초안", ((CaptionConsoleViewModel)console.DataContext).DraftText);
        Assert.Null(session.Deck);
        Assert.Equal(CaptionText.Format("CaptionConsole", "ReopenDeck", failure.Message), console.Status.Text);
        Assert.Equal(["recover caption PPT"], host.Errors);
    });

    private sealed class FailingCapture : ICaptionQueueRecovery
    {
        public (JsonElement[] Items, int Selected) Capture() => throw new InvalidOperationException("worship list busy");
        public void Restore(IReadOnlyList<JsonElement> items, string plan, int selected) { }
        public event Action? Replacing { add { } remove { } }
    }

    // a050 design 12: the host fails to give its worship order for the recovery: told and logged, the recovery is still to
    // be saved (tried again on the next change), no revision is used up.
    [Fact]
    public Task AWorshipOrderTheHostFailsToGiveForTheRecoveryIsToldAndLogged() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var unhandled = Unhandled(console);
        var revision = (long)typeof(CaptionConsole).GetField("_revision", Private)!.GetValue(console)!;
        typeof(CaptionConsole).GetMethod("Changed", Private)!.Invoke(console, null);
        Assert.True(await Until(console, () => host.Errors.Count > 0, 3000)); // the save after the debounce
        await Pump(console, 100);
        Assert.Empty(unhandled);
        Assert.Equal(["capture caption recovery"], host.Errors);
        Assert.Equal(CaptionText.Format("CaptionConsole", "RecoverySaveFailed", "worship list busy"), console.Status.Text);
        Assert.Equal((true, revision), ((bool)typeof(CaptionConsole).GetField("_dirty", Private)!.GetValue(console)!,
            (long)typeof(CaptionConsole).GetField("_revision", Private)!.GetValue(console)!));
    }, setUp: host => host.Recovery = new FailingCapture());
}

/// <summary>a050 design 4 and 5: the caption session's thread, the slide's text boxes.</summary>
public sealed class CaptionReviewFindingsTests
{
    private static CaptionDeck Deck(string path, params string[] slides) =>
        new(path, [.. slides.Select(text => new CaptionSlide([new(CaptionDeck.AllTextKey, "all", text)]))]);

    // a050 design 4: ICaptionLiveSource.Changed may come from another thread; the session changes and tells its changes
    // on the thread it was made on, in the order they were told.
    [Fact]
    public Task ALiveChangeFromAnotherThreadIsHandledOnTheSessionsThreadInOrder() => StaHelper.RunOnStaAsync(async () =>
    {
        var live = new FakeCaptionHost.FakeLive();
        using var session = new CaptionSession(live);
        session.Prepare(Deck("deck.pptx", "일장", "이장", "삼장", "사장"));
        session.Follow(1, CaptionDeck.AllTextKey);
        session.SetVisible(true);
        var ui = Thread.CurrentThread;
        var told = new List<(Thread Thread, int Slide, string Text)>();
        session.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CaptionSession.CurrentSlide)) told.Add((Thread.CurrentThread, session.CurrentSlide, ""));
        };
        session.PublishedChanged += published => told.Add((Thread.CurrentThread, 0, published.Text));
        await Task.Run(() =>
        {
            live.Set(CaptionLiveState.Active, "deck.pptx", 2);
            live.Set(CaptionLiveState.Active, "deck.pptx", 3);
        });
        live.Set(CaptionLiveState.Active, "deck.pptx", 4); // on the session's own thread, told after the two above
        for (int i = 0; i < 100 && told.Count(t => t.Slide > 0) < 3; i++) await Task.Delay(20);
        Assert.All(told, t => Assert.Same(ui, t.Thread));
        Assert.Equal([2, 3, 4], told.Where(t => t.Slide > 0).Select(t => t.Slide));
        Assert.Equal(["이장", "삼장", "사장"], told.Where(t => t.Slide == 0).Select(t => t.Text));
    });

    // a050 design 14: a change told on another thread while the session's thread is busy waits for it; a change told on the
    // session's thread meanwhile waits behind it (not handled at once), and both are handled in the order told.
    [Fact]
    public Task ALiveChangeOnTheSessionsThreadWaitsBehindOneQueuedFromAnotherThread() => StaHelper.RunOnStaAsync(async () =>
    {
        var live = new FakeCaptionHost.FakeLive();
        using var session = new CaptionSession(live);
        session.Prepare(Deck("deck.pptx", "일장", "이장", "삼장"));
        session.Follow(1, CaptionDeck.AllTextKey);
        var slides = new List<int>();
        session.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CaptionSession.CurrentSlide)) slides.Add(session.CurrentSlide); };
        // (the session's thread is busy here: it waits for the other thread, which queues its change)
        Task.Run(() => live.Set(CaptionLiveState.Active, "deck.pptx", 2)).Wait();
        live.Set(CaptionLiveState.Active, "deck.pptx", 3);
        Assert.Empty(slides); // the second waits behind the first
        await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal([2, 3], slides);
        Assert.Equal(3, session.CurrentSlide);
    });

    // a050 design 16: a session made on a thread with no WPF object yet (a WinForms host's UI thread) still handles a
    // change told on another thread on its own thread, when that thread pumps; one told on its own thread at once.
    [Fact]
    public void ASessionMadeBeforeAnyWpfObjectStillHandlesChangesOnItsThread() => StaHelper.RunOnSta(() =>
    {
        var live = new FakeCaptionHost.FakeLive();
        using var session = new CaptionSession(live); // the first WPF object of the thread
        session.Prepare(Deck("deck.pptx", "일장", "이장", "삼장"));
        session.Follow(1, CaptionDeck.AllTextKey);
        var threads = new List<Thread>();
        session.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CaptionSession.CurrentSlide)) threads.Add(Thread.CurrentThread); };
        Task.Run(() => live.Set(CaptionLiveState.Active, "deck.pptx", 2)).Wait();
        Assert.Empty(threads); // waits for the thread to pump
        var pumped = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => pumped.Continue = false));
        Dispatcher.PushFrame(pumped);
        Assert.Equal([Thread.CurrentThread], threads);
        live.Set(CaptionLiveState.Active, "deck.pptx", 3);
        Assert.Equal((2, 3), (threads.Count, session.CurrentSlide)); // at once
    });

    private static string Box(string? name, long y, string text, bool hidden = false) =>
        $"""<p:sp><p:nvSpPr><p:cNvPr id="9"{(name == null ? "" : $" name=\"{name}\"")}{(hidden ? " hidden=\"1\"" : "")}/><p:cNvSpPr/><p:nvPr/></p:nvSpPr><p:spPr><a:xfrm><a:off x="600000" y="{y}"/><a:ext cx="8000000" cy="600000"/></a:xfrm></p:spPr><p:txBody><a:bodyPr/><a:p><a:r><a:t>{text}</a:t></a:r></a:p></p:txBody></p:sp>""";

    private static string Alternate(string choice, string fallback) => $"""<mc:AlternateContent><mc:Choice Requires="a14">{choice}</mc:Choice><mc:Fallback>{fallback}</mc:Fallback></mc:AlternateContent>""";

    private static string HiddenGroup(string shape) =>
        $"""<p:grpSp><p:nvGrpSpPr><p:cNvPr id="3" name="g" hidden="1"/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/>{shape}</p:grpSp>""";

    private static CaptionDeck Read(string folder, string shapes) =>
        CaptionDeck.Read(CaptionAllTextTests.WriteDeck(folder, [], $"<p:spTree>{shapes}</p:spTree>"));

    private static string[] Keys(string folder, string shapes) => [.. Read(folder, shapes).Slides[0].Boxes.Select(box => box.Key)];

    private static string Named(int n) => "shape:" + CaptionText.Format("CaptionConsole", "BoxName", n);

    // a050 design 11 (the spec's scenarios): a box in mc:AlternateContent is listed once; a box only in an mc:Fallback, a
    // hidden box and one in a hidden group stay listed; the keys are as they were, also a box without a name (numbered
    // among the slide's text boxes, the Fallback copy counted).
    [Fact]
    public void TheBoxListHasEachBoxOnceAndKeepsTheKeys() => StaHelper.RunOnSta(() =>
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            string formula = Box("수식", 1500000, "수식 글");
            string shapes = Box("본문", 300000, "본문") + Alternate(formula, formula) + Box("숨김", 2500000, "숨긴 글", hidden: true)
                + HiddenGroup(Box("그룹 안", 3000000, "숨긴 그룹 글")) + Alternate(Box("새 수식", 3200000, "새 글"), Box("옛 수식", 3200000, "옛 글"))
                + Box(null, 3500000, "이름 없는 글");
            Assert.Equal([CaptionDeck.AllTextKey, "shape:본문", "shape:수식", "shape:숨김", "shape:그룹 안", "shape:새 수식", "shape:옛 수식", Named(8)],
                Keys(folder.FullName, shapes));
        }
        finally { folder.Delete(true); }
    });

    // The spec's scenario of a deck whose hidden box is the caption: the operator's choice of it still gives its text
    // (the whole text leaves it out, as before).
    [Fact]
    public void AHiddenBoxChosenAsTheCaptionStillGivesItsText() => StaHelper.RunOnSta(() =>
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            var deck = Read(folder.FullName, Box("본문", 300000, "본문") + Box("자막", 2500000, "자막 전용 글", hidden: true));
            Assert.Equal("본문", deck.Slides[0].Boxes[0].Text);
            using var session = new CaptionSession(new FakeCaptionHost.FakeLive());
            session.Prepare(deck);
            session.Follow(1, "shape:자막");
            Assert.Equal("자막 전용 글", session.ReadyText);
        }
        finally { folder.Delete(true); }
    });

    // A deck with none of those: the list is as it was.
    [Fact]
    public void ABoxListWithoutAlternateOrHiddenShapesIsAsBefore() => StaHelper.RunOnSta(() =>
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            string shapes = Box("제목", 300000, "제목") + Box(null, 1500000, "이름 없는 글") + Box("가사 1", 2500000, "가사");
            Assert.Equal([CaptionDeck.AllTextKey, "shape:제목", Named(2), "shape:가사 1"], Keys(folder.FullName, shapes));
        }
        finally { folder.Delete(true); }
    });
}

/// <summary>a050 design 25: the WPF app's send of the picked row, as the caption tab awaits it.</summary>
public partial class MainViewModelTests
{
    // An output panel whose render throws (as a PowerPoint failure the render service did not turn into a result would).
    private sealed class ThrowingPowerPointRenderService : Axicultus.Wpf.Rendering.IPowerPointRenderService
    {
        public Task<Axicultus.Wpf.Rendering.PowerPointRenderResult> RenderSlideAsync(Axicultus.Wpf.Rendering.PowerPointRenderRequest request,
            CancellationToken cancellationToken = default) => Task.FromException<Axicultus.Wpf.Rendering.PowerPointRenderResult>(new InvalidOperationException("render broke"));

        public void ClearCache() { }
    }

    private static (MainViewModelCaptionHost Host, MainViewModel Main) DeckPicked(Axicultus.Wpf.Rendering.IPowerPointRenderService outputRender)
    {
        var main = CreateSut(outputPowerPoint: new Axicultus.Wpf.Rendering.PowerPointPreviewViewModel(outputRender, _ => Frozen()), seedSampleQueue: false);
        var deck = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };
        main.LoadQueue([deck]);
        main.SelectedItem = deck;
        return (new MainViewModelCaptionHost(main, null, null), main);
    }

    // The send draws the deck in the output panel: its task ends when the panel has it, not before.
    [Fact]
    public async Task TheAppsSendOfThePickedRowEndsWhenTheRowIsInTheOutputPanel()
    {
        using var gate = new GatedPowerPointRenderService();
        var (host, main) = DeckPicked(gate);
        var sending = host.SendPickedAsync();
        await Task.Delay(100);
        Assert.False(sending.IsCompleted); // the output panel's render is held back
        gate.Release();
        await sending.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("ppt:1", main.OutputItem?.Id);
    }

    // A failure in the send ends its task with the cause (the tab tells and logs it).
    [Fact]
    public async Task TheAppsSendOfThePickedRowFailsWithTheCause()
    {
        var (host, _) = DeckPicked(new ThrowingPowerPointRenderService());
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => host.SendPickedAsync().WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal("render broke", failure.Message);
    }
}