using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Media;

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
    string? OutputDisplayId = null,
    ItemMediaOptions? ItemOptions = null);

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
    internal string? CameraDevicePath { get; init; }

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

public sealed class MediaPlaybackChangedEventArgs(MediaPlaybackSnapshot snapshot) : EventArgs
{
    public MediaPlaybackSnapshot Snapshot { get; } = snapshot;
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
        _settings?.SettingsChanged += OnSettingsChanged;
    }

    public event EventHandler<MediaPlaybackChangedEventArgs>? PlaybackChanged;

    public MediaPlaybackSnapshot Current { get; private set; } = MediaPlaybackSnapshot.Empty;

    private ItemMediaOptions? _loadedItemOptions;
    private CancellationTokenSource? _pendingCamera;

    public void Load(MediaPlaybackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Source);
        _pendingCamera?.Cancel();
        _loadedItemOptions = request.ItemOptions;
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

    internal void ReportFilePosition(string source, TimeSpan position, TimeSpan duration)
    {
        if (!IsCurrentFile(source) || Current.State is MediaPlaybackState.Failed or MediaPlaybackState.Empty) return;
        Update(Current with { Duration = ClampDuration(duration), Position = ClampPosition(position, duration) });
    }

    internal void ReportFileEnded(string source)
    {
        if (!IsCurrentFile(source) || Current.State != MediaPlaybackState.Playing) return;
        if (!Current.IsRepeatEnabled) { Stop(); return; }
        Seek(TimeSpan.Zero);
        Play();
    }

    internal void ReportFileFailure(string source, Exception failure)
    {
        if (IsCurrentFile(source)) Update(Current with { State = MediaPlaybackState.Failed, ErrorMessage = failure.Message });
    }

    internal void ReportBackendFailure(Exception failure) =>
        Update(Current with { State = MediaPlaybackState.Failed, ErrorMessage = failure.Message });

    private bool IsCurrentFile(string source)
    {
        if (Current.SourceKind != MediaSourceKind.File || string.IsNullOrWhiteSpace(Current.Source)) return false;
        if (Uri.TryCreate(Current.Source, UriKind.Absolute, out var uri))
            return uri.IsFile && string.Equals(uri.LocalPath, source, StringComparison.OrdinalIgnoreCase);
        try { return string.Equals(Path.GetFullPath(Current.Source), source, StringComparison.OrdinalIgnoreCase); }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException) { return false; }
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

    internal async Task SwitchLiveCameraAsync(InputDevice device, string displayId, CancellationToken ct)
    {
        if (_backend is not ILiveCameraSwitchBackend output) throw new MediaPlaybackException("송출 창의 카메라를 사용할 수 없습니다.");
        var camera = _settings?.Current.Operator;
        var next = Current with
        {
            Volume = camera?.CameraVolume ?? Current.Volume, Balance = camera?.CameraBalance ?? Current.Balance,
            IsMuted = camera?.CameraMuted ?? Current.IsMuted, IsWidescreen = camera?.CameraWide ?? Current.IsWidescreen,
            State = MediaPlaybackState.Playing, SourceKind = MediaSourceKind.CaptureDevice,
            MediaType = "Live Camera", Source = CreateLiveCameraSource(device.CameraNumber), CameraDevicePath = device.Id,
            Duration = TimeSpan.Zero, Position = TimeSpan.Zero, OutputDisplayId = displayId, ErrorMessage = null
        };
        if (_pendingCamera != null) throw new MediaPlaybackException("카메라를 전환하는 중입니다.");
        using var pending = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _pendingCamera = pending;
        try
        {
            await output.SwitchCameraAsync(device, next, pending.Token);
            pending.Token.ThrowIfCancellationRequested();
            _loadedItemOptions = null;
            Update(next);
        }
        finally { _pendingCamera = null; }
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
        _pendingCamera?.Cancel();
        if (Current.State != MediaPlaybackState.Playing)
        {
            return;
        }

        var next = Current with { State = MediaPlaybackState.Paused };
        TryUpdate(next, _backend.Pause);
    }

    public void Stop()
    {
        _pendingCamera?.Cancel();
        if (!HasLoadedMedia)
        {
            return;
        }

        var next = Current with { State = MediaPlaybackState.Stopped, Position = TimeSpan.Zero };
        TryUpdate(next, _backend.Stop);
    }

    public void Unload()
    {
        _pendingCamera?.Cancel();
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
        _pendingCamera?.Cancel();
        _settings?.SettingsChanged -= OnSettingsChanged;
    }

    private bool HasLoadedMedia => Current.State is not MediaPlaybackState.Empty and not MediaPlaybackState.Failed;

    private void ApplySettings(MediaPlaybackSnapshot snapshot)
        => TryUpdate(snapshot, () => _backend.ApplySettings(snapshot));

    private MediaPlaybackRequest ApplyConfiguredAudioDefaults(MediaPlaybackRequest request)
    {
        var item = request.ItemOptions;
        var camera = request.SourceKind == MediaSourceKind.CaptureDevice ? _settings?.Current.Operator : null;
        return request with
        {
            Volume = item?.Volume ?? camera?.CameraVolume ?? (_settings == null ? request.Volume : ToPercent(_settings.Get(AxicultusSettingKeys.MediaVolume), 0, 100)),
            Balance = item?.Balance ?? camera?.CameraBalance ?? (_settings == null ? request.Balance : ToPercent(_settings.Get(AxicultusSettingKeys.MediaBalance), -100, 100)),
            IsMuted = item?.Flags is int flags ? (flags & 1) != 0 : camera?.CameraMuted ?? _settings?.Get(AxicultusSettingKeys.MediaMuted) ?? request.IsMuted,
            IsRepeatEnabled = item?.Flags is int repeat ? (repeat & 2) != 0 : request.IsRepeatEnabled,
            IsWidescreen = item?.Flags is int wide ? (wide & 4) != 0 : camera?.CameraWide ?? request.IsWidescreen
        };
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs args)
    {
        if (!HasLoadedMedia || _settings is null)
        {
            return;
        }

        if (ContainsLiveCameraSetting(args.ChangedKeys) && Current.SourceKind == MediaSourceKind.CaptureDevice && _loadedItemOptions?.Mode != 3)
        {
            var next = Current with
            {
                Source = CreateLiveCameraSource(GetConfiguredLiveCameraNumber()),
                CameraDevicePath = null, ErrorMessage = null
            };
            TryUpdate(next, () =>
            {
                _backend.Load(next);
                if (next.State == MediaPlaybackState.Playing) _backend.Play();
            });
        }

        if (Current.SourceKind == MediaSourceKind.CaptureDevice)
        {
            if (System.Linq.Enumerable.Any(args.ChangedKeys, key => key.StartsWith("operator.Camera", StringComparison.Ordinal)))
            {
                var camera = _settings.Current.Operator;
                ApplySettings(Current with
                {
                    Volume = _loadedItemOptions?.Volume ?? camera.CameraVolume,
                    Balance = _loadedItemOptions?.Balance ?? camera.CameraBalance,
                    IsMuted = _loadedItemOptions?.Flags is int flagsOverride ? (flagsOverride & 1) != 0 : camera.CameraMuted,
                    IsWidescreen = _loadedItemOptions?.Flags is int wide ? (wide & 4) != 0 : camera.CameraWide,
                });
            }
            return;
        }
        if (!ContainsAudioSetting(args.ChangedKeys))
        {
            return;
        }

        ApplySettings(Current with
        {
            Volume = _loadedItemOptions?.Volume ?? ToPercent(_settings.Get(AxicultusSettingKeys.MediaVolume), 0, 100),
            Balance = _loadedItemOptions?.Balance ?? ToPercent(_settings.Get(AxicultusSettingKeys.MediaBalance), -100, 100),
            IsMuted = _loadedItemOptions?.Flags is int flags ? (flags & 1) != 0 : _settings.Get(AxicultusSettingKeys.MediaMuted)
        });
    }

    private string ResolveMediaSource(string fileName)
    {
        if (Path.IsPathRooted(fileName) || _settings is null)
        {
            return fileName;
        }

        var mediaDirectory = _settings.Get(AxicultusSettingKeys.MediaDirectory);
        return string.IsNullOrWhiteSpace(mediaDirectory)
            ? fileName
            : Path.Combine(mediaDirectory, fileName);
    }

    private int GetConfiguredLiveCameraNumber()
        => _settings?.Get(AxicultusSettingKeys.LiveCameraNumber) ?? AxicultusSettingKeys.LiveCameraNumber.DefaultValue;

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
            if (string.Equals(key, AxicultusSettingKeys.MediaVolume.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, AxicultusSettingKeys.MediaBalance.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, AxicultusSettingKeys.MediaMuted.Id, StringComparison.OrdinalIgnoreCase))
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
            if (string.Equals(changedKeys[i], AxicultusSettingKeys.LiveCameraNumber.Id, StringComparison.OrdinalIgnoreCase))
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
