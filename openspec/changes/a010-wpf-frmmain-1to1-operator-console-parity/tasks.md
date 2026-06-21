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

- [ ] production edit 전에 Phase 5 target symbols에 대해 `codegraph-impact.md`를 갱신한다.
- [ ] 모든 Phase 5 `partial` mapping row를 evidence와 함께 `implemented`, `partial`, `defer` 중 하나로 분류한다.
- [ ] FrmMain과 동등한 운영에 필요한 누락된 first-screen `Ind_*`, `Def_*` controls를 닫는다.
- [ ] image/background item-first/default-fallback behavior를 검증한다.
- [ ] FrmMain이 구분하는 경우 item movement와 slide movement의 transition settings가 분리되어 있는지 검증한다.
- [ ] Output live-safety controls가 Preview selection을 빼앗지 않고 hidden/live payload를 갱신하는지 검증한다.

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

### Phase 6: Keyboard Shortcut And Focus Parity

Goal: FrmMain live-operation key가 현재 operator focus에 따라 같은 target으로 routing되게 하고, text input field에서는 live shortcut이 절대 발동하지 않게 한다.

Scope:

- `MainWindow.xaml.cs` key routing과 focus surfaces.
- `ShortcutRegistry`, `CommandCatalog`, `VerseJumpKeyMap`, media key routing, source/list Enter/Delete/Ctrl+A paths.
- shortcut 및 gesture inventories의 mapping rows.

Tasks:

- [ ] production edit 전에 Phase 6 target symbols에 대해 `codegraph-impact.md`를 갱신한다.
- [ ] Preview-focused keys가 Preview에만 영향을 주는지 검증한다.
- [ ] Output-focused keys가 Output/live context에만 영향을 주는지 검증한다.
- [ ] global F12/F11/F9/F3/F7/F8/F10/F5/F4/Space/Shift+Space behavior를 검증한다.
- [ ] Shift mappings를 포함한 number/letter verse jumps를 검증한다.
- [ ] QuickFind, Bible lookup, live message, editable setting text fields가 live shortcut routing을 차단하는지 검증한다.
- [ ] `frmmain-shortcut-parity-map.md`와 `frmmain-to-wpf-1to1-map.md`를 evidence와 함께 갱신한다.

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

### Phase 7: Verification Gate And Ship-Readiness Evidence

Goal: scoped operator-console parity 작업 범위에서 WPF MainWindow가 FrmMain처럼 사용할 수 있다고 보고할 수 있는 concrete evidence를 수집한다.

Scope:

- OpenSpec validation.
- WPF tests.
- WinForms build.
- WPF launch.
- `C:\EasiSlides`가 존재하는 경우를 포함해 configured working folder 아래 실제 legacy data 기준 manual UAT.

Tasks:

- [ ] `openspec validate a010-wpf-frmmain-1to1-operator-console-parity --strict`를 실행한다.
- [ ] `dotnet test Easislides.Wpf.Tests`를 실행한다.
- [ ] `dotnet build Easislides\Easislides.csproj -nologo -v minimal`을 실행한다.
- [ ] WPF MainWindow를 실행하고 startup/layout evidence를 기록한다.
- [ ] 실제 legacy data로 manual UAT checklist를 수행한다.
- [ ] 모든 FAIL 또는 unresolved PARTIAL을 follow-up OpenSpec task 또는 explicit defer로 전환한다.
- [ ] phase complete 표시 전에 final gate evidence를 이 파일에 기록한다.

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
