# FrmMain Control/Event Inventory

작성일: 2026-06-04

목적: WPF 이식 기준을 "비슷한 UI"가 아니라 legacy `FrmMain`의 실제 컨트롤명, 이벤트 핸들러, 데이터 로딩 흐름에 맞춘다.

## 1. Layout Containers

| Area | Legacy control | Evidence | Role |
| --- | --- | --- | --- |
| Main shell | `toolStripContainerMain` | `FrmMain.Designer.cs` | menu/toolstrip/content/status root |
| Left/right split | `splitContainerMain` | `Panel1.Controls.Add(splitContainer1)`, `Panel2.Controls.Add(splitContainer2)` | left source/list and right operator console |
| Left top/bottom split | `splitContainer1` | `Panel1.Controls.Add(tabControlSource)`, `Panel2.Controls.Add(tabControlLists)` | source browser over Worship/Praise lists |
| Right Preview/Output split | `splitContainer2` | `Panel1.Controls.Add(splitContainerPreview)`, `Panel2.Controls.Add(splitContainerOutput)` | Preview column and Output column |
| Preview top/bottom split | `splitContainerPreview` | `Panel1.Controls.Add(panelPreviewTop)`, `Panel2.Controls.Add(panelPreviewBottom)` | Preview controls over Preview screen |
| Output top/bottom split | `splitContainerOutput` | `Panel1.Controls.Add(panelOutputTop)`, `Panel2.Controls.Add(panelOutputBottom)` | Output controls over Output screen |

## 2. Top Toolbar And Menus

| Legacy control/menu | Event handler | Role | WPF parity target |
| --- | --- | --- | --- |
| `Main_New` | `Main_EditBtns_Click` | add new song/item | visible toolbar command |
| `Main_Edit` | `Main_EditBtns_Click` | edit selected item | visible toolbar command |
| `Main_Copy` | `Main_EditBtns_Click` | copy selected item | visible toolbar command |
| `Main_Move` | `Main_EditBtns_Click` | move selected item | visible toolbar command |
| `Main_Delete` | `Main_EditBtns_Click` | delete selected item | visible toolbar command |
| `Main_Media` | `Main_Media_Click` | media settings/action | visible toolbar command |
| `Main_Refresh` | `Main_Refresh_Click` | refresh current lists | visible toolbar command |
| `Main_Options` | `Main_Options_Click` | options | visible toolbar command |
| `Main_NoRotate` | `Main_NoRotate_Click` | rotation toggle | visible toolbar command |
| `Main_RotateStyle` | `Main_RotateStyle_DropDownItemClicked` | rotation style | visible toolbar dropdown |
| `Main_Alerts` | `Main_Alerts_Click` | alert tools | visible toolbar command |
| `Main_Chinese` | `Main_Chinese_Click` | Chinese/secondary text toggle | visible toolbar command |
| `Main_Find`, `Main_QuickFind` | `Main_Find_Click`, `Main_QuickFind_KeyUp` | quick search | top search box |
| `Main_JumpA/B/C` | `Main_Jump_Click` | letter jump | top jump buttons |
| `Menu_StartShow` | `Menu_StartShow_Click` | start/stop show | Output menu and visible live bar |
| `Menu_GoLiveWithPreview` | `Menu_PreviewGoLiveNext_Click` | send Preview live and advance | visible live command |
| `Menu_RefreshOutput` | `Menu_RefreshOutput_Click` | refresh output | visible live command |
| `Menu_BlackScreen` | `Menu_BlackScreen_Click` | black output | visible danger command |
| `Menu_ClearScreen` | `Menu_ClearScreen_Click` | clear output | visible danger command |
| `Menu_RestartCurrentItem` | `Menu_RestartCurrentItem_Click` | restart live item | visible live command |

## 3. Top Left Source Browser

Legacy tab order from `tabControlSource.Controls.Add(...)`:

1. `tabFolders`
2. `tabFiles`
3. `tabPowerpoint`
4. `tabBibles`
5. `tabImages`
6. `tabMedia`
7. `tabDefault`

