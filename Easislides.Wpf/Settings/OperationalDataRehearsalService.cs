using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Settings;

public enum OperationalDataRehearsalIssueKind
{
    WorkingFolderMissing,
    WorkingFolderNotDirectory,
    BackupRootMissing,
    AdminDatabaseMissing,
    AdminDatabaseIncompatible,
    AssetMigrationFailed,
}

public enum OperationalDataRehearsalIssueSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record OperationalDataRehearsalIssue(
    OperationalDataRehearsalIssueKind Kind,
    OperationalDataRehearsalIssueSeverity Severity,
    string Path,
    string Message);

public sealed record OperationalDataRehearsalRequest(
    string? SourceRoot = null,
    string? DestinationRoot = null,
    string? BackupRoot = null,
    string? AdminDatabasePath = null);

public sealed record OperationalDataRehearsalReport(
    bool Succeeded,
    EasiSettingsSnapshot SettingsSnapshot,
    string SourceRoot,
    string DestinationRoot,
    string BackupRoot,
    string AdminDatabasePath,
    AssetMigrationReport? AssetReport,
    AdminDatabaseSchemaInventory? DatabaseInventory,
    IReadOnlyList<OperationalDataRehearsalIssue> Issues);

public interface IOperationalDataRehearsalService
{
    Task<OperationalDataRehearsalReport> RunAsync(
        OperationalDataRehearsalRequest? request = null,
        CancellationToken cancellationToken = default);
}

public sealed class OperationalDataRehearsalService : IOperationalDataRehearsalService
{
    private const string LegacyAdminDatabaseRelativePath = @"Admin\Database\EasiSlidesDb.db";

    private readonly ISettingsService _settings;
    private readonly IAssetMigrationService _assetMigration;
    private readonly IAdminDatabaseRepository _adminDatabase;

    public OperationalDataRehearsalService(
        ISettingsService settings,
        IAssetMigrationService assetMigration,
        IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _assetMigration = assetMigration ?? throw new ArgumentNullException(nameof(assetMigration));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
    }

    public async Task<OperationalDataRehearsalReport> RunAsync(
        OperationalDataRehearsalRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new OperationalDataRehearsalRequest();
        var snapshot = _settings.Current;
        var sourceRoot = NormalizeRoot(FirstNonEmpty(request.SourceRoot, snapshot.General.WorkingFolder, DefaultLegacyRoot()));
        var destinationRoot = NormalizeRoot(FirstNonEmpty(request.DestinationRoot, DefaultUserAssetsRoot()));
        var backupRoot = NormalizeRoot(FirstNonEmpty(request.BackupRoot, snapshot.Data.BackupRoot, DefaultBackupRoot()));
        var adminDatabasePath = NormalizePath(FirstNonEmpty(
            request.AdminDatabasePath,
            snapshot.Data.AdminDatabasePath,
            Path.Combine(sourceRoot, LegacyAdminDatabaseRelativePath)));
        var issues = new List<OperationalDataRehearsalIssue>();

        if (File.Exists(sourceRoot))
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.WorkingFolderNotDirectory,
                OperationalDataRehearsalIssueSeverity.Error,
                sourceRoot,
                "Working folder points to a file, not a directory."));

            return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, null, null, issues);
        }

        if (!Directory.Exists(sourceRoot))
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.WorkingFolderMissing,
                OperationalDataRehearsalIssueSeverity.Error,
                sourceRoot,
                "Working folder does not exist."));

            return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, null, null, issues);
        }

        if (string.IsNullOrWhiteSpace(backupRoot))
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.BackupRootMissing,
                OperationalDataRehearsalIssueSeverity.Error,
                backupRoot,
                "Backup root is required for an operational data rehearsal."));

            return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, null, null, issues);
        }

        var assetReport = await _assetMigration
            .MigrateAsync(new AssetMigrationRequest(sourceRoot, destinationRoot, backupRoot, DryRun: true), cancellationToken)
            .ConfigureAwait(false);

        if (!assetReport.Succeeded)
        {
            foreach (var assetIssue in assetReport.Issues.Where(issue => issue.Severity == AssetMigrationIssueSeverity.Error))
            {
                issues.Add(new OperationalDataRehearsalIssue(
                    OperationalDataRehearsalIssueKind.AssetMigrationFailed,
                    OperationalDataRehearsalIssueSeverity.Error,
                    assetIssue.Path,
                    assetIssue.Message));
            }
        }

        AdminDatabaseSchemaInventory? databaseInventory = null;
        if (File.Exists(adminDatabasePath))
        {
            databaseInventory = await _adminDatabase
                .AnalyzeSchemaAsync(adminDatabasePath)
                .ConfigureAwait(false);

            if (!databaseInventory.Succeeded)
            {
                var message = databaseInventory.Issues.Count == 0
                    ? "Admin database is not compatible with the expected legacy schema."
                    : string.Join(" ", databaseInventory.Issues.Select(issue => issue.Message));
                issues.Add(new OperationalDataRehearsalIssue(
                    OperationalDataRehearsalIssueKind.AdminDatabaseIncompatible,
                    OperationalDataRehearsalIssueSeverity.Error,
                    adminDatabasePath,
                    message));
            }
        }
        else
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.AdminDatabaseMissing,
                OperationalDataRehearsalIssueSeverity.Warning,
                adminDatabasePath,
                "Admin database was not found; asset rehearsal can continue without database inventory."));
        }

        return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, assetReport, databaseInventory, issues);
    }

    private static OperationalDataRehearsalReport CreateReport(
        EasiSettingsSnapshot snapshot,
        string sourceRoot,
        string destinationRoot,
        string backupRoot,
        string adminDatabasePath,
        AssetMigrationReport? assetReport,
        AdminDatabaseSchemaInventory? databaseInventory,
        IReadOnlyList<OperationalDataRehearsalIssue> issues)
        => new(
            !issues.Any(issue => issue.Severity == OperationalDataRehearsalIssueSeverity.Error),
            snapshot,
            sourceRoot,
            destinationRoot,
            backupRoot,
            adminDatabasePath,
            assetReport,
            databaseInventory,
            issues.ToArray());

    private static string FirstNonEmpty(params string?[] values)
        => values.First(value => !string.IsNullOrWhiteSpace(value))!;

    private static string NormalizeRoot(string path) => Path.GetFullPath(path);

    private static string NormalizePath(string path) => Path.GetFullPath(path);

    private static string DefaultLegacyRoot()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Easislides");

    private static string DefaultUserAssetsRoot()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasislidesNext", "UserAssets");

    private static string DefaultBackupRoot()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasislidesNext", "Backups");
}
