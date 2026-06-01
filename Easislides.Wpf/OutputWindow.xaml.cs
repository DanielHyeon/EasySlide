using System;
using System.Windows;
using System.Windows.Media.Animation;
using Easislides.Wpf.Media;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf;

// 레거시 대체: FrmLyricsScreen(출력 렌더) + FrmShowAlert 오버레이. 정보화면(FrmInfoScreen)은 미포팅(gap-analysis.md §2.A).
public partial class OutputWindow : Window, IOutputSurface
{
    private bool _shown;
    private OutputWindowViewModel? _viewModel;
    private AttachableMediaPlaybackBackend? _mediaBridge;

    public OutputWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 출력 창의 MediaElement 를 실제 미디어 백엔드로 감싸 생명주기 브리지에 부착한다
    /// (실제 미디어 백엔드 트랙 2단계). DI 팩토리가 창 생성 시 호출. 창이 닫히면 OnClosed 에서 분리.
    /// </summary>
    public void AttachMedia(AttachableMediaPlaybackBackend bridge)
    {
        _mediaBridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        bridge.Attach(new WpfMediaElementPlaybackBackend(OutputMediaElement));
    }

    public void Bind(OutputWindowViewModel viewModel)
    {
        // 이전 ViewModel 구독 해제 — 윈도우 재바인딩 가능성에 대비.
        if (_viewModel is not null)
        {
            _viewModel.SceneChanged -= OnSceneChanged;
        }

        DataContext = viewModel;
        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.SceneChanged += OnSceneChanged;
        }
    }

    // Scene이 바뀔 때마다 전환 애니메이션을 재생. 종류(Fade/Slide)와 길이는 ViewModel이 제공.
    // Fade=불투명도 0→1, Slide*=새 콘텐츠가 해당 방향에서 밀려 들어옴(불투명도와 함께).
    private void OnSceneChanged(object? sender, EventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var translate = EnsureContentTranslate();
        var duration = _viewModel.ContentFadeDuration;
        if (duration <= TimeSpan.Zero)
        {
            // 전환 비활성(즉시 컷): 남아 있을 수 있는 애니메이션을 멈추고 기본 위치/불투명도로 보장.
            ContentArea.BeginAnimation(OpacityProperty, null);
            translate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
            translate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, null);
            ContentArea.Opacity = 1.0;
            translate.X = 0;
            translate.Y = 0;
            return;
        }

        // 모든 전환에 불투명도 페이드인을 공통 적용(슬라이드도 부드럽게 등장).
        ContentArea.BeginAnimation(OpacityProperty, new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
        });

        // 슬라이드 방향에 따라 시작 오프셋을 잡고 0 으로 애니메이션. Fade 면 오프셋 없음(0→0).
        var (fromX, fromY) = SlideStartOffset(_viewModel.ContentTransitionKind);
        AnimateTranslate(translate, System.Windows.Media.TranslateTransform.XProperty, fromX, duration);
        AnimateTranslate(translate, System.Windows.Media.TranslateTransform.YProperty, fromY, duration);
    }

    // 슬라이드 시작 오프셋(px) — 새 콘텐츠가 화면 밖에서 들어오도록. 콘텐츠 영역 크기 기준(없으면 폴백 1920/1080).
    private (double X, double Y) SlideStartOffset(Settings.LyricsTransitionKind kind)
    {
        var w = ContentArea.ActualWidth > 0 ? ContentArea.ActualWidth : 1920;
        var h = ContentArea.ActualHeight > 0 ? ContentArea.ActualHeight : 1080;
        return kind switch
        {
            Settings.LyricsTransitionKind.SlideFromLeft => (-w, 0),
            Settings.LyricsTransitionKind.SlideFromRight => (w, 0),
            Settings.LyricsTransitionKind.SlideFromTop => (0, -h),
            Settings.LyricsTransitionKind.SlideFromBottom => (0, h),
            _ => (0, 0), // Fade
        };
    }

    private static void AnimateTranslate(
        System.Windows.Media.TranslateTransform translate,
        System.Windows.DependencyProperty axis,
        double from,
        TimeSpan duration)
    {
        if (from == 0)
        {
            // 이동 없음(Fade) — 잔여 애니메이션 제거 후 0 고정.
            translate.BeginAnimation(axis, null);
            if (axis == System.Windows.Media.TranslateTransform.XProperty) translate.X = 0; else translate.Y = 0;
            return;
        }

        translate.BeginAnimation(axis, new DoubleAnimation
        {
            From = from,
            To = 0,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        });
    }

    // ContentArea 에 슬라이드용 TranslateTransform 을 보장(없으면 생성). 기존 RenderTransform 을 보존하지 않는
    // 단순 콘텐츠 영역이라 직접 할당해도 안전하다.
    private System.Windows.Media.TranslateTransform EnsureContentTranslate()
    {
        if (ContentArea.RenderTransform is System.Windows.Media.TranslateTransform existing)
        {
            return existing;
        }

        var translate = new System.Windows.Media.TranslateTransform();
        ContentArea.RenderTransform = translate;
        return translate;
    }

    public void ApplyPlacement(OutputWindowPlacement placement)
    {
        Left = placement.Left;
        Top = placement.Top;
        Width = placement.Width;
        Height = placement.Height;

        if (placement.IsWindowed)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            Topmost = false;
            ShowInTaskbar = true;
            return;
        }

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        ShowInTaskbar = false;
    }

    public new void Show()
    {
        if (_shown)
        {
            return;
        }

        base.Show();
        _shown = true;
    }

    public new void Close()
    {
        if (!_shown)
        {
            return;
        }

        base.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.SceneChanged -= OnSceneChanged;
            _viewModel = null;
        }

        // 미디어 브리지 분리 — 이 창의 MediaElement 가 사라지므로 백엔드를 떼어 이후 호출이 죽은 컨트롤에 닿지 않게 한다.
        _mediaBridge?.Detach();
        _mediaBridge = null;

        _shown = false;
        base.OnClosed(e);
    }
}
