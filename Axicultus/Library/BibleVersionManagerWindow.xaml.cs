using System.Windows;

namespace Axicultus.Wpf.Library;

public partial class BibleVersionManagerWindow : Window
{
    public BibleVersionManagerWindow(BibleViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
