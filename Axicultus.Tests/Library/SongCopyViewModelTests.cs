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

public class SongCopyViewModelTests
{
    [Fact]
    public void Load_IncludesSourceFolderAndDefaultsTitleTargetAndSongNumber()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var target = new SongFolderSummary(2, "Evening", true, 5);
        var recycle = new SongFolderSummary(0, "Recycle Folder", false, 4);
        var disabled = new SongFolderSummary(3, "Folder 3", false, 0);
        var sut = new SongCopyViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo), source, [recycle, source, target, disabled]);

        sut.SourceFolderName.Should().Be("Morning");
        sut.SongTitle.Should().Be("Opening");
        sut.CopyTitle.Should().Be("Opening - 복사본");
        sut.TargetFolders.Should().Equal(source, target);
        sut.SelectedTargetFolder.Should().Be(source);
        sut.SongNumber.Should().Be(10, "the copy keeps the source song number, as in WinForms");

        sut.SelectedTargetFolder = target;

        sut.SongNumber.Should().Be(10);
    }

    [Fact]
    public async Task CopyAsync_CopiesEveryColumnOfTheSourceSong()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var source = new SongFolderSummary(1, "Morning", true, 2);
        var repository = new FakeDetailRepository
        {
            Detail = new SongDetail(10, "Opening", "Alt", 1, 0, "Line 1", "\u0001\u0000", "Writer", "Copyright", -1,
                "4/4", "G", "(0;C)", "Call", "Lic1", "Lic2", "Book", "User", "settings", "format"),
        };
        var sut = new SongCopyViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, Song(10, "Opening", source.FolderNo) with { SongNumber = 0 }, source, [source]);

        await sut.CopyAsync();

        repository.LastSong.Should().Be(new SongWriteModel(null, "Opening - 복사본", "Alt", 1, 0, "Line 1", "\u0001\u0000",
            "Writer", "Copyright", -1, "4/4", "G", "(0;C)", "Call", "Lic1", "Lic2", "Book", "User", "settings", "format",
            SequenceIsStored: true), "the stored sequence is written as it is");
    }

    [Fact]
    public async Task CopyAsync_SeveralSongs_CopiesThemAsTheyAreInOneOperation_AndReportsTheMissing()
    {
        // FrmMain copies every selected song with its own title and number, and leaves out a song whose row is gone.
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var source = new SongFolderSummary(1, "Morning", true, 3);
        var target = new SongFolderSummary(2, "Evening", true, 0);
        var repository = new FakeDetailRepository();
        repository.Details[10] = new SongDetail(10, "Opening", "Alt", 1, 28, "Line 1", "\u0001", "Writer", "Copyright", 2,
            "4/4", "G", "(0;C)", "Call", "Lic1", "Lic2", "Book", "User", "settings", "format");
        repository.Details[11] = new SongDetail(11, "Closing", "", 1, 0, "Line 2", "", "", "", -1, "", "", "", "", "", "", "", "", "", "");
        var sut = new SongCopyViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath,
            [Song(10, "Opening", 1), Song(11, "Closing", 1), Song(12, "Gone", 1)], [source, target]);

        sut.IsSingleSong.Should().BeFalse();
        sut.SongTitle.Should().Be("3곡 선택: Opening, Closing, Gone");
        sut.SourceFolderName.Should().Be("Morning");
        sut.SelectedTargetFolder.Should().Be(source, "FrmCopy offers every folder in use, the songs' own folder too");
        sut.SelectedTargetFolder = target;
        sut.CopyCommand.CanExecute(null).Should().BeTrue("no title is asked for several songs");

        await sut.CopyAsync();

        repository.SaveSongsCalls.Should().Be(1, "one backup and one transaction for the whole selection");
        repository.SavedSongs.Should().Equal(
            new SongWriteModel(null, "Opening", "Alt", 2, 28, "Line 1", "\u0001", "Writer", "Copyright", 2,
                "4/4", "G", "(0;C)", "Call", "Lic1", "Lic2", "Book", "User", "settings", "format", SequenceIsStored: true),
            new SongWriteModel(null, "Closing", "", 2, 0, "Line 2", SequenceIsStored: true));
        sut.CreatedSongIds.Should().Equal(44, 45);
        sut.StatusMessage.Should().Be("2곡을 복사했습니다. 건너뜀 1곡(라이브러리에 없음): Gone");
    }

    [Fact]
    public async Task CopyAsync_WhenValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongCopySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
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
        repository.LastSong.SongNumber.Should().Be(10);
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

    private sealed class FakeDetailRepository : FakeAdminDatabaseRepository, IAdminSongDetailRepository
    {
        public SongDetail? Detail { get; init; }

        public Dictionary<int, SongDetail> Details { get; } = [];

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
            => Task.FromResult(Details.TryGetValue(songId, out var detail) ? detail : Detail);
    }

    private class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public SongWriteModel? LastSong { get; private set; }

        public List<SongWriteModel> SavedSongs { get; } = [];

        public int SaveSongsCalls { get; private set; }

        public Task<AdminDatabaseWriteReport> SaveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongWriteModel> songs)
        {
            SaveSongsCalls++;
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            SavedSongs.AddRange(songs);
            LastSong = songs[^1];
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveSong,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                [.. songs.Select((_, index) => 44 + index)],
                [songs[0].FolderNo],
                Issues: []));
        }

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
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_SongCopy_{Guid.NewGuid():N}"));

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
