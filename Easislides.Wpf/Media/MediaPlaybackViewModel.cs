using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Media;

public sealed partial class MediaPlaybackViewModel : ObservableObject, IDisposable
{
    private static readonly TimeSpan DefaultSeekIncrement = TimeSpan.FromSeconds(5);
    private readonly IMediaPlaybackService _playback;

    [ObservableProperty] private MediaPlaybackState _state = MediaPlaybackState.Empty;
    [ObservableProperty] private string _source = string.Empty;
    [ObservableProperty] private string _mediaType = string.Empty;
    [ObservableProperty] private TimeSpan _position = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _duration = TimeSpan.Zero;
    [ObservableProperty] private string _positionText = "00:00";
    [ObservableProperty] private string _durationText = "00:00";
    [ObservableProperty] private int _volume = 50;
    [ObservableProperty] private int _balance;
    [ObservableProperty] private bool _isMuted;
    [ObservableProperty] private bool _isRepeatEnabled;
    [ObservableProperty] private bool _isWidescreen = true;
    [ObservableProperty] private string _statusText = "NO MEDIA";
    [ObservableProperty] private string _playPauseLabel = "Play";

    public MediaPlaybackViewModel(IMediaPlaybackService playback)
    {
        _playback = playback;
        _playback.PlaybackChanged += OnPlaybackChanged;

        PlayPauseCommand = new RelayCommand(PlayPause, CanUsePlaybackControls);
        StopCommand = new RelayCommand(_playback.Stop, CanUsePlaybackControls);
        FastForwardCommand = new RelayCommand(() => _playback.Seek(Position + DefaultSeekIncrement), CanUsePlaybackControls);
        FastReverseCommand = new RelayCommand(() => _playback.Seek(Position - DefaultSeekIncrement), CanUsePlaybackControls);
        ToggleMuteCommand = new RelayCommand(() => _playback.SetMuted(!IsMuted), CanUsePlaybackControls);
        ToggleRepeatCommand = new RelayCommand(() => _playback.SetRepeatEnabled(!IsRepeatEnabled), CanUsePlaybackControls);

        ApplySnapshot(_playback.Current);
    }

    public IRelayCommand PlayPauseCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IRelayCommand FastForwardCommand { get; }
    public IRelayCommand FastReverseCommand { get; }
    public IRelayCommand ToggleMuteCommand { get; }
    public IRelayCommand ToggleRepeatCommand { get; }

    public void Load(MediaPlaybackRequest request) => _playback.Load(request);

    public void Dispose() => _playback.PlaybackChanged -= OnPlaybackChanged;

    private void OnPlaybackChanged(object? sender, MediaPlaybackChangedEventArgs e)
        => ApplySnapshot(e.Snapshot);

    private void PlayPause()
    {
        if (State == MediaPlaybackState.Playing)
        {
            _playback.Pause();
            return;
        }

        _playback.Play();
    }

    private bool CanUsePlaybackControls()
        => State is MediaPlaybackState.Ready
            or MediaPlaybackState.Playing
            or MediaPlaybackState.Paused
            or MediaPlaybackState.Stopped;

    private void ApplySnapshot(MediaPlaybackSnapshot snapshot)
    {
        State = snapshot.State;
        Source = snapshot.Source;
        MediaType = snapshot.MediaType;
        Position = snapshot.Position;
        Duration = snapshot.Duration;
        PositionText = FormatTime(snapshot.Position);
        DurationText = FormatTime(snapshot.Duration);
        Volume = snapshot.Volume;
        Balance = snapshot.Balance;
        IsMuted = snapshot.IsMuted;
        IsRepeatEnabled = snapshot.IsRepeatEnabled;
        IsWidescreen = snapshot.IsWidescreen;
        StatusText = CreateStatusText(snapshot);
        PlayPauseLabel = snapshot.State == MediaPlaybackState.Playing ? "Pause" : "Play";
        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        PlayPauseCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        FastForwardCommand.NotifyCanExecuteChanged();
        FastReverseCommand.NotifyCanExecuteChanged();
        ToggleMuteCommand.NotifyCanExecuteChanged();
        ToggleRepeatCommand.NotifyCanExecuteChanged();
    }

    private static string CreateStatusText(MediaPlaybackSnapshot snapshot)
        => snapshot.State == MediaPlaybackState.Failed && !string.IsNullOrWhiteSpace(snapshot.ErrorMessage)
            ? $"FAILED: {snapshot.ErrorMessage}"
            : snapshot.State.ToString().ToUpperInvariant();

    private static string FormatTime(TimeSpan value)
        => value.TotalHours >= 1
            ? value.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : value.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
}
