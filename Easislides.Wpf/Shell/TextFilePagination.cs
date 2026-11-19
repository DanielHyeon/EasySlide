using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Easislides.Wpf.Shell;

internal sealed record TextFileLayout(double Width, double Height, string FontFamily, double FontSize, double LineHeight);

internal static class TextFilePagination
{
    internal static IReadOnlyList<string> Create(string? lyrics, string? sequence, TextFileLayout? layout)
    {
        var paragraphs = LyricsDisplayFormatter.ToVersePages(lyrics, sequence);
        if (layout is null) return paragraphs;

        List<string> pages = [];
        var typeface = new Typeface(string.IsNullOrWhiteSpace(layout.FontFamily) ? "Segoe UI" : layout.FontFamily);
        foreach (string paragraph in paragraphs)
        {
            if (Fits(paragraph))
            {
                pages.Add(paragraph);
                continue;
            }

            var offsets = StringInfo.ParseCombiningCharacters(paragraph);
            int start = 0;
            while (start < offsets.Length)
            {
                int low = start + 1, high = offsets.Length;
                while (low < high)
                {
                    int middle = low + (high - low + 1) / 2;
                    if (Fits(paragraph[offsets[start]..End(middle)])) low = middle;
                    else high = middle - 1;
                }

                int end = low;
                if (end < offsets.Length && !char.IsWhiteSpace(paragraph[offsets[end]]))
                {
                    // Prefer a word boundary. Unbroken CJK text still advances by whole text elements.
                    for (int boundary = end; boundary > start; boundary--)
                    {
                        if (!char.IsWhiteSpace(paragraph[offsets[boundary - 1]])) continue;
                        end = boundary;
                        break;
                    }
                }
                string page = paragraph[offsets[start]..End(end)].TrimEnd();
                if (page.Length > 0) pages.Add(page);
                start = end;
                while (start < offsets.Length && char.IsWhiteSpace(paragraph[offsets[start]])) start++;
            }

            int End(int index) => index < offsets.Length ? offsets[index] : paragraph.Length;
        }
        return pages;

        bool Fits(string text)
        {
            var measured = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                typeface, layout.FontSize, Brushes.Black, 1)
            {
                MaxTextWidth = Math.Clamp(layout.Width, 1, 100000),
                LineHeight = layout.LineHeight,
            };
            return measured.Height <= layout.Height;
        }
    }
}
