using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Input;
using Easislides.Wpf.Theme;
using Wpf.Ui.Controls;

namespace Easislides.Wpf.Settings;

public enum SettingsSectionKind
{
    General,
    Appearance,
    LiveOutput,
    PowerPoint,
    Media,
    Shortcuts,
    Data,
    ImportExport,
    Advanced,
}

public sealed record SettingsSectionViewModel(
    SettingsSectionKind Kind,
    string Title,
    string Description,
    SymbolRegular Symbol);

public sealed partial class ShortcutEditorItemViewModel : ObservableObject
{
    private string _customGesture;

    public ShortcutEditorItemViewModel(
        string slotId,
        string commandId,
        string category,
        string displayName,
        string description,
        bool isDangerous,
        bool isGlobal,
        string defaultGesture,
        string? customGesture)
    {
        SlotId = slotId;
        CommandId = commandId;
        Category = category;
        DisplayName = displayName;
        Description = description;
        IsDangerous = isDangerous;
        IsGlobal = isGlobal;
        DefaultGesture = defaultGesture;
        _customGesture = customGesture ?? "";
    }

    public string SlotId { get; }

    public string CommandId { get; }

    public string Category { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public bool IsDangerous { get; }

    public bool IsGlobal { get; }

    public string ScopeText => IsGlobal ? "전역" : "로컬";

    public string DefaultGesture { get; }

    public string CustomGesture
    {
        get => _customGesture;
        set
        {
            if (SetProperty(ref _customGesture, value))
            {
                OnPropertyChanged(nameof(EffectiveGesture));
                OnPropertyChanged(nameof(IsCustomized));
            }
        }
    }

    public string EffectiveGesture => string.IsNullOrWhiteSpace(CustomGesture) ? DefaultGesture : CustomGesture;

    public bool IsCustomized => !string.IsNullOrWhiteSpace(CustomGesture);
}

public sealed partial class SettingsWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IThemeService _theme;
    private readonly IDatabaseMigrationService _databaseMigration;
    private readonly ISettingsPathPicker _pathPicker;
    private readonly ICommandCatalog _commandCatalog;
    private bool _isRefreshing;

    private SettingsSectionViewModel _selectedSection;
    private string _language = "";
    private string _workingFolder = "";
    private ColorTheme _themeValue;
    private InterfaceSize _interfaceSize;
    private string _defaultOutputMonitorId = "";
    private bool _useSafetyConfirmations;
    private bool _showLyricsMonitorAlertBox;
    private bool _advanceNextItem;
    private GapItemMode _gapItemOption;
    private string _gapItemLogoFile = "";
    private bool _gapItemUseFade;
    private bool _displayAlwaysUseSecondaryMonitor;
    private int _displayCustomTop;
    private int _displayCustomLeft;
    private int _displayCustomWidth;
    private int _lyricsMonitorTextColorArgb;
    private int _lyricsMonitorBackgroundColorArgb;
    private bool _lyricsMonitorShowNotations;
    private bool _usePowerPointTab;
    private bool _noPowerPointPanelOverlay;
    private int _powerPointRenderTimeoutSeconds;
    private int _thumbnailCacheMegabytes;
    private int _powerPointMaxFiles;
    private bool _useMediaTab;
    private bool _noMediaPanelOverlay;
    private string _mediaDirectory = "";
    private double _mediaVolume;
    private double _mediaBalance;
    private bool _mediaMuted;
    private int _liveCameraNumber;
    private string _adminDatabasePath = "";
    private string _dataBackupRoot = "";
    private bool _enableDiagnostics;
    private string _settingsTransferPath = "";
    private string _statusMessage = "설정 준비됨";
    private string _databaseAnalysisSummary = "DB 분석 대기";

