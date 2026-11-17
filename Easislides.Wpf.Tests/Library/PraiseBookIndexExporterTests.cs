using System.Collections.Generic;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PraiseBookIndexExporterTests
{
    private static PraiseBookIndexGroup Group(string key, params PraiseBookIndexEntry[] entries)
        => new(key, entries);

    [Fact]
    public void BuildHtml_IncludesTitleGroupsAndEntries()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new[]
        {
            Group("ㄱ", new PraiseBookIndexEntry("가나", 12)),
            Group("ㅎ", new PraiseBookIndexEntry("하늘", 0)),
        };

        var html = sut.BuildHtml("주일 찬양집", groups);

        html.Should().Contain("<title>주일 찬양집</title>");
        html.Should().Contain("<h1>주일 찬양집</h1>");
        html.Should().Contain("<h2>ㄱ</h2>");
        html.Should().Contain("가나");
        html.Should().Contain("12");
        html.Should().Contain("<h2>ㅎ</h2>");
        html.Should().Contain("하늘");
    }

    [Fact]
    public void BuildHtml_EscapesHtmlSpecialCharactersInTitles()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new[] { Group("A", new PraiseBookIndexEntry("Rock & <Roll>", 1)) };

        var html = sut.BuildHtml("Q&A", groups);

        html.Should().Contain("Rock &amp; &lt;Roll&gt;");
        html.Should().NotContain("<Roll>");
        html.Should().Contain("Q&amp;A");
    }

    [Fact]
    public void BuildHtml_ZeroNumber_RendersBlankNumberCell()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new[] { Group("ㄱ", new PraiseBookIndexEntry("가나", 0)) };

        var html = sut.BuildHtml("색인", groups);

        // 번호 0 은 빈 칸으로(노이즈 방지) — "0" 이 텍스트로 나오지 않는다(마크업 형식에 둔감한 의도 검증).
        html.Should().Contain("가나");
        html.Should().NotContain(">0<");
    }

    [Fact]
    public void BuildHtml_BlankTitle_UsesDefault()
    {
        var sut = new PraiseBookIndexExporter();

        var html = sut.BuildHtml("   ", new List<PraiseBookIndexGroup>());

        html.Should().Contain("<title>찬양집 색인</title>");
    }

    // ── 증분158: RTF 내보내기(레거시 "Listing of Selected Folder" 의 RTF 출력 대응) ──

    [Fact]
    public void BuildRtf_StartsWithRtfHeader_AndIncludesTitleGroupAndEntry()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new[] { Group("A", new PraiseBookIndexEntry("Rock", 12)) };

        var rtf = sut.BuildRtf("Index", groups);

        rtf.Should().StartWith(@"{\rtf1");          // RTF 문서 머리.
        rtf.Should().EndWith("}");                  // 균형 잡힌 그룹 닫기.
        rtf.Should().Contain("Index");              // 제목.
        rtf.Should().Contain("Rock");               // 곡 제목(ASCII).
        rtf.Should().Contain(@"\tab 12");           // 번호는 탭 뒤에.
        rtf.Should().Contain(@"\par");              // 문단 구분.
    }

    [Fact]
    public void BuildRtf_EscapesRtfSpecialCharacters()
    {
        var sut = new PraiseBookIndexExporter();
        // RTF 에서 \ { } 는 제어문자라 반드시 이스케이프돼야 문서가 안 깨진다.
        var groups = new[] { Group("A", new PraiseBookIndexEntry(@"a\b{c}", 1)) };

        var rtf = sut.BuildRtf("T", groups);

        rtf.Should().Contain(@"a\\b\{c\}");
    }

    [Fact]
    public void BuildRtf_EscapesNonAsciiTitlesAsUnicodeEscapes()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new[] { Group("ㄱ", new PraiseBookIndexEntry("가", 1)) };

        var rtf = sut.BuildRtf("색인", groups);

        // 한글은 \uN?(부호 있는 16비트) 로 — '가'=U+AC00 → (short)44032 = -21504.
        rtf.Should().Contain(@"\u-21504?");
        rtf.Should().NotContain("가"); // 원시 한글 글자는 본문에 그대로 남지 않는다.
    }

    [Fact]
    public void BuildRtf_ZeroNumber_OmitsNumberAfterTab()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new[] { Group("A", new PraiseBookIndexEntry("Song", 0)) };

        var rtf = sut.BuildRtf("T", groups);

        rtf.Should().Contain("Song");
        rtf.Should().Contain(@"\tab \par"); // 번호 0 은 탭만 두고 빈 칸(노이즈 방지).
        rtf.Should().NotContain(@"\tab 0"); // "0" 이 번호로 찍히지 않는다.
    }

    [Fact]
    public void BuildRtf_NegativeNumber_OmitsNumber_LikeZero()
    {
        // 번호는 int 라 음수가 들어올 수 있다. "Number > 0" 가드는 0뿐 아니라 음수도 빈 칸으로 처리해야 한다
        // (누군가 >= 0 이나 != 0 으로 바꾸면 음수 -5 가 번호로 찍히는 회귀가 생기므로 그 의도를 고정).
        var sut = new PraiseBookIndexExporter();
        var groups = new[] { Group("A", new PraiseBookIndexEntry("Song", -5)) };

        var rtf = sut.BuildRtf("T", groups);

        rtf.Should().Contain(@"\tab \par");
        rtf.Should().NotContain("-5"); // 음수 번호는 찍지 않는다.
    }

    [Fact]
    public void BuildRtf_BlankTitle_FallsBackToDefault()
    {
        var sut = new PraiseBookIndexExporter();
        var groups = new List<PraiseBookIndexGroup>();

        // 빈 제목은 기본 제목("찬양집 색인")과 동일한 출력이어야 한다(이스케이프 방식에 둔감하게 동등성으로 검증).
        sut.BuildRtf("   ", groups).Should().Be(sut.BuildRtf("찬양집 색인", groups));
    }
}
