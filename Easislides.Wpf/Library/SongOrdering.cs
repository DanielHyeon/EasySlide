using System;
using System.Collections.Generic;
using System.Linq;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

/// <summary>
/// 곡 목록 정렬 방식(FrmMain 곡 목록 정렬 — 곡번호/제목). 기본 Original = DB(폴더) 순서 그대로(무회귀).
/// </summary>
public enum LibrarySortMode
{
    /// <summary>원래 순서(DB·폴더 정렬 그대로). 기본값.</summary>
    Original = 0,

    /// <summary>제목 오름차순(가나다·A~Z). 완성형 한글은 유니코드 순이 가나다 순이라 Ordinal 로 정렬.</summary>
    Title = 1,

    /// <summary>곡 번호 오름차순(같은 번호는 제목 순). 번호 0(없음)은 뒤로.</summary>
    Number = 2,

    /// <summary>
    /// 획수(CJK 한자) 정렬 — 한자를 획수 순으로 묶는다(一·人·三·… 적은 획부터). 번체 중국어(zh-Hant) 콜레이션이
    /// 획수 기반이라 별도 획수 데이터 없이 ICU 가 제공한다. 한글·영문 제목은 그 문화권 순서를 따르므로
    /// 한자 곡이 많은 찬양집에서 운영자가 고른다(기본은 Title=가나다 그대로).
    /// </summary>
    StrokeCount = 3,

    /// <summary>
    /// FrmMain `Folders_WordCount` 체크 상태 — `cjk_wordcount, cjk_strokecount` 순으로 묶는다.
    /// </summary>
    WordCount = 4,
}

/// <summary>정렬 콤보박스 선택지 — 정렬 모드와 사람이 읽는 라벨.</summary>
public sealed record LibrarySortOption(LibrarySortMode Mode, string Label);

/// <summary>곡 목록을 선택한 방식으로 정렬하는 순수 헬퍼 — DB·UI 의존 없이 테스트하기 쉽다.</summary>
public static class SongOrdering
{
    // 획수 정렬용 비교기 — 번체 중국어(zh-Hant) 콜레이션은 한자를 획수 순으로 정렬한다(ICU 제공, 별도 데이터 불필요).
    // 문화권을 못 찾는 드문 런타임에선 Ordinal 로 폴백한다(정렬이 깨지지 않게).
    private static readonly StringComparer StrokeComparer = CreateStrokeComparer();

    private static StringComparer CreateStrokeComparer()
    {
        try
        {
            return StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("zh-Hant"), ignoreCase: false);
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            return StringComparer.Ordinal;
        }
    }

    /// <summary>
    /// 곡들을 <paramref name="mode"/> 로 정렬해 새 리스트로 돌려준다. Original 이면 입력 순서를 그대로 보존한다.
    /// 안정 정렬(OrderBy)이라 같은 키의 상대 순서는 입력 순서를 따른다.
    /// </summary>
    public static IReadOnlyList<SongSummary> Order(IEnumerable<SongSummary> songs, LibrarySortMode mode)
    {
        ArgumentNullException.ThrowIfNull(songs);

        // 제목 비교는 StringComparer.Ordinal — 완성형 한글은 코드포인트 순이 곧 가나다 순이라 의도적으로 Ordinal 을
        // 쓴다(PraiseBookIndexService 색인 정렬과 일관). OrdinalIgnoreCase 로 바꾸면 한글 정렬 일관성이 깨지니 주의.
        return mode switch
        {
            // 제목 오름차순. 빈 제목은 뒤로.
            LibrarySortMode.Title => songs
                .OrderBy(s => string.IsNullOrWhiteSpace(s.Title))
                .ThenBy(s => s.Title, StringComparer.Ordinal)
                .ToList(),

            // 곡 번호 오름차순(번호 0=없음은 뒤로), 같은 번호는 제목 순, 그래도 같으면 SongId 로 결정적 정렬.
            LibrarySortMode.Number => songs
                .OrderBy(s => s.SongNumber <= 0)
                .ThenBy(s => s.SongNumber)
                .ThenBy(s => s.Title, StringComparer.Ordinal)
                .ThenBy(s => s.SongId)
                .ToList(),

            // 획수 정렬 — 한자를 획수 순으로(zh-Hant 콜레이션). 빈 제목은 뒤로, 동률은 SongId 로 결정적.
            LibrarySortMode.StrokeCount => songs
                .OrderBy(s => string.IsNullOrWhiteSpace(s.Title))
                .ThenBy(s => s.Title, StrokeComparer)
                .ThenBy(s => s.SongId)
                .ToList(),

            // FrmMain FillList WordCount 경로 — cjk_wordcount 로 먼저 묶고, 같은 글자 수는 cjk_strokecount 순서.
            LibrarySortMode.WordCount => songs
                .OrderBy(s => string.IsNullOrWhiteSpace(s.Title))
                .ThenBy(s => BuildLegacyCjkWordCountKey(s.Title), StringComparer.Ordinal)
                .ThenBy(s => s.Title, StrokeComparer)
                .ThenBy(s => s.SongId)
                .ToList(),

            // 원래 순서 — 입력 그대로(복사본).
            _ => songs.ToList(),
        };
    }

    private static string BuildLegacyCjkWordCountKey(string title)
    {
        if (string.IsNullOrEmpty(title) || title[0] is > '\0' and < (char)128)
        {
            return "000";
        }

        var length = title.Length;
        var parenthesisIndex = title.IndexOf('(', StringComparison.Ordinal);
        if (parenthesisIndex > 0)
        {
            length = parenthesisIndex - 1;
        }

        var spaceIndex = title.IndexOf(' ', StringComparison.Ordinal);
        if (spaceIndex > 0 && spaceIndex - 1 < length)
        {
            length = spaceIndex - 1;
        }

        return Math.Max(length, 0).ToString("000", System.Globalization.CultureInfo.InvariantCulture);
    }
}
