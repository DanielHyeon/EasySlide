using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Axicultus.Wpf.Rendering;
using CommunityToolkit.Mvvm.ComponentModel;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Captions;

/// <summary>Image is a JPEG data URL of a score half shown instead of the text.</summary>
public sealed record PublishedCaption(long Revision, string Text, string Color = CaptionStyle.DefaultColor, bool Box = true, string? Image = null, int Lines = 3,
    double ScoreHeight = .27);
public sealed record CaptionTake(DateTimeOffset Time, string Text);
/// <summary>
/// The caption console's kept choices: look and destinations (Camera is the output screen's camera caption, Obs
/// the browser source), Sync: the console's next/previous also turn the output screen, Lines: the fixed
/// height of the caption area (1 to 3 lines), and Preview: the height the operator gave the caption tab's camera
/// preview (0: the column's width at 16:9).
/// </summary>
public sealed record CaptionStyle(string Color = CaptionStyle.DefaultColor, bool Box = true, bool Camera = true, bool Obs = true, bool Sync = false, int Lines = 3,
    int Score = 1, string Display = CaptionStyle.AutoDisplay, double Preview = 0)
{
    public const string DefaultColor = "#FFFFFF";
    /// <summary>The dark box behind the text on the camera screen (the OBS page draws the same).</summary>
    public const string BoxColor = "#CC101010";
    /// <summary>Display: auto (per slide, the score where its content picture shows staves, else the text), text, score, scoretext.</summary>
    public const string AutoDisplay = "auto";
    public static CaptionStyle Parse(string color, bool box, bool camera = true, bool obs = true, bool sync = false, int lines = 3, int score = 1,
        string? display = null, double preview = 0) =>
        new(System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$") ? color.ToUpperInvariant() : DefaultColor, box, camera, obs, sync,
            Math.Clamp(lines, 1, 3), Math.Clamp(score, 0, 2), ParseDisplay(display), double.IsFinite(preview) && preview > 0 ? preview : 0);

    public static string ParseDisplay(string? display) => display is "text" or "score" or "scoretext" ? display : AutoDisplay;

    /// <summary>Most of the screen height a score row may take: 작게, 보통, 크게.</summary>
    public static double ScoreHeight(int score) => score switch { 0 => .18, 2 => .42, _ => .27 };
}

/// <summary>All mutations run on the operator Dispatcher. Remote observers receive only published text.</summary>
public sealed class CaptionSession : ObservableObject, IDisposable
{
    private readonly ILiveSessionService _live;
    private string _text = "";
    private bool _visible;
    private LiveState _previousState;
    private string? _lastSlidePath;
    private int _lastSlideNumber;
    private readonly IPowerPointSlideShowControl? _show;
    private readonly DispatcherTimer? _poll;
    private bool _polling;
    private int? _showSlide; // the native show's slide when last seen
    private long _generation;

    private readonly string? _stylePath;
    private BitmapSource? _image;
    private string? _imageUrl;

