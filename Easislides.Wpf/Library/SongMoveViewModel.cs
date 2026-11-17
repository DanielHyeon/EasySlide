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

public sealed record SongMovedEventArgs(int SongId, int OldFolderNo, int NewFolderNo);

public sealed partial class SongMoveViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private int _songId;
    [ObservableProperty] private int _sourceFolderNo;
    [ObservableProperty] private string _sourceFolderName = "";
    [ObservableProperty] private string _songTitle = "";
    [ObservableProperty] private SongFolderSummary? _selectedTargetFolder;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;

    public SongMoveViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        MoveCommand = new AsyncRelayCommand(MoveAsync, CanMove);
    }

    public event EventHandler<SongMovedEventArgs>? Moved;

    public ObservableCollection<SongFolderSummary> TargetFolders { get; } = new();

    public IAsyncRelayCommand MoveCommand { get; }

    public void Load(
        string databasePath,
        SongSummary song,
        SongFolderSummary sourceFolder,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(song);
        ArgumentNullException.ThrowIfNull(sourceFolder);
        ArgumentNullException.ThrowIfNull(allFolders);

        DatabasePath = NormalizePath(databasePath);
        SongId = song.SongId;
        SourceFolderNo = sourceFolder.FolderNo;
        SourceFolderName = sourceFolder.Name;
        SongTitle = song.Title;
        TargetFolders.ReplaceWith(allFolders.Where(folder => folder.FolderNo != sourceFolder.FolderNo));
        SelectedTargetFolder = TargetFolders.FirstOrDefault();
        ValidationMessage = "";
        StatusMessage = "";
        MoveCommand.NotifyCanExecuteChanged();
    }

    public async Task MoveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!Validate())
        {
            MoveCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        try
        {
            var targetFolderNo = SelectedTargetFolder!.FolderNo;
            var backupRoot = ResolveBackupRoot();
            var move = new SongMoveRequest(SongId, SourceFolderNo, targetFolderNo);
            var report = await _adminDatabase
                .MoveSongsAsync(DatabasePath, backupRoot, [move])
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "이동 실패"
                    : $"이동 실패: {detail}";
                return;
            }

            ValidationMessage = "";
            StatusMessage = "이동되었습니다.";
            Moved?.Invoke(this, new SongMovedEventArgs(SongId, SourceFolderNo, targetFolderNo));
        }
        catch (Exception ex) when (IsRecoverableMoveException(ex))
        {
            StatusMessage = $"이동 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            MoveCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnSelectedTargetFolderChanged(SongFolderSummary? value)
        => MoveCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
        => MoveCommand.NotifyCanExecuteChanged();

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
            ValidationMessage = "이동할 곡을 선택하세요.";
            return false;
        }

        if (SelectedTargetFolder is null)
        {
            ValidationMessage = "이동할 대상 폴더를 선택하세요.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanMove()
        => !IsBusy && SelectedTargetFolder is not null;

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

    private static bool IsRecoverableMoveException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
