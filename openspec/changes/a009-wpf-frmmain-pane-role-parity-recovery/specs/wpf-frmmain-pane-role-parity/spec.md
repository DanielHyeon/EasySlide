## ADDED Requirements

### Requirement: FrmMain Source Pane Roles
The WPF main shell SHALL expose FrmMain source-browser roles on the first screen using bottom-aligned tabs named `Folders`, `InfoScr`, `PowerPoint`, `Bibles`, `Images`, `Media`, and `Default`.

#### Scenario: Source tabs are visible in FrmMain order
- **WHEN** the WPF main shell is loaded
- **THEN** the left upper source pane lists the `Folders`, `InfoScr`, `PowerPoint`, `Bibles`, `Images`, `Media`, and `Default` tabs in that order
- **AND** the source tab strip is bottom-aligned like FrmMain

#### Scenario: Open the Bible browser with one installed translation
- **WHEN** the operator selects Bibles with a single installed translation
- **THEN** the book selector and passage are available without a translation strip or a verse-number toggle above the source tabs
- **AND** with multiple translations the operator can switch versions and read the same book when available

### Requirement: FrmMain Lower Left List Roles
The WPF main shell SHALL expose `Worship List` and `Praise Book` as lower-left peer tabs on the first screen.

#### Scenario: Praise Book is available beside Worship List
- **WHEN** the WPF main shell is loaded
- **THEN** the lower-left pane contains a bottom-aligned tab strip with `Worship List` and `Praise Book`
- **AND** the `Praise Book` tab provides a first-screen flow for opening a saved Praise Book and adding entries to the Worship List

#### Scenario: Select a song in the worship list
- **WHEN** the operator selects a song or switches between Worship List and Praise Book
- **THEN** the lower tab strip uses the same bottom tab styling as the source strip and the list has no inline sequence editor
- **AND** the song sequence remains editable in the song editor
- **AND** Praise Book groups manage, add and sort beside the book selector and places delete, RTF export and HTML export down the right edge of its list

### Requirement: FrmMain Right Preview Roles
The WPF main shell SHALL keep the right upper thumbnail pane and right lower large output preview pane as distinct roles.

#### Scenario: Right output panes stay separated
- **WHEN** the WPF main shell is loaded
- **THEN** the right upper pane contains the output thumbnail/list region
- **AND** the right lower pane contains the large output preview region

#### Scenario: Operator compares prepared and live slides
- **WHEN** the operator resizes the console or drags the divider between Preview and Output
- **THEN** each upper PowerPoint strip fits one to three slides per row according to its visible width
- **AND** a single live image uses the available upper output pane above its caption
- **AND** the lower Preview and Output panes each display the full 4:3 or 16:9 slide with its shadow outside the image and text area

#### Scenario: Read an unformatted creed text file in Preview and Output
- **WHEN** a text file has indented lines and no individual font override
- **THEN** Preview and Output preserve its indentation and use the projection font, margins and line spacing of the last previewed song folder, falling back to the first song folder on startup
- **AND** upper text lists use the operator preview font setting independently of the standard 12 DIP console font

#### Scenario: Select another paragraph from a source preview
- **WHEN** the operator clicks a different paragraph in the upper text preview with the Worship List empty or another item selected
- **THEN** the selection and lower sample show the clicked page while the audience stays on its current page until the operator sends it

#### Scenario: Open a left list context menu
- **WHEN** the operator opens the Folders, Bible, Images, Worship List or Praise Book list menu
- **THEN** its visible commands, groups, blank icon gutter and selected title follow the corresponding WinForms menu
- **AND** the Worship List enables playback for a single media row, and clearing a named list requires confirmation before moving the saved file to Trash
- **AND** Bible Add & Show adds and sends the selected passage while Add Region 2 previews the secondary translation
- **AND** clicking the right text pane selects a page and clicking the sample background leaves the selection unchanged
