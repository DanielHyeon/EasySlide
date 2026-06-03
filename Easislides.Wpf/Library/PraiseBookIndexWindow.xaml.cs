using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    /// <summary>
    /// 더블클릭으로 "예배 순서에 추가" 하려고 고른 색인 항목(곡 제목·번호). 호출부(MainWindow)가 이 값으로
    /// 라이브러리에서 실제 곡을 찾아 큐에 넣는다. 그냥 닫으면 null(LibraryWindow.SelectedSongForLive 패턴과 동일).
    /// </summary>
    public PraiseBookIndexEntry? SelectedEntryForLive { get; private set; }

    // 색인의 한 곡 줄을 더블클릭하면 그 곡을 "라이브에 추가" 후보로 잡고 창을 닫는다(LibraryWindow 추가 흐름과 동일).
    private void Entry_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2)
        {
            return; // 단일 클릭은 무시 — 더블클릭만 추가 제스처.
        }

        if (sender is FrameworkElement { DataContext: PraiseBookIndexEntry entry })
        {
            SelectedEntryForLive = entry;
            DialogResult = true; // ShowDialog 로 연 창은 DialogResult 설정만으로 닫힌다(LibraryWindow 패턴과 동일).
        }
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

    // 색인을 파일로 내보낸다 — 레거시 "Listing of Selected Folder" 처럼 고른 확장자로 형식을 정한다:
    // .rtf → RTF(Word 편집·인쇄용), 그 외(.html 등) → HTML(브라우저·인쇄용).
    private void ExportIndex_Click(object sender, RoutedEventArgs e)
    {
        var baseName = string.IsNullOrWhiteSpace(_viewModel.CurrentBookName) ? "찬양집색인" : _viewModel.CurrentBookName;
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "찬양집 색인 저장",
            Filter = "HTML 문서 (*.html)|*.html|서식 있는 텍스트 RTF (*.rtf)|*.rtf|모든 파일 (*.*)|*.*",
            FileName = $"{baseName}.html",
            DefaultExt = ".html",
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var isRtf = string.Equals(System.IO.Path.GetExtension(dialog.FileName), ".rtf", System.StringComparison.OrdinalIgnoreCase);
            if (isRtf)
            {
                // RTF 본문은 \uN? 이스케이프라 전부 ASCII — BOM 없는 UTF-8 로 써 RTF 리더가 머리의 BOM 에 걸리지 않게.
                System.IO.File.WriteAllText(dialog.FileName, _viewModel.BuildIndexRtf(), new System.Text.UTF8Encoding(false));
            }
            else
            {
                System.IO.File.WriteAllText(dialog.FileName, _viewModel.BuildIndexHtml(), System.Text.Encoding.UTF8);
            }

            MessageBox.Show(this, $"색인을 저장했습니다:\n{dialog.FileName}", "찬양집 색인 내보내기", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            // 디스크 권한·잠김 등 저장 실패는 사용자에게 알리고 창은 유지(크래시 방지).
            MessageBox.Show(this, $"저장에 실패했습니다:\n{ex.Message}", "찬양집 색인 내보내기", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
