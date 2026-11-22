using System;
using System.Globalization;
using System.Text;

namespace Axicultus.Module;

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

    // SONG.SEQUENCE holds verse codes, but an older WPF build stored the tokens as text ("1,c,2"), and the operator
    // types tokens and verse headings. A value is read as verse codes when:
    //  - it has a character below 32: a chorus (code 0) or a verse 1 to 31, which text never contains
    //    (blank text with a space in it is an empty sequence), or
    //  - it is not token text and is spelled with d e f g o p only: bridge, ending, chorus 2, bridge 2, prechorus
    //    and prechorus 2 without a verse ("de" is bridge + ending).
    // Anything else is text and comes back unchanged, so a decoded value can be decoded again
    // (LiveQueueItem does that on every copy), and a sequence of verse headings ("V1 C V2") is left alone.
    // What stays ambiguous: verse codes that use no chorus and no verse below 32 and are not such a d-p word are
    // printable, and are read as text. As codes "1" is verse 49, "12" verses 49 and 50, "p" prechorus 2; they are
    // read as verse 1, verse 12 and prechorus. Only "e" (ending) means the same either way.
    public static string FromWpfDatabase(string value)
    {
        if (!IsVerseCodes(value)) return value;
        try
        {
            return FromLegacy(value);
        }
        catch (System.IO.InvalidDataException)
        {
            // An unknown code. With a control character the value cannot be text either: the caller decides.
            foreach (char c in value)
                if (char.IsControl(c) && !char.IsWhiteSpace(c)) throw;
            return value;
        }
    }

    // SONG.SEQUENCE as WinForms reads it: the verse codes of typed tokens. A value FromWpfDatabase reads as verse
    // codes is one already and passes through (a legacy "\te" is verse 9 + ending, not the token "e"). Text stays
    // text when it names something the codes cannot (a verse heading), and when FromWpfDatabase would read its
    // codes as other verses (a sequence of verses 32 and above only, or "q" alone): the song editor refuses those.
    public static string ToDatabase(string value)
    {
        if (IsVerseCodes(value) || !IsTokenText(value)) return value;
        string codes = ToLegacy(new ExchangeSong { Sequence = value });
        string read = FromWpfDatabase(codes);
        return IsTokenText(read) && ToLegacy(new ExchangeSong { Sequence = read }) == codes ? codes : value;
    }

    private static bool IsVerseCodes(string value)
    {
        foreach (char c in value)
            if (c < ' ') return !(string.IsNullOrWhiteSpace(value) && value.Contains(' '));
        return value.Length > 0 && !IsTokenText(value) && value.AsSpan().IndexOfAnyExcept("defgop") < 0;
    }

    // True when every token names a verse (also for no token at all).
    private static bool IsTokenText(string value)
    {
        ReadOnlySpan<char> text = value;
        foreach (var range in text.SplitAny(", \t\r\n"))
            if (!text[range].IsEmpty && TokenCode(text[range]) < 0) return false;
        return true;
    }

    private static int TokenCode(ReadOnlySpan<char> token)
    {
        if (int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out int code) && code is >= 1 and <= 99)
            return code;
        return token.Length == 1 ? char.ToLowerInvariant(token[0]) switch
        {
            'c' or '0' => 0, 'b' => 100, 'e' => 101, 't' => 102,
            'w' => 103, 'p' => 111, 'q' => 112, _ => -1
        } : -1;
    }

    public static string ToLegacy(ExchangeSong song)
    {
        var result = new StringBuilder();
        ReadOnlySpan<char> text = song.Sequence;
        foreach (var range in text.SplitAny(", \t\r\n"))
        {
            var token = text[range];
            if (token.IsEmpty) continue;
            int code = TokenCode(token);
            if (code < 0)
            {
                throw new System.IO.InvalidDataException($"Unsupported WinForms verse sequence in '{song.Title1}': {song.Sequence}");
            }
            result.Append((char)code);
        }
        return result.ToString();
    }
}
