## ADDED Requirements

### Requirement: Classic operator first screen

The WPF main window SHALL use the Classic Operator Layout as the default first screen for live worship operation.

#### Scenario: Baseline window size

- **GIVEN** the WPF main window opens at the default size
- **WHEN** the operator views the first screen
- **THEN** the window uses an 1180x760 baseline
- **AND** the content browser, worship list, preview area, output state, live operation controls, and status bar are visible without opening a modal.

### Requirement: Persistent live operation controls

The WPF main window SHALL keep core live operation controls visible even when the right inspector is collapsed.

#### Scenario: Core live controls stay visible

- **GIVEN** the operator collapses the right inspector
- **WHEN** the operator looks at the fixed operation bar
- **THEN** Go Live, send-and-next, Stop Live, Black, Clear, Hide, Restore, Restart, Refresh, Open Output, and Close Output remain visible or wrap within the first screen.

### Requirement: Stop Live stays directly reachable

The WPF main window SHALL expose Stop Live on the fixed operator bar, distinct from Close Output.

#### Scenario: Operator stops live output

- **GIVEN** live output is active
- **WHEN** the operator needs to stop the live session
- **THEN** the fixed operator bar includes a Stop Live button bound to `StopLiveCommand`
- **AND** Close Output remains a separate command bound to `CloseOutputCommand`.

### Requirement: Preview and Output state separation

The WPF main window SHALL visually distinguish the selected Preview item from the currently live Output item.

#### Scenario: Selected item differs from live item

- **GIVEN** a live item is already being output
- **AND** the operator selects a different worship list item for preview
- **WHEN** the operator checks the central header
- **THEN** the Preview section shows the selected item
- **AND** the Output section shows the live state, live item title, and output display.

### Requirement: Binding mode safety

The WPF main window SHALL explicitly declare binding modes for checkbox and toggle checked state bindings.

#### Scenario: XAML binding scan

- **GIVEN** production WPF XAML files are scanned by `XamlBindingModeTests`
- **WHEN** an `IsChecked` binding exists
- **THEN** it declares an explicit `Mode`.
