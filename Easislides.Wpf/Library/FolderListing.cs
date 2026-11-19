using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

internal static class FolderListing
{
    internal static string TabSeparated(IEnumerable<SongSummary> songs, string separators)
    {
        var result = new StringBuilder("Song Title\tWriter\tMP\tSF\tTS\r\n");
        foreach (var song in songs)
        {
            var references = song.BookReference.Split(separators.ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string Number(string prefix) => references.FirstOrDefault(r => r.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))?[prefix.Length..].Trim() ?? "";
            result.AppendJoin('\t', new[] { song.Title, song.Writer, Number("MP"), Number("SF"), Number("TS") }
                .Select(value => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '))).Append("\r\n");
        }
        return result.ToString();
    }
}
