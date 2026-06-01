using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class SongInitialGroupingTests
{
    [Theory]
    [InlineData("가나다", "ㄱ")]      // 완성형 한글 초성
    [InlineData("나의", "ㄴ")]
    [InlineData("하나님", "ㅎ")]
    [InlineData("까치", "ㄱ")]        // 쌍자음 초성(ㄲ) → 기본 자음 ㄱ
    [InlineData("따뜻한", "ㄷ")]      // ㄸ → ㄷ
    [InlineData("amazing", "A")]      // 영문 소문자 → 대문자
    [InlineData("Blessed", "B")]
    [InlineData("3절 찬양", "#")]     // 숫자 → #
    [InlineData("!환호", "기타")]     // 기타
    [InlineData("  은혜", "ㅇ")]      // 앞 공백 무시
    [InlineData("", "기타")]          // 빈 제목
    [InlineData("   ", "기타")]       // 공백뿐
    public void GetInitial_ReturnsExpectedKey(string title, string expected)
        => SongInitialGrouping.GetInitial(title).Should().Be(expected);

    [Fact]
    public void OrderedDistinctInitials_OrdersByKanaThenAlphaThenHashThenOther()
    {
        var titles = new[] { "Blessed", "은혜", "3절", "!기타", "가나다", "amazing", "은총" };

        var initials = SongInitialGrouping.OrderedDistinctInitials(titles);

        // 한글 자음(가나다) → 영문 A~Z → 숫자(#) → 기타. "은혜"·"은총" 은 같은 ㅇ 으로 중복 제거.
        initials.Should().Equal("ㄱ", "ㅇ", "A", "B", "#", "기타");
    }

    [Fact]
    public void OrderedDistinctInitials_IgnoresEmptyTitles()
    {
        var initials = SongInitialGrouping.OrderedDistinctInitials(new[] { "은혜", "", "  " });

        initials.Should().Equal("ㅇ");
    }

    [Fact]
    public void Rank_OrdersHangulBeforeAlphaBeforeHash()
    {
        SongInitialGrouping.Rank("ㄱ").Should().BeLessThan(SongInitialGrouping.Rank("A"));
        SongInitialGrouping.Rank("A").Should().BeLessThan(SongInitialGrouping.Rank("#"));
        SongInitialGrouping.Rank("#").Should().BeLessThan(SongInitialGrouping.Rank(SongInitialGrouping.OtherKey));
    }
}
