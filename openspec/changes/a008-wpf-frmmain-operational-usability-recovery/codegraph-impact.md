## CodeGraph Context

### `AddBibleVerse_Click`

`codegraph_impact("AddBibleVerse_Click", depth: 2)` reports the change is scoped to `Easislides.Wpf/MainWindow.xaml.cs` code-behind and nearby Bible drag/add handlers. The existing add route calls `BibleViewModel.BuildSelection(...)` and then `MainViewModel.AddBibleSelection(...)`.

### `LoadSelectedWorshipListCommand`

`codegraph_impact("LoadSelectedWorshipListCommand", depth: 2)` confirms selected-list loading is centralized in `MainViewModel.LoadSelectedWorshipListAsync`, with legacy `.esw` and WPF JSON handling already routed there. The new panel gestures should call the command rather than duplicate loading logic.

### `BuildSelection`

`codegraph_impact("BuildSelection", depth: 2)` shows `BibleViewModel.BuildSelection` updates `SelectedSelection` through existing repository-backed selection construction and is covered by Bible view-model tests. Main-shell add fallback can safely prefer an existing non-empty `SelectedSelection` when no text range is selected.
