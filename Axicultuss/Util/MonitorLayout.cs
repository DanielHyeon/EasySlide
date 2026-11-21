using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Axicultus.Util;

internal sealed record MonitorDevice(string Name, Rectangle Bounds, bool IsPrimary)
{
    public string Label => Axicultus.Core.MonitorLabel.Connected(Name, IsPrimary, Bounds.Width, Bounds.Height);
}

internal readonly record struct MonitorPlacement(string DeviceName, Rectangle Bounds, bool Enabled, bool IsDual = false);

internal static class MonitorLayout
{
    internal const string Auto = "Auto";
    internal const string None = "None";

    internal static Rectangle Fit(Rectangle area, bool wide)
    {
        var (Left, Top, Width, Height) = Axicultus.Core.SlideAspectRatio.Fit(area.Left, area.Top, area.Width, area.Height, wide);
        return new Rectangle(Left, Top, Width, Height);
    }

    internal static Rectangle RestoreOperatorBounds(Rectangle saved, IEnumerable<Rectangle> workingAreas, Rectangle primary)
    {
        var target = primary;
        long largest = 0;
        foreach (var area in workingAreas)
        {
            var intersection = Rectangle.Intersect(area, saved);
            long overlap = (long)intersection.Width * intersection.Height;
            if (overlap > largest)
            {
                target = area;
                largest = overlap;
            }
        }
        int width = Math.Clamp(saved.Width, 1, target.Width);
        int height = Math.Clamp(saved.Height, 1, target.Height);
        return new Rectangle(Math.Clamp(saved.X, target.Left, target.Right - width),
            Math.Clamp(saved.Y, target.Top, target.Bottom - height), width, height);
    }

    internal static MonitorPlacement ResolveMedia(IReadOnlyList<MonitorDevice> displays, string itemName,
        string sessionName, MonitorPlacement audience, bool wide)
    {
        if (!audience.Enabled)
            return audience;
        var selected = FindDisplay(displays, itemName) ?? FindDisplay(displays, sessionName);
        return selected == null || string.Equals(selected.Name, audience.DeviceName, StringComparison.OrdinalIgnoreCase)
            ? audience
            : new MonitorPlacement(selected.Name, Fit(selected.Bounds, wide), true, audience.IsDual);
    }

    internal static int HeightForWidth(int width, bool wide) => Axicultus.Core.SlideAspectRatio.HeightForWidth(width, wide);

    internal static MonitorDevice FindDisplay(IEnumerable<MonitorDevice> displays, string name) =>
        displays.FirstOrDefault(display => string.Equals(display.Name, name, StringComparison.OrdinalIgnoreCase));

    internal static MonitorPlacement ResolveOutput(
        IReadOnlyList<MonitorDevice> displays, string preferredName, bool preferSecondary, bool wide,
        Point? customOrigin = null, int customWidth = 1, bool asSingle = false)
    {
        var primary = displays.FirstOrDefault(display => display.IsPrimary) ?? (displays.Count > 0 ? displays[0] : null);
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
        MonitorPlacement output, Point? customOrigin = null, int customWidth = 1, string operatorDisplayName = null)
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
        var selected = FindDisplay(candidates, preferredName) ?? (preferSecondary
            ? candidates.FirstOrDefault(display => !string.Equals(display.Name, operatorDisplayName, StringComparison.OrdinalIgnoreCase))
            : null);
        return selected != null ? new MonitorPlacement(selected.Name, Fit(selected.Bounds, wide), true) : default;
    }


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
