using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Easislides.Wpf.Controls;

/// <summary>
/// EsLiveIndicator 상태 — 계획서 §5.1 EsLiveIndicator + §3.1 Q5 라이브 인디케이터.
/// </summary>
public enum LiveState
{
    /// <summary>꺼짐 (송출 안 함) — 인디케이터 미표시</summary>
    Off,
    /// <summary>대기 (전환 중) — 노란색 표시</summary>
    Standby,
    /// <summary>활성 (라이브 송출 중) — 빨간색 + Pulse 깜빡임 (Q5)</summary>
    Active,
    /// <summary>숨김 (Black/Hide 활성) — 어두운 색 표시</summary>
    Hidden,
}

/// <summary>
/// 라이브 상태 시각 인디케이터 — 계획서 §5.1 + Q5 우측 모서리 Pulse.
///
/// 사용:
///   &lt;controls:EsLiveIndicator State="Active" /&gt;
///
/// State 변경 시:
///   - Off    : Visibility=Collapsed
///   - Active : 빨강 + Pulse 애니메이션 (prefers-reduced-motion 끔 시 정적)
///   - Standby: 노랑
///   - Hidden : 어두운 회색
///
/// 토큰: Brush.Live.Active / Brush.Live.Standby / Brush.Live.Hidden
/// 애니메이션: SystemParameters.ClientAreaAnimation false 시 정적 (a11y).
/// 런타임 시스템 a11y 설정 변경도 자동 반영 (StaticPropertyChanged 구독).
/// </summary>
public class EsLiveIndicator : Control
{
    /// <summary>현재 라이브 상태 — DependencyProperty.</summary>
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State),
            typeof(LiveState),
            typeof(EsLiveIndicator),
            new PropertyMetadata(LiveState.Off, OnStateChanged));

    /// <summary>인디케이터 직경 (px) — 기본 12px.</summary>
    public static readonly DependencyProperty DiameterProperty =
        DependencyProperty.Register(
            nameof(Diameter),
            typeof(double),
            typeof(EsLiveIndicator),
            new PropertyMetadata(12.0));

    public LiveState State
    {
        get => (LiveState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public double Diameter
    {
        get => (double)GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    private Ellipse? _dot;
    private Storyboard? _pulseStoryboard;
    private PropertyChangedEventHandler? _systemHandler;

    static EsLiveIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(EsLiveIndicator),
            new FrameworkPropertyMetadata(typeof(EsLiveIndicator)));
    }

    public EsLiveIndicator()
    {
        // 컨트롤 분리 시 애니메이션·이벤트 구독 정리 — 메모리 누수 방지 (리뷰 #3)
        Unloaded += (_, _) => Cleanup();
        Loaded += (_, _) => SubscribeSystemA11y();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _dot = GetTemplateChild("PART_Dot") as Ellipse;
        ApplyState(State);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EsLiveIndicator self)
        {
            self.ApplyState((LiveState)e.NewValue);
        }
    }

    private void ApplyState(LiveState state)
    {
        if (_dot is null) return;

        StopPulse();

        switch (state)
        {
            case LiveState.Off:
                Visibility = Visibility.Collapsed;
                break;

            case LiveState.Active:
                Visibility = Visibility.Visible;
                _dot.Fill = (Brush)FindResource("Brush.Live.Active");
                if (SystemParameters.ClientAreaAnimation)
                {
                    StartPulse();
                }
                break;

            case LiveState.Standby:
                Visibility = Visibility.Visible;
                _dot.Fill = (Brush)FindResource("Brush.Live.Standby");
                break;

            case LiveState.Hidden:
                Visibility = Visibility.Visible;
                _dot.Fill = (Brush)FindResource("Brush.Live.Hidden");
                break;
        }
    }

    private void StartPulse()
    {
        if (_dot is null) return;

        // Pulse: opacity 1.0 → 0.4 → 1.0, 주기 1.2s, 무한 반복.
        // Q5 사양: 1.2초 주기, prefers-reduced-motion 끔 시 정적.
        var animation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.4,
            Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
        };

        _pulseStoryboard = new Storyboard();
        Storyboard.SetTarget(animation, _dot);
        Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
        _pulseStoryboard.Children.Add(animation);
        _pulseStoryboard.Begin();
    }

    private void StopPulse()
    {
        if (_pulseStoryboard is not null)
        {
            _pulseStoryboard.Stop();
            _pulseStoryboard.Remove();
            _pulseStoryboard = null;
        }
        // BeginAnimation(..., null)로 애니메이션 클럭과 ellipse 사이 참조 끊기 (리뷰 #3 누수 가드)
        _dot?.BeginAnimation(OpacityProperty, null);
        if (_dot is not null) _dot.Opacity = 1.0;
    }

    private void SubscribeSystemA11y()
    {
        // 시스템 a11y 설정(애니메이션 표시 토글)이 런타임에 바뀌면 즉시 반영 (리뷰 #4).
        // SystemParameters는 static 이벤트라 강한 참조가 누적되면 누수 → Unloaded 시 반드시 해제.
        if (_systemHandler is not null) return;
        _systemHandler = (s, e) =>
        {
            if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation))
            {
                Dispatcher.InvokeAsync(() => ApplyState(State));
            }
        };
        SystemParameters.StaticPropertyChanged += _systemHandler;
    }

    private void Cleanup()
    {
        StopPulse();
        if (_systemHandler is not null)
        {
            SystemParameters.StaticPropertyChanged -= _systemHandler;
            _systemHandler = null;
        }
    }
}
