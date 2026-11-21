using System;
using System.Globalization;
using System.Windows.Data;

namespace Axicultus.Wpf.Converters;

public sealed class LegacyPowerPointThumbnailSizeConverter : IValueConverter
{
    private const double MinimumWidth = 80d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var panelWidth = value switch
        {
            double d => d,
            int i => i,
            _ => 0d,
        };

        if (parameter is "Console")
        {
            // The right-hand panes may be narrowed independently. Use the scrollable
            // viewport; each slide button also occupies a 2 DIP margin on either side.
            const double buttonMargins = 4;
            const double readableSlideWidth = 160;
            var columns = Math.Clamp(Math.Floor(panelWidth / (readableSlideWidth + buttonMargins)), 1, 3);
            return Math.Max(0, Math.Floor(panelWidth / columns) - buttonMargins);
        }

        var width = Math.Max(MinimumWidth, Math.Floor((panelWidth - 35d) / 3d));
        return string.Equals(parameter as string, "Height", StringComparison.OrdinalIgnoreCase)
            ? Math.Floor(width * 3d / 4d)
            : width;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
