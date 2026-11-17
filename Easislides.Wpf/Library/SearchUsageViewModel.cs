using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed partial class SelectableSearchFolder : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public SelectableSearchFolder(SongFolderSummary folder)
    {
        Folder = folder;
    }

    public SongFolderSummary Folder { get; }

    public int FolderNo => Folder.FolderNo;

    public string Name => Folder.Name;

    public int SongCount => Folder.SongCount;
}

public sealed partial class SelectableUsageRecord : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public SelectableUsageRecord(UsageRecord record)
    {
        Record = record;
    }

    public UsageRecord Record { get; }

    public long RecordId => Record.RecordId;

    public DateTime WorshipDate => Record.WorshipDate;

    public string WorshipList => Record.WorshipList;

    public string SongTitle => Record.SongTitle;

    public int SongNumber => Record.SongNumber;

    public string Admin1 => Record.Admin1;

    public string Admin2 => Record.Admin2;
}

public sealed partial class SearchUsageViewModel : ObservableObject
{
    private const string LegacyAdminDatabaseRelativePath = @"Admin\Database\EasiSlidesDb.db";

    private readonly ISettingsService _settings;
    private readonly ISearchUsageService _service;
    private readonly IUsageDeleteConfirmation _deleteConfirmation;
    private UsageReport? _currentUsageReport;

    [ObservableProperty] private string _workingFolder = "";
    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private string _usageDatabasePath = "";
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _searchKey = "";
    [ObservableProperty] private string _searchTiming = "";
    [ObservableProperty] private bool _searchTitle = true;
    [ObservableProperty] private bool _searchLyrics = true;
    [ObservableProperty] private bool _searchSongNumber = true;
    [ObservableProperty] private bool _searchBookReference;
    [ObservableProperty] private bool _searchUserReference;
    [ObservableProperty] private bool _searchLicenceAdmin;
    [ObservableProperty] private bool _searchWriter;
    [ObservableProperty] private bool _searchCopyright;
    [ObservableProperty] private bool _notationsOnly;
    [ObservableProperty] private bool _mediaOnly;
    [ObservableProperty] private bool _useModifiedDates;
    [ObservableProperty] private DateTime _modifiedFrom = DateTime.Today.AddMonths(-3);
    [ObservableProperty] private DateTime _modifiedTo = DateTime.Today;
    [ObservableProperty] private DateTime _usageFrom = DateTime.Today.AddDays(-91);
    [ObservableProperty] private DateTime _usageTo = DateTime.Today;
    [ObservableProperty] private string _selectedUsageSession = "";
    [ObservableProperty] private string _usageReportOutputPath = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;

    /// <summary>한 번이라도 곡 검색을 실행했는지 — 검색 전 빈 목록과 "검색했는데 결과 없음"을 구분(빈 상태 안내가 검색 전엔 안 뜨도록).</summary>
    [ObservableProperty] private bool _hasSearched;

    /// <summary>
    /// 검색을 실행했지만 결과가 하나도 없을 때만 true — 검색 결과 목록의 "결과 없음" 빈 상태 안내 표시에 쓴다.
    /// 검색 전(HasSearched=false)이나 결과가 있을 때는 false 라, 처음 화면이나 정상 결과에는 안내가 뜨지 않는다.
    /// </summary>
    public bool HasNoSearchResults => HasSearched && SearchResults.Count == 0;

    /// <summary>HasSearched 가 바뀌면 빈 상태 표시 여부도 다시 평가하도록 통지.</summary>
    partial void OnHasSearchedChanged(bool value) => OnPropertyChanged(nameof(HasNoSearchResults));

    /// <summary>한 번이라도 제목 조회를 실행했는지 — 조회 전 빈 목록과 "조회했는데 결과 없음"을 구분(곡 검색 HasSearched 와 같은 취지).</summary>
    [ObservableProperty] private bool _hasLookedUp;

