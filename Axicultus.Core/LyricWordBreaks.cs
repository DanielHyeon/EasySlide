using System;
using System.Globalization;
using System.Text;

namespace Axicultus.Core;

/// <summary>
/// Lyrics as they are drawn: a line breaks between words, not inside a Korean word. Used by the WPF app's
/// LyricWordWrapping converter and by the caption library's camera caption.
/// </summary>
public static class LyricWordBreaks
{
    // WPF normally permits a break between any two Hangul syllables. Give its line
    // formatter the word boundaries without changing the lyrics, indices or stored text.
    public static string ForDisplay(string text)
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
}
