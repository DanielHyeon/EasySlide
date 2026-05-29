using System;
using System.Windows;
using System.Windows.Media.Animation;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf;

public partial class OutputWindow : Window, IOutputSurface
{
    private bool _shown;
    private OutputWindowViewModel? _viewModel;

    public OutputWindow()
    {
        InitializeComponent();
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
        _shown = false;
        base.OnClosed(e);
    }
}
