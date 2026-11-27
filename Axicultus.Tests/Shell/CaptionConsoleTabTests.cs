using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a045: the caption tab of the main window's output column: it follows the output screen also while the live tab is
// shown, its column fits the output column, and the keys typed in it are its own.
public sealed partial class CaptionConsoleTests
{
    internal static (LiveQueueItem First, LiveQueueItem Second, LiveQueueItem Third) ThreeSongs(MainViewModel main)
    {
        var first = new LiveQueueItem("song:a041-1", "입례 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n주님 앞에 나아가" };
        var second = new LiveQueueItem("song:a041-2", "말씀 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n주의 말씀 내 발의 등" };
        var third = new LiveQueueItem("song:a041-3", "봉헌 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n나의 모든 것 드리니" };
        main.LoadQueue([first, second, third]);
        return (first, second, third);
    }

    internal static MainViewModel Main(CaptionConsole console) => (MainViewModel)typeof(CaptionConsole).GetField("_main", Private)!.GetValue(console)!;

    // The item put in the output panel as the main window does it (its worship list, "송출 칸에 올리기").
    private static async Task SendToOutput(CaptionConsole console, MainViewModel main, LiveQueueItem item)
    {
        main.SelectedItem = item;
        main.SendWorshipSelectionToOutputCommand.Execute(null);
        await Pump(console, 300);
    }

