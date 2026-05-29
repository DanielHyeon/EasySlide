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
