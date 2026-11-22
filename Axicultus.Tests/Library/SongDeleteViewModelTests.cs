using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class SongDeleteViewModelTests
{
    [Fact]
    public void Load_PopulatesSelectedSongAndSourceFolder()
    {
        using var fixture = TempSongDeleteSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var sut = new SongDeleteViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source);

        sut.DatabasePath.Should().Be(fixture.AdminDatabasePath);
        sut.SongId.Should().Be(10);
        sut.SourceFolderNo.Should().Be(1);
        sut.SourceFolderName.Should().Be("Morning");
        sut.SongTitle.Should().Be("Opening");
        sut.ValidationMessage.Should().Be("");
    }

    [Fact]
    public async Task DeleteAsync_WhenValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongDeleteSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var sut = new SongDeleteViewModel(fixture.Settings, repository);
        repository.Songs.Add(Song(10, "Opening", source.FolderNo));
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source);

        await sut.DeleteAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastDeletes.Should().ContainSingle().Which.Should().Be(new SongDeleteRequest(10, 1));
        sut.StatusMessage.Should().Be("삭제되었습니다.");
    }

    [Fact]
    public async Task DeleteAsync_WhenSongMissing_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongDeleteSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongDeleteViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(0, "", folderNo: 0), new SongFolderSummary(0, "", true, 0));

        await sut.DeleteAsync();

        sut.ValidationMessage.Should().Be("삭제할 곡을 선택하세요.");
        repository.LastDeletes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WhenBackupRootNotConfigured_UsesDatabaseSiblingBackupsFolder()
    {
        using var fixture = TempSongDeleteSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var sut = new SongDeleteViewModel(fixture.Settings, repository);
        repository.Songs.Add(Song(10, "Opening", source.FolderNo));
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source);

        await sut.DeleteAsync();

        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
    }

    [Fact]
    public async Task DeleteAsync_SeveralSongs_DeletesThemInOneOperation_AndReportsTheMissing()
    {
        // FrmMain asks once about "the N songs you have selected" and files them all in the deleted folder.
        using var fixture = TempSongDeleteSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var morning = new SongFolderSummary(1, "Morning", true, 2);
        var evening = new SongFolderSummary(2, "Evening", true, 1);
        var repository = new FakeAdminDatabaseRepository();
        repository.Songs.AddRange([Song(10, "A", 1), Song(11, "B", 2)]);
        var sut = new SongDeleteViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, [Song(10, "A", 1), Song(11, "B", 2), Song(12, "C", 1)], [morning, evening]);

        sut.SongTitle.Should().Be("3곡 선택: A, B, C");
        sut.ConfirmationText.Should().Be("선택한 3곡을 정말 삭제하시겠습니까?");
        sut.DeleteCommand.CanExecute(null).Should().BeTrue();

        await sut.DeleteAsync();

        repository.DeleteCalls.Should().Be(1, "one backup and one transaction for the whole selection");
        repository.LastDeletes.Should().Equal(new SongDeleteRequest(10, 1), new SongDeleteRequest(11, 2));
        sut.StatusMessage.Should().Be("2곡을 삭제했습니다. 건너뜀 1곡(이미 삭제되었거나 라이브러리에 없음): C");
    }

    private static SongSummary Song(int id, string title, int folderNo)
        => new(id, title, AlternateTitle: "", folderNo, SongNumber: id, Category: "", Key: "", Lyrics: "");

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public List<SongDeleteRequest> LastDeletes { get; } = [];

        public List<SongSummary> Songs { get; } = [];

        public int DeleteCalls { get; private set; }

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => Task.FromResult<IReadOnlyList<SongSummary>>(Songs);

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
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            DeleteCalls++;
            LastDeletes.AddRange(deletes);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SoftDeleteSongs,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                deletes.Select(delete => delete.SongId).ToArray(),
                deletes.Select(delete => delete.OriginalFolderNo).ToArray(),
                Issues: []));
        }

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order)
            => throw new NotSupportedException();
    }

    private sealed class TempSongDeleteSettings : IDisposable
    {
        private TempSongDeleteSettings(string root)
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

        public static TempSongDeleteSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_SongDelete_{Guid.NewGuid():N}"));

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
