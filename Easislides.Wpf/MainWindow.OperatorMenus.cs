using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Easislides.Wpf;

public partial class MainWindow
{
    private readonly RecentItemEdits _recentEdits = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasislidesNext", "recent-item-edits.json"));

    private void RecordSongEdit(SongEditorViewModel editor)
    {
        if (editor.SongId is int id)
            _recentEdits.Record(new(editor.DatabasePath, "song", id.ToString(CultureInfo.InvariantCulture), editor.Title));
    }

    private void RecentEdits_Open(object sender, RoutedEventArgs e)
    {
        RecentEditsMenu.Items.Clear();
        foreach (var entry in _recentEdits.Read(ResolveSongEditorDatabasePath(_viewModel)))
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
        var picker = new OpenFileDialog { Multiselect = true, Filter = filter };
        if (picker.ShowDialog(this) != true) return;
        try
        {
            Directory.CreateDirectory(folder);
            var report = await _services.GetRequiredService<IExternalFileOperationService>().ExecuteAsync(new(
                ExternalFileOperationKind.Copy, kind, ExternalFileDestinationKind.ExternalFolder, picker.FileNames, folder));
            _viewModel.StatusText = report.Succeeded ? $"{report.CreatedFilePaths.Count}개 파일을 가져왔습니다." : string.Join(" ", report.Issues.Select(i => i.Message));
            if (kind == ExternalFileItemKind.InfoScreen) _inlineInfoScreens?.Load();
            else if (kind == ExternalFileItemKind.PowerPoint) _inlinePowerPoint?.LoadCommand.Execute(null);
            else _inlineMedia?.LoadCommand.Execute(null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) { _viewModel.StatusText = ex.Message; }
    }

    private void EditItemDisplay_Click(object sender, RoutedEventArgs e)
    {
        var settings = _services.GetRequiredService<ISettingsService>();
        var dialog = new ItemDisplayOptionsWindow(_viewModel.SelectedItemFormatData,
            Rendering.LiveOutputRenderSettings.From(settings), settings.Get(EasiSettingKeys.LyricsMonitorShowNotations)) { Owner = this };
        if (dialog.ShowDialog() == true) _viewModel.ApplySelectedItemFormatDataTemplate(dialog.FormatData);
    }

    private string? _sessionTemplateFormat;

    private string SessionTemplatesDirectory() =>
        Path.Combine(_services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder, "Templates", "WorshipListsTemplates");

    private void LoadSessionTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = IndividualFormatTemplateFile.DialogFilter, InitialDirectory = SessionTemplatesDirectory() };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            string format = IndividualFormatTemplateFile.LoadFormatData(dialog.FileName);
            SessionFormatTemplate.Transfer(_services.GetRequiredService<ISettingsService>(), format, load: true);
            _sessionTemplateFormat = format;
            _viewModel.StatusText = $"세션 템플릿 적용: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Xml.XmlException or FormatException or OverflowException)
        { _viewModel.StatusText = ex.Message; }
    }

    private void SaveSessionTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = IndividualFormatTemplateFile.DialogFilter, InitialDirectory = SessionTemplatesDirectory(),
            DefaultExt = ".est", FileName = "Session.est" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            var format = SessionFormatTemplate.Transfer(_services.GetRequiredService<ISettingsService>(), _sessionTemplateFormat, load: false);
            IndividualFormatTemplateFile.SaveFormatData(dialog.FileName, format);
            _sessionTemplateFormat = format;
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
        if (MessageBox.Show(this, prompt, "EasiSlides", MessageBoxButton.YesNo, MessageBoxImage.Question,
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
                ? FolderListing.TabSeparated(songs, settings.Get(EasiSettingKeys.ReferenceAlertPickSeparator))
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
        string path = Path.Combine(AppContext.BaseDirectory, "Sys", "EasiSlidesHelp.chm");
        if (!File.Exists(path) || !_services.GetRequiredService<ISupportLauncher>().TryOpen(path)) OpenHelp_Click(sender, e);
    }
}
