using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Easislides.Module;
using Xunit;

namespace Easislides.Tests;

public class LyricsSlidePanelTests
{
    [Fact]
    public void BothPanesReachSlide30001AndRecycleOnlyVisibleRichEdits()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var preview = Pane("flowLayoutPreviewLyrics");
            using var output = Pane("flowLayoutOutputLyrics");
            string[] slides = Enumerable.Range(1, 30001).Select(number => $"가사 {number}").ToArray();
            preview.SetSlides(slides[..30000], true);
            preview.Highlight(30000, Color.White, Color.Blue, navigate: true, alignTop: true);
            Assert.Equal("가사 30000", VisibleSlide(preview, 30000).Text);
            preview.SetSlides(slides, true);
            output.SetSlides(slides, true);
            preview.Highlight(30001, Color.White, Color.Blue, navigate: true, alignTop: true);
            output.Highlight(2, Color.White, Color.Red, navigate: true);
            var last = VisibleSlide(preview, 30001);
            Assert.Equal(("Preview", "가사 30001", Color.Blue), (last.Name, last.Text, last.BackColor));
            Assert.Equal(("Output", Color.Red), (VisibleSlide(output, 2).Name, VisibleSlide(output, 2).BackColor));
            output.Highlight(30001, Color.White, Color.Red, navigate: true);
            Assert.Equal("가사 30001", VisibleSlide(output, 30001).Text);
            Assert.InRange(preview.Controls.OfType<RichTextBox>().Count(), 1, 12);
            Assert.InRange(output.Controls.OfType<RichTextBox>().Count(), 1, 12);

            preview.SetSlides(["첫 장", "둘째", "끝"], false);
            Assert.Equal(3, preview.Controls.OfType<RichTextBox>().Count());
            Assert.Equal("끝", VisibleSlide(preview, 3).Text);
            Assert.Equal("가사 30001", VisibleSlide(output, 30001).Text);
            preview.SetSlides([""], false, blank: true);
            var empty = Assert.Single(preview.Controls.OfType<RichTextBox>());
            Assert.Equal("", empty.Text);
            preview.SetSlides([], false);
            Assert.Empty(preview.Controls.OfType<RichTextBox>());
        });
    }

    [Fact]
    public void WrappedSlidesReflowOnResizeAndManualScrollingRefreshesTheRows()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var panel = Pane("flowLayoutPreviewLyrics");
            string longSlide = string.Concat(Enumerable.Repeat("찬양 ABC 한 줄 ", 35));
            panel.SetSlides([longSlide, "둘째", longSlide, "끝"], true);
            int wideHeight = VisibleSlide(panel, 1).Height;
            panel.Width /= 2;
            int narrowHeight = VisibleSlide(panel, 1).Height;
            Assert.True(narrowHeight > wideHeight);
            var scroll = panel.Controls.OfType<VScrollBar>().Single();
            scroll.Value = scroll.Maximum - scroll.LargeChange + 1;
            Assert.Equal("끝", VisibleSlide(panel, 4).Text);
            scroll.Value = 0;
            Assert.Equal(longSlide, VisibleSlide(panel, 1).Text);
        });
    }

    [Fact]
    public void KeyboardNavigationShowsTheRequestedSlideAfterUnseenRowsWrap()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var panel = Pane("flowLayoutOutputLyrics");
            var slides = Enumerable.Repeat(string.Concat(Enumerable.Repeat("길게 줄바꿈하는 가사 ", 90)), 60).ToArray();
            panel.SetSlides(slides, true);
            panel.Highlight(40, Color.White, Color.Red, navigate: true);
            var current = VisibleSlide(panel, 40);
            Assert.Equal(Color.Red, current.BackColor);
            int clicked = 0;
            panel.SlideMouseUp += (sender, _) => clicked = (int)((Control)sender).Tag;
            typeof(Control).GetMethod("OnMouseUp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(current, [new MouseEventArgs(MouseButtons.Left, 1, 2, 2, 0)]);
            Assert.Equal(40, clicked);
        });
    }

    [Fact]
    public void SlideTextRetainsBothLanguageRegionsAndSelectionOffsets()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var song = new SongSettings { Type = "B", TotalSlides = 1, CurSlide = 5 };
            song.LyricsAndNotationsList.Items.Add(new ListViewItem(new[] { "", "", "첫 영역", "" }));
            song.LyricsAndNotationsList.Items.Add(new ListViewItem(new[] { "", "", "두 번째 영역", "" }));
            song.Slide[1, 0] = -1;
            song.Slide[1, 3] = song.Slide[1, 4] = 1;
            var text = Assert.Single(Gf.BuildLyricsSlideTexts(song, SystemFonts.DefaultFont, false));
            Assert.Equal("첫 영역\n두 번째 영역", text);
            Assert.Equal(1, song.CurSlide);
            Assert.Equal(text.Length + 1, song.Slide[1, 6]);
            Assert.Equal(0, song.Slide[1, 7]);
        });
    }

    private static LyricsSlidePanel Pane(string name)
    {
        var panel = new LyricsSlidePanel { Name = name, Size = new Size(320, 200) };
        panel.CreateControl();
        panel.PrepareFont(12);
        return panel;
    }

    private static RichTextBox VisibleSlide(LyricsSlidePanel panel, int number)
    {
        var box = Assert.Single(panel.Controls.OfType<RichTextBox>(), b => Equals(b.Tag, number));
        Assert.True(panel.ClientRectangle.IntersectsWith(box.Bounds));
        return box;
    }
}
