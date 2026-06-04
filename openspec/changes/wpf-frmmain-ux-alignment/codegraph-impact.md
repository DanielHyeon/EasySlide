# CodeGraph Impact

Checked on 2026-06-04.

## Queries

- `codegraph_status`: index healthy, 506 files, 28011 nodes, 53066 edges.
- `codegraph_context`: WPF MainWindow Classic Operator Layout UX alignment with FrmMain controls, commands, bindings, and tests.
- `codegraph_impact MainViewModel depth=1`: broad impact, 962 affected symbols.
- `codegraph_context`: FrmMain keyboard shortcuts and WPF ShortcutRegistry parity for Go Live F12 F11 F9 F3 Space Shift+Space verse jump keys.

## Decision

Do not change `MainViewModel` in this increment. The impact radius is too wide for a UX-only alignment step. Reuse existing commands and properties from XAML.

## Affected Files

- `Easislides.Wpf/MainWindow.xaml`
- `docs/wpf-migration/inventory/frmmain-ux-control-map.md`
- `docs/wpf-migration/inventory/mainwindow-ux-parity-map.md`
- `docs/wpf-migration/classic-operator-layout-spec.md`

## Out of Scope

- Shared helpers, DB access, Office Interop, output coordinates, render sizing.
