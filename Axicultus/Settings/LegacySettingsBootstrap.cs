using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Settings;

/// <summary>
/// Reads the WinForms profile file written by JsonProfileStore (%AppData%\Axicultuss\settings.json):
/// { "section": { "name": { "kind": "DWord", "value": 1 } } }. The file is only read, never written.
/// </summary>
public sealed class WinFormsProfileSettingsSource : ILegacySettingsSource
{
    // Same lookup order as RegistryLegacySettingsSource: WinForms reads "monitors" before the old "options" copies.
    private static readonly string[] Sections = ["config", "monitors", "options"];

    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    private WinFormsProfileSettingsSource()
    {
    }

    /// <summary>Null when the file is missing or is not a settings document. Entries that cannot be read are skipped.</summary>
    public static WinFormsProfileSettingsSource? TryLoad(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;

            // WinForms may be running and replaces the file by rename: do not lock it.
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return null;

            var source = new WinFormsProfileSettingsSource();
            foreach (var wanted in Sections)
            {
                foreach (var section in document.RootElement.EnumerateObject())
                {
                    if (!string.Equals(section.Name, wanted, StringComparison.OrdinalIgnoreCase) ||
                        section.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    foreach (var entry in section.Value.EnumerateObject())
                    {
                        if (!source._values.ContainsKey(entry.Name) && TryReadValue(entry.Value, out var text))
                        {
                            source._values[entry.Name] = text;
                        }
                    }
                }
            }

            return source;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
            or System.Security.SecurityException or NotSupportedException)
        {
            return null;
        }
    }

    public bool TryGetString(string key, out string? value)
    {
        if (!string.IsNullOrWhiteSpace(key) && _values.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryReadValue(JsonElement entry, out string text)
    {
        text = "";
        if (entry.ValueKind != JsonValueKind.Object || !entry.TryGetProperty("value", out var value)) return false;
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                text = value.GetString() ?? "";
                if (entry.TryGetProperty("kind", out var kind) && kind.ValueKind == JsonValueKind.String &&
                    kind.GetString() == "ExpandString")
                {
                    text = Environment.ExpandEnvironmentVariables(text);
                }

                return true;
            case JsonValueKind.Number:
                text = value.GetRawText();
                return true;
            default:
                // MultiString and Binary entries are not settings the import reads.
                return false;
        }
    }
}

internal static class LegacySettingsBootstrap
{
    /// <summary>
    /// First run only: while the WPF settings file does not exist yet, the WinForms settings are imported once.
    /// A successful import writes <paramref name="settingsFilePath"/>, so it never runs again. Never throws:
    /// a missing or unusable source leaves the WPF defaults in place.
    /// </summary>
    internal static async Task<SettingsResult?> ImportOnFirstRunAsync(
        ISettingsService settings,
        string settingsFilePath,
        Func<ILegacySettingsSource?> openSource,
        Action<Exception>? report = null)
    {
        try
        {
            if (File.Exists(settingsFilePath)) return null;
            var source = openSource();
            return source is null ? null : await settings.MigrateLegacyAsync(source).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            report?.Invoke(ex);
            return null;
        }
    }

    /// <summary>
    /// The WinForms profile file. The registry is what WinForms used before that file existed, so it is read only
    /// when there is no file at all; a file that cannot be read gives no source and the next start tries again.
    /// </summary>
    internal static ILegacySettingsSource? OpenDefaultSource(string appData)
        => OpenDefaultSource(appData, () => new RegistryLegacySettingsSource());

    internal static ILegacySettingsSource? OpenDefaultSource(string appData, Func<RegistryLegacySettingsSource> openRegistry)
    {
        var path = Path.Combine(ProductStorage.WinFormsProfileDirectory(appData), "settings.json");
        if (File.Exists(path)) return WinFormsProfileSettingsSource.TryLoad(path);

        var registry = openRegistry();
        return registry.Exists ? registry : null;
    }
}
