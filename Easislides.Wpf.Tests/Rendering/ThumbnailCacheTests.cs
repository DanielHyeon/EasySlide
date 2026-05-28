using System;
using System.IO;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class ThumbnailCacheTests
{
    [Fact]
    public void TryGet_AfterStore_ReturnsSnapshotAndStats()
    {
        var sut = new ThumbnailCache(new ThumbnailCacheOptions(MaxEntries: 10, MaxBytes: 1024));
        var key = Key("deck.pptx", itemNumber: 1);
        var snapshot = Slide(key.SourcePath, slideNumber: 1, bytes: [1, 2, 3, 4]);

        sut.Store(key, snapshot, byteSize: snapshot.ImageBytes.LongLength);

        sut.TryGet<PowerPointSlideSnapshot>(key, out var cached).Should().BeTrue();
        cached.Should().BeSameAs(snapshot);
        sut.Snapshot().Should().Be(new ThumbnailCacheStats(Count: 1, TotalBytes: 4, EvictionCount: 0));
    }

    [Fact]
    public void TryGet_WhenFileStampChanges_ReturnsMiss()
    {
        using var fixture = TempFile.Create(".pptx");
        var sut = new ThumbnailCache();
        var request = Request(fixture.Path, itemNumber: 1);
        var firstKey = ThumbnailCacheKey.FromFile(request, new FileInfo(fixture.Path));
        sut.Store(firstKey, Slide(firstKey.SourcePath, slideNumber: 1, bytes: [1]));

        fixture.TouchWithNewContent();
        var changedKey = ThumbnailCacheKey.FromFile(request, new FileInfo(fixture.Path));

        changedKey.Should().NotBe(firstKey);
        sut.TryGet<PowerPointSlideSnapshot>(changedKey, out _).Should().BeFalse();
    }

    [Fact]
    public void InvalidateSource_RemovesEveryVariantForPath()
    {
        var sut = new ThumbnailCache();
        var source = TempPath("deck.pptx");
        var first = Key(source, itemNumber: 1, variant: "render");
        var second = Key(source, itemNumber: 2, variant: "preview");
        var other = Key("other.pptx", itemNumber: 1);
        sut.Store(first, Slide(source, slideNumber: 1, bytes: [1]));
        sut.Store(second, Slide(source, slideNumber: 2, bytes: [2]));
        sut.Store(other, Slide(other.SourcePath, slideNumber: 1, bytes: [3]));

        var removed = sut.InvalidateSource(source.ToUpperInvariant());

        removed.Should().Be(2);
        sut.TryGet<PowerPointSlideSnapshot>(first, out _).Should().BeFalse();
        sut.TryGet<PowerPointSlideSnapshot>(second, out _).Should().BeFalse();
        sut.TryGet<PowerPointSlideSnapshot>(other, out _).Should().BeTrue();
        sut.Snapshot().Count.Should().Be(1);
    }

    [Fact]
    public void Store_WhenEntryLimitExceeded_EvictsLeastRecentlyUsed()
    {
        var sut = new ThumbnailCache(new ThumbnailCacheOptions(MaxEntries: 2, MaxBytes: 1024));
        var first = Key("first.pptx", itemNumber: 1);
        var second = Key("second.pptx", itemNumber: 1);
        var third = Key("third.pptx", itemNumber: 1);
        sut.Store(first, Slide(first.SourcePath, slideNumber: 1, bytes: [1]));
        sut.Store(second, Slide(second.SourcePath, slideNumber: 1, bytes: [2]));
        sut.TryGet<PowerPointSlideSnapshot>(first, out _).Should().BeTrue();

        sut.Store(third, Slide(third.SourcePath, slideNumber: 1, bytes: [3]));

        sut.TryGet<PowerPointSlideSnapshot>(first, out _).Should().BeTrue();
        sut.TryGet<PowerPointSlideSnapshot>(second, out _).Should().BeFalse();
        sut.TryGet<PowerPointSlideSnapshot>(third, out _).Should().BeTrue();
        sut.Snapshot().EvictionCount.Should().Be(1);
    }

    [Fact]
    public void Store_WhenByteLimitExceeded_EvictsOldestUntilUnderLimit()
    {
        var sut = new ThumbnailCache(new ThumbnailCacheOptions(MaxEntries: 10, MaxBytes: 5));
        var first = Key("first.pptx", itemNumber: 1);
        var second = Key("second.pptx", itemNumber: 1);
        var third = Key("third.pptx", itemNumber: 1);
        sut.Store(first, Slide(first.SourcePath, slideNumber: 1, bytes: [1, 2]), byteSize: 2);
        sut.Store(second, Slide(second.SourcePath, slideNumber: 1, bytes: [3, 4]), byteSize: 2);

        sut.Store(third, Slide(third.SourcePath, slideNumber: 1, bytes: [5, 6, 7]), byteSize: 3);

        sut.TryGet<PowerPointSlideSnapshot>(first, out _).Should().BeFalse();
        sut.TryGet<PowerPointSlideSnapshot>(second, out _).Should().BeTrue();
        sut.TryGet<PowerPointSlideSnapshot>(third, out _).Should().BeTrue();
        sut.Snapshot().TotalBytes.Should().Be(5);
    }

    private static ThumbnailCacheRequest Request(string sourcePath, int itemNumber, string variant = "render")
        => new(sourcePath, ThumbnailSourceKind.PowerPointSlide, itemNumber, 1280, 720, variant);

    private static ThumbnailCacheKey Key(
        string sourcePath,
        int itemNumber,
        string variant = "render",
        long stamp = 1,
        long length = 10)
        => new(
            Path.GetFullPath(sourcePath).ToUpperInvariant(),
            ThumbnailSourceKind.PowerPointSlide,
            itemNumber,
            PixelWidth: 1280,
            PixelHeight: 720,
            variant,
            stamp,
            length);

    private static PowerPointSlideSnapshot Slide(string sourcePath, int slideNumber, byte[] bytes)
        => new(
            sourcePath,
            slideNumber,
            SlideCount: 3,
            PixelWidth: 1280,
            PixelHeight: 720,
            bytes,
            ContentType: "image/jpeg",
            GeneratedAtUtc: DateTimeOffset.UtcNow);

    private static string TempPath(string fileName) => Path.Combine(Path.GetTempPath(), fileName);

    private sealed class TempFile : IDisposable
    {
        private TempFile(string path) => Path = path;

        public string Path { get; }

        public static TempFile Create(string extension)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_{Guid.NewGuid():N}{extension}");
            File.WriteAllText(path, "fixture");
            return new TempFile(path);
        }

        public void TouchWithNewContent()
        {
            File.AppendAllText(Path, " updated");
            File.SetLastWriteTimeUtc(Path, DateTime.UtcNow.AddMinutes(1));
        }

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch
            {
            }
        }
    }
}
