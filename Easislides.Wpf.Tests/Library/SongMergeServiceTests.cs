using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class SongMergeServiceTests
{
    [Fact]
    public void Merge_InterleavesMatchingSectionsAndAppendsRemainingSourceBSections()
    {
        var sut = new SongMergeService();
        var sourceA = Detail(10, "Alpha", lyrics: "[1]\nA verse\n[2]\nA second", sequence: "12");
        var sourceB = Detail(20, "Alpha", lyrics: "[1]\nB verse\n[3]\nB third");

        var result = sut.Merge(sourceA, sourceB);

        result.Lyrics.Should().Be("[1]\nA verse\n[region 2]\nB verse\n[2]\nA second\n[3]\n[region 2]\nB third");
        result.Notations.Should().BeEmpty();
    }

    [Fact]
    public void Merge_WhenLyricsHaveNoSectionHeadings_AppendsSourceBAsRegionTwo()
    {
        var sut = new SongMergeService();
        var sourceA = Detail(10, "Alpha", lyrics: "A line 1\nA line 2");
        var sourceB = Detail(20, "Alpha", lyrics: "B line 1");

        var result = sut.Merge(sourceA, sourceB);

        result.Lyrics.Should().Be("A line 1\nA line 2\n[region 2]\nB line 1");
    }

    [Fact]
    public void Merge_RemapsSourceNotationLineIndexesToMergedOutput()
    {
        var sut = new SongMergeService();
        var sourceA = Detail(
            10,
            "Alpha",
            lyrics: "[1]\nA verse\n[2]\nA second",
            notations: "(1;A-note)");
        var sourceB = Detail(
            20,
            "Alpha",
            lyrics: "[1]\nB verse\n[3]\nB third",
            notations: "(1;B-note)(3;B3-note)");

        var result = sut.Merge(sourceA, sourceB);

        result.Notations.Should().Be("(1;A-note)(3;B-note)(8;B3-note)");
    }

    private static SongDetail Detail(
        int id,
        string title,
        string lyrics,
        string sequence = "",
        string notations = "")
        => new(
            id,
            title,
            AlternateTitle: "",
            FolderNo: 1,
            SongNumber: 1,
            lyrics,
            sequence,
            Writer: "",
            Copyright: "",
            Capo: 0,
            Timing: "",
            Key: "",
            notations,
            Category: "",
            LicenceAdmin1: "",
            LicenceAdmin2: "",
            BookReference: "",
            UserReference: "",
            Settings: "",
            FormatData: "");
}
