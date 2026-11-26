using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Axicultus.Module;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Library;

/// <summary>
/// Chords in the song editor, as FrmEditItem shows them: a chord line ("G     C »") above the lyric line it
/// belongs to. MSC stores each chord with the index of the lyric character it stands over, so the text is
/// translated with the widths of the editor font (gfLyrics.CombineLyricsAndNotations / ReadEditorNotations).
/// </summary>
internal sealed class SongEditorChords
{
    private const double Tolerance = 0.01;
    private const int MaxChordLineLength = 2000;

    // Chord line and lyric line as Combine last wrote them, with the stored records they came from: a line the
    // operator did not touch is saved with exactly the positions it was loaded with, whatever the font. Two
    // records can give the same pair (a repeated lyric line with chords too close to show apart); they are
    // kept in their order.
    private readonly Dictionary<(string ChordLine, string Lyric), List<string>> _stored = [];

    // Lines the operator wrote, measured once: the panes are read again on every edit.
    private readonly Dictionary<(string ChordLine, string Lyric), string> _measured = [];

    /// <summary>Width of a text in the editor font. The default counts characters (a fixed-width font).</summary>
    public Func<string, double> Measure { get; set; } = static text => text.Length;

    /// <summary>Lyrics with a chord line above every line that has a chord record.</summary>
    public string Combine(string lyrics, string notations)
    {
        _stored.Clear();
        if (string.IsNullOrEmpty(notations) || string.IsNullOrEmpty(lyrics))
        {
            return lyrics;
        }

        string[] lines = lyrics.Split('\n');
        int lineCount = lyrics.EndsWith('\n') ? lines.Length - 1 : lines.Length;
        var byLine = new Dictionary<int, string>();
        string remaining = notations;
        while (TryReadRecord(ref remaining, out int lineNumber, out string chords) && lineNumber >= 0)
        {
            if (lineNumber < lineCount)
            {
                byLine[lineNumber] = chords;
            }
        }

        var result = new StringBuilder(lyrics.Length + notations.Length);
        for (int row = 0; row < lineCount; row++)
        {
            if (byLine.TryGetValue(row, out string? fields) && PlaceChords(lines[row], fields) is { Length: > 0 } placed)
            {
                string chordLine = placed + " »";
                if (!_stored.TryGetValue((chordLine, lines[row]), out var records))
                {
                    _stored[(chordLine, lines[row])] = records = [];
                }

                records.Add(fields);
                result.Append(chordLine).Append('\n');
            }

            result.Append(lines[row]);
            if (row + 1 < lineCount)
            {
                result.Append('\n');
            }
        }

        return result.ToString();
    }

    /// <summary>Editor text as lyrics only and the chord records of its chord lines, by lyric line.</summary>
    public (string Lyrics, string Notations) Read(string text)
    {
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (!text.Contains('»'))
        {
            return (text, string.Empty);
        }

        string[] lines = text.Split('\n');
        var lyrics = new List<string>(lines.Length);
        var records = new StringBuilder();
        var shown = new Dictionary<(string, string), int>();
        for (int index = 0; index + 1 < lines.Length; index++)
        {
            if (IsShownChordLine(lines, index))
            {
                shown[(lines[index], lines[index + 1])] = shown.GetValueOrDefault((lines[index], lines[index + 1])) + 1;
                index++;
            }
        }

        var seen = new Dictionary<(string, string), int>();
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];
            if (IsShownChordLine(lines, index))
            {
                string lyric = lines[++index];
                string? fields;
                // Lines that look the same can come from different records. They are told apart by their order,
                // which only holds while all of them are still there; otherwise the line is read as it is shown.
                if (_stored.TryGetValue((line, lyric), out var stored)
                    && (stored.Count == shown[(line, lyric)] || stored.TrueForAll(record => record == stored[0])))
                {
                    int occurrence = seen.GetValueOrDefault((line, lyric));
                    seen[(line, lyric)] = occurrence + 1;
                    fields = stored[Math.Min(occurrence, stored.Count - 1)];
                }
                else if (!_measured.TryGetValue((line, lyric), out fields))
                {
                    _measured[(line, lyric)] = fields = MeasureChords(line, lyric);
                }

                if (fields.Length > 0)
                {
                    records.Append('(').Append(lyrics.Count).Append(';').Append(fields).Append(')');
                }

                line = lyric;
            }

