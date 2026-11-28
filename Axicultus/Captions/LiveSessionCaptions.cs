using System;
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
        // Only Current fills it, so an event carrying an older snapshot cannot replace it.
        private (LiveSessionSnapshot Source, CaptionLiveSnapshot View)? _current;
        private EventHandler<CaptionLiveSnapshot>? _changed;

        public CaptionLiveSnapshot Current
        {
            get
            {
                var snapshot = live.Current;
                if (_current is { } kept && ReferenceEquals(kept.Source, snapshot)) return kept.View;
                var view = View(snapshot);
                _current = (snapshot, view);
                return view;
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
