using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class OutputWindowViewModelTests
{
    [Fact]
    public void ApplySession_Active_ShowsCurrentLiveItem()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        sut.State.Should().Be(LiveState.Active);
        sut.DisplayTitle.Should().Be("Amazing Grace");
        sut.StatusLabel.Should().Be("LIVE");
        sut.IsBlackout.Should().BeFalse();
        sut.OutputMonitorName.Should().Be("Display 2");
        sut.Scene.Kind.Should().Be(OutputSceneKind.Live);
    }

    [Fact]
    public void ApplySession_Blackout_UsesProtectedBlackScreenLabel()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "Amazing Grace",
            "Display 2",
            IsBlackout: true));

        sut.State.Should().Be(LiveState.Hidden);
        sut.DisplayTitle.Should().Be("BLACK");
        sut.StatusLabel.Should().Be("BLACKOUT");
        sut.IsBlackout.Should().BeTrue();
        sut.Scene.Kind.Should().Be(OutputSceneKind.Blackout);
    }

    [Fact]
    public void ApplyOutput_Closed_ReturnsToStandby()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("display-2", "Display 2", 1920, 0, 1920, 1080, 1),
            new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false)));

        sut.ApplyOutput(OutputWindowState.Closed);

        sut.IsOutputOpen.Should().BeFalse();
        sut.OutputMonitorName.Should().BeEmpty();
        sut.Scene.Kind.Should().Be(OutputSceneKind.Standby);
    }
}
