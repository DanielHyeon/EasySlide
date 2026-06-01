using System;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

/// <summary>찬양집 JSON 영속 스토어 검증(FrmMain PraiseBookDir 대응). 임시 폴더 사용.</summary>
public sealed class PraiseBookStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_PB_{Guid.NewGuid():N}");

    [Fact]
    public async Task Save_Then_Load_RoundTripsEntries()
    {
        var store = new PraiseBookStore(_dir);
        var entries = new[]
        {
            new PraiseBookIndexEntry("가나", 1),
            new PraiseBookIndexEntry("하늘", 305),
        };

        await store.SaveAsync("주일오전", entries);
        var loaded = await store.LoadAsync("주일오전");

        loaded.Should().HaveCount(2);
        loaded[0].Title.Should().Be("가나");
        loaded[0].Number.Should().Be(1);
        loaded[1].Title.Should().Be("하늘");
        loaded[1].Number.Should().Be(305);
    }

    [Fact]
    public async Task ListNames_ReturnsSavedNames_Sorted()
    {
        var store = new PraiseBookStore(_dir);
        await store.SaveAsync("Bravo", Array.Empty<PraiseBookIndexEntry>());
        await store.SaveAsync("Alpha", Array.Empty<PraiseBookIndexEntry>());

        store.ListNames().Should().Equal("Alpha", "Bravo");
    }

    [Fact]
    public async Task Delete_RemovesSavedBook()
    {
        var store = new PraiseBookStore(_dir);
        await store.SaveAsync("Temp", Array.Empty<PraiseBookIndexEntry>());

        store.Delete("Temp");

        store.ListNames().Should().NotContain("Temp");
    }

    [Fact]
    public async Task Load_MissingName_ReturnsEmpty()
    {
        var store = new PraiseBookStore(_dir);

        var loaded = await store.LoadAsync("없음");

        loaded.Should().BeEmpty();
    }

    [Fact]
    public async Task Save_OverwritesExisting()
    {
        var store = new PraiseBookStore(_dir);
        await store.SaveAsync("책", new[] { new PraiseBookIndexEntry("첫번째", 1) });
        await store.SaveAsync("책", new[] { new PraiseBookIndexEntry("두번째", 2) });

        var loaded = await store.LoadAsync("책");

        loaded.Should().ContainSingle();
        loaded[0].Title.Should().Be("두번째");
    }

    [Fact]
    public async Task Rename_MovesBook()
    {
        var store = new PraiseBookStore(_dir);
        await store.SaveAsync("옛이름", new[] { new PraiseBookIndexEntry("곡", 1) });

        store.Rename("옛이름", "새이름");

        store.ListNames().Should().Equal("새이름");
        (await store.LoadAsync("새이름")).Should().ContainSingle();
    }

    [Fact]
    public async Task Rename_ToExistingName_Throws()
    {
        var store = new PraiseBookStore(_dir);
        await store.SaveAsync("A", Array.Empty<PraiseBookIndexEntry>());
        await store.SaveAsync("B", Array.Empty<PraiseBookIndexEntry>());

        var act = () => store.Rename("A", "B");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("..\\escape")]
    [InlineData("sub/dir")]
    [InlineData("CON")]
    [InlineData("")]
    public async Task Save_RejectsUnsafeNames(string name)
    {
        var store = new PraiseBookStore(_dir);

        var act = async () => await store.SaveAsync(name, Array.Empty<PraiseBookIndexEntry>());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Load_CorruptFile_ReturnsEmpty()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(Path.Combine(_dir, "깨진.json"), "{ not json");
        var store = new PraiseBookStore(_dir);

        var loaded = await store.LoadAsync("깨진");

        loaded.Should().BeEmpty();
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }
}
