using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class BibleSelectionIdTests
{
    [Fact]
    public void TryParse_SequentialSingleRange_ReadsVersionsAndRange()
    {
        // BuildSequentialSelection 이 만드는 형식: "0;주버전;보조버전;책;시작장;시작절;끝장;끝절;"
        var ok = BibleSelectionId.TryParse("0;kjv.db;;1;1;1;1;2;", out var parsed);

        ok.Should().BeTrue();
        parsed.IsSequential.Should().BeTrue();
        parsed.Region1FileName.Should().Be("kjv.db");
        parsed.Region2FileName.Should().BeEmpty();
        parsed.HasRegion2.Should().BeFalse();
        parsed.Ranges.Should().ContainSingle();
        parsed.Ranges[0].Should().Be(new BiblePassageRange(1, 1, 1, 1, 2));
    }

    [Fact]
    public void TryParse_DualLanguage_CapturesRegion2File()
    {
        var ok = BibleSelectionId.TryParse("0;kjv.db;niv.db;43;3;16;3;17;", out var parsed);

        ok.Should().BeTrue();
        parsed.HasRegion2.Should().BeTrue();
        parsed.Region2FileName.Should().Be("niv.db");
        parsed.Ranges[0].Should().Be(new BiblePassageRange(43, 3, 16, 3, 17));
    }

    [Fact]
    public void TryParse_AdHocMultipleRanges_KeepsOrder()
    {
        // 임의 선택(종류 1)은 절 단위 범위(시작=끝)가 여러 개 — 선택 순서를 보존해야 한다.
        var ok = BibleSelectionId.TryParse("1;kjv.db;;1;1;1;1;1;43;3;16;3;16;", out var parsed);

        ok.Should().BeTrue();
        parsed.IsSequential.Should().BeFalse();
        parsed.Ranges.Should().HaveCount(2);
        parsed.Ranges[0].Should().Be(new BiblePassageRange(1, 1, 1, 1, 1));
        parsed.Ranges[1].Should().Be(new BiblePassageRange(43, 3, 16, 3, 16));
    }

    [Fact]
    public void TryParse_NoTrailingSemicolon_StillParses()
    {
        BibleSelectionId.TryParse("0;kjv.db;;1;1;1;1;2", out var parsed).Should().BeTrue();
        parsed.Ranges[0].Should().Be(new BiblePassageRange(1, 1, 1, 1, 2));
    }

    [Fact]
    public void Contains_ChecksChapterThenVerse()
    {
        var range = new BiblePassageRange(1, 1, 5, 2, 3);

        range.Contains(1, 5).Should().BeTrue();   // 시작 절
        range.Contains(1, 4).Should().BeFalse();  // 시작 직전 절
        range.Contains(1, 99).Should().BeTrue();  // 시작 장 뒷 절(다음 장 전)
        range.Contains(2, 3).Should().BeTrue();   // 끝 절
        range.Contains(2, 4).Should().BeFalse();  // 끝 직후 절
        range.Contains(3, 1).Should().BeFalse();  // 끝 장 다음 장
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0;kjv.db;")]          // 구절 범위 없음(머리만)
    [InlineData("0;kjv.db;;1;1;1")]    // 5토큰 미만(불완전 범위)
    [InlineData("9;kjv.db;;1;1;1;1;2;")] // 알 수 없는 종류
    [InlineData("0;;;1;1;1;1;2;")]     // 주 버전 파일 없음
    [InlineData("0;kjv.db;;a;1;1;1;2;")] // 숫자 아닌 토큰
    public void TryParse_Invalid_ReturnsFalse(string? idString)
    {
        BibleSelectionId.TryParse(idString, out var parsed).Should().BeFalse();
        parsed.Ranges.Should().BeEmpty();
    }
}
