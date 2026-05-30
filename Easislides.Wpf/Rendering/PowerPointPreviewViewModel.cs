using System;
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
    private readonly IPowerPointRenderService _render;
    private readonly Func<byte[], ImageSource> _decode;

    [ObservableProperty] private ImageSource? _previewImage;
    [ObservableProperty] private PowerPointPreviewState _state = PowerPointPreviewState.Idle;
    [ObservableProperty] private string _statusText = "PPT 없음";
    [ObservableProperty] private int _slideNumber;
    [ObservableProperty] private int _slideCount;

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
        PreviewImage = null;
        State = PowerPointPreviewState.Idle;
        StatusText = "PPT 없음";
        SlideNumber = 0;
        SlideCount = 0;
        LoadedContentPath = null; // 신원 무효화
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
