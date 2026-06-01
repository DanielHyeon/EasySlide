using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

    public PraiseBookStore()
        : this(DefaultDirectory())
    {
    }

    public PraiseBookStore(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
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
        if (!File.Exists(path))
        {
            return Array.Empty<PraiseBookIndexEntry>();
        }

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
        var oldPath = ResolvePath(oldName);
        var newPath = ResolvePath(newName);

        if (!File.Exists(oldPath))
        {
            return;
        }

        if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(newPath))
        {
            throw new ArgumentException($"이미 있는 이름입니다: {newName}", nameof(newName));
        }

        File.Move(oldPath, newPath);
    }

    private string ResolvePath(string name) => StoreFileNaming.ResolveJsonPath(_directory, name, "찬양집");
}
