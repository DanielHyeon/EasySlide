# FrmMain to WPF 1:1 Mapping Table

작성일: 2026-06-04

상태 값:

- `implemented`: 기본 동작과 검증 경로가 존재한다.
- `partial`: 일부 UI 또는 일부 동작만 존재한다.
- `missing`: WPF에서 아직 사용할 수 없다.
- `defer`: 1:1 이후 후순위로 미룬다. 이유가 필요하다.

## 1. Shell Regions

| FrmMain source | WPF target | Status | Phase | Notes |
| --- | --- | --- | --- | --- |
| `toolStripContainerMain` | `MainWindow` root grid/menu/status | partial | 1 | menu/status exist, toolbar parity incomplete |
| `Main_NoRotate`, `Main_RotateStyle`, `Main_Rotate0..3` | `ToggleAutoRotateCommand`, `AutoRotateModeInput`, `AutoRotateModeOptions` | partial | 4/6 | auto-rotation now advances the live Output item rather than the selected Preview item, including diverged Preview selection; exact FrmMain toolbar icon/check/dropdown parity still incomplete |
| `splitContainerMain.Panel1` | `ClassicSourceColumn` / `ClassicSourcePane` (`Tag=splitContainer1`) | partial | 1 | source/list column now scales by splitter-like proportion; behavior/data parity incomplete |
| `splitContainerMain.Panel2` | `ClassicRightColumn` / `ClassicRightConsole` (`Tag=splitContainer2`) | partial | 1 | Preview/Output are now nested under a right splitContainer2-equivalent Grid; exact behavior incomplete |
| `splitContainer1.Panel1` | `LeftBrowserTabs` (`Tag=tabControlSource`) | partial | 1/2/3 | source tabs exist, several are shallow |
| `splitContainer1.Panel2` | `LeftListTabs` (`Tag=tabControlLists`) | partial | 1/2/3 | Worship List visible, Praise Book shallow |
| `splitContainer2.Panel1` | `ClassicPreviewColumn` | partial | 1/4 | Preview column now lives inside `ClassicRightConsole`; Preview slide/lyrics navigation is separated from live Output; legacy controls incomplete |
| `splitContainer2.Panel2` | `ClassicOutputColumn` | partial | 1/4 | Output column now lives inside `ClassicRightConsole`; live PPT/lyrics navigation and position labels use independent Output state; non-PPT live thumbnail/control parity incomplete |
| `splitContainerPreview.Panel1` | `ClassicTopControlRow` + `ClassicPreviewPane` (`Tag=splitContainerPreview.Panel1`) | partial | 4/5 | top row is named and tagged; needs exact item/slide/section/format controls |
| `splitContainerPreview.Panel2` | `ClassicBottomScreenRow` + `ClassicPreviewSlidePane` (`Tag=splitContainerPreview.Panel2`) | partial | 4 | bottom row is named and tagged; Preview PPT/lyrics navigation no longer mutates live Output; background/frame sizing parity still incomplete |
| `splitContainerOutput.Panel1` | `ClassicTopControlRow` + `ClassicOutputPane` (`Tag=splitContainerOutput.Panel1`) | partial | 4/5 | top row is named and tagged; live PPT thumbnails, Output slide buttons, Output lyrics buttons, and Output position labels now use OutputItem state; non-PPT live thumbnails/status still incomplete |
| `splitContainerOutput.Panel2` | `ClassicBottomScreenRow` + `ClassicOutputSlidePane` (`Tag=splitContainerOutput.Panel2`) | partial | 4 | bottom row is named and tagged; large Output PPT screen uses independent OutputItem state and stays separate from Preview; exact sizing/frame parity still incomplete |

