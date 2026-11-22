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

public class FolderEditorViewModelTests
{
    [Fact]
    public void InlineOptionsPreserveIndependentChorusFlagsAndReturnOnlyEditedColumns()
    {
        var fields = new Dictionary<string, string> { ["BIU0"] = "32", ["HeadingOption"] = "2" };
        var folder = new OptionsFolderDraft(new SongFolderSummary(3, "찬양", true, 0, FormatFields: fields));
        folder.GetChanges().Should().BeNull();
        folder["BIU0_Bold"] = true;
        folder["BIU0_Italic"] = 2;
        folder["HeadingOption"] = 1;
        folder.GetChanges()!.FormatChanges.Should().BeEquivalentTo(
            new Dictionary<string, string> { ["BIU0"] = "49", ["HeadingOption"] = "1" });
        fields["BIU0"].Should().Be("32");
        folder["BIU0_Bold"] = false;
        folder["BIU0_Italic"] = 0;
        folder["HeadingOption"] = 2;
        folder.GetChanges().Should().BeNull();
    }

    [Fact]
    public void Load_WithExistingFolder_PopulatesFieldsAndTracksChanges()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: false, SongCount: 4);
        var sut = new FolderEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, folder, [folder]);

        sut.IsNew.Should().BeFalse();
        sut.FolderNo.Should().Be(3);
        sut.Name.Should().Be("Evening");
        sut.IsEnabled.Should().BeFalse();
        sut.HasChanges.Should().BeFalse();

        sut.Name = "Evening Revised";

        sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WhenExistingFolderIsValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: true, SongCount: 4);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, folder, [folder]);
        sut.Name = "  Evening Revised  ";
        sut.IsEnabled = false;

        await sut.SaveAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastFolder.Should().Be(new SongFolderWriteModel(3, "Evening Revised", false));
        sut.StatusMessage.Should().Be("저장되었습니다.");
        sut.ValidationMessage.Should().Be("");
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WhenNewFolder_UsesFirstUnusedFolderNumberAndDefaultBackupRoot()
    {
        // WinForms has folders 1 to 40: 1 is in use, 2 is disabled but holds songs, 3 is disabled and empty.
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        SongFolderSummary[] folders =
        [
            new(0, "Recycle Folder", IsEnabled: false, SongCount: 0),
            new(1, "Morning", IsEnabled: true, SongCount: 2),
            new(2, "Folder 2", IsEnabled: false, SongCount: 1),
            new(3, "Folder 3", IsEnabled: false, SongCount: 0),
            new(40, "Evening", IsEnabled: true, SongCount: 0),
        ];
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, folders);
        sut.Name = "New Folder";

        await sut.SaveAsync();

        repository.LastFolder.Should().Be(new SongFolderWriteModel(3, "New Folder", true, ResetFormat: true),
            "slot 3 was another folder before: the new folder does not take over its formatting");
        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        sut.FolderNo.Should().Be(3);
        sut.IsNew.Should().BeFalse();
    }

    [Fact]
    public void Load_ANewFolder_DoesNotTakeASlotThatDeletedSongsCameFrom()
    {
        // Slot 2 is unused and empty, but two deleted songs would be recovered into it.
        using var fixture = TempFolderEditorSettings.Create();
        SongFolderSummary[] folders =
        [
            new(1, "Morning", IsEnabled: true, SongCount: 2),
            new(2, "Former folder", IsEnabled: false, SongCount: 0, DeletedSongCount: 2),
            new(3, "Folder 3", IsEnabled: false, SongCount: 0),
        ];
        var sut = new FolderEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, null, folders);

        sut.FolderNo.Should().Be(3);
    }

    [Fact]
    public async Task SaveAsync_WhenNoFolderNumberIsFree_RefusesInsteadOfUsingNumber41()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var folders = Enumerable.Range(1, 40).Select(number => new SongFolderSummary(number, $"Folder {number}", true, 0)).ToArray();
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, folders);
        sut.Name = "New Folder";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Contain("40개까지");
        repository.LastFolder.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task SaveAsync_WhenExistingFolderHasSortOrder_DoesNotSendSortOrder(int groupStyle)
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: true, SongCount: 4, GroupStyle: groupStyle);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, folder, [folder]);
        sut.Name = "Evening Revised";
        sut.IsEnabled = false;

        await sut.SaveAsync();

        repository.LastFolder.Should().NotBeNull();
        repository.LastFolder!.GroupStyle.Should().BeNull("the folder editor leaves the stored sort order as it is");
    }

    [Fact]
    public async Task SaveAsync_WhenNameMissing_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, []);
        sut.Name = "   ";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("폴더 이름을 입력하세요.");
        repository.LastFolder.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_WhenNameDuplicatesAnotherFolder_ShowsValidationAndDoesNotWrite()
    {
        // 레거시 이름변경(FrmBibleRename/FrmUpdateFileName)의 고유성 검증을 폴더 관리로 확장 —
        // 새 폴더를 이미 있는 폴더 이름으로 만들면 거부한다(DB 왕복 전에 친절히 알림).
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var existing = new SongFolderSummary(6, "Morning", IsEnabled: true, SongCount: 2);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, [existing]);
        sut.Name = "Morning";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Contain("이미 있는 폴더 이름");
        repository.LastFolder.Should().BeNull("중복 이름이면 저장하지 않는다");
    }

    [Fact]
    public async Task SaveAsync_WhenRenamingToOwnName_IsAllowed()
    {
        // 자기 자신의 이름은 중복으로 보지 않는다(기존 폴더를 그대로 저장 가능).
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: true, SongCount: 4);
        var other = new SongFolderSummary(6, "Morning", IsEnabled: true, SongCount: 2);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, folder, [folder, other]);
        sut.IsEnabled = false; // 이름은 그대로 두고 다른 속성만 변경

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("");
        repository.LastFolder.Should().Be(new SongFolderWriteModel(3, "Evening", false));
    }

    [Fact]
    public async Task SaveAsync_DuplicateFolderName_IsCaseInsensitive()
    {
        using var fixture = TempFolderEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var existing = new SongFolderSummary(6, "Morning", IsEnabled: true, SongCount: 2);
        var sut = new FolderEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, null, [existing]);
        sut.Name = "morning"; // 대소문자만 다른 기존 이름

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Contain("이미 있는 폴더 이름");
        repository.LastFolder.Should().BeNull();
    }

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public SongFolderWriteModel? LastFolder { get; private set; }

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            LastFolder = folder;
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveFolder,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                AffectedSongIds: [],
                [folder.FolderNo],
                Issues: []));
        }

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

    private sealed class TempFolderEditorSettings : IDisposable
    {
        private TempFolderEditorSettings(string root)
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

        public static TempFolderEditorSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_FolderEditor_{Guid.NewGuid():N}"));

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
