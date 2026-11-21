using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Axicultus.Module;

public sealed record ServiceListEntry(string ItemId, string Title, string Folder, string FormatData);

/// <summary>Shared on-disk contract for .esw and .esp. Item order, reference spelling and formatting are significant.</summary>
public sealed record ServiceListDocument(int Version, string? SystemId, string? FormatData, string? Notes,
    IReadOnlyList<ServiceListEntry> Items)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegacyText { get; init; }

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static bool IsListPath(string path) =>
        Path.GetExtension(path).Equals(".esw", StringComparison.OrdinalIgnoreCase) ||
        Path.GetExtension(path).Equals(".esp", StringComparison.OrdinalIgnoreCase) ||
        Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase);

    public static ServiceListDocument ReadJson(string text)
    {
        var document = JsonSerializer.Deserialize<ServiceListDocument>(text, Options);
        if (document is not { Version: 1, Items: not null } ||
            document.Items.Any(i => i is not { ItemId: not null, Title: not null, Folder: not null, FormatData: not null }))
            throw new InvalidDataException("Unsupported or incomplete service list.");
        return document;
    }

    public static ServiceListDocument ReadXml(string text)
    {
        var root = XDocument.Parse(text).Root;
        if (root is null || !ProductStorage.IsDocumentRoot(root.Name.LocalName) ||
            root.Element("ListItem") is not { } list)
            throw new InvalidDataException("Missing service list header.");
        var header = list.Element("ListHeader");
        return new(1, (string?)header?.Element("SystemID"), (string?)header?.Element("FormatData"),
            (string?)header?.Element("Notes"), list.Elements("Item").Select(i => new ServiceListEntry(
                (string?)i.Element("ItemID") ?? "", (string?)i.Element("Title1") ?? "",
                (string?)i.Element("Folder") ?? "", (string?)i.Element("FormatData") ?? "")).ToArray());
    }

    public static ServiceListDocument Read(string text) =>
        text.AsSpan().TrimStart().StartsWith("{") ? ReadJson(text) : ReadXml(text);

    // Per-source receipts keep deletions and renames in the new app independent of the older installation.
    public static string WorkingDirectory(string legacyDirectory, string extension)
    {
        if (extension is not (".esw" or ".esp")) throw new ArgumentException("Invalid list extension.", nameof(extension));
        string directory = Path.Combine(legacyDirectory, "Json");
        string receipts = Path.Combine(directory, ".imported");
        Directory.CreateDirectory(receipts);
        foreach (string source in Directory.EnumerateFiles(legacyDirectory, "*" + extension))
        {
            string receipt = Path.Combine(receipts, Path.GetFileName(source));
            if (File.Exists(receipt)) continue;
            string destination = Path.Combine(directory, Path.GetFileNameWithoutExtension(source) + ".json");
            try
            {
                if (!File.Exists(destination))
                {
                    string original = File.ReadAllText(source);
                    var start = original.AsSpan().TrimStart();
                    var document = start.StartsWith("<") || start.StartsWith("{") ? Read(original)
                        : new ServiceListDocument(1, null, null, null, []) { LegacyText = original };
                    document.Write(destination, overwrite: false);
                }
                using var marker = File.Open(receipt, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Xml.XmlException or JsonException)
            {
                System.Diagnostics.Trace.WriteLine($"Cannot import service list {source}: {ex.Message}");
            }
        }
        return directory;
    }

    public void Save(string path) => Write(path, overwrite: true);

    private void Write(string path, bool overwrite)
    {
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            if (File.Exists(path) && overwrite)
                ReadJson(File.ReadAllText(path)); // Leave damaged or newer documents available for recovery.
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, this, Options);
                stream.Flush(true);
            }
            File.Move(temp, path, overwrite);
        }
        finally { File.Delete(temp); }
    }
}
