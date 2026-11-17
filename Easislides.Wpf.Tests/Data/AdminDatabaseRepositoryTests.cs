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
        fixture.InsertSong(
            11,
            "Second",
            folderNo: 1,
            songNumber: 2,
            title2: "Alt",
            category: "Praise",
            key: "G",
            lyrics: "Line 2",
            bookReference: "시편 23",
            userReference: "입례");
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
        songs[1].BookReference.Should().Be("시편 23");
        songs[1].UserReference.Should().Be("입례");
    }

    [Fact]
    public async Task GetSongDetailAsync_ReturnsLegacyMergeFields()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(
            10,
            "Opening",
            folderNo: 1,
            songNumber: 2,
            title2: "Alt",
            category: "Praise",
            key: "G",
            lyrics: "[1]\nLine",
            sequence: "1",
            writer: "Writer",
            copyright: "Copyright",
            capo: 2,
            timing: "4/4",
            notations: "G C",
            licenceAdmin1: "Admin1",
            licenceAdmin2: "Admin2",
            bookReference: "Book",
            userReference: "User",
            settings: "Settings",
            formatData: "Format");
        var sut = new AdminDatabaseRepository();

        var detail = await sut.GetSongDetailAsync(fixture.DatabasePath, 10);

        detail.Should().NotBeNull();
        detail!.SongId.Should().Be(10);
        detail.Title.Should().Be("Opening");
        detail.AlternateTitle.Should().Be("Alt");
        detail.FolderNo.Should().Be(1);
        detail.SongNumber.Should().Be(2);
        detail.Lyrics.Should().Be("[1]\nLine");
        detail.Sequence.Should().Be("1");
        detail.Writer.Should().Be("Writer");
        detail.Copyright.Should().Be("Copyright");
        detail.Capo.Should().Be(2);
        detail.Timing.Should().Be("4/4");
        detail.Key.Should().Be("G");
        detail.Notations.Should().Be("G C");
        detail.Category.Should().Be("Praise");
        detail.LicenceAdmin1.Should().Be("Admin1");
        detail.LicenceAdmin2.Should().Be("Admin2");
        detail.BookReference.Should().Be("Book");
        detail.UserReference.Should().Be("User");
        detail.Settings.Should().Be("Settings");
        detail.FormatData.Should().Be("Format");
    }

    [Fact]
    public async Task GetDeletedSongsAsync_ReturnsFolderZeroSongsWithOriginalFolderAndDeletedDate()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(
            20,
            "Deleted Song",
            folderNo: 0,
            songNumber: 3,
            oldFolder: 2,
            lastModified: new DateTime(2026, 5, 29));
        fixture.InsertSong(21, "Active Song", folderNo: 2, songNumber: 4);
        var sut = new AdminDatabaseRepository();

        var songs = await sut.GetDeletedSongsAsync(fixture.DatabasePath);

        songs.Should().ContainSingle();
        songs[0].SongId.Should().Be(20);
        songs[0].Title.Should().Be("Deleted Song");
        songs[0].OriginalFolderNo.Should().Be(2);
        songs[0].OriginalFolderName.Should().Be("Evening");
        songs[0].DeletedOn.Should().Be(new DateTime(2026, 5, 29));
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
    public async Task SoftDeleteFoldersAsync_DisablesFoldersWithoutMovingSongs()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.SoftDeleteFoldersAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [new FolderDeleteRequest(1)]);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.SoftDeleteFolders);
        report.BackupPath.Should().NotBeNull();
        File.Exists(report.BackupPath).Should().BeTrue();
        report.AffectedFolderNos.Should().Equal(1);
        fixture.ReadFolderUse(1).Should().Be("False");
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1);
        AdminDatabaseFixture.ReadFolderUse(report.BackupPath!, 1).Should().Be("True");
    }

    [Fact]
    public async Task RecoverFoldersAsync_EnablesDisabledFoldersWithBackup()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "False");
        var sut = new AdminDatabaseRepository();

        var report = await sut.RecoverFoldersAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [new FolderRecoveryRequest(1)]);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.RecoverFolders);
        report.AffectedFolderNos.Should().Equal(1);
        fixture.ReadFolderUse(1).Should().Be("True");
        AdminDatabaseFixture.ReadFolderUse(report.BackupPath!, 1).Should().Be("False");
    }

    [Fact]
    public async Task SoftDeleteFoldersAsync_WhenFolderMissing_RestoresBackupAndReportsNotFound()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SoftDeleteFoldersAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [new FolderDeleteRequest(99)]);

        report.Succeeded.Should().BeFalse();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.SoftDeleteFolders);
        report.Issues.Should().Contain(issue => issue.Kind == AdminDatabaseWriteIssueKind.NotFound);
        fixture.ReadFolderUse(1).Should().Be("True");
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

    [Fact]
    public async Task MoveSongsAsync_WhenNormalMove_PreservesLastModified()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1, lastModified: new DateTime(2001, 2, 3));
        var sut = new AdminDatabaseRepository();

        var report = await sut.MoveSongsAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [new SongMoveRequest(10, OldFolderNo: 1, NewFolderNo: 2)]);

        report.Succeeded.Should().BeTrue();
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(2);
        fixture.ReadSongInt(10, "OldFolder").Should().Be(1);
        DateTime.Parse(fixture.ReadSongText(10, "LastModified")).Date.Should().Be(new DateTime(2001, 2, 3));
    }

    [Fact]
    public async Task SoftDeleteSongsAsync_MovesSongsToDeletedFolderAndUpdatesDeletedDate()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1, lastModified: new DateTime(2001, 2, 3));
        var sut = new AdminDatabaseRepository();

        var report = await sut.SoftDeleteSongsAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [new SongDeleteRequest(10, OriginalFolderNo: 1)]);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.SoftDeleteSongs);
        report.AffectedSongIds.Should().Equal(10);
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(0);
        fixture.ReadSongInt(10, "OldFolder").Should().Be(1);
        DateTime.Parse(fixture.ReadSongText(10, "LastModified")).Date.Should().Be(DateTime.Now.Date);
        AdminDatabaseFixture.ReadSongInt(report.BackupPath!, 10, "FOLDERNO").Should().Be(1);
    }

    [Fact]
    public async Task RecoverSongsAsync_RestoresTargetFolderAndClearsOldFolder()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 0, songNumber: 1, oldFolder: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.RecoverSongsAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [new SongRecoveryRequest(10, TargetFolderNo: 1)]);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.RecoverSongs);
        report.AffectedSongIds.Should().Equal(10);
        report.AffectedFolderNos.Should().Equal(1);
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1);
        fixture.ReadSongInt(10, "OldFolder").Should().Be(0);
        DateTime.Parse(fixture.ReadSongText(10, "LastModified")).Date.Should().Be(DateTime.Now.Date);
        AdminDatabaseFixture.ReadSongInt(report.BackupPath!, 10, "FOLDERNO").Should().Be(0);
    }

    [Fact]
    public async Task ReorderFoldersAsync_SwapsFolderNumbersAndMovesSongsWithBackup()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1);
        fixture.InsertSong(20, "Dismissal", folderNo: 2, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.ReorderFoldersAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            [
                new FolderOrderRequest(1, 2),
                new FolderOrderRequest(2, 1),
            ]);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.ReorderFolders);
        report.BackupPath.Should().NotBeNull();
        report.AffectedFolderNos.Should().Equal(2, 1);
        fixture.ReadFolderName(1).Should().Be("Evening");
        fixture.ReadFolderName(2).Should().Be("Morning");
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(2);
        fixture.ReadSongInt(20, "FOLDERNO").Should().Be(1);
        AdminDatabaseFixture.ReadFolderName(report.BackupPath!, 1).Should().Be("Morning");
        AdminDatabaseFixture.ReadSongInt(report.BackupPath!, 10, "FOLDERNO").Should().Be(1);
    }

    [Fact]
    public async Task CompactDatabaseAsync_VacuumsAndCreatesBackup_PreservesData()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(10, "First", folderNo: 1, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.CompactDatabaseAsync(fixture.DatabasePath, fixture.BackupRoot);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.Compact);
        report.BackupPath.Should().NotBeNullOrWhiteSpace("압축 전 백업을 만든다");
        System.IO.File.Exists(report.BackupPath!).Should().BeTrue();
        // VACUUM 후에도 데이터는 그대로다.
        fixture.ReadSongInt(10, "SONG_NUMBER").Should().Be(1);
    }

    [Fact]
    public async Task CompactDatabaseAsync_MissingDatabase_FailsWithoutThrowing()
    {
        var sut = new AdminDatabaseRepository();

        var report = await sut.CompactDatabaseAsync(@"C:\no\such\__missing__.db", System.IO.Path.GetTempPath());

        report.Succeeded.Should().BeFalse();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.Compact);
    }

    [Fact]
    public async Task CompactDatabaseAsync_EmptyBackupRoot_FailsWithoutCompacting()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.CompactDatabaseAsync(fixture.DatabasePath, backupRoot: "");

        report.Succeeded.Should().BeFalse("백업 경로가 없으면 압축하지 않는다(데이터 보호)");
        report.BackupPath.Should().BeNull();
        report.Issues.Should().Contain(i => i.Kind == AdminDatabaseWriteIssueKind.InvalidRequest);
    }

    [Fact]
    public async Task ReorderSongsAsync_RenumbersSongsWithinFolderWithBackup()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "First", folderNo: 1, songNumber: 1);
        fixture.InsertSong(11, "Second", folderNo: 1, songNumber: 2);
        fixture.InsertSong(12, "Third", folderNo: 1, songNumber: 3);
        fixture.InsertSong(20, "Other", folderNo: 2, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.ReorderSongsAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            1,
            [
                new SongOrderRequest(12, 1),
                new SongOrderRequest(10, 2),
                new SongOrderRequest(11, 3),
            ]);

        report.Succeeded.Should().BeTrue();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.ReorderSongs);
        report.AffectedSongIds.Should().Equal(12, 10, 11);
        report.AffectedFolderNos.Should().Equal(1);
        fixture.ReadSongInt(12, "SONG_NUMBER").Should().Be(1);
        fixture.ReadSongInt(10, "SONG_NUMBER").Should().Be(2);
        fixture.ReadSongInt(11, "SONG_NUMBER").Should().Be(3);
        fixture.ReadSongInt(20, "SONG_NUMBER").Should().Be(1);
        AdminDatabaseFixture.ReadSongInt(report.BackupPath!, 12, "SONG_NUMBER").Should().Be(3);
    }

    [Fact]
    public async Task ReorderSongsAsync_WhenSongIsOutsideFolder_RollsBackAndRestoresBackup()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "First", folderNo: 1, songNumber: 1);
        fixture.InsertSong(20, "Other", folderNo: 2, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.ReorderSongsAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            1,
            [
                new SongOrderRequest(10, 2),
                new SongOrderRequest(20, 1),
            ]);

        report.Succeeded.Should().BeFalse();
        report.Operation.Should().Be(AdminDatabaseWriteOperation.ReorderSongs);
        report.Issues.Should().Contain(issue => issue.Kind == AdminDatabaseWriteIssueKind.NotFound);
        fixture.ReadSongInt(10, "SONG_NUMBER").Should().Be(1);
        fixture.ReadSongInt(20, "SONG_NUMBER").Should().Be(1);
        AdminDatabaseFixture.ReadSongInt(report.BackupPath!, 10, "SONG_NUMBER").Should().Be(1);
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
            string sequence = "",
            string writer = "",
            string copyright = "",
            int capo = 0,
            string timing = "",
            string notations = "",
            string licenceAdmin1 = "",
            string licenceAdmin2 = "",
            string bookReference = "",
            string userReference = "",
            string settings = "",
            string formatData = "",
            int oldFolder = 0,
            DateTime? lastModified = null)
        {
            using var connection = Open();
            using var command = new SQLiteCommand(
                """
                INSERT INTO SONG
                    (SONGID, TITLE_1, TITLE_2, CATEGORY, KEY, FOLDERNO, SONG_NUMBER, LYRICS, SEQUENCE,
                     WRITER, COPYRIGHT, CAPO, TIMING, MSC, LICENCE_ADMIN1, LICENCE_ADMIN2, BOOK_REFERENCE,
                     USER_REFERENCE, SETTINGS, FORMATDATA, OldFolder, LastModified)
                VALUES
                    (@songId, @title, @title2, @category, @key, @folderNo, @songNumber, @lyrics, @sequence,
                     @writer, @copyright, @capo, @timing, @notations, @licenceAdmin1, @licenceAdmin2,
                     @bookReference, @userReference, @settings, @formatData, @oldFolder, @lastModified);
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
            command.Parameters.AddWithValue("@sequence", sequence);
            command.Parameters.AddWithValue("@writer", writer);
            command.Parameters.AddWithValue("@copyright", copyright);
            command.Parameters.AddWithValue("@capo", capo);
            command.Parameters.AddWithValue("@timing", timing);
            command.Parameters.AddWithValue("@notations", notations);
            command.Parameters.AddWithValue("@licenceAdmin1", licenceAdmin1);
            command.Parameters.AddWithValue("@licenceAdmin2", licenceAdmin2);
            command.Parameters.AddWithValue("@bookReference", bookReference);
            command.Parameters.AddWithValue("@userReference", userReference);
            command.Parameters.AddWithValue("@settings", settings);
            command.Parameters.AddWithValue("@formatData", formatData);
            command.Parameters.AddWithValue("@oldFolder", oldFolder);
            command.Parameters.AddWithValue("@lastModified", lastModified ?? DateTime.MinValue.Date);
            command.ExecuteNonQuery();
        }

        public string ReadFolderName(int folderNo) => ReadFolderText(DatabasePath, folderNo, "Name");

        public string ReadFolderUse(int folderNo) => ReadFolderText(DatabasePath, folderNo, "Use");

        public static string ReadFolderName(string databasePath, int folderNo) => ReadFolderText(databasePath, folderNo, "Name");

        public static string ReadFolderUse(string databasePath, int folderNo) => ReadFolderText(databasePath, folderNo, "Use");

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
