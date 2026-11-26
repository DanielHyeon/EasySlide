using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Library;

public sealed record SongMergedEventArgs(int CreatedCount, int TargetFolderNo);

public sealed partial class SongMergeCandidate(
    int sourceSongAId,
    int sourceSongBId,
    string mergeTitle,
    string sourceATitle,
    string sourceBTitle) : ObservableObject
{
    public int SourceSongAId { get; } = sourceSongAId;

    public int SourceSongBId { get; } = sourceSongBId;

    public string MergeTitle { get; } = mergeTitle;

    public string SourceATitle { get; } = sourceATitle;

    public string SourceBTitle { get; } = sourceBTitle;

    [ObservableProperty] private bool _isSelected = true;
}

public sealed partial class SongMergeViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private readonly IAdminSongDetailRepository _songDetails;
    private readonly ISongMergeService _mergeService;

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private SongFolderSummary? _selectedSourceFolderA;
    [ObservableProperty] private SongFolderSummary? _selectedSourceFolderB;
    [ObservableProperty] private SongFolderSummary? _selectedTargetFolder;
    [ObservableProperty] private bool _useSourceAAlternateTitle;
    [ObservableProperty] private bool _useSourceBAlternateTitle;
    [ObservableProperty] private bool _appendSourceBTitleToMergedTitle;
    [ObservableProperty] private bool _areAllCandidatesSelected = true;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private int _createdCount;
    [ObservableProperty] private bool _isBusy;

    public SongMergeViewModel(
        ISettingsService settings,
        IAdminDatabaseRepository adminDatabase,
        IAdminSongDetailRepository songDetails,
        ISongMergeService mergeService)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        _songDetails = songDetails ?? throw new ArgumentNullException(nameof(songDetails));
        _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
        RefreshMatchesCommand = new AsyncRelayCommand(RefreshMatchesAsync, () => !IsBusy);
        MergeCommand = new AsyncRelayCommand(MergeAsync, () => !IsBusy);
    }

    public event EventHandler<SongMergedEventArgs>? Merged;

    public ObservableCollection<SongFolderSummary> Folders { get; } = [];

    public ObservableCollection<SongMergeCandidate> Candidates { get; } = [];

    public IAsyncRelayCommand RefreshMatchesCommand { get; }

    public IAsyncRelayCommand MergeCommand { get; }

    public void Load(string databasePath, IReadOnlyList<SongFolderSummary> folders)
    {
        ArgumentNullException.ThrowIfNull(folders);

        DatabasePath = NormalizePath(databasePath);
        // As in WinForms, only folders in use; folder 0 holds deleted songs.
        Folders.ReplaceWith(folders.Where(folder => folder.FolderNo > 0 && folder.IsEnabled));
        SelectedSourceFolderA = Folders.FirstOrDefault();
        SelectedSourceFolderB = Folders.Skip(1).FirstOrDefault() ?? Folders.FirstOrDefault();
        SelectedTargetFolder = Folders.Skip(2).FirstOrDefault() ?? Folders.FirstOrDefault();
        Candidates.Clear();
        ValidationMessage = "";
        StatusMessage = "";
        CreatedCount = 0;
        AreAllCandidatesSelected = true;
        NotifyCommands();
    }

    public async Task RefreshMatchesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateFolders(requireCandidates: false))
        {
            NotifyCommands();
            return;
        }

        IsBusy = true;
        try
        {
            var sourceA = await _adminDatabase.GetSongsAsync(DatabasePath, SelectedSourceFolderA!.FolderNo).ConfigureAwait(true);
            var sourceB = await _adminDatabase.GetSongsAsync(DatabasePath, SelectedSourceFolderB!.FolderNo).ConfigureAwait(true);
            var bLookup = sourceB
                .Select(song => (Key: MatchKey(song, UseSourceBAlternateTitle), Song: song))
                .Where(item => !string.IsNullOrWhiteSpace(item.Key))
                .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Select(item => item.Song).ToArray(), StringComparer.OrdinalIgnoreCase);

            var candidates = new List<SongMergeCandidate>();
            foreach (var songA in sourceA)
            {
                var key = MatchKey(songA, UseSourceAAlternateTitle);
                if (string.IsNullOrWhiteSpace(key) || !bLookup.TryGetValue(key, out var matches))
                {
                    continue;
                }

                foreach (var songB in matches)
                {
                    candidates.Add(new SongMergeCandidate(
                        songA.SongId,
                        songB.SongId,
                        BuildMergeTitle(songA.Title, songB.Title),
                        songA.Title,
                        songB.Title));
                }
            }

            Candidates.ReplaceWith(candidates.OrderBy(candidate => candidate.MergeTitle, StringComparer.OrdinalIgnoreCase));
            foreach (var candidate in Candidates)
            {
                candidate.IsSelected = AreAllCandidatesSelected;
            }

            ValidationMessage = "";
            StatusMessage = UiText.Format("SongMergeWindow", "CandidatesFound", Candidates.Count);
        }
        catch (Exception ex) when (IsRecoverableMergeException(ex))
        {
            StatusMessage = UiText.Format("SongMergeWindow", "FindFailed", ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    public async Task MergeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!ValidateFolders(requireCandidates: true))
        {
            NotifyCommands();
            return;
        }

        var selected = Candidates.Where(candidate => candidate.IsSelected).ToArray();
        if (selected.Length == 0)
        {
            ValidationMessage = UiText.Get("SongMergeWindow", "SelectCandidate");
            NotifyCommands();
            return;
        }

        IsBusy = true;
        try
        {
            var backupRoot = ResolveBackupRoot();
            // The merged songs are written together: one backup and one transaction for the whole merge.
            var songs = new List<SongWriteModel>();
            foreach (var candidate in selected)
            {
                var sourceA = await _songDetails.GetSongDetailAsync(DatabasePath, candidate.SourceSongAId).ConfigureAwait(true);
                var sourceB = await _songDetails.GetSongDetailAsync(DatabasePath, candidate.SourceSongBId).ConfigureAwait(true);
                if (sourceA is null || sourceB is null)
                {
                    StatusMessage = UiText.Format("SongMergeWindow", "SourceNotFound", candidate.SourceSongAId, candidate.SourceSongBId);
                    return;
                }

                songs.Add(BuildMergedSong(candidate, sourceA, sourceB, _mergeService.Merge(sourceA, sourceB)));
            }

            var report = await _adminDatabase.SaveSongsAsync(DatabasePath, backupRoot, songs).ConfigureAwait(true);
            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail) ? UiText.Get("SongMergeWindow", "MergeFailed") : UiText.Format("SongMergeWindow", "MergeFailedDetail", detail);
                return;
            }

            var created = songs.Count;
            CreatedCount = created;
            ValidationMessage = "";
            StatusMessage = UiText.Format("SongMergeWindow", "Created", created);
            Merged?.Invoke(this, new SongMergedEventArgs(created, SelectedTargetFolder!.FolderNo));
        }
        catch (Exception ex) when (IsRecoverableMergeException(ex))
        {
            StatusMessage = UiText.Format("SongMergeWindow", "MergeFailedDetail", ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    partial void OnUseSourceAAlternateTitleChanged(bool value)
        => Candidates.Clear();

    partial void OnUseSourceBAlternateTitleChanged(bool value)
        => Candidates.Clear();

    partial void OnAppendSourceBTitleToMergedTitleChanged(bool value)
        => Candidates.Clear();

    partial void OnAreAllCandidatesSelectedChanged(bool value)
    {
        foreach (var candidate in Candidates)
        {
            candidate.IsSelected = value;
        }
    }

    partial void OnIsBusyChanged(bool value)
        => NotifyCommands();

    private SongWriteModel BuildMergedSong(
        SongMergeCandidate candidate,
        SongDetail sourceA,
        SongDetail sourceB,
        SongMergeResult merged)
        => new(
            SongId: null,
            candidate.MergeTitle.Trim(),
            sourceA.Title.Trim(),
            SelectedTargetFolder!.FolderNo,
            SongNumber: 0, // A merged song has no number, as in WinForms.
            merged.Lyrics,
            // FrmSmartMerge stores the verses of song A in the order of its lyrics, not A's stored sequence.
            Sequence: SongMergeService.LyricOrderSequence(sourceA.Lyrics),
            Writer: FirstNonEmpty(sourceA.Writer, sourceB.Writer),
            Copyright: FirstNonEmpty(sourceA.Copyright, sourceB.Copyright),
            Capo: sourceA.Capo != -1 ? sourceA.Capo : sourceB.Capo,
            Timing: FirstNonEmpty(sourceA.Timing, sourceB.Timing),
            Key: FirstNonEmpty(sourceA.Key, sourceB.Key),
            Notations: merged.Notations,
            Category: "",
            LicenceAdmin1: FirstNonEmpty(sourceA.LicenceAdmin1, sourceA.LicenceAdmin2),
            LicenceAdmin2: FirstNonEmpty(sourceB.LicenceAdmin1, sourceB.LicenceAdmin2),
            BookReference: JoinReferences(sourceA.BookReference, sourceB.BookReference),
            UserReference: JoinReferences(sourceA.UserReference, sourceB.UserReference),
            Settings: "",
            FormatData: "",
            SequenceIsStored: true);

    private bool ValidateFolders(bool requireCandidates)
    {
        if (string.IsNullOrWhiteSpace(DatabasePath))
        {
            ValidationMessage = UiText.Get("SongMergeWindow", "DatabasePathMissing");
            return false;
        }

        if (!File.Exists(DatabasePath))
        {
            ValidationMessage = UiText.Format("SongMergeWindow", "DatabaseNotFound", DatabasePath);
            return false;
        }

        if (SelectedSourceFolderA is null)
        {
            ValidationMessage = UiText.Get("SongMergeWindow", "SelectSourceA");
            return false;
        }

        if (SelectedSourceFolderB is null)
        {
            ValidationMessage = UiText.Get("SongMergeWindow", "SelectSourceB");
            return false;
        }

        if (SelectedTargetFolder is null)
        {
            ValidationMessage = UiText.Get("SongMergeWindow", "SelectTargetFolder");
            return false;
        }

        if (requireCandidates && Candidates.Count == 0)
        {
            ValidationMessage = UiText.Get("SongMergeWindow", "FindFirst");
            return false;
        }

        ValidationMessage = "";
        return true;
    }

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

    private string BuildMergeTitle(string sourceATitle, string sourceBTitle)
    {
        var title = sourceATitle.Trim();
        if (!AppendSourceBTitleToMergedTitle)
        {
            return title;
        }

        var initial = GetInitialTitle(sourceBTitle);
        return string.IsNullOrWhiteSpace(initial) ? title : $"{title} ({initial})";
    }

    private static string MatchKey(SongSummary song, bool useAlternateTitle)
        => (useAlternateTitle ? song.AlternateTitle : song.Title).Trim();

    private static string GetInitialTitle(string title)
    {
        var trimmed = title.Trim();
        if (!trimmed.StartsWith('('))
        {
            var index = trimmed.IndexOf('(', StringComparison.Ordinal);
            if (index >= 0)
            {
                trimmed = trimmed[..index];
            }
        }

        return trimmed.Trim();
    }

    private static string FirstNonEmpty(string first, string second)
        => !string.IsNullOrWhiteSpace(first) ? first.Trim() : second.Trim();

    private static string JoinReferences(string first, string second)
    {
        first = first.Trim();
        second = second.Trim();
        if (string.IsNullOrWhiteSpace(first))
        {
            return second;
        }

        if (string.IsNullOrWhiteSpace(second))
        {
            return first;
        }

        return $"{first},{second}";
    }

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private void NotifyCommands()
    {
        RefreshMatchesCommand.NotifyCanExecuteChanged();
        MergeCommand.NotifyCanExecuteChanged();
    }

    private static bool IsRecoverableMergeException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or System.Data.Common.DbException // a locked or unreadable database
            or NotSupportedException;
}
