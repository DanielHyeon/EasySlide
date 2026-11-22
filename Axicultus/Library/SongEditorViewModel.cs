using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Library;

public sealed record SongEditorSavedEventArgs(int? SongId, int FolderNo);

/// <summary>
/// 미리보기 가사 한 줄 — 본문(Text)과 그 줄의 코드/노테이션(Notation, '»' 뒤)을 나눠 담는다.
/// Axicultus 가사에서 '»'는 코드/노테이션 구분 마커라, 미리보기에서 코드를 본문과 다른 스타일로 보여 준다(리치 per-line 1차).
/// </summary>
public sealed record SongPreviewLine(string Text, string Notation);

public sealed partial class SongEditorViewModel : ObservableObject, IDisposable
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private readonly IAdminSongDetailRepository? _songDetails;
    private readonly ILiveSessionService? _liveSession;
    private readonly ILiveSafetyPrompt? _liveSafetyPrompt;
    // One per pane: each remembers the chord records its pane was filled from.
    private readonly SongEditorChords _chords1 = new();
    private readonly SongEditorChords _chords2 = new();
    // The panes as the song was opened: text the operator has not changed is not held to the rules that are
    // stricter than FrmEditItem's.
    private string _openedRegion1 = "";
    private string _openedRegion2 = "";
    // SEQUENCE as stored and as the sequence box showed it when the song was opened.
    private string _storedSequence = "";
    private string _shownSequence = "";
    // The song as it was read; null for a new song.
    private SongDetail? _openedDetail;
    private bool _loading;
    private bool _syncingLyrics;
    // Set while Lyrics/Notations may not be what the panes show: a pane edit that could not be read.
    private bool _storedLyricsStale;
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

    // Lyrics and Notations are the stored LYRICS and MSC, as FrmEditItem would save the two panes. The operator
    // edits the panes (region 1 and region 2, chord lines above the lyric lines); each side follows the other.
    [ObservableProperty] private string _lyrics = "";
    [ObservableProperty] private string _lyricsRegion1 = "";
    [ObservableProperty] private string _lyricsRegion2 = "";
    [ObservableProperty] private SongFolderSummary? _selectedFolder;

    /// <summary>Why this song cannot be saved (its stored fields could not be read); empty when it can.</summary>
    [ObservableProperty] private string _saveBlockedReason = "";
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

    // ── 영역별 스타일 인스펙터(Region 1=주 언어 / Region 2=보조 언어) ──────────────────────────────
    // FormatData 를 직접 편집하지 않고 이 속성들로 영역별 글자색·정렬·굵게·기울임을 다룬다.
    // 로드 시 FormatData→속성 디코드(SyncInspectorFromFormatData), 변경 시 속성→FormatData 인코드(RebuildFormatDataFromInspector).
    // 인코드는 인스펙터가 안 건드리는 필드(폰트·크기·배경·미디어)를 보존한다(_preservedFormat).
    private SongFormatData _preservedFormat = new();

    // 색·정렬·굵게·기울임 모두 출력 렌더가 영역별로 실제 적용한다(증분 47에서 굵게/기울임 영역별 렌더 배선 완료).
    [ObservableProperty] private string _region1ColorHex = "";
    [ObservableProperty] private string _region2ColorHex = "";
    [ObservableProperty] private int _region1Alignment;   // 0=상속, 1=왼쪽, 2=가운데, 3=오른쪽
    [ObservableProperty] private int _region2Alignment;
    [ObservableProperty] private bool _region1Bold;
    [ObservableProperty] private bool _region1Italic;
    [ObservableProperty] private bool _region1Underline;
    [ObservableProperty] private bool _region2Bold;
    [ObservableProperty] private bool _region2Italic;
    [ObservableProperty] private bool _region2Underline;

    // 코드 조옮김 반음(미리보기 전용). 변경 시 미리보기만 다시 그린다(곡 dirty 안 됨).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransposeLabel))]
    private int _transposeSemitones;

    /// <summary>중복 정의된 절 라벨 경고(비면 문제 없음). Sequence 모델은 절을 1회 정의해야 하므로 같은 라벨 2회는 작성 오류.</summary>
    [ObservableProperty] private string _sectionWarning = "";
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
        // 코드 조옮김(미리보기 전용 — 저장 가사는 불변). ±반음, 원조 복귀.
        TransposeUpCommand = new RelayCommand(() => SetTranspose(TransposeSemitones + 1));
        TransposeDownCommand = new RelayCommand(() => SetTranspose(TransposeSemitones - 1));
        ResetTransposeCommand = new RelayCommand(() => SetTranspose(0));
        // To Capo 0(레거시) — 카포로 올린 만큼(+Capo 반음) 올려 "카포 없이 치는 실제 코드"를 보여 준다.
        ToCapoZeroCommand = new RelayCommand(() => SetTranspose(Capo), () => Capo > 0);
        TransposeSongUpCommand = new RelayCommand(() => TransposeSong(1));
        TransposeSongDownCommand = new RelayCommand(() => TransposeSong(-1));
        RefreshPreview();
    }

    public event EventHandler<SongEditorSavedEventArgs>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

    /// <summary>FrmEditItem "Transpose chord up/down": moves the chords of both panes, the key and the capo of the song itself.</summary>
    public IRelayCommand TransposeSongUpCommand { get; }

    public IRelayCommand TransposeSongDownCommand { get; }

    /// <summary>The folders the song can be saved in (FrmEditItem.BuildFolderList): folders in use, never folder 0.</summary>
    public ObservableCollection<SongFolderSummary> Folders { get; } = [];

    /// <summary>FrmEditItem's capo list: no capo (stored as -1), then Capo 0 to Capo 11.</summary>
    public IReadOnlyList<KeyValuePair<int, string>> CapoOptions { get; } =
        [new(-1, "없음"), .. Enumerable.Range(0, 12).Select(fret => new KeyValuePair<int, string>(fret, $"Capo {fret}"))];

    /// <summary>The chord names of FrmEditItem's chords menu: its top level (empty group name), then its sub menus.</summary>
    public IReadOnlyList<(string Group, IReadOnlyList<string> Chords)> ChordMenu => SongEditorChords.Menu;

    /// <summary>
    /// Width of a text in the font of the lyric panes. A chord is stored with the lyric character it stands
    /// over, so the window gives the measure of its own font; without one every character counts the same.
    /// </summary>
    public Func<string, double> MeasureEditorText
    {
        get => _chords1.Measure;
        set => _chords1.Measure = _chords2.Measure = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>영역별 정렬 콤보 선택지 — 0=상속(곡 설정 추종)·1=왼쪽·2=가운데·3=오른쪽. Value 가 레거시 정렬 코드.</summary>
    public IReadOnlyList<KeyValuePair<int, string>> AlignmentOptions { get; } =
    [
        new(0, "상속"),
        new(1, "왼쪽"),
        new(2, "가운데"),
        new(3, "오른쪽"),
    ];

    /// <summary>코드 조옮김 — 미리보기 코드를 ±반음 이동(레거시 Transpose Up/Down Semi-Tone). 저장 가사는 바뀌지 않는다.</summary>
    public IRelayCommand TransposeUpCommand { get; }

    public IRelayCommand TransposeDownCommand { get; }

    public IRelayCommand ResetTransposeCommand { get; }

    /// <summary>To Capo 0 — 카포 값만큼 올려 카포 없이 치는 실제 코드를 미리보기로 보여 준다(Capo>0 일 때만).</summary>
    public IRelayCommand ToCapoZeroCommand { get; }

    /// <summary>현재 조옮김 표시(예: "+2", "-1", "원조").</summary>
    public string TransposeLabel
        => TransposeSemitones == 0 ? "원조" : (TransposeSemitones > 0 ? $"+{TransposeSemitones}" : TransposeSemitones.ToString());

    // 조옮김 값을 -11~+11 반음으로 클램프해 설정(한 옥타브 안). 미리보기만 갱신(곡은 dirty 안 됨).
    private void SetTranspose(int semitones) => TransposeSemitones = Math.Clamp(semitones, -11, 11);

    /// <summary>미리보기 가사를 줄 단위로(코드/노테이션 분리) 담는다 — 리치 per-line 미리보기 렌더용.</summary>
    public ObservableCollection<SongPreviewLine> PreviewLyricLines { get; } = [];

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
            _openedDetail = null;
            SongId = song?.SongId;
            FolderNo = folder.FolderNo;
            FolderName = folder.Name;
            SongNumber = song?.SongNumber ?? 0; // 0 is "no number", as in WinForms.
            Title = song?.Title ?? "";
            AlternateTitle = song?.AlternateTitle ?? "";
            Category = song?.Category ?? "";
            Key = song?.Key ?? "";
            SetStoredLyrics(song?.Lyrics ?? "", "");
            Folders.Clear();
            Folders.Add(folder);
            SelectedFolder = folder;
            // The list row lacks the sequence, MSC, settings and format of the song; saving before the detail is
            // read would write them back empty.
            SaveBlockedReason = song is null ? "" : "곡 상세를 불러오지 않아 저장할 수 없습니다.";
            Sequence = "";
            _storedSequence = _shownSequence = "";
            Writer = "";
            Copyright = "";
            Capo = Math.Max(song?.Capo ?? -1, -1); // -1 is "no capo", as WinForms stores it.
            Timing = "";
            LicenceAdmin1 = "";
            LicenceAdmin2 = "";
            BookReference = "";
            UserReference = "";
            SongSettingsData = "";
            FormatData = "";
            SyncInspectorFromFormatData(); // 빈 FormatData → 모두 "상속"으로 초기화.
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
        await LoadFoldersAsync().ConfigureAwait(true);
        if (song is null)
        {
            return;
        }

        if (_songDetails is null)
        {
            SaveBlockedReason = "곡 상세를 읽을 수 없어 저장할 수 없습니다.";
            return;
        }

        try
        {
            var detail = await _songDetails.GetSongDetailAsync(DatabasePath, song.SongId).ConfigureAwait(true);
            if (detail is null)
            {
                SaveBlockedReason = $"곡을 찾을 수 없어 저장할 수 없습니다(ID {song.SongId}).";
                return;
            }

            _loading = true;
            try
            {
                ApplyDetail(detail);
                SaveBlockedReason = "";
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
            SaveBlockedReason = $"곡 상세를 불러오지 못해 저장할 수 없습니다: {ex.Message}";
        }
    }

    // The folder list is a convenience: when it cannot be read the song keeps the folder it was opened with.
    private async Task LoadFoldersAsync()
    {
        IReadOnlyList<SongFolderSummary> all;
        try
        {
            all = await _adminDatabase.GetSongFoldersAsync(DatabasePath).ConfigureAwait(true);
        }
        catch (Exception ex) when (IsRecoverableSaveException(ex))
        {
            return;
        }

        // FrmEditItem lists folders 1 to 40 that are in use, or folder 1 when none is.
        var usable = all.Where(folder => folder.FolderNo is >= 1 and <= 40 && folder.IsEnabled).ToList();
        if (usable.Count == 0 && all.FirstOrDefault(folder => folder.FolderNo == 1) is { } firstFolder)
        {
            usable.Add(firstFolder);
        }

        _loading = true;
        try
        {
            Folders.Clear();
            foreach (var folder in usable)
            {
                Folders.Add(folder);
            }

            SelectFolder(FolderNo);
        }
        finally
        {
            _loading = false;
        }
    }

    // The song's own folder stays selectable even when it is not in use, so that saving never moves the song by itself.
    private void SelectFolder(int folderNo)
    {
        var folder = Folders.FirstOrDefault(candidate => candidate.FolderNo == folderNo);
        if (folder is null && folderNo > 0)
        {
            folder = new SongFolderSummary(folderNo, folderNo == FolderNo && FolderName.Length > 0 ? FolderName : $"Folder {folderNo}", IsEnabled: true, SongCount: 0);
            Folders.Add(folder);
        }

        SelectedFolder = folder;
    }

    public async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (SaveBlockedReason.Length > 0)
        {
            StatusMessage = SaveBlockedReason;
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
                Stored(Title, _openedDetail?.Title),
                Stored(AlternateTitle, _openedDetail?.AlternateTitle),
                FolderNo,
                SongNumber,
                Lyrics,
                // A sequence box with only a stray space or line break is an empty sequence.
                Sequence: Sequence == _shownSequence ? Sequence : Sequence.Trim(),
                Writer: Stored(Writer, _openedDetail?.Writer),
                Copyright: Stored(Copyright, _openedDetail?.Copyright),
                Capo: Capo,
                Timing: Stored(Timing, _openedDetail?.Timing),
                Key: Stored(Key, _openedDetail?.Key),
                Notations: Notations,
                Category: Stored(Category, _openedDetail?.Category),
                LicenceAdmin1: Stored(LicenceAdmin1, _openedDetail?.LicenceAdmin1),
                LicenceAdmin2: Stored(LicenceAdmin2, _openedDetail?.LicenceAdmin2),
                BookReference: Stored(BookReference, _openedDetail?.BookReference),
                UserReference: Stored(UserReference, _openedDetail?.UserReference),
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

            // The fields show what was stored: the caller reads them for the song list.
            _loading = true;
            try
            {
                (Title, AlternateTitle, Writer, Copyright, Timing, Key) = (song.Title, song.AlternateTitle, song.Writer, song.Copyright, song.Timing, song.Key);
                (Category, LicenceAdmin1, LicenceAdmin2, BookReference, UserReference) = (song.Category, song.LicenceAdmin1, song.LicenceAdmin2, song.BookReference, song.UserReference);
            }
            finally
            {
                _loading = false;
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

    // A text field the operator edited is stored trimmed; one left alone is stored as it was read (FrmEditItem
    // does not trim), so a save of something else does not rewrite it.
    private static string Stored(string value, string? opened) => value == opened ? value : value.Trim();

    partial void OnTitleChanged(string value) => MarkChanged();

    partial void OnAlternateTitleChanged(string value) => MarkChanged();

    partial void OnCategoryChanged(string value) => MarkChanged();

    partial void OnKeyChanged(string value) => MarkChanged();

    // New stored text (not the echo of a pane edit) is shown in the panes again.
    partial void OnLyricsChanged(string value)
    {
        if (!_syncingLyrics)
        {
            ShowStoredLyrics(value, Notations);
        }

        MarkChanged();
    }

    partial void OnLyricsRegion1Changed(string value) => OnPaneChanged();

    partial void OnLyricsRegion2Changed(string value) => OnPaneChanged();

    partial void OnSelectedFolderChanged(SongFolderSummary? value)
    {
        // The list control clears its selection while the folder list is rebuilt.
        if (value is null)
        {
            return;
        }

        FolderNo = value.FolderNo;
        FolderName = value.Name;
        MarkChanged();
    }

    partial void OnSaveBlockedReasonChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnSongNumberChanged(int value) => MarkChanged();

    partial void OnSequenceChanged(string value) => MarkChanged();

    partial void OnWriterChanged(string value) => MarkChanged();

    partial void OnCopyrightChanged(string value) => MarkChanged();

    partial void OnCapoChanged(int value)
    {
        MarkChanged();
        ToCapoZeroCommand.NotifyCanExecuteChanged(); // Capo>0 여부가 바뀌면 To Capo 0 버튼 활성/비활성.
    }

    partial void OnTimingChanged(string value) => MarkChanged();

    partial void OnNotationsChanged(string value)
    {
        if (!_syncingLyrics)
        {
            ShowStoredLyrics(Lyrics, value);
        }

        MarkChanged();
    }

    private void SetStoredLyrics(string lyrics, string notations)
    {
        _syncingLyrics = true;
        try
        {
            Notations = notations;
            Lyrics = lyrics;
        }
        finally
        {
            _syncingLyrics = false;
        }

        ShowStoredLyrics(lyrics, notations);
        (_openedRegion1, _openedRegion2) = (LyricsRegion1, LyricsRegion2);
    }

    // Stored LYRICS/MSC into the two panes, as FrmEditItem.LoadSong fills its two lyric boxes.
    private void ShowStoredLyrics(string lyrics, string notations)
    {
        var regions = SongEditorLyrics.Split(lyrics, notations);
        _syncingLyrics = true;
        try
        {
            LyricsRegion1 = _chords1.Combine(regions.Text1, regions.Notations1);
            LyricsRegion2 = _chords2.Combine(regions.Text2, regions.Notations2);
        }
        finally
        {
            _syncingLyrics = false;
        }

        UpdateStoredLyrics();
    }

    private void OnPaneChanged()
    {
        if (_syncingLyrics)
        {
            return;
        }

        UpdateStoredLyrics();
        MarkChanged();
    }

    // The panes as FrmEditItem saves them (ValidateAllDetails): chord lines become MSC records of the merged
    // lyrics, so the chords stay with their lines whatever was inserted or removed above them.
    private void UpdateStoredLyrics()
    {
        // An exception here reaches the binding of the pane, which swallows it; Validate then refuses the save
        // instead of writing the text from before the edit.
        _storedLyricsStale = true;
        var first = _chords1.Read(LyricsRegion1);
        var second = _chords2.Read(LyricsRegion2);
        var stored = SongEditorLyrics.Merge(first.Lyrics, first.Notations, second.Lyrics, second.Notations);
        _syncingLyrics = true;
        try
        {
            Notations = stored.Notations;
            Lyrics = stored.Lyrics;
            _storedLyricsStale = false;
        }
        finally
        {
            _syncingLyrics = false;
        }
    }

    // FrmEditItem.BtnChordsClick: the key and the capo move with the chords, and the key decides sharps or flats.
    private void TransposeSong(int steps)
    {
        string key = Key;
        int spelling = SongEditorChords.TransposeKey(ref key, steps);
        Key = key;
        if (Capo is >= 0 and <= 11)
        {
            Capo = SongEditorChords.Shift(Capo, -steps);
        }

        LyricsRegion1 = TransposePane(_chords1, LyricsRegion1, steps, spelling);
        LyricsRegion2 = TransposePane(_chords2, LyricsRegion2, steps, spelling);
    }

    private static string TransposePane(SongEditorChords chords, string text, int steps, int spelling)
    {
        var (lyrics, notations) = chords.Read(text);
        if (notations.Length == 0)
        {
            return text;
        }

        string display = chords.Combine(lyrics, SongEditorChords.Transpose(notations, steps, spelling));
        return lyrics.EndsWith('\n') ? display + "\n" : display;
    }

    /// <summary>Puts a chord over the caret of a lyric pane (1 or 2) and returns where the caret is afterwards.</summary>
    public int InsertChord(int region, int caret, string chord)
    {
        string updated = (region == 2 ? _chords2 : _chords1).InsertChord(region == 2 ? LyricsRegion2 : LyricsRegion1, ref caret, chord);
        if (region == 2)
        {
            LyricsRegion2 = updated;
        }
        else
        {
            LyricsRegion1 = updated;
        }

        return caret;
    }

    // 조옮김은 곡 내용을 바꾸지 않으므로 dirty(MarkChanged) 가 아니라 미리보기만 다시 그린다.
    partial void OnTransposeSemitonesChanged(int value) => RefreshPreview();

    partial void OnLicenceAdmin1Changed(string value) => MarkChanged();

    partial void OnLicenceAdmin2Changed(string value) => MarkChanged();

    partial void OnBookReferenceChanged(string value) => MarkChanged();

    partial void OnUserReferenceChanged(string value) => MarkChanged();

    partial void OnSongSettingsDataChanged(string value) => MarkChanged();

    partial void OnFormatDataChanged(string value) => MarkChanged();

    // 영역별 스타일 인스펙터 변경 → FormatData 재인코드(나머지 필드 보존). 로드 중에는 RebuildFormatDataFromInspector 가 무시.
    partial void OnRegion1ColorHexChanged(string value) => RebuildFormatDataFromInspector();

    partial void OnRegion2ColorHexChanged(string value) => RebuildFormatDataFromInspector();

    partial void OnRegion1AlignmentChanged(int value) => RebuildFormatDataFromInspector();

    partial void OnRegion2AlignmentChanged(int value) => RebuildFormatDataFromInspector();

    partial void OnRegion1BoldChanged(bool value) => RebuildFormatDataFromInspector();

    partial void OnRegion1ItalicChanged(bool value) => RebuildFormatDataFromInspector();

    partial void OnRegion1UnderlineChanged(bool value) => RebuildFormatDataFromInspector();

    partial void OnRegion2BoldChanged(bool value) => RebuildFormatDataFromInspector();

    partial void OnRegion2ItalicChanged(bool value) => RebuildFormatDataFromInspector();

    partial void OnRegion2UnderlineChanged(bool value) => RebuildFormatDataFromInspector();

    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    // 로드 시 FormatData 를 인스펙터 속성으로 디코드한다. _loading 중에 호출되므로 속성 변경이 곡을 dirty 로 만들지 않는다.
    // 인스펙터가 안 건드리는 나머지 필드는 _preservedFormat 에 보존해 두었다가 재인코드 때 합친다.
    private void SyncInspectorFromFormatData()
    {
        var format = SongFormatData.Parse(FormatData) ?? new SongFormatData();
        _preservedFormat = format;
        Region1ColorHex = SongFormatData.ArgbToHex(format.TextColorArgb1) ?? "";
        Region2ColorHex = SongFormatData.ArgbToHex(format.TextColorArgb2) ?? "";
        Region1Alignment = format.Alignment1 ?? 0;
        Region2Alignment = format.Alignment2 ?? 0;
        Region1Bold = format.Bold1;
        Region1Italic = format.Italic1;
        Region1Underline = format.Underline1;
        Region2Bold = format.Bold2;
        Region2Italic = format.Italic2;
        Region2Underline = format.Underline2;
    }

    // 인스펙터 속성을 FormatData 로 인코드한다(폰트·크기·배경 등 나머지 필드는 _preservedFormat 에서 보존).
    // 로드 중에는 무시(디코드가 속성을 세팅하면서 이 핸들러가 불려도 되돌리지 않게).
    private void RebuildFormatDataFromInspector()
    {
        if (_loading)
        {
            return;
        }

        // 색·정렬·굵게·기울임을 인스펙터로 갱신하고, 폰트·크기·배경 등 나머지는 _preservedFormat 에서 보존한다.
        var updated = _preservedFormat with
        {
            TextColorArgb1 = SongFormatData.HexToArgb(Region1ColorHex),
            TextColorArgb2 = SongFormatData.HexToArgb(Region2ColorHex),
            Alignment1 = Region1Alignment is >= 1 and <= 3 ? Region1Alignment : null,
            Alignment2 = Region2Alignment is >= 1 and <= 3 ? Region2Alignment : null,
            Bold1 = Region1Bold,
            Italic1 = Region1Italic,
            Underline1 = Region1Underline,
            Bold2 = Region2Bold,
            Italic2 = Region2Italic,
            Underline2 = Region2Underline,
        };
        _preservedFormat = updated;
        FormatData = updated.Encode(); // OnFormatDataChanged → MarkChanged(dirty + 미리보기 갱신).
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _liveSession?.SessionChanged -= OnLiveSessionChanged;

        _disposed = true;
    }

    private void ApplyDetail(SongDetail detail)
    {
        _openedDetail = detail;
        SongId = detail.SongId;
        SelectFolder(detail.FolderNo);
        FolderNo = detail.FolderNo;
        SongNumber = detail.SongNumber;
        Title = detail.Title;
        AlternateTitle = detail.AlternateTitle;
        SetStoredLyrics(detail.Lyrics, detail.Notations);
        // WinForms verse codes are control characters; show them as "1,c,2". Saving stores the codes again.
        try { Sequence = Axicultus.Module.SongExchangeSequence.FromWpfDatabase(detail.Sequence); }
        catch (InvalidDataException) { Sequence = detail.Sequence; }
        _storedSequence = detail.Sequence;
        _shownSequence = Sequence;
        Writer = detail.Writer;
        Copyright = detail.Copyright;
        // FrmEditItem stored -2 for a song whose capo list was never touched. It shows any such song without a
        // capo and stores -1 when the song is saved again.
        Capo = Math.Max(detail.Capo, -1);
        Timing = detail.Timing;
        Key = detail.Key;
        Category = detail.Category;
        LicenceAdmin1 = detail.LicenceAdmin1;
        LicenceAdmin2 = detail.LicenceAdmin2;
        BookReference = detail.BookReference;
        UserReference = detail.UserReference;
        SongSettingsData = detail.Settings;
        FormatData = detail.FormatData;
        SyncInspectorFromFormatData(); // 저장된 FormatData → 영역별 스타일 인스펙터 속성으로 디코드.
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

        // FrmEditItem.ValidateTitles, in its order.
        if ((SongEditorLyrics.TitleProblem(Stored(Title, _openedDetail?.Title), "제목")
            ?? SongEditorLyrics.TitleProblem(Stored(AlternateTitle, _openedDetail?.AlternateTitle), "다른 제목")
            ?? SongEditorLyrics.TitleProblem(Stored(Copyright, _openedDetail?.Copyright), "저작권")
            ?? SongEditorLyrics.TitleProblem(Stored(BookReference, _openedDetail?.BookReference), "교단 참조")
            ?? SongEditorLyrics.TitleProblem(Stored(Writer, _openedDetail?.Writer), "작사·작곡")) is { } titleProblem)
        {
            ValidationMessage = titleProblem;
            return false;
        }

        // 0 is "no number" and is saved as it is.
        if (SongNumber < 0)
        {
            ValidationMessage = "곡 번호는 음수가 될 수 없습니다.";
            return false;
        }

        if (LyricsRegion1.Length == 0 && LyricsRegion2.Length == 0)
        {
            ValidationMessage = "저장할 가사가 없습니다.";
            return false;
        }

        if (_storedLyricsStale)
        {
            ValidationMessage = "마지막으로 고친 가사를 저장 형식으로 바꾸지 못했습니다. 그 줄을 다시 고친 뒤 저장하세요.";
            return false;
        }

        // FrmEditItem.ValidateSequence, then ValidateContents for each pane.
        var presentVerses = SongEditorLyrics.PresentVerses(LyricsRegion1, LyricsRegion2);
        if ((SongEditorLyrics.SequenceProblem(Sequence, Sequence == _shownSequence ? _storedSequence : null, presentVerses)
            ?? SongEditorLyrics.ContentProblem(LyricsRegion1, "기본 가사", presentVerses, _openedRegion1)
            ?? SongEditorLyrics.ContentProblem(LyricsRegion2, "보조 가사", presentVerses, _openedRegion2)) is { } lyricsProblem)
        {
            ValidationMessage = lyricsProblem;
            return false;
        }

        // -1 is "no capo".
        if (Capo < -1)
        {
            ValidationMessage = "Capo(카포)는 -1(없음)보다 작을 수 없습니다.";
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
            StatusMessage = "저장 취소: 라이브 편집 안전 확인을 사용할 수 없습니다.";
            return false;
        }

        var snapshot = _liveSession!.Current;
        var title = string.IsNullOrWhiteSpace(Title) ? "이 곡" : Title.Trim();
        var liveTitle = string.IsNullOrWhiteSpace(snapshot.CurrentItemTitle)
            ? "현재 출력"
            : snapshot.CurrentItemTitle;
        var request = new LiveSafetyRequest(
            "라이브 중 곡 저장",
            $"출력이 라이브인 상태에서 \"{title}\" 변경을 저장할까요?",
            $"지금 {snapshot.OutputMonitorName} 화면에 \"{liveTitle}\"을(를) 송출하고 있습니다. 이 항목을 다시 보내기 전까지 출력은 현재 화면을 유지합니다.",
            TimeSpan.FromSeconds(8));
        var confirmed = await _liveSafetyPrompt.ConfirmAsync(request).ConfigureAwait(true);
        if (!confirmed)
        {
            StatusMessage = "라이브 중 저장을 취소했습니다.";
        }

        return confirmed;
    }

    private void RefreshPreview()
    {
        PreviewTitle = string.IsNullOrWhiteSpace(Title) ? "제목 없음" : Title.Trim();
        PreviewLyrics = BuildPreviewLyrics(Lyrics);
        RebuildPreviewLines(Lyrics);
        PreviewMetadata = BuildPreviewMetadata();
        PreviewFormatStatus = BuildPreviewFormatStatus();
        // 곡 자체의 포맷(FormatData)에 글자/배경색이 있으면 그 색(region1)으로 미리보기 — 없으면 전역 설정색.
        // 미리보기엔 색 피커가 없어 사용자 입력과 충돌하지 않는다(레거시 v32 FormatData 디코드 반영).
        var format = SongFormatData.Parse(FormatData);
        PreviewForegroundHex = SongFormatData.ArgbToHex(format?.TextColorArgb1)
            ?? ToArgbHex(_settings.Get(AxicultusSettingKeys.LyricsMonitorTextColorArgb));
        PreviewBackgroundHex = SongFormatData.ArgbToHex(format?.BackgroundColorArgb1)
            ?? ToArgbHex(_settings.Get(AxicultusSettingKeys.LyricsMonitorBackgroundColorArgb));

        // A heading that occurs twice is a warning: its blocks are shown together as one verse (FormatDisplayLyrics).
        // From the pane the operator types in: the stored Lyrics is the merged text, where a repeated heading is already folded.
        var duplicates = Shell.LyricsDisplayFormatter.FindDuplicateSectionLabels(LyricsRegion1);
        SectionWarning = duplicates.Count == 0
            ? ""
            : $"중복된 절 표시: {string.Join(", ", duplicates)} — 같은 절 표시의 가사는 한 절로 합쳐져 표시됩니다. 절을 반복하려면 Sequence(예: 1 C 2 C)를 쓰세요.";
    }

    private string BuildPreviewMetadata()
    {
        var parts = new List<string>();
        AddIfPresent(parts, FolderName);
        AddIfPresent(parts, Category);
        // 키 표시 — 조옮김이 걸려 있으면 "키 G → A"처럼 옮긴 키도 함께 보여 준다(코드와 일관).
        if (!string.IsNullOrWhiteSpace(Key))
        {
            var key = Key.Trim();
            parts.Add(TransposeSemitones != 0
                ? $"키 {key} → {Shell.ChordTransposer.Transpose(key, TransposeSemitones)}"
                : $"키 {key}");
        }
        AddIfPresent(parts, Timing);
        if (Capo > 0)
        {
            parts.Add($"카포 {Capo}");
        }

        AddIfPresent(parts, Writer);
        AddIfPresent(parts, Copyright);
        AddIfPresent(parts, BookReference);
        AddIfPresent(parts, UserReference);
        return parts.Count == 0 ? "메타데이터 없음" : string.Join(" · ", parts);
    }

    private string BuildPreviewFormatStatus()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(FormatData))
        {
            parts.Add("포맷 데이터");
        }

        if (!string.IsNullOrWhiteSpace(Notations) || Lyrics.Contains('»'))
        {
            parts.Add("코드 표기");
        }

        if (!string.IsNullOrWhiteSpace(Sequence))
        {
            parts.Add("절 순서");
        }

        return parts.Count == 0 ? "일반 가사" : string.Join(" · ", parts);
    }

    private void OnLiveSessionChanged(object? sender, LiveSessionChangedEventArgs e)
        => IsLiveEditWarningVisible = IsLive(e.Snapshot);

    private bool CanSave()
        => !IsBusy && !string.IsNullOrWhiteSpace(Title) && SaveBlockedReason.Length == 0;

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

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    // 첫 화면(빈 줄로 나뉜 첫 단락)의 표시 줄들 — region 마커 제외, 최대 8줄. 미리보기 문자열·줄 목록의 공통 소스.
    // Each line comes with its index in the stored lyrics, which is what a chord record of MSC refers to.
    private static (string Text, int Index)[] BuildFirstScreenLines(string lyrics)
    {
        // 출력 렌더와 동일한 정규화 규칙을 공유해 미리보기·출력의 마커 해석이 코드로 일치하도록 한다
        // (이전엔 같은 Replace 블록을 따로 두어 주석으로만 일치를 보장했음 → LyricsDisplayFormatter.NormalizeText 로 단일화).
        var lines = LyricsDisplayFormatter.NormalizeText(lyrics ?? string.Empty).Split('\n');
        var screen = new List<(string Text, int Index)>();
        for (var index = 0; index < lines.Length; index++)
        {
            if (lines[index].Length > 0)
            {
                screen.Add((lines[index].TrimEnd(), index));
            }
            else if (screen.Any(line => line.Text.Length > 0))
            {
                break; // The empty line that ends the first screen.
            }
            else
            {
                screen.Clear();
            }
        }

        return screen.Any(line => line.Text.Length > 0)
            ? [.. screen.Where(line => !IsRegionMarker(line.Text)).Take(8)]
            : [];
    }

    private static string BuildPreviewLyrics(string lyrics)
    {
        var lines = BuildFirstScreenLines(lyrics);
        return lines.Length == 0 ? "가사 없음" : string.Join(Environment.NewLine, lines.Select(line => line.Text));
    }

    // 미리보기 줄 목록을 다시 만든다 — 각 줄을 '»' 기준 본문/코드로 나눠 리치 per-line 렌더에 쓴다.
    // 출력 렌더(LyricsDisplayFormatter)도 같은 마커 해석으로 '»' 뒤를 처리한다 — 미리보기는 코드를 흐리게 '표시',
    // 출력은 코드를 '숨김'. 즉 마커 해석은 일치하고 표시 방식만 의도적으로 다르다.
    private void RebuildPreviewLines(string lyrics)
    {
        PreviewLyricLines.Clear();
        var lines = BuildFirstScreenLines(lyrics);
        if (lines.Length == 0)
        {
            PreviewLyricLines.Add(new SongPreviewLine("가사 없음", ""));
            return;
        }

        var chordsByLine = SongEditorChords.NamesByLine(Notations);
        foreach (var (line, index) in lines)
        {
            var split = SplitNotation(line);
            // Chords placed in the editor are in MSC, not in the lyric line.
            if (split.Notation.Length == 0 && chordsByLine.TryGetValue(index, out var chords))
            {
                split = split with { Notation = chords };
            }

            // 조옮김이 걸려 있고 코드가 있으면 미리보기 코드만 ±반음 이동(본문·저장 가사는 불변).
            if (TransposeSemitones != 0 && split.Notation.Length > 0)
            {
                split = split with { Notation = Shell.ChordTransposer.TransposeLine(split.Notation, TransposeSemitones) };
            }

            PreviewLyricLines.Add(split);
        }
    }

    // '»'(코드/노테이션 마커) 기준으로 한 줄을 본문과 코드로 나눈다. 마커가 없으면 전부 본문.
    // 정상 데이터는 줄당 마커가 1개다(레거시 writer 가 줄 끝에 ' »' 하나만 붙임). 마커가 여러 개면
    // 첫 마커로 나누고 나머지는 코드 칸에 그대로 둔다(드문 케이스 — 미리보기라 무해).
    private static SongPreviewLine SplitNotation(string line)
    {
        var markerIndex = line.IndexOf('»');
        if (markerIndex < 0)
        {
            return new SongPreviewLine(line, "");
        }

        var lyricText = line[..markerIndex].TrimEnd();
        var notation = line[(markerIndex + 1)..].Trim();
        return new SongPreviewLine(lyricText, notation);
    }

    private static bool IsRegionMarker(string line)
        => line.Trim().Equals("[region 1]", StringComparison.OrdinalIgnoreCase) ||
           line.Trim().Equals("[region 2]", StringComparison.OrdinalIgnoreCase);

    private static void AddIfPresent(List<string> parts, string? value)
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

    // DbException: SQLite reports a locked, damaged or unreadable database with it.
    private static bool IsRecoverableSaveException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException
            or System.Data.Common.DbException;
}
