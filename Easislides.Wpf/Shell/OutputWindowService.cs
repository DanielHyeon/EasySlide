using System;
using Easislides.Wpf.Platform;

namespace Easislides.Wpf.Shell;

public sealed record OutputDisplay(
    string Id,
    string Name,
    double X,
    double Y,
    double Width,
    double Height,
    double DpiScale,
    bool IsPrimary = false)
{
    public static OutputDisplay PrimaryFallback { get; } = new(
        "primary",
        "주 모니터",
        0,
        0,
        1920,
        1080,
        1.0,
        IsPrimary: true);
}

public sealed record OutputWindowPlacement(
    double Left,
    double Top,
    double Width,
    double Height,
    bool IsWindowed);

public sealed record OutputWindowState(
    bool IsOpen,
    OutputDisplay? Display,
    OutputWindowPlacement Placement)
{
    public static OutputWindowState Closed { get; } = new(
        IsOpen: false,
        Display: null,
        new OutputWindowPlacement(0, 0, 0, 0, IsWindowed: true));
}

public sealed class OutputWindowChangedEventArgs : EventArgs
{
    public OutputWindowChangedEventArgs(OutputWindowState state) => State = state;

    public OutputWindowState State { get; }
}

public interface IOutputWindowService
{
    event EventHandler<OutputWindowChangedEventArgs>? OutputChanged;

    OutputWindowState Current { get; }

    void Open(OutputDisplay display, bool windowed);
    void MoveTo(OutputDisplay display, bool windowed);
    void Close();
}

public sealed class OutputWindowService : IOutputWindowService
{
    private readonly IWindowPlacementService _placement;

    public OutputWindowService()
        : this(new WindowPlacementService())
    {
    }

    public OutputWindowService(IWindowPlacementService placement)
    {
        _placement = placement;
    }

    public event EventHandler<OutputWindowChangedEventArgs>? OutputChanged;

    public OutputWindowState Current { get; private set; } = OutputWindowState.Closed;

    public void Open(OutputDisplay display, bool windowed) => Update(CreateState(display, windowed));

    public void MoveTo(OutputDisplay display, bool windowed) => Update(CreateState(display, windowed));

    public void Close() => Update(OutputWindowState.Closed);

    private OutputWindowState CreateState(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);
        return new OutputWindowState(
            IsOpen: true,
            display,
            _placement.CreateOutputPlacement(display, windowed));
    }

    private void Update(OutputWindowState state)
    {
        if (state == Current)
        {
            return;
        }

        Current = state;
        OutputChanged?.Invoke(this, new OutputWindowChangedEventArgs(state));
    }
}
