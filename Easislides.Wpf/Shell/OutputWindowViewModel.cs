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
    private bool _isBlackout;
    private bool _isOutputOpen;
    private Brush _sceneForegroundBrush;
    private Brush _sceneBackgroundBrush;
    private Visibility _lyricsAlertVisibility = Visibility.Collapsed;
    private Visibility _notationVisibility = Visibility.Visible;
    private Visibility _panelOverlayVisibility = Visibility.Visible;
    private Visibility _displayTitleVisibility = Visibility.Visible;
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
        _sceneBackgroundBrush = CreateBrush(LiveOutputRenderSettings.Default.LyricsMonitorBackgroundColorArgb);
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
        ContentImageSource = source;
        _contentPixelWidth = pixelWidth;
        _contentPixelHeight = pixelHeight;
        _contentFillMode = fillMode;
        RefreshDisplayText();
    }

    public void ApplySession(LiveSessionSnapshot snapshot)
    {
        _session = snapshot;
        State = snapshot.State;
        CurrentItemTitle = snapshot.CurrentItemTitle;
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
        SceneBackgroundBrush = CreateBrush(scene.LyricsMonitorBackgroundColorArgb);
        LyricsAlertVisibility = scene.ShowsLyricsAlertBox ? Visibility.Visible : Visibility.Collapsed;
        NotationVisibility = scene.LyricsMonitorShowNotations ? Visibility.Visible : Visibility.Collapsed;
        var panelOverlay = scene.ShowsPanelOverlay ? Visibility.Visible : Visibility.Collapsed;
        PanelOverlayVisibility = panelOverlay;
        ApplyGapLogo(scene, panelOverlay);
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

        var hasContent = _contentImageSource is not null
            && scene.ShowsContent
            && placement.Width > 0
            && placement.Height > 0;
        ContentVisibility = hasContent ? Visibility.Visible : Visibility.Collapsed;
    }

    // Blackout(완전 차단) / Hidden(화면 끄기) 모두 송출 화면은 검정으로 덮어야 한다.
    // 두 상태를 시각적으로 구분하는 것은 운영자 패널의 역할이고, 송출 화면은 둘 다 깨끗한 검정이 안전.
    private static bool IsBlackoutOrHidden(OutputSceneKind kind)
        => kind == OutputSceneKind.Blackout || kind == OutputSceneKind.Hidden;

    // GAP 모드(User)에서 로고 파일이 로딩 가능하면 이미지를, 그 외에는 기존 타이틀 텍스트를 표시한다.
    // 로고가 보이는 동안에는 타이틀 텍스트와 시각적으로 겹치지 않도록 DisplayTitleVisibility를 Collapsed로 둔다.
    private void ApplyGapLogo(OutputSceneSnapshot scene, Visibility panelOverlay)
    {
        var logo = TryLoadGapLogo(scene);
        GapLogoSource = logo;
        // 패널 오버레이가 숨겨진 상태(예: PPT 라이브)면 로고도 굳이 노출하지 않는다.
        var gapLogoVisible = logo is not null && panelOverlay == Visibility.Visible;
        GapLogoVisibility = gapLogoVisible ? Visibility.Visible : Visibility.Collapsed;
        DisplayTitleVisibility = gapLogoVisible ? Visibility.Collapsed : panelOverlay;
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

    private static Brush CreateBrush(int argb)
    {
        var value = unchecked((uint)argb);
        var brush = new SolidColorBrush(Color.FromArgb(
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value));
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
                string.Equals(key, EasiSettingKeys.LyricsMonitorShowNotations.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoPowerPointPanelOverlay.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, EasiSettingKeys.NoMediaPanelOverlay.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
