# Phase 20 - Live 실제 송출 경로 보정

## Goal

WPF `Live` 실행 시 FrmMain과 동일하게 텍스트 항목은 설정된 출력 모니터에 전체화면으로 표시하고, PowerPoint 항목은 설정된 출력 모니터로 PowerPoint 슬라이드쇼를 시작한다.

## Scope

- WPF `MainViewModel`의 `Preview -> Live`, `Output -> Live` publish 경로
- WPF 출력창 배치 상태(`OutputWindowService`)
- WPF PowerPoint 슬라이드쇼 제어 인터페이스와 NetOffice COM 세션
- 관련 ViewModel 회귀 테스트

## Tasks

- Live publish 직전에 출력창을 선택된 출력 모니터의 전체화면 배치로 승격한다.
- Live 중 출력 모니터 선택 변경 시에도 windowed 배치로 되돌아가지 않게 한다.
- PowerPoint Live publish 시 `DisplayMonitor`, `UseAutoMonSelection`, `UseMonMgr` 레지스트리를 WinForms와 같은 위치에 best-effort로 기록한다.
- PowerPoint Live publish 시 `SlideShowSettings.Run()`을 호출하고 현재 슬라이드로 이동한다.
- Preview/Output PPT 썸네일 double-click의 기존 `TriggerNextAsync` 동작은 유지한다.

## DoD

- 텍스트 Go Live는 창이 windowed로 열려 있어도 Live 시 전체화면 출력 상태가 된다.
- PPT Go Live는 선택된 출력 모니터명으로 슬라이드쇼 시작 요청을 수행한다.
- 기존 PPT replay/animation trigger 경로는 회귀하지 않는다.

## Tests

- `GoLiveCommand_WhenOutputWasWindowed_PromotesOutputToFullScreenLikeFrmMain`
- `GoLiveCommand_PowerPoint_StartsSlideShowOnSelectedOutputMonitor`
- `NextOutputSlideCommand_PowerPointLive_MovesRunningSlideShow`
- `ReplayPreviewPowerPointSlideCommand_RequestsPreviewSlideShowNext`
- `ReplayOutputPowerPointSlideCommand_RequestsOutputSlideShowNext`
- `ToggleOutputLiveCommand_WhenOutputWindowIsClosed_StaysEnabledAndOpensOutputLikeFrmMain`

## Constraints

- PowerPoint 레지스트리 쓰기는 WinForms처럼 best-effort로 처리하며, 실패해도 텍스트 Live 송출을 막지 않는다.
- OpenSpec이 파싱에 의존하는 구조 키워드는 변경하지 않는다.
- 이번 Phase는 실제 Live 송출 경로만 다루며 렌더링 스타일/레이아웃 변경과 섞지 않는다.
