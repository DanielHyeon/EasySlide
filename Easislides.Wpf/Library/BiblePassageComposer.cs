using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Wpf.Library;

/// <summary>
/// 성경 구절들을 회중 화면 출력용 가사 문자열로 조립한다(순수 함수).
/// 절마다 한 페이지(빈 줄로 경계)를 만들어 기존 절 단위 페이지네이션(절 이동·위치 라벨)을 그대로 쓰고,
/// 보조 언어(Region 2)가 있으면 같은 페이지 안에 <c>[region 2]</c> 마커로 함께 실어
/// 이미 있는 이중 언어 출력 렌더(Region1/Region2 밴드, 영역별 폰트·정렬·색)를 재사용한다.
/// </summary>
public static class BiblePassageComposer
{
    /// <summary>
    /// 주 언어 절 목록과 보조 언어 절 목록을 절 단위로 짝지어 조립한다.
    /// 보조 목록이 비면(단일 언어) 마커 없이 절만 빈 줄로 나눠 단일 밴드로 렌더된다(이중 언어 출력 무영향).
    /// 두 목록 길이가 달라도(보조 버전에 없는 절 등) 더 긴 쪽까지 만들고 모자란 쪽은 빈 칸으로 둔다(절 어긋남 방어).
    /// </summary>
    public static string Compose(IReadOnlyList<string> region1Verses, IReadOnlyList<string> region2Verses)
    {
        ArgumentNullException.ThrowIfNull(region1Verses);
        ArgumentNullException.ThrowIfNull(region2Verses);

        var count = Math.Max(region1Verses.Count, region2Verses.Count);
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                sb.Append("\n\n"); // 빈 줄 = 절(페이지) 경계.
            }

            var region1 = i < region1Verses.Count ? region1Verses[i] ?? "" : "";
            sb.Append(region1);

            var region2 = i < region2Verses.Count ? region2Verses[i] ?? "" : "";
            if (region2.Length > 0)
            {
                // 같은 페이지 안에서 [region 2] 아래 줄부터 보조 언어 — 출력이 Region2 밴드로 동시 송출.
                sb.Append("\n[region 2]\n").Append(region2);
            }
        }

        return sb.ToString();
    }
}
