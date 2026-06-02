using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>예배 순서 항목의 LIVE 여부 판단(순수 함수) 검증.</summary>
public class WorshipLiveStateTests
{
    [Theory]
    [InlineData("song:1", "song:1", true)]   // 같은 Id → 라이브
    [InlineData("song:1", "song:2", false)]  // 다른 Id → 아님
    [InlineData("song:1", null, false)]      // 라이브 항목 없음 → 아님
    [InlineData(null, null, false)]          // 둘 다 없음 → 아님(빈 값끼리 우연히 같다고 표시 안 함)
    [InlineData("", "", false)]              // 빈 문자열끼리 → 아님
    [InlineData(null, "song:1", false)]      // 항목 Id 없음 → 아님
    public void IsLive_TrueOnlyWhenIdsMatchAndNonEmpty(string? itemId, string? liveItemId, bool expected)
        => WorshipLiveState.IsLive(itemId, liveItemId).Should().Be(expected);
}
