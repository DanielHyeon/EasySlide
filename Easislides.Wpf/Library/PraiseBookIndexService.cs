using System;
using System.Collections.Generic;
using System.Linq;

namespace Easislides.Wpf.Library;

/// <summary>찬양집 색인의 한 그룹 — 머리글자(초성/영문/숫자/기타)와 그 그룹에 속한 곡들.</summary>
public sealed record PraiseBookIndexGroup(string Key, IReadOnlyList<PraiseBookIndexEntry> Entries);

/// <summary>찬양집 색인 항목 — 곡 제목과 곡 번호(0이면 번호 없음).</summary>
public sealed record PraiseBookIndexEntry(string Title, int Number);

/// <summary>
/// 찬양집 색인 생성기(FrmMain "Listing of Selected Folder"/PraiseBook CJK 그룹핑 포팅).
/// 곡 제목의 머리글자로 한글 초성(ㄱ~ㅎ)·영문(A~Z)·숫자(#)·기타로 묶어 가나다 순 색인을 만든다.
/// 순수 함수형(파일·DB 접근 없음) — 그룹화 규칙만 담아 테스트하기 쉽다.
/// </summary>
public interface IPraiseBookIndexService
{
    IReadOnlyList<PraiseBookIndexGroup> BuildIndex(IEnumerable<PraiseBookIndexEntry> songs);
}

public sealed class PraiseBookIndexService : IPraiseBookIndexService
{
    // 한글 초성 19개(유니코드 순). 색인은 쌍자음을 기본 자음으로 합쳐 14개로 묶는다(가나다 색인 관례).
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

    // 색인 머리글자 정렬 순서: 한글 자음(가나다) → 영문 A~Z → 숫자(#) → 기타.
    private static readonly string GroupOrder = "ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZ#";

    public IReadOnlyList<PraiseBookIndexGroup> BuildIndex(IEnumerable<PraiseBookIndexEntry> songs)
    {
        ArgumentNullException.ThrowIfNull(songs);

        var groups = new Dictionary<string, List<PraiseBookIndexEntry>>();
        foreach (var song in songs)
        {
            if (song is null || string.IsNullOrWhiteSpace(song.Title))
            {
                continue;
            }

            var key = GetGroupKey(song.Title);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<PraiseBookIndexEntry>();
                groups[key] = list;
            }

            list.Add(song);
        }

        // 그룹 정렬: 정의된 순서(GroupOrder) 우선, 그 외(기타)는 맨 뒤. 그룹 내부는 제목순(유니코드=가나다).
        return groups
            .OrderBy(pair => GroupRank(pair.Key))
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new PraiseBookIndexGroup(
                pair.Key,
                pair.Value.OrderBy(e => e.Title, StringComparer.Ordinal).ToList()))
            .ToList();
    }

    // 제목의 첫 글자로 색인 머리글자를 구한다.
    private static string GetGroupKey(string title)
    {
        var c = title.TrimStart()[0];

        // 완성형 한글 음절(가~힣) → 초성 추출 후 쌍자음 정규화.
        if (c >= 0xAC00 && c <= 0xD7A3)
        {
            var cho = Choseong[(c - 0xAC00) / 588];
            return Normalize(cho).ToString();
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

        return "기타";
    }

    private static char Normalize(char consonant)
        => DoubleToBase.TryGetValue(consonant, out var basic) ? basic : consonant;

    // 머리글자 정렬 우선순위 — GroupOrder 에 있으면 그 인덱스, 없으면(기타) 맨 뒤.
    private static int GroupRank(string key)
    {
        if (key.Length == 1)
        {
            var idx = GroupOrder.IndexOf(key[0]);
            if (idx >= 0)
            {
                return idx;
            }
        }

        return int.MaxValue;
    }
}
