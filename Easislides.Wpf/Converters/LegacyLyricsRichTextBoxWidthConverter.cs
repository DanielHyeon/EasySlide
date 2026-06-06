using System;
using System.Globalization;
using System.Windows.Data;

namespace Easislides.Wpf.Converters;

public sealed class LegacyLyricsRichTextBoxWidthConverter : IValueConverter
{
    private const double ScrollbarGutter = 18d;
    private const double MinimumWidth = 1d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = value switch
        {
            double d when !double.IsNaN(d) => d,
            int i => i,
            _ => 0d,
        };

        return Math.Max(MinimumWidth, Math.Floor(width - ScrollbarGutter));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
