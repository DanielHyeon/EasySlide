using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
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
