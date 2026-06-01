using System;
using System.Collections.Generic;
using System.Linq;

namespace Easislides.Wpf.Library;

/// <summary>
/// 곡 제목의 머리글자(색인 키)를 구하는 순수 헬퍼 — 한글 초성(ㄱ~ㅎ, 쌍자음은 기본 자음으로)·영문(A~Z)·
/// 숫자(#)·기타로 묶는다. 찬양집 색인(PraiseBookIndexService)과 라이브러리 A/B/C(초성) 점프가 같은 규칙을 쓰도록 공통화한다.
/// DB·파일 접근이 없어 테스트하기 쉽다.
/// </summary>
public static class SongInitialGrouping
{
    // 한글 초성 19개(유니코드 순).
    private static readonly char[] Choseong =
    {
        'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ',
        'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ',
    };

    // 쌍자음 → 기본 자음(ㄲ→ㄱ 등). 색인 머리글자를 14개로 정규화.
    private static readonly Dictionary<char, char> DoubleToBase = new()
    {
        ['ㄲ'] = 'ㄱ', ['ㄸ'] = 'ㄷ', ['ㅃ'] = 'ㅂ', ['ㅆ'] = 'ㅅ', ['ㅉ'] = 'ㅈ',
    };

    // 색인 머리글자 정렬 순서: 한글 자음(가나다) → 영문 A~Z → 숫자(#). "기타"는 맨 뒤.
    private const string GroupOrderText = "ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZ#";

    /// <summary>"기타" 그룹 키(정의된 머리글자에 속하지 않는 제목).</summary>
    public const string OtherKey = "기타";

    /// <summary>제목의 첫 글자로 색인 머리글자를 구한다. 빈 제목이면 "기타".</summary>
    public static string GetInitial(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return OtherKey;
        }

        var c = title.TrimStart()[0];

        // 완성형 한글 음절(가~힣) → 초성 추출 후 쌍자음 정규화.
        if (c >= 0xAC00 && c <= 0xD7A3)
        {
            return Normalize(Choseong[(c - 0xAC00) / 588]).ToString();
        }

        // 한글 자음 낱자(ㄱ~ㅎ) → 그대로(쌍자음은 정규화).
        if (c >= 'ㄱ' && c <= 'ㅎ')
        {
            return Normalize(c).ToString();
        }

        if (char.IsDigit(c))
        {
            return "#";
        }

        if (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
        {
            return char.ToUpperInvariant(c).ToString();
        }

        return OtherKey;
    }

    /// <summary>머리글자 정렬 우선순위 — GroupOrder 에 있으면 그 인덱스, 없으면(기타) 맨 뒤.</summary>
    public static int Rank(string key)
    {
        if (key.Length == 1)
        {
            var idx = GroupOrderText.IndexOf(key[0]);
            if (idx >= 0)
            {
                return idx;
            }
        }

        return int.MaxValue;
    }

    /// <summary>
    /// 주어진 제목들에 실제로 나타나는 머리글자들을 색인 순서(가나다→A~Z→#→기타)로 중복 없이 돌려준다.
    /// 라이브러리 색인 스트립(A/B/C 점프 바)에 보일 글자 목록을 만드는 데 쓴다.
    /// </summary>
    public static IReadOnlyList<string> OrderedDistinctInitials(IEnumerable<string> titles)
    {
        ArgumentNullException.ThrowIfNull(titles);

        return titles
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(GetInitial)
            .Distinct()
            .OrderBy(Rank)
            .ThenBy(k => k, StringComparer.Ordinal)
            .ToList();
    }

    private static char Normalize(char consonant)
        => DoubleToBase.TryGetValue(consonant, out var basic) ? basic : consonant;
}
