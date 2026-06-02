using System;
using System.IO;
using System.Windows;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Rendering;

public enum OutputSceneKind
{
    Standby,
    Ready,
    Live,
    Hidden,
    Blackout,
    // 비우기(레거시 LiveClear) — 콘텐츠는 감추되 배경은 유지. Blackout(완전 검정)과 구별.
    Cleared
}

public sealed record LiveOutputRenderSettings(
    bool ShowLyricsMonitorAlertBox = false,
    bool AdvanceNextItem = false,
    GapItemMode GapItemOption = GapItemMode.None,
    string GapItemLogoFile = "",
    bool GapItemUseFade = true,
    int LyricsMonitorTextColorArgb = -16777216,
    int LyricsMonitorBackgroundColorArgb = -1,
    // 배경 그라데이션 끝색(ARGB). IsGradient=true 일 때 배경색→이 색 세로 그라데이션(FrmBackground 슬라이스 / G2).
    int LyricsMonitorBackgroundColor2Argb = -1,
    // 배경 그라데이션 사용 여부(기본 false=솔리드).
    bool LyricsMonitorBackgroundIsGradient = false,
    // 주의: "코드 표시"(LyricsMonitorShowNotations)는 렌더 설정이 아니다 — 본문 텍스트 자체(코드 줄 포함 여부)를
    // 바꾸므로 라이브 본문 계산(LiveSessionService.ComputeBodyText) 단계에서 처리한다. 여기(씬 렌더 설정)엔 두지 않는다.
    bool NoPowerPointPanelOverlay = false,
    bool NoMediaPanelOverlay = false,
    // 출력 가사 가로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center.
    LyricsTextAlignment LyricsMonitorTextAlignment = LyricsTextAlignment.Center,
    // 출력 가사 세로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center.
    LyricsVerticalAlignment LyricsMonitorVerticalAlignment = LyricsVerticalAlignment.Center,
    // 출력 가사 폰트 크기(px, 인-셸 가사 포맷팅 §7.3-A). 기본 48.
    int LyricsMonitorFontSize = 48,
    // 보조 영역(Region2) 전역 폰트 크기(px, FrmMain Ind_Reg2SizeUpDown). 기본 0=본문(Region1)과 동일(무회귀).
    int LyricsMonitorFontSize2 = 0,
    // 출력 가사 폰트 효과(인-셸 가사 포맷팅 §7.3-A). 모두 기본 off.
    bool LyricsMonitorBold = false,
    bool LyricsMonitorItalic = false,
    bool LyricsMonitorShadow = false,
    bool LyricsMonitorUnderline = false,
    // Display Panel 배경 투명(Def_PanelTransparent). 기본 false=반투명 밴드(무회귀).
    bool LyricsMonitorPanelTransparent = false,
    // Display Panel 밴드 배경색(ARGB, Def_PanelColour). 기본 0x66000000=반투명 검정(무회귀).
    int LyricsMonitorPanelColorArgb = unchecked((int)0x66000000),
    // Display Panel 정보 텍스트 글자 크기 비율(%, Def_PanelFont 크기). 기본 100=기존 크기(무회귀).
    int LyricsMonitorPanelFontScalePercent = 100,
    // 출력 가사 줄 간격(폰트 대비 %, 인-셸 가사 포맷팅 §7.3-A). 기본 125.
    int LyricsMonitorLineSpacingPercent = 125,
    // 출력 본문 좌/우/아래 여백(px) — FrmMain ShowLeftMargin/Right/Bottom. 기본 0=기존 레이아웃(무회귀).
    int LyricsMonitorBodyLeftMargin = 0,
    int LyricsMonitorBodyRightMargin = 0,
    int LyricsMonitorBodyBottomMargin = 0,
    // 이중 언어 Region1↔Region2 세로 간격(px, FrmMain Ind_Reg2TopUpDown). 기본 8=기존 간격(무회귀).
    int LyricsMonitorRegionGapPx = 8,
    // 본문 세로 위치 오프셋(px, FrmMain Ind_Reg1TopUpDown). 음수=위·양수=아래. 기본 0=이동 없음(무회귀).
    int LyricsMonitorBodyVerticalOffset = 0,
    // 출력 위치 인디케이터(절/슬라이드 "N/M") 표시 여부(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsPositionIndicator = false,
    // 절 헤딩(현재 절의 섹션 라벨 "1절"/"후렴") 표시 여부(FrmMain Def_Head All). 기본 off.
    bool ShowLyricsVerseHeading = false,
    // 출력 제목 헤딩(가사 위 상단 배너로 곡 제목) 표시 여부(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsTitleHeading = false,
    // 출력 가사 외곽선(Outline Font) 효과 여부(인-셸 §7.3-A 폰트 효과). 기본 off.
    bool ShowLyricsOutline = false,
    // 출력 제목 헤딩 가로 정렬(인-셸 §7.3-A Heading Align). 기본 Center.
    LyricsTextAlignment LyricsMonitorTitleHeadingAlignment = LyricsTextAlignment.Center,
    // 제목 헤딩이 본문(Region1) 정렬을 따름(FrmMain Def_HeadAlign AsR1). 기본 off → 헤딩 전용 정렬 사용.
    bool LyricsMonitorTitleHeadingFollowBody = false,
    // 제목 헤딩이 보조 영역(Region2) 정렬을 따름(FrmMain Def_HeadAlign AsR2). 기본 off. AsR1 보다 우선.
    bool LyricsMonitorTitleHeadingFollowRegion2 = false,
    // 제목 헤딩을 곡 첫 절(첫 화면)에만 표시(인-셸 §7.3-A Heading At First Screen Only). 기본 off.
    bool TitleHeadingFirstScreenOnly = false,
    // 출력에 곡 번호 표시(FrmMain Show Item Number, Display Panel). 기본 off → 기존 출력 무변화.
    bool LyricsMonitorShowItemNumber = false,
    // 정보 패널에 곡 제목 표시(FrmMain Def_PanelTitle). 기본 off → 기존 출력 무변화.
    bool LyricsMonitorShowTitleOnPanel = false,
    // 출력에 저작권 표시(FrmMain Show Copyright Information, Display Panel). 기본 off.
    bool LyricsMonitorShowCopyright = false,
    // 출력에 다음 항목 표시(FrmMain Display Panel PrevNext). 기본 off.
    bool LyricsMonitorShowNextItem = false,
    // 출력 전역 배경 이미지 경로(FrmMain Images 탭). 비었으면 색 배경. 곡별 배경(61)이 우선.
    string LyricsMonitorBackgroundImagePath = "",
    // 출력 배경 이미지 표시 모드(FrmMain Def_ImageMode: Tile/Centre/BestFit). 기본 Fill=UniformToFill(무회귀).
    LyricsBackgroundMode LyricsMonitorBackgroundMode = LyricsBackgroundMode.Fill,
    // 출력 배경 2색 그라데이션 방향(FrmMain Def_BackColour 패턴). 기본 Vertical=위→아래(무회귀).
    LyricsGradientDirection LyricsMonitorBackgroundGradientDirection = LyricsGradientDirection.Vertical,
    // 이중 언어 영역 표시 모드(FrmMain Def_ShowRegion1/2/Both). 기본 Both=둘 다(무회귀).
    LyricsRegionDisplay LyricsMonitorRegionDisplay = LyricsRegionDisplay.Both,
    // 강조(굵게·기울임·밑줄)를 후렴 절에만 적용(FrmMain Ind_*Italics 후렴만). 기본 false=전체 절(무회귀).
    bool LyricsMonitorEmphasisChorusOnly = false,
    // 이중 언어 줄 교차(FrmMain Def_Interlace). 기본 false=영역별 블록(무회귀).
    bool LyricsMonitorInterlace = false,
    // 출력 가사 전역 글꼴명(FrmMain Def_FontName). 비었으면 테마 기본 글꼴 상속(무회귀). 곡별 글꼴(43)이 우선.
    string LyricsMonitorFontFamily = "",
    // 보조 영역(Region2) 전역 글꼴명(FrmMain Ind_Reg2Font). 비었으면 본문(Region1) 글꼴 추종(무회귀). 곡별 글꼴(44)이 우선.
    string LyricsMonitorFontFamily2 = "",
    // 보조 영역(Region2) 전역 글자색(ARGB). 0(투명)=본문(Region1) 색 추종(무회귀). 곡별 색(30)이 우선.
    int LyricsMonitorTextColor2Argb = 0,
    // 보조 영역(Region2) 전역 가로 정렬. FollowRegion1=본문 정렬 추종(무회귀). 곡별 정렬(32)이 우선.
    LyricsRegion2Alignment LyricsMonitorRegion2Alignment = LyricsRegion2Alignment.FollowRegion1,
    // 보조 영역(Region2) 전역 굵게(3-상태). FollowRegion1=본문 굵게 추종(무회귀). 곡별 굵게가 우선.
    LyricsRegion2Emphasis LyricsMonitorRegion2Bold = LyricsRegion2Emphasis.FollowRegion1,
    // 보조 영역(Region2) 전역 기울임(3-상태). FollowRegion1=본문 기울임 추종(무회귀). 곡별 기울임이 우선.
    LyricsRegion2Emphasis LyricsMonitorRegion2Italic = LyricsRegion2Emphasis.FollowRegion1)
{
    public static LiveOutputRenderSettings Default { get; } = new();

    public static LiveOutputRenderSettings From(ISettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new LiveOutputRenderSettings(
            settings.Get(EasiSettingKeys.ShowLyricsMonitorAlertBox),
            settings.Get(EasiSettingKeys.AdvanceNextItem),
            settings.Get(EasiSettingKeys.GapItemOption),
            settings.Get(EasiSettingKeys.GapItemLogoFile),
            settings.Get(EasiSettingKeys.GapItemUseFade),
            settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient),
            settings.Get(EasiSettingKeys.NoPowerPointPanelOverlay),
            settings.Get(EasiSettingKeys.NoMediaPanelOverlay),
            settings.Get(EasiSettingKeys.LyricsMonitorTextAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorVerticalAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorFontSize),
            settings.Get(EasiSettingKeys.LyricsMonitorFontSize2),
            settings.Get(EasiSettingKeys.LyricsMonitorBold),
            settings.Get(EasiSettingKeys.LyricsMonitorItalic),
            settings.Get(EasiSettingKeys.LyricsMonitorShadow),
            settings.Get(EasiSettingKeys.LyricsMonitorUnderline),
            settings.Get(EasiSettingKeys.LyricsMonitorPanelTransparent),
            settings.Get(EasiSettingKeys.LyricsMonitorPanelColorArgb),
            settings.Get(EasiSettingKeys.LyricsMonitorPanelFontScalePercent),
            settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent),
            settings.Get(EasiSettingKeys.LyricsMonitorBodyLeftMargin),
            settings.Get(EasiSettingKeys.LyricsMonitorBodyRightMargin),
            settings.Get(EasiSettingKeys.LyricsMonitorBodyBottomMargin),
            settings.Get(EasiSettingKeys.LyricsMonitorRegionGapPx),
            settings.Get(EasiSettingKeys.LyricsMonitorBodyVerticalOffset),
            settings.Get(EasiSettingKeys.LyricsMonitorShowPositionIndicator),
            settings.Get(EasiSettingKeys.LyricsMonitorShowVerseHeading),
            settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading),
            settings.Get(EasiSettingKeys.LyricsMonitorOutline),
            settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody),
            settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2),
            settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly),
            settings.Get(EasiSettingKeys.LyricsMonitorShowItemNumber),
            settings.Get(EasiSettingKeys.LyricsMonitorShowTitleOnPanel),
            settings.Get(EasiSettingKeys.LyricsMonitorShowCopyright),
            settings.Get(EasiSettingKeys.LyricsMonitorShowNextItem),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundImagePath),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundMode),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundGradientDirection),
            settings.Get(EasiSettingKeys.LyricsMonitorRegionDisplay),
            settings.Get(EasiSettingKeys.LyricsMonitorEmphasisChorusOnly),
            settings.Get(EasiSettingKeys.LyricsMonitorInterlace),
            settings.Get(EasiSettingKeys.LyricsMonitorFontFamily),
            settings.Get(EasiSettingKeys.LyricsMonitorFontFamily2),
            settings.Get(EasiSettingKeys.LyricsMonitorTextColor2Argb),
            settings.Get(EasiSettingKeys.LyricsMonitorRegion2Alignment),
            settings.Get(EasiSettingKeys.LyricsMonitorRegion2Bold),
            settings.Get(EasiSettingKeys.LyricsMonitorRegion2Italic));
    }
}

