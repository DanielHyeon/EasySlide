using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// A row of a score slide: a staff (five long horizontal lines) with the notes just above it and the lyrics under it.
/// The title, empty slide and background are left out, and every row of a slide is cut the same size, so each staff
/// shows at the same scale on the camera.
/// </summary>
internal sealed record ScoreRow(Int32Rect Area, int StaffTop, int StaffBottom);

internal static class ScoreLayout
{
    // Left and Right: where the staff lines run. A background picture beside the score lies outside them.
    private sealed record Staff(int Top, int Bottom, double Spacing, int Left, int Right);

    /// <summary>Rows top to bottom; empty when the slide has no staff.</summary>
    internal static ScoreRow[] FindRows(BitmapSource image)
    {
        var gray = new FormatConvertedBitmap(image, PixelFormats.Gray8, null, 0);
        int width = gray.PixelWidth, height = gray.PixelHeight;
        if (width < 16 || height < 16) return [];
        var pixels = new byte[width * height];
        gray.CopyPixels(pixels, width, 0);
        // Dark notes on a light slide, or light notes on a dark one: the background decides.
        // Thin staff lines come out grey, so ink is anything clearly off the slide's average.
        long sum = 0;
        foreach (var p in pixels) sum += p;
        int mean = (int)(sum / pixels.Length);
        int dark = Math.Min(180, mean - 40), light = Math.Max(150, mean + 60);
        Func<byte, bool> ink = mean >= 128 ? p => p < dark : p => p > light;
        var staves = FindStaves(pixels, width, height, ink);
        return staves.Count > 0 ? Rows(staves, pixels, width, height, ink) : [];
    }

    private static List<Staff> FindStaves(byte[] pixels, int width, int height, Func<byte, bool> ink)
    {
        int minRun = width / 4;
        var lineRows = new List<(int Y, int Left, int Right)>();
        for (int y = 0; y < height; y++)
        {
            int run = 0, best = 0, bestEnd = 0, gap = 0, row = y * width;
            for (int x = 0; x < width; x++)
            {
                if (ink(pixels[row + x])) { run += gap + 1; gap = 0; }
                else if (run > 0 && ++gap > 3) { run = 0; gap = 0; } // a staff line may break for a few pixels
                if (run > best) { best = run; bestEnd = x; }
            }
            if (best >= minRun) lineRows.Add((y, bestEnd - best + 1, bestEnd));
        }

        // A line is one to a few pixel rows thick.
        var lines = new List<(int Y, int Left, int Right)>();
        for (int i = 0; i < lineRows.Count;)
        {
            int start = i;
            while (i + 1 < lineRows.Count && lineRows[i + 1].Y - lineRows[i].Y <= 1) i++;
            var rows = lineRows.GetRange(start, i - start + 1);
            lines.Add(((rows[0].Y + rows[^1].Y) / 2, rows.Min(r => r.Left), rows.Max(r => r.Right)));
            i++;
        }

        // Five evenly spaced lines make a staff; a lone long line (a box border, a dash) does not.
        var staves = new List<Staff>();
        int maxSpacing = Math.Max(4, height / 22);
        for (int i = 0; i < lines.Count;)
        {
            int start = i;
            while (i + 1 < lines.Count && lines[i + 1].Y - lines[i].Y <= maxSpacing) i++;
            int count = i - start + 1;
            if (count >= 4)
            {
                var staff = lines.GetRange(start, count);
                staves.Add(new(staff[0].Y, staff[^1].Y, (staff[^1].Y - staff[0].Y) / (double)(count - 1),
                    staff.Min(l => l.Left), staff.Max(l => l.Right)));
            }
            i++;
        }
        return staves;
    }

    private static ScoreRow[] Rows(List<Staff> staves, byte[] pixels, int width, int height, Func<byte, bool> ink)
    {
        // Bounds no row may cross: the slide's top, the gaps between staves, the end of the last lyrics.
        var cuts = new List<int> { 0 };
        for (int k = 1; k < staves.Count; k++)
            cuts.Add(Gap(pixels, width, staves[k - 1], staves[k], ink));
        cuts.Add(height);

        // How far each row's ink reaches above and below its staff: notes and clef above, notes and lyrics below.
        var above = staves.Select((staff, k) => Above(pixels, width, staff, cuts[k], ink)).ToArray();
        var below = staves.Select((staff, k) => End(pixels, width, cuts[k + 1], staff, ink) - staff.Bottom).ToArray();
        // One size for every row. The last row's lyrics end is the least sure (buttons or a picture may follow),
        // so the rows above it decide when there are any.
        int up = above.Max();
        int down = staves.Count > 1 ? below[..^1].Max() : Math.Min(below[0], (int)(10 * staves[0].Spacing));
        int pad = Math.Max(2, (int)(staves[0].Spacing / 2));
        // Same height, so the staves show the same size; each its own width, so no empty slide beside a shorter staff.
        return [.. staves.Select((staff, k) =>
        {
            int top = Math.Max(cuts[k], staff.Top - up - pad), bottom = Math.Min(cuts[k + 1], staff.Bottom + down + pad);
            return new ScoreRow(Trim(pixels, width, Span(staff, width), top, Math.Max(top + 1, bottom), ink), staff.Top, staff.Bottom);
        })];
    }

