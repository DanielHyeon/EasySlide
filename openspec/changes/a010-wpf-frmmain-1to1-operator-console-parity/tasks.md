## Tasks

### SDD Tree 정렬

- [x] 이 change에서 OpenSpec이 execution tree owner임을 선언한다.
- [x] `docs/wpf-migration/*`를 execution contract가 아니라 reference/evidence input으로 다룬다.
- [x] 남은 작업을 `Goal`, `Scope`, `Tasks`, `DoD`, `Tests`, `Constraints`를 가진 phase contract로 재구성한다.

### 완료된 Historical Phases

- [x] FrmMain 1:1 UI/UX 및 function mapping plan 추가.
- [x] operator-console parity용 OpenSpec proposal/design/spec 추가.
- [x] Phase 0: control/event inventory, FrmMain-to-WPF 1:1 mapping table, manual UAT checklist 추가.
- [x] Phase 1: WPF shell layout을 legacy split/container hierarchy에 맞게 재작업.
- [x] Phase 2: 모든 left source/list tab에 대해 실제 `C:\EasiSlides` data loading 복구.
- [x] Phase 3: drag-and-drop을 포함해 source-to-Worship-List add/insert gesture 복구.
- [x] Phase 4: 독립 Preview와 Output navigation/thumbnail/live-control behavior 복구.

### Phase 5: First-Screen Formatting, Background, Transition, Live-Safety

Goal: 첫 화면의 `Ind_*`, `Def_*`, background, transition, reference alert, live message, Black/Clear/Hide/Restore controls를 FrmMain과 동등한 운영자 동작으로 사용할 수 있게 만든다.

Scope:

- individual formatting, default formatting, image/background selection, media assignment, transitions, live-safety command를 위한 WPF `MainWindow` first-screen controls.
- 해당 control을 직접 뒷받침하는 view-model commands/settings.
- Phase 5 또는 Phase 4/5 partial인 `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md` mapping rows.

Tasks:

- [x] production edit 전에 Phase 5 target symbols에 대해 `codegraph-impact.md`를 갱신한다.
- [x] 모든 Phase 5 `partial` mapping row를 evidence와 함께 `implemented`, `partial`, `defer` 중 하나로 분류한다.
- [x] FrmMain과 동등한 운영에 필요한 누락된 first-screen `Ind_*`, `Def_*` controls를 닫는다.
- [x] image/background item-first/default-fallback behavior를 검증한다.
- [x] FrmMain이 구분하는 경우 item movement와 slide movement의 transition settings가 분리되어 있는지 검증한다.
- [x] Output live-safety controls가 Preview selection을 빼앗지 않고 hidden/live payload를 갱신하는지 검증한다.

DoD:

- 모든 Phase 5 P0 mapping row가 `implemented`이거나 근거와 함께 명시적으로 `defer`다.
- Output safety actions는 첫 화면에 계속 보이며 modal navigation을 요구하지 않는다.
- Preview formatting edits가 live Output state를 예기치 않게 변경하지 않는다.
- 관련 UAT rows가 PASS/PARTIAL/FAIL/BLOCKED evidence로 채워진다.

Tests:

- 영향을 받는 commands/settings에 대해 focused WPF tests를 추가하거나 갱신한다.
- `dotnet test Easislides.Wpf.Tests`를 실행한다.
- UAT-142, UAT-161, UAT-406부터 UAT-411까지 targeted manual UAT를 실행한다.

Constraints:

- DB schema를 변경하지 않는다.
- legacy WinForms behavior를 변경하지 않는다.
- Phase 5 impact가 명시적으로 요구하고 CodeGraph impact를 갱신하지 않는 한 Office/PPT interop을 건드리지 않는다.
- UI edit는 region-specific하게 유지하며 broad visual redesign을 하지 않는다.

Evidence:

