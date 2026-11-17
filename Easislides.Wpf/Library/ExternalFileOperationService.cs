using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

public enum ExternalFileItemKind
{
    InfoScreen,
    PowerPoint,
    Media,
}

public enum ExternalFileOperationKind
{
    Copy,
    Move,
}

public enum ExternalFileDestinationKind
{
    ExternalFolder,
    SongFolder,
}

public enum ExternalFileOperationIssueKind
{
    InvalidRequest,
    SourceMissing,
    SourceNotFile,
    TargetMissing,
    WriteFailed,
    UnsupportedDestination,
}

public sealed record ExternalFileFolder(int Index, string Name, string Path);

public sealed record ExternalFileItem(string Path, string DisplayName);

public sealed record ExternalFileOperationIssue(
    ExternalFileOperationIssueKind Kind,
    string Message,
    string? Path = null);

public sealed record ExternalFileOperationRequest(
    ExternalFileOperationKind OperationKind,
    ExternalFileItemKind ItemKind,
    ExternalFileDestinationKind DestinationKind,
    IReadOnlyList<string> SourceFiles,
    string? TargetFolderPath = null,
    string? DatabasePath = null,
    int? TargetSongFolderNo = null,
    string? BackupRoot = null,
    int StartingSongNumber = 1);

public sealed record ExternalFileOperationReport(
    bool Succeeded,
    ExternalFileOperationKind OperationKind,
    ExternalFileItemKind ItemKind,
    ExternalFileDestinationKind DestinationKind,
    IReadOnlyList<string> CreatedFilePaths,
    IReadOnlyList<int> CreatedSongIds,
    IReadOnlyList<ExternalFileOperationIssue> Issues)
{
    public static ExternalFileOperationReport Success(
        ExternalFileOperationKind operationKind,
        ExternalFileItemKind itemKind,
        ExternalFileDestinationKind destinationKind,
        IReadOnlyList<string> CreatedFilePaths,
        IReadOnlyList<int> CreatedSongIds)
        => new(true, operationKind, itemKind, destinationKind, CreatedFilePaths, CreatedSongIds, []);
}

public interface IExternalFileOperationService
{
    IReadOnlyList<ExternalFileFolder> GetFolders(string workingFolder, ExternalFileItemKind itemKind);

    Task<ExternalFileOperationReport> ExecuteAsync(ExternalFileOperationRequest request);
}

public sealed class ExternalFileOperationService : IExternalFileOperationService
{
    private readonly IAdminDatabaseRepository _adminDatabase;

    public ExternalFileOperationService(IAdminDatabaseRepository adminDatabase)
    {
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
    }

    public IReadOnlyList<ExternalFileFolder> GetFolders(string workingFolder, ExternalFileItemKind itemKind)
    {
        if (string.IsNullOrWhiteSpace(workingFolder))
        {
            return [];
        }

        var root = Path.Combine(Path.GetFullPath(workingFolder), GetRootDirectoryName(itemKind));
        var displayRoot = GetDisplayRootName(itemKind);
        var folders = new List<ExternalFileFolder>
        {
            new(0, displayRoot, EnsureTrailingSeparator(root)),
        };

        if (!Directory.Exists(root))
        {
            return folders;
        }

        foreach (var directory in EnumerateDirectoriesDepthFirst(root))
        {
            folders.Add(new ExternalFileFolder(
                folders.Count,
                "\\" + Path.GetRelativePath(root, directory).Replace(Path.DirectorySeparatorChar, '\\'),
                EnsureTrailingSeparator(directory)));
        }

        return folders;
    }

    public Task<ExternalFileOperationReport> ExecuteAsync(ExternalFileOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.DestinationKind == ExternalFileDestinationKind.SongFolder
            ? CopyInfoScreensToSongFolderAsync(request)
            : Task.FromResult(CopyOrMoveToExternalFolder(request));
    }

