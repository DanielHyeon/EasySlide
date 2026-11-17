using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Support;

namespace Easislides.Wpf.Library;

/// <summary>미디어 갤러리 항목 — 파일 경로와 표시용 파일명.</summary>
public sealed class MediaFileItem
{
    public MediaFileItem(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
    }

    public string FilePath { get; }

    public string FileName { get; }
}

/// <summary>
/// 미디어 폴더 브라우저(FrmMain Media 탭 포팅) — 폴더의 동영상·오디오 파일을 목록으로 보여 주고,
/// 선택한 미디어를 예배 순서에 추가한다. 폴더 탐색은 IMediaLibraryService, 추가는 주입된 콜백(=MainViewModel.AddMedia).
/// (PowerPoint 폴더 브라우저와 동일 구조 — 동작 동등성 우선.)
/// </summary>
public sealed partial class MediaLibraryViewModel : ObservableObject
{
    private readonly IMediaLibraryService _service;
    private readonly Action<string> _addToQueue;
    private readonly Func<string, bool> _deleteFile;
    private readonly string _rootFolderPath;
    private bool _suppressSelectedFolderReload;
    // 폴더에서 읽은 전체 미디어(검색 필터 적용 전 원본). 검색 상자는 이 목록에서 걸러 MediaFiles 에 보여 준다.
    private readonly List<MediaFileItem> _allFiles = new();

    [ObservableProperty]
    private string _folderPath = string.Empty;

    [ObservableProperty]
    private MediaFolderItem? _selectedFolder;

    // 검색어 — 파일명에 이 글자가 든 미디어만 목록에 보인다(대소문자 무시). 비우면 전부. 큰 폴더에서 빠르게 찾는 현대 기능.
    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private MediaFileItem? _selectedFile;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 하위 폴더 포함 토글 시 즉시 다시 읽는다.
    [ObservableProperty]
    private bool _includeSubfolders;

    public MediaLibraryViewModel(
        IMediaLibraryService service,
        Action<string> addToQueue,
        string initialFolder,
        Func<string, bool>? deleteFile = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _addToQueue = addToQueue ?? throw new ArgumentNullException(nameof(addToQueue));
        _deleteFile = deleteFile ?? RecycleBinFileDeleter.Delete;
        _rootFolderPath = initialFolder ?? string.Empty;
        _folderPath = _rootFolderPath;

        LoadCommand = new RelayCommand(Load);
        AddSelectedCommand = new RelayCommand(AddSelected, () => SelectedFile is not null);
        DeleteSelectedCommand = new RelayCommand(DeleteSelected, () => SelectedFile is not null);

        BuildFolderGroups();
    }

    public ObservableCollection<MediaFolderItem> FolderGroups { get; } = new();

    public ObservableCollection<MediaFileItem> MediaFiles { get; } = new();

    public IRelayCommand LoadCommand { get; }

    public IRelayCommand AddSelectedCommand { get; }

    public IRelayCommand DeleteSelectedCommand { get; }

    partial void OnIncludeSubfoldersChanged(bool value) => Load();

    // 검색어가 바뀌면 다시 읽지 않고(폴더 탐색은 비쌈) 이미 읽어 둔 전체 목록에서 걸러 보여 준다.
    partial void OnFilterTextChanged(string value) => ApplyFilter();

    partial void OnSelectedFolderChanged(MediaFolderItem? value)
    {
        if (value is null)
        {
            return;
        }

        FolderPath = value.FolderPath;
        if (!_suppressSelectedFolderReload)
        {
            Load();
        }
    }

    private void BuildFolderGroups()
    {
        _suppressSelectedFolderReload = true;
        try
        {
            FolderGroups.Clear();
            foreach (var folder in _service.EnumerateFolders(_rootFolderPath))
            {
                FolderGroups.Add(folder);
            }

            SelectedFolder = FolderGroups.FirstOrDefault();
            FolderPath = SelectedFolder?.FolderPath ?? _rootFolderPath;
        }
        finally
        {
            _suppressSelectedFolderReload = false;
        }
    }

