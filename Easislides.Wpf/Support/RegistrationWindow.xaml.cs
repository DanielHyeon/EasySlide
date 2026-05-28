using System.Windows;

namespace Easislides.Wpf.Support;

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
