using System.Data.SQLite;

namespace Axicultus.Wpf.Library;

internal static class SqliteFile
{
    internal static SQLiteConnection Open(string path, bool readOnly = false)
    {
        var options = new SQLiteConnectionStringBuilder { DataSource = path, Version = 3, ReadOnly = readOnly };
        var connection = new SQLiteConnection(options.ConnectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
