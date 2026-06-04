# Tasks

- [x] Compare legacy `FrmMain` screenshot against current WPF shell geometry.
- [x] Run CodeGraph context/impact for `MainWindow`.
- [x] Add visual parity OpenSpec delta.
- [x] Rework `MainWindow.xaml` into `FrmMain`-style source/preview/output panes.
- [x] Add XAML drift tests for named panes and command strips.
- [x] Run focused WPF shell tests.
- [x] Run full WPF tests.
- [x] Run main WinForms project build.
- [x] Relaunch WPF app for interactive visual smoke.
- [x] Commit and push.

## Manual QA Gaps

- Visual check at the user's widescreen size should confirm the right side is Output, not the inspector.
- Real output monitor QA still needs manual verification after this XAML-only pass.
