using Easislides.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class BibleReferenceParserTests
{
    [Fact]
    public void TryParse_SingleVerse_ParsesAndIsSingle()
    {
        BibleReferenceParser.TryParse("창 1:1", out var reference).Should().BeTrue();

        reference.Should().Be(new BibleReference("창", 1, 1, 1, 1));
        reference.IsSingleVerse.Should().BeTrue();
    }

    [Fact]
    public void TryParse_SameChapterRange_ExpandsEndChapterToStart()
    {
        // "1:1-5"는 같은 장(1장) 안의 1~5절 — 끝 장이 생략되면 시작 장과 같다.
        BibleReferenceParser.TryParse("창 1:1-5", out var reference).Should().BeTrue();

        reference.Should().Be(new BibleReference("창", 1, 1, 1, 5));
        reference.IsSingleVerse.Should().BeFalse();
    }

    [Fact]
    public void TryParse_CrossChapterRange_KeepsBothChapters()
    {
        BibleReferenceParser.TryParse("창 1:1-2:3", out var reference).Should().BeTrue();

        reference.Should().Be(new BibleReference("창", 1, 1, 2, 3));
    }

    [Theory]
    [InlineData("John 3:16", "John", 3, 16, 3, 16)]
    [InlineData("1요한 4:7-8", "1요한", 4, 7, 4, 8)]   // 숫자로 시작하는 책 이름도 끝에서 꼬리를 잡아 안전.
    [InlineData("1 John 3:16", "1 John", 3, 16, 3, 16)] // 책 이름 안의 공백 보존.
    [InlineData("창 1.1", "창", 1, 1, 1, 1)]           // '.' 구분자 허용.
    [InlineData("  창  1 : 1 - 2 : 3  ", "창", 1, 1, 2, 3)] // 공백 관대.
    [InlineData("시 119:105", "시", 119, 105, 119, 105)]
    [InlineData("창1:1", "창", 1, 1, 1, 1)]            // 책과 장 사이 공백 없어도 됨.
    [InlineData("1 2:3", "1", 2, 3, 2, 3)]             // 책 토큰이 "1"(숫자) — ResolveBook 이 책 번호로 해석(의도된 동작).
    public void TryParse_VariousValidForms(string input, string book, int sc, int sv, int ec, int ev)
    {
        BibleReferenceParser.TryParse(input, out var reference).Should().BeTrue();

        reference.Should().Be(new BibleReference(book, sc, sv, ec, ev));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("창")]            // 장:절 없음.
    [InlineData("abc")]
    [InlineData("3:16")]          // 책 이름 없음 — 어느 책인지 알 수 없어 거부.
    [InlineData("창 0:1")]        // 장은 1 이상.
    [InlineData("창 1:0")]        // 절은 1 이상.
    [InlineData("창 2:5-1:1")]    // 끝(1:1)이 시작(2:5)보다 앞.
    [InlineData("창 1:5-1:2")]    // 같은 장에서 끝 절이 시작 절보다 앞.
    public void TryParse_InvalidForms_ReturnFalse(string input)
    {
        BibleReferenceParser.TryParse(input, out _).Should().BeFalse();
    }

    [Fact]
    public void ResolveBook_ExactNameIgnoringCase()
    {
        var books = new[] { new BibleBook(1, "Genesis"), new BibleBook(2, "Exodus") };

        BibleReferenceParser.ResolveBook(books, "genesis").Should().Be(new BibleBook(1, "Genesis"));
    }

    [Fact]
    public void ResolveBook_PrefixThenContains()
    {
        var books = new[] { new BibleBook(1, "Genesis"), new BibleBook(2, "Exodus") };

        BibleReferenceParser.ResolveBook(books, "Gen").Should().Be(new BibleBook(1, "Genesis"), "이름이 토큰으로 시작");
        BibleReferenceParser.ResolveBook(books, "xod").Should().Be(new BibleBook(2, "Exodus"), "포함 매칭");
    }

    [Fact]
    public void ResolveBook_NumericToken_MatchesByBookNumber()
    {
        var books = new[] { new BibleBook(1, "Genesis"), new BibleBook(2, "Exodus") };

        BibleReferenceParser.ResolveBook(books, "2").Should().Be(new BibleBook(2, "Exodus"));
    }

    [Fact]
    public void ResolveBook_IgnoresSpaces_MatchesSpacedName()
    {
        var books = new[] { new BibleBook(62, "1 John"), new BibleBook(1, "Genesis") };

        BibleReferenceParser.ResolveBook(books, "1John").Should().Be(new BibleBook(62, "1 John"), "공백 무시 비교");
    }

    [Fact]
    public void ResolveBook_NoMatch_ReturnsNull()
    {
        var books = new[] { new BibleBook(1, "Genesis") };

        BibleReferenceParser.ResolveBook(books, "Xyz").Should().BeNull();
    }
}
