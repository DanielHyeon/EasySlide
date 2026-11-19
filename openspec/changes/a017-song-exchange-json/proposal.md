## Why

Song imports can inherit another item's values or stop at empty XML items. Large previews allocate lyrics and images only to count folders. Operators also need a portable JSON song exchange format.

## What Changes

- Read XML folder summaries sequentially and isolate imported item fields.
- Add JSON song exchange to WinForms and WPF using the same file contract and preserve legacy file support.
- Match selected import folders by full name.

## Capabilities

### New Capabilities

- `song-exchange` covers previewing, importing and exporting song collections across the desktop clients.

### Modified Capabilities

## Impact

Shared Core format code, the WinForms and WPF import/export dialogs, and regression tests using temporary data.
