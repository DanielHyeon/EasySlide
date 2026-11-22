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

namespace Axicultus.Wpf.Captions;

/// <summary>Image is a JPEG data URL of a score half shown instead of the text.</summary>
public sealed record PublishedCaption(long Revision, string Text, string Color = CaptionStyle.DefaultColor, bool Box = true, string? Image = null);
public sealed record CaptionTake(DateTimeOffset Time, string Text);
/// <summary>Look and destinations of the caption; Camera is the output screen's camera caption, Obs the browser source.</summary>
public sealed record CaptionStyle(string Color = CaptionStyle.DefaultColor, bool Box = true, bool Camera = true, bool Obs = true)
{
    public const string DefaultColor = "#FFFFFF";
    public static CaptionStyle Parse(string color, bool box, bool camera = true, bool obs = true) =>
        new(System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$") ? color.ToUpperInvariant() : DefaultColor, box, camera, obs);
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
                Style = CaptionStyle.Parse(saved.Color, saved.Box, saved.Camera, saved.Obs);
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
    public PublishedCaption Published { get; private set; } = new(0, "");
    public string PublicText => Published.Text;
    public ImageSource? PublicImage { get; private set; }
    public bool HasText => PublicText.Length > 0 || PublicImage != null;
    public bool HasPublicText => PublicText.Length > 0;
    public CaptionStyle Style { get; private set; } = new();
    public Brush CaptionForeground => Frozen((Color)ColorConverter.ConvertFromString(Style.Color));
    public Brush CaptionBackground => Style.Box ? Frozen(Color.FromArgb(0xCC, 0x10, 0x10, 0x10)) : Brushes.Transparent;
    private static Brush Frozen(Color color) { var brush = new SolidColorBrush(color); brush.Freeze(); return brush; }

    public void SetStyle(CaptionStyle style)
    {
        if (style == Style) return;
        bool obsChanged = style.Obs != Style.Obs;
        Style = style;
        OnPropertyChanged(nameof(Style));
        OnPropertyChanged(nameof(CaptionForeground));
        OnPropertyChanged(nameof(CaptionBackground));
        OnPropertyChanged(nameof(AudienceEnabled));
        OnPropertyChanged(nameof(ObsEnabled));
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

    public void AddRecognized(int slide, string text)
    {
        if (Deck is null || slide < 1 || slide > Deck.Slides.Length) return;
        var current = Deck.Slides[slide - 1];
        Deck.Slides[slide - 1] = current with
        {
            Boxes = [.. current.Boxes.Where(box => box.Key != RecognizedKey), new(RecognizedKey, "그림 속 가사 (자동 인식)", text)]
        };
        if (Following && slide == CurrentSlide && BoxKey == RecognizedKey) MoveToSlide(slide);
    }

    public string TextFor(int slide, string key)
    {
        if (Deck is null || slide < 1 || slide > Deck.Slides.Length) return "";
        return Overrides.TryGetValue(slide, out var text) ? text :
            Deck.Slides[slide - 1].Boxes.FirstOrDefault(box => box.Key == key)?.Text ?? "";
    }

    public void Follow(int slide, string key)
    {
        if (Deck is null || slide < 1 || slide > Deck.Slides.Length) return;
        _generation++;
        _poll?.Start();
        BoxKey = key;
        Following = true;
        _visible = true;
        var live = _live.Current;
        if (live.State == LiveState.Active && string.Equals(Deck.Path, live.PptPath, StringComparison.OrdinalIgnoreCase)
            && live.PptSlideNumber >= 1 && live.PptSlideNumber <= Deck.Slides.Length)
            slide = live.PptSlideNumber;
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
        if (on) _visible = true;
        else StopFollowing();
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

    public void Publish(string text)
    {
        Mirroring = false;
        OnPropertyChanged(nameof(Mirroring));
        StopFollowing();
        _text = text.Trim();
        _visible = true;
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
                MoveToSlide(current.PptSlideNumber);
            else if (trackedDeck) _text = "";
        }
        Emit();
    }

    private void Emit()
    {
        OnPropertyChanged(nameof(Visible));
        bool onAir = _visible && _live.Current.State is not (LiveState.Hidden or LiveState.Standby);
        var image = onAir ? _imageUrl : null;
        var rendered = !onAir ? "" : image == null ? _text : _imageWithText ? _imageCaption ?? _text : "";
        if (Published.Text == rendered && Published.Image == image && Published.Color == Style.Color && Published.Box == Style.Box) return;
        Published = new(Published.Revision + 1, rendered, Style.Color, Style.Box, image);
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
            if (generation == _generation && ReferenceEquals(snapshot, _live.Current) && slide.HasValue && slide != CurrentSlide)
                MoveToSlide(slide.Value);
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
