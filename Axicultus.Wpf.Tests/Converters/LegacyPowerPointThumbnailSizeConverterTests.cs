using System.Globalization;
using Axicultus.Wpf.Converters;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Converters;

public class LegacyPowerPointThumbnailSizeConverterTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(240, 236)]
    [InlineData(327, 323)]
    [InlineData(328, 160)]
    [InlineData(491, 241)]
    [InlineData(492, 160)]
    [InlineData(935, 307)]
    public void ConsoleCardsFitOneToThreeColumnsWithinTheViewport(double viewport, double expected)
    {
        var converter = new LegacyPowerPointThumbnailSizeConverter();
        converter.Convert(viewport, typeof(double), "Console", CultureInfo.InvariantCulture).Should().Be(expected);
    }

    [Fact]
    public void Convert_Width_UsesFrmMainThreeColumnFormula()
    {
        var sut = new LegacyPowerPointThumbnailSizeConverter();

        var result = sut.Convert(935d, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should().Be(300d, "FrmMain uses (panelWidth - 35) / 3 for PowerPoint thumbnail canvas width");
    }

    [Fact]
    public void Convert_Height_UsesFrmMainFourByThreeThumbnailHeight()
    {
        var sut = new LegacyPowerPointThumbnailSizeConverter();

        var result = sut.Convert(935d, typeof(double), "Height", CultureInfo.InvariantCulture);

        result.Should().Be(225d, "FrmMain uses thumbnail width * 3 / 4 for PowerPoint thumbnail canvas height");
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(100d)]
    public void Convert_SmallPanelWidth_ClampsToUsableMinimum(double panelWidth)
    {
        var sut = new LegacyPowerPointThumbnailSizeConverter();

        var result = sut.Convert(panelWidth, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should().Be(80d);
    }
}
