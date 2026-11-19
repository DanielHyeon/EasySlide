## Context

CodeGraph에서 확인한 HookManager 소스를 WinForms와 WPF가 공유한다. WinForms는 기능키와 미디어에 KeyDown, 방향키에 KeyUp을 사용한다. WPF는 GlobalInputService에서 ShortcutRegistry로 명령을 전달한다.

## Goals / Non-Goals

송출 단축키의 기존 설정과 키별 전달 정책을 유지하면서 콜백 시점에 판정을 끝낸다. 검증에는 합성 입력과 격리된 설정을 사용한다.

## Decisions

- 보조키의 좌우 상태를 구분하고 이벤트에 그 시점의 값을 담는다. F9·F10은 활성 명령을 UI 큐에 등록하는 시점에 소비한다.
- WPF는 등록된 키 조합으로 바인딩을 조회하고 UI에 예약한다. 중지된 세션에 남은 명령은 실행하지 않는다.
- 네이티브 포인터 폭을 따르고 해제 실패 후 재시도할 수 있도록 핸들과 콜백 참조를 유지한다.
- 마우스 버튼을 뗄 때 클릭 이벤트를 한 번 발생시킨다.

## Risks / Trade-offs

UI에 예약한 명령은 이후 실행 중 실패할 수 있다. 입력 소비 여부는 예약할 때 결정하며 비동기 실행 오류는 별도로 기록한다.