    // The visual tree under an element (a template).
    internal static System.Collections.Generic.IEnumerable<DependencyObject> Visuals(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var below in Visuals(child)) yield return below;
        }
    }

    // The live tab shown (the caption tab hidden, as the main window hides it): the caption still follows the output
    // panel, and goes out.
    [Fact]
    public Task WithTheLiveTabShownTheCaptionKeepsFollowingTheScreen() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var verses = new LiveQueueItem("song:a045-verses", "세 절 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n첫째 절\n[2]\n둘째 절\n[3]\n셋째 절" };
        main.LoadQueue([verses]);
        console.ScreenMode.IsChecked = true;
        await ClickAsync(console.AirButton);
        await SendToOutput(console, main, verses);
        Assert.Equal("첫째 절", session.PublicText);

        console.Visibility = Visibility.Collapsed;
        main.GoToOutputLyricsPageCommand.Execute(1); // the operator turns the output panel from the live tab
        await Pump(console, 300);
        Assert.Equal("둘째 절", session.PublicText);
        Assert.Equal("2/3", ((CaptionConsoleViewModel)console.DataContext).CurrentStep?.Label);
        console.Visibility = Visibility.Visible;
    });

    // The camera preview is resized with the splitter over it (as the main window's live view is): the typed box or the
    // steps take what is left, never less than their least, and the preview keeps a least height. The splitter chosen
    // with Tab keeps its arrows (they do not step the caption).
    [Fact]
    public Task TheCameraPreviewIsResizedWithItsSplitter() => WithConsole(async (console, _, _) =>
    {
        await Pump(console, 300);
        double preview = console.Stage.ActualHeight, upper = console.Upper.ActualHeight;
        Assert.True(console.StageRow.Height.IsAuto && preview > 200, $"as wide as the column at 16:9: {preview}");
        void Arrow(Key key, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Assert.False(Press(console.StageSplitter, key)); // (the tab leaves the splitter its arrows)
                console.StageSplitter.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(console), 0, key)
                    { RoutedEvent = Keyboard.KeyDownEvent });
                console.UpdateLayout(); // (the splitter moves from the rows as they are laid out: a press, then the layout)
            }
        }
        Arrow(Key.Down, 5); // 10 DIP a press
        await Pump(console, 300);
        Assert.InRange(console.Stage.ActualHeight, preview - 51, preview - 49);
        Assert.InRange(console.Upper.ActualHeight, upper + 49, upper + 51); // the typed box takes what the preview gave
        Assert.Equal(0, console.Body.ScrollableHeight);

        Arrow(Key.Down, 100); // not below its least
        await Pump(console, 300);
        Assert.InRange(console.Stage.ActualHeight, console.StageRow.MinHeight - 0.5, console.StageRow.MinHeight + 0.5);

        Arrow(Key.Up, 200); // not into the upper part's least: nothing above is cut and the column does not scroll
        await Pump(console, 300);
        Assert.True(console.Stage.ActualHeight > preview, $"larger than at first: {console.Stage.ActualHeight}");
        Assert.True(console.Upper.ActualHeight >= console.UpperRow.MinHeight - 0.5 && console.Draft.ActualHeight >= console.Draft.MinHeight - 0.5,
            $"the upper part {console.Upper.ActualHeight} of at least {console.UpperRow.MinHeight}");
        Assert.True(console.Body.ScrollableHeight < 1, $"scrolls {console.Body.ScrollableHeight}");
        Assert.Equal(console.StageRow.Height, console.PreviewHeight); // what the main window hands to the next list's tab
    });

    // On the live tab the switch does not offer to put the caption on air (what would go out is seen on the caption tab):
    // it is there only while the caption is on air, to tell so and to hide it. On the caption tab it is always there.
    [Fact]
    public Task OnTheLiveTabTheSwitchOnlyHidesTheCaption() => WithConsole(async (console, session, _) =>
    {
        var strip = console.Strip;
        await Pump(console, 100);
        Assert.True(strip.CaptionTab.IsChecked == true && strip.AirButton.IsVisible && !session.Visible);
        strip.LiveTab.IsChecked = true;
        await Pump(console, 100);
        Assert.False(strip.AirButton.IsVisible); // a hidden caption: nothing to press on the live tab
        double height = strip.ActualHeight;
        console.ToggleOnAir(); // F6 works on the live tab too
        await Pump(console, 100);
        Assert.True(session.Visible && strip.AirButton.IsVisible); // on air: told, and one press hides it
        Assert.Equal(height, strip.ActualHeight); // the strip keeps its height
        await ClickAsync(strip.AirButton);
        await Pump(console, 100);
        Assert.False(session.Visible || strip.AirButton.IsVisible);
        strip.CaptionTab.IsChecked = true;
        await Pump(console, 100);
        Assert.True(strip.AirButton.IsVisible);
    });

    // The caption tab shown, following the screen, the show not on the screen: the row picked in the worship list goes to
    // the output panel, so its caption is seen (hidden: nothing goes out). Behind the live tab, or with the show on the
    // screen, a pick sends nothing.
    [Fact]
    public Task APickedRowIsPreviewedInTheCaptionTabWhileTheShowIsOff() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var (first, second, third) = ThreeSongs(main);
        await Pump(console, 200);
        main.SelectedItem = second;
        await Pump(console, 300);
        Assert.Equal((second.Id, "주의 말씀 내 발의 등", false, ""), (main.OutputItem?.Id, session.ReadyText, session.Visible, session.PublicText));
        Assert.Equal(Axicultus.Wpf.Controls.LiveState.Off, main.Session.Current.State); // the panel only: the output screen stays off

        console.Visibility = Visibility.Collapsed; // the live tab shown: a pick is the preview's alone (nothing is sent)
        main.SelectedItem = third;
        await Pump(console, 300);
        Assert.Equal(second.Id, main.OutputItem?.Id);
        console.Visibility = Visibility.Visible; // the panel holds a row: showing the tab does not replace it
        await Pump(console, 300);
        Assert.Equal(second.Id, main.OutputItem?.Id);

        main.Session.GoLive(second, "m"); // the show on the screen: a pick changes nothing there
        main.SelectedItem = first;
        await Pump(console, 300);
        Assert.Equal(second.Id, main.OutputItem?.Id);
    }, asOpened: true);

    // The next deck is sent while the output panel still holds the last deck's slide; the panel draws the new deck
    // before or after the tab has read it. The caption follows the new deck from the slide its own panel shows.
    [Theory]
    [InlineData(2, true)]
    [InlineData(2, false)]
    [InlineData(5, true)]
    [InlineData(5, false)]
    public Task TheNextDeckIsFollowedFromItsOwnSlideNotTheLastDecks(int slides, bool panelDrawnFirst) => WithConsole(async (console, session, directory) =>
    {
        var main = Main(console);
        var output = main.OutputPowerPoint;
        LiveQueueItem Deck(string name, int count) => new("ppt:" + name, name, LiveItemKinds.PowerPoint)
        {
            ContentPath = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory(name).FullName, [],
                [.. Enumerable.Range(1, count).Select(n => CaptionAllTextTests.TextSlide($"{name} {n}장"))]),
            SlideNumber = 1,
        };
        void Panel(string path, int slide) // as the panel's own load tells it: the slide, then whose it is, then ready
        {
            output.SlideNumber = slide;
            typeof(PowerPointPreviewViewModel).GetProperty("LoadedContentPath")!.SetValue(output, path);
            output.State = PowerPointPreviewState.Ready;
        }
        var (last, next) = (Deck("last", 4), Deck("next", slides));
        Panel(last.ContentPath!, 4); // the last song ended on its fourth slide
        main.OutputItem = last;
        console.ScreenMode.IsChecked = true;
        await Pump(console, 600);
        Assert.Equal((true, 4), (session.Following, session.CurrentSlide));

        main.OutputItem = next; // the send: the item first, then its panel is emptied and drawn
        output.Clear();
        if (!panelDrawnFirst) await Pump(console, 600); // the deck is read before the panel has drawn it
        Panel(next.ContentPath!, 1);
        await Pump(console, 600);

        Assert.Equal((true, 1), (session.Following, session.CurrentSlide));
        Assert.Equal("next 1장", session.ReadyText);
        Assert.Equal(slides, console.StepStrip.Items.Count);

        session.MoveToSlide(2); // the operator moves the caption: it stays while the panel does not turn
        output.PreviewImage = new System.Windows.Media.Imaging.WriteableBitmap(4, 4, 96, 96, PixelFormats.Bgr32, null);
        await Pump(console, 300);
        Assert.Equal(2, session.CurrentSlide);
    });

    // Pumps the tab's dispatcher until the condition holds (true) or the time runs out (false), instead of a fixed wait.
    private static async Task<bool> Until(DispatcherObject owner, Func<bool> condition, int milliseconds = 5000)
    {
        var until = DateTime.UtcNow.AddMilliseconds(milliseconds);
        while (!condition())
        {
            if (DateTime.UtcNow > until) return false;
            await Pump(owner, 50);
        }
        return true;
    }

    [Theory]
    [InlineData(3, 300, 48)] // three lines of 16 DIP a notch
    [InlineData(-1, 300, 300)] // "one screen at a time"
    [InlineData(0, 300, 300)]
    public void AWheelNotchScrollsTheColumnAsAScrollViewerWould(int lines, double viewport, double step) =>
        Assert.Equal(step, CaptionConsole.WheelStep(lines, viewport));

    // The column's width changes with its scroll bar, and the camera row's height with the width: at no height may the
    // least height and the bar keep changing each other (a layout loop that would take the main window's input).
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task AtEveryHeightTheColumnSettles(bool typing) => WithConsole(async (console, _, _) =>
    {
        if (!typing) console.ScreenMode.IsChecked = true;
        var host = Window.GetWindow(console);
        host.Width = 450;
        // (Task.Delay resumes at the dispatcher's normal priority, above the layout work: a loop fails the test, not hangs it)
        for (double height = 300; height <= 900; height += 3)
        {
            host.Height = height;
            await Task.Delay(60);
            int fits = console.Fits;
            await Task.Delay(120);
            Assert.True(console.Fits - fits <= 1, $"at {height}: fitted {console.Fits - fits} times more while nothing changed");
        }
    });

    // The typed box keeps three lines of its font as the font changes (the interface size).
    [Fact]
    public Task TheBoxIsFittedAgainWhenItsFontChanges() => WithConsole(async (console, _, _) =>
    {
        await Pump(console, 200);
        double box = console.Draft.MinHeight;
        Assert.True(box > 40, $"three lines: {box}");
        console.Resources["Type.BodyLarge.FontSize"] = 24d;
        await Pump(console, 200);
        Assert.Equal(24d, console.Draft.FontSize);
        Assert.True(console.Draft.MinHeight > box * 1.3, $"the box {box} -> {console.Draft.MinHeight}");
    });

    // The tab opens following the screen: the follow choice first and picked, its controls and steps shown, the output
    // screen's text mirrored.
    [Fact]
    public Task TheConsoleOpensFollowingTheScreen() => WithConsole(async (console, session, _) =>
    {
        await Pump(console, 200);
        Assert.Equal((true, false), (console.ScreenMode.IsChecked, console.DirectMode.IsChecked));
        Assert.True(console.ScreenPanel.IsVisible && console.StepStrip.IsVisible && console.NavBar.IsVisible && !console.DirectPanel.IsVisible);
        Assert.True(session.Mirroring);
        Assert.False(session.Visible); // hidden until put on air, as before
        var tabs = (System.Windows.Controls.Primitives.UniformGrid)console.ScreenMode.Parent;
        Assert.Same(console.ScreenMode, tabs.Children[0]);
    }, asOpened: true);

    // A song (no deck) in the output panel: the strip shows its pages and the next one; a picked page turns the panel
    // and the caption follows it.
    [Fact]
    public Task TheStripShowsTheOutputPanelsPagesAndAPickTurnsThePanel() => WithConsole(async (console, session, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var main = Main(console);
        var verses = new LiveQueueItem("song:a045-verses", "세 절 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n첫째 절\n[2]\n둘째 절\n[3]\n셋째 절" };
        main.LoadQueue([verses]);
        main.SelectedItem = null; // no row picked (a picked one would go to the empty panel)
        console.ScreenMode.IsChecked = true;
        await ClickAsync(console.AirButton);
        await Pump(console, 100);
        // nothing in the output panel: the strip keeps its place (the layout does not jump from item to item) and says so
        Assert.True(console.StepStrip.IsVisible && console.NoSteps.IsVisible);
        Assert.Empty(editor.Steps);
        double previewHeight = console.ObsPreviewBox.ActualHeight;
        await SendToOutput(console, main, verses);

        Assert.True(console.StepStrip.IsVisible && !console.NoSteps.IsVisible);
        Assert.Equal(previewHeight, console.ObsPreviewBox.ActualHeight); // the preview kept its size
        Assert.Equal(["1/3", "2/3", "3/3"], editor.Steps.Select(step => step.Label));
        Assert.Equal(["첫째 절", "둘째 절", "셋째 절"], editor.Steps.Select(step => step.Text));
        Assert.Equal(editor.Steps[0], editor.CurrentStep);
        Assert.Same(editor.Steps[1], editor.NextStep); // marked in the strip

        console.StepStrip.SelectedItem = editor.Steps[2];
        await Pump(console, 300);
        Assert.Equal("셋째 절", main.OutputLyricsText.Trim()); // the panel turned ...
        Assert.Equal("셋째 절", session.PublicText); // ... and the caption with it
        Assert.Equal(editor.Steps[2], editor.CurrentStep);
        Assert.Null(editor.NextStep); // the last page
        Assert.Equal(Axicultus.Wpf.Controls.LiveState.Off, main.Session.Current.State);

        console.DirectMode.IsChecked = true; // typing: no strip, no steps back and on
        await Pump(console, 100);
        Assert.False(console.StepStrip.IsVisible || console.NavBar.IsVisible);
    });

    // Another worship list ends the tab's session (the main window makes a new tab for the new list); the end is told once.
    [Fact]
    public Task AnotherWorshipListEndsTheTabOnce() => WithConsole(async (console, session, _) =>
    {
        var ends = new System.Collections.Generic.List<bool>();
        console.Ended += ends.Add;
        await ClickAsync(console.AirButton);
        Assert.True(session.Visible);
        var main = Main(console);
        typeof(MainViewModel).GetMethod("SetCurrentWorshipList", Private)!.Invoke(main, ["a045 다른 예배", null]); // as loading a list does
        await Pump(console, 100);
        Assert.Equal([true], ends); // planChanged
        Assert.False(session.Visible); // the new list starts hidden
        console.EndSession(); // (the test's own end) tells nothing more
        Assert.Equal([true], ends);
    });

    // The main window gives the tab the plain keys typed in it; function keys and Ctrl or Alt shortcuts stay the main
    // window's (F9 the black screen, Ctrl+S, the scene shortcuts).
    [Theory]
    [InlineData(Key.Space, ModifierKeys.None, true)]
    [InlineData(Key.Escape, ModifierKeys.None, true)] // hides the caption, does not end the show
    [InlineData(Key.Down, ModifierKeys.None, true)]
    [InlineData(Key.D2, ModifierKeys.None, true)]
    [InlineData(Key.C, ModifierKeys.None, true)] // no verse jump of the preview item behind the tab
    [InlineData(Key.Space, ModifierKeys.Shift, true)]
    [InlineData(Key.F9, ModifierKeys.None, false)]
    [InlineData(Key.F6, ModifierKeys.None, false)]
    [InlineData(Key.S, ModifierKeys.Control, false)]
    [InlineData(Key.D1, ModifierKeys.Control | ModifierKeys.Alt, false)]
    public void TheTabsOwnKeysAreThePlainOnes(Key key, ModifierKeys modifiers, bool tabs) =>
        Assert.Equal(tabs, MainWindow.IsCaptionTabKey(key, modifiers));
}
