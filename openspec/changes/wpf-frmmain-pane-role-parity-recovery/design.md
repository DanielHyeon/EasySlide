## Design

### Pane Contract

This slice treats FrmMain's screen layout as an operational contract, not just a visual reference:

- Left upper pane: source browser tabs, bottom-aligned, in the FrmMain order.
- Left lower pane: list tabs, bottom-aligned, with `Worship List` and `Praise Book`.
- Middle pane: lyrics/text work area remains unchanged.
- Right upper pane: thumbnail/source preview list for the selected output-capable item.
- Right lower pane: large output preview.

### Source Tabs

Existing WPF source tabs already cover library songs, InfoScreen, PowerPoint, Bible, and Media. The missing parts are:

- legacy labels/order/tab strip placement;
- `Images` as a first-screen entry point;
- `Default` as a first-screen formatting/defaults entry point.

`Images` reuses `ImageLibraryViewModel` and existing background-apply callbacks. `Default` reuses existing appearance/reset commands from `MainViewModel`.

### Praise Book

WPF already has `PraiseBookIndexViewModel` and a modal `PraiseBookIndexWindow`, but FrmMain exposes Praise Book as a lower-left peer of Worship List. The lower-left pane should host a Praise Book tab that can:

- list saved Praise Books;
- open a saved Praise Book through the existing view model command;
- show grouped entries from the current Praise Book/index;
- add an entry to the Worship List with the same song lookup path currently used by the modal window.

The modal window can remain available. This change adds the missing first-screen route.

### Right Output Pane

The current WPF right side already has a thumbnail grid and a large preview surface. This change pins their roles in tests and UI naming so later functional work does not collapse the two panes into a generic preview.

### Risks

- Inline Praise Book uses library song IDs when present, then falls back to title/number matching through the existing `MainViewModel.AddPraiseBookSong` method. If saved book data lacks IDs, matching may still depend on existing library data quality.
- Images thumbnails can be expensive. This slice keeps the existing async `ImageLibraryViewModel` loading behavior.
- This does not yet implement every FrmMain context menu action; it restores first-screen pane roles and primary add/open gestures.
