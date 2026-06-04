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
| `SongsList` | `LibrarySongList` (`Tag=SongsList`) | partial | 3 | headerless `ListView/GridView`, selection, double-click, and drag exist; full context menu/edit parity incomplete |
| `Folders_WordCount` | `ClassicFoldersWordCountMode` (`Tag=Folders_WordCount`) | partial | 5 | stroke-count sort is first-screen; exact check-button behavior still incomplete |
| `tabFiles` | `InfoScreenSourceTab` | partial | 2/3 | folder selector, list management, import/edit/copy/move/delete |
| `InfoScreenFolder` | inline InfoScreen folder selector | partial | 2 | load legacy groups/folders |
| `InfoScreenList` | `InlineInfoScreenList` | partial | 3 | add context menu, edit/manage, drag insert |
| `tabPowerpoint` | `PowerPointSourceTab` | partial | 2/3/4 | list/preview style, real thumbnails, drag/add |
| `PowerpointFolder` | inline PowerPoint folder selector | partial | 2 | match legacy folder groups |
| `PowerpointList` | `InlinePowerPointList` | partial | 3/4 | selection, preview style, context menu, drag insert |
| `PP_ListType` | no exact compact target | missing | 4 | list/preview style toggle |
| `tabBibles` | `Bibles` tab | partial | 2/3 | full version/book/reference/selection workflow |
| `BookLookup` | Bible book selector | partial | 2 | load legacy book list per version |
| `BibleUserLookup` | `BibleReferenceBox` | partial | 2/3 | direct reference and search validation parity |
| `Bibles_Go` | Bible go button | partial | 3 | invoke lookup/search exactly |
| `TabBibleVersions` | Bible versions UI | partial | 2 | version tabs/list with selected version state |
| `BibleText` | `BiblePassageBox` + `CMenuBible_*` | partial | 3 | selection, FrmMain-named context menu, Add/Region2/Copy/InfoScreen, drag `BiblePassage`; exact rich text styling still incomplete |
| `tabImages` | `ImagesSourceTab` | partial | 2/5 | real folders/thumbnails exist; context menu default/item background is wired; folder group parity still incomplete |
| `ImagesFolder` | image folder selector | partial | 2 | legacy image groups |
| `flowLayoutImages` | `InlineImagesList` (`Tag=flowLayoutImages`) | partial | 5 | thumbnails, default background, item background, refresh menu exist; exact thumbnail sizing and folder-group UI still incomplete |
| `tabMedia` | `MediaSourceTab` | partial | 2/3 | real folders, import, double-click, drag insert |
| `MediaFolder` | media folder selector | partial | 2 | legacy media groups |
| `MediaList` | `InlineMediaList` | partial | 3 | double-click, keyboard, context menu |
| `tabDefault` | `DefaultSource` tab / inspector | partial | 5 | full `DefPanel` option parity |

## 3. Lower Left Lists

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `tabControlLists` | `LeftListTabs` | partial | 1 | exact visual density and tab behavior |
| `tabWorshipList` | `Worship List` tab | partial | 2/3 | real `.esw` load, all toolbar/context commands |
| `SessionList` | `SessionCombo` (`Tag=SessionList`) | partial | 2 | compact first-screen combo exists; verify actual `C:\EasiSlides\Admin\WorshipLists` list |
| `WorshipListItems` | `WorshipListPanel.QueueList` (`Tag=WorshipListItems`) | partial | 3/4 | headerless `ListView/GridView`, drag reorder, double-click Go Live, and context menu exist; preview/play-on-output parity incomplete |
| `WL_Manage` | manage list command | partial | 3 | direct or WPF equivalent |
| `WL_Add` | add selected source command | partial | 3 | source-aware add parity |
| `WL_Open` | load selected/open file | partial | 2 | exact `.esw`/template flow |
| `WL_Up`, `WL_Down` | `ClassicWorshipListToolStrip2` move commands/drag reorder | partial | 3 | compact visible toolbar and keyboard paths exist; exact legacy vertical strip layout still incomplete |
| `WL_Delete` | remove selected | implemented | 3 | keep Delete key tests |
| `WL_Word`, `WL_Notes` | no full inline target | missing | 5/7 | export/notes decision needed |
| `CMenuWorship_*` | WPF context menu | partial | 3/4 | select all, clear, edit, play, play on output, usage |
| `tabPraiseBook` | `Praise Book` tab | partial | 2/3 | saved book load plus FrmMain-style item surface now visible; exact legacy `.esp` template management still incomplete |
| `PraiseBook` | `InlinePraiseBookSavedBooksCombo` (`Tag=PraiseBook`) | partial | 2 | saved PraiseBook names load from WPF store; verify legacy `C:\EasiSlides` `.esp` source |
| `PraiseBookItems` | `PraiseBookItems` (`Tag=PraiseBookItems`) | partial | 3 | flat headerless ListView, double-click add-to-Worship, delete/clear/select actions exist; exact preview-on-selection and drag insert still incomplete |
| `PB_Manage`, `PB_Add`, `PB_Delete` | `PB_Manage`, `PB_Add`, `PB_Delete` | partial | 3 | manage window, add selected Folders song, delete selected rows are wired |
| `PB_Word`, `PB_Html`, `PB_WordCount` | `PB_Word`, `PB_Html`, `PB_WordCount` | partial | 5/7 | HTML/RTF export wired; WordCount button exposed but exact CJK word-count sorting still disabled |
| `CMenuPraiseB_*` | `CMenuPraiseB`, `CMenuPraiseB_SelectAll`, `CMenuPraiseB_UnselectAll`, `CMenuPraiseB_Clear`, `CMenuPraiseB_Edit` | partial | 3 | menu names/order/actions are wired; edit opens library context rather than direct editor |