    public SettingsWindowViewModel(
        ISettingsService settings,
        IThemeService theme,
        IDatabaseMigrationService databaseMigration,
        ISettingsPathPicker pathPicker,
        ICommandCatalog commandCatalog)
    {
        _settings = settings;
        _theme = theme;
        _databaseMigration = databaseMigration;
        _pathPicker = pathPicker;
        _commandCatalog = commandCatalog;

        Sections = new ObservableCollection<SettingsSectionViewModel>(CreateSections());
        _selectedSection = Sections[0];

        RestoreDefaultsCommand = new RelayCommand(RestoreDefaults);
        RefreshCommand = new RelayCommand(() => RefreshFromSettings(applyAppearance: true));
        BrowseWorkingFolderCommand = new AsyncRelayCommand(BrowseWorkingFolderAsync);
        BrowseAdminDatabaseCommand = new AsyncRelayCommand(BrowseAdminDatabaseAsync);
        BrowseDataBackupRootCommand = new AsyncRelayCommand(BrowseDataBackupRootAsync);
        BrowseSettingsImportCommand = new AsyncRelayCommand(BrowseSettingsImportAsync);
        BrowseSettingsExportCommand = new AsyncRelayCommand(BrowseSettingsExportAsync);
        SaveShortcutCommand = new RelayCommand<ShortcutEditorItemViewModel>(item =>
        {
            if (item is not null)
            {
                SaveShortcut(item);
            }
        });
        ResetShortcutCommand = new RelayCommand<ShortcutEditorItemViewModel>(item =>
        {
            if (item is not null)
            {
                ResetShortcut(item);
            }
        });
        AnalyzeDatabaseCommand = new AsyncRelayCommand(AnalyzeDatabaseAsync, CanAnalyzeDatabase);
        ExportSettingsCommand = new AsyncRelayCommand(ExportSettingsAsync, CanUseTransferPath);
        ImportSettingsCommand = new AsyncRelayCommand(ImportSettingsAsync, CanUseTransferPath);

        RefreshFromSettings(applyAppearance: true);
    }

    public ObservableCollection<SettingsSectionViewModel> Sections { get; }

    public ObservableCollection<string> ValidationMessages { get; } = new();

    public ObservableCollection<DatabaseTable> DatabaseTables { get; } = new();

    public ObservableCollection<ShortcutEditorItemViewModel> ShortcutItems { get; } = new();

    public IReadOnlyList<ColorTheme> ThemeOptions { get; } = Enum.GetValues<ColorTheme>();

    public IReadOnlyList<InterfaceSize> InterfaceSizeOptions { get; } = Enum.GetValues<InterfaceSize>();

    public IReadOnlyList<GapItemMode> GapItemModeOptions { get; } = Enum.GetValues<GapItemMode>();

    public IRelayCommand RestoreDefaultsCommand { get; }

    public IRelayCommand RefreshCommand { get; }

    public IAsyncRelayCommand BrowseWorkingFolderCommand { get; }

    public IAsyncRelayCommand BrowseAdminDatabaseCommand { get; }

    public IAsyncRelayCommand BrowseDataBackupRootCommand { get; }

    public IAsyncRelayCommand BrowseSettingsImportCommand { get; }

    public IAsyncRelayCommand BrowseSettingsExportCommand { get; }

    public IRelayCommand<ShortcutEditorItemViewModel> SaveShortcutCommand { get; }

    public IRelayCommand<ShortcutEditorItemViewModel> ResetShortcutCommand { get; }

    public IAsyncRelayCommand AnalyzeDatabaseCommand { get; }

    public IAsyncRelayCommand ExportSettingsCommand { get; }

    public IAsyncRelayCommand ImportSettingsCommand { get; }

    public SettingsSectionViewModel SelectedSection
    {
        get => _selectedSection;
        set => SetProperty(ref _selectedSection, value);
    }

    public string Language
    {
        get => _language;
        set => SetAndPersist(ref _language, value, EasiSettingKeys.Language);
    }

    public string WorkingFolder
    {
        get => _workingFolder;
        set => SetAndPersist(ref _workingFolder, value, EasiSettingKeys.WorkingFolder);
    }

    public ColorTheme Theme
    {
        get => _themeValue;
        set => SetAndPersist(ref _themeValue, value, EasiSettingKeys.Theme, _theme.ApplyTheme);
    }

    public InterfaceSize InterfaceSize
    {
        get => _interfaceSize;
        set => SetAndPersist(ref _interfaceSize, value, EasiSettingKeys.InterfaceSize, _theme.ApplyInterfaceSize);
    }

    public string DefaultOutputMonitorId
    {
        get => _defaultOutputMonitorId;
        set => SetAndPersist(ref _defaultOutputMonitorId, value, EasiSettingKeys.DefaultOutputMonitorId);
    }

    public bool UseSafetyConfirmations
    {
        get => _useSafetyConfirmations;
        set => SetAndPersist(ref _useSafetyConfirmations, value, EasiSettingKeys.UseSafetyConfirmations);
    }

    public bool ShowLyricsMonitorAlertBox
    {
        get => _showLyricsMonitorAlertBox;
        set => SetAndPersist(ref _showLyricsMonitorAlertBox, value, EasiSettingKeys.ShowLyricsMonitorAlertBox);
    }

