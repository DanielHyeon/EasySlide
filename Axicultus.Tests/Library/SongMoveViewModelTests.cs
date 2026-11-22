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
        var recycle = new SongFolderSummary(0, "Recycle Folder", false, 4);
        var sut = new SongMoveViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [recycle, source, target, disabled]);

        sut.SourceFolderName.Should().Be("Morning");
        sut.SongTitle.Should().Be("Opening");
        sut.TargetFolders.Select(folder => folder.FolderNo).Should().Equal(new[] { 2 }, "folder 0 holds deleted songs and a disabled folder is not shown");
        sut.SelectedTargetFolder.Should().Be(target);
        sut.ValidationMessage.Should().Be("");
    }

    [Fact]
    public async Task MoveAsync_WhenTargetSelected_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongMoveSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 1);
        var target = new SongFolderSummary(2, "Evening", true, 0);
        var sut = new SongMoveViewModel(fixture.Settings, repository);
        repository.Songs.Add(Song(10, "Opening", source.FolderNo));
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
        repository.Songs.Add(Song(10, "Opening", source.FolderNo));
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source, target]);

        await sut.MoveAsync();

        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
    }

    [Fact]
    public async Task MoveAsync_SeveralSongs_MovesThemInOneOperation_AndReportsTheSkipped()
    {
        // Songs picked from two folders (a search result): the one already in the target folder and the one
        // whose row is gone are left out; the others move together.
        using var fixture = TempSongMoveSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var morning = new SongFolderSummary(1, "Morning", true, 2);
        var evening = new SongFolderSummary(2, "Evening", true, 1);
        var repository = new FakeAdminDatabaseRepository();
        repository.Songs.AddRange([Song(10, "A", 1), Song(11, "B", 1), Song(12, "C", 2)]);
        var sut = new SongMoveViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath,
            [Song(10, "A", 1), Song(11, "B", 1), Song(12, "C", 2), Song(13, "D", 1)], [morning, evening]);

        sut.SongTitle.Should().Be("4곡 선택: A, B, C 외 1곡");
        sut.SourceFolderName.Should().Be("여러 폴더");
        sut.TargetFolders.Should().Equal(morning, evening);
        sut.SelectedTargetFolder = evening;

        await sut.MoveAsync();

        repository.MoveCalls.Should().Be(1, "one backup and one transaction for the whole selection");
        repository.LastMoves.Should().Equal(new SongMoveRequest(10, 1, 2), new SongMoveRequest(11, 1, 2));
        sut.MovedSongIds.Should().Equal(10, 11);
        sut.StatusMessage.Should().Be(
            "2곡을 이동했습니다. 건너뜀 1곡(이미 대상 폴더에 있음): C 건너뜀 1곡(라이브러리에 없음): D");
    }

    [Fact]
    public void Load_SeveralSongsOfOneFolder_DoesNotOfferThatFolder()
    {
        using var fixture = TempSongMoveSettings.Create();
        var morning = new SongFolderSummary(1, "Morning", true, 2);
        var evening = new SongFolderSummary(2, "Evening", true, 0);
        var sut = new SongMoveViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, [Song(10, "A", 1), Song(11, "B", 1)], [morning, evening]);

        sut.SourceFolderName.Should().Be("Morning");
        sut.TargetFolders.Should().Equal(evening);
    }

    private static SongSummary Song(int id, string title, int folderNo)
        => new(id, title, AlternateTitle: "", folderNo, SongNumber: id, Category: "", Key: "", Lyrics: "");

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public List<SongMoveRequest> LastMoves { get; } = [];

        public List<SongSummary> Songs { get; } = [];

        public int MoveCalls { get; private set; }

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
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            MoveCalls++;
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
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_SongMove_{Guid.NewGuid():N}"));

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
