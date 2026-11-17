using System.Windows;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 세션 메모 창(레거시 FrmMain Session Notes) — 현재 예배 세션의 운영자 메모를 편집·저장한다.
/// 모든 동작은 WorshipSessionNotesViewModel 에 위임하고, 창은 닫기만 맡는다.
/// </summary>
public partial class WorshipSessionNotesWindow : Window
{
    public WorshipSessionNotesWindow(WorshipSessionNotesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
