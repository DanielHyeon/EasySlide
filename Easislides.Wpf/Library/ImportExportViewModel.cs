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

public sealed partial class SelectableImportSourceFolder : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public SelectableImportSourceFolder(ImportSourceFolder folder)
    {
        Name = folder.Name;
        SongCount = folder.SongCount;
    }

    public string Name { get; }

    public int SongCount { get; }
}

public sealed partial class SelectableExportSongCandidate : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public SelectableExportSongCandidate(ExportSongCandidate song)
    {
        SongId = song.SongId;
        Title = song.Title;
        FolderNo = song.FolderNo;
        FolderName = song.FolderName;
        SongNumber = song.SongNumber;
    }

    public int SongId { get; }

    public string Title { get; }

    public int FolderNo { get; }

    public string FolderName { get; }

    public int SongNumber { get; }
}

public sealed partial class ImportExportViewModel : ObservableObject
{
    private const string LegacyAdminDatabaseRelativePath = "Admin\\Database\\EasiSlidesDb.db";

    private readonly ISettingsService _settings;
    private readonly IImportExportService _service;
    private AccessImportSchema? _accessSchema;
    private AccessImportMapping? _lastAccessMapping;

    [ObservableProperty] private string _workingFolder = "";
    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private string _backupRoot = "";
    [ObservableProperty] private string _importSourcePath = "";
    [ObservableProperty] private string _exportOutputPath = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private SongFolderSummary? _selectedTargetFolder;
    [ObservableProperty] private SongFolderSummary? _selectedExportFolder;
    [ObservableProperty] private ImportDuplicatePolicy _importDuplicatePolicy = ImportDuplicatePolicy.KeepExisting;
    [ObservableProperty] private ExportFormat _selectedExportFormat = ExportFormat.Xml;
    [ObservableProperty] private bool _isAccessImportSource;
    [ObservableProperty] private AccessImportTable? _selectedAccessTable;
    [ObservableProperty] private string _accessTitleColumn = "";
    [ObservableProperty] private string _accessAlternateTitleColumn = "";
    [ObservableProperty] private string _accessSongNumberColumn = "";
    [ObservableProperty] private string _accessLyricsColumnsText = "";
    [ObservableProperty] private string _accessWriterColumn = "";
    [ObservableProperty] private string _accessCopyrightColumn = "";
    [ObservableProperty] private string _accessKeyColumn = "";
    [ObservableProperty] private string _accessTimingColumn = "";
    [ObservableProperty] private string _accessBookReferenceColumn = "";
    [ObservableProperty] private string _accessUserReferenceColumn = "";
    [ObservableProperty] private string _accessLicenceAdmin1Column = "";
    [ObservableProperty] private string _accessLicenceAdmin2Column = "";

