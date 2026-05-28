using System.Windows;
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
    public LiveBar()
    {
        InitializeComponent();
    }

    public LiveBarViewModel ViewModel
    {
        get => (LiveBarViewModel)DataContext;
        set => DataContext = value;
    }
}
