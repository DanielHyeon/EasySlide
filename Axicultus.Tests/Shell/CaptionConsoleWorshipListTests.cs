using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a041: the worship list in the console (following the screen or typing): pick an item and send it to the output panel
// without leaving a maximized console. A live output shows it as a double-click in the main list does; one that is off
// stays off.
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

    internal static MainViewModel Main(CaptionConsole console) => (MainViewModel)console.WorshipList.DataContext;

    // The first song goes out as the main list's double-click sends it, and the console follows the screen on air.
    private static async Task FirstSongOnAir(CaptionConsole console, MainViewModel main, LiveQueueItem first)
    {
        console.ScreenMode.IsChecked = true;
        main.SelectedItem = first;
        await main.WorshipListDoubleClickCommand.ExecuteAsync(null);
        await ClickAsync(console.AirButton);
        await Pump(console, 300);
    }

    private static ListBoxItem Row(CaptionConsole console, int index)
    {
        console.WorshipList.UpdateLayout();
        return (ListBoxItem)console.WorshipList.ItemContainerGenerator.ContainerFromIndex(index);
    }

    // The visual tree under an element (the row's template).
    internal static System.Collections.Generic.IEnumerable<DependencyObject> Visuals(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var below in Visuals(child)) yield return below;
        }
    }

    [Fact]
    public Task TheNextSongGoesOutFromTheConsoleList() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var (first, second, _) = ThreeSongs(main);
        await FirstSongOnAir(console, main, first);
        Assert.Contains("주님 앞에", session.PublicText);
        Assert.Equal(main.Queue, console.WorshipList.Items.Cast<LiveQueueItem>());

        console.WorshipList.SelectedItem = second;
        await ClickAsync(console.SendItemButton);
        await Pump(console, 300);

        Assert.Same(second, main.OutputItem); // the output screen shows the second song
        Assert.Same(second, main.LiveRow);
        Assert.Contains(Visuals(Row(console, 1)).OfType<TextBlock>(), t => t.Text == "LIVE" && t.IsVisible); // the LIVE badge moved
        Assert.DoesNotContain(Visuals(Row(console, 0)).OfType<TextBlock>(), t => t.Text == "LIVE" && t.IsVisible);
        Assert.Contains("주의 말씀", session.PublicText); // and the caption follows it
    });

    [Fact]
    public Task AnItemOnlyPickedInTheConsoleListMovesNothingOnAir() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var (first, _, third) = ThreeSongs(main);
        await FirstSongOnAir(console, main, first);
        string aired = session.PublicText;

        console.WorshipList.SelectedItem = third;
        await Pump(console, 300);

        Assert.Same(third, main.SelectedItem); // picked in the main list too
        Assert.Same(first, main.OutputItem);
        Assert.Same(first, main.LiveRow);
        Assert.Equal(aired, session.PublicText);
    });

    [Fact]
    public Task SpaceAfterAMouseClickOnAnItemSwitchesTheCaptionAndKeepsThePick() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var (first, second, _) = ThreeSongs(main);
        await FirstSongOnAir(console, main, first);
        var row = Row(console, 1);
        console.WorshipList.SelectedItem = second; // the mouse picks the row ...
        row.Focus();
        row.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseUpEvent });
        await Idle(console);
        Assert.Same(console.Surface, FocusManager.GetFocusedElement(console)); // ... and the keyboard is the console's again

        Assert.True(Press(console.Surface, Key.Space));
        Assert.False(session.Visible);
        Assert.Same(second, main.SelectedItem);
        Assert.Same(first, main.OutputItem);
    });

    // Typing: an item sent from the list goes to the output screen; the typed caption stays and the keyboard is the box's.
    [Fact]
    public Task AnItemSentWhileTypingLeavesTheTypedCaption() => WithConsole(async (console, session, _) =>
    {
        void Click(UIElement target) => target.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseUpEvent });
        var main = Main(console);
        var (_, second, third) = ThreeSongs(main);
        Assert.True(console.WorshipList.IsVisible && console.SendItemButton.IsVisible); // in both sources
        console.Draft.SetCurrentValue(TextBox.TextProperty, "환영합니다");
        await ClickAsync(console.TakeButton);
        Assert.Equal(("환영합니다", true), (session.PublicText, session.Visible));

        var row = Row(console, 1);
        console.WorshipList.SelectedItem = second; // picked with the mouse ...
        row.Focus();
        Click(row);
        await Idle(console);
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console)); // ... the keyboard goes back to the box
        console.SendItemButton.Focus();
        await ClickAsync(console.SendItemButton);
        Click(console.SendItemButton);
        await Pump(console, 300);

        Assert.Same(second, main.OutputItem); // the output panel holds the second song; the output stays off
        Assert.Equal(Axicultus.Wpf.Controls.LiveState.Off, main.Session.Current.State);
        Assert.Equal(("환영합니다", true), (session.PublicText, session.Visible)); // the typed caption stays
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console));
        Assert.False(Press(console.Draft, Key.D2)); // the next words are typed into the box: no display key, no on-air switch
        Assert.False(Press(console.Draft, Key.Space));
        Assert.Equal(("auto", true), (session.Display, session.Visible));

        main.SelectedItem = third; // Enter in the box takes the caption; it sends no worship item
        console.Draft.SetCurrentValue(TextBox.TextProperty, "2절");
        Assert.True(Press(console.Draft, Key.Enter));
        await Pump(console, 300);
        Assert.Equal("2절", session.PublicText);
        Assert.Same(second, main.OutputItem);
    });

    [Fact]
    public Task AListChosenWithTheKeyboardKeepsItsArrowsAndEnterSends() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var (first, second, _) = ThreeSongs(main);
        await FirstSongOnAir(console, main, first);
        console.WorshipList.SelectedItem = second;
        var row = Row(console, 1);
        Assert.True(row.Focus());
        string aired = session.PublicText;
        void KeyIn(UIElement at, Key key) // the key as the keyboard sends it: preview (the console's rules), then the key itself
        {
            if (Press(at, key)) return;
            at.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(at), 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
        }
        KeyIn(row, Key.Down); // the list moves its selection itself, not the caption
        await Idle(console);
        Assert.Same(main.Queue[2], console.WorshipList.SelectedItem);
        Assert.Same(main.Queue[2], main.SelectedItem);
        Assert.Equal((aired, true), (session.PublicText, session.Visible));

        var picked = (ListBoxItem)console.WorshipList.ItemContainerGenerator.ContainerFromIndex(2);
        KeyIn(picked, Key.Enter);
        await Pump(console, 300);
        Assert.Same(main.Queue[2], main.OutputItem);
        Assert.Same(main.Queue[2], main.LiveRow);
        Assert.NotSame(second, main.OutputItem);

        // A held Enter: its repeats send nothing more.
        console.WorshipList.SelectedItem = main.Queue[0];
        var held = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(picked), 0, Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent };
        typeof(KeyEventArgs).GetMethod("SetRepeat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(held, [true]);
        Assert.True(held.IsRepeat);
        picked.RaiseEvent(held);
        await Pump(console, 300);
        Assert.Same(main.Queue[2], main.OutputItem);
    });

    // A press of the mouse as the mouse makes it: the routed MouseDown (it becomes MouseLeftButtonDown and, on its second
    // click, a control's MouseDoubleClick).
    internal static void MousePress(UIElement target, int clicks, MouseButton button = MouseButton.Left)
    {
        var press = new MouseButtonEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, button) { RoutedEvent = Mouse.MouseDownEvent };
        typeof(MouseButtonEventArgs).GetProperty(nameof(MouseButtonEventArgs.ClickCount))!.GetSetMethod(true)!.Invoke(press, [clicks]);
        target.RaiseEvent(press);
        target.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, button) { RoutedEvent = Mouse.MouseUpEvent });
    }

    internal static void DoubleClick(UIElement target)
    {
        MousePress(target, 1);
        MousePress(target, 2);
    }

    // The list's empty space under the rows (its items presenter) and a row's title.
    internal static UIElement Empty(ItemsControl list) => Visuals(list).OfType<ItemsPresenter>().First();

    internal static TextBlock Title(ListBoxItem row, string title) => Visuals(row).OfType<TextBlock>().First(t => t.Text == title);

    // The scenario "항목 더블클릭과 빈 곳 더블클릭": the empty space sends nothing, a row sends that row.
    [Fact]
    public Task ADoubleClickOnARowSendsItAndOnTheEmptySpaceNothing() => WithConsole(async (console, _, _) =>
    {
        var main = Main(console);
        var (first, second, _) = ThreeSongs(main);
        await FirstSongOnAir(console, main, first);
        console.WorshipList.SelectedItem = second;
        console.WorshipList.UpdateLayout();
        DoubleClick(Empty(console.WorshipList));
        await Pump(console, 300);
        Assert.Same(first, main.OutputItem);

        var third = main.Queue[2];
        DoubleClick(Title(Row(console, 2), third.Title)); // the first press picks the row, the second sends it
        await Pump(console, 300);
        Assert.Same(third, main.SelectedItem);
        Assert.Same(third, main.OutputItem);
        Assert.Same(third, main.LiveRow);

        MousePress(Title(Row(console, 1), second.Title), 2, MouseButton.Right); // a right double-click sends nothing
        await Pump(console, 300);
        Assert.Same(third, main.OutputItem);

        // A double-click whose first press did not pick the row (Shift or Ctrl held): the row clicked goes, not the pick.
        console.WorshipList.SelectedItem = main.Queue[0];
        Row(console, 1).RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent });
        await Pump(console, 300);
        Assert.Same(second, main.OutputItem);
        Assert.Same(second, main.SelectedItem);
    });

    [Fact]
    public Task WithAnEmptyWorshipListTheSendButtonIsOffAndSaysWhy() => WithConsole(async (console, _, _) =>
    {
        var main = Main(console);
        console.ScreenMode.IsChecked = true;
        main.LoadQueue(System.Array.Empty<LiveQueueItem>());
        await Idle(console);
        Assert.False(console.SendItemButton.IsEnabled);
        Assert.True(console.WorshipListEmpty.IsVisible);
        ThreeSongs(main);
        main.SelectedItem = main.Queue[0];
        await Idle(console);
        Assert.True(console.SendItemButton.IsEnabled);
        Assert.False(console.WorshipListEmpty.IsVisible);
    });

    [Fact]
    public Task TheConsoleListFollowsTheMainList() => WithConsole(async (console, _, _) =>
    {
        var main = Main(console);
        var (first, second, third) = ThreeSongs(main);
        console.ScreenMode.IsChecked = true;
        main.SelectedItem = second;
        await Idle(console);
        Assert.Same(second, console.WorshipList.SelectedItem);
        main.MoveQueueItem(third, 0);
        main.SelectedItem = first;
        await Idle(console);
        Assert.Equal(new[] { third, first, second }, console.WorshipList.Items.Cast<LiveQueueItem>());
        Assert.Same(first, console.WorshipList.SelectedItem);
        var added = new LiveQueueItem("song:a041-4", "파송 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n가라 가라" };
        main.Queue.Add(added);
        await Idle(console);
        Assert.Same(added, console.WorshipList.Items.Cast<LiveQueueItem>().Last());
    });

    // The app's type sizes at an interface size (ThemeService's scaled sizes), on the console itself: the same whether or
    // not another test made an Application. (An Application made here would belong to this test's thread only.)
    // The console draws with these large tokens only; body and caption text (a044: 11 at Standard) is not on it.
    private static void TypeSizes(CaptionConsole console, double scale)
    {
        foreach (var (key, size) in new[] { ("Type.Display.FontSize", 32d), ("Type.Title1.FontSize", 24d), ("Type.Title2.FontSize", 20d),
                     ("Type.Title3.FontSize", 18d), ("Type.BodyLarge.FontSize", 16d), ("Type.ButtonLarge.FontSize", 16d) })
            console.Resources[key] = size * scale;
    }

    private static async Task LayOut(CaptionConsole console, double width, double height)
    {
        var content = (FrameworkElement)console.Content;
        var client = new Size(width - 16, height - 39); // a standard frame: 8 each side, the title bar 31
        for (int pass = 0; pass < 3; pass++) // the later passes after what the first one set (the box's and the column's heights)
        {
            content.Measure(client);
            content.Arrange(new Rect(client));
            content.UpdateLayout();
            await Idle(console);
        }
    }

    // Laid out as the window shows it at a size: the source's controls on top (typing: a box of three lines), the list in
    // the rest (two rows at least), the buttons in view.
    [Theory]
    [InlineData(1920, 1040, true, 1.0)] // maximized on a 1080p screen
    [InlineData(1040, 720, true, 1.0)] // the default
    [InlineData(900, 620, true, 1.0)] // the minimum: the left column scrolls rather than cutting off the send button
    [InlineData(1920, 1040, false, 1.0)]
    [InlineData(1040, 720, false, 1.0)]
    [InlineData(900, 620, false, 1.0)]
    [InlineData(1920, 1040, true, 1.4)] // the senior interface size
    [InlineData(1040, 720, true, 1.4)]
    [InlineData(1920, 1040, false, 1.4)]
    [InlineData(1040, 720, false, 1.4)]
    public Task TheListTakesTheRestOfTheColumnAndTheButtonsStayReachable(double width, double height, bool following, double scale) => WithConsole(async (console, session, directory) =>
    {
        TypeSizes(console, scale);
        var main = Main(console);
        for (int n = 0; n < 12; n++) main.Queue.Add(new LiveQueueItem($"song:a041-{n}", $"찬양 {n}", LiveItemKinds.Song) { Lyrics = "[1]\n가사" });
        if (following)
        {
            var path = CaptionAllTextTests.WriteDeck(directory.FullName, [], CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"));
            Assert.True(await console.PrepareDeckAsync(path, 1));
            console.ScreenMode.IsChecked = true;
            session.Follow(1, CaptionDeck.AllTextKey); // the step strip is shown too: the least room the list gets
        }
        await Idle(console);
        await LayOut(console, width, height);
        Assert.Equal(following ? Visibility.Visible : Visibility.Collapsed, console.StepStrip.Visibility);
        Rect In(FrameworkElement element, Visual ancestor) => element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));
        var panel = new Rect(console.SourcePanel.RenderSize);
        var controls = In(following ? console.NextButton : console.TakeButton, console.SourcePanel);
        double rowHeight = Row(console, 0).ActualHeight;
        Assert.True(rowHeight > 20);
        Assert.True(console.WorshipList.ActualHeight >= 2 * rowHeight, $"the list is {console.WorshipList.ActualHeight} high, a row {rowHeight}");
        if (console.SourcePanel.ScrollableHeight > 0)
        {
            Assert.True(width == 900 || scale > 1 && width == 1040, "only the minimum (or the senior size at the default) scrolls");
            console.SourcePanel.ScrollToBottom();
            ((FrameworkElement)console.Content).UpdateLayout();
        }
        else Assert.True(Rect.Inflate(panel, 0.5, 0.5).Contains(controls), $"the source's buttons at {controls}");
        var send = In(console.SendItemButton, console.SourcePanel);
        var inView = Rect.Inflate(panel, 0.5, 0.5); // (rounding)
        Assert.True(inView.Contains(send) && send.Height >= 40, $"the send button at {send} in {panel}");
        if (width == 1920) Assert.True(console.WorshipList.ActualHeight > 300);
        if (following) return;

        // The typed box shows three lines of its font as the font spaces them, not four.
        var box = Visuals(console.Draft).OfType<ScrollViewer>().First();
        console.Draft.Text = "첫째 줄\n둘째 줄\n셋째 줄";
        await LayOut(console, width, height);
        Assert.True(box.ScrollableHeight < 0.5, $"three lines: {box.ExtentHeight} in {box.ViewportHeight}");
        console.Draft.Text += "\n넷째 줄";
        await LayOut(console, width, height);
        Assert.True(box.ScrollableHeight > 0, $"four lines: {box.ExtentHeight} in {box.ViewportHeight}");
    }, show: false);

    // A row picked or put on air elsewhere (the main window, the next item) is scrolled into the console's list.
    [Fact]
    public Task ThePickedRowAndTheRowOnAirComeIntoView() => WithConsole(async (console, _, _) =>
    {
        var main = Main(console);
        main.LoadQueue([.. Enumerable.Range(0, 40).Select(n => new LiveQueueItem($"song:a041-{n}", $"찬양 {n}", LiveItemKinds.Song) { Lyrics = "[1]\n가사 " + n })]);
        console.ScreenMode.IsChecked = true;
        await Idle(console);
        var rows = Visuals(console.WorshipList).OfType<ScrollViewer>().First();
        bool Shown(LiveQueueItem item)
        {
            console.WorshipList.UpdateLayout();
            if (console.WorshipList.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem row || !row.IsVisible) return false;
            var at = row.TransformToAncestor(rows).TransformBounds(new Rect(row.RenderSize));
            return at.Top >= 0 && at.Bottom <= rows.ActualHeight + 1; // (a virtualizing list scrolls by rows: its viewport counts rows)
        }
        Assert.True(rows.ScrollableHeight > 0);
        main.SelectedItem = main.Queue[35]; // picked in the main window
        await Idle(console);
        Assert.True(Shown(main.Queue[35]));

        rows.ScrollToTop(); // the operator looks at the top; the row picked goes on air
        await Idle(console);
        Assert.False(Shown(main.Queue[35]));
        await main.WorshipListDoubleClickCommand.ExecuteAsync(null);
        Assert.True(await Until(console, () => ReferenceEquals(main.Queue[35], main.LiveRow) && Shown(main.Queue[35])), "the row on air comes into view");

        // The same row stays on air while the list changes (an item added): the list stays where the operator scrolled.
        rows.ScrollToTop();
        await Idle(console);
        main.Queue.Add(new LiveQueueItem("song:a041-added", "추가 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n가사" });
        await Pump(console, 300);
        Assert.Same(main.Queue[35], main.LiveRow);
        Assert.False(Shown(main.Queue[35]));
        Assert.Equal(0, rows.VerticalOffset);

        // A row put on air while the console is hidden (it hides on close) comes into view when it shows again.
        console.Hide();
        main.SelectedItem = main.Queue[20];
        await main.WorshipListDoubleClickCommand.ExecuteAsync(null);
        await Pump(console, 200);
        main.SelectedItem = null;
        console.Show();
        Assert.True(await Until(console, () =>
        {
            rows = Visuals(console.WorshipList).OfType<ScrollViewer>().First();
            return ReferenceEquals(main.Queue[20], main.LiveRow) && Shown(main.Queue[20]);
        }), "the row put on air while hidden comes into view when the console shows");
    });

    // Pumps the console's dispatcher until the condition holds (true) or the time runs out (false), instead of a fixed wait.
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

    // A small window: the wheel over the list (at its end) or the typed box scrolls the column; after a send the keyboard
    // goes back to the box without scrolling the column back up.
    [Fact]
    public Task OnASmallWindowTheWheelScrollsTheColumnAndASendKeepsItsPlace() => WithConsole(async (console, _, _) =>
    {
        TypeSizes(console, 1.4); // the senior size: the column scrolls in the smallest window whatever the theme
        var main = Main(console);
        main.LoadQueue([.. Enumerable.Range(0, 12).Select(n => new LiveQueueItem($"song:a041-{n}", $"찬양 {n}", LiveItemKinds.Song) { Lyrics = "[1]\n가사 " + n })]);
        main.SelectedItem = main.Queue[0];
        console.Width = 900;
        console.Height = 620;
        await Pump(console, 200);
        Assert.True(console.SourcePanel.ScrollableHeight > 0, "the column scrolls in the smallest window");
        void Wheel(UIElement over, int delta)
        {
            var preview = new MouseWheelEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, delta) { RoutedEvent = Mouse.PreviewMouseWheelEvent };
            over.RaiseEvent(preview);
            if (!preview.Handled) over.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, delta) { RoutedEvent = Mouse.MouseWheelEvent });
            console.UpdateLayout();
        }
        var rows = Visuals(console.WorshipList).OfType<ScrollViewer>().First();
        Wheel(console.Draft, -120); // the box has nothing to scroll: the column moves
        Assert.True(console.SourcePanel.VerticalOffset > 0);
        console.SourcePanel.ScrollToTop();
        console.UpdateLayout();
        Wheel(Row(console, 0), -120); // the list scrolls first ...
        Assert.Equal((0d, true), (console.SourcePanel.VerticalOffset, rows.VerticalOffset > 0));
        rows.ScrollToBottom();
        console.UpdateLayout();
        Wheel(Row(console, 0), -120); // ... and at its end the column
        Assert.True(console.SourcePanel.VerticalOffset > 0);

        // A row picked elsewhere scrolls the list only: the column stays where the operator scrolled it (the top).
        console.SourcePanel.ScrollToTop();
        rows.ScrollToTop();
        console.UpdateLayout();
        main.SelectedItem = main.Queue[11];
        await Pump(console, 300);
        Assert.Equal(0d, console.SourcePanel.VerticalOffset);
        Assert.True(rows.VerticalOffset > 0);
        main.SelectedItem = main.Queue[0];
        await Pump(console, 200);

        await Pump(console, 300); // (the column fitted to the rows made meanwhile)
        console.SourcePanel.ScrollToVerticalOffset(console.SourcePanel.ScrollableHeight); // (a number: ScrollToBottom would follow a longer column)
        console.UpdateLayout();
        double bottom = console.SourcePanel.VerticalOffset;
        await ClickAsync(console.SendItemButton);
        console.SendItemButton.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseUpEvent });
        await Pump(console, 300);
        Assert.Same(main.Queue[0], main.OutputItem);
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console));
        Assert.Equal(bottom, console.SourcePanel.VerticalOffset); // the list and the send button stay in view

        // So does a click on the take button while typing (the keyboard back to the box).
        console.Draft.Text = "환영합니다";
        console.SourcePanel.ScrollToVerticalOffset(console.SourcePanel.ScrollableHeight);
        console.UpdateLayout();
        bottom = console.SourcePanel.VerticalOffset;
        console.TakeButton.Focus();
        await ClickAsync(console.TakeButton);
        await Pump(console, 200);
        Assert.Same(console.Draft, FocusManager.GetFocusedElement(console));
        Assert.Equal(bottom, console.SourcePanel.VerticalOffset);
    });

    // The column's room for two rows: estimated while the list is empty, then the rows as made; the box follows its font.
    [Fact]
    public Task TheColumnIsFittedAgainWhenRowsAppearAndTheBoxsFontChanges() => WithConsole(async (console, _, _) =>
    {
        TypeSizes(console, 1.0);
        var main = Main(console);
        main.LoadQueue(System.Array.Empty<LiveQueueItem>());
        await Pump(console, 200);
        double Rows() => console.SourceColumn.MinHeight - console.SourceControls.ActualHeight - console.WorshipPanel.Margin.Top
            - console.SendItemButton.ActualHeight - console.SendItemButton.Margin.Top - 4;
        ThreeSongs(main);
        await Pump(console, 300);
        double row = Row(console, 0).ActualHeight;
        Assert.Equal(2 * row, Rows(), 1);

        // A long list scrolled to its end (its first row may no longer be made): the room stays two rows as shown.
        for (int n = 0; n < 150; n++) main.Queue.Add(new LiveQueueItem($"song:a041-long-{n}", $"찬양 {n}", LiveItemKinds.Song) { Lyrics = "[1]\n가사" });
        await Pump(console, 300);
        Visuals(console.WorshipList).OfType<ScrollViewer>().First().ScrollToBottom();
        await Pump(console, 300);
        Assert.Equal(2 * row, Rows(), 1);

        double box = console.Draft.Height;
        console.Resources["Type.Title1.FontSize"] = 36d;
        await Pump(console, 200);
        Assert.Equal(36d, console.Draft.FontSize);
        Assert.True(console.Draft.Height > box * 1.3, $"the box {box} -> {console.Draft.Height}");
    });

    // The main window shows a library song (a source preview) on its third page while the worship row stays picked: the
    // console sends the row from its own first page, as the main list does (its press on the picked row shows the row).
    [Fact]
    public Task ASendFromTheConsoleTakesThePickedRowNotTheMainWindowsSourcePreview() => WithConsole(async (console, _, _) =>
    {
        var main = Main(console);
        var verses = new LiveQueueItem("song:a041-verses", "세 절 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n첫째 절\n[2]\n둘째 절\n[3]\n셋째 절" };
        main.LoadQueue([verses]);
        main.SelectedItem = verses;
        await Idle(console);
        main.PreviewLibrarySong(new Axicultus.Wpf.Data.SongSummary(9041, "도서관 곡", "", 1, 1, "", "", "[1]\n가\n[2]\n나\n[3]\n다"));
        main.NextLyricsPageCommand.Execute(null);
        main.NextLyricsPageCommand.Execute(null);
        Assert.Equal(2, main.LyricsPageIndex); // the library song's third page is shown
        Assert.Same(verses, main.SelectedItem);

        await ClickAsync(console.SendItemButton);
        await Pump(console, 300);
        Assert.Same(verses, main.OutputItem);
        Assert.Equal("첫째 절", main.OutputLyricsText.Trim()); // (the output panel: the output is off and stays off)
    });

    // The output is off: an item sent from the console waits in the output panel and the caption follows it there. The
    // output is not started (the song would be on the screen and in the caption).
    [Fact]
    public Task ASendFromTheConsoleDoesNotStartTheLiveOutput() => WithConsole(async (console, session, _) =>
    {
        var main = Main(console);
        var (_, second, third) = ThreeSongs(main);
        console.ScreenMode.IsChecked = true;
        await ClickAsync(console.AirButton);
        console.WorshipList.SelectedItem = second;
        await ClickAsync(console.SendItemButton);
        await Pump(console, 300);

        Assert.Equal(Axicultus.Wpf.Controls.LiveState.Off, main.Session.Current.State);
        Assert.Null(main.LiveRow);
        Assert.Same(second, main.OutputItem);
        Assert.Contains("주의 말씀", session.PublicText);

        DoubleClick(Title(Row(console, 2), third.Title)); // a double-click and Enter send the same way
        await Pump(console, 300);
        Assert.Equal(Axicultus.Wpf.Controls.LiveState.Off, main.Session.Current.State);
        Assert.Same(third, main.OutputItem);
        Assert.Contains("나의 모든 것", session.PublicText);
    });

    // A song (no deck) in the output panel: the strip shows its pages and the next one; a picked page turns the panel
    // and the caption follows it.
    [Fact]
    public Task TheStripShowsTheOutputPanelsPagesAndAPickTurnsThePanel() => WithConsole(async (console, session, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var main = Main(console);
        var verses = new LiveQueueItem("song:a045-verses", "세 절 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n첫째 절\n[2]\n둘째 절\n[3]\n셋째 절" };
        main.LoadQueue([verses]);
        console.ScreenMode.IsChecked = true;
        await ClickAsync(console.AirButton);
        await Pump(console, 100);
        // nothing in the output panel: the strip keeps its place (the layout does not jump from item to item) and says so
        Assert.True(console.StepStrip.IsVisible && console.NoSteps.IsVisible);
        Assert.Empty(editor.Steps);
        double previewHeight = console.ObsPreviewBox.ActualHeight;
        main.SelectedItem = verses;
        await ClickAsync(console.SendItemButton);
        await Pump(console, 300);

        Assert.True(console.StepStrip.IsVisible && !console.NoSteps.IsVisible);
        Assert.Equal(previewHeight, console.ObsPreviewBox.ActualHeight); // the preview kept its size
        Assert.Equal(["1/3", "2/3", "3/3"], editor.Steps.Select(step => step.Label));
        Assert.Equal(["첫째 절", "둘째 절", "셋째 절"], editor.Steps.Select(step => step.Text));
        Assert.Equal(editor.Steps[0], editor.CurrentStep);
        Assert.Equal(("2/3", "둘째 절"), (editor.Next?.Label, editor.Next?.PublicText));

        console.StepStrip.SelectedItem = editor.Steps[2];
        await Pump(console, 300);
        Assert.Equal("셋째 절", main.OutputLyricsText.Trim()); // the panel turned ...
        Assert.Equal("셋째 절", session.PublicText); // ... and the caption with it
        Assert.Equal(editor.Steps[2], editor.CurrentStep);
        Assert.Null(editor.Next); // the last page
        Assert.Equal(Axicultus.Wpf.Controls.LiveState.Off, main.Session.Current.State);

        console.DirectMode.IsChecked = true; // typing: no strip
        await Pump(console, 100);
        Assert.False(console.StepStrip.IsVisible);
    });
}
