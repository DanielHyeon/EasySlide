# EasiSlides SDD Toolchain Runbook

This runbook applies the linked SDD collaboration method to this repository.
The rule is simple: OpenSpec is the contract, CodeGraph is evidence, Superpowers-style TDD is execution, and gstack/code-review is the gate.

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

## 3. CodeGraph

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
- Solution restore
- Debug build
- Debug tests
- Screenshot artifacts on failure

## 7. Manual Worship QA

For output-sensitive changes, record the checked cases in the PR:

- Songs, Bible, PPT, and worship-list switching
- Preview-only vs live-output impact
- Single monitor, selected multi-monitor, `None`, and manual-coordinate output
- PowerPoint/Word process cleanup for Office interop changes
- SQLite and MariaDB/MySQL sync regression for DB changes
