## ADDED Requirements

### Requirement: Legacy Worship Lists Must Be Visible

The WPF main shell SHALL list legacy `.esw` Worship Lists from the configured working folder's `Admin\WorshipLists` directory in the saved Worship List selector.

#### Scenario: Legacy list exists under working folder

- **GIVEN** the configured working folder is `C:\EasiSlides`
- **AND** `C:\EasiSlides\Admin\WorshipLists\1.주일예배.esw` exists
- **WHEN** the WPF Worship List selector refreshes saved names
- **THEN** `1.주일예배` appears as a selectable Worship List

### Requirement: Legacy Worship Lists Must Load Into Queue

Selecting a legacy `.esw` Worship List SHALL load its parsed items into the WPF live queue through the existing legacy mapper.

#### Scenario: Legacy list is selected

- **GIVEN** a legacy `.esw` file contains song, PowerPoint, Bible, or text items
- **WHEN** the operator selects that list and invokes load
- **THEN** the current WPF queue is replaced with corresponding live queue items
- **AND** DB song items use loaded library details when available

### Requirement: Bible Must Be Ready From Main Shell Startup

The WPF main shell SHALL load Bible versions/books on startup so Bible browsing is available without requiring a delayed tab-selection side effect.

#### Scenario: Bible data exists in working folder

- **GIVEN** the configured working folder contains `HolyBibles` and `Admin\Database\EsBiblesList.db`
- **WHEN** the WPF main shell finishes loading
- **THEN** Bible versions and books are loaded once

### Requirement: Worship List Drop Must Accept Legacy Lists

The Worship List panel SHALL accept dropped `.esw` files and import them as legacy Worship Lists.

#### Scenario: Operator drops `.esw` file onto Worship List

- **GIVEN** the operator drags a legacy `.esw` file onto the Worship List
- **WHEN** the file is dropped
- **THEN** the parsed Worship List is loaded or merged into the queue without crashing
