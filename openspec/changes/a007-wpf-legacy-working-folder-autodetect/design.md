## Design

Add a runtime working-folder fallback at the settings-service boundary.

`SettingsServiceOptions.CreateDefault()` supplies the real legacy candidate, `C:\EasiSlides`. Test code can pass another candidate path through the optional `LegacyWorkingFolderPath` option.

When a `SettingsService` instance is created or defaults are restored:

1. Build the normal default/loaded snapshot.
2. If no legacy candidate exists, keep the snapshot.
3. If the snapshot working folder is not the untouched WPF default, keep it.
4. If the snapshot working folder is the untouched WPF default and the legacy candidate exists, replace it with the normalized legacy root.

This keeps user/imported custom paths authoritative while making a clean WPF installation line up with the installed FrmMain data root.

## Non-Goals

- Do not auto-create `C:\EasiSlides`.
- Do not scan arbitrary drives for data roots.
- Do not change BibleRepository, WorshipListStore, or legacy XML parsing.
- Do not overwrite custom settings paths.
