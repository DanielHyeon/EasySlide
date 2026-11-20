## 1. WinForms

- [x] 1.1 Restore operator bounds and correct PowerPoint custom coordinates.
- [x] 1.2 Route live media to the resolved item, session or audience display.

## 2. WPF

- [x] 2.1 Refresh both display lists on topology changes and restore preferred devices.
- [x] 2.2 Separate stage bounds and apply pixel-accurate placement to both windows.

## 3. Verification

- [x] 3.1 Run regression tests, validate the change and compare registry fingerprints.

## Verification results

- Release suite passed with 198 WinForms tests, 2,472 WPF tests and 20 analyzer tests.
- An isolated Windows process decoded a seven-second silent AVI through the live DirectShow path on the secondary monitor, observed the playback position advance and moved playback to the primary monitor.
- The actual WPF audience and stage window handles each matched the native bounds of both connected displays (3440×1440 and 2560×1440). Reconnect and differing DPI were exercised with simulated topology. PowerPoint placement was tested without Office.
- Both registry views of the production EasiSlides key had identical SHA-256 fingerprints before and after verification.

- [x] Exclude the operator's screen from automatic WinForms lyrics fallback. With the main form on the right-hand display and both saved monitor choices pointing to the left, pausing standalone media and sending a Worship List PPT kept the live PowerPoint window on the left and the main form accessible on the right. Unit tests also cover a spare third screen and explicit lyrics selection.
