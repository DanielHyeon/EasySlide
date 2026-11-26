using System.Globalization;
using System.Text.RegularExpressions;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Tests.Localization;

/// <summary>
/// The text a XAML attribute shows, for the tests that read the XAML source: a <c>{loc:Text Area.Key}</c> is looked up in
/// English (the neutral resources) or Korean; any other value is returned as written.
/// </summary>
internal static class XamlText
{
    private static readonly Regex Loc = new(@"^\{loc:Text (\w+)\.([^}\s]+)\}$");

    internal static string English(string value) => Resolve(value, CultureInfo.InvariantCulture);

    internal static string Korean(string value) => Resolve(value, CultureInfo.GetCultureInfo("ko"));

    private static string Resolve(string value, CultureInfo culture)
    {
        var match = Loc.Match(value ?? "");
        return match.Success
            ? UiText.Catalog.Manager(match.Groups[1].Value).GetString(match.Groups[2].Value, culture) ?? value!
            : value!;
    }
}
