## Why

The previous WPF recovery restored a narrow data path, but it did not restore the FrmMain operator workflow. In legacy FrmMain, PowerPoint and Media are visible source panes in the main console and can be added to the Worship List without opening a separate modal browser. The current WPF shell keeps these sources behind menu-launched windows, so the first screen still feels non-functional compared with FrmMain.

## What Changes

- Add inline PowerPoint and Media source tabs to the WPF left source browser.
- Lazy-load each source tab from the configured working folder, preferring `Powerpoint` and `Media` subfolders under the working folder when present.
- Support double-click and drag-to-Worship-List from the inline lists.
- Keep the existing modal PowerPoint/Media windows available from the menu.

## Impact

- Main files: `Easislides.Wpf/MainWindow.xaml`, `Easislides.Wpf/MainWindow.xaml.cs`.
- Existing view models reused: `PowerPointLibraryViewModel`, `MediaLibraryViewModel`.
- Existing Worship List drop path reused through `DataFormats.FileDrop`.
- Tests: structural WPF shell tests for tab presence, binding, lazy load, and drag contracts.
- No SQLite, Office interop, media playback backend, or WinForms changes.