public sealed record OutputRenderRequest(
    LiveSessionSnapshot Session,
    OutputWindowState Output,
    int ViewportWidth,
    int ViewportHeight,
    int ContentPixelWidth = 0,
    int ContentPixelHeight = 0,
    ImageFillMode FillMode = ImageFillMode.Fit,
    TransitionEffectKind TransitionKind = TransitionEffectKind.None,
    TransitionActionKind TransitionAction = TransitionActionKind.None,
    TimeSpan TransitionDuration = default,
    TimeSpan TransitionElapsed = default,
    TransitionBackgroundMode BackgroundMode = TransitionBackgroundMode.BothBackgrounds,
    LiveOutputRenderSettings? LiveOutputSettings = null);

public sealed record OutputSceneSnapshot(
    OutputSceneKind Kind,
    string DisplayTitle,
    string StatusLabel,
    string OutputMonitorName,
    bool IsBlackout,
    bool IsOutputOpen,
    Rect Viewport,
    ImagePlacement ContentPlacement,
    TransitionEffectFrame TransitionFrame,
    bool ShowsLyricsAlertBox,
    int LyricsMonitorTextColorArgb,
    int LyricsMonitorBackgroundColorArgb,
    int LyricsMonitorBackgroundColor2Argb,
    bool LyricsMonitorBackgroundIsGradient,
    GapItemMode GapItemOption,
    string GapItemLogoFile,
    bool GapItemUseFade,
    bool ShowsPanelOverlay,
    // 라이브 곡 가사 본문(출력 중앙 텍스트). Live 가 아니면 빈 문자열이라 출력에 나타나지 않는다.
    string BodyText = "",
    // 출력 가사 가로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center.
    LyricsTextAlignment LyricsMonitorTextAlignment = LyricsTextAlignment.Center,
    // 출력 가사 세로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center.
    LyricsVerticalAlignment LyricsMonitorVerticalAlignment = LyricsVerticalAlignment.Center,
    // 출력 가사 폰트 크기(px, 인-셸 가사 포맷팅 §7.3-A). 기본 48.
    int LyricsMonitorFontSize = 48,
    // 출력 가사 폰트 효과(인-셸 가사 포맷팅 §7.3-A). 모두 기본 off.
    bool LyricsMonitorBold = false,
    bool LyricsMonitorItalic = false,
    bool LyricsMonitorShadow = false,
    bool LyricsMonitorUnderline = false,
    // Display Panel 배경 투명(Def_PanelTransparent). 기본 false=반투명 밴드(무회귀).
    bool LyricsMonitorPanelTransparent = false,
    // Display Panel 밴드 배경색(ARGB, Def_PanelColour). 기본 0x66000000=반투명 검정(무회귀).
    int LyricsMonitorPanelColorArgb = unchecked((int)0x66000000),
    // Display Panel 정보 텍스트 글자 크기 비율(%, Def_PanelFont 크기). 기본 100=기존 크기(무회귀).
    int LyricsMonitorPanelFontScalePercent = 100,
    // 출력 가사 줄 간격(폰트 대비 %, 인-셸 가사 포맷팅 §7.3-A). 기본 125.
    int LyricsMonitorLineSpacingPercent = 125,
    // 출력 본문 좌/우/아래 여백(px) — FrmMain ShowLeftMargin/Right/Bottom. 기본 0=기존 레이아웃(무회귀).
    int LyricsMonitorBodyLeftMargin = 0,
    int LyricsMonitorBodyRightMargin = 0,
    int LyricsMonitorBodyBottomMargin = 0,
    // 이중 언어 Region1↔Region2 세로 간격(px, FrmMain Ind_Reg2TopUpDown). 기본 8=기존 간격(무회귀).
    int LyricsMonitorRegionGapPx = 8,
    // 본문 세로 위치 오프셋(px, FrmMain Ind_Reg1TopUpDown). 음수=위·양수=아래. 기본 0=이동 없음(무회귀).
    int LyricsMonitorBodyVerticalOffset = 0,
    // 위치 라벨(절/슬라이드 "N/M"). Live 가 아니면 빈 문자열로 들어온다.
    string PositionLabel = "",
    // 위치 인디케이터 표시 설정(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsPositionIndicator = false,
    // 절 헤딩 라벨(현재 절의 섹션 라벨 "1절"/"후렴"). Live 가 아니면 빈 문자열로 들어온다.
    string VerseHeadingLabel = "",
    // 절 헤딩 표시 설정(FrmMain Def_Head All). 기본 off.
    bool ShowLyricsVerseHeading = false,
    // 제목 헤딩 표시 설정(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsTitleHeading = false,
    // 가사 외곽선(Outline Font) 효과 설정(인-셸 §7.3-A 폰트 효과). 기본 off.
    bool ShowLyricsOutline = false,
    // 제목 헤딩 가로 정렬(인-셸 §7.3-A Heading Align). 기본 Center.
    LyricsTextAlignment LyricsMonitorTitleHeadingAlignment = LyricsTextAlignment.Center,
    // 제목 헤딩을 첫 화면(첫 절)에만 표시하는 설정(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsTitleHeadingFirstScreenOnly = false,
    // 현재 가사 절 인덱스(0=첫 절) — 오직 ShowsTitleHeading "첫 화면만" 판정 전용(게이트 한정).
    // 비-Live(Cleared/Blackout 등)나 곡이 아니면 0 으로 들어오므로, 실제 세션 페이지 인덱스로 신뢰하지 말 것.
    int CurrentLyricsPageIndex = 0,
    // 출력 가사 글꼴명(곡별 FormatData 43 오버라이드). 비었으면 테마 기본 글꼴 상속(무회귀). §7.3-A per-song 폰트.
    string LyricsMonitorFontFamily = "",
    // 출력 배경 이미지 경로(곡별 FormatData 61 오버라이드). 비었으면 색 배경 유지. VM 이 로드해 색 배경 위에 표시.
    string BackgroundImagePath = "",
    // 출력 배경 이미지 표시 모드(채움/맞춤/가운데/타일). VM 이 이 값으로 ImageBrush 의 Stretch·TileMode 를 정한다.
    LyricsBackgroundMode BackgroundMode = LyricsBackgroundMode.Fill,
    // 출력 배경 2색 그라데이션 방향. VM 이 이 값으로 LinearGradientBrush 의 Start/End 점을 정한다.
    LyricsGradientDirection BackgroundGradientDirection = LyricsGradientDirection.Vertical,
    // 이중 언어([region 2]) 곡의 Region2(보조 언어) 본문. 단일 영역 곡은 빈 문자열 → Region2 미표시(무회귀).
    string BodyText2 = "",
    // Region2 본문 글자색(ARGB). 곡별 FormatData region2 색(30)이 없으면 Region1 색을 추종.
    int LyricsMonitorTextColor2Argb = -16777216,
    // Region2 본문 가로 정렬. 곡별 region2 정렬(32)이 없으면 Region1 정렬을 추종. 기본 Center.
    LyricsTextAlignment LyricsMonitorTextAlignment2 = LyricsTextAlignment.Center,
    // Region2 본문 글꼴명·크기. 곡별 region2 글꼴(44/48)이 없으면 Region1 글꼴을 추종.
    string LyricsMonitorFontFamily2 = "",
    int LyricsMonitorFontSize2 = 48,
    // 곡 번호 표시 설정(Display Panel). 기본 off.
    bool ShowLyricsItemNumber = false,
    // 정보 패널에 곡 제목 표시 설정(FrmMain Def_PanelTitle). 기본 off. 제목은 DisplayTitle 재사용.
    bool ShowLyricsTitleOnPanel = false,
    // 곡 번호 라벨(예: "123"). Live + 곡 번호>0 일 때만 채워진다.
    string ItemNumberLabel = "",
    // 저작권 표시 설정(Display Panel). 기본 off.
    bool ShowLyricsCopyright = false,
    // 저작권 라벨(CCLI 등). Live + 저작권 문자열이 있을 때만 채워진다.
    string CopyrightLabel = "",
    // 다음 항목 표시 설정(Display Panel PrevNext). 기본 off.
    bool ShowLyricsNextItem = false,
    // 다음 항목 제목 라벨. Live + 다음 항목이 있을 때만 채워진다.
    string NextItemLabel = "",
    // Region2(이중 언어) 굵게·기울임·밑줄 — 곡별 region2 비트(41)가 있으면 그것, 없으면 Region1 효과를 추종한다(CreateScene 해석).
    // Region1 굵게/기울임/밑줄은 기존 LyricsMonitorBold/Italic/Underline 에 해석돼 실린다(곡별 region1 비트가 전역을 덮어씀).
    bool LyricsMonitorBold2 = false,
    bool LyricsMonitorItalic2 = false,
    bool LyricsMonitorUnderline2 = false,
    // 이중 언어 영역 표시 모드(Both/Region1Only/Region2Only) — 어느 영역을 화면에 보일지. 기본 Both(무회귀).
    LyricsRegionDisplay LyricsMonitorRegionDisplay = LyricsRegionDisplay.Both,
    // 이중 언어 줄 교차(인터레이스) — on 이면 Region1·Region2 본문을 줄 단위로 번갈아 송출. 기본 false(영역별 블록).
    bool LyricsMonitorInterlace = false)
{
    public bool ShowsContent => Kind == OutputSceneKind.Live && ContentPlacement.Width > 0 && ContentPlacement.Height > 0;

    // 가사 본문을 실제로 송출할지 — Live 상태 + 본문이 있을 때만.
    // 영역 표시가 "Region2만"이면 Region1 을 숨긴다 — 단 보조 언어(Region2)가 있을 때만(없으면 화면이 비지 않게 Region1 유지).
    public bool ShowsBodyText => Kind == OutputSceneKind.Live
        && !string.IsNullOrWhiteSpace(BodyText)
        && !(LyricsMonitorRegionDisplay == LyricsRegionDisplay.Region2Only && !string.IsNullOrWhiteSpace(BodyText2));

    // 줄 교차(인터레이스)를 실제로 송출할지 — Live + 인터레이스 on + 두 영역(Region1·Region2)이 모두 보일 때만.
    // (한쪽만 보이는 영역 표시 모드/단일 언어에선 교차할 게 없으므로 false → 기존 블록 렌더.)
    public bool ShowsInterlace => LyricsMonitorInterlace && ShowsBodyText && ShowsBodyText2;

    // Region2(이중 언어 보조) 본문을 송출할지 — Live + Region2 본문이 있을 때만(단일 영역 곡은 false → 무회귀).
    // 영역 표시가 "Region1만"이면 Region2 를 숨긴다 — 단 주 언어(Region1)가 있을 때만(없으면 화면이 비지 않게 Region2 유지).
    // (ShowsBodyText 의 Region2Only 안전장치와 대칭 — 어느 모드에서도 내용 있는 곡을 빈 화면으로 만들지 않는다.)
    public bool ShowsBodyText2 => Kind == OutputSceneKind.Live
        && !string.IsNullOrWhiteSpace(BodyText2)
        && !(LyricsMonitorRegionDisplay == LyricsRegionDisplay.Region1Only && !string.IsNullOrWhiteSpace(BodyText));

    // 곡 번호를 실제로 노출할지 — 설정 on + Live + 번호 라벨이 있을 때만(Display Panel).
    public bool ShowsItemNumber => ShowLyricsItemNumber && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(ItemNumberLabel);

    // 정보 패널 곡 제목을 실제로 노출할지 — 설정 on + Live + 제목이 있을 때만(FrmMain Def_PanelTitle, 제목은 DisplayTitle).
    public bool ShowsTitleOnPanel => ShowLyricsTitleOnPanel && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(DisplayTitle);

    // 저작권을 실제로 노출할지 — 설정 on + Live + 저작권 문자열이 있을 때만(Display Panel).
    public bool ShowsCopyright => ShowLyricsCopyright && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(CopyrightLabel);

    // 다음 항목을 실제로 노출할지 — 설정 on + Live + 다음 항목 제목이 있을 때만(Display Panel PrevNext).
    public bool ShowsNextItem => ShowLyricsNextItem && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(NextItemLabel);

    // 위치 인디케이터를 실제로 노출할지 — 설정 on + Live + 라벨이 있을 때만.
    public bool ShowsPositionIndicator => ShowLyricsPositionIndicator && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(PositionLabel);

    // 절 헤딩을 실제로 노출할지 — 설정 on + Live + 섹션 라벨이 있을 때만(라벨 없는 절은 표시 안 함).
    public bool ShowsVerseHeading => ShowLyricsVerseHeading && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(VerseHeadingLabel);

    // 제목 헤딩을 실제로 노출할지 — 설정 on + 가사 본문 송출 중 + 제목이 있을 때만(가사 위 상단 배너).
    // 본문이 있을 때만 의미 있다(본문 없으면 기존 중앙 제목이 그대로 제목을 담당).
    // "첫 화면만"(FirstScreenOnly)이 켜져 있으면 곡 첫 절(CurrentLyricsPageIndex==0)에서만 표시(§7.3-A).
    public bool ShowsTitleHeading => ShowLyricsTitleHeading
        && ShowsBodyText
        && !string.IsNullOrWhiteSpace(DisplayTitle)
        && (!ShowLyricsTitleHeadingFirstScreenOnly || CurrentLyricsPageIndex == 0);

    // 외곽선 렌더러를 쓸지 — 설정 on + 가사 본문 송출 중일 때만(본문 없으면 의미 없음).
    // on 이면 일반 본문 TextBlock 대신 외곽선 렌더러를 쓰고, off 면 기존 본문 그대로(상호배타).
    public bool UsesBodyOutline => ShowLyricsOutline && ShowsBodyText;
}

