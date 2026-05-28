using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Platform;

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

public sealed class DisplayService : IDisplayService
{
    private readonly IDisplayReader _reader;

    public DisplayService(IDisplayReader reader)
    {
        _reader = reader;
    }

    public IReadOnlyList<OutputDisplay> GetDisplays()
    {
        var displays = ReadDisplays()
            .Where(display => display.Width > 0 && display.Height > 0)
            .OrderByDescending(display => display.IsPrimary)
            .ThenBy(display => display.Y)
            .ThenBy(display => display.X)
            .ThenBy(display => display.Name, StringComparer.CurrentCulture)
            .ToArray();

        return displays.Length == 0 ? new[] { OutputDisplay.PrimaryFallback } : displays;
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
        var dpiScale = GetSystemDpiScale();
        return Screen.AllScreens
            .Select(screen => new OutputDisplay(
                screen.DeviceName,
                CreateDisplayName(screen),
                screen.Bounds.X,
                screen.Bounds.Y,
                screen.Bounds.Width,
                screen.Bounds.Height,
                dpiScale,
                screen.Primary))
            .ToArray();
    }

    private static string CreateDisplayName(Screen screen)
    {
        var role = screen.Primary ? "주 모니터" : "출력 모니터";
        return $"{role} {screen.DeviceName} ({screen.Bounds.Width}x{screen.Bounds.Height})";
    }

    private static double GetSystemDpiScale()
    {
        try
        {
            using var graphics = Graphics.FromHwnd(IntPtr.Zero);
            return Math.Round(graphics.DpiX / 96d, 2);
        }
        catch
        {
            return 1.0;
        }
    }
}