## 2. Source Browser Tabs

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `tabControlSource` | `LeftBrowserTabs` (`Tag=tabControlSource`) | partial | 1 | tab role is now explicit; enforce exact tab order and source behavior |
| `tabFolders` | `Folders` tab / `ClassicFoldersSourceGrid` | partial | 2/3 | compact FrmMain-style top strip now exists; context menu/edit commands still incomplete |
| `SongFolder` | `ClassicSongFolderCombo` (`Tag=SongFolder`) | partial | 2 | first-screen combo is mapped; legacy folder management still lives outside this strip |
| `SongsList` | `LibrarySongList` (`Tag=SongsList`) | partial | 3 | headerless `ListView/GridView`, selection, double-click, Enter add, and drag exist; full context menu/edit parity incomplete |
| `Folders_WordCount` | `ClassicFoldersWordCountMode` (`Tag=Folders_WordCount`) | partial | 5 | stroke-count sort is first-screen; exact check-button behavior still incomplete |
| `tabFiles` | `InfoScreenSourceTab` | partial | 2/3 | DI-backed store now reads WPF JSON plus legacy `C:\EasiSlides\InfoScreens\*.esi`; folder selector/list management/import/edit/copy/move/delete still incomplete |
| `InfoScreenFolder` | inline InfoScreen folder selector | partial | 2 | legacy root/subfolder `.esi` names load as relative names; exact FrmMain folder combo still missing |
| `InfoScreenList` | `InlineInfoScreenList` | partial | 3 | legacy `.esi` content loads into add/drag flow; double-click, Enter add, and drag insert exist; context menu/edit/manage parity incomplete |
| `tabPowerpoint` | `PowerPointSourceTab` | partial | 2/3/4 | inline folder combo, list, and preview-style flow exist; import/manage and real first-slide thumbnails still incomplete |
| `PowerpointFolder` | `PowerpointFolder` (`Tag=PowerpointFolder`) | partial | 2 | root `Powerpoint Items` plus subfolder groups load and selection reloads contents; import/manage/folder edit parity incomplete |
| `PowerpointList` | `InlinePowerPointList` (`Tag=PowerpointList`) | partial | 3/4 | selection, double-click, Enter add, and drag insert exist; context menu and exact legacy folder-list columns incomplete |
| `PP_ListType` | `PP_ListType` / `PP_ListStyle` / `PP_PreviewStyle` + `flowLayoutExternalPowerPoint` | partial | 4 | list/preview style toggle switches WPF list vs thumbnail-flow source surfaces; preview flow now requests cached first-slide renders and falls back to the PPT icon on render failure; exact FrmMain thumbnail sizing/cache folder behavior still incomplete |
| `tabBibles` | `Bibles` tab | partial | 2/3 | full version/book/reference/selection workflow |
| `BookLookup` | Bible book selector | partial | 2 | load legacy book list per version |
| `BibleUserLookup` | `BibleReferenceBox` | partial | 2/3 | direct reference and search validation parity |
| `Bibles_Go` | Bible go button | partial | 3 | invoke lookup/search exactly |
| `TabBibleVersions` | Bible versions UI | partial | 2 | version tabs/list with selected version state |
| `BibleText` | `BiblePassageBox` + `CMenuBible_*` | partial | 3 | selection, Enter add, FrmMain-named context menu, Add/Region2/Copy/InfoScreen, drag `BiblePassage`; exact rich text styling still incomplete |
| `tabImages` | `ImagesSourceTab` | partial | 2/5 | real folders/thumbnails exist; primary apply now follows FrmMain item-first/default-fallback image background behavior; thumbnail drag emits image `FileDrop` for Preview background drop; folder group parity still incomplete |
| `ImagesFolder` | image folder selector | partial | 2 | legacy image groups |
| `flowLayoutImages` | `InlineImagesList` (`Tag=flowLayoutImages`) | partial | 5 | thumbnails, double-click/Enter/Apply use FrmMain item-first/default-fallback background behavior; item/default context menu, refresh menu, and image-to-background drag exist; exact thumbnail sizing and folder-group UI still incomplete |
| `tabMedia` | `MediaSourceTab` | partial | 2/3 | real folders, import, double-click, drag insert |
| `MediaFolder` | media folder selector | partial | 2 | legacy media groups |
| `MediaList` | `InlineMediaList` | partial | 3 | double-click, Enter add, and drag insert exist; context menu parity incomplete |
| `tabDefault` | `DefaultSource` tab / inspector | partial | 5 | full `DefPanel` option parity |

