using System;
using System.Windows;

namespace Easislides.Wpf.Library;

// 레거시 대체: FrmEditBibleItem(성경 검색/버전/미리보기). 본문 편집 대응은 부분(gap-analysis.md §2.B).
public partial class BibleWindow : Window
{
    private readonly BibleViewModel _viewModel;
    private bool _loadedOnce;

    public BibleWindow(BibleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
    }

    public BibleSelection SelectedSelection => _viewModel.SelectedSelection;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce)
        {
            return;
        }

        _loadedOnce = true;
        await _viewModel.LoadAsync().ConfigureAwait(true);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        var selection = _viewModel.BuildSelection(
            PassageTextBox.SelectionStart,
            PassageTextBox.SelectionLength);
        if (!string.IsNullOrWhiteSpace(selection.IdString))
        {
            DialogResult = true;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
