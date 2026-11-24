using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a042: ending the show (Esc, live stop) keeps the output panel's deck, as WinForms GoLive(false) does.
public partial class MainViewModelTests
{
    // A deck whose slide count can change (a slide past it fails to render, as PowerPoint's export does) and whose
    // renders can be held.
    private sealed class ChangingDeckRender : IPowerPointRenderService
    {
        public int Count { get; set; } = 3;
        public TaskCompletionSource? Gate { get; set; }
        public Func<PowerPointRenderRequest, Task?>? Hold { get; set; } // holds the renders it gives a task for
        public List<PowerPointRenderRequest> Requests { get; } = new();

        public async Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (Gate is { } gate) await gate.Task;
            if (Hold?.Invoke(request) is { } hold) await hold;
            if (request.SlideNumber > Count)
                return new PowerPointRenderResult(PowerPointRenderErrorKind.CorruptFile, null, "Slide number exceeds slide count", false, TimeSpan.Zero);
            return new PowerPointRenderResult(PowerPointRenderErrorKind.None,
                new PowerPointSlideSnapshot(request.FilePath, request.SlideNumber, Count, request.PixelWidth, request.PixelHeight,
                    [1, 2, 3], "image/jpeg", DateTimeOffset.UnixEpoch), null, false, TimeSpan.Zero);
        }

