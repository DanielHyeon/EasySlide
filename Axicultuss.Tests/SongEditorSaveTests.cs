using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.SQLite;
using Xunit;

namespace Axicultus.Tests;

// What FrmEditItem stores for a song. The WPF song editor is tested with the same cases and the same
// expected values (Axicultus.Tests/Library/SongEditorViewModelTests.cs); change both together.
[Collection("WinForms Bible globals")] // The display options and the song database are shared state.
public class SongEditorSaveTests
{
    [Theory]
    [InlineData("Line 1\nLine 2", "", "", "", "Line 1\nLine 2", "")]
    [InlineData("Line 1\r\nLine 2\r\n", "", "", "", "Line 1\nLine 2", "")]
    [InlineData("Amazing grace\nHow sweet", "(0;G;0;C;8;)(1;D;4;)", "", "", "Amazing grace\nHow sweet", "(0;G;0;C;8;)(1;D;4;)")]
    [InlineData("[1]\nverse a\n\nverse b\n[chorus]\nchorus a\n[2]\nverse two", "(1;G;0;)(5;C;2;)", "", "",
        "[1]\nverse a\n\nverse b\n[chorus]\nchorus a\n[2]\nverse two", "(1;G;0;)(5;C;2;)")]
    [InlineData("[chorus]\nchorus a\n[1]\nverse a", "(1;C;0;)(3;G;1;)", "", "", "[chorus]\nchorus a\n[1]\nverse a", "(1;C;0;)(3;G;1;)")]
    [InlineData("[1]\nverse a\n[chorus]\nchorus a", "(1;G;0;)", "[1]\nsecond a\n[chorus]\nsecond c\n[2]\nonly second", "(1;Am;2;)(5;F;0;)",
        "[1]\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[region 2]\nsecond c\n[2]\n[region 2]\nonly second", "(1;G;0;)(3;Am;2;)(10;F;0;)")]
    [InlineData("first a\nfirst b", "(1;G;0;)", "second a", "(0;Am;2;)", "first a\nfirst b\n[region 2]\nsecond a", "(1;G;0;)(3;Am;2;)")]
    [InlineData("one\ntwo", "", "[1]\nuno\ndos", "", "[1]\none\ntwo\n[region 2]\nuno\ndos", "")]
    // Verses that only the second pane has follow in the order of that pane, under their own headings.
    [InlineData("", "", "[chorus]\nbar\n[1]\nfoo", "", "[chorus]\n[region 2]\nbar\n[1]\n[region 2]\nfoo", "")]
    [InlineData("[chorus]\n[1]", "", "[chorus]\nbar\n[1]\nfoo", "(1;Am;0;)", "[chorus]\n[region 2]\nbar\n[1]\n[region 2]\nfoo", "(2;Am;0;)")]
    [InlineData("[1]\na", "", "[1]\nx\n[bridge]\nbb\n[2]\ny", "", "[1]\na\n[region 2]\nx\n[bridge]\n[region 2]\nbb\n[2]\n[region 2]\ny", "")]
    [InlineData("", "", "foo\nbar", "", "[region 2]\nfoo\nbar", "")]
    // A verse with nothing but empty lines in the second pane gets no region marker.
    [InlineData("[1]\na\n[2]\nb", "", "[1]\n\n", "", "[1]\na\n[2]\nb", "")]
    // A heading of the first pane places its verse even when all the lines of that verse are in the second pane.
    [InlineData("[1]\nzh\n[2]\n[chorus]\nzc\n[3]", "(1;G;0;)", "[1]\npy\n[2]\nen\n[chorus]\npc\n[3]\nec", "(3;D;1;)",
        "[1]\nzh\n[region 2]\npy\n[2]\n[region 2]\nen\n[chorus]\nzc\n[region 2]\npc\n[3]\n[region 2]\nec", "(1;G;0;)(6;D;1;)")]
    [InlineData("[1]\na\n\n\n\nb\n\n[2]\n\nc\n\n", "", "", "", "[1]\na\n\nb\n\n[2]\nc", "")]
    [InlineData("[1]\n[2]\nb", "", "", "", "[2]\nb", "")]
    [InlineData("[1]\na\n[bridge]\nb\n[ending]\ne\n[prechorus]\np\n[chorus 2]\nt", "", "[bridge 2]\nw", "",
        "[1]\na\n[bridge]\nb\n[ending]\ne\n[prechorus]\np\n[chorus 2]\nt\n[bridge 2]\n[region 2]\nw", "")]
    [InlineData("  ", "", "", "", "  ", "")]
    [InlineData("", "", "", "", "", "")]
    public void TheTwoLyricPanesAreStoredAsOneText(string first, string firstChords, string second, string secondChords,
        string lyrics, string chords)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            var (storedLyrics, storedChords) = Merge(first, firstChords, second, secondChords, capo: -1);
            Assert.Equal(lyrics, storedLyrics);
            Assert.Equal(chords, storedChords);

