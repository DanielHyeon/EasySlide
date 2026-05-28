using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Controls;

namespace Easislides.Wpf.Shell;

public sealed class OutputWindowViewModel : ObservableObject
{
    private LiveState _state = LiveState.Off;
    private string _currentItemTitle = string.Empty;
    private string _outputMonitorName = string.Empty;
    private string _displayTitle = "STANDBY";
    private string _statusLabel = "STANDBY";
    private bool _isBlackout;
    private bool _isOutputOpen;

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

    public void ApplySession(LiveSessionSnapshot snapshot)
    {
        State = snapshot.State;
        CurrentItemTitle = snapshot.CurrentItemTitle;
        IsBlackout = snapshot.IsBlackout;

        if (!string.IsNullOrWhiteSpace(snapshot.OutputMonitorName))
        {
            OutputMonitorName = snapshot.OutputMonitorName;
        }

        RefreshDisplayText();
    }

    public void ApplyOutput(OutputWindowState state)
    {
        IsOutputOpen = state.IsOpen;
        OutputMonitorName = state.Display?.Name ?? string.Empty;
        RefreshDisplayText();
    }

    private void RefreshDisplayText()
    {
        if (State == LiveState.Hidden && IsBlackout)
        {
            DisplayTitle = "BLACK";
            StatusLabel = "BLACKOUT";
            return;
        }

        if (State == LiveState.Hidden)
        {
            DisplayTitle = "HIDDEN";
            StatusLabel = "HIDDEN";
            return;
        }

        if (State == LiveState.Active)
        {
            DisplayTitle = string.IsNullOrWhiteSpace(CurrentItemTitle) ? "LIVE" : CurrentItemTitle;
            StatusLabel = "LIVE";
            return;
        }

        DisplayTitle = IsOutputOpen ? "OUTPUT READY" : "STANDBY";
        StatusLabel = IsOutputOpen ? "READY" : "STANDBY";
    }
}
