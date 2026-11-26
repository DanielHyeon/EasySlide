using System;
using System.Globalization;
using System.Windows.Data;

namespace Axicultus.Wpf.Localization;

/// <summary>
/// An enum value shown in a list in the UI language: the text "Type.Value" of the EnumTextConverter area
/// (a combo box keeps the enum as its value). XAML: <c>Text="{Binding Converter={x:Static loc:EnumTextConverter.Instance}}"</c>.
/// </summary>
public sealed class EnumTextConverter : IValueConverter
{
    public static readonly EnumTextConverter Instance = new();

    public static string Text(Enum value) => UiText.Get(nameof(EnumTextConverter), value.GetType().Name + "." + value);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Enum item ? Text(item) : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
