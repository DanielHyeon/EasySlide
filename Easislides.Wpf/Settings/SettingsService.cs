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

public sealed record SettingsServiceOptions(string SettingsFilePath, string BackupRoot, string? LegacyWorkingFolderPath = null)
{
    private const string LegacyRootWorkingFolder = @"C:\EasiSlides";

    public static SettingsServiceOptions CreateDefault()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasislidesNext");

        return new SettingsServiceOptions(
            Path.Combine(root, "settings.json"),
            Path.Combine(root, "Backups"),
            LegacyRootWorkingFolder);
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

// 보조 영역(Region2) 전역 가로 정렬 — 정렬 enum 은 "추종" 상태가 없어, 본문 추종을 뜻하는 FollowRegion1=0 센티넬을
// 가진 별도 enum 으로 둔다(글꼴2·크기2·색2 의 "0/빈값=추종"과 같은 개념을 정렬에 맞춘 것). 기본 FollowRegion1=무회귀.
public enum LyricsRegion2Alignment
{
    FollowRegion1 = 0, // 본문(Region1) 정렬을 그대로 따름(기본).
    Left = 1,
    Center = 2,
    Right = 3,
}

// 보조 영역(Region2) 전역 강조(굵게 등) 3-상태 — bool 은 "추종" 상태가 없어, 본문 추종을 뜻하는 FollowRegion1=0 센티넬을
// 가진 enum 으로 둔다(정렬2 의 FollowRegion1 센티넬과 같은 개념). 기본 FollowRegion1=본문 효과 추종(무회귀).
public enum LyricsRegion2Emphasis
{
    FollowRegion1 = 0, // 본문(Region1) 효과를 그대로 따름(기본).
    On = 1,            // 보조 영역에서 강조 켬(본문과 무관하게).
    Off = 2,           // 보조 영역에서 강조 끔(본문과 무관하게).
}

// 출력 화면 가사 세로 정렬(인-셸 가사 정렬 — 레거시 Align Top/Bottom). 기본 Center(기존 동작 보존).
public enum LyricsVerticalAlignment
{
    Top = 0,
    Center = 1,
    Bottom = 2,
}

// 출력 배경 이미지 표시 모드(레거시 FrmMain Def_ImageMode/Ind_ImageMode: Tile/Centre/BestFit).
// 기본 Fill = 기존 UniformToFill(화면 가득, 가장자리 크롭) 동작 보존(무회귀).
public enum LyricsBackgroundMode
{
    Fill = 0,   // 화면을 가득 채우되 비율 유지(가장자리 크롭) — UniformToFill. 기존 동작.
    Fit = 1,    // 이미지 전체가 보이도록 비율 유지(레터박스) — Uniform. 레거시 BestFit.
    Center = 2, // 원본 크기로 화면 가운데 — Stretch 없음. 레거시 Centre.
    Tile = 3,   // 원본 크기로 바둑판 반복 — TileMode=Tile. 레거시 Tile.
}

// 출력 배경 2색 그라데이션 방향(레거시 FrmMain Def_BackColour 패턴 대응). 기본 Vertical = 위→아래(기존 동작 보존, 무회귀).
public enum LyricsGradientDirection
{
    Vertical = 0,     // 위→아래(기존 동작). StartPoint(0.5,0)→EndPoint(0.5,1).
    Horizontal = 1,   // 왼쪽→오른쪽. (0,0.5)→(1,0.5).
    DiagonalDown = 2, // 좌상→우하 대각선. (0,0)→(1,1).
    DiagonalUp = 3,   // 좌하→우상 대각선. (0,1)→(1,0).
}

// 이중 언어 곡의 영역 표시 모드(레거시 FrmMain Def_ShowRegion1/2/Both). 기본 Both = 둘 다 표시(기존 동작 보존, 무회귀).
// 단일 영역 곡엔 영향 없다(Region2 본문이 없으므로 항상 Region1 만 보인다).
public enum LyricsRegionDisplay
{
    Both = 0,        // Region1(주 언어)·Region2(보조 언어) 둘 다 표시. 기본.
    Region1Only = 1, // Region1 만 표시(보조 언어 숨김).
    Region2Only = 2, // Region2 만 표시(주 언어 숨김) — 단, 보조 언어가 없으면 Region1 을 보여 화면이 비지 않게 한다.
}

// 자동 회전(Auto Rotate) 동작 모드(레거시 FrmMain One/One-Repeat/Group/Group-Repeat).
// 매 간격마다 다음 절/슬라이드로 넘어가며, 현재 항목의 끝에 다다랐을 때 무엇을 할지가 모드별로 다르다.
public enum AutoRotateMode
{
    // 현재 항목만 순환 반복(끝 절/슬라이드 다음은 같은 항목 첫 절로). 기존 동작 = 기본값(무회귀).
    OneRepeat = 0,
    // 현재 항목 한 바퀴만 — 마지막 절/슬라이드까지 가면 자동 회전을 멈춘다(반복·항목 이동 없음).
    One = 1,
    // 항목 그룹 순회 — 현재 항목 끝나면 다음 예배 순서 항목으로 넘어가고, 마지막 항목 끝나면 멈춘다.
    Group = 2,
    // 항목 그룹 순회 + 반복 — 마지막 항목까지 끝나면 첫 항목으로 돌아가 계속 순환한다.
    GroupRepeat = 3,
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
    // 시계 와이프 변형 — Spiral=콘텐츠 회전+부채꼴 스윕, WindMill=4날개 바람개비, FanUp=하단 중앙에서 부채 펼침.
    Spiral = 27,
    WindMill = 28,
    FanUp = 29,
}

public static class EasiSettingKeys
{
    public static readonly SettingKey<string> Language = new("general.language", "ko-KR");
    public static readonly SettingKey<string> WorkingFolder = new(
        "general.workingFolder",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasiSlides"));
    public static readonly SettingKey<bool> OnboardingCompleted = new("general.onboardingCompleted", false);
    // 라이브러리 곡 목록에 곡 번호 표시(레거시 Edit 메뉴 "Use Song Numbering"). 기본 off → 제목만(무회귀).
    public static readonly SettingKey<bool> UseSongNumbering = new("general.useSongNumbering", false);
    public static readonly SettingKey<string> RegistrationUser = new("general.registrationUser", "");
    // 메인 창 크기·위치 저장(레거시 FrmMain 창 상태 레지스트리 저장). Width/Height=0 이면 "저장된 적 없음" → 기본 크기·중앙.
    // 닫을 때 복원 좌표(최대화면 RestoreBounds)를 저장하고, 열 때 화면 안으로 보정해 되살린다(모니터 분리·해상도 변경 대비).
    public static readonly SettingKey<int> MainWindowLeft = new("general.mainWindowLeft", 0);
    public static readonly SettingKey<int> MainWindowTop = new("general.mainWindowTop", 0);
    public static readonly SettingKey<int> MainWindowWidth = new("general.mainWindowWidth", 0);
    public static readonly SettingKey<int> MainWindowHeight = new("general.mainWindowHeight", 0);
    public static readonly SettingKey<bool> MainWindowMaximized = new("general.mainWindowMaximized", false);
    // 우측 출력 모양 인스펙터 펼침/접힘 상태 저장(레거시 FrmMain 패널 상태). 기본 true=펼침(무회귀). 접어 두면 다음 실행에도 접힌 채 시작.
    public static readonly SettingKey<bool> MainInspectorExpanded = new("general.mainInspectorExpanded", true);
    // 좌측 브라우저/예배순서 패널의 높이 비율(위 브라우저 패널 %, 레거시 splitter 위치 저장). 0=저장된 적 없음 → XAML 기본(반반).
    public static readonly SettingKey<int> MainBrowserSplitPercent = new("general.mainBrowserSplitPercent", 0);

    public static readonly SettingKey<ColorTheme> Theme = new("appearance.theme", ColorTheme.Light);
    public static readonly SettingKey<InterfaceSize> InterfaceSize =
        new("appearance.interfaceSize", Easislides.Wpf.Theme.InterfaceSize.Standard);
    public static readonly SettingKey<string> DefaultOutputMonitorId = new("liveOutput.defaultOutputMonitorId", "");
    // 스테이지(Preview) 모니터 — 마지막으로 고른 모니터 Id 를 기억해 다음 실행에도 같은 모니터를 기본 선택한다(출력 DefaultOutputMonitorId 와 같은 취지).
    public static readonly SettingKey<string> PreviewMonitorId = new("liveOutput.previewMonitorId", "");
    public static readonly SettingKey<bool> UseSafetyConfirmations = new("liveOutput.useSafetyConfirmations", true);
    public static readonly SettingKey<bool> ShowLyricsMonitorAlertBox = new("liveOutput.showLyricsMonitorAlertBox", false);
    // FrmOptions Reference_Source0..4. 0=None, 1=Song Title, 2=Song Number, 3=Book Reference, 4=User Reference.
    public static readonly SettingKey<int> ReferenceAlertSource = new("liveOutput.referenceAlertSource", 1);
    public static readonly SettingKey<bool> ReferenceAlertUsePick = new("liveOutput.referenceAlertUsePick", false);
    public static readonly SettingKey<bool> ReferenceAlertBlankIfPickNotFound = new("liveOutput.referenceAlertBlankIfPickNotFound", false);
    public static readonly SettingKey<string> ReferenceAlertPickName = new("liveOutput.referenceAlertPickName", "");
    public static readonly SettingKey<string> ReferenceAlertPickSubstitute = new("liveOutput.referenceAlertPickSubstitute", "");
    public static readonly SettingKey<string> ReferenceAlertPickSeparator = new("liveOutput.referenceAlertPickSeparator", ",");
    // FrmOptions ReferenceAlertDuration. Legacy registry accepts 1..999 seconds; FrmOptions UI normally caps at 60.
    public static readonly SettingKey<int> ReferenceAlertDurationSeconds = new("liveOutput.referenceAlertDurationSeconds", 20);
    // FrmOptions ReferenceAlertStyle bit 1. Text enters as a left-to-right reveal in legacy; WPF marquee rendering is tracked separately.
    public static readonly SettingKey<bool> ReferenceAlertScroll = new("liveOutput.referenceAlertScroll", true);
    // FrmOptions ReferenceAlertStyle bit 2. WPF flashes the reference overlay brushes while the alert is visible.
    public static readonly SettingKey<bool> ReferenceAlertFlash = new("liveOutput.referenceAlertFlash", false);
    // FrmOptions ReferenceAlertStyle bit 3. Removes the reference overlay background band.
    public static readonly SettingKey<bool> ReferenceAlertTransparent = new("liveOutput.referenceAlertTransparent", false);
    public static readonly SettingKey<bool> AdvanceNextItem = new("liveOutput.advanceNextItem", false);
    public static readonly SettingKey<GapItemMode> GapItemOption = new("liveOutput.gapItemOption", GapItemMode.None);
    public static readonly SettingKey<string> GapItemLogoFile = new("liveOutput.gapItemLogoFile", "");
    public static readonly SettingKey<bool> GapItemUseFade = new("liveOutput.gapItemUseFade", true);
    public static readonly SettingKey<bool> DisplayAlwaysUseSecondaryMonitor = new("liveOutput.displayAlwaysUseSecondaryMonitor", true);
    public static readonly SettingKey<int> DisplayCustomTop = new("liveOutput.displayCustomTop", 0);
    public static readonly SettingKey<int> DisplayCustomLeft = new("liveOutput.displayCustomLeft", 0);
    public static readonly SettingKey<int> DisplayCustomWidth = new("liveOutput.displayCustomWidth", 100);
    public static readonly SettingKey<int> LyricsMonitorTextColorArgb = new("liveOutput.lyricsMonitorTextColorArgb", -16777216);
    public static readonly SettingKey<int> LyricsMonitorHighlightColorArgb = new("liveOutput.lyricsMonitorHighlightColorArgb", -65536);
    // 보조 영역(Region2) 전역 글자색(ARGB). 0(투명)=본문(Region1) 색 추종(무회귀, 글꼴2·크기2 의 "0=자동"과 동일 개념).
    // 곡별 FormatData 30(per-song region2 색)이 있으면 그 곡 동안은 곡별 색이 우선한다.
    public static readonly SettingKey<int> LyricsMonitorTextColor2Argb = new("liveOutput.lyricsMonitorTextColor2Argb", 0);
    // 보조 영역(Region2) 전역 가로 정렬(FrmMain Ind_Reg2Align). FollowRegion1=본문 정렬 추종(기본, 무회귀). 곡별 정렬(32)이 우선.
    public static readonly SettingKey<LyricsRegion2Alignment> LyricsMonitorRegion2Alignment =
        new("liveOutput.lyricsMonitorRegion2Alignment", LyricsRegion2Alignment.FollowRegion1);
    // 보조 영역(Region2) 전역 굵게(FrmMain Ind_Reg2Bold). FollowRegion1=본문 굵게 추종(기본, 무회귀). 곡별 굵게(41 region2)가 우선.
    public static readonly SettingKey<LyricsRegion2Emphasis> LyricsMonitorRegion2Bold =
        new("liveOutput.lyricsMonitorRegion2Bold", LyricsRegion2Emphasis.FollowRegion1);
    // 보조 영역(Region2) 전역 기울임(FrmMain Ind_Reg2Italic). FollowRegion1=본문 기울임 추종(기본, 무회귀). 곡별 기울임이 우선.
    public static readonly SettingKey<LyricsRegion2Emphasis> LyricsMonitorRegion2Italic =
        new("liveOutput.lyricsMonitorRegion2Italic", LyricsRegion2Emphasis.FollowRegion1);
    // 보조 영역(Region2) 전역 밑줄(FrmMain Ind_Reg2Underline). FollowRegion1=본문 밑줄 추종(기본, 무회귀). 곡별 밑줄이 우선.
    public static readonly SettingKey<LyricsRegion2Emphasis> LyricsMonitorRegion2Underline =
        new("liveOutput.lyricsMonitorRegion2Underline", LyricsRegion2Emphasis.FollowRegion1);
    public static readonly SettingKey<int> LyricsMonitorBackgroundColorArgb = new("liveOutput.lyricsMonitorBackgroundColorArgb", -1);
    // 배경 그라데이션 끝색(ARGB). IsGradient=true 일 때 배경색→이 색 세로 그라데이션 송출(G2 / FrmBackground 슬라이스).
    public static readonly SettingKey<int> LyricsMonitorBackgroundColor2Argb = new("liveOutput.lyricsMonitorBackgroundColor2Argb", -1);
    // 배경 그라데이션 사용 여부(기본 false=솔리드). true 면 배경색→끝색 세로 그라데이션.
    public static readonly SettingKey<bool> LyricsMonitorBackgroundIsGradient = new("liveOutput.lyricsMonitorBackgroundIsGradient", false);
    // 코드(악상) 표시 — on 이면 가사 줄 위에 '»' 뒤 코드 줄을 회중 화면에 함께 송출(레거시 ShowNotations).
    // 기본 false: 회중 화면은 예부터 코드를 숨겼고, 끄면 본문이 기존과 비트 동일(무회귀)하다. 연주팀 운영 시에만 켠다.
    public static readonly SettingKey<bool> LyricsMonitorShowNotations = new("liveOutput.lyricsMonitorShowNotations", false);
    // 출력 가사 가로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center 로 기존 가운데 정렬 동작을 보존.
    public static readonly SettingKey<LyricsTextAlignment> LyricsMonitorTextAlignment =
        new("liveOutput.lyricsMonitorTextAlignment", LyricsTextAlignment.Center);
    // 출력 가사 세로 정렬(인-셸 가사 정렬 §7.3-A). 기본 Center 로 기존 가운데 정렬 동작을 보존.
    public static readonly SettingKey<LyricsVerticalAlignment> LyricsMonitorVerticalAlignment =
        new("liveOutput.lyricsMonitorVerticalAlignment", LyricsVerticalAlignment.Center);
    // 출력 가사 폰트 크기(px, 인-셸 가사 포맷팅 §7.3-A). 기본 48 로 기존 출력 폰트 크기를 보존. 범위 24~120.
    public static readonly SettingKey<int> LyricsMonitorFontSize = new("liveOutput.lyricsMonitorFontSize", 48);
    // 보조 영역(Region2) 전역 폰트 크기(px, FrmMain Ind_Reg2SizeUpDown). 기본 0 = "본문(Region1)과 동일" 자동(무회귀).
    // 0 이 아니면 이중 언어 곡의 Region2 본문에 이 크기를 적용(곡별 region2 크기 48 오버라이드가 있으면 그게 우선). 범위 0 또는 24~120.
    public static readonly SettingKey<int> LyricsMonitorFontSize2 = new("liveOutput.lyricsMonitorFontSize2", 0);
    // 출력 가사 전역 글꼴명(레거시 Def_FontName, FrmMain 출력 기본 글꼴). 비었으면 테마 기본 글꼴 상속(무회귀).
    // 곡별 FormatData 43(per-song) 글꼴이 있으면 그 곡 동안은 곡별 글꼴이 우선하고, 없으면 이 전역 글꼴을 쓴다.
    public static readonly SettingKey<string> LyricsMonitorFontFamily = new("liveOutput.lyricsMonitorFontFamily", "");
    // 보조 영역(Region2) 전역 글꼴명(FrmMain Ind_Reg2Font). 비었으면 본문(Region1) 글꼴을 추종(무회귀, 폰트 크기2 와 동일한 0=자동 개념).
    // 곡별 FormatData 44(per-song region2 글꼴)가 있으면 그 곡 동안은 곡별 글꼴이 우선한다.
    public static readonly SettingKey<string> LyricsMonitorFontFamily2 = new("liveOutput.lyricsMonitorFontFamily2", "");
    // 출력 가사 폰트 효과(인-셸 가사 포맷팅 §7.3-A). 모두 기본 off 로 기존 출력 모양 보존.
    // Bold off = 기존 SemiBold, Italic off = Normal, Shadow off = 효과 없음.
    public static readonly SettingKey<bool> LyricsMonitorBold = new("liveOutput.lyricsMonitorBold", false);
    public static readonly SettingKey<bool> LyricsMonitorItalic = new("liveOutput.lyricsMonitorItalic", false);
    public static readonly SettingKey<bool> LyricsMonitorShadow = new("liveOutput.lyricsMonitorShadow", false);
    // 가사 본문 밑줄(레거시 Ind_R1/R2Underline). off(기본)=밑줄 없음(무회귀). 전역 효과로 Region1/2 본문에 함께 적용.
    public static readonly SettingKey<bool> LyricsMonitorUnderline = new("liveOutput.lyricsMonitorUnderline", false);
    // 강조(굵게·기울임·밑줄)를 "후렴만" 적용(레거시 Ind_*Italics 3종 중 후렴만). off(기본)=전체 절에 적용(무회귀).
    // on 이면 현재 절이 후렴([C]/[Chorus]/[후렴])일 때만 강조하고 그 외 절은 강조를 끈다(곡 강세를 후렴에 모은다).
    public static readonly SettingKey<bool> LyricsMonitorEmphasisChorusOnly = new("liveOutput.lyricsMonitorEmphasisChorusOnly", false);
    // 이중 언어 줄 교차(인터레이스, 레거시 Def_Interlace). on 이면 Region1·Region2 본문을 줄 단위로 번갈아 송출한다
    // (원문 줄 → 번역 줄 → 원문 줄 …). 기본 off=영역별 블록(Region1 위, Region2 아래, 무회귀). 두 영역이 다 보일 때만 의미.
    public static readonly SettingKey<bool> LyricsMonitorInterlace = new("liveOutput.lyricsMonitorInterlace", false);
    // Display Panel 전체 표시(레거시 Def_PanelShow). off 이면 제목/상태/곡번호/저작권/다음항목/위치 밴드 전체를 숨긴다.
    // 기본 on=현재 WPF 오버레이 표시 동작 보존(무회귀).
    public static readonly SettingKey<bool> LyricsMonitorShowDisplayPanel = new("liveOutput.lyricsMonitorShowDisplayPanel", true);
    // Display Panel 배경 투명(레거시 Def_PanelTransparent). on 이면 곡번호·저작권·다음항목·위치 인디케이터 밴드의
    // 어두운 배경(#66000000)을 없애 텍스트가 슬라이드 위에 바로 보인다. 기본 off=기존 반투명 밴드(무회귀).
    public static readonly SettingKey<bool> LyricsMonitorPanelTransparent = new("liveOutput.lyricsMonitorPanelTransparent", false);
    // Display Panel 밴드 배경색(ARGB, 레거시 Def_PanelColour). 기본 0x66000000=반투명 검정(기존 동작 보존, 무회귀).
    // 알파(반투명)는 밴드 뒤 가사가 비치도록 유지하고 RGB(색조)만 사용자가 바꾼다. 투명 토글 on 이면 이 색 대신 완전 투명.
    public static readonly SettingKey<int> LyricsMonitorPanelColorArgb =
        new("liveOutput.lyricsMonitorPanelColorArgb", unchecked((int)0x66000000));
    // Display Panel 정보 텍스트(곡번호·저작권·다음항목·위치) 글자 크기 비율(%, FrmMain Def_PanelFont 크기 대응).
    // 기본 100 = 기존 크기 보존(무회귀). 큰 장소·가독성 위해 확대/축소. 범위 50~200.
    public static readonly SettingKey<int> LyricsMonitorPanelFontScalePercent =
        new("liveOutput.lyricsMonitorPanelFontScalePercent", 100);
    // Display Panel 글자색이 Region1(본문) 글자색을 따를지 여부(레거시 Def_PanelAsR1). 기본 true=기존 본문색 추종.
    public static readonly SettingKey<bool> LyricsMonitorPanelTextColorFollowRegion1 =
        new("liveOutput.lyricsMonitorPanelTextColorFollowRegion1", true);
    // Display Panel 전용 글자색(ARGB, 레거시 Def_PanelTextColour). AsR1 off 일 때만 사용.
    public static readonly SettingKey<int> LyricsMonitorPanelTextColorArgb =
        new("liveOutput.lyricsMonitorPanelTextColorArgb", -16777216);
    // Display Panel 전용 글자 효과(레거시 Def_PanelFont B/I/U). 기본 off=기존 패널 가중치/정자/무밑줄 유지.
    public static readonly SettingKey<bool> LyricsMonitorPanelBold = new("liveOutput.lyricsMonitorPanelBold", false);
    public static readonly SettingKey<bool> LyricsMonitorPanelItalic = new("liveOutput.lyricsMonitorPanelItalic", false);
    public static readonly SettingKey<bool> LyricsMonitorPanelUnderline = new("liveOutput.lyricsMonitorPanelUnderline", false);
    // 출력 가사 줄 간격(폰트 크기 대비 %, 인-셸 가사 포맷팅 §7.3-A). 기본 125 로 기존 줄높이(폰트×1.25) 보존. 범위 100~220.
    public static readonly SettingKey<int> LyricsMonitorLineSpacingPercent = new("liveOutput.lyricsMonitorLineSpacingPercent", 125);
    // 출력 본문 좌/우/아래 여백(픽셀) — FrmMain ShowLeftMargin/ShowRightMargin/ShowBottomMargin 대응.
    // 본문 묶음을 화면 가장자리에서 안쪽으로 들여써 잘림·치우침을 보정한다. 기본 0=기존 레이아웃(무회귀). 범위 0~400.
    public static readonly SettingKey<int> LyricsMonitorBodyLeftMargin = new("liveOutput.lyricsMonitorBodyLeftMargin", 0);
    public static readonly SettingKey<int> LyricsMonitorBodyRightMargin = new("liveOutput.lyricsMonitorBodyRightMargin", 0);
    public static readonly SettingKey<int> LyricsMonitorBodyBottomMargin = new("liveOutput.lyricsMonitorBodyBottomMargin", 0);
    // 이중 언어 곡의 Region1↔Region2 세로 간격(px, FrmMain Ind_Reg2TopUpDown 상대 위치 대응). 기본 8=기존 간격(무회귀). 범위 0~100.
    public static readonly SettingKey<int> LyricsMonitorRegionGapPx = new("liveOutput.lyricsMonitorRegionGapPx", 8);
    // 본문 세로 위치 미세 오프셋(px, FrmMain Ind_Reg1TopUpDown 본문 세로위치 대응). 음수=위로, 양수=아래로 이동.
    // 정렬(위/가운데/아래)을 유지한 채 본문 묶음을 시각적으로 N px 옮긴다(TranslateTransform). 기본 0=이동 없음(무회귀). 범위 -300~300.
    public static readonly SettingKey<int> LyricsMonitorBodyVerticalOffset = new("liveOutput.lyricsMonitorBodyVerticalOffset", 0);
    // 출력 위치 인디케이터 표시(절/슬라이드 "N/M", 인-셸 §7.3-A). 기본 off.
    public static readonly SettingKey<bool> LyricsMonitorShowPositionIndicator = new("liveOutput.lyricsMonitorShowPositionIndicator", false);
    // 절 헤딩 표시(현재 절의 섹션 라벨 "1절"/"후렴" 등을 본문 위에 표시, FrmMain Def_Head All). 기본 off.
    public static readonly SettingKey<bool> LyricsMonitorShowVerseHeading = new("liveOutput.lyricsMonitorShowVerseHeading", false);
    // 출력에 곡 번호 표시(FrmMain "Show Item Number"/"Use Song Numbering", Display Panel). 기본 off → 기존 동작 보존.
    public static readonly SettingKey<bool> LyricsMonitorShowItemNumber = new("liveOutput.lyricsMonitorShowItemNumber", false);
    // 정보 패널에 곡 제목 표시(FrmMain Def_PanelTitle) — 상단 좌측 밴드에 현재 곡 제목을 작게. 기본 off → 기존 동작 보존.
    public static readonly SettingKey<bool> LyricsMonitorShowTitleOnPanel = new("liveOutput.lyricsMonitorShowTitleOnPanel", false);
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
    public static readonly SettingKey<string> LyricsMonitorItemTransitionName =
        new("liveOutput.lyricsMonitorItemTransitionName", "");
    // 슬라이드/절 전환 모션 종류(FrmMain 항목 vs 슬라이드 전환 분리) — 같은 항목 안에서 절·슬라이드만 바뀔 때 쓰는 전환.
    // 항목이 바뀔 때(곡→곡)는 위 LyricsMonitorTransitionKind 를 쓴다. 기본 Fade(기존 단일 전환과 동일 = 무회귀).
    public static readonly SettingKey<LyricsTransitionKind> LyricsMonitorSlideTransitionKind =
        new("liveOutput.lyricsMonitorSlideTransitionKind", LyricsTransitionKind.Fade);
    public static readonly SettingKey<string> LyricsMonitorSlideTransitionName =
        new("liveOutput.lyricsMonitorSlideTransitionName", "");
    // 출력 전역 배경 이미지 경로(FrmMain Images 탭 — 배경으로 적용). 비었으면 색 배경 유지(무회귀).
    // 곡별 FormatData 61(per-song) 배경이 있으면 그 곡 동안은 곡별 배경이 우선하고, 없으면 이 전역 배경을 쓴다.
    public static readonly SettingKey<string> LyricsMonitorBackgroundImagePath = new("liveOutput.lyricsMonitorBackgroundImagePath", "");
    // 출력 배경 이미지 표시 모드(레거시 Def_ImageMode/Ind_ImageMode: Tile/Centre/BestFit). 기본 Fill=기존 UniformToFill(무회귀).
    public static readonly SettingKey<LyricsBackgroundMode> LyricsMonitorBackgroundMode =
        new("liveOutput.lyricsMonitorBackgroundMode", LyricsBackgroundMode.Fill);
    // 출력 배경 2색 그라데이션 방향(FrmMain Def_BackColour 패턴). 기본 Vertical = 위→아래(무회귀).
    public static readonly SettingKey<LyricsGradientDirection> LyricsMonitorBackgroundGradientDirection =
        new("liveOutput.lyricsMonitorBackgroundGradientDirection", LyricsGradientDirection.Vertical);
    // 이중 언어 영역 표시 모드(레거시 Def_ShowRegion1/2/Both). 기본 Both=둘 다(무회귀).
    public static readonly SettingKey<LyricsRegionDisplay> LyricsMonitorRegionDisplay =
        new("liveOutput.lyricsMonitorRegionDisplay", LyricsRegionDisplay.Both);
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
    // 제목 헤딩이 본문(Region1) 가로 정렬을 따름(FrmMain Def_HeadAlign AsR1). 기본 off → 헤딩 전용 정렬(L/C/R) 사용.
    public static readonly SettingKey<bool> LyricsMonitorTitleHeadingFollowBody =
        new("liveOutput.lyricsMonitorTitleHeadingFollowBody", false);
    // 제목 헤딩이 보조 영역(Region2) 가로 정렬을 따름(FrmMain Def_HeadAlign AsR2). 기본 off. AsR1 보다 우선.
    public static readonly SettingKey<bool> LyricsMonitorTitleHeadingFollowRegion2 =
        new("liveOutput.lyricsMonitorTitleHeadingFollowRegion2", false);
    // 자동 회전 간격(초, §7.3-B). 라이브 중 절/슬라이드를 이 간격으로 자동 전환. 기본 20초, 범위 2~600.
    public static readonly SettingKey<int> AutoRotateIntervalSeconds = new("liveOutput.autoRotateIntervalSeconds", 20);
    // 자동 회전 모드(레거시 One/One-Repeat/Group/Group-Repeat). 기본 OneRepeat=기존 동작(현재 항목만 순환).
    public static readonly SettingKey<AutoRotateMode> AutoRotateMode =
        new("liveOutput.autoRotateMode", Easislides.Wpf.Settings.AutoRotateMode.OneRepeat);
    public static readonly SettingKey<bool> UsePowerPointTab = new("powerPoint.usePowerPointTab", false);
    public static readonly SettingKey<bool> NoPowerPointPanelOverlay = new("powerPoint.noPanelOverlay", false);
    public static readonly SettingKey<int> PowerPointRenderTimeoutSeconds = new("powerPoint.renderTimeoutSeconds", 60);
    public static readonly SettingKey<int> ThumbnailCacheMegabytes = new("powerPoint.thumbnailCacheMegabytes", 256);
    public static readonly SettingKey<int> PowerPointMaxFiles = new("powerPoint.maxFiles", 20);
    public static readonly SettingKey<int> PowerPointSourceListingStyle = new("powerPoint.sourceListingStyle", -1);
    public static readonly SettingKey<bool> UseMediaTab = new("media.useMediaTab", false);
    public static readonly SettingKey<bool> NoMediaPanelOverlay = new("media.noPanelOverlay", false);
    public static readonly SettingKey<string> MediaDirectory = new("media.directory", "");
    public static readonly SettingKey<string> DefaultMediaPath = new("media.defaultMediaPath", "");
    public static readonly SettingKey<double> MediaVolume = new("media.volume", 0.8);
    public static readonly SettingKey<double> MediaBalance = new("media.balance", 0.0);
    public static readonly SettingKey<bool> MediaMuted = new("media.muted", false);
    public static readonly SettingKey<int> LiveCameraNumber = new("media.liveCameraNumber", 1);
    public static readonly SettingKey<int> PraiseBookCjkGroupStyle = new("data.praiseBookCjkGroupStyle", 0);
    public static readonly SettingKey<string> CurrentPraiseBookName = new("data.currentPraiseBookName", "");
    public static readonly SettingKey<int> SelectedSongFolderNo = new("data.selectedSongFolderNo", 0);
    public static readonly SettingKey<string> AdminDatabasePath = new("data.adminDatabasePath", "");
    public static readonly SettingKey<string> DataBackupRoot = new("data.backupRoot", "");
    public static readonly SettingKey<bool> EnableDiagnostics = new("advanced.enableDiagnostics", false);

    public static IReadOnlyList<object> All { get; } =
    [
        Language,
        WorkingFolder,
        OnboardingCompleted,
        UseSongNumbering,
        RegistrationUser,
        MainWindowLeft,
        MainWindowTop,
        MainWindowWidth,
        MainWindowHeight,
        MainWindowMaximized,
        MainInspectorExpanded,
        MainBrowserSplitPercent,
        Theme,
        InterfaceSize,
        DefaultOutputMonitorId,
        PreviewMonitorId,
        UseSafetyConfirmations,
        ShowLyricsMonitorAlertBox,
        ReferenceAlertSource,
        ReferenceAlertUsePick,
        ReferenceAlertBlankIfPickNotFound,
        ReferenceAlertPickName,
        ReferenceAlertPickSubstitute,
        ReferenceAlertPickSeparator,
        ReferenceAlertDurationSeconds,
        ReferenceAlertScroll,
        ReferenceAlertFlash,
        ReferenceAlertTransparent,
        AdvanceNextItem,
        GapItemOption,
        GapItemLogoFile,
        GapItemUseFade,
        DisplayAlwaysUseSecondaryMonitor,
        DisplayCustomTop,
        DisplayCustomLeft,
        DisplayCustomWidth,
        LyricsMonitorTextColorArgb,
        LyricsMonitorHighlightColorArgb,
        LyricsMonitorTextColor2Argb,
        LyricsMonitorRegion2Alignment,
        LyricsMonitorRegion2Bold,
        LyricsMonitorRegion2Italic,
        LyricsMonitorRegion2Underline,
        LyricsMonitorBackgroundColorArgb,
        LyricsMonitorBackgroundColor2Argb,
        LyricsMonitorBackgroundIsGradient,
        LyricsMonitorShowNotations,
        LyricsMonitorTextAlignment,
        LyricsMonitorVerticalAlignment,
        LyricsMonitorFontSize,
        LyricsMonitorFontSize2,
        LyricsMonitorFontFamily,
        LyricsMonitorFontFamily2,
        LyricsMonitorBold,
        LyricsMonitorItalic,
        LyricsMonitorShadow,
        LyricsMonitorUnderline,
        LyricsMonitorEmphasisChorusOnly,
        LyricsMonitorInterlace,
        LyricsMonitorShowDisplayPanel,
        LyricsMonitorPanelTransparent,
        LyricsMonitorPanelColorArgb,
        LyricsMonitorPanelFontScalePercent,
        LyricsMonitorPanelTextColorFollowRegion1,
        LyricsMonitorPanelTextColorArgb,
        LyricsMonitorPanelBold,
        LyricsMonitorPanelItalic,
        LyricsMonitorPanelUnderline,
        LyricsMonitorLineSpacingPercent,
        LyricsMonitorBodyLeftMargin,
        LyricsMonitorBodyRightMargin,
        LyricsMonitorBodyBottomMargin,
        LyricsMonitorRegionGapPx,
        LyricsMonitorBodyVerticalOffset,
        LyricsMonitorShowPositionIndicator,
        LyricsMonitorShowVerseHeading,
        // Display Panel 토글들(곡번호·저작권·다음항목) — All 누락 시 FindChangedKeys 가 변경을 못 잡아
        // 메뉴 토글이 라이브 출력에 즉시 반영되지 않는다(다음 GoLive 때까지 지연). 반드시 등록.
        LyricsMonitorShowItemNumber,
        LyricsMonitorShowTitleOnPanel,
        LyricsMonitorShowCopyright,
        LyricsMonitorShowNextItem,
        LyricsMonitorShowTitleHeading,
        LyricsMonitorOutline,
        LyricsMonitorTitleHeadingAlignment,
        LyricsMonitorTitleHeadingFirstScreenOnly,
        LyricsMonitorTitleHeadingFollowBody,
        LyricsMonitorTitleHeadingFollowRegion2,
        // 전환 효과(페이드 사용·길이·모션 종류) — 변경 감지·라이브 반영을 위해 등록.
        LyricsMonitorUseFadeTransition,
        LyricsMonitorTransitionDurationMs,
        LyricsMonitorTransitionKind,
        LyricsMonitorItemTransitionName,
        LyricsMonitorSlideTransitionKind,
        LyricsMonitorSlideTransitionName,
        // 전역 배경 이미지 경로·표시 모드 — 변경 감지·라이브 반영을 위해 등록.
        LyricsMonitorBackgroundImagePath,
        LyricsMonitorBackgroundMode,
        LyricsMonitorBackgroundGradientDirection,
        LyricsMonitorRegionDisplay,
        AutoRotateIntervalSeconds,
        AutoRotateMode,
        UsePowerPointTab,
        NoPowerPointPanelOverlay,
        PowerPointRenderTimeoutSeconds,
        ThumbnailCacheMegabytes,
        PowerPointMaxFiles,
        PowerPointSourceListingStyle,
        UseMediaTab,
        NoMediaPanelOverlay,
        MediaDirectory,
        DefaultMediaPath,
        MediaVolume,
        MediaBalance,
        MediaMuted,
        LiveCameraNumber,
        PraiseBookCjkGroupStyle,
        CurrentPraiseBookName,
        SelectedSongFolderNo,
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

    public bool UseSongNumbering { get; init; } = EasiSettingKeys.UseSongNumbering.DefaultValue;

    public string RegistrationUser { get; init; } = EasiSettingKeys.RegistrationUser.DefaultValue;

    public int MainWindowLeft { get; init; } = EasiSettingKeys.MainWindowLeft.DefaultValue;

    public int MainWindowTop { get; init; } = EasiSettingKeys.MainWindowTop.DefaultValue;

    public int MainWindowWidth { get; init; } = EasiSettingKeys.MainWindowWidth.DefaultValue;

    public int MainWindowHeight { get; init; } = EasiSettingKeys.MainWindowHeight.DefaultValue;

    public bool MainWindowMaximized { get; init; } = EasiSettingKeys.MainWindowMaximized.DefaultValue;

    public bool MainInspectorExpanded { get; init; } = EasiSettingKeys.MainInspectorExpanded.DefaultValue;

    public int MainBrowserSplitPercent { get; init; } = EasiSettingKeys.MainBrowserSplitPercent.DefaultValue;
}

public sealed record AppearanceSettings
{
    public ColorTheme Theme { get; init; } = EasiSettingKeys.Theme.DefaultValue;

    public InterfaceSize InterfaceSize { get; init; } = EasiSettingKeys.InterfaceSize.DefaultValue;
}

public sealed record LiveOutputSettings
{
    public string DefaultOutputMonitorId { get; init; } = EasiSettingKeys.DefaultOutputMonitorId.DefaultValue;

    // 스테이지(Preview) 모니터로 마지막에 고른 모니터 Id(없으면 빈 문자열 → 기본 선호 모니터).
    public string PreviewMonitorId { get; init; } = EasiSettingKeys.PreviewMonitorId.DefaultValue;

    public bool UseSafetyConfirmations { get; init; } = EasiSettingKeys.UseSafetyConfirmations.DefaultValue;

    public bool ShowLyricsMonitorAlertBox { get; init; } = EasiSettingKeys.ShowLyricsMonitorAlertBox.DefaultValue;

    public int ReferenceAlertSource { get; init; } = EasiSettingKeys.ReferenceAlertSource.DefaultValue;

    public bool ReferenceAlertUsePick { get; init; } = EasiSettingKeys.ReferenceAlertUsePick.DefaultValue;

    public bool ReferenceAlertBlankIfPickNotFound { get; init; } = EasiSettingKeys.ReferenceAlertBlankIfPickNotFound.DefaultValue;

    public string ReferenceAlertPickName { get; init; } = EasiSettingKeys.ReferenceAlertPickName.DefaultValue;

    public string ReferenceAlertPickSubstitute { get; init; } = EasiSettingKeys.ReferenceAlertPickSubstitute.DefaultValue;

    public string ReferenceAlertPickSeparator { get; init; } = EasiSettingKeys.ReferenceAlertPickSeparator.DefaultValue;

    public int ReferenceAlertDurationSeconds { get; init; } = EasiSettingKeys.ReferenceAlertDurationSeconds.DefaultValue;

    public bool ReferenceAlertScroll { get; init; } = EasiSettingKeys.ReferenceAlertScroll.DefaultValue;

    public bool ReferenceAlertFlash { get; init; } = EasiSettingKeys.ReferenceAlertFlash.DefaultValue;

    public bool ReferenceAlertTransparent { get; init; } = EasiSettingKeys.ReferenceAlertTransparent.DefaultValue;

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

    public int LyricsMonitorHighlightColorArgb { get; init; } = EasiSettingKeys.LyricsMonitorHighlightColorArgb.DefaultValue;

    public int LyricsMonitorTextColor2Argb { get; init; } = EasiSettingKeys.LyricsMonitorTextColor2Argb.DefaultValue;

    public LyricsRegion2Alignment LyricsMonitorRegion2Alignment { get; init; } = EasiSettingKeys.LyricsMonitorRegion2Alignment.DefaultValue;

    public LyricsRegion2Emphasis LyricsMonitorRegion2Bold { get; init; } = EasiSettingKeys.LyricsMonitorRegion2Bold.DefaultValue;

    public LyricsRegion2Emphasis LyricsMonitorRegion2Italic { get; init; } = EasiSettingKeys.LyricsMonitorRegion2Italic.DefaultValue;

    public LyricsRegion2Emphasis LyricsMonitorRegion2Underline { get; init; } = EasiSettingKeys.LyricsMonitorRegion2Underline.DefaultValue;

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

    public int LyricsMonitorFontSize2 { get; init; } = EasiSettingKeys.LyricsMonitorFontSize2.DefaultValue;

    public string LyricsMonitorFontFamily { get; init; } = EasiSettingKeys.LyricsMonitorFontFamily.DefaultValue;

    public string LyricsMonitorFontFamily2 { get; init; } = EasiSettingKeys.LyricsMonitorFontFamily2.DefaultValue;

    public bool LyricsMonitorBold { get; init; } = EasiSettingKeys.LyricsMonitorBold.DefaultValue;

    public bool LyricsMonitorItalic { get; init; } = EasiSettingKeys.LyricsMonitorItalic.DefaultValue;

    public bool LyricsMonitorShadow { get; init; } = EasiSettingKeys.LyricsMonitorShadow.DefaultValue;

    public bool LyricsMonitorUnderline { get; init; } = EasiSettingKeys.LyricsMonitorUnderline.DefaultValue;

    public bool LyricsMonitorEmphasisChorusOnly { get; init; } = EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.DefaultValue;

    public bool LyricsMonitorInterlace { get; init; } = EasiSettingKeys.LyricsMonitorInterlace.DefaultValue;

    public bool LyricsMonitorShowDisplayPanel { get; init; } = EasiSettingKeys.LyricsMonitorShowDisplayPanel.DefaultValue;

    public bool LyricsMonitorPanelTransparent { get; init; } = EasiSettingKeys.LyricsMonitorPanelTransparent.DefaultValue;

    public int LyricsMonitorPanelColorArgb { get; init; } = EasiSettingKeys.LyricsMonitorPanelColorArgb.DefaultValue;

    public int LyricsMonitorPanelFontScalePercent { get; init; } = EasiSettingKeys.LyricsMonitorPanelFontScalePercent.DefaultValue;

    public bool LyricsMonitorPanelTextColorFollowRegion1 { get; init; } =
        EasiSettingKeys.LyricsMonitorPanelTextColorFollowRegion1.DefaultValue;

    public int LyricsMonitorPanelTextColorArgb { get; init; } = EasiSettingKeys.LyricsMonitorPanelTextColorArgb.DefaultValue;

    public bool LyricsMonitorPanelBold { get; init; } = EasiSettingKeys.LyricsMonitorPanelBold.DefaultValue;

    public bool LyricsMonitorPanelItalic { get; init; } = EasiSettingKeys.LyricsMonitorPanelItalic.DefaultValue;

    public bool LyricsMonitorPanelUnderline { get; init; } = EasiSettingKeys.LyricsMonitorPanelUnderline.DefaultValue;

    public int LyricsMonitorLineSpacingPercent { get; init; } = EasiSettingKeys.LyricsMonitorLineSpacingPercent.DefaultValue;

    public int LyricsMonitorBodyLeftMargin { get; init; } = EasiSettingKeys.LyricsMonitorBodyLeftMargin.DefaultValue;

    public int LyricsMonitorBodyRightMargin { get; init; } = EasiSettingKeys.LyricsMonitorBodyRightMargin.DefaultValue;

    public int LyricsMonitorBodyBottomMargin { get; init; } = EasiSettingKeys.LyricsMonitorBodyBottomMargin.DefaultValue;

    public int LyricsMonitorRegionGapPx { get; init; } = EasiSettingKeys.LyricsMonitorRegionGapPx.DefaultValue;

    public int LyricsMonitorBodyVerticalOffset { get; init; } = EasiSettingKeys.LyricsMonitorBodyVerticalOffset.DefaultValue;

    public bool LyricsMonitorShowPositionIndicator { get; init; } = EasiSettingKeys.LyricsMonitorShowPositionIndicator.DefaultValue;

    public bool LyricsMonitorShowVerseHeading { get; init; } = EasiSettingKeys.LyricsMonitorShowVerseHeading.DefaultValue;

    public bool LyricsMonitorShowItemNumber { get; init; } = EasiSettingKeys.LyricsMonitorShowItemNumber.DefaultValue;

    public bool LyricsMonitorShowTitleOnPanel { get; init; } = EasiSettingKeys.LyricsMonitorShowTitleOnPanel.DefaultValue;

    public bool LyricsMonitorShowCopyright { get; init; } = EasiSettingKeys.LyricsMonitorShowCopyright.DefaultValue;

    public bool LyricsMonitorShowNextItem { get; init; } = EasiSettingKeys.LyricsMonitorShowNextItem.DefaultValue;

    public bool LyricsMonitorUseFadeTransition { get; init; } = EasiSettingKeys.LyricsMonitorUseFadeTransition.DefaultValue;

    public int LyricsMonitorTransitionDurationMs { get; init; } = EasiSettingKeys.LyricsMonitorTransitionDurationMs.DefaultValue;

    public LyricsTransitionKind LyricsMonitorTransitionKind { get; init; } = EasiSettingKeys.LyricsMonitorTransitionKind.DefaultValue;

    public string LyricsMonitorItemTransitionName { get; init; } = EasiSettingKeys.LyricsMonitorItemTransitionName.DefaultValue;

    public LyricsTransitionKind LyricsMonitorSlideTransitionKind { get; init; } = EasiSettingKeys.LyricsMonitorSlideTransitionKind.DefaultValue;

    public string LyricsMonitorSlideTransitionName { get; init; } = EasiSettingKeys.LyricsMonitorSlideTransitionName.DefaultValue;

    public string LyricsMonitorBackgroundImagePath { get; init; } = EasiSettingKeys.LyricsMonitorBackgroundImagePath.DefaultValue;

    public LyricsBackgroundMode LyricsMonitorBackgroundMode { get; init; } = EasiSettingKeys.LyricsMonitorBackgroundMode.DefaultValue;

    public LyricsGradientDirection LyricsMonitorBackgroundGradientDirection { get; init; } = EasiSettingKeys.LyricsMonitorBackgroundGradientDirection.DefaultValue;

    public LyricsRegionDisplay LyricsMonitorRegionDisplay { get; init; } = EasiSettingKeys.LyricsMonitorRegionDisplay.DefaultValue;

    public bool LyricsMonitorShowTitleHeading { get; init; } = EasiSettingKeys.LyricsMonitorShowTitleHeading.DefaultValue;

    public bool LyricsMonitorOutline { get; init; } = EasiSettingKeys.LyricsMonitorOutline.DefaultValue;

    public LyricsTextAlignment LyricsMonitorTitleHeadingAlignment { get; init; } =
        EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.DefaultValue;

    public bool LyricsMonitorTitleHeadingFirstScreenOnly { get; init; } =
        EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.DefaultValue;

    public bool LyricsMonitorTitleHeadingFollowBody { get; init; } =
        EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody.DefaultValue;

    public bool LyricsMonitorTitleHeadingFollowRegion2 { get; init; } =
        EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2.DefaultValue;

    public int AutoRotateIntervalSeconds { get; init; } = EasiSettingKeys.AutoRotateIntervalSeconds.DefaultValue;

    public AutoRotateMode AutoRotateMode { get; init; } = EasiSettingKeys.AutoRotateMode.DefaultValue;
}

public sealed record PowerPointSettings
{
    public bool UsePowerPointTab { get; init; } = EasiSettingKeys.UsePowerPointTab.DefaultValue;

    public bool NoPanelOverlay { get; init; } = EasiSettingKeys.NoPowerPointPanelOverlay.DefaultValue;

    public int RenderTimeoutSeconds { get; init; } = EasiSettingKeys.PowerPointRenderTimeoutSeconds.DefaultValue;

    public int ThumbnailCacheMegabytes { get; init; } = EasiSettingKeys.ThumbnailCacheMegabytes.DefaultValue;

    public int MaxFiles { get; init; } = EasiSettingKeys.PowerPointMaxFiles.DefaultValue;

    public int SourceListingStyle { get; init; } = EasiSettingKeys.PowerPointSourceListingStyle.DefaultValue;
}

public sealed record MediaSettings
{
    public bool UseMediaTab { get; init; } = EasiSettingKeys.UseMediaTab.DefaultValue;

    public bool NoPanelOverlay { get; init; } = EasiSettingKeys.NoMediaPanelOverlay.DefaultValue;

    public string Directory { get; init; } = EasiSettingKeys.MediaDirectory.DefaultValue;

    public string DefaultMediaPath { get; init; } = EasiSettingKeys.DefaultMediaPath.DefaultValue;

    public double Volume { get; init; } = EasiSettingKeys.MediaVolume.DefaultValue;

    public double Balance { get; init; } = EasiSettingKeys.MediaBalance.DefaultValue;

    public bool Muted { get; init; } = EasiSettingKeys.MediaMuted.DefaultValue;

    public int LiveCameraNumber { get; init; } = EasiSettingKeys.LiveCameraNumber.DefaultValue;
}

public sealed record DataSettings
{
    public int PraiseBookCjkGroupStyle { get; init; } = EasiSettingKeys.PraiseBookCjkGroupStyle.DefaultValue;

    public string CurrentPraiseBookName { get; init; } = EasiSettingKeys.CurrentPraiseBookName.DefaultValue;

    public int SelectedSongFolderNo { get; init; } = EasiSettingKeys.SelectedSongFolderNo.DefaultValue;

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
        var initial = Validate(loaded).Succeeded ? loaded : EasiSettingsSnapshot.CreateDefault();
        Current = ApplyLegacyWorkingFolderFallback(initial, options.LegacyWorkingFolderPath);
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

        // 스테이지(Preview) 모니터 Id 도 출력과 같게 제어문자 검사(손상·편집된 settings.json Import 방어).
        if (!string.IsNullOrWhiteSpace(candidate.LiveOutput.PreviewMonitorId))
        {
            RequireNoControlCharacters(
                candidate.LiveOutput.PreviewMonitorId,
                EasiSettingKeys.PreviewMonitorId.Id,
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

        // 배경 표시 모드도 잘못된 값(예: 정수 99)을 거른다(정렬 enum 과 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorBackgroundMode))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorBackgroundMode.Id, "Background image mode value is not supported."));
        }

        // 그라데이션 방향도 잘못된 값을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorBackgroundGradientDirection))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorBackgroundGradientDirection.Id, "Background gradient direction value is not supported."));
        }

        // 영역 표시 모드도 잘못된 값을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorRegionDisplay))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorRegionDisplay.Id, "Region display mode value is not supported."));
        }

        // 자동 회전 모드도 잘못된 값(예: 정수 99)을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.AutoRotateMode))
        {
            issues.Add(Error(EasiSettingKeys.AutoRotateMode.Id, "Auto-rotate mode value is not supported."));
        }

        // 보조 영역(Region2) 전역 정렬도 잘못된 값을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorRegion2Alignment))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorRegion2Alignment.Id, "Region2 alignment value is not supported."));
        }

