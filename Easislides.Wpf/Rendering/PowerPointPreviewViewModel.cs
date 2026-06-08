using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Easislides.Wpf.Rendering;

/// <summary>PPT 미리보기 상태.</summary>
public enum PowerPointPreviewState
{
    Idle,
    Rendering,
    Ready,
    Failed,
}

/// <summary>덱 썸네일 스트립의 한 슬라이드(번호 + 작은 이미지 + 현재 슬라이드 여부).</summary>
public sealed partial class PowerPointSlideThumbnail : ObservableObject
{
    public PowerPointSlideThumbnail(int slideNumber, ImageSource image)
    {
        SlideNumber = slideNumber;
        Image = image;
    }

    public int SlideNumber { get; }

    public ImageSource Image { get; }

    /// <summary>지금 송출/미리보기 중인 슬라이드면 true(스트립에서 강조 표시).</summary>
    [ObservableProperty] private bool _isCurrent;
}

/// <summary>
/// IPowerPointRenderService 를 UI(MainWindow PowerPoint 탭)에 연결하는 미리보기 VM
/// (G1 / gap-analysis.md §4 G-α).
///
/// 렌더 서비스(+COM 백엔드)는 DI 등록만 돼 있고 어떤 창에도 안 붙어 있었다(orphaned).
/// 이 VM 이 슬라이드 1장을 렌더해 ImageSource 로 노출한다(placeholder "Decks: N" 대체).
/// 이미지 디코드는 주입 가능 — 테스트는 실제 PNG 디코드/STA 없이 스텁 디코더로 격리한다.
/// </summary>
public sealed partial class PowerPointPreviewViewModel : ObservableObject
{
    private const int MinimumThumbnailPixelWidth = 3840;
    private const int MinimumThumbnailPixelHeight = 2880;

    private readonly IPowerPointRenderService _render;
    private readonly Func<byte[], ImageSource> _decode;
    private CancellationTokenSource? _thumbnailCts;

    [ObservableProperty] private ImageSource? _previewImage;
    [ObservableProperty] private PowerPointPreviewState _state = PowerPointPreviewState.Idle;
    [ObservableProperty] private string _statusText = "PPT 없음";
    [ObservableProperty] private int _slideNumber;
    [ObservableProperty] private int _slideCount;

    /// <summary>덱 전체 슬라이드 썸네일 스트립(클릭으로 해당 슬라이드 이동). 덱이 바뀔 때 백그라운드로 채워진다.</summary>
    public ObservableCollection<PowerPointSlideThumbnail> Thumbnails { get; } = new();

    /// <summary>
    /// 마지막으로 "성공" 렌더한 PPT 파일 경로(없으면 null). 출력 송출 시 신원 확인용 —
    /// 현재 PreviewImage 가 실제로 어느 파일의 슬라이드인지 확인해, 비동기 렌더 경쟁으로
    /// 다른 항목의 stale 슬라이드가 잘못 송출되는 것을 막는다. SlideNumber 와 함께 신원을 이룬다.
    /// </summary>
    public string? LoadedContentPath { get; private set; }

    public PowerPointPreviewViewModel(IPowerPointRenderService render, Func<byte[], ImageSource>? imageDecoder = null)
    {
        _render = render ?? throw new ArgumentNullException(nameof(render));
        _decode = imageDecoder ?? DecodePng;
    }

    /// <summary>
    /// 다른 PPT 미리보기 VM 의 현재 상태를 복사한다. FrmMain 의 PreviewItem/OutputItem 처럼
    /// 운영자 미리보기와 송출 화면이 서로 다른 덱/슬라이드를 보존해야 할 때 사용한다.
    /// </summary>
    public void CopyFrom(PowerPointPreviewViewModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _thumbnailCts?.Cancel();
        PreviewImage = source.PreviewImage;
        State = source.State;
        StatusText = source.StatusText;
        SlideNumber = source.SlideNumber;
        SlideCount = source.SlideCount;
        LoadedContentPath = source.LoadedContentPath;

        Thumbnails.Clear();
        foreach (var thumbnail in source.Thumbnails)
        {
            Thumbnails.Add(new PowerPointSlideThumbnail(thumbnail.SlideNumber, thumbnail.Image)
            {
                IsCurrent = thumbnail.IsCurrent
            });
        }
    }

