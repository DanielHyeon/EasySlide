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
| `BibleText` | `BiblePassageBox` | partial | 3 | selection, context menu, drag `BiblePassage` |
| `tabImages` | `ImagesSourceTab` | partial | 2/5 | real folders, thumbnails, context menu, default/item background |
| `ImagesFolder` | image folder selector | partial | 2 | legacy image groups |
| `flowLayoutImages` | `InlineImagesList` | partial | 5 | context menu add item/default/refresh |
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
| `tabPraiseBook` | `Praise Book` tab | partial | 2/3 | real list load and add/delete/export |
| `PraiseBook` | `InlinePraiseBookSavedBooksCombo` | partial | 2 | verify real praise book source |
| `PraiseBookItems` | `PraiseBook` grouped list | partial | 3 | exact item list behavior, double-click/add/delete |
| `PB_Manage`, `PB_Add`, `PB_Delete` | WPF buttons | partial | 3 | management and item operations |
| `PB_Word`, `PB_Html`, `PB_WordCount` | no full inline target | missing | 5/7 | export/count parity |

## 4. Preview

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `PreviewPanelDisplayName` | Preview header | partial | 4 | exact title/source/status columns |
| `PreviewInfo` | selected lyrics/info text | partial | 4 | keyboard handling and source-specific content |
| `flowLayoutPreviewLyrics` | `SlidePreviewControl` plus lyrics text | partial | 4 | click/keyboard/page behavior |
| `flowLayoutPreviewPowerPoint` | PowerPoint preview tab/thumbnails | partial | 4 | exact thumbnail selection and key handling |
| `PreviewHolder`, `PreviewBack` | `ClassicPreviewSlidePane` | partial | 4 | sizing/background/frame parity |
| `PreviewBtnVerse1..Ending` | `SectionJumpBar` | partial | 4/6 | all labels and shortcut parity |
| `PreviewBtnItemUp/Down` | Preview item nav | partial | 4 | exact selection behavior |
| `PreviewBtnSlideUp/Down` | Preview slide/page nav | partial | 4 | item-type-aware nav |
| `btnToLive` | `GoLiveCommand` | partial | 4 | exact FrmMain semantics |
| `btnToOutput` | send Preview to Output | partial | 4 | separate from Go Live if needed |
| `btnToOutputMoveNext` | `SendToOutputAndNextCommand` | partial | 4 | verify advance behavior |
| `IndPanel`, `Ind_*` | inspector/default source | partial | 5 | first-screen individual format controls |

## 5. Output

| FrmMain control | WPF target | Status | Phase | Required next work |
| --- | --- | --- | --- | --- |
| `OutputPanelDisplayName` | Output header | partial | 4 | exact live item/status columns |
| `OutputInfo` | live item text/info | partial | 4 | key handling and state |
| `flowLayoutOutputLyrics` | Output live lyrics surface | partial | 4 | independent live page nav |
| `flowLayoutOutputPowerPoint` | `ClassicOutputThumbnailGrid` | partial | 4 | live slide highlight and key handling |
| `OutputHolder`, `OutputBack` | `ClassicOutputSlidePane` | partial | 4 | large output screen frame |
| `OutputBtnVerse1..Ending` | Output section jump bar | missing | 4/6 | live section jump controls |
| `OutputBtnItemUp/Down` | Output item nav | partial | 4 | exact live next/prev semantics |
| `OutputBtnSlideUp/Down` | Output slide nav | partial | 4 | exact live slide semantics |
| `OutputBtnRefAlert` | no direct first-screen target | missing | 5 | reference alert |
| `OutputBtnMedia` | media command | partial | 5 | direct output media behavior |
| `OutputBtnJumpToNonRotate` | no direct first-screen target | missing | 5 | gap/non-rotate jump |
| `cbOutputBlack` | `BlackScreenCommand` buttons | partial | 4/5 | stateful check/toggle parity |
| `cbOutputClear` | `ClearOutputCommand` buttons | partial | 4/5 | stateful check/toggle parity |
| `cbGoLive` | live state commands | partial | 4 | exact checked state |
| `OutputTextBoxLM`, `OutputBtnLMSend/Clear` | no complete target | missing | 5 | live message/lyrics alert |

## 6. Gesture Mapping

| Gesture | FrmMain source | WPF status | Phase |
| --- | --- | --- | --- |
| Song double-click add | `SongsList` | partial | 3 |
| Media double-click add | `MediaList_MouseDoubleClick` | partial | 3 |
| Bible selected passage drag | `BibleText_MouseDown` with `DragDropSource.BiblePassage` | partial | 3 |
| Worship List reorder drag | `DragDropSource.WorshipList` | partial | 3 |
| Source-to-Worship drag | `SongsList`, `InfoScreenList`, `PowerpointList`, `MediaList`, `BiblePassage` | partial | 3 |
| Worship List context menu | `CMenuWorship_*` | partial | 3/4 |
| Bible context menu | `CMenuBible_*` | missing | 3 |
| Images context menu | `CMenuImages_*` | missing | 5 |
| Preview keyboard nav | `flowLayoutPreviewLyrics_KeyUp`, `PreviewInfo_KeyUp` | partial | 6 |
| Output keyboard nav | `flowLayoutOutputLyrics`, `OutputInfo_KeyUp` | partial | 6 |
| Global live shortcuts | `KeyboardActionHandler`, hook handlers | partial | 6 |

## 7. Verification Status

This mapping is Phase 0. It documents the current state and intentionally shows many `partial` and `missing` rows. Implementation must reduce these rows before any area is called complete.
