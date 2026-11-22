using System;
using System.Collections.Generic;
using System.Drawing;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Data;

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

    /// <summary>
    /// What separates the two regions in a page of <see cref="PaginateRegions"/> and <see cref="Paginate"/>: a line
    /// that lyrics cannot hold (a line "[region 2] " with a space after it is a lyric line and is shown as "[region 2]").
    /// </summary>
    internal const string Region2Break = "\n\u001E\n";

    /// <summary>The two regions of a page; a page without <see cref="Region2Break"/> is region 1 only.</summary>
    internal static (string Region1, string Region2) SplitRegions(string page)
    {
        int at = page.IndexOf(Region2Break, StringComparison.Ordinal);
        return at < 0 ? (page, "") : (page[..at], page[(at + Region2Break.Length)..]);
    }

    /// <summary>
    /// The screens of a Bible passage in two versions; an entry is a verse in both. As gfLyrics.BuildSlides, region
    /// 1 alone decides the screens: its verses are measured against the height above region 2 (GetOneScreen with
    /// FS_Height_R2Bound) and a verse that fills a screen alone still gets one. Region 2 is not measured: a screen
    /// takes as many of its lines as region 1 has on it (BuildSlidesReg2), which here are the same verses.
    /// </summary>
    /// <param name="fontScales">Receives, for each page, the part of the font size it is drawn with: below 1 when the font was reduced for a first verse too high for a screen.</param>
    internal List<string> PaginateRegions(IReadOnlyList<LyricsRegionPage> verses, SongFormatData? format,
        double height, int panelPercent, int? itemTop, int region2Top, List<double>? fontScales = null)
    {
        using var measure = new Measure(this, format, height, panelPercent, itemTop, region2Top);
        List<string> pages = [];
        int first = 0;
        double used = 0;
        double pageScale = 1;
        // A verse the first version lacks takes the room of its second-version text, or the screens it is on
        // would never end (gfLyrics.BuildSlides measures region 2 for a block without region-1 lines).
        // shortcut: measured with region 1's font; measure with region 2's own font if the two differ a lot.
        string[] Lines(int verse) => (verses[verse].Region1.Length > 0 ? verses[verse].Region1 : verses[verse].Region2).Split('\n');
        double Required(string[] lines)
        {
            double required = 0;
            foreach (var line in lines) required += measure.Required(line);
            return required;
        }
        for (int i = 0; i < verses.Count; i++)
        {
            var lines = Lines(i);
            if (i == 0) pageScale = measure.BeginScreen(lines[0]);
            double required = Required(lines);
            if (i > first && used + required > measure.AvailableHeight)
            {
                AddPage(i);
                first = i;
                used = 0;
                pageScale = measure.BeginScreen(lines[0]);
                required = Required(lines);
            }
            used += required;
        }
        if (verses.Count > 0) AddPage(verses.Count);
        return pages;

        void AddPage(int end)
        {
            fontScales?.Add(pageScale);
            // A verse that one version lacks leaves no empty line: an empty line would read as a screen break.
            string Lines(Func<LyricsRegionPage, string> region)
            {
                var lines = new List<string>(end - first);
                for (int verse = first; verse < end; verse++)
                    if (region(verses[verse]).Length > 0) lines.Add(region(verses[verse]));
                return string.Join("\n", lines);
            }
            pages.Add(Lines(verse => verse.Region1) + Region2Break + Lines(verse => verse.Region2));
        }
    }

    // The font and the room of a region at the reference width, and what a line needs of that room: region 1, or
    // with second region 2 by itself (a verse that has no region 1).
    private sealed class Measure : IDisposable
    {
        private const int Width = 960;
        private readonly Bitmap _bitmap = new(1, 1);
        private readonly Graphics _graphics;
        private readonly Font _font;
        private readonly float _textWidth;
        private readonly double _lineHeight;
        private Font? _reduced;

        internal Measure(SongPageLayout layout, SongFormatData? format, double height, int panelPercent, int? itemTop, int? region2Top, bool second = false)
        {
            _graphics = Graphics.FromImage(_bitmap);
            var fontName = string.IsNullOrWhiteSpace(format?.FontName1) ? layout.FontName : format.FontName1;
            var points = Math.Clamp(format?.FontSize1 ?? layout.FontPoints, 1, 500);
            if (string.IsNullOrWhiteSpace(fontName)) fontName = FontFamily.GenericSansSerif.Name;
            var style = (FontStyle)(layout.FontStyleBits & 7);
            using var heading = CreateFont(fontName, Math.Max(1, points * Math.Clamp(layout.HeadingPercent, 1, 200) / 100), style);
            if (second)
            {
                if (!string.IsNullOrWhiteSpace(format?.FontName2 ?? layout.Field("FontName1"))) fontName = format?.FontName2 ?? layout.Field("FontName1")!;
                points = Math.Clamp(format?.FontSize2 ?? layout.Integer("Size1") ?? points, 1, 500);
                style = (FontStyle)((layout.Integer("BIU1") ?? layout.FontStyleBits) & 7);
            }
            _font = CreateFont(fontName, points, style);
            _textWidth = Width * Math.Clamp(100 - (format?.BodyLeftMargin ?? layout.LeftPercent) - (format?.BodyRightMargin ?? layout.RightPercent), 1, 100) / 100f;
            double bodyHeight = height * Math.Clamp(1 - 0.022 - (Math.Clamp(panelPercent, 0, 100) / 100d + 0.03)
                - (format?.BodyBottomMargin ?? layout.BottomPercent) / 100d, 0.01, 1);
            double reservedHeading = Math.Min(bodyHeight / 4, _graphics.MeasureString("A", heading).Height * 1.1);
            double top2 = height * Math.Clamp(region2Top ?? 100, 0, 100) / 100d;
            // With a second region, region 1 ends where region 2 starts (gfLyrics.FormatText: FS_Height_R2Bound);
            // region 2 has the body below its top, and no heading above it.
            double top = second ? top2 : height * Math.Clamp(itemTop ?? layout.TopPercent, 0, 100) / 100d + reservedHeading;
            double bottom = !second && region2Top != null ? top2 : bodyHeight;
            // The line spacing of region 1 for both regions, as gfLyrics.GetOneScreen (FontSpacingFactor[0]).
            AvailableHeight = (bottom - top) / (Math.Clamp(layout.LineSpacing, 0.5, 2) + 0.05);
            _lineHeight = _graphics.MeasureString("A", _font, new SizeF(_textWidth, 32000)).Height;
        }

        internal double AvailableHeight { get; }

        /// <summary>
        /// Starts a screen with its first line. As gfLyrics.GetOneScreen: when that line alone is higher than the
        /// screen, the font is reduced a point at a time until it fits (ReduceFontToFit), and the lines of this
        /// screen are counted with the reduced font. Returns the reduced size as a part of the full size (1 when it was not reduced): the screen is drawn with that font.
        /// </summary>
        internal double BeginScreen(string firstLine)
        {
            _reduced?.Dispose();
            _reduced = null;
            if (AvailableHeight <= 0) return 1; // no room at all (the screen is not measured yet): nothing fits at any size
            string text = LyricsDisplayFormatter.StripInlineNotation(firstLine);
            var area = new SizeF(_textWidth, 32000);
            for (float size = _font.Size; size > 1 && _graphics.MeasureString(text, _reduced ?? _font, area).Height > AvailableHeight; )
            {
                _reduced?.Dispose();
                _reduced = CreateFont(_font.Name, --size, _font.Style);
            }
            return _reduced is null ? 1 : _reduced.Size / (double)_font.Size;
        }

        internal double Required(string line)
        {
            string text = LyricsDisplayFormatter.StripInlineNotation(line);
            const int legacyPrefixLength = 8;
            if (text.Length > legacyPrefixLength && text.AsSpan(0, legacyPrefixLength).Contains(' '))
                text = string.Concat(text[..legacyPrefixLength].Replace(' ', '_'), text.AsSpan(legacyPrefixLength));
            // The line height stays that of the full font, as in GetOneScreen.
            return LyricLineMeasurement.Count(text, _reduced ?? _font, _graphics, _textWidth) * _lineHeight;
        }

        public void Dispose()
        {
            _reduced?.Dispose();
            _font.Dispose();
            _graphics.Dispose();
            _bitmap.Dispose();
        }
    }

    /// <summary>
    /// The screens of a song. With <paramref name="region2Top"/> the song has two languages and a page holds both
    /// regions around <see cref="Region2Break"/>. As gfLyrics.BuildSlides, region 1 alone decides the screens, in
    /// the room above region 2. Region 2 is not measured: of its screen of the same number, each screen that
    /// region 1 is cut into takes as many lines as region 1 has there, and the last one takes the rest
    /// (BuildSlidesReg2). A verse that has region 2 only is cut by region 2, in its own room and font; the region 2
    /// screens a verse has left over after its region 1 are kept whole.
    /// </summary>
    /// <param name="fontScales">Receives, for each page, the part of the font size it is drawn with: below 1 when the font was reduced for a first line too high for a screen.</param>
    internal (List<string> Bodies, List<string> Labels) Paginate(string lyrics, string? sequence,
        SongFormatData? format, double height, int panelPercent, int? itemTop = null, List<double>? fontScales = null, int? region2Top = null)
    {
        var guarded = LyricsDisplayFormatter.GuardLiteralNotation(lyrics);
        // OnlyRegion2: the screen belongs to a verse that has no region 1.
        List<(string Label, LyricsRegionPage Screen, bool OnlyRegion2)> screens = [];
        foreach (var (label, verse) in LyricsDisplayFormatter.ReadShownVerses(guarded, sequence, keepSpaceRows: true))
        {
            bool onlyRegion2 = verse.TrueForAll(screen => screen.Region1.Length == 0);
            foreach (var screen in verse)
            {
                if (region2Top != null) screens.Add((label, screen, onlyRegion2));
                else
                {
                    // One language: text that reads as region 2 is a screen of its own, so that no line is lost.
                    if (!string.IsNullOrWhiteSpace(screen.Region1)) screens.Add((label, new(screen.Region1, ""), false));
                    if (!string.IsNullOrWhiteSpace(screen.Region2)) screens.Add((label, new(screen.Region2, ""), false));
                }
            }
        }
        using var measure1 = new Measure(this, format, height, panelPercent, itemTop, region2Top);
        using var measure2 = screens.Exists(screen => screen.OnlyRegion2) ? new Measure(this, format, height, panelPercent, itemTop, region2Top, second: true) : null;
        List<string> pages = [], pageLabels = [];
        foreach (var (label, screen, onlyRegion2) in screens)
        {
            string[] Lines(string text) => text.Length > 0 ? LyricsDisplayFormatter.UnguardLiteralNotation(text).Split('\n') : [];
            string[] lines1 = Lines(screen.Region1), lines2 = Lines(screen.Region2);
            if (lines1.Length == 0 && !onlyRegion2)
            {
                Add(Region2Break + string.Join("\n", lines2), 1);
                continue;
            }
            // The region that is measured: region 1, or region 2 where the verse has nothing else
            // (gfLyrics.BuildSlides: GetScreensRequired with Lyrics[0], or with Lyrics[1]).
            var lines = lines1.Length > 0 ? lines1 : lines2;
            var measure = lines1.Length > 0 ? measure1 : measure2!;
            int first = 0, first2 = 0;
            double used = 0;
            double pageScale = measure.BeginScreen(lines[0]);
            for (int i = 0; i < lines.Length; i++)
            {
                double required = measure.Required(lines[i]);
                if (i > first && used + required > measure.AvailableHeight)
                {
                    AddPage(i, Math.Min(i - first, lines2.Length - first2));
                    first = i;
                    used = 0;
                    pageScale = measure.BeginScreen(lines[i]);
                    required = measure.Required(lines[i]);
                }
                used += required;
            }
            AddPage(lines.Length, lines2.Length - first2);
            void AddPage(int end, int count2)
            {
                string page = string.Join("\n", lines, first, end - first);
                // Region 2 is drawn as large as fits its room, whatever font it was cut with.
                if (lines1.Length == 0) Add(Region2Break + page, 1);
                else Add(count2 > 0 ? page + Region2Break + string.Join("\n", lines2, first2, count2) : page, pageScale);
                first2 += count2;
            }
            void Add(string page, double scale)
            {
                pages.Add(page);
                pageLabels.Add(label);
                fontScales?.Add(scale);
            }
        }
        return (pages, pageLabels);
    }

    // GDI+ throws for a family that lacks the requested style (a bold-only family asked for Regular);
    // this runs on go-live and on every verse change, so it must not take the show down.
    private static Font CreateFont(string name, float size, FontStyle style)
    {
        try { return new Font(name, size, style); }
        catch (ArgumentException) { return new Font(FontFamily.GenericSansSerif, size, style); }
    }
}
