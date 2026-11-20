using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Axicultus.Module;

public class ExchangeFolder
{
    public string Folder { get; init; } = "";
    [JsonIgnore]
    public string SourceFolder => NameOrDefault(Folder);

    internal static string NameOrDefault(string? name) => string.IsNullOrEmpty(name) ? "Default Folder" : name;
}

public sealed class ExchangeSong : ExchangeFolder
{
    public string Title1 { get; init; } = "";
    public string Title2 { get; init; } = "";
    public string Contents { get; init; } = "";
    public string Notations { get; init; } = "";
    public string Sequence { get; init; } = "";
    public string Writer { get; init; } = "";
    public string Copyright { get; init; } = "";
    public string Category { get; init; } = "";
    public string Timing { get; init; } = "";
    public string MusicKey { get; init; } = "";
    public string LicenceAdmin1 { get; init; } = "";
    public string LicenceAdmin2 { get; init; } = "";
    public string BookReference { get; init; } = "";
    public string UserReference { get; init; } = "";
    public string FormatData { get; init; } = "";
    public string Settings { get; init; } = "";
    public int SongNumber { get; init; }
    public int Capo { get; init; } = -1;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; init; }

    public static ExchangeSong? ReadXmlItem(XmlReader item)
    {
        item.MoveToContent();
        var element = (XElement)XNode.ReadFrom(item);
        if (!element.HasElements) return null;

        string Field(string name) => (string?)element.Element(name) ?? "";
        int Number(string name, int fallback) =>
            int.TryParse(Field(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : fallback;

        return new ExchangeSong
        {
            Folder = Field("Folder"), SongNumber = Number("SongNumber", 0), Capo = Number("Capo", -1),
            Title1 = Field("Title1"),
            Title2 = Field("Title2"),
            Contents = Field("Contents"),
            Notations = Field("Notations"),
            Sequence = Field("Sequence"),
            Writer = Field("Writer"),
            Copyright = Field("Copyright"),
            Category = Field("Category"),
            Timing = Field("Timing"),
            MusicKey = Field("MusicKey"),
            LicenceAdmin1 = Field("LicenceAdmin1"),
            LicenceAdmin2 = Field("LicenceAdmin2"),
            BookReference = Field("BookReference"),
            UserReference = Field("UserReference"),
            FormatData = Field("FormatData"),
            Settings = Field("Settings"),
            Image = (string?)element.Element("Image")
        };
    }
}

public static class SongExchangeFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        RespectNullableAnnotations = true
    };

    public static bool IsReadError(Exception ex) =>
        ex is IOException or InvalidDataException or UnauthorizedAccessException or XmlException or JsonException;

    public static bool IsJson(string path) =>
        string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<ExchangeSong> ReadSongs(string path)
    {
        using var input = File.OpenRead(path);
        foreach (var song in ReadSongs(input, IsJson(path))) yield return song;
    }

    public static IEnumerable<ExchangeSong> ReadSongs(FileStream input, bool json) =>
        json ? ReadJson<ExchangeSong>(input) : ReadXml(input, ExchangeSong.ReadXmlItem);

    public static Dictionary<string, int> CountFolders(string path, StringComparer comparer)
    {
        var counts = new Dictionary<string, int>(comparer);
        using var input = File.OpenRead(path);
        var folders = IsJson(path)
            ? ReadJson<ExchangeFolder>(input).Select(entry => entry.SourceFolder)
            : ReadXml(input, ReadXmlFolder);
        foreach (string folder in folders)
        {
            counts[folder] = counts.GetValueOrDefault(folder) + 1;
        }
        return counts;
    }

    private static string? ReadXmlFolder(XmlReader item)
    {
        if (item.IsEmptyElement)
        {
            item.Read();
            return null;
        }
        item.ReadStartElement();
        bool hasFields = false;
        string? folder = null;
        while (item.MoveToContent() != XmlNodeType.EndElement && !item.EOF)
        {
            if (item.NodeType != XmlNodeType.Element)
            {
                item.Read();
                continue;
            }
            hasFields = true;
            if (folder is null && item.Name == "Folder" && item.NamespaceURI.Length == 0)
                folder = ((XElement)XNode.ReadFrom(item)).Value;
            else
                item.Skip();
        }
        item.ReadEndElement();
        return hasFields ? ExchangeFolder.NameOrDefault(folder) : null;
    }

    private static IEnumerable<T> ReadXml<T>(FileStream input, Func<XmlReader, T?> project) where T : class
    {
        using var reader = XmlReader.Create(input);
        if (reader.MoveToContent() != XmlNodeType.Element || !ProductStorage.IsDocumentRoot(reader.LocalName))
            throw new InvalidDataException("XML root is not Axicultus.");

        reader.Read();
        while (!reader.EOF)
        {
            if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Item" && reader.NamespaceURI.Length == 0)
                {
                    var result = project(reader);
                    if (result is not null) yield return result;
                }
                else reader.Skip();
            }
            else reader.Read();
        }
    }

    private static IEnumerable<T> ReadJson<T>(FileStream stream) where T : class
    {
        long start = stream.Position;
        // DeserializeAsyncEnumerable also accepts a root null as an empty sequence.
        // A song file must have an array root, including when it contains no songs.
        int first = stream.ReadByte();
        if (first == 0xef && stream.ReadByte() == 0xbb && stream.ReadByte() == 0xbf) first = stream.ReadByte();
        while (first is ' ' or '\t' or '\r' or '\n') first = stream.ReadByte();
        if (first != '[') throw new InvalidDataException("Expected a JSON array of songs.");
        stream.Position = start;
        foreach (var entry in JsonSerializer.DeserializeAsyncEnumerable<T>(stream, JsonOptions).ToBlockingEnumerable())
            yield return entry ?? throw new InvalidDataException("Expected a song object in the JSON array.");
    }

    public static void WriteJson(string path, IEnumerable<ExchangeSong> songs)
    {
        string temporary = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, Path.GetRandomFileName());
        var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        try
        {
            using (stream) JsonSerializer.Serialize(stream, songs, JsonOptions);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }
}
