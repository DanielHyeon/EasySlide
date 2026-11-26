using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Axicultus.Core;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Platform;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf;

public partial class SettingsWindow : Window
{
    private readonly SettingsWindowViewModel _options;
    private readonly LibraryViewModel _library;
    private readonly FolderEditorViewModel _folderEditor;
    private bool _foldersLoaded;
    private readonly ObservableCollection<OptionsFolderDraft> _folderChoices = [];
    internal int LastOperatorTab => _options.SelectedTabIndex;
    public ObservableCollection<NumberedChoice> JumpFolders { get; } = [];
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
        // "None" is the FrmOptions choice for no output; without it a saved None shows as a disconnected monitor.
        var choices = new List<DisplayChoice> { new("", UiText.Get("SettingsWindow", "AutoSelect")), new("None", UiText.Get("SettingsWindow", "NoOutput")) };
        choices.AddRange(displays.Select(d => new DisplayChoice(d.Id, d.Name)));
        foreach (var id in new[] { options.DefaultOutputMonitorId, options.PreviewMonitorId })
            if (!string.IsNullOrEmpty(id) && !choices.Any(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase)))
                choices.Add(new DisplayChoice(id, MonitorLabel.Disconnected(id)));
        DisplayChoices = choices;
        // "No output" is a choice for the audience screen only.
        StageDisplayChoices = choices.Where(d => d.Id != "None").ToList();
        InitializeComponent();
        DataContext = options;
        options.ConfirmRestoreDefaults = () => MessageBox.Show(this,
            UiText.Get("SettingsWindow", "RestoreDefaultsConfirm"),
            UiText.Get("SettingsWindow", "RestoreDefaults"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;
        _liveBible = bible;
        BiblePanel.Loaded += BiblePanel_Loaded;
        Closing += (_, e) => { if (_saving && DialogResult != true) e.Cancel = true; };
        Closed += (_, _) => _bibleCatalog?.Dispose();
        Loaded += async (_, _) =>
        {
            if (Owner?.DataContext is MainViewModel shell)
            {
                _liveBible = shell.Bible;
                CanConfigureDisplays = !shell.IsShowRunning;
            }
            await LoadFolderChoicesAsync();
            ImportLicenceNames();
        };
        DeviceCheck.Configure(ownership, new DeviceCheckBackend(), obs);
    }

    private static readonly DependencyPropertyKey CanConfigureDisplaysPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanConfigureDisplays), typeof(bool), typeof(SettingsWindow), new PropertyMetadata(true));
    public static readonly DependencyProperty CanConfigureDisplaysProperty = CanConfigureDisplaysPropertyKey.DependencyProperty;
    public bool CanConfigureDisplays { get => (bool)GetValue(CanConfigureDisplaysProperty); private set => SetValue(CanConfigureDisplaysPropertyKey, value); }
    public ObservableCollection<NumberedChoice> CameraChoices { get; } =
        new(Enumerable.Range(1, 10).Select(id => new NumberedChoice(id, $"{id}.")));
    private bool _cameraNamesLoaded;

    private async void CameraList_Loaded(object sender, RoutedEventArgs e)
    {
        if (_cameraNamesLoaded) return;
        _cameraNamesLoaded = true;
        try
        {
            var devices = await Task.Run(() => new DeviceCheckBackend().Cameras());
            foreach (var device in devices.Where(d => d.CameraNumber is >= 1 and <= 10))
                CameraChoices[device.CameraNumber - 1].Name = $"{device.CameraNumber}. {device.Name}";
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            _cameraNamesLoaded = false;
        }
    }

    private void MonitorInfo_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show(this, string.Join(Environment.NewLine, StageDisplayChoices.Skip(1).Select(d => d.Name)), UiText.Get("SettingsWindow", "TabMonitors"));

    public sealed partial class NumberedChoice(int id, string name) : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public int Id { get; } = id;
        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty] private string _name = name;
    }
    public sealed record DisplayChoice(string Id, string Name);
    public IReadOnlyList<DisplayChoice> DisplayChoices { get; }
    public IReadOnlyList<DisplayChoice> StageDisplayChoices { get; }

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
            MessageBox.Show(this, ex.Message, UiText.Get("SettingsWindow", "BibleList"));
        }
    }

    private async void FolderList_Loaded(object sender, RoutedEventArgs e) => await LoadFolderChoicesAsync();

    private async Task LoadFolderChoicesAsync()
    {
        if (_foldersLoaded) return;
        _foldersLoaded = true;
        await _library.LoadFolderOptionsAsync();
        foreach (var folder in _library.Folders.Where(f => f.FolderNo is >= 1 and <= 40)) _folderChoices.Add(new(folder));
        for (int number = 1; number <= 40; number++)
        {
            var match = _folderChoices.FirstOrDefault(f => f.Id == number);
            JumpFolders.Add(new(number, match?.Name ?? UiText.Format("SettingsWindow", "FolderNumber", number)));
        }
        FolderList.ItemsSource = _folderChoices;
        FolderList.SelectedItem = _folderChoices.FirstOrDefault(f => f.Id == _library.SelectedFolder?.FolderNo) ?? _folderChoices.FirstOrDefault(f => f.IsEnabled) ?? _folderChoices.FirstOrDefault();
        FolderStatus.Text = _library.Folders.Count == 0 ? _library.StatusMessage : "";
    }

    private void RenameFolder_Click(object sender, RoutedEventArgs e)
    {
        if (FolderList.SelectedItem is not OptionsFolderDraft selected) return;
        var dialog = new NameEntryWindow(new NameEntryViewModel(UiText.Get("SettingsWindow", "RenameFolder"), UiText.Get("SettingsWindow", "NewName"), selected.Name,
            _folderChoices.Where(f => f != selected).Select(f => f.Name))) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        selected.Name = dialog.EnteredName;
        int index = JumpFolders.ToList().FindIndex(f => f.Id == selected.Id);
        if (index >= 0) JumpFolders[index].Name = selected.Name;
    }

    private BibleViewModel? Catalog => BiblePanel.DataContext as BibleViewModel;

    private void BibleInfo_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog?.SelectedVersion is { } version)
            MessageBox.Show(this, string.Join(Environment.NewLine, version.Description, version.Copyright, version.FileName), version.Name);
    }

    private void BibleUp_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog?.SelectedVersion is { } version) Catalog.MoveVersionUp(version);
    }

    private void BibleDown_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog?.SelectedVersion is { } version) Catalog.MoveVersionDown(version);
    }

    private void RenameBible_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog?.SelectedVersion is not { } version) return;
        var dialog = new NameEntryWindow(new NameEntryViewModel(UiText.Get("SettingsWindow", "RenameBible"), UiText.Get("SettingsWindow", "NewName"), version.Name,
            Catalog.Versions.Select(v => v.Name))) { Owner = this };
        if (dialog.ShowDialog() == true) Catalog.RenameVersion(version, dialog.EnteredName);
    }

    private void RemoveBible_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog?.SelectedVersion is not { } version) return;
        if (MessageBox.Show(this, UiText.Format("SettingsWindow", "RemoveBibleConfirm", version.Name), UiText.Get("SettingsWindow", "BibleList"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
            Catalog.DeleteVersion(version);
            RefreshBibleCandidates();
        }
    }

    private void SearchBibles_Click(object sender, RoutedEventArgs e) => RefreshBibleCandidates();

    private void RefreshBibleCandidates()
    {
        BibleSearchList.ItemsSource = Catalog?.GetAddableVersions();
        if (BibleSearchList.Items.Count > 0) BibleSearchList.SelectedIndex = 0;
    }

    private void AddBible_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog == null || BibleSearchList.SelectedItem is not BibleAddableVersion entry) return;
        var dialog = new NameEntryWindow(new NameEntryViewModel(UiText.Get("SettingsWindow", "AddBible"), UiText.Get("SettingsWindow", "NameToShow"), entry.SuggestedName,
            Catalog.Versions.Select(v => v.Name))) { Owner = this };
        if (dialog.ShowDialog() == true && Catalog.AddVersion(entry.FileName, dialog.EnteredName)) RefreshBibleCandidates();
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

    private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
    {
        var powerPoint = sender is Button { Tag: "PowerPoint" };
        var current = powerPoint ? _options.PowerPointSourceFolder : _options.MediaSourceFolder;
        var picker = new Microsoft.Win32.OpenFolderDialog { Title = powerPoint ? UiText.Get("SettingsWindow", "ChoosePowerPointFolder") : UiText.Get("SettingsWindow", "ChooseAudioVideoFolder") };
        if (Directory.Exists(current)) picker.InitialDirectory = current;
        if (picker.ShowDialog(this) != true) return;
        if (powerPoint)
        {
            _options.PowerPointSourceFolder = picker.FolderName;
            _options.UsePowerPointTab = true;
        }
        else
        {
            _options.MediaSourceFolder = picker.FolderName;
            _options.UseMediaTab = true;
        }
    }

    private void BrowseMediaFolder_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog { Title = UiText.Get("SettingsWindow", "ChooseMediaFolder") };
        if (System.IO.Directory.Exists(_options.MediaDirectory)) picker.InitialDirectory = _options.MediaDirectory;
        if (picker.ShowDialog(this) == true) _options.MediaDirectory = picker.FolderName;
    }

    private void BrowseGapLogo_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Title = UiText.Get("SettingsWindow", "ChooseGapLogo"), Filter = UiText.Get("SettingsWindow", "ImageFilter") };
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
        IsEnabled = false;
        try
        {
            var folderEdits = _folderChoices.Select(f => f.GetChanges()).OfType<SongFolderWriteModel>().ToArray();
            bool libraryChanged = folderEdits.Length > 0;
            if (libraryChanged)
            {
                var report = await _folderEditor.CommitFoldersAsync(_library.DatabasePath, folderEdits);
                if (!report.Succeeded) { MessageBox.Show(this, string.Join("\n", report.Issues.Select(i => i.Message)), UiText.Get("SettingsWindow", "SaveFolders")); return; }
                foreach (var folder in _folderChoices) folder.AcceptChanges();
                await _library.LoadFolderOptionsAsync();
                if (Owner?.DataContext is MainViewModel shell) await shell.Library.LoadAsync();
            }
            if (_bibleCatalog?.Commit() == true) { await _liveBible.LoadAsync(); libraryChanged = true; }
            if (libraryChanged && Owner?.DataContext is MainViewModel main) main.RefreshLibraryFormatting();
            if (_options.ConfirmOptions()) DialogResult = true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException or System.Data.SQLite.SQLiteException)
        {
            MessageBox.Show(this, ex.Message, UiText.Get("SettingsWindow", "SaveOptions"));
        }
        finally { _saving = false; IsEnabled = true; }
    }
}
