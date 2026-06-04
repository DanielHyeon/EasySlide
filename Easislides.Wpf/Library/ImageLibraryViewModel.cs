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

    public ImageLibraryItem(string filePath, string category)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        Category = category;
    }

    public string FilePath { get; }

    public string FileName { get; }

    /// <summary>이미지가 속한 카테고리(루트 바로 아래 하위 폴더명, 예: "Scenery"/"Tiles"). 루트 직속이면 "(기본)".</summary>
    public string Category { get; }
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
    private readonly Action<string> _applyItemBackground;
    private readonly Func<bool> _canApplyItemBackground;
    private readonly Action _clearBackground;

    [ObservableProperty]
    private string _folderPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyAsBackgroundCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplyToItemBackgroundCommand))]
    private ImageLibraryItem? _selectedImage;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 하위 폴더 포함 여부 — 토글하면 즉시 다시 읽는다(FrmMain Images 의 Scenery/Tiles 하위 폴더 탐색 대응).
    [ObservableProperty]
    private bool _includeSubfolders;

    // "전체" 카테고리 — 모든 이미지를 보여 준다(필터 없음). 루트 직속 이미지의 카테고리 라벨.
    public const string AllCategories = "전체";
    public const string RootCategory = "(기본)";

    // 로드된 전체 이미지(필터 전 원본). Images 는 SelectedCategory 로 거른 부분집합이다.
    private readonly System.Collections.Generic.List<ImageLibraryItem> _allItems = new();

    // 썸네일 백그라운드 디코딩이 진행 중인지 — 상태줄에 "미리보기 불러오는 중" 꼬리를 붙일지 판단.
    // 상태줄은 동기 필터(ApplyCategoryFilter)와 비동기 로드(LoadAsync) 두 곳이 함께 쓰므로, 한 함수(RefreshStatus)에 모아
    // 비동기 루프가 필터가 보여 준 개수를 덮어쓰는 경쟁을 없앤다(증분133 리뷰 후속 — 동기 미디어·PPT 와 달리 async 라 이 조율이 필요).
    private bool _thumbnailsLoading;

    // 현재 선택된 카테고리(하위 폴더). "전체"면 모두 표시. 바꾸면 즉시 다시 거른다.
    [ObservableProperty]
    private string _selectedCategory = AllCategories;

    // 검색어 — 파일명에 이 글자가 든 이미지만 보인다(대소문자 무시). 비우면 카테고리 필터만. 큰 폴더에서 빠르게 찾는 현대 기능(미디어·PPT 브라우저와 동일).
    [ObservableProperty]
    private string _filterText = string.Empty;

    public ImageLibraryViewModel(
        IImageLibraryService service,
        Func<string, ImageSource?> thumbnailLoader,
        Action<string> applyBackground,
        Action clearBackground,
        string initialFolder,
        Action<string>? applyItemBackground = null,
        Func<bool>? canApplyItemBackground = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));
        _applyBackground = applyBackground ?? throw new ArgumentNullException(nameof(applyBackground));
        _applyItemBackground = applyItemBackground ?? (_ => { });
        _canApplyItemBackground = canApplyItemBackground ?? (() => false);
        _clearBackground = clearBackground ?? throw new ArgumentNullException(nameof(clearBackground));
        _folderPath = initialFolder ?? string.Empty;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        // AsyncRelayCommand 가 재실행 시 이전 실행의 토큰을 취소한다 → LoadAsync 가 중복 실행돼도 경쟁하지 않는다.
        ApplyAsBackgroundCommand = new RelayCommand(ApplyAsBackground, () => SelectedImage is not null);
        ApplyToItemBackgroundCommand = new RelayCommand(ApplyToItemBackground, () => SelectedImage is not null && _canApplyItemBackground());
        ClearBackgroundCommand = new RelayCommand(ClearBackground);
    }

    public ObservableCollection<ImageLibraryItem> Images { get; } = new();

    /// <summary>카테고리(하위 폴더) 목록 — "전체" + 발견된 하위 폴더들. 카테고리 콤보에 바인딩(FrmMain Scenery/Tiles).</summary>
    public ObservableCollection<string> Categories { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }

    public IRelayCommand ApplyAsBackgroundCommand { get; }

    public IRelayCommand ApplyToItemBackgroundCommand { get; }

    public IRelayCommand ClearBackgroundCommand { get; }

    // 하위 폴더 포함 토글 시 즉시 다시 읽는다(CommunityToolkit 가 생성하는 변경 콜백).
    partial void OnIncludeSubfoldersChanged(bool value) => LoadCommand.Execute(null);

    // 카테고리 선택이 바뀌면 이미 읽은 전체에서 즉시 다시 거른다(재로딩 없이 빠르게).
    partial void OnSelectedCategoryChanged(string value) => ApplyCategoryFilter();

    // 검색어가 바뀌면 다시 읽지 않고(폴더 탐색·썸네일 디코딩은 비쌈) 이미 읽어 둔 전체에서 즉시 다시 거른다.
    partial void OnFilterTextChanged(string value) => ApplyCategoryFilter();

    // 파일 경로의 카테고리 = 루트(root) 바로 아래 하위 폴더명. 루트 직속이면 "(기본)".
    // 예: root=Images, 경로=Images/Scenery/sky.jpg → "Scenery". 경로=Images/logo.png → "(기본)".
    internal static string CategoryOf(string filePath, string root)
    {
        if (string.IsNullOrEmpty(root))
        {
            return RootCategory;
        }

        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
        {
            return RootCategory;
        }

        string rel;
        try
        {
            rel = Path.GetRelativePath(root, dir);
        }
        catch (ArgumentException)
        {
            return RootCategory; // 다른 드라이브 등 상대경로 불가 → 기본 카테고리.
        }

        // 루트 직속(".")이거나 루트 밖(".." 또는 "../foo")이면 기본 카테고리. ".."로 시작하는 정상 폴더명(예 "..bar")은
        // GetRelativePath 가 escape 로 내보내지 않으므로, 정확히 ".." 또는 ".." + 구분자 로 시작할 때만 escape 로 본다.
        if (rel == "."
            || rel == ".."
            || rel.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || rel.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
        {
            return RootCategory;
        }

        // 첫 세그먼트만(중첩 하위 폴더는 최상위 카테고리로 묶는다).
        return rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
    }

    // _allItems 를 SelectedCategory(카테고리) + FilterText(파일명 검색)로 걸러 Images 에 채운다. 썸네일은 같은 인스턴스라 유지된다.
    private void ApplyCategoryFilter()
    {
        Images.Clear();
        foreach (var item in _allItems)
        {
            var categoryMatch = SelectedCategory == AllCategories || item.Category == SelectedCategory;
            // 카테고리와 파일명 검색을 모두 만족해야 보인다(둘은 AND). 검색어가 비면 FileNameFilter 가 항상 true → 카테고리만.
            if (categoryMatch && FileNameFilter.Matches(item.FileName, FilterText))
            {
                Images.Add(item);
            }
        }

        // 선택한 이미지가 현재 필터(또는 재로드된 목록)에 더는 없으면 선택을 해제한다.
        // 안 그러면 화면에 안 보이는 항목이 선택된 채로 남아 "배경 적용"이 엉뚱한(숨은) 이미지에 동작한다(code-review MAJOR).
        if (SelectedImage is not null && !Images.Contains(SelectedImage))
        {
            SelectedImage = null;
        }

        RefreshStatus(); // 거른 뒤 상태줄을 "{보임}/{전체}개" 로 갱신(검색·카테고리 결과 개수 표시).
    }

    // 상태줄을 현재 목록 상태로 갱신한다 — 비었으면 안내, 다 보이면 전체 개수, 걸러졌으면 "{보임}/{전체}개",
    // 디코딩 중이면 "미리보기 불러오는 중" 꼬리. 동기 필터·비동기 로드가 같은 함수를 써서 개수가 서로 덮어쓰지 않게 한다.
    private void RefreshStatus()
    {
        if (_allItems.Count == 0)
        {
            StatusText = "이미지가 없습니다(폴더를 확인하세요).";
            return;
        }

        var countPart = Images.Count == _allItems.Count
            ? $"{_allItems.Count}개 이미지"
            : $"{Images.Count}/{_allItems.Count}개 이미지(검색·필터)";

        StatusText = _thumbnailsLoading ? $"{countPart} — 미리보기 불러오는 중..." : countPart;
    }

    // 현재 폴더의 이미지를 다시 읽는다. 목록(파일명)은 즉시 채우고, 썸네일은 백그라운드에서
    // 디코딩해 하나씩 채워 넣는다 → 대용량 폴더에서도 UI 가 즉시 반응한다.
    // ct: 새 로드(폴더 변경·새로고침·하위포함 토글)가 시작되면 취소돼 이전 실행이 상태를 덮어쓰지 않게 한다.
    private async Task LoadAsync(CancellationToken ct)
    {
        // 디코딩 시작 — 상태줄에 "불러오는 중" 꼬리가 붙는다(완료 시 해제).
        // 첫 await 이전(동기 구간)에 세팅하는 게 중요하다: 새 로드가 옛 로드를 취소하고 시작할 때, 옛 로드의 멈췄던
        // 코드가 다시 깨어나기 전에 새 로드가 동기로 이 플래그를 true 로 다시 세워, 플래그가 잘못된 중간 상태로 안 보인다.
        _thumbnailsLoading = true;
        Images.Clear();
        _allItems.Clear();
        var paths = _service.EnumerateImages(FolderPath, IncludeSubfolders);
        var items = paths.Select(path => new ImageLibraryItem(path, CategoryOf(path, FolderPath))).ToList();
        _allItems.AddRange(items);

        // 카테고리 콤보 갱신 — "전체" + 발견된 하위 폴더(정렬). 폴더가 바뀌었으니 선택은 "전체"로 초기화.
        Categories.Clear();
        Categories.Add(AllCategories);
        foreach (var category in items.Select(i => i.Category).Distinct().OrderBy(c => c, StringComparer.CurrentCulture))
        {
            Categories.Add(category);
        }

        // SelectedCategory 를 "전체"로 되돌린다(OnSelectedCategoryChanged → ApplyCategoryFilter 가 Images 를 채운다).
        // 이미 "전체"면 콜백이 안 울리므로 직접 한 번 거른다.
        // ApplyCategoryFilter(직접 또는 SelectedCategory 변경 콜백)가 RefreshStatus 를 호출해
        // "{보임}/{전체}개 — 불러오는 중..." 상태줄을 세운다(아래 별도 StatusText 설정 불필요).
        if (SelectedCategory == AllCategories)
        {
            ApplyCategoryFilter();
        }
        else
        {
            SelectedCategory = AllCategories;
        }

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

        // 디코딩 완료 — "불러오는 중" 꼬리를 떼고 현재 필터를 반영한 개수로 상태줄을 마무리한다(필터가 보여 준 개수를 덮어쓰지 않음).
        _thumbnailsLoading = false;
        RefreshStatus();
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

    // 선택한 이미지를 현재 선택된 예배 항목 배경으로 적용(FrmMain CMenuImages_AddItem).
    private void ApplyToItemBackground()
    {
        if (SelectedImage is null || !_canApplyItemBackground())
        {
            return;
        }

        _applyItemBackground(SelectedImage.FilePath);
        StatusText = $"항목 배경 적용: {SelectedImage.FileName}";
    }

    // 출력 배경 이미지 해제(색 배경으로 복귀).
    private void ClearBackground()
    {
        _clearBackground();
        StatusText = "배경 해제";
    }
}
