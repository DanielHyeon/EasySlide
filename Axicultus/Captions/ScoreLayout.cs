using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// Finds the rows of a score slide: each staff (five long horizontal lines) with the lyrics under it.
/// The first row also takes what is above the first staff (the title); the last takes the rest of the slide.
/// </summary>
/// <summary>A row of a score slide, and where its staff lies (the lyrics are under it, the title above).</summary>
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
        var cuts = new List<int> { 0 };
        for (int k = 1; k < staves.Count; k++)
            cuts.Add(Gap(pixels, width, staves[k - 1], staves[k], ink));
        cuts.Add(End(pixels, width, height, staves[^1], ink));
        return [.. staves.Select((staff, k) =>
            new ScoreRow(Trim(pixels, width, Span(staff, width), cuts[k], Math.Max(cuts[k] + 1, cuts[k + 1]), ink), staff.Top, staff.Bottom))];
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

    // The last row ends after the lyrics under its staff when a clear gap follows (slide buttons, a footer).
    private static int End(byte[] pixels, int width, int height, Staff last, Func<byte, bool> ink)
    {
        var span = Span(last, width);
        int quiet = (span.Right - span.Left) / 200, blank = Math.Max(3, (int)(2 * last.Spacing)), run = 0;
        bool lyrics = false;
        for (int y = last.Bottom + (int)last.Spacing; y < height; y++)
        {
            if (InkIn(pixels, width, span, y, ink) > quiet) { lyrics = true; run = 0; }
            else if (lyrics && ++run >= blank) return Math.Min(height, y + 1);
        }
        return height;
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
