## 1. Phase 0 - Analysis

- [x] 1.1 Record CodeGraph impact for WinForms and WPF Live text monitor paths.
- [x] 1.2 Run ast-grep with repo-local C# risk/invariant rules and record findings.
- [x] 1.3 Run tree-sitter-c-sharp function extraction and record the target function structure.
- [x] 1.4 Write function logic map and branch test map before production edits.

## 2. Phase 1 - Failing Test

- [x] 2.1 Add a WPF regression test for Live click text monitor scene body output.
- [x] 2.2 Run the focused test and record the expected failure.

## 3. Phase 2 - Minimal Implementation

- [x] 3.1 Patch the WPF Live text monitor path within the approved boundary.
- [x] 3.2 Keep existing Text/Bible/Notice/external text fallback behavior.

## 4. Phase 3 - Verification

- [x] 4.1 Run focused WPF tests.
- [x] 4.2 Run `openspec validate a012-wpf-live-text-monitor-parity --strict`.
- [x] 4.3 Run relevant build/test gate or document why a gate was skipped.
- [x] 4.4 Update verification and failure-log artifacts.
