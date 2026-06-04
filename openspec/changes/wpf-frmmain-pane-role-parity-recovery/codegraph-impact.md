## CodeGraph Impact

Date: 2026-06-04

Task: Restore WPF FrmMain pane role parity for left source tabs, lower-left list tabs, inline Praise Book, Images/Default source entry points, and right output preview roles.

CodeGraph checks:

- `codegraph_status`: healthy index, 511 files, 28150 nodes, 51010 edges.
- `codegraph_context`: pane parity work centers on `MainWindow`, `PraiseBookIndexViewModel`, `ImageLibraryViewModel`, and existing FrmMain panel/event structure.
- `codegraph_explore`: `MainWindow` already has lazy loaders for InfoScreen, PowerPoint, and Media source tabs; `PraiseBookIndexViewModel` already exposes `SavedBooks`, grouped entries, and `OpenBookCommand`; `ImageLibraryViewModel` already loads image thumbnails and applies/clears output backgrounds.
- `codegraph_impact MainWindow`: broad shell impact, so this slice is limited to XAML pane contracts and view-level event wiring.
- `codegraph_impact PraiseBookIndexViewModel`: affects the modal Praise Book window and Praise Book tests; this slice reuses the VM without changing persistence semantics.
- `codegraph_impact ImageLibraryViewModel`: affects image library tests and background application only; this slice reuses the VM without changing loading semantics.

Implementation boundary:

- Touch WPF main-shell XAML/code-behind and focused tests.
- Reuse existing Praise Book, Image, PowerPoint, Media, InfoScreen, Bible, and Worship List services.
- Do not modify SQLite, MariaDB sync, Office interop, DirectShow/media backend, output coordinate calculations, or WinForms `FrmMain`.
