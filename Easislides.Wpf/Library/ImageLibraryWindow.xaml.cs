using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

    // 우클릭한 썸네일을 먼저 선택한다 — 컨텍스트 메뉴 "배경으로 적용"이 (직전 선택이 아니라) 우클릭한 그 이미지에 동작하도록.
    // WPF 기본은 우클릭으로 선택이 안 되므로 직접 컨테이너를 찾아 IsSelected 를 켠다. 썸네일 밖(빈 공간) 우클릭은 무시.
    private void ImageList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source
            && ItemsControl.ContainerFromElement(ImageList, source) is ListBoxItem item)
        {
            item.IsSelected = true;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
