using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Easislides.Wpf.Rendering;
using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Controls;

namespace Easislides.Wpf.Captions;

public sealed record PublishedCaption(long Revision, string Text);
public sealed record CaptionTake(DateTimeOffset Time, string Text);

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

    public CaptionSession(ILiveSessionService live, IPowerPointSlideShowControl? show = null)
    {
        _live = live;
        _show = show;
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
    public bool HasText => PublicText.Length > 0;
    private bool _audienceEnabled = true;
    public bool AudienceEnabled
    {
        get => _audienceEnabled;
        set
        {
            if (SetProperty(ref _audienceEnabled, value)) OnPropertyChanged(nameof(HasAudienceText));
        }
    }
    public bool HasAudienceText => AudienceEnabled && HasText;
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

    public void Publish(string text)
    {
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
        if (current.State == LiveState.Off && _previousState != LiveState.Off) _visible = false;
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
        var rendered = _visible && _live.Current.State is not (LiveState.Hidden or LiveState.Standby) ? _text : "";
        if (Published.Text == rendered) return;
        Published = new(Published.Revision + 1, rendered);
        OnPropertyChanged(nameof(PublicText));
        OnPropertyChanged(nameof(HasText));
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
        if (_poll != null) _poll.Tick -= PollNativeSlide;
        _live.SessionChanged -= OnLiveChanged;
    }
}
