using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
        toast.ArmClose(popup, dismissAfter);
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
