using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class SettingsServiceTests
{
    [Fact]
    public void Set_WithValidTypedKey_PersistsAndRaisesChangedEvent()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var raised = new List<SettingsChangedEventArgs>();
        sut.SettingsChanged += (_, args) => raised.Add(args);

        var result = sut.Set(EasiSettingKeys.MediaVolume, 0.72);

        result.Succeeded.Should().BeTrue();
        sut.Get(EasiSettingKeys.MediaVolume).Should().Be(0.72);
        File.Exists(fixture.SettingsPath).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).Media.Volume.Should().Be(0.72);
        raised.Should().ContainSingle();
        raised[0].ChangedKeys.Should().Contain(EasiSettingKeys.MediaVolume.Id);
        raised[0].Source.Should().Be(SettingsChangeSource.User);
    }

    [Fact]
    public void Set_LyricsTextAlignment_PersistsEnumToDiskAndReloads()
    {
        // 인-셸 가사 정렬 enum 의 디스크 round-trip 고정(JsonStringEnumConverter 역직렬화 경로 검증).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorTextAlignment, LyricsTextAlignment.Right).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorTextAlignment).Should().Be(LyricsTextAlignment.Right);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorTextAlignment.Should().Be(LyricsTextAlignment.Right);
    }

    [Fact]
    public void Set_LyricsVerticalAlignment_PersistsEnumToDiskAndReloads()
    {
        // 세로 정렬 enum 디스크 round-trip 고정.
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorVerticalAlignment, LyricsVerticalAlignment.Bottom).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorVerticalAlignment).Should().Be(LyricsVerticalAlignment.Bottom);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorVerticalAlignment.Should().Be(LyricsVerticalAlignment.Bottom);
    }

    [Fact]
    public void Set_LyricsMonitorShowCopyright_PersistsToDiskAndReloads()
    {
        // 저작권 표시 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증, Display Panel).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorShowCopyright, true).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorShowCopyright).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorShowCopyright.Should().BeTrue();
    }

    [Fact]
    public void Default_LyricsMonitorBackgroundMode_IsFill()
    {
        // 배경 표시 모드 기본값은 Fill(=기존 UniformToFill) — 끄면 기존 출력과 동일(무회귀).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(EasiSettingKeys.LyricsMonitorBackgroundMode).Should().Be(LyricsBackgroundMode.Fill);
    }

    [Fact]
    public void Set_LyricsMonitorBackgroundMode_PersistsEnumToDiskAndReloads()
    {
        // 배경 표시 모드 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorBackgroundMode, LyricsBackgroundMode.Tile).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorBackgroundMode).Should().Be(LyricsBackgroundMode.Tile);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorBackgroundMode.Should().Be(LyricsBackgroundMode.Tile);
    }

    [Fact]
    public void Default_LyricsMonitorShowNotations_IsOff()
    {
        // 코드 표시 기본값은 off — 회중 화면은 예부터 코드를 숨겼고, 끄면 본문이 비트 동일(무회귀)이다.
        // (기본 on 이면 모든 기존 곡이 첫 실행부터 코드를 송출하는 회귀가 생긴다 — 이 핀으로 재발 방지.)
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Get(EasiSettingKeys.LyricsMonitorShowNotations).Should().BeFalse();
    }

    [Fact]
    public void Set_LyricsMonitorShowNotations_PersistsToDiskAndReloads()
    {
        // 코드 표시 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorShowNotations).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorShowNotations.Should().BeTrue();
    }

    [Fact]
    public void Set_LyricsMonitorShowNextItem_PersistsToDiskAndReloads()
    {
        // 다음 항목 표시 토글의 디스크 round-trip 고정(GET/SET 스위치 케이스 검증, Display Panel PrevNext).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorShowNextItem, true).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorShowNextItem).Should().BeTrue();
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorShowNextItem.Should().BeTrue();
    }

    [Fact]
    public void Set_LyricsMonitorTransitionKind_PersistsEnumToDiskAndReloads()
    {
        // 전환 모션 종류(enum)의 디스크 round-trip 고정(JsonStringEnumConverter 경로 + GET/SET 스위치).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorTransitionKind, LyricsTransitionKind.ZoomIn).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorTransitionKind).Should().Be(LyricsTransitionKind.ZoomIn);
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorTransitionKind.Should().Be(LyricsTransitionKind.ZoomIn);
    }

    [Fact]
    public void Set_LyricsMonitorTransitionSettings_PersistToDiskAndReload()
    {
        // 전환 페이드 사용 여부(bool) + 길이(int)의 디스크 round-trip 고정(GET/SET 스위치 검증, FrmMain 전환 효과).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorUseFadeTransition, false).Succeeded.Should().BeTrue();
        sut.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, 500).Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorUseFadeTransition).Should().BeFalse();
        sut.Get(EasiSettingKeys.LyricsMonitorTransitionDurationMs).Should().Be(500);
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

        sut.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, -50).Succeeded.Should().BeFalse();
        sut.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, 9000).Succeeded.Should().BeFalse();

        sut.Get(EasiSettingKeys.LyricsMonitorTransitionDurationMs).Should().Be(250);
    }

    [Fact]
    public void Set_LyricsMonitorBackgroundImagePath_PersistsToDiskAndReloads()
    {
        // 전역 출력 배경 이미지 경로(string)의 디스크 round-trip 고정(GET/SET 스위치 검증, FrmMain Images 탭).
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();

        sut.Set(EasiSettingKeys.LyricsMonitorBackgroundImagePath, @"C:\bg\global.png").Succeeded.Should().BeTrue();

        sut.Get(EasiSettingKeys.LyricsMonitorBackgroundImagePath).Should().Be(@"C:\bg\global.png");
        ReadSnapshot(fixture.SettingsPath).LiveOutput.LyricsMonitorBackgroundImagePath.Should().Be(@"C:\bg\global.png");
    }

    [Fact]
    public void Set_WhenValueIsInvalid_ReturnsIssueAndKeepsPreviousValue()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(EasiSettingKeys.PowerPointRenderTimeoutSeconds, 30).Succeeded.Should().BeTrue();

        var result = sut.Set(EasiSettingKeys.PowerPointRenderTimeoutSeconds, 0);

        result.Succeeded.Should().BeFalse();
        result.Issues.Should().Contain(issue => issue.Key == EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id);
        sut.Get(EasiSettingKeys.PowerPointRenderTimeoutSeconds).Should().Be(30);
        ReadSnapshot(fixture.SettingsPath).PowerPoint.RenderTimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void RestoreDefaults_RevertsChangedValuesAndPersistsDefaultSnapshot()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(EasiSettingKeys.Theme, ColorTheme.Dark).Succeeded.Should().BeTrue();
        sut.Set(EasiSettingKeys.InterfaceSize, InterfaceSize.Senior).Succeeded.Should().BeTrue();

        var result = sut.RestoreDefaults();

        result.Succeeded.Should().BeTrue();
        sut.Get(EasiSettingKeys.Theme).Should().Be(ColorTheme.Light);
        sut.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Standard);
        ReadSnapshot(fixture.SettingsPath).Appearance.Should().Be(EasiSettingsSnapshot.CreateDefault().Appearance);
    }

    [Fact]
    public async Task ExportImport_WhenSnapshotIsValid_CreatesBackupAndAppliesImportedSettings()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(EasiSettingKeys.Theme, ColorTheme.Dark).Succeeded.Should().BeTrue();
        sut.Set(EasiSettingKeys.WorkingFolder, fixture.LegacyFolder).Succeeded.Should().BeTrue();
        var importSnapshot = EasiSettingsSnapshot.CreateDefault() with
        {
            General = EasiSettingsSnapshot.CreateDefault().General with
            {
                Language = "en-US",
                WorkingFolder = fixture.ImportFolder,
            },
            Appearance = EasiSettingsSnapshot.CreateDefault().Appearance with
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
        sut.Get(EasiSettingKeys.Language).Should().Be("en-US");
        sut.Get(EasiSettingKeys.WorkingFolder).Should().Be(fixture.ImportFolder);
        sut.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Large);
    }

    [Fact]
    public async Task Import_WhenSnapshotInvalid_DoesNotChangeCurrentOrCreateBackup()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(EasiSettingKeys.MediaVolume, 0.4).Succeeded.Should().BeTrue();
        var invalid = EasiSettingsSnapshot.CreateDefault() with
        {
            General = EasiSettingsSnapshot.CreateDefault().General with { Language = "" },
            Media = EasiSettingsSnapshot.CreateDefault().Media with { Volume = 2.0 },
        };
        var importFile = Path.Combine(fixture.Root, "invalid.json");
        await File.WriteAllTextAsync(importFile, Serialize(invalid));

        var result = await sut.ImportAsync(importFile);

        result.Succeeded.Should().BeFalse();
        result.BackupPath.Should().BeNull();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
        sut.Get(EasiSettingKeys.MediaVolume).Should().Be(0.4);
        sut.Get(EasiSettingKeys.Language).Should().Be(EasiSettingsSnapshot.CreateDefault().General.Language);
    }

    [Fact]
    public async Task MigrateLegacyAsync_MapsKnownValuesBacksUpAndReportsInvalidEntries()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(EasiSettingKeys.Theme, ColorTheme.Light).Succeeded.Should().BeTrue();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["Language"] = "en-US",
            ["WorkingFolder"] = fixture.LegacyFolder,
            ["Theme"] = "Dark",
            ["InterfaceSize"] = "Senior",
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
        sut.Get(EasiSettingKeys.Language).Should().Be("en-US");
        sut.Get(EasiSettingKeys.WorkingFolder).Should().Be(fixture.LegacyFolder);
        sut.Get(EasiSettingKeys.Theme).Should().Be(ColorTheme.Dark);
        sut.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Senior);
        sut.Get(EasiSettingKeys.MediaVolume).Should().Be(0.65);
        sut.Get(EasiSettingKeys.PowerPointRenderTimeoutSeconds).Should().Be(45);
        sut.Get(EasiSettingKeys.UseSafetyConfirmations).Should().BeTrue();
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
        sut.Get(EasiSettingKeys.RegistrationUser).Should().Be("Grace Church");
    }

    [Fact]
    public async Task ExportAsync_WritesCurrentSnapshotToRequestedPath()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        sut.Set(EasiSettingKeys.EnableDiagnostics, true).Succeeded.Should().BeTrue();
        var exportFile = Path.Combine(fixture.Root, "export", "settings.json");

        var result = await sut.ExportAsync(exportFile);

        result.Succeeded.Should().BeTrue();
        File.Exists(exportFile).Should().BeTrue();
        ReadSnapshot(exportFile).Advanced.EnableDiagnostics.Should().BeTrue();
    }

    private static EasiSettingsSnapshot ReadSnapshot(string path)
        => JsonSerializer.Deserialize<EasiSettingsSnapshot>(File.ReadAllText(path), SettingsService.JsonOptions)
           ?? throw new InvalidOperationException("Unable to deserialize settings snapshot.");

    private static string Serialize(EasiSettingsSnapshot snapshot)
        => JsonSerializer.Serialize(snapshot, SettingsService.JsonOptions);

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
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_Settings_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(SettingsPath, BackupRoot));

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
