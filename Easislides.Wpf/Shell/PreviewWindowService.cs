using System;
using Easislides.Wpf.Platform;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 스테이지(Preview) 모니터 창의 상태 — 출력 창과 같은 지오메트리 레코드(OutputDisplay/OutputWindowPlacement)를 공유한다.
/// 회중용 출력과 독립적으로 열고/옮기고/닫을 수 있도록 별도 상태 타입을 둔다(이름 명료 + 출력 경로 무간섭).
/// </summary>
public sealed record PreviewWindowState(
    bool IsOpen,
    OutputDisplay? Display,
    OutputWindowPlacement Placement)
{
    public static PreviewWindowState Closed { get; } = new(
        IsOpen: false,
        Display: null,
        new OutputWindowPlacement(0, 0, 0, 0, IsWindowed: true));
}

public sealed class PreviewWindowChangedEventArgs : EventArgs
{
    public PreviewWindowChangedEventArgs(PreviewWindowState state) => State = state;

    public PreviewWindowState State { get; }
}

/// <summary>
/// 스테이지(Preview) 모니터 창 상태머신 — 출력 창(IOutputWindowService)과 같은 구조의 병렬 서비스.
/// 회중용 출력과 별개로, 워십 리더·밴드가 보는 확인/스테이지 모니터를 선택한 모니터에 띄운다.
/// 상태만 관리하고, 실제 창 생성·렌더는 PreviewWindowHost 가 이 상태 변화를 구독해 처리한다(출력과 동일 패턴).
/// </summary>
public interface IPreviewWindowService
{
    event EventHandler<PreviewWindowChangedEventArgs>? PreviewChanged;

    PreviewWindowState Current { get; }

    void Open(OutputDisplay display, bool windowed);
    void MoveTo(OutputDisplay display, bool windowed);
    void Close();
}

public sealed class PreviewWindowService : IPreviewWindowService
{
    private readonly IWindowPlacementService _placement;

    public PreviewWindowService()
        : this(new WindowPlacementService())
    {
    }

    public PreviewWindowService(IWindowPlacementService placement)
    {
        _placement = placement;
    }

    public event EventHandler<PreviewWindowChangedEventArgs>? PreviewChanged;

    public PreviewWindowState Current { get; private set; } = PreviewWindowState.Closed;

    public void Open(OutputDisplay display, bool windowed) => Update(CreateState(display, windowed));

    public void MoveTo(OutputDisplay display, bool windowed) => Update(CreateState(display, windowed));

    public void Close() => Update(PreviewWindowState.Closed);

    private PreviewWindowState CreateState(OutputDisplay display, bool windowed)
    {
        ArgumentNullException.ThrowIfNull(display);
        // 모니터 풀스크린(또는 창모드) 배치는 출력과 동일 계산을 재사용한다(같은 "모니터에 꽉 채우기" 의미).
        return new PreviewWindowState(
            IsOpen: true,
            display,
            _placement.CreateOutputPlacement(display, windowed));
    }

    private void Update(PreviewWindowState state)
    {
        // 같은 상태로의 갱신은 무시 — 불필요한 창 재배치·이벤트를 막는다(출력 서비스와 동일 가드).
        if (state == Current)
        {
            return;
        }

        Current = state;
        PreviewChanged?.Invoke(this, new PreviewWindowChangedEventArgs(state));
    }
}
