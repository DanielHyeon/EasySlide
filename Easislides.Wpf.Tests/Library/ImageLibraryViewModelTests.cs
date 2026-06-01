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

        public FakeImageLibraryService(params string[] paths) => _paths = paths;

        public IReadOnlyList<string> EnumerateImages(string folderPath, bool includeSubfolders) => _paths;
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

    [Fact]
    public void Load_PopulatesImagesFromService()
    {
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg", @"C:\bg\b.png");

        sut.LoadCommand.Execute(null);

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

        sut.LoadCommand.Execute(null);

        sut.Images.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }

    [Fact]
    public void ApplyAsBackground_WithSelection_InvokesCallbackWithPath()
    {
        var sut = CreateSut(out var applied, out _, @"C:\bg\a.jpg");
        sut.LoadCommand.Execute(null);
        sut.SelectedImage = sut.Images[0];

        sut.ApplyAsBackgroundCommand.Execute(null);

        applied.Should().ContainSingle().Which.Should().Be(@"C:\bg\a.jpg");
    }

    [Fact]
    public void ApplyAsBackground_WithoutSelection_CannotExecute()
    {
        var sut = CreateSut(out var applied, out _, @"C:\bg\a.jpg");
        sut.LoadCommand.Execute(null);

        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeFalse("선택이 없으면 적용 불가");
        applied.Should().BeEmpty();
    }

    [Fact]
    public void SelectingImage_EnablesApplyCommand()
    {
        var sut = CreateSut(out _, out _, @"C:\bg\a.jpg");
        sut.LoadCommand.Execute(null);

        sut.SelectedImage = sut.Images[0];

        sut.ApplyAsBackgroundCommand.CanExecute(null).Should().BeTrue();
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

        sut.LoadCommand.Execute(null);

        sut.Categories.Should().StartWith(ImageLibraryViewModel.AllCategories);
        sut.Categories.Should().Contain(new[] { "Scenery", "Tiles", "(기본)" });
        sut.Images.Should().HaveCount(3, "기본(전체) 선택이면 모두 표시");
    }

    [Fact]
    public void SelectingCategory_FiltersImagesToThatSubfolder()
    {
        var sut = CreateSut(out _, out _,
            @"C:\bg\Scenery\a.jpg", @"C:\bg\Scenery\b.jpg", @"C:\bg\Tiles\c.png");
        sut.LoadCommand.Execute(null);
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
        sut.LoadCommand.Execute(null);
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
        sut.LoadCommand.Execute(null);
        sut.SelectedCategory = "Tiles";

        sut.LoadCommand.Execute(null); // 새로고침

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
}
