using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Theme;
using Wpf.Ui.Controls;

namespace Easislides.Wpf.Media;

public sealed partial class MediaPlaybackViewModel : ObservableObject, IDisposable
{
    private static readonly TimeSpan DefaultSeekIncrement = TimeSpan.FromSeconds(5);
    private readonly IMediaPlaybackService _playback;
    private bool _disposed;

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
    // 재생/일시정지 텍스트("Play"/"Pause"). 버튼은 이제 아이콘(PlayPauseSymbol)을 쓰지만, 자동화·툴팁·향후 용도를 위해 유지.
    [ObservableProperty] private string _playPauseLabel = "Play";
    // 재생/일시정지 버튼의 Fluent 아이콘 — 재생 중이면 일시정지 아이콘, 아니면 재생 아이콘(레이블과 짝).
    [ObservableProperty] private SymbolRegular _playPauseSymbol = EsIcons.MediaPlay;
    // 음소거 토글의 Fluent 아이콘 — 음소거면 ×표시 스피커, 아니면 소리나는 스피커(현재 상태를 정직하게).
    [ObservableProperty] private SymbolRegular _muteSymbol = EsIcons.MediaUnmute;

    public MediaPlaybackViewModel(IMediaPlaybackService playback)
    {
        _playback = playback;
        _playback.PlaybackChanged += OnPlaybackChanged;

        PlayPauseCommand = new RelayCommand(PlayPause, CanUsePlaybackControls);
        StopCommand = new RelayCommand(_playback.Stop, CanUsePlaybackControls);
        RestartCommand = new RelayCommand(Restart, CanUsePlaybackControls);
        FastForwardCommand = new RelayCommand(() => _playback.Seek(Position + DefaultSeekIncrement), CanUsePlaybackControls);
        FastReverseCommand = new RelayCommand(() => _playback.Seek(Position - DefaultSeekIncrement), CanUsePlaybackControls);
        ToggleMuteCommand = new RelayCommand(() => _playback.SetMuted(!IsMuted), CanUsePlaybackControls);
        ToggleRepeatCommand = new RelayCommand(() => _playback.SetRepeatEnabled(!IsRepeatEnabled), CanUsePlaybackControls);

        ApplySnapshot(_playback.Current);
    }

    public IRelayCommand PlayPauseCommand { get; }
    public IRelayCommand StopCommand { get; }

    /// <summary>처음부터 다시 재생(레거시 미디어 플레이어 Enter = Remote_ResumeItemFromStart). 위치를 0으로 되감고 재생한다.</summary>
    public IRelayCommand RestartCommand { get; }
    public IRelayCommand FastForwardCommand { get; }
    public IRelayCommand FastReverseCommand { get; }
    public IRelayCommand ToggleMuteCommand { get; }
    public IRelayCommand ToggleRepeatCommand { get; }

    public void Load(MediaPlaybackRequest request) => _playback.Load(request);

    /// <summary>미디어를 완전히 내린다(출력 창에서 영상 제거 → 가사 복귀). 다른 종류 항목으로 전환 시 호출.</summary>
    public void Unload() => _playback.Unload();

    public void Dispose()
    {
        // 멱등 — DI 컨테이너(transient IDisposable 추적)와 MainViewModel 이 모두 해제를 시도할 수 있어
        // 이중 호출이 가능하다. 가드로 한 번만 정리(향후 비-멱등 정리 추가에도 안전).
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _playback.PlaybackChanged -= OnPlaybackChanged;
    }

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

    // 처음부터 다시 재생 — 위치를 0으로 되감고 재생(레거시 Enter 키 동작).
    private void Restart()
    {
        _playback.Seek(TimeSpan.Zero);
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
        MuteSymbol = snapshot.IsMuted ? EsIcons.MediaMute : EsIcons.MediaUnmute;
        IsRepeatEnabled = snapshot.IsRepeatEnabled;
        IsWidescreen = snapshot.IsWidescreen;
        StatusText = CreateStatusText(snapshot);
        PlayPauseLabel = snapshot.State == MediaPlaybackState.Playing ? "Pause" : "Play";
        PlayPauseSymbol = snapshot.State == MediaPlaybackState.Playing ? EsIcons.MediaPause : EsIcons.MediaPlay;
        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        PlayPauseCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        RestartCommand.NotifyCanExecuteChanged();
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
