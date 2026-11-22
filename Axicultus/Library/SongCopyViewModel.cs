using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Library;

public sealed record SongCopiedEventArgs(int? SongId, int FolderNo);

public sealed partial class SongCopyViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private IReadOnlyList<SongSummary> _sourceSongs = [];

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
    // One song: its copy can be given a title and a number. Several: each copy keeps its own, as in WinForms.
    [ObservableProperty] private bool _isSingleSong = true;

    public SongCopyViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        CopyCommand = new AsyncRelayCommand(CopyAsync, CanCopy);
    }

    public event EventHandler<SongCopiedEventArgs>? Copied;

    public ObservableCollection<SongFolderSummary> TargetFolders { get; } = [];

    public IAsyncRelayCommand CopyCommand { get; }

    /// <summary>The ids of the copies, in the order of the selection.</summary>
    public IReadOnlyList<int> CreatedSongIds { get; private set; } = [];

    public void Load(
        string databasePath,
        SongSummary song,
        SongFolderSummary sourceFolder,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(song);
        ArgumentNullException.ThrowIfNull(sourceFolder);
        ArgumentNullException.ThrowIfNull(allFolders);

        Load(databasePath, [song], allFolders);
        SourceFolderName = sourceFolder.Name;
        SelectedTargetFolder = TargetFolders.FirstOrDefault(folder => folder.FolderNo == sourceFolder.FolderNo)
            ?? TargetFolders.FirstOrDefault();
    }

    /// <summary>Loads every selected song (FrmMain copies the whole selection of its song list).</summary>
    public void Load(
        string databasePath,
        IReadOnlyList<SongSummary> songs,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(songs);
        ArgumentNullException.ThrowIfNull(allFolders);

        _sourceSongs = songs;
        IsSingleSong = songs.Count <= 1;
        DatabasePath = NormalizePath(databasePath);
        SourceFolderName = SongSelectionText.SourceFolderName(songs, allFolders);
        SongTitle = SongSelectionText.Heading(songs);
        CopyTitle = songs.Count == 1 ? BuildDefaultCopyTitle(songs[0].Title) : "";
        // Folder 0 holds deleted songs, and a disabled folder is shown by neither app.
        TargetFolders.ReplaceWith(allFolders.Where(folder => folder.FolderNo > 0 && folder.IsEnabled));
        SelectedTargetFolder = TargetFolders.FirstOrDefault(folder => folder.FolderNo == SongSelectionText.CommonFolderNo(songs))
            ?? TargetFolders.FirstOrDefault();
        SongNumber = songs.Count == 1 ? songs[0].SongNumber : 0; // The copy keeps the number, as in WinForms; 0 is "no number".
        CreatedSongId = null;
        CreatedSongIds = [];
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
            var targetFolderNo = SelectedTargetFolder!.FolderNo;
            var backupRoot = ResolveBackupRoot();
            var copies = new List<SongWriteModel>();
            var missing = new List<string>();
            foreach (var source in _sourceSongs)
            {
                // Of one song the dialog sets title and number; of several each copy keeps its own, as in WinForms.
                string title = IsSingleSong ? CopyTitle.Trim() : source.Title;
                int songNumber = IsSingleSong ? Math.Max(SongNumber, 0) : source.SongNumber;
                // The list row holds only part of the song; WinForms copies every column.
                if (_adminDatabase is not IAdminSongDetailRepository details)
                {
                    copies.Add(new SongWriteModel(
                        null,
                        title,
                        source.AlternateTitle.Trim(),
                        targetFolderNo,
                        songNumber,
                        source.Lyrics,
                        Key: source.Key.Trim(),
                        Category: source.Category.Trim()));
                    continue;
                }

                var detail = await details.GetSongDetailAsync(DatabasePath, source.SongId).ConfigureAwait(true);
                if (detail is null)
                {
                    // FrmMain leaves out a selected song whose row is gone.
                    missing.Add(source.Title);
                    continue;
                }

                copies.Add(new SongWriteModel(
                    null, IsSingleSong ? title : detail.Title, detail.AlternateTitle, targetFolderNo,
                    IsSingleSong ? songNumber : detail.SongNumber, detail.Lyrics,
                    detail.Sequence, detail.Writer, detail.Copyright, detail.Capo, detail.Timing, detail.Key,
                    detail.Notations, detail.Category, detail.LicenceAdmin1, detail.LicenceAdmin2,
                    detail.BookReference, detail.UserReference, detail.Settings, detail.FormatData,
                    SequenceIsStored: true));
            }

            if (copies.Count == 0)
            {
                StatusMessage = "복사 실패: 원본 곡을 찾을 수 없습니다.";
                return;
            }

            // One backup and one transaction for the whole selection: all the copies are made, or none.
            var report = await _adminDatabase
                .SaveSongsAsync(DatabasePath, backupRoot, copies)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? "복사 실패"
                    : $"복사 실패: {detail}";
                return;
            }

            CreatedSongIds = report.AffectedSongIds;
            CreatedSongId = report.AffectedSongIds.Count > 0 ? report.AffectedSongIds[0] : null;
            ValidationMessage = "";
            StatusMessage = IsSingleSong
                ? "복사되었습니다."
                : $"{copies.Count}곡을 복사했습니다." + SongSelectionText.Skipped(missing, "라이브러리에 없음");
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
        => CopyCommand.NotifyCanExecuteChanged();

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

        if (_sourceSongs.Count == 0)
        {
            ValidationMessage = "복사할 곡을 선택하세요.";
            return false;
        }

        if (SelectedTargetFolder is null)
        {
            ValidationMessage = "복사할 대상 폴더를 선택하세요.";
            return false;
        }

        if (IsSingleSong && string.IsNullOrWhiteSpace(CopyTitle))
        {
            ValidationMessage = "복사할 제목을 입력하세요.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanCopy()
        => !IsBusy && SelectedTargetFolder is not null && (!IsSingleSong || !string.IsNullOrWhiteSpace(CopyTitle));

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

    private static string BuildDefaultCopyTitle(string title)
        => string.IsNullOrWhiteSpace(title) ? "복사본" : $"{title.Trim()} - 복사본";

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableCopyException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or System.Data.Common.DbException // reading the source song
            or NotSupportedException;
}
