using System;
using Easislides.Wpf.Controls;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 스테이지(Preview) 모니터용 세션 변환 — 회중 화면이 검정(Black)·비움(Clear)·숨김이어도
/// 워십 리더·밴드가 가사를 계속 보도록, <see cref="LiveState.Hidden"/> 상태를 <see cref="LiveState.Active"/> 로 되돌린다
/// (LiveSessionService.Restore 와 같은 변환).
///
/// 핵심: HideOutput/ClearOutput 은 <c>Current with { State=Hidden, ... }</c> 라 가사 본문 등 콘텐츠 필드를 **그대로 보존**한다.
/// 그래서 상태만 Active 로 되돌리면 재사용하는 출력 VM(OutputWindowViewModel)이 직전 가사를 다시 그린다 — 별도 스테이지 VM 불필요.
/// Off(정지·라이브 없음)는 보여줄 콘텐츠가 없으므로 그대로 둔다. 순수 함수(부작용 0) — 테스트하기 쉽다.
/// </summary>
public static class StageSessionNormalizer
{
    public static LiveSessionSnapshot Normalize(LiveSessionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        // 회중 화면만 가리는 Hidden(검정/비움/숨김)을 Active 로 되돌린다 — 스테이지는 계속 가사를 보여 준다.
        if (snapshot.State == LiveState.Hidden)
        {
            return snapshot with { State = LiveState.Active, IsBlackout = false, IsCleared = false };
        }

        // Active(이미 표시 중)·Off(보여줄 것 없음)는 변환이 필요 없다.
        return snapshot;
    }
}
