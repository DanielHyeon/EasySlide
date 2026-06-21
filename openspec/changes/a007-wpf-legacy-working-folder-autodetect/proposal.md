## Why

WPF data loaders already know how to read legacy worship lists and Bible databases, but they depend on the configured working folder. The current WPF default points to `Documents\EasiSlides`, while existing FrmMain installations commonly use `C:\EasiSlides`.

When the app starts with the untouched WPF default, operators can see an empty Worship List/Bible experience even though the real data is present under `C:\EasiSlides`.

## What Changes

- Prefer an existing legacy `C:\EasiSlides` root when the WPF settings are still on the untouched default working folder.
- Preserve any custom working folder the user already selected or imported.
- Keep `SettingsServiceOptions` injectable so tests can use a temporary legacy root instead of touching `C:\EasiSlides`.
- Verify Bible and Worship List loaders receive the corrected working folder through existing view model paths.

## Impact

- Main file: `Easislides.Wpf/Settings/SettingsService.cs`.
- Focused tests: settings default/fallback, BibleViewModel working folder, WorshipList legacy name import.
- No SQLite schema, WinForms, output coordinates, or Office interop changes.
