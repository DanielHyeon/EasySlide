using System.Collections.Generic;
using System.Linq;
using Easislides.Wpf.Library;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class ImageLibraryViewModelTests
{
    // 폴더와 무관하게 고정 경로를 돌려주는 가짜 서비스(파일 시스템 없이 VM 로직만 검증).
    private sealed class FakeImageLibraryService : IImageLibraryService
    {
        private readonly IReadOnlyList<string> _paths;
        private readonly IReadOnlyList<ImageFolderItem> _folders;

        public FakeImageLibraryService(params string[] paths)
            : this(new[] { new ImageFolderItem("Images", @"C:\bg") }, paths)
        {
        }

        public FakeImageLibraryService(IReadOnlyList<ImageFolderItem> folders, params string[] paths)
        {
            _paths = paths;
            _folders = folders;
        }

        public string? LastEnumeratedFolder { get; private set; }

        public IReadOnlyList<string> EnumerateImages(string folderPath, bool includeSubfolders)
        {
            LastEnumeratedFolder = folderPath;
            return _paths;
        }

        public IReadOnlyList<ImageFolderItem> EnumerateFolders(string rootFolder) => _folders;
    }

    private static ImageLibraryViewModel CreateSut(
        out List<string> applied,
        out List<bool> cleared,
        params string[] paths)
    {
        var appliedLocal = new List<string>();
        var clearedLocal = new List<bool>();
        applied = appliedLocal;
        cleared = clearedLocal;
        return new ImageLibraryViewModel(
            new FakeImageLibraryService(paths),
            _ => null, // 테스트는 썸네일 디코딩 없이 null
            path => appliedLocal.Add(path),
            () => clearedLocal.Add(true),
            initialFolder: @"C:\bg");
    }

    private static void Load(ImageLibraryViewModel sut)
        => sut.LoadCommand.ExecuteAsync(null).GetAwaiter().GetResult();

    [Fact]
    public void Load_PopulatesImagesFromService()
    {
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg", @"C:\bg\b.png");

        Load(sut);

        sut.Images.Should().HaveCount(2);
        sut.Images[0].FileName.Should().Be("a.jpg");
        sut.Images[1].FileName.Should().Be("b.png");
        sut.StatusText.Should().Contain("2");
    }

    [Fact]
    public async System.Threading.Tasks.Task Load_DecodesThumbnailForEveryImage_OffPopulate()
    {
        // 비동기 디코딩: 목록은 즉시(파일명) 채우고, 썸네일 로더는 모든 경로에 대해 호출된다.
        var requested = new System.Collections.Generic.List<string>();
        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(@"C:\bg\a.jpg", @"C:\bg\b.png"),
            path => { requested.Add(path); return null; },
            _ => { },
            () => { },
            initialFolder: @"C:\bg");

        await sut.LoadCommand.ExecuteAsync(null);

        requested.Should().BeEquivalentTo(new[] { @"C:\bg\a.jpg", @"C:\bg\b.png" });
        sut.Images.Should().HaveCount(2);
        sut.StatusText.Should().Be("2개 이미지");
    }

    [Fact]
    public async System.Threading.Tasks.Task Load_WhenReExecuted_CancelsPriorRun_StopsDecodingRemaining()
    {
        // 재진입 가드: 첫 로드가 썸네일 디코딩 중일 때 두 번째 로드(Cancel)가 들어오면
        // 첫 로드는 남은 썸네일을 디코딩하지 않고 조용히 중단한다(상태/목록 경쟁 방지).
        var gate = new System.Threading.ManualResetEventSlim(false);
        var requested = new System.Collections.Generic.List<string>();
        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(@"C:\bg\a.jpg", @"C:\bg\b.jpg", @"C:\bg\c.jpg"),
            path =>
            {
                lock (requested) { requested.Add(path); }
                gate.Wait(2000); // 첫 항목에서 멈춰 두 번째 로드(취소)가 끼어들 틈을 준다
                return null;
            },
            _ => { },
            () => { },
            initialFolder: @"C:\bg");

        var task = sut.LoadCommand.ExecuteAsync(null);
        sut.LoadCommand.Cancel(); // 진행 중 실행 취소(새 로드가 들어온 상황을 모사)
        gate.Set();
        await task;

        lock (requested)
        {
            requested.Count.Should().BeLessThan(3, "취소 후에는 남은 썸네일을 디코딩하지 않는다");
        }
    }

    [Fact]
    public void Load_EmptyFolder_SetsEmptyStatus()
    {
        var sut = CreateSut(out _, out _);

        Load(sut);

        sut.Images.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }

    [Fact]
    public void ApplyAsBackground_WithSelection_InvokesCallbackWithPath()
    {
        var sut = CreateSut(out var applied, out _, @"C:\bg\a.jpg");
        Load(sut);
        sut.SelectedImage = sut.Images[0];

        sut.ApplyAsBackgroundCommand.Execute(null);

        applied.Should().ContainSingle().Which.Should().Be(@"C:\bg\a.jpg");
    }

    [Fact]
    public void ApplyToItemBackground_WithSelectionAndPreviewItem_InvokesItemCallbackWithPath()
    {
        var appliedToItem = new List<string>();
        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(@"C:\bg\a.jpg"),
            _ => null,
            _ => { },
            () => { },
            initialFolder: @"C:\bg",
            applyItemBackground: path => appliedToItem.Add(path),
            canApplyItemBackground: () => true);
        Load(sut);
        sut.SelectedImage = sut.Images[0];

        sut.ApplyToItemBackgroundCommand.CanExecute(null).Should().BeTrue();
        sut.ApplyToItemBackgroundCommand.Execute(null);

        appliedToItem.Should().ContainSingle().Which.Should().Be(@"C:\bg\a.jpg");
        sut.StatusText.Should().Contain("항목 배경");
    }

    [Fact]
    public void ApplySelectedImage_WithPreviewItem_PrefersItemBackground()
    {
        var appliedDefault = new List<string>();
        var appliedToItem = new List<string>();
        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(@"C:\bg\a.jpg"),
            _ => null,
            path => appliedDefault.Add(path),
            () => { },
            initialFolder: @"C:\bg",
            applyItemBackground: path => appliedToItem.Add(path),
            canApplyItemBackground: () => true);
        Load(sut);
        sut.SelectedImage = sut.Images[0];

        sut.ApplySelectedImageCommand.Execute(null);

        appliedToItem.Should().ContainSingle().Which.Should().Be(@"C:\bg\a.jpg");
        appliedDefault.Should().BeEmpty("FrmMain ApplyBackground(..., 2)는 선택 항목이 있으면 Add to Item으로 동작");
        sut.StatusText.Should().Contain("항목 배경");
    }

    [Fact]
    public void ApplySelectedImage_WithoutPreviewItem_FallsBackToDefaultBackground()
    {
        var appliedDefault = new List<string>();
        var appliedToItem = new List<string>();
        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(@"C:\bg\a.jpg"),
            _ => null,
            path => appliedDefault.Add(path),
            () => { },
            initialFolder: @"C:\bg",
            applyItemBackground: path => appliedToItem.Add(path),
            canApplyItemBackground: () => false);
        Load(sut);
        sut.SelectedImage = sut.Images[0];

        sut.ApplySelectedImageCommand.Execute(null);

        appliedDefault.Should().ContainSingle().Which.Should().Be(@"C:\bg\a.jpg");
        appliedToItem.Should().BeEmpty();
        sut.StatusText.Should().Contain("배경 적용");
    }

    [Fact]
    public void ApplyToItemBackground_WithoutPreviewItem_CannotExecute()
    {
        var appliedToItem = new List<string>();
        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(@"C:\bg\a.jpg"),
            _ => null,
            _ => { },
            () => { },
            initialFolder: @"C:\bg",
            applyItemBackground: path => appliedToItem.Add(path),
            canApplyItemBackground: () => false);
        Load(sut);
        sut.SelectedImage = sut.Images[0];

        sut.ApplyToItemBackgroundCommand.CanExecute(null).Should().BeFalse("FrmMain disables Add to Item when no preview item is selected");
        appliedToItem.Should().BeEmpty();
    }

    [Fact]
    public void ApplyAsBackground_WithoutSelection_CannotExecute()
    {
        var sut = CreateSut(out var applied, out _, @"C:\bg\a.jpg");
        Load(sut);

        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeFalse("선택이 없으면 적용 불가");
        sut.ApplySelectedImageCommand.CanExecute(null).Should().BeFalse("선택이 없으면 FrmMain식 이미지 적용도 불가");
        applied.Should().BeEmpty();
    }

    [Fact]
    public void SelectingImage_EnablesApplyCommand()
    {
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg");
        Load(sut);

        sut.SelectedImage = sut.Images[0];

        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeTrue();
        sut.ApplySelectedImageCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void TogglingIncludeSubfolders_ReloadsImages()
    {
        // 하위 폴더 포함 토글이 즉시 재로드를 트리거한다(빈 목록→토글 후 서비스 결과 반영).
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg", @"C:\bg\sub\b.png");
        sut.Images.Should().BeEmpty("아직 Load 전");

        sut.IncludeSubfolders = true;

        sut.Images.Should().HaveCount(2, "토글이 Load 를 트리거");
    }

    [Fact]
    public void Constructor_BuildsFrmMainImagesFolderGroups_WithoutLoadingFiles()
    {
        var folders = new[]
        {
            new ImageFolderItem("Scenery", @"C:\bg\Scenery"),
            new ImageFolderItem("Tiles", @"C:\bg\Tiles"),
            new ImageFolderItem("Images", @"C:\bg"),
            new ImageFolderItem(@"\Custom", @"C:\bg\Custom"),
        };

        var sut = new ImageLibraryViewModel(
            new FakeImageLibraryService(folders, @"C:\bg\Custom\a.jpg"),
            _ => null,
            _ => { },
            () => { },
            initialFolder: @"C:\bg");

        sut.FolderGroups.Select(f => f.DisplayName).Should().Equal("Scenery", "Tiles", "Images", @"\Custom");
        sut.SelectedFolder.Should().Be(folders[0]);
        sut.FolderPath.Should().Be(@"C:\bg\Scenery");
        sut.IncludeSubfolders.Should().BeFalse("FrmMain ImagesFolder shows the selected folder contents only");
        sut.Images.Should().BeEmpty("constructor should not load thumbnails before the Images tab is opened");
    }

    [Fact]
    public void SelectingImagesFolder_UpdatesFolderPath_AndReloadsThumbnails()
    {
        var folders = new[]
        {
            new ImageFolderItem("Scenery", @"C:\bg\Scenery"),
            new ImageFolderItem("Tiles", @"C:\bg\Tiles"),
        };
        var service = new FakeImageLibraryService(folders, @"C:\bg\Tiles\tile.jpg");
        var sut = new ImageLibraryViewModel(
            service,
            _ => null,
            _ => { },
            () => { },
            initialFolder: @"C:\bg");

        sut.SelectedFolder = folders[1];

        sut.FolderPath.Should().Be(@"C:\bg\Tiles");
        service.LastEnumeratedFolder.Should().Be(@"C:\bg\Tiles");
        sut.Images.Select(i => i.FileName).Should().Equal("tile.jpg");
    }

    [Theory]
    [InlineData(@"C:\bg\Scenery\sky.jpg", @"C:\bg", "Scenery")]
    [InlineData(@"C:\bg\Tiles\brick.png", @"C:\bg", "Tiles")]
    [InlineData(@"C:\bg\logo.png", @"C:\bg", "(기본)")]               // 루트 직속
    [InlineData(@"C:\bg\Scenery\Sub\deep.jpg", @"C:\bg", "Scenery")]  // 중첩은 최상위 카테고리로
    public void CategoryOf_DerivesImmediateSubfolderCategory(string path, string root, string expected)
    {
        ImageLibraryViewModel.CategoryOf(path, root).Should().Be(expected);
    }

    [Fact]
    public void Load_BuildsCategoryListFromSubfolders_WithAllFirst()
    {
        // FrmMain Scenery/Tiles — 하위 폴더가 카테고리로 잡히고 "전체"가 맨 앞에 온다.
        var sut = CreateSut(out _, out _,
            @"C:\bg\Scenery\a.jpg", @"C:\bg\Tiles\b.png", @"C:\bg\logo.png");

        Load(sut);

        sut.Categories.Should().StartWith(ImageLibraryViewModel.AllCategories);
        sut.Categories.Should().Contain(new[] { "Scenery", "Tiles", "(기본)" });
        sut.Images.Should().HaveCount(3, "기본(전체) 선택이면 모두 표시");
    }

    [Fact]
    public void SelectingCategory_FiltersImagesToThatSubfolder()
    {
        var sut = CreateSut(out _, out _,
            @"C:\bg\Scenery\a.jpg", @"C:\bg\Scenery\b.jpg", @"C:\bg\Tiles\c.png");
        Load(sut);
        sut.Images.Should().HaveCount(3, "전체");

        sut.SelectedCategory = "Tiles";

        sut.Images.Should().ContainSingle().Which.FileName.Should().Be("c.png");

        sut.SelectedCategory = ImageLibraryViewModel.AllCategories;
        sut.Images.Should().HaveCount(3, "전체로 되돌리면 모두");
    }

    [Fact]
    public void SelectingCategory_ClearsSelectionWhenSelectedImageFilteredOut()
    {
        // code-review MAJOR 반영 — 선택한 이미지가 필터에서 빠지면 선택 해제(숨은 이미지에 배경 적용되는 것 방지).
        var sut = CreateSut(out _, out _, @"C:\bg\Tiles\t.png", @"C:\bg\Scenery\s.jpg");
        Load(sut);
        sut.SelectedImage = sut.Images.Single(i => i.FileName == "t.png"); // Tiles 선택
        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeTrue();

        sut.SelectedCategory = "Scenery"; // Tiles 선택이 필터에서 빠짐

        sut.SelectedImage.Should().BeNull("필터에서 빠진 선택은 해제");
        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeFalse("선택 해제 → 적용 불가");
    }

    [Fact]
    public void Reload_ResetsCategoryToAll()
    {
        var sut = CreateSut(out _, out _, @"C:\bg\Scenery\a.jpg", @"C:\bg\Tiles\b.png");
        Load(sut);
        sut.SelectedCategory = "Tiles";

        Load(sut); // 새로고침

        sut.SelectedCategory.Should().Be(ImageLibraryViewModel.AllCategories, "재로드 시 카테고리 초기화");
        sut.Images.Should().HaveCount(2);
    }

    [Fact]
    public void ClearBackground_InvokesClearCallback()
    {
        var sut = CreateSut(out _, out var cleared);

        sut.ClearBackgroundCommand.Execute(null);

        cleared.Should().ContainSingle();
        sut.StatusText.Should().Contain("해제");
    }

    [Fact]
    public void FilterText_NarrowsImagesByFileName()
    {
        var sut = CreateSut(out _, out _, @"C:\bg\sunset.jpg", @"C:\bg\mountain.jpg", @"C:\bg\sea.png");
        Load(sut);
        sut.Images.Should().HaveCount(3);

        sut.FilterText = "moun";

        sut.Images.Select(i => i.FileName).Should().Equal("mountain.jpg");
    }

    [Fact]
    public void FilterText_ComposesWithCategory_AsAnd()
    {
        // 카테고리(하위 폴더) + 파일명 검색은 둘 다 만족해야 보인다(AND).
        var sut = CreateSut(out _, out _, @"C:\bg\Scenery\sky.jpg", @"C:\bg\Scenery\sea.jpg", @"C:\bg\Tiles\sky.png");
        Load(sut);

        sut.SelectedCategory = "Scenery";
        sut.FilterText = "sky";

        // Scenery 의 sky.jpg 만(Tiles 의 sky.png 는 카테고리에서 빠지고, Scenery 의 sea.jpg 는 검색에서 빠짐).
        sut.Images.Select(i => i.FileName).Should().Equal("sky.jpg");
    }

    [Fact]
    public void FilterText_ExcludingSelected_DeselectsIt()
    {
        // 선택한 이미지가 검색에 걸러지면 선택 해제(숨은 이미지에 배경 적용 방지) — 카테고리 필터와 동일 가드.
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg", @"C:\bg\b.jpg");
        Load(sut);
        sut.SelectedImage = sut.Images.Single(i => i.FileName == "a.jpg");

        sut.FilterText = "b";

        sut.SelectedImage.Should().BeNull("걸러진 선택은 해제");
        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void FilterText_PersistsAcrossReload()
    {
        // 검색어는 새로고침(재로드) 후에도 유지된다(카테고리는 폴더가 바뀌니 초기화하지만 검색어는 의도적으로 보존 — 미디어·PPT 와 동일).
        var sut = CreateSut(out _, out _, @"C:\bg\찬양.jpg", @"C:\bg\기도.jpg");
        sut.FilterText = "찬양";

        Load(sut);

        sut.FilterText.Should().Be("찬양", "재로드해도 검색어 유지");
        sut.Images.Select(i => i.FileName).Should().Equal(new[] { "찬양.jpg" }, "재로드 목록도 검색어로 걸러 보임");
    }

    [Fact]
    public async System.Threading.Tasks.Task FilterText_ShowsFilteredCountInStatus()
    {
        // 증분134 — 검색·카테고리로 걸러진 개수를 상태줄에 "{보임}/{전체}개" 로 표시(미디어·PPT 와 패리티). 비동기 디코딩과 경쟁 없음.
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg", @"C:\bg\b.jpg", @"C:\bg\c.jpg");
        await sut.LoadCommand.ExecuteAsync(null); // 디코딩까지 완료(null 로더) → "불러오는 중" 꼬리 제거됨
        sut.StatusText.Should().Be("3개 이미지", "필터 없으면 전체 개수");

        sut.FilterText = "a";

        sut.StatusText.Should().Be("1/3개 이미지(검색·필터)", "걸러진 개수/전체 개수 표시");
    }

    [Fact]
    public void FilterRoundTrip_KeepsSameItemInstance()
    {
        // 필터로 숨겼다가 다시 보이게 해도 같은 ImageLibraryItem 인스턴스가 유지된다 —
        // 이 보장 덕분에 백그라운드로 디코딩된 썸네일(인스턴스에 저장)이 필터 왕복 후에도 그대로 살아 있다(두 목록 패턴의 핵심).
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg", @"C:\bg\b.jpg");
        Load(sut);
        var aBefore = sut.Images.Single(i => i.FileName == "a.jpg");

        sut.FilterText = "b"; // a 숨김
        sut.FilterText = "";  // 다시 보임

        sut.Images.Single(i => i.FileName == "a.jpg").Should().BeSameAs(aBefore, "필터 왕복해도 같은 인스턴스(썸네일 등 상태 유지)");
    }
}
