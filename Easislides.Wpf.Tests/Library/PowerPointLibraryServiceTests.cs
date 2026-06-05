using System;
using System.IO;
using System.Linq;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PowerPointLibraryServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_PptLib_{Guid.NewGuid():N}");

    private void Touch(string relative)
    {
        var full = Path.Combine(_dir, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, "x");
    }

    [Fact]
    public void EnumeratePresentations_ReturnsOnlyPptFiles_SortedByName()
    {
        Touch("b.pptx");
        Touch("a.ppt");
        Touch("notes.txt");
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
