using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.Util;
using Xunit;

namespace Axicultus.Tests;

// Adversarial tests of the song editor (FrmEditItem and Merge_Songs). The test that reads real songs needs
// AXICULTUS_REAL_SONG_DB (a song database, opened read-only) and is skipped without it; with it, the tests also
// write reports to <repo>/_review.
[Collection("WinForms Bible globals")]
public class SongEditorAdversarialTests
{
    private static string RealDb => Environment.GetEnvironmentVariable("AXICULTUS_REAL_SONG_DB") ?? "";

    /// <summary>Runs when AXICULTUS_REAL_SONG_DB names a song database; skipped otherwise.</summary>
    private sealed class RealSongsFactAttribute : FactAttribute
    {
        public RealSongsFactAttribute()
        {
            if (!File.Exists(RealDb))
                Skip = "Set AXICULTUS_REAL_SONG_DB to a song database (it is opened read-only).";
        }
    }

    // A report for whoever runs the tests on real songs. Without the variable nothing is written.
    private static void WriteReport(string name, string text)
    {
        if (!File.Exists(RealDb))
            return;
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Axicultus.sln")))
            dir = dir.Parent;
        File.WriteAllText(Path.Combine(Directory.CreateDirectory(Path.Combine(dir!.FullName, "_review")).FullName, name), text);
    }

