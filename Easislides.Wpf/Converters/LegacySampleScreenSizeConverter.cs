using System;
using System.Globalization;
using System.Windows.Data;

namespace Easislides.Wpf.Converters;

public sealed class LegacySampleScreenSizeConverter : IMultiValueConverter
{
    private const double BottomSlack = 15d;
    private const double MinimumSize = 1d;

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var panelWidth = ReadDouble(values, 0);
        var panelHeight = ReadDouble(values, 1);

        var width = Math.Floor(Math.Max(0d, panelWidth) * 18d / 20d);
        var height = Math.Floor(Math.Max(width, MinimumSize) * 3d / 4d);
        var maxHeight = Math.Floor(Math.Max(MinimumSize, panelHeight - BottomSlack));

        if (height > maxHeight)
        {
            height = maxHeight;
            width = Math.Floor(height * 4d / 3d);
        }

        width = Math.Max(MinimumSize, width);
        height = Math.Max(MinimumSize, height);

        return string.Equals(parameter as string, "Height", StringComparison.OrdinalIgnoreCase)
            ? height
            : width;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static double ReadDouble(object[] values, int index)
        => values.Length > index && values[index] is double value && !double.IsNaN(value)
            ? value
            : 0d;
}
