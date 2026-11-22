using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Module;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Shell;

/// <summary>워십 리스트 항목의 영속 DTO — LiveQueueItem 의 직렬화 가능 필드만(ImageSource 등 런타임 상태 제외).</summary>
public sealed record WorshipListItemDto(
    string Id,
    string Title,
    string Kind,
    int SlideNumber,
    string? Lyrics,
    string? ContentPath,
    // 곡별 서식·개별 서식 사용 — 저장/불러오기 시 보존. 구버전 JSON(이 필드 없음)은 기본값(FormatData null, 개별 서식 true)으로 안전 복원.
    [property: JsonConverter(typeof(ListFormatJsonConverter))] string? FormatData = null,
    bool UseIndividualFormatting = true,
    string? Sequence = null,
    Data.SongPageLayout? SongLayout = null,
    int? SourceFolderNo = null,
    int Capo = 0,
    // The stored chords of a song (MSC). A list saved before this field has none until the song is read again.
    string? Notations = null);

/// <summary>예배 순서(워십 리스트)를 이름으로 저장/불러오기/삭제한다(레거시 FrmManageItemLists 대응 — G2).</summary>
public interface IWorshipListStore
{
    Task SaveAsync(string name, IReadOnlyList<LiveQueueItem> items, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LiveQueueItem>> LoadAsync(string name, CancellationToken cancellationToken = default);

    Task SaveSessionAsync(string name, IReadOnlyList<LiveQueueItem> items, string? format, CancellationToken cancellationToken = default)
        => SaveAsync(name, items, cancellationToken);

    async Task<(IReadOnlyList<LiveQueueItem> Items, string? Format)> LoadSessionAsync(string name, CancellationToken cancellationToken = default)
        => (await LoadAsync(name, cancellationToken).ConfigureAwait(false), null);

    IReadOnlyList<string> ListNames();

    void Delete(string name);

    void MoveToTrash(string name);

    // The Trash view of FrmManageItemLists. The defaults serve stores that keep no trash.

    /// <summary>Names of the lists in Trash.</summary>
    IReadOnlyList<string> ListTrashNames() => [];

    /// <summary>Moves a list out of Trash and returns the name it now has: when a list of that name exists,
    /// the restored one gets a time stamp instead of replacing it.</summary>
    string RestoreFromTrash(string name) => name;

    /// <summary>Deletes a list in Trash for good.</summary>
    void DeleteFromTrash(string name) { }

    /// <summary>저장된 예배 순서의 이름을 바꾼다. 새 이름이 이미 있으면 덮어쓰지 않고 예외(호출자가 고유성 검증).</summary>
    void Rename(string oldName, string newName);
}

public interface ILegacyWorshipListStore
{
    Task<string?> LoadLegacyXmlAsync(string name, CancellationToken cancellationToken = default);

    bool IsLegacyWorshipListPath(string path);
}

/// <summary>
/// 워십 리스트를 JSON 파일로 영속화하는 기본 스토어 — `%AppData%/Axicultus/WorshipLists/{name}.json`.
/// 이름은 파일명으로 쓰이므로 무효 문자/경로 구분자를 막아 경로 탈출을 방지한다.
/// 디렉터리는 생성자 주입 가능(테스트는 임시 폴더 사용).
/// </summary>
public sealed class WorshipListStore(string directory, ISettingsService? settings) : IWorshipListStore, ILegacyWorshipListStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private readonly string _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    private readonly ISettingsService? _settings = settings;

    public WorshipListStore()
        : this(DefaultDirectory())
    {
    }

    public WorshipListStore(ISettingsService settings)
        : this(DefaultDirectory(), settings)
    {
    }

    public WorshipListStore(string directory)
        : this(directory, settings: null)
    {
    }

    private static string DefaultDirectory() => Path.Combine(ProductStorage.WpfProfileDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
        "WorshipLists");

