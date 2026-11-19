using System;
using System.Collections.Generic;
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
    [Theory]
    [InlineData("한 줄", "(0;G;0;)", "G »\n한 줄")]
    [InlineData("첫 줄\r\n\r\n마지막 줄", "(2;G;0;)", "첫 줄\n\nG »\n마지막 줄")]
    [InlineData("첫 줄\n마지막 줄\n", "(1;G;0;)", "첫 줄\nG »\n마지막 줄")]
    [InlineData("끝\n\n", "(0;G;0;)", "G »\n끝\n")]
    public void ChordPreviewIncludesTheLastLyricWithOrWithoutATerminator(string lyrics, string notes, string expected)
    {
        OnSta(() =>
        {
            using var font = new Font("Arial", 12);
            using var workspace = new RichTextBox();
            using var scratch = new RichTextBox();
            var work = workspace;
            var chords = scratch;
            Assert.Equal(expected, Gf.CombineLyricsAndNotations(lyrics, notes, font, font, ref work, ref chords));
        });
    }

    [Theory]
    [InlineData(typeof(FrmEditItem), false)]
    [InlineData(typeof(FrmInfoScreen), false)]
    [InlineData(typeof(FrmEditItem), true)]
    [InlineData(typeof(FrmInfoScreen), true)]
    public void EditorTransposeMenusKeepBothRegionsAndTheFinalLyric(Type editorType, bool setKeyAndCapo)
    {
        OnSta(() =>
        {
            using var editor = (Form)Activator.CreateInstance(editorType);
            using var font = new Font("Arial", 12);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo Field(string name) => editorType.GetField(name, fields);
            SetEditorFontsAndHeadings(editor, font, font);
            var first = (RichTextBox)Field("tbLyrics1").GetValue(editor);
            var second = (RichTextBox)Field("tbLyrics2").GetValue(editor);
            var up = (ToolStripItem)Field("Menu_TransposeUp").GetValue(editor);
            var down = (ToolStripItem)Field("Menu_TransposeDown").GetValue(editor);
            var key = (ComboBox)Field("SongKey").GetValue(editor);
            var capo = (ComboBox)Field("SongCapo").GetValue(editor);
            var timing = (ComboBox)Field("SongTiming").GetValue(editor);
            Gf.LoadSongKeyCapoTiming(ref capo, ref key, ref timing);
            if (setKeyAndCapo)
            {
                key.SelectedItem = "G";
                capo.SelectedItem = "Capo 0";
            }
            first.Text = "첫 줄\n\nG/B »\n마지막 줄";
            second.Text = "Am(add9)            G/B »\ntranslation continues into the next phrase\n\n";
            Gf.ReadEditorNotations(second, out string originalChords, out string translation);
            up.PerformClick();
            string bassChord = setKeyAndCapo ? "Ab/C" : "G#/C";
            string minorChord = setKeyAndCapo ? "Bbm" : "A#m";
            Assert.Equal($"첫 줄\n\n{bassChord} »\n마지막 줄", first.Text);
            string[] secondRows = second.Text.Split('\n', 2);
            Assert.Equal(new[] { minorChord + "(add9)", bassChord, "»" }, secondRows[0].Split(' ', StringSplitOptions.RemoveEmptyEntries));
            Assert.Equal(translation, secondRows[1]);
            Assert.Equal(originalChords.Replace("Am(add9)", minorChord + "(add9)").Replace("G/B", bassChord),
                Field("Lyrics2SavedNotations").GetValue(editor));
            Assert.Equal($"(2;{bassChord};0;)", Field("Lyrics1SavedNotations").GetValue(editor));
            Assert.Equal(setKeyAndCapo ? "Ab" : "", key.Text);
            Assert.Equal(setKeyAndCapo ? "Capo 11" : "", capo.Text);
            down.PerformClick();
            Assert.Equal("첫 줄\n\nG/B »\n마지막 줄", first.Text);
            secondRows = second.Text.Split('\n', 2);
            Assert.Equal(new[] { "Am(add9)", "G/B", "»" }, secondRows[0].Split(' ', StringSplitOptions.RemoveEmptyEntries));
            Assert.Equal(translation, secondRows[1]);
            Assert.Equal(setKeyAndCapo ? "G" : "", key.Text);
            Assert.Equal(setKeyAndCapo ? "Capo 0" : "", capo.Text);
        });
    }

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

    [Theory]
    [InlineData("Arial", "Malgun Gothic", 1f)]
    [InlineData("Malgun Gothic", "Consolas", 1.5f)]
    public void PreviewSpaceRunsMatchNativeCaretPositions(string lyricFamily, string chordFamily, float zoom)
    {
        OnSta(() =>
        {
            using var lyricFont = new Font(lyricFamily, 12);
            using var chordFont = new Font(chordFamily, 14);
            using var lyricBox = new RichTextBox { Width = 2400, WordWrap = false, ZoomFactor = zoom };
            using var chordBox = new RichTextBox { Width = 2400, WordWrap = false, ZoomFactor = zoom };
            var work = lyricBox;
            var scratch = chordBox;
            const string lyric = "한글 lyric many columns\tthrough the end 🎵 with more text";
            (string Chord, int Position)[] notes = [("C", 0), ("אב", 6), ("G/B", 17), ("Am", lyric.Length), ("Dm", 3)];
            work.Text = lyric + "\n";
            Gf.MarkSelectedRTB(ref work, 0, work.TextLength, 0, lyricFont, chordFont);
            string expected = "";
            foreach (var (chord, position) in notes)
            {
                int target = work.GetPositionFromCharIndex(position).X - 4;
                while (scratch.GetPositionFromCharIndex(Math.Max(0, expected.Length - 1)).X < target)
                {
                    expected += " ";
                    Assert.True(expected.Length < 1000);
                    scratch.Text = expected;
                    Gf.MarkSelectedRTB(ref scratch, 0, scratch.TextLength, 2, lyricFont, chordFont);
                }
                if (expected.Length > 1 && expected[^1] != ' ')
                    expected += " ";
                expected += chord;
                scratch.Text = expected;
                Gf.MarkSelectedRTB(ref scratch, 0, scratch.TextLength, 2, lyricFont, chordFont);
            }
            string fields = string.Concat(notes.Select(note => $"{note.Chord};{note.Position};"));
            Assert.Equal(expected + " »\n" + lyric, Gf.CombineLyricsAndNotations(lyric, $"(0;{fields})",
                lyricFont, chordFont, ref work, ref scratch));
        });
    }

    [Fact]
    public void SavingEditorNotationsKeepsLyricRowsColumnsAndBlankLines()
    {
        OnSta(() =>
        {
            using var font = new Font("Consolas", 12);
            using var editor = new RichTextBox { Font = font, WordWrap = false, Width = 900 };
            editor.Text = "C   G/B »\r\nabcdefghi\r\n\r\n  »»\r\n\r\n  Am »\r\nfinal";
            Gf.ReadEditorNotations(editor, out string notes, out string lyrics);
            Assert.Equal("(0;C;0;G/B;4;)(4;Am;2;)", notes);
            Assert.Equal("abcdefghi\n\n\n\nfinal", lyrics);
        });
    }

    [Theory]
    [InlineData(false, "가나\n\n끝\n")]
    [InlineData(true, "가----나\n\nG »\n끝\n")]
    public void SlidePreviewKeepsBlankRowsRelativeChordPositionsAndOneFinalNewline(bool chords, string expected)
    {
        OnSta(() =>
        {
            using var workspace = new RichTextBox();
            using var scratch = new RichTextBox();
            var previous = (Gf.tbWorkspace, Gf.tbTempSpace);
            (Gf.tbWorkspace, Gf.tbTempSpace) = (workspace, scratch);
            try
            {
                var song = new SongSettings { Type = "B" };
                using var rows = song.LyricsAndNotationsList;
                using var font = new Font("Arial", 12);
                foreach (var (text, notes) in new[] { ("지역 1", ""), ("가----나", ""), ("", ""), ("끝\u0098", "G;0;"), ("", "") })
                    rows.Items.Add(new ListViewItem(["", "", text, notes]));
                song.Slide[1, 1] = song.Slide[1, 2] = 0;
                song.Slide[1, 3] = 1;
                song.Slide[1, 4] = 22;
                Assert.Equal("지역 1\n", Gf.GetSlideContents(song, 1, 0, font, chords));
                Assert.Equal(expected, Gf.GetSlideContents(song, 1, 1, font, chords));
                song.Slide[1, 3] = song.Slide[1, 4] = -1;
                Assert.Equal("\n", Gf.GetSlideContents(song, 1, 1, font, chords));
            }
            finally
            {
                (Gf.tbWorkspace, Gf.tbTempSpace) = previous;
            }
        });
    }

    [Theory]
    [InlineData("Arial", "Consolas", false)]
    [InlineData("맑은 고딕", "Arial", false)]
    [InlineData("Consolas", "맑은 고딕", true)]
    public void ChordLayoutMatchesTheFirstSpaceThatReachesEachLyricPosition(string lyricsFamily, string chordFamily, bool printer)
    {
        OnSta(() =>
        {
            using var list = new ListView();
            using var graphics = list.CreateGraphics();
            using var lyricFont = new Font(lyricsFamily, 14);
            using var chordFont = new Font(chordFamily, 11);
            using var format = StringFormat.GenericDefault;
            using var typographic = StringFormat.GenericTypographic;
            const string lyrics = "가사  mixed words and emoji 🎵                          끝";
            (string Chord, int Position)[] chords = [("C", 0), ("D", 0), ("Am", 3), ("G/B", lyrics.Length), ("F", 2)];
            int markerWidth = (int)graphics.MeasureString("i", chordFont, 1000, typographic).Width;
            string screenLine = "", printLine = "";
            foreach (var (chord, position) in chords)
            {
                string prefix = lyrics[..position];
                if (prefix.EndsWith(' '))
                    prefix = prefix[..^1] + "i";
                int target = (int)graphics.MeasureString(prefix, lyricFont, 32000, format).Width + markerWidth;
                int spaces = 0;
                while (graphics.MeasureString(screenLine + "i", chordFont, 32000, format).Width < target)
                {
                    screenLine += " ";
                    spaces++;
                }
                if (screenLine.Length > 0 && !screenLine.EndsWith(' '))
                    screenLine += " ";
                screenLine += chord;
                printLine += new string(' ', spaces + spaces / 12) + chord;
            }
            int previous = Gf.PB_PrinterSpaces;
            try
            {
                Gf.PB_PrinterSpaces = printer ? 1 : 0;
                string notes = string.Concat(chords.Select(pair => $"{pair.Chord};{pair.Position};"));
                Assert.Equal(printer ? printLine : screenLine, Gf.FormatNotationString(list, lyrics, notes, lyricFont, chordFont));
                Assert.Equal(" ", Gf.FormatNotationString(list, lyrics, "C;-1;", lyricFont, chordFont));
                Assert.Throws<FormatException>(() => Gf.FormatNotationString(list, lyrics, "C;bad;", lyricFont, chordFont));
            }
            finally
            {
                Gf.PB_PrinterSpaces = previous;
            }
        });
    }

    [Theory]
    [InlineData(15)]
    [InlineData(800)]
    public void DocumentLayoutKeepsChordOffsetsWhenALyricLineWraps(int width)
    {
        OnSta(() =>
        {
            using var list = new ListView();
            var rows = list;
            using var font = new Font("Arial", 12);
            const string lyrics = "첫줄 second last";
            Gf.SubDivideTextAndNotations(lyrics, "C;0;Am;3;G;10;", font, font, ref rows, width);
            Assert.True(rows.Items.Count > 1);
            Assert.Equal(lyrics, string.Concat(rows.Items.Cast<ListViewItem>().Select(row => row.Text)));
            var positions = new List<(string Chord, int Position)>();
            foreach (ListViewItem row in rows.Items)
            {
                int offset = int.Parse(row.SubItems[3].Text);
                string[] fields = row.SubItems[2].Text.Split(';');
                for (int i = 0; i + 1 < fields.Length; i += 2)
                    positions.Add((fields[i], offset + int.Parse(fields[i + 1])));
                Assert.Equal(Gf.FormatNotationString(rows, row.Text, row.SubItems[2].Text, font, font),
                    row.SubItems[1].Text);
            }
            Assert.Equal(new[] { ("C", 0), ("Am", 3), ("G", 10) }, positions);
        });
    }

    [Fact]
    public void ChordsAreStoredOnlyForVisibleLinesAndLaterRecordsReplaceEarlierOnes()
    {
        var chords = Gf.GetNotationsByLine(
            "(32000;Am;0;)(0;C;0;)(999999;Em;0;)(0;G;3;)(8;F;0;)(8;)(-1;)(7;D;0;)", 32001);

        Assert.Equal(3, chords.Count);
        Assert.Equal("Am;0;", chords[32000]);
        Assert.Equal("G;3;", chords[0]);
        Assert.Equal("", chords[8]);
        Assert.Empty(Gf.GetNotationsByLine("(0;C;0;)", 0));
    }

    [Theory]
    [InlineData("skip(2;Am;3;)tail", 2, "Am;3;", "tail")]
    [InlineData("()tail", -1, "", "tail")]
    [InlineData("(;C;0;)", 0, "C;0;", "")]
    [InlineData("(bad;D;1;)", 0, "D;1;", "")]
    [InlineData("(3)", 3, "", "")]
    [InlineData("(unfinished", -1, "previous", "(unfinished")]
    public void StoredNotationRecordsKeepTheirLineNumberChordsAndRemainder(
        string input, int lineNumber, string chords, string remainder)
    {
        string result = "previous";
        Assert.Equal(lineNumber, Gf.ExtractOneNotationsLine(ref input, ref result));
        Assert.Equal(chords, result);
        Assert.Equal(remainder, input);
    }

    [Fact]
    public void CombinedLyricsKeepBlankLinesAndTheLatestChordInTheRightPosition()
    {
        OnSta(() =>
        {
            using var lyricsBox = new RichTextBox();
            using var chordBox = new RichTextBox();
            using var font = new Font("Arial", 12);
            var lyrics = lyricsBox;
            var chords = chordBox;
            Assert.Equal("첫 줄\n\nG »\n끝 줄", Gf.CombineLyricsAndNotations(
                "첫 줄\r\n\r\n끝 줄\r\n", "(2;C;0;)(32000;X;0;)(2;G;0;)", font, font, ref lyrics, ref chords));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothLyricFormatsRecognizeVerseAndRegionHeadingsButKeepOtherText(bool newFormat)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            const string lyrics = "[99]\n한글\n[region 2]\n번역\n[chorus]\n[CHORUS]";
            string first = "", second = "", firstNotes = "", secondNotes = "";
            using var rows = newFormat
                ? Gf.ExtractNewFormatLyrics(lyrics, "", ref first, ref firstNotes, ref second, ref secondNotes)
                : Gf.ExtractDefaultFormatLyrics(lyrics, "", ref first, ref firstNotes, ref second, ref secondNotes);
            Assert.Equal(new[] { "99", "99", "0" }, rows.Items.Cast<ListViewItem>().Select(row => row.Text));
            Assert.Equal(new[] { "1", "2", newFormat ? "2" : "1" },
                rows.Items.Cast<ListViewItem>().Select(row => row.SubItems[1].Text));
            Assert.Equal(new[] { "한글", "번역", "[CHORUS]" },
                rows.Items.Cast<ListViewItem>().Select(row => row.SubItems[2].Text));
            Assert.EndsWith("[prechorus],[prechorus 2],[ending],[region 2]", Gf.SymbolsString);
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

    [Fact]
    public void ScreenBreaksSkipUnknownAndUnclosedBracketsAndContinueToLaterVerses()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            const string text = "[메모] 앞 [미완성\n[prechorus]가\n\n나[prechorus]다[prechorus 2]라\n[region 2] [CHORUS] [";
            using var editor = new RichTextBox { Text = "\n" + text };
            using var breaks = new ListView();
            var lyrics = editor;
            var rows = breaks;
            bool complete = false;
            Gf.MapLyricsBreak(ref rows, ref lyrics, ref complete);
            Assert.True(complete);
            Assert.Equal(text, lyrics.Text);
            Assert.Equal(new[]
            {
                (0, "", 0),
                (text.IndexOf("[prechorus]", StringComparison.Ordinal), "[prechorus]", 0),
                (text.IndexOf('나'), "[prechorus]", 1),
                (text.LastIndexOf("[prechorus]", StringComparison.Ordinal), "[prechorus]", 2),
                (text.IndexOf("[prechorus 2]", StringComparison.Ordinal), "[prechorus 2]", 0)
            }, rows.Items.Cast<ListViewItem>().Select(row =>
                (int.Parse(row.Text), row.SubItems[1].Text, int.Parse(row.SubItems[2].Text))));

            int position = 0, length = 1, page = -1;
            string heading = "";
            Gf.GetBreakPosition(rows, text.IndexOf("[prechorus]", StringComparison.Ordinal), 1,
                ref position, ref length, ref heading, ref page);
            Assert.Equal((text.IndexOf('나'), "[prechorus]", 1), (position, heading, page));

            const string translation = "[prechorus]first page\n\nsecond page";
            lyrics.Text = translation;
            complete = false;
            Gf.MapLyricsBreak(ref rows, ref lyrics, ref complete);
            Gf.GetBreakPosition(rows, ref position, ref length, heading, page);
            Assert.Equal((translation.IndexOf("second", StringComparison.Ordinal), -1), (position, length));
        });
    }

    [Fact]
    public void ScreenBreakHeadingsIncludeBothPrechorusesAndExcludeOtherBracketedText()
    {
        Gf.InitializeVerseHeadings();
        foreach (string heading in new[] { "[1]", "[99]", "[chorus]", "[ending]", "[prechorus]", "[prechorus 2]" })
            Assert.Equal(heading, Gf.ValidateVerseIndicator("가" + heading + "나", 1));
        foreach (string text in new[] { "[region 2]", "[note]", "[CHORUS]", "[[1]", "[1\n]", "[" })
            Assert.Equal("", Gf.ValidateVerseIndicator(text, 0));
    }

    [Theory]
    [InlineData("", "0||0")]
    [InlineData("가\n\n나", "0||0;3||1")]
    [InlineData("가[1]나", "0||0;1|[1]|0")]
    [InlineData("[[1]]", "0||0;1|[1]|0")]
    [InlineData("[1]가\n\n[2]나", "0|[1]|0;6|[1]|1;6|[2]|0")]
    public void ScreenBreaksKeepEditorOffsetsAndPageNumbers(string text, string expected)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var editor = new RichTextBox { Text = text };
            using var breaks = new ListView();
            var lyrics = editor;
            var rows = breaks;
            breaks.Items.Add("previous map");
            bool complete = false;
            Gf.MapLyricsBreak(ref rows, ref lyrics, ref complete);
            Assert.True(complete);
            Assert.Equal(expected, string.Join(";", rows.Items.Cast<ListViewItem>()
                .Select(row => $"{row.Text}|{row.SubItems[1].Text}|{row.SubItems[2].Text}")));
        });
    }

    [Fact]
    public void StrokeTableKeepsPositiveTwoDigitCountsForUtf16Characters()
    {
        var counts = new byte[char.MaxValue + 1];
        Gf.ReadStrokeCounts("一01漢13漢00漢xx\uFFFF99\r\n", counts);
        Assert.Equal((byte)1, counts['一']);
        Assert.Equal((byte)13, counts['漢']);
        Assert.Equal((byte)99, counts['\uFFFF']);
        Assert.Equal((byte)0, counts['가']);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public void WholeWordBibleSearchMatchesBoundTermsAtVerseEdgesAndPunctuation(int matchMode, int expectedRows)
    {
        using var connection = new System.Data.SQLite.SQLiteConnection("Data Source=:memory:");
        connection.Open();
        using var setup = connection.CreateCommand();
        setup.CommandText = """
            CREATE TABLE Bible(book INTEGER, chapter INTEGER, verse INTEGER, bibletext TEXT);
            INSERT INTO Bible VALUES (2,1,1,'David''s  시!'), (2,1,2,'[시]'), (2,1,3,'시작'), (1,1,1,'시');
            """;
        setup.ExecuteNonQuery();
        var query = Gf.BuildBibleSearchQuery("  DAVID'S  시  ", -1, 2, matchMode);
        Assert.DoesNotContain("david", query.Text);
        using var matches = Easislides.SQLite.DbController.GetDataTable(connection, query.Text, query.Parameters);
        Assert.Equal(expectedRows, matches.Rows.Count);
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
