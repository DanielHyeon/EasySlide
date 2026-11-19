using System;
using System.Linq;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Platform;

public interface IWindowPlacementService
{
    OutputWindowPlacement CreateOutputPlacement(OutputDisplay display, bool windowed);
    OutputWindowPlacement CreateStagePlacement(OutputDisplay display, bool windowed) =>
        WindowPlacementService.PlaceOnDisplay(display, windowed);
}

public sealed class WindowPlacementService : IWindowPlacementService
{
    private const double PreferredWindowedWidth = 1280;
    private const double PreferredWindowedHeight = 720;
    private const double AspectRatio = PreferredWindowedWidth / PreferredWindowedHeight;
    private const double LegacyCustomAspectRatio = 4d / 3d;

    private readonly ISettingsService? _settings;
    private readonly IDisplayService? _displays;

    public WindowPlacementService()
    {
    }

    public WindowPlacementService(ISettingsService settings, IDisplayService? displays = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _displays = displays;
    }

    public OutputWindowPlacement CreateOutputPlacement(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);
        var bounds = CreatePlacementBounds(display);
        return Place(bounds.Left, bounds.Top, bounds.Width, bounds.Height, windowed);
    }

    internal static OutputWindowPlacement PlaceOnDisplay(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);
        return Place(display.X, display.Y, display.Width, display.Height, windowed);
    }

    // Virtual desktop bounds and the resulting placement are in physical pixels.
    private static OutputWindowPlacement Place(double x, double y, double screenWidth, double screenHeight, bool windowed)
    {
        if (!windowed)
        {
            return new OutputWindowPlacement(
                x,
                y,
                screenWidth,
                screenHeight,
                IsWindowed: false);
        }

        var width = Math.Min(PreferredWindowedWidth, screenWidth);
        var height = width / AspectRatio;

        if (height > screenHeight)
        {
            height = screenHeight;
            width = height * AspectRatio;
        }

        var left = x + (screenWidth - width) / 2;
        var top = y + (screenHeight - height) / 2;
        return new OutputWindowPlacement(left, top, width, height, IsWindowed: true);
    }

    private DisplayBounds CreatePlacementBounds(OutputDisplay display)
    {
        if (TryGetCustomBounds(out int left, out int top, out int width))
        {
            var height = width / LegacyCustomAspectRatio;
            if (_displays == null || _displays.GetDisplays().Any(screen =>
                left < screen.X + screen.Width && left + (double)width > screen.X &&
                top < screen.Y + screen.Height && top + height > screen.Y))
                return new DisplayBounds(left, top, width, height);
        }
        return new DisplayBounds(display.X, display.Y, display.Width, display.Height);
    }

    private bool TryGetCustomBounds(out int left, out int top, out int width)
    {
        left = 0;
        top = 0;
        width = 0;

        if (_settings is null)
        {
            return false;
        }

        left = _settings.Get(EasiSettingKeys.DisplayCustomLeft);
        top = _settings.Get(EasiSettingKeys.DisplayCustomTop);
        width = _settings.Get(EasiSettingKeys.DisplayCustomWidth);

        return left != EasiSettingKeys.DisplayCustomLeft.DefaultValue
            || top != EasiSettingKeys.DisplayCustomTop.DefaultValue
            || width != EasiSettingKeys.DisplayCustomWidth.DefaultValue;
    }

    private sealed record DisplayBounds(double Left, double Top, double Width, double Height);
}
