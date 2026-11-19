## Why

Imported titles, references and saved song identifiers reach SQL via string concatenation. Listing and merging songs also repeat reads of the same data.

## What Changes

Bind values at the SQLite command boundary, quote dynamic schema identifiers, and reduce redundant reads and per-row database work.

## Capabilities

### New Capabilities

- sqlite-queries covers command construction, query execution and atomic usage deletion.

## Impact

WinForms search, import, merge, list and usage workflows and WPF folder reads.
