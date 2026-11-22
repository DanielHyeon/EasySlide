using System;
using System.Linq;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Platform;

public interface IWindowPlacementService
{
    OutputWindowPlacement CreateOutputPlacement(OutputDisplay display, bool windowed);
    OutputWindowPlacement CreateMediaPlacement(OutputDisplay display) => WindowPlacementService.PlaceOnDisplay(display, windowed: false);
    OutputWindowPlacement CreateStagePlacement(OutputDisplay display, bool windowed) =>
        WindowPlacementService.PlaceOnDisplay(display, windowed);
}

public sealed class WindowPlacementService : IWindowPlacementService
{
    private const double PreferredWindowedWidth = 1280;
    private const double PreferredWindowedHeight = 720;
    private const double AspectRatio = PreferredWindowedWidth / PreferredWindowedHeight;


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
        return PlaceConfiguredBounds(bounds, windowed) with
        {
            AsSingleMonitor = bounds.Custom && _settings?.Current.Operator.OutputAsSingle == true,
            CustomAreaOffScreen = bounds.OffScreen,
        };
    }

    public OutputWindowPlacement CreateMediaPlacement(OutputDisplay display)
    {
        ArgumentNullException.ThrowIfNull(display);
        var area = CreatePlacementBounds(display);
        return new OutputWindowPlacement(area.Left, area.Top, area.Width, area.Height, IsWindowed: false)
        {
            AsSingleMonitor = area.Custom && _settings?.Current.Operator.OutputAsSingle == true,
            CustomAreaOffScreen = area.OffScreen,
        };
    }

    public OutputWindowPlacement CreateStagePlacement(OutputDisplay display, bool windowed)
    {
        if (_settings?.Current.Operator is { StageCustom: true } options)
        {
            int height = Axicultus.Core.SlideAspectRatio.HeightForWidth(options.StageWidth, _settings.Current.LiveOutput.IsMonitorWide);
            return PlaceConfiguredBounds(new(options.StageLeft, options.StageTop, options.StageWidth, height, true), windowed);
        }
        return PlaceConfiguredBounds(new(display.X, display.Y, display.Width, display.Height), windowed);
    }

    private OutputWindowPlacement PlaceConfiguredBounds(DisplayBounds bounds, bool windowed)
    {
        if (_settings is null) return Place(bounds.Left, bounds.Top, bounds.Width, bounds.Height, windowed);
        bool wide = _settings.Get(AxicultusSettingKeys.IsMonitorWide);
        var (Left, Top, Width, Height) = bounds.Custom
            ? (Left: (int)bounds.Left, Top: (int)bounds.Top, Width: (int)bounds.Width, Height: (int)bounds.Height)
            : Axicultus.Core.SlideAspectRatio.Fit((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height, wide);
        if (!windowed || Width <= PreferredWindowedWidth)
            return new(Left, Top, Width, Height, windowed);
        int width = (int)PreferredWindowedWidth;
        int height = Axicultus.Core.SlideAspectRatio.HeightForWidth(width, wide);
        return new(Left + (Width - width) / 2, Top + (Height - height) / 2, width, height, windowed);
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
            var height = Axicultus.Core.SlideAspectRatio.HeightForWidth(width, _settings!.Get(AxicultusSettingKeys.IsMonitorWide));
            if (_displays == null || _displays.GetDisplays().Any(screen =>
                left < screen.X + screen.Width && left + (double)width > screen.X &&
                top < screen.Y + screen.Height && top + height > screen.Y))
                return new DisplayBounds(left, top, width, height, true);
            // FrmMain shows nothing for an area off every screen (MonitorLayout.ResolveOutput); here the display
            // is used and the operator is told (MainViewModel.OutputAreaNote).
            return new DisplayBounds(display.X, display.Y, display.Width, display.Height, OffScreen: true);
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

        left = _settings.Get(AxicultusSettingKeys.DisplayCustomLeft);
        top = _settings.Get(AxicultusSettingKeys.DisplayCustomTop);
        width = _settings.Get(AxicultusSettingKeys.DisplayCustomWidth);

        return width > 0 && UsesCustomBounds(_settings.Current.LiveOutput);
    }

    // WPF profiles predating the explicit switch inferred custom mode from edited coordinates.
    internal static bool UsesCustomBounds(LiveOutputSettings options) => options.OutputPlacementMode switch
    {
        0 => false,
        1 => true,
        // WinForms stores a width of 1 even when custom placement has never been selected.
        _ => options.DisplayCustomWidth > 1 &&
            (options.DisplayCustomLeft != AxicultusSettingKeys.DisplayCustomLeft.DefaultValue
             || options.DisplayCustomTop != AxicultusSettingKeys.DisplayCustomTop.DefaultValue
             || options.DisplayCustomWidth != AxicultusSettingKeys.DisplayCustomWidth.DefaultValue),
    };

    private sealed record DisplayBounds(double Left, double Top, double Width, double Height, bool Custom = false, bool OffScreen = false);
}
