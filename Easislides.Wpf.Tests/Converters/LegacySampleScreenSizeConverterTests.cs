using System.Globalization;
using Easislides.Wpf.Converters;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Converters;

public class LegacySampleScreenSizeConverterTests
{
    [Fact]
    public void Convert_Width_UsesFrmMainPanelWidthFormula()
    {
        var sut = new LegacySampleScreenSizeConverter();

        var result = sut.Convert(new object[] { 1000d, 800d }, typeof(double), "Width", CultureInfo.InvariantCulture);

        result.Should().Be(900d, "FrmMain ResizeSampleScreen uses panelWidth * 18 / 20 before height clamping");
    }

    [Fact]
    public void Convert_Height_UsesFrmMainFourByThreeFormula()
    {
        var sut = new LegacySampleScreenSizeConverter();

        var result = sut.Convert(new object[] { 1000d, 800d }, typeof(double), "Height", CultureInfo.InvariantCulture);

        result.Should().Be(675d, "FrmMain ResizeSampleScreen derives height from width * 3 / 4");
    }

    [Fact]
    public void Convert_WhenHeightIsTooSmall_ClampsHeightAndRecomputesWidth()
    {
        var sut = new LegacySampleScreenSizeConverter();

        var width = sut.Convert(new object[] { 1000d, 300d }, typeof(double), "Width", CultureInfo.InvariantCulture);
        var height = sut.Convert(new object[] { 1000d, 300d }, typeof(double), "Height", CultureInfo.InvariantCulture);

        width.Should().Be(380d, "FrmMain recomputes width as height * 4 / 3 after clamping");
        height.Should().Be(285d, "FrmMain leaves 15 px of bottom slack when clamping sample screens");
    }

    [Fact]
    public void Convert_WithMissingSize_ReturnsLegacyMinimum()
    {
        var sut = new LegacySampleScreenSizeConverter();

        var result = sut.Convert(new object[] { 0d, 0d }, typeof(double), "Height", CultureInfo.InvariantCulture);

        result.Should().Be(1d);
    }
}
