using System.Windows;

namespace Easislides.Wpf.Library;

// 레거시 대체: FrmCopy(곡 복사).
public partial class SongCopyWindow : Window
{
    private readonly SongCopyViewModel _viewModel;

    public SongCopyWindow(SongCopyViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Copied += OnCopied;
        DataContext = viewModel;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Copied -= OnCopied;
        base.OnClosed(e);
    }

    private void OnCopied(object? sender, SongCopiedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
