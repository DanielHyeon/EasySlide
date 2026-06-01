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

    [Fact]
    public void BuildStar_CoversScreen_AtFullScale()
    {
        // 별의 안쪽 골이 모서리보다 멀어 배율 1 에서 네 모서리·중심·가장자리를 모두 포함 → 전체 덮음(잔여 마스크 없음).
        var geo = OutputWindow.BuildStar(1920, 1080);

        geo.FillContains(new System.Windows.Point(0, 0)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(1920, 0)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(0, 1080)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(1920, 1080)).Should().BeTrue();
        geo.FillContains(new System.Windows.Point(960, 0)).Should().BeTrue("상단 가장자리");
        geo.FillContains(new System.Windows.Point(0, 540)).Should().BeTrue("좌측 가장자리");
        geo.FillContains(new System.Windows.Point(960, 540)).Should().BeTrue("중심");

        // 가장 까다로운 방향(별의 오목 골 방위 = 경계가 중심에 가장 가까운 곳)에서도, 모서리 거리만큼
        // 떨어진 점이 별 안에 있어야 한다. 골 반지름(innerR=모서리거리+1)이 더 멀기 때문 → 모서리 표본이면 충분함을 문서화.
        var cornerDist = System.Math.Sqrt((960.0 * 960) + (540.0 * 540));
        var troughAngle = (-System.Math.PI / 2) + (System.Math.PI / 5); // i=1(첫 오목 골) 방위 = -54°.
        var pointAlongTrough = new System.Windows.Point(
            960 + (cornerDist * System.Math.Cos(troughAngle)),
            540 + (cornerDist * System.Math.Sin(troughAngle)));
        geo.FillContains(pointAlongTrough).Should().BeTrue("골 방위로 모서리 거리만큼 떨어진 점도 별 안 → 노치 없음");
    }

    [Fact]
    public void BuildCross_PlusShape_CoversCenterAndBarsButNotCorners()
    {
        // 십자는 중심·막대는 덮지만 코너는 못 덮는다 → 끝에 화면 전체를 못 덮음 → 2-레이어(뒤 옛 프레임)가 필요함을 문서화.
        var geo = OutputWindow.BuildCross(1920, 1080);

        geo.FillContains(new System.Windows.Point(960, 540)).Should().BeTrue("중심(교차) 포함");
        geo.FillContains(new System.Windows.Point(960, 0)).Should().BeTrue("세로 막대 상단");
        geo.FillContains(new System.Windows.Point(0, 540)).Should().BeTrue("가로 막대 좌측");
        geo.FillContains(new System.Windows.Point(0, 0)).Should().BeFalse("코너는 못 덮음 → 2-레이어 필요");
        geo.FillContains(new System.Windows.Point(1920, 1080)).Should().BeFalse("반대 코너도 못 덮음");
    }

    [Fact]
    public void BuildBowTie_TwoTriangles_CoverSidesNotTopBottomCenter()
    {
        // 나비넥타이는 좌/우 삼각형(코너·측면 덮음)이지만 상/하 중앙은 오므라들어 못 덮음 → 2-레이어 필요함을 문서화.
        var geo = OutputWindow.BuildBowTie(1920, 1080);

        geo.FillContains(new System.Windows.Point(960, 540)).Should().BeTrue("중심(두 삼각형 만남)");
        geo.FillContains(new System.Windows.Point(100, 540)).Should().BeTrue("좌측 삼각형 내부");
        geo.FillContains(new System.Windows.Point(1820, 540)).Should().BeTrue("우측 삼각형 내부");
        geo.FillContains(new System.Windows.Point(960, 50)).Should().BeFalse("상단 중앙 오므라듦 → 못 덮음");
        geo.FillContains(new System.Windows.Point(960, 1030)).Should().BeFalse("하단 중앙 오므라듦 → 못 덮음");
    }

    [Fact]
    public void BuildHeart_CoversCenterNotCorners_IsHeartShaped()
    {
        // 하트는 중심·본체는 덮지만 코너·위 노치는 못 덮음 → 2-레이어 필요. 매개변수 곡선 폴리라인이 닫힌 하트.
        var geo = OutputWindow.BuildHeart(1920, 1080);

        geo.FillContains(new System.Windows.Point(960, 540)).Should().BeTrue("하트 본체 중심 포함");
        geo.FillContains(new System.Windows.Point(0, 0)).Should().BeFalse("좌상 코너 못 덮음 → 2-레이어 필요");
        geo.FillContains(new System.Windows.Point(1920, 0)).Should().BeFalse("우상 코너 못 덮음");
        // 위 중앙(두 봉우리 사이 노치)은 하트 밖.
        geo.FillContains(new System.Windows.Point(960, 30)).Should().BeFalse("위 노치는 하트 밖");
        // 닫힌 단일 도형 — 경계가 유효해야 FillContains 가 동작(폴리라인이 깨지지 않음).
        geo.Figures.Should().ContainSingle().Which.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void BuildWedge_SmallSweep_CoversTopRightSectorOnly()
    {
        // 90° 부채꼴: 12시→3시(우상 사분면) 섹터만 덮는다. 우상은 안, 좌하는 밖.
        var geo = OutputWindow.BuildWedge(1920, 1080, 90);

        geo.FillContains(new System.Windows.Point(1100, 300)).Should().BeTrue("12~3시 섹터 안(우상)");
        geo.FillContains(new System.Windows.Point(700, 800)).Should().BeFalse("좌하 섹터는 아직 안 덮음");
    }

    [Fact]
    public void BuildWedge_NearFullSweep_CoversWholeScreen()
    {
        // 거의 360° 부채꼴은 화면 전체를 덮는다(끝에 잔여 마스크 없음 → 단일 레이어로 충분).
        var geo = OutputWindow.BuildWedge(1920, 1080, 359.9);

        geo.FillContains(new System.Windows.Point(960, 540)).Should().BeTrue("중심");
        geo.FillContains(new System.Windows.Point(50, 50)).Should().BeTrue("좌상");
        geo.FillContains(new System.Windows.Point(1870, 1030)).Should().BeTrue("우하");
        geo.FillContains(new System.Windows.Point(700, 800)).Should().BeTrue("좌하");
    }
}
