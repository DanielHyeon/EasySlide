using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Library;

public sealed record SongDeletedEventArgs(int SongId, int OriginalFolderNo);

public sealed partial class SongDeleteViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private IReadOnlyList<SongSummary> _songs = [];

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private int _songId;
    [ObservableProperty] private int _sourceFolderNo;
    [ObservableProperty] private string _sourceFolderName = "";
    [ObservableProperty] private string _songTitle = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _confirmationText = UiText.Get("SongDeleteWindow", "Confirm");

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

        Load(databasePath, [song with { FolderNo = sourceFolder.FolderNo }], [sourceFolder]);
    }

    /// <summary>Loads every selected song (FrmMain deletes the whole selection of its song list).</summary>
    public void Load(string databasePath, IReadOnlyList<SongSummary> songs, IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(songs);
        ArgumentNullException.ThrowIfNull(allFolders);

        _songs = songs;
        DatabasePath = NormalizePath(databasePath);
        SongId = songs.Count > 0 ? songs[0].SongId : 0;
        SourceFolderNo = songs.Count > 0 ? songs[0].FolderNo : 0;
        SourceFolderName = SongSelectionText.SourceFolderName(songs, allFolders);
        SongTitle = SongSelectionText.Heading(songs);
        // FrmMain asks about "the selected song" or "the N songs you have selected".
        ConfirmationText = songs.Count > 1 ? UiText.Format("SongDeleteWindow", "ConfirmSeveral", songs.Count) : UiText.Get("SongDeleteWindow", "Confirm");
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
            // The songs as they are now: FrmMain leaves out a selected song whose row is gone, and the folder a
            // song is in (kept as OldFolder, where recovery puts it back) may have changed since the list was read.
            var current = (await _adminDatabase
                .GetMatchingSongsAsync(DatabasePath, _songs.Select(song => song.SongId).ToHashSet(), new HashSet<string>())
                .ConfigureAwait(true)).ToDictionary(song => song.SongId);
            var deletes = new List<SongDeleteRequest>();
            var missing = new List<string>();
            foreach (var song in _songs)
            {
                if (current.TryGetValue(song.SongId, out var stored)) deletes.Add(new SongDeleteRequest(song.SongId, stored.FolderNo));
                else missing.Add(song.Title);
            }

            string skipped = SongSelectionText.Skipped(missing, UiText.Get("SongDeleteWindow", "AlreadyDeleted"));
            if (deletes.Count == 0)
            {
                StatusMessage = UiText.Get("SongDeleteWindow", "NothingToDelete") + skipped;
                return;
            }

            // One backup and one transaction for the whole selection: all the songs are deleted, or none.
            var report = await _adminDatabase
                .SoftDeleteSongsAsync(DatabasePath, backupRoot, deletes)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? UiText.Get("SongDeleteWindow", "DeleteFailed")
                    : UiText.Format("SongDeleteWindow", "DeleteFailedDetail", detail);
                return;
            }

            ValidationMessage = "";
            StatusMessage = _songs.Count == 1 ? UiText.Get("SongDeleteWindow", "Deleted") : UiText.Format("SongDeleteWindow", "DeletedSeveral", deletes.Count) + skipped;
            Deleted?.Invoke(this, new SongDeletedEventArgs(deletes[0].SongId, deletes[0].OriginalFolderNo));
        }
        catch (Exception ex) when (IsRecoverableDeleteException(ex))
        {
            StatusMessage = UiText.Format("SongDeleteWindow", "DeleteFailedDetail", ex.Message);
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
            ValidationMessage = UiText.Get("SongDeleteWindow", "DatabasePathMissing");
            return false;
        }

        if (!File.Exists(DatabasePath))
        {
            ValidationMessage = UiText.Format("SongDeleteWindow", "DatabaseNotFound", DatabasePath);
            return false;
        }

        if (_songs.Count == 0 || _songs.Any(song => song.SongId <= 0 || song.FolderNo <= 0))
        {
            ValidationMessage = UiText.Get("SongDeleteWindow", "SelectSong");
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanDelete()
        => !IsBusy && _songs.Count > 0 && _songs.All(song => song.SongId > 0 && song.FolderNo > 0);

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
            or System.Data.Common.DbException // reading the selected songs
            or NotSupportedException;
}
