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

public class FolderEditorViewModelTests
{
    [Fact]
    public void Load_WithExistingFolder_PopulatesFieldsAndTracksChanges()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: false, SongCount: 4);
        var sut = new FolderEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, folder, [folder]);

        sut.IsNew.Should().BeFalse();
        sut.FolderNo.Should().Be(3);
        sut.Name.Should().Be("Evening");
        sut.IsEnabled.Should().BeFalse();
        sut.HasChanges.Should().BeFalse();

        sut.Name = "Evening Revised";

        sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WhenExistingFolderIsValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: true, SongCount: 4);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, folder, [folder]);
        sut.Name = "  Evening Revised  ";
        sut.IsEnabled = false;

        await sut.SaveAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastFolder.Should().Be(new SongFolderWriteModel(3, "Evening Revised", false));
        sut.StatusMessage.Should().Be("저장되었습니다.");
        sut.ValidationMessage.Should().Be("");
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WhenNewFolder_UsesNextFolderNumberAndDefaultBackupRoot()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var existing = new SongFolderSummary(6, "Morning", IsEnabled: true, SongCount: 2);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, [existing]);
        sut.Name = "New Folder";

        await sut.SaveAsync();

        repository.LastFolder.Should().Be(new SongFolderWriteModel(7, "New Folder", true));
        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        sut.FolderNo.Should().Be(7);
        sut.IsNew.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WhenNameMissing_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, []);
        sut.Name = "   ";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("폴더 이름을 입력하세요.");
        repository.LastFolder.Should().BeNull();
    }

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public SongFolderWriteModel? LastFolder { get; private set; }

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            LastFolder = folder;
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveFolder,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                AffectedSongIds: [],
                [folder.FolderNo],
                Issues: []));
        }

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

    private sealed class TempFolderEditorSettings : IDisposable
    {
        private TempFolderEditorSettings(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
            BackupRoot = Path.Combine(root, "ConfiguredBackups");
            Settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "SettingsBackups")));
        }

        public string Root { get; }

        public string BackupRoot { get; }

        public ISettingsService Settings { get; }

        public string AdminDatabasePath { get; private set; } = "";

        public static TempFolderEditorSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_FolderEditor_{Guid.NewGuid():N}"));

        public void CreateAdminDatabaseFile(string fileName)
        {
            AdminDatabasePath = Path.Combine(Root, fileName);
            File.WriteAllText(AdminDatabasePath, "");
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
