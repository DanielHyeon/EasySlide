using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 저장된 "원시 가사"(EasiSlides 작성용 마크업 포함)를 송출 화면에 보일 "표시용 텍스트"로 바꾼다.
/// 회중 화면에는 작성용 마커가 보이면 안 되므로 다음을 제거/정리한다:
///   - 맨 앞 코드(노테이션) 블록 <c>[~ ... ]</c> (ImportExportService 가 저장 시 쓰는 형식과 동일)
///   - 절/페이지 마커만 있는 줄 (예: <c>[1]</c>, <c>[Chorus]</c>) → 절 경계로 보고 빈 줄로 대체
/// <para>
/// LiveQueueItem.Lyrics 는 편집·저장을 위해 원시 그대로 두고, 출력 경로(GoLive)에서만 이 변환을 적용한다.
/// 즉 "도메인 텍스트(작성 마크업)"와 "표시 텍스트(송출 글자)"를 분리한다.
/// </para>
/// </summary>
public static class LyricsDisplayFormatter
{
    /// <summary>원시 가사를 출력용 표시 텍스트로 변환한다. 비어 있으면 빈 문자열.</summary>
    public static string ToDisplayText(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return string.Empty;
        }

        // 1) 줄바꿈 정규화(\r\n, \r → \n) + 코드 마커 정규화.
        //    일부 데이터는 인코딩 비대칭으로 '»'(U+00BB)가 "Â»"(U+00C2 U+00BB)로 저장될 수 있어 단일 '»'로 모은다.
        var text = rawLyrics
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("Â»", "»", StringComparison.Ordinal);

        // 2) 맨 앞 코드(노테이션) 블록 [~...] 제거.
        var leading = text.TrimStart('\n', ' ', '\t');
        if (leading.StartsWith("[~", StringComparison.Ordinal))
        {
            var end = leading.IndexOf(']');
            if (end > 1)
            {
                text = leading[(end + 1)..];
            }
        }

        // 3) 줄 단위 정리: 마커만 있는 줄·빈 줄은 절 경계로 보고, 본문 절 사이에 빈 줄 하나만 남긴다.
        var output = new StringBuilder();
        var hasContent = false;   // 첫 본문 줄이 나오기 전의 경계는 모두 버린다
        var pendingBlank = false; // 절 경계(빈 줄)는 "다음 본문 줄" 앞에만 삽입
        foreach (var rawLine in text.Split('\n'))
        {
            // 줄 안의 '»'(코드/노테이션 마커) 뒤는 회중 화면에 보이지 않게 잘라낸다 — 가사 본문만 남긴다.
            // (코드는 운영자/연주자용이며, 미리보기에선 흐리게 보여 주지만 출력에선 숨긴다. [~...] 블록 제거와 같은 원칙.)
            var line = StripInlineNotation(rawLine).TrimEnd();
            if (IsMarkerOnlyLine(line) || string.IsNullOrWhiteSpace(line))
            {
                // 본문이 한 번이라도 나온 뒤의 경계만 빈 줄 후보로 기록.
                pendingBlank = hasContent;
                continue;
            }

            if (hasContent)
            {
                output.Append(pendingBlank ? "\n\n" : "\n");
            }

            output.Append(line);
            hasContent = true;
            pendingBlank = false;
        }

        return output.ToString();
    }

    /// <summary>
    /// 가사 전체를 절 단위 페이지 리스트로 분할한다.
    /// 빈 문자열이거나 변환 결과가 없으면 빈 리스트를 반환한다.
    /// </summary>
    public static IReadOnlyList<string> ToVersePages(string? rawLyrics)
    {
        var text = ToDisplayText(rawLyrics);
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<string>();
        }

        // ToDisplayText 가 보장하는 절 구분자는 정확히 \n\n 하나.
        return text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 지정 인덱스(0-based)의 절 텍스트를 반환한다.
    /// 범위 밖은 클램프(0 이하 → 첫 절, 총 절 이상 → 마지막 절). 가사가 없으면 빈 문자열.
    /// </summary>
    public static string GetVersePage(string? rawLyrics, int pageIndex)
    {
        var pages = ToVersePages(rawLyrics);
        if (pages.Count == 0)
        {
            return string.Empty;
        }

        var clamped = Math.Clamp(pageIndex, 0, pages.Count - 1);
        return pages[clamped];
    }

    // 줄에서 '»'(코드/노테이션 마커) 이후를 잘라 본문만 남긴다. 마커가 없으면 줄 그대로.
    // ('»' 정규화는 호출 전에 끝나 있다고 가정 — ToDisplayText 가 "Â»"→"»"로 모은 뒤 호출.)
    private static string StripInlineNotation(string line)
    {
        var markerIndex = line.IndexOf('»'); // '»'
        return markerIndex < 0 ? line : line[..markerIndex];
    }

    // 줄 전체가 하나의 대괄호 토큰인지( 예: [1] [Chorus] [~G D] ) — 작성용 마커로 간주.
    private static bool IsMarkerOnlyLine(string line)
    {
        var t = line.Trim();
        return t.Length >= 2
            && t[0] == '['
            && t[^1] == ']'
            && t.IndexOf(']', 1) == t.Length - 1; // 닫는 대괄호가 끝에 하나뿐(=뒤에 본문 텍스트 없음)
    }
}
