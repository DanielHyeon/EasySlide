using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Xunit;
using Axicultus.Module;

namespace Axicultus.Tests;

public class ImageGalleryGestureTests
{
    [Fact]
    public void LiveBackgroundMatchesSourceAndQueueRowAndKeepsCurrentSlide()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var live = new SongSettings { Source = ItemSource.WorshipList, CurItemNo = 2, Type = "I", ItemID = "info.rtf", CurSlide = 3, UseDefaultFormat = true };
            using var list = live.LyricsAndNotationsList;
            bool Apply(ItemSource source, int row, string key) =>
                FrmMain.UpdateGalleryBackground(live, source, row, key, "tiles.jpg", 0, "61=tiles.jpg>62=0>");

            Assert.False(Apply(ItemSource.PraiseBook, 2, "Iinfo.rtf"));
            Assert.False(Apply(ItemSource.WorshipList, 1, "Iinfo.rtf"));
            Assert.False(Apply(ItemSource.WorshipList, 2, "Iother.rtf"));
            Assert.True(live.UseDefaultFormat);
            Assert.True(Apply(ItemSource.WorshipList, 2, "Iinfo.rtf"));
            Assert.False(live.UseDefaultFormat);
            Assert.Equal(3, live.CurSlide);
            Assert.Equal("tiles.jpg", live.Format.BackgroundPicture);
            Assert.Equal("tiles.jpg", live.Format.TempImageFileName);
            Assert.Equal(ImageMode.Tile, live.Format.BackgroundMode);
        });
    }

    [Fact]
    public void SelectionAndActivationAreSeparateEventsOnTheThumbnail()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var panel = new FileThumbnailPanel { Size = new Size(420, 240) };
            using var tip = new ToolTip();
            int applied = 0;
            panel.Display(["first.jpg", "second.jpg"], 0, tip,
                null, (_, _) => applied++, false);
            var cells = panel.Controls.OfType<ImageCanvas>().OrderBy(c => (int)c.Tag).ToArray();
            void Raise(ImageCanvas cell, string name, MouseButtons button, int count) =>
                typeof(Control).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(cell, [new MouseEventArgs(button, count, 4, 4, 0)]);

            Raise(cells[0], "OnMouseUp", MouseButtons.Left, 1);
            Assert.True(cells[0].IsDrawnSelected);
            Raise(cells[1], "OnMouseUp", MouseButtons.Left, 1);
            Assert.False(cells[0].IsDrawnSelected);
            Assert.True(cells[1].IsDrawnSelected);
            Assert.Equal(0, applied);
            Raise(cells[1], "OnMouseDoubleClick", MouseButtons.Right, 2);
            Assert.Equal(0, applied);
            Raise(cells[1], "OnMouseDoubleClick", MouseButtons.Left, 2);
            Raise(cells[1], "OnMouseUp", MouseButtons.Left, 1);
            Assert.Equal(1, applied);
        });
    }
}
