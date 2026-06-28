## ADDED Requirements

### Requirement: Settings controls SHALL not clip text

WPF settings panel text inputs and checkboxes SHALL reserve enough vertical space for their content at normal desktop DPI and font settings.

#### Scenario: Settings text boxes have clipping-safe dimensions

GIVEN the settings panel renders numeric and template-name text boxes
WHEN the controls use the shared settings text box style
THEN the style SHALL define a minimum height and vertical padding large enough to avoid clipping the text baseline.

#### Scenario: Individual settings checkbox has clipping-safe dimensions

GIVEN the operator opens the item format settings panel
WHEN the `Use Individual Settings` checkbox is visible
THEN the checkbox SHALL use the settings checkbox style with explicit minimum height and vertical content alignment.
