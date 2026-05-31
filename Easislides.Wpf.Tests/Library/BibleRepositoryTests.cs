using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Easislides.Wpf.Library;
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
