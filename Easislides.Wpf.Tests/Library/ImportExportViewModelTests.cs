using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class ImportExportViewModelTests
{
    [Fact]
    public async Task LoadAsync_DerivesPathsAndLoadsFolders()
    {
        using var fixture = new ImportExportViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);

        var sut = fixture.CreateViewModel();

        await sut.LoadAsync();

        sut.WorkingFolder.Should().Be(Path.GetFullPath(fixture.WorkingFolder));
        sut.DatabasePath.Should().Be(Path.GetFullPath(fixture.AdminDatabasePath));
        sut.BackupRoot.Should().Be(Path.GetFullPath(fixture.BackupRoot));
        sut.Folders.Select(folder => folder.Name).Should().Equal("Morning", "Evening");
        sut.SelectedTargetFolder.Should().Be(sut.Folders[0]);
        sut.SelectedExportFolder.Should().Be(sut.Folders[0]);
    }

    [Fact]
    public async Task PreviewAndImportAsync_UsesSelectedFolderDuplicatePolicyAndSourceFolders()
    {
        using var fixture = new ImportExportViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.ImportSourcePath = fixture.ImportSourcePath;

        await sut.PreviewImportAsync();
        sut.ImportSourceFolders[1].IsSelected = false;
        sut.ImportDuplicatePolicy = ImportDuplicatePolicy.ReplaceExisting;

        await sut.ImportAsync();

        fixture.Service.LastImportRequest.Should().NotBeNull();
        fixture.Service.LastImportRequest!.TargetFolderNo.Should().Be(2);
        fixture.Service.LastImportRequest.DuplicatePolicy.Should().Be(ImportDuplicatePolicy.ReplaceExisting);
        fixture.Service.LastImportRequest.SelectedSourceFolders.Should().Equal("Legacy A");
        sut.StatusMessage.Should().Contain("1");
    }

    [Fact]
    public async Task PreviewAndImportAsync_WhenAccessSource_UsesHelperMapping()
    {
        using var fixture = new ImportExportViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        fixture.ImportSourcePath = Path.Combine(fixture.Root, "songs.mdb");
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.ImportSourcePath = fixture.ImportSourcePath;

        await sut.LoadAccessSchemaAsync();
        sut.AccessTitleColumn = "Name";
        sut.AccessLyricsColumnsText = "Verse1, Chorus";
        await sut.PreviewImportAsync();
        await sut.ImportAsync();

        fixture.Service.SchemaRequestedPath.Should().Be(fixture.ImportSourcePath);
        fixture.Service.LastPreviewAccessMapping.Should().NotBeNull();
        fixture.Service.LastPreviewAccessMapping!.TableName.Should().Be("Songs");
        fixture.Service.LastPreviewAccessMapping.TitleColumn.Should().Be("Name");
        fixture.Service.LastPreviewAccessMapping.LyricsColumns.Should().Equal("Verse1", "Chorus");
        fixture.Service.LastImportRequest.Should().NotBeNull();
        fixture.Service.LastImportRequest!.AccessMapping.Should().Be(fixture.Service.LastPreviewAccessMapping);
    }

    [Fact]
    public async Task ImportSourcePathChanged_WhenAccessPathChanges_ClearsLoadedSchema()
    {
        using var fixture = new ImportExportViewModelFixture();
        var sut = fixture.CreateViewModel();
        sut.ImportSourcePath = Path.Combine(fixture.Root, "songs-a.mdb");
        await sut.LoadAccessSchemaAsync();

        sut.AccessTables.Should().NotBeEmpty();
        sut.AccessTitleColumn.Should().Be("Name");

        sut.ImportSourcePath = Path.Combine(fixture.Root, "songs-b.mdb");

        sut.IsAccessImportSource.Should().BeTrue();
        sut.AccessTables.Should().BeEmpty();
        sut.AccessColumns.Should().BeEmpty();
        sut.SelectedAccessTable.Should().BeNull();
        sut.AccessTitleColumn.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshExportCandidatesAndExportAsync_UsesCheckedSongsAndFormat()
    {
        using var fixture = new ImportExportViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.ExportOutputPath = Path.Combine(fixture.Root, "export.esn");
        sut.SelectedExportFormat = ExportFormat.EasiSlidesText;

        await sut.RefreshExportCandidatesAsync();
        sut.ExportCandidates[1].IsSelected = false;
        await sut.ExportAsync();

        fixture.Service.LastExportRequest.Should().NotBeNull();
        fixture.Service.LastExportRequest!.SongIds.Should().Equal(10);
        fixture.Service.LastExportRequest.FolderNos.Should().Equal(2);
        fixture.Service.LastExportRequest.Format.Should().Be(ExportFormat.EasiSlidesText);
        fixture.Service.LastExportRequest.OutputPath.Should().Be(sut.ExportOutputPath);
        sut.StatusMessage.Should().Contain("1");
    }

    private sealed class ImportExportViewModelFixture : IDisposable
    {
        public ImportExportViewModelFixture()
        {
            Root = Path.Combine(Path.GetTempPath(), $"EasiSlides_ImportExportVm_{Guid.NewGuid():N}");
            WorkingFolder = Path.Combine(Root, "Work");
            BackupRoot = Path.Combine(Root, "Backups");
            AdminDatabasePath = Path.Combine(WorkingFolder, "Admin", "Database", "EasiSlidesDb.db");
            ImportSourcePath = Path.Combine(Root, "import.esn");
            Directory.CreateDirectory(Path.GetDirectoryName(AdminDatabasePath)!);
            File.WriteAllText(AdminDatabasePath, "");
            Directory.CreateDirectory(WorkingFolder);
            Settings = new SettingsService(new SettingsServiceOptions(Path.Combine(Root, "settings.json"), BackupRoot));
            Service = new FakeImportExportService();
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public string BackupRoot { get; }

        public string AdminDatabasePath { get; }

        public string ImportSourcePath { get; set; }

        public SettingsService Settings { get; }

        public FakeImportExportService Service { get; }

        public ImportExportViewModel CreateViewModel()
            => new(Settings, Service);

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

    private sealed class FakeImportExportService : IImportExportService
    {
        public ImportRequest? LastImportRequest { get; private set; }

        public ExportRequest? LastExportRequest { get; private set; }

        public string SchemaRequestedPath { get; private set; } = "";

        public AccessImportMapping? LastPreviewAccessMapping { get; private set; }

        public Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath)
            => Task.FromResult<IReadOnlyList<SongFolderSummary>>(
            [
                new SongFolderSummary(2, "Morning", true, 2),
                new SongFolderSummary(3, "Evening", true, 0),
            ]);

        public Task<AccessImportSchema> GetAccessSchemaAsync(string sourcePath)
        {
            SchemaRequestedPath = sourcePath;
            return Task.FromResult(new AccessImportSchema(
                true,
                sourcePath,
                [new AccessImportTable("Songs", ["Name", "Verse1", "Chorus"], 1)],
                new AccessImportMapping("Songs", "Name", ["Verse1"]),
                []));
        }

        public Task<ImportPreview> PreviewImportAsync(string sourcePath, AccessImportMapping? accessMapping = null)
        {
            LastPreviewAccessMapping = accessMapping;
            return Task.FromResult(new ImportPreview(
                true,
                sourcePath,
                accessMapping is null ? ImportSourceKind.EasiSlidesText : ImportSourceKind.AccessDatabase,
                2,
                [new ImportSourceFolder("Legacy A", 1), new ImportSourceFolder("Legacy B", 1)],
                []));
        }

        public Task<ImportReport> ImportAsync(ImportRequest request)
        {
            LastImportRequest = request;
            return Task.FromResult(new ImportReport(
                true,
                request.SourcePath,
                ImportedNew: 1,
                Replaced: 0,
                Skipped: 0,
                Failed: 0,
                [new ImportResultItem("Alpha", ImportResultKind.Inserted, "Imported")],
                []));
        }

        public Task<IReadOnlyList<ExportSongCandidate>> GetExportCandidatesAsync(
            string databasePath,
            IReadOnlyList<int> folderNos,
            DateOnly? modifiedFrom = null,
            DateOnly? modifiedTo = null)
            => Task.FromResult<IReadOnlyList<ExportSongCandidate>>(
            [
                new ExportSongCandidate(10, "Alpha", 2, "Morning", 1),
                new ExportSongCandidate(11, "Beta", 2, "Morning", 2),
            ]);

        public Task<ExportReport> ExportAsync(ExportRequest request)
        {
            LastExportRequest = request;
            return Task.FromResult(new ExportReport(true, request.OutputPath, request.Format, 1, []));
        }

        public string GetDefaultExportPath(string workingFolder, DateOnly date, ExportFormat format)
            => Path.Combine(workingFolder, "Documents", $"Export_{date:yyyy-MM-dd}.xml");
    }
}
