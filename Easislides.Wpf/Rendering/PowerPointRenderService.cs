using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Interop;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Rendering;

public enum PowerPointRenderErrorKind
{
    None,
    InvalidRequest,
    FileNotFound,
    UnsupportedFile,
    FileLocked,
    Timeout,
    MissingOffice,
    CorruptFile,
    Cancelled,
    Unknown
}

public sealed record PowerPointRenderRequest(
    string FilePath,
    int SlideNumber,
    int PixelWidth,
    int PixelHeight,
    TimeSpan Timeout,
    bool UseCache = true);

public sealed record PowerPointSlideSnapshot(
    string FilePath,
    int SlideNumber,
    int SlideCount,
    int PixelWidth,
    int PixelHeight,
    byte[] ImageBytes,
    string ContentType,
    DateTimeOffset GeneratedAtUtc);

public sealed record PowerPointRenderResult(
    PowerPointRenderErrorKind ErrorKind,
    PowerPointSlideSnapshot? Slide,
    string? ErrorMessage,
    bool FromCache,
    TimeSpan Elapsed)
{
    public bool Succeeded => ErrorKind == PowerPointRenderErrorKind.None && Slide is not null;
}

public sealed class PowerPointRenderException : Exception
{
    public PowerPointRenderException(PowerPointRenderErrorKind errorKind, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorKind = errorKind;
    }

    public PowerPointRenderErrorKind ErrorKind { get; }
}

public interface IPowerPointRenderService
{
    Task<PowerPointRenderResult> RenderSlideAsync(
        PowerPointRenderRequest request,
        CancellationToken cancellationToken = default);

    void ClearCache();
}

public interface IPowerPointRenderBackend
{
    Task<PowerPointSlideSnapshot> RenderSlideAsync(
        PowerPointRenderRequest request,
        CancellationToken cancellationToken);
}

