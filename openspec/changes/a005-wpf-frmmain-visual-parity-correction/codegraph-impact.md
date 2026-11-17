# CodeGraph Impact

## Query

- `codegraph_context`: compare legacy WinForms `FrmMain` layout structure with current WPF `MainWindow`.
- `codegraph_impact`: `MainWindow`, depth 2.

## Result

`MainWindow` impact is limited to the WPF shell surface and code-behind symbols. This correction is scoped to `MainWindow.xaml` and XAML drift tests. It does not modify shared helpers, database access, Office Interop, or output coordinate code.

## Guardrail

No shared module changes are planned, so no additional shared-module impact expansion is required for this pass.
