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
        sut.CurrentBookName.Should().Be("저녁예배");
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
