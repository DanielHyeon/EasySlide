using System;
using System.Linq;
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

    /// <summary>Gives the caption tab, its tab strip and its settings window (each placed apart by the host) the default look.</summary>
    public static void Apply(CaptionConsole console, bool dark)
    {
        Apply((FrameworkElement)console, dark);
        Apply(console.Strip, dark);
        Apply(console.Settings, dark);
        WriteCheckBoxesInTheTextColour(console.Settings);
    }

    // The settings window's check boxes keep the text colour of the control theme under them: the WPF app's writes it in
    // its own colours, the system's black, which the dark look cannot show. With the default look they write it in the
    // look's text colour, as the caption tab's own check boxes do. (Once: a change of light and dark keeps it.)
    private static void WriteCheckBoxesInTheTextColour(Window settings)
    {
        if (settings.Resources[typeof(System.Windows.Controls.CheckBox)] is not Style local
            || local.Setters.OfType<Setter>().Any(setter => setter.Property == System.Windows.Controls.Control.ForegroundProperty)) return;
        settings.Resources[typeof(System.Windows.Controls.CheckBox)] = new Style(typeof(System.Windows.Controls.CheckBox), local)
        {
            Setters = { new Setter(System.Windows.Controls.Control.ForegroundProperty, new DynamicResourceExtension("Brush.Text.Primary")) },
        };
    }

    // Marks the dictionaries this class added, so that a change of light and dark replaces them.
    private const string Marker = "Axicultus.Captions.Theme";
}
