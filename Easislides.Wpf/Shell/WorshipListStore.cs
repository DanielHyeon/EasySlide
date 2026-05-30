using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides.Wpf.Shell;

/// <summary>워십 리스트 항목의 영속 DTO — LiveQueueItem 의 직렬화 가능 필드만(ImageSource 등 런타임 상태 제외).</summary>
public sealed record WorshipListItemDto(
    string Id,
    string Title,
    string Kind,
    int SlideNumber,
    string? Lyrics,
    string? ContentPath);

/// <summary>예배 순서(워십 리스트)를 이름으로 저장/불러오기/삭제한다(레거시 FrmManageItemLists 대응 — G2).</summary>
public interface IWorshipListStore
{
    Task SaveAsync(string name, IReadOnlyList<LiveQueueItem> items, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LiveQueueItem>> LoadAsync(string name, CancellationToken cancellationToken = default);

    IReadOnlyList<string> ListNames();

    void Delete(string name);
}

/// <summary>
/// 워십 리스트를 JSON 파일로 영속화하는 기본 스토어 — `%AppData%/EasislidesNext/WorshipLists/{name}.json`.
/// 이름은 파일명으로 쓰이므로 무효 문자/경로 구분자를 막아 경로 탈출을 방지한다.
/// 디렉터리는 생성자 주입 가능(테스트는 임시 폴더 사용).
/// </summary>
public sealed class WorshipListStore : IWorshipListStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _directory;

    public WorshipListStore()
        : this(DefaultDirectory())
    {
    }

    public WorshipListStore(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    private static string DefaultDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasislidesNext",
        "WorshipLists");

    public async Task SaveAsync(string name, IReadOnlyList<LiveQueueItem> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        var path = ResolvePath(name);
        Directory.CreateDirectory(_directory);

        var dtos = items
            .Select(i => new WorshipListItemDto(i.Id, i.Title, i.Kind, i.SlideNumber, i.Lyrics, i.ContentPath))
            .ToArray();
        var json = JsonSerializer.Serialize(dtos, JsonOptions);

        // 원자적 쓰기 — temp 에 완전히 쓴 뒤 교체. 저장 중 중단돼도 기존 예배 순서가 손상되지 않는다(SettingsService 패턴).
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
        File.Move(tempPath, path, overwrite: true);
    }

    public async Task<IReadOnlyList<LiveQueueItem>> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(name);
        if (!File.Exists(path))
        {
            return Array.Empty<LiveQueueItem>();
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        WorshipListItemDto[]? dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<WorshipListItemDto[]>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // 손상/비-JSON 파일은 빈 목록으로 우아하게 처리(호출자 크래시 방지).
            return Array.Empty<LiveQueueItem>();
        }

        dtos ??= Array.Empty<WorshipListItemDto>();
        return dtos
            .Select(d => new LiveQueueItem(d.Id, d.Title, d.Kind)
            {
                SlideNumber = d.SlideNumber,
                Lyrics = d.Lyrics,
                ContentPath = d.ContentPath,
            })
            .ToList();
    }

    public IReadOnlyList<string> ListNames()
    {
        if (!Directory.Exists(_directory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(_directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void Delete(string name)
    {
        var path = ResolvePath(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>이름을 안전한 파일 경로로 해석(무효 문자/예약명/길이/경로 탈출 차단).</summary>
    private string ResolvePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("워십 리스트 이름이 비어 있습니다.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 100
            || trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.EndsWith('.')
            || IsReservedDeviceName(trimmed))
        {
            throw new ArgumentException($"워십 리스트 이름에 사용할 수 없는 문자/형식입니다: {name}", nameof(name));
        }

        // 정규화 후 의도한 디렉터리 바로 밑인지 최종 확인(블랙리스트 우회까지 막는 화이트리스트 방어선).
        var full = Path.GetFullPath(Path.Combine(_directory, trimmed + ".json"));
        var root = Path.GetFullPath(_directory);
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"워십 리스트 이름이 허용 경로를 벗어납니다: {name}", nameof(name));
        }

        return full;
    }

    /// <summary>Windows 예약 디바이스명(CON/PRN/AUX/NUL/COM1-9/LPT1-9) 여부.</summary>
    private static bool IsReservedDeviceName(string name)
    {
        var upper = name.ToUpperInvariant();
        if (upper is "CON" or "PRN" or "AUX" or "NUL")
        {
            return true;
        }

        return upper.Length == 4
            && (upper.StartsWith("COM", StringComparison.Ordinal) || upper.StartsWith("LPT", StringComparison.Ordinal))
            && upper[3] is >= '1' and <= '9';
    }
}
