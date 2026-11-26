using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Captions;

internal sealed record OcrWord(string Text, int X, int Y, int Width, int Height);
internal sealed record OcrLine(OcrWord[] Words);
internal sealed record OcrPage(string File, string? Error, OcrLine[] Lines);

/// <summary>
/// Lyrics written inside slide pictures, read by the OCR helper (ocr\Axicultus.Ocr.exe, Windows OCR).
/// The helper is a separate program because Windows.Media.Ocr needs the Windows 10 SDK target.
/// </summary>
internal static partial class LyricsReader
{
    internal const string Language = "ko";
    internal static string HelperPath => Path.Combine(AppContext.BaseDirectory, "ocr", "Axicultus.Ocr.exe");
    internal static bool Available => File.Exists(HelperPath);

    internal static async Task<OcrPage[]> ReadAsync(IReadOnlyList<string> images, CancellationToken cancellation)
    {
        var start = new ProcessStartInfo(HelperPath)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
        };
        start.ArgumentList.Add(Language);
        foreach (var image in images) start.ArgumentList.Add(image);
        using var process = Process.Start(start) ?? throw new IOException(UiText.Get("CaptionConsole", "OcrStartFailed"));
        var output = process.StandardOutput.ReadToEndAsync(cancellation);
        var error = process.StandardError.ReadToEndAsync(cancellation);
        try { await process.WaitForExitAsync(cancellation).WaitAsync(TimeSpan.FromMinutes(2), cancellation); }
        catch
        {
            try { process.Kill(true); } catch (InvalidOperationException) { }
            throw;
        }
        if (process.ExitCode == 3) throw new IOException(UiText.Format("CaptionConsole", "OcrNoKorean", (await error).Trim()));
        if (process.ExitCode != 0) throw new IOException(UiText.Format("CaptionConsole", "OcrError", process.ExitCode, (await error).Trim()));
        return JsonSerializer.Deserialize<OcrPage[]>(await output) ?? [];
    }

    /// <summary>
    /// The lyrics of each row (in the order the rows are given): the lines under its staff, without the title above
    /// the first staff or the signs read inside a staff. A line outside every row's area (a render's rows can end
    /// above their lyrics) goes to the nearest staff above it when it lies within <see cref="LyricsReach"/> staff
    /// heights of it; when it sits above the next staff in that row's area (a section name) or under the last row's
    /// area (a button), only if the row above read no lyrics inside its own area. Farther down it is a label (verse
    /// buttons, the page number), not lyrics. Pieces of one line are joined after they are given to a row.
    /// A slide without staves gives one row of all its lines.
    /// </summary>
    internal static string[] RowLyrics(OcrPage page, ScoreRow[] rows)
    {
        var pieces = Pieces(page);
        if (rows.Length == 0) return [string.Join("\n", Join(pieces))];
        int[] order = [.. Enumerable.Range(0, rows.Length).OrderBy(i => rows[i].StaffTop)];
        var sorted = order.Select(i => rows[i]).ToArray();
        var lyrics = sorted.Select(_ => new List<Piece>()).ToArray();
        var outside = new List<Piece>();
        foreach (var piece in pieces)
        {
            double center = piece.Center;
            if (sorted.Any(row => center >= row.StaffTop && center <= row.StaffBottom)) continue; // a sign inside a staff
            // Where bands overlap, the row whose staff is nearest above.
            int k = Array.FindLastIndex(sorted, row => center > row.StaffBottom && center < row.Area.Y + row.Area.Height);
            if (k >= 0) lyrics[k].Add(piece);
            else outside.Add(piece);
        }
        int last = sorted.Length - 1;
        bool[] own = [.. lyrics.Select(row => row.Count > 0)]; // the rows that read lyrics inside their own area
        foreach (var piece in outside)
        {
            double center = piece.Center;
            int above = Array.FindLastIndex(sorted, row => row.StaffBottom < center);
            if (above < 0 || center - sorted[above].StaffBottom > LyricsReach * (sorted[above].StaffBottom - sorted[above].StaffTop)) continue;
            // Above the next staff in its area (a section name) or under the last row's area (a button), a line is that
            // row's lyrics only when the row read none inside its own area (its area ended above them).
            bool label = sorted.Any(row => center >= row.Area.Y && center < row.StaffTop);
            bool below = above == last && center >= sorted[last].Area.Y + sorted[last].Area.Height;
            if ((label || below) && own[above]) continue;
            lyrics[above].Add(piece);
        }
        var result = new string[rows.Length];
        for (int s = 0; s < sorted.Length; s++) result[order[s]] = string.Join("\n", Join(lyrics[s]));
        return result;
    }

    /// <summary>How far under its staff a line outside every row's area is still that staff's lyrics, in staff heights.</summary>
    internal const double LyricsReach = 1.5;

    /// <summary>How far apart side by side two pieces of one line may be, in letter heights (columns farther apart stay apart).</summary>
    internal const double PieceGap = 6;

    private sealed record Piece(string Text, int Left, int Right, double Center, double Height);

    // The read lines with text left after cleaning, each with the extent and the (median) height of its letters
    // (a hold dash read as a word of its own is no letter).
    private static List<Piece> Pieces(OcrPage page) => [.. page.Lines.Where(line => line.Words.Length > 0).Select(line =>
        {
            var words = line.Words.Where(w => w.Text.Any(char.IsLetterOrDigit)).ToArray();
            if (words.Length == 0) words = line.Words;
            return new Piece(Clean(line), words.Min(w => w.X), words.Max(w => w.X + w.Width),
                words.Average(w => w.Y + w.Height / 2.0), Median(words.Select(w => (double)w.Height)));
        }).Where(piece => piece.Text.Length > 0)];

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.Order().ToArray();
        return sorted.Length % 2 == 1 ? sorted[sorted.Length / 2] : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;
    }

    // The lines top to bottom: pieces read at nearly the same height (centers within half the smaller letter height)
    // and close side by side (a gap of at most PieceGap letter heights) are one line, joined left to right.
    private static IEnumerable<string> Join(List<Piece> pieces)
    {
        var line = Enumerable.Range(0, pieces.Count).ToArray();
        int Find(int i) => line[i] == i ? i : line[i] = Find(line[i]);
        for (int i = 0; i < pieces.Count; i++)
            for (int j = i + 1; j < pieces.Count; j++)
            {
                var (a, b) = (pieces[i], pieces[j]);
                double height = Math.Min(a.Height, b.Height);
                if (Math.Abs(a.Center - b.Center) <= height / 2 && Math.Max(a.Left, b.Left) - Math.Min(a.Right, b.Right) <= PieceGap * height)
                    line[Find(i)] = Find(j);
            }
        return Enumerable.Range(0, pieces.Count).GroupBy(Find).Select(group => group.Select(i => pieces[i]).ToList())
            .OrderBy(group => group.Average(piece => piece.Center))
            .Select(group => string.Join(" ", group.OrderBy(piece => piece.Left).Select(piece => piece.Text)));
    }

    // "받으-소-서---" is one word held over several notes; notes read as letters have no Hangul and are dropped.
    internal static string Clean(OcrLine line)
    {
        var text = Spaces().Replace(Holds().Replace(string.Join(" ", line.Words.Select(w => w.Text)), ""), " ").Trim();
        bool hangul = text.Any(c => c is >= '가' and <= '힣');
        int letters = text.Count(char.IsLetter);
        return hangul || (Word().IsMatch(text) && letters >= text.Count(c => !char.IsWhiteSpace(c)) * 0.7) ? text : "";
    }

    [GeneratedRegex(@"\p{L}{3,}")] private static partial Regex Word();
    [GeneratedRegex("[-–—~_]+")] private static partial Regex Holds();
    [GeneratedRegex(@"\s{2,}")] private static partial Regex Spaces();
}
