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
            _viewModel.SceneChanging -= OnSceneChanging;
            _viewModel.SceneChanged -= OnSceneChanged;
        }

        DataContext = viewModel;
        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.SceneChanging += OnSceneChanging;
            _viewModel.SceneChanged += OnSceneChanged;
        }
    }

    // 장면이 바뀌기 직전 — 오목 도형 전환(뒤에 옛 콘텐츠가 보여야 하는 전환)일 때만 ContentArea 의 현재(옛) 프레임을
    // 스냅샷해 PreviousLayer 에 깐다. 그 외 전환은 PreviousLayer 를 비워 둔다(단일 레이어, 무회귀).
    private void OnSceneChanging(object? sender, EventArgs e)
    {
        if (_viewModel is null
            || _viewModel.ContentFadeDuration <= TimeSpan.Zero
            || !RequiresPreviousLayer(_viewModel.ContentTransitionKind)
            // 영상(MediaElement) 이 송출 중이면 RenderTargetBitmap 이 영상 프레임을 검게 캡처한다(예외가 아니라 검은 비트맵).
            // → 뒤 레이어가 검은 코너로 보이므로 단일 레이어로 강등한다(옛 프레임 없이 Cross 진행).
            || OutputMediaElement.Source is not null)
        {
            ClearPreviousLayer();
            return;
        }

        var snapshot = TrySnapshotContent();
        if (snapshot is null)
        {
            ClearPreviousLayer(); // 스냅샷 자체가 실패(크기 0·예외 등) → 단일 레이어로 강등(옛 프레임 없이 진행).
            return;
        }

        PreviousLayer.Source = snapshot;
        PreviousLayer.Visibility = Visibility.Visible;
    }

    // 뒤 레이어(옛 프레임)가 필요한 전환 = 끝에 화면 전체를 못 덮는 오목 도형(현재 Cross). 나머지는 불필요.
    private static bool RequiresPreviousLayer(Settings.LyricsTransitionKind kind)
        => kind == Settings.LyricsTransitionKind.Cross;

    private void ClearPreviousLayer()
    {
        PreviousLayer.Visibility = Visibility.Collapsed;
        PreviousLayer.Source = null;
    }

    // ContentArea 의 현재(옛) 비주얼을 비트맵으로 캡처. 크기 0·예외면 null → 단일 레이어 강등.
    // (영상은 호출 전에 OnSceneChanging 이 걸러낸다 — RTB 가 검게 캡처하므로 별도 처리.)
    private System.Windows.Media.ImageSource? TrySnapshotContent()
    {
        try
        {
            var w = (int)ContentArea.ActualWidth;
            var h = (int)ContentArea.ActualHeight;
            if (w <= 0 || h <= 0)
            {
                return null;
            }

            // 옛 프레임이 확실히 배치(arrange)된 상태에서 캡처하도록 강제 — 첫 전환/리사이즈 직후 빈 프레임 방지.
            // (SceneChanging 은 새 콘텐츠 적용 전이라 VM 바인딩을 건드리지 않으므로 재진입 위험 없음.)
            ContentArea.UpdateLayout();

            var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(w, h, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            rtb.Render(ContentArea);
            rtb.Freeze();
            return rtb;
        }
        catch
        {
            return null;
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
            // 전환 비활성(즉시 컷): 모든 애니메이션을 멈추고 항등/불투명1/클립·뒤 레이어 제거로 보장.
            ContentArea.BeginAnimation(OpacityProperty, null);
            ResetToIdentity(translate, scale, rotate);
            ContentArea.Clip = null;
            ClearPreviousLayer();
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
            case Settings.LyricsTransitionKind.BlindsHorizontal:
                AnimateTileClip(BuildBlinds(w, h, horizontal: true, duration), duration);
                break;
            case Settings.LyricsTransitionKind.BlindsVertical:
                AnimateTileClip(BuildBlinds(w, h, horizontal: false, duration), duration);
                break;
            case Settings.LyricsTransitionKind.Checkerboard:
                AnimateTileClip(BuildCheckerboard(w, h, duration), duration);
                break;
            case Settings.LyricsTransitionKind.Diamond:
                // 다이아몬드(마름모)를 중심에서 0→1 배율로 키운다. 변 길이 (w+h)/2 라 배율 1 에서 화면 전체를 덮는다.
                AnimateScaledShapeClip(BuildDiamond(w, h), w / 2, h / 2, duration);
                break;
            case Settings.LyricsTransitionKind.Star:
                // 별을 중심에서 0→1 배율로 키운다. 안쪽 반지름이 모서리 거리보다 커서 배율 1 에서 전체를 덮는다.
                AnimateScaledShapeClip(BuildStar(w, h), w / 2, h / 2, duration);
                break;
            case Settings.LyricsTransitionKind.Cross:
                // 십자를 중심에서 0→1 배율로 키운다. 코너를 못 덮어 뒤에 옛 프레임(PreviousLayer)이 보인다(2-레이어).
                AnimateScaledShapeClip(BuildCross(w, h), w / 2, h / 2, duration);
                break;
            case Settings.LyricsTransitionKind.DoorsOpen:
                AnimateTileClip(BuildDoors(w, h, open: true, duration), duration);
                break;
            case Settings.LyricsTransitionKind.DoorsClose:
                AnimateTileClip(BuildDoors(w, h, open: false, duration), duration);
                break;
            default:
                // 비-클립 종류(Fade/Slide/Zoom/Spin/Flip) — 이전 클립·뒤 레이어가 남지 않도록 제거.
                ContentArea.Clip = null;
                ClearPreviousLayer();
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

    // 한 타일의 클립 애니메이션 사양(시작/끝 사각·시작지연·길이).
    internal readonly record struct TileClip(
        System.Windows.Rect From,
        System.Windows.Rect To,
        TimeSpan Begin,
        TimeSpan Duration);

    // 블라인드 띠 — 화면을 N개 가로(또는 세로) 띠로 나눠 각 띠가 두께 0→전체로 동시에 커진다.
    internal static System.Collections.Generic.List<TileClip> BuildBlinds(double w, double h, bool horizontal, TimeSpan duration)
    {
        const int Count = 8;
        var tiles = new System.Collections.Generic.List<TileClip>(Count);
        if (horizontal)
        {
            var sh = h / Count;
            for (var i = 0; i < Count; i++)
            {
                var y = i * sh;
                tiles.Add(new TileClip(new System.Windows.Rect(0, y, w, 0), new System.Windows.Rect(0, y, w, sh), TimeSpan.Zero, duration));
            }
        }
        else
        {
            var sw = w / Count;
            for (var i = 0; i < Count; i++)
            {
                var x = i * sw;
                tiles.Add(new TileClip(new System.Windows.Rect(x, 0, 0, h), new System.Windows.Rect(x, 0, sw, h), TimeSpan.Zero, duration));
            }
        }

        return tiles;
    }

    // 체커보드 — 격자 셀이 중심에서 커진다. 짝/홀 셀을 2단계(시작지연)로 나눠 체커 패턴으로 드러난다.
    internal static System.Collections.Generic.List<TileClip> BuildCheckerboard(double w, double h, TimeSpan duration)
    {
        const int Cols = 10;
        const int Rows = 6;
        var cw = w / Cols;
        var ch = h / Rows;
        var phase = TimeSpan.FromMilliseconds(duration.TotalMilliseconds * 0.45);
        var cellDur = TimeSpan.FromMilliseconds(duration.TotalMilliseconds * 0.55);
        var tiles = new System.Collections.Generic.List<TileClip>(Cols * Rows);
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Cols; c++)
            {
                var cx = c * cw;
                var cy = r * ch;
                // 짝 셀은 즉시, 홀 셀은 지연 시작 → 두 단계가 모두 duration 안에 끝난다.
                var begin = ((r + c) % 2 == 0) ? TimeSpan.Zero : phase;
                tiles.Add(new TileClip(
                    new System.Windows.Rect(cx + cw / 2, cy + ch / 2, 0, 0),
                    new System.Windows.Rect(cx, cy, cw, ch),
                    begin,
                    cellDur));
            }
        }

        return tiles;
    }

    // 다중 타일 클립 — 여러 RectangleGeometry 를 GeometryGroup 으로 묶어 각각 from→to 로 RectAnimation.
    // 모든 타일이 끝나면(카운트다운) 클립 제거. 그 사이 다음 장면이 새 클립을 걸었으면 건드리지 않는다(참조 가드).
    private void AnimateTileClip(System.Collections.Generic.List<TileClip> tiles, TimeSpan duration)
    {
        var group = new System.Windows.Media.GeometryGroup();
        var rects = new System.Windows.Media.RectangleGeometry[tiles.Count];
        for (var i = 0; i < tiles.Count; i++)
        {
            rects[i] = new System.Windows.Media.RectangleGeometry(tiles[i].From);
            group.Children.Add(rects[i]);
        }

        ContentArea.Clip = group;

        // 카운트다운 전제: 각 RectAnimation 은 유한 길이·반복 1회·HoldEnd 라 UI 스레드에서 Completed 가 정확히 1회
        // 발화한다(BeginTime 지연돼도 마찬가지). 따라서 remaining 은 반드시 0에 도달해 클립이 정리된다.
        // (RepeatBehavior.Forever 를 넣거나 duration<=0 경로로 들어오면 이 전제가 깨진다 — OnSceneChanged 가
        //  duration<=0 을 조기 반환하므로 여기엔 항상 양수 길이만 온다.)
        var remaining = tiles.Count;
        for (var i = 0; i < tiles.Count; i++)
        {
            var anim = new RectAnimation
            {
                From = tiles[i].From,
                To = tiles[i].To,
                BeginTime = tiles[i].Begin,
                Duration = new Duration(tiles[i].Duration),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
            };
            anim.Completed += (_, _) =>
            {
                // 모든 타일이 완료된 마지막 시점에만, 그리고 내 클립이 아직 활성일 때만 제거.
                if (--remaining == 0 && ShouldClearClip(ContentArea.Clip, group))
                {
                    ContentArea.Clip = null;
                }
            };
            rects[i].BeginAnimation(System.Windows.Media.RectangleGeometry.RectProperty, anim);
        }
    }

    // 화면을 덮는 마름모(다이아몬드) 지오메트리 — 꼭짓점은 중심에서 상하좌우 (w+h)/2 거리.
    // 배율 1 에서 |x-cx|+|y-cy| <= (w+h)/2 영역이 모서리(w/2+h/2)까지 포함 → 화면 전체 덮음(잔여 마스크 없음).
    internal static System.Windows.Media.PathGeometry BuildDiamond(double w, double h)
    {
        var cx = w / 2;
        var cy = h / 2;
        // 모서리 L1 거리 = w/2+h/2 = (w+h)/2 이므로 정확히 변 위에 놓인다. +1px 오버스캔으로 모서리를
        // 변 안쪽으로 밀어 안티에일리어싱 모서리 슬라이버(마지막 1프레임)를 없앤다(code-reviewer MAJOR).
        var s = (w + h) / 2 + 1;
        var fig = new System.Windows.Media.PathFigure { StartPoint = new System.Windows.Point(cx, cy - s), IsClosed = true };
        fig.Segments.Add(new System.Windows.Media.LineSegment(new System.Windows.Point(cx + s, cy), true));
        fig.Segments.Add(new System.Windows.Media.LineSegment(new System.Windows.Point(cx, cy + s), true));
        fig.Segments.Add(new System.Windows.Media.LineSegment(new System.Windows.Point(cx - s, cy), true));
        var geo = new System.Windows.Media.PathGeometry();
        geo.Figures.Add(fig);
        return geo;
    }

    // 화면을 덮는 5각 별 지오메트리 — 안쪽 반지름(오목 골)을 모서리 거리+1 로 잡아 배율 1 에서 모든 화면점을
    // 포함한다(별의 가장 안쪽 경계점 = 골이 모서리보다 멀리 있으므로 전체 덮음). 바깥 점은 화면 밖으로 뻗는다.
    // 애니메이션 중에는 별이 커지는 모습이 보이고, 끝에는 화면을 가득 덮어 잔여 마스크가 없다.
    internal static System.Windows.Media.PathGeometry BuildStar(double w, double h)
    {
        var cx = w / 2;
        var cy = h / 2;
        var cornerDist = Math.Sqrt((w / 2 * (w / 2)) + (h / 2 * (h / 2)));
        var innerR = cornerDist + 1;          // 골(가장 안쪽)이 모서리보다 멀리 → 전체 덮음.
        var outerR = innerR / 0.382;           // 표준 5각 별 안/바깥 비율(≈0.382)로 뾰족한 별.

        var fig = new System.Windows.Media.PathFigure { IsClosed = true };
        // 위 꼭짓점부터 시작해 바깥/안쪽을 번갈아 10개 꼭짓점(72°/2=36° 간격).
        for (var i = 0; i < 10; i++)
        {
            var r = (i % 2 == 0) ? outerR : innerR;
            var angle = (-System.Math.PI / 2) + (i * System.Math.PI / 5); // -90°에서 36°씩.
            var p = new System.Windows.Point(cx + (r * System.Math.Cos(angle)), cy + (r * System.Math.Sin(angle)));
            if (i == 0)
            {
                fig.StartPoint = p;
            }
            else
            {
                fig.Segments.Add(new System.Windows.Media.LineSegment(p, true));
            }
        }

        var geo = new System.Windows.Media.PathGeometry();
        geo.Figures.Add(fig);
        return geo;
    }

    // 십자(플러스) — 전체 너비 가로 막대 + 전체 높이 세로 막대(GeometryGroup). 배율 1 에서 코너는 못 덮으므로
    // 뒤 레이어(옛 프레임)와 함께 써야 자연스럽다(코너에 옛 화면이 보였다가 완료 시 새 화면으로).
    internal static System.Windows.Media.GeometryGroup BuildCross(double w, double h)
    {
        var cx = w / 2;
        var cy = h / 2;
        var barW = w / 3; // 세로 막대 폭.
        var barH = h / 3; // 가로 막대 높이.
        // Nonzero: 두 막대가 겹치는 중심(교차)도 채워야 한다(기본 EvenOdd 면 겹친 영역이 구멍이 됨).
        var group = new System.Windows.Media.GeometryGroup { FillRule = System.Windows.Media.FillRule.Nonzero };
        group.Children.Add(new System.Windows.Media.RectangleGeometry(new System.Windows.Rect(0, cy - (barH / 2), w, barH)));
        group.Children.Add(new System.Windows.Media.RectangleGeometry(new System.Windows.Rect(cx - (barW / 2), 0, barW, h)));
        return group;
    }

    // 양문 열기/닫기 — 좌우 두 사각이 합쳐 화면 전체를 덮는다(끝에 잔여 마스크 없음).
    // open=true: 중심에서 양 끝으로 열림. open=false: 양 끝에서 중심으로 닫힘. 두 문 모두 즉시 시작·전체 길이.
    internal static System.Collections.Generic.List<TileClip> BuildDoors(double w, double h, bool open, TimeSpan duration)
    {
        var cx = w / 2;
        return open
            ? new System.Collections.Generic.List<TileClip>
            {
                new(new System.Windows.Rect(cx, 0, 0, h), new System.Windows.Rect(0, 0, cx, h), System.TimeSpan.Zero, duration),
                new(new System.Windows.Rect(cx, 0, 0, h), new System.Windows.Rect(cx, 0, cx, h), System.TimeSpan.Zero, duration),
            }
            : new System.Collections.Generic.List<TileClip>
            {
                new(new System.Windows.Rect(0, 0, 0, h), new System.Windows.Rect(0, 0, cx, h), System.TimeSpan.Zero, duration),
                new(new System.Windows.Rect(w, 0, 0, h), new System.Windows.Rect(cx, 0, cx, h), System.TimeSpan.Zero, duration),
            };
    }

    // 중심에서 0→1 배율로 키우는 도형 클립(다이아몬드 등). 끝나면 클립 제거(빠른 전환 경쟁은 참조 가드).
    private void AnimateScaledShapeClip(System.Windows.Media.Geometry shape, double cx, double cy, TimeSpan duration)
    {
        var scale = new System.Windows.Media.ScaleTransform(0, 0, cx, cy);
        shape.Transform = scale;
        ContentArea.Clip = shape;
        var animX = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        };
        animX.Completed += (_, _) =>
        {
            // 내 클립이 아직 활성일 때만 정리(빠른 전환 경쟁 가드) — 클립 제거 + 뒤 레이어(2-레이어 Cross 옛 프레임) 숨김.
            if (ShouldClearClip(ContentArea.Clip, shape))
            {
                ContentArea.Clip = null;
                ClearPreviousLayer();
            }
        };
        scale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, animX);
        scale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        });
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
            _viewModel.SceneChanging -= OnSceneChanging;
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
