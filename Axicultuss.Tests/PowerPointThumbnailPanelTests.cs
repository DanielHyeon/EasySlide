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
    public void SlideClicksRedrawSelectionFromTheLoadedPicturesUntilTheDeckIsReloaded()
    {
        using var session = new Global.SessionTempDirectory();
        string prefix = Path.Combine(session.PathWithSeparator, "live-");
        static void WriteSlide(string path, Color colour)
        {
            using var image = new Bitmap(160, 90);
            using var graphics = Graphics.FromImage(image);
            graphics.Clear(colour);
            image.Save(path, ImageFormat.Png);
        }
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var panel = new PowerPointThumbnailPanel { Width = 515 };
            using var tip = new ToolTip();
            panel.SetSlideCount(2, tip, _ => new ImageCanvas
                { PowerPoint = true, ThinSelectedBorder = true, SelectedBorderColor = Color.Magenta });
            string first = SlideImageManifest.ImagePath(prefix, 1);
            string second = SlideImageManifest.ImagePath(prefix, 2);
            WriteSlide(first, Color.Blue);
            WriteSlide(second, Color.Green);
            panel.Display(prefix, 1, tip);
            foreach (var slide in panel.Controls.Cast<ImageCanvas>())
                Assert.True(slide.CalculateImage());

            File.Delete(first);
            File.Delete(second);
            foreach (int selected in new[] { 2, 1, 2 })
            {
                panel.Display(prefix, selected, tip);
                for (int i = 0; i < 2; i++)
                {
                    var cell = panel.SlideAt(i);
                    using var painted = new Bitmap(cell.Width, cell.Height);
                    cell.DrawToBitmap(painted, cell.ClientRectangle);
                    Assert.True(cell.IsImageReady);
                    Assert.Equal(i + 1 == selected, cell.IsDrawnSelected);
                    Assert.Equal((i == 0 ? Color.Blue : Color.Green).ToArgb(),
                        painted.GetPixel(cell.Width / 2, cell.Height / 2).ToArgb());
                    bool hasSelectionBorder = Enumerable.Range(0, cell.Width / 2)
                        .Select(x => painted.GetPixel(x, cell.Height / 2))
                        .Any(pixel => pixel.R > 180 && pixel.B > 180 && pixel.G < 100);
                    Assert.Equal(i + 1 == selected, hasSelectionBorder);
                }
            }
            WriteSlide(second, Color.Purple);
            File.SetLastWriteTimeUtc(second, DateTime.UtcNow.AddMinutes(1));
            panel.ResetSelection();
            panel.Display(prefix, 2, tip);
            var current = panel.SlideAt(1);
            Assert.True(current.CalculateImage());
            using var updated = new Bitmap(current.Width, current.Height);
            current.DrawToBitmap(updated, current.ClientRectangle);
            Assert.Equal(Color.Purple.ToArgb(), updated.GetPixel(current.Width / 2, current.Height / 2).ToArgb());
        });
    }

    [Fact]
    public void ClosingOneSessionPreservesTheOtherSessionsSlideWithTheSameName()
    {
        using var active = new Global.SessionTempDirectory();
        string activeSlide = SlideImageManifest.ImagePath(Path.Combine(active.PathWithSeparator, "live-"), 1);
        File.WriteAllText(activeSlide, "in use");
        string otherPath;
        using (var other = new Global.SessionTempDirectory())
        {
            otherPath = other.PathWithSeparator;
            Assert.NotEqual(active.PathWithSeparator, otherPath);
            File.WriteAllText(Path.Combine(otherPath, Path.GetFileName(activeSlide)), "other");
        }
        Assert.False(Directory.Exists(otherPath));
        Assert.Equal("in use", File.ReadAllText(activeSlide));
    }

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
                Assert.Equal(60, lastPreview.Height); // framed 16:9 at width 100
                Assert.True(lastOutput.CalculateImage());
                Assert.Equal(77, lastOutput.Height); // framed 4:3 at width 100
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

    // The thumbnails get the deck's framed slide height before their pictures load, so rows and the scroll to
    // the current slide do not move when the pictures paint. Without an exported picture the 4:3 cell is kept.
    [Fact]
    public void ThumbnailsGetTheirSlideHeightBeforeThePicturesLoad()
    {
        var directory = Directory.CreateTempSubdirectory("axicultus-thumbnail-layout-");
        try
        {
            const int count = 12;
            string deck = Path.Combine(directory.FullName, "deck-");
            using (var slide = new Bitmap(160, 90))
            {
                for (int number = 1; number <= count; number++)
                    slide.Save(SlideImageManifest.ImagePath(deck, number), ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var panel = new PowerPointThumbnailPanel { AutoScroll = true, Size = new Size(335, 150) };
                panel.CreateControl();
                using var tip = new ToolTip();
                panel.SetSlideCount(count, tip, _ => new ImageCanvas { PowerPoint = true });
                panel.Display(deck, count, tip);
                Rectangle[] Layout() => Enumerable.Range(0, count).Select(i => panel.SlideAt(i).Bounds).ToArray();

                Rectangle[] before = Layout();
                Assert.All(before, bounds => Assert.Equal(new Size(100, 60), bounds.Size)); // framed 16:9
                int scroll = panel.VerticalScroll.Value;
                Assert.True(scroll > 0);
                for (int i = 0; i < count; i++)
                    Assert.True(panel.SlideAt(i).CalculateImage());
                Assert.Equal(before, Layout());
                Assert.Equal(scroll, panel.VerticalScroll.Value);
                Assert.True(panel.ClientRectangle.IntersectsWith(panel.SlideAt(count - 1).Bounds));

                panel.SetSlideCount(count, tip, _ => new ImageCanvas { PowerPoint = true });
                panel.Display(Path.Combine(directory.FullName, "not-exported-"), 1, tip);
                Assert.All(Layout(), bounds => Assert.Equal(new Size(100, 75), bounds.Size));
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    // A slide picture still being written (a cut-off JPG makes GDI+ throw) does not break the display: the
    // slide size comes from another slide.
    [Fact]
    public void HalfWrittenSlidePictureIsSkipped()
    {
        var directory = Directory.CreateTempSubdirectory("axicultus-thumbnail-layout-");
        try
        {
            const int count = 3;
            string deck = Path.Combine(directory.FullName, "deck-");
            using (var picture = new Bitmap(160, 90))
            {
                picture.Save(SlideImageManifest.ImagePath(deck, 1), ImageFormat.Png);
                using var jpeg = new MemoryStream();
                picture.Save(jpeg, ImageFormat.Jpeg);
                File.WriteAllBytes(SlideImageManifest.ImagePath(deck, count), jpeg.ToArray()[..20]);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var panel = new PowerPointThumbnailPanel { Width = 335 };
                using var tip = new ToolTip();
                panel.SetSlideCount(count, tip, _ => new ImageCanvas { PowerPoint = true });
                panel.Display(deck, count, tip);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(new Size(100, 60), panel.SlideAt(i).Size));
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    // The deck's slide size is read once per deck (again while no picture could be read): kept across widths and
    // reloads of the same deck (its pictures may be gone or re-exported), read for another deck (the 4:3
    // placeholder when it has no picture yet).
    [Fact]
    public void DeckSlideSizeIsReadOncePerDeck()
    {
        var directory = Directory.CreateTempSubdirectory("axicultus-thumbnail-layout-");
        try
        {
            const int count = 4;
            string deck = Path.Combine(directory.FullName, "deck-");
            void Export(int width, int height)
            {
                using var slide = new Bitmap(width, height);
                for (int number = 1; number <= count; number++)
                    slide.Save(SlideImageManifest.ImagePath(deck, number), ImageFormat.Png);
            }
            void Remove()
            {
                for (int number = 1; number <= count; number++)
                    File.Delete(SlideImageManifest.ImagePath(deck, number));
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var panel = new PowerPointThumbnailPanel { Width = 335 };
                using var tip = new ToolTip();
                panel.SetSlideCount(count, tip, _ => new ImageCanvas { PowerPoint = true });
                // No picture exported yet: the 4:3 placeholder, and the size is read again on the next display.
                panel.Display(deck, 1, tip);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(new Size(100, 75), panel.SlideAt(i).Size));
                Export(160, 90);
                panel.Width = 395; // cells 120 wide
                panel.Display(deck, 1, tip);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(new Size(120, 72), panel.SlideAt(i).Size));

                Remove();
                panel.Width = 455; // cells 140 wide
                panel.Display(deck, 1, tip);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(new Size(140, 84), panel.SlideAt(i).Size));

                // Reloaded with another slide size: not read again (the cells size themselves as pictures load).
                Export(160, 120);
                panel.ResetSelection();
                panel.Display(deck, 1, tip);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(new Size(140, 84), panel.SlideAt(i).Size));

                panel.Display(Path.Combine(directory.FullName, "other-"), 1, tip);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(new Size(140, 105), panel.SlideAt(i).Size));
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    // After a width change and a selection change every number has the same on-screen size, about 16 px high.
    [Fact]
    public void EveryBadgeHasTheSameScreenSizeAfterAWidthChange()
    {
        var directory = Directory.CreateTempSubdirectory("axicultus-thumbnail-layout-");
        try
        {
            const int count = 6;
            string deck = Path.Combine(directory.FullName, "deck-");
            using (var slide = new Bitmap(1920, 1080))
            {
                for (int number = 1; number <= count; number++)
                    slide.Save(SlideImageManifest.ImagePath(deck, number), ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var panel = new PowerPointThumbnailPanel { Width = 755 }; // cells 240 wide
                using var tip = new ToolTip();
                panel.SetSlideCount(count, tip, _ => new ImageCanvas { PowerPoint = true });
                panel.Display(deck, 1, tip);
                void PaintAll()
                {
                    for (int i = 0; i < count; i++)
                    {
                        var slide = panel.SlideAt(i);
                        using var painted = new Bitmap(slide.Width, slide.Height);
                        slide.DrawToBitmap(painted, slide.ClientRectangle);
                    }
                }
                PaintAll();
                panel.Width = 995; // cells 320 wide
                panel.Display(deck, 1, tip);
                PaintAll();
                panel.Display(deck, 2, tip);
                PaintAll();
                double[] heights = Enumerable.Range(0, count)
                    .Select(i => PowerPointThumbnailNumberTests.BadgeHeightOnScreen(panel.SlideAt(i))).ToArray();
                Assert.All(heights, height => Assert.InRange(height, 15, 17));
                Assert.InRange(heights.Max() - heights.Min(), 0, 0.5);
                Assert.All(Enumerable.Range(0, count), i => Assert.Equal(320, panel.SlideAt(i).Width));
            });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
