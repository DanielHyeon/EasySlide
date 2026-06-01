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
}
