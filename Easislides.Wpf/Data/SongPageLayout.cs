using System;
using System.Collections.Generic;
using System.Drawing;
using Easislides.Wpf.Library;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Data;

// Font sizes in FOLDER are points at the WinForms reference width of 960 pixels.
public sealed record SongPageLayout(int FontPoints, string FontName, int TopPercent,
    int LeftPercent, int RightPercent, int BottomPercent, double LineSpacing, int HeadingPercent, int FontStyleBits)
{
    public IReadOnlyDictionary<string, string>? Fields { get; init; }
    internal string? Field(string name) => Fields != null && Fields.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
    internal int? Integer(string name) => int.TryParse(Field(name), out int value) ? value : null;
    internal double? Decimal(string name) => double.TryParse(Field(name), System.Globalization.CultureInfo.InvariantCulture, out double value) ? value : null;

    internal string Heading(string original)
    {
        var key = original.Trim().ToLowerInvariant() switch
        {
            "prechorus" or "pre-chorus" or "pre chorus" => "PreChorusHeading",
            "chorus" or "c" => "ChorusHeading",
            "bridge" or "b" => "BridgeHeading",
            "ending" or "e" => "EndingHeading",
            _ => null,
        };
        return key != null ? Field(key) ?? original : original;
    }

    internal (List<string> Bodies, List<string> Labels) Paginate(string lyrics, string? sequence,
        SongFormatData? format, double height, int panelPercent)
    {
        const int width = 960;
        var guarded = LyricsDisplayFormatter.GuardLiteralNotation(lyrics);
        var verses = LyricsDisplayFormatter.ToVersePages(guarded, sequence);
        var labels = LyricsDisplayFormatter.GetSectionLabels(lyrics, sequence);
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        var fontName = string.IsNullOrWhiteSpace(format?.FontName1) ? FontName : format.FontName1;
        var points = Math.Clamp(format?.FontSize1 ?? FontPoints, 1, 500);
        if (string.IsNullOrWhiteSpace(fontName)) fontName = FontFamily.GenericSansSerif.Name;
        var style = (FontStyle)(FontStyleBits & 7);
        using var font = new Font(fontName, points, style);
        using var heading = new Font(fontName, Math.Max(1, points * Math.Clamp(HeadingPercent, 1, 200) / 100), style);
        float textWidth = width * Math.Clamp(100 - (format?.BodyLeftMargin ?? LeftPercent) - (format?.BodyRightMargin ?? RightPercent), 1, 100) / 100f;
        double bodyHeight = height * Math.Clamp(1 - 0.022 - (Math.Clamp(panelPercent, 0, 100) / 100d + 0.03)
            - (format?.BodyBottomMargin ?? BottomPercent) / 100d, 0.01, 1);
        double reservedHeading = Math.Min(bodyHeight / 4, graphics.MeasureString("A", heading).Height * 1.1);
        double availableHeight = (bodyHeight - height * Math.Clamp(TopPercent, 0, 100) / 100d - reservedHeading)
            / (Math.Clamp(LineSpacing, 0.5, 2) + 0.05);
        var lineArea = new SizeF(textWidth, 32000);
        double lineHeight = graphics.MeasureString("A", font, lineArea).Height;
        List<string> pages = [], pageLabels = [];
        for (int verse = 0; verse < verses.Count; verse++)
        {
            var lines = LyricsDisplayFormatter.UnguardLiteralNotation(verses[verse]).Split('\n');
            int first = 0;
            double used = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string text = LyricsDisplayFormatter.StripInlineNotation(lines[i]);
                const int legacyPrefixLength = 8;
                if (text.Length > legacyPrefixLength && text.AsSpan(0, legacyPrefixLength).Contains(' '))
                    text = string.Concat(text[..legacyPrefixLength].Replace(' ', '_'), text.AsSpan(legacyPrefixLength));
                double required = LyricLineMeasurement.Count(text, font, graphics, textWidth) * lineHeight;
                if (i > first && used + required > availableHeight)
                {
                    AddPage(i);
                    first = i;
                    used = 0;
                }
                used += required;
            }
            AddPage(lines.Length);
            void AddPage(int end)
            {
                pages.Add(string.Join("\n", lines, first, end - first));
                pageLabels.Add(verse < labels.Count ? labels[verse] : "");
            }
        }
        return (pages, pageLabels);
    }
}
