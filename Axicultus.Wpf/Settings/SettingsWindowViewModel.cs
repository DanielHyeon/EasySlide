using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Theme;
using Wpf.Ui.Controls;

namespace Axicultus.Wpf.Settings;

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
    DeviceCheck,
    MainWindow,
    Show,
    Monitors,
    Alerts,
    Folders,
    Bibles,
    Licence,
    WpfTools,
}

public sealed record SettingsSectionViewModel(
    SettingsSectionKind Kind,
    string Title,
    string Description,
    SymbolRegular Symbol);

public sealed partial class ShortcutEditorItemViewModel(
    string slotId,
    string commandId,
    string category,
    string displayName,
    string description,
    bool isDangerous,
    bool isGlobal,
    string defaultGesture,
    string? customGesture) : ObservableObject
{
    private string _customGesture = customGesture ?? "";

    public string SlotId { get; } = slotId;

    public string CommandId { get; } = commandId;

    public string Category { get; } = category;

    public string DisplayName { get; } = displayName;

    public string Description { get; } = description;

    public bool IsDangerous { get; } = isDangerous;

    public bool IsGlobal { get; } = isGlobal;

    public string ScopeText => IsGlobal ? "전역" : "로컬";

    public string DefaultGesture { get; } = defaultGesture;

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
    private ISettingsService _settings;
    private readonly ISettingsService _applicationSettings;
    private readonly IThemeService _theme;
    private readonly IDatabaseMigrationService _databaseMigration;
    private readonly ISettingsPathPicker _pathPicker;
    private readonly ICommandCatalog _commandCatalog;
    private readonly IOperationalDataRehearsalService _operationalDataRehearsal;
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
    private bool _useCustomOutputBounds;
    private bool _isMonitorWide;
    private bool _fitTextFileParagraphs;
    private bool _autoTextOverflow;
    private int _displayCustomWidth;
    private int _lyricsMonitorTextColorArgb;
    private int _lyricsMonitorBackgroundColorArgb;
    private int _lyricsMonitorBackgroundColor2Argb;
    private bool _lyricsMonitorBackgroundIsGradient;
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
    private string _operationalRehearsalSummary = "운영 데이터 리허설 대기";

    public SettingsWindowViewModel(
        ISettingsService settings,
        IThemeService theme,
        IDatabaseMigrationService databaseMigration,
        ISettingsPathPicker pathPicker,
        ICommandCatalog commandCatalog,
        IOperationalDataRehearsalService operationalDataRehearsal)
    {
        _applicationSettings = _settings = settings;
        _theme = theme;
        _databaseMigration = databaseMigration;
        _pathPicker = pathPicker;
        _commandCatalog = commandCatalog;
        _operationalDataRehearsal = operationalDataRehearsal;

        Sections = [with(CreateSections())];
        _selectedSection = Sections[0];

        RestoreDefaultsCommand = new RelayCommand(RestoreDefaults);
        RefreshCommand = new RelayCommand(ReloadOptions);
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
        RunOperationalDataRehearsalCommand = new AsyncRelayCommand(RunOperationalDataRehearsalAsync);
        ExportSettingsCommand = new AsyncRelayCommand(ExportSettingsAsync, CanUseTransferPath);
        ImportSettingsCommand = new AsyncRelayCommand(ImportSettingsAsync, CanUseTransferPath);

        RefreshFromSettings(applyAppearance: true);
    }

    public ObservableCollection<SettingsSectionViewModel> Sections { get; }

    public ObservableCollection<string> ValidationMessages { get; } = [];

    public ObservableCollection<DatabaseTable> DatabaseTables { get; } = [];

    public ObservableCollection<ShortcutEditorItemViewModel> ShortcutItems { get; } = [];

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

    public IAsyncRelayCommand RunOperationalDataRehearsalCommand { get; }

    public IAsyncRelayCommand ExportSettingsCommand { get; }

    public IAsyncRelayCommand ImportSettingsCommand { get; }

    public SettingsSectionViewModel SelectedSection
    {
        get => _selectedSection;
        set { if (SetProperty(ref _selectedSection, value)) OnPropertyChanged(nameof(SelectedTabIndex)); }
    }

    public int SelectedTabIndex
    {
        get => Sections.IndexOf(SelectedSection);
        set { if (value >= 0 && value < Sections.Count) SelectedSection = Sections[value]; }
    }

    public string Language
    {
        get => _language;
        set => SetAndPersist(ref _language, value, AxicultusSettingKeys.Language);
    }

    public string WorkingFolder
    {
        get => _workingFolder;
        set => SetAndPersist(ref _workingFolder, value, AxicultusSettingKeys.WorkingFolder);
    }

    public ColorTheme Theme
    {
        get => _themeValue;
        set => SetAndPersist(ref _themeValue, value, AxicultusSettingKeys.Theme, _theme.ApplyTheme);
    }

    public InterfaceSize InterfaceSize
    {
        get => _interfaceSize;
        set => SetAndPersist(ref _interfaceSize, value, AxicultusSettingKeys.InterfaceSize, _theme.ApplyInterfaceSize);
    }

    public string DefaultOutputMonitorId
    {
        get => _defaultOutputMonitorId;
        set => SetAndPersist(ref _defaultOutputMonitorId, value, AxicultusSettingKeys.DefaultOutputMonitorId);
    }

    public bool UseSafetyConfirmations
    {
        get => _useSafetyConfirmations;
        set => SetAndPersist(ref _useSafetyConfirmations, value, AxicultusSettingKeys.UseSafetyConfirmations);
    }

    public bool ShowLyricsMonitorAlertBox
    {
        get => _showLyricsMonitorAlertBox;
        set => SetAndPersist(ref _showLyricsMonitorAlertBox, value, AxicultusSettingKeys.ShowLyricsMonitorAlertBox);
    }

    public bool AdvanceNextItem
    {
        get => _advanceNextItem;
        set => SetAndPersist(ref _advanceNextItem, value, AxicultusSettingKeys.AdvanceNextItem);
    }

    public GapItemMode GapItemOption
    {
        get => _gapItemOption;
        set => SetAndPersist(ref _gapItemOption, value, AxicultusSettingKeys.GapItemOption);
    }

    public string GapItemLogoFile
    {
        get => _gapItemLogoFile;
        set => SetAndPersist(ref _gapItemLogoFile, value, AxicultusSettingKeys.GapItemLogoFile);
    }

    public bool GapItemUseFade
    {
        get => _gapItemUseFade;
        set => SetAndPersist(ref _gapItemUseFade, value, AxicultusSettingKeys.GapItemUseFade);
    }

    public bool DisplayAlwaysUseSecondaryMonitor
    {
        get => _displayAlwaysUseSecondaryMonitor;
        set => SetAndPersist(ref _displayAlwaysUseSecondaryMonitor, value, AxicultusSettingKeys.DisplayAlwaysUseSecondaryMonitor);
    }

    public int DisplayCustomTop
    {
        get => _displayCustomTop;
        set => SetAndPersist(ref _displayCustomTop, value, AxicultusSettingKeys.DisplayCustomTop);
    }

    public int DisplayCustomLeft
    {
        get => _displayCustomLeft;
        set => SetAndPersist(ref _displayCustomLeft, value, AxicultusSettingKeys.DisplayCustomLeft);
    }

    public bool UseCustomOutputBounds
    {
        get => _useCustomOutputBounds;
        set
        {
            if (SetProperty(ref _useCustomOutputBounds, value) && !_isRefreshing)
                _settings.Set(AxicultusSettingKeys.OutputPlacementMode, value ? 1 : 0);
        }
    }

    public bool IsMonitorWide
    {
        get => _isMonitorWide;
        set { SetAndPersist(ref _isMonitorWide, value, AxicultusSettingKeys.IsMonitorWide); OnPropertyChanged(nameof(StageHeight)); OnPropertyChanged(nameof(AudienceHeight)); }
    }

    public bool FitTextFileParagraphs
    {
        get => _fitTextFileParagraphs;
        set => SetAndPersist(ref _fitTextFileParagraphs, value, AxicultusSettingKeys.FitTextFileParagraphs);
    }

    public bool AutoTextOverflow
    {
        get => _autoTextOverflow;
        set => SetAndPersist(ref _autoTextOverflow, value, AxicultusSettingKeys.AutoTextOverflow);
    }

    public int DisplayCustomWidth
    {
        get => _displayCustomWidth;
        set { SetAndPersist(ref _displayCustomWidth, value, AxicultusSettingKeys.DisplayCustomWidth); OnPropertyChanged(nameof(AudienceHeight)); }
    }

    public int LyricsMonitorTextColorArgb
    {
        get => _lyricsMonitorTextColorArgb;
        set => SetAndPersist(ref _lyricsMonitorTextColorArgb, value, AxicultusSettingKeys.LyricsMonitorTextColorArgb);
    }

    public int LyricsMonitorBackgroundColorArgb
    {
        get => _lyricsMonitorBackgroundColorArgb;
        set => SetAndPersist(ref _lyricsMonitorBackgroundColorArgb, value, AxicultusSettingKeys.LyricsMonitorBackgroundColorArgb);
    }

    /// <summary>배경 그라데이션 끝색(ARGB). 그라데이션 사용 시 배경색→이 색 세로 그라데이션(G2 / FrmBackground 슬라이스).</summary>
    public int LyricsMonitorBackgroundColor2Argb
    {
        get => _lyricsMonitorBackgroundColor2Argb;
        set => SetAndPersist(ref _lyricsMonitorBackgroundColor2Argb, value, AxicultusSettingKeys.LyricsMonitorBackgroundColor2Argb);
    }

    /// <summary>배경 그라데이션 사용 여부(false=솔리드, true=배경색→끝색 세로 그라데이션).</summary>
    public bool LyricsMonitorBackgroundIsGradient
    {
        get => _lyricsMonitorBackgroundIsGradient;
        set => SetAndPersist(ref _lyricsMonitorBackgroundIsGradient, value, AxicultusSettingKeys.LyricsMonitorBackgroundIsGradient);
    }

    public bool LyricsMonitorShowNotations
    {
        get => _lyricsMonitorShowNotations;
        set => SetAndPersist(ref _lyricsMonitorShowNotations, value, AxicultusSettingKeys.LyricsMonitorShowNotations);
    }

    public bool UsePowerPointTab
    {
        get => _usePowerPointTab;
        set => SetAndPersist(ref _usePowerPointTab, value, AxicultusSettingKeys.UsePowerPointTab);
    }

    public bool NoPowerPointPanelOverlay
    {
        get => _noPowerPointPanelOverlay;
        set => SetAndPersist(ref _noPowerPointPanelOverlay, value, AxicultusSettingKeys.NoPowerPointPanelOverlay);
    }

    public int PowerPointRenderTimeoutSeconds
    {
        get => _powerPointRenderTimeoutSeconds;
        set => SetAndPersist(ref _powerPointRenderTimeoutSeconds, value, AxicultusSettingKeys.PowerPointRenderTimeoutSeconds);
    }

    public int ThumbnailCacheMegabytes
    {
        get => _thumbnailCacheMegabytes;
        set => SetAndPersist(ref _thumbnailCacheMegabytes, value, AxicultusSettingKeys.ThumbnailCacheMegabytes);
    }

    public int PowerPointMaxFiles
    {
        get => _powerPointMaxFiles;
        set => SetAndPersist(ref _powerPointMaxFiles, value, AxicultusSettingKeys.PowerPointMaxFiles);
    }

    public bool UseMediaTab
    {
        get => _useMediaTab;
        set => SetAndPersist(ref _useMediaTab, value, AxicultusSettingKeys.UseMediaTab);
    }

    public bool NoMediaPanelOverlay
    {
        get => _noMediaPanelOverlay;
        set => SetAndPersist(ref _noMediaPanelOverlay, value, AxicultusSettingKeys.NoMediaPanelOverlay);
    }

    public string MediaDirectory
    {
        get => _mediaDirectory;
        set => SetAndPersist(ref _mediaDirectory, value, AxicultusSettingKeys.MediaDirectory);
    }

    public double MediaVolume
    {
        get => _mediaVolume;
        set => SetAndPersist(ref _mediaVolume, value, AxicultusSettingKeys.MediaVolume);
    }

    public double MediaBalance
    {
        get => _mediaBalance;
        set => SetAndPersist(ref _mediaBalance, value, AxicultusSettingKeys.MediaBalance);
    }

    public bool MediaMuted
    {
        get => _mediaMuted;
        set => SetAndPersist(ref _mediaMuted, value, AxicultusSettingKeys.MediaMuted);
    }

    public int LiveCameraNumber
    {
        get => _liveCameraNumber;
        set => SetAndPersist(ref _liveCameraNumber, value, AxicultusSettingKeys.LiveCameraNumber);
    }

    public string AdminDatabasePath
    {
        get => _adminDatabasePath;
        set
        {
            SetAndPersist(ref _adminDatabasePath, value, AxicultusSettingKeys.AdminDatabasePath);
            AnalyzeDatabaseCommand.NotifyCanExecuteChanged();
        }
    }

    public string DataBackupRoot
    {
        get => _dataBackupRoot;
        set => SetAndPersist(ref _dataBackupRoot, value, AxicultusSettingKeys.DataBackupRoot);
    }

    public bool EnableDiagnostics
    {
        get => _enableDiagnostics;
        set => SetAndPersist(ref _enableDiagnostics, value, AxicultusSettingKeys.EnableDiagnostics);
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

    public string OperationalRehearsalSummary
    {
        get => _operationalRehearsalSummary;
        private set => SetProperty(ref _operationalRehearsalSummary, value);
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
            SetIssues([new SettingsIssue(AxicultusSettingKeys.AdminDatabasePath.Id, SettingsIssueSeverity.Error, "DB 경로를 입력하세요.")]);
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
            AxicultusSettingKeys.AdminDatabasePath.Id,
            issue.Severity == DatabaseMigrationIssueSeverity.Error
                ? SettingsIssueSeverity.Error
                : SettingsIssueSeverity.Warning,
            issue.Message));
        SetIssues(issues);
        DatabaseAnalysisSummary = "DB 분석 실패";
        StatusMessage = "DB 분석 실패";
    }

    public async Task RunOperationalDataRehearsalAsync()
    {
        var report = await _operationalDataRehearsal.RunAsync(new OperationalDataRehearsalRequest(
            SourceRoot: WorkingFolder,
            BackupRoot: DataBackupRoot,
            AdminDatabasePath: AdminDatabasePath));
        var assetCount = report.AssetReport?.Items.Count ?? 0;
        var tableCount = report.DatabaseInventory?.Tables.Count ?? 0;
        var errorCount = report.Issues.Count(issue => issue.Severity == OperationalDataRehearsalIssueSeverity.Error);
        var warningCount = report.Issues.Count(issue => issue.Severity == OperationalDataRehearsalIssueSeverity.Warning);

        DatabaseTables.Clear();
        if (report.DatabaseInventory is not null)
        {
            foreach (var table in report.DatabaseInventory.Tables)
            {
                DatabaseTables.Add(table);
            }

            DatabaseAnalysisSummary = string.Format(
                CultureInfo.InvariantCulture,
                "schema {0}, tables {1}",
                report.DatabaseInventory.SchemaVersion,
                report.DatabaseInventory.Tables.Count);
        }
        else
        {
            DatabaseAnalysisSummary = "DB inventory 없음";
        }

        SetIssues(report.Issues.Select(issue => new SettingsIssue(
            $"operationalRehearsal.{issue.Kind}",
            issue.Severity == OperationalDataRehearsalIssueSeverity.Error
                ? SettingsIssueSeverity.Error
                : SettingsIssueSeverity.Warning,
            $"{issue.Path}: {issue.Message}")));

        if (report.Succeeded)
        {
            OperationalRehearsalSummary = string.Format(
                CultureInfo.InvariantCulture,
                "운영 데이터 리허설 완료: 파일 {0}개, 테이블 {1}개, 경고 {2}개",
                assetCount,
                tableCount,
                warningCount);
            StatusMessage = "운영 데이터 리허설 완료";
            return;
        }

        OperationalRehearsalSummary = string.Format(
            CultureInfo.InvariantCulture,
            "운영 데이터 리허설 실패: 파일 {0}개, 오류 {1}개, 경고 {2}개",
            assetCount,
            errorCount,
            warningCount);
        StatusMessage = "운영 데이터 리허설 실패";
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
            new(SettingsSectionKind.MainWindow, "메인 화면", "미리보기와 파일 목록", EsIcons.Settings),
            new(SettingsSectionKind.Show, "송출", "텍스트와 항목 전환", EsIcons.PowerPointSlide),
            new(SettingsSectionKind.Monitors, "모니터", "화면 배치와 라이브 카메라", EsIcons.MonitorDual),
            new(SettingsSectionKind.Alerts, "알림", "회중, 보호자 호출 및 항목 참조", EsIcons.Info),
            new(SettingsSectionKind.Folders, "폴더", "찬양 폴더", EsIcons.FolderOpen),
            new(SettingsSectionKind.Bibles, "성경", "성경 버전 관리", EsIcons.FolderOpen),
            new(SettingsSectionKind.Licence, "라이선스", "관리자와 번호", EsIcons.Info),
            new(SettingsSectionKind.Shortcuts, "키보드", "단축키와 전역 키", EsIcons.Shortcuts),
            new(SettingsSectionKind.WpfTools, "WPF 설정", "일반, 화면 및 데이터 관리", EsIcons.Settings),
            new(SettingsSectionKind.DeviceCheck, "장치 점검", "카메라, 마이크 및 OBS 연결 상태", SymbolRegular.Video24),
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
            UseCustomOutputBounds = Platform.WindowPlacementService.UsesCustomBounds(current.LiveOutput);
            IsMonitorWide = current.LiveOutput.IsMonitorWide;
            FitTextFileParagraphs = current.LiveOutput.FitTextFileParagraphs;
            AutoTextOverflow = current.LiveOutput.AutoTextOverflow;
            DisplayCustomWidth = current.LiveOutput.DisplayCustomWidth;
            LyricsMonitorTextColorArgb = current.LiveOutput.LyricsMonitorTextColorArgb;
            LyricsMonitorBackgroundColorArgb = current.LiveOutput.LyricsMonitorBackgroundColorArgb;
            LyricsMonitorBackgroundColor2Argb = current.LiveOutput.LyricsMonitorBackgroundColor2Argb;
            LyricsMonitorBackgroundIsGradient = current.LiveOutput.LyricsMonitorBackgroundIsGradient;
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

        if (applyAppearance && !IsEditing)
        {
            _theme.ApplyTheme(current.Appearance.Theme);
            _theme.ApplyInterfaceSize(current.Appearance.InterfaceSize);
        }

        if (IsEditing) RefreshAdditionalOptions();
        AnalyzeDatabaseCommand.NotifyCanExecuteChanged();
    }

    private void SetAndPersist<T>(
        ref T field,
        T value,
        SettingKey<T> key,
        Action<T>? afterSuccess = null,
        [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName) || _isRefreshing)
        {
            return;
        }

        var result = _settings.Set(key, value);
        if (result.Succeeded)
        {
            ApplyResult(result, IsEditing ? "확인을 누르면 설정을 저장합니다." : "설정 저장됨");
            if (!IsEditing) afterSuccess?.Invoke(value);
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

    private void RefreshShortcutItems(Dictionary<string, string> overrides)
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
