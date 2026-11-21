using System.Globalization;
using Axicultus.Wpf.Converters;
using Axicultus.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class SongListFormattingTests
{
    [Theory]
    [InlineData("은혜로다", 12, true, "12. 은혜로다")]    // 번호 표시 on + 번호 있음
    [InlineData("은혜로다", 12, false, "은혜로다")]        // off → 제목만
    [InlineData("은혜로다", 0, true, "은혜로다")]          // 번호 0(없음) → 제목만
    [InlineData("은혜로다", -1, true, "은혜로다")]         // 음수 번호 → 제목만
    [InlineData("", 5, true, "5. ")]                       // 빈 제목도 번호는 붙는다
    public void FormatRow_AppliesNumberingRules(string title, int number, bool useNumbering, string expected)
        => SongListFormatting.FormatRow(title, number, useNumbering).Should().Be(expected);

    [Theory]
    [InlineData("가나안의혼인잔치136(새)", false, "가나안의혼인잔치", "136(새)", false)]
    [InlineData("가나안의혼인잔치136(새)", true, "136. 가나안의혼인잔치136(새)", "찬송", false)]
    [InlineData("찬송 008(새) <#>", false, "찬송", "008(새)", true)]
    [InlineData("요한복음 3:16", false, "요한복음 3:16", "찬송", false)]
    public void FolderColumnsFollowFrmMainTitleAndNumberRules(string title, bool numbering, string left, string right, bool media)
    {
        var converter = new SongRowDisplayConverter();
        var row = converter.Convert([title, 136, numbering, "찬송"], typeof(FolderSongRow), "Columns", CultureInfo.InvariantCulture);
        row.Should().Be(new FolderSongRow(left, right, media));
    }

    [Fact]
    public void FormatRow_NullTitle_TreatedAsEmpty()
        => SongListFormatting.FormatRow(null, 3, useNumbering: false).Should().Be("");

    [Fact]
    public void Converter_FormatsFromValuesArray()
    {
        var sut = new SongRowDisplayConverter();

        var result = sut.Convert(["은혜로다", 7, true], typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().Be("7. 은혜로다");
    }

    [Fact]
    public void Converter_TooFewValues_ReturnsEmpty()
    {
        var sut = new SongRowDisplayConverter();

        sut.Convert(["title"], typeof(string), null, CultureInfo.InvariantCulture).Should().Be(string.Empty);
        sut.Convert([], typeof(string), null, CultureInfo.InvariantCulture).Should().Be(string.Empty);
    }

    [Fact]
    public void Converter_WrongTypes_DegradeSafely()
    {
        var sut = new SongRowDisplayConverter();

        // 번호가 int 가 아니면 0 취급(→ 제목만), useNumbering 이 bool 아니면 false.
        sut.Convert(["곡", "notint", "notbool"], typeof(string), null, CultureInfo.InvariantCulture)
            .Should().Be("곡");
    }
}
