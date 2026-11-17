using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using FluentAssertions;
using System.Data.SQLite;
using Xunit;

namespace Easislides.Wpf.Tests.Data;

public class DatabaseMigrationServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_WhenDatabaseExists_ReturnsVersionAndTables()
    {
        using var fixture = TempDatabaseFolder.Create();
        fixture.CreateDatabase(version: 2, "CREATE TABLE Items (Id INTEGER PRIMARY KEY, Title TEXT NOT NULL);");
        var sut = new DatabaseMigrationService();

        var analysis = await sut.AnalyzeAsync(fixture.DatabasePath);

        analysis.Succeeded.Should().BeTrue();
        analysis.SchemaVersion.Should().Be(2);
        analysis.Tables.Should().ContainSingle(table => table.Name == "Items");
        analysis.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task MigrateAsync_WhenDryRun_ReportsPendingStepsWithoutBackupOrMutation()
    {
        using var fixture = TempDatabaseFolder.Create();
        fixture.CreateDatabase(version: 1, "CREATE TABLE Items (Id INTEGER PRIMARY KEY, Title TEXT);");
        var sut = new DatabaseMigrationService();
        var request = fixture.Request(
            targetVersion: 3,
            dryRun: true,
            new DatabaseMigrationStep(1, 2, ["ALTER TABLE Items ADD COLUMN SortOrder INTEGER DEFAULT 0;"]),
            new DatabaseMigrationStep(2, 3, ["CREATE TABLE Tags (Id INTEGER PRIMARY KEY, Name TEXT);"]));

        var report = await sut.MigrateAsync(request);

        report.Succeeded.Should().BeTrue();
        report.IsDryRun.Should().BeTrue();
        report.BackupPath.Should().BeNull();
        report.PendingSteps.Select(step => step.ToVersion).Should().Equal(2, 3);
        fixture.ReadUserVersion().Should().Be(1);
        fixture.ColumnExists("Items", "SortOrder").Should().BeFalse();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
    }

    [Fact]
    public async Task MigrateAsync_CreatesBackupRunsOrderedMigrationsAndUpdatesUserVersion()
    {
        using var fixture = TempDatabaseFolder.Create();
        fixture.CreateDatabase(version: 1, "CREATE TABLE Items (Id INTEGER PRIMARY KEY, Title TEXT);");
        var sut = new DatabaseMigrationService();
        var request = fixture.Request(
            targetVersion: 3,
            dryRun: false,
            new DatabaseMigrationStep(1, 2, ["ALTER TABLE Items ADD COLUMN SortOrder INTEGER DEFAULT 0;"]),
            new DatabaseMigrationStep(2, 3, ["CREATE TABLE Tags (Id INTEGER PRIMARY KEY, Name TEXT);"]));

        var report = await sut.MigrateAsync(request);

        report.Succeeded.Should().BeTrue();
        report.BackupPath.Should().NotBeNull();
        File.Exists(report.BackupPath).Should().BeTrue();
        report.AppliedSteps.Select(step => step.ToVersion).Should().Equal(2, 3);
        fixture.ReadUserVersion().Should().Be(3);
        fixture.ColumnExists("Items", "SortOrder").Should().BeTrue();
        fixture.TableExists("Tags").Should().BeTrue();
        TempDatabaseFolder.ReadUserVersion(report.BackupPath!).Should().Be(1);
    }

    [Fact]
    public async Task MigrateAsync_WhenCommandFails_RollsBackAndRestoresBackup()
    {
        using var fixture = TempDatabaseFolder.Create();
        fixture.CreateDatabase(version: 1, "CREATE TABLE Items (Id INTEGER PRIMARY KEY, Title TEXT);");
        var sut = new DatabaseMigrationService();
        var request = fixture.Request(
            targetVersion: 3,
            dryRun: false,
            new DatabaseMigrationStep(1, 2, ["ALTER TABLE Items ADD COLUMN SortOrder INTEGER DEFAULT 0;"]),
            new DatabaseMigrationStep(2, 3, ["ALTER TABLE Missing ADD COLUMN Broken TEXT;"]));

        var report = await sut.MigrateAsync(request);

        report.Succeeded.Should().BeFalse();
        report.BackupPath.Should().NotBeNull();
        File.Exists(report.BackupPath).Should().BeTrue();
        report.Issues.Should().Contain(issue => issue.Kind == DatabaseMigrationIssueKind.MigrationFailed);
        fixture.ReadUserVersion().Should().Be(1);
        fixture.ColumnExists("Items", "SortOrder").Should().BeFalse();
    }

    [Fact]
    public async Task MigrateAsync_WhenDatabaseMissing_ReturnsSourceMissing()
    {
        using var fixture = TempDatabaseFolder.Create();
        var sut = new DatabaseMigrationService();

        var report = await sut.MigrateAsync(fixture.Request(targetVersion: 1, dryRun: false));

        report.Succeeded.Should().BeFalse();
        report.Issues.Should().Contain(issue => issue.Kind == DatabaseMigrationIssueKind.SourceMissing);
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
    }

    [Fact]
    public async Task AnalyzeAsync_WhenPathIsDirectory_ReturnsSourceNotFile()
    {
        using var fixture = TempDatabaseFolder.Create();
        Directory.CreateDirectory(fixture.DatabasePath);
        var sut = new DatabaseMigrationService();

        var analysis = await sut.AnalyzeAsync(fixture.DatabasePath);

        analysis.Succeeded.Should().BeFalse();
        analysis.Issues.Should().Contain(issue => issue.Kind == DatabaseMigrationIssueKind.SourceNotFile);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenFileIsCorrupt_ReturnsOpenFailedIssue()
    {
        using var fixture = TempDatabaseFolder.Create();
        Directory.CreateDirectory(fixture.Root);
        await File.WriteAllTextAsync(fixture.DatabasePath, "not sqlite");
        var sut = new DatabaseMigrationService();

        var analysis = await sut.AnalyzeAsync(fixture.DatabasePath);

        analysis.Succeeded.Should().BeFalse();
        analysis.Issues.Should().Contain(issue => issue.Kind == DatabaseMigrationIssueKind.OpenFailed);
    }

    private sealed class TempDatabaseFolder : IDisposable
    {
        private TempDatabaseFolder(string root)
        {
            Root = root;
            DatabasePath = Path.Combine(root, "AdminDB.sqlite");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string DatabasePath { get; }

        public string BackupRoot { get; }

        public static TempDatabaseFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_Db_{Guid.NewGuid():N}"));

        public DatabaseMigrationRequest Request(
            int targetVersion,
            bool dryRun,
            params DatabaseMigrationStep[] steps)
            => new(DatabasePath, BackupRoot, targetVersion, steps, dryRun);

        public void CreateDatabase(int version, params string[] commands)
        {
            Directory.CreateDirectory(Root);
            using var connection = Open(DatabasePath);
            Execute(connection, $"PRAGMA user_version = {version};");
            foreach (var command in commands)
            {
                Execute(connection, command);
            }
        }

        public int ReadUserVersion() => ReadUserVersion(DatabasePath);

        public static int ReadUserVersion(string path)
        {
            using var connection = Open(path);
            using var command = new SQLiteCommand("PRAGMA user_version;", connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool TableExists(string tableName)
        {
            using var connection = Open(DatabasePath);
            using var command = new SQLiteCommand(
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @tableName;",
                connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }

        public bool ColumnExists(string tableName, string columnName)
        {
            using var connection = Open(DatabasePath);
            using var command = new SQLiteCommand($"PRAGMA table_info('{tableName.Replace("'", "''")}');", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(Convert.ToString(reader["name"]), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
