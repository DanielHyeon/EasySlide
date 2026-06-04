# WPF FrmMain UX alignment

## Why

The WPF shell already runs and exposes many commands, but its default layout still feels more like a modern dashboard than the proven WinForms `FrmMain` operator console. During worship/live operation, the important requirement is not visual novelty; it is immediate access to the controls operators already use under pressure.

## What Changes

- Document the WinForms `FrmMain` control, event, menu, and shortcut inventory that matters for WPF parity.
- Map the current `MainWindow.xaml` surface against that inventory.
- Define the Classic Operator Layout for the WPF first screen at the 1180x760 baseline.
- Reposition existing WPF controls so live operations remain visible without opening menus, modals, or the right inspector.
- Keep dangerous live stop/safety actions visually available and distinguishable from ordinary navigation.

## Scope

- UI/UX and XAML layout only for this increment.
- No database, Office Interop, output coordinate, or rendering pipeline changes.
- No public API or persistence format changes.

## Acceptance Criteria

- Go Live, send-and-next, Stop Live, Black, Clear, Hide, Restore, Restart, Refresh, and item/slide/section navigation are visible on the first screen.
- Preview and Output state are visually separated.
- `IsChecked` bindings keep explicit `Mode`.
- WPF XAML tests pass.
- Main project build still passes.
