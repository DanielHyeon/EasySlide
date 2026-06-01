using System.Windows;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Library;

/// <summary>
/// 찬양집 창(FrmMain PraiseBook 포팅) — 곡을 머리글자별로 묶어 보여 주고, 이름 붙여 저장하거나
/// 저장된 찬양집을 불러온다. 색인·저장/불러오기 로직은 PraiseBookIndexViewModel 이 담당하고,
/// 창은 이름 입력·삭제 확인 다이얼로그와 콤보 선택 제스처만 맡는다.
/// </summary>
public partial class PraiseBookIndexWindow : Window
{
    private readonly PraiseBookIndexViewModel _viewModel;

    public PraiseBookIndexWindow(PraiseBookIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void SaveBook_Click(object sender, RoutedEventArgs e)
    {
        // 현재 목록을 저장할 이름을 입력받는다. 지금 연 찬양집 이름(CurrentBookName)으로의 재저장은 허용하되,
        // 다른 기존 찬양집 이름과 겹치면 다이얼로그가 막아 실수로 다른 찬양집을 덮어쓰는 사고를 방지한다.
        var dialogViewModel = new NameEntryViewModel(
            "찬양집 저장",
            "저장할 찬양집 이름:",
            _viewModel.CurrentBookName,
            _viewModel.SavedBooks);
        var dialog = new NameEntryWindow(dialogViewModel) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.SaveAsCommand.ExecuteAsync(dialog.EnteredName);
        }
    }

    private async void OpenBook_Click(object sender, RoutedEventArgs e)
    {
        if (SavedBooksCombo.SelectedItem is string name)
        {
            await _viewModel.OpenBookCommand.ExecuteAsync(name);
        }
    }

    private void DeleteBook_Click(object sender, RoutedEventArgs e)
    {
        if (SavedBooksCombo.SelectedItem is not string name)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"'{name}' 찬양집을 삭제할까요?",
            "찬양집 삭제",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirm == MessageBoxResult.OK)
        {
            _viewModel.DeleteBookCommand.Execute(name);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
