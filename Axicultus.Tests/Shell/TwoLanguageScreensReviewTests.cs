using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// Adversarial review of "a two-language song is cut into screens by measurement" (SongPageLayout.Paginate with
/// region2Top, MainViewModel.GetSongPages, LiveSessionService). The specification is gfLyrics.BuildSlides,
/// BuildSlidesReg2 and GetOneScreen. A failing test is a defect; a passing test pins something that was tried.
/// </summary>
public class TwoLanguageScreensReviewTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private const double ScreenHeight = 720;
    private const int Panel = 6;

    private static readonly SongPageLayout Layout = new(12, "Arial", 0, 5, 5, 0, 1, 100, 0);

    private static string[] English(int count, int from = 1) => [.. Enumerable.Range(from, count).Select(line => $"line {line}")];
    private static string[] Korean(int count, int from = 1) => [.. Enumerable.Range(from, count).Select(line => $"줄 {line}")];

    private static string[] Lines(string region) => region.Length > 0 ? region.Split('\n') : [];

    private static List<(string[] Region1, string[] Region2)> Screens(string lyrics, string? sequence = null, int region2Top = 50,
        SongPageLayout? layout = null)
        => (layout ?? Layout).Paginate(lyrics, sequence, null, ScreenHeight, Panel, null, null, region2Top).Bodies
            .ConvertAll(SongPageLayout.SplitRegions)
            .ConvertAll(page => (Lines(page.Region1), Lines(page.Region2)));

    // The lines region 1 has room for above region 2.
    private static int Room(int region2Top = 50)
        => Screens($"[1]\n{string.Join("\n", English(300))}\n[region 2]\n{string.Join("\n", Korean(300))}", region2Top: region2Top)[0].Region1.Length;

    // ── The oracle: gfLyrics.BuildSlides + BuildSlidesReg2 over the WinForms line list ─────────────────────────

    // What a line shows: without what follows a chord marker and without the spaces at its end.
    private static string Shown(string line) => LyricsDisplayFormatter.StripInlineNotation(line).TrimEnd();

    private static string[] Trimmed(IEnumerable<string> lines)
    {
        var list = lines.ToList();
        while (list.Count > 0 && list[^1].Length == 0) list.RemoveAt(list.Count - 1);
        while (list.Count > 0 && list[0].Length == 0) list.RemoveAt(0);
        return [.. list];
    }

    internal sealed record Finding(string Kind, string Text);

    /// <summary>
    /// Compares the cut pages with what FrmMain shows. The written screens come from the line-by-line port of
    /// gfLyrics in RevRealDataParityTests; where region 1 is cut is taken from the pages themselves; what each cut
    /// takes of region 2 is BuildSlidesReg2 (gfLyrics.cs 1602-1633): as many lines as region 1 has on the slide,
    /// the last slide the rest, nothing once region 2 has run out; a screen region 2 has left over is one slide.
    /// </summary>
    internal static List<Finding> Compare(string lyrics, string sequenceRaw, List<string> bodies, out int cutScreens)
    {
        cutScreens = 0;
        var findings = new List<Finding>();
        var slides = RevRealDataParityTests.WinForms(lyrics, sequenceRaw, out _);
        var pages = bodies.ConvertAll(SongPageLayout.SplitRegions)
            .ConvertAll(page => (R1: Lines(page.Region1).Select(Shown).ToArray(), R2: Lines(page.Region2).Select(Shown).ToArray()));
        int next = 0;
        char verseKind = 'P';
        for (int index = 0; index < slides.Count; index++)
        {
            var slide = slides[index];
            if (slide.Verse >= 0) verseKind = slide.R1.Length > 0 ? 'P' : 'O';
            string[] region1 = [.. Lines(slide.R1).Select(Shown)], region2 = [.. Lines(slide.R2).Select(Shown)];
            int text1 = region1.Count(line => line.Length > 0), text2 = region2.Count(line => line.Length > 0);
            if (text1 + text2 == 0) continue; // a screen of nothing but spaces is not shown
            string where = $"written screen {index + 1}";
            if (text1 > 0)
            {
                var group = new List<(string[] R1, string[] R2)>();
                for (int seen = 0; seen < text1 && next < pages.Count; next++)
                {
                    group.Add(pages[next]);
                    seen += pages[next].R1.Count(line => line.Length > 0);
                }
                if (!group.SelectMany(page => page.R1).Where(line => line.Length > 0).SequenceEqual(region1.Where(line => line.Length > 0)))
                {
                    findings.Add(new("region 1 lines", $"{where}: region 1 is [{string.Join("⏎", region1)}], the pages have [{string.Join(" ║ ", group.Select(page => string.Join("⏎", page.R1)))}]"));
                    return findings;
                }
                if (group.Count > 1) cutScreens++;
                // The lines FrmMain counts on each slide: a line of spaces is a line (the pages drop those at
                // the ends of a written screen).
                var counts = group.Select(page => page.R1.Length).ToArray();
                int lead = Math.Max(0, region1.TakeWhile(line => line.Length == 0).Count() - group[0].R1.TakeWhile(line => line.Length == 0).Count());
                int trail = Math.Max(0, region1.Length - counts.Sum() - lead);
                counts[0] += lead;
                counts[^1] += trail;
                if (counts.Sum() != region1.Length)
                    findings.Add(new("region 1 line count", $"{where}: FrmMain has {region1.Length} lines, the pages {counts.Sum()}"));
                int at = region2.Length > 0 ? 0 : -1;
                for (int cut = 0; cut < group.Count; cut++)
                {
                    string[] expected = [];
                    if (at >= 0)
                    {
                        if (region2.Length - at >= counts[cut] && cut < group.Count - 1)
                        {
                            expected = region2[at..(at + counts[cut])];
                            at += counts[cut];
                        }
                        else
                        {
                            expected = region2[at..];
                            at = -1;
                        }
                    }
                    if (!Trimmed(expected).SequenceEqual(Trimmed(group[cut].R2)))
                    {
                        findings.Add(new(lead + trail > 0 || region2.Any(line => line.Length == 0) ? "region 2 distribution (line of spaces)" : "region 2 distribution",
                            $"{where}, cut {cut + 1} of {group.Count} ({counts[cut]} region 1 lines): FrmMain shows [{string.Join("⏎", expected)}], the page [{string.Join("⏎", group[cut].R2)}]"));
                        break;
                    }
                }
                continue;
            }

            // No region 1 text: a verse that has region 2 only is cut by region 2; a region 2 screen left over, or
            // one beside a region 1 screen of spaces, is one slide.
            bool whole = verseKind != 'O' || slide.R1.Length > 0;
            if (slide.R1.Length == 0 && verseKind != 'O') where += " (left over)";
            var only = new List<string[]>();
            for (int seen = 0; seen < text2 && next < pages.Count; next++)
            {
                if (pages[next].R1.Any(line => line.Length > 0)) break;
                only.Add(pages[next].R2);
                seen += pages[next].R2.Count(line => line.Length > 0);
            }
            if (!only.SelectMany(page => page).Where(line => line.Length > 0).SequenceEqual(region2.Where(line => line.Length > 0)))
            {
                findings.Add(new("region 2 lines", $"{where}: region 2 is [{string.Join("⏎", region2)}], the pages have [{string.Join(" ║ ", only.Select(page => string.Join("⏎", page)))}]"));
                return findings;
            }
            if (only.Count > 1) cutScreens++;
            if (whole && only.Count != 1)
                findings.Add(new(slide.R1.Length > 0 ? "region 2 beside a region 1 screen of spaces is cut" : "left-over region 2 screen is cut",
                    $"{where}: FrmMain shows its {region2.Length} lines on one slide, the pages cut it into {only.Count}"));
        }
        if (next != pages.Count)
            findings.Add(new("extra pages", $"{pages.Count - next} page(s) after the last written screen: [{string.Join(" ║ ", pages.Skip(next).Select(page => string.Join("⏎", page.R1) + " / " + string.Join("⏎", page.R2)))}]"));
        return findings;
    }

    // ── The real song database ────────────────────────────────────────────────────────────────────────────────

    /// <summary>Runs when AXICULTUS_REAL_SONG_DB names a song database (opened read-only); skipped otherwise.</summary>
    private sealed class RealSongDatabaseFactAttribute : FactAttribute
    {
        public RealSongDatabaseFactAttribute()
        {
            if (!File.Exists(RealDatabase)) Skip = "Set AXICULTUS_REAL_SONG_DB to a song database (it is opened read-only).";
        }
    }

    private static string RealDatabase => Environment.GetEnvironmentVariable("AXICULTUS_REAL_SONG_DB") ?? "";

    private static string Text(SQLiteDataReader reader, string column)
    {
        for (int i = 0; i < reader.FieldCount; i++)
            if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                return reader.IsDBNull(i) ? "" : Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture) ?? "";
        return "";
    }

    private static int Number(SQLiteDataReader reader, string column) => int.TryParse(Text(reader, column), out int value) ? value : 0;

    internal sealed record RealSong(string Id, string Lyrics, string SequenceRaw, LiveQueueItem Item, SongPageLayout Layout, SongFormatData? Format, ItemDisplayFormat? Display);

    private static List<RealSong> ReadRealTwoLanguageSongs(out int all)
    {
        var builder = new SQLiteConnectionStringBuilder { DataSource = RealDatabase, Version = 3, ReadOnly = true };
        using var connection = new SQLiteConnection(builder.ToString());
        connection.Open();
        var layouts = new Dictionary<int, SongPageLayout>();
        using (var command = new SQLiteCommand("SELECT * FROM FOLDER", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                // As AdminDatabaseRepository.GetFolders builds the layout of a folder.
                if (Number(reader, "Size0") is not (> 0 and <= 500)) continue;
                var fields = FolderFormatField.All.ToDictionary(field => field.Column, field => Text(reader, field.Column));
                layouts[Number(reader, "FolderNo")] = new SongPageLayout(Number(reader, "Size0"), Text(reader, "Fontname0"), Number(reader, "VPos0"),
                    Number(reader, "LMargin"), Number(reader, "RMargin"), Number(reader, "BMargin"),
                    double.TryParse(Text(reader, "LineSpacing"), System.Globalization.CultureInfo.InvariantCulture, out var spacing) && spacing is >= 0.5 and <= 2 ? spacing : 1,
                    Number(reader, "HeadingSize"), Number(reader, "BIU0")) { Fields = fields };
            }
        }
        var fallback = new SongPageLayout(40, "Malgun Gothic", 0, 2, 2, 0, 1, 100, 0);
        var songs = new List<RealSong>();
        all = 0;
        using (var command = new SQLiteCommand("SELECT * FROM SONG ORDER BY SONGID", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string lyrics = Text(reader, "LYRICS"), formatData = Text(reader, "FORMATDATA"), sequence = Text(reader, "SEQUENCE");
                if (lyrics.Length == 0) continue;
                all++;
                var item = new LiveQueueItem("song:" + Text(reader, "SONGID"), "song", LiveItemKinds.Song)
                {
                    Lyrics = lyrics, Notations = Text(reader, "MSC"), Sequence = string.IsNullOrWhiteSpace(sequence) ? null : sequence,
                    FormatData = formatData.Length == 0 ? null : formatData, UseIndividualFormatting = formatData.Length > 0,
                };
                if (!LyricsDisplayFormatter.HasRegion2(item.LyricsWithChords)) continue;
                songs.Add(new(Text(reader, "SONGID"), lyrics, sequence, item, layouts.GetValueOrDefault(Number(reader, "FOLDERNO"), fallback),
                    SongFormatData.Parse(item.FormatData), ItemDisplayFormat.Read(item.FormatData)));
            }
        }
        return songs;
    }

    [RealSongDatabaseFact]
    public void RealTwoLanguageSongs_ShowTheLinesFrmMainShowsOnEveryCutScreen()
    {
        var songs = ReadRealTwoLanguageSongs(out int all);
        songs.Should().NotBeEmpty();
        var kinds = new SortedDictionary<string, List<string>>();
        var examples = new StringBuilder();
        int runs = 0, cutSongs = 0, cutScreens = 0, pages = 0, withChordMarker = 0, labelMismatch = 0;
        foreach (var song in songs)
        {
            if (song.Lyrics.Contains('»')) withChordMarker++;
            int top = song.Display?.Top2 ?? song.Layout.Integer("Vpos1") ?? 50;
            foreach (var (height, region2Top) in new[] { (720d, top), (540d, top), (540d, 25), (720d, 75), (540d, 12) })
            {
                runs++;
                var cut = song.Layout.Paginate(song.Item.LyricsWithChords!, song.Item.Sequence, song.Format, height, Panel, song.Display?.Top1, null, region2Top);
                pages += cut.Bodies.Count;
                if (cut.Labels.Count != cut.Bodies.Count) labelMismatch++;
                var findings = Compare(song.Lyrics, song.SequenceRaw, cut.Bodies, out int cuts);
                cutScreens += cuts;
                if (cuts > 0) cutSongs++;
                foreach (var finding in findings)
                {
                    if (!kinds.TryGetValue(finding.Kind, out var list)) kinds[finding.Kind] = list = [];
                    if (!list.Contains(song.Id))
                    {
                        list.Add(song.Id);
                        if (list.Count <= 6) examples.AppendLine($"[{finding.Kind}] song {song.Id} at {height}/{region2Top}: {finding.Text}");
                    }
                }
            }
        }

        output.WriteLine($"{all} songs with lyrics, {songs.Count} in two languages ({withChordMarker} with a chord marker in the lyrics), {runs} runs, {pages} pages, " +
            $"{cutScreens} written screens cut in {cutSongs} runs, {labelMismatch} runs with a label count that differs");
        foreach (var (kind, list) in kinds) output.WriteLine($"{kind}: {list.Count} songs ({string.Join(",", list.Take(20))})");
        output.WriteLine(examples.ToString());
        cutScreens.Should().BeGreaterThan(0, "the runs must reach the cut");
        labelMismatch.Should().Be(0);
        kinds.Should().BeEmpty(examples.ToString(0, Math.Min(examples.Length, 4000)));
    }

    // ── Constructed cases ─────────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ALineOfSpacesThatStartsARegionOneScreen_IsALineOfThatScreen()
    {
        // gfLyrics.GetOneScreen (1701-1713) counts a line of spaces as a row (LyricLineMeasurement.Count(" ") is 1)
        // and BuildSlidesReg2 (1605) counts it among the lines of the slide. Real song 6576 ([chorus]) starts so.
        int room = Room();
        room.Should().BeGreaterThan(3);
        string lyrics = $"[1]\n \n{string.Join("\n", English(room * 2 - 1))}\n[region 2]\n{string.Join("\n", Korean(room * 2))}";

        var screens = Screens(lyrics);

        screens[1].Region1[0].Should().Be($"line {room}", "the first slide holds the line of spaces and room-1 lines of text");
        screens[1].Region2[0].Should().Be($"줄 {room + 1}", "the first slide has room rows of region 1, so it takes room lines of region 2");
        Compare(lyrics, "", Layout.Paginate(lyrics, null, null, ScreenHeight, Panel, null, null, 50).Bodies, out _).Should().BeEmpty();
    }

    [Fact]
    public void ALineOfSpacesThatStartsARegionTwoScreen_IsCountedAmongItsLines()
    {
        // BuildSlidesReg2 (1615-1626) hands out the list rows of region 2; a row of spaces is one of them.
        int room = Room();
        string lyrics = $"[1]\n{string.Join("\n", English(room * 2))}\n[region 2]\n \n{string.Join("\n", Korean(room * 2 - 1))}";

        var screens = Screens(lyrics);

        screens.Should().HaveCount(2);
        screens[1].Region2[0].Should().Be($"줄 {room}", "slide 1 takes the line of spaces and 줄 1..room-1, as many rows as region 1 has there");
    }

    [Fact]
    public void LinesOfSpacesInsideAScreen_TakeTheRoomOfARow()
    {
        // gfLyrics.GetOneScreen: every list row adds its rows to usedHeight, a row of spaces one. Here such a row
        // needs no room, so a screen holds twice the rows that fit above region 2.
        int room = Room();
        var spaced = English(room * 3).SelectMany(line => new[] { line, " " }).SkipLast(1).ToArray();
        string lyrics = $"[1]\n{string.Join("\n", spaced)}\n[region 2]\n{string.Join("\n", Korean(4))}";

        var screens = Screens(lyrics);

        screens.Max(screen => screen.Region1.Length).Should().BeLessThanOrEqualTo(room,
            $"{room} rows fit above region 2; a screen with {screens.Max(screen => screen.Region1.Length)} rows is drawn smaller or runs into region 2");
    }

    [Fact]
    public void AVerseWhoseFirstRegionOneScreenIsOnlySpaces_KeepsItsRegionTwoScreensWhole()
    {
        // gfLyrics.BuildSlides: the verse has region 1 lines (verseScreenCount > 0), so region 2 is never measured:
        // screen 1 of region 2 goes whole beside the screen of spaces (BuildSlidesReg2, region1ScreenCount 1) and
        // the screen left over after the last region 1 screen is one slide (1638-1651). Paginate takes the verse
        // for one "with region 2 only" because its first page has no region 1 text, and cuts both.
        string lyrics = $"[1]\n \n\nline a\nline b\n[region 2]\n{string.Join("\n", Korean(200))}\n\n줄 x\n\n{string.Join("\n", Korean(200, from: 1000))}";
        static int Text(string[] lines) => lines.Count(line => line.Trim().Length > 0);
        var written = LyricsDisplayFormatter.GetRegionPages(lyrics, null);
        written.Select(page => (Text(Lines(page.Region1)), Text(Lines(page.Region2)))).Should().Equal((0, 200), (2, 1), (0, 200));

        var screens = Screens(lyrics);

        screens.Select(screen => (Text(screen.Region1), Text(screen.Region2))).Should().Equal(
            [(0, 200), (2, 1), (0, 200)], "FrmMain shows these three slides; region 2 is drawn as large as fits");
    }

    [Fact]
    public void ALyricLineThatReadsRegion2_StaysALineOfRegionOne()
    {
        // "[region 2] " with a space after it is no marker for FrmMain (gfLyrics.ExtractLyricRegions looks the whole
        // line up) nor for ReadVerses: it is a line of the verse. The page text joins the regions with the same
        // text, so the page is split at the lyric line.
        string lyrics = "[1]\nline a\n[region 2] \nline b\n[region 2]\n줄 a";
        var written = LyricsDisplayFormatter.GetRegionPages(lyrics, null).Should().ContainSingle().Subject;
        written.Should().Be(new LyricsRegionPage("line a\n[region 2]\nline b", "줄 a"));

        var page = Layout.Paginate(lyrics, null, null, ScreenHeight, Panel, null, null, 50).Bodies.Should().ContainSingle().Subject;

        SongPageLayout.SplitRegions(page).Should().Be((written.Region1, written.Region2), "the cut page shows what the written screen shows");
    }

    [Fact]
    public void ALyricLineThatReadsRegion2_InASongOfOneLanguage_IsNotTakenForASecondRegionOnTheOutput()
    {
        // LiveSessionService.ComputeBodyText/ComputeBodyText2 now split every projected song page.
        string lyrics = "[1]\nline a\n[region 2] \nline b";
        LyricsDisplayFormatter.HasRegion2(lyrics).Should().BeFalse();
        var item = new LiveQueueItem("song", "Song", LiveItemKinds.Song) { Lyrics = lyrics, SongLayout = Layout };
        string page = Layout.Paginate(lyrics, null, null, ScreenHeight, Panel).Bodies.Should().ContainSingle().Subject;
        var session = new LiveSessionService();

        session.GoLive(item, "projector");
        string unpaged = session.Current.CurrentItemBodyText;
        session.GoLive(item with { ProjectedTextPage = page }, "projector");

        session.Current.CurrentItemBodyText2.Should().BeEmpty("the song has one language");
        session.Current.CurrentItemBodyText.Should().Be(unpaged, "Auto Text Overflow does not change a verse that fits");
    }

    // ── Tried and held ────────────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GeneratedSongs_ShowTheLinesFrmMainShowsOnEveryCutScreen()
    {
        // Headings in any order, [region 2] per verse or once for all (the "new R2 format"), blank lines in both
        // regions, region 2 shorter and longer than region 1, verses with one region only, stored sequences with
        // repeats. No line of spaces: those are the defects above.
        var layout = new SongPageLayout(26, "Arial", 0, 5, 5, 0, 1, 100, 0);
        (string Heading, int Code)[] verses = [("[1]", 1), ("[2]", 2), ("[3]", 3), ("[chorus]", 0), ("[bridge]", 100), ("[prechorus]", 111), ("[ending]", 101), ("[chorus 2]", 102)];
        var random = new Random(20261122);
        var problems = new List<string>();
        int cutScreens = 0, pages = 0;
        for (int run = 0; run < 1500; run++)
        {
            int line = 0;
            string[] Region(char mark, int maxScreens, int maxLines)
            {
                var lines = new List<string>();
                int screens = random.Next(0, maxScreens + 1);
                for (int screen = 0; screen < screens; screen++)
                {
                    if (lines.Count > 0) lines.Add("");
                    int count = random.Next(1, maxLines + 1);
                    for (int i = 0; i < count; i++) lines.Add($"{mark}{++line}");
                }
                return [.. lines];
            }
            var chosen = verses.OrderBy(_ => random.Next()).Take(random.Next(1, 5)).ToArray();
            bool separate = random.Next(4) == 0, noHeading = !separate && chosen.Length == 1 && random.Next(3) == 0;
            var text = new StringBuilder();
            var second = new StringBuilder();
            foreach (var (heading, _) in chosen)
            {
                string[] region1 = Region('e', 3, 14), region2 = random.Next(5) == 0 ? [] : Region('k', 4, 16);
                if (!noHeading) text.Append(heading).Append('\n');
                foreach (var l in region1) text.Append(l).Append('\n');
                if (separate)
                {
                    second.Append(heading).Append('\n');
                    foreach (var l in region2) second.Append(l).Append('\n');
                }
                else if (region2.Length > 0)
                {
                    text.Append("[region 2]\n");
                    foreach (var l in region2) text.Append(l).Append('\n');
                }
            }
            if (separate) text.Append("[region 2]\n").Append(second);
            string lyrics = text.ToString().TrimEnd('\n');
            if (!LyricsDisplayFormatter.HasRegion2(lyrics)) continue;
            string sequenceRaw = random.Next(3) == 0 ? "" : new string([.. Enumerable.Range(0, random.Next(1, 7)).Select(_ => (char)verses[random.Next(verses.Length)].Code)]);
            var item = new LiveQueueItem("x", "x") { Lyrics = lyrics, Sequence = string.IsNullOrWhiteSpace(sequenceRaw) ? null : sequenceRaw };
            int top = new[] { 50, 35, 70 }[random.Next(3)];

            var cut = layout.Paginate(lyrics, item.Sequence, null, ScreenHeight, Panel, null, null, top);
            pages += cut.Bodies.Count;
            var findings = Compare(lyrics, sequenceRaw, cut.Bodies, out int cuts);
            cutScreens += cuts;
            if (cut.Labels.Count != cut.Bodies.Count) findings.Add(new("labels", $"{cut.Labels.Count} labels for {cut.Bodies.Count} pages"));
            if (findings.Count > 0 && problems.Count < 8)
                problems.Add($"run {run} (top {top}, sequence [{string.Join(",", sequenceRaw.Select(c => (int)c))}]): {findings[0].Kind}: {findings[0].Text}\n{lyrics.Replace("\n", "⏎")}");
        }

        output.WriteLine($"{pages} pages, {cutScreens} written screens cut, {problems.Count} problems");
        cutScreens.Should().BeGreaterThan(500, "the generated songs must reach the cut");
        problems.Should().BeEmpty();
    }

    [Fact]
    public void TheChordSwitchAndTransposition_DoNotMoveALine()
    {
        // Stored chords (MSC) in both regions travel with their lines; the pages are cut without the chord rows.
        int room = Room();
        string lyrics = $"[1]\n{string.Join("\n", English(room + 2))}\n[region 2]\n{string.Join("\n", Korean(room + 2))}";
        int secondLine = room + 4; // "[1]", the region 1 lines, "[region 2]", then 줄 1
        var item = new LiveQueueItem("song", "Song", LiveItemKinds.Song)
        {
            Lyrics = lyrics, Notations = $"(1;G;0;D;3;)({room + 1};Em;0;)({secondLine};C;0;)", SongLayout = Layout,
        };
        item.LyricsWithChords.Should().Contain("»");
        var cut = Layout.Paginate(item.LyricsWithChords!, null, null, ScreenHeight, Panel, null, null, 50).Bodies;
        var plain = Layout.Paginate(lyrics, null, null, ScreenHeight, Panel, null, null, 50).Bodies;
        cut.Should().HaveCount(plain.Count).And.HaveCount(2);
        var session = new LiveSessionService();

        for (int page = 0; page < cut.Count; page++)
        {
            var (region1, region2) = SongPageLayout.SplitRegions(plain[page]);
            session.GoLive(item with { LyricsPageIndex = page, ProjectedTextPage = cut[page] }, "projector");
            session.Current.CurrentItemBodyText.Should().Be(region1);
            session.Current.CurrentItemBodyText2.Should().Be(region2);
            session.GoLive(item with { LyricsPageIndex = page, ProjectedTextPage = cut[page], ShowNotations = true, TransposeSemitones = 2 }, "projector");
            session.Current.CurrentItemBodyText.Split('\n').Where((_, index) => !session.Current.NotationLines.Contains(index)).Should().Equal(region1.Split('\n'));
            session.Current.CurrentItemBodyText2.Split('\n').Where((_, index) => !session.Current.NotationLines2.Contains(index)).Should().Equal(region2.Split('\n'));
        }
        session.GoLive(item with { ProjectedTextPage = cut[0], ShowNotations = true, TransposeSemitones = 2 }, "projector");
        session.Current.CurrentItemBodyText.Split('\n')[0].Should().Contain("A").And.Contain("E", "G and D two semitones up");
        session.Current.NotationLines2.Should().Equal([0], "the chord of 줄 1 stands above it in region 2");
    }
}

