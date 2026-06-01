using System.Windows;

namespace Easislides.Wpf.Library;

/// <summary>
/// 이미지 갤러리 창(FrmMain Images 탭 포팅) — 폴더의 이미지를 썸네일로 보여 주고
/// 선택/더블클릭으로 출력 전역 배경에 적용한다. 모든 동작은 ImageLibraryViewModel 에 위임하고,
/// 창은 폴더 선택 다이얼로그·더블클릭 제스처만 담당한다(BibleVersionManagerWindow 와 동일한 얇은 창).
/// </summary>
public partial class ImageLibraryWindow : Window
{
    private readonly ImageLibraryViewModel _viewModel;

    public ImageLibraryWindow(ImageLibraryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        // 창이 열리면 현재 폴더의 이미지를 1회 읽어 갤러리를 채운다.
        Loaded += (_, _) => _viewModel.LoadCommand.Execute(null);
    }

    private void ChangeFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "이미지 폴더 선택",
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

    private void ImageList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // 더블클릭 = 선택 이미지를 즉시 배경으로 적용(있을 때만).
        if (_viewModel.ApplyAsBackgroundCommand.CanExecute(null))
        {
            _viewModel.ApplyAsBackgroundCommand.Execute(null);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
