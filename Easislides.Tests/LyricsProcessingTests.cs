using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;
using Xunit;

namespace Easislides.Tests;

public class LyricsProcessingTests
{
    private static void SetEditorFontsAndHeadings(Form editor, Font lyric, Font chord)
    {
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        var type = editor.GetType();
        type.GetField("MainFont", fields).SetValue(editor, lyric);
        type.GetField("NotationFont", fields).SetValue(editor, chord);
        Gf.InitializeVerseHeadings();
        type.GetField("sArray", fields).SetValue(editor, Gf.SymbolsString.Split(','));
    }

    [Theory]
    [InlineData(typeof(FrmEditItem), "tbLyrics1")]
    [InlineData(typeof(FrmInfoScreen), "tbLyrics2")]
    public void EditorTextChangedStylesMultilineInsertionAtTheCaret(Type type, string field)
    {
        OnSta(() =>
        {
            using var form = (Form)Activator.CreateInstance(type);
            using var lyric = new Font("Arial", 12);
            using var chord = new Font("Consolas", 10);
            SetEditorFontsAndHeadings(form, lyric, chord);
            var editor = (RichTextBox)type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
            string region = Gf.VerseSymbol[150];
            _ = editor.Handle;
            editor.Text = "위\n" + region + "\n끝";
            // Setting a native character format itself raises TextChanged. Seed the
            // untouched row before exercising the real TextChanged handler for the paste.
            var ignoreChange = type.GetField("IgnoreChange", BindingFlags.Instance | BindingFlags.NonPublic);
            ignoreChange.SetValue(form, true);
            editor.Select(2, region.Length);
            editor.SelectionColor = Color.Magenta;
            Assert.Equal(Color.Magenta.ToArgb(), editor.SelectionColor.ToArgb());
            ignoreChange.SetValue(form, false);
            editor.Select(2, 0);
            const string insert = "C(add9)    G/B »\nverse 가사 한 줄\n";
            editor.SelectedText = insert;
            Assert.Equal("위\n" + insert + region + "\n끝", editor.Text);
            Assert.Equal((2 + insert.Length, 0), (editor.SelectionStart, editor.SelectionLength));
            editor.Select(2, 1);
            Assert.Equal(chord.Name, editor.SelectionFont.Name);
            Assert.Equal(Gf.NotationColour.ToArgb(), editor.SelectionColor.ToArgb());
            editor.Select(editor.Text.IndexOf("verse", StringComparison.Ordinal), 1);
            Assert.Equal(lyric.Name, editor.SelectionFont.Name);
            editor.Select(2 + insert.Length, 1);
            Assert.Equal(Color.Magenta.ToArgb(), editor.SelectionColor.ToArgb());
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PartialEditorFormattingPreservesSelectionAndSurroundingRows(bool hasSelection)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var control = new CountingRichTextBox();
            RichTextBox editor = control;
            using var lyricFont = new Font("Arial", 12);
            using var chordFont = new Font("Consolas", 10);
            string region = Gf.VerseSymbol[150];
            string verse = Gf.VerseSymbol[1];
            _ = editor.Handle;
            editor.Text = $"위\n{verse}\nC  » »\n{region}\n가사\n{region}\n끝";
            editor.SelectAll();
            editor.SelectionColor = Color.Magenta;
            string text = editor.Text;
            int first = text.IndexOf(verse, StringComparison.Ordinal);
            int last = text.IndexOf("가사", StringComparison.Ordinal);
            editor.Select(last + 1, 0);
            if (hasSelection)
                editor.Select(first, 2);
            var selection = (editor.SelectionStart, editor.SelectionLength);
            control.RefreshCount = 0;
            string previousText = $"위\n기존 가사\n{region}\n끝";
            Gf.FormatEditorLyrics(editor, ref previousText, [verse, region], lyricFont, chordFont);
            Assert.Equal(text, previousText);
            Assert.Equal(selection, (editor.SelectionStart, editor.SelectionLength));
            Assert.Equal(1, control.RefreshCount);
            Assert.Equal(text, editor.Text);
            int ColorAt(int index)
            {
                editor.Select(index, 1);
                return editor.SelectionColor.ToArgb();
            }
            Assert.Equal(Color.Magenta.ToArgb(), ColorAt(0));
            Assert.Equal(Gf.SelectedTextColour.ToArgb(), ColorAt(first));
            Assert.Equal(Gf.NotationColour.ToArgb(), ColorAt(text.IndexOf('C')));
            Assert.Equal(chordFont.Name, editor.SelectionFont.Name);
            Assert.Equal(Gf.SelectedTextColour.ToArgb(), ColorAt(text.IndexOf(region, StringComparison.Ordinal)));
            Assert.Equal(Color.FromArgb(255, Gf.NormalTextColour).ToArgb(), ColorAt(last));
            Assert.Equal(Color.Magenta.ToArgb(), ColorAt(text.LastIndexOf(region, StringComparison.Ordinal)));
        });
    }

