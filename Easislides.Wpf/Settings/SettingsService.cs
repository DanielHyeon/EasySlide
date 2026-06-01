using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Easislides.Wpf.Input;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Theme;

namespace Easislides.Wpf.Settings;

public enum SettingsChangeSource
{
    User,
    Import,
    Migration,
    RestoreDefaults,
}

public enum SettingsIssueSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record SettingsIssue(string Key, SettingsIssueSeverity Severity, string Message);

public sealed record SettingsResult(bool Succeeded, IReadOnlyList<SettingsIssue> Issues, string? BackupPath = null)
{
    public static SettingsResult Success(string? backupPath = null, IReadOnlyList<SettingsIssue>? issues = null)
        => new(true, issues ?? Array.Empty<SettingsIssue>(), backupPath);

    public static SettingsResult Failure(IReadOnlyList<SettingsIssue> issues)
        => new(false, issues, BackupPath: null);
}

public sealed record SettingsChangedEventArgs(
    EasiSettingsSnapshot Previous,
    EasiSettingsSnapshot Current,
    IReadOnlyList<string> ChangedKeys,
    SettingsChangeSource Source,
    string? BackupPath);

public sealed record SettingsServiceOptions(string SettingsFilePath, string BackupRoot)
{
    public static SettingsServiceOptions CreateDefault()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasislidesNext");

        return new SettingsServiceOptions(
            Path.Combine(root, "settings.json"),
            Path.Combine(root, "Backups"));
    }
}

public sealed record SettingKey<T>(string Id, T DefaultValue);

public enum GapItemMode
{
    None = 0,
    Black = 1,
    Default = 2,
    User = 3,
}

// 출력 화면 가사 가로 정렬(인-셸 가사 정렬 — 레거시 Align Left/Centre/Right). 기본 Center(기존 동작 보존).
public enum LyricsTextAlignment
{
    Left = 0,
    Center = 1,
    Right = 2,
}

// 출력 화면 가사 세로 정렬(인-셸 가사 정렬 — 레거시 Align Top/Bottom). 기본 Center(기존 동작 보존).
public enum LyricsVerticalAlignment
{
    Top = 0,
    Center = 1,
    Bottom = 2,
}

// 출력 장면 전환 모션 종류(FrmMain 전환 효과 중 구현분). 기본 Fade(기존 250ms 페이드 동작 보존).
// Fade=불투명도, Slide*=방향 슬라이드, Zoom*=확대/축소, Spin=회전, Flip*=뒤집기 — 모두 단일 콘텐츠
// 트랜스폼(Translate/Scale/Rotate) 기반이라 2-레이어 클립 엔진이 필요 없다.
// (Circle/Diamond/Blinds/Checkerboard 등 셰이프·타일 마스크 전환은 별도 클립 엔진 필요 → P2 미구현.)
public enum LyricsTransitionKind
{
    Fade = 0,
    SlideFromLeft = 1,
    SlideFromRight = 2,
    SlideFromTop = 3,
    SlideFromBottom = 4,
    ZoomIn = 5,
    ZoomOut = 6,
    Spin = 7,
    FlipHorizontal = 8,
    FlipVertical = 9,
    // 클립(마스크) 리빌 — 새 콘텐츠가 확장하는 도형/방향 클립으로 드러난다(단일 레이어 Clip 애니메이션).
    RevealCircle = 10,
    RevealRectangle = 11,
    WipeRight = 12,
    WipeLeft = 13,
    WipeDown = 14,
    WipeUp = 15,
    // 다중 타일 마스크 — 여러 사각 클립(블라인드 띠/체커 격자)이 동시·교차로 커지며 드러난다(단일 레이어 GeometryGroup).
    BlindsHorizontal = 16,
    BlindsVertical = 17,
    Checkerboard = 18,
    // 다이아몬드(중심 확대 도형 클립) · 양문 열기/닫기(2분할 클립) — 끝에 화면 전체를 덮어 잔여 마스크 없음.
    Diamond = 19,
    DoorsOpen = 20,
    DoorsClose = 21,
    // 별(5각) — 안쪽 반지름(오목 골)을 화면 모서리 거리보다 크게 잡아 배율 1 에서 전체를 덮는다(잔여 마스크 없음).
    Star = 22,
    // 십자(플러스) — 끝에 코너를 못 덮는 오목 도형이라 2-레이어(뒤에 옛 프레임)로 처리. 새 콘텐츠가 십자로
    // 드러나고 코너엔 옛 화면이 남았다가 완료 시 새 화면으로 전환된다.
    Cross = 23,
    // 나비넥타이(좌우 두 삼각형, 상/하 중앙이 오므라듦) — 오목 도형이라 2-레이어로 처리.
    BowTie = 24,
    // 하트 — 위 노치·코너를 못 덮는 오목 도형이라 2-레이어로 처리. 매개변수 곡선을 폴리라인으로 근사.
    Heart = 25,
    // 시계 와이프(부채꼴 스윕) — 12시에서 시계방향으로 0→360° 부채꼴이 커지며 새 콘텐츠를 드러낸다.
    // 360°에서 전체를 덮어 단일 레이어로 충분. 각도 애니메이션이라 프레임마다 지오메트리를 다시 만든다.
    Wedge = 26,
}

