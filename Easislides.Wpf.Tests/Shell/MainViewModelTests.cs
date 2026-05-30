using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
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
    public async Task ApplySelectedItemContent_NonMediaItem_UnloadsPreviousMedia()
    {
        // 출력 패리티: 미디어 재생 후 다른 종류 항목 선택 시 직전 미디어를 완전히 내린다(Unload).
        // Stop(정지 후 첫 프레임 잔류)이 아니라 Empty 상태가 돼야 출력 창에서 영상이 사라지고
        // 그 아래 가사가 다시 보인다(실제 미디어 백엔드 트랙 3단계).
        var sut = CreateSut();
        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("media:1", "Intro", "Media") { ContentPath = @"C:\media\intro.mp4" });
        sut.Media.State.Should().Be(MediaPlaybackState.Ready);

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("song:1", "Song", "Song") { Lyrics = "x" });

        sut.Media.State.Should().Be(MediaPlaybackState.Empty, "비-미디어 항목 선택 시 직전 미디어를 내림(가사 복귀)");
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
    public async Task GoLive_PowerPointItemWithRenderedSlide_ProjectsSlideToOutput()
    {
        // G1.2 출력 송출: 렌더된 PPT 슬라이드가 운영자 미리보기뿐 아니라 GoLive 시 출력 창에도 송출돼야 한다
        // (지금까지는 출력엔 타이틀만 나갔음 — PreviewSource 가 프로덕션에서 설정되지 않았기 때문).
        var expectedSlide = new DrawingImage();
        expectedSlide.Freeze();
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => expectedSlide);
        await powerPoint.LoadAsync("deck.pptx", 1, 960, 540);
        powerPoint.State.Should().Be(PowerPointPreviewState.Ready);

        var sut = CreateSut(powerPoint: powerPoint);
        var ppt = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };
        sut.LoadQueue(new[] { ppt });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = ppt;

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemPreviewSource.Should().BeSameAs(expectedSlide,
            "PPT 항목 GoLive 시 렌더된 슬라이드가 출력 송출 슬롯에 실려야 함");
        sut.Session.Current.CurrentItemPreviewFillMode.Should().Be(ImageFillMode.Fit, "PPT 슬라이드는 레터박스(Fit) 송출");
    }

    [Fact]
    public async Task GoLive_PowerPointItemWithoutReadyRender_ProjectsTitleOnly()
    {
        // 렌더 미준비(실패·미적재)면 기존 동작 유지 — 출력엔 타이틀만(슬라이드 미송출).
        var sut = CreateSut(); // 기본 StubPowerPointRenderService → 렌더 실패(State=Failed)
        var ppt = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };
        sut.LoadQueue(new[] { ppt });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = ppt;

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemPreviewSource.Should().BeNull("렌더 미준비면 슬라이드를 송출하지 않음(타이틀만)");
    }

    [Fact]
    public async Task GoLive_PowerPointItem_WhileRenderInFlight_ProjectsTitleNotStaleSlide()
    {
        // 라이브 중 빠른 전환 경쟁: 항목 선택 시 렌더는 fire-and-forget 으로 돌아가므로 송출 시점에
        // 아직 렌더가 끝나지 않았을 수 있다. 이때 (이전 항목의) stale 슬라이드가 아니라 타이틀만 나가야 한다.
        using var gate = new GatedPowerPointRenderService();
        var powerPoint = new PowerPointPreviewViewModel(gate, _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        var ppt = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };
        sut.LoadQueue(new[] { ppt });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = ppt; // 렌더 시작 → 게이트에 막혀 Rendering 상태로 멈춤

        await sut.GoLiveCommand.ExecuteAsync(null);

        powerPoint.State.Should().Be(PowerPointPreviewState.Rendering, "게이트로 렌더가 미완 상태여야 경쟁을 재현");
        sut.Session.Current.CurrentItemPreviewSource.Should().BeNull("렌더 미완 중엔 슬라이드를 송출하지 않음(타이틀만)");

        gate.Release(); // 정리 — 대기 중 렌더 완료시켜 누수 방지
    }

    [Fact]
    public async Task SelectingPowerPoint_WithOutputOpen_RendersAtOutputResolution()
    {
        // G1.2 후속(출력 해상도 렌더): 출력 창이 열려 있으면 PPT 를 출력 모니터 해상도로 렌더해
        // 송출을 선명하게 한다(기존엔 미리보기 960×540 을 업스케일 → 흐림).
        var render = new RecordingPowerPointRenderService();
        var sut = CreateSut(powerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()));
        sut.SelectedOutputDisplay = new OutputDisplay("d", "Display", 0, 0, 1920, 1080, 1.0);
        sut.OpenOutputCommand.Execute(null);

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });

        render.LastRequest!.PixelWidth.Should().Be(1920);
        render.LastRequest.PixelHeight.Should().Be(1080);
    }

    [Fact]
    public async Task SelectingPowerPoint_WithOutputClosed_RendersAtPreviewResolution()
    {
        // 출력 창이 닫혀 있으면(미송출) 가벼운 미리보기 크기로 렌더한다.
        var render = new RecordingPowerPointRenderService();
        var sut = CreateSut(powerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()));

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });

        render.LastRequest!.PixelWidth.Should().Be(960);
        render.LastRequest.PixelHeight.Should().Be(540);
    }

    [Fact]
    public async Task SelectingPowerPoint_With4kOutput_ClampsRenderTo1080p()
    {
        // 초고해상도(4K) 출력에서 매 선택마다 거대한 JPG 를 만들지 않도록 1080p 로 상한.
        var render = new RecordingPowerPointRenderService();
        var sut = CreateSut(powerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()));
        sut.SelectedOutputDisplay = new OutputDisplay("d", "4K", 0, 0, 3840, 2160, 1.0);
        sut.OpenOutputCommand.Execute(null);

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });

        render.LastRequest!.PixelWidth.Should().Be(1920);
        render.LastRequest.PixelHeight.Should().Be(1080);
    }

    [Fact]
    public async Task SelectingPowerPoint_WithNon16by9Output_PreservesAspectRatioWithinBounds()
    {
        // 비-16:9(16:10) 출력: 두 축을 따로 자르지 않고 종횡비를 보존하며 1080p 상한 안에 맞춘다.
        var render = new RecordingPowerPointRenderService();
        var sut = CreateSut(powerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()));
        sut.SelectedOutputDisplay = new OutputDisplay("d", "16:10", 0, 0, 3840, 2400, 1.0);
        sut.OpenOutputCommand.Execute(null);

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });

        var width = render.LastRequest!.PixelWidth;
        var height = render.LastRequest.PixelHeight;
        width.Should().BeLessThanOrEqualTo(1920);
        height.Should().BeLessThanOrEqualTo(1080);
        ((double)width / height).Should().BeApproximately(3840.0 / 2400.0, 0.01, "종횡비(16:10) 보존");
    }

    [Fact]
    public async Task OpeningOutput_AfterSelectingPowerPoint_RerendersAtOutputResolution()
    {
        // 항목을 먼저 고르고 출력을 나중에 여는 흐름: 출력 열림 시 현재 PPT 를 출력 해상도로 다시 렌더한다
        // (그렇지 않으면 직전 렌더가 미리보기 크기로 남아 송출이 흐림 — G1.2 후속 완성).
        var render = new RecordingPowerPointRenderService();
        var sut = CreateSut(powerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()));
        var ppt = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };
        sut.LoadQueue(new[] { ppt });
        sut.SelectedItem = ppt; // 출력 닫힘 → 960×540 렌더
        render.LastRequest!.PixelWidth.Should().Be(960, "출력 닫힘 상태 선택은 미리보기 크기");

        sut.SelectedOutputDisplay = new OutputDisplay("d", "Display", 0, 0, 1920, 1080, 1.0);
        sut.OpenOutputCommand.Execute(null); // 출력 열림 → 현재 PPT 재렌더
        await Task.Yield();

        render.LastRequest!.PixelWidth.Should().Be(1920, "출력 열림 시 출력 해상도로 재렌더");
        render.LastRequest.PixelHeight.Should().Be(1080);
    }

    [Fact]
    public void OpeningOutput_WithNonPowerPointSelected_DoesNotRerender()
    {
        // 가드 회귀: 비-PPT(곡) 항목이 선택돼 있으면 출력 열림이 PPT 재렌더를 유발하지 않는다.
        var render = new RecordingPowerPointRenderService();
        var sut = CreateSut(powerPoint: new PowerPointPreviewViewModel(render, _ => Frozen()));
        var song = new LiveQueueItem("song:1", "찬양", "Song") { Lyrics = "x" };
        sut.LoadQueue(new[] { song });
        sut.SelectedItem = song;

        sut.OpenOutputCommand.Execute(null);

        render.LastRequest.Should().BeNull("비-PPT 선택 시 출력 열림은 PPT 렌더를 유발하지 않음");
    }

    [Fact]
    public async Task GoLive_SecondPowerPointItem_ProjectsItsOwnSlideNotPrevious()
    {
        // 긍정 전환(신원 일치 분기): 두 번째 PPT 항목 송출 시 이전 덱(A)이 아니라 그 항목(B)의
        // 현재 렌더 슬라이드가 출력에 실려야 한다. 동기 렌더라 선택 즉시 Ready(해당 덱)가 된다.
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        var deckA = new LiveQueueItem("ppt:a", "Deck A", "PowerPoint") { ContentPath = "deckA.pptx" };
        var deckB = new LiveQueueItem("ppt:b", "Deck B", "PowerPoint") { ContentPath = "deckB.pptx" };
        sut.LoadQueue(new[] { deckA, deckB });
        sut.OpenOutputCommand.Execute(null);

        sut.SelectedItem = deckA;
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.SelectedItem = deckB;
        await sut.GoLiveCommand.ExecuteAsync(null);

        powerPoint.LoadedContentPath.Should().Be("deckB.pptx", "전환 후 B 가 로드됨");
        sut.Session.Current.CurrentItemPreviewSource.Should().BeSameAs(powerPoint.PreviewImage,
            "두 번째 PPT 송출 시 그 항목(B)의 현재 슬라이드가 출력에 실려야 함(A stale 아님)");
    }

    [Fact]
    public async Task GoLive_PowerPointAliasKind_DoesNotProjectPreviouslyRenderedSlide()
    {
        // "PowerPoint" 정규값만 렌더가 디스패치되지만 IsPowerPointItem 은 별칭("PPT"/"P")도 받는다.
        // 별칭 항목 선택 시엔 렌더가 안 돌고 미리보기가 비워지므로(Clear), 이전에 렌더된 다른 덱의
        // 슬라이드가 별칭 항목 타이틀 아래로 잘못 송출되면 안 된다.
        var firstSlide = Frozen();
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => firstSlide);
        await powerPoint.LoadAsync("deckA.pptx", 1, 960, 540); // 이전 덱 렌더 완료(Ready)
        powerPoint.State.Should().Be(PowerPointPreviewState.Ready);

        var sut = CreateSut(powerPoint: powerPoint);
        var alias = new LiveQueueItem("ppt:alias", "Deck B", "PPT") { ContentPath = "deckB.pptx" };
        sut.LoadQueue(new[] { alias });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = alias;

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemPreviewSource.Should().BeNull("별칭 항목은 이전 덱 슬라이드를 송출하지 않음(신원 불일치)");
    }

    [Fact]
    public async Task NextSlideCommand_AdvancesPreviewToNextSlide()
    {
        // 라이브 슬라이드 이동: 선택된 PPT 미리보기를 다음 슬라이드로 다시 렌더(SuccessStub 은 SlideCount=3).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        powerPoint.SlideNumber.Should().Be(1);

        await sut.NextSlideCommand.ExecuteAsync(null);

        powerPoint.SlideNumber.Should().Be(2);
    }

    [Fact]
    public void SlideNavCommands_RespectDeckBoundaries()
    {
        // 첫 슬라이드에선 이전 비활성, 마지막 슬라이드에선 다음 비활성(덱 범위 벗어남 방지).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });

        powerPoint.SlideNumber.Should().Be(1);
        sut.PreviousSlideCommand.CanExecute(null).Should().BeFalse("첫 슬라이드에선 이전 비활성");
        sut.NextSlideCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task NextSlideCommand_WhileLive_UpdatesOutputToNewSlide()
    {
        // 라이브 송출 중 슬라이드 이동 시 출력도 새 슬라이드로 즉시 갱신된다.
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemPreviewSource.Should().NotBeNull();

        await sut.NextSlideCommand.ExecuteAsync(null);

        powerPoint.SlideNumber.Should().Be(2);
        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentItemPreviewSource.Should().BeSameAs(powerPoint.PreviewImage,
            "라이브 중 슬라이드 이동 시 출력이 새 슬라이드로 갱신");
    }

    [Fact]
    public async Task NextSlideCommand_WhenNotLive_UpdatesPreviewButNotSession()
    {
        // 라이브가 아니면 미리보기만 갱신하고 세션(출력)은 건드리지 않는다.
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });

        await sut.NextSlideCommand.ExecuteAsync(null);

        powerPoint.SlideNumber.Should().Be(2, "미리보기는 갱신");
        sut.Session.Current.State.Should().Be(LiveState.Off, "라이브가 아니면 세션 변화 없음");
    }

    [Fact]
    public async Task SelectingPowerPoint_LoadsThumbnailStrip()
    {
        // 덱 선택 시 PowerPoint 탭 썸네일 스트립이 덱 슬라이드 수만큼 채워진다(SuccessStub=3장).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);

        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });

        powerPoint.Thumbnails.Should().HaveCount(3, "덱 슬라이드 수(3)만큼 썸네일 로드");
    }

    [Fact]
    public async Task ThumbnailStrip_ReloadsOnDeckSwitchButNotOnSameDeck()
    {
        // 같은 덱 재선택(슬라이드 이동/출력열기 재렌더 포함)엔 스트립을 재로드하지 않고,
        // 다른 덱으로 전환하면 새로 채운다(_thumbnailDeckPath 가드).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" });
        var firstStrip = powerPoint.Thumbnails.ToArray();
        firstStrip.Should().HaveCount(3);

        // 같은 덱(같은 ContentPath, 다른 항목 인스턴스) 재선택 → 재로드 안 함(인스턴스 유지)
        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:1b", "Deck again", "PowerPoint") { ContentPath = "deck.pptx" });
        powerPoint.Thumbnails.Should().Equal(firstStrip, "같은 덱이면 스트립 재로드 안 함");

        // 다른 덱 전환 → 재로드(새 인스턴스)
        await sut.ApplySelectedItemContentAsync(
            new LiveQueueItem("ppt:2", "Other", "PowerPoint") { ContentPath = "other.pptx" });
        powerPoint.Thumbnails.Should().NotEqual(firstStrip, "다른 덱이면 스트립 재로드");
    }

    [Fact]
    public async Task GoToSlideCommand_NavigatesToClickedSlide()
    {
        // 썸네일 클릭(슬라이드 번호 인자) → 해당 슬라이드로 이동.
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        powerPoint.SlideNumber.Should().Be(1);

        await sut.GoToSlideCommand.ExecuteAsync(3);

        powerPoint.SlideNumber.Should().Be(3);
    }

    [Fact]
    public async Task PreviousSlideCommand_GoesBackToPriorSlide()
    {
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        await sut.NextSlideCommand.ExecuteAsync(null);
        powerPoint.SlideNumber.Should().Be(2);

        await sut.PreviousSlideCommand.ExecuteAsync(null);

        powerPoint.SlideNumber.Should().Be(1);
    }

    [Fact]
    public async Task SlideNav_WhenSelectionDivergesFromLiveItem_IsDisabled()
    {
        // 라이브 항목과 다른 항목을 선택하면(선택이 라이브에서 벗어남) 슬라이드 이동 비활성 —
        // 라이브 덱이 아닌 항목을 넘겨 라이브 출력이 안 바뀌는 혼란 방지(code-review #1).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        var deckA = new LiveQueueItem("ppt:a", "Deck A", "PowerPoint") { ContentPath = "deckA.pptx" };
        var deckB = new LiveQueueItem("ppt:b", "Deck B", "PowerPoint") { ContentPath = "deckB.pptx" };
        sut.LoadQueue(new[] { deckA, deckB });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = deckA;
        await sut.GoLiveCommand.ExecuteAsync(null); // 라이브 = A

        sut.SelectedItem = deckB; // 선택이 B 로 분기

        sut.NextSlideCommand.CanExecute(null).Should().BeFalse("라이브 항목이 아닌 선택에선 슬라이드 이동 비활성");
        sut.PreviousSlideCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task SlideNav_DuringBlackout_ThenResume_ShowsNavigatedSlide()
    {
        // 블랙아웃 중 슬라이드 이동 후 송출 재개(GO) 시, 이동 전 슬라이드나 타이틀이 아니라
        // 이동한 슬라이드가 송출돼야 한다(code-review #2 — 항목 SlideNumber 미반영으로 인한 타이틀 강등 방지).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);    // 라이브 슬라이드 1
        await sut.BlackScreenCommand.ExecuteAsync(null); // 블랙아웃(Hidden)
        sut.Session.Current.State.Should().Be(LiveState.Hidden);

        await sut.NextSlideCommand.ExecuteAsync(null);   // 슬라이드 2 로 이동(Hidden 이라 출력 갱신 스킵)
        powerPoint.SlideNumber.Should().Be(2);

        await sut.GoLiveCommand.ExecuteAsync(null);       // 재개 → 현재 슬라이드(2) 송출

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentItemPreviewSource.Should().BeSameAs(powerPoint.PreviewImage,
            "재개 시 이동한 슬라이드(2)가 송출(타이틀 강등 아님)");
    }

    [Fact]
    public async Task GoLive_PowerPointDeck_WithAdvanceNextItem_KeepsSelectionForSlideNav()
    {
        // AdvanceNextItem 이 켜져 있어도 PPT 덱 송출 시 다음 항목으로 자동 이동하지 않는다 —
        // 선택이 라이브 덱에 머물러 그 자리에서 슬라이드를 넘길 수 있어야 하므로(code-review #1/AdvanceNextItem).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(settings: settings, powerPoint: powerPoint);
        var deck = new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" };
        var next = new LiveQueueItem("song:1", "다음 곡", "Song") { Lyrics = "x" };
        sut.LoadQueue(new[] { deck, next });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = deck;

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.SelectedItem.Should().Be(deck, "PPT 덱은 자동 다음-항목 이동에서 제외");
        sut.NextSlideCommand.CanExecute(null).Should().BeTrue("라이브 PPT 덱에서 슬라이드 이동 활성 유지");

        await sut.NextSlideCommand.ExecuteAsync(null);

        powerPoint.SlideNumber.Should().Be(2);
        sut.Session.Current.CurrentItemPreviewSource.Should().BeSameAs(powerPoint.PreviewImage,
            "라이브 PPT 슬라이드 이동이 출력을 갱신");
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

    [Fact]
    public void ApplyOutputAppearanceCommand_WritesColorSettingsAndSetsActiveName()
    {
        // 인-셸 출력 모양 인스펙터(§7.5 P0): 프리셋 적용 시 글자색·배경색·그라데이션 설정을 한 번에 쓰고
        // 활성 모양 이름을 갱신한다(설정→출력 VM 으로 라이브 반영).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var navy = sut.OutputAppearancePresets.Single(p => p.Name.Contains("네이비"));

        sut.ApplyOutputAppearanceCommand.Execute(navy);

        settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(navy.TextArgb);
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb).Should().Be(navy.Background1Argb);
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb).Should().Be(navy.Background2Argb);
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient).Should().Be(navy.IsGradient);
        sut.ActiveAppearanceName.Should().Be(navy.Name);
    }

    [Fact]
    public void ActiveAppearanceName_ReflectsCurrentSettingsOnLoad()
    {
        // 기본 설정(글자 검정·배경 흰색·그라데이션 off)은 "검정 글자 · 흰 배경" 프리셋과 일치.
        using var folder = TempSettingsFolder.Create();
        var sut = CreateSut(settings: folder.CreateSettings());

        sut.ActiveAppearanceName.Should().Be("검정 글자 · 흰 배경");
    }

    [Fact]
    public void ActiveAppearanceName_WhenSettingsDoNotMatchAnyPreset_IsCustom()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFF123456)); // 프리셋에 없는 색

        var sut = CreateSut(settings: settings);

        sut.ActiveAppearanceName.Should().Be("사용자 지정");
    }

    [Fact]
    public void AddSelectedLibrarySongCommand_AddsLibrarySelectedSongToQueue()
    {
        // 인라인 콘텐츠 브라우저(§7.5 P0): 별도 LibraryWindow 없이 라이브러리 선택 곡을 예배 순서에 추가.
        var sut = CreateSut();
        sut.Library.SelectedSong = new SongSummary(42, "은혜로다", "", 1, 1, "찬양", "G", "1절 가사");

        sut.AddSelectedLibrarySongCommand.Execute(null);

        sut.Queue.Should().Contain(i => i.Id == "song:42" && i.Title == "은혜로다" && i.Lyrics == "1절 가사");
        sut.SelectedItem!.Id.Should().Be("song:42", "추가한 곡이 선택됨");
    }

    [Fact]
    public void AddBibleSelection_WithEmptyBuildSelection_DoesNotChangeQueue()
    {
        // 인라인 성경 글루 안전성(§7.5 P0): 본문/구절 미선택이면 BuildSelection 이 빈 결과를 내고,
        // AddBibleSelection 이 그것을 무시해 큐가 바뀌지 않는다(미로드 성경에서 BuildSelection 안전).
        var sut = CreateSut();
        var before = sut.Queue.Count;

        var empty = sut.Bible.BuildSelection(0, 0); // 성경 미로드 → 빈 선택
        sut.AddBibleSelection(empty);

        sut.Queue.Count.Should().Be(before, "빈 성경 선택은 예배 순서를 바꾸지 않음");
    }

    [Fact]
    public void Exposes_Library_And_Bible_ViewModels_ForInlineBrowsers()
    {
        // §7.5 P0 인라인 콘텐츠 브라우저: MainWindow 좌측 "라이브러리"·"성경" 탭이 바인딩할 VM 노출(DI 배선 잠금).
        var sut = CreateSut();

        sut.Library.Should().NotBeNull("라이브러리 탭이 바인딩할 VM");
        sut.Bible.Should().NotBeNull("성경 탭이 바인딩할 VM");
    }

    [Fact]
    public void AddSelectedLibrarySongCommand_CanExecute_ReflectsLibrarySelection()
    {
        var sut = CreateSut();
        sut.AddSelectedLibrarySongCommand.CanExecute(null).Should().BeFalse("초기엔 라이브러리 선택 곡 없음");

        sut.Library.SelectedSong = new SongSummary(1, "곡", "", 1, 1, "", "", "");

        sut.AddSelectedLibrarySongCommand.CanExecute(null).Should().BeTrue("곡 선택 시 추가 활성");
    }

    private static MainViewModel CreateSut(
        ILiveSafetyPrompt? prompt = null,
        IDisplayService? display = null,
        ICommandCatalog? commandCatalog = null,
        ISettingsService? settings = null,
        IWorshipListStore? worshipLists = null,
        PowerPointPreviewViewModel? powerPoint = null)
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var telemetry = new InMemoryCommandTelemetry();
        var media = new MediaPlaybackViewModel(new MediaPlaybackService());
        powerPoint ??= new PowerPointPreviewViewModel(new StubPowerPointRenderService());
        var resolvedSettings = settings ?? TempSettingsFolder.CreateDetachedSettings();
        // 라이브러리/성경 VM — 테스트는 작업 폴더/DB 미설정이라 실제 repo 가 데이터를 반환하지 않는다(빈 목록).
        var library = new LibraryViewModel(resolvedSettings, new AdminDatabaseRepository());
        var bible = new BibleViewModel(resolvedSettings, new BibleRepository());
        return new MainViewModel(
            session,
            output,
            prompt ?? new RecordingSafetyPrompt(allow: true),
            telemetry,
            display ?? new FixedDisplayService(OutputDisplay.PrimaryFallback),
            commandCatalog ?? new CommandCatalog(),
            resolvedSettings,
            media,
            powerPoint,
            library,
            bible,
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

    // 렌더 실패만 내는 스텁(대부분 테스트는 PPT 렌더 성공이 필요 없음 — 실패 시 타이틀 송출 경로).
    private sealed class StubPowerPointRenderService : IPowerPointRenderService
    {
        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PowerPointRenderResult(
                PowerPointRenderErrorKind.MissingOffice, Slide: null, ErrorMessage: "stub", FromCache: false, Elapsed: TimeSpan.Zero));

        public void ClearCache()
        {
        }
    }

    // 렌더 성공을 내는 스텁 — 슬라이드 출력 송출(G1.2) 경로 검증용.
    private sealed class SuccessPowerPointRenderService : IPowerPointRenderService
    {
        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(SuccessResult(request));

        public void ClearCache()
        {
        }
    }

    // 렌더를 게이트로 붙잡아 두는 스텁 — Release() 전까지 미완(Rendering) 상태를 재현(경쟁 테스트용).
    private sealed class GatedPowerPointRenderService : IPowerPointRenderService, IDisposable
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release() => _gate.TrySetResult();

        public async Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            await _gate.Task.ConfigureAwait(false);
            return SuccessResult(request);
        }

        public void ClearCache()
        {
        }

        public void Dispose() => _gate.TrySetResult();
    }

    // 렌더 요청(특히 픽셀 크기)을 기록하는 스텁 — 출력 해상도 렌더(G1.2 후속) 검증용.
    private sealed class RecordingPowerPointRenderService : IPowerPointRenderService
    {
        public PowerPointRenderRequest? LastRequest { get; private set; }

        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new PowerPointRenderResult(
                PowerPointRenderErrorKind.MissingOffice, Slide: null, ErrorMessage: "rec", FromCache: false, Elapsed: TimeSpan.Zero));
        }

        public void ClearCache()
        {
        }
    }

    private static PowerPointRenderResult SuccessResult(PowerPointRenderRequest request)
        => new(
            PowerPointRenderErrorKind.None,
            new PowerPointSlideSnapshot(
                request.FilePath, request.SlideNumber, SlideCount: 3,
                request.PixelWidth, request.PixelHeight,
                ImageBytes: [1, 2, 3], ContentType: "image/jpeg", DateTimeOffset.UnixEpoch),
            ErrorMessage: null, FromCache: false, Elapsed: TimeSpan.Zero);

    // 테스트용 frozen ImageSource(텍스트/STA 불필요한 DrawingImage — 디코더 주입으로 PreviewImage 대체).
    private static ImageSource Frozen()
    {
        var image = new DrawingImage();
        image.Freeze();
        return image;
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
