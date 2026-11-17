using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides.Wpf.Settings;

public enum AssetMigrationItemStatus
{
    Planned,
    Copied,
    SkippedExisting,
    CopiedWithConflictRename,
    Failed
}

public enum AssetMigrationIssueKind
{
    SourceMissing,
    SourceNotDirectory,
    DestinationConflict,
    CopyFailed,
    HashMismatch,
    ReportWriteFailed
}

public enum AssetMigrationIssueSeverity
{
    Info,
    Warning,
    Error
}

public sealed record AssetMigrationRequest(
    string SourceRoot,
    string DestinationRoot,
    string BackupRoot,
    bool DryRun = false);

public sealed record AssetMigrationItem(
    string SourcePath,
    string DestinationPath,
    string RelativePath,
    long Length,
    string Sha256,
    AssetMigrationItemStatus Status,
    string? Message = null);

public sealed record AssetMigrationIssue(
    AssetMigrationIssueKind Kind,
    AssetMigrationIssueSeverity Severity,
    string Path,
    string Message);

public sealed record AssetMigrationReport(
    bool Succeeded,
    bool IsDryRun,
    string SourceRoot,
    string DestinationRoot,
    string BackupRoot,
    string? BackupDirectory,
    IReadOnlyList<AssetMigrationItem> Items,
    IReadOnlyList<AssetMigrationIssue> Issues);

