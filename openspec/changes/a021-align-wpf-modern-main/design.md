## Context

WinForms FrmMain의 ModernUi, Icons, ImageList partial과 WPF MainWindow 및 WorshipListPanel을 대조했다. WinForms는 SVG 경로를 크기와 스타일로 선택하지만 WPF는 폰트 심벌과 문자열을 섞어서 표시한다.

## Goals / Non-Goals

두 화면의 도구 식별, 조작 순서, 작업 공간의 비율을 일치시킨다. WPF의 선택 상태와 명령이 계속 실행되도록 기존 컨트롤의 바인딩을 유지한다.

## Decisions

같은 벡터 경로를 WPF와 WinForms에서 참조한다. WPF는 뷰박스 크기와 채움 규칙을 지켜 현재 배율에서 그린다. 색상과 선택 상태는 WPF 리소스와 트리거로 표현한다. 좁은 도구 모음에서는 글자를 접고 검색 입력과 버튼 접근을 유지한다.

## Risks / Trade-offs

서로 다른 프레임워크가 글꼴과 창 장식을 그리므로 화면 검증은 동일한 클라이언트 크기에서 배치, 아이콘, 상태와 가시성을 비교한다.
