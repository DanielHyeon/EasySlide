using System;
using Easislides.Wpf.Media;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Media;

public class MediaPlaybackViewModelTests
{
    [Fact]
    public void Load_UpdatesDisplayPropertiesAndCommandState()
    {
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);

        sut.Load(new MediaPlaybackRequest("intro.mp4", MediaSourceKind.File, TimeSpan.FromSeconds(125), "Video"));

        sut.Source.Should().Be("intro.mp4");
        sut.MediaType.Should().Be("Video");
        sut.DurationText.Should().Be("02:05");
        sut.StatusText.Should().Be("READY");
        sut.PlayPauseCommand.CanExecute(null).Should().BeTrue();
        sut.StopCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void PlayPauseCommand_TogglesBetweenPlayingAndPaused()
    {
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);
        sut.Load(DefaultRequest());

        sut.PlayPauseCommand.Execute(null);
        sut.State.Should().Be(MediaPlaybackState.Playing);
        sut.PlayPauseLabel.Should().Be("Pause");

        sut.PlayPauseCommand.Execute(null);
        sut.State.Should().Be(MediaPlaybackState.Paused);
        sut.PlayPauseLabel.Should().Be("Play");
    }

    [Fact]
    public void FastForwardAndReverseCommands_SeekByConfiguredIncrement()
    {
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);
        sut.Load(DefaultRequest(duration: TimeSpan.FromSeconds(20)));

        sut.FastForwardCommand.Execute(null);
        sut.Position.Should().Be(TimeSpan.FromSeconds(5));
        sut.FastReverseCommand.Execute(null);
        sut.Position.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ToggleCommands_UpdateMuteAndRepeat()
    {
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);
        sut.Load(DefaultRequest());

        sut.ToggleMuteCommand.Execute(null);
        sut.ToggleRepeatCommand.Execute(null);

        sut.IsMuted.Should().BeTrue();
        sut.IsRepeatEnabled.Should().BeTrue();
    }

    [Fact]
    public void RestartCommand_SeeksToStartAndPlays()
    {
        // 레거시 미디어 Enter 키 = 처음부터 다시 재생: 위치를 0으로 되감고 재생 상태가 된다.
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);
        sut.Load(DefaultRequest(duration: TimeSpan.FromSeconds(20)));
        sut.FastForwardCommand.Execute(null); // 위치 5초로 이동.
        sut.Position.Should().Be(TimeSpan.FromSeconds(5));

        sut.RestartCommand.Execute(null);

        sut.Position.Should().Be(TimeSpan.Zero, "처음으로 되감음");
        sut.State.Should().Be(MediaPlaybackState.Playing, "되감고 바로 재생");
    }

    [Fact]
    public void RestartCommand_DisabledWhenNoMedia()
    {
        // 미디어가 없으면(Empty) 다시재생도 비활성 — 다른 재생 컨트롤과 동일 게이팅.
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);

        sut.RestartCommand.CanExecute(null).Should().BeFalse("미디어가 없으면 다시재생 비활성");
    }

    [Fact]
    public void Unload_ResetsStateToEmptyAndDisablesControls()
    {
        var service = new MediaPlaybackService();
        var sut = new MediaPlaybackViewModel(service);
        sut.Load(DefaultRequest());
        sut.PlayPauseCommand.Execute(null);

        sut.Unload();

        sut.State.Should().Be(MediaPlaybackState.Empty);
        sut.Source.Should().BeEmpty();
        // 미디어 키 4종(Space·Esc/S·Enter·M)이 모두 비활성이어야 "미디어 없으면 Space=다음 항목" 넘김 보장이 성립한다.
        sut.PlayPauseCommand.CanExecute(null).Should().BeFalse("미디어를 내리면 재생 컨트롤이 비활성");
        sut.StopCommand.CanExecute(null).Should().BeFalse();
        sut.RestartCommand.CanExecute(null).Should().BeFalse();
        sut.ToggleMuteCommand.CanExecute(null).Should().BeFalse();
    }

    private static MediaPlaybackRequest DefaultRequest(TimeSpan? duration = null)
        => new("intro.mp4", MediaSourceKind.File, duration ?? TimeSpan.FromMinutes(3), "Video");
}
