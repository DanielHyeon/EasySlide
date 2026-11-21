using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Axicultus.Core;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Platform;

public interface IDisplayService
{
    IReadOnlyList<OutputDisplay> GetDisplays();
    OutputDisplay GetPrimaryDisplay();
    OutputDisplay GetPreferredDisplay(string? preferredDisplayId = null);
}

public interface IDisplayReader
{
    IReadOnlyList<OutputDisplay> ReadDisplays();
}

public sealed class DisplayService(IDisplayReader reader) : IDisplayService
{
    private readonly IDisplayReader _reader = reader;

    public IReadOnlyList<OutputDisplay> GetDisplays()
    {
        var displays = ReadDisplays()
            .Where(display => display.Width > 0 && display.Height > 0)
            .OrderByDescending(display => display.IsPrimary)
            .ThenBy(display => display.Y)
            .ThenBy(display => display.X)
            .ThenBy(display => display.Name, StringComparer.CurrentCulture)
            .ToArray();

        return displays.Length == 0 ? [OutputDisplay.PrimaryFallback] : displays;
    }

    public OutputDisplay GetPrimaryDisplay()
    {
        var displays = GetDisplays();
        return displays.FirstOrDefault(display => display.IsPrimary) ?? displays[0];
    }

    public OutputDisplay GetPreferredDisplay(string? preferredDisplayId = null)
    {
        var displays = GetDisplays();
        if (!string.IsNullOrWhiteSpace(preferredDisplayId))
        {
            var preferred = displays.FirstOrDefault(display =>
                string.Equals(display.Id, preferredDisplayId, StringComparison.OrdinalIgnoreCase));
            if (preferred is not null)
            {
                return preferred;
            }
        }

        return displays.FirstOrDefault(display => !display.IsPrimary) ?? GetPrimaryDisplay();
    }

    private IReadOnlyList<OutputDisplay> ReadDisplays()
    {
        try
        {
            return _reader.ReadDisplays();
        }
        catch
        {
            return Array.Empty<OutputDisplay>();
        }
    }
}

public sealed class SystemDisplayReader : IDisplayReader
{
    public IReadOnlyList<OutputDisplay> ReadDisplays()
    {
        return Screen.AllScreens
            .Select(screen => new OutputDisplay(
                screen.DeviceName,
                MonitorLabel.Connected(screen.DeviceName, screen.Primary, screen.Bounds.Width, screen.Bounds.Height),
                screen.Bounds.X,
                screen.Bounds.Y,
                screen.Bounds.Width,
                screen.Bounds.Height,
                GetDpiScale(screen.Bounds),
                screen.Primary))
            .ToArray();
    }

    private static double GetDpiScale(Rectangle bounds)
    {
        var center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        var monitor = Windows.Win32.PInvoke.MonitorFromPoint(center, Windows.Win32.Graphics.Gdi.MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        try
        {
            if (Windows.Win32.PInvoke.GetDpiForMonitor(monitor, 0, out uint dpi, out _) == 0 && dpi != 0)
                return dpi / 96d;
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
        using var graphics = Graphics.FromHwnd(IntPtr.Zero);
        return graphics.DpiX / 96d;
    }

}
