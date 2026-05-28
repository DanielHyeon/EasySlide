using System.Windows;

namespace Easislides.Wpf.Support;

public partial class AboutWindow : Window
{
    public AboutWindow(AboutWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
