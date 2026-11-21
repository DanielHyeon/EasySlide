using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Controls;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public class FrmMainPresentationTests
{
    [Theory]
    [InlineData(960, 540, 640, 360)]
    [InlineData(960, 720, 640, 480)]
    [InlineData(540, 960, 270, 480)]
    public void PowerPointPreviewFitsTheRequestedBoxAtTheDeckAspect(double slideWidth, double slideHeight, int width, int height)
        => Assert.Equal((width, height), Axicultus.Wpf.Interop.OfficePptSession.FitSlideSize(slideWidth, slideHeight, 640, 480));

    [Theory]
    [InlineData("song:1", "D", "music_note_2")]
    [InlineData("ppt:1", "PPT", "powerpoint_badge")]
    [InlineData("esw:W:1", "Notice", "word_badge")]
    [InlineData("esw:T:1", "Notice", "text_t")]
    [InlineData("bible:1", "B", "bible")]
    public void HeaderAndQueueIconsRecognizeImportedItems(string id, string kind, string icon)
    {
        var converter = new Axicultus.Wpf.Converters.KindToFrmIconConverter();
        var item = new LiveQueueItem(id, "항목", kind);
        Assert.Equal(icon, converter.Convert(item, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("song:1", "Song", "우리교회말씀", "bible")]
    [InlineData("esw:T:1", "Notice", "★★★사도신경★★★.txt", "bible")]
    [InlineData("text-file:1", "Notice", "주 기 도 문.TXT", "bible")]
    [InlineData("T1", "Item", "■봉헌찬양", "clipboard_task_list_ltr")]
    [InlineData("text-file:2", "Notice", "찬양대.txt", "clipboard_task_list_ltr")]
    [InlineData("text:1", "Notice", "광고", "clipboard_task_list_ltr")]
    [InlineData("song:2", "Song", "주기도문송", "music_note_2")]
    [InlineData("powerpoint:1", "PowerPoint", "사도신경", "powerpoint_badge")]
    [InlineData("esw:I:1", "Notice", "광고", "info")]
    public void WorshipListDistinguishesPrayerAndServiceText(string id, string kind, string title, string icon)
    {
        var converter = new Axicultus.Wpf.Converters.KindToFrmIconConverter { ForWorshipList = true };
        Assert.Equal(icon, converter.Convert(new LiveQueueItem(id, title, kind), typeof(string), null,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ResizingToolbarKeepsSearchAndButtonsOnOneRowAndRestoresCaptions()
    {
        StaHelper.RunOnSta(() =>
        {
            var bar = new FrmMainToolBar();
            var caption = new TextBlock { Text = "Options", Width = 72 };
            FrmMainToolBar.SetIsCaption(caption, true);
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(new FrmIcon { Icon = "settings" });
            content.Children.Add(caption);
            var button = new Button { Content = content, Padding = new Thickness(0) };
            var search = new Grid();
            FrmMainToolBar.SetIsPhraseBox(search, true);
            var jump = new Button { Width = 65 };
            bar.Children.Add(button);
            bar.Children.Add(search);
            bar.Children.Add(jump);

            CheckLayout(700, Visibility.Visible);
            CheckLayout(200, Visibility.Collapsed);
            CheckLayout(700, Visibility.Visible);

            void CheckLayout(double width, Visibility expected)
            {
                bar.Measure(new Size(width, 30));
                bar.Arrange(new Rect(0, 0, width, 30));
                Assert.Equal(expected, caption.Visibility);
                Assert.InRange(search.ActualWidth, 90, 360);
                var last = jump.TranslatePoint(new Point(), bar);
                Assert.Equal(0, last.Y, 1);
                Assert.True(last.X + jump.ActualWidth <= width);
            }
        });
    }

    [Theory]
    [InlineData("document_add", false)]
    [InlineData("live", false)]
    [InlineData("live", true)]
    [InlineData("closed_caption_off", true)]
    [InlineData("desktop", true)]
    [InlineData("arrow_enter", false)]
    [InlineData("chevron_double_down", false)]
    [InlineData("powerpoint_badge", false)]
    [InlineData("word_badge", false)]
    [InlineData("bible", false)]
    [InlineData("clipboard_task_list_ltr", false)]
    public void SharedFrmMainIconPaintsInsideItsSlot(string name, bool filled)
    {
        StaHelper.RunOnSta(() =>
        {
            var icon = new FrmIcon { Icon = name, Filled = filled, Width = 16, Height = 16, Foreground = Brushes.Black };
            var canvas = new Canvas { Width = 24, Height = 24 };
            Canvas.SetLeft(icon, 4);
            Canvas.SetTop(icon, 4);
            canvas.Children.Add(icon);
            canvas.Measure(new Size(24, 24));
            canvas.Arrange(new Rect(0, 0, 24, 24));
            var bitmap = new RenderTargetBitmap(24, 24, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            var pixels = new int[24 * 24];
            bitmap.CopyPixels(pixels, 24 * sizeof(int), 0);
            var painted = Enumerable.Range(0, pixels.Length).Where(i => pixels[i] != 0).ToArray();
            Assert.NotEmpty(painted);
            Assert.All(painted, i =>
            {
                Assert.InRange(i % 24, 4, 19);
                Assert.InRange(i / 24, 4, 19);
            });
            if (name.EndsWith("_badge", StringComparison.Ordinal))
                Assert.Contains(unchecked((int)0xffffffff), pixels);
        });
    }
}