public interface IAssetMigrationService
{
    Task<AssetMigrationReport> MigrateAsync(
        AssetMigrationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class AssetMigrationService : IAssetMigrationService
{
    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<AssetMigrationReport> MigrateAsync(
        AssetMigrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DestinationRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.BackupRoot);

        var sourceRoot = NormalizeRoot(request.SourceRoot);
        var destinationRoot = NormalizeRoot(request.DestinationRoot);
        var backupRoot = NormalizeRoot(request.BackupRoot);
        var items = new List<AssetMigrationItem>();
        var issues = new List<AssetMigrationIssue>();

        if (File.Exists(sourceRoot))
        {
            issues.Add(new AssetMigrationIssue(
                AssetMigrationIssueKind.SourceNotDirectory,
                AssetMigrationIssueSeverity.Error,
                sourceRoot,
                "Source root is not a directory."));

            return CreateReport(request, sourceRoot, destinationRoot, backupRoot, backupDirectory: null, items, issues);
        }

        if (!Directory.Exists(sourceRoot))
        {
            issues.Add(new AssetMigrationIssue(
                AssetMigrationIssueKind.SourceMissing,
                AssetMigrationIssueSeverity.Error,
                sourceRoot,
                "Source root does not exist."));

            return CreateReport(request, sourceRoot, destinationRoot, backupRoot, backupDirectory: null, items, issues);
        }

        var backupDirectory = request.DryRun ? null : CreateBackupDirectory(backupRoot);
        foreach (var sourcePath in EnumerateSourceFiles(sourceRoot, destinationRoot, backupRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var preferredDestinationPath = Path.Combine(destinationRoot, relativePath);
            var sourceHash = await ComputeSha256Async(sourcePath, cancellationToken).ConfigureAwait(false);
            var sourceInfo = new FileInfo(sourcePath);

            if (request.DryRun)
            {
                items.Add(new AssetMigrationItem(
                    sourcePath,
                    preferredDestinationPath,
                    relativePath,
                    sourceInfo.Length,
                    sourceHash,
                    AssetMigrationItemStatus.Planned));
                continue;
            }

            var destinationPath = preferredDestinationPath;
            var status = AssetMigrationItemStatus.Copied;
            try
            {
                if (File.Exists(destinationPath))
                {
                    var destinationHash = await ComputeSha256Async(destinationPath, cancellationToken).ConfigureAwait(false);
                    if (StringComparer.OrdinalIgnoreCase.Equals(destinationHash, sourceHash))
                    {
                        items.Add(new AssetMigrationItem(
                            sourcePath,
                            destinationPath,
                            relativePath,
                            sourceInfo.Length,
                            sourceHash,
                            AssetMigrationItemStatus.SkippedExisting,
                            "Destination already contains the same file."));
                        continue;
                    }

                    destinationPath = CreateConflictDestinationPath(destinationPath);
                    status = AssetMigrationItemStatus.CopiedWithConflictRename;
                    issues.Add(new AssetMigrationIssue(
                        AssetMigrationIssueKind.DestinationConflict,
                        AssetMigrationIssueSeverity.Warning,
                        preferredDestinationPath,
                        $"Destination already exists. Copied as {Path.GetFileName(destinationPath)}."));
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                File.Copy(sourcePath, destinationPath, overwrite: false);

                var copiedHash = await ComputeSha256Async(destinationPath, cancellationToken).ConfigureAwait(false);
                if (!StringComparer.OrdinalIgnoreCase.Equals(copiedHash, sourceHash))
                {
                    status = AssetMigrationItemStatus.Failed;
                    issues.Add(new AssetMigrationIssue(
                        AssetMigrationIssueKind.HashMismatch,
                        AssetMigrationIssueSeverity.Error,
                        destinationPath,
                        "Copied file hash does not match source hash."));
                }

                items.Add(new AssetMigrationItem(
                    sourcePath,
                    destinationPath,
                    relativePath,
                    sourceInfo.Length,
                    sourceHash,
                    status));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                issues.Add(new AssetMigrationIssue(
                    AssetMigrationIssueKind.CopyFailed,
                    AssetMigrationIssueSeverity.Error,
                    sourcePath,
                    ex.Message));
                items.Add(new AssetMigrationItem(
                    sourcePath,
                    destinationPath,
                    relativePath,
                    sourceInfo.Length,
                    sourceHash,
                    AssetMigrationItemStatus.Failed,
                    ex.Message));
            }
        }

        var report = CreateReport(request, sourceRoot, destinationRoot, backupRoot, backupDirectory, items, issues);
        if (!request.DryRun && backupDirectory is not null)
        {
            report = await WriteReportAsync(report, backupDirectory, issues, cancellationToken).ConfigureAwait(false);
        }

        return report;
    }

    private static AssetMigrationReport CreateReport(
        AssetMigrationRequest request,
        string sourceRoot,
        string destinationRoot,
        string backupRoot,
        string? backupDirectory,
        IReadOnlyList<AssetMigrationItem> items,
        IReadOnlyList<AssetMigrationIssue> issues)
        => new(
            !issues.Any(issue => issue.Severity == AssetMigrationIssueSeverity.Error),
            request.DryRun,
            sourceRoot,
            destinationRoot,
            backupRoot,
            backupDirectory,
            items.ToArray(),
            issues.ToArray());

    private static async Task<AssetMigrationReport> WriteReportAsync(
        AssetMigrationReport report,
        string backupDirectory,
        List<AssetMigrationIssue> issues,
        CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(backupDirectory);
            var reportPath = Path.Combine(backupDirectory, "asset-migration-report.json");
            await using var stream = File.Create(reportPath);
            await JsonSerializer.SerializeAsync(stream, report, ReportJsonOptions, cancellationToken).ConfigureAwait(false);
            return report;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            issues.Add(new AssetMigrationIssue(
                AssetMigrationIssueKind.ReportWriteFailed,
                AssetMigrationIssueSeverity.Error,
                backupDirectory,
                ex.Message));

            return report with
            {
                Succeeded = false,
                Issues = issues.ToArray()
            };
        }
    }

    private static IEnumerable<string> EnumerateSourceFiles(
        string sourceRoot,
        string destinationRoot,
        string backupRoot)
        => Directory
            .EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .Where(path => !IsUnderDirectory(path, destinationRoot))
            .Where(path => !IsUnderDirectory(path, backupRoot))
            .OrderBy(path => Path.GetRelativePath(sourceRoot, path), StringComparer.OrdinalIgnoreCase);

    private static string CreateBackupDirectory(string backupRoot)
    {
        var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var directory = Path.Combine(backupRoot, stamp);
        var suffix = 1;
        while (Directory.Exists(directory))
        {
            directory = Path.Combine(backupRoot, $"{stamp}-{suffix++}");
        }

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string CreateConflictDestinationPath(string preferredPath)
    {
        var directory = Path.GetDirectoryName(preferredPath)!;
        var name = Path.GetFileNameWithoutExtension(preferredPath);
        var extension = Path.GetExtension(preferredPath);
        var index = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(directory, $"{name} (migrated {index++}){extension}");
        }
        while (File.Exists(candidate));

        return candidate;
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool IsUnderDirectory(string path, string directory)
    {
        var fullPath = Path.GetFullPath(path);
        var fullDirectory = EnsureTrailingSeparator(Path.GetFullPath(directory));
        return fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRoot(string path) => Path.GetFullPath(path);

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}
