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

    [Fact]
    public void ApplySession_WithSettingsBackedLyricsMonitorAppearance_UpdatesBrushesAndVisibility()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.ShowLyricsMonitorAlertBox, true).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFF102030)).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFFE0D0C0)).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, false).Succeeded.Should().BeTrue();
        var sut = new OutputWindowViewModel(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Amazing Grace",
            "Display 2",
            IsBlackout: false));

        ((SolidColorBrush)sut.SceneForegroundBrush).Color.Should().Be(Color.FromArgb(0xFF, 0x10, 0x20, 0x30));
        ((SolidColorBrush)sut.SceneBackgroundBrush).Color.Should().Be(Color.FromArgb(0xFF, 0xE0, 0xD0, 0xC0));
        sut.LyricsAlertVisibility.Should().Be(Visibility.Visible);
        sut.NotationVisibility.Should().Be(Visibility.Collapsed);
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
    public void SetContentAsset_BlackoutThenLive_HidesContentDuringBlackout()
    {
        var sut = new OutputWindowViewModel();
        sut.ApplyOutput(new OutputWindowState(
            IsOpen: true,
            new OutputDisplay("d", "d", 0, 0, 1920, 1080, 1),
            new OutputWindowPlacement(0, 0, 1920, 1080, IsWindowed: false)));
        sut.SetContentAsset(CreateStubBitmap(), pixelWidth: 1920, pixelHeight: 1080);
        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Active,
            "Slide 1",
            "Display 2",
            IsBlackout: false));
        sut.ContentVisibility.Should().Be(Visibility.Visible);

        sut.ApplySession(new LiveSessionSnapshot(
            LiveState.Hidden,
            "Slide 1",
            "Display 2",
            IsBlackout: true));

        sut.ContentVisibility.Should().Be(Visibility.Collapsed);
    }

    private static BitmapSource CreateStubBitmap()
    {
        var pixels = new byte[] { 0, 0, 0, 255 };
        var bitmap = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, palette: null, pixels, stride: 4);
        bitmap.Freeze();
        return bitmap;
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
