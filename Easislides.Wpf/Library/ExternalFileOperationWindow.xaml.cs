using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Easislides.Wpf.Library;

// 레거시 대체: FrmCopyMoveExternal(외부 파일 복사/이동).
public partial class ExternalFileOperationWindow : Window
{
    private bool _loadedOnce;

    public ExternalFileOperationWindow(ExternalFileOperationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Completed += OnCompleted;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is ExternalFileOperationViewModel viewModel)
        {
            viewModel.Completed -= OnCompleted;
        }

        base.OnClosed(e);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce || DataContext is not ExternalFileOperationViewModel viewModel)
        {
            return;
        }

        _loadedOnce = true;
        await viewModel.LoadAsync().ConfigureAwait(true);
    }

    private void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ExternalFileOperationViewModel viewModel)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = GetFileDialogFilter(viewModel.ItemKind),
            InitialDirectory = ResolveInitialDirectory(viewModel),
        };

        if (dialog.ShowDialog(this) == true)
        {
            viewModel.AddSourceFiles(dialog.FileNames);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCompleted(object? sender, ExternalFileOperationCompletedEventArgs e)
    {
        DialogResult = true;
    }

    private static string GetFileDialogFilter(ExternalFileItemKind itemKind)
        => itemKind switch
        {
            ExternalFileItemKind.InfoScreen => "InfoScreen (*.esi)|*.esi|All files (*.*)|*.*",
            ExternalFileItemKind.PowerPoint => "PowerPoint (*.ppt;*.pptx;*.pps;*.ppsx)|*.ppt;*.pptx;*.pps;*.ppsx|All files (*.*)|*.*",
            ExternalFileItemKind.Media => "Media (*.mp3;*.wav;*.wma;*.m4a;*.aac;*.flac;*.ogg;*.mp4;*.wmv;*.avi;*.mov;*.mkv)|*.mp3;*.wav;*.wma;*.m4a;*.aac;*.flac;*.ogg;*.mp4;*.wmv;*.avi;*.mov;*.mkv|All files (*.*)|*.*",
            _ => "All files (*.*)|*.*",
        };

    private static string ResolveInitialDirectory(ExternalFileOperationViewModel viewModel)
    {
        var selected = viewModel.SelectedExternalFolder?.Path;
        if (!string.IsNullOrWhiteSpace(selected) && System.IO.Directory.Exists(selected))
        {
            return selected;
        }

        var first = viewModel.ExternalFolders.FirstOrDefault()?.Path;
        return !string.IsNullOrWhiteSpace(first) && System.IO.Directory.Exists(first)
            ? first
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