// The view-model flows of the review.
public partial class MainViewModelTests
{
    private static SongPageLayout ReviewLayout() => new(30, "Arial", 0, 5, 5, 0, 1, 100, 0)
    {
        Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Vpos1"] = "50", ["Size1"] = "30", ["FontName1"] = "Arial" },
    };

    private static string ReviewLines(string word, int count) => string.Join("\n", Enumerable.Range(1, count).Select(line => $"{word} {line}"));

    private static SongSummary ReviewSong() => new(4101, "Review Song", "", 1, 0, "", "",
        $"[1]\n{ReviewLines("line", 40)}\n[region 2]\n{ReviewLines("줄", 40)}\n[chorus]\nrefrain\n[region 2]\n후렴");

    [Fact]
    public void Review_TwoLanguageSong_AutoTextOverflowSwitchedOn_ThePreviewCountFollowsTheCutPages()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AutoTextOverflow, false);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        var layout = ReviewLayout();
        sut.Library.Folders.Add(new SongFolderSummary(1, "Folder 1", false, 0, SlideLayout: layout));
        var song = ReviewSong();
        int cut = layout.Paginate(song.Lyrics, null, null, sut.SampleScreenHeight, settings.Get(AxicultusSettingKeys.PanelHeightPercent), null, null, 50).Bodies.Count;
        cut.Should().BeGreaterThan(3);
        sut.AddSong(song);
        sut.LyricsPageCount.Should().Be(2);

        settings.Set(AxicultusSettingKeys.AutoTextOverflow, true).Succeeded.Should().BeTrue();
        sut.NextLyricsPageCommand.Execute(null);

        sut.PreviewLyricsPages.Should().HaveCount(cut, "the cards are built from the cut pages");
        sut.LyricsPageCount.Should().Be(cut, "the count the preview navigates by is that of the cards");
        sut.NextLyricsPageCommand.CanExecute(null).Should().BeTrue("page 2 of several is not the last");
    }

    [Fact]
    public void Review_TwoLanguageSong_WideMonitorSwitched_ThePreviewCountFollowsTheCutPages()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AutoTextOverflow, true);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        var layout = ReviewLayout();
        sut.Library.Folders.Add(new SongFolderSummary(1, "Folder 1", false, 0, SlideLayout: layout));
        var song = ReviewSong();
        int panel = settings.Get(AxicultusSettingKeys.PanelHeightPercent);
        double before = sut.SampleScreenHeight, after = before == 540 ? 720 : 540;
        int cutBefore = layout.Paginate(song.Lyrics, null, null, before, panel, null, null, 50).Bodies.Count;
        int cutAfter = layout.Paginate(song.Lyrics, null, null, after, panel, null, null, 50).Bodies.Count;
        cutAfter.Should().NotBe(cutBefore, "the two screen shapes cut the verse differently");
        sut.AddSong(song);
        sut.LyricsPageCount.Should().Be(cutBefore);

        settings.Set(AxicultusSettingKeys.IsMonitorWide, after == 540).Succeeded.Should().BeTrue();
        sut.SampleScreenHeight.Should().Be(after);
        sut.GoToPreviewLyricsPageCommand.Execute(1);

        sut.PreviewLyricsPages.Should().HaveCount(cutAfter);
        sut.LyricsPageCount.Should().Be(cutAfter, "the count the preview navigates by is that of the cards");
    }

    [Fact]
    public async Task Review_TwoLanguageSong_EveryConsumerReadsTheSameCutPage()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        settings.Set(AxicultusSettingKeys.AutoTextOverflow, true);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        var layout = ReviewLayout();
        sut.Library.Folders.Add(new SongFolderSummary(1, "Folder 1", false, 0, SlideLayout: layout));
        var song = ReviewSong();
        var expected = layout.Paginate(song.Lyrics, null, null, sut.SampleScreenHeight, settings.Get(AxicultusSettingKeys.PanelHeightPercent), null, null, 50);
        var regions = expected.Bodies.ConvertAll(SongPageLayout.SplitRegions);
        int last = expected.Bodies.Count - 1;
        expected.Labels[last].Should().Be("chorus");

        sut.AddSong(song);
        var row = sut.Queue.Single();

        // The preview pane draws the page of the cards.
        for (int page = 0; page <= last; page++)
        {
            var projected = sut.ProjectPreviewLyricsPage(row, page);
            projected.ProjectedTextPage.Should().Be(expected.Bodies[page]);
            projected.ProjectedSectionLabel.Should().Be(expected.Labels[page]);
        }
        sut.ProjectLyricsPage(row, 999).LyricsPageIndex.Should().Be(last, "a stored page index past the end is the last page");

        // The verse keys jump to the first screen of the verse; the output follows the cut pages one by one.
        sut.JumpToLyricsSectionCommand.Execute("c");
        sut.LyricsPageIndex.Should().Be(last);
        sut.JumpToLyricsSectionCommand.Execute("1");
        sut.LyricsPageIndex.Should().Be(0);
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.OutputLyricsPages.Should().HaveCount(expected.Bodies.Count);
        for (int page = 1; page <= last; page++)
        {
            await sut.NextOutputSlideCommand.ExecuteAsync(null);
            sut.Session.Current.CurrentLyricsPageIndex.Should().Be(page);
            sut.Session.Current.CurrentItemBodyText.Should().Be(regions[page].Region1);
            sut.Session.Current.CurrentItemBodyText2.Should().Be(regions[page].Region2);
            sut.Session.Current.CurrentSectionLabel.Should().Be(expected.Labels[page]);
            sut.Session.Current.CurrentItemPositionLabel.Should().Be($"{page + 1}/{expected.Bodies.Count}");
        }
        sut.Session.Current.CurrentPageIsChorus.Should().BeTrue();
        await sut.PreviousOutputSlideCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemBodyText.Should().Be(regions[last - 1].Region1);
        sut.Session.Current.CurrentPageIsChorus.Should().BeFalse();
        sut.JumpToOutputLyricsSectionCommand.Execute("c");
        sut.Session.Current.CurrentItemBodyText.Should().Be("refrain");
        sut.Session.Current.CurrentItemBodyText2.Should().Be("후렴");

        // The chord switch does not change the pages.
        settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, true).Succeeded.Should().BeTrue();
        sut.LyricsPageCount.Should().Be(expected.Bodies.Count);
        sut.Session.Current.CurrentItemBodyText.Should().Be("refrain");
    }
}
