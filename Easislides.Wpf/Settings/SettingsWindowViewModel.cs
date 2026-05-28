using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
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

public sealed partial class SettingsWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IThemeService _theme;
    private readonly IDatabaseMigrationService _databaseMigration;
    private bool _isRefreshing;

    private SettingsSectionViewModel _selectedSection;
    private string _language = "";
    private string _workingFolder = "";
    private ColorTheme _themeValue;
    private InterfaceSize _interfaceSize;
    private string _defaultOutputMonitorId = "";
    private bool _useSafetyConfirmations;
    private int _powerPointRenderTimeoutSeconds;
    private int _thumbnailCacheMegabytes;
    private double _mediaVolume;
    private double _mediaBalance;
    private bool _mediaMuted;
    private string _adminDatabasePath = "";
    private string _dataBackupRoot = "";
    private bool _enableDiagnostics;
    private string _settingsTransferPath = "";
    private string _statusMessage = "설정 준비됨";
    private string _databaseAnalysisSummary = "DB 분석 대기";

    public SettingsWindowViewModel(
        ISettingsService settings,
        IThemeService theme,
        IDatabaseMigrationService databaseMigration)
    {
        _settings = settings;
        _theme = theme;
        _databaseMigration = databaseMigration;

        Sections = new ObservableCollection<SettingsSectionViewModel>(CreateSections());
        _selectedSection = Sections[0];

        RestoreDefaultsCommand = new RelayCommand(RestoreDefaults);
        RefreshCommand = new RelayCommand(() => RefreshFromSettings(applyAppearance: true));
        AnalyzeDatabaseCommand = new AsyncRelayCommand(AnalyzeDatabaseAsync, CanAnalyzeDatabase);
        ExportSettingsCommand = new AsyncRelayCommand(ExportSettingsAsync, CanUseTransferPath);
        ImportSettingsCommand = new AsyncRelayCommand(ImportSettingsAsync, CanUseTransferPath);

        RefreshFromSettings(applyAppearance: true);
    }

    public ObservableCollection<SettingsSectionViewModel> Sections { get; }

    public ObservableCollection<string> ValidationMessages { get; } = new();

    public ObservableCollection<DatabaseTable> DatabaseTables { get; } = new();

    public IReadOnlyList<ColorTheme> ThemeOptions { get; } = Enum.GetValues<ColorTheme>();

    public IReadOnlyList<InterfaceSize> InterfaceSizeOptions { get; } = Enum.GetValues<InterfaceSize>();

    public IRelayCommand RestoreDefaultsCommand { get; }

    public IRelayCommand RefreshCommand { get; }

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

    private static SettingsSectionViewModel[] CreateSections()
        =>
        [
            new(SettingsSectionKind.General, "일반", "언어와 작업 폴더", EsIcons.Settings),
            new(SettingsSectionKind.Appearance, "화면", "테마와 인터페이스 크기", EsIcons.Template),
            new(SettingsSectionKind.LiveOutput, "송출", "출력 모니터와 안전 확인", EsIcons.MonitorDual),
            new(SettingsSectionKind.PowerPoint, "PowerPoint", "렌더링과 썸네일 캐시", EsIcons.PowerPointSlide),
            new(SettingsSectionKind.Media, "미디어", "재생 볼륨과 음소거", EsIcons.MediaFile),
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
            PowerPointRenderTimeoutSeconds = current.PowerPoint.RenderTimeoutSeconds;
            ThumbnailCacheMegabytes = current.PowerPoint.ThumbnailCacheMegabytes;
            MediaVolume = current.Media.Volume;
            MediaBalance = current.Media.Balance;
            MediaMuted = current.Media.Muted;
            AdminDatabasePath = current.Data.AdminDatabasePath;
            DataBackupRoot = current.Data.BackupRoot;
            EnableDiagnostics = current.Advanced.EnableDiagnostics;
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
