using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

// The PowerPoint thumbnail looks like the WPF app's: the slide fills the frame, and the number sits over the
// slide picture's top-right corner as 12 px bold text on a #F6E600 badge at least 18 px wide.
[Collection("Image rendering globals")]
public class PowerPointThumbnailNumberTests
{
    private static readonly Color Slide = Color.SteelBlue;
    private static readonly Color Frame = Color.Black;
    private static readonly Color Band = Color.Magenta;

    [Theory]
    [InlineData(1920, 1080, 240, 3)]
    [InlineData(1920, 1080, 240, 12)]
    [InlineData(1920, 1080, 240, 123)]
    [InlineData(1920, 1080, 240, 1025)]
    [InlineData(1024, 768, 200, 3)]
    [InlineData(1024, 768, 200, 12)]
    [InlineData(1024, 768, 200, 1025)]
    [InlineData(1920, 1080, 160, 1025)]
    // Tiny canvases: the badge would be wider than half the picture, so it shrinks.
    [InlineData(1920, 1080, 40, 1025)]
    [InlineData(1920, 1080, 24, 12)]
    [InlineData(1024, 768, 16, 3)]
    // A tall portrait slide at a small width (a 160 x 598 raster).
    [InlineData(1000, 4000, 80, 9)]
    [InlineData(1000, 4000, 80, 12)]
    [InlineData(1000, 4000, 80, 1025)]
    public void NumberOverlapsTheTopRightCornerOfTheSlidePicture(int sourceWidth, int sourceHeight, int canvasWidth, int slide)
    {
        Render(sourceWidth, sourceHeight, canvasWidth, slide, selected: false, (_, raster) =>
        {
            Rectangle picture = SlidePicture(raster);
            Rectangle badge = Bounds(raster, IsBadge);
            Assert.False(badge.IsEmpty, "no slide number drawn");

            // Inside the picture, its top and right edges on the picture's, and at most half the picture each way.
            Assert.True(picture.Contains(badge), $"badge {badge} outside picture {picture}");
            Assert.InRange(badge.Top - picture.Top, 0, 1);
            Assert.InRange(picture.Right - badge.Right, 0, 1);
            Assert.True(badge.Width <= picture.Width / 2 + 1, $"badge {badge} wider than half of {picture}");
            Assert.True(badge.Height <= picture.Height / 2 + 1, $"badge {badge} taller than half of {picture}");

            // No strip reserved beside the slide: the picture is centred in the frame.
            Assert.InRange(Math.Abs(picture.Left - (raster.Width - picture.Right)), 0, 1);
            Assert.InRange(Math.Abs(picture.Top - (raster.Height - picture.Bottom)), 0, 1);

            // The number is drawn in full: text pixels inside the badge, none on its left or right edge, one run
            // of columns per digit.
            Rectangle glyphs = Bounds(raster, IsDark, badge);
            Assert.False(glyphs.IsEmpty, "no number text drawn");
            Assert.True(glyphs.Left > badge.Left && glyphs.Right < badge.Right, $"text {glyphs} clipped by badge {badge}");
            Assert.Equal(slide.ToString().Length, GlyphRuns(raster, badge));
        });
    }

    // On screen the badge has the WPF size whatever the thumbnail size: a 16 px line of 12 px bold text,
    // at least 18 px wide, in #F6E600.
    [Theory]
    [InlineData(1920, 1080, 240, 12)]
    [InlineData(1920, 1080, 160, 1025)]
    [InlineData(1024, 768, 331, 3)]
    public void BadgeHasTheWpfSizeAndColourOnScreen(int sourceWidth, int sourceHeight, int canvasWidth, int slide)
    {
        Render(sourceWidth, sourceHeight, canvasWidth, slide, selected: false, (canvas, raster) =>
        {
            double toDip = (double)canvas.Width / raster.Width * 96.0 / canvas.DeviceDpi;
            Rectangle badge = Bounds(raster, IsBadge);
            Assert.InRange(badge.Height * toDip, 15, 17);
            Assert.True(badge.Width * toDip >= 17.5, $"badge {badge.Width * toDip:0.0} px wide on screen");
            Rectangle glyphs = Bounds(raster, IsDark, badge);
            Assert.InRange(glyphs.Height * toDip, 7, 11); // digit height of 12 px text
            Assert.Equal(Color.FromArgb(0xF6, 0xE6, 0x00).ToArgb(), raster.GetPixel(badge.Left, badge.Bottom - 1).ToArgb());
        });
    }

