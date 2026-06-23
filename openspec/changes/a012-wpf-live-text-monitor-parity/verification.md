# Verification

## SDD Exception Notice

The Apostles Creed selected-page and appearance-parity runtime fix was deployed before the required function-internal SDD Step 2 artifacts were completed for that sub-scope.

The user approved keeping the deployed build as a temporary operational hotfix exception on 2026-06-23. This verification file records runtime/test evidence only; it must not be read as proof that the original SDD ordering was followed.

See `sdd-exception.md` for the violation, root cause, and cross-session prevention controls.

## Completed

```powershell
openspec validate a012-wpf-live-text-monitor-parity --strict
```

Outcome: pass.

```powershell
ast-grep --version
```

Outcome: `ast-grep 0.44.0`.

```powershell
python tools\logic_map\extract_function_ast.py --help
```

Outcome: pass.

```powershell
python tools\logic_map\extract_function_ast.py --file Easislides.Wpf\Shell\MainViewModel.cs --method MainViewModel.ResolveLiveProjection --out openspec\changes\a012-wpf-live-text-monitor-parity\analysis\function-ast-summary.md
```

Outcome: pass.

```powershell
python tools\logic_map\extract_function_ast.py --file Easislides\Easislides\FrmMain.cs --method FrmMain.PreviewItemToLive --out openspec\changes\a012-wpf-live-text-monitor-parity\analysis\function-ast-summary-winform-preview.md
```

Outcome: pass.

```powershell
python tools\logic_map\extract_function_ast.py --file Easislides\Easislides\FrmMain.Logic.cs --method FrmMain.CopyPreviewToOutput --out openspec\changes\a012-wpf-live-text-monitor-parity\analysis\function-ast-summary-winform-copy.md
```

Outcome: pass.

```powershell
ast-grep scan --rule ast-grep\rules\csharp-risk-patterns.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf\Shell\LiveSessionService.cs Easislides.Wpf\Shell\OutputWindowHost.cs Easislides.Wpf\Shell\OutputWindowViewModel.cs Easislides\Easislides\FrmMain.Logic.cs
```

Outcome: pass with one existing WinForms warning at `Easislides/Easislides/FrmMain.Logic.cs:1176`; no WPF implementation-boundary findings.

```powershell
ast-grep scan --rule ast-grep\rules\sdd-invariants.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf\Shell\LiveSessionService.cs Easislides.Wpf\Shell\OutputWindowHost.cs Easislides.Wpf\Shell\OutputWindowViewModel.cs
```

Outcome: pass with 10 invariant checkpoints.

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~GoLiveCommand_WhenOutputClosed_UpdatesOutputWindowTextMonitorBody|FullyQualifiedName~GoLiveCommand_WithDatabaseSongMissingLyrics_ReloadsTextMonitorBodyLikeWinForms" --no-restore -v minimal
```

Outcome: pass, 2 tests.

The focused test command also built the affected WPF test project and dependencies successfully.

## Deployment

```powershell
dotnet build Easislides.sln -c Release -nologo -v minimal
```

Outcome: pass with existing warnings.

```powershell
dotnet test Easislides.sln -c Release --no-build -nologo -v minimal
```

Outcome: pass, 20 analyzer tests and 2449 WPF tests.

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal
```

Outcome: pass. Deployed WPF Release output to `C:\EasiSlides\EasislidesNext`.

Deployment artifact check:

- `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
- File count: 42
- `EasislidesNext.exe` LastWriteTime: `2026-06-23 19:50:54`

## Post-Deploy Monitor Fix

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~OpenOutputCommand_WhenAlwaysUseSecondaryMonitorIgnoresStoredPrimaryMonitor|FullyQualifiedName~OpenOutputCommand_UsesDefaultOutputMonitorFromSettings|FullyQualifiedName~OpenOutputCommand_WhenAlwaysUseSecondaryMonitorDisabledWithoutDefault_SelectsPrimary|FullyQualifiedName~OpenOutputCommand_WhenDefaultMonitorMissingAndAlwaysUseSecondaryDisabled_FallsBackToPrimary" --no-restore -v minimal
```

Outcome: pass, 4 tests.

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~GoLiveCommand_WhenOutputClosed_UpdatesOutputWindowTextMonitorBody|FullyQualifiedName~GoLiveCommand_WithDatabaseSongMissingLyrics_ReloadsTextMonitorBodyLikeWinForms" --no-restore -v minimal
```

Outcome: pass, 2 tests.

```powershell
openspec validate a012-wpf-live-text-monitor-parity --strict
```

Outcome: pass.

```powershell
dotnet build Easislides.sln -c Release -nologo -v minimal
```

Outcome: pass with existing warnings.

```powershell
dotnet test Easislides.sln -c Release --no-build -nologo -v minimal
```

Outcome: pass, 20 analyzer tests and 2450 WPF tests.

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal
```

Outcome: pass. Re-deployed WPF Release output to `C:\EasiSlides\EasislidesNext`.

Deployment artifact check:

- `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`
- `EasislidesNext.exe` LastWriteTime: `2026-06-23 19:58:56`

