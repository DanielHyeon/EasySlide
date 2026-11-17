using System;
using System.IO;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Platform;

public class WindowPlacementServiceTests
{
    [Fact]
    public void CreateOutputPlacement_Fullscreen_UsesDisplayBounds()
    {
        var sut = new WindowPlacementService();
        var display = new OutputDisplay("left", "Left display", -1920, 120, 1920, 1080, 1.25);

        var placement = sut.CreateOutputPlacement(display, windowed: false);

        placement.Should().Be(new OutputWindowPlacement(-1920, 120, 1920, 1080, IsWindowed: false));
    }

    [Fact]
    public void CreateOutputPlacement_WithSettingsDefaults_UsesDisplayBounds()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var sut = new WindowPlacementService(settingsFolder.CreateSettings());
        var display = new OutputDisplay("left", "Left display", -1920, 120, 1920, 1080, 1.25);

        var placement = sut.CreateOutputPlacement(display, windowed: false);

        placement.Should().Be(new OutputWindowPlacement(-1920, 120, 1920, 1080, IsWindowed: false));
    }

    [Fact]
    public void CreateOutputPlacement_Fullscreen_UsesSettingsCustomBoundsWhenConfigured()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DisplayCustomTop, 120).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DisplayCustomLeft, -240).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DisplayCustomWidth, 1920).Succeeded.Should().BeTrue();
        var sut = new WindowPlacementService(settings);
        var display = new OutputDisplay("secondary", "Secondary", 1920, 0, 1920, 1080, 1.25);

        var placement = sut.CreateOutputPlacement(display, windowed: false);

        placement.Should().Be(new OutputWindowPlacement(-240, 120, 1920, 1440, IsWindowed: false));
    }

    [Fact]
    public void CreateOutputPlacement_Windowed_CentersPreviewInsideSettingsCustomBounds()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.DisplayCustomTop, 100).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DisplayCustomLeft, 200).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DisplayCustomWidth, 1600).Succeeded.Should().BeTrue();
        var sut = new WindowPlacementService(settings);
        var display = new OutputDisplay("secondary", "Secondary", 1920, 0, 1920, 1080, 1.25);

        var placement = sut.CreateOutputPlacement(display, windowed: true);

        placement.Width.Should().Be(1280);
        placement.Height.Should().Be(720);
        placement.Left.Should().Be(360);
        placement.Top.Should().Be(340);
        placement.IsWindowed.Should().BeTrue();
    }

    [Fact]
    public void CreateOutputPlacement_Windowed_CentersSixteenByNinePreviewInsideDisplay()
    {
        var sut = new WindowPlacementService();
        var display = new OutputDisplay("primary", "Primary", 0, 0, 1920, 1080, 1.0);

        var placement = sut.CreateOutputPlacement(display, windowed: true);

        placement.Width.Should().Be(1280);
        placement.Height.Should().Be(720);
        placement.Left.Should().Be(320);
        placement.Top.Should().Be(180);
        placement.IsWindowed.Should().BeTrue();
    }

    [Fact]
    public void CreateOutputPlacement_Windowed_ShrinksByHeightWhenDisplayIsShort()
    {
        var sut = new WindowPlacementService();
        var display = new OutputDisplay("short", "Short display", 100, 50, 800, 400, 1.0);

        var placement = sut.CreateOutputPlacement(display, windowed: true);

        placement.Width.Should().BeApproximately(711.11, 0.01);
        placement.Height.Should().Be(400);
        placement.Left.Should().BeApproximately(144.44, 0.01);
        placement.Top.Should().Be(50);
        placement.IsWindowed.Should().BeTrue();
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
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_WindowPlacementSettings_{Guid.NewGuid():N}"));

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
