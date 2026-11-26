using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Axicultus.Localization;

/// <summary>
/// An app's UI texts, one resource file per area (a window, a dialog, a view model): "&lt;baseNamespace&gt;.&lt;area&gt;"
/// in the app's assembly, English neutral and Korean in the ko satellite. Looked up in the UI culture
/// (<see cref="UiLanguage"/>). Each app wraps one in its own static UiText.
/// </summary>
public sealed class UiTextCatalog(Assembly assembly, string baseNamespace)
{
    private readonly ConcurrentDictionary<string, ResourceManager> _areas = new(StringComparer.Ordinal);

    /// <summary>The resources of one area (they may not exist: <see cref="Find"/> then gives null).</summary>
    public ResourceManager Manager(string area) =>
        _areas.GetOrAdd(area, name => new ResourceManager(baseNamespace + "." + name, assembly));

    /// <summary>The text, or null when the area or the key has none.</summary>
    public string? Find(string area, string key)
    {
        try { return Manager(area).GetString(key, CultureInfo.CurrentUICulture); }
        catch (MissingManifestResourceException) { return null; }
    }

    /// <summary>The text; a missing one shows as [area.key] (a test finds every key used).</summary>
    public string Get(string area, string key) => Find(area, key) ?? $"[{area}.{key}]";

    /// <summary>The text with its {0}, {1}… filled in.</summary>
    public string Format(string area, string key, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Get(area, key), args);
}