## 3. Lower Left Lists

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `tabControlLists` | `LeftListTabs` (`Tag=tabControlLists`) | partial | 1 | tab role is now explicit; exact visual density and tab behavior still incomplete |
| `tabWorshipList` | `Worship List` tab | partial | 2/3 | real `.esw` load is wired; startup now loads the selected saved list when the queue is empty; DB song items now resolve lyrics by `SongId` from `C:\EasiSlides\Admin\Database\EasiSlidesDb.db`; all toolbar/context commands still incomplete |
| `SessionList` | `SessionCombo` (`Tag=SessionList`) | partial | 2 | compact first-screen combo exists; startup selects the first saved `C:\EasiSlides\Admin\WorshipLists` entry when no prior selection exists; exact legacy selection persistence still incomplete |
| `WorshipListItems` | `WorshipListPanel.QueueList` (`Tag=WorshipListItems`) | partial | 3/4 | headerless `ListView/GridView`, extended multi-select, startup default `.esw` load with DB-backed song lyrics, drag reorder, double-click Go Live, and context menu exist; `CMenuWorship_SelectAll/UnselectAll/Clear/Play/PlayOnOutput` now route to WPF handlers/commands; preview/edit/usage parity incomplete |
| `WL_Manage` | manage list command | partial | 3 | direct or WPF equivalent |
| `WL_Add` | `WL_Add` + `WorshipListPanel_AddSelectedSourceRequested` | partial | 3 | visible lower-left Add button now routes by active `LeftBrowserTabs` source for Folders, Bibles, InfoScreen, PowerPoint, Media, and Search; exact multi-select/source edge cases still incomplete |
| `WL_Open` | load selected/open file | partial | 2 | exact `.esw`/template flow |
| `WL_Up`, `WL_Down` | `ClassicWorshipListToolStrip2` move commands/drag reorder | partial | 3 | compact visible toolbar and keyboard paths exist; exact legacy vertical strip layout still incomplete |
| `WL_Delete` | remove selected | implemented | 3 | keep Delete key tests |
| `WL_Word`, `WL_Notes` | no full inline target | missing | 5/7 | export/notes decision needed |
| `CMenuWorship_*` | WPF context menu | partial | 3/4 | `Select All`, `Unselect All`, `Clear Worship List`, `Play Media`, and `Play Media on Output Monitor` are exposed with legacy names/order and wired; `Edit item` and `Add Songs to Usages` remain disabled placeholders |
| `tabPraiseBook` | `Praise Book` tab | partial | 2/3 | saved book load plus FrmMain-style item surface now visible; DI store now reads legacy `WorkingFolder\Admin\PraiseBooks\*.esp`; exact template management still incomplete |
| `PraiseBook` | `InlinePraiseBookSavedBooksCombo` (`Tag=PraiseBook`) | partial | 2 | saved PraiseBook names load from WPF JSON plus legacy `C:\EasiSlides\Admin\PraiseBooks\*.esp` via `IPraiseBookStore` |
| `PraiseBookItems` | `PraiseBookItems` (`Tag=PraiseBookItems`) | partial | 3 | flat headerless ListView opens legacy `.esp` items with `SongId` from `ItemID`; double-click/Enter add-to-Worship plus drag insert into Worship List now use DB-backed `SongId` resolution; exact preview-on-selection still incomplete |
| `PB_Manage`, `PB_Add`, `PB_Delete` | `PB_Manage`, `PB_Add`, `PB_Delete` | partial | 3 | manage window, add selected Folders song, delete selected rows are wired |
| `PB_Word`, `PB_Html`, `PB_WordCount` | `PB_Word`, `PB_Html`, `PB_WordCount` | partial | 5/7 | HTML/RTF export wired and toolbar column coverage now preserves `PB_Html`; WordCount button exposed but exact CJK word-count sorting still disabled |
| `CMenuPraiseB_*` | `CMenuPraiseB`, `CMenuPraiseB_SelectAll`, `CMenuPraiseB_UnselectAll`, `CMenuPraiseB_Clear`, `CMenuPraiseB_Edit` | partial | 3 | menu names/order/actions are wired; edit opens library context rather than direct editor |

