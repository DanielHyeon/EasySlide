using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;

namespace Axicultus.Wpf.Library;

[Flags]
public enum SongSearchFields
{
    None = 0,
    Title = 1,
    Lyrics = 2,
    SongNumber = 4,
    BookReference = 8,
    UserReference = 16,
    LicenceAdmin = 32,
    Writer = 64,
    Copyright = 128,
    All = Title | Lyrics | SongNumber | BookReference | UserReference | LicenceAdmin | Writer | Copyright,
}

public enum SearchUsageIssueSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record SearchUsageIssue(SearchUsageIssueSeverity Severity, string Message);

public sealed record SongSearchRequest(
    string DatabasePath,
    string Phrase,
    IReadOnlyList<int> FolderNos,
    SongSearchFields Fields,
    string Key = "",
    string Timing = "",
    bool NotationsOnly = false,
    bool MediaOnly = false,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    string WorkingFolder = "",
    string MediaFolder = "");

public sealed record SongSearchResult(SongSummary Song, string FolderName, IReadOnlyList<string> MatchingFields, string Snippet)
{
    public int SongId => Song.SongId;
    public int FolderNo => Song.FolderNo;
    public string Title => Song.Title;
    public string AlternateTitle => Song.AlternateTitle;
    public int SongNumber => Song.SongNumber;
    public string Category => Song.Category;
    public string Key => Song.Key;

    public SongSearchResult(int songId, int folderNo, string folderName, string title, string alternateTitle,
        int songNumber, string category, string key, IReadOnlyList<string> matchingFields, string snippet)
        : this(new SongSummary(songId, title, alternateTitle, folderNo, songNumber, category, key, ""),
            folderName, matchingFields, snippet) { }
}

internal sealed record SongSearchPage(SongSearchRequest Request, IReadOnlyList<SongSearchResult> Rows);

public sealed record LookupTitleCandidate(
    int SongId,
    string Title,
    string AlternateTitle,
    int FolderNo,
    string FolderName,
    string BookReference,
    string UserReference);

public sealed record UsageRequest(
    string DatabasePath,
    DateTime From,
    DateTime To,
    string SelectedSession);

public sealed record UsageRecord(
    long RecordId,
    DateTime WorshipDate,
    string WorshipList,
    string SongTitle,
    int SongNumber,
    int SongId,
    string Admin1,
    string Admin2);

public sealed record UsageAddRecord(
    DateTime WorshipDate,
    string WorshipList,
    string SongTitle,
    int SongNumber,
    int SongId,
    string Admin1,
    string Admin2);

public sealed record UsageAddRequest(
    string DatabasePath,
    IReadOnlyList<UsageAddRecord> Records);

public sealed record UsageSummary(
    string SongTitle,
    int SongNumber,
    int SongId,
    int Occurrences);

public sealed record UsageReport(
    bool Succeeded,
    string DatabasePath,
    DateTime From,
    DateTime To,
    IReadOnlyList<string> Sessions,
    string SelectedSession,
    IReadOnlyList<UsageRecord> Records,
    IReadOnlyList<UsageSummary> Summary,
    IReadOnlyList<SearchUsageIssue> Issues);

public sealed record UsageDeleteReport(
    bool Succeeded,
    string DatabasePath,
    int DeletedCount,
    IReadOnlyList<SearchUsageIssue> Issues);

public sealed record UsageAddReport(
    bool Succeeded,
    string DatabasePath,
    int AddedCount,
    IReadOnlyList<SearchUsageIssue> Issues);

public sealed record UsageExportReport(
    bool Succeeded,
    string OutputPath,
    IReadOnlyList<SearchUsageIssue> Issues);

public interface ISearchUsageService
{
    Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath);

    Task<(string[] Keys, string[] Timings)> GetFilterChoicesAsync(string databasePath);

    Task<IReadOnlyList<SongSearchResult>> SearchSongsAsync(SongSearchRequest request);

    Task<IReadOnlyList<LookupTitleCandidate>> LookupTitlesAsync(string databasePath, string titlePattern);

    Task<UsageReport> GetUsageAsync(UsageRequest request);

    Task<UsageAddReport> AddUsageRecordsAsync(UsageAddRequest request);

    Task<UsageDeleteReport> DeleteUsageRecordsAsync(string databasePath, IReadOnlyList<long> recordIds);

    Task<UsageExportReport> ExportUsageReportAsync(UsageReport report, string outputPath);

    string GetDefaultUsageDatabasePath(string workingFolder);
}

