## Why

The WPF shell still does not behave like FrmMain in the operator-facing pane zones. The left source area is missing legacy source tabs, the lower-left area does not expose the Praise Book tab beside the Worship List, and the right output area needs an explicit contract that separates slide thumbnails from the large output preview. This makes the program feel and function unlike FrmMain even when individual features exist elsewhere.

## What Changes

- Align the WPF left source tabs with FrmMain's first-screen source roles: `Folders`, `InfoScr`, `PowerPoint`, `Bibles`, `Images`, `Media`, and `Default`.
- Restore the lower-left tab role split: `Worship List` and `Praise Book`.
- Surface Praise Book operations in the lower-left pane instead of only behind a modal menu command.
- Surface Images and Default controls in the left source pane so their FrmMain entry points are visible in the same area.
- Pin the right-side output panes as a FrmMain-style thumbnail pane plus large preview pane.
- Add focused XAML regression tests for these pane contracts.

## Capabilities

### New Capabilities
- `wpf-frmmain-pane-role-parity`: The WPF main shell must expose FrmMain's pane roles and first-screen entry points for source browsing, Worship List/Praise Book operation, and output preview.

### Modified Capabilities

## Impact

- WPF `MainWindow.xaml` and `MainWindow.xaml.cs`: pane structure, lazy source tab loading, and inline Praise Book/Image source wiring.
- WPF Praise Book and Image library view models: reuse only; no persistence change planned.
- WPF shell/composite tests: update the expected left/right pane contracts.
- No SQLite schema, Office interop, DirectShow/media backend, or WinForms FrmMain behavior changes.
