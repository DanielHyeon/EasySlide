using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class BibleViewModelTests
{
    [Fact]
    public async Task LoadAsync_DerivesWorkingFolderAndLoadsFirstVersionBooks()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository
        {
            Versions = [fixture.Kjv],
            Books = [new BibleBook(1, "Genesis")],
        };
        var sut = new BibleViewModel(fixture.Settings, repository);

        await sut.LoadAsync();

        sut.WorkingFolder.Should().Be(Path.GetFullPath(fixture.WorkingFolder));
        sut.Versions.Should().ContainSingle().Which.Name.Should().Be("KJV");
        sut.Books.Should().ContainSingle().Which.Name.Should().Be("Genesis");
        sut.SelectedVersion.Should().Be(fixture.Kjv);
        sut.SelectedBook.Should().Be(new BibleBook(1, "Genesis"));
    }

    [Fact]
    public async Task SearchAsync_UsesSelectedVersionAndShowsResultSummary()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository
        {
            Versions = [fixture.Kjv],
            Books = [new BibleBook(1, "Genesis")],
            SearchResult = new BiblePassageResult(
                "Genesis 1:1 In the beginning",
                [new BibleVerseLocation(0, 1, 1, 1, 0, 28)],
                IsSequential: false,
                WasLimited: false),
        };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.SearchText = "beginning";

        await sut.SearchAsync();

        repository.LastSearchText.Should().Be("beginning");
        sut.PassageText.Should().Be("Genesis 1:1 In the beginning");
        sut.StatusMessage.Should().Be("1개 구절을 찾았습니다.");
    }

    [Fact]
    public async Task BuildSelection_UsesCurrentTextSelectionAndRaisesSelectedEvent()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var selection = new BibleSelection("0;kjv.db;;1;1;1;1;1;", "Genesis 1:1 (KJV)");
        var repository = new FakeBibleRepository
        {
            Versions = [fixture.Kjv],
            Books = [new BibleBook(1, "Genesis")],
            LoadedBook = new BiblePassageResult(
                "1:1 In the beginning",
                [new BibleVerseLocation(0, 1, 1, 1, 0, 20)],
                IsSequential: true,
                WasLimited: false),
            Selection = selection,
        };
        var sut = new BibleViewModel(fixture.Settings, repository);
        BibleSelectionChangedEventArgs? raised = null;
        sut.SelectionChanged += (_, args) => raised = args;
        await sut.LoadAsync();
        await sut.LoadSelectedBookAsync();

        var selected = sut.BuildSelection(selectionStart: 3, selectionLength: 8);

        selected.Should().Be(selection);
        sut.SelectedPassageTitle.Should().Be("Genesis 1:1 (KJV)");
        sut.SelectedPassageId.Should().Be("0;kjv.db;;1;1;1;1;1;");
        raised.Should().NotBeNull();
        raised!.Selection.Should().Be(selection);
    }

    private sealed class FakeBibleRepository : IBibleRepository
    {
        public IReadOnlyList<BibleVersion> Versions { get; init; } = [];

        public IReadOnlyList<BibleBook> Books { get; init; } = [];

        public BiblePassageResult LoadedBook { get; init; } = new("", [], IsSequential: true, WasLimited: false);

        public BiblePassageResult SearchResult { get; init; } = new("", [], IsSequential: false, WasLimited: false);

        public BibleSelection Selection { get; init; } = new("", "");

        public string LastSearchText { get; private set; } = "";

        public IReadOnlyList<BibleVersion> GetVersions(string workingFolder)
            => Versions;

        public IReadOnlyList<BibleBook> GetBooks(BibleVersion version)
            => Books;

        public BiblePassageResult LoadBook(BibleVersion version, int bookNumber, bool showVerses)
            => LoadedBook;

        public BiblePassageResult Search(
            BibleVersion version,
            IReadOnlyList<BibleBook> books,
            string searchText,
            BibleSearchMatchMode matchMode,
            bool showVerses,
            int maxResults = 3000)
        {
            LastSearchText = searchText;
            return SearchResult;
        }

        public BibleSelection BuildSelection(
            BibleVersion version,
            IReadOnlyList<BibleBook> books,
            BiblePassageResult result,
            int selectionStart,
            int selectionLength,
            int maxSequentialSelection = 100,
            int maxAdHocSelection = 100)
            => Selection;

        public BibleSelection ChangeSelectionVersions(
            string currentTitle,
            string currentIdString,
            BibleVersion region1,
            BibleVersion? region2)
            => Selection;
    }

    private sealed class TempBibleSettings : IDisposable
    {
        private TempBibleSettings(string root)
        {
            Root = root;
            WorkingFolder = Path.Combine(root, "Work");
            Directory.CreateDirectory(WorkingFolder);
            Settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "SettingsBackups")));
            Kjv = new BibleVersion(0, "KJV", "King James", "Public domain", "kjv.db", Path.Combine(WorkingFolder, "HolyBibles", "kjv.db"), 1, 80, SupportsPartialWordSearch: false);
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public ISettingsService Settings { get; }

        public BibleVersion Kjv { get; }

        public static TempBibleSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_BibleVm_{Guid.NewGuid():N}"));

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
