## ADDED Requirements

### Requirement: Saved Worship Lists Must Load From First-Screen Selector Gestures

The WPF Worship List panel SHALL allow the operator to load the selected saved Worship List from the first-screen selector without opening a management dialog.

#### Scenario: Operator presses Enter on selected saved list

- **GIVEN** `SavedWorshipListNames` contains a legacy or WPF saved Worship List
- **AND** the operator selected a list in the first-screen Worship List selector
- **WHEN** the operator presses Enter while the selector has focus
- **THEN** the existing selected-list load command is invoked
- **AND** the selected Worship List is loaded through the same path as the explicit load button

#### Scenario: Operator double-clicks the selector after choosing a list

- **GIVEN** the operator selected a saved Worship List in the first-screen selector
- **WHEN** the operator double-clicks the selector
- **THEN** the existing selected-list load command is invoked

### Requirement: Bible Add Must Use Current Selection Without Requiring Text Drag

The WPF main shell SHALL allow the operator to add the current Bible selection to the Worship List without requiring a manual text-range drag when a valid current Bible selection already exists.

#### Scenario: Current Bible selection exists but text range is not selected

- **GIVEN** `BibleViewModel.SelectedSelection` contains a valid `IdString`
- **AND** the inline Bible passage text box has no selected range
- **WHEN** the operator invokes add Bible verse from the main shell
- **THEN** that current Bible selection is added to the Worship List

#### Scenario: Text range is selected

- **GIVEN** the inline Bible passage text box has a selected verse range
- **WHEN** the operator invokes add Bible verse from the main shell
- **THEN** the selected text range is converted to a Bible selection and added
- **AND** the current range-based drag/drop behavior remains available
