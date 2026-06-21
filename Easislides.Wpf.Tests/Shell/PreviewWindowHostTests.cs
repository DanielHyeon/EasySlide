using System;
using System.Collections.Generic;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 증분160-B — 스테이지(Preview) 모니터 호스트. 출력 호스트(OutputWindowHostTests)와 같은 패턴(FakeSurface)이되,
/// 핵심 차이는 "회중 화면이 검정/비움이어도 스테이지는 라이브를 계속 보여 준다"는 점을 검증한다.
/// </summary>
public class PreviewWindowHostTests
{
    [Fact]
    public void OpenPreview_CreatesSurfaceAndAppliesPlacement()
    {
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));
        var display = new OutputDisplay("display-2", "Display 2", 1920, 0, 1920, 1080, 1.25);

        preview.Open(display, windowed: false);

        surfaces.Should().ContainSingle();
        surfaces[0].ShowCount.Should().Be(1);
        surfaces[0].Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1920, 1080, IsWindowed: false));
        surfaces[0].ViewModel.Should().NotBeNull();
        surfaces[0].ViewModel!.IsOutputOpen.Should().BeTrue();
        surfaces[0].ViewModel!.OutputMonitorName.Should().Be("Display 2");
    }

    [Fact]
    public void MovePreview_ReusesSurfaceAndReappliesPlacement()
    {
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));
        var display1 = new OutputDisplay("display-1", "Display 1", 0, 0, 1920, 1080, 1);
        var display2 = new OutputDisplay("display-2", "Display 2", 1920, 0, 1280, 720, 1);

        preview.Open(display1, windowed: true);
        preview.MoveTo(display2, windowed: false);

        surfaces.Should().ContainSingle();
        surfaces[0].Placement.Should().Be(new OutputWindowPlacement(1920, 0, 1280, 720, IsWindowed: false));
        surfaces[0].ViewModel!.OutputMonitorName.Should().Be("Display 2");
    }

    [Fact]
    public void StageIgnoresBlackout_KeepsShowingLiveItem()
    {
        // 핵심: 회중 화면을 검정(Black)으로 가려도 스테이지 모니터는 라이브 항목을 계속 보여 준다(State=Active 유지).
        // 같은 동작에서 출력(OutputWindowHostTests)은 IsBlackout=true·DisplayTitle="BLACK" 이지만, 스테이지는 그 반대.
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));

        preview.Open(OutputDisplay.PrimaryFallback, windowed: true);
        session.GoLive(new LiveQueueItem("song-1", "Amazing Grace"), "Primary");
        session.HideOutput(blackout: true);

        surfaces[0].ViewModel!.State.Should().Be(LiveState.Active, "스테이지는 검정 중에도 Active 로 가사를 계속 보여 줌");
        surfaces[0].ViewModel!.IsBlackout.Should().BeFalse();
        surfaces[0].ViewModel!.CurrentItemTitle.Should().Be("Amazing Grace", "라이브 항목이 그대로 살아 있어야 함");
    }

    [Fact]
    public void StageIgnoresClear_KeepsShowingLiveItem()
    {
        // 화면 비우기(Clear)도 마찬가지 — 스테이지는 라이브 항목을 계속 보여 준다.
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));

        preview.Open(OutputDisplay.PrimaryFallback, windowed: true);
        session.GoLive(new LiveQueueItem("song-1", "Amazing Grace"), "Primary");
        session.ClearOutput();

        surfaces[0].ViewModel!.State.Should().Be(LiveState.Active);
        surfaces[0].ViewModel!.CurrentItemTitle.Should().Be("Amazing Grace");
    }

    [Fact]
    public void ClosePreview_ClosesSurface_AndReopenCreatesNewSurface()
    {
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));

        preview.Open(OutputDisplay.PrimaryFallback, windowed: true);
        preview.Close();
        preview.Open(OutputDisplay.PrimaryFallback, windowed: true);

        surfaces.Should().HaveCount(2);
        surfaces[0].CloseCount.Should().Be(1);
        surfaces[1].ShowCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_UnsubscribesFromServices()
    {
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));

        sut.Dispose();
        preview.Open(OutputDisplay.PrimaryFallback, windowed: true);

        surfaces.Should().BeEmpty();
    }

    [Fact]
    public void OpenAfterBlackout_StageStillShowsLiveItem()
    {
        // 출력이 이미 검정(Black) 중일 때 스테이지 창을 "나중에" 열어도 리더는 가사를 본다.
        // 이 경로는 EnsureSurface 의 bind-시점 정규화(직전 세션을 정규화해 새 VM 에 적용)를 탄다 — 위 두 테스트와 다른 분기.
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));

        session.GoLive(new LiveQueueItem("song-1", "Amazing Grace"), "Primary");
        session.HideOutput(blackout: true);                          // 회중 화면 검정
        preview.Open(OutputDisplay.PrimaryFallback, windowed: true); // 그 후 스테이지 오픈

        surfaces[0].ViewModel!.State.Should().Be(LiveState.Active, "검정 중 나중에 열어도 스테이지는 라이브를 보여 줌");
        surfaces[0].ViewModel!.IsBlackout.Should().BeFalse();
        surfaces[0].ViewModel!.CurrentItemTitle.Should().Be("Amazing Grace");
    }

    [Fact]
    public void SessionChange_WhileClosed_DoesNotThrowOrCreateSurface()
    {
        // 스테이지 창이 닫혀 있을 때 라이브가 바뀌어도 NRE 없이 무시한다(_viewModel?. 가드) — 창은 만들어지지 않는다.
        var preview = new PreviewWindowService();
        var session = new LiveSessionService();
        var surfaces = new List<FakeOutputSurface>();
        using var sut = new PreviewWindowHost(preview, session, () => CreateSurface(surfaces));

        session.GoLive(new LiveQueueItem("song-1", "Amazing Grace"), "Primary"); // 창 닫힌 채 라이브
        session.HideOutput(blackout: true);

        surfaces.Should().BeEmpty("창이 닫혀 있으면 세션 변화로 스테이지 창이 생기지 않아야 함");
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
}
