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
