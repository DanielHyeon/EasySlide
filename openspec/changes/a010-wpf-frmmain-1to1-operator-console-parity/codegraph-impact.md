## CodeGraph 영향 분석

Phase 0 문서화와 구현 수정 전에 CodeGraph context/explore에서 생성한 기준 영향 분석이다.

### 진입점

- `Easislides/Easislides/FrmMain.Designer.cs`의 `FrmMain`
- `Easislides/Easislides/FrmMain.cs`의 `FrmMain`
- `Easislides/Easislides/FrmMain.Fields.cs`의 `FrmMain`
- `Easislides.Wpf/MainWindow.xaml.cs`의 `MainWindow`

### 영향이 큰 영역

- `MainWindow`는 source tabs, left lower tabs, Preview/Output panes, lazy loaders, drag/drop handlers, command launchers 전반에 넓은 shell impact를 가진다.
- `MainViewModel`과 WPF source/list view model은 실제 data와 command routing을 다루는 후속 구현 phase에서 수정될 가능성이 높다.
- legacy `FrmMain.*` 파일은 WinForms regression이 발견되지 않는 한 이 change에서는 reference-only다.

### Phase 0 범위

Phase 0은 documentation/inventory 전용이다. Production code는 변경하지 않는다.

### 구현 가드

data parsers, settings, SQLite helpers, Office/PowerPoint interop, output monitor logic 같은 shared logic을 수정하기 전에는 해당 symbol에 대한 targeted `codegraph_impact`를 실행한다.

### 현재 SDD 정렬 업데이트

SDD tree realignment 이후 이 파일은 OpenSpec change의 structural evidence를 소유한다. 앞선 Phase 0 impact는 historical context로만 유지한다. Phase 5 또는 Phase 6 production edit 전에, 실제로 변경할 concrete symbol에 대한 targeted impact를 이 파일에 갱신해야 한다.

현재 CodeGraph evidence 기준으로 알려진 Phase 5/6 high-risk 중심은 다음과 같다:

- `MainViewModel`은 impact radius가 넓으므로 small command/settings slice 단위로만 수정한다.
- `MainWindow` / `MainWindow.xaml.cs`는 WPF shell layout, lazy loading, focus routing, source/list event handlers를 소유한다.
- `ShortcutRegistry`, `CommandCatalog`, `VerseJumpKeyMap`, media key routing은 keyboard/focus parity를 소유한다.
- `OutputWindowService`, `PreviewWindowService`, `OutputWindowViewModel`, `LiveSessionService`는 live output semantics에 영향을 준다.

필수 refresh point:

- Phase 5: formatting/background/live-safety 관련 수정 대상 symbol마다 편집 전에 targeted impact를 실행한다.
- Phase 6: shortcut/focus 관련 수정 대상 symbol마다 편집 전에 targeted impact를 실행한다.
- Phase 7: verification이 production-code fix를 드러내지 않는 한 새 impact는 필요 없다.

### Phase 5 영향 갱신 (2026-06-21)

Phase 5 production edit 전 구조 증거를 갱신했다. 이번 pass에서는 먼저 현재 구현/문서 상태를 분류했으며, 즉시 production code edit는 하지 않았다.

CodeGraph 확인:

- `codegraph_status`: 527 files, 29,637 nodes, 51,708 edges. Index healthy.
- `codegraph_context`: Phase 5 first-screen formatting/background/live-safety query의 entry point는 `MainWindow`, `MainViewModel`, `LiveSessionService`.
- `codegraph_impact MainViewModel depth=2`: 990 symbols 영향. `MainViewModel`은 `Ind_*`, `Def_*`, background, transition, live-safety, live message, reference alert command/state를 대부분 소유한다.
- `codegraph_impact LiveSessionService depth=2`: 628 symbols 영향. hidden/black/clear/restore payload, lyrics alert, reference alert snapshot semantics에 영향.
- `codegraph_impact OutputWindowViewModel depth=2`: 269 symbols 영향. background image/color, content transition, lyrics/reference alert visual rendering에 영향.
- `codegraph_impact OutputWindowService depth=2`: 627 symbols 영향. output open/move/close state와 live host 연결에 영향.

현재 코드 증거:

