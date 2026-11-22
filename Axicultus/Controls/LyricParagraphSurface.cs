using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Axicultus.Wpf.Controls;

public sealed record LyricParagraphStyle(string Text, Typeface Typeface, double Size, double Leading, Brush Ink,
    TextAlignment Alignment, FlowDirection Direction, bool Underline, bool Outline, bool Shadow, bool Indent,
    bool FitLargest, double Width, double Height, int[] NotationLines, int NotationPercent, bool ShrinkToFit = false, bool UseSlideFontMetrics = false);

public sealed class LyricParagraphSurface : FrameworkElement
{
    public static readonly DependencyProperty ParagraphProperty = DependencyProperty.Register(nameof(Paragraph), typeof(LyricParagraphStyle),
        typeof(LyricParagraphSurface), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure |
            FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((LyricParagraphSurface)d).Effect =
                e.NewValue is LyricParagraphStyle { Shadow: true } ? new System.Windows.Media.Effects.DropShadowEffect { ShadowDepth = 2, BlurRadius = 8 } : null));
    public LyricParagraphStyle? Paragraph { get => (LyricParagraphStyle?)GetValue(ParagraphProperty); set => SetValue(ParagraphProperty, value); }
    private readonly List<(FormattedText Text, double Indent, Vector Origin, List<(FormattedText Text, double X)>? Chords)> _lines = [];
    private double _width;

    // The chords drawn over their lyric characters, with the index of their line.
    internal IEnumerable<(int Line, string Chord, double X)> PlacedChords()
    {
        for (int index = 0; index < _lines.Count; index++)
            foreach (var (chord, x) in _lines[index].Chords ?? [])
                yield return (index, chord.Text, x);
    }

    // Where each row is laid out and how high it is, the rows of a wrapped line included.
    internal IEnumerable<(double Top, double Height)> Rows()
    {
        double y = 0;
        foreach (var (text, _, _, _) in _lines)
        {
            int rows = Math.Max(1, (int)Math.Round(text.Height / text.LineHeight));
            for (int row = 0; row < rows; row++) yield return (y + row * text.Height / rows, text.Height / rows);
            y += text.Height;
        }
    }

    /// <summary>
    /// The (negative) shift that puts a row's text back where it stands at the font's own line height: WPF lowers
    /// the baseline in proportion to a larger line height, WinForms draws the text at the top of its row
    /// (gfDisplay.DisplayTextByWidthOneLine).
    /// </summary>
    internal static double LeadIn(FontFamily family, double em, double lineHeight) =>
        Math.Min(0, em * family.Baseline * (1 - lineHeight / (em * family.LineSpacing)));

    protected override Size MeasureOverride(Size available)
    {
        _lines.Clear();
        if (Paragraph is not { } p || string.IsNullOrEmpty(p.Text)) return default;
        _width = Math.Min(p.Width, available.Width);
        if (_width < 1) return default;
        double maxHeight = Math.Min(p.Height, available.Height);
        var lines = Rendering.LyricWordWrapping.ForDisplay(p.Text).ReplaceLineEndings("\n").Split('\n');
        // One line of the paragraph, or (alone) one chord that is placed by its own x.
        (FormattedText Text, double Indent, Vector Origin) Line(int index, double size, string content, bool alone = false)
        {
            double em = Array.BinarySearch(p.NotationLines, index) >= 0 ? size * p.NotationPercent / 100 : size;
            // The slide path uses a 1.3-em outline for a point font measured at 96/72.
            if (p.UseSlideFontMetrics) em *= 1.3 * 72 / 96;
            var text = new FormattedText(content.Length == 0 ? " " : content, CultureInfo.CurrentUICulture,
                p.Direction, p.Typeface, Math.Max(1, em), p.Ink, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                { LineHeight = Math.Max(em * p.Typeface.FontFamily.LineSpacing, (p.UseSlideFontMetrics ? size : em) * p.Leading), TextAlignment = alone ? TextAlignment.Left : p.Alignment };
            if (!alone) text.MaxTextWidth = _width;
            double indent = !alone && p.Indent && p.Alignment == TextAlignment.Left ? Math.Min(_width / 3, em) : 0;
            if (indent > 0) text.SetMaxTextWidths([_width, Math.Max(1, _width - indent)]);
            if (p.Underline) text.SetTextDecorations(TextDecorations.Underline);
            // Extra leading advances the next baseline. Keep the first line at the top of its region.
            Vector origin = p.UseSlideFontMetrics ? new Vector(p.Alignment switch
            {
                TextAlignment.Left => em / 6,
                TextAlignment.Right => -em / 6,
                _ => 0,
            }, em * p.Typeface.FontFamily.Baseline - text.Baseline) : new Vector(0, LeadIn(p.Typeface.FontFamily, em, text.LineHeight));
            return (text, indent, origin);
        }
        // The stored chords of a chord line with the rows of the lyric line below it. Each chord stands at the left
        // edge of its lyric character (gfDisplay.DisplayTextByWidthOneLine) or right of the chord before it when
        // that one is in the way. A lyric line that wraps is cut into its rows, so each row carries its own chords.
        // Null when the chord line is drawn as the text it is, like chords typed after the marker: right-to-left
        // text, or a wrapped line with a hanging indent.
        List<(List<(FormattedText Text, double X)> Chords, (FormattedText Text, double Indent, Vector Origin) Lyric)>? ChordRows(int index, double size)
        {
            if (p.Direction != FlowDirection.LeftToRight || index + 1 >= lines.Length || lines[index + 1].Length == 0 ||
                Array.BinarySearch(p.NotationLines, index) < 0 ||
                Shell.LyricsDisplayFormatter.ReadPlacedChords(lines[index]) is not { } stored) return null;
            string shown = lines[index + 1];
            var whole = Line(index + 1, size, shown);
            // A row starts at the character whose box is a line lower than the row before it. One row can be a
            // fraction of a pixel above its line height; a second row adds a whole one.
            var starts = new List<int> { 0 };
            if (whole.Text.Height > whole.Text.LineHeight * 1.5)
            {
                if (whole.Indent > 0) return null;
                double top = 0;
                for (int i = 1; i < shown.Length; i++)
                {
                    var box = whole.Text.BuildHighlightGeometry(new Point(), i, 1)?.Bounds ?? Rect.Empty;
                    if (box.IsEmpty || box.Top < top + whole.Text.LineHeight / 2) continue;
                    starts.Add(i);
                    top = box.Top;
                }
            }
            var texts = new string[starts.Count];
            var rows = new List<(List<(FormattedText Text, double X)> Chords, (FormattedText Text, double Indent, Vector Origin) Lyric)>(starts.Count);
            for (int row = 0; row < starts.Count; row++)
            {
                texts[row] = starts.Count == 1 ? shown : shown[starts[row]..(row + 1 < starts.Count ? starts[row + 1] : shown.Length)].TrimEnd(' ');
                var lyric = starts.Count == 1 ? whole : Line(index + 1, size, texts[row]);
                if (starts.Count > 1 && lyric.Text.Height > lyric.Text.LineHeight * 1.5) return null;
                rows.Add(([], lyric));
            }
            double gap = Line(index, size, " ", alone: true).Text.WidthIncludingTrailingWhitespace;
            var free = new double[rows.Count];
            Array.Fill(free, double.NegativeInfinity);
            foreach (var (chord, position) in stored)
            {
                // A position counts the characters of the stored line, which has no word joiners.
                int at = 0;
                for (int before = position; at < shown.Length && (shown[at] == '\u2060' || before-- > 0); at++) { }
                int row = starts.FindLastIndex(start => start <= at), inRow = at - starts[row];
                // A chord on the space a row ends with belongs to the word that starts the next row.
                if (inRow >= texts[row].Length && row + 1 < rows.Count) (row, inRow) = (row + 1, 0);
                var lyric = rows[row].Lyric;
                var area = lyric.Text.BuildHighlightGeometry(new Point(), Math.Clamp(inRow, 0, Math.Max(0, texts[row].Length - 1)), 1)?.Bounds ?? Rect.Empty;
                double x = Math.Max(free[row], lyric.Origin.X + (area.IsEmpty ? 0 : inRow < texts[row].Length ? area.Left : area.Right));
                var text = Line(index, size, chord, alone: true).Text;
                rows[row].Chords.Add((text, x));
                free[row] = x + text.WidthIncludingTrailingWhitespace + gap;
            }
            return rows;
        }
        double Measure(double size, bool retain)
        {
            double used = 0;
            void Add((FormattedText Text, double Indent, Vector Origin) line, List<(FormattedText Text, double X)>? chords)
            {
                used += line.Text.Height;
                if (retain) _lines.Add((line.Text, line.Indent, line.Origin, chords));
            }
            for (int index = 0; index < lines.Length && (retain || used <= maxHeight); index++)
            {
                if (ChordRows(index, size) is not { } rows)
                {
                    Add(Line(index, size, lines[index]), null);
                    continue;
                }
                foreach (var (chords, lyric) in rows)
                {
                    // The chord row above a lyric row that has chords; a line of one row always has it.
                    if (chords.Count > 0 || rows.Count == 1) Add(Line(index, size, " "), chords);
                    Add(lyric, null);
                }
                index++; // the lyric line is drawn with its chords
            }
            return used;
        }
        double size = p.Size;
        if (double.IsFinite(maxHeight) && maxHeight > 0 &&
            (p.FitLargest || (p.ShrinkToFit && Measure(size, false) > maxHeight)))
        {
            int low = 1, high = Math.Max(1, (int)(p.FitLargest ? Math.Min(2048, maxHeight) : size));
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (Measure(middle, false) <= maxHeight) low = middle;
                else high = middle - 1;
            }
            size = low;
        }
        return new Size(_width, Math.Min(maxHeight, Measure(size, true)));
    }

    protected override void OnRender(DrawingContext drawing)
    {
        if (Paragraph is not { } p) return;
        drawing.PushClip(new RectangleGeometry(new Rect(RenderSize)));
        double y = 0;
        foreach (var (text, indent, origin, chords) in _lines)
        {
            if (y >= ActualHeight) break;
            void Paint(FormattedText run, double x)
            {
                drawing.PushTransform(new TranslateTransform(x, y + origin.Y));
                if (p.Outline) drawing.DrawGeometry(p.Ink, new Pen(Brushes.Black, 1.5), run.BuildGeometry(new Point()));
                else drawing.DrawText(run, new Point());
                drawing.Pop();
            }
            if (chords != null)
            {
                foreach (var (chord, x) in chords) Paint(chord, x);
            }
            else if (indent > 0 && text.Height > text.LineHeight)
            {
                drawing.PushClip(new RectangleGeometry(new Rect(0, y, _width, text.LineHeight)));
                Paint(text, origin.X);
                drawing.Pop();
                drawing.PushClip(new RectangleGeometry(new Rect(0, y + text.LineHeight, _width, text.Height - text.LineHeight)));
                Paint(text, indent + origin.X);
                drawing.Pop();
            }
            else Paint(text, origin.X);
            y += text.Height;
        }
        drawing.Pop();
    }
}
