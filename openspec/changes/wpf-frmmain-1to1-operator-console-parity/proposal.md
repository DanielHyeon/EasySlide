## Why

The current WPF shell still feels like a frame around incomplete functionality rather than a usable FrmMain replacement. The recent pane-role work exposed the correct broad regions, but the operator still cannot rely on the same top-left source browser, bottom-left Worship List/Praise Book, Bible lookup, drag-and-drop, or right-side Preview/Output workflows that make legacy FrmMain usable during worship.

This change resets the migration target to strict FrmMain 1:1 operator-console parity before further visual modernization.

## What Changes

- Establish a control/event inventory for legacy `FrmMain` using source control names, event handlers, and data-loading methods.
- Establish a FrmMain-to-WPF 1:1 mapping table that marks each major area as `implemented`, `partial`, `missing`, or `intentionally deferred`.
- Require WPF `MainWindow` to mirror the legacy operator console hierarchy:
  - `splitContainerMain`
  - `splitContainer1`
  - `tabControlSource`
  - `tabControlLists`
  - `splitContainer2`
  - `splitContainerPreview`
  - `splitContainerOutput`
- Require the WPF main shell to restore real functionality for:
  - Folders/Songs source browsing
  - Worship List load/edit/reorder
  - Praise Book inline list
  - Bible lookup and selected-passage add
  - InfoScreen, PowerPoint, Images, and Media source tabs
  - Preview and Output as distinct operator targets
  - double-click, Enter, context menu, drag-and-drop, and keyboard shortcuts
- Add a manual UAT checklist based on legacy operator scenarios.

## Non-goals

- Do not redesign FrmMain into a new workflow before parity is achieved.
- Do not remove legacy paths, `.bak` files, or existing WinForms behavior.
- Do not make database schema changes.
- Do not claim completion from layout-only work.

## Impact

- Primary WPF files: `Easislides.Wpf/MainWindow.xaml`, `MainWindow.xaml.cs`, WPF composites, and associated view models/services.
- Legacy reference files: `Easislides/Easislides/FrmMain.*`, `KeyboardActionHandler.cs`.
- Documentation: `docs/wpf-migration/inventory/*` and the 1:1 mapping plan.
- Verification: OpenSpec validation, WPF tests, WinForms build, and manual UAT against `C:\EasiSlides`.
