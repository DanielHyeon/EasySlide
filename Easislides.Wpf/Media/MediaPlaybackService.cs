using System;
using System.Collections.Generic;
using System.IO;
using Easislides.Wpf.Settings;

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
    void LoadFromMediaDirectory(string fileName, TimeSpan duration, string mediaType);
    void LoadLiveCamera(TimeSpan duration, bool isWidescreen = true, string? outputDisplayId = null);
    void Play();
    void Pause();
    void Stop();
    void Unload();
    void Seek(TimeSpan position);
    void SetVolume(int volume);
    void SetBalance(int balance);
    void SetMuted(bool isMuted);
    void SetRepeatEnabled(bool isRepeatEnabled);
}

public sealed class MediaPlaybackService : IMediaPlaybackService, IDisposable
{
    private const string LiveCameraSourcePrefix = "<<Capture>>";

    private readonly IMediaPlaybackBackend _backend;
    private readonly ISettingsService? _settings;

    public MediaPlaybackService()
        : this(new NoOpMediaPlaybackBackend())
    {
    }

    public MediaPlaybackService(IMediaPlaybackBackend backend)
        : this(backend, settings: null)
    {
    }

    public MediaPlaybackService(IMediaPlaybackBackend backend, ISettingsService? settings)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _settings = settings;
        if (_settings is not null)
        {
            _settings.SettingsChanged += OnSettingsChanged;
        }
    }

    public event EventHandler<MediaPlaybackChangedEventArgs>? PlaybackChanged;

    public MediaPlaybackSnapshot Current { get; private set; } = MediaPlaybackSnapshot.Empty;

    public void Load(MediaPlaybackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Source);
        var effectiveRequest = ApplyConfiguredAudioDefaults(request);

        var snapshot = new MediaPlaybackSnapshot(
            MediaPlaybackState.Ready,
            effectiveRequest.Source,
            effectiveRequest.SourceKind,
            TimeSpan.Zero,
            ClampDuration(effectiveRequest.Duration),
            effectiveRequest.MediaType,
            Clamp(effectiveRequest.Volume, 0, 100),
            Clamp(effectiveRequest.Balance, -100, 100),
            effectiveRequest.IsMuted,
            effectiveRequest.IsRepeatEnabled,
            effectiveRequest.IsWidescreen,
            effectiveRequest.OutputDisplayId,
            ErrorMessage: null);

        TryUpdate(snapshot, () => _backend.Load(snapshot));
    }

    public void LoadFromMediaDirectory(string fileName, TimeSpan duration, string mediaType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        Load(new MediaPlaybackRequest(
            ResolveMediaSource(fileName),
            MediaSourceKind.File,
            duration,
            mediaType));
    }

    public void LoadLiveCamera(TimeSpan duration, bool isWidescreen = true, string? outputDisplayId = null)
        => Load(new MediaPlaybackRequest(
            CreateLiveCameraSource(GetConfiguredLiveCameraNumber()),
            MediaSourceKind.CaptureDevice,
            duration,
            "Live Camera",
            IsWidescreen: isWidescreen,
            OutputDisplayId: outputDisplayId));

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

    public void Unload()
    {
        // 이미 비어 있으면 할 일 없음(중복 백엔드 호출·이벤트 방지).
        if (Current.State == MediaPlaybackState.Empty)
        {
            return;
        }

        // Unload 는 "무조건 화면에서 내린다"가 본질이라, Load/Play 같은 "성공 전제" 연산과 달리
        // 백엔드가 실패해도 상태는 Empty 로 강제한다(그래서 TryUpdate 를 쓰지 않음).
        // Failed 로 남으면 HasLoadedMedia=false 가 되어 이후 복구 호출이 막히고 영상이 잔류할 수 있다.
        try
        {
            _backend.Unload();
        }
        catch (Exception ex)
        {
            // 백엔드 정리 실패는 무시 — 출력 패리티(상태 비움)가 우선.
            System.Diagnostics.Debug.WriteLine($"[MEDIA] Unload 백엔드 정리 실패(무시): {ex.Message}");
        }

        Update(MediaPlaybackSnapshot.Empty);
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

    public void Dispose()
    {
        if (_settings is not null)
        {
            _settings.SettingsChanged -= OnSettingsChanged;
        }
    }

    private bool HasLoadedMedia => Current.State is not MediaPlaybackState.Empty and not MediaPlaybackState.Failed;

    private void ApplySettings(MediaPlaybackSnapshot snapshot)
        => TryUpdate(snapshot, () => _backend.ApplySettings(snapshot));

    private MediaPlaybackRequest ApplyConfiguredAudioDefaults(MediaPlaybackRequest request)
    {
        if (_settings is null)
        {
            return request;
        }

        return request with
        {
            Volume = ToPercent(_settings.Get(EasiSettingKeys.MediaVolume), 0, 100),
            Balance = ToPercent(_settings.Get(EasiSettingKeys.MediaBalance), -100, 100),
            IsMuted = _settings.Get(EasiSettingKeys.MediaMuted)
        };
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs args)
    {
        if (!HasLoadedMedia || _settings is null)
        {
            return;
        }

        if (ContainsLiveCameraSetting(args.ChangedKeys) && Current.SourceKind == MediaSourceKind.CaptureDevice)
        {
            var next = Current with
            {
                Source = CreateLiveCameraSource(GetConfiguredLiveCameraNumber()),
                ErrorMessage = null
            };
            TryUpdate(next, () => _backend.Load(next));
        }

        if (!ContainsAudioSetting(args.ChangedKeys))
        {
            return;
        }

        ApplySettings(Current with
        {
            Volume = ToPercent(_settings.Get(EasiSettingKeys.MediaVolume), 0, 100),
            Balance = ToPercent(_settings.Get(EasiSettingKeys.MediaBalance), -100, 100),
            IsMuted = _settings.Get(EasiSettingKeys.MediaMuted)
        });
    }

    private string ResolveMediaSource(string fileName)
    {
        if (Path.IsPathRooted(fileName) || _settings is null)
        {
            return fileName;
        }

        var mediaDirectory = _settings.Get(EasiSettingKeys.MediaDirectory);
        return string.IsNullOrWhiteSpace(mediaDirectory)
            ? fileName
            : Path.Combine(mediaDirectory, fileName);
    }

    private int GetConfiguredLiveCameraNumber()
        => _settings?.Get(EasiSettingKeys.LiveCameraNumber) ?? EasiSettingKeys.LiveCameraNumber.DefaultValue;

    public static string CreateLiveCameraSource(int cameraNumber)
        => $"{LiveCameraSourcePrefix}{cameraNumber}";

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

    private static bool ContainsAudioSetting(IReadOnlyList<string> changedKeys)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            var key = changedKeys[i];
            if (string.Equals(key, EasiSettingKeys.MediaVolume.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.MediaBalance.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.MediaMuted.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsLiveCameraSetting(IReadOnlyList<string> changedKeys)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            if (string.Equals(changedKeys[i], EasiSettingKeys.LiveCameraNumber.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

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

    private static int ToPercent(double value, int min, int max)
        => Clamp((int)Math.Round(value * 100, MidpointRounding.AwayFromZero), min, max);
}
