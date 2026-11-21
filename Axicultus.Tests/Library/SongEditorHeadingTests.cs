using System.Windows.Controls;
using Axicultus.Wpf.Library;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class SongEditorHeadingTests
{
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void InsertHeadingPreservesTheSelectedLyricsAndLineEnding(string newline) => StaHelper.RunOnSta(() =>
    {
        string firstLine = "첫 줄" + newline;
        var lyrics = new TextBox { AcceptsReturn = true, Text = firstLine + "둘째 줄" };
        lyrics.Select(firstLine.Length + 1, 2);
        SongEditorWindow.InsertHeadingAtCurrentLine(lyrics, "[chorus]");
        Assert.Equal(firstLine + "[chorus]" + newline + "둘째 줄", lyrics.Text);
        Assert.Equal(firstLine.Length + "[chorus]".Length + newline.Length, lyrics.CaretIndex);
    });
}
