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

public class LibraryViewModelTests
{
    [Fact]
    public async Task LoadAsync_UsesExplicitAdminDatabasePathAndLoadsFoldersAndSongs()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange([
            new SongFolderSummary(2, "Evening", IsEnabled: true, SongCount: 1),
            new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 2),
        ]);
        repository.SongsByFolder[2] = [Song(20, "Evening Song", folderNo: 2)];
        repository.SongsByFolder[1] = [Song(10, "Morning Song", folderNo: 1), Song(11, "Opening", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);

        await sut.LoadAsync();

        sut.DatabasePath.Should().Be(fixture.AdminDatabasePath);
        sut.Folders.Select(folder => folder.Name).Should().Equal("Evening", "Morning");
        sut.SelectedFolder.Should().Be(repository.Folders[0]);
        sut.Songs.Select(song => song.Title).Should().Equal("Evening Song");
        sut.StatusMessage.Should().Be("1개 폴더, 1개 곡 표시");
    }

    [Fact]
    public async Task LoadSongsForSelectedFolderAsync_WhenSelectionChanges_LoadsThatFolder()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        var morning = new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 1);
        var evening = new SongFolderSummary(2, "Evening", IsEnabled: true, SongCount: 2);
        repository.Folders.AddRange([morning, evening]);
        repository.SongsByFolder[1] = [Song(10, "Morning Song", folderNo: 1)];
        repository.SongsByFolder[2] = [Song(20, "Evening First", folderNo: 2), Song(21, "Evening Second", folderNo: 2)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        sut.SelectedFolder = evening;
        await sut.LoadSongsForSelectedFolderAsync();

        sut.Songs.Select(song => song.Title).Should().Equal("Evening First", "Evening Second");
        repository.RequestedFolderNos.Should().Contain(2);
    }

    [Fact]
    public async Task SearchText_FiltersTitleAlternateCategoryAndLyrics()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 3));
        repository.SongsByFolder[1] =
        [
            Song(10, "Amazing Grace", folderNo: 1, lyrics: "saved a wretch"),
            Song(11, "Holy Forever", folderNo: 1, category: "Communion"),
            Song(12, "Blessing", folderNo: 1, alternateTitle: "축복"),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        sut.SearchText = "communion";

        sut.Songs.Should().ContainSingle().Which.Title.Should().Be("Holy Forever");

        sut.SearchText = "축복";

        sut.Songs.Should().ContainSingle().Which.Title.Should().Be("Blessing");
    }

    [Fact]
    public async Task LoadAsync_WhenDatabasePathMissing_ShowsValidationMessage()
    {
        using var fixture = TempLibrarySettings.Create();
        var sut = new LibraryViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        await sut.LoadAsync();

        sut.Folders.Should().BeEmpty();
        sut.Songs.Should().BeEmpty();
        sut.StatusMessage.Should().Be("AdminDB 경로를 설정해야 합니다.");
    }

    [Fact]
    public async Task MoveSelectedFolderDownAsync_UsesRepositoryAndReloadsMovedFolder()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        fixture.Settings.Set(EasiSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange([
            new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 1),
            new SongFolderSummary(2, "Evening", IsEnabled: true, SongCount: 1),
        ]);
        repository.SongsByFolder[1] = [Song(10, "Opening", folderNo: 1)];
        repository.SongsByFolder[2] = [Song(20, "Closing", folderNo: 2)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        await sut.MoveSelectedFolderDownAsync();

        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastFolderOrder.Should().Equal(
            new FolderOrderRequest(1, 2),
            new FolderOrderRequest(2, 1));
        sut.SelectedFolder.Should().NotBeNull();
        sut.SelectedFolder!.Name.Should().Be("Morning");
        sut.SelectedFolder.FolderNo.Should().Be(2);
        sut.Songs.Should().ContainSingle().Which.SongId.Should().Be(10);
        sut.StatusMessage.Should().Be("폴더 순서를 저장했습니다.");
    }

    [Fact]
    public async Task MoveSelectedSongUpAsync_RenumbersVisibleSongsAndRestoresSelection()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 3));
        repository.SongsByFolder[1] =
        [
            Song(10, "First", folderNo: 1, songNumber: 1),
            Song(11, "Second", folderNo: 1, songNumber: 2),
            Song(12, "Third", folderNo: 1, songNumber: 3),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.SelectedSong = sut.Songs[2];

        await sut.MoveSelectedSongUpAsync();

        repository.LastSongFolderNo.Should().Be(1);
        repository.LastSongOrder.Should().Equal(
            new SongOrderRequest(10, 1),
            new SongOrderRequest(12, 2),
            new SongOrderRequest(11, 3));
        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        sut.Songs.Select(song => song.SongId).Should().Equal(10, 12, 11);
        sut.SelectedSong.Should().NotBeNull();
        sut.SelectedSong!.SongId.Should().Be(12);
        sut.StatusMessage.Should().Be("곡 순서를 저장했습니다.");
    }

    private static SongSummary Song(
        int id,
        string title,
        int folderNo,
        string alternateTitle = "",
        string category = "",
        string key = "",
        string lyrics = "",
        int? songNumber = null)
        => new(id, title, alternateTitle, folderNo, SongNumber: songNumber ?? id, category, key, lyrics);

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public List<SongFolderSummary> Folders { get; } = [];

        public Dictionary<int, IReadOnlyList<SongSummary>> SongsByFolder { get; } = [];

        public List<int?> RequestedFolderNos { get; } = [];

        public string LastBackupRoot { get; private set; } = "";

        public IReadOnlyList<FolderOrderRequest> LastFolderOrder { get; private set; } = [];

        public int LastSongFolderNo { get; private set; }

        public IReadOnlyList<SongOrderRequest> LastSongOrder { get; private set; } = [];

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => Task.FromResult(new AdminDatabaseSchemaInventory(
                Succeeded: true,
                databasePath,
                SchemaVersion: 4,
                Tables: [],
                Columns: new Dictionary<string, IReadOnlyList<DatabaseColumn>>(),
                Issues: []));

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => Task.FromResult<IReadOnlyList<SongFolderSummary>>(Folders);

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
        {
            RequestedFolderNos.Add(folderNo);
            if (folderNo is not null && SongsByFolder.TryGetValue(folderNo.Value, out var songs))
            {
                return Task.FromResult<IReadOnlyList<SongSummary>>(songs
                    .OrderBy(song => song.SongNumber)
                    .ThenBy(song => song.Title, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(song => song.SongId)
                    .ToArray());
            }

            return Task.FromResult<IReadOnlyList<SongSummary>>([]);
        }

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> MoveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongMoveRequest> moves)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(
            string databasePath,
            string backupRoot,
            IReadOnlyList<FolderOrderRequest> order)
        {
            LastBackupRoot = backupRoot;
            LastFolderOrder = order.ToArray();
            var map = order.ToDictionary(item => item.FolderNo, item => item.NewFolderNo);
            var reorderedFolders = Folders
                .Select(folder => map.TryGetValue(folder.FolderNo, out var newFolderNo)
                    ? folder with { FolderNo = newFolderNo }
                    : folder)
                .OrderBy(folder => folder.FolderNo)
                .ToArray();
            Folders.Clear();
            Folders.AddRange(reorderedFolders);

            var songs = SongsByFolder
                .SelectMany(pair => pair.Value)
                .Select(song => map.TryGetValue(song.FolderNo, out var newFolderNo)
                    ? song with { FolderNo = newFolderNo }
                    : song)
                .GroupBy(song => song.FolderNo)
                .ToDictionary(group => group.Key, group => (IReadOnlyList<SongSummary>)group.ToArray());
            SongsByFolder.Clear();
            foreach (var pair in songs)
            {
                SongsByFolder[pair.Key] = pair.Value;
            }

            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.ReorderFolders,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                AffectedSongIds: [],
                order.Select(item => item.NewFolderNo).ToArray(),
                Issues: []));
        }

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(
            string databasePath,
            string backupRoot,
            int folderNo,
            IReadOnlyList<SongOrderRequest> order)
        {
            LastBackupRoot = backupRoot;
            LastSongFolderNo = folderNo;
            LastSongOrder = order.ToArray();
            var map = order.ToDictionary(item => item.SongId, item => item.SongNumber);
            SongsByFolder[folderNo] = SongsByFolder[folderNo]
                .Select(song => map.TryGetValue(song.SongId, out var songNumber)
                    ? song with { SongNumber = songNumber }
                    : song)
                .ToArray();

            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.ReorderSongs,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                order.Select(item => item.SongId).ToArray(),
                [folderNo],
                Issues: []));
        }
    }

    private sealed class TempLibrarySettings : IDisposable
    {
        private TempLibrarySettings(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
            BackupRoot = Path.Combine(root, "ConfiguredBackups");
            Settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups")));
        }

        public string Root { get; }

        public string BackupRoot { get; }

        public ISettingsService Settings { get; }

        public string AdminDatabasePath { get; private set; } = "";

        public static TempLibrarySettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_Library_{Guid.NewGuid():N}"));

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
