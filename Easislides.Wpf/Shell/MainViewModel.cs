using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Input;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ILiveSessionService _session;
    private readonly IOutputWindowService _output;
    private readonly ILiveSafetyPrompt _safetyPrompt;
    private readonly ICommandTelemetry _telemetry;
    private readonly IDisplayService _display;
    private readonly ICommandCatalog _commandCatalog;
    private readonly ISettingsService _settings;

    [ObservableProperty] private LiveQueueItem? _selectedItem;
    [ObservableProperty] private OutputDisplay? _selectedOutputDisplay;
    [ObservableProperty] private string _statusText = "WPF 운영 셸 준비됨";

    public MainViewModel(
        ILiveSessionService session,
        IOutputWindowService output,
        ILiveSafetyPrompt safetyPrompt,
        ICommandTelemetry telemetry,
        IDisplayService display,
        ICommandCatalog commandCatalog,
        ISettingsService settings)
    {
        _session = session;
        _output = output;
        _safetyPrompt = safetyPrompt;
        _telemetry = telemetry;
        _display = display;
        _commandCatalog = commandCatalog;
        _settings = settings;

        _session.SessionChanged += (_, e) => ApplyLiveSnapshot(e.Snapshot);
        _output.OutputChanged += (_, _) => NotifyCommandStates();

        OpenOutputCommand = new RelayCommand(OpenOutput);
        CloseOutputCommand = new AsyncRelayCommand(CloseOutputAsync, () => _output.Current.IsOpen);
        GoLiveCommand = new AsyncRelayCommand(GoLiveAsync, CanGoLive);
        StopLiveCommand = new AsyncRelayCommand(StopLiveAsync, () => _session.Current.State != LiveState.Off);
        NextItemCommand = new RelayCommand(NextItem, CanMoveNext);
        PreviousItemCommand = new RelayCommand(PreviousItem, CanMovePrevious);
        HideOutputCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: false), CanUseLiveSafetyAction);
        BlackScreenCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: true), CanUseLiveSafetyAction);

        SeedPlaceholderQueue();
        RefreshOutputDisplays();
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
    public IAsyncRelayCommand HideOutputCommand { get; }
    public IAsyncRelayCommand BlackScreenCommand { get; }

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
        NotifyCommandStates();
    }

    public void BindShortcuts(ShortcutRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

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
    }

    public void RefreshOutputDisplays()
    {
        var preferredId = SelectedOutputDisplay?.Id;
        var displays = _display.GetDisplays();

        OutputDisplays.Clear();
        foreach (var display in displays)
        {
            OutputDisplays.Add(display);
        }

        var selected = _display.GetPreferredDisplay(preferredId);
        var matching = OutputDisplays.FirstOrDefault(display =>
            string.Equals(display.Id, selected.Id, StringComparison.OrdinalIgnoreCase)) ?? selected;
        if (!OutputDisplays.Contains(matching))
        {
            OutputDisplays.Add(matching);
        }

        SelectedOutputDisplay = matching;
    }

    partial void OnSelectedItemChanged(LiveQueueItem? value)
    {
        LiveBar.CurrentItemTitle = value?.Title ?? string.Empty;
        NotifyCommandStates();
    }

    private void OpenOutput()
    {
        var display = SelectedOutputDisplay ?? _display.GetPreferredDisplay();
        _output.Open(display, windowed: true);
        SelectedOutputDisplay = display;
        LiveBar.OutputMonitorName = _output.Current.Display?.Name ?? string.Empty;
        StatusText = $"출력 창 열림: {LiveBar.OutputMonitorName}";
        _telemetry.Record(MainCommandIds.OutputOpen, succeeded: true, StatusText);
        NotifyCommandStates();
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

        LiveBar.OutputMonitorName = string.Empty;
        StatusText = "출력 창 닫힘";
        _telemetry.Record(MainCommandIds.OutputClose, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    private bool CanGoLive() => SelectedItem is not null && _output.Current.IsOpen;

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
        _session.GoLive(SelectedItem, monitorName);
        StatusText = $"LIVE: {SelectedItem.Title}";
        _telemetry.Record(MainCommandIds.LiveGo, succeeded: true, StatusText);
        NotifyCommandStates();
    }

    private void ApplyLiveSnapshot(LiveSessionSnapshot snapshot)
    {
        LiveBar.State = snapshot.State;
        LiveBar.CurrentItemTitle = snapshot.CurrentItemTitle;
        LiveBar.OutputMonitorName = snapshot.OutputMonitorName;
        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        GoLiveCommand.NotifyCanExecuteChanged();
        CloseOutputCommand.NotifyCanExecuteChanged();
        StopLiveCommand.NotifyCanExecuteChanged();
        NextItemCommand.NotifyCanExecuteChanged();
        PreviousItemCommand.NotifyCanExecuteChanged();
        HideOutputCommand.NotifyCanExecuteChanged();
        BlackScreenCommand.NotifyCanExecuteChanged();
    }

    private static void RegisterIfMissing(ShortcutRegistry registry, Shortcut shortcut)
    {
        if (registry.All.Any(s => s.CommandName == shortcut.CommandName && s.Key == shortcut.Key && s.Modifiers == shortcut.Modifiers))
        {
            return;
        }

        registry.Register(shortcut);
    }

    private void SeedPlaceholderQueue()
    {
        LoadQueue(new[]
        {
            new LiveQueueItem("sample-welcome", "예배 시작 안내", "Notice"),
            new LiveQueueItem("sample-song", "주일찬양 #1", "Song"),
            new LiveQueueItem("sample-sermon", "말씀 본문", "Bible"),
        });
    }
}
