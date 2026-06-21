## Design

### Operator-Facing Recovery

This is a usability recovery slice for paths that already have partial implementation:

- Worship List data is available through `SavedWorshipListNames` and `LoadSelectedWorshipListCommand`.
- Bible data is loaded through `BibleViewModel`, and `SelectedSelection` already represents the latest valid Bible selection.
- `MainViewModel.AddBibleSelection` and Worship List drag/drop insertion already create queue items.

The change avoids new persistence, DB connections, or rewiring the large `MainViewModel`. It makes the existing routes reachable from the main shell.

### Worship List Selector Gestures

The saved-list combo remains an explicit-load control to avoid accidental queue replacement on simple selection. To match FrmMain's practical flow, the operator can also load the selected list by:

- pressing Enter while the saved-list selector has focus;
- double-clicking the saved-list selector after choosing a list.

Both gestures call the existing `LoadSelectedWorshipListCommand`, so all current legacy `.esw` and WPF JSON handling stays centralized.

### Bible Add Fallback

`AddBibleVerse_Click` currently always rebuilds from `BiblePassageBox.SelectionStart/SelectionLength`. If no text range is selected, that produces an empty selection even when `BibleViewModel.SelectedSelection` already contains a valid typed-reference or selection result.

The recovered behavior is:

1. If the passage text box has a selected range, keep the current range-based `BuildSelection` behavior.
2. Otherwise use `BibleViewModel.SelectedSelection` when it has an `IdString`.
3. Add only non-empty selections to the Worship List through `MainViewModel.AddBibleSelection`.

Typed-reference jump already sets `LastReferenceStart`, `LastReferenceLength`, and `SelectedSelection`; the fallback makes the add button work even if the WPF text selection is not retained.

### Risks

- Loading a Worship List still replaces the current queue. The new gestures are explicit Enter/double-click actions, not passive selection changes.
- `MainViewModel` remains high-impact. This slice intentionally avoids touching its load/import internals.
- Bible selection fallback depends on `SelectedSelection` being maintained by existing `BuildSelection`/typed-reference paths; tests should pin that behavior.
