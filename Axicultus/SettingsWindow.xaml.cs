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
        var choices = new List<DisplayChoice> { new("", "자동 선택"), new("None", "출력 안 함") };
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
            "모든 설정을 기본값으로 되돌릴까요?\n\n다음은 그대로 둡니다: 작업·백업·PowerPoint·미디어 폴더와 데이터베이스 경로, 라이선스 정보, 등록 사용자,\n송출·무대 모니터 선택과 화면 위치, 현재 예배 순서와 창 위치, 메뉴 언어.\n확인(OK)을 눌러야 저장됩니다.",
            "기본값 복원", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;
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
        MessageBox.Show(this, string.Join(Environment.NewLine, StageDisplayChoices.Skip(1).Select(d => d.Name)), "Monitors");

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
            MessageBox.Show(this, ex.Message, "성경 목록");
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
            JumpFolders.Add(new(number, match?.Name ?? $"폴더 {number}"));
        }
        FolderList.ItemsSource = _folderChoices;
        FolderList.SelectedItem = _folderChoices.FirstOrDefault(f => f.Id == _library.SelectedFolder?.FolderNo) ?? _folderChoices.FirstOrDefault(f => f.IsEnabled) ?? _folderChoices.FirstOrDefault();
        FolderStatus.Text = _library.Folders.Count == 0 ? _library.StatusMessage : "";
    }

    private void RenameFolder_Click(object sender, RoutedEventArgs e)
    {
        if (FolderList.SelectedItem is not OptionsFolderDraft selected) return;
        var dialog = new NameEntryWindow(new NameEntryViewModel("폴더 이름 변경", "새 이름", selected.Name,
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
        var dialog = new NameEntryWindow(new NameEntryViewModel("성경 이름 변경", "새 이름", version.Name,
            Catalog.Versions.Select(v => v.Name))) { Owner = this };
        if (dialog.ShowDialog() == true) Catalog.RenameVersion(version, dialog.EnteredName);
    }

    private void RemoveBible_Click(object sender, RoutedEventArgs e)
    {
        if (Catalog?.SelectedVersion is not { } version) return;
        if (MessageBox.Show(this, $"'{version.Name}' 버전을 목록에서 제거할까요?", "성경 목록", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
        var dialog = new NameEntryWindow(new NameEntryViewModel("성경 추가", "표시할 이름", entry.SuggestedName,
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
        var picker = new Microsoft.Win32.OpenFolderDialog { Title = powerPoint ? "PowerPoint 폴더 선택" : "음원·동영상 폴더 선택" };
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
        IsEnabled = false;
        try
        {
            var folderEdits = _folderChoices.Select(f => f.GetChanges()).OfType<SongFolderWriteModel>().ToArray();
            bool libraryChanged = folderEdits.Length > 0;
            if (libraryChanged)
            {
                var report = await _folderEditor.CommitFoldersAsync(_library.DatabasePath, folderEdits);
                if (!report.Succeeded) { MessageBox.Show(this, string.Join("\n", report.Issues.Select(i => i.Message)), "폴더 저장"); return; }
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
            MessageBox.Show(this, ex.Message, "옵션 저장");
        }
        finally { _saving = false; IsEnabled = true; }
    }
}
