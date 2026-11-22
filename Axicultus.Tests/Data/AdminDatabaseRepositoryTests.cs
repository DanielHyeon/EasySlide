using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Data;

public class AdminDatabaseRepositoryTests
{
    [Fact]
    public async Task ChangingTheLibrarySongBackgroundPreservesOtherFormatFieldsAndOtherRows()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Songs", "True");
        fixture.InsertSong(7, "First", 1, 1, formatData: "99=retain>50=2>51=test.mp4>");
        fixture.InsertSong(8, "Second", 1, 2, formatData: "99=untouched>");
        var repository = new AdminDatabaseRepository();
        var path = Path.Combine(fixture.BackupRoot, "Tiles", "하늘.jpg");
        var report = await repository.SaveSongBackgroundAsync(fixture.DatabasePath, fixture.BackupRoot, 7, path);
        report.Succeeded.Should().BeTrue();
        var rows = await repository.GetSongsAsync(fixture.DatabasePath, 1);
        rows[0].FormatData.Should().Contain("99=retain>").And.Contain("51=test.mp4>");
        SongFormatData.Parse(rows[0].FormatData)!.BackgroundImagePath.Should().Be(path);
        SongFormatData.Parse(rows[0].FormatData)!.BackgroundImageMode.Should().Be(LyricsBackgroundMode.Tile);
        rows[1].FormatData.Should().Be("99=untouched>");
    }

    [Fact]
    public async Task MatchingCatalogLoadsRequestedIdsAndOrdinalUnicodeTitlesAndKeepsFirstDuplicate()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Music", "True");
        fixture.InsertSong(1, "Élan", 1, 4);
        fixture.InsertSong(2, "Élan", 1, 1);
        fixture.InsertSong(3, "다른 곡", 1, 2);
        fixture.InsertSong(4, "Élan", 0, 1);
        var repo = new AdminDatabaseRepository();
        var matches = await repo.GetMatchingSongsAsync(fixture.DatabasePath, new HashSet<int> { 3, 4 },
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "élan" });
        matches.Select(s => s.SongId).Should().BeEquivalentTo([1, 2, 3]);
        var titles = await repo.GetSongTitleIndexAsync(fixture.DatabasePath, 1);
        titles["élan"].Should().Be(2);
    }

    [Fact]
    public async Task FolderListingAndMediaReadTheCompleteFolderAndMaintenanceWritesOnlyItsTargets()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Songs", "True");
        fixture.InsertSong(7, "A", 1, 1, writer: "Writer", sequence: "1 C C", bookReference: "MP5,SF 2",
            formatData: "99=retain>50=2>51=first.mp4>");
        fixture.InsertSong(8, "Deleted", 0, 2, oldFolder: 1);
        var repo = new AdminDatabaseRepository();
        var saved = await repo.SaveSongMediaAsync(fixture.DatabasePath, fixture.BackupRoot, 7, "O'Brien.mp4");
        saved.Succeeded.Should().BeTrue();
        saved.BackupPath.Should().BeNull("one song's media file is assigned again in one step: no database copy for it");
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
        var songs = await repo.GetSongsAsync(fixture.DatabasePath, 1);
        songs.Single().FormatData.Should().Be("99=retain>50=2>51=O'Brien.mp4>");
        songs.Single().Sequence.Should().Be("1 C C");
        FolderListing.TabSeparated(songs, ",").Should().Contain("A\tWriter\t5\t2\t");
        var purge = await repo.MaintainSongsAsync(fixture.DatabasePath, fixture.BackupRoot, AdminDatabaseWriteOperation.EmptyDeletedSongs);
        purge.Succeeded.Should().BeTrue();
        (await repo.GetSongsAsync(fixture.DatabasePath)).Select(song => song.SongId).Should().Equal(7);
        var clear = await repo.MaintainSongsAsync(fixture.DatabasePath, fixture.BackupRoot, AdminDatabaseWriteOperation.ClearSongFormatting);
        clear.Succeeded.Should().BeTrue();
        var unchanged = (await repo.GetSongsAsync(fixture.DatabasePath)).Single();
        unchanged.Should().Be(songs.Single() with { FormatData = "" });
    }

    [Fact]
    public async Task JsonSongExchange_RoundTripsMetadataAndOnlyTheSelectedFolderInSqlite()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Youth Choir", "True");
        fixture.InsertFolder(2, "Choir", "True");
        fixture.InsertFolder(3, "Destination", "True");
        fixture.InsertSong(10, "찬양 & \"빛\"", 1, 7, "Alt", "Praise", "D", "첫 줄\r\n둘째 줄\n",
            "1,c,12", "Writer", "Copyright", 2, "Slow", "G;2;", "Admin A", "Admin B", "Ps 1", "Ref", "settings", "format");
        fixture.InsertSong(11, "Other", 2, 8);

        var db = new AdminDatabaseRepository();
        var service = new ImportExportService(db, db);
        var path = service.GetDefaultExportPath(fixture.Root, new DateOnly(2026, 1, 1), ExportFormat.Json);
        Path.GetExtension(path).Should().Be(".json");
        var export = await service.ExportAsync(new ExportRequest(fixture.DatabasePath, path, ExportFormat.Json, [11, 10], [1, 2]));
        export.Succeeded.Should().BeTrue();
        var preview = await service.PreviewImportAsync(path);
        preview.Kind.Should().Be(ImportSourceKind.AxicultusJson);
        preview.TotalSongs.Should().Be(2);
        preview.Folders.Should().Equal(new ImportSourceFolder("Choir", 1), new ImportSourceFolder("Youth Choir", 1));
        using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(path));
        json.RootElement[0].GetProperty("Title1").GetString().Should().Be("Other");
        json.RootElement[1].GetProperty("SongNumber").GetInt32().Should().Be(7);

        var request = new ImportRequest(fixture.DatabasePath, fixture.BackupRoot, path, 3, ["Youth Choir"], ImportDuplicatePolicy.SkipExisting);
        var imported = await service.ImportAsync(request);
        imported.Succeeded.Should().BeTrue();
        imported.ImportedNew.Should().Be(1);
        var saved = (await db.GetSongsAsync(fixture.DatabasePath, 3)).Single();
        var original = await db.GetSongDetailAsync(fixture.DatabasePath, 10);
        (await db.GetSongDetailAsync(fixture.DatabasePath, saved.SongId))
            // The imported row holds the sequence "1,c,12" as WinForms verse codes.
            .Should().Be(original! with { SongId = saved.SongId, FolderNo = 3, Sequence = "\u0001\u0000\u000c" });
        (await service.ImportAsync(request)).Skipped.Should().Be(1);
    }

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
    public async Task FolderFormatEdits_UpdateAvailableColumnsAndPreserveUneditedFields()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        using (var connection = new SQLiteConnection($"Data Source={fixture.DatabasePath}"))
        {
            connection.Open();
            using var schema = new SQLiteCommand(
                "ALTER TABLE FOLDER ADD COLUMN Size0 INTEGER DEFAULT 31; ALTER TABLE FOLDER ADD COLUMN BridgeHeading TEXT", connection);
            schema.ExecuteNonQuery();
        }
        var repository = new AdminDatabaseRepository();
        var changes = new Dictionary<string, string> { ["BridgeHeading"] = "Bridge's heading" };
        var result = await repository.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot,
            new SongFolderWriteModel(1, "Morning", true, FormatChanges: changes));
        result.Succeeded.Should().BeTrue();
        var folder = (await repository.GetSongFoldersAsync(fixture.DatabasePath)).Single();
        folder.FormatFields!["BridgeHeading"].Should().Be("Bridge's heading");
        folder.FormatFields["Size0"].Should().Be("31");
        folder.FormatFields.Should().NotContainKey("Size1");
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task SaveFolderAsync_WithGroupStyle_KeepsTitleWordCountAndNumberOrders(int groupStyle)
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.AddFolderGroupStyleColumn();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongFolderWriteModel(1, "Morning", IsEnabled: true, GroupStyle: groupStyle));

        report.Succeeded.Should().BeTrue();
        fixture.ReadFolderGroupStyle(1).Should().Be(groupStyle);
        var folders = await sut.GetSongFoldersAsync(fixture.DatabasePath);
        folders.Single(folder => folder.FolderNo == 1).GroupStyle.Should().Be(groupStyle);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(99)]
    public async Task SaveFolderAsync_WithOutOfRangeGroupStyle_StoresTitleOrder(int groupStyle)
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.AddFolderGroupStyleColumn();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.SetFolderGroupStyle(1, 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongFolderWriteModel(1, "Morning", IsEnabled: true, GroupStyle: groupStyle));

        report.Succeeded.Should().BeTrue();
        fixture.ReadFolderGroupStyle(1).Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task SaveFolderAsync_WithoutGroupStyle_LeavesStoredOrderOfExistingFolder(int storedGroupStyle)
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.AddFolderGroupStyleColumn();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.SetFolderGroupStyle(1, storedGroupStyle);
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongFolderWriteModel(1, "Updated Morning", IsEnabled: false));

        report.Succeeded.Should().BeTrue();
        fixture.ReadFolderName(1).Should().Be("Updated Morning");
        fixture.ReadFolderUse(1).Should().Be("False");
        fixture.ReadFolderGroupStyle(1).Should().Be(storedGroupStyle);
    }

    [Fact]
    public async Task SaveFolderAsync_NewFolderWithoutGroupStyle_StoresTitleOrder()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        // No column default: if the INSERT left GroupStyle out, the stored value would be NULL.
        fixture.AddFolderGroupStyleColumn(withDefault: false);
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(
            fixture.DatabasePath,
            fixture.BackupRoot,
            new SongFolderWriteModel(5, "New Folder", IsEnabled: true));

        report.Succeeded.Should().BeTrue();
        fixture.ReadFolderName(5).Should().Be("New Folder");
        var stored = fixture.ReadFolderGroupStyleRaw(5);
        stored.Should().NotBeNull().And.NotBeOfType<DBNull>("the INSERT must write GroupStyle explicitly");
        Convert.ToInt64(stored).Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FolderEditorSave_RenameAndDisable_KeepsStoredFolderOrder(int storedGroupStyle)
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.AddFolderGroupStyleColumn();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.SetFolderGroupStyle(1, storedGroupStyle);
        var repository = new AdminDatabaseRepository();
        var settings = new SettingsService(new SettingsServiceOptions(
            Path.Combine(fixture.Root, "settings.json"),
            Path.Combine(fixture.Root, "SettingsBackups")));
        settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var folder = (await repository.GetSongFoldersAsync(fixture.DatabasePath)).Single(item => item.FolderNo == 1);
        folder.GroupStyle.Should().Be(storedGroupStyle);
        var editor = new FolderEditorViewModel(settings, repository);
        editor.Load(fixture.DatabasePath, folder, [folder]);

        editor.Name = "Morning Revised";
        await editor.SaveAsync();
        editor.IsEnabled = false;
        await editor.SaveAsync();

        editor.StatusMessage.Should().Be("저장되었습니다.");
        fixture.ReadFolderName(1).Should().Be("Morning Revised");
        fixture.ReadFolderUse(1).Should().Be("False");
        fixture.ReadFolderGroupStyle(1).Should().Be(storedGroupStyle);
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
    public async Task SoftDeleteFoldersAsync_WhenFolderMissing_ReportsNotFoundAndChangesNothing()
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

    [Theory]
    [InlineData("1 C 2", "\u0001\u0000\u0002")] // tokens become WinForms verse codes (0 is the chorus)
    [InlineData("\u0001\u0000\u0002", "\u0001\u0000\u0002")] // verse codes are kept
    [InlineData("V1 C", "V1 C")] // a label the codes cannot express stays as typed
    public async Task SaveSongAsync_StoresSequenceAsWinFormsVerseCodes(string sequence, string stored)
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveSongAsync(fixture.DatabasePath, fixture.BackupRoot,
            new SongWriteModel(null, "Song", "", 1, 0, "[1]\nVerse", Sequence: sequence));

        report.Succeeded.Should().BeTrue();
        fixture.ReadSongText(report.AffectedSongIds[0], "SEQUENCE").Should().Be(stored);
        fixture.ReadSongInt(report.AffectedSongIds[0], "CAPO").Should().Be(-1, "a song saved without a capo has none");
    }

    [Fact]
    public async Task MoveSongsAsync_WhenTargetIsDeletedSongsFolder_RejectsWithoutWriting()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        var report = await sut.MoveSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
            [new SongMoveRequest(10, OldFolderNo: 1, NewFolderNo: 0)]);

        report.Succeeded.Should().BeFalse();
        report.Issues.Should().ContainSingle(issue => issue.Kind == AdminDatabaseWriteIssueKind.InvalidRequest);
        report.BackupPath.Should().BeNull();
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1);
    }

    [Fact]
    public async Task WhenAWriteFails_TheBackupIsNotCopiedOverTheDatabase()
    {
        // The rollback has undone the write; copying the backup back would erase what the other app committed
        // meanwhile. A handle that denies writers makes the write fail and would also make that copy fail, which
        // the report used to show as a second issue (RestoreFailed).
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1);
        var sut = new AdminDatabaseRepository();

        AdminDatabaseWriteReport report;
        using (new FileStream(fixture.DatabasePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            report = await sut.MoveSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
                [new SongMoveRequest(10, OldFolderNo: 1, NewFolderNo: 2)]);
        }

        report.Succeeded.Should().BeFalse();
        report.Issues.Should().ContainSingle().Which.Kind.Should().Be(AdminDatabaseWriteIssueKind.WriteFailed);
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1);
    }

    [Fact]
    public async Task SaveSongsAsync_SavesAllSongsWithOneBackup_OrNoneOfThem()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();
        SongWriteModel Song(string title, int? id = null) => new(id, title, "", 1, 0, "Line");

        var saved = await sut.SaveSongsAsync(fixture.DatabasePath, fixture.BackupRoot, [Song("A"), Song("B"), Song("C")]);

        saved.Succeeded.Should().BeTrue();
        saved.AffectedSongIds.Should().HaveCount(3).And.OnlyHaveUniqueItems();
        Directory.GetFiles(fixture.BackupRoot).Should().ContainSingle("the three songs are one operation");

        // The song with id 999 does not exist: nothing of the batch is kept.
        var failed = await sut.SaveSongsAsync(fixture.DatabasePath, fixture.BackupRoot, [Song("D"), Song("E", 999)]);

        failed.Succeeded.Should().BeFalse();
        (await sut.GetSongsAsync(fixture.DatabasePath, 1)).Select(song => song.Title).Should().BeEquivalentTo("A", "B", "C");
    }

    [Fact]
    public async Task ImportAsync_MakesOneBackupForTheWholeImport()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(3, "Destination", "True");
        var path = Path.Combine(fixture.Root, "songs.json");
        File.WriteAllText(path, """[{"Title1":"A","Contents":"a"},{"Title1":"B","Contents":"b"},{"Title1":"C","Contents":"c"}]""");
        var db = new AdminDatabaseRepository();

        var report = await new ImportExportService(db, db).ImportAsync(
            new ImportRequest(fixture.DatabasePath, fixture.BackupRoot, path, 3, ["Default Folder"], ImportDuplicatePolicy.SkipExisting));

        report.ImportedNew.Should().Be(3);
        Directory.GetFiles(fixture.BackupRoot).Should().ContainSingle();
    }

    [Fact]
    public async Task Backups_OnlyTheNewestAreKept()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        Directory.CreateDirectory(fixture.BackupRoot);
        string name = Path.GetFileNameWithoutExtension(fixture.DatabasePath);
        string Old(int index) => Path.Combine(fixture.BackupRoot, $"{name}.20200101-000000-{index:000}.db");
        for (int index = 0; index < AdminDatabaseRepository.BackupsKept + 4; index++) File.WriteAllText(Old(index), "old");
        // Not backups of this kind: a schema backup and another database's backup.
        string schemaBackup = Path.Combine(fixture.BackupRoot, $"{name}.schema3.20200101-000000-000.db");
        string otherDatabase = Path.Combine(fixture.BackupRoot, "EsUsage.20200101-000000-000.db");
        File.WriteAllText(schemaBackup, "keep");
        File.WriteAllText(otherDatabase, "keep");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(1, "Renamed", true));

        report.Succeeded.Should().BeTrue();
        File.Exists(report.BackupPath).Should().BeTrue("the newest backup is the one just made");
        Directory.GetFiles(fixture.BackupRoot, name + ".2*").Should().HaveCount(AdminDatabaseRepository.BackupsKept + 1);
        File.Exists(Old(0)).Should().BeTrue("the first copy of a month is kept");
        File.Exists(Old(1)).Should().BeFalse("the oldest of the others are removed");
        File.Exists(Old(AdminDatabaseRepository.BackupsKept + 3)).Should().BeTrue();
        File.Exists(schemaBackup).Should().BeTrue();
        File.Exists(otherDatabase).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFolderAsync_WhenNewFolderIsNumberedBeyond40_Rejects()
    {
        // The WinForms app reads folders 1 to 40 only.
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(41, "Too far", true));

        report.Succeeded.Should().BeFalse();
        report.Issues.Should().ContainSingle(issue => issue.Kind == AdminDatabaseWriteIssueKind.InvalidRequest);
        (await sut.GetSongFoldersAsync(fixture.DatabasePath)).Select(folder => folder.FolderNo).Should().Equal(1);
    }

    [Fact]
    public async Task MoveSongsAsync_WhenAnySongIsMissing_RollsBack()
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
    public async Task MoveSongsAsync_SeveralSongs_AreOneOperationWithOneBackup()
    {
        // What the move dialog does with a selection: read the rows still there, then move them together.
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "A", folderNo: 1, songNumber: 1);
        fixture.InsertSong(11, "B", folderNo: 1, songNumber: 2);
        fixture.InsertSong(12, "C", folderNo: 0, songNumber: 3, oldFolder: 1);
        var sut = new AdminDatabaseRepository();

        var stored = await sut.GetMatchingSongsAsync(fixture.DatabasePath, new HashSet<int> { 10, 11, 12, 999 }, new HashSet<string>());
        var report = await sut.MoveSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
            [.. stored.Select(song => new SongMoveRequest(song.SongId, song.FolderNo, 2))]);

        stored.Select(song => song.SongId).Should().BeEquivalentTo([10, 11], "a deleted song and a missing one are not in the library");
        report.Succeeded.Should().BeTrue();
        report.AffectedSongIds.Should().BeEquivalentTo([10, 11]);
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(2);
        fixture.ReadSongInt(11, "FOLDERNO").Should().Be(2);
        fixture.ReadSongInt(11, "OldFolder").Should().Be(1);
        Directory.GetFiles(fixture.BackupRoot).Should().ContainSingle("the selection is one operation");
    }

    [Fact]
    public async Task SoftDeleteSongsAsync_SeveralSongs_AreOneOperationWithOneBackup_OrNoneIsDeleted()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "A", folderNo: 1, songNumber: 1);
        fixture.InsertSong(11, "B", folderNo: 2, songNumber: 2);
        var sut = new AdminDatabaseRepository();

        var failed = await sut.SoftDeleteSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
            [new SongDeleteRequest(10, 1), new SongDeleteRequest(999, 1)]);

        failed.Succeeded.Should().BeFalse();
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1, "nothing of a failed selection is deleted");
        foreach (var backup in Directory.GetFiles(fixture.BackupRoot)) File.Delete(backup);

        var report = await sut.SoftDeleteSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
            [new SongDeleteRequest(10, 1), new SongDeleteRequest(11, 2)]);

        report.Succeeded.Should().BeTrue();
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(0);
        fixture.ReadSongInt(11, "FOLDERNO").Should().Be(0);
        fixture.ReadSongInt(11, "OldFolder").Should().Be(2);
        Directory.GetFiles(fixture.BackupRoot).Should().ContainSingle("the selection is one operation");
    }

    // ── Findings of the review, each on a small database of its own ──────────────────────────────────────

    private static void Sql(string databasePath, string sql)
    {
        using var connection = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = databasePath }.ToString());
        connection.Open();
        using var command = new SQLiteCommand(sql, connection);
        command.ExecuteNonQuery();
        SQLiteConnection.ClearAllPools();
    }

    private static string Text(string databasePath, string sql)
    {
        using var connection = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = databasePath, ReadOnly = true }.ToString());
        connection.Open();
        using var command = new SQLiteCommand(sql, connection);
        object? value = command.ExecuteScalar();
        return value is null or DBNull ? "NULL" : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
    }

    [Fact]
    public async Task Backups_TheOneJustMadeIsKept_WhenTheClockWasSetBack()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        Directory.CreateDirectory(fixture.BackupRoot);
        string name = Path.GetFileNameWithoutExtension(fixture.DatabasePath);
        for (int index = 0; index < AdminDatabaseRepository.BackupsKept; index++)
            File.WriteAllText(Path.Combine(fixture.BackupRoot, $"{name}.20990101-000000-{index:000}.db"), "made before the clock went back");

        var report = await new AdminDatabaseRepository().SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(1, "Renamed", true));

        report.Succeeded.Should().BeTrue();
        File.Exists(report.BackupPath).Should().BeTrue("it is the only copy of the database before this write");
        AdminDatabaseFixture.ReadFolderName(report.BackupPath!, 1).Should().Be("Morning");
    }

    [Fact]
    public async Task Backups_TheFirstOfARecentDayIsKept_BeyondTheNewestOnes()
    {
        // Twenty-two writes three days ago, then one today: the copy from before that day's work must survive.
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        Directory.CreateDirectory(fixture.BackupRoot);
        string name = Path.GetFileNameWithoutExtension(fixture.DatabasePath);
        string day = DateTime.UtcNow.AddDays(-3).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        string Old(int index) => Path.Combine(fixture.BackupRoot, $"{name}.{day}-000000-{index:000}.db");
        for (int index = 0; index < AdminDatabaseRepository.BackupsKept + 2; index++) File.WriteAllText(Old(index), "old");
        // Older than the days that are kept, and not among the newest.
        string longAgo = Path.Combine(fixture.BackupRoot, $"{name}.20200101-000000-000.db");
        string longAgoLater = Path.Combine(fixture.BackupRoot, $"{name}.20200102-000000-000.db");
        File.WriteAllText(longAgo, "old");
        File.WriteAllText(longAgoLater, "old");

        var report = await new AdminDatabaseRepository().SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(1, "Renamed", true));

        File.Exists(report.BackupPath).Should().BeTrue();
        File.Exists(Old(0)).Should().BeTrue("the first copy of a day within the last 30 is kept");
        File.Exists(Old(1)).Should().BeFalse();
        File.Exists(Old(2)).Should().BeFalse();
        File.Exists(Old(3)).Should().BeTrue("the newest 20 are kept");
        File.Exists(longAgo).Should().BeTrue("the first copy of a month is kept however old it is");
        File.Exists(longAgoLater).Should().BeFalse();
        Directory.GetFiles(fixture.BackupRoot).Should().HaveCount(AdminDatabaseRepository.BackupsKept + 2);
    }

    [Fact]
    public async Task SaveFolderAsync_WhenOnlyTheSortOrderChanges_MakesNoBackup()
    {
        // The song list saves the folder on every change of its sort order.
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.AddFolderGroupStyleColumn();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();

        var sorted = await sut.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(1, "Morning", true, GroupStyle: 2));

        sorted.Succeeded.Should().BeTrue();
        sorted.BackupPath.Should().BeNull();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
        fixture.ReadFolderGroupStyle(1).Should().Be(2);

        var renamed = await sut.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(1, "Evening", true, GroupStyle: 2));

        File.Exists(renamed.BackupPath).Should().BeTrue("a change of name is backed up");
    }

    [Fact]
    public async Task ReorderFoldersAsync_TakesOldFolderAlong_SoDeletedSongsAreRecoveredIntoTheirFolder()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "Live in Morning", folderNo: 1, songNumber: 0);
        fixture.InsertSong(11, "Deleted from Morning", folderNo: 0, songNumber: 0, oldFolder: 1);
        fixture.InsertSong(12, "Deleted from Evening", folderNo: 0, songNumber: 0, oldFolder: 2);
        var sut = new AdminDatabaseRepository();

        var report = await sut.ReorderFoldersAsync(fixture.DatabasePath, fixture.BackupRoot,
            [new FolderOrderRequest(1, 2), new FolderOrderRequest(2, 1)]);

        report.Succeeded.Should().BeTrue();
        fixture.ReadFolderName(2).Should().Be("Morning");
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(2);
        fixture.ReadSongInt(11, "OldFolder").Should().Be(2);
        fixture.ReadSongInt(12, "OldFolder").Should().Be(1);
        (await sut.GetDeletedSongsAsync(fixture.DatabasePath)).Single(song => song.SongId == 11).OriginalFolderName.Should().Be("Morning");
    }

    [Fact]
    public async Task RecoverSongsAsync_RecoversOnlySongsThatAreStillDeleted()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "Deleted", folderNo: 0, songNumber: 0, oldFolder: 1);
        fixture.InsertSong(11, "Recovered by the other app and moved", folderNo: 2, songNumber: 0);
        var sut = new AdminDatabaseRepository();

        var report = await sut.RecoverSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
            [new SongRecoveryRequest(10, 1), new SongRecoveryRequest(11, 1), new SongRecoveryRequest(999, 1)]);

        report.Succeeded.Should().BeTrue();
        report.AffectedSongIds.Should().Equal([10], "the others are reported as skipped by the caller");
        fixture.ReadSongInt(10, "FOLDERNO").Should().Be(1);
        fixture.ReadSongInt(11, "FOLDERNO").Should().Be(2);
        // yyyy-MM-dd, as the WinForms app writes LastModified (not "yyyy-MM-dd 00:00:00").
        string today = DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        fixture.ReadSongText(10, "LastModified").Should().Be(today);

        var saved = await sut.SaveSongAsync(fixture.DatabasePath, fixture.BackupRoot, new SongWriteModel(null, "New", "", 1, 0, "Line"));
        fixture.ReadSongText(saved.AffectedSongIds[0], "LastModified").Should().Be(today);
        await sut.SoftDeleteSongsAsync(fixture.DatabasePath, fixture.BackupRoot, [new SongDeleteRequest(11, 2)]);
        fixture.ReadSongText(11, "LastModified").Should().Be(today);
    }

    [Fact]
    public async Task ANewFolderInASlotUsedBefore_StartsWithNewFolderFormatting_AndDeletedSongsAreCountedPerFolder()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.AddFolderGroupStyleColumn();
        Sql(fixture.DatabasePath, """
            ALTER TABLE FOLDER ADD COLUMN ChorusHeading TEXT; ALTER TABLE FOLDER ADD COLUMN Size0 INTEGER;
            ALTER TABLE FOLDER ADD COLUMN Fontname0 TEXT; ALTER TABLE FOLDER ADD COLUMN LMargin TEXT;
            """);
        fixture.InsertFolder(2, "Evening", use: "False");
        fixture.InsertFolder(3, "Former folder", use: "False");
        Sql(fixture.DatabasePath, "UPDATE FOLDER SET GroupStyle = 2, ChorusHeading = 'Refrain', Size0 = 72, Fontname0 = 'Custom', LMargin = '9' WHERE FolderNo = 3");
        fixture.InsertSong(20, "Deleted from Evening", folderNo: 0, songNumber: 0, oldFolder: 2);
        var sut = new AdminDatabaseRepository();

        var folders = await sut.GetSongFoldersAsync(fixture.DatabasePath);
        folders.Single(folder => folder.FolderNo == 2).DeletedSongCount.Should().Be(1, "recovery would file that song in folder 2");
        folders.Single(folder => folder.FolderNo == 3).DeletedSongCount.Should().Be(0);

        var report = await sut.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(3, "New folder", true, ResetFormat: true));

        report.Succeeded.Should().BeTrue();
        Text(fixture.DatabasePath, "SELECT Name || '|' || GroupStyle || '|' || ChorusHeading || '|' || Size0 || '|' || Fontname0 || '|' || COALESCE(LMargin, 'NULL') FROM FOLDER WHERE FolderNo = 3")
            .Should().Be("New folder|0|Chorus:|40|Microsoft Sans Serif|NULL", "what Gf.ResetFolder of the WinForms app gives a new folder");

        // An existing folder that is only renamed keeps its formatting.
        Sql(fixture.DatabasePath, "UPDATE FOLDER SET Size0 = 55 WHERE FolderNo = 3");
        await sut.SaveFolderAsync(fixture.DatabasePath, fixture.BackupRoot, new SongFolderWriteModel(3, "Renamed", true));
        Text(fixture.DatabasePath, "SELECT Size0 FROM FOLDER WHERE FolderNo = 3").Should().Be("55");
    }

    [Fact]
    public async Task SortKeys_AreTheOnesWinFormsWrites()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Chinese", use: "True");
        // As WinForms stored them: two digits of stroke count and five of character code for each character.
        fixture.InsertSong(1, "一件禮物", folderNo: 1, songNumber: 0);
        fixture.InsertSong(2, "?是王", folderNo: 1, songNumber: 0); // the conversion lost the first character; the key still has it
        Sql(fixture.DatabasePath, """
            UPDATE SONG SET CJK_WordCount = '004', CJK_StrokeCount = '0119968062021418311500829289' WHERE SONGID = 1;
            UPDATE SONG SET CJK_WordCount = '003', CJK_StrokeCount = '072032009261590429579' WHERE SONGID = 2;
            """);
        var sut = new AdminDatabaseRepository();
        string[] titles = ["一物", "一件禮物一件禮物", "一件禮物", "?是王", "?件", "주 예수보다 더", "나의 갈길", "Amazing (Grace)", "王"];

        var report = await sut.SaveSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
            [.. titles.Select(title => new SongWriteModel(null, title, "", 1, 0, "x"))]);

        report.Succeeded.Should().BeTrue();
        string[] keys = [.. report.AffectedSongIds.Select(id => fixture.ReadSongText(id, "CJK_WordCount") + " " + fixture.ReadSongText(id, "CJK_StrokeCount"))];
        keys.Should().Equal(
            "002 01199680829289",                                          // stroke counts read from the stored keys
            "008 011996806202141831150082928901199680620214禮物",          // six characters, then the rest of the title
            "004 0119968062021418311500829289",
            "000 072032009261590429579",                                   // the title is one a song already has: its stroke key is kept
            "000 000?件",                                                  // "?" has no stroke count, as in WinForms
            "008 000주 예수보다 더",                                        // one character before the space: the whole length
            "001 000나의 갈길",
            "000 000Amazing (Grace)",
            "001 0429579");                                                // 王 was in a stored key (four strokes)
    }

    [Fact]
    public async Task Sequence_ChorusCodeAndWhitespaceCodes_GoThroughSqliteUnchanged()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        var sut = new AdminDatabaseRepository();
        string Hex(int id) => Text(fixture.DatabasePath, $"SELECT hex(CAST(SEQUENCE AS BLOB)) FROM SONG WHERE SONGID = {id}");

        var typed = await sut.SaveSongAsync(fixture.DatabasePath, fixture.BackupRoot, new SongWriteModel(null, "Typed", "", 1, 0, "x", Sequence: "1,c,2,c,9"));
        Hex(typed.AffectedSongIds[0]).Should().Be("0100020009", "chorus is code 0, verse 9 a tab");
        var detail = await sut.GetSongDetailAsync(fixture.DatabasePath, typed.AffectedSongIds[0]);
        detail!.Sequence.Should().Be("\u0001\0\u0002\0\t");

        // A copy writes the stored value as it is: verse 9 + ending is "\te", which as typed text would be just "e".
        var copies = await sut.SaveSongsAsync(fixture.DatabasePath, fixture.BackupRoot,
        [
            new SongWriteModel(null, "Copy", "", 1, 0, "x", Sequence: detail.Sequence, SequenceIsStored: true),
            new SongWriteModel(null, "Verse 9 and ending", "", 1, 0, "x", Sequence: "\te", SequenceIsStored: true),
            new SongWriteModel(null, "Prechorus 2 alone", "", 1, 0, "x", Sequence: "p", SequenceIsStored: true),
            new SongWriteModel(null, "Not marked as stored", "", 1, 0, "x", Sequence: "\te"),
        ]);
        copies.AffectedSongIds.Select(Hex).Should().Equal("0100020009", "0965", "70", "0965");
    }

    [Fact]
    public async Task SongCopyViewModel_WithTheRealRepository_CopiesEveryStoredColumn()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(1, "Morning", use: "True");
        fixture.InsertFolder(2, "Evening", use: "True");
        fixture.InsertSong(10, "한글 제목 It's 100%", 1, 28, title2: "Alt", category: "Call", key: "G", lyrics: "[1]\r\na\r\n[chorus]\r\nb",
            sequence: "\u0001\0\u0001\0", writer: "W", copyright: "C", capo: 2, timing: "4/4", notations: "(0;C)", licenceAdmin1: "L1",
            licenceAdmin2: "L2", bookReference: "B", userReference: "U", settings: "s", formatData: "50=2>");
        fixture.InsertSong(11, "Nine", 1, 0, lyrics: "[9]\r\nnine\r\n[ending]\r\nend", sequence: "\te", capo: -1);
        fixture.InsertSong(12, "一件禮物", 1, 0, lyrics: "x");
        Sql(fixture.DatabasePath, """
            UPDATE SONG SET CJK_WordCount = '001', CJK_StrokeCount = '000한글 제목 It''s 100%' WHERE SONGID = 10;
            UPDATE SONG SET CJK_WordCount = '000', CJK_StrokeCount = '000Nine' WHERE SONGID = 11;
            UPDATE SONG SET CJK_WordCount = '004', CJK_StrokeCount = '0119968062021418311500829289' WHERE SONGID = 12;
            """);
        var repository = new AdminDatabaseRepository();
        var settings = new Axicultus.Wpf.Settings.SettingsService(new Axicultus.Wpf.Settings.SettingsServiceOptions(
            Path.Combine(fixture.Root, "settings.json"), Path.Combine(fixture.Root, "SettingsBackups")));
        var sut = new SongCopyViewModel(settings, repository);
        sut.Load(fixture.DatabasePath, await repository.GetSongsAsync(fixture.DatabasePath, 1), await repository.GetSongFoldersAsync(fixture.DatabasePath));
        sut.SelectedTargetFolder = sut.TargetFolders.Single(folder => folder.FolderNo == 2);

        await sut.CopyAsync();

        sut.StatusMessage.Should().Be("3곡을 복사했습니다.");
        const string columns = "TITLE_1 || '|' || TITLE_2 || '|' || WRITER || '|' || COPYRIGHT || '|' || CJK_WordCount || '|' || CJK_StrokeCount || '|' || LYRICS"
            + " || '|' || hex(CAST(SEQUENCE AS BLOB)) || '|' || KEY || '|' || CAPO || '|' || TIMING || '|' || MSC || '|' || CATEGORY || '|' || SONG_NUMBER"
            + " || '|' || LICENCE_ADMIN1 || '|' || LICENCE_ADMIN2 || '|' || BOOK_REFERENCE || '|' || USER_REFERENCE || '|' || SETTINGS || '|' || FORMATDATA";
        var copies = await repository.GetSongsAsync(fixture.DatabasePath, 2);
        copies.Select(song => song.Title).Should().BeEquivalentTo("한글 제목 It's 100%", "Nine", "一件禮物");
        foreach (var copy in copies)
        {
            int source = copy.Title switch { "Nine" => 11, "一件禮物" => 12, _ => 10 };
            Text(fixture.DatabasePath, $"SELECT {columns} FROM SONG WHERE SONGID = {copy.SongId}")
                .Should().Be(Text(fixture.DatabasePath, $"SELECT {columns} FROM SONG WHERE SONGID = {source}"), copy.Title);
        }
        Directory.GetFiles(Path.Combine(fixture.Root, "Backups")).Should().ContainSingle("one backup for the whole selection");
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
    public async Task FolderReorderRestoresDatabaseSelectionAndJumpsWhenSettingsAreLocked()
    {
        using var fixture = AdminDatabaseFixture.Create();
        fixture.CreateLegacySchema();
        fixture.InsertFolder(0, "Recycle", "False");
        fixture.InsertFolder(1, "Morning", "True");
        fixture.InsertFolder(2, "Evening", "True");
        fixture.InsertSong(10, "Opening", folderNo: 1, songNumber: 1);
        var path = Path.Combine(fixture.Root, "settings.json");
        var settings = new SettingsService(new SettingsServiceOptions(path, fixture.BackupRoot));
        settings.Set(AxicultusSettingKeys.AdminDatabasePath, fixture.DatabasePath);
        settings.Set(AxicultusSettingKeys.JumpToA, 1);
        var repository = new AdminDatabaseRepository();
        var vm = new LibraryViewModel(settings, repository);
        await vm.LoadAsync();
        vm.SelectFolderByNo(1);
        vm.MoveSelectedFolderUpCommand.CanExecute(null).Should().BeFalse();
        await vm.MoveSelectedFolderToIndexAsync(0);
        vm.SelectedFolder!.Name.Should().Be("Morning");
        using var lockedSettings = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        await vm.MoveSelectedFolderDownAsync();
        (await repository.GetSongFoldersAsync(fixture.DatabasePath)).Select(f => f.Name).Should().Equal("Recycle", "Morning", "Evening");
        vm.SelectedFolder!.Name.Should().Be("Morning");
        vm.JumpFolderA!.Name.Should().Be("Morning");
        vm.Songs.Should().ContainSingle().Which.SongId.Should().Be(10);
        vm.StatusMessage.Should().Contain("복원");
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
    public async Task ReorderSongsAsync_WhenSongIsOutsideFolder_RollsBack()
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
            DatabasePath = Path.Combine(root, "AxicultusDb.db");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string DatabasePath { get; }

        public string BackupRoot { get; }

        public static AdminDatabaseFixture Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_AdminDb_{Guid.NewGuid():N}"));

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

        public void AddFolderGroupStyleColumn(bool withDefault = true)
        {
            using var connection = Open();
            Execute(connection, withDefault
                ? "ALTER TABLE FOLDER ADD COLUMN GroupStyle INTEGER DEFAULT 0;"
                : "ALTER TABLE FOLDER ADD COLUMN GroupStyle INTEGER;");
        }

        // Raw stored value, so a NULL (column left out of the write) is not read as 0.
        public object? ReadFolderGroupStyleRaw(int folderNo)
        {
            using var connection = Open();
            using var command = new SQLiteCommand("SELECT GroupStyle FROM FOLDER WHERE FolderNo = @folderNo;", connection);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            return command.ExecuteScalar();
        }

        public void SetFolderGroupStyle(int folderNo, int groupStyle)
        {
            using var connection = Open();
            using var command = new SQLiteCommand("UPDATE FOLDER SET GroupStyle = @groupStyle WHERE FolderNo = @folderNo;", connection);
            command.Parameters.AddWithValue("@groupStyle", groupStyle);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            command.ExecuteNonQuery();
        }

        public int ReadFolderGroupStyle(int folderNo)
        {
            using var connection = Open();
            using var command = new SQLiteCommand("SELECT GroupStyle FROM FOLDER WHERE FolderNo = @folderNo;", connection);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            return Convert.ToInt32(command.ExecuteScalar());
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
            var path = Path.Combine(directory.FullName, "AdminDB", "Database", "AxicultusDb.db");
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Bundled AdminDB fixture was not found.");
    }
}
