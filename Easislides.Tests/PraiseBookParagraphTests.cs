using Xunit;

namespace Easislides.Tests;

public class PraiseBookParagraphTests
{
    [Fact]
    public void OneAndTwoColumnExportsKeepIndependentIndexTabStopsAndParagraphFormatting()
    {
        var singleColumn = new FrmGenerateDoc.ParagraphLayout(1);
        var twoColumns = new FrmGenerateDoc.ParagraphLayout(2);
        Assert.Equal(8900, singleColumn.TextWidth);
        Assert.Equal(4400, twoColumns.TextWidth);
        Assert.Equal(@"\pard\fi-400\li400\ri680\tqr\tx8900", singleColumn.GetIndent(3));
        Assert.Equal(@"\pard\fi-400\li400\ri680\tqr\tx4400", twoColumns.GetIndent(3));
        Assert.Equal(@"\pard\fi-400\li400", singleColumn.GetIndent(0));
        Assert.Equal(@"\pard\fi-900\li900\tx500", singleColumn.GetIndent(1));
        Assert.Equal(@"\pard\fi-1400\li1400\tx1000", singleColumn.GetIndent(2));
        Assert.Equal(singleColumn.GetIndent(1), twoColumns.GetIndent(4));
        Assert.Equal(@"\pard\fi-500\li500\tx500", twoColumns.GetIndent(5));
        Assert.Equal(@"\b0\i0\ulnone\par ", Gf.RTFNewLine);
    }
}
