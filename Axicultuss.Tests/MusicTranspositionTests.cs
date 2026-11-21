using System.Linq;
using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

public class MusicTranspositionTests
{
    [Theory]
    [InlineData("(0;C(add9);0;G/B;08;)(2;Am7(b5);3;)", "(0;D(add9);0;A/C#;08;)(2;Bm7(b5);3;)")]
    [InlineData("(1;C(6/9)/G;04;F7(add9(no3));12;)", "(1;D(6/9)/A;04;G7(add9(no3));12;)")]
    [InlineData("(1;C(add9;0;)(2;Am;3;)", "(1;D(add9;0;)(2;Bm;3;)")]
    public void TranspositionPreservesParenthesizedChordFieldsAndFollowingRecords(string data, string expected)
        => Assert.Equal(expected, Gf.TransposeNotations(data, 0, 2, "C"));

    [Theory]
    [InlineData("C;001;G/B;한글;", "D;001;A/Db;한글;")]
    [InlineData("C", "D;-1;")]
    [InlineData("C;;G;", "D;;A;-1;")]
    [InlineData(";;", ";;")]
    [InlineData(";", ";-1;")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void StoredChordPairsPreservePositionTextAndMissingFields(string input, string expected)
        => Assert.Equal(expected, Gf.TransposeOneNotationString(input, 2, 0));

    [Fact]
    public void SongTranspositionKeepsRecordOrderAndStopsAtAnEmptyOrNegativeRecord()
    {
        const string prefix = "junk(001;C;00;)(bad;G)(1;)";
        const string expected = "(1;D;00;)(0;A;-1;)(1;)";
        foreach (string end in new[] { "()(2;F;4;)", "(-1;)(2;F;4;)", "(unclosed" })
            Assert.Equal(expected, Gf.TransposeNotations(prefix + end, 0, 2, "C"));
    }

    [Theory]
    [InlineData("[Abm7/Eb]", 2, 0, "[Bbm7/F]")]
    [InlineData("{F#m7/C#}", -2, 1, "{Em7/B}")]
    [InlineData("C/Bb", 1, -1, "C#/B")]
    [InlineData("A", -1, -1, "Ab")]
    [InlineData("Dbmaj7/G", 0, 1, "Dbmaj7/G")]
    [InlineData("N.C.", 4, 0, "N.C.")]
    [InlineData("c/E#", 1, 1, "c/F#")]
    [InlineData("/G", 2, 0, "/A")]
    [InlineData("G/", 1, 1, "G#/")]
    [InlineData("C/G/old", 2, 0, "D/A")]
    [InlineData("G", -24, 0, "G")]
    [InlineData("F#m", 37, 1, "Gm")]
    public void TransposesRootAndBassWithLegacySpelling(string chord, int steps, int spelling, string expected)
    {
        Assert.Equal(expected, Gf.TransposeChord(chord, steps, spelling));
    }

    [Theory]
    [InlineData("G", 1, "Ab", 0)]
    [InlineData("Gm", 1, "G#m", 1)]
    [InlineData("C", 0, "C", 1)]
    [InlineData("Dm", 0, "Dm", 0)]
    [InlineData("C#", 1, "C#", -1)]
    [InlineData("", -1, "", 0)]
    [InlineData("F#", 24, "F#", 1)]
    public void KeysKeepTheirCanonicalSpellingAndAccidentalPreference(string key, int steps, string expected, int spelling)
    {
        Assert.Equal(spelling, Gf.TransposeKey(ref key, steps));
        Assert.Equal(expected, key);
    }

    [Fact]
    public void CapoWrapsNegativeOctavesAndLargeOffsets()
    {
        int start = -12;
        Assert.Equal(0, Gf.IncrementChord(ref start, 0));
        start = int.MaxValue;
        Assert.Equal(2, Gf.IncrementChord(ref start, int.MaxValue));
    }

    [Fact]
    public void ChordMenusKeepGroupOrderEnharmonicSpellingsAndRegionOnClick()
    {
        using var menu = new ContextMenuStrip();
        ToolStripItem clicked = null;
        Gf.AddChordMenuItems(menu, 2, (sender, _) => clicked = (ToolStripItem)sender);
        Assert.IsType<ToolStripSeparator>(menu.Items[0]);
        var groups = menu.Items.OfType<ToolStripMenuItem>().Take(4).ToArray();
        Assert.Equal(new[] { "Minor", "Minor 7th", "Major 7th", "Bass" }, groups.Select(item => item.Text));
        Assert.Equal(new[] { 17, 17, 17, 12 }, groups.Select(item => item.DropDownItems.Count));
        Assert.Equal(new[] { "Gm", "Abm", "G#m" }, groups[0].DropDownItems.Cast<ToolStripItem>().Take(3).Select(item => item.Text));
        Assert.Equal("G#7", groups[2].DropDownItems[1].Text);
        Assert.Equal("/F#", groups[3].DropDownItems[11].Text);
        Assert.Equal(new[] { "G", "G#", "Ab" }, menu.Items.Cast<ToolStripItem>().Skip(5).Take(3).Select(item => item.Text));
        groups[1].DropDownItems[2].PerformClick();
        Assert.Equal(("G#m7", "2"), (clicked.Text, clicked.Tag));
    }
}
