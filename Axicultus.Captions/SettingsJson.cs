using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Axicultus.Wpf.Captions;

/// <summary>The caption settings files (caption-style.json, obs.json) read item by item (a048 design 12).</summary>
internal static class SettingsJson
{
    /// <summary>
    /// The file's object as a <typeparamref name="T"/>, each item that cannot be read as T has it (null for a value,
    /// the wrong type, a number out of its type's range) left out, so that item takes its default and the others keep
    /// the file's values. An item named twice is its last value (design 15). Null when the file holds no object; a file
    /// that is not JSON throws <see cref="JsonException"/>.
    /// </summary>
    internal static T? Read<T>(string json) where T : class
    {
        using var file = JsonDocument.Parse(json);
        if (file.RootElement.ValueKind != JsonValueKind.Object) return null;
        var items = new Dictionary<string, string>();
        foreach (var item in file.RootElement.EnumerateObject()) items[item.Name] = item.Value.GetRawText();
        return JsonSerializer.Deserialize<T>(Object(items.Where(item => Readable<T>(Object([item])))));
    }

    private static string Object(IEnumerable<KeyValuePair<string, string>> items) =>
        "{" + string.Join(",", items.Select(item => JsonSerializer.Serialize(item.Key) + ":" + item.Value)) + "}";

    private static bool Readable<T>(string json)
    {
        try { JsonSerializer.Deserialize<T>(json); return true; }
        catch (JsonException) { return false; }
    }
}
