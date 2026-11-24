using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Theme;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a037 review 1: the on-air states, the native show, keys, an unsaved edit, the next preview, the strip's names, placement.
public sealed partial class CaptionConsoleTests
{
    // A native output show whose slide the test turns.
    private sealed class TurnedShow(int? slide) : IPowerPointSlideShowControl
    {
        public int? Slide = slide; // null: no show
        public Task StartAsync(PowerPointSlideShowRequest request, string outputMonitorName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task TriggerNextAsync(PowerPointSlideShowRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int?> ReadOutputPositionAsync(string path) => Task.FromResult(Slide);
    }

    private static async Task Pump(DispatcherObject owner, int milliseconds)
    {
        var until = DateTime.UtcNow.AddMilliseconds(milliseconds);
        while (DateTime.UtcNow < until)
        {
            await Task.Delay(50);
            await owner.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }
    }

    private static Task Idle(DispatcherObject owner) => owner.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

    [Fact]
    public Task WhenTheOutputScreenIsOffTheCaptionWaitsAndSaysSo()
    {
        var live = new LiveSessionService();
        return WithConsole(async (console, session, _) =>
        {
            console.Draft.SetCurrentValue(TextBox.TextProperty, "주님은 나의 목자");
            await ClickAsync(console.AirButton);
            Assert.Equal(("주님은 나의 목자", false), (session.PublicText, console.HiddenBadge.IsVisible));
            live.GoLive(new LiveQueueItem("x", "song", LiveItemKinds.Song), "m");
            live.HideOutput(blackout: true); // the output screen goes black: nothing goes out (a023)
            await Idle(console);
            Assert.True(session.Visible);
            Assert.Equal(("", false), (session.PublicText, session.HasAudienceText));
            Assert.Equal("송출 대기 · 화면 꺼짐 (Space)", console.AirButton.Content);
            Assert.True(console.HiddenBadge.IsVisible);
            Assert.Equal("화면 꺼짐 · 내보내지 않습니다", console.HiddenBadgeText.Text);
            Assert.Equal(Visibility.Visible, console.HeldCaption.Visibility); // what goes out when the screen is back, dim
            Assert.Equal("주님은 나의 목자", console.HeldCaption.LowerThirdText.Text);
            Assert.Equal(Visibility.Collapsed, console.PreviewHint.Visibility);
            await ClickAsync(console.AirButton); // hidden by the operator
            Assert.Equal("숨김 중 · 내보내지 않습니다", console.HiddenBadgeText.Text);
            Assert.Contains("숨김", (string)console.AirButton.Content);
        }, live: live);
    }

    [Fact]
    public Task AStepPickedWhileANativeShowRunsStaysUntilTheShowTurns()
    {
        var live = new LiveSessionService();
        var show = new TurnedShow(1);
        return WithConsole(async (console, session, directory) =>
        {
            var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
            var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
                CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
            Assert.True(await console.PrepareDeckAsync(path, 1));
            console.ScreenMode.IsChecked = true;
            session.Follow(1, CaptionDeck.AllTextKey);
            live.GoLive(new LiveQueueItem("p", "deck", LiveItemKinds.PowerPoint) { ContentPath = path, SlideNumber = 1 }, "m");
            await Idle(console);
            console.StepStrip.SelectedItem = editor.Steps[2];
            await Pump(console, 1200); // the show is read every 400 ms and still says 1
            Assert.Equal(3, session.CurrentSlide);
            typeof(CaptionConsole).GetMethod("Previous", Private)!.Invoke(console, [null, null]); // the console's own arrow, no sync
            await Pump(console, 1000);
            Assert.Equal(2, session.CurrentSlide);
            show.Slide = 3; // the show turns: the caption follows it
            await Pump(console, 1000);
            Assert.Equal(3, session.CurrentSlide);
        }, live: live, nativeShow: show);
    }

    [Fact]
    public Task ClickingTakeOrTheSwitchWhileTypingGoesBackToTheBox() => WithConsole(async (console, session, _) =>
    {
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, false);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "1절 주님은 나의 목자");
        console.TakeButton.Focus();
        await ClickAsync(console.TakeButton);
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console));
        Assert.True(session.Visible);
        // The operator types on: the next words go into the box; the display and the air stay.
        Assert.False(Press(console.Draft, Key.D2));
        Assert.False(Press(console.Draft, Key.Space));
        Assert.Equal(("auto", true), (session.Display, session.Visible));
        console.AirButton.Focus();
        await ClickAsync(console.AirButton);
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console));
    });

    [Fact]
    public Task EscHidesFromTheSettingsWindowButFirstClosesAnOpenList() => WithConsole(async (console, session, _) =>
    {
        await ClickAsync(console.SettingsButton);
        await ClickAsync(console.AirButton);
        Assert.True(Press(console.Settings.Cue, Key.Escape));
        Assert.False(session.Visible);
        await ClickAsync(console.AirButton);
        console.LinesBox.IsDropDownOpen = true; // Esc closes the list, the caption stays
        Assert.False(Press(console.LinesBox, Key.Escape));
        Assert.True(session.Visible);
        console.LinesBox.IsDropDownOpen = false;
    });

    [Fact]
    public Task WithSyncOnAHeldArrowDoesNotRaceTheScreen() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        await Idle(console);
        session.Follow(1, CaptionDeck.AllTextKey);
        session.SyncShow = true;
        console.Status.Text = "";
        var held = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(console), 0, Key.Down)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent };
        typeof(KeyEventArgs).GetMethod("SetRepeat", Private)!.Invoke(held, [true]);
        console.RaiseEvent(held);
        Assert.True(held.Handled);
        Assert.Equal("", console.Status.Text); // no turn asked for (there is no output screen to turn here)
        Assert.True(Press(console, Key.Down)); // a single press asks for the turn
        Assert.Contains("송출 화면", console.Status.Text);
        session.SyncShow = false;
    });

    [Fact]
    public Task AnUnsavedCaptionEditStaysWhenTheConsoleMovesTheCaption() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        await Idle(console);
        session.Follow(1, CaptionDeck.AllTextKey);
        console.Settings.Cue.Text = "일장 고침"; // typed, not saved
        console.StepStrip.SelectedItem = editor.Steps[1];
        Assert.Equal((2, 1, "일장 고침"), (session.CurrentSlide, console.Settings.Slides.SelectedItem, console.Settings.Cue.Text));
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]); // the next button
        Assert.Equal((3, 1, "일장 고침"), (session.CurrentSlide, console.Settings.Slides.SelectedItem, console.Settings.Cue.Text));
        Assert.True(Press(console, Key.Up));
        Assert.Equal((2, "일장 고침"), (session.CurrentSlide, console.Settings.Cue.Text));
    });

    [Fact]
    public Task TheNextPreviewFollowsTheLookAndIsNotRebuiltForNothing() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        console.ScreenMode.IsChecked = true;
        console.ScoreAndText.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        var (next, held) = (editor.Next!, editor.Held!);
        Assert.Same(session.CaptionForeground, session.CaptionForeground); // one brush per look
        session.SetVisible(false); // goes out again with nothing new: nothing is redrawn
        typeof(CaptionConsole).GetMethod("UpdateSteps", Private)!.Invoke(console, null);
        Assert.Same(next, editor.Next);
        Assert.Same(held, editor.Held);
        console.LinesBox.SelectedIndex = 0;
        console.ScoreSizeBox.SelectedIndex = 2;
        session.SetStyle(session.Style with { Color = "#FFE14D" });
        Assert.Equal((1, 2, 1, 2), (editor.Next!.CaptionLines, editor.Next.ScoreSize, editor.Held!.CaptionLines, editor.Held.ScoreSize));
        Assert.Same(session.CaptionForeground, editor.Next.CaptionForeground);
        // The next slide's picture dropped from the cache: no row to show until it is read again, then it is shown.
        var deck = session.Deck!;
        var cache = (ScorePictures)typeof(CaptionConsole).GetField("_pictures", Private)!.GetValue(console)!;
        Typed(console, "_nextRow", null);
        Next(console); // row 2 of slide 1
        cache.Clear();
        Typed(console, "_nextRow", null);
        typeof(CaptionConsole).GetMethod("UpdateSteps", Private)!.Invoke(console, null);
        Assert.Null(editor.Next!.PublicImage);
        await (Task)typeof(CaptionConsole).GetMethod("LoadPictureAsync", Private)!.Invoke(console, [deck, 2])!;
        Assert.NotNull(editor.Next!.PublicImage);
        Assert.Equal("2장 1줄", editor.Next.Label);

        static void Typed(CaptionConsole console, string field, object? value) => typeof(CaptionConsole).GetField(field, Private)!.SetValue(console, value);
        static void Next(CaptionConsole console) => typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
    });

    [Fact]
    public Task WhileHiddenWithoutTypingAsItGoesThePreviewShowsTheLastTake() => WithConsole(async (console, session, _) =>
    {
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, false);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "1절");
        await ClickAsync(console.TakeButton); // on air
        await ClickAsync(console.AirButton); // hidden
        console.Draft.SetCurrentValue(TextBox.TextProperty, "2절 주님"); // typed, not taken
        await Idle(console);
        Assert.Equal("1절", console.HeldCaption.LowerThirdText.Text); // what Space would put on air
        Assert.Equal("", session.PublicText);
    });

    [Fact]
    public Task TheStripsStepsHaveNames() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장", "둘째 줄"), CaptionAllTextTests.TextSlide("이장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        await Idle(console);
        console.StepStrip.UpdateLayout();
        var peer = (ListBoxAutomationPeer)UIElementAutomationPeer.CreatePeerForElement(console.StepStrip);
        Assert.Equal(["1장 · 일장", "2장 · 이장"], (peer.GetChildren() ?? []).Select(child => child.GetName()));
    });

    [Fact]
    public void AMaximizedWindowsRectangleIsWhereItIsNotWhereItWouldBeRestored() => StaHelper.RunOnSta(() =>
    {
        var main = new Window { Left = 0, Top = 0, Width = 300, Height = 200, ShowActivated = false, ShowInTaskbar = false };
        try
        {
            main.Show();
            main.WindowState = WindowState.Maximized;
            var bounds = CaptionWindowPlacement.Bounds(main)!.Value;
            var screen = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
            Assert.InRange(bounds.Left + bounds.Width / 2, screen.Width / 2 - 20, screen.Width / 2 + 20); // its screen, not its restore place
        }
        finally { main.Close(); }
    });

    [Fact]
    public Task TheConsoleAndItsSettingsFitTheScreensAsTheyAreEachTimeTheyOpen()
    {
        var laptop = new CaptionScreen(new(0, 0, 1366, 768), new(0, 0, 1366, 728), 1, true);
        int reads = 0;
        return WithConsole(async (console, _, _) =>
        {
            console.PlaceOnScreen();
            console.Show();
            await Idle(console);
            await ClickAsync(console.SettingsButton);
            var settings = CaptionWindowPlacement.Bounds(console.Settings)!.Value;
            Assert.True(settings.Top >= 0 && settings.Bottom <= 728.5 && settings.Left >= 0 && settings.Right <= 1366.5, $"settings at {settings}");
            console.Settings.Close(); // hidden
            await ClickAsync(console.SettingsButton);
            console.Close(); // hidden
            console.PlaceOnScreen();
            Assert.Equal(4, reads); // the console twice, the settings window twice
        }, screens: () => { reads++; return [laptop]; }, savedWindow: """{"Left":0,"Top":4,"Width":1040,"Height":720,"Maximized":false,"Version":2}""", show: false);
    }

    // design 18 (e): a control chosen with Tab keeps Space and the arrows; a mouse click leaves the keyboard elsewhere.
    [Fact]
    public Task ACheckBoxChosenWithTabKeepsItsSpace() => WithConsole(async (console, session, _) =>
    {
        console.ScreenMode.IsChecked = true;
        await Idle(console);
        console.SyncShow.Focus();
        Assert.False(Press(console.SyncShow, Key.Space)); // not the on-air switch
        foreach (var routed in new[] { Keyboard.KeyDownEvent, Keyboard.KeyUpEvent })
            console.SyncShow.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(console.SyncShow), 0, Key.Space) { RoutedEvent = routed });
        Assert.Equal((true, false), (session.SyncShow, session.Visible)); // the check box's own Space
        session.SyncShow = false;
        Assert.False(Press(console.TextOnly, Key.Right)); // a radio's arrows
    });

    [Fact]
    public Task AStepChosenWithTheKeyboardKeepsItsArrows() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        await Idle(console);
        console.StepStrip.UpdateLayout();
        var item = (ListBoxItem)console.StepStrip.ItemContainerGenerator.ContainerFromIndex(0);
        Assert.False(Press(item, Key.Right)); // the list moves its selection itself
        Assert.Equal(1, session.CurrentSlide); // no step of the console's own
    });

    [Fact]
    public Task AMouseClickLeavesSpaceToTheOnAirSwitch() => WithConsole(async (console, session, _) =>
    {
        void Click(UIElement target) => target.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseUpEvent });
        console.ScreenMode.IsChecked = true;
        await Idle(console);
        console.NextButton.Focus();
        Click(console.NextButton);
        await Idle(console);
        Assert.Same(console.Surface, FocusManager.GetFocusedElement(console)); // following: the console itself
        Assert.True(Press(console.Surface, Key.Space));
        Assert.True(session.Visible);
        console.DirectMode.IsChecked = true;
        console.BoxCheck.Focus();
        Click(console.BoxCheck);
        await Idle(console);
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console)); // typing: the box
    });

    [Fact]
    public Task FollowedWhileTheScreenIsOffAPickStaysWhenItComesBack()
    {
        var live = new LiveSessionService();
        var show = new TurnedShow(1);
        return WithConsole(async (console, session, directory) =>
        {
            var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
            var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
                CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
            Assert.True(await console.PrepareDeckAsync(path, 1));
            live.GoLive(new LiveQueueItem("p", "deck", LiveItemKinds.PowerPoint) { ContentPath = path, SlideNumber = 1 }, "m");
            live.HideOutput(blackout: true);
            console.ScreenMode.IsChecked = true;
            session.Follow(1, CaptionDeck.AllTextKey);
            console.StepStrip.SelectedItem = editor.Steps[2];
            await Pump(console, 600);
            live.Restore(); // the screen comes back, the show still on 1
            await Pump(console, 1200);
            Assert.Equal(3, session.CurrentSlide);
        }, live: live, nativeShow: show);
    }

    [Fact]
    public Task AShowRestartedOnItsLastSlideLeavesTheCaptionWhereItIs()
    {
        var live = new LiveSessionService();
        var show = new TurnedShow(1);
        return WithConsole(async (console, session, directory) =>
        {
            var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
            var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
                CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"), CaptionAllTextTests.TextSlide("사장"));
            Assert.True(await console.PrepareDeckAsync(path, 1));
            LiveQueueItem Deck(int slide) => new("p", "deck", LiveItemKinds.PowerPoint) { ContentPath = path, SlideNumber = slide };
            live.GoLive(Deck(1), "m");
            console.ScreenMode.IsChecked = true;
            session.Follow(1, CaptionDeck.AllTextKey);
            show.Slide = 2;
            await Pump(console, 900);
            Assert.Equal(2, session.CurrentSlide); // the show turned
            console.StepStrip.SelectedItem = editor.Steps[3];
            show.Slide = null; // the show ends
            live.Stop();
            await Pump(console, 600);
            live.GoLive(Deck(2), "m"); // and starts again on the slide it last showed: no turn
            show.Slide = 2;
            await Pump(console, 1200);
            Assert.Equal(4, session.CurrentSlide);
        }, live: live, nativeShow: show);
    }

    [Fact]
    public Task BackToFollowingTheCaptionGoesToTheSlideOnTheScreenNow() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"),
            CaptionAllTextTests.TextSlide("사장"), CaptionAllTextTests.TextSlide("오장"));
        (string, int)? screen = (path, 5);
        console.ScreenDeck = () => screen;
        console.ScreenMode.IsChecked = true;
        await Pump(console, 800); // the deck is read and followed where the screen is
        Assert.Equal((true, 5), (session.Following, session.CurrentSlide));
        console.DirectMode.IsChecked = true;
        screen = (path, 2); // the output panel moves while the operator types
        console.ScreenMode.IsChecked = true;
        await Pump(console, 300);
        Assert.Equal((true, 2), (session.Following, session.CurrentSlide)); // not slide 5, the one last viewed
    });

    [Fact]
    public Task TheNextPreviewTakesItsRowFromTheSamePicturesAsTheRowOnAir() => WithConsole(async (console, session, directory) =>
    {
        // Scores cut in two (the render would show them) and no renderer: the operator's PowerPoint picture is the source.
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        string Pieces() => """<p:spTree>""" + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000)
            + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2000000) + "</p:spTree>";
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()], Pieces(), Pieces(), Pieces());
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        Assert.Equal("2장", editor.Next?.Label); // no picture of slide 2 yet: its text
        var main = (MainViewModel)typeof(CaptionConsole).GetField("_main", Private)!.GetValue(console)!;
        typeof(Axicultus.Wpf.Rendering.PowerPointPreviewViewModel).GetProperty("LoadedContentPath")!.SetValue(main.PowerPoint, path);
        main.PowerPoint.SlideNumber = 2;
        main.PowerPoint.PreviewImage = ScoreSlide(false, 90, 330);
        await Idle(console);
        Assert.Equal("2장 1줄", editor.Next?.Label);
        Assert.NotNull(editor.Next!.PublicImage);
    });

    [Fact]
    public Task AMousePickInALookListLeavesTheArrowsToTheSteps() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        await Idle(console);
        FocusManager.SetFocusedElement(console, console.LinesBox);
        console.LinesBox.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
        console.LinesBox.IsDropDownOpen = true;
        console.LinesBox.SelectedIndex = 1; // 2 lines, picked with the mouse
        console.LinesBox.IsDropDownOpen = false;
        await Idle(console);
        Assert.Same(console.Surface, FocusManager.GetFocusedElement(console));
        Assert.True(Press(console.Surface, Key.Down)); // the next step, not another line count on air
        Assert.Equal((2, 2), (session.CurrentSlide, session.CaptionLines));
        Assert.True(Press(console.Surface, Key.Space));
        Assert.True(session.Visible);
        // Opened with the keyboard, the list keeps the keyboard.
        FocusManager.SetFocusedElement(console, console.LinesBox);
        Assert.False(Press(console.LinesBox, Key.F4));
        console.LinesBox.IsDropDownOpen = true;
        console.LinesBox.IsDropDownOpen = false;
        await Idle(console);
        Assert.NotSame(console.Surface, FocusManager.GetFocusedElement(console)); // not taken away (the test window is not active)
    });

    [Fact]
    public Task AConsoleHiddenMaximizedComesBackOnAScreenThatIsStillThere()
    {
        IReadOnlyList<CaptionScreen> now = [MainScreen, RightScreen];
        return WithConsole((console, _, _) =>
        {
            console.PlaceOnScreen(); // the kept place: maximized on the right screen
            Assert.Equal(WindowState.Maximized, console.WindowState);
            now = [MainScreen]; // the right screen is taken away while the console is hidden
            console.PlaceOnScreen();
            var normal = (Rect?)typeof(CaptionConsole).GetField("_normal", Private)!.GetValue(console);
            Assert.True(normal is { } back && MainScreen.WorkArea.Contains(back), $"normal rectangle {normal}");
            Assert.Equal(WindowState.Maximized, console.WindowState);
            Assert.False(console.IsVisible);
            return Task.CompletedTask;
        }, screens: () => now, savedWindow: """{"Left":2100,"Top":100,"Width":1560,"Height":1080,"Maximized":true,"Version":2}""", show: false);
    }

    [Fact]
    public Task TabNeverStopsOnTheConsoleBodyAndStartsAtTheOnAirSwitch() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [], CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        console.Activate();
        await Idle(console);
        Assert.False(KeyboardNavigation.GetIsTabStop(console.Surface));
        console.Surface.Focus(); // where a mouse click leaves the keyboard
        Assert.True(console.Surface.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)));
        Assert.Same(console.AirButton, Keyboard.FocusedElement);
        var seen = new List<object>();
        for (int n = 0; n < 80 && Keyboard.FocusedElement is UIElement at; n++)
        {
            at.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            if (ReferenceEquals(Keyboard.FocusedElement, console.AirButton)) break; // round the cycle
            seen.Add(Keyboard.FocusedElement);
        }
        Assert.DoesNotContain(console.Surface, seen);
        Assert.Contains(console.NextButton, seen);
    });

    [Fact]
    public Task ANarrowBarDropsTheDestinationsLabelAndWrapsTheObsStatus() => WithConsole((console, _, _) =>
    {
        var content = (FrameworkElement)console.Content;
        var client = new Size(819.2 - 16, 566.4 - 39); // the console on a 1024×768 screen at 125%
        console.ObsLabel.Text = "OBS 연결 전 · 설정에서 연결하세요";
        for (int pass = 0; pass < 2; pass++)
        {
            content.Measure(client);
            content.Arrange(new Rect(client));
            content.UpdateLayout();
        }
        Assert.Equal(Visibility.Collapsed, console.DestinationsLabel.Visibility);
        Assert.True(console.ObsLabel.ActualWidth >= 120, $"OBS status {console.ObsLabel.ActualWidth} wide");
        Assert.True(console.ObsLabel.ActualHeight > console.ObsLabel.FontSize * 1.6, "the status wraps to a second line");
        return Task.CompletedTask;
    }, show: false);
}

