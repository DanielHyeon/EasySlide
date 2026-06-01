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

    /// <summary>저장된 예배 순서의 이름을 바꾼다. 새 이름이 이미 있으면 덮어쓰지 않고 예외(호출자가 고유성 검증).</summary>
    void Rename(string oldName, string newName);
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

    public void Rename(string oldName, string newName)
    {
        // 두 이름 모두 경로 안전 검증을 거친다(무효 문자/예약명/경로 탈출 차단).
        var oldPath = ResolvePath(oldName);
        var newPath = ResolvePath(newName);

        // 원본이 없으면 조용히 무시(Delete 와 동일하게 관대 — 이미 지워졌거나 잘못된 호출에도 크래시 없음).
        if (!File.Exists(oldPath))
        {
            return;
        }

        // 같은 파일(대소문자만 다른 경우 포함)이면 바꿀 게 없다.
        if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 새 이름이 이미 있으면 덮어쓰지 않고 거부(기존 예배 순서 보호 — 호출자가 사전 검증하지만 스토어도 방어).
        if (File.Exists(newPath))
        {
            throw new ArgumentException($"이미 있는 이름입니다: {newName}", nameof(newName));
        }

        File.Move(oldPath, newPath);
    }

    /// <summary>이름을 안전한 파일 경로로 해석(무효 문자/예약명/길이/경로 탈출 차단 — 공통 헬퍼 위임).</summary>
    private string ResolvePath(string name) => StoreFileNaming.ResolveJsonPath(_directory, name, "예배 순서");
}
