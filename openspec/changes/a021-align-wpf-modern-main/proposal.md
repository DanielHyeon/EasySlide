## Why

현재 WPF 메인은 WinForms와 같은 명령을 다른 아이콘과 부호로 나타낸다. 작업 창의 비율, 빈 송출 화면, 곡 목록의 번호 위치와 도구 모음의 표시 방식도 달라 운영자가 화면을 전환할 때 명령을 다시 찾아야 한다.

## What Changes

현대화한 WinForms FrmMain의 도구 모음, 목록, 미리보기와 송출 머리줄, 하단 조작부와 샘플 화면을 WPF 메인에 반영한다. 대응하는 아이콘은 같은 원본을 사용하고 WPF의 명령과 접근성 이름을 유지한다.

## Capabilities

### New Capabilities

- `modern-operator-console`은 두 데스크톱 화면에서 공통으로 사용하는 시각적 명령과 배치를 정의한다.

### Modified Capabilities

## Impact

WPF MainWindow와 WorshipListPanel, 아이콘 원본, 레이아웃 검증 및 스크린샷 비교에 영향을 준다.
