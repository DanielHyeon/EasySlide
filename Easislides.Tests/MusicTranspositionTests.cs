using Xunit;

namespace Easislides.Tests;

public class MusicTranspositionTests
{
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
}