- `Easislides.Wpf/MainWindow.xaml`에는 `DefPanel`, `Def_*`, `IndPanel`, `Ind_*`, `OutputBtnRefAlert`, `cbOutputBlack`, `cbOutputClear`, `cbGoLive`, `panelOutputLM1`, `OutputTextBoxLM`, `OutputBtnLMSend`, `OutputBtnLMClear`, `flowLayoutImages`가 존재한다.
- `MainViewModel`에는 `ApplyImageBackgroundItemFirst`, `SetSelectedItemBackgroundImage*`, `SetDefaultItemTransition*`, `SetSelectedItemTransition*`, `SendLiveMessageCommand`, `ClearLiveMessageCommand`, `ToggleOutputReferenceAlertCommand`, `ToggleOutputBlackCommand`, `ToggleOutputClearCommand`, `RestoreOutputCommand` 계열이 존재한다.
- `OutputWindowViewModel`은 item/slide transition 구분, background image/color rendering, lyrics alert flash, reference alert flash/scroll rendering을 소유한다.

Phase 5 mapping row 분류:

| Mapping row | 분류 | 근거 | 남은 범위 |
| --- | --- | --- | --- |
| `tabImages` | partial | real folders/thumbnails, import, item-first/default-fallback apply가 존재 | manual UAT-142 및 exact visual parity |
| `flowLayoutImages` | partial | 3-column sizing, double-click/Enter/Apply, context menu, drag evidence 존재 | manual UAT-142 |
| `tabDefault` / `DefPanel` | partial | first-screen `Def_*` controls와 command binding 존재 | exact `Def_*` geometry와 모든 legacy option parity는 후속 visual/UAT 범위 |
| `Def_TransItem`, `Def_TransSlides` | partial | default item/slide transition 분리와 persistence test 존재 | full 58-effect rendering parity는 defer |
| `IndPanel`, `Ind_*` | partial | individual formatting, template, text/background/media, image mode, margin, notes controls 존재 | exact legacy chrome/details와 manual visual UAT |
| `Ind_VAlign`, `Ind_LeftUpDown`, `Ind_RightUpDown`, `Ind_BottomUpDown` | partial | FormatData 63/64/65/66 저장, live snapshot/render 적용 test 존재 | NumericUpDown chrome/unit calibration manual UAT |
| `Ind_TransItem`, `Ind_TransSlides` | partial | FormatData 72/73 저장과 reflected state test 존재 | full legacy transition animation rendering은 defer |
| `OutputBtnRefAlert` | partial | command, checked state, source/duration/style/pick filter, output overlay tests 존재 | manual UAT-406~411 visual timing 확인 |
| `OutputBtnMedia`, `OutputBtnJumpToNonRotate` | partial | independent Output item context와 media/rotate command path 존재 | legacy remote/rotate metadata edge cases는 후속 Phase 6/remote parity |
| `cbOutputBlack`, `cbOutputClear` | partial | top/bottom/menu/shortcut toggle path와 restore tests 존재 | exact legacy remote/icon flash sync defer |
| `OutputTextBoxLM`, `OutputBtnLMSend/Clear` | partial | LM panel visibility, send/clear command, alert overlay/flash tests 존재 | exact legacy duration/options manual UAT |
| Image-to-background drag | partial | `FileDrop` drag 및 Preview drop item-first/default-fallback path 존재 | manual UAT-142 |
| Images primary click/context menu | partial | primary apply and explicit Add Item/Add Default/Refresh context paths 존재 | manual UAT-142 |

Phase 5 현재 결론:

- P0 first-screen control은 현재 XAML과 command binding에 존재한다.
- 남은 `partial`은 주로 manual visual UAT, exact legacy chrome, full transition effect rendering, remote/icon sync, legacy duration/options이다.
- Phase 5에서 production code를 새로 수정해야 하는 명확한 missing P0 control은 현재 증거로 발견되지 않았다. 다음 단계는 focused tests와 UAT evidence를 실행/기록하는 것이다.

### Phase 6 영향 갱신 (2026-06-21)

Phase 6 keyboard/focus parity production edit 전 구조 증거를 갱신했다. 이번 pass에서는 현재 구현과 automated guard를 검증했으며, 즉시 production code edit는 하지 않았다.

CodeGraph 확인:

