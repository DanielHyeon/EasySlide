using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;

namespace Easislides.Wpf.Settings;

public sealed class RegistryLegacySettingsSource : ILegacySettingsSource
{
    private const string DefaultBasePath = @"Software\EasiSlides";

    private static readonly string[] DefaultSections =
    [
        "config",
        "options",
        "monitors",
    ];

    private readonly RegistryKey _root;
    private readonly string _basePath;
    private readonly IReadOnlyList<string> _sections;

    public RegistryLegacySettingsSource()
        : this(Registry.CurrentUser, DefaultBasePath)
    {
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
        _basePath = basePath.TrimEnd('\\');
        _sections = (sections ?? DefaultSections)
            .Where(section => !string.IsNullOrWhiteSpace(section))
            .Select(section => section.Trim('\\'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public bool TryGetString(string key, out string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            value = null;
            return false;
        }

        foreach (var section in _sections)
        {
            using var registryKey = _root.OpenSubKey($@"{_basePath}\{section}", writable: false);
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
