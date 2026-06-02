using System;

namespace Easislides.Wpf.Library;

/// <summary>
/// 파일 목록 검색용 순수 필터 — 파일명에 검색어가 들어 있는지(대소문자 무시) 판단한다.
/// 미디어·PowerPoint 폴더 브라우저의 "검색" 상자가 함께 쓴다(중복 방지). 검색어가 비면 모두 통과(필터 없음).
/// <para>순수 함수라 IO 없이 단위 테스트가 쉽다 — View 가 넘긴 파일명/검색어만 비교한다.</para>
/// </summary>
public static class FileNameFilter
{
    /// <summary>
    /// <paramref name="fileName"/> 이 <paramref name="filter"/> 를 포함하는지(대소문자 무시) 판단한다.
    /// 검색어가 null/공백뿐이면 항상 true(필터 없음 = 모두 보임). 앞뒤 공백은 다듬어 비교한다.
    /// </summary>
    public static bool Matches(string? fileName, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true; // 검색어 없으면 모두 통과.
        }

        var needle = filter.Trim();
        return (fileName ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase);
    }
}