| Tab | Key controls | Event/data handlers | Required behavior |
| --- | --- | --- | --- |
| `tabFolders` | `SongFolder`, `SongsList`, `Folders_WordCount` | `BuildFolderList`, `SongsList` handlers, `AddToWorshipList` | load song folders, select/search songs, double-click/add/drag into Worship List |
| `tabFiles` | `InfoScreenFolder`, `InfoScreenList`, `InfoScreen_New/Edit/Copy/Move/Delete`, `InfoScreen_OpenFolder`, `InfoScreen_Import` | `BuildInfoScreenFolderList`, `ShowInfoScreenFolderContents`, `InfoScreen_Import_Click` | browse InfoScreens, edit/manage, add/drag to Worship List |
| `tabPowerpoint` | `PowerpointFolder`, `PowerpointList`, `PP_ListType`, `PP_OpenFolder`, `PP_Import` | `ShowPowerpointFolderContents`, `PP_Style_DropDownItemClicked`, `PP_Import_Click` | browse/import PPT, show list/preview style, add/drag to Worship List |
| `tabBibles` | `BookLookup`, `BibleUserLookup`, `Bibles_Go`, `TabBibleVersions`, `BibleText` | `Gf.LoadBibleVersions`, `TabBibleVersionsChanged`, `BibleUserLookup_Submit`, `BibleText_MouseDown`, `AddFromHolyBible` | lookup passages, select verses, add/drag selected Bible passage |
| `tabImages` | `ImagesFolder`, `flowLayoutImages`, `Image_OpenFolder`, `Image_Import`, `CMenuImages_*` | `BuildPicturesFolderList`, `Image_Import_Click`, image context menu handlers | browse/import images, apply to item/default background |
| `tabMedia` | `MediaFolder`, `MediaList`, `Media_OpenFolder`, `Media_Import` | `BuildMediaFolderList`, `MediaList_MouseDoubleClick`, `Media_Import_Click` | browse/import media, add/drag to Worship List |
| `tabDefault` | `DefPanel`, `Def_*` controls | default formatting handlers | edit default format/background/transition/display-panel settings |

## 4. Bottom Left Lists

Legacy tab order from `tabControlLists.Controls.Add(...)`:

1. `tabWorshipList`
2. `tabPraiseBook`

| Tab | Key controls | Event/data handlers | Required behavior |
| --- | --- | --- | --- |
| `tabWorshipList` | `SessionList`, `WorshipListItems`, `WL_Manage`, `WL_Add`, `WL_Open`, `WL_Up`, `WL_Down`, `WL_Delete`, `WL_Word`, `WL_Notes` | `PopulateWorshipList`, `LoadWorshipList`, `SaveWorshipList`, `WorshipListPlayOnOutputMonitor`, drag/drop handlers | load saved lists, add from current source, reorder, delete, preview, play, play on output |
| `tabPraiseBook` | `PraiseBook`, `PraiseBookItems`, `PB_Manage`, `PB_Add`, `PB_Delete`, `PB_Word`, `PB_Html`, `PB_WordCount` | `PopulatePraiseBooksList`, `PraiseBook_SelectedIndexChanged`, `PraiseBookList_Change` | load praise books, browse entries, add/delete/export |

## 5. Preview Column

| Legacy control | Event handler | Role | WPF parity target |
| --- | --- | --- | --- |
| `PreviewPanelDisplayName` | resize/update handlers | selected Preview title/status | Preview header |
| `PreviewInfo` | `PreviewInfo_KeyUp` | text/info Preview interaction | Preview text/status area |
| `flowLayoutPreviewLyrics` | `Click`, `KeyUp`, `PreviewKeyDown`, `Leave` handlers | Preview lyric pages and keyboard navigation | Preview lyric surface |
| `flowLayoutPreviewPowerPoint` | `flowLayoutPreviewPowerPoint_KeyUp`, `PowerPointImage_MouseUp` | Preview PPT thumbnails | Preview PPT thumbnail strip/grid |
| `PreviewHolder`, `PreviewBack` | `ResizeSampleScreen` | large Preview surface | Preview large slide/screen |
| `PreviewBtnVerse1..Ending` | `PreviewBtnVerse_Click` | direct section jump | Preview section jump bar |
| `PreviewBtnItemUp/Down` | `PreviewBtnUpDown_Click` | previous/next Preview item | Preview item nav |
| `PreviewBtnSlideUp/Down` | `PreviewBtnUpDown_Click` | previous/next Preview slide/page | Preview slide nav |
| `btnToLive` | `btnToLive_Click` | start/send live | visible Preview live command |
| `btnToOutput` | `btnToOutput_Click` | send selected Preview to Output | visible Preview send command |
| `btnToOutputMoveNext` | `btnToOutputMoveNext_Click` | send and advance | visible Preview send+next command |
| `IndPanel`, `Ind_*` | `Ind_*` handlers | individual formatting | inline or docked individual-format panel |