Runtime setting check:

- `C:\Users\Admin\AppData\Roaming\EasislidesNext\settings.json`
- `LiveOutput.DefaultOutputMonitorId=\\.\DISPLAY1`
- `LiveOutput.DisplayAlwaysUseSecondaryMonitor=true`

## Actual Live Show Runtime Capture

First actual-run check after deployment:

- Executed `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`.
- Clicked the WPF `Preview Go Live` button through UI Automation.
- Captured full virtual desktop and both monitors.
- Before the placement fix, `EasiSlides Output` existed but UI Automation reported it at `X=0, Y=0, Width=2, Height=2`.
- Screenshots:
  - `evidence/screenshots/2026-06-23/live-show-debug/02-after-live-full-virtual.png`
  - `evidence/screenshots/2026-06-23/live-show-debug/02-after-live-monitor-1.png`
  - `evidence/screenshots/2026-06-23/live-show-debug/02-after-live-monitor-2.png`

Focused placement tests:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~WindowPlacementServiceTests" --no-restore -v minimal
```

Outcome: pass, 7 tests.

Related live/monitor tests:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~WindowPlacementServiceTests|FullyQualifiedName~OpenOutputCommand_WhenAlwaysUseSecondaryMonitorIgnoresStoredPrimaryMonitor|FullyQualifiedName~GoLiveCommand_WhenOutputClosed_UpdatesOutputWindowTextMonitorBody|FullyQualifiedName~GoLiveCommand_WithDatabaseSongMissingLyrics_ReloadsTextMonitorBodyLikeWinForms" --no-restore -v minimal
```

Outcome: pass, 10 tests.

Final deployment:

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext --no-restore
```

Outcome: pass. Re-deployed WPF Release output to `C:\EasiSlides\EasislidesNext`.

Final actual-run confirmation:

- Executed `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`.
- Clicked the WPF `Preview Go Live` button through UI Automation.
- Detected monitors:
  - `DISPLAY1`: non-primary, `X=3440, Y=0, Width=2560, Height=1440`.
  - `DISPLAY2`: primary, `X=0, Y=0, Width=3440, Height=1440`.
- UI Automation reported `EasiSlides Output` at `X=3440, Y=0, Width=2560, Height=1440`.
- Screenshot confirms Live Show text fills the output monitor:
  - `evidence/screenshots/2026-06-23/live-show-debug/06-after-live-dpi-fix-monitor-1.png`
  - `evidence/screenshots/2026-06-23/live-show-debug/06-after-live-dpi-fix-full-virtual.png`

## Known Warnings

- Existing `NU1701` NetOffice compatibility warnings appear during test runs.
- Existing nullable warnings from WinForms HookManager code appear during test runs.
- Existing `EasiDS001` token warning appears in `OutputWindowViewModel.cs`.

These warnings were not introduced by this change.

## Apostles Creed Preview/Live Parity Regression

Focused selected-page regression:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CopyPreviewToOutputCommand_PreparesCurrentPreviewLyricsPage" --no-restore -v minimal
```

Outcome: pass, 1 test.

Focused Preview-to-Output/Live parity tests:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~OutputSampleAppearance|FullyQualifiedName~CopyPreviewToOutputCommand|FullyQualifiedName~ToggleOutputLiveCommand_AfterCopyPreviewToOutput" --no-restore -v minimal
```

Outcome: pass, 9 tests.

Related live/preview/output flow tests:

```powershell
dotnet test Easislides.Wpf.Tests\Easislides.Wpf.Tests.csproj --filter "FullyQualifiedName~CopyPreviewToOutputCommand|FullyQualifiedName~PreviewToLiveCommand|FullyQualifiedName~ToggleOutputLiveCommand_AfterCopyPreviewToOutput|FullyQualifiedName~OutputSampleAppearance|FullyQualifiedName~GoLiveCommand_WhenOutputClosed|FullyQualifiedName~GoLiveCommand_WithDatabaseSongMissingLyrics|FullyQualifiedName~OpenOutputCommand_WhenAlwaysUseSecondaryMonitor" --no-restore -v minimal
```

Outcome: pass, 19 tests.

Final deployment:

```powershell
dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext --no-restore
```

Outcome: pass. Re-deployed WPF Release output to `C:\EasiSlides\EasislidesNext`.

Actual-run confirmation:

- Executed `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`.
- Selected `★★★사도신경★★★.txt`.
- Advanced the selected Preview text to page `3/10`.
- Clicked `btnToOutput`.
- Clicked `LIVE`.
- Confirmed the Output preview and actual Live Show both render the selected text:
  `나는 그의 / 유일하신 아들, / 우리 주 예수 / 그리스도를 믿습니다.`
- Confirmed the Output preview and actual Live Show both use the same blue cross background, black text color, centered alignment, and matching text position.

Screenshots:

- `evidence/screenshots/2026-06-23/apostles-live-parity/04-final-after-align-screen1.png`
- `evidence/screenshots/2026-06-23/apostles-live-parity/04-final-after-align-screen0.png`
