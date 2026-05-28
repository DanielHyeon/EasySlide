using System;

namespace Easislides.Wpf.Shell;

public sealed record OutputDisplay(
    string Id,
    string Name,
    double X,
    double Y,
    double Width,
    double Height,
    double DpiScale)
{
    public static OutputDisplay PrimaryFallback { get; } = new(
        "primary",
        "주 모니터",
        0,
        0,
        1920,
        1080,
        1.0);
}

public sealed record OutputWindowPlacement(
    double Left,
    double Top,
    double Width,
    double Height,
    bool IsWindowed)
{
    private const double PreferredWindowedWidth = 1280;
    private const double PreferredWindowedHeight = 720;
    private const double AspectRatio = 16d / 9d;

    public static OutputWindowPlacement FromDisplay(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);

        if (!windowed)
        {
            return new OutputWindowPlacement(
                display.X,
                display.Y,
                display.Width,
                display.Height,
                IsWindowed: false);
        }

        var width = Math.Min(PreferredWindowedWidth, display.Width);
        var height = width / AspectRatio;

        if (height > display.Height)
        {
            height = display.Height;
            width = height * AspectRatio;
        }

        var left = display.X + (display.Width - width) / 2;
        var top = display.Y + (display.Height - height) / 2;
        return new OutputWindowPlacement(left, top, width, height, IsWindowed: true);
    }
}

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
    public event EventHandler<OutputWindowChangedEventArgs>? OutputChanged;

    public OutputWindowState Current { get; private set; } = OutputWindowState.Closed;

    public void Open(OutputDisplay display, bool windowed) => Update(CreateState(display, windowed));

    public void MoveTo(OutputDisplay display, bool windowed) => Update(CreateState(display, windowed));

    public void Close() => Update(OutputWindowState.Closed);

    private static OutputWindowState CreateState(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);
        return new OutputWindowState(
            IsOpen: true,
            display,
            OutputWindowPlacement.FromDisplay(display, windowed));
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
