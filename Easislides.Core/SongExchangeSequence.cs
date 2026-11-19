using System;
using System.Globalization;
using System.Text;

namespace Easislides.Module;

// The WinForms database encodes each verse as one character. Exchange files use printable tokens.
public static class SongExchangeSequence
{
    public static string FromLegacy(string codes)
    {
        var text = new StringBuilder();
        foreach (int code in codes)
        {
            if (text.Length > 0) text.Append(',');
            if (code is >= 1 and <= 99)
                text.Append(code.ToString(CultureInfo.InvariantCulture));
            else
                text.Append(code switch
                {
                    0 => 'c', 100 => 'b', 101 => 'e', 102 => 't',
                    103 => 'w', 111 => 'p', 112 => 'q',
                    _ => throw new System.IO.InvalidDataException($"Unknown verse code {code}.")
                });
        }
        return text.ToString();
    }

    public static string FromWpfDatabase(string value)
    {
        // The WPF repository can also open a database written by WinForms.
        foreach (char c in value)
            if (char.IsControl(c) && !char.IsWhiteSpace(c)) return FromLegacy(value);
        return value;
    }

    public static string ToLegacy(ExchangeSong song)
    {
        var result = new StringBuilder();
        ReadOnlySpan<char> text = song.Sequence;
        foreach (var range in text.SplitAny(", \t\r\n"))
        {
            var token = text[range];
            if (token.IsEmpty) continue;
            int code;
            if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out code) || code is < 1 or > 99)
                code = token.Length == 1 ? char.ToLowerInvariant(token[0]) switch
                {
                    'c' or '0' => 0, 'b' => 100, 'e' => 101, 't' => 102,
                    'w' => 103, 'p' => 111, 'q' => 112, _ => -1
                } : -1;
            if (code < 0)
            {
                throw new System.IO.InvalidDataException($"Unsupported WinForms verse sequence in '{song.Title1}': {song.Sequence}");
            }
            result.Append((char)code);
        }
        return result.ToString();
    }
}
