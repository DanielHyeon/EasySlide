using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Axicultus.Localization;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf;

// View > Language: the menus' language, kept in general.language and applied at the next start (App.OpenShell).
public partial class MainWindow
{
    // The kept choice is checked (it may not be shown yet: it shows after a restart).
    private void LanguageMenu_Opened(object sender, RoutedEventArgs e)
    {
        string kept = UiLanguage.Resolve(_settings?.Get(AxicultusSettingKeys.Language));
        LanguageKorean.IsChecked = kept == UiLanguage.Korean;
        LanguageEnglish.IsChecked = kept == UiLanguage.English;
    }

    private void Language_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string language } || _settings is null
            || language == UiLanguage.Resolve(_settings.Get(AxicultusSettingKeys.Language))) return;
        var saved = _settings.Set(AxicultusSettingKeys.Language, language);
        string title = MenuTextExtension.Plain(nameof(MenuText.Language_Title));
        if (!saved.Succeeded)
        {
            MessageBox.Show(this, string.Format(MenuTextExtension.Plain(nameof(MenuText.Language_SaveFailed)),
                string.Join("; ", saved.Issues.Select(issue => issue.Message))), title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        MessageBox.Show(this, MenuTextExtension.Plain(nameof(MenuText.Language_Restart)), title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
