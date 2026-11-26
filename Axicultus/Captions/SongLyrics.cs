using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// Letters misread in a deck's pictures, put right from the song's own words in the song database. The deck's song is
/// the one most of its read lines are like; in a read line, a word that differs from the song's word in the same place
/// by one syllable becomes the song's word. Words are never added, dropped or swapped for other words.
/// </summary>
internal static partial class SongLyrics
{
    /// <summary>How alike (Hangul syllables, 1 - edits / the longer length) a read line and a part of a song must be.</summary>
    internal const double LineLikeness = 0.7;

    /// <summary>The share of the deck's read lines that must be like a part of the song for the song to be the deck's.</summary>
    internal const double SongShare = 0.5;

    /// <summary>Without a title like the deck's name, this many read lines must be like a part of the song.</summary>
    internal const int Evidence = 3;

    /// <summary>How alike (whole strings) a song's title and the deck's file name must be to stand for the song.</summary>
    internal const double TitleLikeness = 0.6;

    /// <summary>A title shorter than this (in Hangul syllables) says too little to stand for the song.</summary>
    internal const int TitleSyllables = 4;

    // The songs most like the deck's read text (shared syllable pairs) and most like its name, looked at closely.
    private const int TextCandidates = 30, TitleCandidates = 10;

    /// <summary>
    /// A song as read from the database: its lines as the lyrics display reads them (no chords), without tags, kept as
    /// its words (runs of Hangul syllables) only. What the correction of one deck needs beyond that is made when asked.
    /// </summary>
    internal sealed class Song
    {
        public Song(string title, string alternateTitle, string lyrics)
        {
            Title = title;
            var text = string.Join(" ", LyricsDisplayFormatter.ReadLines(lyrics).Select(line => Tag().Replace(line, "").Trim()));
            Words = string.Join(" ", Hangul().Matches(text).Select(match => match.Value));
            Key = Words.Replace(" ", "");
            Titles = [.. new[] { title, alternateTitle }.Select(Syllables).Where(t => t.Length >= TitleSyllables)];
            var pairs = new HashSet<int>();
            for (int i = 0; i + 1 < Key.Length; i++) pairs.Add(Key[i] << 16 | Key[i + 1]);
            Pairs = [.. pairs.Order()];
        }

        public string Title { get; }

        /// <summary>The song's words, one space between them.</summary>
        public string Words { get; }

        /// <summary>The song's syllables, run together.</summary>
        public string Key { get; }

        public string[] Titles { get; }

        /// <summary>The song's pairs of neighbouring syllables, each once, in order.</summary>
        public int[] Pairs { get; }

        // Per syllable, the word it is in; the words; the words as a set. Made for the deck's song only.
        private sealed record WordParts(int[] WordOf, string[] Word, HashSet<string> Vocabulary);

        private WordParts? _parts;

        private WordParts Parts()
        {
            if (Volatile.Read(ref _parts) is { } made) return made;
            var words = Words.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordOf = new int[Key.Length];
            for (int w = 0, i = 0; w < words.Length; w++) for (int k = 0; k < words[w].Length; k++) wordOf[i++] = w;
            Volatile.Write(ref _parts, new WordParts(wordOf, words, [.. words]));
            return _parts!;
        }

        /// <summary>The song's words (runs of syllables), each once.</summary>
        public HashSet<string> Vocabulary => Parts().Vocabulary;

        /// <summary>The word (run of syllables) the song's i-th syllable is in.</summary>
        public int WordOf(int i) => Parts().WordOf[i];

        /// <summary>The syllables of the song's w-th word.</summary>
        public string Word(int w) => Parts().Word[w];

        /// <summary>How many of the given pairs of neighbouring syllables (each once, in order) the song has.</summary>
        public int Shared(int[] pairs)
        {
            int shared = 0;
            for (int i = 0, j = 0; i < pairs.Length && j < Pairs.Length;)
                if (pairs[i] == Pairs[j]) { shared++; i++; j++; }
                else if (pairs[i] < Pairs[j]) i++;
                else j++;
            return shared;
        }
    }

