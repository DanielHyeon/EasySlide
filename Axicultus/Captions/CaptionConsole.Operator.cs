using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Captions;

// The operator's side of the console (a037): its place on screen, keys, the settings window, the step strip and the
// next preview. What goes on air is decided in CaptionConsole.xaml.cs as before.
public partial class CaptionConsole
{
    /// <summary>OBS, camera, scenes, browser source, recovery and PPT fine-tuning, in a window of their own.</summary>
    internal CaptionSettingsWindow Settings { get; }

    // The settings window's controls, by the names the console has always used for them.
    private TextBox Host => Settings.Host;
    private TextBox Port => Settings.Port;
    private PasswordBox Password => Settings.Password;
    private TextBlock SavedPassword => Settings.SavedPassword;
    private CheckBox AutoObs => Settings.AutoObs;
    private TextBlock ServerLabel => Settings.ServerLabel;
    private Button AddObsSource => Settings.AddObsSource;
    private TextBox CameraSource => Settings.CameraSource;
    private CheckBox PreviewObsCamera => Settings.PreviewObsCamera;
    private ComboBox Scenes => Settings.Scenes;
    private TextBox SourceName => Settings.SourceName;
    private CheckBox EnableSource => Settings.EnableSource;
    private TextBox Scene1 => Settings.Scene1;
    private TextBox Scene2 => Settings.Scene2;
    private TextBox Scene3 => Settings.Scene3;
    private TextBox Scene4 => Settings.Scene4;
    private Button RecoverButton => Settings.RecoverButton;
    private ComboBox Slides => Settings.Slides;
    private ComboBox Boxes => Settings.Boxes;
    private TextBox Cue => Settings.Cue;

    private void WireSettings()
    {
        Settings.ConnectButton.Click += ConnectObs;
        Settings.CopyUrlButton.Click += CopyBrowserUrl;
        Settings.AddObsSource.Click += AddCaptionSource;
        Settings.ToggleCameraButton.Click += ToggleCamera;
        Settings.SwitchSceneButton.Click += SwitchScene;
        Settings.RecoverButton.Click += Recover;
        Settings.ReloadDeckButton.Click += OpenLiveDeck;
        Settings.OpenDeckButton.Click += OpenDeck;
        Settings.Slides.SelectionChanged += SlideSelected;
        Settings.Boxes.SelectionChanged += BoxSelected;
        Settings.SaveCueButton.Click += SaveOverride;
        Settings.PreviewKeyDown += OnSettingsKey;
    }

    // A message about something done in the settings window: shown there and on the console.
    private string Told { set => Status.Text = Settings.SettingsStatus.Text = value; }

    // While nothing goes out (hidden, or the output screen off), what would go out, for the dim preview. The camera
    // hint stays behind captions and is hidden while one (or the dim one) is drawn.
    private void UpdateHeld()
    {
        _editor.Held = _captions.Visible && !_captions.ScreenOff ? null
            : new("", _captions.ReadyImage, _captions.ReadyText, _captions.CaptionLines, _captions.ScoreSize,
                _captions.CaptionBackground, _captions.CaptionForeground);
        PreviewHint.Visibility = _captions.HasText || _editor.Held?.HasText == true ? Visibility.Collapsed : Visibility.Visible;
    }

