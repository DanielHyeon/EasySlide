using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Axicultus.Wpf.Library;

// 레거시 대체(통합): FrmFind, FrmUsages, FrmLookupTitles(SongSearchFields.Title).
public partial class SearchUsageWindow : Window
{
    private bool _loadedOnce;

    public SearchUsageWindow(SearchUsageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private BibleViewModel? _bible;
    internal event System.EventHandler? BibleFindRequested;

    internal void ConfigureBibleFind(BibleViewModel bible, bool selected)
    {
        _bible = bible;
        BibleFindTab.Visibility = Visibility.Visible;
        FindVersion.ItemsSource = bible.Versions;
        FindVersion.SelectedItem = bible.SelectedVersion;
        if (bible.SearchSelectedBookOnly) FindBook.SelectedItem = bible.SelectedBook;
        FindPassage.Text = bible.SearchText;
        FindMode.SelectedIndex = (int)bible.MatchMode;
        BibleFindTab.IsEnabled = bible.Versions.Count > 0;
        if (selected && BibleFindTab.IsEnabled) BibleFindTab.IsSelected = true;
    }

    private void FindVersion_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_bible is null || FindVersion.SelectedItem is not BibleVersion version) return;
        BibleBook[] books = [new(0, "모든 책"), .. _bible.SearchBooksFor(version)];
        FindBook.ItemsSource = books;
        FindBook.SelectedIndex = 0;
    }

    private void FindPassage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter) return;
        e.Handled = true;
        FindBible_Click(sender, e);
    }

    private void FindBible_Click(object sender, RoutedEventArgs e)
    {
        if (_bible is null || FindVersion.SelectedItem is not BibleVersion version ||
            FindBook.SelectedItem is not BibleBook book || string.IsNullOrWhiteSpace(FindPassage.Text)) return;
        _bible.SelectedVersion = version;
        _bible.SearchSelectedBookOnly = book.Number > 0;
        if (book.Number > 0) _bible.SelectedBook = book;
        _bible.SearchText = FindPassage.Text.Trim();
        _bible.MatchMode = (BibleSearchMatchMode)FindMode.SelectedIndex;
        BibleFindRequested?.Invoke(this, System.EventArgs.Empty);
    }

    internal void SelectUsage() => UsageTab.IsSelected = true;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce || DataContext is not SearchUsageViewModel viewModel)
        {
            return;
        }

        _loadedOnce = true;
        if (_bible is not null)
        {
            var phrase = BibleFindTab.IsSelected ? FindPassage : SongQuery.PhraseBox;
            phrase.Focus();
            phrase.SelectAll();
        }
        await viewModel.LoadAsync().ConfigureAwait(true);
        if (UsageTab.IsSelected) await viewModel.RefreshUsageAsync().ConfigureAwait(true);
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
