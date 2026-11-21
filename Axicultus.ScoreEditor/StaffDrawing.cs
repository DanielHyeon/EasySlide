using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Axicultus.ScoreEditor;

internal sealed record StaffLine(int Bar, int Offset, XElement[] Events);

internal static class StaffDrawing
{
    private static readonly GlyphTypeface Symbols = new(new Uri(Path.Combine(AppContext.BaseDirectory, "Bravura.otf")));
    private static readonly Typeface TextFace = new("Malgun Gothic");
    internal const double Height = 192;
    private const double Gap = 10, Leading = 82;

    internal static IEnumerable<StaffLine> Lines(MelodyScore score)
    {
        int bar = 0;
        foreach (var measure in score.Measures)
        {
            int offset = 0;
            foreach (var group in MelodyScore.Events(measure).Chunk(8))
            {
                yield return new(bar, offset, group);
                offset += group.Length;
            }
            bar++;
        }
    }

    internal static int Hit(StaffLine line, double x, double width) => line.Offset +
        Math.Clamp((int)((x - Leading) / SlotWidth(line, width)), 0, line.Events.Length - 1);
    private static double SlotWidth(StaffLine line, double width) => Math.Max(1, (width - Leading - 20) / line.Events.Length);
    private static int StaffSteps(int pitch) => pitch / 12 * 7 + ((pitch % 12) switch
        { 0 or 1 => 0, 2 or 3 => 1, 4 => 2, 5 or 6 => 3, 7 or 8 => 4, 9 or 10 => 5, _ => 6 });

    private static (double Top, double LyricY, double Scale) Layout(StaffLine line)
    {
        var steps = line.Events.Select(MelodyScore.Pitch).OfType<int>().Select(p => StaffSteps(p) - StaffSteps(64)).ToArray();
        double top = Math.Max(65, steps.DefaultIfEmpty().Max() * Gap / 2 - 4 * Gap + 20);
        double lyricY = Math.Max(top + 82, top + 4 * Gap - steps.DefaultIfEmpty().Min() * Gap / 2 + 22);
        return (top, lyricY, Math.Min(1, Height / (lyricY + 45)));
    }

    internal static int PitchAt(StaffLine line, double y)
    {
        var (top, _, scale) = Layout(line);
        int step = Math.Clamp(StaffSteps(64) + (int)Math.Round((top + 4 * Gap - y / scale) / (Gap / 2)), 0, StaffSteps(127));
        return step / 7 * 12 + ((step % 7) switch { 0 => 0, 1 => 2, 2 => 4, 3 => 5, 4 => 7, 5 => 9, _ => 11 });
    }

    internal static void Draw(DrawingContext dc, StaffLine line, double width, int selected = -1)
    {
        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, Height));
        var pen = new Pen(Brushes.Black, 1);
        double slot = SlotWidth(line, width);
        if (selected >= line.Offset && selected < line.Offset + line.Events.Length)
            dc.DrawRectangle(Brushes.AliceBlue, null, new Rect(Leading + (selected - line.Offset) * slot, 0, slot, Height));
        var (top, lyricY, scale) = Layout(line);
        dc.PushTransform(new ScaleTransform(scale, scale));
        width /= scale;
        slot /= scale;
        double leading = Leading / scale;
        for (int i = 0; i < 5; i++) dc.DrawLine(pen, new(12, top + i * Gap), new(width - 12, top + i * Gap));
        dc.DrawLine(pen, new(width - 12, top), new(width - 12, top + 4 * Gap));
        Label(dc, (line.Bar + 1).ToString(CultureInfo.InvariantCulture), 12, 13, 12, 60);
        Glyph(dc, 0xe050, 18, top + 3 * Gap);
        if (line.Bar == 0 && line.Offset == 0) { Glyph(dc, 0xe084, 51, top + Gap); Glyph(dc, 0xe084, 51, top + 3 * Gap); }
        for (int i = 0; i < line.Events.Length; i++)
        {
            var entry = line.Events[i];
            var duration = MelodyScore.Duration(entry);
            int level = ScoreDuration.Types.IndexOf(duration.Type);
            double x = leading + slot * (i + .5) - 6;
            double y = top + 2 * Gap;
            if (MelodyScore.Pitch(entry) is int pitch)
            {
                int step = StaffSteps(pitch) - StaffSteps(64);
                y = top + 4 * Gap - step * Gap / 2;
                for (int ledger = -2; ledger >= step; ledger -= 2)
                    dc.DrawLine(pen, new(x - 4, top + 4 * Gap - ledger * Gap / 2), new(x + 17, top + 4 * Gap - ledger * Gap / 2));
                for (int ledger = 10; ledger <= step; ledger += 2)
                    dc.DrawLine(pen, new(x - 4, top + 4 * Gap - ledger * Gap / 2), new(x + 17, top + 4 * Gap - ledger * Gap / 2));
                if (pitch % 12 is 1 or 3 or 6 or 8 or 10) Glyph(dc, 0xe262, x - 12, y);
                else if (line.Events.Take(i).Reverse().Select(MelodyScore.Pitch).FirstOrDefault(p => p.HasValue && StaffSteps(p.Value) == StaffSteps(pitch)) is int prev && prev != pitch)
                    Glyph(dc, 0xe261, x - 12, y);
                Glyph(dc, level <= 2 ? 0xe0a2 : level == 3 ? 0xe0a3 : 0xe0a4, x, y);
                if (level > 2)
                {
                    bool up = step < 4;
                    double stemX = x + (up ? 11.5 : .6), endY = y + (up ? -34 : 34);
                    dc.DrawLine(pen, new(stemX, y), new(stemX, endY));
                    if (level >= 5) Glyph(dc, 0xe240 + (level - 5) * 2 + (up ? 0 : 1), stemX, endY);
                }
                Label(dc, MelodyScore.Lyric(entry), leading + slot * i + 1, lyricY, 17, slot - 2, Height / scale - lyricY);
            }
            else Glyph(dc, 0xe4e3 + level - 2, x, level == 2 ? top + Gap : y);
            for (int d = 0; d < duration.Dots; d++) dc.DrawEllipse(Brushes.Black, null, new(x + 18 + d * 5, y - 3), 1.7, 1.7);
        }
        dc.Pop();
    }

    internal static void Label(DrawingContext dc, string text, double x, double y, double size, double width, double height = 60)
    {
        if (text.Length == 0 || width < 1) return;
        var label = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            TextFace, size, Brushes.Black, 1) { MaxTextWidth = width, TextAlignment = TextAlignment.Center };
        while (label.Height > height && size > 4) label.SetFontSize(--size);
        dc.DrawText(label, new(x, y));
    }

    private static void Glyph(DrawingContext dc, int code, double x, double y)
    {
        ushort glyph = Symbols.CharacterToGlyphMap[code];
        var run = new GlyphRun(Symbols, 0, false, Gap * 4, 1, new[] { glyph }, new Point(x, y),
            new[] { Symbols.AdvanceWidths[glyph] * Gap * 4 }, null, null, null, null, null, null);
        dc.DrawGlyphRun(Brushes.Black, run);
    }
}