    private sealed record SavedSession(WorshipListItemDto[] Items,
        [property: JsonConverter(typeof(ListFormatJsonConverter))] string Format);

    public Task SaveAsync(string name, IReadOnlyList<LiveQueueItem> items, CancellationToken cancellationToken = default)
        => SaveSessionAsync(name, items, null, cancellationToken);

    public async Task SaveSessionAsync(string name, IReadOnlyList<LiveQueueItem> items, string? format, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        var path = ResolvePath(name);
        Directory.CreateDirectory(_directory);

        var dtos = items
            .Select(i => new WorshipListItemDto(i.Id, i.Title, i.Kind, i.SlideNumber, i.Lyrics, i.ContentPath, i.FormatData, i.UseIndividualFormatting, i.Sequence, i.SongLayout, i.SourceFolderNo, i.Capo, i.Notations))
            .ToArray();
        var json = format is null ? JsonSerializer.Serialize(dtos, JsonOptions)
            : JsonSerializer.Serialize(new SavedSession(dtos, format), JsonOptions);

        // 원자적 쓰기 — temp 에 완전히 쓴 뒤 교체. 저장 중 중단돼도 기존 예배 순서가 손상되지 않는다(SettingsService 패턴).
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            try { File.Delete(tempPath); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            throw;
        }
    }

    public async Task<IReadOnlyList<LiveQueueItem>> LoadAsync(string name, CancellationToken cancellationToken = default)
        => (await LoadSessionAsync(name, cancellationToken).ConfigureAwait(false)).Items;

    public async Task<(IReadOnlyList<LiveQueueItem> Items, string? Format)> LoadSessionAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(name);
        if (!File.Exists(path))
        {
            return (Array.Empty<LiveQueueItem>(), null);
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        WorshipListItemDto[]? dtos;
        string? format = null;
        JsonDocument document;
        try { document = JsonDocument.Parse(json); }
        catch (JsonException ex)
        {
            // A cut-off file is not an empty list: the caller keeps its queue, as for an unreadable format below.
            throw new IOException($"예배 순서 '{name}' 파일이 손상되어 읽을 수 없습니다.", ex);
        }

        using (document)
        {
            try
            {
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var session = document.RootElement.Deserialize<SavedSession>(JsonOptions);
                    dtos = session?.Items;
                    format = session?.Format;
                }
                else dtos = document.RootElement.Deserialize<WorshipListItemDto[]>(JsonOptions);
            }
            catch (JsonException ex)
            {
                // Keep the current queue if a newer file contains formatting this presenter cannot read.
                throw new IOException($"예배 순서 '{name}'의 저장 형식을 읽을 수 없습니다.", ex);
            }
        }