    public bool AdvanceNextItem
    {
        get => _advanceNextItem;
        set => SetAndPersist(ref _advanceNextItem, value, EasiSettingKeys.AdvanceNextItem);
    }

    public GapItemMode GapItemOption
    {
        get => _gapItemOption;
        set => SetAndPersist(ref _gapItemOption, value, EasiSettingKeys.GapItemOption);
    }

    public string GapItemLogoFile
    {
        get => _gapItemLogoFile;
        set => SetAndPersist(ref _gapItemLogoFile, value, EasiSettingKeys.GapItemLogoFile);
    }

    public bool GapItemUseFade
    {
        get => _gapItemUseFade;
        set => SetAndPersist(ref _gapItemUseFade, value, EasiSettingKeys.GapItemUseFade);
    }

    public bool DisplayAlwaysUseSecondaryMonitor
    {
        get => _displayAlwaysUseSecondaryMonitor;
        set => SetAndPersist(ref _displayAlwaysUseSecondaryMonitor, value, EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor);
    }

    public int DisplayCustomTop
    {
        get => _displayCustomTop;
        set => SetAndPersist(ref _displayCustomTop, value, EasiSettingKeys.DisplayCustomTop);
    }

    public int DisplayCustomLeft
    {
        get => _displayCustomLeft;
        set => SetAndPersist(ref _displayCustomLeft, value, EasiSettingKeys.DisplayCustomLeft);
    }

    public int DisplayCustomWidth
    {
        get => _displayCustomWidth;
        set => SetAndPersist(ref _displayCustomWidth, value, EasiSettingKeys.DisplayCustomWidth);
    }

    public int LyricsMonitorTextColorArgb
    {
        get => _lyricsMonitorTextColorArgb;
        set => SetAndPersist(ref _lyricsMonitorTextColorArgb, value, EasiSettingKeys.LyricsMonitorTextColorArgb);
    }

    public int LyricsMonitorBackgroundColorArgb
    {
        get => _lyricsMonitorBackgroundColorArgb;
        set => SetAndPersist(ref _lyricsMonitorBackgroundColorArgb, value, EasiSettingKeys.LyricsMonitorBackgroundColorArgb);
    }

    public bool LyricsMonitorShowNotations
    {
        get => _lyricsMonitorShowNotations;
        set => SetAndPersist(ref _lyricsMonitorShowNotations, value, EasiSettingKeys.LyricsMonitorShowNotations);
    }

    public bool UsePowerPointTab
    {
        get => _usePowerPointTab;
        set => SetAndPersist(ref _usePowerPointTab, value, EasiSettingKeys.UsePowerPointTab);
    }

    public bool NoPowerPointPanelOverlay
    {
        get => _noPowerPointPanelOverlay;
        set => SetAndPersist(ref _noPowerPointPanelOverlay, value, EasiSettingKeys.NoPowerPointPanelOverlay);
    }

    public int PowerPointRenderTimeoutSeconds
    {
        get => _powerPointRenderTimeoutSeconds;
        set => SetAndPersist(ref _powerPointRenderTimeoutSeconds, value, EasiSettingKeys.PowerPointRenderTimeoutSeconds);
    }

    public int ThumbnailCacheMegabytes
    {
        get => _thumbnailCacheMegabytes;
        set => SetAndPersist(ref _thumbnailCacheMegabytes, value, EasiSettingKeys.ThumbnailCacheMegabytes);
    }

    public int PowerPointMaxFiles
    {
        get => _powerPointMaxFiles;
        set => SetAndPersist(ref _powerPointMaxFiles, value, EasiSettingKeys.PowerPointMaxFiles);
    }

    public bool UseMediaTab
    {
        get => _useMediaTab;
        set => SetAndPersist(ref _useMediaTab, value, EasiSettingKeys.UseMediaTab);
    }

    public bool NoMediaPanelOverlay
    {
        get => _noMediaPanelOverlay;
        set => SetAndPersist(ref _noMediaPanelOverlay, value, EasiSettingKeys.NoMediaPanelOverlay);
    }

    public string MediaDirectory
    {
        get => _mediaDirectory;
        set => SetAndPersist(ref _mediaDirectory, value, EasiSettingKeys.MediaDirectory);
    }

    public double MediaVolume
    {
        get => _mediaVolume;
        set => SetAndPersist(ref _mediaVolume, value, EasiSettingKeys.MediaVolume);
    }

