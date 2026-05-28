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

    private sealed class AdminDatabaseFixture : IDisposable
    {
        private AdminDatabaseFixture(string root)
        {
            Root = root;
            DatabasePath = Path.Combine(root, "EasiSlidesDb.db");
        }

        public string Root { get; }

        public string DatabasePath { get; }

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
                    LYRICS TEXT,
                    SEQUENCE TEXT,
                    KEY TEXT,
                    CATEGORY TEXT,
                    FOLDERNO INTEGER,
                    SONG_NUMBER INTEGER
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
            string lyrics = "")
        {
            using var connection = Open();
            using var command = new SQLiteCommand(
                """
                INSERT INTO SONG
                    (SONGID, TITLE_1, TITLE_2, CATEGORY, KEY, FOLDERNO, SONG_NUMBER, LYRICS)
                VALUES
                    (@songId, @title, @title2, @category, @key, @folderNo, @songNumber, @lyrics);
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

        private SQLiteConnection Open()
        {
            var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;");
            connection.Open();
            return connection;
        }

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
