## Why

The prior WPF slices made several FrmMain capabilities present, but the main shell is still not operationally usable: core worship-list and Bible flows are hidden behind modal/selection details or require unintuitive text-range gestures. This change focuses on first-screen operator usability, not merely feature existence.

## What Changes

- Make saved Worship Lists loadable from the first-screen Worship List selector with explicit keyboard/mouse gestures in addition to the existing load button.
- Make Bible add-to-Worship-List use the current selected Bible passage when no text range is selected, so typed-reference/search/book selection flows can add without an obscure text selection step.
- Keep existing drag/drop and explicit button behavior intact.
- Add regression tests that describe the recovered operator flows.

## Capabilities

### New Capabilities
- `wpf-frmmain-operational-usability`: First-screen usability requirements for WPF FrmMain parity flows that must be usable during live worship operation.

### Modified Capabilities

## Impact

- WPF main shell code-behind: Bible add and typed-reference selection handling.
- WPF Worship List panel code-behind/XAML: first-screen saved-list load gestures.
- WPF Bible view model: current-selection fallback behavior, if needed.
- WPF tests: focused regression coverage for Worship List loading gestures and Bible add fallback.
