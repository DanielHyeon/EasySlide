using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Media;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ILiveSessionService _session;
    private readonly IOutputWindowService _output;
    private readonly ILiveSafetyPrompt _safetyPrompt;
    private readonly ICommandTelemetry _telemetry;
    private readonly IDisplayService _display;
    private readonly ICommandCatalog _commandCatalog;
    private readonly ISettingsService _settings;
    private readonly IWorshipListStore _worshipLists;
    private readonly IRecentWorshipLists _recentWorshipLists;
    // 예배 순서 검증기 — 라이브 송출 전 깨진 PPT·미디어 파일을 미리 거른다(레거시 ValidateWorshipListItems).
    private readonly WorshipListValidator _worshipValidator;
    private readonly IAppearanceTemplateStore _appearanceTemplates;
    // 좌측 "검색" 탭의 교차 검색 결과를 큐에 추가할 때, 결과(SongSearchResult)엔 가사가 없어 SongId 로 곡 상세(가사)를 불러온다.
    private readonly Data.IAdminSongDetailRepository _songDetail;
    // 명령 팔레트 실행에 쓰는 단축키 레지스트리 — BindShortcuts(앱 시작)에서 주입된다. 그 전엔 null.
    private ShortcutRegistry? _shortcutRegistry;

    [ObservableProperty] private LiveQueueItem? _selectedItem;
    [ObservableProperty] private OutputDisplay? _selectedOutputDisplay;
    [ObservableProperty] private string _statusText = "WPF 운영 준비됨";
    // 예배 순서 검증에서 문제가 하나라도 있으면 true — 좌측 패널의 경고 목록 표시 여부에 쓰인다.
    [ObservableProperty] private bool _hasWorshipListProblems;
    // 예배 순서(큐)가 비어 있으면 true — 좌측 패널의 "비어 있음" 안내 표시 여부. 시작 시 빈 큐(더미 시드 제거).
    [ObservableProperty] private bool _isQueueEmpty = true;
    // 라이브 조옮김 반음 수(레거시 Transpose ±Semi-Tone) — 코드 표시가 켜졌을 때 송출 코드를 이동. 0=원조.
    // 새 곡을 송출하면 0 으로 초기화되어 각 곡이 작성된 키에서 시작한다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LiveTransposeLabel))]
    private int _liveTransposeSemitones;

    // 좌측 "검색" 탭에서 선택한 교차 검색 결과(폴더 가로지름). "예배 순서에 추가" 활성 여부를 좌우한다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSearchedSongCommand))]
    private SongSearchResult? _selectedSearchResult;

    // 검색 탭 "제목" 모드에서 선택한 제목 조회 후보. "예배 순서에 추가"(제목) 활성 여부를 좌우한다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLookupTitleCommand))]
    private LookupTitleCandidate? _selectedTitleCandidate;

    // 중앙 미리보기 탭 인덱스(0=Preview, 1=PowerPoint, 2=Media) — 선택 항목 종류에 맞춰 자동 전환(FrmMain식 멀티페인).
    // 운영자가 항목을 고르면 알맞은 미리보기가 바로 보여 수동 탭 전환을 없앤다(§7.4 단일 콘솔).
    [ObservableProperty] private int _selectedContentTabIndex;

    // 우측 출력 모양 인스펙터 펼침 여부(FrmMain식 가변 패널, §7.4). 접으면 우측 컬럼이 0 으로 줄어 중앙 미리보기가 넓어진다.
    // 기본 펼침(true). 운영바 토글로 전환.
    [ObservableProperty] private bool _isInspectorExpanded = true;

    [ObservableProperty] private bool _isPowerPointTabVisible;
    [ObservableProperty] private bool _isPowerPointPanelOverlayEnabled = true;
    [ObservableProperty] private int _powerPointMaxFiles = EasiSettingKeys.PowerPointMaxFiles.DefaultValue;
    [ObservableProperty] private int _powerPointFileCount;
    [ObservableProperty] private bool _hasPowerPointLimitViolation;
    [ObservableProperty] private bool _isMediaTabVisible;
    [ObservableProperty] private bool _isMediaPanelOverlayEnabled = true;
    [ObservableProperty] private string _mediaDirectory = EasiSettingKeys.MediaDirectory.DefaultValue;
    [ObservableProperty] private int _liveCameraNumber = EasiSettingKeys.LiveCameraNumber.DefaultValue;
    [ObservableProperty] private string _liveCameraSource = MediaPlaybackService.CreateLiveCameraSource(EasiSettingKeys.LiveCameraNumber.DefaultValue);
    // 현재 출력 모양과 일치하는 프리셋 이름(없으면 "사용자 지정"). 인스펙터에서 활성 프리셋 강조용.
    [ObservableProperty] private string _activeAppearanceName = "";
    // 현재 적용된 가사 정렬(인-셸 인스펙터 강조용). 설정에서 유래.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveLyricsAlignmentLabel))]
    private LyricsTextAlignment _activeLyricsAlignment = EasiSettingKeys.LyricsMonitorTextAlignment.DefaultValue;
    // 현재 적용된 가사 세로 정렬.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveLyricsVerticalAlignmentLabel))]
    private LyricsVerticalAlignment _activeLyricsVerticalAlignment = EasiSettingKeys.LyricsMonitorVerticalAlignment.DefaultValue;
    // 현재 적용된 배경 이미지 표시 모드(채움/맞춤/가운데/타일) — 메뉴 체크 표시에 쓰인다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundModeIsFill))]
    [NotifyPropertyChangedFor(nameof(BackgroundModeIsFit))]
    [NotifyPropertyChangedFor(nameof(BackgroundModeIsCenter))]
    [NotifyPropertyChangedFor(nameof(BackgroundModeIsTile))]
    private LyricsBackgroundMode _activeBackgroundMode = EasiSettingKeys.LyricsMonitorBackgroundMode.DefaultValue;
    // 현재 적용된 이중 언어 영역 표시 모드(둘다/Region1만/Region2만) — 메뉴 체크 표시에 쓰인다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RegionDisplayIsBoth))]
    [NotifyPropertyChangedFor(nameof(RegionDisplayIsRegion1Only))]
    [NotifyPropertyChangedFor(nameof(RegionDisplayIsRegion2Only))]
    private LyricsRegionDisplay _activeRegionDisplay = EasiSettingKeys.LyricsMonitorRegionDisplay.DefaultValue;
    // 현재 적용된 가사 폰트 크기(px). +/- 커맨드 활성/비활성 판별에도 쓰인다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreaseLyricsFontSizeCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseLyricsFontSizeCommand))]
    private int _activeLyricsFontSize = EasiSettingKeys.LyricsMonitorFontSize.DefaultValue;
    private bool _disposed;

    // 폰트 크기 조절 범위·단계(설정 Validate 범위 24~120 과 일치).
    private const int LyricsFontSizeMin = 24;
    private const int LyricsFontSizeMax = 120;
    private const int LyricsFontSizeStep = 4;

    // 줄 간격 조절 범위·단계(설정 Validate 범위 100~220% 와 일치).
    private const int LyricsLineSpacingMin = 100;
    private const int LyricsLineSpacingMax = 220;
    private const int LyricsLineSpacingStep = 10;

    // 현재 줄 간격(%). +/- 커맨드 활성/비활성 판별에도 쓰인다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreaseLyricsLineSpacingCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseLyricsLineSpacingCommand))]
    private int _activeLyricsLineSpacing = EasiSettingKeys.LyricsMonitorLineSpacingPercent.DefaultValue;

    // 현재 폰트 효과 상태(인스펙터 ToggleButton IsChecked 바인딩용). 설정에서 유래.
    [ObservableProperty] private bool _activeLyricsBold = EasiSettingKeys.LyricsMonitorBold.DefaultValue;
    [ObservableProperty] private bool _activeLyricsItalic = EasiSettingKeys.LyricsMonitorItalic.DefaultValue;
    [ObservableProperty] private bool _activeLyricsShadow = EasiSettingKeys.LyricsMonitorShadow.DefaultValue;
    [ObservableProperty] private bool _activeLyricsUnderline = EasiSettingKeys.LyricsMonitorUnderline.DefaultValue;
    [ObservableProperty] private bool _activeLyricsEmphasisChorusOnly = EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.DefaultValue;
    // 현재 Display Panel 투명 배경 상태(메뉴 체크 바인딩용).
    [ObservableProperty] private bool _activeLyricsPanelTransparent = EasiSettingKeys.LyricsMonitorPanelTransparent.DefaultValue;
    // 현재 위치 인디케이터 표시 상태(인스펙터 ToggleButton IsChecked 바인딩용).
    [ObservableProperty] private bool _activeLyricsPositionIndicator = EasiSettingKeys.LyricsMonitorShowPositionIndicator.DefaultValue;
    // 현재 곡 번호 표시 상태(메뉴 체크 바인딩용, Display Panel).
    [ObservableProperty] private bool _activeLyricsItemNumber = EasiSettingKeys.LyricsMonitorShowItemNumber.DefaultValue;
    // 현재 저작권 표시 상태(메뉴 체크 바인딩용, Display Panel).
    [ObservableProperty] private bool _activeLyricsCopyright = EasiSettingKeys.LyricsMonitorShowCopyright.DefaultValue;
    // 현재 다음 항목 표시 상태(메뉴 체크 바인딩용, Display Panel PrevNext).
    [ObservableProperty] private bool _activeLyricsNextItem = EasiSettingKeys.LyricsMonitorShowNextItem.DefaultValue;
    // 현재 전환 페이드 사용 상태(메뉴 체크 바인딩용, FrmMain 전환 효과).
    [ObservableProperty] private bool _activeFadeTransition = EasiSettingKeys.LyricsMonitorUseFadeTransition.DefaultValue;
    // 현재 전환 길이(ms). 메뉴 프리셋(빠르게/보통/느리게) 체크 표시 계산에 쓰인다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransitionIsFast))]
    [NotifyPropertyChangedFor(nameof(TransitionIsNormal))]
    [NotifyPropertyChangedFor(nameof(TransitionIsSlow))]
    private int _activeTransitionDurationMs = EasiSettingKeys.LyricsMonitorTransitionDurationMs.DefaultValue;

    // 전환 길이 프리셋 메뉴 체크 표시(빠르게 150 / 보통 250 / 느리게 500). 현재 ms 와 일치할 때만 체크.
    public bool TransitionIsFast => ActiveTransitionDurationMs == 150;
    public bool TransitionIsNormal => ActiveTransitionDurationMs == 250;
    public bool TransitionIsSlow => ActiveTransitionDurationMs == 500;

    // 현재 전환 모션 종류(메뉴 체크 바인딩용, Fade/Slide 4방향).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsFade))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsSlideLeft))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsSlideRight))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsSlideUp))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsSlideDown))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsZoomIn))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsZoomOut))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsSpin))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsFlipH))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsFlipV))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsRevealCircle))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsRevealRectangle))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsWipeRight))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsWipeLeft))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsWipeDown))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsWipeUp))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsBlindsH))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsBlindsV))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsCheckerboard))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsDiamond))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsDoorsOpen))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsDoorsClose))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsStar))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsCross))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsBowTie))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsHeart))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsWedge))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsSpiral))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsWindMill))]
    [NotifyPropertyChangedFor(nameof(TransitionKindIsFanUp))]
    private LyricsTransitionKind _activeTransitionKind = EasiSettingKeys.LyricsMonitorTransitionKind.DefaultValue;

    public bool TransitionKindIsFade => ActiveTransitionKind == LyricsTransitionKind.Fade;
    public bool TransitionKindIsSlideLeft => ActiveTransitionKind == LyricsTransitionKind.SlideFromLeft;
    public bool TransitionKindIsSlideRight => ActiveTransitionKind == LyricsTransitionKind.SlideFromRight;
    public bool TransitionKindIsSlideUp => ActiveTransitionKind == LyricsTransitionKind.SlideFromTop;
    public bool TransitionKindIsSlideDown => ActiveTransitionKind == LyricsTransitionKind.SlideFromBottom;
    public bool TransitionKindIsZoomIn => ActiveTransitionKind == LyricsTransitionKind.ZoomIn;
    public bool TransitionKindIsZoomOut => ActiveTransitionKind == LyricsTransitionKind.ZoomOut;
    public bool TransitionKindIsSpin => ActiveTransitionKind == LyricsTransitionKind.Spin;
    public bool TransitionKindIsFlipH => ActiveTransitionKind == LyricsTransitionKind.FlipHorizontal;
    public bool TransitionKindIsFlipV => ActiveTransitionKind == LyricsTransitionKind.FlipVertical;
    public bool TransitionKindIsRevealCircle => ActiveTransitionKind == LyricsTransitionKind.RevealCircle;
    public bool TransitionKindIsRevealRectangle => ActiveTransitionKind == LyricsTransitionKind.RevealRectangle;
    public bool TransitionKindIsWipeRight => ActiveTransitionKind == LyricsTransitionKind.WipeRight;
    public bool TransitionKindIsWipeLeft => ActiveTransitionKind == LyricsTransitionKind.WipeLeft;
    public bool TransitionKindIsWipeDown => ActiveTransitionKind == LyricsTransitionKind.WipeDown;
    public bool TransitionKindIsWipeUp => ActiveTransitionKind == LyricsTransitionKind.WipeUp;
    public bool TransitionKindIsBlindsH => ActiveTransitionKind == LyricsTransitionKind.BlindsHorizontal;
    public bool TransitionKindIsBlindsV => ActiveTransitionKind == LyricsTransitionKind.BlindsVertical;
    public bool TransitionKindIsCheckerboard => ActiveTransitionKind == LyricsTransitionKind.Checkerboard;
    public bool TransitionKindIsDiamond => ActiveTransitionKind == LyricsTransitionKind.Diamond;
    public bool TransitionKindIsDoorsOpen => ActiveTransitionKind == LyricsTransitionKind.DoorsOpen;
    public bool TransitionKindIsDoorsClose => ActiveTransitionKind == LyricsTransitionKind.DoorsClose;
    public bool TransitionKindIsStar => ActiveTransitionKind == LyricsTransitionKind.Star;
    public bool TransitionKindIsCross => ActiveTransitionKind == LyricsTransitionKind.Cross;
    public bool TransitionKindIsBowTie => ActiveTransitionKind == LyricsTransitionKind.BowTie;
    public bool TransitionKindIsHeart => ActiveTransitionKind == LyricsTransitionKind.Heart;
    public bool TransitionKindIsWedge => ActiveTransitionKind == LyricsTransitionKind.Wedge;
    public bool TransitionKindIsSpiral => ActiveTransitionKind == LyricsTransitionKind.Spiral;
    public bool TransitionKindIsWindMill => ActiveTransitionKind == LyricsTransitionKind.WindMill;
    public bool TransitionKindIsFanUp => ActiveTransitionKind == LyricsTransitionKind.FanUp;
    // 현재 제목 헤딩 표시 상태(인스펙터 ToggleButton IsChecked 바인딩용, §7.3-A).
    [ObservableProperty] private bool _activeLyricsTitleHeading = EasiSettingKeys.LyricsMonitorShowTitleHeading.DefaultValue;
    // 현재 외곽선 효과 상태(인스펙터 ToggleButton IsChecked 바인딩용, §7.3-A 폰트 효과).
    [ObservableProperty] private bool _activeLyricsOutline = EasiSettingKeys.LyricsMonitorOutline.DefaultValue;
    // 현재 제목 헤딩 가로 정렬 상태(인스펙터 정렬 버튼 강조용, §7.3-A Heading Align).
    [ObservableProperty] private LyricsTextAlignment _activeTitleHeadingAlignment = EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.DefaultValue;
    // 현재 "제목 헤딩 첫 화면만" 상태(인스펙터 ToggleButton IsChecked 바인딩용, §7.3-A).
    [ObservableProperty] private bool _activeTitleHeadingFirstScreenOnly = EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.DefaultValue;
    // 자동 회전 활성 상태(View 가 이 값을 보고 DispatcherTimer 시작/정지). 라이브 종료 시 자동 해제.
    [ObservableProperty] private bool _isAutoRotating;
    // 자동 회전 간격(초) — 설정에서 유래, View 타이머가 참조.
    [ObservableProperty] private int _autoRotateIntervalSeconds = EasiSettingKeys.AutoRotateIntervalSeconds.DefaultValue;

    // 현재 글자색/배경색의 hex 표기("#RRGGBB"). 인스펙터 hex 입력칸 표시·프리셋 너머 세분 색 지정용.
    [ObservableProperty] private string _activeTextColorHex = "#000000";
    [ObservableProperty] private string _activeBackgroundColorHex = "#FFFFFF";

    // 출력 모양 템플릿(저장/불러오기) — 새 템플릿 이름 입력, 선택된 기존 템플릿.
    [ObservableProperty] private string _newAppearanceTemplateName = "";
    [ObservableProperty] private string? _selectedAppearanceTemplate;

    /// <summary>저장된 출력 모양 템플릿 이름 목록(우측 인스펙터 콤보 바인딩, §7.3-A).</summary>
    public ObservableCollection<string> AppearanceTemplateNames { get; } = new();

    /// <summary>현재 가사 가로 정렬의 한글 라벨(인스펙터 "현재 정렬" 표시용).</summary>
    public string ActiveLyricsAlignmentLabel
        => ActiveLyricsAlignment switch
        {
            LyricsTextAlignment.Left => "왼쪽",
            LyricsTextAlignment.Right => "오른쪽",
            _ => "가운데",
        };

    /// <summary>현재 가사 세로 정렬의 한글 라벨.</summary>
    public string ActiveLyricsVerticalAlignmentLabel
        => ActiveLyricsVerticalAlignment switch
        {
            LyricsVerticalAlignment.Top => "위",
            LyricsVerticalAlignment.Bottom => "아래",
            _ => "가운데",
        };

    /// <summary>
    /// 인-셸 "출력 모양" 인스펙터 프리셋(글자색 + 배경) — 별도 Settings 모달 없이 MainWindow 에서 즉시 적용,
    /// 설정→출력 VM(SettingsChanged) 경로로 라이브 반영(§7.5 P0 — 단일 콘솔 통합 첫걸음). 색은 ARGB 정수.
    /// </summary>
    public IReadOnlyList<OutputAppearancePreset> OutputAppearancePresets { get; } = new[]
    {
        new OutputAppearancePreset("흰 글자 · 검정", unchecked((int)0xFFFFFFFF), unchecked((int)0xFF000000), unchecked((int)0xFF000000), IsGradient: false),
        new OutputAppearancePreset("흰 글자 · 네이비 그라데이션", unchecked((int)0xFFFFFFFF), unchecked((int)0xFF1B2A4A), unchecked((int)0xFF0A1020), IsGradient: true),
        new OutputAppearancePreset("검정 글자 · 흰 배경", unchecked((int)0xFF000000), unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), IsGradient: false),
        new OutputAppearancePreset("노랑 글자 · 진남색", unchecked((int)0xFFFFD24A), unchecked((int)0xFF101830), unchecked((int)0xFF101830), IsGradient: false),
    };

    /// <summary>
    /// 인-셸 가사 정렬 옵션(좌/중/우) — MainWindow 우측 인스펙터가 바인딩. 별도 Settings 모달 없이 즉시 적용,
    /// 설정→출력 VM(SettingsChanged) 경로로 라이브 반영(§7.3-A / §7.5 P0-a).
    /// </summary>
    public IReadOnlyList<LyricsTextAlignment> LyricsAlignmentOptions { get; } = new[]
    {
        LyricsTextAlignment.Left,
        LyricsTextAlignment.Center,
        LyricsTextAlignment.Right,
    };

    /// <summary>인-셸 가사 세로 정렬 옵션(위/가운데/아래) — 우측 인스펙터 바인딩(§7.3-A).</summary>
    public IReadOnlyList<LyricsVerticalAlignment> LyricsVerticalAlignmentOptions { get; } = new[]
    {
        LyricsVerticalAlignment.Top,
        LyricsVerticalAlignment.Center,
        LyricsVerticalAlignment.Bottom,
    };

    // 현재 라이브 송출 중인 큐 항목의 Id(없으면 null). 슬라이드 이동이 "선택 항목 == 라이브 항목"일 때만
    // 출력을 갱신하도록 판별하는 데 쓴다.
    private string? _liveItemId;

    // 절 단위 페이지네이션 상태 — PPT 의 SlideNumber 와 대칭.
    // LyricsPageIndex: 현재 보여주는 절 인덱스(0-based). LyricsPageCount: 선택 곡의 총 절 수.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LyricsPageLabel))]
    [NotifyCanExecuteChangedFor(nameof(NextLyricsPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousLyricsPageCommand))]
    private int _lyricsPageIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LyricsPageLabel))]
    [NotifyCanExecuteChangedFor(nameof(NextLyricsPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousLyricsPageCommand))]
    private int _lyricsPageCount;

    /// <summary>
    /// 가사 절 페이지 표시 문자열(예: "2/3절"). 단일 절이거나 곡이 아니면 빈 문자열.
    /// MainWindow Preview 탭에서 절 이동 버튼 옆에 표시한다.
    /// </summary>
    public string LyricsPageLabel
        => LyricsPageCount <= 1
            ? string.Empty
            : $"{LyricsPageIndex + 1}/{LyricsPageCount}절";

    // 현재 곡의 페이지별 절 라벨(ToVersePages 와 1:1 정렬) — 절 라벨 직접 점프(JumpToLyricsSection)의 인덱스 근거.
    private IReadOnlyList<string> _pageLabels = Array.Empty<string>();

    /// <summary>
    /// 현재 선택 곡의 절 라벨 목록(중복 제거, 첫 등장 순서). Preview 탭의 절 라벨 점프 버튼이 바인딩한다.
    /// 곡이 아니거나 라벨이 없으면 비어 있음(레거시 FrmInfoScreen 절 버튼 1~9·c·b 즉시 이동 대응).
    /// </summary>
    public ObservableCollection<string> AvailableSectionLabels { get; } = new();

    /// <summary>
    /// 미디어 재생 컨트롤 VM(상태·위치·볼륨·재생/정지/탐색). MainWindow Media 탭이 바인딩한다.
    /// (G1.2 / gap-analysis.md §4 G-α — 기존 placeholder 텍스트 대체, 테스트된 VM 의 UI 연결.)
    /// </summary>
    public MediaPlaybackViewModel Media { get; }

    /// <summary>
    /// PPT 미리보기 VM(슬라이드 렌더 이미지·상태). MainWindow PowerPoint 탭이 바인딩한다.
    /// (G1 / gap-analysis.md §4 G-α — placeholder "Decks: N" 대체, orphaned 렌더 서비스의 UI 연결.)
    /// </summary>
    public Rendering.PowerPointPreviewViewModel PowerPoint { get; }

    /// <summary>
    /// 인라인 콘텐츠 브라우저 VM(폴더·곡·검색). MainWindow 좌측 "라이브러리" 탭이 바인딩한다
    /// — 별도 LibraryWindow 를 열지 않고 셸에서 곡을 찾아 예배 순서에 추가(§7.5 P0 단일 콘솔 통합).
    /// </summary>
    public LibraryViewModel Library { get; }

    /// <summary>
    /// 인라인 성경 브라우저 VM(버전·책·검색·구절). MainWindow 좌측 "성경" 탭이 바인딩한다
    /// — 별도 BibleWindow 없이 셸에서 구절을 찾아 예배 순서에 추가(§7.5 P0 단일 콘솔 통합).
    /// </summary>
    public BibleViewModel Bible { get; }

    /// <summary>
    /// 인라인 곡 검색 VM(폴더 가로지르는 다중 필드 검색). MainWindow 좌측 "검색" 탭이 바인딩한다
    /// — 별도 SearchUsageWindow 를 열지 않고 셸에서 곡을 찾아 예배 순서에 추가(§7.4 단일 콘솔 통합).
    /// (Titles/Usage 같은 관리용 기능은 기존 SearchUsageWindow 에 남겨 두고 팔레트로 연다.)
    /// </summary>
    public SearchUsageViewModel Search { get; }

    /// <summary>
    /// 명령 팔레트 VM(⌘K, §7.4) — 명령 카탈로그를 검색해 단일 진입점으로 실행. MainWindow 오버레이가 바인딩한다.
    /// </summary>
    public CommandPaletteViewModel CommandPalette { get; }

    public MainViewModel(
        ILiveSessionService session,
        IOutputWindowService output,
        ILiveSafetyPrompt safetyPrompt,
        ICommandTelemetry telemetry,
        IDisplayService display,
        ICommandCatalog commandCatalog,
        ISettingsService settings,
        MediaPlaybackViewModel media,
        Rendering.PowerPointPreviewViewModel powerPoint,
        LibraryViewModel library,
        BibleViewModel bible,
        SearchUsageViewModel search,
        IWorshipListStore worshipLists,
        IAppearanceTemplateStore appearanceTemplates,
        Data.IAdminSongDetailRepository songDetail,
        IRecentWorshipLists recentWorshipLists,
        WorshipListValidator? worshipValidator = null)
    {
        _session = session;
        _output = output;
        _safetyPrompt = safetyPrompt;
        _telemetry = telemetry;
        _display = display;
        _commandCatalog = commandCatalog;
        _settings = settings;
        _worshipLists = worshipLists;
        _appearanceTemplates = appearanceTemplates;
        _songDetail = songDetail;
        _recentWorshipLists = recentWorshipLists;
        // 예배 순서 검증기 — 기본은 실제 파일 존재(File.Exists). 테스트는 가짜 판정을 주입해 디스크 없이 검증.
        _worshipValidator = worshipValidator ?? new WorshipListValidator();
        Media = media;
        Library = library;
        Bible = bible;
        Search = search;
        PowerPoint = powerPoint;
        // 명령 팔레트(⌘K, §7.4) — 카탈로그를 검색해 ShortcutRegistry 바인딩으로 실행.
        // registry 는 BindShortcuts 에서 주입되므로(앱 시작 시) invoke 는 그 시점 이후 유효하다.
        CommandPalette = new CommandPaletteViewModel(_commandCatalog, id => _shortcutRegistry?.TryInvoke(id) ?? false);

        _session.SessionChanged += (_, e) => ApplyLiveSnapshot(e.Snapshot);
        _output.OutputChanged += OnOutputChanged;
        _settings.SettingsChanged += OnSettingsChanged;
        // 큐가 바뀔 때마다 "비어 있음" 상태를 갱신한다(추가·제거·로드 등 모든 변경 경로를 한 곳에서 반영).
        Queue.CollectionChanged += (_, _) => IsQueueEmpty = Queue.Count == 0;
        // PPT 렌더 상태/슬라이드 변화에 슬라이드 이동 커맨드 활성 상태를 맞춘다.
        PowerPoint.PropertyChanged += OnPowerPointPropertyChanged;

        OpenOutputCommand = new RelayCommand(OpenOutput);
        CloseOutputCommand = new AsyncRelayCommand(CloseOutputAsync, () => _output.Current.IsOpen);
        GoLiveCommand = new AsyncRelayCommand(GoLiveAsync, CanGoLive);
        StopLiveCommand = new AsyncRelayCommand(StopLiveAsync, () => _session.Current.State != LiveState.Off);
        NextItemCommand = new RelayCommand(NextItem, CanMoveNext);
        PreviousItemCommand = new RelayCommand(PreviousItem, CanMovePrevious);
        FirstItemCommand = new RelayCommand(FirstItem, CanMovePrevious);
        LastItemCommand = new RelayCommand(LastItem, CanMoveNext);
        HideOutputCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: false), CanUseLiveSafetyAction);
        BlackScreenCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: true), CanUseLiveSafetyAction);
        ClearOutputCommand = new AsyncRelayCommand(ClearOutputAsync, CanUseLiveSafetyAction);
        RestoreOutputCommand = new RelayCommand(RestoreOutput, () => _session.Current.State == LiveState.Hidden);
        RestartCurrentItemCommand = new AsyncRelayCommand(RestartCurrentItemAsync, CanRestartCurrentItem);
        RefreshOutputCommand = new RelayCommand(RefreshOutput, () => _output.Current.IsOpen);
        NextSlideCommand = new AsyncRelayCommand(() => GoToSlideAsync(PowerPoint.SlideNumber + 1), CanGoNextSlide);
        PreviousSlideCommand = new AsyncRelayCommand(() => GoToSlideAsync(PowerPoint.SlideNumber - 1), CanGoPreviousSlide);
        GoToSlideCommand = new AsyncRelayCommand<int>(GoToSlideAsync, CanGoToSlide);
        ApplyOutputAppearanceCommand = new RelayCommand<OutputAppearancePreset>(ApplyOutputAppearance);
        ApplyLyricsAlignmentCommand = new RelayCommand<LyricsTextAlignment>(ApplyLyricsAlignment);
        ApplyLyricsVerticalAlignmentCommand = new RelayCommand<LyricsVerticalAlignment>(ApplyLyricsVerticalAlignment);
        ApplyBackgroundModeCommand = new RelayCommand<LyricsBackgroundMode>(ApplyBackgroundMode);
        ApplyRegionDisplayCommand = new RelayCommand<LyricsRegionDisplay>(ApplyRegionDisplay);
        IncreaseLyricsFontSizeCommand = new RelayCommand(() => StepLyricsFontSize(+LyricsFontSizeStep), () => ActiveLyricsFontSize < LyricsFontSizeMax);
        DecreaseLyricsFontSizeCommand = new RelayCommand(() => StepLyricsFontSize(-LyricsFontSizeStep), () => ActiveLyricsFontSize > LyricsFontSizeMin);
        IncreaseLyricsLineSpacingCommand = new RelayCommand(() => StepLyricsLineSpacing(+LyricsLineSpacingStep), () => ActiveLyricsLineSpacing < LyricsLineSpacingMax);
        DecreaseLyricsLineSpacingCommand = new RelayCommand(() => StepLyricsLineSpacing(-LyricsLineSpacingStep), () => ActiveLyricsLineSpacing > LyricsLineSpacingMin);
        ApplyTextColorHexCommand = new RelayCommand<string>(hex => ApplyColorHex(hex, isBackground: false));
        ApplyBackgroundColorHexCommand = new RelayCommand<string>(hex => ApplyColorHex(hex, isBackground: true));
        SaveAppearanceTemplateCommand = new AsyncRelayCommand(SaveAppearanceTemplateAsync);
        ApplyAppearanceTemplateCommand = new AsyncRelayCommand(ApplyAppearanceTemplateAsync, () => !string.IsNullOrWhiteSpace(SelectedAppearanceTemplate));
        DeleteAppearanceTemplateCommand = new RelayCommand(DeleteAppearanceTemplate, () => !string.IsNullOrWhiteSpace(SelectedAppearanceTemplate));
        ToggleLyricsBoldCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorBold, ActiveLyricsBold));
        ToggleLyricsItalicCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorItalic, ActiveLyricsItalic));
        ToggleLyricsShadowCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShadow, ActiveLyricsShadow));
        ToggleLyricsUnderlineCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorUnderline, ActiveLyricsUnderline));
        ToggleEmphasisChorusOnlyCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorEmphasisChorusOnly, ActiveLyricsEmphasisChorusOnly));
        TogglePanelTransparentCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorPanelTransparent, ActiveLyricsPanelTransparent));
        ToggleLyricsPositionIndicatorCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowPositionIndicator, ActiveLyricsPositionIndicator));
        ToggleLyricsItemNumberCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowItemNumber, ActiveLyricsItemNumber));
        ToggleLyricsCopyrightCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowCopyright, ActiveLyricsCopyright));
        ToggleLyricsNextItemCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowNextItem, ActiveLyricsNextItem));
        ToggleFadeTransitionCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorUseFadeTransition, ActiveFadeTransition));
        ApplyTransitionDurationCommand = new RelayCommand<int>(ApplyTransitionDuration);
        ApplyTransitionKindCommand = new RelayCommand<LyricsTransitionKind>(ApplyTransitionKind);
        ClearOutputBackgroundImageCommand = new RelayCommand(ClearOutputBackgroundImage);
        OpenRecentWorshipListCommand = new AsyncRelayCommand<string>(OpenRecentWorshipListAsync);
        ValidateWorshipListCommand = new RelayCommand(ValidateWorshipList);
        // 라이브 조옮김 ↑/↓/원조 — ±반음 이동(±11 클램프) 후 라이브 곡을 재송출해 코드 줄을 다시 그린다.
        TransposeLiveUpCommand = new RelayCommand(() => SetLiveTranspose(LiveTransposeSemitones + 1));
        TransposeLiveDownCommand = new RelayCommand(() => SetLiveTranspose(LiveTransposeSemitones - 1));
        TransposeLiveResetCommand = new RelayCommand(() => SetLiveTranspose(0));
        RefreshRecentWorshipLists();
        ToggleLyricsTitleHeadingCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowTitleHeading, ActiveLyricsTitleHeading));
        ToggleLyricsOutlineCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorOutline, ActiveLyricsOutline));
        ApplyTitleHeadingAlignmentCommand = new RelayCommand<LyricsTextAlignment>(ApplyTitleHeadingAlignment);
        ToggleTitleHeadingFirstScreenOnlyCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly, ActiveTitleHeadingFirstScreenOnly));
        ToggleAutoRotateCommand = new RelayCommand(ToggleAutoRotate, () => _session.Current.State == LiveState.Active);
        AddSelectedLibrarySongCommand = new RelayCommand(AddSelectedLibrarySong, () => Library.SelectedSong is not null);
        AddSearchedSongCommand = new AsyncRelayCommand(AddSearchedSongAsync, () => SelectedSearchResult is not null);
        AddLookupTitleCommand = new AsyncRelayCommand(AddLookupTitleAsync, () => SelectedTitleCandidate is not null);
        MoveSelectedItemUpCommand = new RelayCommand(() => MoveSelectedItem(-1), () => CanMoveSelectedItem(-1));
        MoveSelectedItemDownCommand = new RelayCommand(() => MoveSelectedItem(+1), () => CanMoveSelectedItem(+1));
        RemoveSelectedItemCommand = new RelayCommand(RemoveSelectedItem, () => SelectedItem is not null);
        NextLyricsPageCommand = new RelayCommand(NextLyricsPage, CanGoNextLyricsPage);
        JumpToLyricsSectionCommand = new RelayCommand<string>(JumpToLyricsSection, CanJumpToLyricsSection);
        PreviousLyricsPageCommand = new RelayCommand(PreviousLyricsPage, CanGoPreviousLyricsPage);
        // 라이브러리 선택 곡이 바뀌면 "예배 순서에 추가" 활성 상태를 맞춘다.
        Library.PropertyChanged += OnLibraryPropertyChanged;
        // 재검색으로 결과가 통째로 교체되면 부모의 선택을 비워 stale 방지(아래 OnSearchResultsChanged 참고).
        Search.SearchResults.CollectionChanged += OnSearchResultsChanged;
        // 제목 재조회로 후보가 통째로 교체될 때도 동일하게 stale 선택을 정리한다.
        Search.LookupCandidates.CollectionChanged += OnLookupCandidatesChanged;

        ApplyOperationalSettings(updateStatus: false);
        // 시작 시 큐는 비어 있다 — 과거의 더미 3항목 시드(SeedPlaceholderQueue)를 제거(§7 P0).
        // 운영자가 곡·성경·파일을 직접 추가하거나 "최근 예배 순서"로 불러온다. 좌측 패널이 빈 상태 안내를 보여 준다.
        RefreshOutputDisplays();
        RefreshAppearanceTemplateNames();
    }

    public ObservableCollection<LiveQueueItem> Queue { get; } = new();
    public ObservableCollection<OutputDisplay> OutputDisplays { get; } = new();

    public LiveBarViewModel LiveBar { get; } = new();

    public ILiveSessionService Session => _session;

    public IRelayCommand OpenOutputCommand { get; }
    public IAsyncRelayCommand CloseOutputCommand { get; }
    public IAsyncRelayCommand GoLiveCommand { get; }
    public IAsyncRelayCommand StopLiveCommand { get; }
    public IRelayCommand NextItemCommand { get; }
    public IRelayCommand PreviousItemCommand { get; }
    /// <summary>예배 순서의 첫 항목으로 이동(레거시 First). 라이브 중이면 그 항목을 송출.</summary>
    public IRelayCommand FirstItemCommand { get; }
    /// <summary>예배 순서의 마지막 항목으로 이동(레거시 Last). 라이브 중이면 그 항목을 송출.</summary>
    public IRelayCommand LastItemCommand { get; }
    public IAsyncRelayCommand HideOutputCommand { get; }
    public IAsyncRelayCommand BlackScreenCommand { get; }
    public IAsyncRelayCommand ClearOutputCommand { get; }
    public IRelayCommand RestoreOutputCommand { get; }
    public IAsyncRelayCommand RestartCurrentItemCommand { get; }
    public IRelayCommand RefreshOutputCommand { get; }
    public IAsyncRelayCommand NextSlideCommand { get; }
    public IAsyncRelayCommand PreviousSlideCommand { get; }
    public IAsyncRelayCommand<int> GoToSlideCommand { get; }
    public IRelayCommand<OutputAppearancePreset> ApplyOutputAppearanceCommand { get; }
    public IRelayCommand<LyricsTextAlignment> ApplyLyricsAlignmentCommand { get; }
    public IRelayCommand<LyricsVerticalAlignment> ApplyLyricsVerticalAlignmentCommand { get; }

    /// <summary>배경 이미지 표시 모드(채움/맞춤/가운데/타일) 적용 — 설정→출력 VM 라이브 반영(레거시 Def_ImageMode).</summary>
    public IRelayCommand<LyricsBackgroundMode> ApplyBackgroundModeCommand { get; }

    public bool BackgroundModeIsFill => ActiveBackgroundMode == LyricsBackgroundMode.Fill;
    public bool BackgroundModeIsFit => ActiveBackgroundMode == LyricsBackgroundMode.Fit;
    public bool BackgroundModeIsCenter => ActiveBackgroundMode == LyricsBackgroundMode.Center;
    public bool BackgroundModeIsTile => ActiveBackgroundMode == LyricsBackgroundMode.Tile;

    /// <summary>이중 언어 영역 표시 모드(둘다/Region1만/Region2만) 적용 — 설정→출력 VM 라이브 반영(레거시 Def_ShowRegion).</summary>
    public IRelayCommand<LyricsRegionDisplay> ApplyRegionDisplayCommand { get; }

    public bool RegionDisplayIsBoth => ActiveRegionDisplay == LyricsRegionDisplay.Both;
    public bool RegionDisplayIsRegion1Only => ActiveRegionDisplay == LyricsRegionDisplay.Region1Only;
    public bool RegionDisplayIsRegion2Only => ActiveRegionDisplay == LyricsRegionDisplay.Region2Only;
    public IRelayCommand IncreaseLyricsFontSizeCommand { get; }
    public IRelayCommand DecreaseLyricsFontSizeCommand { get; }
    public IRelayCommand IncreaseLyricsLineSpacingCommand { get; }
    public IRelayCommand DecreaseLyricsLineSpacingCommand { get; }
    public IRelayCommand<string> ApplyTextColorHexCommand { get; }
    public IRelayCommand<string> ApplyBackgroundColorHexCommand { get; }
    public IAsyncRelayCommand SaveAppearanceTemplateCommand { get; }
    public IAsyncRelayCommand ApplyAppearanceTemplateCommand { get; }
    public IRelayCommand DeleteAppearanceTemplateCommand { get; }
    public IRelayCommand ToggleLyricsBoldCommand { get; }
    public IRelayCommand ToggleLyricsItalicCommand { get; }
    public IRelayCommand ToggleLyricsShadowCommand { get; }

    public IRelayCommand ToggleLyricsUnderlineCommand { get; }

    public IRelayCommand ToggleEmphasisChorusOnlyCommand { get; }
    /// <summary>Display Panel 배경 투명 토글(레거시 Def_PanelTransparent) — 설정→출력 VM 라이브 반영.</summary>
    public IRelayCommand TogglePanelTransparentCommand { get; }
    public IRelayCommand ToggleLyricsPositionIndicatorCommand { get; }
    public IRelayCommand ToggleLyricsItemNumberCommand { get; }
    public IRelayCommand ToggleLyricsCopyrightCommand { get; }
    public IRelayCommand ToggleLyricsNextItemCommand { get; }
    public IRelayCommand ToggleFadeTransitionCommand { get; }
    public IRelayCommand<int> ApplyTransitionDurationCommand { get; }
    public IRelayCommand<LyricsTransitionKind> ApplyTransitionKindCommand { get; }
    public IRelayCommand ClearOutputBackgroundImageCommand { get; }
    public IAsyncRelayCommand<string> OpenRecentWorshipListCommand { get; }

    /// <summary>최근 연/저장한 예배 순서 이름(최신순) — 파일 메뉴 "최근 예배 순서" 서브메뉴 바인딩(레거시 Recent Edits).</summary>
    public ObservableCollection<string> RecentWorshipLists { get; } = new();

    /// <summary>예배 순서 검증으로 발견한 문제 목록(없으면 비어 있음) — 도구 메뉴 "예배 순서 검증" 결과.</summary>
    public ObservableCollection<WorshipItemProblem> WorshipListProblems { get; } = new();

    /// <summary>예배 순서 검증을 실행한다(라이브 송출 전 깨진 PPT·미디어 파일 점검, 레거시 ValidateWorshipListItems).</summary>
    public IRelayCommand ValidateWorshipListCommand { get; }

    /// <summary>라이브 조옮김 상태 라벨(예: "원조", "조옮김 +2", "조옮김 -1") — 메뉴/툴팁 표시용.</summary>
    public string LiveTransposeLabel => LiveTransposeSemitones == 0
        ? "원조"
        : $"조옮김 {(LiveTransposeSemitones > 0 ? "+" : "")}{LiveTransposeSemitones}";

    /// <summary>라이브 코드 조옮김 ↑(반음 올림, 레거시 Transpose Up Semi-Tone). 코드 표시 on 일 때 송출 코드 이동.</summary>
    public IRelayCommand TransposeLiveUpCommand { get; }

    /// <summary>라이브 코드 조옮김 ↓(반음 내림, 레거시 Transpose Down Semi-Tone).</summary>
    public IRelayCommand TransposeLiveDownCommand { get; }

    /// <summary>라이브 코드 조옮김 원조 복귀(0, 레거시 To Capo 0 의 운영 단순화).</summary>
    public IRelayCommand TransposeLiveResetCommand { get; }

    public IRelayCommand ToggleLyricsTitleHeadingCommand { get; }

    public IRelayCommand ToggleLyricsOutlineCommand { get; }

    public IRelayCommand<LyricsTextAlignment> ApplyTitleHeadingAlignmentCommand { get; }

    public IRelayCommand ToggleTitleHeadingFirstScreenOnlyCommand { get; }
    public IRelayCommand ToggleAutoRotateCommand { get; }
    public IRelayCommand AddSelectedLibrarySongCommand { get; }
    public IAsyncRelayCommand AddSearchedSongCommand { get; }
    public IAsyncRelayCommand AddLookupTitleCommand { get; }
    public IRelayCommand MoveSelectedItemUpCommand { get; }
    public IRelayCommand MoveSelectedItemDownCommand { get; }
    public IRelayCommand RemoveSelectedItemCommand { get; }
    public IRelayCommand NextLyricsPageCommand { get; }
    public IRelayCommand<string> JumpToLyricsSectionCommand { get; }
    public IRelayCommand PreviousLyricsPageCommand { get; }

    public void LoadQueue(IEnumerable<LiveQueueItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Queue.Clear();
        foreach (var item in items)
        {
            Queue.Add(item);
        }

        SelectedItem = Queue.FirstOrDefault();
        StatusText = Queue.Count == 0 ? "송출할 항목이 없습니다" : $"{Queue.Count}개 항목 로드됨";
        RefreshPowerPointLimitState(updateStatus: true);
        NotifyCommandStates();
    }

    public LiveQueueItem? AddBibleSelection(BibleSelection selection)
    {
        var item = CreateBibleItem(selection);
        if (item is null)
        {
            return null;
        }

        // 선택 항목 바로 뒤에 끼운다(없으면 맨 끝) — 버튼·우클릭·typed 추가의 기존 규칙.
        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"성경 구절 추가됨: {selection.Title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>
    /// 성경 구절을 드롭한 위치(타깃 항목) 앞에 끼운다 — 본문에서 큐로 끌어다 놓는 드래그-드롭 경로(레거시 BibleText DragDrop).
    /// 타깃이 없으면(빈 공간) 맨 끝에. 항목 생성·본문 확장은 AddBibleSelection 과 동일(CreateBibleItem 공유).
    /// </summary>
    public LiveQueueItem? AddBibleSelectionRelativeTo(BibleSelection selection, LiveQueueItem? targetItem)
    {
        var item = CreateBibleItem(selection);
        if (item is null)
        {
            return null;
        }

        // 같은 값(제목·Id)의 항목이 큐에 여러 번 있을 수 있으므로(입례/봉헌 반복 등) 값-동일 IndexOf 가 아니라
        // 참조 일치(IndexOfReference)로 "드롭한 바로 그 인스턴스"의 위치를 찾는다 — 재정렬(MoveQueueItemRelativeTo)과 동일 규칙.
        var targetIndex = targetItem is null ? -1 : IndexOfReference(targetItem);
        var insertIndex = targetIndex >= 0 ? targetIndex : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"성경 구절 추가됨: {selection.Title}";
        NotifyCommandStates();
        return item;
    }

    // 성경 선택을 큐 항목으로 만든다(본문 확장 포함). 빈 선택이면 안내 후 null — 삽입 위치는 호출자가 정한다.
    private LiveQueueItem? CreateBibleItem(BibleSelection selection)
    {
        if (string.IsNullOrWhiteSpace(selection.IdString) || string.IsNullOrWhiteSpace(selection.Title))
        {
            StatusText = "선택된 성경 구절이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        // 선택 구절을 실제 본문(절 단위 페이지·이중 언어면 보조 언어까지)으로 확장해 항목에 싣는다 —
        // 예전엔 성경 항목이 제목만 송출했으나 이제 회중 화면에 구절 본문이 보인다. 본문이 비면(파일 없음 등) 제목만(폴백).
        var body = Bible.ExpandSelectionBody(selection.IdString);
        return new LiveQueueItem(selection.IdString, selection.Title, LiveItemKinds.Bible)
        {
            Lyrics = body,
        };
    }

    /// <summary>
    /// 라이브러리에서 고른 실제 곡을 예배 순서(큐)에 추가한다(라이브 큐 도메인 plumbing — placeholder 대체 기반).
    /// 선택 항목 바로 뒤에 삽입하고 새 항목을 선택. AddBibleSelection 과 동일 규칙.
    /// sequence: 곡 절 순서(있으면 절을 그 순서로 반복 송출). SongSummary 엔 없어 상세 로드 경로에서 넘긴다.
    /// </summary>
    public LiveQueueItem? AddSong(Data.SongSummary? song, string? sequence = null, string? formatData = null)
    {
        if (song is null || string.IsNullOrWhiteSpace(song.Title))
        {
            StatusText = "선택된 곡이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        var item = new LiveQueueItem($"song:{song.SongId}", song.Title, LiveItemKinds.Song)
        {
            Lyrics = song.Lyrics,
            Sequence = sequence,
            // 곡 번호(출력 "곡 번호 표시" 설정 on 일 때 회중 화면에 표시).
            SongNumber = song.SongNumber,
            // 저작권(출력 "저작권 표시" 설정 on 일 때 표시).
            Copyright = song.Copyright,
            // 곡별 출력 색(레거시 v32 FormatData) — 있으면 라이브 송출 시 그 곡의 색으로 표시.
            FormatData = formatData,
        };
        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"곡 추가됨: {song.Title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>
    /// 찬양집 색인에서 더블클릭한 곡을 예배 순서에 추가한다(FrmMain PraiseBook 인터랙티브 목록 대응).
    /// 색인 항목엔 가사가 없으므로 현재 라이브러리에서 같은 곡(가사 포함 SongSummary)을 찾아 AddSong 으로 넘긴다.
    /// 해석 우선순위: ① SongId(있으면 정확 — 같은 제목·번호의 다른 곡/언어를 안전히 가름) → ② 제목+번호 → ③ 제목만
    /// (저장된 찬양집은 SongId=0 이라 ②③으로 폴백). 라이브러리에 같은 곡이 없으면 안내만 하고 큐는 그대로 둔다.
    /// </summary>
    public LiveQueueItem? AddPraiseBookSong(string? title, int songNumber, int songId = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            StatusText = "선택된 곡이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        // ① SongId 정확 일치(가장 안전 — 중복 제목 모호성 없음).
        var song = songId != 0
            ? Library.Songs.FirstOrDefault(candidate => candidate.SongId == songId)
            : null;
        // ② 제목 + 번호(저장된 찬양집·SongId 없는 경로).
        song ??= Library.Songs.FirstOrDefault(candidate =>
            string.Equals(candidate.Title, title, StringComparison.Ordinal)
            && (songNumber == 0 || candidate.SongNumber == songNumber));
        // ③ 제목만(번호도 안 맞을 때 마지막 폴백).
        song ??= Library.Songs.FirstOrDefault(candidate =>
            string.Equals(candidate.Title, title, StringComparison.Ordinal));

        if (song is null)
        {
            StatusText = $"라이브러리에서 곡을 찾을 수 없습니다: {title}";
            NotifyCommandStates();
            return null;
        }

        return AddSong(song);
    }

    /// <summary>
    /// 좌측 "검색" 탭에서 고른 교차 검색 결과를 예배 순서(큐)에 추가한다(§7.4 단일 콘솔 통합 — 검색 창 인라인 흡수).
    /// 검색 결과(SongSearchResult)에는 가사가 없으므로, 선택 결과의 SongId 로 곡 상세(가사 포함)를 불러온 뒤 AddSong 으로 채운다.
    /// </summary>
    private async Task AddSearchedSongAsync()
    {
        var result = SelectedSearchResult;
        if (result is null)
        {
            StatusText = "선택된 검색 결과가 없습니다.";
            NotifyCommandStates();
            return;
        }

        await AddSongByIdAsync(result.SongId, result.Title).ConfigureAwait(true);
    }

    /// <summary>
    /// 검색 탭 "제목" 모드에서 고른 제목 후보를 예배 순서에 추가한다(SearchUsageWindow 의 Titles 인라인 흡수).
    /// 곡 검색과 동일하게 SongId 로 곡 상세(가사)를 불러와 큐에 채운다.
    /// </summary>
    private async Task AddLookupTitleAsync()
    {
        var candidate = SelectedTitleCandidate;
        if (candidate is null)
        {
            StatusText = "선택된 제목이 없습니다.";
            NotifyCommandStates();
            return;
        }

        await AddSongByIdAsync(candidate.SongId, candidate.Title).ConfigureAwait(true);
    }

    /// <summary>
    /// SongId 로 곡 상세(가사 포함)를 불러와 예배 순서에 추가하는 공통 경로.
    /// 곡 검색·제목 조회 결과 모두 가사를 들고 있지 않으므로, 여기서 한 번만 DB 를 조회해 가사를 채운다.
    /// </summary>
    private async Task AddSongByIdAsync(int songId, string fallbackTitle)
    {
        var databasePath = Search.DatabasePath;
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            StatusText = "검색 DB 경로가 설정되지 않았습니다.";
            NotifyCommandStates();
            return;
        }

        var detail = await _songDetail.GetSongDetailAsync(databasePath, songId).ConfigureAwait(true);
        if (detail is null)
        {
            StatusText = $"곡을 찾을 수 없습니다: {fallbackTitle}";
            NotifyCommandStates();
            return;
        }

        AddSong(
            new Data.SongSummary(
                detail.SongId,
                detail.Title,
                detail.AlternateTitle,
                detail.FolderNo,
                detail.SongNumber,
                detail.Category,
                detail.Key,
                detail.Lyrics,
                detail.Copyright), // 저작권 — 출력 "저작권 표시"에 쓰인다.
            detail.Sequence, // 곡 절 순서 — 있으면 절을 그 순서로 반복 송출(레거시 인코딩이면 매칭 0→선형 폴백).
            detail.FormatData); // 곡별 출력 색(레거시 v32) — 라이브 송출 시 그 곡의 글자·배경색 적용.
    }

    // 예배 순서 항목 이동(↑/↓) — 큐 순서를 재정렬한다(FrmMain Move Item Up/Down). 선택 항목은 유지.
    // LiveQueueItem 은 값 동등성 record 라 Queue.IndexOf 는 "값이 같은 첫 항목"을 돌려준다.
    // 같은 곡을 예배 순서에 두 번 넣는 일이 흔하므로(입례·봉헌 등) 선택한 "바로 그 인스턴스"를
    // 참조 동일성으로 찾아야 엉뚱한 동일-값 항목을 이동/제거하지 않는다.
    private int IndexOfSelectedReference() => IndexOfReference(SelectedItem);

    // 큐에서 특정 인스턴스의 위치를 참조(ReferenceEquals)로 찾는다.
    // LiveQueueItem 은 record(값 동등성)라 IndexOf 는 같은-값 중복을 구분 못 하므로 참조 비교가 필수.
    private int IndexOfReference(LiveQueueItem? item)
    {
        if (item is null)
        {
            return -1;
        }

        for (var i = 0; i < Queue.Count; i++)
        {
            if (ReferenceEquals(Queue[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 예배 순서에서 한 항목을 임의 위치로 이동(드래그 재정렬, §7.5 P1).
    /// 항목은 참조로 찾고(같은-값 중복 안전), 목표 위치는 큐 범위로 클램프한다. 이동한 항목의 선택을 유지.
    /// </summary>
    public void MoveQueueItem(LiveQueueItem item, int targetIndex)
    {
        var from = IndexOfReference(item);
        if (from < 0)
        {
            return; // 큐에 없는 항목이면 무시(빈 큐도 여기서 걸림 — IndexOfReference 가 -1)
        }

        var target = Math.Clamp(targetIndex, 0, Queue.Count - 1);
        if (from == target)
        {
            return; // 제자리면 아무 것도 하지 않음(불필요한 컬렉션 이벤트 방지)
        }

        Queue.Move(from, target);
        SelectedItem = item; // 이동 후에도 같은 항목이 선택되어 있도록 유지
        NotifyCommandStates();
    }

    /// <summary>
    /// 드롭 위치의 "타깃 항목"을 기준으로 항목을 이동(드래그 재정렬, §7.5 P1).
    /// 타깃 인덱스도 참조(IndexOfReference)로 구해 같은-값 중복에서 엉뚱한 인스턴스 위치로 가지 않게 한다
    /// (WPF ItemCollection.IndexOf 는 값 동등성이라 record 중복을 구분 못 함 — code-review CRITICAL 반영).
    /// target 이 null 이거나 큐에 없으면 맨 끝으로 이동(빈 공간 드롭).
    /// </summary>
    public void MoveQueueItemRelativeTo(LiveQueueItem item, LiveQueueItem? target)
    {
        var targetIndex = target is null ? Queue.Count - 1 : IndexOfReference(target);
        if (targetIndex < 0)
        {
            targetIndex = Queue.Count - 1; // 타깃이 큐에 없으면 맨 끝
        }

        MoveQueueItem(item, targetIndex);
    }

    private void MoveSelectedItem(int delta)
    {
        if (SelectedItem is not { } item)
        {
            return;
        }

        var index = IndexOfSelectedReference();
        var target = index + delta;
        if (index < 0 || target < 0 || target >= Queue.Count)
        {
            return;
        }

        Queue.Move(index, target);
        SelectedItem = item; // 같은 항목 유지(이동 후에도 선택 따라감)
        NotifyCommandStates();
    }

    private bool CanMoveSelectedItem(int delta)
    {
        var index = IndexOfSelectedReference();
        var target = index + delta;
        return index >= 0 && target >= 0 && target < Queue.Count;
    }

    // 예배 순서에서 선택 항목 제거 — 인접 항목을 새로 선택(라이브 송출 자체는 세션이 유지).
    private void RemoveSelectedItem()
    {
        if (SelectedItem is not { } item)
        {
            return;
        }

        var index = IndexOfSelectedReference();
        if (index < 0)
        {
            return;
        }

        Queue.RemoveAt(index); // 참조로 찾은 정확한 인덱스를 제거(동일-값 중복 안전)

        // 라이브 송출 중인 바로 그 항목을 큐에서 뺐다면, 슬라이드 이동 가드(_liveItemId)가
        // 큐에 없는 고아 Id 를 기준으로 판단하지 않도록 라이브 추적을 정리한다(세션 송출 자체는 유지).
        if (_liveItemId == item.Id)
        {
            _liveItemId = null;
        }

        SelectedItem = Queue.Count == 0 ? null : Queue[Math.Min(index, Queue.Count - 1)];
        StatusText = $"항목 제거: {item.Title}";
        NotifyCommandStates();
    }

    /// <summary>PowerPoint 파일을 예배 순서(큐)에 추가(선택 시 썸네일 렌더 디스패치).</summary>
    public LiveQueueItem? AddPowerPoint(string filePath) => AddExternalFileItem(filePath, LiveItemKinds.PowerPoint, "PowerPoint 파일");

    /// <summary>미디어 파일을 예배 순서(큐)에 추가(선택 시 미디어 Load 디스패치).</summary>
    public LiveQueueItem? AddMedia(string filePath) => AddExternalFileItem(filePath, LiveItemKinds.Media, "미디어 파일");

    private LiveQueueItem? AddExternalFileItem(string filePath, string kind, string label)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusText = $"선택된 {label}이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        var title = Path.GetFileNameWithoutExtension(filePath);
        var item = new LiveQueueItem($"{kind.ToLowerInvariant()}:{filePath}", title, kind) { ContentPath = filePath };
        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"{label} 추가됨: {title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>저장된 예배 순서(워십 리스트) 이름 목록(레거시 FrmManageItemLists 대응 — G2).</summary>
    public IReadOnlyList<string> GetSavedWorshipLists() => _worshipLists.ListNames();

    /// <summary>현재 예배 순서(큐)를 이름으로 저장한다.</summary>
    public async Task SaveWorshipListAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusText = "저장할 예배 순서 이름을 입력하세요.";
            return;
        }

        try
        {
            await _worshipLists.SaveAsync(name.Trim(), Queue.ToArray()).ConfigureAwait(true);
            RecordRecentWorshipList(name.Trim());
            StatusText = $"예배 순서 저장됨: {name.Trim()} ({Queue.Count}개)";
        }
        catch (ArgumentException)
        {
            StatusText = "예배 순서 이름에 사용할 수 없는 문자가 있습니다.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // async void 핸들러로 새지 않도록 IO 실패를 status 로 변환(앱 크래시 방지).
            StatusText = $"예배 순서 저장 실패: {ex.Message}";
        }
    }

    /// <summary>저장된 예배 순서를 불러와 현재 큐를 교체한다.</summary>
    public async Task LoadWorshipListAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        try
        {
            var items = await _worshipLists.LoadAsync(name.Trim()).ConfigureAwait(true);
            LoadQueue(items);
            RecordRecentWorshipList(name.Trim());
            StatusText = $"예배 순서 불러옴: {name.Trim()} ({items.Count}개)";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"예배 순서 불러오기 실패: {ex.Message}";
        }
    }

    /// <summary>현재 예배 세션(마지막으로 저장/불러온 예배 순서) 이름 — 세션 메모 키로 쓰인다. 없으면 빈 문자열.</summary>
    public string CurrentWorshipListName { get; private set; } = string.Empty;

    // 최근 예배 순서에 이름을 기록하고 메뉴 바인딩 컬렉션을 갱신한다(저장·불러오기 성공 시 호출).
    private void RecordRecentWorshipList(string name)
    {
        CurrentWorshipListName = name;
        _recentWorshipLists.Record(name);
        RefreshRecentWorshipLists();
    }

    private void RefreshRecentWorshipLists()
    {
        RecentWorshipLists.Clear();
        foreach (var name in _recentWorshipLists.GetRecent())
        {
            RecentWorshipLists.Add(name);
        }
    }

    // 라이브 조옮김 값을 설정한다(±11 반음 클램프). 값이 바뀌면 라이브 곡을 같은 절로 재송출해 코드를 다시 그리고,
    // 상태바에 현재 조옮김을 알린다. 코드 표시가 꺼져 있으면 코드가 안 보이므로 시각 효과는 없지만 값은 유지된다.
    private void SetLiveTranspose(int semitones)
    {
        // ±11 로 클램프 — 12음 체계에서 ±12 는 옥타브라 원조와 코드가 똑같아진다(라벨만 "조옮김 +12"로 헷갈림).
        // 그래서 원조와 구별되는 최대 이동인 ±11 까지만 허용한다(ChordTransposer 는 % 12 로 계산).
        var clamped = Math.Clamp(semitones, -11, 11);
        if (clamped == LiveTransposeSemitones)
        {
            return; // 변화 없음(이미 한계값) — 불필요한 재송출 방지.
        }

        LiveTransposeSemitones = clamped;
        RepublishLiveSongForBodyChange();
        StatusText = LiveTransposeSemitones == 0 ? "조옮김: 원조" : $"조옮김: {LiveTransposeLabel}";
    }

    /// <summary>
    /// 예배 순서를 검증해 깨진 PPT·미디어 파일을 찾는다(레거시 ValidateWorshipListItems — 예배 중 사고 예방).
    /// 결과를 WorshipListProblems 에 채우고 StatusText 로 요약한다. 문제 없으면 "모든 항목 정상".
    /// </summary>
    public void ValidateWorshipList()
    {
        var problems = _worshipValidator.Validate(Queue);
        WorshipListProblems.Clear();
        foreach (var problem in problems)
        {
            WorshipListProblems.Add(problem);
        }

        HasWorshipListProblems = problems.Count > 0;

        if (problems.Count == 0)
        {
            StatusText = Queue.Count == 0
                ? "예배 순서가 비어 있습니다."
                : $"예배 순서 검증: 모든 항목 정상 ({Queue.Count}개).";
        }
        else
        {
            // 문제 항목 제목을 앞에서 몇 개만 요약(많으면 "외 N건")해 운영자가 바로 알아보게 한다.
            var titles = problems.Take(3).Select(p => p.Message);
            var summary = string.Join(" · ", titles);
            if (problems.Count > 3)
            {
                summary += $" 외 {problems.Count - 3}건";
            }

            StatusText = $"예배 순서 검증: 문제 {problems.Count}건 — {summary}";
        }

        // 텔레메트리의 succeeded 는 "검증 명령이 정상 수행됨"을 뜻한다(문제를 찾는 것도 검증의 정상 동작이므로
        // 문제 발견 = 실패가 아니다). 발견한 문제 수는 StatusText 메시지에 담긴다.
        _telemetry.Record(MainCommandIds.WorshipListValidate, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    // 최근 예배 순서 메뉴 항목 클릭 → 해당 이름으로 다시 불러온다.
    private async Task OpenRecentWorshipListAsync(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await LoadWorshipListAsync(name).ConfigureAwait(true);
        }
    }

    /// <summary>저장된 예배 순서를 삭제한다.</summary>
    public void DeleteWorshipList(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _worshipLists.Delete(name.Trim());
        StatusText = $"예배 순서 삭제됨: {name.Trim()}";
    }

    /// <summary>
    /// 저장된 예배 순서의 이름을 바꾼다(레거시 FrmManageItemLists + FrmUpdateFileName 이름 변경 대응).
    /// 이름이 같으면 변경 없이 성공으로 처리하고, 이미 있는 이름(대소문자 무시)으로는 거부한다.
    /// 반환값: 실제로 처리됐으면 true, 입력이 잘못됐거나 중복이면 false.
    /// </summary>
    public bool RenameWorshipList(string oldName, string newName)
    {
        var from = (oldName ?? "").Trim();
        var to = (newName ?? "").Trim();
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
        {
            StatusText = "이름이 비어 있어 변경할 수 없습니다.";
            return false;
        }

        // 이름이 같으면(대소문자 무시) 바꿀 게 없다 — 성공으로 처리.
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 이미 있는 이름으로는 덮어쓰지 않는다(스토어도 방어하지만 VM 에서 먼저 막아 사용자에게 알린다).
        if (_worshipLists.ListNames().Any(n => string.Equals(n, to, StringComparison.OrdinalIgnoreCase)))
        {
            StatusText = $"이미 있는 이름입니다: {to}";
            return false;
        }

        // 사전 검사를 통과해도 스토어가 막을 수 있다(파일명에 못 쓰는 문자/예약명, 검사~이동 사이 경쟁, 쓰기 권한 등).
        // 이런 예외는 운영자에게 친절한 상태 메시지로 바꿔 보여 주고, 개발자용 예외 창이 뜨지 않게 한다.
        try
        {
            _worshipLists.Rename(from, to);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            StatusText = $"이름을 바꾸지 못했습니다: {to}";
            return false;
        }

        StatusText = $"예배 순서 이름 변경: {from} → {to}";
        return true;
    }

    public void BindShortcuts(ShortcutRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        // 명령 팔레트가 명령 id 로 실행할 수 있도록 레지스트리를 보관(아래 Bind 들이 id→동작을 등록).
        _shortcutRegistry = registry;

        foreach (var shortcut in ShortcutSettings.ApplyOverrides(
                     _commandCatalog.GetDefaultShortcuts(),
                     _settings.Current.Shortcuts))
        {
            RegisterIfMissing(registry, shortcut);
        }

        registry.Bind(MainCommandIds.OutputOpen, () => OpenOutputCommand.Execute(null));
        registry.Bind(MainCommandIds.OutputClose, () => _ = CloseOutputCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveGo, () => _ = GoLiveCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveStop, () => _ = StopLiveCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveNext, () => NextItemCommand.Execute(null));
        registry.Bind(MainCommandIds.LivePrevious, () => PreviousItemCommand.Execute(null));
        registry.Bind(MainCommandIds.LiveBlack, () => _ = BlackScreenCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveHide, () => _ = HideOutputCommand.ExecuteAsync(null));

        // 화면 제어 보강(§7.3-B) — 명령 팔레트(⌘K)에서 실행. CanExecute 가 false 면(예: 출력 창 닫힘·라이브 아님)
        // 실행하지 않아 버튼 비활성과 동일하게 동작한다(code-review MINOR — RelayCommand.Execute 는 CanExecute 를
        // 자체 검사하지 않으므로 여기서 명시 게이트). IRelayCommand.Execute 는 async 커맨드면 fire-and-forget.
        void BindGated(string commandId, CommunityToolkit.Mvvm.Input.IRelayCommand command)
            => registry.Bind(commandId, () =>
            {
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                }
            });

        BindGated(MainCommandIds.LiveClear, ClearOutputCommand);
        BindGated(MainCommandIds.LiveRestart, RestartCurrentItemCommand);
        BindGated(MainCommandIds.LiveRefresh, RefreshOutputCommand);
        BindGated(MainCommandIds.LiveRestore, RestoreOutputCommand);
        BindGated(MainCommandIds.LiveAutoRotate, ToggleAutoRotateCommand);
    }

    public void RefreshOutputDisplays()
    {
        var preferredId = SelectedOutputDisplay?.Id;
        if (string.IsNullOrWhiteSpace(preferredId))
        {
            preferredId = _settings.Get(EasiSettingKeys.DefaultOutputMonitorId);
        }

        var displays = _display.GetDisplays();

        OutputDisplays.Clear();
        foreach (var display in displays)
        {
            OutputDisplays.Add(display);
        }

        var selected = GetPreferredOutputDisplay(preferredId, displays);
        var matching = OutputDisplays.FirstOrDefault(display =>
            string.Equals(display.Id, selected.Id, StringComparison.OrdinalIgnoreCase)) ?? selected;
        if (!OutputDisplays.Contains(matching))
        {
            OutputDisplays.Add(matching);
        }

        SelectedOutputDisplay = matching;
    }

    // PPT 렌더 크기 — 출력 창이 닫혀 있을 때의 가벼운 미리보기용 기본값(출력 미송출 상태).
    private const int PptPreviewWidth = 960;
    private const int PptPreviewHeight = 540;

    // 출력 창이 열려 있으면 PPT 를 출력 모니터 해상도로 렌더해 송출을 선명하게 한다.
    // 다만 4K 등 초고해상도에서 매 선택마다 거대한 JPG 를 만드는 비용을 막기 위해 1080p 로 상한.
    private const int PptMaxRenderWidth = 1920;
    private const int PptMaxRenderHeight = 1080;

    // 덱 썸네일 스트립의 작은 고정 크기(출력 해상도와 무관 — 16:9).
    private const int PptThumbnailWidth = 200;
    private const int PptThumbnailHeight = 112;

    // 현재 썸네일 스트립이 채워진 덱 파일 경로(같은 덱은 재로드 안 함).
    private string? _thumbnailDeckPath;

    partial void OnSelectedItemChanged(LiveQueueItem? value)
    {
        if (_session.Current.State != LiveState.Active)
        {
            LiveBar.CurrentItemTitle = value?.Title ?? string.Empty;
        }

        // 항목이 바뀌면 가사 페이지를 첫 절로 리셋(PPT 의 SlideNumber 리셋과 대칭).
        RefreshLyricsPages(value);

        // 항목 종류에 맞춰 중앙 미리보기 탭을 자동 전환(수동 탭 전환 제거 — FrmMain식).
        UpdateContentTabForItem(value);

        // 선택 항목의 실제 콘텐츠를 적절한 미리보기 VM 으로 적재(라이브 큐 콘텐츠 plumbing).
        // UI 경로라 fire-and-forget; 테스트는 ApplySelectedItemContentAsync 를 직접 await.
        _ = ApplySelectedItemContentAsync(value);

        NotifyCommandStates();
    }

    // 선택 항목 종류에 맞는 중앙 탭을 고른다 — PPT→PowerPoint(미리보기+썸네일), 미디어→Media, 그 외(곡/성경/공지)→Preview.
    // 해당 탭이 설정상 숨겨져 있으면(UsePowerPointTab/UseMediaTab off) Preview(0)로 폴백해 빈 탭을 선택하지 않는다.
    private void UpdateContentTabForItem(LiveQueueItem? item)
    {
        if (item is not null && IsPowerPointItem(item) && IsPowerPointTabVisible)
        {
            SelectedContentTabIndex = 1;
        }
        else if (item is not null && IsMediaItem(item) && IsMediaTabVisible)
        {
            SelectedContentTabIndex = 2;
        }
        else
        {
            SelectedContentTabIndex = 0;
        }
    }

    // 선택된 항목의 가사/구절 총 절 수를 갱신하고 현재 페이지를 첫 절로 리셋.
    // 곡·성경이 아니거나 본문이 없으면 LyricsPageCount=0(절 이동 버튼 비활성).
    private void RefreshLyricsPages(LiveQueueItem? item)
    {
        // 절 단위로 페이지네이션되는 항목 = 곡 + 성경(구절 본문). 둘 다 같은 가사 페이지 모델(절 이동·위치 라벨)을 쓴다.
        var paginated = IsLyricsPaginated(item);
        // 성경 본문은 인용부호 »…« 가 코드 마커로 오인돼 절 수가 어긋나지 않도록 분할 전 보호한다(본문 추출과 동일 규칙).
        var lyrics = paginated ? GuardBibleNotation(item!) : null;
        // 이중 언어([region 2]) 곡·성경은 영역-인식 페이지 수(GetRegionPages)를 쓴다 — [region 2] 가 절 경계로
        // 오인돼 절 수가 부풀던 문제 해소. 단일 영역은 기존 ToVersePages(Sequence 적용) 경로 그대로(무회귀).
        var dual = paginated && LyricsDisplayFormatter.HasRegion2(lyrics);
        LyricsPageCount = !paginated
            ? 0
            : dual
                ? LyricsDisplayFormatter.GetRegionPages(lyrics, item!.Sequence).Count
                : LyricsDisplayFormatter.ToVersePages(lyrics, item!.Sequence).Count;
        // 페이지별 절 라벨(절 점프 근거). 단일 영역은 GetSectionLabels, 이중 언어는 region-aware 라벨을 쓰되
        // 페이지 수와 1:1 정렬될 때만 채운다(라벨 없는 머리말 등으로 어긋나면 비워 점프 비활성 — 잘못된 점프 방지).
        // 성경 본문엔 [라벨] 마커가 없어 라벨 목록은 비고(절 점프 바 없음), 이전/다음 절 이동만 동작한다.
        if (!paginated)
        {
            _pageLabels = Array.Empty<string>();
        }
        else if (!dual)
        {
            _pageLabels = LyricsDisplayFormatter.GetSectionLabels(lyrics, item!.Sequence);
        }
        else
        {
            var regionLabels = LyricsDisplayFormatter.GetRegionSectionLabels(lyrics, item!.Sequence);
            _pageLabels = regionLabels.Count == LyricsPageCount ? regionLabels : Array.Empty<string>();
        }
        RebuildAvailableSectionLabels();
        LyricsPageIndex = 0;
    }

    // 절 수·본문 계산 전 항목 본문을 정리한다. 성경은 코드 마커(») 규약을 안 쓰므로 인용부호 »…« 를 보호 문자로
    // 임시 치환해 절 경계·본문이 잘리지 않게 한다. 곡 등 다른 항목은 원본 그대로(무회귀).
    private static string? GuardBibleNotation(LiveQueueItem item)
        => item.Kind == LiveItemKinds.Bible
            ? LyricsDisplayFormatter.GuardLiteralNotation(item.Lyrics)
            : item.Lyrics;

    // 절 단위로 페이지네이션되는(가사/구절 본문이 있는) 항목인지 — 곡과 성경. 본문 계산·절 이동·위치 라벨의 공통 판정.
    private static bool IsLyricsPaginated(LiveQueueItem? item)
        => item is not null
            && (item.Kind == LiveItemKinds.Song || item.Kind == LiveItemKinds.Bible)
            && !string.IsNullOrEmpty(item.Lyrics);

    // 페이지 라벨에서 중복을 제거(첫 등장 순서)해 점프 버튼 목록을 만든다.
    private void RebuildAvailableSectionLabels()
    {
        AvailableSectionLabels.Clear();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var label in _pageLabels)
        {
            if (!string.IsNullOrEmpty(label) && seen.Add(label))
            {
                AvailableSectionLabels.Add(label);
            }
        }

        JumpToLyricsSectionCommand.NotifyCanExecuteChanged();
    }

    // 절 라벨로 직접 점프 — 그 라벨의 첫 페이지로 이동(레거시 FrmInfoScreen 절 버튼). 라이브 중이면 출력도 즉시 갱신.
    private void JumpToLyricsSection(string? label)
    {
        var index = IndexOfPageLabel(label);
        if (index < 0)
        {
            return; // 없는 라벨은 무시(이동 없음).
        }

        LyricsPageIndex = index;
        PublishLyricsPageIfLive();
        StatusText = LyricsPageCount > 1
            ? $"가사 {LyricsPageIndex + 1}/{LyricsPageCount}절"
            : StatusText;
    }

    // 점프 가능 여부 — 비어 있지 않은 라벨이 현재 곡의 페이지 라벨에 존재할 때만.
    private bool CanJumpToLyricsSection(string? label) => IndexOfPageLabel(label) >= 0;

    // 라벨의 첫 페이지 인덱스(대소문자 무시). 없으면 -1. 점프/CanExecute 공통.
    private int IndexOfPageLabel(string? label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return -1;
        }

        for (var i = 0; i < _pageLabels.Count; i++)
        {
            if (string.Equals(_pageLabels[i], label, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    // 다음 절로 이동 — 라이브 중이고 이 항목이 송출 중일 때만 출력도 즉시 갱신한다.
    private void NextLyricsPage()
    {
        LyricsPageIndex++;
        PublishLyricsPageIfLive();
        StatusText = LyricsPageCount > 1
            ? $"가사 {LyricsPageIndex + 1}/{LyricsPageCount}절"
            : StatusText;
    }

    private bool CanGoNextLyricsPage()
        => LyricsPageCount > 1 && LyricsPageIndex < LyricsPageCount - 1;

    // 이전 절로 이동.
    private void PreviousLyricsPage()
    {
        LyricsPageIndex--;
        PublishLyricsPageIfLive();
        StatusText = LyricsPageCount > 1
            ? $"가사 {LyricsPageIndex + 1}/{LyricsPageCount}절"
            : StatusText;
    }

    private bool CanGoPreviousLyricsPage()
        => LyricsPageCount > 1 && LyricsPageIndex > 0;

    // 자동 회전 토글(레거시 Auto Rotate) — 라이브 중 절/슬라이드를 일정 간격으로 자동 전환.
    // 실제 타이머는 View(코드비하인드)가 IsAutoRotating 변화를 보고 구동하고, 매 tick 에 AdvanceAutoRotation 을 부른다.
    private void ToggleAutoRotate()
    {
        IsAutoRotating = !IsAutoRotating;
        StatusText = IsAutoRotating ? $"자동 회전 켬 ({AutoRotateIntervalSeconds}초)" : "자동 회전 끔";
    }

    /// <summary>
    /// 자동 회전 한 스텝 — 라이브 곡은 다음 절(끝에서 첫 절로 순환), PPT 덱은 다음 슬라이드(순환).
    /// View 타이머가 호출. 라이브가 아니거나 선택≠라이브 항목이면 아무것도 하지 않는다.
    /// </summary>
    public void AdvanceAutoRotation()
    {
        if (_session.Current.State != LiveState.Active) return;
        if (SelectedItem is not { } item || _liveItemId != item.Id) return;

        if (IsPowerPointItem(item) && PowerPoint.SlideCount > 1)
        {
            // 마지막 슬라이드 다음은 첫 슬라이드로 순환.
            var next = PowerPoint.SlideNumber >= PowerPoint.SlideCount ? 1 : PowerPoint.SlideNumber + 1;
            _ = GoToSlideAsync(next);
        }
        else if (IsLyricsPaginated(item) && LyricsPageCount > 1)
        {
            // 곡·성경 모두 절 단위로 회전 — 마지막 절 다음은 첫 절로 순환.
            LyricsPageIndex = LyricsPageIndex + 1 >= LyricsPageCount ? 0 : LyricsPageIndex + 1;
            PublishLyricsPageIfLive();
        }
    }

    // 현재 절 인덱스로 GoLive 를 재호출해 출력을 갱신한다.
    // 라이브 활성 + 이 항목이 송출 중일 때만 실행(블랙아웃/숨김 중에는 송출 안 깨움).
    private void PublishLyricsPageIfLive()
    {
        if (SelectedItem is null) return;
        if (_session.Current.State != LiveState.Active) return;
        if (_liveItemId != SelectedItem.Id) return;

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        // ResolveLiveProjection 경유로 PreviewSource·PositionLabel 등 라이브 장식을 일관되게 얹는다.
        var projection = ResolveLiveProjection(SelectedItem with { LyricsPageIndex = LyricsPageIndex });
        _session.GoLive(projection, monitorName);
    }

    /// <summary>
    /// 선택된 큐 항목의 종류에 따라 콘텐츠를 적재한다:
    ///  - PowerPoint 항목 → 썸네일 렌더(PowerPoint.LoadAsync), 그 외 → PPT 미리보기 비움.
    ///  - Media 항목 → 미디어 재생 VM 에 Load(MediaPlaybackRequest).
    /// 곡 가사는 항목(LiveQueueItem.Lyrics)이 직접 들고 있어 바인딩으로 표시되므로 여기서 추가 적재 불필요.
    /// </summary>
    public async Task ApplySelectedItemContentAsync(LiveQueueItem? item)
    {
        // fire-and-forget(OnSelectedItemChanged)로 호출되므로 예외가 새면 unobserved 가 된다.
        // 안전 불변식을 호출 메서드 안에서 봉인.
        try
        {
            if (item is { Kind: LiveItemKinds.PowerPoint, ContentPath: { Length: > 0 } pptPath })
            {
                var slide = item.SlideNumber <= 0 ? 1 : item.SlideNumber;
                var (renderWidth, renderHeight) = ResolvePptRenderSize();
                await PowerPoint.LoadAsync(pptPath, slide, renderWidth, renderHeight).ConfigureAwait(true);

                // 덱이 바뀌었으면 썸네일 스트립을 백그라운드로 채운다(같은 덱 슬라이드 이동/재렌더 시엔 재로드 안 함).
                if (PowerPoint.State == Rendering.PowerPointPreviewState.Ready
                    && !string.Equals(_thumbnailDeckPath, pptPath, StringComparison.OrdinalIgnoreCase))
                {
                    _thumbnailDeckPath = pptPath;
                    _ = PowerPoint.LoadThumbnailsAsync(pptPath, PowerPoint.SlideCount, PptThumbnailWidth, PptThumbnailHeight);
                }
            }
            else
            {
                PowerPoint.Clear();
                _thumbnailDeckPath = null;
            }

            if (item is { Kind: LiveItemKinds.Media, ContentPath: { Length: > 0 } mediaPath })
            {
                Media.Load(new MediaPlaybackRequest(mediaPath, MediaSourceKind.File, TimeSpan.Zero, InferMediaType(mediaPath)));
            }
            else
            {
                // 다른 종류 항목으로 넘어가면 직전 미디어를 완전히 내린다 — PPT.Clear 와 대칭.
                // Stop(정지 후 첫 프레임 잔류)이 아니라 Unload 라야 출력 창에서 영상이 사라지고
                // 그 아래 가사/타이틀이 다시 보인다(출력 패리티). 이미 비어 있으면 내부에서 무시.
                Media.Unload();
            }
        }
        catch (Exception ex)
        {
            StatusText = $"항목 콘텐츠 로드 실패: {ex.Message}";
        }
    }

    private void OnOutputChanged(object? sender, OutputWindowChangedEventArgs e)
    {
        NotifyCommandStates();

        // 출력 창이 열리면 현재 선택된 PPT 를 그 해상도로 다시 렌더한다 — 항목을 먼저 고르고
        // 출력을 나중에 여는 흐름에서도 송출이 선명하도록(직전 렌더가 미리보기 크기로 남는 문제 해소).
        // 닫힘은 송출하지 않으므로 갱신 불필요(IsOpen 가드). 같은 해상도 갱신은 렌더 캐시가 흡수.
        // (현재 실효 트리거는 Open 뿐 — MoveTo 는 아직 미배선이나, 추가돼도 같은 가드로 안전.)
        // 가드는 본문과 동일하게 정규값 Kind 로 매치한다(ApplySelectedItemContentAsync 가 정규값만
        // 렌더하고 그 외는 PowerPoint.Clear 하므로, 별칭 항목까지 재실행하면 미리보기가 비워짐).
        if (_output.Current.IsOpen && SelectedItem is { Kind: LiveItemKinds.PowerPoint })
        {
            _ = ApplySelectedItemContentAsync(SelectedItem);
        }
    }

    // PPT 슬라이드 렌더 크기 결정 — 출력 창이 열려 있으면 출력 모니터 해상도(1080p 상한)로 렌더해
    // GoLive 송출 시 선명하게, 닫혀 있으면 가벼운 미리보기 크기로 렌더한다.
    private (int Width, int Height) ResolvePptRenderSize()
    {
        var current = _output.Current;
        if (current.IsOpen && current.Display is { } display)
        {
            var width = (int)Math.Round(display.Width);
            var height = (int)Math.Round(display.Height);
            if (width >= 1 && height >= 1)
            {
                // 1080p 상한 — 단, 두 축을 따로 자르면 비-16:9(16:10·울트라와이드) 출력에서
                // 종횡비가 틀어져 선명도가 되레 나빠진다. 그래서 한 축 기준이 아니라 더 빡빡한 쪽
                // 비율로 두 축을 함께 축소해 종횡비를 보존한다(상한보다 작으면 그대로 유지).
                var scale = Math.Min(
                    (double)PptMaxRenderWidth / width,
                    (double)PptMaxRenderHeight / height);
                if (scale < 1.0)
                {
                    width = Math.Max(1, (int)Math.Round(width * scale));
                    height = Math.Max(1, (int)Math.Round(height * scale));
                }

                return (width, height);
            }
        }

        return (PptPreviewWidth, PptPreviewHeight);
    }

    // 라이브 슬라이드 이동 — 현재 선택된 PPT 의 미리보기를 지정 슬라이드로 다시 렌더하고,
    // 그 항목이 라이브 송출 중이면 출력도 그 슬라이드로 즉시 갱신한다(재-GoLive 의식 없이).
    // 이전/다음 버튼과 썸네일 클릭이 모두 이 한 경로를 쓴다.
    private async Task GoToSlideAsync(int target)
    {
        if (SelectedItem is not { Kind: LiveItemKinds.PowerPoint, ContentPath: { Length: > 0 } path } item)
        {
            return;
        }

        if (PowerPoint.State != Rendering.PowerPointPreviewState.Ready
            || target < 1 || target > PowerPoint.SlideCount
            || target == PowerPoint.SlideNumber)
        {
            return;
        }

        var (width, height) = ResolvePptRenderSize();
        await PowerPoint.LoadAsync(path, target, width, height).ConfigureAwait(true);

        // 라이브 송출 중이고 이 항목이 송출 항목이면 출력도 새 슬라이드로 갱신.
        // 블랙아웃/숨김(Hidden)에선 송출을 깨우지 않도록 Active 일 때만 갱신한다.
        if (_session.Current.State == LiveState.Active && _liveItemId == item.Id)
        {
            var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
            _session.GoLive(ResolveLiveProjection(item with { SlideNumber = target }), monitorName);
        }

        NotifyCommandStates();
    }

    private bool CanGoToSlide(int target)
        => IsPowerPointSlideNavReady() && target >= 1 && target <= PowerPoint.SlideCount;

    private bool CanGoNextSlide()
        => IsPowerPointSlideNavReady() && PowerPoint.SlideNumber < PowerPoint.SlideCount;

    private bool CanGoPreviousSlide()
        => IsPowerPointSlideNavReady() && PowerPoint.SlideNumber > 1;

    private bool IsPowerPointSlideNavReady()
        => SelectedItem is { Kind: LiveItemKinds.PowerPoint } selected
            && PowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && PowerPoint.SlideCount > 0
            // 라이브 중이면 "선택 == 라이브 항목"일 때만 이동 허용 — 선택이 라이브 덱에서 벗어났는데
            // 버튼만 활성이면(아무 효과 없이 다른 덱 미리보기만 넘김) 혼란하므로 비활성한다.
            && (_liveItemId is null || _liveItemId == selected.Id);

    // PPT 미리보기 VM 의 상태/슬라이드 변화에 슬라이드 이동 커맨드 활성 상태를 동기화.
    private void OnPowerPointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PowerPoint.State)
            or nameof(PowerPoint.SlideNumber)
            or nameof(PowerPoint.SlideCount))
        {
            NotifyCommandStates();
        }
    }

    /// <summary>확장자로 오디오/비디오를 추정(미디어 요청 MediaType).</summary>
    private static string InferMediaType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".mp3" or ".wav" or ".wma" or ".m4a" or ".aac" or ".flac" or ".ogg"
            ? "Audio"
            : "Video";
    }

    private void OpenOutput()
    {
        var display = SelectedOutputDisplay ?? GetPreferredOutputDisplay(null);
        _output.Open(display, windowed: true);
        SelectedOutputDisplay = display;
        LiveBar.OutputMonitorName = _output.Current.Display?.Name ?? string.Empty;
        StatusText = $"출력 창 열림: {LiveBar.OutputMonitorName}";
        _telemetry.Record(MainCommandIds.OutputOpen, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    private OutputDisplay GetPreferredOutputDisplay(string? preferredId, IReadOnlyList<OutputDisplay>? displays = null)
    {
        var availableDisplays = displays ?? _display.GetDisplays();
        if (!string.IsNullOrWhiteSpace(preferredId))
        {
            var preferred = availableDisplays.FirstOrDefault(display =>
                string.Equals(display.Id, preferredId, StringComparison.OrdinalIgnoreCase));
            if (preferred is not null)
            {
                return preferred;
            }
        }

        if (_settings.Get(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor))
        {
            return availableDisplays.FirstOrDefault(display => !display.IsPrimary)
                ?? availableDisplays.FirstOrDefault(display => display.IsPrimary)
                ?? _display.GetPrimaryDisplay();
        }

        return availableDisplays.FirstOrDefault(display => display.IsPrimary)
            ?? availableDisplays.FirstOrDefault()
            ?? _display.GetPrimaryDisplay();
    }

    private async Task CloseOutputAsync()
    {
        if (_session.Current.State != LiveState.Off)
        {
            var ok = await ConfirmLiveSafetyAsync(
                MainCommandIds.OutputClose,
                "라이브 중 출력 창을 닫을까요?",
                "현재 송출이 중지되고 출력 창이 닫힙니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
            if (!ok)
            {
                return;
            }
        }

        _output.Close();
        if (_session.Current.State != LiveState.Off)
        {
            _session.Stop();
        }

        _liveItemId = null;
        LiveBar.OutputMonitorName = string.Empty;
        StatusText = "출력 창 닫힘";
        _telemetry.Record(MainCommandIds.OutputClose, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    private bool CanGoLive()
        => SelectedItem is not null && _output.Current.IsOpen && !HasPowerPointLimitViolation;

    private async Task GoLiveAsync()
    {
        if (SelectedItem is null)
        {
            _telemetry.Record(MainCommandIds.LiveGo, succeeded: false, "선택 항목 없음");
            return;
        }

        var ok = await ConfirmLiveSafetyAsync(
            MainCommandIds.LiveGo,
            $"'{SelectedItem.Title}' 항목을 라이브로 송출할까요?",
            "선택 항목이 즉시 출력 화면에 표시됩니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            return;
        }

        PublishSelectedItem();
    }

    private async Task StopLiveAsync()
    {
        var ok = await ConfirmLiveSafetyAsync(
            MainCommandIds.LiveStop,
            "현재 라이브 송출을 중지할까요?",
            "출력 화면이 대기 상태로 돌아갑니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            return;
        }

        _session.Stop();
        _liveItemId = null;
        StatusText = "라이브 중지";
        _telemetry.Record(MainCommandIds.LiveStop, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    private bool CanMoveNext()
    {
        if (SelectedItem is null) return false;
        var index = Queue.IndexOf(SelectedItem);
        return index >= 0 && index < Queue.Count - 1;
    }

    private bool CanMovePrevious()
    {
        if (SelectedItem is null) return false;
        return Queue.IndexOf(SelectedItem) > 0;
    }

    private void NextItem()
    {
        if (!CanMoveNext() || SelectedItem is null)
        {
            return;
        }

        SelectedItem = Queue[Queue.IndexOf(SelectedItem) + 1];
        _telemetry.Record(MainCommandIds.LiveNext, succeeded: true, SelectedItem.Title);
        if (_session.Current.State == LiveState.Active)
        {
            PublishSelectedItem();
        }
    }

    private void PreviousItem()
    {
        if (!CanMovePrevious() || SelectedItem is null)
        {
            return;
        }

        SelectedItem = Queue[Queue.IndexOf(SelectedItem) - 1];
        _telemetry.Record(MainCommandIds.LivePrevious, succeeded: true, SelectedItem.Title);
        if (_session.Current.State == LiveState.Active)
        {
            PublishSelectedItem();
        }
    }

    // 예배 순서의 첫 항목으로 이동(레거시 First). 이미 첫 항목이거나 큐가 비면 아무것도 안 한다. 라이브면 송출.
    private void FirstItem()
    {
        if (!CanMovePrevious() || Queue.Count == 0)
        {
            return;
        }

        SelectedItem = Queue[0];
        _telemetry.Record(MainCommandIds.LiveFirst, succeeded: true, SelectedItem!.Title);
        if (_session.Current.State == LiveState.Active)
        {
            PublishSelectedItem();
        }
    }

    // 예배 순서의 마지막 항목으로 이동(레거시 Last). 이미 마지막이거나 큐가 비면 아무것도 안 한다. 라이브면 송출.
    private void LastItem()
    {
        if (!CanMoveNext() || Queue.Count == 0)
        {
            return;
        }

        SelectedItem = Queue[Queue.Count - 1];
        _telemetry.Record(MainCommandIds.LiveLast, succeeded: true, SelectedItem!.Title);
        if (_session.Current.State == LiveState.Active)
        {
            PublishSelectedItem();
        }
    }

    private bool CanUseLiveSafetyAction() => _session.Current.State is LiveState.Active or LiveState.Hidden;

    private async Task HideOutputAsync(bool blackout)
    {
        var actionName = blackout ? MainCommandIds.LiveBlack : MainCommandIds.LiveHide;
        var ok = await ConfirmLiveSafetyAsync(
            actionName,
            blackout ? "현재 송출을 검은 화면으로 전환할까요?" : "현재 송출을 숨길까요?",
            "라이브 출력 상태가 즉시 바뀝니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            return;
        }

        _session.HideOutput(blackout);
        StatusText = blackout ? "검은 화면 송출 중" : "출력 숨김";
        _telemetry.Record(actionName, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    // 화면 비우기(레거시 Clear Screen) — 콘텐츠를 감추되 배경은 유지(완전 검정인 Black 과 구별).
    // Hide/Black 과 동일하게 라이브 안전 확인을 거친다(회중 화면이 즉시 바뀌므로).
    private async Task ClearOutputAsync()
    {
        var ok = await ConfirmLiveSafetyAsync(
            MainCommandIds.LiveClear,
            "현재 송출 화면을 비울까요?",
            "가사·콘텐츠가 사라지고 배경만 남습니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            return;
        }

        _session.ClearOutput();
        StatusText = "화면 비움(배경 유지)";
        _telemetry.Record(MainCommandIds.LiveClear, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    // 숨김/블랙/비우기에서 송출 화면 복귀 — 직전 항목을 그대로 다시 보인다(저위험이라 안전 확인 없음).
    private void RestoreOutput()
    {
        _session.Restore();
        StatusText = "출력 복귀";
        NotifyCommandStates();
    }

    // 현재 항목 처음으로(레거시 Restart Current Item) — 라이브 곡은 첫 절로, PPT 덱은 첫 슬라이드로 되돌려 재송출.
    // 슬라이드/절 이동과 같은 가드(선택 == 라이브 항목)를 써서 라이브 항목에만 적용한다.
    //
    // 안전 확인 정책: Restart 는 안전 확인(5초)을 두지 않는다 — Clear/Hide/Black(화면을 가리는 행위)과 달리
    // "처음으로"는 다음/이전 절·슬라이드 이동과 같은 *네비게이션* 계열이고(되돌릴 수 있음: 다시 넘기면 됨),
    // 라이브 글리치 복구처럼 빠른 조작이 중요하므로 즉시 적용한다(Next/Prev 가 무확인인 것과 일관).
    private async Task RestartCurrentItemAsync()
    {
        if (!CanRestartCurrentItem() || SelectedItem is not { } item)
        {
            return;
        }

        if (IsPowerPointItem(item))
        {
            // PPT 덱: 첫 슬라이드로. 이미 1번이면 GoToSlideAsync 가 무시하므로(target==현재) Refresh 로 강제 재렌더.
            if (PowerPoint.SlideNumber <= 1)
            {
                _session.Refresh();
            }
            else
            {
                await GoToSlideAsync(1).ConfigureAwait(true);
            }
        }
        else
        {
            // 곡: 첫 절로 되돌려 출력 재송출.
            LyricsPageIndex = 0;
            PublishLyricsPageIfLive();
        }

        StatusText = $"처음으로: {item.Title}";
        _telemetry.Record(MainCommandIds.LiveRestart, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    // 라이브 활성 + 선택이 곧 라이브 항목일 때만 — 슬라이드/절 이동 가드와 동일 철학(엉뚱한 항목 재시작 방지).
    private bool CanRestartCurrentItem()
        => _session.Current.State == LiveState.Active
           && SelectedItem is not null
           && _liveItemId == SelectedItem.Id;

    // 출력 새로고침(레거시 Refresh Output) — 현재 세션 스냅샷으로 출력 창을 강제 재렌더한다.
    private void RefreshOutput()
    {
        _session.Refresh();
        StatusText = "출력 새로고침";
        _telemetry.Record(MainCommandIds.LiveRefresh, succeeded: true, StatusText);
        // 사이드이펙트(ApplyLiveSnapshot)에 기대지 않고 명시적으로 커맨드 상태를 갱신(리뷰 #1).
        NotifyCommandStates();
    }

    private async Task<bool> ConfirmLiveSafetyAsync(string actionName, string question, string subtext)
    {
        var ok = await _safetyPrompt.ConfirmAsync(new LiveSafetyRequest(
            actionName,
            question,
            subtext,
            TimeSpan.FromSeconds(5))).ConfigureAwait(true);

        if (ok)
        {
            return true;
        }

        _telemetry.Record(actionName, succeeded: false, "사용자 취소");
        StatusText = "라이브 안전 확인 취소";
        NotifyCommandStates();
        return false;
    }

    private void PublishSelectedItem()
    {
        if (SelectedItem is null)
        {
            _telemetry.Record(MainCommandIds.LiveGo, succeeded: false, "선택 항목 없음");
            return;
        }

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        _liveItemId = SelectedItem.Id; // 라이브 항목 기록(슬라이드 이동이 출력을 갱신할지 판별)
        // 새 곡을 송출하면 조옮김을 원조(0)로 초기화 — 각 곡이 작성된 키에서 시작하도록(절·슬라이드 이동은 유지).
        LiveTransposeSemitones = 0;
        // 가사 항목이면 현재 절 인덱스를 투영에 얹는다(절 단위 페이지네이션 — PR B).
        var projection = SelectedItem with { LyricsPageIndex = LyricsPageIndex };
        _session.GoLive(ResolveLiveProjection(projection), monitorName);
        StatusText = $"LIVE: {SelectedItem.Title}";
        _telemetry.Record(MainCommandIds.LiveGo, succeeded: true, StatusText);
        AdvanceSelectionAfterPublish(SelectedItem);
        NotifyCommandStates();
    }

    // 출력 송출 항목 결정 — PPT 항목이고 슬라이드 렌더가 준비됐으면, 미리보기 탭에만 있던
    // 렌더 이미지를 출력 창에도 송출하도록 PreviewSource 에 실은 복사본을 만든다(G1.2 출력 송출).
    // LiveQueueItem 은 불변(init) 이므로 큐를 건드리지 않고 with 로 전이 복사본만 만든다.
    // (복사본은 큐에 넣지 않는다 — IndexOf/자동 다음 항목은 원본 SelectedItem 기준이어야 함.)
    //
    // 신원 가드: 렌더는 항목 선택 시 fire-and-forget 로 돌아가므로, 빠른 전환 경쟁에서 PreviewImage 가
    // "다른 항목"의 stale 슬라이드일 수 있다. 그래서 단순히 Ready 인지가 아니라, VM 이 마지막으로 성공
    // 렌더한 파일이 송출 항목의 파일과 일치할 때만 슬라이드를 싣는다(불일치/미준비면 타이틀만 — 안전 강등).
    // 슬라이드 번호는 PPT VM 의 현재 렌더 슬라이드(PowerPoint.SlideNumber)를 "단일 진실"로 신뢰하고
    // 송출 항목에도 반영한다 — 라이브 슬라이드 이동으로 item.SlideNumber 와 실제 렌더 슬라이드가 달라져도
    // (이동한 슬라이드가 그대로 송출되고, 재개·재송출 시에도 일관). 파일이 다르면(cross-item stale) 거른다.
    private LiveQueueItem ResolveLiveProjection(LiveQueueItem item)
    {
        var positionLabel = ComputePositionLabel(item);
        var nextTitle = ComputeNextTitle(item);
        // "코드 표시"(Show Notations) 설정을 투영에 얹는다 — 라이브 본문 계산(ComputeBodyText)이 이 값으로
        // 가사 위 코드 줄을 끼울지 판단한다. 모든 곡 송출 경로가 ResolveLiveProjection 을 거치므로 한 곳에서 일관 적용.
        var showNotations = _settings.Get(EasiSettingKeys.LyricsMonitorShowNotations);

        if (IsPowerPointItem(item)
            && PowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && PowerPoint.PreviewImage is not null
            && !string.IsNullOrEmpty(item.ContentPath)
            && string.Equals(PowerPoint.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            return item with
            {
                PreviewSource = PowerPoint.PreviewImage,
                PreviewFillMode = Rendering.ImageFillMode.Fit,
                SlideNumber = PowerPoint.SlideNumber,
                PositionLabel = positionLabel,
                NextTitle = nextTitle,
                ShowNotations = showNotations,
                TransposeSemitones = LiveTransposeSemitones,
            };
        }

        return item with
        {
            PositionLabel = positionLabel,
            NextTitle = nextTitle,
            ShowNotations = showNotations,
            TransposeSemitones = LiveTransposeSemitones,
        };
    }

    // 다음 예배순서 항목 제목(출력 "다음 항목 표시" Display Panel PrevNext).
    // 큐에서 현재 항목(Id 기준)의 바로 다음 항목 제목을 찾는다. 마지막이거나 없으면 빈 문자열 → 출력 미표시.
    // (item 은 with 복사본이라 참조가 다를 수 있어 Id 로 매칭한다. 같은 Id 가 큐에 중복되면 첫 일치의
    //  다음 항목을 쓴다 — ComputePositionLabel 등 기존 큐 탐색과 동일 가정.)
    private string ComputeNextTitle(LiveQueueItem item)
    {
        for (var i = 0; i < Queue.Count - 1; i++)
        {
            if (string.Equals(Queue[i].Id, item.Id, StringComparison.Ordinal))
            {
                return Queue[i + 1].Title;
            }
        }

        return string.Empty;
    }

    // 위치 라벨 계산(절/슬라이드 "N/M") — 곡=현재 절/총 절, PPT=현재 슬라이드/총 슬라이드.
    // 단일(총 1)이면 빈 문자열(표시 의미 없음). 출력은 설정 on 일 때만 노출.
    private string ComputePositionLabel(LiveQueueItem item)
    {
        if (IsPowerPointItem(item))
        {
            var count = PowerPoint.SlideCount;
            var current = PowerPoint.SlideNumber;
            return count > 1 && current >= 1 ? $"{current}/{count}" : string.Empty;
        }

        if (IsLyricsPaginated(item))
        {
            // 이중 언어(성경/곡)는 영역-인식 페이지 수를 써 RefreshLyricsPages 의 LyricsPageCount 와 어긋나지 않게 한다.
            // 성경 본문은 동일하게 인용부호를 보호한 뒤 센다(LyricsPageCount 와 같은 입력 → 위치 라벨 N/M 일치).
            var lyrics = GuardBibleNotation(item);
            var count = LyricsDisplayFormatter.HasRegion2(lyrics)
                ? LyricsDisplayFormatter.GetRegionPages(lyrics, item.Sequence).Count
                : LyricsDisplayFormatter.ToVersePages(lyrics, item.Sequence).Count;
            return count > 1 ? $"{item.LyricsPageIndex + 1}/{count}" : string.Empty;
        }

        return string.Empty;
    }

    private void AdvanceSelectionAfterPublish(LiveQueueItem publishedItem)
    {
        if (!_settings.Get(EasiSettingKeys.AdvanceNextItem))
        {
            return;
        }

        // PPT 덱은 자동 다음-항목 이동에서 제외 — 다중 슬라이드 덱은 다음 항목으로 넘어가지 말고
        // 그 자리에서 슬라이드를 이동해야 한다(자동 advance 는 곡·공지 같은 단발 항목용).
        // 또 선택이 라이브 PPT 에 머물러야 라이브 슬라이드 이동 커맨드가 활성으로 유지된다.
        if (IsPowerPointItem(publishedItem))
        {
            return;
        }

        var index = Queue.IndexOf(publishedItem);
        if (index >= 0 && index < Queue.Count - 1)
        {
            SelectedItem = Queue[index + 1];
            LiveBar.CurrentItemTitle = _session.Current.CurrentItemTitle;
        }
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs args)
    {
        if (ContainsOperationalSetting(args.ChangedKeys))
        {
            ApplyOperationalSettings(updateStatus: true);
        }

        // "코드 표시"가 바뀌면 본문 자체(코드 줄 포함 여부)가 달라지므로, 색·정렬처럼 렌더 계층에서
        // 즉시 반영되지 않는다. 현재 라이브 곡을 같은 절로 다시 송출해 본문을 재계산한다(라이브 즉시 반영).
        if (ContainsKey(args.ChangedKeys, EasiSettingKeys.LyricsMonitorShowNotations.Id))
        {
            RepublishLiveSongForBodyChange();
        }

        // 출력 모양(색/그라데이션)은 운영 설정이 아니므로 별도로 활성 프리셋 강조를 갱신
        // (Settings 창 등 다른 경로에서 바뀌어도 인스펙터가 따라가도록).
        RefreshActiveAppearance();
    }

    // 본문 재계산이 필요한 설정(코드 표시 등)이 바뀌었을 때, 현재 라이브 큐 곡을 현재 절 그대로 다시 송출한다.
    // 라이브가 아니거나, 라이브 항목이 큐에 없거나(공지 센티넬·고아), 곡이 아니면 아무것도 하지 않는다(안전).
    private void RepublishLiveSongForBodyChange()
    {
        if (_session.Current.State != LiveState.Active)
        {
            return;
        }

        var live = Queue.FirstOrDefault(q => q.Id == _liveItemId);
        if (live is null)
        {
            return; // 공지(센티넬)·고아 Id → 본문 재계산 대상 아님.
        }

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        // 라이브 절 인덱스는 "선택 항목"을 따라가는 VM 의 LyricsPageIndex 가 아니라, 세션 스냅샷의
        // 실제 라이브 절(CurrentLyricsPageIndex)을 쓴다 — 송출 후 선택이 다음 항목으로 자동 이동(AdvanceSelectionAfterPublish)
        // 했을 수 있어, VM 값을 쓰면 엉뚱한 절(보통 0절)로 튀어 라이브 화면이 어긋난다.
        var livePageIndex = _session.Current.CurrentLyricsPageIndex;
        var projection = ResolveLiveProjection(live with { LyricsPageIndex = livePageIndex });
        _session.GoLive(projection, monitorName);
    }

    // ChangedKeys 에 특정 키 Id 가 들어 있는지(대소문자 무시). ContainsOperationalSetting 과 동일 규칙.
    private static bool ContainsKey(IReadOnlyList<string> changedKeys, string keyId)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            if (string.Equals(changedKeys[i], keyId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // 인라인 라이브러리에서 선택한 곡을 예배 순서(큐)에 추가(별도 LibraryWindow 없이). AddSong 재사용.
    private void AddSelectedLibrarySong() => AddSong(Library.SelectedSong);

    private void OnLibraryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Library.SelectedSong))
        {
            AddSelectedLibrarySongCommand.NotifyCanExecuteChanged();
        }
    }

    // 재검색을 하면 결과 목록이 통째로 바뀐다(이전 결과를 지우고 새 결과로 채움).
    // 이때 사라진 옛 결과를 계속 "선택됨"으로 들고 있으면 엉뚱한 곡이 추가될 수 있으니,
    // 더는 목록에 없는 선택은 여기서 직접 비운다. (화면 바인딩에 기대지 않고 VM 이 스스로 정리 → 테스트도 쉬움.)
    // 선택을 null 로 바꾸면 [NotifyCanExecuteChangedFor] 가 "예배 순서에 추가" 버튼도 자동으로 끈다.
    private void OnSearchResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (SelectedSearchResult is not null && !Search.SearchResults.Contains(SelectedSearchResult))
        {
            SelectedSearchResult = null;
        }
    }

    // 제목 후보 목록이 교체되면(재조회) 사라진 선택을 비운다 — OnSearchResultsChanged 와 동일 규칙.
    private void OnLookupCandidatesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (SelectedTitleCandidate is not null && !Search.LookupCandidates.Contains(SelectedTitleCandidate))
        {
            SelectedTitleCandidate = null;
        }
    }

    // 출력 모양 프리셋 적용 — 글자색·배경색·그라데이션 설정을 한 번에 쓴다.
    // 설정이 바뀌면 출력 VM(OutputWindowViewModel)이 SettingsChanged 로 라이브 출력을 즉시 갱신한다.
    private void ApplyOutputAppearance(OutputAppearancePreset? preset)
    {
        if (preset is null)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, preset.TextArgb);
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, preset.Background1Argb);
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, preset.Background2Argb);
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundIsGradient, preset.IsGradient);
        RefreshActiveAppearance();
        StatusText = $"출력 모양: {preset.Name}";
    }

    // 인-셸 가사 가로 정렬 적용 — 설정을 쓰면 출력 VM 이 SettingsChanged 로 라이브 출력을 즉시 갱신한다(§7.3-A).
    private void ApplyLyricsAlignment(LyricsTextAlignment alignment)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorTextAlignment, alignment);
        ActiveLyricsAlignment = alignment;
        StatusText = $"가사 정렬: {alignment switch { LyricsTextAlignment.Left => "왼쪽", LyricsTextAlignment.Right => "오른쪽", _ => "가운데" }}";
    }

    // 배경 이미지 표시 모드 적용 — 설정을 쓰면 출력 VM 이 SettingsChanged 로 배경 브러시를 다시 만들어 즉시 반영(레거시 Def_ImageMode).
    private void ApplyBackgroundMode(LyricsBackgroundMode mode)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundMode, mode);
        ActiveBackgroundMode = mode;
        StatusText = $"배경 표시 모드: {mode switch
        {
            LyricsBackgroundMode.Fit => "맞춤(전체 보임)",
            LyricsBackgroundMode.Center => "가운데(원본 크기)",
            LyricsBackgroundMode.Tile => "타일(반복)",
            _ => "채움(가득)",
        }}";
    }

    // 이중 언어 영역 표시 모드 적용 — 설정→출력 VM 라이브 반영(레거시 Def_ShowRegion1/2/Both).
    private void ApplyRegionDisplay(LyricsRegionDisplay mode)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorRegionDisplay, mode);
        ActiveRegionDisplay = mode;
        StatusText = $"영역 표시: {mode switch
        {
            LyricsRegionDisplay.Region1Only => "Region 1만",
            LyricsRegionDisplay.Region2Only => "Region 2만",
            _ => "둘 다",
        }}";
    }

    // 인-셸 제목 헤딩 가로 정렬 적용 — 가사 정렬과 동일 경로(설정→출력 VM 라이브 반영, §7.3-A Heading Align).
    private void ApplyTitleHeadingAlignment(LyricsTextAlignment alignment)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment, alignment);
        ActiveTitleHeadingAlignment = alignment;
        StatusText = $"제목 정렬: {alignment switch { LyricsTextAlignment.Left => "왼쪽", LyricsTextAlignment.Right => "오른쪽", _ => "가운데" }}";
    }

    // 인-셸 가사 세로 정렬 적용 — 가로 정렬과 동일 경로(설정→출력 VM 라이브 반영).
    private void ApplyLyricsVerticalAlignment(LyricsVerticalAlignment alignment)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorVerticalAlignment, alignment);
        ActiveLyricsVerticalAlignment = alignment;
        StatusText = $"가사 세로 정렬: {alignment switch { LyricsVerticalAlignment.Top => "위", LyricsVerticalAlignment.Bottom => "아래", _ => "가운데" }}";
    }

    // ─── 출력 모양 설정 템플릿(저장/불러오기) §7.3-A ──────────────────────────

    // SelectedAppearanceTemplate 변경 시 적용/삭제 커맨드 활성 상태를 맞춘다([ObservableProperty] 부분 훅).
    partial void OnSelectedAppearanceTemplateChanged(string? value)
    {
        ApplyAppearanceTemplateCommand.NotifyCanExecuteChanged();
        DeleteAppearanceTemplateCommand.NotifyCanExecuteChanged();
    }

    // 현재 출력 모양 전체를 이름으로 저장. 이름이 비면 무시. 저장 후 목록 갱신·선택.
    private async Task SaveAppearanceTemplateAsync()
    {
        var name = NewAppearanceTemplateName?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            StatusText = "저장할 템플릿 이름을 입력하세요.";
            return;
        }

        // 같은 이름이면 덮어쓰기(File.Move overwrite) — 사용자가 실수로 프리셋을 잃지 않도록 상태바에 명시.
        var overwrote = AppearanceTemplateNames.Contains(name);
        try
        {
            await _appearanceTemplates.SaveAsync(name, LyricsAppearanceTemplate.Capture(_settings)).ConfigureAwait(true);
            RefreshAppearanceTemplateNames();
            SelectedAppearanceTemplate = name;
            NewAppearanceTemplateName = "";
            StatusText = overwrote ? $"출력 모양 템플릿 저장(덮어씀): {name}" : $"출력 모양 템플릿 저장: {name}";
        }
        catch (ArgumentException)
        {
            StatusText = "템플릿 이름에 사용할 수 없는 문자/형식입니다(100자 이내, 예약명·경로 문자 제외).";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusText = $"템플릿 저장 실패: {ex.Message}";
        }
    }

    // 선택된 템플릿을 불러와 출력 모양 설정에 되적용(설정→출력 VM 라이브 반영). 인스펙터 활성 상태도 동기화.
    private async Task ApplyAppearanceTemplateAsync()
    {
        var name = SelectedAppearanceTemplate?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        try
        {
            var template = await _appearanceTemplates.LoadAsync(name).ConfigureAwait(true);
            if (template is null)
            {
                StatusText = $"템플릿을 찾을 수 없습니다: {name}";
                RefreshAppearanceTemplateNames();
                return;
            }

            template.ApplyTo(_settings);
            RefreshActiveAppearance(); // 인스펙터 표시(색·정렬·크기·효과 등) 동기화
            StatusText = $"출력 모양 템플릿 적용: {name}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"템플릿 적용 실패: {ex.Message}";
        }
    }

    private void DeleteAppearanceTemplate()
    {
        var name = SelectedAppearanceTemplate?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        _appearanceTemplates.Delete(name);
        RefreshAppearanceTemplateNames();
        SelectedAppearanceTemplate = null;
        StatusText = $"출력 모양 템플릿 삭제: {name}";
    }

    private void RefreshAppearanceTemplateNames()
    {
        var selected = SelectedAppearanceTemplate;
        AppearanceTemplateNames.Clear();
        foreach (var n in _appearanceTemplates.ListNames())
        {
            AppearanceTemplateNames.Add(n);
        }

        // 선택이 여전히 유효하면 유지(목록 새로고침으로 선택이 풀리지 않도록).
        if (selected is not null && AppearanceTemplateNames.Contains(selected))
        {
            SelectedAppearanceTemplate = selected;
        }
    }

    // 인-셸 세분 색 직접 지정(hex) — 프리셋 너머 임의 색을 글자색/배경색에 적용(§7.3-A).
    // 배경은 솔리드로 적용(끝색=시작색, 그라데이션 해제). 잘못된 hex 는 무시하고 상태바로 안내.
    private void ApplyColorHex(string? hex, bool isBackground)
    {
        if (!TryParseHexColor(hex, out var argb))
        {
            StatusText = "색 형식이 올바르지 않습니다(예: #1A2B3C).";
            return;
        }

        if (isBackground)
        {
            _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, argb);
            _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb, argb);
            _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundIsGradient, false);
            StatusText = $"배경색: {FormatColorHex(argb)}";
        }
        else
        {
            _settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, argb);
            StatusText = $"글자색: {FormatColorHex(argb)}";
        }

        RefreshActiveAppearance();
    }

    // "#RRGGBB"/"RRGGBB" → 불투명(알파 FF) ARGB 정수. 실패 시 false.
    // 인스펙터 표시는 RGB(6자리) 기준이고 모든 색을 불투명으로 다루므로, 표시·입력 대칭을 위해
    // 8자리(알파 포함)는 일부러 받지 않는다(반투명이 저장되는데 표시는 6자리로 잘리는 비대칭 방지).
    private static bool TryParseHexColor(string? hex, out int argb)
    {
        argb = 0;
        if (string.IsNullOrWhiteSpace(hex))
        {
            return false;
        }

        var s = hex.Trim().TrimStart('#');
        if (s.Length != 6)
        {
            return false;
        }

        if (!uint.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var rgb))
        {
            return false;
        }

        // 항상 불투명(알파 FF) — 설정 기본값(-16777216=0xFF000000 등)과 동일한 표현.
        argb = unchecked((int)(0xFF000000u | rgb));
        return true;
    }

    // ARGB 정수 → "#RRGGBB"(알파 생략 — 인스펙터 표시는 RGB 만).
    private static string FormatColorHex(int argb)
    {
        var v = unchecked((uint)argb);
        return $"#{(v >> 16) & 0xFF:X2}{(v >> 8) & 0xFF:X2}{v & 0xFF:X2}";
    }

    // 인-셸 가사 폰트 효과 토글(굵게/기울임/그림자) — 현재 값을 반전해 저장(출력 VM 라이브 반영).
    // Active* 동기화는 RefreshActiveAppearance(SettingsChanged 경유)가 담당하므로 여기선 설정만 뒤집는다.
    private void ToggleLyricsEffect(SettingKey<bool> key, bool current)
    {
        var next = !current;
        _settings.Set(key, next);
        // 효과별 한글 라벨 — 새 효과를 이 헬퍼로 추가하면 여기에 명시 분기를 더한다(암묵적 fallback 방지).
        var label = key.Id == EasiSettingKeys.LyricsMonitorBold.Id ? "굵게"
            : key.Id == EasiSettingKeys.LyricsMonitorItalic.Id ? "기울임"
            : key.Id == EasiSettingKeys.LyricsMonitorShadow.Id ? "그림자"
            : key.Id == EasiSettingKeys.LyricsMonitorUnderline.Id ? "밑줄"
            : key.Id == EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.Id ? "강조 후렴만"
            : key.Id == EasiSettingKeys.LyricsMonitorShowPositionIndicator.Id ? "위치 표시"
            : key.Id == EasiSettingKeys.LyricsMonitorShowTitleHeading.Id ? "제목 표시"
            : key.Id == EasiSettingKeys.LyricsMonitorOutline.Id ? "외곽선"
            : key.Id == EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.Id ? "제목 첫 화면만"
            : key.Id == EasiSettingKeys.LyricsMonitorShowItemNumber.Id ? "곡 번호"
            : key.Id == EasiSettingKeys.LyricsMonitorShowCopyright.Id ? "저작권"
            : key.Id == EasiSettingKeys.LyricsMonitorShowNextItem.Id ? "다음 항목"
            : key.Id == EasiSettingKeys.LyricsMonitorPanelTransparent.Id ? "패널 투명 배경"
            : key.Id == EasiSettingKeys.LyricsMonitorUseFadeTransition.Id ? "전환 페이드"
            : key.Id;
        StatusText = $"가사 {label}: {(next ? "켬" : "끔")}";
    }

    // 출력 전환(페이드) 길이 프리셋 적용(빠르게 150 / 보통 250 / 느리게 500 ms). 범위(0~2000) 클램프 후 저장.
    private void ApplyTransitionDuration(int durationMs)
    {
        var next = Math.Clamp(durationMs, 0, 2000);
        _settings.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, next);
        ActiveTransitionDurationMs = next;
        StatusText = $"전환 길이: {next}ms";
    }

    // 출력 전환 모션 종류 적용(Fade/Slide 4방향). 저장 후 Active 동기화는 RefreshActiveAppearance 가 담당.
    private void ApplyTransitionKind(LyricsTransitionKind kind)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorTransitionKind, kind);
        ActiveTransitionKind = kind;
        var label = kind switch
        {
            LyricsTransitionKind.Fade => "페이드",
            LyricsTransitionKind.SlideFromLeft => "슬라이드(왼쪽)",
            LyricsTransitionKind.SlideFromRight => "슬라이드(오른쪽)",
            LyricsTransitionKind.SlideFromTop => "슬라이드(위)",
            LyricsTransitionKind.SlideFromBottom => "슬라이드(아래)",
            LyricsTransitionKind.ZoomIn => "줌 인",
            LyricsTransitionKind.ZoomOut => "줌 아웃",
            LyricsTransitionKind.Spin => "회전",
            LyricsTransitionKind.FlipHorizontal => "뒤집기(가로)",
            LyricsTransitionKind.FlipVertical => "뒤집기(세로)",
            LyricsTransitionKind.RevealCircle => "원형 리빌",
            LyricsTransitionKind.RevealRectangle => "사각 리빌",
            LyricsTransitionKind.WipeRight => "와이프(→)",
            LyricsTransitionKind.WipeLeft => "와이프(←)",
            LyricsTransitionKind.WipeDown => "와이프(↓)",
            LyricsTransitionKind.WipeUp => "와이프(↑)",
            LyricsTransitionKind.BlindsHorizontal => "블라인드(가로)",
            LyricsTransitionKind.BlindsVertical => "블라인드(세로)",
            LyricsTransitionKind.Checkerboard => "체커보드",
            LyricsTransitionKind.Diamond => "다이아몬드",
            LyricsTransitionKind.DoorsOpen => "양문 열기",
            LyricsTransitionKind.DoorsClose => "양문 닫기",
            LyricsTransitionKind.Star => "별",
            LyricsTransitionKind.Cross => "십자(2-레이어)",
            LyricsTransitionKind.BowTie => "나비넥타이(2-레이어)",
            LyricsTransitionKind.Heart => "하트(2-레이어)",
            LyricsTransitionKind.Wedge => "시계 와이프",
            LyricsTransitionKind.Spiral => "나선",
            LyricsTransitionKind.WindMill => "바람개비",
            LyricsTransitionKind.FanUp => "부채 펼침",
            _ => kind.ToString(),
        };
        StatusText = $"전환 효과: {label}";
    }

    // 공지 화면 송출(FrmInfoScreen — 자유 텍스트 안내). InfoScreen 창에서 입력한 텍스트를
    // 즉시 회중 출력에 본문으로 송출한다. 출력 창이 열려 있을 때만 동작(닫혀 있으면 false 반환).
    // 공지는 큐 항목이 아니라 일시 라이브 항목이다. _liveItemId 를 센티넬(NoticeLiveId)로 둬
    // 슬라이드/절 이동 가드(== 선택 항목 ID)가 자연히 false 가 되도록 한다 — null 로 두면 슬라이드 이동
    // 가드의 "라이브 미시작" 와일드카드(_liveItemId is null)에 걸려 의미 없는 이동 버튼이 켜진다.
    public bool PublishNotice(string text, int fontSizePt = 0)
    {
        if (string.IsNullOrWhiteSpace(text) || !_output.Current.IsOpen)
        {
            return false;
        }

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        // 글자 크기 지정 시 레거시 FormatData(코드 47=pt)로 실어 기존 곡별 폰트 오버라이드 파이프라인을 그대로 재사용.
        // 0(미지정)이면 FormatData 없음 → 출력 기본 글자 크기 사용. 디코더가 6~100pt 로 검증한다.
        var formatData = fontSizePt > 0 ? $"47={fontSizePt}>" : null;
        var notice = new LiveQueueItem(LiveItemKinds.NoticeLiveId, "공지", LiveItemKinds.Notice)
        {
            Lyrics = text,
            FormatData = formatData,
        };
        _liveItemId = LiveItemKinds.NoticeLiveId;
        // 공지는 가사가 없어 조옮김과 무관하다. 라이브 조옮김 초기화는 곡 송출 진입점인 PublishSelectedItem 한 곳이
        // 책임진다(여기서 건드리지 않음) — 공지 중 조옮김을 눌러도 RepublishLiveSongForBodyChange 가 센티넬을
        // 큐에서 못 찾아 무시하므로 안전하다.
        _session.GoLive(notice, monitorName);
        StatusText = "공지 화면 송출";
        NotifyCommandStates();
        return true;
    }

    // 공지 화면 지우기 — 출력을 검은 화면(숨김)으로 돌려 공지를 내린다(InfoScreen 창의 "지우기").
    // 출력이 열려 있지 않으면 아무것도 안 한다.
    public void ClearNotice()
    {
        if (!_output.Current.IsOpen)
        {
            return;
        }

        _session.HideOutput(blackout: true);
        _liveItemId = null;
        StatusText = "공지 숨김(검은 화면)";
        NotifyCommandStates();
    }

    // 전역 출력 배경 이미지 설정(FrmMain Images 탭 — 배경으로 적용). 코드-비하인드의 파일 선택 결과를 받아 저장.
    // 라이브 출력 VM 이 설정 변경을 즉시 반영해 색 배경 위에 이미지를 깐다(곡별 배경 61 이 있으면 그 곡은 우선).
    public void SetOutputBackgroundImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundImagePath, imagePath);
        StatusText = $"출력 배경 이미지: {System.IO.Path.GetFileName(imagePath)}";
    }

    // 전역 출력 배경 이미지 해제 → 색 배경으로 복귀.
    private void ClearOutputBackgroundImage()
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundImagePath, string.Empty);
        StatusText = "출력 배경 이미지 해제";
    }

    // 인-셸 가사 폰트 크기 조절(+/- 단계) — 범위로 클램프 후 설정 저장(출력 VM 라이브 반영).
    private void StepLyricsFontSize(int delta)
    {
        var next = Math.Clamp(ActiveLyricsFontSize + delta, LyricsFontSizeMin, LyricsFontSizeMax);
        if (next == ActiveLyricsFontSize)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorFontSize, next);
        ActiveLyricsFontSize = next;
        StatusText = $"가사 크기: {next}px";
    }

    // 인-셸 가사 줄 간격 조절(+/- 단계, %) — 폰트 크기 증감과 동일 구조. 범위 클램프 후 설정 저장.
    private void StepLyricsLineSpacing(int delta)
    {
        var next = Math.Clamp(ActiveLyricsLineSpacing + delta, LyricsLineSpacingMin, LyricsLineSpacingMax);
        if (next == ActiveLyricsLineSpacing)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, next);
        ActiveLyricsLineSpacing = next;
        StatusText = $"줄 간격: {next}%";
    }

    // 현재 설정과 일치하는 프리셋을 찾아 활성 이름을 갱신(없으면 "사용자 지정").
    private void RefreshActiveAppearance()
    {
        var text = _settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb);
        var bg1 = _settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb);
        var bg2 = _settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColor2Argb);
        var gradient = _settings.Get(EasiSettingKeys.LyricsMonitorBackgroundIsGradient);

        var match = OutputAppearancePresets.FirstOrDefault(p =>
            p.TextArgb == text && p.Background1Argb == bg1 && p.Background2Argb == bg2 && p.IsGradient == gradient);
        ActiveAppearanceName = match?.Name ?? "사용자 지정";

        // 가사 정렬(가로/세로)·폰트 크기 활성 상태도 함께 동기화(Settings 창 등 다른 경로 변경도 인스펙터가 따라가도록).
        ActiveLyricsAlignment = _settings.Get(EasiSettingKeys.LyricsMonitorTextAlignment);
        ActiveLyricsVerticalAlignment = _settings.Get(EasiSettingKeys.LyricsMonitorVerticalAlignment);
        ActiveBackgroundMode = _settings.Get(EasiSettingKeys.LyricsMonitorBackgroundMode);
        ActiveRegionDisplay = _settings.Get(EasiSettingKeys.LyricsMonitorRegionDisplay);
        ActiveLyricsFontSize = _settings.Get(EasiSettingKeys.LyricsMonitorFontSize);
        ActiveLyricsLineSpacing = _settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent);
        ActiveLyricsBold = _settings.Get(EasiSettingKeys.LyricsMonitorBold);
        ActiveLyricsItalic = _settings.Get(EasiSettingKeys.LyricsMonitorItalic);
        ActiveLyricsShadow = _settings.Get(EasiSettingKeys.LyricsMonitorShadow);
        ActiveLyricsUnderline = _settings.Get(EasiSettingKeys.LyricsMonitorUnderline);
        ActiveLyricsEmphasisChorusOnly = _settings.Get(EasiSettingKeys.LyricsMonitorEmphasisChorusOnly);
        ActiveLyricsPanelTransparent = _settings.Get(EasiSettingKeys.LyricsMonitorPanelTransparent);
        ActiveLyricsPositionIndicator = _settings.Get(EasiSettingKeys.LyricsMonitorShowPositionIndicator);
        ActiveLyricsItemNumber = _settings.Get(EasiSettingKeys.LyricsMonitorShowItemNumber);
        ActiveLyricsCopyright = _settings.Get(EasiSettingKeys.LyricsMonitorShowCopyright);
        ActiveLyricsNextItem = _settings.Get(EasiSettingKeys.LyricsMonitorShowNextItem);
        ActiveFadeTransition = _settings.Get(EasiSettingKeys.LyricsMonitorUseFadeTransition);
        ActiveTransitionDurationMs = _settings.Get(EasiSettingKeys.LyricsMonitorTransitionDurationMs);
        ActiveTransitionKind = _settings.Get(EasiSettingKeys.LyricsMonitorTransitionKind);
        ActiveLyricsTitleHeading = _settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading);
        ActiveLyricsOutline = _settings.Get(EasiSettingKeys.LyricsMonitorOutline);
        ActiveTitleHeadingAlignment = _settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment);
        ActiveTitleHeadingFirstScreenOnly = _settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly);
        ActiveTextColorHex = FormatColorHex(text);
        ActiveBackgroundColorHex = FormatColorHex(bg1);
    }

    private void ApplyOperationalSettings(bool updateStatus)
    {
        IsPowerPointTabVisible = _settings.Get(EasiSettingKeys.UsePowerPointTab);
        IsPowerPointPanelOverlayEnabled = !_settings.Get(EasiSettingKeys.NoPowerPointPanelOverlay);
        PowerPointMaxFiles = _settings.Get(EasiSettingKeys.PowerPointMaxFiles);
        IsMediaTabVisible = _settings.Get(EasiSettingKeys.UseMediaTab);
        IsMediaPanelOverlayEnabled = !_settings.Get(EasiSettingKeys.NoMediaPanelOverlay);
        MediaDirectory = _settings.Get(EasiSettingKeys.MediaDirectory);
        LiveCameraNumber = _settings.Get(EasiSettingKeys.LiveCameraNumber);
        LiveCameraSource = MediaPlaybackService.CreateLiveCameraSource(LiveCameraNumber);
        AutoRotateIntervalSeconds = _settings.Get(EasiSettingKeys.AutoRotateIntervalSeconds);
        RefreshActiveAppearance();
        RefreshPowerPointLimitState(updateStatus);
        // 탭 가시성이 바뀌면 현재 선택 항목 기준으로 중앙 탭을 재평가 — 방금 숨겨진 탭이 선택된 채 남아
        // 빈 패널이 보이는 것을 막는다(WPF 는 가시성 Collapsed 시 선택을 자동으로 풀지 않음, code-review MINOR).
        UpdateContentTabForItem(SelectedItem);
        NotifyCommandStates();
    }

    private void RefreshPowerPointLimitState(bool updateStatus)
    {
        var wasViolation = HasPowerPointLimitViolation;
        PowerPointFileCount = Queue.Count(IsPowerPointItem);
        HasPowerPointLimitViolation = PowerPointFileCount > PowerPointMaxFiles;
        if (updateStatus && HasPowerPointLimitViolation)
        {
            StatusText = $"PowerPoint 제한 초과: {PowerPointFileCount}/{PowerPointMaxFiles}";
        }
        else if (updateStatus && wasViolation)
        {
            StatusText = Queue.Count == 0 ? "송출할 항목이 없습니다" : $"{Queue.Count}개 항목 로드됨";
        }
    }

    private static bool ContainsOperationalSetting(IReadOnlyList<string> changedKeys)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            var key = changedKeys[i];
            if (string.Equals(key, EasiSettingKeys.UsePowerPointTab.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoPowerPointPanelOverlay.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.PowerPointMaxFiles.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.UseMediaTab.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoMediaPanelOverlay.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.MediaDirectory.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LiveCameraNumber.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.AutoRotateIntervalSeconds.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // 종류 판별은 공용 매처(LiveItemKindMatcher)에 위임 — 별칭 목록을 한 곳에서 관리(예배 순서 검증과 동일 어휘).
    private static bool IsPowerPointItem(LiveQueueItem item)
        => LiveItemKindMatcher.IsPowerPoint(item.Kind);

    private static bool IsMediaItem(LiveQueueItem item)
        => LiveItemKindMatcher.IsMedia(item.Kind);

    private void ApplyLiveSnapshot(LiveSessionSnapshot snapshot)
    {
        LiveBar.State = snapshot.State;
        LiveBar.CurrentItemTitle = snapshot.CurrentItemTitle;
        LiveBar.OutputMonitorName = snapshot.OutputMonitorName;
        // 자동 회전은 라이브 완전 종료(Stop=Off)에서만 해제한다 — 숨김/검정/비우기(Hidden)는 임시 상태라
        // 유지하고 복귀(Restore→Active) 시 그대로 이어간다. 숨김 중에는 AdvanceAutoRotation 이 State!=Active 로
        // no-op 이라 출력을 깨우지 않으므로 회전 상태를 유지해도 안전하다(View 타이머가 IsAutoRotating 을 보고 멈춤).
        if (snapshot.State == LiveState.Off && IsAutoRotating)
        {
            IsAutoRotating = false;
        }
        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        GoLiveCommand.NotifyCanExecuteChanged();
        CloseOutputCommand.NotifyCanExecuteChanged();
        StopLiveCommand.NotifyCanExecuteChanged();
        NextItemCommand.NotifyCanExecuteChanged();
        PreviousItemCommand.NotifyCanExecuteChanged();
        FirstItemCommand.NotifyCanExecuteChanged();
        LastItemCommand.NotifyCanExecuteChanged();
        HideOutputCommand.NotifyCanExecuteChanged();
        BlackScreenCommand.NotifyCanExecuteChanged();
        ClearOutputCommand.NotifyCanExecuteChanged();
        RestoreOutputCommand.NotifyCanExecuteChanged();
        RestartCurrentItemCommand.NotifyCanExecuteChanged();
        RefreshOutputCommand.NotifyCanExecuteChanged();
        NextSlideCommand.NotifyCanExecuteChanged();
        PreviousSlideCommand.NotifyCanExecuteChanged();
        GoToSlideCommand.NotifyCanExecuteChanged();
        AddSelectedLibrarySongCommand.NotifyCanExecuteChanged();
        MoveSelectedItemUpCommand.NotifyCanExecuteChanged();
        MoveSelectedItemDownCommand.NotifyCanExecuteChanged();
        RemoveSelectedItemCommand.NotifyCanExecuteChanged();
        NextLyricsPageCommand.NotifyCanExecuteChanged();
        PreviousLyricsPageCommand.NotifyCanExecuteChanged();
        ToggleAutoRotateCommand.NotifyCanExecuteChanged();
    }

    private static void RegisterIfMissing(ShortcutRegistry registry, Shortcut shortcut)
    {
        if (registry.All.Any(s => s.CommandName == shortcut.CommandName && s.Key == shortcut.Key && s.Modifiers == shortcut.Modifiers))
        {
            return;
        }

        registry.Register(shortcut);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _settings.SettingsChanged -= OnSettingsChanged;
        PowerPoint.PropertyChanged -= OnPowerPointPropertyChanged;
        Library.PropertyChanged -= OnLibraryPropertyChanged;
        Search.SearchResults.CollectionChanged -= OnSearchResultsChanged;
        Search.LookupCandidates.CollectionChanged -= OnLookupCandidatesChanged;
        // Media VM 정리. DI 컨테이너도 transient IDisposable 을 추적·해제하므로 이중 호출될 수 있으나
        // MediaPlaybackViewModel.Dispose 가 멱등이라 안전(테스트는 new 생성이라 이 경로가 유일 해제).
        Media.Dispose();
        // PowerPoint VM 은 이벤트 구독/미관리 자원이 없어 IDisposable 이 아니다 — 의도적으로 해제하지 않음.
    }

}
