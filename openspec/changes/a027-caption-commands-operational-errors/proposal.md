## Why

자막 콘솔은 WPF 입력 이벤트에서 초안, 송출 이력, 자동 저장을 함께 변경한다. 운영 빌드의 전역 예외 처리는 디버그 출력과 모달 알림에 의존해 장애 원인을 남기기 어렵다.

## What Changes

- 직접 입력 자막의 편집·송출·숨김을 바인딩 가능한 커맨드로 옮기고 한글 조합과 키 반복을 검증한다.
- 자막 PPT 읽기 오류를 명령에서 안내하고 Release에서 읽을 수 있는 오류 기록을 남긴다. 처리되지 않은 UI 오류는 WPF의 종료 경로를 따른다.

## Capabilities

### New Capabilities

- `wpf-operational-errors` — 작업 실패와 미처리 예외의 기록 및 처리 정책.

### Modified Capabilities

자막 동작은 기존 `a023-live-camera-captions` 계약을 유지한다.

## Impact

WPF 자막 콘솔, 앱 시작·종료 시의 예외 핸들러, WPF 회귀 테스트에 적용한다.
