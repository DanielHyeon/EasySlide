using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Axicultus.Module;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// Reviewer harness: WinForms (gfLyrics) verse/screen rules re-implemented line by line, compared with the WPF formatter
// over a real song database.
public class RevRealDataParityTests(Xunit.Abstractions.ITestOutputHelper output)
{
    /// <summary>Runs when AXICULTUS_REAL_SONG_DB names a song database (opened read-only); skipped otherwise.</summary>
    private sealed class RealSongDatabaseFactAttribute : FactAttribute
    {
        public RealSongDatabaseFactAttribute()
        {
            if (!File.Exists(Db))
            {
                Skip = "Set AXICULTUS_REAL_SONG_DB to a song database (it is opened read-only).";
            }
        }
    }

    private static string Db => Environment.GetEnvironmentVariable("AXICULTUS_REAL_SONG_DB") ?? "";

    internal sealed record WfSlide(int Verse, string R1, string R2);

    private static readonly Dictionary<string, int> Headings = BuildHeadings();

    private static Dictionary<string, int> BuildHeadings()
    {
        var d = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 1; i <= 99; i++) d["[" + i + "]"] = i;
        d["[chorus]"] = 0; d["[chorus 2]"] = 102; d["[bridge]"] = 100; d["[bridge 2]"] = 103;
        d["[prechorus]"] = 111; d["[prechorus 2]"] = 112; d["[ending]"] = 101; d["[region 2]"] = 150;
        return d;
    }

    private static string ReadVerseIndicator(string text)
    {
        if (text.Length == 0 || text[0] != '[') return "";
        var after = text[1..];
        int end = after.IndexOfAny(['[', ']', '\n']);
        if (end < 0 || after[end] != ']') return "";
        var heading = text[..(end + 2)];
        return Headings.TryGetValue(heading, out int v) && v != 150 ? heading : "";
    }

    internal static bool IsNewR2(string lyrics)
    {
        int count = 0; string second = "";
        foreach (var section in lyrics.Split("[region 2]"))
        {
            if (section.Length == 0) continue;
            count++;
            if (count == 2) second = section;
            else if (count > 2) return false;
        }
        if (count != 2) return false;
        if (second.StartsWith("\r\n", StringComparison.Ordinal)) second = second[2..];
        else if (second[0] == '\n') second = second[1..];
        return ReadVerseIndicator(second).Length > 0;
    }

    private static int Count(List<(int v, int r, string t)> l, int s, int e)
    {
        if (s > e || s < 0 || e < 0) return -1;
        int n = 0;
        for (int i = s; i <= e; i++) if (l[i].t == "" || i == e) n++;
        return n;
    }

    private static int ScreenLoc(List<(int v, int r, string t)> l, int s, int e, int screen)
    {
        if (s > e) return -1;
        if (s + 1 <= e && l[s].t == "") s++;
        int n = 1;
        for (int i = s; i <= e; i++)
        {
            if (l[i].t == "") n++;
            if (screen == n)
            {
                if (l[i].t != "") return i;
                if (i < e) return i + 1;
                return -1;
            }
        }
        return -1;
    }

    private static int EndLoc(List<(int v, int r, string t)> l, int s, int e)
    {
        if (s > e) return -1;
        if (s == e) return e;
        if (l[s].t == "") s++;
        for (int i = s; i < e; i++) if (l[i].t == "") return i - 1;
        return e;
    }

    internal static List<WfSlide> WinForms(string lyricsIn, string seqRaw, out string usedSequence)
    {
        var lyrics = lyricsIn.Replace("\r\n", "\n");
        bool separate = IsNewR2(lyrics);
        var rows = new List<(int v, int r, string t)>();
        int verse = 1; bool second = false;
        foreach (var line in lyrics.Split('\n'))
        {
            bool isH = Headings.TryGetValue(line, out int h);
            if (isH && h == 150) { second = true; continue; }
            if (isH) { verse = h; if (!separate) second = false; continue; }
            rows.Add((verse, second ? 2 : 1, line));
        }

        var present = new bool[160]; var basic = new StringBuilder(); int highest = 1;
        foreach (var row in rows)
            if (!present[row.v]) { present[row.v] = true; basic.Append((char)row.v); highest = Math.Max(highest, row.v); }
        var l = new List<(int v, int r, string t)>();
        for (int i = 0; i <= highest; i++)
        {
            if (!present[i]) continue;
            l.AddRange(rows.Where(x => x.v == i && x.r == 1));
            var r2 = rows.Where(x => x.v == i && x.r == 2).ToList();
            if (r2.Count > 0) present[150] = true;
            l.AddRange(r2);
        }
        for (int i = l.Count - 1; i >= 0; i--) { if (l[i].t == "") l.RemoveAt(i); else i = 0; }
        for (int i = l.Count - 1; i >= 1; i--) if (l[i].t == "" && l[i - 1].t == "") l.RemoveAt(i);
        if (l.Count > 0 && l[0].t == "") l.RemoveAt(0);
        int locLen = Math.Max(2, highest + 1);
        var loc = new int[locLen, 5];
        for (int i = 0; i < locLen; i++) for (int c = 0; c < 5; c++) loc[i, c] = -1;
        for (int i = 0; i < l.Count; i++)
        {
            int end = l[i].r * 2;
            if (loc[l[i].v, end - 1] < 0) loc[l[i].v, end - 1] = i;
            loc[l[i].v, end] = i;
        }

        var seq = new StringBuilder();
        foreach (char c in seqRaw ?? "") if (c < present.Length && present[c]) seq.Append(c);
        usedSequence = seq.Length > 0 ? seq.ToString() : basic.ToString();

        var slides = new List<int[]>();
        void Add(int v, int a, int b, int c, int d) => slides.Add([v, a, b, c, d]);
        foreach (int vi in usedSequence)
        {
            bool lastSub = false;
            if (!(vi < locLen && present[vi])) continue;
            // gfLyrics.BuildSlides: region 1 starts at its first line of text.
            int first = loc[vi, 1];
            while (first >= 0 && first <= loc[vi, 2] && l[first].t == "") first++;
            if (first > loc[vi, 2]) first = -1;
            int cnt = Count(l, first, loc[vi, 2]);
            if (cnt > 0)
            {
                try
                {
                    for (int j = 1; j <= cnt; j++)
                    {
                        int s = ScreenLoc(l, first, loc[vi, 2], j);
                        if (s < 0) continue;
                        Add(j == 1 ? vi : -1, s, EndLoc(l, s, loc[vi, 2]), -1, -1);
                        if (loc[vi, 3] >= 0 && loc[vi, 4] >= loc[vi, 3])
                        {
                            if (j == cnt) lastSub = true;
                            Reg2(l, loc[vi, 3], loc[vi, 4], slides, j, lastSub);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException) { }
            }
            else
            {
                try
                {
                    cnt = Count(l, loc[vi, 3], loc[vi, 4]);
                    for (int j = 1; j <= cnt; j++)
                    {
                        int s = ScreenLoc(l, loc[vi, 3], loc[vi, 4], j);
                        if (s >= 0) Add(j == 1 ? vi : -1, -1, -1, s, EndLoc(l, s, loc[vi, 4]));
                    }
                }
                catch (ArgumentOutOfRangeException) { }
            }
        }

        string Text(int a, int b)
        {
            if (a < 0) return "";
            b = Math.Min(b, l.Count - 1);
            if (a > b) return "";
            return string.Join("\n", l.Skip(a).Take(b - a + 1).Select(x => x.t)).TrimEnd('\r', '\n');
        }
        return slides.Select(s => new WfSlide(s[0], Text(s[1], s[2]), Text(s[3], s[4]))).ToList();
    }

    private static void Reg2(List<(int v, int r, string t)> l, int start, int end, List<int[]> slides, int screen, bool lastSub)
    {
        if (start > end) return;
        int screenStart = ScreenLoc(l, start, end, screen);
        if (screenStart < 0) return;
        int screenEnd = EndLoc(l, screenStart, end);
        var cur = slides[^1];
        int r1Lines = cur[2] - cur[1] + 1;
        if (r1Lines > 0)
        {
            cur[3] = screenStart;
            cur[4] = screenEnd;
        }
        if (end <= screenEnd || !lastSub) return;
        screenStart = screenEnd + 1;
        int n = Count(l, screenStart, end);
        for (int j = 1; j <= n; j++)
        {
            int s = ScreenLoc(l, screenStart, end, j);
            if (s >= 0) slides.Add([-1, -1, -1, s, EndLoc(l, s, end)]);
        }
    }

    private static string Norm(string s) => string.Join("\n", s.Replace("\r", "").Split('\n').Select(x => x.TrimEnd())).Trim('\n');

    internal static List<(string R1, string R2)> Wpf(string lyrics, string seqRaw, out string? seq)
    {
        var item = new LiveQueueItem("x", "x") { Lyrics = lyrics, Sequence = string.IsNullOrWhiteSpace(seqRaw) ? null : seqRaw };
        seq = item.Sequence;
        if (LyricsDisplayFormatter.HasRegion2(lyrics))
            return LyricsDisplayFormatter.GetRegionPages(lyrics, seq).Select(p => (p.Region1, p.Region2)).ToList();
        return LyricsDisplayFormatter.ToVersePages(lyrics, seq).Select(p => (p, "")).ToList();
    }

    private static string Show(string s) => s.Replace("\n", "⏎");

    [RealSongDatabaseFact]
    public void EveryRealSong_HasTheScreensFrmMainShows()
    {
        var builder = new SQLiteConnectionStringBuilder { DataSource = Db, Version = 3, ReadOnly = true };
        using var connection = new SQLiteConnection(builder.ToString());
        connection.Open();
        using var command = new SQLiteCommand("SELECT SONGID, TITLE_1, FOLDERNO, LYRICS, SEQUENCE FROM SONG ORDER BY SONGID", connection);
        using var reader = command.ExecuteReader();
        int total = 0, same = 0, threw = 0, zeroPages = 0, labelMismatch = 0;
        var classes = new SortedDictionary<string, (int Count, int Diff, List<int> Samples)>();
        var diffs = new StringBuilder();
        var diffKinds = new SortedDictionary<string, List<int>>();
        void Class(string name, bool differs, int id)
        {
            classes.TryGetValue(name, out var c);
            c.Samples ??= [];
            c.Count++;
            if (differs) { c.Diff++; if (c.Samples.Count < 12) c.Samples.Add(id); }
            classes[name] = c;
        }
        while (reader.Read())
        {
            total++;
            int id = Convert.ToInt32(reader["SONGID"]);
            string title = Convert.ToString(reader["TITLE_1"]) ?? "";
            int folder = reader["FOLDERNO"] is DBNull ? 0 : Convert.ToInt32(reader["FOLDERNO"]);
            string lyrics = reader["LYRICS"] is DBNull ? "" : Convert.ToString(reader["LYRICS"]) ?? "";
            string seqRaw = reader["SEQUENCE"] is DBNull ? "" : Convert.ToString(reader["SEQUENCE"]) ?? "";

            List<WfSlide> wf; string used;
            wf = WinForms(lyrics, seqRaw, out used);
            List<(string R1, string R2)> wpf; string? seq = null;
            bool differs;
            string kind = "";
            try
            {
                wpf = Wpf(lyrics, seqRaw, out seq);
                // labels vs pages
                if (!LyricsDisplayFormatter.HasRegion2(lyrics))
                {
                    var labels = LyricsDisplayFormatter.GetSectionLabels(lyrics, seq);
                    if (labels.Count != wpf.Count) { labelMismatch++; diffs.AppendLine($"LABELCOUNT id={id} labels={labels.Count} pages={wpf.Count}"); }
                }
                else
                {
                    var labels = LyricsDisplayFormatter.GetRegionSectionLabels(lyrics, seq);
                    if (labels.Count != wpf.Count) { labelMismatch++; diffs.AppendLine($"REGIONLABELCOUNT id={id} labels={labels.Count} pages={wpf.Count}"); }
                }
            }
            catch (Exception ex)
            {
                threw++;
                diffs.AppendLine($"THROW id={id} {ex.GetType().Name}: {ex.Message}");
                continue;
            }
            if (wpf.Count == 0 && !string.IsNullOrWhiteSpace(lyrics)) { zeroPages++; diffs.AppendLine($"ZERO id={id} wf={wf.Count} lyricsLen={lyrics.Length}"); }

            var a = wf.Select(s => (Norm(s.R1), Norm(s.R2))).Where(s => s.Item1.Length + s.Item2.Length > 0).ToList();
            var b = wpf.Select(s => (Norm(s.R1), Norm(s.R2))).ToList();
            differs = !a.SequenceEqual(b);
            if (differs)
            {
                if (a.Count != b.Count) kind = "count";
                else if (a.Select(x => x.Item1 + "|" + x.Item2).OrderBy(x => x).SequenceEqual(b.Select(x => x.Item1 + "|" + x.Item2).OrderBy(x => x))) kind = "order";
                else kind = "text";
            }
            else same++;

            var norm = lyrics.Replace("\r\n", "\n");
            var lines = norm.Split('\n');
            bool hasSeq = seqRaw.Length > 0;
            bool hasHeading = lines.Any(x => Headings.ContainsKey(x) && x != "[region 2]");
            // blank line inside a verse (between two non-blank non-heading lines without heading between)
            bool blankInside = false; { bool sawText = false, sawBlank = false; foreach (var x in lines) { if (Headings.ContainsKey(x)) { sawText = sawBlank = false; } else if (x == "") { if (sawText) sawBlank = true; } else { if (sawBlank && hasHeading) blankInside = true; sawText = true; sawBlank = false; } } }
            bool region2 = lines.Any(x => x == "[region 2]");
            bool region2Loose = norm.Contains("region 2", StringComparison.OrdinalIgnoreCase);
            bool firstNoHeading = hasHeading && lines.TakeWhile(x => !Headings.ContainsKey(x)).Any(x => x.Trim().Length > 0);
            bool bracketNonHeading = lines.Any(x => { var t = x.Trim(); return t.Length >= 2 && t[0] == '[' && t[^1] == ']' && !Headings.ContainsKey(t); });
            bool headingNotExact = lines.Any(x => !Headings.ContainsKey(x) && (Headings.ContainsKey(x.Trim()) || Headings.Keys.Any(h => x.Contains(h, StringComparison.Ordinal))));
            bool headingTrailing = lines.Any(x => !Headings.ContainsKey(x) && Headings.ContainsKey(x.Trim()));
            bool wsOnlyLine = lines.Any(x => x.Length > 0 && x.Trim().Length == 0);
            var presentVerses = new HashSet<int>(); { int v = 1; foreach (var x in lines) { if (Headings.TryGetValue(x, out int h)) { if (h != 150) v = h; } else presentVerses.Add(v); } }
            bool seqMissing = hasSeq && seqRaw.Any(c => !presentVerses.Contains(c));
            bool seqRepeat = hasSeq && seqRaw.GroupBy(c => c).Any(g => g.Count() > 1);
            bool special2 = lines.Any(x => x is "[chorus 2]" or "[bridge 2]" or "[prechorus]" or "[prechorus 2]");
            bool above9 = lines.Any(x => Headings.TryGetValue(x, out int h) && h is >= 10 and <= 99);
            var headingList = lines.Where(x => Headings.ContainsKey(x) && x != "[region 2]").ToList();
            bool dupHeading = headingList.Count != headingList.Distinct().Count();
            bool newR2 = IsNewR2(norm);
            bool loneCr = norm.Contains('\r');
            bool seqNotDecoded = hasSeq && seq == seqRaw && seqRaw.Any(c => c < 32 || "defgop".Contains(c));
            bool seqAllWhitespace = hasSeq && string.IsNullOrWhiteSpace(seqRaw);
            bool twoMarkersInVerse = false; { int n = 0; foreach (var x in lines) { if (x == "[region 2]") { n++; if (n > 1) twoMarkersInVerse = true; } else if (Headings.ContainsKey(x)) n = 0; } }
            bool wsInside = false; { bool sawText = false, sawWs = false; foreach (var x in lines) { if (Headings.ContainsKey(x) || x == "") { sawText = sawWs = false; } else if (x.Trim().Length == 0) { if (sawText) sawWs = true; } else { if (sawWs) wsInside = true; sawText = true; sawWs = false; } } }
            bool preTextAndVerse1 = firstNoHeading && lines.Contains("[1]");
            if (twoMarkersInVerse) Class("23 verse with two [region 2] markers", differs, id);
            if (wsInside) Class("53 whitespace-only line between two text lines", differs, id);
            if (preTextAndVerse1) Class("33 text before first heading and a later [1]", differs, id);
            if (preTextAndVerse1 && hasSeq) Class("34 text before first heading, later [1], stored sequence", differs, id);
            if (dupHeading && region2) Class("81 duplicate heading + region 2", differs, id);

            Class("00 all", differs, id);
            if (folder == 0) Class("01 deleted(folder0)", differs, id);
            if (hasSeq) Class("10 stored sequence", differs, id);
            if (hasSeq && blankInside) Class("11 stored sequence + blank line inside verse", differs, id);
            if (!hasSeq && blankInside) Class("12 no sequence + blank line inside verse", differs, id);
            if (region2) Class("20 [region 2] line", differs, id);
            if (region2Loose && !region2) Class("21 'region 2' text but no exact marker line", differs, id);
            if (newR2) Class("22 new R2 format (all region 2 after one marker)", differs, id);
            if (firstNoHeading) Class("30 text before first heading", differs, id);
            if (!hasHeading) Class("31 no heading at all", differs, id);
            if (!hasHeading && hasSeq) Class("32 no heading + stored sequence", differs, id);
            if (bracketNonHeading) Class("40 bracket-only line that is not a heading", differs, id);
            if (headingNotExact) Class("50 heading not on a line of its own / inexact", differs, id);
            if (headingTrailing) Class("51 heading with surrounding spaces", differs, id);
            if (wsOnlyLine) Class("52 whitespace-only line", differs, id);
            if (seqMissing) Class("60 sequence names a missing verse", differs, id);
            if (seqRepeat) Class("61 sequence repeats a verse", differs, id);
            if (special2) Class("70 chorus 2 / bridge 2 / prechorus", differs, id);
            if (above9) Class("71 verse number above 9", differs, id);
            if (dupHeading) Class("80 duplicate heading", differs, id);
            if (loneCr) Class("90 lone CR", differs, id);
            if (seqNotDecoded) Class("91 sequence not decoded by FromWpfDatabase", differs, id);
            if (seqAllWhitespace) Class("92 sequence only whitespace codes", differs, id);

            if (differs)
            {
                if (!diffKinds.TryGetValue(kind, out var list)) diffKinds[kind] = list = [];
                list.Add(id);
                diffs.AppendLine($"DIFF[{kind}] id={id} folder={folder} title={title} seqRaw=[{string.Join(",", seqRaw.Select(c => (int)c))}] wfSeq=[{string.Join(",", used.Select(c => (int)c))}] wpfSeq={seq} wf={a.Count} wpf={b.Count}");
                for (int i = 0; i < Math.Max(a.Count, b.Count) && i < 40; i++)
                {
                    string l = i < a.Count ? Show(a[i].Item1) + (a[i].Item2.Length > 0 ? " ║ " + Show(a[i].Item2) : "") : "(none)";
                    string r = i < b.Count ? Show(b[i].Item1) + (b[i].Item2.Length > 0 ? " ║ " + Show(b[i].Item2) : "") : "(none)";
                    if (l != r) diffs.AppendLine($"   #{i + 1} WF : {l}\n   #{i + 1} WPF: {r}");
                }
            }
        }

        var report = new StringBuilder();
        report.AppendLine($"total={total} same={same} differ={total - same - threw} threw={threw} zeroPages={zeroPages} labelMismatch={labelMismatch}");
        foreach (var (k, v) in diffKinds) report.AppendLine($"kind {k}: {v.Count}");
        foreach (var (k, v) in classes) report.AppendLine($"{k}: count={v.Count} differ={v.Diff} samples={string.Join(",", v.Samples)}");
        output.WriteLine(report.ToString());
        output.WriteLine(diffs.ToString());
        string summary = report.ToString().Split('\n')[0].Trim();
        Assert.True(same == total && threw == 0 && labelMismatch == 0,
            summary + Environment.NewLine + diffs.ToString(0, Math.Min(diffs.Length, 6000)));
    }
}