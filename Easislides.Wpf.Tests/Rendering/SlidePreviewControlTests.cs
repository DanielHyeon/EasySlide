using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Rendering;

public class SlidePreviewControlTests
{
    [Fact]
    public void Source_BindsPreviewCanvasAndHidesFallback()
    {
        StaHelper.RunOnSta(() =>
        {
            var source = CreateBitmap(320, 180);
            var sut = new SlidePreviewControl
            {
                Title = "Deck 1",
                Kind = "PowerPoint",
                Source = source,
                FillMode = ImageFillMode.Fill,
                SlideNumber = 4,
                ShowSlideNumber = true,
                IsSelected = true
            };

            Arrange(sut, width: 320, height: 180);

            var canvas = sut.FindName("PreviewCanvasElement").Should().BeOfType<PreviewCanvas>().Subject;
            canvas.Source.Should().BeSameAs(source);
            canvas.FillMode.Should().Be(ImageFillMode.Fill);
            canvas.SlideNumber.Should().Be(4);
            canvas.ShowSlideNumber.Should().BeTrue();
            canvas.IsSelected.Should().BeTrue();
            canvas.Visibility.Should().Be(Visibility.Visible);
            ((FrameworkElement)sut.FindName("FallbackPanel")).Visibility.Should().Be(Visibility.Collapsed);
        });
    }

    [Fact]
    public void Source_Null_ShowsReadableTitleFallback()
    {
        StaHelper.RunOnSta(() =>
        {
            var sut = new SlidePreviewControl
            {
                Title = "Amazing Grace",
                Kind = "Song",
                Source = null
            };

            Arrange(sut, width: 320, height: 180);

            ((FrameworkElement)sut.FindName("PreviewCanvasElement")).Visibility.Should().Be(Visibility.Collapsed);
            ((FrameworkElement)sut.FindName("FallbackPanel")).Visibility.Should().Be(Visibility.Visible);
            ((System.Windows.Controls.TextBlock)sut.FindName("FallbackTitleText")).Text.Should().Be("Amazing Grace");
            ((System.Windows.Controls.TextBlock)sut.FindName("FallbackKindText")).Text.Should().Be("Song");
        });
    }

    [Fact]
    public void MainWindowPreviewTab_UsesSlidePreviewControlBindings()
    {
        var xaml = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml"),
            Encoding.UTF8);

        xaml.Should().Contain("rendering:SlidePreviewControl");
        xaml.Should().Contain("SelectedItem.PreviewSource");
        xaml.Should().Contain("SelectedItem.PreviewFillMode");
        xaml.Should().Contain("SelectedItem.SlideNumber");
    }

    private static void Arrange(UIElement element, int width, int height)
    {
        var size = new Size(width, height);
        element.Measure(size);
        element.Arrange(new Rect(size));
        element.UpdateLayout();
    }

    private static BitmapSource CreateBitmap(int width, int height)
    {
        var stride = width * 4;
        var pixels = new byte[stride * height];
        for (var i = 0; i < pixels.Length; i += 4)
        {
            pixels[i + 1] = 180;
            pixels[i + 3] = 255;
        }

        var bitmap = BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Bgra32,
            palette: null,
            pixels,
            stride);
        bitmap.Freeze();
        return bitmap;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Easislides.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to find repository root.");
    }
}
