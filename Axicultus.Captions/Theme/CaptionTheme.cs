using System;
using System.Windows;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The default look of the caption screens for a host that has no theme of its own (the WPF app has one and does not
/// call this): every colour, font and type size key the caption XAML uses, light or dark.
/// </summary>
public static class CaptionTheme
{
    /// <summary>The dictionary of the default look.</summary>
    public static ResourceDictionary Defaults(bool dark) => (ResourceDictionary)Application.LoadComponent(
        new Uri($"/Axicultus.Captions;component/Theme/CaptionTheme.{(dark ? "Dark" : "Light")}.xaml", UriKind.Relative));

    /// <summary>Gives the element (and what it holds) the default look; a look given before is replaced.</summary>
    public static void Apply(FrameworkElement element, bool dark)
    {
        var merged = element.Resources.MergedDictionaries;
        for (int i = merged.Count - 1; i >= 0; i--)
            if (merged[i].Contains(Marker)) merged.RemoveAt(i);
        var look = Defaults(dark);
        look[Marker] = dark;
        merged.Add(look);
    }

    // Marks the dictionaries this class added, so that a change of light and dark replaces them.
    private const string Marker = "Axicultus.Captions.Theme";
}
