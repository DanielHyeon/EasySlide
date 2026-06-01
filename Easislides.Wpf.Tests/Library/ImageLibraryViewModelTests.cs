using System.Collections.Generic;
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

    [Fact]
    public void ClearBackground_InvokesClearCallback()
    {
        var sut = CreateSut(out _, out var cleared);

        sut.ClearBackgroundCommand.Execute(null);

        cleared.Should().ContainSingle();
        sut.StatusText.Should().Contain("해제");
    }
}
