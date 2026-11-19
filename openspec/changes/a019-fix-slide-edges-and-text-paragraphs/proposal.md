## Why

The upper PowerPoint views cover slide edges with selection marks and slide numbers. A text file prepared with a blank line between slides can also advance halfway through a paragraph when the configured font is large.

## What Changes

Fit the full slide inside the thumbnail frame and provide a saved Show option to keep each external text paragraph together on screen.

## Capabilities

### New Capabilities

- `slide-thumbnail-content` covers visibility of slide edges in the upper preview and output lists.
- `text-file-paragraph-slides` covers page breaks and font fitting for external text files.

## Impact

WinForms thumbnail painting, text pagination, Options and regression tests.
