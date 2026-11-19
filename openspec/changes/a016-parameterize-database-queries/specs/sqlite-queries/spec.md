## ADDED Requirements

### Requirement: Bound query inputs
Database queries SHALL keep input text separate from executable SQL and handle selectable schema names as individual identifiers.

#### Scenario: Quotes in song data
- **WHEN** an operator searches for or imports a title containing quotes
- **THEN** its value is bound with the same match rules as an ordinary title

### Requirement: Efficient list and batch operations
Song listing and smart merge SHALL avoid repeated queries for the same operation, and a selected usage deletion SHALL commit atomically.

#### Scenario: A merge with duplicate titles
- **WHEN** matching source and target folders contain several matching rows
- **THEN** all matching pairs are returned in one database query

#### Scenario: A usage deletion cannot finish
- **WHEN** any selected row cannot be deleted because database execution fails
- **THEN** the transaction rolls back the batch

### Requirement: Whole-word Bible matching
Whole-word Bible search SHALL recognize verse edges and punctuation and bind each nonempty search term once.

#### Scenario: Quoted text and repeated spaces
- **WHEN** the operator searches for all words, any word or a phrase containing an apostrophe
- **THEN** SQLite applies that match mode within the selected book and excludes substrings of longer words in whole-word mode
