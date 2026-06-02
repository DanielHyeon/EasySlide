using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides.Wpf.Shell;

/// <summary>정보 화면(공지) 저장 DTO — 본문·글자 크기(0=기본)·가로 정렬(0=기본/1왼쪽/2가운데/3오른쪽)·글자색(ARGB int, 0=기본)·배경색(ARGB int, 0=기본)·굵게·기울임·밑줄. 레거시 InfoScreen(.esi) 보존 필드.
/// (새 필드는 모두 맨 끝에 기본값으로 추가 — 옛 저장 파일(필드 없음)도 JSON 역직렬화 시 기본값(0/false)으로 안전하게 읽힌다.)</summary>
public sealed record InfoScreenDto(string Text, int FontSize = 0, int Alignment = 0, int ColorArgb = 0, int BackgroundColorArgb = 0, bool Bold = false, bool Italic = false, bool Underline = false);

/// <summary>공지 송출 옵션 — 글자 크기(pt)·가로 정렬(0기본/1왼쪽/2가운데/3오른쪽)·글자색(ARGB int, 0=기본)·배경색(ARGB int, 0=기본)·굵게·기울임·밑줄(강조).
/// 송출 콜백 인자를 정수 나열(위치 모호) 대신 이름 있는 레코드로 묶어 인자 순서 실수를 막는다.</summary>
public sealed record NoticeOptions(int FontSizePt = 0, int Alignment = 0, int ColorArgb = 0, int BackgroundColorArgb = 0, bool Bold = false, bool Italic = false, bool Underline = false);

/// <summary>
/// 명명 정보 화면(공지 텍스트)을 이름으로 저장/불러오기/삭제한다(레거시 FrmInfoScreen 의 .esi 목록 대응의 첫 슬라이스).
/// 예배 순서·찬양집 스토어와 동일한 JSON 파일·경로 안전 규약을 따른다(무효 문자·예약명·경로 탈출 차단).
/// </summary>
public interface IInfoScreenStore
{
    Task SaveAsync(string name, InfoScreenDto screen, CancellationToken cancellationToken = default);

    Task<InfoScreenDto?> LoadAsync(string name, CancellationToken cancellationToken = default);

    System.Collections.Generic.IReadOnlyList<string> ListNames();

    void Delete(string name);
}

/// <summary>
/// 정보 화면을 JSON 파일로 영속화 — `%AppData%/EasislidesNext/InfoScreens/{name}.json`.
/// 경로 안전은 StoreFileNaming 에 위임(PraiseBookStore 와 동일 패턴). 디렉터리는 생성자 주입 가능(테스트는 임시 폴더).
/// </summary>
public sealed class InfoScreenStore : IInfoScreenStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _directory;

    public InfoScreenStore()
        : this(DefaultDirectory())
    {
    }

    public InfoScreenStore(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
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
        var path = ResolvePath(name);
        if (!File.Exists(path))
        {
            return null;
        }

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

    public System.Collections.Generic.IReadOnlyList<string> ListNames()
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

    private string ResolvePath(string name) => StoreFileNaming.ResolveJsonPath(_directory, name, "정보 화면");
}
