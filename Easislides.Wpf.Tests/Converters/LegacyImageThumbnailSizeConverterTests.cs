using System.Globalization;
using Easislides.Wpf.Converters;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Converters;

public class LegacyImageThumbnailSizeConverterTests
{
    [Fact]
    public void Convert_Width_KeepsFrmMainThreeColumnImageDensityInLeftRail()
    {
        var sut = new LegacyImageThumbnailSizeConverter();

        var result = sut.Convert(190d, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should().Be(52d, "FrmMain Images shows three compact thumbnails in the narrow source rail");
    }

    [Fact]
    public void Convert_Height_UsesFourByThreeImageThumb()
    {
        var sut = new LegacyImageThumbnailSizeConverter();

        var result = sut.Convert(190d, typeof(double), "Height", CultureInfo.InvariantCulture);

        result.Should().Be(39d);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(120d)]
    public void Convert_SmallPanelWidth_ClampsToReadableMinimum(double panelWidth)
    {
        var sut = new LegacyImageThumbnailSizeConverter();

        var result = sut.Convert(panelWidth, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should().Be(46d);
    }
}
