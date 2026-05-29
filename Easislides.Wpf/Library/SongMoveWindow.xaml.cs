using System.Windows;

namespace Easislides.Wpf.Library;

public partial class SongMoveWindow : Window
{
    private readonly SongMoveViewModel _viewModel;

    public SongMoveWindow(SongMoveViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Moved += OnMoved;
        DataContext = viewModel;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Moved -= OnMoved;
        base.OnClosed(e);
    }

    private void OnMoved(object? sender, SongMovedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
