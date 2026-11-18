using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Easislides.Util;

internal sealed record MonitorDevice(string Name, Rectangle Bounds, bool IsPrimary);

internal readonly record struct MonitorPlacement(string DeviceName, Rectangle Bounds, bool Enabled, bool IsDual = false);

internal static class MonitorLayout
{
    internal const string Auto = "Auto";
    internal const string None = "None";

    internal static Rectangle Fit(Rectangle area, bool wide)
    {
        if (area.Width <= 0 || area.Height <= 0)
            return Rectangle.Empty;

        var (aspectWidth, aspectHeight) = Aspect(wide);
        int width = (int)Math.Max(1, Math.Min(area.Width, (long)area.Height * aspectWidth / aspectHeight));
        int height = (int)Math.Max(1, Math.Min(area.Height, (long)area.Width * aspectHeight / aspectWidth));
        return new Rectangle(area.Left + (area.Width - width) / 2, area.Top + (area.Height - height) / 2, width, height);
    }

    internal static int HeightForWidth(int width, bool wide)
    {
        var (aspectWidth, aspectHeight) = Aspect(wide);
        return (int)Math.Max(1, (long)width * aspectHeight / aspectWidth);
    }

    internal static MonitorDevice FindDisplay(IEnumerable<MonitorDevice> displays, string name) =>
        displays.FirstOrDefault(display => string.Equals(display.Name, name, StringComparison.OrdinalIgnoreCase));

    internal static MonitorPlacement ResolveOutput(
        IReadOnlyList<MonitorDevice> displays, string preferredName, bool preferSecondary, bool wide,
        Point? customOrigin = null, int customWidth = 1, bool asSingle = false)
    {
        var primary = displays.FirstOrDefault(display => display.IsPrimary) ?? displays.FirstOrDefault();
        if (primary == null)
            return new MonitorPlacement(null, new Rectangle(0, 0, 960, HeightForWidth(960, wide)), false);

        if (customOrigin is Point origin)
        {
            Rectangle bounds = CustomBounds(origin, customWidth, wide);
            var device = FindOverlappingDisplay(displays, bounds);
            return device != null
                ? new MonitorPlacement(device.Name, bounds, true, displays.Count > 1 && !asSingle)
                : new MonitorPlacement(null, Fit(primary.Bounds, wide), false);
        }

        if (string.Equals(preferredName, None, StringComparison.OrdinalIgnoreCase))
            return new MonitorPlacement(null, Fit(primary.Bounds, wide), false);

        var secondary = preferSecondary ? displays.FirstOrDefault(display => !display.IsPrimary) : null;
        var selected = FindDisplay(displays, preferredName) ?? secondary ?? primary;
        return new MonitorPlacement(selected.Name, Fit(selected.Bounds, wide), true, displays.Count > 1);
    }

    internal static MonitorPlacement ResolveLyrics(
        IReadOnlyList<MonitorDevice> displays, string preferredName, bool preferSecondary, bool wide,
        MonitorPlacement output, Point? customOrigin = null, int customWidth = 1)
    {
        if (customOrigin is Point origin)
        {
            Rectangle bounds = CustomBounds(origin, customWidth, wide);
            var device = FindOverlappingDisplay(displays, bounds);
            return device != null && (!output.Enabled || !output.Bounds.IntersectsWith(bounds))
                ? new MonitorPlacement(device.Name, bounds, true)
                : default;
        }

        if (string.Equals(preferredName, None, StringComparison.OrdinalIgnoreCase))
            return default;

        var candidates = displays.Where(display => !display.IsPrimary && (!output.Enabled ||
            !display.Bounds.IntersectsWith(output.Bounds)));
        var selected = FindDisplay(candidates, preferredName) ?? (preferSecondary ? candidates.FirstOrDefault() : null);
        return selected != null ? new MonitorPlacement(selected.Name, Fit(selected.Bounds, wide), true) : default;
    }

    private static (int Width, int Height) Aspect(bool wide) => wide ? (16, 9) : (4, 3);

    private static Rectangle CustomBounds(Point origin, int width, bool wide) =>
        new(origin, new Size(Math.Max(1, width), HeightForWidth(width, wide)));

    private static MonitorDevice FindOverlappingDisplay(IEnumerable<MonitorDevice> displays, Rectangle bounds) =>
        displays.Where(display => display.Bounds.IntersectsWith(bounds))
            .MaxBy(display =>
            {
                Rectangle overlap = Rectangle.Intersect(display.Bounds, bounds);
                return (long)overlap.Width * overlap.Height;
            });
}
