using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Settings;

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
    AxicultusSettingsSnapshot SettingsSnapshot,
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

public sealed class OperationalDataRehearsalService(
    ISettingsService settings,
    IAssetMigrationService assetMigration,
    IAdminDatabaseRepository adminDatabase) : IOperationalDataRehearsalService
{
    private readonly ISettingsService _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly IAssetMigrationService _assetMigration = assetMigration ?? throw new ArgumentNullException(nameof(assetMigration));
    private readonly IAdminDatabaseRepository _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));

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
            ProductStorage.SongDatabase(sourceRoot)));
        var issues = new List<OperationalDataRehearsalIssue>();

        if (File.Exists(sourceRoot))
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.WorkingFolderNotDirectory,
                OperationalDataRehearsalIssueSeverity.Error,
                sourceRoot,
                UiText.Get(nameof(OperationalDataRehearsalService), "WorkingFolderIsFile")));

            return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, null, null, issues);
        }

        if (!Directory.Exists(sourceRoot))
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.WorkingFolderMissing,
                OperationalDataRehearsalIssueSeverity.Error,
                sourceRoot,
                UiText.Get(nameof(OperationalDataRehearsalService), "WorkingFolderMissing")));

            return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, null, null, issues);
        }

        if (string.IsNullOrWhiteSpace(backupRoot))
        {
            issues.Add(new OperationalDataRehearsalIssue(
                OperationalDataRehearsalIssueKind.BackupRootMissing,
                OperationalDataRehearsalIssueSeverity.Error,
                backupRoot,
                UiText.Get(nameof(OperationalDataRehearsalService), "BackupRootRequired")));

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
                    ? UiText.Get(nameof(OperationalDataRehearsalService), "AdminDbIncompatible")
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
                UiText.Get(nameof(OperationalDataRehearsalService), "AdminDbMissing")));
        }

        return CreateReport(snapshot, sourceRoot, destinationRoot, backupRoot, adminDatabasePath, assetReport, databaseInventory, issues);
    }

    private static OperationalDataRehearsalReport CreateReport(
        AxicultusSettingsSnapshot snapshot,
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
    {
        var parent = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var legacy = Path.Combine(parent, ProductStorage.LegacyName);
        var current = Path.Combine(parent, "Axicultuss");
        if (Directory.Exists(current)) return current;
        var intermediate = Path.Combine(parent, ProductStorage.ProductName);
        if (Directory.Exists(Path.Combine(intermediate, "Admin"))) return intermediate;
        return Directory.Exists(legacy) ? legacy : current;
    }

    private static string DefaultUserAssetsRoot()
        => Path.Combine(ProductStorage.WpfProfileDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "UserAssets");

    private static string DefaultBackupRoot()
        => Path.Combine(ProductStorage.WpfProfileDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "Backups");
}
