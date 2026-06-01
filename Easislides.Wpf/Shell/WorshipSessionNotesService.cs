using System;
using System.IO;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 예배 세션(예배 순서)별 자유 메모를 저장/불러온다(레거시 FrmMain Session Notes 대응).
/// 운영자가 세션마다 진행 메모(순서·전환 안내 등)를 적어 두고 다음에 다시 본다.
/// </summary>
public interface IWorshipSessionNotes
{
    /// <summary>세션 키(예배 순서 이름)의 메모를 돌려준다. 없으면 빈 문자열.</summary>
    string GetNotes(string sessionKey);

    /// <summary>세션 키의 메모를 저장한다(공백뿐이면 파일 삭제). 디스크 오류 등으로 실패하면 false.</summary>
    bool SetNotes(string sessionKey, string notes);
}

/// <summary>
/// 세션 메모를 텍스트 파일로 영속화 — `%AppData%/EasislidesNext/SessionNotes/{key}.txt`.
/// 경로 안전(무효 문자·예약명·경로 탈출 차단)은 공통 StoreFileNaming 규칙을 따른다(.json 확장자 형식 재사용).
/// 읽기/쓰기 실패는 빈 메모/무시로 우아하게 처리(메모는 보조 기능이라 운영을 막지 않는다).
/// </summary>
public sealed class WorshipSessionNotesService : IWorshipSessionNotes
{
    private readonly string _directory;

    public WorshipSessionNotesService()
        : this(DefaultDirectory())
    {
    }

    public WorshipSessionNotesService(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    private static string DefaultDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasislidesNext",
        "SessionNotes");

    public string GetNotes(string sessionKey)
    {
        var path = ResolvePath(sessionKey);
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Empty;
        }
    }

    public bool SetNotes(string sessionKey, string notes)
    {
        var path = ResolvePath(sessionKey);
        try
        {
            // 공백뿐인 메모는 파일을 지워 둔다(다음 Get 은 빈 문자열 반환) — 뷰모델의 "비웠습니다" 판정과 일치.
            if (string.IsNullOrWhiteSpace(notes))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return true;
            }

            Directory.CreateDirectory(_directory);
            File.WriteAllText(path, notes);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 디스크 오류 등 저장 실패 — false 로 알려 뷰모델이 "실패" 상태를 보이게 한다(거짓 성공 방지).
            return false;
        }
    }

    // 세션 키를 안전한 파일 경로로 — StoreFileNaming 의 .json 경로를 받아 확장자만 .txt 로 바꾼다.
    private string ResolvePath(string sessionKey)
    {
        var jsonPath = StoreFileNaming.ResolveJsonPath(_directory, sessionKey, "세션 메모");
        return Path.ChangeExtension(jsonPath, ".txt");
    }
}
