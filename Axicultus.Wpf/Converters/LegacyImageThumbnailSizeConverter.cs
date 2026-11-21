using System;
using System.Globalization;
using System.Windows.Data;

namespace Axicultus.Wpf.Converters;

public sealed class LegacyImageThumbnailSizeConverter : IValueConverter
{
    private const double MinimumWidth = 46d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var panelWidth = value switch
        {
            double d => d,
            int i => i,
            _ => 0d,
        };

        var size = GetThumbnailSize(panelWidth);
        return string.Equals(parameter as string, "Height", StringComparison.OrdinalIgnoreCase) ? size.Height : size.Width;
    }

    internal static System.Windows.Size GetThumbnailSize(double panelWidth)
    {
        var width = Math.Max(MinimumWidth, Math.Floor((panelWidth - 34d) / 3d));
        return new(width, Math.Floor(width * 3d / 4d));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
