using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axicultus.Wpf.Shell;

internal static class AlertHistory
{
    internal const int Limit = 20;
    private static string HistoryPath(string directory, bool parental) =>
        Path.Combine(directory, parental ? "Parental.txt" : "Alerts.txt");

    internal static string[] Read(string directory, bool parental)
    {
        if (directory.Length == 0) return [];
        try
        {
            return [.. File.ReadLines(HistoryPath(directory, parental)).Select(line => line.Trim())
                .Where(line => line.Length > 0).Distinct(StringComparer.Ordinal).Take(Limit)];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return []; }
    }

    internal static bool Save(string directory, bool parental, IEnumerable<string> entries)
    {
        if (directory.Length == 0) return false;
        string target = HistoryPath(directory, parental);
        string temp = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllLines(temp, entries.Take(Limit), Encoding.Unicode);
            File.Move(temp, target, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
        finally
        {
            try { File.Delete(temp); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }
}
