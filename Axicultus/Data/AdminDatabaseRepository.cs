using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Data;

public enum AdminDatabaseIssueKind
{
    SourceMissing,
    SourceNotFile,
    OpenFailed,
    MissingTable,
    MissingColumn,
}

public enum AdminDatabaseIssueSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record AdminDatabaseIssue(
    AdminDatabaseIssueKind Kind,
    AdminDatabaseIssueSeverity Severity,
    string Message,
    string? TableName = null,
    string? ColumnName = null);

public sealed record DatabaseColumn(
    string TableName,
    string Name,
    string Type,
    bool IsNotNull,
    bool IsPrimaryKey,
    int Ordinal);

public sealed record AdminDatabaseSchemaInventory(
    bool Succeeded,
    string DatabasePath,
    int SchemaVersion,
    IReadOnlyList<DatabaseTable> Tables,
    IReadOnlyDictionary<string, IReadOnlyList<DatabaseColumn>> Columns,
    IReadOnlyList<AdminDatabaseIssue> Issues);

public sealed record SongFolderSummary(
    int FolderNo,
    string Name,
    bool IsEnabled,
    int SongCount,
    int GroupStyle = 0,
    SongPageLayout? SlideLayout = null,
    IReadOnlyDictionary<string, string>? FormatFields = null,
    // Deleted songs that recovery would file in this folder (their OldFolder).
    int DeletedSongCount = 0);

public sealed record SongSummary(
    int SongId,
    string Title,
    string AlternateTitle,
    int FolderNo,
    int SongNumber,
    string Category,
    string Key,
    string Lyrics,
    // 저작권(CCLI 등) — 출력 "저작권 표시"(Display Panel)에 쓰인다. 기존 8필드 구성과의 호환을 위해 기본값.
    string Copyright = "",
    // 레거시 BOOK_REFERENCE/USER_REFERENCE — FrmMain Reference alert source 3/4 에 쓰인다.
    string BookReference = "",
    string UserReference = "",
    string Writer = "",
    string? Sequence = null,
    string? FormatData = null,
    string LicenceAdmin1 = "",
    string LicenceAdmin2 = "",
    string? StrokeSortKey = null,
    string? WordSortKey = null,
    int Capo = 0,
    string Notations = "");

public sealed record SongDetail(
    int SongId,
    string Title,
    string AlternateTitle,
    int FolderNo,
    int SongNumber,
    string Lyrics,
    string Sequence,
    string Writer,
    string Copyright,
    int Capo,
    string Timing,
    string Key,
    string Notations,
    string Category,
    string LicenceAdmin1,
    string LicenceAdmin2,
    string BookReference,
    string UserReference,
    string Settings,
    string FormatData);

public sealed record DeletedSongSummary(
    int SongId,
    string Title,
    int OriginalFolderNo,
    string OriginalFolderName,
    DateTime? DeletedOn);

public enum AdminDatabaseWriteOperation
{
    SaveFolder,
    SoftDeleteFolders,
    RecoverFolders,
    SaveSong,
    MoveSongs,
    SoftDeleteSongs,
    RecoverSongs,
    ReorderFolders,
    ReorderSongs,
    Compact,
    EmptyDeletedSongs,
    ClearSongFormatting,
    SaveSongMedia,
    SaveSongBackground,
}

public enum AdminDatabaseWriteIssueKind
{
    SourceMissing,
    SourceNotFile,
    OpenFailed,
    SchemaIncompatible,
    InvalidRequest,
    BackupFailed,
    WriteFailed,
    RestoreFailed,
    NotFound,
}

public sealed record AdminDatabaseWriteIssue(
    AdminDatabaseWriteIssueKind Kind,
    AdminDatabaseIssueSeverity Severity,
    string Message);

/// <summary>
/// Folder write request. <paramref name="GroupStyle"/> is the folder sort order
/// (0 = title, 1 = word count, 2 = song number); null leaves the stored order of an
/// existing folder unchanged and gives a new folder 0.
/// </summary>
public sealed record SongFolderWriteModel(
    int FolderNo,
    string Name,
    bool IsEnabled,
    int? GroupStyle = null,
    IReadOnlyDictionary<string, string>? FormatChanges = null,
    // A new folder in a row that exists already (a slot used before): the row's sort order and formatting
    // are set to what the WinForms app gives a new folder.
    bool ResetFormat = false);

public sealed record SongWriteModel(
    int? SongId,
    string Title,
    string AlternateTitle,
    int FolderNo,
    int SongNumber,
    string Lyrics,
    string Sequence = "",
    string Writer = "",
    string Copyright = "",
    // -1 is "no capo", as WinForms stores it.
    int Capo = -1,
    string Timing = "",
    string Key = "",
    string Notations = "",
    string Category = "",
    string LicenceAdmin1 = "",
    string LicenceAdmin2 = "",
    string BookReference = "",
    string UserReference = "",
    string Settings = "",
    string FormatData = "",
    // True when Sequence is a stored SEQUENCE value (a copy): it is written as it is, not read as typed tokens.
    bool SequenceIsStored = false);

public sealed record SongMoveRequest(
    int SongId,
    int OldFolderNo,
    int NewFolderNo,
    bool UpdateModifiedDate = false);

public sealed record SongDeleteRequest(
    int SongId,
    int OriginalFolderNo);

public sealed record SongRecoveryRequest(
    int SongId,
    int TargetFolderNo);

public sealed record FolderDeleteRequest(int FolderNo);

public sealed record FolderRecoveryRequest(int FolderNo);

public sealed record FolderOrderRequest(
    int FolderNo,
    int NewFolderNo);

public sealed record SongOrderRequest(
    int SongId,
    int SongNumber);

public sealed record AdminDatabaseWriteReport(
    bool Succeeded,
    AdminDatabaseWriteOperation Operation,
    string DatabasePath,
    string? BackupPath,
    IReadOnlyList<int> AffectedSongIds,
    IReadOnlyList<int> AffectedFolderNos,
    IReadOnlyList<AdminDatabaseWriteIssue> Issues);

public interface IAdminDatabaseRepository
{
    Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath);

    Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath);

    Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null);

    async Task<Dictionary<string, int>> GetSongTitleIndexAsync(string databasePath, int folderNo)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var song in await GetSongsAsync(databasePath, folderNo).ConfigureAwait(false))
            index.TryAdd(song.Title, song.SongId);
        return index;
    }

    async Task<IReadOnlyList<SongSummary>> GetMatchingSongsAsync(string databasePath, IReadOnlySet<int> ids, IReadOnlySet<string> titles)
        => (await GetSongsAsync(databasePath).ConfigureAwait(false)).Where(s => s.FolderNo > 0 &&
            (ids.Contains(s.SongId) || titles.Contains(s.Title))).ToArray();

    Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath);

    Task<AdminDatabaseWriteReport> SaveSongBackgroundAsync(string databasePath, string backupRoot, int songId, string path)
        => throw new NotSupportedException("This repository does not support song backgrounds.");

    Task<AdminDatabaseWriteReport> SaveSongMediaAsync(string databasePath, string backupRoot, int songId, string path, Media.ItemMediaOptions? options = null)
        => throw new NotSupportedException("This repository does not support song media assignments.");

    Task<AdminDatabaseWriteReport> MaintainSongsAsync(string databasePath, string backupRoot, AdminDatabaseWriteOperation operation)
        => throw new NotSupportedException("This repository does not support song maintenance.");


    Task<AdminDatabaseWriteReport> SaveFolderAsync(
        string databasePath,
        string backupRoot,
        SongFolderWriteModel folder);

    Task<AdminDatabaseWriteReport> SaveFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<SongFolderWriteModel> folders)
        => folders.Count == 1 ? SaveFolderAsync(databasePath, backupRoot, folders[0]) : throw new NotSupportedException();

    Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<FolderDeleteRequest> deletes);

    Task<AdminDatabaseWriteReport> RecoverFoldersAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<FolderRecoveryRequest> recoveries);

    Task<AdminDatabaseWriteReport> SaveSongAsync(
        string databasePath,
        string backupRoot,
        SongWriteModel song);

    /// <summary>
    /// Saves several songs as one operation: one backup and one transaction, so a failure saves none of them.
    /// AffectedSongIds holds one id per song, in the order given. (This default, for test doubles, saves one by one.)
    /// </summary>
    async Task<AdminDatabaseWriteReport> SaveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongWriteModel> songs)
    {
        var ids = new List<int>();
        string? backupPath = null;
        foreach (var song in songs)
        {
            var report = await SaveSongAsync(databasePath, backupRoot, song).ConfigureAwait(false);
            if (!report.Succeeded) return report;
            ids.AddRange(report.AffectedSongIds);
            backupPath ??= report.BackupPath;
        }

        return new AdminDatabaseWriteReport(true, AdminDatabaseWriteOperation.SaveSong, databasePath, backupPath, ids, [], []);
    }

    Task<AdminDatabaseWriteReport> MoveSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongMoveRequest> moves);

    Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongDeleteRequest> deletes);

    Task<AdminDatabaseWriteReport> RecoverSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongRecoveryRequest> recoveries);

    Task<AdminDatabaseWriteReport> ReorderFoldersAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<FolderOrderRequest> order);

    Task<AdminDatabaseWriteReport> ReorderSongsAsync(
        string databasePath,
        string backupRoot,
        int folderNo,
        IReadOnlyList<SongOrderRequest> order);

    /// <summary>
    /// 데이터베이스 압축·정리(레거시 Tools "Compact &amp; Repair") — 백업을 먼저 만든 뒤 SQLite VACUUM 으로
    /// 빈 공간을 회수하고 단편화를 줄인다. 실패하면 백업으로 복원한다.
    /// 기본 구현은 "지원 안 함" 실패 보고(테스트용 가짜 저장소가 일일이 구현하지 않아도 되게 함 — 실제 저장소는 재정의).
    /// </summary>
    Task<AdminDatabaseWriteReport> CompactDatabaseAsync(string databasePath, string backupRoot)
        => Task.FromResult(new AdminDatabaseWriteReport(
            Succeeded: false,
            AdminDatabaseWriteOperation.Compact,
            databasePath,
            BackupPath: null,
            AffectedSongIds: [],
            AffectedFolderNos: [],
            Issues: [new AdminDatabaseWriteIssue(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                AdminDatabaseIssueSeverity.Error,
                "Compaction is not supported by this repository.")]));
}

