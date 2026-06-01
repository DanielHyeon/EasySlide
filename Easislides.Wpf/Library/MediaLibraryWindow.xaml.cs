using System.Windows;

namespace Easislides.Wpf.Library;

/// <summary>
/// 미디어 폴더 브라우저 창(FrmMain Media 탭 포팅) — 폴더의 동영상·오디오를 보고 예배 순서에 추가한다.
/// 목록·추가 로직은 MediaLibraryViewModel 에 위임하고, 창은 폴더 선택·더블클릭 제스처만 맡는다
/// (PowerPointLibraryWindow 와 동일 구조).
/// </summary>
public partial class MediaLibraryWindow : Window
{
    private readonly MediaLibraryViewModel _viewModel;

    public MediaLibraryWindow(MediaLibraryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += (_, _) => _viewModel.LoadCommand.Execute(null);
    }

    private void ChangeFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "미디어 폴더 선택",
        };

        if (!string.IsNullOrWhiteSpace(_viewModel.FolderPath))
        {
            dialog.InitialDirectory = _viewModel.FolderPath;
        }

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.FolderPath = dialog.FolderName;
            _viewModel.LoadCommand.Execute(null);
        }
    }

    private void MediaList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // 더블클릭 = 선택 미디어를 즉시 예배 순서에 추가.
        if (_viewModel.AddSelectedCommand.CanExecute(null))
        {
            _viewModel.AddSelectedCommand.Execute(null);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
