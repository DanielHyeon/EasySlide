using System.Windows;

namespace Easislides.Wpf.Support;

// 레거시 대체: FrmHelp(도움말).
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
