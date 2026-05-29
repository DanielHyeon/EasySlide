using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Data;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Library;

public sealed record SongEditorSavedEventArgs(int? SongId, int FolderNo);

public sealed partial class SongEditorViewModel : ObservableObject, IDisposable
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private readonly IAdminSongDetailRepository? _songDetails;
    private readonly ILiveSessionService? _liveSession;
    private readonly ILiveSafetyPrompt? _liveSafetyPrompt;
    private bool _loading;
    private bool _disposed;

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
    [ObservableProperty] private string _sequence = "";
    [ObservableProperty] private string _writer = "";
    [ObservableProperty] private string _copyright = "";
    [ObservableProperty] private int _capo;
    [ObservableProperty] private string _timing = "";
    [ObservableProperty] private string _notations = "";
    [ObservableProperty] private string _licenceAdmin1 = "";
    [ObservableProperty] private string _licenceAdmin2 = "";
    [ObservableProperty] private string _bookReference = "";
    [ObservableProperty] private string _userReference = "";
    [ObservableProperty] private string _songSettingsData = "";
    [ObservableProperty] private string _formatData = "";
    [ObservableProperty] private string _previewTitle = "";
    [ObservableProperty] private string _previewLyrics = "";
    [ObservableProperty] private string _previewMetadata = "";
    [ObservableProperty] private string _previewFormatStatus = "";
    [ObservableProperty] private string _previewForegroundHex = "#FF000000";
    [ObservableProperty] private string _previewBackgroundHex = "#FFFFFFFF";
    [ObservableProperty] private string _previewFontFamilyName = "Segoe UI";
    [ObservableProperty] private double _previewMainFontSize = 18;
    [ObservableProperty] private double _previewNotationFontSize = 14;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _hasChanges;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isNew = true;
    [ObservableProperty] private bool _isLiveEditWarningVisible;

    public SongEditorViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
        : this(settings, adminDatabase, adminDatabase as IAdminSongDetailRepository, null, null)
    {
    }

    public SongEditorViewModel(
        ISettingsService settings,
        IAdminDatabaseRepository adminDatabase,
        IAdminSongDetailRepository? songDetails,
        ILiveSessionService? liveSession,
        ILiveSafetyPrompt? liveSafetyPrompt)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        _songDetails = songDetails;
        _liveSession = liveSession;
        _liveSafetyPrompt = liveSafetyPrompt;
        if (_liveSession is not null)
        {
            _liveSession.SessionChanged += OnLiveSessionChanged;
            IsLiveEditWarningVisible = IsLive(_liveSession.Current);
        }

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        RefreshPreview();
    }

    public event EventHandler<SongEditorSavedEventArgs>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

    public IReadOnlyList<string> PreviewFontFamilyOptions { get; } =
    [
        "Segoe UI",
        "Malgun Gothic",
        "Arial",
        "Calibri",
        "Noto Sans CJK KR",
    ];

    public IReadOnlyList<double> PreviewFontSizeOptions { get; } = [14, 16, 18, 20, 22, 24, 28, 32];

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
            Sequence = "";
            Writer = "";
            Copyright = "";
            Capo = 0;
            Timing = "";
            Notations = "";
            LicenceAdmin1 = "";
            LicenceAdmin2 = "";
            BookReference = "";
            UserReference = "";
            SongSettingsData = "";
            FormatData = "";
            ValidationMessage = "";
            StatusMessage = "";
            IsNew = song is null;
            IsLiveEditWarningVisible = IsLive(_liveSession?.Current);
            HasChanges = false;
        }
        finally
        {
            _loading = false;
        }

        RefreshPreview();
        SaveCommand.NotifyCanExecuteChanged();
    }

    public async Task LoadAsync(string databasePath, SongFolderSummary folder, SongSummary? song)
    {
        Load(databasePath, folder, song);
        if (song is null || _songDetails is null)
        {
            return;
        }

        try
        {
            var detail = await _songDetails.GetSongDetailAsync(DatabasePath, song.SongId).ConfigureAwait(true);
            if (detail is null)
            {
                return;
            }

            _loading = true;
            try
            {
                ApplyDetail(detail);
                ValidationMessage = "";
                StatusMessage = "";
                HasChanges = false;
            }
            finally
            {
                _loading = false;
            }

            RefreshPreview();
            SaveCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex) when (IsRecoverableSaveException(ex))
        {
            StatusMessage = $"Unable to load song detail: {ex.Message}";
        }
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

        if (!await ConfirmLiveEditIfNeededAsync().ConfigureAwait(true))
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
                Sequence: Sequence,
                Writer: Writer.Trim(),
                Copyright: Copyright.Trim(),
                Capo: Capo,
                Timing: Timing.Trim(),
                Key: Key.Trim(),
                Notations: Notations,
                Category: Category.Trim(),
                LicenceAdmin1: LicenceAdmin1.Trim(),
                LicenceAdmin2: LicenceAdmin2.Trim(),
                BookReference: BookReference.Trim(),
                UserReference: UserReference.Trim(),
                Settings: SongSettingsData,
                FormatData: FormatData);
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

    partial void OnSequenceChanged(string value) => MarkChanged();

    partial void OnWriterChanged(string value) => MarkChanged();

    partial void OnCopyrightChanged(string value) => MarkChanged();

    partial void OnCapoChanged(int value) => MarkChanged();

    partial void OnTimingChanged(string value) => MarkChanged();

    partial void OnNotationsChanged(string value) => MarkChanged();

    partial void OnLicenceAdmin1Changed(string value) => MarkChanged();

    partial void OnLicenceAdmin2Changed(string value) => MarkChanged();

    partial void OnBookReferenceChanged(string value) => MarkChanged();

    partial void OnUserReferenceChanged(string value) => MarkChanged();

    partial void OnSongSettingsDataChanged(string value) => MarkChanged();

    partial void OnFormatDataChanged(string value) => MarkChanged();

    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_liveSession is not null)
        {
            _liveSession.SessionChanged -= OnLiveSessionChanged;
        }

        _disposed = true;
    }

    private void ApplyDetail(SongDetail detail)
    {
        SongId = detail.SongId;
        FolderNo = detail.FolderNo;
        SongNumber = detail.SongNumber > 0 ? detail.SongNumber : SongNumber;
        Title = detail.Title;
        AlternateTitle = detail.AlternateTitle;
        Lyrics = detail.Lyrics;
        Sequence = detail.Sequence;
        Writer = detail.Writer;
        Copyright = detail.Copyright;
        Capo = detail.Capo;
        Timing = detail.Timing;
        Key = detail.Key;
        Notations = detail.Notations;
        Category = detail.Category;
        LicenceAdmin1 = detail.LicenceAdmin1;
        LicenceAdmin2 = detail.LicenceAdmin2;
        BookReference = detail.BookReference;
        UserReference = detail.UserReference;
        SongSettingsData = detail.Settings;
        FormatData = detail.FormatData;
        IsNew = false;
    }

    private void MarkChanged()
    {
        if (_loading)
        {
            return;
        }

        HasChanges = true;
        RefreshPreview();
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

        if (Capo < 0)
        {
            ValidationMessage = "Capo cannot be negative.";
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private async Task<bool> ConfirmLiveEditIfNeededAsync()
    {
        if (!IsLive(_liveSession?.Current))
        {
            IsLiveEditWarningVisible = false;
            return true;
        }

        IsLiveEditWarningVisible = true;
        if (_liveSafetyPrompt is null)
        {
            StatusMessage = "Save canceled: live edit safety prompt is unavailable.";
            return false;
        }

        var snapshot = _liveSession!.Current;
        var title = string.IsNullOrWhiteSpace(Title) ? "this song" : Title.Trim();
        var liveTitle = string.IsNullOrWhiteSpace(snapshot.CurrentItemTitle)
            ? "current output"
            : snapshot.CurrentItemTitle;
        var request = new LiveSafetyRequest(
            "Save song while live",
            $"Save changes to \"{title}\" while output is live?",
            $"Currently showing \"{liveTitle}\" on {snapshot.OutputMonitorName}. The output keeps its current snapshot until you send this item again.",
            TimeSpan.FromSeconds(8));
        var confirmed = await _liveSafetyPrompt.ConfirmAsync(request).ConfigureAwait(true);
        if (!confirmed)
        {
            StatusMessage = "Save canceled while live.";
        }

        return confirmed;
    }

    private void RefreshPreview()
    {
        PreviewTitle = string.IsNullOrWhiteSpace(Title) ? "Untitled song" : Title.Trim();
        PreviewLyrics = BuildPreviewLyrics(Lyrics);
        PreviewMetadata = BuildPreviewMetadata();
        PreviewFormatStatus = BuildPreviewFormatStatus();
        PreviewForegroundHex = ToArgbHex(_settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb));
        PreviewBackgroundHex = ToArgbHex(_settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb));
    }

    private string BuildPreviewMetadata()
    {
        var parts = new List<string>();
        AddIfPresent(parts, FolderName);
        AddIfPresent(parts, Category);
        AddIfPresent(parts, string.IsNullOrWhiteSpace(Key) ? "" : $"Key {Key.Trim()}");
        AddIfPresent(parts, Timing);
        if (Capo > 0)
        {
            parts.Add($"Capo {Capo}");
        }

        AddIfPresent(parts, Writer);
        AddIfPresent(parts, Copyright);
        AddIfPresent(parts, BookReference);
        AddIfPresent(parts, UserReference);
        return parts.Count == 0 ? "No metadata" : string.Join(" · ", parts);
    }

    private string BuildPreviewFormatStatus()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(FormatData))
        {
            parts.Add("format data");
        }

        if (!string.IsNullOrWhiteSpace(Notations) || Lyrics.Contains('»'))
        {
            parts.Add("notation data");
        }

        if (!string.IsNullOrWhiteSpace(Sequence))
        {
            parts.Add("sequence data");
        }

        return parts.Count == 0 ? "plain lyrics" : string.Join(" · ", parts);
    }

    private void OnLiveSessionChanged(object? sender, LiveSessionChangedEventArgs e)
        => IsLiveEditWarningVisible = IsLive(e.Snapshot);

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

    private static string BuildPreviewLyrics(string lyrics)
    {
        var normalized = (lyrics ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var firstScreen = normalized
            .Split(["\n\n"], StringSplitOptions.None)
            .Select(screen => screen.Trim('\n'))
            .FirstOrDefault(screen => !string.IsNullOrWhiteSpace(screen));
        if (string.IsNullOrWhiteSpace(firstScreen))
        {
            return "No lyrics";
        }

        var lines = firstScreen
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Where(line => !IsRegionMarker(line))
            .Take(8)
            .ToArray();
        return lines.Length == 0 ? "No lyrics" : string.Join(Environment.NewLine, lines);
    }

    private static bool IsRegionMarker(string line)
        => line.Trim().Equals("[region 1]", StringComparison.OrdinalIgnoreCase) ||
           line.Trim().Equals("[region 2]", StringComparison.OrdinalIgnoreCase);

    private static void AddIfPresent(ICollection<string> parts, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add(value.Trim());
        }
    }

    private static string ToArgbHex(int argb)
        => $"#{unchecked((uint)argb).ToString("X8", CultureInfo.InvariantCulture)}";

    private static bool IsLive(LiveSessionSnapshot? snapshot)
        => snapshot is { State: not LiveState.Off };

    private static bool IsRecoverableSaveException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
