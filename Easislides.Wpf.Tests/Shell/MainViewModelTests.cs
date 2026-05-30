using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Data;
using Easislides.Wpf.Media;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_UsesReadableKoreanDefaultStatusAndSampleQueue()
    {
        var sut = CreateSut();

        sut.StatusText.Should().Be("3개 항목 로드됨");
        sut.Queue.Select(item => item.Title)
            .Should()
            .Contain(["예배 시작 안내", "주일 찬양 #1", "말씀 본문"]);
    }

    [Fact]
    public void LoadQueue_WhenEmpty_UsesReadableKoreanStatus()
    {
        var sut = CreateSut();

        sut.LoadQueue([]);

        sut.StatusText.Should().Be("송출할 항목이 없습니다");
    }

    [Fact]
    public async Task GoLiveCommand_RequiresSelectionAndOpenOutput()
    {
        var sut = CreateSut();
        var item = new LiveQueueItem("song-1", "입례 찬양");
        sut.LoadQueue(new[] { item });

        sut.GoLiveCommand.CanExecute(null).Should().BeFalse("출력 창이 열리기 전에는 라이브 시작을 막는다");

        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = item;

        sut.GoLiveCommand.CanExecute(null).Should().BeTrue();
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Active);
        sut.LiveBar.CurrentItemTitle.Should().Be("입례 찬양");
        sut.StatusText.Should().Contain("LIVE");
    }

    [Fact]
    public async Task GoLiveCommand_WhenSafetyPromptDeclines_DoesNotChangeState()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];

        await sut.GoLiveCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle(request => request.ActionName == MainCommandIds.LiveGo);
        sut.LiveBar.State.Should().Be(LiveState.Off);
        sut.Session.Current.State.Should().Be(LiveState.Off);
        sut.StatusText.Should().Be("라이브 안전 확인 취소");
    }

    [Fact]
    public async Task GoLiveCommand_WhenAdvanceNextItemEnabled_PublishesCurrentAndSelectsNext()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("입례 찬양");
        sut.LiveBar.CurrentItemTitle.Should().Be("입례 찬양");
        sut.SelectedItem.Should().Be(second);
    }

    [Fact]
    public void Constructor_UsesOperationalPowerPointAndMediaSettings()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.UsePowerPointTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoPowerPointPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 12).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.UseMediaTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoMediaPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaDirectory, @"C:\EasiSlides\Media").Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LiveCameraNumber, 4).Succeeded.Should().BeTrue();

        var sut = CreateSut(settings: settings);

        sut.IsPowerPointTabVisible.Should().BeTrue();
        sut.IsPowerPointPanelOverlayEnabled.Should().BeFalse();
        sut.PowerPointMaxFiles.Should().Be(12);
        sut.IsMediaTabVisible.Should().BeTrue();
        sut.IsMediaPanelOverlayEnabled.Should().BeFalse();
        sut.MediaDirectory.Should().Be(@"C:\EasiSlides\Media");
        sut.LiveCameraNumber.Should().Be(4);
        sut.LiveCameraSource.Should().Be("<<Capture>>4");
    }

    [Fact]
    public void SettingsChanged_RefreshesOperationalPowerPointAndMediaSettings()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.UsePowerPointTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoPowerPointPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 7).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.UseMediaTab, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.NoMediaPanelOverlay, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaDirectory, @"D:\Media").Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LiveCameraNumber, 5).Succeeded.Should().BeTrue();

        sut.IsPowerPointTabVisible.Should().BeTrue();
        sut.IsPowerPointPanelOverlayEnabled.Should().BeFalse();
        sut.PowerPointMaxFiles.Should().Be(7);
        sut.IsMediaTabVisible.Should().BeTrue();
        sut.IsMediaPanelOverlayEnabled.Should().BeFalse();
        sut.MediaDirectory.Should().Be(@"D:\Media");
        sut.LiveCameraNumber.Should().Be(5);
        sut.LiveCameraSource.Should().Be("<<Capture>>5");
    }

    [Fact]
    public void LoadQueue_WhenPowerPointItemsExceedConfiguredLimit_DisablesGoLiveUntilLimitIncreases()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 1).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("ppt-1", "Deck 1", "P"),
            new LiveQueueItem("ppt-2", "Deck 2", "PowerPoint"),
        });
        sut.StatusText.Should().Be("PowerPoint 제한 초과: 2/1");
        sut.OpenOutputCommand.Execute(null);

        sut.HasPowerPointLimitViolation.Should().BeTrue();
        sut.GoLiveCommand.CanExecute(null).Should().BeFalse();

        settings.Set(EasiSettingKeys.PowerPointMaxFiles, 2).Succeeded.Should().BeTrue();

        sut.HasPowerPointLimitViolation.Should().BeFalse();
        sut.GoLiveCommand.CanExecute(null).Should().BeTrue();
        sut.StatusText.Should().Be("2개 항목 로드됨");
    }

    [Fact]
    public void AddBibleSelection_InsertsAfterCurrentSelectionAndSelectsInsertedItem()
    {
        var sut = CreateSut();
        var opener = new LiveQueueItem("song-1", "Opening Song", "Song");
        var sermon = new LiveQueueItem("sermon", "Sermon", "Message");
        sut.LoadQueue([opener, sermon]);
        sut.SelectedItem = opener;
        var selection = new BibleSelection("0;kjv.db;niv.db;1;1;1;1;1;", "Genesis 1:1 (KJV/NIV)");

        var inserted = sut.AddBibleSelection(selection);

        inserted.Should().NotBeNull();
        inserted!.Id.Should().Be(selection.IdString);
        inserted.Title.Should().Be(selection.Title);
        inserted.Kind.Should().Be("Bible");
        sut.Queue.Should().Equal(opener, inserted, sermon);
        sut.SelectedItem.Should().Be(inserted);
        sut.StatusText.Should().Contain("성경 구절 추가됨");
        sut.StatusText.Should().Contain("Genesis 1:1");
    }

    [Fact]
    public void AddBibleSelection_WhenSelectionIsEmpty_DoesNotChangeQueue()
    {
        var sut = CreateSut();
        var original = sut.Queue.ToArray();

        var inserted = sut.AddBibleSelection(new BibleSelection("", ""));

        inserted.Should().BeNull();
        sut.Queue.Should().Equal(original);
        sut.StatusText.Should().Be("선택된 성경 구절이 없습니다.");
    }

    [Fact]
    public void AddSong_InsertsAfterCurrentSelectionAndSelectsInsertedItem()
    {
        // 라이브 큐 plumbing: 라이브러리에서 고른 실제 곡을 예배 순서에 추가(AddBibleSelection 과 동일 규칙).
        var sut = CreateSut();
        var opener = new LiveQueueItem("a", "Opening", "Notice");
        var sermon = new LiveQueueItem("b", "Sermon", "Bible");
        sut.LoadQueue([opener, sermon]);
        sut.SelectedItem = opener;
        var song = new SongSummary(
            SongId: 42, Title: "Amazing Grace", AlternateTitle: "", FolderNo: 1,
            SongNumber: 1, Category: "", Key: "G", Lyrics: "Amazing grace...");

        var inserted = sut.AddSong(song);

        inserted.Should().NotBeNull();
        inserted!.Id.Should().Be("song:42");
        inserted.Title.Should().Be("Amazing Grace");
        inserted.Kind.Should().Be("Song");
        sut.Queue.Should().Equal(opener, inserted, sermon);
        sut.SelectedItem.Should().Be(inserted);
        sut.StatusText.Should().Contain("곡 추가됨");
    }

    [Fact]
    public void AddSong_WhenNull_DoesNotChangeQueue()
    {
        var sut = CreateSut();
        var original = sut.Queue.ToArray();

        var inserted = sut.AddSong(null);

        inserted.Should().BeNull();
        sut.Queue.Should().Equal(original);
        sut.StatusText.Should().Be("선택된 곡이 없습니다.");
    }

    [Fact]
    public void AddSong_WhenNoSelection_AppendsToEndOfQueue()
    {
        // 선택 항목이 없으면 큐 끝에 추가(insertIndex = Queue.Count 분기).
        var sut = CreateSut();
        var first = new LiveQueueItem("a", "First", "Notice");
        sut.LoadQueue([first]);
        sut.SelectedItem = null;
        var song = new SongSummary(
            SongId: 7, Title: "Hymn", AlternateTitle: "", FolderNo: 1,
            SongNumber: 1, Category: "", Key: "C", Lyrics: "");

        var inserted = sut.AddSong(song);

        inserted.Should().NotBeNull();
        sut.Queue.Should().Equal(first, inserted!);
    }

    [Fact]
    public void AddSong_WhenTitleBlank_DoesNotChangeQueue()
    {
        // 제목이 공백뿐인 곡도 추가하지 않는다(가드 경계).
        var sut = CreateSut();
        var original = sut.Queue.ToArray();
        var song = new SongSummary(
            SongId: 7, Title: "   ", AlternateTitle: "", FolderNo: 1,
            SongNumber: 1, Category: "", Key: "", Lyrics: "");

        var inserted = sut.AddSong(song);

        inserted.Should().BeNull();
        sut.Queue.Should().Equal(original);
        sut.StatusText.Should().Be("선택된 곡이 없습니다.");
    }

    [Fact]
    public void AddSong_CarriesLyricsOnQueueItem()
    {
        // 콘텐츠 plumbing: 추가 시점에 실제 가사가 큐 항목에 적재되어 선택 시 표시 가능.
        var sut = CreateSut();
        var song = new SongSummary(
            SongId: 9, Title: "Grace", AlternateTitle: "", FolderNo: 1,
            SongNumber: 1, Category: "", Key: "G", Lyrics: "line one\nline two");

        var inserted = sut.AddSong(song);

        inserted!.Lyrics.Should().Be("line one\nline two");
    }

    [Fact]
    public async Task ApplySelectedItemContent_PowerPointItem_DrivesPreviewLoad()
    {
        // PowerPoint 항목(ContentPath 보유) 선택 시 PPT 미리보기 LoadAsync 가 발동(콘텐츠 plumbing).
        // 스텁 서비스가 MissingOffice 를 반환하므로 로드 시도 후 Failed 상태가 된다(=발동 증거).
        var sut = CreateSut();
        var ppt = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };

        await sut.ApplySelectedItemContentAsync(ppt);

        sut.PowerPoint.State.Should().Be(PowerPointPreviewState.Failed, "PPT 항목 선택 시 렌더가 시도돼야 함");
    }

    [Fact]
    public async Task ApplySelectedItemContent_NonPowerPointItem_ClearsPreview()
    {
        // PPT 가 아닌 항목 선택 시 PPT 미리보기를 비운다(이전 PPT 잔상 제거).
        var sut = CreateSut();
        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("song:1", "Song", "Song") { Lyrics = "x" });

        sut.PowerPoint.State.Should().Be(PowerPointPreviewState.Idle, "비-PPT 항목 선택 시 PPT 미리보기는 초기화");
    }

    [Fact]
    public async Task SaveAndLoadWorshipList_RoundTripsQueue()
    {
        // G2(ManageItemLists): 현재 예배 순서를 이름으로 저장하고, 비운 뒤 다시 불러오면 동일 항목이 복원된다.
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        sut.LoadQueue([
            new LiveQueueItem("a", "A", "Song") { Lyrics = "L" },
            new LiveQueueItem("b", "B", "Bible"),
        ]);

        await sut.SaveWorshipListAsync("Sunday AM");
        sut.GetSavedWorshipLists().Should().Contain("Sunday AM");

        sut.LoadQueue([]);
        await sut.LoadWorshipListAsync("Sunday AM");

        sut.Queue.Select(i => i.Id).Should().Equal("a", "b");
        sut.Queue[0].Lyrics.Should().Be("L", "콘텐츠(가사)도 함께 영속");
    }

    [Fact]
    public async Task SaveWorshipList_WhenNameBlank_DoesNotSave()
    {
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());

        await sut.SaveWorshipListAsync("   ");

        sut.GetSavedWorshipLists().Should().BeEmpty();
        sut.StatusText.Should().Contain("이름을 입력");
    }

    [Fact]
    public async Task DeleteWorshipList_RemovesSaved()
    {
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        await sut.SaveWorshipListAsync("X");

        sut.DeleteWorshipList("X");

        sut.GetSavedWorshipLists().Should().NotContain("X");
    }

    [Fact]
    public void AddPowerPoint_CreatesItemWithContentPathAndKind()
    {
        var sut = CreateSut();

        var item = sut.AddPowerPoint(@"C:\decks\sermon.pptx");

        item.Should().NotBeNull();
        item!.Kind.Should().Be("PowerPoint");
        item.ContentPath.Should().Be(@"C:\decks\sermon.pptx");
        item.Title.Should().Be("sermon", "제목은 파일명(확장자 제외)");
        sut.SelectedItem.Should().Be(item);
    }

    [Fact]
    public void AddMedia_CreatesItemWithContentPathAndKind()
    {
        var sut = CreateSut();

        var item = sut.AddMedia(@"C:\media\intro.mp4");

        item.Should().NotBeNull();
        item!.Kind.Should().Be("Media");
        item.ContentPath.Should().Be(@"C:\media\intro.mp4");
    }

    [Fact]
    public void AddMedia_WhenPathBlank_DoesNotChangeQueue()
    {
        var sut = CreateSut();
        var original = sut.Queue.ToArray();

        var item = sut.AddMedia("   ");

        item.Should().BeNull();
        sut.Queue.Should().Equal(original);
    }

    [Fact]
    public async Task ApplySelectedItemContent_MediaItem_LoadsMedia()
    {
        // 라이브 큐 plumbing 마무리: Media 항목(ContentPath) 선택 시 미디어 재생 VM 에 Load 디스패치.
        var sut = CreateSut();
        var media = new LiveQueueItem("media:1", "Intro", "Media") { ContentPath = @"C:\media\intro.mp4" };

        await sut.ApplySelectedItemContentAsync(media);

        sut.Media.State.Should().Be(MediaPlaybackState.Ready, "미디어 항목 선택 시 Load 발동");
        sut.Media.Source.Should().Be(@"C:\media\intro.mp4");
    }

    [Fact]
    public async Task ApplySelectedItemContent_NonMediaItem_StopsPreviousMedia()
    {
        // 라이브 중 잔류 재생 방지: 미디어 재생 후 다른 종류 항목 선택 시 직전 미디어가 정지돼야 함.
        var sut = CreateSut();
        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("media:1", "Intro", "Media") { ContentPath = @"C:\media\intro.mp4" });
        sut.Media.State.Should().Be(MediaPlaybackState.Ready);

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("song:1", "Song", "Song") { Lyrics = "x" });

        sut.Media.State.Should().Be(MediaPlaybackState.Stopped, "비-미디어 항목 선택 시 직전 미디어 정지");
    }

    [Fact]
    public async Task ApplySelectedItemContent_AudioFile_InfersAudioMediaType()
    {
        // 확장자 기반 MediaType 추정: .mp3 → Audio.
        var sut = CreateSut();

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("media:1", "BGM", "Media") { ContentPath = @"C:\media\bgm.mp3" });

        sut.Media.MediaType.Should().Be("Audio");
    }

    [Fact]
    public void SelectingPowerPointItem_ThroughSetter_DrivesPreviewLoad()
    {
        // fire-and-forget 배선 검증: SelectedItem setter → OnSelectedItemChanged → 디스패치.
        // 스텁 PPT 서비스가 동기(Task.FromResult) 완료라 setter 반환 시점엔 이미 상태가 갱신된다.
        var sut = CreateSut();
        var ppt = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };

        sut.SelectedItem = ppt;

        sut.PowerPoint.State.Should().Be(PowerPointPreviewState.Failed, "PPT 항목 선택(setter)이 미리보기 로드를 발동");
    }

    [Fact]
    public void OpenOutputCommand_UsesPreferredDisplayFromDisplayService()
    {
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay));

        sut.OutputDisplays.Should().Equal(primary, outputDisplay);
        sut.SelectedOutputDisplay.Should().Be(outputDisplay);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("송출 모니터");
        sut.StatusText.Should().Contain("송출 모니터");
    }

    [Fact]
    public void OpenOutputCommand_UsesDefaultOutputMonitorFromSettings()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DefaultOutputMonitorId, "primary");
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay), settings: settings);

        sut.SelectedOutputDisplay.Should().Be(primary);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("주 모니터");
        sut.StatusText.Should().Contain("주 모니터");
    }

    [Fact]
    public void OpenOutputCommand_WhenAlwaysUseSecondaryMonitorDisabledWithoutDefault_SelectsPrimary()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor, false);
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay), settings: settings);

        sut.SelectedOutputDisplay.Should().Be(primary);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("주 모니터");
        sut.StatusText.Should().Contain("주 모니터");
    }

    [Fact]
    public void OpenOutputCommand_WhenDefaultMonitorMissingAndAlwaysUseSecondaryDisabled_FallsBackToPrimary()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DefaultOutputMonitorId, "removed-monitor");
        settings.Set(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor, false);
        var primary = new OutputDisplay("primary", "주 모니터", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var outputDisplay = new OutputDisplay("display-2", "송출 모니터", 1920, 0, 1920, 1080, 1.25);
        var sut = CreateSut(display: new FixedDisplayService(primary, outputDisplay), settings: settings);

        sut.SelectedOutputDisplay.Should().Be(primary);

        sut.OpenOutputCommand.Execute(null);

        sut.LiveBar.OutputMonitorName.Should().Be("주 모니터");
        sut.StatusText.Should().Contain("주 모니터");
    }

    [Fact]
    public async Task NextItemCommand_WhenLive_AdvancesSelectionAndLiveSession()
    {
        var prompt = new RecordingSafetyPrompt(allow: true);
        var sut = CreateSut(prompt);
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = first;
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();

        sut.NextItemCommand.Execute(null);

        sut.SelectedItem.Should().Be(second);
        sut.LiveBar.CurrentItemTitle.Should().Be("봉헌 찬양");
        prompt.Requests.Should().BeEmpty("라이브 중 Next/Prev 리모컨 흐름은 추가 확인으로 막지 않는다");
    }

    [Fact]
    public async Task BlackScreenCommand_WhenLive_AsksSafetyPromptBeforeChangingState()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        prompt.Allow = true;
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();
        prompt.Allow = false;

        await sut.BlackScreenCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle();
        sut.LiveBar.State.Should().Be(LiveState.Active, "사용자가 취소하면 live 상태가 보존되어야 한다");

        prompt.Allow = true;
        await sut.BlackScreenCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Hidden);
        sut.Session.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public async Task StopLiveCommand_WhenLive_AsksSafetyPromptBeforeStopping()
    {
        var prompt = new RecordingSafetyPrompt(allow: false);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        prompt.Allow = true;
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();
        prompt.Allow = false;

        await sut.StopLiveCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle(request => request.ActionName == MainCommandIds.LiveStop);
        sut.LiveBar.State.Should().Be(LiveState.Active);

        prompt.Allow = true;
        await sut.StopLiveCommand.ExecuteAsync(null);

        sut.LiveBar.State.Should().Be(LiveState.Off);
    }

    [Fact]
    public async Task CloseOutputCommand_WhenLive_AsksSafetyPromptBeforeClosing()
    {
        var prompt = new RecordingSafetyPrompt(allow: true);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "입례 찬양") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        prompt.Requests.Clear();
        prompt.Allow = false;

        await sut.CloseOutputCommand.ExecuteAsync(null);

        prompt.Requests.Should().ContainSingle(request => request.ActionName == MainCommandIds.OutputClose);
        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.CloseOutputCommand.CanExecute(null).Should().BeTrue();

        prompt.Allow = true;
        await sut.CloseOutputCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Off);
        sut.CloseOutputCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void BindShortcuts_RegistersLocalAndGlobalLiveNextCommands()
    {
        var catalog = new CommandCatalog();
        var sut = CreateSut(commandCatalog: catalog);
        var registry = new ShortcutRegistry();

        sut.BindShortcuts(registry);

        registry.All.Should().BeEquivalentTo(catalog.GetDefaultShortcuts());
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveGo && !s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveNext && s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LiveNext && !s.IsGlobal);
        registry.All.Should().Contain(s => s.CommandName == MainCommandIds.LivePrevious && !s.IsGlobal);
    }

    [Fact]
    public void BindShortcuts_AppliesSavedShortcutOverrides()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var catalog = new CommandCatalog();
        var defaultShortcut = catalog.GetDefaultShortcuts()
            .Single(shortcut => shortcut.CommandName == MainCommandIds.LiveNext && shortcut.IsGlobal);
        settings.SetShortcutOverride(ShortcutSettings.GetSlotId(defaultShortcut), "F8");
        var sut = CreateSut(commandCatalog: catalog, settings: settings);
        var registry = new ShortcutRegistry();

        sut.BindShortcuts(registry);

        registry.All.Should().Contain(shortcut =>
            shortcut.CommandName == MainCommandIds.LiveNext &&
            shortcut.IsGlobal &&
            shortcut.Key == Key.F8);
        registry.All.Should().NotContain(shortcut =>
            shortcut.CommandName == MainCommandIds.LiveNext &&
            shortcut.IsGlobal &&
            shortcut.Key == Key.F5);
    }

    [Fact]
    public void Exposes_Media_PlaybackViewModel()
    {
        // G1.2(gap-analysis §4 G-α): orphaned MediaPlaybackViewModel 이 MainViewModel(→ MainWindow Media 탭)에
        // 연결됐는지 고정. 미디어 미적재 초기엔 Empty 상태(컨트롤 비활성).
        var sut = CreateSut();

        sut.Media.Should().NotBeNull("Media 탭이 바인딩할 재생 컨트롤 VM 이 노출돼야 함");
        sut.Media.State.Should().Be(MediaPlaybackState.Empty, "미디어 미적재 초기 상태");
    }

    [Fact]
    public void Exposes_PowerPoint_PreviewViewModel()
    {
        // G1(gap-analysis §4 G-α): orphaned PowerPoint 렌더 서비스가 MainViewModel(→ MainWindow PowerPoint 탭)에 연결됐는지 고정.
        var sut = CreateSut();

        sut.PowerPoint.Should().NotBeNull("PowerPoint 탭이 바인딩할 PPT 미리보기 VM 이 노출돼야 함");
        sut.PowerPoint.State.Should().Be(PowerPointPreviewState.Idle, "PPT 미적재 초기 상태");
    }

    [Fact]
    public void Dispose_Is_Idempotent_With_Media_ViewModel()
    {
        // MainViewModel 과 DI 컨테이너가 모두 Media VM 해제를 시도할 수 있어(이중 dispose),
        // 반복 호출이 예외 없이 안전해야 한다(MediaPlaybackViewModel.Dispose 멱등 가드 검증).
        var sut = CreateSut();

        var disposeTwice = () =>
        {
            sut.Dispose();
            sut.Dispose();
        };
        disposeTwice.Should().NotThrow();
    }

    private static MainViewModel CreateSut(
        ILiveSafetyPrompt? prompt = null,
        IDisplayService? display = null,
        ICommandCatalog? commandCatalog = null,
        ISettingsService? settings = null,
        IWorshipListStore? worshipLists = null)
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var telemetry = new InMemoryCommandTelemetry();
        var media = new MediaPlaybackViewModel(new MediaPlaybackService());
        var powerPoint = new PowerPointPreviewViewModel(new StubPowerPointRenderService());
        return new MainViewModel(
            session,
            output,
            prompt ?? new RecordingSafetyPrompt(allow: true),
            telemetry,
            display ?? new FixedDisplayService(OutputDisplay.PrimaryFallback),
            commandCatalog ?? new CommandCatalog(),
            settings ?? TempSettingsFolder.CreateDetachedSettings(),
            media,
            powerPoint,
            worshipLists ?? new InMemoryWorshipListStore());
    }

    // 워십 리스트 저장/로드 — 파일시스템 없이 인메모리로 검증.
    private sealed class InMemoryWorshipListStore : IWorshipListStore
    {
        private readonly Dictionary<string, IReadOnlyList<LiveQueueItem>> _store = new();

        public Task SaveAsync(string name, IReadOnlyList<LiveQueueItem> items, CancellationToken cancellationToken = default)
        {
            _store[name] = items.ToArray();
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LiveQueueItem>> LoadAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.TryGetValue(name, out var items)
                ? items
                : (IReadOnlyList<LiveQueueItem>)Array.Empty<LiveQueueItem>());

        public IReadOnlyList<string> ListNames() => _store.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToArray();

        public void Delete(string name) => _store.Remove(name);
    }

    // MainViewModel 은 PowerPoint VM 을 노출만 하고 렌더를 호출하지 않으므로, 실패 결과만 내는 스텁이면 충분.
    private sealed class StubPowerPointRenderService : IPowerPointRenderService
    {
        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PowerPointRenderResult(
                PowerPointRenderErrorKind.MissingOffice, Slide: null, ErrorMessage: "stub", FromCache: false, Elapsed: TimeSpan.Zero));

        public void ClearCache()
        {
        }
    }

    private sealed class RecordingSafetyPrompt : ILiveSafetyPrompt
    {
        public RecordingSafetyPrompt(bool allow) => Allow = allow;

        public bool Allow { get; set; }
        public List<LiveSafetyRequest> Requests { get; } = new();

        public Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(Allow);
        }
    }

    private sealed class FixedDisplayService : IDisplayService
    {
        private readonly IReadOnlyList<OutputDisplay> _displays;

        public FixedDisplayService(params OutputDisplay[] displays) => _displays = displays;

        public IReadOnlyList<OutputDisplay> GetDisplays() => _displays;

        public OutputDisplay GetPrimaryDisplay()
            => _displays.FirstOrDefault(display => display.IsPrimary) ?? _displays[0];

        public OutputDisplay GetPreferredDisplay(string? preferredDisplayId = null)
        {
            if (!string.IsNullOrWhiteSpace(preferredDisplayId))
            {
                var preferred = _displays.FirstOrDefault(display => display.Id == preferredDisplayId);
                if (preferred is not null)
                {
                    return preferred;
                }
            }

            return _displays.FirstOrDefault(display => !display.IsPrimary) ?? GetPrimaryDisplay();
        }
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_MainSettings_{Guid.NewGuid():N}"));

        public static ISettingsService CreateDetachedSettings()
        {
            var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_MainSettings_{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            return new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups")));
        }

        public ISettingsService CreateSettings()
            => new SettingsService(new SettingsServiceOptions(
                Path.Combine(Root, "settings.json"),
                Path.Combine(Root, "Backups")));

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
