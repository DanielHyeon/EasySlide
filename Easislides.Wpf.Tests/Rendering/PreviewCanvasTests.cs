using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class PreviewCanvasTests
{
    [Fact]
    public void GetCurrentPlacement_Fit_MatchesImageAssetService()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new PreviewCanvas
            {
                Source = CreateBitmap(1920, 1080),
                FillMode = ImageFillMode.Fit
            };
            Arrange(sut, width: 300, height: 300);

            sut.GetCurrentPlacement().Should().Be(new ImagePlacement(0, 65, 300, 169));
        });
    }

    [Fact]
    public void Constructor_UsesHighQualityBitmapScalingForPptThumbnails()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new PreviewCanvas();

            RenderOptions.GetBitmapScalingMode(sut).Should().Be(
                BitmapScalingMode.HighQuality,
                "PPT preview/output thumbnail canvases should downscale high-resolution exports cleanly");
        });
    }

    [Fact]
    public void GetCurrentPlacement_Fill_CoversViewport()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new PreviewCanvas
            {
                Source = CreateBitmap(400, 200),
                FillMode = ImageFillMode.Fill
            };
            Arrange(sut, width: 300, height: 300);

            sut.GetCurrentPlacement().Should().Be(new ImagePlacement(-150, 0, 600, 300));
        });
    }

    [Fact]
    public void GetCurrentPlacement_BitmapSource_UsesPixelDimensionsInsteadOfDeviceIndependentSize()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new PreviewCanvas
            {
                Source = CreateBitmap(100, 100, dpiX: 192, dpiY: 96),
                FillMode = ImageFillMode.Fit
            };
            Arrange(sut, width: 200, height: 100);

            sut.GetCurrentPlacement().Should().Be(new ImagePlacement(50, 0, 100, 100));
        });
    }

    [Fact]
    public void GetCurrentPlacement_WithoutSource_ReturnsEmpty()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new PreviewCanvas();
            Arrange(sut, width: 300, height: 300);

            sut.GetCurrentPlacement().Should().Be(ImagePlacement.Empty);
        });
    }

    [Fact]
    public void Render_WithImageAndSlideNumber_ProducesPixels()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new PreviewCanvas
            {
                Source = CreateBitmap(20, 10),
                FillMode = ImageFillMode.Fit,
                ShowSlideNumber = true,
                SlideNumber = 3,
                IsSelected = true
            };
            Arrange(sut, width: 100, height: 100);

            var bitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(sut);
            var pixels = new byte[100 * 100 * 4];
            bitmap.CopyPixels(pixels, stride: 100 * 4, offset: 0);

            pixels.Should().Contain(value => value != 0);
        });
    }

    private static void Arrange(UIElement element, int width, int height)
    {
        var size = new Size(width, height);
        element.Measure(size);
        element.Arrange(new Rect(size));
        element.UpdateLayout();
    }

    private static BitmapSource CreateBitmap(int width, int height, int dpiX = 96, int dpiY = 96)
    {
        var stride = width * 4;
        var pixels = new byte[stride * height];
        for (var i = 0; i < pixels.Length; i += 4)
        {
            pixels[i + 2] = 255;
            pixels[i + 3] = 255;
        }

        return BitmapSource.Create(
            width,
            height,
            dpiX,
            dpiY,
            PixelFormats.Bgra32,
            palette: null,
            pixels,
            stride);
    }
}
