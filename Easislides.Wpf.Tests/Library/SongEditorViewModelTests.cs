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

public class SongEditorViewModelTests
{
    [Fact]
    public void Load_WithExistingSong_PopulatesFieldsAndTracksChanges()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: true, SongCount: 1);
        var song = Song(31, "Amazing Grace", folderNo: 3, alternateTitle: "나 같은 죄인", category: "Opening", key: "G", lyrics: "Amazing grace");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, folder, song);

        sut.IsNew.Should().BeFalse();
        sut.SongId.Should().Be(31);
        sut.FolderNo.Should().Be(3);
        sut.FolderName.Should().Be("Evening");
        sut.Title.Should().Be("Amazing Grace");
        sut.AlternateTitle.Should().Be("나 같은 죄인");
        sut.Category.Should().Be("Opening");
        sut.Key.Should().Be("G");
        sut.Lyrics.Should().Be("Amazing grace");
        sut.HasChanges.Should().BeFalse();

        sut.Title = "Amazing Grace Revised";

        sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WhenExistingSongIsValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var folder = new SongFolderSummary(2, "Morning", IsEnabled: true, SongCount: 1);
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, folder, Song(10, "Old", folderNo: 2));
        sut.Title = "  Revised  ";
        sut.AlternateTitle = "  Alt  ";
        sut.Category = "  Communion  ";
        sut.Key = "  D  ";
        sut.Lyrics = "Line 1\r\nLine 2";

        await sut.SaveAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.SongId.Should().Be(10);
        repository.LastSong.Title.Should().Be("Revised");
        repository.LastSong.AlternateTitle.Should().Be("Alt");
        repository.LastSong.FolderNo.Should().Be(2);
        repository.LastSong.Category.Should().Be("Communion");
        repository.LastSong.Key.Should().Be("D");
        repository.LastSong.Lyrics.Should().Be("Line 1\r\nLine 2");
        sut.StatusMessage.Should().Be("저장되었습니다.");
        sut.ValidationMessage.Should().Be("");
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WhenTitleMissing_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "   ";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("제목을 입력하세요.");
        repository.LastSong.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_WhenBackupRootNotConfigured_UsesDatabaseSiblingBackupsFolder()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "New Song";

        await sut.SaveAsync();

        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.SongId.Should().BeNull();
        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        sut.SongId.Should().Be(44);
        sut.IsNew.Should().BeFalse();
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
            var songId = song.SongId ?? 44;
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveSong,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                [songId],
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

    private sealed class TempSongEditorSettings : IDisposable
    {
        private TempSongEditorSettings(string root)
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

        public static TempSongEditorSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_SongEditor_{Guid.NewGuid():N}"));

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
