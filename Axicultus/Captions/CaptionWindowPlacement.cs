using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Axicultus.Wpf.Captions;

/// <summary>A screen in physical pixels: its bounds, its work area (without the taskbar) and its scale (1 at 96 DPI).</summary>
public sealed record CaptionScreen(Rect Bounds, Rect WorkArea, double Scale, bool Primary);

/// <summary>
/// Where the caption console was last (caption-window.json in the profile folder), in physical pixels: one coordinate
/// space over screens of different scales. A file of the earlier form (WPF units, no version) is not used.
/// </summary>
public sealed record CaptionWindowPlacement(double Left, double Top, double Width, double Height, bool Maximized, int Version = 0)
{
    public const string FileName = "caption-window.json";
    public const int PixelVersion = 2;

    /// <summary>The kept place, or null when there is none, it cannot be read or it is of the earlier form.</summary>
    public static CaptionWindowPlacement? Read(string path)
    {
        try
        {
            return File.Exists(path) && JsonSerializer.Deserialize<CaptionWindowPlacement>(File.ReadAllText(path)) is { Version: PixelVersion } place
                ? place : null;
        }
        catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException or NotSupportedException) { return null; }
    }

    public void Write(string path)
    {
        if (!(double.IsFinite(Left) && double.IsFinite(Top) && double.IsFinite(Width) && double.IsFinite(Height))) return; // never placed (not shown)
        try { File.WriteAllText(path, JsonSerializer.Serialize(this with { Version = PixelVersion })); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(error.Message); }
    }

    /// <summary>
    /// The place can be used: a sane size and a stretch of its title bar (200 wide, or all of a narrower window) on one
    /// of the screens' work areas, so the window can be grabbed and moved.
    /// </summary>
    public static bool Fits(CaptionWindowPlacement? place, IEnumerable<CaptionScreen> screens)
    {
        if (place is null || !double.IsFinite(place.Left) || !double.IsFinite(place.Top)
            || !double.IsFinite(place.Width) || !double.IsFinite(place.Height) || place.Width < 1 || place.Height < 1) return false;
        var title = new Rect(place.Left, place.Top, place.Width, 30);
        return screens.Any(screen => Rect.Intersect(title, screen.WorkArea) is { IsEmpty: false } shown
            && shown.Width >= Math.Min(200, place.Width) && shown.Height >= 20);
    }

    /// <summary>The connected screens.</summary>
    public static IReadOnlyList<CaptionScreen> Screens() => [.. System.Windows.Forms.Screen.AllScreens.Select(screen => new CaptionScreen(
        Pixels(screen.Bounds), Pixels(screen.WorkingArea), Platform.SystemDisplayReader.GetDpiScale(screen.Bounds), screen.Primary))];

    private static Rect Pixels(System.Drawing.Rectangle area) => new(area.X, area.Y, area.Width, area.Height);

    /// <summary>The main screen (never just the first in the list).</summary>
    internal static CaptionScreen Primary(IReadOnlyList<CaptionScreen> screens) => screens.FirstOrDefault(screen => screen.Primary) ?? screens[0];

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

    private static Point Middle(Rect area) => new(area.Left + area.Width / 2, area.Top + area.Height / 2);

    /// <summary>The wanted rectangle no larger than the work area and inside it.</summary>
    internal static Rect Inside(Rect wanted, Rect work)
    {
        double width = Math.Min(wanted.Width, work.Width), height = Math.Min(wanted.Height, work.Height);
        return new(Math.Clamp(wanted.Left, work.Left, work.Right - width), Math.Clamp(wanted.Top, work.Top, work.Bottom - height), width, height);
    }

    /// <summary>The screen a window belonging to owner goes on: the owner's (where it really is), else the main screen.</summary>
    internal static CaptionScreen Home(Rect? owner, IReadOnlyList<CaptionScreen> screens) =>
        owner is { } over ? At(Middle(over), screens) : Primary(screens);

    /// <summary>A rectangle of the size in the middle of the owner (else of the screen), inside the screen's work area.</summary>
    internal static Rect Centred(Size size, Rect? owner, CaptionScreen screen)
    {
        var middle = owner is { } around ? Middle(around) : Middle(screen.WorkArea);
        return Inside(new Rect(middle.X - size.Width / 2, middle.Y - size.Height / 2, size.Width, size.Height), screen.WorkArea);
    }

    /// <summary>The smallest a window may be on the screen: its designed minimum (WPF units), or less on a small screen.</summary>
    internal static Size Minimum(Size design, CaptionScreen screen) =>
        new(Math.Min(design.Width, screen.WorkArea.Width / screen.Scale), Math.Min(design.Height, screen.WorkArea.Height / screen.Scale));

    /// <summary>
    /// Where the window goes, in pixels: the kept place when a screen still shows it (moved into its work area), else its
    /// size (WPF units, at the screen's scale) in the middle of the window it belongs to (its screen), else of the main
    /// screen.
    /// </summary>
    internal static (Rect Bounds, bool Maximized, CaptionScreen Screen) Plan(CaptionWindowPlacement? kept, Rect? owner,
        IReadOnlyList<CaptionScreen> screens, Size size)
    {
        if (Fits(kept, screens))
        {
            var place = new Rect(kept!.Left, kept.Top, kept.Width, kept.Height);
            var home = At(new Point(place.Left + Math.Min(place.Width, 200) / 2, place.Top + 15), screens);
            return (Inside(place, home.WorkArea), kept.Maximized, home);
        }
        var screen = Home(owner, screens);
        return (Centred(new Size(size.Width * screen.Scale, size.Height * screen.Scale), owner, screen), false, screen);
    }

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
        Platform.DesktopWindowPlacement.Apply(window, new Shell.OutputWindowPlacement(bounds.Left, bounds.Top, bounds.Width, bounds.Height, true));
    }
}
