using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    [ObservableProperty]
    private string _folderPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSelectedCommand))]
    private MediaFileItem? _selectedFile;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 하위 폴더 포함 토글 시 즉시 다시 읽는다.
    [ObservableProperty]
    private bool _includeSubfolders;

    public MediaLibraryViewModel(
        IMediaLibraryService service,
        Action<string> addToQueue,
        string initialFolder)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _addToQueue = addToQueue ?? throw new ArgumentNullException(nameof(addToQueue));
        _folderPath = initialFolder ?? string.Empty;

        LoadCommand = new RelayCommand(Load);
        AddSelectedCommand = new RelayCommand(AddSelected, () => SelectedFile is not null);
    }

    public ObservableCollection<MediaFileItem> MediaFiles { get; } = new();

    public IRelayCommand LoadCommand { get; }

    public IRelayCommand AddSelectedCommand { get; }

    partial void OnIncludeSubfoldersChanged(bool value) => Load();

    // 현재 폴더의 미디어 파일 목록을 다시 읽는다.
    private void Load()
    {
        MediaFiles.Clear();
        foreach (var path in _service.EnumerateMedia(FolderPath, IncludeSubfolders))
        {
            MediaFiles.Add(new MediaFileItem(path));
        }

        StatusText = MediaFiles.Count == 0
            ? "미디어 파일이 없습니다(폴더를 확인하세요)."
            : $"{MediaFiles.Count}개 미디어";
    }

    // 선택한 미디어를 예배 순서에 추가(MainViewModel.AddMedia 위임).
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
