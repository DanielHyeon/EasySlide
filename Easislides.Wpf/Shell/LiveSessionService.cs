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
    int CurrentItemPreviewPixelHeight = 0,
    // 라이브 곡 항목의 가사 본문(출력 화면 중앙에 텍스트로 송출). 곡이 아니거나 가사가 없으면 빈 문자열.
    // PPT/미디어처럼 미리보기 이미지로 송출되는 항목은 이 값이 비어 있고, 곡은 반대로 이미지가 비고 본문이 찬다.
    string CurrentItemBodyText = "",
    // 비우기(Clear) 모드 — State=Hidden 이면서 IsBlackout=false 일 때 콘텐츠는 감추되 배경은 유지(레거시 LiveClear).
    // IsBlackout(완전 검정)과 상호 배타. 기본 false. 출력 렌더러가 Cleared 씬으로 해석한다.
    bool IsCleared = false,
    // 위치 라벨(예: "2/4") — 곡 절/PPT 슬라이드 위치. 단일이면 빈 문자열. 출력은 설정 on 일 때만 표시(§7.3-A).
    string CurrentItemPositionLabel = "",
    // 현재 가사 절 페이지 인덱스(0=첫 절). 제목 헤딩 "첫 화면만" 표시 판정에 쓰임(§7.3-A). 곡이 아니면 0.
    int CurrentLyricsPageIndex = 0)
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
    void ClearOutput();
    void Restore();
    void Refresh();
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
            pixelHeight,
            // 곡 항목이면 현재 절(LyricsPageIndex)을, 그 외(PPT/미디어 등)는 빈 본문을 싣는다.
            // 원시 가사의 작성 마커( [1], [~코드] 등 )는 회중 화면에 보이면 안 되므로 표시용으로 정리한다.
            // LyricsPageIndex=0 이면 첫 절, GoLive 호출 전에 MainViewModel 이 현재 페이지를 얹어 전달한다.
            CurrentItemBodyText: LyricsDisplayFormatter.GetVersePage(item.Lyrics, item.LyricsPageIndex, item.Sequence),
            CurrentItemPositionLabel: item.PositionLabel,
            // 현재 절 인덱스도 실어 출력 렌더러가 "제목 헤딩 첫 화면만" 표시를 판정하게 한다(§7.3-A).
            CurrentLyricsPageIndex: item.LyricsPageIndex));
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
            // Black/Hide 는 Clear 와 상호 배타 — 전환 시 비우기 플래그를 끈다.
            IsCleared = false,
        });
    }

    // 화면 비우기(레거시 LiveClear) — 콘텐츠는 감추되 배경은 유지(완전 검정인 Black 과 구별).
    // State=Hidden + IsCleared=true 로 두어 출력 렌더러가 Cleared 씬(배경만)으로 해석한다.
    // 콘텐츠(가사·슬라이드 등)는 보존하므로 Restore 시 직전 항목이 그대로 다시 보인다.
    public void ClearOutput()
    {
        if (Current.State == LiveState.Off)
        {
            return;
        }

        Update(Current with
        {
            State = LiveState.Hidden,
            IsBlackout = false,
            IsCleared = true,
        });
    }

    // 숨김/블랙아웃/비우기에서 송출 화면을 되살린다 — 콘텐츠(가사·슬라이드)는 보존되므로
    // 상태만 Active 로 되돌리고 블랙아웃·비우기 플래그를 해제하면 직전 항목이 그대로 다시 보인다.
    public void Restore()
    {
        if (Current.State != LiveState.Hidden)
        {
            return;
        }

        Update(Current with
        {
            State = LiveState.Active,
            IsBlackout = false,
            IsCleared = false,
        });
    }

    // 출력 강제 재렌더(레거시 RefreshOutput) — 스냅샷이 바뀌지 않아도 현재 상태로 SessionChanged 를
    // 다시 발생시켜 출력 창이 씬을 다시 그리게 한다(디스플레이 글리치·설정 변경 후 복구용).
    // Update 의 동등성 가드를 우회해야 하므로 이벤트를 직접 발생시킨다.
    public void Refresh()
        => SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));

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
