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
    public void Restore_FromHidden_ReturnsToActiveAndClearsBlackoutKeepingContent()
    {
        // 숨김/블랙에서 복귀 — 상태만 Active 로 되돌리고 직전 항목(콘텐츠)은 보존(§7.3-B 화면 제어).
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        sut.HideOutput(blackout: true);
        sut.Current.State.Should().Be(LiveState.Hidden);

        sut.Restore();

        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.IsBlackout.Should().BeFalse();
        sut.Current.CurrentItemTitle.Should().Be("은혜로다", "복귀 시 직전 항목 보존");
    }

    [Fact]
    public void Restore_WhenNotHidden_IsNoOp()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");

        sut.Restore();

        sut.Current.State.Should().Be(LiveState.Active, "Active 상태에선 Restore 가 아무 변화 없음");
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

    [Fact]
    public void GoLive_WithSongLyrics_CarriesDisplayTextWithMarkersStripped()
    {
        // 곡 가사가 스냅샷 본문으로 실리되, 작성 마커([1]/[~코드])는 출력용으로 제거된 표시 텍스트여야 한다.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[~G D]\n[1]\n1절 가사\n둘째 줄\n[2]\n2절 가사",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("1절 가사\n둘째 줄\n\n2절 가사");
        sut.Current.CurrentItemBodyText.Should().NotContain("[");
    }

    [Fact]
    public void GoLive_WithoutLyrics_LeavesBodyTextEmpty()
    {
        // 가사 없는 항목(PPT/미디어/공지 등)은 본문이 비어 출력에 텍스트가 나타나지 않는다.
        var item = new LiveQueueItem("ppt-1", "주보 PPT", LiveItemKinds.PowerPoint);
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().BeEmpty();
    }

    [Fact]
    public void Restore_FromHidden_PreservesBodyText()
    {
        // 숨김→복귀 시 가사 본문도 보존되어야 직전 곡이 그대로 다시 보인다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "1절 가사" };
        var sut = new LiveSessionService();
        sut.GoLive(item, "모니터 2");
        sut.HideOutput(blackout: false);

        sut.Restore();

        sut.Current.CurrentItemBodyText.Should().Be("1절 가사");
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