    // 현재 폴더의 미디어 파일 목록을 다시 읽는다(전체를 _allFiles 에 담고, 검색어 적용해 화면 목록 구성).
    private void Load()
    {
        _allFiles.Clear();
        foreach (var path in _service.EnumerateMedia(FolderPath, IncludeSubfolders))
        {
            _allFiles.Add(new MediaFileItem(path));
        }

        ApplyFilter();
    }

    // 검색어로 _allFiles 를 걸러 MediaFiles(화면 목록)를 다시 만든다 — 순수 비교는 FileNameFilter 가 맡는다.
    private void ApplyFilter()
    {
        MediaFiles.Clear();
        foreach (var file in _allFiles.Where(f => FileNameFilter.Matches(f.FileName, FilterText)))
        {
            MediaFiles.Add(file);
        }

        // 선택한 파일이 걸러져 목록에서 빠지면 선택을 해제한다 — 숨은(안 보이는) 항목이 선택된 채 "추가"되는 사고를 막는다.
        // ImageLibraryViewModel 와 동일한 가드: 바인딩된 컬렉션을 다시 만들 때 선택 유효성은 VM 이 직접 책임진다(Selector reset 에 기대지 않음).
        if (SelectedFile is not null && !MediaFiles.Contains(SelectedFile))
        {
            SelectedFile = null;
        }

        if (_allFiles.Count == 0)
        {
            StatusText = "미디어 파일이 없습니다(폴더를 확인하세요).";
        }
        else if (MediaFiles.Count == _allFiles.Count)
        {
            StatusText = $"{_allFiles.Count}개 미디어";
        }
        else
        {
            StatusText = $"{MediaFiles.Count}/{_allFiles.Count}개 미디어(검색 '{FilterText.Trim()}')";
        }
    }

    // 선택한 미디어를 예배 순서에 추가(MainViewModel.AddMedia 위임).
    private void AddSelected()
    {
        if (SelectedFile is null)
        {
            return;
        }

        AddFiles(new[] { SelectedFile });
    }

    /// <summary>FrmMain MediaList multi-select add: 선택된 모든 미디어 파일을 현재 순서대로 예배 순서에 추가한다.</summary>
    public int AddFiles(IEnumerable<MediaFileItem> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var selection = files.Where(file => file is not null).ToList();
        if (selection.Count == 0)
        {
            return 0;
        }

        foreach (var file in selection)
        {
            _addToQueue(file.FilePath);
        }

        StatusText = selection.Count == 1
            ? $"예배 순서에 추가: {selection[0].FileName}"
            : $"예배 순서에 추가: {selection.Count}개 미디어";
        return selection.Count;
    }

    private void DeleteSelected()
    {
        if (SelectedFile is not null)
        {
            DeleteFiles([SelectedFile]);
        }
    }

    public int DeleteFiles(IEnumerable<MediaFileItem> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var selection = files
            .Where(file => file is not null)
            .GroupBy(file => file.FilePath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        if (selection.Count == 0)
        {
            return 0;
        }

        var deletedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in selection)
        {
            if (_deleteFile(file.FilePath))
            {
                deletedPaths.Add(file.FilePath);
            }
        }

        if (deletedPaths.Count == 0)
        {
            StatusText = "No media file deleted.";
            return 0;
        }

        _allFiles.RemoveAll(file => deletedPaths.Contains(file.FilePath));
        if (SelectedFile is not null && deletedPaths.Contains(SelectedFile.FilePath))
        {
            SelectedFile = null;
        }

        ApplyFilter();
        StatusText = deletedPaths.Count == 1
            ? $"Deleted media: {Path.GetFileName(deletedPaths.First())}"
            : $"Deleted {deletedPaths.Count} media files.";
        return deletedPaths.Count;
    }
}
