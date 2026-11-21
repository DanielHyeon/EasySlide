using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axicultus.Wpf.Data;

namespace Axicultus.Wpf.Library;

internal static class FolderListing
{
    internal static string TabSeparated(IEnumerable<SongSummary> songs, string separators)
    {
        var result = new StringBuilder("Song Title\tWriter\tMP\tSF\tTS\r\n");
        var delimiters = separators.ToCharArray();
        foreach (var song in songs)
        {
            string? mp = null, sf = null, ts = null;
            foreach (var token in song.BookReference.Split(delimiters, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (mp is null && token.StartsWith("MP", StringComparison.OrdinalIgnoreCase)) mp = token[2..].Trim();
                else if (sf is null && token.StartsWith("SF", StringComparison.OrdinalIgnoreCase)) sf = token[2..].Trim();
                else if (ts is null && token.StartsWith("TS", StringComparison.OrdinalIgnoreCase)) ts = token[2..].Trim();
            }
            result.Append(Clean(song.Title)).Append('\t').Append(Clean(song.Writer)).Append('\t')
                .Append(Clean(mp)).Append('\t').Append(Clean(sf)).Append('\t').Append(Clean(ts)).Append("\r\n");
        }
        static string? Clean(string? value) => value?.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        return result.ToString();
    }
}
