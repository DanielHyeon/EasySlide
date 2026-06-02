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
            LyricsMonitorBackgroundColorArgb: unchecked((int)0xFFABCDEF));

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Amazing Grace", "Display 2", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsLyricsAlertBox.Should().BeTrue();
        scene.LyricsMonitorTextColorArgb.Should().Be(unchecked((int)0xFF123456));
        scene.LyricsMonitorBackgroundColorArgb.Should().Be(unchecked((int)0xFFABCDEF));
    }

    [Fact]
    public void CreateScene_TitleHeadingFollowBody_UsesBodyAlignmentForHeading()
    {
        // FrmMain Def_HeadAlign AsR1 — 본문 정렬 따름 on 이면 헤딩이 본문(Region1) 정렬을 쓴다(헤딩 전용 정렬 무시).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextAlignment: LyricsTextAlignment.Left,            // 본문은 왼쪽
            LyricsMonitorTitleHeadingAlignment: LyricsTextAlignment.Right,   // 헤딩 전용은 오른쪽
            LyricsMonitorTitleHeadingFollowBody: true);                       // 따름 on → 본문(왼쪽) 사용

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Left, "따름 on 이면 헤딩이 본문(왼쪽) 정렬을 사용");
    }

    [Fact]
    public void CreateScene_TitleHeadingFollowBodyOff_UsesOwnHeadingAlignment()
    {
        // 기본(off) — 헤딩은 자기 전용 정렬(오른쪽)을 그대로 쓴다(무회귀).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextAlignment: LyricsTextAlignment.Left,
            LyricsMonitorTitleHeadingAlignment: LyricsTextAlignment.Right,
            LyricsMonitorTitleHeadingFollowBody: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Right, "따름 off 이면 헤딩 전용 정렬(오른쪽) 유지");
    }

    [Fact]
    public void CreateScene_TitleHeadingFollowBody_FollowsPerSongOverrideAlignment()
    {
        // 가장 미묘한 경로 — 곡별 정렬(OverrideTextAlignment)이 있으면 본문은 그 곡 정렬을 쓰므로,
        // 헤딩도 "본문 따름" on 일 때 전역 본문 설정이 아닌 그 곡의 정렬(가운데)을 따라야 한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextAlignment: LyricsTextAlignment.Left,            // 전역 본문은 왼쪽
            LyricsMonitorTitleHeadingAlignment: LyricsTextAlignment.Right,   // 헤딩 전용은 오른쪽
            LyricsMonitorTitleHeadingFollowBody: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideTextAlignment: LyricsTextAlignment.Center),         // 곡별 정렬 = 가운데
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Center, "헤딩이 곡별 본문 정렬(가운데)을 따른다(전역 왼쪽 아님)");
    }

    [Fact]
    public void CreateScene_TitleHeadingFollowRegion2_UsesRegion2Alignment()
    {
        // FrmMain Def_HeadAlign AsR2 — 보조 영역 따름 on 이면 헤딩이 Region2 정렬(곡별 region2 정렬 32 포함)을 쓴다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextAlignment: LyricsTextAlignment.Left,            // 본문(Region1)은 왼쪽
            LyricsMonitorTitleHeadingAlignment: LyricsTextAlignment.Center,  // 헤딩 전용은 가운데
            LyricsMonitorTitleHeadingFollowRegion2: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                OverrideTextAlignment2: LyricsTextAlignment.Right),         // 곡별 Region2 정렬 = 오른쪽
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Right, "AsR2 on 이면 헤딩이 Region2 정렬(오른쪽)을 사용");
    }

    [Fact]
    public void CreateScene_TitleHeadingFollowRegion2_TakesPrecedenceOverFollowBody()
    {
        // 우선순위 AsR2 > AsR1 — 둘 다 on 이면 Region2 정렬이 이긴다.
        // Region2 정렬은 곡별 override(32) 가 있을 때만 Region1 과 달라지므로, 구별을 위해 override 를 준다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(
            LyricsMonitorTextAlignment: LyricsTextAlignment.Left,            // Region1 왼쪽
            LyricsMonitorTitleHeadingAlignment: LyricsTextAlignment.Center,
            LyricsMonitorTitleHeadingFollowBody: true,                       // AsR1 on
            LyricsMonitorTitleHeadingFollowRegion2: true);                   // AsR2 on → 우선

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                OverrideTextAlignment: LyricsTextAlignment.Left,            // 곡별 Region1 = 왼쪽
                OverrideTextAlignment2: LyricsTextAlignment.Right),         // 곡별 Region2 = 오른쪽
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorTitleHeadingAlignment.Should().Be(LyricsTextAlignment.Right, "AsR2 가 AsR1 보다 우선 — Region2(오른쪽) 사용");
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
    public void CreateScene_Active_ShowItemNumber_ShowsSongNumberWhenLiveAndEnabled()
    {
        // 설정 on + Live + 곡 번호>0 일 때만 곡 번호를 노출(Display Panel).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowItemNumber: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemNumber: 123),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ItemNumberLabel.Should().Be("123");
        scene.ShowsItemNumber.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_ShowItemNumberOff_HidesItemNumber()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowItemNumber: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemNumber: 123),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsItemNumber.Should().BeFalse("설정 off 면 곡 번호 미표시(무회귀)");
    }

    [Fact]
    public void CreateScene_Active_ShowCopyright_ShowsCopyrightWhenLiveAndEnabled()
    {
        // 설정 on + Live + 저작권 문자열이 있을 때만 저작권을 노출(Display Panel).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowCopyright: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemCopyright: "CCLI 12345"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.CopyrightLabel.Should().Be("CCLI 12345");
        scene.ShowsCopyright.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_ShowCopyrightOff_HidesCopyright()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowCopyright: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemCopyright: "CCLI 12345"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsCopyright.Should().BeFalse("설정 off 면 저작권 미표시(무회귀)");
    }

    [Fact]
    public void CreateScene_Active_ShowNextItem_ShowsNextTitleWhenLiveAndEnabled()
    {
        // 설정 on + Live + 다음 항목 제목이 있을 때만 다음 항목을 노출(Display Panel PrevNext).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowNextItem: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemNextTitle: "주 은혜임을"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.NextItemLabel.Should().Be("주 은혜임을");
        scene.ShowsNextItem.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_ShowNextItemOff_HidesNextItem()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowNextItem: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentItemNextTitle: "주 은혜임을"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.ShowsNextItem.Should().BeFalse("설정 off 면 다음 항목 미표시(무회귀)");
    }

    [Fact]
    public void CreateScene_Active_Region2Font_FallsBackToRegion1()
    {
        // region2 글꼴 오버라이드가 없으면 Region1 글꼴(이름·크기)을 추종한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 50);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideFontName: "Batang", OverrideFontSizePx: 90),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontFamily2.Should().Be("Batang", "region2 글꼴 미지정 → region1 글꼴 추종");
        scene.LyricsMonitorFontSize2.Should().Be(90, "region2 크기 미지정 → region1 크기 추종");
    }

    [Fact]
    public void CreateScene_Active_Region2Font_UsesOverrideWhenPresent()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideFontName: "Batang", OverrideFontSizePx: 90,
                OverrideFontName2: "Gulim", OverrideFontSizePx2: 60),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.LyricsMonitorFontFamily2.Should().Be("Gulim", "region2 글꼴 오버라이드 우선");
        scene.LyricsMonitorFontSize2.Should().Be(60);
    }

    [Fact]
    public void CreateScene_Region2GlobalFontSize_AppliesWhenSetAndNoSongOverride()
    {
        // FrmMain Ind_Reg2SizeUpDown — 전역 region2 크기(설정>0)가 있으면 Region2 본문에 적용(곡별 override 없을 때).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 80, LyricsMonitorFontSize2: 40);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontSize.Should().Be(80, "Region1 은 전역 크기");
        scene.LyricsMonitorFontSize2.Should().Be(40, "Region2 는 전역 region2 크기");
    }

    [Fact]
    public void CreateScene_Region2GlobalFontSize_Zero_FollowsRegion1()
    {
        // 전역 region2 크기 0(기본) = 본문(Region1) 크기 추종 — 무회귀.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 80, LyricsMonitorFontSize2: 0);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontSize2.Should().Be(80, "전역 region2 크기 0 → Region1(80) 추종");
    }

    [Fact]
    public void CreateScene_Region2GlobalFontSize_SongOverrideTakesPrecedence()
    {
        // 우선순위: 곡별 region2 크기(48) > 전역 region2 크기 > Region1.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontSize: 80, LyricsMonitorFontSize2: 40);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideFontSizePx2: 60),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontSize2.Should().Be(60, "곡별 region2 크기가 전역(40)을 이긴다");
    }

    [Fact]
    public void CreateScene_Active_Region2Alignment_FallsBackToRegion1WhenNoOverride()
    {
        // region2 정렬 오버라이드가 없으면 Region2 정렬은 Region1 정렬을 추종한다(이중 언어 정렬 일관성).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideTextAlignment: LyricsTextAlignment.Left),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.LyricsMonitorTextAlignment2.Should().Be(LyricsTextAlignment.Left, "region2 정렬 미지정 → region1 정렬 추종");
    }

    [Fact]
    public void CreateScene_Active_Region2Alignment_UsesOverrideWhenPresent()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideTextAlignment: LyricsTextAlignment.Left,
                OverrideTextAlignment2: LyricsTextAlignment.Right),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.LyricsMonitorTextAlignment2.Should().Be(LyricsTextAlignment.Right, "region2 정렬 오버라이드 우선");
    }

    [Fact]
    public void CreateScene_Active_Region1Bold_OverridesGlobalSetting()
    {
        // 곡별 region1 굵게 비트가 켜져 있으면 전역 설정(off)을 덮어쓴다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBold: false, LyricsMonitorItalic: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideBold1: true, OverrideItalic1: true),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorBold.Should().BeTrue("곡별 region1 굵게가 전역을 덮어씀");
        scene.LyricsMonitorItalic.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_Active_Region1Bold_FollowsGlobalWhenNoOverride()
    {
        // region1 비트가 없으면 전역 설정(굵게 on)을 그대로 따른다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBold: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorBold.Should().BeTrue("전역 굵게 추종");
    }

    [Fact]
    public void CreateScene_Active_Region2Bold_UsesOverrideWhenPresent()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideBold2: true),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.LyricsMonitorBold2.Should().BeTrue("곡별 region2 굵게 오버라이드");
    }

    [Fact]
    public void CreateScene_Active_Region2Bold_FollowsRegion1WhenNoOverride()
    {
        // region2 비트가 없으면 Region1 효과(여기선 region1 굵게 오버라이드)를 추종한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideBold1: true),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.LyricsMonitorBold2.Should().BeTrue("region2 미지정 → region1 굵게 추종");
    }

    [Fact]
    public void CreateScene_EmphasisChorusOnly_NonChorusPage_SuppressesEmphasis()
    {
        // 강조 후렴만 on + 현재 절이 후렴 아님 → 전역 굵게가 켜져 있어도 이 절은 강조 끔.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBold: true, LyricsMonitorEmphasisChorusOnly: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentPageIsChorus: false),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.LyricsMonitorBold.Should().BeFalse("후렴 아닌 절은 강조 끔");
    }

    [Fact]
    public void CreateScene_EmphasisChorusOnly_ChorusPage_KeepsEmphasis()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBold: true, LyricsMonitorEmphasisChorusOnly: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "후렴", CurrentPageIsChorus: true),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.LyricsMonitorBold.Should().BeTrue("후렴 절은 강조 유지");
    }

    [Fact]
    public void CreateScene_EmphasisChorusOnlyOff_AlwaysAppliesEmphasis()
    {
        // 기본(off)이면 후렴 아닌 절에서도 전역 강조를 그대로 적용(무회귀).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBold: true, LyricsMonitorEmphasisChorusOnly: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentPageIsChorus: false),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.LyricsMonitorBold.Should().BeTrue("후렴만 off → 전체 절 강조");
    }

    [Fact]
    public void CreateScene_Interlace_BothRegions_ShowsInterlace()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorInterlace: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "원문", CurrentItemBodyText2: "번역"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsInterlace.Should().BeTrue("인터레이스 on + 두 영역 본문 → 교차");
    }

    [Fact]
    public void CreateScene_Interlace_SingleLanguage_NoInterlace()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorInterlace: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "원문만"), // Region2 없음.
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsInterlace.Should().BeFalse("보조 언어 없음 → 교차할 게 없음");
    }

    [Fact]
    public void CreateScene_InterlaceOff_NoInterlace()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "원문", CurrentItemBodyText2: "번역"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.ShowsInterlace.Should().BeFalse("기본 off → 무회귀");
    }

    [Fact]
    public void CreateScene_RegionDisplayBoth_ShowsBothBands()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorRegionDisplay: LyricsRegionDisplay.Both);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsBodyText.Should().BeTrue();
        scene.ShowsBodyText2.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_RegionDisplayRegion1Only_HidesRegion2()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorRegionDisplay: LyricsRegionDisplay.Region1Only);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsBodyText.Should().BeTrue("Region1 은 표시");
        scene.ShowsBodyText2.Should().BeFalse("Region1만 → Region2 숨김");
    }

    [Fact]
    public void CreateScene_RegionDisplayRegion2Only_HidesRegion1WhenRegion2Present()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorRegionDisplay: LyricsRegionDisplay.Region2Only);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsBodyText.Should().BeFalse("Region2만 → Region1 숨김");
        scene.ShowsBodyText2.Should().BeTrue("Region2 표시");
    }

    [Fact]
    public void CreateScene_RegionDisplayRegion1Only_OnlyRegion2Body_StillShowsRegion2()
    {
        // 주 언어(Region1) 본문이 없고 보조 언어만 있는 드문 곡은 Region1만 모드여도 Region2 를 보여 화면이 비지 않게 한다
        // (ShowsBodyText 의 Region2Only 안전장치와 대칭 — 내용 있는 곡을 빈 화면으로 만들지 않음).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorRegionDisplay: LyricsRegionDisplay.Region1Only);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "", CurrentItemBodyText2: "은혜"), // Region1 본문 없음.
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsBodyText.Should().BeFalse("Region1 본문 없음");
        scene.ShowsBodyText2.Should().BeTrue("주 언어 없음 → Region2 유지(빈 화면 방지)");
    }

    [Fact]
    public void CreateScene_RegionDisplayRegion2Only_SingleLanguage_StillShowsRegion1()
    {
        // 보조 언어가 없는 단일 언어 곡은 Region2만 모드여도 Region1 을 보여 화면이 비지 않게 한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorRegionDisplay: LyricsRegionDisplay.Region2Only);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing"), // Region2 본문 없음.
            Output: output, ViewportWidth: 1280, ViewportHeight: 720, LiveOutputSettings: settings));

        scene.ShowsBodyText.Should().BeTrue("보조 언어 없음 → Region1 유지(빈 화면 방지)");
        scene.ShowsBodyText2.Should().BeFalse();
    }

    [Fact]
    public void CreateScene_Active_Region1Underline_OverridesGlobalSetting()
    {
        // 곡별 region1 밑줄 비트가 켜져 있으면 전역 밑줄(off)을 덮어쓴다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorUnderline: false);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideUnderline1: true),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorUnderline.Should().BeTrue("곡별 region1 밑줄이 전역을 덮어씀");
    }

    [Fact]
    public void CreateScene_Active_Region2Underline_UsesOverrideWhenPresent()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜",
                OverrideUnderline2: true),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720));

        scene.LyricsMonitorUnderline2.Should().BeTrue("곡별 region2 밑줄 오버라이드");
        scene.LyricsMonitorUnderline.Should().BeFalse("region1 은 밑줄 없음(전역 off)");
    }

    [Fact]
    public void CreateScene_Active_Region2Underline_FollowsRegion1WhenNoOverride()
    {
        // region2 밑줄 비트가 없으면 Region1 밑줄(여기선 전역 on)을 추종한다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorUnderline: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "Amazing", CurrentItemBodyText2: "은혜"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorUnderline2.Should().BeTrue("region2 미지정 → region1(전역 on) 추종");
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
    public void CreateScene_Active_NoSongFont_UsesGlobalFontFamily()
    {
        // 곡별 글꼴 오버라이드가 없으면 운영 전역 글꼴(설정)을 송출 글꼴로 쓴다(증분: 전역 출력 글꼴).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontFamily: "Malgun Gothic");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontFamily.Should().Be("Malgun Gothic", "곡 글꼴이 없으면 전역 출력 글꼴을 쓴다");
    }

    [Fact]
    public void CreateScene_Active_SongFont_BeatsGlobalFontFamily()
    {
        // 전역 글꼴이 있어도 곡별 글꼴(43)이 있으면 그 곡 동안은 곡별 글꼴이 우선한다(곡 우선순위 보존).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorFontFamily: "Malgun Gothic");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideFontName: "Batang"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorFontFamily.Should().Be("Batang", "곡별 글꼴이 전역 글꼴을 이긴다");
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
    public void CreateScene_Active_GlobalBackgroundImage_AppliedWhenNoSongOverride()
    {
        // 전역 배경 이미지(Images 탭 설정)가 있고 곡별 배경(61)이 없으면 전역 배경을 씬에 싣는다.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBackgroundImagePath: @"C:\bg\global.png");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.BackgroundImagePath.Should().Be(@"C:\bg\global.png", "곡별 배경이 없으면 전역 배경을 적용");
    }

    [Fact]
    public void CreateScene_Active_SongBackgroundImage_WinsOverGlobal()
    {
        // 곡별 배경(61)이 있으면 전역 배경보다 우선한다(그 곡 동안만).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBackgroundImagePath: @"C:\bg\global.png");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사",
                OverrideBackgroundImagePath: @"C:\bg\song.jpg"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.BackgroundImagePath.Should().Be(@"C:\bg\song.jpg", "곡별 배경이 전역 배경을 이긴다");
    }

    [Fact]
    public void CreateScene_NotLive_GlobalBackgroundImage_NotApplied()
    {
        // Live 가 아니면 전역 배경도 적용하지 않는다(대기/숨김 화면은 색 배경, 무회귀).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 2");
        var settings = new LiveOutputRenderSettings(LyricsMonitorBackgroundImagePath: @"C:\bg\global.png");

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(
                LiveState.Hidden, "은혜로다", "Display 2", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.BackgroundImagePath.Should().BeEmpty("Live 가 아니면 전역 배경 미적용");
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
        scene.LyricsMonitorUnderline.Should().BeFalse("밑줄 기본 off — 무회귀");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateScene_ThreadsLyricsUnderline(bool underline)
    {
        // 가사 밑줄 설정이 설정→렌더→scene 으로 전달되는지 고정(전역 효과).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(LyricsMonitorUnderline: underline);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.LyricsMonitorUnderline.Should().Be(underline);
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
    public void CreateScene_ShowsTitleOnPanel_WhenEnabledAndLiveWithTitle()
    {
        // FrmMain Def_PanelTitle — 설정 on + Live + 제목 존재 → ShowsTitleOnPanel true(제목은 DisplayTitle 재사용).
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(LyricsMonitorShowTitleOnPanel: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절 가사"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.DisplayTitle.Should().Be("은혜로다");
        scene.ShowsTitleOnPanel.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_HidesTitleOnPanel_WhenSettingOffOrNotLive()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        // 설정 off → 숨김
        var sceneOff = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: new LiveOutputRenderSettings(LyricsMonitorShowTitleOnPanel: false)));
        sceneOff.ShowsTitleOnPanel.Should().BeFalse("설정 off 면 숨김");

        // 설정 on 이지만 대기(Standby, 비Live) → 숨김
        var sceneStandby = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Standby, "은혜로다", "Display 1", IsBlackout: false),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: new LiveOutputRenderSettings(LyricsMonitorShowTitleOnPanel: true)));
        sceneStandby.ShowsTitleOnPanel.Should().BeFalse("Live 가 아니면 숨김");
    }

    [Fact]
    public void CreateScene_ShowsVerseHeading_WhenEnabledAndLiveWithSectionLabel()
    {
        // 절 헤딩(FrmMain Def_Head All): 설정 on + Live + 섹션 라벨 존재 → ShowsVerseHeading true, 라벨 전달.
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");
        var settings = new LiveOutputRenderSettings(ShowLyricsVerseHeading: true);

        var scene = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "후렴 가사", CurrentSectionLabel: "후렴"),
            Output: output,
            ViewportWidth: 1280,
            ViewportHeight: 720,
            LiveOutputSettings: settings));

        scene.VerseHeadingLabel.Should().Be("후렴");
        scene.ShowsVerseHeading.Should().BeTrue();
    }

    [Fact]
    public void CreateScene_HidesVerseHeading_WhenSettingOffOrNoLabel()
    {
        var sut = CreateRenderer();
        var output = OpenOutput("Display 1");

        // 설정 off → 라벨 있어도 숨김
        var sceneOff = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "후렴", CurrentSectionLabel: "후렴"),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: new LiveOutputRenderSettings(ShowLyricsVerseHeading: false)));
        sceneOff.ShowsVerseHeading.Should().BeFalse("설정 off 면 숨김");

        // 설정 on 이지만 섹션 라벨 없음(라벨 없는 절) → 숨김
        var sceneNoLabel = sut.CreateScene(new OutputRenderRequest(
            Session: new LiveSessionSnapshot(LiveState.Active, "은혜로다", "Display 1", IsBlackout: false,
                CurrentItemBodyText: "1절", CurrentSectionLabel: ""),
            Output: output, ViewportWidth: 1280, ViewportHeight: 720,
            LiveOutputSettings: new LiveOutputRenderSettings(ShowLyricsVerseHeading: true)));
        sceneNoLabel.ShowsVerseHeading.Should().BeFalse("섹션 라벨이 없으면 숨김");
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
