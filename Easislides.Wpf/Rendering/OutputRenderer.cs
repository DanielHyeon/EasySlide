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
    bool LyricsMonitorShowNotations = true,
    bool NoPowerPointPanelOverlay = false,
    bool NoMediaPanelOverlay = false,
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
    // 출력 가사 줄 간격(폰트 대비 %, 인-셸 가사 포맷팅 §7.3-A). 기본 125.
    int LyricsMonitorLineSpacingPercent = 125,
    // 출력 위치 인디케이터(절/슬라이드 "N/M") 표시 여부(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsPositionIndicator = false,
    // 출력 제목 헤딩(가사 위 상단 배너로 곡 제목) 표시 여부(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsTitleHeading = false,
    // 출력 가사 외곽선(Outline Font) 효과 여부(인-셸 §7.3-A 폰트 효과). 기본 off.
    bool ShowLyricsOutline = false,
    // 출력 제목 헤딩 가로 정렬(인-셸 §7.3-A Heading Align). 기본 Center.
    LyricsTextAlignment LyricsMonitorTitleHeadingAlignment = LyricsTextAlignment.Center,
    // 제목 헤딩을 곡 첫 절(첫 화면)에만 표시(인-셸 §7.3-A Heading At First Screen Only). 기본 off.
    bool TitleHeadingFirstScreenOnly = false,
    // 출력에 곡 번호 표시(FrmMain Show Item Number, Display Panel). 기본 off → 기존 출력 무변화.
    bool LyricsMonitorShowItemNumber = false,
    // 출력에 저작권 표시(FrmMain Show Copyright Information, Display Panel). 기본 off.
    bool LyricsMonitorShowCopyright = false,
    // 출력에 다음 항목 표시(FrmMain Display Panel PrevNext). 기본 off.
    bool LyricsMonitorShowNextItem = false,
    // 출력 전역 배경 이미지 경로(FrmMain Images 탭). 비었으면 색 배경. 곡별 배경(61)이 우선.
    string LyricsMonitorBackgroundImagePath = "")
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
            settings.Get(EasiSettingKeys.LyricsMonitorShowNotations),
            settings.Get(EasiSettingKeys.NoPowerPointPanelOverlay),
            settings.Get(EasiSettingKeys.NoMediaPanelOverlay),
            settings.Get(EasiSettingKeys.LyricsMonitorTextAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorVerticalAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorFontSize),
            settings.Get(EasiSettingKeys.LyricsMonitorBold),
            settings.Get(EasiSettingKeys.LyricsMonitorItalic),
            settings.Get(EasiSettingKeys.LyricsMonitorShadow),
            settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent),
            settings.Get(EasiSettingKeys.LyricsMonitorShowPositionIndicator),
            settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading),
            settings.Get(EasiSettingKeys.LyricsMonitorOutline),
            settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment),
            settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly),
            settings.Get(EasiSettingKeys.LyricsMonitorShowItemNumber),
            settings.Get(EasiSettingKeys.LyricsMonitorShowCopyright),
            settings.Get(EasiSettingKeys.LyricsMonitorShowNextItem),
            settings.Get(EasiSettingKeys.LyricsMonitorBackgroundImagePath));
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
    bool LyricsMonitorShowNotations,
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
    // 출력 가사 줄 간격(폰트 대비 %, 인-셸 가사 포맷팅 §7.3-A). 기본 125.
    int LyricsMonitorLineSpacingPercent = 125,
    // 위치 라벨(절/슬라이드 "N/M"). Live 가 아니면 빈 문자열로 들어온다.
    string PositionLabel = "",
    // 위치 인디케이터 표시 설정(인-셸 §7.3-A). 기본 off.
    bool ShowLyricsPositionIndicator = false,
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
    // 곡 번호 라벨(예: "123"). Live + 곡 번호>0 일 때만 채워진다.
    string ItemNumberLabel = "",
    // 저작권 표시 설정(Display Panel). 기본 off.
    bool ShowLyricsCopyright = false,
    // 저작권 라벨(CCLI 등). Live + 저작권 문자열이 있을 때만 채워진다.
    string CopyrightLabel = "",
    // 다음 항목 표시 설정(Display Panel PrevNext). 기본 off.
    bool ShowLyricsNextItem = false,
    // 다음 항목 제목 라벨. Live + 다음 항목이 있을 때만 채워진다.
    string NextItemLabel = "")
{
    public bool ShowsContent => Kind == OutputSceneKind.Live && ContentPlacement.Width > 0 && ContentPlacement.Height > 0;

    // 가사 본문을 실제로 송출할지 — Live 상태 + 본문이 있을 때만.
    public bool ShowsBodyText => Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(BodyText);

    // Region2(이중 언어 보조) 본문을 송출할지 — Live + Region2 본문이 있을 때만(단일 영역 곡은 false → 무회귀).
    public bool ShowsBodyText2 => Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(BodyText2);

    // 곡 번호를 실제로 노출할지 — 설정 on + Live + 번호 라벨이 있을 때만(Display Panel).
    public bool ShowsItemNumber => ShowLyricsItemNumber && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(ItemNumberLabel);

    // 저작권을 실제로 노출할지 — 설정 on + Live + 저작권 문자열이 있을 때만(Display Panel).
    public bool ShowsCopyright => ShowLyricsCopyright && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(CopyrightLabel);

    // 다음 항목을 실제로 노출할지 — 설정 on + Live + 다음 항목 제목이 있을 때만(Display Panel PrevNext).
    public bool ShowsNextItem => ShowLyricsNextItem && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(NextItemLabel);

    // 위치 인디케이터를 실제로 노출할지 — 설정 on + Live + 라벨이 있을 때만.
    public bool ShowsPositionIndicator => ShowLyricsPositionIndicator && Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(PositionLabel);

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
        // 곡별 글꼴명(있으면)도 Live 일 때만 적용. 비었으면 빈 문자열 → VM 이 테마 기본 글꼴을 상속(무회귀).
        var fontFamily = isLive && !string.IsNullOrWhiteSpace(request.Session.OverrideFontName)
            ? request.Session.OverrideFontName!
            : "";
        // 배경 이미지(Live 일 때만): 곡별 배경(FormatData 61)이 있으면 그 곡 동안 우선,
        // 없으면 전역 배경 이미지(Images 탭 설정)를 쓴다. 둘 다 없으면 빈 문자열 → 색 배경 유지(무회귀).
        var backgroundImagePath = !isLive
            ? ""
            : !string.IsNullOrWhiteSpace(request.Session.OverrideBackgroundImagePath)
                ? request.Session.OverrideBackgroundImagePath!
                : liveOutput.LyricsMonitorBackgroundImagePath;
        // Region2(이중 언어) 본문은 Live 일 때만 싣는다. Region2 색은 곡별 region2 색(30)이 있으면 그것, 없으면 Region1 색을 추종.
        var bodyText2 = isLive ? request.Session.CurrentItemBodyText2 : string.Empty;
        var textColor2Argb = isLive && request.Session.OverrideTextColorArgb2 is int songTextColor2
            ? songTextColor2
            : textColorArgb;
        // Region2 정렬도 Live + 곡별 region2 정렬(32)이 있을 때만, 없으면 Region1 정렬(textAlignment) 추종.
        var textAlignment2 = isLive && request.Session.OverrideTextAlignment2 is LyricsTextAlignment songAlign2
            ? songAlign2
            : textAlignment;
        // Region2 글꼴명·크기도 Live + 곡별 region2 값(44/48)이 있을 때만, 없으면 Region1 글꼴(fontFamily/fontSizePx) 추종.
        var fontFamily2 = isLive && !string.IsNullOrWhiteSpace(request.Session.OverrideFontName2)
            ? request.Session.OverrideFontName2!
            : fontFamily;
        var fontSize2Px = isLive && request.Session.OverrideFontSizePx2 is int songFont2Px
            ? songFont2Px
            : fontSizePx;

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
            liveOutput.LyricsMonitorShowNotations,
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
            liveOutput.LyricsMonitorBold,
            liveOutput.LyricsMonitorItalic,
            liveOutput.LyricsMonitorShadow,
            liveOutput.LyricsMonitorLineSpacingPercent,
            // 위치 라벨은 Live 일 때만 의미 있다(숨김/대기에선 빈 문자열).
            kind == OutputSceneKind.Live ? request.Session.CurrentItemPositionLabel : string.Empty,
            liveOutput.ShowLyricsPositionIndicator,
            liveOutput.ShowLyricsTitleHeading,
            liveOutput.ShowLyricsOutline,
            liveOutput.LyricsMonitorTitleHeadingAlignment,
            liveOutput.TitleHeadingFirstScreenOnly,
            // 현재 절 인덱스는 Live 일 때만 의미 있다(그 외엔 0=첫 화면 취급).
            kind == OutputSceneKind.Live ? request.Session.CurrentLyricsPageIndex : 0,
            // 곡별 글꼴명(Live + 오버라이드 있을 때만). 비었으면 VM 이 테마 기본 글꼴을 상속(무회귀).
            fontFamily,
            // 곡별 배경 이미지 경로(Live + 오버라이드 있을 때만). 비었으면 VM 이 색 배경을 유지(무회귀).
            backgroundImagePath,
            // Region2(이중 언어) 본문·색·정렬·글꼴 — Live + 이중 언어 곡일 때만 채워진다.
            bodyText2,
            textColor2Argb,
            textAlignment2,
            fontFamily2,
            fontSize2Px,
            // 곡 번호 표시(설정) + 라벨(Live + 번호>0). 곡 번호 0이면 빈 문자열 → 미표시.
            liveOutput.LyricsMonitorShowItemNumber,
            isLive && request.Session.CurrentItemNumber > 0 ? request.Session.CurrentItemNumber.ToString() : string.Empty,
            // 저작권 표시(설정) + 라벨(Live + 저작권 문자열). 비면 미표시.
            liveOutput.LyricsMonitorShowCopyright,
            isLive ? request.Session.CurrentItemCopyright : string.Empty,
            // 다음 항목 표시(설정) + 라벨(Live + 다음 항목 제목). 마지막 항목이면 빈 문자열 → 미표시.
            liveOutput.LyricsMonitorShowNextItem,
            isLive ? request.Session.CurrentItemNextTitle : string.Empty);
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
