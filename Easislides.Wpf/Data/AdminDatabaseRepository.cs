using System;
using System.Collections.Generic;
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
    int SongCount);

public sealed record SongSummary(
    int SongId,
    string Title,
    string AlternateTitle,
    int FolderNo,
    int SongNumber,
    string Category,
    string Key,
    string Lyrics);

public interface IAdminDatabaseRepository
{
    Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath);

    Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath);

    Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null);
}

public sealed class AdminDatabaseRepository : IAdminDatabaseRepository
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
        using var command = new SQLiteCommand(
            """
            SELECT
                f.FolderNo,
                f.Name,
                f.Use,
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
                GetInt(reader, "SongCount")));
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
                LYRICS
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
                GetString(reader, "LYRICS")));
        }

        return songs;
    }

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
}
