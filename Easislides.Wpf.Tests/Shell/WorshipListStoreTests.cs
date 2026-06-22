using System;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>워십 리스트 JSON 영속 스토어 검증 (G2 / FrmManageItemLists 대응). 임시 폴더 사용.</summary>
public sealed class WorshipListStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_WL_{Guid.NewGuid():N}");

    [Fact]
    public async Task Save_Then_Load_RoundTripsPersistentFields()
    {
        var store = new WorshipListStore(_dir);
        var items = new[]
        {
            new LiveQueueItem("song:1", "찬양", "Song") { Lyrics = "은혜", SlideNumber = 2 },
            new LiveQueueItem("bible:1", "요한복음", "Bible") { ContentPath = null },
        };

        await store.SaveAsync("주일오전", items);
        var loaded = await store.LoadAsync("주일오전");

        loaded.Should().HaveCount(2);
        loaded[0].Id.Should().Be("song:1");
        loaded[0].Kind.Should().Be("Song");
        loaded[0].Lyrics.Should().Be("은혜");
        loaded[0].SlideNumber.Should().Be(2);
        loaded[1].Title.Should().Be("요한복음");
    }

    [Fact]
    public async Task Save_Then_Load_RoundTripsFormatDataAndIndividualFlag()
    {
        // 곡별 서식·개별 서식 사용 플래그가 저장/불러오기로 보존된다.
        var store = new WorshipListStore(_dir);
        var items = new[]
        {
            new LiveQueueItem("song:1", "찬양", "Song") { FormatData = "29=-1", UseIndividualFormatting = false },
            new LiveQueueItem("song:2", "찬양2", "Song") { FormatData = "31=2" }, // 개별 서식 기본 true.
        };

        await store.SaveAsync("주일", items);
        var loaded = await store.LoadAsync("주일");

        loaded[0].FormatData.Should().Be("29=-1");
        loaded[0].UseIndividualFormatting.Should().BeFalse();
        loaded[1].FormatData.Should().Be("31=2");
        loaded[1].UseIndividualFormatting.Should().BeTrue("기본값");
    }

    [Fact]
    public async Task Load_OldJsonWithoutIndividualFields_DefaultsSafely()
    {
        // 구버전 JSON(FormatData·UseIndividualFormatting 없음) → 기본값(null·true)으로 안전 복원.
        var path = System.IO.Path.Combine(_dir, "구버전.json");
        System.IO.Directory.CreateDirectory(_dir);
        await System.IO.File.WriteAllTextAsync(path,
            """[{"Id":"song:1","Title":"찬양","Kind":"Song","SlideNumber":0,"Lyrics":"가사","ContentPath":null}]""");
        var store = new WorshipListStore(_dir);

        var loaded = await store.LoadAsync("구버전");

        loaded.Should().ContainSingle();
        loaded[0].FormatData.Should().BeNull();
        loaded[0].UseIndividualFormatting.Should().BeTrue();
    }

    [Fact]
    public async Task ListNames_ReturnsSavedNames_Sorted()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("Bravo", Array.Empty<LiveQueueItem>());
        await store.SaveAsync("Alpha", Array.Empty<LiveQueueItem>());

        store.ListNames().Should().Equal("Alpha", "Bravo");
    }

    [Fact]
    public async Task ListNames_IncludesLegacyEswNames_FromConfiguredWorkingFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_LegacyWL_{Guid.NewGuid():N}");
        try
        {
            var settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups")));
            settings.Set(EasiSettingKeys.WorkingFolder, root);
            var legacyDir = Path.Combine(root, "Admin", "WorshipLists");
            Directory.CreateDirectory(legacyDir);
            await File.WriteAllTextAsync(Path.Combine(legacyDir, "1.Sunday.esw"), "<EasiSlides />");

            var store = new WorshipListStore(_dir, settings);
            await store.SaveAsync("WpfJson", Array.Empty<LiveQueueItem>());

            store.ListNames().Should().Contain(new[] { "1.Sunday", "WpfJson" });
            var xml = await ((ILegacyWorshipListStore)store).LoadLegacyXmlAsync("1.Sunday");
            xml.Should().Contain("EasiSlides");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ListNames_UsesRuntimeLegacyWorkingFolderFallback()
    {
        var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_LegacyWLFallback_{Guid.NewGuid():N}");
        try
        {
            var legacyRoot = Path.Combine(root, "LegacyRoot");
            var legacyDir = Path.Combine(legacyRoot, "Admin", "WorshipLists");
            Directory.CreateDirectory(legacyDir);
            await File.WriteAllTextAsync(Path.Combine(legacyDir, "1.Sunday.esw"), "<EasiSlides />");
            var settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups"),
                legacyRoot));

            var store = new WorshipListStore(_dir, settings);

            store.ListNames().Should().Contain("1.Sunday");
            var xml = await ((ILegacyWorshipListStore)store).LoadLegacyXmlAsync("1.Sunday");
            xml.Should().Contain("EasiSlides");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Delete_RemovesSavedList()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("Temp", Array.Empty<LiveQueueItem>());

        store.Delete("Temp");

        store.ListNames().Should().NotContain("Temp");
    }

    [Fact]
    public async Task Load_MissingName_ReturnsEmpty()
    {
        var store = new WorshipListStore(_dir);

        var loaded = await store.LoadAsync("없는이름");

        loaded.Should().BeEmpty();
    }

    [Theory]
    [InlineData("..\\escape")]
    [InlineData("bad/name")]
    [InlineData("")]
    [InlineData("CON")]
    [InlineData("com1")]
    [InlineData("trailingdot.")]
    public async Task Save_InvalidName_Throws(string name)
    {
        var store = new WorshipListStore(_dir);

        var act = async () => await store.SaveAsync(name, Array.Empty<LiveQueueItem>());

        await act.Should().ThrowAsync<ArgumentException>("경로 탈출/무효 문자/예약명/끝점/빈 이름은 차단");
    }

    [Fact]
    public async Task Save_SameName_OverwritesPrevious()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("주일", new[] { new LiveQueueItem("a", "A", "Song") });
        await store.SaveAsync("주일", new[] { new LiveQueueItem("b", "B", "Bible"), new LiveQueueItem("c", "C", "Song") });

        var loaded = await store.LoadAsync("주일");

        loaded.Should().HaveCount(2);
        loaded[0].Id.Should().Be("b");
    }

    [Fact]
    public async Task Save_Then_Load_RoundTripsContentPath()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("미디어", new[]
        {
            new LiveQueueItem("ppt:1", "Deck", "PowerPoint") { ContentPath = @"C:\decks\sermon.pptx" },
        });

        var loaded = await store.LoadAsync("미디어");

        loaded[0].ContentPath.Should().Be(@"C:\decks\sermon.pptx");
    }

    [Fact]
    public async Task Rename_MovesList_PreservingItems()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("옛이름", new[] { new LiveQueueItem("song:1", "찬양", "Song") { Lyrics = "은혜" } });

        store.Rename("옛이름", "새이름");

        store.ListNames().Should().Contain("새이름").And.NotContain("옛이름");
        var loaded = await store.LoadAsync("새이름");
        loaded.Should().ContainSingle().Which.Lyrics.Should().Be("은혜", "이름만 바뀌고 내용은 보존");
    }

    [Fact]
    public async Task Rename_ToExistingName_Throws_NoSilentOverwrite()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("A", new[] { new LiveQueueItem("a", "A", "Song") });
        await store.SaveAsync("B", new[] { new LiveQueueItem("b", "B", "Song") });

        var act = () => store.Rename("A", "B");

        act.Should().Throw<ArgumentException>("이미 있는 이름으로 바꾸면 덮어쓰지 않고 거부");
        store.ListNames().Should().Contain(new[] { "A", "B" }, "둘 다 그대로 남는다");
    }

    [Fact]
    public void Rename_MissingSource_NoOp()
    {
        var store = new WorshipListStore(_dir);

        var act = () => store.Rename("없는목록", "아무거나");

        act.Should().NotThrow("원본이 없으면 조용히 무시(Delete 와 동일하게 관대)");
        store.ListNames().Should().BeEmpty();
    }

    [Fact]
    public async Task Rename_InvalidNewName_Throws()
    {
        var store = new WorshipListStore(_dir);
        await store.SaveAsync("원본", Array.Empty<LiveQueueItem>());

        var act = () => store.Rename("원본", "bad/name");

        act.Should().Throw<ArgumentException>("새 이름도 경로 안전 검증을 거친다");
    }

    [Fact]
    public async Task Load_CorruptFile_ReturnsEmpty()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(Path.Combine(_dir, "깨진목록.json"), "{ this is not valid json [[[");
        var store = new WorshipListStore(_dir);

        var loaded = await store.LoadAsync("깨진목록");

        loaded.Should().BeEmpty("손상 파일은 크래시 없이 빈 목록으로 처리");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, recursive: true);
            }
        }
        catch
        {
        }
    }
}
