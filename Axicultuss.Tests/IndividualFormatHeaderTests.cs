using System;
using System.Globalization;
using System.Linq;
using Axicultus.Module;
using Xunit;

namespace Axicultus.Tests;

[Collection("Image rendering globals")]
public class IndividualFormatHeaderTests
{
    [Fact]
    public void ApplyingDefaultsPreservesThePreviewedInfoScreenAndResetsTheOutputWordItem()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var list = new System.Windows.Forms.ListView();
            const string format = "21=2>29=-65281>";
            list.Items.Add(new System.Windows.Forms.ListViewItem(new[] { "Notice", "Inotice", format }));
            list.Items.Add(new System.Windows.Forms.ListViewItem(new[] { "Word", "Wdoc", format }));
            var info = new SongSettings { FolderNo = -1, Source = ItemSource.WorshipList, CurItemNo = 1, Type = "I", ItemID = "notice" };
            var word = new SongSettings { FolderNo = -1, Source = ItemSource.WorshipList, CurItemNo = 2, Type = "W", ItemID = "doc" };
            using var infoLines = info.LyricsAndNotationsList;
            using var wordLines = word.LyricsAndNotationsList;
            Gf.LoadIndividualFormatData(ref info, format);
            Gf.LoadIndividualFormatData(ref word, format);

            Assert.Equal((false, true), FrmMain.ApplyWorshipListDefaults(list.Items, info, word));
            Assert.Equal(format, info.Format.FormatString);
            Assert.Equal(format, list.Items[0].SubItems[2].Text);
            Assert.Equal("", word.Format.FormatString);
            Assert.Equal("", list.Items[1].SubItems[2].Text);
        });
    }

    [Fact]
    public void ApplyingFolderMarginsLeavesTheSessionTemplateMarginsIntact()
    {
        var session = Gf.Folders[0];
        var folder = Gf.Folders[1];
        try
        {
            Gf.Folders[0] = new() { LeftMargin = 1, RightMargin = 2, BottomMargin = 3 };
            Gf.Folders[1] = new() { LeftMargin = 4, RightMargin = 5, BottomMargin = 6 };
            var song = new SongSettings { FolderNo = 1 };
            using var lines = song.LyricsAndNotationsList;
            Gf.LoadIndividualFormatData(ref song, "");
            Assert.Equal((4, 5, 6), (song.Format.ShowLeftMargin, song.Format.ShowRightMargin, song.Format.ShowBottomMargin));
            Assert.Equal((1, 2, 3), (Gf.Folders[0].LeftMargin, Gf.Folders[0].RightMargin, Gf.Folders[0].BottomMargin));
        }
        finally
        {
            Gf.Folders[0] = session;
            Gf.Folders[1] = folder;
        }
    }

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
