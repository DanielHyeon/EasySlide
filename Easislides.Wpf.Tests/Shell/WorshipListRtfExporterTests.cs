using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class WorshipListRtfExporterTests
{
    [Fact]
    public void BuildRtf_IncludesQueueOrderTitleAndLyrics()
    {
        var sut = new WorshipListRtfExporter();
        var items = new[]
        {
            new LiveQueueItem("song:1", "Opening", LiveItemKinds.Song)
            {
                Lyrics = "[1]\nPraise together » C G\nAmen",
            },
            new LiveQueueItem("bible:1", "Psalm 23", LiveItemKinds.Bible)
            {
                Lyrics = "The Lord is my shepherd",
            },
        };

        var rtf = sut.BuildRtf("Sunday", items);

        rtf.Should().StartWith(@"{\rtf1");
        rtf.Should().Contain("Sunday");
        rtf.Should().Contain("1. Opening");
        rtf.Should().Contain("2. Psalm 23");
        rtf.Should().Contain("Praise together");
        rtf.Should().NotContain("C G", "notation suffixes after the legacy music marker should not pollute the lyrics document");
        rtf.Should().Contain("The Lord is my shepherd");
        rtf.Should().EndWith("}");
    }

    [Fact]
    public void BuildRtf_FallsBackToContentPathForExternalItems()
    {
        var sut = new WorshipListRtfExporter();
        var items = new[]
        {
            new LiveQueueItem("ppt:C:\\slides\\song.pptx", "Slides", LiveItemKinds.PowerPoint)
            {
                ContentPath = @"C:\slides\song.pptx",
            },
        };

        var rtf = sut.BuildRtf("Order", items);

        rtf.Should().Contain("Slides");
        rtf.Should().Contain(@"C:\\slides\\song.pptx", "RTF backslashes must be escaped");
    }

    [Fact]
    public void BuildRtf_EscapesNonAsciiAndRtfSpecialCharacters()
    {
        var sut = new WorshipListRtfExporter();
        var items = new[]
        {
            new LiveQueueItem("song:1", @"가사 \ { }", LiveItemKinds.Song)
            {
                Lyrics = "아멘",
            },
        };

        var rtf = sut.BuildRtf("예배", items);

        rtf.Should().Contain(@"\u", "Korean text should be emitted as RTF unicode escapes");
        rtf.Should().Contain(@"\\");
        rtf.Should().Contain(@"\{");
        rtf.Should().Contain(@"\}");
        rtf.Should().NotContain("예배");
        rtf.Should().NotContain("아멘");
    }

    [Fact]
    public void BuildRtf_BlankTitle_UsesDefaultWorshipListTitle()
    {
        var sut = new WorshipListRtfExporter();

        var rtf = sut.BuildRtf("   ", []);

        rtf.Should().Contain("Worship List");
    }
}
