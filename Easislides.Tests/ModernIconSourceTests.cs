using System.Drawing;
using Easislides.Global;
using Xunit;

namespace Easislides.Tests;

public class ModernIconSourceTests
{
    [Fact]
    public void SingleAndMultiplePathBibleIconsRenderAllLayers()
    {
        using var outline = ModernIcons.Render("bible", ModernIconStyle.Regular, 16, Color.Black);
        using var filled = ModernIcons.Render("bible", ModernIconStyle.Filled, 16, Color.Black);
        Assert.NotNull(outline);
        Assert.NotNull(filled);
        Assert.True(outline.GetPixel(3, 8).A > 0);  // Cover
        Assert.True(outline.GetPixel(6, 11).A > 0); // Page edge
        Assert.True(outline.GetPixel(8, 8).A > 0);  // Cross
        Assert.Equal(0, outline.GetPixel(10, 8).A);
        Assert.True(filled.GetPixel(10, 8).A > 0);
        Assert.Equal(0, filled.GetPixel(8, 8).A);
    }
}
