using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Easislides.Wpf.Converters;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Converters;

public class ColorValueToBrushConverterTests
{
    [Fact]
    public void Convert_StringRgbHex_ReturnsOpaqueBrush()
    {
        var sut = new ColorValueToBrushConverter();

        var result = sut.Convert("#336699", typeof(Brush), null, CultureInfo.InvariantCulture);

        var brush = result.Should().BeOfType<SolidColorBrush>().Subject;
        brush.Color.Should().Be(Color.FromArgb(0xFF, 0x33, 0x66, 0x99));
    }

    [Fact]
    public void Convert_IntArgb_ReturnsMatchingBrush()
    {
        var sut = new ColorValueToBrushConverter();

        var result = sut.Convert(unchecked((int)0xFF66CCFF), typeof(Brush), null, CultureInfo.InvariantCulture);

        var brush = result.Should().BeOfType<SolidColorBrush>().Subject;
        brush.Color.Should().Be(Color.FromArgb(0xFF, 0x66, 0xCC, 0xFF));
    }

    [Fact]
    public void ConvertBack_ReturnsDoNothing()
    {
        var sut = new ColorValueToBrushConverter();

        var result = sut.ConvertBack(Brushes.White, typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().BeSameAs(System.Windows.Data.Binding.DoNothing);
    }
}
