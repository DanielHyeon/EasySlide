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

    [Fact]
    public void CreateScene_Active_WithSongOverrideColors_PrefersSongColorsOverSettings()
    {
        // 곡별 FormatData 색이 스냅샷에 실려 오면(OverrideTextColorArgb 등) 운영 기본색 대신
        // 그 곡의 색으로 송출한다(레거시 per-song 색). 배경은 솔리드(그라데이션 해제)로 칠한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextColorArgb: unchecked((int)0xFF000000),
            LyricsMonitorBackgroundColorArgb: unchecked((int)0xFFFFFFFF),
            LyricsMonitorBackgroundColor2Argb: unchecked((int)0xFF333333),
            LyricsMonitorBackgroundIsGradient: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideTextColorArgb: unchecked((int)0xFFFF0000),
                OverrideBackgroundColorArgb: unchecked((int)0xFF0000FF)),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTextColorArgb.Should().Be(unchecked((int)0xFFFF0000), "곡 글자색이 운영 기본색을 이긴다");
        scene.LyricsMonitorBackgroundColorArgb.Should().Be(unchecked((int)0xFF0000FF), "곡 배경색이 운영 기본색을 이긴다");
        scene.LyricsMonitorBackgroundColor2Argb.Should().Be(unchecked((int)0xFF0000FF), "곡 배경은 솔리드 — 끝색도 동일");
        scene.LyricsMonitorBackgroundIsGradient.Should().BeFalse("곡 배경 오버라이드는 단색");
    }

    [Fact]
    public void CreateScene_Hidden_IgnoresSongOverrideColors()
    {
        // 라이브가 아니면(숨김/블랙아웃 등) 곡 색 오버라이드를 적용하지 않는다 — 운영 기본색 유지.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextColorArgb: unchecked((int)0xFF000000),
            LyricsMonitorBackgroundColorArgb: unchecked((int)0xFFFFFFFF));

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideTextColorArgb: unchecked((int)0xFFFF0000),
                OverrideBackgroundColorArgb: unchecked((int)0xFF0000FF)),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTextColorArgb.Should().Be(unchecked((int)0xFF000000), "Live 가 아니면 기본색");
        scene.LyricsMonitorBackgroundColorArgb.Should().Be(unchecked((int)0xFFFFFFFF), "Live 가 아니면 기본색");
    }

    [Fact]
    public void CreateScene_Active_WithSongOverrideAlignment_PrefersSongAlignment()
    {
        // 곡별 FormatData 정렬이 스냅샷에 실려 오면 운영 기본 정렬 대신 그 곡의 정렬로 송출한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorTextAlignment: LyricsTextAlignment.Center);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideTextAlignment: LyricsTextAlignment.Right),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTextAlignment.Should().Be(LyricsTextAlignment.Right, "곡 정렬이 운영 기본 정렬을 이긴다");
    }

    [Fact]
    public void CreateScene_Hidden_IgnoresSongOverrideAlignment()
    {
        // 라이브가 아니면 곡 정렬 오버라이드를 적용하지 않는다 — 운영 기본 정렬 유지.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorTextAlignment: LyricsTextAlignment.Center);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideTextAlignment: LyricsTextAlignment.Right),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTextAlignment.Should().Be(LyricsTextAlignment.Center, "Live 가 아니면 기본 정렬");
    }

    [Fact]
    public void CreateScene_Active_WithSongOverrideFont_PrefersSongFontOverSettings()
    {
        // 곡별 FormatData 폰트명·크기가 스냅샷에 실려 오면 운영 기본 글꼴·크기 대신 그 곡의 것으로 송출한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 48);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideFontName: "Batang",
                OverrideFontSizePx: 90),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontFamily.Should().Be("Batang", "곡 글꼴이 운영 기본 글꼴을 이긴다");
        scene.LyricsMonitorFontSize.Should().Be(90, "곡 폰트 크기가 운영 기본 크기를 이긴다");
    }

    [Fact]
    public void CreateScene_Hidden_IgnoresSongOverrideFont()
    {
        // 라이브가 아니면(숨김 등) 곡 글꼴 오버라이드를 적용하지 않는다 — 운영 기본 글꼴·크기 유지.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 48);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideFontName: "Batang",
                OverrideFontSizePx: 90),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontFamily.Should().BeEmpty("Live 가 아니면 기본 글꼴(빈 문자열=테마 상속)");
        scene.LyricsMonitorFontSize.Should().Be(48, "Live 가 아니면 기본 크기");
    }

    [Fact]
    public void CreateScene_Active_WithSongBackgroundImage_CarriesImagePath()
    {
        // 곡별 FormatData 배경 이미지(61)가 스냅샷에 실려 오면 씬에 경로를 실어 출력이 색 배경 대신 이미지를 표시하게 한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideBackgroundImagePath: @"C:\bg\a.jpg"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.BackgroundImagePath.Should().Be(@"C:\bg\a.jpg", "곡 배경 이미지 경로를 씬에 싣는다");
    }

    [Fact]
    public void CreateScene_Hidden_IgnoresSongBackgroundImage()
    {
        // 라이브가 아니면 배경 이미지 오버라이드를 적용하지 않는다 — 색 배경 유지.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideBackgroundImagePath: @"C:\bg\a.jpg"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.BackgroundImagePath.Should().BeEmpty("Live 가 아니면 배경 이미지 미적용");
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
    public void CreateScene_ShowsPositionIndicator_WhenEnabledAndLiveWithLabel()
    {
        // 위치 인디케이터: 설정 on + Live + 라벨 존재 → ShowsPositionIndicator true, 라벨 전달.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsPositionIndicator: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemPositionLabel: "2/4"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.PositionLabel.Should().Be("2/4");
        scene.ShowsPositionIndicator.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_HidesPositionIndicator_WhenSettingOff()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsPositionIndicator: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemPositionLabel: "2/4"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsPositionIndicator.Should().BeFalse("설정 off 면 숨김");
    }

    [Fact]
    public void CreateScene_HidesPositionIndicator_WhenNotLive()
    {
        // 숨김/대기 상태에선 라벨이 있어도 인디케이터를 노출하지 않는다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsPositionIndicator: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Hidden, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemPositionLabel: "2/4"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsPositionIndicator.Should().BeFalse();
    }

    [Fact]
    public void CreateScene_ShowsTitleHeading_WhenEnabledAndLiveWithBody()
    {
        // 제목 헤딩(§7.3-A): 설정 on + Live + 가사 본문 + 제목 존재 → ShowsTitleHeading true(가사 위 상단 배너).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsTitleHeading: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.DisplayTitle.Should().Be("은혜로다");
        scene.ShowsTitleHeading.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_TitleHeadingFirstScreenOnly_ShowsOnlyOnFirstVerse()
    {
        // "At First Screen Only"(§7.3-A): 설정 on 이면 제목 헤딩을 곡 첫 절(pageIndex 0)에만 표시.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(
            ShowLyricsTitleHeading: true, TitleHeadingFirstScreenOnly: true);

        // 첫 절(pageIndex 0) → 헤딩 표시.
        var first = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사", CurrentLyricsPageIndex: 0),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));
        first.ShowsTitleHeading.Should().BeTrue("첫 절에는 헤딩 표시");

        // 둘째 절(pageIndex 1) → 헤딩 숨김.
        var second = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "2절 가사", CurrentLyricsPageIndex: 1),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));
        second.ShowsTitleHeading.Should().BeFalse("첫 절 이후엔 헤딩 숨김");
    }

    [Fact]
    public void CreateScene_TitleHeadingFirstScreenOff_ShowsOnAllVerses()
    {
        // 기본 off — 헤딩이 켜져 있으면 모든 절에 표시(기존 동작 보존).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(
            ShowLyricsTitleHeading: true, TitleHeadingFirstScreenOnly: false);

        var second = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "2절 가사", CurrentLyricsPageIndex: 1),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        second.ShowsTitleHeading.Should().BeTrue("FirstScreenOnly off 면 모든 절에 헤딩");
    }

    [Fact]
    public void CreateScene_TitleHeadingOff_FirstScreenOnlyCannotForceHeading()
    {
        // 우선순위 잠금(code-review SUGGESTION): 제목 헤딩 자체가 off 면 FirstScreenOnly on·첫 절이어도 헤딩 안 뜸.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(
            ShowLyricsTitleHeading: false, TitleHeadingFirstScreenOnly: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사", CurrentLyricsPageIndex: 0),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsTitleHeading.Should().BeFalse("헤딩 마스터 토글 off 가 우선");
    }

    [Fact]
    public void CreateScene_Threads_TitleHeadingAlignment()
    {
        // 제목 헤딩 정렬(§7.3-A): 설정→렌더→scene 으로 헤딩 가로 정렬이 전달되는지 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(
            ShowLyricsTitleHeading: true, LyricsMonitorTitleHeadingAlignment: LyricsTextAlignment.Left);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Left);
    }

    [Fact]
    public void CreateScene_DefaultsTitleHeadingAlignmentToCenter()
    {
        // 기본 Center — 기존 헤딩 가운데 정렬 동작 보존.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Center);
    }

    [Fact]
    public void CreateScene_HidesTitleHeading_WhenSettingOff()
    {
        // 기본 off — 가사가 보여도 제목 헤딩은 숨김(기존 동작: 본문 송출 시 제목 숨김).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsTitleHeading: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsTitleHeading.Should().BeFalse("설정 off 면 제목 헤딩 숨김");
    }

    [Fact]
    public void CreateScene_HidesTitleHeading_WhenNoBody()
    {
        // 제목 헤딩은 가사 본문이 송출될 때만 의미 있다(본문 없으면 기존 중앙 제목이 담당).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsTitleHeading: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsTitleHeading.Should().BeFalse("본문이 없으면 제목 헤딩 숨김");
    }

    [Fact]
    public void CreateScene_DefaultsTitleHeadingOff()
    {
        // 기본값 off — 신규 설정이 기존 출력 모양을 바꾸지 않음을 고정.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.ShowLyricsTitleHeading.Should().BeFalse();
        scene.ShowsTitleHeading.Should().BeFalse();
    }

    [Fact]
    public void CreateScene_Threads_LyricsOutline_WhenLiveWithBody()
    {
        // 외곽선 효과(§7.3-A 폰트 효과): 설정 on + Live + 본문 → UsesBodyOutline true.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsOutline: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowLyricsOutline.Should().BeTrue();
        scene.UsesBodyOutline.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_DefaultsOutlineOff()
    {
        // 기본 off — 외곽선 신규 설정이 기존 출력 모양을 바꾸지 않음.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720));

        scene.ShowLyricsOutline.Should().BeFalse();
        scene.UsesBodyOutline.Should().BeFalse();
    }

    [Fact]
    public void CreateScene_HidesOutline_WhenNoBody()
    {
        // 외곽선은 가사 본문이 송출될 때만 의미 있다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsOutline: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "주보 PPT", "Display 1", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.UsesBodyOutline.Should().BeFalse("본문이 없으면 외곽선도 적용 안 함");
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
