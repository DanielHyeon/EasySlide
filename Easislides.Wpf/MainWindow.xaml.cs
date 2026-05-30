using System;
using System.Windows;
using System.Windows.Input;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf;

// 레거시 대체: FrmMain(운영 셸). PPT 썸네일/미디어 탭 렌더는 미완(docs/wpf-migration/gap-analysis.md §4 G-α).
public partial class MainWindow : Window
{
    private readonly ShortcutRegistry _shortcuts;
    private readonly IServiceProvider _services;

    public MainWindow(MainViewModel viewModel, ShortcutRegistry shortcuts, IServiceProvider services)
    {
        InitializeComponent();

        _shortcuts = shortcuts;
        _services = services;
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

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = _services.GetRequiredService<SettingsWindow>();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void OpenLibrary_Click(object sender, RoutedEventArgs e)
    {
        var libraryWindow = _services.GetRequiredService<LibraryWindow>();
        libraryWindow.Owner = this;
        if (libraryWindow.ShowDialog() == true && DataContext is MainViewModel viewModel)
        {
            // "라이브에 추가"로 닫혔으면 선택 곡을 예배 순서(큐)에 추가(BibleWindow 흐름과 동일).
            viewModel.AddSong(libraryWindow.SelectedSongForLive);
        }
    }

    private void OpenExternalFiles_Click(object sender, RoutedEventArgs e)
    {
        var externalFileWindow = _services.GetRequiredService<ExternalFileOperationWindow>();
        externalFileWindow.Owner = this;
        externalFileWindow.ShowDialog();
    }

    private void OpenImportExport_Click(object sender, RoutedEventArgs e)
    {
        var importExportWindow = _services.GetRequiredService<ImportExportWindow>();
        importExportWindow.Owner = this;
        importExportWindow.ShowDialog();
    }

    private void OpenSearchUsage_Click(object sender, RoutedEventArgs e)
    {
        var searchUsageWindow = _services.GetRequiredService<SearchUsageWindow>();
        searchUsageWindow.Owner = this;
        searchUsageWindow.ShowDialog();
    }

    private void AddExternalFile_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "예배 순서에 추가할 파일 선택",
            Filter = "PowerPoint (*.ppt;*.pptx)|*.ppt;*.pptx"
                + "|미디어 (*.mp4;*.avi;*.wmv;*.mov;*.mkv;*.mp3;*.wav;*.wma)|*.mp4;*.avi;*.wmv;*.mov;*.mkv;*.mp3;*.wav;*.wma"
                + "|모든 파일 (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            var ext = System.IO.Path.GetExtension(dialog.FileName).ToLowerInvariant();
            if (ext is ".ppt" or ".pptx")
            {
                viewModel.AddPowerPoint(dialog.FileName);
            }
            else
            {
                viewModel.AddMedia(dialog.FileName);
            }
        }
    }

    private void OpenManageWorshipLists_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            var window = new ManageWorshipListsWindow(viewModel) { Owner = this };
            window.ShowDialog();
        }
    }

    private void OpenBible_Click(object sender, RoutedEventArgs e)
    {
        var bibleWindow = _services.GetRequiredService<BibleWindow>();
        bibleWindow.Owner = this;
        if (bibleWindow.ShowDialog() == true && DataContext is MainViewModel viewModel)
        {
            viewModel.AddBibleSelection(bibleWindow.SelectedSelection);
        }
    }

    private void OpenHelp_Click(object sender, RoutedEventArgs e)
    {
        var helpWindow = _services.GetRequiredService<HelpWindow>();
        helpWindow.Owner = this;
        helpWindow.ShowDialog();
    }

    private void OpenRegistration_Click(object sender, RoutedEventArgs e)
    {
        var registrationWindow = _services.GetRequiredService<RegistrationWindow>();
        registrationWindow.Owner = this;
        registrationWindow.ShowDialog();
    }

    private void OpenAbout_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = _services.GetRequiredService<AboutWindow>();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnClosed(e);
    }
}
