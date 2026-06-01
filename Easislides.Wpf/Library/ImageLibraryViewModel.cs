using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Library;

/// <summary>
/// 이미지 갤러리 항목 — 파일 경로·이름과, 비동기로 채워지는 썸네일을 담는다.
/// 목록은 파일명으로 즉시 채우고, 썸네일은 백그라운드 디코딩 후 채워 넣어(Thumbnail 변경 알림)
/// 대용량 폴더에서도 UI 가 멈추지 않는다. 디코딩 실패 시 null → 뷰가 파일명만 보여 준다.
/// </summary>
public sealed partial class ImageLibraryItem : ObservableObject
{
    [ObservableProperty]
    private ImageSource? _thumbnail;

    public ImageLibraryItem(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
    }

    public string FilePath { get; }

    public string FileName { get; }
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

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        // AsyncRelayCommand 가 재실행 시 이전 실행의 토큰을 취소한다 → LoadAsync 가 중복 실행돼도 경쟁하지 않는다.
        ApplyAsBackgroundCommand = new RelayCommand(ApplyAsBackground, () => SelectedImage is not null);
        ClearBackgroundCommand = new RelayCommand(ClearBackground);
    }

    public ObservableCollection<ImageLibraryItem> Images { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }

    public IRelayCommand ApplyAsBackgroundCommand { get; }

    public IRelayCommand ClearBackgroundCommand { get; }

    // 하위 폴더 포함 토글 시 즉시 다시 읽는다(CommunityToolkit 가 생성하는 변경 콜백).
    partial void OnIncludeSubfoldersChanged(bool value) => LoadCommand.Execute(null);

    // 현재 폴더의 이미지를 다시 읽는다. 목록(파일명)은 즉시 채우고, 썸네일은 백그라운드에서
    // 디코딩해 하나씩 채워 넣는다 → 대용량 폴더에서도 UI 가 즉시 반응한다.
    // ct: 새 로드(폴더 변경·새로고침·하위포함 토글)가 시작되면 취소돼 이전 실행이 상태를 덮어쓰지 않게 한다.
    private async Task LoadAsync(CancellationToken ct)
    {
        Images.Clear();
        var paths = _service.EnumerateImages(FolderPath, IncludeSubfolders);
        var items = paths.Select(path => new ImageLibraryItem(path)).ToList();
        foreach (var item in items)
        {
            Images.Add(item);
        }

        StatusText = items.Count == 0
            ? "이미지가 없습니다(폴더를 확인하세요)."
            : $"{items.Count}개 이미지 — 미리보기 불러오는 중...";

        // 썸네일 디코딩은 무거우므로 백그라운드 스레드에서. 로더는 Freeze 된 이미지를 반환해
        // 다른 스레드에서 만들어도 UI 바인딩이 안전하다(await 재개는 UI 컨텍스트).
        // 취소되면(새 로드 시작) 조용히 중단해 옛 실행이 새 목록/상태를 건드리지 않는다.
        foreach (var item in items)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            var thumbnail = await Task.Run(() => _thumbnailLoader(item.FilePath)).ConfigureAwait(true);
            if (ct.IsCancellationRequested)
            {
                return;
            }

            item.Thumbnail = thumbnail;
        }

        if (items.Count > 0)
        {
            StatusText = $"{items.Count}개 이미지";
        }
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