    /// <summary>
    /// The deck's read lyrics (per slide, per row, lines split by '\n') with misread words put right from the deck's song,
    /// or the same lyrics when no song is sure to be the deck's.
    /// </summary>
    internal static string[][] Correct(string deckPath, IReadOnlyList<string[]> slides, IReadOnlyList<Song> songs)
    {
        string[][] same = [.. slides.Select(rows => rows.ToArray())];
        if (Find(deckPath, slides, songs) is not { } song) return same;
        return [.. slides.Select(rows => rows.Select(row => string.Join("\n", row.Split('\n').Select(line => Mend(song, line)))).ToArray())];
    }

    /// <summary>A read line that says enough to be evidence for a song (short lines like "amen" are in many songs).</summary>
    internal const int EvidenceSyllables = 4;

    /// <summary>
    /// The deck's song: the one the most distinct read lines (of <see cref="EvidenceSyllables"/> syllables or more) are
    /// like a part of; between equals the one with more lines exactly like a part, then more alike in all, then the
    /// title more like the deck's file name. Null unless half those lines or more are like it and either
    /// <see cref="Evidence"/> lines are or its title is like the deck's name.
    /// </summary>
    internal static Song? Find(string deckPath, IReadOnlyList<string[]> slides, IReadOnlyList<Song> songs)
    {
        var lines = slides.SelectMany(rows => rows).SelectMany(row => row.Split('\n')).Select(Syllables)
            .Where(line => line.Length >= EvidenceSyllables).Distinct().ToList();
        if (lines.Count == 0 || songs.Count == 0) return null;
        var read = new HashSet<int>();
        foreach (var line in lines) for (int i = 0; i + 1 < line.Length; i++) read.Add(line[i] << 16 | line[i + 1]);
        int[] pairs = [.. read.Order()];
        string name = Syllables(Path.GetFileNameWithoutExtension(deckPath));
        double TitleOf(Song song) => song.Titles.Select(title => Likeness(title, name)).DefaultIfEmpty(0).Max();
        // The shortlist by shared syllable pairs over all the pairs of both (a long hymn shares many pairs with anything).
        double Overlap(Song song) { int shared = song.Shared(pairs); return shared / (double)(pairs.Length + song.Pairs.Length - shared); }
        var candidates = songs.OrderByDescending(Overlap).Take(TextCandidates)
            .Concat(songs.OrderByDescending(TitleOf).Take(TitleCandidates)).Distinct();
        Song? best = null;
        (int Hits, int Exact, double Sum, double Title) bestScore = default;
        foreach (var song in candidates)
        {
            var likeness = lines.Select(line => Part(song, line).Likeness).ToList();
            (int, int, double, double) score = (likeness.Count(l => l >= LineLikeness), likeness.Count(l => l >= 1), likeness.Sum(), TitleOf(song));
            if (best is null || score.CompareTo(bestScore) > 0) (best, bestScore) = (song, score);
        }
        return best is null || bestScore.Hits < SongShare * lines.Count || bestScore.Hits < Evidence && bestScore.Title < TitleLikeness ? null : best;
    }

    // The read line with each word (a run of Hangul syllables, two or more) that differs from the song's word it lines up
    // with by one syllable (one changed, added or missing) made that word; all else as read (other words, spaces,
    // digits, letters, signs). Nothing when the line is like no part of the song.
    private static string Mend(Song song, string line)
    {
        string read = Syllables(line);
        if (read.Length < 2) return line;
        var (likeness, first, last) = Part(song, read);
        if (likeness < LineLikeness) return line;
        int[] across = Align(read, song.Key[first..(last + 1)]); // per read syllable: the song syllable it lines up with, or -1
        var mended = new StringBuilder(line.Length);
        int syllable = 0, at = 0;
        foreach (Match word in Hangul().Matches(line))
        {
            mended.Append(line, at, word.Index - at);
            string mine = word.Value;
            var theirs = across.Skip(syllable).Take(mine.Length).Where(i => i >= 0).Select(i => song.WordOf(first + i)).Distinct().ToList();
            // The song's word must line up with this read word alone: a syllable of it read as another word ("nae"
            // before "mame" for "naemame") is not added again.
            bool alone = theirs.Count == 1 && across.Where((i, k) => i >= 0 && (k < syllable || k >= syllable + mine.Length))
                .All(i => song.WordOf(first + i) != theirs[0]);
            string replacement = mine;
            // (a read word that is itself a word of the song is right as read)
            if (mine.Length >= 2 && alone && !song.Vocabulary.Contains(mine) && song.Word(theirs[0]) is var other && Distance(mine, other) == 1)
                replacement = other;
            mended.Append(replacement);
            syllable += mine.Length;
            at = word.Index + word.Length;
        }
        mended.Append(line, at, line.Length - at);
        return mended.ToString();
    }

