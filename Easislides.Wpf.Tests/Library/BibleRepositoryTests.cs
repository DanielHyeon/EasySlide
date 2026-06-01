using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Easislides.Wpf.Library;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class BibleRepositoryTests
{
    [Fact]
    public void GetVersions_ReturnsLegacyBibleFolderRowsOrderedByDisplayOrder()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("Hidden", "hidden.db", "Hidden version", "Hidden copyright", 2, 90, -1),
            ("KJV", "kjv.db", "King James", "Public domain", 0, 500, 1),
            ("NIV", "niv.db", "New International", "Copyright", 4, 120, 2));
        fixture.CreateBible("kjv.db");
        fixture.CreateBible("niv.db");
        var sut = new BibleRepository();

        var versions = sut.GetVersions(fixture.WorkingFolder);

        versions.Select(version => version.Name).Should().Equal("KJV", "NIV");
        versions[0].Index.Should().Be(0);
        versions[0].FileName.Should().Be("kjv.db");
        versions[0].FilePath.Should().Be(Path.Combine(fixture.BibleFolder, "kjv.db"));
        versions[0].SongFolderNo.Should().Be(1);
        versions[0].Size.Should().Be(80);
        versions[1].SongFolderNo.Should().Be(4);
        versions[1].Size.Should().Be(120);
    }

    [Fact]
    public void GetBooks_ReturnsLegacyBookNameRows()
    {
        using var fixture = BibleDatabaseFixture.Create();
        var version = fixture.CreateVersion("kjv.db");
        fixture.CreateBible("kjv.db", includePartialMarker: false);
        var sut = new BibleRepository();

        var books = sut.GetBooks(version);

        books.Should().Contain([
            new BibleBook(1, "Genesis"),
            new BibleBook(2, "Exodus")]);
    }

    [Fact]
    public void LoadBook_ReturnsTrackedTextAndBuildsLegacySelectionId()
    {
        using var fixture = BibleDatabaseFixture.Create();
        var version = fixture.CreateVersion("kjv.db", "KJV");
        fixture.CreateBible("kjv.db", verses:
        [
            (1, 1, 1, "In the beginning God created the heaven and the earth."),
            (1, 1, 2, "And the earth was without form."),
            (1, 1, 3, "And God said, Let there be light."),
        ]);
        var sut = new BibleRepository();
        var books = sut.GetBooks(version);

        var result = sut.LoadBook(version, books[0].Number, showVerses: true);
        var selection = sut.BuildSelection(version, books, result, selectionStart: 5, selectionLength: 64);

        result.Text.Should().Contain("1:1 In the beginning");
        result.Text.Should().Contain("1:2 And the earth");
        result.IsSequential.Should().BeTrue();
        selection.IdString.Should().Be("0;kjv.db;;1;1;1;1;2;");
        selection.Title.Should().Be("Genesis 1:1-2 (KJV)");
    }

    [Fact]
    public void Search_ReturnsAdHocResultsAndHonorsMatchMode()
    {
        using var fixture = BibleDatabaseFixture.Create();
        var version = fixture.CreateVersion("kjv.db", "KJV");
        fixture.CreateBible("kjv.db", verses:
        [
            (1, 1, 1, "In the beginning God created the heaven and the earth."),
            (1, 1, 2, "And the earth was without form."),
            (1, 1, 3, "And God said, Let there be light."),
        ]);
        var sut = new BibleRepository();
        var books = sut.GetBooks(version);

        var allTerms = sut.Search(version, books, "earth form", BibleSearchMatchMode.AllWords, showVerses: true);
        var anyTerm = sut.Search(version, books, "earth light", BibleSearchMatchMode.AnyWord, showVerses: true);
        var exactPhrase = sut.Search(version, books, "let there", BibleSearchMatchMode.ExactPhrase, showVerses: true);

        allTerms.Text.Should().Contain("Genesis 1:2");
        allTerms.Text.Should().NotContain("Genesis 1:1");
        anyTerm.Locations.Should().HaveCount(3);
        exactPhrase.Text.Should().Contain("Genesis 1:3");
        exactPhrase.Text.Should().NotContain("Genesis 1:1");
        anyTerm.IsSequential.Should().BeFalse();
    }

    [Fact]
    public void RenameVersion_UpdatesNameForMatchingFile()
    {
        // 성경 버전 이름 변경 = Biblefolder.NAME 컬럼 UPDATE(본문 파일은 그대로). FILENAME 으로 행을 찾는다.
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),
            ("NIV", "niv.db", "New International", "C", 4, 120, 2));
        fixture.CreateBible("kjv.db");
        fixture.CreateBible("niv.db");
        var sut = new BibleRepository();

        var ok = sut.RenameVersion(fixture.WorkingFolder, "kjv.db", "개역개정");

        ok.Should().BeTrue();
        sut.GetVersions(fixture.WorkingFolder).Select(v => v.Name).Should().Equal("개역개정", "NIV");
    }

    [Fact]
    public void RenameVersion_UnknownFile_ReturnsFalse_AndLeavesNamesUnchanged()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        fixture.CreateBible("kjv.db");
        var sut = new BibleRepository();

        var ok = sut.RenameVersion(fixture.WorkingFolder, "ghost.db", "없는버전");

        ok.Should().BeFalse("대상 파일이 없으면 변경 없음");
        sut.GetVersions(fixture.WorkingFolder).Single().Name.Should().Be("KJV");
    }

    [Fact]
    public void RenameVersion_HiddenVersion_IsNotRenamed()
    {
        // DISPLAYORDER<0(숨김/삭제 예정) 버전은 GetVersions 가 노출하지 않으므로 rename 대상도 아니다
        // (write 집합을 보이는 버전으로 한정 — VM 의 중복 검사 집합과 일치).
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("Hidden", "hidden.db", "임시", "C", 0, 90, -1),
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        fixture.CreateBible("kjv.db");
        var sut = new BibleRepository();

        var ok = sut.RenameVersion(fixture.WorkingFolder, "hidden.db", "바뀐이름");

        ok.Should().BeFalse("숨김 버전은 rename 대상이 아님(DISPLAYORDER>=0 가드)");
    }

    [Fact]
    public void RenameVersion_EmptyNewName_ReturnsFalse()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        fixture.CreateBible("kjv.db");
        var sut = new BibleRepository();

        sut.RenameVersion(fixture.WorkingFolder, "kjv.db", "   ").Should().BeFalse("빈 이름은 거부");
        sut.GetVersions(fixture.WorkingFolder).Single().Name.Should().Be("KJV");
    }

    // ─── 순서 변경(Reorder) — DISPLAYORDER 재기록 ──────────────────────────────
    [Fact]
    public void ReorderVersions_RewritesDisplayOrderToMatchGivenOrder()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),
            ("NIV", "niv.db", "New International", "C", 4, 120, 2),
            ("개역", "krv.db", "개역개정", "C", 0, 80, 3));
        var sut = new BibleRepository();

        var ok = sut.ReorderVersions(fixture.WorkingFolder, new[] { "krv.db", "kjv.db", "niv.db" });

        ok.Should().BeTrue();
        sut.GetVersions(fixture.WorkingFolder).Select(v => v.Name).Should().Equal("개역", "KJV", "NIV");
    }

    [Fact]
    public void ReorderVersions_IgnoresUnknownFilesAndHiddenVersions()
    {
        // 알 수 없는 파일(ghost)·숨김 버전(DISPLAYORDER<0)은 순서 대상이 아니다(보이는 버전만 재정렬).
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("Hidden", "hidden.db", "임시", "C", 0, 90, -1),
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),
            ("NIV", "niv.db", "New International", "C", 4, 120, 2));
        var sut = new BibleRepository();

        var ok = sut.ReorderVersions(fixture.WorkingFolder, new[] { "niv.db", "ghost.db", "kjv.db" });

        ok.Should().BeTrue();
        sut.GetVersions(fixture.WorkingFolder).Select(v => v.Name).Should().Equal("NIV", "KJV");
    }

    [Fact]
    public void ReorderVersions_EmptyOrder_ReturnsFalse()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        var sut = new BibleRepository();

        sut.ReorderVersions(fixture.WorkingFolder, Array.Empty<string>()).Should().BeFalse("순서가 없으면 변경 없음");
    }

    // ─── 삭제(숨김) — 비파괴적, 본문 파일 보존 ─────────────────────────────────
    [Fact]
    public void DeleteVersion_HidesVersion_PreservesFileAndRemovesFromList()
    {
        // 삭제 = DISPLAYORDER<0 으로 숨김(본문 파일·행 보존, 되돌릴 수 있음 — AddVersion 으로 복구). 레거시 "삭제 예정" 의미.
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),
            ("NIV", "niv.db", "New International", "C", 4, 120, 2));
        fixture.CreateBible("kjv.db");
        var sut = new BibleRepository();

        var ok = sut.DeleteVersion(fixture.WorkingFolder, "kjv.db");

        ok.Should().BeTrue();
        sut.GetVersions(fixture.WorkingFolder).Select(v => v.Name).Should().Equal("NIV");
        File.Exists(Path.Combine(fixture.BibleFolder, "kjv.db")).Should().BeTrue("삭제는 숨김일 뿐 본문 파일은 보존");
    }

    [Fact]
    public void DeleteVersion_UnknownOrAlreadyHidden_ReturnsFalse()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),
            ("Hidden", "hidden.db", "임시", "C", 0, 90, -1));
        var sut = new BibleRepository();

        sut.DeleteVersion(fixture.WorkingFolder, "ghost.db").Should().BeFalse("없는 파일은 거부");
        sut.DeleteVersion(fixture.WorkingFolder, "hidden.db").Should().BeFalse("이미 숨김인 버전은 대상 아님");
        sut.GetVersions(fixture.WorkingFolder).Single().Name.Should().Be("KJV");
    }

    // ─── 추가 — 숨김 복구 + HolyBibles 신규 파일 등록 ──────────────────────────
    [Fact]
    public void GetAddableVersions_ReturnsHiddenAndNewFiles_ExcludesVisible()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),     // 보임 → 제외
            ("임시개역", "hidden.db", "임시", "C", 0, 80, -1));    // 숨김 → 포함(재추가)
        fixture.CreateBible("kjv.db");
        fixture.CreateBible("hidden.db");
        fixture.CreateBible("new.db");                              // Biblefolder 행 없음 → 포함(신규)
        var sut = new BibleRepository();

        var addable = sut.GetAddableVersions(fixture.WorkingFolder);

        addable.Select(a => a.FileName).Should().BeEquivalentTo(new[] { "hidden.db", "new.db" });
        addable.Single(a => a.FileName == "hidden.db").IsHidden.Should().BeTrue();
        addable.Single(a => a.FileName == "hidden.db").SuggestedName.Should().Be("임시개역", "숨김 행의 기존 이름을 제안");
        addable.Single(a => a.FileName == "new.db").IsHidden.Should().BeFalse();
    }

    [Fact]
    public void AddVersion_UnhidesHiddenVersion_AppearsAtEndWithName()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 500, 1),
            ("임시", "hidden.db", "임시", "C", 0, 80, -1));
        fixture.CreateBible("kjv.db");
        fixture.CreateBible("hidden.db");
        var sut = new BibleRepository();

        var ok = sut.AddVersion(fixture.WorkingFolder, "hidden.db", "개역개정");

        ok.Should().BeTrue();
        sut.GetVersions(fixture.WorkingFolder).Select(v => v.Name).Should().Equal("KJV", "개역개정");
    }

    [Fact]
    public void AddVersion_NewFile_InsertsRowAtEnd()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        fixture.CreateBible("kjv.db");
        fixture.CreateBible("niv.db");
        var sut = new BibleRepository();

        var ok = sut.AddVersion(fixture.WorkingFolder, "niv.db", "NIV");

        ok.Should().BeTrue();
        var versions = sut.GetVersions(fixture.WorkingFolder);
        versions.Select(v => v.Name).Should().Equal("KJV", "NIV");
        versions.Single(v => v.Name == "NIV").FileName.Should().Be("niv.db");
    }

    [Fact]
    public void AddVersion_MissingFileOrEmptyName_ReturnsFalse()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        fixture.CreateBible("kjv.db");
        var sut = new BibleRepository();

        sut.AddVersion(fixture.WorkingFolder, "ghost.db", "유령").Should().BeFalse("HolyBibles 에 파일이 없으면 거부");
        sut.AddVersion(fixture.WorkingFolder, "kjv.db", "   ").Should().BeFalse("빈 이름은 거부");
    }

    [Fact]
    public void AddVersion_AlreadyVisibleFile_ReturnsFalse_AndLeavesRowUnchanged()
    {
        // 이미 보이는 버전(DISPLAYORDER>=0)을 다시 추가하면 거부해야 한다 — 기존 이름/순서를 덮어쓰지 않는다.
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(("KJV", "kjv.db", "King James", "PD", 0, 500, 1));
        fixture.CreateBible("kjv.db");
        var sut = new BibleRepository();

        var ok = sut.AddVersion(fixture.WorkingFolder, "kjv.db", "다른이름");

        ok.Should().BeFalse("이미 보이는 버전은 추가 대상이 아님");
        sut.GetVersions(fixture.WorkingFolder).Single().Name.Should().Be("KJV", "기존 이름 보존");
    }

    [Fact]
    public void ChangeSelectionVersions_PreservesPassagesAndUpdatesTitleSuffix()
    {
        using var fixture = BibleDatabaseFixture.Create();
        var kjv = fixture.CreateVersion("kjv.db", "KJV");
        var niv = fixture.CreateVersion("niv.db", "NIV", displayOrder: 2);
        var sut = new BibleRepository();

        var changed = sut.ChangeSelectionVersions(
            "Genesis 1:1 (Old)",
            "0;old.db;;1;1;1;1;1;",
            kjv,
            niv);

        changed.IdString.Should().Be("0;kjv.db;niv.db;1;1;1;1;1;");
        changed.Title.Should().Be("Genesis 1:1 (KJV/NIV)");
    }

    [Fact]
    public void ExpandSelection_SingleLanguage_ReturnsVerseBodyPaginatedByVerse()
    {
        // 예전엔 성경 항목이 제목만 송출했다 — 이제 IdString 을 실제 구절 본문으로 확장해 회중 화면에 보인다.
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateVersion("kjv.db", "KJV");
        fixture.CreateBible("kjv.db", verses:
        [
            (1, 1, 1, "In the beginning God created the heaven and the earth."),
            (1, 1, 2, "And the earth was without form."),
            (1, 1, 3, "And God said, Let there be light."),
        ]);
        var sut = new BibleRepository();

        // 창세기 1:1-2 (절 번호 표시 on).
        var body = sut.ExpandSelection(fixture.WorkingFolder, "0;kjv.db;;1;1;1;1;2;", showVerses: true);

        body.Should().Be("1:1 In the beginning God created the heaven and the earth.\n\n1:2 And the earth was without form.");
        body.Should().NotContain("1:3"); // 범위 밖 절은 빠진다.
        body.Should().NotContain("[region 2]"); // 단일 언어 — 보조 밴드 없음.
    }

    [Fact]
    public void ExpandSelection_HidesVerseNumbersWhenShowVersesOff()
    {
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateVersion("kjv.db", "KJV");
        fixture.CreateBible("kjv.db", verses: [(1, 1, 1, "In the beginning.")]);
        var sut = new BibleRepository();

        var body = sut.ExpandSelection(fixture.WorkingFolder, "0;kjv.db;;1;1;1;1;1;", showVerses: false);

        body.Should().Be("In the beginning."); // 절 번호 없이 본문만.
    }

    [Fact]
    public void ExpandSelection_DualLanguage_PairsRegionsPerVerse()
    {
        // 이중 언어: 주 버전 본문 아래 [region 2] 로 보조 버전 본문을 함께 실어 한/영 동시 송출.
        using var fixture = BibleDatabaseFixture.Create();
        fixture.CreateBibleList(
            ("KJV", "kjv.db", "King James", "PD", 0, 80, 1),
            ("KRV", "krv.db", "개역", "PD", 0, 80, 2));
        fixture.CreateBible("kjv.db", verses:
        [
            (43, 3, 16, "For God so loved the world."),
            (43, 3, 17, "For God sent not his Son."),
        ]);
        fixture.CreateBible("krv.db", verses:
        [
            (43, 3, 16, "하나님이 세상을 이처럼 사랑하사."),
            (43, 3, 17, "하나님이 그 아들을 보내신 것은."),
        ]);
        var sut = new BibleRepository();

        var body = sut.ExpandSelection(fixture.WorkingFolder, "0;kjv.db;krv.db;43;3;16;3;17;", showVerses: true);

        var page0 = LyricsDisplayFormatter.GetRegionPage(body, 0);
        page0.Region1.Should().Be("3:16 For God so loved the world.");
        page0.Region2.Should().Be("하나님이 세상을 이처럼 사랑하사."); // 보조 언어엔 절 번호 중복 안 붙임.
        var page1 = LyricsDisplayFormatter.GetRegionPage(body, 1);
        page1.Region1.Should().Be("3:17 For God sent not his Son.");
        page1.Region2.Should().Be("하나님이 그 아들을 보내신 것은.");
        LyricsDisplayFormatter.HasRegion2(body).Should().BeTrue();
    }

    [Fact]
    public void ExpandSelection_MissingVersionFile_ReturnsEmptyForGracefulFallback()
    {
        using var fixture = BibleDatabaseFixture.Create();
        var sut = new BibleRepository();

        // 버전 파일이 없으면 빈 본문 — 출력은 제목만(우아한 폴백, 예외 없음).
        sut.ExpandSelection(fixture.WorkingFolder, "0;missing.db;;1;1;1;1;1;", showVerses: true)
            .Should().BeEmpty();
    }

    [Fact]
    public void ExpandSelection_InvalidIdString_ReturnsEmpty()
    {
        using var fixture = BibleDatabaseFixture.Create();
        var sut = new BibleRepository();

        sut.ExpandSelection(fixture.WorkingFolder, "쓰레기", showVerses: true).Should().BeEmpty();
    }

    private sealed class BibleDatabaseFixture : IDisposable
    {
        private BibleDatabaseFixture(string root)
        {
            Root = root;
            WorkingFolder = Path.Combine(root, "Work");
            BibleFolder = Path.Combine(WorkingFolder, "HolyBibles");
            BibleListPath = Path.Combine(WorkingFolder, "Admin", "Database", "EsBiblesList.db");
            Directory.CreateDirectory(BibleFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(BibleListPath)!);
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public string BibleFolder { get; }

        public string BibleListPath { get; }

        public static BibleDatabaseFixture Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_Bibles_{Guid.NewGuid():N}"));

        public BibleVersion CreateVersion(
            string fileName,
            string name = "KJV",
            string description = "King James",
            string copyright = "Public domain",
            int songFolderNo = 1,
            int size = 80,
            int displayOrder = 1)
        {
            CreateBibleList((name, fileName, description, copyright, songFolderNo, size, displayOrder));
            return new BibleVersion(0, name, description, copyright, fileName, Path.Combine(BibleFolder, fileName), songFolderNo, size, SupportsPartialWordSearch: false);
        }

        public void CreateBibleList(params (string Name, string FileName, string Description, string Copyright, int SongFolder, int Size, int DisplayOrder)[] rows)
        {
            using var connection = new SQLiteConnection($"Data Source={BibleListPath}");
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS Biblefolder (
                        NAME TEXT,
                        FILENAME TEXT,
                        DESCRIPTION TEXT,
                        COPYRIGHT TEXT,
                        SONGFOLDER INTEGER,
                        SIZE INTEGER,
                        DISPLAYORDER INTEGER
                    );
                    DELETE FROM Biblefolder;
                    """;
                command.ExecuteNonQuery();
            }

            foreach (var row in rows)
            {
                using var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO Biblefolder (NAME, FILENAME, DESCRIPTION, COPYRIGHT, SONGFOLDER, SIZE, DISPLAYORDER)
                    VALUES (@name, @fileName, @description, @copyright, @songFolder, @size, @displayOrder);
                    """;
                command.Parameters.AddWithValue("@name", row.Name);
                command.Parameters.AddWithValue("@fileName", row.FileName);
                command.Parameters.AddWithValue("@description", row.Description);
                command.Parameters.AddWithValue("@copyright", row.Copyright);
                command.Parameters.AddWithValue("@songFolder", row.SongFolder);
                command.Parameters.AddWithValue("@size", row.Size);
                command.Parameters.AddWithValue("@displayOrder", row.DisplayOrder);
                command.ExecuteNonQuery();
            }
        }

        public void CreateBible(
            string fileName,
            bool includePartialMarker = false,
            IReadOnlyList<(int Book, int Chapter, int Verse, string Text)>? verses = null)
        {
            using var connection = new SQLiteConnection($"Data Source={Path.Combine(BibleFolder, fileName)}");
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE Bible (
                        book INTEGER,
                        chapter INTEGER,
                        verse INTEGER,
                        bibletext TEXT
                    );
                    """;
                command.ExecuteNonQuery();
            }

            InsertBibleRow(connection, 0, 10, 1, "Genesis");
            InsertBibleRow(connection, 0, 10, 2, "Exodus");
            InsertBibleRow(connection, 0, 0, 0, "Fixture description");
            InsertBibleRow(connection, 0, 0, 1, "Fixture name");
            InsertBibleRow(connection, 0, 0, 3, "Fixture copyright");
            if (includePartialMarker)
            {
                InsertBibleRow(connection, 0, 0, 20, "partial");
            }

            foreach (var verse in verses ?? [])
            {
                InsertBibleRow(connection, verse.Book, verse.Chapter, verse.Verse, verse.Text);
            }
        }

        private static void InsertBibleRow(SQLiteConnection connection, int book, int chapter, int verse, string text)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Bible (book, chapter, verse, bibletext) VALUES (@book, @chapter, @verse, @text);";
            command.Parameters.AddWithValue("@book", book);
            command.Parameters.AddWithValue("@chapter", chapter);
            command.Parameters.AddWithValue("@verse", verse);
            command.Parameters.AddWithValue("@text", text);
            command.ExecuteNonQuery();
        }

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
