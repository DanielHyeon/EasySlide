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
