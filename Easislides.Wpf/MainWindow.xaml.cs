using System.Windows;
using System.Windows.Input;
using Easislides.Wpf.Input;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf;

public partial class MainWindow : Window
{
    private readonly ShortcutRegistry _shortcuts;

    public MainWindow(MainViewModel viewModel, ShortcutRegistry shortcuts)
    {
        InitializeComponent();

        _shortcuts = shortcuts;
        DataContext = viewModel;
        viewModel.BindShortcuts(_shortcuts);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (_shortcuts.TryHandle(e.Key, Keyboard.Modifiers))
        {
            e.Handled = true;
        }
    }
}