- `codegraph_context`: keyboard/focus parity query의 entry point는 `VerseJumpKeyMap`, `MediaPlayerKeyMap`, `WorshipListKeyMap`.
- `codegraph_impact ShortcutRegistry depth=2`: 43 symbols 영향. local/global shortcut 등록, binding, invocation, `GlobalInputService`, `MainViewModel.BindShortcuts` tests에 영향.
- `codegraph_impact CommandCatalog depth=2`: 681 symbols 영향. command id/default shortcut/source of truth, menu hint, command palette, `MainViewModel.BindShortcuts`에 영향.
- `codegraph_impact VerseJumpKeyMap depth=2`: 3 symbols 영향. FrmMain verse key mapping은 작고 직접 테스트 가능.
- `codegraph_impact MediaPlayerKeyMap depth=2`: 15 symbols 영향. live media key route와 text/button focus block에 영향.
- `codegraph_impact WorshipListKeyMap depth=2`: 5 symbols 영향. Worship List Delete key route에 영향.

현재 코드 증거:

- `MainWindow.OnPreviewKeyDown`는 focused Preview/Output surface handling, media route, text-input guard, final ShortcutRegistry route 순서를 가진다.
- `MainWindow.IsTextInputFocused()`는 `TextBoxBase`, editable `ComboBox`, `PasswordBox`를 차단 대상으로 본다.
- `VerseJumpKeyMap.MapKeyToLabel(key, modifiers)`는 digit, `C/B/P/E`, Shift mappings(`Shift+B/W/P/Q/T`)를 분리한다.
- `MediaPlayerKeyRouter.Resolve(...)`는 modifiers, text input focus, button focus를 고려해 Space/Enter hijack을 막는다.
- `WorshipListKeyMap.IsRemoveSelectedItem(...)`는 plain Delete만 remove로 본다.

Phase 6 검증 결론:

- focused tests 108개가 통과했다.
- Preview/Output focused key routing, global live shortcuts, verse jump mappings, media key routing, text input blocking은 automated guard를 가진다.
- 남은 `partial`은 manual keyboard UAT와 글로벌 hook/window-focus edge case다. 이는 Phase 7 gate의 manual UAT 및 follow-up defer 판단으로 넘긴다.

### Phase 7 Bibles 하단 UI correction 영향 갱신 (2026-06-21)

사용자 캡처 리뷰에서 WPF `Bibles` 탭 아래쪽 UI가 FrmMain과 다르다는 갭이 확인되어, verification-only Phase 중 작은 production UI correction을 수행했다.

CodeGraph 확인:

- `codegraph_context`: legacy `FrmMain` Bibles tab과 WPF `MainWindow` Bibles tab 비교 query의 entry point는 `FrmMain`, `MainWindow`.
- `codegraph_impact MainWindow depth=2`: 307 symbols 영향. 실제 수정은 `Easislides.Wpf/MainWindow.xaml`의 `tabBibles` layout과 해당 XAML 구조 테스트에 한정했다.

Legacy 기준:

- `FrmMain.Designer.cs`의 `tabBibles`는 `BibleText`, `BibleUserLookup`, `panelBible2`, `BookLookup`, `TabBibleVersions`만 포함한다.
- `TabBibleVersions`는 `BibleText` 바로 아래(`BibleText.Top + BibleText.Height`)에 위치한다.
- FrmMain 첫 화면에는 WPF-only 하단 `Bibles_AddSelected` 버튼, 선택 제목, 상태 텍스트 줄이 없다.

수정 및 검증:

- WPF `Bibles` 탭에서 non-legacy `Bibles_AddSelected` 하단 버튼과 `Bible.SelectedPassageTitle`/`Bible.StatusMessage` 하단 표시 줄을 제거했다.
- `TabBibleVersions`가 `BiblePassageBox` 바로 아래에 남도록 row structure를 단순화했다.
- `MainMenuBarTests` focused run: 133 passed, 0 failed.
- 재캡처 `evidence/screenshots/2026-06-21/tabs/source-03-bibles-after-frmmain-bottom-fix.png`: `AddButtonVisible=False`.

### Phase 7 탭별 시각 비교 추가 검토 (2026-06-21)

사용자 요청에 따라 Bibles 외 source/list 탭도 side-by-side 캡처로 재검토했다.

CodeGraph 확인:

- `codegraph_context`: WPF MainWindow tab parity gaps query는 설정 소스 class 위주로 잡혀 XAML 배치 직접 증거는 충분히 제공하지 못했다.
- `codegraph_impact MainWindow depth=2`: 307 symbols 영향. 이후 production edit는 `Easislides.Wpf/MainWindow.xaml`의 Folders tab layout과 `MainMenuBarTests` XAML guard에 한정했다.

