using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

/// <summary>파일명 검색 필터(미디어·PPT 폴더 브라우저 공용 순수 함수) 검증.</summary>
public class FileNameFilterTests
{
    [Theory]
    [InlineData("찬양영상.mp4", "찬양", true)]   // 부분 일치
    [InlineData("Worship.mp4", "worship", true)] // 대소문자 무시
    [InlineData("Worship.mp4", "WOR", true)]     // 앞부분 일치(대소문자 무시)
    [InlineData("intro.mp4", "찬양", false)]      // 불일치
    [InlineData("a.mp4", "", true)]              // 빈 검색어 → 모두 통과
    [InlineData("a.mp4", "   ", true)]           // 공백뿐 → 모두 통과
    [InlineData("a.mp4", null, true)]            // null → 모두 통과
    [InlineData("찬양 영상.mp4", "  영상  ", true)] // 검색어 앞뒤 공백은 다듬어 비교
    public void Matches_SubstringCaseInsensitive_EmptyMeansAll(string? fileName, string? filter, bool expected)
        => FileNameFilter.Matches(fileName, filter).Should().Be(expected);

    [Fact]
    public void Matches_NullFileName_TreatedAsEmpty()
    {
        FileNameFilter.Matches(null, "찬양").Should().BeFalse("이름 없으면 어떤 검색어와도 불일치");
        FileNameFilter.Matches(null, "").Should().BeTrue("검색어 없으면 통과");
    }
}
