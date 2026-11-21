using System.Drawing;
using Axicultus.Global;
using Xunit;

namespace Axicultus.Tests;

public class ModernIconSourceTests
{
    [Theory]
    [InlineData("우리교회말씀", 0, 53)]
    [InlineData("★★★사도신경★★★", 6, 53)]
    [InlineData("주 기 도 문", 7, 54)]
    [InlineData("■봉헌찬양", 6, 55)]
    [InlineData("광고", 7, 56)]
    [InlineData("사도신경", 2, 2)]
    [InlineData("주기도문송", 0, 0)]
    public void WorshipRowDrawsTheServiceIconWithoutChangingItsSourceIndex(string title, int sourceIndex, int displayIndex)
    {
        var row = new System.Windows.Forms.ListViewItem(title, sourceIndex);
        Assert.Equal(displayIndex, FrmMain.ModernWorshipRowPictureIndex(row));
        Assert.Equal(sourceIndex, row.ImageIndex);
    }

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
