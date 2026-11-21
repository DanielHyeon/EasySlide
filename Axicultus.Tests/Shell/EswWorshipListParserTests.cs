using System.Linq;
using System.IO;
using Axicultus.Module;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public class EswWorshipListParserTests
{
    // 레거시 .esw(Axicultus v3.2) XML 샘플 — 헤더(설정·메모) + 곡(D)·PPT(P)·성경(B) 항목.
    private const string SampleEsw = """
        <Axicultus>
          <ListItem>
            <ListHeader>
              <SystemID>SYS-1</SystemID>
              <FormatData>29=-1>47=60></FormatData>
              <Notes>주일 1부 메모</Notes>
            </ListHeader>
            <Item>
              <ItemID>D123</ItemID>
              <Title1>은혜로다</Title1>
              <Folder>찬양</Folder>
              <FormatData>29=-65536></FormatData>
            </Item>
            <Item>
              <ItemID>P설교.pptx</ItemID>
              <Title1>설교 슬라이드</Title1>
              <Folder></Folder>
              <FormatData></FormatData>
            </Item>
            <Item>
              <ItemID>B창1:1</ItemID>
              <Title1>창세기 1:1</Title1>
              <Folder>성경</Folder>
              <FormatData></FormatData>
            </Item>
          </ListItem>
        </Axicultus>
        """;

    [Fact]
    public void ReadsWinFormsNamedJsonWithTheSameItemsAndDefaultsAsXml()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "Sunday.json");
            ServiceListDocument.ReadXml(SampleEsw).Save(path);
            var oldItems = EswWorshipListParser.Parse(SampleEsw, out var oldFormat, out var oldNotes);
            var items = EswWorshipListParser.Parse(File.ReadAllText(path), out var format, out var notes);
            Assert.Equal(oldFormat, format);
            Assert.Equal(oldNotes, notes);
            Assert.Equal(oldItems.ToArray(), items.ToArray());
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void Parse_SampleEsw_ExtractsItemsInOrder_WithTypeCodeAndIdSplit()
    {
        var items = EswWorshipListParser.Parse(SampleEsw);

        items.Should().HaveCount(3, "헤더(FormatData/Notes)는 항목이 아니므로 제외, <Item> 3개만");

        items[0].TypeCode.Should().Be("D", "ItemID 첫 글자 = 종류 코드(곡)");
        items[0].Id.Should().Be("123", "나머지 = 식별자");
        items[0].Title.Should().Be("은혜로다");
        items[0].Folder.Should().Be("찬양");
        items[0].FormatData.Should().Be("29=-65536>", "항목 FormatData(헤더 FormatData 와 혼동 안 함)");

        items[1].TypeCode.Should().Be("P");
        items[1].Id.Should().Be("설교.pptx", "PPT 는 파일명이 식별자");
        items[1].Title.Should().Be("설교 슬라이드");

        items[2].TypeCode.Should().Be("B");
        items[2].Id.Should().Be("창1:1", "성경 참조가 식별자");
        items[2].Title.Should().Be("창세기 1:1");
    }

    [Fact]
    public void ParsesCompactXmlWithTypedReferencesAndPreservesUnknownCode()
    {
        const string xml = """<Axicultus><ListItem><ListHeader><Notes>예배</Notes></ListHeader><Item><ItemID>d42</ItemID></Item><Item><ItemID>P1</ItemID><Title1>E:\예배 자료\설교.pptx</Title1></Item><Item><ItemID>B창1:1</ItemID></Item><Item><ItemID>X17</ItemID></Item></ListItem></Axicultus>""";
        var items = EswWorshipListParser.Parse(xml, out _, out var notes);
        notes.Should().Be("예배");
        items[0].Reference.SongId.Should().Be(42);
        items[1].Reference.ContentPath.Should().Be(@"E:\예배 자료\설교.pptx");
        items[2].Reference.BibleReference.Should().Be("창1:1");
        items[3].Reference.Kind.Should().Be(Axicultus.Module.LegacyListItemKind.Unknown);
        items[3].Id.Should().Be("17");
        items[3].Reference.ContentPath.Should().BeNull();
        EswWorshipListParser.Parse(xml + "<").Should().BeEmpty();
    }

    [Fact]
    public void Parse_NullEmptyOrWhitespace_ReturnsEmpty()
    {
        EswWorshipListParser.Parse(null).Should().BeEmpty();
        EswWorshipListParser.Parse("").Should().BeEmpty();
        EswWorshipListParser.Parse("   ").Should().BeEmpty();
    }

    [Fact]
    public void Parse_MalformedXml_ReturnsEmpty_Gracefully()
    {
        // 손상된/비-XML 파일은 예외 대신 빈 목록(호출 측이 "읽을 수 없음" 안내).
        EswWorshipListParser.Parse("<Axicultus><Item><ItemID>D1</ItemID>").Should().BeEmpty("닫히지 않은 XML");
        EswWorshipListParser.Parse("이건 XML 이 아님").Should().BeEmpty();
    }

    [Fact]
    public void Parse_ItemWithEmptyItemId_IsSkipped()
    {
        const string xml = """
            <Axicultus><ListItem>
              <Item><ItemID></ItemID><Title1>빈 ID</Title1></Item>
              <Item><ItemID>D9</ItemID><Title1>정상</Title1></Item>
            </ListItem></Axicultus>
            """;

        var items = EswWorshipListParser.Parse(xml);

        items.Should().ContainSingle("ItemID 빈 항목은 건너뛴다(레거시와 동일)");
        items[0].Id.Should().Be("9");
    }

    [Fact]
    public void Parse_SingleCharItemId_HasEmptyId()
    {
        // ItemID 가 종류 코드 한 글자뿐이면 식별자는 빈 문자열(Substring 범위 오류 없이).
        const string xml = "<Axicultus><ListItem><Item><ItemID>D</ItemID><Title1>코드만</Title1></Item></ListItem></Axicultus>";

        var items = EswWorshipListParser.Parse(xml);

        items.Should().ContainSingle();
        items[0].TypeCode.Should().Be("D");
        items[0].Id.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MissingOptionalChildren_DefaultToEmptyStrings()
    {
        // Title1/Folder/FormatData 가 없어도 빈 문자열로 안전하게 채운다(부분 손상 내성).
        const string xml = "<Axicultus><ListItem><Item><ItemID>P단독</ItemID></Item></ListItem></Axicultus>";

        var item = EswWorshipListParser.Parse(xml).Single();

        item.TypeCode.Should().Be("P");
        item.Id.Should().Be("단독");
        item.Title.Should().BeEmpty();
        item.Folder.Should().BeEmpty();
        item.FormatData.Should().BeEmpty();
    }
}