public interface IOutputRenderer
{
    OutputSceneSnapshot CreateScene(OutputRenderRequest request);
}

public sealed class OutputRenderer : IOutputRenderer
{
    private readonly IImageAssetService _imageAssets;
    private readonly ITransitionEffectService _transitions;

    public OutputRenderer(IImageAssetService imageAssets, ITransitionEffectService transitions)
    {
        _imageAssets = imageAssets ?? throw new ArgumentNullException(nameof(imageAssets));
        _transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
    }

    // 보조 영역(Region2) 전역 정렬 enum → 본문에서 쓰는 LyricsTextAlignment 로 변환.
    // FollowRegion1 은 호출 측에서 이미 걸러지므로(추종 분기) 여기 들어오지 않지만, 안전하게 Center 로 둔다.
    private static LyricsTextAlignment MapRegion2Alignment(LyricsRegion2Alignment alignment) => alignment switch
    {
        LyricsRegion2Alignment.Left => LyricsTextAlignment.Left,
        LyricsRegion2Alignment.Right => LyricsTextAlignment.Right,
        _ => LyricsTextAlignment.Center,
    };

    // 보조 영역(Region2) 전역 강조 3-상태 → 실제 bool. FollowRegion1 이면 본문 효과(followValue)를 그대로,
    // On=true, Off=false 로 본문과 무관하게 적용. (정렬2 의 FollowRegion1 분기와 같은 "추종 센티넬" 개념.)
    private static bool ResolveRegion2Emphasis(LyricsRegion2Emphasis emphasis, bool followValue) => emphasis switch
    {
        LyricsRegion2Emphasis.On => true,
        LyricsRegion2Emphasis.Off => false,
        _ => followValue,
    };

