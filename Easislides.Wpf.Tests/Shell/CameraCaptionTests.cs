using System;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Shell;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class CameraCaptionTests
{
    private sealed class ShowPosition : IPowerPointSlideShowControl
    {
        public TaskCompletionSource<int?> Result = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<int?> ReadOutputPositionAsync(string path) => Result.Task;
        public Task StartAsync(PowerPointSlideShowRequest request, string monitor, CancellationToken ct) => Task.CompletedTask;
        public Task TriggerNextAsync(PowerPointSlideShowRequest request, CancellationToken ct) => Task.CompletedTask;
        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }

    [Fact]
    public Task NativeSlideAdvancesAndALateReplyCannotOverwriteManualText() => StaHelper.RunOnStaAsync(async () =>
    {
        var show = new ShowPosition();
        var live = new LiveSessionService();
        using var captions = new CaptionSession(live, show);
        var deck = new CaptionDeck(@"C:\songs.pptx", [new([new("a","가사","첫 절")]), new([new("a","가사","후렴")])]);
        captions.Prepare(deck);
        captions.Follow(1,"a");
        live.GoLive(new("p", "찬양", "PPT") { ContentPath = deck.Path, SlideNumber = 1 }, "projector");
        var first = captions.SyncNativeSlideAsync();
        show.Result.SetResult(2);
        await first;
        Assert.Equal("후렴", captions.PublicText);
        show.Result = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var late = captions.SyncNativeSlideAsync();
        captions.Publish("직접 입력");
        show.Result.SetResult(1);
        await late;
        Assert.Equal("직접 입력", captions.PublicText);
    });

    [Fact]
    public void ManualTakeOverridesSlideFollowAndHiddenNavigationDoesNotRevealText()
    {
        var live = new LiveSessionService();
        using var captions = new CaptionSession(live);
        var deck = new CaptionDeck(@"C:\songs.pptx", [
            new([new("body", "본문", "1절")]), new([new("body", "본문", "후렴")]), new([])]);
        captions.Prepare(deck);
        Assert.Equal("", captions.PublicText);
        captions.Follow(1, "body");
        Assert.Equal("1절", captions.PublicText);
        captions.SetVisible(false);
        captions.MoveToSlide(2);
        Assert.Equal("", captions.PublicText);
        captions.SetVisible(true);
        Assert.Equal("후렴", captions.PublicText);
        captions.Publish("안내");
        captions.MoveToSlide(1);
        Assert.Equal("안내", captions.PublicText);
        captions.AudienceEnabled = false;
        Assert.False(captions.HasAudienceText);
        Assert.Equal("안내", captions.PublicText);
        captions.AudienceEnabled = true;
        Assert.True(captions.HasAudienceText);
        captions.Follow(3, "body");
        Assert.Equal("", captions.PublicText);
    }
}