    public double MediaBalance
    {
        get => _mediaBalance;
        set => SetAndPersist(ref _mediaBalance, value, EasiSettingKeys.MediaBalance);
    }

    public bool MediaMuted
    {
        get => _mediaMuted;
        set => SetAndPersist(ref _mediaMuted, value, EasiSettingKeys.MediaMuted);
    }

    public int LiveCameraNumber
    {
        get => _liveCameraNumber;
        set => SetAndPersist(ref _liveCameraNumber, value, EasiSettingKeys.LiveCameraNumber);
    }

    public string AdminDatabasePath
    {
        get => _adminDatabasePath;
        set
        {
            SetAndPersist(ref _adminDatabasePath, value, EasiSettingKeys.AdminDatabasePath);
            AnalyzeDatabaseCommand.NotifyCanExecuteChanged();
        }
    }

    public string DataBackupRoot
    {
        get => _dataBackupRoot;
        set => SetAndPersist(ref _dataBackupRoot, value, EasiSettingKeys.DataBackupRoot);
    }

    public bool EnableDiagnostics
    {
        get => _enableDiagnostics;
        set => SetAndPersist(ref _enableDiagnostics, value, EasiSettingKeys.EnableDiagnostics);
    }

    public string SettingsTransferPath
    {
        get => _settingsTransferPath;
        set
        {
            if (SetProperty(ref _settingsTransferPath, value))
            {
                ExportSettingsCommand.NotifyCanExecuteChanged();
                ImportSettingsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string DatabaseAnalysisSummary
    {
        get => _databaseAnalysisSummary;
        private set => SetProperty(ref _databaseAnalysisSummary, value);
    }

    public void RestoreDefaults()
    {
        var result = _settings.RestoreDefaults();
        ApplyResult(result, "기본값으로 복원됨");
        RefreshFromSettings(applyAppearance: true);
    }

    public async Task AnalyzeDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(AdminDatabasePath))
        {
            SetIssues([new SettingsIssue(EasiSettingKeys.AdminDatabasePath.Id, SettingsIssueSeverity.Error, "DB 경로를 입력하세요.")]);
            StatusMessage = "DB 분석 실패";
            DatabaseAnalysisSummary = "DB 경로 없음";
            return;
        }

        var analysis = await _databaseMigration.AnalyzeAsync(AdminDatabasePath);
        DatabaseTables.Clear();
        foreach (var table in analysis.Tables)
        {
            DatabaseTables.Add(table);
        }

        if (analysis.Succeeded)
        {
            ValidationMessages.Clear();
            DatabaseAnalysisSummary = string.Format(
                CultureInfo.InvariantCulture,
                "schema {0}, tables {1}",
                analysis.SchemaVersion,
                analysis.Tables.Count);
            StatusMessage = "DB 분석 완료";
            return;
        }

        var issues = analysis.Issues.Select(issue => new SettingsIssue(
            EasiSettingKeys.AdminDatabasePath.Id,
            issue.Severity == DatabaseMigrationIssueSeverity.Error
                ? SettingsIssueSeverity.Error
                : SettingsIssueSeverity.Warning,
            issue.Message));
        SetIssues(issues);
        DatabaseAnalysisSummary = "DB 분석 실패";
        StatusMessage = "DB 분석 실패";
    }

    public async Task ExportSettingsAsync()
    {
        var result = await _settings.ExportAsync(SettingsTransferPath);
        ApplyResult(result, "설정 내보내기 완료");
    }

    public async Task ImportSettingsAsync()
    {
        var result = await _settings.ImportAsync(SettingsTransferPath);
        ApplyResult(result, "설정 가져오기 완료");
        if (result.Succeeded)
        {
            RefreshFromSettings(applyAppearance: true);
        }
    }

    public Task BrowseWorkingFolderAsync()
        => BrowsePersistedPathAsync(
            () => _pathPicker.PickWorkingFolderAsync(WorkingFolder),
            value => WorkingFolder = value,
            "작업 폴더 선택됨");

    public Task BrowseAdminDatabaseAsync()
        => BrowsePersistedPathAsync(
            () => _pathPicker.PickAdminDatabaseAsync(AdminDatabasePath),
            value => AdminDatabasePath = value,
            "AdminDB 경로 선택됨");

    public Task BrowseDataBackupRootAsync()
        => BrowsePersistedPathAsync(
            () => _pathPicker.PickDataBackupRootAsync(DataBackupRoot),
            value => DataBackupRoot = value,
            "백업 루트 선택됨");

    public Task BrowseSettingsImportAsync()
        => BrowseTransferPathAsync(
            () => _pathPicker.PickSettingsImportAsync(SettingsTransferPath),
            "가져오기 파일 선택됨");

    public Task BrowseSettingsExportAsync()
        => BrowseTransferPathAsync(
            () => _pathPicker.PickSettingsExportAsync(SettingsTransferPath),
            "내보내기 파일 선택됨");

    public SettingsResult SaveShortcut(ShortcutEditorItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!ShortcutSettings.TryParseGesture(item.CustomGesture, out _, out _, out var parseError))
        {
            var issue = new SettingsIssue($"shortcuts.{item.SlotId}", SettingsIssueSeverity.Error, parseError);
            SetIssues([issue]);
            StatusMessage = "단축키 저장 실패";
            return SettingsResult.Failure([issue]);
        }

        var normalized = ShortcutSettings.NormalizeGesture(item.CustomGesture);
        var collision = ShortcutItems.FirstOrDefault(other =>
            !ReferenceEquals(other, item) &&
            string.Equals(other.EffectiveGesture, normalized, StringComparison.OrdinalIgnoreCase));
        if (collision is not null)
        {
            var issue = new SettingsIssue(
                $"shortcuts.{item.SlotId}",
                SettingsIssueSeverity.Error,
                $"단축키 충돌: {collision.DisplayName} ({collision.ScopeText})");
            SetIssues([issue]);
            StatusMessage = "단축키 저장 실패";
            return SettingsResult.Failure([issue]);
        }

        item.CustomGesture = normalized;
        var result = _settings.SetShortcutOverride(item.SlotId, normalized);
        ApplyResult(result, "단축키 저장됨");
        return result;
    }

