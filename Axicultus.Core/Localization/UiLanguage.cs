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
        // A culture set inside an async method or a dispatcher callback is undone when it returns, and the UI thread
        // goes back to the Windows display language for what its message loop runs later (a tab opened by a click).
        // Set again from the loop itself, outside that flow, the culture stays.
        if (SynchronizationContext.Current is { } loop)
            using (ExecutionContext.SuppressFlow())
                loop.Post(_ => Thread.CurrentThread.CurrentUICulture = culture, null);
    }

    /// <summary>The language the menus are in now.</summary>
    public static string Current => Resolve(CultureInfo.CurrentUICulture.Name);
}
