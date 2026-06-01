using System;
using System.Collections.Generic;

namespace Easislides.Wpf.Library;

/// <summary>
/// 성경 선택 IdString 안의 한 구절 범위 — 책 번호와 시작/끝 (장:절).
/// 순차 선택은 장을 넘나드는 범위, 임의 선택은 시작=끝(한 절)이다.
/// </summary>
public sealed record BiblePassageRange(int Book, int StartChapter, int StartVerse, int EndChapter, int EndVerse)
{
    /// <summary>
    /// (장,절)이 이 범위 안(시작 이상·끝 이하)인지 — 회중 화면에 실을 구절을 고르는 기준.
    /// 장을 먼저 비교하고 같은 장이면 절로 비교한다.
    /// </summary>
    public bool Contains(int chapter, int verse)
    {
        var afterStart = chapter > StartChapter || (chapter == StartChapter && verse >= StartVerse);
        var beforeEnd = chapter < EndChapter || (chapter == EndChapter && verse <= EndVerse);
        return afterStart && beforeEnd;
    }
}

/// <summary>
/// 성경 선택 IdString("0;region1;region2;book;시작장;시작절;끝장;끝절;…")을 구조로 파싱한 결과(순수 함수).
/// IdString 은 자기완결적이다 — 주/보조 버전 파일명과 구절 범위를 모두 담아, 어떤 선택 경로(드래그·typed·모달)로
/// 만들었든 같은 한 입력으로 출력 본문을 확장할 수 있다(BibleRepository.ExpandSelection 의 입력).
/// </summary>
public sealed record BibleSelectionId(
    bool IsSequential,
    string Region1FileName,
    string Region2FileName,
    IReadOnlyList<BiblePassageRange> Ranges)
{
    /// <summary>보조 언어(Region 2) 버전이 지정돼 있으면 true — 회중 화면에 두 본문을 함께 송출(이중 언어).</summary>
    public bool HasRegion2 => Region2FileName.Length > 0;

    /// <summary>
    /// IdString 을 파싱한다. 머리(종류;주버전;보조버전) 뒤로 5개씩(책;시작장;시작절;끝장;끝절) 구절 범위가 반복된다.
    /// 끝의 빈 토큰(후행 ';')은 무시한다. 형식이 어긋나면(토큰 부족·숫자 아님·주버전 없음·범위 0개) false.
    /// </summary>
    public static bool TryParse(string? idString, out BibleSelectionId parsed)
    {
        parsed = new BibleSelectionId(true, "", "", Array.Empty<BiblePassageRange>());
        if (string.IsNullOrWhiteSpace(idString))
        {
            return false;
        }

        var parts = idString.Split(';');
        // 머리 3토큰(종류;주버전;보조버전) + 적어도 한 구절(5토큰) = 최소 8토큰.
        if (parts.Length < 8)
        {
            return false;
        }

        // 종류: 0=순차(장 범위 가능), 1=임의(절 단위). 그 외 값은 형식 오류로 본다.
        var kind = parts[0];
        if (kind != "0" && kind != "1")
        {
            return false;
        }

        var region1 = parts[1].Trim();
        var region2 = parts[2].Trim();
        if (region1.Length == 0)
        {
            return false; // 주 버전(Region 1) 파일명은 반드시 있어야 한다.
        }

        var ranges = new List<BiblePassageRange>();
        // parts[3]부터 5개씩 끊어 구절 범위를 읽는다. i+4 까지 유효해야 한 묶음을 읽을 수 있다.
        for (var i = 3; i + 4 < parts.Length; i += 5)
        {
            if (!int.TryParse(parts[i], out var book) ||
                !int.TryParse(parts[i + 1], out var startChapter) ||
                !int.TryParse(parts[i + 2], out var startVerse) ||
                !int.TryParse(parts[i + 3], out var endChapter) ||
                !int.TryParse(parts[i + 4], out var endVerse))
            {
                return false; // 숫자가 아닌 토큰 = 형식 오류.
            }

            ranges.Add(new BiblePassageRange(book, startChapter, startVerse, endChapter, endVerse));
        }

        if (ranges.Count == 0)
        {
            return false;
        }

        parsed = new BibleSelectionId(kind == "0", region1, region2, ranges);
        return true;
    }
}