public sealed class SearchUsageService(IAdminDatabaseRepository adminDatabase) : ISearchUsageService
{
    private const string UsageRelativePath = @"Admin\Database\EsUsage.db";

    // The shipped EsUsage.db declares REC_ID INTEGER without a key, so a row either app adds has no REC_ID.
    // Such a row is known by its rowid, negated so that it cannot be taken for another row's REC_ID.
    private const string UsageRecordKey = "COALESCE(REC_ID, -rowid)";

    private readonly IAdminDatabaseRepository _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));

    public Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath)
        => _adminDatabase.GetSongFoldersAsync(databasePath);

    public Task<(string[] Keys, string[] Timings)> GetFilterChoicesAsync(string databasePath) => Task.Run(() =>
    {
        if (!File.Exists(databasePath)) return (Array.Empty<string>(), Array.Empty<string>());
        using var connection = OpenReadOnly(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM SONG LIMIT 0";
        Dictionary<string, int> columns;
        using (var reader = command.ExecuteReader()) columns = GetReaderColumns(reader);
        return (Choices("KEY"), Choices("TIMING"));

        string[] Choices(string column)
        {
            var choices = new List<string> { "" };
            if (!columns.ContainsKey(column)) return [.. choices];
            command.CommandText = $"SELECT DISTINCT [{column}] FROM SONG WHERE [{column}] <> '' ORDER BY [{column}]";
            using var reader = command.ExecuteReader();
            while (reader.Read()) choices.Add(reader.GetString(0).Trim());
            return [.. choices];
        }
    });

    public Task<IReadOnlyList<SongSearchResult>> SearchSongsAsync(SongSearchRequest request)
        => Task.Run(() => FindSongsAsync(request));

    private async Task<IReadOnlyList<SongSearchResult>> FindSongsAsync(SongSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DatabasePath) || !File.Exists(request.DatabasePath))
        {
            return [];
        }

        var allowedFolders = request.FolderNos.ToHashSet();
        var fields = request.Fields;
        var phrase = request.Phrase.Trim();
        var results = new List<SongSearchResult>();

        if (allowedFolders.Count == 0 || (fields == SongSearchFields.None && !request.NotationsOnly))
            return results;

        var folders = await _adminDatabase.GetSongFoldersAsync(request.DatabasePath).ConfigureAwait(false);
        var folderNames = folders.ToDictionary(folder => folder.FolderNo, folder => folder.Name);
        using var connection = OpenReadOnly(request.DatabasePath);
        using var command = connection.CreateCommand();
        // Keep large, externally supplied folder selections within SQLite's variable limit.
        // Folder membership is checked below for that fallback as well.
        string folderFilter = "FOLDERNO > 0";
        if (allowedFolders.Count <= 500)
        {
            var names = new List<string>(allowedFolders.Count);
            foreach (int folder in allowedFolders)
            {
                string name = "@folder" + names.Count;
                names.Add(name);
                command.Parameters.AddWithValue(name, folder);
            }
            folderFilter += $" AND FOLDERNO IN ({string.Join(",", names)})";
        }
        command.CommandText = "SELECT * FROM SONG LIMIT 0";
        Dictionary<string, int> available;
        using (var schema = command.ExecuteReader()) available = GetReaderColumns(schema);
        var expressions = new List<string>();
        foreach (var (field, names) in SearchColumns)
        {
            if (!fields.HasFlag(field)) continue;
            var predicates = names.Where(available.ContainsKey).Select(name => $"lower([{name}]) LIKE @phrase").ToArray();
            if (predicates.Length > 0)
                expressions.Add($"CASE WHEN {string.Join(" OR ", predicates)} THEN {(int)field} ELSE 0 END");
        }
        command.Parameters.AddWithValue("@phrase", $"%{phrase.ToLowerInvariant()}%");
        if (fields.HasFlag(SongSearchFields.SongNumber) && int.TryParse(phrase, NumberStyles.None,
            CultureInfo.InvariantCulture, out var number) && number > 0)
        {
            expressions.Add($"CASE WHEN SONG_NUMBER = @number THEN {(int)SongSearchFields.SongNumber} ELSE 0 END");
            command.Parameters.AddWithValue("@number", number);
        }
        if (expressions.Count == 0 && fields != SongSearchFields.None) return results;
        var matchMask = expressions.Count == 0 ? "0" : string.Join(" | ", expressions);
        if (expressions.Count > 0) folderFilter += " AND MatchedFields <> 0";
        command.CommandText = $"SELECT *, ({matchMask}) AS MatchedFields FROM SONG WHERE {folderFilter} ORDER BY FOLDERNO, SONG_NUMBER, TITLE_1, SONGID;";
        var mediaTitles = request.MediaOnly ? FindMediaTitles(request.WorkingFolder, request.MediaFolder) : null;
        using var reader = command.ExecuteReader();
        var columns = GetReaderColumns(reader);
        while (reader.Read())
        {
            var folderNo = GetInt(reader, columns, "FOLDERNO");
            if (!allowedFolders.Contains(folderNo))
            {
                continue;
            }

            var row = SongSearchRow.From(reader, columns);
            if (!MatchesFilter(row, request, mediaTitles))
            {
                continue;
            }

            var mask = (SongSearchFields)reader.GetInt32(columns["MatchedFields"]);
            var matches = mask == SongSearchFields.None ? ["Filter"] :
                IndividualFields.Where(field => mask.HasFlag(field)).Select(field => field.ToString()).ToArray();

            results.Add(new SongSearchResult(
                new SongSummary(row.SongId, row.Title, row.AlternateTitle, row.FolderNo, row.SongNumber, row.Category,
                    row.Key, row.Lyrics, row.Copyright, row.BookReference, row.UserReference, row.Writer, row.Sequence,
                    row.FormatData, row.LicenceAdmin1, row.LicenceAdmin2,
                    columns.ContainsKey("CJK_STROKECOUNT") ? GetString(reader, columns, "CJK_STROKECOUNT") : null,
                    columns.ContainsKey("CJK_WORDCOUNT") ? GetString(reader, columns, "CJK_WORDCOUNT") : null),
                folderNames.GetValueOrDefault(row.FolderNo, $"Folder {row.FolderNo}"),
                matches,
                BuildSnippet(row, matches[0])));
        }

        return results;
    }

    public async Task<IReadOnlyList<LookupTitleCandidate>> LookupTitlesAsync(string databasePath, string titlePattern)
    {
        if (string.IsNullOrWhiteSpace(databasePath) || !File.Exists(databasePath))
        {
            return [];
        }

        var folders = await _adminDatabase.GetSongFoldersAsync(databasePath).ConfigureAwait(false);
        var folderNames = folders.ToDictionary(folder => folder.FolderNo, folder => folder.Name);
        var pattern = $"%{titlePattern.Trim()}%";
        using var connection = OpenReadOnly(databasePath);
        using var command = new SQLiteCommand(
            """
            SELECT *
            FROM SONG
            WHERE FOLDERNO > 0 AND TITLE_1 LIKE @title
            ORDER BY CJK_StrokeCount, TITLE_1, SONGID;
            """,
            connection);
        command.Parameters.AddWithValue("@title", pattern);
        using var reader = command.ExecuteReader();
        var columns = GetReaderColumns(reader);
        var candidates = new List<LookupTitleCandidate>();
        while (reader.Read())
        {
            string title = GetString(reader, columns, "TITLE_1");
            int folder = GetInt(reader, columns, "FOLDERNO");
            candidates.Add(new LookupTitleCandidate(
                GetInt(reader, columns, "SONGID"), title, GetString(reader, columns, "TITLE_2"), folder,
                folderNames.GetValueOrDefault(folder, $"Folder {folder}"),
                GetString(reader, columns, "BOOK_REFERENCE"), GetString(reader, columns, "USER_REFERENCE")));
        }

        return candidates;
    }

    public Task<UsageReport> GetUsageAsync(UsageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DatabasePath) || !File.Exists(request.DatabasePath))
        {
            return Task.FromResult(new UsageReport(
                false,
                request.DatabasePath,
                request.From,
                request.To,
                [],
                request.SelectedSession,
                [],
                [],
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, "Usage database was not found.")]));
        }

        try
        {
            var from = request.From.Date;
            var to = request.To.Date;
            // WORSHIP_DATE is text: "yyyy-MM-dd" in rows converted from the old database and "yyyy-MM-dd HH:mm:ss"
            // in rows the WinForms app writes. The date part is compared, so both match on the first and last day.
            using var connection = OpenReadOnly(request.DatabasePath);
            using var command = new SQLiteCommand(
                """
                SELECT *, COALESCE(REC_ID, -rowid) AS RecordKey
                FROM USAGE
                WHERE substr(WORSHIP_DATE, 1, 10) >= @from AND substr(WORSHIP_DATE, 1, 10) <= @to
                ORDER BY substr(WORSHIP_DATE, 1, 10), REC_ID IS NULL, REC_ID, rowid;
                """,
                connection);
            command.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            using var reader = command.ExecuteReader();
            var columns = GetReaderColumns(reader);
            var allRecords = new List<UsageRecord>();
            while (reader.Read())
            {
                allRecords.Add(ReadUsageRecord(reader, columns));
            }

            var sessions = new List<string> { "" };
            foreach (var session in allRecords.Select(record => record.WorshipList).Where(session => !string.IsNullOrWhiteSpace(session)))
            {
                if (!sessions.Contains(session, StringComparer.OrdinalIgnoreCase))
                {
                    sessions.Add(session);
                }
            }

            var filtered = string.IsNullOrWhiteSpace(request.SelectedSession)
                ? allRecords
                : [.. allRecords.Where(record => string.Equals(record.WorshipList, request.SelectedSession, StringComparison.OrdinalIgnoreCase))];
            var summary = filtered
                .GroupBy(record => record.SongId != 0 ? $"id:{record.SongId}" : $"title:{record.SongTitle}|{record.SongNumber}")
                .Select(group =>
                {
                    var first = group.First();
                    return new UsageSummary(first.SongTitle, first.SongNumber, first.SongId, group.Count());
                })
                .OrderByDescending(item => item.Occurrences)
                .ThenBy(item => item.SongTitle, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Task.FromResult(new UsageReport(
                true,
                request.DatabasePath,
                from,
                to,
                sessions,
                request.SelectedSession,
                filtered,
                summary,
                []));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            return Task.FromResult(new UsageReport(
                false,
                request.DatabasePath,
                request.From,
                request.To,
                [],
                request.SelectedSession,
                [],
                [],
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, ex.Message)]));
        }
    }

    public Task<UsageDeleteReport> DeleteUsageRecordsAsync(string databasePath, IReadOnlyList<long> recordIds)
    {
        ArgumentNullException.ThrowIfNull(recordIds);
        if (string.IsNullOrWhiteSpace(databasePath) || !File.Exists(databasePath))
        {
            return Task.FromResult(new UsageDeleteReport(
                false,
                databasePath,
                0,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, "Usage database was not found.")]));
        }

        try
        {
            using var connection = SqliteFile.Open(Path.GetFullPath(databasePath));
            using var transaction = connection.BeginTransaction();
            var deleted = 0;
            using var command = new SQLiteCommand($"DELETE FROM USAGE WHERE {UsageRecordKey} = @recordId;", connection, transaction);
            var idParameter = command.Parameters.Add("@recordId", DbType.Int64);
            foreach (var recordId in recordIds.Distinct())
            {
                idParameter.Value = recordId;
                deleted += command.ExecuteNonQuery();
            }

            transaction.Commit();
            return Task.FromResult(new UsageDeleteReport(true, databasePath, deleted, []));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            return Task.FromResult(new UsageDeleteReport(
                false,
                databasePath,
                0,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, ex.Message)]));
        }
    }

    public Task<UsageAddReport> AddUsageRecordsAsync(UsageAddRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DatabasePath) || !File.Exists(request.DatabasePath))
        {
            return Task.FromResult(new UsageAddReport(
                false,
                request.DatabasePath,
                0,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, "Usage database was not found.")]));
        }

        if (request.Records.Count == 0)
        {
            return Task.FromResult(new UsageAddReport(true, request.DatabasePath, 0, []));
        }

        try
        {
            using var connection = SqliteFile.Open(Path.GetFullPath(request.DatabasePath));
            using var transaction = connection.BeginTransaction();
            var added = 0;
            using var command = new SQLiteCommand(
                """
                INSERT INTO USAGE (WORSHIP_DATE, WORSHIP_LIST, SONG_TITLE, SONG_NUMBER, SONG_ID, ADMIN_1, ADMIN_2)
                VALUES (@date, @worshipList, @songTitle, @songNumber, @songId, @admin1, @admin2);
                """,
                connection,
                transaction);
            var date = command.Parameters.Add("@date", DbType.String);
            var worshipList = command.Parameters.Add("@worshipList", DbType.String);
            var songTitle = command.Parameters.Add("@songTitle", DbType.String);
            var songNumber = command.Parameters.Add("@songNumber", DbType.Int32);
            var songId = command.Parameters.Add("@songId", DbType.Int32);
            var admin1 = command.Parameters.Add("@admin1", DbType.String);
            var admin2 = command.Parameters.Add("@admin2", DbType.String);
            foreach (var record in request.Records)
            {
                // The form the WinForms app writes (a DateTime parameter) and filters on.
                date.Value = record.WorshipDate.Date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                worshipList.Value = Truncate(record.WorshipList, 50);
                songTitle.Value = RemoveMusicSymbol(Truncate(record.SongTitle, 100));
                songNumber.Value = record.SongNumber;
                songId.Value = record.SongId;
                admin1.Value = Truncate(record.Admin1, 50);
                admin2.Value = Truncate(record.Admin2, 50);
                added += command.ExecuteNonQuery();
            }

            transaction.Commit();
            return Task.FromResult(new UsageAddReport(true, request.DatabasePath, added, []));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            return Task.FromResult(new UsageAddReport(
                false,
                request.DatabasePath,
                0,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, ex.Message)]));
        }
    }

    public Task<UsageExportReport> ExportUsageReportAsync(UsageReport report, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return Task.FromResult(new UsageExportReport(
                false,
                outputPath,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, "Usage report output path is empty.")]));
        }

        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, BuildUsageRtf(report), Encoding.UTF8);
            return Task.FromResult(new UsageExportReport(true, outputPath, []));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            return Task.FromResult(new UsageExportReport(
                false,
                outputPath,
                [new SearchUsageIssue(SearchUsageIssueSeverity.Error, ex.Message)]));
        }
    }

    public string GetDefaultUsageDatabasePath(string workingFolder)
        => string.IsNullOrWhiteSpace(workingFolder)
            ? ""
            : Path.GetFullPath(Path.Combine(workingFolder, UsageRelativePath));

    private static SQLiteConnection OpenReadOnly(string path)
        => SqliteFile.Open(Path.GetFullPath(path), readOnly: true);

    private static Dictionary<string, int> GetReaderColumns(SQLiteDataReader reader)
    {
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            columns[reader.GetName(i)] = i;
        }

        return columns;
    }

    private static UsageRecord ReadUsageRecord(SQLiteDataReader reader, IReadOnlyDictionary<string, int> columns)
        => new(
            GetLong(reader, columns, "RecordKey"),
            GetDate(reader, columns, "WORSHIP_DATE") ?? DateTime.MinValue,
            GetString(reader, columns, "WORSHIP_LIST"),
            RemoveMusicSymbol(GetString(reader, columns, "SONG_TITLE")),
            GetInt(reader, columns, "SONG_NUMBER"),
            GetInt(reader, columns, "SONG_ID"),
            GetString(reader, columns, "ADMIN_1"),
            GetString(reader, columns, "ADMIN_2"));

    private static bool MatchesFilter(SongSearchRow row, SongSearchRequest request, HashSet<string>? mediaTitles)
    {
        if (!string.IsNullOrWhiteSpace(request.Key) &&
            !string.Equals(row.Key, request.Key.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Timing) &&
            !string.Equals(row.Timing, request.Timing.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        if (request.NotationsOnly && row.Notations.Length == 0)
        {
            return false;
        }

        if (mediaTitles is not null && !mediaTitles.Contains(row.Title) && !mediaTitles.Contains(row.AlternateTitle))
        {
            return false;
        }

        if (request.ModifiedFrom is not null && (row.LastModified is null || row.LastModified.Value.Date < request.ModifiedFrom.Value.Date))
        {
            return false;
        }

        if (request.ModifiedTo is not null && (row.LastModified is null || row.LastModified.Value.Date > request.ModifiedTo.Value.Date))
        {
            return false;
        }

        return true;
    }

    private static readonly (SongSearchFields Field, string[] Columns)[] SearchColumns =
    [
        (SongSearchFields.Title, ["TITLE_1", "TITLE_2"]), (SongSearchFields.Lyrics, ["LYRICS"]),
        (SongSearchFields.BookReference, ["BOOK_REFERENCE"]), (SongSearchFields.UserReference, ["USER_REFERENCE"]),
        (SongSearchFields.LicenceAdmin, ["LICENCE_ADMIN1", "LICENCE_ADMIN2"]), (SongSearchFields.Writer, ["WRITER"]),
        (SongSearchFields.Copyright, ["COPYRIGHT"]),
    ];

    private static readonly SongSearchFields[] IndividualFields =
        [SongSearchFields.Title, SongSearchFields.Lyrics, SongSearchFields.SongNumber, SongSearchFields.BookReference,
         SongSearchFields.UserReference, SongSearchFields.LicenceAdmin, SongSearchFields.Writer, SongSearchFields.Copyright];

    private static string BuildSnippet(SongSearchRow row, string field)
        => field switch
        {
            "Title" => string.IsNullOrWhiteSpace(row.AlternateTitle)
                ? row.Title
                : $"{row.Title} / {row.AlternateTitle}",
            "Lyrics" => TrimSnippet(row.Lyrics),
            "SongNumber" => row.SongNumber.ToString(CultureInfo.InvariantCulture),
            "BookReference" => row.BookReference,
            "UserReference" => row.UserReference,
            "LicenceAdmin" => string.Join(" / ", new[] { row.LicenceAdmin1, row.LicenceAdmin2 }.Where(value => !string.IsNullOrWhiteSpace(value))),
            "Writer" => row.Writer,
            "Copyright" => row.Copyright,
            _ => row.Title,
        };

    private static string BuildUsageRtf(UsageReport report)
    {
        var songNumberUsed = report.Records.Any(record => record.SongNumber != 0);
        var builder = new StringBuilder();
        builder.Append(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil Microsoft Sans Serif;}}\viewkind1\uc1\pard\f0\fs20 ");
        builder.Append(@"\b\ul Usage Details:\b0\ulnone\par\par ");
        builder.Append(@"\b Period:\b0  ");
        builder.Append(RtfEscape(report.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        builder.Append(" to ");
        builder.Append(RtfEscape(report.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        builder.Append(@" (yyyy-mm-dd)\par ");
        builder.Append(RtfEscape(string.IsNullOrWhiteSpace(report.SelectedSession)
            ? "All Worship Lists displayed"
            : $"Worship List Restricted to '{report.SelectedSession}'"));
        builder.Append(@"\par\par ");
        builder.Append(RtfEscape("Date\tWorship List\tSong Title"));
        if (songNumberUsed)
        {
            builder.Append(RtfEscape("\tNo."));
        }

        builder.Append(RtfEscape("\tLic Admin"));
        builder.Append(@"\par ");
        foreach (var record in report.Records)
        {
            var admin = string.Join("/", new[] { record.Admin1, record.Admin2 }.Where(value => !string.IsNullOrWhiteSpace(value)));
            builder.Append(RtfEscape($"{record.WorshipDate:yyyy-MM-dd}\t{record.WorshipList}\t{record.SongTitle}"));
            if (songNumberUsed)
            {
                builder.Append(RtfEscape($"\t{record.SongNumber}"));
            }

            builder.Append(RtfEscape($"\t{admin}"));
            builder.Append(@"\par ");
        }

        builder.Append(@"\par\par\b\ul Occurrences:\b0\ulnone\par\par ");
        builder.Append(RtfEscape("Occurrence\tSong Title"));
        if (songNumberUsed)
        {
            builder.Append(RtfEscape("\tNo."));
        }

        builder.Append(@"\par ");
        foreach (var item in report.Summary)
        {
            builder.Append(RtfEscape($"{item.Occurrences}\t{item.SongTitle}"));
            if (songNumberUsed)
            {
                builder.Append(RtfEscape($"\t{item.SongNumber}"));
            }

            builder.Append(@"\par ");
        }

        builder.Append('}');
        return builder.ToString();
    }

    private static HashSet<string> FindMediaTitles(string workingFolder, string mediaFolder)
    {
        var titles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(workingFolder) && string.IsNullOrWhiteSpace(mediaFolder)) return titles;
        var extensions = SourceFileExtensions.Media(workingFolder);
        var mediaRoot = Directory.Exists(mediaFolder) ? mediaFolder : Path.Combine(workingFolder, "Media");
        var files = Rendering.FolderFileEnumerator.Enumerate(mediaRoot, extensions, includeSubfolders: true);
        foreach (var file in files) titles.Add(Path.GetFileNameWithoutExtension(file));
        return titles;
    }

    private static string TrimSnippet(string value)
    {
        var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
        var firstLine = normalized.Split('\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? "";
        return firstLine.Length <= 120 ? firstLine : firstLine[..120];
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string RemoveMusicSymbol(string value)
        => value.Replace("♪", "", StringComparison.Ordinal).Trim();

    private static string RtfEscape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '\\' => @"\\",
                '{' => @"\{",
                '}' => @"\}",
                '\t' => "\t",
                '\n' => @"\par ",
                '\r' => "",
                _ when ch <= 0x7f => ch,
                _ => $"\\u{Convert.ToInt32(ch, CultureInfo.InvariantCulture)}?",
            });
        }

        return builder.ToString();
    }

    private static string GetString(SQLiteDataReader reader, IReadOnlyDictionary<string, int> columns, string name)
    {
        if (!columns.TryGetValue(name, out var ordinal) || reader.IsDBNull(ordinal))
        {
            return "";
        }

        return Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? "";
    }

    private static int GetInt(SQLiteDataReader reader, IReadOnlyDictionary<string, int> columns, string name)
        => int.TryParse(GetString(reader, columns, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;

    private static long GetLong(SQLiteDataReader reader, IReadOnlyDictionary<string, int> columns, string name)
        => long.TryParse(GetString(reader, columns, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;

    private static DateTime? GetDate(SQLiteDataReader reader, IReadOnlyDictionary<string, int> columns, string name)
    {
        var dateText = GetString(reader, columns, name);
        if (string.IsNullOrWhiteSpace(dateText))
        {
            return null;
        }

        // Shared with the WinForms app: yyyy-MM-dd, or what older versions wrote in the Korean or English format.
        return Axicultus.Util.StoredDate.TryParse(dateText, out var value)
            ? value.Date
            : null;
    }

    private static bool IsRecoverable(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException
            or SQLiteException;

    private sealed record SongSearchRow(
        int SongId,
        int FolderNo,
        string Title,
        string AlternateTitle,
        int SongNumber,
        string Category,
        string Key,
        string Lyrics,
        string Sequence,
        string Writer,
        string Copyright,
        string Timing,
        string Notations,
        string LicenceAdmin1,
        string LicenceAdmin2,
        string BookReference,
        string UserReference,
        string Settings,
        string FormatData,
        DateTime? LastModified)
    {
        public static SongSearchRow From(SQLiteDataReader reader, IReadOnlyDictionary<string, int> columns)
            => new(
                GetInt(reader, columns, "SONGID"),
                GetInt(reader, columns, "FOLDERNO"),
                GetString(reader, columns, "TITLE_1"),
                GetString(reader, columns, "TITLE_2"),
                GetInt(reader, columns, "SONG_NUMBER"),
                GetString(reader, columns, "CATEGORY"),
                GetString(reader, columns, "KEY"),
                GetString(reader, columns, "LYRICS"),
                GetString(reader, columns, "SEQUENCE"),
                GetString(reader, columns, "WRITER"),
                GetString(reader, columns, "COPYRIGHT"),
                GetString(reader, columns, "TIMING"),
                GetString(reader, columns, "MSC"),
                GetString(reader, columns, "LICENCE_ADMIN1"),
                GetString(reader, columns, "LICENCE_ADMIN2"),
                GetString(reader, columns, "BOOK_REFERENCE"),
                GetString(reader, columns, "USER_REFERENCE"),
                GetString(reader, columns, "SETTINGS"),
                GetString(reader, columns, "FORMATDATA"),
                GetDate(reader, columns, "LastModified"));
    }
}
