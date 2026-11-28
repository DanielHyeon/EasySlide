using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Axicultus.Wpf.Captions;

/// <summary>A screen in physical pixels: its bounds, its work area (without the taskbar) and its scale (1 at 96 DPI).</summary>
public sealed record CaptionScreen(Rect Bounds, Rect WorkArea, double Scale, bool Primary);

/// <summary>
/// Where the caption settings window goes, in physical pixels: one coordinate space over screens of different scales.
/// </summary>
internal static class CaptionWindowPlacement
{
    /// <summary>The connected screens.</summary>
    public static IReadOnlyList<CaptionScreen> Screens() => [.. System.Windows.Forms.Screen.AllScreens.Select(screen => new CaptionScreen(
        Pixels(screen.Bounds), Pixels(screen.WorkingArea), Axicultus.Interop.DesktopPixels.Scale(screen.Bounds), screen.Primary))];

    private static Rect Pixels(System.Drawing.Rectangle area) => new(area.X, area.Y, area.Width, area.Height);

    /// <summary>The screen holding the point, else the nearest one.</summary>
    internal static CaptionScreen At(Point point, IReadOnlyList<CaptionScreen> screens) =>
        screens.FirstOrDefault(screen => screen.Bounds.Contains(point))
        ?? screens.OrderBy(screen => Distance(screen.Bounds, point)).First();

    private static double Distance(Rect area, Point point)
    {
        double dx = Math.Max(0, Math.Max(area.Left - point.X, point.X - area.Right));
        double dy = Math.Max(0, Math.Max(area.Top - point.Y, point.Y - area.Bottom));
        return dx * dx + dy * dy;
    }

    /// <summary>The wanted rectangle no larger than the work area and inside it.</summary>
    internal static Rect Inside(Rect wanted, Rect work)
    {
        double width = Math.Min(wanted.Width, work.Width), height = Math.Min(wanted.Height, work.Height);
        return new(Math.Clamp(wanted.Left, work.Left, work.Right - width), Math.Clamp(wanted.Top, work.Top, work.Bottom - height), width, height);
    }

    /// <summary>The smallest a window may be on the screen: its designed minimum (WPF units), or less on a small screen.</summary>
    internal static Size Minimum(Size design, CaptionScreen screen) =>
        new(Math.Min(design.Width, screen.WorkArea.Width / screen.Scale), Math.Min(design.Height, screen.WorkArea.Height / screen.Scale));

    /// <summary>A shown window's rectangle in pixels (where it really is, maximized too); null before it has a handle.</summary>
    internal static Rect? Bounds(Window window)
    {
        var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        return handle != 0 && Axicultus.Interop.DesktopWindows.GetWindowRect(handle, out var bounds) && bounds.Width > 0
            ? new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height) : null;
    }

    /// <summary>Puts the window at the pixel rectangle (before it is shown: no jump), with its minimum for that screen.</summary>
    internal static void Apply(Window window, Rect bounds, Size minimum)
    {
        (window.MinWidth, window.MinHeight) = (minimum.Width, minimum.Height);
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        Axicultus.Interop.DesktopPixels.Place(new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle(), bounds.Left, bounds.Top, bounds.Width, bounds.Height);
    }
}
