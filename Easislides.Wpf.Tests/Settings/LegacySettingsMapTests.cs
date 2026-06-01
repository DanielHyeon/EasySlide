using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Input;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class LegacySettingsMapTests
{
    [Fact]
    public void AllCurrentSettingKeys_AreRepresentedInLegacyInventory()
    {
        var mappedIds = LegacySettingsMap.Entries
            .Where(entry => entry.WpfKeyId is not null)
            .Select(entry => entry.WpfKeyId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var setting in EasiSettingKeys.All)
        {
            mappedIds.Should().Contain(GetSettingId(setting));
        }
    }

    [Fact]
    public void FrmOptionsSaveVariables_HighRiskKeysAreDocumented()
    {
        var legacyKeys = LegacySettingsMap.Entries
            .Select(entry => entry.LegacyKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        legacyKeys.Should().Contain([
            "UsePowerpointTab",
            "NoPowerpointPanelOverlay",
            "UseMediaTab",
            "ShowLyricsMonitorAlertBox",
            "AdvanceNextItem",
            "GapItemOption",
            "DMAlwaysUseSecondaryMonitor",
            "OutputMonitorName",
            "LMTextColour",
            "LiveCamVolume",
            "MediaDir",
            "GlobalHookKey_F7",
        ]);
    }

    [Fact]
    public void AutomatedAliases_ExposeLegacyAndCompatibilityNames()
    {
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.WorkingFolder.Id)
            .Should().Contain(["WorkingFolder", "RootEasiSlidesDir", "root_directory"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DefaultOutputMonitorId.Id)
            .Should().Contain(["DefaultOutputMonitorId", "OutputMonitorName"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaVolume.Id)
            .Should().Contain(["MediaVolume", "LiveCamVolume"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.RegistrationUser.Id)
            .Should().Contain(["RegistrationUser"]);
    }

    [Fact]
    public void AutomatedAliases_ExposeLegacyOperationalSettingNames()
    {
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.PowerPointMaxFiles.Id)
            .Should().Contain(["PowerPointMaxFiles", "PowerpointMaxFiles", "PP_MaxFiles"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaDirectory.Id)
            .Should().Contain(["MediaDirectory", "MediaDir", "media_dir"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor.Id)
            .Should().Contain(["DMAlwaysUseSecondaryMonitor", "AlwaysTryDualMonitor"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DisplayCustomWidth.Id)
            .Should().Contain(["DMOption1Width", "DualMonitorOptionCustomWidth"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.LyricsMonitorTextColorArgb.Id)
            .Should().Contain(["LMTextColour", "LyricsMonitorTextColour"]);
    }

    [Fact]
    public void AutomatedAliases_ExposeLegacyShortcutKeys()
    {
        LegacySettingsMap.GetAutomatedAliases(LegacySettingsMap.ShortcutOverridesKeyId)
            .Should()
            .Contain([
                "KeyBoardOption",
                "GlobalHookKey_F7",
                "GlobalHookKey_F8",
                "GlobalHookKey_F9",
                "GlobalHookKey_F10",
                "GlobalHookKey_Arrow",
                "GlobalHookKey_CtrlArrow",
            ]);
    }

    [Fact]
    public async Task MigrateLegacyAsync_UsesLegacyAliasesAndNormalizesMediaScales()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = fixture.LegacyFolder,
            ["OutputMonitorName"] = @"\\.\DISPLAY2",
            ["LiveCamVolume"] = "65",
            ["LiveCamBalance"] = "-25",
            ["LiveCamMute"] = "1",
            ["DBFileName"] = Path.Combine(fixture.LegacyFolder, "AdminDB", "EasiSlidesDb.db"),
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Get(EasiSettingKeys.WorkingFolder).Should().Be(fixture.LegacyFolder);
        sut.Get(EasiSettingKeys.DefaultOutputMonitorId).Should().Be(@"\\.\DISPLAY2");
        sut.Get(EasiSettingKeys.MediaVolume).Should().Be(0.65);
        sut.Get(EasiSettingKeys.MediaBalance).Should().Be(-0.25);
        sut.Get(EasiSettingKeys.MediaMuted).Should().BeTrue();
        sut.Get(EasiSettingKeys.AdminDatabasePath).Should().EndWith(Path.Combine("AdminDB", "EasiSlidesDb.db"));
    }

    [Fact]
    public async Task MigrateLegacyAsync_MapsLegacyOperationalSettings()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var mediaFolder = Path.Combine(fixture.LegacyFolder, "Media");
        var logoFile = Path.Combine(fixture.LegacyFolder, "gap.png");
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["UsePowerpointTab"] = "1",
            ["NoPowerpointPanelOverlay"] = "1",
            ["PowerpointMaxFiles"] = "80",
            ["UseMediaTab"] = "1",
            ["NoMediaPanelOverlay"] = "1",
            ["media_dir"] = mediaFolder,
            ["LiveCamNumber"] = "3",
            ["ShowLyricsMonitorAlertBox"] = "1",
            ["AdvanceNextItem"] = "yes",
            ["GapItemOption"] = "2",
            ["GapItemLogoFile"] = logoFile,
            ["GapItemUseFade"] = "0",
            ["AlwaysTryDualMonitor"] = "0",
            ["DualMonitorOptionCustomTop"] = "120",
            ["DualMonitorOptionCustomLeft"] = "-240",
            ["DualMonitorOptionCustomWidth"] = "1920",
            ["LyricsMonitorTextColour"] = "-65536",
            ["LyricsMonitorBackColour"] = "-1",
            ["LyricsMonitorShowNotations"] = "0",
            ["UseSongNumbering"] = "1",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Get(EasiSettingKeys.UseSongNumbering).Should().BeTrue("레거시 별칭이 곡 번호 표시로 마이그레이션");
        sut.Get(EasiSettingKeys.UsePowerPointTab).Should().BeTrue();
        sut.Get(EasiSettingKeys.NoPowerPointPanelOverlay).Should().BeTrue();
        sut.Get(EasiSettingKeys.PowerPointMaxFiles).Should().Be(80);
        sut.Get(EasiSettingKeys.UseMediaTab).Should().BeTrue();
        sut.Get(EasiSettingKeys.NoMediaPanelOverlay).Should().BeTrue();
        sut.Get(EasiSettingKeys.MediaDirectory).Should().Be(mediaFolder);
        sut.Get(EasiSettingKeys.LiveCameraNumber).Should().Be(3);
        sut.Get(EasiSettingKeys.ShowLyricsMonitorAlertBox).Should().BeTrue();
        sut.Get(EasiSettingKeys.AdvanceNextItem).Should().BeTrue();
        sut.Get(EasiSettingKeys.GapItemOption).Should().Be(GapItemMode.Default);
        sut.Get(EasiSettingKeys.GapItemLogoFile).Should().Be(logoFile);
        sut.Get(EasiSettingKeys.GapItemUseFade).Should().BeFalse();
        sut.Get(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor).Should().BeFalse();
        sut.Get(EasiSettingKeys.DisplayCustomTop).Should().Be(120);
        sut.Get(EasiSettingKeys.DisplayCustomLeft).Should().Be(-240);
        sut.Get(EasiSettingKeys.DisplayCustomWidth).Should().Be(1920);
        sut.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(-65536);
        sut.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb).Should().Be(-1);
        sut.Get(EasiSettingKeys.LyricsMonitorShowNotations).Should().BeFalse();
    }

    [Fact]
    public async Task MigrateLegacyAsync_MapsLegacyShortcutOptionsIntoOverrides()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["KeyBoardOption"] = "1",
            ["GlobalHookKey_F7"] = "1",
            ["GlobalHookKey_F9"] = "1",
            ["GlobalHookKey_Arrow"] = "1",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LiveGo),
            "F7"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LiveBlack),
            "F9"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LivePrevious),
            "Up"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LiveNext),
            "Down"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            LocalSlot(MainCommandIds.LivePrevious),
            "PageUp"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            LocalSlot(MainCommandIds.LiveNext),
            "PageDown"));
    }

    [Fact]
    public async Task MigrateLegacyAsync_MapsLegacyShortcutFallbackKeysAndWarnsForInvalidValues()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["KeyBoardOption"] = "7",
            ["GlobalHookKey_F7"] = "0",
            ["GlobalHookKey_F8"] = "1",
            ["GlobalHookKey_F9"] = "0",
            ["GlobalHookKey_F10"] = "1",
            ["GlobalHookKey_CtrlArrow"] = "1",
            ["GlobalHookKey_Arrow"] = "maybe",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        result.Issues.Should().Contain(issue =>
            issue.Key == "KeyBoardOption" &&
            issue.Severity == SettingsIssueSeverity.Warning);
        result.Issues.Should().Contain(issue =>
            issue.Key == "GlobalHookKey_Arrow" &&
            issue.Severity == SettingsIssueSeverity.Warning);
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LiveGo),
            "F8"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LiveBlack),
            "F10"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LivePrevious),
            "Ctrl+Up"));
        sut.Current.Shortcuts.Should().Contain(new KeyValuePair<string, string>(
            GlobalSlot(MainCommandIds.LiveNext),
            "Ctrl+Down"));
        sut.Current.Shortcuts.Should().NotContainKey(LocalSlot(MainCommandIds.LiveNext));
    }

    private static string GlobalSlot(string commandId)
        => ShortcutSettings.GetSlotId(commandId, isGlobal: true);

    private static string LocalSlot(string commandId)
        => ShortcutSettings.GetSlotId(commandId, isGlobal: false);

    private static string GetSettingId(object setting)
        => Convert.ToString(setting.GetType().GetProperty(nameof(EasiSettingKeys.Language.Id))?.GetValue(setting))
           ?? throw new InvalidOperationException("Setting key id is missing.");

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            LegacyFolder = Path.Combine(root, "LegacyRoot");
            Directory.CreateDirectory(LegacyFolder);
        }

        public string Root { get; }

        public string LegacyFolder { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_LegacySettings_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(
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
