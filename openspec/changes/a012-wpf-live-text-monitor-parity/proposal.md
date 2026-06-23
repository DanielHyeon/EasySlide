## Why

WPF Live can report an active live state while the text monitor payload does not match the WinForms Live button behavior. WinForms routes Live through `PreviewItemToLive`, `CopyPreviewToOutput`, `Start_Presentation`, and the launch screen reload path that formats text into the lyrics monitor buffer before display. WPF must produce the same visible text monitor body when Live is clicked.

## What Changes

- Analyze the WinForms Live text-monitor path and the WPF Live text-monitor path with CodeGraph, ast-grep, and tree-sitter-c-sharp.
- Add a focused regression test for the WPF Live button text monitor payload.
- Fix only the WPF path required to make the output window text body match the WinForms Live text monitor behavior.
- Store tool rules/scripts/results in repo-local SDD locations only; tool binaries and Python packages remain developer/CI dependencies.

## Impact

- `Easislides.Wpf/Shell/MainViewModel.cs`
- `Easislides.Wpf/Shell/LiveSessionService.cs`
- `Easislides.Wpf.Tests/Shell/MainViewModelTests.cs`
- `ast-grep/rules/*.yml`
- `openspec/changes/a012-wpf-live-text-monitor-parity/analysis/*.md`