    private sealed class CountingRichTextBox : RichTextBox
    {
        internal int RefreshCount;
        public override void Refresh()
        {
            RefreshCount++;
            base.Refresh();
        }
    }

    [Theory]
    [InlineData("[region 2]\n끝", "[region 2]\n[region 2]\n끝", 11, 0, 11)]
    [InlineData("위\n끝", "위\nG »\n끝", 6, 2, 6)]
    [InlineData("위\nG »\n끝", "위\n끝", 2, -1, 2)]
    public void EditingWholeRowsLeavesTheNextRowStyleIntact(string previous, string current, int caret,
        int styled, int untouched)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var editor = new RichTextBox();
            using var font = new Font("Arial", 12);
            _ = editor.Handle;
            editor.Text = current;
            editor.SelectAll();
            editor.SelectionColor = Color.Magenta;
            editor.Select(caret, 0);
            Gf.FormatEditorLyrics(editor, ref previous, [Gf.VerseSymbol[150]], font, font);
            Assert.Equal(caret, editor.SelectionStart);
            if (styled >= 0)
            {
                editor.Select(styled, 1);
                Color color = current[styled] == '[' ? Gf.SelectedTextColour : Gf.NotationColour;
                Assert.Equal(color.ToArgb(), editor.SelectionColor.ToArgb());
            }
            editor.Select(untouched, 1);
            Assert.Equal(Color.Magenta.ToArgb(), editor.SelectionColor.ToArgb());
        });
    }

    [Fact]
    public void SplittingAndRejoiningAChordLineReformatsBothSides()
    {
        OnSta(() =>
        {
            using var editor = new RichTextBox();
            using var lyric = new Font("Arial", 12);
            using var chord = new Font("Consolas", 10);
            _ = editor.Handle;
            editor.Text = "lyric C »";
            string previous = null;
            Gf.FormatEditorLyrics(editor, ref previous, [], lyric, chord, formatAll: true);
            editor.Select(6, 0);
            editor.SelectedText = "\n";
            Gf.FormatEditorLyrics(editor, ref previous, [], lyric, chord);
            editor.Select(0, 1);
            Assert.Equal(lyric.Name, editor.SelectionFont.Name);
            editor.Select(7, 1);
            Assert.Equal(chord.Name, editor.SelectionFont.Name);
            editor.Select(6, 1);
            editor.SelectedText = "";
            Gf.FormatEditorLyrics(editor, ref previous, [], lyric, chord);
            editor.Select(0, 1);
            Assert.Equal(chord.Name, editor.SelectionFont.Name);
            Assert.Equal("lyric C »", editor.Text);
        });
    }

    [Fact]
    public void AutomaticRegionDetectionHandlesEmptySectionsAndRequiresAVerseAtTheStart()
    {
        Gf.InitializeVerseHeadings();
        Assert.True(Gf.IsNewR2Format("[1]\r\n가[region 2]\r\n[prechorus 2]나"));
        Assert.True(Gf.IsNewR2Format("가[region 2][region 2][1]나[region 2]"));
        Assert.False(Gf.IsNewR2Format("가[region 2]\n\n[1]나"));
        Assert.False(Gf.IsNewR2Format("가[region 2][CHORUS]"));
        Assert.False(Gf.IsNewR2Format("가[region 2][1]나[region 2]다"));
        Assert.False(Gf.IsNewR2Format("[region 2]"));
    }

    [Theory]
    [InlineData(false, "[1]\n가\n[chorus]\n나\n[2]\n다", "[1]", "(3;G;2;)(4;F;3;)", "")]
    [InlineData(true, "[1]\n가", "[chorus]\n나\n[2]\n다", "", "(1;G;2;)(2;F;3;)")]
    public void LyricRegionsKeepTheirOwnLineNumbersAndBlankTrailingRow(bool newer, string firstText,
        string secondText, string firstNotes, string secondNotes)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            const string lyrics = "[1]\n가\n[region 2]\n[chorus]\n나\n[2]\n다\n";
            const string notes = "(1;C;0;)(4;G;2;)(5;F;3;)";
            string first = "", second = "", firstChords = "", secondChords = "";
            using var rows = newer
                ? Gf.ExtractNewFormatLyrics(lyrics, notes, ref first, ref firstChords, ref second, ref secondChords)
                : Gf.ExtractDefaultFormatLyrics(lyrics, notes, ref first, ref firstChords, ref second, ref secondChords);
            Assert.Equal(firstText, first);
            Assert.Equal(secondText, second);
            Assert.Equal("(1;C;0;)" + firstNotes, firstChords);
            Assert.Equal(secondNotes, secondChords);
            Assert.Equal("", rows.Items[rows.Items.Count - 1].SubItems[2].Text);
        });
    }

    [Fact]
    public void ImportTreatsALoneRegionMarkerAsAnEmptySong()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var rows = Gf.ExtractLyrics("[region 2]", "");
            Assert.Empty(rows.Items);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RegionalChordRowsPreserveStoredOrderThroughDuplicateAndIncompleteRecords(bool newer)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            const string lyrics = "[1]\na\nb\nc\nd\ne\nf";
            const string notes = "(1;C;0;)(1;D;4;)(3;G;2;)()(5;F;8;)(unfinished";
            string first = "", second = "", firstNotes = "", secondNotes = "";
            using var rows = newer
                ? Gf.ExtractNewFormatLyrics(lyrics, notes, ref first, ref firstNotes, ref second, ref secondNotes)
                : Gf.ExtractDefaultFormatLyrics(lyrics, notes, ref first, ref firstNotes, ref second, ref secondNotes);
            Assert.Equal(new[] { "C;0;", "", "G;2;", "", "F;8;", "" },
                rows.Items.Cast<ListViewItem>().Select(row => row.SubItems[3].Text));
        });
    }

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

    [Fact]
    public void RebuildingShortAndLongSongsKeepsTheLastSlideAndVerseLookupInBounds()
    {
        OnSta(() =>
        {
            var song = new SongSettings { SplitScreens = false, FolderNo = 1 };
            using var lines = song.LyricsAndNotationsList;
            lines.Items.Add(new ListViewItem(["1", "1", "verse", "", ""]));
            song.VersePresent[1] = true;
            song.VerseLineLoc[1, 3] = song.VerseLineLoc[1, 4] = -1;
            foreach (int slideCount in new[] { 1, 1000, 1001, 3 })
            {
                song.SongSequence = new string((char)1, slideCount);
                Gf.BuildSlides(song, lines, ref song.SongSequence, ref song.TotalSlides, ref song.SongVerses,
                    ref song.ChorusSlides, ref song.Slide, 0);
                Assert.Equal(slideCount, song.TotalSlides);
                Assert.Equal(1, song.SongVerses[1]);
                Assert.Equal(1, song.Slide[slideCount, 0]);
                Assert.Equal(-1, song.Slide[slideCount, 4]);
                Assert.Equal(-1, song.Slide[0, 3]);
            }
        });
    }

    [Fact]
    public void NewSongReservesOnlyTheSentinelAndFirstRowOfEachTable()
    {
        var song = new SongSettingsBase();
        Assert.Equal(2, song.Slide.GetLength(0));
        Assert.Equal(8, song.Slide.GetLength(1));
        Assert.Equal(2, song.VerseLineLoc.GetLength(0));
        Assert.Equal(5, song.VerseLineLoc.GetLength(1));
    }

    [Fact]
    public void ExtraPagesInTheSecondRegionGrowPastTheFirstSlide()
    {
        OnSta(() =>
        {
            var song = new SongSettings { SongSequence = "\u0001", SplitScreens = false };
            using var lines = song.LyricsAndNotationsList;
            foreach (string line in new[] { "first region", "translation", "", "last translation" })
                lines.Items.Add(new ListViewItem(["", "", line, "", ""]));
            song.VersePresent[1] = true;
            song.VerseLineLoc[1, 3] = 1;
            song.VerseLineLoc[1, 4] = 3;
            Gf.BuildSlides(song, lines, ref song.SongSequence, ref song.TotalSlides, ref song.SongVerses,
                ref song.ChorusSlides, ref song.Slide, 0);
            Assert.Equal(2, song.TotalSlides);
            Assert.Equal(1, song.Slide[1, 0]);
            Assert.Equal(-1, song.Slide[2, 0]);
            Assert.Equal(-1, song.Slide[2, 1]);
            Assert.Equal(3, song.Slide[2, 3]);
            Assert.Equal(3, song.Slide[2, 4]);
        });
    }

    [Fact]
    public void FormattingRebuildsBothRegionsAndClearsRangesOfThePreviousSong()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            var song = new SongSettings { SplitScreens = false, CompleteLyrics = "[1]\none\n[region 2]\ntwo" };
            using var lines = song.LyricsAndNotationsList;
            Gf.FormatDisplayLyrics(ref song, PrepareSlides: true, UseStoredSequence: false);
            Assert.Equal(2, song.VerseLineLoc.GetLength(0));
            Assert.Equal(1, song.TotalSlides);
            Assert.Equal(1, song.Slide[1, 3]);
            Assert.True(song.VersePresent[150]);

            song.CompleteLyrics = "[prechorus 2]\nlast\nline";
            Gf.FormatDisplayLyrics(ref song, PrepareSlides: true, UseStoredSequence: false);
            Assert.Equal(112, song.Slide[1, 0]);
            Assert.Equal(0, song.VerseLineLoc[112, 1]);
            Assert.Equal(1, song.VerseLineLoc[112, 2]);
            Assert.Equal(-1, song.VerseLineLoc[112, 3]);
            Assert.False(song.VersePresent[150]);

            song.CompleteLyrics = "[1]\nnew";
            Gf.FormatDisplayLyrics(ref song, PrepareSlides: true, UseStoredSequence: false);
            Assert.Equal(1, song.SongVerses[1]);
            Assert.Equal(-1, song.VerseLineLoc[112, 1]);
            Assert.Equal(-1, song.Slide[1, 4]);
        });
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
