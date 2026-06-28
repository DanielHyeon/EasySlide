# 검증 계획

- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~SettingsVisual|FullyQualifiedName~WorshipListPanelTests"`
- `dotnet build EasiSlides.sln`
- 필요 시 `dotnet publish Easislides.Wpf/Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext`
- 앱 실행 후 설정/서식 패널 캡처 저장

## 실행 결과

- `openspec validate a014-wpf-settings-alignment-textbox-visual-fix --strict`: 통과
- `dotnet test Easislides.Wpf.Tests/Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~SettingsVisualLayoutTests" --no-restore`: 통과, 4개 테스트
- `dotnet build Easislides.Wpf/Easislides.Wpf.csproj --no-restore`: 통과, 오류 0개
- `sg scan --rule ast-grep/rules/csharp-risk-patterns.yml Easislides.Wpf/MainWindow.xaml.cs Easislides.Wpf/Shell/MainViewModel.cs`: 위험 패턴 없음
- 실제 앱 실행 캡처:
  - `evidence/screenshots/2026-06-23/a014-settings-alignment-textbox-visual-fix/01-main-window-before-apostles-creed.png`
  - `evidence/screenshots/2026-06-23/a014-settings-alignment-textbox-visual-fix/02-apostles-creed-set-panel.png`
  - `evidence/screenshots/2026-06-23/a014-settings-alignment-textbox-visual-fix/03-apostles-creed-live-show.png`
  - `evidence/screenshots/2026-06-23/a014-settings-alignment-textbox-visual-fix/04-apostles-creed-preview-go-live.png`
- 사도신경 검증 결과:
  - 상단 `LIVE` 토글 실행 캡처(`03-apostles-creed-live-show.png`)에서는 선택된 사도신경이 Live로 전환되지 않고 이전 송출 항목이 유지되는 현상이 확인되었다.
  - 미리보기 패널의 `Preview Go Live` 버튼 실행 캡처(`04-apostles-creed-preview-go-live.png`)에서는 사도신경 항목이 Live 상태로 전환되고 동일한 파란 십자가 배경이 출력 패널에 표시되었다.
  - 다만 출력 패널의 글자 크기/스케일은 미리보기 패널과 눈에 띄게 다르게 보여, “미리보기와 Live Show의 완전한 서식 일치”는 별도 결함으로 계속 추적해야 한다.

## 남은 주의점

- 빌드 중 NetOffice 호환성 및 기존 nullable 경고가 출력되었으나 이번 변경과 직접 관련된 오류는 없다.
- 실제 앱 캡처상 설정 패널의 `Left / Center / Right` 버튼 균등 배치와 `Use Individual Settings` 체크 상태는 확인되었다.
- 사도신경 Live Show의 미리보기/출력 서식 일치 문제는 이번 설정 UI 배치 수정 범위 밖의 잔여 이슈로 남는다.
