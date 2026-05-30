using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    [ObservableProperty] private LiveQueueItem? _selectedItem;
    [ObservableProperty] private OutputDisplay? _selectedOutputDisplay;
    [ObservableProperty] private string _statusText = "WPF 운영 준비됨";

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
    private bool _disposed;

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
        IWorshipListStore worshipLists)
    {
        _session = session;
        _output = output;
        _safetyPrompt = safetyPrompt;
        _telemetry = telemetry;
        _display = display;
        _commandCatalog = commandCatalog;
        _settings = settings;
        _worshipLists = worshipLists;
        Media = media;
        PowerPoint = powerPoint;

        _session.SessionChanged += (_, e) => ApplyLiveSnapshot(e.Snapshot);
        _output.OutputChanged += (_, _) => NotifyCommandStates();
        _settings.SettingsChanged += OnSettingsChanged;

        OpenOutputCommand = new RelayCommand(OpenOutput);
        CloseOutputCommand = new AsyncRelayCommand(CloseOutputAsync, () => _output.Current.IsOpen);
        GoLiveCommand = new AsyncRelayCommand(GoLiveAsync, CanGoLive);
        StopLiveCommand = new AsyncRelayCommand(StopLiveAsync, () => _session.Current.State != LiveState.Off);
        NextItemCommand = new RelayCommand(NextItem, CanMoveNext);
        PreviousItemCommand = new RelayCommand(PreviousItem, CanMovePrevious);
        HideOutputCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: false), CanUseLiveSafetyAction);
        BlackScreenCommand = new AsyncRelayCommand(() => HideOutputAsync(blackout: true), CanUseLiveSafetyAction);

        ApplyOperationalSettings(updateStatus: false);
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
        RefreshPowerPointLimitState(updateStatus: true);
        NotifyCommandStates();
    }

    public LiveQueueItem? AddBibleSelection(BibleSelection selection)
    {
        if (string.IsNullOrWhiteSpace(selection.IdString) || string.IsNullOrWhiteSpace(selection.Title))
        {
            StatusText = "선택된 성경 구절이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        var item = new LiveQueueItem(selection.IdString, selection.Title, "Bible");
        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"성경 구절 추가됨: {selection.Title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>
    /// 라이브러리에서 고른 실제 곡을 예배 순서(큐)에 추가한다(라이브 큐 도메인 plumbing — placeholder 대체 기반).
    /// 선택 항목 바로 뒤에 삽입하고 새 항목을 선택. AddBibleSelection 과 동일 규칙.
    /// </summary>
    public LiveQueueItem? AddSong(Data.SongSummary? song)
    {
        if (song is null || string.IsNullOrWhiteSpace(song.Title))
        {
            StatusText = "선택된 곡이 없습니다.";
            NotifyCommandStates();
            return null;
        }

        var item = new LiveQueueItem($"song:{song.SongId}", song.Title, "Song") { Lyrics = song.Lyrics };
        var selectedIndex = SelectedItem is null ? -1 : Queue.IndexOf(SelectedItem);
        var insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : Queue.Count;
        Queue.Insert(insertIndex, item);
        SelectedItem = item;
        StatusText = $"곡 추가됨: {song.Title}";
        NotifyCommandStates();
        return item;
    }

    /// <summary>PowerPoint 파일을 예배 순서(큐)에 추가(선택 시 썸네일 렌더 디스패치).</summary>
    public LiveQueueItem? AddPowerPoint(string filePath) => AddExternalFileItem(filePath, "PowerPoint", "PowerPoint 파일");

    /// <summary>미디어 파일을 예배 순서(큐)에 추가(선택 시 미디어 Load 디스패치).</summary>
    public LiveQueueItem? AddMedia(string filePath) => AddExternalFileItem(filePath, "Media", "미디어 파일");

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
            StatusText = $"예배 순서 불러옴: {name.Trim()} ({items.Count}개)";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"예배 순서 불러오기 실패: {ex.Message}";
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

    // PPT 미리보기 렌더 크기(썸네일 — 출력 해상도와 무관한 미리보기용 고정값).
    private const int PptPreviewWidth = 960;
    private const int PptPreviewHeight = 540;

    partial void OnSelectedItemChanged(LiveQueueItem? value)
    {
        if (_session.Current.State != LiveState.Active)
        {
            LiveBar.CurrentItemTitle = value?.Title ?? string.Empty;
        }

        // 선택 항목의 실제 콘텐츠를 적절한 미리보기 VM 으로 적재(라이브 큐 콘텐츠 plumbing).
        // UI 경로라 fire-and-forget; 테스트는 ApplySelectedItemContentAsync 를 직접 await.
        _ = ApplySelectedItemContentAsync(value);

        NotifyCommandStates();
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
            if (item is { Kind: "PowerPoint", ContentPath: { Length: > 0 } pptPath })
            {
                var slide = item.SlideNumber <= 0 ? 1 : item.SlideNumber;
                await PowerPoint.LoadAsync(pptPath, slide, PptPreviewWidth, PptPreviewHeight).ConfigureAwait(true);
            }
            else
            {
                PowerPoint.Clear();
            }

            if (item is { Kind: "Media", ContentPath: { Length: > 0 } mediaPath })
            {
                Media.Load(new MediaPlaybackRequest(mediaPath, MediaSourceKind.File, TimeSpan.Zero, InferMediaType(mediaPath)));
            }
            else if (Media.StopCommand.CanExecute(null))
            {
                // 다른 종류 항목으로 넘어가면 직전 미디어를 정지(라이브 중 잔류 재생 방지) — PPT.Clear 와 대칭.
                Media.StopCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"항목 콘텐츠 로드 실패: {ex.Message}";
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
        AdvanceSelectionAfterPublish(SelectedItem);
        NotifyCommandStates();
    }

    private void AdvanceSelectionAfterPublish(LiveQueueItem publishedItem)
    {
        if (!_settings.Get(EasiSettingKeys.AdvanceNextItem))
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
        RefreshPowerPointLimitState(updateStatus);
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
                string.Equals(key, EasiSettingKeys.LiveCameraNumber.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPowerPointItem(LiveQueueItem item)
        => string.Equals(item.Kind, "P", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(item.Kind, "PPT", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(item.Kind, "PowerPoint", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(item.Kind, "Presentation", StringComparison.OrdinalIgnoreCase);

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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _settings.SettingsChanged -= OnSettingsChanged;
        // Media VM 정리. DI 컨테이너도 transient IDisposable 을 추적·해제하므로 이중 호출될 수 있으나
        // MediaPlaybackViewModel.Dispose 가 멱등이라 안전(테스트는 new 생성이라 이 경로가 유일 해제).
        Media.Dispose();
        // PowerPoint VM 은 이벤트 구독/미관리 자원이 없어 IDisposable 이 아니다 — 의도적으로 해제하지 않음.
    }

    private void SeedPlaceholderQueue()
    {
        LoadQueue(new[]
        {
            new LiveQueueItem("sample-welcome", "예배 시작 안내", "Notice"),
            new LiveQueueItem("sample-song", "주일 찬양 #1", "Song"),
            new LiveQueueItem("sample-sermon", "말씀 본문", "Bible"),
        });
    }
}