    public ImportExportViewModel(ISettingsService settings, IImportExportService service)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        PreviewImportCommand = new AsyncRelayCommand(PreviewImportAsync, () => !IsBusy);
        ImportCommand = new AsyncRelayCommand(ImportAsync, () => !IsBusy);
        LoadAccessSchemaCommand = new AsyncRelayCommand(LoadAccessSchemaAsync, () => !IsBusy && IsAccessImportSource);
        RefreshExportCandidatesCommand = new AsyncRelayCommand(RefreshExportCandidatesAsync, () => !IsBusy);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !IsBusy);
    }

    public ObservableCollection<SongFolderSummary> Folders { get; } = [];

    public ObservableCollection<SelectableImportSourceFolder> ImportSourceFolders { get; } = [];

    public ObservableCollection<AccessImportTable> AccessTables { get; } = [];

    public ObservableCollection<string> AccessColumns { get; } = [];

    public ObservableCollection<SelectableExportSongCandidate> ExportCandidates { get; } = [];

    public IReadOnlyList<ImportDuplicatePolicy> DuplicatePolicies { get; } =
    [
        ImportDuplicatePolicy.KeepExisting,
        ImportDuplicatePolicy.SkipExisting,
        ImportDuplicatePolicy.ReplaceExisting,
    ];

    public IReadOnlyList<ExportFormat> ExportFormats { get; } =
    [
        ExportFormat.Xml,
        ExportFormat.EasiSlidesText,
        ExportFormat.EasiSlidesDatabase,
        ExportFormat.Html,
        ExportFormat.Rtf,
    ];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand PreviewImportCommand { get; }

    public IAsyncRelayCommand ImportCommand { get; }

    public IAsyncRelayCommand LoadAccessSchemaCommand { get; }

    public IAsyncRelayCommand RefreshExportCandidatesCommand { get; }

    public IAsyncRelayCommand ExportCommand { get; }

    partial void OnIsBusyChanged(bool value)
    {
        PreviewImportCommand.NotifyCanExecuteChanged();
        ImportCommand.NotifyCanExecuteChanged();
        LoadAccessSchemaCommand.NotifyCanExecuteChanged();
        RefreshExportCandidatesCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();
    }

    partial void OnImportSourcePathChanged(string value)
    {
        ClearAccessSchema();
        IsAccessImportSource = IsAccessImportPath(value);
        LoadAccessSchemaCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsAccessImportSourceChanged(bool value)
    {
        if (!value)
        {
            ClearAccessSchema();
        }

        LoadAccessSchemaCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedAccessTableChanged(AccessImportTable? value)
    {
        AccessColumns.ReplaceWith(value?.Columns ?? []);
        if (value is null)
        {
            ClearAccessMapping();
            return;
        }

        var mapping = _accessSchema?.SuggestedMapping is { } suggested &&
                      string.Equals(suggested.TableName, value.Name, StringComparison.OrdinalIgnoreCase)
            ? suggested
            : CreateSuggestedAccessMapping(value);
        if (mapping is null)
        {
            ClearAccessMapping();
            return;
        }

        ApplyAccessMapping(mapping);
    }

    partial void OnSelectedExportFormatChanged(ExportFormat value)
    {
        if (!string.IsNullOrWhiteSpace(WorkingFolder))
        {
            ExportOutputPath = _service.GetDefaultExportPath(WorkingFolder, DateOnly.FromDateTime(DateTime.Today), value);
        }
    }

    public async Task LoadAsync()
    {
        WorkingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        DatabasePath = ResolveDatabasePath();
        BackupRoot = ResolveBackupRoot();
        ExportOutputPath = _service.GetDefaultExportPath(WorkingFolder, DateOnly.FromDateTime(DateTime.Today), SelectedExportFormat);

        await RunBusyAsync(async () =>
        {
            var folders = (await _service.GetFoldersAsync(DatabasePath).ConfigureAwait(true))
                .Where(folder => folder.IsEnabled)
                .OrderBy(folder => folder.FolderNo)
                .ToArray();
            Folders.ReplaceWith(folders);
            SelectedTargetFolder = Folders.FirstOrDefault();
            SelectedExportFolder = Folders.FirstOrDefault();
            StatusMessage = $"{Folders.Count} folders loaded.";
        }).ConfigureAwait(true);
    }

    public async Task LoadAccessSchemaAsync()
    {
        ValidationMessage = "";
        if (string.IsNullOrWhiteSpace(ImportSourcePath))
        {
            ValidationMessage = "Import source path is empty.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var schema = await _service.GetAccessSchemaAsync(ImportSourcePath).ConfigureAwait(true);
            _accessSchema = schema;
            AccessTables.ReplaceWith(schema.Tables);
            SelectedAccessTable = schema.SuggestedMapping is { } suggested
                ? AccessTables.FirstOrDefault(table => string.Equals(table.Name, suggested.TableName, StringComparison.OrdinalIgnoreCase))
                : AccessTables.FirstOrDefault();

            if (SelectedAccessTable is null)
            {
                AccessColumns.Clear();
                ClearAccessMapping();
            }

            StatusMessage = schema.Succeeded
                ? $"{schema.Tables.Count} Access MDB tables loaded."
                : FormatIssues(schema.Issues, "Access MDB schema failed.");
        }).ConfigureAwait(true);
    }

    public async Task PreviewImportAsync()
    {
        ValidationMessage = "";
        if (string.IsNullOrWhiteSpace(ImportSourcePath))
        {
            ValidationMessage = "Import source path is empty.";
            return;
        }

        var accessMapping = await ResolveAccessMappingAsync().ConfigureAwait(true);
        if (IsAccessImportSource && accessMapping is null)
        {
            ValidationMessage = "Access MDB mapping is incomplete.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var preview = await _service.PreviewImportAsync(ImportSourcePath, accessMapping).ConfigureAwait(true);
            ImportSourceFolders.ReplaceWith(preview.Folders.Select(folder => new SelectableImportSourceFolder(folder)));
            StatusMessage = preview.Succeeded
                ? $"{preview.TotalSongs} import items found."
                : FormatIssues(preview.Issues, "Import preview failed.");
        }).ConfigureAwait(true);
    }

    public async Task ImportAsync()
    {
        ValidationMessage = "";
        if (SelectedTargetFolder is null)
        {
            ValidationMessage = "Import target folder is not selected.";
            return;
        }

        if (ImportSourceFolders.Count == 0)
        {
            await PreviewImportAsync().ConfigureAwait(true);
        }

        var selectedFolders = ImportSourceFolders
            .Where(folder => folder.IsSelected)
            .Select(folder => folder.Name)
            .ToArray();
        if (selectedFolders.Length == 0)
        {
            ValidationMessage = "No import source folders are selected.";
            return;
        }

        var accessMapping = await ResolveAccessMappingAsync().ConfigureAwait(true);
        if (IsAccessImportSource && accessMapping is null)
        {
            ValidationMessage = "Access MDB mapping is incomplete.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var report = await _service.ImportAsync(new ImportRequest(
                DatabasePath,
                BackupRoot,
                ImportSourcePath,
                SelectedTargetFolder.FolderNo,
                selectedFolders,
                ImportDuplicatePolicy,
                AccessMapping: accessMapping)).ConfigureAwait(true);
            StatusMessage = report.Succeeded
                ? $"{report.ImportedNew + report.Replaced} imported, {report.Skipped} skipped."
                : FormatIssues(report.Issues, $"{report.ImportedNew + report.Replaced} imported, {report.Failed} failed.");
        }).ConfigureAwait(true);
    }

    public async Task RefreshExportCandidatesAsync()
    {
        ValidationMessage = "";
        if (SelectedExportFolder is null)
        {
            ValidationMessage = "Export folder is not selected.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var candidates = await _service.GetExportCandidatesAsync(
                DatabasePath,
                [SelectedExportFolder.FolderNo]).ConfigureAwait(true);
            ExportCandidates.ReplaceWith(candidates.Select(candidate => new SelectableExportSongCandidate(candidate)));
            StatusMessage = $"{ExportCandidates.Count} export items found.";
        }).ConfigureAwait(true);
    }

    public async Task ExportAsync()
    {
        ValidationMessage = "";
        if (string.IsNullOrWhiteSpace(ExportOutputPath))
        {
            ValidationMessage = "Export output path is empty.";
            return;
        }

        if (ExportCandidates.Count == 0)
        {
            await RefreshExportCandidatesAsync().ConfigureAwait(true);
        }

        var selectedSongs = ExportCandidates
            .Where(candidate => candidate.IsSelected)
            .Select(candidate => candidate.SongId)
            .ToArray();
        if (selectedSongs.Length == 0)
        {
            ValidationMessage = "No export songs are selected.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var report = await _service.ExportAsync(new ExportRequest(
                DatabasePath,
                ExportOutputPath,
                SelectedExportFormat,
                selectedSongs,
                SelectedExportFolder is null ? [] : [SelectedExportFolder.FolderNo])).ConfigureAwait(true);
            StatusMessage = report.Succeeded
                ? $"{report.ExportedSongs} songs exported."
                : FormatIssues(report.Issues, "Export failed.");
        }).ConfigureAwait(true);
    }

    private async Task<AccessImportMapping?> ResolveAccessMappingAsync()
    {
        if (!IsAccessImportSource)
        {
            return null;
        }

        if (AccessTables.Count == 0)
        {
            await LoadAccessSchemaAsync().ConfigureAwait(true);
        }

        return BuildAccessMapping();
    }

    private AccessImportMapping? BuildAccessMapping()
    {
        if (!IsAccessImportSource || SelectedAccessTable is null)
        {
            return null;
        }

        var lyricsColumns = ParseAccessLyricsColumns(AccessLyricsColumnsText);
        if (string.IsNullOrWhiteSpace(AccessTitleColumn) || lyricsColumns.Count == 0)
        {
            return null;
        }

        var mapping = new AccessImportMapping(
            SelectedAccessTable.Name,
            AccessTitleColumn,
            lyricsColumns,
            AlternateTitleColumn: AccessAlternateTitleColumn,
            SongNumberColumn: AccessSongNumberColumn,
            WriterColumn: AccessWriterColumn,
            CopyrightColumn: AccessCopyrightColumn,
            KeyColumn: AccessKeyColumn,
            TimingColumn: AccessTimingColumn,
            BookReferenceColumn: AccessBookReferenceColumn,
            UserReferenceColumn: AccessUserReferenceColumn,
            LicenceAdmin1Column: AccessLicenceAdmin1Column,
            LicenceAdmin2Column: AccessLicenceAdmin2Column);
        if (AccessMappingsMatch(_lastAccessMapping, mapping))
        {
            return _lastAccessMapping;
        }

        _lastAccessMapping = mapping;
        return mapping;
    }

    private void ApplyAccessMapping(AccessImportMapping mapping)
    {
        _lastAccessMapping = mapping;
        AccessTitleColumn = mapping.TitleColumn;
        AccessAlternateTitleColumn = mapping.AlternateTitleColumn;
        AccessSongNumberColumn = mapping.SongNumberColumn;
        AccessLyricsColumnsText = string.Join(", ", mapping.LyricsColumns);
        AccessWriterColumn = mapping.WriterColumn;
        AccessCopyrightColumn = mapping.CopyrightColumn;
        AccessKeyColumn = mapping.KeyColumn;
        AccessTimingColumn = mapping.TimingColumn;
        AccessBookReferenceColumn = mapping.BookReferenceColumn;
        AccessUserReferenceColumn = mapping.UserReferenceColumn;
        AccessLicenceAdmin1Column = mapping.LicenceAdmin1Column;
        AccessLicenceAdmin2Column = mapping.LicenceAdmin2Column;
    }

    private void ClearAccessSchema()
    {
        _accessSchema = null;
        AccessTables.Clear();
        AccessColumns.Clear();
        SelectedAccessTable = null;
        ClearAccessMapping();
    }

    private void ClearAccessMapping()
    {
        _lastAccessMapping = null;
        AccessTitleColumn = "";
        AccessAlternateTitleColumn = "";
        AccessSongNumberColumn = "";
        AccessLyricsColumnsText = "";
        AccessWriterColumn = "";
        AccessCopyrightColumn = "";
        AccessKeyColumn = "";
        AccessTimingColumn = "";
        AccessBookReferenceColumn = "";
        AccessUserReferenceColumn = "";
        AccessLicenceAdmin1Column = "";
        AccessLicenceAdmin2Column = "";
    }

    private static AccessImportMapping? CreateSuggestedAccessMapping(AccessImportTable table)
    {
        var title = PickAccessColumn(table.Columns, "TITLE_1", "Title_1", "Title", "Name");
        var lyrics = PickAccessColumn(table.Columns, "Lyrics", "LYRICS", "Contents", "Body", "Verse1");
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(lyrics))
        {
            return null;
        }

        return new AccessImportMapping(
            table.Name,
            title,
            [lyrics],
            AlternateTitleColumn: PickAccessColumn(table.Columns, "TITLE_2", "Title_2", "Title2", "AltName", "AlternateTitle"),
            SongNumberColumn: PickAccessColumn(table.Columns, "SONG_NUMBER", "SongNumber", "Number", "No"),
            WriterColumn: PickAccessColumn(table.Columns, "writer", "WRITER", "Writer", "Author"),
            CopyrightColumn: PickAccessColumn(table.Columns, "copyright", "COPYRIGHT", "Copyright", "Rights"),
            KeyColumn: PickAccessColumn(table.Columns, "key", "KEY", "MusicKey"),
            TimingColumn: PickAccessColumn(table.Columns, "Timing", "TIMING", "Tempo"),
            BookReferenceColumn: PickAccessColumn(table.Columns, "BOOK_REFERENCE", "BookReference", "BookRef"),
            UserReferenceColumn: PickAccessColumn(table.Columns, "USER_REFERENCE", "UserReference", "UserRef"),
            LicenceAdmin1Column: PickAccessColumn(table.Columns, "LICENCE_ADMIN1", "LicenceAdmin1", "Admin1", "AdminA"),
            LicenceAdmin2Column: PickAccessColumn(table.Columns, "LICENCE_ADMIN2", "LicenceAdmin2", "Admin2", "AdminB"));
    }

    private static IReadOnlyList<string> ParseAccessLyricsColumns(string value)
        => value
            .Split([',', ';', '>', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(column => !string.IsNullOrWhiteSpace(column))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string PickAccessColumn(IReadOnlyList<string> columns, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var column = columns.FirstOrDefault(value => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(column))
            {
                return column;
            }
        }

        return "";
    }

    private static bool AccessMappingsMatch(AccessImportMapping? left, AccessImportMapping right)
        => left is not null &&
           string.Equals(left.TableName, right.TableName, StringComparison.Ordinal) &&
           string.Equals(left.TitleColumn, right.TitleColumn, StringComparison.Ordinal) &&
           left.LyricsColumns.SequenceEqual(right.LyricsColumns, StringComparer.Ordinal) &&
           string.Equals(left.AlternateTitleColumn, right.AlternateTitleColumn, StringComparison.Ordinal) &&
           string.Equals(left.SongNumberColumn, right.SongNumberColumn, StringComparison.Ordinal) &&
           string.Equals(left.WriterColumn, right.WriterColumn, StringComparison.Ordinal) &&
           string.Equals(left.CopyrightColumn, right.CopyrightColumn, StringComparison.Ordinal) &&
           string.Equals(left.KeyColumn, right.KeyColumn, StringComparison.Ordinal) &&
           string.Equals(left.TimingColumn, right.TimingColumn, StringComparison.Ordinal) &&
           string.Equals(left.BookReferenceColumn, right.BookReferenceColumn, StringComparison.Ordinal) &&
           string.Equals(left.UserReferenceColumn, right.UserReferenceColumn, StringComparison.Ordinal) &&
           string.Equals(left.LicenceAdmin1Column, right.LicenceAdmin1Column, StringComparison.Ordinal) &&
           string.Equals(left.LicenceAdmin2Column, right.LicenceAdmin2Column, StringComparison.Ordinal);

    private async Task RunBusyAsync(Func<Task> work)
    {
        IsBusy = true;
        try
        {
            await work().ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
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

    private string ResolveBackupRoot()
    {
        var explicitPath = NormalizePath(_settings.Current.Data.BackupRoot);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return explicitPath;
        }

        return string.IsNullOrWhiteSpace(DatabasePath)
            ? ""
            : NormalizePath(Path.Combine(Path.GetDirectoryName(DatabasePath) ?? WorkingFolder, "Backups"));
    }

    private static string NormalizePath(string path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsAccessImportPath(string path)
        => string.Equals(Path.GetExtension(path), ".mdb", StringComparison.OrdinalIgnoreCase);

    private static string FormatIssues(IReadOnlyList<ImportExportIssue> issues, string fallback)
    {
        var detail = string.Join("; ", issues.Select(issue => issue.Message));
        return string.IsNullOrWhiteSpace(detail) ? fallback : $"{fallback}: {detail}";
    }
}
