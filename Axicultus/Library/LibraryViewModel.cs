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

public sealed partial class LibraryViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private IReadOnlyList<SongSummary> _loadedSongs = [];
    private bool _suppressSelectionLoad;
    private bool _pendingFolderLoad;
    private static readonly SongFolderSummary ResultFolder = new(-1, UiText.Get("LibraryViewModel", "SearchResults"), false, 0);
    private readonly ISearchUsageService _searchService;
    private SongSearchRequest? _shownSearch;
    internal int BrowserRevision { get; private set; }
    internal bool IsRefreshingSongs { get; private set; }
    internal event Action? TitleFilterApplying;
    internal event Action? TitleFilterApplied;

    [ObservableProperty] private string _folderTitleFilter = "";
    public bool CanFilterFolder => _shownSearch is null && SelectedFolder is { IsEnabled: true };
    public IEnumerable<SongFolderSummary> BrowserFolders
    {
        get
        {
            var active = Folders.Where(folder => folder.FolderNo > 0 && folder.IsEnabled);
            return _shownSearch is null ? active : active.Append(ResultFolder);
        }
    }
    public SongFolderSummary? SelectedBrowserFolder
    {
        get => _shownSearch is null ? SelectedFolder : ResultFolder;
        set { if (value is not null && value != ResultFolder) SelectedFolder = value; }
    }
    internal SongFolderSummary? SelectedSongFolder => SelectedSong is { } song
        ? Folders.FirstOrDefault(f => f.FolderNo == song.FolderNo) : SelectedFolder;

    internal void ShowSearchResults(SongSearchPage page)
    {
        if (!IsBrowsingConfiguredDatabase || !string.Equals(DatabasePath, page.Request.DatabasePath, StringComparison.OrdinalIgnoreCase)) return;
        BrowserRevision++;
        _shownSearch = page.Request;
        _suppressSelectionLoad = true;
        try { SelectedFolder = null; }
        finally { _suppressSelectionLoad = false; }
        FolderTitleFilter = "";
        SelectedSong = null;
        _loadedSongs = page.Rows.Select(result => result.Song).ToArray();
        TotalSongCount = _loadedSongs.Count;
        ApplySearch(keepOnlyExistingSelection: true);
        HasLoadedSongs = true;
        UpdateStatus();
        NotifyBrowserChanged();
    }

    private void NotifyBrowserChanged()
    {
        OnPropertyChanged(nameof(BrowserFolders));
        OnPropertyChanged(nameof(SelectedBrowserFolder));
        OnPropertyChanged(nameof(CanFilterFolder));
        OnPropertyChanged(nameof(FolderTitleFilter));
        RaiseLibraryEmptyStateChanged();
    }

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private SongFolderSummary? _selectedFolder;
    [ObservableProperty] private SongSummary? _selectedSong;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _statusMessage = UiText.Get("LibraryViewModel", "Ready");
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

    // 곡 목록을 한 번이라도 불러왔는지 — 불러오기 전(처음 화면·로딩 중)에는 "곡 없음" 안내를 띄우지 않기 위함.
    // 빈 목록이 "아직 안 불러옴"인지 "정말 비었음"인지 구분하는 핵심 플래그(검색 빈 상태 HasSearched 와 같은 취지).
    [ObservableProperty] private bool _hasLoadedSongs;

    /// <summary>Every song of the database at the path, read only (the caption console puts read lyrics right with them).</summary>
    internal Task<IReadOnlyList<SongSummary>> ReadSongsAsync(string databasePath) => _adminDatabase.GetSongsAsync(databasePath);

    public LibraryViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase, ISearchUsageService? searchService = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        _searchService = searchService ?? new SearchUsageService(adminDatabase);
        // 표시 곡 수가 바뀌면 "곡 없음" 빈 상태 안내 표시 여부도 다시 평가하도록 통지.
        Folders.CollectionChanged += (_, _) =>
        {
            RefreshJumpFolders();
            OnPropertyChanged(nameof(BrowserFolders));
            OnPropertyChanged(nameof(SelectedBrowserFolder));
        };
        Songs.CollectionChanged += (_, _) => RaiseLibraryEmptyStateChanged();
        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        LoadSongsForSelectedFolderCommand = new AsyncRelayCommand(LoadSongsForSelectedFolderAsync, () => !IsBusy);
        MoveSelectedFolderUpCommand = new AsyncRelayCommand(MoveSelectedFolderUpAsync, CanMoveSelectedFolderUp);
        MoveSelectedFolderDownCommand = new AsyncRelayCommand(MoveSelectedFolderDownAsync, CanMoveSelectedFolderDown);
        DeleteSelectedFolderCommand = new AsyncRelayCommand(DeleteSelectedFolderAsync, CanDeleteSelectedFolder);
        RecoverSelectedFolderCommand = new AsyncRelayCommand(RecoverSelectedFolderAsync, CanRecoverSelectedFolder);
        MoveSelectedSongUpCommand = new AsyncRelayCommand(MoveSelectedSongUpAsync, CanMoveSelectedSongUp);
        MoveSelectedSongDownCommand = new AsyncRelayCommand(MoveSelectedSongDownAsync, CanMoveSelectedSongDown);
        CompactDatabaseCommand = new AsyncRelayCommand(CompactDatabaseAsync, () => !IsBusy);
        JumpToInitialCommand = new RelayCommand<string>(JumpToInitial);
        // 곡 번호 표시 초기값을 설정에서 읽는다(백킹 필드 직접 설정 — 생성 시점엔 저장 트리거 불필요).
        _useSongNumbering = _settings.Get(AxicultusSettingKeys.UseSongNumbering);
    }

    // 곡 번호 표시 토글 시 설정에 저장한다(다음 실행에도 유지). 목록 텍스트는 다중 바인딩 변환기가 이 값을 보고 갱신한다.
    partial void OnUseSongNumberingChanged(bool value)
    {
        _settings.Set(AxicultusSettingKeys.UseSongNumbering, value);
        FolderTitleFilter = "";
        ApplySelectedFolderSortMode();
        OnPropertyChanged(nameof(IsFolderWordCountSortAvailable));
        OnPropertyChanged(nameof(IsWordCountSortEnabled));
    }

    // ── 라이브러리 곡 목록 빈 상태 안내 (검색 빈 상태 154 / 제목 빈 상태 155 와 같은 취지) ──
    // 곡을 한 번이라도 불러온(HasLoadedSongs) 뒤에 "표시할 곡이 0건"일 때만 안내 문구를 준다.
    // 처음 화면(아직 안 불러옴)이나 로딩 중(IsBusy)에는 빈 문자열 → 안내가 섣불리 뜨지 않는다.

    /// <summary>
    /// 곡 목록이 비었을 때 보여줄 안내 문구. 검색어가 있으면 "검색 결과 없음", 없으면 "빈 폴더" 문구를 돌려준다.
    /// 불러오기 전·로딩 중·곡이 있을 때는 빈 문자열(안내 숨김).
    /// </summary>
    public string LibraryEmptyStateMessage
    {
        get
        {
            // 로딩 중이거나, 아직 한 번도 안 불러왔거나, 보여줄 곡이 있으면 안내를 띄우지 않는다.
            if (IsBusy || !HasLoadedSongs || Songs.Count > 0)
            {
                return "";
            }

            var term = (_shownSearch?.Phrase ?? (FolderTitleFilter.Length > 0 ? FolderTitleFilter : SearchText)).Trim();
            if (_shownSearch is not null && term.Length == 0) return UiText.Get("LibraryViewModel", "NoSearchMatch");
            return term.Length > 0
                ? UiText.Format("LibraryViewModel", "NoTermMatch", term)   // searched, no result
                : UiText.Get("LibraryViewModel", "FolderEmpty");          // folder loaded, no song
        }
    }

    /// <summary>빈 상태 안내를 표시할지(LibraryEmptyStateMessage 가 비어있지 않은지) — XAML Visibility 바인딩용.</summary>
    public bool HasLibraryEmptyState => LibraryEmptyStateMessage.Length > 0;

    // 빈 상태 안내는 IsBusy·HasLoadedSongs·Songs·SearchText 4가지 입력으로 정해진다.
    // 그 입력이 바뀌는 모든 길목(아래 partial 메서드들 + 생성자의 Songs.CollectionChanged)에서 이 통지를 호출해
    // "모든 입력이 각자 변경을 통지한다"는 단순한 규칙을 지킨다.
    private void RaiseLibraryEmptyStateChanged()
    {
        OnPropertyChanged(nameof(LibraryEmptyStateMessage));
        OnPropertyChanged(nameof(HasLibraryEmptyState));
    }

    /// <summary>곡을 처음 불러온 순간(또는 해제 시) 빈 상태 안내 표시 여부를 다시 평가한다.</summary>
    partial void OnHasLoadedSongsChanged(bool value) => RaiseLibraryEmptyStateChanged();

    public SongFolderSummary? JumpFolderA => FindJumpFolder(AxicultusSettingKeys.JumpToA);
    public SongFolderSummary? JumpFolderB => FindJumpFolder(AxicultusSettingKeys.JumpToB);
    public SongFolderSummary? JumpFolderC => FindJumpFolder(AxicultusSettingKeys.JumpToC);

    internal void RefreshJumpFolders()
    {
        OnPropertyChanged(nameof(JumpFolderA));
        OnPropertyChanged(nameof(JumpFolderB));
        OnPropertyChanged(nameof(JumpFolderC));
    }

    private SongFolderSummary? FindJumpFolder(SettingKey<int> key)
    {
        var number = _settings.Get(key);
        return Folders.FirstOrDefault(folder => folder.FolderNo == number);
    }

    public ObservableCollection<SongFolderSummary> Folders { get; } = [];

    public ObservableCollection<SongSummary> Songs { get; } = [];

    internal async Task<SongSummary?> FindLegacyPraiseBookSongAsync(PraiseBookIndexEntry entry)
    {
        IEnumerable<SongSummary> songs = Songs;
        if (!string.IsNullOrWhiteSpace(DatabasePath) && File.Exists(DatabasePath))
        {
            var databasePath = DatabasePath;
            try
            {
                songs = await Task.Run(() => _adminDatabase.GetMatchingSongsAsync(databasePath, new HashSet<int>(),
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { entry.Title }));
            }
            catch (Exception ex) when (IsRecoverableLibraryException(ex) || ex is System.Data.Common.DbException)
            {
                StatusMessage = UiText.Format("LibraryViewModel", "PraiseBookLoadFailed", ex.Message);
                return null;
            }
        }
        return PraiseBookIndexEntry.MatchLegacySong(songs.Where(song => song.FolderNo > 0 &&
            string.Equals(song.Title, entry.Title, StringComparison.OrdinalIgnoreCase)), entry.Number);
    }

    /// <summary>현재 표시된 곡 목록에 나타나는 머리글자들(가나다→A~Z→#→기타) — A/B/C(초성) 점프 바에 바인딩.</summary>
    public ObservableCollection<string> AvailableInitials { get; } = [];

    /// <summary>정렬 방식 선택지(콤보박스 바인딩용) — 모드와 한글 라벨 쌍.</summary>
    public IReadOnlyList<LibrarySortOption> SortOptions { get; } =
    [
        new LibrarySortOption(LibrarySortMode.Original, UiText.Get("LibraryViewModel", "SortOriginal")),
        new LibrarySortOption(LibrarySortMode.Title, UiText.Get("LibraryViewModel", "SortTitle")),
        new LibrarySortOption(LibrarySortMode.Number, UiText.Get("LibraryViewModel", "SortNumber")),
        new LibrarySortOption(LibrarySortMode.StrokeCount, UiText.Get("LibraryViewModel", "SortStrokeCount")),
        new LibrarySortOption(LibrarySortMode.WordCount, UiText.Get("LibraryViewModel", "SortWordCount")),
    ];

    public bool IsWordCountSortEnabled
    {
        get => SortMode == LibrarySortMode.WordCount;
        set => _ = SetFolderWordCountSortEnabledAsync(value);
    }

    public bool IsFolderWordCountSortAvailable => !UseSongNumbering;

    public Task SetFolderWordCountSortEnabledAsync(bool value)
        => SetBrowserSortAsync(value ? LibrarySortMode.WordCount : LibrarySortMode.StrokeCount);

    internal async Task SetBrowserSortAsync(LibrarySortMode nextMode)
    {
        if (!IsFolderWordCountSortAvailable)
        {
            OnPropertyChanged(nameof(IsWordCountSortEnabled));
            return;
        }

        if (SortMode == nextMode) return;
        SortMode = nextMode;
        if (_shownSearch is not null) await LoadSongsForSelectedFolderAsync();
        else await PersistSelectedFolderGroupStyleAsync(nextMode switch
        {
            LibrarySortMode.WordCount => 1,
            LibrarySortMode.Number => 2,
            _ => 0
        });
    }

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

    /// <summary>곡 데이터베이스 압축·정리(레거시 Tools Compact &amp; Repair) — 백업 후 VACUUM. 실패 시 복원.</summary>
    public IAsyncRelayCommand CompactDatabaseCommand { get; }

    internal async Task LoadFolderOptionsAsync()
    {
        if (!TryResolveDatabasePath(out var path, out var message))
        {
            Folders.Clear();
            StatusMessage = message;
            return;
        }
        DatabasePath = path;
        await RunBusyAsync(async () =>
        {
            Folders.ReplaceWith(await _adminDatabase.GetSongFoldersAsync(path).ConfigureAwait(true));
            StatusMessage = Folders.Count == 0 ? UiText.Get("LibraryViewModel", "NoFolders") : "";
        }).ConfigureAwait(true);
    }

    public async Task LoadAsync()
    {
        if (!TryResolveDatabasePath(out var resolvedPath, out var message))
        {
            DatabasePath = resolvedPath;
            ClearLibrary(message);
            return;
        }

        await RunBusyAsync(async () =>
        {
            DatabasePath = resolvedPath;
            var folders = await _adminDatabase.GetSongFoldersAsync(DatabasePath).ConfigureAwait(true);
            Folders.ReplaceWith(folders);
            NotifyReorderCanExecuteChanged();

            _suppressSelectionLoad = true;
            try
            {
                SelectedFolder = ResolveInitialSelectedFolder();
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
        var ordered = Folders.ToList();
        var selected = SelectedFolder;
        if (selected is not { FolderNo: > 0 } || targetIndex < 0 || targetIndex >= ordered.Count
            || ordered[targetIndex].FolderNo <= 0 || !ordered.Remove(selected)) return;
        ordered.Insert(targetIndex, selected);
        await SaveFolderOrderAsync(ordered.Where(folder => folder.FolderNo > 0).ToArray());
    }

    internal async Task<bool> SaveFolderOrderAsync(IReadOnlyList<SongFolderSummary> ordered)
    {
        if (IsBusy) return false;
        if (!TryEnsureWritableDatabasePath(out var message))
        {
            StatusMessage = message;
            return false;
        }

        var numbers = Folders.Select(f => f.FolderNo).Where(number => number > 0).Order().ToArray();
        if (!numbers.SequenceEqual(ordered.Select(f => f.FolderNo).Order()))
        {
            StatusMessage = UiText.Get("LibraryViewModel", "FoldersChanged");
            return false;
        }
        var order = ordered.Select((f, i) => new FolderOrderRequest(f.FolderNo, numbers[i]))
            .Where(f => f.FolderNo != f.NewFolderNo).OrderBy(f => f.FolderNo).ToArray();
        if (order.Length == 0) return true;

        var remap = order.ToDictionary(f => f.FolderNo, f => f.NewFolderNo);
        int selectedNumber = SelectedFolder?.FolderNo ?? numbers[0];
        var selectedSongId = SelectedSong?.SongId;
        IsBusy = true;
        try
        {
            var report = await _adminDatabase.ReorderFoldersAsync(DatabasePath, ResolveBackupRoot(), order);
            if (!report.Succeeded)
            {
                StatusMessage = FormatWriteFailure(UiText.Get("LibraryViewModel", "FolderOrderSaveFailed"), report);
                return false;
            }

            var folderSettings = new Dictionary<string, object>();
            foreach (var key in new[] { AxicultusSettingKeys.JumpToA, AxicultusSettingKeys.JumpToB, AxicultusSettingKeys.JumpToC, AxicultusSettingKeys.SelectedSongFolderNo })
                if (remap.TryGetValue(_settings.Get(key), out var newNumber))
                    folderSettings[key.Id] = newNumber;
            bool savedSettings;
            try
            {
                savedSettings = folderSettings.Count == 0 || _settings.SetValues(folderSettings).Succeeded;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                savedSettings = false;
            }
            if (!savedSettings)
            {
                var reverse = order.Select(f => new FolderOrderRequest(f.NewFolderNo, f.FolderNo)).ToArray();
                var rollback = await _adminDatabase.ReorderFoldersAsync(DatabasePath, ResolveBackupRoot(), reverse);
                await ReloadFoldersAndSelectAsync(rollback.Succeeded ? selectedNumber : remap.GetValueOrDefault(selectedNumber, selectedNumber), selectedSongId);
                StatusMessage = rollback.Succeeded ? UiText.Get("LibraryViewModel", "ShortcutSaveFailedRestored")
                    : FormatWriteFailure(UiText.Get("LibraryViewModel", "ShortcutSaveAndRestoreFailed"), rollback);
                return false;
            }
            await ReloadFoldersAndSelectAsync(remap.GetValueOrDefault(selectedNumber, selectedNumber), selectedSongId);
            StatusMessage = UiText.Get("LibraryViewModel", "FolderOrderSaved");
            return true;
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = UiText.Format("LibraryViewModel", "FolderOrderSaveFailedDetail", ex.Message);
            return false;
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
                ? UiText.Get("LibraryViewModel", "SelectDisabledFolder")
                : UiText.Get("LibraryViewModel", "SelectEnabledFolder");
            return;
        }

        if (selected.IsEnabled == isEnabled)
        {
            StatusMessage = isEnabled
                ? UiText.Get("LibraryViewModel", "FolderAlreadyRestored")
                : UiText.Get("LibraryViewModel", "FolderAlreadyDisabled");
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
                    isEnabled ? UiText.Get("LibraryViewModel", "FolderRestoreFailed") : UiText.Get("LibraryViewModel", "FolderDeleteFailed"),
                    report);
                return;
            }

            await ReloadFoldersAndSelectAsync(selected.FolderNo, selectedSongId).ConfigureAwait(true);
            StatusMessage = isEnabled ? UiText.Get("LibraryViewModel", "FolderRestored") : UiText.Get("LibraryViewModel", "FolderDisabled");
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = isEnabled
                ? UiText.Format("LibraryViewModel", "FolderRestoreFailedDetail", ex.Message)
                : UiText.Format("LibraryViewModel", "FolderDeleteFailedDetail", ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyFolderActionCanExecuteChanged();
        }
    }

    /// <summary>
    /// 곡 데이터베이스를 압축·정리한다(레거시 Tools Compact &amp; Repair). 백업을 만든 뒤 VACUUM 으로 빈 공간을
    /// 회수한다(저장소가 실패 시 백업 복원). DB 경로가 없으면 안내만 한다.
    /// </summary>
    public async Task CompactDatabaseAsync()
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

        IsBusy = true;
        try
        {
            var report = await _adminDatabase
                .CompactDatabaseAsync(DatabasePath, ResolveBackupRoot())
                .ConfigureAwait(true);

            StatusMessage = report.Succeeded
                ? UiText.Get("LibraryViewModel", "Compacted")
                : FormatWriteFailure(UiText.Get("LibraryViewModel", "CompactFailed"), report);
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = UiText.Format("LibraryViewModel", "CompactFailedDetail", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal async Task<bool> SaveSongBackgroundAsync(int songId, string path)
    {
        if (!TryEnsureWritableDatabasePath(out var error)) { StatusMessage = error; return false; }
        var database = DatabasePath;
        var report = await _adminDatabase.SaveSongBackgroundAsync(database, ResolveBackupRoot(), songId, path);
        if (!report.Succeeded) { StatusMessage = FormatWriteFailure(UiText.Get("LibraryViewModel", "BackgroundSaveFailed"), report); return false; }
        if (DatabasePath == database)
        {
            SongSummary Update(SongSummary song) => song with { FormatData = SongFormatData.WithBackgroundPath(song.FormatData, path) };
            _loadedSongs = _loadedSongs.Select(song => song.SongId == songId ? Update(song) : song).ToArray();
            var row = Songs.FirstOrDefault(song => song.SongId == songId);
            if (row != null)
            {
                var updated = Update(row);
                bool selected = SelectedSong?.SongId == songId;
                Songs[Songs.IndexOf(row)] = updated;
                if (selected) SelectedSong = updated;
            }
        }
        return true;
    }

    public async Task<bool> SaveSongMediaAsync(SongSummary song, string path, Media.ItemMediaOptions? options = null)
    {
        var report = await _adminDatabase.SaveSongMediaAsync(DatabasePath, ResolveBackupRoot(), song.SongId, path, options);
        if (!report.Succeeded)
        {
            StatusMessage = FormatWriteFailure(UiText.Get("LibraryViewModel", "MediaSaveFailed"), report);
            return false;
        }
        await LoadSongsForSelectedFolderAsync();
        SelectSongById(song.SongId);
        return true;
    }

    public async Task MaintainSongsAsync(AdminDatabaseWriteOperation operation)
    {
        if (IsBusy) return;
        if (!TryEnsureWritableDatabasePath(out var message))
        {
            StatusMessage = message;
            return;
        }
        IsBusy = true;
        try
        {
            var report = await _adminDatabase.MaintainSongsAsync(DatabasePath, ResolveBackupRoot(), operation);
            if (report.Succeeded)
            {
                IsBusy = false;
                await LoadAsync();
                StatusMessage = UiText.Get("LibraryViewModel", "CleanupDone");
            }
            else StatusMessage = FormatWriteFailure(UiText.Get("LibraryViewModel", "CleanupFailed"), report);
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    public Task MoveSelectedSongToSongAsync(SongSummary targetSong)
        => MoveSelectedSongToIndexAsync(IndexOfSong(targetSong.SongId));

    public Task MoveSelectedSongToEndAsync()
        => MoveSelectedSongToIndexAsync(_loadedSongs.Count - 1);

    // SONG_NUMBER is the song's own number in both apps (a hymn number, 0 for none), not a place in the list,
    // so a hand-made order has nowhere to be stored. This used to renumber every song of the folder 1..N.
    public Task MoveSelectedSongToIndexAsync(int targetIndex)
    {
        StatusMessage = UiText.Get("LibraryViewModel", "SongOrderNotStored");
        return Task.CompletedTask;
    }

    partial void OnSelectedFolderChanged(SongFolderSummary? value)
    {
        BrowserRevision++;
        if (value is not null)
        {
            _shownSearch = null;
            FolderTitleFilter = "";
        }
        NotifyBrowserChanged();
        if (value is not null && _settings.Get(AxicultusSettingKeys.SelectedSongFolderNo) != value.FolderNo)
        {
            _settings.Set(AxicultusSettingKeys.SelectedSongFolderNo, value.FolderNo);
        }

        if (_shownSearch is null) ApplySelectedFolderSortMode();
        NotifyReorderCanExecuteChanged();
        NotifyFolderActionCanExecuteChanged();
        if (!_suppressSelectionLoad)
        {
            if (IsBusy) _pendingFolderLoad = true;
            else _ = LoadSongsForSelectedFolderAsync();
        }
    }

    partial void OnSelectedSongChanged(SongSummary? value)
        => NotifyReorderCanExecuteChanged();

    partial void OnFolderTitleFilterChanged(string value)
    {
        TitleFilterApplying?.Invoke();
        ApplySearch(keepOnlyExistingSelection: true);
        TitleFilterApplied?.Invoke();
        UpdateStatus();
        RaiseLibraryEmptyStateChanged();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearch();
        UpdateStatus();
        RaiseLibraryEmptyStateChanged(); // 검색어가 바뀌면 빈 상태 문구(검색어를 포함)도 다시 평가.
    }

    // 정렬 방식이 바뀌면 현재 목록을 다시 정렬해 표시(데이터 재조회 없이). 정렬 중에는 곡 수동 이동을 막으므로
    // 이동 버튼 활성 상태도 함께 갱신한다.
    partial void OnSortModeChanged(LibrarySortMode value)
    {
        ApplySearch();
        UpdateStatus();
        NotifyReorderCanExecuteChanged();
        OnPropertyChanged(nameof(IsWordCountSortEnabled));
    }

    private void ApplySelectedFolderSortMode()
    {
        var nextMode = UseSongNumbering
            ? LibrarySortMode.Number
            : ResolveFolderGroupStyleSortMode(SelectedFolder);
        if (SortMode != nextMode)
        {
            SortMode = nextMode;
        }
        else
        {
            OnPropertyChanged(nameof(IsWordCountSortEnabled));
        }
    }

    // Folder GroupStyle follows WinForms SortBy: 1 = word count, 2 = song number, anything else = title.
    private static LibrarySortMode ResolveFolderGroupStyleSortMode(SongFolderSummary? folder)
        => folder?.GroupStyle switch
        {
            1 => LibrarySortMode.WordCount,
            2 => LibrarySortMode.Number,
            _ => LibrarySortMode.StrokeCount,
        };

    private SongFolderSummary? ResolveInitialSelectedFolder()
    {
        var savedFolderNo = _settings.Get(AxicultusSettingKeys.SelectedSongFolderNo);
        if (savedFolderNo > 0)
        {
            var saved = Folders.FirstOrDefault(folder => folder.FolderNo == savedFolderNo && folder.IsEnabled);
            if (saved is not null)
            {
                return saved;
            }
        }

        return Folders.FirstOrDefault(IsPreferredInitialFolder)
            ?? Folders.FirstOrDefault(folder => folder.IsEnabled && !IsRecycleFolder(folder))
            ?? Folders.FirstOrDefault(folder => folder.IsEnabled)
            ?? Folders.FirstOrDefault();
    }

    private static bool IsPreferredInitialFolder(SongFolderSummary folder)
        => folder.IsEnabled && folder.SongCount > 0 && !IsRecycleFolder(folder);

    private static bool IsRecycleFolder(SongFolderSummary folder)
        => string.Equals(folder.Name.Trim(), "Recycle Folder", StringComparison.OrdinalIgnoreCase); // not UI text

    private async Task PersistSelectedFolderGroupStyleAsync(int groupStyle)
    {
        if (SelectedFolder is null)
        {
            return;
        }

        if (!TryEnsureWritableDatabasePath(out var message))
        {
            StatusMessage = message;
            return;
        }

        var selected = SelectedFolder;
        try
        {
            var report = await _adminDatabase
                .SaveFolderAsync(
                    DatabasePath,
                    ResolveBackupRoot(),
                    new SongFolderWriteModel(
                        selected.FolderNo,
                        selected.Name,
                        selected.IsEnabled,
                        groupStyle))
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                StatusMessage = FormatWriteFailure(UiText.Get("LibraryViewModel", "FolderSortSaveFailed"), report);
                return;
            }

            if (SelectedFolder != selected) return;
            ReplaceSelectedFolder(selected with { GroupStyle = groupStyle });
            StatusMessage = UiText.Format("LibraryViewModel", "SortedBy", SortOptions.First(option => option.Mode == SortMode).Label);
        }
        catch (Exception ex) when (IsRecoverableLibraryException(ex))
        {
            StatusMessage = UiText.Format("LibraryViewModel", "FolderSortSaveFailedDetail", ex.Message);
        }
    }

    private void ReplaceSelectedFolder(SongFolderSummary updated)
    {
        var selected = SelectedFolder;
        if (selected is null)
        {
            return;
        }

        var index = Folders.IndexOf(selected);
        if (index >= 0)
        {
            Folders[index] = updated;
        }

        _suppressSelectionLoad = true;
        try
        {
            SelectedFolder = updated;
        }
        finally
        {
            _suppressSelectionLoad = false;
        }
    }

    partial void OnIsBusyChanged(bool value)
    {
        LoadCommand.NotifyCanExecuteChanged();
        LoadSongsForSelectedFolderCommand.NotifyCanExecuteChanged();
        CompactDatabaseCommand.NotifyCanExecuteChanged();
        NotifyFolderActionCanExecuteChanged();
        NotifyReorderCanExecuteChanged();
        // 로딩이 끝나면(IsBusy=false) 비로소 "곡 없음" 안내를 평가해 띄운다.
        // (로딩 중에는 곡 목록이 갱신돼도 안내를 숨기므로, 끝나는 이 시점의 재평가가 꼭 필요하다.)
        RaiseLibraryEmptyStateChanged();
        if (!value && _pendingFolderLoad)
        {
            _pendingFolderLoad = false;
            if (_shownSearch is null) _ = LoadSongsForSelectedFolderAsync();
        }
    }

    private async Task LoadSongsForSelectedFolderCoreAsync()
    {
        if (_shownSearch is { } request)
        {
            int revision = BrowserRevision;
            var rows = await _searchService.SearchSongsAsync(request).ConfigureAwait(true);
            if (revision == BrowserRevision) ShowSearchResults(new(request, rows));
            return;
        }
        FolderTitleFilter = "";
        if (SelectedFolder is null)
        {
            _loadedSongs = [];
            ApplySearch();
            UpdateStatus();
            HasLoadedSongs = true; // 불러오기 한 사이클을 마쳤다 — 빈 목록이 "아직 안 불러옴"이 아닌 권위 있는 상태가 된다.
            return;
        }

        var folder = SelectedFolder;
        var songs = await _adminDatabase.GetSongsAsync(DatabasePath, folder.FolderNo).ConfigureAwait(true);
        if (SelectedFolder != folder) return;
        _loadedSongs = songs;
        TotalSongCount = _loadedSongs.Count;
        ApplySearch();
        UpdateStatus();
        HasLoadedSongs = true; // 폴더의 곡을 불러왔다 — 이제 곡이 0건이면 "이 폴더에 곡이 없습니다" 안내 대상.
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
            StatusMessage = UiText.Format("LibraryViewModel", "LoadFailed", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal bool IsBrowsingConfiguredDatabase => TryResolveDatabasePath(out var path, out _) &&
        string.Equals(path, DatabasePath, StringComparison.OrdinalIgnoreCase);

    private bool TryResolveDatabasePath(out string databasePath, out string message)
    {
        var explicitPath = NormalizePath(_settings.Current.Data.AdminDatabasePath);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            databasePath = explicitPath;
            message = File.Exists(explicitPath)
                ? ""
                : UiText.Format("LibraryViewModel", "DatabaseNotFound", explicitPath);
            return File.Exists(explicitPath);
        }

        var workingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        if (!string.IsNullOrWhiteSpace(workingFolder))
        {
            var derivedPath = NormalizePath(ProductStorage.SongDatabase(workingFolder));
            if (File.Exists(derivedPath))
            {
                databasePath = derivedPath;
                message = "";
                return true;
            }
        }

        databasePath = "";
        message = UiText.Get("LibraryViewModel", "DatabasePathMissing");
        return false;
    }

    private void ApplySearch(bool keepOnlyExistingSelection = false)
    {
        var selected = SelectedSong;
        IEnumerable<SongSummary> filtered = _loadedSongs;
        var term = SearchText.Trim();
        if (_shownSearch is null && term.Length > 0) filtered = filtered.Where(song => Matches(song, term));
        var title = FolderTitleFilter.Trim();
        if (CanFilterFolder && title.Length > 0) filtered = filtered.Where(song => TitleContains(song.Title, title));

        IsRefreshingSongs = true;
        try
        {
            Songs.ReplaceWith(SongOrdering.Order(filtered, SortMode, UseSongNumbering));
            DisplayedSongCount = Songs.Count;
            RefreshAvailableInitials();
            SelectedSong = selected is not null && Songs.Contains(selected) ? selected :
                keepOnlyExistingSelection || _shownSearch is not null ? null : Songs.FirstOrDefault();
            NotifyReorderCanExecuteChanged();
        }
        finally { IsRefreshingSongs = false; }
    }

    // SQLite LIKE folds only the ASCII alphabet. The folder filter treats every typed wildcard literally.
    private static bool TitleContains(string title, string term)
    {
        for (int start = 0; start <= title.Length - term.Length; start++)
        {
            int index = 0;
            while (index < term.Length && Fold(title[start + index]) == Fold(term[index])) index++;
            if (index == term.Length) return true;
        }
        return false;

        static char Fold(char c) => c is >= 'A' and <= 'Z' ? (char)(c + ('a' - 'A')) : c;
    }

    public string BrowserCountText => FolderTitleFilter.Trim().Length > 0
        ? UiText.Format("LibraryViewModel", "BrowserCountFiltered", DisplayedSongCount, TotalSongCount) : UiText.Format("LibraryViewModel", "BrowserCount", DisplayedSongCount);

    private void UpdateStatus()
    {
        OnPropertyChanged(nameof(BrowserCountText));
        ActiveFolderCount = SelectedFolder is null ? Folders.Count : 1;
        StatusMessage = FolderTitleFilter.Trim().Length > 0 ? UiText.Format("LibraryViewModel", "ShownOfTotal", TotalSongCount, DisplayedSongCount) :
            UiText.Format("LibraryViewModel", "ShownInFolders", ActiveFolderCount, DisplayedSongCount);
    }

    private void ClearLibrary(string message)
    {
        // DB 경로 실패 등으로 라이브러리를 비울 때는 "불러옴" 상태부터 해제한다.
        // 이래야 바로 아래 Songs.Clear() 통지 시점에도 빈 상태 안내가 잠깐조차 뜨지 않고,
        // 대신 상태바(StatusMessage)가 진짜 원인(경로 없음 등)을 알린다.
        _shownSearch = null;
        FolderTitleFilter = "";
        NotifyBrowserChanged();
        HasLoadedSongs = false;
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
        => CanMoveSelectedFolder(-1);

    private bool CanMoveSelectedFolderDown()
        => CanMoveSelectedFolder(1);

    private bool CanMoveSelectedFolder(int step)
    {
        var index = GetSelectedFolderIndex();
        var target = index + step;
        return !IsBusy && SelectedFolder is { FolderNo: > 0 } && index >= 0 &&
            target >= 0 && target < Folders.Count && Folders[target].FolderNo > 0;
    }

    private bool CanDeleteSelectedFolder()
        => !IsBusy && SelectedFolder is { IsEnabled: true };

    private bool CanRecoverSelectedFolder()
        => !IsBusy && SelectedFolder is { IsEnabled: false };

    // A song cannot be moved by hand: see MoveSelectedSongToIndexAsync.
    private static bool CanMoveSelectedSongUp() => false;

    private static bool CanMoveSelectedSongDown() => false;

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
        => SelectedSong is null ? -1 : IndexOfSong(SelectedSong.SongId);

    private int IndexOfSong(int songId)
    {
        for (int index = 0; index < _loadedSongs.Count; index++)
            if (_loadedSongs[index].SongId == songId) return index;
        return -1;
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
            or System.Data.Common.DbException
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