    public SettingsResult ResetShortcut(ShortcutEditorItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var result = _settings.ResetShortcutOverride(item.SlotId);
        if (result.Succeeded)
        {
            item.CustomGesture = "";
        }

        ApplyResult(result, "단축키 기본값 복원됨");
        return result;
    }

    private static SettingsSectionViewModel[] CreateSections()
        =>
        [
            new(SettingsSectionKind.General, "일반", "언어와 작업 폴더", EsIcons.Settings),
            new(SettingsSectionKind.Appearance, "화면", "테마와 인터페이스 크기", EsIcons.Template),
            new(SettingsSectionKind.LiveOutput, "송출", "출력 모니터와 라이브 동작", EsIcons.MonitorDual),
            new(SettingsSectionKind.PowerPoint, "PowerPoint", "탭 표시와 렌더링", EsIcons.PowerPointSlide),
            new(SettingsSectionKind.Media, "미디어", "탭 표시와 재생", EsIcons.MediaFile),
            new(SettingsSectionKind.Shortcuts, "단축키", "키보드와 리모컨", EsIcons.Shortcuts),
            new(SettingsSectionKind.Data, "데이터", "AdminDB와 백업", EsIcons.FolderOpen),
            new(SettingsSectionKind.ImportExport, "가져오기/내보내기", "설정 파일 이동", EsIcons.ExportRtf),
            new(SettingsSectionKind.Advanced, "고급", "진단과 호환성", EsIcons.Info),
        ];

