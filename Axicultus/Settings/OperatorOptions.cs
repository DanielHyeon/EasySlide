using System;
using System.Collections.Generic;
using System.Linq;

namespace Axicultus.Wpf.Settings;

public sealed record AlertAppearance
{
    public string Family { get; init; } = "Microsoft Sans Serif";
    public int Points { get; init; } = 22;
    public int ForegroundArgb { get; init; } = -16777216;
    public int BackgroundArgb { get; init; } = -1;
    public int Horizontal { get; init; } = 2;
    public int Vertical { get; init; } = 2;
    public bool Bold { get; init; } = false;
    public bool Italic { get; init; } = false;
    public bool Underline { get; init; } = false;
    public bool Shadow { get; init; } = false;
    public bool Outline { get; init; } = false;
}

public sealed record OperatorOptions
{
    public int StageFontPoints { get; init; } = 22;
    public int StageNotationPoints { get; init; } = 20;
    public bool StageShowNotations { get; init; } = true;
    public int StageTextArgb { get; init; } = -16777216;
    public int StageBackgroundArgb { get; init; } = -1;
    public bool StageBold { get; init; } = false;
    public bool StageItalic { get; init; } = false;
    public bool StageUnderline { get; init; } = false;
    public int PreviewFontPoints { get; init; } = 8;
    public bool PreviewDivider { get; init; } = false;
    public bool PreviewFocusColorEnabled { get; init; } = true;
    public int PreviewFocusArgb { get; init; } = -2097921;
    public int PreviewSelectionTextArgb { get; init; } = -65536;
    public int PreviewSelectionBackgroundArgb { get; init; } = -256;
    public int EditHistoryLimit { get; init; } = 10;
    public int BackgroundVideoPercent { get; init; } = 100;
    public int BackgroundVideoAlignment { get; init; } = 1;
    public bool LargestFont { get; init; } = false;
    public bool RegionDivider { get; init; } = true;
    public bool HangingIndent { get; init; } = false;
    public int NotationPercent { get; init; } = 75;
    public bool PreventScreenSaver { get; init; } = true;
    public bool StagePreferSecondary { get; init; } = true;
    public bool StageCustom { get; init; } = false;
    public int StageLeft { get; init; } = 0;
    public int StageTop { get; init; } = 0;
    public int StageWidth { get; init; } = 1024;
    public bool OutputAsSingle { get; init; } = false;
    public bool CameraWide { get; init; } = false;
    public bool CameraHidePanel { get; init; } = false;
    public int CameraVolume { get; init; } = 50;
    public int CameraBalance { get; init; } = 0;
    public bool CameraMuted { get; init; } = false;
    public bool AlternateKeyboard { get; init; } = false;
    public bool GlobalF7 { get; init; } = false;
    public bool GlobalF8 { get; init; } = false;
    public bool GlobalF9 { get; init; } = false;
    public bool GlobalF10 { get; init; } = false;
    public bool GlobalArrow { get; init; } = false;
    public bool GlobalCtrlArrow { get; init; } = false;
    public AlertAppearance Message { get; init; } = new();
    public AlertAppearance Parental { get; init; } = new() { ForegroundArgb = -65281, Horizontal = 3 };
    public AlertAppearance Reference { get; init; } = new() { Horizontal = 3, Vertical = 1 };
}

internal sealed record OperatorOption(string Id, object Key, string LegacyKey, object Default, int? Minimum,
    int? Maximum, int FormatBit, Func<OperatorOptions, object> Read, Func<OperatorOptions, object, OperatorOptions> Write)
{
    internal static OperatorOption Create<T>(string name, string legacy, T fallback,
        Func<OperatorOptions, T> read, Func<OperatorOptions, T, OperatorOptions> write,
        int? min = null, int? max = null, int bit = 0) where T : notnull
    {
        var key = new SettingKey<T>("operator." + name, fallback);
        return new(key.Id, key, legacy, fallback, min, max, bit, o => read(o), (o, value) => write(o, (T)value));
    }
}

