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
        Automated("UseSongNumbering", EasiSettingKeys.UseSongNumbering.Id, SettingsSectionKind.General, LegacySettingValueKind.Boolean, "WPF compatibility", "Show song numbers in the library song list — FrmMain Edit menu Use Song Numbering."),
        Automated("RegistrationUser", EasiSettingKeys.RegistrationUser.Id, SettingsSectionKind.General, LegacySettingValueKind.String, "RegUtil config/RegistrationUser", "Name displayed at startup and About dialog registration field."),
        Automated("MainWindowLeft", EasiSettingKeys.MainWindowLeft.Id, SettingsSectionKind.General, LegacySettingValueKind.Integer, "WPF compatibility", "Main window left coordinate in px (0=never saved) — FrmMain window state (registry)."),
        Automated("MainWindowTop", EasiSettingKeys.MainWindowTop.Id, SettingsSectionKind.General, LegacySettingValueKind.Integer, "WPF compatibility", "Main window top coordinate in px (0=never saved) — FrmMain window state (registry)."),
        Automated("MainWindowWidth", EasiSettingKeys.MainWindowWidth.Id, SettingsSectionKind.General, LegacySettingValueKind.Integer, "WPF compatibility", "Main window width in px (0=never saved, use default) — FrmMain window state (registry)."),
        Automated("MainWindowHeight", EasiSettingKeys.MainWindowHeight.Id, SettingsSectionKind.General, LegacySettingValueKind.Integer, "WPF compatibility", "Main window height in px (0=never saved, use default) — FrmMain window state (registry)."),
        Automated("MainWindowMaximized", EasiSettingKeys.MainWindowMaximized.Id, SettingsSectionKind.General, LegacySettingValueKind.Boolean, "WPF compatibility", "Main window maximized state — FrmMain window state (registry)."),
        Automated("MainInspectorExpanded", EasiSettingKeys.MainInspectorExpanded.Id, SettingsSectionKind.General, LegacySettingValueKind.Boolean, "WPF compatibility", "Right output-appearance inspector expanded/collapsed state — FrmMain panel state (registry)."),
        Automated("MainBrowserSplitPercent", EasiSettingKeys.MainBrowserSplitPercent.Id, SettingsSectionKind.General, LegacySettingValueKind.Integer, "WPF compatibility", "Left browser/worship-queue vertical split ratio (top browser percent, 0=never saved) — FrmMain splitter state (registry)."),

        Automated("Theme", EasiSettingKeys.Theme.Id, SettingsSectionKind.Appearance, LegacySettingValueKind.Enum, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),
        Automated("InterfaceSize", EasiSettingKeys.InterfaceSize.Id, SettingsSectionKind.Appearance, LegacySettingValueKind.Enum, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),

        Automated("DefaultOutputMonitorId", EasiSettingKeys.DefaultOutputMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("OutputMonitorName", EasiSettingKeys.DefaultOutputMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "FrmOptions.SaveVariables", "Legacy dual-monitor output display name."),
        // 스테이지(Preview) 모니터 Id — WPF 전용(레거시 등가 없음). import/export 라운드트립용 별칭.
        Automated("PreviewMonitorId", EasiSettingKeys.PreviewMonitorId.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("UseSafetyConfirmations", EasiSettingKeys.UseSafetyConfirmations.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "No WinForms equivalent; retained for WPF settings import."),
        Automated("ShowLyricsMonitorAlertBox", EasiSettingKeys.ShowLyricsMonitorAlertBox.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy lyrics monitor alert box toggle."),
        Automated("ReferenceAlertSource", EasiSettingKeys.ReferenceAlertSource.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy reference alert source: 0=None, 1=Song Title, 2=Song Number, 3=Book Reference, 4=User Reference."),
        Automated("Reference_Source", EasiSettingKeys.ReferenceAlertSource.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF reference-alert source alias."),
        Automated("ReferenceAlertUsePick", EasiSettingKeys.ReferenceAlertUsePick.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy reference alert pick/filter toggle."),
        Automated("ReferenceAlertBlankIfPickNotFound", EasiSettingKeys.ReferenceAlertBlankIfPickNotFound.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy reference alert blank-when-pick-missing toggle."),
        Automated("ReferenceAlertPickName", EasiSettingKeys.ReferenceAlertPickName.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "FrmOptions.SaveVariables", "Legacy reference alert pick token."),
        Automated("ReferenceAlertPickSubstitute", EasiSettingKeys.ReferenceAlertPickSubstitute.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "FrmOptions.SaveVariables", "Legacy reference alert pick substitute token."),
        Automated("ReferenceAlertPickSeparator", EasiSettingKeys.ReferenceAlertPickSeparator.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "FrmOptions.SaveVariables", "Legacy reference alert pick end separators."),
        Automated("ReferenceAlertDuration", EasiSettingKeys.ReferenceAlertDurationSeconds.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy reference alert auto-hide duration in seconds."),
        Automated("ReferenceAlertDurationSeconds", EasiSettingKeys.ReferenceAlertDurationSeconds.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF reference-alert duration alias."),
        Automated("ReferenceAlertStyle", EasiSettingKeys.ReferenceAlertScroll.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy reference alert style bitfield: bit 1=scroll."),
        Automated("ReferenceAlertScroll", EasiSettingKeys.ReferenceAlertScroll.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF reference-alert scroll alias."),
        Automated("ReferenceAlertStyle", EasiSettingKeys.ReferenceAlertFlash.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy reference alert style bitfield: bit 2=flash."),
        Automated("ReferenceAlertFlash", EasiSettingKeys.ReferenceAlertFlash.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF reference-alert flash alias."),
        Automated("ReferenceAlertStyle", EasiSettingKeys.ReferenceAlertTransparent.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy reference alert style bitfield: bit 3=transparent."),
        Automated("ReferenceAlertTransparent", EasiSettingKeys.ReferenceAlertTransparent.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF reference-alert transparent alias."),
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
        Automated("LMHighlightColour", EasiSettingKeys.LyricsMonitorHighlightColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "FrmOptions.SaveVariables", "Legacy lyrics monitor selected/highlight color."),
        Automated("LyricsMonitorHighlightColour", EasiSettingKeys.LyricsMonitorHighlightColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "RegUtil monitors", "Registry-backed lyrics monitor selected/highlight color."),
        Automated("LyricsMonitorTextColour2", EasiSettingKeys.LyricsMonitorTextColor2Argb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "WPF compatibility", "Global Region2 (dual-language) lyrics text color (0=follow Region1; per-song FormatData 30 overrides when present)."),
        Automated("LyricsMonitorRegion2Alignment", EasiSettingKeys.LyricsMonitorRegion2Alignment.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Global Region2 (dual-language) horizontal alignment (FollowRegion1/Left/Center/Right) — FrmMain Ind_Reg2Align (per-song FormatData 32 overrides when present)."),
        Automated("LyricsMonitorRegion2Bold", EasiSettingKeys.LyricsMonitorRegion2Bold.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Global Region2 (dual-language) bold 3-state (FollowRegion1/On/Off) — FrmMain Ind_Reg2Bold (per-song region2 bold bit overrides when present)."),
        Automated("LyricsMonitorRegion2Italic", EasiSettingKeys.LyricsMonitorRegion2Italic.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Global Region2 (dual-language) italic 3-state (FollowRegion1/On/Off) — FrmMain Ind_Reg2Italic (per-song region2 italic bit overrides when present)."),
        Automated("LyricsMonitorRegion2Underline", EasiSettingKeys.LyricsMonitorRegion2Underline.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Global Region2 (dual-language) underline 3-state (FollowRegion1/On/Off) — FrmMain Ind_Reg2Underline (per-song region2 underline bit overrides when present)."),
        Automated("LyricsMonitorPanelColorArgb", EasiSettingKeys.LyricsMonitorPanelColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "WPF compatibility", "Display Panel band background color (ARGB, semi-transparent) — FrmMain Def_PanelColour."),
        Automated("LyricsMonitorPanelFontScalePercent", EasiSettingKeys.LyricsMonitorPanelFontScalePercent.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Display Panel info-text font scale percent (50-200) — FrmMain Def_PanelFont size."),
        Automated("LyricsMonitorPanelTextColorFollowRegion1", EasiSettingKeys.LyricsMonitorPanelTextColorFollowRegion1.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel text color follows Region1 — FrmMain Def_PanelAsR1."),
        Automated("LyricsMonitorPanelTextColorArgb", EasiSettingKeys.LyricsMonitorPanelTextColorArgb.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Color, "WPF compatibility", "Display Panel text color (ARGB) — FrmMain Def_PanelTextColour."),
        Automated("LyricsMonitorPanelBold", EasiSettingKeys.LyricsMonitorPanelBold.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel bold text effect — FrmMain Def_PanelFont Bold."),
        Automated("LyricsMonitorPanelItalic", EasiSettingKeys.LyricsMonitorPanelItalic.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel italic text effect — FrmMain Def_PanelFont Italic."),
        Automated("LyricsMonitorPanelUnderline", EasiSettingKeys.LyricsMonitorPanelUnderline.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel underline text effect — FrmMain Def_PanelFont Underline."),
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
        Automated("LyricsMonitorUnderline", EasiSettingKeys.LyricsMonitorUnderline.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell lyrics underline toggle — §7.3-A font effects."),
        Automated("LyricsMonitorEmphasisChorusOnly", EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Restrict bold/italic/underline emphasis to chorus verses — FrmMain Ind_*Italics (chorus-only)."),
        Automated("LyricsMonitorInterlace", EasiSettingKeys.LyricsMonitorInterlace.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Interlace dual-language Region1/Region2 lines (original/translation alternating) — FrmMain Def_Interlace."),
        Automated("LyricsMonitorShowDisplayPanel", EasiSettingKeys.LyricsMonitorShowDisplayPanel.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel master visibility toggle — FrmMain Def_PanelShow."),
        Automated("LyricsMonitorPanelTransparent", EasiSettingKeys.LyricsMonitorPanelTransparent.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel transparent background — FrmMain Def_PanelTransparent."),
        Automated("LyricsMonitorFontSize2", EasiSettingKeys.LyricsMonitorFontSize2.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Global Region2 lyrics font size in px (0=same as Region1, else 24-120) — FrmMain Ind_Reg2SizeUpDown."),
        Automated("LyricsMonitorLineSpacingPercent", EasiSettingKeys.LyricsMonitorLineSpacingPercent.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "In-shell lyrics line spacing as % of font size (100-220) — §7.3-A."),
        Automated("LyricsMonitorBodyLeftMargin", EasiSettingKeys.LyricsMonitorBodyLeftMargin.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Output body left margin in px (0-400) — FrmMain ShowLeftMargin."),
        Automated("LyricsMonitorBodyRightMargin", EasiSettingKeys.LyricsMonitorBodyRightMargin.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Output body right margin in px (0-400) — FrmMain ShowRightMargin."),
        Automated("LyricsMonitorBodyBottomMargin", EasiSettingKeys.LyricsMonitorBodyBottomMargin.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Output body bottom margin in px (0-400) — FrmMain ShowBottomMargin."),
        Automated("LyricsMonitorRegionGapPx", EasiSettingKeys.LyricsMonitorRegionGapPx.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Dual-language Region1-Region2 vertical gap in px (0-100) — FrmMain Ind_Reg2TopUpDown."),
        Automated("LyricsMonitorBodyVerticalOffset", EasiSettingKeys.LyricsMonitorBodyVerticalOffset.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Output body vertical position offset in px (-300..300, negative=up) — FrmMain Ind_Reg1TopUpDown."),
        Automated("LyricsMonitorShowPositionIndicator", EasiSettingKeys.LyricsMonitorShowPositionIndicator.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell verse/slide position indicator (N/M) toggle — §7.3-A."),
        Automated("LyricsMonitorShowVerseHeading", EasiSettingKeys.LyricsMonitorShowVerseHeading.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Verse heading (current section label) shown above body — FrmMain Def_Head All."),
        Automated("LyricsMonitorShowItemNumber", EasiSettingKeys.LyricsMonitorShowItemNumber.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel song/item number toggle — FrmMain Show Item Number."),
        Automated("LyricsMonitorShowTitleOnPanel", EasiSettingKeys.LyricsMonitorShowTitleOnPanel.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel song title band toggle — FrmMain Def_PanelTitle."),
        Automated("LyricsMonitorShowCopyright", EasiSettingKeys.LyricsMonitorShowCopyright.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel copyright toggle — FrmMain Show Copyright Information."),
        Automated("LyricsMonitorShowNextItem", EasiSettingKeys.LyricsMonitorShowNextItem.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Display Panel next-item preview toggle — FrmMain Display Panel PrevNext."),
        Automated("LyricsMonitorUseFadeTransition", EasiSettingKeys.LyricsMonitorUseFadeTransition.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Output scene fade transition toggle — FrmMain transition effect (fade only)."),
        Automated("LyricsMonitorTransitionDurationMs", EasiSettingKeys.LyricsMonitorTransitionDurationMs.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Output fade transition duration in ms (0-2000) — FrmMain transition timing."),
        Automated("LyricsMonitorTransitionKind", EasiSettingKeys.LyricsMonitorTransitionKind.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Output ITEM-change transition motion (Fade/Slide 4-way) — FrmMain transition effect (implemented subset)."),
        Automated("LyricsMonitorItemTransitionName", EasiSettingKeys.LyricsMonitorItemTransitionName.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "FrmMain Def_TransItem display text, matching FormatData 72 for per-item overrides."),
        Automated("LyricsMonitorSlideTransitionKind", EasiSettingKeys.LyricsMonitorSlideTransitionKind.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Output SLIDE/verse-change transition motion (within same item) — FrmMain item-vs-slide transition split."),
        Automated("LyricsMonitorSlideTransitionName", EasiSettingKeys.LyricsMonitorSlideTransitionName.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "FrmMain Def_TransSlides display text, matching FormatData 73 for per-slide overrides."),
        Automated("LyricsMonitorFontFamily", EasiSettingKeys.LyricsMonitorFontFamily.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "Global output lyrics font family — FrmMain Def_FontName (per-song FormatData 43 overrides when present)."),
        Automated("LyricsMonitorFontFamily2", EasiSettingKeys.LyricsMonitorFontFamily2.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.String, "WPF compatibility", "Global Region2 (dual-language) lyrics font family — FrmMain Ind_Reg2Font (empty=follow Region1; per-song FormatData 44 overrides when present)."),
        Automated("LyricsMonitorBackgroundImage", EasiSettingKeys.LyricsMonitorBackgroundImagePath.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Path, "WPF compatibility", "Global output background image path — FrmMain Images tab (apply as background)."),
        Automated("LyricsMonitorImageMode", EasiSettingKeys.LyricsMonitorBackgroundMode.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Output background image display mode (Fill/Fit/Center/Tile) — FrmMain Def_ImageMode/Ind_ImageMode (Tile/Centre/BestFit)."),
        Automated("LyricsMonitorGradientDirection", EasiSettingKeys.LyricsMonitorBackgroundGradientDirection.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Output 2-color background gradient direction (Vertical/Horizontal/DiagonalDown/DiagonalUp) — FrmMain Def_BackColour pattern."),
        Automated("LyricsMonitorRegionDisplay", EasiSettingKeys.LyricsMonitorRegionDisplay.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Dual-language region display mode (Both/Region1Only/Region2Only) — FrmMain Def_ShowRegion1/2/Both."),
        Automated("LyricsMonitorShowTitleHeading", EasiSettingKeys.LyricsMonitorShowTitleHeading.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell title heading toggle (song title banner above lyrics) — §7.3-A."),
        Automated("LyricsMonitorOutline", EasiSettingKeys.LyricsMonitorOutline.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell lyrics outline font toggle (stroked glyphs for legibility) — §7.3-A."),
        Automated("LyricsMonitorTitleHeadingAlignment", EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "In-shell title heading horizontal alignment (left/center/right) — §7.3-A heading align."),
        Automated("LyricsMonitorTitleHeadingFirstScreenOnly", EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "In-shell title heading shown only on first verse — §7.3-A heading at first screen only."),
        Automated("LyricsMonitorTitleHeadingFollowBody", EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Title heading follows body (Region1) alignment — FrmMain Def_HeadAlign AsR1."),
        Automated("LyricsMonitorTitleHeadingFollowRegion2", EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Boolean, "WPF compatibility", "Title heading follows secondary region (Region2) alignment — FrmMain Def_HeadAlign AsR2."),
        Automated("AutoRotateIntervalSeconds", EasiSettingKeys.AutoRotateIntervalSeconds.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Integer, "WPF compatibility", "Auto-rotate verse/slide interval in seconds (2-600) — §7.3-B."),
        Automated("AutoRotateMode", EasiSettingKeys.AutoRotateMode.Id, SettingsSectionKind.LiveOutput, LegacySettingValueKind.Enum, "WPF compatibility", "Auto-rotate mode (OneRepeat/One/Group/GroupRepeat) — FrmMain One/One-Repeat/Group/Group-Repeat."),

        Automated("UsePowerpointTab", EasiSettingKeys.UsePowerPointTab.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy tab visibility."),
        Automated("UsePowerPointTab", EasiSettingKeys.UsePowerPointTab.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("NoPowerpointPanelOverlay", EasiSettingKeys.NoPowerPointPanelOverlay.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy live overlay behavior."),
        Automated("NoPowerPointPanelOverlay", EasiSettingKeys.NoPowerPointPanelOverlay.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("PowerPointRenderTimeoutSeconds", EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("ThumbnailCacheMegabytes", EasiSettingKeys.ThumbnailCacheMegabytes.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("PowerPointMaxFiles", EasiSettingKeys.PowerPointMaxFiles.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("PowerpointMaxFiles", EasiSettingKeys.PowerPointMaxFiles.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "RegUtil options", "Registry-backed legacy PowerPoint max files limit."),
        Automated("PP_MaxFiles", EasiSettingKeys.PowerPointMaxFiles.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy PowerPoint recent/max files limit."),
        Automated("PowerPointSourceListingStyle", EasiSettingKeys.PowerPointSourceListingStyle.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF source list/thumbnail mode alias."),
        Automated("ExternalListing", EasiSettingKeys.PowerPointSourceListingStyle.Id, SettingsSectionKind.PowerPoint, LegacySettingValueKind.Integer, "FrmMain PP_Style_DropDownItemClicked", "Legacy left PowerPoint list style: 0=list, 1=thumbnail preview."),

        Automated("UseMediaTab", EasiSettingKeys.UseMediaTab.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy media tab visibility."),
        Automated("NoMediaPanelOverlay", EasiSettingKeys.NoMediaPanelOverlay.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy media overlay behavior."),
        Automated("MediaDirectory", EasiSettingKeys.MediaDirectory.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("MediaDir", EasiSettingKeys.MediaDirectory.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "FrmOptions.SaveVariables", "Legacy music/media directory."),
        Automated("media_dir", EasiSettingKeys.MediaDirectory.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "RegUtil config/media_dir", "Registry-backed legacy media directory."),
        Automated("DefaultMediaPath", EasiSettingKeys.DefaultMediaPath.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "WPF compatibility", "Current WPF default media path alias."),
        Automated("MediaLocation", EasiSettingKeys.DefaultMediaPath.Id, SettingsSectionKind.Media, LegacySettingValueKind.Path, "FrmMain Def_Media_Clicked", "Legacy default session media path."),
        Automated("MediaVolume", EasiSettingKeys.MediaVolume.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "WPF compatibility", "Current WPF normalized 0..1 media volume."),
        Automated("LiveCamVolume", EasiSettingKeys.MediaVolume.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "FrmOptions.SaveVariables", "Legacy trackbar scale is normalized into 0..1 when greater than 1."),
        Automated("MediaBalance", EasiSettingKeys.MediaBalance.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "WPF compatibility", "Current WPF normalized -1..1 media balance."),
        Automated("LiveCamBalance", EasiSettingKeys.MediaBalance.Id, SettingsSectionKind.Media, LegacySettingValueKind.Double, "FrmOptions.SaveVariables", "Legacy trackbar scale is normalized into -1..1 when outside that range."),
        Automated("MediaMuted", EasiSettingKeys.MediaMuted.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "WPF compatibility", "Current WPF setting import/export alias."),
        Automated("LiveCamMute", EasiSettingKeys.MediaMuted.Id, SettingsSectionKind.Media, LegacySettingValueKind.Boolean, "FrmOptions.SaveVariables", "Legacy live camera mute checkbox."),
        Automated("LiveCamNumber", EasiSettingKeys.LiveCameraNumber.Id, SettingsSectionKind.Media, LegacySettingValueKind.Integer, "FrmOptions.SaveVariables", "Legacy camera device index."),

        Automated("PraiseBookCjkGroupStyle", EasiSettingKeys.PraiseBookCjkGroupStyle.Id, SettingsSectionKind.Data, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF inline PraiseBook alpha/word-count sort alias."),
        Automated("PB_CJKGroupStyle", EasiSettingKeys.PraiseBookCjkGroupStyle.Id, SettingsSectionKind.Data, LegacySettingValueKind.Integer, "FrmMain SetSortButtonPB", "Legacy PraiseBook sort: 0=alpha, 1=CJK word count."),
        Automated("SelectedSongFolderNo", EasiSettingKeys.SelectedSongFolderNo.Id, SettingsSectionKind.Data, LegacySettingValueKind.Integer, "WPF compatibility", "Current WPF source SongFolder selection alias."),
        Automated("CurMainSelectedFolder", EasiSettingKeys.SelectedSongFolderNo.Id, SettingsSectionKind.Data, LegacySettingValueKind.Integer, "FrmMain SongFolder_Change", "Legacy current top-left SongFolder number restored on startup."),
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