public sealed class PowerPointRenderService : IPowerPointRenderService, IDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ppt",
        ".pptx",
        ".pps",
        ".ppsx"
    };

    private readonly IPowerPointRenderBackend _backend;
    private readonly IThumbnailCache _cache;
    private readonly ISettingsService? _settings;
    private readonly ThumbnailCache? _settingsBackedCache;

    public PowerPointRenderService(IPowerPointRenderBackend backend)
        : this(backend, new ThumbnailCache())
    {
    }

    public PowerPointRenderService(IPowerPointRenderBackend backend, IThumbnailCache cache)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public PowerPointRenderService(IPowerPointRenderBackend backend, ISettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _settings = settings;
        _settingsBackedCache = new ThumbnailCache(CreateThumbnailCacheOptions(settings));
        _cache = _settingsBackedCache;
        _settings.SettingsChanged += OnSettingsChanged;
    }

    public async Task<PowerPointRenderResult> RenderSlideAsync(
        PowerPointRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!TryNormalizeRequest(request, out var normalized, out var fileInfo, out var failure))
        {
            return Complete(failure, stopwatch.Elapsed);
        }

        var cacheKey = CreateCacheKey(normalized, fileInfo!);
        if (normalized.UseCache && TryGetCached(cacheKey, out var cached))
        {
            return new PowerPointRenderResult(
                PowerPointRenderErrorKind.None,
                cached,
                ErrorMessage: null,
                FromCache: true,
                stopwatch.Elapsed);
        }

        if (!CanOpenForRead(normalized.FilePath, out var readFailure))
        {
            return Complete(readFailure, stopwatch.Elapsed);
        }

        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            var renderTask = _backend.RenderSlideAsync(normalized, linkedCts.Token);
            var delayTask = Task.Delay(normalized.Timeout, cancellationToken);
            var completed = await Task.WhenAny(renderTask, delayTask).ConfigureAwait(false);

            if (completed != renderTask)
            {
                timeoutCts.Cancel();
                ObserveFault(renderTask);
                var kind = cancellationToken.IsCancellationRequested
                    ? PowerPointRenderErrorKind.Cancelled
                    : PowerPointRenderErrorKind.Timeout;
                return Failure(kind, kind.ToString(), stopwatch.Elapsed);
            }

            var slide = await renderTask.ConfigureAwait(false);
            Store(cacheKey, slide);
            return new PowerPointRenderResult(
                PowerPointRenderErrorKind.None,
                slide,
                ErrorMessage: null,
                FromCache: false,
                stopwatch.Elapsed);
        }
        catch (PowerPointRenderException ex)
        {
            return Failure(ex.ErrorKind, ex.Message, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(PowerPointRenderErrorKind.Cancelled, "Cancelled", stopwatch.Elapsed);
        }
        catch (OperationCanceledException)
        {
            return Failure(PowerPointRenderErrorKind.Timeout, "Timeout", stopwatch.Elapsed);
        }
        catch (COMException ex)
        {
            var kind = ClassifyComException(ex);
            return Failure(kind, ex.Message, stopwatch.Elapsed);
        }
        catch (IOException ex)
        {
            return Failure(PowerPointRenderErrorKind.FileLocked, ex.Message, stopwatch.Elapsed);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(PowerPointRenderErrorKind.FileLocked, ex.Message, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            return Failure(PowerPointRenderErrorKind.Unknown, ex.Message, stopwatch.Elapsed);
        }
    }

    public void ClearCache() => _cache.Clear();

    public void Dispose()
    {
        if (_settings is not null)
        {
            _settings.SettingsChanged -= OnSettingsChanged;
        }
    }

    private bool TryNormalizeRequest(
        PowerPointRenderRequest request,
        out PowerPointRenderRequest normalized,
        out FileInfo? fileInfo,
        out PowerPointRenderResult failure)
    {
        normalized = request;
        fileInfo = null;
        failure = Failure(PowerPointRenderErrorKind.InvalidRequest, "Invalid request.", TimeSpan.Zero);

        if (string.IsNullOrWhiteSpace(request.FilePath)
            || request.SlideNumber < 1
            || request.PixelWidth < 1
            || request.PixelHeight < 1)
        {
            return false;
        }

        var extension = Path.GetExtension(request.FilePath);
        if (!SupportedExtensions.Contains(extension))
        {
            failure = Failure(PowerPointRenderErrorKind.UnsupportedFile, $"Unsupported PowerPoint file: {extension}", TimeSpan.Zero);
            return false;
        }

        var fullPath = Path.GetFullPath(request.FilePath);
        fileInfo = new FileInfo(fullPath);
        if (!fileInfo.Exists)
        {
            failure = Failure(PowerPointRenderErrorKind.FileNotFound, $"File not found: {fullPath}", TimeSpan.Zero);
            return false;
        }

        var timeout = request.Timeout <= TimeSpan.Zero ? GetConfiguredTimeout() : request.Timeout;
        normalized = request with { FilePath = fullPath, Timeout = timeout };
        return true;
    }

    private TimeSpan GetConfiguredTimeout()
    {
        if (_settings is null)
        {
            return DefaultTimeout;
        }

        var seconds = Clamp(_settings.Get(EasiSettingKeys.PowerPointRenderTimeoutSeconds), 1, 300);
        return TimeSpan.FromSeconds(seconds);
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs args)
    {
        if (_settingsBackedCache is null || _settings is null || !ContainsSetting(args.ChangedKeys, EasiSettingKeys.ThumbnailCacheMegabytes.Id))
        {
            return;
        }

        _settingsBackedCache.UpdateOptions(CreateThumbnailCacheOptions(_settings));
    }

    private static bool CanOpenForRead(string path, out PowerPointRenderResult failure)
    {
        failure = Failure(PowerPointRenderErrorKind.FileLocked, $"File is locked: {path}", TimeSpan.Zero);

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return true;
        }
        catch (IOException ex)
        {
            failure = Failure(PowerPointRenderErrorKind.FileLocked, ex.Message, TimeSpan.Zero);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            failure = Failure(PowerPointRenderErrorKind.FileLocked, ex.Message, TimeSpan.Zero);
            return false;
        }
    }

    private bool TryGetCached(ThumbnailCacheKey key, out PowerPointSlideSnapshot? snapshot)
        => _cache.TryGet(key, out snapshot);

    private void Store(ThumbnailCacheKey key, PowerPointSlideSnapshot snapshot)
        => _cache.Store(key, snapshot, snapshot.ImageBytes.LongLength);

    private static ThumbnailCacheKey CreateCacheKey(PowerPointRenderRequest request, FileInfo fileInfo)
        => ThumbnailCacheKey.FromFile(
            new ThumbnailCacheRequest(
                request.FilePath,
                ThumbnailSourceKind.PowerPointSlide,
                request.SlideNumber,
                request.PixelWidth,
                request.PixelHeight,
                Variant: "render"),
            fileInfo);

    private static PowerPointRenderResult Complete(PowerPointRenderResult result, TimeSpan elapsed)
        => result with { Elapsed = elapsed };

    private static PowerPointRenderResult Failure(PowerPointRenderErrorKind kind, string message, TimeSpan elapsed)
        => new(kind, Slide: null, message, FromCache: false, elapsed);

    private static void ObserveFault(Task task)
        => _ = task.ContinueWith(
            static completed => _ = completed.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

    private static PowerPointRenderErrorKind ClassifyComException(COMException exception)
    {
        var message = exception.Message;
        return message.Contains("class not registered", StringComparison.OrdinalIgnoreCase)
            || message.Contains("COM class factory", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not installed", StringComparison.OrdinalIgnoreCase)
            ? PowerPointRenderErrorKind.MissingOffice
            : PowerPointRenderErrorKind.CorruptFile;
    }

    private static ThumbnailCacheOptions CreateThumbnailCacheOptions(ISettingsService settings)
        => new(MaxBytes: MegabytesToBytes(settings.Get(EasiSettingKeys.ThumbnailCacheMegabytes)));

    private static long MegabytesToBytes(int megabytes)
        => Math.Max(0L, megabytes) * 1024L * 1024L;

    private static bool ContainsSetting(IReadOnlyList<string> changedKeys, string settingId)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            if (string.Equals(changedKeys[i], settingId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int Clamp(int value, int min, int max)
        => Math.Min(Math.Max(value, min), max);

}

public sealed class OfficePowerPointRenderBackend : IPowerPointRenderBackend, IDisposable
{
    private readonly Lazy<OfficePptSession> _session;

    public OfficePowerPointRenderBackend()
        : this(static () => new OfficePptSession())
    {
    }

    internal OfficePowerPointRenderBackend(Func<OfficePptSession> sessionFactory)
        => _session = new Lazy<OfficePptSession>(sessionFactory);

    public async Task<PowerPointSlideSnapshot> RenderSlideAsync(
        PowerPointRenderRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var export = await _session.Value.ExportSlideAsync(
            request.FilePath,
            request.SlideNumber,
            request.PixelWidth,
            request.PixelHeight).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new PowerPointSlideSnapshot(
            request.FilePath,
            request.SlideNumber,
            export.SlideCount,
            request.PixelWidth,
            request.PixelHeight,
            export.ImageBytes,
            export.ContentType,
            DateTimeOffset.UtcNow);
    }

    public void Dispose()
    {
        if (_session.IsValueCreated)
        {
            _session.Value.Dispose();
        }
    }
}
