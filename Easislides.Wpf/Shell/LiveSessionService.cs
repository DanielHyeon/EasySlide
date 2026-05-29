using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Shell;

public sealed record LiveSessionSnapshot(
    LiveState State,
    string CurrentItemTitle,
    string OutputMonitorName,
    bool IsBlackout,
    string CurrentItemKind = "",
    // 라이브 큐 항목의 미리보기 이미지(슬라이드 썸네일 등).
    // 라이브가 아니거나 미리보기가 없는 항목은 null. OutputWindow는 이 값을 받아
    // SetContentAsset()으로 송출 화면 콘텐츠 슬롯에 적용한다.
    ImageSource? CurrentItemPreviewSource = null,
    ImageFillMode CurrentItemPreviewFillMode = ImageFillMode.Fit,
    int CurrentItemPreviewPixelWidth = 0,
    int CurrentItemPreviewPixelHeight = 0)
{
    public static LiveSessionSnapshot Off { get; } = new(
        LiveState.Off,
        string.Empty,
        string.Empty,
        IsBlackout: false);
}

public sealed class LiveSessionChangedEventArgs : EventArgs
{
    public LiveSessionChangedEventArgs(LiveSessionSnapshot snapshot) => Snapshot = snapshot;

    public LiveSessionSnapshot Snapshot { get; }
}

public interface ILiveSessionService
{
    event EventHandler<LiveSessionChangedEventArgs>? SessionChanged;

    LiveSessionSnapshot Current { get; }

    void GoLive(LiveQueueItem item, string outputMonitorName);
    void HideOutput(bool blackout);
    void Stop();
}

public sealed class LiveSessionService : ILiveSessionService
{
    public event EventHandler<LiveSessionChangedEventArgs>? SessionChanged;

    public LiveSessionSnapshot Current { get; private set; } = LiveSessionSnapshot.Off;

    public void GoLive(LiveQueueItem item, string outputMonitorName)
    {
        ArgumentNullException.ThrowIfNull(item);

        var (pixelWidth, pixelHeight) = ExtractPixelDimensions(item.PreviewSource);
        Update(new LiveSessionSnapshot(
            LiveState.Active,
            item.Title,
            outputMonitorName,
            IsBlackout: false,
            item.Kind,
            item.PreviewSource,
            item.PreviewFillMode,
            pixelWidth,
            pixelHeight));
    }

    // PreviewSource가 BitmapSource이면 픽셀 단위 크기를 추출해 OutputRenderer가 ContentPlacement를
    // 정확히 계산하도록 한다. DrawingImage 등 BitmapSource가 아닌 ImageSource는 0으로 두고,
    // OutputRenderer는 0인 경우 뷰포트 전체를 사용한다.
    private static (int Width, int Height) ExtractPixelDimensions(ImageSource? source)
    {
        if (source is BitmapSource bitmap)
        {
            return (bitmap.PixelWidth, bitmap.PixelHeight);
        }
        return (0, 0);
    }

    public void HideOutput(bool blackout)
    {
        if (Current.State == LiveState.Off)
        {
            return;
        }

        Update(Current with
        {
            State = LiveState.Hidden,
            IsBlackout = blackout,
        });
    }

    public void Stop() => Update(LiveSessionSnapshot.Off);

    private void Update(LiveSessionSnapshot snapshot)
    {
        if (snapshot == Current)
        {
            return;
        }

        Current = snapshot;
        SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(snapshot));
    }
}
