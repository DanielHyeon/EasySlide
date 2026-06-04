## Design

### Principle

WPF must first become a faithful FrmMain operator console. "Faithful" means a legacy operator can perform the same worship workflow from the same visible regions with the same primary gestures. The implementation can use WPF view models and services internally, but visible workflow parity is the contract.

### Source Of Truth

The migration source is the legacy control tree and event graph:

- `FrmMain.Designer.cs` for layout and control names
- `FrmMain.Events.cs` for resize, live-message, and UI event behavior
- `FrmMain.Logic.cs` for data loading and state updates
- `FrmMain.cs` for source selection, drag/drop, Bible, media, and command handlers
- `FrmMain.Fields.cs` for `DragDropSource` and live remote action enums

### Operator Console Regions

WPF `MainWindow` should mirror these regions:

| Legacy region | WPF target |
| --- | --- |
| `splitContainerMain.Panel1` | left source/list column |
| `splitContainerMain.Panel2` | right Preview/Output console |
| `splitContainer1.Panel1` | top-left source tabs |
| `splitContainer1.Panel2` | bottom-left Worship List/Praise Book tabs |
| `tabControlSource` | Folders, InfoScr, PowerPoint, Bibles, Images, Media, Default |
| `tabControlLists` | Worship List, Praise Book |
| `splitContainer2.Panel1` | Preview column |
| `splitContainer2.Panel2` | Output column |
| `splitContainerPreview.Panel1/Panel2` | Preview controls and preview surface |
| `splitContainerOutput.Panel1/Panel2` | Output controls and output surface |

### Mapping Method

Every implementation slice starts by updating `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md`. Each row must include:

- legacy control or handler;
- WPF target control/view model/command;
- status;
- next phase;
- verification evidence.

Rows marked `missing` or `partial` cannot be called complete.

### Data Loading

The WPF shell must use the configured working folder, currently expected to be `C:\EasiSlides`, and must load real legacy data:

- Worship Lists from `Admin\WorshipLists`;
- Praise Books from legacy praise-book sources;
- Bible versions/books/passages from legacy Bible data;
- InfoScreen, PowerPoint, Images, and Media folder contents.

### Interaction Parity

The following gestures are part of parity, not polish:

- double-click to add or preview;
- Enter to execute active lookup/list action;
- Delete to remove selected Worship List item;
- context menus for song, Worship List, Praise Book, Bible, and Images actions;
- drag-and-drop from all legacy `DragDropSource` values into Worship List;
- Preview and Output keyboard navigation;
- live shortcuts including F12, F11, F9, F3, Space, Shift+Space, and verse jump keys.

### Risk Controls

- Shared services and data parsers require CodeGraph impact review before editing.
- UI slices should be small and region-specific.
- Tests must cover view structure and command routing where possible; manual UAT must cover the live operator scenarios.
- WinForms build must remain green because legacy FrmMain remains the behavior oracle.