// The on-air switch and the badge read at 4.5:1 or better in both themes (they take the Live tokens).
[Collection("WPF Application")]
public sealed class CaptionConsoleContrastTests
{
    private static double Luminance(Color color)
    {
        static double Channel(byte value) { double c = value / 255d; return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }

    private static double Contrast(Brush foreground, Brush background)
    {
        double a = Luminance(((SolidColorBrush)foreground).Color), b = Luminance(((SolidColorBrush)background).Color);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    [Theory]
    [InlineData(ColorTheme.Light)]
    [InlineData(ColorTheme.Dark)]
    public Task TheOnAirStatesReadInBothThemes(ColorTheme theme)
    {
        var live = new LiveSessionService();
        return CaptionConsoleTests.WithConsole(async (console, session, _) =>
        {
            var themes = new ThemeService();
            themes.ApplyTheme(theme);
            try
            {
                var seen = new List<string>();
                async Task Check(string state)
                {
                    await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    double air = Contrast(console.AirButton.Foreground, console.AirButton.Background);
                    Assert.True(air >= 4.5, $"{theme} {state}: the switch reads {air:0.00}:1");
                    if (console.HiddenBadge.IsVisible)
                    {
                        double badge = Contrast(console.HiddenBadgeText.Foreground, console.HiddenBadge.Background);
                        Assert.True(badge >= 4.5, $"{theme} {state}: the badge reads {badge:0.00}:1");
                    }
                    seen.Add($"{state}={air:0.0}");
                }
                await Check("hidden");
                await CaptionConsoleTests.ClickAsync(console.AirButton);
                await Check("on air");
                live.GoLive(new LiveQueueItem("x", "song", LiveItemKinds.Song), "m");
                live.HideOutput(blackout: true);
                await Check("screen off");
                Assert.Equal(3, seen.Count);
            }
            finally { themes.ApplyTheme(ColorTheme.Light); }
        }, live: live);
    }
}
