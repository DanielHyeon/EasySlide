using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class LiveSessionServiceTests
{
    [Fact]
    public void NewService_DefaultsToOffSnapshot()
    {
        var sut = new LiveSessionService();

        sut.Current.State.Should().Be(LiveState.Off);
        sut.Current.CurrentItemTitle.Should().BeEmpty();
        sut.Current.OutputMonitorName.Should().BeEmpty();
        sut.Current.IsBlackout.Should().BeFalse();
    }

    [Fact]
    public void GoLive_UpdatesSnapshotAndRaisesChanged()
    {
        var sut = new LiveSessionService();
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");

        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.CurrentItemTitle.Should().Be("주일찬양 #3 은혜로다");
        sut.Current.OutputMonitorName.Should().Be("모니터 2");
        sut.Current.IsBlackout.Should().BeFalse();
        changes.Should().ContainSingle().Which.Should().Be(sut.Current);
    }

    [Fact]
    public void HideOutput_MarksSessionHiddenWithoutForgettingCurrentItem()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");

        sut.HideOutput(blackout: false);

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.CurrentItemTitle.Should().Be("주일찬양 #3 은혜로다");
        sut.Current.IsBlackout.Should().BeFalse();
    }

    [Fact]
    public void BlackoutOutput_MarksHiddenAndBlackout()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");

        sut.HideOutput(blackout: true);

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public void GoLive_PropagatesPreviewSourceAndPixelDimensionsFromBitmap()
    {
        var preview = CreateBitmap(1920, 1080);
        var item = new LiveQueueItem("song-3", "주일찬양 #3 은혜로다")
        {
            PreviewSource = preview,
            PreviewFillMode = ImageFillMode.Fill,
            SlideNumber = 4,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemPreviewSource.Should().BeSameAs(preview);
        sut.Current.CurrentItemPreviewFillMode.Should().Be(ImageFillMode.Fill);
        sut.Current.CurrentItemPreviewPixelWidth.Should().Be(1920);
        sut.Current.CurrentItemPreviewPixelHeight.Should().Be(1080);
    }

    [Fact]
    public void GoLive_WithoutPreviewSource_LeavesPreviewFieldsAtDefaults()
    {
        var item = new LiveQueueItem("song-3", "주일찬양 #3 은혜로다");
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemPreviewSource.Should().BeNull();
        sut.Current.CurrentItemPreviewPixelWidth.Should().Be(0);
        sut.Current.CurrentItemPreviewPixelHeight.Should().Be(0);
    }

    private static BitmapSource CreateBitmap(int width, int height)
    {
        var stride = width * 4;
        var pixels = new byte[stride * height];
        var bitmap = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, palette: null, pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }

    [Fact]
    public void Stop_ReturnsToOffAndClearsBlackout()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");
        sut.HideOutput(blackout: true);

        sut.Stop();

        sut.Current.State.Should().Be(LiveState.Off);
        sut.Current.CurrentItemTitle.Should().BeEmpty();
        sut.Current.OutputMonitorName.Should().BeEmpty();
        sut.Current.IsBlackout.Should().BeFalse();
    }
}
