## Why

Building each preview or output card reparses the entire song. A sequence with repeated verses amplifies this cost on every card refresh.

## What Changes

Prepare the display pages once per card list and use indexed access to fill the cards.

## Capabilities

### New Capabilities

- `lyrics-page-card-generation` covers preparation of page bodies for the operator panels.

### Modified Capabilities

## Impact

WPF operator preview and output panels, the shared output text selection and focused regression tests.
