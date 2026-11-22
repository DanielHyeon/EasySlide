using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// Adversarial review of the two-version Bible flow (SongPageLayout.PaginateRegions, MainViewModel.GetSongPages,
/// LiveSessionService). A failing test is a defect; a passing test is something that was tried and held.
/// This class has the tests of the page arithmetic; the view-model flows are in the MainViewModelTests part below.
/// </summary>
public class BibleFlowAdversarialTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private static readonly SongPageLayout Layout = new(12, "Arial", 0, 5, 5, 0, 1, 100, 0);

    private static string English(int verse) => $"1:{verse} In the beginning God created the heaven and the earth.";
    private static string Korean(int verse) => $"1:{verse} 태초에 하나님이 천지를 창조하시니라";

    // The height of one line of the layout's font, as the paginator measures it.
    private static double LineHeight(SongPageLayout layout)
    {
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font(layout.FontName, layout.FontPoints);
        return graphics.MeasureString("A", font, new SizeF(960, 32000)).Height;
    }

    // ── Defect: a stretch of verses the first version lacks is not measured at all ────────────────────────────

    [Fact]
    public void RegionOneWithoutText_RegionTwoStillFlowsOverTheScreens()
    {
        // gfLyrics.BuildSlides (the else branch, GetScreensRequired with InItem.Lyrics[1]): a block that has no
        // region 1 lines cuts its screens by region 2. Here a verse without region 1 text needs no room, so the
        // whole passage of the second version lands on one screen, far more lines than the screen is high.
        var verses = Enumerable.Range(1, 60).Select(verse => new LyricsRegionPage("", English(verse))).ToArray();

        var pages = Layout.PaginateRegions(verses, null, 720, 6, null, 50).ConvertAll(SongPageLayout.SplitRegions);

        int most = pages.Max(page => page.Region2.Split('\n').Length);
        (most * LineHeight(Layout)).Should().BeLessThanOrEqualTo(720,
            $"a 720 pixel screen cannot show {most} lines of {LineHeight(Layout):0.#} pixels ({pages.Count} screen(s) for 60 verses)");
    }

    // ── Tried: the cut itself ──────────────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(720, 50, null)]
    [InlineData(720, 0, null)]     // region 2 at the very top: no room for region 1
    [InlineData(720, 100, null)]   // region 2 at the bottom edge
    [InlineData(720, 10, 30)]      // region 2 above region 1
    [InlineData(0, 50, null)]
    [InlineData(1, 50, null)]
    [InlineData(60, 50, null)]
    [InlineData(540, 50, 100)]
    public void AnyGeometry_ShowsEveryVerseOnceInOrder_AndTheSameVersesInBothRegions(double height, int region2Top, int? itemTop)
    {
        var verses = Enumerable.Range(1, 40).Select(verse => new LyricsRegionPage(Korean(verse), English(verse))).ToArray();

        var pages = Layout.PaginateRegions(verses, null, height, 6, itemTop, region2Top).ConvertAll(SongPageLayout.SplitRegions);

        pages.SelectMany(page => page.Region1.Split('\n')).Should().Equal(verses.Select(verse => verse.Region1));
        pages.SelectMany(page => page.Region2.Split('\n')).Should().Equal(verses.Select(verse => verse.Region2));
        foreach (var (region1, region2) in pages)
            region2.Split('\n').Select(line => line[..line.IndexOf(' ')]).Should().Equal(region1.Split('\n').Select(line => line[..line.IndexOf(' ')]));
        if (region2Top is 0 or 10 || height < 100) pages.Should().HaveCount(40, "without room a verse is a screen, none is dropped");
    }

    [Fact]
    public void BracketsQuotationMarksAndAOneVersePassage_KeepTheirText()
    {
        // A verse with [brackets], with the quotation marks » « that a song reads as a chord marker, and a passage
        // of one verse (one [region 2] marker only, which LyricsDisplayFormatter reads the song way).
        string first = "하나님이 이르시되 »빛이 있으라« 하시니 [빛이] 있었고 (창세기 1:3)";
        string second = "And God said, »Let there be light«: and there was [light]. (Genesis 1:3)";
        string body = BiblePassageComposer.Compose([first], [second]);

        var guarded = LyricsDisplayFormatter.GetRegionPages(LyricsDisplayFormatter.GuardLiteralNotation(body), null)
            .Select(verse => new LyricsRegionPage(LyricsDisplayFormatter.UnguardLiteralNotation(verse.Region1),
                LyricsDisplayFormatter.UnguardLiteralNotation(verse.Region2))).ToArray();
        var page = Layout.PaginateRegions(guarded, null, 720, 6, null, 50).Should().ContainSingle().Subject;

        SongPageLayout.SplitRegions(page).Should().Be((first, second));
    }

    // ── Tried: the Measure refactor must not move a single page of Paginate ───────────────────────────────────

    // SongPageLayout.Paginate as it was before the change (base), kept here as the oracle.
    private static (List<string> Bodies, List<string> Labels) PaginateBefore(SongPageLayout layout, string lyrics, string? sequence,
        SongFormatData? format, double height, int panelPercent, int? itemTop = null)
    {
        const int width = 960;
        var guarded = LyricsDisplayFormatter.GuardLiteralNotation(lyrics);
        var verses = LyricsDisplayFormatter.ToVersePages(guarded, sequence);
        var labels = LyricsDisplayFormatter.GetSectionLabels(guarded, sequence);
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        var fontName = string.IsNullOrWhiteSpace(format?.FontName1) ? layout.FontName : format.FontName1;
        var points = Math.Clamp(format?.FontSize1 ?? layout.FontPoints, 1, 500);
        if (string.IsNullOrWhiteSpace(fontName)) fontName = FontFamily.GenericSansSerif.Name;
        var style = (FontStyle)(layout.FontStyleBits & 7);
        using var font = CreateFont(fontName, points, style);
        using var heading = CreateFont(fontName, Math.Max(1, points * Math.Clamp(layout.HeadingPercent, 1, 200) / 100), style);
        float textWidth = width * Math.Clamp(100 - (format?.BodyLeftMargin ?? layout.LeftPercent) - (format?.BodyRightMargin ?? layout.RightPercent), 1, 100) / 100f;
        double bodyHeight = height * Math.Clamp(1 - 0.022 - (Math.Clamp(panelPercent, 0, 100) / 100d + 0.03)
            - (format?.BodyBottomMargin ?? layout.BottomPercent) / 100d, 0.01, 1);
        double reservedHeading = Math.Min(bodyHeight / 4, graphics.MeasureString("A", heading).Height * 1.1);
        double availableHeight = (bodyHeight - height * Math.Clamp(itemTop ?? layout.TopPercent, 0, 100) / 100d - reservedHeading)
            / (Math.Clamp(layout.LineSpacing, 0.5, 2) + 0.05);
        var lineArea = new SizeF(textWidth, 32000);
        double lineHeight = graphics.MeasureString("A", font, lineArea).Height;
        List<string> pages = [], pageLabels = [];
        for (int verse = 0; verse < verses.Count; verse++)
        {
            var lines = LyricsDisplayFormatter.UnguardLiteralNotation(verses[verse]).Split('\n');
            int first = 0;
            double used = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string text = LyricsDisplayFormatter.StripInlineNotation(lines[i]);
                const int legacyPrefixLength = 8;
                if (text.Length > legacyPrefixLength && text.AsSpan(0, legacyPrefixLength).Contains(' '))
                    text = string.Concat(text[..legacyPrefixLength].Replace(' ', '_'), text.AsSpan(legacyPrefixLength));
                double required = global::Axicultus.LyricLineMeasurement.Count(text, font, graphics, textWidth) * lineHeight;
                if (i > first && used + required > availableHeight)
                {
                    AddPage(i);
                    first = i;
                    used = 0;
                }
                used += required;
            }
            AddPage(lines.Length);
            void AddPage(int end)
            {
                pages.Add(string.Join("\n", lines, first, end - first));
                pageLabels.Add(verse < labels.Count ? labels[verse] : "");
            }
        }
        return (pages, pageLabels);

        static Font CreateFont(string name, float size, FontStyle fontStyle)
        {
            try { return new Font(name, size, fontStyle); }
            catch (ArgumentException) { return new Font(FontFamily.GenericSansSerif, size, fontStyle); }
        }
    }

    private static readonly string[] LinePool =
    [
        "주의 친절한 팔에 안기세", "우리 맘이 평안하리니 항상 기쁘고 복이 되겠네 영원하신 팔에 안기세", "Amazing grace how sweet the sound",
        "That saved a wretch like me I once was lost but now am found was blind but now I see and grace my fears relieved",
        "[1]", "[2]", "[3]", "[chorus]", "[bridge]", "[ending]", "[prechorus]", "[x2]", "[region 2]", "", "", " ", "   ",
        "주 예수 » G D Em", "»인용« 부호가 있는 줄", "Er sprach: »Es werde Licht« und es ward Licht", "A  B  C  D 띄어쓰기가 많은 머리",
        "Supercalifragilisticexpialidociousandmuchmuchlongerwithoutanyspaceatallinsidethewholelineofthistextthatneverends",
        "1:1 태초에 하나님이 천지를 창조하시니라", "2 땅이 혼돈하고 공허하며 흑암이 깊음 위에 있고 하나님의 신은 수면에 운행하시니라",
        "In the beginning God created the heaven and the earth. (Genesis 1:1)", "[~G D Em C]", "主を賛美します", "Â»mojibake",
    ];

    // A line of spaces: before, it took no room and was dropped at the ends of a screen; FrmMain counts it as a row.
    private static bool HasSpaceRow(string lyrics)
        => lyrics.Replace("\r", "").Split('\n').Any(line => line.Length > 0 && string.IsNullOrWhiteSpace(line));

    private static string[] TextLines(IEnumerable<string> bodies)
        => [.. bodies.SelectMany(body => body.Split('\n')).Where(line => !string.IsNullOrWhiteSpace(line))];

    private static string RandomLyrics(Random random)
    {
        int count = random.Next(1, 40);
        var lines = new string[count];
        for (int i = 0; i < count; i++) lines[i] = LinePool[random.Next(LinePool.Length)];
        return string.Join(random.Next(4) == 0 ? "\r\n" : "\n", lines);
    }

    [Fact]
    public void Paginate_GivesTheSamePagesAsBeforeTheMeasureRefactor_UnlessALineIsHigherThanAScreen()
    {
        var random = new Random(20261122);
        string[] fonts = ["Arial", "Malgun Gothic", "Times New Roman", "", "No Such Font At All"];
        string?[] sequences = [null, null, "1 c 2 c", "c", "2,1", "b e 3"];
        var differences = new List<string>();
        int withReducedPage = 0;
        for (int run = 0; run < 400; run++)
        {
            var layout = new SongPageLayout(random.Next(6, 90), fonts[random.Next(fonts.Length)], random.Next(0, 45), random.Next(0, 30),
                random.Next(0, 30), random.Next(0, 25), Math.Round(0.4 + random.NextDouble() * 1.8, 2), random.Next(0, 220), random.Next(0, 32));
            string lyrics = RandomLyrics(random);
            string? sequence = sequences[random.Next(sequences.Length)];
            var format = random.Next(3) == 0
                ? SongFormatData.Parse($"47={random.Next(6, 100)}>43={fonts[random.Next(fonts.Length)]}>64={random.Next(0, 40)}>65={random.Next(0, 40)}>66={random.Next(0, 60)}>")
                : null;
            double height = random.Next(5) switch { 0 => 540, 1 => 0, 2 => 97, _ => 720 };
            int panel = random.Next(-5, 30);
            int? top = random.Next(3) == 0 ? random.Next(0, 101) : null;

            var before = PaginateBefore(layout, lyrics, sequence, format, height, panel, top);
            var scales = new List<double>();
            var now = layout.Paginate(lyrics, sequence, format, height, panel, top, scales);
            scales.Should().HaveSameCount(now.Bodies);
            scales.Should().NotContain(scale => scale <= 0 || scale > 1);

            if (scales.Exists(scale => scale < 1)) withReducedPage++;
            if (scales.Exists(scale => scale < 1) || HasSpaceRow(lyrics))
            {
                // A first line higher than the screen is counted with a reduced font (gfLyrics.GetOneScreen), so its
                // screen may hold more lines than before; a line of spaces is now a row of its screen, so a screen
                // with one may hold fewer. No line is lost or moved.
                if (!TextLines(before.Bodies).SequenceEqual(TextLines(now.Bodies)))
                    differences.Add($"run {run}: lines differ with a reduced page or a line of spaces ({layout})");
            }
            else if (!before.Bodies.SequenceEqual(now.Bodies) || !before.Labels.SequenceEqual(now.Labels))
                differences.Add($"run {run}: {before.Bodies.Count} pages before, {now.Bodies.Count} now ({layout})");
        }

        output.WriteLine($"400 generated texts, {differences.Count} differ, {withReducedPage} have a page with a reduced font");
        differences.Should().BeEmpty("without a line higher than a screen the pages are those of before");
        withReducedPage.Should().BeGreaterThan(0, "the generated texts must reach the reduced-font rule");
    }

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

    [RealSongDatabaseFact]
    public void Paginate_GivesTheSamePagesAsBeforeTheMeasureRefactor_OverTheRealSongsWithTheirFolderLayouts()
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
                layouts[Number(reader, "FolderNo")] = new SongPageLayout(Number(reader, "Size0"), Text(reader, "Fontname0"), Number(reader, "VPos0"),
                    Number(reader, "LMargin"), Number(reader, "RMargin"), Number(reader, "BMargin"),
                    double.TryParse(Text(reader, "LineSpacing"), System.Globalization.CultureInfo.InvariantCulture, out var spacing) && spacing is >= 0.5 and <= 2 ? spacing : 1,
                    Number(reader, "HeadingSize"), Number(reader, "BIU0"));
            }
        }
        layouts.Should().NotBeEmpty("the database has folders with a layout");
        var fallback = new SongPageLayout(40, "Malgun Gothic", 0, 2, 2, 0, 1, 100, 0);

        int songs = 0, pages = 0, withSpaceRow = 0;
        var differences = new List<string>();
        using (var command = new SQLiteCommand("SELECT * FROM SONG ORDER BY SONGID", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string lyrics = Text(reader, "LYRICS");
                if (lyrics.Length == 0) continue;
                songs++;
                string sequence = Text(reader, "SEQUENCE"), formatData = Text(reader, "FORMATDATA");
                var item = new LiveQueueItem("song:" + Text(reader, "SONGID"), "song", LiveItemKinds.Song)
                {
                    Lyrics = lyrics, Notations = Text(reader, "MSC"), Sequence = sequence.Length == 0 ? null : sequence,
                };
                var layout = layouts.GetValueOrDefault(Number(reader, "FOLDERNO"), fallback);
                var format = SongFormatData.Parse(formatData.Length == 0 ? null : formatData);
                int? top = ItemDisplayFormat.Read(formatData)?.Top1;
                foreach (double height in songs % 4 == 0 ? new[] { 720d, 540d } : [720d])
                {
                    var before = PaginateBefore(layout, item.LyricsWithChords!, item.Sequence, format, height, 6, top);
                    var now = layout.Paginate(item.LyricsWithChords!, item.Sequence, format, height, 6, top);
                    pages += now.Bodies.Count;
                    if (HasSpaceRow(lyrics))
                    {
                        withSpaceRow++;
                        if (!TextLines(before.Bodies).SequenceEqual(TextLines(now.Bodies)))
                            differences.Add($"song {Text(reader, "SONGID")} at {height}: lines differ in a song with a line of spaces");
                    }
                    else if (!before.Bodies.SequenceEqual(now.Bodies) || !before.Labels.SequenceEqual(now.Labels))
                        differences.Add($"song {Text(reader, "SONGID")} at {height}: {before.Bodies.Count} pages before, {now.Bodies.Count} now");
                }
            }
        }

        output.WriteLine($"{songs} songs with lyrics, {layouts.Count} folder layouts, {pages} pages compared ({withSpaceRow} runs of a song with a line of spaces), {differences.Count} differ");
        songs.Should().BeGreaterThan(0);
        pages.Should().BeGreaterThan(songs / 2);
        differences.Should().BeEmpty($"{songs} songs, {pages} pages");
    }

    // The real song pages, cut as MainViewModel.GetSongPages cuts them, drawn on the output with the WinForms row
    // height: does each still fit the room OutputWindowViewModel gives region 1 (BodyTextMaxHeight)?
    [RealSongDatabaseFact]
    public void RealSongPages_FitTheirRoomWithTheWinFormsRowHeight()
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
                if (Number(reader, "Size0") is not (> 0 and <= 500)) continue;
                var fields = FolderFormatField.All.ToDictionary(field => field.Column, field => Text(reader, field.Column));
                layouts[Number(reader, "FolderNo")] = new SongPageLayout(Number(reader, "Size0"), Text(reader, "Fontname0"), Number(reader, "VPos0"),
                    Number(reader, "LMargin"), Number(reader, "RMargin"), Number(reader, "BMargin"),
                    double.TryParse(Text(reader, "LineSpacing"), System.Globalization.CultureInfo.InvariantCulture, out var spacing) && spacing is >= 0.5 and <= 2 ? spacing : 1,
                    Number(reader, "HeadingSize"), Number(reader, "BIU0")) { Fields = fields };
            }
        }
        var songs = new List<(string Id, LiveQueueItem Item, SongPageLayout Layout, SongFormatData? Format, int? Top)>();
        using (var command = new SQLiteCommand("SELECT * FROM SONG ORDER BY SONGID", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string lyrics = Text(reader, "LYRICS"), formatData = Text(reader, "FORMATDATA");
                if (lyrics.Length == 0 || !layouts.TryGetValue(Number(reader, "FOLDERNO"), out var layout)) continue;
                var item = new LiveQueueItem("song:" + Text(reader, "SONGID"), "song", LiveItemKinds.Song)
                {
                    Lyrics = lyrics, Notations = Text(reader, "MSC"), Sequence = Text(reader, "SEQUENCE") is { Length: > 0 } sequence ? sequence : null,
                    SongLayout = layout, FormatData = formatData.Length == 0 ? null : formatData, UseIndividualFormatting = formatData.Length > 0,
                };
                songs.Add((Text(reader, "SONGID"), item, layout, SongFormatData.Parse(item.FormatData), ItemDisplayFormat.Read(item.FormatData)?.Top1));
            }
        }
        songs.Should().NotBeEmpty();

        StaHelper.RunOnSta(() =>
        {
            foreach (var (width, height) in new[] { (1920, 1080), (2048, 1152) })
            {
                var session = new LiveSessionService();
                using var view = new OutputWindowViewModel();
                view.ApplyOutput(new(true, new OutputDisplay("projector", "Projector", 0, 0, width, height, 1), new(0, 0, width, height, false)));
                int pages = 0, before = 0, after = 0, reduced = 0, plain = 0, chordPages = 0, chordsShrinkBefore = 0, chordsShrinkAfter = 0;
                double tightest = 0;
                string tightestPage = "";
                var examples = new List<string>();
                int regionRooms = 0, regionsBefore = 0, regionsAfter = 0, twoLanguageSongs = 0, region2Smaller = 0, cutScreens = 0;
                var regionExamples = new List<string>();
                var region2OnlyVerseSongs = new List<string>();
                double Needs(LyricParagraphStyle style)
                {
                    var surface = new LyricParagraphSurface { Paragraph = style with { Height = double.PositiveInfinity, ShrinkToFit = false } };
                    surface.Measure(new System.Windows.Size(style.Width, double.PositiveInfinity));
                    return surface.DesiredSize.Height;
                }
                foreach (var (id, item, layout, format, top) in songs)
                {
                    // A two-language song, as GetSongPages cuts it: region 1 decides the screens in the room above
                    // region 2, so region 1 fits its room at its own size; region 2 is drawn as large as fits.
                    if (LyricsDisplayFormatter.HasRegion2(item.LyricsWithChords!))
                    {
                        var regionScales = new List<double>();
                        var screens = layout.Paginate(item.LyricsWithChords!, item.Sequence, format, 540, 0, top, regionScales,
                            ItemDisplayFormat.Read(item.FormatData)?.Top2 ?? layout.Integer("Vpos1") ?? 50).Bodies;
                        twoLanguageSongs++;
                        cutScreens += screens.Count;
                        regionsBefore += LyricsDisplayFormatter.GetRegionPages(item.LyricsWithChords, item.Sequence).Count;
                        if (RevRealDataParityTests.WinForms(item.Lyrics!, item.Sequence ?? "", out _).Any(slide => slide.Verse >= 0 && slide.R1.Length == 0 && slide.R2.Length > 0))
                            region2OnlyVerseSongs.Add(id);
                        for (int page = 0; page < screens.Count; page++)
                        {
                            session.GoLive(item with { LyricsPageIndex = page, ProjectedTextPage = screens[page], ProjectedFontScale = regionScales[page] }, "projector");
                            view.ApplySession(session.Current);
                            if (view.SecondaryParagraph is { } second && Needs(second) > second.Height + .5) region2Smaller++;
                            if (view.PrimaryParagraph is not { } region) continue;
                            regionRooms++;
                            if (Needs(region) > region.Height + .5)
                            {
                                regionsAfter++;
                                if (regionExamples.Count < 10) regionExamples.Add($"song {id} screen {page + 1}: needs {Needs(region):F0} of {region.Height:F0} (font {region.Size:F0}, scale {regionScales[page]:F2}, {layout.FontName} {layout.FontPoints}pt x{layout.LineSpacing})");
                            }
                        }
                        continue;
                    }
                    var scales = new List<double>();
                    var cut = layout.Paginate(item.LyricsWithChords!, item.Sequence, format, 540, 0, top, scales);
                    for (int page = 0; page < cut.Bodies.Count; page++)
                    {
                        session.GoLive(item with { LyricsPageIndex = page, ProjectedTextPage = cut.Bodies[page], ProjectedFontScale = scales[page] }, "projector");
                        view.ApplySession(session.Current);
                        if (view.PrimaryParagraph is not { } paragraph) { plain++; continue; }
                        pages++;
                        if (scales[page] < 1) reduced++;
                        // Before: the row was the larger of WPF's own line height and the folder spacing in ems.
                        double oldLeading = Math.Clamp(layout.LineSpacing, .5, 2);
                        bool fitBefore = Needs(paragraph with { Leading = oldLeading }) <= paragraph.Height + .5;
                        double needs = Needs(paragraph);
                        if (!fitBefore) before++;
                        if (needs / paragraph.Height > tightest) (tightest, tightestPage) = (needs / paragraph.Height, $"song {id} page {page + 1}: {needs:F0} of {paragraph.Height:F0}");
                        if (needs > paragraph.Height + .5)
                        {
                            after++;
                            if (examples.Count < 15) examples.Add($"song {id} page {page + 1}: needs {needs:F0} of {paragraph.Height:F0} (font {paragraph.Size:F0}, scale {scales[page]:F2}, {layout.FontName} {layout.FontPoints}pt x{layout.LineSpacing})");
                        }
                        // With the chords shown the page is cut the same and drawn smaller when its chord rows do not fit.
                        if (string.IsNullOrEmpty(item.Notations)) continue;
                        session.GoLive(item with { LyricsPageIndex = page, ProjectedTextPage = cut.Bodies[page], ProjectedFontScale = scales[page], ShowNotations = true }, "projector");
                        view.ApplySession(session.Current);
                        if (view.PrimaryParagraph is not { NotationLines.Length: > 0 } withChords) continue;
                        chordPages++;
                        if (Needs(withChords with { Leading = oldLeading }) > withChords.Height + .5) chordsShrinkBefore++;
                        if (Needs(withChords) > withChords.Height + .5) chordsShrinkAfter++;
                    }
                }
                output.WriteLine($"{width}x{height}: {songs.Count} songs, {pages} pages ({reduced} with a reduced font, {plain} without a surface); " +
                    $"rows taller than the room: {before} before, {after} with the WinForms row height; tightest {tightest:P1} ({tightestPage}); " +
                    $"{chordPages} pages with chord rows drawn smaller to fit: {chordsShrinkBefore} before, {chordsShrinkAfter} now");
                examples.ForEach(output.WriteLine);
                output.WriteLine($"{width}x{height}: {twoLanguageSongs} two-language songs, {regionsBefore} screens as written, {cutScreens} after the cut ({regionRooms} with region 1); " +
                    $"region 1 taller than its room on {regionsAfter}; region 2 drawn smaller on {region2Smaller}; " +
                    $"{region2OnlyVerseSongs.Count} songs with a verse in region 2 only ({string.Join(",", region2OnlyVerseSongs.Take(12))})");
                regionExamples.ForEach(output.WriteLine);
                pages.Should().BeGreaterThan(songs.Count / 2);
                after.Should().Be(0, "a page cut with the WinForms rows fits the room WPF gives it");
                regionsAfter.Should().Be(0, "region 1 of a two-language song is cut to the room above region 2");
            }
        });
    }
}

