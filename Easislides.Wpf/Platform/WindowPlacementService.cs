using System;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Platform;

public interface IWindowPlacementService
{
    OutputWindowPlacement CreateOutputPlacement(OutputDisplay display, bool windowed);
}

public sealed class WindowPlacementService : IWindowPlacementService
{
    private const double PreferredWindowedWidth = 1280;
    private const double PreferredWindowedHeight = 720;
    private const double AspectRatio = PreferredWindowedWidth / PreferredWindowedHeight;

    public OutputWindowPlacement CreateOutputPlacement(OutputDisplay display, bool windowed)
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
