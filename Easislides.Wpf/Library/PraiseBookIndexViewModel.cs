using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Library;

/// <summary>
/// 찬양집 색인 + 명명 저장/관리 뷰모델(FrmMain PraiseBook 포팅).
/// 현재 곡 목록을 머리글자별 색인으로 보여 주고(IPraiseBookIndexService),
/// 그 목록을 이름 붙여 저장하거나 저장된 찬양집을 다시 불러온다(IPraiseBookStore).
/// </summary>
public sealed partial class PraiseBookIndexViewModel : ObservableObject
{
    private readonly IPraiseBookIndexService _indexService;
    private readonly IPraiseBookStore _store;
    private readonly IPraiseBookIndexExporter _exporter = new PraiseBookIndexExporter();

    // 현재 색인 대상 곡 목록(라이브러리에서 받았거나, 저장된 찬양집을 연 결과). 저장 시 이 목록을 쓴다.
    private IReadOnlyList<PraiseBookIndexEntry> _currentEntries;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _currentBookName = string.Empty;

    public PraiseBookIndexViewModel(
        IPraiseBookIndexService indexService,
        IPraiseBookStore store,
        IEnumerable<PraiseBookIndexEntry> songs)
    {
        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        ArgumentNullException.ThrowIfNull(songs);

        _currentEntries = songs.ToList();

        SaveAsCommand = new AsyncRelayCommand<string>(SaveAsAsync);
        OpenBookCommand = new AsyncRelayCommand<string>(OpenBookAsync);
        DeleteBookCommand = new RelayCommand<string>(DeleteBook);

        RebuildIndex();
        RefreshSavedBooks();
    }

    public ObservableCollection<PraiseBookIndexGroup> Groups { get; } = new();

    public ObservableCollection<string> SavedBooks { get; } = new();

    public IAsyncRelayCommand<string> SaveAsCommand { get; }

    public IAsyncRelayCommand<string> OpenBookCommand { get; }

    public IRelayCommand<string> DeleteBookCommand { get; }

    /// <summary>현재 색인을 인쇄용 HTML 문서 문자열로 만든다(호출부가 파일로 저장). 파일 I/O 없음 → 테스트 가능.</summary>
    public string BuildIndexHtml()
    {
        var title = string.IsNullOrWhiteSpace(CurrentBookName) ? "찬양집 색인" : $"찬양집 — {CurrentBookName}";
        return _exporter.BuildHtml(title, Groups);
    }

    // 현재 목록을 머리글자 그룹 색인으로 다시 만든다.
    private void RebuildIndex()
    {
        Groups.Clear();
        var groups = _indexService.BuildIndex(_currentEntries);
        foreach (var group in groups)
        {
            Groups.Add(group);
        }

        var total = groups.Sum(group => group.Entries.Count);
        StatusText = total == 0
            ? "색인할 곡이 없습니다(곡 폴더를 먼저 선택하세요)."
            : $"{total}곡 · {groups.Count}개 머리글자 그룹";
    }

    // 저장된 찬양집 이름 목록을 새로고침(저장/삭제/이름변경 후 호출).
    private void RefreshSavedBooks()
    {
        SavedBooks.Clear();
        foreach (var name in _store.ListNames())
        {
            SavedBooks.Add(name);
        }
    }

    // 현재 목록을 이름 붙여 저장. 이름이 비면 아무것도 안 한다(호출부가 취소).
    private async Task SaveAsAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        await _store.SaveAsync(name, _currentEntries).ConfigureAwait(true);
        CurrentBookName = name;
        RefreshSavedBooks();
        StatusText = $"찬양집 저장됨: {name} ({_currentEntries.Count}곡)";
    }

    // 저장된 찬양집을 불러와 현재 목록·색인을 교체.
    private async Task OpenBookAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var entries = await _store.LoadAsync(name).ConfigureAwait(true);
        _currentEntries = entries;
        CurrentBookName = name;
        RebuildIndex();
        StatusText = $"찬양집 열림: {name} ({entries.Count}곡)";
    }

    // 저장된 찬양집을 삭제.
    private void DeleteBook(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _store.Delete(name);
        if (string.Equals(CurrentBookName, name, StringComparison.OrdinalIgnoreCase))
        {
            CurrentBookName = string.Empty;
        }

        RefreshSavedBooks();
        StatusText = $"찬양집 삭제됨: {name}";
    }
}
