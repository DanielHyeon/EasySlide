# CodeGraph Impact

## Summary

CodeGraph 기준 이번 change의 핵심 영향 심볼은 `OutputWindowHost`이다.

- `OutputWindowHost`: `IOutputWindowService`와 `ILiveSessionService`를 구독해 실제 출력 surface를 만들고 세션을 반영한다.
- `IOutputSurface`: 실제 `OutputWindow`와 테스트 fake surface가 구현하는 출력 창 추상화이다.
- `OutputWindow`: WPF 회중 송출 창이며, WPF `Window.Closed` 이벤트를 통해 외부 닫힘을 알릴 수 있다.
- `OutputWindowService`: 출력 창 상태(`IsOpen`, display, placement)를 소유한다.
- `LiveSessionService`: Live payload와 상태를 알리고 host가 이를 Output surface에 반영한다.

## Root Cause

WPF 출력 창을 사용자가 창 닫기(X/Alt+F4 등)로 직접 닫으면 실제 surface는 사라지지만 `OutputWindowService.Current.IsOpen`은 계속 true로 남을 수 있었다. 이 상태에서 다시 Live를 누르면 `EnsureLiveOutputDisplay()`가 출력 창이 이미 열려 있다고 판단해 `Open()`을 호출하지 않고, host도 새 surface를 만들지 않는다.

그 결과 내부 Live 상태와 상태바는 갱신되어도 실제 회중 출력 창은 다시 열리지 않는 “Live가 안됨” 상태가 발생할 수 있다.

## Impact

- `IOutputSurface`에 `Closed` 이벤트를 추가해 실제 surface 닫힘을 host가 감지한다.
- `OutputWindowHost`는 외부 닫힘을 감지하면 내부 surface/view model 참조를 정리하고 `OutputWindowService.Close()`로 서비스 상태를 닫힘으로 동기화한다.
- 서비스가 닫힘 상태가 되면 다음 Live 시작 시 기존 `EnsureLiveOutputDisplay()`가 다시 `Open(display, windowed:false)`를 호출해 새 출력 창을 만들 수 있다.
- 서비스 주도 `Close()`와 외부 닫힘 이벤트가 재진입하지 않도록 `_closingSurfaceFromService` 가드를 둔다.

## Regression Guards

- `ExternalSurfaceClose_MarksServiceClosedAndAllowsLiveReopen`
  - 외부 surface 닫힘 후 `OutputWindowService.Current.IsOpen == false`가 되는지 확인한다.
  - 같은 display로 다시 output open/live session을 시작하면 새 surface가 만들어지고 Live payload가 반영되는지 확인한다.
- 기존 Preview Live, Output Live, PPT Live 집중 테스트를 함께 실행해 WinForms 흐름의 주요 경로를 유지한다.
