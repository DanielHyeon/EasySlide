using System;
using Easislides.Wpf.Media;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Media;

public class MediaPlaybackServiceTests
{
    [Fact]
    public void Load_CreatesReadySnapshotAndClampsAudioSettings()
    {
        var sut = new MediaPlaybackService();
        var request = new MediaPlaybackRequest(
            "intro.mp4",
            MediaSourceKind.File,
            TimeSpan.FromMinutes(3),
            "Video",
            Volume: 150,
            Balance: -150,
            IsMuted: true,
            IsRepeatEnabled: true,
            IsWidescreen: true,
            OutputDisplayId: "secondary");

        sut.Load(request);

        sut.Current.State.Should().Be(MediaPlaybackState.Ready);
        sut.Current.Source.Should().Be("intro.mp4");
        sut.Current.Duration.Should().Be(TimeSpan.FromMinutes(3));
        sut.Current.Volume.Should().Be(100);
        sut.Current.Balance.Should().Be(-100);
        sut.Current.IsMuted.Should().BeTrue();
        sut.Current.IsRepeatEnabled.Should().BeTrue();
        sut.Current.OutputDisplayId.Should().Be("secondary");
    }

    [Fact]
    public void PlayPauseStop_UpdatesStateAndResetsPositionOnStop()
    {
        var sut = new MediaPlaybackService();
        sut.Load(DefaultRequest());

        sut.Play();
        sut.Seek(TimeSpan.FromSeconds(30));
        sut.Pause();
        sut.Stop();

        sut.Current.State.Should().Be(MediaPlaybackState.Stopped);
        sut.Current.Position.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Seek_ClampsBetweenZeroAndDuration()
    {
        var sut = new MediaPlaybackService();
        sut.Load(DefaultRequest(duration: TimeSpan.FromSeconds(60)));

        sut.Seek(TimeSpan.FromSeconds(90));
        sut.Current.Position.Should().Be(TimeSpan.FromSeconds(60));

        sut.Seek(TimeSpan.FromSeconds(-5));
        sut.Current.Position.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Settings_UpdateSnapshotWithoutChangingPlaybackState()
    {
        var sut = new MediaPlaybackService();
        sut.Load(DefaultRequest());
        sut.Play();

        sut.SetVolume(-10);
        sut.SetBalance(125);
        sut.SetMuted(true);
        sut.SetRepeatEnabled(true);

        sut.Current.State.Should().Be(MediaPlaybackState.Playing);
        sut.Current.Volume.Should().Be(0);
        sut.Current.Balance.Should().Be(100);
        sut.Current.IsMuted.Should().BeTrue();
        sut.Current.IsRepeatEnabled.Should().BeTrue();
    }

    private static MediaPlaybackRequest DefaultRequest(TimeSpan? duration = null)
        => new("intro.mp4", MediaSourceKind.File, duration ?? TimeSpan.FromMinutes(3), "Video");
}
