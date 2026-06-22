using System.Windows;

namespace Easislides.Wpf.Library;

// 레거시 대체: FrmRecoverDeleted(삭제 곡 복구).
public partial class SongRecoveryWindow : Window
{
    private readonly SongRecoveryViewModel _viewModel;
    private bool _loadedOnce;

    public SongRecoveryWindow(SongRecoveryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Recovered += OnRecovered;
        DataContext = viewModel;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Recovered -= OnRecovered;
        base.OnClosed(e);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce)
        {
            return;
        }

        _loadedOnce = true;
        await _viewModel.LoadAsync();
    }

    private void OnRecovered(object? sender, SongRecoveredEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
