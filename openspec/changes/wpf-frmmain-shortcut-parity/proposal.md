## Why

FrmMain operators rely on muscle-memory shortcuts during live worship, and WPF must not silently drift from those keys while layout work continues. The previous operator layout pass exposed core actions on screen; this change locks the matching keyboard paths and menu hints to the same command contract.

## What Changes

- Record the FrmMain-to-WPF shortcut parity contract for live operation keys.
- Add focused tests that guard key shortcut bindings, menu gesture hints, and focus-sensitive command handling.
- Document remaining manual keyboard QA for live output scenarios.
- No breaking changes.

## Capabilities

### New Capabilities

- `wpf-shortcut-parity`: WPF main-window shortcut and keyboard operation parity for FrmMain live operation flows.

### Modified Capabilities

- None.

## Impact

- `Easislides.Wpf` command catalog, shortcut registry, and `MainWindow.xaml` menu hint wiring may be inspected or guarded.
- `Easislides.Wpf.Tests` shortcut and XAML drift tests may be extended.
- `docs/wpf-migration` shortcut parity inventory will be updated.