        // 보조 영역(Region2) 전역 굵게(3-상태)도 잘못된 값을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorRegion2Bold))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorRegion2Bold.Id, "Region2 bold value is not supported."));
        }

        // 보조 영역(Region2) 전역 기울임(3-상태)도 잘못된 값을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorRegion2Italic))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorRegion2Italic.Id, "Region2 italic value is not supported."));
        }

        // 보조 영역(Region2) 전역 밑줄(3-상태)도 잘못된 값을 거른다(enum 일관).
        if (!Enum.IsDefined(candidate.LiveOutput.LyricsMonitorRegion2Underline))
        {
            issues.Add(Error(EasiSettingKeys.LyricsMonitorRegion2Underline.Id, "Region2 underline value is not supported."));
        }

        // 폰트 크기 범위 가드(24~120px) — 0/음수/과대값이 들어와 출력이 깨지지 않도록(다른 수치 설정과 일관).
        RequireRange(
            candidate.LiveOutput.LyricsMonitorFontSize,
            min: 24,
            max: 120,
            EasiSettingKeys.LyricsMonitorFontSize.Id,
            issues);

        // 이중 언어 영역 간 세로 간격 가드(0~100px) — 음수나 과대값으로 두 영역이 겹치거나 화면을 벗어나지 않도록.
        RequireRange(
            candidate.LiveOutput.LyricsMonitorRegionGapPx,
            min: 0,
            max: 100,
            EasiSettingKeys.LyricsMonitorRegionGapPx.Id,
            issues);

        // 본문 세로 오프셋 가드(-300~300px) — 과대 이동으로 본문이 화면 밖으로 완전히 사라지지 않도록.
        RequireRange(
            candidate.LiveOutput.LyricsMonitorBodyVerticalOffset,
            min: -300,
            max: 300,
            EasiSettingKeys.LyricsMonitorBodyVerticalOffset.Id,
            issues);

        // Display Panel 글자 크기 비율 가드(50~200%) — 과소·과대값으로 정보 텍스트가 사라지거나 화면을 덮지 않도록.
        RequireRange(
            candidate.LiveOutput.LyricsMonitorPanelFontScalePercent,
            min: 50,
            max: 200,
            EasiSettingKeys.LyricsMonitorPanelFontScalePercent.Id,
            issues);

        // 보조 영역(Region2) 폰트 크기 가드 — 0(자동=본문 동일)은 허용, 그 외엔 24~120px 만 허용.
        if (candidate.LiveOutput.LyricsMonitorFontSize2 != 0)
        {
            RequireRange(
                candidate.LiveOutput.LyricsMonitorFontSize2,
                min: 24,
                max: 120,
                EasiSettingKeys.LyricsMonitorFontSize2.Id,
                issues);
        }

        // 줄 간격 범위 가드(100~220%) — 폰트 대비 비율. 과소·과대값으로 줄이 겹치거나 벌어지지 않도록.
        RequireRange(
            candidate.LiveOutput.LyricsMonitorLineSpacingPercent,
            min: 100,
            max: 220,
            EasiSettingKeys.LyricsMonitorLineSpacingPercent.Id,
            issues);

        // 본문 여백 범위 가드(0~400px) — 음수나 화면을 다 덮는 과대값으로 본문이 사라지지 않도록(좌/우/아래 동일).
        RequireRange(
            candidate.LiveOutput.LyricsMonitorBodyLeftMargin,
            min: 0,
            max: 400,
            EasiSettingKeys.LyricsMonitorBodyLeftMargin.Id,
            issues);
        RequireRange(
            candidate.LiveOutput.LyricsMonitorBodyRightMargin,
            min: 0,
            max: 400,
            EasiSettingKeys.LyricsMonitorBodyRightMargin.Id,
            issues);
        RequireRange(
            candidate.LiveOutput.LyricsMonitorBodyBottomMargin,
            min: 0,
            max: 400,
            EasiSettingKeys.LyricsMonitorBodyBottomMargin.Id,
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
        RequireRange(
            candidate.LiveOutput.ReferenceAlertDurationSeconds,
            min: 1,
            max: 999,
            EasiSettingKeys.ReferenceAlertDurationSeconds.Id,
            issues);
        RequireNoControlCharacters(candidate.LiveOutput.ReferenceAlertPickName, EasiSettingKeys.ReferenceAlertPickName.Id, issues);
        RequireNoControlCharacters(candidate.LiveOutput.ReferenceAlertPickSubstitute, EasiSettingKeys.ReferenceAlertPickSubstitute.Id, issues);
        RequireNoControlCharacters(candidate.LiveOutput.ReferenceAlertPickSeparator, EasiSettingKeys.ReferenceAlertPickSeparator.Id, issues);

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
        RequireRange(
            candidate.PowerPoint.SourceListingStyle,
            min: -1,
            max: 1,
            EasiSettingKeys.PowerPointSourceListingStyle.Id,
            issues);
        ValidatePath(candidate.Media.Directory, EasiSettingKeys.MediaDirectory.Id, issues, allowEmpty: true);
        ValidatePath(candidate.Media.DefaultMediaPath, EasiSettingKeys.DefaultMediaPath.Id, issues, allowEmpty: true);
        RequireRange(candidate.Media.Volume, min: 0.0, max: 1.0, EasiSettingKeys.MediaVolume.Id, issues);
        RequireRange(candidate.Media.Balance, min: -1.0, max: 1.0, EasiSettingKeys.MediaBalance.Id, issues);
        RequireRange(candidate.Media.LiveCameraNumber, min: 1, max: 5, EasiSettingKeys.LiveCameraNumber.Id, issues);
        RequireRange(
            candidate.Data.PraiseBookCjkGroupStyle,
            min: 0,
            max: 1,
            EasiSettingKeys.PraiseBookCjkGroupStyle.Id,
            issues);
        RequireRange(
            candidate.Data.SelectedSongFolderNo,
            min: 0,
            max: 999999,
            EasiSettingKeys.SelectedSongFolderNo.Id,
            issues);
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
        => ApplySnapshot(CreateRuntimeDefaultSnapshot(), SettingsChangeSource.RestoreDefaults, backupPath: null);

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
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.UseSongNumbering.Id), next, issues, value => next with
        {
            General = next.General with { UseSongNumbering = value },
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
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertSource.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertSource = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertUsePick.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertUsePick = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertBlankIfPickNotFound.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertBlankIfPickNotFound = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertPickName.Id), next, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertPickName = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertPickSubstitute.Id), next, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertPickSubstitute = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertPickSeparator.Id), next, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertPickSeparator = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ReferenceAlertDurationSeconds.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertDurationSeconds = value },
        });
        next = ApplyLegacyReferenceAlertStyle(legacySettings, next, issues);
        next = ApplyLegacyBool(legacySettings, ["ReferenceAlertScroll"], next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertScroll = value },
        });
        next = ApplyLegacyBool(legacySettings, ["ReferenceAlertFlash"], next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertFlash = value },
        });
        next = ApplyLegacyBool(legacySettings, ["ReferenceAlertTransparent"], next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { ReferenceAlertTransparent = value },
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
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.LyricsMonitorHighlightColorArgb.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { LyricsMonitorHighlightColorArgb = value },
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
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.PowerPointSourceListingStyle.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { SourceListingStyle = value },
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
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DefaultMediaPath.Id), next, value => next with
        {
            Media = next.Media with { DefaultMediaPath = value },
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
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.PraiseBookCjkGroupStyle.Id), next, issues, value => next with
        {
            Data = next.Data with { PraiseBookCjkGroupStyle = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.CurrentPraiseBookName.Id), next, value => next with
        {
            Data = next.Data with { CurrentPraiseBookName = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.SelectedSongFolderNo.Id), next, issues, value => next with
        {
            Data = next.Data with { SelectedSongFolderNo = value },
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

    private EasiSettingsSnapshot CreateRuntimeDefaultSnapshot()
        => ApplyLegacyWorkingFolderFallback(EasiSettingsSnapshot.CreateDefault(), _options.LegacyWorkingFolderPath);

    private static EasiSettingsSnapshot ApplyLegacyWorkingFolderFallback(
        EasiSettingsSnapshot snapshot,
        string? legacyWorkingFolderPath)
    {
        if (string.IsNullOrWhiteSpace(legacyWorkingFolderPath)
            || !Directory.Exists(legacyWorkingFolderPath)
            || !PathsEqual(snapshot.General.WorkingFolder, EasiSettingKeys.WorkingFolder.DefaultValue))
        {
            return snapshot;
        }

        return snapshot with
        {
            General = snapshot.General with
            {
                WorkingFolder = NormalizePathForSettings(legacyWorkingFolderPath),
            },
        };
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

    private static bool PathsEqual(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        try
        {
            return string.Equals(
                Path.GetFullPath(left.Trim()),
                Path.GetFullPath(right.Trim()),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string NormalizePathForSettings(string path)
    {
        try
        {
            return Path.GetFullPath(path.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return path.Trim();
        }
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
                SettingKey<LyricsBackgroundMode> backgroundModeKey => backgroundModeKey.Id,
                SettingKey<LyricsGradientDirection> gradientDirectionKey => gradientDirectionKey.Id,
                SettingKey<LyricsRegionDisplay> regionDisplayKey => regionDisplayKey.Id,
                SettingKey<LyricsRegion2Alignment> region2AlignmentKey => region2AlignmentKey.Id,
                SettingKey<LyricsRegion2Emphasis> region2EmphasisKey => region2EmphasisKey.Id,
                SettingKey<AutoRotateMode> autoRotateModeKey => autoRotateModeKey.Id,
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
            "general.useSongNumbering" => snapshot.General.UseSongNumbering,
            "general.registrationUser" => snapshot.General.RegistrationUser,
            "general.mainWindowLeft" => snapshot.General.MainWindowLeft,
            "general.mainWindowTop" => snapshot.General.MainWindowTop,
            "general.mainWindowWidth" => snapshot.General.MainWindowWidth,
            "general.mainWindowHeight" => snapshot.General.MainWindowHeight,
            "general.mainWindowMaximized" => snapshot.General.MainWindowMaximized,
            "general.mainInspectorExpanded" => snapshot.General.MainInspectorExpanded,
            "general.mainBrowserSplitPercent" => snapshot.General.MainBrowserSplitPercent,
            "appearance.theme" => snapshot.Appearance.Theme,
            "appearance.interfaceSize" => snapshot.Appearance.InterfaceSize,
            "liveOutput.defaultOutputMonitorId" => snapshot.LiveOutput.DefaultOutputMonitorId,
            "liveOutput.previewMonitorId" => snapshot.LiveOutput.PreviewMonitorId,
            "liveOutput.useSafetyConfirmations" => snapshot.LiveOutput.UseSafetyConfirmations,
            "liveOutput.showLyricsMonitorAlertBox" => snapshot.LiveOutput.ShowLyricsMonitorAlertBox,
            "liveOutput.referenceAlertSource" => snapshot.LiveOutput.ReferenceAlertSource,
            "liveOutput.referenceAlertUsePick" => snapshot.LiveOutput.ReferenceAlertUsePick,
            "liveOutput.referenceAlertBlankIfPickNotFound" => snapshot.LiveOutput.ReferenceAlertBlankIfPickNotFound,
            "liveOutput.referenceAlertPickName" => snapshot.LiveOutput.ReferenceAlertPickName,
            "liveOutput.referenceAlertPickSubstitute" => snapshot.LiveOutput.ReferenceAlertPickSubstitute,
            "liveOutput.referenceAlertPickSeparator" => snapshot.LiveOutput.ReferenceAlertPickSeparator,
            "liveOutput.referenceAlertDurationSeconds" => snapshot.LiveOutput.ReferenceAlertDurationSeconds,
            "liveOutput.referenceAlertScroll" => snapshot.LiveOutput.ReferenceAlertScroll,
            "liveOutput.referenceAlertFlash" => snapshot.LiveOutput.ReferenceAlertFlash,
            "liveOutput.referenceAlertTransparent" => snapshot.LiveOutput.ReferenceAlertTransparent,
            "liveOutput.advanceNextItem" => snapshot.LiveOutput.AdvanceNextItem,
            "liveOutput.gapItemOption" => snapshot.LiveOutput.GapItemOption,
            "liveOutput.gapItemLogoFile" => snapshot.LiveOutput.GapItemLogoFile,
            "liveOutput.gapItemUseFade" => snapshot.LiveOutput.GapItemUseFade,
            "liveOutput.displayAlwaysUseSecondaryMonitor" => snapshot.LiveOutput.DisplayAlwaysUseSecondaryMonitor,
            "liveOutput.displayCustomTop" => snapshot.LiveOutput.DisplayCustomTop,
            "liveOutput.displayCustomLeft" => snapshot.LiveOutput.DisplayCustomLeft,
            "liveOutput.displayCustomWidth" => snapshot.LiveOutput.DisplayCustomWidth,
            "liveOutput.lyricsMonitorTextColorArgb" => snapshot.LiveOutput.LyricsMonitorTextColorArgb,
            "liveOutput.lyricsMonitorHighlightColorArgb" => snapshot.LiveOutput.LyricsMonitorHighlightColorArgb,
            "liveOutput.lyricsMonitorTextColor2Argb" => snapshot.LiveOutput.LyricsMonitorTextColor2Argb,
            "liveOutput.lyricsMonitorRegion2Alignment" => snapshot.LiveOutput.LyricsMonitorRegion2Alignment,
            "liveOutput.lyricsMonitorRegion2Bold" => snapshot.LiveOutput.LyricsMonitorRegion2Bold,
            "liveOutput.lyricsMonitorRegion2Italic" => snapshot.LiveOutput.LyricsMonitorRegion2Italic,
            "liveOutput.lyricsMonitorRegion2Underline" => snapshot.LiveOutput.LyricsMonitorRegion2Underline,
            "liveOutput.lyricsMonitorBackgroundColorArgb" => snapshot.LiveOutput.LyricsMonitorBackgroundColorArgb,
            "liveOutput.lyricsMonitorBackgroundColor2Argb" => snapshot.LiveOutput.LyricsMonitorBackgroundColor2Argb,
            "liveOutput.lyricsMonitorBackgroundIsGradient" => snapshot.LiveOutput.LyricsMonitorBackgroundIsGradient,
            "liveOutput.lyricsMonitorShowNotations" => snapshot.LiveOutput.LyricsMonitorShowNotations,
            "liveOutput.lyricsMonitorTextAlignment" => snapshot.LiveOutput.LyricsMonitorTextAlignment,
            "liveOutput.lyricsMonitorVerticalAlignment" => snapshot.LiveOutput.LyricsMonitorVerticalAlignment,
            "liveOutput.lyricsMonitorFontSize" => snapshot.LiveOutput.LyricsMonitorFontSize,
            "liveOutput.lyricsMonitorFontSize2" => snapshot.LiveOutput.LyricsMonitorFontSize2,
            "liveOutput.lyricsMonitorFontFamily" => snapshot.LiveOutput.LyricsMonitorFontFamily,
            "liveOutput.lyricsMonitorFontFamily2" => snapshot.LiveOutput.LyricsMonitorFontFamily2,
            "liveOutput.lyricsMonitorBold" => snapshot.LiveOutput.LyricsMonitorBold,
            "liveOutput.lyricsMonitorItalic" => snapshot.LiveOutput.LyricsMonitorItalic,
            "liveOutput.lyricsMonitorShadow" => snapshot.LiveOutput.LyricsMonitorShadow,
            "liveOutput.lyricsMonitorUnderline" => snapshot.LiveOutput.LyricsMonitorUnderline,
            "liveOutput.lyricsMonitorEmphasisChorusOnly" => snapshot.LiveOutput.LyricsMonitorEmphasisChorusOnly,
            "liveOutput.lyricsMonitorInterlace" => snapshot.LiveOutput.LyricsMonitorInterlace,
            "liveOutput.lyricsMonitorShowDisplayPanel" => snapshot.LiveOutput.LyricsMonitorShowDisplayPanel,
            "liveOutput.lyricsMonitorPanelTransparent" => snapshot.LiveOutput.LyricsMonitorPanelTransparent,
            "liveOutput.lyricsMonitorPanelColorArgb" => snapshot.LiveOutput.LyricsMonitorPanelColorArgb,
            "liveOutput.lyricsMonitorPanelFontScalePercent" => snapshot.LiveOutput.LyricsMonitorPanelFontScalePercent,
            "liveOutput.lyricsMonitorPanelTextColorFollowRegion1" => snapshot.LiveOutput.LyricsMonitorPanelTextColorFollowRegion1,
            "liveOutput.lyricsMonitorPanelTextColorArgb" => snapshot.LiveOutput.LyricsMonitorPanelTextColorArgb,
            "liveOutput.lyricsMonitorPanelBold" => snapshot.LiveOutput.LyricsMonitorPanelBold,
            "liveOutput.lyricsMonitorPanelItalic" => snapshot.LiveOutput.LyricsMonitorPanelItalic,
            "liveOutput.lyricsMonitorPanelUnderline" => snapshot.LiveOutput.LyricsMonitorPanelUnderline,
            "liveOutput.lyricsMonitorLineSpacingPercent" => snapshot.LiveOutput.LyricsMonitorLineSpacingPercent,
            "liveOutput.lyricsMonitorBodyLeftMargin" => snapshot.LiveOutput.LyricsMonitorBodyLeftMargin,
            "liveOutput.lyricsMonitorBodyRightMargin" => snapshot.LiveOutput.LyricsMonitorBodyRightMargin,
            "liveOutput.lyricsMonitorBodyBottomMargin" => snapshot.LiveOutput.LyricsMonitorBodyBottomMargin,
            "liveOutput.lyricsMonitorRegionGapPx" => snapshot.LiveOutput.LyricsMonitorRegionGapPx,
            "liveOutput.lyricsMonitorBodyVerticalOffset" => snapshot.LiveOutput.LyricsMonitorBodyVerticalOffset,
            "liveOutput.lyricsMonitorShowPositionIndicator" => snapshot.LiveOutput.LyricsMonitorShowPositionIndicator,
            "liveOutput.lyricsMonitorShowVerseHeading" => snapshot.LiveOutput.LyricsMonitorShowVerseHeading,
            "liveOutput.lyricsMonitorShowItemNumber" => snapshot.LiveOutput.LyricsMonitorShowItemNumber,
            "liveOutput.lyricsMonitorShowTitleOnPanel" => snapshot.LiveOutput.LyricsMonitorShowTitleOnPanel,
            "liveOutput.lyricsMonitorShowCopyright" => snapshot.LiveOutput.LyricsMonitorShowCopyright,
            "liveOutput.lyricsMonitorShowNextItem" => snapshot.LiveOutput.LyricsMonitorShowNextItem,
            "liveOutput.lyricsMonitorUseFadeTransition" => snapshot.LiveOutput.LyricsMonitorUseFadeTransition,
            "liveOutput.lyricsMonitorTransitionDurationMs" => snapshot.LiveOutput.LyricsMonitorTransitionDurationMs,
            "liveOutput.lyricsMonitorTransitionKind" => snapshot.LiveOutput.LyricsMonitorTransitionKind,
            "liveOutput.lyricsMonitorItemTransitionName" => snapshot.LiveOutput.LyricsMonitorItemTransitionName,
            "liveOutput.lyricsMonitorSlideTransitionKind" => snapshot.LiveOutput.LyricsMonitorSlideTransitionKind,
            "liveOutput.lyricsMonitorSlideTransitionName" => snapshot.LiveOutput.LyricsMonitorSlideTransitionName,
            "liveOutput.lyricsMonitorBackgroundImagePath" => snapshot.LiveOutput.LyricsMonitorBackgroundImagePath,
            "liveOutput.lyricsMonitorBackgroundMode" => snapshot.LiveOutput.LyricsMonitorBackgroundMode,
            "liveOutput.lyricsMonitorBackgroundGradientDirection" => snapshot.LiveOutput.LyricsMonitorBackgroundGradientDirection,
            "liveOutput.lyricsMonitorRegionDisplay" => snapshot.LiveOutput.LyricsMonitorRegionDisplay,
            "liveOutput.lyricsMonitorShowTitleHeading" => snapshot.LiveOutput.LyricsMonitorShowTitleHeading,
            "liveOutput.lyricsMonitorOutline" => snapshot.LiveOutput.LyricsMonitorOutline,
            "liveOutput.lyricsMonitorTitleHeadingAlignment" => snapshot.LiveOutput.LyricsMonitorTitleHeadingAlignment,
            "liveOutput.lyricsMonitorTitleHeadingFirstScreenOnly" => snapshot.LiveOutput.LyricsMonitorTitleHeadingFirstScreenOnly,
            "liveOutput.lyricsMonitorTitleHeadingFollowBody" => snapshot.LiveOutput.LyricsMonitorTitleHeadingFollowBody,
            "liveOutput.lyricsMonitorTitleHeadingFollowRegion2" => snapshot.LiveOutput.LyricsMonitorTitleHeadingFollowRegion2,
            "liveOutput.autoRotateIntervalSeconds" => snapshot.LiveOutput.AutoRotateIntervalSeconds,
            "liveOutput.autoRotateMode" => snapshot.LiveOutput.AutoRotateMode,
            "powerPoint.usePowerPointTab" => snapshot.PowerPoint.UsePowerPointTab,
            "powerPoint.noPanelOverlay" => snapshot.PowerPoint.NoPanelOverlay,
            "powerPoint.renderTimeoutSeconds" => snapshot.PowerPoint.RenderTimeoutSeconds,
            "powerPoint.thumbnailCacheMegabytes" => snapshot.PowerPoint.ThumbnailCacheMegabytes,
            "powerPoint.maxFiles" => snapshot.PowerPoint.MaxFiles,
            "powerPoint.sourceListingStyle" => snapshot.PowerPoint.SourceListingStyle,
            "media.useMediaTab" => snapshot.Media.UseMediaTab,
            "media.noPanelOverlay" => snapshot.Media.NoPanelOverlay,
            "media.directory" => snapshot.Media.Directory,
            "media.defaultMediaPath" => snapshot.Media.DefaultMediaPath,
            "media.volume" => snapshot.Media.Volume,
            "media.balance" => snapshot.Media.Balance,
            "media.muted" => snapshot.Media.Muted,
            "media.liveCameraNumber" => snapshot.Media.LiveCameraNumber,
            "data.praiseBookCjkGroupStyle" => snapshot.Data.PraiseBookCjkGroupStyle,
            "data.currentPraiseBookName" => snapshot.Data.CurrentPraiseBookName,
            "data.selectedSongFolderNo" => snapshot.Data.SelectedSongFolderNo,
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
            "general.useSongNumbering" => snapshot with
            {
                General = snapshot.General with { UseSongNumbering = Cast<bool>(keyId, value) },
            },
            "general.registrationUser" => snapshot with
            {
                General = snapshot.General with { RegistrationUser = Cast<string>(keyId, value) },
            },
            "general.mainWindowLeft" => snapshot with
            {
                General = snapshot.General with { MainWindowLeft = Cast<int>(keyId, value) },
            },
            "general.mainWindowTop" => snapshot with
            {
                General = snapshot.General with { MainWindowTop = Cast<int>(keyId, value) },
            },
            "general.mainWindowWidth" => snapshot with
            {
                General = snapshot.General with { MainWindowWidth = Cast<int>(keyId, value) },
            },
            "general.mainWindowHeight" => snapshot with
            {
                General = snapshot.General with { MainWindowHeight = Cast<int>(keyId, value) },
            },
            "general.mainWindowMaximized" => snapshot with
            {
                General = snapshot.General with { MainWindowMaximized = Cast<bool>(keyId, value) },
            },
            "general.mainInspectorExpanded" => snapshot with
            {
                General = snapshot.General with { MainInspectorExpanded = Cast<bool>(keyId, value) },
            },
            "general.mainBrowserSplitPercent" => snapshot with
            {
                General = snapshot.General with { MainBrowserSplitPercent = Cast<int>(keyId, value) },
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
            "liveOutput.previewMonitorId" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { PreviewMonitorId = Cast<string>(keyId, value) },
            },
            "liveOutput.useSafetyConfirmations" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { UseSafetyConfirmations = Cast<bool>(keyId, value) },
            },
            "liveOutput.showLyricsMonitorAlertBox" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ShowLyricsMonitorAlertBox = Cast<bool>(keyId, value) },
            },
            "liveOutput.referenceAlertSource" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertSource = Cast<int>(keyId, value) },
            },
            "liveOutput.referenceAlertUsePick" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertUsePick = Cast<bool>(keyId, value) },
            },
            "liveOutput.referenceAlertBlankIfPickNotFound" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertBlankIfPickNotFound = Cast<bool>(keyId, value) },
            },
            "liveOutput.referenceAlertPickName" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertPickName = Cast<string>(keyId, value) },
            },
            "liveOutput.referenceAlertPickSubstitute" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertPickSubstitute = Cast<string>(keyId, value) },
            },
            "liveOutput.referenceAlertPickSeparator" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertPickSeparator = Cast<string>(keyId, value) },
            },
            "liveOutput.referenceAlertDurationSeconds" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertDurationSeconds = Cast<int>(keyId, value) },
            },
            "liveOutput.referenceAlertScroll" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertScroll = Cast<bool>(keyId, value) },
            },
            "liveOutput.referenceAlertFlash" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertFlash = Cast<bool>(keyId, value) },
            },
            "liveOutput.referenceAlertTransparent" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { ReferenceAlertTransparent = Cast<bool>(keyId, value) },
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
            "liveOutput.lyricsMonitorHighlightColorArgb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorHighlightColorArgb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTextColor2Argb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTextColor2Argb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorRegion2Alignment" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorRegion2Alignment = Cast<LyricsRegion2Alignment>(keyId, value) },
            },
            "liveOutput.lyricsMonitorRegion2Bold" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorRegion2Bold = Cast<LyricsRegion2Emphasis>(keyId, value) },
            },
            "liveOutput.lyricsMonitorRegion2Italic" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorRegion2Italic = Cast<LyricsRegion2Emphasis>(keyId, value) },
            },
            "liveOutput.lyricsMonitorRegion2Underline" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorRegion2Underline = Cast<LyricsRegion2Emphasis>(keyId, value) },
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
            "liveOutput.lyricsMonitorFontSize2" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorFontSize2 = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorFontFamily" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorFontFamily = Cast<string>(keyId, value) },
            },
            "liveOutput.lyricsMonitorFontFamily2" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorFontFamily2 = Cast<string>(keyId, value) },
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
            "liveOutput.lyricsMonitorUnderline" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorUnderline = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorEmphasisChorusOnly" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorEmphasisChorusOnly = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorInterlace" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorInterlace = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowDisplayPanel" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowDisplayPanel = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelTransparent" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelTransparent = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelColorArgb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelColorArgb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelFontScalePercent" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelFontScalePercent = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelTextColorFollowRegion1" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelTextColorFollowRegion1 = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelTextColorArgb" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelTextColorArgb = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelBold" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelBold = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelItalic" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelItalic = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorPanelUnderline" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorPanelUnderline = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorLineSpacingPercent" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorLineSpacingPercent = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBodyLeftMargin" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBodyLeftMargin = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBodyRightMargin" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBodyRightMargin = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBodyBottomMargin" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBodyBottomMargin = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorRegionGapPx" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorRegionGapPx = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBodyVerticalOffset" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBodyVerticalOffset = Cast<int>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowPositionIndicator" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowPositionIndicator = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowVerseHeading" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowVerseHeading = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowItemNumber" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowItemNumber = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorShowTitleOnPanel" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorShowTitleOnPanel = Cast<bool>(keyId, value) },
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
            "liveOutput.lyricsMonitorItemTransitionName" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorItemTransitionName = Cast<string>(keyId, value) },
            },
            "liveOutput.lyricsMonitorSlideTransitionKind" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorSlideTransitionKind = Cast<LyricsTransitionKind>(keyId, value) },
            },
            "liveOutput.lyricsMonitorSlideTransitionName" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorSlideTransitionName = Cast<string>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundImagePath" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundImagePath = Cast<string>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundMode" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundMode = Cast<LyricsBackgroundMode>(keyId, value) },
            },
            "liveOutput.lyricsMonitorBackgroundGradientDirection" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorBackgroundGradientDirection = Cast<LyricsGradientDirection>(keyId, value) },
            },
            "liveOutput.lyricsMonitorRegionDisplay" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorRegionDisplay = Cast<LyricsRegionDisplay>(keyId, value) },
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
            "liveOutput.lyricsMonitorTitleHeadingFollowBody" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTitleHeadingFollowBody = Cast<bool>(keyId, value) },
            },
            "liveOutput.lyricsMonitorTitleHeadingFollowRegion2" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { LyricsMonitorTitleHeadingFollowRegion2 = Cast<bool>(keyId, value) },
            },
            "liveOutput.autoRotateIntervalSeconds" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { AutoRotateIntervalSeconds = Cast<int>(keyId, value) },
            },
            "liveOutput.autoRotateMode" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { AutoRotateMode = Cast<AutoRotateMode>(keyId, value) },
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
            "powerPoint.sourceListingStyle" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { SourceListingStyle = Cast<int>(keyId, value) },
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
            "media.defaultMediaPath" => snapshot with
            {
                Media = snapshot.Media with { DefaultMediaPath = Cast<string>(keyId, value) },
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
            "data.praiseBookCjkGroupStyle" => snapshot with
            {
                Data = snapshot.Data with { PraiseBookCjkGroupStyle = Cast<int>(keyId, value) },
            },
            "data.currentPraiseBookName" => snapshot with
            {
                Data = snapshot.Data with { CurrentPraiseBookName = Cast<string>(keyId, value) },
            },
            "data.selectedSongFolderNo" => snapshot with
            {
                Data = snapshot.Data with { SelectedSongFolderNo = Cast<int>(keyId, value) },
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

    private static EasiSettingsSnapshot ApplyLegacyReferenceAlertStyle(
        ILegacySettingsSource source,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues)
    {
        if (!TryGetLegacyString(source, ["ReferenceAlertStyle"], out var raw, out var matchedKey))
        {
            return current;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var style))
        {
            issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid reference-alert style bitfield."));
            return current;
        }

        return current with
        {
            LiveOutput = current.LiveOutput with
            {
                ReferenceAlertScroll = (style & 1) != 0,
                ReferenceAlertFlash = (style & 2) != 0,
                ReferenceAlertTransparent = (style & 4) != 0,
            },
        };
    }

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
            PutShortcutOverride(shortcuts, MainCommandIds.LivePreviewToOutputClearBlack, isGlobal: true, "F7");
            changed = true;
        }
        else if (hasF8 && useF8)
        {
            PutShortcutOverride(shortcuts, MainCommandIds.LivePreviewToOutput, isGlobal: true, "F8");
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
