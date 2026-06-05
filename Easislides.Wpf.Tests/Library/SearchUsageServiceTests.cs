using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class SearchUsageServiceTests
{
    [Fact]
    public async Task SearchSongsAsync_WhenFieldsFoldersAndFiltersMatch_ReturnsSongMetadata()
    {
        using var fixture = new SearchUsageFixture();
        fixture.InsertSong(
            songId: 10,
            title: "Amazing Grace",
            alternateTitle: "Grace Alt",
            folderNo: 1,
            songNumber: 7,
            lyrics: "Saved a soul",
            key: "G",
            timing: "Slow",
            notations: "G C",
            writer: "Newton",
            copyright: "Public",
            bookReference: "Ps 23",
            userReference: "CCLI 1",
            lastModified: new DateTime(2026, 5, 20));
        fixture.InsertSong(
            songId: 11,
            title: "Evening Song",
            folderNo: 2,
            lyrics: "Amazing line",
            key: "D",
            timing: "Fast",
            lastModified: new DateTime(2026, 4, 1));

        var sut = fixture.CreateService();

        var results = await sut.SearchSongsAsync(new SongSearchRequest(
            fixture.AdminDatabasePath,
            "grace",
            [1],
            SongSearchFields.Title | SongSearchFields.Lyrics | SongSearchFields.BookReference | SongSearchFields.UserReference,
            Key: "G",
            Timing: "Slow",
            NotationsOnly: true,
            ModifiedFrom: new DateTime(2026, 5, 1),
            ModifiedTo: new DateTime(2026, 5, 31)));

        results.Should().ContainSingle();
        var result = results[0];
        result.SongId.Should().Be(10);
        result.FolderName.Should().Be("Morning");
        result.MatchingFields.Should().Contain("Title");
        result.Snippet.Should().Contain("Amazing Grace");
    }

    [Fact]
    public async Task SearchSongsAsync_WhenSongNumberFieldSelected_MatchesExactNumber()
    {
        using var fixture = new SearchUsageFixture();
        fixture.InsertSong(songId: 10, title: "Alpha", folderNo: 1, songNumber: 7);
        fixture.InsertSong(songId: 11, title: "Beta", folderNo: 1, songNumber: 70);

        var sut = fixture.CreateService();

        var results = await sut.SearchSongsAsync(new SongSearchRequest(
            fixture.AdminDatabasePath,
            "7",
            [1],
            SongSearchFields.SongNumber));

        results.Select(result => result.Title).Should().Equal("Alpha");
    }

    [Fact]
    public async Task LookupTitlesAsync_ReturnsTitleCandidatesWithOptionalReferences()
    {
        using var fixture = new SearchUsageFixture();
        fixture.InsertSong(songId: 10, title: "Amazing Grace", alternateTitle: "Grace Alt", folderNo: 1, bookReference: "Ps 23", userReference: "CCLI");
        fixture.InsertSong(songId: 11, title: "Amazing Love", folderNo: 2, bookReference: "Jn 3", userReference: "Admin");

        var sut = fixture.CreateService();

        var candidates = await sut.LookupTitlesAsync(fixture.AdminDatabasePath, "Amazing");

        candidates.Select(candidate => candidate.Title).Should().Equal("Amazing Grace", "Amazing Love");
        candidates[0].FolderName.Should().Be("Morning");
        candidates[0].BookReference.Should().Be("Ps 23");
        candidates[0].UserReference.Should().Be("CCLI");
    }

    [Fact]
    public async Task GetUsageAsync_LoadsSessionsDetailsAndOccurrenceSummary()
    {
        using var fixture = new SearchUsageFixture();
        fixture.InsertUsage(new DateTime(2026, 5, 1), "Sunday AM", "Amazing Grace", 7, 10, "A1", "");
        fixture.InsertUsage(new DateTime(2026, 5, 8), "Sunday PM", "Amazing Grace", 7, 10, "", "A2");
        fixture.InsertUsage(new DateTime(2026, 5, 15), "Sunday AM", "Blessing", 2, 11, "A1", "A2");

        var sut = fixture.CreateService();

        var report = await sut.GetUsageAsync(new UsageRequest(
            fixture.UsageDatabasePath,
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 31),
            "Sunday AM"));

        report.Succeeded.Should().BeTrue();
        report.Sessions.Should().Equal("", "Sunday AM", "Sunday PM");
        report.Records.Select(record => record.SongTitle).Should().Equal("Amazing Grace", "Blessing");
        report.Summary.Select(item => (item.SongTitle, item.Occurrences)).Should().Equal(("Amazing Grace", 1), ("Blessing", 1));
    }

    [Fact]
    public async Task AddUsageRecordsAsync_WritesLegacyUsageRows()
    {
        using var fixture = new SearchUsageFixture();
        var sut = fixture.CreateService();
        var longSession = new string('S', 60);

        var add = await sut.AddUsageRecordsAsync(new UsageAddRequest(
            fixture.UsageDatabasePath,
            [
                new UsageAddRecord(
                    new DateTime(2026, 6, 5),
                    longSession,
                    "♪Amazing Grace",
                    7,
                    10,
                    "Admin1",
                    "Admin2"),
            ]));
        var report = await sut.GetUsageAsync(new UsageRequest(
            fixture.UsageDatabasePath,
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30),
            ""));

        add.Succeeded.Should().BeTrue();
        add.AddedCount.Should().Be(1);
        var record = report.Records.Should().ContainSingle().Subject;
        record.WorshipDate.Should().Be(new DateTime(2026, 6, 5));
        record.WorshipList.Should().HaveLength(50, "FrmMain stores only the first 50 SessionList characters");
        record.SongTitle.Should().Be("Amazing Grace", "FrmMain strips the music symbol before storing the title");
        record.SongNumber.Should().Be(7);
        record.SongId.Should().Be(10);
        record.Admin1.Should().Be("Admin1");
        record.Admin2.Should().Be("Admin2");
    }

    [Fact]
    public async Task DeleteUsageRecordsAsync_RemovesRequestedRows()
    {
        using var fixture = new SearchUsageFixture();
        var firstId = fixture.InsertUsage(new DateTime(2026, 5, 1), "Sunday AM", "Amazing Grace", 7, 10, "A1", "");
        fixture.InsertUsage(new DateTime(2026, 5, 8), "Sunday AM", "Blessing", 2, 11, "A1", "A2");

        var sut = fixture.CreateService();

        var delete = await sut.DeleteUsageRecordsAsync(fixture.UsageDatabasePath, [firstId]);
        var report = await sut.GetUsageAsync(new UsageRequest(
            fixture.UsageDatabasePath,
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 31),
            ""));

        delete.DeletedCount.Should().Be(1);
        report.Records.Should().ContainSingle().Which.SongTitle.Should().Be("Blessing");
    }

    [Fact]
    public async Task ExportUsageReportAsync_WritesLegacyRtfSections()
    {
        using var fixture = new SearchUsageFixture();
        fixture.InsertUsage(new DateTime(2026, 5, 1), "Sunday AM", "Amazing Grace", 7, 10, "A1", "A2");
        var sut = fixture.CreateService();
        var report = await sut.GetUsageAsync(new UsageRequest(
            fixture.UsageDatabasePath,
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 31),
            ""));
        var outputPath = Path.Combine(fixture.Root, "usage.rtf");

        var export = await sut.ExportUsageReportAsync(report, outputPath);

        export.Succeeded.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
        File.ReadAllText(outputPath).Should().Contain("Usage Details").And.Contain("Occurrences").And.Contain("Amazing Grace");
    }

    private sealed class SearchUsageFixture : IDisposable
    {
        public SearchUsageFixture()
        {
            Root = Path.Combine(Path.GetTempPath(), $"EasiSlides_SearchUsage_{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            AdminDatabasePath = Path.Combine(Root, "EasiSlidesDb.db");
            UsageDatabasePath = Path.Combine(Root, "EsUsage.db");
            CreateAdminDatabase();
            CreateUsageDatabase();
        }

        public string Root { get; }

        public string AdminDatabasePath { get; }

        public string UsageDatabasePath { get; }

        public SearchUsageService CreateService()
            => new(new AdminDatabaseRepository());

        public void InsertSong(
            int songId,
            string title,
            int folderNo,
            string alternateTitle = "",
            int songNumber = 1,
            string lyrics = "",
            string key = "",
            string timing = "",
            string notations = "",
            string writer = "",
            string copyright = "",
            string bookReference = "",
            string userReference = "",
            DateTime? lastModified = null)
        {
            using var connection = Open(AdminDatabasePath);
            using var command = new SQLiteCommand(
                """
                INSERT INTO SONG (
                    SONGID, TITLE_1, TITLE_2, FOLDERNO, SONG_NUMBER, CATEGORY, "KEY", LYRICS,
                    SEQUENCE, WRITER, COPYRIGHT, CAPO, TIMING, MSC, LICENCE_ADMIN1, LICENCE_ADMIN2,
                    BOOK_REFERENCE, USER_REFERENCE, SETTINGS, FORMATDATA, LastModified, CJK_StrokeCount
                )
                VALUES (
                    @songId, @title, @alternateTitle, @folderNo, @songNumber, '', @key, @lyrics,
                    '', @writer, @copyright, 0, @timing, @notations, '', '',
                    @bookReference, @userReference, '', '', @lastModified, @stroke
                );
                """,
                connection);
            command.Parameters.AddWithValue("@songId", songId);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@alternateTitle", alternateTitle);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            command.Parameters.AddWithValue("@songNumber", songNumber);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@lyrics", lyrics);
            command.Parameters.AddWithValue("@writer", writer);
            command.Parameters.AddWithValue("@copyright", copyright);
            command.Parameters.AddWithValue("@timing", timing);
            command.Parameters.AddWithValue("@notations", notations);
            command.Parameters.AddWithValue("@bookReference", bookReference);
            command.Parameters.AddWithValue("@userReference", userReference);
            command.Parameters.AddWithValue("@lastModified", (lastModified ?? DateTime.Today).ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@stroke", songId);
            command.ExecuteNonQuery();
        }

        public long InsertUsage(DateTime date, string worshipList, string songTitle, int songNumber, int songId, string admin1, string admin2)
        {
            using var connection = Open(UsageDatabasePath);
            using var command = new SQLiteCommand(
                """
                INSERT INTO USAGE (WORSHIP_DATE, WORSHIP_LIST, SONG_TITLE, SONG_NUMBER, SONG_ID, ADMIN_1, ADMIN_2)
                VALUES (@date, @worshipList, @songTitle, @songNumber, @songId, @admin1, @admin2);
                SELECT last_insert_rowid();
                """,
                connection);
            command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@worshipList", worshipList);
            command.Parameters.AddWithValue("@songTitle", songTitle);
            command.Parameters.AddWithValue("@songNumber", songNumber);
            command.Parameters.AddWithValue("@songId", songId);
            command.Parameters.AddWithValue("@admin1", admin1);
            command.Parameters.AddWithValue("@admin2", admin2);
            return (long)command.ExecuteScalar()!;
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

        private void CreateAdminDatabase()
        {
            using var connection = Open(AdminDatabasePath);
            Execute(
                connection,
                """
                CREATE TABLE FOLDER (FolderNo INTEGER PRIMARY KEY, Name TEXT, Use INTEGER);
                CREATE TABLE SONG (
                    SONGID INTEGER PRIMARY KEY,
                    TITLE_1 TEXT,
                    TITLE_2 TEXT,
                    FOLDERNO INTEGER,
                    SONG_NUMBER INTEGER,
                    CATEGORY TEXT,
                    "KEY" TEXT,
                    LYRICS TEXT,
                    SEQUENCE TEXT,
                    WRITER TEXT,
                    COPYRIGHT TEXT,
                    CAPO INTEGER,
                    TIMING TEXT,
                    MSC TEXT,
                    LICENCE_ADMIN1 TEXT,
                    LICENCE_ADMIN2 TEXT,
                    BOOK_REFERENCE TEXT,
                    USER_REFERENCE TEXT,
                    SETTINGS TEXT,
                    FORMATDATA TEXT,
                    LastModified TEXT,
                    CJK_StrokeCount INTEGER
                );
                INSERT INTO FOLDER (FolderNo, Name, Use) VALUES (1, 'Morning', 1);
                INSERT INTO FOLDER (FolderNo, Name, Use) VALUES (2, 'Evening', 1);
                """);
        }

        private void CreateUsageDatabase()
        {
            using var connection = Open(UsageDatabasePath);
            Execute(
                connection,
                """
                CREATE TABLE USAGE (
                    REC_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    WORSHIP_DATE TEXT,
                    WORSHIP_LIST TEXT,
                    SONG_TITLE TEXT,
                    SONG_NUMBER INTEGER,
                    SONG_ID INTEGER,
                    ADMIN_1 TEXT,
                    ADMIN_2 TEXT
                );
                """);
        }

        private static SQLiteConnection Open(string path)
        {
            var connection = new SQLiteConnection($"Data Source={path};Version=3;");
            connection.Open();
            return connection;
        }

        private static void Execute(SQLiteConnection connection, string sql)
        {
            using var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
    }
}