// The view-model flows of a Bible passage in two versions.
public partial class MainViewModelTests
{
    private static readonly string[] FlowKorean = [.. Enumerable.Range(1, 60).Select(verse => $"1:{verse} 태초에 하나님이 천지를 창조하시니라")];
    private static readonly string[] FlowEnglish = [.. Enumerable.Range(1, 60).Select(verse => $"1:{verse} In the beginning God created the heaven and the earth.")];
    private const string FlowMarker = "[region 2]";

    // A song folder's layout as the database gives it: with the format fields, region 2 at Vpos1.
    private static SongPageLayout FlowFolderLayout(int points = 14, int vpos1 = 50) => new(points, "Arial", 0, 5, 5, 0, 1, 100, 0)
    {
        Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Vpos1"] = vpos1.ToString(), ["Size1"] = points.ToString(), ["FontName1"] = "Arial" },
    };

    // What the running app has: both Bible versions listed, their song folder with a layout.
    private static void FlowListVersions(MainViewModel sut, int points = 14, int vpos1 = 50)
    {
        sut.Bible.Versions.Add(new BibleVersion(0, "개역", "", "", "krv.db", "", 3, 100, false));
        sut.Bible.Versions.Add(new BibleVersion(1, "KJV", "", "", "kjv.db", "", 3, 100, false));
        sut.Library.Folders.Add(new SongFolderSummary(3, "말씀", true, 0, SlideLayout: FlowFolderLayout(points, vpos1)));
    }

