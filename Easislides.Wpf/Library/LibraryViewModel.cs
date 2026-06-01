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
    [ObservableProperty] private bool _canDeleteFolder;
    [ObservableProperty] private bool _canRecoverFolder;
    // 곡 목록 정렬 방식(원래순서/제목/곡번호). 기본 Original = DB 순서(무회귀). 바뀌면 목록을 다시 정렬해 표시.
    [ObservableProperty] private LibrarySortMode _sortMode = LibrarySortMode.Original;
    // 곡 목록에 곡 번호 표시(레거시 Use Song Numbering). 설정과 동기화 — 토글 시 설정에 저장하고 목록 표시를 갱신.
    // 단일 쓰기 가정: 이 값은 오직 여기(메뉴 토글)에서만 바뀐다. 추후 Settings 창 등 두 번째 쓰기 경로가 생기면
    // SettingsChanged 를 구독해 백킹 필드를 동기화해야 한다(지금은 구독 불필요).
    [ObservableProperty] private bool _useSongNumbering;

    public LibraryViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        LoadSongsForSelectedFolderCommand = new AsyncRelayCommand(LoadSongsForSelectedFolderAsync, () => !IsBusy);
        MoveSelectedFolderUpCommand = new AsyncRelayCommand(MoveSelectedFolderUpAsync, CanMoveSelectedFolderUp);
        MoveSelectedFolderDownCommand = new AsyncRelayCommand(MoveSelectedFolderDownAsync, CanMoveSelectedFolderDown);
        DeleteSelectedFolderCommand = new AsyncRelayCommand(DeleteSelectedFolderAsync, CanDeleteSelectedFolder);
        RecoverSelectedFolderCommand = new AsyncRelayCommand(RecoverSelectedFolderAsync, CanRecoverSelectedFolder);
        MoveSelectedSongUpCommand = new AsyncRelayCommand(MoveSelectedSongUpAsync, CanMoveSelectedSongUp);
        MoveSelectedSongDownCommand = new AsyncRelayCommand(MoveSelectedSongDownAsync, CanMoveSelectedSongDown);
        JumpToInitialCommand = new RelayCommand<string>(JumpToInitial);
        // 곡 번호 표시 초기값을 설정에서 읽는다(백킹 필드 직접 설정 — 생성 시점엔 저장 트리거 불필요).
        _useSongNumbering = _settings.Get(EasiSettingKeys.UseSongNumbering);
    }

    // 곡 번호 표시 토글 시 설정에 저장한다(다음 실행에도 유지). 목록 텍스트는 다중 바인딩 변환기가 이 값을 보고 갱신한다.
    partial void OnUseSongNumberingChanged(bool value)
        => _settings.Set(EasiSettingKeys.UseSongNumbering, value);

    public ObservableCollection<SongFolderSummary> Folders { get; } = new();

    public ObservableCollection<SongSummary> Songs { get; } = new();

    /// <summary>현재 표시된 곡 목록에 나타나는 머리글자들(가나다→A~Z→#→기타) — A/B/C(초성) 점프 바에 바인딩.</summary>
    public ObservableCollection<string> AvailableInitials { get; } = new();

    /// <summary>정렬 방식 선택지(콤보박스 바인딩용) — 모드와 한글 라벨 쌍.</summary>
    public IReadOnlyList<LibrarySortOption> SortOptions { get; } =
    [
        new LibrarySortOption(LibrarySortMode.Original, "원래 순서"),
        new LibrarySortOption(LibrarySortMode.Title, "제목순"),
        new LibrarySortOption(LibrarySortMode.Number, "곡 번호순"),
    ];

    /// <summary>색인 머리글자로 점프 — 그 글자로 시작하는 첫 곡을 선택한다(레거시 FrmMain A/B/C 점프).</summary>
    public IRelayCommand<string> JumpToInitialCommand { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand LoadSongsForSelectedFolderCommand { get; }

    public IAsyncRelayCommand MoveSelectedFolderUpCommand { get; }

    public IAsyncRelayCommand MoveSelectedFolderDownCommand { get; }

    public IAsyncRelayCommand DeleteSelectedFolderCommand { get; }

    public IAsyncRelayCommand RecoverSelectedFolderCommand { get; }

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

    /// <summary>
    /// 색인 머리글자(예: "ㄱ", "A", "#")로 점프 — 표시된 곡 중 그 머리글자로 시작하는 첫 곡을 선택한다.
    /// 일치하는 곡이 없거나 빈 글자면 아무것도 하지 않는다(레거시 FrmMain 곡 목록 A/B/C 점프 대응).
    /// </summary>
    public void JumpToInitial(string? initial)
    {
        if (string.IsNullOrWhiteSpace(initial))
        {
            return;
        }

        // 공백뿐인 제목은 머리글자 목록(OrderedDistinctInitials)에서 제외되므로 점프 대상에서도 똑같이 제외해
        // 스트립 글자와 점프 결과가 어긋나지 않게 한다(빈 제목이 엉뚱하게 "기타" 점프를 가로채지 않도록).
        var target = Songs.FirstOrDefault(song =>
            !string.IsNullOrWhiteSpace(song.Title) &&
            string.Equals(SongInitialGrouping.GetInitial(song.Title), initial, StringComparison.Ordinal));
        if (target is not null)
        {
            SelectedSong = target;
        }
    }

    // 표시된 곡 목록의 머리글자(색인 글자)들을 다시 모은다 — 점프 바에 보일 글자 목록.
    private void RefreshAvailableInitials()
        => AvailableInitials.ReplaceWith(SongInitialGrouping.OrderedDistinctInitials(Songs.Select(s => s.Title)));

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
        => MoveSelectedFolderToIndexAsync(GetSelectedFolderIndex() - 1);

    public Task MoveSelectedFolderDownAsync()
        => MoveSelectedFolderToIndexAsync(GetSelectedFolderIndex() + 1);

    public Task DeleteSelectedFolderAsync()
        => SetSelectedFolderEnabledAsync(isEnabled: false);

    public Task RecoverSelectedFolderAsync()
        => SetSelectedFolderEnabledAsync(isEnabled: true);

    public Task MoveSelectedSongUpAsync()
        => MoveSelectedSongToIndexAsync(GetSelectedSongIndex() - 1);

    public Task MoveSelectedSongDownAsync()
        => MoveSelectedSongToIndexAsync(GetSelectedSongIndex() + 1);

    public async Task MoveSelectedFolderToIndexAsync(int targetIndex)
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
        if (selected is null || selectedIndex < 0 || targetIndex < 0 || targetIndex >= Folders.Count)
        {
            StatusMessage = "이동할 폴더를 선택하세요.";
            return;
        }

        if (selectedIndex == targetIndex)
        {
            return;
        }

        var selectedSongId = SelectedSong?.SongId;
        var orderedFolders = Folders.ToList();
        var folderNumbers = orderedFolders
            .Select(folder => folder.FolderNo)
            .OrderBy(folderNo => folderNo)
            .ToArray();
        orderedFolders.RemoveAt(selectedIndex);
        orderedFolders.Insert(targetIndex, selected);

        var order = orderedFolders
            .Select((folder, index) => new FolderOrderRequest(folder.FolderNo, folderNumbers[index]))
            .Where(item => item.FolderNo != item.NewFolderNo)
            .OrderBy(item => item.FolderNo)
            .ToArray();

        if (order.Length == 0)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var report = await _adminDatabase
                .ReorderFoldersAsync(DatabasePath, ResolveBackupRoot(), order)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                StatusMessage = FormatWriteFailure("폴더 순서 저장 실패", report);
                return;
            }

            await ReloadFoldersAndSelectAsync(folderNumbers[targetIndex], selectedSongId).ConfigureAwait(true);
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

    private async Task SetSelectedFolderEnabledAsync(bool isEnabled)
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
        if (selected is null)
        {
            StatusMessage = isEnabled
                ? "Select a disabled folder to restore."
                : "Select an enabled folder to delete.";
            return;
        }

        if (selected.IsEnabled == isEnabled)
        {
            StatusMessage = isEnabled
                ? "Selected folder is already restored."
                : "Selected folder is already disabled.";
            return;
        }

        var selectedSongId = SelectedSong?.SongId;
        IsBusy = true;
        try
        {
            var report = isEnabled
                ? await _adminDatabase
                    .RecoverFoldersAsync(DatabasePath, ResolveBackupRoot(), [new FolderRecoveryRequest(selected.FolderNo)])
                    .ConfigureAwait(true)
                : await _adminDatabase
                    .SoftDeleteFoldersAsync(DatabasePath, ResolveBackupRoot(), [new FolderDeleteRequest(selected.FolderNo)])
                    .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                StatusMessage = FormatWriteFailure(
                    isEnabled ? "Folder restore failed" : "Folder delete failed",
                    report);
                return;
            }

            await ReloadFoldersAndSelectAsync(selected.FolderNo, selectedSongId).ConfigureAwait(true);
            StatusMessage = isEnabled ? "Folder restored." : "Folder disabled.";
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = isEnabled
                ? $"Folder restore failed: {ex.Message}"
                : $"Folder delete failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyFolderActionCanExecuteChanged();
        }
    }

    public Task MoveSelectedSongToSongAsync(SongSummary targetSong)
        => MoveSelectedSongToIndexAsync(_loadedSongs.ToList().FindIndex(song => song.SongId == targetSong.SongId));

    public Task MoveSelectedSongToEndAsync()
        => MoveSelectedSongToIndexAsync(_loadedSongs.Count - 1);

    public async Task MoveSelectedSongToIndexAsync(int targetIndex)
    {
        if (IsBusy)
        {
            return;
        }

        // 수동 이동(위/아래 버튼·드래그 모두)은 "원래 순서"에서만 허용 — 정렬 중에는 화면 순서와 DB 순서가 달라
        // 엉뚱한 위치에 저장된다. 버튼은 CanExecute 로, 드래그(LibraryWindow)는 모든 이동의 길목인 여기서 막는다(심층 방어).
        if (SortMode != LibrarySortMode.Original)
        {
            StatusMessage = "정렬 중에는 곡을 이동할 수 없습니다. '원래 순서'로 바꾸세요.";
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
        if (selectedIndex < 0 || targetIndex < 0 || targetIndex >= orderedSongs.Count)
        {
            StatusMessage = "이동할 곡을 선택하세요.";
            return;
        }

        if (selectedIndex == targetIndex)
        {
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

    partial void OnSelectedFolderChanged(SongFolderSummary? value)
    {
        NotifyReorderCanExecuteChanged();
        NotifyFolderActionCanExecuteChanged();
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

    // 정렬 방식이 바뀌면 현재 목록을 다시 정렬해 표시(데이터 재조회 없이). 정렬 중에는 곡 수동 이동을 막으므로
    // 이동 버튼 활성 상태도 함께 갱신한다.
    partial void OnSortModeChanged(LibrarySortMode value)
    {
        ApplySearch();
        UpdateStatus();
        NotifyReorderCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        LoadCommand.NotifyCanExecuteChanged();
        LoadSongsForSelectedFolderCommand.NotifyCanExecuteChanged();
        NotifyFolderActionCanExecuteChanged();
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

        // 검색으로 거른 뒤 선택한 정렬 방식으로 정렬해 표시(Original 이면 입력 순서 그대로 — 무회귀).
        var ordered = SongOrdering.Order(filtered, SortMode);
        Songs.ReplaceWith(ordered);
        DisplayedSongCount = Songs.Count;
        RefreshAvailableInitials();
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
        AvailableInitials.Clear(); // 곡이 없으면 머리글자도 없음 — 점프 바도 함께 비운다(스트립이 stale 로 남지 않게).
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

    private bool CanDeleteSelectedFolder()
        => !IsBusy && SelectedFolder is { IsEnabled: true };

    private bool CanRecoverSelectedFolder()
        => !IsBusy && SelectedFolder is { IsEnabled: false };

    // 곡 수동 이동(위/아래)은 "원래 순서"에서만 의미가 있다 — 제목순/번호순으로 보고 있을 때는 화면 순서와
    // DB 순서가 달라 운영자가 보는 이웃과 실제 이동 대상이 어긋난다. 그래서 정렬 중에는 이동을 비활성화한다.
    private bool CanMoveSelectedSongUp()
        => !IsBusy && SortMode == LibrarySortMode.Original && SelectedSong is not null
           && _loadedSongs.ToList().FindIndex(song => song.SongId == SelectedSong.SongId) > 0;

    private bool CanMoveSelectedSongDown()
    {
        if (IsBusy || SortMode != LibrarySortMode.Original || SelectedSong is null)
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

    private void NotifyFolderActionCanExecuteChanged()
    {
        CanDeleteFolder = CanDeleteSelectedFolder();
        CanRecoverFolder = CanRecoverSelectedFolder();
        DeleteSelectedFolderCommand.NotifyCanExecuteChanged();
        RecoverSelectedFolderCommand.NotifyCanExecuteChanged();
    }

    private int GetSelectedFolderIndex()
        => SelectedFolder is null ? -1 : Folders.IndexOf(SelectedFolder);

    private int GetSelectedSongIndex()
        => SelectedSong is null ? -1 : _loadedSongs.ToList().FindIndex(song => song.SongId == SelectedSong.SongId);

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
