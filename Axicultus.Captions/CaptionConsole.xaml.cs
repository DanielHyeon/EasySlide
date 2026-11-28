using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Axicultus.Wpf.Media;
using System.Threading;
using Microsoft.Win32;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The camera caption tab of the main window's output (live) column (a045): following the output screen or typing, the
/// steps, the camera preview and the caption's look. <see cref="Strip"/> is the tab strip with the on-air switch. What it
/// takes from the app it is in comes through <see cref="ICaptionHost"/>.
/// </summary>
public partial class CaptionConsole : UserControl
{
    private readonly CaptionSession _captions;
    private readonly CaptionConsoleViewModel _editor;
    private DispatcherOperation? _compositionCommit;
    private readonly CaptionBroadcastServer _server;
    private readonly CaptionRecoveryStore _store;
    private readonly ICaptionHost _host;
    private readonly ICaptionLiveSource _live; // (taken once: the handlers below are removed from the same objects)
    private readonly ICaptionQueueRecovery? _recovery;
    private readonly CameraOwnership _camera;
    private bool _fetchingPreview;
    private readonly DispatcherTimer _previewTimer = new(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly CancellationTokenSource _lifetime = new();
    private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private CaptionRecovery? _recoverable;
    private readonly ObsOperator _obs;
    private readonly ICaptionSlideRenderer? _render;
    private bool _closed, _dirty;
    private long _revision, _deckRequest;
    private readonly string _plan;

    public CaptionConsole(ICaptionHost host, CaptionSession captions, CaptionBroadcastServer server, CaptionRecoveryStore store, ObsOperator obs)
        : this(host, captions, server, store, obs, null) { }

    internal CaptionConsole(ICaptionHost host, CaptionSession captions, CaptionBroadcastServer server, CaptionRecoveryStore store, ObsOperator obs,
        Func<IReadOnlyList<CaptionScreen>>? screens)
    {
        _screens = screens ?? CaptionWindowPlacement.Screens; // (tests give the screens)
        _render = host.Renderer;
        _editor = new(captions);
        _host = host;
        _live = host.Live;
        _recovery = host.Recovery;
        _camera = host.Camera ?? new CameraOwnership(); // (a host without a camera of its own: the OBS camera preview only)
        _obs = obs;
        _captions = captions;
        _server = server;
        _store = store;
        _plan = host.PlanName;
        captions.BeginPlan(_plan);
        captions.SetVisible(false); // a new console starts hidden; the Show button puts the caption on air
        Settings = new CaptionSettingsWindow();
        InitializeComponent();
        Strip = new CaptionTabStrip { DataContext = _editor };
        Strip.AirButton.Click += BackToDraft;
        WireSettings();
        PreviewMouseUp += ReleaseClickFocus;
        WatchLookLists();
        if (captions.Style.Preview > 0) StageRow.Height = new GridLength(Math.Max(StageRow.MinHeight, captions.Style.Preview)); // as the operator sized it
        LinesBox.SelectedIndex = captions.CaptionLines - 1;
        ScoreSizeBox.SelectedIndex = captions.ScoreSize;
        DisplayRadio().IsChecked = true;
        // The previews are camera screens of one size, scaled to fit: the caption is laid out as on the output screen.
        PreviewCaption.Fit(new Size(1920, 1080));
        HeldCaption.Fit(new Size(1920, 1080));
        // before the window's own, or the camera bindings look at the editor first
        CameraPreview.DataContext = CameraStatus.DataContext = Settings.CameraControls.DataContext = _camera;
        if (host.ToggleOutputCamera == null) Settings.ToggleCameraButton.Visibility = Visibility.Collapsed; // (no output camera to turn)
        WatchColumn();
        DataContext = _editor;
        _editor.SaveRequested += Changed;
        Host.Text = obs.Host;
        Port.Text = obs.Port.ToString();
        AutoObs.IsChecked = obs.AutoConnect;
        CameraSource.Text = obs.CameraSource;
        PreviewObsCamera.IsChecked = obs.CameraSource.Length > 0;
        SavedPassword.Visibility = obs.HasSavedPassword ? Visibility.Visible : Visibility.Collapsed;
        BoxCheck.IsChecked = captions.Style.Box;
        _editor.IsInstant = true;
        _live.Changed += LiveChanged;
        Loaded += StartAutomatically;
        var shortcuts = obs.Shortcuts;
        Scene1.Text = shortcuts[0];
        Scene2.Text = shortcuts[1];
        Scene3.Text = shortcuts[2];
        Scene4.Text = shortcuts[3];
        _obs.Changed += ShowObsStatus;
        ShowObsStatus(obs.Status);
        Draft.AddHandler(TextCompositionManager.PreviewTextInputStartEvent, new TextCompositionEventHandler(OnCompositionStarted), true);
        Draft.AddHandler(TextCompositionManager.PreviewTextInputUpdateEvent, new TextCompositionEventHandler(OnCompositionStarted), true);
        Draft.AddHandler(TextCompositionManager.TextInputEvent, new TextCompositionEventHandler((_, _) =>
        {
            if (!_editor.IsComposing || _compositionCommit != null) return;
            // Let TextBox and its binding finish the IME commit before publishing.
            _compositionCommit = Dispatcher.InvokeAsync(() =>
            {
                _compositionCommit = null;
                _editor.CompleteComposition();
            }, DispatcherPriority.DataBind);
        }), true);
        Draft.LostKeyboardFocus += (_, _) => { if (_compositionCommit == null) _editor.IsComposing = false; };
        _debounce.Tick += async (_, _) => { _debounce.Stop(); await SaveAsync(); };
        _previewTimer.Tick += ReadObsPreview;
        IsVisibleChanged += (_, _) =>
        {
            UpdatePreviewTimer();
            StartWhenUsed();
            // (a step put on air while the tab was hidden was not scrolled to: once laid out, it is)
            if (IsVisible) Dispatcher.InvokeAsync(() => { if (!_closed) UpdateSteps(); }, DispatcherPriority.Loaded);
            if (IsVisible && _host.OutputItem == null) StagePickedRow(); // an empty panel: the row already picked
        };
        PreviewObsCamera.Checked += (_, _) => UpdatePreviewTimer();
        PreviewObsCamera.Unchecked += (_, _) => UpdatePreviewTimer();
        _camera.PropertyChanged += CameraChanged;
        if (_recovery != null) _recovery.Replacing += BeforeQueueReplacement;
        _camera.ReleaseLocalCamera += OnCameraRelease;
        PreviewKeyDown += OnKey;
        _host.Changed += HostChanged;
        _captions.PropertyChanged += CaptionChanged;
        UpdateHeld();
        ShowChosenColor();
        Loaded += LoadRecovery;
        if (captions.Deck is { } deck) ShowDeck(deck, captions.CurrentSlide, captions.BoxKey is { Length: > 0 } key ? key : CaptionDeck.AllTextKey);
    }

    private void HostChanged(object? sender, CaptionHostChange change)
    {
        switch (change)
        {
            case CaptionHostChange.Plan when _host.PlanName != _plan:
                EndSession(planChanged: true);
                _captions.BeginPlan(_host.PlanName);
                break;
            case CaptionHostChange.Output: _ = SyncScreenAsync(); break;
            case CaptionHostChange.Picked: StagePickedRow(); break;
            case CaptionHostChange.OutputPages: OutputPagesChanged(); break;
            // The output panel is ready: only now is the deck it has drawn told (after its picture and slide), so its slide counts.
            case CaptionHostChange.OutputPanelDrawn: _ = SyncScreenAsync(); break;
            case CaptionHostChange.OutputPanel:
                _ = SyncScreenAsync();
                UpdateScore();
                break;
            case CaptionHostChange.PreviewPanel: UpdateScore(); break;
        }
    }

    // The tab shown, following the screen, and the show not on the screen (off or standing by): the row picked in the
    // worship list goes to the output panel as the preview's send puts it there (F8), so its caption is seen here
    // without that step. On the screen (live or blacked) a pick changes nothing; nor does a media item (sent, it would
    // start playing) or the row the panel already holds.
    private async void StagePickedRow()
    {
        if (_host.Picked is not { } picked || _editor.IsRestoring) return; // (a recovery picks its own row)
        // after the pick's own work and a row still being sent (a deck being drawn): only the row the operator stops on
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
        if (_host.PickedSending is { IsCompleted: false } sending) await Task.WhenAny(sending);
        if (_closed || !IsVisible || ScreenMode.IsChecked != true || !ReferenceEquals(_host.Picked, picked)
            || _live.Current.State is CaptionLiveState.Active or CaptionLiveState.Hidden
            || picked.Kind == CaptionItemKind.Media || _host.OutputItem?.Id == picked.Id) return;
        if (_host.CanSendPicked) _host.SendPicked();
    }

    /// <summary>The app the tab is in (a test reads the view model behind it).</summary>
    internal ICaptionHost HostApp => _host;

    /// <summary>The tab ended (once): planChanged, another worship list came (the main window makes a new tab for it).</summary>
    internal event Action<bool>? Ended;

    /// <summary>Ends the tab's session: its recovery saved, nothing more watched, the settings window closed.</summary>
    internal void EndSession(bool planChanged = false)
    {
        if (_closed) return;
        Settings.Ending = true; // the settings window closes with the tab
        Finish();
        Ended?.Invoke(planChanged);
    }

    /// <summary>The on-air switch's work (F6 anywhere in the main window).</summary>
    internal void ToggleOnAir() => _editor.ToggleVisibleCommand.Execute(null);

    private void BeforeQueueReplacement()
    {
        if (!_editor.IsRestoring && _dirty) _ = SaveAsync();
    }

    private void OnCameraRelease() => _host.StopLocalCamera();

    private void CameraChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CameraOwnership.ObsOwnsCamera))
        {
            _camera.Preview = null;
            UpdatePreviewTimer();
        }
    }

    private void UpdatePreviewTimer()
    {
        _previewTimer.IsEnabled = !_closed && IsVisible && _camera.ObsOwnsCamera && PreviewObsCamera.IsChecked == true;
        PreviewHint.Text = CaptionText.Get("CaptionConsole", _previewTimer.IsEnabled ? "PreviewWaiting" : "PreviewOff");
        if (!_previewTimer.IsEnabled && _camera.ObsOwnsCamera) _camera.Preview = null;
    }

    private async void ReadObsPreview(object? sender, EventArgs e)
    {
        var connection = _obs.Connection;
        var name = CameraSource.Text.Trim();
        if (connection == null || name.Length == 0) { _camera.Preview = null; return; }
        if (_fetchingPreview) return;
        _fetchingPreview = true;
        try
        {
            var bytes = await connection.ReadPreviewAsync(name, _lifetime.Token);
            var bitmap = await Task.Run(() =>
            {
                using var input = new MemoryStream(bytes, false);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = input;
                image.EndInit();
                image.Freeze();
                return image;
            });
            if (connection != _obs.Connection || !_previewTimer.IsEnabled || name != CameraSource.Text.Trim()) return;
            _camera.ScaleY = 1;
            _camera.Preview = bitmap;
            _camera.Status = CaptionText.Get("CaptionConsole", "ObsPreview");
        }
        catch (Exception ex)
        {
            if (connection == _obs.Connection && _previewTimer.IsEnabled)
            {
                _camera.Preview = null;
                _camera.Status = CaptionText.Format("CaptionConsole", "ObsPreviewWaiting", ex.Message);
            }
        }
        finally { _fetchingPreview = false; }
    }

    // The radio of the kept display choice (auto for anything else).
    private RadioButton DisplayRadio() =>
        new[] { AutoShow, TextOnly, ScoreOnly, ScoreAndText }.FirstOrDefault(radio => (string)radio.Tag == _captions.Display) ?? AutoShow;

    private void CaptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The choice changed elsewhere: its radio shows it (checking it writes back the same choice, which changes nothing).
        if (e.PropertyName == nameof(CaptionSession.Display) && DisplayRadio() is { IsChecked: not true } radio) radio.IsChecked = true;
        if (e.PropertyName == nameof(CaptionSession.Visible)) StartWhenUsed();
        if (e.PropertyName is nameof(CaptionSession.Visible) or nameof(CaptionSession.ScreenOff) or nameof(CaptionSession.HasText)
            or nameof(CaptionSession.ReadyText) or nameof(CaptionSession.ReadyImage) or nameof(CaptionSession.Style)) UpdateHeld();
        if (e.PropertyName == nameof(CaptionSession.Style))
        {
            UpdateSteps(); // the next preview in the new look
            ShowChosenColor();
        }
        if (e.PropertyName is nameof(CaptionSession.CurrentSlide) or nameof(CaptionSession.Following))
        {
            if (e.PropertyName == nameof(CaptionSession.CurrentSlide)) _scoreRow = 0;
            // The source shows on its switch and the slide on the card: the status line is kept for messages.
            Status.Text = "";
            // The viewed slide follows the one on air, unless the operator is editing its caption (not saved yet).
            if (_captions.Following && !CueEdited && Slides.SelectedItem is int shown && shown != _captions.CurrentSlide) Slides.SelectedItem = _captions.CurrentSlide;
            UpdateScore();
        }
    }

    private async void LoadRecovery(object sender, RoutedEventArgs e)
    {
        try
        {
            _recoverable = await Task.Run(() => _store.Read(_plan));
            if (_closed) return;
            RecoverButton.IsEnabled = _recoverable != null;
            if (_recoverable != null)
            {
                _revision = Math.Max(_revision, _recoverable.Revision);
                Status.Text = CaptionText.Get("CaptionConsole", "Recoverable");
            }
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            _host.WriteError("read caption recovery", ex);
            if (!_closed) Status.Text = ex.Message;
        }
    }

    // The main window journals queue edits even while the caption editor is closed.
    private void Changed()
    {
        if (_editor.IsRestoring || _closed) return;
        _dirty = true;
        _debounce.Stop();
        _debounce.Start();
    }

    private async Task SaveAsync()
    {
        if (!_dirty) return;
        _dirty = false;
        var (items, selected) = _recovery?.Capture() ?? (Array.Empty<System.Text.Json.JsonElement>(), -1); // (a host that keeps its own worship order: none)
        var snapshot = new CaptionRecovery(1, ++_revision, _plan, _editor.DraftText, _captions.Deck?.Path,
            SelectedSlide, _chosenKey, [with(_captions.Overrides)], [.. _captions.History], items, selected);
        try { await _store.SaveAsync(snapshot); }
        catch (Exception ex) when (IsFileError(ex))
        {
            _dirty = true;
            _host.WriteError("save caption recovery", ex);
            if (!_closed) Status.Text = CaptionText.Format("CaptionConsole", "RecoverySaveFailed", ex.Message);
        }
    }

    private async void Recover(object sender, RoutedEventArgs e)
    {
        if (_editor.IsRestoring || _recoverable is not { } saved) return;
        if (MessageBox.Show(Settings, CaptionText.Get("CaptionConsole", "RecoverConfirm"), CaptionText.Get("CaptionConsole", "RecoverTitle"),
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        // (a button's handler: whatever the restore runs into is told, never left to end the app)
        try { await RestoreAsync(saved); }
        catch (Exception ex)
        {
            _host.WriteError("recover caption", ex);
            if (!_closed) Told = CaptionText.Format("CaptionConsole", "RecoverFailed", ex.Message);
        }
    }

    internal async Task RestoreAsync(CaptionRecovery saved)
    {
        // Without its deck the recovery is not half applied (the draft and the worship list stay as they are).
        if (saved.DeckPath is { } missing && !File.Exists(missing))
        {
            Told = CaptionText.Format("CaptionConsole", "RecoverNoDeck", Path.GetFileName(missing));
            return;
        }
        _editor.IsRestoring = true;
        ++_deckRequest; // A pending Open must not replace the recovered deck.
        try
        {
            CaptionDeck? deck = null;
            string? warning = null;
            if (saved.DeckPath != null)
            {
                try { deck = await ReadDeckAsync(saved.DeckPath); }
                catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return; }
                catch (Exception ex) when (IsDeckReadError(ex))
                {
                    _host.WriteError("recover caption PPT", ex);
                    warning = CaptionText.Format("CaptionConsole", "ReopenDeck", ex.Message);
                }
            }
            if (_closed) return;
            var (draft, instant) = (_editor.DraftText, _editor.IsInstant);
            _editor.IsInstant = false;
            _editor.DraftText = saved.Draft;
            // The host puts its worship order back; if it cannot, nothing is recovered (the draft as it was).
            try { _recovery?.Restore(saved.Items, saved.Plan, saved.SelectedIndex); }
            catch (Exception ex)
            {
                _host.WriteError("recover worship list", ex);
                (_editor.DraftText, _editor.IsInstant) = (draft, instant);
                if (!_closed) Told = CaptionText.Format("CaptionConsole", "RecoverFailed", ex.Message);
                return;
            }
            if (deck != null)
            {
                _captions.Prepare(deck);
                foreach (var pair in saved.Overrides) _captions.Overrides[pair.Key] = pair.Value;
                ShowDeck(deck, saved.Slide, saved.Box is { Length: > 0 } box ? box : CaptionDeck.AllTextKey);
                // The lyrics in its pictures are read again (the restored choice may be them), the choice kept.
                StartScore(deck);
                LyricsReading = RecognizeDeckAsync(deck, offer: false);
            }
            _captions.History.Clear();
            _captions.History.AddRange(saved.History);
            Told = warning ?? CaptionText.Get("CaptionConsole", "Recovered");
        }
        finally { _editor.IsRestoring = false; }
    }

    private void OnCompositionStarted(object sender, TextCompositionEventArgs e)
    {
        _compositionCommit?.Abort();
        _compositionCommit = null;
        _editor.IsComposing = true;
    }

    private void ToggleCamera(object sender, RoutedEventArgs e)
    {
        if (_camera.ObsOwnsCamera) return;
        _host.ToggleOutputCamera?.Invoke();
    }
    // Enter (or Ctrl+Enter) sends the line and empties the box; Shift+Enter is a new line.
    private void DraftKey(object sender, KeyEventArgs e)
    {
        bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if (e.Key == Key.Enter && !shift)
        {
            e.Handled = true;
            if (!e.IsRepeat) _editor.TakeAndClear();
        }
        else if (e.Key == Key.ImeProcessed && e.ImeProcessedKey == Key.Enter && !shift && !e.IsRepeat)
        {
            // The IME commits its syllable first (and may add a line break); send after that.
            Dispatcher.InvokeAsync(() =>
            {
                _editor.IsComposing = false;
                if (_editor.DraftText.EndsWith('\n')) _editor.DraftText = _editor.DraftText.TrimEnd('\r', '\n');
                _editor.TakeAndClear();
            }, DispatcherPriority.Input);
        }
    }

    private async Task<CaptionDeck> ReadDeckAsync(string path)
    {
        if (Path.GetExtension(path).ToLowerInvariant() is ".ppt" or ".pps")
            return await (_host.ReadLegacyDeck ?? throw new NotSupportedException(CaptionText.Get("CaptionConsole", "LegacyDeckUnsupported")))
                (path, _lifetime.Token).ConfigureAwait(false);
        var token = _lifetime.Token;
        return await Task.Run(() => CaptionDeck.Read(path, token), token).ConfigureAwait(false);
    }

    private async void OpenDeck(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "PowerPoint|*.pptx;*.ppsx;*.pptm;*.ppsm;*.ppt;*.pps", CheckFileExists = true };
        if (dialog.ShowDialog(Settings) == true && await PrepareDeckAsync(dialog.FileName, 1)) FollowLoadedDeck();
    }

    // Reads the deck again: the one on the output screen, or the selected one.
    private async void OpenLiveDeck(object sender, RoutedEventArgs e)
    {
        if (FindCurrentDeck(includeSelected: true) is not var (path, slide)) { Told = CaptionText.Get("CaptionConsole", "NoDeck"); return; }
        if (await PrepareDeckAsync(path, slide) && ScreenMode.IsChecked == true) FollowLoadedDeck();
    }

    private static bool SamePath(string? a, string b) =>
        a != null && string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);

    private string? _readingDeck;
    private int _outputSlide; // the output panel's slide when last seen, live or not (only a change of it while not live is a turn)

    /// <summary>The deck on the output screen and its slide (a test puts a stand-in here; else the live show or output panel).</summary>
    internal Func<(string Path, int Slide)?>? ScreenDeck { get; set; }

    // Screen mode follows the output screen whether or not the show is live: its deck, or else its text.
    private async Task SyncScreenAsync()
    {
        if (_closed || ScreenMode.IsChecked != true || !IsLoaded) return;
        var screen = ScreenDeck ?? (() => FindCurrentDeck(includeSelected: false));
        if (screen() is var (path, slide))
        {
            if (_captions.Deck == null || !SamePath(_captions.Deck.Path, path))
            {
                if (_readingDeck != null && SamePath(_readingDeck, path)) return;
                _captions.ShowScreenText("");
                _readingDeck = path;
                try { if (!await PrepareDeckAsync(path, slide)) return; }
                finally { _readingDeck = null; }
                // the slide the screen shows now: its panel may have drawn the deck (or turned) while it was read
                FollowLoadedDeck(screen() is var (now, shown) && SamePath(now, path) ? shown : slide);
                return;
            }
            if (!_captions.Following) FollowLoadedDeck(slide); // the slide the screen shows now, not the one last viewed
            // Live, the session drives the caption; the panel's slide is noted so the show's end (the panel keeps it) is no turn.
            else if (_live.Current.State != CaptionLiveState.Off) _outputSlide = OutputPanel()?.Slide ?? 0;
            // Not live: a turn of the output panel drives the caption; a caption the operator moved stays until the panel turns.
            else if (OutputPanel() is { } panel && SamePath(panel.Path, path) && panel.Slide >= 1 && panel.Slide != _outputSlide)
            {
                _outputSlide = panel.Slide;
                if (_outputSlide != _captions.CurrentSlide) _captions.MoveToSlide(_outputSlide);
            }
            UpdateScore();
            return;
        }
        // A deck just put in the output panel goes live a moment later; its (empty) panel text is not a caption.
        if (IsDeck(_host.OutputItem)) return;
        string text = _host.OutputText ?? "";
        _captions.ShowScreenText(text);
        UpdateScore();
        Status.Text = !string.IsNullOrWhiteSpace(text) ? "" : CaptionText.Get("CaptionConsole", "NoScreenText");
    }

    private static bool IsDeck(CaptionItem? item) => item is { Kind: CaptionItemKind.PowerPoint, PptPath.Length: > 0 };

    // The output column's deck panel; for a host without one of its own, the output item's own slide is what the
    // output column shows.
    private CaptionSlidePanel? OutputPanel() => _host.OutputPanel
        ?? (_host.OutputItem is { } item && IsDeck(item) ? new CaptionSlidePanel(item.PptPath, item.PptSlide, null) : null);

    // slide: the one the output screen shows (else the one viewed in the settings window).
    private void FollowLoadedDeck(int? slide = null)
    {
        if (_captions.Deck == null) return;
        _captions.Follow(slide ?? SelectedSlide, _chosenKey);
        _outputSlide = OutputPanel()?.Slide ?? 0;
        Status.Text = ""; // the deck is named on the card above
        UpdateScore();
        Changed();
    }

    private void LiveChanged(object? sender, CaptionLiveSnapshot e) => Dispatcher.InvokeAsync(SyncScreenAsync);

    private void SourceChanged(object sender, RoutedEventArgs e)
    {
        if (DirectPanel == null || ScreenPanel == null) return; // during InitializeComponent
        bool screen = ScreenMode.IsChecked == true;
        DirectPanel.Visibility = screen ? Visibility.Collapsed : Visibility.Visible;
        ScreenPanel.Visibility = screen ? Visibility.Visible : Visibility.Collapsed;
        RefitColumn();
        if (!IsLoaded) return;
        if (screen)
        {
            _captions.Mirror(true);
            _ = SyncScreenAsync();
            if (_host.OutputItem == null) StagePickedRow(); // an empty panel: the row already picked
        }
        else
        {
            _captions.SetImage(null);
            _captions.Publish(_editor.DraftText, show: false);
            Status.Text = "";
            Draft.Focus();
            UpdateSteps(); // no deck is followed now
        }
    }

    // A row of the current slide as rendered on screen (a staff with its lyrics), in place of or above the text.
    private int _scoreRow, _scoreRows = 2;
    private bool _rowShown; // a row of the current slide's score is what goes out (not its text while the score is made)

    // Auto / text only / score only / score and text. Auto: the score where the slide's content picture shows staves
    // (until the deck's pictures are measured, a slide with one waits for them with its text on air), else the text.
    private bool ScoreOn => ScoreOnFor(_captions.CurrentSlide);

    private void ScoreChanged(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized) return; // during InitializeComponent (controls further down are not made yet)
        if (sender is RadioButton { Tag: string display }) _captions.Display = display;
        _scoreRow = 0;
        UpdateScore();
    }
    private void SetScoreRow(int row) { _scoreRow = Math.Clamp(row, 0, Math.Max(0, _scoreRows - 1)); UpdateScore(); }

    private void UpdateScore()
    {
        UpdateScoreCore();
        UpdateSteps();
    }

    private void UpdateScoreCore()
    {
        _rowShown = false;
        if (!ScoreOn || ScreenMode.IsChecked != true || !_captions.Following || _captions.Deck is not { } deck)
        {
            ScoreRowLabel.Text = _captions.Following && _captions.Deck != null ? CaptionText.Format("CaptionConsole", "DeckSlide", _captions.CurrentSlide, _captions.Deck.Slides.Length)
                : _captions.Mirroring ? (_host.OutputItem?.Title is { Length: > 0 } title ? title : CaptionText.Get("CaptionConsole", "ScreenText")) : "";
            // the deck name only while it is followed; its line stays, so the list below does not move from item to item
            DeckLabel.Visibility = _captions.Following ? Visibility.Visible : Visibility.Hidden;
            _captions.SetImage(null);
            return;
        }
        static bool Same(string? a, string b) => a != null && string.Equals(Path.GetFullPath(a), b, StringComparison.OrdinalIgnoreCase);
        BitmapSource? image = null;
        ScoreRow[]? measured = null;
        bool rendered = true;
        // The score picture inside the deck first (no background, no wait for PowerPoint); the deck's pictures are read
        // before its first row is cut, so every row has the deck's one size. Meanwhile no row (of the last song) is on air.
        if (deck.Slides.Any(s => s.Picture != null))
        {
            if (_scoreDeck is not { } prepared || !ReferenceEquals(prepared.Deck, deck))
            {
                if (!ReferenceEquals(_preparingScore, deck)) _ = PrepareScoreAsync(deck);
                ScoreRowLabel.Text = CaptionText.Get("CaptionConsole", "ScorePreparing"); // (not the last deck's rows)
                _captions.SetImage(null);
                Status.Text = CaptionText.Get("CaptionConsole", "ScoreMaking");
                return;
            }
            if (prepared.Slides.Contains(_captions.CurrentSlide))
            {
                // Measured once, off the UI thread: only cut here.
                // (while it is read again, the last row does not stay on air)
                if (ContentPicture(deck, _captions.CurrentSlide) is not var (picture, page)) { _captions.SetImage(null); Status.Text = CaptionText.Get("CaptionConsole", "ScoreMaking"); return; }
                (image, measured, rendered) = (picture, page.Fit(prepared.Target), false);
                _pictures.OnAir = (deck.Path, _captions.CurrentSlide);
                int sent = Math.Clamp(_scoreRow, 0, measured.Length - 1);
                measured[sent] = measured[sent] with { Erase = page.Erase(sent) };
            }
        }
        // Else a full-size render of the slide; the small operator picture of it stands in until that is ready.
        if (rendered) image = SharpSlide(deck, _captions.CurrentSlide) ?? new[] { _host.OutputPanel, _host.PreviewPanel }.FirstOrDefault(p =>
            p != null && Same(p.Path, deck.Path) && p.Slide == _captions.CurrentSlide && p.Picture is BitmapSource { PixelHeight: > 1 })?.Picture as BitmapSource;
        // No row of this slide yet: the last row (maybe of another slide) does not stay on air; its text does meanwhile,
        // and the arrows turn slides (no row is shown).
        if (image == null)
        {
            _captions.SetImage(null);
            Status.Text = CaptionText.Get("CaptionConsole", "ScoreMaking");
            return;
        }
        if ((measured != null ? CropRow(image, _scoreRow, measured) : CropRow(image, _scoreRow)) is not var (cropped, rows, found))
        {
            _captions.SetImage(null);
            Status.Text = CaptionText.Get("CaptionConsole", "ScoreFailed");
            return;
        }
        ScoreSource = rendered ? "render" : "picture";
        _scoreRows = rows;
        _scoreRow = Math.Min(_scoreRow, rows - 1);
        bool withText = ScoreAndText.IsChecked == true;
        // With the recognized lyrics as the caption, the row shown gets its own line of them.
        // (the recognized lyrics box, or the whole text they replaced: the caption is the lyrics read on this slide)
        int current = _captions.CurrentSlide;
        string? rowLyrics = withText ? RowLyrics(deck, current, _scoreRow, rows) : null;
        _captions.SetImage(cropped, withText, rowLyrics);
        _rowShown = true;
        DeckLabel.Visibility = Visibility.Visible;
        ScoreRowLabel.Text = found ? CaptionText.Format("CaptionConsole", "ScoreRowFound", _captions.CurrentSlide, deck.Slides.Length, _scoreRow + 1, rows)
            : CaptionText.Format("CaptionConsole", _scoreRow == 0 ? "ScoreTopHalf" : "ScoreBottomHalf", _captions.CurrentSlide, deck.Slides.Length);
        Status.Text = ""; // the row is on the card above
    }

    // Full-size slide pictures for the score (the operator's own are 640×360): rendered once, the next one ahead.
    private readonly Dictionary<(string Path, int Slide), BitmapSource> _sharp = [];
    private readonly HashSet<(string Path, int Slide)> _rendering = [];

    private BitmapSource? SharpSlide(CaptionDeck deck, int slide)
    {
        if (_render == null) return null;
        if (slide < deck.Slides.Length) _ = RenderSharpAsync(deck.Path, slide + 1);
        if (_sharp.TryGetValue((deck.Path, slide), out var image)) return image;
        _ = RenderSharpAsync(deck.Path, slide);
        return null;
    }

    /// <summary>The wait before a failed render of the slide on air is tried again (twice, the second wait double); a test shortens it.</summary>
    internal TimeSpan RenderRetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    // A render that fails (PowerPoint busy with the show, a timeout) is tried again while its slide is on air; after the
    // third failure the operator is told, so the console is not left at "making the score picture" (the text goes out).
    private async Task RenderSharpAsync(string path, int slide)
    {
        if (_closed || _render == null || _sharp.ContainsKey((path, slide)) || !_rendering.Add((path, slide))) return;
        bool OnAir() => !_closed && _captions.Deck?.Path == path && _captions.CurrentSlide == slide;
        bool made = false;
        Exception? failure = null;
        try
        {
            for (int attempt = 1; ; attempt++)
            {
                try { made = await RenderSlideAsync(path, slide, error => failure = error) is { }; }
                catch (Exception error) when (error is not OperationCanceledException) { failure = error; }
                if (made || attempt == 3 || !OnAir()) break;
                await Task.Delay(RenderRetryDelay * attempt, _lifetime.Token);
                if (!OnAir()) break;
            }
        }
        catch (OperationCanceledException) { return; } // the console closes
        finally { _rendering.Remove((path, slide)); }
        if (_closed) return;
        if (!made)
        {
            // (a row cut from the operator's small picture of the slide stands in: nothing to tell)
            if (!OnAir() || _rowShown) return;
            _host.WriteError($"render score slide · slide {slide}", failure ?? new InvalidOperationException("render failed"));
            Status.Text = CaptionText.Format("CaptionConsole", "ScoreRenderFailed", failure?.Message ?? CaptionText.Get("CaptionConsole", "RenderFailed"));
        }
        else if (OnAir()) UpdateScore();
        else if (_captions.Deck?.Path == path && _captions.CurrentSlide + 1 == slide) UpdateSteps(); // the next preview's row
    }

    // A full-size picture of the slide, kept for the score; null when PowerPoint could not render it (failed is told why).
    private async Task<(BitmapSource Image, byte[] Bytes, string ContentType)?> RenderSlideAsync(string path, int slide, Action<Exception>? failed = null)
    {
        if (_render == null) return null;
        var result = await _render.RenderAsync(path, slide, 1920, 1080, TimeSpan.FromSeconds(30), _lifetime.Token);
        if (result.Failure is { } failure)
        {
            failed?.Invoke(new CaptionRenderException(failure));
            return null;
        }
        using var stream = new MemoryStream(result.Bytes);
        var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
            BitmapCacheOption.OnLoad).Frames[0];
        frame.Freeze();
        if (_sharp.Count >= 40) _sharp.Clear();
        _sharp[(path, slide)] = frame;
        return (frame, result.Bytes, result.ContentType);
    }

    /// <summary>Where the score row on air was cut from: "picture" (the deck's content picture) or "render" (the slide).</summary>
    internal string ScoreSource { get; private set; } = "";


    // The followed deck's content pictures: the slides whose picture shows staves and the target all their rows are cut
    // to. The pictures and their measured rows are kept up to a size in bytes and read and measured again (off the UI
    // thread) when dropped; each picture is measured from itself alone, so it is cut the same however often it is read.
    internal const long PictureCache = 256L << 20;
    private (CaptionDeck Deck, HashSet<int> Slides, RowExtent Target)? _scoreDeck;
    private (CaptionDeck Deck, HashSet<int> Slides)? _noStaves;
    private CaptionDeck? _preparingScore;
    private Task _scorePrepared = Task.CompletedTask;
    private CancellationTokenSource? _preparation;
    private readonly ScorePictures _pictures = new(PictureCache);
    private readonly HashSet<(CaptionDeck Deck, int Slide)> _loadingPictures = [];

    /// <summary>The memory a content picture and its measured rows take.</summary>
    internal static long Bytes(BitmapSource image, ScorePage page) => (long)image.PixelWidth * image.PixelHeight * 4 + page.Bytes;

    /// <summary>
    /// The slide's content picture over white and its rows measured once, when staves are found on it; else null (the
    /// render is used). Any failure reading or measuring it gives null and is told to failed.
    /// </summary>
    internal static (BitmapSource Image, ScorePage Page)? ScorePicture(string deckPath, CaptionSlide slide, Action<Exception>? failed = null)
    {
        if (slide.Picture is not { } picture) return null;
        try
        {
            if (CaptionDeck.ReadPicture(deckPath, picture, failed) is not { } image) return null;
            var page = ScoreLayout.Layout(image);
            return page.Rows.Length > 0 ? (image, page) : null;
        }
        catch (Exception error) // whatever goes wrong, the slide falls back to the render
        {
            failed?.Invoke(error);
            return null;
        }
    }

    /// <summary>
    /// The deck's slides whose content picture shows staves (with the picture and its rows while they fit in
    /// <see cref="PictureCache"/>, the rest read when shown) and the deck's row target. A slide whose picture fails is
    /// left out (the render) and told to failed.
    /// </summary>
    internal static (Dictionary<int, (BitmapSource Image, ScorePage Page)?> Slides, RowExtent Target) PrepareScore(string deckPath,
        IReadOnlyList<CaptionSlide> slides, CancellationToken cancellation, Action<int, Exception>? failed = null)
    {
        var found = new Dictionary<int, (BitmapSource Image, ScorePage Page)?>();
        var reach = new List<RowExtent[]>();
        long bytes = 0;
        var gate = new object();
        // A few slides at a time, each read and measured on its own thread; only as many kept as fit (of the others,
        // their reach is all that is needed now).
        int together = Math.Clamp(Environment.ProcessorCount / 2, 1, 4);
        var read = new (BitmapSource Image, ScorePage Page)?[together];
        for (int first = 0; first < slides.Count; first += together)
        {
            int count = Math.Min(together, slides.Count - first);
            Parallel.For(0, count, new ParallelOptions { CancellationToken = cancellation, MaxDegreeOfParallelism = together }, i =>
                read[i] = ScorePicture(deckPath, slides[first + i], error => { lock (gate) failed?.Invoke(first + i + 1, error); }));
            for (int i = 0; i < count; i++)
            {
                if (read[i] is not var (image, page)) continue;
                bytes += Bytes(image, page);
                found[first + i + 1] = bytes <= PictureCache ? (image, page) : null;
                reach.Add(page.Reach);
                read[i] = null;
            }
        }
        return (found, ScoreLayout.Target(reach));
    }

    /// <summary>
    /// The slide's content picture to read its lyrics from (PNG) and its rows as the score row images take them (each
    /// row's own ink, at its own size), so the k-th line read is the k-th row sent; null when the render is used.
    /// </summary>
    internal static (byte[] Png, ScoreRow[] Rows)? OcrPicture(string deckPath, CaptionSlide slide) =>
        ScorePicture(deckPath, slide) is var (image, page) ? OcrPicture(image, page) : null;

    internal static (byte[] Png, ScoreRow[] Rows) OcrPicture(BitmapSource image, ScorePage page) => (Png(image), page.Fit(new RowExtent()));

    private static byte[] Png(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    /// <summary>The running preparation of the followed deck's score pictures (canceled when another deck comes).</summary>
    internal Task ScorePreparation => _scorePrepared;

    // Started right after a deck is read (or when a deck is followed without that, as after a restart); the last
    // deck's preparation, if still running, stops.
    private Task PrepareScoreAsync(CaptionDeck deck)
    {
        StopScorePreparation();
        _preparation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        return _scorePrepared = PrepareScoreCoreAsync(deck, _preparation.Token);
    }

    private void StopScorePreparation()
    {
        _preparation?.Cancel();
        _preparation?.Dispose();
        _preparation = null;
        _preparingScore = null;
    }

    private async Task PrepareScoreCoreAsync(CaptionDeck deck, CancellationToken token)
    {
        _preparingScore = deck;
        _scoreDeck = null;
        _noStaves = null;
        _pictures.Clear();
        var slides = deck.Slides.ToArray();
        var failures = new System.Collections.Concurrent.ConcurrentQueue<(int Slide, Exception Error)>();
        try
        {
            var (found, target) = await Task.Run(() => PrepareScore(deck.Path, slides, token, (n, error) => failures.Enqueue((n, error))), token);
            if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
            foreach (var (slide, error) in failures) _host.WriteError($"read score picture · slide {slide}", error);
            foreach (var (slide, picture) in found) if (picture is var (image, page)) _pictures.Keep((deck.Path, slide), image, page);
            _scoreDeck = (deck, [.. found.Keys], target);
            // The slides whose content picture was read and measured and shows no staff (a photo); not one that failed.
            var failed = failures.Select(f => f.Slide).ToHashSet();
            _noStaves = (deck, [.. Enumerable.Range(1, slides.Length).Where(n => slides[n - 1].Picture != null && !found.ContainsKey(n) && !failed.Contains(n))]);
        }
        catch (OperationCanceledException) { throw; } // another deck came: its own preparation runs
        catch (Exception error) // never left waiting: the whole deck comes from the render
        {
            if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
            _host.WriteError("read score picture", error);
            _scoreDeck = (deck, [], default);
        }
        finally { if (ReferenceEquals(_preparingScore, deck)) _preparingScore = null; }
        UpdateScore();
    }

    private (BitmapSource Image, ScorePage Page)? ContentPicture(CaptionDeck deck, int slide)
    {
        if (slide < deck.Slides.Length) _ = LoadPictureAsync(deck, slide + 1);
        if (_pictures.TryGet((deck.Path, slide), out var picture)) return picture;
        _ = LoadPictureAsync(deck, slide);
        return null;
    }

    private async Task LoadPictureAsync(CaptionDeck deck, int slide)
    {
        var key = (deck.Path, slide);
        if (_closed || _scoreDeck is not { } prepared || !ReferenceEquals(prepared.Deck, deck) || !prepared.Slides.Contains(slide)
            || deck.Slides[slide - 1].Picture is null || _pictures.Contains(key) || !_loadingPictures.Add((deck, slide))) return;
        try
        {
            Exception? failure = null;
            (BitmapSource Image, ScorePage Page)? read;
            var page = deck.Slides[slide - 1];
            try { read = await Task.Run(() => ScorePicture(deck.Path, page, error => failure = error)); }
            catch (Exception error) { (read, failure) = (null, error); }
            // Kept only for the deck it was read for; another deck may have come meanwhile.
            if (_closed || _scoreDeck is not { } current || !ReferenceEquals(current.Deck, deck)) return;
            if (read is not var (image, rows))
            {
                if (failure != null) _host.WriteError($"read score picture · slide {slide}", failure);
                current.Slides.Remove(slide); // the file changed or went away: the render is used
            }
            else _pictures.Keep(key, image, rows);
            if (ReferenceEquals(_captions.Deck, deck) && _captions.CurrentSlide == slide) UpdateScore();
            else if (ReferenceEquals(_captions.Deck, deck) && _captions.CurrentSlide + 1 == slide) UpdateSteps(); // the next preview's row
        }
        finally { _loadingPictures.Remove((deck, slide)); }
    }

    // The lyrics of each score row, per slide, read from the slide pictures.
    private readonly Dictionary<(string Path, int Slide), string[]> _rowLyrics = [];

    // After a deck is read: the lyrics inside its pictures are read in the background (OCR helper) and offered as a box.
    /// <summary>Reads the lyrics in slide pictures (the OCR helper); a test puts a stand-in here.</summary>
    internal Func<IReadOnlyList<string>, CancellationToken, Task<OcrPage[]>>? ReadLyrics { get; set; }

    /// <summary>Where each reading writes its slide pictures for the OCR helper (a test puts another place here).</summary>
    internal string OcrFolder { get; set; } = Path.Combine(Path.GetTempPath(), "Axicultus", "ocr");

    /// <summary>The running reading of the lyrics in the deck's pictures.</summary>
    internal Task LyricsReading { get; private set; } = Task.CompletedTask;

    /// <summary>The songs the read lyrics are put right from (the song database, read only); a test puts a stand-in here.</summary>
    internal Func<CancellationToken, Task<IReadOnlyList<SongLyrics.Song>>>? Songs { get; set; }

    // The song database's songs, read once per version of the database file (its write time, and its write-ahead log's
    // when there is one). A version that cannot be used (a missing file, a schema the app cannot read) is not read
    // again; one that was busy or locked is tried again with the next deck. Each version's failure is told once.
    private (string Key, Task<IReadOnlyList<SongLyrics.Song>> Songs)? _songs;
    private string? _songsUnusable, _songsTold;

    private Task<IReadOnlyList<SongLyrics.Song>> ReadSongsAsync(CancellationToken cancellation)
    {
        if (_host.SongDatabase is not { } database) return Task.FromResult<IReadOnlyList<SongLyrics.Song>>([]);
        string path = database.DatabasePath;
        if (path.Length == 0) return Task.FromResult<IReadOnlyList<SongLyrics.Song>>([]);
        string wal = path + "-wal";
        string key = $"{path}|{File.GetLastWriteTimeUtc(path).Ticks}|{(File.Exists(wal) ? File.GetLastWriteTimeUtc(wal).Ticks : 0)}";
        if (key == _songsUnusable) return Task.FromResult<IReadOnlyList<SongLyrics.Song>>([]);
        if (_songs is { } kept && kept.Key == key && !kept.Songs.IsFaulted && !kept.Songs.IsCanceled) return kept.Songs;
        var songs = Task.Run(async () => (IReadOnlyList<SongLyrics.Song>)[.. (await database.ReadAsync(path).ConfigureAwait(false))
            .Where(song => song.FolderNo > 0).Select(song => new SongLyrics.Song(song.Title, song.AlternateTitle, song.Lyrics))], cancellation);
        _songs = (key, songs);
        return songs;
    }

    // A failure reading the song database that may pass (the file busy, locked or in use).
    private bool Passing(Exception error) => _host.SongDatabase?.IsBusy(error) == true
        || error is TimeoutException || error is IOException and not (FileNotFoundException or DirectoryNotFoundException);

    // The read lyrics (per slide, per row) with misread words put right from the deck's song in the song database, off
    // the UI thread; as read when nothing was read, no song is sure to be the deck's, or anything fails.
    private async Task<string[][]> CorrectLyricsAsync(CaptionDeck deck, string[][] slides)
    {
        if (!slides.Any(rows => rows.Any(row => row.Any(c => c is >= '가' and <= '힣')))) return slides; // nothing to put right
        var lifetime = _lifetime.Token; // (taken now: closing during the reading is no failure)
        string label = DeckLabel.Text;
        DeckLabel.Text = CaptionText.Format("CaptionConsole", "CorrectingLyrics", Path.GetFileName(deck.Path));
        try
        {
            var songs = await (Songs ?? ReadSongsAsync)(lifetime);
            return songs.Count == 0 ? slides : await Task.Run(() => SongLyrics.Correct(deck.Path, slides, songs), lifetime);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // (closing, or a deck the operator already left, is not told)
            if (_closed || lifetime.IsCancellationRequested || !ReferenceEquals(_captions.Deck, deck)) return slides;
            string? failed = _songs is { Songs.IsFaulted: true } faulted ? faulted.Key : null;
            if (failed != null && !Passing(error.GetBaseException())) _songsUnusable = failed; // that version is not read again
            if (failed == null || failed != _songsTold) _host.WriteError("correct lyrics from the song database", error);
            _songsTold = failed ?? _songsTold;
            return slides;
        }
        catch (OperationCanceledException) when (_closed || lifetime.IsCancellationRequested)
        {
            return slides;
        }
        finally
        {
            if (!_closed && ReferenceEquals(_captions.Deck, deck)) DeckLabel.Text = label;
        }
    }

    // offer: the recognized lyrics may become the chosen box (a newly read deck); a restored deck keeps its choice.
    private async Task RecognizeDeckAsync(CaptionDeck deck, bool offer = true)
    {
        var slides = Enumerable.Range(1, deck.Slides.Length).Where(n => deck.Slides[n - 1].HasPicture != false).ToArray();
        if (slides.Length == 0) return; // (without a renderer only the slides with a content picture are read)
        var read = ReadLyrics ?? (LyricsReader.Available ? LyricsReader.ReadAsync : null);
        if (read is null) { Status.Text = CaptionText.Format("CaptionConsole", "NoOcr", "ocr"); return; }
        // Ended when the caption leaves the deck (StartScore): the reading starts no further batch (a048 design 9).
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _lyricsStop = stop;
        // The deck's reading state with the slides whose lyrics could not be read, out of all those to be read (design
        // 10, 11): the deck's label, which the score row on air does not write.
        string Unread(string label, int unread) =>
            unread > 0 ? label + " · " + CaptionText.Format("CaptionConsole", "LyricsPartlyRead", slides.Length, unread) : label;
        var folder = Path.Combine(OcrFolder, Guid.NewGuid().ToString("N"));
        try
        {
            // (inside: a folder that cannot be made is a read failure, and the finally leaves no stopped reading behind)
            Directory.CreateDirectory(folder);
            var files = new List<string>();
            var layouts = new Dictionary<string, (int Slide, ScoreRow[] Rows)>(StringComparer.OrdinalIgnoreCase);
            // The score's preparation reads and measures the content pictures; they are read from what it kept.
            // (a failed preparation is told and the renders are read; another deck coming stops this reading)
            if (deck.Slides.Any(s => s.Picture != null))
                try { await _scorePrepared; }
                catch (OperationCanceledException) { if (_closed || !ReferenceEquals(_captions.Deck, deck)) return; }
                catch (Exception error) { _host.WriteError("read score picture", error); }
            foreach (var slide in slides)
            {
                if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
                DeckLabel.Text = CaptionText.Format("CaptionConsole", "ReadingLyrics", Path.GetFileName(deck.Path), slide, deck.Slides.Length);
                // The content picture the score rows are cut from, when it shows staves; else the slide's render.
                var page = deck.Slides[slide - 1];
                (byte[] Png, ScoreRow[] Rows)? picture = null;
                if (_scoreDeck is { } prepared && ReferenceEquals(prepared.Deck, deck) && prepared.Slides.Contains(slide))
                    picture = _pictures.TryGet((deck.Path, slide), out var kept) ? await Task.Run(() => OcrPicture(kept.Image, kept.Page), _lifetime.Token)
                        : await Task.Run(() => OcrPicture(deck.Path, page), _lifetime.Token);
                string file;
                if (picture is var (png, rows))
                {
                    file = Path.Combine(folder, $"{slide}.png");
                    await File.WriteAllBytesAsync(file, png, _lifetime.Token);
                    layouts[file] = (slide, rows);
                }
                else
                {
                    if (await RenderSlideAsync(deck.Path, slide) is not var (image, bytes, type)) continue;
                    file = Path.Combine(folder, $"{slide}{(type.Contains("jp", StringComparison.OrdinalIgnoreCase) ? ".jpg" : ".png")}");
                    await File.WriteAllBytesAsync(file, bytes, _lifetime.Token);
                    // A slide whose content picture was measured and shows no staff is a photo, whatever lines its render
                    // seems to have (a roof's planks read as a staff): its lines are read as they are, under no staff. When that
                    // could not be told (no preparation, a picture that failed to read) the render's rows are used.
                    bool photo = _noStaves is { } measured && ReferenceEquals(measured.Deck, deck) && measured.Slides.Contains(slide);
                    layouts[file] = (slide, photo ? [] : ScoreLayout.FindRows(image));
                }
                files.Add(file);
            }
            if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
            if (files.Count == 0) { DeckLabel.Text = Unread(Path.GetFileName(deck.Path), slides.Length); return; } // (no picture made)
            var pages = await read(files, stop.Token);
            // Why slides could not be read, once per reading and reason (a failed batch's exit code and error text, its
            // time limit): the error log keeps it, the debug output does not in a release build (design 19).
            foreach (var reason in pages.Select(page => page.Error).OfType<string>().Distinct())
                _host.WriteError("read lyrics in pictures", new IOException(reason));
            if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
            var readPages = pages.Where(page => page.Error == null && layouts.ContainsKey(page.File))
                .DistinctBy(page => page.File, StringComparer.OrdinalIgnoreCase) // (a slide read twice is read once)
                .Select(page => (Layout: layouts[page.File], Rows: LyricsReader.RowLyrics(page, layouts[page.File].Rows))).ToList();
            var corrected = await CorrectLyricsAsync(deck, [.. readPages.Select(page => page.Rows)]);
            if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
            using var together = _captions.Hold(); // the offer, the whole texts and the row's line go out as one caption
            int found = 0;
            var scores = new List<(int Slide, string Lyrics)>();
            for (int k = 0; k < readPages.Count; k++)
            {
                var (layout, rows) = (readPages[k].Layout, corrected[k]);
                _rowLyrics[(deck.Path, layout.Slide)] = rows;
                var lyrics = string.Join("\n", rows.Where(row => row.Length > 0));
                if (lyrics.Length > 0) found++;
                _captions.AddRecognized(layout.Slide, lyrics);
                if (layout.Rows.Length > 0 && lyrics.Length > 0) scores.Add((layout.Slide, lyrics)); // read under staves
            }
            for (int n = 1; n <= deck.Slides.Length; n++)
                if (deck.Slides[n - 1].Boxes.All(box => box.Key != CaptionSession.RecognizedKey)) _captions.AddRecognized(n, "");
            // (unread: a picture not made, a failed batch, or a page the helper could not read)
            DeckLabel.Text = Unread(found > 0 ? CaptionText.Format("CaptionConsole", "LyricsFound", Path.GetFileName(deck.Path), found)
                : Path.GetFileName(deck.Path), slides.Length - readPages.Count);
            if (found > 0 && offer) OfferRecognizedLyrics(deck);
            // Then (the choice above looks at the whole text the slides have) a score slide's whole text that says little
            // (a page mark) becomes the lyrics read under its staves.
            foreach (var (slide, lyrics) in scores) _captions.AddRecognized(slide, lyrics, staves: true);
            UpdateScore(); // the row on air gets its line of the lyrics (the row stays)
            ListBoxes(keepEdit: true); // the recognized lyrics box in the list, the slide viewed and an edit kept
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested && !_lifetime.IsCancellationRequested) { } // (the deck was left)
        catch (Exception error) when (error is IOException or System.ComponentModel.Win32Exception or System.Text.Json.JsonException
            or InvalidOperationException or UnauthorizedAccessException or OperationCanceledException or TimeoutException)
        {
            if (_closed) return;
            _host.WriteError("read lyrics in pictures", error);
            if (!ReferenceEquals(_captions.Deck, deck)) return; // (a deck left: the next deck's label and status are its own, design 17)
            // Nothing read: every slide to be read is told unread on the deck's label, which the score row on air does
            // not write (design 14); the PC without Korean OCR has its own notice.
            DeckLabel.Text = error is LyricsReader.NoKoreanOcrException ? Path.GetFileName(deck.Path) : Unread(Path.GetFileName(deck.Path), slides.Length);
            Status.Text = CaptionText.Format("CaptionConsole", "LyricsReadFailed", error.Message);
        }
        finally
        {
            if (_lyricsStop == stop) _lyricsStop = null;
            try { Directory.Delete(folder, true); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(error.Message); }
        }
    }

    // The running reading of the lyrics in the caption deck's pictures, ended when another deck comes.
    private CancellationTokenSource? _lyricsStop;

    // The recognized lyrics become the caption when the chosen box says the same on every slide
    // (a verse button, a title) or there is none; a box with real per-slide text stays.
    private void OfferRecognizedLyrics(CaptionDeck deck)
    {
        var key = _chosenKey;
        // (empty on every slide, or the same on two slides or more; one slide's typed text is no verse button, unless it
        // says little (a title) and what was read reads as lyrics)
        var texts = deck.Slides.Select(s => s.Boxes.FirstOrDefault(b => b.Key == key)?.Text ?? "").ToList();
        bool sameEverywhere = key != CaptionSession.RecognizedKey &&
            (texts.All(text => text.Length == 0) || texts.Count >= 2 && texts.Distinct().Count() == 1
                || texts.Count == 1 && CaptionDeck.ShortText(texts[0])
                    && CaptionSession.Lyrical(deck.Slides[0].Boxes.FirstOrDefault(b => b.Key == CaptionSession.RecognizedKey)?.Text ?? ""));
        RefreshBoxes(sameEverywhere ? CaptionSession.RecognizedKey : key);
        if (sameEverywhere) Status.Text = CaptionText.Get("CaptionConsole", "RecognizedChosen");
    }

    // The list again with the recognized lyrics box in it, key chosen for the operator: the caption text changes, the
    // score row on air and a caption being edited stay.
    private void RefreshBoxes(string key)
    {
        if (key != _chosenKey)
        {
            _chosenKey = key;
            _captions.ChangeBox(key);
            Changed();
        }
        ListBoxes(keepEdit: true);
        UpdateSteps();
    }

    // The rows of the last slide picture (a picture is never changed once rendered), as one snapshot: CropRow runs on
    // whichever thread owns the picture, so the cache is replaced whole, never changed in place.
    private sealed record RowCache(BitmapSource Image, RowExtent? Target, ScoreRow[] Rows);
    private static RowCache? _rowCache;

    /// <summary>
    /// The row-th row of the slide (the staves found on it), or its top or bottom half when none is found.
    /// target: the deck's (a content picture), so its rows all show the staves the same size, white making up what
    /// the next row or the picture's edge leaves short; null: the slide's own.
    /// A slide picture may belong to a render thread; the work runs there and the result is a frozen copy.
    /// </summary>
    internal static (BitmapSource Image, int Rows, bool Found)? CropRow(BitmapSource image, int row, RowExtent? target = null) => Crop(image, row, () =>
    {
        var cache = Volatile.Read(ref _rowCache);
        if (cache == null || !ReferenceEquals(cache.Image, image) || cache.Target != target)
            Volatile.Write(ref _rowCache, cache = new(image, target, ScoreLayout.FindRows(image, target)));
        return cache.Rows;
    });

    /// <summary>
    /// The row-th of the rows measured before (a content picture's, fitted to the deck, the row-th with its marks):
    /// nothing is measured here.
    /// </summary>
    internal static (BitmapSource Image, int Rows, bool Found)? CropRow(BitmapSource image, int row, ScoreRow[] rows) => Crop(image, row, () => rows);

    private static (BitmapSource Image, int Rows, bool Found)? Crop(BitmapSource image, int row, Func<ScoreRow[]> measured)
    {
        (BitmapSource, int, bool) Make()
        {
            var staves = measured();
            bool found = staves.Length > 0;
            int half = image.PixelHeight / 2;
            ScoreRow[] rows = found ? staves :
                [new(new(0, 0, image.PixelWidth, half), 0, 0), new(new(0, image.PixelHeight - half, image.PixelWidth, half), 0, 0)];
            var chosen = rows[Math.Clamp(row, 0, rows.Length - 1)];
            var cut = new CroppedBitmap(image, chosen.Area);
            BitmapSource copy = chosen.PadTop + chosen.PadBottom > 0 || chosen.Erase != null ? Padded(cut, chosen.PadTop, chosen.PadBottom, chosen.Erase) : new WriteableBitmap(cut);
            copy.Freeze();
            return (copy, rows.Length, found);
        }
        try
        {
            return image.Dispatcher is not { } owner || owner.CheckAccess() ? Make()
                : owner.Invoke(Make, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromSeconds(2));
        }
        catch (Exception error) when (error is InvalidOperationException or TimeoutException or TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine(error.Message);
            return null;
        }
    }

    // The row with white above and below it (content pictures are composited over white), the ink of other rows painted over.
    private static BitmapSource Padded(BitmapSource cut, int top, int bottom, byte[]? erase)
    {
        var colour = new FormatConvertedBitmap(cut, System.Windows.Media.PixelFormats.Bgr32, null, 0);
        int width = colour.PixelWidth, height = colour.PixelHeight, stride = width * 4;
        var pixels = new byte[stride * (top + height + bottom)];
        Array.Fill(pixels, (byte)255);
        colour.CopyPixels(pixels, stride, top * stride);
        // What is painted over takes the paper around it in its own pixel row (the middle of the paper within 16 pixels
        // on either side, else of the whole row), so a tinted or shaded picture shows no patches; white when there is none.
        for (int y = 0; erase != null && y < height; y++)
        {
            int row = (top + y) * stride, line = y * width;
            byte[] marks = erase;
            int Middle(IEnumerable<int> columns)
            {
                var paper = columns.Where(n => n >= 0 && n < width && marks[line + n] == ScoreLayout.Paper)
                    .OrderBy(n => pixels[row + n * 4] + pixels[row + n * 4 + 1] + pixels[row + n * 4 + 2]).ToList();
                return paper.Count == 0 ? -1 : row + paper[paper.Count / 2] * 4;
            }
            for (int x = 0; x < width;)
            {
                if (marks[line + x] != ScoreLayout.Erased) { x++; continue; }
                int start = x;
                while (x < width && marks[line + x] == ScoreLayout.Erased) x++;
                int from = Middle(Enumerable.Range(start - 16, 16).Concat(Enumerable.Range(x, 16)));
                if (from < 0) from = Middle(Enumerable.Range(0, width));
                for (int n = start; n < x; n++)
                    for (int c = 0; c < 3; c++) pixels[row + n * 4 + c] = from < 0 ? (byte)255 : pixels[from + c];
            }
        }
        return BitmapSource.Create(width, top + height + bottom, 96, 96, System.Windows.Media.PixelFormats.Bgr32, null, pixels, stride);
    }

    private void PickColor(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string color }) _captions.SetStyle(_captions.Style with { Color = color });
    }

    // The colour in use has the accent ring (none of them for a colour picked with 직접 선택).
    private void ShowChosenColor()
    {
        foreach (var swatch in Swatches.Children.OfType<Button>())
        {
            if (swatch.Tag is not string color) continue;
            bool chosen = string.Equals(color, _captions.Style.Color, StringComparison.OrdinalIgnoreCase);
            swatch.SetResourceReference(Control.BorderBrushProperty, chosen ? "Brush.Accent.Primary" : "Brush.Border.Strong");
            swatch.BorderThickness = new Thickness(chosen ? 3 : 2);
            System.Windows.Automation.AutomationProperties.SetItemStatus(swatch, chosen ? CaptionText.Get("CaptionConsole", "Selected") : "");
        }
    }

    private void PickCustomColor(object sender, RoutedEventArgs e)
    {
        var current = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_captions.Style.Color);
        using var dialog = new System.Windows.Forms.ColorDialog { Color = System.Drawing.Color.FromArgb(current.R, current.G, current.B), FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            _captions.SetStyle(_captions.Style with { Color = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}" });
    }

    private void BoxChanged(object sender, RoutedEventArgs e) => _captions.SetStyle(_captions.Style with { Box = BoxCheck.IsChecked == true });

    private void CopyBrowserUrl(object sender, RoutedEventArgs e)
    {
        try { Clipboard.SetText(CaptionBroadcastServer.Url); Told = CaptionText.Get("CaptionConsole", "UrlCopied"); }
        catch (System.Runtime.InteropServices.ExternalException ex) { Told = CaptionText.Format("CaptionConsole", "CopyFailed", ex.Message); }
    }

    private void StartAutomatically(object sender, RoutedEventArgs e)
    {
        Loaded -= StartAutomatically;
        // The console opens following the screen (chosen before it loaded, so started here as a pick of it would be).
        if (ScreenMode.IsChecked == true)
        {
            _captions.Mirror(true);
            _ = SyncScreenAsync();
        }
        StartWhenUsed();
    }

    private bool _started;

    // The tab is there from the app's start, behind the live tab. The caption server starts and OBS is connected with
    // the saved settings when the captions are first used: the tab seen, or the caption put on air from the live tab.
    private async void StartWhenUsed()
    {
        if (_started || _closed || !IsLoaded || !(IsVisible || _captions.Visible)) return;
        _started = true;
        await StartServerAsync();
        if (_obs.Connection == null && (_obs.HasSavedPassword || _obs.AutoConnect || File.Exists(_obs.ProfilePath))) await ConnectAsync();
        else if (_obs.Connection == null) ObsLabel.Text = CaptionText.Get("CaptionConsole", "ObsConnectInSettings");
    }

    private async Task StartServerAsync()
    {
        try { await _server.StartAsync(); ServerLabel.Text = CaptionText.Get("CaptionConsole", "ServerRunning"); }
        catch (Exception ex) { ServerLabel.Text = CaptionText.Format("CaptionConsole", "ServerFailed", ex.GetBaseException().Message); }
    }

    // The live deck first; then the deck in the output panel (show not live) and, if asked, the selected one.
    private (string Path, int Slide)? FindCurrentDeck(bool includeSelected)
    {
        var live = _live.Current;
        if (live.PptPath is { Length: > 0 } path) return (path, live.PptSlideNumber);
        // Not live: the deck in the output panel is what the output screen holds, at the slide the panel shows; the item's
        // own slide while the panel is empty or still holds the last deck (its slide number is that deck's).
        if (live.State == CaptionLiveState.Off && _host.OutputItem is { } item && IsDeck(item))
        {
            var panel = OutputPanel()!;
            bool lastDeck = panel.Path is { } drawn && !SamePath(drawn, item.PptPath!);
            return (item.PptPath!, Math.Max(1, lastDeck || panel.Slide < 1 ? item.PptSlide : panel.Slide));
        }
        // The picked deck at the slide the preview column shows.
        if (includeSelected && _host.Picked is { } picked && IsDeck(picked)) return (picked.PptPath!, Math.Max(1, _host.PreviewPanel?.Slide ?? 0));
        return null;
    }

    internal async Task<bool> PrepareDeckAsync(string path, int slide)
    {
        if (_editor.IsRestoring) return false;
        long request = ++_deckRequest;
        CaptionDeck deck;
        try
        {
            deck = await ReadDeckAsync(path);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return false; }
        catch (Exception ex) when (IsDeckReadError(ex))
        {
            _host.WriteError("open caption PPT", ex);
            if (!_closed && request == _deckRequest) Status.Text = CaptionText.Format("CaptionConsole", "DeckReadFailed", ex.Message);
            return false;
        }
        if (_closed || request != _deckRequest) return false;
        _captions.Prepare(deck);
        StartScore(deck);
        ShowDeck(deck, slide, CaptionDeck.AllTextKey); // a newly read deck starts on the slide's whole text
        Status.Text = CaptionText.Format("CaptionConsole", "DeckRead", Path.GetFileName(deck.Path));
        Changed();
        LyricsReading = RecognizeDeckAsync(deck);
        return true;
    }

    // The last deck's score row is not this one's; this deck's pictures are read now, before a row is asked for.
    private void StartScore(CaptionDeck deck)
    {
        _captions.SetImage(null);
        StopScorePreparation();
        _lyricsStop?.Cancel(); // the last deck's lyrics are not read on
        _scoreDeck = null;
        _noStaves = null;
        _pictures.Clear();
        _rowThumbs.Clear();
        if (deck.Slides.Any(s => s.Picture != null)) _ = PrepareScoreAsync(deck);
    }

    private static bool IsFileError(Exception error) => error is
        IOException or InvalidDataException or UnauthorizedAccessException or System.Text.Json.JsonException;

    private static bool IsDeckReadError(Exception error) => IsFileError(error) || error is System.Xml.XmlException
        or FormatException or InvalidOperationException or ArgumentException or NotSupportedException or TimeoutException
        or Win32Exception or CaptionRenderException;

    private int SelectedSlide => Slides.SelectedItem is int slide ? slide : 1;
    private string SelectedKey => (Boxes.SelectedItem as CaptionBox)?.Key ?? "";

    // The box the operator chose (or a newly read deck's whole text). A slide without it shows its whole text in the
    // list, but the choice stays for the next slide that has it; only the operator's pick in the list changes it.
    private string _chosenKey = CaptionDeck.AllTextKey;
    private bool _listing; // the list is being filled for a slide: its selection is no choice of the operator

    private void ShowDeck(CaptionDeck deck, int slide, string key)
    {
        _chosenKey = key;
        DeckLabel.Text = Path.GetFileName(deck.Path);
        Slides.ItemsSource = Enumerable.Range(1, deck.Slides.Length);
        Slides.SelectedItem = Math.Clamp(slide, 1, deck.Slides.Length);
        ListBoxes();
    }

    private void SlideSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_captions.Deck is not {} deck || SelectedSlide > deck.Slides.Length) return;
        ListBoxes();
        if (IsLoaded) Changed();
    }

    // The selected slide's boxes, the chosen one selected; on a slide without it none is (its whole text is the
    // caption), so picking the whole text there is a choice too.
    private void ListBoxes(bool keepEdit = false)
    {
        if (_captions.Deck is not { } deck || SelectedSlide > deck.Slides.Length) return;
        var boxes = deck.Slides[SelectedSlide - 1].Boxes;
        _listing = true;
        try
        {
            Boxes.ItemsSource = boxes;
            Boxes.SelectedItem = boxes.FirstOrDefault(b => b.Key == _chosenKey);
        }
        finally { _listing = false; }
        RefreshCue(keepEdit);
    }

    private void BoxSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_listing) return; // the list is filled for a slide; it shows the cue itself
        // (the list emptied for a moment is not a choice of "no box")
        if (IsLoaded && Boxes.SelectedItem != null) Choose(SelectedKey);
        RefreshCue();
    }

    // The operator's pick: the caption text changes; the slide viewed and the score row on air stay.
    private void Choose(string key)
    {
        _chosenKey = key;
        _captions.ChangeBox(key);
        Changed();
        UpdateSteps(); // the steps show the chosen box
    }

    private string _cueShown = "";
    // The cue holds what the operator typed and has not saved.
    private bool CueEdited => Cue.Text != _cueShown;

    // The selected slide's caption in the cue. keepEdit: not over what the operator typed there and has not saved.
    private void RefreshCue(bool keepEdit = false)
    {
        if (keepEdit && CueEdited) return;
        Cue.Text = _cueShown = _captions.TextFor(SelectedSlide, _chosenKey);
    }
    private void SaveOverride(object sender, RoutedEventArgs e)
    {
        if (_captions.Deck == null) return;
        _captions.Overrides[SelectedSlide] = Cue.Text;
        _cueShown = Cue.Text;
        Told = CaptionText.Format("CaptionConsole", "CueSaved", SelectedSlide);
        Changed();
        UpdateSteps(); // the step shows the saved words
    }

    private void Previous(object sender, RoutedEventArgs e) => Step(-1);
    private void Next(object sender, RoutedEventArgs e) => Step(1);

    private void LinesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LinesBox.SelectedIndex >= 0) _captions.CaptionLines = LinesBox.SelectedIndex + 1;
    }

    private void ScoreSizeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ScoreSizeBox.SelectedIndex >= 0) _captions.ScoreSize = ScoreSizeBox.SelectedIndex;
    }

    private async void Step(int change)
    {
        // A score goes row by row, then to the next slide's first row.
        int next = _scoreRow + change;
        if (_captions.Following && _rowShown && next >= 0 && next < _scoreRows) { SetScoreRow(next); return; }
        if (_captions.SyncShow)
        {
            // Sync: the output screen turns (a slide, a page of a song or passage) and the caption follows it.
            int direction = change > 0 ? 1 : -1;
            if (!_host.CanStepOutput(direction)) { Status.Text = CaptionText.Get("CaptionConsole", change > 0 ? "NoNextSlide" : "NoPreviousSlide"); return; }
            await _host.StepOutputAsync(direction);
            return;
        }
        if (!_captions.Following) { Status.Text = CaptionText.Get("CaptionConsole", "SyncOff"); return; }
        _captions.MoveToSlide(_captions.CurrentSlide + change);
        Changed();
    }

    private async void ConnectObs(object sender, RoutedEventArgs e) => await ConnectAsync();

    private async Task ConnectAsync()
    {
        if (!int.TryParse(Port.Text, out int port) || port is < 1 or > 65535) { ObsLabel.Text = Told = CaptionText.Get("CaptionConsole", "CheckPort"); return; }
        try
        {
            await _obs.ConfigureAsync(Host.Text, port, Password.Password, AutoObs.IsChecked == true,
                [Scene1.Text, Scene2.Text, Scene3.Text, Scene4.Text], CameraSource.Text);
            Password.Clear();
            SavedPassword.Visibility = _obs.HasSavedPassword ? Visibility.Visible : Visibility.Collapsed;
            await StartServerAsync();
        }
        catch (Exception ex) { Told = ex.Message; }
    }

    private void ShowObsStatus(ObsStatus status)
    {
        // (most messages already name OBS: "OBS 연결 중", not "OBS OBS 연결 중")
        string message = status.Message.StartsWith("OBS", StringComparison.Ordinal) ? status.Message : "OBS " + status.Message;
        ObsLabel.Text = (status.Connected ? "● " : "") + message + (status.Scene.Length > 0 ? " · " + status.Scene : "");
        if (Scenes.Items.Cast<string>().SequenceEqual(status.Scenes)) return;
        Scenes.ItemsSource = status.Scenes;
        Scenes.SelectedItem = status.Scene;
    }

    private async void AddCaptionSource(object sender, RoutedEventArgs e)
    {
        // The caption server listens on this PC only, so OBS must run here too.
        if (_obs.Host is not (ObsOperator.LocalHost or "localhost"))
        {
            Told = CaptionText.Get("CaptionConsole", "SourceLocalOnly");
            return;
        }
        if (_obs.Connection is not { } connection) { Told = CaptionText.Get("CaptionConsole", "ConnectFirst"); return; }
        AddObsSource.IsEnabled = false;
        try
        {
            await StartServerAsync();
            var (scene, created) = await connection.AddCaptionSourceAsync(CaptionBroadcastServer.Url);
            Told = CaptionText.Format("CaptionConsole", created ? "SourceAdded" : "SourcePlaced", scene, ObsConnection.CaptionSourceName);
        }
        catch (Exception ex) { Told = CaptionText.Format("CaptionConsole", "SourceAddFailed", ex.Message); }
        finally { AddObsSource.IsEnabled = true; }
    }

    private async void SwitchScene(object sender, RoutedEventArgs e)
    {
        if (_obs.Connection is not { } connection || Scenes.SelectedItem is not string scene) return;
        try { await connection.SwitchAsync(scene, SourceName.Text.Trim(), EnableSource.IsChecked == true); }
        catch (Exception ex) { Told = ex.Message; }
    }

    private async void Finish()
    {
        _closed = true;
        _stopWatchingFonts?.Invoke();
        _compositionCommit?.Abort();
        _editor.SaveRequested -= Changed;
        _lifetime.Cancel();
        _lifetime.Dispose();
        _debounce.Stop();
        _previewTimer.Stop();
        _previewTimer.Tick -= ReadObsPreview;
        _camera.PropertyChanged -= CameraChanged;
        if (_camera.ObsOwnsCamera) _camera.Preview = null;
        if (_recovery != null) _recovery.Replacing -= BeforeQueueReplacement;
        _camera.ReleaseLocalCamera -= OnCameraRelease;
        _host.Changed -= HostChanged;
        _captions.PropertyChanged -= CaptionChanged;
        _live.Changed -= LiveChanged;
        if (!Settings.IsClosed) Settings.Close();
        // The last write runs on a pool thread. Finish before the process can exit.
        _obs.Changed -= ShowObsStatus;
        await SaveAsync();
    }
}

