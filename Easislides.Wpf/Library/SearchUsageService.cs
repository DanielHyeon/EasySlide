using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

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
    DateTime? ModifiedTo = null);

public sealed record SongSearchResult(
    int SongId,
    int FolderNo,
    string FolderName,
    string Title,
    string AlternateTitle,
    int SongNumber,
    string Category,
    string Key,
    IReadOnlyList<string> MatchingFields,
    string Snippet);

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

public sealed record UsageExportReport(
    bool Succeeded,
    string OutputPath,
    IReadOnlyList<SearchUsageIssue> Issues);

public interface ISearchUsageService
{
    Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath);

    Task<IReadOnlyList<SongSearchResult>> SearchSongsAsync(SongSearchRequest request);

    Task<IReadOnlyList<LookupTitleCandidate>> LookupTitlesAsync(string databasePath, string titlePattern);

    Task<UsageReport> GetUsageAsync(UsageRequest request);

    Task<UsageDeleteReport> DeleteUsageRecordsAsync(string databasePath, IReadOnlyList<long> recordIds);

    Task<UsageExportReport> ExportUsageReportAsync(UsageReport report, string outputPath);

    string GetDefaultUsageDatabasePath(string workingFolder);
}

public sealed class SearchUsageService : ISearchUsageService
{
    private const string UsageRelativePath = @"Admin\Database\EsUsage.db";

    private readonly IAdminDatabaseRepository _adminDatabase;

    public SearchUsageService(IAdminDatabaseRepository adminDatabase)
    {
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
    }

