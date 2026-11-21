using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Library;

internal static class PraiseBookDocument
{
    internal static string Build(string title, IReadOnlyList<PraiseBookIndexEntry> entries,
        IEnumerable<SongSummary> catalog, bool html)
    {
        var songs = catalog.Where(song => song.FolderNo > 0).ToList();
        var byId = songs.ToDictionary(song => song.SongId);
        var byTitle = songs.ToLookup(song => song.Title, StringComparer.OrdinalIgnoreCase);
        var items = new List<LiveQueueItem>(entries.Count);
        foreach (var entry in entries)
        {
            SongSummary? song;
            if (entry.SongId != 0)
                byId.TryGetValue(entry.SongId, out song);
            else
            {
                song = PraiseBookIndexEntry.MatchLegacySong(byTitle[entry.Title], entry.Number);
            }
            if (song is null)
                throw new InvalidDataException($"찬양집 곡을 식별할 수 없습니다. 곡을 다시 선택하세요: {entry.Title}");
            items.Add(new LiveQueueItem($"song:{song.SongId}", entry.Title, LiveItemKinds.Song)
                { Lyrics = LyricsDisplayFormatter.ToDisplayText(song.Lyrics) });
        }

        if (!html) return WorshipListRtfExporter.BuildRtf(title, items);
        var body = new StringBuilder("<!doctype html><html lang=\"ko\"><head><meta charset=\"utf-8\"><title>");
        var heading = WebUtility.HtmlEncode(title);
        body.Append(heading).Append("</title><style>p{white-space:pre-wrap}</style></head><body><h1>")
            .Append(heading).Append("</h1>");
        foreach (var item in items)
            body.Append("<section><h2>").Append(WebUtility.HtmlEncode(item.Title)).Append("</h2><p>")
                .Append(WebUtility.HtmlEncode(item.Lyrics)).Append("</p></section>");
        return body.Append("</body></html>").ToString();
    }
}
