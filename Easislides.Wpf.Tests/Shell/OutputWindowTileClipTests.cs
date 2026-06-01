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

    [Fact]
    public void BuildDoors_Open_TwoHalves_CoverFullWidth_StartHidden()
    {
        var doors = OutputWindow.BuildDoors(1920, 1080, open: true, Dur);

        doors.Should().HaveCount(2);
        doors.Should().OnlyContain(t => t.From.Width == 0, "시작 시 두 문 모두 폭 0(숨김)");
        // 두 문이 합쳐 전체 너비를 덮는다(각 절반).
        doors.Sum(t => t.To.Width).Should().BeApproximately(1920, 0.001);
        doors.Should().OnlyContain(t => t.To.Height == 1080 && t.Duration == Dur);
    }

    [Fact]
    public void BuildDoors_Close_StartsAtEdges_MeetAtCenter()
    {
        var doors = OutputWindow.BuildDoors(1920, 1080, open: false, Dur);

        doors.Should().HaveCount(2);
        // 닫기: 한 문은 좌측 끝(x=0)에서, 다른 문은 우측 끝(시작 x=w)에서 시작.
        doors.Should().Contain(t => t.From.X == 0);
        doors.Should().Contain(t => t.From.X == 1920);
        doors.Should().OnlyContain(t => t.From.Width == 0, "닫기도 시작 시 두 문 모두 폭 0(숨김)");
        doors.Sum(t => t.To.Width).Should().BeApproximately(1920, 0.001);
    }

    [Fact]
    public void BuildDiamond_CoversScreen_AtFullScale()
    {
        // 마름모가 배율 1 에서 화면 모서리(가장 먼 점)를 포함 → 전체 덮음(잔여 마스크 없음).
        var geo = OutputWindow.BuildDiamond(1920, 1080);

        // 네 모서리가 모두 마름모 안에 있어야 한다(FillContains).
        geo.FillContains(new System.Windows.Point(0, 0)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(1920, 0)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(0, 1080)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(1920, 1080)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(960, 540)).Should().BeTrue("중심 포함");
    }
}
