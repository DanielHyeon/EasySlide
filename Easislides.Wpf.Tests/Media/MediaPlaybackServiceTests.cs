using System;
using System.Collections.Generic;
using System.IO;
using Easislides.Wpf.Media;
using Easislides.Wpf.Settings;
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
    public void Load_WithSettingsService_UsesPersistedMediaDefaults()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        settings.Set(EasiSettingKeys.MediaVolume, 0.35).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaBalance, -0.25).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaMuted, true).Succeeded.Should().BeTrue();
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend, settings);

        sut.Load(DefaultRequest());

        sut.Current.Volume.Should().Be(35);
        sut.Current.Balance.Should().Be(-25);
        sut.Current.IsMuted.Should().BeTrue();
        backend.LastLoaded!.Volume.Should().Be(35);
        backend.LastLoaded.Balance.Should().Be(-25);
        backend.LastLoaded.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void LoadFromMediaDirectory_WithSettingsService_UsesConfiguredMediaDirectory()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        var mediaDirectory = Path.Combine(fixture.Root, "Media Files");
        settings.Set(EasiSettingKeys.MediaDirectory, mediaDirectory).Succeeded.Should().BeTrue();
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend, settings);

        sut.LoadFromMediaDirectory("intro.mp4", TimeSpan.FromMinutes(2), "Video");

        sut.Current.Source.Should().Be(Path.Combine(mediaDirectory, "intro.mp4"));
        sut.Current.SourceKind.Should().Be(MediaSourceKind.File);
        backend.LastLoaded!.Source.Should().Be(Path.Combine(mediaDirectory, "intro.mp4"));
    }

    [Fact]
    public void LoadLiveCamera_WithSettingsService_UsesConfiguredCaptureDevice()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        settings.Set(EasiSettingKeys.LiveCameraNumber, 3).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaVolume, 0.65).Succeeded.Should().BeTrue();
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend, settings);

        sut.LoadLiveCamera(TimeSpan.Zero);

        sut.Current.Source.Should().Be("<<Capture>>3");
        sut.Current.SourceKind.Should().Be(MediaSourceKind.CaptureDevice);
        sut.Current.MediaType.Should().Be("Live Camera");
        sut.Current.Volume.Should().Be(65);
        backend.LastLoaded!.Source.Should().Be("<<Capture>>3");
    }

    [Fact]
    public void SettingsChanged_WhenMediaLoaded_AppliesPersistedMediaDefaults()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend, settings);
        sut.Load(DefaultRequest());

        settings.Set(EasiSettingKeys.MediaVolume, 0.4).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaBalance, 0.25).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.MediaMuted, true).Succeeded.Should().BeTrue();

        sut.Current.State.Should().Be(MediaPlaybackState.Ready);
        sut.Current.Volume.Should().Be(40);
        sut.Current.Balance.Should().Be(25);
        sut.Current.IsMuted.Should().BeTrue();
        backend.LastSettings!.Volume.Should().Be(40);
        backend.LastSettings.Balance.Should().Be(25);
        backend.LastSettings.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void SettingsChanged_WhenLiveCameraLoaded_UpdatesCaptureDeviceSource()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateService();
        settings.Set(EasiSettingKeys.LiveCameraNumber, 2).Succeeded.Should().BeTrue();
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend, settings);
        sut.LoadLiveCamera(TimeSpan.Zero);

        settings.Set(EasiSettingKeys.LiveCameraNumber, 4).Succeeded.Should().BeTrue();

        sut.Current.Source.Should().Be("<<Capture>>4");
        sut.Current.SourceKind.Should().Be(MediaSourceKind.CaptureDevice);
        backend.LastLoaded!.Source.Should().Be("<<Capture>>4");
        backend.Commands.Should().ContainInOrder("Load:<<Capture>>2", "Load:<<Capture>>4");
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

    [Fact]
    public void Commands_DelegateToBackendInOrder()
    {
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend);

        sut.Load(DefaultRequest(duration: TimeSpan.FromSeconds(60)));
        sut.Play();
        sut.Seek(TimeSpan.FromSeconds(30));
        sut.Pause();
        sut.Stop();

        backend.Commands.Should().Equal(
            "Load:intro.mp4",
            "Play",
            "Seek:00:00:30",
            "Pause",
            "Stop");
        sut.Current.State.Should().Be(MediaPlaybackState.Stopped);
        sut.Current.Position.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Settings_ApplyClampedSnapshotToBackend()
    {
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend);
        sut.Load(DefaultRequest());

        sut.SetVolume(150);
        sut.SetBalance(-150);
        sut.SetMuted(true);
        sut.SetRepeatEnabled(true);

        backend.LastSettings!.Volume.Should().Be(100);
        backend.LastSettings.Balance.Should().Be(-100);
        backend.LastSettings.IsMuted.Should().BeTrue();
        backend.LastSettings.IsRepeatEnabled.Should().BeTrue();
    }

    [Fact]
    public void Load_WhenBackendRejectsMedia_SetsFailedSnapshot()
    {
        var backend = new FakeMediaPlaybackBackend
        {
            LoadException = new MediaPlaybackException("Codec not supported")
        };
        var sut = new MediaPlaybackService(backend);

        sut.Load(DefaultRequest());

        sut.Current.State.Should().Be(MediaPlaybackState.Failed);
        sut.Current.Source.Should().Be("intro.mp4");
        sut.Current.ErrorMessage.Should().Be("Codec not supported");
    }

    [Fact]
    public void Play_WhenBackendFails_SetsFailedSnapshot()
    {
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend);
        sut.Load(DefaultRequest());
        backend.PlayException = new MediaPlaybackException("Renderer disconnected");

        sut.Play();

        sut.Current.State.Should().Be(MediaPlaybackState.Failed);
        sut.Current.ErrorMessage.Should().Be("Renderer disconnected");
    }

    [Fact]
    public void CommandFailures_SetFailedSnapshot()
    {
        AssertBackendFailure(
            (sut, backend) =>
            {
                sut.Play();
                backend.PauseException = new MediaPlaybackException("Pause failed");
                sut.Pause();
            },
            "Pause failed");
        AssertBackendFailure(
            (sut, backend) =>
            {
                backend.StopException = new MediaPlaybackException("Stop failed");
                sut.Stop();
            },
            "Stop failed");
        AssertBackendFailure(
            (sut, backend) =>
            {
                backend.SeekException = new MediaPlaybackException("Seek failed");
                sut.Seek(TimeSpan.FromSeconds(15));
            },
            "Seek failed");
        AssertBackendFailure(
            (sut, backend) =>
            {
                backend.SettingsException = new MediaPlaybackException("Settings failed");
                sut.SetMuted(true);
            },
            "Settings failed");
    }

    private static MediaPlaybackRequest DefaultRequest(TimeSpan? duration = null)
        => new("intro.mp4", MediaSourceKind.File, duration ?? TimeSpan.FromMinutes(3), "Video");

    private static void AssertBackendFailure(
        Action<MediaPlaybackService, FakeMediaPlaybackBackend> command,
        string expectedMessage)
    {
        var backend = new FakeMediaPlaybackBackend();
        var sut = new MediaPlaybackService(backend);
        sut.Load(DefaultRequest());

        command(sut, backend);

        sut.Current.State.Should().Be(MediaPlaybackState.Failed);
        sut.Current.ErrorMessage.Should().Be(expectedMessage);
    }

    private sealed class FakeMediaPlaybackBackend : IMediaPlaybackBackend
    {
        public List<string> Commands { get; } = [];
        public MediaPlaybackSnapshot? LastLoaded { get; private set; }
        public MediaPlaybackSnapshot? LastSettings { get; private set; }
        public Exception? LoadException { get; init; }
        public Exception? PlayException { get; set; }
        public Exception? PauseException { get; set; }
        public Exception? StopException { get; set; }
        public Exception? SeekException { get; set; }
        public Exception? SettingsException { get; set; }

        public void Load(MediaPlaybackSnapshot snapshot)
        {
            LastLoaded = snapshot;
            Commands.Add($"Load:{snapshot.Source}");
            if (LoadException is not null)
            {
                throw LoadException;
            }
        }

        public void Play()
        {
            Commands.Add("Play");
            if (PlayException is not null)
            {
                throw PlayException;
            }
        }

        public void Pause()
        {
            Commands.Add("Pause");
            if (PauseException is not null)
            {
                throw PauseException;
            }
        }

        public void Stop()
        {
            Commands.Add("Stop");
            if (StopException is not null)
            {
                throw StopException;
            }
        }

        public void Seek(TimeSpan position)
        {
            Commands.Add($"Seek:{position:c}");
            if (SeekException is not null)
            {
                throw SeekException;
            }
        }

        public void ApplySettings(MediaPlaybackSnapshot snapshot)
        {
            LastSettings = snapshot;
            Commands.Add($"Settings:{snapshot.Volume}:{snapshot.Balance}:{snapshot.IsMuted}:{snapshot.IsRepeatEnabled}");
            if (SettingsException is not null)
            {
                throw SettingsException;
            }
        }
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            SettingsPath = Path.Combine(root, "settings.json");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public string BackupRoot { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_MediaSettings_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(SettingsPath, BackupRoot));

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
