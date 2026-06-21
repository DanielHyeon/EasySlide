## ADDED Requirements

### Requirement: WPF Main Shell Must Mirror FrmMain Operator Regions

WPF main shell은 legacy FrmMain과 같은 운영 영역을 노출해야 한다(SHALL): 좌상단 source browser, 좌하단 Worship List/Praise Book, Preview column, Output column, bottom status area.

#### Scenario: Operator opens the WPF shell

- **GIVEN** WPF main window가 실행되어 있다
- **WHEN** main shell이 표시된다
- **THEN** 좌상단 source browser와 좌하단 Worship List/Praise Book이 동시에 보인다
- **AND** Preview와 Output은 분리된 visible regions로 보인다
- **AND** live safety commands는 modal을 열지 않고도 보인다

### Requirement: Source Browser Must Preserve FrmMain Tab Roles

WPF source browser는 Folders, InfoScr, PowerPoint, Bibles, Images, Media, Default에 대해 FrmMain과 동등한 source tabs를 제공해야 한다(SHALL).

#### Scenario: Operator uses source tabs

- **GIVEN** legacy data가 `C:\EasiSlides` 아래에 존재한다
- **WHEN** 운영자가 각 source tab을 연다
- **THEN** tab은 대응하는 실제 source data를 로드한다
- **AND** tab은 FrmMain과 같은 primary add/preview gestures를 노출한다

### Requirement: Lower Left Lists Must Preserve FrmMain Worship List And Praise Book Roles

WPF lower-left list region은 Worship List와 Praise Book을 modal-only workflow가 아니라 inline tabs로 유지해야 한다(SHALL).

#### Scenario: Operator prepares a service

- **GIVEN** 저장된 worship lists와 praise books가 존재한다
- **WHEN** 운영자가 lower-left region을 사용한다
- **THEN** 저장된 Worship Lists를 로드할 수 있다
- **AND** Worship List items를 선택, reorder, delete, Preview/Output 전송할 수 있다
- **AND** Praise Book entries를 같은 lower-left region에서 browse하고 add할 수 있다

### Requirement: Bible Lookup Must Match FrmMain Main-Shell Workflow

WPF Bible tab은 version/book loading, direct reference lookup, selected passage text, selected passages to Worship List add를 지원해야 한다(SHALL).

#### Scenario: Operator adds a Bible passage

- **GIVEN** working folder에 Bible versions가 구성되어 있다
- **WHEN** 운영자가 reference를 입력하고 verses를 선택한다
- **THEN** 선택한 passage가 Preview에 표시된다
- **AND** button, Enter, context menu, drag/drop flow 중 해당 가능한 방식으로 Worship List에 추가할 수 있다

### Requirement: Preview And Output Must Be Independent Operator Targets

Preview controls는 selected/preview item에 작용해야 하며, Output controls는 live output item에 작용해야 한다(SHALL).

#### Scenario: Operator navigates slides

- **GIVEN** Preview와 Output이 서로 다른 item 또는 서로 다른 slide를 보여주고 있다
- **WHEN** 운영자가 Preview slide 또는 verse controls를 사용한다
- **THEN** 운영자가 명시적으로 live 전송하지 않는 한 Preview만 변경된다
- **WHEN** 운영자가 Output slide 또는 verse controls를 사용한다
- **THEN** live Output만 변경된다

### Requirement: Legacy Gestures Must Be Mapped Before Completion

마이그레이션은 대응 영역을 complete로 간주하기 전에 legacy double-click, Enter, Delete, context menu, drag-and-drop, keyboard shortcut gestures를 mapping해야 한다(SHALL).

#### Scenario: Phase completion is claimed

- **GIVEN** FrmMain 영역 하나가 complete로 표시되어 있다
- **WHEN** mapping table을 review한다
- **THEN** 해당 영역의 모든 primary legacy gesture가 implemented이거나 rationale과 함께 explicitly deferred로 표시되어 있다
- **AND** verification evidence가 존재한다

### Requirement: Manual UAT Must Use Real Legacy Data

Manual verification은 configured legacy working folder를 사용해야 하며, `C:\EasiSlides`가 존재하면 이를 포함해야 한다(SHALL).

#### Scenario: UAT is run

- **GIVEN** 실제 worship, Bible, PowerPoint, image, media, praise-book data가 존재한다
- **WHEN** UAT checklist가 실행된다
- **THEN** scenario별 result가 기록된다
- **AND** missing behavior는 complete로 표시하지 않고 implementation tasks로 전환한다

### Requirement: OpenSpec Change Tree Must Own SDD Execution

active OpenSpec change tree는 contract, evidence, execution phase status, gate evidence의 source of truth여야 한다(SHALL). supporting `docs/wpf-migration/*` files는 reference material과 inventory details를 제공할 수 있지만, OpenSpec contract 또는 phase DoD를 대체해서는 안 된다.

#### Scenario: A phase is planned or resumed

- **GIVEN** implementation work가 시작되거나 재개되려 한다
- **WHEN** phase plan을 review한다
- **THEN** `tasks.md`가 해당 phase의 `Goal`, `Scope`, `Tasks`, `DoD`, `Tests`, `Constraints`를 정의한다
- **AND** `codegraph-impact.md`가 high-impact symbols에 대한 현재 evidence 또는 명시적으로 갱신된 structural evidence를 포함한다
- **AND** supporting docs는 completion authority가 아니라 evidence inputs로 다룬다

#### Scenario: A phase is marked complete

- **GIVEN** phase checkbox가 complete로 변경된다
- **WHEN** completion을 review한다
- **THEN** OpenSpec phase DoD와 verification evidence가 change tree에 존재한다
- **AND** supporting docs에서만 complete를 주장하여 `partial` mapping row를 숨기지 않는다
