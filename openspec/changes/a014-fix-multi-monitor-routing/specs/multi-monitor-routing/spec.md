# Spec Delta

## Purpose

Keep the audience, operator and stage windows on their intended displays across changes in desktop topology, display scale and media selection.

## ADDED Requirements

### Requirement: WinForms monitor placement

WinForms SHALL restore visible operator bounds, honor live media monitor choices and convert custom PowerPoint placement consistently.

#### Scenario: Signed desktop coordinates
- **WHEN** the operator reopens a window saved on a connected secondary screen
- **THEN** the window is restored within that screen's working area, including negative coordinates.

#### Scenario: Media routing and custom slides
- **WHEN** a live item selects a connected media display
- **THEN** playback uses that display. A missing choice falls back to the session or audience display. Custom PowerPoint positions and sizes use the same unit conversion in single and dual modes.

### Requirement: Independent WPF audience and stage placement

WPF SHALL honor explicit display preferences, use separate default screens when available, and place both windows in desktop pixel coordinates.

#### Scenario: Three screens with different scales
- **WHEN** an operator opens the audience and stage windows
- **THEN** the default stage uses a different available screen and its bounds are independent of custom audience bounds and monitor scaling.

#### Scenario: Disconnect and reconnect
- **WHEN** a selected monitor disappears and later returns during the session
- **THEN** both selections reflect the available screens and return to the preferred devices without persisting temporary fallbacks.
