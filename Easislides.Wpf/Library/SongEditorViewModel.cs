using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed record SongEditorSavedEventArgs(int? SongId, int FolderNo);

public sealed partial class SongEditorViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private bool _loading;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private int? _songId;
    [ObservableProperty] private int _folderNo;
    [ObservableProperty] private string _folderName = "";
    [ObservableProperty] private int _songNumber;
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _alternateTitle = "";
    [ObservableProperty] private string _category = "";
    [ObservableProperty] private string _key = "";
    [ObservableProperty] private string _lyrics = "";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _hasChanges;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isNew = true;

    public SongEditorViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    public event EventHandler<SongEditorSavedEventArgs>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

    public void Load(string databasePath, SongFolderSummary folder, SongSummary? song)
    {
        ArgumentNullException.ThrowIfNull(folder);
        _loading = true;
        try
        {
            DatabasePath = NormalizePath(databasePath);
            SongId = song?.SongId;
            FolderNo = folder.FolderNo;
            FolderName = folder.Name;
            SongNumber = song?.SongNumber ?? NextSongNumber(folder);
            Title = song?.Title ?? "";
            AlternateTitle = song?.AlternateTitle ?? "";
            Category = song?.Category ?? "";
            Key = song?.Key ?? "";
            Lyrics = song?.Lyrics ?? "";
            ValidationMessage = "";
            StatusMessage = "";
            IsNew = song is null;
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
            var song = new SongWriteModel(
                SongId,
                Title.Trim(),
                AlternateTitle.Trim(),
                FolderNo,
                SongNumber,
                Lyrics,
                Key: Key.Trim(),
                Category: Category.Trim());
            var report = await _adminDatabase
                .SaveSongAsync(DatabasePath, backupRoot, song)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "저장 실패"
                    : $"저장 실패: {detail}";
                return;
            }

            if (SongId is null && report.AffectedSongIds.Count > 0)
            {
                SongId = report.AffectedSongIds[0];
            }

            IsNew = false;
            HasChanges = false;
            ValidationMessage = "";
            StatusMessage = "저장되었습니다.";
            Saved?.Invoke(this, new SongEditorSavedEventArgs(SongId, FolderNo));
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

    partial void OnTitleChanged(string value) => MarkChanged();

    partial void OnAlternateTitleChanged(string value) => MarkChanged();

    partial void OnCategoryChanged(string value) => MarkChanged();

    partial void OnKeyChanged(string value) => MarkChanged();

    partial void OnLyricsChanged(string value) => MarkChanged();

    partial void OnSongNumberChanged(int value) => MarkChanged();

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
            ValidationMessage = "저장할 폴더를 선택하세요.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ValidationMessage = "제목을 입력하세요.";
            return false;
        }

        if (SongNumber <= 0)
        {
            SongNumber = SongId ?? 1;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanSave()
        => !IsBusy && !string.IsNullOrWhiteSpace(Title);

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

    private static int NextSongNumber(SongFolderSummary folder)
        => Math.Max(folder.SongCount + 1, 1);

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableSaveException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
