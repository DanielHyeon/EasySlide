using System.Windows;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 공지 화면(InfoScreen) 창(레거시 FrmInfoScreen 대응) — 자유 텍스트 안내를 입력해 출력으로 송출한다.
/// 송출 로직은 NoticeScreenViewModel(→ MainViewModel.PublishNotice)에 위임하고, 창은 닫기만 맡는다.
/// </summary>
public partial class NoticeScreenWindow : Window
{
    public NoticeScreenWindow(NoticeScreenViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
