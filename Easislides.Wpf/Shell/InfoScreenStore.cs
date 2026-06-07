using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

/// <summary>정보 화면(공지) 저장 DTO — 본문·글자 크기(0=기본)·가로 정렬(0=기본/1왼쪽/2가운데/3오른쪽)·글자색(ARGB int, 0=기본)·배경색(ARGB int, 0=기본)·굵게·기울임·밑줄·글꼴명(빈/null=전역 기본). 레거시 InfoScreen(.esi) 보존 필드.
/// (새 필드는 모두 맨 끝에 기본값으로 추가 — 옛 저장 파일(필드 없음)도 JSON 역직렬화 시 기본값(0/false/null)으로 안전하게 읽힌다.)</summary>
public sealed record InfoScreenDto(string Text, int FontSize = 0, int Alignment = 0, int ColorArgb = 0, int BackgroundColorArgb = 0, bool Bold = false, bool Italic = false, bool Underline = false, string? FontName = null);

/// <summary>공지 송출 옵션 — 글자 크기(pt)·가로 정렬(0기본/1왼쪽/2가운데/3오른쪽)·글자색(ARGB int, 0=기본)·배경색(ARGB int, 0=기본)·굵게·기울임·밑줄(강조)·글꼴명(빈/null=전역 기본).
/// 송출 콜백 인자를 정수 나열(위치 모호) 대신 이름 있는 레코드로 묶어 인자 순서 실수를 막는다.</summary>
public sealed record NoticeOptions(int FontSizePt = 0, int Alignment = 0, int ColorArgb = 0, int BackgroundColorArgb = 0, bool Bold = false, bool Italic = false, bool Underline = false, string? FontName = null);

/// <summary>
/// 명명 정보 화면(공지 텍스트)을 이름으로 저장/불러오기/삭제한다(레거시 FrmInfoScreen 의 .esi 목록 대응의 첫 슬라이스).
/// 예배 순서·찬양집 스토어와 동일한 JSON 파일·경로 안전 규약을 따른다(무효 문자·예약명·경로 탈출 차단).
/// </summary>
public interface IInfoScreenStore
{
    Task SaveAsync(string name, InfoScreenDto screen, CancellationToken cancellationToken = default);

    Task<InfoScreenDto?> LoadAsync(string name, CancellationToken cancellationToken = default);

    Task<string?> PrepareCopySourceFileAsync(string name, string exportDirectory, CancellationToken cancellationToken = default);

    System.Collections.Generic.IReadOnlyList<string> ListNames();

    bool Delete(string name);
}