public interface IAdminSongDetailRepository
{
    Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId);
}

public sealed class AdminDatabaseRepository : IAdminDatabaseRepository, IAdminSongDetailRepository
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RequiredSchema =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["FOLDER"] = ["FolderNo", "Name", "Use"],
            ["SONG"] = ["SONGID", "TITLE_1", "TITLE_2", "FOLDERNO", "LYRICS", "SONG_NUMBER", "CATEGORY", "KEY"],
        };

    public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
        => Task.FromResult(AnalyzeSchema(databasePath));

    public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
        => Task.FromResult<IReadOnlyList<SongFolderSummary>>(GetSongFolders(databasePath));

    public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
        => Task.FromResult<IReadOnlyList<SongSummary>>(GetSongs(databasePath, folderNo));

    public Task<Dictionary<string, int>> GetSongTitleIndexAsync(string databasePath, int folderNo)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        using var command = new SQLiteCommand("SELECT TITLE_1, SONGID FROM SONG WHERE FOLDERNO = @folder ORDER BY SONG_NUMBER, TITLE_1, SONGID", connection);
        command.Parameters.AddWithValue("@folder", folderNo);
        using var reader = command.ExecuteReader();
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read()) index.TryAdd(GetString(reader, "TITLE_1"), GetInt(reader, "SONGID"));
        return Task.FromResult(index);
    }

    public Task<IReadOnlyList<SongSummary>> GetMatchingSongsAsync(string databasePath, IReadOnlySet<int> ids, IReadOnlySet<string> titles)
        => Task.FromResult<IReadOnlyList<SongSummary>>(GetSongs(databasePath, null, ids, titles));

    public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
        => Task.FromResult(GetSongDetail(databasePath, songId));

    public Task<AdminDatabaseWriteReport> SaveSongMediaAsync(string databasePath, string backupRoot, int songId, string path, Media.ItemMediaOptions? options = null)
        => Task.FromResult(UpdateSongFormat(databasePath, backupRoot, songId, AdminDatabaseWriteOperation.SaveSongMedia,
            raw => options?.ApplyTo(raw) ?? Library.SongFormatData.WithMediaPath(raw, path)));

    public Task<AdminDatabaseWriteReport> SaveSongBackgroundAsync(string databasePath, string backupRoot, int songId, string path)
        => Task.Run(() => UpdateSongFormat(databasePath, backupRoot, songId, AdminDatabaseWriteOperation.SaveSongBackground,
            raw => Library.SongFormatData.WithBackgroundPath(raw, path)));

    private AdminDatabaseWriteReport UpdateSongFormat(string databasePath, string backupRoot, int songId,
        AdminDatabaseWriteOperation operation, Func<string?, string> update)
        => ExecuteWrite(databasePath, backupRoot, operation, (connection, transaction, outcome) =>
        {
            using var read = new SQLiteCommand("SELECT FORMATDATA FROM SONG WHERE SONGID = @id AND FOLDERNO > 0", connection, transaction);
            read.Parameters.AddWithValue("@id", songId);
            var format = read.ExecuteScalar() ?? throw new InvalidOperationException(UiText.Get("AdminDatabaseRepository", "SongNotFound"));
            using var save = new SQLiteCommand("UPDATE SONG SET FORMATDATA = @format WHERE SONGID = @id", connection, transaction);
            save.Parameters.AddWithValue("@format", update(Convert.ToString(format)));
            save.Parameters.AddWithValue("@id", songId);
            save.ExecuteNonQuery();
            outcome.SongIds.Add(songId);
        }, backup: false); // One song's media or background file: assigned again in one step, not worth a database copy.

    public Task<AdminDatabaseWriteReport> MaintainSongsAsync(string databasePath, string backupRoot, AdminDatabaseWriteOperation operation)
    {
        string sql = operation switch
        {
            AdminDatabaseWriteOperation.EmptyDeletedSongs => "DELETE FROM SONG WHERE FolderNo = 0",
            AdminDatabaseWriteOperation.ClearSongFormatting => "UPDATE SONG SET FORMATDATA = '' WHERE FORMATDATA <> ''",
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        return Task.FromResult(ExecuteWrite(databasePath, backupRoot, operation, (connection, transaction, _) =>
        {
            using var command = new SQLiteCommand(sql, connection, transaction);
            command.ExecuteNonQuery();
        }));
    }

    public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
        => Task.FromResult<IReadOnlyList<DeletedSongSummary>>(GetDeletedSongs(databasePath));

    public Task<AdminDatabaseWriteReport> SaveFolderAsync(
        string databasePath,
        string backupRoot,
        SongFolderWriteModel folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.SaveFolder,
            (connection, transaction, outcome) => SaveFolder(connection, transaction, folder, outcome),
            backup: !ChangesSortOrderOnly(databasePath, folder)));
    }

    // The song list saves the folder each time its sort order is changed. One click puts the order back,
    // so that write takes no database copy.
    private static bool ChangesSortOrderOnly(string databasePath, SongFolderWriteModel folder)
    {
        if (folder.GroupStyle is null || folder.ResetFormat || folder.FormatChanges is { Count: > 0 })
        {
            return false;
        }

        try
        {
            var stored = GetSongFolders(databasePath).FirstOrDefault(item => item.FolderNo == folder.FolderNo);
            return stored is not null && stored.Name == Normalize(folder.Name) && stored.IsEnabled == folder.IsEnabled;
        }
        catch (Exception ex) when (IsSqliteOpenException(ex) || IsFileSystemException(ex) || ex is InvalidOperationException)
        {
            return false; // ExecuteWrite reports what is wrong with the database.
        }
    }

    public Task<AdminDatabaseWriteReport> SaveFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<SongFolderWriteModel> folders)
        => Task.FromResult(ExecuteWrite(databasePath, backupRoot, AdminDatabaseWriteOperation.SaveFolder,
            (connection, transaction, outcome) =>
            {
                foreach (var folder in folders) SaveFolder(connection, transaction, folder, outcome);
            }));

    public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<FolderDeleteRequest> deletes)
    {
        ArgumentNullException.ThrowIfNull(deletes);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.SoftDeleteFolders,
            (connection, transaction, outcome) => SoftDeleteFolders(connection, transaction, deletes, outcome)));
    }

    public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<FolderRecoveryRequest> recoveries)
    {
        ArgumentNullException.ThrowIfNull(recoveries);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.RecoverFolders,
            (connection, transaction, outcome) => RecoverFolders(connection, transaction, recoveries, outcome)));
    }

    public Task<AdminDatabaseWriteReport> SaveSongAsync(
        string databasePath,
        string backupRoot,
        SongWriteModel song)
    {
        ArgumentNullException.ThrowIfNull(song);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.SaveSong,
            (connection, transaction, outcome) => SaveSong(connection, transaction, song, outcome)));
    }

    public Task<AdminDatabaseWriteReport> SaveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongWriteModel> songs)
    {
        ArgumentNullException.ThrowIfNull(songs);
        if (songs.Count == 0)
        {
            return Task.FromResult(new AdminDatabaseWriteReport(
                true, AdminDatabaseWriteOperation.SaveSong, databasePath, BackupPath: null, [], [], []));
        }

        return Task.FromResult(ExecuteWrite(databasePath, backupRoot, AdminDatabaseWriteOperation.SaveSong,
            (connection, transaction, outcome) =>
            {
                foreach (var song in songs) SaveSong(connection, transaction, song, outcome);
            }));
    }

    public Task<AdminDatabaseWriteReport> MoveSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongMoveRequest> moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
        // Folder 0 holds deleted songs; only SoftDeleteSongsAsync files a song there.
        if (moves.Any(move => move.NewFolderNo <= 0))
        {
            return Task.FromResult(FailedWriteReport(
                AdminDatabaseWriteOperation.MoveSongs, databasePath, backupPath: null, new AdminDatabaseWriteOutcome(),
                [WriteIssue(AdminDatabaseWriteIssueKind.InvalidRequest, UiText.Get("AdminDatabaseRepository", "TargetFolderNotPositive"))]));
        }

        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.MoveSongs,
            (connection, transaction, outcome) => MoveSongs(connection, transaction, moves, outcome)));
    }

    public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongDeleteRequest> deletes)
    {
        ArgumentNullException.ThrowIfNull(deletes);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.SoftDeleteSongs,
            (connection, transaction, outcome) => SoftDeleteSongs(connection, transaction, deletes, outcome)));
    }

    public Task<AdminDatabaseWriteReport> RecoverSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongRecoveryRequest> recoveries)
    {
        ArgumentNullException.ThrowIfNull(recoveries);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.RecoverSongs,
            (connection, transaction, outcome) => RecoverSongs(connection, transaction, recoveries, outcome)));
    }

    public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<FolderOrderRequest> order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.ReorderFolders,
            (connection, transaction, outcome) => ReorderFolders(connection, transaction, order, outcome)));
    }

    public Task<AdminDatabaseWriteReport> ReorderSongsAsync(
        string databasePath,
        string backupRoot,
        int folderNo,
        IReadOnlyList<SongOrderRequest> order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return Task.FromResult(ExecuteWrite(
            databasePath,
            backupRoot,
            AdminDatabaseWriteOperation.ReorderSongs,
            (connection, transaction, outcome) => ReorderSongs(connection, transaction, folderNo, order, outcome)));
    }

    public Task<AdminDatabaseWriteReport> CompactDatabaseAsync(string databasePath, string backupRoot)
        => Task.FromResult(CompactDatabase(databasePath, backupRoot));

    // 데이터베이스 압축(VACUUM) — 백업 후 실행. VACUUM 은 트랜잭션 안에서 못 돌리므로 ExecuteWrite(트랜잭션 래핑)를
    // 쓰지 않고 전용 경로로 처리한다.
    private static AdminDatabaseWriteReport CompactDatabase(string databasePath, string backupRoot)
    {
        var inventory = AnalyzeSchema(databasePath);
        if (!inventory.Succeeded)
        {
            return FailedWriteReport(
                AdminDatabaseWriteOperation.Compact, inventory.DatabasePath, backupPath: null,
                new AdminDatabaseWriteOutcome(), ToWriteIssues(inventory.Issues));
        }

        if (string.IsNullOrWhiteSpace(backupRoot))
        {
            return FailedWriteReport(
                AdminDatabaseWriteOperation.Compact, inventory.DatabasePath, backupPath: null,
                new AdminDatabaseWriteOutcome(),
                [WriteIssue(AdminDatabaseWriteIssueKind.InvalidRequest, UiText.Get("AdminDatabaseRepository", "BackupRootEmpty"))]);
        }

        string backupPath;
        try
        {
            backupPath = CreateBackup(inventory.DatabasePath, backupRoot);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return FailedWriteReport(
                AdminDatabaseWriteOperation.Compact, inventory.DatabasePath, backupPath: null,
                new AdminDatabaseWriteOutcome(),
                [WriteIssue(AdminDatabaseWriteIssueKind.BackupFailed, UiText.Format("AdminDatabaseRepository", "BackupFailed", ex.Message))]);
        }

        try
        {
            using var connection = OpenConnection(inventory.DatabasePath, readOnly: false);
            using var command = new SQLiteCommand("VACUUM;", connection);
            command.ExecuteNonQuery();
        }
        catch (Exception ex) when (IsSqliteOpenException(ex))
        {
            // VACUUM writes the file back inside a journalled transaction, so a failure leaves the database as it was.
            return FailedWriteReport(
                AdminDatabaseWriteOperation.Compact, inventory.DatabasePath, backupPath,
                new AdminDatabaseWriteOutcome(),
                [WriteIssue(AdminDatabaseWriteIssueKind.WriteFailed, UiText.Format("AdminDatabaseRepository", "CompactFailed", ex.Message))]);
        }

        return new AdminDatabaseWriteReport(
            Succeeded: true,
            AdminDatabaseWriteOperation.Compact,
            inventory.DatabasePath,
            backupPath,
            AffectedSongIds: [],
            AffectedFolderNos: [],
            Issues: []);
    }

    private static AdminDatabaseSchemaInventory AnalyzeSchema(string databasePath)
    {
        var pathResult = NormalizeExistingDatabasePath(databasePath);
        if (pathResult.Issue is not null)
        {
            return InventoryFailure(pathResult.Path, pathResult.Issue);
        }

        try
        {
            using var connection = OpenConnection(pathResult.Path, readOnly: true);
            var schemaVersion = ReadUserVersion(connection);
            var tables = ReadTables(connection);
            var columns = ReadColumns(connection, tables);
            var issues = ValidateCompatibility(columns);

            return new AdminDatabaseSchemaInventory(
                Succeeded: issues.Count == 0,
                pathResult.Path,
                schemaVersion,
                tables,
                columns,
                issues);
        }
        catch (Exception ex) when (IsSqliteOpenException(ex))
        {
            return InventoryFailure(
                pathResult.Path,
                Issue(AdminDatabaseIssueKind.OpenFailed, UiText.Format("AdminDatabaseRepository", "OpenFailed", ex.Message)));
        }
    }

    private static List<SongFolderSummary> GetSongFolders(string databasePath)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        var folderColumns = ReadColumnMap(connection, "FOLDER");
        using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        var songCounts = new Dictionary<int, int>();
        using (var count = new SQLiteCommand("SELECT FOLDERNO, COUNT(SONGID) AS SongCount FROM SONG WHERE FOLDERNO IS NOT NULL GROUP BY FOLDERNO", connection, transaction))
        using (var rows = count.ExecuteReader())
        {
            while (rows.Read()) songCounts[GetInt(rows, "FOLDERNO")] = GetInt(rows, "SongCount");
        }
        // Where recovery files the deleted songs: their OldFolder, or folder 1 without one (see GetDeletedSongs).
        var deletedCounts = new Dictionary<int, int>();
        if (ReadColumnMap(connection, "SONG").TryGetValue("OldFolder", out var oldFolderColumn))
        {
            using var deleted = new SQLiteCommand(
                $"SELECT MAX(COALESCE({QuoteIdentifier(oldFolderColumn)}, 0), 1) AS OldFolder, COUNT(*) AS SongCount FROM SONG WHERE FOLDERNO = 0 GROUP BY 1",
                connection, transaction);
            using var rows = deleted.ExecuteReader();
            while (rows.Read()) deletedCounts[GetInt(rows, "OldFolder")] = GetInt(rows, "SongCount");
        }
        using var command = new SQLiteCommand(
            $"""
            SELECT
                f.FolderNo,
                f.Name,
                f.Use,
                {SelectColumn(folderColumns, "GroupStyle", "GroupStyle", "f")},
                {string.Join(", ", FolderFormatField.All.Select(field => SelectColumn(folderColumns, field.Column, field.Column, "f")))}
            FROM FOLDER f
            ORDER BY f.FolderNo, f.Name;
            """,
            connection, transaction);
        using var reader = command.ExecuteReader();
        var folders = new List<SongFolderSummary>();
        while (reader.Read())
        {
            var formatting = FolderFormatField.All.Where(field => folderColumns.ContainsKey(field.Column))
                .ToDictionary(field => field.Column, field => GetString(reader, field.Column));
            folders.Add(new SongFolderSummary(
                GetInt(reader, "FolderNo"),
                GetString(reader, "Name"),
                ParseEnabled(GetString(reader, "Use")),
                songCounts.GetValueOrDefault(GetInt(reader, "FolderNo")),
                GetInt(reader, "GroupStyle"),
                GetInt(reader, "Size0") is > 0 and <= 500 ? new SongPageLayout(
                    GetInt(reader, "Size0"), GetString(reader, "Fontname0"), GetInt(reader, "VPos0"),
                    GetInt(reader, "LMargin"), GetInt(reader, "RMargin"), GetInt(reader, "BMargin"),
                    double.TryParse(GetString(reader, "LineSpacing"), CultureInfo.InvariantCulture, out var spacing) && spacing is >= 0.5 and <= 2 ? spacing : 1,
                    GetInt(reader, "HeadingSize"), GetInt(reader, "BIU0")) { Fields = formatting } : null,
                formatting,
                deletedCounts.GetValueOrDefault(GetInt(reader, "FolderNo"))));
        }

        return folders;
    }

    private static IReadOnlyList<SongSummary> GetSongs(string databasePath, int? folderNo, IReadOnlySet<int>? ids = null, IReadOnlySet<string>? titles = null)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        var columns = ReadColumnMap(connection, "SONG");
        // IDs are bound in bounded batches. Compare legacy titles in .NET to preserve Unicode ordinal matching.
        using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        HashSet<int>? matches = ids is null ? null : new(ids);
        if (matches != null && titles is { Count: > 0 })
        {
            using var lookup = new SQLiteCommand("SELECT SONGID, TITLE_1 FROM SONG WHERE FOLDERNO > 0", connection, transaction);
            using var names = lookup.ExecuteReader();
            while (names.Read())
                if (titles.Contains(GetString(names, "TITLE_1"))) matches.Add(GetInt(names, "SONGID"));
        }
        if (matches is { Count: 0 }) return Array.Empty<SongSummary>();
        var songs = new List<SongSummary>();
        if (matches is null) ReadPage(null);
        else foreach (var batch in matches.Chunk(400)) ReadPage(batch);
        return songs;

        void ReadPage(int[]? batch)
        {
            string where = batch != null
                ? $"WHERE FOLDERNO > 0 AND SONGID IN ({string.Join(",", Enumerable.Range(0, batch.Length).Select(i => $"@id{i}"))})"
                : folderNo.HasValue ? "WHERE FOLDERNO = @folderNo" : "";
            using var command = new SQLiteCommand(
                $"""
                SELECT
                    {SelectColumn(columns, "SONGID", "SONGID")},
                    {SelectColumn(columns, "TITLE_1", "TITLE_1")},
                    {SelectColumn(columns, "TITLE_2", "TITLE_2")},
                    {SelectColumn(columns, "FOLDERNO", "FOLDERNO")},
                    {SelectColumn(columns, "SONG_NUMBER", "SONG_NUMBER")},
                    {SelectColumn(columns, "CATEGORY", "CATEGORY")},
                    {SelectColumn(columns, "KEY", "KEY")},
                    {SelectColumn(columns, "LYRICS", "LYRICS")},
                    {SelectColumn(columns, "COPYRIGHT", "COPYRIGHT")},
                    {SelectColumn(columns, "LICENCE_ADMIN1", "LICENCE_ADMIN1")},
                    {SelectColumn(columns, "LICENCE_ADMIN2", "LICENCE_ADMIN2")},
                    {SelectColumn(columns, "FORMATDATA", "FORMATDATA")},
                    {SelectColumn(columns, "SEQUENCE", "SEQUENCE")},
                    {SelectColumn(columns, "WRITER", "WRITER")},
                    {SelectColumn(columns, "BOOK_REFERENCE", "BOOK_REFERENCE")},
                    {SelectColumn(columns, "USER_REFERENCE", "USER_REFERENCE")},
                    {SelectColumn(columns, "CJK_STROKECOUNT", "CJK_STROKECOUNT")},
                    {SelectColumn(columns, "CJK_WORDCOUNT", "CJK_WORDCOUNT")},
                    {SelectColumn(columns, "CAPO", "CAPO")},
                    {SelectColumn(columns, "MSC", "MSC")}
                FROM SONG
                {where}
                ORDER BY {QuoteIdentifier(columns["FOLDERNO"])}, {QuoteIdentifier(columns["SONG_NUMBER"])}, {QuoteIdentifier(columns["TITLE_1"])}, {QuoteIdentifier(columns["SONGID"])};
                """,
                connection, transaction);
            if (folderNo.HasValue) command.Parameters.AddWithValue("@folderNo", folderNo.Value);
            if (batch != null)
                for (int i = 0; i < batch.Length; i++) command.Parameters.AddWithValue($"@id{i}", batch[i]);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(new SongSummary(
                    GetInt(reader, "SONGID"),
                    GetString(reader, "TITLE_1"),
                    GetString(reader, "TITLE_2"),
                    GetInt(reader, "FOLDERNO"),
                    GetInt(reader, "SONG_NUMBER"),
                    GetString(reader, "CATEGORY"),
                    GetString(reader, "KEY"),
                    GetString(reader, "LYRICS"),
                    GetString(reader, "COPYRIGHT"),
                    GetString(reader, "BOOK_REFERENCE"),
                    GetString(reader, "USER_REFERENCE"),
                    GetString(reader, "WRITER"),
                    GetString(reader, "SEQUENCE"),
                    GetString(reader, "FORMATDATA"),
                    GetString(reader, "LICENCE_ADMIN1"), GetString(reader, "LICENCE_ADMIN2"),
                    columns.ContainsKey("CJK_STROKECOUNT") ? GetString(reader, "CJK_STROKECOUNT") : null,
                    columns.ContainsKey("CJK_WORDCOUNT") ? GetString(reader, "CJK_WORDCOUNT") : null,
                    GetInt(reader, "CAPO"),
                    GetString(reader, "MSC")));
            }
        }
    }

    private static SongDetail? GetSongDetail(string databasePath, int songId)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        var columns = ReadColumnMap(connection, "SONG");
        using var command = new SQLiteCommand(
            $"""
            SELECT
                {SelectColumn(columns, "SONGID", "SONGID")},
                {SelectColumn(columns, "TITLE_1", "TITLE_1")},
                {SelectColumn(columns, "TITLE_2", "TITLE_2")},
                {SelectColumn(columns, "FOLDERNO", "FOLDERNO")},
                {SelectColumn(columns, "SONG_NUMBER", "SONG_NUMBER")},
                {SelectColumn(columns, "LYRICS", "LYRICS")},
                {SelectColumn(columns, "SEQUENCE", "SEQUENCE")},
                {SelectColumn(columns, "WRITER", "WRITER")},
                {SelectColumn(columns, "COPYRIGHT", "COPYRIGHT")},
                {SelectColumn(columns, "CAPO", "CAPO")},
                {SelectColumn(columns, "TIMING", "TIMING")},
                {SelectColumn(columns, "KEY", "KEY")},
                {SelectColumn(columns, "MSC", "MSC")},
                {SelectColumn(columns, "CATEGORY", "CATEGORY")},
                {SelectColumn(columns, "LICENCE_ADMIN1", "LICENCE_ADMIN1")},
                {SelectColumn(columns, "LICENCE_ADMIN2", "LICENCE_ADMIN2")},
                {SelectColumn(columns, "BOOK_REFERENCE", "BOOK_REFERENCE")},
                {SelectColumn(columns, "USER_REFERENCE", "USER_REFERENCE")},
                {SelectColumn(columns, "SETTINGS", "SETTINGS")},
                {SelectColumn(columns, "FORMATDATA", "FORMATDATA")}
            FROM SONG
            WHERE {QuoteIdentifier(columns["SONGID"])} = @songId;
            """,
            connection);
        command.Parameters.AddWithValue("@songId", songId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new SongDetail(
            GetInt(reader, "SONGID"),
            GetString(reader, "TITLE_1"),
            GetString(reader, "TITLE_2"),
            GetInt(reader, "FOLDERNO"),
            GetInt(reader, "SONG_NUMBER"),
            GetString(reader, "LYRICS"),
            GetString(reader, "SEQUENCE"),
            GetString(reader, "WRITER"),
            GetString(reader, "COPYRIGHT"),
            GetInt(reader, "CAPO"),
            GetString(reader, "TIMING"),
            GetString(reader, "KEY"),
            GetString(reader, "MSC"),
            GetString(reader, "CATEGORY"),
            GetString(reader, "LICENCE_ADMIN1"),
            GetString(reader, "LICENCE_ADMIN2"),
            GetString(reader, "BOOK_REFERENCE"),
            GetString(reader, "USER_REFERENCE"),
            GetString(reader, "SETTINGS"),
            GetString(reader, "FORMATDATA"));
    }

    private static List<DeletedSongSummary> GetDeletedSongs(string databasePath)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        var columns = ReadColumnMap(connection, "SONG");
        var oldFolderExpression = columns.ContainsKey("OldFolder") ? "s.OldFolder" : "1";
        var lastModifiedExpression = columns.ContainsKey("LastModified") ? "s.LastModified" : "NULL";
        using var command = new SQLiteCommand(
            $"""
            SELECT
                s.SONGID,
                s.TITLE_1,
                CASE WHEN {oldFolderExpression} < 1 THEN 1 ELSE {oldFolderExpression} END AS OriginalFolderNo,
                COALESCE(f.Name, '') AS OriginalFolderName,
                {lastModifiedExpression} AS LastModified
            FROM SONG s
            LEFT JOIN FOLDER f ON f.FolderNo = CASE WHEN {oldFolderExpression} < 1 THEN 1 ELSE {oldFolderExpression} END
            WHERE s.FOLDERNO = 0
            ORDER BY LastModified, s.TITLE_1, s.SONGID;
            """,
            connection);
        using var reader = command.ExecuteReader();
        var songs = new List<DeletedSongSummary>();
        while (reader.Read())
        {
            var originalFolderNo = GetInt(reader, "OriginalFolderNo");
            songs.Add(new DeletedSongSummary(
                GetInt(reader, "SONGID"),
                GetString(reader, "TITLE_1"),
                originalFolderNo,
                string.IsNullOrWhiteSpace(GetString(reader, "OriginalFolderName"))
                    ? $"Folder {originalFolderNo}"
                    : GetString(reader, "OriginalFolderName"),
                GetDate(reader, "LastModified")));
        }

        return songs;
    }

    private static AdminDatabaseWriteReport ExecuteWrite(
        string databasePath,
        string backupRoot,
        AdminDatabaseWriteOperation operation,
        Action<SQLiteConnection, SQLiteTransaction, AdminDatabaseWriteOutcome> write,
        bool backup = true)
    {
        var inventory = AnalyzeSchema(databasePath);
        if (!inventory.Succeeded)
        {
            return FailedWriteReport(
                operation,
                inventory.DatabasePath,
                backupPath: null,
                new AdminDatabaseWriteOutcome(),
                ToWriteIssues(inventory.Issues));
        }

        if (backup && string.IsNullOrWhiteSpace(backupRoot))
        {
            return FailedWriteReport(
                operation,
                inventory.DatabasePath,
                backupPath: null,
                new AdminDatabaseWriteOutcome(),
                [WriteIssue(AdminDatabaseWriteIssueKind.InvalidRequest, UiText.Get("AdminDatabaseRepository", "BackupRootEmpty"))]);
        }

        var outcome = new AdminDatabaseWriteOutcome();
        string? backupPath = null;
        try
        {
            if (backup) backupPath = CreateBackup(inventory.DatabasePath, backupRoot);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return FailedWriteReport(
                operation,
                inventory.DatabasePath,
                backupPath: null,
                outcome,
                [WriteIssue(AdminDatabaseWriteIssueKind.BackupFailed, UiText.Format("AdminDatabaseRepository", "BackupFailed", ex.Message))]);
        }

        try
        {
            using var connection = OpenConnection(inventory.DatabasePath, readOnly: false);
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                write(connection, transaction, outcome);
                transaction.Commit();
            }
            catch
            {
                TryRollback(transaction);
                throw;
            }
        }
        // A failed write needs no file restore: the transaction is atomic, so the database is as it was.
        // Copying the backup over it would erase whatever the WinForms app committed since the backup was taken.
        catch (AdminDatabaseWriteException ex)
        {
            return FailedWriteReport(operation, inventory.DatabasePath, backupPath, outcome, [WriteIssue(ex.Kind, ex.Message)]);
        }
        catch (Exception ex) when (IsSqliteOpenException(ex))
        {
            return FailedWriteReport(
                operation,
                inventory.DatabasePath,
                backupPath,
                outcome,
                [WriteIssue(AdminDatabaseWriteIssueKind.WriteFailed, UiText.Format("AdminDatabaseRepository", "WriteFailed", ex.Message))]);
        }

        return new AdminDatabaseWriteReport(
            Succeeded: true,
            operation,
            inventory.DatabasePath,
            backupPath,
            outcome.SongIds,
            outcome.FolderNos,
            Array.Empty<AdminDatabaseWriteIssue>());
    }

    // The WinForms app has folders 1 to 40 and reads no others, so a row outside them is a folder it never shows.
    internal const int LastFolderNo = 40;

    private static void SaveFolder(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        SongFolderWriteModel folder,
        AdminDatabaseWriteOutcome outcome)
    {
        var folderColumns = ReadColumnMap(connection, "FOLDER");
        using var existsCommand = new SQLiteCommand(
            $"SELECT COUNT(*) FROM FOLDER WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @folderNo;",
            connection,
            transaction);
        existsCommand.Parameters.AddWithValue("@folderNo", folder.FolderNo);
        var exists = Convert.ToInt32(existsCommand.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
        if (!exists && folder.FolderNo is < 1 or > LastFolderNo)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Format("AdminDatabaseRepository", "NewFolderNumberRange", LastFolderNo));
        }

        var hasGroupStyle = folderColumns.TryGetValue("GroupStyle", out var groupStyleColumn);
        // An existing folder saved without a sort order keeps its stored GroupStyle:
        // the column is left out of the UPDATE. A new folder always gets a value (0 by default).
        var writeGroupStyle = hasGroupStyle && (!exists || folder.GroupStyle.HasValue);
        var commandText = exists
            ? BuildFolderUpdateSql(folderColumns, writeGroupStyle, groupStyleColumn)
            : BuildFolderInsertSql(folderColumns, writeGroupStyle, groupStyleColumn);
        using var command = new SQLiteCommand(
            commandText,
            connection,
            transaction);
        command.Parameters.AddWithValue("@folderNo", folder.FolderNo);
        command.Parameters.AddWithValue("@name", Normalize(folder.Name));
        command.Parameters.AddWithValue("@use", folder.IsEnabled ? "True" : "False");
        if (writeGroupStyle)
        {
            command.Parameters.AddWithValue("@groupStyle", NormalizeLegacyFolderGroupStyle(folder.GroupStyle ?? 0));
        }

        command.ExecuteNonQuery();
        if (!exists || folder.ResetFormat)
        {
            ResetFolderFormat(connection, transaction, folderColumns, folder);
        }

        if (folder.FormatChanges is { Count: > 0 } edits)
        {
            using var format = connection.CreateCommand();
            format.Transaction = transaction;
            var assignments = new List<string>();
            foreach (var (column, value) in edits)
            {
                var field = Array.Find(FolderFormatField.All, f => f.Column == column);
                if (field is null || !folderColumns.ContainsKey(column) || !field.IsValid(value))
                    throw new InvalidOperationException(UiText.Format("AdminDatabaseRepository", "FolderFormatNotSaved", column));
                string parameter = $"@format{assignments.Count}";
                assignments.Add($"{QuoteIdentifier(field.Column)} = {parameter}");
                format.Parameters.AddWithValue(parameter, value);
            }
            format.CommandText = $"UPDATE FOLDER SET {string.Join(", ", assignments)} WHERE FolderNo = @folder";
            format.Parameters.AddWithValue("@folder", folder.FolderNo);
            format.ExecuteNonQuery();
        }
        outcome.FolderNos.Add(folder.FolderNo);
    }

    private static string BuildFolderUpdateSql(
        Dictionary<string, string> folderColumns,
        bool hasGroupStyle,
        string? groupStyleColumn)
    {
        var assignments = new List<string>
        {
            $"{QuoteIdentifier(folderColumns["Name"])} = @name",
            $"{QuoteIdentifier(folderColumns["Use"])} = @use",
        };
        if (hasGroupStyle && groupStyleColumn is not null)
        {
            assignments.Add($"{QuoteIdentifier(groupStyleColumn)} = @groupStyle");
        }

        return $"UPDATE FOLDER SET {string.Join(", ", assignments)} WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @folderNo;";
    }

    private static string BuildFolderInsertSql(
        Dictionary<string, string> folderColumns,
        bool hasGroupStyle,
        string? groupStyleColumn)
    {
        var columns = new List<string>
        {
            QuoteIdentifier(folderColumns["FolderNo"]),
            QuoteIdentifier(folderColumns["Name"]),
            QuoteIdentifier(folderColumns["Use"]),
        };
        var values = new List<string> { "@folderNo", "@name", "@use" };
        if (hasGroupStyle && groupStyleColumn is not null)
        {
            columns.Add(QuoteIdentifier(groupStyleColumn));
            values.Add("@groupStyle");
        }

        return $"INSERT INTO FOLDER ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)});";
    }

    // What Gf.ResetFolder of the WinForms app gives a new folder. The other columns of the row are emptied, as in a
    // row WinForms creates; a slot used before would otherwise pass its formatting on to the new folder.
    private static readonly (string Column, object Value)[] NewFolderFormat =
    [
        ("PreChorusHeading", ""), ("ChorusHeading", "Chorus:"), ("BridgeHeading", ""), ("EndingHeading", ""),
        ("BIUHeading", 4), ("HeadingSize", 100), ("HeadingOption", (int)Axicultus.Module.HeadingFormat.AsRegion1Plus),
        ("BIU0", 0), ("Size0", 40), ("Bold0", 0), ("Align0", 2), ("FontName0", "Microsoft Sans Serif"), ("Vpos0", 0),
        ("BIU1", 0), ("Size1", 40), ("Bold1", 0), ("Align1", 2), ("FontName1", "Microsoft Sans Serif"), ("Vpos1", 50),
    ];

    private static void ResetFolderFormat(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        Dictionary<string, string> folderColumns,
        SongFolderWriteModel folder)
    {
        using var command = new SQLiteCommand(connection) { Transaction = transaction };
        var assignments = new List<string>();
        foreach (var (name, column) in folderColumns)
        {
            if (name.Equals("FolderNo", StringComparison.OrdinalIgnoreCase) || name.Equals("Name", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Use", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            object value = name.Equals("GroupStyle", StringComparison.OrdinalIgnoreCase)
                ? NormalizeLegacyFolderGroupStyle(folder.GroupStyle ?? 0)
                : Array.Find(NewFolderFormat, item => item.Column.Equals(name, StringComparison.OrdinalIgnoreCase)).Value ?? DBNull.Value;
            command.Parameters.AddWithValue($"@value{assignments.Count}", value);
            assignments.Add($"{QuoteIdentifier(column)} = @value{assignments.Count}");
        }

        if (assignments.Count == 0)
        {
            return;
        }

        command.CommandText = $"UPDATE FOLDER SET {string.Join(", ", assignments)} WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @folderNo;";
        command.Parameters.AddWithValue("@folderNo", folder.FolderNo);
        command.ExecuteNonQuery();
    }

    // 0 = title, 1 = word count, 2 = song number (WinForms SortBy); anything else is title.
    private static int NormalizeLegacyFolderGroupStyle(int groupStyle)
        => groupStyle is 0 or 1 or 2 ? groupStyle : 0;

    private static void SoftDeleteFolders(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<FolderDeleteRequest> deletes,
        AdminDatabaseWriteOutcome outcome)
    {
        if (deletes.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderDeleteRequired"));
        }

        SetFolderUse(
            connection,
            transaction,
            [.. deletes.Select(delete => delete.FolderNo)],
            isEnabled: false,
            outcome);
    }

    private static void RecoverFolders(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<FolderRecoveryRequest> recoveries,
        AdminDatabaseWriteOutcome outcome)
    {
        if (recoveries.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderRecoveryRequired"));
        }

        SetFolderUse(
            connection,
            transaction,
            [.. recoveries.Select(recovery => recovery.FolderNo)],
            isEnabled: true,
            outcome);
    }

    private static void SetFolderUse(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        int[] folderNos,
        bool isEnabled,
        AdminDatabaseWriteOutcome outcome)
    {
        if (folderNos.Any(folderNo => folderNo <= 0))
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderNumbersNotPositive"));
        }

        if (folderNos.Distinct().Count() != folderNos.Length)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderNumbersNotDistinct"));
        }

        var columns = ReadColumnMap(connection, "FOLDER");
        foreach (var folderNo in folderNos)
        {
            using var command = new SQLiteCommand(
                $"""
                UPDATE FOLDER
                SET {QuoteIdentifier(columns["Use"])} = @use
                WHERE {QuoteIdentifier(columns["FolderNo"])} = @folderNo;
                """,
                connection,
                transaction);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            command.Parameters.AddWithValue("@use", isEnabled ? "True" : "False");
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "FolderNotFound", folderNo));
            }

            outcome.FolderNos.Add(folderNo);
        }
    }

    private static void SaveSong(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        SongWriteModel song,
        AdminDatabaseWriteOutcome outcome)
    {
        var columns = ReadColumnMap(connection, "SONG");
        outcome.StrokeKeys ??= ReadStrokeKeys(connection, transaction, columns);
        var values = BuildSongValues(song, outcome.StrokeKeys);
        var writable = values
            .Where(value => columns.ContainsKey(value.Key))
            .ToArray();

        if (song.SongId is > 0)
        {
            var updates = writable
                .Where(value =>
                    !string.Equals(value.Key, "SONGID", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value.Key, "OldFolder", StringComparison.OrdinalIgnoreCase))
                .Select((value, index) => (value.Key, value.Value, ParameterName: $"@p{index}"))
                .ToArray();
            var assignments = string.Join(
                ", ",
                updates.Select(value => $"{QuoteIdentifier(columns[value.Key])} = {value.ParameterName}"));
            using var command = new SQLiteCommand(
                $"UPDATE SONG SET {assignments} WHERE {QuoteIdentifier(columns["SONGID"])} = @songId;",
                connection,
                transaction);
            foreach (var (Key, Value, ParameterName) in updates)
            {
                command.Parameters.AddWithValue(ParameterName, Value ?? DBNull.Value);
            }

            command.Parameters.AddWithValue("@songId", song.SongId.Value);
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "SongIdNotFound", song.SongId.Value));
            }

            outcome.SongIds.Add(song.SongId.Value);
            return;
        }

        var inserts = writable
            .Where(value => !string.Equals(value.Key, "SONGID", StringComparison.OrdinalIgnoreCase))
            .Select((value, index) => (value.Key, value.Value, ParameterName: $"@p{index}"))
            .ToArray();
        var insertColumns = string.Join(", ", inserts.Select(value => QuoteIdentifier(columns[value.Key])));
        var insertValues = string.Join(", ", inserts.Select(value => value.ParameterName));
        using var insertCommand = new SQLiteCommand(
            $"INSERT INTO SONG ({insertColumns}) VALUES ({insertValues}); SELECT last_insert_rowid();",
            connection,
            transaction);
        foreach (var (Key, Value, ParameterName) in inserts)
        {
            insertCommand.Parameters.AddWithValue(ParameterName, Value ?? DBNull.Value);
        }

        var songId = Convert.ToInt32(insertCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
        outcome.SongIds.Add(songId);
    }

    private static void MoveSongs(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<SongMoveRequest> moves,
        AdminDatabaseWriteOutcome outcome)
    {
        if (moves.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "SongMoveRequired"));
        }

        var columns = ReadColumnMap(connection, "SONG");
        foreach (var move in moves)
        {
            var assignments = new List<string>
            {
                $"{QuoteIdentifier(columns["FOLDERNO"])} = @newFolderNo",
            };
            if (columns.TryGetValue("OldFolder", out var oldFolderColumn))
            {
                assignments.Add($"{QuoteIdentifier(oldFolderColumn)} = @oldFolderNo");
            }

            if (move.UpdateModifiedDate && columns.TryGetValue("LastModified", out var lastModifiedColumn))
            {
                assignments.Add($"{QuoteIdentifier(lastModifiedColumn)} = @lastModified");
            }

            using var command = new SQLiteCommand(
                $"UPDATE SONG SET {string.Join(", ", assignments)} WHERE {QuoteIdentifier(columns["SONGID"])} = @songId;",
                connection,
                transaction);
            command.Parameters.AddWithValue("@songId", move.SongId);
            command.Parameters.AddWithValue("@oldFolderNo", move.OldFolderNo);
            command.Parameters.AddWithValue("@newFolderNo", move.NewFolderNo);
            if (move.UpdateModifiedDate)
            {
                command.Parameters.AddWithValue("@lastModified", Axicultus.Util.StoredDate.ToText(DateTime.Now));
            }

            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "SongIdNotFound", move.SongId));
            }

            outcome.SongIds.Add(move.SongId);
        }
    }

    private static void SoftDeleteSongs(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<SongDeleteRequest> deletes,
        AdminDatabaseWriteOutcome outcome)
    {
        if (deletes.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "SongDeleteRequired"));
        }

        MoveSongs(
            connection,
            transaction,
            deletes.Select(delete => new SongMoveRequest(
                delete.SongId,
                delete.OriginalFolderNo,
                NewFolderNo: 0,
                UpdateModifiedDate: true)).ToArray(),
            outcome);
    }

    private static void RecoverSongs(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<SongRecoveryRequest> recoveries,
        AdminDatabaseWriteOutcome outcome)
    {
        if (recoveries.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "SongRecoveryRequired"));
        }

        var columns = ReadColumnMap(connection, "SONG");
        foreach (var recovery in recoveries)
        {
            var assignments = new List<string>
            {
                $"{QuoteIdentifier(columns["FOLDERNO"])} = @targetFolderNo",
            };
            if (columns.TryGetValue("OldFolder", out var oldFolderColumn))
            {
                assignments.Add($"{QuoteIdentifier(oldFolderColumn)} = 0");
            }

            if (columns.TryGetValue("LastModified", out var lastModifiedColumn))
            {
                assignments.Add($"{QuoteIdentifier(lastModifiedColumn)} = @lastModified");
            }

            // Only a song that is still deleted, as in WinForms (Gf.ReFileSelectedSongs): the list the operator chose
            // from may be old, and the other app may have recovered the song and filed it elsewhere since.
            using var command = new SQLiteCommand(
                $"UPDATE SONG SET {string.Join(", ", assignments)} WHERE {QuoteIdentifier(columns["SONGID"])} = @songId AND {QuoteIdentifier(columns["FOLDERNO"])} = 0;",
                connection,
                transaction);
            command.Parameters.AddWithValue("@songId", recovery.SongId);
            command.Parameters.AddWithValue("@targetFolderNo", recovery.TargetFolderNo);
            command.Parameters.AddWithValue("@lastModified", Axicultus.Util.StoredDate.ToText(DateTime.Now));
            if (command.ExecuteNonQuery() == 0)
            {
                continue; // Not deleted any more, or gone: left as it is. The caller sees it is not in AffectedSongIds.
            }

            outcome.SongIds.Add(recovery.SongId);
            outcome.FolderNos.Add(recovery.TargetFolderNo);
        }
    }

    private static void ReorderFolders(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<FolderOrderRequest> order,
        AdminDatabaseWriteOutcome outcome)
    {
        if (order.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderReorderRequired"));
        }

        if (order.Any(item => item.FolderNo <= 0 || item.NewFolderNo <= 0))
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderNumbersNotPositive"));
        }

        if (order.Select(item => item.FolderNo).Distinct().Count() != order.Count ||
            order.Select(item => item.NewFolderNo).Distinct().Count() != order.Count)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderReorderNotDistinct"));
        }

        var folderColumns = ReadColumnMap(connection, "FOLDER");
        var songColumns = ReadColumnMap(connection, "SONG");
        var sourceNumbers = order.Select(item => item.FolderNo).ToHashSet();
        foreach (var item in order)
        {
            using var existsCommand = new SQLiteCommand(
                $"SELECT COUNT(*) FROM FOLDER WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @folderNo;",
                connection,
                transaction);
            existsCommand.Parameters.AddWithValue("@folderNo", item.FolderNo);
            var exists = Convert.ToInt32(existsCommand.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
            if (!exists)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "FolderNotFound", item.FolderNo));
            }

            if (!sourceNumbers.Contains(item.NewFolderNo))
            {
                using var targetCommand = new SQLiteCommand(
                    $"SELECT COUNT(*) FROM FOLDER WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @folderNo;",
                    connection,
                    transaction);
                targetCommand.Parameters.AddWithValue("@folderNo", item.NewFolderNo);
                var targetExists = Convert.ToInt32(targetCommand.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
                if (targetExists)
                {
                    throw new AdminDatabaseWriteException(
                        AdminDatabaseWriteIssueKind.InvalidRequest,
                        UiText.Format("AdminDatabaseRepository", "TargetFolderInUse", item.NewFolderNo));
                }
            }
        }

        foreach (var item in order)
        {
            using var songCommand = new SQLiteCommand(
                $"UPDATE SONG SET {QuoteIdentifier(songColumns["FOLDERNO"])} = @stagedFolderNo WHERE {QuoteIdentifier(songColumns["FOLDERNO"])} = @folderNo;",
                connection,
                transaction);
            songCommand.Parameters.AddWithValue("@folderNo", item.FolderNo);
            songCommand.Parameters.AddWithValue("@stagedFolderNo", -item.FolderNo);
            songCommand.ExecuteNonQuery();
        }

        foreach (var item in order)
        {
            using var songCommand = new SQLiteCommand(
                $"UPDATE SONG SET {QuoteIdentifier(songColumns["FOLDERNO"])} = @newFolderNo WHERE {QuoteIdentifier(songColumns["FOLDERNO"])} = @stagedFolderNo;",
                connection,
                transaction);
            songCommand.Parameters.AddWithValue("@stagedFolderNo", -item.FolderNo);
            songCommand.Parameters.AddWithValue("@newFolderNo", item.NewFolderNo);
            songCommand.ExecuteNonQuery();
        }

        // A deleted song is recovered into its OldFolder, so that number follows the folder too.
        if (songColumns.TryGetValue("OldFolder", out var oldFolderColumn))
        {
            foreach (bool stage in new[] { true, false })
            {
                foreach (var item in order)
                {
                    using var oldFolderCommand = new SQLiteCommand(
                        $"UPDATE SONG SET {QuoteIdentifier(oldFolderColumn)} = @to WHERE {QuoteIdentifier(oldFolderColumn)} = @from;",
                        connection,
                        transaction);
                    oldFolderCommand.Parameters.AddWithValue("@from", stage ? item.FolderNo : -item.FolderNo);
                    oldFolderCommand.Parameters.AddWithValue("@to", stage ? -item.FolderNo : item.NewFolderNo);
                    oldFolderCommand.ExecuteNonQuery();
                }
            }
        }

        foreach (var item in order)
        {
            using var stageCommand = new SQLiteCommand(
                $"UPDATE FOLDER SET {QuoteIdentifier(folderColumns["FolderNo"])} = @stagedFolderNo WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @folderNo;",
                connection,
                transaction);
            stageCommand.Parameters.AddWithValue("@folderNo", item.FolderNo);
            stageCommand.Parameters.AddWithValue("@stagedFolderNo", -item.FolderNo);
            var affected = stageCommand.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "FolderNotFound", item.FolderNo));
            }
        }

        foreach (var item in order)
        {
            using var applyCommand = new SQLiteCommand(
                $"UPDATE FOLDER SET {QuoteIdentifier(folderColumns["FolderNo"])} = @newFolderNo WHERE {QuoteIdentifier(folderColumns["FolderNo"])} = @stagedFolderNo;",
                connection,
                transaction);
            applyCommand.Parameters.AddWithValue("@newFolderNo", item.NewFolderNo);
            applyCommand.Parameters.AddWithValue("@stagedFolderNo", -item.FolderNo);
            var affected = applyCommand.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "FolderNotFound", item.FolderNo));
            }

            outcome.FolderNos.Add(item.NewFolderNo);
        }
    }

    private static void ReorderSongs(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        int folderNo,
        IReadOnlyList<SongOrderRequest> order,
        AdminDatabaseWriteOutcome outcome)
    {
        if (folderNo <= 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "FolderNumberNotPositive"));
        }

        if (order.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "SongReorderRequired"));
        }

        if (order.Any(item => item.SongId <= 0 || item.SongNumber <= 0))
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "SongNumbersNotPositive"));
        }

        if (order.Select(item => item.SongId).Distinct().Count() != order.Count ||
            order.Select(item => item.SongNumber).Distinct().Count() != order.Count)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                UiText.Get("AdminDatabaseRepository", "SongReorderNotDistinct"));
        }

        var columns = ReadColumnMap(connection, "SONG");
        foreach (var item in order)
        {
            using var command = new SQLiteCommand(
                $"""
                UPDATE SONG
                SET {QuoteIdentifier(columns["SONG_NUMBER"])} = @songNumber
                WHERE {QuoteIdentifier(columns["SONGID"])} = @songId
                  AND {QuoteIdentifier(columns["FOLDERNO"])} = @folderNo;
                """,
                connection,
                transaction);
            command.Parameters.AddWithValue("@songId", item.SongId);
            command.Parameters.AddWithValue("@folderNo", folderNo);
            command.Parameters.AddWithValue("@songNumber", item.SongNumber);
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    UiText.Format("AdminDatabaseRepository", "SongNotFoundInFolder", folderNo, item.SongId));
            }

            outcome.SongIds.Add(item.SongId);
        }

        outcome.FolderNos.Add(folderNo);
    }

    private static Dictionary<string, object?> BuildSongValues(SongWriteModel song, StrokeKeys strokeKeys)
    {
        var title = Limit(song.Title, 100);
        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["TITLE_1"] = title,
            ["TITLE_2"] = Limit(song.AlternateTitle, 100),
            ["SONG_NUMBER"] = song.SongNumber,
            ["FOLDERNO"] = song.FolderNo,
            ["LYRICS"] = Normalize(song.Lyrics),
            ["SEQUENCE"] = Limit(song.SequenceIsStored
                ? Normalize(song.Sequence)
                : Axicultus.Module.SongExchangeSequence.ToDatabase(Normalize(song.Sequence)), 100),
            ["WRITER"] = Limit(song.Writer, 100),
            ["COPYRIGHT"] = Limit(song.Copyright, 100),
            ["CJK_WordCount"] = ComputeLegacyCjkWordCount(title),
            ["CJK_StrokeCount"] = ComputeLegacyCjkStrokeCount(title, strokeKeys),
            ["CAPO"] = song.Capo,
            ["TIMING"] = Limit(song.Timing, 50),
            ["KEY"] = Limit(song.Key, 20),
            ["MSC"] = Normalize(song.Notations),
            ["CATEGORY"] = Normalize(song.Category),
            ["LICENCE_ADMIN1"] = Limit(song.LicenceAdmin1, 50),
            ["LICENCE_ADMIN2"] = Limit(song.LicenceAdmin2, 50),
            ["BOOK_REFERENCE"] = Limit(song.BookReference, 50),
            ["USER_REFERENCE"] = Normalize(song.UserReference),
            ["SETTINGS"] = Normalize(song.Settings),
            ["FORMATDATA"] = Normalize(song.FormatData),
            ["LastModified"] = Axicultus.Util.StoredDate.ToText(DateTime.Now), // yyyy-MM-dd, as WinForms writes it
            ["OldFolder"] = 0,
        };
    }

    private static string CreateBackup(string databasePath, string backupRoot)
    {
        var root = Path.GetFullPath(backupRoot);
        Directory.CreateDirectory(root);
        var fileName = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        var backupPath = Path.Combine(root, $"{fileName}.{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}{extension}");
        File.Copy(databasePath, backupPath, overwrite: false);
        CopySidecarIfExists(databasePath, backupPath, "-wal");
        CopySidecarIfExists(databasePath, backupPath, "-shm");
        PruneBackups(root, fileName, extension, backupPath);
        return backupPath;
    }

    // Every backed-up write adds a full copy of the database. Kept are the newest BackupsKept, the first copy of
    // each of the last BackupDaysKept days (so many writes in a row cannot push out the copy from before a bad
    // change made days earlier), the first copy of every month however old (what a church kept for years is
    // thinned, not emptied), and always the copy just made.
    internal const int BackupsKept = 20;
    internal const int BackupDaysKept = 30;

    private static void PruneBackups(string root, string fileName, string extension, string justMade)
    {
        try
        {
            // The names sort by their UTC time stamp. Other files in the folder (schema backups, other databases) do not match.
            var name = new System.Text.RegularExpressions.Regex(
                "^" + System.Text.RegularExpressions.Regex.Escape(fileName) + @"\.\d{8}-\d{6}-\d{3}" +
                System.Text.RegularExpressions.Regex.Escape(extension) + "$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var backups = Directory.EnumerateFiles(root, fileName + ".*" + extension)
                .Where(path => name.IsMatch(Path.GetFileName(path)))
                .OrderByDescending(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ToArray();
            // The clock may have been set back: the copy just made is then not among the newest names.
            var keep = new HashSet<string>(backups.Take(BackupsKept), StringComparer.OrdinalIgnoreCase) { justMade };
            string Day(string path) => Path.GetFileName(path).Substring(fileName.Length + 1, 8);
            string firstDayKept = DateTime.UtcNow.AddDays(-BackupDaysKept).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            foreach (var day in backups.GroupBy(Day).Where(day => string.CompareOrdinal(day.Key, firstDayKept) >= 0))
            {
                keep.Add(day.Last()); // newest first, so the last is the first copy of that day
            }

            foreach (var month in backups.GroupBy(path => Day(path)[..6]))
            {
                keep.Add(month.Last());
            }

            foreach (var old in backups.Where(path => !keep.Contains(path)))
            {
                File.Delete(old);
                File.Delete(old + "-wal");
                File.Delete(old + "-shm");
            }
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            // A backup that cannot be removed now is removed after a later write.
        }
    }


    private static void CopySidecarIfExists(string databasePath, string backupPath, string suffix)
    {
        var source = databasePath + suffix;
        if (File.Exists(source))
        {
            File.Copy(source, backupPath + suffix, overwrite: false);
        }
    }

    private static Dictionary<string, string> ReadColumnMap(SQLiteConnection connection, string tableName)
    {
        using var command = new SQLiteCommand($"PRAGMA table_info({QuoteIdentifier(tableName)});", connection);
        using var reader = command.ExecuteReader();
        var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
        {
            var columnName = GetString(reader, "name");
            columns[columnName] = columnName;
        }

        return columns;
    }


    private static AdminDatabaseWriteIssue[] ToWriteIssues(IReadOnlyList<AdminDatabaseIssue> issues)
        => [.. issues.Select(issue => WriteIssue(MapWriteIssueKind(issue.Kind), issue.Message, issue.Severity))];

    private static AdminDatabaseWriteIssueKind MapWriteIssueKind(AdminDatabaseIssueKind kind)
        => kind switch
        {
            AdminDatabaseIssueKind.SourceMissing => AdminDatabaseWriteIssueKind.SourceMissing,
            AdminDatabaseIssueKind.SourceNotFile => AdminDatabaseWriteIssueKind.SourceNotFile,
            AdminDatabaseIssueKind.OpenFailed => AdminDatabaseWriteIssueKind.OpenFailed,
            _ => AdminDatabaseWriteIssueKind.SchemaIncompatible,
        };

    private static AdminDatabaseWriteReport FailedWriteReport(
        AdminDatabaseWriteOperation operation,
        string databasePath,
        string? backupPath,
        AdminDatabaseWriteOutcome outcome,
        IReadOnlyList<AdminDatabaseWriteIssue> issues)
        => new(
            Succeeded: false,
            operation,
            databasePath,
            backupPath,
            outcome.SongIds,
            outcome.FolderNos,
            issues);

    private static void EnsureCompatible(string databasePath)
    {
        var inventory = AnalyzeSchema(databasePath);
        if (inventory.Succeeded)
        {
            return;
        }

        var detail = string.Join("; ", inventory.Issues.Select(issue => issue.Message));
        throw new InvalidOperationException(UiText.Format("AdminDatabaseRepository", "SchemaIncompatible", detail));
    }

    private static (string Path, AdminDatabaseIssue? Issue) NormalizeExistingDatabasePath(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return (databasePath, Issue(AdminDatabaseIssueKind.SourceMissing, UiText.Get("AdminDatabaseRepository", "PathEmpty")));
        }

        var fullPath = Path.GetFullPath(databasePath);
        if (Directory.Exists(fullPath))
        {
            return (fullPath, Issue(AdminDatabaseIssueKind.SourceNotFile, UiText.Format("AdminDatabaseRepository", "PathIsDirectory", fullPath)));
        }

        if (!File.Exists(fullPath))
        {
            return (fullPath, Issue(AdminDatabaseIssueKind.SourceMissing, UiText.Format("AdminDatabaseRepository", "FileMissing", fullPath)));
        }

        return (fullPath, null);
    }

    private static List<DatabaseTable> ReadTables(SQLiteConnection connection)
    {
        using var command = new SQLiteCommand(
            "SELECT name, sql FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;",
            connection);
        using var reader = command.ExecuteReader();
        var tables = new List<DatabaseTable>();
        while (reader.Read())
        {
            tables.Add(new DatabaseTable(
                Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["sql"], CultureInfo.InvariantCulture) ?? ""));
        }

        return tables;
    }

    private static Dictionary<string, IReadOnlyList<DatabaseColumn>> ReadColumns(
        SQLiteConnection connection,
        IEnumerable<DatabaseTable> tables)
    {
        var columnsByTable = new Dictionary<string, IReadOnlyList<DatabaseColumn>>(StringComparer.OrdinalIgnoreCase);
        foreach (var table in tables)
        {
            using var command = new SQLiteCommand($"PRAGMA table_info({QuoteIdentifier(table.Name)});", connection);
            using var reader = command.ExecuteReader();
            var columns = new List<DatabaseColumn>();
            while (reader.Read())
            {
                columns.Add(new DatabaseColumn(
                    table.Name,
                    GetString(reader, "name"),
                    GetString(reader, "type"),
                    GetInt(reader, "notnull") != 0,
                    GetInt(reader, "pk") != 0,
                    GetInt(reader, "cid")));
            }

            columnsByTable[table.Name] = columns;
        }

        return columnsByTable;
    }

    private static List<AdminDatabaseIssue> ValidateCompatibility(
        Dictionary<string, IReadOnlyList<DatabaseColumn>> columns)
    {
        var issues = new List<AdminDatabaseIssue>();
        foreach (var (tableName, requiredColumns) in RequiredSchema)
        {
            if (!columns.TryGetValue(tableName, out var tableColumns))
            {
                issues.Add(Issue(
                    AdminDatabaseIssueKind.MissingTable,
                    UiText.Format("AdminDatabaseRepository", "TableMissing", tableName),
                    tableName));
                continue;
            }

            var names = tableColumns.Select(column => column.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var columnName in requiredColumns.Where(column => !names.Contains(column)))
            {
                issues.Add(Issue(
                    AdminDatabaseIssueKind.MissingColumn,
                    UiText.Format("AdminDatabaseRepository", "ColumnMissing", tableName, columnName),
                    tableName,
                    columnName));
            }
        }

        return issues;
    }

    private static SQLiteConnection OpenConnection(string path, bool readOnly)
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = path,
            Version = 3,
            ReadOnly = readOnly,
            ForeignKeys = true,
        };
        var connection = new SQLiteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static int ReadUserVersion(SQLiteConnection connection)
    {
        using var command = new SQLiteCommand("PRAGMA user_version;", connection);
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static string QuoteIdentifier(string identifier)
        => "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string SelectColumn(
        Dictionary<string, string> columns,
        string columnName,
        string alias,
        string? qualifier = null)
        => columns.TryGetValue(columnName, out var actualName)
            ? $"{(qualifier is null ? "" : QuoteIdentifier(qualifier) + ".")}{QuoteIdentifier(actualName)} AS {QuoteIdentifier(alias)}"
            : $"NULL AS {QuoteIdentifier(alias)}";

    private static string GetString(SQLiteDataReader reader, string name)
    {
        var value = reader[name];
        return value is DBNull ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static int GetInt(SQLiteDataReader reader, string name)
    {
        var value = reader[name];
        if (value is DBNull)
        {
            return 0;
        }

        if (value is int number)
        {
            return number;
        }

        return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    private static DateTime? GetDate(SQLiteDataReader reader, string name)
    {
        var value = reader[name];
        if (value is DBNull)
        {
            return null;
        }

        // Shared with the WinForms app: yyyy-MM-dd, or what older versions wrote in the Korean or English format.
        return Axicultus.Util.StoredDate.TryParse(value, out var parsed)
            ? parsed.Date
            : null;
    }

    private static bool ParseEnabled(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "no", StringComparison.OrdinalIgnoreCase);
    }

    private static string Limit(string? value, int maxLength)
    {
        var normalized = Normalize(value);
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string Normalize(string? value)
        => value ?? string.Empty;

    private static string ComputeLegacyCjkWordCount(string title)
    {
        if (string.IsNullOrEmpty(title) || title[0] is > '\0' and < (char)128)
        {
            return "000";
        }

        var length = title.Length;
        var parenthesisIndex = title.IndexOf('(', StringComparison.Ordinal);
        if (parenthesisIndex > 0)
        {
            length = parenthesisIndex - 1;
        }

        // WinForms (DataUtil.CJK_WordCount) takes the space only when more than one character precedes it.
        var spaceIndex = title.IndexOf(' ', StringComparison.Ordinal);
        if (spaceIndex > 1 && spaceIndex - 1 < length)
        {
            length = spaceIndex - 1;
        }

        return Math.Max(length, 0).ToString("000", CultureInfo.InvariantCulture);
    }

    // DataUtil.CJK_StrokeCount of the WinForms app, which sorts its song list by this key: for each of the first six
    // characters two digits of stroke count and five of character code, then the rest of the title; "000" and the
    // title when the first character has no stroke count.
    private static string ComputeLegacyCjkStrokeCount(string title, StrokeKeys strokeKeys)
    {
        // A title some song already has with a stroke key keeps that key (an unchanged title, a copy, a merge).
        // It is what WinForms computed for the title; and where the database conversion turned a character
        // into "?", the stored key still holds the song's place in the list, which the title no longer gives.
        if (strokeKeys.ByTitle.TryGetValue(title, out var stored))
        {
            return stored;
        }

        var strokeCounts = strokeKeys.Counts;
        if (title.Length == 0 || !strokeCounts.ContainsKey(title[0]))
        {
            return "000" + title;
        }

        var key = new System.Text.StringBuilder();
        for (int index = 0; index < title.Length && index < 6; index++)
        {
            key.Append(strokeCounts.GetValueOrDefault(title[index]).ToString("00", CultureInfo.InvariantCulture))
                .Append(((int)title[index]).ToString("00000", CultureInfo.InvariantCulture));
        }

        if (title.Length > 6)
        {
            key.Append(title, 6, title.Length - 6);
        }

        return key.Length <= 100 ? key.ToString() : key.ToString(0, 100);
    }

    // WinForms takes the stroke counts from its Sys\strokecount.dat, which this app does not have. Every key WinForms
    // stored holds the count of each character it covers, so the counts are read back from the keys in the database.
    // A character no stored key covers counts as having none: a new title that starts with one gets "000" and the
    // title until WinForms saves the song.
    private static StrokeKeys ReadStrokeKeys(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        Dictionary<string, string> columns)
    {
        var keys = new StrokeKeys(new Dictionary<string, string>(StringComparer.Ordinal), new Dictionary<char, int>());
        if (!columns.TryGetValue("CJK_StrokeCount", out var column))
        {
            return keys;
        }

        var counts = keys.Counts;
        string key = QuoteIdentifier(column);
        using var command = new SQLiteCommand(
            $"SELECT {QuoteIdentifier(columns["TITLE_1"])}, {key} FROM SONG WHERE {key} GLOB '[0-9][0-9][0-9][0-9][0-9][0-9][0-9]*' AND {key} NOT LIKE '000%';",
            connection,
            transaction);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string stored = Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? "";
            keys.ByTitle.TryAdd(Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture) ?? "", stored);
            // The first six characters of the title have a group each; the rest of the title follows as text.
            string groups = stored.Length <= 42 ? stored : stored[..42];
            for (int at = 0; at + 7 <= groups.Length; at += 7)
            {
                if (!int.TryParse(groups.AsSpan(at, 2), NumberStyles.None, CultureInfo.InvariantCulture, out int count) ||
                    !int.TryParse(groups.AsSpan(at + 2, 5), NumberStyles.None, CultureInfo.InvariantCulture, out int code))
                {
                    break;
                }

                if (count > 0 && code <= char.MaxValue)
                {
                    counts[(char)code] = count;
                }
            }
        }

        return keys;
    }

    private sealed record StrokeKeys(Dictionary<string, string> ByTitle, Dictionary<char, int> Counts);

    private static AdminDatabaseSchemaInventory InventoryFailure(
        string databasePath,
        params AdminDatabaseIssue[] issues)
        => new(
            Succeeded: false,
            databasePath,
            SchemaVersion: 0,
            Tables: Array.Empty<DatabaseTable>(),
            Columns: new Dictionary<string, IReadOnlyList<DatabaseColumn>>(StringComparer.OrdinalIgnoreCase),
            Issues: issues);

    private static AdminDatabaseIssue Issue(
        AdminDatabaseIssueKind kind,
        string message,
        string? tableName = null,
        string? columnName = null,
        AdminDatabaseIssueSeverity severity = AdminDatabaseIssueSeverity.Error)
        => new(kind, severity, message, tableName, columnName);

    private static bool IsSqliteOpenException(Exception ex)
        => ex is SQLiteException or InvalidOperationException or IOException or UnauthorizedAccessException;

    private static bool IsFileSystemException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or NotSupportedException;

    private static void TryRollback(SQLiteTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
        }
    }

    private static AdminDatabaseWriteIssue WriteIssue(
        AdminDatabaseWriteIssueKind kind,
        string message,
        AdminDatabaseIssueSeverity severity = AdminDatabaseIssueSeverity.Error)
        => new(kind, severity, message);

    private sealed class AdminDatabaseWriteOutcome
    {
        public List<int> SongIds { get; } = [];

        public List<int> FolderNos { get; } = [];

        // Read once for the songs saved in one write (see ReadStrokeKeys).
        public StrokeKeys? StrokeKeys { get; set; }
    }

    private sealed class AdminDatabaseWriteException(AdminDatabaseWriteIssueKind kind, string message)
        : InvalidOperationException(message)
    {
        public AdminDatabaseWriteIssueKind Kind { get; } = kind;
    }
}
