using System.Collections.Generic;
using System.Linq;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// A Bible passage in two versions flows over the screens as in the WinForms app: region 1 alone decides the
/// screens (gfLyrics.BuildSlides, GetScreensRequired with the height above region 2) and region 2 takes the same
/// verses (BuildSlidesReg2 hands each screen as many lines of region 2 as region 1 has on it).
/// </summary>
public class BibleTwoVersionFlowTests
{
    private const double ScreenHeight = 720;
    private const int Panel = 6;

    private static readonly SongPageLayout Layout = new(12, "Arial", 0, 5, 5, 0, 1, 100, 0);

    // One line each at the layout's font and width.
    private static readonly string[] Korean =
    [
        "태초에 하나님이 천지를 창조하시니라",
        "땅이 혼돈하고 공허하며 흑암이 깊음 위에 있고",
        "하나님이 이르시되 빛이 있으라 하시니 빛이 있었고",
        "빛이 하나님이 보시기에 좋았더라",
        "하나님이 빛을 낮이라 부르시고 어둠을 밤이라 부르시니라",
    ];

    private static readonly string[] English =
    [
        "In the beginning God created the heaven and the earth.",
        "And the earth was without form, and void; and darkness was upon the face of the deep.",
        "And God said, Let there be light: and there was light.",
        "And God saw the light, that it was good: and God divided the light from the darkness.",
        "And God called the light Day, and the darkness he called Night.",
    ];

    private static LyricsRegionPage[] Passage(int verses, string? longEnglish = null)
        => [.. Enumerable.Range(1, verses).Select(verse => new LyricsRegionPage(
            $"1:{verse} {Korean[verse % Korean.Length]}", $"1:{verse} {longEnglish ?? English[verse % English.Length]}"))];

    private static List<(string[] Region1, string[] Region2)> Screens(IReadOnlyList<LyricsRegionPage> verses, int region2Top = 50,
        double height = ScreenHeight)
        => Layout.PaginateRegions(verses, null, height, Panel, null, region2Top)
            .ConvertAll(page => SongPageLayout.SplitRegions(page))
            .ConvertAll(page => (page.Region1.Split('\n'), page.Region2.Split('\n')));

    // The verses region 1 has room for above region 2 at the given top of region 2.
    private static int VersesOnAScreen(int region2Top = 50) => Screens(Passage(200), region2Top)[0].Region1.Length;

    private static string Reference(string verse) => verse[..verse.IndexOf(' ')];

    [Fact]
    public void APassageThatFitsIsOneScreenWithBothVersions()
    {
        var verses = Passage(3);

        var screens = Screens(verses);

        screens.Should().ContainSingle();
        screens[0].Region1.Should().Equal(verses.Select(verse => verse.Region1));
        screens[0].Region2.Should().Equal(verses.Select(verse => verse.Region2));
    }

    [Fact]
    public void RegionOneFillsEachScreen_AndRegionTwoTakesTheSameVerses()
    {
        // gfLyrics.GetOneScreen adds lines while they fit; BuildSlidesReg2 gives the screen that many lines of region 2.
        int room = VersesOnAScreen();
        room.Should().BeGreaterThan(1);
        var verses = Passage(room * 2 + 1);

        var screens = Screens(verses);

        screens.Select(screen => screen.Region1.Length).Should().Equal(room, room, 1);
        screens.SelectMany(screen => screen.Region1).Should().Equal(verses.Select(verse => verse.Region1), "no verse is lost or repeated");
        screens.SelectMany(screen => screen.Region2).Should().Equal(verses.Select(verse => verse.Region2));
        foreach (var screen in screens)
            screen.Region2.Select(Reference).Should().Equal(screen.Region1.Select(Reference), "a screen shows the same verses in both versions");
    }

    [Fact]
    public void RegionTwoIsNotMeasured_ALongSecondVersionDoesNotChangeTheScreens()
    {
        // gfLyrics.BuildSlides measures Lyrics[0] only (GetScreensRequired); region 2 follows by line count.
        int room = VersesOnAScreen();
        string longEnglish = string.Join(" ", Enumerable.Repeat(English[1], 12));

        var screens = Screens(Passage(room * 2, longEnglish));

        screens.Select(screen => screen.Region1.Length).Should().Equal(room, room);
        screens[1].Region2.Should().HaveCount(room).And.OnlyContain(verse => verse.EndsWith(longEnglish));
    }

