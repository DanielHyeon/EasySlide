using System.Windows;
using System.Windows.Media;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Rendering;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// A caption (a Bible passage, a text, lyrics) breaks its lines between words, not inside a Korean word: on the camera
// screen and the console's previews as on the output screen's lyrics, and on the OBS page.
public sealed class CaptionWordWrappingTests
{
    [Fact]
    public void TheCameraCaptionBreaksBetweenWords() => StaHelper.RunOnSta(() =>
    {
        const string caption = "태초에 하나님이 천지를 창조하시니라 땅이 혼돈하고 공허하며";
        var view = new CameraCaptionView
        {
            DataContext = new CaptionLook("", null, caption, 3, 1, Brushes.Transparent, Brushes.White),
        };
        view.Fit(new Size(480, 270)); // narrow: the caption wraps
        view.Measure(new Size(480, 270));
        view.Arrange(new Rect(0, 0, 480, 270));
        view.UpdateLayout();
        string shown = view.LowerThirdText.Text;
        Assert.Equal(LyricWordWrapping.ForDisplay(caption), shown); // no break between the syllables of a word
        Assert.Contains("태⁠초⁠에 ", shown);
        Assert.Equal(caption, shown.Replace("⁠", "")); // nothing else changes
    });

    [Fact]
    public void TheObsPageBreaksBetweenWords()
    {
        Assert.Contains("word-break:keep-all", CaptionBroadcastServer.Page);
        Assert.DoesNotContain("overflow-wrap:anywhere", CaptionBroadcastServer.Page); // (it broke inside a word)
    }
}
