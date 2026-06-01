using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

/// <summary>
/// 미리보기 가사 한 줄 — 본문(Text)과 그 줄의 코드/노테이션(Notation, '»' 뒤)을 나눠 담는다.
/// EasiSlides 가사에서 '»'는 코드/노테이션 구분 마커라, 미리보기에서 코드를 본문과 다른 스타일로 보여 준다(리치 per-line 1차).
/// </summary>
public sealed record SongPreviewLine(string Text, string Notation);

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

    // ── 영역별 스타일 인스펙터(Region 1=주 언어 / Region 2=보조 언어) ──────────────────────────────
    // FormatData 를 직접 편집하지 않고 이 속성들로 영역별 글자색·정렬·굵게·기울임을 다룬다.
    // 로드 시 FormatData→속성 디코드(SyncInspectorFromFormatData), 변경 시 속성→FormatData 인코드(RebuildFormatDataFromInspector).
    // 인코드는 인스펙터가 안 건드리는 필드(폰트·크기·배경·미디어)를 보존한다(_preservedFormat).
    private SongFormatData _preservedFormat = new();

    // 색·정렬은 출력 렌더가 영역별로 실제 적용한다(OverrideTextColorArgb1/2·정렬). 굵게/기울임은 아직 출력이
    // 영역별로 분리 적용하지 않으므로(전역 설정만) 인스펙터 편집 대상에서 제외했다 — "안 보이는 효과"를 약속하지 않기 위함.
    // 단, 기존 곡의 굵게/기울임 비트(41)는 _preservedFormat 로 보존돼 편집해도 사라지지 않는다(영역별 렌더는 후속 증분).
    [ObservableProperty] private string _region1ColorHex = "";
    [ObservableProperty] private string _region2ColorHex = "";
    [ObservableProperty] private int _region1Alignment;   // 0=상속, 1=왼쪽, 2=가운데, 3=오른쪽
    [ObservableProperty] private int _region2Alignment;

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
        RefreshPreview();
    }

    public event EventHandler<SongEditorSavedEventArgs>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

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
    public ObservableCollection<SongPreviewLine> PreviewLyricLines { get; } = new();

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
            StatusMessage = $"곡 상세를 불러오지 못했습니다: {ex.Message}";
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

    partial void OnCapoChanged(int value)
    {
        MarkChanged();
        ToCapoZeroCommand.NotifyCanExecuteChanged(); // Capo>0 여부가 바뀌면 To Capo 0 버튼 활성/비활성.
    }

    partial void OnTimingChanged(string value) => MarkChanged();

    partial void OnNotationsChanged(string value) => MarkChanged();

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
        // 굵게/기울임은 인스펙터 편집 대상이 아니다(전역만 렌더) — _preservedFormat 가 기존 비트를 보존한다.
    }

    // 인스펙터 속성을 FormatData 로 인코드한다(폰트·크기·배경 등 나머지 필드는 _preservedFormat 에서 보존).
    // 로드 중에는 무시(디코드가 속성을 세팅하면서 이 핸들러가 불려도 되돌리지 않게).
    private void RebuildFormatDataFromInspector()
    {
        if (_loading)
        {
            return;
        }

        // 색·정렬만 인스펙터로 갱신하고, 굵게/기울임·폰트·배경 등 나머지는 _preservedFormat 에서 보존한다.
        var updated = _preservedFormat with
        {
            TextColorArgb1 = SongFormatData.HexToArgb(Region1ColorHex),
            TextColorArgb2 = SongFormatData.HexToArgb(Region2ColorHex),
            Alignment1 = Region1Alignment is >= 1 and <= 3 ? Region1Alignment : null,
            Alignment2 = Region2Alignment is >= 1 and <= 3 ? Region2Alignment : null,
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

        if (SongNumber <= 0)
        {
            SongNumber = SongId ?? 1;
        }

        if (Capo < 0)
        {
            ValidationMessage = "Capo(카포)는 음수가 될 수 없습니다.";
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
            ?? ToArgbHex(_settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb));
        PreviewBackgroundHex = SongFormatData.ArgbToHex(format?.BackgroundColorArgb1)
            ?? ToArgbHex(_settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb));

        // 같은 절 라벨이 두 번 정의되면 경고(절은 1회 정의 + Sequence 로 반복). 비면 문제 없음.
        var duplicates = Shell.LyricsDisplayFormatter.FindDuplicateSectionLabels(Lyrics);
        SectionWarning = duplicates.Count == 0
            ? ""
            : $"중복된 절 표시: {string.Join(", ", duplicates)} — 절은 한 번만 정의하고 Sequence(예: 1 C 2 C)로 반복하세요.";
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

    // 첫 화면(빈 줄로 나뉜 첫 단락)의 표시 줄들 — region 마커 제외, 최대 8줄. 미리보기 문자열·줄 목록의 공통 소스.
    private static string[] BuildFirstScreenLines(string lyrics)
    {
        // 출력 렌더와 동일한 정규화 규칙을 공유해 미리보기·출력의 마커 해석이 코드로 일치하도록 한다
        // (이전엔 같은 Replace 블록을 따로 두어 주석으로만 일치를 보장했음 → LyricsDisplayFormatter.NormalizeText 로 단일화).
        var normalized = LyricsDisplayFormatter.NormalizeText(lyrics ?? string.Empty);
        var firstScreen = normalized
            .Split(["\n\n"], StringSplitOptions.None)
            .Select(screen => screen.Trim('\n'))
            .FirstOrDefault(screen => !string.IsNullOrWhiteSpace(screen));
        if (string.IsNullOrWhiteSpace(firstScreen))
        {
            return [];
        }

        return firstScreen
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Where(line => !IsRegionMarker(line))
            .Take(8)
            .ToArray();
    }

    private static string BuildPreviewLyrics(string lyrics)
    {
        var lines = BuildFirstScreenLines(lyrics);
        return lines.Length == 0 ? "가사 없음" : string.Join(Environment.NewLine, lines);
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

        foreach (var line in lines)
        {
            var split = SplitNotation(line);
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

        var text = line[..markerIndex].TrimEnd();
        var notation = line[(markerIndex + 1)..].Trim();
        return new SongPreviewLine(text, notation);
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
