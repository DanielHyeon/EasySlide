using Easislides.Wpf.Library;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class BiblePassageComposerTests
{
    [Fact]
    public void Compose_SingleLanguage_SeparatesVersesByBlankLine()
    {
        // 보조 언어가 없으면 [region 2] 마커 없이 절만 빈 줄로 나눠 단일 밴드로 렌더된다.
        var body = BiblePassageComposer.Compose(
            ["1:1 In the beginning", "1:2 And the earth"],
            []);

        body.Should().Be("1:1 In the beginning\n\n1:2 And the earth");
        body.Should().NotContain("[region 2]");
        // 절 페이지네이션과 1:1 — 두 절 = 두 페이지.
        LyricsDisplayFormatter.ToVersePages(body).Should().HaveCount(2);
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
    }

    [Fact]
    public void Compose_Empty_ReturnsEmpty()
        => BiblePassageComposer.Compose([], []).Should().BeEmpty();
}
