using System.Linq;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// Reviewer tests: each states what WinForms (gfLyrics) shows, for a defect the comparison over real songs brought out.
public class RevFormatterDefectTests
{
    [Fact]
    public void WhitespaceOnlyLine_IsALineOfTheScreen_NotAScreenBreak()
    {
        // gfLyrics.GetVerseScreenCount breaks only at Text == "" (song 4348 "교독문 57번": "회중: ...\n \n사회: ...").
        var pages = LyricsDisplayFormatter.ToVersePages("회중: 이스라엘을 다스릴 자가\n \n사회: 한 아기가 우리에게 났고");
        pages.Should().HaveCount(1);
    }

    [Fact]
    public void TextBeforeTheFirstHeading_AndVerse1_AreOneVerse_WithAStoredSequence()
    {
        // ExtractLyricRegions: verseNumber starts at 1, so the lines before [1] and the lines under [1] are verse 1.
        var pages = LyricsDisplayFormatter.ToVersePages("Title line\n[1]\nverse one\n[chorus]\nrefrain", "1,c");
        string.Join("|", pages).Should().Contain("verse one");
    }

    [Fact]
    public void TextBeforeTheFirstHeading_AndVerse1_AreOneScreen_WithoutASequence()
    {
        // Songs 1532, 2044, 2369 ... (13 real songs): WinForms 2 slides, here 3.
        var pages = LyricsDisplayFormatter.ToVersePages("작사 작곡\n[1]\nverse one\n[2]\nverse two");
        pages.Should().HaveCount(2);
    }

    [Fact]
    public void SecondBlockOfTheSameHeading_CarriesRegion2OfThatVerse()
    {
        // Song 10144: [chorus] region 1 ... [chorus] [region 2] ... ; FormatDisplayLyrics groups rows by verse number.
        const string lyrics = "[chorus]\nJP 1\n\nJP 2\n[bridge]\nJP b\n[region 2]\nKR b\n[chorus]\n[region 2]\nKR 1\n\nKR 2";
        var pages = LyricsDisplayFormatter.GetRegionPages(lyrics);
        pages.Select(p => (p.Region1, p.Region2)).Should().Equal(("JP 1", "KR 1"), ("JP 2", "KR 2"), ("JP b", "KR b"));
    }

    [Fact]
    public void SecondRegion2MarkerInAVerse_KeepsRegion2()
    {
        // Song 3171 verse 2: region 2 runs to the next heading whatever the number of markers.
        const string lyrics = "[1]\nR1 a\n\nR1 b\n[region 2]\nR2 a\n\nR2 b\n[region 2]\nR2 c\n\nR2 d";
        var pages = LyricsDisplayFormatter.GetRegionPages(lyrics);
        pages.Should().NotContain(p => p.Region1.Contains("R2"), "region 2 text must not be shown as region 1");
    }

    [Fact]
    public void NewRegion2Format_AllOfRegion2AfterOneMarker_IsPairedByVerse()
    {
        // gfLyrics.IsNewR2Format / ExtractLyricRegions(separateRegions: true).
        const string lyrics = "[1]\nEN 1\n[2]\nEN 2\n[region 2]\n[1]\nKO 1\n[2]\nKO 2";
        LyricsDisplayFormatter.HasRegion2(lyrics).Should().BeTrue();
        LyricsDisplayFormatter.GetRegionPages(lyrics, "1,2").Select(p => (p.Region1, p.Region2))
            .Should().Equal(("EN 1", "KO 1"), ("EN 2", "KO 2"));
    }
}
