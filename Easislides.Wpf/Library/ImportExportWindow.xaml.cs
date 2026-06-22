using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace Easislides.Wpf.Library;

// 레거시 대체(통합): FrmImport, FrmExport, FrmGenerateDoc(Rtf), FrmGenerateHtml(Html), FrmImportFolder, FrmImportAccessHelper.
public partial class ImportExportWindow : Window
{
    private bool _loadedOnce;

    public ImportExportWindow(ImportExportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce || DataContext is not ImportExportViewModel viewModel)
        {
            return;
        }

        _loadedOnce = true;
        await viewModel.LoadAsync().ConfigureAwait(true);
    }

    private void BrowseImportFile_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ImportExportViewModel viewModel)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "EasiSlides/XML (*.esf;*.est;*.esn;*.xml)|*.esf;*.est;*.esn;*.xml|Access Database (*.mdb)|*.mdb|All files (*.*)|*.*",
            InitialDirectory = ResolveInitialDirectory(viewModel.ImportSourcePath, viewModel.WorkingFolder),
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            viewModel.ImportSourcePath = dialog.FileName;
        }
    }

    private void BrowseImportFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ImportExportViewModel viewModel)
        {
            return;
        }

        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select a folder containing .txt, .doc, or .docx files.",
            SelectedPath = ResolveInitialDirectory(viewModel.ImportSourcePath, viewModel.WorkingFolder),
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            viewModel.ImportSourcePath = dialog.SelectedPath;
        }
    }

    private void BrowseExportPath_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ImportExportViewModel viewModel)
        {
            return;
        }

        if (viewModel.SelectedExportFormat == ExportFormat.Html)
        {
            using var folderDialog = new Forms.FolderBrowserDialog
            {
                Description = "Select an HTML export folder.",
                SelectedPath = ResolveInitialDirectory(viewModel.ExportOutputPath, viewModel.WorkingFolder),
                UseDescriptionForTitle = true,
            };
            if (folderDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                viewModel.ExportOutputPath = folderDialog.SelectedPath;
            }

            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = GetExportFilter(viewModel.SelectedExportFormat),
            InitialDirectory = ResolveInitialDirectory(viewModel.ExportOutputPath, viewModel.WorkingFolder),
            FileName = string.IsNullOrWhiteSpace(viewModel.ExportOutputPath)
                ? ""
                : Path.GetFileName(viewModel.ExportOutputPath),
            DefaultExt = GetExportExtension(viewModel.SelectedExportFormat),
            OverwritePrompt = true,
        };
        if (saveDialog.ShowDialog(this) == true)
        {
            viewModel.ExportOutputPath = saveDialog.FileName;
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
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private static string GetExportFilter(ExportFormat format)
        => format switch
        {
            ExportFormat.EasiSlidesText => "EasiSlides Text (*.esn)|*.esn",
            ExportFormat.EasiSlidesDatabase => "EasiSlides Database (*.esf)|*.esf",
            ExportFormat.Rtf => "Rich Text (*.rtf)|*.rtf",
            _ => "EasiSlides XML (*.xml)|*.xml",
        };

    private static string GetExportExtension(ExportFormat format)
        => format switch
        {
            ExportFormat.EasiSlidesText => ".esn",
            ExportFormat.EasiSlidesDatabase => ".esf",
            ExportFormat.Rtf => ".rtf",
            _ => ".xml",
        };
}
