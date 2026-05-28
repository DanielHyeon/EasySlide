using System;
using System.Windows;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf;

public partial class OutputWindow : Window, IOutputSurface
{
    private bool _shown;

    public OutputWindow()
    {
        InitializeComponent();
    }

    public void Bind(OutputWindowViewModel viewModel)
    {
        DataContext = viewModel;
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
        _shown = false;
        base.OnClosed(e);
    }
}
