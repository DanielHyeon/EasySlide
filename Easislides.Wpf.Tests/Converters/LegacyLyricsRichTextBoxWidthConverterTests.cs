using System.Globalization;
using Easislides.Wpf.Converters;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Converters;

public class LegacyLyricsRichTextBoxWidthConverterTests
{
    [Fact]
    public void Convert_SubtractsScrollbarGutterFromPanelWidth()
    {
        var sut = new LegacyLyricsRichTextBoxWidthConverter();

        var result = sut.Convert(500d, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should().Be(482d, "FrmMain lyrics RichTextBoxes dock to the panel width, with only the scrollbar gutter reserved in WPF");
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(10d)]
    public void Convert_SmallWidthsClampToUsableMinimum(double width)
    {
        var sut = new LegacyLyricsRichTextBoxWidthConverter();

        var result = sut.Convert(width, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should().Be(1d);
    }
}
