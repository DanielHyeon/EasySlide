using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Axicultus.Wpf.Platform;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Platform;

public class WindowPlacementServiceTests
{
    [Fact]
    public void ChangingAspectUpdatesTheCustomAudienceBoundsAtNegativeCoordinates()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.DisplayCustomLeft, -1600);
        settings.Set(AxicultusSettingKeys.DisplayCustomWidth, 1600);
        var service = new WindowPlacementService(settings);
        var display = new OutputDisplay("left", "Left", -1600, 0, 1600, 900, 1.5);
        service.CreateOutputPlacement(display, false).Height.Should().Be(1200);
        settings.Set(AxicultusSettingKeys.IsMonitorWide, true);
        service.CreateOutputPlacement(display, false).Should().Be(new OutputWindowPlacement(-1600, 0, 1600, 900, false));
        ((IWindowPlacementService)service).CreateStagePlacement(display, false).Height.Should().Be(900);
    }

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

        placement.Should().Be(new OutputWindowPlacement(-1680, 120, 1440, 1080, IsWindowed: false));
    }

    [Fact]
    public void CreateOutputPlacement_Fullscreen_UsesSettingsCustomBoundsWhenConfigured()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(AxicultusSettingKeys.DisplayCustomTop, 120).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.DisplayCustomLeft, -240).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.DisplayCustomWidth, 1920).Succeeded.Should().BeTrue();
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
        settings.Set(AxicultusSettingKeys.DisplayCustomTop, 100).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.DisplayCustomLeft, 200).Succeeded.Should().BeTrue();
        settings.Set(AxicultusSettingKeys.DisplayCustomWidth, 1600).Succeeded.Should().BeTrue();
        var sut = new WindowPlacementService(settings);
        var display = new OutputDisplay("secondary", "Secondary", 1920, 0, 1920, 1080, 1.25);

        var placement = sut.CreateOutputPlacement(display, windowed: true);

        placement.Width.Should().Be(1280);
        placement.Height.Should().Be(960);
        placement.Left.Should().Be(360);
        placement.Top.Should().Be(220);
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

    [Fact]
    public void NativeWindowUsesDesktopPixelBoundsBeforeAndAfterFirstShow()
    {
        StaHelper.RunOnSta(() =>
        {
            var window = new Window { Width = 1280, Height = 720, WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false, ShowInTaskbar = false, Opacity = 0 };
            try
            {
                var placement = new OutputWindowPlacement(120, 140, 320, 200, false);
                DesktopWindowPlacement.Apply(window, placement);
                window.Show();
                AssertNativePlacement(window, placement);
                var moved = new OutputWindowPlacement(-400, 50, 360, 220, false);
                DesktopWindowPlacement.Apply(window, moved);
                AssertNativePlacement(window, moved);
            }
            finally { window.Close(); }
        });
    }

    private static void AssertNativePlacement(Window window, OutputWindowPlacement placement)
    {
        GetWindowRect(new WindowInteropHelper(window).Handle, out var rect).Should().BeTrue();
        (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top)
            .Should().Be(((int)placement.Left, (int)placement.Top, (int)placement.Width, (int)placement.Height));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);

    [Fact]
    public void StageAndAudience_ResolveIndependentPhysicalRectangles()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.SetValues(new System.Collections.Generic.Dictionary<string,object> {
            ["operator.StageCustom"] = true, ["operator.StageLeft"] = -1200, ["operator.StageTop"] = 50,
            ["operator.StageWidth"] = 800, [AxicultusSettingKeys.IsMonitorWide.Id] = true,
            [AxicultusSettingKeys.OutputPlacementMode.Id] = 1, ["operator.OutputAsSingle"] = true });
        var placement = new WindowPlacementService(settings);
        var display = new OutputDisplay("stage", "side", -1920, 0, 1920, 1080, 1.5);
        placement.CreateStagePlacement(display, false).Should().Be(new OutputWindowPlacement(-1200,50,800,450,false));
        placement.CreateOutputPlacement(display, false).AsSingleMonitor.Should().BeTrue();
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
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_WindowPlacementSettings_{Guid.NewGuid():N}"));

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
