using System;
using System.Globalization;
using System.Linq;
using Axicultus.Module;
using Xunit;

namespace Axicultus.Tests;

public class IndividualFormatHeaderTests
{
    [Theory]
    [InlineData("ko-KR")]
    [InlineData("en-US")]
    [InlineData("th-TH")]
    public void LoadingSuccessiveHeadersMatchesSingleKeyLookupForEverySlot(string culture)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var song = new SongSettings { FolderNo = -1 };
        using var lines = song.LyricsAndNotationsList;
        var slots = song.Format.HeaderData;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            string dense = string.Concat(Enumerable.Range(0, slots.Length).Reverse().Select(key => $"{key}={key}>"));
            foreach (string header in new[]
            {
                dense,
                "21=2>1=first>1=second>0=zero>254=last>255=outside>43= 한글🎵 = x >61=>61=late>51=unfinished",
                ">>>oops>12>001=bad>+2=bad> 3=bad>4 =bad>x5=bad>212=6=embedded>73=first>73=unfinished",
                "1\u00ad=soft>1=later>2\0=null>1\u00ad2=joined>3=\u0301mark>3=next>\u00ad4=leading>５=width>6＝wide>153=\u034f\u0301x>",
                "1\n=1=2>1=second>2-3=punctuation>12 =space>9= \u0301accent>",
                "",
                ">"
            })
            {
                var expected = Enumerable.Range(0, slots.Length)
                    .Select(key => Gf.ExtractHeaderInfo(header, key, '>')).ToArray();
                Gf.LoadIndividualFormatData(ref song, header);
                Assert.Same(slots, song.Format.HeaderData);
                Assert.Equal(expected, slots);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void FirstTerminatedValueDrivesTheSongFormatIncludingAnEmptyFirstValue()
    {
        var song = new SongSettings { FolderNo = -1 };
        using var lines = song.LyricsAndNotationsList;
        const string header = "21=2>43= 맑은 고딕 >43=Arial>51=>51=late>61=images/a=b.png>71=4>254=incomplete";
        Gf.LoadIndividualFormatData(ref song, header);
        Assert.Equal(2, song.Format.ShowSongHeadings);
        Assert.Equal(" 맑은 고딕 ", song.Format.ShowFontName[0]);
        Assert.Equal("", song.Format.MediaLocation);
        Assert.Equal("images/a=b.png", song.Format.BackgroundPicture);
        Assert.Equal(4, song.Format.TransposeOffset);
        Assert.Equal("", song.Format.HeaderData[254]);
        Assert.False(song.UseDefaultFormat);

        Gf.LoadIndividualFormatData(ref song, "");
        Assert.True(song.UseDefaultFormat);
        Assert.All(song.Format.HeaderData, value => Assert.Equal("", value));
    }
}
