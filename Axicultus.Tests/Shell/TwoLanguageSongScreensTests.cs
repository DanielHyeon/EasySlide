using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// A two-language song is cut into screens as in the WinForms app: region 1 alone is measured, in the room above
/// region 2 (gfLyrics.BuildSlides, GetScreensRequired with FS_Height_R2Bound), and each of its screens takes as
/// many lines of region 2 as it has of region 1, the last one the rest (BuildSlidesReg2).
/// </summary>
public class TwoLanguageSongScreensTests
{
    private const double ScreenHeight = 720;
    private const int Panel = 6;

    private static readonly SongPageLayout Layout = new(12, "Arial", 0, 5, 5, 0, 1, 100, 0);

    private static string[] English(int count, int from = 1) => [.. Enumerable.Range(from, count).Select(line => $"line {line}")];
    private static string[] Korean(int count, int from = 1) => [.. Enumerable.Range(from, count).Select(line => $"줄 {line}")];

    private static string Song(string[] region1, string[] region2, string heading = "[1]")
        => $"{heading}\n{string.Join("\n", region1)}\n[region 2]\n{string.Join("\n", region2)}";

    private static List<(string[] Region1, string[] Region2)> Screens(string lyrics, string? sequence = null, int region2Top = 50,
        List<double>? scales = null)
        => Layout.Paginate(lyrics, sequence, null, ScreenHeight, Panel, null, scales, region2Top).Bodies
            .ConvertAll(SongPageLayout.SplitRegions)
            .ConvertAll(page => (page.Region1.Length > 0 ? page.Region1.Split('\n') : [], page.Region2.Length > 0 ? page.Region2.Split('\n') : []));

    // The lines region 1 has room for above region 2.
    private static int Room(int region2Top = 50) => Screens(Song(English(300), Korean(300)), region2Top: region2Top)[0].Region1.Length;

    [Fact]
    public void AVerseThatFitsIsOneScreenWithBothRegions()
    {
        var pages = Layout.Paginate(Song(English(3), Korean(3)), null, null, ScreenHeight, Panel, null, null, 50);

        pages.Bodies.Should().Equal("line 1\nline 2\nline 3" + SongPageLayout.Region2Break + "줄 1\n줄 2\n줄 3");
        pages.Labels.Should().Equal(LyricsDisplayFormatter.GetRegionSectionLabels(Song(English(3), Korean(3)), null));
    }

    [Fact]
    public void RegionOneIsCutInTheRoomAboveRegionTwo_AndRegionTwoTakesAsManyLines()
    {
        int room = Room();
        room.Should().BeGreaterThan(2);

        var screens = Screens(Song(English(room * 2 + 1), Korean(room * 2 + 1)));

        screens.Select(screen => screen.Region1.Length).Should().Equal(room, room, 1);
        screens.Select(screen => screen.Region2.Length).Should().Equal(room, room, 1);
        screens.SelectMany(screen => screen.Region1).Should().Equal(English(room * 2 + 1), "no line is lost or repeated");
        screens.SelectMany(screen => screen.Region2).Should().Equal(Korean(room * 2 + 1));
    }

    [Fact]
    public void TheLastScreenTakesTheRestOfRegionTwo()
    {
        // BuildSlidesReg2: i == region1ScreenCount takes everything up to the end of the region 2 screen.
        int room = Room();

        var screens = Screens(Song(English(room * 2), Korean(room * 2 + 5)));

        screens.Select(screen => screen.Region1.Length).Should().Equal(room, room);
        screens.Select(screen => screen.Region2.Length).Should().Equal(room, room + 5);
        screens[1].Region2.Should().Equal(Korean(room + 5, from: room + 1));
    }

    [Fact]
    public void AScreenAfterRegionTwoHasRunOutShowsRegionOneAlone()
    {
        // BuildSlidesReg2: fewer lines left than region 1 has on the screen: it takes them, the next screens none.
        int room = Room();

        var pages = Layout.Paginate(Song(English(room * 3), Korean(room + 2)), null, null, ScreenHeight, Panel, null, null, 50).Bodies;

        pages.Should().HaveCount(3);
        pages.ConvertAll(SongPageLayout.SplitRegions).Select(page => page.Region2.Length > 0 ? page.Region2.Split('\n').Length : 0)
            .Should().Equal(room, 2, 0);
        pages[2].Should().Be(string.Join("\n", English(room, from: room * 2 + 1)), "a page without region 2 is plain text");
    }

    [Fact]
    public void RegionTwoIsNotMeasured()
    {
        // gfLyrics.BuildSlides measures Lyrics[0] only; region 2 is drawn as large as fits its room.
        int room = Room();
        string longLine = string.Join(" ", Enumerable.Repeat("놀라운 은혜 나 같은 죄인 살리신", 40));

        var screens = Screens(Song(English(room * 2), [.. Enumerable.Repeat(longLine, room * 2)]));

        screens.Select(screen => screen.Region1.Length).Should().Equal(room, room);
        screens.Select(screen => screen.Region2.Length).Should().Equal(room, room);
    }

