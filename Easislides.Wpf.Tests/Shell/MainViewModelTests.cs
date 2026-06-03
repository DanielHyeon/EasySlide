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
    public void CreateSut_SeedsSampleQueue_ForTestConvenience()
    {
        // CreateSut 는 기본으로 샘플 3항목을 시드한다(운영 생성자는 빈 큐로 시작 — 별도 테스트로 검증).
        var sut = CreateSut();

        sut.StatusText.Should().Be("3개 항목 로드됨");
        sut.Queue.Select(item => item.Title)
            .Should()
            .Contain(["예배 시작 안내", "주일 찬양 #1", "말씀 본문"]);
    }

    [Fact]
    public void Constructor_StartsWithEmptyQueue_NoDummySeed()
    {
        // §7 P0: 시작 시 더미 3항목 시드를 제거 — 빈 큐로 시작하고 안내 상태를 노출한다.
        var sut = CreateSut(seedSampleQueue: false);

        sut.Queue.Should().BeEmpty();
        sut.IsQueueEmpty.Should().BeTrue();
        sut.StatusText.Should().Be("WPF 운영 준비됨", "더미 시드 없이 기본 상태 유지");
    }

    [Fact]
    public void IsQueueEmpty_TogglesAsItemsAddedAndCleared()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.IsQueueEmpty.Should().BeTrue();

        sut.LoadQueue(new[] { new LiveQueueItem("a", "곡A", LiveItemKinds.Song) });
        sut.IsQueueEmpty.Should().BeFalse("항목이 들어오면 비어 있지 않음");

        sut.LoadQueue([]);
        sut.IsQueueEmpty.Should().BeTrue("모두 비우면 다시 비어 있음");
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
    public async Task LiveItemId_ReflectsPublishedItem_AndNotifiesAndClearsOnStop()
    {
        // 예배 순서 목록의 LIVE 표시 바인딩 소스 — 송출하면 그 항목 Id, 라이브 중지하면 null. 변경 시 PropertyChanged 알림.
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:a", "곡A");
        var b = new LiveQueueItem("song:b", "곡B");
        sut.LoadQueue([a, b]);
        sut.LiveItemId.Should().BeNull("송출 전엔 라이브 항목 없음");

        var notified = new System.Collections.Generic.List<string?>();
        sut.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(MainViewModel.LiveItemId)) notified.Add(sut.LiveItemId); };

        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = a;
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LiveItemId.Should().Be("song:a", "송출한 항목 Id 가 라이브 항목");
        notified.Should().Contain("song:a", "라이브 항목 변경이 통지됨(LIVE 표시 갱신)");

        await sut.StopLiveCommand.ExecuteAsync(null);

        sut.LiveItemId.Should().BeNull("라이브 중지하면 라이브 항목 해제");
        notified.Should().Contain((string?)null, "해제도 통지됨");
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
    public void ResetAllSettingsToDefaults_RevertsChangedSettings_AndReturnsTrue()
    {
        // 증분159 — 레거시 Tools "Clear Registry Settings" 대응. 바꿔 둔 설정이 기본값으로 되돌아오고(디스크 저장 포함)
        // 성공을 알려야 한다(호출부 View 는 이 결과로 재시작 여부를 정한다).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var defaultValue = settings.Get(EasiSettingKeys.AdvanceNextItem); // 손대기 전 기본값.
        settings.Set(EasiSettingKeys.AdvanceNextItem, !defaultValue).Succeeded.Should().BeTrue(); // 기본값과 반대로 바꿔 둔다.
        settings.Get(EasiSettingKeys.AdvanceNextItem).Should().Be(!defaultValue);
        var sut = CreateSut(settings: settings);

        var ok = sut.ResetAllSettingsToDefaults();

        ok.Should().BeTrue("기본값 복원이 성공해야 함");
        settings.Get(EasiSettingKeys.AdvanceNextItem).Should().Be(defaultValue, "초기화로 바꿨던 설정이 기본값으로 되돌아와야 함");
    }

    [Fact]
    public async Task SendToOutputAndNext_PublishesThenAdvances_EvenWhenAutoAdvanceOff()
    {
        // btnToOutputMoveNext: 자동 다음 설정이 꺼져 있어도 송출 후 선택이 다음 항목으로 이동한다.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);

        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("입례 찬양", "송출된 건 첫 항목");
        sut.SelectedItem.Should().Be(second, "자동 다음 off 여도 선택이 다음으로 이동");
    }

    [Fact]
    public async Task SendToOutputAndNext_LastItem_PublishesButStays()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        var only = new LiveQueueItem("song-1", "마지막 찬양");
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { only });
        sut.OpenOutputCommand.Execute(null);

        await sut.SendToOutputAndNextCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("마지막 찬양");
        sut.SelectedItem.Should().Be(only, "마지막 항목이면 그대로 머문다");
    }

    [Fact]
    public async Task GoLive_CarriesNextQueueItemTitleToSession()
    {
        // Display Panel PrevNext: 첫 항목 송출 시 큐의 다음 항목 제목이 세션 스냅샷에 실려야 한다.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        // 자동 다음-항목 이동은 끄고(선택이 첫 항목에 머물도록) 다음 항목 제목 계산만 검증.
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        var first = new LiveQueueItem("song-1", "입례 찬양");
        var second = new LiveQueueItem("song-2", "봉헌 찬양");
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { first, second });
        sut.OpenOutputCommand.Execute(null);

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemNextTitle.Should().Be("봉헌 찬양");
    }

    [Fact]
    public async Task ToggleShowNotations_WhileLive_RepublishesBodyWithChords()
    {
        // 운영 중 "코드 표시"를 켜면 — 색·정렬처럼 렌더만 바뀌는 게 아니라 본문 자체가 달라지므로 —
        // 현재 라이브 곡이 같은 절로 재송출되며 가사 위에 코드 줄이 붙어야 한다(라이브 즉시 반영).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, false).Succeeded.Should().BeTrue();
        var song = new LiveQueueItem("song-1", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "[1]\nAmazing grace » G  C\nHow sweet » D7",
        };
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { song });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);

        // off(기본) → 코드 숨김.
        sut.Session.Current.CurrentItemBodyText.Should().Be("Amazing grace\nHow sweet");

        // 운영 중 토글 → 같은 곡 재송출, 코드 줄이 가사 위에.
        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();

        sut.Session.Current.CurrentItemBodyText.Should().Be("G  C\nAmazing grace\nD7\nHow sweet");
    }

    [Fact]
    public async Task ToggleShowNotations_AfterSelectionMovedOffLive_RepublishesAtLiveVerseNotSelected()
    {
        // MAJOR 회귀 방지: 송출 후 선택이 다음 항목으로 자동 이동(AdvanceNextItem)하면 VM 의 LyricsPageIndex 는
        // 새 선택을 따라 0 으로 리셋된다. "코드 표시" 토글 시 재송출이 VM 값을 쓰면 라이브가 0절로 튄다 →
        // 세션의 실제 라이브 절(CurrentLyricsPageIndex)을 써야 한다. 라이브 곡이 2절에 머물러 있는지 검증.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, false).Succeeded.Should().BeTrue();
        var songA = new LiveQueueItem("song-A", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n1절 » C\n[2]\n2절 » G",
        };
        var songB = new LiveQueueItem("song-B", "다음 곡", LiveItemKinds.Song) { Lyrics = "[1]\n다른 가사" };
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { songA, songB });
        sut.OpenOutputCommand.Execute(null);

        sut.NextLyricsPageCommand.Execute(null); // songA 2절로 이동(LyricsPageIndex=1).
        await sut.GoLiveCommand.ExecuteAsync(null); // songA@2절 송출 → 선택은 songB 로 자동 이동, LyricsPageIndex=0 리셋.
        sut.SelectedItem.Should().Be(songB);
        sut.Session.Current.CurrentItemBodyText.Should().Be("2절", "라이브는 songA 2절(코드 off)");

        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();

        // 재송출이 선택(songB·0절)이 아니라 라이브(songA·2절)를 코드 포함으로 다시 그려야 한다.
        sut.Session.Current.CurrentItemBodyText.Should().Be("G\n2절");
    }

    [Fact]
    public async Task TransposeLiveUp_WhileLiveWithNotations_ShiftsLiveChords()
    {
        // 코드 표시 on 으로 라이브 중에 "반음 올림"을 누르면 라이브 곡이 재송출되며 코드가 한 칸 올라간다.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();
        var song = new LiveQueueItem("song-1", "은혜로다", LiveItemKinds.Song) { Lyrics = "[1]\nAmazing grace » C  G" };
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { song });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemBodyText.Should().Be("C  G\nAmazing grace", "원조");

        sut.TransposeLiveUpCommand.Execute(null);

        sut.LiveTransposeSemitones.Should().Be(1);
        sut.LiveTransposeLabel.Should().Be("조옮김 +1");
        sut.Session.Current.CurrentItemBodyText.Should().Be("C#  G#\nAmazing grace", "반음 올림 후 송출 코드");
    }

    [Fact]
    public void TransposeLive_ClampsToElevenSemitones()
    {
        var sut = CreateSut();

        for (var i = 0; i < 20; i++)
        {
            sut.TransposeLiveUpCommand.Execute(null);
        }

        sut.LiveTransposeSemitones.Should().Be(11, "±11 반음으로 클램프");
    }

    [Fact]
    public async Task GoLive_ResetsTransposeToOriginalKey()
    {
        // 새 곡을 송출하면 조옮김이 원조(0)로 초기화되어 각 곡이 작성된 키에서 시작한다.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가사 » C" } });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.TransposeLiveUpCommand.Execute(null);
        sut.LiveTransposeSemitones.Should().Be(1);

        await sut.GoLiveCommand.ExecuteAsync(null); // 다시 송출(새 라이브) → 원조로 초기화.

        sut.LiveTransposeSemitones.Should().Be(0);
    }

    [Fact]
    public void ApplyBackgroundMode_PersistsAndUpdatesActiveAndMenuChecks()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.BackgroundModeIsFill.Should().BeTrue("기본은 채움");

        sut.ApplyBackgroundModeCommand.Execute(LyricsBackgroundMode.Tile);

        sut.ActiveBackgroundMode.Should().Be(LyricsBackgroundMode.Tile);
        sut.BackgroundModeIsTile.Should().BeTrue();
        sut.BackgroundModeIsFill.Should().BeFalse("이전 채움 체크 해제");
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundMode).Should().Be(LyricsBackgroundMode.Tile);
        sut.StatusText.Should().Contain("타일");
    }

    [Fact]
    public void ApplyRegionDisplay_PersistsAndUpdatesActiveAndMenuChecks()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.RegionDisplayIsBoth.Should().BeTrue("기본은 둘 다");

        sut.ApplyRegionDisplayCommand.Execute(LyricsRegionDisplay.Region2Only);

        sut.ActiveRegionDisplay.Should().Be(LyricsRegionDisplay.Region2Only);
        sut.RegionDisplayIsRegion2Only.Should().BeTrue();
        sut.RegionDisplayIsBoth.Should().BeFalse("이전 둘 다 체크 해제");
        settings.Get(EasiSettingKeys.LyricsMonitorRegionDisplay).Should().Be(LyricsRegionDisplay.Region2Only);
        sut.StatusText.Should().Contain("Region 2만");
    }

    [Fact]
    public void ValidateWorshipList_WithMissingFile_ReportsProblemAndStatus()
    {
        // 예배 순서에 깨진 PPT 파일이 있으면 검증이 문제로 잡아내고 상태바에 알린다(예배 중 사고 예방).
        var validator = new WorshipListValidator(_ => false); // 모든 파일이 없다고 가정.
        var sut = CreateSut(worshipValidator: validator);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song:1", "정상 곡", LiveItemKinds.Song),
            new LiveQueueItem("ppt:a", "찬양 PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\gone.pptx" },
        });

        sut.ValidateWorshipListCommand.Execute(null);

        sut.WorshipListProblems.Should().ContainSingle()
            .Which.Kind.Should().Be(WorshipItemProblemKind.FileNotFound);
        sut.StatusText.Should().Contain("문제 1건").And.Contain("찬양 PPT");
        sut.HasWorshipListProblems.Should().BeTrue("경고 패널 표시");
    }

    [Fact]
    public void ValidateWorshipList_AllValid_ReportsAllGood()
    {
        var validator = new WorshipListValidator(_ => true);
        var sut = CreateSut(worshipValidator: validator);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song:1", "곡", LiveItemKinds.Song),
            new LiveQueueItem("ppt:a", "PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\ok.pptx" },
        });

        sut.ValidateWorshipListCommand.Execute(null);

        sut.WorshipListProblems.Should().BeEmpty();
        sut.StatusText.Should().Contain("모든 항목 정상");
        sut.HasWorshipListProblems.Should().BeFalse("문제 없으면 경고 패널 숨김");
    }

    [Fact]
    public void ValidateWorshipList_RerunClearsPreviousProblems()
    {
        // 두 번째 검증이 깨끗하면 이전 문제 목록을 비워 누적되지 않는다.
        var sut = CreateSut(worshipValidator: new WorshipListValidator(_ => false));
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:a", "PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\gone.pptx" } });
        sut.ValidateWorshipListCommand.Execute(null);
        sut.WorshipListProblems.Should().ContainSingle();

        // 큐를 정상 항목만으로 바꾸고 다시 검증 → 문제 목록이 비워진다.
        sut.LoadQueue(new[] { new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) });
        sut.ValidateWorshipListCommand.Execute(null);

        sut.WorshipListProblems.Should().BeEmpty();
        sut.HasWorshipListProblems.Should().BeFalse("재검증이 깨끗하면 경고도 사라진다");
    }

    [Fact]
    public async Task ValidateWorshipList_SongMissingInDb_ReportsSongNotInDatabase()
    {
        // 곡 항목(song:{id})이 가사 DB 에 없으면 검증이 SongNotInDatabase 문제로 잡는다(레거시 DB 존재 검증, 증분98).
        var sut = CreateSut(songDetail: new StubSongDetailRepository(detail: null)); // 모든 곡이 DB 에 없다고 가정.
        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        sut.LoadQueue(new[] { new LiveQueueItem("song:42", "사라진 곡", LiveItemKinds.Song) });

        await sut.ValidateWorshipListAsync();

        sut.WorshipListProblems.Should().ContainSingle()
            .Which.Kind.Should().Be(WorshipItemProblemKind.SongNotInDatabase);
        sut.StatusText.Should().Contain("사라진 곡");
        sut.HasWorshipListProblems.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateWorshipList_SongExistsInDb_NoProblem()
    {
        // DB 에 곡이 있으면 문제 없음.
        var detail = SampleSongDetail(songId: 42, title: "은혜", lyrics: "1절");
        var sut = CreateSut(songDetail: new StubSongDetailRepository(detail));
        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        sut.LoadQueue(new[] { new LiveQueueItem("song:42", "은혜", LiveItemKinds.Song) });

        await sut.ValidateWorshipListAsync();

        sut.WorshipListProblems.Should().BeEmpty();
        sut.StatusText.Should().Contain("모든 항목 정상");
    }

    [Fact]
    public async Task ValidateWorshipList_NoDbPath_SkipsSongCheck()
    {
        // DB 경로가 없으면(설정 전) 곡 DB 검사를 건너뛴다 — 파일 검사만 하던 기존 동작 보존(무회귀).
        var sut = CreateSut(songDetail: new StubSongDetailRepository(detail: null)); // null=모두 없음이지만 DB 경로 없어 호출 안 됨
        // Search.DatabasePath 를 설정하지 않음(빈 값).
        sut.LoadQueue(new[] { new LiveQueueItem("song:42", "곡", LiveItemKinds.Song) });

        await sut.ValidateWorshipListAsync();

        sut.WorshipListProblems.Should().BeEmpty("DB 경로 없으면 곡 검사 생략 → 곡은 문제로 잡히지 않음");
        sut.StatusText.Should().Contain("곡 DB 검증 생략", "곡 DB 가 검증되지 않았음을 운영자에게 명시(조용한 생략 방지)");
    }

    [Fact]
    public void SelectWorshipProblemItem_SelectsOffendingQueueItem()
    {
        // 증분156 — 검증 문제를 클릭하면 그 항목이 예배 순서에서 선택돼 운영자가 바로 가 고친다.
        var sut = CreateSut(worshipValidator: new WorshipListValidator(_ => false));
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song:1", "정상 곡", LiveItemKinds.Song),
            new LiveQueueItem("ppt:a", "깨진 PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\gone.pptx" },
        });
        sut.ValidateWorshipListCommand.Execute(null);
        var problem = sut.WorshipListProblems.Should().ContainSingle().Subject;
        sut.SelectedItem = sut.Queue[0]; // 일부러 다른 항목을 선택해 둔다.

        sut.SelectWorshipProblemItemCommand.Execute(problem);

        sut.SelectedItem.Should().BeSameAs(sut.Queue[1], "문제의 PPT 항목이 선택됨");
        sut.StatusText.Should().Contain("깨진 PPT");
    }

    [Fact]
    public void SelectWorshipProblemItem_MatchesById_AfterInstanceReplaced()
    {
        // 검증 후 항목 인스턴스가 교체돼도(서식 편집 등) Id 로 찾아 같은 자리의 현재 인스턴스를 선택한다(참조 stale 회피).
        var sut = CreateSut(worshipValidator: new WorshipListValidator(_ => false));
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("ppt:a", "깨진 PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\gone.pptx" },
        });
        sut.ValidateWorshipListCommand.Execute(null);
        var problem = sut.WorshipListProblems.Single();
        // 같은 Id 의 새 인스턴스로 교체(검증 시 캡처한 참조와 달라짐).
        sut.Queue[0] = sut.Queue[0] with { Title = "깨진 PPT(수정본)" };

        sut.SelectWorshipProblemItemCommand.Execute(problem);

        sut.SelectedItem.Should().BeSameAs(sut.Queue[0], "Id 매칭으로 교체된 새 인스턴스를 선택");
    }

    [Fact]
    public void SelectWorshipProblemItem_RemovedItem_NoSelectionChange_AndInforms()
    {
        // 검증 후 그 항목을 큐에서 제거했다면, 클릭해도 선택은 바뀌지 않고 "이미 제거됨"을 알린다.
        var sut = CreateSut(worshipValidator: new WorshipListValidator(_ => false));
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song:1", "정상 곡", LiveItemKinds.Song),
            new LiveQueueItem("ppt:a", "깨진 PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\gone.pptx" },
        });
        sut.ValidateWorshipListCommand.Execute(null);
        var problem = sut.WorshipListProblems.Single();
        sut.Queue.RemoveAt(1); // 문제의 PPT 항목 제거.
        sut.SelectedItem = sut.Queue[0];

        sut.SelectWorshipProblemItemCommand.Execute(problem);

        sut.SelectedItem.Should().BeSameAs(sut.Queue[0], "제거된 항목 클릭은 선택을 바꾸지 않음");
        sut.StatusText.Should().Contain("이미 예배 순서에서 제거된 항목");
    }

    [Fact]
    public void SelectWorshipProblemItem_Null_IsNoOp()
    {
        // 파라미터가 null 이면(바인딩 경계) 아무 일도 안 한다(크래시 방지).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) });
        sut.SelectedItem = sut.Queue[0];

        sut.SelectWorshipProblemItemCommand.Execute(null);

        sut.SelectedItem.Should().BeSameAs(sut.Queue[0], "null 은 무동작");
    }

    [Fact]
    public void ToggleShowNotations_WhenNotLive_DoesNothing()
    {
        // 라이브가 아니면 재송출할 곡이 없어 설정만 바뀌고 세션은 Off 그대로다(안전).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", LiveItemKinds.Song) { Lyrics = "[1]\n가사 » C" } });

        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();

        sut.Session.Current.State.Should().Be(LiveState.Off);
    }

    [Fact]
    public async Task GoLive_LastQueueItem_CarriesEmptyNextTitle()
    {
        // 마지막 항목이면 다음 항목이 없어 빈 문자열 → 출력 미표시(무회귀).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.AdvanceNextItem, false).Succeeded.Should().BeTrue();
        var only = new LiveQueueItem("song-1", "축도 찬양");
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { only });
        sut.OpenOutputCommand.Execute(null);

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemNextTitle.Should().BeEmpty();
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

    // ─── 우측 출력 모양 인스펙터 접기/펼치기 (FrmMain식 가변 패널 — 접으면 중앙 확장) ───

    [Fact]
    public void IsInspectorExpanded_DefaultsTrue_AndToggles()
    {
        // 우측 인스펙터는 기본 펼침(FrmMain식 가변 패널). 토글로 접고 펼친다(접으면 중앙이 넓어짐).
        var sut = CreateSut();
        sut.IsInspectorExpanded.Should().BeTrue("기본은 펼침");

        sut.IsInspectorExpanded = false;
        sut.IsInspectorExpanded.Should().BeFalse("접기");
    }

    // ─── 중앙 미리보기 탭 자동 전환 (FrmMain식 멀티페인 — 항목 종류에 맞춰 탭 수동 전환 제거) ───

    [Fact]
    public void SelectingSongItem_SetsCenterTabToPreview()
    {
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "찬양", LiveItemKinds.Song) });

        sut.SelectedItem = sut.Queue[0];

        sut.SelectedContentTabIndex.Should().Be(0, "곡은 Preview 탭");
    }

    [Fact]
    public void SelectingPowerPointItem_SetsCenterTabToPowerPoint_WhenTabVisible()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.UsePowerPointTab, true).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings);
        sut.IsPowerPointTabVisible.Should().BeTrue();
        sut.LoadQueue(new[] { new LiveQueueItem("ppt-1", "Deck 1", LiveItemKinds.PowerPoint) });

        sut.SelectedItem = sut.Queue[0];

        sut.SelectedContentTabIndex.Should().Be(1, "PPT 선택 시 PowerPoint(미리보기+썸네일) 탭 자동 전환");
    }

    [Fact]
    public void SelectingPowerPointItem_FallsBackToPreview_WhenPowerPointTabHidden()
    {
        var sut = CreateSut(); // 기본 UsePowerPointTab=false → 탭 숨김
        sut.IsPowerPointTabVisible.Should().BeFalse();
        sut.LoadQueue(new[] { new LiveQueueItem("ppt-1", "Deck 1", LiveItemKinds.PowerPoint) });

        sut.SelectedItem = sut.Queue[0];

        sut.SelectedContentTabIndex.Should().Be(0, "PowerPoint 탭이 숨겨져 있으면 Preview 로 폴백");
    }

    [Fact]
    public void HidingPowerPointTabAtRuntime_WhileSelected_FallsBackToPreview()
    {
        // code-review MINOR: 운영 중 PowerPoint 탭을 끄면 선택이 숨은 탭(1)에 잔류해 중앙이 비어 보이면 안 됨.
        // 설정 변경 → ApplyOperationalSettings 가 가시성 갱신 후 현재 항목 기준 탭을 재평가해 Preview(0)로 폴백.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.UsePowerPointTab, true).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt-1", "Deck 1", LiveItemKinds.PowerPoint) });
        sut.SelectedItem = sut.Queue[0];
        sut.SelectedContentTabIndex.Should().Be(1);

        settings.Set(EasiSettingKeys.UsePowerPointTab, false).Succeeded.Should().BeTrue();

        sut.IsPowerPointTabVisible.Should().BeFalse();
        sut.SelectedContentTabIndex.Should().Be(0, "숨겨진 탭이 선택된 채 남으면 안 됨");
    }

    [Fact]
    public void SelectingMediaItem_SetsCenterTabToMedia_WhenTabVisible()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.UseMediaTab, true).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings);
        sut.IsMediaTabVisible.Should().BeTrue();
        sut.LoadQueue(new[] { new LiveQueueItem("media-1", "영상", LiveItemKinds.Media) });

        sut.SelectedItem = sut.Queue[0];

        sut.SelectedContentTabIndex.Should().Be(2, "미디어 선택 시 Media 탭 자동 전환");
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
    public void ToggleUseIndividualFormatting_FlipsFlagOnSelectedItem()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { FormatData = "29=-1", UseIndividualFormatting = true };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;

        sut.ToggleUseIndividualFormattingCommand.Execute(null);

        sut.SelectedItem!.UseIndividualFormatting.Should().BeFalse();
        sut.Queue[0].UseIndividualFormatting.Should().BeFalse("큐 항목도 교체됨");
        sut.StatusText.Should().Contain("전역 기본");

        sut.ToggleUseIndividualFormattingCommand.Execute(null);
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue();
    }

    [Fact]
    public void ApplyGlobalFormatToAll_TurnsOffIndividualFormattingOnAllItems_AndKeepsSelection()
    {
        // FrmMain "Apply to All Except InfoScreens" — 모든 항목의 개별 서식을 끄고 전역 기본 서식으로 통일.
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:1", "곡A", LiveItemKinds.Song) { FormatData = "29=-1", UseIndividualFormatting = true };
        var b = new LiveQueueItem("song:2", "곡B", LiveItemKinds.Song) { FormatData = "29=-2", UseIndividualFormatting = true };
        var c = new LiveQueueItem("verse:1", "성경", LiveItemKinds.Bible) { UseIndividualFormatting = true };
        sut.LoadQueue([a, b, c]);
        sut.SelectedItem = sut.Queue[1]; // 곡B 선택

        sut.ApplyGlobalFormatToAllCommand.Execute(null);

        sut.Queue.Should().OnlyContain(i => !i.UseIndividualFormatting, "모든 항목이 전역 서식으로 전환");
        sut.SelectedItem!.Id.Should().Be("song:2", "선택은 같은 자리(곡B)로 유지");
        sut.SelectedItem.UseIndividualFormatting.Should().BeFalse("선택 항목도 교체된 새 인스턴스");
        sut.StatusText.Should().Contain("3개");
    }

    [Fact]
    public void ApplyGlobalFormatToAll_WhenAllAlreadyGlobal_IsNoOpWithMessage()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:1", "곡A", LiveItemKinds.Song) { UseIndividualFormatting = false };
        sut.LoadQueue([a]);

        sut.ApplyGlobalFormatToAllCommand.Execute(null);

        sut.StatusText.Should().Contain("이미");
    }

    [Fact]
    public void ResetOutputAppearanceCommand_RestoresAppearanceSettingsToDefaults()
    {
        // FrmMain Default Layout — 출력 모양(색·효과·여백·패널·간격 등) 전체를 기본값으로 복원.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 90);
        settings.Set(EasiSettingKeys.LyricsMonitorBold, true);
        settings.Set(EasiSettingKeys.LyricsMonitorBodyLeftMargin, 40);
        settings.Set(EasiSettingKeys.LyricsMonitorPanelFontScalePercent, 150);
        settings.Set(EasiSettingKeys.LyricsMonitorRegionGapPx, 30);
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleOnPanel, true);
        // 템플릿 밖 키도 변형 — "전체" 리셋이 이들까지 되돌리는지 검증(code-review MAJOR 반영).
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundMode, LyricsBackgroundMode.Tile);
        settings.Set(EasiSettingKeys.LyricsMonitorRegionDisplay, LyricsRegionDisplay.Region1Only);
        settings.Set(EasiSettingKeys.LyricsMonitorInterlace, true);
        settings.Set(EasiSettingKeys.LyricsMonitorPanelTransparent, true);
        settings.Set(EasiSettingKeys.LyricsMonitorShowItemNumber, true);
        settings.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, 1000);

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorFontSize).Should().Be(EasiSettingKeys.LyricsMonitorFontSize.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorBold).Should().Be(EasiSettingKeys.LyricsMonitorBold.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyLeftMargin).Should().Be(EasiSettingKeys.LyricsMonitorBodyLeftMargin.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorPanelFontScalePercent).Should().Be(EasiSettingKeys.LyricsMonitorPanelFontScalePercent.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorRegionGapPx).Should().Be(EasiSettingKeys.LyricsMonitorRegionGapPx.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorShowTitleOnPanel).Should().Be(EasiSettingKeys.LyricsMonitorShowTitleOnPanel.DefaultValue);
        // 템플릿 밖 키도 기본값으로 복원되어야 한다(진짜 "전체" 리셋).
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundMode).Should().Be(EasiSettingKeys.LyricsMonitorBackgroundMode.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorRegionDisplay).Should().Be(EasiSettingKeys.LyricsMonitorRegionDisplay.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorInterlace).Should().Be(EasiSettingKeys.LyricsMonitorInterlace.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorPanelTransparent).Should().Be(EasiSettingKeys.LyricsMonitorPanelTransparent.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorShowItemNumber).Should().Be(EasiSettingKeys.LyricsMonitorShowItemNumber.DefaultValue);
        settings.Get(EasiSettingKeys.LyricsMonitorTransitionDurationMs).Should().Be(EasiSettingKeys.LyricsMonitorTransitionDurationMs.DefaultValue);
        sut.StatusText.Should().Contain("기본값");
    }

    [Fact]
    public void AppearanceTemplateDefaults_MatchFreshDefaultSettings()
    {
        // Defaults 템플릿이 설정 기본값과 어긋나지 않도록 가드 — 새 출력 모양 키를 Capture 에만 추가하고
        // Defaults 에 빠뜨리면 이 테스트가 깨진다(단일 진실원 유지).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        LyricsAppearanceTemplate.Defaults.Should().Be(LyricsAppearanceTemplate.Capture(settings));
    }

    [Fact]
    public void UseIndividualFormattingOff_LiveProjectionDropsPerSongColor()
    {
        // 개별 서식 off → 라이브 송출 시 곡별 FormatData 색(29)이 적용되지 않고 전역 기본색을 쓴다.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "1절 가사",
            FormatData = "29=-1", // region1 글자색 흰색.
            UseIndividualFormatting = true,
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;

        sut.GoLiveCommand.Execute(null);
        session.Current.OverrideTextColorArgb.Should().Be(-1, "개별 서식 on → 곡별 색 적용");

        sut.ToggleUseIndividualFormattingCommand.Execute(null); // off → 재송출.
        session.Current.OverrideTextColorArgb.Should().BeNull("개별 서식 off → 곡별 색 무시(전역 기본)");
    }

    [Fact]
    public void ToggleUseIndividualFormatting_LiveMultiVerse_PreservesCurrentVerse()
    {
        // 라이브 다중 절 곡에서 토글해도 현재 절이 유지돼야 한다(0절로 튀지 않음) — 세션의 실제 라이브 절로 재송출.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n1절 가사\n[2]\n2절 가사",
            FormatData = "29=-1",
            UseIndividualFormatting = true,
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);
        sut.NextLyricsPageCommand.Execute(null); // 2절로 이동(라이브 송출).
        session.Current.CurrentLyricsPageIndex.Should().Be(1);

        sut.ToggleUseIndividualFormattingCommand.Execute(null); // off → 재송출.

        session.Current.CurrentLyricsPageIndex.Should().Be(1, "현재 절(2절) 유지 — 0절로 튀지 않음");
        session.Current.OverrideTextColorArgb.Should().BeNull("개별 서식 off → 곡별 색 무시");
    }

    [Fact]
    public void AddPraiseBookSong_ResolvesLibrarySongByTitleAndNumber_AddsToQueue()
    {
        // 찬양집 색인 더블클릭 → 제목·번호로 현재 라이브러리에서 곡(가사 포함)을 찾아 예배 순서에 추가.
        var sut = CreateSut(seedSampleQueue: false);
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(42, "은혜로다", "", 1, 305, "", "", "1절 가사"));

        var added = sut.AddPraiseBookSong("은혜로다", 305);

        added.Should().NotBeNull();
        added!.Kind.Should().Be("Song");
        added.Id.Should().Be("song:42");
        added.Lyrics.Should().Be("1절 가사");
        sut.Queue.Should().Contain(added);
    }

    [Fact]
    public void AddPraiseBookSong_FallsBackToTitleOnly_WhenNumberMismatch()
    {
        // 번호가 안 맞아도(저장된 찬양집·번호 없는 곡) 제목만으로 한 번 더 찾는다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(7, "주 은혜", "", 1, 0, "", "", "가사"));

        var added = sut.AddPraiseBookSong("주 은혜", 999);

        added.Should().NotBeNull();
        added!.Id.Should().Be("song:7");
    }

    [Fact]
    public void AddPraiseBookSong_NotInLibrary_DoesNotChangeQueue()
    {
        var sut = CreateSut(seedSampleQueue: false);

        var added = sut.AddPraiseBookSong("없는 곡", 1);

        added.Should().BeNull();
        sut.Queue.Should().BeEmpty();
        sut.StatusText.Should().Contain("찾을 수 없습니다");
    }

    [Fact]
    public void AddPraiseBookSong_SameTitleDifferentSongs_ResolvesBySongId()
    {
        // 같은 제목·번호의 다른 곡(다른 폴더·언어)이 둘 있어도 SongId 로 더블클릭한 바로 그 곡을 정확히 가른다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(1, "주 은혜", "", 1, 305, "", "", "한국어 가사"));
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(2, "주 은혜", "", 2, 305, "", "", "English lyrics"));

        var added = sut.AddPraiseBookSong("주 은혜", 305, songId: 2);

        added!.Id.Should().Be("song:2");
        added.Lyrics.Should().Be("English lyrics"); // 첫 일치(SongId 1)가 아니라 고른 SongId 2.
    }

    [Fact]
    public void AddPraiseBookSong_SavedBookWithoutSongId_FallsBackToTitleAndNumber()
    {
        // 저장된 찬양집은 SongId=0 → 제목+번호로 해석(번호로 중복 제목을 가른다).
        var sut = CreateSut(seedSampleQueue: false);
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(1, "동일제목", "", 1, 100, "", "", "A"));
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(2, "동일제목", "", 2, 200, "", "", "B"));

        var added = sut.AddPraiseBookSong("동일제목", 200, songId: 0);

        added!.Id.Should().Be("song:2");
    }

    [Fact]
    public void ImportEswWorshipList_MapsEachTypeCodeToKind_AndReplacesQueue()
    {
        // 레거시 .esw 가져오기: 종류 코드(D/P/M/B/T)를 각 WPF 항목 종류로 매핑하고 큐를 통째로 교체한다.
        var sut = CreateSut(seedSampleQueue: true); // 기존 큐가 있어도 가져오기는 비우고 새로 채운다.
        var items = new List<EswWorshipListItem>
        {
            new("D", "123", "은혜로다", "찬양", ""),
            new("P", "설교.pptx", "설교 슬라이드", "", ""),
            new("M", "찬양영상.mp4", "찬양 영상", "", ""),
            new("B", "창1:1", "창세기 1:1", "성경", ""),
            new("T", "광고", "광고 안내", "", ""),
        };

        sut.ImportEswWorshipList(items);

        sut.Queue.Select(i => i.Kind).Should().Equal(
            LiveItemKinds.Song, LiveItemKinds.PowerPoint, LiveItemKinds.Media, LiveItemKinds.Bible, LiveItemKinds.Notice);
        sut.Queue.Select(i => i.Title).Should().Equal("은혜로다", "설교 슬라이드", "찬양 영상", "창세기 1:1", "광고 안내");
        sut.Queue[1].ContentPath.Should().Be("설교.pptx", "PPT 는 파일 참조를 ContentPath 에 보존");
        sut.Queue[2].ContentPath.Should().Be("찬양영상.mp4", "미디어도 파일 참조 보존");
        sut.SelectedItem.Should().Be(sut.Queue[0], "가져온 뒤 첫 항목 선택");
        sut.StatusText.Should().Contain("5개");
    }

    [Fact]
    public void ImportEswWorshipList_DbSongInLibrary_FillsLyricsAndNumber()
    {
        // 곡(D) 항목이 현재 라이브러리에 같은 SongId 로 있으면 가사·번호·저작권까지 채워 온전한 곡으로 가져온다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.Library.Songs.Add(new Easislides.Wpf.Data.SongSummary(123, "은혜로다", "", 1, 305, "", "", "1절 가사", "CCLI 7"));

        sut.ImportEswWorshipList(new List<EswWorshipListItem> { new("D", "123", "은혜로다", "찬양", "") });

        var item = sut.Queue.Single();
        item.Id.Should().Be("song:123");
        item.Lyrics.Should().Be("1절 가사", "라이브러리에서 가사를 채움");
        item.SongNumber.Should().Be(305);
        item.Copyright.Should().Be("CCLI 7");
    }

    [Fact]
    public void ImportEswWorshipList_DbSongNotInLibrary_KeepsTitleOnly()
    {
        // 라이브러리에 없으면(가사 미해석) 제목만 가진 곡 항목으로 가져온다 — 운영자가 나중에 가사를 보정.
        var sut = CreateSut(seedSampleQueue: false);

        sut.ImportEswWorshipList(new List<EswWorshipListItem> { new("D", "999", "없는 곡", "찬양", "") });

        var item = sut.Queue.Single();
        item.Kind.Should().Be(LiveItemKinds.Song);
        item.Id.Should().Be("song:999");
        item.Title.Should().Be("없는 곡");
        item.Lyrics.Should().BeNull("라이브러리에 없으면 가사를 못 채움(제목만)");
    }

    [Fact]
    public void ImportEswWorshipList_EmptyTitle_FallsBackToId()
    {
        // 제목이 비면 식별자를 제목으로 써서 빈 줄 항목이 생기지 않게 한다.
        var sut = CreateSut(seedSampleQueue: false);

        sut.ImportEswWorshipList(new List<EswWorshipListItem> { new("P", "행사.pptx", "", "", "") });

        sut.Queue.Single().Title.Should().Be("행사.pptx");
    }

    [Fact]
    public void ImportEswWorshipList_CarriesFormatDataThrough_OnEveryBranch()
    {
        // 곡별 출력 서식(FORMATDATA)은 가져올 때 항목에 그대로 실려야 한다 — 색·정렬이 사라지지 않게(회귀 방지).
        var sut = CreateSut(seedSampleQueue: false);
        var items = new List<EswWorshipListItem>
        {
            new("D", "5", "곡", "찬양", "29=-65536>"),   // 라이브러리에 없어도 FormatData 는 보존.
            new("P", "행사.pptx", "행사", "", "47=60>"),
            new("B", "창1:1", "창세기", "성경", "29=-1>"),
            new("T", "공지", "공지", "", "31=2>"),
        };

        sut.ImportEswWorshipList(items);

        sut.Queue.Select(i => i.FormatData).Should().Equal("29=-65536>", "47=60>", "29=-1>", "31=2>");
    }

    [Fact]
    public void ImportEswWorshipList_DbSongWithNonNumericId_KeepsTitleOnly()
    {
        // 식별자가 숫자가 아니면(손상·비정상 ItemID) int 파싱이 실패 → 제목만 가진 곡 항목으로 안전하게(예외 없이).
        var sut = CreateSut(seedSampleQueue: false);

        sut.ImportEswWorshipList(new List<EswWorshipListItem> { new("D", "abc", "비정상 식별자 곡", "", "") });

        var item = sut.Queue.Single();
        item.Kind.Should().Be(LiveItemKinds.Song);
        item.Id.Should().Be("song:abc", "숫자 파싱 실패 시 식별자 원문을 그대로 보존(가사 미해석)");
        item.Title.Should().Be("비정상 식별자 곡");
        item.Lyrics.Should().BeNull("숫자가 아니면 라이브러리 조회를 건너뛴다");
    }

    [Fact]
    public void ImportEswWorshipList_Null_Throws()
    {
        // 널 입력은 조용히 빈 큐로 만들지 않고 즉시 예외(LoadQueue 와 같은 방어).
        var sut = CreateSut(seedSampleQueue: false);

        var act = () => sut.ImportEswWorshipList(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ImportEswWorshipList_Empty_ClearsQueue()
    {
        // 빈 목록(읽을 수 없는/빈 .esw)을 가져오면 큐를 비운다 — 이전 내용이 남지 않게.
        var sut = CreateSut(seedSampleQueue: true);

        sut.ImportEswWorshipList(new List<EswWorshipListItem>());

        sut.Queue.Should().BeEmpty();
        sut.SelectedItem.Should().BeNull();
    }

    [Fact]
    public void AddBibleSelectionRelativeTo_InsertsBeforeTargetItem()
    {
        // 본문 드래그→예배순서 드롭: 떨어뜨린 위치(타깃 항목) 앞에 성경 구절을 끼운다.
        var sut = CreateSut(seedSampleQueue: false);
        var first = new LiveQueueItem("a", "첫 항목", LiveItemKinds.Song);
        var second = new LiveQueueItem("b", "둘째 항목", LiveItemKinds.Song);
        sut.LoadQueue([first, second]);
        var selection = new BibleSelection("0;kjv.db;;1;1;1;1;1;", "Genesis 1:1 (KJV)");

        var inserted = sut.AddBibleSelectionRelativeTo(selection, second);

        inserted.Should().NotBeNull();
        inserted!.Kind.Should().Be("Bible");
        sut.Queue.Should().Equal(first, inserted, second); // 둘째 항목 "앞"에.
        sut.SelectedItem.Should().Be(inserted);
    }

    [Fact]
    public void AddBibleSelectionRelativeTo_DuplicateValueTarget_UsesExactInstanceIndex()
    {
        // 같은 값(제목·Id)의 항목이 큐에 여러 번 있어도, 드롭한 "바로 그 인스턴스" 앞에 끼워야 한다
        // (record 값-동등 IndexOf 가 첫 일치로 가면 엉뚱한 위치에 들어감 — 참조 일치로 고정).
        var sut = CreateSut(seedSampleQueue: false);
        var firstDup = new LiveQueueItem("dup", "반복 찬양", LiveItemKinds.Song);
        var middle = new LiveQueueItem("mid", "말씀", LiveItemKinds.Bible);
        var secondDup = new LiveQueueItem("dup", "반복 찬양", LiveItemKinds.Song); // 값은 firstDup 과 동일.
        sut.LoadQueue([firstDup, middle, secondDup]);
        var selection = new BibleSelection("0;kjv.db;;1;1;1;1;1;", "Genesis 1:1 (KJV)");

        var inserted = sut.AddBibleSelectionRelativeTo(selection, secondDup); // 둘째 중복 인스턴스에 드롭.

        // 첫 중복이 아니라 둘째 중복 "앞"에 들어가야 한다.
        sut.Queue.Should().Equal(firstDup, middle, inserted!, secondDup);
    }

    [Fact]
    public void AddBibleSelectionRelativeTo_NullTarget_AppendsToEnd()
    {
        // 빈 공간(마지막 항목 아래)에 드롭하면 타깃이 null → 맨 끝에 추가.
        var sut = CreateSut(seedSampleQueue: false);
        var first = new LiveQueueItem("a", "첫 항목", LiveItemKinds.Song);
        sut.LoadQueue([first]);
        var selection = new BibleSelection("0;kjv.db;;1;1;1;1;1;", "Genesis 1:1 (KJV)");

        var inserted = sut.AddBibleSelectionRelativeTo(selection, null);

        sut.Queue.Should().Equal(first, inserted!);
    }

    [Fact]
    public void AddBibleSelectionRelativeTo_EmptySelection_DoesNotChangeQueue()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var first = new LiveQueueItem("a", "첫 항목", LiveItemKinds.Song);
        sut.LoadQueue([first]);

        var inserted = sut.AddBibleSelectionRelativeTo(new BibleSelection("", ""), first);

        inserted.Should().BeNull();
        sut.Queue.Should().Equal(first);
        sut.StatusText.Should().Be("선택된 성경 구절이 없습니다.");
    }

    [Fact]
    public void SelectingBibleItemWithBody_PaginatesByVerse()
    {
        // 성경 항목도 곡처럼 절 단위로 페이지네이션된다(예전엔 곡만 — 성경은 본문이 없어 절 이동이 안 됐다).
        var sut = CreateSut(seedSampleQueue: false);
        var bible = new LiveQueueItem("0;kjv.db;;1;1;1;1;3;", "Genesis 1:1-3", LiveItemKinds.Bible)
        {
            Lyrics = "1:1 In the beginning\n\n1:2 And the earth\n\n1:3 And God said",
        };
        sut.LoadQueue([bible]);

        sut.SelectedItem = bible;

        sut.LyricsPageCount.Should().Be(3);
        sut.NextLyricsPageCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void SelectingDualLanguageBible_CountsVersesWithRegionAwarePages()
    {
        // 이중 언어 성경([region 2])은 [region 2] 가 절 경계로 오인되지 않고 영역-인식 페이지 수(2절)로 계산된다.
        var sut = CreateSut(seedSampleQueue: false);
        var bible = new LiveQueueItem("0;kjv.db;krv.db;43;3;16;3;17;", "John 3:16-17", LiveItemKinds.Bible)
        {
            Lyrics = "For God so loved\n[region 2]\n하나님이 세상을\n\nFor God sent\n[region 2]\n하나님이 아들을",
        };
        sut.LoadQueue([bible]);

        sut.SelectedItem = bible;

        sut.LyricsPageCount.Should().Be(2);
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
    public void AddSongRelativeTo_InsertsBeforeTarget_WithContent_AndSelectsIt()
    {
        // 라이브러리 곡을 드롭한 위치(타깃) 앞에 끼운다(레거시 외부 소스 드래그). 가사·곡번호·저작권 등 내용을 그대로 싣고 복제본을 선택.
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            new LiveQueueItem("x", "X", "Song"),
            new LiveQueueItem("y", "Y", "Song"),
        ]);
        var target = sut.Queue[1]; // Y

        var song = new SongSummary(
            SongId: 7, Title: "은혜", AlternateTitle: "", FolderNo: 1,
            SongNumber: 42, Category: "", Key: "G", Lyrics: "[1]\n가사", Copyright: "(c)2020");

        var added = sut.AddSongRelativeTo(song, target);

        added.Should().NotBeNull();
        added!.Id.Should().Be("song:7");
        added.Lyrics.Should().Be("[1]\n가사", "라이브러리 곡은 가사를 그대로 싣는다");
        added.SongNumber.Should().Be(42);
        added.Copyright.Should().Be("(c)2020");
        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "x", "song:7", "y" }, "타깃(Y) 앞에 삽입");
        sut.SelectedItem.Should().BeSameAs(added);
    }

    [Fact]
    public void AddSongRelativeTo_NullTarget_AppendsToEnd()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([new LiveQueueItem("x", "X", "Song")]);
        var song = new SongSummary(
            SongId: 7, Title: "은혜", AlternateTitle: "", FolderNo: 1,
            SongNumber: 0, Category: "", Key: "", Lyrics: "가사");

        sut.AddSongRelativeTo(song, targetItem: null);

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "x", "song:7" }, "타깃 없으면(빈 공간 드롭) 맨 끝");
    }

    [Fact]
    public void AddSongRelativeTo_NullSong_IsNoOp()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSongRelativeTo(null, targetItem: null).Should().BeNull();
        sut.Queue.Should().BeEmpty();
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
    public void MoveSelectedItemDown_ReordersQueueAndKeepsSelection()
    {
        // 예배 순서 재정렬: 선택 항목을 아래로 한 칸 이동(FrmMain Move Item Down).
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        var c = new LiveQueueItem("c", "C", "Notice");
        sut.LoadQueue([a, b, c]);
        sut.SelectedItem = a;

        sut.MoveSelectedItemDownCommand.Execute(null);

        sut.Queue.Should().Equal(b, a, c);
        sut.SelectedItem.Should().Be(a); // 같은 항목이 계속 선택됨
    }

    [Fact]
    public void MoveSelectedItemUp_ReordersQueueAndKeepsSelection()
    {
        // 예배 순서 재정렬: 선택 항목을 위로 한 칸 이동(FrmMain Move Item Up).
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        var c = new LiveQueueItem("c", "C", "Notice");
        sut.LoadQueue([a, b, c]);
        sut.SelectedItem = c;

        sut.MoveSelectedItemUpCommand.Execute(null);

        sut.Queue.Should().Equal(a, c, b);
        sut.SelectedItem.Should().Be(c);
    }

    [Fact]
    public void MoveSelectedItem_CanExecute_RespectsBoundaries()
    {
        // 경계: 첫 항목은 위로 불가, 마지막 항목은 아래로 불가, 선택 없음이면 둘 다 불가.
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        sut.LoadQueue([a, b]);

        sut.SelectedItem = null;
        sut.MoveSelectedItemUpCommand.CanExecute(null).Should().BeFalse();
        sut.MoveSelectedItemDownCommand.CanExecute(null).Should().BeFalse();

        sut.SelectedItem = a; // 첫 항목
        sut.MoveSelectedItemUpCommand.CanExecute(null).Should().BeFalse();
        sut.MoveSelectedItemDownCommand.CanExecute(null).Should().BeTrue();

        sut.SelectedItem = b; // 마지막 항목
        sut.MoveSelectedItemUpCommand.CanExecute(null).Should().BeTrue();
        sut.MoveSelectedItemDownCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void MoveQueueItem_MovesItemToTargetIndex_AndKeepsSelection()
    {
        // 드래그 재정렬 코어(§7.5 P1): 항목을 임의 위치로 이동. 첫 항목을 마지막으로.
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        var c = new LiveQueueItem("c", "C", "Notice");
        sut.LoadQueue([a, b, c]);
        sut.SelectedItem = a;

        sut.MoveQueueItem(a, 2);

        sut.Queue.Should().Equal(b, c, a);
        sut.SelectedItem.Should().Be(a, "이동한 항목이 계속 선택됨");
    }

    [Fact]
    public void MoveQueueItem_ClampsTargetIndexToBounds()
    {
        // 드롭 위치가 범위를 벗어나면(음수/초과) 양끝으로 클램프.
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        var c = new LiveQueueItem("c", "C", "Notice");
        sut.LoadQueue([a, b, c]);

        sut.MoveQueueItem(b, 99); // 초과 → 마지막으로

        sut.Queue.Should().Equal(a, c, b);
    }

    [Fact]
    public void MoveQueueItem_SamePosition_IsNoOp()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        sut.LoadQueue([a, b]);

        sut.MoveQueueItem(a, 0); // 제자리

        sut.Queue.Should().Equal(a, b);
    }

    [Fact]
    public void MoveQueueItem_UnknownItem_IsNoOp()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        sut.LoadQueue([a, b]);
        var stranger = new LiveQueueItem("z", "Z", "Notice");

        sut.MoveQueueItem(stranger, 0); // 큐에 없는 항목 → 무시

        sut.Queue.Should().Equal(a, b);
    }

    [Fact]
    public void MoveQueueItem_DuplicateValueItems_MovesExactInstance()
    {
        // 값 동등(record) 중복 항목이 있어도 참조로 정확한 인스턴스만 이동.
        var sut = CreateSut();
        var first = new LiveQueueItem("dup", "같은 제목", "Notice");
        var middle = new LiveQueueItem("m", "중간", "Notice");
        var second = new LiveQueueItem("dup", "같은 제목", "Notice"); // first 와 값 동등, 다른 인스턴스
        sut.LoadQueue([first, middle, second]);

        sut.MoveQueueItem(second, 0); // 두 번째(뒤) 인스턴스를 맨 앞으로

        sut.Queue[0].Should().BeSameAs(second, "참조로 찾은 정확한 인스턴스가 이동");
        sut.Queue.Should().Equal(second, first, middle);
    }

    [Fact]
    public void MoveQueueItemRelativeTo_NullTarget_MovesToEnd()
    {
        // 빈 공간(마지막 항목 아래)에 드롭 → 타깃 null → 맨 끝으로.
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        var c = new LiveQueueItem("c", "C", "Notice");
        sut.LoadQueue([a, b, c]);

        sut.MoveQueueItemRelativeTo(a, null);

        sut.Queue.Should().Equal(b, c, a);
    }

    [Fact]
    public void MoveQueueItemRelativeTo_DuplicateValueTarget_UsesExactInstanceIndex()
    {
        // code-review CRITICAL 회귀잠금: 값 동등(record) 중복 타깃이 있어도 드롭 타깃 인덱스를
        // 참조로 구해야 한다(값 동등성 IndexOf 면 첫 인스턴스 위치로 잘못 감).
        var sut = CreateSut();
        var dup0 = new LiveQueueItem("dup", "같은 제목", "Notice");
        var middle = new LiveQueueItem("m", "중간", "Notice");
        var dup1 = new LiveQueueItem("dup", "같은 제목", "Notice"); // dup0 와 값 동등, 다른 인스턴스
        sut.LoadQueue([dup0, middle, dup1]);

        // middle 을 "뒤쪽" 중복 인스턴스(dup1, 인덱스 2) 위치로 이동.
        sut.MoveQueueItemRelativeTo(middle, dup1);

        // 값 동등성 IndexOf 였다면 dup0(인덱스 0)으로 가 [middle, dup0, dup1] 이 됐을 것.
        // 참조 기반이면 dup1 의 실제 인덱스(2)로 가 [dup0, dup1, middle].
        sut.Queue.Should().Equal(dup0, dup1, middle);
    }

    [Fact]
    public void MoveQueueItemRelativeTo_TargetNotInQueue_MovesToEnd()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        sut.LoadQueue([a, b]);
        var stranger = new LiveQueueItem("z", "Z", "Notice");

        sut.MoveQueueItemRelativeTo(a, stranger); // 큐에 없는 타깃 → 맨 끝

        sut.Queue.Should().Equal(b, a);
    }

    [Fact]
    public void RemoveSelectedItem_RemovesAndSelectsNeighbor()
    {
        // 제거 후 같은 인덱스(또는 마지막) 항목을 새로 선택.
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        var c = new LiveQueueItem("c", "C", "Notice");
        sut.LoadQueue([a, b, c]);
        sut.SelectedItem = b;

        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Queue.Should().Equal(a, c);
        sut.SelectedItem.Should().Be(c); // 같은 인덱스(1)의 새 항목
        sut.StatusText.Should().Contain("항목 제거");
    }

    [Fact]
    public void RemoveSelectedItem_WhenLastItem_SelectsNewLast()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("a", "A", "Notice");
        var b = new LiveQueueItem("b", "B", "Notice");
        sut.LoadQueue([a, b]);
        sut.SelectedItem = b; // 마지막

        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Queue.Should().Equal(a);
        sut.SelectedItem.Should().Be(a);
    }

    [Fact]
    public void RemoveSelectedItem_WhenOnlyItem_LeavesEmptyQueueAndNoSelection()
    {
        var sut = CreateSut();
        var only = new LiveQueueItem("a", "A", "Notice");
        sut.LoadQueue([only]);
        sut.SelectedItem = only;

        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Queue.Should().BeEmpty();
        sut.SelectedItem.Should().BeNull();
        sut.RemoveSelectedItemCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveSelectedItem_WhenItemIsLive_KeepsSessionActive()
    {
        // 라이브 송출 중인 항목을 큐에서 제거해도 송출 세션 자체는 유지(큐와 세션은 독립).
        var sut = CreateSut();
        var live = new LiveQueueItem("song-1", "입례 찬양");
        var other = new LiveQueueItem("song-2", "봉헌 찬양");
        sut.LoadQueue([live, other]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = live;
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Active);

        sut.SelectedItem = live;
        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Queue.Should().Equal(other);
        sut.Session.Current.State.Should().Be(LiveState.Active); // 세션은 큐 제거와 무관하게 유지
    }

    [Fact]
    public void MoveSelectedItem_WithValueEqualDuplicate_MovesSelectedInstanceByReference()
    {
        // LiveQueueItem 은 값 동등성 record. SelectedItem 이 동일-값 중복 중 "특정 인스턴스"를
        // 가리킬 때, 값 비교(IndexOf)가 아니라 참조로 바로 그 인스턴스를 이동해야 한다.
        var sut = CreateSut();
        var anchor = new LiveQueueItem("anchor", "기준 항목", "Notice");
        var first = new LiveQueueItem("song-x", "동일 찬양");
        var second = new LiveQueueItem("song-x", "동일 찬양"); // first 와 값 동등, 다른 인스턴스
        sut.LoadQueue([anchor, first, second]); // 큐를 통제(CreateSut 시드 항목 제거)
        sut.SelectedItem = second; // anchor 와 값이 달라 정상 할당됨 → 인덱스 2 인스턴스 선택

        // 값 비교라면 IndexOf(second)==1(first) 라 엉뚱한 항목이 움직였겠지만, 참조 기반이면 second 가 이동.
        sut.MoveSelectedItemUpCommand.CanExecute(null).Should().BeTrue();
        sut.MoveSelectedItemUpCommand.Execute(null);

        sut.Queue[1].Should().BeSameAs(second);
        sut.Queue[2].Should().BeSameAs(first);
    }

    [Fact]
    public void RemoveSelectedItem_WithValueEqualDuplicate_RemovesSelectedInstanceByReference()
    {
        // 동일-값 중복에서 참조로 정확히 선택된 인스턴스를 제거(값 비교면 앞 인스턴스가 지워짐).
        var sut = CreateSut();
        var anchor = new LiveQueueItem("anchor", "기준 항목", "Notice");
        var first = new LiveQueueItem("song-x", "동일 찬양");
        var second = new LiveQueueItem("song-x", "동일 찬양");
        sut.LoadQueue([anchor, first, second]); // 큐를 통제(CreateSut 시드 항목 제거)
        sut.SelectedItem = second; // 인덱스 2 인스턴스

        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Queue.Should().HaveCount(2);
        sut.Queue[0].Should().BeSameAs(anchor);
        sut.Queue[1].Should().BeSameAs(first); // second(인덱스 2)가 제거됨(값 비교면 first 가 지워졌을 것)
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
    public async Task WorshipListHasUnsavedChanges_TracksDirty_AcrossEditSaveLoad()
    {
        // 증분144 — 미저장 변경 표시: 시작 깨끗 → 큐 편집 시 표시 → 저장/불러오기로 깨끗.
        var sut = CreateSut(seedSampleQueue: false, worshipLists: new InMemoryWorshipListStore());
        sut.WorshipListHasUnsavedChanges.Should().BeFalse("시작 시 미저장 변경 없음");

        sut.AddTextItem("새 항목");
        sut.WorshipListHasUnsavedChanges.Should().BeTrue("큐를 편집하면 미저장 변경 표시");

        await sut.SaveWorshipListAsync("주일 예배");
        sut.WorshipListHasUnsavedChanges.Should().BeFalse("저장하면 깨끗");

        sut.AddTextItem("또 추가");
        sut.WorshipListHasUnsavedChanges.Should().BeTrue("다시 편집 → 미저장");

        await sut.LoadWorshipListAsync("주일 예배");
        sut.WorshipListHasUnsavedChanges.Should().BeFalse("불러오면(전체 교체) 깨끗");
    }

    [Fact]
    public void WorshipListHasUnsavedChanges_NotSetByGoLiveOrSelection()
    {
        // 큐 내용이 안 바뀌는 동작(선택 변경)은 미저장으로 표시하지 않는다 — 큐 편집만 dirty.
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song") { Lyrics = "[1]\n가" }, new LiveQueueItem("b", "B", "Song") { Lyrics = "[1]\n나" }]);
        sut.WorshipListHasUnsavedChanges.Should().BeFalse("불러오기 직후 깨끗");

        sut.SelectedItem = sut.Queue[1]; // 선택만 바꿈 — 큐 내용 불변

        sut.WorshipListHasUnsavedChanges.Should().BeFalse("선택 변경은 미저장 아님");
    }

    [Fact]
    public async Task WorshipListHasUnsavedChanges_NotSetByGoLive()
    {
        // 송출(GoLive)은 큐를 안 바꾸고 투영본만 보내므로 미저장으로 표시하지 않는다 — 거짓 양성 방지(불변식 잠금).
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song") { Lyrics = "[1]\n가" }]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        sut.WorshipListHasUnsavedChanges.Should().BeFalse("불러오기 직후 깨끗");

        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.WorshipListHasUnsavedChanges.Should().BeFalse("송출은 큐를 안 바꾸므로 미저장 아님");
    }

    [Fact]
    public async Task QuickSave_WithCurrentSession_SavesAndClearsDirty()
    {
        // 증분145 — Ctrl+S 빠른 저장: 현재 세션 이름으로 덮어 저장하고 미저장 변경 표시를 끈다.
        var sut = CreateSut(seedSampleQueue: false, worshipLists: new InMemoryWorshipListStore());
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song") { Lyrics = "[1]\n가" }]);
        await sut.SaveWorshipListAsync("주일 예배"); // 현재 세션 이름 설정
        sut.AddTextItem("추가"); // 편집 → 미저장
        sut.WorshipListHasUnsavedChanges.Should().BeTrue();

        await sut.QuickSaveWorshipListCommand.ExecuteAsync(null);

        sut.WorshipListHasUnsavedChanges.Should().BeFalse("빠른 저장 후 깨끗");
        sut.StatusText.Should().Contain("주일 예배", "현재 세션 이름으로 저장");
    }

    [Fact]
    public async Task QuickSave_NoCurrentSession_HintsAndDoesNotSave()
    {
        // 한 번도 저장/불러온 적 없어 세션 이름이 없으면 저장하지 않고 안내만(이름은 관리 창에서).
        var sut = CreateSut(seedSampleQueue: false, worshipLists: new InMemoryWorshipListStore());
        sut.AddTextItem("이름 없는 항목");
        sut.CurrentWorshipListName.Should().BeEmpty("아직 저장/불러온 적 없음");

        await sut.QuickSaveWorshipListCommand.ExecuteAsync(null);

        sut.StatusText.Should().Contain("이름", "이름 없으면 안내");
        sut.WorshipListHasUnsavedChanges.Should().BeTrue("저장 안 했으니 여전히 미저장");
    }

    [Fact]
    public async Task SelectLiveItem_SnapsSelectionBackToLiveItem()
    {
        // 증분148 — 미리보기로 앞서 본 뒤 "현재 송출 항목 선택"으로 라이브 항목으로 선택 복귀.
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:a", "1부 찬양", "Song") { Lyrics = "[1]\n가" };
        var b = new LiveQueueItem("song:b", "2부 찬양", "Song") { Lyrics = "[1]\n나" };
        sut.LoadQueue([a, b]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null); // A 라이브
        sut.SelectedItem = sut.Queue[1]; // 미리보기로 B 선택(라이브는 여전히 A)
        sut.CanSelectLiveItem.Should().BeTrue("송출 중이고 라이브 항목이 큐에 있음");

        sut.SelectLiveItemCommand.Execute(null);

        sut.SelectedItem!.Id.Should().Be("song:a", "선택이 라이브 항목으로 되돌아감");
        sut.StatusText.Should().Contain("현재 송출 항목");
    }

    [Fact]
    public async Task CanSelectLiveItem_FalseWhenNotLive_AndWhenLiveItemRemoved()
    {
        // 송출 전이면 비활성. 라이브 항목이 큐에서 빠져도 비활성(큐에 없는 고아 Id 로 점프 시도 안 함).
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("a", "A", "Song") { Lyrics = "[1]\n가" };
        var b = new LiveQueueItem("b", "B", "Song") { Lyrics = "[1]\n나" };
        sut.LoadQueue([a, b]);
        sut.CanSelectLiveItem.Should().BeFalse("송출 전이라 라이브 항목 없음");
        sut.SelectLiveItemCommand.CanExecute(null).Should().BeFalse();

        // A 를 송출하면 활성.
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.CanSelectLiveItem.Should().BeTrue("송출 중이고 라이브 항목이 큐에 있음");

        // 라이브 항목(A)을 큐에서 제거하면 다시 비활성(큐에 없는 고아 Id).
        sut.SelectedItem = sut.Queue.First(i => i.Id == "a");
        sut.RemoveSelectedItemCommand.Execute(null);
        sut.CanSelectLiveItem.Should().BeFalse("라이브 항목이 큐에서 빠지면 비활성");
    }

    // ── 세션 콤보(예배 순서 빠른 전환) — 콤보로 고르고 명시적 "불러오기"로 적재 ──

    [Fact]
    public async Task RefreshSavedWorshipListNames_PopulatesFromStore_AndSavePushesNewName()
    {
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        sut.SavedWorshipListNames.Should().BeEmpty("처음엔 저장된 게 없음");

        sut.LoadQueue([new LiveQueueItem("a", "A", "Song") { Lyrics = "L" }]);
        await sut.SaveWorshipListAsync("주일 1부"); // 저장하면 콤보에 바로 보여야 함.

        sut.SavedWorshipListNames.Should().Contain("주일 1부", "저장 직후 세션 콤보에 반영");

        // 다른 경로(직접 저장)로 추가된 목록도 콤보 열기(Refresh) 시 보인다.
        await sut.SaveWorshipListAsync("주일 2부");
        sut.RefreshSavedWorshipListNames();
        sut.SavedWorshipListNames.Should().Contain(new[] { "주일 1부", "주일 2부" });
    }

    [Fact]
    public void LoadSelectedWorshipListCommand_CanExecuteFollowsSelection()
    {
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        sut.LoadSelectedWorshipListCommand.CanExecute(null).Should().BeFalse("고른 게 없으면 비활성");

        sut.SelectedSavedWorshipList = "주일 1부";
        sut.LoadSelectedWorshipListCommand.CanExecute(null).Should().BeTrue("고르면 활성");

        sut.SelectedSavedWorshipList = "   ";
        sut.LoadSelectedWorshipListCommand.CanExecute(null).Should().BeFalse("공백뿐이면 비활성");
    }

    [Fact]
    public async Task LoadSelectedWorshipListCommand_LoadsTheSelectedList()
    {
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        sut.LoadQueue([new LiveQueueItem("x", "X", "Song") { Lyrics = "원본" }]);
        await sut.SaveWorshipListAsync("주일 1부");
        sut.LoadQueue([new LiveQueueItem("other", "다른", "Song")]); // 큐를 바꿔 둠.

        sut.SelectedSavedWorshipList = "주일 1부";
        await sut.LoadSelectedWorshipListCommand.ExecuteAsync(null);

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "x" }, "콤보로 고른 예배 순서가 적재됨(현재 큐 대체)");
    }

    [Fact]
    public async Task RefreshSavedWorshipListNames_KeepsSelectionWhenNameStillPresent()
    {
        // 새로고침해도 여전히 있는 선택은 유지돼야 한다(콤보 ItemsSource Clear 로 선택이 풀리는 WPF Selector 동작 대비
        // — VM 의 캡처/복원 로직을 잠근다. 실제 Selector 널링은 단위 테스트에 안 나타나지만, 복원 의도를 고정).
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song")]);
        await sut.SaveWorshipListAsync("주일 1부");
        await sut.SaveWorshipListAsync("주일 2부");
        sut.SelectedSavedWorshipList = "주일 1부";

        sut.RefreshSavedWorshipListNames();

        sut.SelectedSavedWorshipList.Should().Be("주일 1부", "여전히 있는 선택은 유지");
        sut.LoadSelectedWorshipListCommand.CanExecute(null).Should().BeTrue("선택 유지 → 불러오기 활성");
    }

    [Fact]
    public async Task DeleteWorshipList_RemovesFromSessionCombo()
    {
        // 삭제하면 세션 콤보에서도 바로 빠진다(저장 경로와 대칭).
        var sut = CreateSut(worshipLists: new InMemoryWorshipListStore());
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song")]);
        await sut.SaveWorshipListAsync("지울것");
        sut.SavedWorshipListNames.Should().Contain("지울것");

        sut.DeleteWorshipList("지울것");

        sut.SavedWorshipListNames.Should().NotContain("지울것", "삭제 직후 세션 콤보에서 빠짐");
    }

    // ── 대기 화면(Gap) 빠른 조작(출력 메뉴) — 모드 콤보·페이드 토글·로고 픽커 ──

    [Fact]
    public void GapItemModeInput_PersistsSetting_AndSyncsActive()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.GapItemModeInput = GapItemMode.Black;

        settings.Get(EasiSettingKeys.GapItemOption).Should().Be(GapItemMode.Black, "설정에 저장");
        sut.ActiveGapItemOption.Should().Be(GapItemMode.Black, "Active 동기화");
        sut.StatusText.Should().Contain("검정");
    }

    [Fact]
    public void ToggleGapItemUseFade_FlipsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var before = sut.ActiveGapItemUseFade;

        sut.ToggleGapItemUseFadeCommand.Execute(null);

        sut.ActiveGapItemUseFade.Should().Be(!before, "토글");
        settings.Get(EasiSettingKeys.GapItemUseFade).Should().Be(!before, "설정 저장");
        sut.StatusText.Should().Contain("페이드", "상태줄에 페이드 상태 알림");
    }

    [Fact]
    public void GapControls_FollowDirectSettingsChange_ViaRefresh()
    {
        // 메뉴 외 경로(설정 창=직접 settings.Set)로 Gap 모드가 바뀌면, SettingsChanged→RefreshActiveAppearance 로
        // 출력 메뉴 표시(Active/Input)가 따라가야 한다(이 재동기화가 깨지기 쉬워 회귀로 고정).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveGapItemOption.Should().Be(GapItemMode.None, "기본");

        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.Black); // 설정 창이 바꾼 것처럼.

        sut.ActiveGapItemOption.Should().Be(GapItemMode.Black, "설정 변경을 메뉴가 따라감");
        sut.GapItemModeInput.Should().Be(GapItemMode.Black, "콤보 입력도 동기화");
    }

    [Fact]
    public void SetGapItemLogoFile_PersistsPath_AndAutoSwitchesToUserMode()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.GapItemModeInput = GapItemMode.None; // 로고 아닌 모드에서 시작.

        sut.SetGapItemLogoFile(@"  C:\logos\church.png  ");

        settings.Get(EasiSettingKeys.GapItemLogoFile).Should().Be(@"C:\logos\church.png", "경로 다듬어 저장");
        sut.ActiveGapItemLogoFile.Should().Be(@"C:\logos\church.png");
        sut.ActiveGapItemOption.Should().Be(GapItemMode.User, "로고를 고르면 보이도록 모드도 '로고'로 전환");
        sut.StatusText.Should().Contain("church.png", "상태줄에 고른 로고 파일명 알림");
    }

    [Fact]
    public void SetGapItemLogoFile_Blank_ClearsPath_AndKeepsMode()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.SetGapItemLogoFile(@"C:\logos\a.png"); // User 모드 + 경로.

        sut.ClearGapItemLogoFileCommand.Execute(null);

        sut.ActiveGapItemLogoFile.Should().BeEmpty("로고 지움");
        settings.Get(EasiSettingKeys.GapItemLogoFile).Should().BeEmpty();
        sut.ActiveGapItemOption.Should().Be(GapItemMode.User, "지워도 모드는 그대로(운영자가 따로 바꿈)");
    }

    [Fact]
    public void GapControls_SyncFromSettings_OnRefresh()
    {
        // 설정 창 등 다른 경로로 Gap 설정이 바뀌어도 출력 메뉴 표시(Active*)가 따라간다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.Default);
        settings.Set(EasiSettingKeys.GapItemLogoFile, @"C:\x.png");
        var sut = CreateSut(settings: settings);

        sut.ActiveGapItemOption.Should().Be(GapItemMode.Default, "초기 동기화");
        sut.ActiveGapItemLogoFile.Should().Be(@"C:\x.png");
    }

    // ── 외부 파일 드래그-드롭(탐색기 → 예배 순서) — PPT/미디어만 추가, 모르는 형식은 건너뜀 ──

    [Fact]
    public void AddExternalFiles_AddsPowerPointAndMedia_SkipsUnsupported_InOrder()
    {
        var sut = CreateSut(seedSampleQueue: false);

        var added = sut.AddExternalFiles([@"C:\a.pptx", @"C:\b.mp4", @"C:\c.txt", @"C:\d.ppt"]);

        added.Should().Be(3, "PPT 2 + 미디어 1, txt 1 건너뜀");
        sut.Queue.Select(i => i.Kind).Should().Equal(
            LiveItemKinds.PowerPoint, LiveItemKinds.Media, LiveItemKinds.PowerPoint);
        sut.Queue.Select(i => i.ContentPath).Should().Equal(@"C:\a.pptx", @"C:\b.mp4", @"C:\d.ppt");
        sut.StatusText.Should().Contain("3개 추가").And.Contain("1개 건너뜀");
    }

    [Fact]
    public void AddExternalFiles_AllUnsupported_AddsNothing()
    {
        var sut = CreateSut(seedSampleQueue: false);

        var added = sut.AddExternalFiles([@"C:\a.txt", @"C:\b.docx", @"C:\c.png"]);

        added.Should().Be(0);
        sut.Queue.Should().BeEmpty("지원 안 하는 형식만이면 아무것도 안 추가");
        sut.StatusText.Should().Contain("0개 추가").And.Contain("3개 건너뜀");
    }

    [Fact]
    public void AddExternalFiles_InsertsAfterSelection_KeepingBatchContiguous()
    {
        // 비어있지 않은 큐에서 가운데 항목을 고른 뒤 외부 파일을 드롭하면, 그 선택 바로 뒤에 묶음으로 들어간다(연속).
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            new LiveQueueItem("a", "A", LiveItemKinds.Song),
            new LiveQueueItem("b", "B", LiveItemKinds.Song),
            new LiveQueueItem("c", "C", LiveItemKinds.Song),
        ]);
        sut.SelectedItem = sut.Queue[0]; // 첫 항목(A) 선택.

        sut.AddExternalFiles([@"C:\x.pptx", @"C:\y.mp4"]);

        sut.Queue.Select(i => i.Id).Should().Equal(
            new[] { "a", "powerpoint:C:\\x.pptx", "media:C:\\y.mp4", "b", "c" },
            "선택(A) 바로 뒤에 PPT·미디어가 순서대로 연속 삽입");
        sut.SelectedItem!.ContentPath.Should().Be(@"C:\x.pptx", "묶음의 첫 항목 선택(드롭 경로와 동일)");
    }

    [Fact]
    public void AddExternalFilesRelativeTo_InsertsBeforeDropTarget_InOrder()
    {
        // 떨어뜨린 항목(B) 앞에 PPT·미디어가 순서대로 묶음 삽입(성경/곡 드래그와 동일한 드롭-위치 삽입).
        var sut = CreateSut(seedSampleQueue: false);
        var b = new LiveQueueItem("b", "B", LiveItemKinds.Song);
        sut.LoadQueue([
            new LiveQueueItem("a", "A", LiveItemKinds.Song),
            b,
            new LiveQueueItem("c", "C", LiveItemKinds.Song),
        ]);

        sut.AddExternalFilesRelativeTo([@"C:\x.pptx", @"C:\y.mp4"], b);

        sut.Queue.Select(i => i.Id).Should().Equal(
            new[] { "a", "powerpoint:C:\\x.pptx", "media:C:\\y.mp4", "b", "c" },
            "타깃(B) 바로 앞에 순서대로 연속 삽입");
        sut.SelectedItem!.ContentPath.Should().Be(@"C:\x.pptx", "묶음의 첫 항목 선택");
    }

    [Fact]
    public void BuildNoticeFormatData_IncludesBackgroundColorCode26_WithOtherCodes()
    {
        // 공지 옵션이 곡 FormatData 파이프라인으로 인코드 — 배경색은 코드26, 나머지(크기47·정렬31·글자색29)도 함께.
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(
            FontSizePt: 60, Alignment: 2, ColorArgb: -1, BackgroundColorArgb: unchecked((int)0xFF000000)));

        fd.Should().NotBeNull();
        fd!.Should().Contain($"26={unchecked((int)0xFF000000)}", "배경색 코드26");
        fd.Should().Contain("47=60", "크기").And.Contain("31=2", "정렬").And.Contain("29=-1", "글자색");
    }

    [Fact]
    public void BuildNoticeFormatData_NoBackground_OmitsCode26()
    {
        // 배경색 기본(0)이면 코드26 을 넣지 않는다(출력 기본 배경 유지).
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(ColorArgb: -1));

        (fd ?? "").Should().NotContain("26=", "배경색 없으면 코드26 생략");
    }

    [Fact]
    public void PublishNotice_WithBackgroundColor_AppliesOverrideBackground_EndToEnd()
    {
        // 종단(통합): 공지 배경색이 송출 시 세션 오버라이드 배경으로 적용된다(BuildNoticeFormatData 코드26 →
        // GoLive 가 Kind 무관하게 FormatData 파싱 → OverrideBackgroundColorArgb → 출력 렌더). 곡 개별서식 게이트 없음.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        sut.OpenOutputCommand.Execute(null);

        var ok = sut.PublishNotice("광고", new NoticeOptions(BackgroundColorArgb: unchecked((int)0xFF000000)));

        ok.Should().BeTrue("출력이 열려 있으면 송출 성공");
        session.Current.OverrideBackgroundColorArgb.Should().Be(unchecked((int)0xFF000000), "공지 배경색이 라이브 송출 배경으로 적용");
    }

    [Fact]
    public void BuildNoticeFormatData_IncludesEmphasisCode41_WhenSet()
    {
        // 공지 강조(굵게·기울임·밑줄)가 코드41 비트밭으로 인코드 — 비트0=굵게(1)+비트2=밑줄(4)=5.
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(Bold: true, Underline: true));

        fd.Should().NotBeNull();
        fd!.Should().Contain("41=5", "굵게(1)+밑줄(4) 비트 합");
    }

    [Fact]
    public void BuildNoticeFormatData_NoEmphasis_OmitsCode41()
    {
        // 강조가 하나도 없으면 코드41 을 넣지 않는다(출력 기본 강조 유지).
        // 문자열 부분일치(NotContain "41=") 대신 디코더로 파싱해 강조가 실제로 비어 있는지 확인 → 코드 배치에 무관한 보증.
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(ColorArgb: -1));

        var parsed = Easislides.Wpf.Library.SongFormatData.Parse(fd ?? "");
        parsed?.Bold1.Should().NotBe(true, "강조 없으면 굵게 비트 없음");
        parsed?.Italic1.Should().NotBe(true, "강조 없으면 기울임 비트 없음");
        parsed?.Underline1.Should().NotBe(true, "강조 없으면 밑줄 비트 없음");
    }

    [Fact]
    public void PublishNotice_WithBold_AppliesOverrideBold1_EndToEnd()
    {
        // 종단(통합): 공지 굵게가 송출 시 세션 오버라이드 굵게1 로 적용된다(코드41 비트0 → GoLive 가 Kind 무관하게
        // 파싱 → OverrideBold1 → 출력이 영역1=공지 본문에 region1Bold 로 렌더). 곡 개별서식 런타임 게이트 없음.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        sut.OpenOutputCommand.Execute(null);

        var ok = sut.PublishNotice("중요 공지", new NoticeOptions(Bold: true));

        ok.Should().BeTrue("출력이 열려 있으면 송출 성공");
        session.Current.OverrideBold1.Should().Be(true, "공지 굵게가 라이브 송출 강조로 적용");
    }

    [Fact]
    public void BuildNoticeFormatData_IncludesFontNameCode43_WhenSet()
    {
        // 공지 글꼴명이 코드43 으로 인코드 — 다른 코드(크기47)와 함께.
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(FontSizePt: 60, FontName: "나눔고딕"));

        fd.Should().NotBeNull();
        fd!.Should().Contain("43=나눔고딕", "글꼴명 코드43").And.Contain("47=60", "크기도 함께");
    }

    [Theory]
    [InlineData("나눔>고딕", "43=나눔고딕")]   // '>' 제거
    [InlineData("Arial=Bold", "43=ArialBold")] // '=' 제거
    [InlineData("  맑은 고딕  ", "43=맑은 고딕")] // 앞뒤 공백 다듬음(가운데 공백은 보존)
    public void BuildNoticeFormatData_StripsDelimitersFromFontName(string input, string expectedFragment)
    {
        // 글꼴명에 구분자('>'·'=')가 섞여도 FormatData 가 깨지지 않게 제거(곡 글꼴 규약과 동일).
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(FontName: input));

        fd.Should().NotBeNull();
        fd!.Should().Contain(expectedFragment);
    }

    [Fact]
    public void BuildNoticeFormatData_BlankFontName_OmitsCode43()
    {
        // 글꼴명이 비거나 공백뿐이면 코드43 생략(전역 글꼴 추종).
        var fd = MainViewModel.BuildNoticeFormatData(new NoticeOptions(ColorArgb: -1, FontName: "   "));

        (fd ?? "").Should().NotContain("43=", "글꼴 없으면 코드43 생략");
    }

    [Fact]
    public void PublishNotice_WithFontName_AppliesOverrideFontName_EndToEnd()
    {
        // 종단(통합): 공지 글꼴이 송출 시 세션 오버라이드 글꼴로 적용된다(코드43 → GoLive 가 Kind 무관하게 파싱
        // → OverrideFontName → 출력이 영역1=공지 본문에 그 글꼴로 렌더).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        sut.OpenOutputCommand.Execute(null);

        var ok = sut.PublishNotice("공지", new NoticeOptions(FontName: "나눔고딕"));

        ok.Should().BeTrue("출력이 열려 있으면 송출 성공");
        session.Current.OverrideFontName.Should().Be("나눔고딕", "공지 글꼴이 라이브 송출 글꼴로 적용");
    }

    [Fact]
    public void AddExternalFilesRelativeTo_SkipsImages_NotQueueItems()
    {
        // 이미지 파일은 큐 항목이 아니라 출력 배경용이므로, 큐에 드롭하면 추가하지 않고 건너뛴다(배경 드롭은 미리보기 영역).
        var sut = CreateSut(seedSampleQueue: false);

        var added = sut.AddExternalFilesRelativeTo([@"C:\bg.png", @"C:\clip.mp4", @"C:\photo.jpg"], targetItem: null);

        added.Should().Be(1, "미디어 1개만 추가, 이미지 2개는 건너뜀");
        sut.Queue.Should().ContainSingle().Which.Kind.Should().Be(LiveItemKinds.Media);
        sut.StatusText.Should().Contain("1개 추가").And.Contain("2개 건너뜀");
    }

    [Fact]
    public void AddExternalFilesRelativeTo_NullTarget_AppendsAtEnd()
    {
        // 빈 공간(타깃 없음)에 떨어뜨리면 맨 끝에 붙는다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([new LiveQueueItem("a", "A", LiveItemKinds.Song)]);

        sut.AddExternalFilesRelativeTo([@"C:\x.pptx"], targetItem: null);

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "a", "powerpoint:C:\\x.pptx" }, "맨 끝에 추가");
    }

    [Fact]
    public void AddExternalFilesRelativeTo_DuplicateValueTarget_UsesExactInstance()
    {
        // 같은 값의 항목이 여러 번 있어도 드롭한 "바로 그 인스턴스" 앞에 끼운다(참조 일치 IndexOfReference).
        var sut = CreateSut(seedSampleQueue: false);
        var firstDup = new LiveQueueItem("dup", "반복", LiveItemKinds.Song);
        var secondDup = new LiveQueueItem("dup", "반복", LiveItemKinds.Song);
        sut.LoadQueue([firstDup, secondDup]);

        sut.AddExternalFilesRelativeTo([@"C:\x.mp4"], secondDup); // 두 번째 인스턴스 앞에.

        sut.Queue.Should().HaveCount(3);
        sut.Queue.IndexOf(sut.Queue.First(i => i.Kind == LiveItemKinds.Media))
            .Should().Be(1, "두 번째 dup 앞(인덱스 1)에 — 값-동등 첫 일치(0)가 아니라");
    }

    [Fact]
    public void AddExternalFiles_OnlySupported_NoSkippedHint()
    {
        var sut = CreateSut(seedSampleQueue: false);

        sut.AddExternalFiles([@"C:\a.pptx", @"C:\b.wmv"]);

        sut.StatusText.Should().Contain("2개 추가").And.NotContain("건너뜀", "건너뛴 게 없으면 건너뜀 안내 없음");
    }

    [Fact]
    public async Task RefreshSavedWorshipListNames_ClearsSelectionWhenNameGone()
    {
        // 고른 목록이 (다른 창에서) 삭제돼 목록에서 사라지면, 콤보 새로고침 시 선택을 비운다(없는 목록 적재 방지).
        var store = new InMemoryWorshipListStore();
        var sut = CreateSut(worshipLists: store);
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song")]);
        await sut.SaveWorshipListAsync("임시");
        sut.SelectedSavedWorshipList = "임시";

        store.Delete("임시"); // 외부 삭제 시뮬레이션.
        sut.RefreshSavedWorshipListNames();

        sut.SelectedSavedWorshipList.Should().BeNull("사라진 목록은 선택 해제");
        sut.LoadSelectedWorshipListCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void MoveSelectedItemToTopAndBottom_RepositionsAndKeepsSelection()
    {
        // 맨 위로/맨 아래로 한 번에 이동(모던 재정렬). 이동 후에도 같은 항목이 선택되어 있어야 한다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            new LiveQueueItem("a", "A", "Song"),
            new LiveQueueItem("b", "B", "Song"),
            new LiveQueueItem("c", "C", "Song"),
        ]);

        var middle = sut.Queue[1]; // B
        sut.SelectedItem = middle;
        sut.MoveSelectedItemToTopCommand.Execute(null);
        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "b", "a", "c" }, "B 가 맨 위로");
        sut.SelectedItem.Should().BeSameAs(middle, "이동 후에도 선택 유지");

        sut.MoveSelectedItemToBottomCommand.Execute(null);
        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "a", "c", "b" }, "B 가 맨 아래로");
        sut.SelectedItem.Should().BeSameAs(middle);
    }

    [Fact]
    public void MoveToBoundary_CanExecute_DisabledAtEdges()
    {
        // 이미 맨 위면 "맨 위로" 비활성, 맨 아래면 "맨 아래로" 비활성(불필요한 이동 방지).
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            new LiveQueueItem("a", "A", "Song"),
            new LiveQueueItem("b", "B", "Song"),
        ]);

        sut.SelectedItem = sut.Queue[0]; // 맨 위
        sut.MoveSelectedItemToTopCommand.CanExecute(null).Should().BeFalse("이미 맨 위");
        sut.MoveSelectedItemToBottomCommand.CanExecute(null).Should().BeTrue();

        sut.SelectedItem = sut.Queue[1]; // 맨 아래
        sut.MoveSelectedItemToTopCommand.CanExecute(null).Should().BeTrue();
        sut.MoveSelectedItemToBottomCommand.CanExecute(null).Should().BeFalse("이미 맨 아래");

        sut.SelectedItem = null;
        sut.MoveSelectedItemToTopCommand.CanExecute(null).Should().BeFalse("선택 없으면 비활성");
    }

    [Fact]
    public void DuplicateSelectedItem_InsertsCopyWithSameContentNewId_AfterOriginal_AndSelectsIt()
    {
        // 선택 항목 복제 — 같은 내용(가사·종류·번호)·새 Id 로 바로 뒤에 삽입하고 복제본을 선택(같은 곡 두 번 부르기 등).
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            new LiveQueueItem("song:1", "은혜로다", "Song") { Lyrics = "[1]\n가사", SongNumber = 42 },
            new LiveQueueItem("song:2", "다른 곡", "Song"),
        ]);
        sut.SelectedItem = sut.Queue[0];

        var copy = sut.DuplicateSelectedItem();

        copy.Should().NotBeNull();
        sut.Queue.Should().HaveCount(3);
        sut.Queue[1].Should().BeSameAs(copy, "원본 바로 뒤에 삽입");
        sut.Queue[2].Id.Should().Be("song:2", "기존 항목은 뒤로 밀림");
        copy!.Id.Should().NotBe("song:1", "복제본은 새 Id");
        copy.Id.Should().StartWith("dup:");
        copy.Title.Should().Be("은혜로다", "내용은 같음");
        copy.Lyrics.Should().Be("[1]\n가사");
        copy.Kind.Should().Be("Song");
        copy.SongNumber.Should().Be(42, "곡 번호 등 init 속성 모두 복사");
        sut.SelectedItem.Should().BeSameAs(copy, "복제본을 선택");
    }

    [Fact]
    public void DuplicateSelectedItem_NoSelection_IsNoOp()
    {
        var sut = CreateSut(seedSampleQueue: false);

        sut.DuplicateSelectedItem().Should().BeNull();
        sut.Queue.Should().BeEmpty();
    }

    [Fact]
    public void DuplicateSelectedItemCommand_CanExecute_FollowsSelection()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.DuplicateSelectedItemCommand.CanExecute(null).Should().BeFalse("선택 없으면 비활성");

        sut.LoadQueue([new LiveQueueItem("a", "A", "Song")]);
        sut.SelectedItem = sut.Queue[0];
        sut.DuplicateSelectedItemCommand.CanExecute(null).Should().BeTrue("선택 있으면 활성");
    }

    [Fact]
    public void AddTextItem_AddsNoticeKindItem_WithFirstLineTitle_AndFullBody()
    {
        // 자유 텍스트 항목을 예배 순서에 추가 — 종류=Notice, 제목=첫 줄(짧게), 본문(Lyrics)=전체 텍스트.
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song")]);
        sut.SelectedItem = sut.Queue[0];

        var item = sut.AddTextItem("주차 안내\n2부 예배 후 주차장을 비워 주세요");

        item.Should().NotBeNull();
        item!.Kind.Should().Be(LiveItemKinds.Notice, "텍스트 항목은 공지(Notice) 종류로 송출");
        item.Title.Should().Be("주차 안내", "제목은 첫 줄");
        item.Lyrics.Should().Be("주차 안내\n2부 예배 후 주차장을 비워 주세요", "본문은 전체 텍스트");
        item.Id.Should().StartWith("text:", "센티넬과 겹치지 않는 고유 Id");
        sut.Queue.Should().HaveCount(2);
        sut.Queue[1].Should().BeSameAs(item, "선택 항목(A) 바로 뒤에 삽입");
        sut.SelectedItem.Should().BeSameAs(item, "추가 후 새 항목 선택");
    }

    [Fact]
    public void AddTextItem_WithOptions_AttachesFormatData_AndIndividualOn()
    {
        // "순서에 추가"가 공지 편집기 서식(크기·색·강조·글꼴)을 항목에 실어야 한다 — FormatData 인코드 + 개별 서식 켜짐(기본 true).
        var sut = CreateSut(seedSampleQueue: false);

        var item = sut.AddTextItem("광고", new NoticeOptions(FontSizePt: 60, ColorArgb: -1, Bold: true, FontName: "나눔고딕"));

        item.Should().NotBeNull();
        item!.FormatData.Should().NotBeNullOrEmpty("서식이 FormatData 로 실림");
        item.FormatData!.Should().Contain("47=60", "크기").And.Contain("29=-1", "색").And.Contain("41=1", "굵게 비트").And.Contain("43=나눔고딕", "글꼴");
        item.UseIndividualFormatting.Should().BeTrue("서식이 송출에 보이도록 개별 서식 켜짐");
    }

    [Fact]
    public void AddTextItem_NoOptions_HasNoFormatData_BackCompat()
    {
        // 옵션 없이 추가하면(기존 호출·Word 추출 경로) 서식 없음 — 전역 기본으로 송출(무회귀).
        var sut = CreateSut(seedSampleQueue: false);

        var item = sut.AddTextItem("서식 없는 공지");

        item!.FormatData.Should().BeNull("옵션 없으면 FormatData 없음");
    }

    [Fact]
    public async Task AddTextItem_FormattedThenPublished_RendersOverrides_EndToEnd()
    {
        // 종단: "순서에 추가"로 넣은 서식 텍스트 항목을 나중에 송출하면 그 서식(여기선 배경색)이 라이브에 적용된다
        // (FormatData → ResolveLiveProjection 이 개별 서식 켜져 있어 보존 → GoLive Kind 무관 파싱 → OverrideBackgroundColorArgb).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        sut.OpenOutputCommand.Execute(null);

        var item = sut.AddTextItem("광고", new NoticeOptions(BackgroundColorArgb: unchecked((int)0xFF000000)));
        sut.SelectedItem = item;
        await sut.GoLiveCommand.ExecuteAsync(null);

        session.Current.OverrideBackgroundColorArgb.Should().Be(unchecked((int)0xFF000000), "큐에 넣은 서식이 송출 시 적용");
    }

    [Fact]
    public void AddWordTextItem_WithText_AddsNoticeItem()
    {
        // Word 문서에서 추출한 본문을 텍스트(공지) 항목으로 추가(레거시 Word 항목). AddTextItem 재사용 — 종류 Notice·본문 전체.
        var sut = CreateSut(seedSampleQueue: false);

        var item = sut.AddWordTextItem("예배 안내\n오늘 본문은 시편 23편입니다");

        item.Should().NotBeNull();
        item!.Kind.Should().Be(LiveItemKinds.Notice);
        item.Title.Should().Be("예배 안내", "제목은 첫 줄");
        item.Lyrics.Should().Be("예배 안내\n오늘 본문은 시편 23편입니다", "본문 전체");
        sut.Queue.Should().ContainSingle();
    }

    [Fact]
    public void AddWordTextItem_EmptyExtract_ShowsGuidanceAndAddsNothing()
    {
        // Word 미설치·읽기 실패·빈 문서면 GetContents 가 빈 문자열 → 항목을 만들지 않고 안내만(graceful).
        var sut = CreateSut(seedSampleQueue: false);

        sut.AddWordTextItem("").Should().BeNull();
        sut.AddWordTextItem("   ").Should().BeNull();
        sut.AddWordTextItem(null).Should().BeNull();

        sut.Queue.Should().BeEmpty("추출 실패 시 항목 추가 안 함");
        sut.StatusText.Should().Contain("Word 문서를 읽지 못했습니다");
    }

    [Fact]
    public void AddTextItem_BlankText_IsNoOp()
    {
        var sut = CreateSut(seedSampleQueue: false);

        sut.AddTextItem("   ").Should().BeNull();
        sut.AddTextItem(null).Should().BeNull();
        sut.Queue.Should().BeEmpty("빈 텍스트는 항목을 추가하지 않음");
    }

    [Fact]
    public void AddTextItem_LongFirstLine_TruncatesTitleButKeepsFullBody()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var longLine = new string('가', 50);

        var item = sut.AddTextItem(longLine);

        item!.Title.Length.Should().BeLessThan(longLine.Length, "긴 제목은 줄임");
        item.Title.Should().EndWith("…");
        item.Lyrics.Should().Be(longLine, "본문은 전체 보존");
    }

    [Fact]
    public async Task MergeWorshipList_AppendsToCurrentQueue_KeepingExistingItems()
    {
        // 병합은 현재 큐를 지우지 않고 저장된 순서의 항목을 뒤에 이어 붙인다(불러오기=대체와 구별, 레거시 .esw 병합).
        var sut = CreateSut();
        sut.LoadQueue([
            new LiveQueueItem("a", "A", "Song") { Lyrics = "LA" },
            new LiveQueueItem("b", "B", "Song") { Lyrics = "LB" },
        ]);
        await sut.SaveWorshipListAsync("2부");
        sut.GetSavedWorshipLists().Should().Contain("2부");

        // 현재 큐를 다른 항목으로 바꾼 뒤 "2부"를 병합 → 현재 + 저장분 순서로 합쳐진다.
        sut.LoadQueue([new LiveQueueItem("c", "C", "Song") { Lyrics = "LC" }]);
        await sut.MergeWorshipListAsync("2부");

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "c", "a", "b" }, "현재 항목 뒤에 저장분이 추가");
        sut.SelectedItem!.Id.Should().Be("c", "비어 있지 않았으므로 현재 선택 유지");
    }

    [Fact]
    public async Task MergeWorshipList_IntoEmptyQueue_SelectsFirstLikeLoad()
    {
        // 빈 큐에 병합하면 불러오기처럼 첫 항목을 선택해 둔다.
        var sut = CreateSut();
        sut.LoadQueue([new LiveQueueItem("x", "X", "Song") { Lyrics = "LX" }]);
        await sut.SaveWorshipListAsync("저녁");

        sut.LoadQueue([]);
        await sut.MergeWorshipListAsync("저녁");

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "x" });
        sut.SelectedItem!.Id.Should().Be("x", "빈 큐 병합은 불러오기처럼 첫 항목 선택");
    }

    [Fact]
    public async Task MergeWorshipList_BlankName_IsNoOp()
    {
        var sut = CreateSut();
        sut.LoadQueue([new LiveQueueItem("a", "A", "Song")]);

        await sut.MergeWorshipListAsync("   ");

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "a" }, "빈 이름 병합은 무동작");
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
    public async Task SaveWorshipList_RecordsRecentAndUpdatesCollection()
    {
        var recent = new InMemoryRecentWorshipLists();
        var sut = CreateSut(recentWorshipLists: recent);

        await sut.SaveWorshipListAsync("주일 1부");

        recent.GetRecent().Should().Contain("주일 1부");
        sut.RecentWorshipLists.Should().Contain("주일 1부");
    }

    [Fact]
    public async Task LoadWorshipList_SetsCurrentWorshipListName_ForSessionNotesKey()
    {
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("주일오전", System.Array.Empty<LiveQueueItem>());
        var sut = CreateSut(worshipLists: store);

        await sut.LoadWorshipListAsync("주일오전");

        sut.CurrentWorshipListName.Should().Be("주일오전");
    }

    [Fact]
    public async Task LoadWorshipList_RecordsRecent()
    {
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("저녁예배", System.Array.Empty<LiveQueueItem>());
        var recent = new InMemoryRecentWorshipLists();
        var sut = CreateSut(worshipLists: store, recentWorshipLists: recent);

        await sut.LoadWorshipListAsync("저녁예배");

        sut.RecentWorshipLists.Should().Contain("저녁예배");
    }

    [Fact]
    public async Task OpenRecentWorshipList_ReloadsThatList()
    {
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("주일오전", new[] { new LiveQueueItem("s1", "찬양", LiveItemKinds.Song) });
        var sut = CreateSut(worshipLists: store);

        await sut.OpenRecentWorshipListCommand.ExecuteAsync("주일오전");

        sut.Queue.Should().ContainSingle(i => i.Title == "찬양");
    }

    [Fact]
    public async Task OpenRecentWorshipList_MovesOpenedEntryToFront()
    {
        // 최근 항목을 열면 다시 최근 맨 앞으로 올라온다(most-recently-touched).
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("A", System.Array.Empty<LiveQueueItem>());
        await store.SaveAsync("B", System.Array.Empty<LiveQueueItem>());
        var recent = new InMemoryRecentWorshipLists();
        var sut = CreateSut(worshipLists: store, recentWorshipLists: recent);
        await sut.LoadWorshipListAsync("A"); // recent: A
        await sut.LoadWorshipListAsync("B"); // recent: B, A
        sut.RecentWorshipLists.Should().Equal("B", "A");

        await sut.OpenRecentWorshipListCommand.ExecuteAsync("A");

        sut.RecentWorshipLists.Should().Equal("A", "B");
    }

    [Fact]
    public void ApplyTransitionKind_PersistsAndUpdatesActiveAndMenuChecks()
    {
        var sut = CreateSut();
        sut.TransitionKindIsFade.Should().BeTrue("기본은 페이드");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.SlideFromRight);

        sut.ActiveTransitionKind.Should().Be(LyricsTransitionKind.SlideFromRight);
        sut.TransitionKindIsSlideRight.Should().BeTrue();
        sut.TransitionKindIsFade.Should().BeFalse();
        sut.StatusText.Should().Contain("슬라이드");
    }

    [Fact]
    public void ApplyTransitionKind_TransformKinds_UpdateMenuChecks()
    {
        // 트랜스폼 기반 전환(줌/회전/뒤집기)도 설정·Active·메뉴 체크가 정확히 갱신된다.
        var sut = CreateSut();

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Spin);
        sut.ActiveTransitionKind.Should().Be(LyricsTransitionKind.Spin);
        sut.TransitionKindIsSpin.Should().BeTrue();
        sut.TransitionKindIsFade.Should().BeFalse();
        sut.StatusText.Should().Contain("회전");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.ZoomIn);
        sut.TransitionKindIsZoomIn.Should().BeTrue();
        sut.TransitionKindIsSpin.Should().BeFalse("이전 회전 체크는 해제");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.FlipVertical);
        sut.TransitionKindIsFlipV.Should().BeTrue();
        sut.StatusText.Should().Contain("뒤집기");
    }

    [Fact]
    public void ApplyTransitionKind_ClipRevealKinds_UpdateMenuChecks()
    {
        // 클립(마스크) 리빌 전환(원형/사각/와이프)도 설정·Active·메뉴 체크가 정확히 갱신된다.
        var sut = CreateSut();

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.RevealCircle);
        sut.ActiveTransitionKind.Should().Be(LyricsTransitionKind.RevealCircle);
        sut.TransitionKindIsRevealCircle.Should().BeTrue();
        sut.TransitionKindIsFade.Should().BeFalse();
        sut.StatusText.Should().Contain("원형");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.WipeRight);
        sut.TransitionKindIsWipeRight.Should().BeTrue();
        sut.TransitionKindIsRevealCircle.Should().BeFalse("이전 원형 체크 해제");
        sut.StatusText.Should().Contain("와이프");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Checkerboard);
        sut.TransitionKindIsCheckerboard.Should().BeTrue();
        sut.TransitionKindIsWipeRight.Should().BeFalse();
        sut.StatusText.Should().Contain("체커보드");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.BlindsHorizontal);
        sut.TransitionKindIsBlindsH.Should().BeTrue();
        sut.StatusText.Should().Contain("블라인드");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Diamond);
        sut.TransitionKindIsDiamond.Should().BeTrue();
        sut.TransitionKindIsBlindsH.Should().BeFalse();
        sut.StatusText.Should().Contain("다이아몬드");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.DoorsOpen);
        sut.TransitionKindIsDoorsOpen.Should().BeTrue();
        sut.StatusText.Should().Contain("양문");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Star);
        sut.TransitionKindIsStar.Should().BeTrue();
        sut.TransitionKindIsDoorsOpen.Should().BeFalse();
        sut.StatusText.Should().Contain("별");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Cross);
        sut.TransitionKindIsCross.Should().BeTrue();
        sut.TransitionKindIsStar.Should().BeFalse();
        sut.StatusText.Should().Contain("십자");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.BowTie);
        sut.TransitionKindIsBowTie.Should().BeTrue();
        sut.TransitionKindIsCross.Should().BeFalse();
        sut.StatusText.Should().Contain("나비넥타이");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Heart);
        sut.TransitionKindIsHeart.Should().BeTrue();
        sut.TransitionKindIsBowTie.Should().BeFalse();
        sut.StatusText.Should().Contain("하트");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Wedge);
        sut.TransitionKindIsWedge.Should().BeTrue();
        sut.TransitionKindIsHeart.Should().BeFalse();
        sut.StatusText.Should().Contain("시계");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.Spiral);
        sut.TransitionKindIsSpiral.Should().BeTrue();
        sut.TransitionKindIsWedge.Should().BeFalse();
        sut.StatusText.Should().Contain("나선");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.WindMill);
        sut.TransitionKindIsWindMill.Should().BeTrue();
        sut.TransitionKindIsSpiral.Should().BeFalse();
        sut.StatusText.Should().Contain("바람개비");

        sut.ApplyTransitionKindCommand.Execute(LyricsTransitionKind.FanUp);
        sut.TransitionKindIsFanUp.Should().BeTrue();
        sut.TransitionKindIsWindMill.Should().BeFalse();
        sut.StatusText.Should().Contain("부채");
    }

    [Fact]
    public void PublishNotice_WhenOutputClosed_ReturnsFalseAndStaysOff()
    {
        // 출력 창이 닫혀 있으면 공지 송출은 실패(false)하고 라이브 상태를 바꾸지 않는다.
        var sut = CreateSut();

        var ok = sut.PublishNotice("예배 후 다과");

        ok.Should().BeFalse();
        sut.Session.Current.State.Should().Be(LiveState.Off);
    }

    [Fact]
    public void PublishNotice_WhenOutputOpen_SendsNoticeBodyLive()
    {
        // 출력이 열려 있으면 공지 텍스트가 본문으로 라이브 송출된다(FrmInfoScreen 대응).
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);

        var ok = sut.PublishNotice("주차장 만차 안내");

        ok.Should().BeTrue();
        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentItemTitle.Should().Be("공지");
        sut.Session.Current.CurrentItemBodyText.Should().Contain("주차장 만차 안내");
    }

    [Fact]
    public void PublishNotice_WithFontSize_CarriesFontOverrideToSnapshot()
    {
        // 공지 글자 크기 지정(pt)이 FormatData(47=pt)로 실려 기존 폰트 오버라이드 파이프라인을 타고
        // 출력 스냅샷의 OverrideFontSizePx 로 반영된다(InfoScreen 큰 글씨).
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);

        var ok = sut.PublishNotice("크게 보여줄 공지", new NoticeOptions(FontSizePt: 80));

        ok.Should().BeTrue();
        sut.Session.Current.OverrideFontSizePx.Should().NotBeNull("47=80 FormatData 가 폰트 크기 오버라이드로 디코드됨");
    }

    [Fact]
    public void PublishNotice_WithAlignmentAndColor_FlowsToOverrides()
    {
        // 공지 정렬(31)·색(29)이 FormatData 로 실려 기존 곡별 오버라이드 파이프라인을 타고
        // 출력 스냅샷의 OverrideTextAlignment·OverrideTextColorArgb 로 반영된다(InfoScreen 정렬·색).
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);

        var ok = sut.PublishNotice("색·정렬 공지", new NoticeOptions(Alignment: 1, ColorArgb: unchecked((int)0xFFFFE066)));

        ok.Should().BeTrue();
        sut.Session.Current.OverrideTextAlignment.Should().Be(LyricsTextAlignment.Left, "31=1 → 왼쪽 정렬 오버라이드");
        sut.Session.Current.OverrideTextColorArgb.Should().Be(unchecked((int)0xFFFFE066), "29=색 → 글자색 오버라이드");
    }

    [Fact]
    public void PublishNotice_WithoutFontSize_NoFontOverride()
    {
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);

        sut.PublishNotice("기본 크기 공지").Should().BeTrue();

        sut.Session.Current.OverrideFontSizePx.Should().BeNull("크기 미지정이면 출력 기본 글자 크기 사용");
    }

    [Fact]
    public void PublishNotice_BlankText_ReturnsFalse()
    {
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);

        sut.PublishNotice("   ").Should().BeFalse();
    }

    [Fact]
    public async Task PublishNotice_DisablesPowerPointSlideNav_WhilePptSelected()
    {
        // 공지가 떠 있는 동안에는 선택된 PPT 덱의 슬라이드 이동 버튼이 비활성이어야 한다
        // (_liveItemId 센티넬 → 선택 항목 ID 와 불일치 → 이동 가드 false). null 와일드카드 버그 회귀 방지.
        var expectedSlide = new DrawingImage();
        expectedSlide.Freeze();
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => expectedSlide);
        await powerPoint.LoadAsync("deck.pptx", 1, 960, 540); // SlideCount=3, 현재 1
        var sut = CreateSut(powerPoint: powerPoint);
        var ppt = new LiveQueueItem("ppt:1", "Deck", LiveItemKinds.PowerPoint) { ContentPath = "deck.pptx" };
        sut.LoadQueue(new[] { ppt });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = ppt;
        // 공지 송출 전(라이브 미시작)에는 선택 덱의 다음 슬라이드 이동이 가능(대조군).
        sut.NextSlideCommand.CanExecute(null).Should().BeTrue();

        sut.PublishNotice("공지").Should().BeTrue();

        sut.NextSlideCommand.CanExecute(null).Should().BeFalse("공지 송출 중에는 PPT 슬라이드 이동 비활성");
    }

    [Fact]
    public void ClearNotice_HidesOutput()
    {
        var sut = CreateSut();
        sut.OpenOutputCommand.Execute(null);
        sut.PublishNotice("공지").Should().BeTrue();
        sut.Session.Current.State.Should().Be(LiveState.Active);

        sut.ClearNotice();

        sut.Session.Current.State.Should().Be(LiveState.Hidden, "지우기는 출력을 숨김(검은 화면)으로");
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
    public void FirstAndLastItemCommands_JumpToEndsOfQueue()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("song-1", "첫 곡");
        var b = new LiveQueueItem("song-2", "가운데 곡");
        var c = new LiveQueueItem("song-3", "끝 곡");
        sut.LoadQueue(new[] { a, b, c });
        sut.SelectedItem = b;

        sut.LastItemCommand.Execute(null);
        sut.SelectedItem.Should().Be(c, "마지막 항목으로 이동");

        sut.FirstItemCommand.Execute(null);
        sut.SelectedItem.Should().Be(a, "첫 항목으로 이동");
    }

    [Fact]
    public void FirstAndLastItemCommands_CanExecute_ReflectsPosition()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("song-1", "첫 곡");
        var b = new LiveQueueItem("song-2", "끝 곡");
        sut.LoadQueue(new[] { a, b });

        sut.SelectedItem = a; // 첫 항목 → 처음 비활성, 마지막 활성
        sut.FirstItemCommand.CanExecute(null).Should().BeFalse("이미 첫 항목");
        sut.LastItemCommand.CanExecute(null).Should().BeTrue();

        sut.SelectedItem = b; // 마지막 항목 → 처음 활성, 마지막 비활성
        sut.FirstItemCommand.CanExecute(null).Should().BeTrue();
        sut.LastItemCommand.CanExecute(null).Should().BeFalse("이미 마지막 항목");
    }

    [Fact]
    public async Task LastItemCommand_WhenLive_PublishesThatItem()
    {
        var sut = CreateSut();
        var a = new LiveQueueItem("song-1", "입례 찬양");
        var b = new LiveQueueItem("song-2", "축도 찬양");
        sut.LoadQueue(new[] { a, b });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = a;
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LastItemCommand.Execute(null);

        sut.SelectedItem.Should().Be(b);
        sut.Session.Current.CurrentItemTitle.Should().Be("축도 찬양", "라이브 중이면 마지막 항목을 송출");
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
    public void BindShortcuts_BindsOperatorCommandsForPalette()
    {
        // §7.4 팔레트 흡수: 화면 제어 보강 명령(비우기/처음으로/새로고침/복귀/자동회전)이 레지스트리에
        // 바인딩돼 ⌘K(TryInvoke)로 실행 가능해야 한다. 카탈로그에 추가하고 배선을 빠뜨리면 팔레트에서
        // 선택은 되지만 조용히 무동작하므로, 배선 존재를 잠근다.
        // 비라이브 SUT 라 CanExecute=false → 게이트로 no-op → TryInvoke 는 true(바인딩 존재·예외 없음).
        var sut = CreateSut();
        var registry = new ShortcutRegistry();
        sut.BindShortcuts(registry);

        foreach (var id in new[]
                 {
                     MainCommandIds.LiveClear, MainCommandIds.LiveRestart, MainCommandIds.LiveRefresh,
                     MainCommandIds.LiveRestore, MainCommandIds.LiveAutoRotate, MainCommandIds.LiveGoAndNext,
                     MainCommandIds.WorshipMoveItemUp, MainCommandIds.WorshipMoveItemDown,
                     MainCommandIds.WorshipMoveItemToTop, MainCommandIds.WorshipMoveItemToBottom,
                     MainCommandIds.WorshipQuickSave, MainCommandIds.WorshipSelectLiveItem,
                 })
        {
            registry.TryInvoke(id).Should().BeTrue($"{id} 가 레지스트리에 바인딩돼 팔레트·단축키로 실행 가능");
        }
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
    public async Task RestoreOutputCommand_RestoresHiddenOutputToActive()
    {
        // §7.3-B 라이브 화면 제어: 숨김 후 "복귀"로 직전 송출 상태(Active)로 되돌린다.
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        await sut.HideOutputCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Hidden);
        sut.RestoreOutputCommand.CanExecute(null).Should().BeTrue("숨김 상태에서 복귀 활성");

        sut.RestoreOutputCommand.Execute(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.RestoreOutputCommand.CanExecute(null).Should().BeFalse("복귀 후엔 비활성");
    }

    // ─── 화면 제어 보강: Clear / Restart / Refresh (§7.3-B) ───────────────────

    [Fact]
    public async Task ClearOutputCommand_WhenLive_EntersClearedAfterConfirm()
    {
        // 비우기: 라이브 중 콘텐츠를 감추되 배경 유지(Cleared). 안전 확인 후 적용.
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.ClearOutputCommand.CanExecute(null).Should().BeTrue("라이브 중 비우기 활성");
        await sut.ClearOutputCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Hidden);
        sut.Session.Current.IsCleared.Should().BeTrue();
        sut.Session.Current.IsBlackout.Should().BeFalse();
    }

    [Fact]
    public async Task ClearOutputCommand_WhenSafetyDeclined_DoesNotChangeState()
    {
        var prompt = new RecordingSafetyPrompt(allow: true);
        var sut = CreateSut(prompt);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null); // GoLive 는 허용
        prompt.Allow = false; // 이후 Clear 안전 확인만 거부

        await sut.ClearOutputCommand.ExecuteAsync(null);

        sut.Session.Current.State.Should().Be(LiveState.Active, "안전 확인 거부 시 비우기 미적용");
        sut.Session.Current.IsCleared.Should().BeFalse();
    }

    [Fact]
    public void ClearOutputCommand_WhenNotLive_IsDisabled()
    {
        var sut = CreateSut();

        sut.ClearOutputCommand.CanExecute(null).Should().BeFalse("라이브가 아니면 비우기 비활성");
    }

    [Fact]
    public void RestoreOutputCommand_FromCleared_ReturnsToActive()
    {
        // 비우기 후에도 "복귀"로 직전 송출(Active)로 되돌린다(Clear 도 Hidden 변형이라 복귀 가능).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        _ = sut.GoLiveCommand.ExecuteAsync(null);
        _ = sut.ClearOutputCommand.ExecuteAsync(null);

        sut.RestoreOutputCommand.CanExecute(null).Should().BeTrue();
        sut.RestoreOutputCommand.Execute(null);

        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.IsCleared.Should().BeFalse();
    }

    [Fact]
    public async Task RestartCurrentItemCommand_WhenLiveSongAdvanced_ResetsToFirstVerse()
    {
        // 처음으로: 절을 넘긴 라이브 곡을 첫 절(LyricsPageIndex=0)로 되돌려 재송출한다.
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절\n[3]\n3절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.NextLyricsPageCommand.Execute(null); // 2절
        sut.NextLyricsPageCommand.Execute(null); // 3절
        sut.LyricsPageIndex.Should().Be(2);

        sut.RestartCurrentItemCommand.CanExecute(null).Should().BeTrue();
        sut.RestartCurrentItemCommand.Execute(null);

        sut.LyricsPageIndex.Should().Be(0, "처음으로 → 첫 절");
        sut.Session.Current.CurrentItemBodyText.Should().Be("1절", "출력도 첫 절로 재송출");
    }

    [Fact]
    public void RestartCurrentItemCommand_WhenNotLive_IsDisabled()
    {
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.SelectedItem = sut.Queue[0];

        sut.RestartCurrentItemCommand.CanExecute(null).Should().BeFalse("라이브가 아니면 처음으로 비활성");
    }

    [Fact]
    public async Task RestartCurrentItemCommand_WhenCleared_IsDisabled()
    {
        // 비우기(Cleared=Hidden) 상태에선 처음으로 비활성 — 먼저 복귀해야 한다(Active 가드).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        await sut.ClearOutputCommand.ExecuteAsync(null);

        sut.RestartCurrentItemCommand.CanExecute(null).Should().BeFalse("비우기 상태에선 처음으로 비활성");
    }

    [Fact]
    public async Task RestartCurrentItemCommand_WhenLivePptOnFirstSlide_RefreshesOutput()
    {
        // PPT 덱이 이미 첫 슬라이드면 GoToSlide 가 무시되므로 Refresh 로 강제 재렌더해야 한다(리뷰 #2a).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "주보", "PowerPoint") { ContentPath = "deck.pptx" } });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        powerPoint.SlideNumber.Should().Be(1, "라이브 시작 시 첫 슬라이드");

        var changes = 0;
        sut.Session.SessionChanged += (_, _) => changes++;
        sut.RestartCurrentItemCommand.CanExecute(null).Should().BeTrue();
        await sut.RestartCurrentItemCommand.ExecuteAsync(null);

        changes.Should().BeGreaterThanOrEqualTo(1, "첫 슬라이드 PPT 는 Refresh 로 재렌더");
    }

    [Fact]
    public async Task RefreshOutputCommand_AfterClear_ReEmitsClearedSnapshot()
    {
        // 비우기 후 새로고침하면 Cleared 스냅샷이 그대로 재통지돼야 한다(Active 로 되돌아가지 않음).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        await sut.ClearOutputCommand.ExecuteAsync(null);

        LiveSessionSnapshot? last = null;
        sut.Session.SessionChanged += (_, e) => last = e.Snapshot;
        sut.RefreshOutputCommand.Execute(null);

        last.Should().NotBeNull();
        last!.IsCleared.Should().BeTrue("새로고침은 현재(비우기) 상태를 그대로 재통지");
        last.State.Should().Be(LiveState.Hidden);
    }

    [Fact]
    public async Task RefreshOutputCommand_WhenOutputOpen_RaisesSessionRefresh()
    {
        // 새로고침: 출력이 열려 있으면 현재 세션을 강제 재통지(재렌더)한다.
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "1절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        var changes = 0;
        sut.Session.SessionChanged += (_, _) => changes++;
        sut.RefreshOutputCommand.CanExecute(null).Should().BeTrue();
        sut.RefreshOutputCommand.Execute(null);

        changes.Should().Be(1, "새로고침은 세션을 한 번 재통지");
    }

    [Fact]
    public void RefreshOutputCommand_WhenOutputClosed_IsDisabled()
    {
        var sut = CreateSut();

        sut.RefreshOutputCommand.CanExecute(null).Should().BeFalse("출력 닫힘 상태에선 새로고침 비활성");
    }

    // ─── 인-셸 가사 정렬 인스펙터 (§7.3-A / §7.5 P0-a) ────────────────────────

    [Fact]
    public void ApplyLyricsAlignmentCommand_PersistsAlignmentSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyLyricsAlignmentCommand.Execute(LyricsTextAlignment.Left);

        settings.Get(EasiSettingKeys.LyricsMonitorTextAlignment).Should().Be(LyricsTextAlignment.Left);
        sut.ActiveLyricsAlignment.Should().Be(LyricsTextAlignment.Left);
    }

    [Fact]
    public void ActiveLyricsAlignment_ReflectsCurrentSetting_DefaultsCenter()
    {
        var sut = CreateSut();

        sut.ActiveLyricsAlignment.Should().Be(LyricsTextAlignment.Center, "기본 정렬은 가운데");
    }

    [Fact]
    public void LyricsAlignmentOptions_ExposesThreeOptions()
    {
        var sut = CreateSut();

        sut.LyricsAlignmentOptions.Should().BeEquivalentTo(
            new[] { LyricsTextAlignment.Left, LyricsTextAlignment.Center, LyricsTextAlignment.Right });
    }

    [Fact]
    public void ApplyLyricsVerticalAlignmentCommand_PersistsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyLyricsVerticalAlignmentCommand.Execute(LyricsVerticalAlignment.Bottom);

        settings.Get(EasiSettingKeys.LyricsMonitorVerticalAlignment).Should().Be(LyricsVerticalAlignment.Bottom);
        sut.ActiveLyricsVerticalAlignment.Should().Be(LyricsVerticalAlignment.Bottom);
    }

    [Fact]
    public void ActiveLyricsVerticalAlignment_DefaultsCenter()
    {
        var sut = CreateSut();

        sut.ActiveLyricsVerticalAlignment.Should().Be(LyricsVerticalAlignment.Center);
    }

    [Fact]
    public void LyricsVerticalAlignmentOptions_ExposesTopCenterBottom()
    {
        var sut = CreateSut();

        sut.LyricsVerticalAlignmentOptions.Should().BeEquivalentTo(
            new[] { LyricsVerticalAlignment.Top, LyricsVerticalAlignment.Center, LyricsVerticalAlignment.Bottom });
    }

    [Fact]
    public void IncreaseLyricsFontSizeCommand_StepsUpAndPersists()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsFontSize.Should().Be(48, "기본 48px");

        sut.IncreaseLyricsFontSizeCommand.Execute(null);

        sut.ActiveLyricsFontSize.Should().Be(52, "한 단계 +4");
        settings.Get(EasiSettingKeys.LyricsMonitorFontSize).Should().Be(52);
    }

    [Fact]
    public void DecreaseLyricsFontSizeCommand_StepsDownAndPersists()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.DecreaseLyricsFontSizeCommand.Execute(null);

        sut.ActiveLyricsFontSize.Should().Be(44, "한 단계 -4");
    }

    [Fact]
    public void LyricsFontSizeCommands_ClampToRange()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        // 하한(24) 아래로는 내려가지 않는다.
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 24);
        sut.DecreaseLyricsFontSizeCommand.CanExecute(null).Should().BeFalse("하한에서 감소 비활성");

        // 상한(120) 위로는 올라가지 않는다.
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 120);
        sut.IncreaseLyricsFontSizeCommand.CanExecute(null).Should().BeFalse("상한에서 증가 비활성");
    }

    [Fact]
    public void IncreaseLyricsFontSizeCommand_FromNonStepValue_ClampsToMax()
    {
        // 외부 경로(Settings 창/legacy import)가 4의 배수가 아닌 118 을 넣은 뒤 +4 → 120 으로 클램프(122 아님).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 118);

        sut.IncreaseLyricsFontSizeCommand.Execute(null);

        sut.ActiveLyricsFontSize.Should().Be(120, "118+4=122 는 상한 120 으로 클램프");
        settings.Get(EasiSettingKeys.LyricsMonitorFontSize).Should().Be(120);
    }

    // ─── 인-셸 가사 폰트 효과(굵게·기울임·그림자) (§7.3-A) ─────────────────────

    [Fact]
    public void ToggleLyricsBoldCommand_FlipsSettingAndActive()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsBold.Should().BeFalse("기본 굵게 off");

        sut.ToggleLyricsBoldCommand.Execute(null);

        sut.ActiveLyricsBold.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorBold).Should().BeTrue();

        sut.ToggleLyricsBoldCommand.Execute(null);
        sut.ActiveLyricsBold.Should().BeFalse("다시 누르면 off");
    }

    [Fact]
    public void ToggleLyricsNotationsCommand_FlipsSettingAndActive()
    {
        // FrmMain Def_Notations — "코드 표시" 토글이 설정과 활성 상태를 뒤집는다(켜면 송출 본문에 코드 줄).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsNotations.Should().BeFalse("기본 코드 표시 off");

        sut.ToggleLyricsNotationsCommand.Execute(null);

        sut.ActiveLyricsNotations.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorShowNotations).Should().BeTrue();

        sut.ToggleLyricsNotationsCommand.Execute(null);
        sut.ActiveLyricsNotations.Should().BeFalse("다시 누르면 off");
    }

    [Fact]
    public void ToggleLyricsItalicCommand_FlipsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ToggleLyricsItalicCommand.Execute(null);

        sut.ActiveLyricsItalic.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorItalic).Should().BeTrue();
    }

    [Fact]
    public void ToggleLyricsShadowCommand_FlipsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ToggleLyricsShadowCommand.Execute(null);

        sut.ActiveLyricsShadow.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorShadow).Should().BeTrue();
    }

    [Fact]
    public void TogglePanelTransparentCommand_FlipsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsPanelTransparent.Should().BeFalse("기본 off");

        sut.TogglePanelTransparentCommand.Execute(null);

        sut.ActiveLyricsPanelTransparent.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorPanelTransparent).Should().BeTrue();
        sut.StatusText.Should().Contain("패널 투명 배경").And.NotContain("liveOutput", "원시 설정 ID 가 새지 않고 한글 라벨 표시");
    }

    [Fact]
    public void FontEffectActiveFlags_ReflectCurrentSettings_DefaultOff()
    {
        var sut = CreateSut();

        sut.ActiveLyricsBold.Should().BeFalse();
        sut.ActiveLyricsItalic.Should().BeFalse();
        sut.ActiveLyricsShadow.Should().BeFalse();
    }

    // ─── 자동 회전(Auto Rotate, §7.3-B) ──────────────────────────────────────

    [Fact]
    public async Task ToggleAutoRotateCommand_WhenLive_EnablesRotation()
    {
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.ToggleAutoRotateCommand.CanExecute(null).Should().BeTrue("라이브 중 자동 회전 토글 가능");
        sut.ToggleAutoRotateCommand.Execute(null);

        sut.IsAutoRotating.Should().BeTrue();
    }

    [Fact]
    public void ToggleAutoRotateCommand_WhenNotLive_IsDisabled()
    {
        var sut = CreateSut();

        sut.ToggleAutoRotateCommand.CanExecute(null).Should().BeFalse("라이브가 아니면 자동 회전 비활성");
    }

    [Fact]
    public async Task AdvanceAutoRotation_MultiVerseSong_LoopsThroughVerses()
    {
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절\n[3]\n3절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.LyricsPageIndex.Should().Be(0);

        sut.AdvanceAutoRotation();
        sut.LyricsPageIndex.Should().Be(1, "다음 절");

        sut.AdvanceAutoRotation();
        sut.LyricsPageIndex.Should().Be(2, "다음 절");

        sut.AdvanceAutoRotation();
        sut.LyricsPageIndex.Should().Be(0, "마지막 절 다음은 첫 절로 순환");
    }

    [Fact]
    public async Task AdvanceAutoRotation_MultiSlidePpt_LoopsThroughSlides()
    {
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        powerPoint.SlideNumber.Should().Be(1);

        sut.AdvanceAutoRotation();
        powerPoint.SlideNumber.Should().Be(2);

        sut.AdvanceAutoRotation();
        powerPoint.SlideNumber.Should().Be(3);

        sut.AdvanceAutoRotation();
        powerPoint.SlideNumber.Should().Be(1, "마지막 슬라이드 다음은 첫 슬라이드로 순환(SlideCount=3)");
    }

    [Fact]
    public void SlideTransitionKindInput_PersistsAndSyncs()
    {
        // 슬라이드/절 전환 종류 선택이 설정에 저장되고, SettingsChanged 경유로 인스펙터 표시가 동기화된다(항목 전환과 별개).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveSlideTransitionKind.Should().Be(LyricsTransitionKind.Fade, "기본 Fade(기존 단일 전환과 동일)");

        sut.SlideTransitionKindInput = LyricsTransitionKind.WipeUp;
        settings.Get(EasiSettingKeys.LyricsMonitorSlideTransitionKind).Should().Be(LyricsTransitionKind.WipeUp);
        sut.ActiveSlideTransitionKind.Should().Be(LyricsTransitionKind.WipeUp, "인스펙터 표시도 동기화");

        // 다른 경로(설정 직접 변경)도 RefreshActiveAppearance 로 따라간다.
        settings.Set(EasiSettingKeys.LyricsMonitorSlideTransitionKind, LyricsTransitionKind.ZoomOut);
        sut.ActiveSlideTransitionKind.Should().Be(LyricsTransitionKind.ZoomOut);
    }

    [Fact]
    public void TransitionKindOptions_CoversAllKinds()
    {
        // 슬라이드 전환 콤보가 모든 전환 종류를 노출하는지(항목 전환 메뉴와 동일 집합).
        var sut = CreateSut();
        sut.TransitionKindOptions.Select(o => o.Value)
            .Should().BeEquivalentTo(System.Enum.GetValues<LyricsTransitionKind>());
    }

    [Fact]
    public void IsInspectorExpanded_TogglePersists_AndIsRestoredOnConstruction()
    {
        // 인스펙터를 접으면 설정에 저장되고, 같은 설정으로 새 VM 을 만들면 접힌 상태로 복원된다(레거시 패널 상태 저장).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.IsInspectorExpanded.Should().BeTrue("기본 펼침");

        sut.IsInspectorExpanded = false; // 접기
        settings.Get(EasiSettingKeys.MainInspectorExpanded).Should().BeFalse("접힘 상태가 설정에 저장");

        // 같은 설정을 쓰는 새 VM 은 접힌 상태로 시작.
        var restored = CreateSut(settings: settings);
        restored.IsInspectorExpanded.Should().BeFalse("다음 실행에도 접힌 채 복원");
    }

    [Fact]
    public void ClearWorshipList_RemovesAll_AndRestoreBringsThemBack()
    {
        // 예배 순서 전체 비우기 → 되돌리기로 같은 항목들이 같은 순서로 복구된다(레거시 Empty→Trash 복구 대응).
        var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song-1", "곡A", "Song") { Lyrics = "가사A" },
            new LiveQueueItem("song-2", "곡B", "Song") { Lyrics = "가사B" },
            new LiveQueueItem("song-3", "곡C", "Song") { Lyrics = "가사C" },
        });
        sut.Queue.Should().HaveCount(3);

        sut.ClearWorshipListCommand.Execute(null);

        sut.Queue.Should().BeEmpty("전체 비우기 후 큐가 빈다");
        sut.SelectedItem.Should().BeNull("비운 뒤 선택 없음");

        sut.RestoreClearedWorshipListCommand.Execute(null);

        sut.Queue.Select(i => i.Id).Should().Equal(new[] { "song-1", "song-2", "song-3" }, "되돌리기로 같은 순서 복구");
        sut.SelectedItem.Should().Be(sut.Queue[0], "복구 후 첫 항목 선택");
    }

    [Fact]
    public void ClearWorshipList_CanExecute_FollowsQueueAndRestoreFollowsBackup()
    {
        // 비우기는 항목이 있을 때만, 되돌리기는 방금 비운 스냅샷이 있을 때만 활성.
        var sut = CreateSut(seedSampleQueue: false);
        sut.ClearWorshipListCommand.CanExecute(null).Should().BeFalse("빈 큐는 비울 게 없음");
        sut.RestoreClearedWorshipListCommand.CanExecute(null).Should().BeFalse("복구할 스냅샷 없음");

        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "곡A", "Song") { Lyrics = "가사A" } });
        sut.ClearWorshipListCommand.CanExecute(null).Should().BeTrue("항목이 있으면 비울 수 있음");

        sut.ClearWorshipListCommand.Execute(null);
        sut.ClearWorshipListCommand.CanExecute(null).Should().BeFalse("비운 뒤엔 다시 비울 게 없음");
        sut.RestoreClearedWorshipListCommand.CanExecute(null).Should().BeTrue("방금 비웠으니 복구 가능");

        sut.RestoreClearedWorshipListCommand.Execute(null);
        sut.RestoreClearedWorshipListCommand.CanExecute(null).Should().BeFalse("복구 후 스냅샷 비워져 중복 복구 불가");
        sut.ClearWorshipListCommand.CanExecute(null).Should().BeTrue("복구로 항목이 돌아왔으니 다시 비울 수 있음");
    }

    [Fact]
    public void ClearWorshipList_EmptyQueue_IsNoOp()
    {
        // 빈 큐에서 비우기를 실행해도 스냅샷이 생기지 않아 되돌리기가 활성화되지 않는다(빈 상태 보호).
        var sut = CreateSut(seedSampleQueue: false);

        sut.ClearWorshipListCommand.Execute(null);

        sut.Queue.Should().BeEmpty();
        sut.RestoreClearedWorshipListCommand.CanExecute(null).Should().BeFalse("빈 큐 비우기는 무동작 — 복구할 것 없음");
    }

    [Fact]
    public void AutoRotateModeInput_DefaultsToOneRepeat_AndPersists()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveAutoRotateMode.Should().Be(AutoRotateMode.OneRepeat, "기본 OneRepeat=기존 동작(무회귀)");

        sut.AutoRotateModeInput = AutoRotateMode.GroupRepeat;
        settings.Get(EasiSettingKeys.AutoRotateMode).Should().Be(AutoRotateMode.GroupRepeat, "선택이 설정에 저장");
        sut.ActiveAutoRotateMode.Should().Be(AutoRotateMode.GroupRepeat, "인스펙터 표시도 동기화");
    }

    [Fact]
    public async Task AdvanceAutoRotation_OneMode_StopsAtEndOfItem()
    {
        // "한 항목만" 모드 — 마지막 절까지 가면 자동 회전을 멈춘다(첫 절로 순환하지 않음).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.AutoRotateMode, AutoRotateMode.One);
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.ToggleAutoRotateCommand.Execute(null);
        sut.IsAutoRotating.Should().BeTrue();

        sut.AdvanceAutoRotation(); // 0 -> 1(마지막 절)
        sut.LyricsPageIndex.Should().Be(1);
        sut.IsAutoRotating.Should().BeTrue("아직 마지막 절 송출 중");

        sut.AdvanceAutoRotation(); // 끝 -> One 모드는 정지
        sut.IsAutoRotating.Should().BeFalse("한 항목만 모드는 끝나면 자동 회전 정지");
        sut.LyricsPageIndex.Should().Be(1, "정지 시 절은 그대로(첫 절로 순환하지 않음)");
    }

    [Fact]
    public async Task AdvanceAutoRotation_GroupMode_AdvancesToNextItemThenStops()
    {
        // "그룹" 모드 — 현재 항목 끝나면 다음 예배 순서 항목으로, 마지막 항목 끝나면 멈춘다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.AutoRotateMode, AutoRotateMode.Group);
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song-1", "곡A", "Song") { Lyrics = "[1]\n가사A" }, // 단일 절
            new LiveQueueItem("song-2", "곡B", "Song") { Lyrics = "[1]\n가사B" },
        });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.ToggleAutoRotateCommand.Execute(null);

        sut.AdvanceAutoRotation(); // 곡A(단일 절) 끝 -> 다음 항목(곡B)
        sut.SelectedItem.Should().Be(sut.Queue[1], "그룹 모드는 다음 항목으로 이동");
        sut.IsAutoRotating.Should().BeTrue("아직 마지막 항목 아님 → 회전 유지");

        sut.AdvanceAutoRotation(); // 곡B 끝 -> 다음 없음 -> 그룹 끝 정지
        sut.IsAutoRotating.Should().BeFalse("마지막 항목 끝 → 그룹 모드 정지");
    }

    [Fact]
    public async Task AdvanceAutoRotation_GroupRepeatMode_WrapsToFirstItem()
    {
        // "그룹 반복" 모드 — 마지막 항목 끝나면 첫 항목으로 돌아가 계속 순환.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.AutoRotateMode, AutoRotateMode.GroupRepeat);
        var sut = CreateSut(settings: settings);
        sut.LoadQueue(new[]
        {
            new LiveQueueItem("song-1", "곡A", "Song") { Lyrics = "[1]\n가사A" },
            new LiveQueueItem("song-2", "곡B", "Song") { Lyrics = "[1]\n가사B" },
        });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.ToggleAutoRotateCommand.Execute(null);

        sut.AdvanceAutoRotation(); // 곡A 끝 -> 곡B
        sut.SelectedItem.Should().Be(sut.Queue[1]);

        sut.AdvanceAutoRotation(); // 곡B 끝 -> 다음 없음 -> 첫 항목(곡A)으로 순환
        sut.SelectedItem.Should().Be(sut.Queue[0], "그룹 반복은 마지막 항목 끝나면 첫 항목으로 순환");
        sut.IsAutoRotating.Should().BeTrue("반복 모드라 계속 회전");
    }

    [Fact]
    public async Task AutoRotate_StopsWhenLiveEnds()
    {
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.ToggleAutoRotateCommand.Execute(null);
        sut.IsAutoRotating.Should().BeTrue();

        await sut.StopLiveCommand.ExecuteAsync(null);

        sut.IsAutoRotating.Should().BeFalse("라이브 종료 시 자동 회전도 자동 해제");
    }

    [Fact]
    public async Task AutoRotate_SurvivesHideAndRestore_OnlyStopsOnLiveEnd()
    {
        // 숨김/복귀는 임시 상태 — 자동 회전이 꺼지지 않고 이어진다(완전 종료에서만 해제).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.ToggleAutoRotateCommand.Execute(null);
        sut.IsAutoRotating.Should().BeTrue();

        await sut.HideOutputCommand.ExecuteAsync(null);
        sut.IsAutoRotating.Should().BeTrue("숨김은 임시 상태라 자동 회전 유지");

        sut.RestoreOutputCommand.Execute(null);
        sut.IsAutoRotating.Should().BeTrue("복귀 후에도 자동 회전 유지");
    }

    [Fact]
    public void AutoRotateIntervalSeconds_ReflectsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.AutoRotateIntervalSeconds, 15);
        var sut = CreateSut(settings: settings);

        sut.AutoRotateIntervalSeconds.Should().Be(15);
    }

    // ─── 출력 위치 인디케이터(절/슬라이드 "N/M") (§7.3-A) ─────────────────────

    [Fact]
    public async Task GoLive_MultiVerseSong_CarriesPositionLabel()
    {
        // 다절 곡 라이브 시 "1/3" 같은 위치 라벨이 세션 스냅샷에 실린다.
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절\n[3]\n3절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemPositionLabel.Should().Be("1/3");

        sut.NextLyricsPageCommand.Execute(null);
        sut.Session.Current.CurrentItemPositionLabel.Should().Be("2/3", "절 이동 시 갱신");
    }

    [Fact]
    public async Task LiveBar_ShowsLivePosition_AndUpdatesOnVerseNavigation()
    {
        // 증분142 — 상단 LiveBar 가 라이브 위치(곡 절 "1/3")를 보여 주고, 절 이동마다 갱신된다(세션 스냅샷→LiveBar.PositionLabel).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "[1]\n1절\n[2]\n2절\n[3]\n3절" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LiveBar.PositionLabel.Should().Be("1/3", "LiveBar 가 라이브 위치 표시");

        sut.NextLyricsPageCommand.Execute(null);
        sut.LiveBar.PositionLabel.Should().Be("2/3", "절 이동 시 LiveBar 위치도 갱신");
    }

    [Fact]
    public async Task LiveBar_ShowsNextUpItem_AndEmptyOnLastItem()
    {
        // 증분143 — 상단 LiveBar 가 다음 예정 항목을 보여 준다(운영자 준비용). 마지막 항목이면 빈 문자열(다음 줄 숨김).
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:a", "1부 찬양", "Song") { Lyrics = "[1]\n가" };
        var b = new LiveQueueItem("song:b", "2부 찬양", "Song") { Lyrics = "[1]\n나" };
        sut.LoadQueue([a, b]);
        sut.OpenOutputCommand.Execute(null);

        sut.SelectedItem = a;
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.LiveBar.NextItemTitle.Should().Be("2부 찬양", "현재 항목 다음 항목을 미리 표시");

        sut.SelectedItem = b;
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.LiveBar.NextItemTitle.Should().BeEmpty("마지막 항목이면 다음 없음");
    }

    [Fact]
    public async Task GoLive_SingleVerseSong_HasEmptyPositionLabel()
    {
        // 단일 절 곡은 위치 라벨이 비어 있다(표시 의미 없음).
        var sut = CreateSut();
        sut.LoadQueue(new[] { new LiveQueueItem("song-1", "은혜로다", "Song") { Lyrics = "한 절뿐" } });
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemPositionLabel.Should().BeEmpty();
    }

    [Fact]
    public async Task GoToSlide_MultiSlidePpt_UpdatesPositionLabel()
    {
        // PPT 슬라이드 이동 시 위치 라벨이 갱신된다(SuccessStub 은 SlideCount=3).
        var powerPoint = new PowerPointPreviewViewModel(new SuccessPowerPointRenderService(), _ => Frozen());
        var sut = CreateSut(powerPoint: powerPoint);
        sut.LoadQueue(new[] { new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = "deck.pptx" } });
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemPositionLabel.Should().Be("1/3");

        await sut.NextSlideCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemPositionLabel.Should().Be("2/3", "슬라이드 이동 시 위치 라벨 갱신");
    }

    [Fact]
    public void ToggleLyricsPositionIndicatorCommand_FlipsSetting()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsPositionIndicator.Should().BeFalse("기본 off");

        sut.ToggleLyricsPositionIndicatorCommand.Execute(null);

        sut.ActiveLyricsPositionIndicator.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorShowPositionIndicator).Should().BeTrue();
    }

    [Fact]
    public void ToggleLyricsTitleHeadingCommand_FlipsSetting()
    {
        // 제목 헤딩 토글: 설정 반전 + 인스펙터 활성 상태 동기화(§7.3-A).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsTitleHeading.Should().BeFalse("기본 off");

        sut.ToggleLyricsTitleHeadingCommand.Execute(null);

        sut.ActiveLyricsTitleHeading.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading).Should().BeTrue();

        sut.ToggleLyricsTitleHeadingCommand.Execute(null);
        sut.ActiveLyricsTitleHeading.Should().BeFalse("다시 누르면 off");
    }

    [Fact]
    public void ToggleTitleHeadingFirstScreenOnlyCommand_FlipsSetting()
    {
        // "At First Screen Only" 토글: 설정 반전 + 인스펙터 활성 상태 동기화(§7.3-A).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveTitleHeadingFirstScreenOnly.Should().BeFalse("기본 off");

        sut.ToggleTitleHeadingFirstScreenOnlyCommand.Execute(null);

        sut.ActiveTitleHeadingFirstScreenOnly.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly).Should().BeTrue();

        sut.ToggleTitleHeadingFirstScreenOnlyCommand.Execute(null);
        sut.ActiveTitleHeadingFirstScreenOnly.Should().BeFalse("다시 누르면 off");
    }

    [Fact]
    public void ToggleLyricsOutlineCommand_FlipsSetting()
    {
        // 외곽선 효과 토글: 설정 반전 + 인스펙터 활성 상태 동기화(§7.3-A 폰트 효과).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsOutline.Should().BeFalse("기본 off");

        sut.ToggleLyricsOutlineCommand.Execute(null);

        sut.ActiveLyricsOutline.Should().BeTrue();
        settings.Get(EasiSettingKeys.LyricsMonitorOutline).Should().BeTrue();

        sut.ToggleLyricsOutlineCommand.Execute(null);
        sut.ActiveLyricsOutline.Should().BeFalse("다시 누르면 off");
    }

    [Theory]
    [InlineData(LyricsTextAlignment.Left)]
    [InlineData(LyricsTextAlignment.Right)]
    [InlineData(LyricsTextAlignment.Center)]
    public void ApplyTitleHeadingAlignmentCommand_SetsSettingAndActive(LyricsTextAlignment alignment)
    {
        // 제목 헤딩 정렬 적용: 설정 저장 + 인스펙터 활성 상태 동기화(§7.3-A).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyTitleHeadingAlignmentCommand.Execute(alignment);

        sut.ActiveTitleHeadingAlignment.Should().Be(alignment);
        settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment).Should().Be(alignment);
    }

    [Fact]
    public void ActiveTitleHeadingAlignment_DefaultsCenter()
    {
        var sut = CreateSut();
        sut.ActiveTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Center, "기본 가운데");
    }

    // ─── 출력 모양 설정 템플릿(저장/불러오기) (§7.3-A) ────────────────────────

    [Fact]
    public async Task SaveAppearanceTemplateCommand_PersistsCurrentAppearanceUnderName()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 72);
        var store = new InMemoryAppearanceTemplateStore();
        var sut = CreateSut(settings: settings, appearanceTemplates: store);
        sut.NewAppearanceTemplateName = "주일예배";

        await sut.SaveAppearanceTemplateCommand.ExecuteAsync(null);

        sut.AppearanceTemplateNames.Should().Contain("주일예배");
        (await store.LoadAsync("주일예배"))!.FontSize.Should().Be(72);
    }

    [Fact]
    public async Task ApplyAppearanceTemplateCommand_RestoresSavedAppearance()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 80);
        var store = new InMemoryAppearanceTemplateStore();
        var sut = CreateSut(settings: settings, appearanceTemplates: store);
        sut.NewAppearanceTemplateName = "큰글씨";
        await sut.SaveAppearanceTemplateCommand.ExecuteAsync(null);

        // 폰트를 바꾼 뒤 템플릿을 적용하면 저장 시점 값으로 복원된다.
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 40);
        sut.SelectedAppearanceTemplate = "큰글씨";
        await sut.ApplyAppearanceTemplateCommand.ExecuteAsync(null);

        settings.Get(EasiSettingKeys.LyricsMonitorFontSize).Should().Be(80);
        sut.ActiveLyricsFontSize.Should().Be(80, "인스펙터 표시도 복원");
    }

    [Fact]
    public async Task DeleteAppearanceTemplateCommand_RemovesTemplate()
    {
        var store = new InMemoryAppearanceTemplateStore();
        var sut = CreateSut(appearanceTemplates: store);
        sut.NewAppearanceTemplateName = "임시";
        await sut.SaveAppearanceTemplateCommand.ExecuteAsync(null);
        sut.SelectedAppearanceTemplate = "임시";

        sut.DeleteAppearanceTemplateCommand.Execute(null);

        sut.AppearanceTemplateNames.Should().NotContain("임시");
    }

    [Fact]
    public async Task ApplyAppearanceTemplateCommand_MissingTemplate_WarnsAndRefreshes()
    {
        // 외부에서 삭제돼 사라진 템플릿을 적용하면 경고 + 목록 새로고침(크래시 없음).
        var store = new InMemoryAppearanceTemplateStore();
        var sut = CreateSut(appearanceTemplates: store);
        sut.NewAppearanceTemplateName = "사라질것";
        await sut.SaveAppearanceTemplateCommand.ExecuteAsync(null);
        sut.SelectedAppearanceTemplate = "사라질것";
        store.Delete("사라질것"); // VM 모르게 외부 삭제

        await sut.ApplyAppearanceTemplateCommand.ExecuteAsync(null);

        sut.StatusText.Should().Contain("찾을 수 없");
        sut.AppearanceTemplateNames.Should().NotContain("사라질것", "적용 실패 시 목록 새로고침");
    }

    [Fact]
    public async Task SaveAppearanceTemplateCommand_EmptyName_DoesNothing()
    {
        var store = new InMemoryAppearanceTemplateStore();
        var sut = CreateSut(appearanceTemplates: store);
        sut.NewAppearanceTemplateName = "   ";

        await sut.SaveAppearanceTemplateCommand.ExecuteAsync(null);

        sut.AppearanceTemplateNames.Should().BeEmpty();
    }

    // ─── 인-셸 세분 색 직접 지정(hex) (§7.3-A) ────────────────────────────────

    [Fact]
    public void ApplyPanelColorHexCommand_KeepsSemiTransparentAlpha()
    {
        // FrmMain Def_PanelColour — RGB 만 바꾸고 반투명 알파(0x66)는 유지(밴드 뒤 가사 비침).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyPanelColorHexCommand.Execute("#102040");

        settings.Get(EasiSettingKeys.LyricsMonitorPanelColorArgb).Should().Be(unchecked((int)0x66102040), "RGB=102040 + 반투명 알파 66");
    }

    [Fact]
    public void ApplyPanelColorHexCommand_InvalidHex_LeavesSettingUnchanged()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var before = settings.Get(EasiSettingKeys.LyricsMonitorPanelColorArgb);

        sut.ApplyPanelColorHexCommand.Execute("not-a-color");

        settings.Get(EasiSettingKeys.LyricsMonitorPanelColorArgb).Should().Be(before, "형식 오류면 변경 없음");
    }

    [Fact]
    public void ApplyTextColorHexCommand_SetsArgbWithFullAlpha()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyTextColorHexCommand.Execute("#FF0000");

        settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(unchecked((int)0xFFFF0000), "빨강 + 알파 FF");
        sut.ActiveTextColorHex.Should().Be("#FF0000");
    }

    [Fact]
    public void ApplyTextColorHexCommand_AcceptsHexWithoutHash()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyTextColorHexCommand.Execute("00FF00");

        settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(unchecked((int)0xFF00FF00));
    }

    [Fact]
    public void ApplyTextColorHexCommand_InvalidHex_LeavesSettingAndWarns()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var before = settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb);

        sut.ApplyTextColorHexCommand.Execute("not-a-color");

        settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(before, "잘못된 hex 는 무시");
        sut.StatusText.Should().Contain("색");
    }

    [Fact]
    public void ApplyBackgroundColorHexCommand_SetsSolidBackground()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ApplyBackgroundColorHexCommand.Execute("#000080");

        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb).Should().Be(unchecked((int)0xFF000080));
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb).Should().Be(unchecked((int)0xFF000080), "솔리드라 끝색=시작색");
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient).Should().BeFalse("hex 직접 지정은 솔리드");
        sut.ActiveBackgroundColorHex.Should().Be("#000080");
    }

    [Fact]
    public void ActiveColorHex_ReflectsCurrentSettings()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFFABCDEF));
        var sut = CreateSut(settings: settings);

        sut.ActiveTextColorHex.Should().Be("#ABCDEF");
    }

    [Fact]
    public void ApplyTextColorHexCommand_RejectsEightDigitAlphaForSymmetry()
    {
        // 표시는 6자리(RGB)이므로 8자리(알파) 입력은 받지 않는다(저장/표시 비대칭 방지).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var before = settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb);

        sut.ApplyTextColorHexCommand.Execute("#80FF0000");

        settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(before, "8자리는 거부");
    }

    [Fact]
    public void ApplyBackgroundColorHexCommand_AfterGradientPreset_SwitchesToSolid()
    {
        // 그라데이션 프리셋(IsGradient=true) 적용 후 hex 배경을 지정하면 솔리드로 전환된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var gradientPreset = sut.OutputAppearancePresets.First(p => p.IsGradient);
        sut.ApplyOutputAppearanceCommand.Execute(gradientPreset);
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient).Should().BeTrue("프리셋이 그라데이션 설정");

        sut.ApplyBackgroundColorHexCommand.Execute("#123456");

        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient).Should().BeFalse("hex 배경은 솔리드로 전환");
        settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb).Should().Be(unchecked((int)0xFF123456));
    }

    [Fact]
    public void ActiveColorHex_FollowsPresetApplication()
    {
        // 프리셋 적용 시에도 hex 표시가 프리셋 색을 따라간다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        var preset = sut.OutputAppearancePresets.First(p => p.Name == "검정 글자 · 흰 배경");

        sut.ApplyOutputAppearanceCommand.Execute(preset);

        sut.ActiveTextColorHex.Should().Be("#000000");
        sut.ActiveBackgroundColorHex.Should().Be("#FFFFFF");
    }

    // ─── 인-셸 가사 줄 간격 (§7.3-A) ──────────────────────────────────────────

    [Fact]
    public void IncreaseLyricsLineSpacingCommand_StepsUpAndPersists()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsLineSpacing.Should().Be(125, "기본 125%");

        sut.IncreaseLyricsLineSpacingCommand.Execute(null);

        sut.ActiveLyricsLineSpacing.Should().Be(135, "한 단계 +10");
        settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent).Should().Be(135);
    }

    [Fact]
    public void DecreaseLyricsLineSpacingCommand_StepsDown()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.DecreaseLyricsLineSpacingCommand.Execute(null);

        sut.ActiveLyricsLineSpacing.Should().Be(115, "한 단계 -10");
    }

    [Fact]
    public void LyricsLineSpacingCommands_ClampToRange()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, 100);
        sut.DecreaseLyricsLineSpacingCommand.CanExecute(null).Should().BeFalse("하한(100%)에서 감소 비활성");

        settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, 220);
        sut.IncreaseLyricsLineSpacingCommand.CanExecute(null).Should().BeFalse("상한(220%)에서 증가 비활성");
    }

    [Fact]
    public void LyricsFontSizeInput_DirectEntry_ClampsAndPersists()
    {
        // FrmMain NumericUpDown 직접 수치 입력 대응 — 범위 안 값은 그대로 저장, 범위 밖은 클램프.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsFontSizeInput = 80;
        sut.ActiveLyricsFontSize.Should().Be(80, "범위 안(24~120) 값은 그대로 적용");
        settings.Get(EasiSettingKeys.LyricsMonitorFontSize).Should().Be(80);

        sut.LyricsFontSizeInput = 999;
        sut.ActiveLyricsFontSize.Should().Be(120, "상한 120px 으로 클램프");
        sut.LyricsFontSizeInput.Should().Be(120, "입력 박스도 클램프값으로 보정");

        sut.LyricsFontSizeInput = 1;
        sut.ActiveLyricsFontSize.Should().Be(24, "하한 24px 으로 클램프");
    }

    [Fact]
    public void LyricsFontFamilyInput_SetAndClear_PersistsAndTrims()
    {
        // 출력 전역 글꼴명 입력 — 앞뒤 공백은 다듬어 저장, 비우면 기본(테마 상속="") 으로 저장.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsFontFamilyInput = "  Malgun Gothic  ";
        sut.ActiveLyricsFontFamily.Should().Be("Malgun Gothic", "앞뒤 공백을 다듬어 적용");
        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily).Should().Be("Malgun Gothic");
        sut.LyricsFontFamilyInput.Should().Be("Malgun Gothic", "콤보 표시도 동기화");

        sut.LyricsFontFamilyInput = "   ";
        sut.ActiveLyricsFontFamily.Should().BeEmpty("공백만이면 기본(테마 상속)");
        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily).Should().BeEmpty();
    }

    [Fact]
    public void MergeInstalledFonts_AddsInstalledAfterFavorites_KeepingCuratedFirst()
    {
        // 시작 시 설치 글꼴 병합: 추천(맑은 고딕 등)이 맨 앞 그대로, 설치된 새 글꼴은 정렬돼 뒤에 붙는다.
        var sut = CreateSut();

        sut.MergeInstalledFonts(new[] { "Zapfino", "Comic Sans MS", "Malgun Gothic" });

        sut.LyricsFontFamilyOptions[0].Should().Be("맑은 고딕", "추천 글꼴이 맨 앞 순서 유지");
        sut.LyricsFontFamilyOptions.Should().ContainInOrder("Comic Sans MS", "Zapfino"); // 설치분은 ABC 순.
        sut.LyricsFontFamilyOptions.Count(n => n == "Malgun Gothic").Should().Be(1, "추천에 이미 있는 글꼴은 중복 안 됨");
    }

    [Fact]
    public void MergeInstalledFonts_PreservesTypedFontSelection()
    {
        // 핵심 안전성: 콤보 목록을 교체해도(Clear+Add) 운영자가 고른/입력한 글꼴(Text 바인딩)은 그대로 유지돼야 한다.
        // 목록에 없는 글꼴명을 직접 입력한 경우(편집 콤보)에도 사라지지 않아야 한다.
        var sut = CreateSut();
        sut.LyricsFontFamilyInput = "전혀없는글꼴"; // 목록 밖 직접 입력.

        sut.MergeInstalledFonts(new[] { "Arial", "Tahoma" });

        sut.LyricsFontFamilyInput.Should().Be("전혀없는글꼴", "목록 교체는 입력한 글꼴 선택을 건드리지 않음");
        sut.ActiveLyricsFontFamily.Should().Be("전혀없는글꼴");
    }

    [Fact]
    public void MergeInstalledFonts_CalledTwice_DoesNotAccumulateDuplicates()
    {
        // 멱등성: 두 번 병합해도(시작 경로 중복 호출 대비) 목록이 누적·중복되지 않는다(Clear 후 다시 채우므로).
        var sut = CreateSut();

        sut.MergeInstalledFonts(new[] { "Arial", "Tahoma" });
        var afterFirst = sut.LyricsFontFamilyOptions.ToList();
        sut.MergeInstalledFonts(new[] { "Arial", "Tahoma" });

        sut.LyricsFontFamilyOptions.Should().Equal(afterFirst, "같은 입력 재병합은 같은 결과(누적 없음)");
    }

    [Fact]
    public void MergeInstalledFonts_NullOrEmpty_KeepsCuratedFavorites()
    {
        // 설치 글꼴 열거 실패(null/빈)면 추천 목록을 그대로 유지(무회귀 — 콤보가 비지 않게).
        var sut = CreateSut();
        var before = sut.LyricsFontFamilyOptions.ToList();

        sut.MergeInstalledFonts(null);

        sut.LyricsFontFamilyOptions.Should().Equal(before, "null 병합은 추천 목록을 바꾸지 않음");
        sut.LyricsFontFamilyOptions.Should().Contain("맑은 고딕").And.Contain("Arial");
    }

    [Fact]
    public void LyricsFontFamily2Input_SetAndClear_PersistsAndTrims()
    {
        // 보조영역(Region2) 전역 글꼴명 입력 — 앞뒤 공백 다듬어 저장, 비우면 본문 글꼴 추종("")으로 저장.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsFontFamily2Input = "  Gulim  ";
        sut.ActiveLyricsFontFamily2.Should().Be("Gulim", "앞뒤 공백을 다듬어 적용");
        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily2).Should().Be("Gulim");
        sut.LyricsFontFamily2Input.Should().Be("Gulim", "콤보 표시도 동기화");

        sut.LyricsFontFamily2Input = "   ";
        sut.ActiveLyricsFontFamily2.Should().BeEmpty("공백만이면 본문 글꼴 추종");
        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily2).Should().BeEmpty();
    }

    [Fact]
    public void LyricsTextColor2Input_SetAndClear_PersistsAndSyncs()
    {
        // 보조영역(Region2) 전역 글자색 — 색 지정/해제(0=본문 추종)가 설정에 저장되고 인스펙터가 동기화된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveLyricsTextColor2Argb.Should().Be(0, "기본 0=본문 색 추종");

        var yellow = unchecked((int)0xFFFFE066);
        sut.LyricsTextColor2Input = yellow;
        settings.Get(EasiSettingKeys.LyricsMonitorTextColor2Argb).Should().Be(yellow);
        sut.ActiveLyricsTextColor2Argb.Should().Be(yellow, "인스펙터 표시도 동기화");

        sut.LyricsTextColor2Input = 0;
        settings.Get(EasiSettingKeys.LyricsMonitorTextColor2Argb).Should().Be(0, "0=본문 색 추종으로 복귀");
    }

    [Fact]
    public void LyricsRegion2UnderlineInput_SetAndClear_PersistsAndSyncs()
    {
        // 보조영역(Region2) 전역 밑줄(3-상태) — On/Off/추종 전환이 설정에 저장되고 인스펙터가 동기화된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveLyricsRegion2Underline.Should().Be(LyricsRegion2Emphasis.FollowRegion1, "기본=본문 밑줄 추종");

        sut.LyricsRegion2UnderlineInput = LyricsRegion2Emphasis.On;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Underline).Should().Be(LyricsRegion2Emphasis.On);
        sut.ActiveLyricsRegion2Underline.Should().Be(LyricsRegion2Emphasis.On, "인스펙터 표시도 동기화");

        sut.LyricsRegion2UnderlineInput = LyricsRegion2Emphasis.FollowRegion1;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Underline).Should().Be(LyricsRegion2Emphasis.FollowRegion1, "추종으로 복귀");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresRegion2UnderlineToDefault()
    {
        // "기본값으로 복원(전체)" 가 보조영역 밑줄까지 기본(FollowRegion1)으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsRegion2UnderlineInput = LyricsRegion2Emphasis.On;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Underline).Should().NotBe(LyricsRegion2Emphasis.FollowRegion1);

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Underline).Should().Be(
            LyricsRegion2Emphasis.FollowRegion1, "전체 복원은 보조영역 밑줄도 기본(추종)으로");
        sut.ActiveLyricsRegion2Underline.Should().Be(LyricsRegion2Emphasis.FollowRegion1, "인스펙터 표시도 기본으로");
    }

    [Fact]
    public void LyricsRegion2ItalicInput_SetAndClear_PersistsAndSyncs()
    {
        // 보조영역(Region2) 전역 기울임(3-상태) — On/Off/추종 전환이 설정에 저장되고 인스펙터가 동기화된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveLyricsRegion2Italic.Should().Be(LyricsRegion2Emphasis.FollowRegion1, "기본=본문 기울임 추종");

        sut.LyricsRegion2ItalicInput = LyricsRegion2Emphasis.On;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Italic).Should().Be(LyricsRegion2Emphasis.On);
        sut.ActiveLyricsRegion2Italic.Should().Be(LyricsRegion2Emphasis.On, "인스펙터 표시도 동기화");

        sut.LyricsRegion2ItalicInput = LyricsRegion2Emphasis.FollowRegion1;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Italic).Should().Be(LyricsRegion2Emphasis.FollowRegion1, "추종으로 복귀");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresRegion2ItalicToDefault()
    {
        // "기본값으로 복원(전체)" 가 보조영역 기울임까지 기본(FollowRegion1)으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsRegion2ItalicInput = LyricsRegion2Emphasis.On;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Italic).Should().NotBe(LyricsRegion2Emphasis.FollowRegion1);

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Italic).Should().Be(
            LyricsRegion2Emphasis.FollowRegion1, "전체 복원은 보조영역 기울임도 기본(추종)으로");
        sut.ActiveLyricsRegion2Italic.Should().Be(LyricsRegion2Emphasis.FollowRegion1, "인스펙터 표시도 기본으로");
    }

    [Fact]
    public void LyricsRegion2BoldInput_SetAndClear_PersistsAndSyncs()
    {
        // 보조영역(Region2) 전역 굵게(3-상태) — On/Off/추종 전환이 설정에 저장되고 인스펙터가 동기화된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveLyricsRegion2Bold.Should().Be(LyricsRegion2Emphasis.FollowRegion1, "기본=본문 굵게 추종");

        sut.LyricsRegion2BoldInput = LyricsRegion2Emphasis.On;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Bold).Should().Be(LyricsRegion2Emphasis.On);
        sut.ActiveLyricsRegion2Bold.Should().Be(LyricsRegion2Emphasis.On, "인스펙터 표시도 동기화");

        sut.LyricsRegion2BoldInput = LyricsRegion2Emphasis.FollowRegion1;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Bold).Should().Be(LyricsRegion2Emphasis.FollowRegion1, "추종으로 복귀");
    }

    [Fact]
    public void RefreshActiveAppearance_SyncsRegion2BoldFromSettings()
    {
        // 설정 창 등 다른 경로로 보조영역 굵게가 바뀌어도 인스펙터 표시가 따라가야 한다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.LyricsMonitorRegion2Bold, LyricsRegion2Emphasis.Off);

        sut.ActiveLyricsRegion2Bold.Should().Be(LyricsRegion2Emphasis.Off, "SettingsChanged 경유로 인스펙터 동기화");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresRegion2BoldToDefault()
    {
        // "기본값으로 복원(전체)" 가 보조영역 굵게까지 기본(FollowRegion1)으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsRegion2BoldInput = LyricsRegion2Emphasis.On;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Bold).Should().NotBe(LyricsRegion2Emphasis.FollowRegion1);

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Bold).Should().Be(
            LyricsRegion2Emphasis.FollowRegion1, "전체 복원은 보조영역 굵게도 기본(추종)으로");
        sut.ActiveLyricsRegion2Bold.Should().Be(LyricsRegion2Emphasis.FollowRegion1, "인스펙터 표시도 기본으로");
    }

    [Fact]
    public void LyricsRegion2AlignmentInput_SetAndClear_PersistsAndSyncs()
    {
        // 보조영역(Region2) 전역 정렬 — 정렬 지정/추종(FollowRegion1) 전환이 설정에 저장되고 인스펙터가 동기화된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.ActiveLyricsRegion2Alignment.Should().Be(LyricsRegion2Alignment.FollowRegion1, "기본=본문 정렬 추종");

        sut.LyricsRegion2AlignmentInput = LyricsRegion2Alignment.Left;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Alignment).Should().Be(LyricsRegion2Alignment.Left);
        sut.ActiveLyricsRegion2Alignment.Should().Be(LyricsRegion2Alignment.Left, "인스펙터 표시도 동기화");

        sut.LyricsRegion2AlignmentInput = LyricsRegion2Alignment.FollowRegion1;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Alignment).Should().Be(LyricsRegion2Alignment.FollowRegion1, "추종으로 복귀");
    }

    [Fact]
    public void RefreshActiveAppearance_SyncsRegion2AlignmentFromSettings()
    {
        // 설정 창 등 다른 경로로 보조영역 정렬이 바뀌어도 인스펙터 표시가 따라가야 한다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.LyricsMonitorRegion2Alignment, LyricsRegion2Alignment.Right);

        sut.ActiveLyricsRegion2Alignment.Should().Be(LyricsRegion2Alignment.Right, "SettingsChanged 경유로 인스펙터 동기화");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresRegion2AlignmentToDefault()
    {
        // "기본값으로 복원(전체)" 가 보조영역 정렬까지 기본(FollowRegion1)으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsRegion2AlignmentInput = LyricsRegion2Alignment.Center;
        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Alignment).Should().NotBe(LyricsRegion2Alignment.FollowRegion1);

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorRegion2Alignment).Should().Be(
            LyricsRegion2Alignment.FollowRegion1, "전체 복원은 보조영역 정렬도 기본(추종)으로");
        sut.ActiveLyricsRegion2Alignment.Should().Be(LyricsRegion2Alignment.FollowRegion1, "인스펙터 표시도 기본으로");
    }

    [Fact]
    public void RefreshActiveAppearance_SyncsTextColor2FromSettings()
    {
        // 설정 창 등 다른 경로로 보조영역 색이 바뀌어도 인스펙터 표시가 따라가야 한다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        var sky = unchecked((int)0xFF66CCFF);
        settings.Set(EasiSettingKeys.LyricsMonitorTextColor2Argb, sky);

        sut.ActiveLyricsTextColor2Argb.Should().Be(sky, "SettingsChanged 경유로 인스펙터 동기화");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresTextColor2ToDefault()
    {
        // "기본값으로 복원(전체)" 가 보조영역 색까지 기본(0=본문 추종)으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsTextColor2Input = unchecked((int)0xFFFFE066);
        settings.Get(EasiSettingKeys.LyricsMonitorTextColor2Argb).Should().NotBe(0);

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorTextColor2Argb).Should().Be(0, "전체 복원은 보조영역 색도 기본(0)으로");
        sut.ActiveLyricsTextColor2Argb.Should().Be(0, "인스펙터 표시도 기본으로");
    }

    [Fact]
    public void RefreshActiveAppearance_SyncsFontFamily2FromSettings()
    {
        // 설정 창 등 다른 경로로 보조영역 전역 글꼴이 바뀌어도 인스펙터 표시가 따라가야 한다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.LyricsMonitorFontFamily2, "바탕");

        sut.ActiveLyricsFontFamily2.Should().Be("바탕", "SettingsChanged 경유로 인스펙터 동기화");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresFontFamily2ToDefault()
    {
        // "기본값으로 복원(전체)" 가 보조영역 전역 글꼴까지 기본("")으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsFontFamily2Input = "Gulim";
        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily2).Should().Be("Gulim");

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily2).Should().Be(
            EasiSettingKeys.LyricsMonitorFontFamily2.DefaultValue, "전체 복원은 보조영역 글꼴도 기본으로");
        sut.ActiveLyricsFontFamily2.Should().BeEmpty("인스펙터 표시도 기본으로");
    }

    [Fact]
    public void RefreshActiveAppearance_SyncsFontFamilyFromSettings()
    {
        // 설정 창 등 다른 경로로 전역 글꼴이 바뀌어도 인스펙터 표시(ActiveLyricsFontFamily)가 따라가야 한다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.LyricsMonitorFontFamily, "나눔고딕");

        sut.ActiveLyricsFontFamily.Should().Be("나눔고딕", "SettingsChanged 경유로 인스펙터 동기화");
    }

    [Fact]
    public void ResetOutputAppearance_RestoresFontFamilyToDefault()
    {
        // "기본값으로 복원(전체)" 가 전역 글꼴까지 기본(테마 상속="")으로 되돌리는지(증분72 '전체' 정직성).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsFontFamilyInput = "Batang";
        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily).Should().Be("Batang");

        sut.ResetOutputAppearanceCommand.Execute(null);

        settings.Get(EasiSettingKeys.LyricsMonitorFontFamily).Should().Be(
            EasiSettingKeys.LyricsMonitorFontFamily.DefaultValue, "전체 복원은 글꼴도 기본으로");
        sut.ActiveLyricsFontFamily.Should().BeEmpty("인스펙터 표시도 기본으로");
    }

    [Fact]
    public void SelectedOutputDisplayChange_WhileOpen_MovesOutputToNewMonitor()
    {
        // 레거시 런타임 MoveTo — 출력 창이 열린 상태에서 모니터 선택을 바꾸면 그 모니터로 창이 즉시 이동한다.
        var output = new OutputWindowService();
        var sut = CreateSut(output: output);
        sut.OpenOutputCommand.Execute(null);
        output.Current.IsOpen.Should().BeTrue("출력 창이 열려 있어야 이동 테스트가 의미 있음");

        var second = new OutputDisplay("disp-2", "모니터 2", 1920, 0, 1920, 1080, 1.0);
        sut.SelectedOutputDisplay = second;

        output.Current.Display!.Id.Should().Be("disp-2", "열린 상태에서 모니터 선택 변경 → 그 모니터로 이동");
        output.Current.Display.Name.Should().Be("모니터 2");
        sut.LiveBar.OutputMonitorName.Should().Be("모니터 2", "상태 바도 새 모니터 이름 반영");
    }

    [Fact]
    public void SelectedOutputDisplayChange_WhileClosed_DoesNotOpenOrMove()
    {
        // 출력 창이 닫혀 있으면 모니터 선택만 바뀌고 창은 열리지 않는다(다음 Open 때 반영).
        var output = new OutputWindowService();
        var sut = CreateSut(output: output);

        var second = new OutputDisplay("disp-2", "모니터 2", 1920, 0, 1920, 1080, 1.0);
        sut.SelectedOutputDisplay = second;

        output.Current.IsOpen.Should().BeFalse("닫힌 상태에선 이동·열림 없음");
    }

    [Fact]
    public void BodyVerticalOffsetCommands_StepAndClamp()
    {
        // FrmMain Ind_Reg1TopUpDown — 본문 위로(-)/아래로(+) 8px 단계, -300~300 클램프.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveBodyVerticalOffset.Should().Be(0, "기본 0");

        sut.MoveBodyDownCommand.Execute(null);
        sut.ActiveBodyVerticalOffset.Should().Be(8, "아래로 +8");
        settings.Get(EasiSettingKeys.LyricsMonitorBodyVerticalOffset).Should().Be(8);

        sut.MoveBodyUpCommand.Execute(null);
        sut.MoveBodyUpCommand.Execute(null);
        sut.ActiveBodyVerticalOffset.Should().Be(-8, "위로 두 단계 → -8");

        settings.Set(EasiSettingKeys.LyricsMonitorBodyVerticalOffset, 300);
        sut.MoveBodyDownCommand.CanExecute(null).Should().BeFalse("상한(+300)에서 아래로 비활성");

        settings.Set(EasiSettingKeys.LyricsMonitorBodyVerticalOffset, -300);
        sut.MoveBodyUpCommand.CanExecute(null).Should().BeFalse("하한(-300)에서 위로 비활성");
    }

    [Fact]
    public void RegionGapCommands_StepAndClamp()
    {
        // FrmMain Ind_Reg2TopUpDown — +/- 4px 단계, 0~100px 클램프.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveRegionGap.Should().Be(8, "기본 8px");

        sut.IncreaseRegionGapCommand.Execute(null);
        sut.ActiveRegionGap.Should().Be(12, "한 단계 +4px");
        settings.Get(EasiSettingKeys.LyricsMonitorRegionGapPx).Should().Be(12);

        settings.Set(EasiSettingKeys.LyricsMonitorRegionGapPx, 100);
        sut.IncreaseRegionGapCommand.CanExecute(null).Should().BeFalse("상한(100px)에서 증가 비활성");

        settings.Set(EasiSettingKeys.LyricsMonitorRegionGapPx, 0);
        sut.DecreaseRegionGapCommand.CanExecute(null).Should().BeFalse("하한(0px)에서 감소 비활성");
    }

    [Fact]
    public void PanelFontScaleCommands_StepAndClamp()
    {
        // FrmMain Def_PanelFont 크기 — +/- 10% 단계, 50~200% 클램프.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActivePanelFontScale.Should().Be(100, "기본 100%");

        sut.IncreasePanelFontScaleCommand.Execute(null);
        sut.ActivePanelFontScale.Should().Be(110, "한 단계 +10%");
        settings.Get(EasiSettingKeys.LyricsMonitorPanelFontScalePercent).Should().Be(110);

        settings.Set(EasiSettingKeys.LyricsMonitorPanelFontScalePercent, 200);
        sut.IncreasePanelFontScaleCommand.CanExecute(null).Should().BeFalse("상한(200%)에서 증가 비활성");

        settings.Set(EasiSettingKeys.LyricsMonitorPanelFontScalePercent, 50);
        sut.DecreasePanelFontScaleCommand.CanExecute(null).Should().BeFalse("하한(50%)에서 감소 비활성");
    }

    [Fact]
    public void LyricsFontSize2Input_DirectEntry_ZeroIsAuto_ElseClamps()
    {
        // FrmMain Ind_Reg2SizeUpDown — 0=본문과 동일(자동) 허용, 1~23→24, >120→120 클램프.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsFontSize2.Should().Be(0, "기본 0=자동");

        sut.LyricsFontSize2Input = 40;
        sut.ActiveLyricsFontSize2.Should().Be(40, "범위 안 그대로");
        settings.Get(EasiSettingKeys.LyricsMonitorFontSize2).Should().Be(40);

        sut.LyricsFontSize2Input = 0;
        sut.ActiveLyricsFontSize2.Should().Be(0, "0=자동 허용");

        sut.LyricsFontSize2Input = 10;
        sut.ActiveLyricsFontSize2.Should().Be(24, "1~23 은 하한 24 로 클램프");

        sut.LyricsFontSize2Input = 999;
        sut.ActiveLyricsFontSize2.Should().Be(120, "상한 120 으로 클램프");
    }

    [Fact]
    public void LyricsLineSpacingInput_DirectEntry_ClampsAndPersists()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsLineSpacingInput = 160;
        sut.ActiveLyricsLineSpacing.Should().Be(160, "범위 안(100~220) 값은 그대로 적용");
        settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent).Should().Be(160);

        sut.LyricsLineSpacingInput = 9999;
        sut.ActiveLyricsLineSpacing.Should().Be(220, "상한 220% 로 클램프");

        sut.LyricsLineSpacingInput = 0;
        sut.ActiveLyricsLineSpacing.Should().Be(100, "하한 100% 로 클램프");
    }

    [Fact]
    public void LyricsFontSizeInput_OutOfRangeAtBound_RaisesInputNotificationForSnapBack()
    {
        // 이미 상한(120)인데 더 큰 값(999)을 입력하면 값은 안 바뀌지만, 입력 박스를 120 으로 되돌리려면
        // LyricsFontSizeInput 변경 통지가 반드시 발생해야 한다(빈 set 분기에서 OnPropertyChanged 호출).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.LyricsFontSizeInput = 120; // 상한으로 맞춰 둠
        var raised = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.LyricsFontSizeInput))
            {
                raised = true;
            }
        };

        sut.LyricsFontSizeInput = 999; // 클램프하면 120 → 값 불변

        sut.ActiveLyricsFontSize.Should().Be(120, "상한에서 값은 변하지 않음");
        raised.Should().BeTrue("입력 박스를 클램프값으로 되돌리도록 변경 통지가 발생");
    }

    [Fact]
    public void LyricsFontSizeInput_StepCommand_KeepsInputInSync()
    {
        // −/+ 버튼과 직접 입력이 같은 값을 가리키는지(통지 동기) — 버튼 누른 뒤 Input get 이 따라온다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.LyricsFontSizeInput = 60;

        sut.IncreaseLyricsFontSizeCommand.Execute(null);

        sut.LyricsFontSizeInput.Should().Be(64, "버튼 +4 후 입력 박스도 64");
    }

    [Fact]
    public void LyricsBodyMarginInputs_DirectEntry_ClampAndPersist_LeftRightBottom()
    {
        // FrmMain ShowLeftMargin/Right/Bottom 직접 수치 입력 — 범위 안 그대로, 범위 밖은 0~400 으로 클램프.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        sut.LyricsLeftMarginInput = 40;
        sut.LyricsRightMarginInput = 56;
        sut.LyricsBottomMarginInput = 72;
        sut.ActiveLyricsLeftMargin.Should().Be(40);
        sut.ActiveLyricsRightMargin.Should().Be(56);
        sut.ActiveLyricsBottomMargin.Should().Be(72);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyLeftMargin).Should().Be(40);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyRightMargin).Should().Be(56);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyBottomMargin).Should().Be(72);

        sut.LyricsLeftMarginInput = 9999;
        sut.ActiveLyricsLeftMargin.Should().Be(400, "상한 400px 으로 클램프");
        sut.LyricsLeftMarginInput.Should().Be(400, "입력 박스도 클램프값으로 보정");

        sut.LyricsBottomMarginInput = -50;
        sut.ActiveLyricsBottomMargin.Should().Be(0, "하한 0px 으로 클램프");
    }

    [Fact]
    public void LyricsLeftMarginInput_StepCommand_KeepsInputInSync()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.LyricsLeftMarginInput = 16;

        sut.IncreaseLyricsLeftMarginCommand.Execute(null);

        sut.LyricsLeftMarginInput.Should().Be(24, "버튼 +8 후 입력 박스도 24");
    }

    [Fact]
    public void LyricsBodyMarginCommands_StepAndPersist_LeftRightBottom()
    {
        // FrmMain ShowLeftMargin/ShowRightMargin/ShowBottomMargin 대응 — +/- 한 단계(8px)가 설정에 저장된다.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);
        sut.ActiveLyricsLeftMargin.Should().Be(0, "기본 0px");
        sut.ActiveLyricsRightMargin.Should().Be(0);
        sut.ActiveLyricsBottomMargin.Should().Be(0);

        sut.IncreaseLyricsLeftMarginCommand.Execute(null);
        sut.IncreaseLyricsRightMarginCommand.Execute(null);
        sut.IncreaseLyricsBottomMarginCommand.Execute(null);

        sut.ActiveLyricsLeftMargin.Should().Be(8, "한 단계 +8px");
        sut.ActiveLyricsRightMargin.Should().Be(8);
        sut.ActiveLyricsBottomMargin.Should().Be(8);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyLeftMargin).Should().Be(8);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyRightMargin).Should().Be(8);
        settings.Get(EasiSettingKeys.LyricsMonitorBodyBottomMargin).Should().Be(8);

        sut.DecreaseLyricsLeftMarginCommand.Execute(null);
        sut.ActiveLyricsLeftMargin.Should().Be(0, "한 단계 -8px");
    }

    [Fact]
    public void LyricsBodyMarginCommands_ClampToRange()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings);

        settings.Set(EasiSettingKeys.LyricsMonitorBodyLeftMargin, 0);
        sut.DecreaseLyricsLeftMarginCommand.CanExecute(null).Should().BeFalse("하한(0px)에서 감소 비활성");

        settings.Set(EasiSettingKeys.LyricsMonitorBodyLeftMargin, 400);
        sut.IncreaseLyricsLeftMarginCommand.CanExecute(null).Should().BeFalse("상한(400px)에서 증가 비활성");
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

    // ── 후속#1: 검색 창을 좌측 "검색" 탭으로 인라인 흡수(폴더 가로지르는 교차 검색 → 예배 순서 추가) ──

    [Fact]
    public void Search_IsExposed_ForInlineSearchTab()
    {
        var sut = CreateSut();
        sut.Search.Should().NotBeNull("좌측 '검색' 탭이 바인딩할 교차 검색 VM");
    }

    [Fact]
    public void AddSearchedSongCommand_CanExecute_ReflectsSelectedSearchResult()
    {
        var sut = CreateSut();
        sut.AddSearchedSongCommand.CanExecute(null).Should().BeFalse("초기엔 선택된 검색 결과 없음");

        sut.SelectedSearchResult = new SongSearchResult(7, 1, "찬양 폴더", "은혜", "", 12, "", "G", new[] { "Title" }, "");

        sut.AddSearchedSongCommand.CanExecute(null).Should().BeTrue("검색 결과 선택 시 추가 활성");
    }

    [Fact]
    public async Task AddSearchedSong_LoadsLyricsBySongId_AndAddsToQueue()
    {
        // 검색 결과(SongSearchResult)에는 가사가 없으므로, 선택 결과의 SongId 로 곡 상세(가사 포함)를 불러와 큐에 채워야 한다.
        var detail = SampleSongDetail(songId: 7, title: "은혜", lyrics: "1절 가사\n2절 가사");
        var repo = new StubSongDetailRepository(detail);
        var sut = CreateSut(songDetail: repo);

        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        var result = new SongSearchResult(7, 1, "찬양 폴더", "은혜", "", 12, "", "G", new[] { "Title" }, "은혜로다");
        sut.Search.SearchResults.Add(result);
        sut.SelectedSearchResult = result;

        await sut.AddSearchedSongCommand.ExecuteAsync(null);

        repo.LastSongId.Should().Be(7, "선택한 검색 결과의 SongId 로 상세를 조회");
        repo.LastDatabasePath.Should().Be(sut.Search.DatabasePath, "검색 VM 의 DB 경로로 조회");
        sut.Queue.Should().Contain(
            item => item.Title == "은혜" && item.Lyrics == "1절 가사\n2절 가사",
            "검색 결과를 가사까지 채워 예배 순서에 추가");
    }

    [Fact]
    public async Task AddSearchedSong_WhenDetailMissing_DoesNotAddAndReportsStatus()
    {
        // 상세 조회가 null(곡 삭제됨 등)이면 큐에 추가하지 않고 상태만 알린다.
        var repo = new StubSongDetailRepository(detail: null);
        var sut = CreateSut(songDetail: repo);
        var initialCount = sut.Queue.Count;

        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        var result = new SongSearchResult(99, 1, "찬양 폴더", "없는곡", "", 0, "", "", new[] { "Title" }, "");
        sut.Search.SearchResults.Add(result);
        sut.SelectedSearchResult = result;

        await sut.AddSearchedSongCommand.ExecuteAsync(null);

        sut.Queue.Count.Should().Be(initialCount, "상세를 찾지 못하면 큐 변화 없음");
        sut.StatusText.Should().Contain("없는곡", "어떤 곡을 못 찾았는지 알린다");
    }

    [Fact]
    public void Research_ReplacingResults_ClearsStaleSelection_AndDisablesAdd()
    {
        // 재검색으로 결과 목록이 통째로 교체(ReplaceWith=Clear+재추가)되면,
        // 사라진 옛 결과를 가리키던 선택은 VM 이 스스로 비워야 한다(화면 바인딩 비의존).
        var sut = CreateSut();
        var first = new SongSearchResult(1, 1, "찬양 폴더", "첫 검색곡", "", 1, "", "", new[] { "Title" }, "");
        sut.Search.SearchResults.Add(first);
        sut.SelectedSearchResult = first;
        sut.AddSearchedSongCommand.CanExecute(null).Should().BeTrue("선택이 있으면 추가 활성");

        // 새 검색 결과로 목록을 통째로 교체(SearchUsageViewModel.SearchSongsAsync 의 ReplaceWith = Clear+재추가 와 동일 시퀀스).
        sut.Search.SearchResults.Clear();
        sut.Search.SearchResults.Add(new SongSearchResult(2, 1, "찬양 폴더", "다른 곡", "", 2, "", "", new[] { "Title" }, ""));

        sut.SelectedSearchResult.Should().BeNull("교체로 사라진 선택은 비워져야 한다");
        sut.AddSearchedSongCommand.CanExecute(null).Should().BeFalse("선택이 없으면 추가 비활성");
    }

    // ── 검색 탭 "제목" 모드 인라인: 제목 조회 결과(LookupTitleCandidate)를 예배 순서에 추가 ──

    [Fact]
    public void AddLookupTitleCommand_CanExecute_ReflectsSelectedTitleCandidate()
    {
        var sut = CreateSut();
        sut.AddLookupTitleCommand.CanExecute(null).Should().BeFalse("초기엔 선택된 제목 후보 없음");

        sut.SelectedTitleCandidate = new LookupTitleCandidate(7, "은혜", "", 1, "찬양 폴더", "", "");

        sut.AddLookupTitleCommand.CanExecute(null).Should().BeTrue("제목 후보 선택 시 추가 활성");
    }

    [Fact]
    public async Task AddLookupTitle_LoadsLyricsBySongId_AndAddsToQueue()
    {
        // 제목 조회 결과도 가사가 없으므로 SongId 로 곡 상세(가사)를 불러와 큐에 채운다(곡 검색과 동일 경로 재사용).
        var detail = SampleSongDetail(songId: 7, title: "은혜", lyrics: "1절 가사\n2절 가사");
        var sut = CreateSut(songDetail: new StubSongDetailRepository(detail));

        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        var candidate = new LookupTitleCandidate(7, "은혜", "", 1, "찬양 폴더", "", "");
        sut.Search.LookupCandidates.Add(candidate);
        sut.SelectedTitleCandidate = candidate;

        await sut.AddLookupTitleCommand.ExecuteAsync(null);

        sut.Queue.Should().Contain(
            item => item.Title == "은혜" && item.Lyrics == "1절 가사\n2절 가사",
            "제목 후보를 가사까지 채워 예배 순서에 추가");
    }

    [Fact]
    public void LookupCandidates_Replaced_ClearsStaleTitleSelection()
    {
        // 제목을 다시 조회하면 후보 목록이 통째로 바뀐다 — 사라진 선택은 VM 이 스스로 비운다(곡 검색과 동일 규칙).
        var sut = CreateSut();
        var first = new LookupTitleCandidate(1, "첫 제목", "", 1, "폴더", "", "");
        sut.Search.LookupCandidates.Add(first);
        sut.SelectedTitleCandidate = first;
        sut.AddLookupTitleCommand.CanExecute(null).Should().BeTrue();

        sut.Search.LookupCandidates.Clear();
        sut.Search.LookupCandidates.Add(new LookupTitleCandidate(2, "다른 제목", "", 1, "폴더", "", ""));

        sut.SelectedTitleCandidate.Should().BeNull("교체로 사라진 선택은 비워져야 한다");
        sut.AddLookupTitleCommand.CanExecute(null).Should().BeFalse();
    }

    // ── 후속#3: 예배 순서 이름 변경(레거시 FrmManageItemLists + FrmUpdateFileName(rename) 대응) ──

    [Fact]
    public async Task RenameWorshipList_RenamesSavedList()
    {
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("옛이름", new[] { new LiveQueueItem("a", "A", "Song") });
        var sut = CreateSut(worshipLists: store);

        var ok = sut.RenameWorshipList("옛이름", "새이름");

        ok.Should().BeTrue("유효한 새 이름이면 변경 성공");
        store.ListNames().Should().Contain("새이름").And.NotContain("옛이름");
    }

    [Fact]
    public async Task RenameWorshipList_ToExistingName_IsRejected()
    {
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("A", Array.Empty<LiveQueueItem>());
        await store.SaveAsync("B", Array.Empty<LiveQueueItem>());
        var sut = CreateSut(worshipLists: store);

        var ok = sut.RenameWorshipList("A", "B");

        ok.Should().BeFalse("이미 있는 이름으로는 변경 거부(덮어쓰기 방지)");
        store.ListNames().Should().Contain(new[] { "A", "B" }, "둘 다 그대로 남는다");
    }

    [Fact]
    public async Task RenameWorshipList_UnchangedName_IsNoOpSuccess()
    {
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("주일", Array.Empty<LiveQueueItem>());
        var sut = CreateSut(worshipLists: store);

        var ok = sut.RenameWorshipList("주일", "주일");

        ok.Should().BeTrue("이름이 같으면 변경 없이 성공으로 처리");
        store.ListNames().Should().ContainSingle().Which.Should().Be("주일");
    }

    [Fact]
    public void RenameWorshipList_WhenStoreRejectsName_ReturnsFalse_WithoutCrash()
    {
        // 다이얼로그가 못 거른 무효 이름(파일명 불가 문자 등)이나 검사~이동 사이 경쟁으로 스토어가 예외를 던져도,
        // VM 이 잡아 친절한 상태 메시지로 바꾸고 false 를 돌려준다(개발자용 예외 창 방지).
        var sut = CreateSut(worshipLists: new ThrowingRenameWorshipListStore());

        var ok = sut.RenameWorshipList("원본", "bad/name");

        ok.Should().BeFalse("스토어가 이름을 거부하면 실패로 처리");
        sut.StatusText.Should().Contain("바꾸지 못", "운영자에게 친절한 안내(원시 예외 노출 방지)");
    }

    // Rename 이 항상 예외를 던지는 스토어 — VM 의 예외 흡수 경로 검증용.
    private sealed class ThrowingRenameWorshipListStore : IWorshipListStore
    {
        public Task SaveAsync(string name, IReadOnlyList<LiveQueueItem> items, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<LiveQueueItem>> LoadAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<LiveQueueItem>)Array.Empty<LiveQueueItem>());

        public IReadOnlyList<string> ListNames() => Array.Empty<string>();

        public void Delete(string name)
        {
        }

        public void Rename(string oldName, string newName)
            => throw new ArgumentException("이름에 사용할 수 없는 문자", nameof(newName));
    }

    // ── 절 순서(Sequence) 모델: 곡 절을 1회 정의하고 시퀀스로 반복 송출 ──

    [Fact]
    public void AddSong_WithSequence_CarriesSequence_AndPageCountExpands()
    {
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two");

        var item = sut.AddSong(song, "1 C 2 C");

        item!.Sequence.Should().Be("1 C 2 C");
        sut.SelectedItem.Should().BeSameAs(item);
        sut.LyricsPageCount.Should().Be(4, "절을 1회 정의하고 시퀀스(1 C 2 C)로 4페이지로 펼친다");
    }

    [Fact]
    public void AddSong_WithoutSequence_PaginatesLinearly()
    {
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two");

        var item = sut.AddSong(song);

        item!.Sequence.Should().BeNull();
        sut.LyricsPageCount.Should().Be(3, "시퀀스 없으면 선형 3페이지(1·C·2)");
    }

    // ── 절 순서 인라인 편집(SelectedItemSequenceInput) — 운영자가 선택한 곡의 절 순서를 직접 바꾼다 ──

    [Fact]
    public void EditSelectedItemSequence_SetsSequence_AndExpandsPages_AndReflectsInInput()
    {
        // 곡을 선택하고 절 순서를 입력하면 그 항목 Sequence 가 바뀌고 페이지가 펼쳐지며 입력칸도 동기화된다.
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two");
        sut.AddSong(song); // 선형 3페이지로 추가, 자동 선택.
        sut.LyricsPageCount.Should().Be(3);

        sut.SelectedItemSequenceInput = "  1 C 2 C  "; // 앞뒤 공백 포함 입력.

        sut.SelectedItem!.Sequence.Should().Be("1 C 2 C", "앞뒤 공백을 다듬어 저장");
        sut.SelectedItemSequenceInput.Should().Be("1 C 2 C", "입력칸도 교체된 항목을 따라감");
        sut.LyricsPageCount.Should().Be(4, "절 순서(1 C 2 C)로 4페이지로 펼쳐짐");
        sut.AvailableSectionLabels.Should().Equal(new[] { "1", "C", "2" }, "편집 후 라벨 점프 바도 새 절 순서에서 재계산됨");
    }

    [Fact]
    public void EditSelectedItemSequence_WhileLive_PreservesCurrentVerse()
    {
        // 라이브 송출 중 절 순서를 편집해도 현재 절이 유지돼야 한다(0절로 안 튐) — 세션의 실제 라이브 절로 재송출.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n1절 가사\n[2]\n2절 가사",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);
        sut.NextLyricsPageCommand.Execute(null); // 2절로 이동(라이브).
        session.Current.CurrentLyricsPageIndex.Should().Be(1);

        sut.SelectedItemSequenceInput = "1 2 1"; // 절 순서 추가(3페이지) — 현재 절(인덱스 1)은 여전히 유효.

        session.Current.CurrentLyricsPageIndex.Should().Be(1, "현재 절 유지 — 0절로 튀지 않음");
        sut.LyricsPageCount.Should().Be(3, "새 절 순서(1 2 1)로 3페이지");
    }

    [Fact]
    public void EditSelectedItemSequence_Empty_ClearsToLinear()
    {
        // 절 순서를 비우면 null 로 되돌아가 선형 페이지네이션으로 복귀한다.
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two");
        sut.AddSong(song, "1 C 2 C");
        sut.LyricsPageCount.Should().Be(4);

        sut.SelectedItemSequenceInput = "   ";

        sut.SelectedItem!.Sequence.Should().BeNull("공백만이면 순서 해제(null)");
        sut.LyricsPageCount.Should().Be(3, "선형 3페이지로 복귀");
    }

    [Fact]
    public void CanEditSelectedItemSequence_TrueOnlyForSongWithLyrics()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.CanEditSelectedItemSequence.Should().BeFalse("선택 없으면 편집 불가");

        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.CanEditSelectedItemSequence.Should().BeTrue("가사 있는 곡이면 편집 가능");

        // 곡이 아닌 항목(공지) 선택 → 편집 불가.
        sut.LoadQueue([new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" }]);
        sut.SelectedItem = sut.Queue[0];
        sut.CanEditSelectedItemSequence.Should().BeFalse("곡이 아니면 절 순서 편집 불가");
    }

    [Fact]
    public void EditSelectedItemSequence_NonSongSelected_IsNoOp()
    {
        // 곡이 아닌 항목이 선택된 상태에서 절 순서 입력은 무시된다(잘못된 항목에 순서가 안 붙음).
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SelectedItemSequenceInput = "1 C 2";

        sut.SelectedItem!.Sequence.Should().BeNull("공지 항목엔 절 순서가 붙지 않음");
    }

    // ── 항목별 글자색 인라인 편집(SetSelectedItemTextColor) — 선택한 곡만 색을 바꾼다(레거시 Ind_ 색) ──

    [Fact]
    public void SetSelectedItemTextColor_WritesFormatDataCode29_AndEnablesIndividual_AndReflectsHex()
    {
        // 곡을 선택하고 글자색을 주면 그 항목 FormatData(코드 29)에 색이 실리고, 개별 서식이 켜지며, 색 미리보기도 동기화된다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.SelectedItem!.FormatData.Should().BeNull("처음엔 항목별 색 없음");

        sut.SetSelectedItemTextColor("#FFFFFFFF"); // 흰색(ARGB -1).

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "코드 29(영역1 글자색)에 흰색(-1) 인코드");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("색을 주면 그 색이 보이도록 개별 서식을 켠다");
        sut.SelectedItemTextColorHex.Should().Be("#FFFFFFFF", "색 미리보기도 교체된 항목을 따라감");
    }

    [Fact]
    public void SetSelectedItemTextColor_PreservesOtherFormatData()
    {
        // 색만 바꾸고 나머지 곡별 서식(정렬 등)은 보존한다(Parse→with→Encode 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "31=2", // 정렬 가운데(코드 31).
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor("#FFFF0000"); // 빨강.

        sut.SelectedItem!.FormatData.Should().Contain("31=2", "기존 정렬 서식은 보존");
        sut.SelectedItem!.FormatData.Should().Contain("29=", "새 글자색도 추가");
    }

    [Fact]
    public void SetSelectedItemTextColor_EmptyOrInvalid_ClearsColorToGlobal()
    {
        // 빈/형식오류 색을 주면 색을 해제(코드 29 제거) → 전역 기본색을 따른다.
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1", // 흰색이 이미 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor(""); // 전역 기본으로 해제.

        sut.SelectedItemTextColorHex.Should().BeEmpty("색 해제 → 전역 기본 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("29=", "코드 29(글자색)가 제거됨");
    }

    [Fact]
    public void SetSelectedItemTextColor_WhileLive_AppliesOverrideColor()
    {
        // 라이브 송출 중 항목 글자색을 바꾸면 세션의 오버라이드 색이 즉시 반영된다(레거시 per-song 색).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemTextColor("#FFFFFFFF"); // 흰색(-1).

        session.Current.OverrideTextColorArgb.Should().Be(-1, "항목 글자색이 라이브 송출 색을 즉시 이긴다");
    }

    [Fact]
    public void SetSelectedItemTextColor_WhileLiveOnLaterVerse_PreservesVerse()
    {
        // 라이브 2절에서 색을 바꿔도 현재 절이 유지돼야 한다(0절로 안 튐) — 절 순서 편집과 같은 재송출 보장.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n1절\n[2]\n2절" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);
        sut.NextLyricsPageCommand.Execute(null); // 2절로 이동.
        session.Current.CurrentLyricsPageIndex.Should().Be(1);

        sut.SetSelectedItemTextColor("#FFFFFFFF");

        session.Current.CurrentLyricsPageIndex.Should().Be(1, "현재 절(2절) 유지 — 0절로 안 튐");
        session.Current.OverrideTextColorArgb.Should().Be(-1, "그래도 새 색은 반영");
    }

    [Fact]
    public void SetSelectedItemTextColor_WhenIndividualWasOff_ReEnablesAndDisclosesInStatus()
    {
        // 개별 서식을 꺼둔 항목에 색을 주면 자동으로 켜지고, 상태줄이 그 사실을 정직하게 알린다.
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            UseIndividualFormatting = false, // 운영자가 일부러 꺼둠.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor("#FFFFFF00");

        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("색이 보이도록 자동으로 켬");
        sut.StatusText.Should().Contain("개별 서식 켜짐", "자동으로 켠 사실을 상태줄에 알림");
    }

    [Fact]
    public void SetSelectedItemTextColor_NonSong_IsNoOpAndCommandDisabled()
    {
        // 곡이 아닌 항목엔 항목별 색이 붙지 않고, 명령도 비활성.
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.CanEditSelectedItemColor.Should().BeFalse();
        sut.SetSelectedItemTextColorCommand.CanExecute("#FFFFFFFF").Should().BeFalse("곡이 아니면 명령 비활성");

        sut.SetSelectedItemTextColor("#FFFFFFFF");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 색이 붙지 않음");
    }

    [Fact]
    public void CanEditSelectedItemColor_True_ForBibleItemWithBody()
    {
        // 성경 항목도 본문(구절)을 송출하므로 항목별 서식 편집 대상 — 곡과 동일(레거시 Ind_* 종류 무관).
        var sut = CreateSut(seedSampleQueue: false);
        var bible = new LiveQueueItem("bible:gen1", "창세기 1:1", LiveItemKinds.Bible) { Lyrics = "태초에 하나님이..." };
        sut.LoadQueue([bible]);
        sut.SelectedItem = sut.Queue[0];

        sut.CanEditSelectedItemColor.Should().BeTrue("성경 항목도 본문이 있으면 서식 편집 가능");
        sut.SetSelectedItemTextColorCommand.CanExecute("#FFFFFFFF").Should().BeTrue("성경 항목도 명령 활성");
    }

    [Fact]
    public void CanEditSelectedItemColor_False_ForBibleItemWithoutBody()
    {
        // 본문이 비면(파일 없음 등 폴백으로 제목만) 서식 편집 대상이 아니다(곡과 동일 규칙 — 가사/구절 없으면 불가).
        var sut = CreateSut(seedSampleQueue: false);
        var bible = new LiveQueueItem("bible:empty", "창세기 1:1", LiveItemKinds.Bible) { Lyrics = "   " };
        sut.LoadQueue([bible]);
        sut.SelectedItem = sut.Queue[0];

        sut.CanEditSelectedItemColor.Should().BeFalse("본문 없는 성경 항목은 서식 편집 불가");
    }

    [Fact]
    public void SetSelectedItemTextColor_OnBibleItem_WritesFormatData()
    {
        // 성경 항목에 항목별 글자색을 주면 곡과 똑같이 FormatData(코드29)에 실리고 개별 서식이 켜진다.
        var sut = CreateSut(seedSampleQueue: false);
        var bible = new LiveQueueItem("bible:gen1", "창세기 1:1", LiveItemKinds.Bible) { Lyrics = "태초에 하나님이..." };
        sut.LoadQueue([bible]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor("#FFFFE066"); // 노랑.

        sut.SelectedItem!.FormatData.Should().Contain("29=", "성경 항목도 코드29 글자색 인코드");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("색을 주면 개별 서식 켜짐");
    }

    [Fact]
    public async Task SetSelectedItemColor_OnBibleItem_RendersOverride_EndToEnd()
    {
        // 종단: 성경 항목에 준 항목별 글자색이 송출 시 라이브 오버라이드 글자색으로 적용된다(곡과 동일 경로 — GoLive Kind 무관).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var bible = new LiveQueueItem("bible:gen1", "창세기 1:1", LiveItemKinds.Bible) { Lyrics = "태초에 하나님이..." };
        sut.LoadQueue([bible]);
        sut.SelectedItem = sut.Queue[0];
        sut.OpenOutputCommand.Execute(null);

        sut.SetSelectedItemTextColor("#FFFFE066"); // 노랑(0xFFFFE066).
        await sut.GoLiveCommand.ExecuteAsync(null);

        session.Current.OverrideTextColorArgb.Should().Be(unchecked((int)0xFFFFE066), "성경 항목별 글자색이 라이브에 적용");
    }

    [Fact]
    public void ClearSelectedItemFormatting_RemovesAllFormatData_RevertsToGlobal()
    {
        // "이 항목 서식 모두 지우기" — 여러 항목별 서식(색·정렬)을 준 뒤 한 번에 지우면 FormatData 가 null(전역 기본)로 돌아간다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.SetSelectedItemTextColor("#FFFFE066"); // 노랑
        sut.SetSelectedItemAlignment("2");          // 가운데
        sut.SelectedItem!.FormatData.Should().NotBeNullOrEmpty("지우기 전엔 서식 있음");

        sut.ClearSelectedItemFormatting();

        sut.SelectedItem!.FormatData.Should().BeNull("모든 항목별 서식이 제거돼 전역 기본");
        sut.StatusText.Should().Contain("모두 지움");
    }

    [Fact]
    public void CanClearSelectedItemFormatting_OnlyWhenFormatDataPresent()
    {
        // 지울 서식이 있을 때만 활성 — 서식 없으면(또는 곡이 아니면) 비활성.
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.CanClearSelectedItemFormatting.Should().BeFalse("서식이 없으면 지울 게 없어 비활성");
        sut.ClearSelectedItemFormattingCommand.CanExecute(null).Should().BeFalse();

        sut.SetSelectedItemTextColor("#FFFFFFFF");
        sut.CanClearSelectedItemFormatting.Should().BeTrue("서식이 생기면 활성");
        sut.ClearSelectedItemFormattingCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ClearSelectedItemFormatting_WorksForBibleItem()
    {
        // 성경 항목도(증분128) 서식 적용 대상이므로 "모두 지우기"가 동작한다.
        var sut = CreateSut(seedSampleQueue: false);
        var bible = new LiveQueueItem("bible:gen1", "창세기 1:1", LiveItemKinds.Bible)
        {
            Lyrics = "태초에 하나님이...",
            FormatData = "29=-1>31=2>", // 흰색 + 가운데
        };
        sut.LoadQueue([bible]);
        sut.SelectedItem = sut.Queue[0];

        sut.CanClearSelectedItemFormatting.Should().BeTrue("성경 항목도 서식 있으면 지우기 활성");
        sut.ClearSelectedItemFormatting();

        sut.SelectedItem!.FormatData.Should().BeNull("성경 항목 서식도 모두 제거");
    }

    [Fact]
    public async Task ClearSelectedItemFormatting_WhileLive_RevertsRender_EndToEnd()
    {
        // 종단: 라이브 중 "모두 지우기"를 누르면 같은 절로 재송출돼 라이브 오버라이드가 사라진다(전역 기본 복귀).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.OpenOutputCommand.Execute(null);
        sut.SetSelectedItemTextColor("#FFFFE066"); // 노랑
        await sut.GoLiveCommand.ExecuteAsync(null);
        session.Current.OverrideTextColorArgb.Should().Be(unchecked((int)0xFFFFE066), "송출 시 항목별 색 적용 확인");

        sut.ClearSelectedItemFormatting();

        session.Current.OverrideTextColorArgb.Should().BeNull("서식을 지우면 라이브가 전역 기본색으로 복귀");
    }

    [Fact]
    public void CopyPasteSelectedItemFormatting_AppliesSameFormatToAnotherItem()
    {
        // 한 곡의 서식(색·정렬)을 복사해 다른 곡에 붙여넣으면 같은 FormatData 가 적용된다(본문은 그대로).
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가사A", FormatData = "29=-1>31=2>" };
        var b = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n가사B" };
        sut.LoadQueue([a, b]);

        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();

        sut.SelectedItem = sut.Queue[1];
        sut.CanPasteSelectedItemFormatting.Should().BeTrue("복사한 서식이 있으면 붙여넣기 활성");
        sut.PasteSelectedItemFormatting();

        sut.Queue[1].FormatData.Should().Contain("29=-1", "색 복사됨").And.Contain("31=2", "정렬 복사됨");
        sut.Queue[1].Lyrics.Should().Be("[1]\n가사B", "본문은 그대로(서식만 붙여넣음)");
        sut.Queue[1].UseIndividualFormatting.Should().BeTrue("붙여넣으면 개별 서식 켜짐");
    }

    [Fact]
    public void PasteSelectedItemFormatting_FullyReplaces_NotMerges_TargetFormatting()
    {
        // 붙여넣기는 대상의 기존 서식을 합치지 않고 통째로 덮어쓴다(레거시 "붙여넣기" 의미). 대상의 옛 색은 사라져야 한다.
        var sut = CreateSut(seedSampleQueue: false);
        var src = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "47=80>" }; // 크기만(색 없음)
        var dst = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n나", FormatData = "29=-1>31=3>" }; // 흰색+오른쪽
        sut.LoadQueue([src, dst]);

        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();
        sut.SelectedItem = sut.Queue[1];
        sut.PasteSelectedItemFormatting();

        sut.Queue[1].FormatData.Should().Contain("47=80", "복사원 크기로 교체");
        (sut.Queue[1].FormatData ?? "").Should().NotContain("29=", "대상의 옛 색은 합쳐지지 않고 사라짐")
            .And.NotContain("31=", "대상의 옛 정렬도 사라짐");
    }

    [Fact]
    public void CanCopyAndPaste_GatesReflectStateCorrectly()
    {
        // 복사: 서식 있을 때만. 붙여넣기: 복사한 게 있고 대상이 편집 가능할 때만.
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가사", FormatData = "29=-1>" };
        var plain = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([a, plain]);

        sut.SelectedItem = sut.Queue[1]; // 서식 없는 곡
        sut.CanCopySelectedItemFormatting.Should().BeFalse("서식 없으면 복사 불가");
        sut.CanPasteSelectedItemFormatting.Should().BeFalse("아직 복사한 게 없으면 붙여넣기 불가");

        sut.SelectedItem = sut.Queue[0]; // 서식 있는 곡
        sut.CanCopySelectedItemFormatting.Should().BeTrue("서식 있으면 복사 가능");
        sut.CopySelectedItemFormatting();
        sut.CanPasteSelectedItemFormatting.Should().BeTrue("복사 후엔 붙여넣기 가능");
    }

    [Fact]
    public void ApplyCopiedFormatToAll_AppliesToAllSongAndBible_SkipsOthers()
    {
        // "복사한 서식을 전 항목에 적용" — 곡·성경 항목엔 모두 적용, PPT/미디어/공지는 건너뛴다.
        var sut = CreateSut(seedSampleQueue: false);
        var src = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "47=80>" };
        var song2 = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n나" };
        var bible = new LiveQueueItem("bible:1", "창 1:1", LiveItemKinds.Bible) { Lyrics = "태초에..." };
        var ppt = new LiveQueueItem("ppt:1", "발표.pptx", LiveItemKinds.PowerPoint) { ContentPath = @"C:\발표.pptx" };
        sut.LoadQueue([src, song2, bible, ppt]);

        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();
        sut.ApplyCopiedFormatToAll();

        // 복사원의 원래 FormatData("47=80>")는 정규화(Parse→Encode)돼 표준형 "47=80"으로 다시 쓰인다(꼬리 '>' 정리).
        sut.Queue[0].FormatData.Should().Be("47=80", "복사원도 정규화된 표준 서식으로 통일");
        sut.Queue[1].FormatData.Should().Be("47=80", "곡B 에 적용");
        sut.Queue[2].FormatData.Should().Be("47=80", "성경에도 적용");
        sut.Queue[2].UseIndividualFormatting.Should().BeTrue("적용 시 개별 서식 켜짐");
        sut.Queue[3].FormatData.Should().BeNull("PPT 항목은 건너뜀");
        sut.StatusText.Should().Contain("항목에 복사한 서식 적용");
    }

    [Fact]
    public void CanApplyCopiedFormatToAll_RequiresClipboardAndTargets()
    {
        // 담아 둔 서식이 없으면 비활성. 복사 후, 대상(곡·성경)이 있으면 활성.
        var sut = CreateSut(seedSampleQueue: false);
        var src = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "29=-1>" };
        sut.LoadQueue([src]);

        sut.CanApplyCopiedFormatToAll.Should().BeFalse("복사 전엔 비활성");

        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();
        sut.CanApplyCopiedFormatToAll.Should().BeTrue("복사 후 대상 있으면 활성");
    }

    [Fact]
    public void ApplyCopiedFormatToAll_NoEligibleTargets_IsNoOp()
    {
        // 대상(곡·성경)이 하나도 없으면(PPT 만 있는 큐) 적용할 게 없어 비활성·무동작.
        var sut = CreateSut(seedSampleQueue: false);
        var song = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "29=-1>" };
        sut.LoadQueue([song]);
        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();
        // 복사 후 큐를 PPT 만으로 교체.
        sut.LoadQueue([new LiveQueueItem("ppt:1", "p.pptx", LiveItemKinds.PowerPoint) { ContentPath = @"C:\p.pptx" }]);

        sut.CanApplyCopiedFormatToAll.Should().BeFalse("적용 대상 곡·성경이 없으면 비활성");
    }

    [Fact]
    public void ClearAllItemsFormatting_ClearsAllSongAndBible_SkipsOthersAndUnformatted()
    {
        // "전 항목 서식 지우기" — 서식 있는 곡·성경은 모두 전역 기본으로 비우고, PPT/미디어/공지·이미 서식 없는 항목은 건드리지 않는다.
        var sut = CreateSut(seedSampleQueue: false);
        var song = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "29=-1>" };       // 흰색
        var bible = new LiveQueueItem("bible:1", "창 1:1", LiveItemKinds.Bible) { Lyrics = "태초에...", FormatData = "31=2>" }; // 가운데
        var plainSong = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n나" };                          // 서식 없음
        var ppt = new LiveQueueItem("ppt:1", "발표.pptx", LiveItemKinds.PowerPoint) { ContentPath = @"C:\발표.pptx", FormatData = "29=-1>" };
        sut.LoadQueue([song, bible, plainSong, ppt]);

        sut.ClearAllItemsFormatting();

        sut.Queue[0].FormatData.Should().BeNull("서식 있던 곡 → 전역 기본으로 비워짐");
        sut.Queue[1].FormatData.Should().BeNull("서식 있던 성경 → 전역 기본으로 비워짐");
        sut.Queue[2].FormatData.Should().BeNull("원래 서식 없던 곡은 그대로(여전히 비어 있음)");
        sut.Queue[3].FormatData.Should().Be("29=-1>", "PPT 는 항목별 서식 대상이 아니라 건드리지 않음");
        sut.StatusText.Should().Contain("항목 서식 지움");
        // 지운 직후엔 지울 서식이 더 없으므로 메뉴가 비활성으로 바뀌어야 한다(통지 누락 회귀 방지).
        sut.CanClearAllItemsFormatting.Should().BeFalse("모두 지운 뒤엔 지울 서식이 없어 비활성");
        sut.ClearAllItemsFormattingCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanClearAllItemsFormatting_OnlyWhenSomeFormattableItemHasFormatData()
    {
        // 큐에 지울 서식(곡·성경 FormatData)이 하나라도 있을 때만 활성. PPT 서식이나 빈 큐는 비활성.
        var sut = CreateSut(seedSampleQueue: false);
        sut.CanClearAllItemsFormatting.Should().BeFalse("빈 큐 → 비활성");

        var plainSong = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n나" };
        sut.LoadQueue([plainSong]);
        sut.CanClearAllItemsFormatting.Should().BeFalse("서식 없는 곡만 → 지울 게 없어 비활성");
        sut.ClearAllItemsFormattingCommand.CanExecute(null).Should().BeFalse();

        var formatted = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "29=-1>" };
        sut.LoadQueue([plainSong, formatted]);
        sut.CanClearAllItemsFormatting.Should().BeTrue("서식 있는 곡이 생기면 활성");
        sut.ClearAllItemsFormattingCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ClearAllItemsFormatting_PreservesSelection()
    {
        // 항목 인스턴스가 교체돼도 같은 자리의 항목이 선택으로 유지된다(운영 중 선택 튐 방지).
        var sut = CreateSut(seedSampleQueue: false);
        var a = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가", FormatData = "29=-1>" };
        var b = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n나", FormatData = "31=2>" };
        sut.LoadQueue([a, b]);
        sut.SelectedItem = sut.Queue[1];

        sut.ClearAllItemsFormatting();

        sut.SelectedItem.Should().BeSameAs(sut.Queue[1], "1번 자리의 새 인스턴스가 다시 선택됨");
        sut.SelectedItem!.Id.Should().Be("song:b");
    }

    [Fact]
    public async Task ClearAllItemsFormatting_WhileLive_RevertsRender_EndToEnd()
    {
        // 종단: 라이브 중 "전 항목 서식 지우기"를 누르면 라이브 곡이 같은 절로 재송출돼 항목별 오버라이드가 사라진다(전역 기본 복귀).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.OpenOutputCommand.Execute(null);
        sut.SetSelectedItemTextColor("#FFFFE066"); // 노랑
        await sut.GoLiveCommand.ExecuteAsync(null);
        session.Current.OverrideTextColorArgb.Should().Be(unchecked((int)0xFFFFE066), "송출 시 항목별 색 적용 확인");

        sut.ClearAllItemsFormatting();

        session.Current.OverrideTextColorArgb.Should().BeNull("전 항목 서식을 지우면 라이브가 전역 기본색으로 복귀");
    }

    [Fact]
    public void PasteSelectedItemFormatting_ToBibleItem_Works()
    {
        // 곡 서식을 복사해 성경 항목(증분128)에 붙여넣어도 적용된다(서식은 종류 무관).
        var sut = CreateSut(seedSampleQueue: false);
        var song = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가사", FormatData = "47=60>" };
        var bible = new LiveQueueItem("bible:gen1", "창세기 1:1", LiveItemKinds.Bible) { Lyrics = "태초에..." };
        sut.LoadQueue([song, bible]);

        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();
        sut.SelectedItem = sut.Queue[1];
        sut.PasteSelectedItemFormatting();

        sut.Queue[1].FormatData.Should().Contain("47=60", "성경 항목에도 글자 크기 서식 붙여넣음");
    }

    [Fact]
    public async Task PasteSelectedItemFormatting_WhileLive_RendersOverride_EndToEnd()
    {
        // 종단: 라이브 항목에 서식을 붙여넣으면 즉시 그 서식이 라이브에 적용된다(같은 절 재송출).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var a = new LiveQueueItem("song:a", "곡A", LiveItemKinds.Song) { Lyrics = "[1]\n가사", FormatData = "29=-256>" }; // 노랑(0xFFFFFF00=-256)
        var b = new LiveQueueItem("song:b", "곡B", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([a, b]);
        sut.SelectedItem = sut.Queue[0];
        sut.CopySelectedItemFormatting();

        sut.SelectedItem = sut.Queue[1];
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        session.Current.OverrideTextColorArgb.Should().BeNull("붙여넣기 전엔 서식 없음");

        sut.PasteSelectedItemFormatting();

        session.Current.OverrideTextColorArgb.Should().Be(-256, "붙여넣은 색이 라이브에 즉시 적용");
    }

    // ── 항목별 정렬 인라인 편집(SetSelectedItemAlignment) — 선택한 곡만 가로 정렬(레거시 Ind_ 정렬, 코드31) ──

    [Theory]
    [InlineData("1", 1, "31=1")] // 왼쪽
    [InlineData("2", 2, "31=2")] // 가운데
    [InlineData("3", 3, "31=3")] // 오른쪽
    public void SetSelectedItemAlignment_WritesFormatDataCode31_AndReflects(string param, int expected, string token)
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemAlignment(param);

        sut.SelectedItem!.FormatData.Should().Contain(token, "코드 31(영역1 정렬)에 인코드");
        sut.SelectedItemAlignment.Should().Be(expected, "정렬 미리보기도 동기화");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("정렬을 주면 보이도록 개별 서식을 켠다");
    }

    [Fact]
    public void SetSelectedItemAlignment_PreservesExistingColor()
    {
        // 정렬만 바꾸고 기존 항목별 색(코드29)은 보존한다(공통 헬퍼의 Parse→with→Encode 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1", // 흰색이 이미 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemAlignment("2"); // 가운데.

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("31=2", "새 정렬 추가");
    }

    [Fact]
    public void SetSelectedItemAlignment_ZeroOrInvalid_ClearsToGlobal()
    {
        // 0/형식오류 정렬은 해제(코드31 제거) → 전역 정렬을 따른다.
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "31=3", // 오른쪽이 이미 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemAlignment("0");

        sut.SelectedItemAlignment.Should().Be(0, "정렬 해제 → 전역 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("31=", "코드 31(정렬)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemAlignment_WhileLive_AppliesOverrideAlignment()
    {
        // 라이브 송출 중 항목 정렬을 바꾸면 세션의 오버라이드 정렬이 즉시 반영된다(레거시 per-song 정렬).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemAlignment("3"); // 오른쪽.

        session.Current.OverrideTextAlignment.Should().Be(LyricsTextAlignment.Right, "항목 정렬이 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void SetSelectedItemAlignment_SameValue_IsNoOp_DoesNotReplaceItem()
    {
        // 이미 같은 정렬이면(레코드 동등) 큐 항목을 교체하지 않는다 — 불필요한 재송출·인스턴스 churn 방지(공통 헬퍼 가드).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "31=2", // 이미 가운데.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];
        var before = sut.SelectedItem;

        sut.SetSelectedItemAlignment("2"); // 같은 정렬.

        sut.SelectedItem.Should().BeSameAs(before, "같은 값이면 항목을 새로 만들지 않음(교체 없음)");
        sut.Queue[0].Should().BeSameAs(before, "큐 안의 인스턴스도 그대로");
    }

    [Fact]
    public void SetSelectedItemAlignment_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemAlignmentCommand.CanExecute("2").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemAlignment("2");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 정렬이 붙지 않음");
    }

    // ── 항목별 글자 크기 인라인 편집(SetSelectedItemFontSize) — 선택한 곡만 크기(레거시 Ind_ 크기, 코드47 pt) ──

    [Theory]
    [InlineData("6", 6)]     // 하한 경계(포함)
    [InlineData("36", 36)]
    [InlineData("72", 72)]
    [InlineData("90", 90)]
    [InlineData("100", 100)] // 상한 경계(포함)
    public void SetSelectedItemFontSize_WritesFormatDataCode47_AndReflects(string param, int expected)
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemFontSize(param);

        sut.SelectedItem!.FormatData.Should().Contain($"47={expected}", "코드 47(영역1 글자 크기)에 인코드");
        sut.SelectedItemFontSize.Should().Be(expected, "크기 미리보기도 동기화");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("크기를 주면 보이도록 개별 서식을 켠다");
    }

    [Theory]
    [InlineData("0")]    // 해제 센티넬
    [InlineData("5")]    // 범위 밖(6 미만)
    [InlineData("101")]  // 범위 밖(100 바로 초과 — 상한 경계)
    [InlineData("200")]  // 범위 밖(100 초과)
    [InlineData("abc")]  // 숫자 아님
    public void SetSelectedItemFontSize_InvalidOrOutOfRange_ClearsToGlobal(string param)
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "47=54", // 이미 크기 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSize(param);

        sut.SelectedItemFontSize.Should().Be(0, "유효하지 않은 크기는 해제 → 전역 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("47=", "코드 47(크기)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemFontSize_PreservesExistingColor()
    {
        // 크기만 바꾸고 기존 항목별 색(코드29)은 보존한다(공통 헬퍼 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1", // 흰색.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSize("72");

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("47=72", "새 크기 추가");
    }

    [Fact]
    public void SetSelectedItemFontSize_WhileLive_AppliesOverrideSizePx()
    {
        // 라이브 송출 중 항목 크기를 바꾸면 세션 오버라이드 크기(pt→px 변환)가 즉시 반영된다.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemFontSize("72"); // 72pt → 72*96/72 = 96px.

        session.Current.OverrideFontSizePx.Should().Be(96, "항목 크기가 pt→px 변환돼 라이브에 반영");
    }

    [Fact]
    public void SetSelectedItemFontSize_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSizeCommand.CanExecute("72").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemFontSize("72");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 크기가 붙지 않음");
    }

    // ── 항목별 글꼴명 인라인 편집(SetSelectedItemFontName) — 선택한 곡만 글꼴(레거시 Ind_ 글꼴, 코드43) ──

    [Fact]
    public void SetSelectedItemFontName_WritesFormatDataCode43_AndReflects_AndTrims()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemFontName("  맑은 고딕  "); // 앞뒤 공백 포함.

        sut.SelectedItem!.FormatData.Should().Contain("43=맑은 고딕", "코드 43(영역1 글꼴명)에 다듬어 인코드");
        sut.SelectedItemFontName.Should().Be("맑은 고딕", "글꼴 미리보기도 동기화");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("글꼴을 주면 보이도록 개별 서식을 켠다");
    }

    [Theory]
    [InlineData("")]    // 빈 문자열 → 해제
    [InlineData("   ")] // 공백뿐 → 해제
    public void SetSelectedItemFontName_BlankClearsToGlobal(string param)
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "43=Arial", // 이미 글꼴 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontName(param);

        sut.SelectedItemFontName.Should().BeEmpty("빈/공백 글꼴은 해제 → 전역 글꼴 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("43=", "코드 43(글꼴)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemFontName_PreservesExistingColor()
    {
        // 글꼴만 바꾸고 기존 항목별 색(코드29)은 보존한다(공통 헬퍼 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1", // 흰색.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontName("나눔고딕");

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("43=나눔고딕", "새 글꼴 추가");
    }

    [Fact]
    public void SetSelectedItemFontName_WhileLive_AppliesOverrideFontName()
    {
        // 라이브 송출 중 항목 글꼴을 바꾸면 세션 오버라이드 글꼴이 즉시 반영된다(레거시 per-song 글꼴).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemFontName("Segoe UI");

        session.Current.OverrideFontName.Should().Be("Segoe UI", "항목 글꼴이 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void SetSelectedItemFontName_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontNameCommand.CanExecute("Arial").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemFontName("Arial");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 글꼴이 붙지 않음");
    }

    // ── 항목별 배경색 인라인 편집(SetSelectedItemBackgroundColor) — 선택한 곡만 배경(레거시 Ind_ 배경, 코드26) ──

    [Fact]
    public void SetSelectedItemBackgroundColor_WritesFormatDataCode26_AndEnablesIndividual_AndReflects()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemBackgroundColor("#FF000000"); // 검정(ARGB -16777216).

        sut.SelectedItem!.FormatData.Should().Contain("26=-16777216", "코드 26(영역1 배경색)에 검정 인코드");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("배경을 주면 보이도록 개별 서식을 켠다");
        sut.SelectedItemBackgroundColorHex.Should().Be("#FF000000", "배경 미리보기도 동기화");
    }

    [Fact]
    public void SetSelectedItemBackgroundColor_PreservesExistingTextColor()
    {
        // 배경만 바꾸고 기존 항목별 글자색(코드29)은 보존한다(공통 헬퍼 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1", // 흰색 글자.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemBackgroundColor("#FF202020"); // 진회색 배경.

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("26=", "새 배경색 추가");
    }

    [Fact]
    public void SetSelectedItemBackgroundColor_EmptyOrInvalid_ClearsToGlobal()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "26=-16777216", // 검정 배경이 이미 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemBackgroundColor(""); // 전역 기본으로 해제.

        sut.SelectedItemBackgroundColorHex.Should().BeEmpty("배경 해제 → 전역 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("26=", "코드 26(배경)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemBackgroundColor_WhileLive_AppliesOverrideBackground()
    {
        // 라이브 송출 중 항목 배경색을 바꾸면 세션 오버라이드 배경이 즉시 반영된다(레거시 per-song 배경).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemBackgroundColor("#FF000000"); // 검정(-16777216).

        session.Current.OverrideBackgroundColorArgb.Should().Be(-16777216, "항목 배경이 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void SetSelectedItemBackgroundColor_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemBackgroundColorCommand.CanExecute("#FF000000").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemBackgroundColor("#FF000000");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 배경이 붙지 않음");
    }

    // ── 항목별 배경 이미지 인라인 편집(SetSelectedItemBackgroundImage) — 선택한 곡만 배경 이미지(레거시 Ind_ 배경 이미지, 코드61) ──

    [Fact]
    public void SetSelectedItemBackgroundImage_WritesFormatDataCode61_AndEnablesIndividual_AndReflects()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemBackgroundImage(@"  C:\bg\worship.jpg  "); // 앞뒤 공백 포함.

        sut.SelectedItem!.FormatData.Should().Contain(@"61=C:\bg\worship.jpg", "코드 61(배경 이미지 경로)에 다듬어 인코드");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("이미지를 주면 보이도록 개별 서식을 켠다");
        sut.SelectedItemBackgroundImagePath.Should().Be(@"C:\bg\worship.jpg", "경로 미리보기도 동기화");
    }

    [Fact]
    public void SetSelectedItemBackgroundImage_PreservesExistingTextColor()
    {
        // 배경 이미지만 바꾸고 기존 항목별 글자색(코드29)은 보존한다(공통 헬퍼 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemBackgroundImage(@"C:\bg\a.png");

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain(@"61=C:\bg\a.png", "새 배경 이미지 추가");
    }

    [Fact]
    public void SetSelectedItemBackgroundImage_Blank_ClearsToGlobal()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = @"61=C:\bg\old.jpg",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemBackgroundImage("   ");

        sut.SelectedItemBackgroundImagePath.Should().BeEmpty("빈/공백 경로는 해제 → 전역 배경 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("61=", "코드 61(배경 이미지)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemBackgroundImage_WhileLive_CarriesOverridePath()
    {
        // 라이브 송출 중 항목 배경 이미지를 바꾸면 세션 오버라이드 경로가 즉시 실린다(실제 그리기는 출력 VM 담당).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemBackgroundImage(@"C:\bg\live.png");

        session.Current.OverrideBackgroundImagePath.Should().Be(@"C:\bg\live.png", "항목 배경 이미지 경로가 라이브 세션에 실림");
    }

    [Fact]
    public void SetSelectedItemBackgroundImage_StripsDelimiter_NoCodeInjection()
    {
        // FormatData 구분자 '>'가 경로에 섞이면 "61=...>29=-1>..." 로 엉뚱한 코드(여기선 글자색29=-1)가 주입될 수 있다 →
        // '>'를 제거해 한 항목(경로)으로 유지하고 주입을 막는다('='는 경로에 쓰일 수 있어 보존).
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemBackgroundImage(@"C:\a>29=-1>x.png"); // '>'로 코드29(글자색 -1) 주입 시도.

        sut.SelectedItemTextColorHex.Should().BeEmpty("'>'가 제거돼 코드29(글자색) 주입이 막힘");
        sut.SelectedItemBackgroundImagePath.Should().Be(@"C:\a29=-1x.png", "'>'만 제거되고 경로는 한 항목으로 유지");
    }

    [Fact]
    public void SetSelectedItemBackgroundImage_SamePath_IsNoOp_DoesNotReplaceItem()
    {
        // 이미 같은 배경 이미지면(레코드 동등) 큐 항목을 교체하지 않는다(불필요한 재송출 방지).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = @"61=C:\bg\same.jpg",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];
        var before = sut.SelectedItem;

        sut.SetSelectedItemBackgroundImage(@"C:\bg\same.jpg"); // 같은 경로.

        sut.SelectedItem.Should().BeSameAs(before, "같은 경로면 항목을 새로 만들지 않음(교체 없음)");
    }

    [Fact]
    public void SetSelectedItemBackgroundImage_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemBackgroundImageCommand.CanExecute("").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemBackgroundImage(@"C:\bg\x.jpg");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 배경 이미지가 붙지 않음");
    }

    // ── 항목별 강조 인라인 토글(굵게/기울임/밑줄) — 선택한 곡만(레거시 Ind_ 강조, FormatData 코드41 비트) ──

    [Fact]
    public void ToggleSelectedItemBold_TogglesCode41Bit0_AndEnablesIndividual_AndReflects()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.SelectedItemBold.Should().BeFalse("처음엔 굵게 아님");

        sut.ToggleSelectedItemBold(); // 켜기.

        sut.SelectedItem!.FormatData.Should().Contain("41=1", "코드 41 bit0(굵게) = 1");
        sut.SelectedItemBold.Should().BeTrue("미리보기도 켜짐");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("켜면 보이도록 개별 서식을 켠다");

        sut.ToggleSelectedItemBold(); // 다시 끄기.

        sut.SelectedItemBold.Should().BeFalse("다시 끄면 꺼짐");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("41=", "코드 41(강조 비트)이 제거됨");
    }

    [Fact]
    public void ToggleSelectedItemBoldAndItalic_CombinesBits()
    {
        // 굵게(bit0=1) + 기울임(bit1=2) → 41=3 (비트 합산), 서로의 비트를 보존.
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.ToggleSelectedItemBold();
        sut.ToggleSelectedItemItalic();

        sut.SelectedItem!.FormatData.Should().Contain("41=3", "굵게(1)+기울임(2)=3");
        sut.SelectedItemBold.Should().BeTrue();
        sut.SelectedItemItalic.Should().BeTrue();
        sut.SelectedItemUnderline.Should().BeFalse();
    }

    [Fact]
    public void ToggleSelectedItemUnderline_PreservesExistingColor()
    {
        // 밑줄만 켜고 기존 항목별 글자색(코드29)은 보존한다(공통 헬퍼 왕복).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemUnderline();

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("41=4", "밑줄 bit2=4 추가");
    }

    [Fact]
    public void ToggleSelectedItemBold_Off_PreservesOtherEmphasisBit()
    {
        // 굵게+기울임이 켜진 상태에서 굵게만 끄면, 기울임 비트(2)는 그대로 남아야 한다(코드41 한 비트만 정확히 제거).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "41=3", // 굵게(1)+기울임(2).
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemBold(); // 굵게만 끄기.

        sut.SelectedItem!.FormatData.Should().Contain("41=2", "굵게만 빠지고 기울임(2)은 남음");
        sut.SelectedItemBold.Should().BeFalse();
        sut.SelectedItemItalic.Should().BeTrue("기울임 비트 보존");
    }

    [Fact]
    public void ToggleSelectedItemBold_WhileLive_AppliesOverrideBold()
    {
        // 라이브 송출 중 굵게를 켜면 세션 오버라이드 굵게가 즉시 반영된다(레거시 per-song 강조).
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.ToggleSelectedItemBold();

        session.Current.OverrideBold1.Should().Be(true, "항목 굵게가 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void ToggleSelectedItemEmphasis_NonSong_IsNoOpAndCommandsDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemBoldCommand.CanExecute(null).Should().BeFalse("곡이 아니면 비활성");
        sut.ToggleSelectedItemItalicCommand.CanExecute(null).Should().BeFalse();
        sut.ToggleSelectedItemUnderlineCommand.CanExecute(null).Should().BeFalse();

        sut.ToggleSelectedItemBold();
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 강조가 붙지 않음");
    }

    // ── 이중 언어 보조 영역(Region2) 항목별 글자색(SetSelectedItemTextColor2) — 선택한 곡만(레거시 Ind_ Region2 색, 코드30) ──

    [Fact]
    public void SetSelectedItemTextColor2_WritesFormatDataCode30_AndEnablesIndividual_AndReflects()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemTextColor2("#FFFFFF00"); // 노랑(ARGB 0xFFFFFF00 = -256).

        sut.SelectedItem!.FormatData.Should().Contain("30=-256", "코드 30(Region2 글자색)에 노랑 인코드");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("색을 주면 보이도록 개별 서식을 켠다");
        sut.SelectedItemTextColor2Hex.Should().Be("#FFFFFF00", "보조 영역 색 미리보기도 동기화");
    }

    [Fact]
    public void SetSelectedItemTextColor2_PreservesRegion1Color_AndRegion2Background()
    {
        // 보조 영역 글자색(코드30)만 바꾸고 본문 색(코드29)·보조 영역 배경(코드27)은 보존한다(공통 헬퍼 왕복 — 코드 분리 확인).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n본문\n[region 2]\n번역",
            FormatData = "27=-16777216>29=-1", // Region2 배경 검정 + 본문 흰색.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor2("#FFFFFF00");

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "본문(Region1) 색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("27=-16777216", "보조 영역 배경(코드27) 보존 — 코드30과 무혼선");
        sut.SelectedItem!.FormatData.Should().Contain("30=-256", "보조 영역 글자색(코드30) 추가");
    }

    [Fact]
    public void SetSelectedItemTextColor2_Empty_ClearsToFollowRegion1()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "30=-65536", // 보조 영역 색(빨강 -65536)이 이미 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor2(""); // 본문 색 추종으로 해제.

        sut.SelectedItemTextColor2Hex.Should().BeEmpty("해제 → 본문(Region1) 색 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("30=", "코드 30(Region2 색)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemTextColor2_WhileLive_AppliesOverrideTextColor2()
    {
        // 라이브 송출 중 보조 영역 색을 바꾸면 세션 오버라이드(Region2 색)가 즉시 반영된다.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n본문\n[region 2]\n번역" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemTextColor2("#FFFFFF00"); // 노랑(0xFFFFFF00 = -256).

        session.Current.OverrideTextColorArgb2.Should().Be(-256, "보조 영역 색이 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void SetSelectedItemTextColor2_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemTextColor2Command.CanExecute("#FFFFFF00").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemTextColor2("#FFFFFF00");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 보조 영역 색이 붙지 않음");
    }

    // ── 이중 언어 보조 영역(Region2) 항목별 정렬(SetSelectedItemAlignment2) — 선택한 곡만(레거시 Ind_ Region2 정렬, 코드32) ──

    [Theory]
    [InlineData("1", 1, "32=1")] // 왼쪽
    [InlineData("2", 2, "32=2")] // 가운데
    [InlineData("3", 3, "32=3")] // 오른쪽
    public void SetSelectedItemAlignment2_WritesFormatDataCode32_AndReflects(string param, int expected, string token)
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemAlignment2(param);

        sut.SelectedItem!.FormatData.Should().Contain(token, "코드 32(Region2 정렬)에 인코드");
        sut.SelectedItemAlignment2.Should().Be(expected, "보조 영역 정렬 미리보기도 동기화");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("정렬을 주면 보이도록 개별 서식을 켠다");
    }

    [Fact]
    public void SetSelectedItemAlignment2_PreservesRegion1Alignment()
    {
        // 보조 영역 정렬(코드32)만 바꾸고 본문 정렬(코드31)은 보존한다(공통 헬퍼 왕복 — 코드31/32 분리).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n본문\n[region 2]\n번역",
            FormatData = "31=2", // 본문 가운데 정렬.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemAlignment2("3"); // 보조 영역 오른쪽.

        sut.SelectedItem!.FormatData.Should().Contain("31=2", "본문(Region1) 정렬 보존");
        sut.SelectedItem!.FormatData.Should().Contain("32=3", "보조 영역(Region2) 정렬 추가");
    }

    [Fact]
    public void SetSelectedItemAlignment2_ZeroOrInvalid_ClearsToFollowRegion1()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "32=3", // 보조 영역 오른쪽이 이미 지정됨.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemAlignment2("0");

        sut.SelectedItemAlignment2.Should().Be(0, "해제 → 본문 정렬 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("32=", "코드 32(Region2 정렬)가 제거됨");
    }

    [Fact]
    public void SetSelectedItemAlignment2_WhileLive_AppliesOverrideAlignment2()
    {
        // 라이브 송출 중 보조 영역 정렬을 바꾸면 세션 오버라이드(Region2 정렬)가 즉시 반영된다.
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n본문\n[region 2]\n번역" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemAlignment2("3"); // 오른쪽.

        session.Current.OverrideTextAlignment2.Should().Be(LyricsTextAlignment.Right, "보조 영역 정렬이 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void SetSelectedItemAlignment2_NonSong_IsNoOpAndCommandDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemAlignment2Command.CanExecute("2").Should().BeFalse("곡이 아니면 명령 비활성");
        sut.SetSelectedItemAlignment2("2");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 보조 영역 정렬이 붙지 않음");
    }

    // ── 이중 언어 보조 영역(Region2) 항목별 글자 크기(코드48)·글꼴(코드44) ──

    [Theory]
    [InlineData("36", 36)]
    [InlineData("72", 72)]
    public void SetSelectedItemFontSize2_WritesFormatDataCode48_AndReflects(string param, int expected)
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemFontSize2(param);

        sut.SelectedItem!.FormatData.Should().Contain($"48={expected}", "코드 48(Region2 글자 크기)에 인코드");
        sut.SelectedItemFontSize2.Should().Be(expected, "보조 영역 크기 미리보기도 동기화");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("크기를 주면 개별 서식을 켠다");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("5")]
    [InlineData("abc")]
    public void SetSelectedItemFontSize2_InvalidOrOutOfRange_ClearsToFollowRegion1(string param)
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "48=54",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSize2(param);

        sut.SelectedItemFontSize2.Should().Be(0, "유효하지 않은 크기는 해제 → 본문 크기 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("48=", "코드 48(Region2 크기)이 제거됨");
    }

    [Fact]
    public void SetSelectedItemFontSize2_WhileLive_AppliesOverrideSize2()
    {
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n본문\n[region 2]\n번역" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.SetSelectedItemFontSize2("72"); // 72pt → 96px.

        session.Current.OverrideFontSizePx2.Should().Be(96, "보조 영역 크기가 pt→px 변환돼 라이브에 반영");
    }

    [Fact]
    public void SetSelectedItemFontName2_WritesFormatDataCode44_AndTrims_AndReflects()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemFontName2("  나눔고딕  ");

        sut.SelectedItem!.FormatData.Should().Contain("44=나눔고딕", "코드 44(Region2 글꼴)에 다듬어 인코드");
        sut.SelectedItemFontName2.Should().Be("나눔고딕", "보조 영역 글꼴 미리보기도 동기화");
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("글꼴을 주면 개별 서식을 켠다");
    }

    [Fact]
    public void SetSelectedItemFontName2_Blank_ClearsToFollowRegion1()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "44=Arial",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontName2("   ");

        sut.SelectedItemFontName2.Should().BeEmpty("빈/공백 글꼴은 해제 → 본문 글꼴 추종");
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("44=", "코드 44(Region2 글꼴)가 제거됨");
    }

    [Fact]
    public void SetSelectedItemFontSize2AndName2_PreserveRegion1AndEachOther()
    {
        // Region2 크기(48)·글꼴(44)은 본문(47/43)·서로와 무혼선(코드 분리).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "43=굴림>47=60", // 본문 글꼴·크기.
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSize2("36");
        sut.SetSelectedItemFontName2("Arial");

        var fd = sut.SelectedItem!.FormatData ?? "";
        fd.Should().Contain("43=굴림", "본문 글꼴 보존");
        fd.Should().Contain("47=60", "본문 크기 보존");
        fd.Should().Contain("48=36", "Region2 크기");
        fd.Should().Contain("44=Arial", "Region2 글꼴");
    }

    [Fact]
    public void SetSelectedItemFontSize2_WhenIndividualWasOff_DisclosesAutoEnableInStatus()
    {
        // 개별 서식을 꺼둔 항목에 Region2 크기를 주면 자동으로 켜지고, 상태줄이 그 사실을 정직하게 알린다(turnedOn 분기).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            UseIndividualFormatting = false,
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSize2("54");

        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("크기를 주면 자동으로 켬");
        sut.StatusText.Should().Contain("54pt").And.Contain("개별 서식 켜짐", "크기와 자동 켬을 상태줄에 알림");
    }

    [Fact]
    public void SetSelectedItemFontSize2AndName2_NonSong_CommandsDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.SetSelectedItemFontSize2Command.CanExecute("72").Should().BeFalse();
        sut.SetSelectedItemFontName2Command.CanExecute("Arial").Should().BeFalse();
        sut.SetSelectedItemFontSize2("72");
        sut.SetSelectedItemFontName2("Arial");
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 보조 영역 서식이 붙지 않음");
    }

    // ── 이중 언어 보조 영역(Region2) 항목별 강조(굵게/기울임/밑줄, 코드41 상위비트 bit3/4/5) ──

    [Fact]
    public void ToggleSelectedItemBold2_TogglesCode41Bit3_AndReflects()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));
        sut.SelectedItemBold2.Should().BeFalse();

        sut.ToggleSelectedItemBold2(); // 켜기.

        sut.SelectedItem!.FormatData.Should().Contain("41=8", "코드 41 bit3(Region2 굵게) = 8");
        sut.SelectedItemBold2.Should().BeTrue();
        sut.SelectedItem!.UseIndividualFormatting.Should().BeTrue("켜면 개별 서식을 켠다");

        sut.ToggleSelectedItemBold2(); // 끄기.
        sut.SelectedItemBold2.Should().BeFalse();
        (sut.SelectedItem!.FormatData ?? "").Should().NotContain("41=", "비트 0 → 코드41 제거");
    }

    [Fact]
    public void ToggleSelectedItemBold2_CoexistsWithRegion1Bold_InSameCode41()
    {
        // 본문 굵게(bit0=1)와 보조 영역 굵게(bit3=8)는 같은 코드41 안에서 독립적으로 공존 → 41=9.
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n본문\n[region 2]\n번역",
            FormatData = "41=1", // 본문 굵게(bit0).
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemBold2(); // 보조 영역 굵게 추가(bit3).

        sut.SelectedItem!.FormatData.Should().Contain("41=9", "본문 굵게(1)+보조 굵게(8)=9");
        sut.SelectedItemBold.Should().BeTrue("본문 굵게 비트 보존");
        sut.SelectedItemBold2.Should().BeTrue("보조 영역 굵게 추가됨");
    }

    [Fact]
    public void ToggleSelectedItemBold2_Off_PreservesOtherRegion2AndRegion1Bits()
    {
        // 본문 굵게(1)+보조 굵게(8)+보조 기울임(16)=41=25 에서 보조 굵게만 끄면 → 41=17(본문 굵게 1 + 보조 기울임 16) 보존.
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "41=25", // Bold1(1)+Bold2(8)+Italic2(16).
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemBold2(); // 보조 굵게만 끄기.

        sut.SelectedItem!.FormatData.Should().Contain("41=17", "본문 굵게(1)+보조 기울임(16)=17만 남음");
        sut.SelectedItemBold.Should().BeTrue("본문 굵게(bit0) 보존");
        sut.SelectedItemItalic2.Should().BeTrue("보조 기울임(bit4) 보존");
        sut.SelectedItemBold2.Should().BeFalse("보조 굵게(bit3)만 꺼짐");
    }

    [Fact]
    public void ToggleSelectedItemItalic2_PreservesExistingColor()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "29=-1",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemItalic2();

        sut.SelectedItem!.FormatData.Should().Contain("29=-1", "기존 글자색 보존");
        sut.SelectedItem!.FormatData.Should().Contain("41=16", "Region2 기울임 bit4=16");
    }

    [Fact]
    public void ToggleSelectedItemUnderline2_WhileLive_AppliesOverrideUnderline2()
    {
        var session = new LiveSessionService();
        var sut = CreateSut(seedSampleQueue: false, liveSession: session);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n본문\n[region 2]\n번역" };
        sut.LoadQueue([item]);
        sut.SelectedItem = item;
        sut.GoLiveCommand.Execute(null);

        sut.ToggleSelectedItemUnderline2();

        session.Current.OverrideUnderline2.Should().Be(true, "보조 영역 밑줄이 라이브 송출에 즉시 반영");
    }

    [Fact]
    public void ToggleSelectedItemEmphasis2_NonSong_IsNoOpAndCommandsDisabled()
    {
        var sut = CreateSut(seedSampleQueue: false);
        var notice = new LiveQueueItem("n", "공지", LiveItemKinds.Notice) { Lyrics = "안내" };
        sut.LoadQueue([notice]);
        sut.SelectedItem = sut.Queue[0];

        sut.ToggleSelectedItemBold2Command.CanExecute(null).Should().BeFalse();
        sut.ToggleSelectedItemItalic2Command.CanExecute(null).Should().BeFalse();
        sut.ToggleSelectedItemUnderline2Command.CanExecute(null).Should().BeFalse();

        sut.ToggleSelectedItemBold2();
        sut.SelectedItem!.FormatData.Should().BeNull("공지 항목엔 보조 영역 강조가 붙지 않음");
    }

    [Fact]
    public void SetSelectedItemFontName_SameName_IsNoOp_DoesNotReplaceItem()
    {
        // 이미 같은 글꼴이면(레코드 동등, 문자열 "" 센티넬 포함) 큐 항목을 교체하지 않는다(불필요한 재송출 방지).
        var sut = CreateSut(seedSampleQueue: false);
        var item = new LiveQueueItem("song:1", "곡", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사",
            FormatData = "43=Arial",
        };
        sut.LoadQueue([item]);
        sut.SelectedItem = sut.Queue[0];
        var before = sut.SelectedItem;

        sut.SetSelectedItemFontName("Arial"); // 같은 글꼴.

        sut.SelectedItem.Should().BeSameAs(before, "같은 글꼴이면 항목을 새로 만들지 않음(교체 없음)");
    }

    [Fact]
    public void SetSelectedItemFontName_StripsDelimiterChars_NoFormatCorruption()
    {
        // FormatData 구분자('>'·'=')가 글꼴명에 섞여 들어와도 제거해 포맷이 깨지거나 엉뚱한 코드가 주입되지 않게 한다.
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\n가사"));

        sut.SetSelectedItemFontName("Foo>29=-1"); // 악성/오타 입력 — '>'·'=' 가 주입 시도.

        sut.SelectedItemFontName.Should().Be("Foo29-1", "구분자('>'·'=')는 제거됨");
        // 주입이 막혔는지: 글자색(코드29)이 따로 생기지 않아야 한다.
        sut.SelectedItem!.FormatData.Should().NotContain("29=", "엉뚱한 코드 주입 없음");
    }

    // ── 절 라벨 직접 점프(레거시 FrmInfoScreen 절 버튼 1~9·c·b 대응) ──

    [Fact]
    public void AddSong_WithSequence_ExposesDistinctSectionLabels()
    {
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two");

        sut.AddSong(song, "1 C 2 C");

        // 펼친 순서 1 C 2 C 의 중복 제거(첫 등장 순서) → 1, C, 2.
        sut.AvailableSectionLabels.Should().Equal("1", "C", "2");
    }

    [Fact]
    public void JumpToLyricsSection_MovesToFirstPageWithLabel()
    {
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two");
        sut.AddSong(song, "1 C 2 C"); // 페이지: 1(0) C(1) 2(2) C(3)

        sut.JumpToLyricsSectionCommand.Execute("C");
        sut.LyricsPageIndex.Should().Be(1, "첫 후렴(C)은 인덱스 1");

        sut.JumpToLyricsSectionCommand.Execute("2");
        sut.LyricsPageIndex.Should().Be(2);

        sut.JumpToLyricsSectionCommand.Execute("1");
        sut.LyricsPageIndex.Should().Be(0);
    }

    [Fact]
    public void JumpToLyricsSection_UnknownLabel_DoesNotChangePage()
    {
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus");
        sut.AddSong(song); // 선형: 1(0) C(1)
        sut.NextLyricsPageCommand.Execute(null); // → 인덱스 1

        sut.JumpToLyricsSectionCommand.Execute("X"); // 없는 라벨

        sut.LyricsPageIndex.Should().Be(1, "없는 라벨은 무시(이동 없음)");
    }

    [Fact]
    public void AddSong_DualLanguage_PageCountUsesRegionPages()
    {
        // 이중 언어([region 2]) 곡은 영역-인식 페이지 수를 쓴다 — [region 2] 가 절 경계로 오인돼 절 수가 부풀지 않음.
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nAmazing\n[region 2]\n은혜\n[2]\nGrace\n[region 2]\n주님");

        sut.AddSong(song);

        sut.LyricsPageCount.Should().Be(2, "두 절(각 R1/R2 쌍) → 2페이지(=GetRegionPages 수)");
        // 전부 라벨링된 이중 언어 곡은 절 라벨 점프가 켜진다(라벨이 페이지와 정렬 — 슬라이스 7).
        sut.AvailableSectionLabels.Should().Equal("1", "2");
    }

    [Fact]
    public void AddSong_CarriesSongNumberToQueueItem()
    {
        // 곡 번호가 큐 항목에 실려 출력 "곡 번호 표시"에 쓰인다(데이터가 큐에 들어오는 유일 지점).
        var sut = CreateSut();
        var song = new SongSummary(123, "은혜", "", 1, 123, "", "", "[1]\n1절");

        var item = sut.AddSong(song);

        item!.SongNumber.Should().Be(123);
    }

    [Fact]
    public void AddSong_DualLanguage_AllLabeled_EnablesSectionJump()
    {
        // 전부 라벨링된 이중 언어 곡은 절 라벨 점프가 켜진다(라벨이 페이지와 정렬).
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "",
            "[1]\nV R1\n[region 2]\nV R2\n[C]\nC R1\n[region 2]\nC R2");

        sut.AddSong(song);

        sut.AvailableSectionLabels.Should().Equal("1", "C");

        sut.JumpToLyricsSectionCommand.Execute("C");
        sut.LyricsPageIndex.Should().Be(1, "후렴(C)은 인덱스 1");
    }

    [Fact]
    public void AddSong_DualLanguageWithSequence_PageCountExpandsBySequence()
    {
        // 이중 언어 곡 + 시퀀스("1 C 1") → 영역 쌍이 시퀀스 순서로 펼쳐져 3페이지.
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "",
            "[1]\nVerse R1\n[region 2]\nVerse R2\n[C]\nChorus R1\n[region 2]\nChorus R2");

        sut.AddSong(song, "1 C 1");

        sut.LyricsPageCount.Should().Be(3, "이중 언어도 시퀀스(1 C 1)로 3페이지로 펼쳐진다");
    }

    [Fact]
    public void RefreshLyricsPages_NonSongItem_ClearsSectionLabels()
    {
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one\n[C]\nChorus");
        sut.AddSong(song, "1 C");
        sut.AvailableSectionLabels.Should().NotBeEmpty();

        // PPT 항목을 추가·선택하면 곡이 아니므로 절 라벨이 비워진다.
        sut.AddPowerPoint(@"C:\deck.pptx");

        sut.AvailableSectionLabels.Should().BeEmpty();
    }

    [Fact]
    public async Task AddSearchedSong_CarriesSongDetailSequence_ToQueueItem_AndExpandsPages()
    {
        // 종단(통합): 검색 결과 추가 → 상세 로드 → 큐 항목에 절 순서(Sequence)가 실리고 페이지가 펼쳐진다.
        // (단위 테스트가 AddSong 직접 주입만 검증하면 상세 로드 경로 누락을 못 잡으므로 종단 테스트 필요.)
        var detail = SampleSongDetail(7, "은혜", "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two", sequence: "1 C 2 C");
        var sut = CreateSut(songDetail: new StubSongDetailRepository(detail));
        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        var result = new SongSearchResult(7, 1, "찬양 폴더", "은혜", "", 12, "", "G", new[] { "Title" }, "");
        sut.Search.SearchResults.Add(result);
        sut.SelectedSearchResult = result;

        await sut.AddSearchedSongCommand.ExecuteAsync(null);

        var added = sut.Queue.Single(i => i.Title == "은혜");
        added.Sequence.Should().Be("1 C 2 C", "상세(SongDetail)의 절 순서가 큐 항목까지 전달돼야 함");
        sut.SelectedItem.Should().BeSameAs(added);
        sut.LyricsPageCount.Should().Be(4, "시퀀스(1 C 2 C)로 4페이지로 펼쳐진다");
    }

    [Fact]
    public void AddSong_WithFormatData_CarriesFormatData_ToQueueItem()
    {
        // 곡별 FormatData(레거시 v32 색·정렬 사전)도 큐 항목까지 실어 라이브 출력이 그 곡의 색으로 송출하게 한다.
        var sut = CreateSut();
        var song = new SongSummary(1, "은혜", "", 1, 1, "", "", "[1]\nVerse one");

        var item = sut.AddSong(song, sequence: null, formatData: "29=-65536>26=-16776961>");

        item!.FormatData.Should().Be("29=-65536>26=-16776961>");
    }

    [Fact]
    public async Task AddSearchedSong_CarriesSongDetailFormatData_ToQueueItem()
    {
        // 종단(통합): 검색 결과 추가 → 상세 로드 → 큐 항목에 곡 FormatData 가 실린다(per-song 색 송출의 전제).
        var detail = SampleSongDetail(7, "은혜", "[1]\nVerse one", formatData: "29=-65536>");
        var sut = CreateSut(songDetail: new StubSongDetailRepository(detail));
        sut.Search.DatabasePath = @"C:\work\Admin\Database\EasiSlidesDb.db";
        var result = new SongSearchResult(7, 1, "찬양 폴더", "은혜", "", 12, "", "G", new[] { "Title" }, "");
        sut.Search.SearchResults.Add(result);
        sut.SelectedSearchResult = result;

        await sut.AddSearchedSongCommand.ExecuteAsync(null);

        var added = sut.Queue.Single(i => i.Title == "은혜");
        added.FormatData.Should().Be("29=-65536>", "상세(SongDetail)의 FormatData 가 큐 항목까지 전달돼야 함");
    }

    private static SongDetail SampleSongDetail(int songId, string title, string lyrics, string sequence = "", string formatData = "")
        => new(songId, title, "", 1, 0, lyrics, sequence, "", "", 0, "", "", "", "", "", "", "", "", "", formatData);

    // 곡 상세(가사) 조회 스텁 — 검색 결과 SongId → 가사 로드 경로 검증용.
    private sealed class StubSongDetailRepository : IAdminSongDetailRepository
    {
        private readonly SongDetail? _detail;

        public StubSongDetailRepository(SongDetail? detail) => _detail = detail;

        public string? LastDatabasePath { get; private set; }

        public int LastSongId { get; private set; }

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
        {
            LastDatabasePath = databasePath;
            LastSongId = songId;
            return Task.FromResult(_detail);
        }
    }

    // ── 증분160-D: 스테이지(Preview) 모니터 선택 + 열기/닫기 ──

    [Fact]
    public void RefreshPreviewDisplays_PopulatesCandidates_AndSelectsOne()
    {
        var sut = CreateSut(seedSampleQueue: false);

        sut.PreviewDisplays.Should().NotBeEmpty("스테이지 모니터 후보 목록이 채워져야 함");
        sut.SelectedPreviewDisplay.Should().NotBeNull("기본 스테이지 모니터가 선택돼야 함");
    }

    [Fact]
    public void OpenStageMonitorCommand_OpensPreviewOnSelectedDisplay()
    {
        var preview = new PreviewWindowService();
        var sut = CreateSut(preview: preview, seedSampleQueue: false);

        sut.OpenStageMonitorCommand.Execute(null);

        preview.Current.IsOpen.Should().BeTrue("스테이지 모니터 서비스가 열려야 함");
        preview.Current.Display.Should().Be(sut.SelectedPreviewDisplay, "선택한 모니터에 열려야 함");
        sut.IsStageMonitorOpen.Should().BeTrue("VM 의 열림 상태가 따라가야 함");
    }

    [Fact]
    public void CloseStageMonitorCommand_ClosesPreview()
    {
        var preview = new PreviewWindowService();
        var sut = CreateSut(preview: preview, seedSampleQueue: false);
        sut.OpenStageMonitorCommand.Execute(null);

        sut.CloseStageMonitorCommand.Execute(null);

        preview.Current.IsOpen.Should().BeFalse();
        sut.IsStageMonitorOpen.Should().BeFalse();
    }

    [Fact]
    public void CloseStageMonitorCommand_DisabledUntilOpen()
    {
        var preview = new PreviewWindowService();
        var sut = CreateSut(preview: preview, seedSampleQueue: false);

        sut.CloseStageMonitorCommand.CanExecute(null).Should().BeFalse("닫혀 있으면 닫기 비활성");

        sut.OpenStageMonitorCommand.Execute(null);

        sut.CloseStageMonitorCommand.CanExecute(null).Should().BeTrue("열리면 닫기 활성");
    }

    [Fact]
    public void SelectingDifferentPreviewDisplay_WhileOpen_MovesStageMonitor()
    {
        // 스테이지 창이 열린 채 다른 모니터를 고르면 그 모니터로 즉시 옮긴다(출력 모니터 이동과 같은 취지).
        var primary = new OutputDisplay("p1", "Primary", 0, 0, 1920, 1080, 1, IsPrimary: true);
        var stage = new OutputDisplay("p2", "Stage", 1920, 0, 1920, 1080, 1);
        var preview = new PreviewWindowService();
        var sut = CreateSut(display: new FixedDisplayService(primary, stage), preview: preview, seedSampleQueue: false);
        sut.OpenStageMonitorCommand.Execute(null); // 기본 선택(primary)에 열림

        sut.SelectedPreviewDisplay = stage;        // 다른 모니터 선택 → 이동

        preview.Current.IsOpen.Should().BeTrue();
        preview.Current.Display.Should().Be(stage, "열린 채 모니터를 바꾸면 그 모니터로 이동해야 함");
    }

    private static MainViewModel CreateSut(
        ILiveSafetyPrompt? prompt = null,
        IDisplayService? display = null,
        ICommandCatalog? commandCatalog = null,
        ISettingsService? settings = null,
        IWorshipListStore? worshipLists = null,
        PowerPointPreviewViewModel? powerPoint = null,
        IAppearanceTemplateStore? appearanceTemplates = null,
        IAdminSongDetailRepository? songDetail = null,
        IRecentWorshipLists? recentWorshipLists = null,
        WorshipListValidator? worshipValidator = null,
        LiveSessionService? liveSession = null,
        IOutputWindowService? output = null,
        IPreviewWindowService? preview = null,
        bool seedSampleQueue = true)
    {
        output ??= new OutputWindowService();
        var session = liveSession ?? new LiveSessionService();
        var telemetry = new InMemoryCommandTelemetry();
        var media = new MediaPlaybackViewModel(new MediaPlaybackService());
        powerPoint ??= new PowerPointPreviewViewModel(new StubPowerPointRenderService());
        var resolvedSettings = settings ?? TempSettingsFolder.CreateDetachedSettings();
        // 라이브러리/성경/검색 VM — 테스트는 작업 폴더/DB 미설정이라 실제 repo 가 데이터를 반환하지 않는다(빈 목록).
        var library = new LibraryViewModel(resolvedSettings, new AdminDatabaseRepository());
        var bible = new BibleViewModel(resolvedSettings, new BibleRepository());
        var search = new SearchUsageViewModel(resolvedSettings, new SearchUsageService(new AdminDatabaseRepository()));
        var vm = new MainViewModel(
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
            search,
            worshipLists ?? new InMemoryWorshipListStore(),
            appearanceTemplates ?? new InMemoryAppearanceTemplateStore(),
            songDetail ?? new AdminDatabaseRepository(),
            recentWorshipLists ?? new InMemoryRecentWorshipLists(),
            worshipValidator,
            preview);

        // 운영 기본 큐는 비어 있다(더미 시드 제거). 대부분의 테스트는 Queue[0] 등 채워진 큐를 가정하므로
        // CreateSut 가 기본으로 샘플 3항목을 시드해 기존 테스트를 보존한다. 빈 큐가 필요하면 seedSampleQueue:false.
        if (seedSampleQueue)
        {
            vm.LoadQueue(new[]
            {
                new LiveQueueItem("sample-welcome", "예배 시작 안내", LiveItemKinds.Notice),
                new LiveQueueItem("sample-song", "주일 찬양 #1", LiveItemKinds.Song),
                new LiveQueueItem("sample-sermon", "말씀 본문", LiveItemKinds.Bible),
            });
        }

        return vm;
    }

    // 최근 예배 순서 — 파일시스템 없이 인메모리로 검증.
    private sealed class InMemoryRecentWorshipLists : IRecentWorshipLists
    {
        private readonly List<string> _items = new();

        public void Record(string name)
        {
            var trimmed = name?.Trim();
            if (string.IsNullOrEmpty(trimmed)) return;
            _items.RemoveAll(n => string.Equals(n, trimmed, System.StringComparison.OrdinalIgnoreCase));
            _items.Insert(0, trimmed);
        }

        public IReadOnlyList<string> GetRecent() => _items.ToArray();
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

        public void Rename(string oldName, string newName)
        {
            if (!_store.TryGetValue(oldName, out var items))
            {
                return; // 원본 없음 — 무시(실제 스토어와 동일하게 관대)
            }

            if (_store.ContainsKey(newName))
            {
                throw new ArgumentException($"이미 있는 이름입니다: {newName}", nameof(newName));
            }

            _store.Remove(oldName);
            _store[newName] = items;
        }
    }

    // 출력 모양 템플릿 저장/로드 — 파일시스템 없이 인메모리로 검증.
    private sealed class InMemoryAppearanceTemplateStore : IAppearanceTemplateStore
    {
        private readonly Dictionary<string, LyricsAppearanceTemplate> _store = new();

        public Task SaveAsync(string name, LyricsAppearanceTemplate template, CancellationToken cancellationToken = default)
        {
            _store[name.Trim()] = template;
            return Task.CompletedTask;
        }

        public Task<LyricsAppearanceTemplate?> LoadAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.TryGetValue(name.Trim(), out var t) ? t : null);

        public IReadOnlyList<string> ListNames() => _store.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToArray();

        public void Delete(string name) => _store.Remove(name.Trim());
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