    /// <summary>
    /// PPT 한 슬라이드를 렌더해 미리보기 이미지로 노출. 성공이면 Ready, 실패면 Failed 상태.
    /// (라이브 흐름이 PPT 항목을 송출할 때 호출 — 현재는 placeholder 대체 + 향후 연결 지점.)
    /// </summary>
    public async Task LoadAsync(
        string filePath,
        int slideNumber,
        int pixelWidth,
        int pixelHeight,
        CancellationToken cancellationToken = default)
    {
        State = PowerPointPreviewState.Rendering;
        StatusText = "렌더링 중…";

        var result = await _render.RenderSlideAsync(
            new PowerPointRenderRequest(filePath, slideNumber, pixelWidth, pixelHeight, TimeSpan.FromSeconds(60)),
            cancellationToken).ConfigureAwait(true);

        if (result.Succeeded && result.Slide is { } slide)
        {
            ImageSource image;
            try
            {
                image = _decode(slide.ImageBytes);
            }
            catch (Exception ex) when (
                ex is NotSupportedException or FileFormatException or ArgumentException or OverflowException or IOException)
            {
                // 렌더는 성공했으나 바이트 디코드 실패 — 상태 머신이 Rendering 에 고착되지 않도록 Failed 로 마무리.
                SetFailed($"이미지 디코드 실패: {ex.Message}");
                return;
            }

            PreviewImage = image;
            SlideNumber = slide.SlideNumber;
            SlideCount = slide.SlideCount;
            LoadedContentPath = filePath; // 성공 렌더의 신원 기록(출력 송출 시 항목 일치 확인)
            State = PowerPointPreviewState.Ready;
            StatusText = $"슬라이드 {slide.SlideNumber}/{slide.SlideCount}";
        }
        else
        {
            SetFailed(result.ErrorMessage ?? "PPT 렌더 실패");
        }
    }

    /// <summary>
    /// 덱 전체 슬라이드 썸네일을 백그라운드로 채운다(덱이 바뀔 때 호출). 작은 고정 크기로 렌더하며,
    /// 새 호출이 오면 이전 로딩은 취소한다(빠른 덱 전환 대비). 슬라이드 수가 많으면 순차로 채워진다.
    /// </summary>
    public async Task LoadThumbnailsAsync(string filePath, int slideCount, int thumbnailWidth, int thumbnailHeight)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var renderWidth = Math.Max(MinimumThumbnailPixelWidth, thumbnailWidth);
        var renderHeight = Math.Max(MinimumThumbnailPixelHeight, thumbnailHeight);
        _thumbnailCts?.Cancel();
        var cts = new CancellationTokenSource();
        _thumbnailCts = cts;
        var token = cts.Token;

        Thumbnails.Clear();

        // 썸네일은 best-effort 장식이라, fire-and-forget 로 호출돼도(MainViewModel) 어떤 실패든
        // 앱을 죽이지 않도록 전체를 봉인한다(메인 미리보기는 별개 경로라 영향 없음).
        try
        {
            for (var slide = 1; slide <= slideCount; slide++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var result = await _render.RenderSlideAsync(
                    new PowerPointRenderRequest(filePath, slide, renderWidth, renderHeight, TimeSpan.FromSeconds(60)),
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
                        image = _decode(slideSnapshot.ImageBytes);
                    }
                    catch (Exception ex) when (
                        ex is NotSupportedException or FileFormatException or ArgumentException or OverflowException or IOException)
                    {
                        continue; // 한 장 디코드 실패는 건너뛰고 나머지 썸네일을 계속 채운다.
                    }

                    Thumbnails.Add(new PowerPointSlideThumbnail(slide, image) { IsCurrent = slide == SlideNumber });
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
    }

    // 현재 슬라이드가 바뀌면 썸네일 스트립의 강조(IsCurrent)를 갱신한다.
    partial void OnSlideNumberChanged(int value)
    {
        foreach (var thumbnail in Thumbnails)
        {
            thumbnail.IsCurrent = thumbnail.SlideNumber == value;
        }
    }

    private void SetFailed(string status)
    {
        PreviewImage = null;
        SlideNumber = 0;
        SlideCount = 0;
        LoadedContentPath = null; // 실패 시 신원 무효화 — 이전 성공 슬라이드가 잘못 송출되지 않도록
        State = PowerPointPreviewState.Failed;
        StatusText = status;
    }

    /// <summary>미리보기를 비우고 초기 상태로(다른 종류 항목 선택 시).</summary>
    public void Clear()
    {
        _thumbnailCts?.Cancel();
        PreviewImage = null;
        State = PowerPointPreviewState.Idle;
        StatusText = "PPT 없음";
        SlideNumber = 0;
        SlideCount = 0;
        LoadedContentPath = null; // 신원 무효화
        Thumbnails.Clear();
    }

    /// <summary>렌더된 PNG/JPEG 바이트를 frozen ImageSource 로 디코드(ImageAssetService 와 동일 방식).</summary>
    private static ImageSource DecodePng(byte[] bytes)
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
