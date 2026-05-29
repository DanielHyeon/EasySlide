using System.Windows;

namespace Easislides.Wpf.Library;

public partial class SongDeleteWindow : Window
{
    private readonly SongDeleteViewModel _viewModel;

    public SongDeleteWindow(SongDeleteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Deleted += OnDeleted;
        DataContext = viewModel;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Deleted -= OnDeleted;
        base.OnClosed(e);
    }

    private void OnDeleted(object? sender, SongDeletedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
