using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;

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
    int CurrentLyricsPageIndex = 0,
    // 곡별 FormatData(레거시 v32)에서 디코드한 region1 글자색(ARGB). 있으면 출력 렌더러가 운영 기본 글자색 대신 사용.
    // FormatData 가 없거나 글자색 항목(29)이 없으면 null → 기본색 유지(무회귀).
    int? OverrideTextColorArgb = null,
    // 곡별 FormatData region1 배경색(ARGB). 있으면 운영 기본 배경색 대신 사용(솔리드). 없으면 null.
    int? OverrideBackgroundColorArgb = null,
    // 곡별 FormatData region1 가로 정렬(31). 있으면 운영 기본 정렬 대신 사용. 없으면 null.
    LyricsTextAlignment? OverrideTextAlignment = null,
    // 곡별 FormatData region1 폰트명(43). 있으면 운영/테마 기본 글꼴 대신 사용. 비었으면 null → 기본 글꼴 상속(무회귀).
    string? OverrideFontName = null,
    // 곡별 FormatData region1 폰트 크기(47). 레거시 pt 를 WPF px(DIP)로 변환해 싣는다. 없으면 null → 기본 크기 유지.
    int? OverrideFontSizePx = null,
    // 곡별 FormatData 배경 이미지 경로(61). 있으면 출력이 색 배경 대신(위에) 이미지를 표시. 없으면 null → 색 배경 유지.
    // 경로 해석·이미지 로딩은 출력 VM 이 담당(렌더러는 순수). 비었으면 null.
    string? OverrideBackgroundImagePath = null)
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
        // 곡별 FormatData(레거시 v32)를 디코드해 region1 글자·배경색을 오버라이드로 싣는다(미리보기와 동일 규약).
        // 곡이 아니거나 색 항목이 없으면 null → 출력은 운영 기본색을 그대로 쓴다.
        var format = Library.SongFormatData.Parse(item.FormatData);
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
            CurrentLyricsPageIndex: item.LyricsPageIndex,
            // 곡별 색(있으면) — 렌더러가 Live 일 때 운영 기본색 대신 적용.
            OverrideTextColorArgb: format?.TextColorArgb1,
            OverrideBackgroundColorArgb: format?.BackgroundColorArgb1,
            // 곡별 가로 정렬(있으면) — 레거시 1=왼쪽/2=가운데/3=오른쪽. region1 한정
            // (region2 정렬(32)은 현재 출력이 단일 정렬 모델이라 미적용 — 이중 언어 출력 도입 시 확장).
            OverrideTextAlignment: MapAlignment(format?.Alignment1),
            // 곡별 글꼴명(43)·크기(47, region1). 폰트명은 비면 null(테마 상속), 크기는 레거시 pt→px 변환.
            OverrideFontName: NormalizeFontName(format?.FontName1),
            OverrideFontSizePx: LegacyPointToPixel(format?.FontSize1),
            // 곡별 배경 이미지(61). 비었으면 null(색 배경 유지). 경로 해석·로딩은 출력 VM 이 담당.
            OverrideBackgroundImagePath: NormalizeImagePath(format?.BackgroundImagePath)));
    }

    // 곡별 배경 이미지 경로 정리 — 공백뿐이거나 비었으면 null(색 배경 유지). 앞뒤 공백 제거.
    private static string? NormalizeImagePath(string? imagePath)
        => string.IsNullOrWhiteSpace(imagePath) ? null : imagePath.Trim();

    // 곡별 폰트명 정리 — 공백뿐이거나 비었으면 null(운영/테마 기본 글꼴 상속). 앞뒤 공백 제거.
    private static string? NormalizeFontName(string? fontName)
        => string.IsNullOrWhiteSpace(fontName) ? null : fontName.Trim();

    // 레거시 폰트 크기(pt, WinForms 기본 단위)를 WPF px(DIP, 1/96")로 변환한다.
    // 1pt = 1/72", 1px(DIP) = 1/96" → px = pt × 96/72. 없으면 null(기본 크기 유지).
    // 디코더가 이미 6~100pt 로 검증하므로 변환 결과는 8~133px 범위 — 그래도 방어적으로 클램프.
    private static int? LegacyPointToPixel(int? legacyPoint)
    {
        if (legacyPoint is not int pt)
        {
            return null;
        }

        var px = (int)Math.Round(pt * 96.0 / 72.0, MidpointRounding.AwayFromZero);
        return Math.Clamp(px, 8, 240);
    }

    // 레거시 FormatData 정렬값(1~3) → WPF 가사 정렬 enum. 없거나 범위 밖이면 null(운영 기본 정렬 유지).
    private static LyricsTextAlignment? MapAlignment(int? legacyAlign)
        => legacyAlign switch
        {
            1 => LyricsTextAlignment.Left,   // Near
            2 => LyricsTextAlignment.Center,
            3 => LyricsTextAlignment.Right,  // Far
            _ => null,
        };

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
