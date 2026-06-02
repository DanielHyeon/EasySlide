using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class WindowPlacementCalculatorTests
{
    // 단일 1920x1080 화면(왼쪽 위 0,0)을 기준으로 한 표준 시나리오.
    private const double ScreenLeft = 0;
    private const double ScreenTop = 0;
    private const double ScreenWidth = 1920;
    private const double ScreenHeight = 1080;
    private const double MinWidth = 800;
    private const double MinHeight = 600;

    private static WindowPlacement? Compute(int left, int top, int width, int height, bool maximized = false)
        => WindowPlacementCalculator.ComputeRestore(
            left, top, width, height, maximized,
            ScreenLeft, ScreenTop, ScreenWidth, ScreenHeight, MinWidth, MinHeight);

    [Fact]
    public void NeverSaved_ZeroSize_ReturnsNull()
    {
        // 저장된 적 없음(폭/높이 0) → null(호출 측이 기본 크기·중앙을 쓰게).
        Compute(100, 100, 0, 0).Should().BeNull();
        Compute(100, 100, 1200, 0).Should().BeNull("높이만 0 이어도 미저장 취급");
        Compute(100, 100, 0, 800).Should().BeNull("폭만 0 이어도 미저장 취급");
    }

    [Fact]
    public void NegativeSize_ReturnsNull()
    {
        // 손상된 음수 크기도 미저장 취급(안전).
        Compute(100, 100, -1, 800).Should().BeNull();
        Compute(100, 100, 1200, -5).Should().BeNull();
    }

    [Fact]
    public void FullyOnScreen_ReturnsUnchanged()
    {
        // 화면 안에 완전히 들어오는 정상 배치는 그대로 복원.
        var p = Compute(200, 150, 1200, 800);

        p.Should().NotBeNull();
        p!.Value.Left.Should().Be(200);
        p.Value.Top.Should().Be(150);
        p.Value.Width.Should().Be(1200);
        p.Value.Height.Should().Be(800);
    }

    [Fact]
    public void OffScreenRight_PulledFullyOnScreen()
    {
        // 오른쪽으로 넘친 위치는 창 전체가 보이도록 왼쪽으로 당긴다(left = 화면너비 − 창너비).
        var p = Compute(1800, 100, 1200, 800);

        p.Should().NotBeNull();
        p!.Value.Left.Should().Be(ScreenWidth - 1200, "오른쪽 한계 = 1920-1200=720");
        (p.Value.Left + p.Value.Width).Should().BeLessThanOrEqualTo(ScreenWidth, "창 오른쪽 끝이 화면 안");
    }

    [Fact]
    public void OffScreenBottom_PulledFullyOnScreen()
    {
        // 아래로 넘친 위치는 위로 당긴다(top = 화면높이 − 창높이).
        var p = Compute(100, 1000, 1200, 800);

        p.Should().NotBeNull();
        p!.Value.Top.Should().Be(ScreenHeight - 800, "아래 한계 = 1080-800=280");
        (p.Value.Top + p.Value.Height).Should().BeLessThanOrEqualTo(ScreenHeight);
    }

    [Fact]
    public void NegativePosition_ClampedToScreenOrigin()
    {
        // 화면 원점보다 왼쪽/위(음수)면 원점으로 끌어온다(분리된 모니터에 있던 창 대비).
        var p = Compute(-500, -300, 1200, 800);

        p.Should().NotBeNull();
        p!.Value.Left.Should().Be(ScreenLeft);
        p.Value.Top.Should().Be(ScreenTop);
    }

    [Fact]
    public void LargerThanScreen_ClampedToScreenSize()
    {
        // 화면보다 큰 창은 화면 크기로 줄여 복원(해상도가 작아진 모니터 대비).
        var p = Compute(0, 0, 3000, 2000);

        p.Should().NotBeNull();
        p!.Value.Width.Should().Be(ScreenWidth);
        p.Value.Height.Should().Be(ScreenHeight);
    }

    [Fact]
    public void SmallerThanMinimum_ClampedUpToMinimum()
    {
        // 최소 크기보다 작게 저장됐으면 최소 크기로 키워 복원(찌그러진 창 방지).
        var p = Compute(100, 100, 400, 300);

        p.Should().NotBeNull();
        p!.Value.Width.Should().Be(MinWidth);
        p.Value.Height.Should().Be(MinHeight);
    }

    [Fact]
    public void MaximizedFlag_Preserved()
    {
        // 최대화 상태 플래그는 그대로 전달(View 가 WindowState.Maximized 로 적용, 복원 좌표는 un-maximize 대비).
        var p = Compute(200, 150, 1200, 800, maximized: true);

        p.Should().NotBeNull();
        p!.Value.Maximized.Should().BeTrue();
    }

    [Fact]
    public void SecondMonitorRight_PositiveOffsetScreen_RestoresThere()
    {
        // 가상 데스크톱이 0~3840(듀얼 모니터)일 때 오른쪽 모니터 위치(2000)도 그대로 복원.
        var p = WindowPlacementCalculator.ComputeRestore(
            2000, 100, 1200, 800, false,
            screenLeft: 0, screenTop: 0, screenWidth: 3840, screenHeight: 1080,
            MinWidth, MinHeight);

        p.Should().NotBeNull();
        p!.Value.Left.Should().Be(2000, "듀얼 모니터 가상 데스크톱 안이면 그대로");
    }

    [Fact]
    public void InvalidScreen_ReturnsNull()
    {
        // 화면 크기가 0 이하로 들어오면 보정 기준이 없으므로 복원 포기(기본 배치).
        WindowPlacementCalculator.ComputeRestore(
            100, 100, 1200, 800, false,
            0, 0, 0, 0, MinWidth, MinHeight).Should().BeNull();
    }

    // ── ComputeSplitPercent: 두 패널 높이 → 위 패널 비율(%) ──────────────────────────
    private const int SplitMin = 15;
    private const int SplitMax = 85;

    private static int Split(double top, double bottom)
        => WindowPlacementCalculator.ComputeSplitPercent(top, bottom, SplitMin, SplitMax);

    [Fact]
    public void Split_HalfHalf_Returns50()
    {
        Split(400, 400).Should().Be(50, "반반이면 50%");
    }

    [Fact]
    public void Split_TopLarger_ReturnsProportion()
    {
        Split(600, 200).Should().Be(75, "600/(600+200)=75%");
        Split(300, 700).Should().Be(30, "300/1000=30%");
    }

    [Fact]
    public void Split_ZeroTotal_ReturnsZeroSentinel()
    {
        // 측정 전(합 0)·음수는 0(저장 안 함) 센티넬.
        Split(0, 0).Should().Be(0);
        Split(-10, 400).Should().Be(0, "음수 높이는 비정상 → 0");
        Split(400, -5).Should().Be(0);
    }

    [Fact]
    public void Split_ExtremeRatio_ClampedIntoSafeRange()
    {
        // 한 패널이 거의 사라진 비율은 [15,85] 로 클램프해 다음 실행에 패널이 0 으로 복원되지 않게 한다.
        Split(950, 50).Should().Be(SplitMax, "95% → 상한 85 로 클램프");
        Split(20, 980).Should().Be(SplitMin, "2% → 하한 15 로 클램프");
    }

    [Fact]
    public void Split_ClampedNeverReturnsZeroForValidTotal()
    {
        // 합이 양수면(유효) 절대 0(센티넬)을 돌려주지 않아야 한다 — 하한이 15 라 0 위쪽으로 클램프.
        Split(1, 999).Should().BeGreaterThanOrEqualTo(SplitMin, "유효 입력은 0 센티넬과 겹치지 않음");
    }
}
