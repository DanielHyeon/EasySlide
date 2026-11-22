using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// The native PowerPoint slide show against the live session: what the audience must see after Black, Clear, item
// changes and quick keys. (Written by the adversarial review of the native show, wV1; each failed before its fix.)
public partial class MainViewModelTests
{
    // A native show whose start and step can be held open, as a PowerPoint that takes its time.
    private sealed class HeldPowerPointShow : IPowerPointSlideShowControl
    {
        public TaskCompletionSource? StartGate { get; set; }
        public TaskCompletionSource<int?>? AdvanceGate { get; set; }
        public bool Shown { get; set; }
        public int? ActualSlide { get; set; } = 1;
        public int AdvanceCount { get; private set; }
        public int RetreatCount { get; private set; }
        public int StopCount { get; private set; }
        public List<bool> Blackouts { get; } = new();
        public List<int> StartSlides { get; } = new();
        public List<string> StartPaths { get; } = new();
        // The start fails after PowerPoint opened the show: nothing is reported as shown.
        public bool FailStart { get; set; }
        // PowerPoint refuses this many closes (a modal dialog is open in it).
        public int RefusedStops { get; set; }

        public bool HasOutputShow => Shown;
        public Task<int?> ReadOutputPositionAsync(string path) => Task.FromResult(Shown ? ActualSlide : null);

        public async Task StartAsync(PowerPointSlideShowRequest request, string outputMonitorName, CancellationToken cancellationToken = default)
        {
            StartSlides.Add(request.SlideNumber);
            StartPaths.Add(request.FilePath);
            if (StartGate is { } gate) await gate.Task.ConfigureAwait(true);
            if (FailStart) throw new TimeoutException("PowerPoint did not answer.");
            Shown = true;
            ActualSlide = request.SlideNumber;
        }

        public Task TriggerNextAsync(PowerPointSlideShowRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            StopCount++;
            if (RefusedStops > 0)
            {
                RefusedStops--;
                return Task.FromException(new InvalidOperationException("PowerPoint is busy"));
            }
            Shown = false;
            return Task.CompletedTask;
        }

        public Task SetBlackoutAsync(bool black)
        {
            Blackouts.Add(black);
            return Task.CompletedTask;
        }

        public Task<int?> AdvanceOutputAsync(string path)
        {
            AdvanceCount++;
            if (AdvanceGate is { } gate) return gate.Task;
            return Task.FromResult(Shown ? ActualSlide : null);
        }

        public Task<int?> RetreatOutputAsync(string path)
        {
            RetreatCount++;
            return Task.FromResult(Shown ? ActualSlide : null);
        }
    }

