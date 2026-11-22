using System.Collections.Generic;
using System.Linq;
using Axicultus.Wpf.Data;

namespace Axicultus.Wpf.Library;

/// <summary>The wording the copy, move and delete dialogs share when they act on several selected songs.</summary>
internal static class SongSelectionText
{
    private const int TitlesShown = 3;

    /// <summary>The dialog heading: the title of one song, or the count and the first titles of several.</summary>
    public static string Heading(IReadOnlyList<SongSummary> songs)
        => songs.Count switch
        {
            0 => "",
            1 => songs[0].Title,
            _ => $"{songs.Count}곡 선택: {Titles(songs.Select(song => song.Title))}",
        };

    /// <summary>The name of the folder all the songs are in, or a note that they are in several.</summary>
    public static string SourceFolderName(IReadOnlyList<SongSummary> songs, IReadOnlyList<SongFolderSummary> allFolders)
    {
        if (CommonFolderNo(songs) is not { } folderNo)
        {
            return songs.Count == 0 ? "" : "여러 폴더";
        }

        return allFolders.FirstOrDefault(folder => folder.FolderNo == folderNo)?.Name ?? $"Folder {folderNo}";
    }

    /// <summary>The folder number all the songs share; null when there is no song or they are in several folders.</summary>
    public static int? CommonFolderNo(IReadOnlyList<SongSummary> songs)
        => songs.Count > 0 && songs.All(song => song.FolderNo == songs[0].FolderNo) ? songs[0].FolderNo : null;

    /// <summary>What follows the result message when songs were left out, e.g. " 건너뜀 2곡(이미 대상 폴더에 있음): A, B".</summary>
    public static string Skipped(IReadOnlyCollection<string> titles, string reason)
        => titles.Count == 0 ? "" : $" 건너뜀 {titles.Count}곡({reason}): {Titles(titles)}";

    private static string Titles(IEnumerable<string> titles)
    {
        var all = titles.ToArray();
        var shown = string.Join(", ", all.Take(TitlesShown));
        return all.Length > TitlesShown ? $"{shown} 외 {all.Length - TitlesShown}곡" : shown;
    }
}
