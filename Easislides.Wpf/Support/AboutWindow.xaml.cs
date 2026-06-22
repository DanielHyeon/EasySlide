using System.Windows;

namespace Easislides.Wpf.Support;

// 레거시 대체: FrmAbout(정보).
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
