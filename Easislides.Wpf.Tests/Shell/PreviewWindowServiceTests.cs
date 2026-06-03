using System;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 증분160-A — 스테이지(Preview) 모니터 상태머신. 출력 창(OutputWindowService)과 같은 구조의 병렬 서비스로,
/// 회중용 출력과 독립적으로 열고/옮기고/닫힌다. (실제 창·렌더는 PreviewWindowHost 가 이 상태를 구독해 처리 — 별도 증분.)
/// </summary>
public class PreviewWindowServiceTests
{
    private static OutputDisplay Display2 => new("display-2", "Display 2", 1920, 0, 1920, 1080, 1.25);

    [Fact]
    public void Current_StartsClosed()
    {
        var sut = new PreviewWindowService();

        sut.Current.Should().Be(PreviewWindowState.Closed);
        sut.Current.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Open_SetsOpenStateWithFullScreenPlacement_AndRaisesChanged()
    {
        var sut = new PreviewWindowService();
        PreviewWindowState? raised = null;
        sut.PreviewChanged += (_, e) => raised = e.State;

        sut.Open(Display2, windowed: false);

        sut.Current.IsOpen.Should().BeTrue();
        sut.Current.Display.Should().Be(Display2);
        // 창모드가 아니면(windowed:false) 그 모니터를 꽉 채운다(출력과 동일 배치 계산).
        sut.Current.Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false));
        raised.Should().Be(sut.Current, "상태가 바뀌면 PreviewChanged 가 새 상태로 발생해야 함");
    }

    [Fact]
    public void MoveTo_ReappliesPlacement_AndRaisesChanged()
    {
        var sut = new PreviewWindowService();
        var display1 = new OutputDisplay("display-1", "Display 1", 0, 0, 1920, 1080, 1);
        sut.Open(display1, windowed: true);
        PreviewWindowState? raised = null;
        sut.PreviewChanged += (_, e) => raised = e.State;

        sut.MoveTo(Display2, windowed: false);

        sut.Current.Display.Should().Be(Display2);
        sut.Current.Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false));
        raised.Should().Be(sut.Current);
    }

    [Fact]
    public void Close_ResetsToClosed_AndRaisesChanged()
    {
        var sut = new PreviewWindowService();
        sut.Open(Display2, windowed: false);
        PreviewWindowState? raised = null;
        sut.PreviewChanged += (_, e) => raised = e.State;

        sut.Close();

        sut.Current.Should().Be(PreviewWindowState.Closed);
        raised.Should().Be(PreviewWindowState.Closed);
    }

    [Fact]
    public void Open_SameStateTwice_SecondIsNoOp_NoSecondEvent()
    {
        var sut = new PreviewWindowService();
        var changeCount = 0;
        sut.PreviewChanged += (_, _) => changeCount++;

        sut.Open(Display2, windowed: false);
        sut.Open(Display2, windowed: false); // 같은 모니터·같은 모드 → 상태 동일 → 무시.

        changeCount.Should().Be(1, "같은 상태로의 갱신은 이벤트를 다시 내지 않아야 함");
    }

    [Fact]
    public void Open_NullDisplay_Throws()
    {
        var sut = new PreviewWindowService();

        var act = () => sut.Open(null!, windowed: false);

        act.Should().Throw<ArgumentNullException>();
    }
}
