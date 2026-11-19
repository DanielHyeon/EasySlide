## Why

송출 담당자가 Ctrl+방향키만 켰을 때 슬라이드 이동이 누락되고 검은 화면 키가 다른 프로그램으로 전달된다. 표시와 관계없는 입력도 UI 큐에 쌓인다.

## What Changes

- WinForms와 WPF에서 전역 단축키의 판정과 UI 명령 실행 시점을 분리한다.
- 키보드와 마우스 훅의 네이티브 형식, 해제 실패 처리 및 클릭 횟수를 바로잡는다.

## Capabilities

### New Capabilities

- `global-presentation-input`은 송출 단축키 전달과 전역 입력 훅의 수명을 규정한다.

### Modified Capabilities

## Impact

공유 HookManager, WinForms FrmMain, WPF 입력 서비스와 단축키 레지스트리, 두 테스트 프로젝트가 대상이다.