## 4. Preview

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `PreviewPanelDisplayName` | `ClassicPreviewPanelDisplayName` (`Tag=PreviewPanelDisplayName`) | partial | 4 | title role is explicit; exact source/status columns still incomplete |
| `PreviewInfo` | `ClassicPreviewInfo` (`Tag=PreviewInfo`) | partial | 4/6 | selected lyrics/info surface is explicit; focused verse keys and Up/Down/Page/Space now route to Preview lyrics commands; source-specific content still incomplete |
| `flowLayoutPreviewLyrics` | `SlidePreviewControl` plus lyrics text | partial | 4/6 | focused Preview surface now accepts keyboard focus and routes page/verse keys; exact click/highlight/page behavior still incomplete |
| `flowLayoutPreviewPowerPoint` | `ClassicPreviewPowerPointThumbnailGrid` (`Tag=flowLayoutPreviewPowerPoint`) | partial | 4/6 | preview thumbnail surface uses `PowerPoint` PreviewItem state; focused Up/Down/Left/Right/PageUp/PageDown/Space/Home/End now route to Preview-only slide navigation and do not advance live Output; exact animation/media trigger parity still incomplete |
| `PreviewHolder`, `PreviewBack` | `ClassicPreviewHolder` (`Tag=PreviewHolder`) / `ClassicPreviewSlidePane` | partial | 4 | holder role is explicit and lower Preview now scales a fixed 960x540 frame like a slide surface; exact legacy background controls still incomplete |
| `PreviewBtnVerse1..Ending` | `flowLayoutPanel1` + `PreviewBtnVerse1..Ending` | partial | 4/6 | static buttons are visible; Preview verse navigation is Preview-only; verify label casing and shortcut parity |
| `PreviewBtnItemUp/Down` | `PreviewBtnItemUp/Down` + item nav commands | partial | 4 | exact selection behavior |
| `PreviewBtnSlideUp/Down` | `PreviewBtnSlideUp/Down` + lyrics page/PPT preview nav commands | partial | 4 | moves PreviewItem only; item-type-aware media/page edge cases still incomplete |
| `btnToLive` | `btnToLive` + `GoLiveCommand` | partial | 4 | exact FrmMain semantics |
| `btnToOutput` | `btnToOutput` + `CopyPreviewToOutputCommand`; global F8 `CopyPreviewToOutputShortcutCommand` | partial | 4/6 | local button still prepares `OutputItem`/PPT output state without starting live; FrmMain global F8 now copies Preview to Output and republishes the current Preview page when live; `OutputBtnItemUp/Down` can move the prepared OutputItem independently; hidden-output payload refresh and focus parity still incomplete |
| `btnToOutputMoveNext` | `btnToOutputMoveNext` + `CopyPreviewToOutputAndNextCommand` | partial | 4 | FrmMain copy-to-Output plus Preview NextOne now does not start live; focus/OutputItem navigation parity still incomplete |
| `IndPanel`, `Ind_*` | `IndcbPreviewNotes`, `IndradioButtonText/Format/Info` plus inspector/default source | partial | 5 | first-screen individual format controls and mode switching |