    [Fact]
    public void RegionOneEndsWhereRegionTwoStarts()
    {
        // gfLyrics.FormatText: FS_Height_R2Bound = top of region 2 - top of region 1 (with the heading).
        int half = VersesOnAScreen(region2Top: 50);
        int quarter = VersesOnAScreen(region2Top: 25);
        int single = Layout.Paginate(string.Join("\n", Passage(200).Select(verse => verse.Region1)), null, null, ScreenHeight, Panel)
            .Bodies[0].Split('\n').Length;

        quarter.Should().BeLessThan(half);
        half.Should().BeLessThan(single, "a passage in one version has the whole body height");
    }

    [Fact]
    public void AVerseHigherThanAScreen_MarksItsScreenAsCutWithAReducedFont()
    {
        // gfLyrics.GetOneScreen: ReduceFontToFit on the first line of a screen; that screen is drawn with the reduced font.
        var verses = Passage(5);
        verses[2] = verses[2] with { Region1 = "1:3 " + string.Join(" ", Enumerable.Repeat(Korean[1], 60)) };
        var scales = new List<double>();

        Layout.PaginateRegions(verses, null, ScreenHeight, Panel, null, 50, scales).Should().HaveCount(3);
        scales.Select(scale => scale < 1).Should().Equal(false, true, false);
        scales[1].Should().BeInRange(0.05, 0.95, "the font is reduced until the verse fits the room above region 2");

        // One version: the whole body height is the room, so the verse has to be longer to be too high.
        var lines = verses.Select(verse => verse.Region1).ToArray();
        lines[2] = "1:3 " + string.Join(" ", Enumerable.Repeat(Korean[1], 200));
        var single = new List<double>();
        var bodies = Layout.Paginate(string.Join("\n", lines), null, null, ScreenHeight, Panel, null, single).Bodies;
        single.Should().HaveSameCount(bodies);
        single.Count(scale => scale < 1).Should().Be(1);
        bodies[single.FindIndex(scale => scale < 1)].Should().StartWith("1:3 ");
        string.Join("\n", bodies).Should().Be(string.Join("\n", lines), "no line is lost or moved");

        // Nothing is marked when every verse fits.
        var none = new List<double>();
        Layout.PaginateRegions(Passage(40), null, ScreenHeight, Panel, null, 50, none);
        none.Should().NotBeEmpty().And.OnlyContain(scale => scale == 1);
    }

    [Fact]
    public void AVerseThatFillsAScreenAloneIsAScreenOfItsOwn_WithItsSecondVersion()
    {
        // gfLyrics.GetOneScreen: endline = startline when the first line alone is too high. Nothing is cut off.
        var verses = Passage(5);
        string longVerse = "1:3 " + string.Join(" ", Enumerable.Repeat(Korean[1], 60));
        verses[2] = verses[2] with { Region1 = longVerse };

        var screens = Screens(verses);

        screens.Should().HaveCount(3);
        screens[0].Region1.Should().Equal(verses[0].Region1, verses[1].Region1);
        screens[1].Region1.Should().Equal(longVerse);
        screens[1].Region2.Should().Equal(verses[2].Region2);
        screens[2].Region1.Should().Equal(verses[3].Region1, verses[4].Region1);
        screens[2].Region2.Should().Equal(verses[3].Region2, verses[4].Region2);
    }

    [Fact]
    public void AVerseOneVersionLacks_KeepsTheOtherVersesTogether()
    {
        // The saved passage pairs the versions verse by verse (BiblePassageComposer), so a verse missing in one
        // version does not shift the other verses as a plain line count would.
        var verses = Passage(3);
        verses[1] = verses[1] with { Region2 = "" };

        var screens = Screens(verses);

        screens.Should().ContainSingle();
        screens[0].Region1.Should().Equal(verses.Select(verse => verse.Region1));
        screens[0].Region2.Should().Equal(verses[0].Region2, verses[2].Region2);
    }

    [Fact]
    public void ThePagesReadBackAsTheTwoRegions()
    {
        var verses = Passage(2);
        var page = Layout.PaginateRegions(verses, null, ScreenHeight, Panel, null, 50).Should().ContainSingle().Subject;

        page.Should().Be($"{verses[0].Region1}\n{verses[1].Region1}{SongPageLayout.Region2Break}{verses[0].Region2}\n{verses[1].Region2}");
        SongPageLayout.SplitRegions("one version only").Should().Be(("one version only", ""));
        Layout.PaginateRegions([], null, ScreenHeight, Panel, null, 50).Should().BeEmpty();
    }

    [Fact]
    public void TheSavedPassageGivesTheVersePairsTheScreensAreBuiltFrom()
    {
        // BiblePassageComposer writes a verse of both versions in each block; the screens are built from those pairs.
        var body = BiblePassageComposer.Compose([.. Passage(4).Select(verse => verse.Region1)], [.. Passage(4).Select(verse => verse.Region2)]);

        LyricsDisplayFormatter.GetRegionPages(body, null).Should().Equal(Passage(4));
    }
}
