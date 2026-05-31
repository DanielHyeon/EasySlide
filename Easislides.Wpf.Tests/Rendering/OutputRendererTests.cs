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
    public void CreateScene_Active_WithBodyText_PassesThroughAndShowsBody()
    {
        // 라이브 곡 가사 본문이 씬에 전달되어 출력에 텍스트로 표시(ShowsBodyText)되어야 한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사\n둘째 줄"),
            Output: output,
            ViewportWidth: 300,
            ViewportHeight: 300));

        scene.Kind.Should().Be(OutputSceneKind.Live);
        scene.BodyText.Should().Be("1절 가사\n둘째 줄");
        scene.ShowsBodyText.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_ActiveWithoutBody_DoesNotShowBody()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "주보 PPT", "Display 2", IsBlackout: false),
            Output: output,
            ViewportWidth: 300,
            ViewportHeight: 300));

        scene.BodyText.Should().BeEmpty();
        scene.ShowsBodyText.Should().BeFalse();
    }

    [Fact]
    public void CreateScene_Hidden_SuppressesBodyText()
    {
        // 숨김 상태에선 본문이 실려 있어도 출력에 노출하지 않는다(Live 가 아니므로).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 300,
            ViewportHeight: 300));

        scene.Kind.Should().Be(OutputSceneKind.Hidden);
        scene.BodyText.Should().BeEmpty();
        scene.ShowsBodyText.Should().BeFalse();
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
    public void CreateScene_Cleared_ShowsBackgroundButSuppressesContentAndOverlay()
    {
        // 비우기(Cleared): 배경은 유지하되 콘텐츠/본문/패널 오버레이를 모두 감춘다(레거시 LiveClear).
        // Blackout 과 달리 IsBlackout=false 라 출력 VM 이 검정 오버레이를 덮지 않는다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사")
            {
                IsCleared = true,
            },
            Output: output,
            ViewportWidth: 300,
            ViewportHeight: 300,
            ContentPixelWidth: 1920,
            ContentPixelHeight: 1080));

        scene.Kind.Should().Be(OutputSceneKind.Cleared);
        scene.IsBlackout.Should().BeFalse("비우기는 검정 오버레이를 쓰지 않음");
        scene.BodyText.Should().BeEmpty("비우기는 본문을 감춤");
        scene.ShowsBodyText.Should().BeFalse();
        scene.ShowsContent.Should().BeFalse();
        scene.ContentPlacement.Should().Be(ImagePlacement.Empty);
        scene.ShowsPanelOverlay.Should().BeFalse("비우기 화면엔 모니터/상태 오버레이도 감춤");
        scene.StatusLabel.Should().Be("CLEARED");
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

    [Theory]
    [InlineData(LyricsTextAlignment.Left)]
    [InlineData(LyricsTextAlignment.Center)]
    [InlineData(LyricsTextAlignment.Right)]
    public void CreateScene_Threads_LyricsTextAlignment(LyricsTextAlignment alignment)
    {
        // 인-셸 가사 정렬(§7.3-A): 설정→렌더→scene 으로 정렬 값이 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(LyricsMonitorTextAlignment: alignment);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTextAlignment.Should().Be(alignment);
    }

    [Fact]
    public void CreateScene_DefaultsLyricsAlignmentToCenter()
    {
        // 기본값은 Center — 기존 출력(가운데 정렬) 동작 보존.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.LyricsMonitorTextAlignment.Should().Be(LyricsTextAlignment.Center);
        scene.LyricsMonitorVerticalAlignment.Should().Be(LyricsVerticalAlignment.Center, "세로 정렬 기본도 가운데");
    }

    [Fact]
    public void CreateScene_Threads_LyricsFontSize()
    {
        // 인-셸 폰트 크기(§7.3-A): 설정→렌더→scene 으로 폰트 크기(px)가 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 64);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontSize.Should().Be(64);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public void CreateScene_Threads_LyricsFontEffects(bool bold, bool italic, bool shadow)
    {
        // 인-셸 폰트 효과(§7.3-A): 굵게·기울임·그림자 bool 이 설정→렌더→scene 으로 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorBold: bold,
            LyricsMonitorItalic: italic,
            LyricsMonitorShadow: shadow);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorBold.Should().Be(bold);
        scene.LyricsMonitorItalic.Should().Be(italic);
        scene.LyricsMonitorShadow.Should().Be(shadow);
    }

    [Fact]
    public void CreateScene_DefaultsLyricsFontEffectsToOff()
    {
        // 기본 모두 off — 기존 출력(효과 없음) 보존.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.LyricsMonitorBold.Should().BeFalse();
        scene.LyricsMonitorItalic.Should().BeFalse();
        scene.LyricsMonitorShadow.Should().BeFalse();
    }

    [Fact]
    public void CreateScene_Threads_LyricsLineSpacing()
    {
        // 인-셸 줄 간격(§7.3-A): 설정→렌더→scene 으로 줄 간격(%)이 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(LyricsMonitorLineSpacingPercent: 150);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorLineSpacingPercent.Should().Be(150);
    }

    [Fact]
    public void CreateScene_DefaultsLineSpacingTo125()
    {
        // 기본 125% — 기존 줄높이(폰트×1.25) 보존.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.LyricsMonitorLineSpacingPercent.Should().Be(125);
    }

    [Fact]
    public void CreateScene_DefaultsLyricsFontSizeTo48()
    {
        // 기본 48px — 기존 출력 폰트 크기 보존.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.LyricsMonitorFontSize.Should().Be(48);
    }

    [Theory]
    [InlineData(LyricsVerticalAlignment.Top)]
    [InlineData(LyricsVerticalAlignment.Center)]
    [InlineData(LyricsVerticalAlignment.Bottom)]
    public void CreateScene_Threads_LyricsVerticalAlignment(LyricsVerticalAlignment alignment)
    {
        // 인-셸 가사 세로 정렬(§7.3-A): 설정→렌더→scene 으로 세로 정렬 값이 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(LyricsMonitorVerticalAlignment: alignment);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorVerticalAlignment.Should().Be(alignment);
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
