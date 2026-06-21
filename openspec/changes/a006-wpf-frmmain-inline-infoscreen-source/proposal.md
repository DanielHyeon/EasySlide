## Why

Legacy `FrmMain` exposes an `InfoScr` source list in the left browser. Operators can pick saved information screens and add them to the Worship List without opening a separate editor first.

The WPF shell currently has a Notice/InfoScreen editor window and persistent `InfoScreenStore`, but saved notices are not available as an inline source tab. This keeps one more common FrmMain workflow hidden behind a modal path.

## What Changes

- Add an inline `InfoScr` source tab to the WPF left browser.
- Load saved `InfoScreenStore` names into the tab.
- Add selected saved notices to the Worship List by double-click or button.
- Support dragging a saved notice into the Worship List at the drop position.

## Impact

- Main files: `MainWindow.xaml`, `MainWindow.xaml.cs`, `WorshipListPanel.xaml.cs`.
- New focused source view model reusing `InfoScreenStore`.
- Small `MainViewModel` insertion helper for Notice items at a target position.
- Tests cover InfoScreen source loading/addition and XAML/drag wiring.
- No SQLite, Office Interop, media playback, output coordinates, or legacy WinForms changes.
