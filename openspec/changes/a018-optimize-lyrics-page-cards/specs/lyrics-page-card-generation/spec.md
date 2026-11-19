## Purpose

Prepare WPF preview and output cards efficiently while keeping operator navigation and displayed verse text aligned.

## ADDED Requirements

### Requirement: Reuse projected pages within a card list
The operator panels SHALL reuse the prepared display pages across all cards for the current item and settings.

#### Scenario: Repeated chorus or verse
- **WHEN** a sequence presents a verse multiple times
- **THEN** every card displays the corresponding body and label without reparsing the entire item per card

#### Scenario: Change presentation settings
- **WHEN** a card list is rebuilt with a new transpose or notation setting
- **THEN** cards reflect the new settings and retain the selected page index
