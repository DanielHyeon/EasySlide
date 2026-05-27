using System.Windows;
using Easislides.Wpf.Poc;
using Easislides.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf.Demo;

public partial class DemoWindow : Window
{
    private readonly IThemeService _theme;

    public DemoWindow()
    {
        InitializeComponent();
        _theme = App.Services.GetRequiredService<IThemeService>();
    }

    private void OnLight_Click(object sender, RoutedEventArgs e) => _theme.ApplyTheme(ColorTheme.Light);
    private void OnDark_Click(object sender, RoutedEventArgs e) => _theme.ApplyTheme(ColorTheme.Dark);

    private void OnSizeStandard_Click(object sender, RoutedEventArgs e) => _theme.ApplyInterfaceSize(InterfaceSize.Standard);
    private void OnSizeLarge_Click(object sender, RoutedEventArgs e) => _theme.ApplyInterfaceSize(InterfaceSize.Large);
    private void OnSizeSenior_Click(object sender, RoutedEventArgs e) => _theme.ApplyInterfaceSize(InterfaceSize.Senior);

    private void OnPocA_Click(object sender, RoutedEventArgs e)
    {
        var poc = new PocAHookTest { Owner = this };
        poc.Show();
    }

    private void OnPocB_Click(object sender, RoutedEventArgs e)
    {
        var poc = new PocBComStress { Owner = this };
        poc.Show();
    }
}
