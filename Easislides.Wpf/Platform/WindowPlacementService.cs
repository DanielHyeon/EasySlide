using System;
using Easislides.Wpf.Settings;
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
    private const double LegacyCustomAspectRatio = 4d / 3d;
    private const int MinimumCustomWidth = 320;

    private readonly ISettingsService? _settings;

    public WindowPlacementService()
    {
    }

    public WindowPlacementService(ISettingsService settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public OutputWindowPlacement CreateOutputPlacement(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);
        var bounds = CreatePlacementBounds(display);

        if (!windowed)
        {
            return new OutputWindowPlacement(
                bounds.Left,
                bounds.Top,
                bounds.Width,
                bounds.Height,
                IsWindowed: false);
        }

        var width = Math.Min(PreferredWindowedWidth, bounds.Width);
        var height = width / AspectRatio;

        if (height > bounds.Height)
        {
            height = bounds.Height;
            width = height * AspectRatio;
        }

        var left = bounds.Left + (bounds.Width - width) / 2;
        var top = bounds.Top + (bounds.Height - height) / 2;
        return new OutputWindowPlacement(left, top, width, height, IsWindowed: true);
    }

    private DisplayBounds CreatePlacementBounds(OutputDisplay display)
    {
        if (!TryGetCustomBounds(out var left, out var top, out var width))
        {
            return new DisplayBounds(
                ToDips(display.X, display),
                ToDips(display.Y, display),
                ToDips(display.Width, display),
                ToDips(display.Height, display));
        }

        return new DisplayBounds(
            ToDips(left, display),
            ToDips(top, display),
            ToDips(width, display),
            ToDips(width / LegacyCustomAspectRatio, display));
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

        if (width < MinimumCustomWidth)
        {
            return false;
        }

        return left != EasiSettingKeys.DisplayCustomLeft.DefaultValue
            || top != EasiSettingKeys.DisplayCustomTop.DefaultValue
            || width != EasiSettingKeys.DisplayCustomWidth.DefaultValue;
    }

    private static double ToDips(double physicalPixels, OutputDisplay display)
    {
        var dpiScale = display.DpiScale > 0 ? display.DpiScale : 1d;
        return physicalPixels / dpiScale;
    }

    private sealed record DisplayBounds(double Left, double Top, double Width, double Height);
}
