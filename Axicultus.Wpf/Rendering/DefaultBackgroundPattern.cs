using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Rendering;

internal static class DefaultBackgroundPattern
{
    internal static int SelectedIndex(LiveOutputSettings settings) =>
        !settings.LyricsMonitorBackgroundIsGradient ? 11 : settings.LyricsMonitorBackgroundPattern is >= 0 and < 11
            ? settings.LyricsMonitorBackgroundPattern : settings.LyricsMonitorBackgroundGradientDirection switch
            {
                LyricsGradientDirection.DiagonalDown => 2, LyricsGradientDirection.DiagonalUp => 3,
                LyricsGradientDirection.Horizontal => 9, _ => 7
            };

    [ThreadStatic] private static (int First, int Second, int Index, ImageBrush Brush)? _last;

    internal static ImageBrush CreateBrush(int first, int second, int index)
    {
        if (_last is { } last && (first, second, index) == (last.First, last.Second, last.Index))
            return last.Brush;
        const int width = 384, height = 288;
        using var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            var target = graphics;
            string id = string.Empty;
            new Patterns().Fill(ref target, System.Drawing.Color.FromArgb(first), System.Drawing.Color.FromArgb(second), index, width, height, ref id);
        }
        var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        try
        {
            var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, data.Scan0, data.Stride * height, data.Stride);
            source.Freeze();
            var brush = new ImageBrush(source) { Stretch = Stretch.Fill };
            brush.Freeze();
            _last = (first, second, index, brush);
            return brush;
        }
        finally { bitmap.UnlockBits(data); }
    }
}
