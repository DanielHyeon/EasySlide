using System.Linq;
using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PraiseBookIndexServiceTests
{
    private static PraiseBookIndexEntry Song(string title, int number = 0) => new(title, number);

    [Fact]
    public void BuildIndex_GroupsHangulTitlesByChoseong()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("나무"), Song("가시"), Song("하늘") });

        groups.Select(g => g.Key).Should().Equal("ㄱ", "ㄴ", "ㅎ");
    }

    [Fact]
    public void BuildIndex_NormalizesDoubleConsonantToBase()
    {
        // 까치(ㄲ) 는 ㄱ 그룹으로 합쳐진다(가나다 색인 관례).
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("가위"), Song("까치") });

        groups.Should().ContainSingle();
        groups[0].Key.Should().Be("ㄱ");
        groups[0].Entries.Select(e => e.Title).Should().Equal("가위", "까치");
    }

    [Fact]
    public void BuildIndex_DigitTitles_GroupUnderHash()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("3절 찬양"), Song("1번 곡") });

        groups.Should().ContainSingle();
        groups[0].Key.Should().Be("#");
    }

    [Fact]
    public void BuildIndex_LatinTitles_GroupByUppercaseLetter()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("amazing grace"), Song("Awesome God"), Song("Blessed") });

        groups.Select(g => g.Key).Should().Equal("A", "B");
        groups[0].Entries.Should().HaveCount(2, "amazing/Awesome 모두 A");
    }

    [Fact]
    public void BuildIndex_OtherTitles_GroupUnderEtc()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("♪ 전주") });

        groups.Should().ContainSingle();
        groups[0].Key.Should().Be("기타");
    }

    [Fact]
    public void BuildIndex_OrdersGroups_HangulThenLatinThenDigitThenEtc()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[]
        {
            Song("♪ 기타곡"),
            Song("3번"),
            Song("Amazing"),
            Song("하늘"),
            Song("가나"),
        });

        groups.Select(g => g.Key).Should().Equal("ㄱ", "ㅎ", "A", "#", "기타");
    }

    [Fact]
    public void BuildIndex_SortsEntriesWithinGroupByTitle()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("강물"), Song("가시"), Song("거울") });

        // 유니코드(=가나다) 순: 가 < 강 < 거
        groups[0].Entries.Select(e => e.Title).Should().Equal("가시", "강물", "거울");
    }

    [Fact]
    public void BuildIndex_WordCountMode_GroupsAndOrdersByLegacyCjkWordCount()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(
            new[] { Song("가나다"), Song("Alpha"), Song("나"), Song("가나") },
            PraiseBookSortMode.WordCount);

        groups.Select(g => g.Key).Should().Equal("000", "001", "002", "003");
        groups.SelectMany(g => g.Entries).Select(e => e.Title)
            .Should().Equal("Alpha", "나", "가나", "가나다");
    }

    [Fact]
    public void BuildIndex_SkipsBlankTitles()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("가나"), Song("  "), Song("") });

        groups.Should().ContainSingle();
        groups[0].Entries.Should().ContainSingle();
    }

    [Fact]
    public void BuildIndex_PreservesSongNumber()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("가나", 305) });

        groups[0].Entries[0].Number.Should().Be(305);
    }

    [Fact]
    public void BuildIndex_BareCompatibilityJamo_GroupsUnderThatConsonant()
    {
        // 제목이 낱자 자음(ㄲ)으로 시작하면 음절 경로가 아닌 낱자 경로로 처리되며, 쌍자음은 ㄱ 으로 정규화된다.
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("ㄲ코드"), Song("ㄴ표기") });

        groups.Select(g => g.Key).Should().Equal("ㄱ", "ㄴ");
    }

    [Fact]
    public void BuildIndex_LeadingWhitespace_IgnoredForGrouping()
    {
        var sut = new PraiseBookIndexService();

        var groups = sut.BuildIndex(new[] { Song("   하늘") });

        groups[0].Key.Should().Be("ㅎ");
    }
}
