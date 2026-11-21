using System.Windows;
using Axicultus.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Axicultus.Wpf.Onboarding;

/// <summary>
/// 데모 갤러리에서 인터페이스 크기를 미리 보는 창.
/// </summary>
public partial class WelcomeWindow : Window
{
    private readonly IThemeService _theme;

    /// <summary>사용자가 선택한 인터페이스 크기. 창 닫힌 후 조회.</summary>
    public InterfaceSize SelectedSize { get; private set; } = InterfaceSize.Standard;

    public WelcomeWindow()
    {
        InitializeComponent();
        _theme = App.Services.GetRequiredService<IThemeService>();
    }

    private void OnStandard_Click(object sender, RoutedEventArgs e) => Choose(InterfaceSize.Standard);
    private void OnLarge_Click(object sender, RoutedEventArgs e) => Choose(InterfaceSize.Large);
    private void OnSenior_Click(object sender, RoutedEventArgs e) => Choose(InterfaceSize.Senior);

    private void Choose(InterfaceSize size)
    {
        SelectedSize = size;
        _theme.ApplyInterfaceSize(size);
        DialogResult = true;
        Close();
    }
}
