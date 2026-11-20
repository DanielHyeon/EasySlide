using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Axicultus.SQLite;

internal static class SongQueries
{
    internal static int? FindImportSongId(SQLiteConnection connection, int folder, string title)
    {
        using var command = DbController.CreateCommand(connection,
            "SELECT SONGID FROM SONG WHERE FolderNo=@folder AND Title_1=@title LIMIT 1",
            ("@folder", folder), ("@title", title ?? ""));
        object value = command.ExecuteScalar();
        return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
    }

    internal static DataRow FindListEntry(SQLiteConnection connection, bool useId, string id, string title, int folder)
    {
        const string select = "SELECT SongID, FolderNo, Title_1, Title_2, song_number, LICENCE_ADMIN1, LICENCE_ADMIN2 FROM SONG WHERE ";
        return useId
            ? DbController.GetDataRowScalar(connection, select + "SongID=@id AND FolderNo>0 LIMIT 1", ("@id", id))
            : DbController.GetDataRowScalar(connection, select + "lower(Title_1) LIKE @title AND FolderNo=@folder LIMIT 1",
                ("@title", title.ToLower()), ("@folder", folder));
    }

    internal readonly record struct MergePair(long SourceId, string SourceTitle, long TargetId, string TargetTitle);

    internal static IEnumerable<MergePair> FindMergePairs(SQLiteConnection connection, int sourceFolder, bool sourcePrimary,
        int targetFolder, bool targetPrimary)
    {
        string sourceTitle = sourcePrimary ? "a.Title_1" : "a.Title_2";
        string targetTitle = targetPrimary ? "b.Title_1" : "b.Title_2";
        using var command = DbController.CreateCommand(connection, $"""
            SELECT a.SongID AS SourceId, a.Title_1 AS SourceTitle, b.SongID AS TargetId, b.Title_1 AS TargetTitle, {sourceTitle} AS MatchTitle
            FROM SONG a JOIN SONG b ON lower({targetTitle}) LIKE lower({sourceTitle})
            WHERE a.FolderNo=@source AND b.FolderNo=@target AND trim({sourceTitle}) <> ''
            """, ("@source", sourceFolder), ("@target", targetFolder));
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(reader[4]))) continue;
            yield return new MergePair(reader.GetInt64(0), Convert.ToString(reader[1]), reader.GetInt64(2), Convert.ToString(reader[3]));
        }
    }
}
