using System.Runtime.CompilerServices;

namespace Axicultus.Tests;

// The tests see the app's texts in Korean (the default language) whatever the machine's language: a test that
// checks another language sets the culture itself.
internal static class TestUiLanguage
{
    [ModuleInitializer]
    internal static void Korean() => Axicultus.Localization.UiLanguage.Apply(Axicultus.Localization.UiLanguage.Korean);
}
