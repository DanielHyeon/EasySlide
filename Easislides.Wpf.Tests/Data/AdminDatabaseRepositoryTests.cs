using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Data;

public class AdminDatabaseRepositoryTests
{
    [Fact]
    public async Task AnalyzeSchemaAsync_WhenLegacyAdminDatabaseExists_ReturnsTablesColumnsAndCompatibility()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        var sut = new AdminDatabaseRepository();

        var inventory = await sut.AnalyzeSchemaAsync(fixture.DatabasePath);

        inventory.Succeeded.Should().BeTrue();
        inventory.SchemaVersion.Should().Be(4);
        inventory.Tables.Select(table => table.Name).Should().Contain(["FOLDER", "SONG", "LICENCE"]);
        inventory.Columns["FOLDER"].Select(column => column.Name).Should().Contain(["FolderNo", "Name", "Use"]);
        inventory.Columns["SONG"].Select(column => column.Name).Should().Contain(["SONGID", "TITLE_1", "FOLDERNO", "LYRICS"]);
        inventory.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeSchemaAsync_WithBundledAdminDatabase_ReturnsCompatibleSongSchema()
    {
        var databasePath = FindBundledAdminDatabase();
        var sut = new AdminDatabaseRepository();

        var inventory = await sut.AnalyzeSchemaAsync(databasePath);

        inventory.Succeeded.Should().BeTrue();
        inventory.Tables.Select(table => table.Name).Should().Contain(["FOLDER", "SONG", "LICENCE"]);
        inventory.Columns["FOLDER"].Select(column => column.Name).Should().Contain(["FolderNo", "Name", "Use"]);
        inventory.Columns["SONG"].Select(column => column.Name).Should().Contain(["SONGID", "TITLE_1", "FOLDERNO", "LYRICS"]);
    }

    [Fact]
    public async Task AnalyzeSchemaAsync_WhenRequiredTableOrColumnMissing_ReturnsCompatibilityIssues()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateDatabase(
            version: 1,
            """
            CREATE TABLE FOLDER (
                FolderNo INTEGER PRIMARY KEY
            );
            """);
        var sut = new AdminDatabaseRepository();

        var inventory = await sut.AnalyzeSchemaAsync(fixture.DatabasePath);