    // Per syllable of a, the syllable of b it lines up with in a least-edits alignment, or -1 where a has one b lacks.
    private static int[] Align(string a, string b)
    {
        int m = a.Length, n = b.Length;
        var cost = new int[m + 1, n + 1];
        for (int i = 0; i <= m; i++) cost[i, 0] = i;
        for (int j = 0; j <= n; j++) cost[0, j] = j;
        for (int i = 1; i <= m; i++)
            for (int j = 1; j <= n; j++)
                cost[i, j] = Math.Min(Math.Min(cost[i - 1, j], cost[i, j - 1]) + 1, cost[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
        var across = new int[m];
        for (int i = m, j = n; i > 0;)
        {
            if (j > 0 && cost[i, j] == cost[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1)) across[--i] = --j;
            else if (cost[i, j] == cost[i - 1, j] + 1) across[--i] = -1;
            else j--;
        }
        return across;
    }

    // The part of the song (any run of its syllables, across its lines) most like the line: the fewest edits with the
    // part's start and end free. Likeness: 1 - edits / the longer length.
    private static (double Likeness, int First, int Last) Part(Song song, string line)
    {
        string key = song.Key;
        int m = line.Length, n = key.Length;
        if (n == 0) return (0, 0, 0);
        var cost = new int[n + 1];
        var start = new int[n + 1];
        var nextCost = new int[n + 1];
        var nextStart = new int[n + 1];
        for (int j = 0; j <= n; j++) start[j] = j;
        for (int i = 1; i <= m; i++)
        {
            nextCost[0] = i;
            nextStart[0] = 0;
            for (int j = 1; j <= n; j++)
            {
                int keep = cost[j - 1] + (line[i - 1] == key[j - 1] ? 0 : 1), drop = cost[j] + 1, add = nextCost[j - 1] + 1;
                if (keep <= drop && keep <= add) (nextCost[j], nextStart[j]) = (keep, start[j - 1]);
                else if (drop <= add) (nextCost[j], nextStart[j]) = (drop, start[j]);
                else (nextCost[j], nextStart[j]) = (add, nextStart[j - 1]);
            }
            (cost, nextCost) = (nextCost, cost);
            (start, nextStart) = (nextStart, start);
        }
        int end = 1;
        for (int j = 2; j <= n; j++) if (cost[j] < cost[end]) end = j;
        int first = start[end];
        if (end <= first) return (0, 0, 0);
        return (1 - cost[end] / (double)Math.Max(m, end - first), first, end - 1);
    }

    // How alike two keys are as wholes: 1 - edits / the longer length.
    private static double Likeness(string a, string b) =>
        a.Length == 0 || b.Length == 0 ? 0 : 1 - Distance(a, b) / (double)Math.Max(a.Length, b.Length);

    private static int Distance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];
        for (int j = 0; j <= b.Length; j++) previous[j] = j;
        for (int i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (int j = 1; j <= b.Length; j++)
                current[j] = Math.Min(Math.Min(current[j - 1], previous[j]) + 1, previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            (previous, current) = (current, previous);
        }
        return previous[b.Length];
    }

    internal static string Syllables(string text) => new([.. text.Where(IsSyllable)]);

    private static bool IsSyllable(char c) => c is >= '가' and <= '힣';

    [GeneratedRegex(@"\[[^\]]*\]")] private static partial Regex Tag();
    [GeneratedRegex("[가-힣]+")] private static partial Regex Hangul(); // not UI text
}
