using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed record FolderEditorSavedEventArgs(int FolderNo);

public sealed partial class FolderEditorViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private bool _loading;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private int _folderNo;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _hasChanges;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isNew = true;

    public FolderEditorViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    public event EventHandler<FolderEditorSavedEventArgs>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

    public void Load(
        string databasePath,
        SongFolderSummary? folder,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(allFolders);

        _loading = true;
        try
        {
            DatabasePath = NormalizePath(databasePath);
            FolderNo = folder?.FolderNo ?? NextFolderNumber(allFolders);
            Name = folder?.Name ?? "";
            IsEnabled = folder?.IsEnabled ?? true;
            ValidationMessage = "";
            StatusMessage = "";
            IsNew = folder is null;
            HasChanges = false;
        }
        finally
        {
            _loading = false;
        }

        SaveCommand.NotifyCanExecuteChanged();
    }

    public async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!Validate())
        {
            SaveCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        try
        {
            var backupRoot = ResolveBackupRoot();
            var folder = new SongFolderWriteModel(FolderNo, Name.Trim(), IsEnabled);
            var report = await _adminDatabase
                .SaveFolderAsync(DatabasePath, backupRoot, folder)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "저장 실패"
                    : $"저장 실패: {detail}";
                return;
            }

            if (report.AffectedFolderNos.Count > 0)
            {
                FolderNo = report.AffectedFolderNos[0];
            }

            IsNew = false;
            HasChanges = false;
            ValidationMessage = "";
            StatusMessage = "저장되었습니다.";
            Saved?.Invoke(this, new FolderEditorSavedEventArgs(FolderNo));
        }
        catch (Exception ex) when (IsRecoverableSaveException(ex))
        {
            StatusMessage = $"저장 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnNameChanged(string value) => MarkChanged();

    partial void OnIsEnabledChanged(bool value) => MarkChanged();

    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    private void MarkChanged()
    {
        if (_loading)
        {
            return;
        }

        HasChanges = true;
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath))
        {
            ValidationMessage = "AdminDB 경로를 설정해야 합니다.";
            return false;
        }

        if (!File.Exists(DatabasePath))
        {
            ValidationMessage = $"AdminDB 파일을 찾을 수 없습니다: {DatabasePath}";
            return false;
        }

        if (FolderNo <= 0)
        {
            ValidationMessage = "폴더 번호가 올바르지 않습니다.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "폴더 이름을 입력하세요.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanSave()
        => !IsBusy && !string.IsNullOrWhiteSpace(Name);

    private string ResolveBackupRoot()
    {
        var configured = _settings.Current.Data.BackupRoot;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return NormalizePath(configured);
        }

        var databaseDirectory = Path.GetDirectoryName(DatabasePath);
        return Path.Combine(
            string.IsNullOrWhiteSpace(databaseDirectory) ? Environment.CurrentDirectory : databaseDirectory,
            "Backups");
    }

    private static int NextFolderNumber(IReadOnlyList<SongFolderSummary> folders)
        => folders.Count == 0 ? 1 : folders.Max(folder => folder.FolderNo) + 1;

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableSaveException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
