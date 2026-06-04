## ADDED Requirements

### Requirement: WPF Main Shell Must Mirror FrmMain Operator Regions

The WPF main shell SHALL expose the same operator regions as legacy FrmMain: top-left source browser, bottom-left Worship List/Praise Book, Preview column, Output column, and bottom status area.

#### Scenario: Operator opens the WPF shell

- **GIVEN** the WPF main window is launched
- **WHEN** the main shell is displayed
- **THEN** the top-left source browser and bottom-left Worship List/Praise Book are visible at the same time
- **AND** Preview and Output are separate visible regions
- **AND** live safety commands are visible without opening a modal

### Requirement: Source Browser Must Preserve FrmMain Tab Roles

The WPF source browser SHALL provide FrmMain-equivalent source tabs for Folders, InfoScr, PowerPoint, Bibles, Images, Media, and Default.

#### Scenario: Operator uses source tabs

- **GIVEN** legacy data exists under `C:\EasiSlides`
- **WHEN** the operator opens each source tab
- **THEN** the tab loads the corresponding real source data
- **AND** the tab exposes the same primary add/preview gestures as FrmMain

### Requirement: Lower Left Lists Must Preserve FrmMain Worship List And Praise Book Roles

The WPF lower-left list region SHALL keep Worship List and Praise Book as inline tabs, not modal-only workflows.

#### Scenario: Operator prepares a service

- **GIVEN** saved worship lists and praise books exist
- **WHEN** the operator uses the lower-left region
- **THEN** saved Worship Lists can be loaded
- **AND** Worship List items can be selected, reordered, deleted, and sent to Preview/Output
- **AND** Praise Book entries can be browsed and added from the same lower-left region

### Requirement: Bible Lookup Must Match FrmMain Main-Shell Workflow

The WPF Bible tab SHALL support version/book loading, direct reference lookup, selected passage text, and adding selected passages to Worship List.

#### Scenario: Operator adds a Bible passage

- **GIVEN** Bible versions are configured in the working folder
- **WHEN** the operator enters a reference and selects verses
- **THEN** the selected passage is shown in Preview
- **AND** it can be added to Worship List using button, Enter, context menu, or drag/drop flow as applicable

### Requirement: Preview And Output Must Be Independent Operator Targets

Preview controls SHALL act on the selected/preview item, while Output controls SHALL act on the live output item.

#### Scenario: Operator navigates slides

- **GIVEN** Preview and Output are showing different items or different slides
- **WHEN** the operator uses Preview slide or verse controls
- **THEN** only Preview changes unless the operator explicitly sends it live
- **WHEN** the operator uses Output slide or verse controls
- **THEN** only the live Output changes

### Requirement: Legacy Gestures Must Be Mapped Before Completion

The migration SHALL map legacy double-click, Enter, Delete, context menu, drag-and-drop, and keyboard shortcut gestures before the corresponding area is considered complete.

#### Scenario: Phase completion is claimed

- **GIVEN** a FrmMain area is marked complete
- **WHEN** the mapping table is reviewed
- **THEN** every primary legacy gesture for that area is marked implemented or explicitly deferred with rationale
- **AND** verification evidence exists

### Requirement: Manual UAT Must Use Real Legacy Data

Manual verification SHALL use the configured legacy working folder, including `C:\EasiSlides` when present.

#### Scenario: UAT is run

- **GIVEN** real worship, Bible, PowerPoint, image, media, and praise-book data exists
- **WHEN** the UAT checklist is executed
- **THEN** results are recorded per scenario
- **AND** missing behavior is converted into implementation tasks rather than marked complete
