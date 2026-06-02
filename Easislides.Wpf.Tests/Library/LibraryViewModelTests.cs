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
    public async Task AvailableInitials_ReflectLoadedSongTitles_InIndexOrder()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 3));
        repository.SongsByFolder[1] = [
            Song(10, "은혜로다", folderNo: 1),
            Song(11, "Amazing Grace", folderNo: 1),
            Song(12, "가나안", folderNo: 1),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);

        await sut.LoadAsync();

        // 가나다(ㄱ,ㅇ) → 영문(A) 순서.
        sut.AvailableInitials.Should().Equal("ㄱ", "ㅇ", "A");
    }

    [Fact]
    public async Task ClearLibrary_EmptiesAvailableInitials_NoStaleStrip()
    {
        // 라이브러리를 채운 뒤 DB 경로가 무효가 되어 ClearLibrary 가 돌면, 점프 바도 비워져야 한다(stale 방지).
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 1));
        repository.SongsByFolder[1] = [Song(10, "은혜로다", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.AvailableInitials.Should().NotBeEmpty("로드 후 머리글자 존재");

        // DB 경로를 없는 파일로 바꾸고 다시 로드 → ClearLibrary 경로.
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, @"C:\no\such\__gone__.db");
        await sut.LoadAsync();

        sut.AvailableInitials.Should().BeEmpty("곡이 없으면 점프 바도 비어야 함");
    }

    [Fact]
    public async Task CompactDatabase_Success_CallsRepositoryAndReportsStatus()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository { CompactSucceeds = true };
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        await sut.CompactDatabaseAsync();

        repository.CompactCalls.Should().Be(1);
        sut.StatusMessage.Should().Contain("압축했습니다");
    }

    [Fact]
    public async Task CompactDatabase_Failure_ReportsFailureStatus()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository { CompactSucceeds = false };
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        await sut.CompactDatabaseAsync();

        repository.CompactCalls.Should().Be(1);
        sut.StatusMessage.Should().Contain("압축 실패");
    }

    [Fact]
    public async Task SortMode_TitleAndNumber_ReordersDisplayedSongs_WithoutRefetch()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 3));
        // 제목 순서와 번호 순서가 달라 정렬 효과를 구분할 수 있게 데이터를 짠다.
        // (테스트용 FakeAdminDatabaseRepository 는 DB 처럼 번호순으로 돌려준다 → Original=번호순.)
        repository.SongsByFolder[1] = [
            Song(10, "다윗의 노래", folderNo: 1, songNumber: 1),
            Song(11, "가나안", folderNo: 1, songNumber: 2),
            Song(12, "나의 찬양", folderNo: 1, songNumber: 3),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.Songs.Select(s => s.Title).Should().Equal("다윗의 노래", "가나안", "나의 찬양"); // 원래 순서(=번호순 1,2,3)
        var fetchesAfterLoad = repository.RequestedFolderNos.Count;

        sut.SortMode = LibrarySortMode.Title;
        sut.Songs.Select(s => s.Title).Should().Equal("가나안", "나의 찬양", "다윗의 노래"); // 제목순(가<나<다)

        sut.SortMode = LibrarySortMode.Number;
        sut.Songs.Select(s => s.Title).Should().Equal("다윗의 노래", "가나안", "나의 찬양"); // 번호순(1,2,3)

        repository.RequestedFolderNos.Count.Should().Be(fetchesAfterLoad, "정렬 변경은 DB 재조회 없이 표시만 재정렬");
    }

    [Fact]
    public void UseSongNumbering_InitializesFromSettings_AndPersistsOnToggle()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.Settings.Set(EasiSettingKeys.UseSongNumbering, true); // 저장된 값 = on
        var sut = new LibraryViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.UseSongNumbering.Should().BeTrue("생성 시 설정값을 읽어 초기화");

        sut.UseSongNumbering = false;

        fixture.Settings.Get(EasiSettingKeys.UseSongNumbering).Should().BeFalse("토글하면 설정에 저장(다음 실행 유지)");
    }

    [Fact]
    public async Task SongMove_DisabledWhileSorted_EnabledInOriginalOrder()
    {
        // 정렬(제목/번호) 중에는 화면 순서와 DB 순서가 달라 수동 이동이 헷갈리므로 이동 버튼을 비활성화한다.
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 2));
        repository.SongsByFolder[1] = [
            Song(10, "가", folderNo: 1, songNumber: 1),
            Song(11, "나", folderNo: 1, songNumber: 2),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.SelectedSong = sut.Songs.Last(); // 두 번째 곡 선택 → 원래 순서면 위로 이동 가능.
        sut.MoveSelectedSongUpCommand.CanExecute(null).Should().BeTrue("원래 순서에서는 이동 가능");

        sut.SortMode = LibrarySortMode.Title;

        sut.MoveSelectedSongUpCommand.CanExecute(null).Should().BeFalse("정렬 중에는 이동 비활성");
        sut.MoveSelectedSongDownCommand.CanExecute(null).Should().BeFalse();

        sut.SortMode = LibrarySortMode.Original;
        sut.MoveSelectedSongUpCommand.CanExecute(null).Should().BeTrue("원래 순서로 돌아오면 다시 활성");
    }

    [Fact]
    public async Task MoveSelectedSongToIndex_WhileSorted_IsNoOp_DoesNotPersist()
    {
        // 드래그 경로(MoveSelectedSongTo*) 도 정렬 중에는 막아야 한다 — CanExecute 우회 방지(심층 방어).
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 2));
        repository.SongsByFolder[1] = [
            Song(10, "가", folderNo: 1, songNumber: 1),
            Song(11, "나", folderNo: 1, songNumber: 2),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.SelectedSong = sut.Songs.First();
        sut.SortMode = LibrarySortMode.Title;
        var reordersBefore = repository.ReorderSongsCalls;

        await sut.MoveSelectedSongToEndAsync(); // 드래그-드롭이 호출하는 경로.

        repository.ReorderSongsCalls.Should().Be(reordersBefore, "정렬 중 드래그 이동은 DB 에 저장하지 않음");
        sut.StatusMessage.Should().Contain("정렬 중에는");
    }

    [Fact]
    public async Task JumpToInitial_SelectsFirstSongWithThatInitial()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 3));
        repository.SongsByFolder[1] = [
            Song(10, "가나안", folderNo: 1),
            Song(11, "은혜로다", folderNo: 1),
            Song(12, "은총", folderNo: 1),
        ];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        sut.JumpToInitialCommand.Execute("ㅇ");

        sut.SelectedSong!.Title.Should().Be("은혜로다", "ㅇ 으로 시작하는 첫 곡");
    }

    [Fact]
    public async Task JumpToInitial_NoMatchOrEmpty_LeavesSelectionUnchanged()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 1));
        repository.SongsByFolder[1] = [Song(10, "가나안", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        var before = sut.SelectedSong;

        sut.JumpToInitialCommand.Execute("ㅎ"); // 일치 없음
        sut.JumpToInitialCommand.Execute("");   // 빈 글자

        sut.SelectedSong.Should().Be(before, "일치 없거나 빈 글자면 선택 유지");
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
    public async Task MoveSelectedFolderToIndexAsync_ReordersAcrossMultipleFolders()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange([
            new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 1),
            new SongFolderSummary(2, "Evening", IsEnabled: true, SongCount: 1),
            new SongFolderSummary(3, "Late", IsEnabled: true, SongCount: 1),
        ]);
        repository.SongsByFolder[1] = [Song(10, "Opening", folderNo: 1)];
        repository.SongsByFolder[2] = [Song(20, "Middle", folderNo: 2)];
        repository.SongsByFolder[3] = [Song(30, "Closing", folderNo: 3)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        await sut.MoveSelectedFolderToIndexAsync(2);

        repository.LastFolderOrder.Should().Equal(
            new FolderOrderRequest(1, 3),
            new FolderOrderRequest(2, 1),
            new FolderOrderRequest(3, 2));
        sut.Folders.Select(folder => folder.Name).Should().Equal("Evening", "Late", "Morning");
        sut.SelectedFolder.Should().NotBeNull();
        sut.SelectedFolder!.Name.Should().Be("Morning");
        sut.SelectedFolder.FolderNo.Should().Be(3);
        sut.Songs.Should().ContainSingle().Which.SongId.Should().Be(10);
    }

    [Fact]
    public async Task DeleteSelectedFolderAsync_DisablesSelectedFolderAndReloadsSelection()
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
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        sut.DeleteSelectedFolderCommand.CanExecute(null).Should().BeTrue();
        await sut.DeleteSelectedFolderAsync();

        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastFolderDeletes.Should().Equal(new FolderDeleteRequest(1));
        sut.SelectedFolder.Should().NotBeNull();
        sut.SelectedFolder!.FolderNo.Should().Be(1);
        sut.SelectedFolder.IsEnabled.Should().BeFalse();
        sut.Songs.Should().ContainSingle().Which.SongId.Should().Be(10);
        sut.DeleteSelectedFolderCommand.CanExecute(null).Should().BeFalse();
        sut.RecoverSelectedFolderCommand.CanExecute(null).Should().BeTrue();
        sut.StatusMessage.Should().Be("Folder disabled.");
    }

    [Fact]
    public async Task RecoverSelectedFolderAsync_EnablesSelectedDisabledFolderAndReloadsSelection()
    {
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "Archive", IsEnabled: false, SongCount: 1));
        repository.SongsByFolder[1] = [Song(10, "Archived Song", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();

        sut.RecoverSelectedFolderCommand.CanExecute(null).Should().BeTrue();
        await sut.RecoverSelectedFolderAsync();

        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        repository.LastFolderRecoveries.Should().Equal(new FolderRecoveryRequest(1));
        sut.SelectedFolder.Should().NotBeNull();
        sut.SelectedFolder!.FolderNo.Should().Be(1);
        sut.SelectedFolder.IsEnabled.Should().BeTrue();
        sut.Songs.Should().ContainSingle().Which.SongId.Should().Be(10);
        sut.RecoverSelectedFolderCommand.CanExecute(null).Should().BeFalse();
        sut.DeleteSelectedFolderCommand.CanExecute(null).Should().BeTrue();
        sut.StatusMessage.Should().Be("Folder restored.");
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

    [Fact]
    public async Task MoveSelectedSongToIndexAsync_RenumbersAcrossMultipleRows()
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
        sut.SelectedSong = sut.Songs[0];

        await sut.MoveSelectedSongToIndexAsync(2);

        repository.LastSongOrder.Should().Equal(
            new SongOrderRequest(11, 1),
            new SongOrderRequest(12, 2),
            new SongOrderRequest(10, 3));
        sut.Songs.Select(song => song.SongId).Should().Equal(11, 12, 10);
        sut.SelectedSong.Should().NotBeNull();
        sut.SelectedSong!.SongId.Should().Be(10);
    }

    // ── 증분157: 라이브러리 곡 목록 빈 상태 안내(LibraryEmptyStateMessage / HasLibraryEmptyState) ──

    [Fact]
    public void LibraryEmptyState_BeforeAnyLoad_IsHidden()
    {
        // 아직 한 번도 안 불러온 처음 화면 — 곡이 0건이어도 "곡 없음" 안내를 띄우지 않는다(섣부른 안내 방지).
        using var fixture = TempLibrarySettings.Create();
        var sut = new LibraryViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.HasLibraryEmptyState.Should().BeFalse("불러오기 전에는 빈 상태 안내가 뜨면 안 됨");
        sut.LibraryEmptyStateMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LibraryEmptyState_AfterLoadingEmptyFolder_ShowsEmptyFolderMessage()
    {
        // 폴더는 불러왔는데 곡이 0건 → "이 폴더에 곡이 없습니다." 안내.
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "빈 폴더", IsEnabled: true, SongCount: 0));
        // SongsByFolder[1] 을 두지 않음 → GetSongsAsync 가 빈 목록을 돌려준다.
        var sut = new LibraryViewModel(fixture.Settings, repository);

        await sut.LoadAsync();

        sut.Songs.Should().BeEmpty();
        sut.HasLibraryEmptyState.Should().BeTrue("폴더를 불러왔는데 곡이 0건이면 안내가 떠야 함");
        sut.LibraryEmptyStateMessage.Should().Be("이 폴더에 곡이 없습니다.");
    }

    [Fact]
    public async Task LibraryEmptyState_WhenSearchMatchesNothing_ShowsSearchMessageWithTerm()
    {
        // 곡이 있는 폴더를 불러온 뒤 아무 곡과도 안 맞는 검색어 → "'검색어'에 맞는 곡이 없습니다." 안내.
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 1));
        repository.SongsByFolder[1] = [Song(10, "은혜로다", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.HasLibraryEmptyState.Should().BeFalse("로드 직후엔 곡이 있어 안내가 없어야 함");

        sut.SearchText = "없는검색어zzz";

        sut.Songs.Should().BeEmpty("검색 결과 0건");
        sut.HasLibraryEmptyState.Should().BeTrue();
        sut.LibraryEmptyStateMessage.Should().Be("'없는검색어zzz'에 맞는 곡이 없습니다.");
    }

    [Fact]
    public async Task LibraryEmptyState_WhenFolderHasSongs_IsHidden()
    {
        // 표시할 곡이 있으면 안내는 숨김(빈 문자열).
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 1));
        repository.SongsByFolder[1] = [Song(10, "은혜로다", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);

        await sut.LoadAsync();

        sut.Songs.Should().NotBeEmpty();
        sut.HasLibraryEmptyState.Should().BeFalse("곡이 있으면 빈 상태 안내가 뜨면 안 됨");
        sut.LibraryEmptyStateMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LibraryEmptyState_AfterClearLibraryOnInvalidPath_IsHidden_SoStatusBarExplains()
    {
        // 곡을 불러온 뒤 DB 경로가 무효가 되어 ClearLibrary 가 돌면, 빈 상태 안내는 숨겨야 한다.
        // 원인은 "빈 폴더"가 아니라 "경로 없음"이므로, 오해 소지 오버레이 대신 상태바 메시지가 알리도록 한다.
        using var fixture = TempLibrarySettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.Add(new SongFolderSummary(1, "찬양", IsEnabled: true, SongCount: 1));
        repository.SongsByFolder[1] = [Song(10, "은혜로다", folderNo: 1)];
        var sut = new LibraryViewModel(fixture.Settings, repository);
        await sut.LoadAsync();
        sut.HasLoadedSongs.Should().BeTrue("로드 후엔 '불러옴' 상태");

        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, @"C:\no\such\__gone__.db");
        await sut.LoadAsync();

        sut.HasLoadedSongs.Should().BeFalse("경로 실패로 비우면 '불러옴' 상태가 해제돼야 함");
        sut.HasLibraryEmptyState.Should().BeFalse("경로 실패로 비운 라이브러리에는 '곡 없음' 오버레이를 띄우지 않음");
        sut.LibraryEmptyStateMessage.Should().BeEmpty();
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

        public IReadOnlyList<FolderDeleteRequest> LastFolderDeletes { get; private set; } = [];

        public IReadOnlyList<FolderRecoveryRequest> LastFolderRecoveries { get; private set; } = [];

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

        public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(
            string databasePath,
            string backupRoot,
            IReadOnlyList<FolderDeleteRequest> deletes)
        {
            LastBackupRoot = backupRoot;
            LastFolderDeletes = deletes.ToArray();
            SetFolderEnabled(deletes.Select(delete => delete.FolderNo), isEnabled: false);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SoftDeleteFolders,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                AffectedSongIds: [],
                deletes.Select(delete => delete.FolderNo).ToArray(),
                Issues: []));
        }

        public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(
            string databasePath,
            string backupRoot,
            IReadOnlyList<FolderRecoveryRequest> recoveries)
        {
            LastBackupRoot = backupRoot;
            LastFolderRecoveries = recoveries.ToArray();
            SetFolderEnabled(recoveries.Select(recovery => recovery.FolderNo), isEnabled: true);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.RecoverFolders,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                AffectedSongIds: [],
                recoveries.Select(recovery => recovery.FolderNo).ToArray(),
                Issues: []));
        }

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

        public int ReorderSongsCalls { get; private set; }

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(
            string databasePath,
            string backupRoot,
            int folderNo,
            IReadOnlyList<SongOrderRequest> order)
        {
            ReorderSongsCalls++;
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

        public int CompactCalls { get; private set; }

        public bool CompactSucceeds { get; set; } = true;

        public Task<AdminDatabaseWriteReport> CompactDatabaseAsync(string databasePath, string backupRoot)
        {
            CompactCalls++;
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: CompactSucceeds,
                AdminDatabaseWriteOperation.Compact,
                databasePath,
                CompactSucceeds ? Path.Combine(backupRoot, "backup.db") : null,
                AffectedSongIds: [],
                AffectedFolderNos: [],
                Issues: CompactSucceeds
                    ? []
                    : [new AdminDatabaseWriteIssue(AdminDatabaseWriteIssueKind.WriteFailed, AdminDatabaseIssueSeverity.Error, "boom")]));
        }

        private void SetFolderEnabled(IEnumerable<int> folderNos, bool isEnabled)
        {
            var targets = folderNos.ToHashSet();
            var updated = Folders
                .Select(folder => targets.Contains(folder.FolderNo)
                    ? folder with { IsEnabled = isEnabled }
                    : folder)
                .ToArray();
            Folders.Clear();
            Folders.AddRange(updated);
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