    private static MainViewModel FlowSut(ISettingsService? settings = null, int points = 14, int vpos1 = 50, IWorshipListStore? store = null, bool listed = true)
    {
        var sut = CreateSut(settings: settings, worshipLists: store, seedSampleQueue: false);
        if (listed) FlowListVersions(sut, points, vpos1);
        return sut;
    }

    private static LiveQueueItem FlowPassage(string[]? first = null, string[]? second = null, string id = "0;krv.db;kjv.db;1;1;1;1;60;")
        => new(id, "창세기 1", LiveItemKinds.Bible) { Lyrics = BiblePassageComposer.Compose(first ?? FlowKorean, second ?? FlowEnglish) };

    private static async Task<MainViewModel> FlowLiveAsync(LiveQueueItem passage, MainViewModel? sut = null)
    {
        sut ??= FlowSut();
        sut.LoadQueue([passage, new LiveQueueItem("song:after", "찬양", LiveItemKinds.Song) { Lyrics = "one\n\ntwo" }]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);
        return sut;
    }

    // Every text an operator or the audience reads at this moment.
    private static IEnumerable<string> FlowVisibleTexts(MainViewModel sut)
        => sut.PreviewLyricsPages.Select(card => card.BodyText)
            .Concat(sut.OutputLyricsPages.Select(card => card.BodyText))
            .Concat([sut.Session.Current.CurrentItemBodyText, sut.Session.Current.CurrentItemBodyText2, sut.OutputLyricsText,
                sut.PreviewLyricsText, sut.StatusText, sut.Session.Current.CurrentSectionLabel, sut.Session.Current.CurrentItemPositionLabel]);