## 5. Output

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `OutputPanelDisplayName` | `ClassicOutputPanelDisplayName` (`Tag=OutputPanelDisplayName`) | partial | 4 | title now follows prepared `OutputItem` rather than selected Preview item; exact status columns still incomplete |
| `OutputInfo` | `ClassicOutputInfo` (`Tag=OutputInfo`) | partial | 4/6 | live info surface is explicit; focused verse keys now call `JumpToOutputLyricsSectionCommand` and cannot fall through to Preview; exact state columns still incomplete |
| `flowLayoutOutputLyrics` | Output live lyrics surface | partial | 4/6 | focused Output large/info surfaces now route Up/Down/Page/Space to live Output page commands; exact click/highlight behavior still incomplete |
| `flowLayoutOutputPowerPoint` | `ClassicOutputPowerPointSurface` (`Tag=flowLayoutOutputPowerPoint`) / `ClassicOutputThumbnailGrid` bound to `OutputPowerPoint.Thumbnails` | partial | 4/6 | live thumbnail/list role now uses independent OutputItem PPT state; focused Up/Down/Left/Right/PageUp/PageDown/Space/Home/End route to Output-only slide navigation; Output PPT position labels update from `OutputPowerPoint`; non-PPT live thumbnail parity still incomplete |
| `OutputHolder`, `OutputBack` | `ClassicOutputHolder` (`Tag=OutputHolder`) / `ClassicOutputBack` (`Tag=OutputBack`) with `OutputPowerPoint.PreviewImage` overlay | partial | 4 | large output screen uses independent OutputItem PPT state inside a fixed 960x540 frame and is not changed by Preview slide navigation; exact legacy background controls still incomplete |
| `OutputBtnVerse1..Ending` | `flowLayoutPanel2` + `OutputBtnVerse1..Ending` | partial | 4/6 | static buttons are visible; buttons now call `JumpToOutputLyricsSectionCommand` so live Output lyrics jump independently from Preview selection; shortcut parity still incomplete |
| `OutputBtnItemUp/Down` | `OutputBtnItemUp/Down` + `PreviousOutputItemCommand` / `NextOutputItemCommand` | partial | 4 | live/prepared OutputItem next/prev now uses OutputItem/live id independently from Preview selection; exact keyboard/focus parity still incomplete |
| `OutputBtnSlideUp/Down` | `OutputBtnSlideUp/Down` + `PreviousOutputSlideCommand` / `NextOutputSlideCommand` | partial | 4 | PPT slide and live lyrics page movement now target OutputItem independently, including when Preview selection diverges; shortcut/focus parity still incomplete |
| `OutputBtnRefAlert` | `OutputBtnRefAlert` + `ToggleOutputReferenceAlertCommand` + output `ReferenceAlertVisibility/Text` overlay | partial | 4/5 | toggles current live title/reference overlay like `QueryShowActive`; legacy reference source/pick/scroll/flash/duration options still incomplete |
| `OutputBtnMedia` | `OutputBtnMedia` + `PlayOutputMediaCommand` | partial | 4/5 | button now resolves the current OutputItem/live item independently from Preview selection, opens Output if needed, and loads/toggles the matching media file; exact legacy live-show remote pause/play edge cases still incomplete |
| `OutputBtnJumpToNonRotate` | `OutputBtnJumpToNonRotate` + `JumpToNextNonRotateOutputItemCommand` | partial | 4/5 | placeholder is now wired; it moves the independent Output/live context to the next item that is non-rotating in WPF's current page/slide model; legacy `RotateStyle`/`RotateGap`/`RotateTimings` and Gap pre-roll semantics still need metadata parity |
| `cbOutputBlack` | `cbOutputBlack` + bottom/menu/F9/F10 Output Black toggle + `ToggleOutputBlackCommand` / `IsOutputBlackActive` | partial | 4/5/6 | top, bottom, menu, F9, F10, and palette paths now behave as checked toggles that call Black and restore on second click; exact legacy remote/icon flash sync still incomplete |
| `cbOutputClear` | `cbOutputClear` + bottom/menu/F3 Output Clear toggle + `ToggleOutputClearCommand` / `IsOutputClearActive` | partial | 4/5/6 | top, bottom, menu, F3, and palette paths now behave as checked toggles that call Clear and restore on second click; exact legacy remote/icon flash sync still incomplete |
| `cbGoLive` | `cbGoLive` + bottom/menu/F12 Output live toggle + `ToggleOutputLiveCommand` / `IsOutputLiveActive` | partial | 4/6 | top, bottom, menu, F12, and palette paths now expose live checked state, start from Off, restore from Hidden, and stop from Active; exact legacy start-show edge cases remain incomplete |
| `OutputTextBoxLM`, `OutputBtnLMSend/Clear` | `OutputTextBoxLM`, `OutputBtnLMSend`, `OutputBtnLMClear` + `OutputLiveMessage` commands | partial | 5 | currently uses Notice publish/clear; implement true lyrics-monitor overlay parity |

