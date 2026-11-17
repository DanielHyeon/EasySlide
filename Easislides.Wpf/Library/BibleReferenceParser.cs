using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Easislides.Wpf.Library;

/// <summary>
/// 타이핑한 성경 구절(예: "창 1:1", "창 1:1-5", "창 1:1-2:3", "John 3:16", "1요한 4:7-8")을
/// 책 토큰 + 시작/끝 (장:절)로 파싱한다(레거시 FrmMain 인라인 성경의 BibleUserLookup 대응).
/// <para>
/// 순수 함수 — DB·VM 에 의존하지 않아 단위 테스트가 쉽다. 책 토큰을 실제 책으로 푸는 일은
/// <see cref="ResolveBook"/> 가, 본문 로드·범위 선택은 BibleViewModel 이 맡는다.
/// </para>
/// </summary>
public sealed record BibleReference(string BookToken, int StartChapter, int StartVerse, int EndChapter, int EndVerse)
{
    /// <summary>시작과 끝이 같은 한 구절인지.</summary>
    public bool IsSingleVerse => StartChapter == EndChapter && StartVerse == EndVerse;
}

public static class BibleReferenceParser
{
    // 끝에서부터 "장:절[-[장:]절]" 숫자 꼬리를 잡는다. 구분자는 ':' 또는 '.' 허용(한글/영문 입력 관용).
    //  - book : 숫자 꼬리 앞 전부(책 이름. "1요한"·"2 Samuel"처럼 숫자로 시작해도 됨 — 꼬리를 끝에서 잡으므로 안전)
    //  - sc:sv : 시작 장:절,  (선택) "- [ec:]ev" : 끝.  끝 장(ec)이 없으면 시작 장과 같은 장으로 본다.
    private static readonly Regex Pattern = new(
        @"^\s*(?<book>.*?)\s*(?<sc>\d+)\s*[:.]\s*(?<sv>\d+)(?:\s*-\s*(?:(?<ec>\d+)\s*[:.]\s*)?(?<ev>\d+))?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// 입력 문자열을 성경 구절 참조로 파싱한다. 성공하면 true 와 <paramref name="reference"/> 를 돌려준다.
    /// 책 토큰이 비었거나, 장·절이 1 미만이거나, 끝이 시작보다 앞서면 실패(false).
    /// </summary>
    public static bool TryParse(string? input, out BibleReference reference)
    {
        reference = new BibleReference("", 0, 0, 0, 0);
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var match = Pattern.Match(input);
        if (!match.Success)
        {
            return false;
        }

        var book = match.Groups["book"].Value.Trim();
        if (book.Length == 0)
        {
            return false; // 책 이름 없이 장:절만 있으면(예: "3:16") 거부 — 어느 책인지 알 수 없음.
        }

        var startChapter = ToInt(match.Groups["sc"].Value);
        var startVerse = ToInt(match.Groups["sv"].Value);

        // 끝 지정이 없으면 시작과 같은 한 구절. 끝 장(ec)이 없고 끝 절(ev)만 있으면 같은 장 안의 절 범위.
        var hasEndVerse = match.Groups["ev"].Success;
        var endChapter = match.Groups["ec"].Success ? ToInt(match.Groups["ec"].Value)
            : hasEndVerse ? startChapter
            : startChapter;
        var endVerse = hasEndVerse ? ToInt(match.Groups["ev"].Value) : startVerse;

        // 장·절은 1 이상, 끝은 시작 이상(장 먼저, 같으면 절)이어야 한다.
        if (startChapter < 1 || startVerse < 1 || endChapter < 1 || endVerse < 1)
        {
            return false;
        }

        if (endChapter < startChapter || (endChapter == startChapter && endVerse < startVerse))
        {
            return false;
        }

        reference = new BibleReference(book, startChapter, startVerse, endChapter, endVerse);
        return true;
    }

    /// <summary>
    /// 책 토큰을 실제 <see cref="BibleBook"/> 로 푼다(현재 버전의 책 목록 기준 — 레거시도 버전별 책 목록과 대조).
    /// 매칭 우선순위: ① 숫자면 책 번호 ② 이름 정확히 일치(대소문자·공백 무시) ③ 이름이 토큰으로 시작
    /// ④ 이름에 토큰 포함. 못 찾으면 null. 토큰의 공백은 무시해 "1 John"과 "1John"을 같게 본다.
    /// 같은 등급(③/④)에서 여러 책이 걸리면 <b>목록 순서상 먼저 나온 책</b>을 고른다(정경 책 목록은 정확 일치 등급에서
    /// 대부분 결정되어 모호함이 드물다).
    /// </summary>
    public static BibleBook? ResolveBook(IEnumerable<BibleBook> books, string? token)
    {
        ArgumentNullException.ThrowIfNull(books);
        var needle = Collapse(token);
        if (needle.Length == 0)
        {
            return null;
        }

        // 숫자만이면 책 번호로 바로 찾는다(예: "1" → 첫 책).
        if (int.TryParse(needle, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bookNumber))
        {
            foreach (var book in books)
            {
                if (book.Number == bookNumber)
                {
                    return book;
                }
            }

            return null;
        }

        BibleBook? startsWith = null;
        BibleBook? contains = null;
        foreach (var book in books)
        {
            var name = Collapse(book.Name);
            if (string.Equals(name, needle, StringComparison.OrdinalIgnoreCase))
            {
                return book; // 정확히 일치 — 최우선.
            }

            if (startsWith is null && name.StartsWith(needle, StringComparison.OrdinalIgnoreCase))
            {
                startsWith = book;
            }
            else if (contains is null && name.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                contains = book;
            }
        }

        return startsWith ?? contains;
    }

    private static int ToInt(string value)
        => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    // 공백을 모두 제거하고 trim — "1 John"·"1John"·" 1 john " 를 같은 비교 키로 만든다.
    private static string Collapse(string? value)
        => string.IsNullOrWhiteSpace(value) ? "" : value.Replace(" ", "", StringComparison.Ordinal).Trim();
}
