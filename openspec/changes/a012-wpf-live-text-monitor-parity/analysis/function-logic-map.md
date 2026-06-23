# Function Logic Map

## WinForms Baseline

### `FrmMain.PreviewItemToLive`

- Input: selected preview item and current live/show state.
- Main branch: copy preview state to output state via `CopyPreviewToOutput`.
- Side effect: when not already showing, starts live presentation.
- Text-monitor invariant: the item that becomes live is copied into the output/live model before display logic runs.

### `FrmMain.CopyPreviewToOutput`

- Input: preview item state.
- Main branch: copies preview item fields into output fields.
- Side effect: calls `LoadItem(...)` for output/live context.
- Text-monitor invariant: live output is not just a command state; it is backed by an output item payload that can be formatted by the launch screen.

## WPF Target Path

### `MainViewModel.PublishSelectedItemAsync`

- Input: selected queue item from the WPF preview/live command.
- Existing behavior: prepared item is projected and passed to `_session.GoLive(...)`.
- Missing branch: DB song items with empty `Lyrics` were not hydrated before projection.
- New behavior: call `PrepareLiveItemForOutputAsync(...)` before `ResolveLiveProjection(...)`.

### `MainViewModel.PublishOutputItemAsync`

- Input: output item already selected or navigated by live controls.
- Existing behavior: prepared item is projected and passed to `_session.GoLive(...)`.
- Missing branch: same empty-lyrics DB song case.
- New behavior: call `PrepareLiveItemForOutputAsync(...)` before `ResolveLiveProjection(...)`.

### `PrepareLiveItemForOutputAsync`

- First preserves all existing synchronous preparation by calling `PrepareLiveItemForOutput(...)`.
- If the item is not a song, return unchanged.
- If lyrics are already present, return unchanged.
- If the song DB id cannot be resolved, return unchanged.
- Otherwise load `SongDetail` from `_songDetail`.
- If detail or detail lyrics are missing, return unchanged.
- Merge display fields from detail only when the prepared item has no value.
- On load failure, set `StatusText` and return the original prepared item.

## Invariants

- `LiveSessionService.GoLive(...)` remains the single session publish operation.
- `OutputWindowHost` and `OutputWindowViewModel` still receive body text through `LiveSessionSnapshot`.
- Existing fallback handling for Text, Bible, Notice, external text, and PowerPoint remains in the existing projection/snapshot flow.
- The implementation does not change WinForms or database contracts.
