using System;
using System.Collections.Generic;
using System.Text;

namespace Axicultus.Wpf.Shell;

/// <summary>
/// 이중 언어 절 페이지 — Region1(주 언어)·Region2(보조 언어) 한 쌍.
/// 가사에 <c>[region 2]</c> 마커가 없으면 Region2 는 빈 문자열(단일 영역).
/// </summary>
public sealed record LyricsRegionPage(string Region1, string Region2);

/// <summary>
/// 저장된 "원시 가사"(Axicultus 작성용 마크업 포함)를 송출 화면에 보일 "표시용 텍스트"로 바꾼다.
/// 회중 화면에는 작성용 마커가 보이면 안 되므로 다음을 제거/정리한다:
///   - 맨 앞 코드(노테이션) 블록 <c>[~ ... ]</c> (ImportExportService 가 저장 시 쓰는 형식과 동일)
///   - 절/페이지 마커만 있는 줄 (예: <c>[1]</c>, <c>[chorus]</c>) → 절 경계로 보고 빈 줄로 대체
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
        var normalizedLyrics = NormalizeText(rawLyrics);

        // 2) 맨 앞 코드(노테이션) 블록 [~...] 제거.
        var leading = normalizedLyrics.TrimStart('\n', ' ', '\t');
        if (leading.StartsWith("[~", StringComparison.Ordinal))
        {
            var end = leading.IndexOf(']');
            if (end > 1)
            {
                normalizedLyrics = leading[(end + 1)..];
            }
        }

        // 3) 줄 단위 정리: 마커만 있는 줄·빈 줄은 절 경계로 보고, 본문 절 사이에 빈 줄 하나만 남긴다.
        var output = new StringBuilder();
        var hasContent = false;   // 첫 본문 줄이 나오기 전의 경계는 모두 버린다
        var pendingBlank = false; // 절 경계(빈 줄)는 "다음 본문 줄" 앞에만 삽입
        foreach (var rawLine in normalizedLyrics.Split('\n'))
        {
            // 줄 안의 '»'(코드/노테이션 마커) 뒤는 회중 화면에 보이지 않게 잘라낸다 — 가사 본문만 남긴다.
            // (코드는 운영자/연주자용이며, 미리보기에선 흐리게 보여 주지만 출력에선 숨긴다. [~...] 블록 제거와 같은 원칙.)
            var line = StripInlineNotation(rawLine).TrimEnd();
            // This text is for reading (a printed song, not a slide): a heading counts with spaces around it too.
            if (IsHiddenMarker(line.TrimStart()) || string.IsNullOrWhiteSpace(line))
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
    {
        var screens = ReadVerseScreens(rawLyrics, sequence);
        var pages = new string[screens.Count];
        for (var i = 0; i < pages.Length; i++)
        {
            pages[i] = screens[i].Text;
        }

        return pages;
    }

    /// <summary>
    /// 가사 전체를 절 단위 페이지 리스트로 분할한다.
    /// 빈 문자열이거나 변환 결과가 없으면 빈 리스트를 반환한다.
    /// </summary>
    public static IReadOnlyList<string> ToVersePages(string? rawLyrics)
        => ToVersePages(rawLyrics, sequence: null);

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
        => GetRegionPages(rawLyrics, sequence: null);

    /// <summary>
    /// 가사에 region2 <b>본문(내용)</b>이 하나라도 있으면 true(= 이중 언어 곡). 마커 유무가 아니라 내용 유무로 판정 —
    /// <c>[region 2]</c> 마커만 있고 뒤에 글자가 없으면 false(단일 영역으로 안전 폴백).
    /// </summary>
    public static bool HasRegion2(string? rawLyrics)
    {
        foreach (var (_, _, pages) in ReadVerses(rawLyrics))
        {
            foreach (var page in pages)
            {
                if (page.Region2.Length > 0)
                {
                    return true;
                }
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
    {
        var screens = ReadScreens(rawLyrics, sequence);
        var pages = new LyricsRegionPage[screens.Count];
        for (var i = 0; i < pages.Length; i++)
        {
            pages[i] = screens[i].Page;
        }

        return pages;
    }

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
    /// 이중 언어 절 페이지의 라벨 목록 — <see cref="GetRegionPages(string?,string?)"/> 와 1:1. 절 점프에 쓴다.
    /// 제목(heading) 없이 시작한 절의 화면은 빈 문자열.
    /// </summary>
    public static IReadOnlyList<string> GetRegionSectionLabels(string? rawLyrics, string? sequence)
    {
        var screens = ReadScreens(rawLyrics, sequence);
        var labels = new string[screens.Count];
        for (var i = 0; i < labels.Length; i++)
        {
            labels[i] = screens[i].Label;
        }

        return labels;
    }

    /// <summary>
    /// 절 페이지마다의 라벨을 <see cref="ToVersePages(string?,string?)"/> 와 1:1 정렬되게 반환한다(절 라벨 직접 점프용).
    /// 라벨이 없는(빈 줄로만 구분된) 페이지는 빈 문자열. Sequence 적용 시 펼쳐진 순서대로 라벨이 반복된다.
    /// 운영자가 "후렴(C)으로", "3절로" 즉시 이동(레거시 FrmInfoScreen 절 버튼 1~9·c·b 대응)하는 데 쓰인다.
    /// </summary>
    public static IReadOnlyList<string> GetSectionLabels(string? rawLyrics, string? sequence)
    {
        var screens = ReadVerseScreens(rawLyrics, sequence);
        var labels = new string[screens.Count];
        for (var i = 0; i < labels.Length; i++)
        {
            labels[i] = screens[i].Label;
        }

        return labels;
    }

    /// <summary>
    /// The text with every line of nothing but spaces made empty, so that it ends a page. FrmMain does this to a
    /// text file whose paragraphs are kept whole (gfLyrics.FormatDisplayLyrics, FitTextFileParagraphs); in a song
    /// such a line is a line of the screen.
    /// </summary>
    internal static string? EmptyBlankLines(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var lines = NormalizeText(text).Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // The screens as single texts, for a caller that shows one region. A screen of two languages gives its
    // region 1 text and then its region 2 text, so that no line is lost.
    private static List<(string Label, string Text)> ReadVerseScreens(string? rawLyrics, string? sequence)
    {
        var screens = new List<(string Label, string Text)>();
        foreach (var (label, page) in ReadScreens(rawLyrics, sequence))
        {
            if (page.Region1.Length > 0)
            {
                screens.Add((label, page.Region1));
            }

            if (page.Region2.Length > 0)
            {
                screens.Add((label, page.Region2));
            }
        }

        return screens;
    }

    // The screens in show order. FrmMain keeps the verses of the stored sequence that the song has and, when none
    // is left, shows every verse in the order it first appears (gfConfig.ValidateSequence).
    private static List<(string Label, LyricsRegionPage Page)> ReadScreens(string? rawLyrics, string? sequence)
    {
        var screens = new List<(string Label, LyricsRegionPage Page)>();
        foreach (var (label, pages) in ReadShownVerses(rawLyrics, sequence))
        {
            foreach (var page in pages)
            {
                screens.Add((label, page));
            }
        }

        return screens;
    }

    // The verses in show order, each with its screens (a verse always has at least one). With keepSpaceRows a
    // line of spaces stays a line of one space, also first or last on its screen: FrmMain counts it as a row when
    // it cuts a screen and when it deals out region 2 (gfLyrics.GetOneScreen, BuildSlidesReg2).
    internal static List<(string Label, List<LyricsRegionPage> Pages)> ReadShownVerses(string? rawLyrics, string? sequence, bool keepSpaceRows = false)
    {
        var verses = ReadVerses(rawLyrics, keepSpaceRows);
        var shown = new List<(string Label, List<LyricsRegionPage> Pages)>();
        if (!string.IsNullOrWhiteSpace(sequence))
        {
            foreach (var token in sequence.Split([',', ' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var (key, label, pages) in verses)
                {
                    if (MatchesSection(key, token))
                    {
                        shown.Add((label, pages));
                        break;
                    }
                }
            }
        }

        if (shown.Count == 0)
        {
            foreach (var (_, label, pages) in verses)
            {
                shown.Add((label, pages));
            }
        }

        return shown;
    }

    private const string Region2Line = "[region 2]";

    // The verses of a song with their screens, as FrmMain builds them (gfLyrics.ExtractLyricRegions,
    // FormatDisplayLyrics and BuildSlides), in the order the verses first appear:
    //  - a heading is a whole line, exactly as written; text before the first heading is verse 1, and every later
    //    block under the same heading adds its lines to that verse;
    //  - [region 2] puts the lines that follow into region 2 until the next heading; in the form that keeps all of
    //    region 2 after a single marker (gfLyrics.IsNewR2Format) it does so to the end, each line under its verse;
    //  - an empty line is a screen break; a line of spaces is a line of the screen;
    //  - screen n of region 2 is shown with screen n of region 1, and the screens region 2 has left over follow
    //    the last one alone (gfLyrics.BuildSlidesReg2).
    // Key is what a sequence names the verse by; Label is empty for a verse 1 that has no heading.
    private static List<(string Key, string Label, List<LyricsRegionPage> Pages)> ReadVerses(string? rawLyrics, bool keepSpaceRows = false)
    {
        var verses = new List<(string Key, string Label, List<LyricsRegionPage> Pages)>();
        var lines = ReadLines(rawLyrics);
        var markers = 0;
        var hasHeading = false;
        foreach (var line in lines)
        {
            hasHeading |= SectionLabel(line) is not null;
            markers += IsRegionMarker(line, 2) ? 1 : 0;
        }

        if (!hasHeading && markers > 1)
        {
            var pages = new List<LyricsRegionPage>();
            AddPassagePages(lines, pages);
            if (pages.Count > 0)
            {
                verses.Add(("1", string.Empty, pages));
            }

            return verses;
        }

        var separateRegions = IsSeparateRegionFormat(string.Join('\n', lines));
        var rows = new List<(string Verse, int Region, string Text)>();
        var order = new List<string>();
        var headed = new HashSet<string>(StringComparer.Ordinal);
        var verse = "1";
        var region = 1;
        foreach (var line in lines)
        {
            if (IsRegionMarker(line, 2))
            {
                region = 2;
            }
            else if (IsRegionMarker(line, 1))
            {
                region = 1; // Not a FrmMain marker: text written for this program returns to region 1 with it.
            }
            else if (SectionLabel(line) is { } heading)
            {
                verse = heading;
                headed.Add(heading);
                region = separateRegions ? region : 1;
            }
            else
            {
                if (!order.Contains(verse))
                {
                    order.Add(verse);
                }

                rows.Add((verse, region, line));
            }
        }

        // FrmMain lists the lines by verse code, region 1 before region 2, then drops the empty lines at the end,
        // every second of two empty lines in a row and an empty first line (gfLyrics.FormatDisplayLyrics).
        var sorted = new List<string>(order);
        sorted.Sort((a, b) => VerseCode(a).CompareTo(VerseCode(b)));
        var list = new List<(string Verse, int Region, string Text)>(rows.Count);
        foreach (var key in sorted)
        {
            for (var part = 1; part <= 2; part++)
            {
                foreach (var row in rows)
                {
                    if (row.Verse == key && row.Region == part)
                    {
                        list.Add(row);
                    }
                }
            }
        }

        while (list.Count > 0 && list[^1].Text.Length == 0)
        {
            list.RemoveAt(list.Count - 1);
        }

        for (var i = list.Count - 1; i >= 1; i--)
        {
            if (list[i].Text.Length == 0 && list[i - 1].Text.Length == 0)
            {
                list.RemoveAt(i);
            }
        }

        if (list.Count > 0 && list[0].Text.Length == 0)
        {
            list.RemoveAt(0);
        }

        foreach (var key in order)
        {
            int start1 = -1, end1 = -1, start2 = -1, end2 = -1;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Verse != key)
                {
                    continue;
                }

                if (list[i].Region == 1)
                {
                    // Region 1 starts at its first line of text: an empty line before it is no screen. Counted as
                    // one, it kept the screens region 2 has left over from being shown (fixed in gfLyrics.BuildSlides).
                    start1 = start1 < 0 && list[i].Text.Length > 0 ? i : start1;
                    end1 = i;
                }
                else
                {
                    start2 = start2 < 0 ? i : start2;
                    end2 = i;
                }
            }

            // Each slide: first and last line of region 1, first and last line of region 2 (gfLyrics.BuildSlides).
            var slides = new List<int[]>();
            var count = CountScreens(list, start1, end1);
            if (count > 0)
            {
                for (var screen = 1; screen <= count; screen++)
                {
                    var start = FindScreenStart(list, start1, end1, screen);
                    if (start < 0)
                    {
                        continue;
                    }

                    slides.Add([start, FindScreenEnd(list, start, end1), -1, -1]);
                    if (start2 >= 0)
                    {
                        AddSecondRegion(list, start2, end2, slides, screen, screen == count);
                    }
                }
            }
            else
            {
                count = CountScreens(list, start2, end2);
                for (var screen = 1; screen <= count; screen++)
                {
                    var start = FindScreenStart(list, start2, end2, screen);
                    if (start >= 0)
                    {
                        slides.Add([-1, -1, start, FindScreenEnd(list, start, end2)]);
                    }
                }
            }

            var pages = new List<LyricsRegionPage>(slides.Count);
            foreach (var slide in slides)
            {
                // A screen of nothing but spaces is not shown.
                var page = new LyricsRegionPage(JoinRows(list, slide[0], slide[1], keepSpaceRows), JoinRows(list, slide[2], slide[3], keepSpaceRows));
                if (!string.IsNullOrWhiteSpace(page.Region1) || !string.IsNullOrWhiteSpace(page.Region2))
                {
                    pages.Add(page);
                }
            }

            if (pages.Count > 0)
            {
                verses.Add((key, headed.Contains(key) ? key : string.Empty, pages));
            }
        }

        return verses;
    }

    // The lines of the lyrics as the display reads them (Axicultus.Core.LyricLines; the caption library reads songs the same way).
    internal static string[] ReadLines(string? rawLyrics) => Axicultus.Core.LyricLines.ReadLines(rawLyrics);

    // gfLyrics.IsNewR2Format: the text has two parts around a single [region 2] and the second starts with a heading.
    private static bool IsSeparateRegionFormat(string lyrics)
    {
        var sections = lyrics.Split(Region2Line, StringSplitOptions.RemoveEmptyEntries);
        if (sections.Length != 2)
        {
            return false;
        }

        var second = sections[1][0] == '\n' ? sections[1][1..] : sections[1];
        var end = second.Length > 0 && second[0] == '[' ? second.IndexOfAny(['[', ']', '\n'], 1) : -1;
        return end > 0 && second[end] == ']' && SectionLabel(second[..(end + 1)]) is not null;
    }

    // The order FrmMain keeps the verses in (gfConfig.InitializeVerseSymbols): the chorus, verses 1 to 99, then
    // bridge, ending, chorus 2, bridge 2, prechorus and prechorus 2.
    private static int VerseCode(string key) => key switch
    {
        "chorus" => 0,
        "bridge" => 100,
        "ending" => 101,
        "chorus 2" => 102,
        "bridge 2" => 103,
        "prechorus" => 111,
        "prechorus 2" => 112,
        _ => int.Parse(key, System.Globalization.CultureInfo.InvariantCulture),
    };

    // gfLyrics.GetVerseScreenCount: every empty line and the last line end a screen.
    private static int CountScreens(List<(string Verse, int Region, string Text)> list, int start, int end)
    {
        if (start > end || start < 0 || end < 0)
        {
            return -1;
        }

        var count = 0;
        for (var i = start; i <= end; i++)
        {
            if (list[i].Text.Length == 0 || i == end)
            {
                count++;
            }
        }

        return count;
    }

    // gfLyrics.GetVerseScreenLoc: the first line of the given screen, or -1 when the lines hold no such screen.
    private static int FindScreenStart(List<(string Verse, int Region, string Text)> list, int start, int end, int screen)
    {
        if (start > end)
        {
            return -1;
        }

        if (start + 1 <= end && list[start].Text.Length == 0)
        {
            start++;
        }

        var current = 1;
        for (var i = start; i <= end; i++)
        {
            if (list[i].Text.Length == 0)
            {
                current++;
            }

            if (screen == current)
            {
                return list[i].Text.Length > 0 ? i : i < end ? i + 1 : -1;
            }
        }

        return -1;
    }

    // gfLyrics.GetVerseScreenEndLoc: the last line of the screen that starts at the given line.
    private static int FindScreenEnd(List<(string Verse, int Region, string Text)> list, int start, int end)
    {
        if (start > end)
        {
            return -1;
        }

        if (start == end)
        {
            return end;
        }

        if (list[start].Text.Length == 0)
        {
            start++;
        }

        for (var i = start; i < end; i++)
        {
            if (list[i].Text.Length == 0)
            {
                return i - 1;
            }
        }

        return end;
    }

    // gfLyrics.BuildSlidesReg2: the region 2 screen of the same number goes on the slide just added; after the last
    // region 1 screen of the verse the region 2 screens that are left become slides of their own.
    private static void AddSecondRegion(
        List<(string Verse, int Region, string Text)> list, int first, int last, List<int[]> slides, int screen, bool lastScreen)
    {
        var start = FindScreenStart(list, first, last, screen);
        if (start < 0)
        {
            return;
        }

        var end = FindScreenEnd(list, start, last);
        slides[^1][2] = start;
        slides[^1][3] = end;
        if (last <= end || !lastScreen)
        {
            return;
        }

        var rest = end + 1;
        var count = CountScreens(list, rest, last);
        for (var extra = 1; extra <= count; extra++)
        {
            var at = FindScreenStart(list, rest, last, extra);
            if (at >= 0)
            {
                slides.Add([-1, -1, at, FindScreenEnd(list, at, last)]);
            }
        }
    }

    private static string JoinRows(List<(string Verse, int Region, string Text)> list, int start, int end, bool keepSpaceRows)
    {
        if (start < 0 || end < start)
        {
            return string.Empty;
        }

        var text = new StringBuilder();
        for (var i = start; i <= end; i++)
        {
            var line = list[i].Text.TrimEnd();
            text.Append(keepSpaceRows && line.Length == 0 && list[i].Text.Length > 0 ? " " : line).Append('\n');
        }

        return text.ToString().Trim('\n');
    }

    // A two-language Bible passage (BiblePassageComposer): lyrics without a heading that carry a [region 2] for
    // every verse. Each blank line ends the page and the next page starts in region 1 again. FrmMain has no such
    // text; read its way, everything after the first marker would be region 2.
    private static void AddPassagePages(string[] lines, List<LyricsRegionPage> pages)
    {
        StringBuilder[] regions = [new(), new()];
        var region = 0;

        void Flush()
        {
            if (regions[0].Length > 0 || regions[1].Length > 0)
            {
                pages.Add(new LyricsRegionPage(regions[0].ToString(), regions[1].ToString()));
            }

            regions[0].Clear();
            regions[1].Clear();
            region = 0;
        }

        foreach (var line in lines)
        {
            if (IsRegionMarker(line, 2))
            {
                region = 1;
            }
            else if (IsRegionMarker(line, 1))
            {
                region = 0;
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                Flush();
            }
            else
            {
                var target = regions[region];
                if (target.Length > 0)
                {
                    target.Append('\n');
                }

                target.Append(line.TrimEnd());
            }
        }

        Flush();
    }

    // A [region N] line. Case and the space inside may differ (text written for this program); a space around
    // the brackets makes it a line of the verse, as it is for FrmMain.
    private static bool IsRegionMarker(string line, int regionNumber)
        => line.Length > 2 && line[0] == '[' && line[^1] == ']'
            && line[1..^1].Replace(" ", "", StringComparison.Ordinal).Equals($"region{regionNumber}", StringComparison.OrdinalIgnoreCase);

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

        var normalizedLyrics = NormalizeText(rawLyrics);

        var firstSeen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = new List<string>();
        foreach (var line in normalizedLyrics.Split('\n'))
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

    // 원시 가사 정규화 — 줄바꿈(\r\n, \r → \n)과 코드 마커를 단일 형태로 모은다.
    // 일부 데이터는 인코딩 비대칭으로 '»'(U+00BB)가 "Â»"(U+00C2 U+00BB)로 저장될 수 있어 단일 '»'로 정규화한다.
    // 모든 가사 진입점(표시 텍스트·절 페이지·중복 검사·미리보기 줄 분리)이 동일 규칙을 쓰도록 한 곳에 모았다.
    internal static string NormalizeText(string raw) => Axicultus.Core.LyricLines.NormalizeText(raw);

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

        var normalizedLyrics = NormalizeText(rawLyrics); // 줄바꿈·'Â»' 통일 후 줄 단위로 처리.
        var sb = new StringBuilder(normalizedLyrics.Length + 16);
        var lines = normalizedLyrics.Split('\n');
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
            if (lyric.Length == 0 || chord.Length == 0 || IsHiddenMarker(lyric))
            {
                sb.Append(lyric);
                continue;
            }

            // 라이브 조옮김이 걸려 있으면 코드만 ±반음 이동(가사·저장 데이터는 불변). 0 이면 원본 코드 그대로.
            var shownChord = chord[0] == PlacedChordMark ? PlaceStoredChords(chord[1..], lyric.Length, transposeSemitones)
                : transposeSemitones == 0 ? chord : ChordTransposer.TransposeLine(chord, transposeSemitones);
            if (shownChord.Length > 0)
            {
                sb.Append(shownChord).Append('\n'); // 코드 줄을 가사 줄 "위"에.
            }

            sb.Append(lyric);
        }

        return sb.ToString();
    }

    // Starts a chord line written from stored chords and, inside it, stands once before a chord for every
    // character the chord before it pushed it to the right. U+2060 has no width: LyricWordWrapping already puts
    // it into every displayed Hangul line, so a view that shows the line as plain text shows only the chords.
    internal const char PlacedChordMark = '\u2060';

    /// <summary>
    /// The lyrics with the chords the database keeps apart from them (MSC, "(line;chord;position;...)" records)
    /// written on their lines as "lyric»" + mark + "chord;position;...". From there they travel with the line
    /// through verse splitting, the sequence, region pairing and pagination like chords typed after '»', and
    /// <see cref="ExpandNotations"/> turns them into a chord line. A record names its line by the index among
    /// all lines of the stored lyrics (headings, [region 2] and blank lines count), and the records are read in
    /// their stored order, one at most per line (gfLyrics.ExtractLyricRegions). A heading or an empty line shows
    /// no chords there; a line that also has typed chords shows the stored ones.
    /// </summary>
    public static string? WithStoredChords(string? lyrics, string? notations)
    {
        if (string.IsNullOrEmpty(lyrics) || string.IsNullOrEmpty(notations))
        {
            return lyrics;
        }

        var lines = lyrics.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var remaining = notations;
        var fields = string.Empty;
        var recordLine = -1;
        for (var index = 0; index < lines.Length; index++)
        {
            if (recordLine < index && remaining.Length > 0
                && !Library.SongEditorChords.TryReadRecord(ref remaining, out recordLine, out fields))
            {
                remaining = string.Empty;
            }

            var line = StripInlineNotation(lines[index].Replace("Â»", "»", StringComparison.Ordinal)).TrimEnd();
            if (recordLine == index && fields.Length > 0 && line.Length > 0 && !IsHiddenMarker(line))
            {
                lines[index] = string.Concat(line, "»", PlacedChordMark.ToString(), fields);
            }
        }

        return string.Join('\n', lines);
    }

    // "chord;position;..." as a chord line: every chord starts in the column of the lyric character it stands
    // over. A chord that would run into the one before it goes one space after it instead, with a mark for
    // every column it gave way, so that ReadPlacedChords still tells its position.
    private static string PlaceStoredChords(string fields, int lyricLength, int transposeSemitones)
    {
        var line = new StringBuilder().Append(PlacedChordMark);
        var column = 0;
        var parts = fields.Split(';');
        for (var i = 0; i + 1 < parts.Length; i += 2)
        {
            // gfDisplay.DisplayTextByWidthOneLine skips a chord without a position. It also leaves out a chord
            // placed after the end of the line; here that chord is shown at the end, as the song editor shows it.
            if (parts[i].Length == 0 || parts[i] == "-1" || !int.TryParse(parts[i + 1], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var position) || position < 0)
            {
                continue;
            }

            position = Math.Min(position, lyricLength);
            if (column > 0 && column >= position)
            {
                line.Append(' ');
                column++;
            }

            line.Append(' ', Math.Max(0, position - column));
            column = Math.Max(column, position);
            line.Append(PlacedChordMark, column - position);
            var chord = transposeSemitones == 0 ? parts[i] : ChordTransposer.Transpose(parts[i], transposeSemitones);
            line.Append(chord);
            column += chord.Length;
        }

        return column == 0 ? string.Empty : line.ToString();
    }

    /// <summary>
    /// The chords of a chord line written from stored chords, each with the index of the lyric character it
    /// stands over; null for any other line (a lyric line, or chords typed after '»', which have no position).
    /// </summary>
    internal static List<(string Chord, int Position)>? ReadPlacedChords(string line)
    {
        if (line.Length == 0 || line[0] != PlacedChordMark)
        {
            return null;
        }

        var chords = new List<(string Chord, int Position)>();
        var column = 0;
        for (var i = 1; i < line.Length;)
        {
            if (line[i] == ' ')
            {
                column++;
                i++;
                continue;
            }

            var pushed = 0;
            for (; i < line.Length && line[i] == PlacedChordMark; i++)
            {
                pushed++;
            }

            var start = i;
            for (; i < line.Length && line[i] != ' ' && line[i] != PlacedChordMark; i++)
            {
            }

            if (i > start)
            {
                chords.Add((line[start..i], column - pushed));
                column += i - start;
            }
        }

        return chords;
    }

    /// <summary>
    /// 절 라벨이 "후렴(Chorus)"을 가리키는지 — "강조를 후렴만"(ChorusOnly) 효과 판정에 쓴다.
    /// 공백·대소문자 무시. 인식: C·Chorus·Refrain·후렴(흔하고 모호하지 않은 후렴 표기만).
    /// 한 글자 "렴" 같은 모호한 약어는 절 번호·다른 라벨과 충돌할 수 있어 의도적으로 제외. 그 외(1·2·Bridge 등)는 false.
    /// </summary>
    public static bool IsChorusLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var t = label.Trim();
        return t.Equals("C", StringComparison.OrdinalIgnoreCase)
            || t.Equals("Chorus", StringComparison.OrdinalIgnoreCase)
            || t.Equals("Refrain", StringComparison.OrdinalIgnoreCase)
            || t.Equals("후렴", StringComparison.Ordinal); // not UI text
    }

    // A sequence token names a verse by its heading text or by the FrmMain sequence symbol (c, b, e, p, t, w, q).
    internal static bool MatchesSection(string label, string token) =>
        string.Equals(label, token, StringComparison.OrdinalIgnoreCase) ||
        (token.ToLowerInvariant(), label.ToLowerInvariant()) is
            ("c", "chorus") or ("b", "bridge") or ("e", "ending") or ("p", "prechorus") or
            ("t", "chorus 2") or ("w", "bridge 2") or ("q", "prechorus 2");

    // Only the headings FrmMain knows start a verse (gfConfig.InitializeVerseHeadings: [1]..[99] and seven names,
    // compared case-sensitively). Any other bracket-only line ([Chorus], [x2]) is lyric text, as it is there,
    // and so is a heading with a space before or after it (gfLyrics.ExtractLyricRegions looks the whole line up).
    private static string? SectionLabel(string line)
    {
        if (line.Length < 3 || line[0] != '[' || line[^1] != ']')
        {
            return null;
        }

        var inner = line[1..^1];
        return inner is "chorus" or "chorus 2" or "bridge" or "bridge 2" or "prechorus" or "prechorus 2" or "ending"
            || (inner[0] != '0' && int.TryParse(inner, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var verse) && verse <= 99)
            ? inner
            : null;
    }

    // A bracket-only line the audience never sees: a verse heading, a region marker or a [~chords] block.
    private static bool IsHiddenMarker(string line)
        => SectionLabel(line) is not null || IsRegionMarker(line, 1) || IsRegionMarker(line, 2)
            || (IsMarkerOnlyLine(line) && line.TrimStart().StartsWith("[~", StringComparison.Ordinal));

    // 코드 마커(»)를 쓰지 않는 "순수 본문"(성경 구절 등)을 절 분할에 안전하게 통과시키기 위한 사적 영역(PUA) 보호 문자.
    // 일부 번역의 인용부호 »…« 가 StripInlineNotation 에 코드 마커로 오인돼 잘리지 않도록, 분할 동안만 이 문자로
    // 치환했다가(Guard) 결과 본문에서 되돌린다(Unguard). 사용자 텍스트엔 절대 나타나지 않는 U+E000 을 쓴다.
    private const char LiteralNotationGuard = '\uE000';

    /// <summary>코드 마커 규약을 안 쓰는 본문에서 '»'를 사적 영역 문자로 임시 치환한다(절 분할 전 보호).</summary>
    public static string GuardLiteralNotation(string? text)
        => string.IsNullOrEmpty(text) ? string.Empty : text.Replace('»', LiteralNotationGuard);

    /// <summary>Guard 로 치환했던 사적 영역 문자를 다시 '»'로 되돌린다(분할 후 본문 복원).</summary>
    public static string UnguardLiteralNotation(string? text)
        => string.IsNullOrEmpty(text) ? string.Empty : text.Replace(LiteralNotationGuard, '»');

    // A page of SongPageLayout.Paginate as it is shown. The page is display text already (a line such as "[2] "
    // that FrmMain shows must stay); with the chords off only what follows a chord marker is dropped.
    internal static string FormatSongSlide(string? page, bool showNotations, int transpose)
    {
        if (showNotations || string.IsNullOrEmpty(page))
        {
            return ExpandNotations(page, showNotations, transpose);
        }

        var lines = NormalizeText(page).Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = StripInlineNotation(lines[i]).TrimEnd();
        }

        return string.Join('\n', lines).Trim('\n');
    }

    // 줄에서 '»'(코드/노테이션 마커) 이후를 잘라 본문만 남긴다. 마커가 없으면 줄 그대로.
    // ('»' 정규화는 호출 전에 끝나 있다고 가정 — ToDisplayText 가 "Â»"→"»"로 모은 뒤 호출.)
    internal static string StripInlineNotation(string line) => Axicultus.Core.LyricLines.StripInlineNotation(line);

    // 줄 전체가 하나의 대괄호 토큰인지( 예: [1] [Chorus] [~G D] ) — 작성용 마커로 간주.
    private static bool IsMarkerOnlyLine(string line) => Axicultus.Core.LyricLines.IsMarkerOnlyLine(line);
}
