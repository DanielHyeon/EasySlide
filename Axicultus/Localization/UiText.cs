using System;
using System.Windows.Markup;
using Axicultus.Localization;

namespace Axicultus.Wpf.Localization;

/// <summary>
/// The WPF app's UI texts: Localization/Text/&lt;Area&gt;.resx (English) and &lt;Area&gt;.ko.resx (Korean), one pair per
/// window or view model. Code: <c>UiText.Get("SettingsWindow", "Title")</c>; XAML: <c>{loc:Text SettingsWindow.Title}</c>.
/// The values are WPF strings as written (an access key is "_"). The main menu has its own shared MenuText.
/// </summary>
public static class UiText
{
    public static readonly UiTextCatalog Catalog = new(typeof(UiText).Assembly, "Axicultus.Wpf.Localization.Text");

    public static string Get(string area, string key) => Catalog.Get(area, key);

    public static string Format(string area, string key, params object?[] args) => Catalog.Format(area, key, args);
}

/// <summary>A UI text in XAML: <c>Content="{loc:Text Area.Key}"</c> (see <see cref="UiText"/>).</summary>
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
        return dot > 0 ? UiText.Get(Key[..dot], Key[(dot + 1)..]) : $"[{Key}]";
    }
}
