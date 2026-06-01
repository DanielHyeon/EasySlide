using System;
using System.Collections.ObjectModel;
using System.IO;
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

    [ObservableProperty]
    private string _folderPath = string.Empty;

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

    // 현재 폴더의 PPT 파일 목록을 다시 읽는다.
    private void Load()
    {
        Presentations.Clear();
        foreach (var path in _service.EnumeratePresentations(FolderPath, IncludeSubfolders))
        {
            Presentations.Add(new PowerPointFileItem(path));
        }

        StatusText = Presentations.Count == 0
            ? "PowerPoint 파일이 없습니다(폴더를 확인하세요)."
            : $"{Presentations.Count}개 PowerPoint";
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
