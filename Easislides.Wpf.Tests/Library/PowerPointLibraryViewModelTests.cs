using System.Collections.Generic;
using System.Linq;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PowerPointLibraryViewModelTests
{
    private sealed class FakePptService : IPowerPointLibraryService
    {
        private readonly IReadOnlyList<string> _paths;
        public FakePptService(params string[] paths) => _paths = paths;
        public IReadOnlyList<string> EnumeratePresentations(string folderPath, bool includeSubfolders) => _paths;
    }

    private static PowerPointLibraryViewModel CreateSut(out List<string> added, params string[] paths)
    {
        var addedLocal = new List<string>();
        added = addedLocal;
        return new PowerPointLibraryViewModel(
            new FakePptService(paths),
            path => addedLocal.Add(path),
            initialFolder: @"C:\decks");
    }

    [Fact]
    public void Load_PopulatesPresentations()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\b.ppt");

        sut.LoadCommand.Execute(null);

        sut.Presentations.Select(p => p.FileName).Should().Equal("a.pptx", "b.ppt");
        sut.StatusText.Should().Contain("2");
    }

    [Fact]
    public void Load_EmptyFolder_SetsEmptyStatus()
    {
        var sut = CreateSut(out _);

        sut.LoadCommand.Execute(null);

        sut.Presentations.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }

    [Fact]
    public void AddSelected_WithSelection_InvokesCallback()
    {
        var sut = CreateSut(out var added, @"C:\decks\a.pptx");
        sut.LoadCommand.Execute(null);
        sut.SelectedFile = sut.Presentations[0];

        sut.AddSelectedCommand.Execute(null);

        added.Should().ContainSingle().Which.Should().Be(@"C:\decks\a.pptx");
    }

    [Fact]
    public void AddSelected_WithoutSelection_CannotExecute()
    {
        var sut = CreateSut(out var added, @"C:\decks\a.pptx");
        sut.LoadCommand.Execute(null);

        sut.AddSelectedCommand.CanExecute(null).Should().BeFalse();
        added.Should().BeEmpty();
    }

    [Fact]
    public void TogglingIncludeSubfolders_ReloadsList()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\sub\b.pptx");
        sut.Presentations.Should().BeEmpty("아직 Load 전");

        sut.IncludeSubfolders = true;

        sut.Presentations.Should().HaveCount(2);
    }
}
