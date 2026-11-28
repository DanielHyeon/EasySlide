using System;
using System.Windows.Markup;
using Axicultus.Localization;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The caption texts: Localization/Text/&lt;Area&gt;.resx (English) and &lt;Area&gt;.ko.resx (Korean) of this library, in the
/// UI language the host chose (<see cref="UiLanguage"/>). Code: <c>CaptionText.Get("CaptionConsole", "Tab")</c>; XAML:
/// <c>{loc:Text CaptionConsole.Tab}</c> with loc mapped to this namespace. The host app's own texts stay in its UiText.
/// </summary>
public static class CaptionText
{
    public static readonly UiTextCatalog Catalog = new(typeof(CaptionText).Assembly, "Axicultus.Wpf.Captions.Localization.Text");

    public static string Get(string area, string key) => Catalog.Get(area, key);

    public static string Format(string area, string key, params object?[] args) => Catalog.Format(area, key, args);
}

/// <summary>A caption text in XAML: <c>Content="{loc:Text Area.Key}"</c> (see <see cref="CaptionText"/>).</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class TextExtension : MarkupExtension
{
    public TextExtension() { }

    public TextExtension(string key) => Key = key;

    /// <summary>"Area.Key".</summary>
    [ConstructorArgument("key")]
    public string Key { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        int dot = Key.IndexOf('.');
        return dot > 0 ? CaptionText.Get(Key[..dot], Key[(dot + 1)..]) : $"[{Key}]";
    }
}
