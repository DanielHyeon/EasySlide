## Why

운영자가 WinForms에서 쓰던 옵션을 WPF 설정 창에서 같은 순서로 찾을 수 있어야 한다. 두 앱은 서로 다른 설정 저장소를 사용한다.

## What Changes

- WinForms 옵션의 탭과 그룹을 조사하고 WPF 설정 창에 대응시킨다.
- 기존 WPF 전용 기능과 장치 점검 진입점을 보존한다.
- 확인·취소, 메뉴 접근 경로와 편집 가능한 항목을 검증한다.

## Capabilities

### New Capabilities

- `wpf-options-layout` — WinForms 옵션 탭 구성과 WPF 편집 동작.

### Modified Capabilities

## Impact

WPF 설정 창, 설정 서비스, 관련 라이브러리 작업, 회귀 테스트.