    // A ready thumbnail resized without a rebuild redraws its number at the screen size of the new width,
    // instead of stretching the old one.
    [Fact]
    public void BadgeKeepsItsScreenSizeWhenTheCanvasIsResizedWithoutARebuild()
    {
        string path = Path.Combine(Path.GetTempPath(), $"axicultus-thumbnail-number-{Guid.NewGuid():N}.png");
        try
        {
            using (var source = new Bitmap(1920, 1080))
            {
                using (var graphics = Graphics.FromImage(source))
                    graphics.Clear(Slide);
                source.Save(path, ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var canvas = new PaintableCanvas { IsPowerPoint = true };
                using var tip = new ToolTip();
                canvas.BuildImageThumbnail(11, 240, 240, path, 1, Frame, tip, false);
                Assert.True(canvas.CalculateImage());
                Assert.InRange(BadgeHeightOnScreen(canvas), 15, 17);

                canvas.BuildImageThumbnail(11, 320, 240, path, 1, Frame, tip, false);
                Assert.Equal(320, canvas.Width);
                canvas.PaintNow();
                Assert.InRange(BadgeHeightOnScreen(canvas), 15, 17);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    // A ready framed thumbnail resized without a rebuild keeps showing its cached picture, even when the slide
    // picture file is gone (deleted or being re-exported): the raster is stretched to the new size (a pixel of
    // band drift is accepted) and only the number is redrawn, at its on-screen size.
    [Fact]
    public void FramedThumbnailResizedWithoutARebuildKeepsItsPictureWhenTheFileIsGone()
    {
        string path = Path.Combine(Path.GetTempPath(), $"axicultus-thumbnail-number-{Guid.NewGuid():N}.png");
        try
        {
            using (var source = new Bitmap(1920, 1080))
            {
                using (var graphics = Graphics.FromImage(source))
                    graphics.Clear(Slide);
                source.Save(path, ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var canvas = new PaintableCanvas { IsPowerPoint = true };
                using var tip = new ToolTip();
                canvas.BuildImageThumbnail(0, 240, 240, path, 2, Frame, tip, false);
                Assert.True(canvas.CalculateImage());
                File.Delete(path);

                canvas.BuildImageThumbnail(0, 140, 240, path, 2, Frame, tip, false);
                canvas.PaintNow();
                Assert.True(canvas.IsImageReady);
                Assert.Equal(480, canvas.Image.Width); // the cached raster, stretched
                Assert.Equal(new Size(140, 84), canvas.Size);
                Assert.Equal(new Rectangle(0, 0, 140, 84),
                    new Rectangle(canvas.PosLeft, canvas.PosTop, canvas.PosWidth, canvas.PosHeight));
                var raster = (Bitmap)canvas.Image;
                Assert.True(IsSlide(raster.GetPixel(raster.Width / 2, raster.Height / 2)), "picture not shown");
                Assert.InRange(BadgeHeightOnScreen(canvas), 15, 17);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    // A ready framed thumbnail resized without a rebuild is made again from its slide picture while the file is
    // there: a raster for the new width, shown over the whole client area, with the number at its screen size.
    [Fact]
    public void FramedThumbnailResizedWithoutARebuildIsMadeAgainFromItsPicture()
    {
        string path = Path.Combine(Path.GetTempPath(), $"axicultus-thumbnail-number-{Guid.NewGuid():N}.png");
        try
        {
            using (var source = new Bitmap(1920, 1080))
            {
                using (var graphics = Graphics.FromImage(source))
                    graphics.Clear(Slide);
                source.Save(path, ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var canvas = new PaintableCanvas { IsPowerPoint = true };
                using var tip = new ToolTip();
                canvas.BuildImageThumbnail(0, 240, 240, path, 2, Frame, tip, false);
                Assert.True(canvas.CalculateImage());
                Assert.Equal(new Size(480, 288), canvas.Image.Size);

                canvas.BuildImageThumbnail(0, 140, 240, path, 2, Frame, tip, false);
                canvas.PaintNow();
                Assert.True(canvas.IsImageReady);
                Assert.Equal(new Size(280, 168), canvas.Image.Size);
                Assert.Equal(new Rectangle(0, 0, 140, 84),
                    new Rectangle(canvas.PosLeft, canvas.PosTop, canvas.PosWidth, canvas.PosHeight));
                Assert.InRange(BadgeHeightOnScreen(canvas), 15, 17);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    // The framed raster is twice the canvas width (at most 1024 px), so wide cells stay sharp and neighbouring
    // widths do not jump between raster sizes; the number keeps its screen size.
    [Theory]
    [InlineData(157, 314)]
    [InlineData(158, 316)]
    [InlineData(400, 800)]
    [InlineData(600, 1024)]
    public void FramedRasterIsTwiceTheCanvasWidth(int canvasWidth, int rasterWidth)
    {
        Render(1920, 1080, canvasWidth, 12, selected: false, (canvas, raster) =>
        {
            Assert.Equal(rasterWidth, raster.Width);
            Assert.InRange(BadgeHeightOnScreen(canvas), 15, 17);
        });
    }

    // A PowerPoint thumbnail in a fixed cell is not framed: its raster has the slide's aspect and is letterboxed
    // in the cell. The slide still sits inside the border band (the selection band never covers its edges), and
    // the number has the WPF size at the width the raster is shown at.
    [Fact]
    public void FixedCellThumbnailIsNotFramedAndSizesTheNumberForItsShownWidth()
    {
        string path = Path.Combine(Path.GetTempPath(), $"axicultus-thumbnail-number-{Guid.NewGuid():N}.png");
        try
        {
            using (var source = new Bitmap(1920, 1080))
            {
                using (var graphics = Graphics.FromImage(source))
                    graphics.Clear(Slide);
                source.Save(path, ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var canvas = new ImageCanvas
                    { IsPowerPoint = true, KeepThumbnailCellHeight = true, SelectedBorderColor = Band };
                using var tip = new ToolTip();
                canvas.BuildImageThumbnail(0, 240, 240, path, 1, Frame, tip, false);
                Assert.True(canvas.CalculateImage());
                Assert.Equal(new Size(240, 240), canvas.Size);
                Assert.Equal(new Rectangle(0, 52, 240, 135),
                    new Rectangle(canvas.PosLeft, canvas.PosTop, canvas.PosWidth, canvas.PosHeight));

                var raster = (Bitmap)canvas.Image;
                Assert.InRange(Math.Abs(raster.Width / (double)raster.Height - 16.0 / 9), 0, 0.01);
                int band = (int)Math.Ceiling(raster.Width / 25.0);
                Rectangle picture = SlidePicture(raster);
                Assert.True(Rectangle.Inflate(new Rectangle(Point.Empty, raster.Size), -band, -band).Contains(picture),
                    $"slide {picture} under the {band} px band");
                Rectangle badge = Bounds(raster, IsBadge);
                Assert.True(picture.Contains(badge), $"badge {badge} outside the slide {picture}");
                double toDip = (double)canvas.PosWidth / raster.Width * 96.0 / canvas.DeviceDpi;
                Assert.InRange(badge.Height * toDip, 15, 17);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    // The badge height on screen in device-independent pixels.
    internal static double BadgeHeightOnScreen(ImageCanvas canvas)
    {
        var raster = (Bitmap)canvas.Image;
        Rectangle badge = Bounds(raster, IsBadge);
        Assert.False(badge.IsEmpty, "no slide number drawn");
        return badge.Height * (double)canvas.Width / raster.Width * 96.0 / canvas.DeviceDpi;
    }

    private sealed class PaintableCanvas : ImageCanvas
    {
        public void PaintNow()
        {
            using var target = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(target);
            using var args = new PaintEventArgs(graphics, ClientRectangle);
            OnPaint(args);
        }
    }

    [Theory]
    [InlineData(4000, 100, 240)]
    [InlineData(1920, 1080, 4)]
    public void ExtremeSizesStillDrawTheNumberInsideThePicture(int sourceWidth, int sourceHeight, int canvasWidth)
    {
        Render(sourceWidth, sourceHeight, canvasWidth, 12, selected: false, (_, raster) =>
        {
            Rectangle badge = Bounds(raster, IsBadge);
            Assert.False(badge.IsEmpty, "no slide number drawn");
            Assert.True(SlidePicture(raster).Contains(badge), "badge outside the slide picture");
        });
    }

    // A picture too small for a two-pixel badge (a 1:160 portrait slide gives a 6 px wide raster) skips the number.
    [Fact]
    public void PictureTooSmallForTheBadgeSkipsTheNumber()
    {
        Render(100, 16000, 240, 12, selected: false, (_, raster) =>
        {
            Assert.Equal(6, raster.Width);
            Assert.True(Bounds(raster, IsBadge).IsEmpty);
            Assert.False(SlidePicture(raster).IsEmpty);
        });
    }

    // Like WPF, the thumbnail keeps its width and its height follows the slide: the picture meets the border
    // band on every side (at most a pixel apart) and the canvas shows the raster over its whole client area.
    [Theory]
    [InlineData(1920, 1080, 240, 144)]
    [InlineData(1920, 1080, 157, 94)]
    [InlineData(1024, 768, 200, 154)]
    [InlineData(1024, 768, 331, 255)]
    public void SlideFillsTheFrameOnEverySideAndKeepsTheWidth(int sourceWidth, int sourceHeight, int canvasWidth, int height)
    {
        Render(sourceWidth, sourceHeight, canvasWidth, 3, selected: true, (canvas, raster) =>
        {
            Assert.Equal(new Size(canvasWidth, height), canvas.Size);
            Assert.Equal(new Rectangle(Point.Empty, canvas.Size),
                new Rectangle(canvas.PosLeft, canvas.PosTop, canvas.PosWidth, canvas.PosHeight));

            int x = raster.Width / 4; // left of the badge
            int y = raster.Height / 2;
            Color Row(int n) => raster.GetPixel(n, y);
            Color Column(int n) => raster.GetPixel(x, n);
            AssertNoStrip(GapToBand(Row, raster.Width / 2, -1, raster.Width));
            AssertNoStrip(GapToBand(Row, raster.Width / 2, +1, raster.Width));
            AssertNoStrip(GapToBand(Column, y, -1, raster.Height));
            AssertNoStrip(GapToBand(Column, y, +1, raster.Height));
        });
    }

    // From inside the slide outwards: the pixels between the slide picture and the border band.
    private static List<Color> GapToBand(Func<int, Color> pixel, int start, int step, int length)
    {
        int position = start;
        while (position >= 0 && position < length && IsSlide(pixel(position)))
            position += step;
        var gap = new List<Color>();
        for (; position >= 0 && position < length && !IsBand(pixel(position)); position += step)
            gap.Add(pixel(position));
        Assert.True(position >= 0 && position < length, "no border band");
        return gap;
    }

    // At most one background-dark pixel (the band's anti-aliased inner edge) between the slide and the band.
    private static void AssertNoStrip(List<Color> gap) =>
        Assert.True(gap.Count(IsDark) <= 1, "strip between slide and band: " + string.Join(", ", gap));

    private static void Render(int sourceWidth, int sourceHeight, int canvasWidth, int slide, bool selected,
        Action<ImageCanvas, Bitmap> check)
    {
        string path = Path.Combine(Path.GetTempPath(), $"axicultus-thumbnail-number-{Guid.NewGuid():N}.png");
        try
        {
            using (var source = new Bitmap(sourceWidth, sourceHeight))
            {
                using (var graphics = Graphics.FromImage(source))
                    graphics.Clear(Slide);
                source.Save(path, ImageFormat.Png);
            }
            MonitorPresentationTests.RunOnSta(() =>
            {
                using var canvas = new ImageCanvas { IsPowerPoint = true, SelectedBorderColor = Band };
                using var tip = new ToolTip();
                // Unless selected, another slide is selected and the frame keeps the background colour.
                canvas.BuildImageThumbnail(slide - 1, canvasWidth, canvasWidth, path, selected ? slide : slide + 1,
                    Frame, tip, false);
                Assert.True(canvas.CalculateImage());
                Assert.Equal(slide, canvas.PowerPointSlideNumbering);
                check(canvas, (Bitmap)canvas.Image);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static bool IsBadge(Color c) => c.R > 200 && c.G > 200 && c.B < 90;

    private static bool IsDark(Color c) => c.R < 90 && c.G < 90 && c.B < 90;

    private static bool IsSlide(Color c) =>
        Math.Abs(c.R - Slide.R) < 40 && Math.Abs(c.G - Slide.G) < 40 && Math.Abs(c.B - Slide.B) < 40;

    private static bool IsBand(Color c) =>
        Math.Abs(c.R - Band.R) < 40 && Math.Abs(c.G - Band.G) < 40 && Math.Abs(c.B - Band.B) < 40;

    // The slide picture: the bounding box of slide-coloured pixels. A badge over its corner leaves the
    // rows below and the columns left of it, so the box is still the whole picture.
    private static Rectangle SlidePicture(Bitmap raster) => Bounds(raster, IsSlide);

    private static Rectangle Bounds(Bitmap raster, Func<Color, bool> match, Rectangle? area = null)
    {
        Rectangle scan = area ?? new Rectangle(Point.Empty, raster.Size);
        int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
        for (int y = scan.Top; y < scan.Bottom; y++)
            for (int x = scan.Left; x < scan.Right; x++)
            {
                if (!match(raster.GetPixel(x, y)))
                    continue;
                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
    }

    // Runs of badge columns holding text pixels: one per digit.
    private static int GlyphRuns(Bitmap raster, Rectangle badge)
    {
        int runs = 0;
        bool inRun = false;
        for (int x = badge.Left; x < badge.Right; x++)
        {
            bool dark = false;
            for (int y = badge.Top; y < badge.Bottom && !dark; y++)
                dark = IsDark(raster.GetPixel(x, y));
            if (dark && !inRun)
                runs++;
            inRun = dark;
        }
        return runs;
    }
}
