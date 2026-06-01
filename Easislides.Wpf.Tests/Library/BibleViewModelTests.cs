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
    public async Task JumpToReference_ValidRange_SelectsVerseSpanAndRaisesEvent()
    {
        // typed-reference: "Genesis 1:1-2" 를 입력하면 책을 풀고 그 절 범위를 선택해 추가용 BibleSelection 을 돌려준다.
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var selection = new BibleSelection("0;kjv.db;;1;1;1;1;2;", "Genesis 1:1-2 (KJV)");
        var repository = new FakeBibleRepository
        {
            Versions = [fixture.Kjv],
            Books = [new BibleBook(1, "Genesis"), new BibleBook(2, "Exodus")],
            LoadedBook = new BiblePassageResult(
                "1:1 In the beginning\n1:2 And the earth",
                [
                    new BibleVerseLocation(0, 1, 1, 1, Start: 0, Length: 20),
                    new BibleVerseLocation(0, 1, 1, 2, Start: 21, Length: 16),
                ],
                IsSequential: true,
                WasLimited: false),
            Selection = selection,
        };
        var sut = new BibleViewModel(fixture.Settings, repository);
        BibleSelectionChangedEventArgs? raised = null;
        sut.SelectionChanged += (_, args) => raised = args;
        await sut.LoadAsync();
        sut.TypedReference = "Genesis 1:1-2";

        var result = sut.JumpToReference();

        result.Should().Be(selection);
        sut.SelectedBook.Should().Be(new BibleBook(1, "Genesis"), "참조의 책으로 선택을 옮긴다");
        repository.LastBuildStart.Should().Be(0, "시작 절(1:1)의 본문 시작 오프셋");
        repository.LastBuildLength.Should().Be(37, "끝 절(1:2) 끝까지 = 21+16");
        sut.LastReferenceStart.Should().Be(0);
        sut.LastReferenceLength.Should().Be(37);
        raised.Should().NotBeNull();
        raised!.Selection.Should().Be(selection);
    }

    [Fact]
    public async Task JumpToReference_InvalidFormat_SetsValidationAndReturnsEmpty()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv], Books = [new BibleBook(1, "Genesis")] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.TypedReference = "이건구절이아님";

        var result = sut.JumpToReference();

        result.IdString.Should().BeEmpty();
        sut.ValidationMessage.Should().Contain("형식");
    }

    [Fact]
    public async Task JumpToReference_UnknownBook_SetsValidationAndReturnsEmpty()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv], Books = [new BibleBook(1, "Genesis")] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.TypedReference = "Mars 1:1";

        var result = sut.JumpToReference();

        result.IdString.Should().BeEmpty();
        sut.ValidationMessage.Should().Contain("책을 찾을 수 없습니다");
    }

    [Fact]
    public async Task JumpToReference_VerseNotInBook_SetsValidationAndReturnsEmpty()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository
        {
            Versions = [fixture.Kjv],
            Books = [new BibleBook(1, "Genesis")],
            LoadedBook = new BiblePassageResult(
                "1:1 In the beginning",
                [new BibleVerseLocation(0, 1, 1, 1, Start: 0, Length: 20)],
                IsSequential: true,
                WasLimited: false),
        };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.TypedReference = "Genesis 9:9"; // 본문에 없는 절.

        var result = sut.JumpToReference();

        result.IdString.Should().BeEmpty();
        sut.ValidationMessage.Should().Contain("구절을 찾을 수 없습니다");
    }

    [Fact]
    public void JumpToReference_NoVersionSelected_SetsValidationAndReturnsEmpty()
    {
        using var fixture = TempBibleSettings.Create();
        var repository = new FakeBibleRepository(); // LoadAsync 안 함 → SelectedVersion null.
        var sut = new BibleViewModel(fixture.Settings, repository);
        sut.TypedReference = "Genesis 1:1";

        var result = sut.JumpToReference();

        result.IdString.Should().BeEmpty();
        sut.ValidationMessage.Should().Contain("버전을 선택");
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

    [Fact]
    public async Task MoveVersionDown_ReordersAndPreservesSelection()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv, fixture.Niv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.MoveVersionDown(fixture.Kjv);

        ok.Should().BeTrue();
        sut.Versions.Select(v => v.FileName).Should().Equal("niv.db", "kjv.db");
        sut.SelectedVersion!.FileName.Should().Be("kjv.db", "이동 후 같은 버전 선택 유지");
    }

    [Fact]
    public async Task MoveVersionUp_AtTop_ReturnsFalse_NoChange()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv, fixture.Niv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.MoveVersionUp(fixture.Kjv);

        ok.Should().BeFalse("맨 위에서는 더 올릴 수 없음");
        sut.Versions.Select(v => v.FileName).Should().Equal("kjv.db", "niv.db");
    }

    [Fact]
    public async Task DeleteVersion_RemovesAndSelectsNeighbor()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv, fixture.Niv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.DeleteVersion(fixture.Kjv);

        ok.Should().BeTrue();
        sut.Versions.Select(v => v.Name).Should().Equal("NIV");
        sut.SelectedVersion!.FileName.Should().Be("niv.db", "삭제 후 인접 버전 선택");
    }

    [Fact]
    public async Task AddVersion_AddsAndSelectsNewVersion()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv] };
        repository.Addable.Add(new BibleAddableVersion("niv.db", "NIV", IsHidden: false));
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.AddVersion("niv.db", "NIV");

        ok.Should().BeTrue();
        sut.Versions.Select(v => v.Name).Should().Equal("KJV", "NIV");
        sut.SelectedVersion!.FileName.Should().Be("niv.db", "추가 후 새 버전 선택");
    }

    [Fact]
    public async Task AddVersion_DuplicateName_IsRejected()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv] };
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        var ok = sut.AddVersion("kjv2.db", "KJV");

        ok.Should().BeFalse("보이는 버전과 이름이 겹치면 거부");
        sut.Versions.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAddableVersions_ReturnsRepositoryCandidates()
    {
        using var fixture = TempBibleSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var repository = new FakeBibleRepository { Versions = [fixture.Kjv] };
        repository.Addable.Add(new BibleAddableVersion("niv.db", "NIV", IsHidden: false));
        repository.Addable.Add(new BibleAddableVersion("hidden.db", "임시", IsHidden: true));
        var sut = new BibleViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        sut.GetAddableVersions().Select(a => a.FileName).Should().BeEquivalentTo("niv.db", "hidden.db");
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

        // typed-reference 점프가 계산해 넘긴 본문 선택 범위(검증용).
        public int LastBuildStart { get; private set; } = -1;

        public int LastBuildLength { get; private set; } = -1;

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
        {
            LastBuildStart = selectionStart;
            LastBuildLength = selectionLength;
            return Selection;
        }

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

        // 추가 후보(관리 UI 가 GetAddableVersions 로 받는 목록) — 테스트에서 설정.
        public List<BibleAddableVersion> Addable { get; } = new();

        public bool ReorderVersions(string workingFolder, IReadOnlyList<string> orderedFileNames)
        {
            _live ??= Versions.ToList();
            if (orderedFileNames is null || orderedFileNames.Count == 0)
            {
                return false;
            }

            // 주어진 파일 순서대로 보이는 버전을 재배열(매칭 안 되는 파일은 무시).
            var reordered = orderedFileNames
                .Select(file => _live.FirstOrDefault(v => v.FileName == file))
                .Where(v => v is not null)
                .Select(v => v!)
                .ToList();
            if (reordered.Count == 0)
            {
                return false;
            }

            // 순서에 없던 보이는 버전은 뒤에 그대로 붙인다.
            reordered.AddRange(_live.Where(v => !reordered.Contains(v)));
            _live.Clear();
            _live.AddRange(reordered);
            return true;
        }

        public bool DeleteVersion(string workingFolder, string fileName)
        {
            _live ??= Versions.ToList();
            var index = _live.FindIndex(v => v.FileName == fileName);
            if (index < 0)
            {
                return false;
            }

            _live.RemoveAt(index);
            return true;
        }

        public IReadOnlyList<BibleAddableVersion> GetAddableVersions(string workingFolder)
            => Addable;

        public bool AddVersion(string workingFolder, string fileName, string name)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            _live ??= Versions.ToList();
            _live.Add(new BibleVersion(_live.Count, name.Trim(), "", "", fileName, fileName, 1, 80, SupportsPartialWordSearch: false));
            Addable.RemoveAll(a => a.FileName == fileName);
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
