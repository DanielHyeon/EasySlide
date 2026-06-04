## CodeGraph Impact

Generated from CodeGraph context/explore before Phase 0 documentation and before any implementation edits.

### Entry Points

- `FrmMain` in `Easislides/Easislides/FrmMain.Designer.cs`
- `FrmMain` in `Easislides/Easislides/FrmMain.cs`
- `FrmMain` in `Easislides/Easislides/FrmMain.Fields.cs`
- `MainWindow` in `Easislides.Wpf/MainWindow.xaml.cs`

### High Impact Areas

- `MainWindow` has broad shell impact: source tabs, left lower tabs, Preview/Output panes, lazy loaders, drag/drop handlers, and command launchers.
- `MainViewModel` and WPF source/list view models are likely touched by later implementation phases for real data and command routing.
- Legacy `FrmMain.*` files are reference-only for this change unless a WinForms regression is discovered.

### Phase 0 Scope

Phase 0 is documentation/inventory only. No production code is changed.

### Implementation Guard

Before editing shared logic such as data parsers, settings, SQLite helpers, Office/PowerPoint interop, or output monitor logic, run targeted `codegraph_impact` for the specific symbol.
