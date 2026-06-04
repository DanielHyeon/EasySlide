## ADDED Requirements

### Requirement: FrmMain Source Pane Roles
The WPF main shell SHALL expose FrmMain source-browser roles on the first screen using bottom-aligned tabs named `Folders`, `InfoScr`, `PowerPoint`, `Bibles`, `Images`, `Media`, and `Default`.

#### Scenario: Source tabs are visible in FrmMain order
- **WHEN** the WPF main shell is loaded
- **THEN** the left upper source pane lists the `Folders`, `InfoScr`, `PowerPoint`, `Bibles`, `Images`, `Media`, and `Default` tabs in that order
- **AND** the source tab strip is bottom-aligned like FrmMain

### Requirement: FrmMain Lower Left List Roles
The WPF main shell SHALL expose `Worship List` and `Praise Book` as lower-left peer tabs on the first screen.

#### Scenario: Praise Book is available beside Worship List
- **WHEN** the WPF main shell is loaded
- **THEN** the lower-left pane contains a bottom-aligned tab strip with `Worship List` and `Praise Book`
- **AND** the `Praise Book` tab provides a first-screen flow for opening a saved Praise Book and adding entries to the Worship List

### Requirement: FrmMain Right Preview Roles
The WPF main shell SHALL keep the right upper thumbnail pane and right lower large output preview pane as distinct roles.

#### Scenario: Right output panes stay separated
- **WHEN** the WPF main shell is loaded
- **THEN** the right upper pane contains the output thumbnail/list region
- **AND** the right lower pane contains the large output preview region
