## ADDED Requirements

### Requirement: FrmMain live shortcuts remain catalog-backed
The WPF shell SHALL define FrmMain live-operation shortcuts in the command catalog, not only as menu hint text.

#### Scenario: Core live shortcuts are present
- **WHEN** the default WPF command catalog is inspected
- **THEN** Go Live, send-and-next, next item, previous item, Black, Clear, Restart, Refresh, and Help have the expected default shortcut definitions

### Requirement: Menu shortcut hints match real shortcuts
The WPF main menu SHALL display shortcut hints only when the hinted gesture exists in the command catalog.

#### Scenario: Output menu hints stay synchronized
- **WHEN** an Output menu item displays an `InputGestureText`
- **THEN** that gesture matches a default shortcut in the command catalog for the same command

### Requirement: Local and global navigation shortcuts remain distinct
The WPF shortcut registry SHALL keep local UI shortcuts and global live-navigation shortcuts distinct by key and modifier.

#### Scenario: F5 and Ctrl+F5 do not collide
- **WHEN** next item uses F5 and output refresh uses Ctrl+F5
- **THEN** both shortcuts can coexist without registry collision

### Requirement: WPF show keys follow the active output

WPF SHALL route WinForms show keys to the running output, including when PowerPoint owns keyboard focus.

#### Scenario: End an active show
- **WHEN** the operator presses Esc in the console, output window, or a foreground external application during a show
- **THEN** the live session and its native slide show end once without a confirmation
- **AND** an idle session leaves Esc available to the focused application

#### Scenario: Use Space during worship
- **WHEN** the operator presses Space in the lyrics output, the active PowerPoint slide show, or the output file player
- **THEN** the corresponding output advances one lyrics page, advances one native PowerPoint click, or toggles file playback respectively
- **AND** the PowerPoint slide number follows the native presentation after a slide change
- **AND** typing a space in the console search field preserves the text input

#### Scenario: Apply WinForms global key options
- **WHEN** global F7 through F10 or global arrows are enabled
- **THEN** F7 copies preview and clears black, F8 copies preview, F9 and F10 toggle black, and the enabled arrows move output on release
- **AND** toggling the options after migration updates the active bindings while preserving unrelated custom chords

#### Scenario: Open an existing or closed PowerPoint deck
- **WHEN** a PowerPoint show starts for an already open deck
- **THEN** its editor minimizes, other presentations retain their window state, and ending the show restores the editor
- **AND** thumbnail or caption rendering can read the open deck without closing or activating its editor
- **AND** presenting a closed file opens the show without an editor window
