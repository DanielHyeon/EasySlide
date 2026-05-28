using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
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
    private OutputSceneSnapshot _scene;
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
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _settings = settings;
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
    }

    private OutputSceneSnapshot CreateScene()
        => _renderer.CreateScene(new OutputRenderRequest(
            _session,
            _output,
            GetViewportWidth(_output),
            GetViewportHeight(_output),
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
        PanelOverlayVisibility = scene.ShowsPanelOverlay ? Visibility.Visible : Visibility.Collapsed;
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