    private void RefreshFromSettings(bool applyAppearance)
    {
        var current = _settings.Current;
        _isRefreshing = true;
        try
        {
            Language = current.General.Language;
            WorkingFolder = current.General.WorkingFolder;
            Theme = current.Appearance.Theme;
            InterfaceSize = current.Appearance.InterfaceSize;
            DefaultOutputMonitorId = current.LiveOutput.DefaultOutputMonitorId;
            UseSafetyConfirmations = current.LiveOutput.UseSafetyConfirmations;
            ShowLyricsMonitorAlertBox = current.LiveOutput.ShowLyricsMonitorAlertBox;
            AdvanceNextItem = current.LiveOutput.AdvanceNextItem;
            GapItemOption = current.LiveOutput.GapItemOption;
            GapItemLogoFile = current.LiveOutput.GapItemLogoFile;
            GapItemUseFade = current.LiveOutput.GapItemUseFade;
            DisplayAlwaysUseSecondaryMonitor = current.LiveOutput.DisplayAlwaysUseSecondaryMonitor;
            DisplayCustomTop = current.LiveOutput.DisplayCustomTop;
            DisplayCustomLeft = current.LiveOutput.DisplayCustomLeft;
            DisplayCustomWidth = current.LiveOutput.DisplayCustomWidth;
            LyricsMonitorTextColorArgb = current.LiveOutput.LyricsMonitorTextColorArgb;
            LyricsMonitorBackgroundColorArgb = current.LiveOutput.LyricsMonitorBackgroundColorArgb;
            LyricsMonitorShowNotations = current.LiveOutput.LyricsMonitorShowNotations;
            UsePowerPointTab = current.PowerPoint.UsePowerPointTab;
            NoPowerPointPanelOverlay = current.PowerPoint.NoPanelOverlay;
            PowerPointRenderTimeoutSeconds = current.PowerPoint.RenderTimeoutSeconds;
            ThumbnailCacheMegabytes = current.PowerPoint.ThumbnailCacheMegabytes;
            PowerPointMaxFiles = current.PowerPoint.MaxFiles;
            UseMediaTab = current.Media.UseMediaTab;
            NoMediaPanelOverlay = current.Media.NoPanelOverlay;
            MediaDirectory = current.Media.Directory;
            MediaVolume = current.Media.Volume;
            MediaBalance = current.Media.Balance;
            MediaMuted = current.Media.Muted;
            LiveCameraNumber = current.Media.LiveCameraNumber;
            AdminDatabasePath = current.Data.AdminDatabasePath;
            DataBackupRoot = current.Data.BackupRoot;
            EnableDiagnostics = current.Advanced.EnableDiagnostics;
            RefreshShortcutItems(current.Shortcuts);
        }
        finally
        {
            _isRefreshing = false;
        }

        if (applyAppearance)
        {
            _theme.ApplyTheme(current.Appearance.Theme);
            _theme.ApplyInterfaceSize(current.Appearance.InterfaceSize);
        }

        AnalyzeDatabaseCommand.NotifyCanExecuteChanged();
    }

    private void SetAndPersist<T>(
        ref T field,
        T value,
        SettingKey<T> key,
        Action<T>? afterSuccess = null)
    {
        if (!SetProperty(ref field, value) || _isRefreshing)
        {
            return;
        }

        var result = _settings.Set(key, value);
        if (result.Succeeded)
        {
            ApplyResult(result, "설정 저장됨");
            afterSuccess?.Invoke(value);
            return;
        }

        ApplyResult(result, "설정 저장 실패");
        RefreshFromSettings(applyAppearance: false);
    }

    private void ApplyResult(SettingsResult result, string successMessage)
    {
        SetIssues(result.Issues);
        StatusMessage = result.Succeeded ? successMessage : "설정 저장 실패";
    }

    private void RefreshShortcutItems(IReadOnlyDictionary<string, string> overrides)
    {
        ShortcutItems.Clear();
        foreach (var command in _commandCatalog.All)
        {
            foreach (var shortcut in command.DefaultShortcuts)
            {
                var slotId = ShortcutSettings.GetSlotId(shortcut);
                overrides.TryGetValue(slotId, out var customGesture);
                ShortcutItems.Add(new ShortcutEditorItemViewModel(
                    slotId,
                    command.Id,
                    command.Category,
                    command.DisplayName,
                    command.Description,
                    command.IsDangerous,
                    shortcut.IsGlobal,
                    shortcut.DisplayText,
                    customGesture));
            }
        }
    }

    private async Task BrowsePersistedPathAsync(
        Func<Task<string?>> pickPath,
        Action<string> applyPath,
        string successMessage)
    {
        var selectedPath = await pickPath();
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            StatusMessage = "경로 선택 취소됨";
            return;
        }

        applyPath(selectedPath);
        if (ValidationMessages.Count == 0)
        {
            StatusMessage = successMessage;
        }
    }

    private async Task BrowseTransferPathAsync(Func<Task<string?>> pickPath, string successMessage)
    {
        var selectedPath = await pickPath();
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            StatusMessage = "경로 선택 취소됨";
            return;
        }

        SettingsTransferPath = selectedPath;
        StatusMessage = successMessage;
    }

    private void SetIssues(IEnumerable<SettingsIssue> issues)
    {
        ValidationMessages.Clear();
        foreach (var issue in issues)
        {
            ValidationMessages.Add($"{issue.Key}: {issue.Message}");
        }
    }

    private bool CanAnalyzeDatabase()
        => !string.IsNullOrWhiteSpace(AdminDatabasePath);

    private bool CanUseTransferPath()
        => !string.IsNullOrWhiteSpace(SettingsTransferPath);
}