캡처 검토:

- `tabs-compare/compare-02-infoscr.png`: WPF 탭은 열리지만 `C:\EasiSlides\InfoScreens` 파일 수 0으로 population parity 검증은 BLOCKED.
- `tabs-compare/compare-04-images.png`: WPF Images는 실제 이미지/썸네일을 표시하지만 첫 화면 썸네일 밀도와 visible count가 FrmMain보다 낮아 UAT-141을 PARTIAL로 유지.
- `tabs-compare/compare-05-default.png`: WPF `Def_*` control은 존재하지만 FrmMain 첫 화면의 `Apply to All Except InfoScreens`, `Default Background`, `Display Panel` 노출 순서/밀도와 달라 UAT-161을 PARTIAL로 유지.
- `tabs-compare/compare-06-worship-list.png`: 실제 `.esw`와 항목 로드는 PASS이나 lower-left toolbar density는 아직 exact visual parity가 아니다.
- `tabs-compare/compare-07-praise-book.png`: selector/entry surface는 표시되지만 WPF toolbar가 FrmMain compact top/vertical toolstrip과 달라 UAT-211을 PARTIAL로 유지.

### Phase 8 PARTIAL/BLOCKED closure impact

- CodeGraph 확인: `MainWindow` impact는 WPF shell event/gesture/visual surface 및 관련 tests 중심으로 확인했다.
- 코드 보정 범위: `MainWindow.xaml`, `LegacyImageThumbnailSizeConverter`, `MainMenuBarTests`, `LegacyImageThumbnailSizeConverterTests`.
- UAT-141: Images source thumbnail sizing을 PowerPoint sizing과 분리해 `LegacyImageThumbnailSize`로 고정했다.
- UAT-161: Default first viewport에서 `Apply` 그룹을 최상단으로 이동하고 `DefgroupBox1` 높이를 제한해 `Background / Transition` 및 Display Panel 계열 접근성을 개선했다.
- UAT-211: Praise Book toolbar를 compact하게 줄이고 status/window row가 entry list에 겹치던 row 정의 누락을 수정했다.
- 외부 조건 유지: UAT-406~408은 외부 송출 모니터 직접 관찰이 필요하고, BLOCKED 12개는 실제 데이터/설정 부재로 유지한다.

Folders 수정 및 검증:

- Legacy 기준: `FrmMain.Designer.cs`의 `tabFolders`는 `SongFolder`, `SongsList`, `panelFolders/Folders_WordCount`만 포함한다.
- WPF-only `Library.AvailableInitials` 머리글자 점프 바를 `MainWindow.xaml`에서 제거했다.
- `ClassicFoldersSourceGrid` row structure를 `SongFolder`, search, `SongsList`, status 순서로 정리했다.
- `MainMenuBarTests`에 `AutomationProperties.Name="머리글자 점프 바"`가 없어야 한다는 guard를 추가했다.
- Focused `MainMenuBarTests`: 133 passed, 0 failed.
- 재캡처: `evidence/screenshots/2026-06-21/tabs/source-01-folders-after-initial-strip-fix.png`, `tabs-compare/compare-01-folders-after-initial-strip-fix.png`.

### Phase 9 registry runtime settings sync impact (2026-06-21)

사용자 확인으로 `HKCU\Software\EasiSlides` 아래 registry-backed FrmMain settings가 실제 source of truth임을 재확인했다.

CodeGraph 확인:

- `codegraph_context`: registry-backed settings parity query에서 `RegistryLegacySettingsSource`, `SettingsService`, `FileLegacySettingsSource`, `CompositeLegacySettingsSource`가 entry point로 확인됐다.
- `codegraph_explore`: `App.OnStartup`은 `ISettingsBootstrapMigrationService.MigrateIfNeededAsync()`를 호출하고, `RegistryLegacySettingsSource`는 `Software\EasiSlides`의 `config`, `options`, `monitors` sections를 읽는다.
- `LegacySettingsMap`에는 `root_directory`, `media_dir`, `UsePowerpointTab`, `UseMediaTab`, `current_praisebook`, `current_session` alias가 이미 정의돼 있었다.

원인:

