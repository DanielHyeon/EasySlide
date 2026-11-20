using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace Axicultus;

internal sealed record BibleVersion(long? RowId, string Name, string FileName, string Description,
    string Copyright, int SongFolder, int FontSize);

internal sealed class BibleVersionCatalog
{
    public static BibleVersionCatalog Empty { get; } = new([]);

    private readonly List<BibleVersion> versions;
    private readonly Dictionary<string, int> indexByFileName;

    private BibleVersionCatalog(List<BibleVersion> versions)
    {
        this.versions = versions;
        indexByFileName = new(versions.Count, StringComparer.Ordinal);
        for (int index = 0; index < versions.Count; index++)
            indexByFileName.TryAdd(versions[index].FileName, index);
    }

    public int Count => versions.Count;
    public BibleVersion this[int index] => versions[index];
    public bool ContainsIndex(int index) => index >= 0 && index < Count;
    public int IndexOfFile(string fileName) => indexByFileName.GetValueOrDefault(fileName ?? "", -1);
    public List<BibleVersion>.Enumerator GetEnumerator() => versions.GetEnumerator();

    public static BibleVersionCatalog Load(string connectionString)
    {
        var settings = new SQLiteConnectionStringBuilder(connectionString) { ReadOnly = true, FailIfMissing = true };
        using var connection = new SQLiteConnection(settings.ToString());
        connection.Open();
        return new(ReadVersions(connection, null));
    }

    private static List<BibleVersion> ReadVersions(SQLiteConnection connection, SQLiteTransaction transaction)
    {
        using var select = new SQLiteCommand("""
            SELECT rowid, NAME, FILENAME, DESCRIPTION, COPYRIGHT, CAST(SONGFOLDER AS TEXT), CAST(SIZE AS TEXT)
            FROM Biblefolder WHERE DISPLAYORDER >= 0 AND NAME IS NOT NULL ORDER BY DISPLAYORDER, NAME, rowid
            """, connection, transaction);
        using var reader = select.ExecuteReader();
        List<BibleVersion> versions = [];
        while (reader.Read())
        {
            int folder = int.TryParse(reader[5].ToString(), out var parsedFolder) && parsedFolder is > 0 and < Gf.MAXSONGSFOLDERS ? parsedFolder : 1;
            int size = int.TryParse(reader[6].ToString(), out var parsedSize) && parsedSize is >= 5 and <= 200 ? parsedSize : 80;
            versions.Add(new(reader.GetInt64(0), reader.GetString(1), reader[2].ToString(),
                reader[3].ToString(), reader[4].ToString(), folder, size));
        }
        return versions;
    }

    public void Save(string connectionString, IReadOnlyList<BibleVersion> edited)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        if (!versions.SequenceEqual(ReadVersions(connection, transaction)))
            throw new DBConcurrencyException("The Bible list has changed. Reopen Options to load the current list.");

        var retained = edited.Where(version => version.RowId.HasValue).ToDictionary(version => version.RowId.Value);
        using var delete = Command("DELETE FROM Biblefolder WHERE rowid=@id", "@id");
        using var rename = Command("UPDATE Biblefolder SET NAME=@name WHERE rowid=@id", "@id", "@name");
        foreach (var original in versions)
        {
            if (!retained.TryGetValue(original.RowId.Value, out var version))
            {
                delete.Parameters["@id"].Value = original.RowId.Value;
                delete.ExecuteNonQuery();
            }
            else if (original.Name != version.Name)
            {
                // Free the old primary key before assigning final names (two versions can swap names).
                rename.Parameters["@id"].Value = original.RowId.Value;
                rename.Parameters["@name"].Value = Guid.NewGuid().ToString("N");
                rename.ExecuteNonQuery();
            }
        }

        Dictionary<string, long> hiddenFiles = [with(StringComparer.Ordinal)];
        if (retained.Count < edited.Count)
        {
            using var hidden = Command("SELECT FILENAME, rowid FROM Biblefolder WHERE DISPLAYORDER < 0 ORDER BY rowid");
            using var reader = hidden.ExecuteReader();
            while (reader.Read())
                hiddenFiles.TryAdd(reader[0].ToString(), reader.GetInt64(1));
        }

        using var update = Command("""
            UPDATE Biblefolder SET NAME=@name, SONGFOLDER=@folder, SIZE=@size, DISPLAYORDER=@position WHERE rowid=@id
            """, "@name", "@folder", "@size", "@position", "@id");
        using var insert = Command("""
            INSERT INTO Biblefolder(NAME,SONGFOLDER,SIZE,DISPLAYORDER,FILENAME,DESCRIPTION,COPYRIGHT)
            VALUES (@name,@folder,@size,@position,@file,@description,@copyright)
            """, "@name", "@folder", "@size", "@position", "@file", "@description", "@copyright");
        for (int position = 0; position < edited.Count; position++)
        {
            var version = edited[position];
            long? rowId = version.RowId;
            if (rowId == null && hiddenFiles.Remove(version.FileName, out long hiddenId))
                rowId = hiddenId;
            var command = rowId.HasValue ? update : insert;
            command.Parameters["@name"].Value = version.Name;
            command.Parameters["@folder"].Value = version.SongFolder;
            command.Parameters["@size"].Value = version.FontSize;
            command.Parameters["@position"].Value = position;
            if (rowId is long id)
                command.Parameters["@id"].Value = id;
            else
            {
                command.Parameters["@file"].Value = version.FileName;
                command.Parameters["@description"].Value = version.Description;
                command.Parameters["@copyright"].Value = version.Copyright;
            }
            if (command.ExecuteNonQuery() != 1)
                throw new DataException($"Could not save Bible version '{version.Name}'.");
        }
        transaction.Commit();

        SQLiteCommand Command(string sql, params string[] parameterNames)
        {
            var command = new SQLiteCommand(sql, connection, transaction);
            foreach (string name in parameterNames)
                command.Parameters.Add(new SQLiteParameter(name));
            return command;
        }
    }
}
