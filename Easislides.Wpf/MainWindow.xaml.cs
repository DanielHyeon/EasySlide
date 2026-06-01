using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
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

        if (_shortcuts.TryHandle(e.Key, Keyboard.Modifiers))
        {
            e.Handled = true;
        }
    }

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
