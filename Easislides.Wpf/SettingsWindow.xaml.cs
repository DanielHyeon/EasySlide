using System.Windows;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
