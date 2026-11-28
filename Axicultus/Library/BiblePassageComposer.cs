using System;
using System.Collections.Generic;
using System.Text;

namespace Axicultus.Wpf.Library;

/// <summary>
/// 성경 구절들을 회중 화면 출력용 가사 문자열로 조립한다(순수 함수).
/// A single-language passage is one block with a verse on each line, which the page layout flows over as many
/// slides as it needs. With a second language (Region 2) every verse is a page of its own (a blank line ends it) that
/// carries both texts around a <c>[region 2]</c> marker, for the existing two-region output.
/// </summary>
public static class BiblePassageComposer
{
    /// <summary>
    /// 주 언어 절 목록과 보조 언어 절 목록을 절 단위로 짝지어 조립한다.
    /// With no second list (one language) the passage is one block, a line per verse, without markers.
    /// 두 목록 길이가 달라도(보조 버전에 없는 절 등) 더 긴 쪽까지 만들고 모자란 쪽은 빈 칸으로 둔다(절 어긋남 방어).
    /// </summary>
    public static string Compose(IReadOnlyList<string> region1Verses, IReadOnlyList<string> region2Verses)
    {
        ArgumentNullException.ThrowIfNull(region1Verses);
        ArgumentNullException.ThrowIfNull(region2Verses);

        var count = Math.Max(region1Verses.Count, region2Verses.Count);
        var dual = false;
        foreach (var verse in region2Verses)
        {
            dual |= !string.IsNullOrEmpty(verse);
        }

        var sb = new StringBuilder();
        if (!dual)
        {
            // One block, a verse on each line: the WinForms app sends a passage as one text that flows over as
            // many slides as it needs (gfLyrics.FitWholeLyricPage exempts type "B"), so no verse ends a page.
            foreach (var verse in region1Verses)
            {
                if (string.IsNullOrEmpty(verse))
                {
                    continue; // an empty line would be a page break
                }

                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }

                sb.Append(verse);
            }

            return sb.ToString();
        }

        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                sb.Append("\n\n"); // 빈 줄 = 절(페이지) 경계.
            }

            var region1 = i < region1Verses.Count ? region1Verses[i] ?? "" : "";
            sb.Append(region1);

            // On the same page the lines under [region 2] are the second language: the output shows them in its Region2 band at the same time.
            // Every verse carries the marker, also one the second version lacks: a passage with one marker
            // only would read like a WinForms song whose region 2 runs on to the end.
            var region2 = i < region2Verses.Count ? region2Verses[i] ?? "" : "";
            sb.Append("\n[region 2]\n").Append(region2);
        }

        return sb.ToString();
    }
}
