using System;
using System.Windows;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class OutputRendererTests
{
    [Fact]
    public void CreateScene_Active_UsesLiveTitlePlacementAndTransitionFrame()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Amazing Grace", "Display 2", IsBlackout: false),
            Output: output,
            ViewportWidth: 300,
            ViewportHeight: 300,
            ContentPixelWidth: 1920,
            ContentPixelHeight: 1080,
            FillMode: ImageFillMode.Fit,
            TransitionKind: TransitionEffectKind.Fade,
            TransitionAction: TransitionActionKind.AsStored,
            TransitionDuration: TimeSpan.FromSeconds(2),
            TransitionElapsed: TimeSpan.FromSeconds(1),
            BackgroundMode: TransitionBackgroundMode.BothBackgrounds));

        scene.Kind.Should().Be(OutputSceneKind.Live);
        scene.DisplayTitle.Should().Be("Amazing Grace");
        scene.StatusLabel.Should().Be("LIVE");
        scene.OutputMonitorName.Should().Be("Display 2");
        scene.IsOutputOpen.Should().BeTrue();
        scene.ContentPlacement.Should().Be(new ImagePlacement(0, 65, 300, 169));
        scene.TransitionFrame.Kind.Should().Be(TransitionEffectKind.Fade);
        scene.TransitionFrame.Progress.Should().BeApproximately(0.5, 0.0001);
    }

    [Fact]
    public void CreateScene_Blackout_SuppressesContentAndUsesBlackLabels()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Hidden, "Amazing Grace", "Display 2", IsBlackout: true),
            Output: output,
            ViewportWidth: 300,
            ViewportHeight: 300,
            ContentPixelWidth: 1920,
            ContentPixelHeight: 1080));

        scene.Kind.Should().Be(OutputSceneKind.Blackout);
        scene.DisplayTitle.Should().Be("BLACK");
        scene.StatusLabel.Should().Be("BLACKOUT");
        scene.IsBlackout.Should().BeTrue();
        scene.ContentPlacement.Should().Be(ImagePlacement.Empty);
    }

    [Fact]
    public void CreateScene_OutputClosed_ReturnsStandbyScene()
    {
        var sut = CreateRenderer();

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: LiveSessionSnapshot.Off,
            Output: OutputWindowState.Closed,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.Kind.Should().Be(OutputSceneKind.Standby);
        scene.DisplayTitle.Should().Be("STANDBY");
        scene.StatusLabel.Should().Be("STANDBY");
        scene.IsOutputOpen.Should().BeFalse();
        scene.Viewport.Should().Be(new Rect(0, 0, 1280, 720));
    }

    private static OutputRenderer CreateRenderer()
        => new(new ImageAssetService(), new TransitionEffectService());

    private static OutputWindowState OpenOutput(string displayName)
    {
        var display = new OutputDisplay("display-2", displayName, 1920, 0, 1920, 1080, 1);
        return new OutputWindowState(
            IsOpen: true,
            display,
            new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false));
    }
}
