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
    public void GoLive_WithSongLyrics_CarriesFirstVerseByDefault()
    {
        // 절 단위 페이지네이션(PR B): LyricsPageIndex=0(기본)이면 첫 절만 출력에 보인다.
        // 작성 마커([1]/[~코드])는 출력용으로 제거된다.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[~G D]\n[1]\n1절 가사\n둘째 줄\n[2]\n2절 가사",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("1절 가사\n둘째 줄");
        sut.Current.CurrentItemBodyText.Should().NotContain("[");
    }

    [Fact]
    public void GoLive_WithSongLyricsAtPage1_CarriesSecondVerse()
    {
        // LyricsPageIndex=1이면 두 번째 절만 출력에 보인다(MainViewModel이 이동 시 얹어 전달).
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[~G D]\n[1]\n1절 가사\n둘째 줄\n[2]\n2절 가사",
            LyricsPageIndex = 1,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("2절 가사");
    }

    [Fact]
    public void GoLive_CarriesPositionLabelToSnapshot()
    {
        // 위치 인디케이터(§7.3-A): 항목의 PositionLabel("2/4" 등)이 스냅샷으로 전달된다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절\n[2]\n2절", PositionLabel = "1/2" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemPositionLabel.Should().Be("1/2");
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

    // ─── ClearOutput (배경 유지·콘텐츠 비우기 — 레거시 LiveClear) ──────────────

    [Fact]
    public void ClearOutput_FromActive_EntersClearedKeepingContentAndNoBlackout()
    {
        // 비우기: 콘텐츠는 숨기되 배경은 유지(Black=완전검정과 구별). 콘텐츠는 보존(복귀 대비).
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다") { Lyrics = "1절 가사" }, "Display 1");

        sut.ClearOutput();

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.IsCleared.Should().BeTrue();
        sut.Current.IsBlackout.Should().BeFalse("비우기는 검정이 아니라 배경 유지");
        sut.Current.CurrentItemBodyText.Should().Be("1절 가사", "복귀 대비 콘텐츠 보존");
    }

    [Fact]
    public void ClearOutput_WhenOff_IsNoOp()
    {
        var sut = new LiveSessionService();

        sut.ClearOutput();

        sut.Current.State.Should().Be(LiveState.Off);
        sut.Current.IsCleared.Should().BeFalse();
    }

    [Fact]
    public void HideOutput_ResetsClearedFlag()
    {
        // Black/Hide 와 Clear 는 상호 배타 — Clear 후 Black 으로 전환하면 IsCleared 가 꺼진다.
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        sut.ClearOutput();

        sut.HideOutput(blackout: true);

        sut.Current.IsCleared.Should().BeFalse();
        sut.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public void Restore_FromCleared_ReturnsToActiveAndResetsClearedFlag()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다") { Lyrics = "1절 가사" }, "Display 1");
        sut.ClearOutput();

        sut.Restore();

        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.IsCleared.Should().BeFalse();
        sut.Current.CurrentItemBodyText.Should().Be("1절 가사");
    }

    [Fact]
    public void Stop_ClearsClearedFlag()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        sut.ClearOutput();

        sut.Stop();

        sut.Current.IsCleared.Should().BeFalse();
    }

    // ─── Refresh (출력 강제 재렌더 — 레거시 RefreshOutput) ─────────────────────

    [Fact]
    public void Refresh_RaisesSessionChangedWithCurrentSnapshot()
    {
        // 새로고침: 스냅샷이 바뀌지 않아도 SessionChanged 를 다시 발생시켜 출력을 강제 재렌더한다.
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.Refresh();

        changes.Should().ContainSingle("Refresh 는 현재 스냅샷으로 한 번 재통지");
        changes[0].Should().Be(sut.Current);
    }

    [Fact]
    public void Refresh_WhenOff_StillRaisesForConsistency()
    {
        var sut = new LiveSessionService();
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.Refresh();

        changes.Should().ContainSingle();
        changes[0].State.Should().Be(LiveState.Off);
    }
}
