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

public sealed partial class LibraryViewModel : ObservableObject
{
    private const string LegacyAdminDatabaseRelativePath = @"Admin\Database\EasiSlidesDb.db";

    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private IReadOnlyList<SongSummary> _loadedSongs = [];
    private bool _suppressSelectionLoad;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private SongFolderSummary? _selectedFolder;
    [ObservableProperty] private SongSummary? _selectedSong;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _statusMessage = "라이브러리 준비됨";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _activeFolderCount;
    [ObservableProperty] private int _displayedSongCount;
    [ObservableProperty] private int _totalSongCount;

    public LibraryViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        LoadSongsForSelectedFolderCommand = new AsyncRelayCommand(LoadSongsForSelectedFolderAsync, () => !IsBusy);
        MoveSelectedFolderUpCommand = new AsyncRelayCommand(MoveSelectedFolderUpAsync, CanMoveSelectedFolderUp);
        MoveSelectedFolderDownCommand = new AsyncRelayCommand(MoveSelectedFolderDownAsync, CanMoveSelectedFolderDown);
        MoveSelectedSongUpCommand = new AsyncRelayCommand(MoveSelectedSongUpAsync, CanMoveSelectedSongUp);
        MoveSelectedSongDownCommand = new AsyncRelayCommand(MoveSelectedSongDownAsync, CanMoveSelectedSongDown);
    }

    public ObservableCollection<SongFolderSummary> Folders { get; } = new();

    public ObservableCollection<SongSummary> Songs { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand LoadSongsForSelectedFolderCommand { get; }

    public IAsyncRelayCommand MoveSelectedFolderUpCommand { get; }

    public IAsyncRelayCommand MoveSelectedFolderDownCommand { get; }

    public IAsyncRelayCommand MoveSelectedSongUpCommand { get; }

    public IAsyncRelayCommand MoveSelectedSongDownCommand { get; }

    public async Task LoadAsync()
    {
        if (!TryResolveDatabasePath(out var resolvedPath, out var message))
        {
            DatabasePath = resolvedPath;
            ClearLibrary(message);
            return;
        }

        DatabasePath = resolvedPath;
        await RunBusyAsync(async () =>
        {
            var folders = await _adminDatabase.GetSongFoldersAsync(DatabasePath).ConfigureAwait(true);
            Folders.ReplaceWith(folders);
            NotifyReorderCanExecuteChanged();

            _suppressSelectionLoad = true;
            try
            {
                SelectedFolder = Folders.FirstOrDefault();
            }
            finally
            {
                _suppressSelectionLoad = false;
            }

            if (SelectedFolder is null)
            {
                _loadedSongs = [];
                ApplySearch();
                UpdateStatus();
                return;
            }

            await LoadSongsForSelectedFolderCoreAsync().ConfigureAwait(true);
        }).ConfigureAwait(true);
    }

    public async Task LoadSongsForSelectedFolderAsync()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        {
            if (!TryResolveDatabasePath(out var resolvedPath, out var message))
            {
                DatabasePath = resolvedPath;
                ClearLibrary(message);
                return;
            }

            DatabasePath = resolvedPath;
        }

        await RunBusyAsync(LoadSongsForSelectedFolderCoreAsync).ConfigureAwait(true);
    }

    public void SelectSongById(int songId)
    {
        SelectedSong = Songs.FirstOrDefault(song => song.SongId == songId) ?? SelectedSong;
    }

    public bool SelectFolderByNo(int folderNo)
    {
        var folder = Folders.FirstOrDefault(item => item.FolderNo == folderNo);
        if (folder is null)
        {
            return false;
        }

        _suppressSelectionLoad = true;
        try
        {
            SelectedFolder = folder;
        }
        finally
        {
            _suppressSelectionLoad = false;
        }

        return true;
    }

    public Task MoveSelectedFolderUpAsync()
        => MoveSelectedFolderAsync(-1);

    public Task MoveSelectedFolderDownAsync()
        => MoveSelectedFolderAsync(1);

    public Task MoveSelectedSongUpAsync()
        => MoveSelectedSongAsync(-1);

    public Task MoveSelectedSongDownAsync()
        => MoveSelectedSongAsync(1);

    partial void OnSelectedFolderChanged(SongFolderSummary? value)
    {
        NotifyReorderCanExecuteChanged();
        if (!_suppressSelectionLoad)
        {
            _ = LoadSongsForSelectedFolderAsync();
        }
    }

    partial void OnSelectedSongChanged(SongSummary? value)
        => NotifyReorderCanExecuteChanged();

    partial void OnSearchTextChanged(string value)
    {
        ApplySearch();
        UpdateStatus();
    }

    partial void OnIsBusyChanged(bool value)
    {
        LoadCommand.NotifyCanExecuteChanged();
        LoadSongsForSelectedFolderCommand.NotifyCanExecuteChanged();
        NotifyReorderCanExecuteChanged();
    }

    private async Task LoadSongsForSelectedFolderCoreAsync()
    {
        if (SelectedFolder is null)
        {
            _loadedSongs = [];
            ApplySearch();
            UpdateStatus();
            return;
        }

        _loadedSongs = await _adminDatabase
            .GetSongsAsync(DatabasePath, SelectedFolder.FolderNo)
            .ConfigureAwait(true);
        TotalSongCount = _loadedSongs.Count;
        ApplySearch();
        UpdateStatus();
    }

    private async Task MoveSelectedFolderAsync(int direction)
    {
        if (IsBusy)
        {
            return;
        }

        if (!TryEnsureWritableDatabasePath(out var message))
        {
            StatusMessage = message;
            return;
        }

        var selected = SelectedFolder;
        var selectedIndex = selected is null ? -1 : Folders.IndexOf(selected);
        var targetIndex = selectedIndex + direction;
        if (selected is null || selectedIndex < 0 || targetIndex < 0 || targetIndex >= Folders.Count)
        {
            StatusMessage = "이동할 폴더를 선택하세요.";
            return;
        }

        var target = Folders[targetIndex];
        var selectedSongId = SelectedSong?.SongId;
        IsBusy = true;
        try
        {
            var report = await _adminDatabase
                .ReorderFoldersAsync(
                    DatabasePath,
                    ResolveBackupRoot(),
                    [
                        new FolderOrderRequest(selected.FolderNo, target.FolderNo),
                        new FolderOrderRequest(target.FolderNo, selected.FolderNo),
                    ])
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                StatusMessage = FormatWriteFailure("폴더 순서 저장 실패", report);
                return;
            }

            await ReloadFoldersAndSelectAsync(target.FolderNo, selectedSongId).ConfigureAwait(true);
            StatusMessage = "폴더 순서를 저장했습니다.";
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = $"폴더 순서 저장 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MoveSelectedSongAsync(int direction)
    {
        if (IsBusy)
        {
            return;
        }

        if (!TryEnsureWritableDatabasePath(out var message))
        {
            StatusMessage = message;
            return;
        }

        if (SelectedFolder is null || SelectedSong is null)
        {
            StatusMessage = "이동할 곡을 선택하세요.";
            return;
        }

        var orderedSongs = _loadedSongs.ToList();
        var selectedIndex = orderedSongs.FindIndex(song => song.SongId == SelectedSong.SongId);
        var targetIndex = selectedIndex + direction;
        if (selectedIndex < 0 || targetIndex < 0 || targetIndex >= orderedSongs.Count)
        {
            StatusMessage = "이동할 곡을 선택하세요.";
            return;
        }

        var selectedSongId = SelectedSong.SongId;
        var selected = orderedSongs[selectedIndex];
        orderedSongs.RemoveAt(selectedIndex);
        orderedSongs.Insert(targetIndex, selected);
        var order = orderedSongs
            .Select((song, index) => new SongOrderRequest(song.SongId, index + 1))
            .ToArray();

        IsBusy = true;
        try
        {
            var report = await _adminDatabase
                .ReorderSongsAsync(DatabasePath, ResolveBackupRoot(), SelectedFolder.FolderNo, order)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                StatusMessage = FormatWriteFailure("곡 순서 저장 실패", report);
                return;
            }

            await LoadSongsForSelectedFolderCoreAsync().ConfigureAwait(true);
            SelectSongById(selectedSongId);
            StatusMessage = "곡 순서를 저장했습니다.";
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = $"곡 순서 저장 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadFoldersAndSelectAsync(int folderNo, int? songId)
    {
        var folders = await _adminDatabase.GetSongFoldersAsync(DatabasePath).ConfigureAwait(true);
        Folders.ReplaceWith(folders);
        SelectFolderByNo(folderNo);
        await LoadSongsForSelectedFolderCoreAsync().ConfigureAwait(true);
        if (songId is > 0)
        {
            SelectSongById(songId.Value);
        }
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await action().ConfigureAwait(true);
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = $"라이브러리 로드 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool TryResolveDatabasePath(out string databasePath, out string message)
    {
        var explicitPath = NormalizePath(_settings.Current.Data.AdminDatabasePath);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            databasePath = explicitPath;
            message = File.Exists(explicitPath)
                ? ""
                : $"AdminDB 파일을 찾을 수 없습니다: {explicitPath}";
            return File.Exists(explicitPath);
        }

        var workingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        if (!string.IsNullOrWhiteSpace(workingFolder))
        {
            var derivedPath = NormalizePath(Path.Combine(workingFolder, LegacyAdminDatabaseRelativePath));
            if (File.Exists(derivedPath))
            {
                databasePath = derivedPath;
                message = "";
                return true;
            }
        }

        databasePath = "";
        message = "AdminDB 경로를 설정해야 합니다.";
        return false;
    }

    private void ApplySearch()
    {
        var term = SearchText.Trim();
        var filtered = string.IsNullOrEmpty(term)
            ? _loadedSongs
            : _loadedSongs.Where(song => Matches(song, term)).ToArray();

        Songs.ReplaceWith(filtered);
        DisplayedSongCount = Songs.Count;
        var selected = SelectedSong;
        SelectedSong = selected is not null && Songs.Contains(selected)
            ? selected
            : Songs.FirstOrDefault();
        NotifyReorderCanExecuteChanged();
    }

    private void UpdateStatus()
    {
        ActiveFolderCount = SelectedFolder is null ? Folders.Count : 1;
        StatusMessage = $"{ActiveFolderCount}개 폴더, {DisplayedSongCount}개 곡 표시";
    }

    private void ClearLibrary(string message)
    {
        Folders.Clear();
        Songs.Clear();
        _loadedSongs = [];
        SelectedFolder = null;
        SelectedSong = null;
        ActiveFolderCount = 0;
        DisplayedSongCount = 0;
        TotalSongCount = 0;
        StatusMessage = message;
    }

    private static bool Matches(SongSummary song, string term)
        => Contains(song.Title, term)
           || Contains(song.AlternateTitle, term)
           || Contains(song.Category, term)
           || Contains(song.Key, term)
           || Contains(song.Lyrics, term);

    private static bool Contains(string value, string term)
        => value.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private bool TryEnsureWritableDatabasePath(out string message)
    {
        if (!string.IsNullOrWhiteSpace(DatabasePath) && File.Exists(DatabasePath))
        {
            message = "";
            return true;
        }

        if (TryResolveDatabasePath(out var resolvedPath, out message))
        {
            DatabasePath = resolvedPath;
            return true;
        }

        DatabasePath = resolvedPath;
        return false;
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

    private bool CanMoveSelectedFolderUp()
        => !IsBusy && SelectedFolder is not null && Folders.IndexOf(SelectedFolder) > 0;

    private bool CanMoveSelectedFolderDown()
        => !IsBusy && SelectedFolder is not null && Folders.IndexOf(SelectedFolder) >= 0 && Folders.IndexOf(SelectedFolder) < Folders.Count - 1;

    private bool CanMoveSelectedSongUp()
        => !IsBusy && SelectedSong is not null && _loadedSongs.ToList().FindIndex(song => song.SongId == SelectedSong.SongId) > 0;

    private bool CanMoveSelectedSongDown()
    {
        if (IsBusy || SelectedSong is null)
        {
            return false;
        }

        var index = _loadedSongs.ToList().FindIndex(song => song.SongId == SelectedSong.SongId);
        return index >= 0 && index < _loadedSongs.Count - 1;
    }

    private void NotifyReorderCanExecuteChanged()
    {
        MoveSelectedFolderUpCommand.NotifyCanExecuteChanged();
        MoveSelectedFolderDownCommand.NotifyCanExecuteChanged();
        MoveSelectedSongUpCommand.NotifyCanExecuteChanged();
        MoveSelectedSongDownCommand.NotifyCanExecuteChanged();
    }

    private static string FormatWriteFailure(string prefix, AdminDatabaseWriteReport report)
    {
        var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
        return string.IsNullOrWhiteSpace(detail) ? prefix : $"{prefix}: {detail}";
    }

    private static bool IsRecoverableLibraryException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}

internal static class LibraryCollectionExtensions
{
    public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
