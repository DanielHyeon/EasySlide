using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace Easislides.Wpf.Settings;

public interface ISettingsPathPicker
{
    Task<string?> PickWorkingFolderAsync(string initialPath);

    Task<string?> PickAdminDatabaseAsync(string initialPath);

    Task<string?> PickDataBackupRootAsync(string initialPath);

    Task<string?> PickSettingsImportAsync(string initialPath);

    Task<string?> PickSettingsExportAsync(string initialPath);
}

public sealed class SettingsPathPicker : ISettingsPathPicker
{
    private const string SettingsJsonFilter = "EasiSlides settings (*.json)|*.json|All files (*.*)|*.*";
    private const string DatabaseFilter = "SQLite/AdminDB (*.db;*.sqlite;*.sqlite3)|*.db;*.sqlite;*.sqlite3|All files (*.*)|*.*";

    public Task<string?> PickWorkingFolderAsync(string initialPath)
        => PickFolderAsync("작업 폴더 선택", initialPath);

    public Task<string?> PickAdminDatabaseAsync(string initialPath)
        => PickOpenFileAsync("AdminDB 선택", DatabaseFilter, initialPath);

    public Task<string?> PickDataBackupRootAsync(string initialPath)
        => PickFolderAsync("백업 루트 선택", initialPath);

    public Task<string?> PickSettingsImportAsync(string initialPath)
        => PickOpenFileAsync("설정 가져오기 파일 선택", SettingsJsonFilter, initialPath);

    public Task<string?> PickSettingsExportAsync(string initialPath)
        => PickSaveFileAsync("설정 내보내기 파일 선택", SettingsJsonFilter, initialPath, "easislides-settings.json");

    private static Task<string?> PickFolderAsync(string title, string initialPath)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(initialPath) ? initialPath : "",
            ShowNewFolderButton = true,
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath)
            ? dialog.SelectedPath
            : null);
    }

    private static Task<string?> PickOpenFileAsync(string title, string filter, string initialPath)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true,
            Multiselect = false,
        };
        ApplyInitialPath(dialog, initialPath, defaultFileName: "");

        return Task.FromResult(dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FileName)
            ? dialog.FileName
            : null);
    }

    private static Task<string?> PickSaveFileAsync(
        string title,
        string filter,
        string initialPath,
        string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter,
            AddExtension = true,
            DefaultExt = ".json",
            OverwritePrompt = true,
        };
        ApplyInitialPath(dialog, initialPath, defaultFileName);

        return Task.FromResult(dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FileName)
            ? dialog.FileName
            : null);
    }

    private static void ApplyInitialPath(FileDialog dialog, string initialPath, string defaultFileName)
    {
        if (!string.IsNullOrWhiteSpace(initialPath))
        {
            var fullPath = Path.GetFullPath(initialPath);
            var directory = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
            }

            if (!Directory.Exists(fullPath))
            {
                dialog.FileName = Path.GetFileName(fullPath);
                return;
            }
        }

        dialog.FileName = defaultFileName;
    }
}
