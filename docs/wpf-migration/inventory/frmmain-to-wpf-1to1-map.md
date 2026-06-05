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
| `splitContainerMain.Panel1` | `ClassicSourceColumn` / `ClassicSourcePane` | partial | 1 | source/list column now scales by splitter-like proportion; behavior/data parity incomplete |
| `splitContainerMain.Panel2` | `ClassicPreviewColumn` + `ClassicOutputColumn` | partial | 1 | Preview/Output columns are explicitly named; exact top/bottom behavior incomplete |
| `splitContainer1.Panel1` | `LeftBrowserTabs` | partial | 1/2/3 | source tabs exist, several are shallow |
| `splitContainer1.Panel2` | `LeftListTabs` | partial | 1/2/3 | Worship List visible, Praise Book shallow |
| `splitContainer2.Panel1` | `ClassicPreviewColumn` | partial | 1/4 | Preview column is named; legacy controls incomplete |
| `splitContainer2.Panel2` | `ClassicOutputColumn` | partial | 1/4 | Output column is named; live thumbnails/control parity incomplete |
| `splitContainerPreview.Panel1` | `ClassicTopControlRow` + `ClassicPreviewPane` | partial | 4/5 | top row is named; needs exact item/slide/section/format controls |
| `splitContainerPreview.Panel2` | `ClassicBottomScreenRow` + `ClassicPreviewSlidePane` | partial | 4 | bottom row is named; needs independent Preview surface behavior |
| `splitContainerOutput.Panel1` | `ClassicTopControlRow` + `ClassicOutputPane` | partial | 4/5 | top row is named; needs live output thumbnails/status/buttons |
| `splitContainerOutput.Panel2` | `ClassicBottomScreenRow` + `ClassicOutputSlidePane` | partial | 4 | bottom row is named; needs independent Output large surface |