    public OutputSceneSnapshot CreateScene(OutputRenderRequest request)
    {
        var viewportWidth = Math.Max(0, request.ViewportWidth);
        var viewportHeight = Math.Max(0, request.ViewportHeight);
        var liveOutput = request.LiveOutputSettings ?? LiveOutputRenderSettings.Default;
        var kind = GetSceneKind(request.Session, request.Output);
        var display = GetDisplayText(kind, request.Session, liveOutput);
        var placement = GetContentPlacement(kind, request, viewportWidth, viewportHeight);
        var transition = CreateTransitionFrame(request, viewportWidth, viewportHeight, kind, liveOutput);

        // 곡별 FormatData 색(있으면)은 Live 일 때만 운영 기본색을 이긴다(레거시 per-song 색).
        // 비-Live(숨김/블랙아웃/대기)에선 무시 → 운영 기본색 유지.
        var isLive = kind == OutputSceneKind.Live;
        var textColorArgb = isLive && request.Session.OverrideTextColorArgb is int songTextColor
            ? songTextColor
            : liveOutput.LyricsMonitorTextColorArgb;
        // 배경 오버라이드가 있으면 솔리드 단색으로 칠한다(끝색=시작색, 그라데이션 해제).
        var hasBgOverride = isLive && request.Session.OverrideBackgroundColorArgb is int;
        var bgColorArgb = hasBgOverride
            ? request.Session.OverrideBackgroundColorArgb!.Value
            : liveOutput.LyricsMonitorBackgroundColorArgb;
        var bgColor2Argb = hasBgOverride
            ? request.Session.OverrideBackgroundColorArgb!.Value
            : liveOutput.LyricsMonitorBackgroundColor2Argb;
        var bgIsGradient = !hasBgOverride && liveOutput.LyricsMonitorBackgroundIsGradient;
        // 곡별 가로 정렬(있으면)도 Live 일 때만 운영 기본 정렬을 이긴다.
        var textAlignment = isLive && request.Session.OverrideTextAlignment is LyricsTextAlignment songAlign
            ? songAlign
            : liveOutput.LyricsMonitorTextAlignment;
        // 곡별 폰트 크기(있으면)도 Live 일 때만 운영 기본 크기를 이긴다(레거시 pt→px 는 GoLive 에서 변환됨).
        var fontSizePx = isLive && request.Session.OverrideFontSizePx is int songFontPx
            ? songFontPx
            : liveOutput.LyricsMonitorFontSize;
        // 글꼴명 우선순위: 곡별 글꼴(43, Live 일 때만) > 전역 출력 글꼴(설정) > 빈 문자열(VM 이 테마 기본 글꼴 상속, 무회귀).
        var fontFamily = isLive && !string.IsNullOrWhiteSpace(request.Session.OverrideFontName)
            ? request.Session.OverrideFontName!
            : liveOutput.LyricsMonitorFontFamily;
        // 배경 이미지(Live 일 때만): 곡별 배경(FormatData 61)이 있으면 그 곡 동안 우선,
        // 없으면 전역 배경 이미지(Images 탭 설정)를 쓴다. 둘 다 없으면 빈 문자열 → 색 배경 유지(무회귀).
        var backgroundImagePath = !isLive
            ? ""
            : !string.IsNullOrWhiteSpace(request.Session.OverrideBackgroundImagePath)
                ? request.Session.OverrideBackgroundImagePath!
                : liveOutput.LyricsMonitorBackgroundImagePath;
        // Region2(이중 언어) 본문은 Live 일 때만 싣는다.
        var bodyText2 = isLive ? request.Session.CurrentItemBodyText2 : string.Empty;
        // Region2 색 우선순위: 곡별 region2 색(30, Live 한정) > 전역 region2 색(설정, 0=투명 아니면) > Region1 색 추종.
        // (글꼴2·크기2 의 "곡별 > 전역(0 아니면) > Region1 추종" 우선순위와 동일한 구조.)
        var textColor2Argb = isLive && request.Session.OverrideTextColorArgb2 is int songTextColor2
            ? songTextColor2
            : liveOutput.LyricsMonitorTextColor2Argb != 0
                ? liveOutput.LyricsMonitorTextColor2Argb
                : textColorArgb;
        // Region2 정렬 우선순위: 곡별 region2 정렬(32, Live 한정) > 전역 region2 정렬(설정, FollowRegion1 이 아니면) > Region1 정렬 추종.
        // (글꼴2·크기2·색2 의 "곡별 > 전역(추종 아니면) > Region1 추종" 우선순위와 동일한 구조.)
        var textAlignment2 = isLive && request.Session.OverrideTextAlignment2 is LyricsTextAlignment songAlign2
            ? songAlign2
            : liveOutput.LyricsMonitorRegion2Alignment != LyricsRegion2Alignment.FollowRegion1
                ? MapRegion2Alignment(liveOutput.LyricsMonitorRegion2Alignment)
                : textAlignment;
        // Region2 글꼴명 우선순위: 곡별 region2 글꼴(44, Live 한정) > 전역 region2 글꼴(설정, 비어있지 않으면) > Region1 글꼴 추종.
        // (폰트 크기2 의 "곡별 > 전역(0 아니면) > Region1 추종" 우선순위와 동일한 구조.)
        var fontFamily2 = isLive && !string.IsNullOrWhiteSpace(request.Session.OverrideFontName2)
            ? request.Session.OverrideFontName2!
            : !string.IsNullOrWhiteSpace(liveOutput.LyricsMonitorFontFamily2)
                ? liveOutput.LyricsMonitorFontFamily2
                : fontFamily;
        // Region2 폰트 크기 우선순위: 곡별 region2 크기(48) > 전역 region2 크기(설정, 0 이 아니면) > Region1 크기 추종.
        var fontSize2Px = isLive && request.Session.OverrideFontSizePx2 is int songFont2Px
            ? songFont2Px
            : liveOutput.LyricsMonitorFontSize2 > 0
                ? liveOutput.LyricsMonitorFontSize2
                : fontSizePx;
        // Region1 굵게/기울임 — 곡별 region1 비트(41)가 켜져 있으면 전역 설정을 덮어쓴다(없으면 전역 그대로).
        var region1Bold = isLive && request.Session.OverrideBold1 == true ? true : liveOutput.LyricsMonitorBold;
        var region1Italic = isLive && request.Session.OverrideItalic1 == true ? true : liveOutput.LyricsMonitorItalic;
        // Region2 굵게 우선순위: 곡별 region2 비트(Live 한정) > 전역 region2 굵게(설정, FollowRegion1 아니면 On/Off) > Region1 효과 추종.
        // (정렬2·색2·글꼴2 의 "곡별 > 전역(추종 아니면) > Region1 추종" 우선순위와 동일한 구조.)
        var region2Bold = isLive && request.Session.OverrideBold2 is bool songBold2
            ? songBold2
            : ResolveRegion2Emphasis(liveOutput.LyricsMonitorRegion2Bold, region1Bold);
        // Region2 기울임 우선순위도 굵게와 동일: 곡별 비트(Live 한정) > 전역(FollowRegion1 아니면) > Region1 추종.
        var region2Italic = isLive && request.Session.OverrideItalic2 is bool songItalic2
            ? songItalic2
            : ResolveRegion2Emphasis(liveOutput.LyricsMonitorRegion2Italic, region1Italic);
        // 밑줄도 동일 캐스케이드 — Region1=곡별 비트∥전역, Region2=곡별 비트∥Region1 추종.
        var region1Underline = isLive && request.Session.OverrideUnderline1 == true ? true : liveOutput.LyricsMonitorUnderline;
        var region2Underline = isLive ? request.Session.OverrideUnderline2 ?? region1Underline : region1Underline;
        // "강조 후렴만"이 켜져 있으면 후렴 절에서만 강조(굵게·기울임·밑줄)를 적용한다 — 그 외 절은 강조를 끈다.
        // off(기본)면 항상 적용(무회귀). 색·정렬은 강조가 아니므로 이 게이트와 무관하게 그대로 둔다.
        var emphasisActive = !liveOutput.LyricsMonitorEmphasisChorusOnly || (isLive && request.Session.CurrentPageIsChorus);
        if (!emphasisActive)
        {
            region1Bold = region1Italic = region1Underline = false;
            region2Bold = region2Italic = region2Underline = false;
        }

        return new OutputSceneSnapshot(
            kind,
            display.Title,
            display.Status,
            GetOutputMonitorName(request.Session, request.Output),
            kind == OutputSceneKind.Blackout,
            request.Output.IsOpen,
            CreateViewport(viewportWidth, viewportHeight),
            placement,
            transition,
            liveOutput.ShowLyricsMonitorAlertBox,
            textColorArgb,
            bgColorArgb,
            bgColor2Argb,
            bgIsGradient,
            liveOutput.GapItemOption,
            liveOutput.GapItemLogoFile,
            liveOutput.GapItemUseFade,
            ShouldShowPanelOverlay(kind, request.Session, liveOutput),
            // 곡 가사 본문은 Live 일 때만 송출(숨김/블랙아웃/대기 상태에선 빈 문자열).
            kind == OutputSceneKind.Live ? request.Session.CurrentItemBodyText : string.Empty,
            textAlignment,
            liveOutput.LyricsMonitorVerticalAlignment,
            fontSizePx,
            region1Bold,
            region1Italic,
            liveOutput.LyricsMonitorShadow,
            region1Underline,
            liveOutput.LyricsMonitorPanelTransparent,
            liveOutput.LyricsMonitorPanelColorArgb,
            liveOutput.LyricsMonitorPanelFontScalePercent,
            liveOutput.LyricsMonitorLineSpacingPercent,
            liveOutput.LyricsMonitorBodyLeftMargin,
            liveOutput.LyricsMonitorBodyRightMargin,
            liveOutput.LyricsMonitorBodyBottomMargin,
            liveOutput.LyricsMonitorRegionGapPx,
            liveOutput.LyricsMonitorBodyVerticalOffset,
            // 위치 라벨은 Live 일 때만 의미 있다(숨김/대기에선 빈 문자열).
            kind == OutputSceneKind.Live ? request.Session.CurrentItemPositionLabel : string.Empty,
            liveOutput.ShowLyricsPositionIndicator,
            // 절 헤딩 라벨도 Live 일 때만 의미 있다(섹션 라벨, 그 외엔 빈 문자열).
            kind == OutputSceneKind.Live ? request.Session.CurrentSectionLabel : string.Empty,
            liveOutput.ShowLyricsVerseHeading,
            liveOutput.ShowLyricsTitleHeading,
            liveOutput.ShowLyricsOutline,
            // 헤딩 정렬(FrmMain Def_HeadAlign): 우선순위 AsR2 > AsR1 > 헤딩 전용(L/C/R).
            // "보조 영역 따름"(AsR2) on 이면 Region2 가 실제 쓰는 정렬(textAlignment2, 곡별 override·Region1 폴백 포함),
            // 아니면 "본문 정렬 따름"(AsR1) on 일 때 Region1 정렬(textAlignment), 둘 다 off 면 헤딩 전용 정렬을 쓴다.
            liveOutput.LyricsMonitorTitleHeadingFollowRegion2 ? textAlignment2
                : liveOutput.LyricsMonitorTitleHeadingFollowBody ? textAlignment
                : liveOutput.LyricsMonitorTitleHeadingAlignment,
            liveOutput.TitleHeadingFirstScreenOnly,
            // 현재 절 인덱스는 Live 일 때만 의미 있다(그 외엔 0=첫 화면 취급).
            kind == OutputSceneKind.Live ? request.Session.CurrentLyricsPageIndex : 0,
            // 곡별 글꼴명(Live + 오버라이드 있을 때만). 비었으면 VM 이 테마 기본 글꼴을 상속(무회귀).
            fontFamily,
            // 곡별 배경 이미지 경로(Live + 오버라이드 있을 때만). 비었으면 VM 이 색 배경을 유지(무회귀).
            backgroundImagePath,
            // 배경 이미지 표시 모드(설정값) — 이미지가 있을 때 VM 이 이 값으로 ImageBrush 를 만든다.
            liveOutput.LyricsMonitorBackgroundMode,
            // 배경 그라데이션 방향(설정값) — VM 이 이 값으로 LinearGradientBrush 의 Start/End 를 정한다.
            liveOutput.LyricsMonitorBackgroundGradientDirection,
            // Region2(이중 언어) 본문·색·정렬·글꼴 — Live + 이중 언어 곡일 때만 채워진다.
            bodyText2,
            textColor2Argb,
            textAlignment2,
            fontFamily2,
            fontSize2Px,
            // 곡 번호 표시(설정) + 라벨(Live + 번호>0). 곡 번호 0이면 빈 문자열 → 미표시.
            liveOutput.LyricsMonitorShowItemNumber,
            liveOutput.LyricsMonitorShowTitleOnPanel,
            isLive && request.Session.CurrentItemNumber > 0 ? request.Session.CurrentItemNumber.ToString() : string.Empty,
            // 저작권 표시(설정) + 라벨(Live + 저작권 문자열). 비면 미표시.
            liveOutput.LyricsMonitorShowCopyright,
            isLive ? request.Session.CurrentItemCopyright : string.Empty,
            // 다음 항목 표시(설정) + 라벨(Live + 다음 항목 제목). 마지막 항목이면 빈 문자열 → 미표시.
            liveOutput.LyricsMonitorShowNextItem,
            isLive ? request.Session.CurrentItemNextTitle : string.Empty,
            // Region2 굵게·기울임·밑줄(해석된 값) — 이중 언어 보조 본문에 적용.
            region2Bold,
            region2Italic,
            region2Underline,
            // 이중 언어 영역 표시 모드(어느 영역을 보일지) — 설정값 그대로.
            liveOutput.LyricsMonitorRegionDisplay,
            // 줄 교차(인터레이스) 설정 — VM 이 두 영역 줄을 번갈아 배치할지 판단.
            liveOutput.LyricsMonitorInterlace);
    }

