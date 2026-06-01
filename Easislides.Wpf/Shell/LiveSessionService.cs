using System;
using System.Collections.Generic;
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
    // 이중 언어 곡에서는 이 값이 Region1(주 언어), CurrentItemBodyText2 가 Region2(보조 언어)다.
    string CurrentItemBodyText = "",
    // 이중 언어([region 2]) 곡의 Region2(보조 언어) 본문. 단일 영역 곡·비곡은 빈 문자열(무회귀).
    string CurrentItemBodyText2 = "",
    // 비우기(Clear) 모드 — State=Hidden 이면서 IsBlackout=false 일 때 콘텐츠는 감추되 배경은 유지(레거시 LiveClear).
    // IsBlackout(완전 검정)과 상호 배타. 기본 false. 출력 렌더러가 Cleared 씬으로 해석한다.
    bool IsCleared = false,
    // 위치 라벨(예: "2/4") — 곡 절/PPT 슬라이드 위치. 단일이면 빈 문자열. 출력은 설정 on 일 때만 표시(§7.3-A).
    string CurrentItemPositionLabel = "",
    // 현재 가사 절 페이지 인덱스(0=첫 절). 제목 헤딩 "첫 화면만" 표시 판정에 쓰임(§7.3-A). 곡이 아니면 0.
    int CurrentLyricsPageIndex = 0,
    // 현재 항목의 곡 번호(출력 "곡 번호 표시" 설정 on 일 때 표시). 곡이 아니거나 번호가 없으면 0.
    int CurrentItemNumber = 0,
    // 현재 항목의 저작권(출력 "저작권 표시" 설정 on 일 때 표시). 곡이 아니거나 저작권이 없으면 빈 문자열.
    string CurrentItemCopyright = "",
    // 다음 예배순서 항목 제목(출력 "다음 항목 표시" 설정 on 일 때 표시). 마지막 항목이면 빈 문자열.
    string CurrentItemNextTitle = "",
    // 곡별 FormatData(레거시 v32)에서 디코드한 region1 글자색(ARGB). 있으면 출력 렌더러가 운영 기본 글자색 대신 사용.
    // FormatData 가 없거나 글자색 항목(29)이 없으면 null → 기본색 유지(무회귀).
    int? OverrideTextColorArgb = null,
    // 곡별 FormatData region2 글자색(30). 이중 언어 곡의 Region2 본문 색. 없으면 null → Region1 색(또는 기본색) 사용.
    int? OverrideTextColorArgb2 = null,
    // 곡별 FormatData region1 배경색(ARGB). 있으면 운영 기본 배경색 대신 사용(솔리드). 없으면 null.
    int? OverrideBackgroundColorArgb = null,
    // 곡별 FormatData region1 가로 정렬(31). 있으면 운영 기본 정렬 대신 사용. 없으면 null.
    LyricsTextAlignment? OverrideTextAlignment = null,
    // 곡별 FormatData region2 가로 정렬(32). 이중 언어 Region2 본문 정렬. 없으면 null → Region1 정렬 추종.
    LyricsTextAlignment? OverrideTextAlignment2 = null,
    // 곡별 FormatData region1 폰트명(43). 있으면 운영/테마 기본 글꼴 대신 사용. 비었으면 null → 기본 글꼴 상속(무회귀).
    string? OverrideFontName = null,
    // 곡별 FormatData region1 폰트 크기(47). 레거시 pt 를 WPF px(DIP)로 변환해 싣는다. 없으면 null → 기본 크기 유지.
    int? OverrideFontSizePx = null,
    // 곡별 FormatData region2 폰트명(44)·크기(48). 이중 언어 Region2 본문 글꼴. 없으면 null → Region1 글꼴 추종.
    string? OverrideFontName2 = null,
    int? OverrideFontSizePx2 = null,
    // 곡별 FormatData 배경 이미지 경로(61). 있으면 출력이 색 배경 대신(위에) 이미지를 표시. 없으면 null → 색 배경 유지.
    // 경로 해석·이미지 로딩은 출력 VM 이 담당(렌더러는 순수). 비었으면 null.
    string? OverrideBackgroundImagePath = null,
    // 곡별 FormatData 글꼴 효과 비트(41) — region1/region2 굵게·기울임을 영역별로 출력에 적용한다.
    // null = 설정(전역) 추종, true = 그 영역만 굵게/기울임. 레거시 비트는 "켜기"만 표현하므로 "끄기"는 전역을 따른다.
    // Region2 값이 null 이면 Region1 효과를 추종한다(색·정렬·글꼴 추종과 동일 규칙, 해석은 OutputRenderer 가 담당).
    bool? OverrideBold1 = null,
    bool? OverrideItalic1 = null,
    bool? OverrideBold2 = null,
    bool? OverrideItalic2 = null,
    // 밑줄 비트(41 bit2/bit5) — null=전역(Region1)/Region1 추종, true=그 영역 밑줄.
    bool? OverrideUnderline1 = null,
    bool? OverrideUnderline2 = null,
    // 현재 송출 중인 절이 후렴([C]/[Chorus]/[후렴])인지 — "강조 후렴만"(ChorusOnly) 효과 판정용. 곡이 아니거나 라벨 없으면 false.
    bool CurrentPageIsChorus = false)
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
            // 이중 언어([region 2]) 곡이면 Region1·Region2 를 각각 싣는다(출력이 영역별 스타일로 동시 송출).
            CurrentItemBodyText: ComputeBodyText(item),
            CurrentItemBodyText2: ComputeBodyText2(item),
            CurrentItemPositionLabel: item.PositionLabel,
            // 현재 절 인덱스도 실어 출력 렌더러가 "제목 헤딩 첫 화면만" 표시를 판정하게 한다(§7.3-A).
            CurrentLyricsPageIndex: item.LyricsPageIndex,
            // 곡 번호(출력 "곡 번호 표시" 설정 on 일 때 표시).
            CurrentItemNumber: item.SongNumber,
            // 저작권(출력 "저작권 표시" 설정 on 일 때 표시).
            CurrentItemCopyright: item.Copyright,
            // 다음 항목 제목(출력 "다음 항목 표시" 설정 on 일 때 표시).
            CurrentItemNextTitle: item.NextTitle,
            // 곡별 색(있으면) — 렌더러가 Live 일 때 운영 기본색 대신 적용.
            OverrideTextColorArgb: format?.TextColorArgb1,
            // 이중 언어 Region2 글자색(30) — 있으면 Region2 본문에 적용(없으면 Region1 색 추종).
            OverrideTextColorArgb2: format?.TextColorArgb2,
            OverrideBackgroundColorArgb: format?.BackgroundColorArgb1,
            // 곡별 가로 정렬(있으면) — 레거시 1=왼쪽/2=가운데/3=오른쪽.
            OverrideTextAlignment: MapAlignment(format?.Alignment1),
            // region2 정렬(32) — 이중 언어 Region2 본문에 적용(없으면 Region1 정렬 추종).
            OverrideTextAlignment2: MapAlignment(format?.Alignment2),
            // 곡별 글꼴명(43)·크기(47, region1). 폰트명은 비면 null(테마 상속), 크기는 레거시 pt→px 변환.
            OverrideFontName: NormalizeFontName(format?.FontName1),
            OverrideFontSizePx: LegacyPointToPixel(format?.FontSize1),
            // region2 글꼴명(44)·크기(48) — 이중 언어 Region2 본문에 적용(없으면 Region1 글꼴 추종).
            OverrideFontName2: NormalizeFontName(format?.FontName2),
            OverrideFontSizePx2: LegacyPointToPixel(format?.FontSize2),
            // 곡별 배경 이미지(61). 비었으면 null(색 배경 유지). 경로 해석·로딩은 출력 VM 이 담당.
            OverrideBackgroundImagePath: NormalizeImagePath(format?.BackgroundImagePath),
            // 굵게/기울임 비트(41) — 켜진 영역만 true, 없으면 null(전역/Region1 추종). 출력이 영역별로 적용한다.
            OverrideBold1: format?.Bold1 == true ? true : null,
            OverrideItalic1: format?.Italic1 == true ? true : null,
            OverrideBold2: format?.Bold2 == true ? true : null,
            OverrideItalic2: format?.Italic2 == true ? true : null,
            OverrideUnderline1: format?.Underline1 == true ? true : null,
            OverrideUnderline2: format?.Underline2 == true ? true : null,
            // 현재 절이 후렴인지 — 강조 후렴만 효과가 켜졌을 때 출력 렌더가 이 절에 강조를 적용할지 판단한다.
            CurrentPageIsChorus: ComputeCurrentPageIsChorus(item)));
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

    // 현재 절의 Region1(주 언어) 본문. 단일 영역 곡은 기존 경로(Sequence 적용 GetVersePage)를 그대로 써 비트 동일(무회귀).
    // 이중 언어 곡은 영역-인식 GetRegionPage 의 Region1. 곡이 아니면 빈 문자열.
    // 주의: 이중 언어 경로는 현재 Sequence 를 무시하고 선형 GetRegionPages 를 쓴다(count·body 동일 소스로 정렬 보장).
    //       이중 언어 + Sequence 동시 지원·절 라벨 점프는 차기 슬라이스.
    private static string ComputeBodyText(LiveQueueItem item)
    {
        // 공지(InfoScreen)는 자유 텍스트라 가사 작성 마커([광고]·»·빈줄 등)를 해석하면 안 된다 —
        // 가사 포맷터를 건너뛰고 입력 그대로 본문으로 송출한다(마커 손상·빈 화면 false-positive 방지).
        if (item.Kind == LiveItemKinds.Notice)
        {
            return item.Lyrics ?? string.Empty;
        }

        // 성경 본문은 가사 코드 마커(») 규약을 쓰지 않는다 — 코드 전처리(ExpandNotations)를 건너뛰고,
        // 일부 번역의 인용부호 »…« 가 코드 마커로 오인돼 잘리지 않게 보호한 채 절만 나눈다.
        if (item.Kind == LiveItemKinds.Bible)
        {
            return ComputeBibleBody(item.Lyrics, item.LyricsPageIndex, region2: false);
        }

        // "코드 표시" on 이면 가사 위에 코드 줄을 끼운 전처리 가사를, off 면 원시 가사 그대로(무회귀)를 쓴다.
        var lyrics = LyricsDisplayFormatter.ExpandNotations(item.Lyrics, item.ShowNotations, item.TransposeSemitones);
        return LyricsDisplayFormatter.HasRegion2(lyrics)
            ? LyricsDisplayFormatter.GetRegionPage(lyrics, item.LyricsPageIndex, item.Sequence).Region1
            : LyricsDisplayFormatter.GetVersePage(lyrics, item.LyricsPageIndex, item.Sequence);
    }

    // 성경 한 절(페이지) 본문을 안전하게 추출한다. 성경은 코드 마커(») 규약을 안 쓰므로 코드 전처리를 건너뛰고,
    // 절 분할(StripInlineNotation 포함) 동안만 '»'를 사적 영역 문자로 보호했다가 결과에서 되돌린다 — 번역 인용부호 보존.
    // region2=true 면 이중 언어 본문의 보조 언어 절을, false 면 주 언어 절을 돌려준다.
    private static string ComputeBibleBody(string? body, int pageIndex, bool region2)
    {
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        // 먼저 정규화(줄바꿈·"Â»"→"»" 모지바케 정리)한 뒤 '»'를 보호한다 — 보호를 먼저 하면 모지바케 정리가
        // '»'를 못 찾아 'Â' 가 본문에 남는다(정규화 → 보호 순서라야 송출 본문이 깨끗).
        var guarded = LyricsDisplayFormatter.GuardLiteralNotation(LyricsDisplayFormatter.NormalizeText(body));
        string page;
        if (LyricsDisplayFormatter.HasRegion2(guarded))
        {
            var regionPage = LyricsDisplayFormatter.GetRegionPage(guarded, pageIndex);
            page = region2 ? regionPage.Region2 : regionPage.Region1;
        }
        else
        {
            page = region2 ? string.Empty : LyricsDisplayFormatter.GetVersePage(guarded, pageIndex);
        }

        return LyricsDisplayFormatter.UnguardLiteralNotation(page);
    }

    // 현재 절의 Region2(보조 언어) 본문 — 이중 언어 곡일 때만. 단일 영역·비곡은 빈 문자열.
    // Region1 과 동일 인덱스·Sequence(GetRegionPage)라 두 영역이 같은 절로 짝지어진다.
    private static string ComputeBodyText2(LiveQueueItem item)
    {
        if (item.Kind == LiveItemKinds.Notice)
        {
            return string.Empty;
        }

        // 성경은 코드 전처리를 건너뛰고 보조 언어(Region2) 절을 인용부호 보호한 채 추출(주 언어 경로와 대칭).
        if (item.Kind == LiveItemKinds.Bible)
        {
            return ComputeBibleBody(item.Lyrics, item.LyricsPageIndex, region2: true);
        }

        var lyrics = LyricsDisplayFormatter.ExpandNotations(item.Lyrics, item.ShowNotations, item.TransposeSemitones);
        return LyricsDisplayFormatter.HasRegion2(lyrics)
            ? LyricsDisplayFormatter.GetRegionPage(lyrics, item.LyricsPageIndex, item.Sequence).Region2
            : string.Empty;
    }

    // 현재 송출 절이 후렴인지 — "강조 후렴만"(ChorusOnly) 효과 판정용. 곡이 아니거나 가사·라벨이 없으면 false.
    // 절 라벨은 MainViewModel 의 절 라벨 계산과 같은 포맷터 경로(단일=GetSectionLabels, 이중 언어=GetRegionSectionLabels)를 쓴다.
    private static bool ComputeCurrentPageIsChorus(LiveQueueItem item)
    {
        if (item.Kind != LiveItemKinds.Song || string.IsNullOrEmpty(item.Lyrics))
        {
            return false;
        }

        IReadOnlyList<string> labels;
        if (LyricsDisplayFormatter.HasRegion2(item.Lyrics))
        {
            // 이중 언어 라벨(GetRegionSectionLabels)은 라벨 없는 머리말 절을 빠뜨려 페이지와 어긋날 수 있다.
            // 페이지 수와 1:1 정렬될 때만 신뢰한다(어긋나면 후렴 판정 비활성 — 엉뚱한 절 강조 방지).
            // MainViewModel.RefreshLyricsPages 의 절 점프 바 가드와 동일 규칙.
            var regionLabels = LyricsDisplayFormatter.GetRegionSectionLabels(item.Lyrics, item.Sequence);
            var pageCount = LyricsDisplayFormatter.GetRegionPages(item.Lyrics, item.Sequence).Count;
            labels = regionLabels.Count == pageCount ? regionLabels : System.Array.Empty<string>();
        }
        else
        {
            // 단일 영역 라벨은 ToVersePages 와 1:1 정렬이 보장된다(라벨 없는 절은 빈 문자열 → 후렴 아님).
            labels = LyricsDisplayFormatter.GetSectionLabels(item.Lyrics, item.Sequence);
        }

        var index = item.LyricsPageIndex;
        return index >= 0 && index < labels.Count && LyricsDisplayFormatter.IsChorusLabel(labels[index]);
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
