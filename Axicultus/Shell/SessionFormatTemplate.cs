using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Settings;
using static Axicultus.Wpf.Settings.AxicultusSettingKeys;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Shell;

/// <summary>Reads and updates the session's ListHeader/FormatData in a WinForms .est template.</summary>
internal static class SessionFormatTemplate
{
    internal static string Transfer(ISettingsService settings, string? original, bool load)
    {
        var fields = new Dictionary<int, string>();
        foreach (string field in (original ?? "").Split('>', StringSplitOptions.RemoveEmptyEntries))
        {
            int split = field.IndexOf('=');
            if (split > 0 && int.TryParse(field.AsSpan(0, split), out int code)) fields[code] = field[(split + 1)..];
        }
        if (load && fields.Count == 0) throw new InvalidDataException(UiText.Get("SessionFormatTemplate", "NoSessionFormat"));
        var pending = new Dictionary<string, object>();
        void Set<T>(SettingKey<T> key, T value) => pending[key.Id] = value!;
        string Number(int n) => n.ToString(CultureInfo.InvariantCulture);
        void Map<T>(int code, SettingKey<T> key, Func<T, string> encode, Func<string,T> decode)
        {
            if (!load) fields[code] = encode(settings.Get(key));
            else if (fields.TryGetValue(code, out string? value) && (value.Length > 0 || typeof(T) == typeof(string))) Set(key, decode(value));
        }
        int Parse(string text) => int.Parse(text, CultureInfo.InvariantCulture);
        void Integer(int code, SettingKey<int> key) => Map(code, key, Number, Parse);
        void Text(int code, SettingKey<string> key) => Map(code, key, value => value, value => value);
        void Bits(int code, params (int Mask, SettingKey<bool> Key)[] members)
        {
            bool found = fields.TryGetValue(code, out string? stored) && int.TryParse(stored, out _);
            if (load && !found) return;
            int bits = found ? Math.Max(0, Parse(stored!)) : 0;
            foreach (var (mask, key) in members)
            {
                if (load) Set(key, (bits & mask) != 0);
                else bits = settings.Get(key) ? bits | mask : bits & ~mask;
            }
            if (!load) fields[code] = Number(bits);
        }
        Integer(11, LyricsMonitorPanelColorArgb);
        Bits(12, (1, LyricsMonitorPanelTransparent));
        Integer(13, LyricsMonitorPanelTextColorArgb);
        Bits(14, (1, LyricsMonitorPanelTextColorFollowRegion1));
        Bits(15, (1, LyricsMonitorShowDisplayPanel), (2, LyricsMonitorShowPositionIndicator), (4, LyricsMonitorShowItemNumber),
            (8, LyricsMonitorShowTitleOnPanel), (16, LyricsMonitorShowCopyright), (32, LyricsMonitorShowNextItem));
        static int LegacyPanelHeight(int height) => height is >= 6 and <= 20 ? height : 8;
        Map(16, PanelHeightPercent, height => Number(Math.Clamp(height, 6, 20)), value => LegacyPanelHeight(Parse(value)));
        static string PanelFont(string name) => string.IsNullOrWhiteSpace(name) ? "Microsoft Sans Serif" : name;
        Map(17, PanelFontFamily, PanelFont, PanelFont);
        Bits(18, (1, LyricsMonitorPanelBold), (2, LyricsMonitorPanelItalic), (4, LyricsMonitorPanelUnderline),
            (8, LyricsMonitorPanelShadow), (16, LyricsMonitorPanelOutline));
        if (load && fields.TryGetValue(21, out var heading))
        {
            Set(LyricsMonitorShowTitleHeading, Parse(heading) > 0);
            Set(LyricsMonitorTitleHeadingFirstScreenOnly, Parse(heading) >= 2);
        }
        else if (!load) fields[21] = !settings.Get(LyricsMonitorShowTitleHeading) ? "0" : settings.Get(LyricsMonitorTitleHeadingFirstScreenOnly) ? "2" : "1";
        Bits(22, (2, LyricsMonitorShadow), (4, LyricsMonitorShowNotations), (8, LyricsMonitorCapoZero), (16, LyricsMonitorInterlace), (32, LyricsMonitorOutline));
        if (load && fields.TryGetValue(23, out var headingAlign))
        {
            int index = Math.Clamp(Parse(headingAlign), 0, 4);
            Set(LyricsMonitorTitleHeadingFollowBody, index == 0);
            Set(LyricsMonitorTitleHeadingFollowRegion2, index == 1);
            if (index >= 2) Set(LyricsMonitorTitleHeadingAlignment, (LyricsTextAlignment)(index - 2));
        }
        else if (!load) fields[23] = Number(settings.Get(LyricsMonitorTitleHeadingFollowBody) ? 0 : settings.Get(LyricsMonitorTitleHeadingFollowRegion2) ? 1 : 2 + (int)settings.Get(LyricsMonitorTitleHeadingAlignment));
        Map(25, LyricsMonitorRegionDisplay, v => v == LyricsRegionDisplay.Both ? "2" : Number((int)v - 1),
            v => v == "0" ? LyricsRegionDisplay.Region1Only : v == "1" ? LyricsRegionDisplay.Region2Only : LyricsRegionDisplay.Both);
        Integer(26, LyricsMonitorBackgroundColorArgb);
        Integer(27, LyricsMonitorBackgroundColor2Argb);
        if (load && fields.TryGetValue(28, out var pattern))
        {
            int index = Math.Clamp(Parse(pattern), 0, 11);
            Set(LyricsMonitorBackgroundPattern, index);
            Set(LyricsMonitorBackgroundIsGradient, index != 11);
        }
        else if (!load) fields[28] = Number(Rendering.DefaultBackgroundPattern.SelectedIndex(settings.Current.LiveOutput));
        Integer(29, LyricsMonitorTextColorArgb);
        Integer(30, LyricsMonitorTextColor2Argb);
        Map(31, LyricsMonitorTextAlignment, v => Number(1 + (int)v), v => (LyricsTextAlignment)Math.Clamp(Parse(v) - 1, 0, 2));
        Map(32, LyricsMonitorRegion2Alignment, v => Number((int)v), v => (LyricsRegion2Alignment)Math.Clamp(Parse(v), 0, 3));
        Text(61, LyricsMonitorBackgroundImagePath);
        Map(62, LyricsMonitorBackgroundMode, v => Number(SongFormatData.ToLegacyBackgroundImageMode(v) ?? 2),
            v => SongFormatData.ParseLegacyBackgroundImageMode(v) ?? LyricsBackgroundMode.Fill);
        Map(63, LyricsMonitorVerticalAlignment, v => Number((int)v), v => (LyricsVerticalAlignment)Math.Clamp(Parse(v), 0, 2));
        Integer(64, LyricsMonitorBodyLeftMargin);
        Integer(65, LyricsMonitorBodyRightMargin);
        Integer(66, LyricsMonitorBodyBottomMargin);
        void Transition(int code, SettingKey<string> name, SettingKey<LyricsTransitionKind> motion)
        {
            if (load)
            {
                if (!fields.TryGetValue(code, out var value)) return;
                Set(name, value);
                Set(motion, MainViewModel.TemplateTransitionMotion(value));
            }
            else fields[code] = MainViewModel.TemplateTransitionName(settings.Get(name), settings.Get(motion));
        }
        Transition(72, LyricsMonitorItemTransitionName, LyricsMonitorTransitionKind);
        Transition(73, LyricsMonitorSlideTransitionName, LyricsMonitorSlideTransitionKind);

        string Encode() => string.Concat(fields.Select(pair => $"{pair.Key}={pair.Value}>"));
        if (load)
        {
            if (fields.Keys.Any(key => key is >= 50 and <= 56))
            {
                var media = ItemMediaOptions.Read(original);
                Set(DefaultMediaPath, media.FileName);
                Set(DefaultMediaFormat, media.ApplyTo(""));
            }
            var result = settings.SetValues(pending);
            if (!result.Succeeded) throw new InvalidDataException(string.Join(" ", result.Issues.Select(issue => issue.Message)));
            return Encode();
        }
        string assigned = settings.Get(DefaultMediaPath);
        var options = ItemMediaOptions.Read(settings.Get(DefaultMediaFormat));
        if (options.Mode < 0 && assigned.Length > 0) options = options with { Mode = 2, FileName = assigned };
        fields[50] = Number(Math.Max(0, options.Mode));
        fields[51] = options.FileName;
        fields[52] = Number(options.Volume ?? 50);
        fields[53] = Number(options.Balance ?? 0);
        fields[54] = Number(options.Flags ?? 0);
        fields[55] = Number(options.Camera);
        fields[56] = options.Monitor;
        return Encode();
    }
}
