using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Media;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf;

public partial class SettingsWindow : Window
{
    private readonly SettingsWindowViewModel _options;
    private readonly LibraryViewModel _library;
    private readonly FolderEditorViewModel _folderEditor;
    private bool _foldersLoaded;
    private readonly Dictionary<int, SongFolderWriteModel> _pendingFolders = new();
    private readonly ObservableCollection<SongFolderSummary> _folderChoices = new();
    public ObservableCollection<FolderChoice> JumpFolders { get; } = new();
    private BibleCatalogEdit? _bibleCatalog;
    private BibleViewModel _liveBible;
    private bool _saving;

    public SettingsWindow(SettingsWindowViewModel options, CameraOwnership ownership, ObsOperator obs,
        IDisplayService displayService, LibraryViewModel library, FolderEditorViewModel folderEditor, BibleViewModel bible)
    {
        _options = options;
        _library = library;
        _folderEditor = folderEditor;
        options.BeginOptionsEdit();
        var displays = displayService.GetDisplays();
        var choices = new List<DisplayChoice> { new("", "자동 선택") };
        choices.AddRange(displays.Select(d => new DisplayChoice(d.Id, $"{d.Name} ({d.Width:0} × {d.Height:0})")));
        foreach (var id in new[] { options.DefaultOutputMonitorId, options.PreviewMonitorId })
            if (!string.IsNullOrEmpty(id) && !choices.Any(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase)))
                choices.Add(new DisplayChoice(id, $"{id} (연결 해제)"));
        DisplayChoices = choices;
        InitializeComponent();
        DataContext = options;
        _liveBible = bible;
        BiblePanel.Loaded += BiblePanel_Loaded;
        Closed += (_, _) => _bibleCatalog?.Dispose();
        Loaded += async (_, _) =>
        {
            if (Owner?.DataContext is MainViewModel shell) _liveBible = shell.Bible;
            await LoadFolderChoicesAsync();
            ImportLicenceNames();
        };
        DeviceCheck.Configure(ownership, new DeviceCheckBackend(), obs);
    }

    public sealed record FolderChoice(int Id, string Name);
    public sealed record DisplayChoice(string Id, string Name);
    public IReadOnlyList<DisplayChoice> DisplayChoices { get; }

    private async void BiblePanel_Loaded(object sender, RoutedEventArgs e)
    {
        if (_bibleCatalog != null) return;
        try
        {
            var path = Path.Combine(_liveBible.CatalogRoot, BibleRepository.BibleListRelativePath);
            if (!File.Exists(path)) { BiblePanel.IsEnabled = false; return; }
            _bibleCatalog = new(path);
            var draft = _liveBible.ForCatalog(_bibleCatalog.DraftPath);
            await draft.LoadAsync();
            BiblePanel.DataContext = draft;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Data.SQLite.SQLiteException)
        {
            MessageBox.Show(this, ex.Message, "성경 목록");
        }
    }

    private async void FolderList_Loaded(object sender, RoutedEventArgs e) => await LoadFolderChoicesAsync();

    private async Task LoadFolderChoicesAsync()
    {
        if (_foldersLoaded) return;
        _foldersLoaded = true;
        await _library.LoadFolderOptionsAsync();
        foreach (var folder in _library.Folders) _folderChoices.Add(folder);
        for (int number = 1; number <= 40; number++)
        {
            var match = _folderChoices.FirstOrDefault(f => f.FolderNo == number);
            JumpFolders.Add(new(number, match?.Name ?? $"폴더 {number}"));
        }
        FolderList.ItemsSource = _folderChoices;
        FolderList.SelectedIndex = _library.Folders.Count > 0 ? 0 : -1;
        FolderStatus.Text = _library.Folders.Count == 0 ? _library.StatusMessage : "";
    }

    private void EditFolder_Click(object sender, RoutedEventArgs e)
    {
        if (FolderList.SelectedItem is not SongFolderSummary selected) return;
        _folderEditor.Load(_library.DatabasePath, selected, _folderChoices);
        _folderEditor.StageChanges = edit =>
        {
            var changes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (_pendingFolders.TryGetValue(selected.FolderNo, out var prior) && prior.FormatChanges != null)
                foreach (var pair in prior.FormatChanges) changes[pair.Key] = pair.Value;
            if (edit.FormatChanges != null)
                foreach (var pair in edit.FormatChanges) changes[pair.Key] = pair.Value;
            _pendingFolders[selected.FolderNo] = edit with { FormatChanges = changes };
            var fields = (selected.FormatFields ?? new Dictionary<string,string>()).ToDictionary(p => p.Key, p => p.Value);
            foreach (var pair in changes) fields[pair.Key] = pair.Value;
            _folderChoices[_folderChoices.IndexOf(selected)] = selected with { Name = edit.Name, IsEnabled = edit.IsEnabled, FormatFields = fields };
            FolderList.SelectedItem = _folderChoices.First(f => f.FolderNo == selected.FolderNo);
            int choice = JumpFolders.ToList().FindIndex(f => f.Id == selected.FolderNo);
            if (choice >= 0) JumpFolders[choice] = new(selected.FolderNo, edit.Name);
        };
        try { new FolderEditorWindow(_folderEditor) { Owner = this }.ShowDialog(); }
        finally { _folderEditor.StageChanges = null; }
    }

    private void ImportLicenceNames()
    {
        if (string.IsNullOrWhiteSpace(_library.DatabasePath) || !File.Exists(_library.DatabasePath)) return;
        try
        {
            using var connection = new System.Data.SQLite.SQLiteConnection(new System.Data.SQLite.SQLiteConnectionStringBuilder
                { DataSource = _library.DatabasePath, ReadOnly = true, FailIfMissing = true }.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='LICENCE' COLLATE NOCASE";
            if (command.ExecuteScalar() == null) return;
            command.CommandText = "SELECT Ref, ADMINISTRATOR FROM LICENCE WHERE Ref BETWEEN 4 AND 8";
            using var reader = command.ExecuteReader();
            var names = new Dictionary<int, string>();
            while (reader.Read()) names[Convert.ToInt32(reader[0])] = Convert.ToString(reader[1])?.Trim() ?? "";
            _options.ImportLicenceNames(_library.DatabasePath, names);
        }
        catch (Exception ex) when (ex is System.Data.SQLite.SQLiteException or IOException or UnauthorizedAccessException)
        {
            FolderStatus.Text = ex.Message;
        }
    }

    private void BrowseMediaFolder_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog { Title = "미디어 폴더 선택" };
        if (System.IO.Directory.Exists(_options.MediaDirectory)) picker.InitialDirectory = _options.MediaDirectory;
        if (picker.ShowDialog(this) == true) _options.MediaDirectory = picker.FolderName;
    }

    private void BrowseGapLogo_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Title = "간격 화면 로고 선택", Filter = "이미지|*.png;*.jpg;*.jpeg;*.bmp;*.gif|모든 파일|*.*" };
        if (picker.ShowDialog(this) != true) return;
        _options.GapItemLogoFile = picker.FileName;
        _options.GapItemOption = GapItemMode.User;
    }

    private static DependencyObject? FindInvalidEditor(DependencyObject element)
    {
        if (Validation.GetHasError(element)) return element;
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = FindInvalidEditor(System.Windows.Media.VisualTreeHelper.GetChild(element, i));
            if (child != null) return child;
        }
        return null;
    }

    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        // Enter on a text field can invoke the default button before LostFocus.
        if (Keyboard.FocusedElement is TextBox text)
            text.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        if (FindInvalidEditor(this) is FrameworkElement invalid)
        {
            invalid.BringIntoView(); invalid.Focus(); return;
        }
        if (_saving) return;
        _saving = true;
        try
        {
            bool libraryChanged = _pendingFolders.Count != 0;
            if (libraryChanged)
            {
                var report = await _folderEditor.CommitFoldersAsync(_library.DatabasePath, _pendingFolders.Values.ToArray());
                if (!report.Succeeded) { MessageBox.Show(this, string.Join("\n", report.Issues.Select(i => i.Message)), "폴더 저장"); return; }
                _pendingFolders.Clear();
                await _library.LoadFolderOptionsAsync();
                if (Owner?.DataContext is MainViewModel shell) await shell.Library.LoadAsync();
            }
            if (_bibleCatalog?.Commit() == true) { await _liveBible.LoadAsync(); libraryChanged = true; }
            if (libraryChanged && Owner?.DataContext is MainViewModel main) main.RefreshLibraryFormatting();
            if (_options.ConfirmOptions()) DialogResult = true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException or System.Data.SQLite.SQLiteException)
        {
            MessageBox.Show(this, ex.Message, "옵션 저장");
        }
        finally { _saving = false; }
    }
}
