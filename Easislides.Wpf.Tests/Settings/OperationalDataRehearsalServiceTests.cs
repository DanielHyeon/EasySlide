using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class OperationalDataRehearsalServiceTests
{
    [Fact]
    public async Task RunAsync_DerivesLegacyAdminDbAndDryRunsAssetsWithoutCreatingDestinationOrBackup()
    {
        using var fixture = OperationalDataFixture.Create();
        fixture.WriteWorkingFile("Images\\stage.png", "image-bytes");
        fixture.CreateLegacyAdminDatabase();
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot).Succeeded.Should().BeTrue();
        var sut = new OperationalDataRehearsalService(
            settings,
            new AssetMigrationService(),
            new AdminDatabaseRepository());

        var report = await sut.RunAsync(new OperationalDataRehearsalRequest(
            DestinationRoot: fixture.DestinationRoot));

        report.Succeeded.Should().BeTrue();
        report.SourceRoot.Should().Be(fixture.WorkingFolder);
        report.DestinationRoot.Should().Be(fixture.DestinationRoot);
        report.BackupRoot.Should().Be(fixture.BackupRoot);
        report.AdminDatabasePath.Should().Be(fixture.AdminDatabasePath);
        report.AssetReport.Should().NotBeNull();
        report.AssetReport!.IsDryRun.Should().BeTrue();
        report.AssetReport.Items.Should().ContainSingle(item => item.RelativePath == "Images\\stage.png");
        report.DatabaseInventory.Should().NotBeNull();
        report.DatabaseInventory!.Succeeded.Should().BeTrue();
        Directory.Exists(fixture.DestinationRoot).Should().BeFalse();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
        report.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_WhenWorkingFolderIsMissing_ReturnsErrorWithoutCreatingFolders()
    {
        using var fixture = OperationalDataFixture.Create();
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot).Succeeded.Should().BeTrue();
        var sut = new OperationalDataRehearsalService(
            settings,
            new AssetMigrationService(),
            new AdminDatabaseRepository());

        var report = await sut.RunAsync(new OperationalDataRehearsalRequest(
            DestinationRoot: fixture.DestinationRoot));

        report.Succeeded.Should().BeFalse();
        report.Issues.Should().Contain(issue =>
            issue.Kind == OperationalDataRehearsalIssueKind.WorkingFolderMissing &&
            issue.Severity == OperationalDataRehearsalIssueSeverity.Error);
        report.AssetReport.Should().BeNull();
        report.DatabaseInventory.Should().BeNull();
        Directory.Exists(fixture.DestinationRoot).Should().BeFalse();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_WhenAdminDatabaseIsMissing_ReturnsWarningWithAssetDryRun()
    {
        using var fixture = OperationalDataFixture.Create();
        fixture.WriteWorkingFile("Media\\intro.mp4", "media-bytes");
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder).Succeeded.Should().BeTrue();
        settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot).Succeeded.Should().BeTrue();
        var sut = new OperationalDataRehearsalService(
            settings,
            new AssetMigrationService(),
            new AdminDatabaseRepository());

        var report = await sut.RunAsync(new OperationalDataRehearsalRequest(
            DestinationRoot: fixture.DestinationRoot));

        report.Succeeded.Should().BeTrue();
        report.AssetReport.Should().NotBeNull();
        report.AssetReport!.Items.Should().ContainSingle(item => item.RelativePath == "Media\\intro.mp4");
        report.DatabaseInventory.Should().BeNull();
        report.Issues.Should().Contain(issue =>
            issue.Kind == OperationalDataRehearsalIssueKind.AdminDatabaseMissing &&
            issue.Severity == OperationalDataRehearsalIssueSeverity.Warning);
    }

    private sealed class OperationalDataFixture : IDisposable
    {
        private OperationalDataFixture(string root)
        {
            Root = root;
            WorkingFolder = Path.Combine(root, "LegacyRoot");
            DestinationRoot = Path.Combine(root, "EasislidesNext", "UserAssets");
            BackupRoot = Path.Combine(root, "EasislidesNext", "Backups");
            SettingsPath = Path.Combine(root, "settings.json");
            AdminDatabasePath = Path.Combine(WorkingFolder, "Admin", "Database", "EasiSlidesDb.db");
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public string DestinationRoot { get; }

        public string BackupRoot { get; }

        public string SettingsPath { get; }

        public string AdminDatabasePath { get; }

        public static OperationalDataFixture Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_Rehearsal_{Guid.NewGuid():N}"));

        public SettingsService CreateSettings()
            => new(new SettingsServiceOptions(SettingsPath, Path.Combine(Root, "SettingsBackups")));

        public void WriteWorkingFile(string relativePath, string content)
        {
            var path = Path.Combine(WorkingFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public void CreateLegacyAdminDatabase()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AdminDatabasePath)!);
            using var connection = Open(AdminDatabasePath);
            Execute(connection, "PRAGMA user_version = 4;");
            Execute(
                connection,
                """
                CREATE TABLE FOLDER (
                    FolderNo INTEGER PRIMARY KEY,
                    Name TEXT,
                    Use TEXT
                );
                """);
            Execute(
                connection,
                """
                CREATE TABLE SONG (
                    SONGID INTEGER PRIMARY KEY,
                    TITLE_1 TEXT,
                    TITLE_2 TEXT,
                    LYRICS TEXT,
                    KEY TEXT,
                    CATEGORY TEXT,
                    FOLDERNO INTEGER,
                    SONG_NUMBER INTEGER
                );
                """);
            Execute(connection, "CREATE TABLE LICENCE (ADMINISTRATOR TEXT, REF TEXT);");
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

        private static SQLiteConnection Open(string databasePath)
        {
            var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;");
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
