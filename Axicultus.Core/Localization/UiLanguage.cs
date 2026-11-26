using System;
using System.Globalization;
using System.Threading;

namespace Axicultus.Localization;

/// <summary>
/// The language the apps' menus are shown in: Korean or English. Chosen in the View menu, kept in each app's
/// settings and applied as the UI culture at start-up (a change shows after a restart).
/// </summary>
public static class UiLanguage
{
    public const string Korean = "ko-KR";
    public const string English = "en-US";

    /// <summary>The kept choice as one of the two languages: English for any English name, else Korean (the default).</summary>
    public static string Resolve(string? name) =>
        name is { Length: >= 2 } && name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? English : Korean;

    /// <summary>Makes the language the UI culture of this thread and of every thread made later.</summary>
    public static void Apply(string? name)
    {
        var culture = CultureInfo.GetCultureInfo(Resolve(name));
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    /// <summary>The language the menus are in now.</summary>
    public static string Current => Resolve(CultureInfo.CurrentUICulture.Name);
}
