using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 줄 교차(인터레이스) 송출용 한 줄 — 각 줄이 자기 영역(Region1=주 언어/Region2=보조 언어)의
/// 색·글꼴·크기·굵게·기울임·밑줄·정렬을 그대로 지닌다. 출력의 ItemsControl 이 이 목록을 줄마다 렌더한다.
/// </summary>
public sealed record InterlacedLine(
    string Text,
    Brush Foreground,
    object FontFamily,
    double FontSize,
    FontWeight FontWeight,
    FontStyle FontStyle,
    TextDecorationCollection? TextDecorations,
    TextAlignment TextAlignment,
    double LineHeight);

public sealed class OutputWindowViewModel : ObservableObject, IDisposable
{
    private const int DefaultViewportWidth = 1280;
    private const int DefaultViewportHeight = 720;
    // 제목 헤딩 배너가 차지하는 상단 높이(px) — 헤딩 표시 중 본문 상단 여백으로 예약(겹침 방지, §7.3-A).
    // 헤딩 폰트 40px + 상단 마진 28px + 여유. 헤딩이 1줄(NoWrap)이라 결정적.
    private const double TitleHeadingReservedHeight = 96;

    private readonly IOutputRenderer _renderer;
    private readonly ISettingsService? _settings;
    private readonly Func<string, ImageSource?> _gapLogoLoader;
    private LiveState _state = LiveState.Off;
    private LiveSessionSnapshot _session = LiveSessionSnapshot.Off;
    // 직전 송출 항목의 식별자 — 다음 ApplySession 에서 "항목이 바뀌었는지"를 판별해 전환 효과(항목 vs 슬라이드)를 고른다.
    private string _previousLiveItemId = string.Empty;
    private OutputWindowState _output = OutputWindowState.Closed;
    private string _currentItemTitle = string.Empty;
    private string _outputMonitorName = string.Empty;
    private string _displayTitle = "STANDBY";
    private string _statusLabel = "STANDBY";
    private string _bodyText = string.Empty;
    // 이중 언어 Region2(보조 언어) 본문 + 표시 + 글자색(Region2 색). 단일 영역이면 빈 문자열·Collapsed(무회귀).
    private string _bodyText2 = string.Empty;
    private Visibility _bodyText2Visibility = Visibility.Collapsed;
    private bool _isBlackout;
    private bool _isOutputOpen;
    private Brush _sceneForegroundBrush;
    // Region2(이중 언어 보조 언어) 글자색 브러시 — 곡별 region2 색(30) 또는 Region1 색 추종.
    private Brush _sceneForegroundBrush2;
    private Brush _sceneBackgroundBrush;
    private Visibility _lyricsAlertVisibility = Visibility.Collapsed;
    private string _lyricsAlertText = string.Empty;
    private Brush _lyricsAlertForegroundBrush = CreateBrush(EasiSettingKeys.LyricsMonitorHighlightColorArgb.DefaultValue);
    private Brush _lyricsAlertBackgroundBrush = CreateBrush(EasiSettingKeys.LyricsMonitorBackgroundColorArgb.DefaultValue);
    private Visibility _referenceAlertVisibility = Visibility.Collapsed;
    private string _referenceAlertText = string.Empty;
    private Brush _referenceAlertForegroundBrush = CreateBrush(LiveOutputRenderSettings.Default.LyricsMonitorTextColorArgb);
    private Brush _referenceAlertBackgroundBrush = PanelDefaultBrush;
    private double _referenceAlertHorizontalOffset;
    private Visibility _panelOverlayVisibility = Visibility.Visible;
    private Visibility _displayTitleVisibility = Visibility.Visible;
    private Visibility _bodyTextVisibility = Visibility.Collapsed;
    private TextAlignment _bodyTextAlignment = TextAlignment.Center;
    // 이중 언어 Region2 본문 가로 정렬(곡별 region2 정렬 32) — 없으면 Region1 정렬 추종.
    private TextAlignment _bodyText2Alignment = TextAlignment.Center;
    private HorizontalAlignment _bodyHorizontalAlignment = HorizontalAlignment.Center;
    private VerticalAlignment _bodyVerticalAlignment = VerticalAlignment.Center;
    private double _bodyFontSize = 48;
    private double _bodyLineHeight = 60;
    // 본문 글꼴 — 곡별 FormatData 폰트명(43) 오버라이드. 오버라이드가 없으면 UnsetValue 로 두어
    // XAML FontFamily 바인딩이 부모(테마)의 글꼴을 그대로 상속하게 한다(null 로 두면 기본 글꼴로 떨어져 회귀 위험).
    private object _bodyFontFamily = DependencyProperty.UnsetValue;
    // 이중 언어 Region2 본문 글꼴·크기(곡별 region2 글꼴 44/48) — 없으면 Region1 글꼴 추종.
    private object _bodyFont2Family = DependencyProperty.UnsetValue;
    private double _bodyFont2Size = 48;
    private FontWeight _bodyFontWeight = FontWeights.SemiBold;
    private FontStyle _bodyFontStyle = FontStyles.Normal;
    private FontWeight _bodyFont2Weight = FontWeights.SemiBold;
    private FontStyle _bodyFont2Style = FontStyles.Normal;
    private bool _bodyHasShadow;
    private TextDecorationCollection? _bodyTextDecorations;
    private TextDecorationCollection? _bodyText2Decorations;
    private bool _bodyUnderline;
    private Visibility _interlaceVisibility = Visibility.Collapsed;
    // 외곽선 효과 사용 여부(§7.3-A). on 이면 본문을 외곽선 렌더러로 그린다(일반 본문과 상호배타).
    private bool _bodyHasOutline;
    // 외곽선 렌더러(OutlinedTextBlock) 가시성 — 외곽선 on + 본문 송출 중일 때만 Visible.
    private Visibility _bodyOutlineVisibility = Visibility.Collapsed;
    private string _positionLabel = string.Empty;
    private Visibility _positionIndicatorVisibility = Visibility.Collapsed;
    private string _verseHeadingText = string.Empty;
    private Visibility _verseHeadingVisibility = Visibility.Collapsed;
    // 곡 번호 표시(Display Panel) — 설정 on + Live + 번호 있을 때만 보인다.
    private string _itemNumberText = string.Empty;
    private Visibility _itemNumberVisibility = Visibility.Collapsed;
    private Visibility _titleOnPanelVisibility = Visibility.Collapsed;
    private string _copyrightText = string.Empty;
    private Visibility _copyrightVisibility = Visibility.Collapsed;
    private string _nextItemText = string.Empty;
    private Visibility _nextItemVisibility = Visibility.Collapsed;
    // 제목 헤딩(가사 위 상단 배너) 가시성 — 기본 숨김(설정 off 일 때 기존 동작 보존). §7.3-A
    private Visibility _titleHeadingVisibility = Visibility.Collapsed;
    // 제목 헤딩 가로 정렬(설정에서 유래, §7.3-A Heading Align). 기본 가운데.
    private TextAlignment _titleHeadingTextAlignment = TextAlignment.Center;
    private HorizontalAlignment _titleHeadingHorizontalAlignment = HorizontalAlignment.Center;
    // 본문 상단 여백 — 제목 헤딩이 보일 때만 헤딩 배너 높이만큼 위를 비워(겹침 방지),
    // 본문 세로 정렬이 "위"여도 헤딩과 포개지지 않게 한다(§7.3-A code-review MAJOR 반영). 기본 0.
    private Thickness _bodyContentMargin = new(0);
    private Thickness _bodyText2Margin = new(0, 8, 0, 0);
    private double _bodyVerticalOffset;
    private Visibility _gapLogoVisibility = Visibility.Collapsed;
    private Visibility _blackoutOverlayVisibility = Visibility.Collapsed;
    private Visibility _contentVisibility = Visibility.Collapsed;
    private ImageSource? _gapLogoSource;
    // 외부에서 주입되는 콘텐츠 이미지(예: PPT 슬라이드 썸네일).
    // 비어 있으면 ContentVisibility=Collapsed로 화면에 표시되지 않는다.
    private ImageSource? _contentImageSource;
    private int _contentPixelWidth;
    private int _contentPixelHeight;
    private ImageFillMode _contentFillMode = ImageFillMode.Fit;
    private double _contentLeft;
    private double _contentTop;
    private double _contentWidth;
    private double _contentHeight;
    // GapLogoLoader 캐시: 같은 경로를 매번 디스크에서 다시 디코딩하지 않도록 보관
    private string? _cachedGapLogoPath;
    private ImageSource? _cachedGapLogoSource;
    // 곡별 배경 이미지(61) — 색 배경 위에 표시(이미지 우선). 없으면 색 배경만 보인다(무회귀).
    private ImageSource? _sceneBackgroundImageSource;
    private Brush? _backgroundImageBrush;
    private Brush _panelBackgroundBrush = PanelDefaultBrush;
    private Brush _panelForegroundBrush = CreateBrush(LiveOutputRenderSettings.Default.LyricsMonitorTextColorArgb);
    private double _panelPrimaryFontSize = PanelPrimaryBaseFontSize;
    private double _panelSecondaryFontSize = PanelSecondaryBaseFontSize;
    private FontWeight _panelPrimaryFontWeight = FontWeights.SemiBold;
    private FontWeight _panelSecondaryFontWeight = FontWeights.Normal;
    private FontStyle _panelFontStyle = FontStyles.Normal;
    private TextDecorationCollection? _panelTextDecorations;
    private Visibility _backgroundImageVisibility = Visibility.Collapsed;
    // 배경 이미지 캐시: 같은(해석된) 경로를 매번 다시 디코딩하지 않도록 보관.
    private string? _cachedBackgroundImagePath;
    private ImageSource? _cachedBackgroundImageSource;
    private OutputSceneSnapshot _scene;
    private TimeSpan _contentFadeDuration = TimeSpan.FromMilliseconds(250);
    private LyricsTransitionKind _contentTransitionKind = LyricsTransitionKind.Fade;
    private readonly DispatcherTimer _lyricsAlertFlashTimer;
    private readonly DispatcherTimer _referenceAlertFlashTimer;
    private readonly DispatcherTimer _referenceAlertScrollTimer;
    private bool _lastShowsLiveLyricsAlertMessage;
    private string _lastLyricsAlertMessage = string.Empty;
    private bool _lyricsAlertFlashAlternate;
    private int _lyricsAlertFlashCount;
    private bool _lastShowsLiveReferenceAlert;
    private string _lastReferenceAlertText = string.Empty;
    private bool _referenceAlertFlashAlternate;
    private int _referenceAlertFlashCount;
    private int _referenceAlertScrollTick;
    private int _referenceAlertScrollTotalTicks = 1;
    private bool _disposed;

