using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

internal static class ItemFormatFields
{
    internal static Dictionary<int, string> Parse(string? raw)
    {
        var fields = new Dictionary<int, string>();
        foreach (var field in (raw ?? "").Split('>', StringSplitOptions.RemoveEmptyEntries))
        {
            int split = field.IndexOf('=');
            if (split > 0 && int.TryParse(field.AsSpan(0, split), NumberStyles.Integer, CultureInfo.InvariantCulture, out int key))
                fields[key] = field[(split + 1)..];
        }
        return fields;
    }

    internal static string Encode(IReadOnlyDictionary<int, string> fields) =>
        string.Join('>', fields.Select(field => FormattableString.Invariant($"{field.Key}={field.Value}")));

    internal static string MergeEdits(string? raw, SongFormatData previous, SongFormatData updated)
    {
        var original = Parse(raw);
        var before = Parse(previous.Encode());
        var after = Parse(updated.Encode());
        foreach (int key in before.Keys.Union(after.Keys))
        {
            if (before.GetValueOrDefault(key) == after.GetValueOrDefault(key)) continue;
            if (after.TryGetValue(key, out string? value)) original[key] = value;
            else original.Remove(key);
        }
        return Encode(original);
    }
}

public sealed record ItemDisplayFormat(int? Effects, int? Heading, int? HeadingAlignment, int? Region, int? Top1, int? Top2, int Transpose)
{
    internal static ItemDisplayFormat? Read(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        var fields = ItemFormatFields.Parse(raw);
        int? Number(int key, int max) => int.TryParse(fields.GetValueOrDefault(key), out int n) && n >= 0 && n <= max ? n : null;
        return new(Number(22, 127), Number(21, 3), Number(23, 4), Number(25, 2), Number(45, 100), Number(46, 100), Number(71, 11) ?? 0);
    }

    internal bool? Flag(int bit) => Effects is int flags ? (flags & bit) != 0 : null;

    internal LiveOutputRenderSettings Apply(LiveOutputRenderSettings settings, int viewportHeight) => settings with
    {
        LyricsMonitorShadow = Flag(2) ?? settings.LyricsMonitorShadow,
        ShowLyricsOutline = Flag(32) ?? settings.ShowLyricsOutline,
        LyricsMonitorInterlace = Flag(16) ?? settings.LyricsMonitorInterlace,
        LyricsMonitorShowDisplayPanel = Flag(64) == true ? false : settings.LyricsMonitorShowDisplayPanel,
        ShowLyricsTitleHeading = Heading is int heading ? heading > 0 : settings.ShowLyricsTitleHeading,
        TitleHeadingFirstScreenOnly = Heading is int mode ? mode >= 2 : settings.TitleHeadingFirstScreenOnly,
        LyricsMonitorTitleHeadingFollowBody = HeadingAlignment is int alignment ? alignment == 0 : settings.LyricsMonitorTitleHeadingFollowBody,
        LyricsMonitorTitleHeadingFollowRegion2 = HeadingAlignment is int region ? region == 1 : settings.LyricsMonitorTitleHeadingFollowRegion2,
        LyricsMonitorTitleHeadingAlignment = HeadingAlignment is >= 2 and <= 4 ? (LyricsTextAlignment)(HeadingAlignment.Value - 2) : settings.LyricsMonitorTitleHeadingAlignment,
        LyricsMonitorRegionDisplay = Region switch { 0 => LyricsRegionDisplay.Region1Only, 1 => LyricsRegionDisplay.Region2Only, 2 => LyricsRegionDisplay.Both, _ => settings.LyricsMonitorRegionDisplay },
        LyricsMonitorBodyVerticalOffset = Top1 is int top ? viewportHeight * top / 100 : settings.LyricsMonitorBodyVerticalOffset,
        LyricsMonitorRegionGapPx = Top2 is int second ? viewportHeight * Math.Max(0, second - (Top1 ?? 0)) / 100 : settings.LyricsMonitorRegionGapPx
    };
}