    // ── Defect 1: the pages of a two-version passage are measured again on every call ─────────────────────────

    [Fact]
    public void BibleFlow_TwoVersionPassageOfListedVersions_IsPaginatedOnce_NotOnEveryCall()
    {
        // MainViewModel.GetSongPages keys its cache by the layout record. ResolveBibleLayout builds the layout of a
        // two-version passage with a new Fields dictionary each time, and a record compares that by reference: the
        // key never matches, so every page count, card list and projection measures the whole passage again.
        var sut = FlowSut();
        sut.LoadQueue([FlowPassage()]);
        var row = sut.Queue[0];

        var first = sut.ProjectLyricsPage(row, 0);
        var second = sut.ProjectLyricsPage(row, 0);

        second.ProjectedTextPage.Should().Be(first.ProjectedTextPage);
        ReferenceEquals(first.ProjectedTextPage, second.ProjectedTextPage).Should().BeTrue(
            "the second call should come from the cache (the same page instance), as it does for a song or a passage in one version; " +
            $"layouts equal: {first.SongLayout == second.SongLayout}");
    }

    [Theory]
    [InlineData(true, false)]   // one version, listed: cached
    [InlineData(false, true)]   // two versions, not listed (the row's own layout): cached
    public void BibleFlow_OtherPassages_ArePaginatedOnce(bool listed, bool twoVersions)
    {
        var sut = FlowSut(listed: listed);
        var passage = twoVersions ? FlowPassage() with { SongLayout = FlowFolderLayout() }
            : FlowPassage(FlowKorean, [], "0;krv.db;;1;1;1;1;60;");
        sut.LoadQueue([passage]);

        var first = sut.ProjectLyricsPage(sut.Queue[0], 0);
        var second = sut.ProjectLyricsPage(sut.Queue[0], 0);

        first.ProjectedTextPage.Should().StartWith(FlowKorean[0] + "\n" + FlowKorean[1]);
        ReferenceEquals(first.ProjectedTextPage, second.ProjectedTextPage).Should().BeTrue();
    }

