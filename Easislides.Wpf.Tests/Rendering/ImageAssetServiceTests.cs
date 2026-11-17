using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class ImageAssetServiceTests
{
    [Fact]
    public void CalculatePlacement_Fit_PreservesAspectRatioAndCenters()
    {
        var sut = new ImageAssetService();

        var placement = sut.CalculatePlacement(1920, 1080, 300, 300, ImageFillMode.Fit);

        placement.Left.Should().Be(0);
        placement.Top.Should().Be(65);
        placement.Width.Should().Be(300);
        placement.Height.Should().Be(169);
    }

    [Fact]
    public void CalculatePlacement_Fill_CoversViewportAndCropsEvenly()
    {
        var sut = new ImageAssetService();

        var placement = sut.CalculatePlacement(400, 200, 300, 300, ImageFillMode.Fill);

        placement.Left.Should().Be(-150);
        placement.Top.Should().Be(0);
        placement.Width.Should().Be(600);
        placement.Height.Should().Be(300);
    }

    [Fact]
    public async Task LoadAsync_LoadsImageMetadataAndFitPlacement()
    {
        using var fixture = TempImageFile.CreatePng(width: 200, height: 100);
        var sut = new ImageAssetService();

        var result = await sut.LoadAsync(new ImageAssetRequest(fixture.Path, ViewportWidth: 100, ViewportHeight: 100));

        result.Succeeded.Should().BeTrue();
        result.Asset!.PixelWidth.Should().Be(200);
        result.Asset.PixelHeight.Should().Be(100);
        result.Asset.ContentType.Should().Be("image/png");
        result.Placement.Should().Be(new ImagePlacement(0, 25, 100, 50));
    }

    [Fact]
    public async Task LoadAsync_WhenExtensionIsUnsupported_ReturnsUnsupported()
    {
        using var fixture = TempImageFile.CreateText(extension: ".txt");
        var sut = new ImageAssetService();

        var result = await sut.LoadAsync(new ImageAssetRequest(fixture.Path, ViewportWidth: 100, ViewportHeight: 100));

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(ImageAssetErrorKind.UnsupportedFile);
    }

    [Fact]
    public async Task LoadAsync_WhenFileIsLocked_ReturnsFileLocked()
    {
        using var fixture = TempImageFile.CreatePng(width: 64, height: 64);
        await using var lockStream = new FileStream(fixture.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var sut = new ImageAssetService();

        var result = await sut.LoadAsync(new ImageAssetRequest(fixture.Path, ViewportWidth: 100, ViewportHeight: 100));

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(ImageAssetErrorKind.FileLocked);
    }

    [Fact]
    public async Task LoadAsync_WhenImageCannotBeDecoded_ReturnsDecodeFailed()
    {
        using var fixture = TempImageFile.CreateText(extension: ".png");
        var sut = new ImageAssetService();

        var result = await sut.LoadAsync(new ImageAssetRequest(fixture.Path, ViewportWidth: 100, ViewportHeight: 100));

        result.Succeeded.Should().BeFalse();
        result.ErrorKind.Should().Be(ImageAssetErrorKind.DecodeFailed);
    }

    private sealed class TempImageFile : IDisposable
    {
        private TempImageFile(string path) => Path = path;

        public string Path { get; }

        public static TempImageFile CreatePng(int width, int height)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_{Guid.NewGuid():N}.png");
            using var bitmap = new Bitmap(width, height);
            bitmap.SetPixel(0, 0, Color.Blue);
            bitmap.Save(path, ImageFormat.Png);
            return new TempImageFile(path);
        }

        public static TempImageFile CreateText(string extension)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_{Guid.NewGuid():N}{extension}");
            File.WriteAllText(path, "not an image");
            return new TempImageFile(path);
        }

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch
            {
            }
        }
    }
}