    /// <summary>
    /// 제목 조회를 실행했지만 후보가 하나도 없을 때만 true — "제목" 서브탭의 "결과 없음" 빈 상태 안내 표시에 쓴다(곡 검색 HasNoSearchResults 와 대칭).
    /// 조회 전(HasLookedUp=false)이나 후보가 있을 때는 false 라, 처음 화면이나 정상 결과에는 안내가 뜨지 않는다.
    /// </summary>
    public bool HasNoLookupResults => HasLookedUp && LookupCandidates.Count == 0;

    /// <summary>HasLookedUp 가 바뀌면 빈 상태 표시 여부도 다시 평가하도록 통지.</summary>
    partial void OnHasLookedUpChanged(bool value) => OnPropertyChanged(nameof(HasNoLookupResults));

    public SearchUsageViewModel(ISettingsService settings, ISearchUsageService service)
        : this(settings, service, new WpfUsageDeleteConfirmation())
    {
    }

    public SearchUsageViewModel(
        ISettingsService settings,
        ISearchUsageService service,
        IUsageDeleteConfirmation deleteConfirmation)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _deleteConfirmation = deleteConfirmation ?? throw new ArgumentNullException(nameof(deleteConfirmation));
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        SearchSongsCommand = new AsyncRelayCommand(SearchSongsAsync, () => !IsBusy);
        LookupTitlesCommand = new AsyncRelayCommand(LookupTitlesAsync, () => !IsBusy);
        RefreshUsageCommand = new AsyncRelayCommand(RefreshUsageAsync, () => !IsBusy);
        DeleteSelectedUsageCommand = new AsyncRelayCommand(DeleteSelectedUsageAsync, () => !IsBusy);
        ExportUsageReportCommand = new AsyncRelayCommand(ExportUsageReportAsync, () => !IsBusy);
        // 검색 결과 수가 바뀌면 "결과 없음" 빈 상태 표시 여부도 다시 평가하도록 통지.
        SearchResults.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoSearchResults));
        // 제목 조회 후보 수가 바뀌면 "제목" 서브탭 빈 상태 표시 여부도 다시 평가하도록 통지.
        LookupCandidates.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoLookupResults));
    }

    public ObservableCollection<SelectableSearchFolder> SearchFolders { get; } = new();

    public ObservableCollection<SongSearchResult> SearchResults { get; } = new();

    public ObservableCollection<LookupTitleCandidate> LookupCandidates { get; } = new();

    public ObservableCollection<string> UsageSessions { get; } = new();

    public ObservableCollection<SelectableUsageRecord> UsageRecords { get; } = new();

    public ObservableCollection<UsageSummary> UsageSummary { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SearchSongsCommand { get; }

    public IAsyncRelayCommand LookupTitlesCommand { get; }

    public IAsyncRelayCommand RefreshUsageCommand { get; }

    public IAsyncRelayCommand DeleteSelectedUsageCommand { get; }

    public IAsyncRelayCommand ExportUsageReportCommand { get; }

    public async Task LoadAsync()
    {
        WorkingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        DatabasePath = ResolveDatabasePath();
        UsageDatabasePath = _service.GetDefaultUsageDatabasePath(WorkingFolder);
        UsageReportOutputPath = string.IsNullOrWhiteSpace(WorkingFolder)
            ? ""
            : NormalizePath(Path.Combine(WorkingFolder, "Documents", "Song Usages.rtf"));

        await RunBusyAsync(async () =>
        {
            var folders = (await _service.GetFoldersAsync(DatabasePath).ConfigureAwait(true))
                .Where(folder => folder.IsEnabled)
                .OrderBy(folder => folder.FolderNo)
                .Select(folder => new SelectableSearchFolder(folder))
                .ToArray();
            SearchFolders.ReplaceWith(folders);
            StatusMessage = $"{SearchFolders.Count} search folders loaded.";
        }).ConfigureAwait(true);
    }

    public async Task SearchSongsAsync()
    {
        ValidationMessage = "";
        if (!EnsureSearchDatabase())
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var request = new SongSearchRequest(
                DatabasePath,
                SearchText,
                SelectedFolderNos(),
                BuildSearchFields(),
                SearchKey,
                SearchTiming,
                NotationsOnly,
                MediaOnly,
                UseModifiedDates ? ModifiedFrom : null,
                UseModifiedDates ? ModifiedTo : null);
            var results = await _service.SearchSongsAsync(request).ConfigureAwait(true);
            SearchResults.ReplaceWith(results);
            HasSearched = true; // 검색을 실행했음을 표시 — 이제부터 결과 0건이면 "결과 없음" 안내가 뜬다.
            StatusMessage = $"{SearchResults.Count} songs found.";
        }).ConfigureAwait(true);
    }

    public async Task LookupTitlesAsync()
    {
        ValidationMessage = "";
        if (!EnsureSearchDatabase())
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var candidates = await _service.LookupTitlesAsync(DatabasePath, SearchText).ConfigureAwait(true);
            LookupCandidates.ReplaceWith(candidates);
            HasLookedUp = true; // 조회를 실행했음을 표시 — 이제부터 후보 0건이면 "결과 없음" 안내가 뜬다.
            StatusMessage = $"{LookupCandidates.Count} title candidates found.";
        }).ConfigureAwait(true);
    }

    public async Task RefreshUsageAsync()
    {
        ValidationMessage = "";
        if (string.IsNullOrWhiteSpace(UsageDatabasePath))
        {
            ValidationMessage = "사용 이력 DB 경로가 아직 준비되지 않았습니다.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            _currentUsageReport = await _service.GetUsageAsync(new UsageRequest(
                UsageDatabasePath,
                UsageFrom,
                UsageTo,
                SelectedUsageSession)).ConfigureAwait(true);
            UsageSessions.ReplaceWith(_currentUsageReport.Sessions);
            if (!UsageSessions.Contains(SelectedUsageSession))
            {
                SelectedUsageSession = UsageSessions.FirstOrDefault() ?? "";
            }

            UsageRecords.ReplaceWith(_currentUsageReport.Records.Select(record => new SelectableUsageRecord(record)));
            UsageSummary.ReplaceWith(_currentUsageReport.Summary);
            StatusMessage = _currentUsageReport.Succeeded
                ? $"{UsageRecords.Count} usage records loaded."
                : FormatIssues(_currentUsageReport.Issues, "Usage refresh failed.");
        }).ConfigureAwait(true);
    }

    public async Task<UsageAddReport> AddUsageRecordsAsync(IReadOnlyList<UsageAddRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        ValidationMessage = "";
        EnsureUsageDatabasePath();
        if (string.IsNullOrWhiteSpace(UsageDatabasePath))
        {
            ValidationMessage = "사용 이력 DB 경로가 아직 준비되지 않았습니다.";
            return new UsageAddReport(
                false,
                UsageDatabasePath,
                0,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, ValidationMessage)]);
        }

        var report = new UsageAddReport(true, UsageDatabasePath, 0, []);
        await RunBusyAsync(async () =>
        {
            report = await _service.AddUsageRecordsAsync(new UsageAddRequest(UsageDatabasePath, records)).ConfigureAwait(true);
            StatusMessage = report.Succeeded
                ? $"{report.AddedCount} usage records added."
                : FormatIssues(report.Issues, "Usage add failed.");
        }).ConfigureAwait(true);

        return report;
    }

    public async Task DeleteSelectedUsageAsync()
    {
        ValidationMessage = "";
        var selectedRecords = UsageRecords
            .Where(record => record.IsSelected)
            .ToArray();
        var selectedIds = selectedRecords
            .Select(record => record.RecordId)
            .ToArray();
        if (selectedIds.Length == 0)
        {
            ValidationMessage = "No usage records are selected.";
            return;
        }

        var confirmed = await _deleteConfirmation.ConfirmAsync(new UsageDeleteConfirmationRequest(
            selectedIds.Length,
            UsageFrom,
            UsageTo,
            SelectedUsageSession,
            selectedRecords.Select(record => record.Record).ToArray())).ConfigureAwait(true);
        if (!confirmed)
        {
            StatusMessage = "Usage delete canceled.";
            return;
        }

        var deletedCount = 0;
        await RunBusyAsync(async () =>
        {
            var report = await _service.DeleteUsageRecordsAsync(UsageDatabasePath, selectedIds).ConfigureAwait(true);
            StatusMessage = report.Succeeded
                ? $"{report.DeletedCount} usage records deleted."
                : FormatIssues(report.Issues, "Usage delete failed.");
            deletedCount = report.Succeeded ? report.DeletedCount : 0;
        }).ConfigureAwait(true);

        if (deletedCount > 0)
        {
            await RefreshUsageAsync().ConfigureAwait(true);
            StatusMessage = $"{deletedCount} usage records deleted.";
        }
    }

    public async Task ExportUsageReportAsync()
    {
        ValidationMessage = "";
        if (string.IsNullOrWhiteSpace(UsageReportOutputPath))
        {
            ValidationMessage = "Usage report output path is empty.";
            return;
        }

        if (_currentUsageReport is null)
        {
            await RefreshUsageAsync().ConfigureAwait(true);
        }

        if (_currentUsageReport is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var report = await _service.ExportUsageReportAsync(_currentUsageReport, UsageReportOutputPath).ConfigureAwait(true);
            StatusMessage = report.Succeeded
                ? $"Usage report exported to {report.OutputPath}."
                : FormatIssues(report.Issues, "Usage report export failed.");
        }).ConfigureAwait(true);
    }

    private async Task RunBusyAsync(Func<Task> work)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        NotifyCommands();
        try
        {
            await work().ConfigureAwait(true);
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private string ResolveDatabasePath()
    {
        var explicitPath = NormalizePath(_settings.Current.Data.AdminDatabasePath);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return explicitPath;
        }

        return string.IsNullOrWhiteSpace(WorkingFolder)
            ? ""
            : NormalizePath(Path.Combine(WorkingFolder, LegacyAdminDatabaseRelativePath));
    }

    private void EnsureUsageDatabasePath()
    {
        if (!string.IsNullOrWhiteSpace(UsageDatabasePath))
        {
            return;
        }

        WorkingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        UsageDatabasePath = _service.GetDefaultUsageDatabasePath(WorkingFolder);
    }

    private bool EnsureSearchDatabase()
    {
        if (!string.IsNullOrWhiteSpace(DatabasePath))
        {
            return true;
        }

        ValidationMessage = "검색 DB 경로가 아직 준비되지 않았습니다.";
        return false;
    }

    private int[] SelectedFolderNos()
        => SearchFolders
            .Where(folder => folder.IsSelected)
            .Select(folder => folder.FolderNo)
            .ToArray();

    private SongSearchFields BuildSearchFields()
    {
        var fields = SongSearchFields.None;
        if (SearchTitle)
        {
            fields |= SongSearchFields.Title;
        }

        if (SearchLyrics)
        {
            fields |= SongSearchFields.Lyrics;
        }

        if (SearchSongNumber)
        {
            fields |= SongSearchFields.SongNumber;
        }

        if (SearchBookReference)
        {
            fields |= SongSearchFields.BookReference;
        }

        if (SearchUserReference)
        {
            fields |= SongSearchFields.UserReference;
        }

        if (SearchLicenceAdmin)
        {
            fields |= SongSearchFields.LicenceAdmin;
        }

        if (SearchWriter)
        {
            fields |= SongSearchFields.Writer;
        }

        if (SearchCopyright)
        {
            fields |= SongSearchFields.Copyright;
        }

        return fields;
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SearchSongsCommand.NotifyCanExecuteChanged();
        LookupTitlesCommand.NotifyCanExecuteChanged();
        RefreshUsageCommand.NotifyCanExecuteChanged();
        DeleteSelectedUsageCommand.NotifyCanExecuteChanged();
        ExportUsageReportCommand.NotifyCanExecuteChanged();
    }

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static string FormatIssues(System.Collections.Generic.IReadOnlyList<SearchUsageIssue> issues, string fallback)
    {
        var detail = string.Join("; ", issues.Select(issue => issue.Message));
        return string.IsNullOrWhiteSpace(detail) ? fallback : $"{fallback}: {detail}";
    }

    private static bool IsRecoverable(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException
            or System.Data.SQLite.SQLiteException;
}