## 6. Gesture Mapping

| Gesture | FrmMain source | WPF status | Phase |
| --- | --- | --- | --- |
| Song double-click add | `SongsList` | partial | 3 |
| Media double-click add | `MediaList_MouseDoubleClick` | partial | 3 |
| Bible selected passage drag | `BibleText_MouseDown` with `DragDropSource.BiblePassage` | implemented | 3 | typed `BibleSelection` drag inserts at Worship List drop target |
| Worship List reorder drag | `DragDropSource.WorshipList` | implemented | 3 | `LiveQueueItem` drag/drop reorders by reference, preserving duplicate queue items |
| Source-to-Worship drag | `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `BiblePassage`, `PraiseBookItems` | implemented | 3 | typed/file-drop payloads insert before the drop target; PraiseBook now resolves legacy `SongId` through the Admin DB when needed |
| Image-to-background drag | `flowLayoutImages` thumbnail drag to preview/background surface | partial | 5 | Inline image thumbnails now drag the exact pressed image as `FileDrop`; Preview area accepts image drops and applies Output background |
| Source/PraiseBook Enter add | `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `SearchResults`, `LookupCandidates`, `BibleText`, `PraiseBookItems` | implemented | 3/6 | source lists reuse `AddSelectedSourceToWorshipListAsync`; PraiseBook Enter reuses DB-backed add path as double-click; broader focus/shortcut parity remains Phase 6 |
| Worship List context menu | `CMenuWorship_*` | partial | 3/4 | Select/Unselect/Clear/Play/Play-on-Output now map to WPF handlers/commands; Edit/Usage still partial |
| Bible context menu | `CMenuBible`, `CMenuBible_SelectAll`, `CMenuBible_UnselectAll`, `CMenuBible_AddShow`, `CMenuBible_AddRegion2`, `CMenuBible_Copy`, `CMenuBible_CopyInfoScreen` | partial | 3 | names/order/actions and opening enable rules are wired; exact legacy keyboard accelerators and rich-text menu state still incomplete |
| Images primary click/context menu | `ApplySelectedImageCommand`, `CMenuImages`, `CMenuImages_AddItem`, `CMenuImages_AddDefault`, `CMenuImages_Refresh` | partial | 5 | primary apply matches `ApplyBackground(..., 2)` item-first/default-fallback behavior; context menu still exposes explicit Add to Item/Add to Default/Refresh |
| Preview keyboard nav | `flowLayoutPreviewLyrics_KeyUp`, `PreviewInfo_KeyUp`, `flowLayoutPreviewPowerPoint_KeyUp` | partial | 6 | PPT thumbnail focus handles arrow/Page/Space/Home/End before global shortcuts; Preview lyrics/info focus now handles verse keys and previous/next page keys; Home/End/item-nav parity still incomplete |
| Output keyboard nav | `flowLayoutOutputLyrics`, `OutputInfo_KeyUp`, `flowLayoutOutputPowerPoint_KeyUp` | partial | 6 | PPT thumbnail focus handles arrow/Page/Space/Home/End against live Output state; Output lyrics/info focus now handles verse keys and previous/next live page keys without Preview fallthrough; Home/End/item-nav parity still incomplete |
| Global live shortcuts | `KeyboardActionHandler`, hook handlers | partial | 6 | F12/F9/F3 command registry paths use the same checked toggle commands as the visible FrmMain-style controls; F7 now copies Preview to Output and clears Black, F8 copies Preview to Output and republishes when live, and F10 toggles Black; remaining global hook option edge cases, hidden-output payload refresh, and focus edge cases still incomplete |

## 7. Verification Status

This mapping is Phase 0. It documents the current state and intentionally shows many `partial` and `missing` rows. Implementation must reduce these rows before any area is called complete.
