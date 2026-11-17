using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Easislides.Wpf.Library;

// 레거시 대체(통합): FrmFind, FrmUsages, FrmLookupTitles(SongSearchFields.Title).
public partial class SearchUsageWindow : Window
{
    private bool _loadedOnce;

    public SearchUsageWindow(SearchUsageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce || DataContext is not SearchUsageViewModel viewModel)
        {
            return;
        }

        _loadedOnce = true;
        await viewModel.LoadAsync().ConfigureAwait(true);
    }

    private void BrowseUsageReport_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SearchUsageViewModel viewModel)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Rich Text (*.rtf)|*.rtf",
            DefaultExt = ".rtf",
            FileName = string.IsNullOrWhiteSpace(viewModel.UsageReportOutputPath)
                ? "Song Usages.rtf"
                : Path.GetFileName(viewModel.UsageReportOutputPath),
            InitialDirectory = ResolveInitialDirectory(viewModel.UsageReportOutputPath, viewModel.WorkingFolder),
            OverwritePrompt = true,
        };
        if (dialog.ShowDialog(this) == true)
        {
            viewModel.UsageReportOutputPath = dialog.FileName;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static string ResolveInitialDirectory(string path, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var candidate = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(candidate) && Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return !string.IsNullOrWhiteSpace(fallback) && Directory.Exists(fallback)
            ? fallback
            : System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
    }
}
