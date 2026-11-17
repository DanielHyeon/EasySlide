# Design

## Reference

The legacy `FrmMain` screenshot is not a centered dashboard. It is a dense broadcast console:

- Left column spans the full height and is vertically split into source tabs and Worship List.
- The remaining area is horizontally split into a top text/list row and a bottom preview row.
- Preview and Output are side-by-side columns. Both are visible at all times.
- Command buttons sit directly on the Preview/Output pane edges, not in a distant global panel.

## WPF Layout Strategy

Use the existing `MainViewModel` bindings and commands, but reshape `MainWindow.xaml`:

- Root operator grid columns:
  - left fixed source/list column
  - middle Preview column
  - right Output column
- Root operator grid rows:
  - top text/list work area
  - splitter
  - bottom slide preview/output area
- Keep the existing source browser and `WorshipListPanel` in the left column.
- Reuse `SlidePreviewControl` for the Preview slide surface.
- Render an Output slide-like surface using current live state bindings until the full output render surface can be embedded.
- Keep inspector-related VM and menu behavior intact; first-screen visual priority is Output.

## Non-Goals

- No new live rendering model.
- No changes to projector coordinates or display selection behavior.
- No manual deletion of previous inspector code paths.
