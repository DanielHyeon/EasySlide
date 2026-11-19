using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OfficeLib;
using Xunit;

namespace Easislides.Tests;

public class PowerPointThumbnailPanelTests
{
    [Fact]
    public void PreviewAndOutputTrackTheirOwnDeckAndRenderTheLastSlide()
    {
        var directory = Directory.CreateTempSubdirectory("easislides-thumbnails-");
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