- 기존 `SettingsBootstrapMigrationService.MigrateIfNeededAsync()`는 `%APPDATA%\EasislidesNext\settings.json`이 있으면 `null`을 반환해 full migration을 skip했다.
- 따라서 기존 WPF 설정 파일이 있는 환경에서는 `current_praisebook`, `current_session`, `media_dir`, `UsePowerpointTab`, `UseMediaTab`가 레지스트리에서 다시 반영되지 않았다.

수정 범위:

- `Easislides.Wpf/Settings/SettingsBootstrapMigrationService.cs`: 기존 설정 파일이 있을 때 full migration은 반복하지 않고 runtime registry settings만 재동기화한다.
- `Easislides.Wpf.Tests/Settings/SettingsBootstrapMigrationServiceTests.cs`: 기존 skip 테스트를 runtime refresh 테스트로 갱신하고 invalid bool warning 테스트를 추가했다.

검증:

- `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~SettingsBootstrapMigrationServiceTests|FullyQualifiedName~RegistryLegacySettingsSourceTests" -v minimal`
- 결과: 실패 0, 통과 8, 건너뜀 0.

### Phase 10 deployed runtime verification impact (2026-06-21)

### Phase 11 PPT image resolution and preview speed impact (2026-06-21)

사용자 확인으로 WPF PPT 이미지가 WinForms보다 다른 해상도로 보이고 미리보기 체감 속도가 느린 문제가 확인되었다.

- Legacy baseline: `OfficeLib/PowerPoint.cs`의 `EXPORT_WIDTH=640`, `EXPORT_HEIGHT=480`. WinForms는 PPT preview/output dump를 이 크기의 JPG로 만든 뒤 `ImageCanvas`에서 화면 썸네일 크기에 맞춰 그린다.
- WPF pre-fix behavior: `PowerPointPreviewViewModel`, `PowerPointLibraryViewModel`, `MainViewModel`이 PPT preview/thumbnail 렌더 요청에 `4096x3072` 또는 출력 모니터 해상도 기반 크기를 사용했다.
- CodeGraph impact: `PowerPointPreviewViewModel` 변경 영향은 `MainViewModel` PPT preview/output slide routing, `PowerPointLibraryViewModel` source preview thumbnails, `MainViewModelTests`, `PowerPointPreviewViewModelTests`, `PowerPointLibraryViewModelTests`, `MainMenuBarTests`에 집중된다.
- Fix scope: `LegacyPowerPointImageSize`를 추가해 WPF PPT preview/source thumbnail/Preview-Output thumbnail strip 렌더 크기를 `640x480`으로 중앙화했다.
- Expected effect: Office export와 WPF PNG/JPG decode 대상 픽셀 수가 약 25배 줄어 PowerPoint preview/source thumbnail 체감 지연을 줄이고, WinForms와 같은 PPT 이미지 해상도 기준을 유지한다.

Phase 10은 production code 변경 없이 배포/실행 검증만 수행했다.

- WPF Release publish: `C:\EasiSlides\EasislidesNext`.
- 기존 WinForms 배포본: `C:\EasiSlides\Easislides.exe`.
- 검증 대상 runtime path는 `App.OnStartup` -> `SettingsBootstrapMigrationService.MigrateIfNeededAsync()` -> `RegistryLegacySettingsSource`/`SettingsService` -> `MainWindow`/`MainViewModel`이다.
- 배포 실행 결과 WPF settings snapshot이 `HKCU\Software\EasiSlides` 값과 일치했다.
- UIAutomation과 캡처로 WPF/WinForms가 같은 Folders song list, current worship list `1.주일예배`, Worship 23 items 계열 화면을 표시함을 확인했다.

### Phase 12 Worship List item-operation toolbar impact (2026-06-21)

사용자 확인으로 좌측 하단 Worship List 중간 조작 아이콘(`Move Item Up` 등)이 WinForms와 다르게 수평 배열되고 버튼 폭이 너무 큰 문제가 확인됐다.

