using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed record BibleSelectionChangedEventArgs(BibleSelection Selection);

public sealed partial class BibleViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IBibleRepository _repository;
    private BiblePassageResult _currentResult = new("", [], IsSequential: true, WasLimited: false);
    private BibleSelection _baseSelection = new("", "");

    [ObservableProperty] private string _workingFolder = "";
    [ObservableProperty] private BibleVersion? _selectedVersion;
    [ObservableProperty] private BibleBook? _selectedBook;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private BibleSearchMatchMode _matchMode = BibleSearchMatchMode.AllWords;
    [ObservableProperty] private bool _showVerses = true;
    [ObservableProperty] private string _passageText = "";
    [ObservableProperty] private string _selectedPassageId = "";
    [ObservableProperty] private string _selectedPassageTitle = "";
    [ObservableProperty] private BibleSelection _selectedSelection = new("", "");
    [ObservableProperty] private BibleVersion? _previewRegion1Version;
    [ObservableProperty] private BibleVersion? _previewRegion2Version;
    [ObservableProperty] private bool _useRegion2Preview;
    [ObservableProperty] private string _previewPassageId = "";
    [ObservableProperty] private string _previewPassageTitle = "";
    [ObservableProperty] private string _previewRegionSummary = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;

    public BibleViewModel(ISettingsService settings, IBibleRepository repository)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        LoadSelectedBookCommand = new AsyncRelayCommand(LoadSelectedBookAsync, () => !IsBusy && SelectedVersion is not null && SelectedBook is not null);
        SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy && SelectedVersion is not null);
    }

    public event EventHandler<BibleSelectionChangedEventArgs>? SelectionChanged;

    public ObservableCollection<BibleVersion> Versions { get; } = new();

    public ObservableCollection<BibleBook> Books { get; } = new();

    public IReadOnlyList<BibleSearchMatchMode> MatchModes { get; } =
        [BibleSearchMatchMode.AllWords, BibleSearchMatchMode.AnyWord, BibleSearchMatchMode.ExactPhrase];

    public IAsyncRelayCommand LoadSelectedBookCommand { get; }

    public IAsyncRelayCommand SearchCommand { get; }

    public Task LoadAsync()
    {
        WorkingFolder = NormalizePath(_settings.Current.General.WorkingFolder);
        Versions.ReplaceWith(_repository.GetVersions(WorkingFolder));
        SelectedVersion = Versions.Count > 0 ? Versions[0] : null;
        PreviewRegion1Version = SelectedVersion;
        PreviewRegion2Version = Versions.FirstOrDefault(version => !Equals(version, PreviewRegion1Version));
        LoadBooksForSelectedVersion();
        ValidationMessage = Versions.Count == 0
            ? "성경 목록 데이터베이스를 찾을 수 없습니다."
            : "";
        StatusMessage = Versions.Count == 0
            ? ""
            : $"{Versions.Count}개 성경 버전을 불러왔습니다.";
        NotifyCommands();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 성경 버전의 표시 이름을 바꾼다(성경 버전 관리 — 레거시 FrmBibleRename 대응).
    /// 이름이 같으면 변경 없이 성공, 다른 버전과 중복(대소문자 무시)이면 거부.
    /// 저장소(Biblefolder.NAME UPDATE) 성공 시 목록을 다시 불러오고 같은 버전을 계속 선택한다.
    /// 반환값: 처리됐으면 true, 입력 오류·중복·저장소 실패면 false.
    /// </summary>
    public bool RenameVersion(BibleVersion version, string newName)
    {
        ArgumentNullException.ThrowIfNull(version);
        var to = (newName ?? "").Trim();
        if (to.Length == 0)
        {
            ValidationMessage = "버전 이름을 입력하세요.";
            return false;
        }

        // 이름이 같으면(대소문자 무시) 바꿀 게 없다 — 성공으로 처리.
        if (string.Equals(to, version.Name, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 다른 버전과 이름이 겹치면 거부(자기 자신은 파일명으로 제외).
        if (Versions.Any(other =>
                !string.Equals(other.FileName, version.FileName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(other.Name, to, StringComparison.OrdinalIgnoreCase)))
        {
            ValidationMessage = "이미 있는 버전 이름입니다. 다른 이름을 입력하세요.";
            return false;
        }

        bool renamed;
        try
        {
            renamed = _repository.RenameVersion(WorkingFolder, version.FileName, to);
        }
        catch (Exception ex) when (ex is IOException or System.Data.SQLite.SQLiteException or UnauthorizedAccessException)
        {
            ValidationMessage = $"버전 이름을 바꾸지 못했습니다: {to}";
            return false;
        }

        if (!renamed)
        {
            ValidationMessage = $"버전을 찾을 수 없습니다: {version.Name}";
            return false;
        }

        // NAME 만 바뀌었으므로 목록을 통째로 재로드하지 않고 해당 레코드만 제자리 교체한다.
        // (전체 재로드는 GetVersions DB 왕복 + 공유 ComboBox 의 선택 널 깜빡임 + 동일 버전 books 재조회를 유발.)
        var renamedVersion = version with { Name = to };
        var index = Versions.IndexOf(version);
        if (index >= 0)
        {
            var wasSelected = ReferenceEquals(SelectedVersion, version);
            Versions[index] = renamedVersion;
            if (wasSelected)
            {
                // 같은 파일(books/passage 불변)이지만 선택 레코드 참조를 새 이름 레코드로 맞춘다.
                SelectedVersion = renamedVersion;
            }
        }

        ValidationMessage = "";
        StatusMessage = $"버전 이름 변경: {version.Name} → {to}";
        return true;
    }

    /// <summary>선택 버전을 한 칸 위로 올린다(맨 위면 false). 레거시 성경 버전 순서 변경.</summary>
    public bool MoveVersionUp(BibleVersion version) => MoveVersion(version, -1);

    /// <summary>선택 버전을 한 칸 아래로 내린다(맨 아래면 false).</summary>
    public bool MoveVersionDown(BibleVersion version) => MoveVersion(version, +1);

    /// <summary>
    /// 보이는 버전의 표시 순서를 한 칸 옮긴다(delta=-1 위 / +1 아래). 경계를 벗어나면 변경 없이 false.
    /// 저장소가 DISPLAYORDER 를 재기록한 뒤 목록을 다시 불러오고 같은 버전을 계속 선택한다.
    /// </summary>
    private bool MoveVersion(BibleVersion version, int delta)
    {
        ArgumentNullException.ThrowIfNull(version);
        var index = Versions.IndexOf(version);
        if (index < 0)
        {
            // 참조가 달라졌을 수 있어 파일명으로 한 번 더 찾는다(재로드 후 인스턴스 교체 대비).
            index = IndexOfByFile(version.FileName);
        }

        var target = index + delta;
        if (index < 0 || target < 0 || target >= Versions.Count)
        {
            return false; // 목록에 없거나 경계 밖 — 변경 없음.
        }

        var ordered = Versions.Select(v => v.FileName).ToList();
        (ordered[index], ordered[target]) = (ordered[target], ordered[index]);

        bool reordered;
        try
        {
            reordered = _repository.ReorderVersions(WorkingFolder, ordered);
        }
        catch (Exception ex) when (ex is IOException or System.Data.SQLite.SQLiteException or UnauthorizedAccessException)
        {
            ValidationMessage = "버전 순서를 바꾸지 못했습니다.";
            return false;
        }

        if (!reordered)
        {
            ValidationMessage = "버전 순서를 바꾸지 못했습니다.";
            return false;
        }

        ReloadPreservingSelection(version.FileName);
        ValidationMessage = "";
        StatusMessage = $"버전 순서 변경: {version.Name}";
        return true;
    }

    /// <summary>
    /// 성경 버전을 삭제한다(저장소는 숨김 처리 — 본문 보존, 되돌릴 수 있음). 삭제 후 인접 버전을 선택한다.
    /// 저장소 실패·없는 버전이면 false.
    /// </summary>
    public bool DeleteVersion(BibleVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        // 삭제 후 선택할 인접 버전 파일명을 미리 정한다(다음 것, 없으면 이전 것).
        var index = IndexOfByFile(version.FileName);
        string? nextSelection = null;
        if (index >= 0)
        {
            if (index + 1 < Versions.Count)
            {
                nextSelection = Versions[index + 1].FileName;
            }
            else if (index - 1 >= 0)
            {
                nextSelection = Versions[index - 1].FileName;
            }
        }

        bool deleted;
        try
        {
            deleted = _repository.DeleteVersion(WorkingFolder, version.FileName);
        }
        catch (Exception ex) when (ex is IOException or System.Data.SQLite.SQLiteException or UnauthorizedAccessException)
        {
            ValidationMessage = $"버전을 삭제하지 못했습니다: {version.Name}";
            return false;
        }

        if (!deleted)
        {
            ValidationMessage = $"버전을 삭제하지 못했습니다: {version.Name}";
            return false;
        }

        ReloadPreservingSelection(nextSelection);
        ValidationMessage = "";
        StatusMessage = $"버전 삭제(숨김): {version.Name}";
        return true;
    }

    /// <summary>관리 UI 가 "추가" 후보(숨김 + HolyBibles 신규 파일)를 받는 통로.</summary>
    public IReadOnlyList<BibleAddableVersion> GetAddableVersions()
        => _repository.GetAddableVersions(WorkingFolder);

    /// <summary>
    /// 성경 버전을 추가한다(HolyBibles 의 파일을 보이는 버전으로 등록·숨김 복구). 이름이 비거나 보이는 버전과
    /// 겹치면(대소문자 무시) 거부. 성공 시 목록을 다시 불러오고 새 버전을 선택한다.
    /// </summary>
    public bool AddVersion(string fileName, string name)
    {
        var to = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            ValidationMessage = "추가할 성경 파일을 선택하세요.";
            return false;
        }

        if (to.Length == 0)
        {
            ValidationMessage = "버전 이름을 입력하세요.";
            return false;
        }

        // 보이는 버전과 이름이 겹치면 거부(이름 변경과 동일 규칙).
        if (Versions.Any(other => string.Equals(other.Name, to, StringComparison.OrdinalIgnoreCase)))
        {
            ValidationMessage = "이미 있는 버전 이름입니다. 다른 이름을 입력하세요.";
            return false;
        }

        bool added;
        try
        {
            added = _repository.AddVersion(WorkingFolder, fileName, to);
        }
        catch (Exception ex) when (ex is IOException or System.Data.SQLite.SQLiteException or UnauthorizedAccessException)
        {
            ValidationMessage = $"버전을 추가하지 못했습니다: {to}";
            return false;
        }

        if (!added)
        {
            ValidationMessage = $"버전을 추가하지 못했습니다: {to}";
            return false;
        }

        ReloadPreservingSelection(fileName);
        ValidationMessage = "";
        StatusMessage = $"버전 추가: {to}";
        return true;
    }

    private int IndexOfByFile(string fileName)
    {
        for (var i = 0; i < Versions.Count; i++)
        {
            if (string.Equals(Versions[i].FileName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    // 저장소가 진실의 원천이므로 추가·삭제·순서 변경 후엔 목록을 통째로 다시 불러온다(인덱스·순서 정합).
    // 그런 다음 주어진 파일명의 버전을 다시 선택해 운영자의 맥락을 보존한다(없으면 첫 버전).
    private void ReloadPreservingSelection(string? selectFileName)
    {
        Versions.ReplaceWith(_repository.GetVersions(WorkingFolder));
        var match = selectFileName is null
            ? null
            : Versions.FirstOrDefault(v => string.Equals(v.FileName, selectFileName, StringComparison.OrdinalIgnoreCase));
        SelectedVersion = match ?? (Versions.Count > 0 ? Versions[0] : null);
    }

    public Task LoadSelectedBookAsync()
    {
        if (SelectedVersion is null || SelectedBook is null)
        {
            ValidationMessage = "성경 버전과 책을 선택하세요.";
            NotifyCommands();
            return Task.CompletedTask;
        }

        return ExecuteOperationAsync(() =>
        {
            _currentResult = _repository.LoadBook(SelectedVersion, SelectedBook.Number, ShowVerses);
            PassageText = _currentResult.Text;
            ClearSelection();
            StatusMessage = $"{_currentResult.Locations.Count}개 구절을 불러왔습니다.";
        });
    }

    public Task SearchAsync()
    {
        if (SelectedVersion is null)
        {
            ValidationMessage = "성경 버전을 선택하세요.";
            NotifyCommands();
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            ValidationMessage = "검색어를 입력하세요.";
            NotifyCommands();
            return Task.CompletedTask;
        }

        return ExecuteOperationAsync(() =>
        {
            _currentResult = _repository.Search(SelectedVersion, Books, SearchText, MatchMode, ShowVerses);
            PassageText = _currentResult.Text;
            ClearSelection();
            var suffix = _currentResult.WasLimited ? " 결과가 3000개로 제한되었습니다." : "";
            StatusMessage = $"{_currentResult.Locations.Count}개 구절을 찾았습니다.{suffix}";
        });
    }

    public BibleSelection BuildSelection(int selectionStart, int selectionLength)
    {
        if (SelectedVersion is null)
        {
            ValidationMessage = "성경 버전을 선택하세요.";
            NotifyCommands();
            return new BibleSelection("", "");
        }

        var selection = _repository.BuildSelection(
            SelectedVersion,
            Books,
            _currentResult,
            selectionStart,
            selectionLength);

        _baseSelection = selection;
        UpdatePreviewSelection();

        var selected = SelectedSelection;
        ValidationMessage = string.IsNullOrWhiteSpace(selected.IdString)
            ? "선택된 구절이 없습니다."
            : "";
        if (!string.IsNullOrWhiteSpace(selected.IdString))
        {
            StatusMessage = "성경 구절을 선택했습니다.";
            SelectionChanged?.Invoke(this, new BibleSelectionChangedEventArgs(selected));
        }

        NotifyCommands();
        return selected;
    }

    partial void OnSelectedVersionChanged(BibleVersion? value)
    {
        LoadBooksForSelectedVersion();
        PreviewRegion1Version = value;
        if (PreviewRegion2Version is null || Equals(PreviewRegion2Version, value))
        {
            PreviewRegion2Version = Versions.FirstOrDefault(version => !Equals(version, value));
        }

        UpdatePreviewSelection();
        NotifyCommands();
    }

    partial void OnSelectedBookChanged(BibleBook? value)
        => NotifyCommands();

    partial void OnPreviewRegion1VersionChanged(BibleVersion? value)
        => UpdatePreviewSelection();

    partial void OnPreviewRegion2VersionChanged(BibleVersion? value)
        => UpdatePreviewSelection();

    partial void OnUseRegion2PreviewChanged(bool value)
        => UpdatePreviewSelection();

    partial void OnIsBusyChanged(bool value)
        => NotifyCommands();

    private Task ExecuteOperationAsync(Action operation)
    {
        if (IsBusy)
        {
            return Task.CompletedTask;
        }

        IsBusy = true;
        try
        {
            ValidationMessage = "";
            operation();
        }
        catch (Exception ex) when (IsRecoverableBibleException(ex))
        {
            StatusMessage = $"성경 데이터를 불러오지 못했습니다: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }

        return Task.CompletedTask;
    }

    private void LoadBooksForSelectedVersion()
    {
        Books.Clear();
        if (SelectedVersion is null)
        {
            SelectedBook = null;
            PassageText = "";
            return;
        }

        Books.ReplaceWith(_repository.GetBooks(SelectedVersion));
        SelectedBook = Books.Count > 0 ? Books[0] : null;
    }

    private void ClearSelection()
    {
        _baseSelection = new BibleSelection("", "");
        SelectedSelection = new BibleSelection("", "");
        SelectedPassageId = "";
        SelectedPassageTitle = "";
        PreviewPassageId = "";
        PreviewPassageTitle = "";
        PreviewRegionSummary = BuildPreviewRegionSummary();
    }

    private void UpdatePreviewSelection()
    {
        PreviewRegionSummary = BuildPreviewRegionSummary();
        if (string.IsNullOrWhiteSpace(_baseSelection.IdString))
        {
            SelectedSelection = new BibleSelection("", "");
            SelectedPassageId = "";
            SelectedPassageTitle = "";
            PreviewPassageId = "";
            PreviewPassageTitle = "";
            return;
        }

        var region1 = PreviewRegion1Version ?? SelectedVersion;
        if (region1 is null)
        {
            SelectedSelection = new BibleSelection("", "");
            SelectedPassageId = "";
            SelectedPassageTitle = "";
            PreviewPassageId = "";
            PreviewPassageTitle = "";
            return;
        }

        var region2 = UseRegion2Preview ? PreviewRegion2Version : null;
        if (region2 is not null &&
            string.Equals(region2.FileName, region1.FileName, StringComparison.OrdinalIgnoreCase))
        {
            region2 = null;
        }

        var preview = _repository.ChangeSelectionVersions(_baseSelection.Title, _baseSelection.IdString, region1, region2);
        SelectedSelection = preview;
        SelectedPassageId = preview.IdString;
        SelectedPassageTitle = preview.Title;
        PreviewPassageId = preview.IdString;
        PreviewPassageTitle = preview.Title;
        PreviewRegionSummary = BuildPreviewRegionSummary(region1, region2);
    }

    private string BuildPreviewRegionSummary()
        => BuildPreviewRegionSummary(PreviewRegion1Version ?? SelectedVersion, UseRegion2Preview ? PreviewRegion2Version : null);

    private static string BuildPreviewRegionSummary(BibleVersion? region1, BibleVersion? region2)
    {
        if (region1 is null)
        {
            return "";
        }

        if (region2 is not null &&
            string.Equals(region2.FileName, region1.FileName, StringComparison.OrdinalIgnoreCase))
        {
            region2 = null;
        }

        return region2 is null ? region1.Name : $"{region1.Name} / {region2.Name}";
    }

    private void NotifyCommands()
    {
        LoadSelectedBookCommand.NotifyCanExecuteChanged();
        SearchCommand.NotifyCanExecuteChanged();
    }

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableBibleException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException
            or System.Data.SQLite.SQLiteException;
}