public static class EasiSettingKeys
{
    public static readonly SettingKey<string> Language = new("general.language", "ko-KR");
    public static readonly SettingKey<string> WorkingFolder = new(
        "general.workingFolder",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasiSlides"));
    public static readonly SettingKey<bool> OnboardingCompleted = new("general.onboardingCompleted", false);
    public static readonly SettingKey<string> RegistrationUser = new("general.registrationUser", "");

    public static readonly SettingKey<ColorTheme> Theme = new("appearance.theme", ColorTheme.Light);
    public static readonly SettingKey<InterfaceSize> InterfaceSize =
        new("appearance.interfaceSize", Easislides.Wpf.Theme.InterfaceSize.Standard);
    public static readonly SettingKey<string> DefaultOutputMonitorId = new("liveOutput.defaultOutputMonitorId", "");
    public static readonly SettingKey<bool> UseSafetyConfirmations = new("liveOutput.useSafetyConfirmations", true);
    public static readonly SettingKey<bool> ShowLyricsMonitorAlertBox = new("liveOutput.showLyricsMonitorAlertBox", false);
    public static readonly SettingKey<bool> AdvanceNextItem = new("liveOutput.advanceNextItem", false);
    public static readonly SettingKey<GapItemMode> GapItemOption = new("liveOutput.gapItemOption", GapItemMode.None);
    public static readonly SettingKey<string> GapItemLogoFile = new("liveOutput.gapItemLogoFile", "");
    public static readonly SettingKey<bool> GapItemUseFade = new("liveOutput.gapItemUseFade", true);
    public static readonly SettingKey<bool> DisplayAlwaysUseSecondaryMonitor = new("liveOutput.displayAlwaysUseSecondaryMonitor", true);
    public static readonly SettingKey<int> DisplayCustomTop = new("liveOutput.displayCustomTop", 0);
    public static readonly SettingKey<int> DisplayCustomLeft = new("liveOutput.displayCustomLeft", 0);
    public static readonly SettingKey<int> DisplayCustomWidth = new("liveOutput.displayCustomWidth", 100);
    public static readonly SettingKey<int> LyricsMonitorTextColorArgb = new("liveOutput.lyricsMonitorTextColorArgb", -16777216);
    public static readonly SettingKey<int> LyricsMonitorBackgroundColorArgb = new("liveOutput.lyricsMonitorBackgroundColorArgb", -1);
    // 배경 그라데이션 끝색(ARGB). IsGradient=true 일 때 배경색→이 색 세로 그라데이션 송출(G2 / FrmBackground 슬라이스).
    public static readonly SettingKey<int> LyricsMonitorBackgroundColor2Argb = new("liveOutput.lyricsMonitorBackgroundColor2Argb", -1);
    // 배경 그라데이션 사용 여부(기본 false=솔리드). true 면 배경색→끝색 세로 그라데이션.
    public static readonly SettingKey<bool> LyricsMonitorBackgroundIsGradient = new("liveOutput.lyricsMonitorBackgroundIsGradient", false);
    public static readonly SettingKey<bool> LyricsMonitorShowNotations = new("liveOutput.lyricsMonitorShowNotations", true);
    // 출력 가사 가로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center 로 기존 가운데 정렬 동작을 보존.
    public static readonly SettingKey<LyricsTextAlignment> LyricsMonitorTextAlignment =
        new("liveOutput.lyricsMonitorTextAlignment", LyricsTextAlignment.Center);
    // 출력 가사 세로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center 로 기존 가운데 정렬 동작을 보존.
    public static readonly SettingKey<LyricsVerticalAlignment> LyricsMonitorVerticalAlignment =
        new("liveOutput.lyricsMonitorVerticalAlignment", LyricsVerticalAlignment.Center);
    // 출력 가사 폰트 크기(px, 인-셸 가사 포맷팅 §7.3-A). 기본 48 로 기존 출력 폰트 크기를 보존. 범위 24~120.
    public static readonly SettingKey<int> LyricsMonitorFontSize = new("liveOutput.lyricsMonitorFontSize", 48);
    // 출력 가사 폰트 효과(인-셸 가사 포맷팅 §7.3-A). 모두 기본 off 로 기존 출력 모양 보존.
    // Bold off = 기존 SemiBold, Italic off = Normal, Shadow off = 효과 없음.
    public static readonly SettingKey<bool> LyricsMonitorBold = new("liveOutput.lyricsMonitorBold", false);
    public static readonly SettingKey<bool> LyricsMonitorItalic = new("liveOutput.lyricsMonitorItalic", false);
    public static readonly SettingKey<bool> LyricsMonitorShadow = new("liveOutput.lyricsMonitorShadow", false);
    // 출력 가사 줄 간격(폰트 크기 대비 %, 인-셸 가사 포맷팅 §7.3-A). 기본 125 로 기존 줄높이(폰트×1.25) 보존. 범위 100~220.
    public static readonly SettingKey<int> LyricsMonitorLineSpacingPercent = new("liveOutput.lyricsMonitorLineSpacingPercent", 125);
    // 출력 위치 인디케이터 표시(절/슬라이드 "N/M", 인-셸 §7.3-A). 기본 off.
    public static readonly SettingKey<bool> LyricsMonitorShowPositionIndicator = new("liveOutput.lyricsMonitorShowPositionIndicator", false);
    // 출력에 곡 번호 표시(FrmMain "Show Item Number"/"Use Song Numbering", Display Panel). 기본 off → 기존 동작 보존.
    public static readonly SettingKey<bool> LyricsMonitorShowItemNumber = new("liveOutput.lyricsMonitorShowItemNumber", false);
    // 출력에 저작권 표시(FrmMain "Show Copyright Information", Display Panel — CCLI 등 라이선스 표기). 기본 off.
    public static readonly SettingKey<bool> LyricsMonitorShowCopyright = new("liveOutput.lyricsMonitorShowCopyright", false);
    // 출력에 다음 항목 표시(FrmMain Display Panel PrevNext — 다음 예배순서 항목 제목 미리보기). 기본 off.
    public static readonly SettingKey<bool> LyricsMonitorShowNextItem = new("liveOutput.lyricsMonitorShowNextItem", false);
    // 출력 장면 전환 시 페이드 효과 사용(FrmMain 전환 효과 — 현재는 Fade 만 구현). 기본 on → 기존 250ms 페이드 동작 보존.
    // off 면 즉시 컷(애니메이션 없음). 50여 종 셰이프/타일 전환은 미구현이므로 정직하게 페이드 on/off 만 노출.
    public static readonly SettingKey<bool> LyricsMonitorUseFadeTransition = new("liveOutput.lyricsMonitorUseFadeTransition", true);
    // 출력 페이드 전환 길이(ms). 기본 250(기존 동작). 범위 0~2000. 0 이면 사실상 즉시 전환.
    public static readonly SettingKey<int> LyricsMonitorTransitionDurationMs = new("liveOutput.lyricsMonitorTransitionDurationMs", 250);
    // 출력 장면 전환 모션 종류(Fade/Slide 4방향). 기본 Fade(기존 동작). UseFadeTransition off 면 모션과 무관하게 즉시 컷.
    public static readonly SettingKey<LyricsTransitionKind> LyricsMonitorTransitionKind =
        new("liveOutput.lyricsMonitorTransitionKind", LyricsTransitionKind.Fade);
    // 출력 전역 배경 이미지 경로(FrmMain Images 탭 — 배경으로 적용). 비었으면 색 배경 유지(무회귀).
    // 곡별 FormatData 61(per-song) 배경이 있으면 그 곡 동안은 곡별 배경이 우선하고, 없으면 이 전역 배경을 쓴다.
    public static readonly SettingKey<string> LyricsMonitorBackgroundImagePath = new("liveOutput.lyricsMonitorBackgroundImagePath", "");
    // 출력 제목 헤딩 표시(가사 위 상단 배너로 곡 제목, 인-셸 §7.3-A). 기본 off → 기존 동작(본문 송출 시 제목 숨김) 보존.
    public static readonly SettingKey<bool> LyricsMonitorShowTitleHeading = new("liveOutput.lyricsMonitorShowTitleHeading", false);
    // 출력 가사 외곽선(Outline Font) 효과(인-셸 §7.3-A 폰트 효과). 기본 off → 기존 출력 모양 보존.
    // on 이면 본문을 외곽선 렌더러(OutlinedTextBlock)로 그려 어두운/영상 배경 위 가독성을 높인다.
    public static readonly SettingKey<bool> LyricsMonitorOutline = new("liveOutput.lyricsMonitorOutline", false);
    // 출력 제목 헤딩 가로 정렬(인-셸 §7.3-A, 레거시 Heading Align L/C/R). 기본 Center 로 기존 헤딩 가운데 정렬 보존.
    public static readonly SettingKey<LyricsTextAlignment> LyricsMonitorTitleHeadingAlignment =
        new("liveOutput.lyricsMonitorTitleHeadingAlignment", LyricsTextAlignment.Center);
    // 제목 헤딩을 곡 첫 절(첫 화면)에만 표시(인-셸 §7.3-A, 레거시 Heading At First Screen Only). 기본 off → 모든 절에 표시.
    public static readonly SettingKey<bool> LyricsMonitorTitleHeadingFirstScreenOnly =
        new("liveOutput.lyricsMonitorTitleHeadingFirstScreenOnly", false);
    // 자동 회전 간격(초, §7.3-B). 라이브 중 절/슬라이드를 이 간격으로 자동 전환. 기본 20초, 범위 2~600.
    public static readonly SettingKey<int> AutoRotateIntervalSeconds = new("liveOutput.autoRotateIntervalSeconds", 20);
    public static readonly SettingKey<bool> UsePowerPointTab = new("powerPoint.usePowerPointTab", false);
    public static readonly SettingKey<bool> NoPowerPointPanelOverlay = new("powerPoint.noPanelOverlay", false);
    public static readonly SettingKey<int> PowerPointRenderTimeoutSeconds = new("powerPoint.renderTimeoutSeconds", 60);
    public static readonly SettingKey<int> ThumbnailCacheMegabytes = new("powerPoint.thumbnailCacheMegabytes", 256);
    public static readonly SettingKey<int> PowerPointMaxFiles = new("powerPoint.maxFiles", 20);
    public static readonly SettingKey<bool> UseMediaTab = new("media.useMediaTab", false);
    public static readonly SettingKey<bool> NoMediaPanelOverlay = new("media.noPanelOverlay", false);
    public static readonly SettingKey<string> MediaDirectory = new("media.directory", "");
    public static readonly SettingKey<double> MediaVolume = new("media.volume", 0.8);
    public static readonly SettingKey<double> MediaBalance = new("media.balance", 0.0);
    public static readonly SettingKey<bool> MediaMuted = new("media.muted", false);
    public static readonly SettingKey<int> LiveCameraNumber = new("media.liveCameraNumber", 1);
    public static readonly SettingKey<string> AdminDatabasePath = new("data.adminDatabasePath", "");
    public static readonly SettingKey<string> DataBackupRoot = new("data.backupRoot", "");
    public static readonly SettingKey<bool> EnableDiagnostics = new("advanced.enableDiagnostics", false);

    public static IReadOnlyList<object> All { get; } =
    [
        Language,
        WorkingFolder,
        OnboardingCompleted,
        RegistrationUser,
        Theme,
        InterfaceSize,
        DefaultOutputMonitorId,
        UseSafetyConfirmations,
        ShowLyricsMonitorAlertBox,
        AdvanceNextItem,
        GapItemOption,
        GapItemLogoFile,
        GapItemUseFade,
        DisplayAlwaysUseSecondaryMonitor,
        DisplayCustomTop,
        DisplayCustomLeft,
        DisplayCustomWidth,
        LyricsMonitorTextColorArgb,
        LyricsMonitorBackgroundColorArgb,
        LyricsMonitorBackgroundColor2Argb,
        LyricsMonitorBackgroundIsGradient,
        LyricsMonitorShowNotations,
        LyricsMonitorTextAlignment,
        LyricsMonitorVerticalAlignment,
        LyricsMonitorFontSize,
        LyricsMonitorBold,
        LyricsMonitorItalic,
        LyricsMonitorShadow,
        LyricsMonitorLineSpacingPercent,
        LyricsMonitorShowPositionIndicator,
        // Display Panel 토글들(곡번호·저작권·다음항목) — All 누락 시 FindChangedKeys 가 변경을 못 잡아
        // 메뉴 토글이 라이브 출력에 즉시 반영되지 않는다(다음 GoLive 때까지 지연). 반드시 등록.
        LyricsMonitorShowItemNumber,
        LyricsMonitorShowCopyright,
        LyricsMonitorShowNextItem,
        LyricsMonitorShowTitleHeading,
        LyricsMonitorOutline,
        LyricsMonitorTitleHeadingAlignment,
        LyricsMonitorTitleHeadingFirstScreenOnly,
        // 전환 효과(페이드 사용·길이·모션 종류) — 변경 감지·라이브 반영을 위해 등록.
        LyricsMonitorUseFadeTransition,
        LyricsMonitorTransitionDurationMs,
        LyricsMonitorTransitionKind,
        // 전역 배경 이미지 경로 — 변경 감지·라이브 반영을 위해 등록.
        LyricsMonitorBackgroundImagePath,
        AutoRotateIntervalSeconds,
        UsePowerPointTab,
        NoPowerPointPanelOverlay,
        PowerPointRenderTimeoutSeconds,
        ThumbnailCacheMegabytes,
        PowerPointMaxFiles,
        UseMediaTab,
        NoMediaPanelOverlay,
        MediaDirectory,
        MediaVolume,
        MediaBalance,
        MediaMuted,
        LiveCameraNumber,
        AdminDatabasePath,
        DataBackupRoot,
        EnableDiagnostics,
    ];
}

public sealed record GeneralSettings
{
    public string Language { get; init; } = EasiSettingKeys.Language.DefaultValue;

