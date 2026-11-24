using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Axicultus.Wpf.Rendering;

/// <summary>PPT 미리보기 상태.</summary>
public enum PowerPointPreviewState
{
    Idle,
    Rendering,
    Ready,
    Failed,
}

/// <summary>What became of one <see cref="PowerPointPreviewViewModel.LoadAsync"/> call: its slide is shown, it failed, or a
/// newer load or a clear replaced it before it finished.</summary>
public enum PowerPointLoadResult
{
    Shown,
    Failed,
    Superseded,
}

/// <summary>덱 썸네일 스트립의 한 슬라이드(번호 + 작은 이미지 + 현재 슬라이드 여부).</summary>
public sealed partial class PowerPointSlideThumbnail(int slideNumber, ImageSource image, int pixelWidth = 0, int pixelHeight = 0) : ObservableObject
{
    public int SlideNumber { get; } = slideNumber;

    public ImageSource Image { get; } = image;

    public int PixelWidth { get; } = pixelWidth;

    public int PixelHeight { get; } = pixelHeight;

    internal (int Width, int Height)? RenderBounds { get; init; }

    /// <summary>지금 송출/미리보기 중인 슬라이드면 true(스트립에서 강조 표시).</summary>
    [ObservableProperty] private bool _isCurrent;
    [ObservableProperty] private bool _isNext;
}

/// <summary>
/// IPowerPointRenderService 를 UI(MainWindow PowerPoint 탭)에 연결하는 미리보기 VM
/// (G1 / gap-analysis.md §4 G-α).
///
/// 렌더 서비스(+COM 백엔드)는 DI 등록만 돼 있고 어떤 창에도 안 붙어 있었다(orphaned).
/// 이 VM 이 슬라이드 1장을 렌더해 ImageSource 로 노출한다(placeholder "Decks: N" 대체).
/// 이미지 디코드는 주입 가능 — 테스트는 실제 PNG 디코드/STA 없이 스텁 디코더로 격리한다.
/// </summary>
public sealed partial class PowerPointPreviewViewModel(IPowerPointRenderService render, Func<byte[], ImageSource>? imageDecoder = null) : ObservableObject
{
    private const int MinimumThumbnailPixelWidth = LegacyPowerPointImageSize.Width;
    private const int MinimumThumbnailPixelHeight = LegacyPowerPointImageSize.Height;

    private readonly IPowerPointRenderService _render = render ?? throw new ArgumentNullException(nameof(render));
    private readonly Func<byte[], Task<ImageSource>> _decodeAsync = imageDecoder == null
            ? bytes => Task.Run<ImageSource>(() => DecodePng(bytes))
            : bytes => Task.FromResult(imageDecoder(bytes));
    private CancellationTokenSource? _thumbnailCts;
    private Task? _thumbnailTask;
    private StripKey? _stripKey;
    private long _previewGeneration;

    private sealed record StripKey(string Path, long LastWriteTicks, long Length, int Count, int Width, int Height)
    {
        public static StripKey? TryCreate(string path, int count, int width, int height)
        {
            try
            {
                var file = new FileInfo(path);
                bool exists = file.Exists;
                return new(ThumbnailCacheKey.NormalizePath(path), exists ? file.LastWriteTimeUtc.Ticks : 0,
                    exists ? file.Length : -1, count, Math.Max(MinimumThumbnailPixelWidth, width),
                    Math.Max(MinimumThumbnailPixelHeight, height));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                return null;
            }
        }
    }

    [ObservableProperty] private ImageSource? _previewImage;
    [ObservableProperty] private PowerPointPreviewState _state = PowerPointPreviewState.Idle;
    [ObservableProperty] private string _statusText = "PPT 없음";
    [ObservableProperty] private int _slideNumber;
    [ObservableProperty] private int _slideCount;

    /// <summary>덱 전체 슬라이드 썸네일 스트립(클릭으로 해당 슬라이드 이동). 덱이 바뀔 때 백그라운드로 채워진다.</summary>
    public ObservableCollection<PowerPointSlideThumbnail> Thumbnails { get; } = [];

    /// <summary>
    /// 마지막으로 "성공" 렌더한 PPT 파일 경로(없으면 null). 출력 송출 시 신원 확인용 —
    /// 현재 PreviewImage 가 실제로 어느 파일의 슬라이드인지 확인해, 비동기 렌더 경쟁으로
    /// 다른 항목의 stale 슬라이드가 잘못 송출되는 것을 막는다. SlideNumber 와 함께 신원을 이룬다.
    /// </summary>
    public string? LoadedContentPath { get; private set; }
    internal OfficeLib.SlideImageManifest.SourceVersion? LoadedSourceVersion { get; private set; }

