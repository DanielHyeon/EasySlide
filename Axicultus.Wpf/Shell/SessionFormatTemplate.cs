using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Settings;
using static Axicultus.Wpf.Settings.AxicultusSettingKeys;

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
        if (load && fields.Count == 0) throw new InvalidDataException("템플릿에 세션 서식이 없습니다.");
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
        Integer(16, PanelHeightPercent);
        Text(17, PanelFontFamily);
        Bits(18, (1, LyricsMonitorPanelBold), (2, LyricsMonitorPanelItalic), (4, LyricsMonitorPanelUnderline),
            (8, LyricsMonitorPanelShadow), (16, LyricsMonitorPanelOutline));
        if (load && fields.TryGetValue(21, out var heading))
        {
            Set(LyricsMonitorShowTitleHeading, Parse(heading) > 0);
            Set(LyricsMonitorTitleHeadingFirstScreenOnly, Parse(heading) >= 2);
        }
        else if (!load) fields[21] = !settings.Get(LyricsMonitorShowTitleHeading) ? "0" : settings.Get(LyricsMonitorTitleHeadingFirstScreenOnly) ? "2" : "1";
        Bits(22, (2, LyricsMonitorShadow), (4, LyricsMonitorShowNotations), (16, LyricsMonitorInterlace), (32, LyricsMonitorOutline));
        Map(25, LyricsMonitorRegionDisplay, v => v == LyricsRegionDisplay.Both ? "2" : Number((int)v - 1),
            v => v == "0" ? LyricsRegionDisplay.Region1Only : v == "1" ? LyricsRegionDisplay.Region2Only : LyricsRegionDisplay.Both);
        Integer(26, LyricsMonitorBackgroundColorArgb);
        Integer(27, LyricsMonitorBackgroundColor2Argb);
        Integer(29, LyricsMonitorTextColorArgb);
        Integer(30, LyricsMonitorTextColor2Argb);
        Map(31, LyricsMonitorTextAlignment, v => Number(1 + (int)v), v => (LyricsTextAlignment)Math.Clamp(Parse(v) - 1, 0, 2));
        Map(32, LyricsMonitorRegion2Alignment, v => Number((int)v), v => (LyricsRegion2Alignment)Math.Clamp(Parse(v), 0, 3));
        Text(61, LyricsMonitorBackgroundImagePath);
        Map(62, LyricsMonitorBackgroundMode, v => v == LyricsBackgroundMode.Tile ? "0" : v == LyricsBackgroundMode.Center ? "1" : "2",
            v => v == "0" ? LyricsBackgroundMode.Tile : v == "1" ? LyricsBackgroundMode.Center : LyricsBackgroundMode.Fit);
        Map(63, LyricsMonitorVerticalAlignment, v => Number((int)v), v => (LyricsVerticalAlignment)Math.Clamp(Parse(v), 0, 2));
        Integer(64, LyricsMonitorBodyLeftMargin);
        Integer(65, LyricsMonitorBodyRightMargin);
        Integer(66, LyricsMonitorBodyBottomMargin);
        Text(72, LyricsMonitorItemTransitionName);
        Text(73, LyricsMonitorSlideTransitionName);

        string format = string.Join("", fields.Select(pair => $"{pair.Key}={pair.Value}>"));
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
            return format;
        }
        string assigned = settings.Get(DefaultMediaPath);
        var options = ItemMediaOptions.Read(settings.Get(DefaultMediaFormat));
        if (options.Mode < 0 && assigned.Length > 0) options = options with { Mode = 2, FileName = assigned };
        return options.ApplyTo(format);
    }
}
