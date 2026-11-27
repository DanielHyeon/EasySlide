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
using Axicultus.Wpf.Localization;

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

    /// <summary>The tab strip over the output (live) column: the live and caption tabs and the on-air switch.</summary>
    internal CaptionTabStrip Strip { get; }

    /// <summary>The on-air switch (on the tab strip, seen on both tabs).</summary>
    internal Button AirButton => Strip.AirButton;

    // After a mouse click on a button, check box, radio or the strip the keyboard does not stay there: typing, it goes to
    // the box; following, to the tab itself (no focus mark). Lists and text boxes keep it. A click anywhere else in the
    // tab (its empty space) brings the keyboard here from the rest of the main window, so Space is the on-air switch.
    private void ReleaseClickFocus(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || !ClickedControl(e.OriginalSource as DependencyObject) && IsKeyboardFocusWithin) return;
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
        // (up to the top: a look list's click lands on the button inside it, and a list left by the keyboard closes again)
        bool clicked = false;
        for (; element != null; element = element is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element))
        {
            if (element is TextBoxBase or PasswordBox or ComboBox or ComboBoxItem) return false;
            clicked |= element is ButtonBase or ListBoxItem or ListBox;
        }
        return clicked;
    }

    /// <summary>The keyboard to the tab while it shows: typing, to the box; following, to the tab itself.</summary>
    internal void ReleaseFocus()
    {
        if (IsVisible) FocusKeepingColumn(DirectMode.IsChecked == true ? Draft : Surface);
    }

    // The keyboard to the box or the tab itself. A focused box asks to be seen: a scrolled column stays where it is.
    private void FocusKeepingColumn(IInputElement target)
    {
        double kept = Body.VerticalOffset;
        if (target is DependencyObject element && FocusManager.GetFocusScope(element) is { } scope)
            FocusManager.SetFocusedElement(scope, target); // (also when the window is not the active one)
        target.Focus();
        Body.ScrollToVerticalOffset(kept);
    }

    // The wheel over the steps or the typed box scrolls them; at their end (a low column) it scrolls the column.
    private void ColumnWheel(object sender, MouseWheelEventArgs e)
    {
        if (DropDownOpen(LinesBox, ScoreSizeBox)) return; // an open list's own wheel
        for (var at = e.OriginalSource as DependencyObject; at != null && at != Body;
             at = at is Visual ? VisualTreeHelper.GetParent(at) : LogicalTreeHelper.GetParent(at))
        {
            if (at is Popup) return;
            if (at is not ScrollViewer inner) continue;
            if (e.Delta > 0 ? inner.VerticalOffset > 0 : inner.VerticalOffset < inner.ScrollableHeight) return; // its own
            Body.ScrollToVerticalOffset(Body.VerticalOffset - e.Delta / 120d * WheelStep(SystemParameters.WheelScrollLines, Body.ViewportHeight));
            e.Handled = true;
            return;
        }
    }

    // A wheel notch, as a ScrollViewer's own: the system's lines of 16 DIP, or a screen ("one screen at a time": -1).
    internal static double WheelStep(int lines, double viewport) => lines > 0 ? lines * 16 : viewport;

    // After a click on the take button or the on-air switch while typing, the next words go into the box.
    private void BackToDraft(object sender, RoutedEventArgs e)
    {
        if (DirectMode.IsChecked == true && IsVisible) FocusKeepingColumn(Draft);
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        Settings.Owner ??= Window.GetWindow(this);
        PlaceSettings();
        Settings.Show();
        Settings.Activate();
    }

    // Every time it opens: over the window the tab is in (the main window), within the work area of its screen as the
    // screens are now (scrolling below its own minimum).
    private void PlaceSettings()
    {
        var screens = _screens();
        if (screens.Count == 0 || Window.GetWindow(this) is not { } host || CaptionWindowPlacement.Bounds(host) is not { } console) return;
        var screen = CaptionWindowPlacement.At(new Point(console.Left + console.Width / 2, console.Top + console.Height / 2), screens);
        var size = CaptionWindowPlacement.Bounds(Settings) is { } shown && Settings.IsVisible ? shown.Size
            : new Size(Settings.Width * screen.Scale, Settings.Height * screen.Scale);
        var wanted = new Rect(console.Left + (console.Width - size.Width) / 2, console.Top + (console.Height - size.Height) / 2, size.Width, size.Height);
        CaptionWindowPlacement.Apply(Settings, CaptionWindowPlacement.Inside(wanted, screen.WorkArea),
            CaptionWindowPlacement.Minimum(new Size(SettingsMinWidth, SettingsMinHeight), screen));
    }

    // ---- the settings window's screens ----

    private readonly Func<IReadOnlyList<CaptionScreen>> _screens; // read again each time the settings window opens
    private const double SettingsMinWidth = 680, SettingsMinHeight = 480;

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

    // The typed caption's box shows at least three lines (a caption has up to three) at any interface size; the steps
    // at least one row. A lower column (a small main window) scrolls rather than cutting a control off: its least height
    // is that of the rows of fixed height and the source's least. Fitted again when those rows change size, when the
    // source changes and when the box's font changes. Three lines are measured as Hangul lays them out (a fallback font
    // may space lines wider).
    private void FitColumn()
    {
        var lines = new FormattedText("가나다\n가나다\n가나다", System.Globalization.CultureInfo.CurrentUICulture, Draft.FlowDirection, // not UI text
            new Typeface(Draft.FontFamily, Draft.FontStyle, Draft.FontWeight, Draft.FontStretch), Draft.FontSize, Brushes.Black,
            VisualTreeHelper.GetDpi(Draft).PixelsPerDip);
        Draft.MinHeight = lines.Height + Draft.Padding.Top + Draft.Padding.Bottom + Draft.BorderThickness.Top + Draft.BorderThickness.Bottom
            + 1; // (1: layout rounding)
        double source = DirectMode.IsChecked == true
            ? Draft.MinHeight + TakeButton.ActualHeight + TakeButton.Margin.Top + InstantRow.ActualHeight + InstantRow.Margin.Top
            : ModeTrack.ActualHeight + StepArea.Margin.Top + StepStrip.MinHeight;
        double least = Column.RowDefinitions.Where(row => !row.Height.IsStar).Sum(row => row.ActualHeight) + SourceArea.Margin.Top + source;
        // The column's width comes and goes with its scroll bar, and the camera row and the look row with the width: a
        // least height that followed every such change would show and hide the bar for ever (a layout loop). It grows at
        // once; it shrinks only by more than the bar's change can make (a little extra to scroll, never a control cut).
        Fits++;
        if (least > Column.MinHeight || Column.MinHeight - least > ScrollBarSlack) Column.MinHeight = least;
    }

    private const double ScrollBarSlack = 64;

    /// <summary>How often the column was fitted (a test sees that it settles).</summary>
    internal int Fits { get; private set; }

    private bool _fitPending;

    // After the layout that changed a row (the rows' heights are known then); once for many changes.
    private void RefitColumn()
    {
        if (_fitPending) return;
        _fitPending = true;
        Dispatcher.InvokeAsync(() => { _fitPending = false; if (!_closed) FitColumn(); }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private Action? _stopWatchingFonts;

    private void WatchColumn()
    {
        foreach (var part in new FrameworkElement[] { TopBar, ModeTrack, TakeButton, InstantRow, Status, NavBar, Stage, LookRow })
            part.SizeChanged += (_, _) => RefitColumn();
        EventHandler refit = (_, _) => RefitColumn();
        var fonts = new[] { Control.FontSizeProperty, Control.FontFamilyProperty }
            .Select(font => System.ComponentModel.DependencyPropertyDescriptor.FromProperty(font, typeof(TextBox))).ToArray();
        foreach (var font in fonts) font.AddValueChanged(Draft, refit);
        _stopWatchingFonts = () => { foreach (var font in fonts) font.RemoveValueChanged(Draft, refit); }; // (the descriptor holds the box)
    }

    // ---- the step strip ----

    // Row thumbnails per slide, made once per deck (small copies: they keep no whole picture alive).
    private readonly Dictionary<(string Path, int Slide), ImageSource?[]> _rowThumbs = [];
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
        AutomationProperties.SetName(StepStrip, UiText.Get("CaptionConsole", _pageSteps ? "StepsPages" : "StepsDeck"));
        _showingSteps = true;
        try
        {
            IReadOnlyList<CaptionStep> steps = following ? DeckSteps(_captions.Deck!)
                : [.. pages.Select(page => new CaptionStep(page.PageIndex + 1, 0, PageLabel(page), FirstLine(page.BodyText), null))];
            if (!steps.SequenceEqual(_editor.Steps)) _editor.Steps = steps;
            steps = _editor.Steps; // the strip's own steps (equal ones are kept): the marks below are found by reference
            int row = _rowShown ? _scoreRow : 0;
            int at = Array.FindIndex(pages, page => page.IsCurrent);
            _editor.CurrentStep = _pageSteps ? (at >= 0 ? steps[at] : null)
                : steps.FirstOrDefault(s => s.Slide == _captions.CurrentSlide && s.Row == row)
                    ?? steps.FirstOrDefault(s => s.Slide == _captions.CurrentSlide);
            StepStrip.SetCurrentValue(Selector.SelectedItemProperty, _editor.CurrentStep);
            _editor.NextStep = _pageSteps ? (at >= 0 && at + 1 < steps.Count ? steps[at + 1] : null)
                : following ? NextDeckStep(steps) : null;
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
                for (int k = 0; k < rows.Length; k++) steps.Add(new(n, k, UiText.Format("CaptionConsole", "StepRow", n, k + 1), "", rows[k]));
            else steps.Add(new(n, 0, UiText.Format("CaptionConsole", "StepSlide", n), FirstLine(_captions.TextFor(n, _chosenKey)), null));
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

    // The step the next button goes to (Next): the next score row of the slide on air, else the next slide (its first
    // row); none at the end, or when the next row is not a step of its own (a score made by rendering the slide).
    private CaptionStep? NextDeckStep(IReadOnlyList<CaptionStep> steps)
    {
        int slide = _captions.CurrentSlide;
        if (_rowShown && _scoreRow + 1 < _scoreRows) return steps.FirstOrDefault(s => s.Slide == slide && s.Row == _scoreRow + 1);
        return steps.FirstOrDefault(s => s.Slide == slide + 1);
    }}
