using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Easislides.Wpf.Data;

public enum DatabaseMigrationIssueKind
{
    SourceMissing,
    SourceNotFile,
    OpenFailed,
    SchemaAhead,
    NoMigrationPath,
    InvalidMigrationStep,
    BackupFailed,
    MigrationFailed,
    RestoreFailed,
}

public enum DatabaseMigrationIssueSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record DatabaseMigrationIssue(
    DatabaseMigrationIssueKind Kind,
    DatabaseMigrationIssueSeverity Severity,
    string Message);

public sealed record DatabaseTable(string Name, string Sql);

public sealed record DatabaseMigrationStep(
    int FromVersion,
    int ToVersion,
    IReadOnlyList<string> Commands,
    string Description = "");

public sealed record DatabaseMigrationRequest(
    string DatabasePath,
    string BackupRoot,
    int TargetVersion,
    IReadOnlyList<DatabaseMigrationStep> Steps,
    bool IsDryRun = false,
    bool RestoreBackupOnFailure = true);

public sealed record DatabaseMigrationAnalysis(
    bool Succeeded,
    string DatabasePath,
    int SchemaVersion,
    IReadOnlyList<DatabaseTable> Tables,
    IReadOnlyList<DatabaseMigrationIssue> Issues);

public sealed record DatabaseMigrationReport(
    bool Succeeded,
    bool IsDryRun,
    string DatabasePath,
    int SourceVersion,
    int TargetVersion,
    int FinalVersion,
    string? BackupPath,
    IReadOnlyList<DatabaseMigrationStep> PendingSteps,
    IReadOnlyList<DatabaseMigrationStep> AppliedSteps,
    IReadOnlyList<DatabaseMigrationIssue> Issues);

public interface IDatabaseMigrationService
{
    Task<DatabaseMigrationAnalysis> AnalyzeAsync(string databasePath);

    Task<DatabaseMigrationReport> MigrateAsync(DatabaseMigrationRequest request);
}

public sealed class DatabaseMigrationService : IDatabaseMigrationService
{
    public Task<DatabaseMigrationAnalysis> AnalyzeAsync(string databasePath)
        => Task.FromResult(Analyze(databasePath));

    public Task<DatabaseMigrationReport> MigrateAsync(DatabaseMigrationRequest request)
        => Task.FromResult(Migrate(request));

    private static DatabaseMigrationAnalysis Analyze(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return AnalysisFailure(
                databasePath,
                Issue(DatabaseMigrationIssueKind.SourceMissing, "Database path cannot be empty."));
        }

        var fullPath = Path.GetFullPath(databasePath);
        if (Directory.Exists(fullPath))
        {
            return AnalysisFailure(
                fullPath,
                Issue(DatabaseMigrationIssueKind.SourceNotFile, $"Database path is a directory: {fullPath}"));
        }

        if (!File.Exists(fullPath))
        {
            return AnalysisFailure(
                fullPath,
                Issue(DatabaseMigrationIssueKind.SourceMissing, $"Database file does not exist: {fullPath}"));
        }