## 2. Source Browser Tabs

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `tabControlSource` | `LeftBrowserTabs` | partial | 1 | enforce exact tab order and tab roles |
| `tabFolders` | `Folders` tab / `ClassicFoldersSourceGrid` | partial | 2/3 | compact FrmMain-style top strip now exists; context menu/edit commands still incomplete |
| `SongFolder` | `ClassicSongFolderCombo` (`Tag=SongFolder`) | partial | 2 | first-screen combo is mapped; legacy folder management still lives outside this strip |
| `SongsList` | `LibrarySongList` (`Tag=SongsList`) | partial | 3 | headerless `ListView/GridView`, selection, double-click, Enter add, and drag exist; full context menu/edit parity incomplete |
| `Folders_WordCount` | `ClassicFoldersWordCountMode` (`Tag=Folders_WordCount`) | partial | 5 | stroke-count sort is first-screen; exact check-button behavior still incomplete |
| `tabFiles` | `InfoScreenSourceTab` | partial | 2/3 | folder selector, list management, import/edit/copy/move/delete |
| `InfoScreenFolder` | inline InfoScreen folder selector | partial | 2 | load legacy groups/folders |
| `InfoScreenList` | `InlineInfoScreenList` | partial | 3 | double-click, Enter add, and drag insert exist; context menu/edit/manage parity incomplete |
| `tabPowerpoint` | `PowerPointSourceTab` | partial | 2/3/4 | list/preview style, real thumbnails, drag/add |
| `PowerpointFolder` | inline PowerPoint folder selector | partial | 2 | match legacy folder groups |
| `PowerpointList` | `InlinePowerPointList` | partial | 3/4 | selection, double-click, Enter add, and drag insert exist; preview style/context menu incomplete |
| `PP_ListType` | no exact compact target | missing | 4 | list/preview style toggle |
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
| `tabControlLists` | `LeftListTabs` | partial | 1 | exact visual density and tab behavior |
| `tabWorshipList` | `Worship List` tab | partial | 2/3 | real `.esw` load is wired; startup now loads the selected saved list when the queue is empty; all toolbar/context commands still incomplete |
| `SessionList` | `SessionCombo` (`Tag=SessionList`) | partial | 2 | compact first-screen combo exists; startup selects the first saved `C:\EasiSlides\Admin\WorshipLists` entry when no prior selection exists; exact legacy selection persistence still incomplete |
| `WorshipListItems` | `WorshipListPanel.QueueList` (`Tag=WorshipListItems`) | partial | 3/4 | headerless `ListView/GridView`, startup default `.esw` load, drag reorder, double-click Go Live, and context menu exist; preview/play-on-output parity incomplete |
| `WL_Manage` | manage list command | partial | 3 | direct or WPF equivalent |
| `WL_Add` | `WL_Add` + `WorshipListPanel_AddSelectedSourceRequested` | partial | 3 | visible lower-left Add button now routes by active `LeftBrowserTabs` source for Folders, Bibles, InfoScreen, PowerPoint, Media, and Search; exact multi-select/source edge cases still incomplete |
| `WL_Open` | load selected/open file | partial | 2 | exact `.esw`/template flow |
| `WL_Up`, `WL_Down` | `ClassicWorshipListToolStrip2` move commands/drag reorder | partial | 3 | compact visible toolbar and keyboard paths exist; exact legacy vertical strip layout still incomplete |
| `WL_Delete` | remove selected | implemented | 3 | keep Delete key tests |
| `WL_Word`, `WL_Notes` | no full inline target | missing | 5/7 | export/notes decision needed |
| `CMenuWorship_*` | WPF context menu | partial | 3/4 | select all, clear, edit, play, play on output, usage |
| `tabPraiseBook` | `Praise Book` tab | partial | 2/3 | saved book load plus FrmMain-style item surface now visible; DI store now reads legacy `WorkingFolder\Admin\PraiseBooks\*.esp`; exact template management still incomplete |
| `PraiseBook` | `InlinePraiseBookSavedBooksCombo` (`Tag=PraiseBook`) | partial | 2 | saved PraiseBook names load from WPF JSON plus legacy `C:\EasiSlides\Admin\PraiseBooks\*.esp` via `IPraiseBookStore` |
| `PraiseBookItems` | `PraiseBookItems` (`Tag=PraiseBookItems`) | partial | 3 | flat headerless ListView opens legacy `.esp` items with `SongId` from `ItemID`; double-click/Enter add-to-Worship plus drag insert into Worship List now use the same song-resolution path; exact preview-on-selection still incomplete |
| `PB_Manage`, `PB_Add`, `PB_Delete` | `PB_Manage`, `PB_Add`, `PB_Delete` | partial | 3 | manage window, add selected Folders song, delete selected rows are wired |
| `PB_Word`, `PB_Html`, `PB_WordCount` | `PB_Word`, `PB_Html`, `PB_WordCount` | partial | 5/7 | HTML/RTF export wired and toolbar column coverage now preserves `PB_Html`; WordCount button exposed but exact CJK word-count sorting still disabled |
| `CMenuPraiseB_*` | `CMenuPraiseB`, `CMenuPraiseB_SelectAll`, `CMenuPraiseB_UnselectAll`, `CMenuPraiseB_Clear`, `CMenuPraiseB_Edit` | partial | 3 | menu names/order/actions are wired; edit opens library context rather than direct editor |