internal static class OperatorOptionCatalog
{
    internal static readonly OperatorOption[] All =
    [
        OperatorOption.Create("StageFontPoints", "LyricsMonitorFontSize", 22,
            o => o.StageFontPoints, (o, v) => o with { StageFontPoints = v }, 8, 40),
        OperatorOption.Create("StageNotationPoints", "LyricsMonitorNotationsFontSize", 20,
            o => o.StageNotationPoints, (o, v) => o with { StageNotationPoints = v }, 8, 40),
        OperatorOption.Create("StageShowNotations", "LyricsMonitorShowNotations", true,
            o => o.StageShowNotations, (o, v) => o with { StageShowNotations = v }),
        OperatorOption.Create("StageTextArgb", "LyricsMonitorTextColour", -16777216,
            o => o.StageTextArgb, (o, v) => o with { StageTextArgb = v }),
        OperatorOption.Create("StageBackgroundArgb", "LyricsMonitorBackColour", -1,
            o => o.StageBackgroundArgb, (o, v) => o with { StageBackgroundArgb = v }),
        OperatorOption.Create("StageBold", "LyricsMonitorFontFormat", false,
            o => o.StageBold, (o, v) => o with { StageBold = v }, bit: 1),
        OperatorOption.Create("StageItalic", "LyricsMonitorFontFormat", false,
            o => o.StageItalic, (o, v) => o with { StageItalic = v }, bit: 2),
        OperatorOption.Create("StageUnderline", "LyricsMonitorFontFormat", false,
            o => o.StageUnderline, (o, v) => o with { StageUnderline = v }, bit: 4),
        OperatorOption.Create("PreviewFontPoints", "PreviewAreaFontSize", 8,
            o => o.PreviewFontPoints, (o, v) => o with { PreviewFontPoints = v }, 8, 20),
        OperatorOption.Create("PreviewDivider", "PreviewAreaLineBetweenScreens", false,
            o => o.PreviewDivider, (o, v) => o with { PreviewDivider = v }),
        OperatorOption.Create("PreviewFocusColorEnabled", "UseFocusedBackColour", true,
            o => o.PreviewFocusColorEnabled, (o, v) => o with { PreviewFocusColorEnabled = v }),
        OperatorOption.Create("PreviewFocusArgb", "FocusedBackColour", -2097921,
            o => o.PreviewFocusArgb, (o, v) => o with { PreviewFocusArgb = v }),
        OperatorOption.Create("PreviewSelectionTextArgb", "SelectedSlideTextColour", -65536,
            o => o.PreviewSelectionTextArgb, (o, v) => o with { PreviewSelectionTextArgb = v }),
        OperatorOption.Create("PreviewSelectionBackgroundArgb", "SelectedSlideBackColour", -256,
            o => o.PreviewSelectionBackgroundArgb, (o, v) => o with { PreviewSelectionBackgroundArgb = v }),
        OperatorOption.Create("EditHistoryLimit", "MaxEditHistory", 10,
            o => o.EditHistoryLimit, (o, v) => o with { EditHistoryLimit = v }, 0, 20),
        OperatorOption.Create("BackgroundVideoPercent", "VideoSize", 100,
            o => o.BackgroundVideoPercent, (o, v) => o with { BackgroundVideoPercent = v }, 25, 100),
        OperatorOption.Create("BackgroundVideoAlignment", "VideoVAlign", 1,
            o => o.BackgroundVideoAlignment, (o, v) => o with { BackgroundVideoAlignment = v }, 0, 2),
        OperatorOption.Create("LargestFont", "UseLargestFontSize", false,
            o => o.LargestFont, (o, v) => o with { LargestFont = v }),
        OperatorOption.Create("RegionDivider", "LineBetweenRegions", true,
            o => o.RegionDivider, (o, v) => o with { RegionDivider = v }),
        OperatorOption.Create("HangingIndent", "WordWrapLeftAlignIndent", false,
            o => o.HangingIndent, (o, v) => o with { HangingIndent = v }),
        OperatorOption.Create("NotationPercent", "NotationFontFactor", 75,
            o => o.NotationPercent, (o, v) => o with { NotationPercent = v }, 20, 200),
        OperatorOption.Create("PreventScreenSaver", "DisableSreenSaver", true,
            o => o.PreventScreenSaver, (o, v) => o with { PreventScreenSaver = v }),
        OperatorOption.Create("StagePreferSecondary", "AlwaysTrySecondaryLyricsMonitor", true,
            o => o.StagePreferSecondary, (o, v) => o with { StagePreferSecondary = v }),
        OperatorOption.Create("StageCustom", "LyricsMonitorOption", false,
            o => o.StageCustom, (o, v) => o with { StageCustom = v }),
        OperatorOption.Create("StageLeft", "LyricsMonitorOptionCustomLeft", 0,
            o => o.StageLeft, (o, v) => o with { StageLeft = v }, -9999, 9999),
        OperatorOption.Create("StageTop", "LyricsMonitorOptionCustomTop", 0,
            o => o.StageTop, (o, v) => o with { StageTop = v }, -9999, 9999),
        OperatorOption.Create("StageWidth", "LyricsMonitorOptionCustomWidth", 1024,
            o => o.StageWidth, (o, v) => o with { StageWidth = v }, 1, 9999),
        OperatorOption.Create("OutputAsSingle", "DualMonitorOptionCustomAsSingle", false,
            o => o.OutputAsSingle, (o, v) => o with { OutputAsSingle = v }),
        OperatorOption.Create("CameraWide", "LiveCamWidescreen", false,
            o => o.CameraWide, (o, v) => o with { CameraWide = v }),
        OperatorOption.Create("CameraHidePanel", "LiveCamNoPanelOverlay", false,
            o => o.CameraHidePanel, (o, v) => o with { CameraHidePanel = v }),
        OperatorOption.Create("CameraVolume", "LiveCamVolume", 50,
            o => o.CameraVolume, (o, v) => o with { CameraVolume = v }, 0, 100),
        OperatorOption.Create("CameraBalance", "LiveCamBalance", 0,
            o => o.CameraBalance, (o, v) => o with { CameraBalance = v }, -100, 100),
        OperatorOption.Create("CameraMuted", "LiveCamMute", false,
            o => o.CameraMuted, (o, v) => o with { CameraMuted = v }),
        OperatorOption.Create("AlternateKeyboard", "KeyBoardOption", false,
            o => o.AlternateKeyboard, (o, v) => o with { AlternateKeyboard = v }),
        OperatorOption.Create("GlobalF7", "GlobalHookKey_F7", false,
            o => o.GlobalF7, (o, v) => o with { GlobalF7 = v }),
        OperatorOption.Create("GlobalF8", "GlobalHookKey_F8", false,
            o => o.GlobalF8, (o, v) => o with { GlobalF8 = v }),
        OperatorOption.Create("GlobalF9", "GlobalHookKey_F9", false,
            o => o.GlobalF9, (o, v) => o with { GlobalF9 = v }),
        OperatorOption.Create("GlobalF10", "GlobalHookKey_F10", false,
            o => o.GlobalF10, (o, v) => o with { GlobalF10 = v }),
        OperatorOption.Create("GlobalArrow", "GlobalHookKey_Arrow", false,
            o => o.GlobalArrow, (o, v) => o with { GlobalArrow = v }),
        OperatorOption.Create("GlobalCtrlArrow", "GlobalHookKey_CtrlArrow", false,
            o => o.GlobalCtrlArrow, (o, v) => o with { GlobalCtrlArrow = v }),
        OperatorOption.Create("MessageFamily", "MessageAlertFontName", "Microsoft Sans Serif",
            o => o.Message.Family, (o, v) => o with { Message = o.Message with { Family = v } }),
        OperatorOption.Create("MessagePoints", "MessageAlertFontSize", 22,
            o => o.Message.Points, (o, v) => o with { Message = o.Message with { Points = v } }, 8, 100),
        OperatorOption.Create("MessageForegroundArgb", "MessageAlertTextColour", -16777216,
            o => o.Message.ForegroundArgb, (o, v) => o with { Message = o.Message with { ForegroundArgb = v } }),
        OperatorOption.Create("MessageBackgroundArgb", "MessageAlertBackColour", -1,
            o => o.Message.BackgroundArgb, (o, v) => o with { Message = o.Message with { BackgroundArgb = v } }),
        OperatorOption.Create("MessageHorizontal", "MessageAlertTextAlign", 2,
            o => o.Message.Horizontal, (o, v) => o with { Message = o.Message with { Horizontal = v } }, 1, 3),
        OperatorOption.Create("MessageVertical", "MessageAlertVerticalAlign", 2,
            o => o.Message.Vertical, (o, v) => o with { Message = o.Message with { Vertical = v } }, 0, 2),
        OperatorOption.Create("MessageBold", "MessageAlertFontFormat", false,
            o => o.Message.Bold, (o, v) => o with { Message = o.Message with { Bold = v } }, bit: 1),
        OperatorOption.Create("MessageItalic", "MessageAlertFontFormat", false,
            o => o.Message.Italic, (o, v) => o with { Message = o.Message with { Italic = v } }, bit: 2),
        OperatorOption.Create("MessageUnderline", "MessageAlertFontFormat", false,
            o => o.Message.Underline, (o, v) => o with { Message = o.Message with { Underline = v } }, bit: 4),
        OperatorOption.Create("MessageShadow", "MessageAlertFontFormat", false,
            o => o.Message.Shadow, (o, v) => o with { Message = o.Message with { Shadow = v } }, bit: 8),
        OperatorOption.Create("MessageOutline", "MessageAlertFontFormat", false,
            o => o.Message.Outline, (o, v) => o with { Message = o.Message with { Outline = v } }, bit: 16),
        OperatorOption.Create("ParentalFamily", "ParentalAlertFontName", "Microsoft Sans Serif",
            o => o.Parental.Family, (o, v) => o with { Parental = o.Parental with { Family = v } }),
        OperatorOption.Create("ParentalPoints", "ParentalAlertFontSize", 22,
            o => o.Parental.Points, (o, v) => o with { Parental = o.Parental with { Points = v } }, 8, 100),
        OperatorOption.Create("ParentalForegroundArgb", "ParentalAlertTextColour", -65281,
            o => o.Parental.ForegroundArgb, (o, v) => o with { Parental = o.Parental with { ForegroundArgb = v } }),
        OperatorOption.Create("ParentalBackgroundArgb", "ParentalAlertBackColour", -1,
            o => o.Parental.BackgroundArgb, (o, v) => o with { Parental = o.Parental with { BackgroundArgb = v } }),
        OperatorOption.Create("ParentalHorizontal", "ParentalAlertTextAlign", 3,
            o => o.Parental.Horizontal, (o, v) => o with { Parental = o.Parental with { Horizontal = v } }, 1, 3),
        OperatorOption.Create("ParentalVertical", "ParentalAlertVerticalAlign", 2,
            o => o.Parental.Vertical, (o, v) => o with { Parental = o.Parental with { Vertical = v } }, 0, 2),
        OperatorOption.Create("ParentalBold", "ParentalAlertFontFormat", false,
            o => o.Parental.Bold, (o, v) => o with { Parental = o.Parental with { Bold = v } }, bit: 1),
        OperatorOption.Create("ParentalItalic", "ParentalAlertFontFormat", false,
            o => o.Parental.Italic, (o, v) => o with { Parental = o.Parental with { Italic = v } }, bit: 2),
        OperatorOption.Create("ParentalUnderline", "ParentalAlertFontFormat", false,
            o => o.Parental.Underline, (o, v) => o with { Parental = o.Parental with { Underline = v } }, bit: 4),
        OperatorOption.Create("ParentalShadow", "ParentalAlertFontFormat", false,
            o => o.Parental.Shadow, (o, v) => o with { Parental = o.Parental with { Shadow = v } }, bit: 8),
        OperatorOption.Create("ParentalOutline", "ParentalAlertFontFormat", false,
            o => o.Parental.Outline, (o, v) => o with { Parental = o.Parental with { Outline = v } }, bit: 16),
        OperatorOption.Create("ReferenceFamily", "ReferenceAlertFontName", "Microsoft Sans Serif",
            o => o.Reference.Family, (o, v) => o with { Reference = o.Reference with { Family = v } }),
        OperatorOption.Create("ReferencePoints", "ReferenceAlertFontSize", 22,
            o => o.Reference.Points, (o, v) => o with { Reference = o.Reference with { Points = v } }, 8, 100),
        OperatorOption.Create("ReferenceForegroundArgb", "ReferenceAlertTextColour", -16777216,
            o => o.Reference.ForegroundArgb, (o, v) => o with { Reference = o.Reference with { ForegroundArgb = v } }),
        OperatorOption.Create("ReferenceBackgroundArgb", "ReferenceAlertBackColour", -1,
            o => o.Reference.BackgroundArgb, (o, v) => o with { Reference = o.Reference with { BackgroundArgb = v } }),
        OperatorOption.Create("ReferenceHorizontal", "ReferenceAlertTextAlign", 3,
            o => o.Reference.Horizontal, (o, v) => o with { Reference = o.Reference with { Horizontal = v } }, 1, 3),
        OperatorOption.Create("ReferenceVertical", "ReferenceAlertVerticalAlign", 1,
            o => o.Reference.Vertical, (o, v) => o with { Reference = o.Reference with { Vertical = v } }, 0, 2),
        OperatorOption.Create("ReferenceBold", "ReferenceAlertFontFormat", false,
            o => o.Reference.Bold, (o, v) => o with { Reference = o.Reference with { Bold = v } }, bit: 1),
        OperatorOption.Create("ReferenceItalic", "ReferenceAlertFontFormat", false,
            o => o.Reference.Italic, (o, v) => o with { Reference = o.Reference with { Italic = v } }, bit: 2),
        OperatorOption.Create("ReferenceUnderline", "ReferenceAlertFontFormat", false,
            o => o.Reference.Underline, (o, v) => o with { Reference = o.Reference with { Underline = v } }, bit: 4),
        OperatorOption.Create("ReferenceShadow", "ReferenceAlertFontFormat", false,
            o => o.Reference.Shadow, (o, v) => o with { Reference = o.Reference with { Shadow = v } }, bit: 8),
        OperatorOption.Create("ReferenceOutline", "ReferenceAlertFontFormat", false,
            o => o.Reference.Outline, (o, v) => o with { Reference = o.Reference with { Outline = v } }, bit: 16),
    ];

    internal static readonly IReadOnlyDictionary<string, OperatorOption> ById = All.ToDictionary(o => o.Id);
}
