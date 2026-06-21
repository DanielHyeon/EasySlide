# Phase 21 - Live 즉시 송출, 전체화면, PPT 슬라이드쇼 보정

## Goal

WPF MainWindow의 Live 송출 동작을 frmMain 사용성과 맞춘다.

- Live 버튼 클릭 시 메시지 박스 없이 즉시 송출한다.
- Black 화면 전환은 질문 없이 즉시 실행한다.
- 텍스트/성경/가사 송출 창은 Live 진입 시 선택 출력 모니터 전체화면으로 전환한다.
- PPT 항목 Live 진입 시 PowerPoint 슬라이드쇼가 선택 출력 모니터에서 시작한다.

## Scope

- `MainViewModel` Live/Black 명령 흐름
- `OutputWindow` 전체화면 배치 적용
- `OfficePptSession` PowerPoint 슬라이드쇼 시작 안정화
- WPF 단위 테스트 기대값 갱신

## Tasks

- Live 시작 계열(`GoLive`, `ToggleOutputLive`, `SendToOutputAndNext`)에서 안전 확인 프롬프트를 제거한다.
- Black 화면 명령에서 안전 확인 프롬프트를 제거한다.
- Live 송출 시 `OutputDisplay.Id`를 PowerPoint 모니터 레지스트리 값으로 사용한다.
- 이미 표시된 WPF 송출 창도 전체화면 배치 시 `WindowState`, borderless 스타일, 활성화를 다시 적용한다.
- PowerPoint 슬라이드쇼 창 판정 시 실제 `View` 접근까지 확인하고, 없으면 `SlideShowSettings.Run()`을 수행한다.

## DoD

- Live 버튼 클릭 시 `ILiveSafetyPrompt` 호출이 없어야 한다.
- Black 화면 클릭 시 `ILiveSafetyPrompt` 호출이 없어야 한다.
- windowed 상태로 열린 Output 창도 Live 진입 시 fullscreen placement로 바뀌어야 한다.
- PPT Live 송출은 표시명 대신 `\\.\DISPLAYx` 계열 장치 ID를 슬라이드쇼 대상 모니터로 전달해야 한다.
- 기존 Stop/Close/Clear 안전 확인은 유지한다.

## Tests

- `dotnet test Easislides.Wpf.Tests --no-restore --filter "FullyQualifiedName~MainViewModelTests" -v minimal`
- `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal`
- `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -v minimal`
- `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`

## Constraints

- Live 시작 외의 종료/닫기 계열 안전 가드는 임의로 제거하지 않는다.
- PowerPoint COM 예외는 기존처럼 송출 앱을 죽이지 않도록 격리한다.
- OpenSpec 구조 키워드는 파서 호환을 위해 보수적으로 유지한다.
