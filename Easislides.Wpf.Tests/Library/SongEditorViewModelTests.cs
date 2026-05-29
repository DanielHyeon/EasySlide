using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
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

    [Fact]
    public async Task LoadAsync_WithExistingSong_LoadsDetailFieldsAndBuildsPreview()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Details[10] = Detail(
            10,
            "Detailed",
            folderNo: 2,
            lyrics: "Verse one\r\nLine two\r\n\r\nVerse two",
            sequence: "\u0001\u0002",
            writer: "Writer",
            copyright: "Copyright",
            capo: 2,
            timing: "4/4",
            key: "D",
            notations: "C;1;",
            category: "Praise",
            licenceAdmin1: "CCLI",
            licenceAdmin2: "Admin2",
            bookReference: "Book 12",
            userReference: "User 7",
            settings: "legacy-settings",
            formatData: "legacy-format");
        fixture.Settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFF102030));
        fixture.Settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFFEFE8DD));
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));

        sut.Title.Should().Be("Detailed");
        sut.Sequence.Should().Be("\u0001\u0002");
        sut.Writer.Should().Be("Writer");
        sut.Copyright.Should().Be("Copyright");
        sut.Capo.Should().Be(2);
        sut.Timing.Should().Be("4/4");
        sut.Notations.Should().Be("C;1;");
        sut.LicenceAdmin1.Should().Be("CCLI");
        sut.LicenceAdmin2.Should().Be("Admin2");
        sut.BookReference.Should().Be("Book 12");
        sut.UserReference.Should().Be("User 7");
        sut.SongSettingsData.Should().Be("legacy-settings");
        sut.FormatData.Should().Be("legacy-format");
        sut.PreviewTitle.Should().Be("Detailed");
        sut.PreviewLyrics.Should().Contain("Verse one").And.Contain("Line two");
        sut.PreviewMetadata.Should().Contain("D").And.Contain("4/4").And.Contain("Writer");
        sut.PreviewFormatStatus.Should().Contain("format").And.Contain("notation").And.Contain("sequence");
        sut.PreviewForegroundHex.Should().Be("#FF102030");
        sut.PreviewBackgroundHex.Should().Be("#FFEFE8DD");
        sut.PreviewFontFamilyOptions.Should().Contain("Malgun Gothic");
        sut.PreviewMainFontSize.Should().Be(18);
        sut.PreviewNotationFontSize.Should().Be(14);
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WhenExistingSongLoadedFromDetail_PreservesLegacyFields()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Details[10] = Detail(
            10,
            "Detailed",
            folderNo: 2,
            lyrics: "Original",
            sequence: "\u0001",
            writer: "Writer",
            copyright: "Copyright",
            capo: 3,
            timing: "6/8",
            key: "E",
            notations: "D;2;",
            category: "Offering",
            licenceAdmin1: "Lic1",
            licenceAdmin2: "Lic2",
            bookReference: "Book",
            userReference: "User",
            settings: "settings",
            formatData: "format");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));
        sut.Title = "Revised";
        sut.Lyrics = "Changed";

        await sut.SaveAsync();

        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.Title.Should().Be("Revised");
        repository.LastSong.Lyrics.Should().Be("Changed");
        repository.LastSong.Sequence.Should().Be("\u0001");
        repository.LastSong.Writer.Should().Be("Writer");
        repository.LastSong.Copyright.Should().Be("Copyright");
        repository.LastSong.Capo.Should().Be(3);
        repository.LastSong.Timing.Should().Be("6/8");
        repository.LastSong.Key.Should().Be("E");
        repository.LastSong.Notations.Should().Be("D;2;");
        repository.LastSong.Category.Should().Be("Offering");
        repository.LastSong.LicenceAdmin1.Should().Be("Lic1");
        repository.LastSong.LicenceAdmin2.Should().Be("Lic2");
        repository.LastSong.BookReference.Should().Be("Book");
        repository.LastSong.UserReference.Should().Be("User");
        repository.LastSong.Settings.Should().Be("settings");
        repository.LastSong.FormatData.Should().Be("format");
    }

    [Fact]
    public async Task SaveAsync_WhenLiveSessionActiveAndPromptDeclines_DoesNotWrite()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var liveSession = new FakeLiveSessionService
        {
            Current = new LiveSessionSnapshot(LiveState.Active, "Live song", "Monitor 2", IsBlackout: false, "Song")
        };
        var prompt = new FakeLiveSafetyPrompt { NextResult = false };
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, liveSession, prompt);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), Song(10, "Old", folderNo: 1));
        sut.Title = "Revised";

        await sut.SaveAsync();

        prompt.LastRequest.Should().NotBeNull();
        prompt.LastRequest!.ActionName.Should().Be("Save song while live");
        repository.LastSong.Should().BeNull();
        sut.IsLiveEditWarningVisible.Should().BeTrue();
        sut.StatusMessage.Should().Contain("cancel");
    }

    [Fact]
    public async Task SaveAsync_WhenLiveSessionActiveAndPromptApproves_Writes()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var liveSession = new FakeLiveSessionService
        {
            Current = new LiveSessionSnapshot(LiveState.Hidden, "Live song", "Monitor 2", IsBlackout: true, "Song")
        };
        var prompt = new FakeLiveSafetyPrompt { NextResult = true };
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, liveSession, prompt);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), Song(10, "Old", folderNo: 1));
        sut.Title = "Revised";

        await sut.SaveAsync();

        prompt.LastRequest.Should().NotBeNull();
        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.Title.Should().Be("Revised");
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

    private static SongDetail Detail(
        int id,
        string title,
        int folderNo,
        string lyrics = "",
        string alternateTitle = "",
        int songNumber = 1,
        string sequence = "",
        string writer = "",
        string copyright = "",
        int capo = 0,
        string timing = "",
        string key = "",
        string notations = "",
        string category = "",
        string licenceAdmin1 = "",
        string licenceAdmin2 = "",
        string bookReference = "",
        string userReference = "",
        string settings = "",
        string formatData = "")
        => new(
            id,
            title,
            alternateTitle,
            folderNo,
            songNumber,
            lyrics,
            sequence,
            writer,
            copyright,
            capo,
            timing,
            key,
            notations,
            category,
            licenceAdmin1,
            licenceAdmin2,
            bookReference,
            userReference,
            settings,
            formatData);

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository, IAdminSongDetailRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public SongWriteModel? LastSong { get; private set; }

        public Dictionary<int, SongDetail> Details { get; } = [];

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
            => Task.FromResult(Details.TryGetValue(songId, out var detail) ? detail : null);

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

    private sealed class FakeLiveSessionService : ILiveSessionService
    {
        public event EventHandler<LiveSessionChangedEventArgs>? SessionChanged;

        public LiveSessionSnapshot Current { get; set; } = LiveSessionSnapshot.Off;

        public void GoLive(LiveQueueItem item, string outputMonitorName)
        {
            Current = new LiveSessionSnapshot(LiveState.Active, item.Title, outputMonitorName, IsBlackout: false, item.Kind);
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void HideOutput(bool blackout)
        {
            Current = Current with { State = LiveState.Hidden, IsBlackout = blackout };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void Stop()
        {
            Current = LiveSessionSnapshot.Off;
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }
    }

    private sealed class FakeLiveSafetyPrompt : ILiveSafetyPrompt
    {
        public bool NextResult { get; init; } = true;

        public LiveSafetyRequest? LastRequest { get; private set; }

        public Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(NextResult);
        }
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
