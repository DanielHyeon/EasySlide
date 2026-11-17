## Why

The WPF FrmMain-aligned shell currently resembles the legacy FrmMain layout, but key live-operation functions do not work from real legacy data:

- Saved Worship List selection only reads the new `%AppData%/EasislidesNext/WorshipLists/*.json` store, so existing `C:\EasiSlides\Admin\WorshipLists\*.esw` sessions are invisible.
- Bible data exists under `C:\EasiSlides\HolyBibles`, but the main shell only loads Bible metadata lazily when the Bible tab is selected, making first-screen operation appear empty.
- Drag-and-drop into the Worship List must accept the same practical sources as FrmMain: library songs, Bible selections, external PPT/media files, and legacy `.esw` files.

## What Changes

- Include legacy `.esw` worship lists from the configured working folder in the WPF saved Worship List selector.
- Load a selected legacy `.esw` Worship List through the existing parser and `MainViewModel.ImportEswWorshipList` path so song, Bible, PPT, media, and text items are mapped consistently.
- Preload Bible metadata on main window startup, matching FrmMain's always-available left-browser workflow.
- Accept dropped `.esw` files on the Worship List as a legacy Worship List import path, while keeping PPT/media drop insertion behavior.

## Impact

- Main functional files: `WorshipListStore`, `MainViewModel`, `MainWindow`, `WorshipListPanel`.
- Tests: focused WPF shell and store tests for legacy list discovery/load, Bible startup readiness, and drop classification.
- No direct SQLite schema or connection changes.
