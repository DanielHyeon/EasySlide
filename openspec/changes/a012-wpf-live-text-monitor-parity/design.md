## Context

WinForms Live button behavior is the product baseline:

1. `btnToLive_Click` calls `PreviewItemToLive`.
2. `PreviewItemToLive` calls `CopyPreviewToOutput`.
3. `CopyPreviewToOutput` copies preview state into output state and reloads the output item.
4. `GoLive(true)` calls `Start_Presentation`.
5. The launch screen loads the live item, calls `FormatText`, `FormatDisplayLyrics`, and `DisplaySlidesFormattedLyrics`, then updates the text monitor buffer.

WPF has separate Preview, Output, session, output-host, and renderer state. A Live click must not stop at command/live-state success; it must carry the same display-ready text body into the `LiveSessionSnapshot` and the `OutputWindowViewModel` scene.

## Goals

- WPF Live click opens/moves the output surface and publishes display-ready text.
- Text, Bible, Notice, and external text file items keep their current fallback rules.
- The right-side WPF output text and the actual output window scene are driven by the same live body payload.

## Non-Goals

- No WinForms production code changes.
- No DB schema changes.
- No PowerPoint COM refactor.
- No new tool binaries or package lock changes in the repo.

## Phase Plan

### Phase 0

Goal: Establish analysis evidence.

Scope: CodeGraph impact, ast-grep risk scan, tree-sitter function structure, function logic map, branch test map.

Tasks: Create analysis artifacts under this change.

DoD: Analysis artifacts exist and identify implementation boundary.

Tests: Not applicable in Phase 0.

Constraints: No production code changes before analysis artifacts are present.

### Phase 1

Goal: Capture the WPF text monitor failure as a test.

Scope: Focused WPF ViewModel/host tests.

Tasks: Add a failing test proving the Live button updates output scene body text.

DoD: The test fails before implementation for the intended reason.

Tests: Focused `dotnet test` filter.

Constraints: Do not change expected values after implementation to make the test pass.

### Phase 2

Goal: Minimal implementation.

Scope: WPF live publish/text body path only.

Tasks: Patch the smallest function boundary identified by analysis.

DoD: New test and existing focused tests pass.

Tests: Focused WPF tests.

Constraints: Do not change WinForms, public contracts, DB, or PowerPoint behavior.

### Phase 3

Goal: Verify and record evidence.

Scope: OpenSpec validation, focused tests, relevant builds.

Tasks: Run validation commands and update verification/failure log.

DoD: Verification artifacts list commands and outcomes.

Tests: `openspec validate`, focused WPF tests, and build as practical.

Constraints: Report any skipped gate explicitly.

