using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Shell;

public sealed class OutputWindowViewModel : ObservableObject
{
    private const int DefaultViewportWidth = 1280;
    private const int DefaultViewportHeight = 720;

    private readonly IOutputRenderer _renderer;
    private LiveState _state = LiveState.Off;
    private LiveSessionSnapshot _session = LiveSessionSnapshot.Off;
    private OutputWindowState _output = OutputWindowState.Closed;
    private string _currentItemTitle = string.Empty;
    private string _outputMonitorName = string.Empty;
    private string _displayTitle = "STANDBY";
    private string _statusLabel = "STANDBY";
    private bool _isBlackout;
    private bool _isOutputOpen;
    private OutputSceneSnapshot _scene;

    public OutputWindowViewModel()
        : this(new OutputRenderer(new ImageAssetService(), new TransitionEffectService()))
    {
    }

    public OutputWindowViewModel(IOutputRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _scene = CreateScene();
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
            GetViewportHeight(_output)));

    private void ApplyScene(OutputSceneSnapshot scene)
    {
        IsOutputOpen = scene.IsOutputOpen;
        IsBlackout = scene.IsBlackout;
        OutputMonitorName = scene.OutputMonitorName;
        DisplayTitle = scene.DisplayTitle;
        StatusLabel = scene.StatusLabel;
    }

    private static int GetViewportWidth(OutputWindowState output)
        => output.IsOpen && output.Placement.Width > 0
            ? (int)output.Placement.Width
            : DefaultViewportWidth;

    private static int GetViewportHeight(OutputWindowState output)
        => output.IsOpen && output.Placement.Height > 0
            ? (int)output.Placement.Height
            : DefaultViewportHeight;
}
