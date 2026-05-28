using System;
using System.Windows;
using System.Windows.Input;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf;

public partial class MainWindow : Window
{
    private readonly ShortcutRegistry _shortcuts;
    private readonly IServiceProvider _services;

    public MainWindow(MainViewModel viewModel, ShortcutRegistry shortcuts, IServiceProvider services)
    {
        InitializeComponent();

        _shortcuts = shortcuts;
        _services = services;
        DataContext = viewModel;
        viewModel.BindShortcuts(_shortcuts);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (_shortcuts.TryHandle(e.Key, Keyboard.Modifiers))
        {
            e.Handled = true;
        }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = _services.GetRequiredService<SettingsWindow>();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void OpenLibrary_Click(object sender, RoutedEventArgs e)
    {
        var libraryWindow = _services.GetRequiredService<LibraryWindow>();
        libraryWindow.Owner = this;
        libraryWindow.ShowDialog();
    }

    private void OpenHelp_Click(object sender, RoutedEventArgs e)
    {
        var helpWindow = _services.GetRequiredService<HelpWindow>();
        helpWindow.Owner = this;
        helpWindow.ShowDialog();
    }

    private void OpenRegistration_Click(object sender, RoutedEventArgs e)
    {
        var registrationWindow = _services.GetRequiredService<RegistrationWindow>();
        registrationWindow.Owner = this;
        registrationWindow.ShowDialog();
    }

    private void OpenAbout_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = _services.GetRequiredService<AboutWindow>();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnClosed(e);
    }
}
