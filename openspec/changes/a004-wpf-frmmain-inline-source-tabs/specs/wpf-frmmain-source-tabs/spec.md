## ADDED Requirements

### Requirement: PowerPoint Source Must Be Inline

The WPF main shell SHALL expose PowerPoint files as a left-browser source tab in the main console.

#### Scenario: Operator opens the PowerPoint source tab

- **GIVEN** the configured working folder contains a `Powerpoint` directory
- **WHEN** the operator selects the PowerPoint source tab
- **THEN** the tab loads PowerPoint files from that directory
- **AND** the operator can add the selected file to the Worship List without opening a modal window

### Requirement: Media Source Must Be Inline

The WPF main shell SHALL expose media files as a left-browser source tab in the main console.

#### Scenario: Operator opens the Media source tab

- **GIVEN** the configured working folder contains a `Media` directory
- **WHEN** the operator selects the Media source tab
- **THEN** the tab loads media files from that directory
- **AND** the operator can add the selected file to the Worship List without opening a modal window

### Requirement: Inline Source Drag Must Insert Into Worship List

The inline PowerPoint and Media lists SHALL allow dragging selected files into the Worship List.

#### Scenario: Operator drags a source file to the Worship List

- **GIVEN** a PowerPoint or Media file is selected in an inline source tab
- **WHEN** the operator drags that row onto the Worship List
- **THEN** the drag payload uses the existing file-drop contract
- **AND** the Worship List inserts the file at the drop position through the existing external file path
