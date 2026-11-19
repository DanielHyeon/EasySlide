using Xunit;

namespace Easislides.Tests;

public class FolderNameTests
{
    [Fact]
    public void FolderNameValidationAndBothCorrectionsUseTheSameNineCharacters()
    {
        const string invalid = "\\/:*?\"<>|";
        string folder = "한글" + invalid + " 폴더";
        Assert.False(Gf.ValidateDirNameFormat(folder));
        Assert.Equal("한글 폴더", Gf.CorrectDirNameFormat(folder));
        Gf.RemoveInvalidDirNameChars(ref folder);
        Assert.Equal("한글_________ 폴더", folder);
        Assert.True(Gf.ValidateDirNameFormat(folder));
        Assert.Same(folder, Gf.CorrectDirNameFormat(folder));
        string original = folder;
        Gf.RemoveInvalidDirNameChars(ref folder);
        Assert.Same(original, folder);
    }
}
