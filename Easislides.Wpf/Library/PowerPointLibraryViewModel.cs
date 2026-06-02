using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Library;

/// <summary>PowerPoint 갤러리 항목 — 파일 경로와 표시용 파일명.</summary>
public sealed class PowerPointFileItem
{
    public PowerPointFileItem(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
    }

    public string FilePath { get; }

    public string FileName { get; }
}

/// <summary>
/// PowerPoint 폴더 브라우저(FrmMain PowerP 탭 포팅) — 폴더의 PPT 덱을 목록으로 보여 주고,
/// 선택한 덱을 예배 순서에 추가한다. 폴더 탐색은 IPowerPointLibraryService, 추가는 주입된 콜백(=MainViewModel).
/// </summary>
public sealed partial class PowerPointLibraryViewModel : ObservableObject
{
    private readonly IPowerPointLibraryService _service;
    private readonly Action<string> _addToQueue;
    // 폴더에서 읽은 전체 PPT(검색 필터 적용 전 원본). 검색 상자는 이 목록에서 걸러 Presentations 에 보여 준다.
    private readonly List<PowerPointFileItem> _allFiles = new();

    [ObservableProperty]
    private string _folderPath = string.Empty;

    // 검색어 — 파일명에 이 글자가 든 PPT 만 목록에 보인다(대소문자 무시). 비우면 전부. 큰 폴더에서 빠르게 찾는 현대 기능.
    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommand))]
    private PowerPointFileItem? _selectedFile;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 하위 폴더 포함 토글 시 즉시 다시 읽는다.
    [ObservableProperty]
    private bool _includeSubfolders;

    public PowerPointLibraryViewModel(
        IPowerPointLibraryService service,
        Action<string> addToQueue,
        string initialFolder)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _addToQueue = addToQueue ?? throw new ArgumentNullException(nameof(addToQueue));
        _folderPath = initialFolder ?? string.Empty;

        LoadCommand = new RelayCommand(Load);
        AddSelectedCommand = new RelayCommand(AddSelected, () => SelectedFile is not null);
    }

    public ObservableCollection<PowerPointFileItem> Presentations { get; } = new();

    public IRelayCommand LoadCommand { get; }

    public IRelayCommand AddSelectedCommand { get; }

    partial void OnIncludeSubfoldersChanged(bool value) => Load();

    // 검색어가 바뀌면 다시 읽지 않고(폴더 탐색은 비쌈) 이미 읽어 둔 전체 목록에서 걸러 보여 준다.
    partial void OnFilterTextChanged(string value) => ApplyFilter();

    // 현재 폴더의 PPT 파일 목록을 다시 읽는다(전체를 _allFiles 에 담고, 검색어 적용해 화면 목록 구성).
    private void Load()
    {
        _allFiles.Clear();
        foreach (var path in _service.EnumeratePresentations(FolderPath, IncludeSubfolders))
        {
            _allFiles.Add(new PowerPointFileItem(path));
        }

        ApplyFilter();
    }

    // 검색어로 _allFiles 를 걸러 Presentations(화면 목록)를 다시 만든다 — 순수 비교는 FileNameFilter 가 맡는다.
    private void ApplyFilter()
    {
        Presentations.Clear();
        foreach (var file in _allFiles.Where(f => FileNameFilter.Matches(f.FileName, FilterText)))
        {
            Presentations.Add(file);
        }

        // 선택한 파일이 걸러져 목록에서 빠지면 선택을 해제한다 — 숨은(안 보이는) 항목이 선택된 채 "추가"되는 사고를 막는다.
        // ImageLibraryViewModel 와 동일한 가드: 바인딩된 컬렉션을 다시 만들 때 선택 유효성은 VM 이 직접 책임진다(Selector reset 에 기대지 않음).
        if (SelectedFile is not null && !Presentations.Contains(SelectedFile))
        {
            SelectedFile = null;
        }

        if (_allFiles.Count == 0)
        {
            StatusText = "PowerPoint 파일이 없습니다(폴더를 확인하세요).";
        }
        else if (Presentations.Count == _allFiles.Count)
        {
            StatusText = $"{_allFiles.Count}개 PowerPoint";
        }
        else
        {
            StatusText = $"{Presentations.Count}/{_allFiles.Count}개 PowerPoint(검색 '{FilterText.Trim()}')";
        }
    }

    // 선택한 덱을 예배 순서에 추가(MainViewModel.AddPowerPoint 위임).
    private void AddSelected()
    {
        if (SelectedFile is null)
        {
            return;
        }

        _addToQueue(SelectedFile.FilePath);
        StatusText = $"예배 순서에 추가: {SelectedFile.FileName}";
    }
}
