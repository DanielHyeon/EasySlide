# Risk Pattern Report

## Tooling

- `ast-grep --version`: `ast-grep 0.44.0`
- Rules stored in repo:
  - `ast-grep/rules/csharp-risk-patterns.yml`
  - `ast-grep/rules/sdd-invariants.yml`

## Risk Scan

Command:

```powershell
ast-grep scan --rule ast-grep\rules\csharp-risk-patterns.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf\Shell\LiveSessionService.cs Easislides.Wpf\Shell\OutputWindowHost.cs Easislides.Wpf\Shell\OutputWindowViewModel.cs Easislides\FrmMain.Logic.cs
```

Corrected command:

```powershell
ast-grep scan --rule ast-grep\rules\csharp-risk-patterns.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf\Shell\LiveSessionService.cs Easislides.Wpf\Shell\OutputWindowHost.cs Easislides.Wpf\Shell\OutputWindowViewModel.cs Easislides\Easislides\FrmMain.Logic.cs
```

Outcome: pass with one warning in existing WinForms code:

- `Easislides/Easislides/FrmMain.Logic.cs:1176`: `Convert.ToInt32(DataUtil.ExtractOneInfo(...))`

Disposition: existing WinForms parsing code, outside the implementation boundary, not modified by this change.

Covered risk patterns:

- `Convert.ToInt32(...)`
- `Marshal.ReleaseComObject(...)`

## SDD Invariant Scan

Command:

```powershell
ast-grep scan --rule ast-grep\rules\sdd-invariants.yml Easislides.Wpf\Shell\MainViewModel.cs Easislides.Wpf\Shell\LiveSessionService.cs Easislides.Wpf\Shell\OutputWindowHost.cs Easislides.Wpf\Shell\OutputWindowViewModel.cs
```

Outcome: pass. The rule reported 10 live-session mutation checkpoints around `GoLive(...)` and `UpdateHiddenContent(...)`.

Invariant conclusion: the fix must preserve the same session mutation calls and only alter the prepared `LiveQueueItem` payload before those calls.
