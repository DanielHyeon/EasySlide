using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a042: the output panel keeps its deck after the show ends; screen mode must treat that as no turn of the panel.
public sealed partial class CaptionConsoleTests
{
    [Fact]
    public Task AfterTheShowEndsTheKeptOutputDeckLeavesACaptionTheOperatorMoved() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        var main = (MainViewModel)typeof(CaptionConsole).GetField("_main", Private)!.GetValue(console)!;
        var output = main.OutputPowerPoint;
        void Panel(int slide)
        {
            typeof(PowerPointPreviewViewModel).GetProperty("LoadedContentPath")!.SetValue(output, path);
            output.SlideNumber = slide;
        }
        var deck = new LiveQueueItem("p", "deck", LiveItemKinds.PowerPoint) { ContentPath = path, SlideNumber = 1 };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        Panel(1);
        main.OutputItem = deck; // the output panel holds the deck (kept after the show ends)
        main.Session.GoLive(deck, "m");
        console.ScreenMode.IsChecked = true;
        await Pump(console, 600);
        Assert.Equal((true, 1), (session.Following, session.CurrentSlide));
        main.Session.GoLive(deck with { SlideNumber = 2 }, "m"); // the show turns; the output panel with it
        Panel(2);
        await Pump(console, 300);
        Assert.Equal(2, session.CurrentSlide);
        session.MoveToSlide(3); // the operator moves the caption
        main.Session.Stop(); // Esc: the output panel keeps slide 2
        await Pump(console, 500);
        Assert.Equal((true, 3), (session.Following, session.CurrentSlide)); // as before a042: the show's end is no turn
        Panel(1); // the operator picks slide 1 in the kept thumbnails
        await Pump(console, 300);
        Assert.Equal(1, session.CurrentSlide);
    });
}
