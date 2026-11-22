using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Axicultus.Wpf.Library;

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

    internal void UseSelectedSourceItems(ExternalFileItemKind kind, ExternalFileOperationKind operation)
    {
        Title = $"{kind} {(operation == ExternalFileOperationKind.Move ? "이동" : "복사")}";
        ItemKindPicker.IsEnabled = OperationPicker.IsEnabled = false;
        SourceActions.Visibility = Visibility.Collapsed;
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
            Filter = GetFileDialogFilter(viewModel.ItemKind, viewModel.WorkingFolder),
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

    // The same extensions as the folder browsers list, so a file copied in here shows up there.
    internal static string GetFileDialogFilter(ExternalFileItemKind itemKind, string? workingFolder)
        => itemKind switch
        {
            ExternalFileItemKind.InfoScreen => "InfoScreen (*.esi)|*.esi|All files (*.*)|*.*",
            ExternalFileItemKind.PowerPoint => SourceFileExtensions.DialogFilter("PowerPoint", SourceFileExtensions.PowerPoint) + "|All files (*.*)|*.*",
            ExternalFileItemKind.Media => SourceFileExtensions.DialogFilter("Media",
                SourceFileExtensions.Audio(workingFolder).Concat(SourceFileExtensions.Video(workingFolder))) + "|All files (*.*)|*.*",
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
