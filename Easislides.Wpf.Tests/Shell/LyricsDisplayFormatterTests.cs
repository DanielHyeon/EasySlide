using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class LyricsDisplayFormatterTests
{
    [Fact]
    public void ToDisplayText_StripsVerseMarkersAndSeparatesVerses()
    {
        // [1] [2] 같은 절/페이지 마커는 회중 화면에 보이면 안 되고, 절 사이는 빈 줄 하나로 구분.
        var raw = "[1]\n첫째 줄\n둘째 줄\n[2]\n셋째 줄";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("첫째 줄\n둘째 줄\n\n셋째 줄");
        display.Should().NotContain("[1]").And.NotContain("[2]");
    }

    [Fact]
    public void ToDisplayText_StripsLeadingNotationBlock()
    {
        // 맨 앞 코드(노테이션) 블록 [~...] 은 제거된다.
        var raw = "[~G D Em C]\n[1]\nAmazing grace";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("Amazing grace");
        display.Should().NotContain("[~").And.NotContain("G D Em C");
    }

    [Fact]
    public void ToDisplayText_NormalizesCrlfAndCollapsesBlankRuns()
    {
        // \r\n 정규화 + 연속 빈 줄은 하나로 축약, 앞뒤 경계는 제거.
        var raw = "\r\n[1]\r\n첫째\r\n\r\n\r\n둘째\r\n";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("첫째\n\n둘째");
    }

    [Fact]
    public void ToDisplayText_KeepsLyricLineThatMerelyContainsBrackets()
    {
        // 줄 전체가 마커가 아닌( 뒤에 본문이 붙은 ) 경우는 그대로 둔다(과도한 제거 방지).
        var raw = "[1] 후렴 시작\n본문";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("[1] 후렴 시작\n본문");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \n  ")]
    public void ToDisplayText_EmptyOrWhitespace_ReturnsEmpty(string? raw)
    {
        LyricsDisplayFormatter.ToDisplayText(raw).Should().BeEmpty();
    }

    [Fact]
    public void ToDisplayText_StripsInlineChordNotationAfterMarker()
    {
        // '»'(U+00BB)는 코드/노테이션 구분 마커 — 회중 화면엔 가사만 보이고 '»' 뒤 코드는 빠진다([~...] 제거와 동일 원칙).
        var raw = "Amazing grace » G  C\nHow sweet » D7";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("Amazing grace\nHow sweet");
        display.Should().NotContain("»").And.NotContain("G  C").And.NotContain("D7");
    }

    [Fact]
    public void ToDisplayText_ChordOnlyLine_IsDroppedAsBoundary()
    {
        // '»'로 시작하는(코드 전용) 줄은 본문이 없어 절 경계로 처리(회중 화면에 표시 안 됨).
        var raw = "» G  C  D\n주 은혜 놀라워";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("주 은혜 놀라워");
    }

    [Fact]
    public void ToDisplayText_ChordOnlyLineBetweenLyrics_SplitsIntoTwoVerses()
    {
        // 절 중간의 코드 전용 줄('»'로 시작)은 빈 줄→절 경계로 작동한다(현 정책 고정 — 레거시 writer 는 보통 줄 끝에 ' »'를 붙임).
        var raw = "첫째\n» G  C\n둘째";

        LyricsDisplayFormatter.ToDisplayText(raw).Should().Be("첫째\n\n둘째");
    }

    [Fact]
    public void ToDisplayText_NormalizesMojibakeChordMarker()
    {
        // 인코딩 비대칭으로 '»'(U+00BB)가 "Â»"(U+00C2 U+00BB)로 저장된 데이터도 동일하게 코드 마커로 처리.
        var raw = "은혜로다 Â» G C";

        var display = LyricsDisplayFormatter.ToDisplayText(raw);

        display.Should().Be("은혜로다");
        display.Should().NotContain("Â").And.NotContain("»");
    }

    // ─── Sequence(절 순서) 모델 ──────────────────────────────────────────────

    [Fact]
    public void ToVersePages_WithSequence_DefinesSectionsOnce_RepeatsByOrder()
    {
        // 절을 1회 정의([1],[C],[2])하고 시퀀스(1 C 2 C)로 반복 → 후렴(C)이 두 번 나온다(레거시 절 순서 모델).
        var lyrics = "[1]\nVerse one\n[C]\nChorus line\n[2]\nVerse two";

        var pages = LyricsDisplayFormatter.ToVersePages(lyrics, "1 C 2 C");

        pages.Should().Equal("Verse one", "Chorus line", "Verse two", "Chorus line");
    }

    [Fact]
    public void ToVersePages_SequenceTokens_CommaOrSpace_CaseInsensitive()
    {
        var lyrics = "[1]\nVerse\n[C]\nChorus";

        LyricsDisplayFormatter.ToVersePages(lyrics, "1, c").Should().Equal("Verse", "Chorus");
    }

    [Fact]
    public void ToVersePages_EmptySequence_FallsBackToLinear()
    {
        var lyrics = "[1]\nVerse one\n[C]\nChorus line";

        LyricsDisplayFormatter.ToVersePages(lyrics, "")
            .Should().Equal(LyricsDisplayFormatter.ToVersePages(lyrics));
    }

    [Fact]
    public void ToVersePages_SequenceWithNoMatchingLabels_FallsBackToLinear()
    {
        // 토큰이 어떤 절 라벨과도 안 맞으면(예: 레거시 char-인코딩 Sequence) 선형으로 안전 폴백 — 오작동 방지.
        var lyrics = "[1]\nVerse one\n[2]\nVerse two";

        LyricsDisplayFormatter.ToVersePages(lyrics, "X Y Z")
            .Should().Equal(LyricsDisplayFormatter.ToVersePages(lyrics));
    }

    [Fact]
    public void GetVersePage_WithSequence_ReturnsExpandedPageAtIndex()
    {
        var lyrics = "[1]\nVerse one\n[C]\nChorus line\n[2]\nVerse two";

        // 시퀀스 "1 C 2 C" 의 3번 인덱스(0-based)는 두 번째 후렴.
        LyricsDisplayFormatter.GetVersePage(lyrics, 3, "1 C 2 C").Should().Be("Chorus line");
        // 범위 밖은 클램프.
        LyricsDisplayFormatter.GetVersePage(lyrics, 99, "1 C 2 C").Should().Be("Chorus line");
    }

    // ─── 중복 절 라벨 검증(Sequence 모델: 절은 1회 정의) ──────────────────────

    [Fact]
    public void FindDuplicateSectionLabels_DetectsRepeatedLabels_CaseInsensitive()
    {
        // 절은 한 번만 정의해야 한다(반복은 Sequence 로). 같은 라벨이 두 번 정의되면 둘째가 무시되는 오류.
        var lyrics = "[1]\nVerse one\n[C]\nChorus\n[c]\nDup chorus\n[2]\nVerse two";

        LyricsDisplayFormatter.FindDuplicateSectionLabels(lyrics).Should().Equal("C");
    }

    [Fact]
    public void FindDuplicateSectionLabels_NoDuplicates_ReturnsEmpty()
    {
        var lyrics = "[1]\nVerse one\n[C]\nChorus\n[2]\nVerse two";

        LyricsDisplayFormatter.FindDuplicateSectionLabels(lyrics).Should().BeEmpty();
    }

    [Fact]
    public void FindDuplicateSectionLabels_ThreeOrMoreDefinitions_ReportedOnce()
    {
        // 같은 라벨이 3번 이상 정의돼도 중복은 한 번만 보고(경고 중복 방지).
        var lyrics = "[C]\nA\n[C]\nB\n[C]\nD";

        LyricsDisplayFormatter.FindDuplicateSectionLabels(lyrics).Should().Equal("C");
    }

    [Fact]
    public void FindDuplicateSectionLabels_IgnoresNotationBlocks()
    {
        // [~..] 노테이션 블록은 절 라벨이 아니므로 중복 판정에서 제외.
        var lyrics = "[~G C]\n[1]\nVerse\n[~D]\n[1]\nDup";

        LyricsDisplayFormatter.FindDuplicateSectionLabels(lyrics).Should().Equal("1");
    }

    [Fact]
    public void FindDuplicateSectionLabels_NormalizesCrlf()
    {
        // CRLF 로 저장된 가사도 \n 과 동일하게 라벨 경계를 인식해 중복을 잡는다(정규화 일관성).
        var lyrics = "[1]\r\nVerse one\r\n[1]\r\nDup";

        LyricsDisplayFormatter.FindDuplicateSectionLabels(lyrics).Should().Equal("1");
    }

    [Fact]
    public void ToVersePages_WithSequence_NormalizesCrlfAndMojibake()
    {
        // 시퀀스 확장 경로(ParseLabeledSections)도 CRLF·"Â»" 모지바케를 동일하게 정규화한다.
        var lyrics = "[1]\r\nVerse one Â» G\r\n[C]\r\nChorus";

        var pages = LyricsDisplayFormatter.ToVersePages(lyrics, "1 C");

        // '»' 뒤 코드는 송출용으로 잘리고, CRLF 는 \n 으로 정규화돼 절 경계가 정확히 잡힌다.
        pages.Should().Equal("Verse one", "Chorus");
    }

    // ─── ToVersePages ────────────────────────────────────────────────────────

    [Fact]
    public void ToVersePages_SingleVerse_ReturnsSinglePage()
    {
        // 절 구분자(\n\n)가 없으면 전체가 하나의 페이지.
        var raw = "Amazing grace";

        var pages = LyricsDisplayFormatter.ToVersePages(raw);

        pages.Should().ContainSingle().Which.Should().Be("Amazing grace");
    }

    [Fact]
    public void ToVersePages_MultipleVerses_ReturnsOnePagePerVerse()
    {
        // 마커([ ]) 로 구분된 두 절 → 두 페이지.
        var raw = "[1]\n첫째 줄\n둘째 줄\n[2]\n셋째 줄";

        var pages = LyricsDisplayFormatter.ToVersePages(raw);

        pages.Should().HaveCount(2);
        pages[0].Should().Be("첫째 줄\n둘째 줄");
        pages[1].Should().Be("셋째 줄");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \n  ")]
    public void ToVersePages_EmptyOrWhitespace_ReturnsEmpty(string? raw)
    {
        LyricsDisplayFormatter.ToVersePages(raw).Should().BeEmpty();
    }

    [Fact]
    public void ToVersePages_ThreeVerses_ReturnsThreePages()
    {
        var raw = "[1]\n1절\n[2]\n2절\n[3]\n3절";

        var pages = LyricsDisplayFormatter.ToVersePages(raw);

        pages.Should().HaveCount(3);
        pages[0].Should().Be("1절");
        pages[1].Should().Be("2절");
        pages[2].Should().Be("3절");
    }

    // ─── GetVersePage ────────────────────────────────────────────────────────

    [Fact]
    public void GetVersePage_ValidIndex_ReturnsCorrectPage()
    {
        var raw = "[1]\n첫째\n[2]\n둘째\n[3]\n셋째";

        LyricsDisplayFormatter.GetVersePage(raw, 0).Should().Be("첫째");
        LyricsDisplayFormatter.GetVersePage(raw, 1).Should().Be("둘째");
        LyricsDisplayFormatter.GetVersePage(raw, 2).Should().Be("셋째");
    }

    [Fact]
    public void GetVersePage_NegativeIndex_ClampsToFirst()
    {
        var raw = "[1]\n첫째\n[2]\n둘째";

        LyricsDisplayFormatter.GetVersePage(raw, -1).Should().Be("첫째");
    }

    [Fact]
    public void GetVersePage_IndexAboveCount_ClampsToLast()
    {
        var raw = "[1]\n첫째\n[2]\n둘째";

        LyricsDisplayFormatter.GetVersePage(raw, 99).Should().Be("둘째");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetVersePage_EmptyLyrics_ReturnsEmpty(string? raw)
    {
        LyricsDisplayFormatter.GetVersePage(raw, 0).Should().BeEmpty();
    }
}
