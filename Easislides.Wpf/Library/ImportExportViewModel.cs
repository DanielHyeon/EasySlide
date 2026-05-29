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

    public ImportExportViewModel(ISettingsService settings, IImportExportService service)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        PreviewImportCommand = new AsyncRelayCommand(PreviewImportAsync, () => !IsBusy);
        ImportCommand = new AsyncRelayCommand(ImportAsync, () => !IsBusy);
        RefreshExportCandidatesCommand = new AsyncRelayCommand(RefreshExportCandidatesAsync, () => !IsBusy);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !IsBusy);
    }

    public ObservableCollection<SongFolderSummary> Folders { get; } = [];

    public ObservableCollection<SelectableImportSourceFolder> ImportSourceFolders { get; } = [];

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

    public IAsyncRelayCommand RefreshExportCandidatesCommand { get; }

    public IAsyncRelayCommand ExportCommand { get; }

    partial void OnIsBusyChanged(bool value)
    {
        PreviewImportCommand.NotifyCanExecuteChanged();
        ImportCommand.NotifyCanExecuteChanged();
        RefreshExportCandidatesCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();
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

    public async Task PreviewImportAsync()
    {
        ValidationMessage = "";
        if (string.IsNullOrWhiteSpace(ImportSourcePath))
        {
            ValidationMessage = "Import source path is empty.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var preview = await _service.PreviewImportAsync(ImportSourcePath).ConfigureAwait(true);
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

        await RunBusyAsync(async () =>
        {
            var report = await _service.ImportAsync(new ImportRequest(
                DatabasePath,
                BackupRoot,
                ImportSourcePath,
                SelectedTargetFolder.FolderNo,
                selectedFolders,
                ImportDuplicatePolicy)).ConfigureAwait(true);
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

    private static string FormatIssues(IReadOnlyList<ImportExportIssue> issues, string fallback)
    {
        var detail = string.Join("; ", issues.Select(issue => issue.Message));
        return string.IsNullOrWhiteSpace(detail) ? fallback : $"{fallback}: {detail}";
    }
}
