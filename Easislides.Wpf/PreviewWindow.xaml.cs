using System;
using System.Windows;
using System.Windows.Threading;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf;

/// <summary>
/// 스테이지(Preview) 모니터 창 — 회중용 출력 창(OutputWindow)과 별개의 확인용 화면(가사 + 시계 + 다음 항목).
/// 같은 <see cref="OutputWindowViewModel"/> 에 바인딩하되(가사·색·글꼴), 출력의 전환 엔진은 쓰지 않고 핵심만 보여 준다.
/// 회중 화면이 검정/비움이어도 가사가 계속 보이는 동작은 PreviewWindowHost 의 세션 정규화가 담당한다.
/// </summary>
public partial class PreviewWindow : Window, IOutputSurface
{
    private readonly DispatcherTimer _clockTimer;
    private bool _shown;

    public PreviewWindow()
    {
        InitializeComponent();
        // 1초마다 시계를 갱신(분까지 표시라 1초 간격이면 충분). 창이 보일 때만 돈다(Show/Close 에서 시작/정지).
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => UpdateClock();
        UpdateClock();
    }

    private void UpdateClock() => ClockText.Text = StageClock.Format(DateTime.Now);

    public void Bind(OutputWindowViewModel viewModel) => DataContext = viewModel;

    // 선택한 모니터에 맞춘다 — 풀스크린(!IsWindowed)이면 테두리 없는 top-most, 창모드면 일반 창(출력 창과 동일 규칙).
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
        UpdateClock();
        _clockTimer.Start();
        _shown = true;
    }

    public new void Close()
    {
        if (!_shown)
        {
            return;
        }

        _clockTimer.Stop();
        base.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _clockTimer.Stop(); // 창이 닫히면 타이머도 멈춰 죽은 창을 건드리지 않게 한다.
        _shown = false;
        base.OnClosed(e);
    }
}
