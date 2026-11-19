## Purpose

Exchange song collections between WinForms and WPF while keeping large library previews inexpensive.

## ADDED Requirements

### Requirement: Portable song collections
Both desktop clients SHALL import and export UTF-8 JSON arrays with titles, folders, lyrics, notations, sequence, numbers, rights, references and formatting settings.

#### Scenario: Round trip between clients
- **WHEN** an operator exports selected songs and imports them into a destination folder
- **THEN** the song order and supported metadata survive and the selected duplicate policy applies

### Requirement: Independent XML song records
XML song imports SHALL skip empty items, reset omitted fields and match selected folders by full name.

#### Scenario: Sparse items and overlapping folder names
- **WHEN** an empty item separates two songs and the second omits fields
- **THEN** processing continues with fresh values and only selected folder names match

### Requirement: Streaming previews
The desktop clients SHALL count folders without materializing song bodies during XML and JSON preview.

#### Scenario: Preview a large library
- **WHEN** a song file contains long lyrics and embedded images
- **THEN** the preview reports the song count and source folders and reports malformed input

### Requirement: Legacy text export line endings
WinForms SHALL emit each CRLF, LF or CR lyric line break as one CRLF in UTF-16LE text exports with a byte-order mark.

#### Scenario: Export lyrics with mixed line endings
- **WHEN** the lyrics contain mixed line endings, consecutive blank lines or a trailing newline
- **THEN** the exported song preserves the line count and text, using a single CRLF for each input line break
