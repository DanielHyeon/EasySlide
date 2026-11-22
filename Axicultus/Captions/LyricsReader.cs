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
        using var process = Process.Start(start) ?? throw new IOException("OCR 도우미를 시작할 수 없습니다.");
        var output = process.StandardOutput.ReadToEndAsync(cancellation);
        var error = process.StandardError.ReadToEndAsync(cancellation);
        try { await process.WaitForExitAsync(cancellation).WaitAsync(TimeSpan.FromMinutes(2), cancellation); }
        catch
        {
            try { process.Kill(true); } catch (InvalidOperationException) { }
            throw;
        }
        if (process.ExitCode == 3) throw new IOException("이 PC에 한국어 OCR이 설치되어 있지 않습니다. " + (await error).Trim());
        if (process.ExitCode != 0) throw new IOException($"OCR 도우미 오류 ({process.ExitCode}) · {(await error).Trim()}");
        return JsonSerializer.Deserialize<OcrPage[]>(await output) ?? [];
    }

    /// <summary>
    /// The lyrics of each row: the lines under its staff, without the title above it, the signs read inside it
    /// or what lies below the last row (the slide's buttons). A slide without staves gives one row of all its lines.
    /// </summary>
    internal static string[] RowLyrics(OcrPage page, ScoreRow[] rows)
    {
        var lines = page.Lines.Select(line => (Text: Clean(line), Center: line.Words.Length == 0 ? 0 : line.Words.Average(w => w.Y + w.Height / 2.0)))
            .Where(line => line.Text.Length > 0).OrderBy(line => line.Center).ToList();
        if (rows.Length == 0) return [string.Join("\n", lines.Select(line => line.Text))];
        return [.. rows.Select(row => string.Join("\n", lines
            .Where(line => line.Center > row.StaffBottom && line.Center < row.Area.Y + row.Area.Height)
            .Select(line => line.Text)))];
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
