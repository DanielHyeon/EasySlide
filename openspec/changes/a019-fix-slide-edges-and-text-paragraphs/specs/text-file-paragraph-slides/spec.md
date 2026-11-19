## Purpose

Operators prepare external worship texts with blank lines marking the page boundaries for projection.

## ADDED Requirements

### Requirement: Optional paragraph fitting for text files

The Show settings SHALL persist a default-enabled option to fit each external text paragraph on one slide, with ordinary line breaks retained and consecutive blank lines treated as one separator.

#### Scenario: Multi-line paragraph with automatic overflow enabled

- **WHEN** the option is enabled and the original font is too large for a paragraph
- **THEN** preview and output fit all its lines onto the same slide.

#### Scenario: Restore ordinary overflow

- **WHEN** the operator disables paragraph fitting
- **THEN** external text uses the existing overflow rules and the preference survives restart.
