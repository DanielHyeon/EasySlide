using System.Windows;

namespace Easislides.Wpf.Library;

public partial class LibraryWindow : Window
{
    private bool _loadedOnce;

    public LibraryWindow(LibraryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce || DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        _loadedOnce = true;
        await viewModel.LoadAsync();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