    private (MainViewModel Sut, HeldPowerPointShow Show, FixedSlideCountPowerPointRenderService Renderer, LiveQueueItem Deck, LiveQueueItem Song)
        CreateLiveDeckSut(TempSettingsFolder folder, bool advanceNextItem, bool songFirst = false, int? failSlide = null)
    {
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, advanceNextItem).Succeeded.Should().BeTrue();
        string path = Path.Combine(folder.Root, "service.pptx");
        File.WriteAllText(path, "slides");
        var deck = new LiveQueueItem("deck", "Service", LiveItemKinds.PowerPoint) { ContentPath = path };
        var song = new LiveQueueItem("song", "Closing", LiveItemKinds.Song) { Lyrics = "one\n\ntwo" };
        var show = new HeldPowerPointShow();
        var renderer = new FixedSlideCountPowerPointRenderService(3) { FailSlide = failSlide };
        var sut = CreateSut(settings: settings, seedSampleQueue: false, powerPointSlideShow: show,
            powerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()),
            outputPowerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()));
        sut.LoadQueue(songFirst ? [song, deck] : [deck, song]);
        sut.SelectedItem = deck;
        return (sut, show, renderer, deck, song);
    }

    // Black, move on to the song, un-Black: the usual way to change items. The blacked native show is a
    // full-screen window over the audience window, so it has to go (or be un-blacked) when the song comes back.
    [Fact]
    public async Task UnblackAfterLeavingTheDeckUnderBlack_ClosesTheBlackedShowOverTheSong()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, deck, song) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.Shown.Should().BeTrue();
        sut.Session.Current.CurrentItemId.Should().Be(deck.Id);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        show.Blackouts.Should().Equal(true);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentItemId.Should().Be(song.Id, "the operator sees the song as live");
        (show.StopCount > 0 || (show.Blackouts.Count > 1 && !show.Blackouts[^1])).Should().BeTrue(
            "the native show is still up and still black: the audience sees black instead of the song " +
            $"(stops {show.StopCount}, blackouts [{string.Join(",", show.Blackouts)}])");
    }

    // The same under Clear, which leaves the deck showing: the song that follows must not stay behind the deck.
    [Fact]
    public async Task LeavingTheDeckUnderClear_ClosesTheDeckShowOverTheSong()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, song) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.ToggleOutputClearCommand.ExecuteAsync(null);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        await sut.ToggleOutputClearCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentItemId.Should().Be(song.Id, "the operator sees the song as live");
        show.Shown.Should().BeFalse("the deck's full-screen show is still up over the audience window");
    }

    // The native show is gone (PowerPoint was closed or killed; ForgetPowerPoint): Space, the documented
    // "next slide" key, must bring the deck back as the Next key does, or at least say why nothing happens.
    [Fact]
    public async Task SpaceAfterTheNativeShowIsLost_RestartsTheShowOrReports()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, _) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        var keys = new Axicultus.Wpf.Input.ShortcutRegistry();
        sut.BindShortcuts(keys);
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.Shown = false;
        int starts = show.StartSlides.Count;
        string status = sut.StatusText;

        keys.TryInvoke(MainCommandIds.OutputSpace).Should().BeTrue();

        (show.StartSlides.Count > starts || sut.StatusText != status).Should().BeTrue(
            $"Space did nothing and said nothing (advances {show.AdvanceCount}, status still '{sut.StatusText}')");
    }

    // Black, Next, un-Black: the deck must come back on the slide the operator moved to.
    [Fact]
    public async Task UnblackAfterNextSlideUnderBlack_ShowsTheSlideTheOperatorMovedTo()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, _) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.LiveNextShortcutCommand.ExecuteAsync(null);
        sut.Session.Current.PptSlideNumber.Should().Be(2, "the operator moved to slide 2 under Black");
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Active);

        // The position timer (MainWindow, _powerPointPositionTimer) then reads the native show.
        await sut.SyncNativePowerPointPositionAsync();
        (show.AdvanceCount > 0 || show.StartSlides[^1] == 2).Should().BeTrue(
            $"the native show was never moved: it is revealed on slide {show.ActualSlide}");
        sut.Session.Current.PptSlideNumber.Should().Be(2, "and the poll must not take the operator back");
    }

    // Black pressed while PowerPoint is still starting the show (seconds on a cold start).
    [Fact]
    public async Task BlackWhileTheShowIsStillStarting_BlacksTheShowOnceItIsUp()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, _) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        show.StartGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var goLive = sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Active, "the session is live while PowerPoint starts");
        show.Shown.Should().BeFalse();
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.IsOutputBlackActive.Should().BeTrue();

        show.StartGate.SetResult();
        await goLive;
        show.Shown.Should().BeTrue();
        sut.IsOutputBlackActive.Should().BeTrue("the operator still has Black on");
        show.Blackouts.Should().Contain(true, "the show that came up afterwards covers the black audience window");
    }

    // The Next key twice in quick succession (two click animations), the first step still on its way.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SecondNextKeyWhileTheFirstStepIsPending_StepsTheDeckAgain(bool advanceNextItem)
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, deck, _) = CreateLiveDeckSut(folder, advanceNextItem);
        using var _sut = sut;
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.AdvanceGate = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);

        // As ShortcutRegistry's LiveNext binding: "_ = LiveNextShortcutCommand.ExecuteAsync(null)", no gate.
        var first = sut.LiveNextShortcutCommand.ExecuteAsync(null);
        var second = sut.LiveNextShortcutCommand.ExecuteAsync(null);
        show.ActualSlide = 2;
        show.AdvanceGate.SetResult(2);
        await Task.WhenAll(first, second);

        sut.Session.Current.CurrentItemId.Should().Be(deck.Id, "the second press belongs to the deck, not to the next item");
        show.AdvanceCount.Should().Be(2, $"the second press was dropped (status: {sut.StatusText})");
    }

    // The slide picture of the stepped slide could not be rendered (worker timed out): the native show still runs.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviousKeyAfterAFailedSlideRender_StepsTheNativeShowBack(bool advanceNextItem)
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, renderer, deck, _) = CreateLiveDeckSut(folder, advanceNextItem, songFirst: true, failSlide: 2);
        using var _sut = sut;
        // Slide 2 cannot be rendered at all (neither its picture nor its thumbnail): renderer.FailSlide == 2.
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemId.Should().Be(deck.Id);

        show.ActualSlide = 2;
        await sut.LiveNextShortcutCommand.ExecuteAsync(null);
        show.AdvanceCount.Should().Be(1);
        sut.Session.Current.PptSlideNumber.Should().Be(2, "the live deck is on slide 2");
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Failed, $"slide {sut.OutputPowerPoint.SlideNumber}/{sut.OutputPowerPoint.SlideCount} status '{sut.OutputPowerPoint.StatusText}' failedRenders {renderer.FailedRenders} thumbs [{string.Join(",", System.Linq.Enumerable.Select(sut.OutputPowerPoint.Thumbnails, t => t.SlideNumber))}]");

        show.ActualSlide = 1;
        await sut.LivePreviousShortcutCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemId.Should().Be(deck.Id, "Previous on slide 2 stays in the deck");
        show.RetreatCount.Should().Be(1, $"the native show was not stepped back (status: {sut.StatusText})");
    }

    // Black, move on to another deck, un-Black: the first deck's show must not be the one that is revealed.
    [Fact]
    public async Task UnblackAfterMovingToAnotherDeckUnderBlack_StartsThatDeck()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, deck, _) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        string otherPath = Path.Combine(folder.Root, "other.pptx");
        File.WriteAllText(otherPath, "slides");
        var other = new LiveQueueItem("deck2", "Other", LiveItemKinds.PowerPoint) { ContentPath = otherPath };
        sut.LoadQueue([deck, other]);
        sut.SelectedItem = deck;
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        show.StartPaths.Should().HaveCount(1, "a deck started under Black would flash on the audience screen");
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemId.Should().Be(other.Id);
        show.StartPaths[^1].Should().Be(otherPath);
        show.Blackouts[^1].Should().BeFalse();
    }

    // The start was given up after PowerPoint had opened the show: the control reports no show, yet one may be
    // on the audience screen when the next item is a song.
    [Fact]
    public async Task SongAfterADeckWhoseShowStartFailed_StillAsksPowerPointToCloseTheShow()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, song) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        show.FailStart = true;
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.Shown.Should().BeFalse();

        await sut.NextOutputItemCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemId.Should().Be(song.Id);
        show.StopCount.Should().Be(1);
    }

    // PowerPoint refused the close (a modal dialog is open in it): End Show must be available again.
    [Fact]
    public async Task EndShowPressedAgainAfterPowerPointRefusedToClose_ClosesTheShow()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, _) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        var keys = new Axicultus.Wpf.Input.ShortcutRegistry();
        sut.BindShortcuts(keys);
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.RefusedStops = 1;

        keys.TryInvoke(MainCommandIds.EndShow).Should().BeTrue();
        sut.IsShowRunning.Should().BeFalse();
        sut.StatusText.Should().Contain("PowerPoint 종료 실패");
        show.Shown.Should().BeTrue("PowerPoint did not close the show");

        keys.TryInvoke(MainCommandIds.EndShow).Should().BeTrue("the show is still on the audience screen");
        show.Shown.Should().BeFalse();
        keys.TryInvoke(MainCommandIds.EndShow).Should().BeFalse();
    }

    // A key that cannot act soon is dropped: a slide change that arrives seconds late is worse than none.
    [Fact]
    public async Task NextKeyWhileAnEarlierStepGetsNoAnswer_IsDroppedInsteadOfActingLate()
    {
        using var folder = TempSettingsFolder.Create();
        var (sut, show, _, _, _) = CreateLiveDeckSut(folder, advanceNextItem: false);
        using var _sut = sut;
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.AdvanceGate = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var first = sut.LiveNextShortcutCommand.ExecuteAsync(null);
        var second = sut.LiveNextShortcutCommand.ExecuteAsync(null);
        await second.WaitAsync(TimeSpan.FromSeconds(10));
        show.AdvanceCount.Should().Be(1);
        sut.StatusText.Should().Contain("건너뜁니다");

        show.ActualSlide = 2;
        show.AdvanceGate.SetResult(2);
        await first;
        show.AdvanceCount.Should().Be(1, "the dropped key does not act once PowerPoint answers");
    }
}
