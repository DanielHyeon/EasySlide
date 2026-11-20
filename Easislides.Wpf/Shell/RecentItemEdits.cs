using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Easislides.Wpf.Shell;

internal sealed record EditedItem(string Library, string Kind, string Key, string Title);

internal sealed class RecentItemEdits(string path)
{
    public IReadOnlyList<EditedItem> Read(string library, int limit = 10) => ReadAll()
        .Where(entry => string.Equals(entry.Library, library, StringComparison.OrdinalIgnoreCase)).Take(Math.Clamp(limit, 0, 20)).ToArray();

    private IReadOnlyList<EditedItem> ReadAll()
    {
        try
        {
            if (!File.Exists(path)) return [];
            return (JsonSerializer.Deserialize<List<EditedItem>>(File.ReadAllText(path)) ?? [])
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Key) && !string.IsNullOrWhiteSpace(entry.Title)).ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { return []; }
    }

    public void Record(EditedItem item, int limit = 10)
    {
        var entries = ReadAll().Where(old => !string.Equals(old.Library, item.Library, StringComparison.OrdinalIgnoreCase)
            || old.Kind != item.Kind || !string.Equals(old.Key, item.Key, StringComparison.OrdinalIgnoreCase))
            .Prepend(item).GroupBy(entry => entry.Library, StringComparer.OrdinalIgnoreCase).SelectMany(group => group.Take(Math.Clamp(limit, 0, 20)));
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(temp, JsonSerializer.Serialize(entries));
            File.Move(temp, path, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        finally { try { File.Delete(temp); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } }
    }
}
