using System;

namespace Easislides.Wpf.Media;

public enum MediaSourceKind
{
    File,
    CaptureDevice,
    ItemTitle,
    Default
}

public enum MediaPlaybackState
{
    Empty,
    Ready,
    Playing,
    Paused,
    Stopped,
    Failed
}

public sealed record MediaPlaybackRequest(
    string Source,
    MediaSourceKind SourceKind,
    TimeSpan Duration,
    string MediaType,
    int Volume = 50,
    int Balance = 0,
    bool IsMuted = false,
    bool IsRepeatEnabled = false,
    bool IsWidescreen = true,
    string? OutputDisplayId = null);

public sealed record MediaPlaybackSnapshot(
    MediaPlaybackState State,
    string Source,
    MediaSourceKind SourceKind,
    TimeSpan Position,
    TimeSpan Duration,
    string MediaType,
    int Volume,
    int Balance,
    bool IsMuted,
    bool IsRepeatEnabled,
    bool IsWidescreen,
    string? OutputDisplayId,
    string? ErrorMessage)
{
    public static MediaPlaybackSnapshot Empty { get; } = new(
        MediaPlaybackState.Empty,
        string.Empty,
        MediaSourceKind.File,
        TimeSpan.Zero,
        TimeSpan.Zero,
        string.Empty,
        Volume: 50,
        Balance: 0,
        IsMuted: false,
        IsRepeatEnabled: false,
        IsWidescreen: true,
        OutputDisplayId: null,
        ErrorMessage: null);
}

public sealed class MediaPlaybackChangedEventArgs : EventArgs
{
    public MediaPlaybackChangedEventArgs(MediaPlaybackSnapshot snapshot) => Snapshot = snapshot;

    public MediaPlaybackSnapshot Snapshot { get; }
}

public interface IMediaPlaybackService
{
    event EventHandler<MediaPlaybackChangedEventArgs>? PlaybackChanged;

    MediaPlaybackSnapshot Current { get; }

    void Load(MediaPlaybackRequest request);
    void Play();
    void Pause();
    void Stop();
    void Seek(TimeSpan position);
    void SetVolume(int volume);
    void SetBalance(int balance);
    void SetMuted(bool isMuted);
    void SetRepeatEnabled(bool isRepeatEnabled);
}

public sealed class MediaPlaybackService : IMediaPlaybackService
{
    public event EventHandler<MediaPlaybackChangedEventArgs>? PlaybackChanged;

    public MediaPlaybackSnapshot Current { get; private set; } = MediaPlaybackSnapshot.Empty;

    public void Load(MediaPlaybackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Source);

        Update(new MediaPlaybackSnapshot(
            MediaPlaybackState.Ready,
            request.Source,
            request.SourceKind,
            TimeSpan.Zero,
            ClampDuration(request.Duration),
            request.MediaType,
            Clamp(request.Volume, 0, 100),
            Clamp(request.Balance, -100, 100),
            request.IsMuted,
            request.IsRepeatEnabled,
            request.IsWidescreen,
            request.OutputDisplayId,
            ErrorMessage: null));
    }

    public void Play()
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { State = MediaPlaybackState.Playing, ErrorMessage = null });
    }

    public void Pause()
    {
        if (Current.State != MediaPlaybackState.Playing)
        {
            return;
        }

        Update(Current with { State = MediaPlaybackState.Paused });
    }

    public void Stop()
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { State = MediaPlaybackState.Stopped, Position = TimeSpan.Zero });
    }

    public void Seek(TimeSpan position)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { Position = ClampPosition(position, Current.Duration) });
    }

    public void SetVolume(int volume)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { Volume = Clamp(volume, 0, 100) });
    }

    public void SetBalance(int balance)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { Balance = Clamp(balance, -100, 100) });
    }

    public void SetMuted(bool isMuted)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { IsMuted = isMuted });
    }

    public void SetRepeatEnabled(bool isRepeatEnabled)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        Update(Current with { IsRepeatEnabled = isRepeatEnabled });
    }

    private bool HasLoadedMedia => Current.State != MediaPlaybackState.Empty;

    private void Update(MediaPlaybackSnapshot snapshot)
    {
        if (snapshot == Current)
        {
            return;
        }

        Current = snapshot;
        PlaybackChanged?.Invoke(this, new MediaPlaybackChangedEventArgs(snapshot));
    }

    private static TimeSpan ClampDuration(TimeSpan duration)
        => duration < TimeSpan.Zero ? TimeSpan.Zero : duration;

    private static TimeSpan ClampPosition(TimeSpan position, TimeSpan duration)
    {
        if (position < TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        return position > duration ? duration : position;
    }

    private static int Clamp(int value, int min, int max)
        => Math.Min(Math.Max(value, min), max);
}