- 2026-06-21 CodeGraph: `MainViewModel`, `LiveSessionService`, `OutputWindowViewModel`, `OutputWindowService` impact 갱신을 `codegraph-impact.md`에 기록.
- 2026-06-21 XAML 확인: `DefPanel`, `Def_*`, `IndPanel`, `Ind_*`, `OutputBtnRefAlert`, `cbOutputBlack`, `cbOutputClear`, `cbGoLive`, `OutputTextBoxLM`, `OutputBtnLMSend`, `OutputBtnLMClear`, `flowLayoutImages` 존재 확인.
- 2026-06-21 focused tests: `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~ApplyImageBackgroundItemFirst|FullyQualifiedName~SetSelectedItemBackgroundImage|FullyQualifiedName~SetSelectedItemTransitions|FullyQualifiedName~SlideTransitionKindInput|FullyQualifiedName~UpdateHiddenContent|FullyQualifiedName~ToggleOutputBlackCommand|FullyQualifiedName~ToggleOutputClearCommand|FullyQualifiedName~RestoreOutputCommand|FullyQualifiedName~SendLiveMessageCommand|FullyQualifiedName~ToggleOutputReferenceAlertCommand|FullyQualifiedName~ApplySession_WithReferenceAlert|FullyQualifiedName~ApplySession_WithLyricsAlertMessage"` 통과. 결과: 실패 0, 통과 35, 건너뜀 0.
- 남은 Phase 5 DoD: UAT-142, UAT-161, UAT-406~UAT-411 manual UAT 결과 기록. exact legacy chrome, full transition effect rendering, remote/icon sync, legacy duration/options는 `codegraph-impact.md`에서 partial/defer 범위로 분류.

### Phase 6: Keyboard Shortcut And Focus Parity

Goal: FrmMain live-operation key가 현재 operator focus에 따라 같은 target으로 routing되게 하고, text input field에서는 live shortcut이 절대 발동하지 않게 한다.

Scope:

- `MainWindow.xaml.cs` key routing과 focus surfaces.
- `ShortcutRegistry`, `CommandCatalog`, `VerseJumpKeyMap`, media key routing, source/list Enter/Delete/Ctrl+A paths.
- shortcut 및 gesture inventories의 mapping rows.

Tasks:

- [x] production edit 전에 Phase 6 target symbols에 대해 `codegraph-impact.md`를 갱신한다.
- [x] Preview-focused keys가 Preview에만 영향을 주는지 검증한다.
- [x] Output-focused keys가 Output/live context에만 영향을 주는지 검증한다.
- [x] global F12/F11/F9/F3/F7/F8/F10/F5/F4/Space/Shift+Space behavior를 검증한다.
- [x] Shift mappings를 포함한 number/letter verse jumps를 검증한다.
- [x] QuickFind, Bible lookup, live message, editable setting text fields가 live shortcut routing을 차단하는지 검증한다.
- [x] `frmmain-shortcut-parity-map.md`와 `frmmain-to-wpf-1to1-map.md`를 evidence와 함께 갱신한다.

DoD:

- UAT-501부터 UAT-508까지 기록된다.
- 모든 shortcut/gesture row가 `implemented`이거나 근거와 함께 명시적으로 deferred다.
- 가능한 command routing은 automated tests로 커버하고, focus-only behavior의 나머지는 manual UAT evidence를 가진다.

Tests:

- shortcut registry, command catalog, focused key-routing tests를 추가하거나 갱신한다.
- `dotnet test Easislides.Wpf.Tests`를 실행한다.
- WPF shell에서 manual keyboard UAT를 실행한다.

Constraints:

- text input에 입력 중일 때 global hook이 fire되지 않게 한다.
- Preview와 Output keyboard context를 하나의 command path로 합치지 않는다.
- visual polish를 evidence로 사용하지 않는다. command target과 resulting state를 기록한다.

Evidence:

- 2026-06-21 CodeGraph: `ShortcutRegistry`, `CommandCatalog`, `VerseJumpKeyMap`, `MediaPlayerKeyMap`, `WorshipListKeyMap` impact 갱신을 `codegraph-impact.md`에 기록.
- 2026-06-21 code evidence: `MainWindow.OnPreviewKeyDown`, `IsTextInputFocused`, `VerseJumpKeyMap.MapKeyToLabel`, `MediaPlayerKeyRouter.Resolve`, `WorshipListKeyMap.IsRemoveSelectedItem` 확인.
- 2026-06-21 focused tests: `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~CommandCatalogTests|FullyQualifiedName~ShortcutRegistryTests|FullyQualifiedName~GlobalInputServiceTests|FullyQualifiedName~VerseJumpKeyMapTests|FullyQualifiedName~MediaPlayerKeyMapTests|FullyQualifiedName~WorshipListKeyMapTests|FullyQualifiedName~BindShortcuts_|FullyQualifiedName~MainWindow_UsesFocusedPreviewOutputKeyboardBeforeGlobalShortcuts|FullyQualifiedName~MainWindow_BlocksGlobalShortcutsWhenTextInputFocused|FullyQualifiedName~MainWindow_PassesShiftToVerseJumpKeyMap"` 통과. 결과: 실패 0, 통과 108, 건너뜀 0.
- 남은 Phase 6 DoD: UAT-501~UAT-508 manual keyboard UAT 결과 기록. Global hook/window-focus edge case는 Phase 7 manual UAT에서 확인하거나 explicit defer로 전환.

### Phase 7: Verification Gate And Ship-Readiness Evidence

Goal: scoped operator-console parity 작업 범위에서 WPF MainWindow가 FrmMain처럼 사용할 수 있다고 보고할 수 있는 concrete evidence를 수집한다.

Scope:

- OpenSpec validation.
- WPF tests.
- WinForms build.
- WPF launch.
- `C:\EasiSlides`가 존재하는 경우를 포함해 configured working folder 아래 실제 legacy data 기준 manual UAT.

Tasks:

- [x] `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`를 실행한다.
- [x] `dotnet test Easislides.Wpf.Tests`를 실행한다.
- [x] `dotnet build Easislides\Easislides.csproj -nologo -v minimal`을 실행한다.
- [x] WPF MainWindow를 실행하고 startup/layout evidence를 기록한다.
- [x] 실제 legacy data로 manual UAT checklist를 수행한다.
- [x] 모든 FAIL 또는 unresolved PARTIAL을 follow-up OpenSpec task 또는 explicit defer로 전환한다.
- [x] phase complete 표시 전에 final gate evidence를 이 파일에 기록한다.

DoD:

- OpenSpec validation이 통과한다.
- WPF tests가 통과한다.
- WinForms build가 통과한다.
- WPF launch가 성공한다.
- 모든 P0 UAT rows가 PASS이거나 예외가 owner/rationale과 함께 명시적으로 deferred다.
- concrete evidence 없이 phase를 complete로 보고하지 않는다.

Tests:

- `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`
- `dotnet test Easislides.Wpf.Tests`
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal`
- `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`의 manual UAT checklist

Constraints:

- blocking regression이 발견되지 않는 한 이 phase는 verification-only다.
- gate evidence가 기록되기 전에는 change를 archive하지 않는다.
- gate가 통과하기 전에는 GBrain canonical lessons를 저장하지 않는다. unresolved notes는 non-canonical로 남긴다.

Evidence:

- 2026-06-21 `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict` 통과. 결과: Change is valid.
- 2026-06-21 `dotnet test Easislides.Wpf.Tests` 통과. 결과: 실패 0, 통과 2417, 건너뜀 0.
- 2026-06-21 `dotnet build Easislides\Easislides.csproj -nologo -v minimal` 통과. 결과: 오류 0, 경고 35. 주요 경고는 기존 NetOffice NU1701, WindowsDesktop SDK NETSDK1137, DirectShow CA1416, WinForms WFDEV004, Resources CS0649.
- 2026-06-21 WPF launch smoke 통과. `Easislides.Wpf\bin\Debug\net10.0-windows\EasislidesNext.exe` 실행 결과 `MainWindowTitle=EasiSlides`, `MainWindowHandle=1966334`, UIAutomation `AutomationName=EasiSlides`, `AutomationClassName=Window`, descendant 355개 확인 후 `CloseMainWindow=True`로 정상 종료.
- 2026-06-21 실제 legacy data smoke 확인. `C:\EasiSlides` 존재, `Admin`, `Backgrounds`, `HolyBibles`, `Images`, `InfoScreens`, `Media`, `Powerpoint` 폴더 확인. WPF UIAutomation에서 `Legacy worship list loaded: 1.주일예배 (23 .esw items)`, `Folders`, `InfoScr`, `Bibles`, `Images`, `Default`, `Worship List`, `Praise Book`, `Preview`, `Output`, `Go Live`, `Output 검은 화면`, `Output 화면 비우기`, `Output 복귀`, `Output 새로고침` 등 주요 first-screen 요소 확인.
- 2026-06-21 screenshot comparison evidence 추가. 캡처 위치: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/screenshots/2026-06-21/`. 주요 파일: `frmmain-vs-wpf-printwindow-side-by-side.png`, `winforms-frmmain-printwindow-1440x900.png`, `wpf-mainwindow-printwindow-1440x900.png`, `wpf-mainwindow-printwindow-1180x760.png`, `wpf-mainwindow-printwindow-1920x1080.png`.
- 2026-06-21 screenshot comparison 결과: WPF는 FrmMain처럼 좌상단 source browser, 좌하단 Worship List/Praise Book, 오른쪽 Preview/Output column, bottom preview/output surfaces를 첫 화면에 유지한다. UAT-001, UAT-002, UAT-003, UAT-004는 캡처 증거로 PASS 기록.
- 2026-06-21 DPI 확인: WPF `MainWindow.xaml`은 `Width=1180`, `Height=760`, `MinWidth=1180`, `MinHeight=760`으로 정의되어 있다. 125% DPI 환경에서 1180x760 logical size가 1475x950 physical capture로 기록되므로, 이 크기 차이는 결함으로 보지 않는다.
- 2026-06-21 source/list tab screenshot evidence 추가. 캡처 위치: `evidence/screenshots/2026-06-21/tabs/`. 주요 파일: `wpf-source-tabs-contact-sheet.png`, `source-01-folders.png`, `source-02-infoscr.png`, `source-03-bibles.png`, `source-04-images.png`, `source-05-default.png`, `list-01-worship-list.png`, `list-02-praise-book.png`.
- 2026-06-21 source/list tab UAT 결과: UAT-101 Folders PASS(1개 폴더, 6444개 곡), UAT-131 Bibles PASS(DB 2개, 책/본문/버전 UI 표시), UAT-141 Images PARTIAL(117개 이미지/썸네일은 표시되나 첫 화면 밀도 보정 필요), UAT-161 Default PARTIAL(`Def_*`는 존재하나 first-viewport 순서/밀도 보정 필요), UAT-201/202 Worship List PASS(35개 `.esw`, `1.주일예배` 23개 항목), UAT-211 Praise Book PARTIAL(selector/entry surface는 표시되나 toolbar/list 밀도와 실제 entry population 추가 확인 필요).
- 2026-06-21 source tab BLOCKED: UAT-111 InfoScr는 `C:\EasiSlides\InfoScreens` 파일 수 0으로 population 검증 불가. UAT-121 PowerPoint와 UAT-151 Media는 현재 설정 `UsePowerPointTab=false`, `UseMediaTab=false`로 탭이 숨겨지고 해당 폴더 파일 수 0이라 실제 데이터 population 검증 불가. XAML에는 `PowerPointSourceTab`, `MediaSourceTab`이 존재한다.
- 2026-06-21 사용자 지적 반영: Bibles 탭 아래쪽 UI가 FrmMain과 달라 WPF-only `Bibles_AddSelected` 하단 버튼 및 선택 제목/상태 줄을 제거했다. `BibleText` 바로 아래 `TabBibleVersions`가 오는 FrmMain 구조로 재정렬했고, 재캡처 `tabs/source-03-bibles-after-frmmain-bottom-fix.png`에서 `AddButtonVisible=False` 확인. Focused `MainMenuBarTests` 133개 통과.
- 2026-06-21 Bibles 하단 UI correction 후 전체 `dotnet test Easislides.Wpf.Tests` 재실행 통과. 결과: 실패 0, 통과 2417, 건너뜀 0. `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`도 통과.
- 2026-06-21 추가 탭 시각 비교 반영: `tabs-compare/compare-02-infoscr.png`, `compare-04-images.png`, `compare-05-default.png`, `compare-06-worship-list.png`, `compare-07-praise-book.png`를 검토했다. Bibles 외에도 Folders의 WPF-only 머리글자 점프 바, Images 첫 화면 썸네일 밀도, Default 첫 화면 `Default Background`/`Display Panel` 순서와 밀도, Praise Book toolbar/list 밀도, Worship List toolbar density 차이를 확인했다.
- 2026-06-21 Folders visual gap 수정: FrmMain `tabFolders`는 `SongFolder`, `SongsList`, `panelFolders/Folders_WordCount`만 가지므로 WPF-only 머리글자 점프 바를 제거했다. `MainMenuBarTests`에 “머리글자 점프 바 없음” 가드를 추가했고 focused `MainMenuBarTests` 133개 통과. 재캡처: `tabs/source-01-folders-after-initial-strip-fix.png`, `tabs-compare/compare-01-folders-after-initial-strip-fix.png`.
- 2026-06-21 manual UAT 재분류: visual usability parity가 아직 부족한 UAT-141 Images, UAT-161 Default, UAT-211 Praise Book을 PASS에서 PARTIAL로 내렸다. 이는 기능 surface 존재와 FrmMain 첫 화면 사용성 동일성을 분리하기 위함이다.
- 2026-06-21 Folders 보정 후 전체 `dotnet test Easislides.Wpf.Tests` 재실행 통과. 결과: 실패 0, 통과 2417, 건너뜀 0. `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`도 통과.
- 2026-06-21 Go Live screenshot evidence 추가. 주요 파일: `wpf-before-preview-go-live.png`, `wpf-after-preview-go-live.png`, `wpf-live-safety-contact-sheet.png`, `wpf-live-before-safety-actions.png`, `wpf-output-black.png`, `wpf-output-clear.png`, `wpf-output-restore.png`.
- 2026-06-21 Go Live UAT 결과: UAT-307, UAT-401 PASS. `Preview Go Live` 호출 후 live banner, Worship List LIVE badge, Output header/body, Output large screen이 선택 Preview 항목으로 갱신됨을 캡처로 확인.
- 2026-06-21 live-safety note: Black/Clear/Restore 버튼 호출 캡처는 남겼지만, 실제 외부 송출 화면의 시각 결과를 직접 관찰하지 않았으므로 UAT-406~UAT-408은 아직 PASS로 처리하지 않는다.
- 2026-06-21 actual manual UAT 수행 완료. `C:\EasiSlides` 실제 데이터로 WPF MainWindow를 실행하고 `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`의 62개 row를 모두 채웠다. 결과: PASS 25, PARTIAL 25, BLOCKED 12, FAIL 0. 상세 evidence: `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/evidence/manual-uat/2026-06-21/manual-uat-summary.md`.
- 2026-06-21 PARTIAL/BLOCKED 전환 완료. `follow-ups.md`에 PARTIAL 25개를 Follow-Up Tasks로, BLOCKED 12개를 Explicit Defers로 매핑했다. FAIL은 0개.
- 2026-06-21 final gate evidence 기록 완료. `final-gate-evidence.md`에 OpenSpec validation, WPF tests, WinForms build, WPF launch, actual manual UAT, follow-up/defer register 결과를 요약했다.

