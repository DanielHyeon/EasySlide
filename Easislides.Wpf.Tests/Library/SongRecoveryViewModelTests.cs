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

public class SongRecoveryViewModelTests
{
    [Fact]
    public async Task LoadAsync_PopulatesDeletedSongsWithoutSelectingThem()
    {
        using var fixture = TempSongRecoverySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var deleted = DeletedSong(10, "Opening", originalFolderNo: 2, "Evening");
        var repository = new FakeAdminDatabaseRepository { DeletedSongs = [deleted] };
        var sut = new SongRecoveryViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath);

        await sut.LoadAsync();

        sut.DeletedSongs.Should().ContainSingle();
        sut.DeletedSongs[0].Song.Should().Be(deleted);
        sut.DeletedSongs[0].IsSelected.Should().BeFalse();
        sut.SelectedCount.Should().Be(0);
        sut.StatusMessage.Should().Be("1개 삭제 곡 표시");
    }

    [Fact]
    public async Task RecoverAsync_WhenSongsSelected_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongRecoverySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository
        {
            DeletedSongs =
            [
                DeletedSong(10, "Opening", originalFolderNo: 2, "Evening"),
                DeletedSong(11, "Prayer", originalFolderNo: 3, "Archive"),
            ],
        };
        var sut = new SongRecoveryViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath);
        await sut.LoadAsync();
        sut.DeletedSongs[1].IsSelected = true;

        await sut.RecoverAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastRecoveries.Should().ContainSingle().Which.Should().Be(new SongRecoveryRequest(11, 3));
        sut.RecoveredSongId.Should().Be(11);
        sut.RecoveredFolderNo.Should().Be(3);
        sut.DeletedSongs.Select(item => item.Song.SongId).Should().Equal(10);
        sut.StatusMessage.Should().Be("복구되었습니다.");
    }

    [Fact]
    public async Task RecoverAsync_WhenNoSongsSelected_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongRecoverySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository
        {
            DeletedSongs = [DeletedSong(10, "Opening", originalFolderNo: 2, "Evening")],
        };
        var sut = new SongRecoveryViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath);
        await sut.LoadAsync();

        await sut.RecoverAsync();

        sut.ValidationMessage.Should().Be("복구할 곡을 선택하세요.");
        repository.LastRecoveries.Should().BeEmpty();
    }

    [Fact]
    public async Task RecoverAsync_WhenBackupRootNotConfigured_UsesDatabaseSiblingBackupsFolder()
    {
        using var fixture = TempSongRecoverySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository
        {
            DeletedSongs = [DeletedSong(10, "Opening", originalFolderNo: 2, "Evening")],
        };
        var sut = new SongRecoveryViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath);
        await sut.LoadAsync();
        sut.DeletedSongs[0].IsSelected = true;

        await sut.RecoverAsync();

        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
    }

    private static DeletedSongSummary DeletedSong(int id, string title, int originalFolderNo, string originalFolderName)
        => new(id, title, originalFolderNo, originalFolderName, new DateTime(2026, 5, 29));

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public IReadOnlyList<DeletedSongSummary> DeletedSongs { get; init; } = [];

        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public List<SongRecoveryRequest> LastRecoveries { get; } = [];

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => Task.FromResult(DeletedSongs);

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> MoveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongMoveRequest> moves)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(
            string databasePath,
            string backupRoot,
            IReadOnlyList<SongRecoveryRequest> recoveries)
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            LastRecoveries.AddRange(recoveries);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.RecoverSongs,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                recoveries.Select(recovery => recovery.SongId).ToArray(),
                recoveries.Select(recovery => recovery.TargetFolderNo).ToArray(),
                Issues: []));
        }

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order)
            => throw new NotSupportedException();
    }

    private sealed class TempSongRecoverySettings : IDisposable
    {
        private TempSongRecoverySettings(string root)
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

        public static TempSongRecoverySettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_SongRecovery_{Guid.NewGuid():N}"));

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