## 4. Preview

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `PreviewPanelDisplayName` | `ClassicPreviewPanelDisplayName` (`Tag=PreviewPanelDisplayName`) | partial | 4 | title role is explicit; exact source/status columns still incomplete |
| `PreviewInfo` | `ClassicPreviewInfo` (`Tag=PreviewInfo`) | partial | 4 | selected lyrics/info surface is explicit; keyboard handling and source-specific content still incomplete |
| `flowLayoutPreviewLyrics` | `SlidePreviewControl` plus lyrics text | partial | 4 | click/keyboard/page behavior |
| `flowLayoutPreviewPowerPoint` | `ClassicPreviewPowerPointThumbnailGrid` (`Tag=flowLayoutPreviewPowerPoint`) | partial | 4 | preview thumbnail surface is explicit; exact thumbnail selection and key handling still incomplete |
| `PreviewHolder`, `PreviewBack` | `ClassicPreviewHolder` (`Tag=PreviewHolder`) / `ClassicPreviewSlidePane` | partial | 4 | holder role is explicit; background/frame sizing parity still incomplete |
| `PreviewBtnVerse1..Ending` | `flowLayoutPanel1` + `PreviewBtnVerse1..Ending` | partial | 4/6 | static buttons are visible; verify label casing, shortcut, and live-page parity |
| `PreviewBtnItemUp/Down` | `PreviewBtnItemUp/Down` + item nav commands | partial | 4 | exact selection behavior |
| `PreviewBtnSlideUp/Down` | `PreviewBtnSlideUp/Down` + lyrics page nav commands | partial | 4 | item-type-aware PPT/media/page nav |
| `btnToLive` | `btnToLive` + `GoLiveCommand` | partial | 4 | exact FrmMain semantics |
| `btnToOutput` | `btnToOutput` + `GoLiveCommand` | partial | 4 | split "copy to Output" from "start live" if needed |
| `btnToOutputMoveNext` | `btnToOutputMoveNext` + `SendToOutputAndNextCommand` | partial | 4 | verify advance behavior |
| `IndPanel`, `Ind_*` | `IndcbPreviewNotes`, `IndradioButtonText/Format/Info` plus inspector/default source | partial | 5 | first-screen individual format controls and mode switching |

## 5. Output

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `OutputPanelDisplayName` | `ClassicOutputPanelDisplayName` (`Tag=OutputPanelDisplayName`) | partial | 4 | live item title role is explicit; exact status columns still incomplete |
| `OutputInfo` | `ClassicOutputInfo` (`Tag=OutputInfo`) | partial | 4 | live info surface is explicit; key handling and state still incomplete |
| `flowLayoutOutputLyrics` | Output live lyrics surface | partial | 4 | independent live page nav |
| `flowLayoutOutputPowerPoint` | `ClassicOutputPowerPointSurface` (`Tag=flowLayoutOutputPowerPoint`) / `ClassicOutputThumbnailGrid` | partial | 4 | live thumbnail/list role is explicit; key handling still incomplete |
| `OutputHolder`, `OutputBack` | `ClassicOutputHolder` (`Tag=OutputHolder`) / `ClassicOutputBack` (`Tag=OutputBack`) | partial | 4 | large output screen frame roles are explicit; exact sizing/frame parity still incomplete |
| `OutputBtnVerse1..Ending` | `flowLayoutPanel2` + `OutputBtnVerse1..Ending` | partial | 4/6 | static buttons are visible; make nav act on live output independently |
| `OutputBtnItemUp/Down` | `OutputBtnItemUp/Down` + item nav commands | partial | 4 | exact live next/prev semantics |
| `OutputBtnSlideUp/Down` | `OutputBtnSlideUp/Down` + lyrics page nav commands | partial | 4 | exact live slide/PPT semantics |
| `OutputBtnRefAlert` | `OutputBtnRefAlert` disabled placeholder | missing | 5 | reference alert command and output overlay |
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
| Source-to-Worship drag | `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `BiblePassage` | partial | 3 |
| Worship List context menu | `CMenuWorship_*` | partial | 3/4 |
| Bible context menu | `CMenuBible`, `CMenuBible_SelectAll`, `CMenuBible_UnselectAll`, `CMenuBible_AddShow`, `CMenuBible_AddRegion2`, `CMenuBible_Copy`, `CMenuBible_CopyInfoScreen` | partial | 3 | names/order/actions and opening enable rules are wired; exact legacy keyboard accelerators and rich-text menu state still incomplete |
| Images context menu | `CMenuImages`, `CMenuImages_AddItem`, `CMenuImages_AddDefault`, `CMenuImages_Refresh` | partial | 5 |
| Preview keyboard nav | `flowLayoutPreviewLyrics_KeyUp`, `PreviewInfo_KeyUp` | partial | 6 |
| Output keyboard nav | `flowLayoutOutputLyrics`, `OutputInfo_KeyUp` | partial | 6 |
| Global live shortcuts | `KeyboardActionHandler`, hook handlers | partial | 6 |

## 7. Verification Status

This mapping is Phase 0. It documents the current state and intentionally shows many `partial` and `missing` rows. Implementation must reduce these rows before any area is called complete.
