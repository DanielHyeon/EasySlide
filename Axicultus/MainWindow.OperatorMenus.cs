using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Axicultus.Wpf;

public partial class MainWindow
{
    private readonly RecentItemEdits _recentEdits;

    private void RecordSongEdit(SongEditorViewModel editor)
    {
        if (editor.SongId is int id)
            _recentEdits.Record(new(editor.DatabasePath, "song", id.ToString(CultureInfo.InvariantCulture), editor.Title), _viewModel.EditHistoryLimit);
    }

    private void RecentEdits_Open(object sender, RoutedEventArgs e)
    {
        RecentEditsMenu.Items.Clear();
        foreach (var entry in _recentEdits.Read(ResolveSongEditorDatabasePath(_viewModel), _viewModel.EditHistoryLimit))
        {
            var item = new MenuItem { Header = entry.Title, Tag = entry };
            item.Click += ReopenEdit_Click;
            RecentEditsMenu.Items.Add(item);
        }
        if (RecentEditsMenu.Items.Count == 0) RecentEditsMenu.Items.Add(new MenuItem { Header = "최근 편집 항목 없음", IsEnabled = false });
    }

    private async void ReopenEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: EditedItem item }) return;
        if (item.Kind == "song" && int.TryParse(item.Key, out int id))
            await OpenPraiseBookSongEditorAsync(_viewModel, new PraiseBookIndexEntry(item.Title, 0, id));
        else if (item.Kind == "info")
            await OpenInfoScreenEditorAsync(item.Key);
        else if (item.Kind == "file" && IsEditableExternalMedia(item.Key))
            LaunchExternalFileForEdit(item.Key, "File");
    }

    private static bool IsEditableExternalMedia(string path)
        => Path.GetExtension(path).ToLowerInvariant() is ".ppt" or ".pptx" or ".pps" or ".ppsx" or ".txt" or ".doc" or ".docx"
            or ".mp4" or ".m4v" or ".mov" or ".avi" or ".wmv" or ".mpg" or ".mpeg" or ".mkv" or ".webm"
            or ".mp3" or ".wav" or ".wma" or ".m4a" or ".aac" or ".flac" or ".ogg";

    private void OpenFolderImport_Click(object sender, RoutedEventArgs e)
    {
        var window = _services.GetRequiredService<ImportExportWindow>();
        window.Owner = this;
        window.StartFolderImport();
        window.ShowDialog();
    }

    private void OpenExport_Click(object sender, RoutedEventArgs e)
    {
        var window = _services.GetRequiredService<ImportExportWindow>();
        window.Owner = this;
        window.SelectExport();
        window.ShowDialog();
    }

    private string? SelectedSourceFolder() => (LeftBrowserTabs.SelectedItem as TabItem)?.Tag switch
    {
        "InfoScreenSource" => Path.Combine(_services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder,
            "InfoScreens", _inlineInfoScreens?.SelectedFolder?.RelativePath ?? ""),
        "PowerPointSource" => _inlinePowerPoint?.FolderPath,
        "MediaSource" => _inlineMedia?.FolderPath,
        "ImagesSource" => _inlineImages?.FolderPath,
        _ => null
    };

    private void OpenSourceFolder_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSourceFolder() is not { Length: > 0 } folder) return;
        try
        {
            Directory.CreateDirectory(folder);
            if (!_services.GetRequiredService<ISupportLauncher>().TryOpen(Path.GetFullPath(folder))) _viewModel.StatusText = "폴더를 열 수 없습니다.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) { _viewModel.StatusText = ex.Message; }
    }

    private async void ImportSourceFiles_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSourceFolder() is not { Length: > 0 } folder) return;
        var (kind, filter) = (LeftBrowserTabs.SelectedItem as TabItem)?.Tag switch
        {
            "InfoScreenSource" => (ExternalFileItemKind.InfoScreen, "정보 화면|*.esi"),
            "PowerPointSource" => (ExternalFileItemKind.PowerPoint, "PowerPoint|*.ppt;*.pptx;*.pps;*.ppsx;*.pptm;*.ppsm"),
            _ => (ExternalFileItemKind.Media, "미디어|*.mp4;*.avi;*.wmv;*.mkv;*.webm;*.mpg;*.mpeg;*.mov;*.m4v;*.mp3;*.wav;*.m4a;*.aac;*.wma;*.ogg;*.flac")
        };
        var picker = new OpenFileDialog { Multiselect = true, Filter = filter, Title = $"{kind} 가져오기 · {folder}" };
        if (picker.ShowDialog(this) != true) return;
        var imported = new List<string>();
        var errors = new List<string>();
        var sources = picker.FileNames;
        try
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(folder);
                foreach (var source in sources)
                {
                    var destination = Path.Combine(folder, Path.GetFileName(source));
                    try
                    {
                        File.Copy(source, destination, overwrite: false);
                        imported.Add(destination);
                    }
                    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                    {
                        errors.Add(error.Message);
                    }
                }
            });
            _viewModel.StatusText = errors.Count > 0 ? string.Join(Environment.NewLine, errors) : $"{imported.Count}개 파일을 가져왔습니다.";
            if (!string.Equals(folder, SelectedSourceFolder(), StringComparison.OrdinalIgnoreCase)) return;
            ShowTransferredFiles(kind, imported);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) { _viewModel.StatusText = ex.Message; }
    }

    private void ShowTransferredFiles(ExternalFileItemKind kind, IReadOnlyList<string> paths)
    {
        if (paths.Count == 0 || Path.GetDirectoryName(paths[0]) is not { } folder) return;
        var files = paths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        switch (kind)
        {
            case ExternalFileItemKind.InfoScreen when _inlineInfoScreens is { } info:
                var root = Path.Combine(_services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder, "InfoScreens");
                var relative = Path.GetRelativePath(root, folder);
                info.Load();
                info.SelectedFolder = info.Folders.FirstOrDefault(item => string.Equals(item.RelativePath, relative == "." ? "" : relative, StringComparison.OrdinalIgnoreCase));
                SelectFiles(InlineInfoScreenList, info.Screens.Where(item => files.Contains(Path.Combine(root, item.Name + ".esi"))));
                break;
            case ExternalFileItemKind.PowerPoint when _inlinePowerPoint is { } ppt:
                ppt.ChangeSourceFolder(folder);
                if (ppt.IsListStyle)
                    SelectFiles(InlinePowerPointList, ppt.Presentations.Where(item => files.Contains(item.FilePath)));
                break;
            case ExternalFileItemKind.Media when _inlineMedia is { } media:
                media.ChangeSourceFolder(folder);
                SelectFiles(InlineMediaList, media.MediaFiles.Where(item => files.Contains(item.FilePath)));
                break;
        }

        static void SelectFiles(ListBox list, IEnumerable<object> matches)
        {
            list.UnselectAll();
            foreach (var item in matches) list.SelectedItems.Add(item);
            if (list.SelectedItem is { } selected) list.ScrollIntoView(selected);
        }
    }

    private void EditItemDisplay_Click(object sender, RoutedEventArgs e)
    {
        var settings = _services.GetRequiredService<ISettingsService>();
        var dialog = new ItemDisplayOptionsWindow(_viewModel.SelectedItemFormatData,
            Rendering.LiveOutputRenderSettings.From(settings), settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations), _viewModel.PreviewFormatLayout) { Owner = this };
        if (dialog.ShowDialog() == true) _viewModel.ApplySelectedItemFormatDataTemplate(dialog.FormatData);
    }

    private string SessionTemplatesDirectory() =>
        Path.Combine(_services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder, "Admin", "Templates", "WorshipListsTemplates");

    private async void LoadSessionTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Load Session Settings from a Template", Filter = IndividualFormatTemplateFile.DialogFilter, InitialDirectory = SessionTemplatesDirectory() };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            string format = IndividualFormatTemplateFile.LoadFormatData(dialog.FileName);
            if (!await _viewModel.ApplySessionTemplateAsync(format)) return;
            _viewModel.StatusText = $"세션 템플릿 적용: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Xml.XmlException or FormatException or OverflowException)
        { _viewModel.StatusText = ex.Message; }
    }

    private void SaveSessionTemplate_Click(object sender, RoutedEventArgs e)
    {
        var name = Path.GetFileName(_viewModel.CurrentWorshipListName);
        var dialog = new SaveFileDialog { Title = "Save Current Session Settings to a Template",
            Filter = IndividualFormatTemplateFile.DialogFilter, InitialDirectory = SessionTemplatesDirectory(),
            DefaultExt = ".est", FileName = $"{(string.IsNullOrWhiteSpace(name) ? "Session" : name)}.est" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            var format = _viewModel.GetSessionFormat();
            IndividualFormatTemplateFile.SaveFormatData(dialog.FileName, format);
            _viewModel.StatusText = $"세션 템플릿 저장: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException or ArgumentException)
        { _viewModel.StatusText = ex.Message; }
    }

    private void OpenUsageReport_Click(object sender, RoutedEventArgs e)
    {
        var window = _services.GetRequiredService<SearchUsageWindow>();
        window.Owner = this;
        window.SelectUsage();
        window.ShowDialog();
    }

    private async void AddListUsages_Click(object sender, RoutedEventArgs e) => await _viewModel.AddWorshipListSongsToUsagesAsync();

    private void EmptyDeleted_Click(object sender, RoutedEventArgs e) =>
        ConfirmSongMaintenance(AdminDatabaseWriteOperation.EmptyDeletedSongs, "삭제 폴더의 곡을 영구 삭제하시겠습니까?");

    private void ClearDatabaseFormats_Click(object sender, RoutedEventArgs e) =>
        ConfirmSongMaintenance(AdminDatabaseWriteOperation.ClearSongFormatting, "곡 데이터베이스의 개별 서식을 모두 지우시겠습니까?");

    private async void ConfirmSongMaintenance(AdminDatabaseWriteOperation operation, string prompt)
    {
        if (MessageBox.Show(this, prompt, "Axicultus", MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No) != MessageBoxResult.Yes) return;
        await EnsureLibraryLoadedOnceAsync();
        await _viewModel.Library.MaintainSongsAsync(operation);
        _viewModel.StatusText = _viewModel.Library.StatusMessage;
    }

    private async void ExportFolderListing_Click(object sender, RoutedEventArgs e)
    {
        await EnsureLibraryLoadedOnceAsync();
        var library = _viewModel.Library;
        if (library.SelectedFolder is not { } folder) { _viewModel.StatusText = "곡 폴더를 선택하세요."; return; }
        var dialog = new SaveFileDialog { FileName = "Index Of Database Items.rtf", Filter = "서식 있는 문서|*.rtf|탭 구분 텍스트|*.txt", AddExtension = true };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            var songs = await _services.GetRequiredService<IAdminDatabaseRepository>().GetSongsAsync(library.DatabasePath, folder.FolderNo);
            var settings = _services.GetRequiredService<ISettingsService>();
            var report = Path.GetExtension(dialog.FileName).Equals(".txt", StringComparison.OrdinalIgnoreCase)
                ? FolderListing.TabSeparated(songs, settings.Get(AxicultusSettingKeys.ReferenceAlertPickSeparator))
                : new PraiseBookIndexExporter().BuildRtf(folder.Name, new PraiseBookIndexService().BuildIndex(
                    songs.Select(song => new PraiseBookIndexEntry(song.Title, song.SongNumber, song.SongId))));
            await File.WriteAllTextAsync(dialog.FileName, report);
            _viewModel.StatusText = dialog.FileName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Data.Common.DbException or InvalidOperationException)
        { _viewModel.StatusText = ex.Message; }
    }

    private void WebHelp_Click(object sender, RoutedEventArgs e)
    {
        if (!_services.GetRequiredService<ISupportLauncher>().TryOpen("https://www.easislides.com/help"))
            _viewModel.StatusText = "온라인 도움말을 열 수 없습니다.";
    }

    private void HelpContents_Click(object sender, RoutedEventArgs e)
    {
        string path = ProductStorage.HelpFileIn(Path.Combine(AppContext.BaseDirectory, "Sys"));
        if (!File.Exists(path) || !_services.GetRequiredService<ISupportLauncher>().TryOpen(path)) OpenHelp_Click(sender, e);
    }
}
