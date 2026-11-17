using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class ExternalFileOperationServiceTests
{
    [Fact]
    public async Task ExecuteAsync_CopyToExternalFolder_CreatesTargetAndUsesLegacyCollisionName()
    {
        using var fixture = TempExternalFiles.Create();
        var source = fixture.WriteSource("notice.esi", "source");
        fixture.WriteTarget("notice.esi", "existing");
        var sut = new ExternalFileOperationService(new FakeAdminRepository());

        var report = await sut.ExecuteAsync(new ExternalFileOperationRequest(
            ExternalFileOperationKind.Copy,
            ExternalFileItemKind.InfoScreen,
            ExternalFileDestinationKind.ExternalFolder,
            [source],
            TargetFolderPath: fixture.TargetRoot));

        report.Succeeded.Should().BeTrue();
        report.CreatedFilePaths.Should().ContainSingle()
            .Which.Should().EndWith(Path.Combine("target", "notice - Copy (1).esi"));
        File.ReadAllText(source).Should().Be("source");
        File.ReadAllText(Path.Combine(fixture.TargetRoot, "notice - Copy (1).esi")).Should().Be("source");
    }

    [Fact]
    public async Task ExecuteAsync_MoveToExternalFolder_RemovesSourceAndKeepsCollisionRule()
    {
        using var fixture = TempExternalFiles.Create();
        var source = fixture.WriteSource("deck.pptx", "deck");
        fixture.WriteTarget("deck.pptx", "existing");
        var sut = new ExternalFileOperationService(new FakeAdminRepository());

        var report = await sut.ExecuteAsync(new ExternalFileOperationRequest(
            ExternalFileOperationKind.Move,
            ExternalFileItemKind.PowerPoint,
            ExternalFileDestinationKind.ExternalFolder,
            [source],
            TargetFolderPath: fixture.TargetRoot));

        report.Succeeded.Should().BeTrue();
        File.Exists(source).Should().BeFalse();
        File.ReadAllText(Path.Combine(fixture.TargetRoot, "deck - Copy (1).pptx")).Should().Be("deck");
    }

    [Fact]
    public async Task ExecuteAsync_CopyInfoScreensToSongFolder_ParsesXmlAndSavesSongs()
    {
        using var fixture = TempExternalFiles.Create();
        var database = fixture.WriteDatabase();
        var source = fixture.WriteSource(
            "service.esi",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <EasiSlides>
              <Item>
                <Title1>Service Notice</Title1>
                <Title2>Alt Notice</Title2>
                <SongNumber>9</SongNumber>
                <Contents>[1]
            Welcome</Contents>
                <Notations>(1;note)</Notations>
                <Sequence>1</Sequence>
                <Writer>Team</Writer>
                <Copyright>CCLI</Copyright>
                <Category>Info</Category>
                <Timing>4/4</Timing>
                <MusicKey>G</MusicKey>
                <Capo>2</Capo>
                <LicenceAdmin1>A</LicenceAdmin1>
                <LicenceAdmin2>B</LicenceAdmin2>
                <BookReference>Book</BookReference>
                <UserReference>User</UserReference>
                <FormatData>format</FormatData>
                <Settings>settings</Settings>
              </Item>
            </EasiSlides>
            """);
        var repository = new FakeAdminRepository();
        var sut = new ExternalFileOperationService(repository);

        var report = await sut.ExecuteAsync(new ExternalFileOperationRequest(
            ExternalFileOperationKind.Copy,
            ExternalFileItemKind.InfoScreen,
            ExternalFileDestinationKind.SongFolder,
            [source],
            DatabasePath: database,
            TargetSongFolderNo: 7,
            BackupRoot: fixture.BackupRoot,
            StartingSongNumber: 4));

        report.Succeeded.Should().BeTrue();
        report.CreatedSongIds.Should().ContainSingle().Which.Should().Be(101);
        repository.SavedSongs.Should().ContainSingle();
        var saved = repository.SavedSongs[0];
        saved.Title.Should().Be("Service Notice");
        saved.AlternateTitle.Should().Be("Alt Notice");
        saved.FolderNo.Should().Be(7);
        saved.SongNumber.Should().Be(4);
        saved.Lyrics.Should().Be("[1]\nWelcome");
        saved.Notations.Should().Be("(1;note)");
        saved.Sequence.Should().Be("1");
        saved.Writer.Should().Be("Team");
        saved.Copyright.Should().Be("CCLI");
        saved.Category.Should().Be("Info");
        saved.Timing.Should().Be("4/4");
        saved.Key.Should().Be("G");
        saved.Capo.Should().Be(2);
        saved.LicenceAdmin1.Should().Be("A");
        saved.LicenceAdmin2.Should().Be("B");
        saved.BookReference.Should().Be("Book");
        saved.UserReference.Should().Be("User");
        saved.FormatData.Should().Be("format");
        saved.Settings.Should().Be("settings");
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
    }

    [Fact]
    public void GetFolders_ReturnsLegacyRootAndSortedSubfolderNames()
    {
        using var fixture = TempExternalFiles.Create();
        Directory.CreateDirectory(Path.Combine(fixture.WorkingRoot, "InfoScreens", "B"));
        Directory.CreateDirectory(Path.Combine(fixture.WorkingRoot, "InfoScreens", "A", "Child"));
        var sut = new ExternalFileOperationService(new FakeAdminRepository());

        var folders = sut.GetFolders(fixture.WorkingRoot, ExternalFileItemKind.InfoScreen);

        folders.Select(folder => folder.Name).Should().Equal(
            "InfoScreen Items",
            "\\A",
            "\\A\\Child",
            "\\B");
        folders.Should().OnlyContain(folder => folder.Path.EndsWith(Path.DirectorySeparatorChar));
    }

    [Fact]
    public void GetFolders_Media_ReturnsLegacyMediaRootAndSortedSubfolderNames()
    {
        using var fixture = TempExternalFiles.Create();
        Directory.CreateDirectory(Path.Combine(fixture.WorkingRoot, "Media", "Video"));
        Directory.CreateDirectory(Path.Combine(fixture.WorkingRoot, "Media", "Audio", "Loops"));
        var sut = new ExternalFileOperationService(new FakeAdminRepository());

        var folders = sut.GetFolders(fixture.WorkingRoot, ExternalFileItemKind.Media);

        folders.Select(folder => folder.Name).Should().Equal(
            "Media Items",
            "\\Audio",
            "\\Audio\\Loops",
            "\\Video");
        folders[0].Path.Should().Be(Path.Combine(Path.GetFullPath(fixture.WorkingRoot), "Media") + Path.DirectorySeparatorChar);
        folders.Should().OnlyContain(folder => folder.Path.EndsWith(Path.DirectorySeparatorChar));
    }

    private sealed class FakeAdminRepository : IAdminDatabaseRepository
    {
        public List<SongWriteModel> SavedSongs { get; } = [];

        public string LastBackupRoot { get; private set; } = "";

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
            LastBackupRoot = backupRoot;
            SavedSongs.Add(song);
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveSong,
                databasePath,
                Path.Combine(backupRoot, "admin.bak"),
                [100 + SavedSongs.Count],
                [song.FolderNo],
                []));
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

    private sealed class TempExternalFiles : IDisposable
    {
        private TempExternalFiles(string root)
        {
            Root = root;
            SourceRoot = Path.Combine(root, "source");
            TargetRoot = Path.Combine(root, "target");
            WorkingRoot = Path.Combine(root, "work");
            BackupRoot = Path.Combine(root, "backups");
            Directory.CreateDirectory(SourceRoot);
            Directory.CreateDirectory(TargetRoot);
            Directory.CreateDirectory(WorkingRoot);
            Directory.CreateDirectory(BackupRoot);
        }

        public string Root { get; }

        public string SourceRoot { get; }

        public string TargetRoot { get; }

        public string WorkingRoot { get; }

        public string BackupRoot { get; }

        public static TempExternalFiles Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_ExternalFiles_{Guid.NewGuid():N}"));

        public string WriteSource(string name, string content)
            => WriteFile(SourceRoot, name, content);

        public string WriteTarget(string name, string content)
            => WriteFile(TargetRoot, name, content);

        public string WriteDatabase()
            => WriteFile(Root, "admin.db", "sqlite");

        private static string WriteFile(string root, string name, string content)
        {
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, name);
            File.WriteAllText(path, content);
            return path;
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
