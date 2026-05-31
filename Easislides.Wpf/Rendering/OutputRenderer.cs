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
    LyricsTextAlignment LyricsMonitorTextAlignment = LyricsTextAlignment.Center)
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
            settings.Get(EasiSettingKeys.LyricsMonitorTextAlignment));
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
    LyricsTextAlignment LyricsMonitorTextAlignment = LyricsTextAlignment.Center)
{
    public bool ShowsContent => Kind == OutputSceneKind.Live && ContentPlacement.Width > 0 && ContentPlacement.Height > 0;

    // 가사 본문을 실제로 송출할지 — Live 상태 + 본문이 있을 때만.
    public bool ShowsBodyText => Kind == OutputSceneKind.Live && !string.IsNullOrWhiteSpace(BodyText);
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
            liveOutput.LyricsMonitorTextColorArgb,
            liveOutput.LyricsMonitorBackgroundColorArgb,
            liveOutput.LyricsMonitorBackgroundColor2Argb,
            liveOutput.LyricsMonitorBackgroundIsGradient,
            liveOutput.GapItemOption,
            liveOutput.GapItemLogoFile,
            liveOutput.GapItemUseFade,
            ShouldShowPanelOverlay(kind, request.Session, liveOutput),
            // 곡 가사 본문은 Live 일 때만 송출(숨김/블랙아웃/대기 상태에선 빈 문자열).
            kind == OutputSceneKind.Live ? request.Session.CurrentItemBodyText : string.Empty,
            liveOutput.LyricsMonitorTextAlignment);
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
