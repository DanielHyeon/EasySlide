using System;
using System.Globalization;
using System.Windows.Data;
using Axicultus.Core;

namespace Axicultus.Wpf.Captions;

/// <summary>A caption breaks its lines between words, not inside a Korean word, as the output screen's lyrics do.</summary>
public sealed class CaptionWordWrapping : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string text ? LyricWordBreaks.ForDisplay(text) : value;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
