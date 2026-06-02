using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf;

// 레거시 대체: FrmMain(운영 셸). PPT 썸네일/미디어 탭 렌더는 미완(docs/wpf-migration/gap-analysis.md §4 G-α).
public partial class MainWindow : Window
{
    private readonly ShortcutRegistry _shortcuts;
    private readonly IServiceProvider _services;
    private readonly MainViewModel _viewModel;
    // 창 크기·위치 저장/복원용 설정 서비스(레거시 FrmMain 창 상태 저장 대응).
    private readonly ISettingsService? _settings;
    // 자동 회전 타이머(§7.3-B) — VM 은 로직만 갖고(테스트 용이), 실제 주기 구동은 View 가 맡는다.
    // IsAutoRotating 이 켜지면 시작, 꺼지면 정지. 매 tick 에 VM.AdvanceAutoRotation 을 호출.
    private readonly DispatcherTimer _autoRotateTimer;
    private bool _libraryLoadedOnce;
    private bool _bibleLoadedOnce;
    private bool _searchLoadedOnce;

    public MainWindow(MainViewModel viewModel, ShortcutRegistry shortcuts, IServiceProvider services)
    {
        InitializeComponent();

        _shortcuts = shortcuts;
        _services = services;
        _viewModel = viewModel;
        // 설정 서비스(창 배치 저장/복원). 해석 실패해도 창 동작에 지장 없도록 null 허용(복원/저장만 건너뜀).
        _settings = services?.GetService<ISettingsService>();
        DataContext = viewModel;
        viewModel.BindShortcuts(_shortcuts);
        BindWindowLaunchers();

        _autoRotateTimer = new DispatcherTimer();
        _autoRotateTimer.Tick += (_, _) => _viewModel.AdvanceAutoRotation();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // 명령 팔레트(§7.4)가 열리면 검색창에 포커스를 줘 바로 타이핑할 수 있게 한다.
        CommandPaletteOverlay.IsVisibleChanged += CommandPaletteOverlay_IsVisibleChanged;

        // FrmMain식 멀티페인: 좌측 브라우저가 항상 보이므로 시작 시 곡 목록을 채운다(FrmMain 은 곡 목록을 즉시 표시).
        // 성경은 비용이 커 기존대로 "성경" 탭 첫 선택 시 지연 로드한다.
        Loaded += MainWindow_Loaded;
    }

    // 창 런처(§7.4) — 명령 팔레트(⌘K)가 분리 창을 열 수 있도록 레지스트리에 등록.
    // 창 열기는 IServiceProvider/Window 가 필요한 View 책임이라 VM 의 BindShortcuts 가 아닌 여기서 등록한다.
    // 각 핸들러는 sender/e 를 쓰지 않으므로 빈 RoutedEventArgs 로 호출한다(버튼 클릭과 동일 동작).
    private void BindWindowLaunchers()
    {
        void Bind(string id, RoutedEventHandler handler) => _shortcuts.Bind(id, () => handler(this, new RoutedEventArgs()));

        Bind(MainCommandIds.WindowLibrary, OpenLibrary_Click);
        Bind(MainCommandIds.WindowBible, OpenBible_Click);
        Bind(MainCommandIds.WindowManageBibleVersions, OpenBibleVersionManager_Click);
        Bind(MainCommandIds.WindowSearch, OpenSearchUsage_Click);
        Bind(MainCommandIds.WindowImportExport, OpenImportExport_Click);
        Bind(MainCommandIds.WindowExternalFiles, OpenExternalFiles_Click);
        Bind(MainCommandIds.WindowManageLists, OpenManageWorshipLists_Click);
        Bind(MainCommandIds.WindowSettings, OpenSettings_Click);
        Bind(MainCommandIds.WindowHelp, OpenHelp_Click);
        Bind(MainCommandIds.WindowRegistration, OpenRegistration_Click);
        Bind(MainCommandIds.WindowAbout, OpenAbout_Click);
        Bind(MainCommandIds.AddExternalFile, AddExternalFile_Click);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) => EnsureLibraryLoadedOnce();

    // 좌측 브라우저가 항상 보이므로 곡 목록을 1회 채운다. 시작 시(Loaded)와 "라이브러리" 탭 첫 선택 중
    // 먼저 오는 쪽이 로드하고 _libraryLoadedOnce 로 멱등 보장(WPF 는 Loaded 전에 기본 탭 SelectionChanged 를
    // 낼 수 있어 두 진입점이 같은 가드를 공유 — 이벤트 순서와 무관하게 정확히 1회).
    private void EnsureLibraryLoadedOnce()
    {
        if (_libraryLoadedOnce || !_viewModel.Library.LoadCommand.CanExecute(null))
        {
            return;
        }

        _libraryLoadedOnce = true;
        _viewModel.Library.LoadCommand.Execute(null);
    }