        inventory.Succeeded.Should().BeFalse();
        inventory.Issues.Should().Contain(issue => issue.Kind == AdminDatabaseIssueKind.MissingTable && issue.TableName == "SONG");
        inventory.Issues.Should().Contain(issue => issue.Kind == AdminDatabaseIssueKind.MissingColumn && issue.TableName == "FOLDER" && issue.ColumnName == "Name");
    }

    [Fact]
    public async Task GetSongFoldersAsync_ReturnsFoldersWithSongCountsOrderedByFolderNo()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(2, "Evening", use: "False");
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 2);
        fixture.InsertSong(11, "Benediction", folderNo: 1, songNumber: 1);
        fixture.InsertSong(12, "Night Prayer", folderNo: 2, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var folders = await sut.GetSongFoldersAsync(fixture.DatabasePath);

        folders.Select(folder => folder.FolderNo).Should().Equal(1, 2);
        folders[0].Name.Should().Be("Morning");
        folders[0].IsEnabled.Should().BeTrue();
        folders[0].SongCount.Should().Be(2);
        folders[1].Name.Should().Be("Evening");
        folders[1].IsEnabled.Should().BeFalse();
        folders[1].SongCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSongsAsync_WhenFolderFilterProvided_ReturnsOrderedSongSummaries()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(11, "Second", folderNo: 1, songNumber: 2, title2: "Alt", category: "Praise", key: "G", lyrics: "Line 2");
        fixture.InsertSong(10, "First", folderNo: 1, songNumber: 1, lyrics: "Line 1");
        fixture.InsertSong(12, "Other Folder", folderNo: 2, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var songs = await sut.GetSongsAsync(fixture.DatabasePath, folderNo: 1);

        songs.Select(song => song.SongId).Should().Equal(10, 11);
        songs[0].Title.Should().Be("First");
        songs[0].Lyrics.Should().Be("Line 1");
        songs[1].Title.Should().Be("Second");
        songs[1].AlternateTitle.Should().Be("Alt");
        songs[1].Category.Should().Be("Praise");
        songs[1].Key.Should().Be("G");
    }

    [Fact]
    public async Task SaveFolderAsync_CreatesBackupAndUpsertsFolderInTransaction()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongFolderWriteModel(1, "Updated Morning", IsEnabled: false));

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.SaveFolder);
        report.BackupPath.Should().NotBeNull();
        File.Exists(report.BackupPath).Should().BeTrue();
        report.AffectedFolderNos.Should().Equal(1);
        fixture.ReadFolderName(1).Should().Be("Updated Morning");
        fixture.ReadFolderUse(1).Should().Be("False");
        AdminDatabaseFixture.ReadFolderName(report.BackupPath!, 1).Should().Be("Morning");
    }

    [Fact]
    public async Task SaveSongAsync_WhenNewSong_CreatesBackupInsertsLegacyFieldsAndReturnsSongId()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveSongAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongWriteModel(
                SongId: null,
                Title: new string('A', 120),
                AlternateTitle: "Alt",
                FolderNo: 1,
                SongNumber: 7,
                Lyrics: "Line 1",
                Sequence: "V1 C",
                Writer: "Writer",
                Copyright: "Copyright",
                Capo: 2,
                Timing: "4/4",
                Key: "G",
                Notations: "G C D",
                Category: "Praise",
                LicenceAdmin1: "Admin1",
                LicenceAdmin2: "Admin2",
                BookReference: "Book",
                UserReference: "User",
                Settings: "Settings",
                FormatData: "Format"));

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.SaveSong);
        report.BackupPath.Should().NotBeNull();
        report.AffectedSongIds.Should().ContainSingle().Which.Should().BeGreaterThan(0);
        var songId = report.AffectedSongIds[0];
        fixture.ReadSongText(songId, "TITLE_1").Should().HaveLength(100).And.StartWith("AAAA");
        fixture.ReadSongText(songId, "TITLE_2").Should().Be("Alt");
        fixture.ReadSongInt(songId, "FOLDERNO").Should().Be(1);
        fixture.ReadSongText(songId, "CJK_WordCount").Should().Be("000");
        fixture.ReadSongText(songId, "CJK_StrokeCount").Should().StartWith("000AAAA");
        fixture.ReadSongText(songId, "FORMATDATA").Should().Be("Format");
    }

    [Fact]
    public async Task SaveSongAsync_WhenExistingSong_UpdatesEditableFieldsWithoutResettingOldFolder()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(20, "Original", folderNo: 1, songNumber: 1, oldFolder: 7);
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveSongAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongWriteModel(
                SongId: 20,
                Title: "Updated",
                AlternateTitle: "",
                FolderNo: 2,
                SongNumber: 4,
                Lyrics: "New lyrics"));

        report.Succeeded.Should().BeTrue();
        report.AffectedSongIds.Should().Equal(20);
        fixture.ReadSongText(20, "TITLE_1").Should().Be("Updated");
        fixture.ReadSongInt(20, "FOLDERNO").Should().Be(2);
        fixture.ReadSongInt(20, "SONG_NUMBER").Should().Be(4);
        fixture.ReadSongInt(20, "OldFolder").Should().Be(7);
        AdminDatabaseFixture.ReadSongText(report.BackupPath!, 20, "TITLE_1").Should().Be("Original");
    }

    [Fact]
    public async Task MoveSongsAsync_WhenAnySongIsMissing_RollsBackAndRestoresBackup()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.MoveSongsAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [
                new SongMoveRequest(10, OldFolderNo: 1, NewFolderNo: 2),
                new SongMoveRequest(999, OldFolderNo: 1, NewFolderNo: 2),
            ]);

        report.Succeeded.Should().BeFalse();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.MoveSongs);
        report.BackupPath.Should().NotBeNull();
        File.Exists(report.BackupPath).Should().BeTrue();
        report.Issues.Should().Contain(issue => issue.Kind == AdminDatabaseWriteIssueKind.NotFound);
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1);
        fixture.ReadSongInt(10, "OldFolder").Should().Be(0);
        AdminDatabaseFixture.ReadSongInt(report.BackupPath!, 10, "FOLDERNO").Should().Be(1);
    }

    private sealed class AdminDatabaseFixture : IDisposable
    {
        private AdminDatabaseFixture(string root)
        {
            Root = root;
            DatabasePath = Path.Combine(root, "EasiSlidesDb.db");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string DatabasePath { get; }

        public string BackupRoot { get; }

        public static AdminDatabaseFixture Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_AdminDb_{Guid.NewGuid():N}"));

        public void CreateLegacySchema()
        {
            CreateDatabase(
                version: 4,
                """
                CREATE TABLE FOLDER (
                    FolderNo INTEGER PRIMARY KEY,
                    Name TEXT,
                    Use TEXT
                );
                """,
                """
                CREATE TABLE SONG (
                    SONGID INTEGER PRIMARY KEY,
                    TITLE_1 TEXT,
                    TITLE_2 TEXT,
                    WRITER TEXT,
                    COPYRIGHT TEXT,
                    CJK_WordCount TEXT,
                    CJK_StrokeCount TEXT,
                    LYRICS TEXT,
                    SEQUENCE TEXT,
                    KEY TEXT,
                    CAPO INTEGER,
                    TIMING TEXT,
                    MSC TEXT,
                    CATEGORY TEXT,
                    FOLDERNO INTEGER,
                    SONG_NUMBER INTEGER,
                    LICENCE_ADMIN1 TEXT,
                    LICENCE_ADMIN2 TEXT,
                    BOOK_REFERENCE TEXT,
                    USER_REFERENCE TEXT,
                    SETTINGS TEXT,
                    FORMATDATA TEXT,
                    LastModified TEXT,
                    OldFolder INTEGER DEFAULT 0
                );
                """,
                """
                CREATE TABLE LICENCE (
                    ADMINISTRATOR TEXT,
                    REF TEXT
                );
                """);
        }

        public void CreateDatabase(int version, params string[] commands)
        {
            Directory.CreateDirectory(Root);
            using var connection = Open();
            Execute(connection, $"PRAGMA user_version = {version};");
            foreach (var command in commands)
            {
                Execute(connection, command);
            }
        }

        public void InsertFolder(int folderNo, string name, string use)
        {
            using var connection = Open();
            using var command = new SQLiteCommand(
                "INSERT INTO FOLDER (FolderNo, Name, Use) VALUES (@folderNo, @name, @use);",
                connection);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@use", use);
            command.ExecuteNonQuery();
        }

        public void InsertSong(
            int songId,
            string title,
            int folderNo,
            int songNumber,
            string title2 = "",
            string category = "",
            string key = "",
            string lyrics = "",
            int oldFolder = 0)
        {
            using var connection = Open();
            using var command = new SQLiteCommand(
                """
                INSERT INTO SONG
                    (SONGID, TITLE_1, TITLE_2, CATEGORY, KEY, FOLDERNO, SONG_NUMBER, LYRICS, OldFolder)
                VALUES
                    (@songId, @title, @title2, @category, @key, @folderNo, @songNumber, @lyrics, @oldFolder);
                """,
                connection);
            command.Parameters.AddWithValue("@songId", songId);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@title2", title2);
            command.Parameters.AddWithValue("@category", category);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            command.Parameters.AddWithValue("@songNumber", songNumber);
            command.Parameters.AddWithValue("@lyrics", lyrics);
            command.Parameters.AddWithValue("@oldFolder", oldFolder);
            command.ExecuteNonQuery();
        }

        public string ReadFolderName(int folderNo) => ReadFolderText(DatabasePath, folderNo, "Name");

        public string ReadFolderUse(int folderNo) => ReadFolderText(DatabasePath, folderNo, "Use");

        public static string ReadFolderName(string databasePath, int folderNo) => ReadFolderText(databasePath, folderNo, "Name");

        public string ReadSongText(int songId, string columnName) => ReadSongText(DatabasePath, songId, columnName);

        public static string ReadSongText(string databasePath, int songId, string columnName)
        {
            using var connection = Open(databasePath);
            using var command = new SQLiteCommand($"SELECT {QuoteIdentifier(columnName)} FROM SONG WHERE SONGID = @songId;", connection);
            command.Parameters.AddWithValue("@songId", songId);
            return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
        }

        public int ReadSongInt(int songId, string columnName) => ReadSongInt(DatabasePath, songId, columnName);

        public static int ReadSongInt(string databasePath, int songId, string columnName)
        {
            using var connection = Open(databasePath);
            using var command = new SQLiteCommand($"SELECT {QuoteIdentifier(columnName)} FROM SONG WHERE SONGID = @songId;", connection);
            command.Parameters.AddWithValue("@songId", songId);
            return Convert.ToInt32(command.ExecuteScalar());
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

        private SQLiteConnection Open()
            => Open(DatabasePath);

        private static SQLiteConnection Open(string databasePath)
        {
            var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;");
            connection.Open();
            return connection;
        }

        private static string ReadFolderText(string databasePath, int folderNo, string columnName)
        {
            using var connection = Open(databasePath);
            using var command = new SQLiteCommand($"SELECT {QuoteIdentifier(columnName)} FROM FOLDER WHERE FolderNo = @folderNo;", connection);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
        }

        private static string QuoteIdentifier(string identifier)
            => "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

        private static void Execute(SQLiteConnection connection, string sql)
        {
            using var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
    }

    private static string FindBundledAdminDatabase()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "AdminDB", "Database", "EasiSlidesDb.db");
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Bundled AdminDB fixture was not found.");
    }
}
