using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
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
    public void ApplySession_ActiveSong_RendersBodyTextAndHidesTitle()
    {
        // 곡 가사가 본문으로 송출되면 출력 중앙에 가사가 보이고, 타이틀은 겹침 방지로 숨겨진다.
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "은혜로다",
            "Display 2",
            IsBlackout: false,
            CurrentItemBodyText: "1절 가사\n둘째 줄"));

        sut.BodyText.Should().Be("1절 가사\n둘째 줄");
        sut.BodyTextVisibility.Should().Be(Visibility.Visible);
        sut.DisplayTitleVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_ActiveSongWithFontOverride_MapsBodyFontFamilyAndSize()
    {
        // 곡별 FormatData 폰트명/크기 오버라이드가 출력 본문 글꼴·크기에 반영된다(§7.3-A per-song 폰트).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사",
            OverrideFontName: "Batang",
            OverrideFontSizePx: 90));

        sut.BodyFontFamily.Should().BeOfType<FontFamily>()
            .Which.Source.Should().Be("Batang");
        sut.BodyFontSize.Should().Be(90);
    }

    [Fact]
    public void ApplySession_ActiveSongWithoutFontOverride_LeavesBodyFontFamilyInherited()
    {
        // 폰트 오버라이드가 없으면 BodyFontFamily 는 UnsetValue → XAML 이 테마 기본 글꼴을 상속(무회귀).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사"));

        sut.BodyFontFamily.Should().Be(DependencyProperty.UnsetValue);
    }

    [Fact]
    public void ApplySession_ActiveSongWithBackgroundImage_LoadsImageAndShowsItOverColor()
    {
        // 곡별 배경 이미지 경로가 실려 오면 로더로 이미지를 읽어 색 배경 위에 표시한다(이미지 우선).
        var stub = CreateStubBitmap();
        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings: null,
            (Func<string, ImageSource?>)(_ => stub));

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사",
            OverrideBackgroundImagePath: @"C:\bg\sky.jpg"));

        sut.SceneBackgroundImageSource.Should().BeSameAs(stub);
        sut.BackgroundImageVisibility.Should().Be(Visibility.Visible);
    }

    [Theory]
    [InlineData(LyricsBackgroundMode.Fill, Stretch.UniformToFill)]
    [InlineData(LyricsBackgroundMode.Fit, Stretch.Uniform)]
    [InlineData(LyricsBackgroundMode.Center, Stretch.None)]
    [InlineData(LyricsBackgroundMode.Tile, Stretch.None)]
    public void BuildBackgroundImageBrush_SetsStretchPerMode(LyricsBackgroundMode mode, Stretch expectedStretch)
    {
        var image = CreateStubBitmap();

        var brush = OutputWindowViewModel.BuildBackgroundImageBrush(image, mode);

        brush.Stretch.Should().Be(expectedStretch);
        brush.ImageSource.Should().BeSameAs(image);
        brush.IsFrozen.Should().BeTrue("렌더 스레드 공유·성능을 위해 Freeze");
    }

    [Fact]
    public void BuildBackgroundImageBrush_Tile_UsesTileModeAndAbsoluteViewport()
    {
        var image = CreateStubBitmap(4, 3);

        var brush = OutputWindowViewModel.BuildBackgroundImageBrush(image, LyricsBackgroundMode.Tile);

        brush.TileMode.Should().Be(TileMode.Tile);
        brush.ViewportUnits.Should().Be(BrushMappingMode.Absolute);
        // 타일 한 칸은 원본 픽셀 크기(96dpi 스텁이라 PixelWidth==Width).
        brush.Viewport.Should().Be(new System.Windows.Rect(0, 0, image.PixelWidth, image.PixelHeight));
    }

    [Fact]
    public void BuildBackgroundImageBrush_Center_CentersWithoutStretch()
    {
        var brush = OutputWindowViewModel.BuildBackgroundImageBrush(CreateStubBitmap(), LyricsBackgroundMode.Center);

        brush.Stretch.Should().Be(Stretch.None);
        brush.AlignmentX.Should().Be(AlignmentX.Center);
        brush.AlignmentY.Should().Be(AlignmentY.Center);
    }

    [Fact]
    public void ApplySession_BackgroundImageWithTileMode_BuildsTiledBrush()
    {
        // 씬의 표시 모드(Tile)가 배경 브러시에 반영된다(설정→씬→VM 브러시 경로 통합 확인).
        var stub = CreateStubBitmap(4, 4);
        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings: null,
            (Func<string, ImageSource?>)(_ => stub));

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사",
            OverrideBackgroundImagePath: @"C:\bg\pattern.png"));

        // 기본 씬 모드는 Fill(설정 기본) — Tile 을 직접 검증하려면 BuildBackgroundImageBrush 단위 테스트가 담당.
        sut.BackgroundImageBrush.Should().BeOfType<ImageBrush>();
        ((ImageBrush)sut.BackgroundImageBrush!).Stretch.Should().Be(Stretch.UniformToFill, "기본 모드 Fill");
    }

    [Fact]
    public void ApplySession_ActiveSongWithoutBackgroundImage_HidesBackgroundImage()
    {
        // 배경 이미지가 없으면 색 배경만 보인다(이미지 숨김 — 무회귀).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사"));

        sut.SceneBackgroundImageSource.Should().BeNull();
        sut.BackgroundImageVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_BackgroundImageLoaderReturnsNull_FallsBackToColorBackground()
    {
        // 경로가 있어도 로더가 null(파일 없음/디코드 실패)이면 색 배경으로 안전 폴백(이미지 숨김).
        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings: null,
            (Func<string, ImageSource?>)(_ => null));

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사",
            OverrideBackgroundImagePath: @"C:\bg\missing.jpg"));

        sut.SceneBackgroundImageSource.Should().BeNull();
        sut.BackgroundImageVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_ActiveDualLanguage_ShowsRegion2Body()
    {
        // 이중 언어([region 2]) 곡 라이브 → Region2 본문(BodyText2)과 표시가 켜진다(영역별 동시 송출).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "Amazing grace",
            CurrentItemBodyText2: "놀라운 은혜"));

        sut.BodyText.Should().Be("Amazing grace");
        sut.BodyText2.Should().Be("놀라운 은혜");
        sut.BodyText2Visibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ApplySession_SingleLanguage_HidesRegion2Body()
    {
        // 단일 영역 곡은 Region2 가 비어 Collapsed(무회귀).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "Amazing grace"));

        sut.BodyText2.Should().BeEmpty();
        sut.BodyText2Visibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_ItemNumberPresentButSettingOff_HidesItemNumber()
    {
        // 곡 번호가 있어도 설정(기본 off)이면 표시 안 함 — 라벨은 채워질 수 있으나 Collapsed(무회귀 계약).
        var sut = new OutputWindowViewModel(); // 설정 null → ShowItemNumber 기본 off

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절", CurrentItemNumber: 123));

        sut.ItemNumberVisibility.Should().Be(Visibility.Collapsed, "설정 off 면 곡 번호 숨김");
    }

    [Fact]
    public void ApplySession_CopyrightPresentButSettingOff_HidesCopyright()
    {
        // 저작권이 있어도 설정(기본 off)이면 표시 안 함 — Collapsed(무회귀 계약).
        var sut = new OutputWindowViewModel(); // 설정 null → ShowCopyright 기본 off

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절", CurrentItemCopyright: "CCLI 12345"));

        sut.CopyrightVisibility.Should().Be(Visibility.Collapsed, "설정 off 면 저작권 숨김");
    }

    [Fact]
    public void ApplySession_ActiveWithoutBody_ShowsTitleNotBody()
    {
        // 가사가 없으면 본문은 숨고 기존처럼 타이틀이 보인다(PPT/미디어/공지 등).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "주보 PPT", "Display 2", IsBlackout: false));

        sut.BodyText.Should().BeEmpty();
        sut.BodyTextVisibility.Should().Be(Visibility.Collapsed);
        sut.DisplayTitleVisibility.Should().Be(Visibility.Visible);
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
    public void ApplySession_Cleared_ShowsBackgroundWithoutBlackOverlayOrContent()
    {
        // 비우기(Cleared): 배경 브러시는 그대로 보이고(검정 오버레이 없음), 본문·타이틀·패널은 감춘다.
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "은혜로다",
            "Display 2",
            IsBlackout: false,
            CurrentItemBodyText: "1절 가사")
        {
            IsCleared = true,
        });

        sut.Scene.Kind.Should().Be(OutputSceneKind.Cleared);
        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Collapsed, "비우기는 배경을 보여야 하므로 검정 오버레이 없음");
        sut.BodyTextVisibility.Should().Be(Visibility.Collapsed);
        sut.PanelOverlayVisibility.Should().Be(Visibility.Collapsed);
        sut.DisplayTitleVisibility.Should().Be(Visibility.Collapsed);
        sut.IsBlackout.Should().BeFalse();
    }

    [Fact]
    public void ApplySession_Hidden_StillUsesBlackOverlay()
    {
        // 숨김(Hide, Cleared 아님)은 기존대로 검정 오버레이로 덮는다(안전).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "은혜로다",
            "Display 2",
            IsBlackout: false));

        sut.Scene.Kind.Should().Be(OutputSceneKind.Hidden);
        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Visible);
    }

    [Theory]
    [InlineData(LyricsTextAlignment.Left, TextAlignment.Left, HorizontalAlignment.Left)]
    [InlineData(LyricsTextAlignment.Center, TextAlignment.Center, HorizontalAlignment.Center)]
    [InlineData(LyricsTextAlignment.Right, TextAlignment.Right, HorizontalAlignment.Right)]
    public void ApplySession_MapsLyricsAlignmentToWpfAlignments(
        LyricsTextAlignment alignment, TextAlignment expectedText, HorizontalAlignment expectedHorizontal)
    {
        // 인-셸 가사 정렬: 설정 enum → 출력 본문 TextBlock 의 TextAlignment + HorizontalAlignment 매핑.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorTextAlignment, alignment).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사"));

        sut.BodyTextAlignment.Should().Be(expectedText);
        sut.BodyHorizontalAlignment.Should().Be(expectedHorizontal);
    }

    [Fact]
    public void Constructor_FadeTransitionOn_UsesConfiguredDuration()
    {
        // 전환 페이드 on + 250ms 설정이면 ContentFadeDuration 이 그 길이로 적용된다(FrmMain 전환 효과).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorUseFadeTransition, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, 250).Succeeded.Should().BeTrue();

        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ContentFadeDuration.Should().Be(TimeSpan.FromMilliseconds(250));
    }

    [Fact]
    public void Constructor_FadeTransitionOff_DisablesFade()
    {
        // 페이드 off 면 ContentFadeDuration 0 → 즉시 컷(애니메이션 없음).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorUseFadeTransition, false).Succeeded.Should().BeTrue();

        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ContentFadeDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ApplySession_RaisesSceneChanging_BeforeSceneChanged()
    {
        // 2-레이어 전환의 전제: 새 콘텐츠가 적용되기 전에 SceneChanging 이 먼저 발화해 뷰가 옛 프레임을 스냅샷할 수 있어야 한다.
        var sut = new OutputWindowViewModel();
        var order = new System.Collections.Generic.List<string>();
        sut.SceneChanging += (_, _) => order.Add("changing");
        sut.SceneChanged += (_, _) => order.Add("changed");

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        order.Should().Contain("changing").And.Contain("changed");
        order.IndexOf("changing").Should().BeLessThan(order.IndexOf("changed"), "옛 프레임 스냅샷이 새 적용보다 먼저");
    }

    [Fact]
    public void Constructor_TransitionKind_ReflectsSetting()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorTransitionKind, LyricsTransitionKind.SlideFromBottom).Succeeded.Should().BeTrue();

        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ContentTransitionKind.Should().Be(LyricsTransitionKind.SlideFromBottom);
    }

    [Fact]
    public void SettingsChanged_TransitionKind_LiveUpdatesContentTransitionKind()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        settings.Set(EasiSettingKeys.LyricsMonitorTransitionKind, LyricsTransitionKind.SlideFromLeft).Succeeded.Should().BeTrue();

        sut.ContentTransitionKind.Should().Be(LyricsTransitionKind.SlideFromLeft);
    }

    [Fact]
    public void SettingsChanged_TransitionDuration_LiveUpdatesContentFadeDuration()
    {
        // 전환 길이 설정 변경이 라이브로 ContentFadeDuration 에 즉시 반영된다.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorUseFadeTransition, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, 250).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        settings.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, 500).Succeeded.Should().BeTrue();

        sut.ContentFadeDuration.Should().Be(TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void ApplySession_DefaultLyricsAlignment_IsCenter()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절 가사"));

        sut.BodyTextAlignment.Should().Be(TextAlignment.Center);
        sut.BodyHorizontalAlignment.Should().Be(HorizontalAlignment.Center);
        sut.BodyVerticalAlignment.Should().Be(VerticalAlignment.Center);
    }

    [Theory]
    [InlineData(LyricsVerticalAlignment.Top, VerticalAlignment.Top)]
    [InlineData(LyricsVerticalAlignment.Center, VerticalAlignment.Center)]
    [InlineData(LyricsVerticalAlignment.Bottom, VerticalAlignment.Bottom)]
    public void ApplySession_MapsLyricsVerticalAlignmentToWpf(
        LyricsVerticalAlignment alignment, VerticalAlignment expected)
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorVerticalAlignment, alignment).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.BodyVerticalAlignment.Should().Be(expected);
    }

    [Fact]
    public void ApplySession_DefaultFontEffects_PreserveCurrentLook()
    {
        // 기본: 굵게 off→SemiBold(기존), 기울임 off→Normal, 그림자 off.
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        sut.BodyFontWeight.Should().Be(FontWeights.SemiBold);
        sut.BodyFontStyle.Should().Be(FontStyles.Normal);
        sut.BodyHasShadow.Should().BeFalse();
    }

    [Fact]
    public void ApplySession_BoldItalicShadowOn_MapsToWpf()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorBold, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorItalic, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorShadow, true).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        sut.BodyFontWeight.Should().Be(FontWeights.Bold);
        sut.BodyFontStyle.Should().Be(FontStyles.Italic);
        sut.BodyHasShadow.Should().BeTrue();
    }

    [Fact]
    public void ApplySession_OutlineOff_ShowsPlainBodyNotOutline()
    {
        // 기본 off → 일반 본문 렌더러만 보이고 외곽선 렌더러는 숨김(기존 동작 보존).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.BodyHasOutline.Should().BeFalse();
        sut.BodyTextVisibility.Should().Be(Visibility.Visible, "외곽선 off 면 일반 본문 표시");
        sut.BodyOutlineVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_OutlineOn_ShowsOutlineBodyNotPlain()
    {
        // 외곽선 on → 외곽선 렌더러만 보이고 일반 본문은 숨김(상호배타, 겹침 방지).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorOutline, true).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.BodyHasOutline.Should().BeTrue();
        sut.BodyTextVisibility.Should().Be(Visibility.Collapsed, "외곽선 on 이면 일반 본문은 숨김");
        sut.BodyOutlineVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void SettingsChanged_RefreshesOutline()
    {
        // 라이브 중 외곽선 토글이 즉시 두 렌더러 가시성에 반영(SettingsChanged 화이트리스트).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));
        sut.BodyOutlineVisibility.Should().Be(Visibility.Collapsed);

        settings.Set(EasiSettingKeys.LyricsMonitorOutline, true).Succeeded.Should().BeTrue();

        sut.BodyHasOutline.Should().BeTrue();
        sut.BodyOutlineVisibility.Should().Be(Visibility.Visible);
        sut.BodyTextVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_TitleHeadingOn_ShowsTitleAboveBody()
    {
        // 제목 헤딩 설정 on → 가사 본문 송출 중에도 제목을 상단 배너로 노출(본문도 함께 보임).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, true).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.DisplayTitle.Should().Be("은혜로다");
        sut.TitleHeadingVisibility.Should().Be(Visibility.Visible);
        sut.BodyTextVisibility.Should().Be(Visibility.Visible, "헤딩과 본문은 함께 보인다");
    }

    [Theory]
    [InlineData(LyricsTextAlignment.Left, TextAlignment.Left, HorizontalAlignment.Left)]
    [InlineData(LyricsTextAlignment.Center, TextAlignment.Center, HorizontalAlignment.Center)]
    [InlineData(LyricsTextAlignment.Right, TextAlignment.Right, HorizontalAlignment.Right)]
    public void ApplySession_MapsTitleHeadingAlignment(
        LyricsTextAlignment alignment, TextAlignment expectedText, HorizontalAlignment expectedHorizontal)
    {
        // 제목 헤딩 정렬(§7.3-A): 설정 enum → 헤딩 TextBlock 의 TextAlignment + HorizontalAlignment 매핑.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment, alignment).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.TitleHeadingTextAlignment.Should().Be(expectedText);
        sut.TitleHeadingHorizontalAlignment.Should().Be(expectedHorizontal);
    }

    [Fact]
    public void ApplySession_DefaultTitleHeadingAlignment_IsCenter()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, true).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.TitleHeadingTextAlignment.Should().Be(TextAlignment.Center, "기본 가운데");
        sut.TitleHeadingHorizontalAlignment.Should().Be(HorizontalAlignment.Center);
    }

    [Fact]
    public void ApplySession_TitleHeadingOff_HidesHeading()
    {
        // 기본 off → 헤딩 숨김(기존 동작: 본문 송출 시 중앙 제목도 숨김).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.TitleHeadingVisibility.Should().Be(Visibility.Collapsed);
        sut.DisplayTitleVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ApplySession_TitleHeadingOn_ReservesBodyTopMargin_PreventingOverlap()
    {
        // code-review MAJOR 반영: 본문 세로정렬이 "위"여도 헤딩과 겹치지 않도록 본문 상단 여백을 확보.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorVerticalAlignment, LyricsVerticalAlignment.Top).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.TitleHeadingVisibility.Should().Be(Visibility.Visible);
        sut.BodyVerticalAlignment.Should().Be(VerticalAlignment.Top);
        sut.BodyContentMargin.Top.Should().BeGreaterThan(0, "헤딩이 보이면 본문 상단 여백을 확보해 겹침 방지");
    }

    [Fact]
    public void ApplySession_TitleHeadingOff_NoBodyTopMargin()
    {
        // 헤딩 off(기본) → 본문 상단 여백 0(기존 레이아웃 보존).
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절 가사"));

        sut.TitleHeadingVisibility.Should().Be(Visibility.Collapsed);
        sut.BodyContentMargin.Top.Should().Be(0, "헤딩이 없으면 본문 여백을 추가하지 않음");
    }

    [Fact]
    public void SettingsChanged_RefreshesTitleHeading()
    {
        // 라이브 중 설정 토글이 즉시 헤딩 가시성에 반영(SettingsChanged 화이트리스트).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));
        sut.TitleHeadingVisibility.Should().Be(Visibility.Collapsed);

        settings.Set(EasiSettingKeys.LyricsMonitorShowTitleHeading, true).Succeeded.Should().BeTrue();

        sut.TitleHeadingVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void SettingsChanged_RefreshesFontEffects()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        settings.Set(EasiSettingKeys.LyricsMonitorShadow, true).Succeeded.Should().BeTrue();

        sut.BodyHasShadow.Should().BeTrue();
    }

    [Fact]
    public void ApplySession_DefaultFontSize_Is48WithProportionalLineHeight()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        sut.BodyFontSize.Should().Be(48);
        sut.BodyLineHeight.Should().Be(60, "기본 48px → 줄높이 1.25배 = 60");
    }

    [Fact]
    public void ApplySession_MapsFontSizeAndProportionalLineHeight()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 80).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        sut.BodyFontSize.Should().Be(80);
        sut.BodyLineHeight.Should().Be(100, "80px → 1.25배 = 100");
    }

    [Fact]
    public void ApplySession_LineSpacingSetting_DrivesLineHeight()
    {
        // 줄 간격 150% + 폰트 48 → 줄높이 72. 줄 간격이 줄높이를 결정.
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 48).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, 150).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        sut.BodyLineHeight.Should().Be(72, "48 × 150% = 72");
    }

    [Fact]
    public void SettingsChanged_RefreshesLineSpacing()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 40).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, 200).Succeeded.Should().BeTrue();

        sut.BodyLineHeight.Should().Be(80, "40 × 200% = 80");
    }

    [Fact]
    public void SettingsChanged_RefreshesLyricsFontSize()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        settings.Set(EasiSettingKeys.LyricsMonitorFontSize, 36).Succeeded.Should().BeTrue();

        sut.BodyFontSize.Should().Be(36);
    }

    [Fact]
    public void SettingsChanged_RefreshesLyricsVerticalAlignment()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        settings.Set(EasiSettingKeys.LyricsMonitorVerticalAlignment, LyricsVerticalAlignment.Bottom).Succeeded.Should().BeTrue();

        sut.BodyVerticalAlignment.Should().Be(VerticalAlignment.Bottom);
    }

    [Fact]
    public void SettingsChanged_RefreshesLyricsAlignment()
    {
        // 인-셸 인스펙터가 정렬 설정을 바꾸면 라이브 출력에 즉시 반영(SettingsChanged 화이트리스트).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false, CurrentItemBodyText: "1절"));

        settings.Set(EasiSettingKeys.LyricsMonitorTextAlignment, LyricsTextAlignment.Left).Succeeded.Should().BeTrue();

        sut.BodyTextAlignment.Should().Be(TextAlignment.Left);
        sut.BodyHorizontalAlignment.Should().Be(HorizontalAlignment.Left);
    }

    [Fact]
    public void SettingsChanged_RefreshesItemNumberVisibility()
    {
        // 회귀 가드: Display Panel 곡 번호 토글이 EasiSettingKeys.All 에 등록돼 있어야
        // SettingsChanged 가 발화하고 라이브 출력이 즉시 갱신된다(All 누락 시 다음 GoLive 까지 지연됐던 버그).
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "은혜로다", "Display 2", IsBlackout: false,
            CurrentItemBodyText: "1절", CurrentItemNumber: 123));
        sut.ItemNumberVisibility.Should().Be(Visibility.Collapsed, "기본 off");

        settings.Set(EasiSettingKeys.LyricsMonitorShowItemNumber, true).Succeeded.Should().BeTrue();

        sut.ItemNumberVisibility.Should().Be(Visibility.Visible, "토글 on 이 라이브로 즉시 반영");
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

    [Fact]
    public void PanelBackgroundBrush_DefaultsToSemiTransparentBlack_AndBecomesTransparentWhenPanelTransparentOn()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(LiveState.Active, "Amazing Grace", "Display 2", IsBlackout: false));
        ((SolidColorBrush)sut.PanelBackgroundBrush).Color.Should().Be(Color.FromArgb(0x66, 0x00, 0x00, 0x00), "기본 반투명 검정");

        settings.Set(EasiSettingKeys.LyricsMonitorPanelTransparent, true).Succeeded.Should().BeTrue();
        sut.ApplySession(new LiveSessionSnapshot(LiveState.Active, "Amazing Grace", "Display 2", IsBlackout: false));
        ((SolidColorBrush)sut.PanelBackgroundBrush).Color.Should().Be(Colors.Transparent, "패널 투명 on 이면 Transparent");
    }

    [Fact]
    public void ApplySession_WithSettingsBackedLyricsMonitorAppearance_UpdatesBrushesAndVisibility()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.ShowLyricsMonitorAlertBox, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFF102030)).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFFE0D0C0)).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        ((SolidColorBrush)sut.SceneForegroundBrush).Color.Should().Be(Color.FromArgb(0xFF, 0x10, 0x20, 0x30));
        ((SolidColorBrush)sut.SceneBackgroundBrush).Color.Should().Be(Color.FromArgb(0xFF, 0xE0, 0xD0, 0xC0));
        sut.LyricsAlertVisibility.Should().Be(Visibility.Visible);
        sut.Scene.ShowsLyricsAlertBox.Should().BeTrue();
    }

    [Fact]
    public void SettingsChanged_RefreshesSettingsBackedLyricsMonitorAppearance()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFF445566)).Succeeded.Should().BeTrue();

        ((SolidColorBrush)sut.SceneForegroundBrush).Color.Should().Be(Color.FromArgb(0xFF, 0x44, 0x55, 0x66));
        sut.Scene.LyricsMonitorTextColorArgb.Should().Be(unchecked((int)0xFF445566));
    }

    [Fact]
    public void SettingsChanged_RefreshesPanelOverlayVisibility()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.NoMediaPanelOverlay, true).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Media Item",
            "Display 2",
            IsBlackout: false,
            CurrentItemKind: "M"));

        sut.PanelOverlayVisibility.Should().Be(Visibility.Collapsed);
        sut.Scene.ShowsPanelOverlay.Should().BeFalse();

        settings.Set(EasiSettingKeys.NoMediaPanelOverlay, false).Succeeded.Should().BeTrue();

        sut.PanelOverlayVisibility.Should().Be(Visibility.Visible);
        sut.Scene.ShowsPanelOverlay.Should().BeTrue();
    }

    [Fact]
    public void ReadyState_WithGapUserModeAndLoadableLogo_ShowsLogoAndHidesTitle()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.User).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.GapItemLogoFile, "logo.png").Succeeded.Should().BeTrue();
        var loaderCalls = new List<string>();
        var stubImage = CreateStubBitmap();
        ImageSource? Loader(string path)
        {
            loaderCalls.Add(path);
            return stubImage;
        }

        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings,
            (Func<string, ImageSource?>)Loader);
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("display-2", "Display 2", 1920, 0, 1920, 1080, 1),
            new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false)));

        sut.Scene.Kind.Should().Be(OutputSceneKind.Ready);
        sut.GapLogoSource.Should().BeSameAs(stubImage);
        sut.GapLogoVisibility.Should().Be(Visibility.Visible);
        sut.DisplayTitleVisibility.Should().Be(Visibility.Collapsed);
        loaderCalls.Should().ContainSingle().Which.Should().Be("logo.png");
    }

    [Fact]
    public void ReadyState_WithGapUserModeButLoaderReturnsNull_FallsBackToTitleText()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.User).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.GapItemLogoFile, "missing.png").Succeeded.Should().BeTrue();

        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings,
            (Func<string, ImageSource?>)(_ => null));
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("display-2", "Display 2", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: true)));

        sut.GapLogoSource.Should().BeNull();
        sut.GapLogoVisibility.Should().Be(Visibility.Collapsed);
        sut.DisplayTitleVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ReadyState_WithGapDefaultMode_DoesNotInvokeLoader()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.Default).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.GapItemLogoFile, "logo.png").Succeeded.Should().BeTrue();
        var loaderCalls = 0;

        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings,
            (Func<string, ImageSource?>)(_ => { loaderCalls++; return CreateStubBitmap(); }));
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1280, 720, 1),
            new OutputWindowPlacement(0, 0, 1280, 720, IsWindowed: true)));

        loaderCalls.Should().Be(0);
        sut.GapLogoVisibility.Should().Be(Visibility.Collapsed);
        sut.DisplayTitle.Should().Be("OUTPUT READY");
        sut.DisplayTitleVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void LiveState_WithGapUserMode_DoesNotShowGapLogo()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.User).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.GapItemLogoFile, "logo.png").Succeeded.Should().BeTrue();

        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings,
            (Func<string, ImageSource?>)(_ => CreateStubBitmap()));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        sut.Scene.Kind.Should().Be(OutputSceneKind.Live);
        sut.GapLogoVisibility.Should().Be(Visibility.Collapsed);
        sut.DisplayTitle.Should().Be("Amazing Grace");
    }

    [Fact]
    public void GapLogo_RepeatedScenesWithSamePath_OnlyInvokeLoaderOnce()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.User).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.GapItemLogoFile, "logo.png").Succeeded.Should().BeTrue();
        var loaderCalls = 0;
        var stubImage = CreateStubBitmap();

        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings,
            (Func<string, ImageSource?>)(_ => { loaderCalls++; return stubImage; }));
        var openState = new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1280, 720, 1),
            new OutputWindowPlacement(0, 0, 1280, 720, IsWindowed: true));

        sut.ApplyOutput(openState);
        sut.ApplyOutput(openState);
        sut.ApplyOutput(openState);

        loaderCalls.Should().Be(1);
    }

    [Fact]
    public void Blackout_ShowsBlackoutOverlay()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "Amazing Grace",
            "Display 2",
            IsBlackout: true));

        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Hidden_ShowsBlackoutOverlay()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        sut.Scene.Kind.Should().Be(OutputSceneKind.Hidden);
        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Live_DoesNotShowBlackoutOverlay()
    {
        var sut = new OutputWindowViewModel();

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Standby_DoesNotShowBlackoutOverlay()
    {
        var sut = new OutputWindowViewModel();

        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Blackout_ThenLive_HidesBlackoutOverlay()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "Amazing Grace",
            "Display 2",
            IsBlackout: true));
        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Visible);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void SceneChanged_FiresOnApplySession()
    {
        var sut = new OutputWindowViewModel();
        var fireCount = 0;
        sut.SceneChanged += (_, _) => fireCount++;

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        fireCount.Should().Be(1);
    }

    [Fact]
    public void SceneChanged_FiresOnApplyOutput()
    {
        var sut = new OutputWindowViewModel();
        var fireCount = 0;
        sut.SceneChanged += (_, _) => fireCount++;

        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1280, 720, 1),
            new OutputWindowPlacement(0, 0, 1280, 720, IsWindowed: true)));

        fireCount.Should().Be(1);
    }

    [Fact]
    public void ContentFadeDuration_DefaultIs250Ms()
    {
        var sut = new OutputWindowViewModel();

        sut.ContentFadeDuration.Should().Be(TimeSpan.FromMilliseconds(250));
    }

    [Fact]
    public void ContentFadeDuration_CanBeOverridden()
    {
        var sut = new OutputWindowViewModel
        {
            ContentFadeDuration = TimeSpan.Zero
        };

        sut.ContentFadeDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void SetContentAsset_WithImageAndLiveSession_ShowsContentWithFitPlacement()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide 1",
            "Display 2",
            IsBlackout: false));

        sut.SetContentAsset(CreateStubBitmap(), pixelWidth: 1920, pixelHeight: 1080);

        sut.ContentVisibility.Should().Be(Visibility.Visible);
        sut.ContentImageSource.Should().NotBeNull();
        sut.ContentWidth.Should().Be(1920);
        sut.ContentHeight.Should().Be(1080);
    }

    [Fact]
    public void SetContentAsset_WithImageButNotLive_KeepsContentHidden()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1280, 720, 1),
            new OutputWindowPlacement(0, 0, 1280, 720, IsWindowed: true)));

        sut.SetContentAsset(CreateStubBitmap(), pixelWidth: 1280, pixelHeight: 720);

        sut.Scene.Kind.Should().Be(OutputSceneKind.Ready);
        sut.ContentVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void SetContentAsset_NullSource_HidesContentEvenWhenLive()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide 1",
            "Display 2",
            IsBlackout: false));

        sut.SetContentAsset(source: null, pixelWidth: 0, pixelHeight: 0);

        sut.ContentImageSource.Should().BeNull();
        sut.ContentVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void SetContentAsset_FitMode_CentersImageWithLetterbox()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide 1",
            "Display 2",
            IsBlackout: false));

        // 4:3 이미지를 16:9 뷰포트에 Fit → 좌우 레터박스가 생긴다.
        sut.SetContentAsset(CreateStubBitmap(), pixelWidth: 1600, pixelHeight: 1200, fillMode: ImageFillMode.Fit);

        sut.ContentWidth.Should().Be(1440);
        sut.ContentHeight.Should().Be(1080);
        sut.ContentLeft.Should().Be(240);
        sut.ContentTop.Should().Be(0);
    }

    [Fact]
    public void ApplySession_LiveWithPreviewSource_PropagatesToContentImage()
    {
        var preview = CreateStubBitmap(width: 1920, height: 1080);
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide 3",
            "Display 2",
            IsBlackout: false,
            CurrentItemKind: "P",
            CurrentItemPreviewSource: preview,
            CurrentItemPreviewFillMode: ImageFillMode.Fit,
            CurrentItemPreviewPixelWidth: 1920,
            CurrentItemPreviewPixelHeight: 1080));

        sut.ContentImageSource.Should().BeSameAs(preview);
        sut.ContentVisibility.Should().Be(Visibility.Visible);
        sut.ContentWidth.Should().Be(1920);
        sut.ContentHeight.Should().Be(1080);
    }

    [Fact]
    public void ApplySession_BodyTextSuppressesContentImage()
    {
        // 본문(가사)이 있으면 콘텐츠 이미지는 숨긴다(본문 우선 — 가사와 이미지 겹침 방지).
        var preview = CreateStubBitmap(width: 1920, height: 1080);
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "은혜로다",
            "Display 2",
            IsBlackout: false,
            CurrentItemPreviewSource: preview,
            CurrentItemPreviewPixelWidth: 1920,
            CurrentItemPreviewPixelHeight: 1080,
            CurrentItemBodyText: "1절 가사"));

        sut.BodyTextVisibility.Should().Be(Visibility.Visible);
        sut.ContentVisibility.Should().Be(Visibility.Collapsed, "본문이 보이면 이미지는 숨긴다");
    }

    [Fact]
    public void ApplySession_StopClearsContentImage()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide",
            "Display 2",
            IsBlackout: false,
            CurrentItemPreviewSource: CreateStubBitmap(),
            CurrentItemPreviewPixelWidth: 100,
            CurrentItemPreviewPixelHeight: 100));
        sut.ContentImageSource.Should().NotBeNull();

        sut.ApplySession(LiveSessionSnapshot.Off);

        sut.ContentImageSource.Should().BeNull();
        sut.ContentVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void LiveContentImage_DuringBlackout_BecomesHiddenButPreservesSource()
    {
        var preview = CreateStubBitmap(1920, 1080);
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide 1",
            "Display 2",
            IsBlackout: false,
            CurrentItemPreviewSource: preview,
            CurrentItemPreviewPixelWidth: 1920,
            CurrentItemPreviewPixelHeight: 1080));
        sut.ContentVisibility.Should().Be(Visibility.Visible);

        // LiveSessionService.HideOutput처럼 preview는 보존하면서 상태만 Blackout으로 전이.
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "Slide 1",
            "Display 2",
            IsBlackout: true,
            CurrentItemPreviewSource: preview,
            CurrentItemPreviewPixelWidth: 1920,
            CurrentItemPreviewPixelHeight: 1080));

        sut.ContentImageSource.Should().BeSameAs(preview);
        sut.ContentVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void LiveContentImage_RestoreFromHidden_ShowsSamePreservedContent()
    {
        // §7.3-B 복귀: 숨김/블랙에서 Active 로 되돌리면(LiveSessionService.Restore 가 콘텐츠 보존한 채 상태만 전이)
        // 직전 콘텐츠가 그대로 다시 표시돼야 한다(블랙아웃 오버레이 걷힘 + ContentVisibility 복원).
        var preview = CreateStubBitmap(1920, 1080);
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "Slide 1", "Display 2", IsBlackout: false,
            CurrentItemPreviewSource: preview, CurrentItemPreviewPixelWidth: 1920, CurrentItemPreviewPixelHeight: 1080));
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden, "Slide 1", "Display 2", IsBlackout: true,
            CurrentItemPreviewSource: preview, CurrentItemPreviewPixelWidth: 1920, CurrentItemPreviewPixelHeight: 1080));
        sut.ContentVisibility.Should().Be(Visibility.Collapsed);

        // Restore: 콘텐츠 보존한 채 Active + blackout 해제(Restore() 산출 스냅샷과 동일 형태).
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active, "Slide 1", "Display 2", IsBlackout: false,
            CurrentItemPreviewSource: preview, CurrentItemPreviewPixelWidth: 1920, CurrentItemPreviewPixelHeight: 1080));

        sut.ContentImageSource.Should().BeSameAs(preview, "복귀 시 직전 콘텐츠 보존");
        sut.ContentVisibility.Should().Be(Visibility.Visible, "복귀 시 콘텐츠 다시 표시");
        sut.BlackoutOverlayVisibility.Should().Be(Visibility.Collapsed, "복귀 시 블랙아웃 오버레이 걷힘");
    }

    private static BitmapSource CreateStubBitmap(int width = 1, int height = 1)
    {
        var stride = width * 4;
        var pixels = new byte[stride * height];
        for (var i = 3; i < pixels.Length; i += 4)
        {
            pixels[i] = 255;
        }
        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, palette: null, pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }

    [Fact]
    public void SceneBackgroundBrush_Is_Solid_By_Default()
    {
        // G2: 기본 설정은 배경색1==배경색2(-1) → 솔리드(그라데이션 미적용, 회귀 없음).
        var sut = new OutputWindowViewModel();

        sut.SceneBackgroundBrush.Should().BeOfType<SolidColorBrush>("두 배경색이 같으면 솔리드");
    }

    [Fact]
    public void SceneBackgroundBrush_Is_Gradient_When_Background_Colors_Differ()
    {
        // G2(FrmBackground 슬라이스): 그라데이션 사용 ON + 배경색1≠배경색2 면 세로 그라데이션 브러시로 송출.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFF112233));
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, unchecked((int)0xFF445566));
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundIsGradient, true);

        var renderer = new OutputRenderer(new ImageAssetService(), new TransitionEffectService());
        var sut = new OutputWindowViewModel(renderer, settings);
        sut.ApplySession(new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false));

        // 타입뿐 아니라 색 순서(배경색→끝색)와 세로 방향(위→아래)까지 고정(회귀 방지).
        var brush = sut.SceneBackgroundBrush.Should().BeOfType<LinearGradientBrush>("배경색이 다르면 세로 그라데이션").Subject;
        brush.StartPoint.Should().Be(new Point(0.5, 0), "세로 그라데이션 시작은 위 중앙");
        brush.EndPoint.Should().Be(new Point(0.5, 1), "세로 그라데이션 끝은 아래 중앙");
        brush.GradientStops.Should().HaveCount(2);
        brush.GradientStops[0].Color.Should().Be(Color.FromArgb(0xFF, 0x11, 0x22, 0x33), "시작색=배경색1");
        brush.GradientStops[1].Color.Should().Be(Color.FromArgb(0xFF, 0x44, 0x55, 0x66), "끝색=배경색2");
    }

    [Fact]
    public void SettingsChanged_LiveGradientApply_UpdatesBackgroundImmediately()
    {
        // 인-셸 출력 모양 인스펙터 경로(code-review CRITICAL): 이미 구독 중인 VM 에 배경색1/끝색/그라데이션을
        // 한 키씩 Set 하면(마지막이 IsGradient) 라이브 배경이 즉시 세로 그라데이션으로 갱신돼야 한다.
        // (끝색·그라데이션 키가 ContainsLiveOutputSetting 화이트리스트에 없어 마지막 Set 이 갱신을 못 받던 버그.)
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = new OutputWindowViewModel(
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);
        sut.ApplySession(new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false));

        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFF112233));
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, unchecked((int)0xFF445566));
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundIsGradient, true);

        sut.SceneBackgroundBrush.Should().BeOfType<LinearGradientBrush>(
            "끝색·그라데이션 키도 라이브 갱신을 트리거해야 즉시 그라데이션으로 송출");
    }

    [Fact]
    public void SceneBackgroundBrush_Is_Solid_When_Gradient_Disabled_Even_If_Colors_Differ()
    {
        // opt-in 시맨틱: 그라데이션 OFF(기본)면 배경색2가 달라도 솔리드(기존 단색 설정 회귀 방지).
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFF112233));
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, unchecked((int)0xFF445566));
        // LyricsMonitorBackgroundIsGradient 미설정 → 기본 false

        var renderer = new OutputRenderer(new ImageAssetService(), new TransitionEffectService());
        var sut = new OutputWindowViewModel(renderer, settings);
        sut.ApplySession(new LiveSessionSnapshot(LiveState.Active, "Test", "Display 1", IsBlackout: false));

        sut.SceneBackgroundBrush.Should().BeOfType<SolidColorBrush>("그라데이션 OFF면 색이 달라도 솔리드");
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_OutputSettings_{Guid.NewGuid():N}"));

        public ISettingsService CreateSettings()
            => new SettingsService(new SettingsServiceOptions(
                Path.Combine(Root, "settings.json"),
                Path.Combine(Root, "Backups")));

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