    private void CommandPaletteOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            // 레이아웃·렌더 후 포커스를 줘야 확실히 잡힌다(Input 우선순위로 디스패치).
            Dispatcher.BeginInvoke(
                () =>
                {
                    CommandPaletteSearchBox.Focus();
                    CommandPaletteSearchBox.SelectAll();
                },
                DispatcherPriority.Input);
        }
    }

    // 바깥 반투명 영역을 직접 클릭하면 팔레트를 닫는다(안쪽 패널 클릭은 OriginalSource 가 자식이라 무시).
    private void CommandPaletteOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, CommandPaletteOverlay))
        {
            _viewModel.CommandPalette.Close();
        }
    }

    // 결과 항목 더블클릭 = 실행(키보드 Enter 와 동일 경로).
    private void CommandPaletteResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        => _viewModel.CommandPalette.ExecuteSelectedCommand();

    // 명령 팔레트 검색창 키 처리: ↓/↑ 선택 이동, Enter 실행, Esc 닫기(로직은 VM 에 위임).
    private void CommandPaletteSearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var palette = _viewModel.CommandPalette;
        switch (e.Key)
        {
            case Key.Down:
                palette.SelectNext();
                e.Handled = true;
                break;
            case Key.Up:
                palette.SelectPrevious();
                e.Handled = true;
                break;
            case Key.Enter:
                palette.ExecuteSelectedCommand();
                e.Handled = true;
                break;
            case Key.Escape:
                palette.Close();
                e.Handled = true;
                break;
        }
    }

    // VM 의 자동 회전 상태/간격 변화를 받아 타이머를 시작·정지·재설정한다.
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.IsAutoRotating) or nameof(MainViewModel.AutoRotateIntervalSeconds))
        {
            SyncAutoRotateTimer();
        }
    }

    private void SyncAutoRotateTimer()
    {
        if (_viewModel.IsAutoRotating)
        {
            // 간격은 설정 범위(2~600초)에서 오므로 안전하지만 방어적으로 최소 1초 보장.
            _autoRotateTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, _viewModel.AutoRotateIntervalSeconds));
            _autoRotateTimer.Start();
        }
        else
        {
            _autoRotateTimer.Stop();
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // 명령 팔레트가 열려 검색 중이면 전역 단축키를 막는다 — 검색어에 단축키 조합(예: Ctrl+B)이
        // 섞여 백그라운드에서 위험 명령이 실행되는 사고 방지(code-review MINOR). 팔레트 키는 검색창이 처리.
        if (_viewModel.CommandPalette.IsOpen)
        {
            return;
        }

        // 절 점프 숫자/문자 키(레거시 PreviewBtnVerse 1~9·c·b 등) — 텍스트 입력 중이 아니고 수식 키가 없을 때만.
        // 검색창·공지문구·글꼴명 입력에 "1"·"c" 를 칠 때 절이 점프하는 사고를 막기 위해 포커스가 입력 컨트롤이면 건너뛴다.
        if (TryHandleVerseJumpKey(e))
        {
            e.Handled = true;
            return;
        }

        if (_shortcuts.TryHandle(e.Key, Keyboard.Modifiers))
        {
            e.Handled = true;
        }
    }

    // 절 점프 키 처리: 수식 키 없음 + 텍스트 입력에 포커스 없음 + 매핑된 라벨이 현재 곡에 존재할 때만 점프.
    private bool TryHandleVerseJumpKey(KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.None || IsTextInputFocused())
        {
            return false;
        }

        var label = VerseJumpKeyMap.MapKeyToLabel(e.Key);
        if (label is null || !_viewModel.JumpToLyricsSectionCommand.CanExecute(label))
        {
            return false;
        }

        _viewModel.JumpToLyricsSectionCommand.Execute(label);
        return true;
    }

    // 텍스트 입력 컨트롤(텍스트박스·편집 가능 콤보)에 포커스가 있으면 true — 절 점프 키가 타이핑을 가로채지 않게 한다.
    // 주의: 메인 창엔 현재 PasswordBox·편집 가능 DataGrid 가 없다(검증됨). 나중에 추가하면
    // PasswordBox(TextBoxBase 비상속) 등을 여기 조건에 더해야 숫자/문자 입력이 절 점프에 가로채이지 않는다.
    private static bool IsTextInputFocused()
        => Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase
            or System.Windows.Controls.ComboBox { IsEditable: true };

    // 라이브러리 탭을 처음 선택할 때 한 번 자동 로드(시작 비용 회피 — 시작 시점엔 DB 를 읽지 않음).
    private void LeftBrowserTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TabControl.SelectionChanged 는 내부 Selector(ListBox/ComboBox) 선택에서도 버블링되므로,
        // 탭 컨트롤 자신의 변경만 처리한다.
        if (!ReferenceEquals(e.OriginalSource, sender))
        {
            return;
        }

        if (sender is not TabControl { SelectedItem: TabItem tab }
            || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        // 헤더 문구·다국어 변경에 견고하도록 Tag 로 식별(Header 리터럴 의존 회피). 탭별 1회 자동 로드.
        switch (tab.Tag)
        {
            case "Library":
                EnsureLibraryLoadedOnce(); // 멱등 — 시작 로드(Loaded)와 동일 가드 공유
                break;
            case "Bible" when !_bibleLoadedOnce:
                _bibleLoadedOnce = true;
                _ = viewModel.Bible.LoadAsync(); // 버전·책 로드(작업 폴더 기준). 예외는 VM 내부에서 흡수.
                break;
            case "Search" when !_searchLoadedOnce:
                _searchLoadedOnce = true;
                _ = viewModel.Search.LoadAsync(); // 검색 폴더 목록(기본 전체 선택)·DB 경로 로드. 예외는 VM 내부에서 흡수.
                break;
        }
    }

    // 본문에서 드래그 선택한 구절 범위를 BibleSelection 으로 만들어 예배 순서에 추가(BibleWindow Select_Click 과 동일).
    private void AddBibleVerse_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var selection = viewModel.Bible.BuildSelection(
            BiblePassageBox.SelectionStart,
            BiblePassageBox.SelectionLength);
        if (!string.IsNullOrWhiteSpace(selection.IdString))
        {
            viewModel.AddBibleSelection(selection);
        }
    }

    // 성경 본문 드래그-드롭 시작점(왼쪽 버튼 누른 위치)과 무장 여부 — 이미 선택된 글자 위를 눌렀을 때만 드래그를 시작한다
    // (새 선택 제스처와 충돌하지 않도록). 레거시 인라인 성경의 본문 드래그→예배순서 드롭 대응.
    private Point _bibleDragStart;
    private bool _bibleDragArmed;

    // 왼쪽 버튼 누름: 누른 지점이 "현재 선택 범위 안"이면 드래그 후보로 무장(밖이면 새 선택 제스처이므로 무장 안 함).
    private void BiblePassageBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _bibleDragStart = e.GetPosition(null);
        // snapToText:false 면 글자 위가 아닌 빈 영역(본문 끝 너머 등)에선 -1 → 빈 공간 클릭에서 헛-무장하지 않는다.
        var index = BiblePassageBox.GetCharacterIndexFromPoint(e.GetPosition(BiblePassageBox), snapToText: false);
        var selectionStart = BiblePassageBox.SelectionStart;
        var selectionEnd = selectionStart + BiblePassageBox.SelectionLength;
        _bibleDragArmed = BiblePassageBox.SelectionLength > 0 && index >= selectionStart && index < selectionEnd;
    }

    // 임계 거리 이상 움직이면 선택 구절을 BibleSelection 으로 만들어 드래그를 시작한다 — 예배 순서 목록에 드롭하면 추가된다.
    private void BiblePassageBox_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_bibleDragArmed || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var moved = e.GetPosition(null) - _bibleDragStart;
        if (Math.Abs(moved.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(moved.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var selection = viewModel.Bible.BuildSelection(
            BiblePassageBox.SelectionStart,
            BiblePassageBox.SelectionLength);
        if (string.IsNullOrWhiteSpace(selection.IdString))
        {
            return;
        }

        _bibleDragArmed = false; // 한 제스처에서 한 번만 시작.
        DragDrop.DoDragDrop(BiblePassageBox, new DataObject(typeof(BibleSelection), selection), DragDropEffects.Copy);
    }

    // "Region 2(이중 언어)와 함께 추가" 서브메뉴 클릭 — 선택 구절을 클릭한 보조 버전과 합쳐(이중 언어) 예배 순서에 추가한다.
    // 클릭한 메뉴 항목의 DataContext 가 고른 BibleVersion(Region2VersionOptions 의 한 항목)이다.
    private void AddBibleVerseWithRegion2_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        if ((sender as MenuItem)?.DataContext is not BibleVersion region2)
        {
            return;
        }

        var selection = viewModel.Bible.BuildSelectionWithRegion2(
            BiblePassageBox.SelectionStart,
            BiblePassageBox.SelectionLength,
            region2);
        if (!string.IsNullOrWhiteSpace(selection.IdString))
        {
            viewModel.AddBibleSelection(selection);
        }
    }

    // 입력창에서 Enter 를 누르면 "이동"과 동일하게 처리(타이핑→Enter 한 번에 추가).
    private void BibleReferenceBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            JumpToTypedBibleReference();
        }
    }

    private void JumpBibleReference_Click(object sender, RoutedEventArgs e)
        => JumpToTypedBibleReference();

    // 타이핑한 구절(예: "창 1:1-2:3")로 점프해 본문에서 그 범위를 하이라이트하고 예배 순서에 추가한다.
    // 드래그 선택 흐름(BuildSelection→AddBibleSelection)과 동일 경로를 재사용한다.
    private void JumpToTypedBibleReference()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var selection = viewModel.Bible.JumpToReference();
        if (string.IsNullOrWhiteSpace(selection.IdString))
        {
            return; // 파싱·해석 실패 — VM 이 ValidationMessage 로 안내함.
        }

        // 찾은 본문 범위를 시각적으로 하이라이트(운영자 확인용).
        var start = viewModel.Bible.LastReferenceStart;
        var length = viewModel.Bible.LastReferenceLength;
        if (length > 0 && start >= 0 && start + length <= BiblePassageBox.Text.Length)
        {
            BiblePassageBox.Select(start, length);
        }

        viewModel.AddBibleSelection(selection);
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = _services.GetRequiredService<SettingsWindow>();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void OpenLibrary_Click(object sender, RoutedEventArgs e)
    {
        var libraryWindow = _services.GetRequiredService<LibraryWindow>();
        libraryWindow.Owner = this;
        if (libraryWindow.ShowDialog() == true && DataContext is MainViewModel viewModel)
        {
            // "라이브에 추가"로 닫혔으면 선택 곡을 예배 순서(큐)에 추가(BibleWindow 흐름과 동일).
            viewModel.AddSong(libraryWindow.SelectedSongForLive);
        }
    }

    private void OpenExternalFiles_Click(object sender, RoutedEventArgs e)
    {
        var externalFileWindow = _services.GetRequiredService<ExternalFileOperationWindow>();
        externalFileWindow.Owner = this;
        externalFileWindow.ShowDialog();
    }

    private void OpenImportExport_Click(object sender, RoutedEventArgs e)
    {
        var importExportWindow = _services.GetRequiredService<ImportExportWindow>();
        importExportWindow.Owner = this;
        importExportWindow.ShowDialog();
    }

    private void OpenSearchUsage_Click(object sender, RoutedEventArgs e)
    {
        var searchUsageWindow = _services.GetRequiredService<SearchUsageWindow>();
        searchUsageWindow.Owner = this;
        searchUsageWindow.ShowDialog();
    }

    private void AddExternalFile_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "예배 순서에 추가할 파일 선택",
            Filter = "PowerPoint (*.ppt;*.pptx)|*.ppt;*.pptx"
                + "|미디어 (*.mp4;*.avi;*.wmv;*.mov;*.mkv;*.mp3;*.wav;*.wma)|*.mp4;*.avi;*.wmv;*.mov;*.mkv;*.mp3;*.wav;*.wma"
                + "|모든 파일 (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            var ext = System.IO.Path.GetExtension(dialog.FileName).ToLowerInvariant();
            if (ext is ".ppt" or ".pptx")
            {
                viewModel.AddPowerPoint(dialog.FileName);
            }
            else
            {
                viewModel.AddMedia(dialog.FileName);
            }
        }
    }

    private void SelectOutputBackgroundImage_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "출력 배경으로 사용할 이미지 선택",
            Filter = "이미지 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
                + "|모든 파일 (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            viewModel.SetOutputBackgroundImage(dialog.FileName);
        }
    }

    // 세션 메모 — 현재 예배 세션(예배 순서)의 운영자 메모를 편집·저장(FrmMain Session Notes 포팅).
    private void OpenSessionNotes_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        // 현재 예배 세션 이름이 없으면(아직 저장/불러오기 전) "일반" 키로 떨어진다(VM 이 처리).
        var notesViewModel = new Easislides.Wpf.Shell.WorshipSessionNotesViewModel(
            new Easislides.Wpf.Shell.WorshipSessionNotesService(),
            viewModel.CurrentWorshipListName);

        var window = new Easislides.Wpf.Shell.WorshipSessionNotesWindow(notesViewModel) { Owner = this };
        window.ShowDialog();
    }

    // PowerPoint 폴더 브라우저 — 폴더의 PPT 덱을 보고 예배 순서에 추가(FrmMain PowerP 탭 포팅).
    private void OpenPowerPointLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var workingFolder = _services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder;
        var initialFolder = !string.IsNullOrWhiteSpace(workingFolder) && Directory.Exists(workingFolder)
            ? workingFolder
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var pptViewModel = new Easislides.Wpf.Library.PowerPointLibraryViewModel(
            new Easislides.Wpf.Library.PowerPointLibraryService(),
            path => viewModel.AddPowerPoint(path), // AddPowerPoint 는 LiveQueueItem 을 반환하므로 람다로 감싼다
            initialFolder);

        var window = new Easislides.Wpf.Library.PowerPointLibraryWindow(pptViewModel) { Owner = this };
        window.ShowDialog();
    }

    // 미디어 폴더 브라우저 — 폴더의 동영상·오디오를 보고 예배 순서에 추가(FrmMain Media 탭 포팅).
    private void OpenMediaLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        // 미디어 폴더는 설정의 MediaDirectory 를 우선 쓰고, 없으면 작업 폴더, 그래도 없으면 내 문서.
        var settings = _services.GetRequiredService<ISettingsService>();
        var mediaDir = settings.Get(EasiSettingKeys.MediaDirectory);
        var workingFolder = settings.Current.General.WorkingFolder;
        var initialFolder =
            !string.IsNullOrWhiteSpace(mediaDir) && Directory.Exists(mediaDir) ? mediaDir
            : !string.IsNullOrWhiteSpace(workingFolder) && Directory.Exists(workingFolder) ? workingFolder
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var mediaViewModel = new Easislides.Wpf.Library.MediaLibraryViewModel(
            new Easislides.Wpf.Library.MediaLibraryService(),
            path => viewModel.AddMedia(path), // AddMedia 는 LiveQueueItem 을 반환하므로 람다로 감싼다
            initialFolder);

        var window = new Easislides.Wpf.Library.MediaLibraryWindow(mediaViewModel) { Owner = this };
        window.ShowDialog();
    }

    // 공지 화면(InfoScreen) — 자유 텍스트 안내를 입력해 회중 출력으로 송출(FrmInfoScreen 포팅).
    private void OpenNoticeScreen_Click(object sender, RoutedEventArgs e) => OpenNoticeScreen(initialText: null);

    // 공지 화면 편집기를 연다(초기 텍스트가 있으면 미리 채워서). 성경 "공지 화면으로 복사"에서 재사용.
    private void OpenNoticeScreen(string? initialText)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var noticeViewModel = new Easislides.Wpf.Shell.NoticeScreenViewModel(
            (text, options) => viewModel.PublishNotice(text, options),
            viewModel.ClearNotice,
            initialText,
            store: null,
            // 예배 순서에 텍스트 항목으로 추가(레거시 InfoScreen 항목) — 추가 성공 시 true.
            addToWorshipQueue: text => viewModel.AddTextItem(text) is not null);
        var window = new Easislides.Wpf.Shell.NoticeScreenWindow(noticeViewModel) { Owner = this };
        window.ShowDialog();
    }

    // 성경 본문 전체 선택(우클릭 메뉴).
    private void SelectAllBiblePassage_Click(object sender, RoutedEventArgs e) => BiblePassageBox.SelectAll();

    // 성경 본문에서 선택한 구절을 클립보드로 복사(우클릭 메뉴). 선택이 없으면 아무것도 안 한다.
    private void CopyBiblePassage_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(BiblePassageBox.SelectedText))
        {
            BiblePassageBox.Copy();
        }
    }

    // 성경 본문에서 선택한(없으면 전체) 구절을 공지 화면 편집기로 복사해 연다(레거시 Bible 우클릭 Copy to InfoScreen).
    private void CopyBibleToInfoScreen_Click(object sender, RoutedEventArgs e)
    {
        // 선택 우선·없으면 전체·둘 다 비면 null — 순수 결정 로직은 NoticeScreenViewModel.ResolveCopyText(테스트됨).
        var text = Easislides.Wpf.Shell.NoticeScreenViewModel.ResolveCopyText(BiblePassageBox.SelectedText, BiblePassageBox.Text);
        if (text is not null)
        {
            OpenNoticeScreen(text);
        }
    }

    // 찬양집 색인 — 현재 곡 라이브러리를 머리글자(초성/영문/숫자)별로 묶어 보여 준다(FrmMain PraiseBook/Listing 포팅).
    private void OpenPraiseBookIndex_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        // 라이브러리 곡 목록 → 색인 항목(제목·번호)으로 변환. 곡 폴더를 안 골랐으면 빈 목록(창이 안내 문구 표시).
        var entries = viewModel.Library.Songs
            .Select(song => new Easislides.Wpf.Library.PraiseBookIndexEntry(song.Title, song.SongNumber, song.SongId))
            .ToList();

        var indexViewModel = new Easislides.Wpf.Library.PraiseBookIndexViewModel(
            new Easislides.Wpf.Library.PraiseBookIndexService(),
            new Easislides.Wpf.Library.PraiseBookStore(),
            entries);

        var window = new Easislides.Wpf.Library.PraiseBookIndexWindow(indexViewModel) { Owner = this };
        // 곡을 더블클릭해 닫혔으면(SelectedEntryForLive) 그 곡을 라이브러리에서 찾아 예배 순서에 추가(인터랙티브 목록).
        if (window.ShowDialog() == true && window.SelectedEntryForLive is { } entry)
        {
            viewModel.AddPraiseBookSong(entry.Title, entry.Number, entry.SongId);
        }
    }

    // 이미지 갤러리 — 폴더의 이미지를 썸네일로 보고 출력 배경으로 적용(FrmMain Images 탭 포팅).
    private void OpenImageLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        // 시작 폴더: 작업 폴더(있으면), 없으면 사진 폴더. 운영자는 창에서 폴더를 바꿀 수 있다.
        var workingFolder = _services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder;
        var initialFolder = !string.IsNullOrWhiteSpace(workingFolder) && Directory.Exists(workingFolder)
            ? workingFolder
            : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        var libraryViewModel = new Easislides.Wpf.Library.ImageLibraryViewModel(
            new Easislides.Wpf.Rendering.ImageLibraryService(),
            LoadThumbnail,
            viewModel.SetOutputBackgroundImage,
            () => viewModel.ClearOutputBackgroundImageCommand.Execute(null),
            initialFolder);

        var window = new Easislides.Wpf.Library.ImageLibraryWindow(libraryViewModel) { Owner = this };
        window.ShowDialog();
    }

    // 썸네일 로더 — 140px 폭으로 다운스케일 디코딩해 메모리를 아끼고 빠르게 그린다.
    // 디코딩 실패(잠김·손상·미지원)면 null → 갤러리는 파일명만 보여 준다(안전 강등).
    private static System.Windows.Media.ImageSource? LoadThumbnail(string path)
    {
        try
        {
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreColorProfile;
            image.DecodePixelWidth = 140;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    private void OpenManageWorshipLists_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            var window = new ManageWorshipListsWindow(viewModel) { Owner = this };
            window.ShowDialog();
        }
    }

    private void OpenBible_Click(object sender, RoutedEventArgs e)
    {
        var bibleWindow = _services.GetRequiredService<BibleWindow>();
        bibleWindow.Owner = this;
        if (bibleWindow.ShowDialog() == true && DataContext is MainViewModel viewModel)
        {
            viewModel.AddBibleSelection(bibleWindow.SelectedSelection);
        }
    }

    // 성경 버전 관리 창(이름 변경) — 셸의 Bible VM 을 공유하므로 변경이 좌측 성경 탭에도 반영된다.
    private void OpenBibleVersionManager_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            var window = new Easislides.Wpf.Library.BibleVersionManagerWindow(viewModel.Bible) { Owner = this };
            window.ShowDialog();
        }
    }

    private void OpenHelp_Click(object sender, RoutedEventArgs e)
    {
        var helpWindow = _services.GetRequiredService<HelpWindow>();
        helpWindow.Owner = this;
        helpWindow.ShowDialog();
    }

    private void OpenRegistration_Click(object sender, RoutedEventArgs e)
    {
        var registrationWindow = _services.GetRequiredService<RegistrationWindow>();
        registrationWindow.Owner = this;
        registrationWindow.ShowDialog();
    }

    private void OpenAbout_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = _services.GetRequiredService<AboutWindow>();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    // ─── 메뉴바(편집) 곡/폴더 관리 창 런처 — 레거시 FrmCopy/Move/Delete/Recover/SmartMerge/RearrangeFolders 대응. ───
    // 이 창들은 빈 상태로 열면 안 되고 컨텍스트(선택 곡·폴더·DB 경로·폴더 목록)를 주입해야 동작한다.
    // 셸의 좌측 "라이브러리" 탭이 이미 그 컨텍스트(_viewModel.Library)를 갖고 있어 LibraryWindow 와 동일하게 주입한다.
    // 곡이 필요한 작업(복사/이동/삭제/폴더정리)은 선택이 없으면 안내만 하고 창을 열지 않는다(빈 창 방지).

    private void SongCopy_Click(object sender, RoutedEventArgs e)
        => LaunchSongContextWindow<SongCopyWindow, SongCopyViewModel>(
            "복사할 곡을 라이브러리 탭에서 선택하세요.",
            (vm, lib) => vm.Load(lib.DatabasePath, lib.SelectedSong!, lib.SelectedFolder!, lib.Folders));

    private void SongMove_Click(object sender, RoutedEventArgs e)
        => LaunchSongContextWindow<SongMoveWindow, SongMoveViewModel>(
            "이동할 곡을 라이브러리 탭에서 선택하세요.",
            (vm, lib) => vm.Load(lib.DatabasePath, lib.SelectedSong!, lib.SelectedFolder!, lib.Folders));

    private void SongDelete_Click(object sender, RoutedEventArgs e)
        => LaunchSongContextWindow<SongDeleteWindow, SongDeleteViewModel>(
            "삭제할 곡을 라이브러리 탭에서 선택하세요.",
            (vm, lib) => vm.Load(lib.DatabasePath, lib.SelectedSong!, lib.SelectedFolder!));

    private void FolderEditor_Click(object sender, RoutedEventArgs e)
        => LaunchFolderContextWindow<FolderEditorWindow, FolderEditorViewModel>(
            (vm, lib) => vm.Load(lib.DatabasePath, lib.SelectedFolder!, lib.Folders));

    // 복구·병합은 곡 선택이 필요 없고 DB 경로(+폴더 목록)만 있으면 된다 — 셸 라이브러리의 DB 경로를 쓴다.
    private void SongRecover_Click(object sender, RoutedEventArgs e)
        => LaunchDatabaseContextWindow<SongRecoveryWindow, SongRecoveryViewModel>(
            (vm, lib) => vm.Load(lib.DatabasePath));

    private void SongMerge_Click(object sender, RoutedEventArgs e)
        => LaunchDatabaseContextWindow<SongMergeWindow, SongMergeViewModel>(
            (vm, lib) => vm.Load(lib.DatabasePath, lib.Folders));

    // 곡 컨텍스트(선택 곡+폴더)가 필요한 창 — 선택이 없으면 안내만 하고 열지 않는다.
    private async void LaunchSongContextWindow<TWindow, TViewModel>(string noSelectionMessage, Action<TViewModel, LibraryViewModel> load)
        where TWindow : Window
        where TViewModel : class
    {
        var lib = _viewModel.Library;
        if (lib.SelectedFolder is null || lib.SelectedSong is null)
        {
            lib.StatusMessage = noSelectionMessage;
            return;
        }

        if (!TryOpenContextWindow<TWindow, TViewModel>(lib, load))
        {
            return;
        }

        await lib.LoadAsync(); // 작업 반영 — 셸 라이브러리 목록 새로고침.
    }

    // 폴더 컨텍스트(선택 폴더)가 필요한 창.
    private async void LaunchFolderContextWindow<TWindow, TViewModel>(Action<TViewModel, LibraryViewModel> load)
        where TWindow : Window
        where TViewModel : class
    {
        var lib = _viewModel.Library;
        if (lib.SelectedFolder is null)
        {
            lib.StatusMessage = "정리할 폴더를 라이브러리 탭에서 선택하세요.";
            return;
        }

        if (!TryOpenContextWindow<TWindow, TViewModel>(lib, load))
        {
            return;
        }

        await lib.LoadAsync();
    }

    // DB 경로만 필요한 창(복구/병합).
    private async void LaunchDatabaseContextWindow<TWindow, TViewModel>(Action<TViewModel, LibraryViewModel> load)
        where TWindow : Window
        where TViewModel : class
    {
        var lib = _viewModel.Library;
        if (string.IsNullOrWhiteSpace(lib.DatabasePath))
        {
            lib.StatusMessage = "AdminDB 경로를 설정에서 먼저 지정하세요.";
            return;
        }

        if (!TryOpenContextWindow<TWindow, TViewModel>(lib, load))
        {
            return;
        }

        await lib.LoadAsync();
    }

    // 공통: DI 로 창을 만들고 VM 에 컨텍스트를 주입한 뒤 모달로 띄운다. 확정(true) 시 true 반환.
    private bool TryOpenContextWindow<TWindow, TViewModel>(LibraryViewModel lib, Action<TViewModel, LibraryViewModel> load)
        where TWindow : Window
        where TViewModel : class
    {
        var window = _services.GetRequiredService<TWindow>();
        window.Owner = this;
        if (window.DataContext is not TViewModel viewModel)
        {
            return false;
        }

        load(viewModel, lib);
        return window.ShowDialog() == true;
    }

    // 보기 메뉴 — 작업 폴더를 탐색기로 연다(레거시 Menu_EasiSlidesFolder 대응).
    private void OpenEasiSlidesFolder_Click(object sender, RoutedEventArgs e)
    {
        var folder = _services.GetRequiredService<ISettingsService>().Current.General.WorkingFolder;
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            MessageBox.Show(this, "작업 폴더를 찾을 수 없습니다. 설정에서 작업 폴더를 먼저 지정하세요.",
                "EasiSlides 폴더", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // UseShellExecute=true 로 탐색기에서 폴더를 연다(경로에 공백이 있어 따옴표로 감싼다).
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
    }

    // 파일 메뉴 — 종료(레거시 Menu_Exit). 창을 닫으면 OnClosed 가 정리한다.
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    // 창 핸들이 만들어진 직후(표시 전) 저장된 크기·위치를 화면 안으로 보정해 복원한다(레거시 FrmMain 창 상태 복원).
    // 좌측 브라우저/예배순서 패널 높이 비율의 안전 범위(%, 한 패널이 사라지지 않도록 — 행 MinHeight 와 이중 안전).
    private const int BrowserSplitMinPercent = 15;
    private const int BrowserSplitMaxPercent = 85;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        RestoreWindowPlacement();
        RestoreBrowserSplit();
    }

    // 닫기 직전에 현재 창 크기·위치·최대화 상태와 좌측 패널 높이 비율을 설정에 저장한다(다음 실행 때 복원).
    protected override void OnClosing(CancelEventArgs e)
    {
        SaveWindowPlacement();
        SaveBrowserSplit();
        base.OnClosing(e);
    }

    private void RestoreBrowserSplit()
    {
        if (_settings is null)
        {
            return;
        }

        try
        {
            var percent = _settings.Get(EasiSettingKeys.MainBrowserSplitPercent);
            // 저장된 적 없음(0)이거나 안전 범위 밖이면 XAML 기본 비율(반반)을 그대로 둔다(무회귀).
            if (percent < BrowserSplitMinPercent || percent > BrowserSplitMaxPercent)
            {
                return;
            }

            // 두 행을 별(star) 비율로 설정 — 측정 전이어도 동작하고, 창 크기가 바뀌어도 비율이 유지된다.
            BrowserRow.Height = new GridLength(percent, GridUnitType.Star);
            WorshipQueueRow.Height = new GridLength(100 - percent, GridUnitType.Star);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MainWindow] 패널 비율 복원 실패: {ex.Message}");
        }
    }

    private void SaveBrowserSplit()
    {
        if (_settings is null)
        {
            return;
        }

        try
        {
            var percent = WindowPlacementCalculator.ComputeSplitPercent(
                BrowserRow.ActualHeight,
                WorshipQueueRow.ActualHeight,
                BrowserSplitMinPercent,
                BrowserSplitMaxPercent);

            // 0 은 "측정 전/비정상" — 저장하지 않는다(다음 실행은 기존 저장값 또는 기본).
            if (percent <= 0)
            {
                return;
            }

            _settings.Set(EasiSettingKeys.MainBrowserSplitPercent, percent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MainWindow] 패널 비율 저장 실패: {ex.Message}");
        }
    }

    private void RestoreWindowPlacement()
    {
        if (_settings is null)
        {
            return;
        }

        try
        {
            var placement = WindowPlacementCalculator.ComputeRestore(
                _settings.Get(EasiSettingKeys.MainWindowLeft),
                _settings.Get(EasiSettingKeys.MainWindowTop),
                _settings.Get(EasiSettingKeys.MainWindowWidth),
                _settings.Get(EasiSettingKeys.MainWindowHeight),
                _settings.Get(EasiSettingKeys.MainWindowMaximized),
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight,
                MinWidth,
                MinHeight);

            // 저장된 적 없으면(null) XAML 의 기본 크기·시작 위치를 그대로 둔다(무회귀).
            if (placement is not { } p)
            {
                return;
            }

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = p.Left;
            Top = p.Top;
            Width = p.Width;
            Height = p.Height;
            if (p.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }
        catch (Exception ex)
        {
            // 창 복원 실패가 시작 자체를 막으면 안 된다 — 기본 배치로 진행.
            Debug.WriteLine($"[MainWindow] 창 위치 복원 실패: {ex.Message}");
        }
    }

    private void SaveWindowPlacement()
    {
        if (_settings is null)
        {
            return;
        }

        try
        {
            var maximized = WindowState == WindowState.Maximized;
            // 일반 상태면 현재 좌표·실제 크기를, 최대화/최소화 상태면 RestoreBounds(원래 크기·위치)를 저장한다 —
            // 그래야 다음에 최대화를 풀었을 때 올바른 크기로 돌아간다.
            var bounds = WindowState == WindowState.Normal
                ? new Rect(Left, Top, ActualWidth, ActualHeight)
                : RestoreBounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return; // 비정상 크기는 저장하지 않음(다음 실행은 기본 배치).
            }

            _settings.Set(EasiSettingKeys.MainWindowLeft, (int)Math.Round(bounds.Left));
            _settings.Set(EasiSettingKeys.MainWindowTop, (int)Math.Round(bounds.Top));
            _settings.Set(EasiSettingKeys.MainWindowWidth, (int)Math.Round(bounds.Width));
            _settings.Set(EasiSettingKeys.MainWindowHeight, (int)Math.Round(bounds.Height));
            _settings.Set(EasiSettingKeys.MainWindowMaximized, maximized);
        }
        catch (Exception ex)
        {
            // 저장 실패는 무시 — 다음 실행은 기본 배치로 시작.
            Debug.WriteLine($"[MainWindow] 창 위치 저장 실패: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _autoRotateTimer.Stop();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnClosed(e);
    }
}