    [Fact]
    public void RegionOneEndsWhereRegionTwoStarts()
    {
        // gfLyrics.FormatText: FS_Height_R2Bound = top of region 2 - top of region 1 (with the heading).
        int single = Layout.Paginate("[1]\n" + string.Join("\n", English(300)), null, null, ScreenHeight, Panel).Bodies[0].Split('\n').Length;

        Room(region2Top: 25).Should().BeLessThan(Room(region2Top: 50));
        Room(region2Top: 50).Should().BeLessThan(single, "a song in one language has the whole body height");
    }

    [Fact]
    public void ScreensTheLyricsSeparateArePairedByNumber_AndEachIsCutByItself()
    {
        // An empty line is a screen break in either region; region 2 screens left over follow alone and whole.
        int room = Room();
        string lyrics = $"[1]\n{string.Join("\n", English(room + 1))}\n\nlast a\nlast b\n[region 2]\n줄 1\n\n줄 2\n줄 3\n\n{string.Join("\n", Korean(room * 3, from: 10))}";
        var scales = new List<double>();

        var screens = Screens(lyrics, scales: scales);

        screens.Select(screen => (screen.Region1.Length, screen.Region2.Length)).Should().Equal(
            (room, 1),       // the first region 2 screen has one line only
            (1, 0),          // nothing left of it for the second cut
            (2, 2),          // the second screens of both regions
            (0, room * 3));  // the region 2 screen left over, not cut
        screens[2].Region1.Should().Equal("last a", "last b");
        screens[2].Region2.Should().Equal("줄 2", "줄 3");
        scales.Should().Equal(1, 1, 1, 1);
    }

    [Fact]
    public void AVerseWithRegionTwoOnly_IsCutByRegionTwoInItsOwnRoom()
    {
        // gfLyrics.BuildSlides: a verse without region 1 is cut with Lyrics[1] (GetScreensRequired), whose room is
        // the body below the top of region 2 (FS_Height = lyricsAreaHeight - FS_Top), at region 2's own font.
        string lyrics = $"[1]\nline 1\n[region 2]\n줄 1\n[2]\n[region 2]\n{string.Join("\n", Korean(300))}";
        int Lines(SongPageLayout layout, int top) => SongPageLayout.SplitRegions(
            layout.Paginate(lyrics, null, null, ScreenHeight, Panel, null, null, top).Bodies[1]).Region2.Split('\n').Length;

        var pages = Layout.Paginate(lyrics, null, null, ScreenHeight, Panel, null, null, 50);

        pages.Bodies[0].Should().Be("line 1" + SongPageLayout.Region2Break + "줄 1");
        pages.Bodies.Count.Should().BeGreaterThan(3, "the second verse does not fit one screen");
        pages.Bodies.Skip(1).Should().OnlyContain(page => page.StartsWith(SongPageLayout.Region2Break), "these screens have no region 1");
        pages.Bodies.Skip(1).SelectMany(page => SongPageLayout.SplitRegions(page).Region2.Split('\n')).Should().Equal(Korean(300));
        pages.Labels.Skip(1).Distinct().Should().ContainSingle().Which.Should().NotBe(pages.Labels[0]);

        Lines(Layout, 25).Should().BeGreaterThan(Lines(Layout, 50), "a higher top leaves region 2 more room");
        var larger = Layout with { Fields = new Dictionary<string, string> { ["Size1"] = "24", ["FontName1"] = "Arial" } };
        Lines(larger, 50).Should().BeLessThan(Lines(Layout, 50), "region 2 is measured with its own font size");
    }

    [Fact]
    public void EveryScreenOfAVerseCarriesItsLabel_InTheOrderOfTheSequence()
    {
        int room = Room();
        string lyrics = Song(English(room + 1), Korean(room + 1)) + "\n" + Song(["refrain"], ["후렴"], "[chorus]");
        var labels = LyricsDisplayFormatter.GetRegionSectionLabels(lyrics, "1,c,1");
        labels.Should().HaveCount(3);

        var pages = Layout.Paginate(lyrics, "1,c,1", null, ScreenHeight, Panel, null, null, 50);

        pages.Labels.Should().Equal(labels[0], labels[0], labels[1], labels[2], labels[2]);
        pages.Bodies[2].Should().Be("refrain" + SongPageLayout.Region2Break + "후렴");
        pages.Bodies[4].Should().Be(pages.Bodies[1]);
    }

    [Fact]
    public void AFirstLineHigherThanTheRoom_IsCutWithAReducedFont()
    {
        // gfLyrics.GetOneScreen: ReduceFontToFit on the first line of a screen, against the room above region 2.
        string longLine = string.Join(" ", Enumerable.Repeat("Amazing grace how sweet the sound", 120));
        var scales = new List<double>();

        var screens = Screens(Song(["short", longLine, "after"], Korean(3)), scales: scales);

        screens.Select(screen => screen.Region1[0]).Should().Equal("short", longLine, "after");
        scales.Select(scale => scale < 1).Should().Equal(false, true, false);
        scales[1].Should().BeInRange(0.05, 0.95);
        screens.SelectMany(screen => screen.Region2).Should().Equal(Korean(3));
    }

