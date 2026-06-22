using System.Windows;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf;

// 레거시 대체: FrmOptions(ADR-0005 분해) + FrmGetWorkingFolder(작업 폴더 선택).
public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
