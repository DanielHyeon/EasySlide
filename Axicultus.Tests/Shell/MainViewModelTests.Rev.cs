using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// Live-output review: Black, Clear and Hide, item moves under them, the show keys and the gap screen.
public partial class MainViewModelTests
{
    [Fact]
    public async Task DeckBlackedThenNextItemThenUnBlack_LeavesNoBlackNativeShowOverTheSong()
    {
        using var folder = TempSettingsFolder.Create();
        string deck = Path.Combine(folder.Root, "service.pptx");
        File.WriteAllText(deck, "slides");
        var slides = new LiveQueueItem("deck", "Service", LiveItemKinds.PowerPoint) { ContentPath = deck };
        var song = new LiveQueueItem("song", "Closing", LiveItemKinds.Song) { Lyrics = "one\n\ntwo" };
        var show = new RecordingPowerPointSlideShowControl { ActualSlide = 1 };
        var renderer = new FixedSlideCountPowerPointRenderService(3);
        using var sut = CreateSut(seedSampleQueue: false, powerPointSlideShow: show,
            powerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()),
            outputPowerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()));
        sut.LoadQueue([slides, song]);
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        show.Blackouts.Should().Equal(true);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemId.Should().Be(song.Id);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentItemId.Should().Be(song.Id);
        // The deck's native show is a window over the audience window and is still blacked: it must be stopped
        // (or at least un-blacked) once the live item is the song.
        (show.StopCount > 0 || show.Blackouts.LastOrDefault() == false).Should().BeTrue(
            "the blacked native show of the previous deck covers the song on the audience screen");
    }

    [Fact]
    public async Task DeckSlideMovedUnderBlack_IsTheSlideTheAudienceSeesAfterUnBlack()
    {
        using var folder = TempSettingsFolder.Create();
        string deck = Path.Combine(folder.Root, "service.pptx");
        File.WriteAllText(deck, "slides");
        var slides = new LiveQueueItem("deck", "Service", LiveItemKinds.PowerPoint) { ContentPath = deck };
        var show = new RecordingPowerPointSlideShowControl { ActualSlide = 1 };
        var renderer = new FixedSlideCountPowerPointRenderService(3);
        using var sut = CreateSut(seedSampleQueue: false, powerPointSlideShow: show,
            powerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()),
            outputPowerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()));
        sut.LoadQueue([slides]);
        await sut.GoLiveCommand.ExecuteAsync(null);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        sut.Session.Current.PptSlideNumber.Should().Be(2, "the operator moved to slide 2 under Black");
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        // FrmLaunchShow moves the slide under the black overlay; here the native show never heard of slide 2.
        (show.AdvanceCount > 0 || show.StartRequests[^1].Request.SlideNumber == 2).Should().BeTrue(
            "after un-Black the audience must see slide 2, the slide the operator panel shows");
    }

    [Fact]
    public async Task NextItemUnderBlack_OpensTheItemOnItsFirstVerse_AsWhenActive()
    {
        var first = new LiveQueueItem("song-1", "첫 곡", LiveItemKinds.Song) { Lyrics = "a1\n\na2\n\na3" };
        var second = new LiveQueueItem("song-2", "둘째 곡", LiveItemKinds.Song) { Lyrics = "b1\n\nb2\n\nb3" };
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([first, second]);
        sut.SelectedItem = first;
        await sut.GoLiveCommand.ExecuteAsync(null);
        // The operator looks ahead in the preview: second song, third verse.
        sut.SelectedItem = second;
        sut.NextLyricsPageCommand.Execute(null);
        sut.NextLyricsPageCommand.Execute(null);
        sut.Session.Current.CurrentItemId.Should().Be(first.Id);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemId.Should().Be(second.Id);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(0, "Next item opens the item on its first verse (it does when not blacked)");
    }

    [Fact]
    public void ShowKeyFromTheAudienceWindow_DoesNotChangeTheSavedSetting()
    {
        // FrmLaunchShow R / N toggle Gf.ShowRunning_* (reset by ResetShowRunningSettings at the next show).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var region = settings.Get(AxicultusSettingKeys.LyricsMonitorRegionDisplay);
        var notations = settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations);
        var sut = CreateSut(settings: settings);

        sut.OutputShowKeyCommand(System.Windows.Input.Key.R)!.Execute(null);
        sut.OutputShowKeyCommand(System.Windows.Input.Key.N)!.Execute(null);

        settings.Get(AxicultusSettingKeys.LyricsMonitorRegionDisplay).Should().Be(region, "R only changes the running show in FrmLaunchShow");
        settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations).Should().Be(notations, "N only changes the running show in FrmLaunchShow");
    }

    [Fact]
    public Task ErrorStormDuringAShow_IsNotSwallowedForEver() => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        var log = new OperationalErrorLog(directory.FullName) { ReportDuringShow = _ => true };
        var dispatcher = Dispatcher.CurrentDispatcher;
        int handled = 0, seen = 0;
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Observe(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            if (args.Handled) handled++;
            args.Handled = true; // Only the test keeps this dispatcher running.
            if (++seen == 20) done.SetResult(true);
        }

        dispatcher.UnhandledException += log.OnUiException;
        dispatcher.UnhandledException += Observe;
        try
        {
            for (int i = 0; i < 20; i++)
                _ = dispatcher.BeginInvoke(new Action(() => throw new InvalidOperationException("render failure")));
            await done.Task.WaitAsync(TimeSpan.FromSeconds(10));
            handled.Should().BeLessThanOrEqualTo(5, "the stated policy is at most 5 handled errors in 10 seconds");
        }
        finally
        {
            dispatcher.UnhandledException -= Observe;
            dispatcher.UnhandledException -= log.OnUiException;
            directory.Delete(true);
        }
    });

    [Fact]
    public async Task ShowKeys_ChangeTheRunningShowOnly_AndTheSavedValuesComeBackWhenItEnds()
    {
        // FrmLaunchShow R / N: Gf.ShowRunning_* (reset for the next show). The output reads the settings, so the
        // saved values are put back when the show ends.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var region = settings.Get(AxicultusSettingKeys.LyricsMonitorRegionDisplay);
        var notations = settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations);
        var sut = CreateSut(settings: settings);
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.OutputShowKeyCommand(System.Windows.Input.Key.R)!.Execute(null);
        sut.OutputShowKeyCommand(System.Windows.Input.Key.R)!.Execute(null);
        sut.OutputShowKeyCommand(System.Windows.Input.Key.N)!.Execute(null);
        sut.ActiveRegionDisplay.Should().NotBe(region, "the key changes the running show");
        sut.ActiveLyricsNotations.Should().Be(!notations);

        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);

        sut.IsShowRunning.Should().BeFalse();
        settings.Get(AxicultusSettingKeys.LyricsMonitorRegionDisplay).Should().Be(region);
        settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations).Should().Be(notations);
        sut.ActiveRegionDisplay.Should().Be(region);
    }

    [Fact]
    public async Task BlackOverAHiddenOutput_ReturnsToHidden_AndTheStatusLineSaysHidden()
    {
        // Black and Hide are two switches, as Black and Clear are.
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.HideOutputCommand.ExecuteAsync(null);
        sut.OutputScreenStatus.Should().Contain("화면: 숨김");
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.IsOutputBlackActive.Should().BeTrue();
        sut.OutputScreenStatus.Should().Contain("화면: 검은 화면");
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);

        sut.IsOutputBlackActive.Should().BeFalse();
        sut.Session.Current.State.Should().Be(LiveState.Hidden, "turning Black off must not show a hidden output");
        sut.StatusText.Should().Be("출력 숨김");
        sut.OutputScreenStatus.Should().Contain("화면: 숨김");

        sut.RestoreOutputCommand.Execute(null);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Active, "Black alone still returns to the output");
        sut.OutputScreenStatus.Should().Contain("화면: 보통");
    }

    [Fact]
    public async Task PreviousAtTheFirstPage_OpensATextFileRowOnItsLastPage()
    {
        // FrmLaunchShow.MoveToSlideLiveItem: MoveToLiveItem(PrevOne, 30000). The pages of a text file exist only
        // once the row is prepared for output.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        var path = Path.Combine(folder.Root, "notice.txt");
        File.WriteAllText(path, "first\n \nsecond\n \nthird");
        var text = new LiveQueueItem("text:notice", "Notice", LiveItemKinds.Notice) { ContentPath = path };
        var song = new LiveQueueItem("song", "Closing", LiveItemKinds.Song) { Lyrics = "one\n\ntwo" };
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        sut.LoadQueue([text, song]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = song;
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.LivePreviousShortcutCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemId.Should().Be(text.Id);
        sut.Session.Current.CurrentItemBodyText.Should().Be("third");
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(2);
    }

    [Fact]
    public async Task MovingToAnotherItemUnderBlack_TakesDownTheAudioOfTheItemLeftBehind()
    {
        // FrmLaunchShow.LoadItem: the media player follows the live item (Remote_StopItem), black or not.
        using var folder = TempSettingsFolder.Create();
        var audio = Path.Combine(folder.Root, "prelude.mp3");
        File.WriteAllText(audio, "media placeholder");
        var clip = new LiveQueueItem("media:prelude", "Prelude", LiveItemKinds.Media) { ContentPath = audio };
        var song = new LiveQueueItem("song", "Opening", LiveItemKinds.Song) { Lyrics = "one" };
        using var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([clip, song]);
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Media.PlayPauseCommand.Execute(null);
        sut.Media.State.Should().Be(MediaPlaybackState.Playing);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.NextOutputItemCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemId.Should().Be(song.Id);
        sut.Media.State.Should().Be(MediaPlaybackState.Empty, "the prelude must not play on under the next item");
    }

    [Fact]
    public async Task GapScreen_AfterADeck_UnderBlack_AndAcrossAStopAndRestart()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.Default).Succeeded.Should().BeTrue();
        string deck = Path.Combine(folder.Root, "service.pptx");
        File.WriteAllText(deck, "slides");
        var slides = new LiveQueueItem("deck", "Service", LiveItemKinds.PowerPoint) { ContentPath = deck };
        var song = new LiveQueueItem("song", "Closing", LiveItemKinds.Song) { Lyrics = "one\n\ntwo" };
        var show = new RecordingPowerPointSlideShowControl { ActualSlide = 1 };
        var renderer = new FixedSlideCountPowerPointRenderService(3);
        using var sut = CreateSut(settings: settings, seedSampleQueue: false, powerPointSlideShow: show,
            powerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()),
            outputPowerPoint: new PowerPointPreviewViewModel(renderer, _ => Frozen()));
        sut.LoadQueue([slides, song]);
        await sut.GoLiveCommand.ExecuteAsync(null);

        // After a deck: the session holds no deck while the gap shows.
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue();
        sut.Session.Current.PptPath.Should().BeNullOrEmpty("the gap screen is not a slide of the deck");

        // Under Black the gap is left like any item, and the next item waits under the black screen.
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue();
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeFalse();
        sut.Session.Current.CurrentItemId.Should().Be(song.Id);
        sut.Session.Current.State.Should().Be(LiveState.Hidden);

        // The gap screen is reached under Black too.
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue("the gap screen follows the last item, also under Black");
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.IsGapItemOnDisplay.Should().BeTrue();

        // Stop on the gap screen, start again: the show opens the selected row, not a gap screen.
        await sut.ToggleOutputLiveCommand.ExecuteAsync(null);
        sut.IsShowRunning.Should().BeFalse();
        sut.IsGapItemOnDisplay.Should().BeFalse();
        sut.LiveItemId.Should().BeNull();
        sut.SelectedItem = song;
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeFalse();
        sut.Session.Current.CurrentItemId.Should().Be(song.Id);
    }

    [Fact]
    public async Task RemovingTheOutputRowWhileTheGapShows_PutsTheNewSelectionOnOutputAtOnce()
    {
        // FrmMain.RemoveWorshipListSong: OutputItem.CurItemNo stays the row while item "G" shows, so the removal
        // counts as the output row and CopyPreviewToOutput loads the newly selected row (Remote_SongChanged).
        var sut = await StartShowWithGapOptionAsync(GapItemMode.Default, advanceNextItem: true);
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue();

        sut.SelectedItem = sut.Queue[0];
        sut.RemoveSelectedItemCommand.Execute(null);

        sut.IsGapItemOnDisplay.Should().BeFalse();
        sut.Session.Current.CurrentItemId.Should().Be("song-b");
    }
}