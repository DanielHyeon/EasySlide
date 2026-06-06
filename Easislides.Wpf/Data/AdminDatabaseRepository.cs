using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Easislides.Wpf.Data;

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
    int GroupStyle = 0);

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
    string Copyright = "");

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

public sealed record SongFolderWriteModel(
    int FolderNo,
    string Name,
    bool IsEnabled,
    int GroupStyle = 0);

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
    int Capo = 0,
    string Timing = "",
    string Key = "",
    string Notations = "",
    string Category = "",
    string LicenceAdmin1 = "",
    string LicenceAdmin2 = "",
    string BookReference = "",
    string UserReference = "",
    string Settings = "",
    string FormatData = "");

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

    Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath);

    Task<AdminDatabaseWriteReport> SaveFolderAsync(
        string databasePath,
        string backupRoot,
        SongFolderWriteModel folder);

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
        => Task.FromResult(GetSongFolders(databasePath));

    public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
        => Task.FromResult(GetSongs(databasePath, folderNo));

    public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
        => Task.FromResult(GetSongDetail(databasePath, songId));

    public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
        => Task.FromResult(GetDeletedSongs(databasePath));

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
            (connection, transaction, outcome) => SaveFolder(connection, transaction, folder, outcome)));
    }

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

    public Task<AdminDatabaseWriteReport> MoveSongsAsync(
        string databasePath,
        string backupRoot,
        IReadOnlyList<SongMoveRequest> moves)
    {
        ArgumentNullException.ThrowIfNull(moves);
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
    // 쓰지 않고 전용 경로로 처리한다. 실패하면 백업으로 복원한다(다른 쓰기 작업과 동일한 안전 보장).
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
                [WriteIssue(AdminDatabaseWriteIssueKind.InvalidRequest, "Backup root cannot be empty.")]);
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
                [WriteIssue(AdminDatabaseWriteIssueKind.BackupFailed, $"Unable to create AdminDB backup: {ex.Message}")]);
        }

        try
        {
            using var connection = OpenConnection(inventory.DatabasePath, readOnly: false);
            using var command = new SQLiteCommand("VACUUM;", connection);
            command.ExecuteNonQuery();
        }
        catch (Exception ex) when (IsSqliteOpenException(ex))
        {
            // 압축 실패 — 백업으로 복원하고 실패 보고.
            return RestoreAndReportFailure(
                AdminDatabaseWriteOperation.Compact, inventory.DatabasePath, backupPath,
                new AdminDatabaseWriteOutcome(), AdminDatabaseWriteIssueKind.WriteFailed,
                $"Database compaction failed: {ex.Message}");
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
                Issue(AdminDatabaseIssueKind.OpenFailed, $"Unable to open AdminDB SQLite database: {ex.Message}"));
        }
    }

    private static IReadOnlyList<SongFolderSummary> GetSongFolders(string databasePath)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        var folderColumns = ReadColumnMap(connection, "FOLDER");
        using var command = new SQLiteCommand(
            $"""
            SELECT
                f.FolderNo,
                f.Name,
                f.Use,
                {SelectColumn(folderColumns, "GroupStyle", "GroupStyle", "f")},
                COUNT(s.SONGID) AS SongCount
            FROM FOLDER f
            LEFT JOIN SONG s ON s.FOLDERNO = f.FolderNo
            GROUP BY f.FolderNo, f.Name, f.Use
            ORDER BY f.FolderNo, f.Name;
            """,
            connection);
        using var reader = command.ExecuteReader();
        var folders = new List<SongFolderSummary>();
        while (reader.Read())
        {
            folders.Add(new SongFolderSummary(
                GetInt(reader, "FolderNo"),
                GetString(reader, "Name"),
                ParseEnabled(GetString(reader, "Use")),
                GetInt(reader, "SongCount"),
                GetInt(reader, "GroupStyle")));
        }

        return folders;
    }

    private static IReadOnlyList<SongSummary> GetSongs(string databasePath, int? folderNo)
    {
        EnsureCompatible(databasePath);
        using var connection = OpenConnection(Path.GetFullPath(databasePath), readOnly: true);
        using var command = new SQLiteCommand(
            """
            SELECT
                SONGID,
                TITLE_1,
                TITLE_2,
                FOLDERNO,
                SONG_NUMBER,
                CATEGORY,
                "KEY",
                LYRICS,
                COPYRIGHT
            FROM SONG
            WHERE (@folderNo IS NULL OR FOLDERNO = @folderNo)
            ORDER BY FOLDERNO, SONG_NUMBER, TITLE_1, SONGID;
            """,
            connection);
        command.Parameters.AddWithValue("@folderNo", folderNo is null ? DBNull.Value : folderNo.Value);
        using var reader = command.ExecuteReader();
        var songs = new List<SongSummary>();
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
                GetString(reader, "COPYRIGHT")));
        }

        return songs;
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

    private static IReadOnlyList<DeletedSongSummary> GetDeletedSongs(string databasePath)
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
        Action<SQLiteConnection, SQLiteTransaction, AdminDatabaseWriteOutcome> write)
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

        if (string.IsNullOrWhiteSpace(backupRoot))
        {
            return FailedWriteReport(
                operation,
                inventory.DatabasePath,
                backupPath: null,
                new AdminDatabaseWriteOutcome(),
                [WriteIssue(AdminDatabaseWriteIssueKind.InvalidRequest, "Backup root cannot be empty.")]);
        }

        var outcome = new AdminDatabaseWriteOutcome();
        string backupPath;
        try
        {
            backupPath = CreateBackup(inventory.DatabasePath, backupRoot);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return FailedWriteReport(
                operation,
                inventory.DatabasePath,
                backupPath: null,
                outcome,
                [WriteIssue(AdminDatabaseWriteIssueKind.BackupFailed, $"Unable to create AdminDB backup: {ex.Message}")]);
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
        catch (AdminDatabaseWriteException ex)
        {
            return RestoreAndReportFailure(operation, inventory.DatabasePath, backupPath, outcome, ex.Kind, ex.Message);
        }
        catch (Exception ex) when (IsSqliteOpenException(ex))
        {
            return RestoreAndReportFailure(
                operation,
                inventory.DatabasePath,
                backupPath,
                outcome,
                AdminDatabaseWriteIssueKind.WriteFailed,
                $"AdminDB write failed: {ex.Message}");
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
        var hasGroupStyle = folderColumns.TryGetValue("GroupStyle", out var groupStyleColumn);
        var commandText = exists
            ? BuildFolderUpdateSql(folderColumns, hasGroupStyle, groupStyleColumn)
            : BuildFolderInsertSql(folderColumns, hasGroupStyle, groupStyleColumn);
        using var command = new SQLiteCommand(
            commandText,
            connection,
            transaction);
        command.Parameters.AddWithValue("@folderNo", folder.FolderNo);
        command.Parameters.AddWithValue("@name", Normalize(folder.Name));
        command.Parameters.AddWithValue("@use", folder.IsEnabled ? "True" : "False");
        if (hasGroupStyle)
        {
            command.Parameters.AddWithValue("@groupStyle", NormalizeLegacyFolderGroupStyle(folder.GroupStyle));
        }

        command.ExecuteNonQuery();
        outcome.FolderNos.Add(folder.FolderNo);
    }

    private static string BuildFolderUpdateSql(
        IReadOnlyDictionary<string, string> folderColumns,
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
        IReadOnlyDictionary<string, string> folderColumns,
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

    private static int NormalizeLegacyFolderGroupStyle(int groupStyle)
        => groupStyle == 1 ? 1 : 0;

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
                "At least one folder delete is required.");
        }

        SetFolderUse(
            connection,
            transaction,
            deletes.Select(delete => delete.FolderNo).ToArray(),
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
                "At least one folder recovery is required.");
        }

        SetFolderUse(
            connection,
            transaction,
            recoveries.Select(recovery => recovery.FolderNo).ToArray(),
            isEnabled: true,
            outcome);
    }

    private static void SetFolderUse(
        SQLiteConnection connection,
        SQLiteTransaction transaction,
        IReadOnlyList<int> folderNos,
        bool isEnabled,
        AdminDatabaseWriteOutcome outcome)
    {
        if (folderNos.Any(folderNo => folderNo <= 0))
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "Folder numbers must be positive.");
        }

        if (folderNos.Distinct().Count() != folderNos.Count)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "Folder requests must contain distinct folder numbers.");
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
                    $"Folder was not found: {folderNo}");
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
        var values = BuildSongValues(song);
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
            foreach (var value in updates)
            {
                command.Parameters.AddWithValue(value.ParameterName, value.Value ?? DBNull.Value);
            }

            command.Parameters.AddWithValue("@songId", song.SongId.Value);
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    $"Song was not found: {song.SongId.Value}");
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
        foreach (var value in inserts)
        {
            insertCommand.Parameters.AddWithValue(value.ParameterName, value.Value ?? DBNull.Value);
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
                "At least one song move is required.");
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
                command.Parameters.AddWithValue("@lastModified", DateTime.Now.Date);
            }

            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    $"Song was not found: {move.SongId}");
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
                "At least one song delete is required.");
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
                "At least one song recovery is required.");
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

            using var command = new SQLiteCommand(
                $"UPDATE SONG SET {string.Join(", ", assignments)} WHERE {QuoteIdentifier(columns["SONGID"])} = @songId;",
                connection,
                transaction);
            command.Parameters.AddWithValue("@songId", recovery.SongId);
            command.Parameters.AddWithValue("@targetFolderNo", recovery.TargetFolderNo);
            command.Parameters.AddWithValue("@lastModified", DateTime.Now.Date);
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new AdminDatabaseWriteException(
                    AdminDatabaseWriteIssueKind.NotFound,
                    $"Song was not found: {recovery.SongId}");
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
                "At least one folder reorder request is required.");
        }

        if (order.Any(item => item.FolderNo <= 0 || item.NewFolderNo <= 0))
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "Folder numbers must be positive.");
        }

        if (order.Select(item => item.FolderNo).Distinct().Count() != order.Count ||
            order.Select(item => item.NewFolderNo).Distinct().Count() != order.Count)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "Folder reorder requests must contain distinct source and target folder numbers.");
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
                    $"Folder was not found: {item.FolderNo}");
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
                        $"Target folder number is already in use: {item.NewFolderNo}");
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
                    $"Folder was not found: {item.FolderNo}");
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
                    $"Folder was not found: {item.FolderNo}");
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
                "Folder number must be positive.");
        }

        if (order.Count == 0)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "At least one song reorder request is required.");
        }

        if (order.Any(item => item.SongId <= 0 || item.SongNumber <= 0))
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "Song ids and song numbers must be positive.");
        }

        if (order.Select(item => item.SongId).Distinct().Count() != order.Count ||
            order.Select(item => item.SongNumber).Distinct().Count() != order.Count)
        {
            throw new AdminDatabaseWriteException(
                AdminDatabaseWriteIssueKind.InvalidRequest,
                "Song reorder requests must contain distinct song ids and song numbers.");
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
                    $"Song was not found in folder {folderNo}: {item.SongId}");
            }

            outcome.SongIds.Add(item.SongId);
        }

        outcome.FolderNos.Add(folderNo);
    }

    private static IReadOnlyDictionary<string, object?> BuildSongValues(SongWriteModel song)
    {
        var title = Limit(song.Title, 100);
        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["TITLE_1"] = title,
            ["TITLE_2"] = Limit(song.AlternateTitle, 100),
            ["SONG_NUMBER"] = song.SongNumber,
            ["FOLDERNO"] = song.FolderNo,
            ["LYRICS"] = Normalize(song.Lyrics),
            ["SEQUENCE"] = Limit(song.Sequence, 100),
            ["WRITER"] = Limit(song.Writer, 100),
            ["COPYRIGHT"] = Limit(song.Copyright, 100),
            ["CJK_WordCount"] = ComputeLegacyCjkWordCount(title),
            ["CJK_StrokeCount"] = ComputeLegacyCjkStrokeCount(title),
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
            ["LastModified"] = DateTime.Now.Date,
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
        return backupPath;
    }

    private static void RestoreBackup(string backupPath, string databasePath)
    {
        File.Copy(backupPath, databasePath, overwrite: true);
        RestoreSidecar(backupPath, databasePath, "-wal");
        RestoreSidecar(backupPath, databasePath, "-shm");
    }

    private static void RestoreSidecar(string backupPath, string databasePath, string suffix)
    {
        var backupSidecar = backupPath + suffix;
        var databaseSidecar = databasePath + suffix;
        if (File.Exists(backupSidecar))
        {
            File.Copy(backupSidecar, databaseSidecar, overwrite: true);
            return;
        }

        if (File.Exists(databaseSidecar))
        {
            File.Delete(databaseSidecar);
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

    private static IReadOnlyDictionary<string, string> ReadColumnMap(SQLiteConnection connection, string tableName)
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

    private static AdminDatabaseWriteReport RestoreAndReportFailure(
        AdminDatabaseWriteOperation operation,
        string databasePath,
        string backupPath,
        AdminDatabaseWriteOutcome outcome,
        AdminDatabaseWriteIssueKind kind,
        string message)
    {
        var issues = new List<AdminDatabaseWriteIssue>
        {
            WriteIssue(kind, message),
        };
        try
        {
            RestoreBackup(backupPath, databasePath);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            issues.Add(WriteIssue(
                AdminDatabaseWriteIssueKind.RestoreFailed,
                $"AdminDB backup restore failed: {ex.Message}",
                AdminDatabaseIssueSeverity.Warning));
        }

        return FailedWriteReport(operation, databasePath, backupPath, outcome, issues);
    }

    private static IReadOnlyList<AdminDatabaseWriteIssue> ToWriteIssues(IReadOnlyList<AdminDatabaseIssue> issues)
        => issues
            .Select(issue => WriteIssue(MapWriteIssueKind(issue.Kind), issue.Message, issue.Severity))
            .ToArray();

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
        throw new InvalidOperationException($"AdminDB schema is not compatible: {detail}");
    }

    private static (string Path, AdminDatabaseIssue? Issue) NormalizeExistingDatabasePath(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return (databasePath, Issue(AdminDatabaseIssueKind.SourceMissing, "AdminDB path cannot be empty."));
        }

        var fullPath = Path.GetFullPath(databasePath);
        if (Directory.Exists(fullPath))
        {
            return (fullPath, Issue(AdminDatabaseIssueKind.SourceNotFile, $"AdminDB path is a directory: {fullPath}"));
        }

        if (!File.Exists(fullPath))
        {
            return (fullPath, Issue(AdminDatabaseIssueKind.SourceMissing, $"AdminDB file does not exist: {fullPath}"));
        }

        return (fullPath, null);
    }

    private static IReadOnlyList<DatabaseTable> ReadTables(SQLiteConnection connection)
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

    private static IReadOnlyDictionary<string, IReadOnlyList<DatabaseColumn>> ReadColumns(
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

    private static IReadOnlyList<AdminDatabaseIssue> ValidateCompatibility(
        IReadOnlyDictionary<string, IReadOnlyList<DatabaseColumn>> columns)
    {
        var issues = new List<AdminDatabaseIssue>();
        foreach (var (tableName, requiredColumns) in RequiredSchema)
        {
            if (!columns.TryGetValue(tableName, out var tableColumns))
            {
                issues.Add(Issue(
                    AdminDatabaseIssueKind.MissingTable,
                    $"Required AdminDB table is missing: {tableName}",
                    tableName));
                continue;
            }

            var names = tableColumns.Select(column => column.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var columnName in requiredColumns.Where(column => !names.Contains(column)))
            {
                issues.Add(Issue(
                    AdminDatabaseIssueKind.MissingColumn,
                    $"Required AdminDB column is missing: {tableName}.{columnName}",
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
        IReadOnlyDictionary<string, string> columns,
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

        if (value is DateTime date)
        {
            return date.Date;
        }

        return DateTime.TryParse(
            Convert.ToString(value, CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var parsed)
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

        var spaceIndex = title.IndexOf(' ', StringComparison.Ordinal);
        if (spaceIndex > 0 && spaceIndex - 1 < length)
        {
            length = spaceIndex - 1;
        }

        return Math.Max(length, 0).ToString("000", CultureInfo.InvariantCulture);
    }

    private static string ComputeLegacyCjkStrokeCount(string title)
    {
        var value = "000" + title;
        return value.Length <= 100 ? value : value[..100];
    }

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
    }

    private sealed class AdminDatabaseWriteException(AdminDatabaseWriteIssueKind kind, string message)
        : InvalidOperationException(message)
    {
        public AdminDatabaseWriteIssueKind Kind { get; } = kind;
    }
}
