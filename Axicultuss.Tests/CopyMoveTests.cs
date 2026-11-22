using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

public class CopyMoveTests
{
    [Fact]
    public void MoveRecordsExistingSongsAndRemovesRowsOnlyAfterCommit()
    {
        WithTempFolder(dir =>
        {
            using var db = CreateDatabase(dir);
            using var list = CreateList("D1", "D2", "D99");
            list.Items[1].Selected = false;

            var ids = Gf.ReFileSelectedSongs(list, 1, 8, false, db.ConnectionString);
            Assert.Equal(1, Assert.Single(ids));
            Assert.Equal(["D2", "D99"], list.Items.Cast<ListViewItem>().Select(row => row.SubItems[1].Text).ToArray());
            Assert.Equal("1|8|original", Scalar(db, "SELECT OldFolder || '|' || FolderNo || '|' || LastModified FROM SONG WHERE SongID=1"));
            Assert.Equal("1", Scalar(db, "SELECT FolderNo FROM SONG WHERE SongID=2"));

            list.Items[0].Selected = true;
            ids = Gf.ReFileSelectedSongs(list, 1, 0, true, db.ConnectionString);
            Assert.Equal(2, Assert.Single(ids));
            Assert.Equal("0", Scalar(db, "SELECT FolderNo FROM SONG WHERE SongID=2"));
            Assert.NotEqual("original", Scalar(db, "SELECT LastModified FROM SONG WHERE SongID=2"));
        });
    }

    [Fact]
    public void MoveAndDeleteRecordEachSongsOwnFolderNotTheFolderBoxOne()
    {
        // A search lists songs of several folders; the folder box (currentFolder) names only one of them.
        WithTempFolder(dir =>
        {
            using var db = CreateDatabase(dir);
            Execute(db, "UPDATE SONG SET FolderNo=5 WHERE SongID=2");
            using var list = CreateList("D1", "D2");

            Gf.ReFileSelectedSongs(list, 1, 0, true, db.ConnectionString);

            Assert.Equal("1|0", Scalar(db, "SELECT OldFolder || '|' || FolderNo FROM SONG WHERE SongID=1"));
            Assert.Equal("5|0", Scalar(db, "SELECT OldFolder || '|' || FolderNo FROM SONG WHERE SongID=2"));
        });
    }

    [Fact]
    public void RolledBackMoveKeepsBothSelectedRowsAndReturnsNoIds()
    {
        WithTempFolder(dir =>
        {
            using var db = CreateDatabase(dir);
            Execute(db, "CREATE TRIGGER reject_update BEFORE UPDATE ON SONG WHEN NEW.SongID=2 BEGIN SELECT RAISE(ABORT,'test'); END");
            using var list = CreateList("D1", "D2");

            Assert.Empty(Gf.ReFileSelectedSongs(list, 1, 8, false, db.ConnectionString));
            Assert.Equal(2, list.SelectedItems.Count);
            Assert.Equal("2", Scalar(db, "SELECT COUNT(*) FROM SONG WHERE FolderNo=1 AND LastModified='original'"));
        });
    }

