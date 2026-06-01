using System.Windows;

namespace Easislides.Wpf.Library;

/// <summary>
/// 찬양집 색인 창(FrmMain PraiseBook/Listing of Selected Folder 포팅) — 곡을 머리글자별로 묶어 보여 준다.
/// 색인 데이터는 PraiseBookIndexViewModel 이 생성 시 만들고, 창은 표시·닫기만 담당한다(읽기 전용).
/// </summary>
public partial class PraiseBookIndexWindow : Window
{
    public PraiseBookIndexWindow(PraiseBookIndexViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
