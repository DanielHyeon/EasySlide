## wpf-frmmain-inline-source-tabs

Date: 2026-06-04

Task: Restore FrmMain-style inline PowerPoint and Media source tabs in the WPF main shell.

CodeGraph checks:

- `codegraph_status`: healthy index, 508 files, 28063 nodes, 50729 edges.
- `codegraph_context`: WPF source-browser parity centers on `MainWindow`, `WorshipListPanel`, `PowerPointLibraryViewModel`, and `MediaLibraryViewModel`.
- `codegraph_impact MainWindow`: UI shell impact only; main touched surface is `Easislides.Wpf/MainWindow.xaml(.cs)`.
- `codegraph_impact PowerPointLibraryViewModel`: existing VM and library tests; no VM behavior change planned.
- `codegraph_impact MediaLibraryViewModel`: existing VM and library tests; no VM behavior change planned.
- `codegraph_explore`: `WorshipListPanel` already accepts `DataFormats.FileDrop` and inserts through `MainViewModel.AddExternalFilesRelativeTo`, so inline PowerPoint/Media source lists can reuse the same drop contract.

Implementation boundary:

- Add inline WPF source tabs and code-behind gesture wiring only.
- Reuse existing PowerPoint/Media services and view models.
- Do not touch SQLite, Office interop, media playback, or WinForms `FrmMain`.

## wpf-frmmain-inline-infoscreen-source

Date: 2026-06-04

Task: Restore FrmMain-style inline `InfoScr` source tab in the WPF main shell.

CodeGraph checks:

- `codegraph_context`: saved information screen support is centered on `InfoScreenStore`, `NoticeScreenViewModel`, `MainWindow`, `WorshipListPanel`, and `MainViewModel`.
- `codegraph_explore`: `InfoScreenStore` already exposes `ListNames`, `LoadAsync`, `SaveAsync`, and `Delete`; `NoticeScreenViewModel` maps saved DTO data into `NoticeOptions`.
- `codegraph_explore`: legacy `FrmMain` uses `InfoScreenList_MouseDoubleClick`, `InfoScreenList_KeyUp`, and list selection handlers for saved InfoScreen workflows.
- `codegraph_impact InfoScreenStore`: existing store consumers are `NoticeScreenViewModel` and store-focused tests, so the new source VM can consume it without changing persistence.
- `codegraph_impact MainViewModel/AddTextItem`: queue insertion is shared behavior; add a narrow notice insertion helper rather than changing external file, Bible, or output flows.
- `codegraph_impact MainWindow`: touched surface is source-browser XAML/code-behind wiring.

Implementation boundary:

- Add inline WPF `InfoScr` tab with lazy load, add button, double-click add, and drag source support.
- Reuse `InfoScreenStore` and `NoticeOptions`.
- Extend `WorshipListPanel` only to accept `InfoScreenSelection` drops and insert a Notice item at the target position.
- Do not touch SQLite, Office interop, media playback, output coordinates, or legacy WinForms `FrmMain`.

## wpf-legacy-working-folder-autodetect

Date: 2026-06-04

Task: Make WPF data loaders discover the installed legacy `C:\EasiSlides` working folder when WPF settings are still on the untouched default.

CodeGraph checks:

- `codegraph_context`: Worship List and Bible gaps converge on `SettingsService`, `WorshipListStore`, `BibleViewModel`, and `BibleRepository`.
- `codegraph_explore`: `WorshipListStore.LegacyDirectory()` reads `_settings.Current.General.WorkingFolder\Admin\WorshipLists`, while `BibleRepository.GetVersions()` reads `workingFolder\Admin\Database\EsBiblesList.db` and `workingFolder\HolyBibles`.
- `codegraph_search WorkingFolder`: default WPF key is `Documents\EasiSlides`; WPF tests already cover explicit working-folder paths for Bible and legacy Worship List loading.
- `codegraph_impact WorkingFolder`: broad shared settings surface, so the change is restricted to default/runtime settings resolution and preserves explicit custom paths.

Implementation boundary:

- Add an optional legacy working-folder candidate to `SettingsServiceOptions`.
- Apply the candidate only when the current working folder is the untouched WPF default and the candidate directory exists.
- Preserve custom/imported paths.
- Do not change Bible SQL, Worship List XML parsing, SQLite schema, output coordinates, Office interop, or WinForms.
