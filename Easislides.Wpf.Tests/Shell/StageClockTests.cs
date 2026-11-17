using System;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>증분160-E — 스테이지 시계 포맷(순수 함수). 24시간 HH:mm.</summary>
public class StageClockTests
{
    [Theory]
    [InlineData(2026, 1, 1, 14, 5, 0, "14:05")]
    [InlineData(2026, 6, 4, 9, 30, 45, "09:30")]   // 초는 버리고 분까지만.
    [InlineData(2026, 12, 31, 0, 0, 0, "00:00")]    // 자정.
    [InlineData(2026, 12, 31, 23, 59, 0, "23:59")]  // 24시간 표기.
    public void Format_ProducesHourMinute24h(int y, int mo, int d, int h, int mi, int s, string expected)
    {
        StageClock.Format(new DateTime(y, mo, d, h, mi, s)).Should().Be(expected);
    }
}
