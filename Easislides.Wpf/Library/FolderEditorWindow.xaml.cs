using System.Windows;

namespace Easislides.Wpf.Library;

// 레거시 대체: FrmRearrangeFolderPositions(폴더 생성/이름/번호; 위치 정렬 전용 기능은 부분).
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
