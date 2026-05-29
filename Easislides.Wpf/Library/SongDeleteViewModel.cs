using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed record SongDeletedEventArgs(int SongId, int OriginalFolderNo);

public sealed partial class SongDeleteViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private int _songId;
    [ObservableProperty] private int _sourceFolderNo;
    [ObservableProperty] private string _sourceFolderName = "";
    [ObservableProperty] private string _songTitle = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;

    public SongDeleteViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanDelete);
    }

    public event EventHandler<SongDeletedEventArgs>? Deleted;

    public IAsyncRelayCommand DeleteCommand { get; }

    public void Load(string databasePath, SongSummary song, SongFolderSummary sourceFolder)
    {
        ArgumentNullException.ThrowIfNull(song);
        ArgumentNullException.ThrowIfNull(sourceFolder);

        DatabasePath = NormalizePath(databasePath);
        SongId = song.SongId;
        SourceFolderNo = sourceFolder.FolderNo;
        SourceFolderName = sourceFolder.Name;
        SongTitle = song.Title;
        ValidationMessage = "";
        StatusMessage = "";
        DeleteCommand.NotifyCanExecuteChanged();
    }

    public async Task DeleteAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!Validate())
        {
            DeleteCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        try
        {
            var backupRoot = ResolveBackupRoot();
            var delete = new SongDeleteRequest(SongId, SourceFolderNo);
            var report = await _adminDatabase
                .SoftDeleteSongsAsync(DatabasePath, backupRoot, [delete])
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "삭제 실패"
                    : $"삭제 실패: {detail}";
                return;
            }

            ValidationMessage = "";
            StatusMessage = "삭제되었습니다.";
            Deleted?.Invoke(this, new SongDeletedEventArgs(SongId, SourceFolderNo));
        }
        catch (Exception ex) when (IsRecoverableDeleteException(ex))
        {
            StatusMessage = $"삭제 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            DeleteCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnIsBusyChanged(bool value)
        => DeleteCommand.NotifyCanExecuteChanged();

    private bool Validate()
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

        if (SongId <= 0 || SourceFolderNo <= 0)
        {
            ValidationMessage = "삭제할 곡을 선택하세요.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanDelete()
        => !IsBusy && SongId > 0 && SourceFolderNo > 0;

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

    private static bool IsRecoverableDeleteException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