### Phase 8: PARTIAL/BLOCKED Closure

Goal: manual UAT에서 남은 PARTIAL 25개와 BLOCKED 12개를 다시 확인하고, 코드로 닫을 수 있는 항목은 WPF MainWindow 구현과 테스트 증거로 닫는다.

Scope:

- `Easislides.Wpf/MainWindow.xaml`
- WPF converter/tests
- `docs/wpf-migration/inventory/frmmain-manual-uat-checklist.md`
- `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/follow-ups.md`
- `openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/final-gate-evidence.md`

Tasks:

- [x] PARTIAL/BLOCKED 항목을 구현 누락, 수동 UAT 증거 부족, 외부 데이터/하드웨어 blocker로 재분류한다.
- [x] CodeGraph impact/context로 WPF MainWindow 영향 범위를 확인한다.
- [x] 코드로 닫을 수 있는 visual usability gap(Images, Default, Praise Book)을 FrmMain 첫 화면 밀도에 맞게 보정한다.
- [x] 이미 구현된 gesture/keyboard/output/live-message 항목은 테스트와 코드 evidence로 PASS 또는 evidence-closed로 재분류한다.
- [x] 실제 데이터/설정/외부 송출 모니터가 없어서 닫을 수 없는 BLOCKED 항목은 explicit external blocker로 유지하고 완료 불가 사유를 기록한다.
- [x] focused tests, full WPF tests, OpenSpec validation, 필요 시 WinForms build를 실행하고 evidence를 기록한다.

