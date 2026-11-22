using System;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

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
        captions.SetVisible(true);
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
    public void TheCaptionAreaKeepsItsSizeAndLongTextShrinksToFit() => StaHelper.RunOnSta(() =>
    {
        var text = new System.Windows.Controls.TextBlock { TextWrapping = System.Windows.TextWrapping.Wrap, LineStackingStrategy = System.Windows.LineStackingStrategy.BlockLineHeight };
        var area = new System.Windows.Controls.Border { Child = text };

        text.Text = "아멘";
        CaptionFit.Apply(text, area, 900, 54);
        Assert.Equal(54, text.FontSize);
        double height = area.Height;

        text.Text = "전능하사 천지를 만드신 하나님 아버지를 내가 믿사오며\n그 외아들 우리 주 예수 그리스도를 믿사오니\n이는 성령으로 잉태하사 동정녀 마리아에게 나시고\n본디오 빌라도에게 고난을 받으사";
        CaptionFit.Apply(text, area, 900, 54);
        Assert.Equal(height, area.Height);
        Assert.True(text.FontSize < 54);
        Assert.True(text.DesiredSize.Height <= area.Height + .5);

        text.Text = "주님은 나의 목자시니\n내게 부족함 없으리";
        CaptionFit.Apply(text, area, 900, 54, lines: 1); // a one-line area: a third of the height, two lines shrink into it
        Assert.Equal(height / 3, area.Height, 3);
        Assert.True(text.DesiredSize.Height <= area.Height + .5);

        text.Text = "전능하사 천지를 만드신\n하나님 아버지를\n내가 믿사오며\n그 외아들";
        CaptionFit.Apply(text, area, 900, 54, lines: 1); // too much for one line even at the smallest size: grows, never cut
        Assert.True(area.Height > height / 3);
        Assert.Equal(text.DesiredSize.Height, area.Height, 3);
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
        Assert.Equal("", captions.PublicText); // following starts hidden
        captions.SetVisible(true);
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
