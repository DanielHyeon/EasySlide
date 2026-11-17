## Design

### Source Tabs

The WPF `LeftBrowserTabs` gains two new `TabItem`s:

- `Tag="PowerPointSource"` bound to an inline `PowerPointLibraryViewModel`.
- `Tag="MediaSource"` bound to an inline `MediaLibraryViewModel`.

The tabs use the same simple controls as the existing modal windows: refresh, add selected, include subfolders, filename filter, file list, and status text.

### Lazy Loading

`MainWindow.LeftBrowserTabs_SelectionChanged` initializes each inline source once. This keeps startup cost low while making the source visible in the main shell. The initial folder resolver prefers:

- PowerPoint: `<WorkingFolder>\Powerpoint`, then `<WorkingFolder>`, then Documents.
- Media: configured `MediaDirectory`, then `<WorkingFolder>\Media`, then `<WorkingFolder>`, then Documents.

### Add And Drag

Double-click executes the existing `AddSelectedCommand` on the inline view model.

Drag starts only from an actual list row and packages the selected file as `DataFormats.FileDrop`. `WorshipListPanel` already accepts file drops and inserts them through `MainViewModel.AddExternalFilesRelativeTo`, preserving the existing validated queue insertion behavior.

### Boundaries

The existing modal menu commands remain. Their folder resolution is updated to use the same helper methods so modal and inline paths agree.
