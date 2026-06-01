using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 공지 화면(InfoScreen) 편집 뷰모델(레거시 FrmInfoScreen 대응) — 자유 텍스트 안내를 입력해
/// 회중 출력으로 송출한다. 송출은 주입된 콜백(=MainViewModel.PublishNotice)에 위임하고,
/// 콜백이 false(출력 미개방 등)면 안내 문구를 보여 준다.
/// </summary>
public sealed partial class NoticeScreenViewModel : ObservableObject
{
    private readonly Func<string, int, bool> _publish;
    private readonly Action _clear;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    // 공지 글자 크기(pt). 출력에 47=pt FormatData 로 실려 큰 글씨로 송출. 기본 40(보통).
    [ObservableProperty]
    private int _fontSizePt = 40;

    public NoticeScreenViewModel(Func<string, int, bool> publish, Action clear)
    {
        _publish = publish ?? throw new ArgumentNullException(nameof(publish));
        _clear = clear ?? throw new ArgumentNullException(nameof(clear));
        SendCommand = new RelayCommand(Send, () => !string.IsNullOrWhiteSpace(Text));
        ClearCommand = new RelayCommand(Clear);
    }

    /// <summary>글자 크기 프리셋(pt) — 콤보 바인딩용(보통 40 / 크게 60 / 아주 크게 80).</summary>
    public IReadOnlyList<int> FontSizePresets { get; } = new[] { 40, 60, 80 };

    public IRelayCommand SendCommand { get; }

    public IRelayCommand ClearCommand { get; }

    // 입력한 공지 텍스트를 출력으로 송출. 출력 창이 닫혀 있으면 콜백이 false → 안내.
    private void Send()
    {
        var ok = _publish(Text, FontSizePt);
        StatusText = ok
            ? "공지를 출력에 송출했습니다."
            : "출력 창이 열려 있지 않습니다. 먼저 출력을 여세요.";
    }

    // 송출한 공지를 내린다(출력을 검은 화면으로). 출력으로 전달은 콜백에 위임.
    private void Clear()
    {
        _clear();
        StatusText = "공지를 내렸습니다(검은 화면).";
    }
}
