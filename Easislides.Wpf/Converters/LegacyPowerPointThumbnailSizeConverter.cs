using System;
using System.Globalization;
using System.Windows.Data;

namespace Easislides.Wpf.Converters;

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

        var width = Math.Max(MinimumWidth, Math.Floor((panelWidth - 35d) / 3d));
        return string.Equals(parameter as string, "Height", StringComparison.OrdinalIgnoreCase)
            ? Math.Floor(width * 3d / 4d)
            : width;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