/// <summary>
/// 정보 화면을 JSON 파일로 영속화 — `%AppData%/EasislidesNext/InfoScreens/{name}.json`.
/// 경로 안전은 StoreFileNaming 에 위임(PraiseBookStore 와 동일 패턴). 디렉터리는 생성자 주입 가능(테스트는 임시 폴더).
/// </summary>
public sealed class InfoScreenStore : IInfoScreenStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly ISettingsService? _settings;
    private readonly Func<string, bool> _deleteFile;

    public InfoScreenStore()
        : this(DefaultDirectory(), settings: null)
    {
    }

    public InfoScreenStore(ISettingsService settings)
        : this(DefaultDirectory(), settings)
    {
    }

    public InfoScreenStore(string directory)
        : this(directory, settings: null)
    {
    }

    public InfoScreenStore(string directory, ISettingsService? settings)
        : this(directory, settings, SendFileToRecycleBin)
    {
    }

    internal InfoScreenStore(string directory, ISettingsService? settings, Func<string, bool> deleteFile)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _settings = settings;
        _deleteFile = deleteFile ?? throw new ArgumentNullException(nameof(deleteFile));
    }

    private static string DefaultDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasislidesNext",
        "InfoScreens");

    public async Task SaveAsync(string name, InfoScreenDto screen, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(screen);
        var path = ResolvePath(name);
        Directory.CreateDirectory(_directory);

        var json = JsonSerializer.Serialize(screen, JsonOptions);

        // 원자적 쓰기 — temp 에 완전히 쓴 뒤 교체(저장 중 중단돼도 기존 파일 보존).
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, path, overwrite: true);
    }

    public async Task<InfoScreenDto?> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = TryResolvePath(name);
        if (path is not null && File.Exists(path))
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            try
            {
                return JsonSerializer.Deserialize<InfoScreenDto>(json, JsonOptions);
            }
            catch (JsonException)
            {
                // 손상/비-JSON 파일은 null 로 우아하게 처리(호출자 크래시 방지).
                return null;
            }
        }

        var legacyPath = ResolveLegacyPath(name);
        if (legacyPath is null || !File.Exists(legacyPath))
        {
            return null;
        }

        return await LoadLegacyAsync(legacyPath, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> PrepareCopySourceFileAsync(string name, string exportDirectory, CancellationToken cancellationToken = default)
    {
        var legacyPath = ResolveLegacyPath(name);
        if (legacyPath is not null && File.Exists(legacyPath))
        {
            return legacyPath;
        }

        if (string.IsNullOrWhiteSpace(exportDirectory))
        {
            throw new ArgumentException("Export directory is required.", nameof(exportDirectory));
        }

        var jsonPath = TryResolvePath(name);
        if (jsonPath is null || !File.Exists(jsonPath))
        {
            return null;
        }

        var screen = await LoadAsync(name, cancellationToken).ConfigureAwait(false);
        if (screen is null)
        {
            return null;
        }

        Directory.CreateDirectory(exportDirectory);
        var exportPath = Path.Combine(exportDirectory, SanitizeExportFileName(name) + ".esi");
        var tempPath = $"{exportPath}.{Guid.NewGuid():N}.tmp";
        var document = CreateLegacyExportDocument(name, screen);
        await File.WriteAllTextAsync(tempPath, document.ToString(SaveOptions.None), cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, exportPath, overwrite: true);
        return exportPath;
    }

    public System.Collections.Generic.IReadOnlyList<string> ListNames()
    {
        var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(_directory))
        {
            foreach (var name in Directory.GetFiles(_directory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => n!))
            {
                names.Add(name);
            }
        }

        foreach (var name in EnumerateLegacyNames())
        {
            names.Add(name);
        }

        return names.ToArray();
    }

    public bool Delete(string name)
    {
        var path = TryResolvePath(name);
        if (path is not null && File.Exists(path))
        {
            return _deleteFile(path);
        }

        var legacyPath = ResolveLegacyPath(name);
        return legacyPath is not null && File.Exists(legacyPath) && _deleteFile(legacyPath);
    }

    private string ResolvePath(string name) => StoreFileNaming.ResolveJsonPath(_directory, name, "정보 화면");

    private string? TryResolvePath(string name)
    {
        try
        {
            return ResolvePath(name);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private IEnumerable<string> EnumerateLegacyNames()
    {
        var legacyDirectory = LegacyDirectory();
        if (legacyDirectory is null || !Directory.Exists(legacyDirectory))
        {
            return Array.Empty<string>();
        }

        try
        {
            return Directory.EnumerateFiles(legacyDirectory, "*.esi", SearchOption.AllDirectories)
                .Select(path => LegacyNameFromPath(legacyDirectory, path))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToArray();
        }
        catch (Exception ex) when (IsRecoverableFileException(ex))
        {
            return Array.Empty<string>();
        }
    }

    private string? ResolveLegacyPath(string name)
    {
        var legacyDirectory = LegacyDirectory();
        if (legacyDirectory is null || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        var extension = Path.GetExtension(trimmed);
        if (!string.IsNullOrEmpty(extension)
            && !string.Equals(extension, ".esi", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relative = string.IsNullOrEmpty(extension) ? trimmed + ".esi" : trimmed;
        if (!IsSafeLegacyRelativePath(relative))
        {
            return null;
        }

        var full = Path.GetFullPath(Path.Combine(legacyDirectory, relative));
        var root = Path.GetFullPath(legacyDirectory);
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        return full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) ? full : null;
    }

    private string? LegacyDirectory()
    {
        var workingFolder = _settings?.Current.General.WorkingFolder;
        if (string.IsNullOrWhiteSpace(workingFolder))
        {
            return null;
        }

        return Path.Combine(workingFolder, "InfoScreens");
    }

    private static string? LegacyNameFromPath(string legacyDirectory, string path)
    {
        var relative = Path.GetRelativePath(legacyDirectory, path);
        var directory = Path.GetDirectoryName(relative);
        var name = Path.GetFileNameWithoutExtension(relative);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(directory)
            ? name
            : Path.Combine(directory, name);
    }

    private static XDocument CreateLegacyExportDocument(string name, InfoScreenDto screen)
        => new(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("EasiSlides",
                new XElement("Item",
                    new XElement("Title1", name),
                    new XElement("Contents", screen.Text ?? string.Empty))));

    private static string SanitizeExportFileName(string name)
    {
        var value = Path.GetFileNameWithoutExtension((name ?? string.Empty).Trim());
        if (string.IsNullOrWhiteSpace(value))
        {
            value = "InfoScreen";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim(' ', '.');
        return string.IsNullOrWhiteSpace(sanitized) ? "InfoScreen" : sanitized;
    }

    private static async Task<InfoScreenDto?> LoadLegacyAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            var contents = TryReadLegacyXmlContents(text) ?? text;
            return new InfoScreenDto(NormalizeLegacyInfoFileContents(contents));
        }
        catch (Exception ex) when (IsRecoverableFileException(ex))
        {
            return null;
        }
    }

    private static string? TryReadLegacyXmlContents(string text)
    {
        try
        {
            var document = XDocument.Parse(text, LoadOptions.PreserveWhitespace);
            var item = document.Descendants().FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, "Item", StringComparison.OrdinalIgnoreCase));
            return item is null ? null : ReadElement(item, "Contents");
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }

    private static string ReadElement(XElement parent, string name)
        => parent.Elements().FirstOrDefault(element =>
            string.Equals(element.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? "";

    private static string NormalizeLegacyInfoFileContents(string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (!normalized.StartsWith("[", StringComparison.Ordinal))
        {
            return normalized;
        }

        var close = normalized.IndexOf(']', StringComparison.Ordinal);
        return close >= 0 && close < normalized.Length - 1
            ? normalized[(close + 1)..].TrimStart('\n')
            : normalized;
    }

    private static bool IsSafeLegacyRelativePath(string relative)
    {
        var parts = relative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        return parts.All(part =>
            part.Length > 0
            && part != "."
            && part != ".."
            && part.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && !part.EndsWith(".", StringComparison.Ordinal));
    }

    private static bool IsRecoverableFileException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;

    private static bool SendFileToRecycleBin(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var operation = new SHFILEOPSTRUCT
            {
                hwnd = IntPtr.Zero,
                wFunc = 3,
                pFrom = path + '\0' + '\0',
                pTo = null,
                fFlags = 0x40 | 0x10 | 0x04 | 0x0400,
                fAnyOperationsAborted = false,
                hNameMappings = IntPtr.Zero,
                lpszProgressTitle = null,
            };

            var result = SHFileOperation(ref operation);
            return result == 0 && !operation.fAnyOperationsAborted && !File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string? pTo;
        public ushort fFlags;
        [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string? lpszProgressTitle;
    }
}
