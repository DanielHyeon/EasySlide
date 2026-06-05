using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
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

public enum PreviewPanelMode
{
    Text,
    Format,
    Info,
}

public sealed record OperatorLyricsPageCard(
    int PageIndex,
    string Label,
    string PositionText,
    string BodyText,
    bool IsCurrent);

public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ILiveSessionService _session;
    private readonly IOutputWindowService _output;
    // 스테이지(Preview) 모니터 서비스 — 회중용 출력과 별개로 리더·밴드가 보는 확인 모니터를 열고/옮기고/닫는다(gap §3.1).
    // DI 에선 PreviewWindowHost 와 같은 싱글톤이 주입돼, 여기서 Open/Close 하면 그 호스트가 실제 창을 띄운다.
    private readonly IPreviewWindowService _preview;
    private readonly ILiveSafetyPrompt _safetyPrompt;
    private readonly ICommandTelemetry _telemetry;
    private readonly IDisplayService _display;
    private readonly ICommandCatalog _commandCatalog;
    private readonly ISettingsService _settings;
    private readonly IWorshipListStore _worshipLists;
    private readonly IRecentWorshipLists _recentWorshipLists;
    private readonly Func<string, bool> _worshipMediaLauncher;
    // 예배 순서 검증기 — 라이브 송출 전 깨진 PPT·미디어 파일을 미리 거른다(레거시 ValidateWorshipListItems).
    private readonly WorshipListValidator _worshipValidator;
    private readonly IAppearanceTemplateStore _appearanceTemplates;
    // 좌측 "검색" 탭의 교차 검색 결과를 큐에 추가할 때, 결과(SongSearchResult)엔 가사가 없어 SongId 로 곡 상세(가사)를 불러온다.
    private readonly Data.IAdminSongDetailRepository _songDetail;
    // 명령 팔레트 실행에 쓰는 단축키 레지스트리 — BindShortcuts(앱 시작)에서 주입된다. 그 전엔 null.
    private ShortcutRegistry? _shortcutRegistry;
    // 스테이지 모니터 목록 갱신(RefreshPreviewDisplays) 중인가 — 그때의 선택 변경은 사용자 입력이 아니므로
    // PreviewMonitorId 를 영속화하지 않는다(분리된 모니터 fallback 으로 저장된 선호를 덮어쓰지 않게).
    private bool _suppressPreviewMonitorPersist;

    [ObservableProperty] private LiveQueueItem? _selectedItem;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOutputPowerPointContext))]
    private LiveQueueItem? _outputItem;
    [ObservableProperty] private OutputDisplay? _selectedOutputDisplay;
    // 스테이지(Preview) 모니터로 선택된 디스플레이 + 창 열림 여부(메뉴·버튼 활성 상태에 쓰임).
    [ObservableProperty] private OutputDisplay? _selectedPreviewDisplay;
    [ObservableProperty] private bool _isStageMonitorOpen;
    [ObservableProperty] private string _statusText = "WPF 운영 준비됨";
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendLiveMessageCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearLiveMessageCommand))]
    private string _outputLiveMessage = string.Empty;
    // 예배 순서 검증에서 문제가 하나라도 있으면 true — 좌측 패널의 경고 목록 표시 여부에 쓰인다.
    [ObservableProperty] private bool _hasWorshipListProblems;
    // 예배 순서(큐)가 비어 있으면 true — 좌측 패널의 "비어 있음" 안내 표시 여부. 시작 시 빈 큐(더미 시드 제거).
    [ObservableProperty] private bool _isQueueEmpty = true;

    // 예배 순서가 마지막 저장/불러오기 이후 바뀌었는지(미저장 변경) — 세션 콤보 옆 "● 수정됨" 표시에 바인딩.
    // 큐 변경(추가·삭제·이동·서식 편집)마다 true, 저장/불러오기/시작 시 false. 운영자가 저장을 잊지 않게 한다.
    [ObservableProperty] private bool _worshipListHasUnsavedChanges;
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

    // FrmMain Preview 하단 Text/Set/Info 버튼 상태. WinForms 는 RadioButton(Appearance=Button)으로
    // flowLayoutPreviewLyrics / IndPanel / PreviewInfo 중 하나만 보이게 한다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPreviewTextMode))]
    [NotifyPropertyChangedFor(nameof(IsPreviewFormatMode))]
    [NotifyPropertyChangedFor(nameof(IsPreviewInfoMode))]
    private PreviewPanelMode _previewPanelMode = PreviewPanelMode.Text;

    // 우측 출력 모양 인스펙터 펼침 여부(FrmMain식 가변 패널, §7.4). 접으면 우측 컬럼이 0 으로 줄어 중앙 미리보기가 넓어진다.
    // 기본 펼침(true). 운영바 토글로 전환. 변경 시 설정에 저장돼 다음 실행에도 접힘/펼침 상태가 유지된다(레거시 패널 상태 저장).
    [ObservableProperty] private bool _isInspectorExpanded = true;
    // 설정에서 인스펙터 상태를 "복원하는 중"인지 표시 — 복원이 OnChanged 를 울려 같은 값을 되저장하는 군더더기·재진입을 막는다.
    private bool _isApplyingOperationalSettings;

    // 인스펙터를 접거나 펼치면 그 상태를 설정에 저장한다(다음 실행에도 유지). 단, 복원 중에는 되저장하지 않는다.
    partial void OnIsInspectorExpandedChanged(bool value)
    {
        if (_isApplyingOperationalSettings)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.MainInspectorExpanded, value);
    }

    [ObservableProperty] private bool _isPowerPointTabVisible;
    [ObservableProperty] private bool _isPowerPointPanelOverlayEnabled = true;
    [ObservableProperty] private int _powerPointMaxFiles = EasiSettingKeys.PowerPointMaxFiles.DefaultValue;
    [ObservableProperty] private int _powerPointFileCount;
    [ObservableProperty] private bool _hasPowerPointLimitViolation;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOutputLyricsText))]
    private string _outputLyricsText = string.Empty;
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
    // 현재 적용된 배경 그라데이션 방향(세로/가로/대각↘/대각↗) — 라디오 체크 표시에 쓰인다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GradientDirectionIsVertical))]
    [NotifyPropertyChangedFor(nameof(GradientDirectionIsHorizontal))]
    [NotifyPropertyChangedFor(nameof(GradientDirectionIsDiagonalDown))]
    [NotifyPropertyChangedFor(nameof(GradientDirectionIsDiagonalUp))]
    private LyricsGradientDirection _activeGradientDirection = EasiSettingKeys.LyricsMonitorBackgroundGradientDirection.DefaultValue;
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
    // 현재 보조 영역(Region2) 전역 폰트 크기(px). 0 = 본문(Region1)과 동일(자동). 직접 입력 박스에 바인딩.
    [ObservableProperty] private int _activeLyricsFontSize2 = EasiSettingKeys.LyricsMonitorFontSize2.DefaultValue;
    // 현재 적용된 출력 가사 전역 글꼴명(FrmMain Def_FontName). 빈 문자열=테마 기본 글꼴 상속. 글꼴 선택 콤보에 바인딩.
    [ObservableProperty] private string _activeLyricsFontFamily = EasiSettingKeys.LyricsMonitorFontFamily.DefaultValue;
    // 현재 적용된 보조 영역(Region2) 전역 글꼴명(FrmMain Ind_Reg2Font). 빈 문자열=본문(Region1) 글꼴 추종. 글꼴 선택 콤보에 바인딩.
    [ObservableProperty] private string _activeLyricsFontFamily2 = EasiSettingKeys.LyricsMonitorFontFamily2.DefaultValue;
    // 현재 적용된 보조 영역(Region2) 전역 글자색(ARGB). 0=본문(Region1) 색 추종. 색 선택 콤보에 바인딩.
    [ObservableProperty] private int _activeLyricsTextColor2Argb = EasiSettingKeys.LyricsMonitorTextColor2Argb.DefaultValue;
    // 현재 적용된 보조 영역(Region2) 전역 가로 정렬. FollowRegion1=본문 정렬 추종. 정렬 선택 콤보에 바인딩.
    [ObservableProperty] private LyricsRegion2Alignment _activeLyricsRegion2Alignment = EasiSettingKeys.LyricsMonitorRegion2Alignment.DefaultValue;
    // 현재 적용된 보조 영역(Region2) 전역 굵게(3-상태). FollowRegion1=본문 굵게 추종. 굵게 선택 콤보에 바인딩.
    [ObservableProperty] private LyricsRegion2Emphasis _activeLyricsRegion2Bold = EasiSettingKeys.LyricsMonitorRegion2Bold.DefaultValue;
    // 현재 적용된 보조 영역(Region2) 전역 기울임(3-상태). FollowRegion1=본문 기울임 추종. 기울임 선택 콤보에 바인딩.
    [ObservableProperty] private LyricsRegion2Emphasis _activeLyricsRegion2Italic = EasiSettingKeys.LyricsMonitorRegion2Italic.DefaultValue;
    // 현재 적용된 보조 영역(Region2) 전역 밑줄(3-상태). FollowRegion1=본문 밑줄 추종. 밑줄 선택 콤보에 바인딩.
    [ObservableProperty] private LyricsRegion2Emphasis _activeLyricsRegion2Underline = EasiSettingKeys.LyricsMonitorRegion2Underline.DefaultValue;

    /// <summary>FrmMain cbOutputBlack 체크 상태: 현재 출력이 검은 화면으로 숨겨져 있는가.</summary>
    public bool IsOutputBlackActive => _session.Current.State == LiveState.Hidden && _session.Current.IsBlackout;

    /// <summary>FrmMain cbOutputClear 체크 상태: 현재 출력이 배경만 남기고 비워져 있는가.</summary>
    public bool IsOutputClearActive => _session.Current.State == LiveState.Hidden && _session.Current.IsCleared;

    /// <summary>FrmMain cbGoLive 체크 상태: 현재 출력이 라이브로 표시 중인가.</summary>
    public bool IsOutputLiveActive => _session.Current.State == LiveState.Active;

    /// <summary>FrmMain flowLayoutOutputPowerPoint 표시 대상인가.</summary>
    public bool IsOutputPowerPointContext => OutputItem is not null && IsPowerPointItem(OutputItem);

    /// <summary>FrmMain flowLayoutOutputLyrics/OutputInfo 에 보여 줄 비-PPT Output 본문이 있는가.</summary>
    public bool HasOutputLyricsText => !string.IsNullOrWhiteSpace(OutputLyricsText);

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

    // Display Panel 글자 크기 비율 조절 범위·단계(설정 Validate 범위 50~200% 와 일치). FrmMain Def_PanelFont 크기.
    private const int LyricsPanelFontScaleMin = 50;
    private const int LyricsPanelFontScaleMax = 200;
    private const int LyricsPanelFontScaleStep = 10;

    // 현재 Display Panel 글자 크기 비율(%). +/- 커맨드 활성/비활성 판별에도 쓰인다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreasePanelFontScaleCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreasePanelFontScaleCommand))]
    private int _activePanelFontScale = EasiSettingKeys.LyricsMonitorPanelFontScalePercent.DefaultValue;

    // 본문 여백 조절 범위·단계(설정 Validate 범위 0~400px 와 일치). FrmMain ShowLeftMargin/Right/Bottom 대응.
    private const int LyricsBodyMarginMin = 0;
    private const int LyricsBodyMarginMax = 400;
    private const int LyricsBodyMarginStep = 8;

    // 현재 본문 좌/우/아래 여백(px). +/- 커맨드 활성/비활성 판별에도 쓰인다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreaseLyricsLeftMarginCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseLyricsLeftMarginCommand))]
    private int _activeLyricsLeftMargin = EasiSettingKeys.LyricsMonitorBodyLeftMargin.DefaultValue;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreaseLyricsRightMarginCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseLyricsRightMarginCommand))]
    private int _activeLyricsRightMargin = EasiSettingKeys.LyricsMonitorBodyRightMargin.DefaultValue;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreaseLyricsBottomMarginCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseLyricsBottomMarginCommand))]
    private int _activeLyricsBottomMargin = EasiSettingKeys.LyricsMonitorBodyBottomMargin.DefaultValue;

    // 이중 언어 영역 간 세로 간격 조절 범위·단계(설정 Validate 범위 0~100px 와 일치). FrmMain Ind_Reg2TopUpDown.
    private const int LyricsRegionGapMin = 0;
    private const int LyricsRegionGapMax = 100;
    private const int LyricsRegionGapStep = 4;

    // 현재 이중 언어 영역 간 세로 간격(px). +/- 커맨드 활성/비활성 판별에도 쓰인다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IncreaseRegionGapCommand))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseRegionGapCommand))]
    private int _activeRegionGap = EasiSettingKeys.LyricsMonitorRegionGapPx.DefaultValue;

    // 본문 세로 위치 오프셋 조절 범위·단계(설정 Validate 범위 -300~300px 와 일치). FrmMain Ind_Reg1TopUpDown.
    private const int LyricsBodyVOffsetMin = -300;
    private const int LyricsBodyVOffsetMax = 300;
    private const int LyricsBodyVOffsetStep = 8;

    // 현재 본문 세로 위치 오프셋(px, 음수=위). +/- 커맨드 활성/비활성 판별에도 쓰인다.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveBodyDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveBodyUpCommand))]
    private int _activeBodyVerticalOffset = EasiSettingKeys.LyricsMonitorBodyVerticalOffset.DefaultValue;

    // 현재 폰트 효과 상태(인스펙터 ToggleButton IsChecked 바인딩용). 설정에서 유래.
    [ObservableProperty] private bool _activeLyricsBold = EasiSettingKeys.LyricsMonitorBold.DefaultValue;
    [ObservableProperty] private bool _activeLyricsItalic = EasiSettingKeys.LyricsMonitorItalic.DefaultValue;
    [ObservableProperty] private bool _activeLyricsShadow = EasiSettingKeys.LyricsMonitorShadow.DefaultValue;
    [ObservableProperty] private bool _activeLyricsUnderline = EasiSettingKeys.LyricsMonitorUnderline.DefaultValue;
    // 현재 "코드/악상 표시"(FrmMain Def_Notations) 상태 — on 이면 송출 본문에 코드 줄을 함께 그린다(조옮김은 코드 표시 on 일 때만 의미).
    [ObservableProperty] private bool _activeLyricsNotations = EasiSettingKeys.LyricsMonitorShowNotations.DefaultValue;
    [ObservableProperty] private bool _activeLyricsEmphasisChorusOnly = EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.DefaultValue;
    [ObservableProperty] private bool _activeLyricsInterlace = EasiSettingKeys.LyricsMonitorInterlace.DefaultValue;
    // 현재 Display Panel 투명 배경 상태(메뉴 체크 바인딩용).
    [ObservableProperty] private bool _activeLyricsPanelTransparent = EasiSettingKeys.LyricsMonitorPanelTransparent.DefaultValue;
    // 현재 위치 인디케이터 표시 상태(인스펙터 ToggleButton IsChecked 바인딩용).
    [ObservableProperty] private bool _activeLyricsPositionIndicator = EasiSettingKeys.LyricsMonitorShowPositionIndicator.DefaultValue;
    // 현재 "절 헤딩 표시" 상태(인스펙터/메뉴 토글 IsChecked 바인딩용, FrmMain Def_Head All).
    [ObservableProperty] private bool _activeLyricsVerseHeading = EasiSettingKeys.LyricsMonitorShowVerseHeading.DefaultValue;
    // 현재 곡 번호 표시 상태(메뉴 체크 바인딩용, Display Panel).
    [ObservableProperty] private bool _activeLyricsItemNumber = EasiSettingKeys.LyricsMonitorShowItemNumber.DefaultValue;
    // 현재 "정보 패널 제목 표시"(FrmMain Def_PanelTitle) 상태(메뉴 토글 IsChecked 바인딩용).
    [ObservableProperty] private bool _activeLyricsTitleOnPanel = EasiSettingKeys.LyricsMonitorShowTitleOnPanel.DefaultValue;
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
    // 현재 적용된 슬라이드/절 전환 종류(같은 항목 안 절·슬라이드 이동 때). 항목 전환(_activeTransitionKind)과 별개. 콤보에 바인딩.
    [ObservableProperty] private LyricsTransitionKind _activeSlideTransitionKind = EasiSettingKeys.LyricsMonitorSlideTransitionKind.DefaultValue;

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
    // 현재 "헤딩이 본문 정렬 따름"(FrmMain AsR1) 상태 — on 이면 헤딩이 본문(Region1) 정렬을 그대로 사용.
    [ObservableProperty] private bool _activeTitleHeadingFollowBody = EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody.DefaultValue;
    // 현재 "헤딩이 보조 영역(Region2) 정렬 따름"(FrmMain AsR2) 상태 — on 이면 헤딩이 Region2 정렬을 사용(AsR1 보다 우선).
    [ObservableProperty] private bool _activeTitleHeadingFollowRegion2 = EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2.DefaultValue;
    // 자동 회전 활성 상태(View 가 이 값을 보고 DispatcherTimer 시작/정지). 라이브 종료 시 자동 해제.
    [ObservableProperty] private bool _isAutoRotating;
    // 자동 회전 간격(초) — 설정에서 유래, View 타이머가 참조.
    [ObservableProperty] private int _autoRotateIntervalSeconds = EasiSettingKeys.AutoRotateIntervalSeconds.DefaultValue;
    // 자동 회전 모드(One/One-Repeat/Group/Group-Repeat) — 설정에서 유래, 콤보 선택에 바인딩. 끝 절/슬라이드 도달 시 동작이 모드별로 다르다.
    [ObservableProperty] private AutoRotateMode _activeAutoRotateMode = EasiSettingKeys.AutoRotateMode.DefaultValue;
    // 대기 화면(Gap) 모드(없음/검정/기본/로고) — 출력 메뉴 "대기 화면(Gap)" 콤보에 바인딩. 설정에서 유래, 출력이 라이브 갱신한다.
    [ObservableProperty] private GapItemMode _activeGapItemOption = EasiSettingKeys.GapItemOption.DefaultValue;
    // 대기 화면(Gap) 전환 페이드 사용 여부 — 출력 메뉴 토글.
    [ObservableProperty] private bool _activeGapItemUseFade = EasiSettingKeys.GapItemUseFade.DefaultValue;
    // 대기 화면(Gap) "로고" 모드에서 보여줄 이미지 경로 — 출력 메뉴 로고 선택/지우기.
    [ObservableProperty] private string _activeGapItemLogoFile = EasiSettingKeys.GapItemLogoFile.DefaultValue;

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

    private async Task<bool> TryLoadLegacyWorshipListAsync(string name)
    {
        if (_worshipLists is not ILegacyWorshipListStore legacyStore)
        {
            return false;
        }

        var xml = await legacyStore.LoadLegacyXmlAsync(name).ConfigureAwait(true);
        if (xml is null)
        {
            return false;
        }

        var items = EswWorshipListParser.Parse(xml);
        if (items.Count == 0)
        {
            StatusText = $"Legacy worship list is empty: {name} (.esw)";
            return true;
        }

        var queueItems = await BuildEswQueueItemsAsync(items).ConfigureAwait(true);
        LoadQueue(queueItems);
        RecordRecentWorshipList(name);
        WorshipListHasUnsavedChanges = false;
        StatusText = $"Legacy worship list loaded: {name} ({items.Count} .esw items)";
        return true;
    }

    private async Task<bool> TryMergeLegacyWorshipListAsync(string name)
    {
        if (_worshipLists is not ILegacyWorshipListStore legacyStore)
        {
            return false;
        }

        var xml = await legacyStore.LoadLegacyXmlAsync(name).ConfigureAwait(true);
        if (xml is null)
        {
            return false;
        }

        var items = EswWorshipListParser.Parse(xml);
        if (items.Count == 0)
        {
            StatusText = $"Legacy worship list merge skipped: {name} (.esw is empty)";
            return true;
        }

        var added = await InsertEswWorshipListItemsAsync(items, targetItem: null).ConfigureAwait(true);
        StatusText = $"Legacy worship list merged: {name} ({added} added, total {Queue.Count})";
        return true;
    }

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

    /// <summary>
    /// 현재 라이브 송출 중인 큐 항목의 Id(없으면 null). 예배 순서 목록이 "지금 출력 중인 항목"에 LIVE 표시를 붙이는 데 바인딩한다
    /// (선택 하이라이트는 선택일 뿐 라이브와 다를 수 있어 — 송출 후 다음으로 자동 이동하면 선택≠라이브). 공지 송출 시엔 센티넬(NoticeLiveId)이라 큐 항목과 안 맞아 표시 안 됨.
    /// </summary>
    public string? LiveItemId => _liveItemId;

    // _liveItemId 를 바꾸는 단일 통로 — 값이 바뀌면 LiveItemId 변경을 알려 목록의 LIVE 표시가 갱신되게 한다(직접 대입 대신 이걸로).
    private void SetLiveItemId(string? id)
    {
        if (_liveItemId == id)
        {
            return;
        }

        _liveItemId = id;
        OnPropertyChanged(nameof(LiveItemId));
        OnPropertyChanged(nameof(CanSelectLiveItem)); // 라이브 항목이 바뀌면 "현재 송출 항목 선택" 활성/비활성도 바뀐다.
        RefreshOutputSurfaceText();
        SelectLiveItemCommand?.NotifyCanExecuteChanged();
    }

    // 현재 송출 중인 큐 항목을 선택한다(LIVE 배지 항목으로 점프). 라이브가 아니거나 그 항목이 큐에 없으면(공지 센티넬 등) 아무 일도 안 한다.
    private void SelectLiveItem()
    {
        var live = Queue.FirstOrDefault(q => string.Equals(q.Id, _liveItemId, StringComparison.Ordinal));
        if (live is not null)
        {
            SelectedItem = live;
            StatusText = $"현재 송출 항목 선택: {live.Title}";
        }
    }

    /// <summary>
    /// 검증 문제 목록에서 한 문제를 클릭하면 그 항목을 예배 순서에서 선택한다 — 운영자가 깨진 PPT·미디어로 바로 가 제거·교체하게 한다.
    /// 검증 후 항목 인스턴스가 교체됐을 수 있어 참조 대신 <b>Id</b> 로 큐의 현재 인스턴스를 찾는다. 이미 제거된 항목이면 상태바로 알린다.
    /// </summary>
    private void SelectWorshipProblemItem(WorshipItemProblem? problem)
    {
        if (problem is null)
        {
            return;
        }

        var match = Queue.FirstOrDefault(q => q is not null && string.Equals(q.Id, problem.Item.Id, StringComparison.Ordinal));
        if (match is null)
        {
            StatusText = "이미 예배 순서에서 제거된 항목입니다.";
            return;
        }

        SelectedItem = match;
        StatusText = $"검증 문제 항목 선택: {(string.IsNullOrWhiteSpace(match.Title) ? match.Id : match.Title)}";
    }

    // "이 항목 서식 복사"로 담아 둔 항목별 서식(FormatData 문자열). 다른 항목에 "붙여넣기"하면 그대로 적용된다(없으면 null=붙여넣기 비활성).
    private string? _copiedItemFormatData;

    // 절 단위 페이지네이션 상태 — PPT 의 SlideNumber 와 대칭.
    // LyricsPageIndex: 현재 보여주는 절 인덱스(0-based). LyricsPageCount: 선택 곡의 총 절 수.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LyricsPageLabel))]
    [NotifyCanExecuteChangedFor(nameof(NextLyricsPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousLyricsPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextSlideCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousSlideCommand))]
    private int _lyricsPageIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LyricsPageLabel))]
    [NotifyCanExecuteChangedFor(nameof(NextLyricsPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousLyricsPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextSlideCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousSlideCommand))]
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
    /// FrmMain flowLayoutPreviewLyrics 대응: 선택 Preview 항목의 가사/성경 페이지 카드.
    /// </summary>
    public ObservableCollection<OperatorLyricsPageCard> PreviewLyricsPages { get; } = new();

    /// <summary>
    /// FrmMain flowLayoutOutputLyrics 대응: 현재 준비/라이브 Output 항목의 가사/성경 페이지 카드.
    /// </summary>
    public ObservableCollection<OperatorLyricsPageCard> OutputLyricsPages { get; } = new();

    public bool HasPreviewLyricsPages => PreviewLyricsPages.Count > 0;

    public bool HasOutputLyricsPages => OutputLyricsPages.Count > 0;

    public string PreviewLyricsText
        => PreviewLyricsPages.FirstOrDefault(card => card.IsCurrent)?.BodyText ?? string.Empty;

    public bool HasPreviewLyricsText => !string.IsNullOrWhiteSpace(PreviewLyricsText);

    public ImageSource? PreviewVisualSource
        => SelectedItem is not null && IsPowerPointItem(SelectedItem)
            ? PowerPoint.PreviewImage
            : SelectedItem?.PreviewSource;

    public bool HasPreviewVisualSource => PreviewVisualSource is not null;

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
    /// 송출(Output) PPT 상태. FrmMain 의 OutputItem 처럼 Preview 선택 변경과 별개로 현재 라이브 덱/슬라이드를 유지한다.
    /// </summary>
    public Rendering.PowerPointPreviewViewModel OutputPowerPoint { get; }

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
        Rendering.PowerPointPreviewViewModel outputPowerPoint,
        LibraryViewModel library,
        BibleViewModel bible,
        SearchUsageViewModel search,
        IWorshipListStore worshipLists,
        IAppearanceTemplateStore appearanceTemplates,
        Data.IAdminSongDetailRepository songDetail,
        IRecentWorshipLists recentWorshipLists,
        WorshipListValidator? worshipValidator = null,
        IPreviewWindowService? preview = null,
        Func<string, bool>? worshipMediaLauncher = null)
    {
        _session = session;
        _output = output;
        // 스테이지 모니터 서비스 — DI 에선 PreviewWindowHost 와 같은 싱글톤이 주입된다(여기서 Open/Close 하면 그 호스트가 창을 띄움).
        // 테스트에서 미주입이면 독립 인스턴스(상태머신만 — 창 호스트 없이 상태 변화만 검증).
        _preview = preview ?? new PreviewWindowService();
        _safetyPrompt = safetyPrompt;
        _telemetry = telemetry;
        _display = display;
        _commandCatalog = commandCatalog;
        _settings = settings;
        _worshipLists = worshipLists;
        _appearanceTemplates = appearanceTemplates;
        _songDetail = songDetail;
        _recentWorshipLists = recentWorshipLists;
        _worshipMediaLauncher = worshipMediaLauncher ?? LaunchWorshipMediaProcess;
        // 예배 순서 검증기 — 기본은 실제 파일 존재(File.Exists). 테스트는 가짜 판정을 주입해 디스크 없이 검증.
        _worshipValidator = worshipValidator ?? new WorshipListValidator();
        Media = media;
        Library = library;
        Bible = bible;
        Search = search;
        PowerPoint = powerPoint;
        OutputPowerPoint = outputPowerPoint;
        // 명령 팔레트(⌘K, §7.4) — 카탈로그를 검색해 ShortcutRegistry 바인딩으로 실행.
        // registry 는 BindShortcuts 에서 주입되므로(앱 시작 시) invoke 는 그 시점 이후 유효하다.
        CommandPalette = new CommandPaletteViewModel(_commandCatalog, id => _shortcutRegistry?.TryInvoke(id) ?? false);

        _session.SessionChanged += (_, e) => ApplyLiveSnapshot(e.Snapshot);
        _output.OutputChanged += OnOutputChanged;
        _preview.PreviewChanged += OnPreviewChanged;
        _settings.SettingsChanged += OnSettingsChanged;
        // 큐가 바뀔 때마다 "비어 있음" 상태를 갱신한다(추가·제거·로드 등 모든 변경 경로를 한 곳에서 반영).
        Queue.CollectionChanged += (_, _) =>
        {
            IsQueueEmpty = Queue.Count == 0;
            // 큐가 비거나 차면 "전체 비우기" 가능 여부가 바뀌므로 명령 활성 상태를 갱신(생성 도중엔 명령이 아직 null 일 수 있어 가드).
            ClearWorshipListCommand?.NotifyCanExecuteChanged();
            // 큐 내용이 바뀌면 미저장 변경으로 표시 — 불러오기(LoadQueue)·저장·시작은 끝에서 false 로 되돌려 깨끗한 상태로 만든다.
            WorshipListHasUnsavedChanges = true;
            // 라이브 항목이 큐에서 빠지면 "현재 송출 항목 선택" 가능 여부도 바뀐다.
            OnPropertyChanged(nameof(CanSelectLiveItem));
            SelectLiveItemCommand?.NotifyCanExecuteChanged();
            // 항목이 들고 나면 "전 항목 서식 지우기" 가능 여부(큐에 지울 서식 있는 항목 유무)도 바뀐다 — RelayCommand 는 수동 통지 필요.
            OnPropertyChanged(nameof(CanClearAllItemsFormatting));
            ClearAllItemsFormattingCommand?.NotifyCanExecuteChanged();
        };
        // PPT 렌더 상태/슬라이드 변화에 슬라이드 이동 커맨드 활성 상태를 맞춘다.
        PowerPoint.PropertyChanged += OnPowerPointPropertyChanged;
        OutputPowerPoint.PropertyChanged += OnOutputPowerPointPropertyChanged;

        OpenOutputCommand = new RelayCommand(OpenOutput);
        CloseOutputCommand = new AsyncRelayCommand(CloseOutputAsync, () => _output.Current.IsOpen);
        OpenStageMonitorCommand = new RelayCommand(OpenStageMonitor);
        CloseStageMonitorCommand = new RelayCommand(CloseStageMonitor, () => IsStageMonitorOpen);
        CopyPreviewToOutputCommand = new RelayCommand(CopyPreviewToOutput, CanCopyPreviewToOutput);
        CopyPreviewToOutputAndNextCommand = new RelayCommand(CopyPreviewToOutputAndNext, CanCopyPreviewToOutput);
        CopyPreviewToOutputShortcutCommand = new RelayCommand(CopyPreviewToOutputShortcut, CanCopyPreviewToOutput);
        CopyPreviewToOutputAndClearBlackCommand = new RelayCommand(CopyPreviewToOutputAndClearBlack, CanCopyPreviewToOutput);
        ShowPreviewTextModeCommand = new RelayCommand(() => ShowPreviewPanelMode(PreviewPanelMode.Text));
        ShowPreviewFormatModeCommand = new RelayCommand(() => ShowPreviewPanelMode(PreviewPanelMode.Format));
        ShowPreviewInfoModeCommand = new RelayCommand(() => ShowPreviewPanelMode(PreviewPanelMode.Info));
        PreviewToLiveCommand = new AsyncRelayCommand(PreviewToLiveAsync, CanPreviewToLive);
        GoLiveCommand = new AsyncRelayCommand(GoLiveAsync, CanGoLive);
        SendToOutputAndNextCommand = new AsyncRelayCommand(SendToOutputAndNextAsync, CanGoLive);
        StopLiveCommand = new AsyncRelayCommand(StopLiveAsync, () => _session.Current.State != LiveState.Off);
        NextItemCommand = new RelayCommand(NextItem, CanMoveNext);
        PreviousItemCommand = new RelayCommand(PreviousItem, CanMovePrevious);
        LiveNextShortcutCommand = new AsyncRelayCommand(() => ExecuteLiveNavigationShortcutAsync(+1));
        LivePreviousShortcutCommand = new AsyncRelayCommand(() => ExecuteLiveNavigationShortcutAsync(-1));
        NextOutputItemCommand = new AsyncRelayCommand(() => MoveOutputItemAsync(+1), CanMoveOutputNext);
        PreviousOutputItemCommand = new AsyncRelayCommand(() => MoveOutputItemAsync(-1), CanMoveOutputPrevious);
        FirstOutputItemCommand = new AsyncRelayCommand(
            () => MoveOutputItemToIndexAsync(0, MainCommandIds.LiveFirst),
            CanMoveOutputPrevious);
        LastOutputItemCommand = new AsyncRelayCommand(
            () => MoveOutputItemToIndexAsync(Queue.Count - 1, MainCommandIds.LiveLast),
            CanMoveOutputNext);
        JumpToNextNonRotateOutputItemCommand = new AsyncRelayCommand(
            JumpToNextNonRotateOutputItemAsync,
            CanJumpToNextNonRotateOutputItem);
        FirstItemCommand = new RelayCommand(FirstItem, CanMovePrevious);
        LastItemCommand = new RelayCommand(LastItem, CanMoveNext);
        HideOutputCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: false), CanUseLiveSafetyAction);
        BlackScreenCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: true), CanUseLiveSafetyAction);
        ClearOutputCommand = new AsyncRelayCommand(ClearOutputAsync, CanUseLiveSafetyAction);
        ToggleOutputBlackCommand = new AsyncRelayCommand(ToggleOutputBlackAsync, CanUseLiveSafetyAction);
        ToggleOutputClearCommand = new AsyncRelayCommand(ToggleOutputClearAsync, CanUseLiveSafetyAction);
        ToggleOutputLiveCommand = new AsyncRelayCommand(ToggleOutputLiveAsync, CanToggleOutputLive);
        SendLiveMessageCommand = new RelayCommand(SendLiveMessage, CanSendLiveMessage);
        ClearLiveMessageCommand = new RelayCommand(ClearLiveMessage, CanClearLiveMessage);
        ToggleOutputReferenceAlertCommand = new RelayCommand(ToggleOutputReferenceAlert, CanToggleOutputReferenceAlert);
        RestoreOutputCommand = new RelayCommand(RestoreOutput, () => _session.Current.State == LiveState.Hidden);
        RestartCurrentItemCommand = new AsyncRelayCommand(RestartCurrentItemAsync, CanRestartCurrentItem);
        RefreshOutputCommand = new RelayCommand(RefreshOutput, () => _output.Current.IsOpen);
        NextSlideCommand = new AsyncRelayCommand(NextPreviewPageAsync, CanGoNextSlide);
        PreviousSlideCommand = new AsyncRelayCommand(PreviousPreviewPageAsync, CanGoPreviousSlide);
        GoToSlideCommand = new AsyncRelayCommand<int>(GoToSlideAsync, CanGoToSlide);
        NextOutputSlideCommand = new AsyncRelayCommand(NextOutputPageAsync, CanGoNextOutputSlide);
        PreviousOutputSlideCommand = new AsyncRelayCommand(PreviousOutputPageAsync, CanGoPreviousOutputSlide);
        GoToOutputSlideCommand = new AsyncRelayCommand<int>(GoToOutputSlideAsync, CanGoToOutputSlide);
        GoToPreviewLyricsPageCommand = new RelayCommand<int>(GoToPreviewLyricsPage, CanGoToPreviewLyricsPage);
        GoToOutputLyricsPageCommand = new RelayCommand<int>(GoToOutputLyricsPage, CanGoToOutputLyricsPage);
        JumpToOutputLyricsSectionCommand = new RelayCommand<string>(JumpToOutputLyricsSection, CanJumpToOutputLyricsSection);
        ApplyOutputAppearanceCommand = new RelayCommand<OutputAppearancePreset>(ApplyOutputAppearance);
        ApplyLyricsAlignmentCommand = new RelayCommand<LyricsTextAlignment>(ApplyLyricsAlignment);
        ApplyLyricsVerticalAlignmentCommand = new RelayCommand<LyricsVerticalAlignment>(ApplyLyricsVerticalAlignment);
        ApplyBackgroundModeCommand = new RelayCommand<LyricsBackgroundMode>(ApplyBackgroundMode);
        ApplyGradientDirectionCommand = new RelayCommand<LyricsGradientDirection>(ApplyGradientDirection);
        ApplyRegionDisplayCommand = new RelayCommand<LyricsRegionDisplay>(ApplyRegionDisplay);
        IncreaseLyricsFontSizeCommand = new RelayCommand(() => StepLyricsFontSize(+LyricsFontSizeStep), () => ActiveLyricsFontSize < LyricsFontSizeMax);
        DecreaseLyricsFontSizeCommand = new RelayCommand(() => StepLyricsFontSize(-LyricsFontSizeStep), () => ActiveLyricsFontSize > LyricsFontSizeMin);
        IncreaseLyricsLineSpacingCommand = new RelayCommand(() => StepLyricsLineSpacing(+LyricsLineSpacingStep), () => ActiveLyricsLineSpacing < LyricsLineSpacingMax);
        DecreaseLyricsLineSpacingCommand = new RelayCommand(() => StepLyricsLineSpacing(-LyricsLineSpacingStep), () => ActiveLyricsLineSpacing > LyricsLineSpacingMin);
        // Display Panel 글자 크기 비율 +/- (FrmMain Def_PanelFont 크기).
        IncreasePanelFontScaleCommand = new RelayCommand(() => StepPanelFontScale(+LyricsPanelFontScaleStep), () => ActivePanelFontScale < LyricsPanelFontScaleMax);
        DecreasePanelFontScaleCommand = new RelayCommand(() => StepPanelFontScale(-LyricsPanelFontScaleStep), () => ActivePanelFontScale > LyricsPanelFontScaleMin);
        // 본문 좌/우/아래 여백 +/- (FrmMain ShowLeftMargin/Right/Bottom) — 폰트·줄간격 증감과 동일 구조.
        IncreaseLyricsLeftMarginCommand = new RelayCommand(() => StepLyricsLeftMargin(+LyricsBodyMarginStep), () => ActiveLyricsLeftMargin < LyricsBodyMarginMax);
        DecreaseLyricsLeftMarginCommand = new RelayCommand(() => StepLyricsLeftMargin(-LyricsBodyMarginStep), () => ActiveLyricsLeftMargin > LyricsBodyMarginMin);
        IncreaseLyricsRightMarginCommand = new RelayCommand(() => StepLyricsRightMargin(+LyricsBodyMarginStep), () => ActiveLyricsRightMargin < LyricsBodyMarginMax);
        DecreaseLyricsRightMarginCommand = new RelayCommand(() => StepLyricsRightMargin(-LyricsBodyMarginStep), () => ActiveLyricsRightMargin > LyricsBodyMarginMin);
        IncreaseLyricsBottomMarginCommand = new RelayCommand(() => StepLyricsBottomMargin(+LyricsBodyMarginStep), () => ActiveLyricsBottomMargin < LyricsBodyMarginMax);
        DecreaseLyricsBottomMarginCommand = new RelayCommand(() => StepLyricsBottomMargin(-LyricsBodyMarginStep), () => ActiveLyricsBottomMargin > LyricsBodyMarginMin);
        // 이중 언어 영역 간 세로 간격 +/- (FrmMain Ind_Reg2TopUpDown).
        IncreaseRegionGapCommand = new RelayCommand(() => StepRegionGap(+LyricsRegionGapStep), () => ActiveRegionGap < LyricsRegionGapMax);
        DecreaseRegionGapCommand = new RelayCommand(() => StepRegionGap(-LyricsRegionGapStep), () => ActiveRegionGap > LyricsRegionGapMin);
        // 본문 세로 위치 이동(FrmMain Ind_Reg1TopUpDown) — 아래(+)/위(-).
        MoveBodyDownCommand = new RelayCommand(() => StepBodyVerticalOffset(+LyricsBodyVOffsetStep), () => ActiveBodyVerticalOffset < LyricsBodyVOffsetMax);
        MoveBodyUpCommand = new RelayCommand(() => StepBodyVerticalOffset(-LyricsBodyVOffsetStep), () => ActiveBodyVerticalOffset > LyricsBodyVOffsetMin);
        ApplyTextColorHexCommand = new RelayCommand<string>(hex => ApplyColorHex(hex, isBackground: false));
        ApplyBackgroundColorHexCommand = new RelayCommand<string>(hex => ApplyColorHex(hex, isBackground: true));
        // 선택한 곡 항목의 글자색·정렬(이 항목만) — 프리셋/전역기본을 우클릭 메뉴에서 적용. 곡일 때만 활성.
        SetSelectedItemTextColorCommand = new RelayCommand<string?>(SetSelectedItemTextColor, _ => CanEditSelectedItemColor);
        SetSelectedItemAlignmentCommand = new RelayCommand<string?>(SetSelectedItemAlignment, _ => CanEditSelectedItemColor);
        SetSelectedItemFontSizeCommand = new RelayCommand<string?>(SetSelectedItemFontSize, _ => CanEditSelectedItemColor);
        SetSelectedItemFontNameCommand = new RelayCommand<string?>(SetSelectedItemFontName, _ => CanEditSelectedItemColor);
        SetSelectedItemBackgroundColorCommand = new RelayCommand<string?>(SetSelectedItemBackgroundColor, _ => CanEditSelectedItemColor);
        SetSelectedItemBackgroundImageCommand = new RelayCommand<string?>(SetSelectedItemBackgroundImage, _ => CanEditSelectedItemColor);
        // 항목별 강조(굵게·기울임·밑줄, FormatData 코드41 비트) — 우클릭 토글. 곡일 때만 활성.
        ToggleSelectedItemBoldCommand = new RelayCommand(ToggleSelectedItemBold, () => CanEditSelectedItemColor);
        ToggleSelectedItemItalicCommand = new RelayCommand(ToggleSelectedItemItalic, () => CanEditSelectedItemColor);
        ToggleSelectedItemUnderlineCommand = new RelayCommand(ToggleSelectedItemUnderline, () => CanEditSelectedItemColor);
        // 이중 언어(보조 영역 Region2) 글자색(FormatData 코드30) — 비우면 본문(Region1) 색 추종. 곡일 때만 활성.
        SetSelectedItemTextColor2Command = new RelayCommand<string?>(SetSelectedItemTextColor2, _ => CanEditSelectedItemColor);
        // 이중 언어(보조 영역 Region2) 가로 정렬(FormatData 코드32) — 비우면 본문(Region1) 정렬 추종. 곡일 때만 활성.
        SetSelectedItemAlignment2Command = new RelayCommand<string?>(SetSelectedItemAlignment2, _ => CanEditSelectedItemColor);
        // 이중 언어(보조 영역 Region2) 글자 크기(코드48)·글꼴(코드44) — 비우면 본문(Region1) 추종. 곡일 때만 활성.
        SetSelectedItemFontSize2Command = new RelayCommand<string?>(SetSelectedItemFontSize2, _ => CanEditSelectedItemColor);
        SetSelectedItemFontName2Command = new RelayCommand<string?>(SetSelectedItemFontName2, _ => CanEditSelectedItemColor);
        // 이중 언어(보조 영역 Region2) 강조(굵게·기울임·밑줄, 코드41 상위비트 bit3/4/5) — 우클릭 토글. 곡일 때만 활성.
        ToggleSelectedItemBold2Command = new RelayCommand(ToggleSelectedItemBold2, () => CanEditSelectedItemColor);
        ToggleSelectedItemItalic2Command = new RelayCommand(ToggleSelectedItemItalic2, () => CanEditSelectedItemColor);
        ToggleSelectedItemUnderline2Command = new RelayCommand(ToggleSelectedItemUnderline2, () => CanEditSelectedItemColor);
        // 이 항목 서식 모두 지우기(레거시 Clear All Formatting) — 항목별 FormatData 전부 제거 → 전역 기본으로 송출. 서식이 있을 때만 활성.
        ClearSelectedItemFormattingCommand = new RelayCommand(ClearSelectedItemFormatting, () => CanClearSelectedItemFormatting);
        // 이 항목 서식 복사/붙여넣기 — 한 항목의 서식을 담아 다른 항목들에 그대로 적용(여러 항목 통일에 편리). 복사는 서식 있을 때, 붙여넣기는 담아둔 서식 있고 대상이 편집 가능할 때.
        CopySelectedItemFormattingCommand = new RelayCommand(CopySelectedItemFormatting, () => CanCopySelectedItemFormatting);
        PasteSelectedItemFormattingCommand = new RelayCommand(PasteSelectedItemFormatting, () => CanPasteSelectedItemFormatting);
        // 복사한 서식을 전 항목(곡·성경)에 일괄 적용 — 담아 둔 서식 있고 대상 항목 있을 때만 활성.
        ApplyCopiedFormatToAllCommand = new RelayCommand(ApplyCopiedFormatToAll, () => CanApplyCopiedFormatToAll);
        // 전 항목 서식 한 번에 지우기 — 큐에 지울 서식 있는 곡·성경 항목이 하나라도 있을 때만 활성(전역 기본으로 일괄 리셋).
        ClearAllItemsFormattingCommand = new RelayCommand(ClearAllItemsFormatting, () => CanClearAllItemsFormatting);
        ApplyPanelColorHexCommand = new RelayCommand<string>(ApplyPanelColorHex);
        SaveAppearanceTemplateCommand = new AsyncRelayCommand(SaveAppearanceTemplateAsync);
        ApplyAppearanceTemplateCommand = new AsyncRelayCommand(ApplyAppearanceTemplateAsync, () => !string.IsNullOrWhiteSpace(SelectedAppearanceTemplate));
        DeleteAppearanceTemplateCommand = new RelayCommand(DeleteAppearanceTemplate, () => !string.IsNullOrWhiteSpace(SelectedAppearanceTemplate));
        ResetOutputAppearanceCommand = new RelayCommand(ResetOutputAppearance);
        // 세션 콤보(예배 순서 빠른 전환) — 콤보에서 고른 저장 목록을 명시적 "불러오기" 버튼으로 적재(자동 적재 안 함 = 실수로 작업물 날림 방지).
        LoadSelectedWorshipListCommand = new AsyncRelayCommand(LoadSelectedWorshipListAsync, () => !string.IsNullOrWhiteSpace(SelectedSavedWorshipList));
        // 빠른 저장(Ctrl+S) — 현재 세션 이름이 있으면 그 이름으로 덮어 저장. 이름이 없으면(한 번도 저장 안 함) 안내만.
        QuickSaveWorshipListCommand = new AsyncRelayCommand(QuickSaveWorshipListAsync);
        // 현재 송출 항목 선택 — 미리보기로 앞서 가다가 라이브 항목으로 선택을 되돌린다(송출 중인 큐 항목 있을 때만).
        SelectLiveItemCommand = new RelayCommand(SelectLiveItem, () => CanSelectLiveItem);
        // FrmMain CMenuWorship_Play — Output 이 아닌 일반 미디어 플레이어/연결 앱으로 연결 미디어를 연다.
        PlaySelectedWorshipMediaCommand = new RelayCommand(
            PlaySelectedWorshipMedia,
            CanPlaySelectedWorshipMedia);
        // FrmMain CMenuWorship_PlayOnOutput — 선택 Worship 항목의 연결 미디어를 Output 창에서 즉시 재생한다.
        PlaySelectedWorshipMediaOnOutputCommand = new RelayCommand(
            PlaySelectedWorshipMediaOnOutput,
            CanPlaySelectedWorshipMediaOnOutput);
        // FrmMain OutputBtnMedia — Preview 선택이 아니라 현재 OutputItem/live 항목의 미디어를 재생/일시정지한다.
        PlayOutputMediaCommand = new RelayCommand(
            PlayOutputMedia,
            CanPlayOutputMedia);
        // 검증 문제 항목 클릭 → 큐에서 그 항목 선택(깨진 항목으로 바로 이동해 고치도록). 항상 클릭 가능(없는 항목은 메서드가 안내).
        SelectWorshipProblemItemCommand = new RelayCommand<WorshipItemProblem?>(SelectWorshipProblemItem);
        RefreshSavedWorshipListNames();
        // 대기 화면(Gap) 빠른 조작 — 페이드 토글·로고 지우기(로고 선택은 View 파일 픽커). 모드는 콤보(GapItemModeInput).
        ToggleGapItemUseFadeCommand = new RelayCommand(ToggleGapItemUseFade);
        ClearGapItemLogoFileCommand = new RelayCommand(() => SetGapItemLogoFile(null));
        ToggleLyricsBoldCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorBold, ActiveLyricsBold));
        ToggleLyricsItalicCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorItalic, ActiveLyricsItalic));
        ToggleLyricsShadowCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShadow, ActiveLyricsShadow));
        // "코드 표시" 토글 — 설정만 바꾸면 OnSettingsChanged 가 라이브 곡을 재송출해 코드 줄을 켜고 끈다(본문 재계산).
        ToggleLyricsNotationsCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowNotations, ActiveLyricsNotations));
        ToggleLyricsUnderlineCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorUnderline, ActiveLyricsUnderline));
        ToggleEmphasisChorusOnlyCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorEmphasisChorusOnly, ActiveLyricsEmphasisChorusOnly));
        ToggleInterlaceCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorInterlace, ActiveLyricsInterlace));
        ToggleUseIndividualFormattingCommand = new RelayCommand(ToggleUseIndividualFormatting, () => SelectedItem is not null);
        ApplyGlobalFormatToAllCommand = new RelayCommand(ApplyGlobalFormatToAll);
        TogglePanelTransparentCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorPanelTransparent, ActiveLyricsPanelTransparent));
        ToggleLyricsPositionIndicatorCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowPositionIndicator, ActiveLyricsPositionIndicator));
        ToggleLyricsVerseHeadingCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowVerseHeading, ActiveLyricsVerseHeading));
        ToggleLyricsItemNumberCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowItemNumber, ActiveLyricsItemNumber));
        ToggleLyricsTitleOnPanelCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowTitleOnPanel, ActiveLyricsTitleOnPanel));
        ToggleLyricsCopyrightCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowCopyright, ActiveLyricsCopyright));
        ToggleLyricsNextItemCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowNextItem, ActiveLyricsNextItem));
        ToggleFadeTransitionCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorUseFadeTransition, ActiveFadeTransition));
        ApplyTransitionDurationCommand = new RelayCommand<int>(ApplyTransitionDuration);
        ApplyTransitionKindCommand = new RelayCommand<LyricsTransitionKind>(ApplyTransitionKind);
        ClearOutputBackgroundImageCommand = new RelayCommand(ClearOutputBackgroundImage);
        OpenRecentWorshipListCommand = new AsyncRelayCommand<string>(OpenRecentWorshipListAsync);
        ValidateWorshipListCommand = new AsyncRelayCommand(ValidateWorshipListAsync);
        // 라이브 조옮김 ↑/↓/원조 — ±반음 이동(±11 클램프) 후 라이브 곡을 재송출해 코드 줄을 다시 그린다.
        TransposeLiveUpCommand = new RelayCommand(() => SetLiveTranspose(LiveTransposeSemitones + 1));
        TransposeLiveDownCommand = new RelayCommand(() => SetLiveTranspose(LiveTransposeSemitones - 1));
        TransposeLiveResetCommand = new RelayCommand(() => SetLiveTranspose(0));
        RefreshRecentWorshipLists();
        ToggleLyricsTitleHeadingCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorShowTitleHeading, ActiveLyricsTitleHeading));
        ToggleLyricsOutlineCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorOutline, ActiveLyricsOutline));
        ApplyTitleHeadingAlignmentCommand = new RelayCommand<LyricsTextAlignment>(ApplyTitleHeadingAlignment);
        ToggleTitleHeadingFirstScreenOnlyCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly, ActiveTitleHeadingFirstScreenOnly));
        ToggleTitleHeadingFollowBodyCommand = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody, ActiveTitleHeadingFollowBody));
        ToggleTitleHeadingFollowRegion2Command = new RelayCommand(() => ToggleLyricsEffect(EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2, ActiveTitleHeadingFollowRegion2));
        ToggleAutoRotateCommand = new RelayCommand(ToggleAutoRotate, () => _session.Current.State == LiveState.Active);
        AddSelectedLibrarySongCommand = new RelayCommand(AddSelectedLibrarySong, () => Library.SelectedSong is not null);
        AddSearchedSongCommand = new AsyncRelayCommand(AddSearchedSongAsync, () => SelectedSearchResult is not null);
        AddLookupTitleCommand = new AsyncRelayCommand(AddLookupTitleAsync, () => SelectedTitleCandidate is not null);
        MoveSelectedItemUpCommand = new RelayCommand(() => MoveSelectedItem(-1), () => CanMoveSelectedItem(-1));
        MoveSelectedItemDownCommand = new RelayCommand(() => MoveSelectedItem(+1), () => CanMoveSelectedItem(+1));
        MoveSelectedItemToTopCommand = new RelayCommand(MoveSelectedItemToTop, () => CanMoveSelectedToBoundary(toTop: true));
        MoveSelectedItemToBottomCommand = new RelayCommand(MoveSelectedItemToBottom, () => CanMoveSelectedToBoundary(toTop: false));
        RemoveSelectedItemCommand = new RelayCommand(RemoveSelectedItem, () => SelectedItem is not null);
        DuplicateSelectedItemCommand = new RelayCommand(() => DuplicateSelectedItem(), () => SelectedItem is not null);
        ClearWorshipListCommand = new RelayCommand(ClearWorshipList, () => Queue.Count > 0);
        RestoreClearedWorshipListCommand = new RelayCommand(RestoreClearedWorshipList, () => _clearedWorshipBackup.Count > 0);
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
        RefreshPreviewDisplays();
        IsStageMonitorOpen = _preview.Current.IsOpen;
        RefreshAppearanceTemplateNames();
    }

    public ObservableCollection<LiveQueueItem> Queue { get; } = new();
    public ObservableCollection<OutputDisplay> OutputDisplays { get; } = new();
    // 스테이지(Preview) 모니터 선택 후보 — 출력 모니터 목록과 같은 방식(IDisplayService)으로 채운다.
    public ObservableCollection<OutputDisplay> PreviewDisplays { get; } = new();

    public LiveBarViewModel LiveBar { get; } = new();

    public ILiveSessionService Session => _session;

    public bool IsPreviewTextMode => PreviewPanelMode == PreviewPanelMode.Text;

    public bool IsPreviewFormatMode => PreviewPanelMode == PreviewPanelMode.Format;

    public bool IsPreviewInfoMode => PreviewPanelMode == PreviewPanelMode.Info;

    public string PreviewItemInfoText => BuildPreviewItemInfoText(SelectedItem);

    public IRelayCommand OpenOutputCommand { get; }
    public IAsyncRelayCommand CloseOutputCommand { get; }
    // 스테이지(Preview) 모니터 열기/닫기 — 선택한 모니터에 풀스크린으로 띄우거나 닫는다(회중 출력엔 영향 없음).
    public IRelayCommand OpenStageMonitorCommand { get; }
    public IRelayCommand CloseStageMonitorCommand { get; }
    /// <summary>FrmMain btnToOutput: PreviewItem 을 OutputItem 으로 복사하되 라이브 송출은 시작하지 않는다.</summary>
    public IRelayCommand CopyPreviewToOutputCommand { get; }
    /// <summary>FrmMain btnToOutputMoveNext: PreviewItem 을 OutputItem 으로 복사하고 Preview 선택만 다음 항목으로 옮긴다.</summary>
    public IRelayCommand CopyPreviewToOutputAndNextCommand { get; }
    /// <summary>FrmMain 글로벌 F8: PreviewItem 을 OutputItem 으로 복사하고, 라이브 중이면 현재 Preview 를 재송출한다.</summary>
    public IRelayCommand CopyPreviewToOutputShortcutCommand { get; }
    /// <summary>FrmMain 글로벌 F7: PreviewItem 을 OutputItem 으로 복사하고 Black 화면을 해제한다.</summary>
    public IRelayCommand CopyPreviewToOutputAndClearBlackCommand { get; }
    /// <summary>FrmMain IndradioButtonText: Preview 상단을 가사 텍스트 보기로 전환한다.</summary>
    public IRelayCommand ShowPreviewTextModeCommand { get; }
    /// <summary>FrmMain IndradioButtonFormat: Preview 상단을 항목별 서식(IndPanel) 보기로 전환한다.</summary>
    public IRelayCommand ShowPreviewFormatModeCommand { get; }
    /// <summary>FrmMain IndradioButtonInfo: Preview 상단을 항목 정보(PreviewInfo) 보기로 전환한다.</summary>
    public IRelayCommand ShowPreviewInfoModeCommand { get; }
    /// <summary>FrmMain btnToLive: PreviewItem 을 OutputItem 으로 복사하고, live off 면 시작, live 중이면 현재 Output 을 갱신한다.</summary>
    public IAsyncRelayCommand PreviewToLiveCommand { get; }
    public IAsyncRelayCommand GoLiveCommand { get; }

    /// <summary>상단/메뉴 F11: 선택 항목을 라이브로 송출하고 곧바로 다음 항목으로 넘어간다(자동 다음 설정과 무관).</summary>
    public IAsyncRelayCommand SendToOutputAndNextCommand { get; }
    public IAsyncRelayCommand StopLiveCommand { get; }
    public IRelayCommand NextItemCommand { get; }
    public IRelayCommand PreviousItemCommand { get; }
    /// <summary>FrmMain Space/F5: 라이브 중에는 Output 슬라이드/절을 우선 이동하고, 끝에서는 설정에 따라 다음 Output 항목으로 진행한다.</summary>
    public IAsyncRelayCommand LiveNextShortcutCommand { get; }
    /// <summary>FrmMain Shift+Space/F4: 라이브 중에는 Output 슬라이드/절을 우선 되돌리고, 처음에서는 설정에 따라 이전 Output 항목으로 진행한다.</summary>
    public IAsyncRelayCommand LivePreviousShortcutCommand { get; }
    /// <summary>FrmMain OutputBtnItemDown: Preview 선택과 별개로 현재 OutputItem/live 항목을 다음 항목으로 이동한다.</summary>
    public IAsyncRelayCommand NextOutputItemCommand { get; }
    /// <summary>FrmMain OutputBtnItemUp: Preview 선택과 별개로 현재 OutputItem/live 항목을 이전 항목으로 이동한다.</summary>
    public IAsyncRelayCommand PreviousOutputItemCommand { get; }
    /// <summary>FrmMain Output PPT focus Home: Preview 선택과 별개로 현재 Output/live context 를 첫 항목으로 이동한다.</summary>
    public IAsyncRelayCommand FirstOutputItemCommand { get; }
    /// <summary>FrmMain Output PPT focus End: Preview 선택과 별개로 현재 Output/live context 를 마지막 항목으로 이동한다.</summary>
    public IAsyncRelayCommand LastOutputItemCommand { get; }
    /// <summary>FrmMain OutputBtnJumpToNonRotate: 현재 OutputItem/live 이후의 다음 비회전 항목으로 이동한다.</summary>
    public IAsyncRelayCommand JumpToNextNonRotateOutputItemCommand { get; }
    /// <summary>예배 순서의 첫 항목으로 이동(레거시 First). 라이브 중이면 그 항목을 송출.</summary>
    public IRelayCommand FirstItemCommand { get; }
    /// <summary>예배 순서의 마지막 항목으로 이동(레거시 Last). 라이브 중이면 그 항목을 송출.</summary>
    public IRelayCommand LastItemCommand { get; }
    public IAsyncRelayCommand HideOutputCommand { get; }
    public IAsyncRelayCommand BlackScreenCommand { get; }
    public IAsyncRelayCommand ClearOutputCommand { get; }
    /// <summary>FrmMain cbOutputBlack: 체크하면 Black, 체크 해제하면 Restore.</summary>
    public IAsyncRelayCommand ToggleOutputBlackCommand { get; }
    /// <summary>FrmMain cbOutputClear: 체크하면 Clear, 체크 해제하면 Restore.</summary>
    public IAsyncRelayCommand ToggleOutputClearCommand { get; }
    /// <summary>FrmMain cbGoLive: Off 에서는 GoLive, Hidden 에서는 Restore, Active 에서는 Stop.</summary>
    public IAsyncRelayCommand ToggleOutputLiveCommand { get; }
    public IRelayCommand SendLiveMessageCommand { get; }
    public IRelayCommand ClearLiveMessageCommand { get; }
    public IRelayCommand ToggleOutputReferenceAlertCommand { get; }
    public IRelayCommand RestoreOutputCommand { get; }
    public IAsyncRelayCommand RestartCurrentItemCommand { get; }
    public IRelayCommand RefreshOutputCommand { get; }
    public IAsyncRelayCommand NextSlideCommand { get; }
    public IAsyncRelayCommand PreviousSlideCommand { get; }
    public IAsyncRelayCommand<int> GoToSlideCommand { get; }
    public IAsyncRelayCommand NextOutputSlideCommand { get; }
    public IAsyncRelayCommand PreviousOutputSlideCommand { get; }
    public IAsyncRelayCommand<int> GoToOutputSlideCommand { get; }
    public IRelayCommand<int> GoToPreviewLyricsPageCommand { get; }
    public IRelayCommand<int> GoToOutputLyricsPageCommand { get; }
    public IRelayCommand<string> JumpToOutputLyricsSectionCommand { get; }
    public IRelayCommand<OutputAppearancePreset> ApplyOutputAppearanceCommand { get; }
    public IRelayCommand<LyricsTextAlignment> ApplyLyricsAlignmentCommand { get; }
    public IRelayCommand<LyricsVerticalAlignment> ApplyLyricsVerticalAlignmentCommand { get; }

    /// <summary>배경 이미지 표시 모드(채움/맞춤/가운데/타일) 적용 — 설정→출력 VM 라이브 반영(레거시 Def_ImageMode).</summary>
    public IRelayCommand<LyricsBackgroundMode> ApplyBackgroundModeCommand { get; }

    public bool BackgroundModeIsFill => ActiveBackgroundMode == LyricsBackgroundMode.Fill;
    public bool BackgroundModeIsFit => ActiveBackgroundMode == LyricsBackgroundMode.Fit;
    public bool BackgroundModeIsCenter => ActiveBackgroundMode == LyricsBackgroundMode.Center;
    public bool BackgroundModeIsTile => ActiveBackgroundMode == LyricsBackgroundMode.Tile;

    public IRelayCommand<LyricsGradientDirection> ApplyGradientDirectionCommand { get; }

    public bool GradientDirectionIsVertical => ActiveGradientDirection == LyricsGradientDirection.Vertical;
    public bool GradientDirectionIsHorizontal => ActiveGradientDirection == LyricsGradientDirection.Horizontal;
    public bool GradientDirectionIsDiagonalDown => ActiveGradientDirection == LyricsGradientDirection.DiagonalDown;
    public bool GradientDirectionIsDiagonalUp => ActiveGradientDirection == LyricsGradientDirection.DiagonalUp;

    /// <summary>이중 언어 영역 표시 모드(둘다/Region1만/Region2만) 적용 — 설정→출력 VM 라이브 반영(레거시 Def_ShowRegion).</summary>
    public IRelayCommand<LyricsRegionDisplay> ApplyRegionDisplayCommand { get; }

    public bool RegionDisplayIsBoth => ActiveRegionDisplay == LyricsRegionDisplay.Both;
    public bool RegionDisplayIsRegion1Only => ActiveRegionDisplay == LyricsRegionDisplay.Region1Only;
    public bool RegionDisplayIsRegion2Only => ActiveRegionDisplay == LyricsRegionDisplay.Region2Only;
    public IRelayCommand IncreaseLyricsFontSizeCommand { get; }
    public IRelayCommand DecreaseLyricsFontSizeCommand { get; }
    public IRelayCommand IncreaseLyricsLineSpacingCommand { get; }
    public IRelayCommand DecreaseLyricsLineSpacingCommand { get; }
    public IRelayCommand IncreasePanelFontScaleCommand { get; }
    public IRelayCommand DecreasePanelFontScaleCommand { get; }
    // 본문 좌/우/아래 여백 +/- (FrmMain ShowLeftMargin/Right/Bottom).
    public IRelayCommand IncreaseLyricsLeftMarginCommand { get; }
    public IRelayCommand DecreaseLyricsLeftMarginCommand { get; }
    public IRelayCommand IncreaseLyricsRightMarginCommand { get; }
    public IRelayCommand DecreaseLyricsRightMarginCommand { get; }
    public IRelayCommand IncreaseLyricsBottomMarginCommand { get; }
    public IRelayCommand DecreaseLyricsBottomMarginCommand { get; }
    public IRelayCommand IncreaseRegionGapCommand { get; }
    public IRelayCommand DecreaseRegionGapCommand { get; }
    public IRelayCommand MoveBodyDownCommand { get; }
    public IRelayCommand MoveBodyUpCommand { get; }
    public IRelayCommand<string> ApplyTextColorHexCommand { get; }
    public IRelayCommand<string> ApplyBackgroundColorHexCommand { get; }
    public IRelayCommand<string?> SetSelectedItemTextColorCommand { get; }
    public IRelayCommand<string?> SetSelectedItemAlignmentCommand { get; }
    public IRelayCommand<string?> SetSelectedItemFontSizeCommand { get; }
    public IRelayCommand<string?> SetSelectedItemFontNameCommand { get; }
    public IRelayCommand<string?> SetSelectedItemBackgroundColorCommand { get; }
    public IRelayCommand<string?> SetSelectedItemBackgroundImageCommand { get; }
    public IRelayCommand ToggleSelectedItemBoldCommand { get; }
    public IRelayCommand ToggleSelectedItemItalicCommand { get; }
    public IRelayCommand ToggleSelectedItemUnderlineCommand { get; }
    public IRelayCommand<string?> SetSelectedItemTextColor2Command { get; }
    public IRelayCommand<string?> SetSelectedItemAlignment2Command { get; }
    public IRelayCommand<string?> SetSelectedItemFontSize2Command { get; }
    public IRelayCommand<string?> SetSelectedItemFontName2Command { get; }
    public IRelayCommand ToggleSelectedItemBold2Command { get; }
    public IRelayCommand ToggleSelectedItemItalic2Command { get; }
    public IRelayCommand ToggleSelectedItemUnderline2Command { get; }

    /// <summary>이 항목의 항목별 서식(색·정렬·크기·글꼴·배경·강조)을 모두 지워 전역 기본으로 되돌린다(레거시 Clear All Formatting).</summary>
    public IRelayCommand ClearSelectedItemFormattingCommand { get; }

    /// <summary>이 항목의 항목별 서식을 클립보드처럼 담아 둔다(다른 항목에 "붙여넣기"로 그대로 적용).</summary>
    public IRelayCommand CopySelectedItemFormattingCommand { get; }

    /// <summary>담아 둔 항목별 서식을 이 항목에 그대로 적용한다(여러 항목 서식 통일).</summary>
    public IRelayCommand PasteSelectedItemFormattingCommand { get; }

    /// <summary>복사해 둔 항목별 서식을 예배 순서의 모든 곡·성경 항목에 한 번에 적용한다.</summary>
    public IRelayCommand ApplyCopiedFormatToAllCommand { get; }

    /// <summary>예배 순서의 모든 곡·성경 항목에서 항목별 서식을 한 번에 지워 전역 기본으로 되돌린다(레거시 Clear All Formatting 전체판).</summary>
    public IRelayCommand ClearAllItemsFormattingCommand { get; }

    public IRelayCommand<string> ApplyPanelColorHexCommand { get; }
    public IAsyncRelayCommand SaveAppearanceTemplateCommand { get; }
    public IAsyncRelayCommand ApplyAppearanceTemplateCommand { get; }
    public IRelayCommand DeleteAppearanceTemplateCommand { get; }
    public IRelayCommand ResetOutputAppearanceCommand { get; }
    public IAsyncRelayCommand LoadSelectedWorshipListCommand { get; }

    /// <summary>빠른 저장(Ctrl+S) — 현재 세션 이름으로 덮어 저장(이름 없으면 안내). 미저장 변경 표시(● 수정됨)와 짝.</summary>
    public IAsyncRelayCommand QuickSaveWorshipListCommand { get; }

    /// <summary>현재 송출 중인 큐 항목을 선택한다 — 미리보기로 앞서 본 뒤 라이브 항목으로 되돌릴 때(LIVE 배지 항목으로 점프).</summary>
    public IRelayCommand SelectLiveItemCommand { get; }

    /// <summary>"현재 송출 항목 선택"을 누를 수 있는지 — 라이브 송출 중이고 그 항목이 큐에 있을 때만(공지 센티넬은 큐에 없어 false).</summary>
    public bool CanSelectLiveItem => Queue.Any(q => string.Equals(q.Id, _liveItemId, StringComparison.Ordinal));

    /// <summary>FrmMain CMenuWorship_Play: 선택 Worship 항목의 미디어를 기본 앱으로 연다.</summary>
    public IRelayCommand PlaySelectedWorshipMediaCommand { get; }

    /// <summary>FrmMain CMenuWorship_PlayOnOutput: 선택 Worship 항목의 미디어를 Output 창에서 재생한다.</summary>
    public IRelayCommand PlaySelectedWorshipMediaOnOutputCommand { get; }

    /// <summary>FrmMain OutputBtnMedia: 현재 Output/live 항목의 미디어를 Output 창에서 재생/일시정지한다.</summary>
    public IRelayCommand PlayOutputMediaCommand { get; }

    /// <summary>검증 문제 항목을 클릭하면 그 항목을 예배 순서에서 선택한다 — 운영자가 깨진 항목으로 바로 가 고치게(수동 스크롤 없이).</summary>
    public IRelayCommand<WorshipItemProblem?> SelectWorshipProblemItemCommand { get; }
    public IRelayCommand ToggleGapItemUseFadeCommand { get; }
    public IRelayCommand ClearGapItemLogoFileCommand { get; }
    public IRelayCommand ToggleLyricsBoldCommand { get; }
    public IRelayCommand ToggleLyricsItalicCommand { get; }
    public IRelayCommand ToggleLyricsShadowCommand { get; }
    public IRelayCommand ToggleLyricsNotationsCommand { get; }

    public IRelayCommand ToggleLyricsUnderlineCommand { get; }

    public IRelayCommand ToggleEmphasisChorusOnlyCommand { get; }

    public IRelayCommand ToggleInterlaceCommand { get; }

    /// <summary>선택 항목의 "개별 서식 사용"을 토글한다(레거시 Ind_checkBox). off 면 전역 기본 서식으로 송출.</summary>
    public IRelayCommand ToggleUseIndividualFormattingCommand { get; }
    public IRelayCommand ApplyGlobalFormatToAllCommand { get; }
    /// <summary>Display Panel 배경 투명 토글(레거시 Def_PanelTransparent) — 설정→출력 VM 라이브 반영.</summary>
    public IRelayCommand TogglePanelTransparentCommand { get; }
    public IRelayCommand ToggleLyricsPositionIndicatorCommand { get; }
    public IRelayCommand ToggleLyricsVerseHeadingCommand { get; }
    public IRelayCommand ToggleLyricsItemNumberCommand { get; }
    public IRelayCommand ToggleLyricsTitleOnPanelCommand { get; }
    public IRelayCommand ToggleLyricsCopyrightCommand { get; }
    public IRelayCommand ToggleLyricsNextItemCommand { get; }
    public IRelayCommand ToggleFadeTransitionCommand { get; }
    public IRelayCommand<int> ApplyTransitionDurationCommand { get; }
    public IRelayCommand<LyricsTransitionKind> ApplyTransitionKindCommand { get; }
    public IRelayCommand ClearOutputBackgroundImageCommand { get; }
    public IAsyncRelayCommand<string> OpenRecentWorshipListCommand { get; }

    /// <summary>최근 연/저장한 예배 순서 이름(최신순) — 파일 메뉴 "최근 예배 순서" 서브메뉴 바인딩(레거시 Recent Edits).</summary>
    public ObservableCollection<string> RecentWorshipLists { get; } = new();

    /// <summary>저장된 모든 예배 순서 이름(가나다순) — 예배 순서 패널 세션 콤보 바인딩(레거시 FrmMain 세션 콤보). 콤보 열 때 새로고침된다.</summary>
    public ObservableCollection<string> SavedWorshipListNames { get; } = new();

    /// <summary>세션 콤보에서 고른 저장 예배 순서 이름. "불러오기" 버튼(LoadSelectedWorshipListCommand)으로 명시적 적재.</summary>
    [ObservableProperty]
    private string? _selectedSavedWorshipList;

    /// <summary>예배 순서 검증으로 발견한 문제 목록(없으면 비어 있음) — 도구 메뉴 "예배 순서 검증" 결과.</summary>
    public ObservableCollection<WorshipItemProblem> WorshipListProblems { get; } = new();

    /// <summary>예배 순서 검증을 실행한다(라이브 송출 전 깨진 PPT·미디어 파일 + 곡 DB 존재 점검, 레거시 ValidateWorshipListItems).</summary>
    public IAsyncRelayCommand ValidateWorshipListCommand { get; }

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
    public IRelayCommand ToggleTitleHeadingFollowBodyCommand { get; }
    public IRelayCommand ToggleTitleHeadingFollowRegion2Command { get; }
    public IRelayCommand ToggleAutoRotateCommand { get; }
    public IRelayCommand AddSelectedLibrarySongCommand { get; }
    public IAsyncRelayCommand AddSearchedSongCommand { get; }
    public IAsyncRelayCommand AddLookupTitleCommand { get; }
    public IRelayCommand MoveSelectedItemUpCommand { get; }
    public IRelayCommand MoveSelectedItemDownCommand { get; }
    public IRelayCommand MoveSelectedItemToTopCommand { get; }
    public IRelayCommand MoveSelectedItemToBottomCommand { get; }
    public IRelayCommand RemoveSelectedItemCommand { get; }
    public IRelayCommand DuplicateSelectedItemCommand { get; }
    public IRelayCommand ClearWorshipListCommand { get; }
    public IRelayCommand RestoreClearedWorshipListCommand { get; }
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
        // 전체 교체(불러오기/시작 시드)는 "깨끗한" 상태 — 위 Add 들이 켠 미저장 표시를 끈다(사용자 편집만 미저장으로 남게).
        WorshipListHasUnsavedChanges = false;
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
    /// 자유 텍스트(공지/안내) 항목을 예배 순서(큐)에 추가한다(레거시 InfoScreen 항목). 선택 항목 뒤에 끼우고 새 항목을 선택.
    /// 송출(GoLive)하면 공지 렌더 경로로 그 텍스트가 회중 화면에 표시된다 — NoticeScreen 즉시 송출과 달리 예배 순서에 저장돼
    /// 나중에 다른 항목처럼 골라 송출할 수 있다. 항목 종류는 Notice(공지)라 절·슬라이드 이동과 무관하다.
    /// </summary>
    public LiveQueueItem? AddTextItem(string? text, NoticeOptions? options = null)
    {
        var item = CreateTextQueueItem(text, options);
        if (item is null)
        {
            NotifyCommandStates();
            return null;
        }

        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"텍스트 항목 추가됨: {item.Title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>
    /// 자유 텍스트(공지/안내) 항목을 드롭 위치(타깃 항목) 앞에 끼운다 — InfoScr source drag/drop path.
    /// 타깃이 없으면 맨 끝에 추가한다.
    /// </summary>
    public LiveQueueItem? AddTextItemRelativeTo(string? text, NoticeOptions? options, LiveQueueItem? targetItem)
    {
        var item = CreateTextQueueItem(text, options);
        if (item is null)
        {
            NotifyCommandStates();
            return null;
        }

        var targetIndex = targetItem is null ? -1 : IndexOfReference(targetItem);
        var insertIndex = targetIndex >= 0 ? targetIndex : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"텍스트 항목 추가됨: {item.Title}";
        NotifyCommandStates();
        return item;
    }

    private LiveQueueItem? CreateTextQueueItem(string? text, NoticeOptions? options)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            StatusText = "추가할 텍스트가 없습니다.";
            return null;
        }

        var trimmed = text.Trim();
        var title = BuildTextItemTitle(trimmed);
        // 공지 편집기에서 정한 서식(크기·정렬·색·배경·강조·글꼴)을 곡 FormatData 파이프라인으로 인코드해 항목에 실어 둔다.
        // 그래야 "송출"뿐 아니라 "순서에 추가"로 큐에 넣은 항목도 나중에 송출될 때 같은 서식으로 보인다(송출 경로는 Kind 무관).
        // 서식이 없으면(options 미지정 또는 전부 기본) null → 전역 기본 서식으로 송출(무회귀). UseIndividualFormatting 기본 true 라 서식이 있으면 그대로 렌더.
        var formatData = options is null ? null : BuildNoticeFormatData(options);
        // 고유 Id(센티넬 NoticeLiveId 와 겹치지 않게) — 큐 내 다른 항목과 구별되는 새 식별자.
        return new LiveQueueItem($"text:{Guid.NewGuid():N}", title, LiveItemKinds.Notice)
        {
            Lyrics = trimmed,
            FormatData = formatData,
        };
    }

    /// <summary>
    /// Word 문서에서 추출한 본문 텍스트를 예배 순서에 텍스트(공지) 항목으로 추가한다(레거시 Word 항목 — OfficeLib.WordDoc 추출).
    /// 추출 결과가 비어 있으면(Word 미설치·읽기 실패·빈 문서) 항목을 만들지 않고 안내만 한다. 본문 적재는 검증된 AddTextItem 재사용.
    /// (Word 본문 추출 자체는 인터롭이라 View 가 담당하고, 이 메서드는 그 결과를 받아 판단·적재만 한다.)
    /// </summary>
    public LiveQueueItem? AddWordTextItem(string? extractedText)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            StatusText = "Word 문서를 읽지 못했습니다(Word 설치 필요 또는 빈 문서).";
            NotifyCommandStates();
            return null;
        }

        return AddTextItem(extractedText);
    }

    // 큐 목록 표시용 짧은 제목 — 첫 줄을 쓰되 너무 길면 줄여 표시(본문 전체는 Lyrics 에 보존).
    private static string BuildTextItemTitle(string text)
    {
        var firstLine = text.Replace("\r", string.Empty).Split('\n')[0].Trim();
        if (firstLine.Length == 0)
        {
            return "텍스트";
        }

        const int max = 30;
        return firstLine.Length <= max ? firstLine : firstLine[..max] + "…";
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

    /// <summary>
    /// 라이브러리 곡을 드롭한 위치(타깃 항목) 앞에 끼운다 — 곡 목록에서 큐로 끌어다 놓는 드래그-드롭 경로(레거시 외부 소스 드래그).
    /// 타깃이 없으면(빈 공간) 맨 끝에. 항목 생성은 AddSong 과 동일하지만 삽입 위치만 타깃 기준이다(라이브러리 곡은 가사를 이미 들고 있어 동기).
    /// </summary>
    public LiveQueueItem? AddSongRelativeTo(Data.SongSummary? song, LiveQueueItem? targetItem, string? sequence = null, string? formatData = null)
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
            SongNumber = song.SongNumber,
            Copyright = song.Copyright,
            FormatData = formatData,
        };

        // 참조 일치(IndexOfReference)로 "드롭한 바로 그 인스턴스"의 위치를 찾는다 — 성경 드롭·재정렬과 동일 규칙(같은-값 중복 안전).
        var targetIndex = targetItem is null ? -1 : IndexOfReference(targetItem);
        var insertIndex = targetIndex >= 0 ? targetIndex : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"곡 추가됨: {song.Title}";
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
    /// 예배 순서에서 선택 중인 DB 곡 항목을 곡 편집기 저장 결과로 갱신한다(FrmMain CMenuWorship_Edit → Edit item 대응).
    /// 같은 곡이 여러 번 들어간 예배 순서에서도 선택한 인스턴스만 바꾸기 위해 참조 일치로 위치를 찾는다.
    /// </summary>
    public bool UpdateSelectedSongQueueItem(Data.SongSummary song, string? sequence, string? formatData)
    {
        ArgumentNullException.ThrowIfNull(song);

        if (SelectedItem is not { Kind: LiveItemKinds.Song } item || !TryGetSongDatabaseId(item, out var selectedSongId))
        {
            StatusText = "편집할 곡 항목을 선택하세요.";
            NotifyCommandStates();
            return false;
        }

        if (selectedSongId != song.SongId)
        {
            StatusText = "편집한 곡과 선택 항목이 일치하지 않습니다.";
            NotifyCommandStates();
            return false;
        }

        var index = IndexOfReference(item);
        if (index < 0)
        {
            StatusText = "선택 항목을 예배 순서에서 찾을 수 없습니다.";
            NotifyCommandStates();
            return false;
        }

        var normalizedSequence = string.IsNullOrWhiteSpace(sequence) ? null : sequence.Trim();
        var editorFormatData = string.IsNullOrWhiteSpace(formatData) ? null : formatData;
        var updated = item with
        {
            Title = song.Title,
            Lyrics = song.Lyrics,
            Sequence = normalizedSequence,
            SongNumber = song.SongNumber,
            Copyright = song.Copyright,
            // 이미 예배순서 항목에 서식이 있으면 .esw/항목별 override 로 보고 보존한다.
            // 서식이 비어 있던 항목만 DB 곡 편집기의 FormatData 를 따라간다.
            FormatData = string.IsNullOrWhiteSpace(item.FormatData) ? editorFormatData : item.FormatData,
        };

        Queue[index] = updated;
        SelectedItem = updated;
        RepublishLiveSongForBodyChange();
        NotifyCommandStates();
        StatusText = $"예배 순서 항목 갱신: {song.Title}";
        return true;
    }

    public bool CanAddWorshipListSongsToUsages
        => Queue.Any(item => item is { Kind: LiveItemKinds.Song } && TryGetSongDatabaseId(item, out _));

    /// <summary>
    /// FrmMain AddToUsages 대응: 현재 Worship List 전체를 훑어 DB 곡(song:{id})만 USAGE DB 에 기록한다.
    /// 컨텍스트 메뉴가 선택 행에서 열려도 legacy 는 전체 목록을 기록하므로 WPF 도 같은 범위를 유지한다.
    /// </summary>
    public async Task<int> AddWorshipListSongsToUsagesAsync()
    {
        if (Queue.Count == 0)
        {
            StatusText = "현재 예배 순서에 항목이 없습니다.";
            NotifyCommandStates();
            return 0;
        }

        var songItems = Queue
            .Where(item => item is { Kind: LiveItemKinds.Song } && TryGetSongDatabaseId(item, out _))
            .ToArray();
        if (songItems.Length == 0)
        {
            StatusText = "예배 순서에 DB 곡 항목이 없습니다.";
            NotifyCommandStates();
            return 0;
        }

        var records = await BuildUsageAddRecordsAsync(songItems).ConfigureAwait(true);
        var report = await Search.AddUsageRecordsAsync(records).ConfigureAwait(true);
        if (!report.Succeeded)
        {
            var detail = string.Join(" ", report.Issues.Select(issue => issue.Message).Where(message => !string.IsNullOrWhiteSpace(message)));
            StatusText = string.IsNullOrWhiteSpace(detail)
                ? "사용 이력 기록 실패"
                : $"사용 이력 기록 실패: {detail}";
            NotifyCommandStates();
            return 0;
        }

        StatusText = report.AddedCount == 1
            ? "사용 이력에 DB 곡 1개 추가됨"
            : $"사용 이력에 DB 곡 {report.AddedCount}개 추가됨";
        NotifyCommandStates();
        return report.AddedCount;
    }

    private async Task<IReadOnlyList<UsageAddRecord>> BuildUsageAddRecordsAsync(IReadOnlyList<LiveQueueItem> songItems)
    {
        var databasePath = ResolveSongDetailDatabasePath();
        var detailCache = new Dictionary<int, Data.SongDetail?>();
        var sessionName = ResolveUsageSessionName();
        var records = new List<UsageAddRecord>(songItems.Count);

        foreach (var item in songItems)
        {
            if (item is not { Kind: LiveItemKinds.Song } || !TryGetSongDatabaseId(item, out var songId))
            {
                continue;
            }

            Data.SongDetail? detail = null;
            if (!string.IsNullOrWhiteSpace(databasePath))
            {
                if (!detailCache.TryGetValue(songId, out detail))
                {
                    detail = await _songDetail.GetSongDetailAsync(databasePath, songId).ConfigureAwait(true);
                    detailCache[songId] = detail;
                }
            }

            records.Add(new UsageAddRecord(
                DateTime.Today,
                sessionName,
                item.Title,
                item.SongNumber != 0 ? item.SongNumber : detail?.SongNumber ?? 0,
                songId,
                detail?.LicenceAdmin1 ?? "",
                detail?.LicenceAdmin2 ?? ""));
        }

        return records;
    }

    private string ResolveUsageSessionName()
    {
        var sessionName = SelectedSavedWorshipList;
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            sessionName = CurrentWorshipListName;
        }

        return sessionName?.Trim() ?? "";
    }

    /// <summary>
    /// 찬양집 색인에서 더블클릭한 곡을 예배 순서에 추가한다(FrmMain PraiseBook 인터랙티브 목록 대응).
    /// 색인 항목엔 가사가 없으므로 현재 라이브러리에서 같은 곡(가사 포함 SongSummary)을 찾아 AddSong 으로 넘긴다.
    /// 해석 우선순위: ① SongId(있으면 정확 — 같은 제목·번호의 다른 곡/언어를 안전히 가름) → ② 제목+번호 → ③ 제목만
    /// (저장된 찬양집은 SongId=0 이라 ②③으로 폴백). 라이브러리에 같은 곡이 없으면 안내만 하고 큐는 그대로 둔다.
    /// </summary>
    public LiveQueueItem? AddPraiseBookSong(string? title, int songNumber, int songId = 0)
    {
        var song = ResolvePraiseBookSong(title, songNumber, songId);
        return song is null ? null : AddSong(song);
    }

    public async Task<LiveQueueItem?> AddPraiseBookSongAsync(PraiseBookIndexEntry? entry)
    {
        var resolved = await ResolvePraiseBookSongAsync(entry).ConfigureAwait(true);
        if (resolved is null)
        {
            return null;
        }

        return AddSong(resolved.Song, resolved.Sequence, resolved.FormatData);
    }

    /// <summary>
    /// 하단 PraiseBook 항목을 예배 순서의 드롭 위치 앞에 끼운다(FrmMain PraiseBookItems → WorshipList drag/drop 대응).
    /// 곡 해석은 더블클릭 경로와 동일하게 SongId/제목+번호/제목 폴백을 쓰고, 삽입 위치만 타깃 항목 기준으로 보존한다.
    /// </summary>
    public LiveQueueItem? AddPraiseBookSongRelativeTo(PraiseBookIndexEntry? entry, LiveQueueItem? targetItem)
    {
        if (entry is null)
        {
            StatusText = "선택된 곡이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        var song = ResolvePraiseBookSong(entry.Title, entry.Number, entry.SongId);
        return song is null ? null : AddSongRelativeTo(song, targetItem);
    }

    public async Task<LiveQueueItem?> AddPraiseBookSongRelativeToAsync(PraiseBookIndexEntry? entry, LiveQueueItem? targetItem)
    {
        var resolved = await ResolvePraiseBookSongAsync(entry).ConfigureAwait(true);
        if (resolved is null)
        {
            return null;
        }

        return AddSongRelativeTo(resolved.Song, targetItem, resolved.Sequence, resolved.FormatData);
    }

    private async Task<ResolvedPraiseBookSong?> ResolvePraiseBookSongAsync(PraiseBookIndexEntry? entry)
    {
        if (entry is null || string.IsNullOrWhiteSpace(entry.Title))
        {
            StatusText = "선택된 곡이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        if (entry.SongId != 0)
        {
            var databasePath = ResolveSongDetailDatabasePath();
            if (!string.IsNullOrWhiteSpace(databasePath))
            {
                var detail = await _songDetail.GetSongDetailAsync(databasePath, entry.SongId).ConfigureAwait(true);
                if (detail is not null)
                {
                    return new ResolvedPraiseBookSong(
                        new Data.SongSummary(
                            detail.SongId,
                            detail.Title,
                            detail.AlternateTitle,
                            detail.FolderNo,
                            detail.SongNumber,
                            detail.Category,
                            detail.Key,
                            detail.Lyrics,
                            detail.Copyright),
                        string.IsNullOrWhiteSpace(detail.Sequence) ? null : detail.Sequence,
                        string.IsNullOrWhiteSpace(detail.FormatData) ? null : detail.FormatData);
                }
            }
        }

        var song = ResolvePraiseBookSong(entry.Title, entry.Number, entry.SongId);
        return song is null ? null : new ResolvedPraiseBookSong(song, null, null);
    }

    private Data.SongSummary? ResolvePraiseBookSong(string? title, int songNumber, int songId)
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

        return song;
    }

    private sealed record ResolvedPraiseBookSong(Data.SongSummary Song, string? Sequence, string? FormatData);

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

    // 선택 항목을 맨 위로 이동(드래그 여러 번 없이 한 번에, 모던 재정렬 UX). MoveQueueItem 이 참조로 옮기고 선택 유지.
    private void MoveSelectedItemToTop()
    {
        if (SelectedItem is { } item)
        {
            MoveQueueItem(item, 0);
        }
    }

    // 선택 항목을 맨 아래로 이동.
    private void MoveSelectedItemToBottom()
    {
        if (SelectedItem is { } item)
        {
            MoveQueueItem(item, Queue.Count - 1);
        }
    }

    // 맨 위로는 첫 항목이 아닐 때만, 맨 아래로는 마지막 항목이 아닐 때만 가능(이미 끝이면 비활성).
    private bool CanMoveSelectedToBoundary(bool toTop)
    {
        var index = IndexOfSelectedReference();
        if (index < 0)
        {
            return false;
        }

        return toTop ? index > 0 : index < Queue.Count - 1;
    }

    /// <summary>
    /// 선택한 예배 순서 항목을 같은 내용으로 복제해 바로 뒤에 삽입한다(예: 같은 곡을 예배 중 두 번 부를 때).
    /// record `with` 로 모든 내용(가사·서식·슬라이드 등)을 복사하되 새 Id 를 부여해 원본과 구별한다
    /// (중복 Id 로 라이브 추적·재송출이 엉뚱한 인스턴스를 고르지 않도록). 복제본을 새로 선택한다.
    /// </summary>
    public LiveQueueItem? DuplicateSelectedItem()
    {
        if (SelectedItem is not { } item)
        {
            return null;
        }

        var index = IndexOfSelectedReference();
        if (index < 0)
        {
            return null;
        }

        // 새 고유 Id — 원본과 값은 같아도 참조·식별자는 달라 라이브 가드/재송출이 정확한 인스턴스를 가린다.
        var copy = item with { Id = $"dup:{Guid.NewGuid():N}" };
        Queue.Insert(index + 1, copy);
        SelectedItem = copy;
        StatusText = $"항목 복제: {copy.Title}";
        NotifyCommandStates();
        return copy;
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
            SetLiveItemId(null);
        }

        SelectedItem = Queue.Count == 0 ? null : Queue[Math.Min(index, Queue.Count - 1)];
        StatusText = $"항목 제거: {item.Title}";
        NotifyCommandStates();
    }

    // 방금 비운 예배 순서를 담아 두는 복구용 스냅샷("휴지통"). "되돌리기"로 한 번 복구할 수 있다.
    // 레거시 Empty→Trash 폴더 이동의 WPF 대응 — 실제 폴더 대신 세션 내 1단계 실행취소.
    private readonly List<LiveQueueItem> _clearedWorshipBackup = new();

    // 예배 순서 전체 비우기(레거시 Empty Worship List). 비우기 전에 현재 목록을 스냅샷으로 보관해
    // "되돌리기"로 복구할 수 있게 한다(실수로 다 지워도 한 번은 되살릴 수 있음).
    private void ClearWorshipList()
    {
        if (Queue.Count == 0)
        {
            return;
        }

        // 복구용 스냅샷 보관(직전 비우기 1회만 복구 가능 — 새로 비우면 이전 스냅샷은 대체된다).
        _clearedWorshipBackup.Clear();
        _clearedWorshipBackup.AddRange(Queue);

        var count = Queue.Count;
        Queue.Clear();
        SelectedItem = null;
        SetLiveItemId(null); // 큐가 비었으니 라이브 추적 정리(큐에 없는 고아 Id 로 슬라이드 가드가 오판하지 않도록)
        StatusText = $"예배 순서 {count}개 항목을 비웠습니다 (되돌리기로 복구 가능)";
        NotifyCommandStates();
    }

    // 방금 비운 예배 순서를 복구(되돌리기). 스냅샷이 있을 때만 동작하고, 복구 후 스냅샷은 비운다(중복 복구 방지).
    private void RestoreClearedWorshipList()
    {
        if (_clearedWorshipBackup.Count == 0)
        {
            return;
        }

        // 방어적: 스냅샷은 비우기 직후(큐가 빈 상태)에만 존재하지만, 혹시 큐에 항목이 있어도
        // 중복이 쌓이지 않도록 먼저 비우고 복구한다(append 가 아니라 replace 의미를 보장).
        Queue.Clear();
        foreach (var item in _clearedWorshipBackup)
        {
            Queue.Add(item);
        }

        var count = _clearedWorshipBackup.Count;
        _clearedWorshipBackup.Clear();
        SelectedItem = Queue.FirstOrDefault();
        StatusText = $"예배 순서 {count}개 항목을 복구했습니다";
        NotifyCommandStates();
    }

    /// <summary>PowerPoint 파일을 예배 순서(큐)에 추가(선택 시 썸네일 렌더 디스패치).</summary>
    public LiveQueueItem? AddPowerPoint(string filePath) => AddExternalFileItem(filePath, LiveItemKinds.PowerPoint, "PowerPoint 파일");

    /// <summary>미디어 파일을 예배 순서(큐)에 추가(선택 시 미디어 Load 디스패치).</summary>
    public LiveQueueItem? AddMedia(string filePath) => AddExternalFileItem(filePath, LiveItemKinds.Media, "미디어 파일");

    /// <summary>
    /// 탐색기 등에서 끌어다 놓은 외부 파일 여러 개를 확장자에 맞춰 예배 순서의 <b>드롭 위치</b>에 추가한다(레거시 외부 파일 드래그).
    /// <paramref name="targetItem"/> 앞에 끼우고(성경/곡 드래그와 동일), 빈 공간(타깃 없음)에 떨어뜨리면 맨 끝에 붙인다.
    /// 여러 파일은 순서를 유지한 채 한 묶음으로 연속 삽입한다. PowerPoint·미디어만 추가하고 모르는 확장자는 건너뛴다(개수 안내).
    /// 추가된 항목 수를 돌려준다. 파일 종류 판별은 순수 <see cref="ExternalFileClassifier"/>, 위치는 참조-안전한 <see cref="IndexOfReference"/>.
    /// </summary>
    public int AddExternalFilesRelativeTo(IReadOnlyList<string> paths, LiveQueueItem? targetItem)
    {
        ArgumentNullException.ThrowIfNull(paths);

        // 떨어뜨린 항목(타깃) 앞에 삽입할 위치. 타깃이 없거나 큐에 없으면 맨 끝.
        var targetIndex = targetItem is null ? -1 : IndexOfReference(targetItem);
        var insertIndex = targetIndex >= 0 ? targetIndex : Queue.Count;

        var added = 0;
        var skipped = 0;
        LiveQueueItem? firstAdded = null;
        foreach (var path in paths)
        {
            if (IsLegacyWorshipListFile(path))
            {
                string xml;
                try
                {
                    xml = File.ReadAllText(path);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    skipped++;
                    continue;
                }

                var legacyItems = EswWorshipListParser.Parse(xml);
                if (legacyItems.Count == 0)
                {
                    skipped++;
                    continue;
                }

                foreach (var importedItem in legacyItems.Select(BuildEswQueueItem))
                {
                    Queue.Insert(insertIndex++, importedItem);
                    firstAdded ??= importedItem;
                    added++;
                }

                continue;
            }

            var kind = ExternalFileClassifier.Classify(path) switch
            {
                ExternalFileKind.PowerPoint => LiveItemKinds.PowerPoint,
                ExternalFileKind.Media => LiveItemKinds.Media,
                _ => null,
            };

            if (kind is null)
            {
                skipped++; // 지원하지 않는 확장자 — 추가하지 않음.
                continue;
            }

            var item = CreateExternalFileItem(path, kind);
            Queue.Insert(insertIndex++, item); // 순서 유지: 다음 파일은 방금 넣은 것 뒤에(타깃 앞에서 묶음 유지).
            firstAdded ??= item;
            added++;
        }

        if (firstAdded is not null)
        {
            SelectedItem = firstAdded; // 묶음의 첫 항목 선택(곡/성경 드래그와 같은 결).
        }

        StatusText = skipped > 0
            ? $"외부 파일 {added}개 추가, {skipped}개 건너뜀(지원하지 않는 형식)"
            : $"외부 파일 {added}개 추가";
        NotifyCommandStates();
        return added;
    }

    /// <summary>
    /// 외부 파일들을 현재 선택 뒤에 추가한다(드롭 위치 없는 경로 — 끝이 아니라 선택 뒤). 다중 파일 메뉴 추가 등에 쓸 수 있다.
    /// 선택은 묶음의 <b>첫</b> 항목에 둔다(AddExternalFilesRelativeTo 와 동일 — 드롭 경로와 결을 맞춤).
    /// </summary>
    public int AddExternalFiles(IReadOnlyList<string> paths)
        => AddExternalFilesRelativeTo(paths, NextAfterSelectedOrNull()); // NextAfterSelectedOrNull 이 선택 없음(null)도 처리.

    // 현재 선택 "다음" 항목을 돌려준다 — 외부 파일을 선택 바로 뒤에 끼우려고 그 다음 항목을 타깃(앞에 삽입)으로 쓴다.
    // 선택이 마지막이면 다음이 없어 null → 맨 끝에 붙는다(=선택 뒤). 선택 없으면 호출 측에서 null 을 넘겨 끝에 붙는다.
    private LiveQueueItem? NextAfterSelectedOrNull()
    {
        if (SelectedItem is null)
        {
            return null;
        }

        var index = IndexOfReference(SelectedItem);
        return index >= 0 && index + 1 < Queue.Count ? Queue[index + 1] : null;
    }

    // 외부 파일 한 개를 큐 항목(LiveQueueItem)으로 만든다(삽입은 안 함). Id="kind:경로", 제목=확장자 뺀 파일명, ContentPath=경로.
    private static LiveQueueItem CreateExternalFileItem(string filePath, string kind)
    {
        var title = Path.GetFileNameWithoutExtension(filePath);
        return new LiveQueueItem($"{kind.ToLowerInvariant()}:{filePath}", title, kind) { ContentPath = filePath };
    }

    private LiveQueueItem? AddExternalFileItem(string filePath, string kind, string label)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusText = $"선택된 {label}이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        var item = CreateExternalFileItem(filePath, kind);
        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"{label} 추가됨: {item.Title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>저장된 예배 순서(워십 리스트) 이름 목록(레거시 FrmManageItemLists 대응 — G2).</summary>
    public IReadOnlyList<string> GetSavedWorshipLists() => _worshipLists.ListNames();

    /// <summary>
    /// 세션 콤보 목록(SavedWorshipListNames)을 저장된 예배 순서 이름으로 새로 채운다. 콤보를 열 때(View) 호출해 항상 최신 목록을 보여 준다.
    /// 현재 고른 이름이 목록에서 사라졌으면 선택을 비운다(없어진 목록 적재 시도 방지).
    /// </summary>
    public void RefreshSavedWorshipListNames()
    {
        // 새로고침 전에 현재 선택을 기억한다 — 콤보의 ItemsSource 를 Clear 하면 WPF Selector 가
        // "선택 항목이 사라졌다"며 SelectedItem 을 null 로 풀어 버리기 때문(TwoWay 라 VM 선택도 함께 풀림).
        var previousSelection = SelectedSavedWorshipList;

        SavedWorshipListNames.Clear();
        foreach (var name in _worshipLists.ListNames())
        {
            SavedWorshipListNames.Add(name);
        }

        // 선택했던 이름이 여전히 있으면 되살리고(새로고침으로 선택이 사라지지 않게), 없으면 첫 저장 목록을 고른다.
        // FrmMain 시작 화면처럼 SessionList 에 기본 목록이 잡혀 있어야 곧바로 불러오기/자동 초기 로드가 가능하다.
        SelectedSavedWorshipList =
            previousSelection is not null && SavedWorshipListNames.Contains(previousSelection)
                ? previousSelection
                : SavedWorshipListNames.FirstOrDefault();
    }

    /// <summary>
    /// 시작 직후 큐가 비어 있으면 SessionList 의 기본 선택 목록을 한 번 불러온다(FrmMain 시작 화면의 예배 순서 표시 대응).
    /// 이미 큐가 있으면 운영자가 만든 현재 순서를 보존한다.
    /// </summary>
    public async Task<bool> LoadInitialSelectedWorshipListIfQueueEmptyAsync()
    {
        if (Queue.Count > 0 || string.IsNullOrWhiteSpace(SelectedSavedWorshipList))
        {
            return false;
        }

        var name = SelectedSavedWorshipList;
        await LoadWorshipListAsync(name).ConfigureAwait(true);
        return true;
    }

    // 세션 콤보에서 고른 예배 순서를 명시적으로 적재한다(콤보 선택만으로는 적재 안 함 — "불러오기" 버튼이 호출).
    private async Task LoadSelectedWorshipListAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedSavedWorshipList))
        {
            return;
        }

        await LoadWorshipListAsync(SelectedSavedWorshipList).ConfigureAwait(true);
    }

    // 콤보 선택이 바뀌면 "불러오기" 버튼의 활성 상태를 갱신한다(고른 게 있어야 활성).
    partial void OnSelectedSavedWorshipListChanged(string? value)
        => LoadSelectedWorshipListCommand.NotifyCanExecuteChanged();

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
            RefreshSavedWorshipListNames(); // 새로 저장한 목록이 세션 콤보에 바로 보이도록.
            WorshipListHasUnsavedChanges = false; // 방금 저장했으니 미저장 변경 없음.
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

    /// <summary>
    /// 빠른 저장(Ctrl+S) — 현재 세션 이름(CurrentWorshipListName)으로 곧바로 덮어 저장한다(미저장 변경 표시와 짝).
    /// 한 번도 저장·불러온 적이 없어 이름이 없으면 저장하지 않고 안내만 한다(이름은 "순서" 관리 창에서 정한다).
    /// </summary>
    public async Task QuickSaveWorshipListAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentWorshipListName))
        {
            StatusText = "저장할 세션 이름이 없습니다. '순서' 버튼에서 이름을 정해 저장하세요.";
            return;
        }

        await SaveWorshipListAsync(CurrentWorshipListName).ConfigureAwait(true);
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
            if (await TryLoadLegacyWorshipListAsync(name.Trim()).ConfigureAwait(true))
            {
                return;
            }

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

    /// <summary>
    /// 저장된 예배 순서를 현재 큐 "뒤에 이어 붙인다"(불러오기=대체, 병합=추가, 레거시 .esw 병합).
    /// 현재 진행 중인 순서를 지우지 않고 다른 순서의 항목을 합칠 때 쓴다. 빈 큐에 병합하면 불러오기와 같은 효과(첫 항목 선택).
    /// 세션 이름(CurrentWorshipListName)은 바꾸지 않는다 — 결과는 두 순서의 조합이라 어느 한쪽 이름이 아니기 때문.
    /// </summary>
    public async Task MergeWorshipListAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        try
        {
            if (await TryMergeLegacyWorshipListAsync(name.Trim()).ConfigureAwait(true))
            {
                return;
            }

            var items = await _worshipLists.LoadAsync(name.Trim()).ConfigureAwait(true);
            var wasEmpty = Queue.Count == 0;
            foreach (var item in items)
            {
                Queue.Add(item);
            }

            // 빈 큐에 병합했으면 불러오기처럼 첫 항목을 선택해 둔다(비어 있지 않았으면 현재 선택 유지).
            if (wasEmpty)
            {
                SelectedItem = Queue.FirstOrDefault();
            }

            StatusText = $"예배 순서 병합: {name.Trim()} ({items.Count}개 추가, 총 {Queue.Count}개)";
            RefreshPowerPointLimitState(updateStatus: false);
            NotifyCommandStates();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"예배 순서 병합 실패: {ex.Message}";
        }
    }

    /// <summary>
    /// 레거시 .esw(EasiSlides v3.2) 파서 결과를 현재 예배 순서로 가져온다(대체) — 레거시 예배 순서 구조 복원.
    /// 최선 노력(best-effort): 항목 순서·제목·종류는 복원하고, 곡(D)은 라이브러리에 같은 곡이 있으면 가사까지 채운다.
    /// PowerPoint·미디어는 파일 참조(ContentPath), 성경·텍스트·InfoScreen·Word 는 제목/참조 수준으로 가져온다
    /// (곡 가사 DB 조회·성경 본문 확장·정확한 파일 경로 재해석은 후속 — 원본 환경 의존이라 가져오기 후 운영자가 보정).
    /// </summary>
    public void ImportEswWorshipList(IReadOnlyList<EswWorshipListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items); // LoadQueue 와 동일하게 입력을 방어(널이면 즉시 알림).

        LoadQueue(items.Select(BuildEswQueueItem));
        StatusText = $"레거시 예배 순서 가져옴: {Queue.Count}개 항목";
        // PPT 개수 제한 위반 플래그(HasPowerPointLimitViolation)는 갱신하되 상태줄은 덮지 않는다(updateStatus:false)
        // — 방금 띄운 "가져옴: N개" 안내를 PPT 경고가 즉시 지우지 않게(위반은 UI 배지로 계속 보인다). LoadQueue(true)와 의도된 차이.
    }

    // .esw 원시 항목 → WPF 큐 항목 매핑(종류 코드별). 내용 해석은 가능한 한(곡=라이브러리 가사), 나머지는 참조/제목 수준.
    public int ImportEswWorshipListFileRelativeTo(string filePath, LiveQueueItem? targetItem)
    {
        if (!IsLegacyWorshipListFile(filePath))
        {
            StatusText = "Legacy worship list file (.esw) was not found.";
            NotifyCommandStates();
            return 0;
        }

        string xml;
        try
        {
            xml = File.ReadAllText(filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusText = $"Could not read worship list file: {ex.Message}";
            NotifyCommandStates();
            return 0;
        }

        var items = EswWorshipListParser.Parse(xml);
        if (items.Count == 0)
        {
            StatusText = "Legacy worship list file has no importable items.";
            NotifyCommandStates();
            return 0;
        }

        var added = InsertEswWorshipListItems(items, targetItem);
        StatusText = $"Legacy worship list added: {Path.GetFileNameWithoutExtension(filePath)} ({added})";
        return added;
    }

    private int InsertEswWorshipListItems(IReadOnlyList<EswWorshipListItem> items, LiveQueueItem? targetItem)
    {
        ArgumentNullException.ThrowIfNull(items);

        var targetIndex = targetItem is null ? -1 : IndexOfReference(targetItem);
        var insertIndex = targetIndex >= 0 ? targetIndex : Queue.Count;
        LiveQueueItem? firstAdded = null;
        var added = 0;
        foreach (var item in items.Select(BuildEswQueueItem))
        {
            Queue.Insert(insertIndex++, item);
            firstAdded ??= item;
            added++;
        }

        if (firstAdded is not null)
        {
            SelectedItem = firstAdded;
        }

        RefreshPowerPointLimitState(updateStatus: false);
        NotifyCommandStates();
        return added;
    }

    private async Task<int> InsertEswWorshipListItemsAsync(IReadOnlyList<EswWorshipListItem> items, LiveQueueItem? targetItem)
    {
        ArgumentNullException.ThrowIfNull(items);

        var queueItems = await BuildEswQueueItemsAsync(items).ConfigureAwait(true);
        var targetIndex = targetItem is null ? -1 : IndexOfReference(targetItem);
        var insertIndex = targetIndex >= 0 ? targetIndex : Queue.Count;
        LiveQueueItem? firstAdded = null;
        foreach (var item in queueItems)
        {
            Queue.Insert(insertIndex++, item);
            firstAdded ??= item;
        }

        if (firstAdded is not null)
        {
            SelectedItem = firstAdded;
        }

        RefreshPowerPointLimitState(updateStatus: false);
        NotifyCommandStates();
        return queueItems.Count;
    }

    private static bool IsLegacyWorshipListFile(string? filePath)
        => !string.IsNullOrWhiteSpace(filePath)
            && string.Equals(Path.GetExtension(filePath), ".esw", StringComparison.OrdinalIgnoreCase)
            && File.Exists(filePath);

    private LiveQueueItem BuildEswQueueItem(EswWorshipListItem esw)
    {
        var title = string.IsNullOrWhiteSpace(esw.Title) ? esw.Id : esw.Title;
        switch (esw.TypeCode.ToUpperInvariant())
        {
            case "D": // DB 곡 — 라이브러리에 같은 곡(가사 포함)이 있으면 가사·번호·저작권까지, 없으면 제목만(가사 해석은 후속).
                if (int.TryParse(esw.Id, out var songId)
                    && Library.Songs.FirstOrDefault(s => s.SongId == songId) is { } song)
                {
                    return new LiveQueueItem($"song:{song.SongId}", song.Title, LiveItemKinds.Song)
                    {
                        Lyrics = song.Lyrics,
                        SongNumber = song.SongNumber,
                        Copyright = song.Copyright,
                        FormatData = esw.FormatData,
                    };
                }

                return new LiveQueueItem($"song:{esw.Id}", title, LiveItemKinds.Song) { FormatData = esw.FormatData };

            case "P": // PowerPoint — 식별자가 파일 참조. 경로가 안 맞으면 검증이 잡아 운영자가 보정.
                var powerPointPath = ResolveLegacyExternalPath(esw);
                return new LiveQueueItem($"powerpoint:{powerPointPath}", DisplayTitleForPath(title, powerPointPath), LiveItemKinds.PowerPoint)
                { ContentPath = powerPointPath, FormatData = esw.FormatData };

            case "M": // 미디어 — 식별자가 파일 참조.
                var mediaPath = ResolveLegacyExternalPath(esw);
                return new LiveQueueItem($"media:{mediaPath}", DisplayTitleForPath(title, mediaPath), LiveItemKinds.Media)
                { ContentPath = mediaPath, FormatData = esw.FormatData };

            case "B": // 성경 — 참조+제목(본문 확장은 후속).
                return new LiveQueueItem($"bible:{esw.Id}", title, LiveItemKinds.Bible)
                {
                    Lyrics = Bible.ExpandSelectionBody(esw.Id),
                    FormatData = esw.FormatData,
                };

            default: // T(텍스트)·I(InfoScreen)·W(Word)·미상 → 텍스트(공지) 항목으로 제목 표시(정확한 내용 재구성은 후속).
                var contentPath = ResolveLegacyExternalPath(esw);
                return new LiveQueueItem($"esw:{esw.TypeCode}:{contentPath}", DisplayTitleForPath(title, contentPath), LiveItemKinds.Notice)
                { ContentPath = contentPath, Lyrics = TryReadTextFile(contentPath) ?? title, FormatData = esw.FormatData };
        }
    }

    private async Task<IReadOnlyList<LiveQueueItem>> BuildEswQueueItemsAsync(IReadOnlyList<EswWorshipListItem> items)
    {
        var queueItems = new List<LiveQueueItem>(items.Count);
        foreach (var item in items)
        {
            queueItems.Add(await BuildEswQueueItemAsync(item).ConfigureAwait(true));
        }

        return queueItems;
    }

    private async Task<LiveQueueItem> BuildEswQueueItemAsync(EswWorshipListItem esw)
    {
        if (!string.Equals(esw.TypeCode, "D", StringComparison.OrdinalIgnoreCase)
            || !int.TryParse(esw.Id, out var songId))
        {
            return BuildEswQueueItem(esw);
        }

        var databasePath = ResolveSongDetailDatabasePath();
        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            var detail = await _songDetail.GetSongDetailAsync(databasePath, songId).ConfigureAwait(true);
            if (detail is not null)
            {
                return CreateSongQueueItem(
                    new Data.SongSummary(
                        detail.SongId,
                        detail.Title,
                        detail.AlternateTitle,
                        detail.FolderNo,
                        detail.SongNumber,
                        detail.Category,
                        detail.Key,
                        detail.Lyrics,
                        detail.Copyright),
                    string.IsNullOrWhiteSpace(detail.Sequence) ? null : detail.Sequence,
                    string.IsNullOrWhiteSpace(esw.FormatData) ? detail.FormatData : esw.FormatData);
            }
        }

        return BuildEswQueueItem(esw);
    }

    private LiveQueueItem CreateSongQueueItem(Data.SongSummary song, string? sequence = null, string? formatData = null)
        => new($"song:{song.SongId}", song.Title, LiveItemKinds.Song)
        {
            Lyrics = song.Lyrics,
            Sequence = sequence,
            SongNumber = song.SongNumber,
            Copyright = song.Copyright,
            FormatData = formatData,
        };

    private string ResolveSongDetailDatabasePath()
    {
        if (!string.IsNullOrWhiteSpace(Library.DatabasePath) && File.Exists(Library.DatabasePath))
        {
            return Library.DatabasePath;
        }

        if (!string.IsNullOrWhiteSpace(Search.DatabasePath) && File.Exists(Search.DatabasePath))
        {
            return Search.DatabasePath;
        }

        var workingFolder = _settings.Current.General.WorkingFolder;
        if (!string.IsNullOrWhiteSpace(workingFolder))
        {
            var databasePath = Path.Combine(workingFolder, "Admin", "Database", "EasiSlidesDb.db");
            if (File.Exists(databasePath))
            {
                return databasePath;
            }
        }

        return "";
    }

    /// <summary>현재 예배 세션(마지막으로 저장/불러온 예배 순서) 이름 — 세션 메모 키로 쓰인다. 없으면 빈 문자열.</summary>
    private static string ResolveLegacyExternalPath(EswWorshipListItem esw)
    {
        var title = esw.Title?.Trim() ?? string.Empty;
        if (LooksLikeFilePath(title))
        {
            return title;
        }

        return esw.Id?.Trim() ?? string.Empty;
    }

    private static string DisplayTitleForPath(string title, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !LooksLikeFilePath(title))
        {
            return string.IsNullOrWhiteSpace(title) ? path : title;
        }

        var fileName = Path.GetFileName(path);
        return string.IsNullOrWhiteSpace(fileName) ? title : fileName;
    }

    private static bool LooksLikeFilePath(string value)
        => !string.IsNullOrWhiteSpace(value)
            && (Path.IsPathRooted(value) || !string.IsNullOrWhiteSpace(Path.GetExtension(value)));

    private static string? TryReadTextFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)
            || !File.Exists(path)
            || !string.Equals(Path.GetExtension(path), ".txt", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

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
    public async Task ValidateWorshipListAsync()
    {
        // 1) 파일 차원 검사(동기) — 깨진/이동·삭제된 PPT·미디어 파일.
        var problems = _worshipValidator.Validate(Queue).ToList();

        // 2) 곡 DB 존재 검사(비동기·DB 의존) — DB 경로가 있을 때만. song:{id} 항목이 가사 DB 에 아직 있는지 확인한다.
        //    DB 경로가 없으면(설정 전·테스트) 건너뛴다 — 이 경우 await 에 닿지 않아 동기적으로 완료된다(무회귀).
        var databasePath = Search.DatabasePath;
        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            foreach (var item in Queue)
            {
                if (item is null || !TryGetSongDatabaseId(item, out var songId))
                {
                    continue; // 곡(song:{id})이 아니거나 복제(dup:)·텍스트 등은 DB 검사 대상 아님.
                }

                var detail = await _songDetail.GetSongDetailAsync(databasePath, songId).ConfigureAwait(true);
                if (detail is null)
                {
                    var label = string.IsNullOrWhiteSpace(item.Title) ? item.Id : item.Title;
                    problems.Add(new WorshipItemProblem(
                        item, WorshipItemProblemKind.SongNotInDatabase, $"{label}: 곡을 가사 DB 에서 찾을 수 없습니다."));
                }
            }
        }

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

        // DB 경로가 없어 곡 DB 검사를 건너뛰었고, 검사 대상 곡이 큐에 있었다면 그 사실을 상태바에 명시한다 —
        // "모든 항목 정상"만 보고 운영자가 곡 DB 까지 검증된 것으로 오해하지 않도록(리뷰어 지적: 조용한 생략 방지).
        if (string.IsNullOrWhiteSpace(databasePath)
            && Queue.Any(i => i is not null && TryGetSongDatabaseId(i, out _)))
        {
            StatusText += " (곡 DB 검증 생략 — DB 경로 없음)";
        }

        // 텔레메트리의 succeeded 는 "검증 명령이 정상 수행됨"을 뜻한다(문제를 찾는 것도 검증의 정상 동작이므로
        // 문제 발견 = 실패가 아니다). 발견한 문제 수는 StatusText 메시지에 담긴다.
        _telemetry.Record(MainCommandIds.WorshipListValidate, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    // 큐 항목 Id 가 "song:{정수}" 형태면 곡 DB Id 를 꺼낸다(AddSong 이 부여한 식별자). 그 외(dup:·text:·bible:·ppt: 등)는 false.
    // 복제(dup:) 항목은 가사를 이미 들고 있어 DB 검사 대상이 아니므로 자연히 제외된다.
    private static bool TryGetSongDatabaseId(LiveQueueItem item, out int songId)
    {
        songId = 0;
        const string prefix = "song:";
        if (item.Id is null || !item.Id.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        return int.TryParse(item.Id.AsSpan(prefix.Length), out songId);
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
        RefreshSavedWorshipListNames(); // 삭제한 목록이 세션 콤보에서 바로 빠지도록(저장 경로와 대칭).
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

        // 선택 중이던 이름이 바뀌었으면 세션 콤보 선택도 새 이름으로 따라가게 먼저 맞춘 뒤 목록을 새로고침한다.
        if (string.Equals(SelectedSavedWorshipList, from, StringComparison.Ordinal))
        {
            SelectedSavedWorshipList = to;
        }

        RefreshSavedWorshipListNames(); // 바뀐 이름이 세션 콤보에 바로 반영되도록(저장·삭제 경로와 대칭).
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
        registry.Bind(MainCommandIds.LiveGo, () => _ = ToggleOutputLiveCommand.ExecuteAsync(null));
        // F11 = 송출 후 다음 항목. 형제 명령 LiveGo 와 같은 방식으로 바인딩 —
        // 둘 다 CanGoLive 조건·내부 안전 확인을 SendToOutputAndNextAsync 안에서 처리하므로 여기서 따로 게이트하지 않는다.
        registry.Bind(MainCommandIds.LiveGoAndNext, () => _ = SendToOutputAndNextCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveStop, () => _ = StopLiveCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveNext, () => _ = LiveNextShortcutCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LivePrevious, () => _ = LivePreviousShortcutCommand.ExecuteAsync(null));
        registry.Bind(MainCommandIds.LiveBlack, () => _ = ToggleOutputBlackCommand.ExecuteAsync(null));
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

        BindGated(MainCommandIds.LiveClear, ToggleOutputClearCommand);
        BindGated(MainCommandIds.LivePreviewToOutput, CopyPreviewToOutputShortcutCommand);
        BindGated(MainCommandIds.LivePreviewToOutputClearBlack, CopyPreviewToOutputAndClearBlackCommand);
        BindGated(MainCommandIds.LiveRestart, RestartCurrentItemCommand);
        BindGated(MainCommandIds.LiveRefresh, RefreshOutputCommand);
        BindGated(MainCommandIds.LiveRestore, RestoreOutputCommand);
        BindGated(MainCommandIds.LiveAutoRotate, ToggleAutoRotateCommand);
        // 예배 순서 항목 복제(Ctrl+D + 명령 팔레트) — 선택 없으면 CanExecute 가 막아 무동작.
        BindGated(MainCommandIds.WorshipDuplicateItem, DuplicateSelectedItemCommand);
        // 예배 순서 재정렬(Ctrl+Shift+Up/Down + 팔레트) — CanExecute(경계·선택 없음)가 막으면 무동작.
        BindGated(MainCommandIds.WorshipMoveItemUp, MoveSelectedItemUpCommand);
        BindGated(MainCommandIds.WorshipMoveItemDown, MoveSelectedItemDownCommand);
        BindGated(MainCommandIds.WorshipMoveItemToTop, MoveSelectedItemToTopCommand);
        BindGated(MainCommandIds.WorshipMoveItemToBottom, MoveSelectedItemToBottomCommand);
        // 빠른 저장(Ctrl+S) — 메서드가 이름 없음/실패를 스스로 안내하므로 게이트 없이 실행(fire-and-forget).
        registry.Bind(MainCommandIds.WorshipQuickSave, () => _ = QuickSaveWorshipListCommand.ExecuteAsync(null));
        // 현재 송출 항목 선택(팔레트) — CanExecute(송출 중·큐에 있음)가 막으면 무동작.
        BindGated(MainCommandIds.WorshipSelectLiveItem, SelectLiveItemCommand);
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

    // 출력 모니터 선택이 바뀌면 — 출력 창이 이미 열려 있을 때 그 모니터로 즉시 창을 옮긴다(레거시 런타임 MoveTo 배선).
    // 닫혀 있으면 다음 Open 때 반영되므로 아무것도 안 한다.
    // 이미 같은 모니터(레코드 값 전체 동일 — Id·위치·크기·DPI)면 이동 불필요. OpenOutput 이 Open 직후
    // SelectedOutputDisplay 를 같은 모니터로 세팅해도 값이 같아 재이동하지 않는다(무한 이동 방지).
    // 값 전체로 비교하므로 같은 Id 라도 해상도/위치가 바뀐 경우(모니터 재구성)엔 정상적으로 다시 이동한다.
    partial void OnSelectedOutputDisplayChanged(OutputDisplay? value)
    {
        if (value is null || !_output.Current.IsOpen)
        {
            return;
        }

        if (_output.Current.Display is { } current && current == value)
        {
            return;
        }

        _output.MoveTo(value, windowed: true);
        LiveBar.OutputMonitorName = value.Name;
        StatusText = $"출력 모니터 이동: {value.Name}";
    }

    // 스테이지(Preview) 모니터 선택 후보 목록을 채운다 — 출력 모니터 목록(RefreshOutputDisplays)과 같은 방식.
    // 기본 선택은 저장된 PreviewMonitorId(있으면) → 없으면 선호 모니터 규칙(GetPreferredOutputDisplay).
    public void RefreshPreviewDisplays()
    {
        var preferredId = SelectedPreviewDisplay?.Id;
        if (string.IsNullOrWhiteSpace(preferredId))
        {
            // 저장된 스테이지 모니터(있으면)를 기본 선택으로 — 다음 실행에도 같은 모니터를 고른다(출력 RefreshOutputDisplays 와 같은 취지).
            preferredId = _settings.Get(EasiSettingKeys.PreviewMonitorId);
        }

        var displays = _display.GetDisplays();

        PreviewDisplays.Clear();
        foreach (var display in displays)
        {
            PreviewDisplays.Add(display);
        }

        var selected = GetPreferredOutputDisplay(preferredId, displays);
        var matching = PreviewDisplays.FirstOrDefault(display =>
            string.Equals(display.Id, selected.Id, StringComparison.OrdinalIgnoreCase)) ?? selected;
        if (!PreviewDisplays.Contains(matching))
        {
            PreviewDisplays.Add(matching);
        }

        // 목록 갱신으로 인한 선택 변경은 사용자 입력이 아니므로 영속화하지 않는다 — 저장된 모니터가 분리돼
        // fallback 이 들어와도 저장된 선호(PreviewMonitorId)를 덮어쓰지 않게(재연결 시 복원되도록).
        _suppressPreviewMonitorPersist = true;
        try
        {
            SelectedPreviewDisplay = matching;
        }
        finally
        {
            _suppressPreviewMonitorPersist = false;
        }
    }

    // 스테이지 모니터를 선택한 디스플레이에 풀스크린(windowed:false)으로 띄운다 — 회중 출력 창과 별개의 창이다.
    private void OpenStageMonitor()
    {
        var display = SelectedPreviewDisplay ?? GetPreferredOutputDisplay(null);
        _preview.Open(display, windowed: false);
        SelectedPreviewDisplay = display;
        StatusText = $"스테이지 모니터 열림: {display.Name}";
    }

    // 스테이지 모니터를 닫는다 — 회중 출력과 무관(송출 안 함)하므로 출력 닫기(CloseOutputAsync)와 달리 라이브 안전 확인이 필요 없다.
    private void CloseStageMonitor()
    {
        _preview.Close();
        StatusText = "스테이지 모니터 닫힘";
    }

    // 스테이지 창 상태(열림/닫힘)가 바뀌면 메뉴·버튼 활성(IsStageMonitorOpen)을 따라가게 한다.
    private void OnPreviewChanged(object? sender, PreviewWindowChangedEventArgs e)
        => IsStageMonitorOpen = e.State.IsOpen;

    // 스테이지 모니터로 닫기 명령은 창이 열려 있을 때만 가능 — 상태가 바뀌면 활성 상태를 갱신한다.
    partial void OnIsStageMonitorOpenChanged(bool value)
        => CloseStageMonitorCommand.NotifyCanExecuteChanged();

    // 스테이지 모니터 선택이 바뀌면 — 창이 이미 열려 있을 때 그 모니터로 즉시 옮긴다(출력 OnSelectedOutputDisplayChanged 와 같은 취지).
    // 닫혀 있으면 다음 열기 때 반영되므로 아무것도 안 한다. 같은 모니터(값 전체 동일)면 재이동하지 않는다(무한 이동 방지).
    partial void OnSelectedPreviewDisplayChanged(OutputDisplay? value)
    {
        if (value is null)
        {
            return;
        }

        // 사용자가 콤보에서 직접 고른 경우에만 영속화한다 — 목록 갱신(RefreshPreviewDisplays)의 프로그램적 선택은 제외해
        // 분리된 모니터 fallback 으로 저장된 선호를 덮어쓰지 않게. 출력은 설정 창에서 저장하지만 스테이지는 설정 창 UI 가
        // 없어 여기(콤보 선택)서 바로 영속화한다(다음 실행에도 같은 모니터를 기본 선택).
        if (!_suppressPreviewMonitorPersist)
        {
            _settings.Set(EasiSettingKeys.PreviewMonitorId, value.Id);
        }

        // 창이 이미 열려 있으면 그 모니터로 즉시 옮긴다(닫혀 있으면 다음 열기 때 반영). 같은 모니터(값 전체 동일)면 재이동 안 함(무한 이동 방지).
        if (!_preview.Current.IsOpen)
        {
            return;
        }

        if (_preview.Current.Display is { } current && current == value)
        {
            return;
        }

        _preview.MoveTo(value, windowed: false);
        StatusText = $"스테이지 모니터 이동: {value.Name}";
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

    private static readonly string[] WorshipOutputMediaExtensions =
    [
        ".mp4", ".m4v", ".mov", ".avi", ".wmv", ".mpg", ".mpeg", ".mkv", ".webm",
        ".mp3", ".wav", ".wma", ".m4a", ".aac", ".flac", ".ogg"
    ];

    // 현재 Preview 썸네일 스트립이 채워진 덱 파일 경로(같은 덱은 재로드 안 함).
    private string? _thumbnailDeckPath;
    // 현재 Output 썸네일 스트립이 가리키는 덱. Preview 선택 변경과 독립적으로 보존한다.
    private string? _outputThumbnailDeckPath;

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

        // 절 순서 입력칸(곡만)·항목별 색(곡·성경)이 새로 선택된 항목을 따라가도록 통지(대상이면 활성, 아니면 비활성).
        OnPropertyChanged(nameof(SelectedItemSequenceInput));
        OnPropertyChanged(nameof(CanEditSelectedItemSequence));
        OnPropertyChanged(nameof(CanEditSelectedItemColor));
        OnPropertyChanged(nameof(CanClearSelectedItemFormatting));
        OnPropertyChanged(nameof(CanCopySelectedItemFormatting));
        OnPropertyChanged(nameof(CanPasteSelectedItemFormatting));
        OnPropertyChanged(nameof(CanApplyCopiedFormatToAll));
        OnPropertyChanged(nameof(CanClearAllItemsFormatting));
        OnPropertyChanged(nameof(PreviewItemInfoText));
        OnPropertyChanged(nameof(PreviewVisualSource));
        OnPropertyChanged(nameof(HasPreviewVisualSource));
        OnPropertyChanged(nameof(SelectedItemTextColorHex));
        OnPropertyChanged(nameof(SelectedItemAlignment));
        OnPropertyChanged(nameof(SelectedItemFontSize));
        OnPropertyChanged(nameof(SelectedItemFontName));
        OnPropertyChanged(nameof(SelectedItemBackgroundColorHex));
        OnPropertyChanged(nameof(SelectedItemBackgroundImagePath));
        OnPropertyChanged(nameof(SelectedItemBold));
        OnPropertyChanged(nameof(SelectedItemItalic));
        OnPropertyChanged(nameof(SelectedItemUnderline));
        OnPropertyChanged(nameof(SelectedItemTextColor2Hex));
        OnPropertyChanged(nameof(SelectedItemAlignment2));
        OnPropertyChanged(nameof(SelectedItemFontSize2));
        OnPropertyChanged(nameof(SelectedItemFontName2));
        OnPropertyChanged(nameof(SelectedItemBold2));
        OnPropertyChanged(nameof(SelectedItemItalic2));
        OnPropertyChanged(nameof(SelectedItemUnderline2));

        NotifyCommandStates();
    }

    private static string BuildPreviewItemInfoText(LiveQueueItem? item)
    {
        if (item is null)
        {
            return "선택 항목 없음";
        }

        var lines = new List<string>
        {
            $"제목: {item.Title}",
            $"종류: {item.Kind}",
            $"ID: {item.Id}",
            $"개별 서식: {(item.UseIndividualFormatting ? "켬" : "끔")}",
        };

        if (item.SongNumber > 0)
        {
            lines.Add($"곡 번호: {item.SongNumber}");
        }

        if (!string.IsNullOrWhiteSpace(item.ContentPath))
        {
            lines.Add($"파일: {item.ContentPath}");
        }

        if (item.SlideNumber > 0)
        {
            lines.Add($"슬라이드: {item.SlideNumber}");
        }

        if (!string.IsNullOrWhiteSpace(item.Sequence))
        {
            lines.Add($"절 순서: {item.Sequence}");
        }

        if (!string.IsNullOrWhiteSpace(item.Copyright))
        {
            lines.Add($"저작권: {item.Copyright}");
        }

        if (!string.IsNullOrWhiteSpace(item.FormatData))
        {
            lines.Add($"서식 데이터: {item.FormatData}");
        }

        if (!string.IsNullOrWhiteSpace(item.Lyrics))
        {
            lines.Add($"가사 줄: {CountNonEmptyLines(item.Lyrics)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static int CountNonEmptyLines(string value)
        => value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length;

    private void ShowPreviewPanelMode(PreviewPanelMode mode)
    {
        if (PreviewPanelMode != mode)
        {
            PreviewPanelMode = mode;
            return;
        }

        // ToggleButton 은 이미 켜진 버튼을 다시 누르면 로컬 IsChecked 가 false 로 떨어질 수 있다.
        // 값은 그대로라도 파생 상태를 다시 알려 FrmMain RadioButton처럼 항상 한 모드가 눌린 상태를 유지한다.
        OnPropertyChanged(nameof(IsPreviewTextMode));
        OnPropertyChanged(nameof(IsPreviewFormatMode));
        OnPropertyChanged(nameof(IsPreviewInfoMode));
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
        var pageModel = BuildLyricsPageModel(item);
        LyricsPageCount = pageModel.Count;
        _pageLabels = pageModel.Labels;
        RebuildAvailableSectionLabels();
        LyricsPageIndex = 0;
        RebuildPreviewLyricsPages();
    }

    partial void OnLyricsPageIndexChanged(int value)
    {
        RebuildPreviewLyricsPages();
        GoToPreviewLyricsPageCommand.NotifyCanExecuteChanged();
    }

    partial void OnLyricsPageCountChanged(int value)
    {
        RebuildPreviewLyricsPages();
        GoToPreviewLyricsPageCommand.NotifyCanExecuteChanged();
    }

    partial void OnOutputLyricsTextChanged(string value)
    {
        RebuildOutputLyricsPages();
    }

    partial void OnLiveTransposeSemitonesChanged(int value)
    {
        RebuildPreviewLyricsPages();
        RefreshOutputSurfaceText();
        RebuildOutputLyricsPages();
    }

    private static (int Count, IReadOnlyList<string> Labels) BuildLyricsPageModel(LiveQueueItem? item)
    {
        var paginated = IsLyricsPaginated(item);
        if (!paginated)
        {
            return (0, Array.Empty<string>());
        }

        // 성경 본문은 인용부호 »…« 가 코드 마커로 오인돼 절 수가 어긋나지 않도록 분할 전 보호한다(본문 추출과 동일 규칙).
        var lyrics = GuardBibleNotation(item!);
        // 이중 언어([region 2]) 곡·성경은 영역-인식 페이지 수(GetRegionPages)를 쓴다 — [region 2] 가 절 경계로
        // 오인돼 절 수가 부풀던 문제 해소. 단일 영역은 기존 ToVersePages(Sequence 적용) 경로 그대로(무회귀).
        var dual = LyricsDisplayFormatter.HasRegion2(lyrics);
        var count = dual
            ? LyricsDisplayFormatter.GetRegionPages(lyrics, item!.Sequence).Count
            : LyricsDisplayFormatter.ToVersePages(lyrics, item!.Sequence).Count;

        // 페이지별 절 라벨(절 점프 근거). 단일 영역은 GetSectionLabels, 이중 언어는 region-aware 라벨을 쓰되
        // 페이지 수와 1:1 정렬될 때만 채운다(라벨 없는 머리말 등으로 어긋나면 비워 점프 비활성 — 잘못된 점프 방지).
        // 성경 본문엔 [라벨] 마커가 없어 라벨 목록은 비고(절 점프 바 없음), 이전/다음 절 이동만 동작한다.
        if (!dual)
        {
            return (count, LyricsDisplayFormatter.GetSectionLabels(lyrics, item!.Sequence));
        }

        var regionLabels = LyricsDisplayFormatter.GetRegionSectionLabels(lyrics, item!.Sequence);
        return (count, regionLabels.Count == count ? regionLabels : Array.Empty<string>());
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
        JumpToOutputLyricsSectionCommand.NotifyCanExecuteChanged();
        GoToPreviewLyricsPageCommand.NotifyCanExecuteChanged();
        GoToOutputLyricsPageCommand.NotifyCanExecuteChanged();
    }

    private void RebuildPreviewLyricsPages()
    {
        ReplaceLyricsPageCards(
            PreviewLyricsPages,
            BuildLyricsPageCards(SelectedItem, IsLyricsPaginated(SelectedItem) ? LyricsPageIndex : 0));
        OnPropertyChanged(nameof(HasPreviewLyricsPages));
        OnPropertyChanged(nameof(PreviewLyricsText));
        OnPropertyChanged(nameof(HasPreviewLyricsText));
    }

    private void RebuildOutputLyricsPages()
    {
        var item = GetOutputLyricsCardItem();
        var currentPage = item is not null && IsLyricsPaginated(item)
            ? GetOutputLyricsPageIndex(item)
            : 0;
        ReplaceLyricsPageCards(OutputLyricsPages, BuildLyricsPageCards(item, currentPage));
        OnPropertyChanged(nameof(HasOutputLyricsPages));
        GoToOutputLyricsPageCommand.NotifyCanExecuteChanged();
    }

    private static void ReplaceLyricsPageCards(
        ObservableCollection<OperatorLyricsPageCard> target,
        IReadOnlyList<OperatorLyricsPageCard> source)
    {
        target.Clear();
        foreach (var card in source)
        {
            target.Add(card);
        }
    }

    private IReadOnlyList<OperatorLyricsPageCard> BuildLyricsPageCards(LiveQueueItem? item, int currentPage)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Lyrics))
        {
            return [];
        }

        if (!IsLyricsPaginated(item))
        {
            return
            [
                new OperatorLyricsPageCard(
                    0,
                    "",
                    "",
                    item.Lyrics,
                    true),
            ];
        }

        var pageModel = BuildLyricsPageModel(item);
        if (pageModel.Count == 0)
        {
            return [];
        }

        var clamped = Math.Clamp(currentPage, 0, pageModel.Count - 1);
        var cards = new List<OperatorLyricsPageCard>(pageModel.Count);
        for (var index = 0; index < pageModel.Count; index++)
        {
            var label = pageModel.Labels.Count > index && !string.IsNullOrWhiteSpace(pageModel.Labels[index])
                ? pageModel.Labels[index]
                : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            cards.Add(new OperatorLyricsPageCard(
                index,
                label,
                $"{index + 1}/{pageModel.Count}",
                BuildOutputSurfaceText(item with { LyricsPageIndex = index }),
                index == clamped));
        }

        return cards;
    }

    private LiveQueueItem? GetOutputLyricsCardItem()
    {
        if (GetOutputLyricsNavigationItem() is { } paginated)
        {
            return paginated;
        }

        return OutputItem is not null
            && !IsPowerPointItem(OutputItem)
            && !IsMediaItem(OutputItem)
            && !string.IsNullOrWhiteSpace(OutputLyricsText)
            ? OutputItem
            : null;
    }

    private void GoToPreviewLyricsPage(int target)
    {
        if (!CanGoToPreviewLyricsPage(target))
        {
            return;
        }

        if (!IsLyricsPaginated(SelectedItem) || target == LyricsPageIndex)
        {
            RebuildPreviewLyricsPages();
            return;
        }

        LyricsPageIndex = target;
        StatusText = LyricsPageCount > 1
            ? $"가사 {LyricsPageIndex + 1}/{LyricsPageCount}절"
            : StatusText;
        NotifyCommandStates();
    }

    private bool CanGoToPreviewLyricsPage(int target)
        => target >= 0 && target < PreviewLyricsPages.Count;

    private void GoToOutputLyricsPage(int target)
    {
        if (!CanGoToOutputLyricsPage(target))
        {
            return;
        }

        if (GetOutputLyricsNavigationItem() is null)
        {
            RebuildOutputLyricsPages();
            return;
        }

        MoveOutputLyricsPage(target);
    }

    private bool CanGoToOutputLyricsPage(int target)
        => target >= 0 && target < OutputLyricsPages.Count;

    // Preview 절 라벨로 직접 점프 — 그 라벨의 첫 페이지로 이동한다.
    // FrmMain PreviewItem/OutputItem 분리와 같이, 이 Preview 명령은 live Output 을 즉시 갱신하지 않는다.
    private void JumpToLyricsSection(string? label)
    {
        var index = IndexOfPageLabel(label);
        if (index < 0)
        {
            return; // 없는 라벨은 무시(이동 없음).
        }

        LyricsPageIndex = index;
        StatusText = LyricsPageCount > 1
            ? $"가사 {LyricsPageIndex + 1}/{LyricsPageCount}절"
            : StatusText;
    }

    // 점프 가능 여부 — 비어 있지 않은 라벨이 현재 곡의 페이지 라벨에 존재할 때만.
    private bool CanJumpToLyricsSection(string? label) => IndexOfPageLabel(label) >= 0;

    // 라벨의 첫 페이지 인덱스(대소문자 무시). 없으면 -1. 점프/CanExecute 공통.
    private int IndexOfPageLabel(string? label) => IndexOfPageLabel(_pageLabels, label);

    private static int IndexOfPageLabel(IReadOnlyList<string> labels, string? label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return -1;
        }

        for (var i = 0; i < labels.Count; i++)
        {
            if (string.Equals(labels[i], label, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    // Preview 다음 절로 이동 — Output 은 Output 전용 명령으로만 이동한다.
    private void NextLyricsPage()
    {
        LyricsPageIndex++;
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
    /// 자동 회전 한 스텝(View 타이머가 매 간격마다 호출).
    /// 먼저 현재 항목 안에서 "다음 절/슬라이드"가 남아 있으면 그것부터 진행한다(모든 모드 공통).
    /// 현재 항목의 끝(마지막 절/슬라이드 또는 회전할 내부 페이지가 없음)에 다다르면 모드별로 마무리한다
    /// (OneRepeat=같은 항목 순환, One=정지, Group=다음 항목/끝이면 정지, GroupRepeat=다음 항목/끝이면 첫 항목으로).
    /// 라이브가 아니거나 현재 라이브 항목을 큐에서 찾을 수 없으면 아무것도 하지 않는다.
    /// </summary>
    public void AdvanceAutoRotation()
    {
        if (_session.Current.State != LiveState.Active) return;
        if (GetLiveQueueItem() is not { } item) return;

        // 1) live Output 항목 안에 다음 슬라이드가 남아 있으면 그것부터(끝 슬라이드면 통과해 2)로).
        if (IsPowerPointItem(item) && IsOutputPowerPointSlideNavReady())
        {
            if (OutputPowerPoint.SlideNumber < OutputPowerPoint.SlideCount)
            {
                _ = GoToOutputSlideAsync(OutputPowerPoint.SlideNumber + 1);
                return;
            }
        }
        // 1) live 곡·성경 항목은 절 단위로 — 다음 절이 남아 있으면 그것부터(마지막 절이면 통과해 2)로).
        else if (IsLyricsPaginated(item))
        {
            var pageModel = BuildLyricsPageModel(item);
            if (pageModel.Count > 1 && _session.Current.CurrentLyricsPageIndex + 1 < pageModel.Count)
            {
                PublishOutputLyricsPage(item, _session.Current.CurrentLyricsPageIndex + 1, pageModel.Count);
                return;
            }
        }

        // 2) 현재 항목의 끝 — 자동 회전 모드에 따라 마무리.
        HandleAutoRotationItemEnd(item);
    }

    // 현재 항목의 마지막 절/슬라이드(또는 회전할 내부 페이지 없음)에 다다랐을 때 모드별 동작.
    private void HandleAutoRotationItemEnd(LiveQueueItem item)
    {
        switch (ActiveAutoRotateMode)
        {
            case AutoRotateMode.OneRepeat:
                // 같은 항목 첫 절/슬라이드로 순환(기존 기본 동작 — 무회귀).
                RewindCurrentItemToStart(item);
                break;
            case AutoRotateMode.One:
                // 한 항목만 — 끝까지 가면 자동 회전을 멈춘다(반복·항목 이동 없음).
                IsAutoRotating = false;
                StatusText = "자동 회전 완료(한 항목)";
                break;
            case AutoRotateMode.Group:
                // 다음 예배 순서 항목으로; 마지막 항목이면 멈춘다.
                if (!TryAdvanceAutoRotationToNextItem())
                {
                    IsAutoRotating = false;
                    StatusText = "자동 회전 완료(그룹 끝)";
                }
                break;
            case AutoRotateMode.GroupRepeat:
                // 다음 항목으로; 마지막 항목이면 첫 항목으로 돌아가 계속 순환.
                if (!TryAdvanceAutoRotationToNextItem())
                {
                    RewindAutoRotationToFirstItem();
                }
                break;
        }
    }

    // 같은 항목의 첫 절/슬라이드로 되돌려 다시 송출(OneRepeat 순환). 단일 페이지 항목은 되돌릴 게 없어 그대로.
    private void RewindCurrentItemToStart(LiveQueueItem item)
    {
        if (IsPowerPointItem(item) && IsOutputPowerPointSlideNavReady() && OutputPowerPoint.SlideCount > 1)
        {
            _ = GoToOutputSlideAsync(1);
        }
        else if (IsLyricsPaginated(item))
        {
            var pageModel = BuildLyricsPageModel(item);
            if (pageModel.Count > 1)
            {
                PublishOutputLyricsPage(item, 0, pageModel.Count);
            }
        }
    }

    // 다음 예배 순서 항목으로 이동해 송출. 다음 항목이 있으면 true, 마지막(다음 없음)이면 false.
    // 자동 회전은 live Output 항목 기준으로 다음을 계산하고, 선택은 송출할 다음 항목으로만 이동한다.
    private bool TryAdvanceAutoRotationToNextItem()
    {
        var live = GetLiveQueueItem();
        if (live is null) return false;
        var index = Queue.IndexOf(live);
        if (index < 0 || index + 1 >= Queue.Count) return false;

        SelectedItem = Queue[index + 1];
        PublishSelectedItem(autoAdvance: false);
        return true;
    }

    // 첫 항목으로 돌아가 송출(GroupRepeat 순환). 큐가 비면 아무것도 안 한다.
    private void RewindAutoRotationToFirstItem()
    {
        if (Queue.Count == 0) return;
        SelectedItem = Queue[0];
        PublishSelectedItem(autoAdvance: false);
    }

    /// <summary>자동 회전 모드 콤보의 (한글 라벨 → 모드) 목록. SelectedValue 로 모드를, DisplayMember 로 라벨을 쓴다.</summary>
    public IReadOnlyList<KeyValuePair<string, AutoRotateMode>> AutoRotateModeOptions { get; } =
    [
        new("현재 항목 반복", AutoRotateMode.OneRepeat),
        new("한 항목만", AutoRotateMode.One),
        new("그룹(다음 항목)", AutoRotateMode.Group),
        new("그룹 반복", AutoRotateMode.GroupRepeat),
    ];

    /// <summary>자동 회전 모드 선택(콤보 양방향 바인딩). 바뀌면 설정에 저장하고 인스펙터 표시를 동기화한다.</summary>
    public AutoRotateMode AutoRotateModeInput
    {
        get => ActiveAutoRotateMode;
        set
        {
            if (value == ActiveAutoRotateMode)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.AutoRotateMode, value);
            ActiveAutoRotateMode = value;
        }
    }

    // 모드가 다른 경로(설정 동기화 등)로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveAutoRotateModeChanged(AutoRotateMode value) => OnPropertyChanged(nameof(AutoRotateModeInput));

    /// <summary>대기 화면(Gap) 모드 콤보의 (한글 라벨 → 모드) 목록 — 출력 메뉴에서 빠르게 고른다.</summary>
    public IReadOnlyList<KeyValuePair<string, GapItemMode>> GapItemModeOptions { get; } =
    [
        new("없음(대기 문구)", GapItemMode.None),
        new("검정(검은 화면)", GapItemMode.Black),
        new("기본(대기 문구)", GapItemMode.Default),
        new("로고 이미지", GapItemMode.User),
    ];

    /// <summary>대기 화면(Gap) 모드 선택(콤보 양방향). 바뀌면 설정에 저장(출력이 라이브 갱신)하고 Active 동기화.</summary>
    public GapItemMode GapItemModeInput
    {
        get => ActiveGapItemOption;
        set
        {
            if (value == ActiveGapItemOption)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.GapItemOption, value);
            ActiveGapItemOption = value;
            var label = GapItemModeOptions.FirstOrDefault(o => o.Value == value).Key;
            StatusText = $"대기 화면(Gap) 모드: {(string.IsNullOrEmpty(label) ? value.ToString() : label)}";
        }
    }

    // 모드가 다른 경로(설정 창 등)로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveGapItemOptionChanged(GapItemMode value) => OnPropertyChanged(nameof(GapItemModeInput));

    /// <summary>대기 화면(Gap) 전환 페이드 사용을 켜고 끈다(출력 메뉴 토글). 설정 저장 → 출력 라이브 갱신.</summary>
    public void ToggleGapItemUseFade()
    {
        var next = !ActiveGapItemUseFade;
        _settings.Set(EasiSettingKeys.GapItemUseFade, next);
        ActiveGapItemUseFade = next;
        StatusText = next ? "대기 화면 페이드: 켜짐" : "대기 화면 페이드: 꺼짐";
    }

    /// <summary>
    /// 대기 화면(Gap) "로고" 모드에서 쓸 이미지 경로를 설정한다(출력 메뉴 → 파일 선택은 View). 비우면 로고를 지운다.
    /// 로고를 고르면 모드도 "로고"(User)로 자동 전환해 바로 보이게 한다. 경로/모드 모두 설정 저장 → 출력 라이브 갱신.
    /// </summary>
    public void SetGapItemLogoFile(string? path)
    {
        var logo = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim();
        _settings.Set(EasiSettingKeys.GapItemLogoFile, logo);
        ActiveGapItemLogoFile = logo;

        if (logo.Length == 0)
        {
            StatusText = "대기 화면 로고: 지움";
            return;
        }

        // 로고를 골랐으면 보이도록 모드도 "로고"로 전환(없음/검정이면 로고가 안 보이므로 운영자 의도대로).
        if (ActiveGapItemOption != GapItemMode.User)
        {
            GapItemModeInput = GapItemMode.User;
        }

        StatusText = $"대기 화면 로고: {System.IO.Path.GetFileName(logo)}";
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

    private bool CanPlaySelectedWorshipMedia()
        => SelectedItem is not null;

    private bool CanPlaySelectedWorshipMediaOnOutput()
        => CanPlaySelectedWorshipMedia();

    private bool CanPlayOutputMedia()
        => GetOutputNavigationItem() is not null;

    private void PlaySelectedWorshipMedia()
    {
        if (SelectedItem is not { } item)
        {
            StatusText = "재생할 Worship 항목을 선택하세요.";
            return;
        }

        var mediaPath = ResolveWorshipOutputMediaPath(item);
        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            StatusText = $"미디어 파일을 찾을 수 없습니다: {item.Title}";
            return;
        }

        try
        {
            if (_worshipMediaLauncher(mediaPath))
            {
                StatusText = $"미디어 재생: {Path.GetFileName(mediaPath)}";
            }
            else
            {
                StatusText = $"미디어 파일을 열 수 없습니다: {mediaPath}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"미디어 파일을 열 수 없습니다: {ex.Message}";
        }

        NotifyCommandStates();
    }

    partial void OnOutputItemChanged(LiveQueueItem? value)
    {
        RefreshOutputSurfaceText();
        RebuildOutputLyricsPages();
        OnPropertyChanged(nameof(IsOutputPowerPointContext));
        NotifyCommandStates();
    }

    private void RefreshOutputSurfaceText()
    {
        if (OutputItem is not { } item || IsPowerPointItem(item) || IsMediaItem(item))
        {
            OutputLyricsText = string.Empty;
            return;
        }

        if (_session.Current.State != LiveState.Off
            && string.Equals(item.Id, _liveItemId, StringComparison.Ordinal))
        {
            var liveText = JoinOutputBodyText(_session.Current.CurrentItemBodyText, _session.Current.CurrentItemBodyText2);
            if (!string.IsNullOrWhiteSpace(liveText))
            {
                OutputLyricsText = liveText;
                return;
            }
        }

        OutputLyricsText = BuildOutputSurfaceText(item);
    }

    private static string JoinOutputBodyText(string region1, string region2)
    {
        if (string.IsNullOrWhiteSpace(region2))
        {
            return region1;
        }

        if (string.IsNullOrWhiteSpace(region1))
        {
            return region2;
        }

        return $"{region1}{Environment.NewLine}{Environment.NewLine}{region2}";
    }

    private void PlaySelectedWorshipMediaOnOutput()
    {
        if (SelectedItem is not { } item)
        {
            StatusText = "Output으로 재생할 Worship 항목을 선택하세요.";
            return;
        }

        var mediaPath = ResolveWorshipOutputMediaPath(item);
        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            StatusText = $"Output 미디어 파일을 찾을 수 없습니다: {item.Title}";
            return;
        }

        if (!_output.Current.IsOpen)
        {
            OpenOutput();
        }

        OutputItem = item;
        Media.Load(new MediaPlaybackRequest(
            mediaPath,
            MediaSourceKind.File,
            TimeSpan.Zero,
            InferMediaType(mediaPath)));

        if (Media.PlayPauseCommand.CanExecute(null))
        {
            Media.PlayPauseCommand.Execute(null);
        }

        StatusText = $"Output 미디어 재생: {Path.GetFileName(mediaPath)}";
        NotifyCommandStates();
    }

    private void PlayOutputMedia()
    {
        if (GetOutputNavigationItem() is not { } item)
        {
            StatusText = "Output 미디어를 재생할 항목이 없습니다.";
            NotifyCommandStates();
            return;
        }

        var mediaPath = ResolveWorshipOutputMediaPath(item);
        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            StatusText = $"Output 미디어 파일을 찾을 수 없습니다: {item.Title}";
            NotifyCommandStates();
            return;
        }

        if (!_output.Current.IsOpen)
        {
            OpenOutput();
        }

        OutputItem = item;
        var shouldLoad = !string.Equals(Media.Source, mediaPath, StringComparison.OrdinalIgnoreCase)
            || Media.State is MediaPlaybackState.Empty or MediaPlaybackState.Failed;
        if (shouldLoad)
        {
            Media.Load(new MediaPlaybackRequest(
                mediaPath,
                MediaSourceKind.File,
                TimeSpan.Zero,
                InferMediaType(mediaPath)));
        }

        if (!Media.PlayPauseCommand.CanExecute(null))
        {
            StatusText = $"Output 미디어를 제어할 수 없습니다: {Path.GetFileName(mediaPath)}";
            NotifyCommandStates();
            return;
        }

        Media.PlayPauseCommand.Execute(null);
        var action = Media.State == MediaPlaybackState.Playing ? "재생" : "일시정지";
        StatusText = $"Output 미디어 {action}: {Path.GetFileName(mediaPath)}";
        NotifyCommandStates();
    }

    private string? ResolveWorshipOutputMediaPath(LiveQueueItem item)
    {
        if (IsMediaItem(item) && TryResolveExistingFile(item.ContentPath, out var directMediaPath))
        {
            return directMediaPath;
        }

        if (!string.IsNullOrWhiteSpace(item.ContentPath)
            && IsSupportedWorshipOutputMediaFile(item.ContentPath)
            && TryResolveExistingFile(item.ContentPath, out var contentMediaPath))
        {
            return contentMediaPath;
        }

        return FindWorshipOutputMediaByTitle(item.Title);
    }

    private string? FindWorshipOutputMediaByTitle(string title)
    {
        var root = ResolveWorshipOutputMediaRoot();
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        foreach (var extension in WorshipOutputMediaExtensions)
        {
            var direct = Path.Combine(root, $"{title}{extension}");
            if (File.Exists(direct))
            {
                return direct;
            }
        }

        var normalizedTitle = NormalizeWorshipMediaLookupName(title);
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return null;
        }

        try
        {
            return Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(IsSupportedWorshipOutputMediaFile)
                .FirstOrDefault(file => string.Equals(
                    NormalizeWorshipMediaLookupName(Path.GetFileNameWithoutExtension(file)),
                    normalizedTitle,
                    StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"Output 미디어 폴더 검색 실패: {ex.Message}";
            return null;
        }
    }

    private string? ResolveWorshipOutputMediaRoot()
    {
        var candidates = new[]
        {
            MediaDirectory,
            _settings.Get(EasiSettingKeys.MediaDirectory),
            Path.Combine(_settings.Current.General.WorkingFolder, "Media")
        };

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path.Trim())
            .FirstOrDefault(Directory.Exists);
    }

    private static bool TryResolveExistingFile(string? path, out string resolvedPath)
    {
        resolvedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var candidate = Path.IsPathFullyQualified(path)
                ? path
                : Path.GetFullPath(path);
            if (!File.Exists(candidate))
            {
                return false;
            }

            resolvedPath = candidate;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
    }

    private static bool IsSupportedWorshipOutputMediaFile(string filePath)
        => WorshipOutputMediaExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);

    private static string NormalizeWorshipMediaLookupName(string value)
        => new(value.Where(char.IsLetterOrDigit).ToArray());

    private static bool LaunchWorshipMediaProcess(string mediaPath)
    {
        Process.Start(new ProcessStartInfo(mediaPath)
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal
        });
        return true;
    }

    private void OnOutputChanged(object? sender, OutputWindowChangedEventArgs e)
    {
        NotifyCommandStates();

        // 출력 창이 열리면 현재 선택된 PPT 를 그 해상도로 다시 렌더한다 — 항목을 먼저 고르고
        // 출력을 나중에 여는 흐름에서도 송출이 선명하도록(직전 렌더가 미리보기 크기로 남는 문제 해소).
        // 닫힘은 송출하지 않으므로 갱신 불필요(IsOpen 가드). 같은 해상도 갱신은 렌더 캐시가 흡수.
        // (트리거는 Open + MoveTo(OnSelectedOutputDisplayChanged) — 둘 다 IsOpen 가드로 안전. MoveTo 시 새 모니터 해상도로 PPT 재렌더.)
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

    // Preview 하단 슬라이드 버튼 — FrmMain MoveToSlide(Gf.PreviewItem, ...)처럼 현재 PreviewItem 내부에서만 이동한다.
    // PPT 는 슬라이드 렌더를 넘기고, 곡/성경은 Preview 가사 페이지를 넘긴다. Output/live 는 건드리지 않는다.
    private async Task NextPreviewPageAsync()
    {
        if (IsPowerPointSlideNavReady())
        {
            await GoToSlideAsync(PowerPoint.SlideNumber + 1).ConfigureAwait(true);
            return;
        }

        if (CanGoNextLyricsPage())
        {
            NextLyricsPage();
            NotifyCommandStates();
        }
    }

    private async Task PreviousPreviewPageAsync()
    {
        if (IsPowerPointSlideNavReady())
        {
            await GoToSlideAsync(PowerPoint.SlideNumber - 1).ConfigureAwait(true);
            return;
        }

        if (CanGoPreviousLyricsPage())
        {
            PreviousLyricsPage();
            NotifyCommandStates();
        }
    }

    // Preview PPT 썸네일/키보드 직접 이동 — 현재 선택된 PPT 의 미리보기만 지정 슬라이드로 다시 렌더한다.
    // FrmMain 처럼 PreviewItem 과 OutputItem 을 분리해야 하므로, 선택 항목이 현재 live 항목과 같더라도
    // Output 은 여기서 갱신하지 않는다. live Output 이동은 GoToOutputSlideAsync 전용 경로가 맡는다.
    private async Task GoToSlideAsync(int target)
    {
        if (SelectedItem is not { Kind: LiveItemKinds.PowerPoint, ContentPath: { Length: > 0 } path })
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

        NotifyCommandStates();
    }

    private bool CanGoToSlide(int target)
        => IsPowerPointSlideNavReady() && target >= 1 && target <= PowerPoint.SlideCount;

    private bool CanGoNextSlide()
        => (IsPowerPointSlideNavReady() && PowerPoint.SlideNumber < PowerPoint.SlideCount)
            || CanGoNextLyricsPage();

    private bool CanGoPreviousSlide()
        => (IsPowerPointSlideNavReady() && PowerPoint.SlideNumber > 1)
            || CanGoPreviousLyricsPage();

    private bool IsPowerPointSlideNavReady()
        => SelectedItem is { Kind: LiveItemKinds.PowerPoint } selected
            && PowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && PowerPoint.SlideCount > 0
            && !string.IsNullOrEmpty(selected.ContentPath);

    private async Task GoToOutputSlideAsync(int target)
    {
        if (GetOutputPowerPointNavigationItem() is not { ContentPath: { Length: > 0 } path } item)
        {
            return;
        }

        if (OutputPowerPoint.State != Rendering.PowerPointPreviewState.Ready
            || target < 1 || target > OutputPowerPoint.SlideCount
            || target == OutputPowerPoint.SlideNumber)
        {
            return;
        }

        var (width, height) = ResolvePptRenderSize();
        await OutputPowerPoint.LoadAsync(path, target, width, height).ConfigureAwait(true);
        EnsureOutputPowerPointThumbnails(path, OutputPowerPoint.SlideCount);

        if (_session.Current.State == LiveState.Active)
        {
            var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
            _session.GoLive(ResolveLiveProjection(item with { SlideNumber = target }, OutputPowerPoint), monitorName);
        }
        else
        {
            OutputItem = item with { SlideNumber = target };
            StatusText = OutputPowerPoint.SlideCount > 1
                ? $"Output 슬라이드 {target}/{OutputPowerPoint.SlideCount} 준비"
                : StatusText;
        }

        NotifyCommandStates();
    }

    private async Task NextOutputPageAsync()
    {
        if (IsOutputPowerPointSlideNavReady())
        {
            await GoToOutputSlideAsync(OutputPowerPoint.SlideNumber + 1).ConfigureAwait(true);
            return;
        }

        if (GetOutputLyricsNavigationItem() is { } item)
        {
            MoveOutputLyricsPage(GetOutputLyricsPageIndex(item) + 1);
        }
    }

    private async Task PreviousOutputPageAsync()
    {
        if (IsOutputPowerPointSlideNavReady())
        {
            await GoToOutputSlideAsync(OutputPowerPoint.SlideNumber - 1).ConfigureAwait(true);
            return;
        }

        if (GetOutputLyricsNavigationItem() is { } item)
        {
            MoveOutputLyricsPage(GetOutputLyricsPageIndex(item) - 1);
        }
    }

    private void MoveOutputLyricsPage(int target)
    {
        if (GetOutputLyricsNavigationItem() is not { } item)
        {
            return;
        }

        var pageModel = BuildLyricsPageModel(item);
        var current = GetOutputLyricsPageIndex(item);
        if (target < 0 || target >= pageModel.Count || target == current)
        {
            return;
        }

        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputLyricsPage(item, target, pageModel.Count);
            return;
        }

        PrepareOutputLyricsPage(item, target, pageModel.Count);
    }

    private bool CanGoToOutputSlide(int target)
        => IsOutputPowerPointSlideNavReady() && target >= 1 && target <= OutputPowerPoint.SlideCount;

    private bool CanGoNextOutputSlide()
        => (IsOutputPowerPointSlideNavReady() && OutputPowerPoint.SlideNumber < OutputPowerPoint.SlideCount)
            || CanGoNextOutputLyricsPage();

    private bool CanGoPreviousOutputSlide()
        => (IsOutputPowerPointSlideNavReady() && OutputPowerPoint.SlideNumber > 1)
            || CanGoPreviousOutputLyricsPage();

    private bool IsOutputPowerPointSlideNavReady()
        => GetOutputPowerPointNavigationItem() is { ContentPath.Length: > 0 }
            && OutputPowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && OutputPowerPoint.SlideCount > 0;

    private LiveQueueItem? GetLiveQueueItem()
        => Queue.FirstOrDefault(item =>
            string.Equals(item.Id, _liveItemId, StringComparison.Ordinal));

    private LiveQueueItem? GetLivePowerPointItem()
        => Queue.FirstOrDefault(item =>
            string.Equals(item.Id, _liveItemId, StringComparison.Ordinal)
            && IsPowerPointItem(item));

    private LiveQueueItem? GetOutputPowerPointNavigationItem()
    {
        if (_session.Current.State == LiveState.Active)
        {
            return GetLivePowerPointItem();
        }

        return _session.Current.State == LiveState.Off
            && OutputItem is { } output
            && IsPowerPointItem(output)
            ? output
            : null;
    }

    private bool CanGoNextOutputLyricsPage()
    {
        if (GetOutputLyricsNavigationItem() is not { } item)
        {
            return false;
        }

        var pageModel = BuildLyricsPageModel(item);
        return pageModel.Count > 1 && GetOutputLyricsPageIndex(item) < pageModel.Count - 1;
    }

    private bool CanGoPreviousOutputLyricsPage()
    {
        if (GetOutputLyricsNavigationItem() is not { } item)
        {
            return false;
        }

        var pageModel = BuildLyricsPageModel(item);
        return pageModel.Count > 1 && GetOutputLyricsPageIndex(item) > 0;
    }

    private void JumpToOutputLyricsSection(string? label)
    {
        if (GetOutputLyricsNavigationItem() is not { } item)
        {
            return;
        }

        var pageModel = BuildLyricsPageModel(item);
        var target = IndexOfPageLabel(pageModel.Labels, label);
        if (target < 0)
        {
            return;
        }

        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputLyricsPage(item, target, pageModel.Count);
            return;
        }

        PrepareOutputLyricsPage(item, target, pageModel.Count);
    }

    private void PublishOutputLyricsPage(LiveQueueItem item, int target, int pageCount)
    {
        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        _session.GoLive(ResolveLiveProjection(item with { LyricsPageIndex = target }), monitorName);
        StatusText = pageCount > 1
            ? $"Output 가사 {target + 1}/{pageCount}절"
            : StatusText;
        NotifyCommandStates();
    }

    private bool CanJumpToOutputLyricsSection(string? label)
    {
        if (GetOutputLyricsNavigationItem() is not { } item)
        {
            return false;
        }

        var pageModel = BuildLyricsPageModel(item);
        return IndexOfPageLabel(pageModel.Labels, label) >= 0;
    }

    private LiveQueueItem? GetOutputLyricsNavigationItem()
    {
        if (_session.Current.State == LiveState.Active)
        {
            return GetLiveLyricsItem();
        }

        return _session.Current.State == LiveState.Off
            && OutputItem is { } output
            && IsLyricsPaginated(output)
            ? output
            : null;
    }

    private int GetOutputLyricsPageIndex(LiveQueueItem item)
        => _session.Current.State == LiveState.Active
            ? _session.Current.CurrentLyricsPageIndex
            : item.LyricsPageIndex;

    private void PrepareOutputLyricsPage(LiveQueueItem item, int target, int pageCount)
    {
        var updated = item with { LyricsPageIndex = target };
        OutputItem = updated;
        OutputLyricsText = BuildOutputSurfaceText(updated);
        StatusText = pageCount > 1
            ? $"Output 가사 {target + 1}/{pageCount}절 준비"
            : StatusText;
        NotifyCommandStates();
    }

    private string BuildOutputSurfaceText(LiveQueueItem item)
    {
        if (item.Kind == LiveItemKinds.Notice)
        {
            return item.Lyrics ?? string.Empty;
        }

        if (!IsLyricsPaginated(item))
        {
            return item.Lyrics ?? string.Empty;
        }

        var projected = item with
        {
            ShowNotations = _settings.Get(EasiSettingKeys.LyricsMonitorShowNotations),
            TransposeSemitones = LiveTransposeSemitones,
        };

        if (projected.Kind == LiveItemKinds.Bible)
        {
            var body = LyricsDisplayFormatter.NormalizeText(projected.Lyrics ?? string.Empty);
            var guarded = LyricsDisplayFormatter.GuardLiteralNotation(body);
            if (LyricsDisplayFormatter.HasRegion2(guarded))
            {
                var page = LyricsDisplayFormatter.GetRegionPage(guarded, projected.LyricsPageIndex, projected.Sequence);
                return JoinOutputBodyText(
                    LyricsDisplayFormatter.UnguardLiteralNotation(page.Region1),
                    LyricsDisplayFormatter.UnguardLiteralNotation(page.Region2));
            }

            return LyricsDisplayFormatter.UnguardLiteralNotation(
                LyricsDisplayFormatter.GetVersePage(guarded, projected.LyricsPageIndex, projected.Sequence));
        }

        var lyrics = LyricsDisplayFormatter.ExpandNotations(
            projected.Lyrics,
            projected.ShowNotations,
            projected.TransposeSemitones);
        if (LyricsDisplayFormatter.HasRegion2(lyrics))
        {
            var page = LyricsDisplayFormatter.GetRegionPage(lyrics, projected.LyricsPageIndex, projected.Sequence);
            return JoinOutputBodyText(page.Region1, page.Region2);
        }

        return LyricsDisplayFormatter.GetVersePage(lyrics, projected.LyricsPageIndex, projected.Sequence);
    }

    private LiveQueueItem? GetLiveLyricsItem()
        => Queue.FirstOrDefault(item =>
            string.Equals(item.Id, _liveItemId, StringComparison.Ordinal)
            && IsLyricsPaginated(item));

    // PPT 미리보기 VM 의 상태/슬라이드 변화에 슬라이드 이동 커맨드 활성 상태를 동기화.
    private void OnPowerPointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PowerPoint.PreviewImage))
        {
            OnPropertyChanged(nameof(PreviewVisualSource));
            OnPropertyChanged(nameof(HasPreviewVisualSource));
        }

        if (e.PropertyName is nameof(PowerPoint.State)
            or nameof(PowerPoint.SlideNumber)
            or nameof(PowerPoint.SlideCount)
            or nameof(PowerPoint.PreviewImage))
        {
            NotifyCommandStates();
        }
    }

    private void OnOutputPowerPointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(OutputPowerPoint.State)
            or nameof(OutputPowerPoint.SlideNumber)
            or nameof(OutputPowerPoint.SlideCount))
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

        SetLiveItemId(null);
        OutputItem = null;
        OutputPowerPoint.Clear();
        _outputThumbnailDeckPath = null;
        LiveBar.OutputMonitorName = string.Empty;
        StatusText = "출력 창 닫힘";
        _telemetry.Record(MainCommandIds.OutputClose, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    private bool CanCopyPreviewToOutput()
        => SelectedItem is not null && !HasPowerPointLimitViolation;

    private void CopyPreviewToOutput()
    {
        var item = PrepareOutputFromPreview();
        if (item is null)
        {
            return;
        }

        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputItemAtPreviewPage(item, MainCommandIds.LivePreviewToOutput);
            return;
        }

        RefreshHiddenOutputPayload(item, MainCommandIds.LivePreviewToOutput);
        StatusText = $"Output 준비: {item.Title}";
        NotifyCommandStates();
    }

    private void CopyPreviewToOutputAndNext()
    {
        var item = PrepareOutputFromPreview();
        if (item is null)
        {
            return;
        }

        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputItemAtPreviewPage(item, MainCommandIds.LivePreviewToOutput);
        }
        else
        {
            RefreshHiddenOutputPayload(item, MainCommandIds.LivePreviewToOutput);
        }

        var next = MovePreviewSelectionToNext(item);
        StatusText = next is null
            ? $"Output 준비: {item.Title}"
            : $"Output 준비: {item.Title} / Preview 다음: {next.Title}";
        NotifyCommandStates();
    }

    private void CopyPreviewToOutputShortcut()
        => CopyPreviewToOutputFromShortcut(clearBlackAfterCopy: false, MainCommandIds.LivePreviewToOutput);

    private void CopyPreviewToOutputAndClearBlack()
        => CopyPreviewToOutputFromShortcut(clearBlackAfterCopy: true, MainCommandIds.LivePreviewToOutputClearBlack);

    private void CopyPreviewToOutputFromShortcut(bool clearBlackAfterCopy, string commandId)
    {
        var item = PrepareOutputFromPreview();
        if (item is null)
        {
            return;
        }

        var shouldPublish = _session.Current.State == LiveState.Active
            || (clearBlackAfterCopy && IsOutputBlackActive);
        if (shouldPublish)
        {
            PublishOutputItemAtPreviewPage(item, commandId);
            return;
        }

        RefreshHiddenOutputPayload(item, commandId);
        StatusText = $"Output 준비: {item.Title}";
        NotifyCommandStates();
    }

    private Task PreviewToLiveAsync()
    {
        var item = PrepareOutputFromPreview();
        if (item is null)
        {
            return Task.CompletedTask;
        }

        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputItemAtPreviewPage(item, MainCommandIds.LiveGo);
            return Task.CompletedTask;
        }

        if (_session.Current.State == LiveState.Hidden)
        {
            RefreshHiddenOutputPayload(item, MainCommandIds.LiveGo);
            StatusText = $"Output 준비: {item.Title}";
            NotifyCommandStates();
            return Task.CompletedTask;
        }

        if (!_output.Current.IsOpen)
        {
            OpenOutput();
        }

        PublishOutputItemAtPreviewPage(item, MainCommandIds.LiveGo);
        return Task.CompletedTask;
    }

    private bool CanPreviewToLive()
        => CanCopyPreviewToOutput();

    private void PublishOutputItemAtPreviewPage(LiveQueueItem item, string commandId)
        => PublishOutputItem(item, commandId, GetPreviewLyricsPageIndex(item));

    private int GetPreviewLyricsPageIndex(LiveQueueItem item)
        => ReferenceEquals(item, SelectedItem)
            ? LyricsPageIndex
            : item.LyricsPageIndex;

    private bool RefreshHiddenOutputPayload(LiveQueueItem item, string commandId)
    {
        if (_session.Current.State != LiveState.Hidden)
        {
            return false;
        }

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        var lyricsPageIndex = ReferenceEquals(item, SelectedItem)
            ? LyricsPageIndex
            : item.LyricsPageIndex;
        var projection = item with { LyricsPageIndex = lyricsPageIndex };
        if (IsPowerPointItem(item) && OutputPowerPoint.SlideNumber > 0)
        {
            projection = projection with { SlideNumber = OutputPowerPoint.SlideNumber };
        }

        SetLiveItemId(item.Id);
        LiveTransposeSemitones = 0;
        _session.UpdateHiddenContent(ResolveLiveProjection(projection, OutputPowerPoint), monitorName);
        _telemetry.Record(commandId, succeeded: true, $"{item.Title} (hidden)");
        return true;
    }

    private LiveQueueItem? PrepareOutputFromPreview()
    {
        if (SelectedItem is not { } item)
        {
            StatusText = "Output으로 복사할 Preview 항목이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        OutputItem = item;
        PrepareOutputPowerPointForPublish(item);
        return item;
    }

    private LiveQueueItem? MovePreviewSelectionToNext(LiveQueueItem copiedItem)
    {
        var index = IndexOfReference(copiedItem);
        if (index < 0 || index >= Queue.Count - 1)
        {
            return null;
        }

        SelectedItem = Queue[index + 1];
        return SelectedItem;
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
            NotifyOutputLiveSafetyProperties();
            return;
        }

        PublishSelectedItem();
    }

    // 상단/메뉴 "송출 후 다음" — 선택 항목을 라이브로 송출하고 곧바로 다음 항목으로 선택을 옮긴다(자동 다음 설정과 무관).
    // FrmMain 지역 btnToOutputMoveNext 의 비송출 복사+다음은 CopyPreviewToOutputAndNextCommand 가 담당한다.
    private async Task SendToOutputAndNextAsync()
    {
        if (SelectedItem is null)
        {
            _telemetry.Record(MainCommandIds.LiveGo, succeeded: false, "선택 항목 없음");
            return;
        }

        var ok = await ConfirmLiveSafetyAsync(
            MainCommandIds.LiveGo,
            $"'{SelectedItem.Title}' 항목을 송출하고 다음 항목으로 넘어갈까요?",
            "선택 항목이 즉시 출력 화면에 표시되고 선택이 다음 항목으로 이동합니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            NotifyOutputLiveSafetyProperties();
            return;
        }

        // 자동 advance 를 끈 채 송출한 뒤(중복 이동 방지), 설정과 무관하게 명시적으로 다음 항목으로 이동.
        var published = SelectedItem;
        PublishSelectedItem(autoAdvance: false);
        AdvanceSelectionToNext(published);
    }

    private async Task StopLiveAsync()
    {
        var ok = await ConfirmLiveSafetyAsync(
            MainCommandIds.LiveStop,
            "현재 라이브 송출을 중지할까요?",
            "출력 화면이 대기 상태로 돌아갑니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            NotifyOutputLiveSafetyProperties();
            return;
        }

        _session.Stop();
        SetLiveItemId(null);
        OutputPowerPoint.Clear();
        _outputThumbnailDeckPath = null;
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

    private async Task ExecuteLiveNavigationShortcutAsync(int direction)
    {
        if (_session.Current.State == LiveState.Active)
        {
            var slideCommand = direction > 0
                ? NextOutputSlideCommand
                : PreviousOutputSlideCommand;
            if (slideCommand.CanExecute(null))
            {
                await slideCommand.ExecuteAsync(null).ConfigureAwait(true);
                return;
            }

            if (_settings.Get(EasiSettingKeys.AdvanceNextItem))
            {
                var itemCommand = direction > 0
                    ? NextOutputItemCommand
                    : PreviousOutputItemCommand;
                if (itemCommand.CanExecute(null))
                {
                    await itemCommand.ExecuteAsync(null).ConfigureAwait(true);
                    return;
                }
            }

            StatusText = direction > 0
                ? "Output 다음 슬라이드/절이 없습니다."
                : "Output 이전 슬라이드/절이 없습니다.";
            NotifyCommandStates();
            return;
        }

        var previewCommand = direction > 0
            ? NextItemCommand
            : PreviousItemCommand;
        if (previewCommand.CanExecute(null))
        {
            previewCommand.Execute(null);
        }
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

    private bool CanMoveOutputNext()
        => TryGetOutputNavigationIndex(out var index) && index < Queue.Count - 1;

    private bool CanMoveOutputPrevious()
        => TryGetOutputNavigationIndex(out var index) && index > 0;

    private bool CanJumpToNextNonRotateOutputItem()
        => TryFindNextNonRotatingOutputItem(out _);

    private bool TryGetOutputNavigationIndex(out int index)
    {
        index = -1;
        var item = GetOutputNavigationItem();
        if (item is null)
        {
            return false;
        }

        index = IndexOfReference(item);
        if (index >= 0)
        {
            return true;
        }

        for (var i = 0; i < Queue.Count; i++)
        {
            if (string.Equals(Queue[i].Id, item.Id, StringComparison.Ordinal))
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    private LiveQueueItem? GetOutputNavigationItem()
    {
        if (_session.Current.State != LiveState.Off && GetLiveQueueItem() is { } live)
        {
            return live;
        }

        if (OutputItem is { } output)
        {
            return Queue.FirstOrDefault(item => string.Equals(item.Id, output.Id, StringComparison.Ordinal))
                ?? output;
        }

        return null;
    }

    private async Task MoveOutputItemAsync(int delta)
    {
        if (!TryGetOutputNavigationIndex(out var index))
        {
            return;
        }

        var targetIndex = index + delta;
        if (targetIndex < 0 || targetIndex >= Queue.Count)
        {
            return;
        }

        var target = Queue[targetIndex];
        await PrepareOutputItemForNavigationAsync(target).ConfigureAwait(true);
        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputItem(target, delta > 0 ? MainCommandIds.LiveNext : MainCommandIds.LivePrevious);
            return;
        }

        StatusText = $"Output 준비: {target.Title}";
        NotifyCommandStates();
    }

    private async Task MoveOutputItemToIndexAsync(int targetIndex, string commandId)
    {
        if (!TryGetOutputNavigationIndex(out var index))
        {
            return;
        }

        if (targetIndex < 0 || targetIndex >= Queue.Count || targetIndex == index)
        {
            return;
        }

        var target = Queue[targetIndex];
        await PrepareOutputItemForNavigationAsync(target).ConfigureAwait(true);
        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputItem(target, commandId);
            return;
        }

        StatusText = $"Output 준비: {target.Title}";
        NotifyCommandStates();
    }

    private async Task PrepareOutputItemForNavigationAsync(LiveQueueItem item)
    {
        OutputItem = item;

        if (!IsPowerPointItem(item) || string.IsNullOrEmpty(item.ContentPath))
        {
            OutputPowerPoint.Clear();
            _outputThumbnailDeckPath = null;
            return;
        }

        if (PowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && string.Equals(PowerPoint.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            OutputPowerPoint.CopyFrom(PowerPoint);
            EnsureOutputPowerPointThumbnails(item.ContentPath, OutputPowerPoint.SlideCount);
            return;
        }

        var slide = item.SlideNumber <= 0 ? 1 : item.SlideNumber;
        var (width, height) = ResolvePptRenderSize();
        await OutputPowerPoint.LoadAsync(item.ContentPath, slide, width, height).ConfigureAwait(true);
        if (OutputPowerPoint.State == Rendering.PowerPointPreviewState.Ready)
        {
            EnsureOutputPowerPointThumbnails(item.ContentPath, OutputPowerPoint.SlideCount);
        }
    }

    private void PublishOutputItem(LiveQueueItem item, string commandId, int lyricsPageIndex = 0)
    {
        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        OutputItem = item;
        SetLiveItemId(item.Id);
        LiveTransposeSemitones = 0;

        var projection = item with { LyricsPageIndex = lyricsPageIndex };
        if (IsPowerPointItem(item) && OutputPowerPoint.SlideNumber > 0)
        {
            projection = projection with { SlideNumber = OutputPowerPoint.SlideNumber };
        }

        _session.GoLive(ResolveLiveProjection(projection, OutputPowerPoint), monitorName);
        StatusText = $"LIVE: {item.Title}";
        _telemetry.Record(commandId, succeeded: true, item.Title);
        NotifyCommandStates();
    }

    private async Task JumpToNextNonRotateOutputItemAsync()
    {
        if (!TryFindNextNonRotatingOutputItem(out var target))
        {
            StatusText = "다음 회전 제외 Output 항목이 없습니다.";
            NotifyCommandStates();
            return;
        }

        await PrepareOutputItemForNavigationAsync(target).ConfigureAwait(true);
        if (_session.Current.State == LiveState.Active)
        {
            PublishOutputItem(target, MainCommandIds.LiveNext);
            return;
        }

        StatusText = $"Output 회전 제외 항목 준비: {target.Title}";
        NotifyCommandStates();
    }

    private bool TryFindNextNonRotatingOutputItem(out LiveQueueItem target)
    {
        target = null!;
        if (!TryGetOutputNavigationIndex(out var index))
        {
            return false;
        }

        for (var i = index + 1; i < Queue.Count; i++)
        {
            if (IsNonRotatingOutputItem(Queue[i]))
            {
                target = Queue[i];
                return true;
            }
        }

        return false;
    }

    private bool IsNonRotatingOutputItem(LiveQueueItem item)
    {
        if (IsPowerPointItem(item))
        {
            return TryGetKnownPowerPointSlideCount(item, out var slideCount) && slideCount <= 1;
        }

        if (IsLyricsPaginated(item))
        {
            return BuildLyricsPageModel(item).Count <= 1;
        }

        return true;
    }

    private bool TryGetKnownPowerPointSlideCount(LiveQueueItem item, out int slideCount)
    {
        slideCount = 0;
        if (string.IsNullOrWhiteSpace(item.ContentPath))
        {
            return true;
        }

        if (OutputPowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && string.Equals(OutputPowerPoint.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            slideCount = OutputPowerPoint.SlideCount;
            return true;
        }

        if (PowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && string.Equals(PowerPoint.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            slideCount = PowerPoint.SlideCount;
            return true;
        }

        return false;
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

    private bool CanToggleOutputLive()
        => _session.Current.State is LiveState.Active or LiveState.Hidden || CanGoLive();

    private async Task ToggleOutputBlackAsync()
    {
        if (IsOutputBlackActive)
        {
            RestoreOutput();
            return;
        }

        await HideOutputAsync(blackout: true).ConfigureAwait(true);
    }

    private async Task ToggleOutputClearAsync()
    {
        if (IsOutputClearActive)
        {
            RestoreOutput();
            return;
        }

        await ClearOutputAsync().ConfigureAwait(true);
    }

    private async Task ToggleOutputLiveAsync()
    {
        if (_session.Current.State == LiveState.Hidden)
        {
            RestoreOutput();
            return;
        }

        if (_session.Current.State == LiveState.Active)
        {
            await StopLiveAsync().ConfigureAwait(true);
            return;
        }

        if (CanGoLive())
        {
            await GoLiveAsync().ConfigureAwait(true);
            return;
        }

        NotifyOutputLiveSafetyProperties();
    }

    private async Task HideOutputAsync(bool blackout)
    {
        var actionName = blackout ? MainCommandIds.LiveBlack : MainCommandIds.LiveHide;
        var ok = await ConfirmLiveSafetyAsync(
            actionName,
            blackout ? "현재 송출을 검은 화면으로 전환할까요?" : "현재 송출을 숨길까요?",
            "라이브 출력 상태가 즉시 바뀝니다. 5초 안에 확인하지 않으면 취소됩니다.").ConfigureAwait(true);
        if (!ok)
        {
            NotifyOutputLiveSafetyProperties();
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
            NotifyOutputLiveSafetyProperties();
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
            // PPT 덱: live Output 첫 슬라이드로. 이미 1번이면 GoToOutputSlideAsync 가 무시하므로(target==현재) Refresh 로 강제 재렌더.
            if (OutputPowerPoint.SlideNumber <= 1)
            {
                _session.Refresh();
            }
            else
            {
                await GoToOutputSlideAsync(1).ConfigureAwait(true);
            }
        }
        else
        {
            // 곡: live Output 첫 절로 되돌려 출력 재송출. Preview 절 인덱스는 건드리지 않는다.
            var pageModel = BuildLyricsPageModel(item);
            if (pageModel.Count > 0)
            {
                PublishOutputLyricsPage(item, 0, pageModel.Count);
            }
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

    // autoAdvance: true 면 "자동 다음 항목" 설정에 따라 선택을 다음으로 옮긴다(기존 동작).
    // false 면 옮기지 않는다 — "전송 후 다음"(btnToOutputMoveNext) 처럼 호출부가 직접 이동을 제어할 때 쓴다.
    private void PublishSelectedItem(bool autoAdvance = true)
    {
        if (SelectedItem is null)
        {
            _telemetry.Record(MainCommandIds.LiveGo, succeeded: false, "선택 항목 없음");
            return;
        }

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        OutputItem = SelectedItem;
        SetLiveItemId(SelectedItem.Id); // Output 전용 이동/상태 표시가 참조할 live 항목 기록.
        PrepareOutputPowerPointForPublish(SelectedItem);
        // 새 곡을 송출하면 조옮김을 원조(0)로 초기화 — 각 곡이 작성된 키에서 시작하도록(절·슬라이드 이동은 유지).
        LiveTransposeSemitones = 0;
        // 가사 항목이면 현재 절 인덱스를 투영에 얹는다(절 단위 페이지네이션 — PR B).
        var projection = SelectedItem with { LyricsPageIndex = LyricsPageIndex };
        _session.GoLive(ResolveLiveProjection(projection, OutputPowerPoint), monitorName);
        StatusText = $"LIVE: {SelectedItem.Title}";
        _telemetry.Record(MainCommandIds.LiveGo, succeeded: true, StatusText);
        if (autoAdvance)
        {
            AdvanceSelectionAfterPublish(SelectedItem);
        }

        NotifyCommandStates();
    }

    private void PrepareOutputPowerPointForPublish(LiveQueueItem item)
    {
        if (IsPowerPointItem(item)
            && PowerPoint.State == Rendering.PowerPointPreviewState.Ready
            && !string.IsNullOrEmpty(item.ContentPath)
            && string.Equals(PowerPoint.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            OutputPowerPoint.CopyFrom(PowerPoint);
            EnsureOutputPowerPointThumbnails(item.ContentPath, OutputPowerPoint.SlideCount);
            return;
        }

        OutputPowerPoint.Clear();
        _outputThumbnailDeckPath = null;
    }

    private void EnsureOutputPowerPointThumbnails(string filePath, int slideCount)
    {
        if (slideCount <= 0
            || string.Equals(_outputThumbnailDeckPath, filePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _outputThumbnailDeckPath = filePath;
        _ = OutputPowerPoint.LoadThumbnailsAsync(filePath, slideCount, PptThumbnailWidth, PptThumbnailHeight);
    }

    // 출력 송출 항목 결정 — PPT 항목이고 지정한 PPT VM(Preview/Output)의 슬라이드 렌더가 준비됐으면
    // 해당 이미지를 출력 창 PreviewSource 에 실은 복사본을 만든다(G1.2 출력 송출).
    // LiveQueueItem 은 불변(init) 이므로 큐를 건드리지 않고 with 로 전이 복사본만 만든다.
    // (복사본은 큐에 넣지 않는다 — IndexOf/자동 다음 항목은 원본 SelectedItem 기준이어야 함.)
    //
    // 신원 가드: 렌더는 항목 선택 시 fire-and-forget 로 돌아가므로, 빠른 전환 경쟁에서 PreviewImage 가
    // "다른 항목"의 stale 슬라이드일 수 있다. 그래서 단순히 Ready 인지가 아니라, VM 이 마지막으로 성공
    // 렌더한 파일이 송출 항목의 파일과 일치할 때만 슬라이드를 싣는다(불일치/미준비면 타이틀만 — 안전 강등).
    // 슬라이드 번호는 PPT VM 의 현재 렌더 슬라이드를 "단일 진실"로 신뢰하고
    // 송출 항목에도 반영한다 — Output 전용 슬라이드 이동으로 item.SlideNumber 와 실제 렌더 슬라이드가 달라져도
    // (이동한 슬라이드가 그대로 송출되고, 재개·재송출 시에도 일관). 파일이 다르면(cross-item stale) 거른다.
    // 선택 항목의 "개별 서식 사용" 토글(레거시 Ind_checkBox "Use Individual Settings").
    // 레코드는 불변이라 큐의 해당 인스턴스를 토글된 복사본으로 교체한다(참조 일치로 정확히 그 항목만).
    // 라이브 항목이면 즉시 재송출해 서식 변화를 화면에 반영한다.
    private void ToggleUseIndividualFormatting()
    {
        if (SelectedItem is not { } item)
        {
            return;
        }

        var index = IndexOfReference(item);
        if (index < 0)
        {
            return;
        }

        var updated = item with { UseIndividualFormatting = !item.UseIndividualFormatting };
        Queue[index] = updated;
        SelectedItem = updated; // 주의: 선택 변경은 RefreshLyricsPages 로 VM 의 LyricsPageIndex 를 0 으로 리셋한다.
        StatusText = updated.UseIndividualFormatting ? "개별 서식 사용" : "전역 기본 서식 사용";

        // 라이브 송출 중인 항목이면 같은 절로 다시 송출해 서식(색·정렬·폰트·배경)을 즉시 반영한다.
        // VM 의 LyricsPageIndex(방금 0 으로 리셋됨)가 아니라 세션의 실제 라이브 절을 쓰는 RepublishLiveSongForBodyChange 를
        // 써야 라이브 화면이 0절로 튀지 않는다(큐의 교체된 인스턴스를 찾아 새 UseIndividualFormatting 플래그를 반영).
        RepublishLiveSongForBodyChange();

        NotifyCommandStates();
    }

    /// <summary>
    /// 선택한 곡 항목의 절 순서를 편집할 수 있는가 — 곡(가사 있음)일 때만 true. 성경/PPT/미디어/공지는 절 순서 개념이 없어 false.
    /// 절 순서 입력칸의 활성화에 바인딩한다.
    /// </summary>
    public bool CanEditSelectedItemSequence
        => SelectedItem is { Kind: LiveItemKinds.Song } item && !string.IsNullOrWhiteSpace(item.Lyrics);

    /// <summary>
    /// 선택한 곡 항목의 절 순서(레거시 절 순서 모델, LiveQueueItem.Sequence)와 양방향 바인딩. 예: "1 2 C 3 C C"
    /// — 가사에 [라벨]로 절을 정의한 뒤 라벨을 나열해 부르는 순서·반복을 지정한다(후렴 반복 등). 비우면 가사를 선형으로 페이지네이션(기본).
    /// </summary>
    public string SelectedItemSequenceInput
    {
        get => SelectedItem?.Sequence ?? string.Empty;
        set => CommitSelectedItemSequence(value);
    }

    // 선택 곡 항목의 절 순서를 바꾼다 — 레코드는 불변이라 큐의 그 인스턴스를 새 Sequence 복사본으로 교체(참조 일치로 정확히 그 항목만).
    // 선택을 교체본으로 바꾸면 RefreshLyricsPages 가 새 절 순서로 절 수·라벨을 다시 계산하고, 라이브 항목이면 즉시 재송출해 반영한다.
    private void CommitSelectedItemSequence(string? value)
    {
        if (!CanEditSelectedItemSequence || SelectedItem is not { } item)
        {
            return; // 곡이 아니거나 선택 없음 — 절 순서 편집 대상이 아님.
        }

        // 빈 입력은 "순서 없음"(null) → 선형 페이지네이션으로 복귀. 앞뒤 공백은 다듬는다.
        var trimmed = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (string.Equals(trimmed ?? string.Empty, item.Sequence ?? string.Empty, StringComparison.Ordinal))
        {
            return; // 같은 값이면 무시 — 불필요한 교체·재송출 방지.
        }

        var index = IndexOfReference(item);
        if (index < 0)
        {
            return;
        }

        var updated = item with { Sequence = trimmed };
        Queue[index] = updated;
        SelectedItem = updated; // 선택 변경 → RefreshLyricsPages 로 새 절 순서 기준 페이지·라벨 재계산(LyricsPageIndex 0 리셋).
        StatusText = trimmed is null ? "절 순서 해제(선형 송출)" : $"절 순서: {trimmed}";

        // 라이브 송출 중이면 같은 절로 다시 송출해 새 절 순서를 즉시 반영(세션 실제 라이브 절 사용 — 0절로 안 튀게).
        RepublishLiveSongForBodyChange();

        NotifyCommandStates();
    }

    /// <summary>
    /// 선택한 항목에 항목별 서식(색·정렬·크기·글꼴·배경·강조 — 이 항목만)을 편집할 수 있는지 — <b>곡 또는 성경</b> 항목이고 본문(가사/구절)이 있을 때.
    /// 성경도 본문(구절)을 회중 화면에 렌더하고 송출 경로(GoLive)가 Kind 무관이라, 곡과 똑같이 항목별 서식을 적용할 수 있다(레거시 Ind_* 도 항목 종류 무관).
    /// 우클릭 "이 항목 글자색"·"이 항목 정렬" 등 메뉴의 활성화에 공통으로 쓴다(이름은 호환을 위해 Color 유지). 절 순서 편집은 곡만이라 별도 조건(CanEditSelectedItemSequence).
    /// </summary>
    public bool CanEditSelectedItemColor
        => SelectedItem is { } item && IsPerItemFormattable(item);

    /// <summary>
    /// 항목별 서식(색·정렬·크기·글꼴·배경·강조)을 적용할 수 있는 항목인지 — <b>곡 또는 성경</b>이고 본문(가사/구절)이 있을 때.
    /// 선택 항목 게이트(<see cref="CanEditSelectedItemColor"/>)와 전 항목 일괄 적용이 같은 기준을 쓰도록 한 곳에 모은다(중복 방지).
    /// </summary>
    private static bool IsPerItemFormattable(LiveQueueItem item)
        => item.Kind is LiveItemKinds.Song or LiveItemKinds.Bible && !string.IsNullOrWhiteSpace(item.Lyrics);

    /// <summary>
    /// "이 항목 서식 모두 지우기"를 누를 수 있는지 — 서식 편집 대상(곡·성경)이면서 실제로 지울 항목별 서식(FormatData)이 있을 때만.
    /// 서식이 없으면 지울 게 없어 비활성(불필요한 재송출 방지).
    /// </summary>
    public bool CanClearSelectedItemFormatting
        => CanEditSelectedItemColor && !string.IsNullOrEmpty(SelectedItem?.FormatData);

    /// <summary>
    /// 선택한 항목의 항목별 서식(FormatData 전체 — 색·정렬·크기·글꼴·배경·강조, Region1·Region2)을 모두 지워 전역 기본 서식으로 되돌린다(레거시 Clear All Formatting).
    /// 빈 <see cref="SongFormatData"/> 로 교체하면 인코딩 결과가 비어 FormatData 가 null 이 되고, 라이브면 즉시 같은 절로 재송출돼 전역 모양으로 바뀐다.
    /// 개별 서식 사용(UseIndividualFormatting) 플래그는 건드리지 않는다 — FormatData 가 비면 어차피 전역으로 송출되므로 무해하고, 각 facet 해제와 동작이 일관된다.
    /// </summary>
    public void ClearSelectedItemFormatting()
    {
        if (ApplySelectedSongFormatChange(_ => new SongFormatData(), wantsIndividual: false, out _))
        {
            StatusText = "이 항목 서식 모두 지움(전역 기본)";
        }
    }

    /// <summary>"이 항목 서식 복사"를 누를 수 있는지 — 복사할 항목별 서식(FormatData)이 있을 때만(지우기와 같은 조건).</summary>
    public bool CanCopySelectedItemFormatting => CanClearSelectedItemFormatting;

    /// <summary>"이 항목에 서식 붙여넣기"를 누를 수 있는지 — 담아 둔 서식이 있고, 대상이 서식 편집 가능(곡·성경, 본문 있음)일 때만.</summary>
    public bool CanPasteSelectedItemFormatting
        => CanEditSelectedItemColor && !string.IsNullOrEmpty(_copiedItemFormatData);

    /// <summary>
    /// 선택한 항목의 항목별 서식(FormatData)을 클립보드처럼 담아 둔다 — 다른 항목에 "붙여넣기"하면 그대로 적용된다.
    /// 본문·제목은 복사하지 않고 서식만 담는다(여러 항목을 같은 색·글꼴로 빠르게 통일하는 용도).
    /// </summary>
    public void CopySelectedItemFormatting()
    {
        if (!CanCopySelectedItemFormatting)
        {
            return;
        }

        _copiedItemFormatData = SelectedItem?.FormatData;
        StatusText = "이 항목 서식 복사함(다른 항목에 붙여넣기 가능)";
        NotifyCommandStates(); // 붙여넣기 활성화 통지.
    }

    /// <summary>
    /// 담아 둔 항목별 서식을 선택한 항목에 그대로 적용한다(본문은 그대로 두고 서식만 교체). 적용 시 개별 서식을 켜 송출에 보이게 하고, 라이브면 즉시 재송출한다.
    /// 담아 둔 서식이 없거나(복사 먼저), 대상이 편집 대상이 아니거나, 이미 같은 서식이면 아무 일도 안 한다.
    /// </summary>
    public void PasteSelectedItemFormatting()
    {
        if (!CanPasteSelectedItemFormatting)
        {
            return;
        }

        // 담아 둔 FormatData 를 그대로 적용 — 헬퍼가 Parse→Encode 왕복으로 안전하게 검증·재인코드한다(잘못된 코드 걸러짐).
        var pasted = SongFormatData.Parse(_copiedItemFormatData) ?? new SongFormatData();
        if (ApplySelectedSongFormatChange(_ => pasted, wantsIndividual: true, out _))
        {
            StatusText = "이 항목에 서식 붙여넣음";
        }
    }

    /// <summary>"복사한 서식을 전 항목에 적용"을 누를 수 있는지 — 담아 둔 서식이 있고, 큐에 서식 적용 가능한 항목(곡·성경)이 하나라도 있을 때.</summary>
    public bool CanApplyCopiedFormatToAll
        => !string.IsNullOrEmpty(_copiedItemFormatData) && Queue.Any(IsPerItemFormattable);

    /// <summary>
    /// 복사("이 항목 서식 복사")해 둔 항목별 서식을 예배 순서의 모든 곡·성경 항목(본문 있음)에 한 번에 적용한다 — 여러 곡을 같은 색·글꼴로 빠르게 통일.
    /// PPT·미디어·공지 항목은 항목별 서식 대상이 아니라 건너뛴다(레거시 "Apply to All Except InfoScreens" 와 같은 취지). 이미 같은 서식인 항목은 바꾸지 않는다(불필요한 재송출 방지).
    /// 라이브 항목이 영향을 받으면 세션 실제 절로 다시 송출해 즉시 반영한다.
    /// </summary>
    public void ApplyCopiedFormatToAll()
    {
        if (!CanApplyCopiedFormatToAll)
        {
            return;
        }

        // 담아 둔 서식을 Parse→Encode 로 한 번 정규화한다(잘못된 코드 걸러지고, 비교용 표준 문자열 확보).
        var encoded = SongFormatData.Parse(_copiedItemFormatData)?.Encode();
        if (string.IsNullOrEmpty(encoded))
        {
            StatusText = "복사한 서식이 비어 있습니다.";
            return;
        }

        // 선택 항목은 인스턴스가 교체되므로 위치를 잡아 두었다가 같은 자리의 새 인스턴스로 다시 선택한다(선택 유지).
        var selectedIndex = SelectedItem is { } sel ? IndexOfReference(sel) : -1;

        var changed = 0;
        for (var i = 0; i < Queue.Count; i++)
        {
            var item = Queue[i];
            if (!IsPerItemFormattable(item))
            {
                continue; // 곡·성경(본문 있음)만 대상 — 나머지는 건너뜀.
            }

            if (string.Equals(item.FormatData, encoded, StringComparison.Ordinal) && item.UseIndividualFormatting)
            {
                continue; // 이미 같은 서식이면 그대로 둔다.
            }

            Queue[i] = item with { FormatData = encoded, UseIndividualFormatting = true };
            changed++;
        }

        if (changed == 0)
        {
            StatusText = "이미 모든 대상 항목이 같은 서식을 쓰고 있습니다.";
            return;
        }

        if (selectedIndex >= 0 && selectedIndex < Queue.Count)
        {
            SelectedItem = Queue[selectedIndex];
        }

        RepublishLiveSongForBodyChange();
        NotifyCommandStates();
        StatusText = $"{changed}개 항목에 복사한 서식 적용";
    }

    /// <summary>
    /// "전 항목 서식 지우기"를 누를 수 있는지 — 예배 순서에 실제로 지울 항목별 서식(FormatData)이 있는 곡·성경 항목이 하나라도 있을 때만.
    /// 지울 서식이 하나도 없으면 비활성(불필요한 일괄 교체·재송출 방지).
    /// </summary>
    public bool CanClearAllItemsFormatting
        => Queue.Any(item => IsPerItemFormattable(item) && !string.IsNullOrEmpty(item.FormatData));

    /// <summary>
    /// 예배 순서의 모든 곡·성경 항목에서 항목별 서식(FormatData 전체 — 색·정렬·크기·글꼴·배경·강조, Region1·Region2)을 한 번에 지워 전역 기본으로 되돌린다(레거시 Clear All Formatting 전체판).
    /// PPT·미디어·공지는 항목별 서식 대상이 아니라 건너뛰고, 이미 서식이 없는 항목도 건너뛴다(불필요한 교체 방지 — "복사한 서식 전 항목 적용"과 같은 구조).
    /// 개별 서식 사용(UseIndividualFormatting) 플래그는 그대로 둔다 — FormatData 가 비면 어차피 전역으로 송출되므로 무해하고, 단일 항목 지우기와 동작이 일관된다.
    /// 라이브 항목이 영향을 받으면 세션 실제 절로 다시 송출해 즉시 전역 모양으로 바꾼다.
    /// </summary>
    public void ClearAllItemsFormatting()
    {
        if (!CanClearAllItemsFormatting)
        {
            return;
        }

        // 선택 항목은 인스턴스가 교체되므로 위치를 잡아 두었다가 같은 자리의 새 인스턴스로 다시 선택한다(선택 유지).
        var selectedIndex = SelectedItem is { } sel ? IndexOfReference(sel) : -1;

        var cleared = 0;
        for (var i = 0; i < Queue.Count; i++)
        {
            var item = Queue[i];
            // 곡·성경(본문 있음)이면서 실제로 지울 서식이 있는 항목만 — 나머지는 그대로 둔다.
            if (!IsPerItemFormattable(item) || string.IsNullOrEmpty(item.FormatData))
            {
                continue;
            }

            Queue[i] = item with { FormatData = null }; // 서식 제거 → 전역 기본으로 송출. 개별 서식 플래그는 보존.
            cleared++;
        }

        if (cleared == 0)
        {
            return; // CanClearAllItemsFormatting 가 true 였으면 여기 안 옴 — 방어적.
        }

        if (selectedIndex >= 0 && selectedIndex < Queue.Count)
        {
            SelectedItem = Queue[selectedIndex];
        }

        RepublishLiveSongForBodyChange();
        NotifyCommandStates();
        StatusText = $"{cleared}개 항목 서식 지움(전역 기본)";
    }

    /// <summary>현재 선택한 항목에 적용된 글자색(이 항목만, "#AARRGGBB"). 없으면 빈 문자열(전역 기본색 추종).</summary>
    public string SelectedItemTextColorHex
        => SongFormatData.ArgbToHex(SongFormatData.Parse(SelectedItem?.FormatData)?.TextColorArgb1) ?? string.Empty;

    /// <summary>
    /// 선택한 항목(<b>곡·성경</b>, 증분128)의 항목별 서식(FormatData) 중 한 가지만 바꾸는 공통 경로 — 항목별 색·정렬·크기·글꼴·배경·강조가 공유한다(이름은 호환을 위해 Song 유지).
    /// <paramref name="mutate"/> 가 현재 <see cref="SongFormatData"/> 를 받아 한 필드 바꾼 값을 돌려주면, 나머지 서식은
    /// 그대로 보존한 채(Parse→with→Encode 왕복) 큐의 그 인스턴스를 교체(참조 일치)·재선택하고 라이브면 즉시 재송출한다.
    /// <paramref name="wantsIndividual"/> 가 true 면(=서식을 켜는 변경) 그 서식이 보이도록 개별 서식 사용을 켠다(끄면 FormatData 무시됨).
    /// 바뀐 게 없거나(레코드 동등) 편집 대상이 아니면 false 를 돌려준다(상태 메시지는 호출 측이 정한다).
    /// </summary>
    private bool ApplySelectedSongFormatChange(
        Func<SongFormatData, SongFormatData> mutate,
        bool wantsIndividual,
        out bool turnedOnIndividual)
    {
        turnedOnIndividual = false;
        if (!CanEditSelectedItemColor || SelectedItem is not { } item)
        {
            return false; // 곡·성경이 아니거나 선택 없음 — 항목별 서식 편집 대상이 아님.
        }

        var current = SongFormatData.Parse(item.FormatData) ?? new SongFormatData();
        var next = mutate(current);
        if (next == current)
        {
            return false; // 레코드 동등 — 바뀐 게 없으면 무시(불필요한 교체·재송출 방지).
        }

        var index = IndexOfReference(item);
        if (index < 0)
        {
            return false;
        }

        // 결과가 비면 FormatData 는 null(서식 없음). 서식을 켜는 변경이면 개별 서식도 켜서 송출에 보이게 한다.
        var encoded = next.Encode();
        turnedOnIndividual = wantsIndividual && !item.UseIndividualFormatting; // 일부러 꺼둔 걸 자동으로 켰는지(상태줄 고지용).
        var updated = item with
        {
            FormatData = string.IsNullOrEmpty(encoded) ? null : encoded,
            UseIndividualFormatting = wantsIndividual ? true : item.UseIndividualFormatting,
        };
        Queue[index] = updated;
        SelectedItem = updated; // 선택 변경 → 미리보기·메뉴 통지.

        // 라이브 송출 중이면 같은 절로 다시 송출해 새 서식을 즉시 반영(세션 실제 라이브 절 사용 — 0절로 안 튀게).
        RepublishLiveSongForBodyChange();

        NotifyCommandStates();
        return true;
    }

    /// <summary>
    /// 선택한 곡 항목의 글자색(이 항목만)을 바꾼다(레거시 항목별 색, FormatData 코드 29). hex 가 비거나 형식 오류면 색을 해제해 전역 기본색을 따른다.
    /// 색을 주면 그 곡 동안 보이도록 개별 서식도 켜고(꺼져 있었으면 상태줄에 고지), 나머지 곡별 서식은 보존한다.
    /// </summary>
    public void SetSelectedItemTextColor(string? hex)
    {
        var argb = SongFormatData.HexToArgb(hex); // 비거나 형식 오류면 null = 색 해제(전역 기본색).
        if (ApplySelectedSongFormatChange(format => format with { TextColorArgb1 = argb }, wantsIndividual: argb is not null, out var turnedOn))
        {
            StatusText = argb is null
                ? "항목 글자색: 전역 기본"
                : $"항목 글자색: {SongFormatData.ArgbToHex(argb)}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 가로 정렬(이 항목만) — 레거시 1=왼쪽/2=가운데/3=오른쪽, 없으면 0(전역 정렬 추종).</summary>
    public int SelectedItemAlignment
        => SongFormatData.Parse(SelectedItem?.FormatData)?.Alignment1 ?? 0;

    /// <summary>
    /// 선택한 곡 항목의 가로 정렬(이 항목만)을 바꾼다(레거시 항목별 정렬, FormatData 코드 31). 파라미터 "1"/"2"/"3"=왼쪽/가운데/오른쪽,
    /// 그 밖(빈·0·오류)이면 정렬을 해제해 전역 정렬을 따른다. 정렬을 주면 개별 서식을 켜고, 나머지 곡별 서식(색·폰트 등)은 보존한다.
    /// </summary>
    public void SetSelectedItemAlignment(string? alignmentParam)
    {
        // "1"/"2"/"3" 만 유효(왼쪽/가운데/오른쪽), 그 밖은 null = 정렬 해제.
        int? align = int.TryParse(alignmentParam, out var n) && n is >= 1 and <= 3 ? n : null;
        if (ApplySelectedSongFormatChange(format => format with { Alignment1 = align }, wantsIndividual: align is not null, out var turnedOn))
        {
            var label = align switch { 1 => "왼쪽", 2 => "가운데", 3 => "오른쪽", _ => "전역 기본" };
            StatusText = $"항목 정렬: {label}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 글자 크기(이 항목만, 레거시 pt). 없으면 0(전역 크기 추종).</summary>
    public int SelectedItemFontSize
        => SongFormatData.Parse(SelectedItem?.FormatData)?.FontSize1 ?? 0;

    /// <summary>
    /// 선택한 곡 항목의 글자 크기(이 항목만)를 바꾼다(레거시 항목별 크기, FormatData 코드 47, pt 단위 6~100).
    /// 파라미터가 6~100 범위 숫자면 그 크기, 그 밖(빈·0·범위 밖·오류)이면 크기를 해제해 전역 크기를 따른다.
    /// 크기를 주면 개별 서식을 켜고, 나머지 곡별 서식(색·정렬·폰트 등)은 보존한다. 송출 시 pt→px 로 변환돼 적용된다.
    /// </summary>
    public void SetSelectedItemFontSize(string? sizeParam)
    {
        // 레거시 유효 범위 6~100pt 만 허용, 그 밖은 null = 크기 해제.
        int? size = int.TryParse(sizeParam, out var n) && n is >= 6 and <= 100 ? n : null;
        if (ApplySelectedSongFormatChange(format => format with { FontSize1 = size }, wantsIndividual: size is not null, out var turnedOn))
        {
            StatusText = size is null
                ? "항목 글자 크기: 전역 기본"
                : $"항목 글자 크기: {size}pt{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 글꼴명(이 항목만). 없으면 빈 문자열(전역 글꼴 추종).</summary>
    public string SelectedItemFontName
        => SongFormatData.Parse(SelectedItem?.FormatData)?.FontName1 ?? string.Empty;

    /// <summary>
    /// 선택한 곡 항목의 글꼴명(이 항목만)을 바꾼다(레거시 항목별 글꼴, FormatData 코드 43). 이름이 비거나 공백뿐이면 글꼴을 해제해 전역 글꼴을 따른다.
    /// 글꼴을 주면 개별 서식을 켜고, 나머지 곡별 서식(색·정렬·크기 등)은 보존한다. 송출 시 그 곡 동안 이 글꼴로 표시된다.
    /// </summary>
    public void SetSelectedItemFontName(string? name)
    {
        // 빈/공백 이름 = 글꼴 해제(전역 글꼴 추종). 앞뒤 공백·구분자('>'·'=') 제거는 공통 규약(SanitizeFontName).
        var fontName = SongFormatData.SanitizeFontName(name);
        if (ApplySelectedSongFormatChange(format => format with { FontName1 = fontName }, wantsIndividual: fontName.Length > 0, out var turnedOn))
        {
            StatusText = fontName.Length == 0
                ? "항목 글꼴: 전역 기본"
                : $"항목 글꼴: {fontName}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목에 적용된 배경색(이 항목만, "#AARRGGBB"). 없으면 빈 문자열(전역 배경 추종).</summary>
    public string SelectedItemBackgroundColorHex
        => SongFormatData.ArgbToHex(SongFormatData.Parse(SelectedItem?.FormatData)?.BackgroundColorArgb1) ?? string.Empty;

    /// <summary>
    /// 선택한 곡 항목의 배경색(이 항목만)을 바꾼다(레거시 항목별 배경, FormatData 코드 26). hex 가 비거나 형식 오류면 해제해 전역 배경을 따른다.
    /// 배경을 주면 개별 서식을 켜고, 나머지 곡별 서식(글자색·정렬·크기·글꼴 등)은 보존한다. 송출 시 그 곡 동안 이 배경색으로 표시된다.
    /// <para>주의: 배경 <b>이미지</b>(전역 또는 곡별 FormatData 61)가 설정돼 있으면 그 이미지가 배경색 위를 덮어 색이 안 보일 수 있다(이미지가 우선).</para>
    /// </summary>
    public void SetSelectedItemBackgroundColor(string? hex)
    {
        var argb = SongFormatData.HexToArgb(hex); // 비거나 형식 오류면 null = 배경 해제(전역 배경).
        if (ApplySelectedSongFormatChange(format => format with { BackgroundColorArgb1 = argb }, wantsIndividual: argb is not null, out var turnedOn))
        {
            StatusText = argb is null
                ? "항목 배경색: 전역 기본"
                : $"항목 배경색: {SongFormatData.ArgbToHex(argb)}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 배경 이미지 경로(이 항목만). 없으면 빈 문자열(전역 배경 추종).</summary>
    public string SelectedItemBackgroundImagePath
        => SongFormatData.Parse(SelectedItem?.FormatData)?.BackgroundImagePath ?? string.Empty;

    /// <summary>
    /// 선택한 곡 항목의 배경 이미지(이 항목만)를 바꾼다(레거시 항목별 배경 이미지, FormatData 코드 61). 경로가 비거나 공백뿐이면 해제해 전역 배경을 따른다.
    /// 이미지를 주면 개별 서식을 켜고, 나머지 곡별 서식(색·정렬·크기·글꼴 등)은 보존한다. 송출 시 그 곡 동안 이 이미지가 <b>배경색 위에</b> 표시된다
    /// (배경색이 설정돼 있어도 이미지가 우선 — <see cref="SetSelectedItemBackgroundColor"/> 와 짝).
    /// (이미지 파일을 실제로 읽어 그리는 일은 출력 창 VM 이 맡는다 — 여기선 경로만 기록한다.)
    /// </summary>
    public void SetSelectedItemBackgroundImage(string? path)
    {
        // 빈/공백 경로 = 배경 이미지 해제(전역 배경 추종). 앞뒤 공백은 다듬는다.
        // '>'는 FormatData 항목 구분자라 윈도우 경로엔 못 쓰지만, 혹시 섞이면 포맷이 깨지므로 방어적으로 제거('='는 경로에 쓰일 수 있어 보존).
        var imagePath = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Replace(">", string.Empty);
        if (ApplySelectedSongFormatChange(format => format with { BackgroundImagePath = imagePath }, wantsIndividual: imagePath.Length > 0, out var turnedOn))
        {
            StatusText = imagePath.Length == 0
                ? "항목 배경 이미지: 전역 기본"
                : $"항목 배경 이미지: {System.IO.Path.GetFileName(imagePath)}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 굵게(이 항목만) 적용 여부 — 우클릭 강조 메뉴 체크 표시에 바인딩.</summary>
    public bool SelectedItemBold => SongFormatData.Parse(SelectedItem?.FormatData)?.Bold1 ?? false;

    /// <summary>현재 선택한 항목의 기울임(이 항목만) 적용 여부.</summary>
    public bool SelectedItemItalic => SongFormatData.Parse(SelectedItem?.FormatData)?.Italic1 ?? false;

    /// <summary>현재 선택한 항목의 밑줄(이 항목만) 적용 여부.</summary>
    public bool SelectedItemUnderline => SongFormatData.Parse(SelectedItem?.FormatData)?.Underline1 ?? false;

    // 선택 곡 항목의 강조(굵게/기울임/밑줄, FormatData 코드41 비트 중 하나)를 켜고 끈다. 켜면 보이도록 개별 서식도 켠다.
    // 나머지 강조 비트·다른 서식(색·정렬·크기·글꼴)은 그대로 보존(Encode 가 6비트를 다시 합산). 켜는 쪽일 때만 wantsIndividual=true.
    // 참고: 출력 "강조 후렴만"(LyricsMonitorEmphasisChorusOnly) 전역 설정이 켜져 있으면, 후렴이 아닌 절에선 이 항목 강조도 가려진다
    // (OutputRenderer 의 후렴 게이트는 항목별·전역 강조 모두에 동일 적용 — 의도된 전역 우선). 즉 강조가 안 보이면 후렴만 설정을 확인.
    private void ToggleSelectedItemEmphasis(
        Func<SongFormatData, bool> current,
        Func<SongFormatData, bool, SongFormatData> withValue,
        string label)
    {
        var parsed = SongFormatData.Parse(SelectedItem?.FormatData) ?? new SongFormatData();
        var newValue = !current(parsed);
        if (ApplySelectedSongFormatChange(format => withValue(format, newValue), wantsIndividual: newValue, out var turnedOn))
        {
            StatusText = $"항목 {label}: {(newValue ? "켜짐" : "꺼짐")}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>선택한 곡 항목의 굵게(이 항목만)를 켜고 끈다(레거시 항목별 굵게, FormatData 코드41 bit0).</summary>
    public void ToggleSelectedItemBold()
        => ToggleSelectedItemEmphasis(f => f.Bold1, (f, v) => f with { Bold1 = v }, "굵게");

    /// <summary>선택한 곡 항목의 기울임(이 항목만)을 켜고 끈다(레거시 항목별 기울임, FormatData 코드41 bit1).</summary>
    public void ToggleSelectedItemItalic()
        => ToggleSelectedItemEmphasis(f => f.Italic1, (f, v) => f with { Italic1 = v }, "기울임");

    /// <summary>선택한 곡 항목의 밑줄(이 항목만)을 켜고 끈다(레거시 항목별 밑줄, FormatData 코드41 bit2).</summary>
    public void ToggleSelectedItemUnderline()
        => ToggleSelectedItemEmphasis(f => f.Underline1, (f, v) => f with { Underline1 = v }, "밑줄");

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 글자색(이 항목만, "#AARRGGBB"). 없으면 빈 문자열(본문 색 추종).</summary>
    public string SelectedItemTextColor2Hex
        => SongFormatData.ArgbToHex(SongFormatData.Parse(SelectedItem?.FormatData)?.TextColorArgb2) ?? string.Empty;

    /// <summary>
    /// 선택한 곡 항목의 보조 영역(이중 언어 Region2) 글자색(이 항목만)을 바꾼다(레거시 항목별 Region2 색, FormatData 코드 30).
    /// hex 가 비거나 형식 오류면 해제해 본문(Region1) 색을 따른다. 이중 언어 곡([region 2] 마커가 있는 곡)에서만 화면에 보인다 — 단일 언어 곡엔 영향 없음.
    /// 나머지 곡별 서식(본문 색·정렬·크기·글꼴 등)은 보존한다.
    /// </summary>
    public void SetSelectedItemTextColor2(string? hex)
    {
        var argb = SongFormatData.HexToArgb(hex); // 비거나 형식 오류면 null = 해제(본문 색 추종).
        if (ApplySelectedSongFormatChange(format => format with { TextColorArgb2 = argb }, wantsIndividual: argb is not null, out var turnedOn))
        {
            StatusText = argb is null
                ? "보조 영역 글자색: 본문과 동일"
                : $"보조 영역 글자색: {SongFormatData.ArgbToHex(argb)}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 가로 정렬(이 항목만) — 레거시 1=왼쪽/2=가운데/3=오른쪽, 없으면 0(본문 정렬 추종).</summary>
    public int SelectedItemAlignment2
        => SongFormatData.Parse(SelectedItem?.FormatData)?.Alignment2 ?? 0;

    /// <summary>
    /// 선택한 곡 항목의 보조 영역(이중 언어 Region2) 가로 정렬(이 항목만)을 바꾼다(레거시 항목별 Region2 정렬, FormatData 코드 32).
    /// 파라미터 "1"/"2"/"3"=왼쪽/가운데/오른쪽, 그 밖(빈·0·오류)이면 해제해 본문(Region1) 정렬을 따른다. 이중 언어 곡에서만 화면에 보인다.
    /// </summary>
    public void SetSelectedItemAlignment2(string? alignmentParam)
    {
        // "1"/"2"/"3" 만 유효(왼쪽/가운데/오른쪽), 그 밖은 null = 정렬 해제(본문 정렬 추종).
        int? align = int.TryParse(alignmentParam, out var n) && n is >= 1 and <= 3 ? n : null;
        if (ApplySelectedSongFormatChange(format => format with { Alignment2 = align }, wantsIndividual: align is not null, out var turnedOn))
        {
            var label = align switch { 1 => "왼쪽", 2 => "가운데", 3 => "오른쪽", _ => "본문과 동일" };
            StatusText = $"보조 영역 정렬: {label}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 글자 크기(이 항목만, 레거시 pt). 없으면 0(본문 크기 추종).</summary>
    public int SelectedItemFontSize2
        => SongFormatData.Parse(SelectedItem?.FormatData)?.FontSize2 ?? 0;

    /// <summary>
    /// 선택한 곡 항목의 보조 영역(이중 언어 Region2) 글자 크기(이 항목만)를 바꾼다(레거시 항목별 Region2 크기, FormatData 코드 48, pt 6~100).
    /// 6~100 범위 숫자면 그 크기, 그 밖(빈·0·범위 밖·오류)이면 해제해 본문(Region1) 크기를 따른다. 이중 언어 곡에서만 화면에 보인다.
    /// </summary>
    public void SetSelectedItemFontSize2(string? sizeParam)
    {
        int? size = int.TryParse(sizeParam, out var n) && n is >= 6 and <= 100 ? n : null;
        if (ApplySelectedSongFormatChange(format => format with { FontSize2 = size }, wantsIndividual: size is not null, out var turnedOn))
        {
            StatusText = size is null
                ? "보조 영역 글자 크기: 본문과 동일"
                : $"보조 영역 글자 크기: {size}pt{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 글꼴명(이 항목만). 없으면 빈 문자열(본문 글꼴 추종).</summary>
    public string SelectedItemFontName2
        => SongFormatData.Parse(SelectedItem?.FormatData)?.FontName2 ?? string.Empty;

    /// <summary>
    /// 선택한 곡 항목의 보조 영역(이중 언어 Region2) 글꼴명(이 항목만)을 바꾼다(레거시 항목별 Region2 글꼴, FormatData 코드 44).
    /// 이름이 비거나 공백뿐이면 해제해 본문(Region1) 글꼴을 따른다. 이중 언어 곡에서만 화면에 보인다. '>'·'='는 포맷 구분자라 방어적으로 제거.
    /// </summary>
    public void SetSelectedItemFontName2(string? name)
    {
        var fontName = string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : name.Trim().Replace(">", string.Empty).Replace("=", string.Empty);
        if (ApplySelectedSongFormatChange(format => format with { FontName2 = fontName }, wantsIndividual: fontName.Length > 0, out var turnedOn))
        {
            StatusText = fontName.Length == 0
                ? "보조 영역 글꼴: 본문과 동일"
                : $"보조 영역 글꼴: {fontName}{(turnedOn ? " (개별 서식 켜짐)" : "")}";
        }
    }

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 굵게(이 항목만) 적용 여부 — 우클릭 메뉴 체크 표시에 바인딩.</summary>
    public bool SelectedItemBold2 => SongFormatData.Parse(SelectedItem?.FormatData)?.Bold2 ?? false;

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 기울임(이 항목만) 적용 여부.</summary>
    public bool SelectedItemItalic2 => SongFormatData.Parse(SelectedItem?.FormatData)?.Italic2 ?? false;

    /// <summary>현재 선택한 항목의 보조 영역(Region2) 밑줄(이 항목만) 적용 여부.</summary>
    public bool SelectedItemUnderline2 => SongFormatData.Parse(SelectedItem?.FormatData)?.Underline2 ?? false;

    /// <summary>선택한 곡 항목의 보조 영역(Region2) 굵게(이 항목만)를 켜고 끈다(레거시 Region2 굵게, FormatData 코드41 bit3).</summary>
    public void ToggleSelectedItemBold2()
        => ToggleSelectedItemEmphasis(f => f.Bold2, (f, v) => f with { Bold2 = v }, "보조 영역 굵게");

    /// <summary>선택한 곡 항목의 보조 영역(Region2) 기울임(이 항목만)을 켜고 끈다(레거시 Region2 기울임, FormatData 코드41 bit4).</summary>
    public void ToggleSelectedItemItalic2()
        => ToggleSelectedItemEmphasis(f => f.Italic2, (f, v) => f with { Italic2 = v }, "보조 영역 기울임");

    /// <summary>선택한 곡 항목의 보조 영역(Region2) 밑줄(이 항목만)을 켜고 끈다(레거시 Region2 밑줄, FormatData 코드41 bit5).</summary>
    public void ToggleSelectedItemUnderline2()
        => ToggleSelectedItemEmphasis(f => f.Underline2, (f, v) => f with { Underline2 = v }, "보조 영역 밑줄");

    // 모든 항목을 전역 기본 서식으로 적용(레거시 FrmMain "Apply to All Except InfoScreens" 대응).
    // 각 항목의 UseIndividualFormatting 을 false 로 바꿔 곡별 FormatData 대신 운영 기본 서식으로 송출하게 한다.
    // (이 플래그는 곡·성경 텍스트 항목 송출에만 영향 — PPT/미디어/공지엔 무영향. 레거시 "Except InfoScreens" 와 같은 취지.)
    private void ApplyGlobalFormatToAll()
    {
        if (Queue.Count == 0)
        {
            StatusText = "예배 순서가 비어 있습니다.";
            return;
        }

        // 선택 항목은 인스턴스가 교체되므로 위치를 잡아 두었다가 같은 자리의 새 인스턴스로 다시 선택한다(선택 유지).
        var selectedIndex = SelectedItem is { } sel ? IndexOfReference(sel) : -1;

        var changed = 0;
        for (var i = 0; i < Queue.Count; i++)
        {
            var item = Queue[i];
            if (item.UseIndividualFormatting)
            {
                Queue[i] = item with { UseIndividualFormatting = false };
                changed++;
            }
        }

        if (changed == 0)
        {
            StatusText = "이미 모든 항목이 전역 기본 서식을 사용 중입니다.";
            return;
        }

        // 교체로 끊긴 선택을 같은 자리의 새 인스턴스로 복원(선택이 UI 에서 사라지지 않게).
        if (selectedIndex >= 0 && selectedIndex < Queue.Count)
        {
            SelectedItem = Queue[selectedIndex];
        }

        // 라이브 항목이면 세션 실제 절로 다시 송출해 새 서식을 즉시 반영(Id 로 큐의 교체 인스턴스를 찾는다).
        RepublishLiveSongForBodyChange();
        NotifyCommandStates();
        StatusText = $"{changed}개 항목에 전역 기본 서식 적용";
    }

    private LiveQueueItem ResolveLiveProjection(
        LiveQueueItem item,
        Rendering.PowerPointPreviewViewModel? powerPoint = null)
    {
        var ppt = powerPoint ?? PowerPoint;
        var positionLabel = ComputePositionLabel(item, ppt);
        var nextTitle = ComputeNextTitle(item);
        // "코드 표시"(Show Notations) 설정을 투영에 얹는다 — 라이브 본문 계산(ComputeBodyText)이 이 값으로
        // 가사 위 코드 줄을 끼울지 판단한다. 모든 곡 송출 경로가 ResolveLiveProjection 을 거치므로 한 곳에서 일관 적용.
        var showNotations = _settings.Get(EasiSettingKeys.LyricsMonitorShowNotations);
        // 항목별 "개별 서식 사용"이 꺼져 있으면 곡별 FormatData(색·정렬·폰트·배경)를 비워 전역 기본 설정으로 송출한다.
        // 기본(true)이면 곡별 서식 그대로(무회귀). 레거시 FrmMain Ind_checkBox "Use Individual Settings" 대응.
        var formatData = item.UseIndividualFormatting ? item.FormatData : null;

        if (IsPowerPointItem(item)
            && ppt.State == Rendering.PowerPointPreviewState.Ready
            && ppt.PreviewImage is not null
            && !string.IsNullOrEmpty(item.ContentPath)
            && string.Equals(ppt.LoadedContentPath, item.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            return item with
            {
                PreviewSource = ppt.PreviewImage,
                PreviewFillMode = Rendering.ImageFillMode.Fit,
                SlideNumber = ppt.SlideNumber,
                PositionLabel = positionLabel,
                NextTitle = nextTitle,
                ShowNotations = showNotations,
                TransposeSemitones = LiveTransposeSemitones,
                FormatData = formatData,
            };
        }

        return item with
        {
            PositionLabel = positionLabel,
            NextTitle = nextTitle,
            ShowNotations = showNotations,
            TransposeSemitones = LiveTransposeSemitones,
            FormatData = formatData,
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
    private string ComputePositionLabel(
        LiveQueueItem item,
        Rendering.PowerPointPreviewViewModel? powerPoint = null)
    {
        if (IsPowerPointItem(item))
        {
            var ppt = powerPoint ?? PowerPoint;
            var count = ppt.SlideCount;
            var current = ppt.SlideNumber;
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
            // 라이브 절 이동 후 절 순서를 편집해 페이지 수가 줄면 LyricsPageIndex 가 범위를 넘을 수 있어 클램프(예: "4/3" 방지).
            // 본문은 GetVersePage 가 이미 클램프하므로, 위치 라벨도 같은 마지막 절로 맞춰 일관되게 보여 준다.
            var position = System.Math.Clamp(item.LyricsPageIndex, 0, count - 1) + 1;
            return count > 1 ? $"{position}/{count}" : string.Empty;
        }

        return string.Empty;
    }

    private void AdvanceSelectionAfterPublish(LiveQueueItem publishedItem)
    {
        if (!_settings.Get(EasiSettingKeys.AdvanceNextItem))
        {
            return;
        }

        AdvanceSelectionToNext(publishedItem);
    }

    // 선택을 다음 항목으로 옮긴다(설정 무관). 자동 advance(설정 on)와 명시 "전송 후 다음" 모두 이 한 곳을 쓴다.
    // PPT 덱은 제자리 유지 — 다중 슬라이드 덱은 다음 항목으로 넘어가지 말고 그 자리에서 슬라이드를 이동해야 하고,
    // live Output PPT 는 Output 전용 슬라이드 버튼으로 따로 넘긴다.
    // 인덱스는 참조 일치(IndexOfReference)로 찾는다 — 같은 값 항목이 큐에 여러 번 있어도(입례/봉헌 반복) 정확히 그 인스턴스.
    private void AdvanceSelectionToNext(LiveQueueItem publishedItem)
    {
        if (IsPowerPointItem(publishedItem))
        {
            return;
        }

        var index = IndexOfReference(publishedItem);
        if (index >= 0 && index < Queue.Count - 1)
        {
            SelectedItem = Queue[index + 1];
            LiveBar.CurrentItemTitle = _session.Current.CurrentItemTitle;
        }
    }

    /// <summary>
    /// 모든 설정을 기본값으로 초기화한다(레거시 Tools "Clear EasiSlides Registry Settings and Exit" 대응).
    /// 설정이 꼬여 앱이 이상하게 동작할 때 쓰는 복구 탈출구 — 설정 창을 거치지 않고 한 번에 기본값으로 되돌린다.
    /// 설정 파일을 기본값으로 덮어써 디스크에 즉시 저장한다(SettingsService.RestoreDefaults). 곡·성경 데이터는 건드리지 않는다.
    /// 성공 여부를 돌려주며, 성공하면 호출부(View)가 확인 후 앱을 다시 시작한다(레거시는 종료).
    /// </summary>
    public bool ResetAllSettingsToDefaults() => _settings.RestoreDefaults().Succeeded;

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

    // 배경 그라데이션 방향 적용 — 설정→출력 VM 이 SettingsChanged 로 LinearGradientBrush 를 다시 만들어 즉시 반영(FrmMain Def_BackColour 패턴).
    private void ApplyGradientDirection(LyricsGradientDirection direction)
    {
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundGradientDirection, direction);
        ActiveGradientDirection = direction;
        StatusText = $"그라데이션 방향: {direction switch
        {
            LyricsGradientDirection.Horizontal => "가로(좌→우)",
            LyricsGradientDirection.DiagonalDown => "대각선 ↘",
            LyricsGradientDirection.DiagonalUp => "대각선 ↗",
            _ => "세로(위→아래)",
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

    // 출력 모양을 모두 기본값으로 되돌린다(레거시 FrmMain Default Layout). 버튼이 "전체"를 약속하므로 진짜 전체를 리셋한다.
    // ① 모양 템플릿이 다루는 30개 키(색·정렬·폰트·효과·여백·패널 색/크기/제목·그라데이션·헤딩·영역 간격)는 Defaults 템플릿으로,
    // ② 템플릿에 없는 나머지 출력 모양 키(전환·배경 모드/이미지·영역 표시·강조 후렴만·인터레이스·패널 투명·Display Panel 토글)는
    //    여기서 각 SettingKey.DefaultValue 로 직접 리셋한다. 출력 VM 이 SettingsChanged 로 라이브 즉시 반영.
    private void ResetOutputAppearance()
    {
        // ① 템플릿 커버 키.
        LyricsAppearanceTemplate.Defaults.ApplyTo(_settings);

        // ② 템플릿 밖 출력 모양 키 — "기본값으로 복원"이 전체를 뜻하도록 함께 리셋(인스펙터 같은 패널의 전환·배경·영역표시 등).
        _settings.Set(EasiSettingKeys.LyricsMonitorTransitionKind, EasiSettingKeys.LyricsMonitorTransitionKind.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorTransitionDurationMs, EasiSettingKeys.LyricsMonitorTransitionDurationMs.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundMode, EasiSettingKeys.LyricsMonitorBackgroundMode.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorBackgroundImagePath, EasiSettingKeys.LyricsMonitorBackgroundImagePath.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorRegionDisplay, EasiSettingKeys.LyricsMonitorRegionDisplay.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorEmphasisChorusOnly, EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorInterlace, EasiSettingKeys.LyricsMonitorInterlace.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorPanelTransparent, EasiSettingKeys.LyricsMonitorPanelTransparent.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorShowItemNumber, EasiSettingKeys.LyricsMonitorShowItemNumber.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorShowCopyright, EasiSettingKeys.LyricsMonitorShowCopyright.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorShowNextItem, EasiSettingKeys.LyricsMonitorShowNextItem.DefaultValue);
        // 전역 출력 글꼴명도 함께 기본(테마 상속)으로 — "전체" 복원이 글꼴까지 포함하도록.
        _settings.Set(EasiSettingKeys.LyricsMonitorFontFamily, EasiSettingKeys.LyricsMonitorFontFamily.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorFontFamily2, EasiSettingKeys.LyricsMonitorFontFamily2.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorTextColor2Argb, EasiSettingKeys.LyricsMonitorTextColor2Argb.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Alignment, EasiSettingKeys.LyricsMonitorRegion2Alignment.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Bold, EasiSettingKeys.LyricsMonitorRegion2Bold.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Italic, EasiSettingKeys.LyricsMonitorRegion2Italic.DefaultValue);
        _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Underline, EasiSettingKeys.LyricsMonitorRegion2Underline.DefaultValue);

        RefreshActiveAppearance(); // 인스펙터 표시(색·정렬·크기·효과·전환·배경·영역표시·글꼴 등) 동기화
        StatusText = "출력 모양 기본값 복원";
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
    // Display Panel 밴드 색 적용(FrmMain Def_PanelColour) — 사용자는 RGB(색조)만 고르고,
    // 밴드 뒤 가사가 비치도록 반투명 알파(0x66)는 항상 유지한다. 설정→출력 VM 라이브 반영.
    private void ApplyPanelColorHex(string? hex)
    {
        if (!TryParseHexColor(hex, out var rgbArgb))
        {
            StatusText = "색 형식이 올바르지 않습니다(예: #1A2B3C).";
            return;
        }

        // TryParseHexColor 는 불투명(0xFF) 알파를 주므로, RGB 만 떼어 반투명(0x66) 알파와 다시 합친다.
        var panelArgb = unchecked((int)(0x66000000u | ((uint)rgbArgb & 0x00FFFFFFu)));
        _settings.Set(EasiSettingKeys.LyricsMonitorPanelColorArgb, panelArgb);
        StatusText = $"패널 색: {FormatColorHex(rgbArgb)} (반투명)";
        RefreshActiveAppearance();
    }

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
            : key.Id == EasiSettingKeys.LyricsMonitorShowNotations.Id ? "코드 표시"
            : key.Id == EasiSettingKeys.LyricsMonitorUnderline.Id ? "밑줄"
            : key.Id == EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.Id ? "강조 후렴만"
            : key.Id == EasiSettingKeys.LyricsMonitorInterlace.Id ? "줄 교차"
            : key.Id == EasiSettingKeys.LyricsMonitorShowPositionIndicator.Id ? "위치 표시"
            : key.Id == EasiSettingKeys.LyricsMonitorShowVerseHeading.Id ? "절 헤딩"
            : key.Id == EasiSettingKeys.LyricsMonitorShowTitleHeading.Id ? "제목 표시"
            : key.Id == EasiSettingKeys.LyricsMonitorOutline.Id ? "외곽선"
            : key.Id == EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.Id ? "제목 첫 화면만"
            : key.Id == EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody.Id ? "제목 본문 정렬 따름"
            : key.Id == EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2.Id ? "제목 보조영역 정렬 따름"
            : key.Id == EasiSettingKeys.LyricsMonitorShowItemNumber.Id ? "곡 번호"
            : key.Id == EasiSettingKeys.LyricsMonitorShowTitleOnPanel.Id ? "패널 제목"
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
        StatusText = $"항목 전환 효과: {TransitionKindLabel(kind)}";
    }

    // 전환 효과 종류 → 한글 라벨(항목 전환 메뉴·슬라이드 전환 콤보 공용).
    private static string TransitionKindLabel(LyricsTransitionKind kind) => kind switch
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

    /// <summary>슬라이드/절 전환 콤보의 (라벨 → 종류) 목록(모든 전환 종류). 항목 전환 메뉴와 동일 라벨을 공유한다.</summary>
    public IReadOnlyList<KeyValuePair<string, LyricsTransitionKind>> TransitionKindOptions { get; } =
        System.Enum.GetValues<LyricsTransitionKind>()
            .Select(k => new KeyValuePair<string, LyricsTransitionKind>(TransitionKindLabel(k), k))
            .ToArray();

    /// <summary>슬라이드/절 전환 종류 선택(콤보 양방향 바인딩). 같은 항목 안 절·슬라이드 이동 때 쓰는 전환. 바뀌면 설정 저장.</summary>
    public LyricsTransitionKind SlideTransitionKindInput
    {
        get => ActiveSlideTransitionKind;
        set
        {
            if (value == ActiveSlideTransitionKind)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.LyricsMonitorSlideTransitionKind, value);
            ActiveSlideTransitionKind = value;
            StatusText = $"슬라이드 전환 효과: {TransitionKindLabel(value)}";
        }
    }

    // 슬라이드 전환 종류가 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveSlideTransitionKindChanged(LyricsTransitionKind value) => OnPropertyChanged(nameof(SlideTransitionKindInput));

    // 공지 화면 송출(FrmInfoScreen — 자유 텍스트 안내). InfoScreen 창에서 입력한 텍스트를
    // 즉시 회중 출력에 본문으로 송출한다. 출력 창이 열려 있을 때만 동작(닫혀 있으면 false 반환).
    // 공지는 큐 항목이 아니라 일시 라이브 항목이다. _liveItemId 를 센티넬(NoticeLiveId)로 둬
    // 슬라이드/절 이동 가드(== 선택 항목 ID)가 자연히 false 가 되도록 한다 — null 로 두면 슬라이드 이동
    // 가드의 "라이브 미시작" 와일드카드(_liveItemId is null)에 걸려 의미 없는 이동 버튼이 켜진다.
    // 공지 FormatData 조립(순수 함수) — 글자 크기(47=pt)·가로 정렬(31=1왼/2가운데/3오른쪽)·글자색(29=ARGB int)을
    // 레거시 FormatData 로 싣는다. 모두 미지정이면 null(출력 기본 사용). 곡별 FormatData 오버라이드 파이프라인을 그대로
    // 재사용하므로 공지에도 같은 크기·정렬·색 렌더가 적용된다.
    internal static string? BuildNoticeFormatData(NoticeOptions options)
    {
        var sb = new System.Text.StringBuilder();
        if (options.FontSizePt > 0)
        {
            sb.Append($"47={options.FontSizePt}>");
        }

        if (options.Alignment is 1 or 2 or 3)
        {
            sb.Append($"31={options.Alignment}>");
        }

        if (options.ColorArgb != 0)
        {
            sb.Append($"29={options.ColorArgb}>");
        }

        if (options.BackgroundColorArgb != 0)
        {
            // 코드 26 = 영역1 배경색(곡 배경과 동일 파이프라인) → 출력이 라이브에서 OverrideBackgroundColorArgb 로 적용.
            sb.Append($"26={options.BackgroundColorArgb}>");
        }

        // 코드 41 = 강조 비트밭(곡 강조와 동일 파이프라인) — 비트0=굵게, 비트1=기울임, 비트2=밑줄.
        // 출력이 라이브에서 OverrideBold1/Italic1/Underline1 로 적용한다(영역1=공지 본문).
        // 주의: 출력의 "강조 후렴만" 설정이 켜져 있으면 공지(=후렴 아님)에서는 강조가 억제된다(기본은 꺼짐이라 그대로 보인다).
        var effectBits = (options.Bold ? 1 : 0) | (options.Italic ? 2 : 0) | (options.Underline ? 4 : 0);
        if (effectBits != 0)
        {
            sb.Append($"41={effectBits}>");
        }

        // 코드 43 = 영역1 글꼴명(곡 글꼴과 동일 파이프라인) → 출력이 라이브에서 OverrideFontName 으로 적용(영역1=공지 본문).
        // 구분자 제거·다듬기는 곡 글꼴과 같은 공통 규약(SanitizeFontName). 비면 생략(전역 글꼴 추종).
        var fontName = SongFormatData.SanitizeFontName(options.FontName);
        if (fontName.Length > 0)
        {
            sb.Append($"43={fontName}>");
        }

        return sb.Length == 0 ? null : sb.ToString();
    }

    private bool CanSendLiveMessage() => _output.Current.IsOpen && !string.IsNullOrWhiteSpace(OutputLiveMessage);

    private bool CanClearLiveMessage() => _output.Current.IsOpen || !string.IsNullOrWhiteSpace(OutputLiveMessage);

    private void SendLiveMessage()
    {
        var message = OutputLiveMessage.Trim();
        if (PublishNotice(message))
        {
            StatusText = "라이브 메시지 송출";
        }
    }

    private void ClearLiveMessage()
    {
        if (_output.Current.IsOpen)
        {
            ClearNotice();
        }

        OutputLiveMessage = string.Empty;
        NotifyCommandStates();
    }

    private bool CanToggleOutputReferenceAlert()
        => _output.Current.IsOpen && _session.Current.State == LiveState.Active;

    private void ToggleOutputReferenceAlert()
    {
        var show = !_session.Current.IsReferenceAlertVisible;
        var text = show ? ResolveReferenceAlertText(_session.Current) : string.Empty;
        _session.SetReferenceAlert(show, text);
        StatusText = show ? "구절 알림 표시" : "구절 알림 숨김";
        NotifyCommandStates();
    }

    private static string ResolveReferenceAlertText(LiveSessionSnapshot snapshot)
    {
        if (!string.IsNullOrWhiteSpace(snapshot.CurrentItemTitle))
        {
            return snapshot.CurrentItemTitle;
        }

        if (!string.IsNullOrWhiteSpace(snapshot.CurrentSectionLabel))
        {
            return snapshot.CurrentSectionLabel;
        }

        return snapshot.CurrentItemPositionLabel;
    }

    public bool PublishNotice(string text, NoticeOptions? options = null)
    {
        options ??= new NoticeOptions();
        if (string.IsNullOrWhiteSpace(text) || !_output.Current.IsOpen)
        {
            return false;
        }

        var monitorName = _output.Current.Display?.Name ?? OutputDisplay.PrimaryFallback.Name;
        // 글자 크기·정렬·색을 레거시 FormatData(47=pt·31=정렬·29=색)로 실어 기존 곡별 오버라이드 파이프라인을 그대로 재사용.
        // 모두 0(미지정)이면 FormatData 없음 → 출력 기본 사용. 디코더가 6~100pt·정렬 1~3·ARGB 로 검증한다.
        var formatData = BuildNoticeFormatData(options);
        var notice = new LiveQueueItem(LiveItemKinds.NoticeLiveId, "공지", LiveItemKinds.Notice)
        {
            Lyrics = text,
            FormatData = formatData,
        };
        SetLiveItemId(LiveItemKinds.NoticeLiveId);
        OutputItem = notice;
        OutputPowerPoint.Clear();
        _outputThumbnailDeckPath = null;
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
        SetLiveItemId(null);
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

    // 인-셸 가사 폰트 크기 조절(+/- 단계) — 절대값 커밋 함수에 위임(직접 수치 입력과 동일 경로).
    private void StepLyricsFontSize(int delta) => CommitLyricsFontSize(ActiveLyricsFontSize + delta);

    // 인-셸 가사 폰트 크기 절대값 커밋 — 범위로 클램프 후 설정 저장(출력 VM 라이브 반영).
    // −/+ 버튼과 직접 수치 입력(TextBox)이 모두 이 한 경로로 모인다(FrmMain NumericUpDown 직접 입력 대응).
    private void CommitLyricsFontSize(int value)
    {
        var next = Math.Clamp(value, LyricsFontSizeMin, LyricsFontSizeMax);
        if (next == ActiveLyricsFontSize)
        {
            // 범위 밖 값이 입력돼 클램프 후 같아졌으면(예: 999→120, 현재 120) 입력 박스를 클램프값으로 되돌려 표시.
            OnPropertyChanged(nameof(LyricsFontSizeInput));
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorFontSize, next);
        ActiveLyricsFontSize = next;
        StatusText = $"가사 크기: {next}px";
    }

    /// <summary>가사 폰트 크기 직접 수치 입력(TextBox 양방향 바인딩). 범위 밖 값은 클램프되고 입력 박스가 자동 보정된다.</summary>
    public int LyricsFontSizeInput
    {
        get => ActiveLyricsFontSize;
        set => CommitLyricsFontSize(value);
    }

    // 폰트 크기가 다른 경로(−/+ 버튼·설정 창·프리셋)로 바뀌어도 입력 박스가 따라가도록 통지.
    partial void OnActiveLyricsFontSizeChanged(int value) => OnPropertyChanged(nameof(LyricsFontSizeInput));

    // 출력 글꼴 콤보의 추천 글꼴(자주 쓰는 한/영). 시스템 글꼴 병합 전 기본값이자, 병합 시 맨 앞 순서.
    // 추천 글꼴 목록 — 가사·공지 글꼴 콤보가 공유한다(SystemFontCatalog.DefaultFavorites 한 곳에서 정의, 중복 방지).
    private static readonly IReadOnlyList<string> CuratedFontFavorites = SystemFontCatalog.DefaultFavorites;

    /// <summary>
    /// 출력 글꼴 콤보 목록. 처음엔 추천 글꼴만 담고, 시작 시 View 가 <see cref="MergeInstalledFonts"/> 로
    /// 설치된 시스템 글꼴 전체를 합친다(추천 앞·설치 정렬 뒤). 편집 가능 콤보라 목록 밖 글꼴명도 직접 입력할 수 있다.
    /// </summary>
    public ObservableCollection<string> LyricsFontFamilyOptions { get; } = new(CuratedFontFavorites);

    /// <summary>
    /// 설치된 시스템 글꼴을 콤보 목록에 합친다(추천 앞·설치 글꼴 가나다/ABC 순 뒤, 대소문자 무시 중복 제거).
    /// 시작 시 1회 View 가 <c>Fonts.SystemFontFamilies</c> 이름으로 호출한다. 순수 계산은 <see cref="SystemFontCatalog"/> 가
    /// 맡고, 여기선 결과로 컬렉션 내용을 교체한다(ObservableCollection 변경을 콤보가 받아 즉시 갱신). null/빈 목록이면 추천만 유지.
    /// </summary>
    public void MergeInstalledFonts(IEnumerable<string>? installedFamilies)
    {
        var merged = SystemFontCatalog.BuildFontFamilyList(installedFamilies, CuratedFontFavorites);
        LyricsFontFamilyOptions.Clear();
        foreach (var name in merged)
        {
            LyricsFontFamilyOptions.Add(name);
        }
    }

    /// <summary>
    /// 출력 가사 전역 글꼴명 선택(콤보 양방향 바인딩). 빈 문자열/공백이면 "테마 기본 글꼴 상속"으로 저장(무회귀).
    /// 곡별 글꼴(FormatData 43)이 있으면 그 곡 동안은 곡별 글꼴이 우선한다.
    /// </summary>
    public string LyricsFontFamilyInput
    {
        get => ActiveLyricsFontFamily;
        set => CommitLyricsFontFamily(value);
    }

    // 출력 가사 전역 글꼴명 커밋 — 같은 값이면 무시, 다르면 설정 저장(출력 VM 라이브 반영). 앞뒤 공백은 다듬는다.
    private void CommitLyricsFontFamily(string? value)
    {
        var next = (value ?? string.Empty).Trim();
        if (string.Equals(next, ActiveLyricsFontFamily, StringComparison.Ordinal))
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorFontFamily, next);
        ActiveLyricsFontFamily = next;
        StatusText = next.Length == 0 ? "출력 글꼴: 기본(테마)" : $"출력 글꼴: {next}";
    }

    // 글꼴명이 다른 경로(설정 창·기본값 복원 등)로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsFontFamilyChanged(string value) => OnPropertyChanged(nameof(LyricsFontFamilyInput));

    /// <summary>
    /// 보조 영역(Region2) 전역 글꼴명 선택(콤보 양방향 바인딩). 빈 문자열/공백이면 "본문(Region1) 글꼴 추종"으로 저장(무회귀).
    /// 곡별 글꼴(FormatData 44)이 있으면 그 곡 동안은 곡별 글꼴이 우선한다.
    /// </summary>
    public string LyricsFontFamily2Input
    {
        get => ActiveLyricsFontFamily2;
        set => CommitLyricsFontFamily2(value);
    }

    // 보조 영역(Region2) 전역 글꼴명 커밋 — 같은 값이면 무시, 다르면 설정 저장(출력 VM 라이브 반영). 앞뒤 공백은 다듬는다.
    private void CommitLyricsFontFamily2(string? value)
    {
        var next = (value ?? string.Empty).Trim();
        if (string.Equals(next, ActiveLyricsFontFamily2, StringComparison.Ordinal))
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorFontFamily2, next);
        ActiveLyricsFontFamily2 = next;
        StatusText = next.Length == 0 ? "보조영역 글꼴: 본문과 동일" : $"보조영역 글꼴: {next}";
    }

    // 보조영역 글꼴명이 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsFontFamily2Changed(string value) => OnPropertyChanged(nameof(LyricsFontFamily2Input));

    /// <summary>보조 영역(Region2) 글자색 콤보의 (라벨 → ARGB) 프리셋. 0=본문 색 추종, 그 외는 불투명(0xFF..) 색.</summary>
    public IReadOnlyList<KeyValuePair<string, int>> LyricsTextColor2Presets { get; } =
    [
        new("본문과 동일", 0),
        new("흰색", unchecked((int)0xFFFFFFFF)),
        new("노랑", unchecked((int)0xFFFFE066)),
        new("하늘", unchecked((int)0xFF66CCFF)),
        new("연두", unchecked((int)0xFF99E066)),
        new("연회색", unchecked((int)0xFFCCCCCC)),
    ];

    /// <summary>보조 영역(Region2) 전역 글자색 선택(콤보 양방향 바인딩). 0=본문(Region1) 색 추종. 바뀌면 설정 저장(라이브 반영).</summary>
    public int LyricsTextColor2Input
    {
        get => ActiveLyricsTextColor2Argb;
        set
        {
            if (value == ActiveLyricsTextColor2Argb)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.LyricsMonitorTextColor2Argb, value);
            ActiveLyricsTextColor2Argb = value;
            StatusText = value == 0 ? "보조영역 글자색: 본문과 동일" : $"보조영역 글자색: {FormatColorHex(value)}";
        }
    }

    // 보조영역 글자색이 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsTextColor2ArgbChanged(int value) => OnPropertyChanged(nameof(LyricsTextColor2Input));

    /// <summary>보조 영역(Region2) 가로 정렬 콤보의 (라벨 → 모드) 프리셋. FollowRegion1=본문 정렬 추종.</summary>
    public IReadOnlyList<KeyValuePair<string, LyricsRegion2Alignment>> LyricsRegion2AlignmentPresets { get; } =
    [
        new("본문과 동일", LyricsRegion2Alignment.FollowRegion1),
        new("왼쪽", LyricsRegion2Alignment.Left),
        new("가운데", LyricsRegion2Alignment.Center),
        new("오른쪽", LyricsRegion2Alignment.Right),
    ];

    /// <summary>보조 영역(Region2) 전역 가로 정렬 선택(콤보 양방향 바인딩). FollowRegion1=본문 정렬 추종. 바뀌면 설정 저장(라이브 반영).</summary>
    public LyricsRegion2Alignment LyricsRegion2AlignmentInput
    {
        get => ActiveLyricsRegion2Alignment;
        set
        {
            if (value == ActiveLyricsRegion2Alignment)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Alignment, value);
            ActiveLyricsRegion2Alignment = value;
            StatusText = value == LyricsRegion2Alignment.FollowRegion1
                ? "보조영역 정렬: 본문과 동일"
                : $"보조영역 정렬: {value}";
        }
    }

    // 보조영역 정렬이 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsRegion2AlignmentChanged(LyricsRegion2Alignment value) => OnPropertyChanged(nameof(LyricsRegion2AlignmentInput));

    /// <summary>보조 영역(Region2) 굵게 콤보의 (라벨 → 3-상태) 프리셋. FollowRegion1=본문 굵게 추종.</summary>
    public IReadOnlyList<KeyValuePair<string, LyricsRegion2Emphasis>> LyricsRegion2BoldPresets { get; } =
    [
        new("본문과 동일", LyricsRegion2Emphasis.FollowRegion1),
        new("굵게", LyricsRegion2Emphasis.On),
        new("보통", LyricsRegion2Emphasis.Off),
    ];

    /// <summary>보조 영역(Region2) 전역 굵게 선택(콤보 양방향 바인딩). FollowRegion1=본문 굵게 추종. 바뀌면 설정 저장(라이브 반영).</summary>
    public LyricsRegion2Emphasis LyricsRegion2BoldInput
    {
        get => ActiveLyricsRegion2Bold;
        set
        {
            if (value == ActiveLyricsRegion2Bold)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Bold, value);
            ActiveLyricsRegion2Bold = value;
            StatusText = value switch
            {
                LyricsRegion2Emphasis.On => "보조영역 굵게: 켬",
                LyricsRegion2Emphasis.Off => "보조영역 굵게: 보통",
                _ => "보조영역 굵게: 본문과 동일",
            };
        }
    }

    // 보조영역 굵게가 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsRegion2BoldChanged(LyricsRegion2Emphasis value) => OnPropertyChanged(nameof(LyricsRegion2BoldInput));

    /// <summary>보조 영역(Region2) 기울임 콤보의 (라벨 → 3-상태) 프리셋. FollowRegion1=본문 기울임 추종.</summary>
    public IReadOnlyList<KeyValuePair<string, LyricsRegion2Emphasis>> LyricsRegion2ItalicPresets { get; } =
    [
        new("본문과 동일", LyricsRegion2Emphasis.FollowRegion1),
        new("기울임", LyricsRegion2Emphasis.On),
        new("곧게", LyricsRegion2Emphasis.Off),
    ];

    /// <summary>보조 영역(Region2) 전역 기울임 선택(콤보 양방향 바인딩). FollowRegion1=본문 기울임 추종. 바뀌면 설정 저장(라이브 반영).</summary>
    public LyricsRegion2Emphasis LyricsRegion2ItalicInput
    {
        get => ActiveLyricsRegion2Italic;
        set
        {
            if (value == ActiveLyricsRegion2Italic)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Italic, value);
            ActiveLyricsRegion2Italic = value;
            StatusText = value switch
            {
                LyricsRegion2Emphasis.On => "보조영역 기울임: 켬",
                LyricsRegion2Emphasis.Off => "보조영역 기울임: 곧게",
                _ => "보조영역 기울임: 본문과 동일",
            };
        }
    }

    // 보조영역 기울임이 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsRegion2ItalicChanged(LyricsRegion2Emphasis value) => OnPropertyChanged(nameof(LyricsRegion2ItalicInput));

    /// <summary>보조 영역(Region2) 밑줄 콤보의 (라벨 → 3-상태) 프리셋. FollowRegion1=본문 밑줄 추종.</summary>
    public IReadOnlyList<KeyValuePair<string, LyricsRegion2Emphasis>> LyricsRegion2UnderlinePresets { get; } =
    [
        new("본문과 동일", LyricsRegion2Emphasis.FollowRegion1),
        new("밑줄", LyricsRegion2Emphasis.On),
        new("없음", LyricsRegion2Emphasis.Off),
    ];

    /// <summary>보조 영역(Region2) 전역 밑줄 선택(콤보 양방향 바인딩). FollowRegion1=본문 밑줄 추종. 바뀌면 설정 저장(라이브 반영).</summary>
    public LyricsRegion2Emphasis LyricsRegion2UnderlineInput
    {
        get => ActiveLyricsRegion2Underline;
        set
        {
            if (value == ActiveLyricsRegion2Underline)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.LyricsMonitorRegion2Underline, value);
            ActiveLyricsRegion2Underline = value;
            StatusText = value switch
            {
                LyricsRegion2Emphasis.On => "보조영역 밑줄: 켬",
                LyricsRegion2Emphasis.Off => "보조영역 밑줄: 없음",
                _ => "보조영역 밑줄: 본문과 동일",
            };
        }
    }

    // 보조영역 밑줄이 다른 경로로 바뀌어도 콤보가 따라가도록 통지.
    partial void OnActiveLyricsRegion2UnderlineChanged(LyricsRegion2Emphasis value) => OnPropertyChanged(nameof(LyricsRegion2UnderlineInput));

    // Display Panel 글자 크기 비율 조절(+/- 단계, %) — 줄 간격 증감과 동일 구조. 범위 클램프 후 설정 저장(FrmMain Def_PanelFont 크기).
    private void StepPanelFontScale(int delta)
    {
        var next = Math.Clamp(ActivePanelFontScale + delta, LyricsPanelFontScaleMin, LyricsPanelFontScaleMax);
        if (next == ActivePanelFontScale)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorPanelFontScalePercent, next);
        ActivePanelFontScale = next;
        StatusText = $"패널 글자 크기: {next}%";
    }

    // 보조 영역(Region2) 전역 폰트 크기 절대값 커밋 — 0(자동=본문 동일)은 허용, 그 외엔 24~120 으로 클램프(FrmMain Ind_Reg2SizeUpDown).
    private void CommitLyricsFontSize2(int value)
    {
        var next = value <= 0 ? 0 : Math.Clamp(value, LyricsFontSizeMin, LyricsFontSizeMax);
        if (next == ActiveLyricsFontSize2)
        {
            OnPropertyChanged(nameof(LyricsFontSize2Input));
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorFontSize2, next);
        ActiveLyricsFontSize2 = next;
        StatusText = next == 0 ? "보조영역 크기: 본문과 동일(자동)" : $"보조영역 크기: {next}px";
    }

    /// <summary>보조 영역(Region2) 폰트 크기 직접 수치 입력. 0=본문과 동일(자동), 그 외 24~120 클램프.</summary>
    public int LyricsFontSize2Input
    {
        get => ActiveLyricsFontSize2;
        set => CommitLyricsFontSize2(value);
    }

    partial void OnActiveLyricsFontSize2Changed(int value) => OnPropertyChanged(nameof(LyricsFontSize2Input));

    // 인-셸 가사 줄 간격 조절(+/- 단계, %) — 절대값 커밋 함수에 위임(직접 수치 입력과 동일 경로).
    private void StepLyricsLineSpacing(int delta) => CommitLyricsLineSpacing(ActiveLyricsLineSpacing + delta);

    // 인-셸 가사 줄 간격 절대값 커밋(%) — 폰트 크기 커밋과 동일 구조. 범위 클램프 후 설정 저장.
    private void CommitLyricsLineSpacing(int value)
    {
        var next = Math.Clamp(value, LyricsLineSpacingMin, LyricsLineSpacingMax);
        if (next == ActiveLyricsLineSpacing)
        {
            OnPropertyChanged(nameof(LyricsLineSpacingInput));
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorLineSpacingPercent, next);
        ActiveLyricsLineSpacing = next;
        StatusText = $"줄 간격: {next}%";
    }

    /// <summary>가사 줄 간격(%) 직접 수치 입력(TextBox 양방향 바인딩). 범위 밖 값은 클램프되고 입력 박스가 자동 보정된다.</summary>
    public int LyricsLineSpacingInput
    {
        get => ActiveLyricsLineSpacing;
        set => CommitLyricsLineSpacing(value);
    }

    // 줄 간격이 다른 경로로 바뀌어도 입력 박스가 따라가도록 통지.
    partial void OnActiveLyricsLineSpacingChanged(int value) => OnPropertyChanged(nameof(LyricsLineSpacingInput));

    // 본문 왼쪽 여백 조절(+/- 단계) — 절대값 커밋에 위임(직접 수치 입력과 동일 경로, FrmMain ShowLeftMargin).
    private void StepLyricsLeftMargin(int delta) => CommitLyricsLeftMargin(ActiveLyricsLeftMargin + delta);

    // 본문 왼쪽 여백 절대값 커밋(px) — 범위 클램프 후 설정 저장. −/+ 버튼과 직접 입력이 한 경로로 모인다.
    private void CommitLyricsLeftMargin(int value)
    {
        var next = Math.Clamp(value, LyricsBodyMarginMin, LyricsBodyMarginMax);
        if (next == ActiveLyricsLeftMargin)
        {
            OnPropertyChanged(nameof(LyricsLeftMarginInput));
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorBodyLeftMargin, next);
        ActiveLyricsLeftMargin = next;
        StatusText = $"본문 왼쪽 여백: {next}px";
    }

    /// <summary>본문 왼쪽 여백 직접 수치 입력(TextBox 양방향). 범위 밖 값은 클램프되고 입력 박스가 자동 보정.</summary>
    public int LyricsLeftMarginInput
    {
        get => ActiveLyricsLeftMargin;
        set => CommitLyricsLeftMargin(value);
    }

    partial void OnActiveLyricsLeftMarginChanged(int value) => OnPropertyChanged(nameof(LyricsLeftMarginInput));

    // 본문 오른쪽 여백 조절(+/- 단계) — 절대값 커밋에 위임(FrmMain ShowRightMargin).
    private void StepLyricsRightMargin(int delta) => CommitLyricsRightMargin(ActiveLyricsRightMargin + delta);

    private void CommitLyricsRightMargin(int value)
    {
        var next = Math.Clamp(value, LyricsBodyMarginMin, LyricsBodyMarginMax);
        if (next == ActiveLyricsRightMargin)
        {
            OnPropertyChanged(nameof(LyricsRightMarginInput));
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorBodyRightMargin, next);
        ActiveLyricsRightMargin = next;
        StatusText = $"본문 오른쪽 여백: {next}px";
    }

    /// <summary>본문 오른쪽 여백 직접 수치 입력(TextBox 양방향).</summary>
    public int LyricsRightMarginInput
    {
        get => ActiveLyricsRightMargin;
        set => CommitLyricsRightMargin(value);
    }

    partial void OnActiveLyricsRightMarginChanged(int value) => OnPropertyChanged(nameof(LyricsRightMarginInput));

    // 이중 언어 영역 간 세로 간격 조절(+/- 단계, px) — 줄 간격 증감과 동일 구조. 범위 클램프 후 설정 저장(FrmMain Ind_Reg2TopUpDown).
    private void StepRegionGap(int delta)
    {
        var next = Math.Clamp(ActiveRegionGap + delta, LyricsRegionGapMin, LyricsRegionGapMax);
        if (next == ActiveRegionGap)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorRegionGapPx, next);
        ActiveRegionGap = next;
        StatusText = $"영역 간 간격: {next}px";
    }

    // 본문 세로 위치 오프셋 조절(+/- 단계, px) — 영역 간격 증감과 동일 구조. 범위 클램프 후 설정 저장(FrmMain Ind_Reg1TopUpDown).
    private void StepBodyVerticalOffset(int delta)
    {
        var next = Math.Clamp(ActiveBodyVerticalOffset + delta, LyricsBodyVOffsetMin, LyricsBodyVOffsetMax);
        if (next == ActiveBodyVerticalOffset)
        {
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorBodyVerticalOffset, next);
        ActiveBodyVerticalOffset = next;
        StatusText = next == 0 ? "본문 세로 위치: 기본" : $"본문 세로 위치: {(next > 0 ? "+" : "")}{next}px";
    }

    // 본문 아래 여백 조절(+/- 단계) — 절대값 커밋에 위임(FrmMain ShowBottomMargin).
    private void StepLyricsBottomMargin(int delta) => CommitLyricsBottomMargin(ActiveLyricsBottomMargin + delta);

    private void CommitLyricsBottomMargin(int value)
    {
        var next = Math.Clamp(value, LyricsBodyMarginMin, LyricsBodyMarginMax);
        if (next == ActiveLyricsBottomMargin)
        {
            OnPropertyChanged(nameof(LyricsBottomMarginInput));
            return;
        }

        _settings.Set(EasiSettingKeys.LyricsMonitorBodyBottomMargin, next);
        ActiveLyricsBottomMargin = next;
        StatusText = $"본문 아래 여백: {next}px";
    }

    /// <summary>본문 아래 여백 직접 수치 입력(TextBox 양방향).</summary>
    public int LyricsBottomMarginInput
    {
        get => ActiveLyricsBottomMargin;
        set => CommitLyricsBottomMargin(value);
    }

    partial void OnActiveLyricsBottomMarginChanged(int value) => OnPropertyChanged(nameof(LyricsBottomMarginInput));

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
        ActiveGradientDirection = _settings.Get(EasiSettingKeys.LyricsMonitorBackgroundGradientDirection);
        ActiveRegionDisplay = _settings.Get(EasiSettingKeys.LyricsMonitorRegionDisplay);
        ActiveLyricsFontSize = _settings.Get(EasiSettingKeys.LyricsMonitorFontSize);
        ActiveLyricsFontSize2 = _settings.Get(EasiSettingKeys.LyricsMonitorFontSize2);
        ActiveLyricsFontFamily = _settings.Get(EasiSettingKeys.LyricsMonitorFontFamily);
        ActiveLyricsFontFamily2 = _settings.Get(EasiSettingKeys.LyricsMonitorFontFamily2);
        ActiveLyricsTextColor2Argb = _settings.Get(EasiSettingKeys.LyricsMonitorTextColor2Argb);
        ActiveLyricsRegion2Alignment = _settings.Get(EasiSettingKeys.LyricsMonitorRegion2Alignment);
        ActiveLyricsRegion2Bold = _settings.Get(EasiSettingKeys.LyricsMonitorRegion2Bold);
        ActiveLyricsRegion2Italic = _settings.Get(EasiSettingKeys.LyricsMonitorRegion2Italic);
        ActiveLyricsRegion2Underline = _settings.Get(EasiSettingKeys.LyricsMonitorRegion2Underline);
        ActivePanelFontScale = _settings.Get(EasiSettingKeys.LyricsMonitorPanelFontScalePercent);
        ActiveLyricsLineSpacing = _settings.Get(EasiSettingKeys.LyricsMonitorLineSpacingPercent);
        ActiveLyricsLeftMargin = _settings.Get(EasiSettingKeys.LyricsMonitorBodyLeftMargin);
        ActiveLyricsRightMargin = _settings.Get(EasiSettingKeys.LyricsMonitorBodyRightMargin);
        ActiveLyricsBottomMargin = _settings.Get(EasiSettingKeys.LyricsMonitorBodyBottomMargin);
        ActiveRegionGap = _settings.Get(EasiSettingKeys.LyricsMonitorRegionGapPx);
        ActiveBodyVerticalOffset = _settings.Get(EasiSettingKeys.LyricsMonitorBodyVerticalOffset);
        ActiveLyricsBold = _settings.Get(EasiSettingKeys.LyricsMonitorBold);
        ActiveLyricsItalic = _settings.Get(EasiSettingKeys.LyricsMonitorItalic);
        ActiveLyricsShadow = _settings.Get(EasiSettingKeys.LyricsMonitorShadow);
        ActiveLyricsNotations = _settings.Get(EasiSettingKeys.LyricsMonitorShowNotations);
        ActiveLyricsUnderline = _settings.Get(EasiSettingKeys.LyricsMonitorUnderline);
        ActiveLyricsEmphasisChorusOnly = _settings.Get(EasiSettingKeys.LyricsMonitorEmphasisChorusOnly);
        ActiveLyricsInterlace = _settings.Get(EasiSettingKeys.LyricsMonitorInterlace);
        ActiveLyricsPanelTransparent = _settings.Get(EasiSettingKeys.LyricsMonitorPanelTransparent);
        ActiveLyricsPositionIndicator = _settings.Get(EasiSettingKeys.LyricsMonitorShowPositionIndicator);
        ActiveLyricsVerseHeading = _settings.Get(EasiSettingKeys.LyricsMonitorShowVerseHeading);
        ActiveLyricsItemNumber = _settings.Get(EasiSettingKeys.LyricsMonitorShowItemNumber);
        ActiveLyricsTitleOnPanel = _settings.Get(EasiSettingKeys.LyricsMonitorShowTitleOnPanel);
        ActiveLyricsCopyright = _settings.Get(EasiSettingKeys.LyricsMonitorShowCopyright);
        ActiveLyricsNextItem = _settings.Get(EasiSettingKeys.LyricsMonitorShowNextItem);
        ActiveFadeTransition = _settings.Get(EasiSettingKeys.LyricsMonitorUseFadeTransition);
        ActiveTransitionDurationMs = _settings.Get(EasiSettingKeys.LyricsMonitorTransitionDurationMs);
        ActiveTransitionKind = _settings.Get(EasiSettingKeys.LyricsMonitorTransitionKind);
        ActiveSlideTransitionKind = _settings.Get(EasiSettingKeys.LyricsMonitorSlideTransitionKind);
        // 대기 화면(Gap) 모드·페이드·로고 — 설정 창 등 다른 경로로 바뀌어도 출력 메뉴 표시가 따라가게 동기화.
        ActiveGapItemOption = _settings.Get(EasiSettingKeys.GapItemOption);
        ActiveGapItemUseFade = _settings.Get(EasiSettingKeys.GapItemUseFade);
        ActiveGapItemLogoFile = _settings.Get(EasiSettingKeys.GapItemLogoFile);
        ActiveLyricsTitleHeading = _settings.Get(EasiSettingKeys.LyricsMonitorShowTitleHeading);
        ActiveLyricsOutline = _settings.Get(EasiSettingKeys.LyricsMonitorOutline);
        ActiveTitleHeadingAlignment = _settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingAlignment);
        ActiveTitleHeadingFirstScreenOnly = _settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly);
        ActiveTitleHeadingFollowBody = _settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFollowBody);
        ActiveTitleHeadingFollowRegion2 = _settings.Get(EasiSettingKeys.LyricsMonitorTitleHeadingFollowRegion2);
        ActiveTextColorHex = FormatColorHex(text);
        ActiveBackgroundColorHex = FormatColorHex(bg1);
    }

    private void ApplyOperationalSettings(bool updateStatus)
    {
        // 인스펙터 접힘/펼침 상태 복원(레거시 패널 상태 저장). 저장값이 기본과 같으면 OnChanged 가 안 울려 무회귀.
        // try/finally 로 플래그를 반드시 풀어 준다 — 복원 도중 예외가 나도 플래그가 true 로 갇혀
        // 이후 모든 토글이 조용히 저장을 멈추는 일이 없게 한다(리뷰어 지적: 가드 플래그 예외 안전).
        _isApplyingOperationalSettings = true;
        try
        {
            IsInspectorExpanded = _settings.Get(EasiSettingKeys.MainInspectorExpanded);
        }
        finally
        {
            _isApplyingOperationalSettings = false;
        }
        IsPowerPointTabVisible = _settings.Get(EasiSettingKeys.UsePowerPointTab);
        IsPowerPointPanelOverlayEnabled = !_settings.Get(EasiSettingKeys.NoPowerPointPanelOverlay);
        PowerPointMaxFiles = _settings.Get(EasiSettingKeys.PowerPointMaxFiles);
        IsMediaTabVisible = _settings.Get(EasiSettingKeys.UseMediaTab);
        IsMediaPanelOverlayEnabled = !_settings.Get(EasiSettingKeys.NoMediaPanelOverlay);
        MediaDirectory = _settings.Get(EasiSettingKeys.MediaDirectory);
        LiveCameraNumber = _settings.Get(EasiSettingKeys.LiveCameraNumber);
        LiveCameraSource = MediaPlaybackService.CreateLiveCameraSource(LiveCameraNumber);
        AutoRotateIntervalSeconds = _settings.Get(EasiSettingKeys.AutoRotateIntervalSeconds);
        ActiveAutoRotateMode = _settings.Get(EasiSettingKeys.AutoRotateMode);
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
        // 라이브 위치(곡 절 "3/12"·PPT 슬라이드 "5/20") — 절/슬라이드 이동마다 세션이 다시 알려 LiveBar 가 갱신된다(없으면 빈 문자열→숨김).
        LiveBar.PositionLabel = snapshot.CurrentItemPositionLabel;
        // 다음 송출 예정 항목 — 운영자가 미리 준비하도록 LiveBar 에 "다음 ▸ X"로 보여 준다(마지막 항목이면 빈 문자열→숨김).
        LiveBar.NextItemTitle = snapshot.CurrentItemNextTitle;
        LiveBar.OutputMonitorName = snapshot.OutputMonitorName;
        RefreshOutputSurfaceText();
        RebuildOutputLyricsPages();
        // 자동 회전은 라이브 완전 종료(Stop=Off)에서만 해제한다 — 숨김/검정/비우기(Hidden)는 임시 상태라
        // 유지하고 복귀(Restore→Active) 시 그대로 이어간다. 숨김 중에는 AdvanceAutoRotation 이 State!=Active 로
        // no-op 이라 출력을 깨우지 않으므로 회전 상태를 유지해도 안전하다(View 타이머가 IsAutoRotating 을 보고 멈춤).
        if (snapshot.State == LiveState.Off && IsAutoRotating)
        {
            IsAutoRotating = false;
        }
        NotifyOutputLiveSafetyProperties();
        NotifyCommandStates();
    }

    private void NotifyOutputLiveSafetyProperties()
    {
        OnPropertyChanged(nameof(IsOutputBlackActive));
        OnPropertyChanged(nameof(IsOutputClearActive));
        OnPropertyChanged(nameof(IsOutputLiveActive));
    }

    private void NotifyCommandStates()
    {
        ToggleUseIndividualFormattingCommand.NotifyCanExecuteChanged();
        CopyPreviewToOutputCommand.NotifyCanExecuteChanged();
        CopyPreviewToOutputAndNextCommand.NotifyCanExecuteChanged();
        CopyPreviewToOutputShortcutCommand.NotifyCanExecuteChanged();
        CopyPreviewToOutputAndClearBlackCommand.NotifyCanExecuteChanged();
        PreviewToLiveCommand.NotifyCanExecuteChanged();
        PlaySelectedWorshipMediaCommand.NotifyCanExecuteChanged();
        PlaySelectedWorshipMediaOnOutputCommand.NotifyCanExecuteChanged();
        PlayOutputMediaCommand.NotifyCanExecuteChanged();
        GoLiveCommand.NotifyCanExecuteChanged();
        SendToOutputAndNextCommand.NotifyCanExecuteChanged();
        CloseOutputCommand.NotifyCanExecuteChanged();
        StopLiveCommand.NotifyCanExecuteChanged();
        NextItemCommand.NotifyCanExecuteChanged();
        PreviousItemCommand.NotifyCanExecuteChanged();
        NextOutputItemCommand.NotifyCanExecuteChanged();
        PreviousOutputItemCommand.NotifyCanExecuteChanged();
        FirstOutputItemCommand.NotifyCanExecuteChanged();
        LastOutputItemCommand.NotifyCanExecuteChanged();
        JumpToNextNonRotateOutputItemCommand.NotifyCanExecuteChanged();
        FirstItemCommand.NotifyCanExecuteChanged();
        LastItemCommand.NotifyCanExecuteChanged();
        HideOutputCommand.NotifyCanExecuteChanged();
        BlackScreenCommand.NotifyCanExecuteChanged();
        ClearOutputCommand.NotifyCanExecuteChanged();
        ToggleOutputBlackCommand.NotifyCanExecuteChanged();
        ToggleOutputClearCommand.NotifyCanExecuteChanged();
        ToggleOutputLiveCommand.NotifyCanExecuteChanged();
        SendLiveMessageCommand.NotifyCanExecuteChanged();
        ClearLiveMessageCommand.NotifyCanExecuteChanged();
        ToggleOutputReferenceAlertCommand.NotifyCanExecuteChanged();
        RestoreOutputCommand.NotifyCanExecuteChanged();
        RestartCurrentItemCommand.NotifyCanExecuteChanged();
        RefreshOutputCommand.NotifyCanExecuteChanged();
        NextSlideCommand.NotifyCanExecuteChanged();
        PreviousSlideCommand.NotifyCanExecuteChanged();
        GoToSlideCommand.NotifyCanExecuteChanged();
        NextOutputSlideCommand.NotifyCanExecuteChanged();
        PreviousOutputSlideCommand.NotifyCanExecuteChanged();
        GoToOutputSlideCommand.NotifyCanExecuteChanged();
        GoToPreviewLyricsPageCommand.NotifyCanExecuteChanged();
        GoToOutputLyricsPageCommand.NotifyCanExecuteChanged();
        JumpToOutputLyricsSectionCommand.NotifyCanExecuteChanged();
        AddSelectedLibrarySongCommand.NotifyCanExecuteChanged();
        MoveSelectedItemUpCommand.NotifyCanExecuteChanged();
        MoveSelectedItemDownCommand.NotifyCanExecuteChanged();
        MoveSelectedItemToTopCommand.NotifyCanExecuteChanged();
        MoveSelectedItemToBottomCommand.NotifyCanExecuteChanged();
        RemoveSelectedItemCommand.NotifyCanExecuteChanged();
        DuplicateSelectedItemCommand.NotifyCanExecuteChanged();
        SetSelectedItemTextColorCommand.NotifyCanExecuteChanged();
        SetSelectedItemAlignmentCommand.NotifyCanExecuteChanged();
        SetSelectedItemFontSizeCommand.NotifyCanExecuteChanged();
        SetSelectedItemFontNameCommand.NotifyCanExecuteChanged();
        SetSelectedItemBackgroundColorCommand.NotifyCanExecuteChanged();
        SetSelectedItemBackgroundImageCommand.NotifyCanExecuteChanged();
        ToggleSelectedItemBoldCommand.NotifyCanExecuteChanged();
        ToggleSelectedItemItalicCommand.NotifyCanExecuteChanged();
        ToggleSelectedItemUnderlineCommand.NotifyCanExecuteChanged();
        SetSelectedItemTextColor2Command.NotifyCanExecuteChanged();
        SetSelectedItemAlignment2Command.NotifyCanExecuteChanged();
        SetSelectedItemFontSize2Command.NotifyCanExecuteChanged();
        SetSelectedItemFontName2Command.NotifyCanExecuteChanged();
        ToggleSelectedItemBold2Command.NotifyCanExecuteChanged();
        ToggleSelectedItemItalic2Command.NotifyCanExecuteChanged();
        ToggleSelectedItemUnderline2Command.NotifyCanExecuteChanged();
        ClearSelectedItemFormattingCommand.NotifyCanExecuteChanged();
        CopySelectedItemFormattingCommand.NotifyCanExecuteChanged();
        PasteSelectedItemFormattingCommand.NotifyCanExecuteChanged();
        ApplyCopiedFormatToAllCommand.NotifyCanExecuteChanged();
        ClearAllItemsFormattingCommand.NotifyCanExecuteChanged();
        ClearWorshipListCommand.NotifyCanExecuteChanged();
        RestoreClearedWorshipListCommand.NotifyCanExecuteChanged();
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
        OutputPowerPoint.PropertyChanged -= OnOutputPowerPointPropertyChanged;
        Library.PropertyChanged -= OnLibraryPropertyChanged;
        Search.SearchResults.CollectionChanged -= OnSearchResultsChanged;
        Search.LookupCandidates.CollectionChanged -= OnLookupCandidatesChanged;
        // Media VM 정리. DI 컨테이너도 transient IDisposable 을 추적·해제하므로 이중 호출될 수 있으나
        // MediaPlaybackViewModel.Dispose 가 멱등이라 안전(테스트는 new 생성이라 이 경로가 유일 해제).
        Media.Dispose();
        // PowerPoint VM 은 이벤트 구독/미관리 자원이 없어 IDisposable 이 아니다 — 의도적으로 해제하지 않음.
    }

}