- Legacy baseline: `FrmMain.Designer.cs`의 `panelWorshipList2`는 폭 33px이고, `toolStripWorshipList2.LayoutStyle=VerticalStackWithOverflow`이며 `WL_Up/WL_Down/WL_Delete/WL_Word/WL_Notes`는 22x22 `ToolStripButton`이다.
- WPF pre-fix behavior: `WorshipListPanel.xaml`에서 `WL_Manage/WL_Add/WL_Open`과 `MoveSelectedItemUpCommand` 이후 항목 조작 버튼들이 같은 `ClassicWorshipListToolStrip2` 수평 `StackPanel`에 섞여 있었다. 또한 `EsButton.Secondary`가 `EsButton.Base`의 `MinWidth=80`을 상속해, `Width=28` 지정에도 실제 버튼 폭이 WinForms보다 크게 잡힐 수 있었다.
- Fix scope: `WorshipListPanel.xaml`만 production UI로 수정했다. 상단 관리 버튼은 `ClassicWorshipListToolStrip1` 수평 스트립으로 분리하고, 항목 조작 버튼은 `ClassicWorshipListToolStrip2Frame`/`ClassicWorshipListToolStrip2` 오른쪽 33px 세로 레일로 이동했다.
- User correction: 최초 보정은 배열만 맞추고 WPF-only 편의 버튼(`MoveToTop/Bottom`, `Duplicate`, `SelectLive`, `Clear/Restore`, `Validate`)을 세로 레일에 남겨 WinForms 대비 아이콘 숫자가 많았다. 최종 보정에서는 WinForms `toolStripWorshipList2.Items`와 동일하게 `WL_Up`, `WL_Down`, `WL_Delete`, separator, `WL_Word`, `WL_Notes`만 남겼다.
- Test scope: `WorshipListPanelTests`에 top strip/side rail 구조, `DockPanel.Dock=Right`, `Width=33`, `Orientation=Vertical`, compact `MinWidth=0/MinHeight=0`, `MoveSelectedItemUpCommand` 22x22 크기 가드를 추가했다.
- 영향 제한: ViewModel command, WorshipList drag/drop, DB/Interop/송출 경로는 변경하지 않았다. UI 배치와 XAML 구조 가드만 변경했다.

### Phase 13 Main top ToolStrip single-row impact (2026-06-21)

사용자 확인으로 WinForms 상단 가로 아이콘은 두 줄이 아니라 한 줄인데, WPF 상단에 두 번째 가로 아이콘 줄이 보이는 문제가 확인됐다.

- Legacy baseline: `FrmMain.Designer.cs`의 `toolStripMain`은 `Main_New`, `Main_Edit`, `Main_Copy`, `Main_Move`, `Main_Delete`, `Main_Media`, `Main_Refresh`, `Main_Options`, `Main_NoRotate`, `Main_RotateStyle`, `Main_Alerts`, `Main_Chinese`, `Main_Find`, `Main_QuickFind`, `Main_JumpA/B/C`를 한 줄 `ToolStrip`에 배치한다. 기준 크기는 `632x31`이다.
- WPF pre-fix behavior: `MainWindow.xaml`의 `toolStripMain`은 `WrapPanel`이라 폭이 줄면 줄바꿈될 수 있었고, 바로 아래 `ClassicOperatorBar`가 `Grid.Row=1`의 visible `WrapPanel`로 붙어 WinForms에 없는 두 번째 가로 아이콘 줄을 만들었다.
- Fix scope: `MainWindow.xaml`에서 `toolStripMain`을 `StackPanel Orientation=Horizontal`로 변경하고, `ClassicOperatorBar`를 `Visibility=Collapsed`로 접었다.
- Command impact: 라이브/출력 명령 구현과 ViewModel command는 변경하지 않았다. 해당 명령은 기존 메뉴 및 Preview/Output 패널 경로에 남아 있으며, 숨겨진 `ClassicOperatorBar` XAML은 command regression test 대상으로 유지한다.
- Test scope: `MainMenuBarTests`의 `ToolStripMainXaml` helper를 `StackPanel` 기준으로 갱신하고, `ClassicOperatorBar`가 두 번째 visible toolbar row를 만들지 않는다는 guard를 추가했다.
- 영향 제한: DB, Office Interop, PPT 렌더링, Worship List 데이터/명령, 송출 창 로직은 변경하지 않았다. 변경은 WPF top toolbar XAML 배치와 XAML 구조 테스트에 한정된다.

### Phase 14 Worship List horizontal ToolStrip runtime impact (2026-06-21)

사용자 확인으로 좌측 하단 Worship List의 가로줄 아이콘 영역(`WL_Manage`, `WL_Add`, `WL_Open`)이 WinForms와 다르게 보이는 문제가 확인됐다.

