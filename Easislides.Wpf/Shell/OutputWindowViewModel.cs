using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

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
    private OutputWindowState _output = OutputWindowState.Closed;
    private string _currentItemTitle = string.Empty;
    private string _outputMonitorName = string.Empty;
    private string _displayTitle = "STANDBY";
    private string _statusLabel = "STANDBY";
    private string _bodyText = string.Empty;
    private bool _isBlackout;
    private bool _isOutputOpen;
    private Brush _sceneForegroundBrush;
    private Brush _sceneBackgroundBrush;
    private Visibility _lyricsAlertVisibility = Visibility.Collapsed;
    private Visibility _notationVisibility = Visibility.Visible;
    private Visibility _panelOverlayVisibility = Visibility.Visible;
    private Visibility _displayTitleVisibility = Visibility.Visible;
    private Visibility _bodyTextVisibility = Visibility.Collapsed;
    private TextAlignment _bodyTextAlignment = TextAlignment.Center;
    private HorizontalAlignment _bodyHorizontalAlignment = HorizontalAlignment.Center;
    private VerticalAlignment _bodyVerticalAlignment = VerticalAlignment.Center;
    private double _bodyFontSize = 48;
    private double _bodyLineHeight = 60;
    // 본문 글꼴 — 곡별 FormatData 폰트명(43) 오버라이드. 오버라이드가 없으면 UnsetValue 로 두어
    // XAML FontFamily 바인딩이 부모(테마)의 글꼴을 그대로 상속하게 한다(null 로 두면 기본 글꼴로 떨어져 회귀 위험).
    private object _bodyFontFamily = DependencyProperty.UnsetValue;
    private FontWeight _bodyFontWeight = FontWeights.SemiBold;
    private FontStyle _bodyFontStyle = FontStyles.Normal;
    private bool _bodyHasShadow;
    // 외곽선 효과 사용 여부(§7.3-A). on 이면 본문을 외곽선 렌더러로 그린다(일반 본문과 상호배타).
    private bool _bodyHasOutline;
    // 외곽선 렌더러(OutlinedTextBlock) 가시성 — 외곽선 on + 본문 송출 중일 때만 Visible.
    private Visibility _bodyOutlineVisibility = Visibility.Collapsed;
    private string _positionLabel = string.Empty;
    private Visibility _positionIndicatorVisibility = Visibility.Collapsed;
    // 제목 헤딩(가사 위 상단 배너) 가시성 — 기본 숨김(설정 off 일 때 기존 동작 보존). §7.3-A
    private Visibility _titleHeadingVisibility = Visibility.Collapsed;
    // 제목 헤딩 가로 정렬(설정에서 유래, §7.3-A Heading Align). 기본 가운데.
    private TextAlignment _titleHeadingTextAlignment = TextAlignment.Center;
    private HorizontalAlignment _titleHeadingHorizontalAlignment = HorizontalAlignment.Center;
    // 본문 상단 여백 — 제목 헤딩이 보일 때만 헤딩 배너 높이만큼 위를 비워(겹침 방지),
    // 본문 세로 정렬이 "위"여도 헤딩과 포개지지 않게 한다(§7.3-A code-review MAJOR 반영). 기본 0.
    private Thickness _bodyContentMargin = new(0);
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
    private Visibility _backgroundImageVisibility = Visibility.Collapsed;
    // 배경 이미지 캐시: 같은(해석된) 경로를 매번 다시 디코딩하지 않도록 보관.
    private string? _cachedBackgroundImagePath;
    private ImageSource? _cachedBackgroundImageSource;
    private OutputSceneSnapshot _scene;
    private TimeSpan _contentFadeDuration = TimeSpan.FromMilliseconds(250);
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
        // 로더 미주입 시 기본 로더(BitmapImage)로 디스크에서 직접 로드
        _gapLogoLoader = gapLogoLoader ?? DefaultGapLogoLoader;
        _sceneForegroundBrush = CreateBrush(LiveOutputRenderSettings.Default.LyricsMonitorTextColorArgb);
        _sceneBackgroundBrush = CreateBackgroundBrush(
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundColorArgb,
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundColor2Argb,
            LiveOutputRenderSettings.Default.LyricsMonitorBackgroundIsGradient);
        if (_settings is not null)
        {
            _settings.SettingsChanged += OnSettingsChanged;
        }

        _scene = CreateScene();
        ApplyScene(_scene);
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

    /// <summary>가사 본문 그림자 표시 여부 — 어두운/영상 배경 위 가독성. XAML 이 DropShadowEffect 적용. §7.3-A.</summary>
    public bool BodyHasShadow
    {
        get => _bodyHasShadow;
        private set => SetProperty(ref _bodyHasShadow, value);
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

    public Visibility NotationVisibility
    {
        get => _notationVisibility;
        private set => SetProperty(ref _notationVisibility, value);
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

    private void RefreshDisplayText()
    {
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
        SceneBackgroundBrush = CreateBackgroundBrush(
            scene.LyricsMonitorBackgroundColorArgb,
            scene.LyricsMonitorBackgroundColor2Argb,
            scene.LyricsMonitorBackgroundIsGradient);
        // 곡별 배경 이미지(있으면) 로드 — 색 배경 위에 표시(이미지 우선). 없거나 실패면 색 배경만 보인다.
        ApplyBackgroundImage(scene);
        LyricsAlertVisibility = scene.ShowsLyricsAlertBox ? Visibility.Visible : Visibility.Collapsed;
        NotationVisibility = scene.LyricsMonitorShowNotations ? Visibility.Visible : Visibility.Collapsed;
        // 곡 가사 본문을 송출 슬롯에 반영. 본문이 보이면 타이틀과 겹치므로 ApplyGapLogo 에서 타이틀을 숨긴다.
        BodyText = scene.BodyText;
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
        BodyHasShadow = scene.LyricsMonitorShadow;
        PositionLabel = scene.PositionLabel;
        PositionIndicatorVisibility = scene.ShowsPositionIndicator ? Visibility.Visible : Visibility.Collapsed;
        // 제목 헤딩: 설정 on + 가사 본문 송출 중일 때만 상단 배너로 곡 제목 노출(§7.3-A).
        TitleHeadingVisibility = scene.ShowsTitleHeading ? Visibility.Visible : Visibility.Collapsed;
        // 제목 헤딩 가로 정렬 — 본문 정렬과 동일 enum·헬퍼 재사용(§7.3-A Heading Align).
        TitleHeadingTextAlignment = ToTextAlignment(scene.LyricsMonitorTitleHeadingAlignment);
        TitleHeadingHorizontalAlignment = ToHorizontalAlignment(scene.LyricsMonitorTitleHeadingAlignment);
        // 헤딩이 보이면 본문 위에 배너 높이만큼 여백을 확보 — 본문 세로정렬이 "위"여도 겹치지 않게 한다.
        // 헤딩은 1줄(NoWrap)로 높이가 결정적이라 고정 여백으로 충분하다.
        BodyContentMargin = scene.ShowsTitleHeading ? new Thickness(0, TitleHeadingReservedHeight, 0, 0) : new Thickness(0);
        var bodyShown = scene.ShowsBodyText;
        // 외곽선 효과(§7.3-A): on 이면 외곽선 렌더러만, off 면 일반 본문만 보이게 상호배타로 전환(겹침 방지).
        // 외곽선 기본 off 라 기존 테스트/동작에선 BodyTextVisibility=bodyShown 그대로다.
        BodyHasOutline = scene.UsesBodyOutline;
        BodyTextVisibility = (bodyShown && !scene.UsesBodyOutline) ? Visibility.Visible : Visibility.Collapsed;
        BodyOutlineVisibility = (bodyShown && scene.UsesBodyOutline) ? Visibility.Visible : Visibility.Collapsed;
        var panelOverlay = scene.ShowsPanelOverlay ? Visibility.Visible : Visibility.Collapsed;
        PanelOverlayVisibility = panelOverlay;
        ApplyGapLogo(scene, panelOverlay, bodyShown);
        BlackoutOverlayVisibility = IsBlackoutOrHidden(scene.Kind) ? Visibility.Visible : Visibility.Collapsed;
        ApplyContentPlacement(scene);
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
        BackgroundImageVisibility = image is not null ? Visibility.Visible : Visibility.Collapsed;
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
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

    private static Brush CreateBrush(int argb)
    {
        var brush = new SolidColorBrush(ColorFromArgb(argb));
        brush.Freeze();
        return brush;
    }

    // 배경 브러시 — isGradient=true 이고 두 색이 다르면 위→아래 세로 그라데이션, 아니면 솔리드(G2 / FrmBackground 슬라이스).
    private static Brush CreateBackgroundBrush(int color1Argb, int color2Argb, bool isGradient)
    {
        if (!isGradient || color1Argb == color2Argb)
        {
            return CreateBrush(color1Argb);
        }

        var brush = new LinearGradientBrush(
            ColorFromArgb(color1Argb),
            ColorFromArgb(color2Argb),
            new Point(0.5, 0),
            new Point(0.5, 1));
        brush.Freeze();
        return brush;
    }

    private static bool ContainsLiveOutputSetting(IReadOnlyList<string> changedKeys)
    {
        for (var i = 0; i < changedKeys.Count; i++)
        {
            var key = changedKeys[i];
            if (string.Equals(key, EasiSettingKeys.ShowLyricsMonitorAlertBox.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.GapItemOption.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.GapItemLogoFile.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.GapItemUseFade.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTextColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundColorArgb.Id, StringComparison.OrdinalIgnoreCase) ||
                // 배경 끝색·그라데이션 여부도 출력 배경에 직접 영향 — 누락 시 그라데이션 설정 변경이
                // 라이브로 즉시 반영되지 않는다(인-셸 출력 모양 인스펙터가 한 키씩 Set 하므로 마지막 키가
                // 화이트리스트에 없으면 갱신을 못 받음). code-review CRITICAL 반영.
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundColor2Argb.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorBackgroundIsGradient.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowNotations.Id, StringComparison.OrdinalIgnoreCase) ||
                // 인-셸 가사 정렬(가로/세로)·폰트 크기 변경도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorTextAlignment.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorVerticalAlignment.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorFontSize.Id, StringComparison.OrdinalIgnoreCase) ||
                // 줄 간격 변경도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorLineSpacingPercent.Id, StringComparison.OrdinalIgnoreCase) ||
                // 폰트 효과(굵게/기울임/그림자) 변경도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorBold.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorItalic.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorShadow.Id, StringComparison.OrdinalIgnoreCase) ||
                // 위치 인디케이터 표시 토글도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowPositionIndicator.Id, StringComparison.OrdinalIgnoreCase) ||
                // 제목 헤딩 표시·정렬·첫화면만 토글도 라이브 출력에 즉시 반영(§7.3-A).
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowTitleHeading.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTitleHeadingAlignment.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.LyricsMonitorTitleHeadingFirstScreenOnly.Id, StringComparison.OrdinalIgnoreCase) ||
                // 외곽선 효과 토글도 라이브 출력에 즉시 반영(§7.3-A 폰트 효과).
                string.Equals(key, EasiSettingKeys.LyricsMonitorOutline.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoPowerPointPanelOverlay.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoMediaPanelOverlay.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
