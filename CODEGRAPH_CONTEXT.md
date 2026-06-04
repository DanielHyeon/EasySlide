## wpf-frmmain-inline-source-tabs

Date: 2026-06-04

Task: Restore FrmMain-style inline PowerPoint and Media source tabs in the WPF main shell.

CodeGraph checks:

- `codegraph_status`: healthy index, 508 files, 28063 nodes, 50729 edges.
- `codegraph_context`: WPF source-browser parity centers on `MainWindow`, `WorshipListPanel`, `PowerPointLibraryViewModel`, and `MediaLibraryViewModel`.
- `codegraph_impact MainWindow`: UI shell impact only; main touched surface is `Easislides.Wpf/MainWindow.xaml(.cs)`.
- `codegraph_impact PowerPointLibraryViewModel`: existing VM and library tests; no VM behavior change planned.
- `codegraph_impact MediaLibraryViewModel`: existing VM and library tests; no VM behavior change planned.
- `codegraph_explore`: `WorshipListPanel` already accepts `DataFormats.FileDrop` and inserts through `MainViewModel.AddExternalFilesRelativeTo`, so inline PowerPoint/Media source lists can reuse the same drop contract.

Implementation boundary:

- Add inline WPF source tabs and code-behind gesture wiring only.
- Reuse existing PowerPoint/Media services and view models.
- Do not touch SQLite, Office interop, media playback, or WinForms `FrmMain`.
