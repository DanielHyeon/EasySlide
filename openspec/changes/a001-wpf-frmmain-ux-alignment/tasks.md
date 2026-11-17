# Tasks

- [x] Record `FrmMain` UX control inventory.
- [x] Record current `MainWindow.xaml` parity map.
- [x] Define Classic Operator Layout spec.
- [x] Keep live core actions visible in `MainWindow.xaml`.
- [x] Separate Preview and Output state in the central header.
- [x] Run WPF tests.
- [x] Run main project build.
- [x] Record any remaining manual QA gaps.
- [x] Add Stop Live to the fixed operator bar.
- [x] Add a XAML guard test for fixed operator bar core commands.
- [x] Update parity docs for Stop Live and shortcut coverage.
- [x] Re-run WPF tests and main build after P1 follow-up.

## Manual QA gaps

- WPF app launch and live binding debug notification check still need an interactive desktop smoke run.
- 1180x760 visual inspection should confirm the wrapped operation bar does not cover preview content.
- Real output monitor scenarios still need manual QA: single monitor, selected monitor, no output display, and restored output after Black/Clear/Hide.
- Worship flow QA still needs a real session: select next item, Go Live, send-and-next, section jump, PPT slide navigation, Black, Restore, Refresh.
