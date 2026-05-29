using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class ImportExportServiceTests
{
    [Fact]
    public async Task PreviewImportAsync_WhenLegacyText_ReturnsDistinctFoldersAndSongCount()
    {
        using var fixture = new ImportExportFixture();
        var source = fixture.WriteLegacyText(
            """
            [est3.1]
            [>Alpha>fLegacy A>n7>rPs 1>uCCLI>cCopyright>wWriter>kG>tFast>02>aAdminA>bAdminB>@1,c]
            [~G C]
            Line 1
            Line 2
            [>Beta>fLegacy B]
            Beta line
            """);

        var sut = fixture.CreateService();

        var preview = await sut.PreviewImportAsync(source);

        preview.Succeeded.Should().BeTrue();
        preview.Kind.Should().Be(ImportSourceKind.EasiSlidesText);
        preview.TotalSongs.Should().Be(2);
        preview.Folders.Select(folder => folder.Name).Should().Equal("Legacy A", "Legacy B");
    }

    [Fact]
    public async Task ImportAsync_WhenDuplicatePolicyKeepExisting_SavesCopyWithLegacyMetadata()
    {
        using var fixture = new ImportExportFixture();
        fixture.Repository.ExistingSongs.Add(new SongSummary(10, "Alpha", "", 2, 1, "", "", ""));
        var source = fixture.WriteLegacyText(
            """
            [est3.1]
            [>Alpha>fLegacy A>n7>rPs 1>uCCLI>cCopyright>wWriter>kG>tFast>02>aAdminA>bAdminB>@1,c]
            [~G C]
            Line 1
            Line 2
            """);

        var sut = fixture.CreateService();

        var report = await sut.ImportAsync(new ImportRequest(
            fixture.AdminDatabasePath,
            fixture.BackupRoot,
            source,
            TargetFolderNo: 2,
            SelectedSourceFolders: ["Legacy A"],
            DuplicatePolicy: ImportDuplicatePolicy.KeepExisting));

        report.Succeeded.Should().BeTrue();
        report.ImportedNew.Should().Be(1);
        report.Replaced.Should().Be(0);
        report.Skipped.Should().Be(0);
        fixture.Repository.SavedSongs.Should().ContainSingle();
        var saved = fixture.Repository.SavedSongs[0];
        saved.SongId.Should().BeNull();
        saved.FolderNo.Should().Be(2);
        saved.Title.Should().Be("Alpha");
        saved.SongNumber.Should().Be(7);
        saved.Lyrics.Should().Be("Line 1\nLine 2");
        saved.Notations.Should().Be("G C");
        saved.BookReference.Should().Be("Ps 1");
        saved.UserReference.Should().Be("CCLI");
        saved.Copyright.Should().Be("Copyright");
        saved.Writer.Should().Be("Writer");
        saved.Key.Should().Be("G");
        saved.Timing.Should().Be("Fast");
        saved.Capo.Should().Be(2);
        saved.LicenceAdmin1.Should().Be("AdminA");
        saved.LicenceAdmin2.Should().Be("AdminB");
        saved.Sequence.Should().Be("1,c");
    }

    [Fact]
    public async Task ImportAsync_WhenXmlWithReplaceExisting_UpdatesMatchingTitle()
    {
        using var fixture = new ImportExportFixture();
        fixture.Repository.ExistingSongs.Add(new SongSummary(22, "Gamma", "", 3, 1, "", "", ""));
        var source = fixture.WriteXml(
            new XElement("Item",
                new XElement("Title1", "Gamma"),
                new XElement("Title2", "Alt"),
                new XElement("Folder", "Xml Folder"),
                new XElement("SongNumber", "9"),
                new XElement("Contents", "Verse\r\nLine"),
                new XElement("Notations", "D A"),
                new XElement("Sequence", "1,t"),
                new XElement("Writer", "Writer"),
                new XElement("Copyright", "Copy"),
                new XElement("Category", "Praise"),
                new XElement("Timing", "Slow"),
                new XElement("MusicKey", "D"),
                new XElement("Capo", "1"),
                new XElement("LicenceAdmin1", "Admin 1"),
                new XElement("LicenceAdmin2", "Admin 2"),
                new XElement("BookReference", "Book"),
                new XElement("UserReference", "User"),
                new XElement("FormatData", "format"),
                new XElement("Settings", "settings")));

        var sut = fixture.CreateService();

        var report = await sut.ImportAsync(new ImportRequest(
            fixture.AdminDatabasePath,
            fixture.BackupRoot,
            source,
            TargetFolderNo: 3,
            SelectedSourceFolders: ["Xml Folder"],
            DuplicatePolicy: ImportDuplicatePolicy.ReplaceExisting));

        report.Succeeded.Should().BeTrue();
        report.ImportedNew.Should().Be(0);
        report.Replaced.Should().Be(1);
        var saved = fixture.Repository.SavedSongs.Single();
        saved.SongId.Should().Be(22);
        saved.AlternateTitle.Should().Be("Alt");
        saved.Lyrics.Should().Be("Verse\nLine");
        saved.Notations.Should().Be("D A");
        saved.Settings.Should().Be("settings");
        saved.FormatData.Should().Be("format");
    }

    [Fact]
    public async Task ImportAsync_WhenDocumentFolder_ScansTextAndDocxFilesAsSongs()
    {
        using var fixture = new ImportExportFixture();
        var sourceFolder = Directory.CreateDirectory(Path.Combine(fixture.Root, "Docs")).FullName;
        File.WriteAllText(Path.Combine(sourceFolder, "Plain.txt"), "Plain lyric", Encoding.UTF8);
        fixture.WriteDocx(Path.Combine(sourceFolder, "Word.docx"), "Word lyric");

        var sut = fixture.CreateService();

        var preview = await sut.PreviewImportAsync(sourceFolder);
        var report = await sut.ImportAsync(new ImportRequest(
            fixture.AdminDatabasePath,
            fixture.BackupRoot,
            sourceFolder,
            TargetFolderNo: 4,
            SelectedSourceFolders: ["Documents"],
            DuplicatePolicy: ImportDuplicatePolicy.SkipExisting));

        preview.Kind.Should().Be(ImportSourceKind.DocumentFolder);
        preview.TotalSongs.Should().Be(2);
        report.ImportedNew.Should().Be(2);
        fixture.Repository.SavedSongs.Select(song => song.Title).Should().Equal("Plain", "Word");
        fixture.Repository.SavedSongs.Select(song => song.Lyrics).Should().Equal("Plain lyric", "Word lyric");
    }

    [Fact]
    public async Task ExportAsync_WritesXmlTextDatabaseHtmlAndRtfOutputs()
    {
        using var fixture = new ImportExportFixture();
        fixture.Repository.Folders.Add(new SongFolderSummary(2, "Morning", true, 1));
        fixture.Repository.Details[10] = new SongDetail(
            10,
            "Alpha",
            "Alt",
            2,
            7,
            "Line 1\nLine 2",
            "1,c",
            "Writer",
            "Copyright",
            2,
            "Fast",
            "G",
            "G C",
            "Praise",
            "AdminA",
            "AdminB",
            "Ps 1",
            "CCLI",
            "settings",
            "format");

        var sut = fixture.CreateService();
        var xmlPath = Path.Combine(fixture.Root, "export.xml");
        var textPath = Path.Combine(fixture.Root, "export.esn");
        var dbPath = Path.Combine(fixture.Root, "export.esf");
        var htmlDir = Path.Combine(fixture.Root, "html");
        var rtfPath = Path.Combine(fixture.Root, "book.rtf");

        (await sut.ExportAsync(new ExportRequest(fixture.AdminDatabasePath, xmlPath, ExportFormat.Xml, [10], [2]))).Succeeded.Should().BeTrue();
        (await sut.ExportAsync(new ExportRequest(fixture.AdminDatabasePath, textPath, ExportFormat.EasiSlidesText, [10], [2]))).Succeeded.Should().BeTrue();
        (await sut.ExportAsync(new ExportRequest(fixture.AdminDatabasePath, dbPath, ExportFormat.EasiSlidesDatabase, [10], [2]))).Succeeded.Should().BeTrue();
        (await sut.ExportAsync(new ExportRequest(fixture.AdminDatabasePath, htmlDir, ExportFormat.Html, [10], [2]))).Succeeded.Should().BeTrue();
        (await sut.ExportAsync(new ExportRequest(fixture.AdminDatabasePath, rtfPath, ExportFormat.Rtf, [10], [2]))).Succeeded.Should().BeTrue();

        File.ReadAllText(xmlPath).Should().Contain("<Title1>Alpha</Title1>").And.Contain("<Folder>Morning</Folder>");
        File.ReadAllText(textPath).Should().StartWith("[est3.1]").And.Contain("[>Alpha>>Alt>fMorning>n7").And.Contain("[~G C]");
        File.Exists(Path.Combine(htmlDir, "index.htm")).Should().BeTrue();
        File.ReadAllText(Path.Combine(htmlDir, "Alpha.htm")).Should().Contain("Line 1").And.Contain("Ps 1");
        File.ReadAllText(rtfPath).Should().Contain(@"{\rtf1").And.Contain("Alpha").And.Contain("Line 2");

        using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        connection.Open();
        using var command = new SQLiteCommand("SELECT TITLE_1, FOLDERNO, LYRICS FROM SONG;", connection);
        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetString(0).Should().Be("Alpha");
        reader.GetInt32(1).Should().Be(2);
        reader.GetString(2).Should().Be("Line 1\nLine 2");
    }

    private sealed class ImportExportFixture : IDisposable
    {
        public ImportExportFixture()
        {
            Root = Path.Combine(Path.GetTempPath(), $"EasiSlides_ImportExport_{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            AdminDatabasePath = Path.Combine(Root, "Admin.db");
            BackupRoot = Path.Combine(Root, "Backups");
            File.WriteAllText(AdminDatabasePath, "");
            Repository = new FakeAdminRepository();
        }

        public string Root { get; }

        public string AdminDatabasePath { get; }

        public string BackupRoot { get; }

        public FakeAdminRepository Repository { get; }

        public ImportExportService CreateService()
            => new(Repository, Repository);

        public string WriteLegacyText(string contents)
        {
            var path = Path.Combine(Root, "import.esn");
            File.WriteAllText(path, contents.Replace("\r\n", "\n"), Encoding.UTF8);
            return path;
        }

        public string WriteXml(params XElement[] items)
        {
            var path = Path.Combine(Root, "import.xml");
            new XDocument(new XElement("EasiSlides", items)).Save(path);
            return path;
        }

        public void WriteDocx(string path, string text)
        {
            using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
            var entry = archive.CreateEntry("word/document.xml");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write($"""
                <?xml version="1.0" encoding="utf-8"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
                  <w:body><w:p><w:r><w:t>{text}</w:t></w:r></w:p></w:body>
                </w:document>
                """);
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

    private sealed class FakeAdminRepository : IAdminDatabaseRepository, IAdminSongDetailRepository
    {
        private int _nextSongId = 1000;

        public List<SongFolderSummary> Folders { get; } =
        [
            new(2, "Morning", true, 1),
            new(3, "Evening", true, 1),
            new(4, "Imported", true, 0),
        ];

        public List<SongSummary> ExistingSongs { get; } = [];

        public Dictionary<int, SongDetail> Details { get; } = [];

        public List<SongWriteModel> SavedSongs { get; } = [];

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => Task.FromResult(new AdminDatabaseSchemaInventory(true, databasePath, 0, [], new Dictionary<string, IReadOnlyList<DatabaseColumn>>(), []));

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => Task.FromResult<IReadOnlyList<SongFolderSummary>>(Folders);

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => Task.FromResult<IReadOnlyList<SongSummary>>(ExistingSongs.Where(song => folderNo is null || song.FolderNo == folderNo.Value).ToArray());

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
            => Task.FromResult(Details.TryGetValue(songId, out var detail) ? detail : null);

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => Task.FromResult<IReadOnlyList<DeletedSongSummary>>([]);

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
            => Task.FromResult(Report(databasePath, []));

        public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
        {
            SavedSongs.Add(song);
            var songId = song.SongId ?? _nextSongId++;
            ExistingSongs.RemoveAll(existing => existing.SongId == songId);
            ExistingSongs.Add(new SongSummary(songId, song.Title, song.AlternateTitle, song.FolderNo, song.SongNumber, song.Category, song.Key, song.Lyrics));
            return Task.FromResult(Report(databasePath, [songId]));
        }

        public Task<AdminDatabaseWriteReport> MoveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongMoveRequest> moves)
            => Task.FromResult(Report(databasePath, moves.Select(move => move.SongId).ToArray()));

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes)
            => Task.FromResult(Report(databasePath, deletes.Select(delete => delete.SongId).ToArray()));

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries)
            => Task.FromResult(Report(databasePath, recoveries.Select(recovery => recovery.SongId).ToArray()));

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order)
            => Task.FromResult(Report(databasePath, []));

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order)
            => Task.FromResult(Report(databasePath, order.Select(item => item.SongId).ToArray()));

        private static AdminDatabaseWriteReport Report(string databasePath, IReadOnlyList<int> songIds)
            => new(true, AdminDatabaseWriteOperation.SaveSong, databasePath, null, songIds, [], []);
    }
}
