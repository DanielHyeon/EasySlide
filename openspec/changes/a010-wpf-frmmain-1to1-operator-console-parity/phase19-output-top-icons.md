# Phase 19: Output 상단 라이브 아이콘 활성/동작 정합

## Goal

오른쪽 Output 상단 `Black`, `Clear/Tx`, `LIVE` 아이콘이 WinForms frmMain 사용 흐름처럼 준비/라이브 상태에서 올바르게 활성화되고 실제 조작 가능한지 보장한다.

## Scope

- `Easislides.Wpf/Shell/MainViewModel.cs`
- `Easislides.Wpf/Shell/LiveSafetyPrompt.cs`
- `Easislides.Wpf/Composites/SafetyConfirm.cs`
- `Easislides.Wpf.Tests/Shell/MainViewModelTests.cs`
- `evidence/screenshots/2026-06-21/phase19-output-top-icons/`

## Tasks

- [x] 배포본 UIA로 재현 확인: Output 준비 후 `cbOutputBlack`, `cbOutputClear`, `cbGoLive`가 모두 비활성처럼 보이는 흐름을 확인했다.
- [x] `cbGoLive`는 Output 창이 아직 열리지 않았더라도 준비된 Output 항목이 있으면 누를 수 있도록 `CanStartOutputLive` 조건에서 Output 창 open 선행조건을 제거했다.
- [x] `cbGoLive` 실행 시 필요한 경우 Output 창을 먼저 열고 기존 Output 항목을 라이브로 publish하도록 보강했다.
- [x] Live 상태 진입 후 `Black/Clear` 토글 명령이 WinForms처럼 즉시 활성화되는지 `CanExecute` 회귀 테스트를 추가했다.
- [x] 안전 확인 Popup이 MainWindow 하단 화면 밖으로 배치될 수 있는 문제를 막기 위해 현재 마우스/키보드 포커스 요소를 anchor로 우선 사용하고, Popup 배치를 화면 안에 남는 중앙 배치로 변경했다.
- [x] 안전 확인 확인/취소 버튼에 UIA 자동화 이름을 부여해 다음 수동 UAT/자동 캡처에서 직접 조작할 수 있게 했다.

## DoD

- Output 준비 상태에서는 `LIVE`가 활성이고, `Black/Clear`는 실제 라이브 전 안전 동작으로 비활성 상태를 유지한다.
- `LIVE` 실행 후 Live 상태에서는 `Black/Clear`가 활성 명령으로 전환된다.
- 안전 확인 카드가 화면 밖으로 사라지지 않고 사용자가 확인/취소할 수 있는 위치에 표시된다.
- focused tests, full WPF tests, WinForms build, OpenSpec strict validation, Release publish, 배포본 캡처가 통과한다.

## Tests

- `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~ToggleOutputLiveCommand|FullyQualifiedName~OutputSafetyToggleCommands|FullyQualifiedName~BindShortcuts_LiveSafetyKeysUseFrmMainCheckedToggleSemantics|FullyQualifiedName~MainMenuBarTests" --no-restore -v minimal`
- `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~ToggleOutputLiveCommand_WhenOutputWindowIsClosed_StaysEnabledAndOpensOutputLikeFrmMain|FullyQualifiedName~OutputSafetyToggleCommands|FullyQualifiedName~SafetyConfirmTests" --no-restore -v minimal`
- `dotnet test Easislides.Wpf.Tests --no-restore -v minimal`
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal`
- `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`
- `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal`

## Evidence

- 2026-06-21 CodeGraph context 확인: `StartOutputLiveAsync`, `CanStartOutputLive`, `ToggleOutputBlackCommand`, `ToggleOutputClearCommand`, `WpfLiveSafetyPrompt`, `SafetyConfirm` 영향으로 한정했다.
- 2026-06-21 UIA 재현: pre-fix 배포본에서 `cbOutputBlack=False`, `cbOutputClear=False`, `cbGoLive=False`였고, 수정 후 Output 준비 상태에서 `cbGoLive=True`가 되는 상태를 확인했다.
- 2026-06-21 focused tests 통과. 결과: 실패 0, 통과 140, 건너뜀 0.
- 2026-06-21 focused safety tests 통과. 결과: 실패 0, 통과 4, 건너뜀 0.
- 2026-06-21 full WPF tests 통과. 결과: 실패 0, 통과 2427, 건너뜀 0.
- 2026-06-21 WinForms build 통과. 결과: 오류 0, 경고 13.
- 2026-06-21 OpenSpec strict validation 통과.
- 2026-06-21 Release 배포 통과: `C:\EasiSlides\EasislidesNext`.
- 2026-06-21 배포본 캡처: `evidence/screenshots/2026-06-21/phase19-output-top-icons/01-after-publish-output-prepared.png`, `04-enter-confirm-after-prepare.png`, `07-foreground-after-prepare.png`, `14-fixed-after-prepare.png`, `15-fixed-golive-safety-card.png`, `16-fixed-after-safety-timeout.png`.
