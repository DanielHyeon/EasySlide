## 왜 필요한가

현재 WPF 셸은 아직 사용할 수 있는 FrmMain 대체물이기보다, 미완성 기능을 감싼 프레임처럼 느껴진다. 최근 pane-role 작업으로 큰 영역 배치는 드러났지만, 예배 중 운영자는 여전히 legacy FrmMain의 사용성을 만드는 좌상단 소스 브라우저, 좌하단 Worship List/Praise Book, 성경 검색, 드래그 앤 드롭, 우측 Preview/Output 흐름을 같은 방식으로 신뢰할 수 없다.

이 change는 추가 시각 현대화보다 먼저, 마이그레이션 목표를 엄격한 FrmMain 1:1 운영 콘솔 동등성으로 되돌린다.

## 변경 내용

- legacy `FrmMain`의 소스 control 이름, event handler, data loading method를 기준으로 control/event inventory를 확립한다.
- 각 주요 영역을 `implemented`, `partial`, `missing`, `intentionally deferred`로 표시하는 FrmMain-to-WPF 1:1 mapping table을 확립한다.
- WPF `MainWindow`가 legacy 운영 콘솔 계층을 반영하도록 요구한다:
  - `splitContainerMain`
  - `splitContainer1`
  - `tabControlSource`
  - `tabControlLists`
  - `splitContainer2`
  - `splitContainerPreview`
  - `splitContainerOutput`
- WPF main shell이 다음 실제 기능을 복구하도록 요구한다:
  - Folders/Songs source browsing
  - Worship List load/edit/reorder
  - Praise Book inline list
  - Bible lookup and selected-passage add
  - InfoScreen, PowerPoint, Images, Media source tabs
  - Preview와 Output을 서로 다른 운영 대상(target)으로 유지
  - double-click, Enter, context menu, drag-and-drop, keyboard shortcuts
- legacy 운영자 시나리오 기반 manual UAT checklist를 추가한다.

## 비목표

- 동등성이 달성되기 전에 FrmMain을 새로운 workflow로 재설계하지 않는다.
- legacy path, `.bak` 파일, 기존 WinForms 동작을 제거하지 않는다.
- database schema를 변경하지 않는다.
- layout-only 작업만으로 완료를 주장하지 않는다.

## 영향

- 주요 WPF 파일: `Easislides.Wpf/MainWindow.xaml`, `MainWindow.xaml.cs`, WPF composites, 관련 view models/services.
- legacy reference files: `Easislides/Easislides/FrmMain.*`, `KeyboardActionHandler.cs`.
- 문서: `docs/wpf-migration/inventory/*`와 1:1 mapping plan.
- 검증: OpenSpec validation, WPF tests, WinForms build, `C:\EasiSlides` 기준 manual UAT.
