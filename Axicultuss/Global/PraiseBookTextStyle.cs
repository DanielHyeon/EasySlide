using System;
using System.Drawing;
using System.Text;

namespace Axicultus;

internal struct PraiseBookTextStyle
{
    internal const int PartCount = 8;
    private const int CopyrightPart = 2;
    private const int FirstReferencePart = 6;

    internal bool Visible;
    internal bool Bold;
    internal bool Italic;
    internal bool Underline;
    internal int FontSize;
    internal Color Colour;

    private static int HeaderKey(int part) => 101 + part * 5;

    internal static void ReadHeader(ReadOnlySpan<string> header, Span<PraiseBookTextStyle> styles, Color defaultColour)
    {
        for (int part = 0; part < styles.Length; part++)
        {
            int key = HeaderKey(part);
            int flags = Gf.ExtractNumericData(header[key]);
            ref PraiseBookTextStyle style = ref styles[part];
            if (part >= FirstReferencePart)
            {
                // Both reference lines use the copyright font and have their own visibility flags.
                style = styles[CopyrightPart];
            }
            else
            {
                int fontSize = Gf.ExtractNumericData(header[key + 1]);
                style.FontSize = fontSize is >= 4 and <= 72 ? fontSize : part switch
                {
                    0 => 13,
                    CopyrightPart => 6,
                    _ => 11
                };
                style.Colour = header[key + 2] == "" ? defaultColour : Color.FromArgb(Gf.ExtractNumericData(header[key + 2]));
                style.Bold = flags >= 0 && (flags & 2) != 0;
                style.Italic = flags >= 0 && (flags & 4) != 0;
                style.Underline = flags >= 0 && (flags & 8) != 0;
            }
            style.Visible = flags < 0 || (flags & 1) != 0;
        }
    }

    internal readonly void AppendToHeader(StringBuilder header, int part)
    {
        int key = HeaderKey(part);
        int flags = (Visible ? 1 : 0) | (Bold ? 2 : 0) | (Italic ? 4 : 0) | (Underline ? 8 : 0);
        header.Append(key).Append('=').Append(flags).Append('>');
        header.Append(key + 1).Append('=').Append(FontSize).Append('>');
        header.Append(key + 2).Append('=').Append(Colour.ToArgb()).Append('>');
    }

    internal readonly string ToRtf(int part)
    {
        string bold = Bold ? @"\b" : @"\b0";
        string italic = Italic ? @"\i" : @"\i0";
        string underline = Underline ? @"\ul" : @"\ulnone";
        return $@"\fs{FontSize * 2}\cf{part + 1}{bold}{italic}{underline} ";
    }
}
