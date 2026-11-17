using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Easislides.Wpf.Data;
using OfficeLib;

namespace Easislides.Wpf.Library;

public enum ImportSourceKind
{
    Unknown,
    EasiSlidesText,
    EasiSlidesXml,
    EasiSlidesDatabase,
    DocumentFolder,
    AccessDatabase,
}

public enum ImportDuplicatePolicy
{
    SkipExisting,
    KeepExisting,
    ReplaceExisting,
}

public enum ImportExportIssueSeverity
{
    Info,
    Warning,
    Error,
}

public enum ImportResultKind
{
    Inserted,
    Replaced,
    Skipped,
    Failed,
}

public enum ExportFormat
{
    Xml,
    EasiSlidesText,
    EasiSlidesDatabase,
    Html,
    Rtf,
}

public sealed record ImportExportIssue(ImportExportIssueSeverity Severity, string Message);

public sealed record ImportSourceFolder(string Name, int SongCount);

public sealed record ImportPreview(
    bool Succeeded,
    string SourcePath,
    ImportSourceKind Kind,
    int TotalSongs,
    IReadOnlyList<ImportSourceFolder> Folders,
    IReadOnlyList<ImportExportIssue> Issues);

public sealed record AccessImportTable(string Name, IReadOnlyList<string> Columns, int RowCount);

public sealed record AccessImportMapping(
    string TableName,
    string TitleColumn,
    IReadOnlyList<string> LyricsColumns,
    string AlternateTitleColumn = "",
    string SongNumberColumn = "",
    string WriterColumn = "",
    string CopyrightColumn = "",
    string KeyColumn = "",
    string TimingColumn = "",
    string BookReferenceColumn = "",
    string UserReferenceColumn = "",
    string LicenceAdmin1Column = "",
    string LicenceAdmin2Column = "");

public sealed record AccessImportSchema(
    bool Succeeded,
    string SourcePath,
    IReadOnlyList<AccessImportTable> Tables,
    AccessImportMapping? SuggestedMapping,
    IReadOnlyList<ImportExportIssue> Issues);

public sealed record ImportRequest(
    string DatabasePath,
    string BackupRoot,
    string SourcePath,
    int TargetFolderNo,
    IReadOnlyList<string> SelectedSourceFolders,
    ImportDuplicatePolicy DuplicatePolicy,
    AccessImportMapping? AccessMapping = null);

public sealed record ImportResultItem(string Title, ImportResultKind Kind, string Message);

public sealed record ImportReport(
    bool Succeeded,
    string SourcePath,
    int ImportedNew,
    int Replaced,
    int Skipped,
    int Failed,
    IReadOnlyList<ImportResultItem> Items,
    IReadOnlyList<ImportExportIssue> Issues);

public sealed record ExportSongCandidate(
    int SongId,
    string Title,
    int FolderNo,
    string FolderName,
    int SongNumber);

public sealed record PraiseBookTextStyle(
    bool Bold = false,
    bool Italic = false,
    bool Underline = false,
    int FontSize = 11,
    string ColorHex = "#000000");

public sealed record PraiseBookExportOptions(
    bool IncludeSongNumber = false,
    bool IncludeTitle = true,
    bool IncludeCopyright = false,
    bool IncludeBookReference = true,
    bool IncludeUserReference = true,
    bool IncludeKey = false,
    bool IncludeCapo = false,
    bool IncludeTiming = false,
    bool IncludeNotations = false,
    bool IncludeIndex = false,
    bool OneSongPerPage = false,
    int LineSpacing = 0,
    int SongSpacing = 1,
    PraiseBookTextStyle? TitleStyle = null,
    PraiseBookTextStyle? MetadataStyle = null,
    PraiseBookTextStyle? LyricsStyle = null,
    PraiseBookTextStyle? NotationStyle = null)
{
    public static PraiseBookExportOptions Default { get; } = new();
}

public sealed record ExportRequest(
    string DatabasePath,
    string OutputPath,
    ExportFormat Format,
    IReadOnlyList<int> SongIds,
    IReadOnlyList<int> FolderNos,
    PraiseBookExportOptions? PraiseBookOptions = null);

public sealed record ExportReport(
    bool Succeeded,
    string OutputPath,
    ExportFormat Format,
    int ExportedSongs,
    IReadOnlyList<ImportExportIssue> Issues);

public interface IImportExportService
{
    Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath);

    Task<AccessImportSchema> GetAccessSchemaAsync(string sourcePath);

    Task<ImportPreview> PreviewImportAsync(string sourcePath, AccessImportMapping? accessMapping = null);

    Task<ImportReport> ImportAsync(ImportRequest request);

    Task<IReadOnlyList<ExportSongCandidate>> GetExportCandidatesAsync(
        string databasePath,
        IReadOnlyList<int> folderNos,
        DateOnly? modifiedFrom = null,
        DateOnly? modifiedTo = null);

    Task<ExportReport> ExportAsync(ExportRequest request);

    string GetDefaultExportPath(string workingFolder, DateOnly date, ExportFormat format);
}

public interface IDocumentTextExtractor
{
    string ExtractText(string path);
}

public sealed class OfficeDocumentTextExtractor : IDocumentTextExtractor
{
    public string ExtractText(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return "";
        }

        return NormalizeLineEndings(new WordDoc().GetContents(path)).TrimEnd('\n', '\r');
    }

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}

public sealed class ImportExportService : IImportExportService
{
    private const string DefaultSourceFolder = "Default Folder";
    private const string DocumentSourceFolder = "Documents";

    private readonly IAdminDatabaseRepository _adminDatabase;
    private readonly IAdminSongDetailRepository _songDetails;
    private readonly IDocumentTextExtractor _documentTextExtractor;

    public ImportExportService(IAdminDatabaseRepository adminDatabase, IAdminSongDetailRepository songDetails)
        : this(adminDatabase, songDetails, new OfficeDocumentTextExtractor())
    {
    }

    public ImportExportService(
        IAdminDatabaseRepository adminDatabase,
        IAdminSongDetailRepository songDetails,
        IDocumentTextExtractor documentTextExtractor)
    {
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        _songDetails = songDetails ?? throw new ArgumentNullException(nameof(songDetails));
        _documentTextExtractor = documentTextExtractor ?? throw new ArgumentNullException(nameof(documentTextExtractor));
    }

