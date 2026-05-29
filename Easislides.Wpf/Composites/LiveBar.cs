using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Easislides.Wpf.Composites;

/// <summary>
/// LiveBar ViewModel — 계획서 §6.2.1 FrmMain 상단 56px LiveBar.
///
/// 책임:
///   - 현재 라이브 송출 정보 (제목·모니터·다음 단축키 힌트)
///   - LiveState 토글 (Off/Standby/Active/Hidden)
///
/// Q5 라이브 인디케이터 이중 배치의 "상단 LiveBar" 부분 (개별 아이템 Pulse는 별도).
/// </summary>
public sealed partial class LiveBarViewModel : ObservableObject
{
    /// <summary>현재 송출 상태 — EsLiveIndicator.State에 바인딩.</summary>
    [ObservableProperty] private Controls.LiveState _state = Controls.LiveState.Off;

    /// <summary>송출 중인 아이템 제목 (예: "주일찬양 #3 은혜로다").</summary>
    [ObservableProperty] private string _currentItemTitle = string.Empty;

    /// <summary>송출 중인 모니터 식별자 (예: "모니터 2").</summary>
    [ObservableProperty] private string _outputMonitorName = string.Empty;

    /// <summary>사용자에게 보여줄 단축키 힌트 (예: "Space=Next").</summary>
    [ObservableProperty] private string _shortcutHint = "Space=다음 슬라이드";

    /// <summary>State가 Off일 때는 LiveBar 전체를 숨김.</summary>
    public Visibility BarVisibility => State == Controls.LiveState.Off
        ? Visibility.Collapsed : Visibility.Visible;

    partial void OnStateChanged(Controls.LiveState value)
    {
        // State 변경 시 파생 속성들도 통지 — XAML 바인딩 자동 갱신 보장
        OnPropertyChanged(nameof(BarVisibility));
        OnPropertyChanged(nameof(StateLabel));
    }

    /// <summary>현재 상태에 따른 사용자용 한글 라벨.</summary>
    public string StateLabel => State switch
    {
        Controls.LiveState.Active => "LIVE",
        Controls.LiveState.Standby => "STANDBY",
        Controls.LiveState.Hidden => "HIDDEN",
        _ => string.Empty,
    };

    partial void OnCurrentItemTitleChanged(string value) => OnPropertyChanged(nameof(BarVisibility));
}

/// <summary>
/// LiveBar 컨트롤 — UserControl 패턴. XAML은 LiveBar.xaml에 정의.
/// </summary>
public partial class LiveBar : UserControl
{
    // 현재 PropertyChanged를 구독 중인 ViewModel — DataContext 교체 시 누수 없이 재구독하기 위해 보관.
    private LiveBarViewModel? _subscribedViewModel;

    public LiveBar()
    {
        InitializeComponent();

        // DataContext(=ViewModel)가 바뀌면 라이브 영역 알림 구독을 갱신한다.
        DataContextChanged += OnDataContextChanged;
    }

    public LiveBarViewModel ViewModel
    {
        get => (LiveBarViewModel)DataContext;
        set => DataContext = value;
    }

    /// <summary>접근성 이름·라이브 영역에서 안전하게 접근할 ViewModel (없으면 null).</summary>
    internal LiveBarViewModel? ViewModelOrNull => DataContext as LiveBarViewModel;

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // 약한 이벤트(WeakEventManager) 사용 — ViewModel이 LiveBar를 강하게 붙잡지 않도록.
        // (ViewModel은 보통 앱 수명 동안 살아있으므로, 강참조 구독이면 LiveBar가 GC되지 못함)
        // DataContext 교체 시 이전 구독을 해제하고 새 ViewModel을 다시 구독한다.
        if (_subscribedViewModel is not null)
        {
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                .RemoveHandler(_subscribedViewModel, nameof(INotifyPropertyChanged.PropertyChanged), OnViewModelPropertyChanged);
            _subscribedViewModel = null;
        }

        if (e.NewValue is LiveBarViewModel vm)
        {
            _subscribedViewModel = vm;
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                .AddHandler(vm, nameof(INotifyPropertyChanged.PropertyChanged), OnViewModelPropertyChanged);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 송출 상태/현재 아이템/상태 라벨이 바뀌면 스크린리더에 즉시 통지한다.
        if (e.PropertyName is nameof(LiveBarViewModel.State)
            or nameof(LiveBarViewModel.CurrentItemTitle)
            or nameof(LiveBarViewModel.StateLabel))
        {
            RaiseLiveRegionChanged();
        }
    }

    /// <summary>
    /// 스크린리더에 "라이브 영역 내용이 바뀌었다"고 통지.
    /// 듣는 자동화 클라이언트가 없으면 비용 없이 즉시 반환.
    /// </summary>
    private void RaiseLiveRegionChanged()
    {
        if (!AutomationPeer.ListenerExists(AutomationEvents.LiveRegionChanged))
        {
            return;
        }

        var peer = UIElementAutomationPeer.FromElement(this)
            ?? UIElementAutomationPeer.CreatePeerForElement(this);
        peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }

    /// <summary>
    /// 커스텀 자동화 피어 — LiveBar를 Assertive 라이브 영역으로 노출하고
    /// "상태 라벨: 현재 아이템"을 접근성 이름으로 읽어준다 (계획서 §7.3).
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new LiveBarAutomationPeer(this);

    private sealed class LiveBarAutomationPeer : FrameworkElementAutomationPeer
    {
        public LiveBarAutomationPeer(LiveBar owner) : base(owner)
        {
        }

        // 라이브 송출 상태 변화는 포커스 없이도 즉시 읽혀야 하므로 Assertive.
        protected override AutomationLiveSetting GetLiveSettingCore() => AutomationLiveSetting.Assertive;

        // 상태 바는 상태 텍스트 알림이므로 StatusBar 컨트롤 타입으로 노출.
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.StatusBar;

        protected override string GetNameCore()
        {
            var bar = (LiveBar)Owner;
            var vm = bar.ViewModelOrNull;
            if (vm is null)
            {
                return base.GetNameCore();
            }

            var parts = new[] { vm.StateLabel, vm.CurrentItemTitle }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            var name = string.Join(": ", parts);
            return string.IsNullOrWhiteSpace(name) ? base.GetNameCore() : name;
        }
    }
}
