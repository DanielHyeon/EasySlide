using System;
using System.Windows.Markup;
using Axicultus.Localization;

namespace Axicultus.Wpf.Localization;

/// <summary>
/// A menu text in the UI language (Axicultus.Core's MenuText resources): <c>Header="{loc:MenuText File_Exit}"</c>.
/// The resources mark access keys the WinForms way ("&amp;File"); here they become WPF's ("_File").
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class MenuTextExtension : MarkupExtension
{
    /// <summary>The app name put in a text's {0}.</summary>
    public const string AppName = "Axicultus";

    public MenuTextExtension() { }

    public MenuTextExtension(string key) => Key = key;

    [ConstructorArgument("key")]
    public string Key { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider) => Text(Key);

    /// <summary>The text of key for a WPF header (a key missing from the resources shows as itself).</summary>
    public static string Text(string key) => ForWpf(Raw(key));

    /// <summary>The text of key without its access key (a message or a window title).</summary>
    public static string Plain(string key) =>
        System.Text.RegularExpressions.Regex.Replace(Raw(key), @"\(&\w\)", "").Replace("&&", "\u0001").Replace("&", "").Replace("\u0001", "&");

    private static string Raw(string key) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, MenuText.ResourceManager.GetString(key) ?? key, AppName);

    // "&" marks the access key ("&&" a plain "&"); a plain "_" is doubled so WPF does not take it for one.
    internal static string ForWpf(string text) =>
        text.Replace("_", "__").Replace("&&", "\u0001").Replace("&", "_").Replace("\u0001", "&");
}
