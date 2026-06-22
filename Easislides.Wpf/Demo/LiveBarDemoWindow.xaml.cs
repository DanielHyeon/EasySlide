using System;
using System.Windows;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Controls;

namespace Easislides.Wpf.Demo;

public partial class LiveBarDemoWindow : Window
{
    private readonly LiveBarViewModel _vm;

    public LiveBarDemoWindow()
    {
        InitializeComponent();
        _vm = new LiveBarViewModel
        {
            State = LiveState.Off,
            CurrentItemTitle = "주일찬양 #3 \"은혜로다\"",
            OutputMonitorName = "모니터 2",
            ShortcutHint = "Space=다음 슬라이드",
        };
        LiveBarControl.ViewModel = _vm;
    }

    private void OnOff_Click(object sender, RoutedEventArgs e) => _vm.State = LiveState.Off;
    private void OnStandby_Click(object sender, RoutedEventArgs e) => _vm.State = LiveState.Standby;
    private void OnLive_Click(object sender, RoutedEventArgs e) => _vm.State = LiveState.Active;
    private void OnHidden_Click(object sender, RoutedEventArgs e) => _vm.State = LiveState.Hidden;

    private async void OnDangerAction_Click(object sender, RoutedEventArgs e)
    {
        ResultText.Text = "(확인 대기 중...)";
        var ok = await SafetyConfirm.AskAsync(
            BtnDanger,
            "지금 라이브 송출을 검정으로 전환하시겠습니까?",
            "5초 안에 확인 안 하면 자동 취소됩니다.");
        ResultText.Text = ok
            ? $"✓ 확인됨 → State=Hidden 적용 ({DateTime.Now:HH:mm:ss})"
            : $"✗ 취소됨 ({DateTime.Now:HH:mm:ss})";
        if (ok) _vm.State = LiveState.Hidden;
    }
}
