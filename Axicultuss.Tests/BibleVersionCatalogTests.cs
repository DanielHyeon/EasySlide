using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Localization;
using Xunit;

namespace Axicultus.Tests;

[CollectionDefinition("WinForms Bible globals", DisableParallelization = true)]
public sealed class BibleGlobalsCollection { }

[Collection("WinForms Bible globals")]
public sealed class BibleVersionCatalogTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(251)]
    public void TabsAndFileLookupsUseTheSameVisibleOrder(int count)
    {
        using var folder = new TemporaryBibleDirectory();
        using (var db = folder.Open())
        using (var seed = db.CreateCommand())
        {
            seed.CommandText = $"""
                WITH RECURSIVE numbers(n) AS (SELECT 1 UNION ALL SELECT n+1 FROM numbers WHERE n<{count})
                INSERT INTO Biblefolder(NAME,FILENAME,DISPLAYORDER)
                SELECT printf('Bible %03d',n), printf('%03d.db',n), n*3 FROM numbers;
                INSERT INTO Biblefolder(NAME,FILENAME,DISPLAYORDER) VALUES('Hidden','hidden.db',-1);
                """;
            seed.ExecuteNonQuery();
        }
        string previousConnection = Gf.ConnectStringBibleDB;
        string previousDirectory = Gf.BibleDir;
        var previousCatalog = Gf.BibleVersions;
        var previousVerses = Gf.HB_VersesLocation;
        using var control = new TabControl();
        var tabs = control;
        try
        {
            Gf.ConnectStringBibleDB = folder.ConnectionString;
            Gf.BibleDir = folder.DirectoryPath + Path.DirectorySeparatorChar;
            Gf.HB_VersesLocation = new();
            Gf.HB_VersesLocation[0].Version = 1;
            Gf.HB_VersesLocation[1].Version = count - 1;
            Gf.LoadBibleVersions(ref tabs);
            Assert.Equal(count, tabs.TabPages.Count);
            Assert.Equal("Bible 001", tabs.TabPages[0].Text);
            // Dropping the table proves that switching tabs and looking up saved IDs use the loaded catalog.
            folder.Execute("DROP TABLE Biblefolder");
            Assert.Equal(Gf.BibleDir + "001.db", Gf.GetBibleFileName(0));
            Assert.Equal(count - 1, Gf.LookUpBibleVersionNumber($"{count:000}.db"));
            Assert.Equal(-1, Gf.LookUpBibleVersionNumber("hidden.db"));
            Assert.Equal("", Gf.GetBibleFileName(count));
            Assert.True(Gf.HBConvertVersion(0));
            Assert.Equal(0, Gf.HB_VersesLocation[1].Version);
            Assert.False(Gf.HBConvertVersion(-1));
            Assert.False(Gf.HBConvertVersion(count));

            using var bookList = new ComboBox();
            var books = bookList;
            using (var db = new SQLiteConnection("Data Source=" + Gf.GetBibleFileName(0)))
            {
                db.Open();
                using var command = new SQLiteCommand("""
                    CREATE TABLE Bible(book INTEGER, chapter INTEGER, verse INTEGER, bibletext TEXT);
                    INSERT INTO Bible VALUES(0,10,2,'Exodus'), (0,10,1,'Genesis'), (0,10,67,'skip'), (1,1,2,'Second verse');
                    """, db);
                command.ExecuteNonQuery();
            }
            Assert.True(Gf.LoadBibleBooksList(0, ref books, true, true));
            Assert.Equal(new[] { UiText.Get("gfBible", "AllBooks"), "Genesis", "Exodus", UiText.Get("gfBible", "SearchResults") }, books.Items.Cast<string>());
            Assert.Equal(UiText.Get("gfBible", "SearchResults"), books.SelectedItem);
            Assert.True(Gf.LoadBibleBooksList(0, ref books, false, false));
            Assert.Equal("Genesis", books.SelectedItem);
            Gf.HB_VersesLocation[1].Book = 1;
            Gf.HB_VersesLocation[1].Chapter = 1;
            Gf.HB_VersesLocation[1].Verse = 2;
            using var controlText = new RichTextBox();
            var verses = controlText;
            Assert.True(Gf.RefreshBiblePassages(0, books, ref verses));
            Assert.Equal("Genesis 1:2 Second verse", verses.Text);

            Assert.False(Gf.LoadBibleBooksList(1, ref books, false, false));
            Assert.False(File.Exists(Gf.GetBibleFileName(1)));
            Gf.LoadBibleVersions(ref tabs);
            Assert.False(tabs.Enabled);
            Assert.Equal(UiText.Get("gfBible", "ErrorLoadingBibles"), tabs.TabPages[0].Text);
            Assert.Equal(-1, Gf.LookUpBibleVersionNumber("001.db"));
        }
        finally
        {
            Gf.ConnectStringBibleDB = previousConnection;
            Gf.BibleDir = previousDirectory;
            Gf.HB_VersesLocation = previousVerses;
            Gf.ShowBibleVersions(tabs, previousCatalog);
        }
    }

    [Fact]
    public void OrderedVersionsKeepLegacyDefaultsAndTheFirstExactFileMatch()
    {
        using var folder = new TemporaryBibleDirectory();
        Assert.Equal(0, BibleVersionCatalog.Load(folder.ConnectionString).Count);
        folder.Execute("""
            INSERT INTO Biblefolder(NAME,FILENAME,SONGFOLDER,SIZE,DISPLAYORDER) VALUES
            ('B','same.db',0,201,7),('A','same.db',40,200,7),
            ('C','Same.db',41,'legacy',12),('D','d.db',1,5,20)
            """);
        var catalog = BibleVersionCatalog.Load(folder.ConnectionString);
        Assert.Equal(0, catalog.IndexOfFile("same.db"));
        Assert.Equal(2, catalog.IndexOfFile("Same.db"));
        Assert.Equal(-1, catalog.IndexOfFile("SAME.DB"));
        Assert.Equal(("A", 40, 200), (catalog[0].Name, catalog[0].SongFolder, catalog[0].FontSize));
        Assert.Equal((1, 80), (catalog[1].SongFolder, catalog[1].FontSize));
        Assert.Equal((1, 80), (catalog[2].SongFolder, catalog[2].FontSize));
        Assert.Equal(5, catalog[3].FontSize);
    }

    [Fact]
    public void SaveReordersRenamesAndAddsWithoutLosingMissingFileMetadataOrHiddenRows()
    {
        using var folder = new TemporaryBibleDirectory();
        folder.Execute("""
            INSERT INTO Biblefolder(NAME,FILENAME,DESCRIPTION,DISPLAYORDER,Extra) VALUES
            ('First','same.db','old description',0,'first metadata'),('Second','same.db','other description',3,'second metadata'),
            ('Remove','remove.db','',10,NULL),('Hidden','hidden.db','',-1,'preserve'),('Old name','restore.db','restore description',-1,'restore')
            """);
        var catalog = BibleVersionCatalog.Load(folder.ConnectionString);
        catalog.Save(folder.ConnectionString, [
            catalog[1] with { Name = "First", FontSize = 110 },
            new(null, "새 번역", "a'b.db", "한글 설명", "copyright", 4, 75),
            catalog[0] with { Name = "Second", SongFolder = 2 },
            new(null, "Restored", "restore.db", "", "", 1, 80)
        ]);
        var saved = BibleVersionCatalog.Load(folder.ConnectionString);
        Assert.Equal(4, saved.Count);
        Assert.Equal(catalog[1] with { Name = "First", FontSize = 110 }, saved[0]);
        Assert.Equal(catalog[0] with { Name = "Second", SongFolder = 2 }, saved[2]);
        Assert.Equal(("a'b.db", "한글 설명", "copyright"), (saved[1].FileName, saved[1].Description, saved[1].Copyright));
        Assert.Equal("restore description", saved[3].Description);
        Assert.Equal("second metadata", folder.Scalar("SELECT Extra FROM Biblefolder WHERE NAME='First'"));
        Assert.Equal("preserve", folder.Scalar("SELECT Extra FROM Biblefolder WHERE DISPLAYORDER=-1"));
        Assert.Equal(-1, saved.IndexOfFile("remove.db"));
        Assert.Equal(0, saved.IndexOfFile("same.db"));
    }

    [Fact]
    public void FailedSaveRollsBackAndAStaleEditorCannotOverwriteLaterChanges()
    {
        using var folder = new TemporaryBibleDirectory();
        folder.Execute("INSERT INTO Biblefolder(NAME,FILENAME,DISPLAYORDER) VALUES ('A','a.db',0),('B','b.db',1)");
        var catalog = BibleVersionCatalog.Load(folder.ConnectionString);
        Assert.Throws<SQLiteException>(() => catalog.Save(folder.ConnectionString,
            [catalog[0] with { Name = "Changed" }, new(null, "Changed", "new.db", "", "", 1, 80)]));
        var afterFailure = BibleVersionCatalog.Load(folder.ConnectionString);
        Assert.Equal(catalog[0], afterFailure[0]);
        Assert.Equal(catalog[1], afterFailure[1]);
        folder.Execute("UPDATE Biblefolder SET NAME='Other edit' WHERE FILENAME='a.db'");
        Assert.Throws<DBConcurrencyException>(() => catalog.Save(folder.ConnectionString, []));
        Assert.Equal("Other edit", BibleVersionCatalog.Load(folder.ConnectionString)[0].Name);
    }

    [Fact]
    public void DiscoveryFindsInstalledSqliteAndLegacyExtensionsAndSkipsListedFiles()
    {
        using var folder = new TemporaryBibleDirectory();
        foreach (string name in new[] { "new.DB", "legacy.mdb", "listed.db", "notes.txt" })
            File.WriteAllText(Path.Combine(folder.DirectoryPath, name), "");
        var listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LISTED.DB", "catalog.db" };
        var names = FrmOptions.FindUnlistedBibleFiles(folder.DirectoryPath, listed).Select(Path.GetFileName).Order();
        Assert.Equal(new[] { "legacy.mdb", "new.DB" }, names);
    }

    [Fact]
    public void OptionsKeepsEditsWithTheMovedRowAndMissingFileWarningsOutOfStorage()
    {
        using var folder = new TemporaryBibleDirectory();
        folder.Execute("""
            INSERT INTO Biblefolder(NAME,FILENAME,DESCRIPTION,DISPLAYORDER) VALUES
            ('First','a.db','description A',0),('Second','b.db','description B',1)
            """);
        ExceptionDispatchInfo failure = null;
        var thread = new Thread(() =>
        {
            var previousConnection = Gf.ConnectStringBibleDB;
            var previousDirectory = Gf.BibleDir;
            var previouslyChanged = Gf.Options_BibleListChanged;
            try
            {
                Gf.ConnectStringBibleDB = folder.ConnectionString;
                Gf.BibleDir = folder.DirectoryPath + Path.DirectorySeparatorChar;
                using var options = new FrmOptions();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var type = typeof(FrmOptions);
                var list = (ListView)type.GetField("BibleList", flags).GetValue(options);
                var folders = (ToolStripComboBox)type.GetField("BibleAssociatedFolder", flags).GetValue(options);
                var size = (NumericUpDown)type.GetField("BibleFontSizeUpDown", flags).GetValue(options);
                folders.Items.AddRange(["Folder 1", "Folder 2"]);
                _ = list.Handle;
                type.GetMethod("BuildBibleList", flags).Invoke(options, null);
                type.GetField("InitFormLoad", flags).SetValue(options, false);
                Assert.Contains("Cannot find Bible", list.Items[0].SubItems[1].Text);
                type.GetMethod("MoveSelectedBible", flags).Invoke(options, [1]);
                size.Value = 105;
                folders.SelectedIndex = 1;
                options.SaveBibleChanges();
                var saved = BibleVersionCatalog.Load(folder.ConnectionString);
                Assert.Equal(("Second", "description B", 80), (saved[0].Name, saved[0].Description, saved[0].FontSize));
                Assert.Equal(("First", "description A", 105, 2),
                    (saved[1].Name, saved[1].Description, saved[1].FontSize, saved[1].SongFolder));
            }
            catch (Exception ex) { failure = ExceptionDispatchInfo.Capture(ex); }
            finally
            {
                Gf.ConnectStringBibleDB = previousConnection;
                Gf.BibleDir = previousDirectory;
                Gf.Options_BibleListChanged = previouslyChanged;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        failure?.Throw();
    }

    private sealed class TemporaryBibleDirectory : IDisposable
    {
        public string DirectoryPath { get; } = Directory.CreateTempSubdirectory().FullName;
        public string ConnectionString => new SQLiteConnectionStringBuilder
            { DataSource = Path.Combine(DirectoryPath, "catalog.db"), Pooling = false }.ToString();

        public TemporaryBibleDirectory()
        {
            using var db = Open();
            using var create = db.CreateCommand();
            create.CommandText = """
                CREATE TABLE Biblefolder(NAME TEXT PRIMARY KEY, FILENAME TEXT, DESCRIPTION TEXT,
                    COPYRIGHT TEXT, SONGFOLDER INTEGER, SIZE INTEGER, DISPLAYORDER INTEGER, Extra TEXT)
                """;
            create.ExecuteNonQuery();
        }

        public SQLiteConnection Open()
        {
            var db = new SQLiteConnection(ConnectionString);
            db.Open();
            return db;
        }

        public void Execute(string sql)
        {
            using var db = Open();
            using var command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
        }

        public object Scalar(string sql)
        {
            using var db = Open();
            using var command = new SQLiteCommand(sql, db);
            return command.ExecuteScalar();
        }

        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
    }
}
