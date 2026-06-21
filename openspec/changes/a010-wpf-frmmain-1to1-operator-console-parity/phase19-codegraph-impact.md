# Phase 19 CodeGraph Impact

## Structural Evidence

CodeGraph context was used for the Output top-icon flow:

- `MainViewModel.StartOutputLiveAsync`
- `MainViewModel.CanStartOutputLive`
- `MainViewModel.ToggleOutputBlackCommand`
- `MainViewModel.ToggleOutputClearCommand`
- `MainViewModel.CanUseLiveSafetyAction`
- `WpfLiveSafetyPrompt.ConfirmAsync`
- `SafetyConfirm.AskAsync`

## Impact Surface

- `MainViewModel.CanStartOutputLive`: removed the `_output.Current.IsOpen` prerequisite so `cbGoLive` can be enabled once an Output item is prepared, even before the output window exists.
- `MainViewModel.StartOutputLiveAsync`: opens the Output window on demand before publishing the prepared Output item.
- `LiveSafetyPrompt.ConfirmAsync`: prefers the current mouse/keyboard focused element as the safety confirmation anchor and falls back to the main window.
- `SafetyConfirm.AskAsync`: changes popup placement from bottom to center to keep the safety card on-screen for maximized windows and automation paths.
- `SafetyConfirm.BuildCard`: adds UIA names for confirm/cancel buttons.
- `MainViewModelTests`: adds regression coverage that `Go Live` is enabled while the Output window is closed and that `Black/Clear` become executable after Live starts.

## Non-Impact / Guardrails

- No DB access path changed.
- No Office Interop path changed.
- No lyrics rendering or PowerPoint thumbnail generation changed.
- `Black/Clear` safety semantics remain gated to `LiveState.Active` or `LiveState.Hidden`; they are not enabled before live output starts.
- Existing live safety confirmation policy remains in place; only its visible placement/automation affordance changed.

## Validation

- Focused Output/live safety tests passed.
- Full WPF test suite passed: 2427 passed, 0 failed.
- WinForms build passed with existing warnings.
- OpenSpec strict validation passed.
- Release publish to `C:\EasiSlides\EasislidesNext` passed.
