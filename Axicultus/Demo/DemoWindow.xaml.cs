using System.Windows;
using Axicultus.Wpf.Poc;
using Axicultus.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Axicultus.Wpf.Demo;

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
    private void OnSizeSmall_Click(object sender, RoutedEventArgs e) => _theme.ApplyInterfaceSize(InterfaceSize.Small);
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

    private void OnIconGallery_Click(object sender, RoutedEventArgs e)
    {
        var gallery = new IconGalleryWindow { Owner = this };
        gallery.Show();
    }

    private void OnControlsGallery_Click(object sender, RoutedEventArgs e)
    {
        var gallery = new ControlsGalleryWindow { Owner = this };
        gallery.Show();
    }

    private void OnLiveBarDemo_Click(object sender, RoutedEventArgs e)
    {
        var demo = new LiveBarDemoWindow { Owner = this };
        demo.Show();
    }
}