    private ImagePlacement GetContentPlacement(
        OutputSceneKind kind,
        OutputRenderRequest request,
        int viewportWidth,
        int viewportHeight)
    {
        if (kind != OutputSceneKind.Live || viewportWidth < 1 || viewportHeight < 1)
        {
            return ImagePlacement.Empty;
        }

        if (request.ContentPixelWidth < 1 || request.ContentPixelHeight < 1)
        {
            return new ImagePlacement(0, 0, viewportWidth, viewportHeight);
        }

        return _imageAssets.CalculatePlacement(
            request.ContentPixelWidth,
            request.ContentPixelHeight,
            viewportWidth,
            viewportHeight,
            request.FillMode);
    }

    private TransitionEffectFrame CreateTransitionFrame(
        OutputRenderRequest request,
        int viewportWidth,
        int viewportHeight,
        OutputSceneKind kind,
        LiveOutputRenderSettings settings)
    {
        var transitionKind = request.TransitionKind;
        var transitionAction = request.TransitionAction;
        var transitionDuration = request.TransitionDuration;

        if (kind == OutputSceneKind.Ready &&
            settings.GapItemOption != GapItemMode.None &&
            settings.GapItemUseFade &&
            transitionKind == TransitionEffectKind.None &&
            transitionAction == TransitionActionKind.None)
        {
            transitionKind = TransitionEffectKind.Fade;
            transitionAction = TransitionActionKind.AsStored;
            transitionDuration = TimeSpan.FromMilliseconds(500);
        }

        var plan = _transitions.CreatePlan(new TransitionEffectRequest(
            transitionKind,
            transitionAction,
            transitionDuration,
            request.BackgroundMode,
            viewportWidth,
            viewportHeight));

        return _transitions.GetFrame(plan, request.TransitionElapsed);
    }