    public string WorkingFolder { get; init; } = EasiSettingKeys.WorkingFolder.DefaultValue;

    public bool OnboardingCompleted { get; init; } = EasiSettingKeys.OnboardingCompleted.DefaultValue;

    public string RegistrationUser { get; init; } = EasiSettingKeys.RegistrationUser.DefaultValue;
}

public sealed record AppearanceSettings
{
    public ColorTheme Theme { get; init; } = EasiSettingKeys.Theme.DefaultValue;

    public InterfaceSize InterfaceSize { get; init; } = EasiSettingKeys.InterfaceSize.DefaultValue;
}

public sealed record LiveOutputSettings
{
    public string DefaultOutputMonitorId { get; init; } = EasiSettingKeys.DefaultOutputMonitorId.DefaultValue;

    public bool UseSafetyConfirmations { get; init; } = EasiSettingKeys.UseSafetyConfirmations.DefaultValue;

    public bool ShowLyricsMonitorAlertBox { get; init; } = EasiSettingKeys.ShowLyricsMonitorAlertBox.DefaultValue;

    public bool AdvanceNextItem { get; init; } = EasiSettingKeys.AdvanceNextItem.DefaultValue;

    public GapItemMode GapItemOption { get; init; } = EasiSettingKeys.GapItemOption.DefaultValue;

    public string GapItemLogoFile { get; init; } = EasiSettingKeys.GapItemLogoFile.DefaultValue;

    public bool GapItemUseFade { get; init; } = EasiSettingKeys.GapItemUseFade.DefaultValue;

    public bool DisplayAlwaysUseSecondaryMonitor { get; init; } =
        EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor.DefaultValue;

    public int DisplayCustomTop { get; init; } = EasiSettingKeys.DisplayCustomTop.DefaultValue;

    public int DisplayCustomLeft { get; init; } = EasiSettingKeys.DisplayCustomLeft.DefaultValue;

    public int DisplayCustomWidth { get; init; } = EasiSettingKeys.DisplayCustomWidth.DefaultValue;

    public int LyricsMonitorTextColorArgb { get; init; } = EasiSettingKeys.LyricsMonitorTextColorArgb.DefaultValue;

    public int LyricsMonitorBackgroundColorArgb { get; init; } =
        EasiSettingKeys.LyricsMonitorBackgroundColorArgb.DefaultValue;

    public int LyricsMonitorBackgroundColor2Argb { get; init; } =
        EasiSettingKeys.LyricsMonitorBackgroundColor2Argb.DefaultValue;

    public bool LyricsMonitorBackgroundIsGradient { get; init; } =
        EasiSettingKeys.LyricsMonitorBackgroundIsGradient.DefaultValue;

    public bool LyricsMonitorShowNotations { get; init; } = EasiSettingKeys.LyricsMonitorShowNotations.DefaultValue;

    public LyricsTextAlignment LyricsMonitorTextAlignment { get; init; } =
        EasiSettingKeys.LyricsMonitorTextAlignment.DefaultValue;

    public LyricsVerticalAlignment LyricsMonitorVerticalAlignment { get; init; } =
        EasiSettingKeys.LyricsMonitorVerticalAlignment.DefaultValue;

    public int LyricsMonitorFontSize { get; init; } = EasiSettingKeys.LyricsMonitorFontSize.DefaultValue;

    public bool LyricsMonitorBold { get; init; } = EasiSettingKeys.LyricsMonitorBold.DefaultValue;

    public bool LyricsMonitorItalic { get; init; } = EasiSettingKeys.LyricsMonitorItalic.DefaultValue;

    public bool LyricsMonitorShadow { get; init; } = EasiSettingKeys.LyricsMonitorShadow.DefaultValue;

    public int LyricsMonitorLineSpacingPercent { get; init; } = EasiSettingKeys.LyricsMonitorLineSpacingPercent.DefaultValue;

    public bool LyricsMonitorShowPositionIndicator { get; init; } = EasiSettingKeys.LyricsMonitorShowPositionIndicator.DefaultValue;

    public bool LyricsMonitorShowItemNumber { get; init; } = EasiSettingKeys.LyricsMonitorShowItemNumber.DefaultValue;

    public bool LyricsMonitorShowCopyright { get; init; } = EasiSettingKeys.LyricsMonitorShowCopyright.DefaultValue;

    public bool LyricsMonitorShowNextItem { get; init; } = EasiSettingKeys.LyricsMonitorShowNextItem.DefaultValue;

    public bool LyricsMonitorUseFadeTransition { get; init; } = EasiSettingKeys.LyricsMonitorUseFadeTransition.DefaultValue;

    public int LyricsMonitorTransitionDurationMs { get; init; } = EasiSettingKeys.LyricsMonitorTransitionDurationMs.DefaultValue;

    public LyricsTransitionKind LyricsMonitorTransitionKind { get; init; } = EasiSettingKeys.LyricsMonitorTransitionKind.DefaultValue;

    public string LyricsMonitorBackgroundImagePath { get; init; } = EasiSettingKeys.LyricsMonitorBackgroundImagePath.DefaultValue;

    public bool LyricsMonitorShowTitleHeading { get; init; } = EasiSettingKeys.LyricsMonitorShowTitleHeading.DefaultValue;

    public bool LyricsMonitorOutline { get; init; } = EasiSettingKeys.LyricsMonitorOutline.DefaultValue;

    public LyricsTextAlignment LyricsMonitorTitleHeadingAlignment { get; init; } =
        EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.DefaultValue;

    public bool LyricsMonitorTitleHeadingFirstScreenOnly { get; init; } =
        EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.DefaultValue;

    public int AutoRotateIntervalSeconds { get; init; } = EasiSettingKeys.AutoRotateIntervalSeconds.DefaultValue;
}

public sealed record PowerPointSettings
{
    public bool UsePowerPointTab { get; init; } = EasiSettingKeys.UsePowerPointTab.DefaultValue;

    public bool NoPanelOverlay { get; init; } = EasiSettingKeys.NoPowerPointPanelOverlay.DefaultValue;

    public int RenderTimeoutSeconds { get; init; } = EasiSettingKeys.PowerPointRenderTimeoutSeconds.DefaultValue;

    public int ThumbnailCacheMegabytes { get; init; } = EasiSettingKeys.ThumbnailCacheMegabytes.DefaultValue;

