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