    private static OutputSceneKind GetSceneKind(LiveSessionSnapshot session, OutputWindowState output)
    {
        if (session.State == LiveState.Hidden && session.IsBlackout)
        {
            return OutputSceneKind.Blackout;
        }

        // 비우기(배경 유지)는 검정보다 우선순위가 낮다 — Black 과 동시 설정될 수 없도록 세션이 보장하지만,
        // 방어적으로 Blackout 을 먼저 확인한 뒤 Cleared 를 판정한다.
        if (session.State == LiveState.Hidden && session.IsCleared)
        {
            return OutputSceneKind.Cleared;
        }

        if (session.State == LiveState.Hidden)
        {
            return OutputSceneKind.Hidden;
        }

        if (session.State == LiveState.Active)
        {
            return OutputSceneKind.Live;
        }

        return output.IsOpen ? OutputSceneKind.Ready : OutputSceneKind.Standby;
    }

    private static (string Title, string Status) GetDisplayText(
        OutputSceneKind kind,
        LiveSessionSnapshot session,
        LiveOutputRenderSettings settings)
        => kind switch
        {
            OutputSceneKind.Blackout => ("BLACK", "BLACKOUT"),
            OutputSceneKind.Hidden => ("HIDDEN", "HIDDEN"),
            // 비우기는 배경만 보이므로 타이틀은 비우고 상태 라벨만 둔다(패널 오버레이가 숨겨져 화면엔 안 보임).
            OutputSceneKind.Cleared => ("", "CLEARED"),
            OutputSceneKind.Live => (string.IsNullOrWhiteSpace(session.CurrentItemTitle) ? "LIVE" : session.CurrentItemTitle, "LIVE"),
            OutputSceneKind.Ready => GetReadyDisplayText(settings),
            _ => ("STANDBY", "STANDBY")
        };