DoD:

- PARTIAL 25개 각각이 `closed`, `code-fixed`, `evidence-closed`, `external-blocked` 중 하나로 재판정된다.
- BLOCKED 12개 각각이 코드 결함인지 외부 의존성인지 재판정된다.
- 코드로 보정한 항목은 자동 테스트가 통과한다.
- 외부 데이터/하드웨어가 필요한 항목은 완료 불가 조건과 다음 UAT 조건이 명시된다.

Tests:

- `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~LegacyImageThumbnailSizeConverterTests|FullyQualifiedName~FoldersTab_|FullyQualifiedName~ImagesSource|FullyQualifiedName~DefaultTab_|FullyQualifiedName~PraiseBook"`
- `dotnet test Easislides.Wpf.Tests -v minimal`
- `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`
- `dotnet build Easislides\Easislides.csproj -nologo -v minimal`

Constraints:

- 실제 `C:\EasiSlides` 사용자 데이터는 생성/삭제/변경하지 않는다.
- PowerPoint/Media/InfoScreen/PraiseBook 실제 데이터가 없는 BLOCKED 항목을 임의 샘플 데이터로 PASS 처리하지 않는다.
- 외부 송출 모니터가 없는 항목은 operator surface 또는 unit test만으로 “완전 PASS”라고 과장하지 않는다.

Evidence:

- 2026-06-21 CodeGraph: `MainWindow` impact 확인. 영향 범위는 WPF shell gesture/visual surface 및 관련 tests 중심.
- 2026-06-21 코드 보정: `LegacyImageThumbnailSizeConverter` 추가, Images source thumbnail을 compact 3열 sizing으로 분리, Default `Apply` 그룹 상단 이동 및 Text 그룹 높이 제한, Praise Book toolbar compact화 및 status/window row 겹침 수정.
- 2026-06-21 screenshot evidence: `evidence/screenshots/2026-06-21/phase8/wpf-phase8-images.png`, `wpf-phase8-default-after-density-fix.png`, `wpf-phase8-praise-book-after-row-fix.png`.
- 2026-06-21 focused tests: `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~LegacyImageThumbnailSizeConverterTests|FullyQualifiedName~MainMenuBarTests" -v minimal` 통과. 결과: 실패 0, 통과 137, 건너뜀 0.
- 2026-06-21 closure report: `evidence/manual-uat/2026-06-21/phase8-partial-blocked-closure.md`. PARTIAL 25개 중 코드 보정 3, 구현/테스트 증거로 닫음 19, 외부 송출 모니터 관찰 필요 3. BLOCKED 12개는 source 폴더/설정/선택 상태 재검증 필요로 유지.
- 2026-06-21 데이터 위치 정정: `C:\EasiSlides` 운영 데이터 루트는 존재한다. PPT/Media 운영 자료는 `C:\EasiSlides\Documents\*.lnk`가 가리키는 `D:\예배자료`에 존재한다. 다만 현재 `UsePowerPointTab=0`, `UseMediaTab=0`이고 `C:\EasiSlides\Powerpoint`, `C:\EasiSlides\Media`, `C:\EasiSlides\InfoScreens` 직접 source 폴더는 비어 있어 source tab UAT는 설정/경로 재확인 후 재수행해야 한다.
- 2026-06-21 full WPF tests: `dotnet test Easislides.Wpf.Tests -v minimal` 통과. 결과: 실패 0, 통과 2421, 건너뜀 0.
- 2026-06-21 OpenSpec validation: `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict` 통과. 결과: Change is valid.
- 2026-06-21 WinForms build: `dotnet build Easislides\Easislides.csproj -nologo -v minimal` 통과. 결과: 오류 0, 경고 13.

