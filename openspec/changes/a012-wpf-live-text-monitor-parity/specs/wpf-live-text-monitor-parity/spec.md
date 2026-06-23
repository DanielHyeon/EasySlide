## ADDED Requirements

### Requirement: WPF Live click shall publish text monitor body
WPF MainWindow SHALL publish display-ready text into the output text monitor scene when the operator clicks Live on a text-based Preview item.

#### Scenario: Song live click updates output text monitor body
- **GIVEN** a song Preview item with multiple lyric pages
- **WHEN** the operator clicks the WPF Live button
- **THEN** the output window SHALL be open fullscreen
- **AND** the live session body SHALL contain the current preview lyric page
- **AND** the output text monitor view model SHALL show that same body

### Requirement: Text monitor body shall follow WinForms display semantics
WPF text monitor body calculation SHALL preserve the WinForms display semantics for Song, Bible, Notice, and external text file items.

#### Scenario: Notice live click keeps full notice text
- **GIVEN** a Notice item with plain multiline text
- **WHEN** the operator clicks the WPF Live button
- **THEN** the output text monitor SHALL show the full notice body rather than parsing it as song notation