/// <summary>
/// The followed deck's content pictures and their measured rows, kept up to capacity bytes: the least recently used go
/// first, never the slide on air nor the one being kept.
/// </summary>
internal sealed class ScorePictures(long capacity)
{
    private readonly Dictionary<(string Path, int Slide), (BitmapSource Image, ScorePage Page, long Used)> _kept = [];
    private long _clock;

    /// <summary>The slide whose row is on air: it is not dropped.</summary>
    public (string Path, int Slide)? OnAir { get; set; }

    public int Count => _kept.Count;

    public bool Contains((string Path, int Slide) key) => _kept.ContainsKey(key);

    public bool TryGet((string Path, int Slide) key, out (BitmapSource Image, ScorePage Page) picture)
    {
        if (!_kept.TryGetValue(key, out var kept)) { picture = default; return false; }
        _kept[key] = kept with { Used = ++_clock };
        picture = (kept.Image, kept.Page);
        return true;
    }

    public void Keep((string Path, int Slide) key, BitmapSource image, ScorePage page)
    {
        _kept.Remove(key);
        long size = CaptionConsole.Bytes(image, page);
        while (_kept.Count > 0 && _kept.Values.Sum(kept => CaptionConsole.Bytes(kept.Image, kept.Page)) + size > capacity)
        {
            var oldest = _kept.Where(kept => !kept.Key.Equals(OnAir)).OrderBy(kept => kept.Value.Used).Select(kept => ((string, int)?)kept.Key).FirstOrDefault();
            if (oldest is not { } drop) break;
            _kept.Remove(drop);
        }
        _kept[key] = (image, page, ++_clock);
    }

    public void Clear()
    {
        _kept.Clear();
        OnAir = null;
    }
}
