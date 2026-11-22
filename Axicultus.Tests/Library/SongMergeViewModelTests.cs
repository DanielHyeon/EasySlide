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

public class SongMergeViewModelTests
{
    [Fact]
    public async Task RefreshMatchesAsync_UsesSelectedTitleFieldsAndBuildsMergeCandidates()
    {
        using var fixture = TempSongMergeSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeMergeRepository();
        var sourceA = new SongFolderSummary(1, "Source A", true, 2);
        var sourceB = new SongFolderSummary(2, "Source B", true, 2);
        var target = new SongFolderSummary(3, "Merged", true, 5);
        repository.SongsByFolder[1] =
        [
            Song(10, "Amazing Grace", 1, alternateTitle: "Grace Alt"),
            Song(11, "Only A", 1),
        ];
        repository.SongsByFolder[2] =
        [
            Song(20, "Different (Choir)", 2, alternateTitle: "Grace Alt"),
            Song(21, "Other", 2),
        ];
        var sut = CreateSut(fixture, repository);
        sut.Load(fixture.AdminDatabasePath, [sourceA, sourceB, target]);
        sut.UseSourceAAlternateTitle = true;
        sut.UseSourceBAlternateTitle = true;
        sut.AppendSourceBTitleToMergedTitle = true;

        await sut.RefreshMatchesAsync();

        sut.Candidates.Should().ContainSingle();
        var candidate = sut.Candidates[0];
        candidate.SourceSongAId.Should().Be(10);
        candidate.SourceSongBId.Should().Be(20);
        candidate.MergeTitle.Should().Be("Amazing Grace (Different)");
        candidate.SourceATitle.Should().Be("Amazing Grace");
        candidate.SourceBTitle.Should().Be("Different (Choir)");
        candidate.IsSelected.Should().BeTrue();
        sut.StatusMessage.Should().Be("1개 병합 후보를 찾았습니다.");
    }

    [Fact]
    public async Task RefreshMatchesAsync_WhenDatabaseCannotBeRead_ReportsInsteadOfThrowing()
    {
        using var fixture = TempSongMergeSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeMergeRepository
        {
            ReadError = new System.Data.SQLite.SQLiteException(System.Data.SQLite.SQLiteErrorCode.Busy, "database is locked"),
        };
        var sut = CreateSut(fixture, repository);
        sut.Load(fixture.AdminDatabasePath,
            [new SongFolderSummary(1, "A", true, 1), new SongFolderSummary(2, "B", true, 1), new SongFolderSummary(3, "C", true, 0)]);

        await sut.RefreshMatchesAsync();

        sut.StatusMessage.Should().StartWith("병합 후보 조회 실패");
    }

    [Fact]
    public void Load_OffersOnlyFoldersInUse()
    {
        using var fixture = TempSongMergeSettings.Create();
        var sut = CreateSut(fixture, new FakeMergeRepository());

        sut.Load(fixture.AdminDatabasePath,
        [
            new SongFolderSummary(0, "Recycle Folder", false, 3),
            new SongFolderSummary(1, "Source A", true, 2),
            new SongFolderSummary(2, "Folder 2", false, 0),
        ]);

        sut.Folders.Select(folder => folder.FolderNo).Should().Equal(1);
        sut.SelectedTargetFolder!.FolderNo.Should().Be(1);
    }

