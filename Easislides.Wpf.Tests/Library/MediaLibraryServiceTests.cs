using System;
using System.IO;
using System.Linq;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class MediaLibraryServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_MediaLib_{Guid.NewGuid():N}");

    private void Touch(string relative)
    {
        var full = Path.Combine(_dir, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, "x");
    }

    [Fact]
    public void EnumerateMedia_ReturnsOnlyMediaFiles_SortedByName()
    {
        Touch("song.mp3");
        Touch("clip.mp4");
        Touch("notes.txt");   // 미디어 아님
        Touch("slide.pptx");  // 미디어 아님
        var sut = new MediaLibraryService();

        var result = sut.EnumerateMedia(_dir, includeSubfolders: false);

        result.Select(Path.GetFileName).Should().Equal("clip.mp4", "song.mp3");
    }

    [Theory]
    [InlineData("a.mp4")]
    [InlineData("a.avi")]
    [InlineData("a.wmv")]
    [InlineData("a.mov")]
    [InlineData("a.mkv")]
    [InlineData("a.m4v")]
    [InlineData("a.mp3")]
    [InlineData("a.wav")]
    [InlineData("a.wma")]
    [InlineData("a.m4a")]
    [InlineData("a.aac")]
    [InlineData("a.flac")]
    [InlineData("a.ogg")]
    public void EnumerateMedia_AcceptsAllSupportedExtensions(string file)
    {
        Touch(file);
        var sut = new MediaLibraryService();

        sut.EnumerateMedia(_dir, includeSubfolders: false).Should().ContainSingle();
    }

    [Fact]
    public void EnumerateMedia_TopLevelOnly_ExcludesSubfolders()
    {
        Touch("top.mp4");
        Touch("sub/nested.mp4");
        var sut = new MediaLibraryService();

        sut.EnumerateMedia(_dir, includeSubfolders: false)
            .Select(Path.GetFileName).Should().Equal("top.mp4");
    }

    [Fact]
    public void EnumerateMedia_IncludeSubfolders_FindsNested()
    {
        Touch("top.mp4");
        Touch("sub/nested.mp3");
        var sut = new MediaLibraryService();

        sut.EnumerateMedia(_dir, includeSubfolders: true)
            .Select(Path.GetFileName).Should().BeEquivalentTo("nested.mp3", "top.mp4");
    }

    [Fact]
    public void EnumerateMedia_MissingFolder_ReturnsEmpty()
    {
        var sut = new MediaLibraryService();

        sut.EnumerateMedia(@"C:\no\such\__missing__", includeSubfolders: false).Should().BeEmpty();
    }

    [Fact]
    public void EnumerateFolders_ReturnsRootAndSubfolders_AsFrmMainMediaFolderGroups()
    {
        Directory.CreateDirectory(Path.Combine(_dir, "찬양"));
        Directory.CreateDirectory(Path.Combine(_dir, "찬양", "절기"));
        Directory.CreateDirectory(Path.Combine(_dir, "말씀"));
        var sut = new MediaLibraryService();

        var result = sut.EnumerateFolders(_dir);

        result.Select(f => f.DisplayName).Should().Equal(
            "Media Files",
            @"\말씀",
            @"\찬양",
            @"\찬양\절기");
        result[0].FolderPath.Should().Be(Path.GetFullPath(_dir));
        result.Select(f => f.FolderPath).Should().OnlyContain(path => Directory.Exists(path));
    }

    [Fact]
    public void EnumerateFolders_MissingFolder_ReturnsEmpty()
    {
        var sut = new MediaLibraryService();

        sut.EnumerateFolders(@"C:\no\such\__missing__").Should().BeEmpty();
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }
}
