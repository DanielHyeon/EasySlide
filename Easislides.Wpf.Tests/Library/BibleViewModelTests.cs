using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    [Fact]
    public async Task BuildSelection_WhenRegionTwoPreviewEnabled_UpdatesPreviewSelectionAndRaisesPreview()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var baseSelection = new BibleSelection("0;kjv.db;;1;1;1;1;1;", "Genesis 1:1 (KJV)");
        var repository = new FakeBibleRepository
        {
            Versions = [fixture.Kjv, fixture.Niv],
            Books = [new BibleBook(1, "Genesis")],
            LoadedBook = new BiblePassageResult(
                "1:1 In the beginning",
                [new BibleVerseLocation(0, 1, 1, 1, 0, 20)],
                IsSequential: true,
                WasLimited: false),
            Selection = baseSelection,
        };
        var sut = new BibleViewModel(fixture.Settings, repository);
        BibleSelectionChangedEventArgs? raised = null;
        sut.SelectionChanged += (_, args) => raised = args;
        await sut.LoadAsync();
        await sut.LoadSelectedBookAsync();
        sut.UseRegion2Preview = true;

        var selected = sut.BuildSelection(selectionStart: 3, selectionLength: 8);

        repository.LastRegion1.Should().Be(fixture.Kjv);
        repository.LastRegion2.Should().Be(fixture.Niv);
        selected.IdString.Should().Be("0;kjv.db;niv.db;1;1;1;1;1;");
        selected.Title.Should().Be("Genesis 1:1 (KJV/NIV)");
        sut.SelectedSelection.Should().Be(selected);
        sut.PreviewPassageId.Should().Be(selected.IdString);
        sut.PreviewPassageTitle.Should().Be(selected.Title);
        sut.SelectedPassageId.Should().Be(selected.IdString);
        sut.SelectedPassageTitle.Should().Be(selected.Title);
        sut.PreviewRegionSummary.Should().Be("KJV / NIV");
        raised.Should().NotBeNull();
        raised!.Selection.Should().Be(selected);
    }

    [Fact]
    public async Task RenameVersion_RenamesAndReloads_PreservingSelection()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv, fixture.Niv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.RenameVersion(fixture.Kjv, "개역개정");

        ok.Should().BeTrue();
        repository.LastRenamedFileName.Should().Be("kjv.db");
        repository.LastRenamedNewName.Should().Be("개역개정");
        sut.Versions.Should().Contain(v => v.Name == "개역개정").And.NotContain(v => v.Name == "KJV");
        sut.SelectedVersion!.FileName.Should().Be("kjv.db", "이름 변경 후 같은 버전 선택 유지");
    }

    [Fact]
    public async Task RenameVersion_DuplicateName_IsRejected_NoRepoCall()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv, fixture.Niv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.RenameVersion(fixture.Kjv, "NIV");

        ok.Should().BeFalse("이미 있는 버전 이름으로는 거부");
        repository.LastRenamedFileName.Should().BeNull("중복이면 저장소 호출 안 함");
        sut.Versions.Should().Contain(v => v.Name == "KJV");
    }

    [Fact]
    public async Task RenameVersion_UnchangedName_IsNoOpSuccess()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.RenameVersion(fixture.Kjv, "KJV");

        ok.Should().BeTrue("이름이 같으면 변경 없이 성공으로 처리");
        repository.LastRenamedFileName.Should().BeNull("변경 없으면 저장소 호출 안 함");
    }

    private sealed class FakeBibleRepository : IBibleRepository
    {
        public IReadOnlyList<BibleVersion> Versions { get; init; } = [];

        public IReadOnlyList<BibleBook> Books { get; init; } = [];

        public BiblePassageResult LoadedBook { get; init; } = new("", [], IsSequential: true, WasLimited: false);

        public BiblePassageResult SearchResult { get; init; } = new("", [], IsSequential: false, WasLimited: false);

        public BibleSelection Selection { get; init; } = new("", "");

        public string LastSearchText { get; private set; } = "";

        public BibleVersion? LastRegion1 { get; private set; }

        public BibleVersion? LastRegion2 { get; private set; }

        public string? LastRenamedFileName { get; private set; }

        public string? LastRenamedNewName { get; private set; }

        // 이름 변경이 재로드에 반영되도록 가변 작업 목록을 둔다(실제 저장소처럼).
        private List<BibleVersion>? _live;

        public IReadOnlyList<BibleVersion> GetVersions(string workingFolder)
            => _live ??= Versions.ToList();

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
        {
            LastRegion1 = region1;
            LastRegion2 = region2;
            if (string.IsNullOrWhiteSpace(currentIdString))
            {
                return new BibleSelection("", "");
            }

            var parts = currentIdString.Split(';');
            var tail = string.Join(';', parts[3..]);
            var baseTitle = currentTitle.Split(" (", StringSplitOptions.None)[0];
            var suffix = region2 is null ? region1.Name : $"{region1.Name}/{region2.Name}";
            return new BibleSelection(
                $"{parts[0]};{region1.FileName};{region2?.FileName ?? ""};{tail}",
                $"{baseTitle} ({suffix})");
        }

        public bool RenameVersion(string workingFolder, string fileName, string newName)
        {
            LastRenamedFileName = fileName;
            LastRenamedNewName = newName;
            _live ??= Versions.ToList();
            var index = _live.FindIndex(v => v.FileName == fileName);
            if (index < 0 || string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }

            _live[index] = _live[index] with { Name = newName.Trim() };
            return true;
        }
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
            Niv = new BibleVersion(1, "NIV", "New International", "Licensed", "niv.db", Path.Combine(WorkingFolder, "HolyBibles", "niv.db"), 2, 80, SupportsPartialWordSearch: true);
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public ISettingsService Settings { get; }

        public BibleVersion Kjv { get; }

        public BibleVersion Niv { get; }

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
