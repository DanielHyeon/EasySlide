using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

public class LegacySettingsMapTests
{
    [Fact]
    public void AllCurrentSettingKeys_AreRepresentedInLegacyInventory()
    {
        var mappedIds = LegacySettingsMap.Entries
            .Where(entry => entry.WpfKeyId is not null)
            .Select(entry => entry.WpfKeyId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var setting in AxicultusSettingKeys.All)
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
            "ReferenceAlertSource",
            "ReferenceAlertUsePick",
            "ReferenceAlertBlankIfPickNotFound",
            "ReferenceAlertPickName",
            "ReferenceAlertPickSubstitute",
            "ReferenceAlertPickSeparator",
            "ReferenceAlertDuration",
            "ReferenceAlertStyle",
            "AdvanceNextItem",
            "GapItemOption",
            "DMAlwaysUseSecondaryMonitor",
            "OutputMonitorName",
            "LyricsMonitorTextColour",
            "LiveCamVolume",
            "MediaDir",
            "GlobalHookKey_F7",
        ]);
    }

    [Fact]
    public void AutomatedAliases_ExposeLegacyAndCompatibilityNames()
    {
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.WorkingFolder.Id)
            .Should().Contain(["WorkingFolder", "RootAxicultusDir", "root_directory"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.DefaultOutputMonitorId.Id)
            .Should().Contain(["DefaultOutputMonitorId", "OutputMonitorName"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.MediaVolume.Id)
            .Should().Contain(["MediaVolume"]).And.NotContain(["LiveCamVolume"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.RegistrationUser.Id)
            .Should().Contain(["RegistrationUser"]);
    }

    [Fact]
    public void AutomatedAliases_ExposeLegacyOperationalSettingNames()
    {
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.PowerPointMaxFiles.Id)
            .Should().Contain(["PowerPointMaxFiles", "PowerpointMaxFiles", "PP_MaxFiles"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.PowerPointSourceListingStyle.Id)
            .Should().Contain(["PowerPointSourceListingStyle", "ExternalListing"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.PraiseBookCjkGroupStyle.Id)
            .Should().Contain(["PraiseBookCjkGroupStyle", "PB_CJKGroupStyle"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.CurrentPraiseBookName.Id)
            .Should().Contain(["CurrentPraiseBookName", "current_praisebook", "CurPraiseBook"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.CurrentWorshipListName.Id)
            .Should().Contain(["CurrentWorshipListName", "current_session", "CurSession"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.SelectedSongFolderNo.Id)
            .Should().Contain(["SelectedSongFolderNo", "CurMainSelectedFolder"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.MediaDirectory.Id)
            .Should().Contain(["MediaDirectory", "MediaDir", "media_dir"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.DisplayAlwaysUseSecondaryMonitor.Id)
            .Should().Contain(["DMAlwaysUseSecondaryMonitor", "AlwaysTryDualMonitor"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.DisplayCustomWidth.Id)
            .Should().Contain(["DMOption1Width", "DualMonitorOptionCustomWidth"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertDurationSeconds.Id)
            .Should().Contain(["ReferenceAlertDuration", "ReferenceAlertDurationSeconds"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertUsePick.Id)
            .Should().Contain(["ReferenceAlertUsePick"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertBlankIfPickNotFound.Id)
            .Should().Contain(["ReferenceAlertBlankIfPickNotFound"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertPickName.Id)
            .Should().Contain(["ReferenceAlertPickName"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertPickSubstitute.Id)
            .Should().Contain(["ReferenceAlertPickSubstitute"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertPickSeparator.Id)
            .Should().Contain(["ReferenceAlertPickSeparator"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertScroll.Id)
            .Should().Contain(["ReferenceAlertStyle", "ReferenceAlertScroll"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertFlash.Id)
            .Should().Contain(["ReferenceAlertStyle", "ReferenceAlertFlash"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.ReferenceAlertTransparent.Id)
            .Should().Contain(["ReferenceAlertStyle", "ReferenceAlertTransparent"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.LyricsMonitorTextColorArgb.Id)
            .Should().NotContain(["LMTextColour", "LyricsMonitorTextColour"]);
        LegacySettingsMap.GetAutomatedAliases(AxicultusSettingKeys.LyricsMonitorHighlightColorArgb.Id)
            .Should().Contain(["LMHighlightColour", "LyricsMonitorHighlightColour"]);
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
    public async Task MigrateLegacyAsync_UsesLegacyAliasesAndKeepsCameraValuesOffMediaSettings()
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
            ["DBFileName"] = Path.Combine(fixture.LegacyFolder, "AdminDB", "AxicultusDb.db"),
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(fixture.LegacyFolder);
        sut.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().Be(@"\\.\DISPLAY2");
        sut.Current.Operator.CameraVolume.Should().Be(65);
        sut.Current.Operator.CameraBalance.Should().Be(-25);
        sut.Current.Operator.CameraMuted.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.MediaVolume).Should().Be(AxicultusSettingKeys.MediaVolume.DefaultValue);
        sut.Get(AxicultusSettingKeys.MediaBalance).Should().Be(AxicultusSettingKeys.MediaBalance.DefaultValue);
        sut.Get(AxicultusSettingKeys.MediaMuted).Should().BeFalse("a muted live camera must not mute all media");
        sut.Get(AxicultusSettingKeys.AdminDatabasePath).Should().EndWith(Path.Combine("AdminDB", "AxicultusDb.db"));
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
            ["ExternalListing"] = "1",
            ["PB_CJKGroupStyle"] = "1",
            ["current_praisebook"] = "주일예배",
            ["current_session"] = "Sunday AM",
            ["CurMainSelectedFolder"] = "2",
            ["UseMediaTab"] = "1",
            ["NoMediaPanelOverlay"] = "1",
            ["media_dir"] = mediaFolder,
            ["LiveCamNumber"] = "3",
            ["ShowLyricsMonitorAlertBox"] = "1",
            ["ReferenceAlertSource"] = "2",
            ["ReferenceAlertUsePick"] = "1",
            ["ReferenceAlertBlankIfPickNotFound"] = "1",
            ["ReferenceAlertPickName"] = "No.",
            ["ReferenceAlertPickSubstitute"] = "#",
            ["ReferenceAlertPickSeparator"] = "/,",
            ["ReferenceAlertDuration"] = "7",
            ["ReferenceAlertStyle"] = "6",
            ["AdvanceNextItem"] = "yes",
            ["GapItemOption"] = "2",
            ["GapItemLogoFile"] = logoFile,
            ["GapItemUseFade"] = "0",
            ["AlwaysTryDualMonitor"] = "0",
            ["DualMonitorOptionCustomTop"] = "120",
            ["DualMonitorOptionCustomLeft"] = "-240",
            ["DualMonitorOptionCustomWidth"] = "1920",
            ["LyricsMonitorTextColour"] = "-256",
            ["LyricsMonitorHighlightColour"] = "-16711936",
            ["LyricsMonitorBackColour"] = "-16777216",
            ["LyricsMonitorShowNotations"] = "1",
            ["UseSongNumbering"] = "1",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.UseSongNumbering).Should().BeTrue("레거시 별칭이 곡 번호 표시로 마이그레이션");
        sut.Get(AxicultusSettingKeys.UsePowerPointTab).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.NoPowerPointPanelOverlay).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.PowerPointMaxFiles).Should().Be(80);
        sut.Get(AxicultusSettingKeys.PowerPointSourceListingStyle).Should().Be(1);
        sut.Get(AxicultusSettingKeys.PraiseBookCjkGroupStyle).Should().Be(1);
        sut.Get(AxicultusSettingKeys.CurrentPraiseBookName).Should().Be("주일예배");
        sut.Get(AxicultusSettingKeys.CurrentWorshipListName).Should().Be("Sunday AM");
        sut.Get(AxicultusSettingKeys.SelectedSongFolderNo).Should().Be(2);
        sut.Get(AxicultusSettingKeys.UseMediaTab).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.NoMediaPanelOverlay).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.MediaDirectory).Should().Be(mediaFolder);
        sut.Get(AxicultusSettingKeys.LiveCameraNumber).Should().Be(3);
        sut.Get(AxicultusSettingKeys.ShowLyricsMonitorAlertBox).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.ReferenceAlertSource).Should().Be(2);
        sut.Get(AxicultusSettingKeys.ReferenceAlertUsePick).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.ReferenceAlertBlankIfPickNotFound).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.ReferenceAlertPickName).Should().Be("No.");
        sut.Get(AxicultusSettingKeys.ReferenceAlertPickSubstitute).Should().Be("#");
        sut.Get(AxicultusSettingKeys.ReferenceAlertPickSeparator).Should().Be("/,");
        sut.Get(AxicultusSettingKeys.ReferenceAlertDurationSeconds).Should().Be(7);
        sut.Get(AxicultusSettingKeys.ReferenceAlertScroll).Should().BeFalse();
        sut.Get(AxicultusSettingKeys.ReferenceAlertFlash).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.ReferenceAlertTransparent).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.AdvanceNextItem).Should().BeTrue();
        sut.Get(AxicultusSettingKeys.GapItemOption).Should().Be(GapItemMode.Default);
        sut.Get(AxicultusSettingKeys.GapItemLogoFile).Should().Be(logoFile);
        sut.Get(AxicultusSettingKeys.GapItemUseFade).Should().BeFalse();
        sut.Get(AxicultusSettingKeys.DisplayAlwaysUseSecondaryMonitor).Should().BeFalse();
        sut.Get(AxicultusSettingKeys.DisplayCustomTop).Should().Be(120);
        sut.Get(AxicultusSettingKeys.DisplayCustomLeft).Should().Be(-240);
        sut.Get(AxicultusSettingKeys.DisplayCustomWidth).Should().Be(1920);
        // The WinForms lyrics monitor is the stage monitor: its colours and chords stay off the congregation output.
        sut.Current.Operator.StageTextArgb.Should().Be(-256);
        sut.Current.Operator.StageBackgroundArgb.Should().Be(-16777216);
        sut.Current.Operator.StageShowNotations.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.LyricsMonitorTextColorArgb).Should().Be(AxicultusSettingKeys.LyricsMonitorTextColorArgb.DefaultValue);
        sut.Get(AxicultusSettingKeys.LyricsMonitorHighlightColorArgb).Should().Be(unchecked((int)0xFF00FF00));
        sut.Get(AxicultusSettingKeys.LyricsMonitorBackgroundColorArgb).Should().Be(AxicultusSettingKeys.LyricsMonitorBackgroundColorArgb.DefaultValue);
        sut.Get(AxicultusSettingKeys.LyricsMonitorShowNotations).Should().BeFalse();
    }

    [Fact]
    public async Task MigrateLegacyAsync_RepairsOutOfRangeValuesInsteadOfFailing()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = "",
            ["OutputmonitorName"] = "Auto",
            ["LiveCamNumber"] = "0",
            ["PowerpointMaxFiles"] = "500",
            ["ExternalListing"] = "7",
            ["ReferenceAlertDuration"] = "0",
            ["ReferenceAlertSource"] = "9",
            ["DualMonitorOptionCustomWidth"] = "0",
            ["JumpToA"] = "7",
            ["MessageAlertFontSize"] = "12",
            ["LyricsMonitorFontSize"] = "60",
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue("WinForms repairs these values on load; one of them must not discard the import");
        sut.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(AxicultusSettingKeys.WorkingFolder.DefaultValue);
        sut.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().BeEmpty();
        sut.Get(AxicultusSettingKeys.LiveCameraNumber).Should().Be(1);
        sut.Get(AxicultusSettingKeys.PowerPointMaxFiles).Should().Be(100);
        sut.Get(AxicultusSettingKeys.PowerPointSourceListingStyle).Should().Be(0);
        sut.Get(AxicultusSettingKeys.ReferenceAlertDurationSeconds).Should().Be(30);
        sut.Get(AxicultusSettingKeys.ReferenceAlertSource).Should().Be(0);
        sut.Get(AxicultusSettingKeys.DisplayCustomWidth).Should().Be(100);
        sut.Current.Operator.Message.Points.Should().Be(25, "WinForms shows 25 for an alert font size outside 20..50");
        sut.Current.Operator.StageFontPoints.Should().Be(20, "WinForms shows 20 for a stage font size outside 8..40");
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
            GlobalSlot(MainCommandIds.LivePreviewToOutputClearBlack),
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
            GlobalSlot(MainCommandIds.LivePreviewToOutput),
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
        => Convert.ToString(setting.GetType().GetProperty(nameof(AxicultusSettingKeys.Language.Id))?.GetValue(setting))
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
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_LegacySettings_{Guid.NewGuid():N}"));

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
