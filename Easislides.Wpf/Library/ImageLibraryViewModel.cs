using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Library;

/// <summary>
/// 이미지 갤러리 항목 — 파일 경로·이름과, 지연 로드한 썸네일을 담는다.
/// 썸네일은 주입된 로더로 만들며(테스트는 null 로더), 로드 실패 시 null → 뷰가 파일명만 보여 준다.
/// </summary>
public sealed class ImageLibraryItem
{
    public ImageLibraryItem(string filePath, ImageSource? thumbnail)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        Thumbnail = thumbnail;
    }

    public string FilePath { get; }

    public string FileName { get; }

    public ImageSource? Thumbnail { get; }
}

/// <summary>
/// 이미지 갤러리(FrmMain Images 탭 포팅) — 폴더의 이미지를 썸네일로 나열하고,
/// 선택한 이미지를 출력 전역 배경으로 적용하거나 배경을 해제한다.
/// 폴더 탐색은 IImageLibraryService, 배경 적용/해제는 주입된 콜백(=MainViewModel)에 위임한다.
/// </summary>
public sealed partial class ImageLibraryViewModel : ObservableObject
{
    private readonly IImageLibraryService _service;
    private readonly Func<string, ImageSource?> _thumbnailLoader;
    private readonly Action<string> _applyBackground;
    private readonly Action _clearBackground;

    [ObservableProperty]
    private string _folderPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyAsBackgroundCommand))]
    private ImageLibraryItem? _selectedImage;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 하위 폴더 포함 여부 — 토글하면 즉시 다시 읽는다(FrmMain Images 의 Scenery/Tiles 하위 폴더 탐색 대응).
    [ObservableProperty]
    private bool _includeSubfolders;

    public ImageLibraryViewModel(
        IImageLibraryService service,
        Func<string, ImageSource?> thumbnailLoader,
        Action<string> applyBackground,
        Action clearBackground,
        string initialFolder)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));
        _applyBackground = applyBackground ?? throw new ArgumentNullException(nameof(applyBackground));
        _clearBackground = clearBackground ?? throw new ArgumentNullException(nameof(clearBackground));
        _folderPath = initialFolder ?? string.Empty;

        LoadCommand = new RelayCommand(Load);
        ApplyAsBackgroundCommand = new RelayCommand(ApplyAsBackground, () => SelectedImage is not null);
        ClearBackgroundCommand = new RelayCommand(ClearBackground);
    }

    public ObservableCollection<ImageLibraryItem> Images { get; } = new();

    public IRelayCommand LoadCommand { get; }

    public IRelayCommand ApplyAsBackgroundCommand { get; }

    public IRelayCommand ClearBackgroundCommand { get; }

    // 하위 폴더 포함 토글 시 즉시 다시 읽는다(CommunityToolkit 가 생성하는 변경 콜백).
    partial void OnIncludeSubfoldersChanged(bool value) => Load();

    // 현재 폴더의 이미지를 다시 읽어 썸네일 목록을 채운다(폴더 변경·새로고침·하위포함 토글 시 호출).
    private void Load()
    {
        Images.Clear();
        var paths = _service.EnumerateImages(FolderPath, IncludeSubfolders);
        foreach (var path in paths)
        {
            Images.Add(new ImageLibraryItem(path, _thumbnailLoader(path)));
        }

        StatusText = Images.Count == 0
            ? "이미지가 없습니다(폴더를 확인하세요)."
            : $"{Images.Count}개 이미지";
    }

    // 선택한 이미지를 출력 전역 배경으로 적용(MainViewModel.SetOutputBackgroundImage 위임).
    private void ApplyAsBackground()
    {
        if (SelectedImage is null)
        {
            return;
        }

        _applyBackground(SelectedImage.FilePath);
        StatusText = $"배경 적용: {SelectedImage.FileName}";
    }

    // 출력 배경 이미지 해제(색 배경으로 복귀).
    private void ClearBackground()
    {
        _clearBackground();
        StatusText = "배경 해제";
    }
}
