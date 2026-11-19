using Easislides.Util;
using Xunit;

namespace Easislides.Tests;

public class SessionFormatHeaderTests
{
    [Fact]
    public void LegacyConversionPreservesMetadataDuplicatesAndValuesContainingEquals()
    {
        string title = "old", folder = "keep", rotate = "old";
        string header = "[\u0001=320*\u0002old=a=[b]*\u0002*=\u0002*n=one*n=제[목]*\u001f=]";
        Assert.Equal(320, DataUtil.Convertv32FormatString(ref header, '*', ref title, ref folder, ref rotate));
        Assert.Equal("11=a=b>11=>", header);
        Assert.Equal(("제목", "keep", ""), (title, folder, rotate));
    }
}
