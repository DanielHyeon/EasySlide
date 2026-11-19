using System;
using Easislides.Util;
using Xunit;

namespace Easislides.Tests;

public class SessionFormatHeaderTests
{
    [Theory]
    [InlineData('>')]
    [InlineData('|')]
    public void LoadingReusesSlotsAndKeepsTheLastValueForKeysOneThrough254(char separator)
    {
        var slots = new string[Gf.HeaderDataMax + 1];
        Array.Fill(slots, "previous");
        const string header = "0=outside>001=old> +1 = 한글 = 값 >2=old>2>" +
                              "3=old>3=>4=x|5=y>255=outside>-1=outside>999999999999=outside>" +
                              "invalid>6=value>>7=first>7=last>254=final";
        var expected = new string[slots.Length];
        Array.Fill(expected, "");
        expected[1] = " 한글 = 값 ";
        expected[4] = "x|5=y";
        expected[6] = "value";
        expected[7] = "last";
        expected[254] = "final";
        expected[255] = "previous";
        Assert.Equal(1, Gf.LoadHeaderData(header, ref slots, separator));
        Assert.Equal(expected, slots);
    }

    [Theory]
    [InlineData("", -1)]
    [InlineData("[old format]", -1)]
    [InlineData(">", 1)]
    public void ReloadClearsPreviousSessionEvenWhenHeaderIsEmptyOrLegacy(string header, int result)
    {
        var slots = new string[Gf.HeaderDataMax];
        Gf.LoadHeaderData("1=320>254=last", ref slots, '>');
        Assert.Equal(result, Gf.LoadHeaderData(header, ref slots, '>'));
        Assert.All(slots, value => Assert.Equal("", value));
    }

    [Fact]
    public void LegacyConversionPreservesMetadataDuplicatesAndValuesContainingEquals()
    {
        string title = "old", folder = "keep", rotate = "old";
        string header = "[\u0001=320*\u0002old=a=[b]*\u0002*=\u0002*n=one*n=제[목]*\u001f=]";
        Assert.Equal(320, DataUtil.Convertv32FormatString(ref header, '*', ref title, ref folder, ref rotate));
        Assert.Equal("11=a=b>11=>", header);
        Assert.Equal(("제목", "keep", ""), (title, folder, rotate));
    }

    [Fact]
    public void Legacy32ImportReadsTheConvertedHeaderBeforeTheFirstSong()
    {
        const string file = "[\u0001=320|\u0002=old|\u0002=last|\u0003=kept=whole]\nfirst song";
        var slots = new string[Gf.HeaderDataMax];
        Assert.Equal(file.IndexOf(']'), Gf.Load32HeaderData("", file, ref slots, '|'));
        Assert.Equal("last", slots[11]);
        Assert.Equal("kept=whole", slots[12]);
    }
}