    [Fact]
    public void SongCopiesReportNewIdsAndInfoScreensRoundTripWithoutReusingAFailedItem()
    {
        WithTempFolder(dir =>
        {
            using var db = CreateDatabase(dir);
            using var songs = CreateList("D1", "D99", "D2");
            songs.Items[2].Selected = false;
            var (ids, files) = FrmMain.CopySelectedSongs(songs, 4, "", db.ConnectionString);
            int newId = Assert.Single(ids);
            Assert.True(newId > 2);
            Assert.Empty(files);
            Assert.Equal("찬양|가사", Scalar(db, $"SELECT Title_1 || '|' || Lyrics FROM SONG WHERE FolderNo=4 AND SongID={newId}"));

            File.WriteAllText(Path.Combine(dir, "찬양.esi"), "existing");
            var exported = FrmMain.CopySelectedSongs(songs, -1, dir, db.ConnectionString);
            Assert.Empty(exported.SongIds);
            string fileKey = Assert.Single(exported.FileKeys);
            Assert.Equal("I" + Path.Combine(dir, "찬양 - Copy (1).esi"), fileKey);
            Assert.Equal("existing", File.ReadAllText(Path.Combine(dir, "찬양.esi")));

            string compact = Path.Combine(dir, "제목 없음.esi");
            File.WriteAllText(compact, "<Axicultus><Item><Title2>Second</Title2><Contents>다음</Contents></Item></Axicultus>");
            string legacy = Path.Combine(dir, "이전 형식.esi");
            File.WriteAllText(legacy, "[\u0001=320]옛 가사");
            string invalid = Path.Combine(dir, "invalid.esi");
            File.WriteAllText(invalid, "<Axicultus><Item><Title1>partial</Title1>");
            using var imports = CreateList(fileKey, "I" + invalid, "I" + compact, "I" + Path.Combine(dir, "missing.esi"), "I" + legacy);

            var imported = FrmMain.CopyInfoScreensToSongFolder(imports, 5, db.ConnectionString);
            Assert.Equal(3, imported.Count);
            Assert.Equal("3", Scalar(db, "SELECT COUNT(*) FROM SONG WHERE FolderNo=5"));
            Assert.Equal("가사", Scalar(db, "SELECT Lyrics FROM SONG WHERE FolderNo=5 AND Title_1='찬양'"));
            Assert.Equal("Second|다음|", Scalar(db, "SELECT Title_2 || '|' || Lyrics || '|' || SETTINGS FROM SONG WHERE FolderNo=5 AND Title_1='제목 없음'"));
            Assert.Equal("옛 가사", Scalar(db, "SELECT Lyrics FROM SONG WHERE FolderNo=5 AND Title_1='이전 형식'"));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExternalTransfersSelectTheExactDestinationEvenWhenDisplayNamesMatch(bool move)
    {
        WithTempFolder(dir =>
        {
            string sourcePath = Path.Combine(dir, "stage.pptx");
            string target = Directory.CreateDirectory(Path.Combine(dir, "target")).FullName;
            File.WriteAllText(sourcePath, "slides");
            File.WriteAllText(Path.Combine(target, "stage.pptx"), "existing");
            using var source = CreateList("P" + sourcePath, "P" + Path.Combine(dir, "missing.pptx"));
            var keys = FrmMain.TransferFiles(FrmMain.SelectedItemKeys(source), target, move ? Gf.MoveExternalFile : Gf.CopyExternalFile);

            string transferred = Path.Combine(target, "stage - Copy (1).pptx");
            Assert.Equal("P" + transferred, Assert.Single(keys));
            Assert.Equal("slides", File.ReadAllText(transferred));
            Assert.Equal("existing", File.ReadAllText(Path.Combine(target, "stage.pptx")));
            Assert.Equal(!move, File.Exists(sourcePath));
            using var destination = CreateList("P" + Path.ChangeExtension(transferred, ".ppt"), "P" + transferred,
                "P" + Path.Combine(target, "sub", Path.GetFileName(transferred)));
            FrmMain.SelectTransferredItems(destination, keys.Contains);
            Assert.Equal(1, Assert.Single(destination.SelectedIndices.Cast<int>()));
        });
    }

    [Fact]
    public void FailedInfoScreenWriteLeavesNoPartialFileOrSelectionKey()
    {
        WithTempFolder(dir =>
        {
            using var db = CreateDatabase(dir);
            Execute(db, "UPDATE SONG SET FolderNo=999 WHERE SongID=2");
            using var songs = CreateList("D1", "D2");
            string output = Directory.CreateDirectory(Path.Combine(dir, "exports")).FullName;
            var result = FrmMain.CopySelectedSongs(songs, -1, output, db.ConnectionString);
            string saved = Assert.Single(Directory.GetFiles(output));
            Assert.Equal("찬양.esi", Path.GetFileName(saved));
            Assert.Equal("I" + saved, Assert.Single(result.FileKeys));
        });
    }

    [Fact]
    public void TransferResultsCanHoldMoreThanThirtyThousandItems()
    {
        const int count = 30_001;
        var keys = FrmMain.TransferFiles(Enumerable.Range(1, count).Select(i => $"I{i}.esi"), "out",
            (file, folder) => Path.Combine(folder, file));
        Assert.Equal(count, keys.Count);
        Assert.Contains("I" + Path.Combine("out", $"{count}.esi"), keys);
    }

    private static ListView CreateList(params string[] keys)
    {
        var list = new ListView();
        _ = list.Handle;
        foreach (string key in keys)
            list.Items.Add(new ListViewItem(["same title", key])).Selected = true;
        return list;
    }

    private static SQLiteConnection CreateDatabase(string dir)
    {
        var db = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = Path.Combine(dir, "test.db"), Pooling = false }.ConnectionString);
        db.Open();
        Execute(db, """
            CREATE TABLE SONG (SongID INTEGER PRIMARY KEY, FolderNo INTEGER, OldFolder INTEGER, LastModified TEXT,
                Title_1 TEXT, Title_2 TEXT, song_number INTEGER, Lyrics TEXT, Sequence TEXT, writer TEXT, copyright TEXT,
                capo INTEGER, Timing TEXT, [Key] TEXT, msc TEXT, CATEGORY TEXT, LICENCE_ADMIN1 TEXT, LICENCE_ADMIN2 TEXT,
                BOOK_REFERENCE TEXT, USER_REFERENCE TEXT, SETTINGS TEXT, FORMATDATA TEXT, CJK_WordCount TEXT, CJK_StrokeCount TEXT);
            INSERT INTO SONG (FolderNo, Title_1, Lyrics, SETTINGS, LastModified) VALUES
                (1, '찬양', '가사', 'original settings', 'original'), (1, '다른 곡', '본문', '', 'original');
            """);
        return db;
    }

    private static void Execute(SQLiteConnection db, string sql)
    {
        using var command = new SQLiteCommand(sql, db);
        command.ExecuteNonQuery();
    }

    private static string Scalar(SQLiteConnection db, string sql)
    {
        using var command = new SQLiteCommand(sql, db);
        return command.ExecuteScalar()?.ToString();
    }

    private static void WithTempFolder(Action<string> check)
    {
        Exception failure = null;
        var thread = new Thread(() => failure = Record.Exception(() =>
        {
            DirectoryInfo dir = Directory.CreateTempSubdirectory("axicultus-copy-test-");
            try { check(dir.FullName); }
            finally { dir.Delete(recursive: true); }
        })) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)));
        if (failure != null)
            ExceptionDispatchInfo.Throw(failure);
    }
}
