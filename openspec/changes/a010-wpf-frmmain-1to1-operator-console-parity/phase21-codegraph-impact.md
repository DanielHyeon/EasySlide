# Phase 21 CodeGraph Impact

## Summary

CodeGraph 컨텍스트 조회 결과, 이번 변경의 핵심 진입점은 다음 구조로 확인했다.

- `MainViewModel`: Live/Black 명령과 PowerPoint 시작 요청의 소유자
- `OutputWindowService`/`OutputWindowHost`: 출력 창 상태와 실제 WPF surface 적용 경로
- `OutputWindow`: 실제 전체화면 위치, 스타일, 활성화 적용 지점
- `OfficePptSession`: PowerPoint COM 슬라이드쇼 시작 및 모니터 레지스트리 저장 지점
- `DisplayService`/`OutputDisplay`: `Id`가 WinForms `Screen.DeviceName` 기준 장치명, `Name`은 UI 표시명

## Impact

- Live 시작 프롬프트 제거는 `GoLiveCommand`, `ToggleOutputLiveCommand`, `SendToOutputAndNextCommand` 사용성에 영향을 준다.
- Black 즉시 실행은 `BlackScreenCommand`, `ToggleOutputBlackCommand`, F9 단축키 흐름에 영향을 준다.
- `OutputWindow.ApplyPlacement` 변경은 출력 창과 스테이지가 아닌 실제 Output surface에만 적용된다.
- PowerPoint 모니터명 변경은 WPF PPT Live 송출 시작과 output slide navigation 시 슬라이드쇼 위치에 영향을 준다.

## Regression Guards

- Stop/Close/Clear의 기존 안전 확인 테스트는 유지한다.
- Live/Black은 프롬프트가 호출되지 않는 테스트로 frmMain 즉시 조작감을 고정한다.
- PPT 시작 요청은 `OutputDisplay.Name`이 아니라 `OutputDisplay.Id`를 기대하도록 테스트를 갱신한다.
