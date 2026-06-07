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

public sealed record ExternalFileOperationCompletedEventArgs(
    ExternalFileOperationKind OperationKind,
    ExternalFileItemKind ItemKind,
    ExternalFileDestinationKind DestinationKind,
    int ProcessedCount);

public sealed partial class ExternalFileOperationViewModel : ObservableObject
{
    private const string LegacyAdminDatabaseRelativePath = @"Admin\Database\EasiSlidesDb.db";

    private static readonly IReadOnlyList<ExternalFileDestinationKind> ExternalFolderOnlyDestinationKinds =
        [ExternalFileDestinationKind.ExternalFolder];

    private static readonly IReadOnlyList<ExternalFileDestinationKind> InfoScreenCopyDestinationKinds =
        [ExternalFileDestinationKind.ExternalFolder, ExternalFileDestinationKind.SongFolder];

    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private readonly IExternalFileOperationService _externalFiles;

    [ObservableProperty] private string _workingFolder = "";
    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private ExternalFileItemKind _itemKind = ExternalFileItemKind.InfoScreen;
    [ObservableProperty] private ExternalFileOperationKind _operationKind = ExternalFileOperationKind.Copy;
    [ObservableProperty] private ExternalFileDestinationKind _destinationKind = ExternalFileDestinationKind.ExternalFolder;
    [ObservableProperty] private ExternalFileItem? _selectedSourceFile;
    [ObservableProperty] private ExternalFileFolder? _selectedExternalFolder;
    [ObservableProperty] private SongFolderSummary? _selectedSongFolder;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;

