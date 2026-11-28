using System;

namespace Axicultus.Core;

/// <summary>
/// The lines of stored lyrics as the lyrics display reads them (moved here from the WPF app's LyricsDisplayFormatter,
/// which uses them): the caption library's song matching (SongLyrics) reads a song the same way.
/// </summary>
public static class LyricLines
{
    // Raw lyrics made one form: line breaks (\r\n, \r to \n) and the chord marker; some data stores '»' (U+00BB) as
    // "Â»" (U+00C2 U+00BB). Every lyrics entry point (display text, verse pages, duplicate check, preview lines) uses it.
    public static string NormalizeText(string raw)
        => raw
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("Â»", "»", StringComparison.Ordinal);

    // The lines of the lyrics as the display reads them: without a leading [~...] notation block, each line cut
    // at its chord marker, and a [~chords] line empty.
    public static string[] ReadLines(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return [];
        }

        // Same first steps as ToDisplayText: normalise, then drop a leading [~...] notation block.
        var normalizedLyrics = NormalizeText(rawLyrics);
        var leading = normalizedLyrics.TrimStart('\n', ' ', '\t');
        if (leading.StartsWith("[~", StringComparison.Ordinal))
        {
            var end = leading.IndexOf(']');
            if (end > 1)
            {
                normalizedLyrics = leading[(end + 1)..];
            }
        }

        var lines = normalizedLyrics.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            // Text typed before a chord marker ends where the lyric ends; any other line is kept as stored,
            // because FrmMain reads a heading with a space around it as a line of the verse.
            var line = lines[i].Contains('»') ? StripInlineNotation(lines[i]).TrimEnd() : lines[i];
            lines[i] = IsMarkerOnlyLine(line) && line.TrimStart().StartsWith("[~", StringComparison.Ordinal) ? string.Empty : line;
        }

        return lines;
    }

    // The line without what follows its chord marker '»' (the line as it is when it has none). The marker is
    // expected normalized already (NormalizeText).
    public static string StripInlineNotation(string line)
    {
        var markerIndex = line.IndexOf('»'); // '»'
        return markerIndex < 0 ? line : line[..markerIndex];
    }

    // The whole line is one bracket token ([1], [Chorus], [~G D]): a writing marker.
    public static bool IsMarkerOnlyLine(string line)
    {
        var t = line.Trim();
        return t.Length >= 2
            && t[0] == '['
            && t[^1] == ']'
            && t.IndexOf(']', 1) == t.Length - 1; // one closing bracket, at the end (no text after it)
    }
}
