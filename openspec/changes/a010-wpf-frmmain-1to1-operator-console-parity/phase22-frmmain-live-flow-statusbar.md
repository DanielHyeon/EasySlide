# Phase 22 - FrmMain Live 흐름 및 상태바 파리티

## 목적

사용자 피드백 기준으로 WPF MainWindow의 Live 동작을 WinForms `FrmMain` 흐름에 다시 맞춘다.

- Live 클릭 시 별도 빨간 상태 라인을 상단에 노출하지 않는다.
- Live 상태는 WinForms처럼 조작 아이콘/하단 상태바 맥락 안에서 표시한다.
- Live 시작은 출력 창을 사용자가 먼저 열어야만 가능한 동작이 아니라, `FrmMain`처럼 선택된 Preview 항목을 Output으로 복사하고 쇼를 바로 시작하는 흐름이어야 한다.

## WinForms 흐름 근거

WinForms `FrmMain`의 핵심 흐름은 다음과 같다.

- `btnToLive_Click` -> `PreviewItemToLive()`
- `PreviewItemToLive()`는 쇼가 실행 중이면 Preview를 Output으로 복사하고, 쇼가 실행 중이 아니면 Preview를 Output으로 복사한 뒤 `GoLive(true)`를 호출한다.
- `cbGoLive_Click` -> `GoLive(cbGoLive.Checked)`
- `GoLive(true)` -> `Start_Presentation()`
- `Start_Presentation()`은 Output 항목이 없으면 Worship List 첫 항목을 Output으로 로드한 뒤 송출을 시작한다.
- Live 상태는 별도 상단 라인이 아니라 `cbGoLive`, worship list 아이콘, `statusStripMain` 같은 기존 조작/상태 영역에서 표현된다.

## 변경 범위

- `Easislides.Wpf/Shell/MainViewModel.cs`
  - `GoLiveCommand` 실행 가능 조건에서 "출력 창이 이미 열려 있어야 함" 조건을 제거한다.
  - 실제 Live 시작 시에는 기존 `PublishSelectedItem()` -> `EnsureLiveOutputDisplay()` 흐름이 출력 창을 전체 화면으로 연다.
- `Easislides.Wpf/MainWindow.xaml`
  - 상단 `LiveBar`는 보이지 않게 유지한다.
  - 하단 `StatusBar`에 작은 Live indicator와 상태 텍스트, 현재 Live 항목 제목을 배치한다.
- `Easislides.Wpf.Tests`
  - 출력 창이 닫힌 상태에서도 Preview 항목의 Live 시작이 가능하고 출력 창이 전체 화면으로 열리는 회귀 테스트를 추가한다.
  - 상단 LiveBar 비노출 및 하단 상태바 Live indicator 배치를 XAML 구조 테스트로 고정한다.

## 완료 조건

- WPF Live 버튼은 출력 창을 먼저 열라는 UX 없이 실행 가능해야 한다.
- Live 실행 시 메시지 박스가 없어야 한다.
- Live 실행 시 출력 창은 전체 화면 송출 경로로 열린다.
- 상단 빨간 Live 상태 라인은 보이지 않아야 한다.
- Live 상태는 하단 상태바의 아이콘 상태로 확인 가능해야 한다.

## 검증

- `dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~GoLiveCommand_WhenOutputClosed_OpensOutputAndStartsPreviewLikeFrmMain|FullyQualifiedName~MainStatusBarTests|FullyQualifiedName~GoLiveCommand_WhenOutputWasWindowed|FullyQualifiedName~ToggleOutputLiveCommand_WhenOutputWindowIsClosed|FullyQualifiedName~ToggleOutputLiveCommand_WhenNoPreparedOutput" -v minimal`
- 전체 WPF 테스트 및 OpenSpec strict validate는 Phase 완료 게이트에서 수행한다.
