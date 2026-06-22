using System.Windows;

namespace Easislides.Wpf.Support;

// 레거시 대체: FrmRegister(등록).
public partial class RegistrationWindow : Window
{
    public RegistrationWindow(RegistrationWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
