using System.Windows;

namespace Easislides.Wpf.Library;

public partial class SongEditorWindow : Window
{
    private readonly SongEditorViewModel _viewModel;

    public SongEditorWindow(SongEditorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Saved += OnSaved;
        DataContext = viewModel;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Saved -= OnSaved;
        base.OnClosed(e);
    }

    private void OnSaved(object? sender, SongEditorSavedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
