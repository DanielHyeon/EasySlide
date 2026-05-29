using System.Windows;

namespace Easislides.Wpf.Library;

public partial class SongMergeWindow : Window
{
    private readonly SongMergeViewModel _viewModel;

    public SongMergeWindow(SongMergeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Merged += OnMerged;
        DataContext = viewModel;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Merged -= OnMerged;
        base.OnClosed(e);
    }

    private void OnMerged(object? sender, SongMergedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