- Legacy baseline: `FrmMain.Designer.cs`의 초기 배치는 `SessionList`가 `(3,5)`, `panelWorshipList1`이 `(88,5)`이고, `toolStripWorshipList1.Items`는 `WL_Manage`, `WL_Add`, `WL_Open` 3개다.
- Runtime baseline: `FrmMain.cs`의 `ResizeComboAndToolBar(tabControlLists, ref SessionList, ref panelWorshipList1)`가 `SessionList`를 `tabControlLists.Width - (panelWorshipList1.Width + 15)`로 늘리고, `panelWorshipList1.Left = SessionList.Left + SessionList.Width + 1`로 가로 아이콘 패널을 오른쪽에 붙인다.
- WPF pre-fix behavior: `WorshipListPanel.xaml`은 `SessionCombo` 줄 아래에 `ClassicWorshipListToolStrip1`을 별도 `DockPanel.Dock=Top` row로 두고, WinForms에 없는 `LoadSelectedWorshipListCommand` 버튼을 `SessionCombo` 오른쪽에 표시했다.
- Fix scope: `ClassicWorshipListToolStrip1`을 `ClassicWorshipListSessionStrip` 내부 오른쪽 94px column으로 이동하고, `SessionCombo`는 남는 폭(`*`)을 쓰도록 변경했다. `LoadSelectedWorshipListCommand` 버튼은 이 band에서 제거했다.
- Test scope: `WorshipListPanelTests`에 `SessionCombo` runtime resize semantics(`Width` 고정 없음, `MinWidth=60`), `ClassicWorshipListToolStrip1` column 위치, 버튼 순서/폭/tooltip guard를 추가했다.
- 영향 제한: ViewModel command, WorshipList load/reorder/delete/export/notes handlers, DB/Interop/송출 경로는 변경하지 않았다. UI 배치와 XAML 구조 guard만 변경했다.

### Phase 15 Worship List toolbar icon spacing impact (2026-06-21)

사용자 확인으로 Phase 14 이후 가로 아이콘과 세로 아이콘이 서로 너무 붙어 보이는 문제가 남아 있음이 확인됐다.

- Impact surface: `WorshipListPanel.xaml`의 `ClassicWorshipListToolStrip1`/`ClassicWorshipListToolStrip2` XAML layout 속성만 변경했다.
- Horizontal change: `WL_Manage`, `WL_Add`에 오른쪽 3px margin을 추가하고 `WL_Open`은 마지막 버튼으로 0px margin을 유지했다. 총 폭은 `29+3+29+3+29=93`으로 94px band 안에 남아 wrap을 유발하지 않는다.
- Vertical change: 22x22 세로 버튼의 크기는 유지하고 버튼 사이에 하단 3px margin을 추가했다. rail style은 `HorizontalAlignment=Center`로 고정하여 33px rail 안의 중앙 정렬을 유지한다.
- Test scope: `WorshipListPanelTests`에 horizontal margin, final button no-overflow margin, vertical margin, centered side rail style guard를 추가했다.
- 영향 제한: ViewModel command, WorshipList 데이터, DB, Office Interop, PPT/이미지 렌더링, 실제 송출 경로는 변경하지 않았다. 변경은 시각적 간격과 XAML 구조 테스트에 한정된다.

### Phase 16 Worship List RTF icon and SessionCombo text polish impact (2026-06-21)

사용자 확인으로 `Generate RTF Document` 아이콘이 직관적이지 않고, `SessionList` 콤보박스의 선택 텍스트 아래쪽이 잘려 보이는 문제가 확인됐다.

- Impact surface: `WorshipListPanel.xaml`의 `WL_Word` icon symbol과 `SessionCombo` layout 속성만 변경했다.
- Icon change: Wpf.Ui `SymbolRegular` enum에서 `DocumentText24` 존재를 확인하고, 기존 `DocumentArrowDown24`를 `DocumentText24`로 교체했다. `WL_Word_Click` handler와 RTF 생성 로직은 변경하지 않았다.
- Combo change: `SessionCombo`는 Phase 14의 28px height와 `*` width semantics를 유지하고, 내부 padding을 `8,2,4,2`로 낮추며 `VerticalContentAlignment=Center`를 지정했다.
- Test scope: `WorshipListPanelTests`에 `SessionCombo` padding/vertical alignment guard와 `WL_Word` icon symbol guard를 추가했다.
- 영향 제한: ViewModel command, saved worship list loading, RTF document generation handler, DB, Office Interop, 송출 경로는 변경하지 않았다. 변경은 visual affordance와 text clipping 보정에 한정된다.

