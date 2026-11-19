using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Easislides.Global;
using Easislides.SQLite;
using Xunit;

namespace Easislides.Tests;

[Collection("WinForms Bible globals")]
public sealed class DatabaseQueryTests
{
    private const string Title = "은혜 O'Brien \"Reprise\"";

    private static SQLiteConnection OpenSongs()
    {
        var connection = new SQLiteConnection("Data Source=:memory:");
        connection.Open();
        using var create = new SQLiteCommand("""
            CREATE TABLE SONG (SongID INTEGER PRIMARY KEY, FolderNo INTEGER, Title_1 TEXT, Title_2 TEXT,
                song_number INTEGER, LICENCE_ADMIN1 TEXT, LICENCE_ADMIN2 TEXT, [Key] TEXT, Timing TEXT, LastModified TEXT);
            """, connection);
        create.ExecuteNonQuery();
        return connection;
    }

    private static void AddSong(SQLiteConnection connection, int id, int folder, string primary, string alternate = "")
    {
        using var insert = DbController.CreateCommand(connection,
            "INSERT INTO SONG VALUES (@id, @folder, @primary, @alternate, 0, '', '', @key, @timing, @date)",
            ("@id", id), ("@folder", folder), ("@primary", primary), ("@alternate", alternate),
            ("@key", "D'm"), ("@timing", "3/4"), ("@date", "2026-06-01"));
        insert.ExecuteNonQuery();
    }

    [Fact]
    public void ImportedAndSavedTitlesKeepQuotesAndMatchTheIntendedRow()
    {
        using var connection = OpenSongs();
        AddSong(connection, 5, 2, Title);
        AddSong(connection, 7, 3, Title);
        Assert.Equal(5, SongQueries.FindImportSongId(connection, 2, Title));
        Assert.Null(SongQueries.FindImportSongId(connection, 2, "Title_1"));
        Assert.Equal(7L, SongQueries.FindListEntry(connection, false, "", Title, 3)["SongID"]);
        Assert.Equal(5L, SongQueries.FindListEntry(connection, true, "5", "", 0)["SongID"]);
        Assert.Null(SongQueries.FindListEntry(connection, true, "D'5", "", 0));
    }

