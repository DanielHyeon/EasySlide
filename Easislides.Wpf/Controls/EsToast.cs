using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Easislides.Wpf.Controls;

/// <summary>토스트 유형 — 계획서 §5.1.</summary>
public enum ToastKind
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// EsToast — 일시 알림 (모달 아님, auto-dismiss). 계획서 §5.1 + §6.2.4 FrmShowAlert 대체.
///
/// 사용 (정적 헬퍼):
///   EsToast.Show(parentWindow, ToastKind.Success, "저장됨", "예배 리스트가 저장되었습니다");
///
/// 동작:
///   - 부모 윈도우 우측 상단에 슬라이드 인 (250ms)
///   - 기본 5초 후 자동 닫힘
///   - 마우스 호버 시 카운트다운 정지 (사용자가 읽을 수 있도록)
///   - 마우스 떠나면 절반 시간 후 닫힘 (단일 CTS로 정확 재구독)
///   - 클릭 시 즉시 닫힘
///   - Popup 닫힐 때 모든 리소스(CTS, Popup.Child) 정리 — 메모리 누수 방지
/// </summary>
public class EsToast : Control
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(ToastKind), typeof(EsToast),
            new PropertyMetadata(ToastKind.Info));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(EsToast),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(EsToast),
            new PropertyMetadata(string.Empty));

    public ToastKind Kind { get => (ToastKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Message { get => (string)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }

    // 토스트 인스턴스가 자체 CTS 관리 — MouseEnter/Leave 재구독 시에도 단일 활성 CTS만 유지.
    private CancellationTokenSource? _activeCts;

    static EsToast()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EsToast),
            new FrameworkPropertyMetadata(typeof(EsToast)));
    }

    /// <summary>토스트를 부모 윈도우에 표시. 5초 후 자동 닫힘.</summary>
    public static void Show(Window owner, ToastKind kind, string title, string message, TimeSpan? autoDismiss = null)
    {
        if (owner is null) throw new ArgumentNullException(nameof(owner));
        var dismissAfter = autoDismiss ?? TimeSpan.FromSeconds(5);

        // 토스트 너비 — 부모 너비가 좁으면 부모에 맞춤 (음수 오프셋 방지)
        const double toastWidth = 340;
        var width = Math.Min(toastWidth, Math.Max(200, owner.ActualWidth - 48));
        var rightMargin = Math.Max(0, owner.ActualWidth - width - 24);

        var popup = new Popup
        {
            Placement = PlacementMode.RelativePoint,
            PlacementTarget = owner,
            HorizontalOffset = rightMargin,
            VerticalOffset = 24,
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Slide,
            StaysOpen = true,
        };

        var toast = new EsToast
        {
            Kind = kind,
            Title = title,
            Message = message,
            Width = width,
        };

        // 단일 진입 정리 — 두 경로(timer/click)가 모두 닫기를 트리거할 수 있으므로 idempotent.
        void Close()
        {
            toast.CancelActive();
            if (popup.IsOpen) popup.IsOpen = false;
        }

        toast.MouseEnter += (_, _) => toast.CancelActive();
        toast.MouseLeave += (_, _) => toast.ArmClose(popup, TimeSpan.FromSeconds(dismissAfter.TotalSeconds / 2));
        toast.MouseLeftButtonUp += (_, _) => Close();

        // Popup 정리 — 닫힐 때 CTS·Child 참조 해제 (GC 회수 보장)
        popup.Closed += (_, _) =>
        {
            toast.CancelActive();
            popup.Child = null;
        };

        popup.Child = toast;
        popup.IsOpen = true;

        // 라이브 영역 알림 — 스크린리더가 포커스 없이도 토스트 내용을 즉시 읽도록
        // LiveRegionChanged 자동화 이벤트를 올린다 (계획서 §7.3).
        // 팝업이 막 열린 직후엔 토스트가 아직 자동화 트리에 연결되기 전일 수 있으므로,
        // Loaded 우선순위로 한 단계 미뤄 피어가 준비된 뒤 알림을 올린다.
        toast.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(toast.RaiseLiveRegionChanged));

        toast.ArmClose(popup, dismissAfter);
    }

    /// <summary>
    /// 스크린리더에 "이 라이브 영역 내용이 바뀌었다"고 통지한다.
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
    /// 커스텀 자동화 피어 — 토스트를 Assertive 라이브 영역으로 노출하고
    /// 제목+본문을 접근성 이름으로 읽어준다 (계획서 §7.3).
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new EsToastAutomationPeer(this);

    private sealed class EsToastAutomationPeer : FrameworkElementAutomationPeer
    {
        public EsToastAutomationPeer(EsToast owner) : base(owner)
        {
        }

        // 포커스가 없어도 즉시 읽히는 Assertive 라이브 영역.
        protected override AutomationLiveSetting GetLiveSettingCore() => AutomationLiveSetting.Assertive;

        // 토스트는 본문 텍스트 알림이므로 Text 컨트롤 타입으로 노출.
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;

        protected override string GetNameCore()
        {
            var toast = (EsToast)Owner;
            var parts = new[] { toast.Title, toast.Message }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            var name = string.Join(". ", parts);
            return string.IsNullOrWhiteSpace(name) ? base.GetNameCore() : name;
        }
    }

    private void ArmClose(Popup popup, TimeSpan after)
    {
        CancelActive();
        var cts = new CancellationTokenSource();
        _activeCts = cts;

        _ = Task.Delay(after, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;
            popup.Dispatcher.InvokeAsync(() =>
            {
                if (popup.IsOpen) popup.IsOpen = false;
            });
        }, TaskScheduler.Default);
    }

    private void CancelActive()
    {
        var cts = Interlocked.Exchange(ref _activeCts, null);
        if (cts is null) return;
        try { cts.Cancel(); } catch { }
        cts.Dispose();
    }
}