            // Opened again and saved untouched, the song is stored the same.
            string text1 = "", chords1 = "", text2 = "", chords2 = "";
            using var rows = Gf.ExtractLyrics(storedLyrics, storedChords, ref text1, ref chords1, ref text2, ref chords2);
            Assert.Equal((lyrics, chords), Merge(text1, chords1, text2, chords2, capo: -1));
        });
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public void SavingKeepsTheChordsWhenTheShowTransposesToCapoZero(int capo)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            int showNotations = Gf.ShowNotations, capoZero = Gf.ShowCapoZero;
            Gf.ShowNotations = Gf.ShowCapoZero = 1;
            try
            {
                var (_, chords) = Merge("Amazing grace", "(0;G;0;C;8;)", "", "", capo);
                Assert.Equal("(0;G;0;C;8;)", chords);
            }
            finally
            {
                Gf.ShowNotations = showNotations;
                Gf.ShowCapoZero = capoZero;
            }
        });
    }

    // The item is the one the editor opened: other chords, a capo, and the show set to "notations" and "capo zero".
    [Theory]
    [InlineData(typeof(FrmEditItem))]
    [InlineData(typeof(FrmInfoScreen))]
    public void SavingFromAnEditorKeepsTheChordsAsEntered(Type editorType)
    {
        OnSta(() =>
        {
            const BindingFlags members = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            Gf.InitializeVerseHeadings();
            using var editor = (Form)Activator.CreateInstance(editorType);
            using var font = new Font("Arial", 12);
            FieldInfo Field(string name) => editorType.GetField(name, members);
            Field("MainFont").SetValue(editor, font);
            Field("NotationFont").SetValue(editor, font);
            Field("sArray").SetValue(editor, Gf.SymbolsString.Split(','));
            ((TextBox)Field("SongTitle").GetValue(editor)).Text = "Title";
            ((RichTextBox)Field("tbLyrics1").GetValue(editor)).Text = "G »\nAmazing grace";
            ref SongSettings item = ref editorType == typeof(FrmEditItem) ? ref Gf.EditItem1 : ref Gf.InfoItem1;
            item.OriginalNotations = "(0;C;0;)";
            item.Capo = 2;
            int showNotations = Gf.ShowNotations, capoZero = Gf.ShowCapoZero;
            Gf.ShowNotations = Gf.ShowCapoZero = 1;
            try
            {
                Assert.True((bool)editorType.GetMethod("ValidateAllDetails", members).Invoke(editor, null));
                Assert.Equal("Amazing grace", Field("CombinedLyrics").GetValue(editor));
                Assert.Equal("(0;G;0;)", Field("CombinedNotations").GetValue(editor));
            }
            finally
            {
                Gf.ShowNotations = showNotations;
                Gf.ShowCapoZero = capoZero;
                Gf.InitialiseIndividualData(ref item);
            }
        });
    }

    // FrmSmartMerge.Start_Merge for one pair of songs: both looked up, then merged.
    [Fact]
    public void ASmartMergeKeepsTheChordsOfBothSongs()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            string path = Path.GetTempFileName();
            var saved = (Gf.ConnectStringMainDB, Gf.DBFileName, Gf.Folders[1].Use, Gf.ShowNotations, Gf.ShowCapoZero);
            try
            {
                Gf.ConnectStringMainDB = DbController.ConnectionStringForFile(path);
                Gf.DBFileName = path;
                Gf.Folders[1].Use = Gf.ShowNotations = Gf.ShowCapoZero = 1;
                using (var connection = DbController.GetDbConnection(Gf.ConnectStringMainDB))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = """
                        CREATE TABLE SONG (SongID INTEGER PRIMARY KEY, FolderNo INTEGER, Lyrics TEXT, msc TEXT, Sequence TEXT, capo INTEGER,
                            "Key" TEXT, Timing TEXT, Writer TEXT, Copyright TEXT, Book_Reference TEXT, User_Reference TEXT,
                            LICENCE_ADMIN1 TEXT, LICENCE_ADMIN2 TEXT);
                        INSERT INTO SONG (SongID, FolderNo, Lyrics, msc, capo) VALUES
                            (1, 1, '[1]' || char(10) || 'Amazing grace', '(1;G;0;C;8;)', 2),
                            (2, 1, '[1]' || char(10) || '놀라운 은혜', '(1;Em;0;)', -1);
                        """;
                    command.ExecuteNonQuery();
                }
                var lookUp = typeof(FrmSmartMerge).GetMethod("LookUpSong", BindingFlags.Static | BindingFlags.NonPublic);
                SongSettings Look(int id)
                {
                    object[] arguments = [id.ToString(), new SongSettings(), "", "", "", "", "", "", "", "", "", "", ""];
                    Assert.True((bool)lookUp.Invoke(null, arguments));
                    return (SongSettings)arguments[1];
                }
                string lyrics = "", chords = "";
                Gf.Merge_Songs(Look(1), Look(2), ref lyrics, ref chords);
                Assert.Equal("[1]\nAmazing grace\n[region 2]\n놀라운 은혜", lyrics);
                Assert.Equal("(1;G;0;C;8;)(3;Em;0;)", chords);
            }
            finally
            {
                (Gf.ConnectStringMainDB, Gf.DBFileName, Gf.Folders[1].Use, Gf.ShowNotations, Gf.ShowCapoZero) = saved;
                File.Delete(path);
            }
        });
    }

    // The same list made for the show (with slides) still holds the chords the show displays.
    [Fact]
    public void TheShowStillTransposesToCapoZero()
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            var song = new SongSettings { SplitScreens = false, CompleteLyrics = "Amazing grace", Notations = "(0;G;0;C;8;)", OriginalNotations = "(0;G;0;C;8;)", Capo = 2 };
            using var lines = song.LyricsAndNotationsList;
            int showNotations = Gf.ShowNotations, capoZero = Gf.ShowCapoZero;
            Gf.ShowNotations = Gf.ShowCapoZero = 1;
            try
            {
                Gf.FormatDisplayLyrics(ref song, PrepareSlides: true, UseStoredSequence: true);
                Assert.Equal("(0;A;0;D;8;)", song.Notations);
                Assert.Equal("A;0;D;8;", lines.Items[0].SubItems[3].Text);
                Assert.Equal("(0;G;0;C;8;)", song.OriginalNotations);
            }
            finally
            {
                Gf.ShowNotations = showNotations;
                Gf.ShowCapoZero = capoZero;
            }
        });
    }

    [Theory]
    [InlineData("[1]\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[region 2]\nsecond c", "(1;G;0;)(3;Am;2;)(7;F;1;)",
        "[1]\nverse a\n[chorus]\nchorus a", "(1;G;0;)", "[1]\nsecond a\n[chorus]\nsecond c", "(1;Am;2;)(3;F;1;)")]
    [InlineData("[1]\na\n[2]\nb\n[region 2]\n[1]\nx\n[2]\ny", "(1;C;0;)(6;D;1;)", "[1]\na\n[2]\nb", "(1;C;0;)", "[1]\nx\n[2]\ny", "(1;D;1;)")]
    [InlineData("first a\nfirst b\n[region 2]\nsecond a", "(1;G;0;)(3;Am;2;)", "first a\nfirst b", "(1;G;0;)", "second a", "(0;Am;2;)")]
    [InlineData("[1]\na\nb\nc\nd\ne\nf", "(1;C;0;)(1;D;4;)(3;G;2;)()(5;F;8;)(unfinished", "[1]\na\nb\nc\nd\ne\nf", "(1;C;0;)(3;G;2;)(5;F;8;)", "", "")]
    [InlineData("Line 1\r\nLine 2\r\n\r\n", "", "Line 1\nLine 2", "", "", "")]
    [InlineData("[chorus]\nc line\n[region 2]\nc second\n\n[1]\nv line", "(3;Em;0;)", "[chorus]\nc line\n[1]\nv line", "", "[chorus]\nc second", "(1;Em;0;)")]
    [InlineData("", "", "", "", "", "")]
    public void AStoredSongIsShownInTwoLyricPanes(string lyrics, string chords, string first, string firstChords,
        string second, string secondChords)
    {
        OnSta(() =>
        {
            Gf.InitializeVerseHeadings();
            string text1 = "", chords1 = "", text2 = "", chords2 = "";
            using var rows = Gf.ExtractLyrics(lyrics, chords, ref text1, ref chords1, ref text2, ref chords2);
            Assert.Equal((first, firstChords, second, secondChords), (text1, chords1, text2, chords2));
        });
    }

    [Theory]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", 1, 0, "(0;Ab/C;0;Bbm(add9);4;)(2;Gm7;1;[Db;3;F#;5;Db/F;7;N.C.;9;)")]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", 1, 1, "(0;G#/C;0;A#m(add9);4;)(2;Gm7;1;[C#;3;F#;5;C#/F;7;N.C.;9;)")]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", -1, -1, "(0;Gb/Bb;0;Abm(add9);4;)(2;Fm7;1;[B;3;Eb#;5;B/Eb;7;N.C.;9;)")]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", 14, -1, "(0;A/C#;0;Bm(add9);4;)(2;G#m7;1;[D;3;F##;5;D/F#;7;N.C.;9;)")]
    [InlineData("(1;Bb;2;)()(3;C;1;)", 2, 1, "(1;C;2;)")]
    [InlineData("(0;G)", 1, 1, "(0;G#;-1;)")]
    [InlineData("junk", 1, 1, "")]
    public void TransposingMovesEveryChordOfTheRecords(string chords, int steps, int spelling, string expected)
        => Assert.Equal(expected, Gf.TransposeNotations(chords, steps, spelling));

    [Theory]
    [InlineData("G", 1, "Ab", 0)]
    [InlineData("Ab", 1, "A", 1)]
    [InlineData("", 1, "", 1)]
    [InlineData("", -1, "", 0)]
    [InlineData("Xyz", 1, "Xyz", -1)]
    [InlineData("Am", -1, "G#m", 1)]
    [InlineData("F#", 1, "G", 1)]
    [InlineData("Gb", 1, "Gb", -1)]
    [InlineData("C#m", 2, "D#m", 1)]
    [InlineData("E", -13, "Eb", 0)]
    [InlineData("Dbm", 1, "Dbm", -1)]
    public void TransposingMovesTheKeyAndChoosesSharpsOrFlats(string key, int steps, string expected, int spelling)
    {
        Assert.Equal(spelling, Gf.TransposeKey(ref key, steps));
        Assert.Equal(expected, key);
    }

    [Fact]
    public void ASongWithoutACapoIsStoredWithMinusOne()
    {
        OnSta(() =>
        {
            using var capo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            Assert.Equal(-1, FrmEditItem.StoredCapo(capo)); // A new song: the list is still empty.
            capo.Items.AddRange(["", "Capo 0", "Capo 1"]);
            capo.SelectedIndex = 0;
            Assert.Equal(-1, FrmEditItem.StoredCapo(capo));
            capo.SelectedIndex = 2;
            Assert.Equal(1, FrmEditItem.StoredCapo(capo));
        });
    }

    [Fact]
    public void ASongFromAFolderThatIsNotInUseKeepsItsFolder()
    {
        OnSta(() =>
        {
            using var folders = new System.Windows.Forms.ComboBox { DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            folders.Items.AddRange(["Morning", "Evening"]);
            folders.Text = "Morning";
            folders.Text = "Archive";
            Assert.Equal("Morning", folders.Text); // The list ignores a name it does not have.

            FrmEditItem.SelectSongFolder(folders, 7, "Archive");
            Assert.Equal("Archive", folders.Text);

            FrmEditItem.SelectSongFolder(folders, 2, "Evening");
            Assert.Equal("Evening", folders.Text);
            Assert.Equal(3, folders.Items.Count);

            // Folder 0 holds the deleted songs and is never offered.
            FrmEditItem.SelectSongFolder(folders, 0, "Deleted");
            Assert.Equal(3, folders.Items.Count);
        });
    }

    private static (string Lyrics, string Chords) Merge(string first, string firstChords, string second, string secondChords, int capo)
    {
        var item1 = new SongSettings { CompleteLyrics = first, Notations = firstChords, OriginalNotations = firstChords, Capo = capo };
        var item2 = new SongSettings { CompleteLyrics = second, Notations = secondChords, OriginalNotations = secondChords };
        string lyrics = "", chords = "";
        Gf.Merge_Songs(item1, item2, ref lyrics, ref chords);
        return (lyrics, chords);
    }

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
