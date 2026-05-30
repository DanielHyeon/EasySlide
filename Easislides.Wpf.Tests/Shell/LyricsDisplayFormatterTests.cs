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
}
