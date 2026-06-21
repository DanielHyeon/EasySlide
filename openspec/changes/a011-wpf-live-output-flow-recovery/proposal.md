## Why

WPF MainWindow에서 Live 버튼을 눌러도 실제 회중 송출이 시작되지 않는 문제가 남아 있다. 예배 운영 중 Live는 가장 핵심적인 즉시 송출 동작이므로, 기존 WinForms `FrmMain` 흐름을 기준으로 별도 change에서 원인을 분리하고 복구한다.

## What Changes

- WPF Live 버튼/명령이 WinForms `btnToLive_Click` 및 `cbGoLive_Click` 흐름과 같은 사용자 결과를 내도록 수정한다.
- Preview 항목이 선택된 상태에서 Live를 누르면 출력 창을 자동으로 준비하고 선택 항목을 실제 Output surface에 송출한다.
- PPT 항목의 경우 PowerPoint SlideShow가 설정된 출력 모니터에서 실행되도록 검증하고 필요한 경로를 보정한다.
- Live 시작 시 메시지 박스나 별도 확인 질문 없이 즉시 실행되는 조건을 유지한다.
- Live 상태 표시는 별도 상단 라인이 아니라 상태바/아이콘 상태에 남긴다.

## Capabilities

### New Capabilities

- `wpf-live-output-flow`: WPF MainWindow Live 버튼이 WinForms FrmMain과 동일하게 Preview/Output 항목을 실제 회중 출력으로 전환하고, 텍스트/PPT 송출을 시작하는 동작.

### Modified Capabilities

- 없음

## Impact

- `Easislides.Wpf/Shell/MainViewModel.cs`: Live 명령, Preview -> Output publish, 상태 갱신.
- `Easislides.Wpf/Shell/OutputWindowHost.cs`, `OutputWindowService`, `OutputWindowViewModel`: 실제 Output surface 갱신 및 full-screen host 상태.
- `Easislides.Wpf/Services/Presentation/OfficePptSession.cs`: PPT SlideShow 시작/모니터 지정 경로.
- `Easislides.Wpf/MainWindow.xaml`: Live 버튼 및 상태바 UX.
- `Easislides.Wpf.Tests`: Live flow 회귀 테스트, PPT/텍스트 송출 경로 테스트, XAML 상태 표시 테스트.
- 외부 영향: `C:\EasiSlides` 실제 데이터, `HKCU\Software\EasiSlides` 설정, PowerPoint COM/멀티모니터 송출.
