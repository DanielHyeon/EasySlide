## OpenSpec

- Change id:
- Scope source: `openspec/changes/<change-id>/`
- Non-goals unchanged: yes/no

## Evidence

- [ ] CodeGraph impact/context checked when shared helpers, Office interop, DB, output geometry, or FrmMain partial logic changed
- [ ] Behavior changes have failing-test evidence before implementation, or `Test-Needed: no` with reason
- [ ] `openspec validate --all --no-interactive`
- [ ] `dotnet build Easislides\Easislides.csproj -nologo -v minimal`
- [ ] `dotnet test Easislides.Wpf.Tests`

## Worship QA

- [ ] Not applicable
- [ ] Songs/Bible/PPT/worship order flow checked
- [ ] Preview vs live output impact checked
- [ ] Single/multi/None/manual-coordinate monitor case checked
- [ ] PowerPoint/Word process cleanup checked when Office interop changed
- [ ] SQLite/MariaDB sync regression checked when DB behavior changed

## Review Gate

- [ ] gstack/code-review gate run, or documented why not needed
- [ ] Completed OpenSpec changes are ready for `/opsx:sync` and `/opsx:archive`
