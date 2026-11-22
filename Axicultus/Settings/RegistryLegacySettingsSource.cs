using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;

namespace Axicultus.Wpf.Settings;

public sealed class RegistryLegacySettingsSource : ILegacySettingsSource
{
    private const string DefaultBasePath = @"Software\Axicultuss";

    private static readonly string[] DefaultSections =
    [
        // WinForms reads "monitors" first for the values it moved there from "options" (MonitorRegistrySettings).
        "config",
        "monitors",
        "options",
    ];

    private readonly RegistryKey _root;
    private readonly IReadOnlyList<string> _basePaths;
    private readonly IReadOnlyList<string> _sections;

    public RegistryLegacySettingsSource()
        : this(Registry.CurrentUser)
    {
    }

    internal RegistryLegacySettingsSource(RegistryKey root)
        : this(root, DefaultBasePath)
    {
        _basePaths = GetProfilePaths(root).ToArray();
    }

    private static IEnumerable<string> GetProfilePaths(RegistryKey root)
    {
        yield return DefaultBasePath;
        using var current = root.OpenSubKey(DefaultBasePath);
        if (current?.GetValue("SettingsProfileImportedV2") is 1) yield break;

        const string previousPath = @"Software\Axicultus";
        yield return previousPath;
        using var previous = root.OpenSubKey(previousPath);
        if (previous?.GetValue("LegacyProfileImported") is not 1) yield return @"Software\EasiSlides";
    }

    public RegistryLegacySettingsSource(
        RegistryKey root,
        string basePath,
        IEnumerable<string>? sections = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException("Registry base path cannot be empty.", nameof(basePath));
        }

        _root = root;
        _basePaths = [basePath.TrimEnd('\\')];
        _sections = (sections ?? DefaultSections)
            .Where(section => !string.IsNullOrWhiteSpace(section))
            .Select(section => section.Trim('\\'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>True when a legacy profile key is present; nothing is created.</summary>
    public bool Exists => _basePaths.Any(path =>
    {
        using var key = _root.OpenSubKey(path, writable: false);
        return key is not null;
    });

    public bool TryGetString(string key, out string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            value = null;
            return false;
        }

        foreach (var basePath in _basePaths)
        foreach (var section in _sections)
        {
            using var registryKey = _root.OpenSubKey($@"{basePath}\{section}", writable: false);
            var raw = registryKey?.GetValue(key);
            if (raw is null)
            {
                continue;
            }

            value = ConvertRegistryValue(raw);
            return true;
        }

        value = null;
        return false;
    }

    private static string ConvertRegistryValue(object raw)
        => raw switch
        {
            string[] values => string.Join(";", values),
            _ => Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty,
        };
}
