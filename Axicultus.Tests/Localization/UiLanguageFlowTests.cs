using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Threading;
using Axicultus.Localization;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Localization;
using Xunit;

namespace Axicultus.Wpf.Tests.Localization;

// The app takes its language in its async startup. On a Windows whose display language is English, the UI thread
// starts in en-US: what its message loop runs later (a click opening the caption tab) must still be in the chosen
// language, not back in en-US.
public sealed class UiLanguageFlowTests
{
    [Fact]
    public void TextsLookedUpAfterTheAsyncStartupAreInTheChosenLanguage() => StaHelper.RunOnSta(() =>
    {
        var system = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(UiLanguage.English); // English Windows display
            var dispatcher = Dispatcher.CurrentDispatcher;
            string? later = null, app = null, menu = null;
            async void Startup()
            {
                UiLanguage.Apply(UiLanguage.Korean);
                await Task.Yield();
            }
            dispatcher.BeginInvoke(Startup);
            dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                later = CaptionText.Get("CaptionConsole", "Tab"); // the caption library's texts
                app = UiText.Get("SettingsWindow", "Title"); // the app's own texts
                menu = MenuText.Output_CameraCaptions;
                dispatcher.InvokeShutdown();
            });
            Dispatcher.Run();
            Assert.Equal(("자막", "옵션", "카메라 하단 자막"), (later, app, menu));
        }
        finally
        {
            CultureInfo.CurrentUICulture = system;
            UiLanguage.Apply(UiLanguage.Korean); // (the tests' language)
        }
    });
}
