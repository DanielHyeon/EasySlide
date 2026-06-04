using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
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
    public async Task ListNames_IncludesLegacyEspNames_FromConfiguredWorkingFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_PB_Legacy_{Guid.NewGuid():N}");
        try
        {
            var settings = CreateSettings(root);
            var legacyDir = Path.Combine(root, "Admin", "PraiseBooks");
            Directory.CreateDirectory(legacyDir);
            await File.WriteAllTextAsync(Path.Combine(legacyDir, "주일예배.esp"), SampleLegacyEsp("D123", "은혜로다"));
            await File.WriteAllTextAsync(Path.Combine(legacyDir, "저녁예배.esp"), SampleLegacyEsp("D456", "찬양합니다"));
            var store = new PraiseBookStore(_dir, settings);

            store.ListNames().Should().Equal("저녁예배", "주일예배");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task LoadAsync_LegacyEsp_ReadsTitleAndSongId()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_PB_Legacy_{Guid.NewGuid():N}");
        try
        {
            var settings = CreateSettings(root);
            var legacyDir = Path.Combine(root, "Admin", "PraiseBooks");
            Directory.CreateDirectory(legacyDir);
            await File.WriteAllTextAsync(Path.Combine(legacyDir, "주일예배.esp"),
                SampleLegacyEsp("D123", "은혜로다", ("D0", "■예배찬송 <Error - Item Not Found>")));
            var store = new PraiseBookStore(_dir, settings);

            var loaded = await store.LoadAsync("주일예배");

            loaded.Should().HaveCount(2);
            loaded[0].Title.Should().Be("은혜로다");
            loaded[0].SongId.Should().Be(123);
            loaded[0].Number.Should().Be(0, "legacy .esp stores ItemID/SongId, not WPF JSON song number");
            loaded[1].Title.Should().Be("■예배찬송 <Error - Item Not Found>");
            loaded[1].SongId.Should().Be(0);
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task LoadAsync_WhenJsonAndLegacyShareName_UsesJson()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_PB_Legacy_{Guid.NewGuid():N}");
        try
        {
            var settings = CreateSettings(root);
            var legacyDir = Path.Combine(root, "Admin", "PraiseBooks");
            Directory.CreateDirectory(legacyDir);
            await File.WriteAllTextAsync(Path.Combine(legacyDir, "주일예배.esp"), SampleLegacyEsp("D123", "Legacy"));
            var store = new PraiseBookStore(_dir, settings);
            await store.SaveAsync("주일예배", new[] { new PraiseBookIndexEntry("Json", 7, SongId: 9) });

            var loaded = await store.LoadAsync("주일예배");

            loaded.Should().ContainSingle();
            loaded[0].Title.Should().Be("Json");
            loaded[0].Number.Should().Be(7);
            loaded[0].SongId.Should().Be(0, "the current JSON DTO stores title/number only");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { }
        }
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

    private static SettingsService CreateSettings(string workingFolder)
    {
        Directory.CreateDirectory(workingFolder);
        var settings = new SettingsService(new SettingsServiceOptions(
            Path.Combine(workingFolder, "settings.json"),
            Path.Combine(workingFolder, "Backups")));
        settings.Set(EasiSettingKeys.WorkingFolder, workingFolder).Succeeded.Should().BeTrue();
        return settings;
    }

    private static string SampleLegacyEsp(string itemId, string title, params (string ItemId, string Title)[] additionalItems)
    {
        var items = new[] { (ItemId: itemId, Title: title) }.Concat(additionalItems);
        var itemXml = string.Join(Environment.NewLine, items.Select(item => $"""
            <Item>
              <ItemID>{System.Security.SecurityElement.Escape(item.ItemId)}</ItemID>
              <Title1>{System.Security.SecurityElement.Escape(item.Title)}</Title1>
              <Folder />
              <FormatData>0</FormatData>
            </Item>
            """));

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <EasiSlides>
              <ListItem>
                <ListHeader>
                  <SystemID>test</SystemID>
                  <FormatData />
                  <Notes />
                </ListHeader>
            {itemXml}
              </ListItem>
            </EasiSlides>
            """;
    }
}