    // Notes, ledger lines and the clef touch the staff from above; a title sits apart, past a clear gap.
    private static int Above(byte[] pixels, int width, Staff staff, int limit, Func<byte, bool> ink)
    {
        var span = Span(staff, width);
        int quiet = (span.Right - span.Left) / 200, blank = Math.Max(3, (int)(1.5 * staff.Spacing)), run = 0, reach = 0;
        int most = (int)(5 * staff.Spacing);
        for (int d = 1; d <= most && staff.Top - d >= limit; d++)
        {
            if (InkIn(pixels, width, span, staff.Top - d, ink) > quiet) { reach = d; run = 0; }
            else if (++run >= blank) break;
        }
        return reach;
    }

    // Where a row's notes, lyrics and title can be: the staff's width and a little more. Everything is measured
    // inside it, so a background picture or a slide button beside the score counts as empty slide.
    private static (int Left, int Right) Span(Staff staff, int width)
    {
        int pad = Math.Max((int)(4 * staff.Spacing), width / 60);
        return (Math.Max(0, staff.Left - pad), Math.Min(width - 1, staff.Right + pad));
    }

    private static int InkIn(byte[] pixels, int width, (int Left, int Right) span, int y, Func<byte, bool> ink)
    {
        int count = 0;
        for (int x = span.Left, row = y * width; x <= span.Right; x++) if (ink(pixels[row + x])) count++;
        return count;
    }

    // A row ends after the lyrics under its staff when a clear gap follows (the next row, slide buttons, a footer),
    // at the last ink before that gap; at limit at the latest.
    private static int End(byte[] pixels, int width, int limit, Staff staff, Func<byte, bool> ink)
    {
        var span = Span(staff, width);
        int quiet = (span.Right - span.Left) / 200, blank = Math.Max(3, (int)(2 * staff.Spacing)), run = 0, last = staff.Bottom;
        bool lyrics = false;
        for (int y = staff.Bottom + (int)staff.Spacing; y < limit; y++)
        {
            if (InkIn(pixels, width, span, y, ink) > quiet) { lyrics = true; run = 0; last = y + 1; }
            else if (lyrics && ++run >= blank) return last;
        }
        return lyrics ? last : Math.Min(limit, staff.Bottom + (int)(2 * staff.Spacing)); // no lyrics: just the notes under the staff
    }

    // Left and right of the notes is empty slide; leaving it out makes the score larger on the camera.
    private static Int32Rect Trim(byte[] pixels, int width, (int Left, int Right) span, int top, int bottom, Func<byte, bool> ink)
    {
        int left = span.Right + 1, right = -1;
        for (int y = top; y < bottom; y++)
            for (int x = span.Left, row = y * width; x <= span.Right; x++)
                if (ink(pixels[row + x])) { if (x < left) left = x; if (x > right) right = x; }
        if (right < left) return new(span.Left, top, span.Right - span.Left + 1, bottom - top);
        int pad = width / 60;
        left = Math.Max(0, left - pad);
        right = Math.Min(width - 1, right + pad);
        return new(left, top, right - left + 1, bottom - top);
    }

    // The cut between two staves: the middle of the emptiest stretch between the lyrics above and the notes below.
    private static int Gap(byte[] pixels, int width, Staff above, Staff below, Func<byte, bool> ink)
    {
        int from = above.Bottom + (int)(2 * above.Spacing), to = below.Top - (int)below.Spacing;
        if (to <= from) return (above.Bottom + below.Top) / 2;
        var (a, b) = (Span(above, width), Span(below, width));
        var span = (Math.Min(a.Left, b.Left), Math.Max(a.Right, b.Right));
        var counts = new int[to - from];
        for (int y = from; y < to; y++) counts[y - from] = InkIn(pixels, width, span, y, ink);
        int quiet = counts.Min() + (span.Item2 - span.Item1) / 200;
        int bestStart = 0, bestLength = 0;
        for (int i = 0; i < counts.Length;)
        {
            if (counts[i] > quiet) { i++; continue; }
            int start = i;
            while (i < counts.Length && counts[i] <= quiet) i++;
            if (i - start >= bestLength) { bestStart = start; bestLength = i - start; } // the lower one on a tie
        }
        return from + bestStart + bestLength / 2;
    }
}