    /// <summary>
    /// 다른 PPT 미리보기 VM 의 현재 상태를 복사한다. FrmMain 의 PreviewItem/OutputItem 처럼
    /// 운영자 미리보기와 송출 화면이 서로 다른 덱/슬라이드를 보존해야 할 때 사용한다.
    /// </summary>
    public void CopyFrom(PowerPointPreviewViewModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(source, this))
            return;

        _previewGeneration++;
        PreviewImage = source.PreviewImage;
        State = source.State;
        StatusText = source.StatusText;
        SlideNumber = source.SlideNumber;
        SlideCount = source.SlideCount;
        LoadedContentPath = source.LoadedContentPath;
        LoadedSourceVersion = source.LoadedSourceVersion;

        if (_stripKey is { } key && key == source._stripKey &&
            (_thumbnailTask is not null || HasCompleteStrip(key)))
            return;

        InvalidateThumbnailLoad();
        _stripKey = source._stripKey;
        Thumbnails.Clear();
        foreach (var thumbnail in source.Thumbnails)
        {
            Thumbnails.Add(new PowerPointSlideThumbnail(
                thumbnail.SlideNumber,
                thumbnail.Image,
                thumbnail.PixelWidth,
                thumbnail.PixelHeight)
            {
                IsCurrent = thumbnail.IsCurrent,
                IsNext = thumbnail.IsNext,
                RenderBounds = thumbnail.RenderBounds
            });
        }
    }

    /// <summary>
    /// PPT 한 슬라이드를 렌더해 미리보기 이미지로 노출. 성공이면 Ready, 실패면 Failed 상태.
    /// (라이브 흐름이 PPT 항목을 송출할 때 호출 — 현재는 placeholder 대체 + 향후 연결 지점.)
    /// </summary>
    public async Task<PowerPointLoadResult> LoadAsync(
        string filePath,
        int slideNumber,
        int pixelWidth,
        int pixelHeight,
        CancellationToken cancellationToken = default,
        OfficeLib.SlideImageManifest.SourceVersion? expectedVersion = null)
    {
        var sourceVersion = OfficeLib.SlideImageManifest.ReadSource(filePath);
        if (expectedVersion != null && !expectedVersion.Matches(sourceVersion)) return PowerPointLoadResult.Failed;
        long generation = ++_previewGeneration;
        // A live reload keeps the last good frame and strip until the saved replacement has decoded.
        if (expectedVersion == null)
        {
            ClearOutdatedStrip(filePath);
            _renderingPath = filePath;
            State = PowerPointPreviewState.Rendering;
            StatusText = "렌더링 중…";
        }

        var result = await _render.RenderSlideAsync(
            new PowerPointRenderRequest(filePath, slideNumber, pixelWidth, pixelHeight, TimeSpan.FromSeconds(60)),
            cancellationToken).ConfigureAwait(true);

        if (generation != _previewGeneration) return PowerPointLoadResult.Superseded;

        if (result.Succeeded && result.Slide is { } slide)
        {
            ImageSource image;
            try
            {
                image = await _decodeAsync(slide.ImageBytes);
            }
            catch (Exception ex) when (
                ex is NotSupportedException or FileFormatException or ArgumentException or OverflowException or IOException)
            {
                // 렌더는 성공했으나 바이트 디코드 실패 — 상태 머신이 Rendering 에 고착되지 않도록 Failed 로 마무리.
                if (generation != _previewGeneration) return PowerPointLoadResult.Superseded;
                if (expectedVersion == null) SetFailed($"이미지 디코드 실패: {ex.Message}");
                return PowerPointLoadResult.Failed;
            }

            if (generation != _previewGeneration) return PowerPointLoadResult.Superseded;
            if (cancellationToken.IsCancellationRequested)
            {
                if (expectedVersion == null) SetFailed("렌더링 취소됨");
                return PowerPointLoadResult.Failed;
            }
            if (expectedVersion != null)
            {
                if (!expectedVersion.Matches(OfficeLib.SlideImageManifest.ReadSource(filePath))) return PowerPointLoadResult.Failed;
                ClearOutdatedStrip(filePath);
            }
            PreviewImage = image;
            SlideNumber = slide.SlideNumber;
            SlideCount = slide.SlideCount;
            LoadedContentPath = filePath; // 성공 렌더의 신원 기록(출력 송출 시 항목 일치 확인)
            LoadedSourceVersion = sourceVersion;
            State = PowerPointPreviewState.Ready;
            StatusText = $"슬라이드 {slide.SlideNumber}/{slide.SlideCount}";
            return PowerPointLoadResult.Shown;
        }
        if (expectedVersion == null) SetFailed(result.ErrorMessage ?? "PPT 렌더 실패");
        return PowerPointLoadResult.Failed;
    }

    private void ClearOutdatedStrip(string path)
    {
        if (_stripKey is { } strip && strip != StripKey.TryCreate(path, strip.Count, strip.Width, strip.Height))
        {
            InvalidateThumbnailLoad();
            Thumbnails.Clear();
        }
    }

    /// <summary>
    /// 덱 전체 슬라이드 썸네일을 백그라운드로 채운다(덱이 바뀔 때 호출). FrmMain처럼 고해상도 4:3 이미지로 렌더한 뒤 UI에서 축소하며,
    /// 같은 요청은 진행 중인 로딩을 공유하고 파일 버전이나 크기가 바뀌면 이전 로딩을 취소한다. 슬라이드 수가 많으면 순차로 채워진다.
    /// </summary>
    public Task LoadThumbnailsAsync(string filePath, int slideCount, int thumbnailWidth, int thumbnailHeight)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var key = StripKey.TryCreate(filePath, slideCount, thumbnailWidth, thumbnailHeight);
        if (key == null)
        {
            InvalidateThumbnailLoad();
            Thumbnails.Clear();
            return Task.CompletedTask;
        }

        bool sameStrip = key == _stripKey;
        if (sameStrip && _thumbnailTask != null)
            return _thumbnailTask;
        if (sameStrip && HasCompleteStrip(key))
            return Task.CompletedTask;

        InvalidateThumbnailLoad();
        var cts = new CancellationTokenSource();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _thumbnailCts = cts;
        _thumbnailTask = completion.Task;
        _stripKey = key;
        _ = PopulateThumbnailsAsync(filePath, key, sameStrip, cts, completion);
        return completion.Task;
    }

    private bool HasCompleteStrip(StripKey key)
    {
        if (Thumbnails.Count != key.Count)
            return false;
        for (int index = 0; index < Thumbnails.Count; index++)
            if (!MatchesSlide(Thumbnails[index], index + 1, key))
                return false;
        return true;
    }

    private static bool MatchesSlide(PowerPointSlideThumbnail thumbnail, int slide, StripKey key)
    {
        var (width, height) = thumbnail.RenderBounds ?? (thumbnail.PixelWidth, thumbnail.PixelHeight);
        return thumbnail.SlideNumber == slide && width >= key.Width && height >= key.Height;
    }

    private void InvalidateThumbnailLoad()
    {
        var previous = _thumbnailCts;
        _thumbnailCts = null;
        _thumbnailTask = null;
        _stripKey = null;
        previous?.Cancel();
    }

    private async Task PopulateThumbnailsAsync(string filePath, StripKey key, bool sameStrip,
        CancellationTokenSource cts, TaskCompletionSource completion)
    {
        var token = cts.Token;
        // 썸네일은 best-effort 장식이라, fire-and-forget 로 호출돼도(MainViewModel) 어떤 실패든
        // 앱을 죽이지 않도록 전체를 봉인한다(메인 미리보기는 별개 경로라 영향 없음).
        try
        {
            if (!sameStrip)
                Thumbnails.Clear();
            int index = 0;
            for (var slide = 1; slide <= key.Count; slide++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (index < Thumbnails.Count && MatchesSlide(Thumbnails[index], slide, key))
                {
                    index++;
                    continue;
                }
                if (index < Thumbnails.Count && Thumbnails[index].SlideNumber == slide)
                    Thumbnails.RemoveAt(index);

                var result = await _render.RenderSlideAsync(
                    new PowerPointRenderRequest(filePath, slide, key.Width, key.Height, TimeSpan.FromSeconds(60)),
                    token).ConfigureAwait(true);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (result.Succeeded && result.Slide is { } slideSnapshot)
                {
                    ImageSource image;
                    try
                    {
                        image = await _decodeAsync(slideSnapshot.ImageBytes);
                    }
                    catch (Exception ex) when (
                        ex is NotSupportedException or FileFormatException or ArgumentException or OverflowException or IOException)
                    {
                        continue; // 한 장 디코드 실패는 건너뛰고 나머지 썸네일을 계속 채운다.
                    }

                    if (token.IsCancellationRequested)
                        return;

                    Thumbnails.Insert(index, new PowerPointSlideThumbnail(
                        slide,
                        image,
                        slideSnapshot.PixelWidth,
                        slideSnapshot.PixelHeight)
                    {
                        IsNext = slide == SlideNumber + 1 && index > 0 && Thumbnails[index - 1].IsCurrent,
                        IsCurrent = slide == SlideNumber,
                        RenderBounds = slideSnapshot.RenderBounds
                    });
                    index++;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 취소는 정상 — 다음 로딩이 이어받는다.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PPT] 썸네일 로드 실패(무시): {ex.Message}");
        }
        finally
        {
            if (ReferenceEquals(cts, _thumbnailCts))
            {
                UpdateThumbnailSelection(SlideNumber);
                _thumbnailTask = null;
                _thumbnailCts = null;
            }
            cts.Dispose();
            completion.TrySetResult();
        }
    }

    // 현재 슬라이드가 바뀌면 썸네일 스트립의 강조(IsCurrent)를 갱신한다.
    partial void OnSlideNumberChanged(int value) => UpdateThumbnailSelection(value);

    private void UpdateThumbnailSelection(int value)
    {
        bool selected = false;
        foreach (var thumbnail in Thumbnails)
        {
            thumbnail.IsNext = selected && thumbnail.SlideNumber == value + 1;
            thumbnail.IsCurrent = thumbnail.SlideNumber == value;
            selected = thumbnail.IsCurrent;
        }
    }

    internal void MarkNativeSlideAfterRenderFailure(string path, int slide, int knownCount)
    {
        SlideNumber = slide;
        SlideCount = knownCount >= slide ? knownCount : 0;
        if (_stripKey is not { } key || key != StripKey.TryCreate(path, key.Count, key.Width, key.Height)) return;
        UseThumbnail(path, slide, key.Count, OfficeLib.SlideImageManifest.ReadSource(path));
    }

    internal bool TryUseNativeThumbnail(int slide, OfficeLib.SlideImageManifest.SourceVersion version)
    {
        if (_stripKey is not { } strip || strip.LastWriteTicks != version.LastWriteTicks || strip.Length != version.Length ||
            !string.Equals(strip.Path, version.FullPath, StringComparison.OrdinalIgnoreCase)) return false;
        return UseThumbnail(version.FullPath, slide, strip.Count, version);
    }

    private bool UseThumbnail(string path, int slide, int count, OfficeLib.SlideImageManifest.SourceVersion? version)
    {
        foreach (var thumbnail in Thumbnails)
        {
            if (thumbnail.SlideNumber != slide) continue;
            PreviewImage = thumbnail.Image;
            LoadedContentPath = path;
            LoadedSourceVersion = version;
            SlideNumber = slide;
            SlideCount = count;
            State = PowerPointPreviewState.Ready;
            StatusText = $"슬라이드 {slide} (썸네일 사용)";
            return true;
        }
        return false;
    }

    private void SetFailed(string status)
    {
        PreviewImage = null;
        SlideNumber = 0;
        SlideCount = 0;
        LoadedContentPath = null; // 실패 시 신원 무효화 — 이전 성공 슬라이드가 잘못 송출되지 않도록
        LoadedSourceVersion = null;
        State = PowerPointPreviewState.Failed;
        StatusText = status;
    }

    private string? _renderingPath; // the deck of the last render that marked the preview Rendering

    /// <summary>
    /// Drops a slide render in flight and keeps the slide shown (what <see cref="Clear"/> does, without clearing). A
    /// render of another deck is dropped with what is shown, since the frame left is not the deck asked for.
    /// </summary>
    public void AbandonPendingRender()
    {
        _previewGeneration++;
        if (State != PowerPointPreviewState.Rendering) return;
        if (PreviewImage is null || !string.Equals(LoadedContentPath, _renderingPath, StringComparison.OrdinalIgnoreCase)) { Clear(); return; }
        State = PowerPointPreviewState.Ready;
        StatusText = $"슬라이드 {SlideNumber}/{SlideCount}";
    }

    /// <summary>미리보기를 비우고 초기 상태로(다른 종류 항목 선택 시).</summary>
    public void Clear()
    {
        _previewGeneration++;
        InvalidateThumbnailLoad();
        PreviewImage = null;
        State = PowerPointPreviewState.Idle;
        StatusText = "PPT 없음";
        SlideNumber = 0;
        SlideCount = 0;
        LoadedContentPath = null; // 신원 무효화
        LoadedSourceVersion = null;
        Thumbnails.Clear();
    }

    /// <summary>렌더된 PNG/JPEG 바이트를 frozen ImageSource 로 디코드(ImageAssetService 와 동일 방식).</summary>
    private static BitmapFrame DecodePng(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
            BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        frame.Freeze();
        return frame;
    }
}
