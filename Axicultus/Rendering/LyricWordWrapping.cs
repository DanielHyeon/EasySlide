using System;
using System.Globalization;
using System.Windows.Data;

namespace Axicultus.Wpf.Rendering;

public sealed class LyricWordWrapping : IValueConverter
{
    // WPF normally permits a break between any two Hangul syllables. Give its line
    // formatter the word boundaries without changing the lyrics, indices or stored text (Axicultus.Core.LyricWordBreaks).
    internal static string ForDisplay(string text) => Axicultus.Core.LyricWordBreaks.ForDisplay(text);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string text ? ForDisplay(text) : value;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
