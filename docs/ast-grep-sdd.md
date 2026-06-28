# ast-grep SDD Gate

Verified on 2026-06-28 with `ast-grep 0.44.0`.

## CLI Behavior

- `ast-grep scan --config sgconfig.yml --globs "*.cs" --format github` reports warning/info diagnostics and exits `0`.
- `ast-grep scan --config sgconfig.yml --globs "*.cs" --format github --filter wpf-shell-no-raw-db-connection --error=wpf-shell-no-raw-db-connection` keeps the zero-baseline hard invariant in the error tier and exits `1` when it matches.
- A missing filtered rule exits non-zero; local verification returned exit `3`.
- `--format github` emits GitHub Actions annotations.
- `--rule ast-grep/rules/<rule>.yml` works for single rule execution.
- `sgconfig.yml` with `ruleDirs` works for central rule discovery.
- Rule-level `files` and `ignores` scope a rule to repository paths.

## Repository Gate

The repository uses a two-stage scan:

1. Advisory: scan configured C# rules and report diagnostics.
2. Enforced: fail only on zero-baseline invariant rules.

Broad risk patterns stay at warning/info because existing production and test code already contains legitimate review targets. The initial error seed is `wpf-shell-no-raw-db-connection`, scoped to WPF presentation paths: `Easislides.Wpf/Shell/**/*.cs`, `Easislides.Wpf/Controls/**/*.cs`, and `Easislides.Wpf/Rendering/**/*.cs`.

CI runs both stages through `tools/run-ast-grep-sdd.ps1`. The local `pre-commit` hook runs both stages on staged C# files through `.githooks/ast-grep-sdd.sh`.
