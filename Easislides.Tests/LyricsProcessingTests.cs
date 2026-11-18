using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;
using Xunit;

namespace Easislides.Tests;

public class LyricsProcessingTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    public void RepeatedVersesKeepTheirSlideNumbersAndInclusiveLineRanges(bool split, bool secondRegionOnly, bool fitWholeVerse)
    {
        OnSta(() =>
        {
            var song = new SongSettings
            {
                SongSequence = "\u0001\u0001",
                SplitScreens = split,
                Type = fitWholeVerse ? "D" : "B",
                RotateStyle = fitWholeVerse ? 2 : 1
            };
            using var lines = song.LyricsAndNotationsList;
            using var font = new Font("Arial", 12);
            foreach (string line in new[] { "가", "나", "다", "one", "two", "three" })
                lines.Items.Add(new ListViewItem(["", "", line, "", ""]));

            song.VersePresent[1] = song.VersePresent[150] = true;
            song.VerseLineLoc[1, 1] = secondRegionOnly ? -1 : 0;
            song.VerseLineLoc[1, 2] = secondRegionOnly ? -1 : 2;
            song.VerseLineLoc[1, 3] = 3;
            song.VerseLineLoc[1, 4] = 5;
            for (int region = 0; region < 2; region++)
                song.Lyrics[region] = new SongLyrics { Font = font, FS_Font = font, FS_Width = 960, FS_Height = 1, FS_Height_R2Bound = 1 };

            Gf.BuildSlides(song, lines, ref song.SongSequence, ref song.TotalSlides, ref song.SongVerses,
                ref song.ChorusSlides, ref song.Slide, InShowNotations: 1);

            int pagesPerVerse = split && !fitWholeVerse ? 3 : 1;
            Assert.Equal(pagesPerVerse * 2, song.TotalSlides);
            Assert.Equal(1, song.SongVerses[1]);
            Assert.Equal(secondRegionOnly ? 2 : 1, song.Slide[0, 3]);
            for (int slide = 1; slide <= song.TotalSlides; slide++)
            {
                int page = (slide - 1) % pagesPerVerse;
                int end = pagesPerVerse == 1 ? 2 : page;
                Assert.Equal(page == 0 ? 1 : -1, song.Slide[slide, 0]);
                Assert.Equal(secondRegionOnly ? -1 : page, song.Slide[slide, 1]);
                Assert.Equal(secondRegionOnly ? -1 : end, song.Slide[slide, 2]);
                Assert.Equal(3 + page, song.Slide[slide, 3]);
                Assert.Equal(3 + end, song.Slide[slide, 4]);
            }
        });
    }

    [Theory]
    [InlineData("", 0, 0, "old")]
    [InlineData("가나다라", 1000, 1, "1>12>4>")]
    [InlineData("가나다라", 0, 5, "5>12>0>1>1>1>1>")]
    [InlineData("a bc", 0, 4, "4>12>1>1>1>1>")]
    public void WrappingPreservesStoredBreakOffsets(string text, int width, int expectedLines, string expectedBreaks)
    {
        OnSta(() =>
        {
            using var list = new ListView();
            var lines = list;
            var item = list.Items.Add(new ListViewItem(["", "", text, "", "old"]));
            using var image = new Bitmap(1, 1);
            using var graphics = Graphics.FromImage(image);
            using var font = new Font("Arial", 12);
            Assert.Equal(expectedLines, Gf.GetLinesRequiredAndAddBreakPlusFont(ref lines, font, graphics, 0, width));
            Assert.Equal(expectedBreaks, item.SubItems[4].Text);
        });
    }

    [Fact]
    public void WrapSpaceLogRetainsOnlySpacesInTheConfiguredPrefix()
    {
        OnSta(() =>
        {
            string line = " a  bcde f";
            Assert.Equal("0;2;3;", Gf.ActionWordWrapSpacesAtStart(ref line));
            Assert.Equal("_a__bcde f", line);
        });
    }

    [Fact]
    public void DocumentEncodersKeepLegacyCodeUnitBoundariesAndControlCharacters()
    {
        const string ascii = "A\r\n\t\\<>&\u007f";
        const string text = ascii + "\u0080ÿĀ한😀";
        Assert.Equal(ascii + "\u0080ÿ" + @"\u00256?\u54620?\u55357?\u56832?", DataUtil.UnicodeToAscii_RTF(text));
        Assert.Equal(ascii + "\u0080ÿ&#00256;&#54620;&#55357;&#56832;", DataUtil.UnicodeToAscii_HTML(text));
        Assert.Equal(ascii + "80ff100d55cd83dde00", DataUtil.HexString(text));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("a\rb\r\n\n다\n", 3)]
    public void CountsOnlyLineFeeds(string value, int expected) => Assert.Equal(expected, DataUtil.CountLf(value));

    private static void OnSta(Action check)
    {
        Exception failure = null;
        var thread = new Thread(() => failure = Record.Exception(check)) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)));
        if (failure != null)
            ExceptionDispatchInfo.Throw(failure);
    }
}
