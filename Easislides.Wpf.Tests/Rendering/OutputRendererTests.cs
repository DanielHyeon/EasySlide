using System;
using System.Windows;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
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

    [Fact]
    public void CreateScene_Active_UsesLyricsMonitorAppearanceSettings()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            ShowLyricsMonitorAlertBox: true,
            LyricsMonitorTextColorArgb: unchecked((int)0xFF123456),
            LyricsMonitorBackgroundColorArgb: unchecked((int)0xFFABCDEF),
            LyricsMonitorShowNotations: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Amazing Grace", "Display 2", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsLyricsAlertBox.Should().BeTrue();
        scene.LyricsMonitorTextColorArgb.Should().Be(unchecked((int)0xFF123456));
        scene.LyricsMonitorBackgroundColorArgb.Should().Be(unchecked((int)0xFFABCDEF));
        scene.LyricsMonitorShowNotations.Should().BeFalse();
    }

    [Theory]
    [InlineData("P", true, false, false)]
    [InlineData("PowerPoint", true, false, false)]
    [InlineData("M", false, true, false)]
    [InlineData("Media", false, true, false)]
    [InlineData("Song", true, true, true)]
    public void CreateScene_ActiveWithPanelOverlaySettings_UsesItemKind(
        string itemKind,
        bool noPowerPointPanelOverlay,
        bool noMediaPanelOverlay,
        bool expectedOverlay)
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            NoPowerPointPanelOverlay: noPowerPointPanelOverlay,
            NoMediaPanelOverlay: noMediaPanelOverlay);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active,
                "Live Item",
                "Display 2",
                IsBlackout: false,
                CurrentItemKind: itemKind),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsPanelOverlay.Should().Be(expectedOverlay);
    }

    [Fact]
    public void CreateScene_ReadyWithUserGap_UsesGapLogoAndFadeSettings()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            GapItemOption: GapItemMode.User,
            GapItemLogoFile: @"C:\EasiSlides\Images\gap-logo.png",
            GapItemUseFade: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: LiveSessionSnapshot.Off,
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            TransitionElapsed: TimeSpan.FromMilliseconds(250),
            LiveOutputSettings: settings));

        scene.Kind.Should().Be(OutputSceneKind.Ready);
        scene.DisplayTitle.Should().Be("gap-logo");
        scene.StatusLabel.Should().Be("GAP");
        scene.GapItemOption.Should().Be(GapItemMode.User);
        scene.GapItemLogoFile.Should().Be(@"C:\EasiSlides\Images\gap-logo.png");
        scene.GapItemUseFade.Should().BeTrue();
        scene.TransitionFrame.Kind.Should().Be(TransitionEffectKind.Fade);
    }

    [Fact]
    public void CreateScene_Threads_BackgroundGradientColor2()
    {
        // G2(FrmBackground 슬라이스): 배경 그라데이션 끝색이 설정→렌더→scene 으로 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorBackgroundColorArgb: unchecked((int)0xFF112233),
            LyricsMonitorBackgroundColor2Argb: unchecked((int)0xFF445566));

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorBackgroundColorArgb.Should().Be(unchecked((int)0xFF112233));
        scene.LyricsMonitorBackgroundColor2Argb.Should().Be(unchecked((int)0xFF445566),
            "그라데이션 끝색이 scene 으로 전달돼야 함");
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