    [Fact]
    public void ChordsStayOnTheirLines()
    {
        var pages = Layout.Paginate("[1]\nAmazing grace » G   D\n[region 2]\n놀라운 은혜 » G", null, null, ScreenHeight, Panel, null, null, 50).Bodies;

        var (region1, region2) = SongPageLayout.SplitRegions(pages.Should().ContainSingle().Subject);
        LyricsDisplayFormatter.FormatSongSlide(region1, false, 0).Should().Be("Amazing grace");
        LyricsDisplayFormatter.FormatSongSlide(region2, false, 0).Should().Be("놀라운 은혜");
        LyricsDisplayFormatter.FormatSongSlide(region1, true, 0).Should().Contain("G").And.EndWith("Amazing grace");
    }

    [Fact]
    public void WithoutASecondRegionThePagesAreThoseOfBefore()
    {
        string lyrics = "[1]\n" + string.Join("\n", English(200)) + "\n[chorus]\nrefrain";

        var pages = Layout.Paginate(lyrics, null, null, ScreenHeight, Panel);

        pages.Bodies.Should().OnlyContain(page => !page.Contains(SongPageLayout.Region2Break));
        string.Join("\n", pages.Bodies).Should().Be(string.Join("\n", English(200)) + "\nrefrain");
    }
}

// The view-model flow of a two-language song that needs more than one screen.
public partial class MainViewModelTests
{
    private static readonly string[] TwoLanguageEnglish = [.. Enumerable.Range(1, 40).Select(line => $"line {line}")];
    private static readonly string[] TwoLanguageKorean = [.. Enumerable.Range(1, 40).Select(line => $"줄 {line}")];

    private static SongPageLayout TwoLanguageLayout() => new(30, "Arial", 0, 5, 5, 0, 1, 100, 0)
    {
        Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Vpos1"] = "50", ["Size1"] = "30", ["FontName1"] = "Arial" },
    };

    private static SongSummary TwoLanguageSong() => new(4001, "Amazing Grace", "", 1, 0, "", "",
        $"[1]\n{string.Join("\n", TwoLanguageEnglish)}\n[region 2]\n{string.Join("\n", TwoLanguageKorean)}\n[chorus]\nrefrain\n[region 2]\n후렴");

    [Fact]
    public async Task TwoLanguageSong_IsCutIntoScreensByRegionOne_OnTheCardsAndTheOutput()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        settings.Set(AxicultusSettingKeys.AutoTextOverflow, true);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        var layout = TwoLanguageLayout();
        sut.Library.Folders.Add(new SongFolderSummary(1, "Folder 1", false, 0, SlideLayout: layout));
        var song = TwoLanguageSong();
        var expected = layout.Paginate(song.Lyrics, null, null, sut.SampleScreenHeight, settings.Get(AxicultusSettingKeys.PanelHeightPercent), null, null, 50);
        expected.Bodies.Should().HaveCountGreaterThan(3, "the verse needs several screens at this font");
        var regions = expected.Bodies.ConvertAll(SongPageLayout.SplitRegions);

        sut.AddSong(song);

        sut.LyricsPageCount.Should().Be(expected.Bodies.Count);
        sut.PreviewLyricsPages.Select(card => card.BodyText).Should().Equal(
            regions.Select(page => page.Region1 + Environment.NewLine + Environment.NewLine + page.Region2));
        regions.SelectMany(page => page.Region1.Split('\n')).Should().Equal([.. TwoLanguageEnglish, "refrain"]);
        regions.SelectMany(page => page.Region2.Split('\n')).Should().Equal([.. TwoLanguageKorean, "후렴"]);

        sut.GoToPreviewLyricsPageCommand.Execute(1);
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemBodyText.Should().Be(regions[1].Region1);
        sut.Session.Current.CurrentItemBodyText2.Should().Be(regions[1].Region2);
        sut.Session.Current.CurrentSectionLabel.Should().Be(expected.Labels[1]);

        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemBodyText.Should().Be(regions[2].Region1);
        sut.Session.Current.CurrentItemBodyText2.Should().Be(regions[2].Region2);
    }

    [Fact]
    public void TwoLanguageSong_WithAutoTextOverflowOff_KeepsAVerseOnOneScreen()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AutoTextOverflow, false);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        sut.Library.Folders.Add(new SongFolderSummary(1, "Folder 1", false, 0, SlideLayout: TwoLanguageLayout()));

        sut.AddSong(TwoLanguageSong());

        sut.LyricsPageCount.Should().Be(2, "the verse and the chorus");
    }
}