    private static (string Title, string Status) GetReadyDisplayText(LiveOutputRenderSettings settings)
        => settings.GapItemOption switch
        {
            GapItemMode.Black => ("BLACK", "GAP"),
            GapItemMode.Default => ("OUTPUT READY", "GAP"),
            GapItemMode.User => (GetGapLogoTitle(settings.GapItemLogoFile), "GAP"),
            _ => ("OUTPUT READY", "READY")
        };

    private static string GetGapLogoTitle(string gapLogoFile)
    {
        var title = Path.GetFileNameWithoutExtension(gapLogoFile);
        return string.IsNullOrWhiteSpace(title) ? "USER GAP" : title;
    }

    private static string GetOutputMonitorName(LiveSessionSnapshot session, OutputWindowState output)
        => !string.IsNullOrWhiteSpace(session.OutputMonitorName)
            ? session.OutputMonitorName
            : output.Display?.Name ?? string.Empty;

    private static bool ShouldShowPanelOverlay(
        OutputSceneKind kind,
        LiveSessionSnapshot session,
        LiveOutputRenderSettings settings)
    {
        // 비우기 화면은 배경만 깨끗이 보여야 하므로 모니터명/상태 오버레이도 감춘다.
        if (kind == OutputSceneKind.Cleared)
        {
            return false;
        }

        if (kind != OutputSceneKind.Live)
        {
            return true;
        }

        if (settings.NoPowerPointPanelOverlay && IsPowerPointKind(session.CurrentItemKind))
        {
            return false;
        }

        if (settings.NoMediaPanelOverlay && IsMediaKind(session.CurrentItemKind))
        {
            return false;
        }

        return true;
    }

    private static bool IsPowerPointKind(string kind)
        => string.Equals(kind, "P", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "PPT", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, LiveItemKinds.PowerPoint, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "Presentation", StringComparison.OrdinalIgnoreCase);

    private static bool IsMediaKind(string kind)
        => string.Equals(kind, "M", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, LiveItemKinds.Media, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "Video", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "Audio", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "LiveCamera", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "Live Camera", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(kind, "CaptureDevice", StringComparison.OrdinalIgnoreCase);

    private static Rect CreateViewport(int viewportWidth, int viewportHeight)
        => viewportWidth > 0 && viewportHeight > 0
            ? new Rect(0, 0, viewportWidth, viewportHeight)
            : Rect.Empty;
}
