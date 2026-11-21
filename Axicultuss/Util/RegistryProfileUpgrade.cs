using System;
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.Win32;

namespace Axicultus.Util;

internal static class RegistryProfileUpgrade
{
    internal const string CurrentPath = @"Software\Axicultuss";
    private const string PreviousPath = @"Software\Axicultus";
    private const string LegacyPath = @"Software\EasiSlides";
    private const string Imported = "SettingsProfileImportedV2";
    private const string PreviousMarker = "LegacyProfileImported";

    // Read-only migration. A completed import or reset in a newer profile takes precedence.
    internal static JsonObject Read(RegistryKey userRoot)
    {
        var document = new JsonObject(JsonProfileStore.Names);
        using var current = userRoot.OpenSubKey(CurrentPath);
        CopyMissing(current, document, "");
        if (current?.GetValue(Imported) is 1) return document;
        using var previous = userRoot.OpenSubKey(PreviousPath);
        CopyMissing(previous, document, "");
        if (previous?.GetValue(PreviousMarker) is not 1)
        {
            using var legacy = userRoot.OpenSubKey(LegacyPath);
            CopyMissing(legacy, document, "");
        }
        return document;
    }

    private static void CopyMissing(RegistryKey source, JsonObject document, string section)
    {
        if (source == null) return;
        foreach (string name in source.GetValueNames())
        {
            if (section.Length == 0 && (name.Equals(Imported, StringComparison.OrdinalIgnoreCase)
                || name.Equals(PreviousMarker, StringComparison.OrdinalIgnoreCase))) continue;
            if (document[section] is not JsonObject values) document[section] = values = new JsonObject(JsonProfileStore.Names);
            if (values.ContainsKey(name)) continue;
            var value = source.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames)
                ?? throw new IOException("Could not read a legacy setting.");
            values[name] = JsonProfileStore.Encode(value, source.GetValueKind(name));
        }
        foreach (string name in source.GetSubKeyNames())
        {
            using var child = source.OpenSubKey(name) ?? throw new IOException("Could not read a legacy settings section.");
            CopyMissing(child, document, section.Length == 0 ? name : section + "\\" + name);
        }
    }
}