    public Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath)
        => _adminDatabase.GetSongFoldersAsync(databasePath);

    public Task<AccessImportSchema> GetAccessSchemaAsync(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return Task.FromResult(new AccessImportSchema(
                false,
                sourcePath,
                [],
                null,
                [new ImportExportIssue(ImportExportIssueSeverity.Error, "Source path is empty.")]));
        }

        var fullPath = Path.GetFullPath(sourcePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult(new AccessImportSchema(
                false,
                fullPath,
                [],
                null,
                [new ImportExportIssue(ImportExportIssueSeverity.Error, "Source file does not exist.")]));
        }

        try
        {
            using var connection = new SQLiteConnection($"Data Source={fullPath};Version=3;");
            connection.Open();
            return Task.FromResult(ReadAccessSchema(connection, fullPath));
        }
        catch (Exception ex) when (ex is SQLiteException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            return Task.FromResult(new AccessImportSchema(
                false,
                fullPath,
                [],
                null,
                [new ImportExportIssue(ImportExportIssueSeverity.Error, ex.Message)]));
        }
    }

    public Task<ImportPreview> PreviewImportAsync(string sourcePath, AccessImportMapping? accessMapping = null)
    {
        var loaded = LoadImportSongs(sourcePath, accessMapping);
        var folders = loaded.Songs
            .GroupBy(song => song.SourceFolder, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ImportSourceFolder(group.Key, group.Count()))
            .OrderBy(folder => folder.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(new ImportPreview(
            loaded.Succeeded,
            sourcePath,
            loaded.Kind,
            loaded.Songs.Count,
            folders,
            loaded.Issues));
    }

    public async Task<ImportReport> ImportAsync(ImportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var loaded = LoadImportSongs(request.SourcePath, request.AccessMapping);
        if (!loaded.Succeeded)
        {
            return new ImportReport(false, request.SourcePath, 0, 0, 0, 0, [], loaded.Issues);
        }

        var selectedFolders = new HashSet<string>(
            request.SelectedSourceFolders.Where(folder => !string.IsNullOrWhiteSpace(folder)),
            StringComparer.OrdinalIgnoreCase);
        var sourceSongs = loaded.Songs
            .Where(song => selectedFolders.Count == 0 || selectedFolders.Contains(song.SourceFolder))
            .ToArray();

        var existing = (await _adminDatabase.GetSongsAsync(request.DatabasePath, request.TargetFolderNo).ConfigureAwait(false))
            .ToList();
        var items = new List<ImportResultItem>();
        var issues = new List<ImportExportIssue>(loaded.Issues);
        var inserted = 0;
        var replaced = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var source in sourceSongs)
        {
            if (string.IsNullOrWhiteSpace(source.Song.Title))
            {
                failed++;
                items.Add(new ImportResultItem("", ImportResultKind.Failed, "Title is empty."));
                continue;
            }

            var duplicate = existing.FirstOrDefault(song =>
                string.Equals(song.Title, source.Song.Title, StringComparison.OrdinalIgnoreCase));
            if (duplicate is not null && request.DuplicatePolicy == ImportDuplicatePolicy.SkipExisting)
            {
                skipped++;
                items.Add(new ImportResultItem(source.Song.Title, ImportResultKind.Skipped, "Existing item was kept."));
                continue;
            }

            var songId = duplicate is not null && request.DuplicatePolicy == ImportDuplicatePolicy.ReplaceExisting
                ? duplicate.SongId
                : (int?)null;
            var writeModel = source.Song with
            {
                SongId = songId,
                FolderNo = request.TargetFolderNo,
                Lyrics = NormalizeLineEndings(source.Song.Lyrics).TrimStart('\n'),
            };

            var report = await _adminDatabase
                .SaveSongAsync(request.DatabasePath, request.BackupRoot, writeModel)
                .ConfigureAwait(false);
            if (!report.Succeeded)
            {
                failed++;
                issues.AddRange(report.Issues.Select(issue => new ImportExportIssue(
                    issue.Severity == AdminDatabaseIssueSeverity.Error
                        ? ImportExportIssueSeverity.Error
                        : ImportExportIssueSeverity.Warning,
                    issue.Message)));
                items.Add(new ImportResultItem(source.Song.Title, ImportResultKind.Failed, "Database write failed."));
                continue;
            }

            if (songId is null)
            {
                inserted++;
                items.Add(new ImportResultItem(source.Song.Title, ImportResultKind.Inserted, "Imported as a new item."));
                var newId = report.AffectedSongIds.FirstOrDefault();
                if (newId > 0)
                {
                    existing.Add(new SongSummary(
                        newId,
                        writeModel.Title,
                        writeModel.AlternateTitle,
                        writeModel.FolderNo,
                        writeModel.SongNumber,
                        writeModel.Category,
                        writeModel.Key,
                        writeModel.Lyrics));
                }
            }
            else
            {
                replaced++;
                items.Add(new ImportResultItem(source.Song.Title, ImportResultKind.Replaced, "Existing item was replaced."));
            }
        }

        return new ImportReport(failed == 0, request.SourcePath, inserted, replaced, skipped, failed, items, issues);
    }

    public async Task<IReadOnlyList<ExportSongCandidate>> GetExportCandidatesAsync(
        string databasePath,
        IReadOnlyList<int> folderNos,
        DateOnly? modifiedFrom = null,
        DateOnly? modifiedTo = null)
    {
        ArgumentNullException.ThrowIfNull(folderNos);

        var folders = await _adminDatabase.GetSongFoldersAsync(databasePath).ConfigureAwait(false);
        var folderNames = folders
            .GroupBy(folder => folder.FolderNo)
            .ToDictionary(group => group.Key, group => group.First().Name);
        var selectedFolders = folderNos.Count == 0
            ? folders.Select(folder => folder.FolderNo).ToHashSet()
            : folderNos.ToHashSet();
        var songs = await _adminDatabase.GetSongsAsync(databasePath).ConfigureAwait(false);

        return songs
            .Where(song => selectedFolders.Contains(song.FolderNo))
            .OrderBy(song => song.FolderNo)
            .ThenBy(song => song.SongNumber)
            .ThenBy(song => song.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(song => song.SongId)
            .Select(song => new ExportSongCandidate(
                song.SongId,
                song.Title,
                song.FolderNo,
                folderNames.TryGetValue(song.FolderNo, out var folderName) ? folderName : "",
                song.SongNumber))
            .ToArray();
    }

    public async Task<ExportReport> ExportAsync(ExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SongIds.Count == 0)
        {
            return new ExportReport(false, request.OutputPath, request.Format, 0, [
                new ImportExportIssue(ImportExportIssueSeverity.Error, "No songs were selected for export."),
            ]);
        }

        var folders = await _adminDatabase.GetSongFoldersAsync(request.DatabasePath).ConfigureAwait(false);
        var folderNames = folders
            .GroupBy(folder => folder.FolderNo)
            .ToDictionary(group => group.Key, group => group.First().Name);
        var exportItems = new List<ExportSong>();
        var issues = new List<ImportExportIssue>();

        foreach (var songId in request.SongIds.Distinct())
        {
            var detail = await _songDetails.GetSongDetailAsync(request.DatabasePath, songId).ConfigureAwait(false);
            if (detail is null)
            {
                issues.Add(new ImportExportIssue(ImportExportIssueSeverity.Warning, $"Song {songId} was not found."));
                continue;
            }

            exportItems.Add(new ExportSong(
                detail,
                folderNames.TryGetValue(detail.FolderNo, out var folderName) ? folderName : DefaultSourceFolder));
        }

        if (exportItems.Count == 0)
        {
            return new ExportReport(false, request.OutputPath, request.Format, 0, issues);
        }

        Directory.CreateDirectory(GetOutputRoot(request.OutputPath, request.Format));
        try
        {
            switch (request.Format)
            {
                case ExportFormat.EasiSlidesText:
                    WriteLegacyText(request.OutputPath, exportItems);
                    break;
                case ExportFormat.EasiSlidesDatabase:
                    WriteDatabase(request.OutputPath, exportItems, folders);
                    break;
                case ExportFormat.Html:
                    WriteHtml(request.OutputPath, exportItems, request.PraiseBookOptions);
                    break;
                case ExportFormat.Rtf:
                    WriteRtf(request.OutputPath, exportItems, request.PraiseBookOptions);
                    break;
                default:
                    WriteXml(request.OutputPath, exportItems);
                    break;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SQLiteException or InvalidOperationException)
        {
            issues.Add(new ImportExportIssue(ImportExportIssueSeverity.Error, ex.Message));
            return new ExportReport(false, request.OutputPath, request.Format, 0, issues);
        }

        return new ExportReport(true, request.OutputPath, request.Format, exportItems.Count, issues);
    }

    public string GetDefaultExportPath(string workingFolder, DateOnly date, ExportFormat format)
    {
        var root = string.IsNullOrWhiteSpace(workingFolder)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Path.Combine(Path.GetFullPath(workingFolder), "Documents");
        var extension = format switch
        {
            ExportFormat.EasiSlidesText => ".esn",
            ExportFormat.EasiSlidesDatabase => ".esf",
            ExportFormat.Rtf => ".rtf",
            ExportFormat.Html => "",
            _ => ".xml",
        };
        var path = format == ExportFormat.Html
            ? Path.Combine(root, $"Html_{date:yyyy-MM-dd}")
            : Path.Combine(root, $"Export_{date:yyyy-MM-dd}{extension}");

        if (format == ExportFormat.Html)
        {
            return GetAvailableDirectory(path);
        }

        return GetAvailableFile(path);
    }

    private LoadedImportSongs LoadImportSongs(string sourcePath, AccessImportMapping? accessMapping = null)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return Failed(ImportSourceKind.Unknown, sourcePath, "Source path is empty.");
        }

        var fullPath = Path.GetFullPath(sourcePath);
        if (Directory.Exists(fullPath))
        {
            return LoadDocumentFolder(fullPath);
        }

        if (!File.Exists(fullPath))
        {
            return Failed(ImportSourceKind.Unknown, fullPath, "Source file does not exist.");
        }

        return Path.GetExtension(fullPath).ToLowerInvariant() switch
        {
            ".esn" or ".est" => LoadLegacyText(fullPath),
            ".xml" => LoadXml(fullPath),
            ".esf" => LoadDatabase(fullPath),
            ".mdb" => LoadAccessDatabase(fullPath, accessMapping),
            _ => Failed(ImportSourceKind.Unknown, fullPath, "Unsupported import source format."),
        };
    }

    private static LoadedImportSongs LoadLegacyText(string path)
    {
        var text = NormalizeLineEndings(File.ReadAllText(path));
        if (text.Length < 8)
        {
            return Failed(ImportSourceKind.EasiSlidesText, path, "Text import file is too short.");
        }

        var header = text[..8].ToLowerInvariant();
        var separator = header switch
        {
            "[est3.1]" => '>',
            "[esf1.0]" => '#',
            _ => '\0',
        };
        if (separator == '\0')
        {
            return Failed(ImportSourceKind.EasiSlidesText, path, "Text import header is not an EasiSlides header.");
        }

        var marker = "[" + separator;
        var body = text[8..].Replace(marker, "\u0001", StringComparison.Ordinal);
        var songs = new List<ImportSong>();
        foreach (var segment in body.Split('\u0001'))
        {
            var close = segment.IndexOf(']');
            if (close <= 0)
            {
                continue;
            }

            var song = ParseLegacyTextSong(segment[..close], segment[(close + 1)..], separator);
            songs.Add(new ImportSong(ImportSourceKind.EasiSlidesText, song.SourceFolder, song.Song));
        }

        return new LoadedImportSongs(true, ImportSourceKind.EasiSlidesText, songs, []);
    }

    private static (string SourceFolder, SongWriteModel Song) ParseLegacyTextSong(string itemHeader, string lyrics, char separator)
    {
        var firstSeparator = itemHeader.IndexOf(separator);
        var title = firstSeparator > 0 ? itemHeader[..firstSeparator] : itemHeader;
        var fields = firstSeparator > 0 ? itemHeader[(firstSeparator + 1)..] : "";
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (fields.Length > 0)
        {
            var next = fields.IndexOf(separator, 1);
            var field = next > 0 ? fields[..next] : fields;
            fields = next > 0 ? fields[(next + 1)..] : "";
            if (field.Length >= 2)
            {
                values[separator + field[0].ToString()] = field[1..];
            }
        }

        var normalizedLyrics = NormalizeLineEndings(lyrics).TrimStart('\n', '\r');
        var notations = "";
        if (normalizedLyrics.StartsWith("[~", StringComparison.Ordinal))
        {
            var notationEnd = normalizedLyrics.IndexOf(']');
            if (notationEnd > 1)
            {
                notations = normalizedLyrics[2..notationEnd];
                normalizedLyrics = normalizedLyrics[(notationEnd + 1)..].TrimStart('\n', '\r');
            }
        }

        var sourceFolder = Value(values, separator, "f", DefaultSourceFolder);
        return (sourceFolder, new SongWriteModel(
            SongId: null,
            Title: title,
            AlternateTitle: Value(values, separator, separator.ToString()),
            FolderNo: 0,
            SongNumber: ToInt(Value(values, separator, "n")),
            Lyrics: NormalizeLineEndings(normalizedLyrics).TrimEnd('\n', '\r'),
            Sequence: Value(values, separator, "@"),
            Writer: Value(values, separator, "w"),
            Copyright: Value(values, separator, "c"),
            Capo: ToInt(Value(values, separator, "0"), defaultValue: -1),
            Timing: Value(values, separator, "t"),
            Key: Value(values, separator, "k"),
            Notations: notations,
            LicenceAdmin1: Value(values, separator, "a"),
            LicenceAdmin2: Value(values, separator, "b"),
            BookReference: Value(values, separator, "r"),
            UserReference: Value(values, separator, "u"),
            FormatData: Value(values, separator, "q")));
    }

    private static LoadedImportSongs LoadXml(string path)
    {
        var document = XDocument.Load(path);
        if (!string.Equals(document.Root?.Name.LocalName, "EasiSlides", StringComparison.Ordinal))
        {
            return Failed(ImportSourceKind.EasiSlidesXml, path, "XML root is not EasiSlides.");
        }

        var songs = document.Root!
            .Elements("Item")
            .Select(element =>
            {
                var folder = Text(element, "Folder", DefaultSourceFolder);
                var song = new SongWriteModel(
                    SongId: null,
                    Title: Text(element, "Title1"),
                    AlternateTitle: Text(element, "Title2"),
                    FolderNo: 0,
                    SongNumber: ToInt(Text(element, "SongNumber")),
                    Lyrics: NormalizeLineEndings(Text(element, "Contents")).TrimEnd('\n', '\r'),
                    Sequence: Text(element, "Sequence"),
                    Writer: Text(element, "Writer"),
                    Copyright: Text(element, "Copyright"),
                    Capo: ToInt(Text(element, "Capo"), defaultValue: -1),
                    Timing: Text(element, "Timing"),
                    Key: Text(element, "MusicKey"),
                    Notations: Text(element, "Notations"),
                    Category: Text(element, "Category"),
                    LicenceAdmin1: Text(element, "LicenceAdmin1"),
                    LicenceAdmin2: Text(element, "LicenceAdmin2"),
                    BookReference: Text(element, "BookReference"),
                    UserReference: Text(element, "UserReference"),
                    Settings: Text(element, "Settings"),
                    FormatData: Text(element, "FormatData"));
                return new ImportSong(ImportSourceKind.EasiSlidesXml, folder, song);
            })
            .ToArray();

        return new LoadedImportSongs(true, ImportSourceKind.EasiSlidesXml, songs, []);
    }

    private static LoadedImportSongs LoadDatabase(string path)
    {
        var songs = new List<ImportSong>();
        var folderNames = new Dictionary<int, string>();
        using var connection = new SQLiteConnection($"Data Source={path};Version=3;");
        connection.Open();

        using (var command = new SQLiteCommand("SELECT FolderNo, Name FROM FOLDER WHERE FolderNo > 0 ORDER BY FolderNo;", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                folderNames[reader.GetInt32(0)] = reader.IsDBNull(1) ? DefaultSourceFolder : reader.GetString(1);
            }
        }

        using (var command = new SQLiteCommand("SELECT * FROM SONG;", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var folderNo = GetInt(reader, "FOLDERNO");
                var folder = folderNames.TryGetValue(folderNo, out var folderName) ? folderName : DefaultSourceFolder;
                songs.Add(new ImportSong(
                    ImportSourceKind.EasiSlidesDatabase,
                    folder,
                    new SongWriteModel(
                        SongId: null,
                        Title: GetString(reader, "TITLE_1"),
                        AlternateTitle: GetString(reader, "TITLE_2"),
                        FolderNo: 0,
                        SongNumber: GetInt(reader, "SONG_NUMBER"),
                        Lyrics: NormalizeLineEndings(GetString(reader, "LYRICS")).TrimEnd('\n', '\r'),
                        Sequence: GetString(reader, "SEQUENCE"),
                        Writer: GetString(reader, "WRITER"),
                        Copyright: GetString(reader, "COPYRIGHT"),
                        Capo: GetInt(reader, "CAPO", -1),
                        Timing: GetString(reader, "TIMING"),
                        Key: GetString(reader, "KEY"),
                        Notations: GetString(reader, "MSC"),
                        Category: GetString(reader, "CATEGORY"),
                        LicenceAdmin1: GetString(reader, "LICENCE_ADMIN1"),
                        LicenceAdmin2: GetString(reader, "LICENCE_ADMIN2"),
                        BookReference: GetString(reader, "BOOK_REFERENCE"),
                        UserReference: GetString(reader, "USER_REFERENCE"),
                        Settings: GetString(reader, "SETTINGS"),
                        FormatData: GetString(reader, "FORMATDATA"))));
            }
        }

        return new LoadedImportSongs(true, ImportSourceKind.EasiSlidesDatabase, songs, []);
    }

    private static LoadedImportSongs LoadAccessDatabase(string path, AccessImportMapping? accessMapping)
    {
        try
        {
            using var connection = new SQLiteConnection($"Data Source={path};Version=3;");
            connection.Open();
            var schema = ReadAccessSchema(connection, path);
            if (!schema.Succeeded)
            {
                return new LoadedImportSongs(false, ImportSourceKind.AccessDatabase, [], schema.Issues);
            }

            var mapping = accessMapping ?? schema.SuggestedMapping;
            if (mapping is null)
            {
                return Failed(ImportSourceKind.AccessDatabase, path, "Access MDB helper could not infer a table mapping.");
            }

            var table = schema.Tables.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, mapping.TableName, StringComparison.OrdinalIgnoreCase));
            if (table is null)
            {
                return Failed(ImportSourceKind.AccessDatabase, path, $"Access table '{mapping.TableName}' was not found.");
            }

            var issues = ValidateAccessMapping(table, mapping);
            if (issues.Count > 0)
            {
                return new LoadedImportSongs(false, ImportSourceKind.AccessDatabase, [], issues);
            }

            var songs = new List<ImportSong>();
            using var command = new SQLiteCommand($"SELECT * FROM {QuoteSqlIdentifier(table.Name)};", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(new ImportSong(
                    ImportSourceKind.AccessDatabase,
                    table.Name,
                    new SongWriteModel(
                        SongId: null,
                        Title: GetString(reader, mapping.TitleColumn),
                        AlternateTitle: GetOptionalString(reader, mapping.AlternateTitleColumn),
                        FolderNo: 0,
                        SongNumber: GetOptionalInt(reader, mapping.SongNumberColumn),
                        Lyrics: MergeAccessLyrics(reader, mapping.LyricsColumns),
                        Sequence: GetOptionalString(reader, "Sequence"),
                        Writer: GetOptionalString(reader, mapping.WriterColumn),
                        Copyright: GetOptionalString(reader, mapping.CopyrightColumn),
                        Capo: GetOptionalInt(reader, "capo", -1),
                        Timing: GetOptionalString(reader, mapping.TimingColumn),
                        Key: GetOptionalString(reader, mapping.KeyColumn),
                        Notations: GetOptionalString(reader, "msc"),
                        LicenceAdmin1: GetOptionalString(reader, mapping.LicenceAdmin1Column),
                        LicenceAdmin2: GetOptionalString(reader, mapping.LicenceAdmin2Column),
                        BookReference: GetOptionalString(reader, mapping.BookReferenceColumn),
                        UserReference: GetOptionalString(reader, mapping.UserReferenceColumn))));
            }

            return new LoadedImportSongs(true, ImportSourceKind.AccessDatabase, songs, []);
        }
        catch (Exception ex) when (ex is SQLiteException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            return Failed(ImportSourceKind.AccessDatabase, path, ex.Message);
        }
    }

    private static AccessImportSchema ReadAccessSchema(SQLiteConnection connection, string path)
    {
        var tables = new List<AccessImportTable>();
        using (var command = new SQLiteCommand(
                   "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name COLLATE NOCASE;",
                   connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                if (!string.IsNullOrWhiteSpace(name) &&
                    !name.StartsWith("msys", StringComparison.OrdinalIgnoreCase))
                {
                    tables.Add(new AccessImportTable(name, [], 0));
                }
            }
        }

        var hydrated = tables
            .Select(table => table with
            {
                Columns = GetAccessColumns(connection, table.Name),
                RowCount = GetAccessRowCount(connection, table.Name),
            })
            .ToArray();

        var suggested = BuildSuggestedAccessMapping(hydrated);
        IReadOnlyList<ImportExportIssue> issues = hydrated.Length == 0
            ? [new ImportExportIssue(ImportExportIssueSeverity.Error, "Access MDB file does not contain importable tables.")]
            : [];
        return new AccessImportSchema(hydrated.Length > 0, path, hydrated, suggested, issues);
    }

    private static IReadOnlyList<string> GetAccessColumns(SQLiteConnection connection, string tableName)
    {
        var columns = new List<string>();
        using var command = new SQLiteCommand($"PRAGMA table_info({QuoteSqlLiteral(tableName)});", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
            if (!string.IsNullOrWhiteSpace(name))
            {
                columns.Add(name);
            }
        }

        return columns;
    }

    private static int GetAccessRowCount(SQLiteConnection connection, string tableName)
    {
        using var command = new SQLiteCommand($"SELECT COUNT(*) FROM {QuoteSqlIdentifier(tableName)};", connection);
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static AccessImportMapping? BuildSuggestedAccessMapping(IReadOnlyList<AccessImportTable> tables)
    {
        var table = tables.FirstOrDefault(candidate => string.Equals(candidate.Name, "SONG", StringComparison.OrdinalIgnoreCase))
            ?? tables.FirstOrDefault();
        if (table is null)
        {
            return null;
        }

        var title = PickColumn(table.Columns, "TITLE_1", "Title_1", "Title", "Name");
        var lyrics = PickColumn(table.Columns, "Lyrics", "LYRICS", "Contents", "Body", "Verse1");
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(lyrics))
        {
            return null;
        }

        return new AccessImportMapping(
            table.Name,
            title,
            [lyrics],
            AlternateTitleColumn: PickColumn(table.Columns, "TITLE_2", "Title_2", "Title2", "AltName", "AlternateTitle"),
            SongNumberColumn: PickColumn(table.Columns, "SONG_NUMBER", "SongNumber", "Number", "No"),
            WriterColumn: PickColumn(table.Columns, "writer", "WRITER", "Writer", "Author"),
            CopyrightColumn: PickColumn(table.Columns, "copyright", "COPYRIGHT", "Copyright", "Rights"),
            KeyColumn: PickColumn(table.Columns, "key", "KEY", "MusicKey"),
            TimingColumn: PickColumn(table.Columns, "Timing", "TIMING", "Tempo"),
            BookReferenceColumn: PickColumn(table.Columns, "BOOK_REFERENCE", "BookReference", "BookRef"),
            UserReferenceColumn: PickColumn(table.Columns, "USER_REFERENCE", "UserReference", "UserRef"),
            LicenceAdmin1Column: PickColumn(table.Columns, "LICENCE_ADMIN1", "LicenceAdmin1", "Admin1", "AdminA"),
            LicenceAdmin2Column: PickColumn(table.Columns, "LICENCE_ADMIN2", "LicenceAdmin2", "Admin2", "AdminB"));
    }

    private static IReadOnlyList<ImportExportIssue> ValidateAccessMapping(AccessImportTable table, AccessImportMapping mapping)
    {
        var issues = new List<ImportExportIssue>();
        if (!ColumnExists(table.Columns, mapping.TitleColumn))
        {
            issues.Add(new ImportExportIssue(ImportExportIssueSeverity.Error, "Access title column is not selected or does not exist."));
        }

        if (mapping.LyricsColumns.Count == 0 || mapping.LyricsColumns.Any(column => !ColumnExists(table.Columns, column)))
        {
            issues.Add(new ImportExportIssue(ImportExportIssueSeverity.Error, "Access lyrics merge columns are not selected or do not exist."));
        }

        foreach (var column in new[]
                 {
                     mapping.AlternateTitleColumn,
                     mapping.SongNumberColumn,
                     mapping.WriterColumn,
                     mapping.CopyrightColumn,
                     mapping.KeyColumn,
                     mapping.TimingColumn,
                     mapping.BookReferenceColumn,
                     mapping.UserReferenceColumn,
                     mapping.LicenceAdmin1Column,
                     mapping.LicenceAdmin2Column,
                 })
        {
            if (!string.IsNullOrWhiteSpace(column) && !ColumnExists(table.Columns, column))
            {
                issues.Add(new ImportExportIssue(ImportExportIssueSeverity.Error, $"Access column '{column}' does not exist."));
            }
        }

        return issues;
    }

    private LoadedImportSongs LoadDocumentFolder(string sourceFolder)
    {
        var files = Directory
            .EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
            .Where(path => IsDocumentImportFile(path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var songs = files.Select(path => new ImportSong(
            ImportSourceKind.DocumentFolder,
            DocumentSourceFolder,
            new SongWriteModel(
                SongId: null,
                Title: Path.GetFileNameWithoutExtension(path),
                AlternateTitle: "",
                FolderNo: 0,
                SongNumber: 0,
                Lyrics: ExtractDocumentText(path).TrimEnd('\n', '\r')))).ToArray();

        return new LoadedImportSongs(true, ImportSourceKind.DocumentFolder, songs, []);
    }

    private static bool IsDocumentImportFile(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".txt" or ".doc" or ".docx";
    }

    private string ExtractDocumentText(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".doc" => NormalizeLineEndings(_documentTextExtractor.ExtractText(path)),
            ".docx" => ExtractDocxText(path),
            _ => NormalizeLineEndings(File.ReadAllText(path)),
        };
    }

    private static string ExtractDocxText(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var entry = archive.GetEntry("word/document.xml");
        if (entry is null)
        {
            return "";
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        return string.Join("\n", document.Descendants(w + "p")
            .Select(paragraph => string.Concat(paragraph.Descendants(w + "t").Select(text => text.Value)))
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static void WriteXml(string outputPath, IReadOnlyList<ExportSong> songs)
    {
        var document = new XDocument(
            new XElement("EasiSlides", songs.Select(song => new XElement("Item",
                new XElement("Title1", song.Detail.Title),
                new XElement("Title2", song.Detail.AlternateTitle),
                new XElement("Folder", song.FolderName),
                new XElement("SongNumber", song.Detail.SongNumber.ToString(CultureInfo.InvariantCulture)),
                new XElement("Contents", NormalizeLineEndings(song.Detail.Lyrics).Replace("\n", "\r\n", StringComparison.Ordinal)),
                new XElement("Notations", song.Detail.Notations),
                new XElement("Sequence", song.Detail.Sequence),
                new XElement("Writer", song.Detail.Writer),
                new XElement("Copyright", song.Detail.Copyright),
                new XElement("Category", song.Detail.Category),
                new XElement("Timing", song.Detail.Timing),
                new XElement("MusicKey", song.Detail.Key),
                new XElement("Capo", song.Detail.Capo.ToString(CultureInfo.InvariantCulture)),
                new XElement("LicenceAdmin1", song.Detail.LicenceAdmin1),
                new XElement("LicenceAdmin2", song.Detail.LicenceAdmin2),
                new XElement("BookReference", song.Detail.BookReference),
                new XElement("UserReference", song.Detail.UserReference),
                new XElement("FormatData", song.Detail.FormatData),
                new XElement("Settings", song.Detail.Settings)))));
        document.Save(outputPath);
    }

    private static void WriteLegacyText(string outputPath, IReadOnlyList<ExportSong> songs)
    {
        var builder = new StringBuilder("[est3.1]");
        foreach (var song in songs)
        {
            builder.Append("\r\n[>").Append(song.Detail.Title);
            AppendTextField(builder, ">>", song.Detail.AlternateTitle);
            AppendTextField(builder, ">f", song.FolderName);
            AppendNumberField(builder, ">n", song.Detail.SongNumber, greaterThan: 0);
            AppendTextField(builder, ">r", song.Detail.BookReference);
            AppendTextField(builder, ">u", song.Detail.UserReference);
            AppendTextField(builder, ">c", song.Detail.Copyright);
            AppendTextField(builder, ">w", song.Detail.Writer);
            AppendTextField(builder, ">k", song.Detail.Key);
            AppendTextField(builder, ">t", song.Detail.Timing);
            AppendNumberField(builder, ">0", song.Detail.Capo, greaterThan: -1);
            AppendTextField(builder, ">a", song.Detail.LicenceAdmin1);
            AppendTextField(builder, ">b", song.Detail.LicenceAdmin2);
            AppendTextField(builder, ">@", song.Detail.Sequence);
            builder.Append(']');
            if (!string.IsNullOrWhiteSpace(song.Detail.Notations))
            {
                builder.Append("\r\n[~").Append(song.Detail.Notations).Append(']');
            }

            builder.Append("\r\n").Append(NormalizeLineEndings(song.Detail.Lyrics).Replace("\n", "\r\n", StringComparison.Ordinal));
        }

        File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);
    }

    private static void WriteDatabase(string outputPath, IReadOnlyList<ExportSong> songs, IReadOnlyList<SongFolderSummary> folders)
    {
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        using var connection = new SQLiteConnection($"Data Source={outputPath};Version=3;");
        connection.Open();
        Execute(connection, "CREATE TABLE FOLDER (FolderNo INTEGER PRIMARY KEY, Name TEXT, Use INTEGER);");
        Execute(connection, """
            CREATE TABLE SONG (
                SONGID INTEGER PRIMARY KEY AUTOINCREMENT,
                TITLE_1 TEXT,
                TITLE_2 TEXT,
                FOLDERNO INTEGER,
                SONG_NUMBER INTEGER,
                LYRICS TEXT,
                SEQUENCE TEXT,
                WRITER TEXT,
                COPYRIGHT TEXT,
                CAPO INTEGER,
                TIMING TEXT,
                "KEY" TEXT,
                MSC TEXT,
                CATEGORY TEXT,
                LICENCE_ADMIN1 TEXT,
                LICENCE_ADMIN2 TEXT,
                BOOK_REFERENCE TEXT,
                USER_REFERENCE TEXT,
                SETTINGS TEXT,
                FORMATDATA TEXT,
                LastModified TEXT
            );
            """);

        var folderNos = songs.Select(song => song.Detail.FolderNo).Distinct().ToHashSet();
        foreach (var folder in folders
            .Where(folder => folderNos.Contains(folder.FolderNo))
            .GroupBy(folder => folder.FolderNo)
            .Select(group => group.First()))
        {
            using var command = new SQLiteCommand("INSERT INTO FOLDER (FolderNo, Name, Use) VALUES (@no, @name, @use);", connection);
            command.Parameters.AddWithValue("@no", folder.FolderNo);
            command.Parameters.AddWithValue("@name", folder.Name);
            command.Parameters.AddWithValue("@use", folder.IsEnabled ? 1 : 0);
            command.ExecuteNonQuery();
        }

        foreach (var song in songs)
        {
            using var command = new SQLiteCommand("""
                INSERT INTO SONG (
                    TITLE_1, TITLE_2, FOLDERNO, SONG_NUMBER, LYRICS, SEQUENCE, WRITER, COPYRIGHT,
                    CAPO, TIMING, "KEY", MSC, CATEGORY, LICENCE_ADMIN1, LICENCE_ADMIN2,
                    BOOK_REFERENCE, USER_REFERENCE, SETTINGS, FORMATDATA, LastModified)
                VALUES (
                    @title1, @title2, @folder, @number, @lyrics, @sequence, @writer, @copyright,
                    @capo, @timing, @key, @notations, @category, @admin1, @admin2,
                    @book, @user, @settings, @format, @modified);
                """, connection);
            command.Parameters.AddWithValue("@title1", song.Detail.Title);
            command.Parameters.AddWithValue("@title2", song.Detail.AlternateTitle);
            command.Parameters.AddWithValue("@folder", song.Detail.FolderNo);
            command.Parameters.AddWithValue("@number", song.Detail.SongNumber);
            command.Parameters.AddWithValue("@lyrics", NormalizeLineEndings(song.Detail.Lyrics));
            command.Parameters.AddWithValue("@sequence", song.Detail.Sequence);
            command.Parameters.AddWithValue("@writer", song.Detail.Writer);
            command.Parameters.AddWithValue("@copyright", song.Detail.Copyright);
            command.Parameters.AddWithValue("@capo", song.Detail.Capo);
            command.Parameters.AddWithValue("@timing", song.Detail.Timing);
            command.Parameters.AddWithValue("@key", song.Detail.Key);
            command.Parameters.AddWithValue("@notations", song.Detail.Notations);
            command.Parameters.AddWithValue("@category", song.Detail.Category);
            command.Parameters.AddWithValue("@admin1", song.Detail.LicenceAdmin1);
            command.Parameters.AddWithValue("@admin2", song.Detail.LicenceAdmin2);
            command.Parameters.AddWithValue("@book", song.Detail.BookReference);
            command.Parameters.AddWithValue("@user", song.Detail.UserReference);
            command.Parameters.AddWithValue("@settings", song.Detail.Settings);
            command.Parameters.AddWithValue("@format", song.Detail.FormatData);
            command.Parameters.AddWithValue("@modified", DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.ExecuteNonQuery();
        }
    }

    private static void WriteHtml(
        string outputDirectory,
        IReadOnlyList<ExportSong> songs,
        PraiseBookExportOptions? praiseBookOptions)
    {
        Directory.CreateDirectory(outputDirectory);
        var options = praiseBookOptions ?? PraiseBookExportOptions.Default;
        var styles = GetPraiseBookStyles(options);
        var index = new StringBuilder("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>EasiSlides Export</title>")
            .Append("<style>")
            .Append(".song-title{").Append(BuildCssStyle(styles.Title)).Append("}")
            .Append(".song-meta{").Append(BuildCssStyle(styles.Metadata)).Append("}")
            .Append(".song-lyrics{").Append(BuildCssStyle(styles.Lyrics)).Append("white-space:pre-wrap;}")
            .Append(".song-notations{").Append(BuildCssStyle(styles.Notation)).Append("white-space:pre-wrap;}")
            .Append(".song.page-break{page-break-after: always; break-after: page;}")
            .Append("</style></head><body>");
        index.Append(options.IncludeIndex ? "<section class=\"book-index\"><h1>Index</h1><ol>" : "<ul>");

        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var song in songs)
        {
            var fileName = GetUniqueHtmlFileName(song.Detail.Title, usedNames);
            var indexText = BuildPraiseBookHeading(song, options);
            if (string.IsNullOrWhiteSpace(indexText))
            {
                indexText = song.Detail.Title;
            }

            index.Append("<li><a href=\"").Append(WebUtility.HtmlEncode(fileName)).Append("\">")
                .Append(WebUtility.HtmlEncode(indexText)).Append("</a></li>");

            var body = new StringBuilder("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>")
                .Append(WebUtility.HtmlEncode(song.Detail.Title))
                .Append("</title><style>")
                .Append(".song-title{").Append(BuildCssStyle(styles.Title)).Append("}")
                .Append(".song-meta{").Append(BuildCssStyle(styles.Metadata)).Append("}")
                .Append(".song-lyrics{").Append(BuildCssStyle(styles.Lyrics)).Append("white-space:pre-wrap;}")
                .Append(".song-notations{").Append(BuildCssStyle(styles.Notation)).Append("white-space:pre-wrap;}")
                .Append(".song.page-break{page-break-after: always; break-after: page;}")
                .Append("</style></head><body><main class=\"song")
                .Append(options.OneSongPerPage ? " page-break" : "")
                .Append("\">");

            var heading = BuildPraiseBookHeading(song, options);
            if (!string.IsNullOrWhiteSpace(heading))
            {
                body.Append("<h1 class=\"song-title\">")
                    .Append(WebUtility.HtmlEncode(heading))
                    .Append("</h1>");
            }

            var metadata = BuildPraiseBookMetadata(song, options);
            if (metadata.Count > 0)
            {
                body.Append("<p class=\"song-meta\">")
                    .Append(WebUtility.HtmlEncode(string.Join(" | ", metadata)))
                    .Append("</p>");
            }

            if (options.IncludeNotations && !string.IsNullOrWhiteSpace(song.Detail.Notations))
            {
                body.Append("<pre class=\"song-notations\">")
                    .Append(WebUtility.HtmlEncode(NormalizeLineEndings(song.Detail.Notations)))
                    .Append("</pre>");
            }

            body.Append("<pre class=\"song-lyrics\">")
                .Append(WebUtility.HtmlEncode(NormalizeLineEndings(song.Detail.Lyrics)))
                .Append("</pre></main></body></html>");
            File.WriteAllText(Path.Combine(outputDirectory, fileName), body.ToString(), Encoding.UTF8);
        }

        index.Append(options.IncludeIndex ? "</ol></section>" : "</ul>");
        index.Append("</body></html>");
        File.WriteAllText(Path.Combine(outputDirectory, "index.htm"), index.ToString(), Encoding.UTF8);
    }

    private static void WriteRtf(
        string outputPath,
        IReadOnlyList<ExportSong> songs,
        PraiseBookExportOptions? praiseBookOptions)
    {
        var options = praiseBookOptions ?? PraiseBookExportOptions.Default;
        var styles = GetPraiseBookStyles(options);
        var colorIndexes = BuildRtfColorIndexes(styles);
        var builder = new StringBuilder(@"{\rtf1\ansi\ansicpg1252\deff0")
            .Append(@"{\fonttbl{\f0\fnil Segoe UI;}}")
            .Append(BuildRtfColorTable(colorIndexes))
            .Append(@"\viewkind1\uc1\pard ");

        for (var index = 0; index < songs.Count; index++)
        {
            var song = songs[index];
            if (index > 0 && options.OneSongPerPage)
            {
                builder.Append(@"\page ");
            }

            var heading = BuildPraiseBookHeading(song, options);
            if (!string.IsNullOrWhiteSpace(heading))
            {
                AppendStyledRtfParagraph(builder, heading, styles.Title, colorIndexes);
            }

            foreach (var metadataLine in BuildPraiseBookMetadata(song, options))
            {
                AppendStyledRtfParagraph(builder, metadataLine, styles.Metadata, colorIndexes);
            }

            if (options.IncludeNotations && !string.IsNullOrWhiteSpace(song.Detail.Notations))
            {
                foreach (var line in SplitNormalizedLines(song.Detail.Notations))
                {
                    AppendStyledRtfParagraph(builder, line, styles.Notation, colorIndexes);
                }
            }

            foreach (var line in SplitNormalizedLines(song.Detail.Lyrics))
            {
                AppendStyledRtfParagraph(builder, line, styles.Lyrics, colorIndexes);
                AppendRtfBlankLines(builder, options.LineSpacing);
            }

            AppendRtfBlankLines(builder, options.SongSpacing);
        }

        if (options.IncludeIndex)
        {
            builder.Append(@"\page ");
            AppendStyledRtfParagraph(builder, "INDEX", styles.Title, colorIndexes);
            foreach (var song in songs)
            {
                var heading = BuildPraiseBookHeading(song, options);
                AppendStyledRtfParagraph(
                    builder,
                    string.IsNullOrWhiteSpace(heading) ? song.Detail.Title : heading,
                    styles.Metadata,
                    colorIndexes);
            }
        }

        builder.Append('}');
        File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);
    }

    private static PraiseBookStyleSet GetPraiseBookStyles(PraiseBookExportOptions options)
        => new(
            NormalizeTextStyle(options.TitleStyle ?? new PraiseBookTextStyle(Bold: true, FontSize: 14, ColorHex: "#000000")),
            NormalizeTextStyle(options.MetadataStyle ?? new PraiseBookTextStyle(Italic: true, FontSize: 10, ColorHex: "#333333")),
            NormalizeTextStyle(options.LyricsStyle ?? new PraiseBookTextStyle(FontSize: 11, ColorHex: "#000000")),
            NormalizeTextStyle(options.NotationStyle ?? new PraiseBookTextStyle(FontSize: 10, ColorHex: "#555555")));

    private static PraiseBookTextStyle NormalizeTextStyle(PraiseBookTextStyle style)
        => style with
        {
            FontSize = Math.Clamp(style.FontSize, 4, 72),
            ColorHex = NormalizeHexColor(style.ColorHex),
        };

    private static string BuildPraiseBookHeading(ExportSong song, PraiseBookExportOptions options)
    {
        var parts = new List<string>(2);
        if (options.IncludeSongNumber && song.Detail.SongNumber > 0)
        {
            parts.Add($"No. {song.Detail.SongNumber.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.IncludeTitle && !string.IsNullOrWhiteSpace(song.Detail.Title))
        {
            parts.Add(song.Detail.Title);
        }

        return string.Join(" ", parts);
    }

    private static IReadOnlyList<string> BuildPraiseBookMetadata(ExportSong song, PraiseBookExportOptions options)
    {
        var metadata = new List<string>();
        AddMetadata(metadata, options.IncludeCopyright, song.Detail.Copyright);
        AddMetadata(metadata, options.IncludeBookReference, song.Detail.BookReference);
        AddMetadata(metadata, options.IncludeUserReference, song.Detail.UserReference);
        AddMetadata(metadata, options.IncludeKey && !string.IsNullOrWhiteSpace(song.Detail.Key), $"Key: {song.Detail.Key}");
        AddMetadata(metadata, options.IncludeCapo && song.Detail.Capo > 0, $"Capo {song.Detail.Capo.ToString(CultureInfo.InvariantCulture)}");
        AddMetadata(metadata, options.IncludeTiming && !string.IsNullOrWhiteSpace(song.Detail.Timing), $"({song.Detail.Timing})");
        return metadata;
    }

    private static void AddMetadata(List<string> metadata, bool include, string value)
    {
        if (include && !string.IsNullOrWhiteSpace(value))
        {
            metadata.Add(value.Trim());
        }
    }

    private static string[] SplitNormalizedLines(string value)
        => NormalizeLineEndings(value)
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .ToArray();

    private static Dictionary<string, RtfColor> BuildRtfColorIndexes(PraiseBookStyleSet styles)
    {
        var colors = new Dictionary<string, RtfColor>(StringComparer.OrdinalIgnoreCase);
        foreach (var style in new[] { styles.Title, styles.Metadata, styles.Lyrics, styles.Notation })
        {
            if (colors.ContainsKey(style.ColorHex))
            {
                continue;
            }

            var (red, green, blue) = ParseHexColor(style.ColorHex);
            colors[style.ColorHex] = new RtfColor(colors.Count + 1, red, green, blue);
        }

        return colors;
    }

    private static string BuildRtfColorTable(IReadOnlyDictionary<string, RtfColor> colorIndexes)
    {
        var builder = new StringBuilder(@"{\colortbl;");
        foreach (var color in colorIndexes.Values.OrderBy(color => color.Index))
        {
            builder.Append(@"\red").Append(color.Red.ToString(CultureInfo.InvariantCulture))
                .Append(@"\green").Append(color.Green.ToString(CultureInfo.InvariantCulture))
                .Append(@"\blue").Append(color.Blue.ToString(CultureInfo.InvariantCulture))
                .Append(';');
        }

        return builder.Append('}').ToString();
    }

    private static void AppendStyledRtfParagraph(
        StringBuilder builder,
        string text,
        PraiseBookTextStyle style,
        IReadOnlyDictionary<string, RtfColor> colorIndexes)
    {
        builder.Append(@"\pard");
        if (style.Bold)
        {
            builder.Append(@"\b");
        }

        if (style.Italic)
        {
            builder.Append(@"\i");
        }

        if (style.Underline)
        {
            builder.Append(@"\ul");
        }

        builder.Append(@"\fs").Append((style.FontSize * 2).ToString(CultureInfo.InvariantCulture));
        if (colorIndexes.TryGetValue(style.ColorHex, out var color))
        {
            builder.Append(@"\cf").Append(color.Index.ToString(CultureInfo.InvariantCulture));
        }

        builder.Append(' ')
            .Append(EscapeRtf(text))
            .Append(@"\b0\i0\ulnone\cf0\par ");
    }

    private static void AppendRtfBlankLines(StringBuilder builder, int count)
    {
        for (var i = 0; i < Math.Clamp(count, 0, 12); i++)
        {
            builder.Append(@"\par ");
        }
    }

    private static string BuildCssStyle(PraiseBookTextStyle style)
    {
        var builder = new StringBuilder()
            .Append("font-size:").Append(style.FontSize.ToString(CultureInfo.InvariantCulture)).Append("pt;")
            .Append("color:").Append(style.ColorHex).Append(';');
        if (style.Bold)
        {
            builder.Append("font-weight:700;");
        }

        if (style.Italic)
        {
            builder.Append("font-style:italic;");
        }

        if (style.Underline)
        {
            builder.Append("text-decoration:underline;");
        }

        return builder.ToString();
    }

    private static string NormalizeHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "#000000";
        }

        var hex = value.Trim();
        if (!hex.StartsWith('#'))
        {
            hex = $"#{hex}";
        }

        if (hex.Length != 7)
        {
            return "#000000";
        }

        return int.TryParse(hex[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
            ? hex.ToUpperInvariant()
            : "#000000";
    }

    private static (int Red, int Green, int Blue) ParseHexColor(string colorHex)
    {
        var hex = NormalizeHexColor(colorHex);
        var red = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var green = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var blue = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return (red, green, blue);
    }

    private static void AppendTextField(StringBuilder builder, string token, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            builder.Append(token).Append(value);
        }
    }

    private static void AppendNumberField(StringBuilder builder, string token, int value, int greaterThan)
    {
        if (value > greaterThan)
        {
            builder.Append(token).Append(value.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static string GetOutputRoot(string outputPath, ExportFormat format)
        => format == ExportFormat.Html
            ? Path.GetFullPath(outputPath)
            : Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? Directory.GetCurrentDirectory();

    private static string GetAvailableFile(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path) ?? "";
        var name = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        for (var i = 1; i < 100; i++)
        {
            var candidate = Path.Combine(directory, $"{name}_{i:00}{extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        return path;
    }

    private static string GetAvailableDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return path;
        }

        for (var i = 1; i < 100; i++)
        {
            var candidate = $"{path}_{i:00}";
            if (!Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return path;
    }

    private static string GetUniqueHtmlFileName(string title, HashSet<string> usedNames)
    {
        var safeName = string.Concat(title.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "Song";
        }

        var fileName = safeName + ".htm";
        for (var i = 1; !usedNames.Add(fileName); i++)
        {
            fileName = $"{safeName}{i}.htm";
        }

        return fileName;
    }

    private static string EscapeRtf(string text)
    {
        return text
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("{", @"\{", StringComparison.Ordinal)
            .Replace("}", @"\}", StringComparison.Ordinal);
    }

    private static string Value(IReadOnlyDictionary<string, string> values, char separator, string key, string fallback = "")
        => values.TryGetValue(separator + key, out var value) ? value : fallback;

    private static string Text(XElement element, string name, string fallback = "")
        => element.Element(name)?.Value ?? fallback;

    private static int ToInt(string value, int defaultValue = 0)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : defaultValue;

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private static string GetString(SQLiteDataReader reader, string columnName)
    {
        var ordinal = GetOrdinal(reader, columnName);
        return ordinal >= 0 && !reader.IsDBNull(ordinal) ? Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? "" : "";
    }

    private static int GetInt(SQLiteDataReader reader, string columnName, int fallback = 0)
    {
        var value = GetString(reader, columnName);
        return ToInt(value, fallback);
    }

    private static string GetOptionalString(SQLiteDataReader reader, string columnName)
        => string.IsNullOrWhiteSpace(columnName) ? "" : GetString(reader, columnName);

    private static int GetOptionalInt(SQLiteDataReader reader, string columnName, int fallback = 0)
        => string.IsNullOrWhiteSpace(columnName) ? fallback : GetInt(reader, columnName, fallback);

    private static string MergeAccessLyrics(SQLiteDataReader reader, IReadOnlyList<string> columns)
    {
        var parts = columns
            .Select(column => NormalizeLineEndings(GetOptionalString(reader, column)).Trim())
            .Where(part => !string.IsNullOrWhiteSpace(part));
        return string.Join("\n\n", parts).TrimEnd('\n', '\r');
    }

    private static string PickColumn(IReadOnlyList<string> columns, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var column = columns.FirstOrDefault(value => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(column))
            {
                return column;
            }
        }

        return "";
    }

    private static bool ColumnExists(IReadOnlyList<string> columns, string columnName)
        => !string.IsNullOrWhiteSpace(columnName) &&
           columns.Any(column => string.Equals(column, columnName, StringComparison.OrdinalIgnoreCase));

    private static string QuoteSqlIdentifier(string value)
        => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string QuoteSqlLiteral(string value)
        => "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";

    private static int GetOrdinal(SQLiteDataReader reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static void Execute(SQLiteConnection connection, string sql)
    {
        using var command = new SQLiteCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private static LoadedImportSongs Failed(ImportSourceKind kind, string path, string message)
        => new(false, kind, [], [new ImportExportIssue(ImportExportIssueSeverity.Error, message)]);

    private sealed record ImportSong(ImportSourceKind Kind, string SourceFolder, SongWriteModel Song);

    private sealed record LoadedImportSongs(
        bool Succeeded,
        ImportSourceKind Kind,
        IReadOnlyList<ImportSong> Songs,
        IReadOnlyList<ImportExportIssue> Issues);

    private sealed record PraiseBookStyleSet(
        PraiseBookTextStyle Title,
        PraiseBookTextStyle Metadata,
        PraiseBookTextStyle Lyrics,
        PraiseBookTextStyle Notation);

    private sealed record RtfColor(int Index, int Red, int Green, int Blue);

    private sealed record ExportSong(SongDetail Detail, string FolderName);
}
