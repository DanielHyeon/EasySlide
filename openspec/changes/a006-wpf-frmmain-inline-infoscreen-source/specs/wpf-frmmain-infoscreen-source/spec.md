## ADDED Requirements

### Requirement: InfoScreen Source Must Be Inline

The WPF main shell SHALL expose saved InfoScreens as a left-browser source tab.

#### Scenario: Operator opens the InfoScr source tab

- **GIVEN** saved InfoScreens exist in the WPF InfoScreen store
- **WHEN** the operator selects the InfoScr tab
- **THEN** the saved names are listed inline in the main shell

### Requirement: Saved InfoScreen Must Add To Worship List

The WPF main shell SHALL add a selected saved InfoScreen to the Worship List without requiring the modal editor.

#### Scenario: Operator double-clicks a saved InfoScreen

- **GIVEN** a saved InfoScreen is selected in the InfoScr tab
- **WHEN** the operator double-clicks it
- **THEN** a Notice queue item is added with the saved text and formatting

### Requirement: InfoScreen Drag Must Preserve Drop Position

The inline InfoScreen source SHALL support dragging saved notices into the Worship List.

#### Scenario: Operator drops a saved InfoScreen onto the Worship List

- **GIVEN** a saved InfoScreen is selected in the InfoScr tab
- **WHEN** the operator drags it onto a Worship List row
- **THEN** the Notice item is inserted at that drop position