    [Fact]
    public async Task BibleFlow_NextOnATwoVersionPassage_IsAboutAsFastWithListedVersionsAsWithout()
    {
        // What the operator feels of the missing cache: each Next paginates the passage about eight times.
        // Measured here: 30 verses about 0.3 s, 100 verses about 1 s, Psalm 119 about 2.5 s for one Next;
        // about a millisecond when the versions are not listed (the cache works) and before the change.
        var korean = Enumerable.Range(1, 40).Select(verse => $"{verse} 내가 진실로 진실로 너희에게 이르노니 내 말을 듣고 또 나 보내신 이를 믿는 자는 영생을 얻었고 심판에 이르지 아니하나니 사망에서 생명으로 옮겼느니라").ToArray();
        var english = Enumerable.Range(1, 40).Select(verse => $"{verse} Verily, verily, I say unto you, He that heareth my word, and believeth on him that sent me, hath everlasting life.").ToArray();

        async Task<long> FiveNextAsync(bool listed)
        {
            var passage = FlowPassage(korean, english, "0;krv.db;kjv.db;43;5;1;5;40;") with { SongLayout = FlowFolderLayout(30) };
            var sut = await FlowLiveAsync(passage, FlowSut(points: 30, listed: listed));
            sut.LyricsPageCount.Should().BeGreaterThan(6);
            await sut.NextOutputSlideCommand.ExecuteAsync(null); // warm up
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 5; i++) await sut.NextOutputSlideCommand.ExecuteAsync(null);
            return watch.ElapsedMilliseconds;
        }

