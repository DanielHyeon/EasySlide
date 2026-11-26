using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

public class SettingsServiceTests
{
    [Fact]
    public void Set_WithValidTypedKey_PersistsAndRaisesChangedEvent()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var raised = new List<SettingsChangedEventArgs>();
        sut.SettingsChanged += (_, args) => raised.Add(args);

        var result = sut.Set(AxicultusSettingKeys.MediaVolume, 0.72);

        result.Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.MediaVolume).Should().Be(0.72);
        File.Exists(fixture.SettingsPath).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).Media.Volume.Should().Be(0.72);
        raised.Should().ContainSingle();
        raised[0].ChangedKeys.Should().Contain(AxicultusSettingKeys.MediaVolume.Id);
        raised[0].Source.Should().Be(SettingsChangeSource.User);
    }

    [Fact]
    public void Set_LyricsTextAlignment_PersistsEnumToDiskAndReloads()
    {
        // 인-셸 가사 정렬 enum 의 디스크 round-trip 고정(JsonStringEnumConverter 역직렬화 경로 검증).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorTextAlignment, LyricsTextAlignment.Right).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorTextAlignment).Should().Be(LyricsTextAlignment.Right);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorTextAlignment.Should().Be(LyricsTextAlignment.Right);
    }

    [Fact]
    public void Set_LyricsVerticalAlignment_PersistsEnumToDiskAndReloads()
    {
        // 세로 정렬 enum 디스크 round-trip 고정.
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorVerticalAlignment, LyricsVerticalAlignment.Bottom).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorVerticalAlignment).Should().Be(LyricsVerticalAlignment.Bottom);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorVerticalAlignment.Should().Be(LyricsVerticalAlignment.Bottom);
    }

    [Fact]
    public void Set_LyricsMonitorShowCopyright_PersistsToDiskAndReloads()
    {
        // 저작권 표시 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증, Display Panel).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorShowCopyright, true).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorShowCopyright).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorShowCopyright.Should().BeTrue();
    }

    [Fact]
    public void Default_LyricsMonitorPanelTransparent_IsOff()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.LyricsMonitorPanelTransparent).Should().BeFalse("기본은 반투명 밴드(무회귀)");
    }

    [Fact]
    public void Set_LyricsMonitorPanelTransparent_PersistsToDiskAndReloads()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorPanelTransparent, true).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorPanelTransparent).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorPanelTransparent.Should().BeTrue();
    }

    [Fact]
    public void Default_UseSongNumbering_IsOff()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.UseSongNumbering).Should().BeFalse("기본은 제목만(무회귀)");
    }

    [Fact]
    public void Constructor_WhenNoSettingsAndLegacyRootExists_UsesLegacyWorkingFolder()
    {
        using var fixture = TempSettingsFolder.Create();

        var sut = fixture.CreateService(fixture.LegacyFolder);

        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(Path.GetFullPath(fixture.LegacyFolder));
    }

    [Fact]
    public void Constructor_WhenStoredDefaultWorkingFolderAndLegacyRootExists_UsesLegacyWorkingFolder()
    {
        using var fixture = TempSettingsFolder.Create();
        File.WriteAllText(fixture.SettingsPath, Serialize(AxicultusSettingsSnapshot.CreateDefault()));

        var sut = fixture.CreateService(fixture.LegacyFolder);

        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(Path.GetFullPath(fixture.LegacyFolder));
    }

    [Fact]
    public void Constructor_WhenStoredCustomWorkingFolderAndLegacyRootExists_PreservesCustomWorkingFolder()
    {
        using var fixture = TempSettingsFolder.Create();
        var customWorkingFolder = Path.Combine(fixture.Root, "CustomWork");
        var snapshot = AxicultusSettingsSnapshot.CreateDefault() with
        {
            General = AxicultusSettingsSnapshot.CreateDefault().General with
            {
                WorkingFolder = customWorkingFolder,
            },
        };
        File.WriteAllText(fixture.SettingsPath, Serialize(snapshot));

        var sut = fixture.CreateService(fixture.LegacyFolder);

        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(customWorkingFolder);
    }

    [Fact]
    public void Set_UseSongNumbering_PersistsToDiskAndReloads()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.UseSongNumbering, true).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.UseSongNumbering).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).General.UseSongNumbering.Should().BeTrue();
    }

    [Fact]
    public void Default_LyricsMonitorBackgroundMode_IsFill()
    {
        // 배경 표시 모드 기본값은 Fill(=기존 UniformToFill) — 끄면 기존 출력과 동일(무회귀).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.LyricsMonitorBackgroundMode).Should().Be(LyricsBackgroundMode.Fill);
    }

    [Fact]
    public void Set_LyricsMonitorBackgroundMode_PersistsEnumToDiskAndReloads()
    {
        // 배경 표시 모드 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorBackgroundMode, LyricsBackgroundMode.Tile).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorBackgroundMode).Should().Be(LyricsBackgroundMode.Tile);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorBackgroundMode.Should().Be(LyricsBackgroundMode.Tile);
    }

    [Fact]
    public void Default_LyricsMonitorShowNotations_IsOff()
    {
        // 코드 표시 기본값은 off — 회중 화면은 예부터 코드를 숨겼고, 끄면 본문이 비트 동일(무회귀)이다.
        // (기본 on 이면 모든 기존 곡이 첫 실행부터 코드를 송출하는 회귀가 생긴다 — 이 핀으로 재발 방지.)
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.LyricsMonitorShowNotations).Should().BeFalse();
    }

    [Fact]
    public void Set_LyricsMonitorShowNotations_PersistsToDiskAndReloads()
    {
        // 코드 표시 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorShowNotations).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorShowNotations.Should().BeTrue();
    }

    [Fact]
    public void Set_LyricsMonitorShowNextItem_PersistsToDiskAndReloads()
    {
        // 다음 항목 표시 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증, Display Panel PrevNext).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorShowNextItem, true).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorShowNextItem).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorShowNextItem.Should().BeTrue();
    }

    [Fact]
    public void Set_LyricsMonitorTransitionKind_PersistsEnumToDiskAndReloads()
    {
        // 전환 모션 종류(enum)의 디스크 round-trip 고정(JsonStringEnumConverter 경로 + GET/SET 스위치).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorTransitionKind, LyricsTransitionKind.ZoomIn).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorTransitionKind).Should().Be(LyricsTransitionKind.ZoomIn);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorTransitionKind.Should().Be(LyricsTransitionKind.ZoomIn);
    }

    [Fact]
    public void Set_LyricsMonitorTransitionSettings_PersistToDiskAndReload()
    {
        // 전환 페이드 사용 여부(bool) + 길이(int)의 디스크 round-trip 고정(GET/SET 스위치 검증, FrmMain 전환 효과).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorUseFadeTransition, false).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.LyricsMonitorTransitionDurationMs, 500).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorUseFadeTransition).Should().BeFalse();
        sut.Get(AxicultusSettingKeys.LyricsMonitorTransitionDurationMs).Should().Be(500);
        var snapshot = ReadSnapshot(fixture.SettingsPath);
        snapshot.LiveOutput.LyricsMonitorUseFadeTransition.Should().BeFalse();
        snapshot.LiveOutput.LyricsMonitorTransitionDurationMs.Should().Be(500);
    }

    [Fact]
    public void Set_LyricsMonitorTransitionDuration_OutOfRange_IsRejected()
    {
        // 전환 길이는 0~2000ms 범위 가드. 음수/과대값은 거부되고 이전 값(기본 250) 유지.
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorTransitionDurationMs, -50).Succeeded.Should().BeFalse();
        sut.Set(AxicultusSettingKeys.LyricsMonitorTransitionDurationMs, 9000).Succeeded.Should().BeFalse();

        sut.Get(AxicultusSettingKeys.LyricsMonitorTransitionDurationMs).Should().Be(250);
    }

    [Fact]
    public void Set_LyricsMonitorBackgroundImagePath_PersistsToDiskAndReloads()
    {
        // 전역 출력 배경 이미지 경로(string)의 디스크 round-trip 고정(GET/SET 스위치 검증, FrmMain Images 탭).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.LyricsMonitorBackgroundImagePath, @"C:\bg\global.png").Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.LyricsMonitorBackgroundImagePath).Should().Be(@"C:\bg\global.png");
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorBackgroundImagePath.Should().Be(@"C:\bg\global.png");
    }

    [Fact]
    public void Set_DefaultMediaPath_PersistsToDiskAndReloads()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(AxicultusSettingKeys.DefaultMediaPath, @"C:\media\default.mp4").Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.DefaultMediaPath).Should().Be(@"C:\media\default.mp4");
        ReadSnapshot(fixture.SettingsPath).Media.DefaultMediaPath.Should().Be(@"C:\media\default.mp4");
    }

    [Fact]
    public void Set_WhenValueIsInvalid_ReturnsIssueAndKeepsPreviousValue()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.PowerPointRenderTimeoutSeconds, 30).Succeeded.Should().BeTrue();

        var result = sut.Set(AxicultusSettingKeys.PowerPointRenderTimeoutSeconds, 0);

        result.Succeeded.Should().BeFalse();
        result.Issues.Should().Contain(issue => issue.Key == AxicultusSettingKeys.PowerPointRenderTimeoutSeconds.Id);
        sut.Get(AxicultusSettingKeys.PowerPointRenderTimeoutSeconds).Should().Be(30);
        ReadSnapshot(fixture.SettingsPath).PowerPoint.RenderTimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void PowerPointTitleOrderIsOptInAndSurvivesReload()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        settings.Get(AxicultusSettingKeys.PowerPointSortByTitle).Should().BeFalse();
        settings.Set(AxicultusSettingKeys.PowerPointSortByTitle, true).Succeeded.Should().BeTrue();
        fixture.CreateService().Get(AxicultusSettingKeys.PowerPointSortByTitle).Should().BeTrue();
    }

    [Fact]
    public void Set_PowerPointSourceListingStyle_PersistsFrmMainListPreviewMode()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.PowerPointSourceListingStyle).Should().Be(-1);
        sut.Set(AxicultusSettingKeys.PowerPointSourceListingStyle, 1).Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.PowerPointSourceListingStyle).Should().Be(1);
        ReadSnapshot(fixture.SettingsPath).PowerPoint.SourceListingStyle.Should().Be(1);

        var invalid = sut.Set(AxicultusSettingKeys.PowerPointSourceListingStyle, 2);

        invalid.Succeeded.Should().BeFalse();
        invalid.Issues.Should().Contain(issue => issue.Key == AxicultusSettingKeys.PowerPointSourceListingStyle.Id);
        sut.Get(AxicultusSettingKeys.PowerPointSourceListingStyle).Should().Be(1);
    }

    [Fact]
    public void Set_PraiseBookCjkGroupStyle_PersistsFrmMainWordCountSortMode()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.PraiseBookCjkGroupStyle).Should().Be(0);
        sut.Set(AxicultusSettingKeys.PraiseBookCjkGroupStyle, 1).Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.PraiseBookCjkGroupStyle).Should().Be(1);
        ReadSnapshot(fixture.SettingsPath).Data.PraiseBookCjkGroupStyle.Should().Be(1);

        var invalid = sut.Set(AxicultusSettingKeys.PraiseBookCjkGroupStyle, 2);

        invalid.Succeeded.Should().BeFalse();
        invalid.Issues.Should().Contain(issue => issue.Key == AxicultusSettingKeys.PraiseBookCjkGroupStyle.Id);
        sut.Get(AxicultusSettingKeys.PraiseBookCjkGroupStyle).Should().Be(1);
    }

    [Fact]
    public void Set_CurrentPraiseBookName_PersistsFrmMainCurrentPraiseBook()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.CurrentPraiseBookName).Should().BeEmpty();
        sut.Set(AxicultusSettingKeys.CurrentPraiseBookName, "주일예배").Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.CurrentPraiseBookName).Should().Be("주일예배");
        ReadSnapshot(fixture.SettingsPath).Data.CurrentPraiseBookName.Should().Be("주일예배");
    }

    [Fact]
    public void Set_CurrentWorshipListName_PersistsFrmMainCurrentSession()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.CurrentWorshipListName).Should().BeEmpty();
        sut.Set(AxicultusSettingKeys.CurrentWorshipListName, "Sunday AM").Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.CurrentWorshipListName).Should().Be("Sunday AM");
        ReadSnapshot(fixture.SettingsPath).Data.CurrentWorshipListName.Should().Be("Sunday AM");
    }

    [Fact]
    public void Set_SelectedSongFolderNo_PersistsFrmMainCurrentSourceFolder()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.SelectedSongFolderNo).Should().Be(0);
        sut.Set(AxicultusSettingKeys.SelectedSongFolderNo, 12).Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.SelectedSongFolderNo).Should().Be(12);
        ReadSnapshot(fixture.SettingsPath).Data.SelectedSongFolderNo.Should().Be(12);

        var invalid = sut.Set(AxicultusSettingKeys.SelectedSongFolderNo, -1);

        invalid.Succeeded.Should().BeFalse();
        invalid.Issues.Should().Contain(issue => issue.Key == AxicultusSettingKeys.SelectedSongFolderNo.Id);
        sut.Get(AxicultusSettingKeys.SelectedSongFolderNo).Should().Be(12);
    }

    [Fact]
    public void RestoreDefaults_RevertsChangedValuesAndPersistsDefaultSnapshot()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.Theme, ColorTheme.Dark).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.InterfaceSize, InterfaceSize.Senior).Succeeded.Should().BeTrue();

        var result = sut.RestoreDefaults();

        result.Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.Theme).Should().Be(ColorTheme.Light);
        sut.Get(AxicultusSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Standard);
        ReadSnapshot(fixture.SettingsPath).Appearance.Should().Be(AxicultusSettingsSnapshot.CreateDefault().Appearance);
    }

    [Fact]
    public async Task ExportImport_WhenSnapshotIsValid_CreatesBackupAndAppliesImportedSettings()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.Theme, ColorTheme.Dark).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.WorkingFolder, fixture.LegacyFolder).Succeeded.Should().BeTrue();
        var importSnapshot = AxicultusSettingsSnapshot.CreateDefault() with
        {
            General = AxicultusSettingsSnapshot.CreateDefault().General with
            {
                Language = "en-US",
                WorkingFolder = fixture.ImportFolder,
            },
            Appearance = AxicultusSettingsSnapshot.CreateDefault().Appearance with
            {
                Theme = ColorTheme.Light,
                InterfaceSize = InterfaceSize.Large,
            },
        };
        var importFile = Path.Combine(fixture.Root, "import.json");
        await File.WriteAllTextAsync(importFile, Serialize(importSnapshot));

        var result = await sut.ImportAsync(importFile);

        result.Succeeded.Should().BeTrue();
        result.BackupPath.Should().NotBeNull();
        File.Exists(result.BackupPath).Should().BeTrue();
        ReadSnapshot(result.BackupPath!).Appearance.Theme.Should().Be(ColorTheme.Dark);
        sut.Get(AxicultusSettingKeys.Language).Should().Be("en-US");
        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(fixture.ImportFolder);
        sut.Get(AxicultusSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Large);
    }

    [Fact]
    public async Task Import_WhenSnapshotInvalid_DoesNotChangeCurrentOrCreateBackup()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.MediaVolume, 0.4).Succeeded.Should().BeTrue();
        var invalid = AxicultusSettingsSnapshot.CreateDefault() with
        {
            General = AxicultusSettingsSnapshot.CreateDefault().General with { Language = "" },
            Media = AxicultusSettingsSnapshot.CreateDefault().Media with { Volume = 2.0 },
        };
        var importFile = Path.Combine(fixture.Root, "invalid.json");
        await File.WriteAllTextAsync(importFile, Serialize(invalid));

        var result = await sut.ImportAsync(importFile);

        result.Succeeded.Should().BeFalse();
        result.BackupPath.Should().BeNull();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
        sut.Get(AxicultusSettingKeys.MediaVolume).Should().Be(0.4);
        sut.Get(AxicultusSettingKeys.Language).Should().Be(AxicultusSettingsSnapshot.CreateDefault().General.Language);
    }

    [Fact]
    public async Task MigrateLegacyAsync_MapsKnownValuesBacksUpAndReportsInvalidEntries()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.Theme, ColorTheme.Light).Succeeded.Should().BeTrue();
        var legacyDefaultMedia = Path.Combine(fixture.Root, "legacy-default.mp4");
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["Language"] = "en-US",
            ["WorkingFolder"] = fixture.LegacyFolder,
            ["Theme"] = "Dark",
            ["InterfaceSize"] = "Senior",
            ["MediaLocation"] = legacyDefaultMedia,
            ["MediaVolume"] = "0.65",
            ["PowerPointRenderTimeoutSeconds"] = "45",
            ["UseSafetyConfirmations"] = "definitely",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        result.BackupPath.Should().NotBeNull();
        result.Issues.Should().Contain(issue =>
            issue.Key == "UseSafetyConfirmations" &&
            issue.Severity == SettingsIssueSeverity.Warning);
        sut.Get(AxicultusSettingKeys.Language).Should().Be("en-US");
        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(fixture.LegacyFolder);
        sut.Get(AxicultusSettingKeys.Theme).Should().Be(ColorTheme.Dark);
        sut.Get(AxicultusSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Senior);
        sut.Get(AxicultusSettingKeys.DefaultMediaPath).Should().Be(legacyDefaultMedia);
        sut.Get(AxicultusSettingKeys.MediaVolume).Should().Be(0.65);
        sut.Get(AxicultusSettingKeys.PowerPointRenderTimeoutSeconds).Should().Be(45);
        sut.Get(AxicultusSettingKeys.UseSafetyConfirmations).Should().BeTrue();
    }

    [Fact]
    public async Task MigrateLegacyAsync_ImportsRegistrationUser()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["RegistrationUser"] = "Grace Church",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.RegistrationUser).Should().Be("Grace Church");
    }

    [Fact]
    public async Task ExportAsync_WritesCurrentSnapshotToRequestedPath()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.EnableDiagnostics, true).Succeeded.Should().BeTrue();
        var exportFile = Path.Combine(fixture.Root, "export", "settings.json");

        var result = await sut.ExportAsync(exportFile);

        result.Succeeded.Should().BeTrue();
        File.Exists(exportFile).Should().BeTrue();
        ReadSnapshot(exportFile).Advanced.EnableDiagnostics.Should().BeTrue();
    }

    [Fact]
    public void Defaults_MatchWinFormsForAlertsAndBibleLimits()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.ReferenceAlertSource).Should().Be(0, "WinForms shows no reference alert by default");
        sut.Get(AxicultusSettingKeys.MessageAlertScroll).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.MessageAlertFlash).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.ParentalAlertScroll).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.ParentalAlertFlash).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.BibleSequentialLimit).Should().Be(500);
        sut.Get(AxicultusSettingKeys.BibleAdHocLimit).Should().Be(200);
        sut.Get(AxicultusSettingKeys.ParentalAlertHeading).Should().Be("보호자 호출");
    }

    [Fact]
    public void Load_KeepsSavedValuesWhenDefaultsChange()
    {
        using var fixture = TempSettingsFolder.Create();
        var saved = AxicultusSettingsSnapshot.CreateDefault();
        saved = saved with
        {
            General = saved.General with { BibleSequentialLimit = 100, MessageAlertFlash = false },
            LiveOutput = saved.LiveOutput with { ReferenceAlertSource = 1 },
        };
        File.WriteAllText(fixture.SettingsPath, Serialize(saved));

        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.BibleSequentialLimit).Should().Be(100);
        sut.Get(AxicultusSettingKeys.MessageAlertFlash).Should().BeFalse();
        sut.Get(AxicultusSettingKeys.ReferenceAlertSource).Should().Be(1);
    }

    [Fact]
    public void Load_WhenOneValueIsOutOfRange_ResetsOnlyThatSettingAndKeepsACopy()
    {
        using var fixture = TempSettingsFolder.Create();
        var saved = AxicultusSettingsSnapshot.CreateDefault();
        saved = saved with
        {
            General = saved.General with { Language = "en-US" },
            Media = saved.Media with { LiveCameraNumber = 99 },
            Operator = saved.Operator with { StageFontPoints = 400, PreviewFontPoints = 12 },
        };
        File.WriteAllText(fixture.SettingsPath, Serialize(saved));

        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.Language).Should().Be("en-US");
        sut.Current.Operator.PreviewFontPoints.Should().Be(12);
        sut.Get(AxicultusSettingKeys.LiveCameraNumber).Should().Be(AxicultusSettingKeys.LiveCameraNumber.DefaultValue);
        sut.Current.Operator.StageFontPoints.Should().Be(new OperatorOptions().StageFontPoints);
        Directory.GetFiles(fixture.Root, "settings.invalid-*.json").Should().ContainSingle();
    }

    [Fact]
    public void Load_WhenAnEnumNameIsUnknown_ResetsOnlyThatSetting()
    {
        using var fixture = TempSettingsFolder.Create();
        var saved = AxicultusSettingsSnapshot.CreateDefault();
        saved = saved with
        {
            General = saved.General with { Language = "en-US" },
            Appearance = saved.Appearance with { Theme = ColorTheme.Dark },
            LiveOutput = saved.LiveOutput with { LyricsMonitorTransitionKind = LyricsTransitionKind.FanUp, LyricsMonitorFontSize = 60 },
        };
        File.WriteAllText(fixture.SettingsPath, Serialize(saved).Replace("\"FanUp\"", "\"FromANewerBuild\""));

        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.LyricsMonitorTransitionKind).Should().Be(AxicultusSettingKeys.LyricsMonitorTransitionKind.DefaultValue);
        sut.Get(AxicultusSettingKeys.Language).Should().Be("en-US");
        sut.Get(AxicultusSettingKeys.Theme).Should().Be(ColorTheme.Dark);
        sut.Get(AxicultusSettingKeys.LyricsMonitorFontSize).Should().Be(60);
        Directory.GetFiles(fixture.Root, "settings.invalid-*.json").Should().ContainSingle();
    }

    [Fact]
    public void Load_WhenATextValueIsNull_ResetsOnlyThatSetting()
    {
        using var fixture = TempSettingsFolder.Create();
        var saved = AxicultusSettingsSnapshot.CreateDefault();
        saved = saved with { General = saved.General with { JumpToA = 7 } };
        File.WriteAllText(fixture.SettingsPath, Serialize(saved)
            .Replace("\"Language\": \"ko-KR\"", "\"Language\": null")
            .Replace("\"ReferenceAlertPickSeparator\": \",\"", "\"ReferenceAlertPickSeparator\": null"));

        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
        sut.Get(AxicultusSettingKeys.Language).Should().Be(AxicultusSettingKeys.Language.DefaultValue);
        sut.Get(AxicultusSettingKeys.ReferenceAlertPickSeparator).Should().Be(AxicultusSettingKeys.ReferenceAlertPickSeparator.DefaultValue, "code that splits on it must never see null");
    }

    [Fact]
    public void Load_WhenTheSchemaVersionIsNotOurs_UsesDefaultsAndKeepsTheFileContent()
    {
        using var fixture = TempSettingsFolder.Create();
        var saved = AxicultusSettingsSnapshot.CreateDefault();
        var json = Serialize(saved with { SchemaVersion = 2, General = saved.General with { JumpToA = 7 } });
        File.WriteAllText(fixture.SettingsPath, json);

        var sut = fixture.CreateService();

        sut.Get(AxicultusSettingKeys.JumpToA).Should().Be(AxicultusSettingKeys.JumpToA.DefaultValue, "a file from another schema is not read key by key");
        File.ReadAllText(Directory.GetFiles(fixture.Root, "settings.invalid-*.json").Should().ContainSingle().Subject)
            .Should().Be(json, "the copy is what the church had, byte for byte");
    }

    [Fact]
    public void Options_WithPersistOff_KeepChangesInMemoryOnly()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = new SettingsService(new SettingsServiceOptions(fixture.SettingsPath, fixture.BackupRoot, Persist: false));

        sut.Set(AxicultusSettingKeys.JumpToA, 7).Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
        File.Exists(fixture.SettingsPath).Should().BeFalse("an earlier settings.json is still waiting to be copied into this place");
    }

    [Fact]
    public void RestoreDefaults_KeepsFoldersSessionStateAndWindowPlacement()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(AxicultusSettingKeys.MediaDirectory, fixture.LegacyFolder).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.PowerPointSourceFolder, fixture.ImportFolder).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.CurrentWorshipListName, "Sunday AM").Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.OnboardingCompleted, true).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.MainWindowWidth, 1200).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.DefaultOutputMonitorId, @"\\.\DISPLAY2").Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.OutputPlacementMode, 1).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.DisplayCustomWidth, 1920).Succeeded.Should().BeTrue();
        sut.SetValues(new Dictionary<string, object> { ["operator.StageWidth"] = 800 }).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.Theme, ColorTheme.Dark).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.Language, "en-US").Succeeded.Should().BeTrue();

        sut.RestoreDefaults().Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.Theme).Should().Be(AxicultusSettingKeys.Theme.DefaultValue);
        sut.Get(AxicultusSettingKeys.Language).Should().Be("en-US", "a reset keeps the menus' language");
        sut.Get(AxicultusSettingKeys.MediaDirectory).Should().Be(fixture.LegacyFolder);
        sut.Get(AxicultusSettingKeys.PowerPointSourceFolder).Should().Be(fixture.ImportFolder);
        sut.Get(AxicultusSettingKeys.CurrentWorshipListName).Should().Be("Sunday AM");
        sut.Get(AxicultusSettingKeys.OnboardingCompleted).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.MainWindowWidth).Should().Be(1200);
        sut.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().Be(@"\\.\DISPLAY2", "a reset must not move the audience window");
        sut.Get(AxicultusSettingKeys.OutputPlacementMode).Should().Be(1);
        sut.Get(AxicultusSettingKeys.DisplayCustomWidth).Should().Be(1920);
        sut.Current.Operator.StageWidth.Should().Be(800);
    }

    [Fact]
    public void Load_WhenFileIsNotJson_UsesDefaults()
    {
        using var fixture = TempSettingsFolder.Create();
        File.WriteAllText(fixture.SettingsPath, "{ \"General\": { \"Language\": \"en-US\"");

        fixture.CreateService().Get(AxicultusSettingKeys.Language).Should().Be(AxicultusSettingKeys.Language.DefaultValue);
    }

    [Fact]
    public void RestoreDefaults_KeepsDataPathsLicencesAndRegistration()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var database = Path.Combine(fixture.LegacyFolder, "AxicultusDb.db");
        sut.Set(AxicultusSettingKeys.WorkingFolder, fixture.LegacyFolder).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.AdminDatabasePath, database).Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.RegistrationUser, "Grace Church").Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.LicenceNumber3, "1234567").Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.LicenceName4, "Publisher").Succeeded.Should().BeTrue();
        sut.Set(AxicultusSettingKeys.Theme, ColorTheme.Dark).Succeeded.Should().BeTrue();

        sut.RestoreDefaults().Succeeded.Should().BeTrue();

        sut.Get(AxicultusSettingKeys.Theme).Should().Be(AxicultusSettingKeys.Theme.DefaultValue);
        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(fixture.LegacyFolder);
        sut.Get(AxicultusSettingKeys.AdminDatabasePath).Should().Be(database);
        sut.Get(AxicultusSettingKeys.RegistrationUser).Should().Be("Grace Church");
        sut.Get(AxicultusSettingKeys.LicenceNumber3).Should().Be("1234567");
        sut.Get(AxicultusSettingKeys.LicenceName4).Should().Be("Publisher");
    }

    private static AxicultusSettingsSnapshot ReadSnapshot(string path)
        => JsonSerializer.Deserialize<AxicultusSettingsSnapshot>(File.ReadAllText(path), SettingsService.JsonOptions)
           ?? throw new InvalidOperationException("Unable to deserialize settings snapshot.");

    private static string Serialize(AxicultusSettingsSnapshot snapshot)
        => JsonSerializer.Serialize(snapshot, SettingsService.JsonOptions);

    [Fact]
    public async Task MigratedToolbarFolderShortcutsSurviveReload()
    {
        using var temp = TempSettingsFolder.Create();
        var settings = temp.CreateService();
        await settings.MigrateLegacyAsync(new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["JumpToA"] = "7",
            ["JumpToB"] = "0",
            ["JumpToC"] = "19",
        }));
        var reopened = temp.CreateService();
        reopened.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
        reopened.Get(AxicultusSettingKeys.JumpToB).Should().Be(2);
        reopened.Get(AxicultusSettingKeys.JumpToC).Should().Be(19);
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            SettingsPath = Path.Combine(root, "settings.json");
            BackupRoot = Path.Combine(root, "Backups");
            LegacyFolder = Path.Combine(root, "LegacyWork");
            ImportFolder = Path.Combine(root, "ImportedWork");
            Directory.CreateDirectory(LegacyFolder);
            Directory.CreateDirectory(ImportFolder);
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public string BackupRoot { get; }

        public string LegacyFolder { get; }

        public string ImportFolder { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_Settings_{Guid.NewGuid():N}"));

        public SettingsService CreateService(string? legacyWorkingFolderPath = null)
            => new(new SettingsServiceOptions(SettingsPath, BackupRoot, legacyWorkingFolderPath));

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
