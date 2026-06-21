# Phase 22 CodeGraph Impact

## Summary

CodeGraph 컨텍스트 조회 결과, 이번 변경의 구조적 영향은 WPF MainWindow의 Live 명령 조건과 상태 표시 위치에 집중된다.

- `MainViewModel`: `GoLiveCommand`의 실행 가능 조건과 Preview -> Output -> Live 시작 흐름을 소유한다.
- `MainWindow`: Live 상태를 어느 화면 영역에 보여줄지 결정하는 XAML surface이다.
- `OutputWindowService`/`OutputWindowHost`: Live 시작 시 출력 창을 열고 배치하는 기존 경로이며, 이번 Phase에서는 재사용만 한다.
- `OfficePptSession`: PPT 슬라이드쇼 시작 경로이며, Phase 21 변경을 유지한다.
- `FrmMain`: WinForms 기준 흐름 근거이다. `btnToLive_Click`, `PreviewItemToLive`, `GoLive`, `Start_Presentation` 흐름이 WPF UX 판단의 기준이다.

## Impact

- `CanGoLive()`에서 `_output.Current.IsOpen` 조건을 제거하면, Preview 선택 후 Live 버튼을 누르는 WinForms 조작 흐름과 맞아진다.
- 실제 출력 창 열기와 전체 화면 배치는 `PublishSelectedItem()` 내부의 `EnsureLiveOutputDisplay()`가 계속 담당하므로 출력 창 배치 책임은 새로 늘어나지 않는다.
- 상단 `LiveBar`를 숨기고 하단 `StatusBar`에 compact indicator를 추가하면, Live 상태 정보는 유지하면서 frmMain과 다른 별도 빨간 상태 라인을 제거한다.
- `ToggleOutputLiveCommand`의 Output-first 흐름은 기존 테스트와 구현을 유지한다.

## Regression Guards

- 출력 창이 닫힌 상태에서 `GoLiveCommand`가 Preview 항목을 Live로 시작하고 출력 창을 전체 화면으로 여는 테스트를 추가한다.
- `TopLiveBar`가 `Collapsed` 상태임을 XAML 테스트로 고정한다.
- 하단 상태바에 `EsLiveIndicator`, `LiveBar.StateLabel`, `LiveBar.CurrentItemTitle` 바인딩이 존재함을 테스트한다.
- Phase 21의 전체 화면/PPT 즉시 송출 회귀 테스트를 함께 실행한다.
