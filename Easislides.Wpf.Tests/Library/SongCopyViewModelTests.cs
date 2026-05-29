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

public class SongCopyViewModelTests
{
    [Fact]
    public void Load_IncludesSourceFolderAndDefaultsTitleTargetAndSongNumber()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var target = new SongFolderSummary(2, "Evening", true, 5);
        var sut = new SongCopyViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source, target]);

        sut.SourceFolderName.Should().Be("Morning");
        sut.SongTitle.Should().Be("Opening");
        sut.CopyTitle.Should().Be("Opening - 복사본");
        sut.TargetFolders.Should().Equal(source, target);
        sut.SelectedTargetFolder.Should().Be(source);
        sut.SongNumber.Should().Be(3);

        sut.SelectedTargetFolder = target;

        sut.SongNumber.Should().Be(6);
    }

    [Fact]
    public async Task CopyAsync_WhenValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var target = new SongFolderSummary(2, "Evening", true, 5);
        var sut = new SongCopyViewModel(fixture.Settings, repository);
        sut.Load(
            fixture.AdminDatabasePath,
            Song(10, "Opening", source.FolderNo, "Alt", "Call", "G", "Line 1"),
            source,
            [source, target]);
        sut.SelectedTargetFolder = target;
        sut.CopyTitle = "  Evening Opening  ";

        await sut.CopyAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.SongId.Should().BeNull();
        repository.LastSong.Title.Should().Be("Evening Opening");
        repository.LastSong.AlternateTitle.Should().Be("Alt");
        repository.LastSong.FolderNo.Should().Be(target.FolderNo);
        repository.LastSong.SongNumber.Should().Be(6);
        repository.LastSong.Category.Should().Be("Call");
        repository.LastSong.Key.Should().Be("G");
        repository.LastSong.Lyrics.Should().Be("Line 1");
        sut.CreatedSongId.Should().Be(44);
        sut.StatusMessage.Should().Be("복사되었습니다.");
    }

    [Fact]
    public async Task CopyAsync_WhenTitleMissing_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var sut = new SongCopyViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source]);
        sut.CopyTitle = "   ";

        await sut.CopyAsync();

        sut.ValidationMessage.Should().Be("복사할 제목을 입력하세요.");
        repository.LastSong.Should().BeNull();
    }

    [Fact]
    public async Task CopyAsync_WhenBackupRootNotConfigured_UsesDatabaseSiblingBackupsFolder()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var sut = new SongCopyViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [source]);

        await sut.CopyAsync();

        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        sut.CreatedSongId.Should().Be(44);
    }

    private static SongSummary Song(
        int id,
        string title,
        int folderNo,
        string alternateTitle = "",
        string category = "",
        string key = "",
        string lyrics = "")
        => new(id, title, alternateTitle, folderNo, SongNumber: id, category, key, lyrics);

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public SongWriteModel? LastSong { get; private set; }

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

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            LastSong = song;
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveSong,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                [44],
                [song.FolderNo],
                Issues: []));
        }

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

    private sealed class TempSongCopySettings : IDisposable
    {
        private TempSongCopySettings(string root)
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

        public static TempSongCopySettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_SongCopy_{Guid.NewGuid():N}"));

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
