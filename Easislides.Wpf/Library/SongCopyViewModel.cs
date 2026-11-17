using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Library;

public sealed record SongCopiedEventArgs(int? SongId, int FolderNo);

public sealed partial class SongCopyViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private SongSummary? _sourceSong;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private string _sourceFolderName = "";
    [ObservableProperty] private string _songTitle = "";
    [ObservableProperty] private string _copyTitle = "";
    [ObservableProperty] private int _songNumber;
    [ObservableProperty] private int? _createdSongId;
    [ObservableProperty] private SongFolderSummary? _selectedTargetFolder;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _isBusy;

    public SongCopyViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        CopyCommand = new AsyncRelayCommand(CopyAsync, CanCopy);
    }

    public event EventHandler<SongCopiedEventArgs>? Copied;

    public ObservableCollection<SongFolderSummary> TargetFolders { get; } = new();

    public IAsyncRelayCommand CopyCommand { get; }

    public void Load(
        string databasePath,
        SongSummary song,
        SongFolderSummary sourceFolder,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(song);
        ArgumentNullException.ThrowIfNull(sourceFolder);
        ArgumentNullException.ThrowIfNull(allFolders);

        _sourceSong = song;
        DatabasePath = NormalizePath(databasePath);
        SourceFolderName = sourceFolder.Name;
        SongTitle = song.Title;
        CopyTitle = BuildDefaultCopyTitle(song.Title);
        TargetFolders.ReplaceWith(allFolders);
        SelectedTargetFolder = TargetFolders.FirstOrDefault(folder => folder.FolderNo == sourceFolder.FolderNo)
            ?? TargetFolders.FirstOrDefault();
        SongNumber = NextSongNumber(SelectedTargetFolder);
        CreatedSongId = null;
        ValidationMessage = "";
        StatusMessage = "";
        CopyCommand.NotifyCanExecuteChanged();
    }

    public async Task CopyAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!Validate())
        {
            CopyCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        try
        {
            var source = _sourceSong!;
            var targetFolderNo = SelectedTargetFolder!.FolderNo;
            var backupRoot = ResolveBackupRoot();
            var song = new SongWriteModel(
                null,
                CopyTitle.Trim(),
                source.AlternateTitle.Trim(),
                targetFolderNo,
                SongNumber <= 0 ? NextSongNumber(SelectedTargetFolder) : SongNumber,
                source.Lyrics,
                Key: source.Key.Trim(),
                Category: source.Category.Trim());
            var report = await _adminDatabase
                .SaveSongAsync(DatabasePath, backupRoot, song)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "복사 실패"
                    : $"복사 실패: {detail}";
                return;
            }

            CreatedSongId = report.AffectedSongIds.Count > 0 ? report.AffectedSongIds[0] : null;
            ValidationMessage = "";
            StatusMessage = "복사되었습니다.";
            Copied?.Invoke(this, new SongCopiedEventArgs(CreatedSongId, targetFolderNo));
        }
        catch (Exception ex) when (IsRecoverableCopyException(ex))
        {
            StatusMessage = $"복사 실패: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            CopyCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnCopyTitleChanged(string value)
        => CopyCommand.NotifyCanExecuteChanged();

    partial void OnSelectedTargetFolderChanged(SongFolderSummary? value)
    {
        SongNumber = NextSongNumber(value);
        CopyCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
        => CopyCommand.NotifyCanExecuteChanged();

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

        if (_sourceSong is null)
        {
            ValidationMessage = "복사할 곡을 선택하세요.";
            return false;
        }

        if (SelectedTargetFolder is null)
        {
            ValidationMessage = "복사할 대상 폴더를 선택하세요.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CopyTitle))
        {
            ValidationMessage = "복사할 제목을 입력하세요.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanCopy()
        => !IsBusy && SelectedTargetFolder is not null && !string.IsNullOrWhiteSpace(CopyTitle);

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

    private static int NextSongNumber(SongFolderSummary? folder)
        => Math.Max((folder?.SongCount ?? 0) + 1, 1);

    private static string BuildDefaultCopyTitle(string title)
        => string.IsNullOrWhiteSpace(title) ? "복사본" : $"{title.Trim()} - 복사본";

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableCopyException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
