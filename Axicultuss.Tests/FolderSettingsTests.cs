using Axicultus.Global;
using Xunit;

namespace Axicultus.Tests;

public class FolderSettingsTests
{
    [Fact]
    public void EditingACopyKeepsTheOriginalRegionsHeadingsAndAlternateStyles()
    {
        var original = new FolderSettings { LeftMargin = 3 };
        original.FontName[1] = "Second font";
        original.LyricsHeading[3] = "Ending";
        original.FontBold[3] = 1;
        original.HeadingFontUnderline[1] = 1;
        original.LineSpacing[1] = 1.5;

        var edited = original.Clone();
        Assert.Equal("Ending", edited.LyricsHeading[3]);
        Assert.Equal("Second font", edited.FontName[1]);
        Assert.Equal(1, edited.FontBold[3]);
        Assert.Equal(1, edited.HeadingFontUnderline[1]);
        Assert.Equal(1.5, edited.LineSpacing[1]);
        edited.LyricsHeading[3] = "Outro";
        edited.FontName[1] = "New font";
        edited.FontBold[3] = 0;
        edited.HeadingFontUnderline[1] = 0;
        edited.LineSpacing[1] = 2;
        edited.LeftMargin = 7;

        Assert.Null(edited.FontName[0]);
        Assert.Equal("Ending", original.LyricsHeading[3]);
        Assert.Equal("Second font", original.FontName[1]);
        Assert.Equal(1, original.FontBold[3]);
        Assert.Equal(1, original.HeadingFontUnderline[1]);
        Assert.Equal(1.5, original.LineSpacing[1]);
        Assert.Equal(3, original.LeftMargin);
    }
}
