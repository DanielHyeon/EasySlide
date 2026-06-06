using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PraiseBookIndexViewModelTests
{
    // 인-메모리 가짜 스토어(파일 시스템 없이 저장/불러오기/목록/삭제 VM 로직 검증).
    private sealed class FakePraiseBookStore : IPraiseBookStore
    {
        private readonly Dictionary<string, IReadOnlyList<PraiseBookIndexEntry>> _books = new();

        public Task SaveAsync(string name, IReadOnlyList<PraiseBookIndexEntry> entries, CancellationToken cancellationToken = default)
        {
            _books[name] = entries.ToList();
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PraiseBookIndexEntry>> LoadAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(_books.TryGetValue(name, out var entries) ? entries : Array.Empty<PraiseBookIndexEntry>());

        public IReadOnlyList<string> ListNames() => _books.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();

        public void Delete(string name) => _books.Remove(name);

        public void Rename(string oldName, string newName)
        {
            if (_books.Remove(oldName, out var entries))
            {
                _books[newName] = entries;
            }
        }
    }

    private static PraiseBookIndexViewModel CreateSut(IPraiseBookStore store, params PraiseBookIndexEntry[] songs)
        => new(new PraiseBookIndexService(), store, songs);

    [Fact]
    public void Constructor_BuildsGroupsFromSongs()
    {
        var sut = CreateSut(
            new FakePraiseBookStore(),
            new PraiseBookIndexEntry("가나", 1),
            new PraiseBookIndexEntry("하늘", 2));

        sut.Groups.Select(g => g.Key).Should().Equal("ㄱ", "ㅎ");
        sut.Entries.Select(entry => entry.Title).Should().Equal("가나", "하늘");
        sut.StatusText.Should().Contain("2곡");
    }

    [Fact]
    public void Constructor_EmptySongs_SetsEmptyStatus()
    {
        var sut = CreateSut(new FakePraiseBookStore());

        sut.Groups.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }

    [Fact]
    public async Task SaveAs_PersistsCurrentEntriesAndAppearsInSavedBooks()
    {
        var store = new FakePraiseBookStore();
        var sut = CreateSut(store, new PraiseBookIndexEntry("가나", 1));

        await sut.SaveAsCommand.ExecuteAsync("주일 1부");

        sut.SavedBooks.Should().Contain("주일 1부");
        sut.CurrentBookName.Should().Be("주일 1부");
        store.ListNames().Should().Contain("주일 1부");
    }

    [Fact]
    public async Task SaveAs_BlankName_DoesNothing()
    {
        var store = new FakePraiseBookStore();
        var sut = CreateSut(store, new PraiseBookIndexEntry("가나", 1));

        await sut.SaveAsCommand.ExecuteAsync("   ");

        sut.SavedBooks.Should().BeEmpty();
        store.ListNames().Should().BeEmpty();
    }

    [Fact]
    public async Task OpenBook_ReplacesCurrentEntriesAndRebuildsIndex()
    {
        var store = new FakePraiseBookStore();
        await store.SaveAsync("저녁예배", new[] { new PraiseBookIndexEntry("바다", 5) });
        // 초기에는 라이브러리 곡(가나=ㄱ)으로 색인.
        var sut = CreateSut(store, new PraiseBookIndexEntry("가나", 1));
        sut.Groups.Select(g => g.Key).Should().Equal("ㄱ");

        await sut.OpenBookCommand.ExecuteAsync("저녁예배");

        sut.Groups.Select(g => g.Key).Should().Equal("ㅂ"); // 바다=ㅂ 그룹으로 교체
        sut.Entries.Should().ContainSingle().Which.Title.Should().Be("바다");
        sut.CurrentBookName.Should().Be("저녁예배");
    }

    [Fact]
    public void AddEntry_AppendsToCurrentEntriesAndRebuildsFlatList()
    {
        var sut = CreateSut(new FakePraiseBookStore(), new PraiseBookIndexEntry("가나", 1));

        var added = sut.AddEntry(new PraiseBookIndexEntry("하늘", 2, SongId: 9));

        added.Should().BeTrue();
        sut.Entries.Select(entry => entry.Title).Should().Equal("가나", "하늘");
        sut.Groups.Select(group => group.Key).Should().Equal("ㄱ", "ㅎ");
        sut.StatusText.Should().Contain("하늘");
    }

    [Fact]
    public void ToggleWordCountSort_RebuildsCurrentBookLikeFrmMainPBWordCount()
    {
        var sut = CreateSut(
            new FakePraiseBookStore(),
            new PraiseBookIndexEntry("가나다", 1),
            new PraiseBookIndexEntry("나", 2),
            new PraiseBookIndexEntry("가나", 3));
        sut.Entries.Select(entry => entry.Title).Should().Equal("가나", "가나다", "나");

        sut.ToggleWordCountSort();

        sut.IsWordCountSortEnabled.Should().BeTrue();
        sut.Groups.Select(group => group.Key).Should().Equal("001", "002", "003");
        sut.Entries.Select(entry => entry.Title).Should().Equal("나", "가나", "가나다");
        sut.StatusText.Should().Contain("CJK Word Count");
    }

    [Fact]
    public void Constructor_WhenInitialWordCountSortIsTrue_BuildsWordCountIndex()
    {
        var sut = new PraiseBookIndexViewModel(
            new PraiseBookIndexService(),
            new FakePraiseBookStore(),
            [
                new PraiseBookIndexEntry("\uAC00\uB098\uB2E4", 1),
                new PraiseBookIndexEntry("\uAC00", 2),
            ],
            initialWordCountSort: true);

        sut.IsWordCountSortEnabled.Should().BeTrue();
        sut.Groups.Select(group => group.Key).Should().Equal("001", "003");
        sut.Entries.Select(entry => entry.Title).Should().Equal("\uAC00", "\uAC00\uB098\uB2E4");
    }

    [Fact]
    public void ToggleWordCountSort_NotifiesPersistenceCallback()
    {
        bool? persisted = null;
        var sut = new PraiseBookIndexViewModel(
            new PraiseBookIndexService(),
            new FakePraiseBookStore(),
            [new PraiseBookIndexEntry("\uAC00", 1)],
            wordCountSortChanged: enabled => persisted = enabled);

        sut.ToggleWordCountSort();

        persisted.Should().BeTrue();

        sut.ToggleWordCountSort();

        persisted.Should().BeFalse();
    }

    [Fact]
    public void RemoveEntries_RemovesSelectedRowsAndRebuildsGroups()
    {
        var sut = CreateSut(
            new FakePraiseBookStore(),
            new PraiseBookIndexEntry("가나", 1),
            new PraiseBookIndexEntry("하늘", 2));

        var removed = sut.RemoveEntries([sut.Entries.Single(entry => entry.Title == "가나")]);

        removed.Should().Be(1);
        sut.Entries.Should().ContainSingle().Which.Title.Should().Be("하늘");
        sut.Groups.Select(group => group.Key).Should().Equal("ㅎ");
    }

    [Fact]
    public void ClearEntries_EmptiesCurrentBookSurface()
    {
        var sut = CreateSut(new FakePraiseBookStore(), new PraiseBookIndexEntry("가나", 1));

        var removed = sut.ClearEntries();

        removed.Should().Be(1);
        sut.Entries.Should().BeEmpty();
        sut.Groups.Should().BeEmpty();
        sut.StatusText.Should().Contain("비웠습니다");
    }

    [Fact]
    public void BuildIndexHtml_ReflectsCurrentGroups()
    {
        var sut = CreateSut(new FakePraiseBookStore(), new PraiseBookIndexEntry("가나", 7));

        var html = sut.BuildIndexHtml();

        html.Should().Contain("<h2>ㄱ</h2>");
        html.Should().Contain("가나");
        html.Should().Contain("7");
    }

    [Fact]
    public async Task BuildIndexHtml_AfterOpenBook_UsesBookNameInTitle()
    {
        var store = new FakePraiseBookStore();
        await store.SaveAsync("주일낮", new[] { new PraiseBookIndexEntry("바다", 5) });
        var sut = CreateSut(store, new PraiseBookIndexEntry("가나", 1));
        await sut.OpenBookCommand.ExecuteAsync("주일낮");

        var html = sut.BuildIndexHtml();

        html.Should().Contain("주일낮");
        html.Should().Contain("바다");
    }

    [Fact]
    public async Task DeleteBook_RemovesFromStoreAndSavedBooks()
    {
        var store = new FakePraiseBookStore();
        var sut = CreateSut(store, new PraiseBookIndexEntry("가나", 1));
        await sut.SaveAsCommand.ExecuteAsync("삭제대상");
        sut.SavedBooks.Should().Contain("삭제대상");

        sut.DeleteBookCommand.Execute("삭제대상");

        sut.SavedBooks.Should().NotContain("삭제대상");
        store.ListNames().Should().BeEmpty();
    }
}