        long withoutList = await FiveNextAsync(listed: false);
        long withList = await FiveNextAsync(listed: true);

        withList.Should().BeLessThan(withoutList * 10 + 250, $"five Next took {withList} ms with the versions listed and {withoutList} ms without");
    }

    // ── Defect 2: verses the first version lacks are not measured ─────────────────────────────────────────────

    [Theory]
    [InlineData(60)]   // the first version has none of the passage (a New Testament only version, a missing file)
    [InlineData(30)]   // it lacks the first half
    public async Task BibleFlow_FirstVersionLacksVerses_NoScreenGetsMoreOfTheSecondVersionThanItCanShow(int missing)
    {
        var korean = FlowKorean.ToArray();
        for (int i = 0; i < missing; i++) korean[i] = "";
        var sut = await FlowLiveAsync(FlowPassage(korean, FlowEnglish));
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font("Arial", 14);
        double lineHeight = graphics.MeasureString("A", font).Height;

        int pages = sut.LyricsPageCount, most = 0;
        var shown = new List<string>();
        for (int page = 0; page < pages; page++)
        {
            var lines = sut.Session.Current.CurrentItemBodyText2.Split('\n');
            most = Math.Max(most, lines.Length);
            shown.AddRange(lines);
            if (page < pages - 1) await sut.NextOutputSlideCommand.ExecuteAsync(null);
        }

        shown.Should().Equal(FlowEnglish, "every verse of the second version is on a screen");
        (most * lineHeight).Should().BeLessThanOrEqualTo(sut.SampleScreenHeight,
            $"a screen of {sut.SampleScreenHeight} pixels cannot show {most} lines of {lineHeight:0.#} pixels; the passage has {pages} screen(s). " +
            "Before the change each of these verses was a screen of its own");
    }

    // ── A verse higher than the screen: gfLyrics.GetOneScreen reduces the font for that screen ──────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BibleFlow_AVerseHigherThanTheScreen_IsDrawnSmallerOnItsScreenOnly(bool twoVersions)
    {
        // FrmLaunchShow draws the screen with the font the first line was reduced to (Gf.GetOneRegionHeight), and
        // region 2, which is not measured when the screens are cut, always as large as fits its room.
        var korean = FlowKorean.ToArray();
        korean[1] = "1:2 " + string.Join(" ", Enumerable.Repeat("땅이 혼돈하고 공허하며 흑암이 깊음 위에 있고", 200));
        var passage = twoVersions ? FlowPassage(korean, FlowEnglish) : FlowPassage(korean, [], "0;krv.db;;1;1;1;1;60;");
        var sut = await FlowLiveAsync(passage);

        var screens = new List<(string Body, double Scale, bool Shrink2)>();
        for (int page = 0, pages = sut.LyricsPageCount; page < pages; page++)
        {
            var now = sut.Session.Current;
            screens.Add((now.CurrentItemBodyText, now.BodyFontScale, now.ShrinkBody2));
            if (page < pages - 1) await sut.NextOutputSlideCommand.ExecuteAsync(null);
        }

        screens.Where(screen => screen.Scale < 1).Should().ContainSingle("only the screen of the high verse is drawn with a reduced font")
            .Which.Body.Should().StartWith("1:2 ");
        screens.Should().OnlyContain(screen => screen.Shrink2 == twoVersions, "region 2 of a flowed passage always fits its room");
        string.Join("\n", screens.Select(screen => screen.Body)).Should().Be(string.Join("\n", korean), "no verse is lost or moved");
    }

    [Fact]
    public async Task BibleFlow_ASongIsNotDrawnSmaller()
    {
        var sut = await FlowLiveAsync(FlowPassage());
        await sut.NextOutputItemCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("찬양");
        sut.Session.Current.BodyFontScale.Should().Be(1);
        sut.Session.Current.ShrinkBody2.Should().BeFalse();
    }

    // ── Defect 3 (was there before the change): a second version whose verses all start with » ───────────────

    [Fact]
    public async Task BibleFlow_SecondVersionVerseThatStartsWithAQuotationMark_IsShownInRegionTwo()
    {
        // A search-result selection puts the verse text first ("text (Book c:v)"). GetSongPages asks HasRegion2 of
        // the unguarded text, where » cuts the line as a chord marker: no region 2 is seen, and the passage is
        // paginated as one version, its second version a screen of its own in region 1.
        const string korean = "하나님이 세상을 이처럼 사랑하사 독생자를 주셨으니 (요한복음 3:16)";
        const string german = "»Denn so hat Gott die Welt geliebt, dass er seinen eingeborenen Sohn gab« (Johannes 3:16)";
        var passage = new LiveQueueItem("1;krv.db;kjv.db;43;3;16;3;16;", "요한복음 3:16", LiveItemKinds.Bible)
        {
            Lyrics = BiblePassageComposer.Compose([korean], [german]),
        };

        var sut = await FlowLiveAsync(passage);

        sut.Session.Current.CurrentItemBodyText2.Should().Be(german, "the second version belongs in region 2");
        sut.Session.Current.CurrentItemBodyText.Should().Be(korean);
        sut.LyricsPageCount.Should().Be(1);
    }

    // ── Defect 4: the page count does not follow a layout change made in the Bible version manager ────────────

    [Fact]
    public void BibleFlow_VersionSizeChangedInTheBibleVersionManager_ThePageCountFollowsTheCards()
    {
        // MainWindow opens BibleVersionManagerWindow on the live BibleViewModel and refreshes nothing when it closes;
        // BibleViewModel.SetVersionLayout replaces the version in Versions (this line). Before the change a
        // two-version passage was a verse a screen whatever the layout, so the count could not go stale.
        var sut = FlowSut();
        sut.LoadQueue([FlowPassage()]);
        sut.SelectedItem = sut.Queue[0];
        int before = sut.LyricsPageCount;

        sut.Bible.Versions[0] = sut.Bible.Versions[0] with { Size = 200 };
        sut.NextLyricsPageCommand.Execute(null);

        sut.PreviewLyricsPages.Count.Should().BeGreaterThan(before, "the cards are built with the larger font");
        sut.LyricsPageCount.Should().Be(sut.PreviewLyricsPages.Count, "the count the status line and the page buttons use is the count of the cards");
    }

    // ── Tried and held ─────────────────────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(false)]
    [InlineData(true)]   // the saved text with CRLF line ends
    public async Task BibleFlow_NoMarkerReachesAnyText_AndEveryVerseIsShownOnce_WithMissingVersesBracketsAndQuotationMarks(bool crlf)
    {
        var korean = FlowKorean.ToArray();
        var english = FlowEnglish.ToArray();
        korean[2] = "1:3 하나님이 이르시되 »빛이 있으라« 하시니 [빛이] 있었고";
        english[2] = "1:3 And God said, »Let there be light«: and there was [light].";
        english[7] = "";   // a verse the second version lacks
        korean[12] = "";   // a verse the first version lacks
        var passage = FlowPassage(korean, english);
        if (crlf) passage = passage with { Lyrics = passage.Lyrics!.Replace("\n", "\r\n") };
        var sut = await FlowLiveAsync(passage);

        int pages = sut.LyricsPageCount;
        pages.Should().BeInRange(2, 59);
        sut.PreviewLyricsPages.Should().HaveCount(pages);
        sut.OutputLyricsPages.Should().HaveCount(pages);
        var region1 = new List<string>();
        var region2 = new List<string>();
        for (int page = 0; page < pages; page++)
        {
            var session = sut.Session.Current;
            session.CurrentLyricsPageIndex.Should().Be(page);
            session.CurrentItemPositionLabel.Should().Be($"{page + 1}/{pages}");
            FlowVisibleTexts(sut).Should().NotContain(text => text.Contains(FlowMarker) || text.Contains('') || text.Contains("region 2", StringComparison.OrdinalIgnoreCase));
            sut.OutputLyricsText.Should().Be(sut.OutputLyricsPages[page].BodyText, "the card of the live page is the live text");
            sut.OutputLyricsPages[page].BodyText.Should().Be(sut.PreviewLyricsPages[page].BodyText);
            region1.AddRange(session.CurrentItemBodyText.Split('\n'));
            region2.AddRange(session.CurrentItemBodyText2.Split('\n'));
            if (page < pages - 1) await sut.NextOutputSlideCommand.ExecuteAsync(null);
        }

        region1.Should().Equal(korean.Where(verse => verse.Length > 0));
        region2.Should().Equal(english.Where(verse => verse.Length > 0));
    }

    [Fact]
    public async Task BibleFlow_FormatChangesWhileLive_KeepTheCountTheCardsAndThePositionInStep()
    {
        var sut = await FlowLiveAsync(FlowPassage());
        int pages = sut.LyricsPageCount;
        sut.GoToLastOutputLyricsPageCommand.Execute(null);
        sut.Session.Current.CurrentItemPositionLabel.Should().Be($"{pages}/{pages}");

        void InStep(string because)
        {
            int count = sut.LyricsPageCount, index = sut.Session.Current.CurrentLyricsPageIndex;
            sut.OutputLyricsPages.Should().HaveCount(count, because);
            sut.PreviewLyricsPages.Should().HaveCount(count, because);
            index.Should().BeInRange(0, count - 1, because);
            sut.Session.Current.CurrentItemPositionLabel.Should().Be(count > 1 ? $"{index + 1}/{count}" : "", because);
            sut.OutputLyricsPages.Single(card => card.IsCurrent).PageIndex.Should().Be(index, because);
            sut.OutputLyricsText.Should().Be(sut.OutputLyricsPages[index].BodyText, because);
            sut.Session.Current.CurrentItemBodyText2.Split('\n').Select(line => line[..line.IndexOf(' ')])
                .Should().Equal(sut.Session.Current.CurrentItemBodyText.Split('\n').Select(line => line[..line.IndexOf(' ')]), because);
        }

        sut.SecondaryTopPercent = 90; // region 2 lower: fewer screens, the live index is past the end
        sut.LyricsPageCount.Should().BeLessThan(pages);
        InStep("region 2 at 90%");
        sut.SecondaryTopPercent = 20;
        sut.LyricsPageCount.Should().BeGreaterThan(pages);
        InStep("region 2 at 20%");
        sut.PrimaryFontPoints = 40;
        sut.LyricsPageCount.Should().Be(60, "at 40 points a verse fills the room above region 2");
        InStep("40 points");
        sut.Library.Folders[0] = sut.Library.Folders[0] with { SlideLayout = FlowFolderLayout(14, 80) };
        sut.RefreshLibraryFormatting();
        InStep("the folder format changed");
    }

    [Fact]
    public async Task BibleFlow_IndividualFormatSwitchedOff_IsNotUsedForTheScreens()
    {
        var plain = await FlowLiveAsync(FlowPassage());
        var off = await FlowLiveAsync(FlowPassage() with { FormatData = "46=20>47=30>", UseIndividualFormatting = false });
        var on = await FlowLiveAsync(FlowPassage() with { FormatData = "46=20>47=30>" });

        off.LyricsPageCount.Should().Be(plain.LyricsPageCount);
        off.Session.Current.CurrentItemBodyText.Should().Be(plain.Session.Current.CurrentItemBodyText);
        off.Session.Current.ItemDisplay.Should().BeNull();
        on.LyricsPageCount.Should().BeGreaterThan(plain.LyricsPageCount);
        on.Session.Current.ItemDisplay!.Top2.Should().Be(20);
    }

    [Fact]
    public async Task BibleFlow_GoLiveAtAPreviewPage_BlackHideAndClear_KeepThePage()
    {
        var sut = FlowSut();
        sut.LoadQueue([FlowPassage()]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        sut.NextLyricsPageCommand.Execute(null);
        sut.NextLyricsPageCommand.Execute(null);
        string card = sut.PreviewLyricsPages[2].BodyText;

        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(2);
        sut.OutputLyricsText.Should().Be(card);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        sut.Session.Current.State.Should().Be(LiveState.Active);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(3);
        sut.Session.Current.CurrentItemBodyText.Should().StartWith(sut.OutputLyricsPages[3].BodyText.Split('\n')[0]);
        sut.Session.Current.CurrentItemBodyText2.Should().StartWith("1:31 In the beginning");

        await sut.ToggleOutputClearCommand.ExecuteAsync(null);
        await sut.ToggleOutputClearCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(3);
        sut.Session.Current.CurrentItemBodyText2.Should().StartWith("1:31 In the beginning");

        await sut.HideOutputCommand.ExecuteAsync(null);
        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(4);
        sut.OutputLyricsText.Should().Be(sut.OutputLyricsPages[4].BodyText);
    }

    [Fact]
    public async Task BibleFlow_ListedVersions_NextAfterTheLastScreenOpensTheNextItem_AndPreviousComesBackToTheLastScreen()
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        var sut = await FlowLiveAsync(FlowPassage(), FlowSut(settings));
        // The list selection has moved on to the song (advance to next item); the output cards are the passage's.
        int pages = sut.OutputLyricsPages.Count;
        pages.Should().BeInRange(2, 59);
        sut.GoToLastOutputLyricsPageCommand.Execute(null);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(pages - 1);

        await sut.LiveNextShortcutCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemId.Should().Be("song:after");
        sut.Session.Current.CurrentItemBodyText2.Should().BeEmpty();
        await sut.LivePreviousShortcutCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemId.Should().Be(sut.Queue[0].Id);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(pages - 1);
        sut.Session.Current.CurrentItemBodyText.Should().EndWith(FlowKorean[59]);
        sut.Session.Current.CurrentItemBodyText2.Should().EndWith(FlowEnglish[59]);
    }

    [Fact]
    public async Task BibleFlow_ListSavedWithAnotherLayoutAndCrLf_LoadsWithTheLayoutOfTheVersion()
    {
        using var temp = TempSettingsFolder.Create();
        var store = new WorshipListStore(Path.Combine(temp.Root, "lists"));
        var passage = FlowPassage();
        await store.SaveAsync("Sunday", [passage with { SongLayout = FlowFolderLayout(30, 30), Lyrics = passage.Lyrics!.Replace("\n", "\r\n") }]);
        var reference = await FlowLiveAsync(FlowPassage());

        var sut = FlowSut(temp.CreateSettings(), store: store);
        await sut.LoadWorshipListAsync("Sunday");
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[0];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LyricsPageCount.Should().Be(reference.LyricsPageCount);
        sut.PreviewLyricsPages.Select(card => card.BodyText).Should().Equal(reference.PreviewLyricsPages.Select(card => card.BodyText));
        sut.Session.Current.CurrentItemBodyText.Should().Be(reference.Session.Current.CurrentItemBodyText);
        sut.Session.Current.CurrentItemBodyText2.Should().Be(reference.Session.Current.CurrentItemBodyText2);
    }

    [Fact]
    public async Task BibleFlow_SongsInTwoLanguagesAndWithStoredChords_AreNotFlowedAsAPassage()
    {
        var sut = FlowSut();
        var twoLanguages = new LiveQueueItem("song:2", "two", LiveItemKinds.Song) { Lyrics = "[1]\nfirst\n[region 2]\n첫째\n[2]\nsecond\n[region 2]\n둘째" };
        var chords = new LiveQueueItem("song:3", "chords", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n주의 친절한 팔에 안기세\n우리 맘이 평안하리니", Notations = "(1;G;0;Am;6;)(2;G;0;D;9;)",
        };
        sut.LoadQueue([twoLanguages, chords]);
        sut.OpenOutputCommand.Execute(null);

        sut.SelectedItem = sut.Queue[0];
        sut.PreviewLyricsPages.Select(card => card.BodyText).Should().Equal(
            $"first{Environment.NewLine}{Environment.NewLine}첫째", $"second{Environment.NewLine}{Environment.NewLine}둘째");
        await sut.GoLiveCommand.ExecuteAsync(null);
        (sut.Session.Current.CurrentItemBodyText, sut.Session.Current.CurrentItemBodyText2).Should().Be(("first", "첫째"));

        sut.SelectedItem = sut.Queue[1];
        sut.PreviewLyricsPages.Should().ContainSingle().Which.BodyText.Should().Be("주의 친절한 팔에 안기세\n우리 맘이 평안하리니");
        FlowVisibleTexts(sut).Should().NotContain(text => text.Contains(FlowMarker));
    }
}
