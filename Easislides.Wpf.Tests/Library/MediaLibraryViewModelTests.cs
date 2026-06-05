using System.Collections.Generic;
using System.Linq;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class MediaLibraryViewModelTests
{
    private sealed class FakeMediaService : IMediaLibraryService
    {
        private readonly IReadOnlyList<string> _paths;
        public FakeMediaService(params string[] paths) => _paths = paths;
        public IReadOnlyList<string> EnumerateMedia(string folderPath, bool includeSubfolders) => _paths;
    }

    private static MediaLibraryViewModel CreateSut(out List<string> added, params string[] paths)
    {
        var addedLocal = new List<string>();
        added = addedLocal;
        return new MediaLibraryViewModel(
            new FakeMediaService(paths),
            path => addedLocal.Add(path),
            initialFolder: @"C:\media");
    }

    [Fact]
    public void Load_PopulatesMediaFiles()
    {
        var sut = CreateSut(out _, @"C:\media\a.mp4", @"C:\media\b.mp3");

        sut.LoadCommand.Execute(null);

        sut.MediaFiles.Select(p => p.FileName).Should().Equal("a.mp4", "b.mp3");
        sut.StatusText.Should().Contain("2");
    }

    [Fact]
    public void Load_EmptyFolder_SetsEmptyStatus()
    {
        var sut = CreateSut(out _);

        sut.LoadCommand.Execute(null);

        sut.MediaFiles.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }

    [Fact]
    public void AddSelected_WithSelection_InvokesCallback()
    {
        var sut = CreateSut(out var added, @"C:\media\a.mp4");
        sut.LoadCommand.Execute(null);
        sut.SelectedFile = sut.MediaFiles[0];

        sut.AddSelectedCommand.Execute(null);

        added.Should().ContainSingle().Which.Should().Be(@"C:\media\a.mp4");
    }

    [Fact]
    public void AddFiles_WithSeveralSelections_InvokesCallbackForEachInOrder()
    {
        var sut = CreateSut(out var added, @"C:\media\a.mp4", @"C:\media\b.mp3", @"C:\media\c.mp4");
        sut.LoadCommand.Execute(null);

        var count = sut.AddFiles(new[] { sut.MediaFiles[0], sut.MediaFiles[2] });

        count.Should().Be(2);
        added.Should().Equal(@"C:\media\a.mp4", @"C:\media\c.mp4");
        sut.StatusText.Should().Contain("2개 미디어");
    }

    [Fact]
    public void AddFiles_WithEmptySelection_DoesNotInvokeCallback()
    {
        var sut = CreateSut(out var added, @"C:\media\a.mp4");

        var count = sut.AddFiles(Enumerable.Empty<MediaFileItem>());

        count.Should().Be(0);
        added.Should().BeEmpty();
    }

    [Fact]
    public void AddSelected_WithoutSelection_CannotExecute()
    {
        var sut = CreateSut(out var added, @"C:\media\a.mp4");
        sut.LoadCommand.Execute(null);

        sut.AddSelectedCommand.CanExecute(null).Should().BeFalse();
        added.Should().BeEmpty();
    }

    [Fact]
    public void TogglingIncludeSubfolders_ReloadsList()
    {
        var sut = CreateSut(out _, @"C:\media\a.mp4", @"C:\media\sub\b.mp4");
        sut.MediaFiles.Should().BeEmpty("아직 Load 전");

        sut.IncludeSubfolders = true;

        sut.MediaFiles.Should().HaveCount(2);
    }

    [Fact]
    public void FilterText_NarrowsListByFileName_WithoutReloading()
    {
        var sut = CreateSut(out _, @"C:\media\찬양영상.mp4", @"C:\media\intro.mp4", @"C:\media\특송.mp3");
        sut.LoadCommand.Execute(null);
        sut.MediaFiles.Should().HaveCount(3);

        sut.FilterText = "특송";

        sut.MediaFiles.Select(f => f.FileName).Should().Equal("특송.mp3");
        sut.StatusText.Should().Contain("1/3", "걸러진 개수/전체 개수 표시");
    }

    [Fact]
    public void FilterText_Cleared_RestoresFullList()
    {
        var sut = CreateSut(out _, @"C:\media\a.mp4", @"C:\media\b.mp4");
        sut.LoadCommand.Execute(null);
        sut.FilterText = "a";
        sut.MediaFiles.Should().HaveCount(1);

        sut.FilterText = "";

        sut.MediaFiles.Should().HaveCount(2, "검색어 지우면 전체 복원");
    }

    [Fact]
    public void FilterText_PersistsAcrossReload()
    {
        // 검색어가 있는 상태로 새로고침(Load)하면 새 목록도 검색어로 걸러 보여 준다.
        var sut = CreateSut(out _, @"C:\media\찬양.mp4", @"C:\media\기도.mp4");
        sut.FilterText = "찬양";
        sut.LoadCommand.Execute(null);

        sut.MediaFiles.Select(f => f.FileName).Should().Equal("찬양.mp4");
    }

    [Fact]
    public void FilterText_ExcludingSelected_DeselectsIt_AndDisablesAdd()
    {
        // 선택한 파일이 검색에 걸러져 사라지면 선택을 해제한다 — 숨은 항목이 선택된 채 추가되는 사고 방지(ImageLibrary 와 동일 가드).
        var sut = CreateSut(out var added, @"C:\media\a.mp4", @"C:\media\b.mp4");
        sut.LoadCommand.Execute(null);
        sut.SelectedFile = sut.MediaFiles.First(f => f.FileName == "a.mp4");

        sut.FilterText = "b"; // a 를 걸러 냄

        sut.SelectedFile.Should().BeNull("걸러져 사라진 항목은 선택 해제");
        sut.AddSelectedCommand.CanExecute(null).Should().BeFalse("선택이 풀리면 추가 비활성");
        added.Should().BeEmpty();
    }
}
