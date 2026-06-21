# Phase 20 CodeGraph Impact - Live 실제 송출

## Context

CodeGraph로 WPF Live 송출 경로를 확인했다.

- `LiveSessionService.GoLive(...)`는 `LiveSessionSnapshot` 갱신만 수행한다.
- `OutputWindowHost`는 `IOutputWindowService.OutputChanged`와 `ILiveSessionService.SessionChanged`를 구독해 실제 WPF 출력창을 표시한다.
- `MainViewModel.PublishOutputItem(...)` 및 `PublishSelectedItem(...)`가 Live publish의 중심 경로다.
- `OfficePowerPointSlideShowControl`은 기존에 `TriggerNextAsync(...)`만 제공해 PPT Live 시작과 대상 모니터 지정이 분리되어 있었다.

## Affected Symbols

- `Easislides.Wpf.Shell.MainViewModel`
  - `OpenOutput()`
  - `EnsureLiveOutputDisplay()`
  - `PublishOutputItem(...)`
  - `PublishSelectedItem(...)`
  - `StartPowerPointSlideShowForLive(...)`
  - `OnSelectedOutputDisplayChanged(...)`
- `Easislides.Wpf.Rendering.IPowerPointSlideShowControl`
- `Easislides.Wpf.Rendering.OfficePowerPointSlideShowControl`
- `Easislides.Wpf.Interop.OfficePptSession`
- `Easislides.Wpf.Tests.Shell.MainViewModelTests`

## WinForms Evidence

WinForms `Gf.RunPowerpointSong(...)` sets `InPPT.displayName = OutputMonitorName` and calls `PowerPoint.Run(...)`.
`OfficeLib.PowerPoint.Run(...)` writes `HKCU\Software\Microsoft\Office\<version>\PowerPoint\Options\DisplayMonitor` before `SlideShowSettings.Run()`.

## Risk

- Text Live: low to medium. The change only promotes output placement to full-screen at Live publish time.
- PPT Live: medium. COM startup remains asynchronous and best-effort; registry write failure is swallowed to avoid blocking text Live.
- Existing PPT replay: guarded by keeping `TriggerNextAsync(...)` unchanged and adding a separate `StartAsync(...)`.

## Validation Plan

- Focused ViewModel regression tests for text full-screen promotion and PPT slideshow start request.
- Existing PPT replay tests for preview/output trigger behavior.
- Full `Easislides.Wpf.Tests` run.
- `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`.
