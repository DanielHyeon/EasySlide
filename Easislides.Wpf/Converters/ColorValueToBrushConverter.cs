using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Easislides.Wpf.Converters;

public sealed class ColorValueToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = value switch
        {
            int argb => Color.FromArgb(
                (byte)((uint)argb >> 24),
                (byte)((uint)argb >> 16),
                (byte)((uint)argb >> 8),
                (byte)(uint)argb),
            string text when TryParseHexColor(text, out var parsed) => parsed,
            _ => default,
        };

        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;

    private static bool TryParseHexColor(string text, out Color color)
    {
        color = default;

        var normalized = text.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return false;
        }

        if (!normalized.StartsWith('#'))
        {
            normalized = "#" + normalized;
        }

        if (normalized.Length == 7)
        {
            normalized = "#FF" + normalized[1..];
        }

        try
        {
            if (ColorConverter.ConvertFromString(normalized) is Color parsed)
            {
                color = parsed;
                return true;
            }
        }
        catch (FormatException)
        {
        }

        return false;
    }
}
