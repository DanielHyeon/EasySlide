using System;
using Easislides.Wpf.Controls;

namespace Easislides.Wpf.Shell;

public sealed record LiveSessionSnapshot(
    LiveState State,
    string CurrentItemTitle,
    string OutputMonitorName,
    bool IsBlackout)
{
    public static LiveSessionSnapshot Off { get; } = new(
        LiveState.Off,
        string.Empty,
        string.Empty,
        IsBlackout: false);
}

public sealed class LiveSessionChangedEventArgs : EventArgs
{
    public LiveSessionChangedEventArgs(LiveSessionSnapshot snapshot) => Snapshot = snapshot;

    public LiveSessionSnapshot Snapshot { get; }
}

public interface ILiveSessionService
{
    event EventHandler<LiveSessionChangedEventArgs>? SessionChanged;

    LiveSessionSnapshot Current { get; }

    void GoLive(LiveQueueItem item, string outputMonitorName);
    void HideOutput(bool blackout);
    void Stop();
}

public sealed class LiveSessionService : ILiveSessionService
{
    public event EventHandler<LiveSessionChangedEventArgs>? SessionChanged;

    public LiveSessionSnapshot Current { get; private set; } = LiveSessionSnapshot.Off;

    public void GoLive(LiveQueueItem item, string outputMonitorName)
    {
        ArgumentNullException.ThrowIfNull(item);

        Update(new LiveSessionSnapshot(
            LiveState.Active,
            item.Title,
            outputMonitorName,
            IsBlackout: false));
    }

    public void HideOutput(bool blackout)
    {
        if (Current.State == LiveState.Off)
        {
            return;
        }

        Update(Current with
        {
            State = LiveState.Hidden,
            IsBlackout = blackout,
        });
    }

    public void Stop() => Update(LiveSessionSnapshot.Off);

    private void Update(LiveSessionSnapshot snapshot)
    {
        if (snapshot == Current)
        {
            return;
        }

        Current = snapshot;
        SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(snapshot));
    }
}
