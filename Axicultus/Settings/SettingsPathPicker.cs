using System;
using System.IO;
using System.Threading.Tasks;
using Axicultus.Wpf.Localization;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace Axicultus.Wpf.Settings;

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
    private static string SettingsJsonFilter => UiText.Get("SettingsWindow", "SettingsFilter");
    private static string DatabaseFilter => UiText.Get("SettingsWindow", "DatabaseFilter");

    public Task<string?> PickWorkingFolderAsync(string initialPath)
        => PickFolderAsync(UiText.Get("SettingsWindow", "PickWorkingFolder"), initialPath);

    public Task<string?> PickAdminDatabaseAsync(string initialPath)
        => PickOpenFileAsync(UiText.Get("SettingsWindow", "PickAdminDb"), DatabaseFilter, initialPath);

    public Task<string?> PickDataBackupRootAsync(string initialPath)
        => PickFolderAsync(UiText.Get("SettingsWindow", "PickBackupRoot"), initialPath);

    public Task<string?> PickSettingsImportAsync(string initialPath)
        => PickOpenFileAsync(UiText.Get("SettingsWindow", "PickImportFile"), SettingsJsonFilter, initialPath);

    public Task<string?> PickSettingsExportAsync(string initialPath)
        => PickSaveFileAsync(UiText.Get("SettingsWindow", "PickExportFile"), SettingsJsonFilter, initialPath, "axicultus-settings.json");

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
