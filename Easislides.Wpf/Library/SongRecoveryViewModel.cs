using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed record SongRecoveredEventArgs(int? SongId, int? FolderNo);

public sealed partial class DeletedSongSelection : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public DeletedSongSelection(DeletedSongSummary song)
        => Song = song ?? throw new ArgumentNullException(nameof(song));

    public DeletedSongSummary Song { get; }
}

public sealed partial class SongRecoveryViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private int? _recoveredSongId;
    [ObservableProperty] private int? _recoveredFolderNo;

    public SongRecoveryViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        RecoverCommand = new AsyncRelayCommand(RecoverAsync, CanRecover);
    }

    public event EventHandler<SongRecoveredEventArgs>? Recovered;

    public ObservableCollection<DeletedSongSelection> DeletedSongs { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand RecoverCommand { get; }

    public void Load(string databasePath)
    {
        DatabasePath = NormalizePath(databasePath);
        ValidationMessage = "";
        StatusMessage = "";
        RecoveredSongId = null;
        RecoveredFolderNo = null;
        DeletedSongs.Clear();
        SelectedCount = 0;
        LoadCommand.NotifyCanExecuteChanged();
        RecoverCommand.NotifyCanExecuteChanged();
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateDatabasePath())
        {
            return;
        }

        IsBusy = true;
        try
        {
            var deletedSongs = await _adminDatabase.GetDeletedSongsAsync(DatabasePath).ConfigureAwait(true);
            DeletedSongs.Clear();
            foreach (var song in deletedSongs)
            {
                var item = new DeletedSongSelection(song);
                item.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(DeletedSongSelection.IsSelected))
                    {
                        UpdateSelectionCount();
                    }
                };
                DeletedSongs.Add(item);
            }

            UpdateSelectionCount();
            ValidationMessage = "";
            StatusMessage = DeletedSongs.Count == 0
                ? "복구할 삭제 곡이 없습니다."
                : $"{DeletedSongs.Count}개 삭제 곡 표시";
        }
        catch (Exception ex) when (IsRecoverableRecoveryException(ex))
        {
            StatusMessage = $"삭제 곡 로드 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            LoadCommand.NotifyCanExecuteChanged();
            RecoverCommand.NotifyCanExecuteChanged();
        }
    }

    public async Task RecoverAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateDatabasePath())
        {
            return;
        }

        var selected = DeletedSongs.Where(item => item.IsSelected).ToArray();
        if (selected.Length == 0)
        {
            ValidationMessage = "복구할 곡을 선택하세요.";
            RecoverCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        try
        {
            var backupRoot = ResolveBackupRoot();
            var recoveries = selected
                .Select(item => new SongRecoveryRequest(item.Song.SongId, item.Song.OriginalFolderNo))
                .ToArray();
            var report = await _adminDatabase
                .RecoverSongsAsync(DatabasePath, backupRoot, recoveries)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "복구 실패"
                    : $"복구 실패: {detail}";
                return;
            }

            foreach (var item in selected)
            {
                DeletedSongs.Remove(item);
            }

            RecoveredSongId = report.AffectedSongIds.FirstOrDefault();
            RecoveredFolderNo = report.AffectedFolderNos.FirstOrDefault();
            if (RecoveredSongId == 0)
            {
                RecoveredSongId = null;
            }

            if (RecoveredFolderNo == 0)
            {
                RecoveredFolderNo = null;
            }

            UpdateSelectionCount();
            ValidationMessage = "";
            StatusMessage = "복구되었습니다.";
            Recovered?.Invoke(this, new SongRecoveredEventArgs(RecoveredSongId, RecoveredFolderNo));
        }
        catch (Exception ex) when (IsRecoverableRecoveryException(ex))
        {
            StatusMessage = $"복구 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            LoadCommand.NotifyCanExecuteChanged();
            RecoverCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnIsBusyChanged(bool value)
    {
        LoadCommand.NotifyCanExecuteChanged();
        RecoverCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedCountChanged(int value)
        => RecoverCommand.NotifyCanExecuteChanged();

    private void UpdateSelectionCount()
        => SelectedCount = DeletedSongs.Count(item => item.IsSelected);

    private bool ValidateDatabasePath()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath))
        {
            ValidationMessage = "AdminDB 경로를 설정해야 합니다.";
            return false;
        }

        if (!File.Exists(DatabasePath))
        {
            ValidationMessage = $"AdminDB 파일을 찾을 수 없습니다: {DatabasePath}";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanRecover()
        => !IsBusy && SelectedCount > 0;

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

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableRecoveryException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