        public void ClearCache() { }
    }

    // A native show whose close can be held, as a PowerPoint that takes its time to end the show.
    private sealed class HeldStopShow : IPowerPointSlideShowControl
    {
        public List<int> Starts { get; } = new();
        public List<string> StartPaths { get; } = new();
        public TaskCompletionSource? StopGate { get; set; }
        public Task StartAsync(PowerPointSlideShowRequest request, string outputMonitorName, CancellationToken cancellationToken = default)
        {
            Starts.Add(request.SlideNumber);
            StartPaths.Add(request.FilePath);
            return Task.CompletedTask;
        }
        public Task TriggerNextAsync(PowerPointSlideShowRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken = default) => StopGate?.Task ?? Task.CompletedTask;
        public int Steps { get; private set; }
        public Task<int?> AdvanceOutputAsync(string path) { Steps++; return Task.FromResult<int?>(null); }
        public Task<int?> RetreatOutputAsync(string path) { Steps++; return Task.FromResult<int?>(null); }
    }

    private static (MainViewModel Sut, ShortcutRegistry Keys, LiveQueueItem Deck) DeckSut(IPowerPointSlideShowControl show,
        IPowerPointRenderService? render = null, IPowerPointRenderService? previewRender = null, string deckPath = "deck.pptx",
        ISettingsService? settings = null, LiveQueueItem? second = null)
    {
        render ??= new SuccessPowerPointRenderService();
        var sut = CreateSut(
            settings: settings,
            seedSampleQueue: false,
            powerPoint: new PowerPointPreviewViewModel(previewRender ?? render, _ => Frozen()),
            outputPowerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()),
            powerPointSlideShow: show,
            worshipValidator: new WorshipListValidator(_ => true)); // the stand-in deck file may not exist
        var deck = new LiveQueueItem("ppt:deck", "Deck", LiveItemKinds.PowerPoint) { ContentPath = deckPath };
        sut.LoadQueue([deck, second ?? new LiveQueueItem("song:next", "Next song", LiveItemKinds.Song) { Lyrics = "[1]\nnext" }]);
        var keys = new ShortcutRegistry();
        sut.BindShortcuts(keys);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = deck;
        return (sut, keys, deck);
    }

    private (MainViewModel Sut, RecordingPowerPointSlideShowControl Show, ShortcutRegistry Keys, LiveQueueItem Deck) DeckSentWithF11(
        ChangingDeckRender? render = null, string deckPath = "deck.pptx", ISettingsService? settings = null)
    {
        var show = new RecordingPowerPointSlideShowControl();
        var (sut, keys, deck) = DeckSut(show, render, deckPath: deckPath, settings: settings);
        return (sut, show, keys, deck);
    }

    private static bool PressEscape(ShortcutRegistry keys) => keys.TryHandleGlobal(System.Windows.Forms.Keys.Escape, System.Windows.Forms.Keys.None);

    [Fact]
    public async Task EscAfterF11_KeepsTheOutputDeckThumbnailsAndSlide()
    {
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(2);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);

        PressEscape(keys).Should().BeTrue();

        sut.Session.Current.State.Should().Be(LiveState.Off);
        sut.OutputItem!.ContentPath.Should().Be(deck.ContentPath);
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
        sut.OutputPowerPoint.LoadedContentPath.Should().Be("deck.pptx");
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3, "WinForms keeps flowLayoutOutputPowerPoint when the show ends");
        sut.OutputPowerPoint.SlideNumber.Should().Be(2);
        sut.OutputPowerPoint.PreviewImage.Should().NotBeNull();
        sut.IsOutputPowerPointContext.Should().BeTrue();
    }

    [Fact]
    public async Task AfterEsc_ASlidePickedInTheKeptThumbnailsIsTheOneSentAgain()
    {
        var (sut, show, keys, _) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        sut.SelectedItem = sut.Queue[0]; // the preview shows the same deck (on slide 1) again

        sut.GoToOutputSlideCommand.CanExecute(3).Should().BeTrue();
        await sut.GoToOutputSlideCommand.ExecuteAsync(3);
        sut.Session.Current.State.Should().Be(LiveState.Off, "picking a slide while not live only prepares it");
        sut.OutputPowerPoint.SlideNumber.Should().Be(3);
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.PptPath.Should().Be("deck.pptx");
        sut.Session.Current.PptSlideNumber.Should().Be(3);
        show.StartRequests.Last().Request.Should().Be(new PowerPointSlideShowRequest("deck.pptx", 3, PowerPointSlideShowTarget.Output));
        sut.OutputPowerPoint.SlideNumber.Should().Be(3);
    }

    [Fact]
    public async Task ClosingTheOutputWindowAfterEscEmptiesTheOutputPanel()
    {
        var (sut, _, keys, _) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3, "kept after the show ended");
        if (!sut.CloseOutputCommand.CanExecute(null)) sut.OpenOutputCommand.Execute(null); // Esc closes a full-screen output

        await sut.CloseOutputCommand.ExecuteAsync(null);

        sut.OutputItem.Should().BeNull();
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Idle);
        sut.OutputPowerPoint.Thumbnails.Should().BeEmpty();
        sut.OutputPowerPoint.SlideNumber.Should().Be(0);
        sut.IsOutputPowerPointContext.Should().BeFalse();
    }

    [Fact]
    public async Task DuringTheShow_AnItemOpenedAgainFromTheGapScreenOrRestartedStartsOnItsFirstSlide()
    {
        // FrmMain MoveToOutputItem reopens a deck on its first slide; only a start from the output panel while no
        // show runs keeps the panel's slide.
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.Default).Succeeded.Should().BeTrue();
        var (sut, show, _, _) = DeckSentWithF11(settings: settings);
        await sut.GoLiveCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(3);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue();

        await sut.PreviousOutputItemCommand.ExecuteAsync(null);

        sut.IsGapItemOnDisplay.Should().BeFalse();
        sut.Session.Current.PptSlideNumber.Should().Be(1);
        show.StartRequests.Last().Request.SlideNumber.Should().Be(1);

        await sut.GoToOutputSlideCommand.ExecuteAsync(3);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue();
        await sut.RestartCurrentItemCommand.ExecuteAsync(null); // F5 on the gap screen reopens the item it follows

        sut.Session.Current.PptSlideNumber.Should().Be(1);
        show.StartRequests.Last().Request.SlideNumber.Should().Be(1);
    }

    [Fact]
    public async Task AfterEsc_AnEmptiedWorshipListAndItsUndoLeaveTheOutputPanel()
    {
        // FrmMain: an emptied list leaves the output as it is.
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();

        sut.ClearWorshipListCommand.Execute(null);
        sut.OutputItem!.Id.Should().Be(deck.Id);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);

        sut.RestoreClearedWorshipListCommand.Execute(null);
        sut.Queue.Should().HaveCount(2);
        sut.OutputItem!.Id.Should().Be(deck.Id);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
        sut.OutputPowerPoint.LoadedContentPath.Should().Be("deck.pptx");
    }

    [Fact]
    public async Task AWorshipListEmptiedDuringTheShowEndsItAndLeavesTheOutputPanel()
    {
        var (sut, _, _, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);

        sut.ClearWorshipListCommand.Execute(null);

        sut.IsShowRunning.Should().BeFalse();
        sut.OutputItem!.Id.Should().Be(deck.Id);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
    }

    [Fact]
    public async Task AfterEsc_AnotherWorshipListEmptiesTheOutputPanel()
    {
        var (sut, _, keys, _) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();

        var other = new LiveQueueItem("song:new", "New list song", LiveItemKinds.Song) { Lyrics = "[1]\nnew" };
        sut.LoadQueue([other]);

        sut.OutputItem.Should().BeNull();
        sut.OutputPowerPoint.Thumbnails.Should().BeEmpty();
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemId.Should().Be(other.Id, "a start from the output panel takes the new list, not the old deck");
    }

    [Fact]
    public async Task AfterEsc_ALoadedListWithTheSameItemGivesTheOutputPanelItsNewRow_AtTheKeptSlide()
    {
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(2);
        PressEscape(keys).Should().BeTrue();

        var reloaded = deck with { Title = "Deck (new list)" };
        sut.LoadQueue([reloaded, new LiveQueueItem("song:other", "Other", LiveItemKinds.Song) { Lyrics = "[1]\nother" }]);

        sut.OutputItem!.Title.Should().Be("Deck (new list)", "the new list's row, not the old list's");
        sut.OutputPowerPoint.SlideNumber.Should().Be(2);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemTitle.Should().Be("Deck (new list)");
        sut.Session.Current.PptSlideNumber.Should().Be(2);
    }

    [Fact]
    public async Task AfterEsc_ALoadedListWithTheSameSongGivesTheOutputPanelItsNewWords()
    {
        var song = new LiveQueueItem("song:7", "Song", LiveItemKinds.Song) { Lyrics = "[1]\nold words" };
        var (sut, keys, _) = DeckSut(new RecordingPowerPointSlideShowControl(), second: song);
        sut.SelectedItem = song;
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        sut.OutputItem!.Lyrics.Should().Contain("old words");

        sut.LoadQueue([song with { Lyrics = "[1]\nnew words" }]);

        sut.OutputItem!.Lyrics.Should().Contain("new words");
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemBodyText.Should().Contain("new words");
    }

    [Fact]
    public async Task AWorshipListLoadedDuringTheShowLeavesTheShow_AndTheOutputPanelFollowsItWhenTheShowEnds()
    {
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);

        sut.LoadQueue([new LiveQueueItem("song:new", "New list song", LiveItemKinds.Song) { Lyrics = "[1]\nnew" }]);
        sut.IsShowRunning.Should().BeTrue();
        sut.OutputItem!.Id.Should().Be(deck.Id, "the running show keeps its item");

        PressEscape(keys).Should().BeTrue();

        sut.OutputItem.Should().BeNull();
        sut.OutputPowerPoint.Thumbnails.Should().BeEmpty();
    }

    [Fact]
    public async Task AfterEsc_ADeckChangedOnDiskIsReadAgainBeforeItGoesLive()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, show, keys, _) = DeckSentWithF11(render, path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(3);
        PressEscape(keys).Should().BeTrue();

        File.WriteAllText(path, "now only two slides"); // saved again with a slide fewer
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
        render.Count = 2;
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.OutputPowerPoint.SlideCount.Should().Be(2);
        sut.OutputPowerPoint.SlideNumber.Should().Be(2, "the kept slide 3 is gone; the last slide is the nearest");
        sut.Session.Current.PptSlideNumber.Should().Be(2);
        show.StartRequests.Last().Request.SlideNumber.Should().Be(2);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(2);
    }

    [Fact]
    public async Task AfterEsc_ADeckChangedOnDiskThatCannotBeDrawnIsPreparedAsAnyStart_NotSentOnTheKeptSlide()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var output = new ChangingDeckRender();
        var preview = new ChangingDeckRender();
        var show = new RecordingPowerPointSlideShowControl();
        var (sut, keys, deck) = DeckSut(show, output, preview, path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(3);
        PressEscape(keys).Should().BeTrue();

        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
        output.Count = 0; // the output panel cannot draw the saved deck
        sut.SelectedItem = deck; // the preview shows the saved deck on its first slide
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
        sut.Session.Current.PptSlideNumber.Should().Be(sut.OutputPowerPoint.SlideNumber, "the slide that goes out is the one the panel shows");
        sut.Session.Current.PptSlideNumber.Should().Be(1);
        show.StartRequests.Last().Request.SlideNumber.Should().Be(1);
    }

    [Fact]
    public async Task Esc_DropsAnOutputSlideStillBeingDrawnForTheShow()
    {
        var render = new ChangingDeckRender();
        var (sut, _, keys, _) = DeckSentWithF11(render);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var turning = sut.GoToOutputSlideCommand.ExecuteAsync(2);
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Rendering);

        PressEscape(keys).Should().BeTrue();
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready, "the kept panel shows its slide, not a render that was dropped");
        render.Gate.SetResult();
        await turning;

        sut.OutputPowerPoint.SlideNumber.Should().Be(1, "the slide drawn for the show that ended does not turn the kept panel");
        sut.OutputItem!.SlideNumber.Should().NotBe(2);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
    }

    [Fact]
    public async Task Esc_WhileTheNextDeckIsStillDrawn_LeavesThatDeckDrawnInTheOutputPanel()
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.None).Succeeded.Should().BeTrue();
        var render = new ChangingDeckRender();
        var deckB = new LiveQueueItem("ppt:b", "Deck B", LiveItemKinds.PowerPoint) { ContentPath = "b.pptx" };
        var (sut, keys, _) = DeckSut(new RecordingPowerPointSlideShowControl(), render, new SuccessPowerPointRenderService(),
            settings: settings, second: deckB);
        await sut.GoLiveCommand.ExecuteAsync(null);
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var moving = sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.OutputItem!.Id.Should().Be(deckB.Id);
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Rendering);

        PressEscape(keys).Should().BeTrue();
        render.Gate.SetResult();
        await moving;

        await Until(() => sut.OutputPowerPoint.State == PowerPointPreviewState.Ready && sut.OutputPowerPoint.Thumbnails.Count == 3);

        sut.OutputItem!.Id.Should().Be(deckB.Id);
        sut.OutputPowerPoint.LoadedContentPath.Should().Be("b.pptx", "deck A's frame must not stand for deck B");
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
    }

    [Fact]
    public async Task ASlideDrawnWhilePowerPointClosesTheShowDoesNotStartTheShowAgain()
    {
        var render = new ChangingDeckRender();
        var show = new HeldStopShow();
        var (sut, keys, _) = DeckSut(show, render);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        show.Starts.Should().Equal(1);
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var turning = sut.GoToOutputSlideCommand.ExecuteAsync(2);
        show.StopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        PressEscape(keys).Should().BeTrue(); // the show is closing: PowerPoint has not answered yet
        render.Gate.SetResult();
        await turning;
        show.StopGate.SetResult();
        for (int i = 0; i < 100 && sut.IsShowRunning; i++) await Task.Delay(20);

        sut.IsShowRunning.Should().BeFalse();
        show.Starts.Should().Equal([1], "the slide drawn while the show closed must not open it again");
        sut.OutputPowerPoint.SlideNumber.Should().Be(1);
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
    }

    private static async Task Until(Func<bool> done)
    {
        for (int i = 0; i < 100 && !done(); i++) await Task.Delay(20);
    }

    [Fact]
    public async Task AnItemMoveDrawnWhilePowerPointClosesTheShowDoesNotPublishIt()
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.None).Succeeded.Should().BeTrue();
        var render = new ChangingDeckRender();
        var show = new HeldStopShow();
        var deckB = new LiveQueueItem("ppt:b", "Deck B", LiveItemKinds.PowerPoint) { ContentPath = "b.pptx" };
        var (sut, keys, _) = DeckSut(show, render, new SuccessPowerPointRenderService(), settings: settings, second: deckB);
        await sut.GoLiveCommand.ExecuteAsync(null);
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var moving = sut.NextOutputItemCommand.ExecuteAsync(null); // deck B is being drawn
        show.StopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        PressEscape(keys).Should().BeTrue(); // the show is closing: PowerPoint has not answered yet
        render.Gate.SetResult();
        await moving;
        show.StopGate.SetResult();
        await Until(() => !sut.IsShowRunning);

        sut.IsShowRunning.Should().BeFalse();
        show.StartPaths.Should().NotContain("b.pptx", "a move finished while the show closed must not open the next deck");
    }

    [Fact]
    public async Task AStartFromTheOutputPanelStopsWhenAnotherItemIsCopiedThereWhileTheDeckIsReadAgain()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var output = new ChangingDeckRender();
        var show = new RecordingPowerPointSlideShowControl();
        var deckB = new LiveQueueItem("ppt:b", "Deck B", LiveItemKinds.PowerPoint) { ContentPath = "b.pptx" };
        var (sut, keys, _) = DeckSut(show, output, new SuccessPowerPointRenderService(), path, second: deckB);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        int starts = show.StartRequests.Count;
        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));

        output.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var starting = sut.ToggleOutputLiveCommand.ExecuteAsync(null); // the changed deck is read again
        sut.SelectedItem = deckB;
        sut.CopyPreviewToOutputCommand.Execute(null); // the operator puts deck B on the output panel meanwhile
        output.Gate.SetResult();
        await starting;

        sut.IsShowRunning.Should().BeFalse("the deck the start was for left the output panel");
        sut.StatusText.Should().Contain("송출 칸 항목이 바뀌어");
        show.StartRequests.Should().HaveCount(starts);
        sut.OutputItem!.Id.Should().Be(deckB.Id);
        sut.OutputPowerPoint.LoadedContentPath.Should().Be("b.pptx");
    }

    [Fact]
    public async Task AListLoadedDuringAShowClosedWithTheOutputWindowIsNotFollowedByALaterShow()
    {
        var (sut, _, _, _) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        var solo = new LiveQueueItem("song:solo", "Solo", LiveItemKinds.Song) { Lyrics = "[1]\nsolo" };
        sut.LoadQueue([solo]); // during the show
        await sut.CloseOutputCommand.ExecuteAsync(null);
        sut.IsShowRunning.Should().BeFalse();

        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = solo;
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.RemoveSelectedItemCommand.Execute(null); // the list is emptied: the show ends
        await Until(() => !sut.IsShowRunning);

        sut.IsShowRunning.Should().BeFalse();
        sut.OutputItem!.Id.Should().Be(solo.Id, "an emptied list leaves the output panel; the earlier show's list load is done with");
    }

    [Fact]
    public async Task EscDroppingTheReReadOfAChangedDeckDrawsItsThumbnailsAgain()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, _, keys, _) = DeckSentWithF11(render, path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        File.WriteAllText(path, "saved again with two slides");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
        render.Count = 2;
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var turning = sut.GoToOutputSlideCommand.ExecuteAsync(2); // the changed deck's strip is emptied for the re-read
        sut.OutputPowerPoint.Thumbnails.Should().BeEmpty();

        PressEscape(keys).Should().BeTrue();
        render.Gate.SetResult();
        await turning;
        await Until(() => sut.OutputPowerPoint.Thumbnails.Count == 2);

        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
        sut.OutputPowerPoint.SlideCount.Should().Be(2, "the kept panel is the saved deck, not the old frame over a new strip");
        sut.OutputPowerPoint.SlideNumber.Should().Be(1);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(2, "the kept panel is not left without its strip");
    }

    [Fact]
    public async Task AfterEsc_ALoadedListWithAShorterSongKeepsTheOutputPageWithinIt()
    {
        var song = new LiveQueueItem("song:7", "Song", LiveItemKinds.Song) { Lyrics = "one\n\ntwo\n\nthree" };
        var (sut, keys, _) = DeckSut(new RecordingPowerPointSlideShowControl(), second: song);
        sut.SelectedItem = song;
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        sut.OutputItem!.LyricsPageIndex.Should().Be(2);

        sut.LoadQueue([song with { Lyrics = "only" }]);

        sut.OutputItem!.LyricsPageIndex.Should().Be(0);
        sut.OutputLyricsText.Should().Contain("only");
    }

    [Fact]
    public async Task AMoveOfTheShowThatEndedDoesNotGoIntoTheNextShow()
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.None).Succeeded.Should().BeTrue();
        var render = new ChangingDeckRender();
        var show = new RecordingPowerPointSlideShowControl();
        var deckB = new LiveQueueItem("ppt:b", "Deck B", LiveItemKinds.PowerPoint) { ContentPath = "b.pptx" };
        var (sut, keys, _) = DeckSut(show, render, new SuccessPowerPointRenderService(), settings: settings, second: deckB);
        var song = new LiveQueueItem("song:third", "Third", LiveItemKinds.Song) { Lyrics = "[1]\nthird" };
        sut.Queue.Add(song);
        await sut.GoLiveCommand.ExecuteAsync(null);
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var moving = sut.NextOutputItemCommand.ExecuteAsync(null); // deck B is drawn slowly

        PressEscape(keys).Should().BeTrue();
        sut.SelectedItem = song;
        await sut.GoLiveCommand.ExecuteAsync(null); // the next show starts before deck B is drawn
        render.Gate.SetResult();
        await moving;

        sut.Session.Current.CurrentItemId.Should().Be(song.Id, "the move of the show that ended must not publish into this one");
        show.StartRequests.Select(r => r.Request.FilePath).Should().NotContain("b.pptx");
    }

    [Fact]
    public async Task PageDownWhilePowerPointClosesTheShowLeavesTheKeptPanel()
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.None).Succeeded.Should().BeTrue();
        var show = new HeldStopShow();
        var deckB = new LiveQueueItem("ppt:b", "Deck B", LiveItemKinds.PowerPoint) { ContentPath = "b.pptx" };
        var (sut, keys, deck) = DeckSut(show, settings: settings, second: deckB);
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.StopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        PressEscape(keys).Should().BeTrue(); // PowerPoint has not closed the show yet
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        show.StopGate.SetResult();
        await Until(() => !sut.IsShowRunning);

        sut.OutputItem!.Id.Should().Be(deck.Id, "a move while the show closes is not taken");
        sut.OutputPowerPoint.LoadedContentPath.Should().Be("deck.pptx");
        show.StartPaths.Should().NotContain("b.pptx");
    }

    [Fact]
    public async Task ASlideDrawnWhileTheOutputWindowClosesDoesNotStartTheShowAgain()
    {
        var render = new ChangingDeckRender();
        var show = new HeldStopShow();
        var (sut, _, _) = DeckSut(show, render);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var turning = sut.GoToOutputSlideCommand.ExecuteAsync(2);
        show.StopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var closing = sut.CloseOutputCommand.ExecuteAsync(null); // PowerPoint has not closed the show yet
        render.Gate.SetResult();
        await turning;
        show.StopGate.SetResult();
        await closing;

        show.Starts.Should().Equal([1], "the slide drawn while the output window closed must not open the show again");
        sut.IsShowRunning.Should().BeFalse();
        sut.OutputItem.Should().BeNull();
    }

    [Fact]
    public async Task AfterEsc_ALoadedRowOfTheSameIdNamingAnotherDeckFileIsDrawnInThePanel()
    {
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();

        sut.LoadQueue([deck with { ContentPath = "moved.pptx" }]);
        await Until(() => sut.OutputPowerPoint.LoadedContentPath == "moved.pptx" && sut.OutputPowerPoint.Thumbnails.Count == 3);

        sut.OutputItem!.ContentPath.Should().Be("moved.pptx");
        sut.OutputPowerPoint.LoadedContentPath.Should().Be("moved.pptx", "the panel does not keep the old file under the new row");
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
    }

    [Fact]
    public async Task ASettleReplacedByANewerLoadStopsInsteadOfFallingBackToTheFirstSlide()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var show = new RecordingPowerPointSlideShowControl();
        var deckB = new LiveQueueItem("ppt:b", "Deck B", LiveItemKinds.PowerPoint) { ContentPath = "b.pptx" };
        var (sut, keys, _) = DeckSut(show, render, new SuccessPowerPointRenderService(), path, second: deckB); // the preview shows deck B
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        await sut.GoToOutputSlideCommand.ExecuteAsync(2); // the output item is prepared on slide 2
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(3); // live on slide 3
        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
        var three = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var two = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        render.Hold = r => r.SlideNumber switch { 3 => three.Task, 2 => two.Task, _ => null };

        sut.SelectedItem = deckB; // the preview shows another deck, so the start draws the item itself
        PressEscape(keys).Should().BeTrue(); // the changed deck is read again at slide 3
        var starting = sut.ToggleOutputLiveCommand.ExecuteAsync(null); // the start prepares the item on slide 2
        three.SetResult(); // the settle's render ends after the start replaced it
        await Task.Delay(200);
        two.SetResult();
        await starting;
        render.Hold = null;

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.PptSlideNumber.Should().Be(2, "the replaced settle must not draw slide 1 over the start");
        sut.OutputPowerPoint.SlideNumber.Should().Be(2);
        show.StartRequests.Last().Request.SlideNumber.Should().Be(2);
    }

    [Fact]
    public async Task EscAfterAListLoadDuringTheShowSettlesTheOutputPanelOnce()
    {
        var render = new ChangingDeckRender();
        var (sut, keys, deck) = DeckSut(new RecordingPowerPointSlideShowControl(), render, new SuccessPowerPointRenderService());
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        sut.LoadQueue([deck with { ContentPath = "moved.pptx" }]); // during the show
        var held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        render.Hold = r => r.FilePath == "moved.pptx" ? held.Task : null;

        PressEscape(keys).Should().BeTrue();

        render.Requests.Count(r => r.FilePath == "moved.pptx").Should().Be(1, "one settle draws the new file, not two at once");
        held.SetResult();
        await Until(() => sut.OutputPowerPoint.LoadedContentPath == "moved.pptx" && sut.OutputPowerPoint.Thumbnails.Count == 3);
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
    }

    [Fact]
    public async Task AfterEsc_ALoadedListKeepsTheSlideThePanelShows_NotTheOneTheItemWasSentOn()
    {
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        await sut.GoToOutputSlideCommand.ExecuteAsync(3); // a live turn moves the panel, not the item
        PressEscape(keys).Should().BeTrue();

        sut.LoadQueue([deck with { Title = "Deck (again)" }]);
        sut.OutputItem!.SlideNumber.Should().Be(3);

        sut.LoadQueue([deck with { ContentPath = "moved.pptx" }]);
        await Until(() => sut.OutputPowerPoint.LoadedContentPath == "moved.pptx");
        sut.OutputPowerPoint.SlideNumber.Should().Be(3, "another file of the row opens where the panel was");
    }

    [Fact]
    public async Task AStepWhilePowerPointClosesTheShowIsNotSentToIt()
    {
        var show = new HeldStopShow();
        var (sut, keys, _) = DeckSut(show);
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.StopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        PressEscape(keys).Should().BeTrue(); // PowerPoint has not closed the show yet
        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        show.StopGate.SetResult();
        await Until(() => !sut.IsShowRunning);

        show.Steps.Should().Be(0);
        sut.OutputPowerPoint.SlideNumber.Should().Be(1);
    }

    [Fact]
    public async Task AStartFromTheOutputPanelGoesOnWhenTheListIsLoadedAgainWhileTheDeckIsReadAgain()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, _, keys, deck) = DeckSentWithF11(render, path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));

        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var starting = sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.LoadQueue([deck with { Title = "Deck (again)" }]); // the same row in the list loaded again
        render.Gate.SetResult();
        await starting;

        sut.Session.Current.State.Should().Be(LiveState.Active, "the output item is still that deck");
        sut.Session.Current.CurrentItemTitle.Should().Be("Deck (again)");
    }

    [Fact]
    public async Task AShowStartedWhileTheDeckIsReadAgainIsNotToldTheStartWasDropped()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, _, keys, _) = DeckSentWithF11(render, path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));

        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var starting = sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.SelectedItem = sut.Queue[1];
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null); // F11 on the song meanwhile
        render.Gate.SetResult();
        await starting;

        sut.Session.Current.CurrentItemId.Should().Be("song:next");
        sut.StatusText.Should().NotContain("송출 칸 항목이 바뀌어");
    }

    [Fact]
    public async Task AStartFromTheOutputPanelKeepsTheRowTheDeckWasSentFrom()
    {
        // The kept-deck start leaves the output's source row as it is: the list may hold the deck twice.
        var (sut, _, keys, deck) = DeckSentWithF11();
        var again = deck with { Title = "Deck (second row)" };
        sut.Queue.Add(again);
        sut.SelectedItem = again;
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        await sut.GoToOutputSlideCommand.ExecuteAsync(2);

        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);

        sut.LiveRow.Should().BeSameAs(again);
    }

    [Fact]
    public async Task AfterEsc_ALoadedListGivesTheOutputPanelItsRowPreparedForOutput()
    {
        var legacy = new LiveQueueItem("powerpoint:legacy.pptx", "Legacy", LiveItemKinds.Item) { ContentPath = "legacy.pptx" };
        var (sut, keys, _) = DeckSut(new RecordingPowerPointSlideShowControl(), second: legacy);
        sut.SelectedItem = legacy;
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        sut.OutputItem!.Kind.Should().Be(LiveItemKinds.PowerPoint);

        sut.LoadQueue([legacy with { Title = "Legacy (again)" }]);

        sut.OutputItem!.Title.Should().Be("Legacy (again)");
        sut.OutputItem.Kind.Should().Be(LiveItemKinds.PowerPoint, "the panel holds the row as it goes out, not the raw row");
    }

    [Fact]
    public async Task StoppingTheShowDoesNotWaitForTheKeptPanelToBeDrawn()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, _, _, _) = DeckSentWithF11(render, path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var stopping = sut.StopLiveCommand.ExecuteAsync(null); // the kept deck is read again (held)
        (await Task.WhenAny(stopping, Task.Delay(2000))).Should().BeSameAs(stopping, "the stop does not wait for the panel");
        sut.IsShowRunning.Should().BeFalse();
        sut.ToggleOutputLiveCommand.CanExecute(null).Should().BeTrue();
        sut.OutputPanelSettled.IsCompleted.Should().BeFalse();

        render.Gate.SetResult();
        await sut.OutputPanelSettled;
        sut.OutputPowerPoint.State.Should().Be(PowerPointPreviewState.Ready);
    }

    [Fact]
    public async Task AStartFromTheOutputPanelGoesOnWhenTheSameDeckIsCopiedThereWhileItIsReadAgain()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, keys, deck) = DeckSut(new RecordingPowerPointSlideShowControl(), render, new SuccessPowerPointRenderService(), path);
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();
        File.WriteAllText(path, "saved again");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));

        render.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var starting = sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.SelectedItem = deck;
        sut.CopyPreviewToOutputCommand.Execute(null); // the same deck, drawn anew, while the start reads it again
        render.Gate.SetResult();
        await starting;

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.StatusText.Should().NotContain("송출 칸 항목이 바뀌어");
    }

    [Fact]
    public async Task ASettleWaitingBehindAnotherDoesNotDrawWhileANewShowRuns()
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "deck.pptx");
        File.WriteAllText(path, "three slides");
        var render = new ChangingDeckRender();
        var (sut, keys, deck) = DeckSut(new RecordingPowerPointSlideShowControl(), render, new SuccessPowerPointRenderService(), path);
        async Task SendDeck() { sut.SelectedItem = deck; await sut.SendToOutputAndNextCommand.ExecuteAsync(null); }
        void Save(string text) { File.WriteAllText(path, text); File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(text.Length)); }
        var held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await SendDeck();
        Save("saved once");
        render.Hold = _ => held.Task;
        PressEscape(keys).Should().BeTrue(); // the first settle reads the saved deck (held)
        await SendDeck();
        PressEscape(keys).Should().BeTrue(); // the second settle waits for the first
        await SendDeck(); // a new show runs
        Save("saved twice, longer");
        int requests = render.Requests.Count;

        held.SetResult();
        await sut.OutputPanelSettled;

        sut.IsShowRunning.Should().BeTrue();
        render.Requests.Should().HaveCount(requests, "a settle of an ended show does not draw under the running one");
    }

    [Fact]
    public async Task AfterEsc_LoadingAnEmptyListLeavesTheOutputPanel()
    {
        var (sut, _, keys, deck) = DeckSentWithF11();
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);
        PressEscape(keys).Should().BeTrue();

        sut.LoadQueue([]);

        sut.OutputItem!.Id.Should().Be(deck.Id);
        sut.OutputPowerPoint.Thumbnails.Should().HaveCount(3);
    }

    [Fact]
    public async Task NothingGoesLiveWhilePowerPointClosesTheShow()
    {
        var show = new HeldStopShow();
        var (sut, keys, deck) = DeckSut(show);
        await sut.GoLiveCommand.ExecuteAsync(null);
        show.StopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        PressEscape(keys).Should().BeTrue(); // PowerPoint has not closed the show yet
        sut.SelectedItem = deck;
        await sut.SendToOutputAndNextCommand.ExecuteAsync(null); // F11 again at once

        sut.StatusText.Should().Contain("쇼를 끝내는 중");
        show.Starts.Should().Equal([1], "no new native show is started after the stop was sent");
        show.StopGate.SetResult();
        await Until(() => !sut.IsShowRunning);
        sut.IsShowRunning.Should().BeFalse();
        show.Starts.Should().Equal([1]);
    }
}