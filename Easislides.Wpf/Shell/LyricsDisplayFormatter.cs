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

        // 1) 줄바꿈·코드 마커 정규화(NormalizeText 단일 규칙).
        var text = NormalizeText(rawLyrics);

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
    /// 절 순서(Sequence)를 적용해 가사를 페이지로 분할한다.
    /// 절을 [라벨] 마커로 1회 정의하고 sequence(예: "1 C 2 C")로 순서·반복을 지정하는 모델(레거시 절 순서 대응).
    /// sequence 가 비었거나 어떤 절 라벨과도 안 맞으면(예: 레거시 char-인코딩) 기존 선형 분할로 안전 폴백한다.
    /// </summary>
    public static IReadOnlyList<string> ToVersePages(string? rawLyrics, string? sequence)
        => TryExpandBySequence(rawLyrics, sequence) ?? ToVersePages(rawLyrics);

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
    /// 지정 인덱스(0-based)의 절 텍스트를 반환한다(절 순서 Sequence 적용).
    /// 범위 밖은 클램프. 가사가 없으면 빈 문자열.
    /// </summary>
    public static string GetVersePage(string? rawLyrics, int pageIndex, string? sequence)
    {
        var pages = ToVersePages(rawLyrics, sequence);
        if (pages.Count == 0)
        {
            return string.Empty;
        }

        return pages[Math.Clamp(pageIndex, 0, pages.Count - 1)];
    }

    /// <summary>
    /// 지정 인덱스(0-based)의 절 텍스트를 반환한다.
    /// 범위 밖은 클램프(0 이하 → 첫 절, 총 절 이상 → 마지막 절). 가사가 없으면 빈 문자열.
    /// </summary>
    public static string GetVersePage(string? rawLyrics, int pageIndex)
        => GetVersePage(rawLyrics, pageIndex, sequence: null);

    /// <summary>
    /// 절 페이지마다의 라벨을 <see cref="ToVersePages(string?,string?)"/> 와 1:1 정렬되게 반환한다(절 라벨 직접 점프용).
    /// 라벨이 없는(빈 줄로만 구분된) 페이지는 빈 문자열. Sequence 적용 시 펼쳐진 순서대로 라벨이 반복된다.
    /// 운영자가 "후렴(C)으로", "3절로" 즉시 이동(레거시 FrmInfoScreen 절 버튼 1~9·c·b 대응)하는 데 쓰인다.
    /// </summary>
    public static IReadOnlyList<string> GetSectionLabels(string? rawLyrics, string? sequence)
    {
        var pages = BuildLabeledPages(rawLyrics, sequence);
        var labels = new string[pages.Count];
        for (var i = 0; i < pages.Count; i++)
        {
            labels[i] = pages[i].Label;
        }

        return labels;
    }

    // 절 페이지를 (라벨, 본문)으로 만든다 — 본문 순서는 ToVersePages 와 동일하도록 보장(정렬 가드 테스트로 잠금).
    private static IReadOnlyList<(string Label, string Content)> BuildLabeledPages(string? rawLyrics, string? sequence)
        => TryExpandLabeledBySequence(rawLyrics, sequence) ?? BuildLinearLabeledPages(rawLyrics);

    // 시퀀스 경로: 각 토큰을 섹션에 매칭 → (섹션 라벨, 본문). 매칭 0이면 null(→ 선형 폴백). TryExpandBySequence 와 동일 매칭 규칙.
    private static IReadOnlyList<(string Label, string Content)>? TryExpandLabeledBySequence(string? rawLyrics, string? sequence)
    {
        if (string.IsNullOrWhiteSpace(sequence))
        {
            return null;
        }

        var sections = ParseLabeledSections(rawLyrics);
        if (sections.Count == 0)
        {
            return null;
        }

        var tokens = sequence.Split([',', ' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var pages = new List<(string Label, string Content)>();
        foreach (var token in tokens)
        {
            foreach (var section in sections)
            {
                if (string.Equals(section.Label, token, StringComparison.OrdinalIgnoreCase)
                    && section.Content.Length > 0)
                {
                    pages.Add((section.Label, section.Content));
                    break;
                }
            }
        }

        return pages.Count > 0 ? pages : null;
    }

    // 선형 경로: ToDisplayText 와 동일한 절 경계 규칙으로 분할하되 각 절의 라벨([X] 마커)을 함께 기록한다.
    // 본문은 ToVersePages(선형)와 동일해야 한다(정렬 가드). 라벨은 절 시작 직전 마지막으로 본 [X] 마커.
    private static IReadOnlyList<(string Label, string Content)> BuildLinearLabeledPages(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return Array.Empty<(string, string)>();
        }

        // ToDisplayText 1~2단계와 동일: 정규화 + 맨 앞 [~...] 노테이션 블록 제거.
        var text = NormalizeText(rawLyrics);
        var leading = text.TrimStart('\n', ' ', '\t');
        if (leading.StartsWith("[~", StringComparison.Ordinal))
        {
            var end = leading.IndexOf(']');
            if (end > 1)
            {
                text = leading[(end + 1)..];
            }
        }

        var pages = new List<(string Label, string Content)>();
        var current = new StringBuilder();
        var pendingLabel = ""; // 가장 최근에 본 [X] 라벨(다음 절의 라벨 후보) — 절 경계를 넘어도 유지.
        var verseLabel = "";   // 현재 모으는 절의 라벨.
        var inVerse = false;

        void Flush()
        {
            if (inVerse && current.Length > 0)
            {
                pages.Add((verseLabel, current.ToString()));
            }

            current.Clear();
            inVerse = false;
        }

        foreach (var rawLine in text.Split('\n'))
        {
            var line = StripInlineNotation(rawLine).TrimEnd();
            var label = SectionLabel(line);
            if (label is not null)
            {
                Flush();              // 새 라벨 = 절 경계.
                pendingLabel = label;
                continue;
            }

            if (IsMarkerOnlyLine(line) || string.IsNullOrWhiteSpace(line))
            {
                Flush();              // [~..]·빈 줄 = 절 경계(라벨은 유지).
                continue;
            }

            if (!inVerse)
            {
                inVerse = true;
                verseLabel = pendingLabel; // 절 시작 시점의 최근 라벨을 절 라벨로.
            }
            else
            {
                current.Append('\n');
            }

            current.Append(line);
        }

        Flush();
        return pages;
    }

    /// <summary>
    /// 중복 정의된 절 라벨을 찾는다(대소문자 무시, 처음 본 표기로 한 번씩, 발견 순서).
    /// Sequence 모델에선 절을 [라벨] 마커로 한 번만 정의해야 하므로(반복은 Sequence 로), 같은 라벨이 두 번 나오면
    /// 둘째 정의가 무시되는 작성 오류다 — SongEditor 가 이 결과로 경고를 띄운다(레거시 FrmInfoScreen 중복 절 검증 대응).
    /// </summary>
    public static IReadOnlyList<string> FindDuplicateSectionLabels(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return Array.Empty<string>();
        }

        var text = NormalizeText(rawLyrics);

        var firstSeen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = new List<string>();
        foreach (var line in text.Split('\n'))
        {
            var label = SectionLabel(line);
            if (label is null)
            {
                continue;
            }

            if (firstSeen.TryGetValue(label, out var original))
            {
                if (reported.Add(label))
                {
                    duplicates.Add(original); // 처음 본 표기로 한 번만 보고.
                }
            }
            else
            {
                firstSeen[label] = label;
            }
        }

        return duplicates;
    }

    /// <summary>
    /// 절 순서(Sequence)로 절을 펼친다. sequence 가 비었거나 매칭되는 절 라벨이 하나도 없으면 null(→ 선형 폴백).
    /// </summary>
    private static IReadOnlyList<string>? TryExpandBySequence(string? rawLyrics, string? sequence)
    {
        if (string.IsNullOrWhiteSpace(sequence))
        {
            return null;
        }

        var sections = ParseLabeledSections(rawLyrics);
        if (sections.Count == 0)
        {
            return null;
        }

        // 시퀀스 토큰: 쉼표/공백 구분, 대소문자 무시로 절 라벨과 매칭.
        var tokens = sequence.Split([',', ' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var pages = new List<string>();
        foreach (var token in tokens)
        {
            foreach (var section in sections)
            {
                if (string.Equals(section.Label, token, StringComparison.OrdinalIgnoreCase)
                    && section.Content.Length > 0)
                {
                    pages.Add(section.Content);
                    break;
                }
            }
        }

        // 매칭이 하나도 없으면(레거시 인코딩 등) 선형으로 폴백.
        return pages.Count > 0 ? pages : null;
    }

    // 가사를 [라벨] 마커 기준 섹션으로 나눈다 — (라벨, 정리된 본문). 마커 앞 본문이나 라벨 없는 구역은 제외(시퀀스 참조 불가).
    private static IReadOnlyList<(string Label, string Content)> ParseLabeledSections(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return Array.Empty<(string, string)>();
        }

        var text = NormalizeText(rawLyrics);

        var sections = new List<(string Label, string Content)>();
        string? currentLabel = null;
        var body = new StringBuilder();

        void Flush()
        {
            if (currentLabel is not null)
            {
                var content = body.ToString().Trim('\n');
                sections.Add((currentLabel, content));
            }

            body.Clear();
        }

        foreach (var rawLine in text.Split('\n'))
        {
            var label = SectionLabel(rawLine);
            if (label is not null)
            {
                Flush();
                currentLabel = label;
                continue;
            }

            if (currentLabel is null)
            {
                continue; // 첫 라벨 마커 전의 줄(라벨 없는 머리말)은 시퀀스로 못 부르므로 건너뜀.
            }

            var line = StripInlineNotation(rawLine).TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue; // 절 안 빈 줄은 모은 뒤 Trim 으로 정리.
            }

            if (body.Length > 0)
            {
                body.Append('\n');
            }

            body.Append(line);
        }

        Flush();
        return sections;
    }

    // 원시 가사 정규화 — 줄바꿈(\r\n, \r → \n)과 코드 마커를 단일 형태로 모은다.
    // 일부 데이터는 인코딩 비대칭으로 '»'(U+00BB)가 "Â»"(U+00C2 U+00BB)로 저장될 수 있어 단일 '»'로 정규화한다.
    // 모든 가사 진입점(표시 텍스트·절 페이지·중복 검사·미리보기 줄 분리)이 동일 규칙을 쓰도록 한 곳에 모았다.
    internal static string NormalizeText(string raw)
        => raw
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("Â»", "»", StringComparison.Ordinal);

    // 마커 줄이 라벨 마커( [1] [Chorus] [C] )면 그 라벨, 노테이션 블록([~...])이나 일반 줄이면 null.
    private static string? SectionLabel(string line)
    {
        if (!IsMarkerOnlyLine(line))
        {
            return null;
        }

        var inner = line.Trim()[1..^1].Trim(); // 대괄호 안.
        return inner.StartsWith('~') || inner.Length == 0 ? null : inner;
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
