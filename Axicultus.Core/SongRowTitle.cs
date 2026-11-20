using System;
using System.Text.RegularExpressions;

namespace Axicultus.Core;

/// <summary>Display parts of a song title in the modern folder list.</summary>
public static class SongRowTitle
{
    private const string MediaMarker = " <#>";

    // Digits at the end, optionally followed by one bracketed mark: "136(새)", "23", "5 [B]", "7（新）".
    private static readonly Regex TrailingNumber = new Regex(
        @"^(?<title>.*?)(?<number>[0-9]+\s*(?:\([^()]*\)|\[[^\[\]]*\]|（[^（）]*）)?)$",
        RegexOptions.CultureInvariant | RegexOptions.Singleline);

    // Characters that join the trailing digits to the title: no split when one stands right before them.
    private const string NumberJoiners = ":.：．";

    public static void Split(string? text, out string title, out string? number, out bool media)
    {
        text ??= "";
        media = text.EndsWith(MediaMarker, StringComparison.Ordinal);
        if (media)
        {
            text = text.Substring(0, text.Length - MediaMarker.Length);
        }
        title = text;
        number = null;
        Match m = TrailingNumber.Match(text.TrimEnd());
        if (m.Success)
        {
            string before = m.Groups["title"].Value;
            // A number right after ':' or '.' belongs to the title ("요한복음 3:16", "Song 2.5", "1.5").
            if (before.Length > 0 && NumberJoiners.IndexOf(before[before.Length - 1]) >= 0)
            {
                return;
            }
            string head = before.TrimEnd();
            if (head.Length > 0)
            {
                title = head;
                number = m.Groups["number"].Value;
            }
        }
    }
}
