using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Library;

/// <summary>The two lyric panes of the song editor: region 1 and region 2, each with its own chord records.</summary>
internal sealed record SongEditorRegions(string Text1, string Notations1, string Text2, string Notations2);

/// <summary>
/// What FrmEditItem does with the lyric text of a song, without the controls: split the stored LYRICS/MSC
/// into the two panes (gfLyrics.ExtractLyrics), merge them back on save (gfLyrics.Merge_Songs) and the
/// checks it runs before saving (FrmEditItem.ValidateTitles/ValidateSequence/ValidateContents).
/// </summary>
internal static class SongEditorLyrics
{
    private const int Region2 = 150;
    private const string Region2Symbol = "[region 2]";

    // Verse codes of the WinForms headings (gfConfig.InitializeVerseHeadings). A heading is a whole line, exact case.
    private static readonly Dictionary<string, int> Headings = BuildHeadings();

    private static Dictionary<string, int> BuildHeadings()
    {
        var headings = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int verse = 1; verse <= 99; verse++)
        {
            headings.Add(Symbol(verse), verse);
        }

        foreach (int verse in (int[])[0, 102, 100, 103, 111, 112, 101, Region2])
        {
            headings.Add(Symbol(verse), verse);
        }

        return headings;
    }

    internal static string Symbol(int verse) => verse switch
    {
        0 => "[chorus]",
        100 => "[bridge]",
        101 => "[ending]",
        102 => "[chorus 2]",
        103 => "[bridge 2]",
        111 => "[prechorus]",
        112 => "[prechorus 2]",
        Region2 => Region2Symbol,
        _ => "[" + verse.ToString(CultureInfo.InvariantCulture) + "]",
    };

    /// <summary>Stored LYRICS and MSC as the two editor panes show them (lyrics only; chord records per pane line).</summary>
    internal static SongEditorRegions Split(string? lyrics, string? notations)
    {
        string text = (lyrics ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal);
        bool separateRegions = IsSeparateRegionFormat(text);
        var first = new StringBuilder();
        var second = new StringBuilder();
        var firstNotes = new StringBuilder();
        var secondNotes = new StringBuilder();
        int firstLine = -1, secondLine = -1, lineIndex = -1;
        bool inSecondRegion = false;
        string verseHeading = string.Empty;
        var records = new NotationCursor(notations);
        foreach (string line in text.Split('\n'))
        {
            lineIndex++;
            string chords = records.FieldsFor(lineIndex);
            bool isHeading = Headings.TryGetValue(line, out int heading);
            if (isHeading && heading == Region2)
            {
                inSecondRegion = true;
                if (!separateRegions && verseHeading.Length > 0)
                {
                    second.Append(verseHeading).Append('\n');
                    secondLine++;
                }

                continue;
            }

            if (isHeading)
            {
                verseHeading = line;
                // In the usual format each verse starts in region 1 again.
                if (!separateRegions)
                {
                    inSecondRegion = false;
                }
            }

            int regionLine = inSecondRegion ? ++secondLine : ++firstLine;
            (inSecondRegion ? second : first).Append(line).Append('\n');
            if (chords.Length > 0)
            {
                (inSecondRegion ? secondNotes : firstNotes)
                    .Append('(').Append(regionLine).Append(';').Append(chords).Append(')');
            }
        }

        return new SongEditorRegions(Finish(first), firstNotes.ToString(), Finish(second), secondNotes.ToString());

        static string Finish(StringBuilder region) => region.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>LYRICS and MSC as FrmEditItem saves the two panes (lyrics only, "\n" lines, chord records by merged line).</summary>
    internal static (string Lyrics, string Notations) Merge(string text1, string notations1, string text2, string notations2)
    {
        var first = new Region(text1, notations1);
        var second = new Region(text2, notations2);
        var merged = new List<(string Text, string Chords)>();
        bool keepFirstHeading = false;
        // The verses in the order of the first pane, then those only the second pane has, in its order. A heading
        // of the first pane places its verse even without lines under it (the lines may all be in the second
        // pane). An empty pane still lists one empty line as verse 1, which is not a verse of the song.
        var verses = new List<int>(first.Text.Length > 0 ? first.Order : []);
        if (second.Text.Length > 0)
        {
            verses.AddRange(second.Sequence.Except(verses).ToList());
        }

        foreach (int verse in verses)
        {
            // Nothing but empty lines in the second pane gets no region marker: a single marker right above a
            // heading reads back as the form that keeps all of region 2 after one marker.
            bool inSecond = second.Text.Length > 0 && second.LinesOf(verse).Any();
            if (!first.Present.Contains(verse) && !inSecond)
            {
                continue; // A heading without lines in either pane is not stored.
            }

            // The heading of the first verse is stored when a pane has it as a line of its own; a symbol inside
            // a line ("[1]  ", "[1] text") is lyric text.
            if (merged.Count == 0 && (first.Headed.Contains(verse) || second.Headed.Contains(verse)))
            {
                keepFirstHeading = true;
            }

            merged.Add((Symbol(verse), string.Empty));
            merged.AddRange(first.LinesOf(verse));
            if (inSecond)
            {
                merged.Add((Region2Symbol, string.Empty));
                merged.AddRange(second.LinesOf(verse));
            }
        }

        // Lyrics without a heading are verse 1; the heading added for it is not stored.
        if (!keepFirstHeading && merged.Count > 0)
        {
            merged.RemoveAt(0);
        }

        var notations = new StringBuilder();
        for (int index = 0; index < merged.Count; index++)
        {
            if (merged[index].Chords.Length > 0)
            {
                notations.Append('(').Append(index).Append(';').Append(merged[index].Chords).Append(')');
            }
        }

        return (string.Join('\n', merged.Select(line => line.Text)).TrimEnd('\n', '\r'), notations.ToString());
    }

    /// <summary>FrmEditItem.ValidateTitleDetails; null when the value is accepted.</summary>
    internal static string? TitleProblem(string value, string label)
    {
        if (value.AsSpan().IndexOfAny("[]*\"<>") >= 0)
        {
            return UiText.Format("SongEditorWindow", "TitleBadCharacters", label);
        }

        return value.Length > 100
            ? UiText.Format("SongEditorWindow", "TitleTooLong", label, value.Length)
            : null;
    }

    /// <summary>
    /// The verses a sequence may name (FrmEditItem.UpdateVersesList): every heading found anywhere in either
    /// pane, and verse 1, which FrmEditItem always lists once there is any lyric text.
    /// </summary>
    internal static HashSet<int> PresentVerses(string text1, string text2)
    {
        var present = new HashSet<int> { 1 };
        foreach (var (symbol, verse) in Headings)
        {
            if (verse != Region2 && (text1.Contains(symbol, StringComparison.Ordinal) || text2.Contains(symbol, StringComparison.Ordinal)))
            {
                present.Add(verse);
            }
        }

        return present;
    }

    /// <summary>
    /// FrmEditItem.ValidateSequence. The sequence must also be one WinForms can read: SEQUENCE holds one verse
    /// code per character, so a token outside 1-99 and c t b w p q e cannot be saved for it.
    /// <paramref name="stored"/> is the SEQUENCE value the text was loaded from while the operator has not changed it.
    /// </summary>
    internal static string? SequenceProblem(string sequence, string? stored, HashSet<int> present)
    {
        var codes = new StringBuilder(stored);
        if (stored is null)
        {
            // Saved without the white space around it. Inside the text a tab or a line break is a verse code
            // to SongExchangeSequence.ToDatabase, which would then store the text as it is.
            sequence = sequence.Trim();
            if (sequence.AsSpan().IndexOfAny('\t', '\r', '\n') >= 0)
            {
                return UiText.Get("SongEditorWindow", "SequenceOneLine");
            }

            foreach (string token in sequence.Split([',', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                int verse = token.Length == 1
                    ? char.ToLowerInvariant(token[0]) switch { 'c' => 0, 'b' => 100, 'e' => 101, 't' => 102, 'w' => 103, 'p' => 111, 'q' => 112, _ => -1 }
                    : -1;
                if (verse < 0 && int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
                    && number is >= 1 and <= 99 && token[0] != '0')
                {
                    verse = number;
                }

                if (verse < 0)
                {
                    string allowed = string.Join(' ', SequenceLetters.Select(code => $"{code.Letter}({UiText.Get("SongEditorWindow", code.Key)})"));
                    return UiText.Format("SongEditorWindow", "SequenceBadToken", token, allowed);
                }

                codes.Append((char)verse);
            }
        }

        string verses = codes.ToString();
        foreach (int verse in verses)
        {
            if (!present.Contains(verse))
            {
                string name = verse is <= 103 or 111 or 112 ? Symbol(verse) : UiText.Format("SongEditorWindow", "VerseCode", verse);
                return UiText.Format("SongEditorWindow", "SequenceVerseMissing", name);
            }
        }

        // The repository writes the text with SongExchangeSequence.ToDatabase, which only turns tokens into verse
        // codes when one of the codes is a control character; otherwise the text itself would be stored.
        return verses.Length == 0 || Axicultus.Module.SongExchangeSequence.ToDatabase(sequence) == verses
            ? null
            : UiText.Format("SongEditorWindow", "SequenceNotStorable", "c");
    }

    // The one-letter sequence tokens and the resource key of the section each stands for.
    private static readonly (char Letter, string Key)[] SequenceLetters =
        [('c', "Chorus"), ('t', "Chorus2"), ('b', "Bridge"), ('w', "Bridge2"), ('p', "PreChorus"), ('q', "PreChorus2"), ('e', "Ending")];

    /// <summary>
    /// FrmEditItem.ValidateContents for one pane; null when the text can be saved. Like FrmEditItem it looks for
    /// the headings anywhere in the text. FrmEditItem also moves a heading typed inside a line onto its own line
    /// and drops a stray chord marker; text the operator changed is refused for those instead, so that what is
    /// saved is what the operator sees. <paramref name="opened"/> is the pane as the song was opened: a line
    /// the song already had is saved as it is stored.
    /// </summary>
    internal static string? ContentProblem(string text, string pane, HashSet<int> present, string opened)
    {
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        opened = opened.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (text.Contains(Region2Symbol, StringComparison.Ordinal))
        {
            return UiText.Format("SongEditorWindow", "Region2MarkerNotAllowed", pane, Region2Symbol);
        }

        string[] lines = text.Split('\n');
        var openedLines = new HashSet<string>(opened.Split('\n'), StringComparer.Ordinal);
        bool changed = text != opened;
        for (int index = 0; changed && index < lines.Length; index++)
        {
            string line = lines[index];
            if (!Headings.ContainsKey(line) && !openedLines.Contains(line) && HeadingInside(line) is { } symbol)
            {
                // Both apps show a heading with a space before or after it as a line of the verse.
                return Headings.ContainsKey(line.Trim())
                    ? UiText.Format("SongEditorWindow", "HeadingSpaces", pane, symbol)
                    : UiText.Format("SongEditorWindow", "HeadingOwnLine", pane, symbol);
            }

            if (!line.Contains('»'))
            {
                continue;
            }

            if (SongEditorChords.ChordLineProblem(lines, index) is { } problem)
            {
                return $"{pane}: {problem}";
            }

            // The chords of a heading line are not stored (gfLyrics.FormatDisplayLyrics lists lyric lines only).
            if (Headings.ContainsKey(lines[index + 1]))
            {
                return UiText.Format("SongEditorWindow", "HeadingBelowChords", pane, lines[index + 1]);
            }
        }

        int missing = -1;
        foreach (int verse in present.Order())
        {
            string symbol = Symbol(verse);
            int position = text.IndexOf(symbol, StringComparison.Ordinal);
            if (position < 0)
            {
                continue;
            }

            if (text.IndexOf(symbol, position + symbol.Length, StringComparison.Ordinal) >= 0)
            {
                return UiText.Format("SongEditorWindow", "HeadingTwice", pane, symbol);
            }

            missing = missing < 0 && verse is >= 2 and <= 99 && !present.Contains(verse - 1) ? verse - 1 : missing;
        }

        if (missing > 0)
        {
            return UiText.Format("SongEditorWindow", "HeadingMissing", pane, missing, Symbol(missing));
        }

        // FrmEditItem asks for a heading above such lyrics. A song that is stored like that is saved all the
        // same: the lines are lines of verse 1, as both apps show them.
        int unmarked = HeadingBelowLyrics(text, present);
        return unmarked >= 0 && HeadingBelowLyrics(opened, present) < 0
            ? UiText.Format("SongEditorWindow", "HeadingNeeded", pane, Symbol(unmarked))
            : null;
    }

    // The first heading of a pane with lyrics above it, once a verse (not only the chorus) is marked; -1 when
    // there is none. Empty lines above the first heading are not stored (Merge), so they do not count.
    private static int HeadingBelowLyrics(string text, HashSet<int> present)
    {
        int firstPosition = int.MaxValue, firstVerse = -1;
        bool verseMarked = false;
        foreach (int verse in present)
        {
            int position = text.IndexOf(Symbol(verse), StringComparison.Ordinal);
            if (position >= 0)
            {
                (firstPosition, firstVerse) = position < firstPosition ? (position, verse) : (firstPosition, firstVerse);
                verseMarked |= verse != 0;
            }
        }

        return verseMarked && text.AsSpan(0, firstPosition).Trim('\n').Length > 0 ? firstVerse : -1;
    }
    // A heading that shares its line with other text: FrmEditItem counts it as a verse, the saved lyrics do not.
    private static string? HeadingInside(string line)
    {
        for (int open = line.IndexOf('['); open >= 0; open = line.IndexOf('[', open + 1))
        {
            int close = line.IndexOf(']', open);
            if (close < 0)
            {
                return null;
            }

            string candidate = line[open..(close + 1)];
            if (Headings.ContainsKey(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    // gfLyrics.IsNewR2Format: one [region 2] marker that separates two whole sets of verses.
    private static bool IsSeparateRegionFormat(string lyrics)
    {
        string[] sections = [.. lyrics.Split(Region2Symbol).Where(section => section.Length > 0)];
        if (sections.Length != 2)
        {
            return false;
        }

        string second = sections[1];
        second = second.StartsWith("\r\n", StringComparison.Ordinal) ? second[2..] : second[0] == '\n' ? second[1..] : second;
        if (second.Length == 0 || second[0] != '[')
        {
            return false;
        }

        int end = second.AsSpan(1).IndexOfAny('[', ']', '\n');
        return end >= 0 && second[end + 1] == ']'
            && Headings.TryGetValue(second[..(end + 2)], out int verse) && verse != Region2;
    }

    // MSC is read in order, at most one record per lyric line (gfLyrics.ExtractLyricRegions).
    private sealed class NotationCursor(string? notations)
    {
        private string _remaining = notations ?? string.Empty;
        private int _line = -1;
        private string _fields = string.Empty;

        public string FieldsFor(int lineIndex)
        {
            if (_line < lineIndex && _remaining.Length > 0
                && !SongEditorChords.TryReadRecord(ref _remaining, out _line, out _fields))
            {
                _remaining = string.Empty;
            }

            return _line == lineIndex ? _fields : string.Empty;
        }
    }

    // One pane as gfLyrics.FormatDisplayLyrics lists it for the merge: lines grouped by verse number.
    private sealed class Region
    {
        private readonly List<(int Verse, string Text, string Chords)> _lines = [];

        public Region(string text, string notations)
        {
            Text = text.Replace("\r\n", "\n", StringComparison.Ordinal);
            var rows = new List<(int Verse, string Text, string Chords)>();
            var records = new NotationCursor(notations);
            int verse = 1, lineIndex = -1, highest = 1;
            foreach (string line in Text.Split('\n'))
            {
                lineIndex++;
                string chords = records.FieldsFor(lineIndex);
                bool isHeading = Headings.TryGetValue(line, out int heading);
                if (isHeading && heading == Region2)
                {
                    continue; // It cannot be saved from a pane (ContentProblem) and is left out here.
                }

                verse = isHeading ? heading : verse;
                if (!Order.Contains(verse))
                {
                    Order.Add(verse);
                }

                if (isHeading)
                {
                    Headed.Add(verse);
                    continue;
                }

                rows.Add((verse, line, chords));
                if (Present.Add(verse))
                {
                    Sequence.Add(verse);
                    highest = Math.Max(highest, verse);
                }
            }

            for (int number = 0; number <= highest; number++)
            {
                _lines.AddRange(rows.Where(row => row.Verse == number));
            }

            // Empty lines: none at either end, never two in a row.
            while (_lines.Count > 0 && _lines[^1].Text.Length == 0)
            {
                _lines.RemoveAt(_lines.Count - 1);
            }

            for (int index = _lines.Count - 1; index >= 1; index--)
            {
                if (_lines[index].Text.Length == 0 && _lines[index - 1].Text.Length == 0)
                {
                    _lines.RemoveAt(index);
                }
            }

            if (_lines.Count > 0 && _lines[0].Text.Length == 0)
            {
                _lines.RemoveAt(0);
            }
        }

        public string Text { get; }

        /// <summary>Verses in the order their lines first appear.</summary>
        public List<int> Sequence { get; } = [];

        /// <summary>Verses in the order they first appear, be it as a heading without lines.</summary>
        public List<int> Order { get; } = [];

        public HashSet<int> Present { get; } = [];

        /// <summary>Verses whose heading is a line of the pane.</summary>
        public HashSet<int> Headed { get; } = [];

        public IEnumerable<(string Text, string Chords)> LinesOf(int verse)
            => _lines.Where(line => line.Verse == verse).Select(line => (line.Text, line.Chords));
    }
}
