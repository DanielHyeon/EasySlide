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

            var key = SongInitialGrouping.GetInitial(song.Title);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<PraiseBookIndexEntry>();
                groups[key] = list;
            }

            list.Add(song);
        }

        // 그룹 정렬: 정의된 순서(SongInitialGrouping.Rank) 우선, 그 외(기타)는 맨 뒤. 그룹 내부는 제목순(유니코드=가나다).
        return groups
            .OrderBy(pair => SongInitialGrouping.Rank(pair.Key))
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new PraiseBookIndexGroup(
                pair.Key,
                pair.Value.OrderBy(e => e.Title, StringComparer.Ordinal).ToList()))
            .ToList();
    }
}