        dtos ??= [];
        return (dtos
            .Select(d => new LiveQueueItem(d.Id, d.Title, d.Kind)
            {
                SlideNumber = d.SlideNumber,
                Lyrics = d.Lyrics,
                ContentPath = d.ContentPath,
                FormatData = d.FormatData,
                Sequence = d.Sequence,
                SongLayout = d.SongLayout,
                SourceFolderNo = d.SourceFolderNo,
                Capo = d.Capo,
                Notations = d.Notations,
                UseIndividualFormatting = d.UseIndividualFormatting,
            })
            .ToList(), format);
    }

    public IReadOnlyList<string> ListNames()
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

        return names
            .ToArray();
    }

    public async Task<string?> LoadLegacyXmlAsync(string name, CancellationToken cancellationToken = default)
    {
        if (File.Exists(ResolvePath(name)))
        {
            return null; // WPF JSON lists win when both stores contain the same display name.
        }

        var path = ResolveLegacyPath(name);
        if (path is null || !File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
    }

    public bool IsLegacyWorshipListPath(string path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && (string.Equals(Path.GetExtension(path), ".esw", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase))
            && File.Exists(path);
    }

    public void MoveToTrash(string name)
    {
        Archive(ResolveLegacyPath(name));
        Archive(ResolvePath(name));
    }

    private static void Archive(string? source)
    {
        if (source is null || !File.Exists(source)) return;
        var trash = Path.Combine(Path.GetDirectoryName(source)!, "Trash");
        Directory.CreateDirectory(trash);
        var destination = Path.Combine(trash, Path.GetFileName(source));
        try { File.Move(source, destination); }
        catch (IOException) when (File.Exists(destination) && File.Exists(source))
        {
            File.Move(source, Path.Combine(trash, $"{Path.GetFileNameWithoutExtension(source)}_{Guid.NewGuid():N}{Path.GetExtension(source)}"));
        }
    }

    // MoveToTrash files a list under "Trash" next to where it was: this store's folder and, for a list that
    // came from the WinForms app, its working copy under the working folder.
    private IEnumerable<string> ListFolders()
    {
        yield return _directory;
        if (LegacyDirectory() is { } legacy) yield return legacy;
    }

    private static string TrashFile(string folder, string name)
    {
        // A trash name is a file name taken from the folder listing; anything else is refused.
        if (string.IsNullOrWhiteSpace(name) || Path.GetFileName(name) != name)
        {
            throw new ArgumentException($"예배 순서 이름에 사용할 수 없는 문자/형식입니다: {name}", nameof(name));
        }

        return Path.Combine(folder, "Trash", name + ".json");
    }

    public IReadOnlyList<string> ListTrashNames()
    {
        var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var folder in ListFolders())
        {
            var trash = Path.Combine(folder, "Trash");
            if (!Directory.Exists(trash)) continue;
            foreach (var file in Directory.GetFiles(trash, "*.json"))
            {
                names.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        return names.ToArray();
    }

    public string RestoreFromTrash(string name)
    {
        _ = TrashFile(_directory, name); // refuses a name that is not a file name before anything is looked up
        var folders = ListFolders().ToArray();
        // FrmManageItemLists.RestoreBtn_Click: a list of the same name stays; the restored one gets a time stamp.
        // The name is one across both folders (ListNames), so a clash in either counts.
        var restored = folders.Any(folder => File.Exists(Path.Combine(folder, name + ".json")))
            ? $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}"
            : name;
        foreach (var folder in folders.Where(folder => File.Exists(TrashFile(folder, name))))
        {
            File.Move(TrashFile(folder, name), Path.Combine(folder, restored + ".json"));
        }

        return restored;
    }

    public void DeleteFromTrash(string name)
    {
        foreach (var folder in ListFolders())
        {
            var file = TrashFile(folder, name);
            if (File.Exists(file)) File.Delete(file);
        }
    }

    public void Delete(string name)
    {
        var path = ResolvePath(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (ResolveLegacyPath(name) is { } migrated) File.Delete(migrated);
    }

    public void Rename(string oldName, string newName) =>
        StoreFileNaming.RenameList(ResolvePath(oldName), ResolvePath(newName), ResolveLegacyPath(oldName), ResolveLegacyPath(newName));

    private string ResolvePath(string name) => StoreFileNaming.ResolveJsonPath(_directory, name, "예배 순서");

    private IEnumerable<string> EnumerateLegacyNames()
    {
        var legacyDirectory = LegacyDirectory();
        if (legacyDirectory is null || !Directory.Exists(legacyDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(legacyDirectory, "*.json")
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
            throw new ArgumentException($"예배 순서 이름에 사용할 수 없는 문자/형식입니다: {name}", nameof(name));
        }

        var full = Path.GetFullPath(Path.Combine(legacyDirectory, trimmed + ".json"));
        var root = Path.GetFullPath(legacyDirectory);
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"예배 순서 이름이 허용 경로를 벗어납니다: {name}", nameof(name));
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

        // A working folder on a dropped USB/NAS drive must not take the session list (or app start) down with it.
        try
        {
            return Axicultus.Module.ServiceListDocument.WorkingDirectory(Path.Combine(workingFolder, "Admin", "WorshipLists"), ".esw");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