    public CaptionSession(ILiveSessionService live, IPowerPointSlideShowControl? show = null, string? stylePath = null)
    {
        _live = live;
        _show = show;
        _stylePath = stylePath;
        try
        {
            if (stylePath != null && File.Exists(stylePath) &&
                JsonSerializer.Deserialize<CaptionStyle>(File.ReadAllText(stylePath)) is { } saved)
                Style = CaptionStyle.Parse(saved.Color, saved.Box, saved.Camera, saved.Obs, saved.Sync, saved.Lines, saved.Score, saved.Display, saved.Preview);
        }
        catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException) { }
        if (show != null)
        {
            _poll = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(400) };
            _poll.Tick += PollNativeSlide;
        }
        _previousState = live.Current.State;
        live.SessionChanged += OnLiveChanged;
    }

    public CaptionDeck? Deck { get; private set; }
    public Dictionary<int, string> Overrides { get; } = [];
    public List<CaptionTake> History { get; } = [];
    public int CurrentSlide { get; private set; } = 1;
    public string BoxKey { get; private set; } = "";
    public bool Following { get; private set; }
    public bool Visible => _visible;
    /// <summary>The output screen is blacked out or on standby: a caption chosen on air does not go out (a023).</summary>
    public bool ScreenOff => _live.Current.State is LiveState.Hidden or LiveState.Standby;
    public PublishedCaption Published { get; private set; } = new(0, "");
    public string PublicText => Published.Text;
    public ImageSource? PublicImage { get; private set; }
    public bool HasText => PublicText.Length > 0 || PublicImage != null;
    public bool HasPublicText => PublicText.Length > 0;
    /// <summary>The text and picture that would be published if the caption were on air (only shown by the console).</summary>
    public string ReadyText { get; private set; } = "";
    public ImageSource? ReadyImage { get; private set; }
    public CaptionStyle Style { get; private set; } = new();
    public Brush CaptionForeground => Look().Foreground;
    public Brush CaptionBackground => Look().Background;
    // The brushes of the current look, made once per look (one object until it changes, so the console sees no change).
    private (string Color, bool Box, Brush? Foreground, Brush? Background) _look;
    private (Brush Foreground, Brush Background) Look()
    {
        if (_look.Foreground == null || _look.Color != Style.Color || _look.Box != Style.Box)
            _look = (Style.Color, Style.Box, Frozen((Color)ColorConverter.ConvertFromString(Style.Color)),
                Style.Box ? Frozen((Color)ColorConverter.ConvertFromString(CaptionStyle.BoxColor)) : Brushes.Transparent);
        return (_look.Foreground!, _look.Background!);
    }
    private static Brush Frozen(Color color) { var brush = new SolidColorBrush(color); brush.Freeze(); return brush; }

    public void SetStyle(CaptionStyle style)
    {
        style = style with { Display = CaptionStyle.ParseDisplay(style.Display) };
        if (style == Style) return;
        bool obsChanged = style.Obs != Style.Obs;
        Style = style;
        OnPropertyChanged(nameof(Style));
        OnPropertyChanged(nameof(CaptionForeground));
        OnPropertyChanged(nameof(CaptionBackground));
        OnPropertyChanged(nameof(AudienceEnabled));
        OnPropertyChanged(nameof(ObsEnabled));
        OnPropertyChanged(nameof(SyncShow));
        OnPropertyChanged(nameof(CaptionLines));
        OnPropertyChanged(nameof(ScoreSize));
        OnPropertyChanged(nameof(Display));
        OnPropertyChanged(nameof(HasAudienceText));
        try { if (_stylePath != null) File.WriteAllText(_stylePath, JsonSerializer.Serialize(style)); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(error.Message); }
        Emit();
        if (obsChanged) PublishedChanged?.Invoke(Published); // the OBS page shows or drops the current caption
    }

    private bool _imageWithText;
    private string? _imageCaption;

    /// <summary>
    /// Shows a picture (a row of a score slide) instead of the text, or above it; null returns to the text.
    /// caption, when given, is the text under the picture instead of the slide's (the lyrics of that row).
    /// </summary>
    public void SetImage(BitmapSource? image, bool withText = false, string? caption = null)
    {
        if (ReferenceEquals(image, _image) && withText == _imageWithText && caption == _imageCaption) return;
        _imageWithText = withText;
        _imageCaption = caption;
        if (ReferenceEquals(image, _image)) { Emit(); return; }
        _image = image;
        _imageUrl = image == null ? null : "data:image/jpeg;base64," + Convert.ToBase64String(EncodeJpeg(image));
        Emit();
    }

    private static byte[] EncodeJpeg(BitmapSource image)
    {
        var encoder = new JpegBitmapEncoder { QualityLevel = 88 };
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
    /// <summary>Destination: the camera caption on Axicultus's output screen.</summary>
    public bool AudienceEnabled
    {
        get => Style.Camera;
        set { if (value != Style.Camera) SetStyle(Style with { Camera = value }); }
    }

    /// <summary>Destination: the OBS browser source.</summary>
    public bool ObsEnabled
    {
        get => Style.Obs;
        set { if (value != Style.Obs) SetStyle(Style with { Obs = value }); }
    }

    /// <summary>The console's next/previous turn the output screen too; the caption then follows the screen.</summary>
    public bool SyncShow
    {
        get => Style.Sync;
        set { if (value != Style.Sync) SetStyle(Style with { Sync = value }); }
    }

    /// <summary>Height of the caption area in lines (1 to 3); longer text shrinks to fit it.</summary>
    public int CaptionLines
    {
        get => Style.Lines;
        set { value = Math.Clamp(value, 1, 3); if (value != Style.Lines) SetStyle(Style with { Lines = value }); }
    }

    /// <summary>Score row size: 0 작게, 1 보통, 2 크게 (see <see cref="CaptionStyle.ScoreHeight"/>).</summary>
    public int ScoreSize
    {
        get => Style.Score;
        set { value = Math.Clamp(value, 0, 2); if (value != Style.Score) SetStyle(Style with { Score = value }); }
    }

    /// <summary>What a followed PPT sends: auto, text, score or scoretext (see <see cref="CaptionStyle.Display"/>).</summary>
    public string Display
    {
        get => Style.Display;
        set { value = CaptionStyle.ParseDisplay(value); if (value != Style.Display) SetStyle(Style with { Display = value }); }
    }

    public bool HasAudienceText => AudienceEnabled && HasText;
    /// <summary>What the OBS browser source shows: empty while OBS is not a destination.</summary>
    public PublishedCaption ForObs => ObsEnabled ? Published : Published with { Text = "", Image = null };
    public event Action<PublishedCaption>? PublishedChanged;

    private string? _plan;
    public void BeginPlan(string plan)
    {
        if (_plan == plan) return;
        StopFollowing();
        _lastSlidePath = null;
        _lastSlideNumber = 0;
        _plan = plan;
        Deck = null;
        Overrides.Clear();
        History.Clear();
        BoxKey = "";
        CurrentSlide = 1;
        _text = "";
        SetVisible(false);
    }

    public void Prepare(CaptionDeck deck)
    {
        StopFollowing();
        Deck = deck;
        Overrides.Clear();
        BoxKey = "";
        CurrentSlide = 1;
        OnPropertyChanged(nameof(Following));
    }

    /// <summary>The box holding the lyrics read from the slide's picture.</summary>
    public const string RecognizedKey = "ocr:lyrics";

    /// <summary>Text that reads as Korean lyrics: 4 Hangul syllables or more, and 60% or more of its letters.</summary>
    internal static bool Lyrical(string text)
    {
        // (letters only: digits, punctuation and note signs read from a score do not count against it)
        int syllables = text.Count(c => c is >= (char)0xAC00 and <= (char)0xD7A3), letters = text.Count(char.IsLetter); // Hangul syllables
        return syllables >= 4 && syllables >= 0.6 * letters;
    }

    /// <summary>
    /// Sets the slide's recognized lyrics box. staves: they were read under staves found in the picture (a score, not a
    /// photo); then they also become the slide's whole text when that says little (a page mark over the score).
    /// </summary>
    public void AddRecognized(int slide, string text, bool staves = false)
    {
        if (Deck is null || slide < 1 || slide > Deck.Slides.Length) return;
        var current = Deck.Slides[slide - 1];
        var boxes = current.Boxes.Where(box => box.Key != RecognizedKey);
        if (staves && Lyrical(text))
            boxes = boxes.Select(box => box.Key == CaptionDeck.AllTextKey && CaptionDeck.ShortText(box.Text) ? box with { Text = text } : box);
        Deck.Slides[slide - 1] = current with { Boxes = [.. boxes, new(RecognizedKey, UiText.Get("CaptionConsole", "RecognizedBox"), text)] };
        if (slide == CurrentSlide) Resend();
    }

    /// <summary>Follows another box of the slide on air (a choice made for the operator): only the caption text changes.</summary>
    public void ChangeBox(string key)
    {
        if (!Following) return;
        BoxKey = key;
        Resend();
    }

    // The slide on air says something new: only its caption text goes out again (not a slide change: the score row on
    // air, the slide viewed and a caption being edited stay); nothing goes out when it reads the same.
    private void Resend()
    {
        if (!Following || TextFor(CurrentSlide, BoxKey) is not { } caption || caption == _text) return;
        _text = caption;
        Emit();
    }

    public string TextFor(int slide, string key)
    {
        if (Deck is null || slide < 1 || slide > Deck.Slides.Length) return "";
        if (Overrides.TryGetValue(slide, out var text)) return text;
        // A box this slide does not have (text boxes are named per slide), or lyrics not read on it, gives the slide's
        // whole text, not an empty caption.
        var boxes = Deck.Slides[slide - 1].Boxes;
        var box = boxes.FirstOrDefault(b => b.Key == key);
        if (box is null || key == RecognizedKey && box.Text.Length == 0) box = boxes.FirstOrDefault(b => b.Key == CaptionDeck.AllTextKey);
        return box?.Text ?? "";
    }

    public void Follow(int slide, string key)
    {
        if (Deck is null || slide < 1 || slide > Deck.Slides.Length) return;
        _generation++;
        _poll?.Start();
        BoxKey = key;
        Following = true;
        var live = _live.Current;
        // The show's slide now, whatever the output screen's state: only a turn from it moves the caption later.
        bool showsDeck = string.Equals(Deck.Path, live.PptPath, StringComparison.OrdinalIgnoreCase)
            && live.PptSlideNumber >= 1 && live.PptSlideNumber <= Deck.Slides.Length;
        _showSlide = showsDeck ? live.PptSlideNumber : null;
        if (showsDeck && live.State == LiveState.Active) slide = live.PptSlideNumber;
        MoveToSlide(slide);
        OnPropertyChanged(nameof(Following));
    }

    public void MoveToSlide(int slide)
    {
        if (!Following || Deck is null || slide < 1 || slide > Deck.Slides.Length) return;
        CurrentSlide = slide;
        _text = TextFor(slide, BoxKey);
        OnPropertyChanged(nameof(CurrentSlide));
        Emit();
    }

    /// <summary>Caption follows the output screen, live or not: the console follows its deck, or hands its text here.</summary>
    public bool Mirroring { get; private set; }

    public void Mirror(bool on)
    {
        Mirroring = on;
        if (!on) StopFollowing();
        OnPropertyChanged(nameof(Mirroring));
        OnPropertyChanged(nameof(Following));
        Emit();
    }

    /// <summary>The text on the output screen (not a deck) while mirroring.</summary>
    public void ShowScreenText(string text)
    {
        if (!Mirroring) return;
        if (Following) { StopFollowing(); OnPropertyChanged(nameof(Following)); }
        _text = text.Trim();
        Emit();
    }

    /// <summary>
    /// Sets the typed caption. show puts it on air (the 송출 button); otherwise a hidden caption stays hidden until
    /// 표시 — following the screen or typing never reveals it by itself.
    /// </summary>
    public void Publish(string text, bool show = true)
    {
        Mirroring = false;
        OnPropertyChanged(nameof(Mirroring));
        StopFollowing();
        _text = text.Trim();
        if (show) _visible = true;
        OnPropertyChanged(nameof(Following));
        Emit();
    }

    public void RememberTake()
    {
        if (_text.Length == 0 || History.LastOrDefault()?.Text == _text) return;
        History.Add(new(DateTimeOffset.UtcNow, _text));
        if (History.Count > 100) History.RemoveAt(0);
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
        Emit();
    }

    private void OnLiveChanged(object? sender, LiveSessionChangedEventArgs e)
    {
        var current = e.Snapshot;
        if (current.State == LiveState.Off && _previousState != LiveState.Off && !Mirroring) _visible = false;
        _previousState = current.State;
        if (current.State == LiveState.Active &&
            (_lastSlideNumber != current.PptSlideNumber || _lastSlidePath != current.PptPath))
        {
            var trackedDeck = Following && Deck != null && string.Equals(Deck.Path, _lastSlidePath, StringComparison.OrdinalIgnoreCase);
            _lastSlideNumber = current.PptSlideNumber;
            _lastSlidePath = current.PptPath;
            if (Deck != null && string.Equals(Deck.Path, current.PptPath, StringComparison.OrdinalIgnoreCase))
            {
                // A show started again on the slide it last showed is no turn: a caption the operator moved stays.
                bool turned = _showSlide != current.PptSlideNumber;
                _showSlide = current.PptSlideNumber;
                if (turned) MoveToSlide(current.PptSlideNumber);
            }
            // Mirroring, the console hands over what replaced the deck; blanking first would flash an empty caption.
            else if (trackedDeck && !Mirroring) _text = "";
        }
        Emit();
    }

    private int _holding;
    private bool _held;

    /// <summary>What changes until the returned object is disposed goes out as one caption (no flash of a step between).</summary>
    internal IDisposable Hold()
    {
        _holding++;
        return new Release(this);
    }

    private sealed class Release(CaptionSession session) : IDisposable
    {
        private bool _done;
        public void Dispose()
        {
            if (_done) return;
            _done = true;
            if (--session._holding == 0 && session._held) { session._held = false; session.Emit(); }
        }
    }

    private void Emit()
    {
        if (_holding > 0) { _held = true; return; }
        // What would go out on air, for the console's preview while hidden; nothing is sent for it.
        var readyText = _image == null ? _text : _imageWithText ? _imageCaption ?? _text : "";
        if (readyText != ReadyText || !ReferenceEquals(_image, ReadyImage))
        {
            (ReadyText, ReadyImage) = (readyText, _image);
            OnPropertyChanged(nameof(ReadyText));
            OnPropertyChanged(nameof(ReadyImage));
        }
        OnPropertyChanged(nameof(Visible));
        OnPropertyChanged(nameof(ScreenOff));
        bool onAir = _visible && !ScreenOff;
        var image = onAir ? _imageUrl : null;
        var rendered = !onAir ? "" : image == null ? _text : _imageWithText ? _imageCaption ?? _text : "";
        if (Published.Text == rendered && Published.Image == image && Published.Color == Style.Color && Published.Box == Style.Box
            && Published.Lines == Style.Lines && Published.ScoreHeight == CaptionStyle.ScoreHeight(Style.Score)) return;
        Published = new(Published.Revision + 1, rendered, Style.Color, Style.Box, image, Style.Lines, CaptionStyle.ScoreHeight(Style.Score));
        PublicImage = image == null ? null : _image;
        OnPropertyChanged(nameof(PublicImage));
        OnPropertyChanged(nameof(PublicText));
        OnPropertyChanged(nameof(HasText));
        OnPropertyChanged(nameof(HasPublicText));
        OnPropertyChanged(nameof(HasAudienceText));
        PublishedChanged?.Invoke(Published);
    }

    private async void PollNativeSlide(object? sender, EventArgs e) => await SyncNativeSlideAsync();

    internal async Task SyncNativeSlideAsync()
    {
        if (_polling || !Following || _show == null || Deck == null || _live.Current.State != LiveState.Active) return;
        if (!string.Equals(_live.Current.PptPath, Deck.Path, StringComparison.OrdinalIgnoreCase)) return;
        var generation = _generation;
        var snapshot = _live.Current;
        _polling = true;
        try
        {
            var slide = await _show.ReadOutputPositionAsync(Deck.Path);
            // Only a turn of the show moves the caption: one moved by the operator (strip, arrows) stays until the show turns.
            // Not known yet (followed before the show was seen): this reading only records it.
            if (generation == _generation && ReferenceEquals(snapshot, _live.Current) && slide.HasValue && slide != _showSlide)
            {
                bool turned = _showSlide.HasValue;
                _showSlide = slide;
                if (turned && slide != CurrentSlide) MoveToSlide(slide.Value);
            }
        }
        catch (Exception error) { System.Diagnostics.Debug.WriteLine(error.Message); }
        finally { _polling = false; }
    }

    private void StopFollowing()
    {
        _generation++;
        _poll?.Stop();
        Following = false;
    }

    public void Dispose()
    {
        StopFollowing();
        _poll?.Tick -= PollNativeSlide;
        _live.SessionChanged -= OnLiveChanged;
    }
}
