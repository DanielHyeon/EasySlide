using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 공지 화면(InfoScreen) 편집 뷰모델(레거시 FrmInfoScreen 대응) — 자유 텍스트 안내를 입력해
/// 회중 출력으로 송출한다. 송출은 주입된 콜백(=MainViewModel.PublishNotice)에 위임하고,
/// 콜백이 false(출력 미개방 등)면 안내 문구를 보여 준다.
/// </summary>
public sealed partial class NoticeScreenViewModel : ObservableObject
{
    private readonly Func<string, NoticeOptions, bool> _publish;
    private readonly Action _clear;
    private readonly IInfoScreenStore _store;
    // 현재 편집 중인 텍스트를 예배 순서(큐)에 항목으로 추가하는 콜백(레거시 InfoScreen 항목). 없으면 "추가" 비활성.
    // 서식(NoticeOptions)도 함께 넘겨 "송출"과 똑같이 글꼴·색·크기·강조가 큐 항목에 실리게 한다.
    private readonly Func<string, NoticeOptions, bool>? _addToWorshipQueue;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddToQueueCommand))]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 공지 글자 크기(pt). 출력에 47=pt FormatData 로 실려 큰 글씨로 송출. 기본 40(보통).
    [ObservableProperty]
    private int _fontSizePt = 40;

    // 공지 가로 정렬(0=기본/1왼쪽/2가운데/3오른쪽). 출력에 31= FormatData 로 실린다. 기본 0=출력 기본 정렬.
    [ObservableProperty]
    private int _alignment;

    // 공지 글자색(ARGB int, 0=기본). 출력에 29= FormatData 로 실린다. 기본 0=출력 기본 글자색.
    [ObservableProperty]
    private int _colorArgb;

    // 공지 배경색(ARGB int, 0=기본). 출력에 26= FormatData 로 실린다(곡 배경과 동일 파이프라인). 기본 0=출력 기본 배경.
    [ObservableProperty]
    private int _backgroundColorArgb;

    // 공지 강조 — 굵게/기울임/밑줄. 출력에 41=비트밭(비트0/1/2) FormatData 로 실린다(곡 강조와 동일 파이프라인). 기본 꺼짐.
    [ObservableProperty]
    private bool _bold;

    [ObservableProperty]
    private bool _italic;

    [ObservableProperty]
    private bool _underline;

    // 공지 글꼴명(빈=전역 기본). 출력에 43= FormatData 로 실린다(곡 글꼴과 동일 파이프라인 → OverrideFontName). 편집 콤보라 목록 밖 글꼴도 직접 입력 가능.
    [ObservableProperty]
    private string _fontName = string.Empty;

    // 새로 저장할 정보 화면 이름(저장 버튼 활성 판별).
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAsCommand))]
    private string _newScreenName = string.Empty;

    // 콤보에서 고른 저장된 정보 화면(불러오기·삭제 버튼 활성 판별).
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private string? _selectedScreen;

    public NoticeScreenViewModel(Func<string, NoticeOptions, bool> publish, Action clear, string? initialText = null, IInfoScreenStore? store = null, Func<string, NoticeOptions, bool>? addToWorshipQueue = null, IReadOnlyList<string>? fontFamilies = null)
    {
        _publish = publish ?? throw new ArgumentNullException(nameof(publish));
        _clear = clear ?? throw new ArgumentNullException(nameof(clear));
        _store = store ?? new InfoScreenStore();
        _addToWorshipQueue = addToWorshipQueue;
        // 글꼴 콤보 목록 — 호출자(MainWindow)가 설치 글꼴 합친 목록을 넘긴다. 없으면(테스트·단독) 추천 글꼴만(공유 목록).
        FontNameOptions = fontFamilies is { Count: > 0 } ? fontFamilies : SystemFontCatalog.DefaultFavorites;
        // 초기 텍스트(성경 "공지 화면으로 복사" 등에서 미리 채워 열 때). 비면 빈 편집기로 시작.
        _text = initialText ?? string.Empty;
        SendCommand = new RelayCommand(Send, () => !string.IsNullOrWhiteSpace(Text));
        ClearCommand = new RelayCommand(Clear);
        // 예배 순서에 텍스트 항목으로 추가(콜백이 주입됐고 텍스트가 있을 때만).
        AddToQueueCommand = new RelayCommand(AddToQueue, () => _addToWorshipQueue is not null && !string.IsNullOrWhiteSpace(Text));
        // 명명 정보 화면 저장/불러오기/삭제(레거시 InfoScreen .esi 목록의 첫 슬라이스).
        SaveAsCommand = new AsyncRelayCommand(SaveAsAsync, () => !string.IsNullOrWhiteSpace(NewScreenName));
        OpenCommand = new AsyncRelayCommand(OpenAsync, () => !string.IsNullOrWhiteSpace(SelectedScreen));
        DeleteCommand = new RelayCommand(Delete, () => !string.IsNullOrWhiteSpace(SelectedScreen));
        RefreshSavedScreens();
    }

    /// <summary>저장된 정보 화면 이름 목록 — 콤보에 바인딩.</summary>
    public ObservableCollection<string> SavedScreens { get; } = new();

    public IAsyncRelayCommand SaveAsCommand { get; }

    public IAsyncRelayCommand OpenCommand { get; }

    public IRelayCommand DeleteCommand { get; }

    /// <summary>
    /// "공지 화면으로 복사"에 쓸 텍스트를 고른다 — 드래그 선택이 있으면 그 선택, 없으면 본문 전체.
    /// 둘 다 공백뿐이면 null(복사할 게 없음). 양끝 공백은 다듬는다. (성경 본문 우클릭 흐름의 순수 결정 로직.)
    /// </summary>
    public static string? ResolveCopyText(string? selected, string? full)
    {
        var source = string.IsNullOrWhiteSpace(selected) ? full : selected;
        return string.IsNullOrWhiteSpace(source) ? null : source.Trim();
    }

    /// <summary>글자 크기 프리셋(pt) — 콤보 바인딩용(보통 40 / 크게 60 / 아주 크게 80).</summary>
    public IReadOnlyList<int> FontSizePresets { get; } = new[] { 40, 60, 80 };

    /// <summary>글꼴 콤보 목록(추천 앞·설치 글꼴 뒤). 생성자에서 주입(없으면 추천 글꼴). 편집 콤보라 목록 밖 글꼴도 직접 입력 가능.</summary>
    public IReadOnlyList<string> FontNameOptions { get; }

    public IRelayCommand SendCommand { get; }

    public IRelayCommand ClearCommand { get; }

    /// <summary>현재 텍스트를 예배 순서(큐)에 항목으로 추가(레거시 InfoScreen 항목). 콜백이 주입됐을 때만 활성.</summary>
    public IRelayCommand AddToQueueCommand { get; }

    /// <summary>정렬 프리셋(라벨·값) — 콤보 바인딩용(기본 0 / 왼쪽 1 / 가운데 2 / 오른쪽 3).</summary>
    public IReadOnlyList<KeyValuePair<string, int>> AlignmentPresets { get; } = new[]
    {
        new KeyValuePair<string, int>("기본", 0),
        new KeyValuePair<string, int>("왼쪽", 1),
        new KeyValuePair<string, int>("가운데", 2),
        new KeyValuePair<string, int>("오른쪽", 3),
    };

    /// <summary>글자색 프리셋(라벨·ARGB int) — 콤보 바인딩용(기본 0=출력 기본 / 흰·노랑·하늘·연두).</summary>
    public IReadOnlyList<KeyValuePair<string, int>> ColorPresets { get; } = new[]
    {
        new KeyValuePair<string, int>("기본", 0),
        new KeyValuePair<string, int>("흰색", unchecked((int)0xFFFFFFFF)),
        new KeyValuePair<string, int>("노랑", unchecked((int)0xFFFFE066)),
        new KeyValuePair<string, int>("하늘", unchecked((int)0xFF66CCFF)),
        new KeyValuePair<string, int>("연두", unchecked((int)0xFF99E066)),
    };

    /// <summary>배경색 프리셋(라벨·ARGB int) — 콤보 바인딩용(기본 0=출력 기본 / 글자가 잘 보이는 어두운 색).</summary>
    public IReadOnlyList<KeyValuePair<string, int>> BackgroundColorPresets { get; } = new[]
    {
        new KeyValuePair<string, int>("기본", 0),
        new KeyValuePair<string, int>("검정", unchecked((int)0xFF000000)),
        new KeyValuePair<string, int>("진회색", unchecked((int)0xFF202020)),
        new KeyValuePair<string, int>("네이비", unchecked((int)0xFF001A33)),
        new KeyValuePair<string, int>("짙은 보라", unchecked((int)0xFF1A0033)),
    };

    // 현재 편집기 상태(크기·정렬·색·배경·강조·글꼴)를 송출 옵션 한 묶음으로 만든다 — "송출"과 "순서에 추가"가 똑같이 쓴다.
    // 인자를 이름으로 넘겨(같은 타입 인자가 이웃해 있어) 순서가 뒤바뀌어도 조용히 새지 않게 한다.
    private NoticeOptions CurrentOptions()
        => new(
            FontSizePt: FontSizePt,
            Alignment: Alignment,
            ColorArgb: ColorArgb,
            BackgroundColorArgb: BackgroundColorArgb,
            Bold: Bold,
            Italic: Italic,
            Underline: Underline,
            FontName: FontName);

    // 입력한 공지 텍스트를 출력으로 송출. 출력 창이 닫혀 있으면 콜백이 false → 안내.
    private void Send()
    {
        var ok = _publish(Text, CurrentOptions());
        StatusText = ok
            ? "공지를 출력에 송출했습니다."
            : "출력 창이 열려 있지 않습니다. 먼저 출력을 여세요.";
    }

    // 송출한 공지를 내린다(출력을 검은 화면으로). 출력으로 전달은 콜백에 위임.
    private void Clear()
    {
        _clear();
        StatusText = "공지를 내렸습니다(검은 화면).";
    }

    // 현재 텍스트를 예배 순서에 항목으로 추가(즉시 송출과 달리 순서에 저장돼 나중에 송출 가능). 콜백에 위임.
    private void AddToQueue()
    {
        if (_addToWorshipQueue is null)
        {
            return;
        }

        // 서식(글꼴·색·크기·강조)도 함께 넘겨 큐 항목에 실리게 한다 — "송출"과 동일한 모양으로 나중에 송출된다.
        var ok = _addToWorshipQueue(Text, CurrentOptions());
        StatusText = ok
            ? "예배 순서에 텍스트 항목으로 추가했습니다."
            : "예배 순서에 추가하지 못했습니다.";
    }

    // 저장된 정보 화면 이름을 다시 읽어 콤보를 갱신한다.
    private void RefreshSavedScreens()
    {
        SavedScreens.Clear();
        foreach (var name in _store.ListNames())
        {
            SavedScreens.Add(name);
        }
    }

    // 현재 본문·글자 크기를 이름으로 저장(레거시 InfoScreen .esi 저장). 무효 이름은 안내 후 무동작.
    private async Task SaveAsAsync()
    {
        var name = NewScreenName?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        try
        {
            await _store.SaveAsync(name, new InfoScreenDto(Text, FontSizePt, Alignment, ColorArgb, BackgroundColorArgb, Bold, Italic, Underline, FontName)).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"정보 화면 저장 실패: {ex.Message}";
            return;
        }

        RefreshSavedScreens();
        SelectedScreen = name;
        StatusText = $"정보 화면 저장됨: {name}";
    }

    // 고른 정보 화면을 편집기로 불러온다(본문·글자 크기). 파일이 없거나 손상이면 안내.
    private async Task OpenAsync()
    {
        var name = SelectedScreen?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        InfoScreenDto? dto;
        try
        {
            dto = await _store.LoadAsync(name).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"정보 화면 불러오기 실패: {ex.Message}";
            return;
        }

        if (dto is null)
        {
            // 파일이 외부에서 사라졌으면 목록 갱신 + 선택 해제(삭제 경로와 일관 — 사라진 이름에 Open/Delete 가 계속 활성되지 않도록).
            StatusText = $"정보 화면을 찾을 수 없습니다: {name}";
            RefreshSavedScreens();
            SelectedScreen = null;
            return;
        }

        Text = dto.Text;
        if (dto.FontSize > 0)
        {
            FontSizePt = dto.FontSize;
        }

        Alignment = dto.Alignment; // 0=기본 포함 그대로 복원.
        ColorArgb = dto.ColorArgb; // 0=기본 포함 그대로 복원.
        BackgroundColorArgb = dto.BackgroundColorArgb; // 0=기본 포함 그대로 복원(옛 파일은 필드 없어 0).
        Bold = dto.Bold; // 강조도 그대로 복원(옛 파일은 필드 없어 false).
        Italic = dto.Italic;
        Underline = dto.Underline;
        FontName = dto.FontName ?? string.Empty; // 글꼴명 복원(옛 파일은 필드 없어 null → 빈=전역 기본).
        StatusText = $"정보 화면 불러옴: {name}";
    }

    // 고른 정보 화면을 삭제한다(파일 제거). 편집기 내용은 그대로 둔다.
    private void Delete()
    {
        var name = SelectedScreen?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        try
        {
            _store.Delete(name);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText = $"정보 화면 삭제 실패: {ex.Message}";
            return;
        }

        RefreshSavedScreens();
        SelectedScreen = null;
        StatusText = $"정보 화면 삭제됨: {name}";
    }
}
