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

    /// <summary>One run of the OCR helper on some pictures: its exit code and what it wrote (a test puts a stand-in here).</summary>
    internal delegate Task<(int ExitCode, string Output, string Error)> HelperRun(IReadOnlyList<string> images, CancellationToken cancellation);

    // At most this many pictures per run: Windows OCR reads one in well under a second, so a run stays far inside BatchTime.
    internal const int BatchPictures = 40;
    // At most this many characters of picture arguments per run: half a Windows command line's 32,767, room left for the rest.
    internal const int BatchCharacters = 16_000;
    // The time limit of one run (the whole deck's limit before batches): a hung helper is ended and its pictures told unread.
    internal static readonly TimeSpan BatchTime = TimeSpan.FromMinutes(2);

    internal static Task<OcrPage[]> ReadAsync(IReadOnlyList<string> images, CancellationToken cancellation) =>
        ReadAsync(images, RunHelperAsync, BatchTime, cancellation);

    /// <summary>
    /// The pictures read in batches, one run of the helper each with its own time limit. A batch that fails (the helper
    /// not started, out of time, an error exit, output that is no page list) gives its pictures as pages with
    /// <see cref="OcrPage.Error"/> and the next batch is read, unless it is the second batch in a row out of time (then
    /// the batches left are not run, their pictures unread); when every batch fails, the first failure is thrown (as
    /// one run's failure was). No Korean OCR (exit code 3) ends the reading at once, as does the cancellation.
    /// </summary>
    internal static async Task<OcrPage[]> ReadAsync(IReadOnlyList<string> images, HelperRun run, TimeSpan batchTime, CancellationToken cancellation)
    {
        var pages = new List<OcrPage>();
        Exception? firstFailure = null;
        bool anyRead = false;
        int timeouts = 0; // batches out of time in a row
        var batches = Batches(images).ToList();
        for (int b = 0; b < batches.Count; b++)
        {
            var batch = batches[b];
            cancellation.ThrowIfCancellationRequested();
            Exception? failure = null;
            using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            limit.CancelAfter(batchTime);
            (int ExitCode, string Output, string Error)? result = null;
            try { result = await run(batch, limit.Token); }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { failure = new TimeoutException(); }
            catch (Exception error) when (error is IOException or System.ComponentModel.Win32Exception
                or InvalidOperationException or UnauthorizedAccessException) { failure = error; }
            if (result is { } ran)
            {
                var (exitCode, output, message) = ran;
                // (every batch would say the same)
                if (exitCode == 3) throw new NoKoreanOcrException(CaptionText.Format("CaptionConsole", "OcrNoKorean", message.Trim()));
                if (exitCode != 0) failure = new IOException(CaptionText.Format("CaptionConsole", "OcrError", exitCode, message.Trim()));
                else
                    try
                    {
                        pages.AddRange(JsonSerializer.Deserialize<OcrPage[]>(output) ?? []);
                        anyRead = true;
                    }
                    catch (JsonException error) { failure = error; }
            }
            timeouts = failure is TimeoutException ? timeouts + 1 : 0;
            if (failure == null) continue;
            Debug.WriteLine($"OCR batch of {batch.Count} pictures failed: {failure.Message}");
            firstFailure ??= failure;
            // A helper that hung on two batches in a row hangs on the next: the batches left are not run, their pictures
            // unread (the wait is two batch limits whatever the deck's size, a048 design 8).
            var unread = timeouts == 2 ? batches.Skip(b).SelectMany(rest => rest) : batch;
            pages.AddRange(unread.Select(image => new OcrPage(image, failure.Message, [])));
            if (timeouts == 2) break;
        }
        if (!anyRead && firstFailure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(firstFailure);
        return [.. pages];
    }

    /// <summary>The PC has no Korean OCR (the helper's exit code 3): a notice of its own, no slide counted unread.</summary>
    internal sealed class NoKoreanOcrException(string message) : IOException(message);

    // The pictures in order, cut where a batch would pass BatchPictures or BatchCharacters (a picture whose path alone
    // passes it is a batch of its own).
    internal static IEnumerable<IReadOnlyList<string>> Batches(IReadOnlyList<string> images)
    {
        var batch = new List<string>();
        int characters = 0;
        foreach (var image in images)
        {
            int length = image.Length + 3; // the space before it and the quotes around it
            if (batch.Count > 0 && (batch.Count == BatchPictures || characters + length > BatchCharacters))
            {
                yield return batch;
                batch = [];
                characters = 0;
            }
            batch.Add(image);
            characters += length;
        }
        if (batch.Count > 0) yield return batch;
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunHelperAsync(IReadOnlyList<string> images, CancellationToken cancellation)
    {
        var start = new ProcessStartInfo(HelperPath)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
        };
        start.ArgumentList.Add(Language);
        foreach (var image in images) start.ArgumentList.Add(image);
        using var process = Process.Start(start) ?? throw new IOException(CaptionText.Get("CaptionConsole", "OcrStartFailed"));
        var output = process.StandardOutput.ReadToEndAsync(cancellation);
        var error = process.StandardError.ReadToEndAsync(cancellation);
        try { await process.WaitForExitAsync(cancellation); }
        catch
        {
            // (whatever ending the helper throws, the time limit or the cancellation goes up as it was, design 18)
            try { process.Kill(true); }
            catch (Exception notEnded) { Debug.WriteLine($"OCR helper not ended: {notEnded.Message}"); }
            throw;
        }
        return (process.ExitCode, await output, await error);
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
