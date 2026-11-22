using System;
using System.Windows;
using System.Windows.Controls;

namespace Axicultus.Wpf.Captions;

/// <summary>Keeps the caption area a fixed size: long text shrinks its font instead of growing the box.</summary>
internal static class CaptionFit
{
    public const double Spacing = 1.28;

    /// <summary>
    /// Sizes <paramref name="area"/> for <paramref name="lines"/> lines and shrinks <paramref name="text"/> until it fits.
    /// Text that still does not fit at the smallest readable size grows the area rather than being cut.
    /// </summary>
    public static void Apply(TextBlock text, FrameworkElement area, double width, double baseSize, int lines = 3)
    {
        if (!(width > 0) || !(baseSize > 0)) return; // also rejects NaN before the first layout
        double height = baseSize * Spacing * Math.Clamp(lines, 1, 3);
        for (double size = baseSize; ; size *= .94)
        {
            text.FontSize = size;
            text.LineHeight = size * Spacing;
            text.Measure(new Size(width, double.PositiveInfinity));
            if (text.DesiredSize.Height <= height + .5 || size < baseSize * .35) break;
        }
        var padding = area is Border border ? border.Padding.Top + border.Padding.Bottom : 0;
        area.Height = Math.Max(height, text.DesiredSize.Height) + padding;
    }
}