            // A marker that is not on a chord line stays in the text; ChordLineProblem stops the save.
            lyrics.Add(line);
        }

        return (string.Join('\n', lyrics), records.ToString());
    }

    // A chord line the operator can write, or one Combine wrote itself above a line a typed chord line cannot
    // stand over (an empty line, a lyric line that holds the marker): the stored record is saved again.
    private bool IsShownChordLine(string[] lines, int index)
        => IsChordLine(lines, index) || (index + 1 < lines.Length && _stored.ContainsKey((lines[index], lines[index + 1])));

    /// <summary>
    /// A chord line ends with the marker and has a lyric line right below it (gfLyrics.ReadEditorNotations).
    /// FrmEditItem also reads chords from a line with the marker in its middle; here such a line is lyric text
    /// (ChordLineProblem refuses it), so "lyric » G C" is never taken for the chords of the next line.
    /// </summary>
    internal static bool IsChordLine(string[] lines, int index)
    {
        string line = lines[index].TrimEnd();
        return line.Length > 0 && line.IndexOf('»') == line.Length - 1 && index + 1 < lines.Length
            && lines[index + 1].Length > 0 && !lines[index + 1].Contains('»');
    }

    /// <summary>Why a line with the chord marker cannot be saved as a chord line; null when it can.</summary>
    internal static string? ChordLineProblem(string[] lines, int index)
    {
        if (!IsChordLine(lines, index))
        {
            return UiText.Get("SongEditorWindow", "ChordLineMisplaced");
        }

        foreach (string chord in lines[index].TrimEnd()[..^1].Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            // The record format gives ';' and ')' a meaning of their own.
            if (chord.Contains(';') || ChordNotation.FindRecordEnd(chord + ")") != chord.Length)
            {
                return UiText.Format("SongEditorWindow", "ChordBadCharacters", chord);
            }
        }

        return null;
    }

    /// <summary>
    /// Puts a chord over the caret (gfLyrics.InsertChordAboveCurrentLine): into the chord line above the lyric
    /// line, which is created when there is none, or at the caret when the caret is in a chord line.
    /// </summary>
    public string InsertChord(string text, ref int caret, string chord)
    {
        caret = Math.Clamp(caret, 0, text.Length);
        int lineStart = caret == 0 ? 0 : text.LastIndexOf('\n', caret - 1) + 1;
        int lineEnd = text.IndexOf('\n', caret);
        lineEnd = lineEnd < 0 ? text.Length : lineEnd;
        int contentEnd = lineEnd > lineStart && text[lineEnd - 1] == '\r' ? lineEnd - 1 : lineEnd;
        int marker = text.IndexOf('»', lineStart, contentEnd - lineStart);
        if (marker >= 0)
        {
            int at = Math.Min(caret, marker);
            string inserted = (at > lineStart && text[at - 1] != ' ' ? " " : string.Empty) + chord + " ";
            caret = at + inserted.Length;
            return text.Insert(at, inserted);
        }

        double target = Measure(text[lineStart..Math.Min(caret, contentEnd)]);
        int previousStart = lineStart < 2 ? 0 : text.LastIndexOf('\n', lineStart - 2) + 1;
        int previousEnd = lineStart == 0 ? 0 : lineStart - 1;
        previousEnd = previousEnd > previousStart && text[previousEnd - 1] == '\r' ? previousEnd - 1 : previousEnd;
        int previousMarker = lineStart == 0 ? -1 : text.IndexOf('»', previousStart, previousEnd - previousStart);
        if (previousMarker < 0)
        {
            string chordLine = PlaceOver(string.Empty, target, chord) + " »" + (text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n");
            caret += chordLine.Length;
            return text.Insert(lineStart, chordLine);
        }

        string chords = PlaceOver(text[previousStart..previousMarker], target, chord) + " ";
        caret += chords.Length - (previousMarker - previousStart);
        return string.Concat(text.AsSpan(0, previousStart), chords, text.AsSpan(previousMarker));
    }

    // The chords of a chord line with one more chord at the given width, never inside another chord.
    private string PlaceOver(string chords, double target, string chord)
    {
        var line = new StringBuilder(chords.TrimEnd());
        int column = 0;
        while (column < line.Length && Measure(line.ToString(0, column)) < target - Tolerance)
        {
            column++;
        }

        while (column > 0 && column < line.Length && line[column - 1] != ' ')
        {
            column++;
        }

        if (column == line.Length)
        {
            PadTo(line, target);
            return line.Append(chord).ToString();
        }

        line.Insert(column, chord + " ");
        // Take the spaces back that the new chord used, so the chords to its right stay where they were.
        int after = column + chord.Length + 1;
        for (int removable = chord.Length + 1; removable > 0 && after < line.Length && line[after] == ' '; removable--)
        {
            line.Remove(after, 1);
        }

        return line.ToString();
    }

    private string PlaceChords(string lyric, string fields)
    {
        var line = new StringBuilder();
        string rest = fields;
        while (rest.Length > 0)
        {
            string chord = TakeField(ref rest);
            int position = int.TryParse(TakeField(ref rest), NumberStyles.Integer, CultureInfo.InvariantCulture, out int stored)
                ? Math.Clamp(stored, 0, lyric.Length)
                : 0;
            if (chord.Length == 0)
            {
                continue;
            }

            PadTo(line, Measure(lyric[..position]));
            line.Append(chord);
        }

        return line.ToString();
    }

    // The fewest spaces that bring the line to the given width, and one at least after a chord.
    private void PadTo(StringBuilder line, double target)
    {
        double width = Measure(line.ToString());
        double space = Measure(line + " ") - width;
        if (width < target - Tolerance && space > 0)
        {
            line.Append(' ', Math.Min((int)Math.Ceiling((target - Tolerance - width) / space), MaxChordLineLength));
        }

        // The estimate takes every space for the same width; the font has the last word.
        while (line.Length < MaxChordLineLength && Measure(line.ToString()) < target - Tolerance)
        {
            line.Append(' ');
        }

        while (line.Length > 0 && line[^1] == ' ' && Measure(line.ToString(0, line.Length - 1)) >= target - Tolerance)
        {
            line.Length--;
        }

        if (line.Length > 0 && line[^1] != ' ')
        {
            line.Append(' ');
        }
    }

    // Each chord with the index of the last lyric character that starts at or left of it.
    private string MeasureChords(string chordLine, string lyric)
    {
        var fields = new StringBuilder();
        int offset = 0;
        for (int start = 0; start < chordLine.Length;)
        {
            if (chordLine[start] is ' ' or '»')
            {
                start++;
                continue;
            }

            int length = chordLine.AsSpan(start).IndexOfAny(' ', '»');
            if (length < 0)
            {
                break; // White space other than a space after the marker (a tab, a no-break space): no chord.
            }

            double x = Measure(chordLine[..start]);
            while (offset < lyric.Length && Measure(lyric[..(offset + 1)]) <= x + Tolerance)
            {
                offset++;
            }

            fields.Append(chordLine, start, length).Append(';').Append(offset).Append(';');
            start += length;
        }

        return fields.ToString();
    }

    /// <summary>The next "(line;chord;position;...)" record of MSC (gfLyrics.TryReadNotationRecord).</summary>
    internal static bool TryReadRecord(ref string remaining, out int lineNumber, out string chords)
    {
        lineNumber = -1;
        chords = string.Empty;
        int opening = remaining.IndexOf('(');
        if (opening < 0)
        {
            return false;
        }

        ReadOnlySpan<char> record = remaining.AsSpan(opening + 1);
        int closing = ChordNotation.FindRecordEnd(record);
        if (closing < 0)
        {
            return false;
        }

        string rest = record[(closing + 1)..].ToString();
        record = record[..closing];
        if (!record.IsEmpty)
        {
            int separator = record.IndexOf(';');
            // A line label that is not a number counts as line 0, an empty record ends the list.
            lineNumber = int.TryParse(separator < 0 ? record : record[..separator], out int parsed) ? parsed : 0;
            if (separator >= 0)
            {
                chords = record[(separator + 1)..].ToString();
            }
        }

        remaining = rest;
        return true;
    }

    /// <summary>The chord names of each lyric line that has a record, in order, for a preview.</summary>
    internal static Dictionary<int, string> NamesByLine(string notations)
    {
        var names = new Dictionary<int, string>();
        string remaining = notations;
        while (TryReadRecord(ref remaining, out int lineNumber, out string fields) && lineNumber >= 0)
        {
            var chords = new List<string>();
            while (fields.Length > 0)
            {
                chords.Add(TakeField(ref fields));
                TakeField(ref fields);
            }

            names[lineNumber] = string.Join("  ", chords);
        }

        return names;
    }

    private static string TakeField(ref string fields)
    {
        if (fields.Length == 0)
        {
            return "-1"; // The position is missing after the last chord.
        }

        int separator = fields.IndexOf(';');
        string field = separator < 0 ? fields : fields[..separator];
        fields = separator < 0 ? string.Empty : fields[(separator + 1)..];
        return field;
    }

    // ── Transposing, as the WinForms editor does it (gfMusic.cs) ─────────────────────────────────

    private readonly record struct Pitch(string Flat, string Sharp, bool MajorUsesSharps, bool MinorUsesSharps);

    // Starts on G like the WinForms chord menu. A key also chooses how the chords in that key are spelled.
    private static readonly Pitch[] Scale =
    [
        new("G", "G", true, false),
        new("Ab", "G#", false, true),
        new("A", "A", true, true),
        new("Bb", "A#", false, false),
        new("B", "B", true, true),
        new("C", "C", true, false),
        new("Db", "C#", false, true),
        new("D", "D", true, false),
        new("Eb", "D#", false, true),
        new("E", "E", true, true),
        new("F", "F", false, false),
        new("Gb", "F#", true, true),
    ];

    /// <summary>The chord names FrmEditItem offers in its chords menu: its top level, then the four sub menus.</summary>
    internal static IReadOnlyList<(string Group, IReadOnlyList<string> Chords)> Menu { get; } = BuildMenu();

    private static (string, IReadOnlyList<string>)[] BuildMenu()
    {
        List<string> major = [], minor = [], minorSeventh = [], seventh = [], bass = [];
        foreach (var pitch in Scale)
        {
            minor.Add(pitch.Flat + "m");
            minorSeventh.Add(pitch.Flat + "m7");
            seventh.Add(pitch.Sharp + "7");
            bass.Add("/" + pitch.Sharp);
            major.Add(pitch.Sharp);
            if (pitch.Flat != pitch.Sharp)
            {
                minor.Add(pitch.Sharp + "m");
                minorSeventh.Add(pitch.Sharp + "m7");
                seventh.Add(pitch.Flat + "7");
                major.Add(pitch.Flat);
            }
        }

        return [(string.Empty, major), ("Minor", minor), ("Minor 7th", minorSeventh), ("Major 7th", seventh), ("Bass", bass)];
    }

    /// <summary>Semitones from a capo fret or pitch, wrapped into one octave.</summary>
    internal static int Shift(int index, int semitones)
    {
        int pitch = (int)(((long)index + semitones) % Scale.Length);
        return pitch < 0 ? pitch + Scale.Length : pitch;
    }

    /// <summary>
    /// Moves a key and tells how its chords are spelled: 1 sharps, 0 flats, -1 when the key is not one of
    /// the listed keys (the key is then left alone and the direction decides).
    /// </summary>
    internal static int TransposeKey(ref string key, int steps)
    {
        if (key.Length == 0)
        {
            return steps > 0 ? 1 : 0;
        }

        int index = ReadRoot(key, out _);
        if (index < 0)
        {
            return -1;
        }

        bool minor = key.EndsWith('m');
        if (key != KeyName(Scale[index], minor))
        {
            return -1;
        }

        var target = Scale[Shift(index, steps)];
        key = KeyName(target, minor);
        return (minor ? target.MinorUsesSharps : target.MajorUsesSharps) ? 1 : 0;

        static string KeyName(Pitch pitch, bool minor)
            => minor ? (pitch.MinorUsesSharps ? pitch.Sharp : pitch.Flat) + "m" : pitch.MajorUsesSharps ? pitch.Sharp : pitch.Flat;
    }

    /// <summary>All chords of MSC records moved by the given semitones; positions are kept.</summary>
    internal static string Transpose(string notations, int steps, int spelling)
    {
        var result = new StringBuilder();
        string remaining = notations;
        while (TryReadRecord(ref remaining, out int lineNumber, out string fields) && lineNumber >= 0)
        {
            result.Append('(').Append(lineNumber).Append(';');
            while (fields.Length > 0)
            {
                string chord = TakeField(ref fields);
                result.Append(steps == 0 ? chord : TransposeChord(chord, steps, spelling)).Append(';').Append(TakeField(ref fields)).Append(';');
            }

            result.Append(')');
        }

        return result.ToString();
    }

    private static string TransposeChord(ReadOnlySpan<char> chord, int steps, int spelling)
    {
        bool useSharps = spelling switch { 0 => false, 1 => true, _ => steps > 0 };
        int slash = ChordNotation.FindBassSeparator(chord);
        if (slash < 0)
        {
            return TransposePart(chord, steps, useSharps);
        }

        var bass = chord[(slash + 1)..];
        int nextSlash = ChordNotation.FindBassSeparator(bass);
        if (nextSlash >= 0)
        {
            bass = bass[..nextSlash];
        }

        return string.Concat(TransposePart(chord[..slash], steps, useSharps), "/", TransposePart(bass, steps, useSharps));
    }

    private static string TransposePart(ReadOnlySpan<char> chord, int steps, bool useSharps)
    {
        int prefix = !chord.IsEmpty && chord[0] is '[' or '{' ? 1 : 0;
        int index = ReadRoot(chord[prefix..], out int length);
        if (index < 0)
        {
            return chord.ToString();
        }

        var target = Scale[Shift(index, steps)];
        return string.Concat(chord[..prefix], useSharps ? target.Sharp : target.Flat, chord[(prefix + length)..]);
    }

    private static int ReadRoot(ReadOnlySpan<char> chord, out int length)
    {
        length = 0;
        if (chord.IsEmpty)
        {
            return -1;
        }

        int natural = chord[0] switch { 'G' => 0, 'A' => 2, 'B' => 4, 'C' => 5, 'D' => 7, 'E' => 9, 'F' => 10, _ => -1 };
        if (natural < 0)
        {
            return -1;
        }

        length = 1;
        if (chord.Length >= 2 && chord[1] is '#' or 'b')
        {
            int adjacent = Shift(natural, chord[1] == '#' ? 1 : -1);
            // A spelling the scale does not have, such as E#, keeps the sign as part of the chord's suffix.
            if (Scale[adjacent].Flat != Scale[adjacent].Sharp)
            {
                length = 2;
                return adjacent;
            }
        }

        return natural;
    }
}