### Phase 9: Legacy Registry Runtime Settings Sync

Goal: `HKCU\Software\EasiSlides` 레지스트리에 있는 FrmMain 런타임 설정이 WPF `settings.json` 존재 여부와 무관하게 MainWindow 시작 시 반영되도록 한다.

Scope:

- `Easislides.Wpf/Settings/SettingsBootstrapMigrationService.cs`
- `Easislides.Wpf.Tests/Settings/SettingsBootstrapMigrationServiceTests.cs`
- registry-backed runtime keys: `current_praisebook`, `current_session`, `media_dir`, `UsePowerpointTab`, `UseMediaTab`

Tasks:

- [x] `HKCU\Software\EasiSlides` 실제 값을 확인한다.
- [x] 기존 WPF `settings.json`이 있으면 full migration이 skip되어 `current_praisebook` 등 런타임 선택값이 반영되지 않는 원인을 확인한다.
- [x] 기존 WPF 작업 폴더를 덮어쓰지 않으면서 FrmMain 런타임 선택값과 source tab 표시 설정만 재동기화한다.
- [x] 기존 skip 테스트를 registry runtime refresh 테스트로 갱신한다.
- [x] focused settings tests를 실행한다.

DoD:

- WPF 설정 파일이 이미 있어도 `current_praisebook`, `current_session`, `media_dir`, `UsePowerpointTab`, `UseMediaTab`가 레지스트리에서 갱신된다.
- 기존 WPF `WorkingFolder`는 반복 full migration으로 덮어쓰지 않는다.
- invalid legacy bool 값은 경고로 기록하고 startup을 막지 않는다.

Tests:

- `dotnet test Easislides.Wpf.Tests --filter "FullyQualifiedName~SettingsBootstrapMigrationServiceTests|FullyQualifiedName~RegistryLegacySettingsSourceTests" -v minimal`

Evidence:

- 2026-06-21 registry 확인: `HKCU\Software\EasiSlides\config`에 `root_directory=C:\EasiSlides\`, `current_session=1.주일예배`, `current_praisebook=PraiseBook 1`, `media_dir=C:\EasiSlides\Media\`가 존재한다. `HKCU\Software\EasiSlides\options`에 `UsePowerpointTab=0`, `UseMediaTab=0`, `PowerpointMaxFiles=20`이 존재한다.
- 2026-06-21 원인 확인: `%APPDATA%\EasislidesNext\settings.json`이 이미 존재하면 `SettingsBootstrapMigrationService.MigrateIfNeededAsync()`가 기존에는 `null`을 반환하여 registry-backed runtime settings를 재반영하지 않았다. 실제 WPF 설정 파일의 `Data.CurrentPraiseBookName`은 빈 값이었다.
- 2026-06-21 코드 보정: 기존 설정 파일이 있으면 full migration은 반복하지 않고 `current_praisebook`, `current_session`, `media_dir`, `UsePowerpointTab`, `UseMediaTab`만 재동기화하도록 변경했다.
- 2026-06-21 focused tests 통과. 결과: 실패 0, 통과 8, 건너뜀 0.
- 2026-06-21 full WPF tests 재실행 통과. `dotnet test Easislides.Wpf.Tests -v minimal` 결과: 실패 0, 통과 2423, 건너뜀 0.
- 2026-06-21 OpenSpec validation 재실행 통과. `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict` 결과: Change is valid.

### Phase 10: `C:\EasiSlides` Deployment Smoke And Legacy Data Parity

Goal: WPF 프로그램을 실제 운영 루트 `C:\EasiSlides` 아래에 배포하고, WinForms 프로그램과 동일한 레지스트리 설정 및 운영 데이터를 읽는지 검증한다.

Scope:

- WPF publish output: `C:\EasiSlides\EasislidesNext`
- Existing WinForms output: `C:\EasiSlides\Easislides.exe`
- Legacy registry: `HKCU\Software\EasiSlides`
- WPF settings snapshot: `%APPDATA%\EasislidesNext\settings.json`
- Smoke UAT only; 운영 데이터 파일은 생성/삭제/수정하지 않는다.

Tasks:

- [x] `C:\EasiSlides` 루트의 기존 WinForms 배포본과 운영 데이터 존재를 확인한다.
- [x] WPF Release 산출물을 `C:\EasiSlides\EasislidesNext`에 배포한다.
- [x] 배포된 `EasislidesNext.exe`를 실행해 MainWindow launch, UIAutomation, 설정 동기화를 확인한다.
- [x] Praise Book 탭을 직접 선택해 `current_praisebook` 반영 여부를 확인한다.
- [x] 기존 WinForms `C:\EasiSlides\Easislides.exe`를 실행해 동일 루트 데이터가 표시되는지 비교한다.
- [x] WPF/WinForms 실행 화면을 캡처해 evidence로 저장한다.

DoD:

- WPF 배포본이 `C:\EasiSlides\EasislidesNext\EasislidesNext.exe`로 존재하고 실행된다.
- WPF settings snapshot이 registry-backed 값과 일치한다: `WorkingFolder=C:\EasiSlides\`, `CurrentWorshipListName=1.주일예배`, `CurrentPraiseBookName=PraiseBook 1`, `UsePowerPointTab=false`, `UseMediaTab=false`, `MediaDirectory=C:\EasiSlides\Media\`.
- WPF UIAutomation에서 Folders/Bibles/Images/Default/Worship List/Praise Book/Preview/Output/Go Live surface가 확인된다.
- WPF가 `Legacy worship list loaded: 1.주일예배 (23 .esw items)` 상태를 표시한다.
- WinForms도 `C:\EasiSlides\Easislides.exe`에서 실행되고 같은 찬송가 source list 및 Worship 23 items 화면을 표시한다.

Tests:

- `dotnet publish Easislides.Wpf\Easislides.Wpf.csproj -c Release -o C:\EasiSlides\EasislidesNext -nologo -v minimal`
- 배포 WPF UIAutomation launch smoke
- 배포 WPF Praise Book tab selection smoke
- WinForms UIAutomation launch smoke
- screenshot capture comparison

Evidence:

- 2026-06-21 publish 통과. 출력: `C:\EasiSlides\EasislidesNext`, 파일 42개, `EasislidesNext.exe` 존재.
- 2026-06-21 WPF deployment smoke 통과. `MainWindowTitle=EasiSlides`, `AutomationName=EasiSlides`, `AutomationClassName=Window`, descendants 358개.
- 2026-06-21 WPF registry/settings parity 통과. Registry 값 `root_directory=C:\EasiSlides\`, `current_session=1.주일예배`, `current_praisebook=PraiseBook 1`, `media_dir=C:\EasiSlides\Media\`, `UsePowerpointTab=0`, `UseMediaTab=0`가 WPF settings에 반영됐다.
- 2026-06-21 WPF UIAutomation 확인: Folders, Bibles, Images, Default, Worship List, Praise Book, Preview, Output, Go Live 표시. `Legacy worship list loaded: 1.주일예배 (23 .esw items)` 확인.
- 2026-06-21 Praise Book tab 선택 확인: `찬양집 열림: PraiseBook 1 (0곡)` 확인. 현재 0곡은 WPF 미로딩이 아니라 legacy registry 선택값 `PraiseBook 1`의 현재 데이터 상태다.
- 2026-06-21 WinForms comparison smoke 통과. `C:\EasiSlides\Easislides.exe` 실행, `MainWindowTitle=EasiSlides`, descendants 5502개, Folders source list와 Worship 23 items 화면 확인.
- 2026-06-21 screenshots:
  - `evidence/screenshots/2026-06-21/phase10-deployed/wpf-deployed-c-easislidesnext.png`
  - `evidence/screenshots/2026-06-21/phase10-deployed/winforms-c-easislides.png`
