using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class ExternalFileOperationViewModelTests
{
    [Fact]
    public async Task LoadAsync_DerivesLegacyFoldersAndAdminSongFolders()
    {
        using var fixture = TempExternalOperationSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        fixture.CreateAdminDatabase();
        var service = new FakeExternalFileOperationService
        {
            Folders =
            [
                new ExternalFileFolder(0, "InfoScreen Items", fixture.InfoScreensRoot + Path.DirectorySeparatorChar),
                new ExternalFileFolder(1, "\\Notices", Path.Combine(fixture.InfoScreensRoot, "Notices") + Path.DirectorySeparatorChar),
            ],
        };
        var repository = new FakeAdminRepository
        {
            Folders = [new SongFolderSummary(7, "Service", true, 3)],
        };
        var sut = new ExternalFileOperationViewModel(fixture.Settings, repository, service);

        await sut.LoadAsync();

        sut.WorkingFolder.Should().Be(Path.GetFullPath(fixture.WorkingFolder));
        sut.DatabasePath.Should().Be(Path.GetFullPath(fixture.AdminDatabasePath));
        sut.ExternalFolders.Should().HaveCount(2);
        sut.SongFolders.Should().ContainSingle().Which.Name.Should().Be("Service");
        sut.SelectedExternalFolder.Should().Be(sut.ExternalFolders[0]);
        sut.SelectedSongFolder.Should().Be(sut.SongFolders[0]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCopyingInfoScreensToSongFolder_ForwardsImportRequest()
    {
        using var fixture = TempExternalOperationSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        fixture.CreateAdminDatabase();
        var source = fixture.WriteInfoScreen("notice.esi");
        var service = new FakeExternalFileOperationService
        {
            Report = ExternalFileOperationReport.Success(
                ExternalFileOperationKind.Copy,
                ExternalFileItemKind.InfoScreen,
                ExternalFileDestinationKind.SongFolder,
                CreatedFilePaths: [],
                CreatedSongIds: [501]),
        };
        var repository = new FakeAdminRepository
        {
            Folders = [new SongFolderSummary(7, "Service", true, 3)],
        };
        var sut = new ExternalFileOperationViewModel(fixture.Settings, repository, service);
        await sut.LoadAsync();
        sut.AddSourceFiles([source]);
        sut.DestinationKind = ExternalFileDestinationKind.SongFolder;
        sut.SelectedSongFolder = sut.SongFolders[0];

        await sut.ExecuteAsync();

        service.LastRequest.Should().NotBeNull();
        service.LastRequest!.DestinationKind.Should().Be(ExternalFileDestinationKind.SongFolder);
        service.LastRequest.SourceFiles.Should().Equal(Path.GetFullPath(source));
        service.LastRequest.DatabasePath.Should().Be(Path.GetFullPath(fixture.AdminDatabasePath));
        service.LastRequest.TargetSongFolderNo.Should().Be(7);
        service.LastRequest.BackupRoot.Should().Be(Path.GetFullPath(fixture.BackupRoot));
        service.LastRequest.StartingSongNumber.Should().Be(4);
        sut.StatusMessage.Should().Be("1개 항목을 처리했습니다.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenMoveUsesSongFolderDestination_ShowsValidation()
    {
        using var fixture = TempExternalOperationSettings.Create();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        fixture.CreateAdminDatabase();
        var source = fixture.WriteInfoScreen("notice.esi");
        var service = new FakeExternalFileOperationService();
        var repository = new FakeAdminRepository
        {
            Folders = [new SongFolderSummary(7, "Service", true, 3)],
        };
        var sut = new ExternalFileOperationViewModel(fixture.Settings, repository, service);
        await sut.LoadAsync();
        sut.AddSourceFiles([source]);
        sut.OperationKind = ExternalFileOperationKind.Move;
        sut.DestinationKind = ExternalFileDestinationKind.SongFolder;

        await sut.ExecuteAsync();

        sut.ValidationMessage.Should().Be("곡 폴더로 가져오기는 InfoScreen 복사에서만 사용할 수 있습니다.");
        service.LastRequest.Should().BeNull();
    }

    private sealed class FakeExternalFileOperationService : IExternalFileOperationService
    {
        public IReadOnlyList<ExternalFileFolder> Folders { get; init; } = [];

        public ExternalFileOperationRequest? LastRequest { get; private set; }

        public ExternalFileOperationReport Report { get; init; } = ExternalFileOperationReport.Success(
            ExternalFileOperationKind.Copy,
            ExternalFileItemKind.InfoScreen,
            ExternalFileDestinationKind.ExternalFolder,
            CreatedFilePaths: ["created.esi"],
            CreatedSongIds: []);

        public IReadOnlyList<ExternalFileFolder> GetFolders(string workingFolder, ExternalFileItemKind itemKind)
            => Folders;

        public Task<ExternalFileOperationReport> ExecuteAsync(ExternalFileOperationRequest request)
        {
            LastRequest = request;
            return Task.FromResult(Report);
        }
    }

    private sealed class FakeAdminRepository : IAdminDatabaseRepository
    {
        public IReadOnlyList<SongFolderSummary> Folders { get; init; } = [];

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => Task.FromResult(Folders);

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> MoveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongMoveRequest> moves)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order)
            => throw new NotSupportedException();
    }

    private sealed class TempExternalOperationSettings : IDisposable
    {
        private TempExternalOperationSettings(string root)
        {
            Root = root;
            WorkingFolder = Path.Combine(root, "Work");
            InfoScreensRoot = Path.Combine(WorkingFolder, "InfoScreens");
            BackupRoot = Path.Combine(root, "Backups");
            Directory.CreateDirectory(InfoScreensRoot);
            Directory.CreateDirectory(BackupRoot);
            Settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "SettingsBackups")));
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public string InfoScreensRoot { get; }

        public string BackupRoot { get; }

        public string AdminDatabasePath => Path.Combine(WorkingFolder, "Admin", "Database", "EasiSlidesDb.db");

        public ISettingsService Settings { get; }

        public static TempExternalOperationSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_ExternalOperation_{Guid.NewGuid():N}"));

        public void CreateAdminDatabase()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AdminDatabasePath)!);
            File.WriteAllText(AdminDatabasePath, "db");
        }

        public string WriteInfoScreen(string name)
        {
            var path = Path.Combine(InfoScreensRoot, name);
            File.WriteAllText(path, "<EasiSlides />");
            return path;
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
    }
}
