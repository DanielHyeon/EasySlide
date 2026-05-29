using System.Windows;

namespace Easislides.Wpf.Library;

public partial class FolderEditorWindow : Window
{
    private readonly FolderEditorViewModel _viewModel;

    public FolderEditorWindow(FolderEditorViewModel viewModel)
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

    private void OnSaved(object? sender, FolderEditorSavedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
