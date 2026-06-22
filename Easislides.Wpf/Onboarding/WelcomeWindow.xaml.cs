using System.Windows;
using Easislides.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf.Onboarding;

/// <summary>
/// 시니어 모드 온보딩 윈도우 — 계획서 §7.4 Q2 확정.
///
/// 사용:
///   var welcome = new WelcomeWindow();
///   welcome.ShowDialog(); // 사용자 선택까지 차단
///
/// 최초 실행 시 1회만 표시 — 호출자가 settings.json에 "OnboardingShown=true" 저장 책임.
///
/// 레거시 대응: 신규 개념(온보딩). FrmSplashScreen(로딩 스플래시)과는 개념 상이.
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
