## Design

### Inline Source Model

Add `InfoScreenSourceViewModel` under `Easislides.Wpf/Shell`. It reads saved names from `IInfoScreenStore`, tracks selection, and exposes:

- `Screens`
- `SelectedScreen`
- `StatusText`
- `LoadCommand`
- `AddSelectedCommand`
- `LoadSelectionAsync`

The view model converts `InfoScreenDto` into `NoticeOptions` so queue insertion preserves saved font size, alignment, color, background color, emphasis, and font name.

### Main Window

`LeftBrowserTabs` gains `Tag="InfoScreenSource"`. The tab lazy-loads an `InfoScreenSourceViewModel`, using the existing `InfoScreenStore` and `MainViewModel.AddTextItem`.

Double-click executes the view model's add command.

Drag creates an `InfoScreenSelection` typed payload. This avoids treating arbitrary text drops as notices.

### Worship List Drop

`WorshipListPanel` accepts `InfoScreenSelection` and calls a new `MainViewModel.AddTextItemRelativeTo` helper, matching existing Bible/song/file drop insertion semantics.

### Boundaries

The existing `NoticeScreenWindow` remains the editor and immediate-send path. This change only restores the saved notice source list in the main shell.
