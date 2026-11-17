using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Wpf.Shell;

/// <summary>
/// FrmMain WL_Word("Generate RTF Document") parity: converts the current Worship List into an RTF lyrics document.
/// </summary>
public sealed class WorshipListRtfExporter
{
    public string BuildRtf(string? title, IReadOnlyList<LiveQueueItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var safeTitle = EscapeRtf(string.IsNullOrWhiteSpace(title) ? "Worship List" : title.Trim());
        var builder = new StringBuilder();
        builder.Append(@"{\rtf1\ansi\ansicpg1252\deff0{\fonttbl{\f0\fnil\fcharset129 Malgun Gothic;}}");
        builder.Append(@"\viewkind4\uc1\pard\f0 ");
        builder.Append(@"\sa220\b\fs36 ").Append(safeTitle).Append(@"\b0\fs24\par ");

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var heading = $"{index + 1}. {item.Title}";
            builder.Append(@"\pard\sa80\b\fs28 ").Append(EscapeRtf(heading)).Append(@"\b0\fs22\par ");

            var body = GetDocumentBody(item);
            if (string.IsNullOrWhiteSpace(body))
            {
                builder.Append(@"\pard\fs20 ").Append(EscapeRtf(item.Kind)).Append(@"\par ");
            }
            else
            {
                AppendBody(builder, body);
            }

            builder.Append(@"\par ");
        }

        builder.Append('}');
        return builder.ToString();
    }

    private static string GetDocumentBody(LiveQueueItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Lyrics))
        {
            return item.Lyrics;
        }

        return string.IsNullOrWhiteSpace(item.ContentPath)
            ? string.Empty
            : item.ContentPath;
    }

    private static void AppendBody(StringBuilder builder, string body)
    {
        var normalized = body.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        foreach (var rawLine in normalized.Split('\n'))
        {
            var line = RemoveNotationSuffix(rawLine).TrimEnd();
            if (line.Length == 0)
            {
                builder.Append(@"\pard\fs22\par ");
                continue;
            }

            builder.Append(@"\pard\fs22 ").Append(EscapeRtf(line)).Append(@"\par ");
        }
    }

    private static string RemoveNotationSuffix(string line)
    {
        var marker = line.IndexOf('»', StringComparison.Ordinal);
        return marker >= 0 ? line[..marker].TrimEnd() : line;
    }

    private static string EscapeRtf(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 8);
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\\':
                    builder.Append(@"\\");
                    break;
                case '{':
                    builder.Append(@"\{");
                    break;
                case '}':
                    builder.Append(@"\}");
                    break;
                case '\r':
                case '\n':
                    builder.Append(' ');
                    break;
                default:
                    if (ch < 0x20)
                    {
                        builder.Append(' ');
                    }
                    else if (ch < 0x80)
                    {
                        builder.Append(ch);
                    }
                    else
                    {
                        builder.Append(@"\u").Append((short)ch).Append('?');
                    }

                    break;
            }
        }

        return builder.ToString();
    }
}
