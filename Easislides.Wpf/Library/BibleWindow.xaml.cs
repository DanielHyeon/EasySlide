using System;
using System.Windows;

namespace Easislides.Wpf.Library;

public partial class BibleWindow : Window
{
    private readonly BibleViewModel _viewModel;
    private bool _loadedOnce;

    public BibleWindow(BibleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce)
        {
            return;
        }

        _loadedOnce = true;
        await _viewModel.LoadAsync().ConfigureAwait(true);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        var selection = _viewModel.BuildSelection(
            PassageTextBox.SelectionStart,
            PassageTextBox.SelectionLength);
        if (!string.IsNullOrWhiteSpace(selection.IdString))
        {
            DialogResult = true;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