## 4. Preview

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `PreviewPanelDisplayName` | `ClassicPreviewPanelDisplayName` (`Tag=PreviewPanelDisplayName`) | partial | 4 | title role is explicit; exact source/status columns still incomplete |
| `PreviewInfo` | `ClassicPreviewInfo` (`Tag=PreviewInfo`) | partial | 4/6 | selected lyrics/info surface is explicit; focused verse keys and Up/Down/Page/Space now route to Preview lyrics commands; source-specific content still incomplete |
| `flowLayoutPreviewLyrics` | `SlidePreviewControl` plus lyrics text | partial | 4/6 | focused Preview surface now accepts keyboard focus and routes page/verse keys; exact click/highlight/page behavior still incomplete |
| `flowLayoutPreviewPowerPoint` | `ClassicPreviewPowerPointThumbnailGrid` (`Tag=flowLayoutPreviewPowerPoint`) | partial | 4/6 | preview thumbnail surface uses `PowerPoint` PreviewItem state; focused Up/Down/Left/Right/PageUp/PageDown/Space/Home/End now route to Preview-only slide navigation; exact animation/media trigger parity still incomplete |
| `PreviewHolder`, `PreviewBack` | `ClassicPreviewHolder` (`Tag=PreviewHolder`) / `ClassicPreviewSlidePane` | partial | 4 | holder role is explicit; background/frame sizing parity still incomplete |
| `PreviewBtnVerse1..Ending` | `flowLayoutPanel1` + `PreviewBtnVerse1..Ending` | partial | 4/6 | static buttons are visible; verify label casing, shortcut, and live-page parity |
| `PreviewBtnItemUp/Down` | `PreviewBtnItemUp/Down` + item nav commands | partial | 4 | exact selection behavior |
| `PreviewBtnSlideUp/Down` | `PreviewBtnSlideUp/Down` + lyrics page nav commands | partial | 4 | item-type-aware PPT/media/page nav |
| `btnToLive` | `btnToLive` + `GoLiveCommand` | partial | 4 | exact FrmMain semantics |
| `btnToOutput` | `btnToOutput` + `CopyPreviewToOutputCommand` | partial | 4 | FrmMain `CopyPreviewToOutput` semantics now prepare `OutputItem`/PPT output state without starting live; full OutputItem navigation still incomplete |
| `btnToOutputMoveNext` | `btnToOutputMoveNext` + `CopyPreviewToOutputAndNextCommand` | partial | 4 | FrmMain copy-to-Output plus Preview NextOne now does not start live; focus/OutputItem navigation parity still incomplete |
| `IndPanel`, `Ind_*` | `IndcbPreviewNotes`, `IndradioButtonText/Format/Info` plus inspector/default source | partial | 5 | first-screen individual format controls and mode switching |

## 5. Output

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `OutputPanelDisplayName` | `ClassicOutputPanelDisplayName` (`Tag=OutputPanelDisplayName`) | partial | 4 | title now follows prepared `OutputItem` rather than selected Preview item; exact status columns still incomplete |
| `OutputInfo` | `ClassicOutputInfo` (`Tag=OutputInfo`) | partial | 4/6 | live info surface is explicit; focused verse keys now call `JumpToOutputLyricsSectionCommand` and cannot fall through to Preview; exact state columns still incomplete |
| `flowLayoutOutputLyrics` | Output live lyrics surface | partial | 4/6 | focused Output large/info surfaces now route Up/Down/Page/Space to live Output page commands; exact click/highlight behavior still incomplete |
| `flowLayoutOutputPowerPoint` | `ClassicOutputPowerPointSurface` (`Tag=flowLayoutOutputPowerPoint`) / `ClassicOutputThumbnailGrid` bound to `OutputPowerPoint.Thumbnails` | partial | 4/6 | live thumbnail/list role now uses independent OutputItem PPT state; focused Up/Down/Left/Right/PageUp/PageDown/Space/Home/End route to Output-only slide navigation; non-PPT live thumbnail parity still incomplete |
| `OutputHolder`, `OutputBack` | `ClassicOutputHolder` (`Tag=OutputHolder`) / `ClassicOutputBack` (`Tag=OutputBack`) with `OutputPowerPoint.PreviewImage` overlay | partial | 4 | large output screen now uses independent OutputItem PPT state; exact sizing/frame parity still incomplete |
| `OutputBtnVerse1..Ending` | `flowLayoutPanel2` + `OutputBtnVerse1..Ending` | partial | 4/6 | static buttons are visible; buttons now call `JumpToOutputLyricsSectionCommand` so live Output lyrics jump independently from Preview selection; shortcut parity still incomplete |
| `OutputBtnItemUp/Down` | `OutputBtnItemUp/Down` + item nav commands | partial | 4 | exact live next/prev semantics |
| `OutputBtnSlideUp/Down` | `OutputBtnSlideUp/Down` + `PreviousOutputSlideCommand` / `NextOutputSlideCommand` | partial | 4 | PPT slide and live lyrics page movement now target OutputItem independently; shortcut/focus parity still incomplete |
| `OutputBtnRefAlert` | `OutputBtnRefAlert` + `ToggleOutputReferenceAlertCommand` + output `ReferenceAlertVisibility/Text` overlay | partial | 4/5 | toggles current live title/reference overlay like `QueryShowActive`; legacy reference source/pick/scroll/flash/duration options still incomplete |
| `OutputBtnMedia` | `OutputBtnMedia` + `Media.PlayPauseCommand` | partial | 5 | direct output media behavior |
| `OutputBtnJumpToNonRotate` | `OutputBtnJumpToNonRotate` disabled placeholder | missing | 5 | gap/non-rotate jump |
| `cbOutputBlack` | `cbOutputBlack` + `BlackScreenCommand` | partial | 4/5 | stateful check/toggle parity |
| `cbOutputClear` | `cbOutputClear` + `ClearOutputCommand` | partial | 4/5 | stateful check/toggle parity |
| `cbGoLive` | `cbGoLive` + `RestoreOutputCommand` | partial | 4 | exact checked state/start-show semantics |
| `OutputTextBoxLM`, `OutputBtnLMSend/Clear` | `OutputTextBoxLM`, `OutputBtnLMSend`, `OutputBtnLMClear` + `OutputLiveMessage` commands | partial | 5 | currently uses Notice publish/clear; implement true lyrics-monitor overlay parity |

