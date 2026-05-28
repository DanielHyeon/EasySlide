using System.Windows;

namespace Easislides.Wpf.Support;

public partial class HelpWindow : Window
{
    public HelpWindow(HelpWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
