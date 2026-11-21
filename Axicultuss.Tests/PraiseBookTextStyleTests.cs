using System.Drawing;
using System.Text;
using Xunit;

namespace Axicultus.Tests;

public class PraiseBookTextStyleTests
{
    [Fact]
    public void SessionStylesKeepReferenceVisibilityAndInheritTheCopyrightFont()
    {
        string[] header = new string[255];
        Gf.LoadHeaderData("101=8>102=99>111=7>112=18>113=-65536>131=0>132=40>136=1>", ref header, '>');
        var styles = new PraiseBookTextStyle[PraiseBookTextStyle.PartCount];
        PraiseBookTextStyle.ReadHeader(header, styles, Color.Black);

        Assert.Equal(@"\fs26\cf1\b0\i0\ul ", styles[0].ToRtf(0));
        Assert.Equal(@"\fs22\cf2\b0\i0\ulnone ", styles[1].ToRtf(1));
        Assert.Equal(Color.Black.ToArgb(), styles[1].Colour.ToArgb());
        Assert.True(styles[1].Visible);
        Assert.False(styles[6].Visible);
        Assert.True(styles[7].Visible);
        Assert.Equal(@"\fs36\cf7\b\i\ulnone ", styles[6].ToRtf(6));
        Assert.Equal(Color.Red.ToArgb(), styles[7].Colour.ToArgb());
        var saved = new StringBuilder();
        styles[6].AppendToHeader(saved, 6);
        styles[7].AppendToHeader(saved, 7);
        Assert.Equal("131=6>132=18>133=-65536>136=7>137=18>138=-65536>", saved.ToString());
    }
}
