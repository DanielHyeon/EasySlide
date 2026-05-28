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

    private static MediaPlaybackRequest DefaultRequest(TimeSpan? duration = null)
        => new("intro.mp4", MediaSourceKind.File, duration ?? TimeSpan.FromMinutes(3), "Video");
}
