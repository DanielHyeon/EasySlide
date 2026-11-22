using Axicultus.Wpf.Library;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class BiblePassageComposerTests
{
    [Fact]
    public void Compose_SingleLanguage_IsOneBlockWithAVerseOnEachLine()
    {
        // The WinForms app sends a passage as one text that flows over the slides (gfLyrics.FitWholeLyricPage
        // exempts type "B"): no verse ends a page, the page layout does. An empty verse must not break the block.
        var body = BiblePassageComposer.Compose(
            ["1:1 In the beginning", "", "1:2 And the earth"],
            ["", ""]);

        body.Should().Be("1:1 In the beginning\n1:2 And the earth");
        body.Should().NotContain("[region 2]");
        LyricsDisplayFormatter.ToVersePages(body).Should().ContainSingle();
        LyricsDisplayFormatter.HasRegion2(body).Should().BeFalse();
    }

    [Fact]
    public void Compose_DualLanguage_PairsRegionsPerVerse()
    {
        var body = BiblePassageComposer.Compose(
            ["3:16 하나님이 세상을", "3:17 하나님이 아들을"],
            ["For God so loved", "For God sent not"]);

        // 첫 페이지: Region1=주 언어, Region2=보조 언어.
        var page0 = LyricsDisplayFormatter.GetRegionPage(body, 0);
        page0.Region1.Should().Be("3:16 하나님이 세상을");
        page0.Region2.Should().Be("For God so loved");
        var page1 = LyricsDisplayFormatter.GetRegionPage(body, 1);
        page1.Region1.Should().Be("3:17 하나님이 아들을");
        page1.Region2.Should().Be("For God sent not");
        LyricsDisplayFormatter.HasRegion2(body).Should().BeTrue();
        LyricsDisplayFormatter.GetRegionPages(body, null).Should().HaveCount(2);
    }

    [Fact]
    public void Compose_UnevenRegions_PadsShorterSide()
    {
        // 보조 버전에 한 절이 없을 때 — 더 긴 쪽까지 만들고 모자란 절은 빈 보조 본문(절 어긋남 방어).
        var body = BiblePassageComposer.Compose(
            ["v1", "v2"],
            ["b1"]);

        LyricsDisplayFormatter.GetRegionPages(body, null).Should().HaveCount(2);
        LyricsDisplayFormatter.GetRegionPage(body, 1).Region1.Should().Be("v2");
        LyricsDisplayFormatter.GetRegionPage(body, 1).Region2.Should().BeEmpty();
        // Every verse carries the marker; with one marker only the passage would read like a WinForms song whose
        // region 2 runs on to the end, and "v2" would land in region 2.
        body.Should().Be("v1\n[region 2]\nb1\n\nv2\n[region 2]\n");
    }

    [Fact]
    public void RegionTwoOfAHeadinglessSongRunsToTheEnd_AsInTheWinFormsApp()
    {
        // gfLyrics.BuildSlidesReg2: region 2 lasts until the next heading and the screens of the two regions
        // pair by position. Only the composer's passages (a marker for every verse) page at every blank line.
        var pages = LyricsDisplayFormatter.GetRegionPages("a1\n\na2\n[region 2]\nb1\n\nb2");

        pages.Should().Equal(new LyricsRegionPage("a1", "b1"), new LyricsRegionPage("a2", "b2"));
    }

    [Fact]
    public void Compose_Empty_ReturnsEmpty()
        => BiblePassageComposer.Compose([], []).Should().BeEmpty();
}
