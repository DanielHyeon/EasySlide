## ADDED Requirements

### Requirement: WPF shall prefer an existing legacy working folder for untouched defaults

When WPF settings are missing or still use the untouched default working folder, the app SHALL use an existing legacy `C:\EasiSlides` root as the runtime working folder so legacy Bible databases and Worship Lists are discoverable.

#### Scenario: Missing settings and legacy root exists

- **GIVEN** no WPF settings file exists
- **AND** the configured legacy working-folder candidate exists
- **WHEN** the WPF settings service is created
- **THEN** `general.workingFolder` is the legacy candidate path

#### Scenario: Stored default path and legacy root exists

- **GIVEN** a WPF settings file stores the untouched default working folder
- **AND** the configured legacy working-folder candidate exists
- **WHEN** the WPF settings service is created
- **THEN** `general.workingFolder` is the legacy candidate path

#### Scenario: Custom working folder is stored

- **GIVEN** a WPF settings file stores a custom working folder
- **AND** the configured legacy working-folder candidate exists
- **WHEN** the WPF settings service is created
- **THEN** `general.workingFolder` remains the stored custom path
