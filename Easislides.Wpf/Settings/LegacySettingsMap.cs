using System;
using System.Collections.Generic;
using System.Linq;

namespace Easislides.Wpf.Settings;

public enum LegacySettingValueKind
{
    String,
    Boolean,
    Integer,
    Double,
    Enum,
    Color,
    Path,
    Complex,
}

public enum LegacySettingMigrationStatus
{
    Automated,
    DocumentedOnly,
    WpfOnly,
}

public sealed record LegacySettingMapEntry(
    string LegacyKey,
    string? WpfKeyId,
    SettingsSectionKind Section,
    LegacySettingValueKind ValueKind,
    LegacySettingMigrationStatus Status,
    string LegacySource,
    string Notes);

public static class LegacySettingsMap
{
    public const string ShortcutOverridesKeyId = "shortcuts";

    public static IReadOnlyList<LegacySettingMapEntry> Entries { get; } =
    [
        Automated("Language", EasiSettingKeys.Language.Id, SettingsSectionKind.General, LegacySettingValueKind.String, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("WorkingFolder", EasiSettingKeys.WorkingFolder.Id, SettingsSectionKind.General, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("RootEasiSlidesDir", EasiSettingKeys.WorkingFolder.Id, SettingsSectionKind.General, LegacySettingValueKind.Path, "Gf.RootEasiSlidesDir", "Legacy working root after Gf.InitEasiSlidesDir."),
        Automated("root_directory", EasiSettingKeys.WorkingFolder.Id, SettingsSectionKind.General, LegacySettingValueKind.Path, "RegUtil config/root_directory", "Registry-backed legacy working root."),

        Automated("Theme", EasiSettingKeys.Theme.Id, SettingsSectionKind.Appearance, LegacySettingValueKind.Enum, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),
        Automated("InterfaceSize", EasiSettingKeys.InterfaceSize.Id, SettingsSectionKind.Appearance, LegacySettingValueKind.Enum, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),

        Automated("DefaultOutputMonitorId", EasiSettingKeys.DefaultOutputMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("OutputMonitorName", EasiSettingKeys.DefaultOutputMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "FrmOptions.SaveVariables", "Legacy dual-monitor output display name."),
        Automated("UseSafetyConfirmations", EasiSettingKeys.UseSafetyConfirmations.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),

        Automated("PowerPointRenderTimeoutSeconds", EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("ThumbnailCacheMegabytes", EasiSettingKeys.ThumbnailCacheMegabytes.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),

        Automated("MediaVolume", EasiSettingKeys.MediaVolume.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "WPF compatibility", "Current WPF normalized 0..1 media volume."),
        Automated("LiveCamVolume", EasiSettingKeys.MediaVolume.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "FrmOptions.SaveVariables", "Legacy trackbar scale is normalized into 0..1 when greater than 1."),
        Automated("MediaBalance", EasiSettingKeys.MediaBalance.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "WPF compatibility", "Current WPF normalized -1..1 media balance."),
        Automated("LiveCamBalance", EasiSettingKeys.MediaBalance.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "FrmOptions.SaveVariables", "Legacy trackbar scale is normalized into -1..1 when outside that range."),
        Automated("MediaMuted", EasiSettingKeys.MediaMuted.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("LiveCamMute", EasiSettingKeys.MediaMuted.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy live camera mute checkbox."),

        Automated("AdminDatabasePath", EasiSettingKeys.AdminDatabasePath.Id, SettingsSectionKind.Data, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("DBFileName", EasiSettingKeys.AdminDatabasePath.Id, SettingsSectionKind.Data, LegacySettingValueKind.Path, "Gf.DBFileName", "Legacy SQLite database file path."),
        Automated("DataBackupRoot", EasiSettingKeys.DataBackupRoot.Id, SettingsSectionKind.Data, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),

        Automated("EnableDiagnostics", EasiSettingKeys.EnableDiagnostics.Id, SettingsSectionKind.Advanced, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),

        Documented("UsePowerpointTab", SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy tab visibility; needs SettingsWindow PowerPoint section coverage."),
        Documented("NoPowerpointPanelOverlay", SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy live overlay behavior; no WPF setting key yet."),
        Documented("PP_MaxFiles", SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy PowerPoint recent/max files limit; no WPF setting key yet."),
        Documented("UseMediaTab", SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy media tab visibility; no WPF setting key yet."),
        Documented("NoMediaPanelOverlay", SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy media overlay behavior; no WPF setting key yet."),
        Documented("MediaDir", SettingsSectionKind.Media, LegacySettingValueKind.Path, "FrmOptions.SaveVariables", "Legacy music/media directory; needs import/export path section mapping."),
        Documented("ShowLyricsMonitorAlertBox", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy lyrics monitor alert box toggle; no WPF setting key yet."),
        Documented("AdvanceNextItem", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy automatic next-item behavior; no WPF setting key yet."),
        Documented("GapItemOption", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "FrmOptions.SaveVariables", "Legacy gap item behavior; no WPF setting key yet."),
        Documented("GapItemLogoFile", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Path, "FrmOptions.SaveVariables", "Legacy user gap logo path; should join asset migration."),
        Documented("GapItemUseFade", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy gap fade toggle; no WPF setting key yet."),
        Documented("DMAlwaysUseSecondaryMonitor", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy output monitor fallback policy; no WPF setting key yet."),
        Documented("DMOption1Top", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy custom output monitor top coordinate; no WPF setting key yet."),
        Documented("DMOption1Left", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy custom output monitor left coordinate; no WPF setting key yet."),
        Documented("DMOption1Width", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy custom output monitor width; no WPF setting key yet."),
        Documented("LMTextColour", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "FrmOptions.SaveVariables", "Legacy lyrics monitor text color; no WPF setting key yet."),
        Documented("LMBackColour", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "FrmOptions.SaveVariables", "Legacy lyrics monitor background color; no WPF setting key yet."),
        Documented("LMShowNotations", SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy lyrics monitor notation toggle; no WPF setting key yet."),
        Documented("LiveCamNumber", SettingsSectionKind.Media, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy camera device index; no WPF setting key yet."),
        Automated("KeyBoardOption", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy keyboard routing mode mapped into local next/previous shortcut overrides."),
        Automated("GlobalHookKey_F7", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy global hook toggle for F7."),
        Automated("GlobalHookKey_F8", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy global hook toggle for F8."),
        Automated("GlobalHookKey_F9", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy global hook toggle for F9."),
        Automated("GlobalHookKey_F10", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy global hook toggle for F10."),
        Automated("GlobalHookKey_Arrow", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy global hook toggle for arrow keys."),
        Automated("GlobalHookKey_CtrlArrow", ShortcutOverridesKeyId, SettingsSectionKind.Shortcuts, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy global hook toggle for Ctrl+Arrow."),
    ];

    public static IReadOnlyList<string> GetAutomatedAliases(string wpfKeyId)
        => Entries
            .Where(entry =>
                entry.Status == LegacySettingMigrationStatus.Automated &&
                string.Equals(entry.WpfKeyId, wpfKeyId, StringComparison.OrdinalIgnoreCase))
            .Select(entry => entry.LegacyKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static LegacySettingMapEntry Automated(
        string legacyKey,
        string wpfKeyId,
        SettingsSectionKind section,
        LegacySettingValueKind valueKind,
        string legacySource,
        string notes)
        => new(legacyKey, wpfKeyId, section, valueKind, LegacySettingMigrationStatus.Automated, legacySource, notes);

    private static LegacySettingMapEntry Documented(
        string legacyKey,
        SettingsSectionKind section,
        LegacySettingValueKind valueKind,
        string legacySource,
        string notes)
        => new(legacyKey, null, section, valueKind, LegacySettingMigrationStatus.DocumentedOnly, legacySource, notes);
}
