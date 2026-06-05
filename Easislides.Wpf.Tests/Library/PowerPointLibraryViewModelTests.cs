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
    public void ListingStyle_DefaultsToList_AndCanSwitchToPreview()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx");

        sut.ListingStyle.Should().Be(PowerPointListingStyle.List);
        sut.IsListStyle.Should().BeTrue();
        sut.IsPreviewStyle.Should().BeFalse();
        sut.ListingStyleLabel.Should().Be("List");

        sut.UsePreviewStyleCommand.Execute(null);

        sut.ListingStyle.Should().Be(PowerPointListingStyle.Preview);
        sut.IsListStyle.Should().BeFalse();
        sut.IsPreviewStyle.Should().BeTrue();
        sut.ListingStyleLabel.Should().Be("Preview");

        sut.UseListStyleCommand.Execute(null);
        sut.ListingStyle.Should().Be(PowerPointListingStyle.List);
    }

    [Fact]
    public void TogglingIncludeSubfolders_ReloadsList()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\sub\b.pptx");
        sut.Presentations.Should().BeEmpty("아직 Load 전");

        sut.IncludeSubfolders = true;

        sut.Presentations.Should().HaveCount(2);
    }

    [Fact]
    public void FilterText_NarrowsListByFileName_WithoutReloading()
    {
        var sut = CreateSut(out _, @"C:\decks\찬양.pptx", @"C:\decks\공지사항.pptx", @"C:\decks\intro.ppt");
        sut.LoadCommand.Execute(null);
        sut.Presentations.Should().HaveCount(3);

        sut.FilterText = "공지";

        sut.Presentations.Select(p => p.FileName).Should().Equal("공지사항.pptx");
        sut.StatusText.Should().Contain("1/3");
    }

    [Fact]
    public void FilterText_Cleared_RestoresFullList()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\b.pptx");
        sut.LoadCommand.Execute(null);
        sut.FilterText = "a";
        sut.Presentations.Should().HaveCount(1);

        sut.FilterText = "";

        sut.Presentations.Should().HaveCount(2, "검색어 지우면 전체 복원");
    }

    [Fact]
    public void FilterText_ExcludingSelected_DeselectsIt_AndDisablesAdd()
    {
        // 선택한 PPT 가 검색에 걸러져 사라지면 선택을 해제한다 — 숨은 항목이 선택된 채 추가되는 사고 방지.
        var sut = CreateSut(out var added, @"C:\decks\a.pptx", @"C:\decks\b.pptx");
        sut.LoadCommand.Execute(null);
        sut.SelectedFile = sut.Presentations.First(p => p.FileName == "a.pptx");

        sut.FilterText = "b";

        sut.SelectedFile.Should().BeNull("걸러져 사라진 항목은 선택 해제");
        sut.AddSelectedCommand.CanExecute(null).Should().BeFalse();
        added.Should().BeEmpty();
    }
}
