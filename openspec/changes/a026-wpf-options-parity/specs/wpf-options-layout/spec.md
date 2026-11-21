## ADDED Requirements

### Requirement: Operator options navigation

The WPF options dialog SHALL present the eight WinForms categories in their original order and retain access to WPF device diagnostics and administrative tools.

#### Scenario: Operator opens options

- **WHEN** the operator opens Options
- **THEN** View → Options opens a native WPF options window with Main Window, Show, Monitors, Alerts, Folders, Bibles, Licence and Keyboard in this order, including the same control groups, positions and icons
- **AND** the WPF settings and device diagnostics button switches pages within that window

#### Scenario: Text file without an individual font size

- **WHEN** a worship list references a text file with no individual font size or assigned song folder
- **THEN** its preview, pagination and audience output use the stored default folder font when available
- **AND** importing the WinForms lyrics monitor font size updates the stage display preference independently

### Requirement: Options edit lifetime

The WPF options dialog SHALL stage changes to application preferences until confirmation.

#### Scenario: Cancel edited preferences

- **WHEN** an operator cancels or closes the dialog after editing a preference
- **THEN** the application settings retain their previous value

#### Scenario: Concurrent change

- **WHEN** an operator confirms an edit after another component updates an unrelated preference
- **THEN** both preference updates are retained

### Requirement: Apply operator preferences at runtime

The WPF application SHALL apply confirmed preview, presentation, lyrics monitor, camera, alert and keyboard options to their respective runtime consumers.

#### Scenario: Independent outputs

- **WHEN** the operator enables a separately positioned lyrics display
- **THEN** that display follows live lyrics independently of the projector and the operator's preview

#### Scenario: Targeted formatting and input

- **WHEN** the operator changes a specific alert style or global key switch
- **THEN** the named banner or shortcut uses the new setting without changing the other banners or switches

#### Scenario: Bible and folder edits are cancelled

- **WHEN** an operator edits folders or Bible versions in Options and closes it without confirming
- **THEN** the installed databases and active library retain their previous contents

#### Scenario: Version-specific Bible display

- **WHEN** an operator sends a bilingual Bible selection to the projector
- **THEN** each version uses its linked folder font and its own size percentage

#### Scenario: Preview and stage settings differ

- **WHEN** the congregation display hides chord lines and the lyrics monitor enables them
- **THEN** the lyrics monitor renders the chords in its configured notation size while the congregation sees the lyrics

#### Scenario: Styles survive simultaneous alerts

- **WHEN** a message and a parental call are shown in the same vertical band
- **THEN** both remain readable, with the family, size, colours and effects configured for each type
