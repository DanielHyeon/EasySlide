## ADDED Requirements

### Requirement: WPF first screen matches FrmMain operator geometry
The WPF main shell SHALL present the same first-screen operator geometry as legacy `FrmMain`: source/list on the left, Preview in the middle, and Output on the right.

#### Scenario: Three operator panes are present
- **WHEN** `MainWindow.xaml` is inspected
- **THEN** it contains stable `ClassicSourcePane`, `ClassicPreviewPane`, and `ClassicOutputPane` regions

### Requirement: Preview and Output remain simultaneously visible
The WPF main shell SHALL show Preview and Output text/list areas and slide surfaces at the same time.

#### Scenario: Preview and Output split surfaces are visible
- **WHEN** the default shell is rendered
- **THEN** Preview and Output each have a top work pane and a bottom slide pane in the first screen layout

### Requirement: Local pane command strips stay attached to Preview and Output
The WPF main shell SHALL keep core live operation controls adjacent to Preview and Output panes.

#### Scenario: Preview and Output command strips expose live controls
- **WHEN** the XAML command strips are inspected
- **THEN** Preview exposes send/navigation controls and Output exposes Black/Clear/Restore/navigation controls