## 6. Output Column

| Legacy control | Event handler | Role | WPF parity target |
| --- | --- | --- | --- |
| `OutputPanelDisplayName` | resize/update handlers | live Output title/status | Output header |
| `OutputInfo` | `OutputInfo_KeyUp` | live item keyboard/status | Output text/status area |
| `flowLayoutOutputLyrics` | `Click`, `Leave`, rich text handlers | live lyric pages | Output lyric surface |
| `flowLayoutOutputPowerPoint` | `flowLayoutOutputPowerPoint_KeyUp`, `PowerPointImage_MouseUp` | live PPT thumbnails | Output PPT thumbnail grid |
| `OutputHolder`, `OutputBack` | `ResizeOutputBottomPanel` | large Output surface | Output large slide/screen |
| `OutputBtnVerse1..Ending` | `OutputBtnVerse_Click` | live section jump | Output section jump bar |
| `OutputBtnItemUp/Down` | `OutputBtnUpDown_Click` | previous/next live item | Output item nav |
| `OutputBtnSlideUp/Down` | `OutputBtnUpDown_Click` | previous/next live slide/page | Output slide nav |
| `OutputBtnRefAlert` | `OutputBtnRefAlert_Click` | reference alert | Output alert command |
| `OutputBtnMedia` | `OutputBtnMedia_Click` | media command | Output media command |
| `OutputBtnJumpToNonRotate` | `OutputBtnJumpToNonRotate_Click` | jump to non-rotate/gap | Output special jump |
| `cbOutputBlack` | `cbOutputBlack_Click` | black output | visible danger command |
| `cbOutputClear` | `cbOutputClear_Click` | clear output | visible danger command |
| `cbGoLive` | `cbGoLive_Click` | live state toggle | visible live state |
| `OutputTextBoxLM`, `OutputBtnLMSend`, `OutputBtnLMClear` | live message handlers | live message/lyrics alert | Output live-message area |

## 7. Data Loading Inventory

| Legacy method | Role | WPF parity need |
| --- | --- | --- |
| `Gf.InitAppData()` | initialize legacy working data | WPF must bind to real working folder |
| `BuildFolderList()` | load song folders | Folders tab real data |
| `PopulateWorshipList()` | saved worship list names | lower-left `SessionList` equivalent |
| `LoadWorshipList(int/string)` | load `.esw` or template | actual queue import |
| `PopulatePraiseBooksList()` | praise book names | lower-left Praise Book combo |
| `PraiseBookList_Change()` | praise book entries | inline Praise Book list |
| `BuildPicturesFolderList()` | image folders | Images tab |
| `BuildInfoScreenFolderList()` | InfoScreen folders | InfoScr tab |
| `BuildMediaFolderList()` | media folders | Media tab |
| `ShowPowerpointFolderContents(bool)` | PPT file list/thumbnails | PowerPoint tab |
| `Gf.LoadBibleVersions(ref TabBibleVersions)` | Bible versions | Bibles tab |
| `TabBibleVersionsChanged()` | Bible books/passages | Bibles tab |
| `BibleUserLookup_Submit()` | direct reference/search | Bibles lookup |
| `BibleVerseSearch()` | text search | Bibles search mode |
| `AddFromHolyBible()` | add selected passage | Worship List insertion |

## 8. Drag And Drop Inventory

Legacy source enum from `FrmMain.Fields.cs`:

| `DragDropSource` | Legacy source | Required WPF target |
| --- | --- | --- |
| `WorshipList` | `WorshipListItems` | reorder queue item |
| `SongsList` | `SongsList` | insert selected song at drop location |
| `InfoScreenList` | `InfoScreenList` | insert InfoScreen/text item |
| `PowerpointList` | `PowerpointList` | insert PPT item |
| `MediaList` | `MediaList` | insert media item |
| `BiblePassage` | `BibleText` selected verses | insert Bible passage |

## 9. Completion Rule

A region is not complete until its controls, event handlers, data loading, visible UI placement, and primary gestures are all represented in the WPF mapping table with verification evidence.