    // Merge_Songs as it was before the change (base0 gfLyrics.cs 2419-2487), unchanged but for the names it reaches.
    private static void OldMergeSongs(SongSettings InItem1, SongSettings InItem2, ref string CombinedLyrics, ref string CombinedNotations)
    {
        Gf.FormatDisplayLyrics(ref InItem1, PrepareSlides: false, UseStoredSequence: true);
        Gf.FormatDisplayLyrics(ref InItem2, PrepareSlides: false, UseStoredSequence: true);
        ListViewItem listViewItem = new();
        Gf.TempItem1.LyricsAndNotationsList.Items.Clear();
        bool keepFirstHeading = false;
        for (int i = 0; i < InItem1.SongSequence.Length; i++)
        {
            int verseIndex = InItem1.SongSequence[i];
            if (i == 0 && (InItem1.CompleteLyrics.Contains(Gf.VerseSymbol[verseIndex], StringComparison.CurrentCulture) || InItem2.CompleteLyrics.Contains(Gf.VerseSymbol[verseIndex], StringComparison.CurrentCulture)))
            {
                keepFirstHeading = true;
            }
            listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(Gf.VerseSymbol[verseIndex]);
            listViewItem.SubItems.Add("");
            for (int j = InItem1.VerseLineLoc[verseIndex, 1]; j >= 0 && j <= InItem1.VerseLineLoc[verseIndex, 2]; j++)
            {
                listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[2].Text);
                listViewItem.SubItems.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[3].Text);
            }
            if (InItem2.VersePresent[verseIndex] && InItem2.CompleteLyrics != "")
            {
                listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(Gf.VerseSymbol[150]);
                listViewItem.SubItems.Add("");
                for (int k = InItem2.VerseLineLoc[verseIndex, 1]; k >= 0 && k <= InItem2.VerseLineLoc[verseIndex, 2]; k++)
                {
                    listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
                    listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
                }
                InItem2.VersePresent[verseIndex] = false;
            }
        }
        for (int i = 0; i <= 112; i++)
        {
            if (InItem2.VersePresent[i] && InItem2.CompleteLyrics != "")
            {
                int verseIndex = i;
                listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(Gf.VerseSymbol[verseIndex]);
                listViewItem.SubItems.Add("");
                listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(Gf.VerseSymbol[150]);
                listViewItem.SubItems.Add("");
                for (int k = InItem2.VerseLineLoc[verseIndex, 1]; k >= 0 && k <= InItem2.VerseLineLoc[verseIndex, 2]; k++)
                {
                    listViewItem = Gf.TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
                    listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
                }
            }
        }
        if (!keepFirstHeading && Gf.TempItem1.LyricsAndNotationsList.Items.Count > 0)
        {
            Gf.TempItem1.LyricsAndNotationsList.Items[0].Remove();
        }
        CombinedLyrics = "";
        for (int i = 0; i < Gf.TempItem1.LyricsAndNotationsList.Items.Count; i++)
        {
            CombinedLyrics = CombinedLyrics + "\n" + Gf.TempItem1.LyricsAndNotationsList.Items[i].SubItems[0].Text;
        }
        CombinedLyrics = DataUtil.Mid(CombinedLyrics, 1);
        CombinedNotations = "";
        for (int i = 0; i < Gf.TempItem1.LyricsAndNotationsList.Items.Count; i++)
        {
            if (Gf.TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text != "")
            {
                CombinedNotations = CombinedNotations + "(" + Convert.ToString(i) + ";" + Gf.TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text + ")";
            }
        }
        CombinedLyrics = DataUtil.TrimEnd(CombinedLyrics);
    }

    private static (string Lyrics, string Chords) Merge(bool old, string first, string firstChords, string second, string secondChords)
    {
        var item1 = new SongSettings { CompleteLyrics = first, Notations = firstChords, OriginalNotations = firstChords, Capo = -1 };
        var item2 = new SongSettings { CompleteLyrics = second, Notations = secondChords, OriginalNotations = secondChords };
        string lyrics = "", chords = "";
        try
        {
            if (old)
                OldMergeSongs(item1, item2, ref lyrics, ref chords);
            else
                Gf.Merge_Songs(item1, item2, ref lyrics, ref chords);
        }
        catch (Exception ex)
        {
            return ("throws " + ex.GetType().Name, "");
        }
        finally
        {
            item1.LyricsAndNotationsList.Dispose();
            item2.LyricsAndNotationsList.Dispose();
        }
        return (lyrics, chords);
    }

    private static string Esc(string text) => text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

    private static List<(int Id, string Lyrics, string Msc, string Sequence)> ReadRealSongs()
    {
        var songs = new List<(int, string, string, string)>();
        using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={RealDb};Read Only=True;Pooling=False");
        connection.Open();
        using var command = new System.Data.SQLite.SQLiteCommand("SELECT SONGID, LYRICS, MSC, SEQUENCE FROM SONG ORDER BY SONGID", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string Text(int column) => reader.IsDBNull(column) ? "" : Convert.ToString(reader.GetValue(column)) ?? "";
            songs.Add((Convert.ToInt32(reader.GetValue(0)), Text(1), Text(2), Text(3)));
        }
        return songs;
    }

    private static IEnumerable<string> LyricLines(string lyrics) => lyrics.Split('\n').Where(line => line.Length > 0 && !line.StartsWith('[')).Order(StringComparer.Ordinal);

    // Old and new Merge_Songs over the panes of every real song (what an untouched FrmEditItem save stores).
    [RealSongsFact]
    public void RealSongs_TheNewMergeSongs_HoldsTheLyricLinesOfTheOldOne_AndChangesNoMoreStoredSongs()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            int showNotations = Gf.ShowNotations;
            Gf.ShowNotations = 0;
            try
            {
                var report = new StringBuilder();
                var songs = ReadRealSongs();
                int differing = 0, newChangesStored = 0, oldChangesStored = 0;
                var otherLines = new List<int>();
                foreach (var song in songs)
                {
                    string text1 = "", chords1 = "", text2 = "", chords2 = "";
                    using (Gf.ExtractLyrics(song.Lyrics, song.Msc, ref text1, ref chords1, ref text2, ref chords2)) { }
                    var now = Merge(false, text1, chords1, text2, chords2);
                    var before = Merge(true, text1, chords1, text2, chords2);
                    string stored = song.Lyrics.Replace("\r\n", "\n").TrimEnd('\n', '\r');
                    if (now != (stored, song.Msc)) newChangesStored++;
                    if (before != (stored, song.Msc)) oldChangesStored++;
                    if (!LyricLines(now.Lyrics).SequenceEqual(LyricLines(before.Lyrics)))
                        otherLines.Add(song.Id);
                    if (now != before)
                    {
                        differing++;
                        report.AppendLine($"song {song.Id}: old==stored {before == (stored, song.Msc)}, new==stored {now == (stored, song.Msc)}")
                            .AppendLine($"  stored <{Esc(stored)}> msc <{song.Msc}>")
                            .AppendLine($"  old    <{Esc(before.Lyrics)}> msc <{before.Chords}>")
                            .AppendLine($"  new    <{Esc(now.Lyrics)}> msc <{now.Chords}>");
                    }
                }
                report.Insert(0, $"songs {songs.Count}; old and new Merge_Songs differ on {differing}; an untouched save changes the stored text: old {oldChangesStored}, new {newChangesStored}\n");
                WriteReport("merge-old-new-real.txt", report.ToString());
                Assert.Empty(otherLines);
                Assert.True(newChangesStored <= oldChangesStored, $"an untouched save changes the stored text: old {oldChangesStored}, new {newChangesStored}");
            }
            finally
            {
                Gf.ShowNotations = showNotations;
            }
        }, 600);
    }

    // Old and new Merge_Songs on made-up panes: every difference, grouped by what kind it is.
    [Fact]
    public void Fuzz_OldAndNewMergeSongs_HoldTheSameLyricLines_AndNeitherThrows()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            int showNotations = Gf.ShowNotations;
            Gf.ShowNotations = 0;
            try
            {
                var random = new Random(7);
                string[] alphabet = ["", "", "a", "b", "가사", "[1]", "[2]", "[3]", "[chorus]", "[bridge]", "[ending]", " "];
                string Text() => string.Join("\n", Enumerable.Range(0, random.Next(0, 9)).Select(_ => alphabet[random.Next(alphabet.Length)]));
                string Notes(string text)
                {
                    var notes = new StringBuilder();
                    string[] lines = text.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                        if (lines[i].Trim().Length > 0 && !lines[i].StartsWith('[') && random.Next(3) == 0)
                            notes.Append($"({i};G;0;)");
                    return notes.ToString();
                }
                var kinds = new SortedDictionary<string, List<string>>();
                int cases = 20000, differing = 0;
                for (int i = 0; i < cases; i++)
                {
                    string t1 = Text(), t2 = random.Next(3) == 0 ? "" : Text();
                    string n1 = Notes(t1), n2 = Notes(t2);
                    var now = Merge(false, t1, n1, t2, n2);
                    var before = Merge(true, t1, n1, t2, n2);
                    if (now == before)
                        continue;
                    differing++;
                    string[] a = before.Lyrics.Split('\n'), b = now.Lyrics.Split('\n');
                    static bool Words(string line) => line.Length > 0 && !line.StartsWith('[');
                    string kind =
                        before.Lyrics.StartsWith("throws") || now.Lyrics.StartsWith("throws") ? "one of them throws"
                        : !a.Where(Words).Order().SequenceEqual(b.Where(Words).Order()) ? "LYRIC LINES LOST OR ADDED"
                        : !a.Where(Words).SequenceEqual(b.Where(Words)) ? "lyric lines in another order"
                        : a.Count(line => line == "[region 2]") != b.Count(line => line == "[region 2]") ? "number of region markers"
                        : a.Count(line => line.Length == 0) != b.Count(line => line.Length == 0) ? "empty lines"
                        : "headings only";
                    bool pane1Only = t2.Length == 0, pane2Only = t1.Length == 0;
                    kind += pane1Only ? " (pane 1 only)" : pane2Only ? " (pane 2 only)" : " (both panes)";
                    if (!kinds.TryGetValue(kind, out var list))
                        kinds[kind] = list = [];
                    list.Add($"pane1 <{Esc(t1)}> {n1} pane2 <{Esc(t2)}> {n2}\n      old <{Esc(before.Lyrics)}> {before.Chords}\n      new <{Esc(now.Lyrics)}> {now.Chords}");
                }
                var report = new StringBuilder($"cases {cases}; old and new differ on {differing}\n");
                foreach (var (kind, list) in kinds)
                {
                    report.AppendLine($"== {kind}: {list.Count}");
                    foreach (string example in list.OrderBy(text => text.Length).Take(6))
                        report.AppendLine("   " + example);
                }
                WriteReport("merge-old-new-fuzz.txt", report.ToString());
                Assert.DoesNotContain(kinds.Keys, kind => kind.StartsWith("LYRIC LINES LOST OR ADDED", StringComparison.Ordinal) || kind.StartsWith("one of them throws", StringComparison.Ordinal));
            }
            finally
            {
                Gf.ShowNotations = showNotations;
            }
        }, 600);
    }

    // FrmEditItem's own checks (ValidateContents) on pane text. Text with a heading that shares its line is left
    // out: FrmEditItem rewrites it through the clipboard.
    private sealed class Editor : IDisposable
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private readonly FrmEditItem _form = new();
        private readonly Font _font = new("Arial", 12);

        public Editor()
        {
            Field("MainFont").SetValue(_form, _font);
            Field("NotationFont").SetValue(_form, _font);
            Field("sArray").SetValue(_form, Gf.SymbolsString.Split(','));
            ((TextBox)Field("SongTitle").GetValue(_form)).Text = "Title";
            Field("InitLoad").SetValue(_form, false);
        }

        private static FieldInfo Field(string name) => typeof(FrmEditItem).GetField(name, Members);

        public string Message1 => ((Label)Field("LabeltbLyrics").GetValue(_form)).Text;

        public string Message2 => ((Label)Field("LabeltbLyrics2").GetValue(_form)).Text;

        public static bool NeedsClipboard(string pane1, string pane2)
        {
            foreach (string text in (string[])[pane1, pane2])
            {
                for (int verse = 0; verse <= 150; verse++)
                {
                    string symbol = Gf.VerseSymbol[verse];
                    if (symbol == "" || (!pane1.Contains(symbol) && !pane2.Contains(symbol)))
                        continue;
                    int at = text.IndexOf(symbol, StringComparison.Ordinal);
                    if (at > 0 && (text[at - 1] != '\n' || at + symbol.Length >= text.Length || text[at + symbol.Length] != '\n'))
                        return true;
                }
            }
            return false;
        }

        public (bool Valid, string Message) Validate(string pane1, string pane2)
        {
            Field("IgnoreChange").SetValue(_form, true);
            ((RichTextBox)Field("tbLyrics1").GetValue(_form)).Text = pane1;
            ((RichTextBox)Field("tbLyrics2").GetValue(_form)).Text = pane2;
            Field("IgnoreChange").SetValue(_form, false);
            typeof(FrmEditItem).GetMethod("UpdateVersesList", Members).Invoke(_form, null);
            bool valid = (bool)typeof(FrmEditItem).GetMethod("ValidateContents", Members, Type.EmptyTypes).Invoke(_form, null);
            return (valid, valid ? "" : (Message1.Contains("Region 1") ? "" : "1: " + Message1) + (Message2.StartsWith("Additional") ? "" : " 2: " + Message2));
        }

        public (bool Valid, string Lyrics, string Notations) Save(string pane1, string pane2)
        {
            if (NeedsClipboard(pane1, pane2))
                throw new InvalidOperationException("this text would go through the clipboard");
            Field("IgnoreChange").SetValue(_form, true);
            ((RichTextBox)Field("tbLyrics1").GetValue(_form)).Text = pane1;
            ((RichTextBox)Field("tbLyrics2").GetValue(_form)).Text = pane2;
            Field("IgnoreChange").SetValue(_form, false);
            typeof(FrmEditItem).GetMethod("UpdateVersesList", Members).Invoke(_form, null);
            bool valid = (bool)typeof(FrmEditItem).GetMethod("ValidateAllDetails", Members).Invoke(_form, null);
            return (valid, (string)Field("CombinedLyrics").GetValue(_form), (string)Field("CombinedNotations").GetValue(_form));
        }

        // The "insert heading" menu of pane 1 with the caret at the given place; returns the pane text.
        public string InsertHeading(string pane1, int caret, int verse)
        {
            var box = (RichTextBox)Field("tbLyrics1").GetValue(_form);
            Field("IgnoreChange").SetValue(_form, true);
            box.Text = pane1;
            box.SelectionStart = caret;
            typeof(FrmEditItem).GetMethod("InsertMarker", Members).Invoke(_form, [1, verse]);
            return box.Text;
        }

        public void Dispose()
        {
            _form.Dispose();
            _font.Dispose();
        }
    }

    [Theory]
    [InlineData("[1]\nverse", "", "")]
    [InlineData("[2]\nverse", "", "")]
    [InlineData("\n[2]\nverse", "", "")]                 // empty lines above the first heading are not lyrics above it
    [InlineData("intro\n[2]\nverse", "", "1: Indicator is required for the lyrics before Verse 2")]
    // A pane that starts with an empty line: the save stores it without that line, as text that is refused the next time.
    [InlineData("\nintro\n[2]\nverse", "", "1: Indicator is required for the lyrics before Verse 2")]
    [InlineData("\n\nintro\n[2]\nverse", "", "1: Indicator is required for the lyrics before Verse 2")]
    [InlineData(" \nintro\n[2]\nverse", "", "1: Indicator is required for the lyrics before Verse 2")]
    [InlineData("\nintro\n[chorus]\nc\n[2]\nverse", "", "1: Indicator is required for the lyrics before the ")]
    [InlineData("[1]\na\n[3]\nc", "", "1: Verse 2 indicator is missing.")]
    [InlineData("[1]\na\n[2]\nb\n[1]\nc", "", "1: Duplicate Verse 1 indicator found - please amend.")]
    [InlineData("[1]\na", "x\n[1]\nb", " 2: Indicator is required for the lyrics before Verse 1")]
    [InlineData("[1]\na", "\nx\n[1]\nb", " 2: Indicator is required for the lyrics before Verse 1")]
    public void FrmEditItem_ValidateContents_AcceptsAPaneOrRefusesItWithThisMessage(string pane1, string pane2, string refusal)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var editor = new Editor();
            var (valid, message) = editor.Validate(pane1, pane2);
            Assert.Equal(refusal.Length == 0, valid);
            Assert.StartsWith(refusal, message);
        }, 60);
    }

    // What the whole FrmEditItem save path (ValidateAllDetails) stores for a pane as typed. These are the cases
    // FrmEditItem saves without a word although the text is not what the operator sees; the WPF editor refuses
    // the first two, and the heading menu no longer makes the first.
    [Theory]
    [InlineData("[1]\nfirst line\nG   D »\n[2]\nsecond verse", "[1]\nfirst line\n[2]\nsecond verse", "")]   // a chord line right above a heading: the chords are dropped
    [InlineData("first\nG »\n\nlast", "first\nG\n\nlast", "")]                                                 // a chord line above an empty line becomes lyric text
    // A lyric line that ends with the marker character is a chord line in both apps: LYRICS/MSC cannot tell the
    // two apart, so this stays until the stored format changes.
    [InlineData("Il dit: « Viens »\net suis-moi", "et suis-moi", "(0;Il;")]
    public void FrmEditItem_ValidateAllDetails_StoresThisForAPane(string pane1, string lyrics, string notationsStart)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var editor = new Editor();
            var result = editor.Save(pane1, "");
            Assert.True(result.Valid);
            Assert.Equal(lyrics, result.Lyrics);
            Assert.StartsWith(notationsStart, result.Notations);
            if (notationsStart.Length == 0)
                Assert.Equal("", result.Notations);
        }, 60);
    }

    // The heading menu: a chord line belongs to the lyric line below it, so the heading goes above the two.
    [Theory]
    [InlineData("[1]\nfirst line\nG   D »\nsecond verse", "second", "[1]\nfirst line\n[2]\nG   D »\nsecond verse")]
    [InlineData("[1]\nfirst line\nsecond verse", "second", "[1]\nfirst line\n[2]\nsecond verse")]
    [InlineData("G »\nfirst line", "first", "[2]\nG »\nfirst line")]
    public void FrmEditItem_InsertHeading_PutsItAboveTheChordLineOfTheCaretLine(string pane, string caretAt, string expected)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            using var editor = new Editor();
            Assert.Equal(expected, editor.InsertHeading(pane, pane.IndexOf(caretAt, StringComparison.Ordinal) + 2, 2));
        }, 60);
    }

    // A symbol inside a line is lyric text ("[1]  " is shown as a line of the verse). Merge_Songs used to add a
    // heading for it; the saved lyrics then held the symbol twice and FrmEditItem refused the song the next time.
    [Theory]
    [InlineData("[1]  \nfirst\n[chorus]\nrefrain")]
    [InlineData("[1]glued\nfirst\n[2]\nsecond")]
    [InlineData("[1]\nfirst\n[2]\nsecond")]
    [InlineData("first\n[2]\nsecond")]
    public void MergeSongs_StoresTheFirstHeadingOnlyWhenItIsALineOfItsOwn(string pane1)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            Assert.Equal((pane1, ""), Merge(false, pane1, "", "", ""));
        }, 60);
    }
    private static void OnSta(Action check, int seconds)
    {
        Exception failure = null;
        var thread = new Thread(() => failure = Record.Exception(check)) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(seconds)));
        if (failure != null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(failure);
    }
}
