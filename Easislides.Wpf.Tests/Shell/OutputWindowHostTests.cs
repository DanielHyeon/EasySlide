using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class OutputWindowHostTests
{
    [Fact]
    public void OpenOutput_CreatesSurfaceAndAppliesPlacement()
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new OutputWindowHost(output, session, () => CreateSurface(surfaces));
        var display = new OutputDisplay("display-2", "Display 2", 1920, 0, 1920, 1080, 1.25);

        output.Open(display, windowed: false);

        surfaces.Should().ContainSingle();
        surfaces[0].ShowCount.Should().Be(1);
        surfaces[0].Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false));
        surfaces[0].ViewModel.Should().NotBeNull();
        surfaces[0].ViewModel!.IsOutputOpen.Should().BeTrue();
        surfaces[0].ViewModel!.OutputMonitorName.Should().Be("Display 2");
    }

    [Fact]
    public void MoveOutput_ReusesSurfaceAndReappliesPlacement()
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new OutputWindowHost(output, session, () => CreateSurface(surfaces));
        var display1 = new OutputDisplay("display-1", "Display 1", 0, 0, 1920, 1080, 1);
        var display2 = new OutputDisplay("display-2", "Display 2", 1920, 0, 1280, 720, 1);

        output.Open(display1, windowed: true);
        output.MoveTo(display2, windowed: false);

        surfaces.Should().ContainSingle();
        surfaces[0].Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1280, 720, IsWindowed: false));
        surfaces[0].ViewModel!.OutputMonitorName.Should().Be("Display 2");
    }

    [Fact]
    public void LiveSessionChange_UpdatesBoundOutputViewModel()
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new OutputWindowHost(output, session, () => CreateSurface(surfaces));

        output.Open(OutputDisplay.PrimaryFallback, windowed: true);
        session.GoLive(new LiveQueueItem("song-1", "Amazing Grace"), "Primary");
        session.HideOutput(blackout: true);

        surfaces[0].ViewModel!.DisplayTitle.Should().Be("BLACK");
        surfaces[0].ViewModel!.StatusLabel.Should().Be("BLACKOUT");
        surfaces[0].ViewModel!.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public void OpenOutput_UsesSettingsBackedOutputViewModel()
    {
        using var settingsFolder = TempSettingsFolder.Create();
        var settings = settingsFolder.CreateSettings();
        settings.Set(EasiSettingKeys.ShowLyricsMonitorAlertBox, true).Succeeded.Should().BeTrue();
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new OutputWindowHost(
            output,
            session,
            () => CreateSurface(surfaces),
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings);

        output.Open(OutputDisplay.PrimaryFallback, windowed: true);

        surfaces[0].ViewModel!.LyricsAlertVisibility.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void CloseOutput_ClosesSurfaceAndReopenCreatesNewSurface()
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new OutputWindowHost(output, session, () => CreateSurface(surfaces));

        output.Open(OutputDisplay.PrimaryFallback, windowed: true);
        output.Close();
        output.Open(OutputDisplay.PrimaryFallback, windowed: true);

        surfaces.Should().HaveCount(2);
        surfaces[0].CloseCount.Should().Be(1);
        surfaces[1].ShowCount.Should().Be(1);
    }

    [Fact]
    public void ExternalSurfaceClose_MarksServiceClosedAndAllowsLiveReopen()
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new OutputWindowHost(output, session, () => CreateSurface(surfaces));

        output.Open(OutputDisplay.PrimaryFallback, windowed: false);
        surfaces[0].RaiseClosed();

        output.Current.IsOpen.Should().BeFalse("사용자가 출력 창을 직접 닫아도 host/service 상태가 닫힘으로 동기화되어야 한다");

        output.Open(OutputDisplay.PrimaryFallback, windowed: false);
        session.GoLive(new LiveQueueItem("song-1", "다시 송출"), OutputDisplay.PrimaryFallback.Name);

        surfaces.Should().HaveCount(2);
        surfaces[1].ShowCount.Should().Be(1);
        surfaces[1].ViewModel!.State.Should().Be(LiveState.Active);
        surfaces[1].ViewModel!.CurrentItemTitle.Should().Be("다시 송출");
    }

    [Fact]
    public void Dispose_UnsubscribesFromServices()
    {
        var output = new OutputWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        var sut = new OutputWindowHost(output, session, () => CreateSurface(surfaces));

        sut.Dispose();
        output.Open(OutputDisplay.PrimaryFallback, windowed: true);

        surfaces.Should().BeEmpty();
    }

    private static FakeOutputSurface CreateSurface(ICollection<FakeOutputSurface> surfaces)
    {
        var surface = new FakeOutputSurface();
        surfaces.Add(surface);
        return surface;
    }

    private sealed class FakeOutputSurface : IOutputSurface
    {
        public event EventHandler? Closed;

        public OutputWindowViewModel? ViewModel { get; private set; }
        public OutputWindowPlacement? Placement { get; private set; }
        public int ShowCount { get; private set; }
        public int CloseCount { get; private set; }

        public void Bind(OutputWindowViewModel viewModel) => ViewModel = viewModel;

        public void ApplyPlacement(OutputWindowPlacement placement) => Placement = placement;

        public void Show() => ShowCount++;

        public void Close() => CloseCount++;

        public void RaiseClosed() => Closed?.Invoke(this, EventArgs.Empty);
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
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_OutputHostSettings_{Guid.NewGuid():N}"));

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