        try
        {
            using var connection = OpenConnection(fullPath, readOnly: true);
            var schemaVersion = ReadUserVersion(connection);
            var tables = ReadTables(connection);
            return new DatabaseMigrationAnalysis(
                Succeeded: true,
                fullPath,
                schemaVersion,
                tables,
                Array.Empty<DatabaseMigrationIssue>());
        }
        catch (Exception ex) when (IsSqliteOpenException(ex))
        {
            return AnalysisFailure(
                fullPath,
                Issue(DatabaseMigrationIssueKind.OpenFailed, $"Unable to open SQLite database: {ex.Message}"));
        }
    }

    private static DatabaseMigrationReport Migrate(DatabaseMigrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var analysis = Analyze(request.DatabasePath);
        if (!analysis.Succeeded)
        {
            return FailedReport(request, analysis.SchemaVersion, analysis.SchemaVersion, null, analysis.Issues);
        }

        if (request.TargetVersion < analysis.SchemaVersion)
        {
            return FailedReport(
                request,
                analysis.SchemaVersion,
                analysis.SchemaVersion,
                null,
                [Issue(DatabaseMigrationIssueKind.SchemaAhead, "Database schema is newer than this WPF build.")]);
        }

        var pathResult = BuildMigrationPath(analysis.SchemaVersion, request.TargetVersion, request.Steps);
        if (pathResult.Issues.Count > 0)
        {
            return FailedReport(
                request,
                analysis.SchemaVersion,
                analysis.SchemaVersion,
                null,
                pathResult.Issues);
        }

        if (request.IsDryRun || pathResult.Steps.Count == 0)
        {
            return new DatabaseMigrationReport(
                Succeeded: true,
                request.IsDryRun,
                analysis.DatabasePath,
                analysis.SchemaVersion,
                request.TargetVersion,
                analysis.SchemaVersion,
                BackupPath: null,
                PendingSteps: pathResult.Steps,
                AppliedSteps: Array.Empty<DatabaseMigrationStep>(),
                Issues: Array.Empty<DatabaseMigrationIssue>());
        }

        string? backupPath;
        try
        {
            backupPath = CreateBackup(analysis.DatabasePath, request.BackupRoot, analysis.SchemaVersion);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return FailedReport(
                request,
                analysis.SchemaVersion,
                analysis.SchemaVersion,
                null,
                [Issue(DatabaseMigrationIssueKind.BackupFailed, $"Unable to create database backup: {ex.Message}")]);
        }

        var applied = new List<DatabaseMigrationStep>();
        var issues = new List<DatabaseMigrationIssue>();
        try
        {
            using var connection = OpenConnection(analysis.DatabasePath, readOnly: false);
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                foreach (var step in pathResult.Steps)
                {
                    ExecuteStep(connection, transaction, step);
                    SetUserVersion(connection, transaction, step.ToVersion);
                    applied.Add(step);
                }

                transaction.Commit();
            }
            catch
            {
                TryRollback(transaction);
                throw;
            }
        }
        catch (Exception ex) when (IsSqliteOpenException(ex) || ex is InvalidOperationException)
        {
            issues.Add(Issue(DatabaseMigrationIssueKind.MigrationFailed, $"Migration failed: {ex.Message}"));
            if (request.RestoreBackupOnFailure)
            {
                try
                {
                    RestoreBackup(backupPath, analysis.DatabasePath);
                }
                catch (Exception restoreEx) when (IsFileSystemException(restoreEx))
                {
                    issues.Add(Issue(
                        DatabaseMigrationIssueKind.RestoreFailed,
                        $"Backup restore failed: {restoreEx.Message}",
                        DatabaseMigrationIssueSeverity.Warning));
                }
            }

            var finalVersion = SafeReadUserVersion(analysis.DatabasePath, analysis.SchemaVersion);
            return new DatabaseMigrationReport(
                Succeeded: false,
                IsDryRun: false,
                analysis.DatabasePath,
                analysis.SchemaVersion,
                request.TargetVersion,
                finalVersion,
                backupPath,
                PendingSteps: pathResult.Steps,
                AppliedSteps: applied,
                Issues: issues);
        }

        var migrated = Analyze(analysis.DatabasePath);
        return new DatabaseMigrationReport(
            Succeeded: true,
            IsDryRun: false,
            analysis.DatabasePath,
            analysis.SchemaVersion,
            request.TargetVersion,
            migrated.SchemaVersion,
            backupPath,
            PendingSteps: Array.Empty<DatabaseMigrationStep>(),
            AppliedSteps: applied,
            Issues: Array.Empty<DatabaseMigrationIssue>());
    }

    private static (IReadOnlyList<DatabaseMigrationStep> Steps, IReadOnlyList<DatabaseMigrationIssue> Issues)
        BuildMigrationPath(int sourceVersion, int targetVersion, IReadOnlyList<DatabaseMigrationStep> availableSteps)
    {
        var pending = new List<DatabaseMigrationStep>();
        var issues = new List<DatabaseMigrationIssue>();
        var version = sourceVersion;

        while (version < targetVersion)
        {
            var step = availableSteps.FirstOrDefault(candidate => candidate.FromVersion == version);
            if (step is null)
            {
                issues.Add(Issue(
                    DatabaseMigrationIssueKind.NoMigrationPath,
                    $"No migration step starts at schema version {version}."));
                break;
            }

            if (step.ToVersion <= step.FromVersion || step.ToVersion > targetVersion)
            {
                issues.Add(Issue(
                    DatabaseMigrationIssueKind.InvalidMigrationStep,
                    $"Invalid migration step {step.FromVersion}->{step.ToVersion}."));
                break;
            }

            if (step.Commands.Count == 0 || step.Commands.Any(string.IsNullOrWhiteSpace))
            {
                issues.Add(Issue(
                    DatabaseMigrationIssueKind.InvalidMigrationStep,
                    $"Migration step {step.FromVersion}->{step.ToVersion} has no SQL command."));
                break;
            }

            pending.Add(step);
            version = step.ToVersion;
        }

        return (pending, issues);
    }

    private static string CreateBackup(string databasePath, string backupRoot, int sourceVersion)
    {
        if (string.IsNullOrWhiteSpace(backupRoot))
        {
            throw new IOException("Backup root cannot be empty.");
        }

        Directory.CreateDirectory(backupRoot);
        var fileName = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        var backupPath = Path.Combine(
            backupRoot,
            $"{fileName}.schema{sourceVersion}.{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}{extension}");
        File.Copy(databasePath, backupPath, overwrite: false);
        CopySidecarIfExists(databasePath, backupPath, "-wal");
        CopySidecarIfExists(databasePath, backupPath, "-shm");
        return backupPath;
    }

    private static void RestoreBackup(string backupPath, string databasePath)
    {
        File.Copy(backupPath, databasePath, overwrite: true);
        RestoreSidecar(backupPath, databasePath, "-wal");
        RestoreSidecar(backupPath, databasePath, "-shm");
    }

    private static void RestoreSidecar(string backupPath, string databasePath, string suffix)
    {
        var backupSidecar = backupPath + suffix;
        var databaseSidecar = databasePath + suffix;
        if (File.Exists(backupSidecar))
        {
            File.Copy(backupSidecar, databaseSidecar, overwrite: true);
            return;
        }

        if (File.Exists(databaseSidecar))
        {
            File.Delete(databaseSidecar);
        }
    }

    private static void CopySidecarIfExists(string databasePath, string backupPath, string suffix)
    {
        var source = databasePath + suffix;
        if (!File.Exists(source))
        {
            return;
        }

        File.Copy(source, backupPath + suffix, overwrite: false);
    }

    private static SQLiteConnection OpenConnection(string path, bool readOnly)
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = path,
            Version = 3,
            ReadOnly = readOnly,
            ForeignKeys = true,
        };
        var connection = new SQLiteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static int ReadUserVersion(SQLiteConnection connection)
    {
        using var command = new SQLiteCommand("PRAGMA user_version;", connection);
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static int SafeReadUserVersion(string databasePath, int fallback)
    {
        try
        {
            using var connection = OpenConnection(databasePath, readOnly: true);
            return ReadUserVersion(connection);
        }
        catch
        {
            return fallback;
        }
    }

    private static void SetUserVersion(SQLiteConnection connection, SQLiteTransaction transaction, int version)
    {
        using var command = new SQLiteCommand($"PRAGMA user_version = {version};", connection, transaction);
        command.ExecuteNonQuery();
    }

    private static IReadOnlyList<DatabaseTable> ReadTables(SQLiteConnection connection)
    {
        using var command = new SQLiteCommand(
            "SELECT name, sql FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;",
            connection);
        using var reader = command.ExecuteReader();
        var tables = new List<DatabaseTable>();
        while (reader.Read())
        {
            tables.Add(new DatabaseTable(
                Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["sql"], CultureInfo.InvariantCulture) ?? ""));
        }

        return tables;
    }

    private static void ExecuteStep(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        DatabaseMigrationStep step)
    {
        foreach (var sql in step.Commands)
        {
            using var command = new SQLiteCommand(sql, connection, transaction);
            command.ExecuteNonQuery();
        }
    }

    private static void TryRollback(SQLiteTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
        }
    }

    private static DatabaseMigrationAnalysis AnalysisFailure(
        string databasePath,
        params DatabaseMigrationIssue[] issues)
        => new(
            Succeeded: false,
            databasePath,
            SchemaVersion: 0,
            Tables: Array.Empty<DatabaseTable>(),
            Issues: issues);

    private static DatabaseMigrationReport FailedReport(
        DatabaseMigrationRequest request,
        int sourceVersion,
        int finalVersion,
        string? backupPath,
        IReadOnlyList<DatabaseMigrationIssue> issues)
        => new(
            Succeeded: false,
            request.IsDryRun,
            request.DatabasePath,
            sourceVersion,
            request.TargetVersion,
            finalVersion,
            backupPath,
            PendingSteps: Array.Empty<DatabaseMigrationStep>(),
            AppliedSteps: Array.Empty<DatabaseMigrationStep>(),
            Issues: issues);

    private static DatabaseMigrationIssue Issue(
        DatabaseMigrationIssueKind kind,
        string message,
        DatabaseMigrationIssueSeverity severity = DatabaseMigrationIssueSeverity.Error)
        => new(kind, severity, message);

    private static bool IsSqliteOpenException(Exception ex)
        => ex is SQLiteException or InvalidOperationException or IOException or UnauthorizedAccessException;

    private static bool IsFileSystemException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or NotSupportedException;
}