    public OutputWindowViewModel()
        : this(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()), settings: null)
    {
    }

    public OutputWindowViewModel(IOutputRenderer renderer)
        : this(renderer, settings: null)
    {
    }

    public OutputWindowViewModel(IOutputRenderer renderer, ISettingsService? settings)
        : this(renderer, settings, gapLogoLoader: null)
    {
    }

    public OutputWindowViewModel(
        IOutputRenderer renderer,
        ISettingsService? settings,
        Func<string, ImageSource?>? gapLogoLoader)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _settings = settings;
        _lyricsAlertFlashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _lyricsAlertFlashTimer.Tick += (_, _) => AdvanceLyricsAlertFlash();
        _referenceAlertFlashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _referenceAlertFlashTimer.Tick += (_, _) => AdvanceReferenceAlertFlash();
        _referenceAlertScrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _referenceAlertScrollTimer.Tick += (_, _) => AdvanceReferenceAlertScroll();
        // 로더 미주입 시 기본 로더(BitmapImage)로 디스크에서 직접 로드
        _gapLogoLoader = gapLogoLoader ?? DefaultGapLogoLoader;
        _sceneForegroundBrush = CreateBrush(LiveOutputRenderSettings.Default.LyricsMonitorTextColorArgb);
        _sceneForegroundBrush2 = CreateBrush(LiveOutputRenderSettings.Default.LyricsMonitorTextColorArgb);
        _sceneBackgroundBrush = CreateBackgroundBrush(
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundColorArgb,
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundColor2Argb,
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundIsGradient,
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundGradientDirection);
        if (_settings is not null)
        {
            _settings.SettingsChanged += OnSettingsChanged;
        }

        // 전환(페이드) 설정을 초기 적용 — 설정 미주입(테스트)이면 기본 250ms 유지.
        ApplyTransitionSettings();

        _scene = CreateScene();
        ApplyScene(_scene);
    }

    // 출력 장면 전환 페이드 길이를 설정에서 읽어 ContentFadeDuration 에 반영한다(FrmMain 전환 효과).
    // 페이드 off 면 0(즉시 컷), on 이면 설정한 ms. 설정 미주입이면 기본값을 유지한다.
    private void ApplyTransitionSettings()
    {
        if (_settings is null)
        {
            return;
        }

        var useFade = _settings.Get(EasiSettingKeys.LyricsMonitorUseFadeTransition);
        var durationMs = _settings.Get(EasiSettingKeys.LyricsMonitorTransitionDurationMs);
        ContentFadeDuration = useFade && durationMs > 0
            ? TimeSpan.FromMilliseconds(durationMs)
            : TimeSpan.Zero;
        // 전환 모션 종류(Fade/Slide). 페이드 off(ContentFadeDuration=0)면 뷰가 즉시 컷하므로 종류는 무시된다.
        ContentTransitionKind = _settings.Get(EasiSettingKeys.LyricsMonitorTransitionKind);
    }

    public LiveState State
    {
        get => _state;
        private set => SetProperty(ref _state, value);
    }

    public string CurrentItemTitle
    {
        get => _currentItemTitle;
        private set => SetProperty(ref _currentItemTitle, value);
    }

    public string OutputMonitorName
    {
        get => _outputMonitorName;
        private set => SetProperty(ref _outputMonitorName, value);
    }

    public string DisplayTitle
    {
        get => _displayTitle;
        private set => SetProperty(ref _displayTitle, value);
    }

    public string StatusLabel
    {
        get => _statusLabel;
        private set => SetProperty(ref _statusLabel, value);
    }

    /// <summary>라이브 곡 가사 본문(출력 화면 중앙 텍스트). 곡이 아니거나 Live 가 아니면 빈 문자열.</summary>
    public string BodyText
    {
        get => _bodyText;
        private set => SetProperty(ref _bodyText, value);
    }

    /// <summary>일반(외곽선 없는) 가사 본문 표시 여부 — 본문 송출 중 + 외곽선 off 일 때만 Visible(외곽선 렌더러와 상호배타).</summary>
    public Visibility BodyTextVisibility
    {
        get => _bodyTextVisibility;
        private set => SetProperty(ref _bodyTextVisibility, value);
    }

    /// <summary>이중 언어 Region2(보조 언어) 본문. 단일 영역 곡·비곡은 빈 문자열.</summary>
    public string BodyText2
    {
        get => _bodyText2;
        private set => SetProperty(ref _bodyText2, value);
    }

    /// <summary>Region2 본문 표시 여부 — 이중 언어 곡 본문 송출 중일 때만 Visible(단일 영역은 Collapsed).</summary>
    public Visibility BodyText2Visibility
    {
        get => _bodyText2Visibility;
        private set => SetProperty(ref _bodyText2Visibility, value);
    }

    /// <summary>Region2 글자색 브러시 — 곡별 region2 색(30) 또는 Region1 색 추종.</summary>
    public Brush SceneForegroundBrush2
    {
        get => _sceneForegroundBrush2;
        private set => SetProperty(ref _sceneForegroundBrush2, value);
    }

    /// <summary>Region2 본문 줄 정렬(좌/중/우) — 곡별 region2 정렬(32) 또는 Region1 정렬 추종.</summary>
    public TextAlignment BodyText2Alignment
    {
        get => _bodyText2Alignment;
        private set => SetProperty(ref _bodyText2Alignment, value);
    }

    /// <summary>Region2 본문 글꼴(곡별 region2 폰트명 44) — 없으면 UnsetValue(Region1/테마 글꼴 상속).</summary>
    public object BodyFont2Family
    {
        get => _bodyFont2Family;
        private set => SetProperty(ref _bodyFont2Family, value);
    }

    /// <summary>Region2 본문 폰트 크기(px) — 곡별 region2 크기(48) 또는 Region1 크기 추종.</summary>
    public double BodyFont2Size
    {
        get => _bodyFont2Size;
        private set => SetProperty(ref _bodyFont2Size, value);
    }

    /// <summary>외곽선 가사 본문(OutlinedTextBlock) 표시 여부 — 본문 송출 중 + 외곽선 on 일 때만 Visible(§7.3-A).</summary>
    public Visibility BodyOutlineVisibility
    {
        get => _bodyOutlineVisibility;
        private set => SetProperty(ref _bodyOutlineVisibility, value);
    }

    /// <summary>가사 외곽선 효과 사용 여부(설정에서 유래, §7.3-A 폰트 효과).</summary>
    public bool BodyHasOutline
    {
        get => _bodyHasOutline;
        private set => SetProperty(ref _bodyHasOutline, value);
    }

    /// <summary>가사 본문 줄 정렬(좌/중/우) — 인-셸 가사 정렬 설정에서 유래(§7.3-A).</summary>
    public TextAlignment BodyTextAlignment
    {
        get => _bodyTextAlignment;
        private set => SetProperty(ref _bodyTextAlignment, value);
    }

    /// <summary>가사 본문 블록의 가로 배치(좌/중/우) — MaxWidth 블록을 화면 좌/중/우로 정렬.</summary>
    public HorizontalAlignment BodyHorizontalAlignment
    {
        get => _bodyHorizontalAlignment;
        private set => SetProperty(ref _bodyHorizontalAlignment, value);
    }

    /// <summary>가사 본문 블록의 세로 배치(상/중/하) — 화면 위/가운데/아래로 정렬(§7.3-A).</summary>
    public VerticalAlignment BodyVerticalAlignment
    {
        get => _bodyVerticalAlignment;
        private set => SetProperty(ref _bodyVerticalAlignment, value);
    }

    /// <summary>가사 본문 폰트 크기(px) — 인-셸 폰트 크기 설정에서 유래(§7.3-A).</summary>
    public double BodyFontSize
    {
        get => _bodyFontSize;
        private set => SetProperty(ref _bodyFontSize, value);
    }

    /// <summary>가사 본문 줄 높이 — 폰트 크기의 1.25배(가독성 비례).</summary>
    public double BodyLineHeight
    {
        get => _bodyLineHeight;
        private set => SetProperty(ref _bodyLineHeight, value);
    }

    /// <summary>
    /// 가사 본문 글꼴(곡별 FormatData 폰트명 43 오버라이드). 오버라이드가 없으면 <see cref="DependencyProperty.UnsetValue"/> —
    /// XAML FontFamily 바인딩이 UnsetValue 를 받으면 부모(테마)의 글꼴을 상속한다(기존 출력 글꼴 무회귀). §7.3-A per-song 폰트.
    /// </summary>
    public object BodyFontFamily
    {
        get => _bodyFontFamily;
        private set => SetProperty(ref _bodyFontFamily, value);
    }

    /// <summary>가사 본문 굵기 — 굵게 on=Bold, off=SemiBold(기존 출력 보존). §7.3-A 폰트 효과.</summary>
    public FontWeight BodyFontWeight
    {
        get => _bodyFontWeight;
        private set => SetProperty(ref _bodyFontWeight, value);
    }

    /// <summary>가사 본문 기울임 — on=Italic, off=Normal. §7.3-A 폰트 효과.</summary>
    public FontStyle BodyFontStyle
    {
        get => _bodyFontStyle;
        private set => SetProperty(ref _bodyFontStyle, value);
    }

    /// <summary>Region2(이중 언어 보조) 본문 굵게 — 곡별 region2 비트가 있으면 그것, 없으면 Region1 효과를 추종.</summary>
    public FontWeight BodyFont2Weight
    {
        get => _bodyFont2Weight;
        private set => SetProperty(ref _bodyFont2Weight, value);
    }

    /// <summary>Region2(이중 언어 보조) 본문 기울임 — 곡별 region2 비트가 있으면 그것, 없으면 Region1 효과를 추종.</summary>
    public FontStyle BodyFont2Style
    {
        get => _bodyFont2Style;
        private set => SetProperty(ref _bodyFont2Style, value);
    }

    /// <summary>가사 본문 그림자 표시 여부 — 어두운/영상 배경 위 가독성. XAML 이 DropShadowEffect 적용. §7.3-A.</summary>
    public bool BodyHasShadow
    {
        get => _bodyHasShadow;
        private set => SetProperty(ref _bodyHasShadow, value);
    }

    /// <summary>Region1 본문 밑줄 — on 이면 TextDecorations.Underline, off 면 null. 곡별 region1 비트∥전역으로 해석된 값.</summary>
    public TextDecorationCollection? BodyTextDecorations
    {
        get => _bodyTextDecorations;
        private set => SetProperty(ref _bodyTextDecorations, value);
    }

    /// <summary>Region2(이중 언어 보조) 본문 밑줄 — 곡별 region2 비트가 있으면 그것, 없으면 Region1 추종으로 해석된 값.</summary>
    public TextDecorationCollection? BodyText2Decorations
    {
        get => _bodyText2Decorations;
        private set => SetProperty(ref _bodyText2Decorations, value);
    }

    /// <summary>Region1 밑줄(bool) — 외곽선 모드(OutlinedTextBlock)용. 일반 TextBlock 은 BodyTextDecorations 를, 외곽선 컨트롤은 이 bool 을 쓴다.</summary>
    public bool BodyUnderline
    {
        get => _bodyUnderline;
        private set => SetProperty(ref _bodyUnderline, value);
    }

    /// <summary>줄 교차(인터레이스) 송출 줄 목록 — Region1·Region2 줄을 번갈아 담는다(각 줄이 자기 영역 스타일을 지님). 비-인터레이스 시 빈 목록.</summary>
    public ObservableCollection<InterlacedLine> InterlacedLines { get; } = new();

    /// <summary>줄 교차 목록의 가시성 — 인터레이스 on + 두 영역이 모두 보일 때만 Visible(그때 기존 두 밴드는 숨김).</summary>
    public Visibility InterlaceVisibility
    {
        get => _interlaceVisibility;
        private set => SetProperty(ref _interlaceVisibility, value);
    }

    /// <summary>위치 인디케이터 텍스트(절/슬라이드 "N/M"). §7.3-A.</summary>
    public string PositionLabel
    {
        get => _positionLabel;
        private set => SetProperty(ref _positionLabel, value);
    }

    /// <summary>위치 인디케이터 표시 여부 — 설정 on + Live + 라벨 존재 시에만 Visible.</summary>
    public Visibility PositionIndicatorVisibility
    {
        get => _positionIndicatorVisibility;
        private set => SetProperty(ref _positionIndicatorVisibility, value);
    }

    /// <summary>절 헤딩 텍스트(현재 절의 섹션 라벨 "1절"/"후렴"). FrmMain Def_Head All.</summary>
    public string VerseHeadingText
    {
        get => _verseHeadingText;
        private set => SetProperty(ref _verseHeadingText, value);
    }

    /// <summary>절 헤딩 표시 여부 — 설정 on + Live + 섹션 라벨 존재 시에만 Visible.</summary>
    public Visibility VerseHeadingVisibility
    {
        get => _verseHeadingVisibility;
        private set => SetProperty(ref _verseHeadingVisibility, value);
    }

    /// <summary>곡 번호 텍스트(Display Panel "Show Item Number").</summary>
    public string ItemNumberText
    {
        get => _itemNumberText;
        private set => SetProperty(ref _itemNumberText, value);
    }

    /// <summary>곡 번호 표시 여부 — 설정 on + Live + 번호 존재 시에만 Visible.</summary>
    public Visibility ItemNumberVisibility
    {
        get => _itemNumberVisibility;
        private set => SetProperty(ref _itemNumberVisibility, value);
    }

    /// <summary>정보 패널 곡 제목 밴드 표시 여부(FrmMain Def_PanelTitle) — 설정 on + Live + 제목 있을 때만. 텍스트는 DisplayTitle 재사용.</summary>
    public Visibility TitleOnPanelVisibility
    {
        get => _titleOnPanelVisibility;
        private set => SetProperty(ref _titleOnPanelVisibility, value);
    }

    /// <summary>저작권 텍스트(Display Panel "Show Copyright Information").</summary>
    public string CopyrightText
    {
        get => _copyrightText;
        private set => SetProperty(ref _copyrightText, value);
    }

    /// <summary>저작권 표시 여부 — 설정 on + Live + 저작권 존재 시에만 Visible.</summary>
    public Visibility CopyrightVisibility
    {
        get => _copyrightVisibility;
        private set => SetProperty(ref _copyrightVisibility, value);
    }

    /// <summary>다음 항목 제목 텍스트(Display Panel PrevNext, "다음 ▶ ...").</summary>
    public string NextItemText
    {
        get => _nextItemText;
        private set => SetProperty(ref _nextItemText, value);
    }

    /// <summary>다음 항목 표시 여부 — 설정 on + Live + 다음 항목 존재 시에만 Visible.</summary>
    public Visibility NextItemVisibility
    {
        get => _nextItemVisibility;
        private set => SetProperty(ref _nextItemVisibility, value);
    }

    public bool IsBlackout
    {
        get => _isBlackout;
        private set => SetProperty(ref _isBlackout, value);
    }

    public bool IsOutputOpen
    {
        get => _isOutputOpen;
        private set => SetProperty(ref _isOutputOpen, value);
    }

    public OutputSceneSnapshot Scene
    {
        get => _scene;
        private set => SetProperty(ref _scene, value);
    }

    public Brush SceneForegroundBrush
    {
        get => _sceneForegroundBrush;
        private set => SetProperty(ref _sceneForegroundBrush, value);
    }

    public Brush SceneBackgroundBrush
    {
        get => _sceneBackgroundBrush;
        private set => SetProperty(ref _sceneBackgroundBrush, value);
    }

    /// <summary>
    /// 곡별 배경 이미지(FormatData 61). 있으면 색 배경 위(앞)에 표시해 이미지가 색을 가린다(이미지 우선). §7.3-A 배경.
    /// 없거나 로드 실패면 null → <see cref="BackgroundImageVisibility"/> 가 Collapsed 가 되어 색 배경만 보인다.
    /// </summary>
    public ImageSource? SceneBackgroundImageSource
    {
        get => _sceneBackgroundImageSource;
        private set => SetProperty(ref _sceneBackgroundImageSource, value);
    }

    /// <summary>
    /// 배경 이미지를 표시 모드(채움/맞춤/가운데/타일)에 맞춰 그리는 브러시. 이미지가 없으면 null(색 배경만 보임).
    /// 출력 창의 배경 Rectangle.Fill 이 이 브러시를 쓴다 — 모드별 Stretch·TileMode 가 브러시에 담겨 있다.
    /// </summary>
    public Brush? BackgroundImageBrush
    {
        get => _backgroundImageBrush;
        private set => SetProperty(ref _backgroundImageBrush, value);
    }

    /// <summary>
    /// Display Panel(곡번호·저작권·다음항목·위치 인디케이터) 밴드의 배경 브러시 — 기본은 반투명 검정(#66000000),
    /// "패널 투명"(Def_PanelTransparent) on 이면 Transparent. 4개 밴드가 모두 이 브러시를 쓴다.
    /// </summary>
    public Brush PanelBackgroundBrush
    {
        get => _panelBackgroundBrush;
        private set => SetProperty(ref _panelBackgroundBrush, value);
    }

    /// <summary>Display Panel 전용 글자색 — 기본은 Region1 글자색 추종, AsR1 off 면 패널 전용 색.</summary>
    public Brush PanelForegroundBrush
    {
        get => _panelForegroundBrush;
        private set => SetProperty(ref _panelForegroundBrush, value);
    }

    // Display Panel 정보 텍스트 기본 글자 크기(XAML 하드코딩 값과 동일) — 위치/곡번호=20, 저작권/다음=16.
    private const double PanelPrimaryBaseFontSize = 20.0;
    private const double PanelSecondaryBaseFontSize = 16.0;

    /// <summary>위치 인디케이터·곡 번호 글자 크기(기본 20 × 패널 비율). FrmMain Def_PanelFont 크기.</summary>
    public double PanelPrimaryFontSize
    {
        get => _panelPrimaryFontSize;
        private set => SetProperty(ref _panelPrimaryFontSize, value);
    }

    /// <summary>저작권·다음 항목 글자 크기(기본 16 × 패널 비율).</summary>
    public double PanelSecondaryFontSize
    {
        get => _panelSecondaryFontSize;
        private set => SetProperty(ref _panelSecondaryFontSize, value);
    }

    public FontWeight PanelPrimaryFontWeight
    {
        get => _panelPrimaryFontWeight;
        private set => SetProperty(ref _panelPrimaryFontWeight, value);
    }

    public FontWeight PanelSecondaryFontWeight
    {
        get => _panelSecondaryFontWeight;
        private set => SetProperty(ref _panelSecondaryFontWeight, value);
    }

    public FontStyle PanelFontStyle
    {
        get => _panelFontStyle;
        private set => SetProperty(ref _panelFontStyle, value);
    }

    public TextDecorationCollection? PanelTextDecorations
    {
        get => _panelTextDecorations;
        private set => SetProperty(ref _panelTextDecorations, value);
    }

    /// <summary>배경 이미지 표시 여부 — 곡별 배경 이미지가 로드됐을 때만 Visible(색 배경 위에 덮음).</summary>
    public Visibility BackgroundImageVisibility
    {
        get => _backgroundImageVisibility;
        private set => SetProperty(ref _backgroundImageVisibility, value);
    }

    public Visibility LyricsAlertVisibility
    {
        get => _lyricsAlertVisibility;
        private set => SetProperty(ref _lyricsAlertVisibility, value);
    }

    public string LyricsAlertText
    {
        get => _lyricsAlertText;
        private set => SetProperty(ref _lyricsAlertText, value);
    }

    public Brush LyricsAlertForegroundBrush
    {
        get => _lyricsAlertForegroundBrush;
        private set => SetProperty(ref _lyricsAlertForegroundBrush, value);
    }

    public Brush LyricsAlertBackgroundBrush
    {
        get => _lyricsAlertBackgroundBrush;
        private set => SetProperty(ref _lyricsAlertBackgroundBrush, value);
    }

    public Visibility ReferenceAlertVisibility
    {
        get => _referenceAlertVisibility;
        private set => SetProperty(ref _referenceAlertVisibility, value);
    }

    public string ReferenceAlertText
    {
        get => _referenceAlertText;
        private set => SetProperty(ref _referenceAlertText, value);
    }

    public Brush ReferenceAlertForegroundBrush
    {
        get => _referenceAlertForegroundBrush;
        private set => SetProperty(ref _referenceAlertForegroundBrush, value);
    }

    public Brush ReferenceAlertBackgroundBrush
    {
        get => _referenceAlertBackgroundBrush;
        private set => SetProperty(ref _referenceAlertBackgroundBrush, value);
    }

    public double ReferenceAlertHorizontalOffset
    {
        get => _referenceAlertHorizontalOffset;
        private set => SetProperty(ref _referenceAlertHorizontalOffset, value);
    }

    public Visibility PanelOverlayVisibility
    {
        get => _panelOverlayVisibility;
        private set => SetProperty(ref _panelOverlayVisibility, value);
    }

    // GAP 모드(User)에서 로고 이미지가 표시 중이면 타이틀 텍스트를 숨겨야 하므로
    // 별도 프로퍼티로 노출. 일반 상황에서는 PanelOverlayVisibility와 동일하게 동작.
    public Visibility DisplayTitleVisibility
    {
        get => _displayTitleVisibility;
        private set => SetProperty(ref _displayTitleVisibility, value);
    }

    // 제목 헤딩(가사 위 상단 배너) 가시성 — 설정 on + 가사 본문 송출 중일 때만 Visible(§7.3-A).
    // 중앙 DisplayTitle 과 별개의 상단 TextBlock 으로, 본문과 겹치지 않게 곡 제목을 보여준다.
    public Visibility TitleHeadingVisibility
    {
        get => _titleHeadingVisibility;
        private set => SetProperty(ref _titleHeadingVisibility, value);
    }

    /// <summary>제목 헤딩 줄 정렬(좌/중/우) — 인-셸 헤딩 정렬 설정에서 유래(§7.3-A).</summary>
    public TextAlignment TitleHeadingTextAlignment
    {
        get => _titleHeadingTextAlignment;
        private set => SetProperty(ref _titleHeadingTextAlignment, value);
    }

    /// <summary>제목 헤딩 가로 배치(좌/중/우) — 헤딩 배너의 화면 내 위치(§7.3-A).</summary>
    public HorizontalAlignment TitleHeadingHorizontalAlignment
    {
        get => _titleHeadingHorizontalAlignment;
        private set => SetProperty(ref _titleHeadingHorizontalAlignment, value);
    }

    // 본문 TextBlock 의 Margin — 제목 헤딩이 보일 때만 상단 여백을 확보해 헤딩과 겹치지 않게 한다(§7.3-A).
    public Thickness BodyContentMargin
    {
        get => _bodyContentMargin;
        private set => SetProperty(ref _bodyContentMargin, value);
    }

    /// <summary>Region2 본문 블록의 위쪽 여백 — Region1↔Region2 세로 간격(FrmMain Ind_Reg2TopUpDown). 기본 8px.</summary>
    public Thickness BodyText2Margin
    {
        get => _bodyText2Margin;
        private set => SetProperty(ref _bodyText2Margin, value);
    }

    /// <summary>본문 묶음 세로 위치 오프셋(px, 음수=위) — FrmMain Ind_Reg1TopUpDown. TranslateTransform.Y 에 바인딩. 기본 0.</summary>
    public double BodyVerticalOffset
    {
        get => _bodyVerticalOffset;
        private set => SetProperty(ref _bodyVerticalOffset, value);
    }

    public Visibility GapLogoVisibility
    {
        get => _gapLogoVisibility;
        private set => SetProperty(ref _gapLogoVisibility, value);
    }

    public ImageSource? GapLogoSource
    {
        get => _gapLogoSource;
        private set => SetProperty(ref _gapLogoSource, value);
    }

    // Blackout/Hidden 상태에서 송출 화면을 검정으로 강제 덮는 오버레이.
    // 사용자가 설정한 배경 브러시(LyricsMonitorBackgroundColorArgb)가 새지 않도록 보장한다.
    public Visibility BlackoutOverlayVisibility
    {
        get => _blackoutOverlayVisibility;
        private set => SetProperty(ref _blackoutOverlayVisibility, value);
    }

    // 컨텐츠 영역(타이틀/로고) 진입 시 페이드 인 애니메이션 길이.
    // 0이면 페이드 비활성 — 테스트에서는 TimeSpan.Zero로 설정해 즉시 1로 만든다.
    public TimeSpan ContentFadeDuration
    {
        get => _contentFadeDuration;
        set => SetProperty(ref _contentFadeDuration, value);
    }

    // 장면 전환 모션 종류(Fade/Slide 4방향). 코드-비하인드가 SceneChanged 시 이 값으로 애니메이션을 고른다.
    public LyricsTransitionKind ContentTransitionKind
    {
        get => _contentTransitionKind;
        set => SetProperty(ref _contentTransitionKind, value);
    }

    // 장면이 "바뀌기 직전" 알림 — 새 콘텐츠가 적용되기 전에 발화한다. 2-레이어 전환에서 뷰가 옛 프레임을
    // 스냅샷해 뒤 레이어로 깔 수 있게 한다(오목 도형 전환 등). 단일 레이어 전환은 이 이벤트를 무시한다.
    public event EventHandler? SceneChanging;

    // Scene 변경 알림을 받아 페이드를 트리거할 코드-비하인드/뷰가 구독한다.
    // ViewModel 자체는 WPF 애니메이션을 모르고, 단순히 "장면이 갱신됐다"는 사실만 전달.
    public event EventHandler? SceneChanged;

    // 외부에서 주입된 콘텐츠 이미지(슬라이드 썸네일 등). null이면 Canvas Image가 숨겨진다.
    public ImageSource? ContentImageSource
    {
        get => _contentImageSource;
        private set => SetProperty(ref _contentImageSource, value);
    }

    public Visibility ContentVisibility
    {
        get => _contentVisibility;
        private set => SetProperty(ref _contentVisibility, value);
    }

    // 콘텐츠 이미지의 화면 좌표·크기. Scene.ContentPlacement에서 계산된 값.
    public double ContentLeft
    {
        get => _contentLeft;
        private set => SetProperty(ref _contentLeft, value);
    }

    public double ContentTop
    {
        get => _contentTop;
        private set => SetProperty(ref _contentTop, value);
    }

    public double ContentWidth
    {
        get => _contentWidth;
        private set => SetProperty(ref _contentWidth, value);
    }

    public double ContentHeight
    {
        get => _contentHeight;
        private set => SetProperty(ref _contentHeight, value);
    }

    // 외부 호출자(상위 컨트롤러)가 송출할 이미지 자산을 주입한다.
    // source가 null이거나 픽셀 크기가 0이면 화면에서 사라진다.
    // FillMode 변경은 OutputRenderRequest 재계산을 통해 새 ContentPlacement를 만들어 낸다.
    public void SetContentAsset(
        ImageSource? source,
        int pixelWidth,
        int pixelHeight,
        ImageFillMode fillMode = ImageFillMode.Fit)
    {
        AssignContentAsset(source, pixelWidth, pixelHeight, fillMode);
        RefreshDisplayText();
    }

    // RefreshDisplayText를 호출하지 않는 내부용 — ApplySession처럼 본인이 직접 Refresh를 부르는
    // 경로에서 중복 호출을 피하기 위해 분리.
    private void AssignContentAsset(
        ImageSource? source,
        int pixelWidth,
        int pixelHeight,
        ImageFillMode fillMode)
    {
        ContentImageSource = source;
        _contentPixelWidth = pixelWidth;
        _contentPixelHeight = pixelHeight;
        _contentFillMode = fillMode;
    }

    public void ApplySession(LiveSessionSnapshot snapshot)
    {
        // 새 콘텐츠를 적용하기 전에 전환 효과 종류부터 정한다 — 항목이 바뀌면(곡→곡) 항목 전환,
        // 같은 항목 안에서 절·슬라이드만 바뀌면 슬라이드 전환(FrmMain 항목 vs 슬라이드 전환 분리).
        UpdateContentTransitionKindForChange(snapshot);
        _session = snapshot;
        State = snapshot.State;
        CurrentItemTitle = snapshot.CurrentItemTitle;
        // 라이브 큐 항목이 들고 있던 미리보기 자산을 그대로 송출 콘텐츠 슬롯에 주입.
        // Stop/Hidden 등으로 snapshot의 PreviewSource가 null이면 자동으로 콘텐츠가 사라진다.
        AssignContentAsset(
            snapshot.CurrentItemPreviewSource,
            snapshot.CurrentItemPreviewPixelWidth,
            snapshot.CurrentItemPreviewPixelHeight,
            snapshot.CurrentItemPreviewFillMode);
        RefreshDisplayText();
    }

    public void ApplyOutput(OutputWindowState state)
    {
        _output = state;
        RefreshDisplayText();
    }

    // 이번 송출이 "다른 항목으로의 전환"인지(곡→곡) "같은 항목 안 절·슬라이드 이동"인지 판별해
    // ContentTransitionKind 를 항목 전환/슬라이드 전환 설정 중 알맞은 것으로 정한다. 같은 곡의 절 이동은
    // 같은 Id 로 재송출되므로 itemChanged=false → 슬라이드 전환을, 다음 곡은 다른 Id → 항목 전환을 쓴다.
    private void UpdateContentTransitionKindForChange(LiveSessionSnapshot snapshot)
    {
        if (_settings is null)
        {
            return;
        }

        var newId = snapshot.CurrentItemId;
        // 빈 Id(비-라이브·정지)는 항목 전환으로 치지 않는다 — 콘텐츠가 사라지는 경우라 슬라이드 전환으로 둔다.
        var itemChanged = !string.IsNullOrEmpty(newId)
            && !string.Equals(newId, _previousLiveItemId, StringComparison.Ordinal);

        ContentTransitionKind = itemChanged
            ? _settings.Get(EasiSettingKeys.LyricsMonitorTransitionKind)
            : _settings.Get(EasiSettingKeys.LyricsMonitorSlideTransitionKind);

        _previousLiveItemId = newId;
    }

    private void RefreshDisplayText()
    {
        // 새 콘텐츠를 적용하기 전에 알림 → 뷰가 옛 프레임을 스냅샷할 기회(2-레이어 전환). 단일 레이어는 무시.
        SceneChanging?.Invoke(this, EventArgs.Empty);
        Scene = CreateScene();
        ApplyScene(Scene);
        // 모든 프로퍼티가 갱신된 뒤에 알림 → 뷰(코드-비하인드)가 fade-in 등 진입 애니메이션을 트리거할 수 있게 한다.
        SceneChanged?.Invoke(this, EventArgs.Empty);
    }

    private OutputSceneSnapshot CreateScene()
        => _renderer.CreateScene(new OutputRenderRequest(
            _session,
            _output,
            GetViewportWidth(_output),
            GetViewportHeight(_output),
            ContentPixelWidth: _contentPixelWidth,
            ContentPixelHeight: _contentPixelHeight,
            FillMode: _contentFillMode,
            LiveOutputSettings: GetLiveOutputSettings()));

    private void ApplyScene(OutputSceneSnapshot scene)
    {
        IsOutputOpen = scene.IsOutputOpen;
        IsBlackout = scene.IsBlackout;
        OutputMonitorName = scene.OutputMonitorName;
        DisplayTitle = scene.DisplayTitle;
        StatusLabel = scene.StatusLabel;
        SceneForegroundBrush = CreateBrush(scene.LyricsMonitorTextColorArgb);
        SceneForegroundBrush2 = CreateBrush(scene.LyricsMonitorTextColor2Argb);
        SceneBackgroundBrush = CreateBackgroundBrush(
            scene.LyricsMonitorBackgroundColorArgb,
            scene.LyricsMonitorBackgroundColor2Argb,
            scene.LyricsMonitorBackgroundIsGradient,
            scene.BackgroundGradientDirection);
        // Display Panel 밴드 배경 — "패널 투명" on 이면 Transparent, off(기본)면 반투명 검정(무회귀).
        // Display Panel 밴드 배경 — "패널 투명" on 이면 완전 투명, off(기본)면 설정한 패널 색(기본 반투명 검정 = 기존 동작).
        PanelBackgroundBrush = scene.LyricsMonitorPanelTransparent ? Brushes.Transparent : CreateBrush(scene.LyricsMonitorPanelColorArgb);
        // Display Panel 정보 텍스트 글자 크기 — 기본 크기(주 20 / 보조 16)에 설정 비율(%)을 곱한다. 100%=기존 크기(무회귀).
        var panelScale = scene.LyricsMonitorPanelFontScalePercent / 100.0;
        PanelPrimaryFontSize = PanelPrimaryBaseFontSize * panelScale;   // 위치 인디케이터·곡 번호
        PanelSecondaryFontSize = PanelSecondaryBaseFontSize * panelScale; // 저작권·다음 항목
        PanelForegroundBrush = CreateBrush(scene.LyricsMonitorPanelTextColorArgb);
        PanelPrimaryFontWeight = scene.LyricsMonitorPanelBold ? FontWeights.Bold : FontWeights.SemiBold;
        PanelSecondaryFontWeight = scene.LyricsMonitorPanelBold ? FontWeights.Bold : FontWeights.Normal;
        PanelFontStyle = scene.LyricsMonitorPanelItalic ? FontStyles.Italic : FontStyles.Normal;
        PanelTextDecorations = scene.LyricsMonitorPanelUnderline ? TextDecorations.Underline : null;
        // 곡별 배경 이미지(있으면) 로드 — 색 배경 위에 표시(이미지 우선). 없거나 실패면 색 배경만 보인다.
        ApplyBackgroundImage(scene);
        LyricsAlertVisibility = scene.ShowsLyricsAlertBox || scene.ShowsLiveLyricsAlertMessage
            ? Visibility.Visible
            : Visibility.Collapsed;
        LyricsAlertText = scene.ShowsLiveLyricsAlertMessage ? scene.LyricsAlertMessage : scene.StatusLabel;
        UpdateLyricsAlertBrushes(scene);
        ReferenceAlertVisibility = scene.ShowsLiveReferenceAlert ? Visibility.Visible : Visibility.Collapsed;
        ReferenceAlertText = scene.ShowsLiveReferenceAlert ? scene.ReferenceAlertText : string.Empty;
        UpdateReferenceAlertBrushes(scene);
        // "코드 표시"(Show Notations)는 더 이상 렌더 계층의 가시성이 아니라 본문 텍스트 자체(코드 줄 포함 여부)로
        // 반영된다 — 라이브 본문 계산(LiveSessionService.ComputeBodyText)이 처리하고, 설정 변경 시 MainViewModel 이
        // 라이브 곡을 재송출한다. (과거엔 이 값이 상태 라벨 가시성에 잘못 묶여 있던 결함을 제거.)
        // 곡 가사 본문을 송출 슬롯에 반영. 본문이 보이면 타이틀과 겹치므로 ApplyGapLogo 에서 타이틀을 숨긴다.
        BodyText = scene.BodyText;
        BodyText2 = scene.BodyText2;
        BodyText2Visibility = scene.ShowsBodyText2 ? Visibility.Visible : Visibility.Collapsed;
        BodyText2Alignment = ToTextAlignment(scene.LyricsMonitorTextAlignment2);
        // Region2 글꼴명: 있으면 FontFamily, 없으면 UnsetValue → XAML 이 Region1/테마 글꼴을 상속.
        BodyFont2Family = string.IsNullOrWhiteSpace(scene.LyricsMonitorFontFamily2)
            ? DependencyProperty.UnsetValue
            : new FontFamily(scene.LyricsMonitorFontFamily2);
        BodyFont2Size = scene.LyricsMonitorFontSize2;
        // Region1↔Region2 세로 간격(FrmMain Ind_Reg2TopUpDown) — Region2 블록 위쪽 여백으로 적용. 기본 8=기존(무회귀).
        BodyText2Margin = new Thickness(0, scene.LyricsMonitorRegionGapPx, 0, 0);
        // 본문 세로 위치 오프셋(FrmMain Ind_Reg1TopUpDown) — TranslateTransform.Y 로 본문 묶음을 위(-)/아래(+)로 옮긴다. 기본 0=무이동.
        BodyVerticalOffset = scene.LyricsMonitorBodyVerticalOffset;
        BodyTextAlignment = ToTextAlignment(scene.LyricsMonitorTextAlignment);
        BodyHorizontalAlignment = ToHorizontalAlignment(scene.LyricsMonitorTextAlignment);
        BodyVerticalAlignment = ToVerticalAlignment(scene.LyricsMonitorVerticalAlignment);
        BodyFontSize = scene.LyricsMonitorFontSize;
        // 곡별 글꼴명 오버라이드: 있으면 FontFamily, 없으면 UnsetValue → XAML 이 테마 기본 글꼴을 상속(무회귀).
        BodyFontFamily = string.IsNullOrWhiteSpace(scene.LyricsMonitorFontFamily)
            ? DependencyProperty.UnsetValue
            : new FontFamily(scene.LyricsMonitorFontFamily);
        // 줄 높이 = 폰트 크기 × 줄 간격 비율(설정 %). 기본 125% → 폰트×1.25(기존 동작 보존).
        BodyLineHeight = scene.LyricsMonitorFontSize * (scene.LyricsMonitorLineSpacingPercent / 100.0);
        // 폰트 효과: 굵게 off 는 기존 SemiBold 를 유지(완전 Normal 로 떨어뜨리지 않음).
        BodyFontWeight = scene.LyricsMonitorBold ? FontWeights.Bold : FontWeights.SemiBold;
        BodyFontStyle = scene.LyricsMonitorItalic ? FontStyles.Italic : FontStyles.Normal;
        // Region2(이중 언어 보조) 굵게/기울임 — 씬이 영역별로 해석한 값(없으면 Region1 추종)을 적용.
        BodyFont2Weight = scene.LyricsMonitorBold2 ? FontWeights.Bold : FontWeights.SemiBold;
        BodyFont2Style = scene.LyricsMonitorItalic2 ? FontStyles.Italic : FontStyles.Normal;
        BodyHasShadow = scene.LyricsMonitorShadow;
        // 밑줄 — on 이면 Underline 장식, off 면 null(무회귀). Region1/2 를 영역별로 해석한 값(곡별 비트∥전역∥Region1 추종).
        BodyTextDecorations = scene.LyricsMonitorUnderline ? TextDecorations.Underline : null;
        BodyText2Decorations = scene.LyricsMonitorUnderline2 ? TextDecorations.Underline : null;
        // 외곽선 모드 컨트롤은 TextDecorations 대신 bool 을 받는다(Region1 밑줄 값).
        BodyUnderline = scene.LyricsMonitorUnderline;
        PositionLabel = scene.PositionLabel;
        PositionIndicatorVisibility = scene.ShowsPositionIndicator ? Visibility.Visible : Visibility.Collapsed;
        VerseHeadingText = scene.VerseHeadingLabel;
        VerseHeadingVisibility = scene.ShowsVerseHeading ? Visibility.Visible : Visibility.Collapsed;
        ItemNumberText = scene.ItemNumberLabel;
        ItemNumberVisibility = scene.ShowsItemNumber ? Visibility.Visible : Visibility.Collapsed;
        TitleOnPanelVisibility = scene.ShowsTitleOnPanel ? Visibility.Visible : Visibility.Collapsed;
        CopyrightText = scene.CopyrightLabel;
        CopyrightVisibility = scene.ShowsCopyright ? Visibility.Visible : Visibility.Collapsed;
        NextItemText = scene.ShowsNextItem ? $"다음 ▶ {scene.NextItemLabel}" : string.Empty;
        NextItemVisibility = scene.ShowsNextItem ? Visibility.Visible : Visibility.Collapsed;
        // 제목 헤딩: 설정 on + 가사 본문 송출 중일 때만 상단 배너로 곡 제목 노출(§7.3-A).
        TitleHeadingVisibility = scene.ShowsTitleHeading ? Visibility.Visible : Visibility.Collapsed;
        // 제목 헤딩 가로 정렬 — 본문 정렬과 동일 enum·헬퍼 재사용(§7.3-A Heading Align).
        TitleHeadingTextAlignment = ToTextAlignment(scene.LyricsMonitorTitleHeadingAlignment);
        TitleHeadingHorizontalAlignment = ToHorizontalAlignment(scene.LyricsMonitorTitleHeadingAlignment);
        // 헤딩이 보이면 본문 위에 배너 높이만큼 여백을 확보 — 본문 세로정렬이 "위"여도 겹치지 않게 한다.
        // 헤딩은 1줄(NoWrap)로 높이가 결정적이라 고정 여백으로 충분하다.
        // 본문 묶음 여백 = 사용자 설정 좌/우/아래 여백 + (헤딩이 보이면) 헤딩 높이만큼 상단 확보.
        // 좌/우/아래는 FrmMain ShowLeftMargin/Right/Bottom 대응 — 가장자리에서 안쪽으로 들여써 잘림·치우침을 보정한다.
        // 기본값 0/0/0 이면 헤딩 유무에 따라 기존 Thickness(0,heading,0,0)/Thickness(0) 과 완전히 동일(무회귀).
        var headingTop = scene.ShowsTitleHeading ? TitleHeadingReservedHeight : 0;
        BodyContentMargin = new Thickness(
            scene.LyricsMonitorBodyLeftMargin,
            headingTop,
            scene.LyricsMonitorBodyRightMargin,
            scene.LyricsMonitorBodyBottomMargin);
        var bodyShown = scene.ShowsBodyText;
        // 외곽선 효과(§7.3-A): on 이면 외곽선 렌더러만, off 면 일반 본문만 보이게 상호배타로 전환(겹침 방지).
        // 외곽선 기본 off 라 기존 테스트/동작에선 BodyTextVisibility=bodyShown 그대로다.
        BodyHasOutline = scene.UsesBodyOutline;
        BodyTextVisibility = (bodyShown && !scene.UsesBodyOutline) ? Visibility.Visible : Visibility.Collapsed;
        BodyOutlineVisibility = (bodyShown && scene.UsesBodyOutline) ? Visibility.Visible : Visibility.Collapsed;
        // 줄 교차(인터레이스): on + 두 영역이 다 보이면 줄을 번갈아 쌓고 기존 두 밴드를 숨긴다(아래에서 가시성 재정의).
        RebuildInterlacedLines(scene);
        var panelOverlay = scene.ShowsPanelOverlay ? Visibility.Visible : Visibility.Collapsed;
        PanelOverlayVisibility = panelOverlay;
        ApplyGapLogo(scene, panelOverlay, bodyShown);
        BlackoutOverlayVisibility = IsBlackoutOrHidden(scene.Kind) ? Visibility.Visible : Visibility.Collapsed;
        ApplyContentPlacement(scene);
    }

    // 줄 교차(인터레이스) 줄 목록을 다시 만든다 — Region1·Region2 본문을 줄 단위로 번갈아 쌓는다.
    // 각 줄은 자기 영역의 색·글꼴·크기·굵게·기울임·밑줄·정렬을 그대로 지닌다(ApplyScene 이 이미 계산한 영역별 VM 값 사용).
    // 인터레이스 중엔 기존 두 밴드(Region1 일반/외곽선·Region2)를 숨겨 교차 목록만 보이게 한다.
    private void RebuildInterlacedLines(OutputSceneSnapshot scene)
    {
        InterlacedLines.Clear();
        InterlaceVisibility = scene.ShowsInterlace ? Visibility.Visible : Visibility.Collapsed;
        if (!scene.ShowsInterlace)
        {
            return;
        }

        var region1Lines = BodyText.Split('\n');
        var region2Lines = BodyText2.Split('\n');
        var lineCount = Math.Max(region1Lines.Length, region2Lines.Length);
        for (var i = 0; i < lineCount; i++)
        {
            if (i < region1Lines.Length)
            {
                InterlacedLines.Add(new InterlacedLine(region1Lines[i], SceneForegroundBrush, BodyFontFamily, BodyFontSize,
                    BodyFontWeight, BodyFontStyle, BodyTextDecorations, BodyTextAlignment, BodyLineHeight));
            }

            if (i < region2Lines.Length)
            {
                InterlacedLines.Add(new InterlacedLine(region2Lines[i], SceneForegroundBrush2, BodyFont2Family, BodyFont2Size,
                    BodyFont2Weight, BodyFont2Style, BodyText2Decorations, BodyText2Alignment, BodyLineHeight));
            }
        }

        // 교차 목록이 본문을 담당하므로 기존 밴드는 모두 숨긴다.
        BodyTextVisibility = Visibility.Collapsed;
        BodyOutlineVisibility = Visibility.Collapsed;
        BodyText2Visibility = Visibility.Collapsed;
    }

    // Scene.ContentPlacement(픽셀 좌표·크기)를 XAML 바인딩에 노출.
    // Live 상태 + 콘텐츠 이미지 + 유효한 Placement가 모두 만족할 때만 ContentVisibility=Visible.
    private void ApplyContentPlacement(OutputSceneSnapshot scene)
    {
        var placement = scene.ContentPlacement;
        ContentLeft = placement.Left;
        ContentTop = placement.Top;
        ContentWidth = placement.Width;
        ContentHeight = placement.Height;

        // 가사 본문이 송출 중이면 콘텐츠 이미지는 숨긴다 — 둘이 겹쳐 가사가 읽기 어려워지는 것을 막는다(본문 우선).
        // 현재 곡은 이미지가 없어 상호배타적이지만, 미래에 한 항목이 본문+이미지를 모두 가질 때를 위한 명시적 가드.
        var hasContent = _contentImageSource is not null
            && scene.ShowsContent
            && !scene.ShowsBodyText
            && placement.Width > 0
            && placement.Height > 0;
        ContentVisibility = hasContent ? Visibility.Visible : Visibility.Collapsed;
    }

    // Blackout(완전 차단) / Hidden(화면 끄기) 모두 송출 화면은 검정으로 덮어야 한다.
    // 두 상태를 시각적으로 구분하는 것은 운영자 패널의 역할이고, 송출 화면은 둘 다 깨끗한 검정이 안전.
    // 단 Cleared(비우기)는 의도적으로 제외 — 배경을 그대로 보여야 하므로 검정 오버레이를 씌우지 않는다.
    private static bool IsBlackoutOrHidden(OutputSceneKind kind)
        => kind == OutputSceneKind.Blackout || kind == OutputSceneKind.Hidden;

    // GAP 모드(User)에서 로고 파일이 로딩 가능하면 이미지를, 그 외에는 기존 타이틀 텍스트를 표시한다.
    // 로고나 가사 본문이 보이는 동안에는 타이틀 텍스트와 시각적으로 겹치지 않도록 DisplayTitleVisibility를 Collapsed로 둔다.
    private void ApplyGapLogo(OutputSceneSnapshot scene, Visibility panelOverlay, bool bodyShown)
    {
        var logo = TryLoadGapLogo(scene);
        GapLogoSource = logo;
        // 패널 오버레이가 숨겨진 상태(예: PPT 라이브)면 로고도 굳이 노출하지 않는다.
        var gapLogoVisible = logo is not null && panelOverlay == Visibility.Visible;
        GapLogoVisibility = gapLogoVisible ? Visibility.Visible : Visibility.Collapsed;
        // 가사 본문이 송출 중이면(곡 라이브) 타이틀은 숨겨 본문만 보이게 한다.
        DisplayTitleVisibility = (gapLogoVisible || bodyShown) ? Visibility.Collapsed : panelOverlay;
    }

    // 곡별 배경 이미지를 로드해 출력에 반영한다. 경로가 비었으면(비-Live·오버라이드 없음) 색 배경만 보인다.
    // 상대 경로는 작업 폴더 기준으로 해석하고, 로드 실패(파일 없음/디코드 실패)면 null → 색 배경으로 안전 폴백.
    private void ApplyBackgroundImage(OutputSceneSnapshot scene)
    {
        var image = TryLoadBackgroundImage(scene.BackgroundImagePath);
        SceneBackgroundImageSource = image;
        // 이미지가 있으면 표시 모드(채움/맞춤/가운데/타일)에 맞춘 브러시를 만들어 배경 Rectangle 이 칠한다.
        BackgroundImageBrush = image is null ? null : BuildBackgroundImageBrush(image, scene.BackgroundMode);
        BackgroundImageVisibility = image is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// 배경 이미지를 표시 모드에 맞춰 그리는 <see cref="ImageBrush"/> 를 만든다(레거시 Def_ImageMode 대응).
    ///   - Fill: 화면 가득(비율 유지·가장자리 크롭) = UniformToFill. 기존 동작(무회귀).
    ///   - Fit: 이미지 전체가 보이게(레터박스) = Uniform.
    ///   - Center: 원본 크기로 가운데 = Stretch 없음.
    ///   - Tile: 원본 크기로 바둑판 반복 = TileMode.Tile + 이미지 픽셀 크기 뷰포트.
    /// 만든 브러시는 Freeze 해 렌더 스레드 공유·성능에 안전하게 한다.
    /// </summary>
    internal static ImageBrush BuildBackgroundImageBrush(ImageSource image, Settings.LyricsBackgroundMode mode)
    {
        var brush = new ImageBrush(image);
        switch (mode)
        {
            case Settings.LyricsBackgroundMode.Fit:
                brush.Stretch = Stretch.Uniform;
                break;
            case Settings.LyricsBackgroundMode.Center:
                brush.Stretch = Stretch.None;
                brush.AlignmentX = AlignmentX.Center;
                brush.AlignmentY = AlignmentY.Center;
                break;
            case Settings.LyricsBackgroundMode.Tile:
                // 원본 픽셀 크기로 바둑판 반복. 절대 단위 뷰포트(이미지 크기)에 TileMode.Tile.
                // 비-96dpi 이미지에서도 픽셀 단위로 반복되도록 PixelWidth/Height 를 우선 쓴다(없으면 DIP).
                brush.Stretch = Stretch.None;
                brush.TileMode = TileMode.Tile;
                brush.ViewportUnits = BrushMappingMode.Absolute;
                var tileWidth = image is BitmapSource tileBmp ? tileBmp.PixelWidth : image.Width;
                var tileHeight = image is BitmapSource tileBmp2 ? tileBmp2.PixelHeight : image.Height;
                brush.Viewport = new Rect(0, 0, tileWidth, tileHeight);
                brush.AlignmentX = AlignmentX.Left;
                brush.AlignmentY = AlignmentY.Top;
                break;
            default: // Fill — 기존 동작(화면 가득, 가장자리 크롭).
                brush.Stretch = Stretch.UniformToFill;
                break;
        }

        if (brush.CanFreeze)
        {
            brush.Freeze();
        }

        return brush;
    }

    private ImageSource? TryLoadBackgroundImage(string backgroundImagePath)
    {
        if (string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            _cachedBackgroundImagePath = null;
            _cachedBackgroundImageSource = null;
            return null;
        }

        var resolved = ResolveBackgroundImagePath(backgroundImagePath);
        if (string.Equals(_cachedBackgroundImagePath, resolved, StringComparison.OrdinalIgnoreCase)
            && _cachedBackgroundImageSource is not null)
        {
            return _cachedBackgroundImageSource;
        }

        // GAP 로고와 동일한 이미지 디코더(_gapLogoLoader)를 재사용 — 경로→ImageSource 변환은 동일하다.
        var loaded = _gapLogoLoader(resolved);
        _cachedBackgroundImagePath = resolved;
        _cachedBackgroundImageSource = loaded;
        return loaded;
    }

    // 배경 이미지 경로 해석 — 절대 경로면 그대로, 상대 경로면 작업 폴더 기준으로 합친다.
    // 작업 폴더를 모르면(설정 없음) 입력 경로를 그대로 쓴다(로더가 존재 여부를 최종 판단).
    private string ResolveBackgroundImagePath(string rawPath)
    {
        if (Path.IsPathRooted(rawPath))
        {
            return rawPath;
        }

        var workingFolder = _settings?.Current.General.WorkingFolder;
        return string.IsNullOrWhiteSpace(workingFolder)
            ? rawPath
            : Path.Combine(workingFolder, rawPath);
    }

    private ImageSource? TryLoadGapLogo(OutputSceneSnapshot scene)
    {
        // 로고가 의미 있는 시점은 Ready(GAP) 상태 + User 모드 + 경로 존재.
        if (scene.Kind != OutputSceneKind.Ready
            || scene.GapItemOption != GapItemMode.User
            || string.IsNullOrWhiteSpace(scene.GapItemLogoFile))
        {
            _cachedGapLogoPath = null;
            _cachedGapLogoSource = null;
            return null;
        }

        if (string.Equals(_cachedGapLogoPath, scene.GapItemLogoFile, StringComparison.OrdinalIgnoreCase)
            && _cachedGapLogoSource is not null)
        {
            return _cachedGapLogoSource;
        }

        var loaded = _gapLogoLoader(scene.GapItemLogoFile);
        _cachedGapLogoPath = scene.GapItemLogoFile;
        _cachedGapLogoSource = loaded;
        return loaded;
    }

    private void UpdateLyricsAlertBrushes(OutputSceneSnapshot scene)
    {
        if (!scene.ShowsLiveLyricsAlertMessage)
        {
            _lyricsAlertFlashTimer.Stop();
            _lastShowsLiveLyricsAlertMessage = false;
            _lastLyricsAlertMessage = string.Empty;
            LyricsAlertForegroundBrush = CreateBrush(unchecked((int)0xFFFFFFFF));
            LyricsAlertBackgroundBrush = CreateBrush(unchecked((int)0xFF008000));
            return;
        }

        var messageChanged = !_lastShowsLiveLyricsAlertMessage
            || !string.Equals(_lastLyricsAlertMessage, scene.LyricsAlertMessage, StringComparison.Ordinal);

        _lastShowsLiveLyricsAlertMessage = true;
        _lastLyricsAlertMessage = scene.LyricsAlertMessage;

        if (messageChanged)
        {
            ResetLyricsAlertFlash();
            _lyricsAlertFlashTimer.Start();
            return;
        }

        ApplyLyricsAlertFlashBrushes();
    }

    private void ResetLyricsAlertFlash()
    {
        _lyricsAlertFlashCount = 0;
        _lyricsAlertFlashAlternate = false;
        ApplyLyricsAlertFlashBrushes();
    }

    private void AdvanceLyricsAlertFlash()
    {
        if (!_lastShowsLiveLyricsAlertMessage)
        {
            _lyricsAlertFlashTimer.Stop();
            return;
        }

        _lyricsAlertFlashAlternate = !_lyricsAlertFlashAlternate;
        ApplyLyricsAlertFlashBrushes();
        _lyricsAlertFlashCount++;

        if (_lyricsAlertFlashCount > 3)
        {
            _lyricsAlertFlashCount = 0;
            _lyricsAlertFlashTimer.Stop();
        }
    }

    internal void AdvanceLyricsAlertFlashForTest() => AdvanceLyricsAlertFlash();

    private void ApplyLyricsAlertFlashBrushes()
    {
        LyricsAlertForegroundBrush = CreateBrush(_lyricsAlertFlashAlternate
            ? GetIntSetting(EasiSettingKeys.LyricsMonitorTextColorArgb)
            : GetIntSetting(EasiSettingKeys.LyricsMonitorHighlightColorArgb));
        LyricsAlertBackgroundBrush = CreateBrush(_lyricsAlertFlashAlternate
            ? unchecked((int)0xFFFF0000)
            : GetIntSetting(EasiSettingKeys.LyricsMonitorBackgroundColorArgb));
    }

    private void UpdateReferenceAlertBrushes(OutputSceneSnapshot scene)
    {
        if (!scene.ShowsLiveReferenceAlert)
        {
            _referenceAlertFlashTimer.Stop();
            StopReferenceAlertScroll();
            _lastShowsLiveReferenceAlert = false;
            _lastReferenceAlertText = string.Empty;
            _referenceAlertFlashAlternate = false;
            _referenceAlertFlashCount = 0;
            ApplyReferenceAlertBrushes(scene);
            return;
        }

        var alertChanged = !_lastShowsLiveReferenceAlert
            || !string.Equals(_lastReferenceAlertText, scene.ReferenceAlertText, StringComparison.Ordinal);

        _lastShowsLiveReferenceAlert = true;
        _lastReferenceAlertText = scene.ReferenceAlertText;
        UpdateReferenceAlertScroll(scene, alertChanged);

        if (!scene.ReferenceAlertFlash)
        {
            _referenceAlertFlashTimer.Stop();
            _referenceAlertFlashAlternate = false;
            _referenceAlertFlashCount = 0;
            ApplyReferenceAlertBrushes(scene);
            return;
        }

        if (alertChanged)
        {
            ResetReferenceAlertFlash(scene);
            _referenceAlertFlashTimer.Start();
            return;
        }

        ApplyReferenceAlertBrushes(scene);
    }

    private void ResetReferenceAlertFlash(OutputSceneSnapshot scene)
    {
        _referenceAlertFlashCount = 0;
        _referenceAlertFlashAlternate = false;
        ApplyReferenceAlertBrushes(scene);
    }

    private void AdvanceReferenceAlertFlash()
    {
        if (!_lastShowsLiveReferenceAlert)
        {
            _referenceAlertFlashTimer.Stop();
            return;
        }

        _referenceAlertFlashAlternate = !_referenceAlertFlashAlternate;
        ApplyReferenceAlertBrushes(Scene);
        _referenceAlertFlashCount++;

        if (_referenceAlertFlashCount > 3)
        {
            _referenceAlertFlashCount = 0;
            _referenceAlertFlashTimer.Stop();
        }
    }

    internal void AdvanceReferenceAlertFlashForTest() => AdvanceReferenceAlertFlash();

    private void UpdateReferenceAlertScroll(OutputSceneSnapshot scene, bool alertChanged)
    {
        if (!scene.ReferenceAlertScroll)
        {
            StopReferenceAlertScroll();
            return;
        }

        if (alertChanged)
        {
            ResetReferenceAlertScroll(scene.ReferenceAlertText);
        }
    }

    private void ResetReferenceAlertScroll(string text)
    {
        _referenceAlertScrollTick = 0;
        _referenceAlertScrollTotalTicks = ComputeReferenceAlertScrollTicks(text);
        ReferenceAlertHorizontalOffset = 180;
        _referenceAlertScrollTimer.Start();
    }

    private static int ComputeReferenceAlertScrollTicks(string text)
    {
        var charCount = Math.Max(1, text?.Length ?? 0);
        var durationSeconds = Math.Clamp(charCount / 8.0, 0.75, 4.0);
        return Math.Max(1, (int)Math.Ceiling(durationSeconds / 0.15));
    }

    private void AdvanceReferenceAlertScroll()
    {
        if (!_lastShowsLiveReferenceAlert || !Scene.ReferenceAlertScroll)
        {
            StopReferenceAlertScroll();
            return;
        }

        _referenceAlertScrollTick++;
        var progress = Math.Min(1.0, _referenceAlertScrollTick / (double)_referenceAlertScrollTotalTicks);
        ReferenceAlertHorizontalOffset = 180 * (1.0 - progress);
        if (progress >= 1.0)
        {
            _referenceAlertScrollTimer.Stop();
            ReferenceAlertHorizontalOffset = 0;
        }
    }

    internal void AdvanceReferenceAlertScrollForTest() => AdvanceReferenceAlertScroll();

    private void StopReferenceAlertScroll()
    {
        _referenceAlertScrollTimer.Stop();
        _referenceAlertScrollTick = 0;
        _referenceAlertScrollTotalTicks = 1;
        ReferenceAlertHorizontalOffset = 0;
    }

    private void ApplyReferenceAlertBrushes(OutputSceneSnapshot scene)
    {
        var normalBackground = scene.ReferenceAlertTransparent ? Brushes.Transparent : PanelBackgroundBrush;
        if (!scene.ReferenceAlertFlash || !_referenceAlertFlashAlternate)
        {
            ReferenceAlertForegroundBrush = PanelForegroundBrush;
            ReferenceAlertBackgroundBrush = normalBackground;
            return;
        }

        ReferenceAlertForegroundBrush = scene.ReferenceAlertTransparent
            ? CreateBrush(unchecked((int)0xFFFF0000))
            : CreateBrush(unchecked((int)0xFFFFFFFF));
        ReferenceAlertBackgroundBrush = scene.ReferenceAlertTransparent
            ? Brushes.Transparent
            : CreateBrush(unchecked((int)0xFFFF0000));
    }

    private int GetIntSetting(SettingKey<int> key)
        => _settings is null ? key.DefaultValue : _settings.Get(key);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _lyricsAlertFlashTimer.Stop();
        _referenceAlertFlashTimer.Stop();
        _referenceAlertScrollTimer.Stop();
        if (_settings is not null)
        {
            _settings.SettingsChanged -= OnSettingsChanged;
        }
    }

    private LiveOutputRenderSettings GetLiveOutputSettings()
        => _settings is null
            ? LiveOutputRenderSettings.Default
            : LiveOutputRenderSettings.From(_settings);

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs args)
    {
        if (ContainsLiveOutputSetting(args.ChangedKeys))
        {
            RefreshDisplayText();
        }

        // 전환(페이드) 설정 변경은 다음 장면 전환부터 즉시 반영 — 텍스트 재계산은 불필요하고 페이드 길이만 갱신.
        if (ContainsTransitionSetting(args.ChangedKeys))
        {
            ApplyTransitionSettings();
        }
    }

    // 참고: 슬라이드 전환 종류(LyricsMonitorSlideTransitionKind)는 여기 일부러 넣지 않는다 —
    // ApplyTransitionSettings 는 "항목 전환" 종류만 읽고, 슬라이드 전환 종류는 매 송출마다
    // UpdateContentTransitionKindForChange 가 새로 읽어 적용하므로 설정 변경 시 별도 재적용이 필요 없다.
    private static bool ContainsTransitionSetting(IReadOnlyList<string> changedKeys)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            var key = changedKeys[i];
            if (string.Equals(key, EasiSettingKeys.LyricsMonitorUseFadeTransition.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTransitionDurationMs.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTransitionKind.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int GetViewportWidth(OutputWindowState output)
        => output.IsOpen && output.Placement.Width > 0
            ? (int)output.Placement.Width
            : DefaultViewportWidth;

    private static int GetViewportHeight(OutputWindowState output)
        => output.IsOpen && output.Placement.Height > 0
            ? (int)output.Placement.Height
            : DefaultViewportHeight;

    // 기본 GAP 로고 로더: 경로가 유효하면 BitmapImage로 로드 후 Freeze.
    // BitmapCacheOption.OnLoad로 즉시 디코딩해 파일 핸들을 닫고, Freeze로 다른 스레드에서도 안전하게 사용 가능.
    private static ImageSource? DefaultGapLogoLoader(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            // 파일이 잠겨 있거나 디코딩 실패 시 조용히 무시 → 호출부는 텍스트 폴백 사용
            return null;
        }
    }

    // 가사 정렬 enum → WPF 매핑. TextAlignment 는 줄 내부 정렬, HorizontalAlignment 는 MaxWidth 블록의 화면 배치.
    private static TextAlignment ToTextAlignment(LyricsTextAlignment alignment)
        => alignment switch
        {
            LyricsTextAlignment.Left => TextAlignment.Left,
            LyricsTextAlignment.Right => TextAlignment.Right,
            _ => TextAlignment.Center,
        };

    private static HorizontalAlignment ToHorizontalAlignment(LyricsTextAlignment alignment)
        => alignment switch
        {
            LyricsTextAlignment.Left => HorizontalAlignment.Left,
            LyricsTextAlignment.Right => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Center,
        };

    // 가사 세로 정렬 enum → WPF VerticalAlignment(본문 블록을 화면 위/가운데/아래로 배치).
    private static VerticalAlignment ToVerticalAlignment(LyricsVerticalAlignment alignment)
        => alignment switch
        {
            LyricsVerticalAlignment.Top => VerticalAlignment.Top,
            LyricsVerticalAlignment.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Center,
        };

    private static Color ColorFromArgb(int argb)
    {
        var value = unchecked((uint)argb);
        return Color.FromArgb(
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value);
    }

    // Display Panel 밴드의 기본(반투명 검정) 배경 — 기존 XAML 리터럴 #66000000 과 동일. Freeze 해 공유.
    private static readonly Brush PanelDefaultBrush = CreateFrozenPanelBrush();

    private static Brush CreateFrozenPanelBrush()
    {
        var brush = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00));
        brush.Freeze();
        return brush;
    }

    private static Brush CreateBrush(int argb)
    {
        var brush = new SolidColorBrush(ColorFromArgb(argb));
        brush.Freeze();
        return brush;
    }

    // 배경 브러시 — isGradient=true 이고 두 색이 다르면 방향(direction)대로 2색 그라데이션, 아니면 솔리드(G2 / FrmBackground 슬라이스).
    private static Brush CreateBackgroundBrush(int color1Argb, int color2Argb, bool isGradient, Settings.LyricsGradientDirection direction)
    {
        if (!isGradient || color1Argb == color2Argb)
        {
            return CreateBrush(color1Argb);
        }

        var (start, end) = GradientPoints(direction);
        var brush = new LinearGradientBrush(
            ColorFromArgb(color1Argb),
            ColorFromArgb(color2Argb),
            start,
            end);
        brush.Freeze();
        return brush;
    }

    // 그라데이션 방향 → LinearGradientBrush 의 시작/끝 점(상대 좌표 0~1). 기본 Vertical(위→아래)은 기존과 동일.
    private static (Point Start, Point End) GradientPoints(Settings.LyricsGradientDirection direction)
        => direction switch
        {
            Settings.LyricsGradientDirection.Horizontal => (new Point(0, 0.5), new Point(1, 0.5)),
            Settings.LyricsGradientDirection.DiagonalDown => (new Point(0, 0), new Point(1, 1)),
            Settings.LyricsGradientDirection.DiagonalUp => (new Point(0, 1), new Point(1, 0)),
            _ => (new Point(0.5, 0), new Point(0.5, 1)), // Vertical(기본) — 위→아래.
        };

    private static bool ContainsLiveOutputSetting(IReadOnlyList<string> changedKeys)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            var key = changedKeys[i];
            if (string.Equals(key, EasiSettingKeys.ShowLyricsMonitorAlertBox.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.ReferenceAlertScroll.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.ReferenceAlertFlash.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.ReferenceAlertTransparent.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.GapItemOption.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.GapItemLogoFile.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.GapItemUseFade.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTextColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorHighlightColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                // 배경 끝색·그라데이션 여부도 출력 배경에 직접 영향 — 누락 시 그라데이션 설정 변경이
                // 라이브로 즉시 반영되지 않는다(인-셸 출력 모양 인스펙터가 한 키씩 Set 하므로 마지막 키가
                // 화이트리스트에 없으면 갱신을 못 받음). code-review CRITICAL 반영.
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundColor2Argb.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundIsGradient.Id, StringComparison.OrdinalIgnoreCase) ||
                // 주의: "코드 표시"(LyricsMonitorShowNotations)는 여기(렌더 계층 화이트리스트)에 두지 않는다 —
                // 본문 텍스트 자체가 달라지는 설정이라, MainViewModel 이 라이브 곡을 재송출해 본문을 다시 계산한다.
                // 인-셸 가사 정렬(가로/세로)·폰트 크기 변경도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorTextAlignment.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorVerticalAlignment.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorFontSize.Id, StringComparison.OrdinalIgnoreCase) ||
                // 줄 간격 변경도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorLineSpacingPercent.Id, StringComparison.OrdinalIgnoreCase) ||
                // 본문 여백(좌/우/아래) 변경도 라이브 출력에 즉시 반영(FrmMain ShowLeftMargin/Right/Bottom).
                string.Equals(key, EasiSettingKeys.LyricsMonitorBodyLeftMargin.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBodyRightMargin.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBodyBottomMargin.Id, StringComparison.OrdinalIgnoreCase) ||
                // 이중 언어 영역 간 세로 간격 변경도 라이브 출력에 즉시 반영(FrmMain Ind_Reg2TopUpDown).
                string.Equals(key, EasiSettingKeys.LyricsMonitorRegionGapPx.Id, StringComparison.OrdinalIgnoreCase) ||
                // 본문 세로 위치 오프셋 변경도 라이브 출력에 즉시 반영(FrmMain Ind_Reg1TopUpDown).
                string.Equals(key, EasiSettingKeys.LyricsMonitorBodyVerticalOffset.Id, StringComparison.OrdinalIgnoreCase) ||
                // 폰트 효과(굵게/기울임/그림자) 변경도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorBold.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorItalic.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorShadow.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorUnderline.Id, StringComparison.OrdinalIgnoreCase) ||
                // 강조 후렴만 토글도 라이브 출력에 즉시 반영(현재 절이 후렴인지에 따라 강조가 켜지고 꺼진다).
                string.Equals(key, EasiSettingKeys.LyricsMonitorEmphasisChorusOnly.Id, StringComparison.OrdinalIgnoreCase) ||
                // 줄 교차(인터레이스) 토글도 라이브 출력에 즉시 반영.
                string.Equals(key, EasiSettingKeys.LyricsMonitorInterlace.Id, StringComparison.OrdinalIgnoreCase) ||
                // Display Panel 전체 표시 토글도 라이브 출력에 즉시 반영(Def_PanelShow).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowDisplayPanel.Id, StringComparison.OrdinalIgnoreCase) ||
                // Display Panel 투명 토글도 라이브 출력에 즉시 반영(Def_PanelTransparent).
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelTransparent.Id, StringComparison.OrdinalIgnoreCase) ||
                // Display Panel 밴드 색 변경도 라이브 출력에 즉시 반영(Def_PanelColour).
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                // Display Panel 글자 크기 비율 변경도 라이브 출력에 즉시 반영(Def_PanelFont 크기).
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelFontScalePercent.Id, StringComparison.OrdinalIgnoreCase) ||
                // Display Panel 글자색/효과도 라이브 출력에 즉시 반영(Def_PanelAsR1/TextColour/Font B-I-U).
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelTextColorFollowRegion1.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelTextColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelBold.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelItalic.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorPanelUnderline.Id, StringComparison.OrdinalIgnoreCase) ||
                // 위치 인디케이터 표시 토글도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowPositionIndicator.Id, StringComparison.OrdinalIgnoreCase) ||
                // 절 헤딩 표시 토글도 라이브 출력에 즉시 반영(FrmMain Def_Head All).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowVerseHeading.Id, StringComparison.OrdinalIgnoreCase) ||
                // 곡 번호 표시 토글도 라이브 출력에 즉시 반영(Display Panel).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowItemNumber.Id, StringComparison.OrdinalIgnoreCase) ||
                // 정보 패널 곡 제목 표시 토글도 라이브 출력에 즉시 반영(Def_PanelTitle).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowTitleOnPanel.Id, StringComparison.OrdinalIgnoreCase) ||
                // 저작권 표시 토글도 라이브 출력에 즉시 반영(Display Panel).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowCopyright.Id, StringComparison.OrdinalIgnoreCase) ||
                // 다음 항목 표시 토글도 라이브 출력에 즉시 반영(Display Panel PrevNext).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowNextItem.Id, StringComparison.OrdinalIgnoreCase) ||
                // 전역 배경 이미지 경로·표시 모드 변경도 라이브 출력에 즉시 반영(FrmMain Images 탭 / Def_ImageMode).
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundImagePath.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundMode.Id, StringComparison.OrdinalIgnoreCase) ||
                // 배경 그라데이션 방향 변경도 라이브 출력에 즉시 반영(FrmMain Def_BackColour 패턴).
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundGradientDirection.Id, StringComparison.OrdinalIgnoreCase) ||
                // 이중 언어 영역 표시 모드(둘다/Region1만/Region2만)도 라이브 출력에 즉시 반영.
                string.Equals(key, EasiSettingKeys.LyricsMonitorRegionDisplay.Id, StringComparison.OrdinalIgnoreCase) ||
                // 제목 헤딩 표시·정렬·첫화면만 토글도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowTitleHeading.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.Id, StringComparison.OrdinalIgnoreCase) ||
                // 외곽선 효과 토글도 라이브 출력에 즉시 반영(§7.3-A 폰트 효과).
                string.Equals(key, EasiSettingKeys.LyricsMonitorOutline.Id, StringComparison.OrdinalIgnoreCase) ||
                // 전역 출력 글꼴명 변경도 라이브 출력에 즉시 반영(FrmMain Def_FontName, 곡별 글꼴이 없을 때).
                string.Equals(key, EasiSettingKeys.LyricsMonitorFontFamily.Id, StringComparison.OrdinalIgnoreCase) ||
                // 보조 영역(Region2) 전역 글꼴명 변경도 라이브 출력에 즉시 반영(FrmMain Ind_Reg2Font).
                string.Equals(key, EasiSettingKeys.LyricsMonitorFontFamily2.Id, StringComparison.OrdinalIgnoreCase) ||
                // 보조 영역(Region2) 전역 글자색 변경도 라이브 출력에 즉시 반영(0=본문 색 추종).
                string.Equals(key, EasiSettingKeys.LyricsMonitorTextColor2Argb.Id, StringComparison.OrdinalIgnoreCase) ||
                // 보조 영역(Region2) 전역 가로 정렬 변경도 라이브 출력에 즉시 반영(FollowRegion1=본문 정렬 추종).
                string.Equals(key, EasiSettingKeys.LyricsMonitorRegion2Alignment.Id, StringComparison.OrdinalIgnoreCase) ||
                // 보조 영역(Region2) 전역 굵게(3-상태) 변경도 라이브 출력에 즉시 반영(FollowRegion1=본문 굵게 추종).
                string.Equals(key, EasiSettingKeys.LyricsMonitorRegion2Bold.Id, StringComparison.OrdinalIgnoreCase) ||
                // 보조 영역(Region2) 전역 기울임(3-상태) 변경도 라이브 출력에 즉시 반영(FollowRegion1=본문 기울임 추종).
                string.Equals(key, EasiSettingKeys.LyricsMonitorRegion2Italic.Id, StringComparison.OrdinalIgnoreCase) ||
                // 보조 영역(Region2) 전역 밑줄(3-상태) 변경도 라이브 출력에 즉시 반영(FollowRegion1=본문 밑줄 추종).
                string.Equals(key, EasiSettingKeys.LyricsMonitorRegion2Underline.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoPowerPointPanelOverlay.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoMediaPanelOverlay.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
