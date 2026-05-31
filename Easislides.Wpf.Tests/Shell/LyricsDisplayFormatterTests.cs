using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class LyricsDisplayFormatterTests
{
    [Fact]
    public void ToDisplayText_StripsVerseMarkersAndSeparatesVerses()
    {
        // [1] [2] 같은 절/페이지 마커는 회중 화면에 보이면 안 되고, 절 사이는 빈 줄 하나로 구분.
        var raw = "[1]\n첫째 줄\n둘째 줄\n[2]\n셋째 줄";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("첫째 줄\n둘째 줄\n\n셋째 줄");
        display.Should().NotContain("[1]").And.NotContain("[2]");
    }

    [Fact]
    public void ToDisplayText_StripsLeadingNotationBlock()
    {
        // 맨 앞 코드(노테이션) 블록 [~...] 은 제거된다.
        var raw = "[~G D Em C]\n[1]\nAmazing grace";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("Amazing grace");
        display.Should().NotContain("[~").And.NotContain("G D Em C");
    }

    [Fact]
    public void ToDisplayText_NormalizesCrlfAndCollapsesBlankRuns()
    {
        // \r\n 정규화 + 연속 빈 줄은 하나로 축약, 앞뒤 경계는 제거.
        var raw = "\r\n[1]\r\n첫째\r\n\r\n\r\n둘째\r\n";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("첫째\n\n둘째");
    }

    [Fact]
    public void ToDisplayText_KeepsLyricLineThatMerelyContainsBrackets()
    {
        // 줄 전체가 마커가 아닌( 뒤에 본문이 붙은 ) 경우는 그대로 둔다(과도한 제거 방지).
        var raw = "[1] 후렴 시작\n본문";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("[1] 후렴 시작\n본문");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \n  ")]
    public void ToDisplayText_EmptyOrWhitespace_ReturnsEmpty(string? raw)
    {
        LyricsDisplayFormatter.ToDisplayText(raw).Should().BeEmpty();
    }

    // ─── ToVersePages ────────────────────────────────────────────────────────

    [Fact]
    public void ToVersePages_SingleVerse_ReturnsSinglePage()
    {
        // 절 구분자(\n\n)가 없으면 전체가 하나의 페이지.
        var raw = "Amazing grace";

        var pages = LyricsDisplayFormatter.ToVersePages(raw);

        pages.Should().ContainSingle().Which.Should().Be("Amazing grace");
    }

    [Fact]
    public void ToVersePages_MultipleVerses_ReturnsOnePagePerVerse()
    {
        // 마커([ ]) 로 구분된 두 절 → 두 페이지.
        var raw = "[1]\n첫째 줄\n둘째 줄\n[2]\n셋째 줄";

        var pages = LyricsDisplayFormatter.ToVersePages(raw);

        pages.Should().HaveCount(2);
        pages[0].Should().Be("첫째 줄\n둘째 줄");
        pages[1].Should().Be("셋째 줄");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \n  ")]
    public void ToVersePages_EmptyOrWhitespace_ReturnsEmpty(string? raw)
    {
        LyricsDisplayFormatter.ToVersePages(raw).Should().BeEmpty();
    }

    [Fact]
    public void ToVersePages_ThreeVerses_ReturnsThreePages()
    {
        var raw = "[1]\n1절\n[2]\n2절\n[3]\n3절";

        var pages = LyricsDisplayFormatter.ToVersePages(raw);

        pages.Should().HaveCount(3);
        pages[0].Should().Be("1절");
        pages[1].Should().Be("2절");
        pages[2].Should().Be("3절");
    }

    // ─── GetVersePage ────────────────────────────────────────────────────────

    [Fact]
    public void GetVersePage_ValidIndex_ReturnsCorrectPage()
    {
        var raw = "[1]\n첫째\n[2]\n둘째\n[3]\n셋째";

        LyricsDisplayFormatter.GetVersePage(raw, 0).Should().Be("첫째");
        LyricsDisplayFormatter.GetVersePage(raw, 1).Should().Be("둘째");
        LyricsDisplayFormatter.GetVersePage(raw, 2).Should().Be("셋째");
    }

    [Fact]
    public void GetVersePage_NegativeIndex_ClampsToFirst()
    {
        var raw = "[1]\n첫째\n[2]\n둘째";

        LyricsDisplayFormatter.GetVersePage(raw, -1).Should().Be("첫째");
    }

    [Fact]
    public void GetVersePage_IndexAboveCount_ClampsToLast()
    {
        var raw = "[1]\n첫째\n[2]\n둘째";

        LyricsDisplayFormatter.GetVersePage(raw, 99).Should().Be("둘째");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetVersePage_EmptyLyrics_ReturnsEmpty(string? raw)
    {
        LyricsDisplayFormatter.GetVersePage(raw, 0).Should().BeEmpty();
    }
}
