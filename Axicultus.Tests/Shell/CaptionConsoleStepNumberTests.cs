using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Axicultus.Wpf.Captions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a049: a step's card shows its number at its top right, as the main window's preview thumbnails show a slide's
// (yellow, black bold figures): slide-row for a score row, the slide for a slide in one, the position for a page. No
// label line above the picture or the text; a screen reader still reads the step in words.
public sealed partial class CaptionConsoleTests
{
    private static readonly Color PreviewNumberYellow = Color.FromArgb(0xFF, 0xF6, 0xE6, 0x00);

    [Fact]
    public Task AScoreRowStepIsNumberedSlideDashRowInTheCardsTopRightCorner() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        await Idle(console);
        Assert.Equal(["1-1", "1-2", "2-1", "2-2"], editor.Steps.Select(step => step.Label));
        Assert.Equal("1장 2줄", editor.Steps[1].Spoken); // read in words

        var (card, number, texts) = Card(console, 1);
        Assert.Equal("1-2", number.Text);
        Assert.DoesNotContain(texts, text => text.Text.Contains('장')); // no label line
        var peer = (ListBoxAutomationPeer)UIElementAutomationPeer.CreatePeerForElement(console.StepStrip);
        Assert.Equal("1장 2줄", peer.GetChildren()![1].GetName());
        // The picture has the card's height the label line took (less the card's margin of 3 at the top and the bottom).
        var picture = Visuals(card).OfType<Image>().Single();
        var room = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(picture);
        Assert.True(room.Height >= card.ActualHeight - 6.5, $"the picture's room {room} in the card {card.ActualHeight} high");
    });

    [Fact]
    public Task ASlideInOneStepIsNumberedWithTheSlideAndItsTextIsNotUnderTheNumber() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("여호와는 나의 목자시니 내게 부족함이 없으리로다"), CaptionAllTextTests.TextSlide("이장"),
            CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        await Idle(console);
        Assert.Equal(["1", "2", "3"], editor.Steps.Select(step => step.Label));
        Assert.Equal("3장 · 삼장", editor.Steps[2].Spoken);

        var (card, number, texts) = Card(console, 0);
        Assert.Equal("1", number.Text);
        var text = Assert.Single(texts, block => block.Text == editor.Steps[0].Text);
        Assert.DoesNotContain(texts, block => block.Text == "1장"); // no label line
        var badge = Bounds(number.Parent as FrameworkElement ?? number, card);
        var words = Bounds(text, card);
        Assert.True(words.Top < 8, $"the text starts at the card's top ({words})");
        Assert.True(words.Right <= badge.Left + 0.5, $"the text {words} is beside the number {badge}, not under it");
    });

    [Fact]
    public Task APageStepIsNumberedWithItsPosition() => FakeCaptionHost.WithConsole(async (console, session, host, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        console.ToggleOnAir();
        host.Show(Verses, ThreeVerses, 1);
        await Pump(console, 200);
        Assert.Equal("2/3", editor.CurrentStep?.Label);
        Assert.Equal("2/3 · 둘째 절", editor.CurrentStep?.Spoken);
        Assert.Equal("2/3", Card(console, 1).Number.Text);
    });

    // The card of the strip's step at that index: its number (checked to look as the preview thumbnails' and to sit in the
    // card's top right corner) and the other text of the card.
    private static (Border Card, TextBlock Number, TextBlock[] Texts) Card(CaptionConsole console, int index)
    {
        console.StepStrip.UpdateLayout();
        var item = (ListBoxItem)console.StepStrip.ItemContainerGenerator.ContainerFromIndex(index);
        var card = Visuals(item).OfType<Border>().First(border => border.Width == 140);
        var blocks = Visuals(card).OfType<TextBlock>().ToList();
        var number = Assert.Single(blocks, block => YellowUnder(block, card));
        Assert.Equal((FontWeights.Bold, Colors.Black), (number.FontWeight, ((SolidColorBrush)number.Foreground).Color));
        var badge = Bounds(number.Parent as FrameworkElement ?? number, card);
        Assert.True(badge.Width >= 18, $"the number's least width ({badge.Width})");
        Assert.True(System.Math.Abs(badge.Right - card.ActualWidth) < 0.5 && System.Math.Abs(badge.Top) < 0.5,
            $"the number {badge} in the corner of the card {card.ActualWidth}×{card.ActualHeight}");
        return (card, number, [.. blocks.Where(block => block != number)]);
    }

    // The block is drawn on the preview's yellow (its own background, or the element holding it).
    private static bool YellowUnder(TextBlock block, FrameworkElement card)
    {
        for (DependencyObject? at = block; at != null && at != card; at = VisualTreeHelper.GetParent(at))
        {
            var brush = at switch { TextBlock t => t.Background, Border b => b.Background, Panel p => p.Background, _ => null };
            if (brush is SolidColorBrush solid && solid.Color == PreviewNumberYellow) return true;
        }
        return false;
    }

    private static Rect Bounds(FrameworkElement element, Visual card)
        => element.TransformToAncestor(card).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
}