    // After a mouse click on a button, check box, radio or the strip the keyboard does not stay there: typing, it goes to
    // the box; following, to the console itself (no focus mark). Lists and text boxes keep it.
    private void ReleaseClickFocus(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || !ClickedControl(e.OriginalSource as DependencyObject)) return;
        Dispatcher.InvokeAsync(ReleaseFocus, System.Windows.Threading.DispatcherPriority.Input);
    }

    // The look lists: picked with the mouse, the keyboard does not stay in the list when it closes (an arrow would change
    // the caption's look on air); opened with the keyboard, it stays.
    private bool _listByMouse;

    private void WatchLookLists()
    {
        foreach (var list in new[] { LinesBox, ScoreSizeBox })
        {
            list.PreviewMouseDown += (_, _) => _listByMouse = true;
            list.PreviewKeyDown += (_, _) => { if (!list.IsDropDownOpen) _listByMouse = false; };
            list.DropDownClosed += (_, _) =>
            {
                if (!_listByMouse) return;
                _listByMouse = false;
                Dispatcher.InvokeAsync(ReleaseFocus, System.Windows.Threading.DispatcherPriority.Input);
            };
        }
    }

    private static bool ClickedControl(DependencyObject? element)
    {
        for (; element != null; element = element is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element))
        {
            if (element is TextBoxBase or PasswordBox or ComboBox or ComboBoxItem) return false;
            if (element is ButtonBase or ListBoxItem or ListBox) return true;
        }
        return false;
    }

    private void ReleaseFocus() => FocusKeepingColumn(DirectMode.IsChecked == true ? Draft : Surface);

    // The keyboard to the box or the console itself. A focused box asks to be seen: a scrolled column stays where it is.
    private void FocusKeepingColumn(IInputElement target)
    {
        double kept = SourcePanel.VerticalOffset;
        FocusManager.SetFocusedElement(this, target); // (also when the window is not the active one)
        target.Focus();
        SourcePanel.ScrollToVerticalOffset(kept);
    }

    // Enter in the worship list (chosen with Tab) sends the picked item, as the send button does; a held Enter sends once.
    private void WorshipListKey(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None) return;
        e.Handled = true;
        if (!e.IsRepeat && _main.SendWorshipSelectionToOutputCommand.CanExecute(null)) _main.SendWorshipSelectionToOutputCommand.Execute(null);
    }

    // The wheel over the worship list or the typed box scrolls them; at their end (a small window) it scrolls the column.
    private void ColumnWheel(object sender, MouseWheelEventArgs e)
    {
        if (DropDownOpen(LinesBox, ScoreSizeBox)) return; // an open list's own wheel
        for (var at = e.OriginalSource as DependencyObject; at != null && at != SourcePanel;
             at = at is Visual ? VisualTreeHelper.GetParent(at) : LogicalTreeHelper.GetParent(at))
        {
            if (at is Popup) return;
            if (at is not ScrollViewer inner) continue;
            if (e.Delta > 0 ? inner.VerticalOffset > 0 : inner.VerticalOffset < inner.ScrollableHeight) return; // its own
            SourcePanel.ScrollToVerticalOffset(SourcePanel.VerticalOffset - e.Delta / 120d * WheelStep(SystemParameters.WheelScrollLines, SourcePanel.ViewportHeight));
            e.Handled = true;
            return;
        }
    }

    // A wheel notch, as a ScrollViewer's own: the system's lines of 16 DIP, or a screen ("one screen at a time": -1).
    internal static double WheelStep(int lines, double viewport) => lines > 0 ? lines * 16 : viewport;

    // The console's worship list shows the picked row and the row on air when they change elsewhere (the main window,
    // the next item); only then, not on each change of the list or the output (the operator may be scrolling).
    private LiveQueueItem? _shownLive;

    // Hidden (closed), or the list not yet holding the row: tried again when the console shows and when the list changes.
    private void ShowLiveRow()
    {
        if (ReferenceEquals(_main.LiveRow, _shownLive) || !IsVisible || _main.LiveRow is { } live && !WorshipList.Items.Contains(live)) return;
        _shownLive = _main.LiveRow;
        ShowWorshipRow(_shownLive);
    }

    private void ShowWorshipRow(object? item)
    {
        if (item != null) WorshipList.ScrollIntoView(item);
    }

    // A row brought into view moves the list, not the column (a small window keeps what the operator scrolled to), unless
    // the keyboard is in the list (Tab, arrows): then the column follows it.
    private void KeepColumnForList(object sender, RequestBringIntoViewEventArgs e)
    {
        if (!WorshipList.IsKeyboardFocusWithin && e.TargetObject is Visual target && (target == WorshipList || WorshipList.IsAncestorOf(target)))
            e.Handled = true;
    }

    // After a click on the take button or the on-air switch while typing, the next words go into the box.
    private void BackToDraft(object sender, RoutedEventArgs e)
    {
        if (DirectMode.IsChecked == true) FocusKeepingColumn(Draft);
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        Settings.Owner ??= this;
        PlaceSettings();
        Settings.Show();
        Settings.Activate();
    }

    // Every time it opens: over the console, within the work area of the console's screen as the screens are now
    // (scrolling below its own minimum).
    private void PlaceSettings()
    {
        var screens = _screens();
        if (screens.Count == 0 || CaptionWindowPlacement.Bounds(this) is not { } console) return;
        var screen = CaptionWindowPlacement.At(new Point(console.Left + console.Width / 2, console.Top + console.Height / 2), screens);
        var size = CaptionWindowPlacement.Bounds(Settings) is { } shown && Settings.IsVisible ? shown.Size
            : new Size(Settings.Width * screen.Scale, Settings.Height * screen.Scale);
        var wanted = new Rect(console.Left + (console.Width - size.Width) / 2, console.Top + (console.Height - size.Height) / 2, size.Width, size.Height);
        CaptionWindowPlacement.Apply(Settings, CaptionWindowPlacement.Inside(wanted, screen.WorkArea),
            CaptionWindowPlacement.Minimum(new Size(SettingsMinWidth, SettingsMinHeight), screen));
    }

    // ---- where the window is ----

    private readonly string _placementPath;
    private readonly Func<IReadOnlyList<CaptionScreen>> _screens; // read again each time a window is shown
    private const double DefaultWidth = 1040, DefaultHeight = 720, DesignMinWidth = 900, DesignMinHeight = 620;
    private const double SettingsMinWidth = 680, SettingsMinHeight = 480;
    private bool _placed;
    private Rect? _normal; // the window's normal rectangle in pixels, while it was last neither maximized nor minimized
    private WindowState _lastState = WindowState.Normal; // the state before it was minimized

    /// <summary>Shows the console placed on its screen (see <see cref="PlaceOnScreen"/>) and brings it forward.</summary>
    internal void ShowOnScreen()
    {
        PlaceOnScreen();
        Show();
        Activate();
    }

    /// <summary>
    /// Before it is shown: the first time the kept place when a screen still shows it, else the middle of the main
    /// window's screen (the main screen without one); afterwards where it was. Either way within the work area of the
    /// screens connected now (on a small screen the body scrolls). Pixels throughout (screens of other scales).
    /// </summary>
    internal void PlaceOnScreen()
    {
        var screens = _screens();
        if (screens.Count == 0 || IsVisible) return;
        var owner = Owner is { } main ? CaptionWindowPlacement.Bounds(main) : null; // a maximized main window: where it is
        if (_placed && WindowState != WindowState.Normal)
        {
            // Hidden maximized or minimized: its normal rectangle, when no screen shows it now (a screen taken away),
            // comes back into the work area before the state is put on again.
            if (_normal is not { } normal || CaptionWindowPlacement.Fits(Kept(normal), screens)) return;
            var home = CaptionWindowPlacement.Home(owner, screens);
            var back = CaptionWindowPlacement.Centred(normal.Size, owner, home);
            var state = WindowState;
            WindowState = WindowState.Normal;
            CaptionWindowPlacement.Apply(this, back, CaptionWindowPlacement.Minimum(new Size(DesignMinWidth, DesignMinHeight), home));
            _normal = back;
            WindowState = state;
            return;
        }
        var kept = _placed
            ? CaptionWindowPlacement.Bounds(this) is { } now ? Kept(now) : null
            : CaptionWindowPlacement.Read(_placementPath);
        var (bounds, maximized, screen) = CaptionWindowPlacement.Plan(kept, owner, screens, new Size(DefaultWidth, DefaultHeight));
        CaptionWindowPlacement.Apply(this, bounds, CaptionWindowPlacement.Minimum(new Size(DesignMinWidth, DesignMinHeight), screen));
        _normal = bounds;
        if (maximized && !_placed) WindowState = WindowState.Maximized;
        _placed = true;
    }

    private static CaptionWindowPlacement Kept(Rect bounds) =>
        new(bounds.Left, bounds.Top, bounds.Width, bounds.Height, false, CaptionWindowPlacement.PixelVersion);

    private void TrackPlacement()
    {
        void Normal() { if (WindowState == WindowState.Normal && CaptionWindowPlacement.Bounds(this) is { } now) _normal = now; }
        LocationChanged += (_, _) => Normal();
        SizeChanged += (_, _) => Normal();
        StateChanged += (_, _) => { if (WindowState != WindowState.Minimized) _lastState = WindowState; };
    }

    // Its normal rectangle (pixels) and whether it was maximized, also when it is closed minimized from maximized.
    private void KeepPlacement()
    {
        if (WindowState == WindowState.Normal && CaptionWindowPlacement.Bounds(this) is { } now) _normal = now;
        if (_normal is not { } normal) return;
        bool maximized = WindowState == WindowState.Maximized || WindowState == WindowState.Minimized && _lastState == WindowState.Maximized;
        new CaptionWindowPlacement(normal.Left, normal.Top, normal.Width, normal.Height, maximized, CaptionWindowPlacement.PixelVersion).Write(_placementPath);
    }

    // ---- keys ----

    private static readonly Dictionary<Key, int> DisplayKeys = new()
    {
        [Key.D1] = 0, [Key.NumPad1] = 0, [Key.D2] = 1, [Key.NumPad2] = 1, [Key.D3] = 2, [Key.NumPad3] = 2, [Key.D4] = 3, [Key.NumPad4] = 3,
    };

    // Space on air / hidden, 1-4 the display (auto, text, score, score and text; only while following the screen, where
    // those buttons are), Esc hides, the arrows step. Space and the digits are the text's own in a text box or while a
    // syllable is being put together; the arrows in a text box too. With sync on, a held arrow does not race the screen.
    private void OnKey(object sender, KeyEventArgs e)
    {
        if (_obs.TrySceneShortcut(e)) { e.Handled = true; return; }
        bool typing = e.OriginalSource is TextBoxBase or PasswordBox or ComboBox or ComboBoxItem;
        // A button, check box, radio, step or list the operator chose with Tab keeps its own Space and arrows.
        bool chosen = e.OriginalSource is ButtonBase or ListBoxItem or ListBox;
        bool plain = !typing && !_editor.IsComposing && e.Key != Key.ImeProcessed && Keyboard.Modifiers == ModifierKeys.None;
        if (e.Key == Key.Escape && !_editor.IsComposing && !e.IsRepeat && !DropDownOpen(LinesBox, ScoreSizeBox))
        {
            _editor.HideCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Space && plain && !chosen)
        {
            if (!e.IsRepeat) _editor.ToggleVisibleCommand.Execute(null);
            e.Handled = true;
        }
        else if (plain && ScreenMode.IsChecked == true && DisplayKeys.TryGetValue(e.Key, out int display))
        {
            new[] { AutoShow, TextOnly, ScoreOnly, ScoreAndText }[display].IsChecked = true;
            e.Handled = true;
        }
        else if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right && ScreenPanel.IsVisible && !typing && !chosen)
        {
            if (!(e.IsRepeat && _captions.SyncShow)) Step(e.Key is Key.Down or Key.Right ? 1 : -1);
            e.Handled = true;
        }
    }

    // In the settings window: the scene shortcuts, and Esc hides the caption (an open list only closes).
    private void OnSettingsKey(object sender, KeyEventArgs e)
    {
        if (_obs.TrySceneShortcut(e)) { e.Handled = true; return; }
        if (e.Key != Key.Escape || e.IsRepeat || DropDownOpen(Settings.Scenes, Settings.Slides, Settings.Boxes)) return;
        _editor.HideCommand.Execute(null);
        e.Handled = true;
    }

    private static bool DropDownOpen(params ComboBox[] boxes) => boxes.Any(box => box.IsDropDownOpen);

    // The typed caption's box shows three lines (a caption has up to three) at any interface size. The worship list keeps
    // room for two rows under the source's controls (the default window shows them with the step strip): a shorter column
    // (a small window) scrolls rather than cutting off a button.
    // Fitted again when the controls change size, when rows are made (the first ones replace the estimate) and when the
    // box's font changes. Three lines are measured as Hangul lays them out (a fallback font may space lines wider).
    // (a row as shown, else a line of body text and the row's padding; 4: the list's border)
    private void FitSourceColumn()
    {
        var lines = new FormattedText("가나다\n가나다\n가나다", System.Globalization.CultureInfo.CurrentUICulture, Draft.FlowDirection,
            new Typeface(Draft.FontFamily, Draft.FontStyle, Draft.FontWeight, Draft.FontStretch), Draft.FontSize, Brushes.Black,
            VisualTreeHelper.GetDpi(Draft).PixelsPerDip);
        Draft.Height = lines.Height + Draft.Padding.Top + Draft.Padding.Bottom + Draft.BorderThickness.Top + Draft.BorderThickness.Bottom
            + 1; // (1: layout rounding)
        // any row made (a scrolled long list has no first row), else the last one seen
        if (ShownRowHeight() is { } shown) _rowHeight = shown;
        SourceColumn.MinHeight = SourceControls.ActualHeight + WorshipPanel.Margin.Top + SendItemButton.ActualHeight + SendItemButton.Margin.Top
            + 2 * (_rowHeight > 0 ? _rowHeight : WorshipList.FontSize * 1.3 + 8) + 4;
    }

    private double _rowHeight;

    private double? ShownRowHeight() => Enumerable.Range(0, WorshipList.Items.Count).Select(WorshipList.ItemContainerGenerator.ContainerFromIndex)
        .OfType<ListBoxItem>().FirstOrDefault(row => row.ActualHeight > 0)?.ActualHeight;

    // Rows made or dropped (each scroll of a long list): fitted again only when the row height is new.
    private void FitForRows()
    {
        if (_rowHeight > 0 && ShownRowHeight() is { } shown && Math.Abs(shown - _rowHeight) < 0.01) return;
        FitSourceColumn();
    }

    private void WatchSourceColumn()
    {
        SourceControls.SizeChanged += (_, _) => FitSourceColumn();
        SendItemButton.SizeChanged += (_, _) => FitSourceColumn();
        // rows made: fitted after their layout
        WorshipList.ItemContainerGenerator.StatusChanged += (_, _) =>
            Dispatcher.InvokeAsync(FitForRows, System.Windows.Threading.DispatcherPriority.Loaded);
        EventHandler refit = (_, _) => FitSourceColumn();
        var fonts = new[] { Control.FontSizeProperty, Control.FontFamilyProperty }
            .Select(font => System.ComponentModel.DependencyPropertyDescriptor.FromProperty(font, typeof(TextBox))).ToArray();
        foreach (var font in fonts) font.AddValueChanged(Draft, refit);
        Closed += (_, _) => { foreach (var font in fonts) font.RemoveValueChanged(Draft, refit); }; // (the descriptor holds the box)
    }

    // A narrow bar first drops the destinations label, so the OBS status keeps room for its two lines.
    private void NarrowTopBar(object sender, SizeChangedEventArgs e) =>
        DestinationsLabel.Visibility = e.NewSize.Width < 900 ? Visibility.Collapsed : Visibility.Visible;

    // ---- the step strip and the next preview ----

    // The row shown and what it was cut from (the content picture's page and the deck's target, or a render: no page).
    private (BitmapSource Image, ScorePage? Page, RowExtent Target)? _shownRows;
    // Row thumbnails per slide, made once per deck (small copies: they keep no whole picture alive).
    private readonly Dictionary<(string Path, int Slide), ImageSource?[]> _rowThumbs = [];
    // The next preview's last row (what it was cut from and which row), so it is cut once.
    private (BitmapSource Source, int Row, BitmapSource? Image, int Rows)? _nextRow;
    private bool _showingSteps; // the strip is being filled or its step set: no pick of the operator
    private CaptionStep? _scrolledTo;

    // Whether slide n sends its score (the rule of ScoreOn for any slide).
    private bool ScoreOnFor(int slide) => ScoreOnly.IsChecked == true || ScoreAndText.IsChecked == true
        || AutoShow.IsChecked == true && _captions.Deck is { } deck && slide <= deck.Slides.Length
            && deck.Slides[slide - 1].Picture != null
            && (_scoreDeck is not { } prepared || !ReferenceEquals(prepared.Deck, deck) || prepared.Slides.Contains(slide));

    // With the recognized lyrics as the caption, a score row gets its own line of them.
    // (the recognized lyrics box, or the whole text they replaced: the caption is the lyrics read on this slide)
    private string? RowLyrics(CaptionDeck deck, int slide, int row, int rows) =>
        deck.Slides[slide - 1].Boxes.FirstOrDefault(b => b.Key == CaptionSession.RecognizedKey)?.Text is { Length: > 0 } read
        && _captions.TextFor(slide, _captions.BoxKey) == read
        && _rowLyrics.TryGetValue((deck.Path, slide), out var lyrics) && lyrics.Length == rows ? lyrics[row] : null;

    // The strip shows the pages of the output panel's song, passage or text (no deck is followed): Slide is the page, from 1.
    private bool _pageSteps;
    private bool _pagesChanged;

    // The panel's pages are replaced one by one, after its text is told: the strip is filled once, afterwards.
    private void OutputPagesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_pagesChanged) return;
        _pagesChanged = true;
        Dispatcher.InvokeAsync(() =>
        {
            _pagesChanged = false;
            if (!_closed) UpdateSteps();
        });
    }

    private static string PageLabel(OperatorLyricsPageCard page) => page.PositionText.Length > 0 ? page.PositionText : "1/1";

    private void UpdateSteps()
    {
        bool following = ScreenMode.IsChecked == true && _captions.Following && _captions.Deck != null;
        OperatorLyricsPageCard[] pages = ScreenMode.IsChecked == true && !following && _captions.Mirroring ? [.. _main.OutputLyricsPages] : [];
        _pageSteps = pages.Length > 0;
        AutomationProperties.SetName(StepStrip, _pageSteps ? "송출 칸 페이지 (누르면 송출 칸과 자막이 그 페이지로)" : "PPT 단계 (누르면 자막만 그 단계로)");
        _showingSteps = true;
        try
        {
            IReadOnlyList<CaptionStep> steps = following ? DeckSteps(_captions.Deck!)
                : [.. pages.Select(page => new CaptionStep(page.PageIndex + 1, 0, PageLabel(page), FirstLine(page.BodyText), null))];
            if (!steps.SequenceEqual(_editor.Steps)) _editor.Steps = steps;
            int row = _rowShown ? _scoreRow : 0;
            int at = Array.FindIndex(pages, page => page.IsCurrent);
            _editor.CurrentStep = _pageSteps ? (at >= 0 ? steps[at] : null)
                : steps.FirstOrDefault(s => s.Slide == _captions.CurrentSlide && s.Row == row)
                    ?? steps.FirstOrDefault(s => s.Slide == _captions.CurrentSlide);
            StepStrip.SetCurrentValue(Selector.SelectedItemProperty, _editor.CurrentStep);
            // (the panel's next page as the caption would show it: the caption is the panel's text)
            _editor.Next = following ? NextLook(_captions.Deck!)
                : at >= 0 && at + 1 < pages.Length ? new(PageLabel(pages[at + 1]), null, pages[at + 1].BodyText.Trim(),
                    _captions.CaptionLines, _captions.ScoreSize, _captions.CaptionBackground, _captions.CaptionForeground)
                : null;
        }
        finally { _showingSteps = false; }
        // (scrolled to only when the step changes: the operator may be looking further along the strip)
        if (_editor.CurrentStep is { } current && current != _scrolledTo && StepStrip.IsVisible) StepStrip.ScrollIntoView(_scrolledTo = current);
    }

    private List<CaptionStep> DeckSteps(CaptionDeck deck)
    {
        var steps = new List<CaptionStep>();
        for (int n = 1; n <= deck.Slides.Length; n++)
        {
            if (ScoreOnFor(n) && RowThumbs(deck, n) is { Length: > 0 } rows)
                for (int k = 0; k < rows.Length; k++) steps.Add(new(n, k, $"{n}장 {k + 1}줄", "", rows[k]));
            else steps.Add(new(n, 0, $"{n}장", FirstLine(_captions.TextFor(n, _chosenKey)), null));
        }
        return steps;
    }

    private static string FirstLine(string text) => text.Split('\n').Select(line => line.Trim()).FirstOrDefault(line => line.Length > 0) ?? "";

    // shortcut: only slides whose content picture is in the cache get row steps (a render-path score is one step per
    // slide); make rows for renders too if operators ask for them.
    private ImageSource?[]? RowThumbs(CaptionDeck deck, int slide)
    {
        if (_rowThumbs.TryGetValue((deck.Path, slide), out var kept)) return kept;
        if (_scoreDeck is not { } prepared || !ReferenceEquals(prepared.Deck, deck) || !prepared.Slides.Contains(slide)
            || !_pictures.TryGet((deck.Path, slide), out var picture)) return null;
        return _rowThumbs[(deck.Path, slide)] = [.. picture.Page.Fit(prepared.Target).Select(row => Thumbnail(picture.Image, row.Area))];
    }

    private static BitmapSource? Thumbnail(BitmapSource image, Int32Rect area)
    {
        try
        {
            BitmapSource cut = new CroppedBitmap(image, area);
            double scale = Math.Min(1, 96d / Math.Max(1, area.Height));
            if (scale < 1) cut = new TransformedBitmap(cut, new ScaleTransform(scale, scale));
            var copy = new WriteableBitmap(cut);
            copy.Freeze();
            return copy;
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine(error.Message);
            return null;
        }
    }

    // A picked step of a deck: only the caption goes there (the output screen stays, sync or not), as one caption.
    // A picked page of the output panel's song, passage or text: the panel turns to it and the caption follows.
    private void StepPicked(object sender, SelectionChangedEventArgs e)
    {
        if (_showingSteps || StepStrip.SelectedItem is not CaptionStep step || step == _editor.CurrentStep) return;
        if (_pageSteps)
        {
            _main.GoToOutputLyricsPageCommand.Execute(step.Slide - 1);
            UpdateSteps(); // (a page the panel did not turn to: the strip shows the page it is on)
            return;
        }
        if (!_captions.Following) return;
        using (_captions.Hold())
        {
            if (step.Slide != _captions.CurrentSlide) _captions.MoveToSlide(step.Slide);
            if (step.Row != _scoreRow) SetScoreRow(step.Row);
        }
        Changed();
    }

    // What the next button would send: the next row of the score row on air, else the next slide (its first row when it sends a
    // score whose picture is at hand, else its text); null at the end.
    private CaptionLook? NextLook(CaptionDeck deck)
    {
        int slide = _captions.CurrentSlide, row = 0, rows = 0;
        BitmapSource? image = null;
        if (_rowShown && _scoreRow + 1 < _scoreRows && _shownRows is var (shown, page, target))
        {
            row = _scoreRow + 1;
            image = RowOf(shown, row, page?.Fit(target), page, out rows);
        }
        else if (slide < deck.Slides.Length)
        {
            slide++;
            if (ScoreOnFor(slide)) image = FirstRow(deck, slide, out rows);
        }
        else return null;
        bool withText = image == null || ScoreAndText.IsChecked == true;
        string text = !withText ? "" : (image != null ? RowLyrics(deck, slide, row, rows) : null) ?? _captions.TextFor(slide, _chosenKey);
        return new(image != null ? $"{slide}장 {row + 1}줄" : $"{slide}장", image, text,
            _captions.CaptionLines, _captions.ScoreSize, _captions.CaptionBackground, _captions.CaptionForeground);
    }

    // The next slide's first row, from the picture its row on air would come from: its content picture when kept, else
    // its render when made, else the operator's picture of it (rows found once).
    private BitmapSource? FirstRow(CaptionDeck deck, int slide, out int rows)
    {
        rows = 0;
        if (_scoreDeck is { } prepared && ReferenceEquals(prepared.Deck, deck) && prepared.Slides.Contains(slide))
            return _pictures.TryGet((deck.Path, slide), out var picture) ? RowOf(picture.Image, 0, picture.Page.Fit(prepared.Target), picture.Page, out rows) : null;
        // While the deck's pictures are read no row goes out (UpdateScore), so none is previewed.
        if (deck.Slides.Any(s => s.Picture != null) && (_scoreDeck is not { } read || !ReferenceEquals(read.Deck, deck))) return null;
        if (_sharp.TryGetValue((deck.Path, slide), out var render)) return RowOf(render, 0, null, null, out rows);
        return new[] { _main.OutputPowerPoint, _main.PowerPoint }.FirstOrDefault(p => SamePath(p.LoadedContentPath, deck.Path)
            && p.SlideNumber == slide && p.PreviewImage is BitmapSource { PixelHeight: > 1 })?.PreviewImage is BitmapSource preview
            ? RowOf(preview, 0, null, null, out rows) : null;
    }

    // Row k of a picture: from rows measured before (with its marks painted over), or found on it (a render).
    private BitmapSource? RowOf(BitmapSource source, int row, ScoreRow[]? measured, ScorePage? page, out int rows)
    {
        rows = 0;
        if (_nextRow is { } kept && ReferenceEquals(kept.Source, source) && kept.Row == row) { rows = kept.Rows; return kept.Image; }
        if (measured != null && page != null && row < measured.Length) measured[row] = measured[row] with { Erase = page.Erase(row) };
        var cut = measured != null ? CropRow(source, row, measured)
            // the row on air's own picture (its rows are cached), or one of another thread: rows found where it lives
            : ReferenceEquals(_shownRows?.Image, source) || !source.IsFrozen ? CropRow(source, row)
            : CropRow(source, row, ScoreLayout.FindRows(source));
        _nextRow = (source, row, cut?.Image, cut?.Rows ?? 0);
        rows = cut?.Rows ?? 0;
        return cut?.Image;
    }
}
