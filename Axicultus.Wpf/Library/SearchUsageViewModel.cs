using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Library;

public sealed partial class SelectableSearchFolder(SongFolderSummary folder) : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public SongFolderSummary Folder { get; } = folder;

    public int FolderNo => Folder.FolderNo;

    public string Name => Folder.Name;

    public int SongCount => Folder.SongCount;
}

public sealed partial class SelectableUsageRecord(UsageRecord record) : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public UsageRecord Record { get; } = record;

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
    [ObservableProperty] private DateTime _modifiedFrom = DateTime.Today.AddDays(-91);
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
        var general = settings.Current.General;
        if (Axicultus.Util.StoredDate.TryParse(general.FindModifiedFrom, out var from)) _modifiedFrom = from.Date;
        if (Axicultus.Util.StoredDate.TryParse(general.FindModifiedTo, out var to)) _modifiedTo = to.Date;
        _searchTitle = general.FindTitle;
        _searchLyrics = general.FindLyrics;
        _searchSongNumber = general.FindSongNumber;
        _searchBookReference = general.FindBookReference;
        _searchUserReference = general.FindUserReference;
        _searchLicenceAdmin = general.FindLicenceAdmin;
        _searchWriter = general.FindWriter;
        _searchCopyright = general.FindCopyright;
        _useModifiedDates = general.FindUseModifiedDates;
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

    public ObservableCollection<string> SearchKeys { get; } = [];
    public ObservableCollection<string> SearchTimings { get; } = [];
    private int _songSearchRevision;

    internal void CancelPendingFind() => _songSearchRevision++;

    public ObservableCollection<SelectableSearchFolder> SearchFolders { get; } = [];

    internal event EventHandler? SongsFound;

    public ObservableCollection<SongSearchResult> SearchResults { get; } = [];

    public ObservableCollection<LookupTitleCandidate> LookupCandidates { get; } = [];

    public ObservableCollection<string> UsageSessions { get; } = [];

    public ObservableCollection<SelectableUsageRecord> UsageRecords { get; } = [];

    public ObservableCollection<UsageSummary> UsageSummary { get; } = [];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SearchSongsCommand { get; }

    public IAsyncRelayCommand LookupTitlesCommand { get; }

    public IAsyncRelayCommand RefreshUsageCommand { get; }

    public IAsyncRelayCommand DeleteSelectedUsageCommand { get; }

    public IAsyncRelayCommand ExportUsageReportCommand { get; }

    public async Task LoadAsync()
    {
        WorkingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        var previousDatabase = DatabasePath;
        DatabasePath = ResolveDatabasePath();
        UsageDatabasePath = _service.GetDefaultUsageDatabasePath(WorkingFolder);
        UsageReportOutputPath = string.IsNullOrWhiteSpace(WorkingFolder)
            ? ""
            : NormalizePath(Path.Combine(WorkingFolder, "Documents", "Song Usages.rtf"));

        await RunBusyAsync(async () =>
        {
            var previousFolders = SearchFolders.ToLookup(folder => folder.Name, StringComparer.OrdinalIgnoreCase);
            var sameDatabase = string.Equals(previousDatabase, DatabasePath, StringComparison.OrdinalIgnoreCase);
            var folders = (await _service.GetFoldersAsync(DatabasePath).ConfigureAwait(true))
                .Where(folder => folder.IsEnabled && folder.FolderNo > 0)
                .OrderBy(folder => folder.FolderNo)
                .Select(folder =>
                {
                    var candidates = previousFolders[folder.Name];
                    var previous = candidates.FirstOrDefault(item => item.FolderNo == folder.FolderNo) ?? candidates.FirstOrDefault();
                    return new SelectableSearchFolder(folder) { IsSelected = !sameDatabase || previous?.IsSelected != false };
                })
                .ToArray();
            SearchFolders.ReplaceWith(folders);
            var choices = await _service.GetFilterChoicesAsync(DatabasePath);
            SearchKeys.ReplaceWith(choices.Keys);
            SearchTimings.ReplaceWith(choices.Timings);
            StatusMessage = $"{SearchFolders.Count} search folders loaded.";
        }).ConfigureAwait(true);
    }

    internal SongSearchPage? LastSearch { get; private set; }

    internal async Task<SongSearchPage?> QuickFindAsync(string phrase, string database)
    {
        if (string.IsNullOrWhiteSpace(database)) return null;
        try
        {
            var folders = await _service.GetFoldersAsync(database).ConfigureAwait(true);
            var request = new SongSearchRequest(database, phrase,
                folders.Where(f => f.IsEnabled && f.FolderNo > 0).Select(f => f.FolderNo).ToArray(), SongSearchFields.All);
            return new SongSearchPage(request, await _service.SearchSongsAsync(request).ConfigureAwait(true));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            StatusMessage = ex.Message;
            return null;
        }
    }

    public async Task SearchSongsAsync()
    {
        ValidationMessage = "";
        if (!EnsureSearchDatabase())
        {
            return;
        }

        if (!string.Equals(DatabasePath, ResolveDatabasePath(), StringComparison.OrdinalIgnoreCase))
        {
            await LoadAsync();
            if (!EnsureSearchDatabase()) return;
        }
        await RunBusyAsync(async () =>
        {
            var folders = SelectedFolderNos();
            if (folders.Length == 0)
            {
                SearchResults.Clear();
                HasSearched = true;
                ValidationMessage = "검색할 폴더를 하나 이상 선택하세요.";
                StatusMessage = ValidationMessage;
                return;
            }
            if (UseModifiedDates && ModifiedFrom.Date > ModifiedTo.Date)
            {
                SearchResults.Clear();
                ValidationMessage = "시작일은 종료일보다 늦을 수 없습니다.";
                StatusMessage = ValidationMessage;
                return;
            }
            var from = ModifiedFrom;
            var to = ModifiedTo;
            var useDates = UseModifiedDates;
            var request = new SongSearchRequest(
                DatabasePath,
                SearchText,
                folders,
                BuildSearchFields(),
                SearchKey,
                SearchTiming,
                NotationsOnly,
                MediaOnly,
                useDates ? from : null,
                useDates ? to : null,
                WorkingFolder,
                _settings.Get(AxicultusSettingKeys.MediaDirectory));
            var revision = ++_songSearchRevision;
            var results = await _service.SearchSongsAsync(request).ConfigureAwait(true);
            if (revision != _songSearchRevision) return;
            LastSearch = new SongSearchPage(request, results);
            SearchResults.ReplaceWith(results);
            _settings.SetValues(new Dictionary<string, object>
            {
                [AxicultusSettingKeys.FindTitle.Id] = request.Fields.HasFlag(SongSearchFields.Title),
                [AxicultusSettingKeys.FindLyrics.Id] = request.Fields.HasFlag(SongSearchFields.Lyrics),
                [AxicultusSettingKeys.FindSongNumber.Id] = request.Fields.HasFlag(SongSearchFields.SongNumber),
                [AxicultusSettingKeys.FindBookReference.Id] = request.Fields.HasFlag(SongSearchFields.BookReference),
                [AxicultusSettingKeys.FindUserReference.Id] = request.Fields.HasFlag(SongSearchFields.UserReference),
                [AxicultusSettingKeys.FindLicenceAdmin.Id] = request.Fields.HasFlag(SongSearchFields.LicenceAdmin),
                [AxicultusSettingKeys.FindWriter.Id] = request.Fields.HasFlag(SongSearchFields.Writer),
                [AxicultusSettingKeys.FindCopyright.Id] = request.Fields.HasFlag(SongSearchFields.Copyright),
                [AxicultusSettingKeys.FindModifiedFrom.Id] = Axicultus.Util.StoredDate.ToText(from),
                [AxicultusSettingKeys.FindModifiedTo.Id] = Axicultus.Util.StoredDate.ToText(to),
                [AxicultusSettingKeys.FindUseModifiedDates.Id] = useDates
            });
            HasSearched = true; // 검색을 실행했음을 표시 — 이제부터 결과 0건이면 "결과 없음" 안내가 뜬다.
            StatusMessage = $"{SearchResults.Count} songs found.";
            SongsFound?.Invoke(this, EventArgs.Empty);
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
            : NormalizePath(ProductStorage.SongDatabase(WorkingFolder));
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
        => [.. SearchFolders
            .Where(folder => folder.IsSelected)
            .Select(folder => folder.FolderNo)];

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
