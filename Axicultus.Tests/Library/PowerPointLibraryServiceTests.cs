using System;
using System.IO;
using System.Linq;
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
            Touch(Path.Combine("주일 예배", name));

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
        added.Should().Equal(Path.Combine(external, "2.pptx"), Path.Combine(external, "3.mp3"));
        media.ChangeSourceFolder(builtIn + Path.DirectorySeparatorChar);
        media.FolderGroups.Should().ContainSingle();
        media.MediaFiles.Should().BeEmpty();
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
            "Powerpoint Items",
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
