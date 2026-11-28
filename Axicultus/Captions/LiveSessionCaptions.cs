using System;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Captions;

/// <summary>The WPF app's live session and native show as the caption library sees them.</summary>
public static class LiveSessionCaptions
{
    public static ICaptionLiveSource ForCaptions(this ILiveSessionService live) => new LiveSource(live);

    public static ICaptionSlideShow? ForCaptions(this IPowerPointSlideShowControl? show) => show == null ? null : new SlideShow(show);

    internal static CaptionLiveState Map(LiveState state) => state switch
    {
        LiveState.Off => CaptionLiveState.Off,
        LiveState.Standby => CaptionLiveState.Standby,
        LiveState.Active => CaptionLiveState.Active,
        LiveState.Hidden => CaptionLiveState.Hidden,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    private sealed class LiveSource(ILiveSessionService live) : ICaptionLiveSource
    {
        // The caption view of the live snapshot last read as Current: one object per live snapshot, as Current promises.
        // Only Current fills it, so an event carrying an older snapshot cannot replace it. One immutable pair swapped
        // whole: Current may be read on one thread while a change is told on another, and a view is never paired with
        // another snapshot than its own.
        private sealed record Kept(LiveSessionSnapshot Source, CaptionLiveSnapshot View);
        private Kept? _current;
        private EventHandler<CaptionLiveSnapshot>? _changed;

        public CaptionLiveSnapshot Current
        {
            get
            {
                var snapshot = live.Current;
                var kept = Volatile.Read(ref _current);
                if (kept != null && ReferenceEquals(kept.Source, snapshot)) return kept.View;
                var made = new Kept(snapshot, View(snapshot));
                // (another reader that kept the same snapshot first: its view, so the snapshot keeps one view)
                var seen = Interlocked.CompareExchange(ref _current, made, kept);
                return ReferenceEquals(seen, kept) || seen == null || !ReferenceEquals(seen.Source, snapshot) ? made.View : seen.View;
            }
        }

        public event EventHandler<CaptionLiveSnapshot>? Changed
        {
            add
            {
                if (value == null) return;
                if (_changed == null) live.SessionChanged += OnSessionChanged;
                _changed += value;
            }
            remove
            {
                if (value == null || _changed == null) return;
                _changed -= value;
                if (_changed == null) live.SessionChanged -= OnSessionChanged;
            }
        }

        private void OnSessionChanged(object? sender, LiveSessionChangedEventArgs e) =>
            _changed?.Invoke(this, ReferenceEquals(e.Snapshot, live.Current) ? Current : View(e.Snapshot));

        private static CaptionLiveSnapshot View(LiveSessionSnapshot snapshot) => new(Map(snapshot.State), snapshot.PptPath, snapshot.PptSlideNumber);
    }

    private sealed class SlideShow(IPowerPointSlideShowControl show) : ICaptionSlideShow
    {
        public Task<int?> ReadSlideAsync(string path) => show.ReadOutputPositionAsync(path);
    }
}