    public ExternalFileOperationViewModel(
        ISettingsService settings,
        IAdminDatabaseRepository adminDatabase,
        IExternalFileOperationService externalFiles)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        _externalFiles = externalFiles ?? throw new ArgumentNullException(nameof(externalFiles));
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, () => !IsBusy);
        RemoveSourceFileCommand = new RelayCommand(RemoveSelectedSourceFile, () => SelectedSourceFile is not null && !IsBusy);
    }

    public event EventHandler<ExternalFileOperationCompletedEventArgs>? Completed;

    public ObservableCollection<ExternalFileItem> SourceFiles { get; } = new();

    public ObservableCollection<ExternalFileFolder> ExternalFolders { get; } = new();

    public ObservableCollection<SongFolderSummary> SongFolders { get; } = new();

    public IReadOnlyList<ExternalFileItemKind> ItemKinds { get; } =
        [ExternalFileItemKind.InfoScreen, ExternalFileItemKind.PowerPoint, ExternalFileItemKind.Media];

    public IReadOnlyList<ExternalFileOperationKind> OperationKinds { get; } =
        [ExternalFileOperationKind.Copy, ExternalFileOperationKind.Move];

    public bool CanUseSongFolderDestination =>
        OperationKind == ExternalFileOperationKind.Copy && ItemKind == ExternalFileItemKind.InfoScreen;

    public IReadOnlyList<ExternalFileDestinationKind> DestinationKinds =>
        CanUseSongFolderDestination ? InfoScreenCopyDestinationKinds : ExternalFolderOnlyDestinationKinds;

    public IAsyncRelayCommand ExecuteCommand { get; }

    public IRelayCommand RemoveSourceFileCommand { get; }

    public async Task LoadAsync()
    {
        WorkingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        DatabasePath = ResolveDatabasePath();
        RefreshExternalFolders();
        await LoadSongFoldersAsync().ConfigureAwait(true);
        ValidationMessage = "";
        StatusMessage = "";
        NotifyCommands();
    }

    public void AddSourceFiles(IEnumerable<string> filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        foreach (var filePath in filePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                continue;
            }

            var path = Path.GetFullPath(filePath);
            if (SourceFiles.Any(item => string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            SourceFiles.Add(new ExternalFileItem(path, Path.GetFileName(path)));
        }

        SelectedSourceFile ??= SourceFiles.FirstOrDefault();
        NotifyCommands();
    }

    public async Task ExecuteAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!Validate())
        {
            NotifyCommands();
            return;
        }

        IsBusy = true;
        try
        {
            var request = BuildRequest();
            var report = await _externalFiles.ExecuteAsync(request).ConfigureAwait(true);
            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail) ? "외부 파일 처리 실패" : $"외부 파일 처리 실패: {detail}";
                return;
            }

            var processed = report.CreatedFilePaths.Count + report.CreatedSongIds.Count;
            ValidationMessage = "";
            StatusMessage = $"{processed}개 항목을 처리했습니다.";
            Completed?.Invoke(this, new ExternalFileOperationCompletedEventArgs(
                OperationKind,
                ItemKind,
                DestinationKind,
                processed));
        }
        catch (Exception ex) when (IsRecoverableOperationException(ex))
        {
            StatusMessage = $"외부 파일 처리 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    partial void OnItemKindChanged(ExternalFileItemKind value)
    {
        RefreshExternalFolders();
        RefreshDestinationOptions();
        NotifyCommands();
    }

    partial void OnOperationKindChanged(ExternalFileOperationKind value)
    {
        RefreshDestinationOptions();
        NotifyCommands();
    }

    partial void OnDestinationKindChanged(ExternalFileDestinationKind value)
    {
        if (!DestinationKinds.Contains(value))
        {
            DestinationKind = ExternalFileDestinationKind.ExternalFolder;
            return;
        }

        NotifyCommands();
    }

    partial void OnSelectedSourceFileChanged(ExternalFileItem? value)
        => NotifyCommands();

    partial void OnSelectedExternalFolderChanged(ExternalFileFolder? value)
        => NotifyCommands();

    partial void OnSelectedSongFolderChanged(SongFolderSummary? value)
        => NotifyCommands();

    partial void OnIsBusyChanged(bool value)
        => NotifyCommands();

    private void RemoveSelectedSourceFile()
    {
        if (SelectedSourceFile is null)
        {
            return;
        }

        var index = SourceFiles.IndexOf(SelectedSourceFile);
        SourceFiles.Remove(SelectedSourceFile);
        SelectedSourceFile = SourceFiles.Count == 0
            ? null
            : SourceFiles[Math.Clamp(index, 0, SourceFiles.Count - 1)];
        NotifyCommands();
    }

    private ExternalFileOperationRequest BuildRequest()
        => new(
            OperationKind,
            ItemKind,
            DestinationKind,
            SourceFiles.Select(item => item.Path).ToArray(),
            TargetFolderPath: SelectedExternalFolder?.Path,
            DatabasePath: DatabasePath,
            TargetSongFolderNo: SelectedSongFolder?.FolderNo,
            BackupRoot: ResolveBackupRoot(),
            StartingSongNumber: Math.Max((SelectedSongFolder?.SongCount ?? 0) + 1, 1));

    private bool Validate()
    {
        if (SourceFiles.Count == 0)
        {
            ValidationMessage = "처리할 외부 파일을 추가하세요.";
            return false;
        }

        if (DestinationKind == ExternalFileDestinationKind.SongFolder)
        {
            if (OperationKind != ExternalFileOperationKind.Copy || ItemKind != ExternalFileItemKind.InfoScreen)
            {
                ValidationMessage = "곡 폴더로 가져오기는 InfoScreen 복사에서만 사용할 수 있습니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
            {
                ValidationMessage = "AdminDB 경로를 설정해야 합니다.";
                return false;
            }

            if (SelectedSongFolder is null)
            {
                ValidationMessage = "대상 곡 폴더를 선택하세요.";
                return false;
            }
        }
        else if (SelectedExternalFolder is null)
        {
            ValidationMessage = "대상 외부 폴더를 선택하세요.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private void RefreshExternalFolders()
    {
        ExternalFolders.ReplaceWith(_externalFiles.GetFolders(WorkingFolder, ItemKind));
        SelectedExternalFolder = ExternalFolders.FirstOrDefault();
    }

    private void RefreshDestinationOptions()
    {
        OnPropertyChanged(nameof(CanUseSongFolderDestination));
        OnPropertyChanged(nameof(DestinationKinds));

        if (!DestinationKinds.Contains(DestinationKind))
        {
            DestinationKind = ExternalFileDestinationKind.ExternalFolder;
        }
    }

    private async Task LoadSongFoldersAsync()
    {
        SongFolders.Clear();
        if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        {
            SelectedSongFolder = null;
            return;
        }

        var folders = await _adminDatabase.GetSongFoldersAsync(DatabasePath).ConfigureAwait(true);
        SongFolders.ReplaceWith(folders.Where(folder => folder.IsEnabled));
        SelectedSongFolder = SongFolders.FirstOrDefault();
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
        var configured = _settings.Current.Data.BackupRoot;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return NormalizePath(configured);
        }

        var databaseDirectory = Path.GetDirectoryName(DatabasePath);
        return Path.Combine(
            string.IsNullOrWhiteSpace(databaseDirectory) ? Environment.CurrentDirectory : databaseDirectory,
            "Backups");
    }

    private void NotifyCommands()
    {
        ExecuteCommand.NotifyCanExecuteChanged();
        RemoveSourceFileCommand.NotifyCanExecuteChanged();
    }

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableOperationException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
