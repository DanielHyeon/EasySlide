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

public class SongMoveViewModelTests
{
    [Fact]
    public void Load_ExcludesSourceFolderAndSelectsFirstTarget()
    {
        using var fixture = TempSongMoveSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var target = new SongFolderSummary(2, "Evening", true, 0);
        var disabled = new SongFolderSummary(3, "Archive", false, 1);
        var sut = new SongMoveViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source, target, disabled]);

        sut.SourceFolderName.Should().Be("Morning");
        sut.SongTitle.Should().Be("Opening");
        sut.TargetFolders.Select(folder => folder.FolderNo).Should().Equal(2, 3);
        sut.SelectedTargetFolder.Should().Be(target);
        sut.ValidationMessage.Should().Be("");
    }

    [Fact]
    public async Task MoveAsync_WhenTargetSelected_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongMoveSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 1);
        var target = new SongFolderSummary(2, "Evening", true, 0);
        var sut = new SongMoveViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source, target]);

        await sut.MoveAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastMoves.Should().ContainSingle().Which.Should().Be(new SongMoveRequest(10, 1, 2));
        sut.StatusMessage.Should().Be("이동되었습니다.");
    }

    [Fact]
    public async Task MoveAsync_WhenNoTargetFolder_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongMoveSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 1);
        var sut = new SongMoveViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source]);

        await sut.MoveAsync();

        sut.ValidationMessage.Should().Be("이동할 대상 폴더를 선택하세요.");
        repository.LastMoves.Should().BeEmpty();
    }

    [Fact]
    public async Task MoveAsync_WhenBackupRootNotConfigured_UsesDatabaseSiblingBackupsFolder()
    {
        using var fixture = TempSongMoveSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 1);
        var target = new SongFolderSummary(2, "Evening", true, 0);
        var sut = new SongMoveViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source, target]);

        await sut.MoveAsync();

        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
    }

    private static SongSummary Song(int id, string title, int folderNo)
        => new(id, title, AlternateTitle: "", folderNo, SongNumber: id, Category: "", Key: "", Lyrics: "");

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public List<SongMoveRequest> LastMoves { get; } = [];

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => throw new NotSupportedException();

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
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            LastMoves.AddRange(moves);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.MoveSongs,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                moves.Select(move => move.SongId).ToArray(),
                moves.Select(move => move.NewFolderNo).ToArray(),
                Issues: []));
        }

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order)
            => throw new NotSupportedException();
    }

    private sealed class TempSongMoveSettings : IDisposable
    {
        private TempSongMoveSettings(string root)
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

        public static TempSongMoveSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_SongMove_{Guid.NewGuid():N}"));

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
