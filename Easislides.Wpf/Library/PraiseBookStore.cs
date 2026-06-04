using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Library;

/// <summary>찬양집 항목의 영속 DTO — 곡 제목과 번호만(색인 표시에 필요한 최소 필드).</summary>
public sealed record PraiseBookEntryDto(string Title, int Number);

/// <summary>
/// 명명 찬양집(곡 모음)을 이름으로 저장/불러오기/삭제/이름변경한다(레거시 PraiseBookDir 대응).
/// 예배 순서 스토어(IWorshipListStore)와 동일한 JSON 파일·경로 안전 규약을 따른다.
/// </summary>
public interface IPraiseBookStore
{
    Task SaveAsync(string name, IReadOnlyList<PraiseBookIndexEntry> entries, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PraiseBookIndexEntry>> LoadAsync(string name, CancellationToken cancellationToken = default);

    IReadOnlyList<string> ListNames();

    void Delete(string name);

    void Rename(string oldName, string newName);
}

/// <summary>
/// 찬양집을 JSON 파일로 영속화하는 기본 스토어 — `%AppData%/EasislidesNext/PraiseBooks/{name}.json`.
/// 경로 안전(무효 문자·예약명·경로 탈출 차단)은 WorshipListStore 와 공유하는 StoreFileNaming 에 위임한다.
/// 디렉터리는 생성자 주입 가능(테스트는 임시 폴더 사용).
/// </summary>
public sealed class PraiseBookStore : IPraiseBookStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly ISettingsService? _settings;

    public PraiseBookStore()
        : this(DefaultDirectory())
    {
    }

    public PraiseBookStore(ISettingsService settings)
        : this(DefaultDirectory(), settings)
    {
    }

    public PraiseBookStore(string directory)
        : this(directory, settings: null)
    {
    }

    public PraiseBookStore(string directory, ISettingsService? settings)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _settings = settings;
    }

    private static string DefaultDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasislidesNext",
        "PraiseBooks");

    public async Task SaveAsync(string name, IReadOnlyList<PraiseBookIndexEntry> entries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var path = ResolvePath(name);
        Directory.CreateDirectory(_directory);

        var dtos = entries.Select(e => new PraiseBookEntryDto(e.Title, e.Number)).ToArray();
        var json = JsonSerializer.Serialize(dtos, JsonOptions);

        // 원자적 쓰기 — temp 에 완전히 쓴 뒤 교체(저장 중 중단돼도 기존 찬양집 보존).
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, path, overwrite: true);
    }

    public async Task<IReadOnlyList<PraiseBookIndexEntry>> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(name);
        if (File.Exists(path))
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            PraiseBookEntryDto[]? dtos;
            try
            {
                dtos = JsonSerializer.Deserialize<PraiseBookEntryDto[]>(json, JsonOptions);
            }
            catch (JsonException)
            {
                // 손상/비-JSON 파일은 빈 목록으로 우아하게 처리(호출자 크래시 방지).
                return Array.Empty<PraiseBookIndexEntry>();
            }

            dtos ??= Array.Empty<PraiseBookEntryDto>();
            return dtos.Select(d => new PraiseBookIndexEntry(d.Title, d.Number)).ToList();
        }

        var legacyPath = ResolveLegacyPath(name);
        if (legacyPath is null || !File.Exists(legacyPath))
        {
            return Array.Empty<PraiseBookIndexEntry>();
        }

        var xml = await File.ReadAllTextAsync(legacyPath, cancellationToken).ConfigureAwait(false);
        return ParseLegacyEsp(xml);
    }

    public IReadOnlyList<string> ListNames()
    {
        var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(_directory))
        {
            foreach (var name in EnumerateLegacyNames())
            {
                names.Add(name);
            }

            return names.ToArray();
        }

        foreach (var name in Directory.GetFiles(_directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!))
        {
            names.Add(name);
        }

        foreach (var name in EnumerateLegacyNames())
        {
            names.Add(name);
        }

        return names.ToArray();
    }

    public void Delete(string name)
    {
        var path = ResolvePath(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var legacyPath = ResolveLegacyPath(name);
        if (legacyPath is not null && File.Exists(legacyPath))
        {
            File.Delete(legacyPath);
        }
    }

    public void Rename(string oldName, string newName)
    {
        var oldPath = ResolvePath(oldName);
        var newPath = ResolvePath(newName);
        var oldLegacyPath = ResolveLegacyPath(oldName);
        var newLegacyPath = ResolveLegacyPath(newName);

        var hasJson = File.Exists(oldPath);
        var hasLegacy = oldLegacyPath is not null && File.Exists(oldLegacyPath);
        if (!hasJson && !hasLegacy)
        {
            return;
        }

        if (hasJson && !string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            if (File.Exists(newPath))
            {
                throw new ArgumentException($"이미 있는 이름입니다: {newName}", nameof(newName));
            }

            File.Move(oldPath, newPath);
        }

        if (hasLegacy && oldLegacyPath is not null && newLegacyPath is not null
            && !string.Equals(oldLegacyPath, newLegacyPath, StringComparison.OrdinalIgnoreCase))
        {
            if (File.Exists(newLegacyPath))
            {
                throw new ArgumentException($"이미 있는 이름입니다: {newName}", nameof(newName));
            }

            File.Move(oldLegacyPath, newLegacyPath);
        }
    }

    private string ResolvePath(string name) => StoreFileNaming.ResolveJsonPath(_directory, name, "찬양집");

    private IEnumerable<string> EnumerateLegacyNames()
    {
        var legacyDirectory = LegacyDirectory();
        if (legacyDirectory is null || !Directory.Exists(legacyDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(legacyDirectory, "*.esp")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!);
    }

    private string? ResolveLegacyPath(string name)
    {
        var legacyDirectory = LegacyDirectory();
        if (legacyDirectory is null || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.EndsWith('.'))
        {
            throw new ArgumentException($"찬양집 이름에 사용할 수 없는 문자/형식입니다: {name}", nameof(name));
        }

        var full = Path.GetFullPath(Path.Combine(legacyDirectory, trimmed + ".esp"));
        var root = Path.GetFullPath(legacyDirectory);
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"찬양집 이름이 허용 경로를 벗어납니다: {name}", nameof(name));
        }

        return full;
    }

    private string? LegacyDirectory()
    {
        var workingFolder = _settings?.Current.General.WorkingFolder;
        if (string.IsNullOrWhiteSpace(workingFolder))
        {
            return null;
        }

        return Path.Combine(workingFolder, "Admin", "PraiseBooks");
    }

    private static IReadOnlyList<PraiseBookIndexEntry> ParseLegacyEsp(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return Array.Empty<PraiseBookIndexEntry>();
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException)
        {
            return Array.Empty<PraiseBookIndexEntry>();
        }

        var entries = new List<PraiseBookIndexEntry>();
        foreach (var element in document.Descendants("Item"))
        {
            var itemId = ((string?)element.Element("ItemID") ?? string.Empty).Trim();
            var title = ((string?)element.Element("Title1") ?? string.Empty).Trim();
            if (itemId.Length == 0 || title.Length == 0)
            {
                continue;
            }

            var songIdText = itemId.Length > 1 ? itemId[1..] : string.Empty;
            var songId = itemId[0] is 'D' or 'd'
                && int.TryParse(songIdText, out var parsedSongId)
                    ? parsedSongId
                    : 0;

            entries.Add(new PraiseBookIndexEntry(title, Number: 0, SongId: songId));
        }

        return entries;
    }
}
