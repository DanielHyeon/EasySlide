## 설계

### 원칙

WPF는 먼저 FrmMain에 충실한 운영 콘솔이 되어야 한다. 여기서 "충실함"은 legacy 운영자가 같은 가시 영역과 같은 주요 조작으로 같은 예배 workflow를 수행할 수 있다는 뜻이다. 내부 구현은 WPF view model과 service를 사용할 수 있지만, 사용자가 보는 workflow 동등성이 계약이다.

### 기준 소스

마이그레이션 기준은 legacy control tree와 event graph다:

- `FrmMain.Designer.cs`: layout과 control 이름
- `FrmMain.Events.cs`: resize, live-message, UI event behavior
- `FrmMain.Logic.cs`: data loading과 state update
- `FrmMain.cs`: source selection, drag/drop, Bible, media, command handler
- `FrmMain.Fields.cs`: `DragDropSource`와 live remote action enum

### 운영 콘솔 영역

WPF `MainWindow`는 다음 영역을 반영해야 한다:

| Legacy region | WPF target |
| --- | --- |
| `splitContainerMain.Panel1` | 왼쪽 source/list column |
| `splitContainerMain.Panel2` | 오른쪽 Preview/Output console |
| `splitContainer1.Panel1` | 좌상단 source tabs |
| `splitContainer1.Panel2` | 좌하단 Worship List/Praise Book tabs |
| `tabControlSource` | Folders, InfoScr, PowerPoint, Bibles, Images, Media, Default |
| `tabControlLists` | Worship List, Praise Book |
| `splitContainer2.Panel1` | Preview column |
| `splitContainer2.Panel2` | Output column |
| `splitContainerPreview.Panel1/Panel2` | Preview controls and preview surface |
| `splitContainerOutput.Panel1/Panel2` | Output controls and output surface |

### Mapping 방법

모든 구현 slice는 `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md` 갱신으로 시작한다. 각 row는 다음 항목을 포함해야 한다:

- legacy control 또는 handler
- WPF target control/view model/command
- status
- next phase
- verification evidence

`missing` 또는 `partial`로 표시된 row는 완료로 부를 수 없다.

### 데이터 로딩

WPF shell은 구성된 working folder를 사용해야 하며, 현재 기대값은 `C:\EasiSlides`다. 또한 실제 legacy data를 로드해야 한다:

- `Admin\WorshipLists`의 Worship Lists
- legacy praise-book source의 Praise Books
- legacy Bible data의 Bible versions/books/passages
- InfoScreen, PowerPoint, Images, Media folder contents

### 동작 동등성

다음 조작은 polish가 아니라 parity의 일부다:

- double-click으로 add 또는 preview
- Enter로 현재 active lookup/list action 실행
- Delete로 선택한 Worship List item 제거
- song, Worship List, Praise Book, Bible, Images action용 context menu
- 모든 legacy `DragDropSource` 값에서 Worship List로 drag-and-drop
- Preview와 Output keyboard navigation
- F12, F11, F9, F3, Space, Shift+Space, verse jump keys를 포함한 live shortcuts

### 위험 통제

- shared service와 data parser를 수정하기 전에는 CodeGraph impact review가 필요하다.
- UI slice는 작고 region-specific하게 유지한다.
- 가능한 경우 test는 view structure와 command routing을 커버해야 하며, manual UAT는 live operator scenario를 커버해야 한다.
- legacy FrmMain이 behavior oracle이므로 WinForms build는 green을 유지해야 한다.

### 이 시점 이후 SDD 문서 트리

지금부터 이 작업의 SDD tree는 OpenSpec change directory가 소유한다. 기존 `docs/wpf-migration/*` 파일은 reference와 evidence로 유용하지만, execution order, completion status, Definition of Done의 소유자가 아니다.

```text
openspec/changes/a010-wpf-frmmain-1to1-operator-console-parity/
├─ proposal.md
│  └─ 계약: 이유, 범위, 비목표, 영향 표면, 필수 검증.
├─ design.md
│  └─ 계약 설계: behavior model, source of truth, risk controls, 이 SDD tree.
├─ specs/a010-wpf-frmmain-1to1-operator-console-parity/spec.md
│  └─ 수용 계약: parity를 정의하는 requirements와 scenarios.
├─ codegraph-impact.md
│  └─ 증거: structural impact, high-risk symbols, phase별 impact refresh rules.
└─ tasks.md
   └─ 실행 계획과 gate: phase Goal/Scope/Tasks/DoD/Tests/Constraints.

docs/wpf-migration/
├─ frmmain-1to1-ui-ux-function-mapping-plan.md
│  └─ reference baseline only; 실행은 이 OpenSpec tree가 우선한다.
└─ inventory/
   ├─ frmmain-control-event-inventory.md
   ├─ frmmain-to-wpf-1to1-map.md
   ├─ frmmain-shortcut-parity-map.md
   └─ frmmain-manual-uat-checklist.md
      └─ OpenSpec tasks와 final gate가 소비하는 evidence inputs.
```

규칙:

- 계약 변경은 `proposal.md`, `design.md`, `spec.md`에 기록한다.
- 구조적 증거는 `codegraph-impact.md`에 기록한다. shared WPF shell, settings, output, DB, Office/PPT interop symbol을 수정하기 전에 갱신한다.
- Phase execution은 `tasks.md`만 소유한다. 각 active phase는 `Goal`, `Scope`, `Tasks`, `DoD`, `Tests`, `Constraints`를 포함해야 한다.
- `docs/wpf-migration/inventory/*`는 상세 inventory와 UAT row를 기록할 수 있지만, phase는 `tasks.md`에 gate evidence가 기록되기 전까지 완료가 아니다.
- OpenSpec phase DoD와 verification evidence가 완료되지 않았다면 `partial` mapping row를 완료로 표시하지 않는다.

이 시점 이후 change id convention:

- `openspec/changes/` 아래의 새 change는 `<prefix><NNN>-short-kebab-intent`를 사용한다.
- prefix는 소문자 알파벳 순서로 증가한다: `a001`~`a999`, 그 다음 `b001`, 그 다음 `c001`.
- 첫 글자는 OpenSpec CLI가 요구하는 leading letter다.
- `NNN`은 0으로 채운 생성 순서 번호이며, priority나 risk가 아니다.
- 현재 프로젝트의 기존 active change는 `a001`부터 `a010`까지 번호가 적용되어 있다. 향후 외부 또는 legacy unnumbered change가 발견되면 docs, reports, branches, saved agent context에서 경로를 참조할 수 있으므로 명시 승인 없이 rename하지 않는다.
- archive 시에도 original numbered id를 유지해 historical order가 보이도록 한다.
