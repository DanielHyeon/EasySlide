# EasiSlides SDD Toolchain Runbook

This runbook applies the linked SDD collaboration method to this repository.
The rule is simple: OpenSpec is the contract, codebase-memory-mcp is repo-level memory/search, CodeGraph is structural evidence, Superpowers-style TDD is execution, and gstack/code-review is the gate.

## 1. Branch Discipline

- Use one branch per approved OpenSpec change or approved implementation plan.
- Prefer names that include the change id: `feat/<change-id>` or `fix/<change-id>`.
- Keep Claude and Codex single-writer: do not let both agents edit the same branch/file set at the same time.
- PRs merge only after contract, tests, and review evidence are present.

## 2. OpenSpec

- Official requirement source: `openspec/changes/<change-id>/` and archived `openspec/specs/`.
- Validate before review:

```powershell
openspec validate --all --no-interactive
```

- Completed changes must be synced and archived so `openspec/specs/` stays current.
- gstack/GSD/spec notes are advisory unless copied into OpenSpec.

## 3. Repo Memory And CodeGraph

- After OpenSpec, run a codebase-memory-mcp Repo Memory Query to narrow candidate files/classes/methods and related call chains, routes, or cross-service links.
- Record that result in `openspec/changes/<change-id>/analysis/repo-memory-query.md` for non-trivial changes.
- Repo Memory Query is source recon only. It does not replace CodeGraph, ast-grep, tree-sitter-c-sharp, Function Logic Map, or Branch Test Map, and it must not be the sole basis for production edits.
- Structural questions use CodeGraph before manual file search.
- Git hooks are enabled through:

```powershell
git config core.hooksPath .githooks
```

- The hooks refresh CodeGraph on commit and push. If the git index is locked, the hook now waits briefly and skips safely instead of blocking the git operation.
- Manual refresh:

```powershell
codegraph sync .
```

## 4. Superpowers-Style TDD

- Required for new behavior, bug fixes, and changed contracts.
- Not required for docs, settings, formatting, pure moves, or behavior-preserving refactors.
- Expected loop: failing test, minimal implementation, refactor, verify.
- Ambiguous cases lean toward adding tests.

## 5. gstack / Code Review

- Use gstack as a quality gate, not as a competing spec source.
- Preferred gates for this project: guard/freeze/review/cso/ship-readiness.
- Browser QA skills are web-oriented and are not a substitute for WPF worship-output QA.
- Store review evidence in the PR or the relevant OpenSpec change notes.

## 6. CI

GitHub Actions is the final blocker for PRs. It runs:

- OpenSpec validation
- ast-grep SDD advisory scan across C# files
- ast-grep SDD enforced scan for zero-baseline invariants
- Solution restore
- Debug build
- Debug tests
- Screenshot artifacts on failure

ast-grep rules are discovered from the root `sgconfig.yml`. The advisory pass demotes all rules to warnings and reports findings without failing the build. The enforced pass keeps the zero-baseline error rules active. Broad risk patterns such as numeric conversion, COM lifetime, raw DB usage in legacy/test code, or empty catches remain warning/info evidence, not automatic verdicts.

## 7. Manual Worship QA

For output-sensitive changes, record the checked cases in the PR:

- Songs, Bible, PPT, and worship-list switching
- Preview-only vs live-output impact
- Single monitor, selected multi-monitor, `None`, and manual-coordinate output
- PowerPoint/Word process cleanup for Office interop changes
- SQLite and MariaDB/MySQL sync regression for DB changes