    public int MaxFiles { get; init; } = EasiSettingKeys.PowerPointMaxFiles.DefaultValue;
}

public sealed record MediaSettings
{
    public bool UseMediaTab { get; init; } = EasiSettingKeys.UseMediaTab.DefaultValue;

    public bool NoPanelOverlay { get; init; } = EasiSettingKeys.NoMediaPanelOverlay.DefaultValue;

    public string Directory { get; init; } = EasiSettingKeys.MediaDirectory.DefaultValue;

    public double Volume { get; init; } = EasiSettingKeys.MediaVolume.DefaultValue;

    public double Balance { get; init; } = EasiSettingKeys.MediaBalance.DefaultValue;

    public bool Muted { get; init; } = EasiSettingKeys.MediaMuted.DefaultValue;

    public int LiveCameraNumber { get; init; } = EasiSettingKeys.LiveCameraNumber.DefaultValue;
}

public sealed record DataSettings
{
    public string AdminDatabasePath { get; init; } = EasiSettingKeys.AdminDatabasePath.DefaultValue;

    public string BackupRoot { get; init; } = EasiSettingKeys.DataBackupRoot.DefaultValue;
}

public sealed record AdvancedSettings
{
    public bool EnableDiagnostics { get; init; } = EasiSettingKeys.EnableDiagnostics.DefaultValue;
}

public sealed record EasiSettingsSnapshot
{
    public int SchemaVersion { get; init; } = 1;

    public GeneralSettings General { get; init; } = new();

    public AppearanceSettings Appearance { get; init; } = new();

    public LiveOutputSettings LiveOutput { get; init; } = new();

    public PowerPointSettings PowerPoint { get; init; } = new();

    public MediaSettings Media { get; init; } = new();

    public DataSettings Data { get; init; } = new();

    public Dictionary<string, string> Shortcuts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public AdvancedSettings Advanced { get; init; } = new();

    public static EasiSettingsSnapshot CreateDefault() => new();
}

public interface ILegacySettingsSource
{
    bool TryGetString(string key, out string? value);
}

public sealed class DictionaryLegacySettingsSource : ILegacySettingsSource
{
    private readonly IReadOnlyDictionary<string, string?> _values;

    public DictionaryLegacySettingsSource(IReadOnlyDictionary<string, string?> values)
    {
        _values = new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetString(string key, out string? value) => _values.TryGetValue(key, out value);
}

public interface ISettingsService
{
    EasiSettingsSnapshot Current { get; }

    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    T Get<T>(SettingKey<T> key);

    SettingsResult Set<T>(SettingKey<T> key, T value, SettingsChangeSource source = SettingsChangeSource.User);

    SettingsResult Validate(EasiSettingsSnapshot snapshot);

    SettingsResult RestoreDefaults();

    Task<SettingsResult> ExportAsync(string destinationPath);

    Task<SettingsResult> ImportAsync(string sourcePath);

    Task<SettingsResult> MigrateLegacyAsync(ILegacySettingsSource legacySettings);

    SettingsResult SetShortcutOverride(string slotId, string gesture, SettingsChangeSource source = SettingsChangeSource.User);

    SettingsResult ResetShortcutOverride(string slotId, SettingsChangeSource source = SettingsChangeSource.User);
}

public sealed class SettingsService : ISettingsService
{
    public static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly SettingsServiceOptions _options;
    private readonly object _sync = new();

    public SettingsService()
        : this(SettingsServiceOptions.CreateDefault())
    {
    }

    public SettingsService(SettingsServiceOptions options)
    {
        _options = options;
        var loaded = LoadCurrent(options.SettingsFilePath);
        Current = Validate(loaded).Succeeded ? loaded : EasiSettingsSnapshot.CreateDefault();
    }

    public EasiSettingsSnapshot Current { get; private set; }

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public T Get<T>(SettingKey<T> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var value = GetValue(Current, key.Id);
        if (value is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Setting '{key.Id}' is not a {typeof(T).Name} value.");
    }

    public SettingsResult Set<T>(SettingKey<T> key, T value, SettingsChangeSource source = SettingsChangeSource.User)
    {
        ArgumentNullException.ThrowIfNull(key);
        var next = SetValue(Current, key.Id, value);
        return ApplySnapshot(next, source, backupPath: null);
    }

    public SettingsResult SetShortcutOverride(
        string slotId,
        string gesture,
        SettingsChangeSource source = SettingsChangeSource.User)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            return SettingsResult.Failure([Error("shortcuts", "Shortcut slot id cannot be empty.")]);
        }

        if (string.IsNullOrWhiteSpace(gesture))
        {
            return SettingsResult.Failure([Error($"shortcuts.{slotId}", "Shortcut gesture cannot be empty.")]);
        }

        var shortcuts = new Dictionary<string, string>(Current.Shortcuts, StringComparer.OrdinalIgnoreCase)
        {
            [slotId] = gesture,
        };
        return ApplySnapshot(Current with { Shortcuts = shortcuts }, source, backupPath: null);
    }

    public SettingsResult ResetShortcutOverride(
        string slotId,
        SettingsChangeSource source = SettingsChangeSource.User)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            return SettingsResult.Failure([Error("shortcuts", "Shortcut slot id cannot be empty.")]);
        }

