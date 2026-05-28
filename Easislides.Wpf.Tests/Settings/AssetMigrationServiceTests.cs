using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class AssetMigrationServiceTests
{
    [Fact]
    public async Task MigrateAsync_WhenDryRun_ReportsFilesAndHashesWithoutCopying()
    {
        using var fixture = TempMigrationFolder.Create();
        fixture.WriteSourceFile("Backgrounds\\stage.png", "image-bytes");
        fixture.WriteSourceFile("Media\\intro.mp4", "video-bytes");
        var sut = new AssetMigrationService();

        var report = await sut.MigrateAsync(fixture.Request(dryRun: true));

        report.Succeeded.Should().BeTrue();
        report.IsDryRun.Should().BeTrue();
        report.Items.Should().HaveCount(2);
        report.Items.Should().OnlyContain(item => item.Status == AssetMigrationItemStatus.Planned);
        report.Items.Select(item => item.RelativePath).Should().BeEquivalentTo(
            "Backgrounds\\stage.png",
            "Media\\intro.mp4");
        report.Items.Should().OnlyContain(item => item.Sha256.Length == 64);
        Directory.Exists(fixture.DestinationRoot).Should().BeFalse();
        Directory.Exists(fixture.BackupRoot).Should().BeFalse();
    }

    [Fact]
    public async Task MigrateAsync_CopiesFilesVerifiesHashAndWritesReport()
    {
        using var fixture = TempMigrationFolder.Create();
        fixture.WriteSourceFile("Backgrounds\\stage.png", "image-bytes");
        fixture.WriteSourceFile("Media\\intro.mp4", "video-bytes");
        var sut = new AssetMigrationService();

        var report = await sut.MigrateAsync(fixture.Request());

        report.Succeeded.Should().BeTrue();
        report.IsDryRun.Should().BeFalse();
        report.BackupDirectory.Should().NotBeNull();
        File.ReadAllText(Path.Combine(fixture.DestinationRoot, "Backgrounds\\stage.png")).Should().Be("image-bytes");
        File.ReadAllText(Path.Combine(fixture.DestinationRoot, "Media\\intro.mp4")).Should().Be("video-bytes");
        report.Items.Should().OnlyContain(item => item.Status == AssetMigrationItemStatus.Copied);
        foreach (var item in report.Items)
        {
            HashFile(item.DestinationPath).Should().Be(item.Sha256);
        }

        File.Exists(Path.Combine(report.BackupDirectory!, "asset-migration-report.json")).Should().BeTrue();
    }

    [Fact]
    public async Task MigrateAsync_WhenDestinationFileDiffers_PreservesExistingAndCopiesWithSafeName()
    {
        using var fixture = TempMigrationFolder.Create();
        fixture.WriteSourceFile("Backgrounds\\stage.png", "new-image");
        fixture.WriteDestinationFile("Backgrounds\\stage.png", "existing-image");
        var sut = new AssetMigrationService();

        var report = await sut.MigrateAsync(fixture.Request());

        report.Succeeded.Should().BeTrue();
        File.ReadAllText(Path.Combine(fixture.DestinationRoot, "Backgrounds\\stage.png")).Should().Be("existing-image");
        var migratedPath = Path.Combine(fixture.DestinationRoot, "Backgrounds\\stage (migrated 1).png");
        File.ReadAllText(migratedPath).Should().Be("new-image");
        report.Items.Should().ContainSingle().Which.Should().Match<AssetMigrationItem>(item =>
            item.Status == AssetMigrationItemStatus.CopiedWithConflictRename &&
            item.DestinationPath == migratedPath);
        report.Issues.Should().ContainSingle(issue =>
            issue.Kind == AssetMigrationIssueKind.DestinationConflict &&
            issue.Severity == AssetMigrationIssueSeverity.Warning);
    }

    [Fact]
    public async Task MigrateAsync_WhenSourceIsMissing_ReturnsFailedReport()
    {
        using var fixture = TempMigrationFolder.Create();
        var sut = new AssetMigrationService();

        var report = await sut.MigrateAsync(new AssetMigrationRequest(
            Path.Combine(fixture.Root, "missing"),
            fixture.DestinationRoot,
            fixture.BackupRoot));

        report.Succeeded.Should().BeFalse();
        report.Items.Should().BeEmpty();
        report.Issues.Should().ContainSingle(issue =>
            issue.Kind == AssetMigrationIssueKind.SourceMissing &&
            issue.Severity == AssetMigrationIssueSeverity.Error);
    }

    [Fact]
    public async Task MigrateAsync_WhenSourceIsFile_ReturnsSourceNotDirectory()
    {
        using var fixture = TempMigrationFolder.Create();
        var sourceFile = Path.Combine(fixture.Root, "source-file.txt");
        File.WriteAllText(sourceFile, "not a directory");
        var sut = new AssetMigrationService();

        var report = await sut.MigrateAsync(new AssetMigrationRequest(
            sourceFile,
            fixture.DestinationRoot,
            fixture.BackupRoot));

        report.Succeeded.Should().BeFalse();
        report.Items.Should().BeEmpty();
        report.Issues.Should().ContainSingle(issue =>
            issue.Kind == AssetMigrationIssueKind.SourceNotDirectory &&
            issue.Severity == AssetMigrationIssueSeverity.Error);
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private sealed class TempMigrationFolder : IDisposable
    {
        private TempMigrationFolder(string root)
        {
            Root = root;
            SourceRoot = Path.Combine(root, "source");
            DestinationRoot = Path.Combine(root, "destination");
            BackupRoot = Path.Combine(root, "backups");
            Directory.CreateDirectory(SourceRoot);
        }

        public string Root { get; }

        public string SourceRoot { get; }

        public string DestinationRoot { get; }

        public string BackupRoot { get; }

        public static TempMigrationFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlidesAssetMigration_{Guid.NewGuid():N}"));

        public AssetMigrationRequest Request(bool dryRun = false)
            => new(SourceRoot, DestinationRoot, BackupRoot, dryRun);

        public void WriteSourceFile(string relativePath, string content)
            => WriteFile(SourceRoot, relativePath, content);

        public void WriteDestinationFile(string relativePath, string content)
            => WriteFile(DestinationRoot, relativePath, content);

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

        private static void WriteFile(string root, string relativePath, string content)
        {
            var path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
