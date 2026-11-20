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

### Requirement: Play Media on Output Monitor

The WPF worship-list command SHALL play selected or linked media in a borderless player that fills the audience monitor or custom output rectangle. Video SHALL scale proportionally and crop to fill. The player SHALL preserve the operator’s preview, live slide selection and configured PowerPoint aspect ratio.

#### Scenario: A clip is started over a native PowerPoint slide

- **GIVEN** the current slide is visible on the audience monitor
- **WHEN** the operator invokes Play Media on Output Monitor
- **THEN** moving video frames appear in front of that slide, including when the operator has selected another preview item
- **AND** Escape or the end of a non-repeating clip reveals the same live slide
- **AND** subsequent slide shortcuts control the show again

#### Scenario: The slide window is closed or windowed

- **WHEN** the operator invokes Play Media on Output Monitor
- **THEN** the standalone player uses the full configured output area and preserves the slide window's state and placement

#### Scenario: An imported profile contains the legacy default width

- **GIVEN** an imported profile has no explicit placement mode and a stored custom width of 1
- **WHEN** a clip plays on the output monitor with a 4:3 slide setting or a reduced background-video size
- **THEN** the video fills the selected physical monitor, including its four corners
- **AND** closing the player reveals the slide at its original 4:3 size

#### Scenario: A standalone clip precedes the show or an explicit PPT publish

- **WHEN** the operator starts the show or sends a Worship List PPT while the standalone clip is playing or paused
- **THEN** the player releases the output and the selected live item takes over
- **AND** a read-only PowerPoint deck appears in front with the entire slide visible, including on a scaled monitor
- **AND** browsing the Worship List alone preserves standalone playback

#### Scenario: The output is interrupted

- **WHEN** the operator blacks out, hides or closes the live output, or the selected audience display changes during playback
- **THEN** the standalone video is taken off the previous output

#### Scenario: The operator and audience share a monitor

- **WHEN** a native PowerPoint show and the console use the same monitor
- **THEN** the operator can bring the console forward and launch the clip
- **AND** dismissing the focused player returns to the running slide show

### Requirement: 운영자 곡·성경 찾기와 폴더 제목 필터

WPF 운영 콘솔은 WinForms에서 사용하는 검색 대상, 검색 조건, 결과 표시 위치와 곡 목록의 정렬 순서를 유지해야 한다(SHALL).

#### Scenario: 상단 검색창에서 Enter를 누름

- **WHEN** 운영자가 곡 폴더에서 검색어를 입력하고 Enter를 누르면
- **THEN** 활성 폴더의 제목·대체 제목·가사·곡 번호·참조·라이선스·작사자·저작권을 검사해 같은 곡 목록에 결과를 표시한다.
- **AND** 결과 목록은 기존 정렬을 유지하고 첫 곡을 자동으로 미리보기에 보내지 않는다. 일반 폴더로 돌아가면 제목 필터를 다시 쓸 수 있다.
- **WHEN** 운영자가 성경에서 같은 동작을 수행하면
- **THEN** 현재 번역본 전체에서 정확한 구문을 찾고 그 번역본의 부분 단어 설정을 적용한다. 다른 소스에서 Enter를 누르면 해당 소스와 목록을 유지한다.

#### Scenario: 한 폴더의 제목 필터를 사용함

- **WHEN** 운영자가 폴더 필터에 제목 일부를 입력하면
- **THEN** 표시 제목에서 입력 문자를 그대로 찾고, 결과 수와 전체 곡 수를 표시한다. 남아 있는 선택은 유지한다.
- **AND** Enter는 즉시 적용하고 Esc 또는 지우기 버튼은 필터를 해제한다. 아래 방향키가 전역 단축키로 지정되지 않았으면 목록으로 이동한다.

#### Scenario: 상세 찾기를 반복함

- **WHEN** 운영자가 찾기 창에서 곡 또는 성경을 검색하면
- **THEN** 메인 창에서 결과를 확인하고 찾기 창을 열어 둔 채 조건을 바꿔 다시 검색할 수 있다.
- **AND** 곡 검색은 선택한 폴더·검색 필드·조·박자·표기·수정일과 미디어 폴더에서 제목이나 대체 제목이 일치하는 실제 파일을 기준으로 한다. 성경은 번역본·책·모든 단어·한 단어 이상·정확한 구문을 선택할 수 있다.
- **AND** 검색 결과에서 곡을 열 때에는 검색에 사용한 DB와 해당 곡의 원래 폴더를 사용한다.
- **AND** 성경 결과를 보는 동안 번역본이나 본문 표시를 바꾸면 찾은 장절을 유지하고, 책을 선택하면 해당 책의 연속 본문으로 돌아간다.
