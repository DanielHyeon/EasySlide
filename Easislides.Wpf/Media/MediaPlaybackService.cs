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
    private readonly IMediaPlaybackBackend _backend;

    public MediaPlaybackService()
        : this(new NoOpMediaPlaybackBackend())
    {
    }

    public MediaPlaybackService(IMediaPlaybackBackend backend)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
    }

    public event EventHandler<MediaPlaybackChangedEventArgs>? PlaybackChanged;

    public MediaPlaybackSnapshot Current { get; private set; } = MediaPlaybackSnapshot.Empty;

    public void Load(MediaPlaybackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Source);

        var snapshot = new MediaPlaybackSnapshot(
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
            ErrorMessage: null);

        TryUpdate(snapshot, () => _backend.Load(snapshot));
    }

    public void Play()
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        var next = Current with { State = MediaPlaybackState.Playing, ErrorMessage = null };
        TryUpdate(next, _backend.Play);
    }

    public void Pause()
    {
        if (Current.State != MediaPlaybackState.Playing)
        {
            return;
        }

        var next = Current with { State = MediaPlaybackState.Paused };
        TryUpdate(next, _backend.Pause);
    }

    public void Stop()
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        var next = Current with { State = MediaPlaybackState.Stopped, Position = TimeSpan.Zero };
        TryUpdate(next, _backend.Stop);
    }

    public void Seek(TimeSpan position)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        var clamped = ClampPosition(position, Current.Duration);
        var next = Current with { Position = clamped };
        TryUpdate(next, () => _backend.Seek(clamped));
    }

    public void SetVolume(int volume)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        ApplySettings(Current with { Volume = Clamp(volume, 0, 100) });
    }

    public void SetBalance(int balance)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        ApplySettings(Current with { Balance = Clamp(balance, -100, 100) });
    }

    public void SetMuted(bool isMuted)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        ApplySettings(Current with { IsMuted = isMuted });
    }

    public void SetRepeatEnabled(bool isRepeatEnabled)
    {
        if (!HasLoadedMedia)
        {
            return;
        }

        ApplySettings(Current with { IsRepeatEnabled = isRepeatEnabled });
    }

    private bool HasLoadedMedia => Current.State is not MediaPlaybackState.Empty and not MediaPlaybackState.Failed;

    private void ApplySettings(MediaPlaybackSnapshot snapshot)
        => TryUpdate(snapshot, () => _backend.ApplySettings(snapshot));

    private void TryUpdate(MediaPlaybackSnapshot snapshot, Action backendAction)
    {
        try
        {
            backendAction();
            Update(snapshot);
        }
        catch (Exception ex)
        {
            Update(snapshot with
            {
                State = MediaPlaybackState.Failed,
                ErrorMessage = ex.Message
            });
        }
    }

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
