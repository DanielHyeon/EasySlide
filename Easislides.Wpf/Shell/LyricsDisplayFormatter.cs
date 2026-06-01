using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 이중 언어 절 페이지 — Region1(주 언어)·Region2(보조 언어) 한 쌍.
/// 가사에 <c>[region 2]</c> 마커가 없으면 Region2 는 빈 문자열(단일 영역).
/// </summary>
public sealed record LyricsRegionPage(string Region1, string Region2);

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
    /// 이중 언어(Region 1/2) 절 페이지 — 절 안에서 <c>[region 2]</c> 마커 앞은 Region1, 뒤는 Region2.
    /// 같은 페이지에 두 영역이 짝지어져 한 화면에 동시 송출(레거시 이중 언어/병렬 본문 대응)된다.
    /// <c>[region 2]</c> 가 없으면 Region2 는 빈 문자열이고 Region1 은 <see cref="ToVersePages(string?)"/> 와 동일하다(단일 영역 무회귀).
    /// </summary>
    public static IReadOnlyList<LyricsRegionPage> GetRegionPages(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return Array.Empty<LyricsRegionPage>();
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

        var pages = new List<LyricsRegionPage>();
        var region1 = new StringBuilder();
        var region2 = new StringBuilder();
        var currentRegion = 1; // 절 시작 시 항상 region1.
        var inVerse = false;

        void Flush()
        {
            if (inVerse && (region1.Length > 0 || region2.Length > 0))
            {
                pages.Add(new LyricsRegionPage(region1.ToString(), region2.ToString()));
            }

            region1.Clear();
            region2.Clear();
            currentRegion = 1;
            inVerse = false;
        }

        foreach (var rawLine in text.Split('\n'))
        {
            var line = StripInlineNotation(rawLine).TrimEnd();

            // [region 2]·[region 1] 영역 전환 마커 — 절 경계가 아니며(같은 절 안), 본문에 안 보인다.
            if (IsRegionMarker(line, 2))
            {
                currentRegion = 2;
                continue;
            }

            if (IsRegionMarker(line, 1))
            {
                currentRegion = 1;
                continue;
            }

            // 절 라벨([1],[C] 등) = 절 경계(다음 절은 다시 region1 부터).
            if (SectionLabel(line) is not null)
            {
                Flush();
                continue;
            }

            // [~..] 노테이션·빈 줄 = 절 경계(라벨 없는 절 구분).
            if (IsMarkerOnlyLine(line) || string.IsNullOrWhiteSpace(line))
            {
                Flush();
                continue;
            }

            inVerse = true;
            var target = currentRegion == 2 ? region2 : region1;
            if (target.Length > 0)
            {
                target.Append('\n');
            }

            target.Append(line);
        }

        Flush();
        return pages;
    }

    /// <summary>
    /// 가사에 region2 <b>본문(내용)</b>이 하나라도 있으면 true(= 이중 언어 곡). 마커 유무가 아니라 내용 유무로 판정 —
    /// <c>[region 2]</c> 마커만 있고 뒤에 글자가 없으면 false(단일 영역으로 안전 폴백).
    /// </summary>
    public static bool HasRegion2(string? rawLyrics)
    {
        foreach (var page in GetRegionPages(rawLyrics))
        {
            if (page.Region2.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>지정 인덱스(0-based, 범위 밖 클램프)의 이중 언어 절 페이지. 가사가 없으면 빈 페이지.</summary>
    public static LyricsRegionPage GetRegionPage(string? rawLyrics, int pageIndex)
        => GetRegionPage(rawLyrics, pageIndex, sequence: null);

    /// <summary>
    /// 절 순서(Sequence)를 적용한 이중 언어 절 페이지 — 절을 [라벨]로 1회 정의하고 sequence("1 C 2 C")로 순서·반복을 지정.
    /// sequence 가 비었거나 어떤 라벨과도 안 맞으면 선형(GetRegionPages(lyrics))으로 안전 폴백(단일 영역 모델과 동일 규칙).
    /// </summary>
    public static IReadOnlyList<LyricsRegionPage> GetRegionPages(string? rawLyrics, string? sequence)
        => TryExpandRegionBySequence(rawLyrics, sequence) ?? GetRegionPages(rawLyrics);

    /// <summary>지정 인덱스(범위 밖 클램프)의 이중 언어 절 페이지(절 순서 Sequence 적용). 가사가 없으면 빈 페이지.</summary>
    public static LyricsRegionPage GetRegionPage(string? rawLyrics, int pageIndex, string? sequence)
    {
        var pages = GetRegionPages(rawLyrics, sequence);
        if (pages.Count == 0)
        {
            return new LyricsRegionPage(string.Empty, string.Empty);
        }

        return pages[Math.Clamp(pageIndex, 0, pages.Count - 1)];
    }

    /// <summary>
    /// 이중 언어 절 페이지의 라벨 목록 — 라벨이 있는(시퀀스 또는 전부 라벨링된) 곡에서 절 점프에 쓴다.
    /// 정렬 보장이 어려운 경우(라벨 없는 머리말 등)는 호출자가 GetRegionPages 수와 비교해 가드한다(불일치면 점프 비활성).
    /// </summary>
    public static IReadOnlyList<string> GetRegionSectionLabels(string? rawLyrics, string? sequence)
    {
        var sections = ParseRegionSections(rawLyrics);
        if (sections.Count == 0)
        {
            return Array.Empty<string>();
        }

        // 시퀀스: 토큰 매칭 섹션의 라벨(펼친 순서).
        if (!string.IsNullOrWhiteSpace(sequence))
        {
            var tokens = sequence.Split([',', ' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            var seqLabels = new List<string>();
            foreach (var token in tokens)
            {
                foreach (var section in sections)
                {
                    if (string.Equals(section.Label, token, StringComparison.OrdinalIgnoreCase)
                        && (section.Region1.Length > 0 || section.Region2.Length > 0))
                    {
                        seqLabels.Add(section.Label);
                        break;
                    }
                }
            }

            if (seqLabels.Count > 0)
            {
                return seqLabels;
            }
        }

        // 선형: 내용이 있는 라벨 섹션의 라벨(순서대로) — 전부 라벨링된 곡이면 GetRegionPages 와 1:1.
        var labels = new List<string>();
        foreach (var section in sections)
        {
            if (section.Region1.Length > 0 || section.Region2.Length > 0)
            {
                labels.Add(section.Label);
            }
        }

        return labels;
    }

    // 시퀀스로 이중 언어 절을 펼친다. sequence 가 비었거나 매칭 라벨이 없으면 null(→ 선형 폴백). TryExpandBySequence 와 동일 규칙.
    private static IReadOnlyList<LyricsRegionPage>? TryExpandRegionBySequence(string? rawLyrics, string? sequence)
    {
        if (string.IsNullOrWhiteSpace(sequence))
        {
            return null;
        }

        var sections = ParseRegionSections(rawLyrics);
        if (sections.Count == 0)
        {
            return null;
        }

        var tokens = sequence.Split([',', ' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var pages = new List<LyricsRegionPage>();
        foreach (var token in tokens)
        {
            foreach (var section in sections)
            {
                if (string.Equals(section.Label, token, StringComparison.OrdinalIgnoreCase)
                    && (section.Region1.Length > 0 || section.Region2.Length > 0))
                {
                    pages.Add(new LyricsRegionPage(section.Region1, section.Region2));
                    break;
                }
            }
        }

        return pages.Count > 0 ? pages : null;
    }

    // 가사를 [라벨] 섹션으로 나누되 각 섹션 본문을 [region 2] 기준으로 Region1/Region2 로 분리한다(이중 언어 시퀀스 확장용).
    // 라벨 없는 머리말·라벨 전 줄은 제외(시퀀스로 참조 불가). ParseLabeledSections 의 region-aware 판.
    private static IReadOnlyList<(string Label, string Region1, string Region2)> ParseRegionSections(string? rawLyrics)
    {
        if (string.IsNullOrWhiteSpace(rawLyrics))
        {
            return Array.Empty<(string, string, string)>();
        }

        var text = NormalizeText(rawLyrics);
        var sections = new List<(string Label, string Region1, string Region2)>();
        string? currentLabel = null;
        var region1 = new StringBuilder();
        var region2 = new StringBuilder();
        var currentRegion = 1;

        void Flush()
        {
            if (currentLabel is not null)
            {
                sections.Add((currentLabel, region1.ToString().Trim('\n'), region2.ToString().Trim('\n')));
            }

            region1.Clear();
            region2.Clear();
            currentRegion = 1; // 새 절은 다시 region1.
        }

        foreach (var rawLine in text.Split('\n'))
        {
            var line = StripInlineNotation(rawLine).TrimEnd();

            if (IsRegionMarker(line, 2))
            {
                currentRegion = 2;
                continue;
            }

            if (IsRegionMarker(line, 1))
            {
                currentRegion = 1;
                continue;
            }

            var label = SectionLabel(line);
            if (label is not null)
            {
                Flush();
                currentLabel = label;
                continue;
            }

            if (currentLabel is null)
            {
                continue; // 첫 라벨 전의 줄은 시퀀스로 못 부르므로 건너뜀.
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue; // 절 안 빈 줄은 모은 뒤 Trim 으로 정리.
            }

            var target = currentRegion == 2 ? region2 : region1;
            if (target.Length > 0)
            {
                target.Append('\n');
            }

            target.Append(line);
        }

        Flush();
        return sections;
    }

    // 줄이 [region N] 영역 전환 마커인지(대소문자·공백 무시: "[region 2]"·"[Region2]" 모두 인식).
    private static bool IsRegionMarker(string line, int regionNumber)
    {
        if (!IsMarkerOnlyLine(line))
        {
            return false;
        }

        var inner = line.Trim()[1..^1].Trim().ToLowerInvariant().Replace(" ", "", StringComparison.Ordinal);
        return inner == $"region{regionNumber}";
    }

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

    /// <summary>
    /// 코드(악상) 표시 전처리 — "코드 표시"(Show Notations) 설정이 켜져 있으면 각 가사 줄의
    /// '»' 뒤 코드를 가사 줄 "위"에 별도 줄로 끼워 넣어, 회중 화면에 코드가 함께 보이게 한다(레거시 ShowNotations 대응).
    /// <para>
    /// 끄면(기본) 원시 가사를 <b>그대로</b> 돌려준다 — 이후 파이프라인의 기존 코드-숨김(StripInlineNotation)이
    /// 동작해 비트 동일(무회귀)하다. 켜면 <c>가사 » 코드</c> 한 줄을 <c>코드\n가사</c> 두 줄로 바꾼다.
    /// </para>
    /// <para>
    /// 절 경계(빈 줄·[마커] 줄)는 손대지 않으므로 절(페이지) 수가 켜고/끄고에 상관없이 같다 —
    /// MainViewModel 의 페이지 수 계산(원시 가사 기준)과 본문(전처리 가사 기준)이 어긋나지 않는다.
    /// 그래서 빈 가사·빈 코드 줄은 코드 줄을 넣지 않고 가사만 남겨(끈 것과 동일한 경계) 패리티를 지킨다.
    /// </para>
    /// <para>
    /// <paramref name="transposeSemitones"/>(라이브 조옮김, 레거시 Transpose ±Semi-Tone)가 0 이 아니면 끼워 넣는
    /// 코드 줄을 그만큼 반음 이동한다(가사는 불변). 코드 표시가 꺼져 있으면 코드가 안 보이므로 효과가 없다.
    /// </para>
    /// </summary>
    public static string ExpandNotations(string? rawLyrics, bool showNotations, int transposeSemitones = 0)
    {
        // 꺼져 있으면 아무것도 바꾸지 않는다 — 기존 파이프라인이 그대로 코드를 숨겨 무회귀.
        if (!showNotations || string.IsNullOrEmpty(rawLyrics))
        {
            return rawLyrics ?? string.Empty;
        }

        var text = NormalizeText(rawLyrics); // 줄바꿈·'Â»' 통일 후 줄 단위로 처리.
        var sb = new StringBuilder(text.Length + 16);
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                sb.Append('\n');
            }

            var line = lines[i];
            var markerIndex = line.IndexOf('»');
            if (markerIndex < 0)
            {
                sb.Append(line); // 코드 마커가 없는 줄(가사·[마커]·빈 줄)은 그대로.
                continue;
            }

            var lyric = line[..markerIndex].TrimEnd();
            var chord = line[(markerIndex + 1)..].Trim();
            // 코드 줄을 넣지 않고 가사 부분만 남기는 경우(= RAW 의 StripInlineNotation 경계 처리와 동일):
            //  ① 가사 부분이 비었거나(코드 전용 줄) ② 코드가 비었거나
            //  ③ 가사 부분이 작성 마커( [1] » G 같은 드문 오작성 )이면 — 절 경계 판정이 RAW 와 같아야
            //     페이지 수(원시 기준)와 본문(전처리 기준)이 어긋나지 않는다.
            if (lyric.Length == 0 || chord.Length == 0 || IsMarkerOnlyLine(lyric))
            {
                sb.Append(lyric);
                continue;
            }

            // 라이브 조옮김이 걸려 있으면 코드만 ±반음 이동(가사·저장 데이터는 불변). 0 이면 원본 코드 그대로.
            var shownChord = transposeSemitones == 0 ? chord : ChordTransposer.TransposeLine(chord, transposeSemitones);
            sb.Append(shownChord).Append('\n').Append(lyric); // 코드 줄을 가사 줄 "위"에.
        }

        return sb.ToString();
    }

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
