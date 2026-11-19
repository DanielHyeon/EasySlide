using System;
using System.Drawing;
using System.Text;

namespace Easislides;

internal static class LyricLineMeasurement
{
    // Offsets in legacy slide metadata are one-based. The WPF paginator only needs the count.
    internal static int Count(string text, Font font, Graphics graphics, float width, StringBuilder? breaks = null)
    {
        int length = text.Length;
        if (length == 0)
        {
            return 0;
        }
        int lineStart = 1;
        int lastSpacePos = 0;
        bool hasSpace = false;
        int previousBreak = 1;
        int breakCount = 0;
        for (int i = 1; i <= length; i++)
        {
            if (i < length && text[i] == ' ')
            {
                lastSpacePos = i;
                hasSpace = true;
            }
            else if (graphics.MeasureString(text.AsSpan(lineStart, Math.Min(i + 1, length) - lineStart), font).Width > width)
            {
                breakCount++;
                lineStart = hasSpace ? lastSpacePos + 1 : i;
                breaks?.Append(lineStart - previousBreak).Append('>');
                previousBreak = lineStart;
                hasSpace = false;
            }
        }
        breaks?.Append(length - previousBreak + 1).Append('>');
        return breakCount + 1;
    }
}
