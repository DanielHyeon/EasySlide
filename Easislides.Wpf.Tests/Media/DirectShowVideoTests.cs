using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Easislides.Wpf.Media;
using Xunit;

namespace Easislides.Wpf.Tests.Media;

public class DirectShowVideoTests
{
    [Fact]
    public void LocalFileFallbackPresentsVideoFramesAndEndsOnTheGraphClock() => StaHelper.RunOnSta(() =>
    {
        var element = new MediaElement { LoadedBehavior = MediaState.Manual };
        var image = new Image();
        var backend = new WpfMediaElementPlaybackBackend(element, image);
        var clip = MediaPlaybackSnapshot.Empty with { State = MediaPlaybackState.Playing, IsMuted = true,
            Source = Path.Combine(AppContext.BaseDirectory, "Media", "color-bars.avi") };
        try
        {
            backend.Load(clip);
            Assert.False(backend.TryFileFallback(clip with { Source = "previous.avi" }, TimeSpan.Zero, out _));
            Assert.True(backend.TryFileFallback(clip, TimeSpan.Zero, out string? error), error);
            var graph = Assert.IsType<DirectShowVideo>(backend.FileGraph);
            var elapsed = Stopwatch.StartNew();
            var clock = graph.ReadClock();
            while (!clock.Ended && elapsed.Elapsed < TimeSpan.FromSeconds(6))
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                Thread.Sleep(20);
                clock = graph.ReadClock();
            }
            Assert.True(clock.Ended);
            Assert.InRange(clock.Duration.TotalSeconds, 0.9, 1.1);
            var bitmap = Assert.IsType<WriteableBitmap>(image.Source);
            Assert.Equal(16, bitmap.PixelWidth);
            int stride = bitmap.PixelWidth * 3;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);
            Assert.True(pixels.Max() - pixels.Min() > 80);
        }
        finally { backend.Unload(); }
        Assert.Null(image.Source);
    });
}
