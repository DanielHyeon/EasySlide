using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Axicultus.Wpf.Rendering;

public sealed class LyricWordWrapping : IValueConverter
{
    // WPF normally permits a break between any two Hangul syllables. Give its line
    // formatter the word boundaries without changing the lyrics, indices or stored text.
    internal static string ForDisplay(string text)
    {
        StringBuilder? result = null;
        bool previousHangul = false, previousSpace = true;
        int copied = 0;
        for (int i = 0; i < text.Length;)
        {
            char c = text[i];
            bool hangul = c is >= '\uac00' and <= '\ud7a3' or >= '\u1100' and <= '\u11ff'
                or >= '\u3130' and <= '\u318f' or >= '\ua960' and <= '\ua97f' or >= '\ud7b0' and <= '\ud7ff';
            bool space = char.IsWhiteSpace(c) || c is '\u200b' or '\u2060';
            if (!space && !previousSpace && (hangul || previousHangul))
            {
                result ??= new StringBuilder(text.Length);
                result.Append(text.AsSpan(copied, i - copied)).Append('\u2060');
                copied = i;
            }
            previousHangul = hangul;
            previousSpace = space;
            i += StringInfo.GetNextTextElementLength(text.AsSpan(i));
        }
        return result?.Append(text.AsSpan(copied)).ToString() ?? text;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string text ? ForDisplay(text) : value;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
