using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Library;

public sealed record SongMovedEventArgs(int SongId, int OldFolderNo, int NewFolderNo);

public sealed partial class SongMoveViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private IReadOnlyList<SongSummary> _songs = [];

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

    public ObservableCollection<SongFolderSummary> TargetFolders { get; } = [];

    public IAsyncRelayCommand MoveCommand { get; }

    /// <summary>The ids of the songs the last move filed in the target folder.</summary>
    public IReadOnlyList<int> MovedSongIds { get; private set; } = [];

    public void Load(
        string databasePath,
        SongSummary song,
        SongFolderSummary sourceFolder,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(song);
        ArgumentNullException.ThrowIfNull(sourceFolder);
        ArgumentNullException.ThrowIfNull(allFolders);

        Load(databasePath, [song with { FolderNo = sourceFolder.FolderNo }], allFolders);
        SourceFolderName = sourceFolder.Name;
    }

    /// <summary>Loads every selected song (FrmMain moves the whole selection of its song list).</summary>
    public void Load(
        string databasePath,
        IReadOnlyList<SongSummary> songs,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(songs);
        ArgumentNullException.ThrowIfNull(allFolders);

        _songs = songs;
        MovedSongIds = [];
        DatabasePath = NormalizePath(databasePath);
        SongId = songs.Count > 0 ? songs[0].SongId : 0;
        SourceFolderNo = songs.Count > 0 ? songs[0].FolderNo : 0;
        SourceFolderName = SongSelectionText.SourceFolderName(songs, allFolders);
        SongTitle = SongSelectionText.Heading(songs);
        // Folder 0 holds deleted songs, and a disabled folder is shown by neither app. The folder all the
        // songs are in is not a target; songs picked from several folders can go to any of them.
        int? sourceFolderNo = SongSelectionText.CommonFolderNo(songs);
        TargetFolders.ReplaceWith(allFolders.Where(folder =>
            folder.FolderNo > 0 && folder.IsEnabled && folder.FolderNo != sourceFolderNo));
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
            // The songs as they are now: FrmMain leaves out a selected song whose row is gone, and the folder a
            // song is in (kept as OldFolder) may have changed since the list was read.
            var current = (await _adminDatabase
                .GetMatchingSongsAsync(DatabasePath, _songs.Select(song => song.SongId).ToHashSet(), new HashSet<string>())
                .ConfigureAwait(true)).ToDictionary(song => song.SongId);
            var moves = new List<SongMoveRequest>();
            var missing = new List<string>();
            var alreadyThere = new List<string>();
            foreach (var song in _songs)
            {
                if (!current.TryGetValue(song.SongId, out var stored)) missing.Add(song.Title);
                else if (stored.FolderNo == targetFolderNo) alreadyThere.Add(song.Title);
                else moves.Add(new SongMoveRequest(song.SongId, stored.FolderNo, targetFolderNo));
            }

            string skipped = SongSelectionText.Skipped(alreadyThere, UiText.Get("SongMoveWindow", "AlreadyInTarget"))
                + SongSelectionText.Skipped(missing, UiText.Get("SongMoveWindow", "NotInLibrary"));
            if (moves.Count == 0)
            {
                StatusMessage = UiText.Get("SongMoveWindow", "NothingToMove") + skipped;
                return;
            }

            // One backup and one transaction for the whole selection: all the songs are moved, or none.
            var report = await _adminDatabase
                .MoveSongsAsync(DatabasePath, backupRoot, moves)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? UiText.Get("SongMoveWindow", "MoveFailed")
                    : UiText.Format("SongMoveWindow", "MoveFailedDetail", detail);
                return;
            }

            MovedSongIds = moves.Select(move => move.SongId).ToArray();
            ValidationMessage = "";
            StatusMessage = _songs.Count == 1 ? UiText.Get("SongMoveWindow", "Moved") : UiText.Format("SongMoveWindow", "MovedSeveral", moves.Count) + skipped;
            Moved?.Invoke(this, new SongMovedEventArgs(moves[0].SongId, moves[0].OldFolderNo, targetFolderNo));
        }
        catch (Exception ex) when (IsRecoverableMoveException(ex))
        {
            StatusMessage = UiText.Format("SongMoveWindow", "MoveFailedDetail", ex.Message);
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
            ValidationMessage = UiText.Get("SongMoveWindow", "DatabasePathMissing");
            return false;
        }

        if (!File.Exists(DatabasePath))
        {
            ValidationMessage = UiText.Format("SongMoveWindow", "DatabaseNotFound", DatabasePath);
            return false;
        }

        if (_songs.Count == 0 || _songs.Any(song => song.SongId <= 0 || song.FolderNo <= 0))
        {
            ValidationMessage = UiText.Get("SongMoveWindow", "SelectSong");
            return false;
        }

        if (SelectedTargetFolder is null)
        {
            ValidationMessage = UiText.Get("SongMoveWindow", "SelectTargetFolder");
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
            or System.Data.Common.DbException // reading the selected songs
            or NotSupportedException;
}
