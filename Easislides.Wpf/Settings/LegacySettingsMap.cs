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
        Automated("OnboardingCompleted", EasiSettingKeys.OnboardingCompleted.Id, SettingsSectionKind.General, LegacySettingValueKind.Boolean, "WPF compatibility", "First-run welcome completion marker."),
        Automated("RegistrationUser", EasiSettingKeys.RegistrationUser.Id, SettingsSectionKind.General, LegacySettingValueKind.String, "RegUtil config/RegistrationUser", "Name displayed at startup and About dialog registration field."),

        Automated("Theme", EasiSettingKeys.Theme.Id, SettingsSectionKind.Appearance, LegacySettingValueKind.Enum, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),
        Automated("InterfaceSize", EasiSettingKeys.InterfaceSize.Id, SettingsSectionKind.Appearance, LegacySettingValueKind.Enum, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),

        Automated("DefaultOutputMonitorId", EasiSettingKeys.DefaultOutputMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("OutputMonitorName", EasiSettingKeys.DefaultOutputMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "FrmOptions.SaveVariables", "Legacy dual-monitor output display name."),
        Automated("UseSafetyConfirmations", EasiSettingKeys.UseSafetyConfirmations.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),
        Automated("ShowLyricsMonitorAlertBox", EasiSettingKeys.ShowLyricsMonitorAlertBox.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy lyrics monitor alert box toggle."),
        Automated("AdvanceNextItem", EasiSettingKeys.AdvanceNextItem.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy automatic next-item behavior."),
        Automated("GapItemOption", EasiSettingKeys.GapItemOption.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "FrmOptions.SaveVariables", "Legacy gap item behavior."),
        Automated("GapItemLogoFile", EasiSettingKeys.GapItemLogoFile.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Path, "FrmOptions.SaveVariables", "Legacy user gap logo path."),
        Automated("GapItemUseFade", EasiSettingKeys.GapItemUseFade.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy gap fade toggle."),
        Automated("DisplayAlwaysUseSecondaryMonitor", EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("DMAlwaysUseSecondaryMonitor", EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy output monitor fallback policy."),
        Automated("AlwaysTryDualMonitor", EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "RegUtil monitors/options", "Registry-backed dual monitor fallback policy."),
        Automated("DisplayCustomTop", EasiSettingKeys.DisplayCustomTop.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("DMOption1Top", EasiSettingKeys.DisplayCustomTop.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy custom output monitor top coordinate."),
        Automated("DualMonitorOptionCustomTop", EasiSettingKeys.DisplayCustomTop.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "RegUtil monitors/options", "Registry-backed custom output monitor top coordinate."),
        Automated("DisplayCustomLeft", EasiSettingKeys.DisplayCustomLeft.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("DMOption1Left", EasiSettingKeys.DisplayCustomLeft.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy custom output monitor left coordinate."),
        Automated("DualMonitorOptionCustomLeft", EasiSettingKeys.DisplayCustomLeft.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "RegUtil monitors/options", "Registry-backed custom output monitor left coordinate."),
        Automated("DisplayCustomWidth", EasiSettingKeys.DisplayCustomWidth.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("DMOption1Width", EasiSettingKeys.DisplayCustomWidth.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy custom output monitor width."),
        Automated("DualMonitorOptionCustomWidth", EasiSettingKeys.DisplayCustomWidth.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "RegUtil monitors/options", "Registry-backed custom output monitor width."),
        Automated("LyricsMonitorTextColorArgb", EasiSettingKeys.LyricsMonitorTextColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "WPF compatibility", "Current WPF ARGB text color."),
        Automated("LMTextColour", EasiSettingKeys.LyricsMonitorTextColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "FrmOptions.SaveVariables", "Legacy lyrics monitor text color."),
        Automated("LyricsMonitorTextColour", EasiSettingKeys.LyricsMonitorTextColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "RegUtil monitors", "Registry-backed lyrics monitor text color."),
        Automated("LyricsMonitorBackgroundColorArgb", EasiSettingKeys.LyricsMonitorBackgroundColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "WPF compatibility", "Current WPF ARGB background color."),
        Automated("LMBackColour", EasiSettingKeys.LyricsMonitorBackgroundColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "FrmOptions.SaveVariables", "Legacy lyrics monitor background color."),
        Automated("LyricsMonitorBackColour", EasiSettingKeys.LyricsMonitorBackgroundColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "RegUtil monitors", "Registry-backed lyrics monitor background color."),
        Automated("LyricsMonitorBackgroundColor2Argb", EasiSettingKeys.LyricsMonitorBackgroundColor2Argb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "WPF compatibility", "WPF ARGB background gradient end color (G2 / FrmBackground)."),
        Automated("LyricsMonitorBackgroundIsGradient", EasiSettingKeys.LyricsMonitorBackgroundIsGradient.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Whether the lyrics monitor background uses a vertical gradient (G2 / FrmBackground)."),
        Automated("LyricsMonitorShowNotations", EasiSettingKeys.LyricsMonitorShowNotations.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "RegUtil monitors", "Registry-backed lyrics notation toggle."),
        Automated("LMShowNotations", EasiSettingKeys.LyricsMonitorShowNotations.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy lyrics monitor notation toggle."),
        Automated("LyricsMonitorTextAlignment", EasiSettingKeys.LyricsMonitorTextAlignment.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "In-shell lyrics horizontal alignment (left/center/right) — §7.3-A."),
        Automated("LyricsMonitorVerticalAlignment", EasiSettingKeys.LyricsMonitorVerticalAlignment.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "In-shell lyrics vertical alignment (top/center/bottom) — §7.3-A."),
        Automated("LyricsMonitorFontSize", EasiSettingKeys.LyricsMonitorFontSize.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "In-shell lyrics font size in px (24-120) — §7.3-A."),
        Automated("LyricsMonitorBold", EasiSettingKeys.LyricsMonitorBold.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell lyrics bold toggle — §7.3-A font effects."),
        Automated("LyricsMonitorItalic", EasiSettingKeys.LyricsMonitorItalic.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell lyrics italic toggle — §7.3-A font effects."),
        Automated("LyricsMonitorShadow", EasiSettingKeys.LyricsMonitorShadow.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell lyrics drop-shadow toggle — §7.3-A font effects."),
        Automated("LyricsMonitorLineSpacingPercent", EasiSettingKeys.LyricsMonitorLineSpacingPercent.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "In-shell lyrics line spacing as % of font size (100-220) — §7.3-A."),
        Automated("LyricsMonitorShowPositionIndicator", EasiSettingKeys.LyricsMonitorShowPositionIndicator.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell verse/slide position indicator (N/M) toggle — §7.3-A."),
        Automated("LyricsMonitorShowItemNumber", EasiSettingKeys.LyricsMonitorShowItemNumber.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel song/item number toggle — FrmMain Show Item Number."),
        Automated("LyricsMonitorShowCopyright", EasiSettingKeys.LyricsMonitorShowCopyright.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel copyright toggle — FrmMain Show Copyright Information."),
        Automated("LyricsMonitorShowNextItem", EasiSettingKeys.LyricsMonitorShowNextItem.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel next-item preview toggle — FrmMain Display Panel PrevNext."),
        Automated("LyricsMonitorUseFadeTransition", EasiSettingKeys.LyricsMonitorUseFadeTransition.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Output scene fade transition toggle — FrmMain transition effect (fade only)."),
        Automated("LyricsMonitorTransitionDurationMs", EasiSettingKeys.LyricsMonitorTransitionDurationMs.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Output fade transition duration in ms (0-2000) — FrmMain transition timing."),
        Automated("LyricsMonitorShowTitleHeading", EasiSettingKeys.LyricsMonitorShowTitleHeading.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell title heading toggle (song title banner above lyrics) — §7.3-A."),
        Automated("LyricsMonitorOutline", EasiSettingKeys.LyricsMonitorOutline.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell lyrics outline font toggle (stroked glyphs for legibility) — §7.3-A."),
        Automated("LyricsMonitorTitleHeadingAlignment", EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "In-shell title heading horizontal alignment (left/center/right) — §7.3-A heading align."),
        Automated("LyricsMonitorTitleHeadingFirstScreenOnly", EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell title heading shown only on first verse — §7.3-A heading at first screen only."),
        Automated("AutoRotateIntervalSeconds", EasiSettingKeys.AutoRotateIntervalSeconds.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Auto-rotate verse/slide interval in seconds (2-600) — §7.3-B."),

        Automated("UsePowerpointTab", EasiSettingKeys.UsePowerPointTab.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy tab visibility."),
        Automated("UsePowerPointTab", EasiSettingKeys.UsePowerPointTab.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("NoPowerpointPanelOverlay", EasiSettingKeys.NoPowerPointPanelOverlay.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy live overlay behavior."),
        Automated("NoPowerPointPanelOverlay", EasiSettingKeys.NoPowerPointPanelOverlay.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("PowerPointRenderTimeoutSeconds", EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("ThumbnailCacheMegabytes", EasiSettingKeys.ThumbnailCacheMegabytes.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("PowerPointMaxFiles", EasiSettingKeys.PowerPointMaxFiles.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("PowerpointMaxFiles", EasiSettingKeys.PowerPointMaxFiles.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "RegUtil options", "Registry-backed legacy PowerPoint max files limit."),
        Automated("PP_MaxFiles", EasiSettingKeys.PowerPointMaxFiles.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy PowerPoint recent/max files limit."),

        Automated("UseMediaTab", EasiSettingKeys.UseMediaTab.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy media tab visibility."),
        Automated("NoMediaPanelOverlay", EasiSettingKeys.NoMediaPanelOverlay.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy media overlay behavior."),
        Automated("MediaDirectory", EasiSettingKeys.MediaDirectory.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("MediaDir", EasiSettingKeys.MediaDirectory.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "FrmOptions.SaveVariables", "Legacy music/media directory."),
        Automated("media_dir", EasiSettingKeys.MediaDirectory.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "RegUtil config/media_dir", "Registry-backed legacy media directory."),
        Automated("MediaVolume", EasiSettingKeys.MediaVolume.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "WPF compatibility", "Current WPF normalized 0..1 media volume."),
        Automated("LiveCamVolume", EasiSettingKeys.MediaVolume.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "FrmOptions.SaveVariables", "Legacy trackbar scale is normalized into 0..1 when greater than 1."),
        Automated("MediaBalance", EasiSettingKeys.MediaBalance.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "WPF compatibility", "Current WPF normalized -1..1 media balance."),
        Automated("LiveCamBalance", EasiSettingKeys.MediaBalance.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "FrmOptions.SaveVariables", "Legacy trackbar scale is normalized into -1..1 when outside that range."),
        Automated("MediaMuted", EasiSettingKeys.MediaMuted.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("LiveCamMute", EasiSettingKeys.MediaMuted.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy live camera mute checkbox."),
        Automated("LiveCamNumber", EasiSettingKeys.LiveCameraNumber.Id, SettingsSectionKind.Media, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy camera device index."),

        Automated("AdminDatabasePath", EasiSettingKeys.AdminDatabasePath.Id, SettingsSectionKind.Data, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("DBFileName", EasiSettingKeys.AdminDatabasePath.Id, SettingsSectionKind.Data, LegacySettingValueKind.Path, "Gf.DBFileName", "Legacy SQLite database file path."),
        Automated("DataBackupRoot", EasiSettingKeys.DataBackupRoot.Id, SettingsSectionKind.Data, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),

        Automated("EnableDiagnostics", EasiSettingKeys.EnableDiagnostics.Id, SettingsSectionKind.Advanced, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),

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
            .Distinct(StringComparer.Ordinal)
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