    private ExternalFileOperationReport CopyOrMoveToExternalFolder(ExternalFileOperationRequest request)
    {
        var issues = new List<ExternalFileOperationIssue>();
        var created = new List<string>();
        if (string.IsNullOrWhiteSpace(request.TargetFolderPath))
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.TargetMissing,
                "대상 외부 폴더를 선택하세요."));
            return CreateReport(request, created, [], issues);
        }

        var targetFolder = Path.GetFullPath(request.TargetFolderPath);
        try
        {
            Directory.CreateDirectory(targetFolder);
        }
        catch (Exception ex) when (IsRecoverableFileException(ex))
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.WriteFailed,
                $"대상 폴더를 만들 수 없습니다: {ex.Message}",
                targetFolder));
            return CreateReport(request, created, [], issues);
        }

        foreach (var source in NormalizeSources(request.SourceFiles))
        {
            if (!ValidateSource(source, issues))
            {
                continue;
            }

            try
            {
                var destination = GetAvailableDestinationPath(source, targetFolder);
                if (request.OperationKind == ExternalFileOperationKind.Move)
                {
                    File.Move(source, destination);
                }
                else
                {
                    File.Copy(source, destination);
                }

                created.Add(destination);
            }
            catch (Exception ex) when (IsRecoverableFileException(ex))
            {
                issues.Add(new ExternalFileOperationIssue(
                    ExternalFileOperationIssueKind.WriteFailed,
                    $"{Path.GetFileName(source)} 처리 실패: {ex.Message}",
                    source));
            }
        }

        return CreateReport(request, created, [], issues);
    }

    private async Task<ExternalFileOperationReport> CopyInfoScreensToSongFolderAsync(ExternalFileOperationRequest request)
    {
        var issues = new List<ExternalFileOperationIssue>();
        var createdIds = new List<int>();
        if (request.OperationKind != ExternalFileOperationKind.Copy ||
            request.ItemKind != ExternalFileItemKind.InfoScreen)
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.UnsupportedDestination,
                "곡 폴더로 가져오기는 InfoScreen 복사에서만 사용할 수 있습니다."));
            return CreateReport(request, [], createdIds, issues);
        }

        if (string.IsNullOrWhiteSpace(request.DatabasePath) || !File.Exists(request.DatabasePath))
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.TargetMissing,
                "AdminDB 파일을 찾을 수 없습니다.",
                request.DatabasePath));
            return CreateReport(request, [], createdIds, issues);
        }

        if (request.TargetSongFolderNo is null or <= 0)
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.TargetMissing,
                "대상 곡 폴더를 선택하세요."));
            return CreateReport(request, [], createdIds, issues);
        }

        var databasePath = Path.GetFullPath(request.DatabasePath);
        var backupRoot = ResolveBackupRoot(databasePath, request.BackupRoot);
        var nextSongNumber = Math.Max(request.StartingSongNumber, 1);
        foreach (var source in NormalizeSources(request.SourceFiles))
        {
            if (!ValidateSource(source, issues))
            {
                continue;
            }

            var song = CreateSongFromInfoScreen(source, request.TargetSongFolderNo.Value, nextSongNumber);
            var report = await _adminDatabase.SaveSongAsync(databasePath, backupRoot, song).ConfigureAwait(true);
            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                issues.Add(new ExternalFileOperationIssue(
                    ExternalFileOperationIssueKind.WriteFailed,
                    string.IsNullOrWhiteSpace(detail) ? "AdminDB 저장 실패" : detail,
                    source));
                continue;
            }

            if (report.AffectedSongIds.Count > 0)
            {
                createdIds.Add(report.AffectedSongIds[0]);
            }

            nextSongNumber++;
        }

        return CreateReport(request, [], createdIds, issues);
    }

    private static SongWriteModel CreateSongFromInfoScreen(string sourcePath, int folderNo, int songNumber)
    {
        var title = Path.GetFileNameWithoutExtension(sourcePath);
        try
        {
            var document = XDocument.Load(sourcePath, LoadOptions.PreserveWhitespace);
            var item = document.Descendants().FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, "Item", StringComparison.OrdinalIgnoreCase));
            if (item is not null)
            {
                title = FirstNonEmpty(ReadElement(item, "Title1"), title);
                return new SongWriteModel(
                    SongId: null,
                    title,
                    ReadElement(item, "Title2"),
                    folderNo,
                    songNumber,
                    NormalizeLyrics(ReadElement(item, "Contents")),
                    Sequence: ReadElement(item, "Sequence"),
                    Writer: ReadElement(item, "Writer"),
                    Copyright: ReadElement(item, "Copyright"),
                    Capo: ParseCapo(ReadElement(item, "Capo")),
                    Timing: ReadElement(item, "Timing"),
                    Key: ReadElement(item, "MusicKey"),
                    Notations: ReadElement(item, "Notations"),
                    Category: ReadElement(item, "Category"),
                    LicenceAdmin1: ReadElement(item, "LicenceAdmin1"),
                    LicenceAdmin2: ReadElement(item, "LicenceAdmin2"),
                    BookReference: ReadElement(item, "BookReference"),
                    UserReference: ReadElement(item, "UserReference"),
                    Settings: ReadElement(item, "Settings"),
                    FormatData: ReadElement(item, "FormatData"));
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
        }

        var text = File.ReadAllText(sourcePath);
        return new SongWriteModel(
            SongId: null,
            title,
            AlternateTitle: "",
            folderNo,
            songNumber,
            NormalizeLegacyInfoFileContents(text));
    }

    private static ExternalFileOperationReport CreateReport(
        ExternalFileOperationRequest request,
        IReadOnlyList<string> createdFiles,
        IReadOnlyList<int> createdSongIds,
        IReadOnlyList<ExternalFileOperationIssue> issues)
        => new(
            issues.Count == 0,
            request.OperationKind,
            request.ItemKind,
            request.DestinationKind,
            createdFiles,
            createdSongIds,
            issues);

    private static IEnumerable<string> NormalizeSources(IReadOnlyList<string> sources)
        => sources
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static bool ValidateSource(string source, List<ExternalFileOperationIssue> issues)
    {
        if (Directory.Exists(source))
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.SourceNotFile,
                "소스는 파일이어야 합니다.",
                source));
            return false;
        }

        if (!File.Exists(source))
        {
            issues.Add(new ExternalFileOperationIssue(
                ExternalFileOperationIssueKind.SourceMissing,
                "소스 파일을 찾을 수 없습니다.",
                source));
            return false;
        }

        return true;
    }

    private static string GetAvailableDestinationPath(string sourcePath, string targetFolder)
    {
        var extension = Path.GetExtension(sourcePath);
        var name = Path.GetFileNameWithoutExtension(sourcePath);
        var destination = Path.Combine(targetFolder, name + extension);
        var suffix = 0;
        while (File.Exists(destination))
        {
            suffix++;
            destination = Path.Combine(targetFolder, $"{name} - Copy ({suffix}){extension}");
        }

        return destination;
    }

    private static IEnumerable<string> EnumerateDirectoriesDepthFirst(string root)
    {
        foreach (var directory in Directory.EnumerateDirectories(root).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            yield return directory;
            foreach (var child in EnumerateDirectoriesDepthFirst(directory))
            {
                yield return child;
            }
        }
    }

    private static string GetRootDirectoryName(ExternalFileItemKind itemKind)
        => itemKind switch
        {
            ExternalFileItemKind.InfoScreen => "InfoScreens",
            ExternalFileItemKind.PowerPoint => "Powerpoint",
            ExternalFileItemKind.Media => "Media",
            _ => throw new ArgumentOutOfRangeException(nameof(itemKind), itemKind, null),
        };

    private static string GetDisplayRootName(ExternalFileItemKind itemKind)
        => itemKind switch
        {
            ExternalFileItemKind.InfoScreen => "InfoScreen Items",
            ExternalFileItemKind.PowerPoint => "Powerpoint Items",
            ExternalFileItemKind.Media => "Media Items",
            _ => throw new ArgumentOutOfRangeException(nameof(itemKind), itemKind, null),
        };

    private static string EnsureTrailingSeparator(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return fullPath.EndsWith(Path.DirectorySeparatorChar)
            ? fullPath
            : fullPath + Path.DirectorySeparatorChar;
    }

    private static string ResolveBackupRoot(string databasePath, string? configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        var directory = Path.GetDirectoryName(databasePath);
        return Path.Combine(string.IsNullOrWhiteSpace(directory) ? Environment.CurrentDirectory : directory, "Backups");
    }

    private static string ReadElement(XElement parent, string name)
        => parent.Elements().FirstOrDefault(element =>
            string.Equals(element.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? "";

    private static string NormalizeLyrics(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private static string NormalizeLegacyInfoFileContents(string text)
    {
        var normalized = NormalizeLyrics(text);
        if (!normalized.StartsWith("[", StringComparison.Ordinal))
        {
            return normalized;
        }

        var close = normalized.IndexOf(']', StringComparison.Ordinal);
        return close >= 0 && close < normalized.Length - 1
            ? normalized[(close + 1)..].TrimStart('\n')
            : normalized;
    }

    private static string FirstNonEmpty(string first, string second)
        => string.IsNullOrWhiteSpace(first) ? second : first.Trim();

    private static int ParseCapo(string value)
        => int.TryParse(value, out var capo) ? capo : -1;

    private static bool IsRecoverableFileException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