        var shortcuts = new Dictionary<string, string>(Current.Shortcuts, StringComparer.OrdinalIgnoreCase);
        shortcuts.Remove(slotId);
        return ApplySnapshot(Current with { Shortcuts = shortcuts }, source, backupPath: null);
    }

    public SettingsResult Validate(EasiSettingsSnapshot snapshot)
    {
        var candidate = Normalize(snapshot);
        var issues = new List<SettingsIssue>();

        if (candidate.SchemaVersion != 1)
        {
            issues.Add(Error("schemaVersion", "Only schema version 1 is supported."));
        }

        RequireText(candidate.General.Language, EasiSettingKeys.Language.Id, issues);
        RequireText(candidate.General.WorkingFolder, EasiSettingKeys.WorkingFolder.Id, issues);
        ValidatePath(candidate.General.WorkingFolder, EasiSettingKeys.WorkingFolder.Id, issues, allowEmpty: false);
        if (!string.IsNullOrWhiteSpace(candidate.General.RegistrationUser))
        {
            RequireNoControlCharacters(
                candidate.General.RegistrationUser,
                EasiSettingKeys.RegistrationUser.Id,
                issues);
        }

        if (!Enum.IsDefined(candidate.Appearance.Theme))
        {
            issues.Add(Error(EasiSettingKeys.Theme.Id, "Theme value is not supported."));
        }

        if (!Enum.IsDefined(candidate.Appearance.InterfaceSize))
        {
            issues.Add(Error(EasiSettingKeys.InterfaceSize.Id, "Interface size value is not supported."));
        }

        if (!string.IsNullOrWhiteSpace(candidate.LiveOutput.DefaultOutputMonitorId))
        {
            RequireNoControlCharacters(
                candidate.LiveOutput.DefaultOutputMonitorId,
                EasiSettingKeys.DefaultOutputMonitorId.Id,
                issues);
        }

        if (!Enum.IsDefined(candidate.LiveOutput.GapItemOption))
        {
            issues.Add(Error(EasiSettingKeys.GapItemOption.Id, "Gap item option value is not supported."));
        }

        // 손상되거나 외부 편집된 settings.json 의 잘못된 정렬 값(예: 정수 99)을 임포트/로드 시 거른다(다른 enum 과 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorTextAlignment))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorTextAlignment.Id, "Lyrics text alignment value is not supported."));
        }

        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorVerticalAlignment))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorVerticalAlignment.Id, "Lyrics vertical alignment value is not supported."));
        }

        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorTitleHeadingAlignment))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.Id, "Title heading alignment value is not supported."));
        }

        // 폰트 크기 범위 가드(24~120px) — 0/음수/과대값이 들어와 출력이 깨지지 않도록(다른 수치 설정과 일관).
        RequireRange(
            candidate.LiveOutput.LyricsMonitorFontSize,
            min: 24,
            max: 120,
            EasiSettingKeys.LyricsMonitorFontSize.Id,
            issues);

        // 줄 간격 범위 가드(100~220%) — 폰트 대비 비율. 과소·과대값으로 줄이 겹치거나 벌어지지 않도록.
        RequireRange(
            candidate.LiveOutput.LyricsMonitorLineSpacingPercent,
            min: 100,
            max: 220,
            EasiSettingKeys.LyricsMonitorLineSpacingPercent.Id,
            issues);

        // 전환 길이 범위 가드(0~2000ms) — 음수/과대값으로 애니메이션이 멈추거나 비현실적으로 길어지지 않도록.
        RequireRange(
            candidate.LiveOutput.LyricsMonitorTransitionDurationMs,
            min: 0,
            max: 2000,
            EasiSettingKeys.LyricsMonitorTransitionDurationMs.Id,
            issues);

        // 자동 회전 간격 가드(2~600초) — 0/음수로 폭주하거나 비현실적으로 길어지지 않도록.
        RequireRange(
            candidate.LiveOutput.AutoRotateIntervalSeconds,
            min: 2,
            max: 600,
            EasiSettingKeys.AutoRotateIntervalSeconds.Id,
            issues);

        ValidatePath(candidate.LiveOutput.GapItemLogoFile, EasiSettingKeys.GapItemLogoFile.Id, issues, allowEmpty: true);
        RequireRange(
            candidate.LiveOutput.DisplayCustomTop,
            min: -9999,
            max: 9999,
            EasiSettingKeys.DisplayCustomTop.Id,
            issues);
        RequireRange(
            candidate.LiveOutput.DisplayCustomLeft,
            min: -9999,
            max: 9999,
            EasiSettingKeys.DisplayCustomLeft.Id,
            issues);
        RequireRange(
            candidate.LiveOutput.DisplayCustomWidth,
            min: 1,
            max: 9999,
            EasiSettingKeys.DisplayCustomWidth.Id,
            issues);

        RequireRange(
            candidate.PowerPoint.RenderTimeoutSeconds,
            min: 1,
            max: 300,
            EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id,
            issues);
        RequireRange(
            candidate.PowerPoint.ThumbnailCacheMegabytes,
            min: 0,
            max: 8192,
            EasiSettingKeys.ThumbnailCacheMegabytes.Id,
            issues);
        RequireRange(
            candidate.PowerPoint.MaxFiles,
            min: 1,
            max: 100,
            EasiSettingKeys.PowerPointMaxFiles.Id,
            issues);
        ValidatePath(candidate.Media.Directory, EasiSettingKeys.MediaDirectory.Id, issues, allowEmpty: true);
        RequireRange(candidate.Media.Volume, min: 0.0, max: 1.0, EasiSettingKeys.MediaVolume.Id, issues);
        RequireRange(candidate.Media.Balance, min: -1.0, max: 1.0, EasiSettingKeys.MediaBalance.Id, issues);
        RequireRange(candidate.Media.LiveCameraNumber, min: 1, max: 5, EasiSettingKeys.LiveCameraNumber.Id, issues);
        ValidatePath(candidate.Data.AdminDatabasePath, EasiSettingKeys.AdminDatabasePath.Id, issues, allowEmpty: true);
        ValidatePath(candidate.Data.BackupRoot, EasiSettingKeys.DataBackupRoot.Id, issues, allowEmpty: true);

        foreach (var shortcut in candidate.Shortcuts)
        {
            if (string.IsNullOrWhiteSpace(shortcut.Key))
            {
                issues.Add(Error("shortcuts", "Shortcut command id cannot be empty."));
            }

            if (string.IsNullOrWhiteSpace(shortcut.Value))
            {
                issues.Add(Error($"shortcuts.{shortcut.Key}", "Shortcut gesture cannot be empty."));
            }
        }

        return issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error)
            ? SettingsResult.Failure(issues)
            : SettingsResult.Success(issues: issues);
    }

    public SettingsResult RestoreDefaults()
        => ApplySnapshot(EasiSettingsSnapshot.CreateDefault(), SettingsChangeSource.RestoreDefaults, backupPath: null);

    public async Task<SettingsResult> ExportAsync(string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            return SettingsResult.Failure([Error("export.path", "Export path cannot be empty.")]);
        }

        try
        {
            await WriteSnapshotAsync(destinationPath, Current).ConfigureAwait(false);
            return SettingsResult.Success();
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return SettingsResult.Failure([Error("export.path", ex.Message)]);
        }
    }

    public async Task<SettingsResult> ImportAsync(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return SettingsResult.Failure([Error("import.path", "Import path cannot be empty.")]);
        }

        try
        {
            var imported = await ReadSnapshotAsync(sourcePath).ConfigureAwait(false);
            var validation = Validate(imported);
            if (!validation.Succeeded)
            {
                return validation;
            }

            var backupPath = await BackupCurrentAsync().ConfigureAwait(false);
            return ApplySnapshot(imported, SettingsChangeSource.Import, backupPath);
        }
        catch (JsonException ex)
        {
            return SettingsResult.Failure([Error("import.json", ex.Message)]);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return SettingsResult.Failure([Error("import.path", ex.Message)]);
        }
    }

    public async Task<SettingsResult> MigrateLegacyAsync(ILegacySettingsSource legacySettings)
    {
        ArgumentNullException.ThrowIfNull(legacySettings);
        var issues = new List<SettingsIssue>();
        var next = Current;

        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.Language.Id), next, value => next with
        {
            General = next.General with { Language = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.WorkingFolder.Id), next, value => next with
        {
            General = next.General with { WorkingFolder = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.OnboardingCompleted.Id), next, issues, value => next with
        {
            General = next.General with { OnboardingCompleted = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.RegistrationUser.Id), next, value => next with
        {
            General = next.General with { RegistrationUser = value },
        });
        next = ApplyLegacyEnum<ColorTheme>(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.Theme.Id), next, issues, value => next with
        {
            Appearance = next.Appearance with { Theme = value },
        });
        next = ApplyLegacyEnum<InterfaceSize>(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.InterfaceSize.Id), next, issues, value => next with
        {
            Appearance = next.Appearance with { InterfaceSize = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DefaultOutputMonitorId.Id), next, value => next with
        {
            LiveOutput = next.LiveOutput with { DefaultOutputMonitorId = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.UseSafetyConfirmations.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { UseSafetyConfirmations = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ShowLyricsMonitorAlertBox.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ShowLyricsMonitorAlertBox = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.AdvanceNextItem.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { AdvanceNextItem = value },
        });
        next = ApplyLegacyEnum<GapItemMode>(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.GapItemOption.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { GapItemOption = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.GapItemLogoFile.Id), next, value => next with
        {
            LiveOutput = next.LiveOutput with { GapItemLogoFile = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.GapItemUseFade.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { GapItemUseFade = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { DisplayAlwaysUseSecondaryMonitor = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DisplayCustomTop.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { DisplayCustomTop = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DisplayCustomLeft.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { DisplayCustomLeft = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DisplayCustomWidth.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { DisplayCustomWidth = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.LyricsMonitorTextColorArgb.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { LyricsMonitorTextColorArgb = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.LyricsMonitorBackgroundColorArgb.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { LyricsMonitorBackgroundColorArgb = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.LyricsMonitorShowNotations.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { LyricsMonitorShowNotations = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.UsePowerPointTab.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { UsePowerPointTab = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.NoPowerPointPanelOverlay.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { NoPanelOverlay = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { RenderTimeoutSeconds = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ThumbnailCacheMegabytes.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { ThumbnailCacheMegabytes = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.PowerPointMaxFiles.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { MaxFiles = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.UseMediaTab.Id), next, issues, value => next with
        {
            Media = next.Media with { UseMediaTab = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.NoMediaPanelOverlay.Id), next, issues, value => next with
        {
            Media = next.Media with { NoPanelOverlay = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaDirectory.Id), next, value => next with
        {
            Media = next.Media with { Directory = value },
        });
        next = ApplyLegacyDouble(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaVolume.Id), next, issues, NormalizeLegacyUnitScale, value => next with
        {
            Media = next.Media with { Volume = value },
        });
        next = ApplyLegacyDouble(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaBalance.Id), next, issues, NormalizeLegacyUnitScale, value => next with
        {
            Media = next.Media with { Balance = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaMuted.Id), next, issues, value => next with
        {
            Media = next.Media with { Muted = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.LiveCameraNumber.Id), next, issues, value => next with
        {
            Media = next.Media with { LiveCameraNumber = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.AdminDatabasePath.Id), next, value => next with
        {
            Data = next.Data with { AdminDatabasePath = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DataBackupRoot.Id), next, value => next with
        {
            Data = next.Data with { BackupRoot = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.EnableDiagnostics.Id), next, issues, value => next with
        {
            Advanced = next.Advanced with { EnableDiagnostics = value },
        });
        next = ApplyLegacyShortcutOverrides(legacySettings, next, issues);

        var validation = Validate(next);
        var combinedIssues = issues.Concat(validation.Issues).ToArray();
        if (combinedIssues.Any(issue => issue.Severity == SettingsIssueSeverity.Error))
        {
            return SettingsResult.Failure(combinedIssues);
        }

        var backupPath = await BackupCurrentAsync().ConfigureAwait(false);
        return ApplySnapshot(next, SettingsChangeSource.Migration, backupPath, combinedIssues);
    }

    private SettingsResult ApplySnapshot(
        EasiSettingsSnapshot snapshot,
        SettingsChangeSource source,
        string? backupPath,
        IReadOnlyList<SettingsIssue>? priorIssues = null)
    {
        snapshot = Normalize(snapshot);
        var validation = Validate(snapshot);
        var issues = priorIssues is { Count: > 0 }
            ? priorIssues.Concat(validation.Issues).Distinct().ToArray()
            : validation.Issues;

        if (issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error))
        {
            return SettingsResult.Failure(issues);
        }

        EasiSettingsSnapshot previous;
        IReadOnlyList<string> changedKeys;
        lock (_sync)
        {
            previous = Current;
            changedKeys = FindChangedKeys(previous, snapshot);
            PersistSnapshot(snapshot);
            Current = snapshot;
        }

        if (changedKeys.Count > 0)
        {
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(previous, snapshot, changedKeys, source, backupPath));
        }

        return SettingsResult.Success(backupPath, issues);
    }

    private void PersistSnapshot(EasiSettingsSnapshot snapshot) => WriteSnapshot(_options.SettingsFilePath, snapshot);

    private async Task<string> BackupCurrentAsync()
    {
        var fileName = $"settings-backup-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}.json";
        var path = Path.Combine(_options.BackupRoot, fileName);
        await WriteSnapshotAsync(path, Current).ConfigureAwait(false);
        return path;
    }

    private static EasiSettingsSnapshot LoadCurrent(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return EasiSettingsSnapshot.CreateDefault();
            }

            var snapshot = JsonSerializer.Deserialize<EasiSettingsSnapshot>(File.ReadAllText(path), JsonOptions);
            return Normalize(snapshot);
        }
        catch (Exception ex) when (ex is JsonException || IsFileSystemException(ex))
        {
            return EasiSettingsSnapshot.CreateDefault();
        }
    }

    private static async Task<EasiSettingsSnapshot> ReadSnapshotAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        var snapshot = JsonSerializer.Deserialize<EasiSettingsSnapshot>(json, JsonOptions);
        return Normalize(snapshot);
    }

    private static void WriteSnapshot(string path, EasiSettingsSnapshot snapshot)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(snapshot, JsonOptions));
        File.Move(tempPath, path, overwrite: true);
    }

    private static async Task WriteSnapshotAsync(string path, EasiSettingsSnapshot snapshot)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(snapshot, JsonOptions)).ConfigureAwait(false);
        File.Move(tempPath, path, overwrite: true);
    }

    private static EasiSettingsSnapshot Normalize(EasiSettingsSnapshot? snapshot)
    {
        var defaults = EasiSettingsSnapshot.CreateDefault();
        if (snapshot is null)
        {
            return defaults;
        }

        return snapshot with
        {
            General = snapshot.General ?? defaults.General,
            Appearance = snapshot.Appearance ?? defaults.Appearance,
            LiveOutput = snapshot.LiveOutput ?? defaults.LiveOutput,
            PowerPoint = snapshot.PowerPoint ?? defaults.PowerPoint,
            Media = snapshot.Media ?? defaults.Media,
            Data = snapshot.Data ?? defaults.Data,
            Shortcuts = snapshot.Shortcuts is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(snapshot.Shortcuts, StringComparer.OrdinalIgnoreCase),
            Advanced = snapshot.Advanced ?? defaults.Advanced,
        };
    }

    private static IReadOnlyList<string> FindChangedKeys(EasiSettingsSnapshot previous, EasiSettingsSnapshot current)
    {
        var changed = new List<string>();
        foreach (var key in EasiSettingKeys.All)
        {
            var id = key switch
            {
                SettingKey<string> stringKey => stringKey.Id,
                SettingKey<ColorTheme> themeKey => themeKey.Id,
                SettingKey<InterfaceSize> sizeKey => sizeKey.Id,
                SettingKey<GapItemMode> gapItemModeKey => gapItemModeKey.Id,
                SettingKey<LyricsTextAlignment> alignmentKey => alignmentKey.Id,
                SettingKey<LyricsVerticalAlignment> verticalAlignmentKey => verticalAlignmentKey.Id,
                SettingKey<LyricsTransitionKind> transitionKindKey => transitionKindKey.Id,
                SettingKey<bool> boolKey => boolKey.Id,
                SettingKey<int> intKey => intKey.Id,
                SettingKey<double> doubleKey => doubleKey.Id,
                _ => throw new InvalidOperationException("Unsupported setting key type."),
            };

            if (!Equals(GetValue(previous, id), GetValue(current, id)))
            {
                changed.Add(id);
            }
        }

        if (!previous.Shortcuts.OrderBy(pair => pair.Key).SequenceEqual(current.Shortcuts.OrderBy(pair => pair.Key)))
        {
            changed.Add("shortcuts");
        }

        return changed;
    }

    private static object GetValue(EasiSettingsSnapshot snapshot, string keyId)
        => keyId switch
        {
            "general.language" => snapshot.General.Language,
            "general.workingFolder" => snapshot.General.WorkingFolder,
            "general.onboardingCompleted" => snapshot.General.OnboardingCompleted,
            "general.registrationUser" => snapshot.General.RegistrationUser,
            "appearance.theme" => snapshot.Appearance.Theme,
            "appearance.interfaceSize" => snapshot.Appearance.InterfaceSize,
            "liveOutput.defaultOutputMonitorId" => snapshot.LiveOutput.DefaultOutputMonitorId,
            "liveOutput.useSafetyConfirmations" => snapshot.LiveOutput.UseSafetyConfirmations,
            "liveOutput.showLyricsMonitorAlertBox" => snapshot.LiveOutput.ShowLyricsMonitorAlertBox,
            "liveOutput.advanceNextItem" => snapshot.LiveOutput.AdvanceNextItem,
            "liveOutput.gapItemOption" => snapshot.LiveOutput.GapItemOption,
            "liveOutput.gapItemLogoFile" => snapshot.LiveOutput.GapItemLogoFile,
            "liveOutput.gapItemUseFade" => snapshot.LiveOutput.GapItemUseFade,
            "liveOutput.displayAlwaysUseSecondaryMonitor" => snapshot.LiveOutput.DisplayAlwaysUseSecondaryMonitor,
            "liveOutput.displayCustomTop" => snapshot.LiveOutput.DisplayCustomTop,
            "liveOutput.displayCustomLeft" => snapshot.LiveOutput.DisplayCustomLeft,
            "liveOutput.displayCustomWidth" => snapshot.LiveOutput.DisplayCustomWidth,
            "liveOutput.lyricsMonitorTextColorArgb" => snapshot.LiveOutput.LyricsMonitorTextColorArgb,
            "liveOutput.lyricsMonitorBackgroundColorArgb" => snapshot.LiveOutput.LyricsMonitorBackgroundColorArgb,
            "liveOutput.lyricsMonitorBackgroundColor2Argb" => snapshot.LiveOutput.LyricsMonitorBackgroundColor2Argb,
            "liveOutput.lyricsMonitorBackgroundIsGradient" => snapshot.LiveOutput.LyricsMonitorBackgroundIsGradient,
            "liveOutput.lyricsMonitorShowNotations" => snapshot.LiveOutput.LyricsMonitorShowNotations,
            "liveOutput.lyricsMonitorTextAlignment" => snapshot.LiveOutput.LyricsMonitorTextAlignment,
            "liveOutput.lyricsMonitorVerticalAlignment" => snapshot.LiveOutput.LyricsMonitorVerticalAlignment,
            "liveOutput.lyricsMonitorFontSize" => snapshot.LiveOutput.LyricsMonitorFontSize,
            "liveOutput.lyricsMonitorBold" => snapshot.LiveOutput.LyricsMonitorBold,
            "liveOutput.lyricsMonitorItalic" => snapshot.LiveOutput.LyricsMonitorItalic,
            "liveOutput.lyricsMonitorShadow" => snapshot.LiveOutput.LyricsMonitorShadow,
            "liveOutput.lyricsMonitorLineSpacingPercent" => snapshot.LiveOutput.LyricsMonitorLineSpacingPercent,
            "liveOutput.lyricsMonitorShowPositionIndicator" => snapshot.LiveOutput.LyricsMonitorShowPositionIndicator,
            "liveOutput.lyricsMonitorShowItemNumber" => snapshot.LiveOutput.LyricsMonitorShowItemNumber,
            "liveOutput.lyricsMonitorShowCopyright" => snapshot.LiveOutput.LyricsMonitorShowCopyright,
            "liveOutput.lyricsMonitorShowNextItem" => snapshot.LiveOutput.LyricsMonitorShowNextItem,
            "liveOutput.lyricsMonitorUseFadeTransition" => snapshot.LiveOutput.LyricsMonitorUseFadeTransition,
            "liveOutput.lyricsMonitorTransitionDurationMs" => snapshot.LiveOutput.LyricsMonitorTransitionDurationMs,
            "liveOutput.lyricsMonitorTransitionKind" => snapshot.LiveOutput.LyricsMonitorTransitionKind,
            "liveOutput.lyricsMonitorBackgroundImagePath" => snapshot.LiveOutput.LyricsMonitorBackgroundImagePath,
            "liveOutput.lyricsMonitorShowTitleHeading" => snapshot.LiveOutput.LyricsMonitorShowTitleHeading,
            "liveOutput.lyricsMonitorOutline" => snapshot.LiveOutput.LyricsMonitorOutline,
            "liveOutput.lyricsMonitorTitleHeadingAlignment" => snapshot.LiveOutput.LyricsMonitorTitleHeadingAlignment,
            "liveOutput.lyricsMonitorTitleHeadingFirstScreenOnly" => snapshot.LiveOutput.LyricsMonitorTitleHeadingFirstScreenOnly,
            "liveOutput.autoRotateIntervalSeconds" => snapshot.LiveOutput.AutoRotateIntervalSeconds,
            "powerPoint.usePowerPointTab" => snapshot.PowerPoint.UsePowerPointTab,
            "powerPoint.noPanelOverlay" => snapshot.PowerPoint.NoPanelOverlay,
            "powerPoint.renderTimeoutSeconds" => snapshot.PowerPoint.RenderTimeoutSeconds,
            "powerPoint.thumbnailCacheMegabytes" => snapshot.PowerPoint.ThumbnailCacheMegabytes,
            "powerPoint.maxFiles" => snapshot.PowerPoint.MaxFiles,
            "media.useMediaTab" => snapshot.Media.UseMediaTab,
            "media.noPanelOverlay" => snapshot.Media.NoPanelOverlay,
            "media.directory" => snapshot.Media.Directory,
            "media.volume" => snapshot.Media.Volume,
            "media.balance" => snapshot.Media.Balance,
            "media.muted" => snapshot.Media.Muted,
            "media.liveCameraNumber" => snapshot.Media.LiveCameraNumber,
            "data.adminDatabasePath" => snapshot.Data.AdminDatabasePath,
            "data.backupRoot" => snapshot.Data.BackupRoot,
            "advanced.enableDiagnostics" => snapshot.Advanced.EnableDiagnostics,
            _ => throw new ArgumentOutOfRangeException(nameof(keyId), keyId, "Unknown setting key."),
        };

    private static EasiSettingsSnapshot SetValue<T>(EasiSettingsSnapshot snapshot, string keyId, T value)
        => keyId switch
        {
            "general.language" => snapshot with
            {
                General = snapshot.General with { Language = Cast<string>(keyId, value) },
            },
            "general.workingFolder" => snapshot with
            {
                General = snapshot.General with { WorkingFolder = Cast<string>(keyId, value) },
            },
            "general.onboardingCompleted" => snapshot with
            {
                General = snapshot.General with { OnboardingCompleted = Cast<bool>(keyId, value) },
            },
            "general.registrationUser" => snapshot with
            {
                General = snapshot.General with { RegistrationUser = Cast<string>(keyId, value) },
            },
            "appearance.theme" => snapshot with
            {
                Appearance = snapshot.Appearance with { Theme = Cast<ColorTheme>(keyId, value) },
            },
            "appearance.interfaceSize" => snapshot with
            {
                Appearance = snapshot.Appearance with { InterfaceSize = Cast<InterfaceSize>(keyId, value) },
            },
            "liveOutput.defaultOutputMonitorId" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { DefaultOutputMonitorId = Cast<string>(keyId, value) },
            },
            "liveOutput.useSafetyConfirmations" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { UseSafetyConfirmations = Cast<bool>(keyId, value) },
            },
            "liveOutput.showLyricsMonitorAlertBox" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ShowLyricsMonitorAlertBox = Cast<bool>(keyId, value) },
            },
            "liveOutput.advanceNextItem" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { AdvanceNextItem = Cast<bool>(keyId, value) },
            },
            "liveOutput.gapItemOption" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { GapItemOption = Cast<GapItemMode>(keyId, value) },
            },
            "liveOutput.gapItemLogoFile" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { GapItemLogoFile = Cast<string>(keyId, value) },
            },
            "liveOutput.gapItemUseFade" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { GapItemUseFade = Cast<bool>(keyId, value) },
            },
            "liveOutput.displayAlwaysUseSecondaryMonitor" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { DisplayAlwaysUseSecondaryMonitor = Cast<bool>(keyId, value) },
            },
            "liveOutput.displayCustomTop" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { DisplayCustomTop = Cast<int>(keyId, value) },
            },
            "liveOutput.displayCustomLeft" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { DisplayCustomLeft = Cast<int>(keyId, value) },
            },
            "liveOutput.displayCustomWidth" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { DisplayCustomWidth = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTextColorArgb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTextColorArgb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundColorArgb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundColorArgb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundColor2Argb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundColor2Argb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundIsGradient" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundIsGradient = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowNotations" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowNotations = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTextAlignment" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTextAlignment = Cast<LyricsTextAlignment>(keyId, value) },
            },
            "liveOutput.lyricsMonitorVerticalAlignment" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorVerticalAlignment = Cast<LyricsVerticalAlignment>(keyId, value) },
            },
            "liveOutput.lyricsMonitorFontSize" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorFontSize = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBold" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBold = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorItalic" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorItalic = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShadow" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShadow = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorLineSpacingPercent" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorLineSpacingPercent = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowPositionIndicator" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowPositionIndicator = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowItemNumber" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowItemNumber = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowCopyright" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowCopyright = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowNextItem" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowNextItem = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorUseFadeTransition" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorUseFadeTransition = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTransitionDurationMs" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTransitionDurationMs = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTransitionKind" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTransitionKind = Cast<LyricsTransitionKind>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundImagePath" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundImagePath = Cast<string>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowTitleHeading" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowTitleHeading = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorOutline" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorOutline = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTitleHeadingAlignment" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTitleHeadingAlignment = Cast<LyricsTextAlignment>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTitleHeadingFirstScreenOnly" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTitleHeadingFirstScreenOnly = Cast<bool>(keyId, value) },
            },
            "liveOutput.autoRotateIntervalSeconds" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { AutoRotateIntervalSeconds = Cast<int>(keyId, value) },
            },
            "powerPoint.usePowerPointTab" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { UsePowerPointTab = Cast<bool>(keyId, value) },
            },
            "powerPoint.noPanelOverlay" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { NoPanelOverlay = Cast<bool>(keyId, value) },
            },
            "powerPoint.renderTimeoutSeconds" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { RenderTimeoutSeconds = Cast<int>(keyId, value) },
            },
            "powerPoint.thumbnailCacheMegabytes" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { ThumbnailCacheMegabytes = Cast<int>(keyId, value) },
            },
            "powerPoint.maxFiles" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { MaxFiles = Cast<int>(keyId, value) },
            },
            "media.useMediaTab" => snapshot with
            {
                Media = snapshot.Media with { UseMediaTab = Cast<bool>(keyId, value) },
            },
            "media.noPanelOverlay" => snapshot with
            {
                Media = snapshot.Media with { NoPanelOverlay = Cast<bool>(keyId, value) },
            },
            "media.directory" => snapshot with
            {
                Media = snapshot.Media with { Directory = Cast<string>(keyId, value) },
            },
            "media.volume" => snapshot with
            {
                Media = snapshot.Media with { Volume = Cast<double>(keyId, value) },
            },
            "media.balance" => snapshot with
            {
                Media = snapshot.Media with { Balance = Cast<double>(keyId, value) },
            },
            "media.muted" => snapshot with
            {
                Media = snapshot.Media with { Muted = Cast<bool>(keyId, value) },
            },
            "media.liveCameraNumber" => snapshot with
            {
                Media = snapshot.Media with { LiveCameraNumber = Cast<int>(keyId, value) },
            },
            "data.adminDatabasePath" => snapshot with
            {
                Data = snapshot.Data with { AdminDatabasePath = Cast<string>(keyId, value) },
            },
            "data.backupRoot" => snapshot with
            {
                Data = snapshot.Data with { BackupRoot = Cast<string>(keyId, value) },
            },
            "advanced.enableDiagnostics" => snapshot with
            {
                Advanced = snapshot.Advanced with { EnableDiagnostics = Cast<bool>(keyId, value) },
            },
            _ => throw new ArgumentOutOfRangeException(nameof(keyId), keyId, "Unknown setting key."),
        };

    private static TTarget Cast<TTarget>(string keyId, object? value)
    {
        if (value is null && !typeof(TTarget).IsValueType)
        {
            return default!;
        }

        if (value is TTarget typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Setting '{keyId}' expects {typeof(TTarget).Name}.");
    }

    private static EasiSettingsSnapshot ApplyLegacyString(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        Func<string, EasiSettingsSnapshot> apply)
        => ApplyLegacyString(source, [legacyKey], current, apply);

    private static EasiSettingsSnapshot ApplyLegacyString(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        Func<string, EasiSettingsSnapshot> apply)
        => TryGetLegacyString(source, legacyKeys, out var raw, out _) ? apply(raw) : current;

    private static EasiSettingsSnapshot ApplyLegacyBool(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<bool, EasiSettingsSnapshot> apply)
        => ApplyLegacyBool(source, [legacyKey], current, issues, apply);

    private static EasiSettingsSnapshot ApplyLegacyBool(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<bool, EasiSettingsSnapshot> apply)
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (TryParseLegacyBool(raw, out var parsed))
        {
            return apply(parsed);
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid Boolean."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyInt(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<int, EasiSettingsSnapshot> apply)
        => ApplyLegacyInt(source, [legacyKey], current, issues, apply);

    private static EasiSettingsSnapshot ApplyLegacyInt(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<int, EasiSettingsSnapshot> apply)
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return apply(parsed);
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid integer."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyDouble(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<double, EasiSettingsSnapshot> apply)
        => ApplyLegacyDouble(source, [legacyKey], current, issues, value => value, apply);

    private static EasiSettingsSnapshot ApplyLegacyDouble(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<double, double> normalize,
        Func<double, EasiSettingsSnapshot> apply)
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return apply(normalize(parsed));
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid number."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyEnum<TEnum>(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<TEnum, EasiSettingsSnapshot> apply)
        where TEnum : struct, Enum
        => ApplyLegacyEnum(source, [legacyKey], current, issues, apply);

    private static EasiSettingsSnapshot ApplyLegacyEnum<TEnum>(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<TEnum, EasiSettingsSnapshot> apply)
        where TEnum : struct, Enum
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return apply(parsed);
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid {typeof(TEnum).Name}."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyShortcutOverrides(
        ILegacySettingsSource source,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues)
    {
        var shortcuts = new Dictionary<string, string>(current.Shortcuts, StringComparer.OrdinalIgnoreCase);
        var changed = false;

        var hasF7 = TryGetLegacyBool(source, "GlobalHookKey_F7", issues, out var useF7);
        var hasF8 = TryGetLegacyBool(source, "GlobalHookKey_F8", issues, out var useF8);
        if (hasF7 && useF7)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LiveGo, isGlobal: true, "F7");
            changed = true;
        }
        else if (hasF8 && useF8)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LiveGo, isGlobal: true, "F8");
            changed = true;
        }

        var hasF9 = TryGetLegacyBool(source, "GlobalHookKey_F9", issues, out var useF9);
        var hasF10 = TryGetLegacyBool(source, "GlobalHookKey_F10", issues, out var useF10);
        if (hasF9 && useF9)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LiveBlack, isGlobal: true, "F9");
            changed = true;
        }
        else if (hasF10 && useF10)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LiveBlack, isGlobal: true, "F10");
            changed = true;
        }

        var hasArrow = TryGetLegacyBool(source, "GlobalHookKey_Arrow", issues, out var useArrow);
        var hasCtrlArrow = TryGetLegacyBool(source, "GlobalHookKey_CtrlArrow", issues, out var useCtrlArrow);
        if (hasArrow && useArrow)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LivePrevious, isGlobal: true, "Up");
            PutShortcutOverride(shortcuts, MainCommandIds.LiveNext, isGlobal: true, "Down");
            changed = true;
        }
        else if (hasCtrlArrow && useCtrlArrow)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LivePrevious, isGlobal: true, "Ctrl+Up");
            PutShortcutOverride(shortcuts, MainCommandIds.LiveNext, isGlobal: true, "Ctrl+Down");
            changed = true;
        }

        if (TryGetLegacyInt(source, "KeyBoardOption", issues, out var keyBoardOption))
        {
            if (keyBoardOption == 1)
            {
                PutShortcutOverride(shortcuts, MainCommandIds.LivePrevious, isGlobal: false, "PageUp");
                PutShortcutOverride(shortcuts, MainCommandIds.LiveNext, isGlobal: false, "PageDown");
                changed = true;
            }
            else if (keyBoardOption != 0)
            {
                issues.Add(Warning("KeyBoardOption", $"Legacy value '{keyBoardOption}' is not a supported keyboard option."));
            }
        }

        return changed ? current with { Shortcuts = shortcuts } : current;
    }

    private static void PutShortcutOverride(
        IDictionary<string, string> shortcuts,
        string commandId,
        bool isGlobal,
        string gesture)
    {
        shortcuts[ShortcutSettings.GetSlotId(commandId, isGlobal)] = ShortcutSettings.NormalizeGesture(gesture);
    }

    private static bool TryGetLegacyBool(
        ILegacySettingsSource source,
        string legacyKey,
        ICollection<SettingsIssue> issues,
        out bool value)
    {
        if (!TryGetLegacyString(source, [legacyKey], out var raw, out var matchedKey))
        {
            value = false;
            return false;
        }

        if (TryParseLegacyBool(raw, out value))
        {
            return true;
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid Boolean."));
        value = false;
        return false;
    }

    private static bool TryGetLegacyInt(
        ILegacySettingsSource source,
        string legacyKey,
        ICollection<SettingsIssue> issues,
        out int value)
    {
        if (!TryGetLegacyString(source, [legacyKey], out var raw, out var matchedKey))
        {
            value = 0;
            return false;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid integer."));
        value = 0;
        return false;
    }

    private static bool TryGetLegacyString(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        out string value,
        out string matchedKey)
    {
        foreach (var legacyKey in legacyKeys)
        {
            if (source.TryGetString(legacyKey, out var raw) && raw is not null)
            {
                value = raw;
                matchedKey = legacyKey;
                return true;
            }
        }

        value = "";
        matchedKey = legacyKeys.Count > 0 ? legacyKeys[0] : "";
        return false;
    }

    private static bool TryParseLegacyBool(string raw, out bool value)
    {
        if (bool.TryParse(raw, out value))
        {
            return true;
        }

        if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "y", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "n", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }

    private static double NormalizeLegacyUnitScale(double value)
        => Math.Abs(value) > 1.0 ? value / 100.0 : value;

    private static void RequireText(string? value, string key, ICollection<SettingsIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(Error(key, "Value cannot be empty."));
        }
    }

    private static void RequireRange(int value, int min, int max, string key, ICollection<SettingsIssue> issues)
    {
        if (value < min || value > max)
        {
            issues.Add(Error(key, $"Value must be between {min} and {max}."));
        }
    }

    private static void RequireRange(double value, double min, double max, string key, ICollection<SettingsIssue> issues)
    {
        if (double.IsNaN(value) || value < min || value > max)
        {
            issues.Add(Error(key, $"Value must be between {min} and {max}."));
        }
    }

    private static void ValidatePath(
        string? path,
        string key,
        ICollection<SettingsIssue> issues,
        bool allowEmpty)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!allowEmpty)
            {
                issues.Add(Error(key, "Path cannot be empty."));
            }

            return;
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            issues.Add(Error(key, "Path contains invalid characters."));
        }
    }

    private static void RequireNoControlCharacters(string value, string key, ICollection<SettingsIssue> issues)
    {
        if (value.Any(char.IsControl))
        {
            issues.Add(Error(key, "Value cannot contain control characters."));
        }
    }

    private static SettingsIssue Error(string key, string message)
        => new(key, SettingsIssueSeverity.Error, message);

    private static SettingsIssue Warning(string key, string message)
        => new(key, SettingsIssueSeverity.Warning, message);

    private static bool IsFileSystemException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or NotSupportedException;

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
