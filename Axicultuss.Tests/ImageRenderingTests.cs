using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Axicultus.Module;
using Xunit;

namespace Axicultus.Tests;

[CollectionDefinition("Image rendering globals", DisableParallelization = true)]
public sealed class ImageRenderingCollection { }

[Collection("Image rendering globals")]
public class ImageRenderingTests
{
    [Theory]
    [InlineData(3, 2)]
    [InlineData(1, 1)]
    [InlineData(21, 18)]
    public void FileBackgroundReusesOnlyMatchingRastersAndTilesMatchReference(int tileWidth, int tileHeight)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var path = Path.GetTempFileName();
            var screen = new ImageTransitionControl { Size = new Size(13, 9) };
            var originalSize = new Size(Gf.Buffer_LS_Width, Gf.Buffer_LS_Height);
            try
            {
                Gf.Buffer_LS_Width = 13;
                Gf.Buffer_LS_Height = 9;
                using var tile = new Bitmap(tileWidth, tileHeight);
                for (int y = 0; y < tile.Height; y++)
                    for (int x = 0; x < tile.Width; x++)
                        tile.SetPixel(x, y, Color.FromArgb((x + y) % 3 == 0 ? 79 : 255, 127, x * 11, y * 9));
                tile.Save(path, ImageFormat.Png);
                Gf.SetImageFormat(ref screen, path, ImageMode.Tile, false);
                var first = (Bitmap)screen.NewBackgroundPicture;
                using var expected = new Bitmap(first.Width, first.Height);
                using (var g = Graphics.FromImage(expected))
                using (var decoded = Image.FromFile(path))
                using (var detached = new Bitmap(decoded))
                {
                    for (int y = 0; y < first.Height; y += tile.Height)
                        for (int x = 0; x < first.Width; x += tile.Width)
                            g.DrawImage(detached, new Rectangle(x, y, tile.Width, tile.Height),
                                new Rectangle(Point.Empty, tile.Size), GraphicsUnit.Pixel);
                }
                for (int y = 0; y < first.Height; y++)
                    for (int x = 0; x < first.Width; x++)
                        Assert.Equal(expected.GetPixel(x, y), first.GetPixel(x, y));

                Gf.SetImageFormat(ref screen, path, ImageMode.Tile, false);
                Assert.Same(first, screen.NewBackgroundPicture);
                Assert.Same(first, screen.CurrentBackgroundPicture);
                File.SetLastWriteTimeUtc(path, File.GetLastWriteTimeUtc(path).AddMinutes(1));
                Gf.SetImageFormat(ref screen, path, ImageMode.Tile, false);
                Assert.NotSame(first, screen.NewBackgroundPicture);
                Assert.Same(first, screen.CurrentBackgroundPicture);
                var refreshed = screen.NewBackgroundPicture;
                Gf.SetImageFormat(ref screen, path, ImageMode.Centre, false);
                Assert.NotSame(refreshed, screen.NewBackgroundPicture);
                var centered = screen.NewBackgroundPicture;
                screen.Width += 7;
                Gf.SetImageFormat(ref screen, path, ImageMode.Centre, false);
                Assert.NotSame(centered, screen.NewBackgroundPicture);
                var resizedViewport = screen.NewBackgroundPicture;
                Gf.Buffer_LS_Width += 5;
                Gf.SetImageFormat(ref screen, path, ImageMode.Centre, false);
                Assert.NotSame(resizedViewport, screen.NewBackgroundPicture);
                Assert.Equal(18, screen.NewBackgroundPicture.Width);
                var last = screen.NewBackgroundPicture;
                File.Delete(path);
                Assert.Throws<FileNotFoundException>(() => Gf.SetImageFormat(ref screen, path, ImageMode.Tile, false));
                Assert.Same(last, screen.NewBackgroundPicture);
            }
            finally
            {
                screen.Dispose();
                Gf.Buffer_LS_Width = originalSize.Width;
                Gf.Buffer_LS_Height = originalSize.Height;
                File.Delete(path);
            }
        });
    }

    [Fact]
    public void TransitionListShowsEveryTransitionInTheUiLanguageAndSavesTheEnglishName()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var screen = new ImageTransitionControl();
            using var combo = new ToolStripComboBox();
            var list = combo;
            screen.BuildTransitionsList(ref list, ImageTransitionControl.TransitionTypes.Fade);
            Assert.Equal(58, combo.Items.Count);
            Assert.DoesNotContain(combo.Items.Cast<string>(), text => text.StartsWith('['));
            Assert.Equal(Axicultus.Localization.UiText.Get("ImageTransitionControl", "TransitionFade"), combo.Text);
            Assert.Equal("Fade", screen.GetTransitionText((int)ImageTransitionControl.TransitionTypes.Fade));
        });
    }

    [Fact]
    public void CompositionReusesUnpublishedBuffersAndPreservesBothFramesOnFailure()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var screen = new ImageTransitionControl();
            using var layer = new Bitmap(32, 24);
            using var background = new Bitmap(32, 24);
            screen.NewTextImage = screen.NewPanelImage = layer;
            screen.NewBackgroundPicture = background;
            void Draw(ImageTransitionControl.TransitionAction action, Color color)
            {
                using (var graphics = Graphics.FromImage(background))
                    graphics.Clear(color);
                screen.Go(action, false, false, false, false);
            }
            Draw(ImageTransitionControl.TransitionAction.None, Color.Red);
            var first = screen.NewCombinedImage;
            Draw(ImageTransitionControl.TransitionAction.None, Color.Green);
            Draw(ImageTransitionControl.TransitionAction.None, Color.Transparent);
            Assert.Same(first, screen.NewCombinedImage);
            Assert.Equal(0, ((Bitmap)first).GetPixel(0, 0).A);
            Draw(ImageTransitionControl.TransitionAction.AsFade, Color.Blue);
            var second = screen.NewCombinedImage;
            Assert.Same(first, screen.CurrentCombinedImage);
            screen.NewPanelImage = null;
            Assert.Throws<ArgumentNullException>(() => Draw(ImageTransitionControl.TransitionAction.AsFade, Color.Yellow));
            Assert.Same(first, screen.CurrentCombinedImage);
            Assert.Same(second, screen.NewCombinedImage);
            Assert.Equal(Color.Blue.ToArgb(), ((Bitmap)second).GetPixel(0, 0).ToArgb());
            screen.NewPanelImage = layer;
            Draw(ImageTransitionControl.TransitionAction.AsFade, Color.Yellow);
            Assert.Same(second, screen.CurrentCombinedImage);
            Assert.Equal(Color.Yellow.ToArgb(), ((Bitmap)screen.NewCombinedImage).GetPixel(0, 0).ToArgb());
        });
    }

    [Fact]
    public void AlphaLayersOnReusedCompositeMatchFreshBitmap()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var screen = new ImageTransitionControl();
            using var background = new Bitmap(19, 13, PixelFormat.Format32bppArgb);
            using var text = new Bitmap(19, 13);
            using var panel = new Bitmap(19, 13);
            screen.NewBackgroundPicture = background;
            screen.NewTextImage = text;
            screen.NewPanelImage = panel;
            using (var g = Graphics.FromImage(text))
            using (var brush = new SolidBrush(Color.FromArgb(143, 67, 89, 103)))
                g.FillRectangle(brush, 3, 2, 9, 7);
            using (var g = Graphics.FromImage(panel))
            using (var brush = new SolidBrush(Color.FromArgb(75, 222, 34, 123)))
                g.FillRectangle(brush, 4, 4, 9, 6);
            foreach (var color in new[] { Color.Red, Color.Blue, Color.FromArgb(111, 28, 83, 101), Color.Transparent })
            {
                using (var g = Graphics.FromImage(background)) g.Clear(color);
                screen.Go(ImageTransitionControl.TransitionAction.None, false, false, false, false);
                using var expected = new Bitmap(19, 13, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(expected))
                {
                    g.DrawImageUnscaled(background, 0, 0);
                    g.DrawImageUnscaled(text, 0, 0);
                    g.DrawImageUnscaled(panel, 0, 0);
                }
                var actual = (Bitmap)screen.NewCombinedImage;
                for (int y = 0; y < actual.Height; y++)
                    for (int x = 0; x < actual.Width; x++)
                        Assert.Equal(expected.GetPixel(x, y), actual.GetPixel(x, y));
            }
        });
    }

    [Fact]
    public void MarkersRedrawFromScaledPixelsAfterSourceClosesAndReplacementRefreshesImage()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var path = Path.GetTempFileName();
            try
            {
                using var source = new Bitmap(3840, 2160);
                using (var g = Graphics.FromImage(source)) g.Clear(Color.SteelBlue);
                source.Save(path, ImageFormat.Png);
                using var canvas = new PaintableCanvas { IsPowerPoint = true };
                using var tip = new ToolTip();
                canvas.BuildImageThumbnail(0, 160, 90, path, 1, Color.Black, tip, false);
                Assert.True(canvas.CalculateImage());
                var displayed = (Bitmap)canvas.Image;
                var content = (Image)typeof(ImageCanvas).GetField("_thumbnailContent", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(canvas)!;
                Assert.Equal(displayed.Size, content.Size);
                Assert.InRange(content.Width, 160, 1024);
                // Framed heights: (width - 2 x border band) x slide aspect + 2 x border band.
                Assert.Equal(new Size(160, 96), canvas.Size);
                var before = displayed.GetPixel(0, displayed.Height / 2);
                File.Delete(path);
                canvas.SelectedBorderColor = Color.Lime;
                canvas.PaintMarkers();
                Assert.Same(displayed, canvas.Image);
                Assert.NotEqual(before, displayed.GetPixel(0, displayed.Height / 2));
                Assert.Equal(Color.SteelBlue.ToArgb(), displayed.GetPixel(displayed.Width / 2, displayed.Height / 2).ToArgb());
                using var replacement = new Bitmap(160, 120);
                using (var g = Graphics.FromImage(replacement)) g.Clear(Color.DarkRed);
                replacement.Save(path, ImageFormat.Png);
                Assert.True(canvas.CalculateImage());
                Assert.Equal(new Size(160, 123), canvas.Size);
                Assert.Equal(Color.DarkRed.ToArgb(), ((Bitmap)canvas.Image).GetPixel(100, 100).ToArgb());
            }
            finally
            {
                File.Delete(path);
            }
        });
    }

    [Fact]
    public void TranslucentThumbnailMatchesDirectBicubicDrawing()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            string path = Path.GetTempFileName();
            try
            {
                using var source = new Bitmap(97, 29);
                for (int y = 0; y < source.Height; y++)
                    for (int x = 0; x < source.Width; x++)
                        source.SetPixel(x, y, Color.FromArgb((x + 3 * y) % 256, x, y, 109));
                source.Save(path, ImageFormat.Png);
                using var canvas = new ImageCanvas();
                using var tip = new ToolTip();
                canvas.BuildImageThumbnail(0, 64, 48, path, 0, Color.Black, tip, false);
                Assert.True(canvas.CalculateImage());
                var actual = (Bitmap)canvas.Image;
                using var expected = new Bitmap(actual.Width, actual.Height, PixelFormat.Format24bppRgb);
                using (var decoded = Image.FromFile(path))
                using (var detached = new Bitmap(decoded))
                using (var graphics = Graphics.FromImage(expected))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(detached, new Rectangle(Point.Empty, expected.Size));
                }
                for (int y = 0; y < actual.Height; y++)
                    for (int x = 0; x < actual.Width; x++)
                        Assert.Equal(expected.GetPixel(x, y), actual.GetPixel(x, y));
            }
            finally
            {
                File.Delete(path);
            }
        });
    }

    private sealed class PaintableCanvas : ImageCanvas
    {
        public void PaintMarkers()
        {
            using var target = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(target);
            using var args = new PaintEventArgs(graphics, ClientRectangle);
            OnPaint(args);
        }
    }
}
