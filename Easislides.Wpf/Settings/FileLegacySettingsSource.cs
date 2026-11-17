using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Easislides.Wpf.Settings;

public sealed class CompositeLegacySettingsSource : ILegacySettingsSource
{
    private readonly IReadOnlyList<ILegacySettingsSource> _sources;

    public CompositeLegacySettingsSource(params ILegacySettingsSource[] sources)
        : this((IEnumerable<ILegacySettingsSource>)sources)
    {
    }

    public CompositeLegacySettingsSource(IEnumerable<ILegacySettingsSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.Where(source => source is not null).ToArray();
    }

    public bool TryGetString(string key, out string? value)
    {
        foreach (var source in _sources)
        {
            if (source.TryGetString(key, out value))
            {
                return true;
            }
        }

        value = null;
        return false;
    }
}

public sealed class FileLegacySettingsSource : ILegacySettingsSource
{
    private readonly IReadOnlyDictionary<string, string> _values;

    public FileLegacySettingsSource(params string[] filePaths)
        : this((IEnumerable<string>)filePaths)
    {
    }

    public FileLegacySettingsSource(IEnumerable<string> filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in filePaths
                     .Where(path => !string.IsNullOrWhiteSpace(path))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            AddFileValues(values, filePath);
        }

        _values = values;
    }

    public static FileLegacySettingsSource CreateDefault()
        => new(GetDefaultFilePaths());

    public bool TryGetString(string key, out string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            value = null;
            return false;
        }

        if (_values.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        value = null;
        return false;
    }

    private static void AddFileValues(IDictionary<string, string> values, string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        try
        {
            var extension = Path.GetExtension(filePath);
            if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
            {
                AddJsonValues(values, filePath);
                return;
            }

            if (IsXmlFile(extension, filePath))
            {
                AddXmlValues(values, filePath);
                return;
            }

            AddKeyValueValues(values, File.ReadAllLines(filePath));
        }
        catch (Exception ex) when (IsIgnoredParseException(ex))
        {
            // Legacy setting files are optional; unreadable candidates must not block startup.
        }
    }

    private static bool IsXmlFile(string extension, string filePath)
    {
        if (string.Equals(extension, ".config", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(Path.GetFileName(filePath), "user.config", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static void AddXmlValues(IDictionary<string, string> values, string filePath)
    {
        var document = XDocument.Load(filePath);

        foreach (var setting in document.Descendants().Where(element => element.Name.LocalName == "setting"))
        {
            var key = setting.Attribute("name")?.Value;
            var value = setting.Elements().FirstOrDefault(element => element.Name.LocalName == "value")?.Value;
            AddIfMissing(values, key, value);
        }

        foreach (var add in document.Descendants().Where(element => element.Name.LocalName == "add"))
        {
            var key = add.Attribute("key")?.Value ?? add.Attribute("name")?.Value;
            var value = add.Attribute("value")?.Value;
            AddIfMissing(values, key, value);
        }
    }

    private static void AddJsonValues(IDictionary<string, string> values, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var document = JsonDocument.Parse(stream);
        AddJsonElement(values, document.RootElement, prefix: null);
    }

    private static void AddJsonElement(IDictionary<string, string> values, JsonElement element, string? prefix)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrWhiteSpace(prefix) ? property.Name : $"{prefix}.{property.Name}";
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    AddJsonElement(values, property.Value, key);
                    break;
                case JsonValueKind.String:
                    AddIfMissing(values, property.Name, property.Value.GetString());
                    AddIfMissing(values, key, property.Value.GetString());
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    AddIfMissing(values, property.Name, property.Value.ToString());
                    AddIfMissing(values, key, property.Value.ToString());
                    break;
            }
        }
    }

    private static void AddKeyValueValues(IDictionary<string, string> values, IEnumerable<string> lines)
    {
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 ||
                line.StartsWith('#') ||
                line.StartsWith(';') ||
                line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
            {
                separatorIndex = line.IndexOf(':');
            }

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = Unquote(line[(separatorIndex + 1)..].Trim());
            AddIfMissing(values, key, value);
        }
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') ||
             (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }

        return value;
    }

    private static void AddIfMissing(IDictionary<string, string> values, string? key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key) || value is null || values.ContainsKey(key))
        {
            return;
        }

        values[key] = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static IEnumerable<string> GetDefaultFilePaths()
    {
        foreach (var root in GetDefaultRoots())
        {
            yield return Path.Combine(root, "settings.ini");
            yield return Path.Combine(root, "Settings.ini");
            yield return Path.Combine(root, "settings.txt");
            yield return Path.Combine(root, "legacy-settings.json");
            yield return Path.Combine(root, "user.config");
        }
    }

    private static IEnumerable<string> GetDefaultRoots()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(appData))
        {
            yield return Path.Combine(appData, "Easislides");
            yield return Path.Combine(appData, "EasiSlides");
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            yield return Path.Combine(localAppData, "Easislides");
            yield return Path.Combine(localAppData, "EasiSlides");
        }
    }

    private static bool IsIgnoredParseException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or System.Security.SecurityException
            or XmlException
            or JsonException
            or InvalidOperationException;
}
