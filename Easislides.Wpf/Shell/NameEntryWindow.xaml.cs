using System.Windows;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 재사용 "이름 입력/변경" 다이얼로그(레거시 FrmBibleRename·FrmUpdateFileName 대응).
/// 호출자는 NameEntryViewModel 을 만들어 넘기고, ShowDialog()==true 면 EnteredName 으로 확정 이름을 읽는다.
/// </summary>
public partial class NameEntryWindow : Window
{
    private readonly NameEntryViewModel _viewModel;

    public NameEntryWindow(NameEntryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        // 창이 뜨면 입력칸에 포커스를 주고 전체 선택(레거시 SelectAll 동작 — 바로 새 이름 타이핑).
        Loaded += (_, _) =>
        {
            InputBox.Focus();
            InputBox.SelectAll();
        };
    }

    /// <summary>확인 시 호출자가 받을 최종 이름(공백 제거됨).</summary>
    public string EnteredName => _viewModel.Result;

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        // 확인 가능할 때만 닫는다(버튼이 IsEnabled 로 막지만 방어적으로 한 번 더 확인).
        if (_viewModel.CanConfirm)
        {
            DialogResult = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
