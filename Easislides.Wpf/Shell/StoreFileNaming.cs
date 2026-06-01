using System;
using System.IO;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 이름 기반 JSON 파일 스토어의 공통 경로 안전 규칙(WorshipListStore·PraiseBookStore 공유).
/// 사용자 입력 이름을 파일명으로 쓰므로 무효 문자·예약명·경로 탈출(.. 등)을 차단해
/// 의도한 디렉터리 바로 밑의 "{name}.json" 만 허용한다(보안 중요 — 중복 구현 방지로 한 곳에 모음).
/// </summary>
internal static class StoreFileNaming
{
    /// <summary>이름을 안전한 "{directory}/{name}.json" 절대 경로로 해석. 위반 시 ArgumentException.</summary>
    /// <param name="kindLabel">오류 메시지에 쓸 대상 이름(예: "예배 순서", "찬양집").</param>
    public static string ResolveJsonPath(string directory, string name, string kindLabel)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"{kindLabel} 이름이 비어 있습니다.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 100
            || trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.EndsWith('.')
            || IsReservedDeviceName(trimmed))
        {
            throw new ArgumentException($"{kindLabel} 이름에 사용할 수 없는 문자/형식입니다: {name}", nameof(name));
        }

        // 정규화 후 의도한 디렉터리 바로 밑인지 최종 확인(블랙리스트 우회까지 막는 화이트리스트 방어선).
        var full = Path.GetFullPath(Path.Combine(directory, trimmed + ".json"));
        var root = Path.GetFullPath(directory);
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{kindLabel} 이름이 허용 경로를 벗어납니다: {name}", nameof(name));
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