## 6. Gesture Mapping

| Gesture | FrmMain source | WPF status | Phase |
| --- | --- | --- | --- |
| Song double-click add | `SongsList` | partial | 3 |
| Media double-click add | `MediaList_MouseDoubleClick` | partial | 3 |
| Bible selected passage drag | `BibleText_MouseDown` with `DragDropSource.BiblePassage` | partial | 3 |
| Worship List reorder drag | `DragDropSource.WorshipList` | partial | 3 |
| Source-to-Worship drag | `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `BiblePassage`, `PraiseBookItems` | partial | 3 |
| Image-to-background drag | `flowLayoutImages` thumbnail drag to preview/background surface | partial | 5 | Inline image thumbnails now drag the exact pressed image as `FileDrop`; Preview area accepts image drops and applies Output background |
| Source/PraiseBook Enter add | `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `SearchResults`, `LookupCandidates`, `BibleText`, `PraiseBookItems` | partial | 3/6 | source lists reuse `AddSelectedSourceToWorshipListAsync`; PraiseBook Enter reuses the same add path as double-click |
| Worship List context menu | `CMenuWorship_*` | partial | 3/4 |
| Bible context menu | `CMenuBible`, `CMenuBible_SelectAll`, `CMenuBible_UnselectAll`, `CMenuBible_AddShow`, `CMenuBible_AddRegion2`, `CMenuBible_Copy`, `CMenuBible_CopyInfoScreen` | partial | 3 | names/order/actions and opening enable rules are wired; exact legacy keyboard accelerators and rich-text menu state still incomplete |
| Images primary click/context menu | `ApplySelectedImageCommand`, `CMenuImages`, `CMenuImages_AddItem`, `CMenuImages_AddDefault`, `CMenuImages_Refresh` | partial | 5 | primary apply matches `ApplyBackground(..., 2)` item-first/default-fallback behavior; context menu still exposes explicit Add to Item/Add to Default/Refresh |
| Preview keyboard nav | `flowLayoutPreviewLyrics_KeyUp`, `PreviewInfo_KeyUp`, `flowLayoutPreviewPowerPoint_KeyUp` | partial | 6 | PPT thumbnail focus handles arrow/Page/Space/Home/End before global shortcuts; Preview lyrics/info focus now handles verse keys and previous/next page keys; Home/End/item-nav parity still incomplete |
| Output keyboard nav | `flowLayoutOutputLyrics`, `OutputInfo_KeyUp`, `flowLayoutOutputPowerPoint_KeyUp` | partial | 6 | PPT thumbnail focus handles arrow/Page/Space/Home/End against live Output state; Output lyrics/info focus now handles verse keys and previous/next live page keys without Preview fallthrough; Home/End/item-nav parity still incomplete |
| Global live shortcuts | `KeyboardActionHandler`, hook handlers | partial | 6 |

## 7. Verification Status

This mapping is Phase 0. It documents the current state and intentionally shows many `partial` and `missing` rows. Implementation must reduce these rows before any area is called complete.
