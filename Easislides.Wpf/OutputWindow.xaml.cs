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

    // Scene이 바뀔 때마다 ContentArea를 0 → 1로 페이드 인. 길이는 ViewModel이 제공.
    private void OnSceneChanged(object? sender, EventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var duration = _viewModel.ContentFadeDuration;
        if (duration <= TimeSpan.Zero)
        {
            // 페이드 비활성: 즉시 1로 보장(이전 애니메이션이 남아 있을 수 있음).
            ContentArea.BeginAnimation(OpacityProperty, null);
            ContentArea.Opacity = 1.0;
            return;
        }

        var animation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd
        };
        ContentArea.BeginAnimation(OpacityProperty, animation);
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
