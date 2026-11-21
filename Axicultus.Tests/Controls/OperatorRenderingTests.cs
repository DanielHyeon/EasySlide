using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Controls;

public class OperatorRenderingTests
{
    [Fact]
    public void ActualLyricsSurface_SizesChordAndFitsLongWrappedLyrics()
    {
        StaHelper.RunOnSta(() =>
        {
            var style = new LyricParagraphStyle("Am  G  F\n우리 함께 부르는 노래 우리 함께 부르는 노래",
                new Typeface("Malgun Gothic"), 24, 1.2, Brushes.White, TextAlignment.Left, FlowDirection.LeftToRight,
                false, false, false, true, false, 240, 400, [0], 100);
            var surface = new LyricParagraphSurface { Paragraph = style };
            var available = new Size(240, 400);
            surface.Measure(available);
            var originalHeight = surface.DesiredSize.Height;
            surface.Paragraph = style with { NotationPercent = 50 };
            surface.Measure(available);
            surface.DesiredSize.Height.Should().BeLessThan(originalHeight);
            surface.Paragraph = style with { FitLargest = true, Height = 130, Outline = true };
            surface.Measure(available);
            surface.DesiredSize.Height.Should().BeInRange(80, 130);
            surface.Arrange(new Rect(surface.DesiredSize));
            var bitmap = new RenderTargetBitmap(240, 130, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(surface);
            byte[] pixels = new byte[240 * 130 * 4];
            bitmap.CopyPixels(pixels, 240 * 4, 0);
            pixels.Should().Contain(v => v != 0);
        });
    }

    [Fact]
    public void MessageAndParentalAlerts_StackAtChosenEdgeAndUseSeparateAppearance()
    {
        StaHelper.RunOnSta(() =>
        {
            var message = new OutputAlertBanner { Appearance = new AlertAppearance { Points = 16, Vertical = 0, Horizontal = 1 },
                Alert = new OutputAlert("회중 메시지", false, false, false) };
            var parent = new OutputAlertBanner { Appearance = new AlertAppearance { Points = 30, Vertical = 0, Outline = true, Shadow = true },
                Alert = new OutputAlert("보호자 호출", false, false, true) };
            var panel = new AlertOverlayPanel();
            panel.Children.Add(message);
            panel.Children.Add(parent);
            panel.Measure(new Size(800, 450));
            panel.Arrange(new Rect(0, 0, 800, 450));
            parent.TranslatePoint(new Point(), panel).Y.Should().BeGreaterThanOrEqualTo(message.ActualHeight);
            parent.Background.Should().Be(Brushes.Transparent);
            parent.Appearance = parent.Appearance with { Vertical = 2 };
            panel.Measure(new Size(800, 450));
            panel.Arrange(new Rect(0, 0, 800, 450));
            parent.UpdateLayout();
            parent.TranslatePoint(new Point(), panel).Y.Should().BeGreaterThan(300);
        });
    }

    [Fact]
    public void StageShowsChordsIndependentlyOfProjector()
    {
        StaHelper.RunOnSta(() =>
        {
            var session = new LiveSessionService();
            session.GoLive(new LiveQueueItem("song:7", "찬양", LiveItemKinds.Song) { Lyrics = "가사»Am", ShowNotations = false }, "audience");
            session.Current.CurrentItemBodyText.Should().Be("가사");
            session.Current.StageRegion1!.WithChords.Should().Contain("Am");
            using var view = new OutputWindowViewModel();
            view.ApplySession(session.Current);
            view.StageParagraph!.Text.Should().Contain("Am");
            view.StageParagraph.NotationLines.Should().Equal(0);
        });
    }

    [Fact]
    public void NotationSelection_DistinguishesChordFromIdenticalLyric()
    {
        LiveSessionService.FindNotationLines("Am\nAm\nF\n가사", "Am\n가사").Should().Equal(0, 2);
    }
}