    [Fact]
    public async Task MergeAsync_WhenCandidatesSelected_SavesMergedSongWithLegacyFallbackMetadata()
    {
        using var fixture = TempSongMergeSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeMergeRepository();
        var sourceA = new SongFolderSummary(1, "Source A", true, 1);
        var sourceB = new SongFolderSummary(2, "Source B", true, 1);
        var target = new SongFolderSummary(3, "Merged", true, 5);
        repository.SongsByFolder[1] = [Song(10, "Alpha", 1)];
        repository.SongsByFolder[2] = [Song(20, "Alpha", 2)];
        repository.Details[10] = Detail(
            10,
            "Alpha",
            1,
            lyrics: "[1]\nA verse",
            sequence: "1",
            copyright: "",
            bookReference: "BookA",
            userReference: "UserA",
            writer: "",
            capo: -1,
            key: "",
            timing: "",
            licenceAdmin1: "AdminA");
        repository.Details[20] = Detail(
            20,
            "Alpha",
            2,
            lyrics: "[1]\nB verse",
            sequence: "1",
            copyright: "CopyrightB",
            bookReference: "BookB",
            userReference: "UserB",
            writer: "WriterB",
            capo: 3,
            key: "G",
            timing: "4/4",
            licenceAdmin1: "AdminB");
        var sut = CreateSut(fixture, repository);
        sut.Load(fixture.AdminDatabasePath, [sourceA, sourceB, target]);
        sut.SelectedTargetFolder = target;
        await sut.RefreshMatchesAsync();

        await sut.MergeAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.SavedSongs.Should().ContainSingle();
        var saved = repository.SavedSongs[0];
        saved.SongId.Should().BeNull();
        saved.Title.Should().Be("Alpha");
        saved.AlternateTitle.Should().Be("Alpha");
        saved.FolderNo.Should().Be(3);
        saved.SongNumber.Should().Be(0, "a merged song has no number, as in WinForms");
        saved.Lyrics.Should().Be("[1]\nA verse\n[region 2]\nB verse");
        // FrmSmartMerge stores the verses of song A in lyric order (verse codes), not A's stored sequence.
        saved.Sequence.Should().Be("\u0001");
        saved.SequenceIsStored.Should().BeTrue();
        saved.Copyright.Should().Be("CopyrightB");
        saved.BookReference.Should().Be("BookA,BookB");
        saved.UserReference.Should().Be("UserA,UserB");
        saved.Writer.Should().Be("WriterB");
        saved.Capo.Should().Be(3);
        saved.Key.Should().Be("G");
        saved.Timing.Should().Be("4/4");
        saved.LicenceAdmin1.Should().Be("AdminA");
        saved.LicenceAdmin2.Should().Be("AdminB");
        sut.CreatedCount.Should().Be(1);
        sut.StatusMessage.Should().Be("1개 병합 곡을 만들었습니다.");
    }

    [Fact]
    public async Task MergeAsync_WhenNoCandidateSelected_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongMergeSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeMergeRepository();
        var sourceA = new SongFolderSummary(1, "Source A", true, 1);
        var sourceB = new SongFolderSummary(2, "Source B", true, 1);
        repository.SongsByFolder[1] = [Song(10, "Alpha", 1)];
        repository.SongsByFolder[2] = [Song(20, "Alpha", 2)];
        var sut = CreateSut(fixture, repository);
        sut.Load(fixture.AdminDatabasePath, [sourceA, sourceB]);
        await sut.RefreshMatchesAsync();
        sut.Candidates[0].IsSelected = false;

        await sut.MergeAsync();

        sut.ValidationMessage.Should().Be("병합할 항목을 선택하세요.");
        repository.SavedSongs.Should().BeEmpty();
    }

    private static SongMergeViewModel CreateSut(TempSongMergeSettings fixture, FakeMergeRepository repository)
        => new(fixture.Settings, repository, repository, new SongMergeService());

    private static SongSummary Song(
        int id,
        string title,
        int folderNo,
        string alternateTitle = "")
        => new(id, title, alternateTitle, folderNo, SongNumber: id, Category: "", Key: "", Lyrics: "");

    private static SongDetail Detail(
        int id,
        string title,
        int folderNo,
        string lyrics,
        string sequence = "",
        string copyright = "",
        string bookReference = "",
        string userReference = "",
        string writer = "",
        int capo = 0,
        string key = "",
        string timing = "",
        string licenceAdmin1 = "",
        string licenceAdmin2 = "")
        => new(
            id,
            title,
            AlternateTitle: "",
            folderNo,
            SongNumber: id,
            lyrics,
            sequence,
            writer,
            copyright,
            capo,
            timing,
            key,
            Notations: "",
            Category: "",
            licenceAdmin1,
            licenceAdmin2,
            bookReference,
            userReference,
            Settings: "",
            FormatData: "");

    private sealed class FakeMergeRepository : IAdminDatabaseRepository, IAdminSongDetailRepository
    {
        public Dictionary<int, IReadOnlyList<SongSummary>> SongsByFolder { get; } = [];

        public Dictionary<int, SongDetail> Details { get; } = [];

        public List<SongWriteModel> SavedSongs { get; } = [];

        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => throw new NotSupportedException();

        public Exception? ReadError { get; init; }

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => ReadError is not null ? throw ReadError : Task.FromResult(folderNo is not null && SongsByFolder.TryGetValue(folderNo.Value, out var songs)
                ? songs
                : Array.Empty<SongSummary>());

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
            => Task.FromResult(Details.TryGetValue(songId, out var detail) ? detail : null);

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            SavedSongs.Add(song);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveSong,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                [1000 + SavedSongs.Count],
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

    private sealed class TempSongMergeSettings : IDisposable
    {
        private TempSongMergeSettings(string root)
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

        public static TempSongMergeSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_SongMerge_{Guid.NewGuid():N}"));

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
