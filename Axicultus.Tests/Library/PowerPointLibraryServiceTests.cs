using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Axicultus.Module;
using Axicultus.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class PowerPointLibraryServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"Axicultus_PptLib_{Guid.NewGuid():N}");

    private void Touch(string relative)
    {
        var full = Path.Combine(_dir, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, "x");
    }

    [Fact]
    public void MixedSourceFolderFeedsBothLibrariesAndAddUsesTheSelectedFilePath()
    {
        var builtIn = Path.Combine(_dir, "built-in");
        var external = Path.Combine(_dir, "주일 예배");
        Directory.CreateDirectory(builtIn);
        foreach (var name in new[] { "1.PPT", "2.pptx", "~$2.pptx", "3.mp3", "4.MP4", "5.txt" })
            Touch(Path.Combine("주일 예배", "주일", "2부", name));

        var added = new System.Collections.Generic.List<string>();
        var presentations = new PowerPointLibraryViewModel(new PowerPointLibraryService(), added.Add, builtIn, sourceFolder: external);
        var media = new MediaLibraryViewModel(new MediaLibraryService(), added.Add, builtIn);
        presentations.LoadCommand.Execute(null);
        media.ChangeSourceFolder(external);
        presentations.Presentations.Select(file => file.FileName).Should().Equal("1.PPT", "2.pptx");
        media.MediaFiles.Select(file => file.FileName).Should().Equal("3.mp3", "4.MP4");
        added.Should().BeEmpty();
        presentations.SelectedFile = presentations.Presentations[1];
        presentations.AddSelectedCommand.Execute(null);
        media.SelectedFile = media.MediaFiles[0];
        media.AddSelectedCommand.Execute(null);
        added.Should().Equal(Path.Combine(external, "주일", "2부", "2.pptx"), Path.Combine(external, "주일", "2부", "3.mp3"));
        media.ChangeSourceFolder(builtIn + Path.DirectorySeparatorChar);
        media.FolderGroups.Should().ContainSingle();
        media.MediaFiles.Should().BeEmpty();
    }

    [Fact]
    public void NumberedDecksKeepTheirOrderAcrossFoldersFilteringAndPreview()
    {
        string[] relativePaths = ["찬송1장.pptx", "찬송2장.pptx", "10부/찬송6장.pptx", "2부/찬송7장.ppt", "찬송10장.pptx",
            "2부/찬송60장.pptx", "10부/찬송60장.pptx", "찬송100장.pptx"];
        foreach (var path in relativePaths.Reverse()) Touch(path);
        Touch("~$찬송1장.pptx");

        string? added = null;
        var tab = new PowerPointLibraryViewModel(new PowerPointLibraryService(), path => added = path, _dir);
        tab.LoadCommand.Execute(null);
        var expected = relativePaths.Select(path => Path.GetFullPath(Path.Combine(_dir, path))).ToArray();
        tab.FolderGroups.Select(group => group.DisplayName).Should().Equal(Axicultus.Wpf.Localization.UiText.Get("PowerPointLibraryService", "RootFolder"), @"\2부", @"\10부");
        tab.Presentations.Select(item => item.FilePath).Should().Equal(expected);
        tab.UsePreviewStyleCommand.Execute(null);
        tab.Presentations.Select(item => item.FilePath).Should().Equal(expected);
        tab.FilterText = "60장";
        var sixties = expected.Where(path => Path.GetFileName(path) == "찬송60장.pptx").ToArray();
        tab.Presentations.Select(item => item.FilePath).Should().Equal(sixties);
        tab.SelectedFile = tab.Presentations[1];
        tab.AddSelectedCommand.Execute(null);
        added.Should().Be(sixties[1]);
    }

    [Fact]
    public void ResortPublishesTheCompleteThumbnailOrderInOneReset()
    {
        string[] names = ["1장_만복.pptx", "6장_목소리.pptx", "10장_전능.pptx", "60장_영혼.pptx", "100장_미리암.pptx", "136장_가나.pptx"];
        foreach (var name in names) Touch(name);
        var tab = new PowerPointLibraryViewModel(new PowerPointLibraryService(), _ => { }, _dir);
        tab.LoadCommand.Execute(null);
        tab.UsePreviewStyleCommand.Execute(null);
        var numbered = tab.Presentations.ToArray();
        var changes = new List<NotifyCollectionChangedAction>();
        PowerPointFileItem[] visibleAtReset = [];
        tab.Presentations.CollectionChanged += (_, args) =>
        {
            changes.Add(args.Action);
            visibleAtReset = tab.Presentations.ToArray();
        };

        tab.SelectSortOrderCommand.Execute(PowerPointSortOrder.Title);
        Assert.True(tab.IsTitleSort);
        Assert.False(tab.IsHymnNumberSort);
        changes.Should().Equal(NotifyCollectionChangedAction.Reset);
        visibleAtReset.Should().Equal(numbered[5], numbered[0], numbered[1], numbered[4], numbered[3], numbered[2]);
    }

    [Fact]
    public void SortSelectorUsesTheSongTitleOrHymnNumberAndKeepsTheFilteredSelection()
    {
        string[] numberOrder = ["1장_하늘 찬송.pptx", "6장 찬송의 소리.ppt", "찬10장_감사 찬송.pptx",
            "100장_나의 찬송.pptx", "가정 찬송.pptx"];
        foreach (var name in numberOrder.Reverse()) Touch(name);
        Touch("200장_별도 파일.pptx");
        var tab = new PowerPointLibraryViewModel(new PowerPointLibraryService(), _ => { }, _dir);
        tab.LoadCommand.Execute(null);
        tab.FilterText = "찬송";
        tab.Presentations.Select(file => file.FileName).Should().Equal(numberOrder);
        var selected = tab.SelectedFile = tab.Presentations[2];

        tab.SortOrder = PowerPointSortOrder.Title;
        tab.Presentations.Select(file => file.FileName).Should().Equal(numberOrder[4], numberOrder[2], numberOrder[3], numberOrder[1], numberOrder[0]);
        tab.SelectedFile.Should().BeSameAs(selected);
        tab.UsePreviewStyleCommand.Execute(null);
        tab.SortOrder = PowerPointSortOrder.HymnNumber;
        tab.FilterText.Should().Be("찬송");
        tab.SelectedFile.Should().BeSameAs(selected);
        tab.Presentations.Select(file => file.FileName).Should().Equal(numberOrder);
    }

    [Fact]
    public void EnumeratePresentations_ReturnsOnlyPptFiles_SortedByName()
    {
        Touch("b.pptx");
        Touch("a.ppt");
        Touch("notes.txt");
        Touch("~$b.pptx");
        Touch("image.png");
        var sut = new PowerPointLibraryService();

        var result = sut.EnumeratePresentations(_dir, includeSubfolders: false);

        result.Select(Path.GetFileName).Should().Equal("a.ppt", "b.pptx");
    }

    [Fact]
    public void EnumeratePresentations_TopLevelOnly_ExcludesSubfolders()
    {
        Touch("top.pptx");
        Touch("sub/nested.pptx");
        var sut = new PowerPointLibraryService();

        sut.EnumeratePresentations(_dir, includeSubfolders: false)
            .Select(Path.GetFileName).Should().Equal("top.pptx");
    }

    [Fact]
    public void EnumeratePresentations_IncludeSubfolders_FindsNested()
    {
        Touch("top.pptx");
        Touch("sub/nested.pptx");
        var sut = new PowerPointLibraryService();

        sut.EnumeratePresentations(_dir, includeSubfolders: true)
            .Select(Path.GetFileName).Should().BeEquivalentTo("nested.pptx", "top.pptx");
    }

    [Fact]
    public void EnumeratePresentations_MissingFolder_ReturnsEmpty()
    {
        var sut = new PowerPointLibraryService();

        sut.EnumeratePresentations(@"C:\no\such\__missing__", includeSubfolders: false).Should().BeEmpty();
    }

    [Fact]
    public void EnumerateFolders_ReturnsRootAndSubfolders_AsFrmMainPowerpointFolderGroups()
    {
        Directory.CreateDirectory(Path.Combine(_dir, "찬양"));
        Directory.CreateDirectory(Path.Combine(_dir, "찬양", "절기"));
        Directory.CreateDirectory(Path.Combine(_dir, "말씀"));
        var sut = new PowerPointLibraryService();

        var result = sut.EnumerateFolders(_dir);

        result.Select(f => f.DisplayName).Should().Equal(
            Axicultus.Wpf.Localization.UiText.Get("PowerPointLibraryService", "RootFolder"),
            @"\말씀",
            @"\찬양",
            @"\찬양\절기");
        result[0].FolderPath.Should().Be(Path.GetFullPath(_dir));
        result.Select(f => f.FolderPath).Should().OnlyContain(path => Directory.Exists(path));
    }

    [Fact]
    public void EnumerateFolders_MissingFolder_ReturnsEmpty()
    {
        var sut = new PowerPointLibraryService();

        sut.EnumerateFolders(@"C:\no\such\__missing__").Should().BeEmpty();
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }
}