### Phase 17 Default tab color swatch display impact (2026-06-21)

사용자 확인으로 Default 탭 상세 설정에서 색상이 실제 색으로 보이지 않고 코드처럼 표시되는 문제가 확인됐다.

- Impact surface: `MainWindow.xaml`의 Default 탭 색상 표시 XAML, 신규 `ColorValueToBrushConverter`, converter/XAML 구조 테스트에 한정된다.
- Pre-fix behavior: `Def_BackColourHex`, `Def_PanelBackColourHex`, `Def_PanelTextColourHex`, `TextColorHexBox`, `BackgroundColorHexBox`가 색상 값을 `#RRGGBB` 텍스트로 노출했고, `Def_R2Colour`와 상세 보조영역 글자색 콤보는 `DisplayMemberPath=Key` 텍스트만 표시했다.
- Fix scope: 색상 적용 ViewModel command와 코드비하인드 color picker handler는 유지하고, 노출 UI만 `ClassicColorSwatch`/`ColorPresetItemTemplate` 기반 색상 스와치로 변경했다.
- Converter scope: `ColorValueToBrushConverter`는 string hex 및 int ARGB 값을 frozen `SolidColorBrush`로 바꾸는 표시용 converter이며, setting 저장/불러오기/송출 렌더링 로직은 변경하지 않는다.
- Test scope: `ColorValueToBrushConverterTests`로 변환 동작을 검증하고, `MainMenuBarTests`로 raw hex TextBox 회귀와 `DisplayMemberPath` 텍스트 전용 표시 회귀를 막는다.
- 영향 제한: ViewModel 색상 상태, 레지스트리/JSON 설정, DB, Office Interop, PPT/이미지 렌더링, 실제 송출 창 로직은 변경하지 않았다. 변경은 Default 탭 색상 표시 affordance에 한정된다.

### Phase 18 Output text preview header impact (2026-06-21)

사용자 확인으로 WPF 오른쪽 Output 텍스트 렌더링 미리보기 상단에 WinForms와 다른 노란 제목 영역이 보이는 문제가 확인됐다.

- Impact surface: `MainWindow.xaml`의 `ClassicOutputInfo` text context, `MainViewModel.IsOutputHeaderTextVisible`, `MainMenuBarTests`, `MainViewModelTests`에 한정된다.
- Legacy baseline: WinForms `flowLayoutOutputLyrics`는 `gfUiText.HighlightRichTextBox`가 현재 row 배경(`TextRegionSlideBackColour`)과 글자색(`TextRegionSlideTextColour`)만 바꾼다. 패널 위에 별도의 `OutputItem.Title` header strip을 만들지 않는다.
- Pre-fix behavior: WPF `ClassicOutputInfo`가 `HasOutputLyricsText`일 때 `Background="#FFF6E600"`인 별도 `Border`를 표시하고, 그 안에 `OutputItem.Title`을 빨간 글씨로 보여 주었다. 이 줄이 사용자가 본 “윗쪽 한줄 노란 제목 영역”이다.
- Fix scope: 별도 header `Border`를 제거하고 `IsOutputHeaderTextVisible`을 false로 고정했다. 또한 오른쪽 Output text scroller margin을 왼쪽 Preview text content와 같은 `Thickness.Lg`로 맞춰 좌우 텍스트 row 시작 위치를 정합했다. Output lyrics rows, page navigation, current row yellow highlight, lower 4:3 Output preview frame은 변경하지 않았다.
- Test scope: `MainMenuBarTests`는 `ClassicOutputInfo`에 `IsOutputHeaderTextVisible`, `OutputItem.Title`, `#FFF6E600` title band가 없고 `ClassicOutputLyricsScrollViewer`가 Preview와 같은 content inset을 쓰는지 검증한다. `MainViewModelTests`는 lyrics Output 상태에서도 header visibility가 false임을 검증한다.
- 영향 제한: Live session routing, Preview/Output item independence, DB, Office Interop, PPT/이미지 렌더링, 실제 송출 창 text rendering은 변경하지 않았다.
