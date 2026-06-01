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
}
