using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OfficeLib;
using Xunit;

namespace Axicultus.Tests;

public class PowerPointThumbnailPanelTests
{
    [Fact]
    public void PowerPointListSortMovesTheSameSelectedFilesBetweenBothOrders()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var list = new ListView { View = View.Details };
            list.Columns.Add("곡");
            _ = list.Handle;
            string[] names = ["1장_다시", "2장_가을", "찬100장_나무"];
            foreach (var name in names)
                list.Items.Add(name).SubItems.Add(@"PC:\찬송\" + name + ".pptx");
            list.Items[0].Selected = list.Items[2].Selected = true;
            FrmMain.SortPowerPointList(list, Module.PowerPointSortOrder.Title);
            Assert.Equal([names[1], names[2], names[0]], list.Items.Cast<ListViewItem>().Select(item => item.Text));
            Assert.Equal([names[2], names[0]], list.SelectedItems.Cast<ListViewItem>().Select(item => item.Text));
            FrmMain.SortPowerPointList(list, Module.PowerPointSortOrder.HymnNumber);
            Assert.Equal(names, list.Items.Cast<ListViewItem>().Select(item => item.Text));
            Assert.Equal([names[0], names[2]], list.SelectedItems.Cast<ListViewItem>().Select(item => item.Text));
        });
    }

    [Fact]
    public void PowerPointFileThumbnailsFollowHymnNumbersInsteadOfAlphabeticalOrder()
    {
        var folder = Directory.CreateTempSubdirectory("axicultuss-numbered-ppt-");
        try
        {
            string[] names = ["1장.pptx", "2장.pptx", "6장.pptx", "7장.ppt", "10장.pptx", "60장.pptx", "100장.pptx"];
            foreach (var name in names.Reverse()) File.WriteAllText(Path.Combine(folder.FullName, name), "");
            MonitorPresentationTests.RunOnSta(() =>
            {
                var files = Module.PowerPointSourceFiles.ListFiles(folder.FullName, includeSubfolders: true);
                using var panel = new FileThumbnailPanel { Size = new Size(420, 420) };
                using var tip = new ToolTip();
                panel.Display(files, 0, tip, null, null, true);
                var cells = panel.Controls.OfType<ImageCanvas>().OrderBy(cell => (int)cell.Tag);
                Assert.Equal(names, cells.Select(cell => Path.GetFileName(tip.GetToolTip(cell))));
            });
        }
        finally { folder.Delete(recursive: true); }
    }

    [Theory]
    [InlineData(2, 1, 1, 2, true)]
    [InlineData(2, 2, 1, 2, true)]
    [InlineData(2, 2, 2, 2, false)]
    [InlineData(null, 1, 1, 1, false)]
    [InlineData(4, 1, 1, 1, false)]
    public void NativeSlideUpdatesOutputAndRepairsStaleThumbnail(int? native, int outputSlide,
        int selectedThumbnail, int expected, bool redraw)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var live = new Module.SongSettings { CurSlide = outputSlide };
            var output = new Module.SongSettings { CurSlide = outputSlide, TotalSlides = 3 };
            using var liveList = live.LyricsAndNotationsList;
            using var outputList = output.LyricsAndNotationsList;

            Assert.Equal(redraw, FrmMain.ApplyNativeSlidePosition(live, output, native, selectedThumbnail));
            Assert.Equal(expected, live.CurSlide);
            Assert.Equal(expected, output.CurSlide);
        });
    }

    [Theory]
    [InlineData(320, 180, 180)]
    [InlineData(320, 240, 240)]
    public void UpperThumbnailsKeepAllFourSlideEdgesVisible(int sourceWidth, int sourceHeight, int width)
    {
        var directory = Directory.CreateTempSubdirectory("axicultus-frame-");
        try
        {
            MonitorPresentationTests.RunOnSta(() =>
            {
                string prefix = Path.Combine(directory.FullName, "slide-");
                using var source = new Bitmap(sourceWidth, sourceHeight);
                using (var g = Graphics.FromImage(source))
                {
                    g.Clear(Color.White);
                    g.FillRectangle(Brushes.Magenta, 0, 0, sourceWidth, 4);
                    g.FillRectangle(Brushes.Cyan, 0, sourceHeight - 4, sourceWidth, 4);
                    g.FillRectangle(Brushes.Lime, 0, 0, 4, sourceHeight);
                    g.FillRectangle(Brushes.Blue, sourceWidth - 4, 0, 4, sourceHeight);
                }
                for (int slide = 1; slide <= 2; slide++)
                    source.Save(SlideImageManifest.ImagePath(prefix, slide), ImageFormat.Png);

                using var tip = new ToolTip();
                foreach (bool output in new[] { false, true })
                {
                    using var panel = new PowerPointThumbnailPanel { Width = width * 3 + 35 };
                    panel.SetSlideCount(2, tip, i => new ImageCanvas
                        { PowerPoint = true, ThinSelectedBorder = output, IsNextSlide = output && i == 1 });
                    foreach (int selected in new[] { 1, 2 })
                    {
                        panel.Display(prefix, selected, tip);
                        for (int index = 0; index < 2; index++)
                        {
                            var canvas = panel.SlideAt(index);
                            Assert.True(canvas.CalculateImage());
                            var raster = (Bitmap)canvas.Image;
                            int x = raster.Width / 2;
                            int y = raster.Height / 2;
                            bool HasColour(Color colour, int length, Func<int, Color> pixel) =>
                                Enumerable.Range(0, length).Any(n => pixel(n).ToArgb() == colour.ToArgb());
                            Assert.True(HasColour(Color.Magenta, y, n => raster.GetPixel(x, n)), "top edge");
                            Assert.True(HasColour(Color.Cyan, raster.Height - y, n => raster.GetPixel(x, y + n)), "bottom edge");
                            Assert.True(HasColour(Color.Lime, x, n => raster.GetPixel(n, y)), "left edge");
                            Assert.True(HasColour(Color.Blue, raster.Width - x, n => raster.GetPixel(x + n, y)), "right edge");
                        }
                        panel.Width += 120;
                    }
                }
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void PreviewAndOutputTrackTheirOwnDeckAndRenderTheLastSlide()
    {
        var directory = Directory.CreateTempSubdirectory("axicultus-thumbnails-");
        try
        {
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var preview = new PowerPointThumbnailPanel { AutoScroll = true, Size = new Size(335, 220) };
                preview.CreateControl();
                using var output = new PowerPointThumbnailPanel { Width = 335 };
                using var tip = new ToolTip();
                string previewPrefix = Path.Combine(directory.FullName, "preview-");
                string outputPrefix = Path.Combine(directory.FullName, "output-");
                void Display(PowerPointThumbnailPanel panel, string prefix, int count)
                {
                    panel.SetSlideCount(count, tip, _ => new ImageCanvas { PowerPoint = true });
                    panel.Display(prefix, -1, tip);
                    Assert.Equal(count, panel.Controls.Count);
                    if (count == 0)
                        return;
                    var last = panel.SlideAt(count - 1);
                    Assert.True(last.IsDrawnSelected);
                    Assert.Equal(count, last.SlideNumber);
                    Assert.Equal(count.ToString(), tip.GetToolTip(last));
                    Assert.Equal(SlideImageManifest.ImagePath(prefix, count), last.FileName);
                }

                Display(preview, previewPrefix, 1024);
                Display(output, outputPrefix, 1024);
                var first = preview.SlideAt(0);
                Display(preview, previewPrefix, 1025);
                Display(output, outputPrefix, 1025);
                var lastPreview = preview.SlideAt(1024);
                var lastOutput = output.SlideAt(1024);
                Assert.True(preview.IsHandleCreated);
                Assert.True(preview.VerticalScroll.Value > 0);
                Assert.True(preview.ClientRectangle.IntersectsWith(lastPreview.Bounds));
                using (var jpg = new Bitmap(160, 90))
                    jpg.Save(lastPreview.FileName, ImageFormat.Jpeg);
                using (var jpg = new Bitmap(160, 120))
                    jpg.Save(lastOutput.FileName, ImageFormat.Jpeg);
                Assert.True(lastPreview.CalculateImage());
                Assert.Equal(lastPreview.Width * 9 / 16, lastPreview.Height);
                Assert.True(lastOutput.CalculateImage());
                Assert.Equal(lastOutput.Width * 3 / 4, lastOutput.Height);
                preview.Display(previewPrefix, 1, tip);
                Assert.True(first.IsDrawnSelected);
                Assert.True(preview.ClientRectangle.IntersectsWith(first.Bounds));
                Assert.False(lastPreview.IsDrawnSelected);
                Assert.True(lastOutput.IsDrawnSelected);

                Display(preview, previewPrefix, 3);
                Assert.True(lastPreview.IsDisposed);
                Assert.Empty(tip.GetToolTip(lastPreview));
                Assert.Same(first, preview.SlideAt(0));
                Assert.False(lastOutput.IsDisposed);
                Display(output, outputPrefix, 3);
                Assert.True(lastOutput.IsDisposed);
                preview.Display(outputPrefix, 0, tip);
                Assert.Equal(SlideImageManifest.ImagePath(outputPrefix, 3), preview.SlideAt(2).FileName);
                Assert.True(first.IsDrawnSelected);
                preview.Display(outputPrefix, 1025, tip);
                Assert.True(preview.SlideAt(2).IsDrawnSelected);
                Display(preview, previewPrefix, 0);
                Assert.True(first.IsDisposed);
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