    public Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath)
        => _adminDatabase.GetSongFoldersAsync(databasePath);

    public async Task<IReadOnlyList<SongSearchResult>> SearchSongsAsync(SongSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DatabasePath) || !File.Exists(request.DatabasePath))
        {
            return [];
        }

        var folders = await _adminDatabase.GetSongFoldersAsync(request.DatabasePath).ConfigureAwait(false);
        var folderNames = folders.ToDictionary(folder => folder.FolderNo, folder => folder.Name);
        var allowedFolders = request.FolderNos.Count > 0
            ? new HashSet<int>(request.FolderNos)
            : folders.Where(folder => folder.IsEnabled).Select(folder => folder.FolderNo).ToHashSet();
        var fields = request.Fields == SongSearchFields.None ? SongSearchFields.Title : request.Fields;
        var phrase = request.Phrase.Trim();
        var results = new List<SongSearchResult>();

        using var connection = OpenReadOnly(request.DatabasePath);
        using var command = new SQLiteCommand(
            """
            SELECT *
            FROM SONG
            WHERE FOLDERNO > 0
            ORDER BY FOLDERNO, SONG_NUMBER, TITLE_1, SONGID;
            """,
            connection);
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
            if (!MatchesFilter(row, request))
            {
                continue;
            }

            var matches = GetMatchingFields(row, phrase, fields);
            if (matches.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(phrase))
                {
                    continue;
                }

                matches.Add("Filter");
            }

            results.Add(new SongSearchResult(
                row.SongId,
                row.FolderNo,
                folderNames.GetValueOrDefault(row.FolderNo, $"Folder {row.FolderNo}"),
                row.Title,
                row.AlternateTitle,
                row.SongNumber,
                row.Category,
                row.Key,
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
        var pattern = titlePattern.Trim();
        using var connection = OpenReadOnly(databasePath);
        using var command = new SQLiteCommand(
            """
            SELECT *
            FROM SONG
            WHERE FOLDERNO > 0
            ORDER BY CJK_StrokeCount, TITLE_1, SONGID;
            """,
            connection);
        using var reader = command.ExecuteReader();
        var columns = GetReaderColumns(reader);
        var candidates = new List<LookupTitleCandidate>();
        while (reader.Read())
        {
            var row = SongSearchRow.From(reader, columns);
            if (!Contains(row.Title, pattern))
            {
                continue;
            }

            candidates.Add(new LookupTitleCandidate(
                row.SongId,
                row.Title,
                row.AlternateTitle,
                row.FolderNo,
                folderNames.GetValueOrDefault(row.FolderNo, $"Folder {row.FolderNo}"),
                row.BookReference,
                row.UserReference));
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
            using var connection = OpenReadOnly(request.DatabasePath);
            using var command = new SQLiteCommand(
                """
                SELECT *
                FROM USAGE
                WHERE WORSHIP_DATE >= @from AND WORSHIP_DATE <= @to
                ORDER BY WORSHIP_DATE, REC_ID;
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
                : allRecords
                    .Where(record => string.Equals(record.WorshipList, request.SelectedSession, StringComparison.OrdinalIgnoreCase))
                    .ToList();
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
            using var connection = new SQLiteConnection($"Data Source={Path.GetFullPath(databasePath)};Version=3;");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var deleted = 0;
            foreach (var recordId in recordIds.Distinct())
            {
                using var command = new SQLiteCommand("DELETE FROM USAGE WHERE REC_ID = @recordId;", connection, transaction);
                command.Parameters.AddWithValue("@recordId", recordId);
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
    {
        var connection = new SQLiteConnection($"Data Source={Path.GetFullPath(path)};Version=3;Read Only=True;");
        connection.Open();
        return connection;
    }

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
            GetLong(reader, columns, "REC_ID"),
            GetDate(reader, columns, "WORSHIP_DATE") ?? DateTime.MinValue,
            GetString(reader, columns, "WORSHIP_LIST"),
            RemoveMusicSymbol(GetString(reader, columns, "SONG_TITLE")),
            GetInt(reader, columns, "SONG_NUMBER"),
            GetInt(reader, columns, "SONG_ID"),
            GetString(reader, columns, "ADMIN_1"),
            GetString(reader, columns, "ADMIN_2"));

    private static bool MatchesFilter(SongSearchRow row, SongSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Key) &&
            !string.Equals(row.Key, request.Key.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Timing) &&
            !string.Equals(row.Timing, request.Timing.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (request.NotationsOnly && string.IsNullOrWhiteSpace(row.Notations))
        {
            return false;
        }

        if (request.MediaOnly && !ContainsMediaMarker(row))
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

    private static List<string> GetMatchingFields(SongSearchRow row, string phrase, SongSearchFields fields)
    {
        var matches = new List<string>();
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return matches;
        }

        if (fields.HasFlag(SongSearchFields.Title) &&
            (Contains(row.Title, phrase) || Contains(row.AlternateTitle, phrase)))
        {
            matches.Add("Title");
        }

        if (fields.HasFlag(SongSearchFields.Lyrics) && Contains(row.Lyrics, phrase))
        {
            matches.Add("Lyrics");
        }

        if (fields.HasFlag(SongSearchFields.SongNumber) &&
            int.TryParse(phrase, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) &&
            row.SongNumber == number)
        {
            matches.Add("SongNumber");
        }

        if (fields.HasFlag(SongSearchFields.BookReference) && Contains(row.BookReference, phrase))
        {
            matches.Add("BookReference");
        }

        if (fields.HasFlag(SongSearchFields.UserReference) && Contains(row.UserReference, phrase))
        {
            matches.Add("UserReference");
        }

        if (fields.HasFlag(SongSearchFields.LicenceAdmin) &&
            (Contains(row.LicenceAdmin1, phrase) || Contains(row.LicenceAdmin2, phrase)))
        {
            matches.Add("LicenceAdmin");
        }

        if (fields.HasFlag(SongSearchFields.Writer) && Contains(row.Writer, phrase))
        {
            matches.Add("Writer");
        }

        if (fields.HasFlag(SongSearchFields.Copyright) && Contains(row.Copyright, phrase))
        {
            matches.Add("Copyright");
        }

        return matches;
    }

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

    private static bool Contains(string value, string phrase)
        => string.IsNullOrWhiteSpace(phrase) ||
           value.Contains(phrase, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsMediaMarker(SongSearchRow row)
        => Contains(row.Lyrics, "<<") || Contains(row.Settings, "<<") || Contains(row.FormatData, "<<");

    private static string TrimSnippet(string value)
    {
        var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
        var firstLine = normalized.Split('\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? "";
        return firstLine.Length <= 120 ? firstLine : firstLine[..120];
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
        var text = GetString(reader, columns, name);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var value)
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
