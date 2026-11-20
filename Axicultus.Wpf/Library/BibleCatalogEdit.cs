using System;
using System.Data.SQLite;
using System.IO;

namespace Axicultus.Wpf.Library;

// Only the catalogue is copied. Verse reads and discovery continue to use the installed Bible files.
internal sealed class BibleCatalogEdit : IDisposable
{
    private readonly SQLiteConnection _source;
    private readonly SQLiteConnection _draft;
    private long _originalVersion;
    private long _draftVersion;
    internal string DraftPath { get; } = Path.Combine(Path.GetTempPath(), $"axicultus-bibles-{Guid.NewGuid():N}.db");

    internal BibleCatalogEdit(string sourcePath)
    {
        _source = Connect(sourcePath, true);
        try
        {
            _draft = Connect(DraftPath, false);
            _originalVersion = Version(_source);
            _source.BackupDatabase(_draft, "main", "main", -1, null, 0);
            _draftVersion = Version(_draft);
            using var attach = _source.CreateCommand();
            attach.CommandText = "ATTACH DATABASE @file AS draft";
            attach.Parameters.AddWithValue("@file", DraftPath);
            attach.ExecuteNonQuery();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal bool Commit()
    {
        long draftVersion = Version(_draft);
        if (_draftVersion == draftVersion) return false;
        using var transaction = _source.BeginTransaction();
        if (Version(_source, transaction) != _originalVersion)
            throw new InvalidOperationException("다른 창에서 성경 목록을 변경했습니다. 옵션을 다시 열어 주세요.");
        using var command = _source.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE main.Biblefolder AS live
            SET (NAME, SONGFOLDER, SIZE, DISPLAYORDER) = (SELECT NAME, SONGFOLDER, SIZE, DISPLAYORDER
                FROM draft.Biblefolder AS edited WHERE edited.FILENAME = live.FILENAME)
            WHERE EXISTS (SELECT 1 FROM draft.Biblefolder AS edited WHERE edited.FILENAME = live.FILENAME
                AND (edited.NAME, edited.SONGFOLDER, edited.SIZE, edited.DISPLAYORDER)
                    IS NOT (live.NAME, live.SONGFOLDER, live.SIZE, live.DISPLAYORDER));
            INSERT INTO main.Biblefolder (NAME,FILENAME,DESCRIPTION,COPYRIGHT,SONGFOLDER,SIZE,DISPLAYORDER)
            SELECT NAME,FILENAME,DESCRIPTION,COPYRIGHT,SONGFOLDER,SIZE,DISPLAYORDER FROM draft.Biblefolder AS edited
            WHERE NOT EXISTS (SELECT 1 FROM main.Biblefolder WHERE FILENAME = edited.FILENAME);
            """;
        command.ExecuteNonQuery();
        transaction.Commit();
        _draftVersion = draftVersion;
        return true;
    }

    private static SQLiteConnection Connect(string path, bool existing)
    {
        var connection = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = path, FailIfMissing = existing, Pooling = false }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static long Version(SQLiteConnection connection, SQLiteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA main.data_version";
        return Convert.ToInt64(command.ExecuteScalar());
    }

    public void Dispose()
    {
        _source.Dispose();
        _draft?.Dispose();
        try { File.Delete(DraftPath); } catch (IOException) { }
    }
}