    [Fact]
    public void SavedSearchBindsTextFiltersDatesAndPreservesWildcardMatchingWhenReordered()
    {
        var savedFolders = Gf.Folders.Select(f => (f.Use, f.FindSongs)).ToArray();
        try
        {
            Gf.Folders[2].Use = 1;
            using var connection = OpenSongs();
            AddSong(connection, 1, 2, Title);
            AddSong(connection, 3, 2, Title + " II");
            AddSong(connection, 5, 0, Title);
            var options = new ItemSearchOptions
            {
                InTitle = true, InContents = false, InSongNumber = false, InBookRef = false, InUserRef = false,
                InLicAdmin = false, InWriter = false, InCopyright = false, Key = "D'm", Timing = "3/4",
                UseDates = true, DateFrom = new DateTime(2026, 6, 1), DateTo = new DateTime(2026, 6, 1)
            };
            var query = Gf.BuildItemSearchQuery("O'Brien%Reprise", options, null, activeFoldersOnly: true);
            Assert.DoesNotContain("brien", query.Text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("D'm", query.Text);
            using var result = DbController.GetDataTable(connection, query.Text + " ORDER BY SongID DESC", query.Parameters);
            Assert.Equal(new long[] { 3, 1 }, result.Rows.Cast<DataRow>().Select(r => (long)r["SongID"]));
        }
        finally
        {
            for (int i = 0; i < savedFolders.Length; i++)
                (Gf.Folders[i].Use, Gf.Folders[i].FindSongs) = savedFolders[i];
        }
    }

    [Fact]
    public void MergeReturnsEveryPairUsingTheChosenColumnsAndExistingLikeRules()
    {
        using var connection = OpenSongs();
        AddSong(connection, 1, 1, "First", "O'Brien%");
        AddSong(connection, 2, 1, "Second", "O'Brien%");
        AddSong(connection, 3, 1, "Empty", "");
        AddSong(connection, 4, 2, "O'Brien");
        AddSong(connection, 5, 2, "O'BRIEN reprise");
        AddSong(connection, 6, 3, "O'Brien");
        AddSong(connection, 7, 1, "Whitespace", "\t");
        AddSong(connection, 8, 2, "\t");
        int selects = 0;
        connection.Trace += (_, trace) =>
        {
            if (trace.Statement.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                selects++;
        };
        var pairs = SongQueries.FindMergePairs(connection, 1, false, 2, true);
        Assert.Equal(new[] { (1L, 4L), (1L, 5L), (2L, 4L), (2L, 5L) },
            pairs.Select(r => (r.SourceId, r.TargetId)).Order());
        Assert.Equal(1, selects);
    }

    [Fact]
    public void FileAndTableNamesRetainPunctuationAndReaderOwnsItsConnectionUntilClosed()
    {
        var directory = Directory.CreateTempSubdirectory();
        string path = Path.Combine(directory.FullName, "Sunday; 음악.db");
        const string name = "Music \"A\"; archive";
        try
        {
            string settings = DbController.ConnectionStringForFile(path);
            string identifier = DbController.QuoteIdentifier(name);
            using (var connection = DbController.GetDbConnection(settings))
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"CREATE TABLE {identifier} (title TEXT)";
                command.ExecuteNonQuery();
                Assert.Equal(new[] { "title" }, DbController.GetColumnNames(connection, name));
                Assert.Empty(DbController.GetColumnNames(connection, "absent"));
                using var insert = DbController.CreateCommand(connection, $"INSERT INTO {identifier} VALUES (@title)", ("@title", Title));
                insert.ExecuteNonQuery();
            }
            var (owner, reader) = DbController.GetDataReader(settings, $"SELECT title FROM {identifier} WHERE title=@title", ("@title", Title));
            using (owner)
            using (reader)
            {
                Assert.True(reader.Read());
                Assert.Equal(Title, reader.GetString(0));
                Assert.False(reader.Read());
            }
            Assert.True(File.Exists(path));
            string missing = Path.Combine(directory.FullName, "missing.db");
            using var unopened = DbController.GetDbConnection(DbController.ConnectionStringForFile(missing, readOnly: true));
            Assert.Null(unopened);
            Assert.False(File.Exists(missing));
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    [Fact]
    public void RecoveryCommitsBeforeRemovingItemsAndKeepsAlreadyActiveSongsInTheirFolder()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            string original = Gf.ConnectStringMainDB;
            string path = Path.GetTempFileName();
            try
            {
                Gf.ConnectStringMainDB = DbController.ConnectionStringForFile(path);
                using var connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
                using var command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE SONG (SongID INTEGER PRIMARY KEY, FolderNo INTEGER, OldFolder INTEGER, LastModified TEXT);
                    INSERT INTO SONG VALUES (1,0,3,''), (2,0,4,''), (3,7,0,'');
                    CREATE TRIGGER keep_song BEFORE UPDATE ON SONG WHEN OLD.SongID=2
                    BEGIN SELECT RAISE(ABORT, 'retained song'); END;
                    """;
                command.ExecuteNonQuery();
                using var ownedList = new System.Windows.Forms.ListView { CheckBoxes = true };
                var list = ownedList;
                for (int id = 1; id <= 3; id++)
                    list.Items.Add(new System.Windows.Forms.ListViewItem(new[] { "Title", "Folder", "", id.ToString(), (id + 2).ToString() }) { Checked = true });
                Assert.Equal(3, Gf.ReFileSelectedSongs(ref list));
                command.CommandText = "SELECT COUNT(*) FROM SONG WHERE FolderNo=0";
                Assert.Equal(2L, command.ExecuteScalar());
                command.CommandText = "DROP TRIGGER keep_song";
                command.ExecuteNonQuery();
                Assert.Equal(1, Gf.ReFileSelectedSongs(ref list));
                command.CommandText = "SELECT COUNT(*) FROM SONG WHERE (SongID=1 AND FolderNo=3) OR (SongID=2 AND FolderNo=4) OR (SongID=3 AND FolderNo=7)";
                Assert.Equal(3L, command.ExecuteScalar());
            }
            finally
            {
                Gf.ConnectStringMainDB = original;
                File.Delete(path);
            }
        });
    }

    [Fact]
    public void ExportResetRecognizesTheSameDatabaseDespiteDifferentConnectionOptions()
    {
        string original = Gf.ConnectStringMainDB;
        string path = Path.GetTempFileName();
        try
        {
            Gf.ConnectStringMainDB = DbController.ConnectionStringForFile(path);
            using var connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Folder (FolderNo INTEGER); INSERT INTO Folder VALUES (1)";
            command.ExecuteNonQuery();
            string alternate = new SQLiteConnectionStringBuilder { DataSource = path, Version = 3 }.ConnectionString;
            Assert.False(Gf.DeleteAllFolders(alternate));
            command.CommandText = "SELECT COUNT(*) FROM Folder";
            Assert.Equal(1L, command.ExecuteScalar());
        }
        finally
        {
            Gf.ConnectStringMainDB = original;
            File.Delete(path);
        }
    }

    [Fact]
    public void SelectedUsageDeletionRollsBackWhenALaterRecordIsLockedByABusinessRule()
    {
        using var connection = OpenSongs();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE USAGE (REC_ID INTEGER PRIMARY KEY);
            INSERT INTO USAGE VALUES (1), (2), (3);
            CREATE TRIGGER keep_record BEFORE DELETE ON USAGE WHEN OLD.REC_ID=2
            BEGIN SELECT RAISE(ABORT, 'retained usage record'); END;
            """;
        command.ExecuteNonQuery();
        Assert.Throws<SQLiteException>(() => FrmUsages.DeleteUsageRecords(connection, [1, 2]));
        command.CommandText = "SELECT COUNT(*) FROM USAGE";
        Assert.Equal(3L, command.ExecuteScalar());
        FrmUsages.DeleteUsageRecords(connection, [1, 3]);
        command.CommandText = "SELECT REC_ID FROM USAGE";
        Assert.Equal(2L, command.ExecuteScalar());
    }
}
