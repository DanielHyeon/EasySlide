using System;
using System.IO;
using System.Linq;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class ImageLibraryServiceTests
{
    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_ImageLib_{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Touch(string relative)
        {
            var full = System.IO.Path.Combine(Path, relative);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
            File.WriteAllText(full, "x");
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { }
        }
    }

    [Fact]
    public void EnumerateImages_ReturnsOnlyImageFiles_SortedByName()
    {
        using var folder = new TempFolder();
        folder.Touch("b.png");
        folder.Touch("a.jpg");
        folder.Touch("notes.txt");   // 이미지 아님 → 제외
        folder.Touch("song.pptx");   // 이미지 아님 → 제외
        var sut = new ImageLibraryService();

        var result = sut.EnumerateImages(folder.Path, includeSubfolders: false);

        result.Select(System.IO.Path.GetFileName).Should().Equal("a.jpg", "b.png");
    }

    [Fact]
    public void EnumerateImages_TopLevelOnly_ExcludesSubfolders()
    {
        using var folder = new TempFolder();
        folder.Touch("top.png");
        folder.Touch("sub/nested.jpg");
        var sut = new ImageLibraryService();

        var result = sut.EnumerateImages(folder.Path, includeSubfolders: false);

        result.Select(System.IO.Path.GetFileName).Should().Equal("top.png");
    }

    [Fact]
    public void EnumerateImages_IncludeSubfolders_FindsNested()
    {
        using var folder = new TempFolder();
        folder.Touch("top.png");
        folder.Touch("sub/nested.jpg");
        var sut = new ImageLibraryService();

        var result = sut.EnumerateImages(folder.Path, includeSubfolders: true);

        result.Select(System.IO.Path.GetFileName).Should().BeEquivalentTo("nested.jpg", "top.png");
    }

    [Fact]
    public void EnumerateImages_MissingFolder_ReturnsEmpty()
    {
        var sut = new ImageLibraryService();

        var result = sut.EnumerateImages(@"C:\no\such\folder\__missing__", includeSubfolders: false);

        result.Should().BeEmpty();
    }

    [Fact]
    public void EnumerateImages_BlankFolder_ReturnsEmpty()
    {
        var sut = new ImageLibraryService();

        sut.EnumerateImages("", includeSubfolders: false).Should().BeEmpty();
        sut.EnumerateImages("   ", includeSubfolders: false).Should().BeEmpty();
    }
}
