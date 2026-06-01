using System;
using System.Linq;
using Easislides.Wpf;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class OutputWindowTileClipTests
{
    private static readonly TimeSpan Dur = TimeSpan.FromMilliseconds(300);

    [Fact]
    public void BuildBlinds_Horizontal_TilesFullHeight_NoGaps_StartHidden()
    {
        var tiles = OutputWindow.BuildBlinds(1920, 1080, horizontal: true, Dur);

        tiles.Should().HaveCount(8);
        // 각 띠는 시작 시 두께 0(숨김) → 끝에 띠 높이.
        tiles.Should().OnlyContain(t => t.From.Height == 0);
        tiles[0].To.Height.Should().BeApproximately(1080.0 / 8, 0.001);
        // 띠들의 끝 사각이 전체 높이를 빈틈없이 덮는다(합 = 전체 높이).
        tiles.Sum(t => t.To.Height).Should().BeApproximately(1080, 0.001);
        // 각 띠는 전체 너비.
        tiles.Should().OnlyContain(t => t.To.Width == 1920);
    }

    [Fact]
    public void BuildBlinds_Vertical_TilesFullWidth()
    {
        var tiles = OutputWindow.BuildBlinds(1920, 1080, horizontal: false, Dur);

        tiles.Should().HaveCount(8);
        tiles.Should().OnlyContain(t => t.From.Width == 0);
        tiles.Sum(t => t.To.Width).Should().BeApproximately(1920, 0.001);
        tiles.Should().OnlyContain(t => t.To.Height == 1080);
    }

    [Fact]
    public void BuildCheckerboard_CoversGrid_TwoPhases_StartHiddenEndsWithinDuration()
    {
        var tiles = OutputWindow.BuildCheckerboard(1920, 1080, Dur);

        tiles.Should().HaveCount(10 * 6);
        // 시작은 0-크기(중심점) → 셀 전체.
        tiles.Should().OnlyContain(t => t.From.Width == 0 && t.From.Height == 0);
        // 두 단계: 일부는 지연 시작(체커 패턴), 일부는 즉시.
        tiles.Should().Contain(t => t.Begin == TimeSpan.Zero);
        tiles.Should().Contain(t => t.Begin > TimeSpan.Zero);
        // 모든 셀이 자기 시작지연+길이로 전체 duration 안에 끝난다(클립 제거 카운트다운 보장).
        tiles.Should().OnlyContain(t => t.Begin + t.Duration <= Dur + TimeSpan.FromMilliseconds(1));
        // 끝 사각 면적 합 = 전체 화면 면적(셀이 격자를 빈틈없이 덮음).
        tiles.Sum(t => t.To.Width * t.To.Height).Should().BeApproximately(1920.0 * 1080.0, 1.0);
    }
}
