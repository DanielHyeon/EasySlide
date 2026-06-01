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

    // Scene이 바뀔 때마다 전환 애니메이션을 재생. 종류와 길이는 ViewModel이 제공.
    // 모든 전환에 불투명도 페이드인을 공통 적용하고, 종류에 따라 Translate/Scale/Rotate 시작값을
    // 잡아 항등(이동0·배율1·각0)으로 애니메이션한다 → Fade/Slide/Zoom/Spin/Flip 을 한 경로로 처리.
    private void OnSceneChanged(object? sender, EventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var (translate, scale, rotate) = EnsureContentTransforms();
        var duration = _viewModel.ContentFadeDuration;
        if (duration <= TimeSpan.Zero)
        {
            // 전환 비활성(즉시 컷): 모든 애니메이션을 멈추고 항등/불투명1/클립 제거로 보장.
            ContentArea.BeginAnimation(OpacityProperty, null);
            ResetToIdentity(translate, scale, rotate);
            ContentArea.Clip = null;
            ContentArea.Opacity = 1.0;
            return;
        }

        // 공통 불투명도 페이드인(모든 전환을 부드럽게 등장시킨다).
        ContentArea.BeginAnimation(OpacityProperty, new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
        });

        // 종류별 시작 트랜스폼 → 항등으로 애니메이션. 항등(시작=끝)인 축은 애니메이션 없이 고정.
        // 클립 리빌 종류는 모든 트랜스폼이 항등(이동/배율/회전 없음) — 클립 애니메이션으로만 드러난다.
        var start = TransitionStart(_viewModel.ContentTransitionKind);
        AnimateAxis(translate, System.Windows.Media.TranslateTransform.XProperty, start.Tx, 0, duration);
        AnimateAxis(translate, System.Windows.Media.TranslateTransform.YProperty, start.Ty, 0, duration);
        AnimateAxis(scale, System.Windows.Media.ScaleTransform.ScaleXProperty, start.Sx, 1, duration);
        AnimateAxis(scale, System.Windows.Media.ScaleTransform.ScaleYProperty, start.Sy, 1, duration);
        AnimateAxis(rotate, System.Windows.Media.RotateTransform.AngleProperty, start.Angle, 0, duration);

        // 클립(마스크) 리빌 — 클립 종류면 확장 클립을 설정·애니메이션하고, 아니면 클립을 제거한다.
        ApplyClipReveal(_viewModel.ContentTransitionKind, duration);
    }

    // 클립 리빌: 새 콘텐츠를 확장하는 도형/방향 클립으로 드러낸다(단일 레이어). 비-클립 종류면 Clip 제거.
    private void ApplyClipReveal(Settings.LyricsTransitionKind kind, TimeSpan duration)
    {
        var w = ContentArea.ActualWidth > 0 ? ContentArea.ActualWidth : 1920;
        var h = ContentArea.ActualHeight > 0 ? ContentArea.ActualHeight : 1080;
        var full = new System.Windows.Rect(0, 0, w, h);

        switch (kind)
        {
            case Settings.LyricsTransitionKind.RevealCircle:
            {
                // 중심에서 반지름 0 → 모서리까지(전체 덮음) 커지는 원형 클립.
                var maxR = Math.Sqrt((w * w) + (h * h)) / 2.0;
                var ellipse = new System.Windows.Media.EllipseGeometry(new System.Windows.Point(w / 2, h / 2), 0, 0);
                ContentArea.Clip = ellipse;
                var anim = new DoubleAnimation
                {
                    From = 0,
                    To = maxR,
                    Duration = new Duration(duration),
                    FillBehavior = FillBehavior.HoldEnd,
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
                };
                // 끝나면 클립 제거(리사이즈 시 잔여 크롭 방지). 단, 그 사이 다음 장면이 새 클립을 걸었으면
                // 그 활성 클립을 지우면 안 된다 — 내 도형이 아직 붙어 있을 때만 제거(빠른 장면 전환 경쟁 방지).
                anim.Completed += (_, _) => { if (ShouldClearClip(ContentArea.Clip, ellipse)) ContentArea.Clip = null; };
                ellipse.BeginAnimation(System.Windows.Media.EllipseGeometry.RadiusXProperty, anim);
                ellipse.BeginAnimation(System.Windows.Media.EllipseGeometry.RadiusYProperty, new DoubleAnimation
                {
                    From = 0,
                    To = maxR,
                    Duration = new Duration(duration),
                    FillBehavior = FillBehavior.HoldEnd,
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
                });
                break;
            }

            case Settings.LyricsTransitionKind.RevealRectangle:
                AnimateRectClip(new System.Windows.Rect(w / 2, h / 2, 0, 0), full, duration);
                break;
            case Settings.LyricsTransitionKind.WipeRight:
                AnimateRectClip(new System.Windows.Rect(0, 0, 0, h), full, duration);
                break;
            case Settings.LyricsTransitionKind.WipeLeft:
                AnimateRectClip(new System.Windows.Rect(w, 0, 0, h), full, duration);
                break;
            case Settings.LyricsTransitionKind.WipeDown:
                AnimateRectClip(new System.Windows.Rect(0, 0, w, 0), full, duration);
                break;
            case Settings.LyricsTransitionKind.WipeUp:
                AnimateRectClip(new System.Windows.Rect(0, h, w, 0), full, duration);
                break;
            default:
                // 비-클립 종류(Fade/Slide/Zoom/Spin/Flip) — 이전 클립이 남지 않도록 제거.
                ContentArea.Clip = null;
                break;
        }
    }

    // 사각형 클립을 from→to(전체)로 RectAnimation. 끝나면 클립 제거.
    private void AnimateRectClip(System.Windows.Rect from, System.Windows.Rect full, TimeSpan duration)
    {
        var rect = new System.Windows.Media.RectangleGeometry(from);
        ContentArea.Clip = rect;
        var anim = new RectAnimation
        {
            From = from,
            To = full,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        };
        anim.Completed += (_, _) => { if (ShouldClearClip(ContentArea.Clip, rect)) ContentArea.Clip = null; };
        rect.BeginAnimation(System.Windows.Media.RectangleGeometry.RectProperty, anim);
    }

    // 전환 완료 시 클립을 제거해도 되는지 — 현재 활성 클립이 "내가 건 그 도형"일 때만 true.
    // 그 사이 다음 장면이 새 클립을 걸었으면(다른 참조) 건드리지 않는다(빠른 장면 전환 경쟁 가드).
    internal static bool ShouldClearClip(System.Windows.Media.Geometry? activeClip, System.Windows.Media.Geometry myClip)
        => ReferenceEquals(activeClip, myClip);

    // 종류별 시작 트랜스폼(이동 px·배율·각도). 끝값은 항상 항등(Tx=Ty=0, Sx=Sy=1, Angle=0).
    private (double Tx, double Ty, double Sx, double Sy, double Angle) TransitionStart(Settings.LyricsTransitionKind kind)
    {
        var w = ContentArea.ActualWidth > 0 ? ContentArea.ActualWidth : 1920;
        var h = ContentArea.ActualHeight > 0 ? ContentArea.ActualHeight : 1080;
        return kind switch
        {
            Settings.LyricsTransitionKind.SlideFromLeft => (-w, 0, 1, 1, 0),
            Settings.LyricsTransitionKind.SlideFromRight => (w, 0, 1, 1, 0),
            Settings.LyricsTransitionKind.SlideFromTop => (0, -h, 1, 1, 0),
            Settings.LyricsTransitionKind.SlideFromBottom => (0, h, 1, 1, 0),
            Settings.LyricsTransitionKind.ZoomIn => (0, 0, 0.6, 0.6, 0),       // 작게→정상(확대 등장)
            Settings.LyricsTransitionKind.ZoomOut => (0, 0, 1.4, 1.4, 0),      // 크게→정상(축소 안착)
            Settings.LyricsTransitionKind.Spin => (0, 0, 0.7, 0.7, -180),      // 반바퀴 회전+살짝 확대
            Settings.LyricsTransitionKind.FlipHorizontal => (0, 0, 0, 1, 0),   // 가로 0→1(좌우 펼침)
            Settings.LyricsTransitionKind.FlipVertical => (0, 0, 1, 0, 0),     // 세로 0→1(상하 펼침)
            _ => (0, 0, 1, 1, 0), // Fade — 모든 축 항등(불투명도만)
        };
    }

    private static void AnimateAxis(
        System.Windows.Media.Animation.IAnimatable target,
        System.Windows.DependencyProperty property,
        double from,
        double to,
        TimeSpan duration)
    {
        if (from == to)
        {
            // 변화 없는 축 — 잔여 애니메이션 제거 후 항등값 고정(이전 효과가 남지 않게).
            target.BeginAnimation(property, null);
            ((System.Windows.DependencyObject)target).SetValue(property, to);
            return;
        }

        target.BeginAnimation(property, new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        });
    }

    private static void ResetToIdentity(
        System.Windows.Media.TranslateTransform translate,
        System.Windows.Media.ScaleTransform scale,
        System.Windows.Media.RotateTransform rotate)
    {
        translate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
        translate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, null);
        scale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, null);
        scale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, null);
        rotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);
        translate.X = 0;
        translate.Y = 0;
        scale.ScaleX = 1;
        scale.ScaleY = 1;
        rotate.Angle = 0;
    }

    // ContentArea 에 Translate+Scale+Rotate 트랜스폼 그룹을 보장(없으면 생성). Scale/Rotate 가 중앙 기준으로
    // 동작하도록 RenderTransformOrigin 을 (0.5,0.5)로 둔다(Translate 는 원점 무관).
    private (System.Windows.Media.TranslateTransform Translate,
             System.Windows.Media.ScaleTransform Scale,
             System.Windows.Media.RotateTransform Rotate) EnsureContentTransforms()
    {
        if (ContentArea.RenderTransform is System.Windows.Media.TransformGroup existing
            && existing.Children.Count == 3
            && existing.Children[0] is System.Windows.Media.TranslateTransform t0
            && existing.Children[1] is System.Windows.Media.ScaleTransform s0
            && existing.Children[2] is System.Windows.Media.RotateTransform r0)
        {
            return (t0, s0, r0);
        }

        var translate = new System.Windows.Media.TranslateTransform();
        var scale = new System.Windows.Media.ScaleTransform(1, 1);
        var rotate = new System.Windows.Media.RotateTransform(0);
        var group = new System.Windows.Media.TransformGroup();
        group.Children.Add(translate);
        group.Children.Add(scale);
        group.Children.Add(rotate);
        ContentArea.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        ContentArea.RenderTransform = group;
        return (translate, scale, rotate);
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
