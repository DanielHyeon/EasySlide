## Design

### Legacy Worship List Discovery

`WorshipListStore` remains the JSON persistence store for new WPF lists, but gains optional awareness of `ISettingsService`.

When settings are available, `ListNames()` returns the union of:

- existing WPF JSON names from `%AppData%/EasislidesNext/WorshipLists`;
- legacy `.esw` file names from `<WorkingFolder>\Admin\WorshipLists`.

JSON keeps precedence when the same name exists in both stores.

### Legacy Worship List Loading

`WorshipListStore` exposes a small secondary interface for legacy `.esw` reads without changing the primary `IWorshipListStore` contract. `MainViewModel.LoadWorshipListAsync` first checks whether the selected name resolves to a legacy `.esw`; if so, it reads/parses that file and calls the already-tested `ImportEswWorshipList` mapper. That preserves the richer mapping that can use `Library.Songs` for DB song lyrics.

### Startup Bible Load

`MainWindow.Loaded` calls a new `EnsureBibleLoadedOnce()` alongside `EnsureLibraryLoadedOnce()`. The existing tab selection handler still calls the same guard, so repeated loads are avoided.

### Drag And Drop

`WorshipListPanel` already accepts `LiveQueueItem`, `BibleSelection`, `SongSummary`, and file drops. The file-drop branch will treat `.esw` as a legacy Worship List import/merge source and keep PPT/media files as inserted external items.

## Risks

- `MainViewModel` has a large impact surface. The change is constrained to Worship List load/drop helpers and uses existing parser/mapping logic.
- Legacy `.esw` may reference files outside `C:\EasiSlides`. The import should still show items; validation can flag missing files without crashing.
